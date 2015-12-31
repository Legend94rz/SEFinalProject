using Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using HtmlAgilityPack;
using NCrawler;
using NCrawler.Extensions;
using NCrawler.HtmlProcessor;
using NCrawler.Interfaces;
using NCrawler.Services;

namespace Asp.netMVC.Controllers
{
	public class CrawlerInfo
	{
		public int Count;
		public UrlInfo url;
		public Task Collect;
		public CancellationTokenSource Ct;
		public CrawlerInfo(UrlInfo url)
		{
			Count = 0;
			this.url = url;
			Ct = new CancellationTokenSource();
		}
		public void Init(Action task)
		{
			Collect = new Task(task);
		}
	}

	class Crawler
	{
		private Dictionary<Guid, CrawlerInfo> dict;
		private void DO(CrawlerInfo ci) {
		    var c = new NCrawler.Crawler(new Uri(ci.url.Url), new HtmlDocumentProcessor(),
		        new MyPipelineStep(ci)) {
		            MaximumCrawlDepth = 2,
		            MaximumThreadCount = 5,
                    IncludeFilter = new[] {
                        new RegexFilter(
                            new Regex("jxzbw/zfcg/017002"))
                    },
                    ExcludeFilter = new[] {
                        new RegexFilter(
                            new Regex(@"(\.jpg|\.css|\.js|\.gif|\.jpeg|\.png|\.ico)",
                                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)),
                        new RegexFilter(
                            new Regex("jxzbw/zfcg"))
                    },
            };
            c.Crawl();
        }
		public void Start(List<UrlInfo> urls)
		{
			dict = new Dictionary<Guid, CrawlerInfo>();
			foreach (UrlInfo url in urls)
			{
				CrawlerInfo s = new CrawlerInfo(url);
				s.Init(() => DO(s));
				s.Collect.Start();
				dict.Add(url.Id, s);
			}
		}
		public Dictionary<Guid, CrawlerInfo> GetStatus()
		{
			return dict;
		}
		public void Stop()
		{
			foreach (var item in dict)
			{
				item.Value.Ct.Cancel();
			}
		}
	}

    class BiddingRecord
    {
        public string Product = null;
        public string Amount = null;
        public string Facility = null;
        public string Money = null;
        public string Date = null;

        public string Print()
        {
            return $"{NullToken(Product)}\t" +
                   $"{NullToken(Amount)}\t" +
                   $"{NullToken(Facility)}\t" +
                   $"{NullToken(Money)}";
        }

        private string NullToken(string str)
        {
            return str ?? "NULL";
        }
    }

    class MyPipelineStep : IPipelineStep {
        public MyPipelineStep(CrawlerInfo aCrawlerInfo) {
            ci = aCrawlerInfo;
        }
        private CrawlerInfo ci;
        public void Process(NCrawler.Crawler crawler, PropertyBag propertyBag)
        {
            var rsp = propertyBag.GetResponse();
            try
            {
                HtmlDocument htmlDoc = HtmlParse.LoadFromHtml(propertyBag);
                var title = HtmlParse.ParseTitle(htmlDoc);
                if (!(Regex.IsMatch(title, "公告") && title.Length > 8))
                {
                    return;
                }
                ci.Count++;
                var records = Parse(htmlDoc);
                foreach (var record in records) {
                    DAL.Data.Add(record);
                }
            }
            catch (NullReferenceException)
            {
                ;
            }
            //Console.WriteLine(propertyBag.ResponseUri);
        }

        private enum ContentType
        {
            Table,
            Paragraph,
            Unrecognized
        }


        public static List<Model.Data> Parse(HtmlDocument doc)
        {
            var result = new List<Model.Data>();
            var docNode = doc.DocumentNode;
            //从标题提取产品信息
            var product = HtmlParse.ParseProjectName(HtmlParse.ParseTitle(doc));
            var amounts = new List<string>();
            var facilities = new List<string>();
            var moneys = new List<string>();
            var date = "";
            var contentType = ContentType.Unrecognized;

            //尝试按表格型解析
            if (contentType == ContentType.Unrecognized)
            {
                foreach (var table in docNode.SelectNodes("//table"))
                {
                    if (Regex.IsMatch(table.InnerText, "中标|供应商|项目|招标"))
                    {
                        var content = HtmlParse.ParseTable(table);
                        if (content.ContainsKey("Title") && content["Title"].Count > 0)
                        {
                            product = product ?? content["Title"][0];
                        }
                        foreach (var item in content)
                        {
                            if (Regex.IsMatch(item.Key, "数量"))
                            {
                                amounts = item.Value;
                            }
                            else if (Regex.IsMatch(item.Key, "商|单位|公司"))
                            {
                                facilities = item.Value;
                            }
                            else if (Regex.IsMatch(item.Key, "金额"))
                            {
                                moneys = item.Value;
                            }
                        }
                        //按表格解析成功
                        contentType = ContentType.Table;
                    }
                }
            }
            //尝试按文本型解析
            if (contentType == ContentType.Unrecognized)
            {
                var content = docNode.SelectSingleNode("//div[@class='content']");
                content = content ?? docNode.SelectSingleNode("body");
                facilities = HtmlParse.ParseText(content.InnerText, new Regex("公司"));
            }
            var count = Math.Max(amounts.Count, Math.Max(facilities.Count, moneys.Count));
            while (amounts.Count < count)
            {
                amounts.Add(null);
            }
            while (facilities.Count < count)
            {
                facilities.Add(null);
            }
            while (moneys.Count < count)
            {
                moneys.Add(null);
            }
            for (var i = 0; i < count; i++)
            {
                try
                {
                    int money;
                    int.TryParse(moneys[i], out money);
                    DateTime time;
                    DateTime.TryParse(date, out time);
                    if (product == null)
                    {
                        continue;
                    }
                    result.Add(new Model.Data()
                    {
                        ProjectName = product ?? "NULL",
                        WinCom = facilities[i] ?? "NULL",
                        Money = money,
                        Time = time,
                    });
                }
                catch (FormatException)
                {

                }
            }
            return result;
        }
    }
    public class DataCollectController : Controller
	{
		// GET: DataCollect
		public ActionResult Collect()
		{
			List<Model.UrlInfo> all = DAL.UrlInfo.Get();
			return View(all);
		}
		[HttpPost]
		public ActionResult Collect(int optCode,List<string> ids)
		{
			if (Session["user"] == null)
				return RedirectToAction("LogIn", "Home");
			else
				if (((User)Session["user"]).permission < (int)Model.User.PERMISSION.Admin)
					return RedirectToAction("Index", "Home");

			if (optCode == 0)	//开始
			{
				if (ids != null && ids.Count > 0)
				{
					string s = $"'{ids[0]}'";
					for (int i = 1; i < ids.Count; i++)
					{
						s += $",'{ids[i]}'";
					}
					List<UrlInfo> urls = DAL.UrlInfo.Get($"id in ({s})");
					var crawer = new Crawler();
					Session["crawer"] = crawer;
					crawer.Start(urls);
				}
			}
			else if(optCode==2)	//结束
			{
				((Crawler)Session["crawer"]).Stop();
			}
			var res = new JsonResult();
			var dict = ((Crawler)Session["crawer"]).GetStatus();
			List<object> list = new List<object>();
			foreach (var item in dict)
			{
				list.Add(new
				{
					id = item.Key,
					url = item.Value.url.Url,
					count = item.Value.Count,
					status = item.Value.Collect.IsCanceled || item.Value.Collect.IsCompleted || item.Value.Collect.IsFaulted
				});
			}
			res.Data = list;
			return res;
		}
	}
    
    class HtmlParse
    {
        public static HtmlDocument LoadFromHtml(PropertyBag propertyBag)
        {
            try
            {
                HtmlDocument htmlDoc = new HtmlDocument
                {
                    OptionAddDebuggingAttributes = false,
                    OptionAutoCloseOnEnd = true,
                    OptionFixNestedTags = true,
                    OptionReadEncoding = true
                };
                using (Stream reader = propertyBag.GetResponse())
                {
                    Encoding documentEncoding = Encoding.GetEncoding(propertyBag.CharacterSet);
                    if (propertyBag.CharacterSet == "ISO-8859-1")
                    {
                        documentEncoding = htmlDoc.DetectEncoding(reader);
                    }
                    reader.Seek(0, SeekOrigin.Begin);
                    if (!documentEncoding.IsNull())
                    {
                        htmlDoc.Load(reader, documentEncoding, true);
                    }
                    else {
                        htmlDoc.Load(reader, true);
                    }
                }
                return htmlDoc;
            }
            catch (Exception)
            {
                return null;
            }

        }

        /// <summary>
        /// 输入table节点, 按表头解析表格
        /// 如存在标题, 返回值中以Title为键存放
        /// </summary>
        /// <param name="tableRoot"></param>
        /// <returns></returns>
        public static Dictionary<string, List<string>> ParseTable(HtmlNode tableRoot)
        {
            var content = new Dictionary<string, List<string>>();
            content.Add("Title", new List<string>());
            var tbody = tableRoot.SelectSingleNode("./tbody");
            tbody = tbody ?? tableRoot;
            var columnCnt = 0;
            foreach (var tr in tbody.SelectNodes("./tr"))
            {
                var tds = tr.SelectNodes("./td");
                if (tds == null)
                    continue;
                if (tds.Count > columnCnt)
                    columnCnt = tds.Count;
            }
            if (columnCnt <= 0)
            {
                return content;
            }
            var tbodyContent = new List<List<string>>();
            for (var i = 0; i < columnCnt; i++)
            {
                tbodyContent.Add(new List<string>());
            }
            foreach (var tr in tbody.SelectNodes("./tr"))
            {
                var tds = tr.SelectNodes("./td");
                var count = tds.Count;
                if (count < columnCnt)
                {
                    var product = HtmlParse.ParseProjectName(tr.InnerText);
                    if (product != null)
                    {
                        content["Title"].Add(product);
                    }
                    continue;
                }
                for (var i = 0; i < count; i++)
                {
                    tbodyContent[i].Add(Regex.Replace(tds[i].InnerText, "\\s+", " "));
                }
            }
            foreach (var items in tbodyContent)
            {
                if (items.Count == 0)
                {
                    continue;
                }
                if (content.ContainsKey(items[0]))
                {
                    Console.WriteLine($"Repeat Key: {items[0]}");
                    continue;
                }
                string key = items[0];
                items.RemoveAt(0);
                content.Add(key, items);
            }
            return content;
        }

        public static List<string> ParseText(string text, Regex regex)
        {
            var content = new List<string>();
            var lines = text.Split('\n');
            foreach (var line in lines)
            {
                if (regex.IsMatch(line))
                {
                    var info = Regex.Replace(line, "\\s+", " ");
                    content.Add(info);
                }
            }
            return content;
        }

        public static string ParseTitle(HtmlDocument htmlDoc)
        {
            var rootNode = htmlDoc.DocumentNode;
            var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//h2");
            titleNode = titleNode ?? rootNode.SelectSingleNode("//span[@id='lblTitle']");
            titleNode = titleNode ?? rootNode.SelectSingleNode("//td[@class='article_bt']");
            titleNode = titleNode ?? rootNode.SelectSingleNode("//span[@class='news_title']");
            titleNode = titleNode ?? rootNode.SelectSingleNode("//p[@align='center']");
            titleNode = titleNode ?? rootNode.SelectSingleNode("//div[@align='center']");
            return titleNode == null ? "" : titleNode.InnerText;
        }

        public static string ParseProjectName(string text)
        {
            var match = Regex.Match(text, "关于\\S+[设备,工程,项目]");
            var product = Regex.Replace(match.Value, "关于", "");
            if (Regex.IsMatch(product, "、|，|,"))
            {
                //TODO 暂不支持多项记录解析
                product = null;
            }
            return product == "" ? null : product;
        }
    }
}