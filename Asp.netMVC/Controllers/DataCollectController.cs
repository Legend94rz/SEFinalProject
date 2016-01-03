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
            var uri = new Uri(ci.url.Url);
            var siteType = HtmlParse.RecogSite(uri);
		    var c = new NCrawler.Crawler(uri, new HtmlDocumentProcessor(),
		        new MyPipelineStep(ci)) {
		            MaximumCrawlDepth = CrawlArgs.CrawlDepth(siteType),
		            MaximumThreadCount = 5,
                    IncludeFilter = CrawlArgs.IncludeFilter(siteType),
                    ExcludeFilter = CrawlArgs.ExcludeFilter(siteType),
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

    class MyPipelineStep : IPipelineStep{
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
                var siteType = HtmlParse.RecogSite(propertyBag.ResponseUri);
                var records = Parse(htmlDoc, siteType);
                if (records == null)
                    return;
                foreach (var record in records)
                {
                    DAL.Data.Add(record);
                    ++ci.Count;
                }
            }
            catch (NullReferenceException)
            {
            }
        }

        private enum ContentType
        {
            Table,
            Paragraph,
            Unrecognized
        }

        public static List<Model.Data> Parse(HtmlDocument doc, SiteType siteType)
        {
            var result = new List<Model.Data>();
            var title = HtmlParse.ParseTitle(doc, siteType);
            var rootNode = doc.DocumentNode;
            if (title.Length < 8 || !Regex.IsMatch(title, "(公告|公示|中标)") || Regex.IsMatch(title, "(流标|废标)"))
            {
                return null;
            }
            //Console.WriteLine($"\t{MyPipelineStep.count++}");
            var contentType = ContentType.Unrecognized;
            HtmlNode contentNode = null;
            switch (siteType)
            {
                case SiteType.NanChang:
                    contentNode = rootNode.SelectSingleNode("//table[@class='MsoNormalTable']");
                    contentType = ContentType.Table;
                    break;
                case SiteType.JingDeZhen:
                    contentNode = rootNode.SelectSingleNode("//*[@id='MyContent']");
                    contentType = ContentType.Paragraph;
                    break;
                case SiteType.JiangXi:
                    contentType = ContentType.Table;
                    contentNode = rootNode.SelectSingleNode("//*[@id='TDContent']//*[@class='MsoNormalTable']");
                    break;
                case SiteType.PinXiang:
                    contentNode = rootNode.SelectSingleNode("//table[@align='center'//talbe[@align='center']");
                    contentType = ContentType.Table;
                    break;
                case SiteType.GanZhou:
                    contentNode = rootNode.SelectSingleNode("//table[@class='MsoNormalTable']");
                    contentType = ContentType.Table;
                    break;
                case SiteType.FuZhou:
                    contentNode = rootNode.SelectSingleNode("//body");
                    contentType = ContentType.Paragraph;
                    break;
                default:
                    return null;
            }

            var product = HtmlParse.ParseProjectName(title);
            var facilities = new List<string>();
            var money = 0;
            var date = HtmlParse.ParseDate(doc, siteType);

            //按表格型解析
            if (contentType == ContentType.Table)
            {
                if (contentNode == null) return result;
                var table = contentNode;
                var content = HtmlParse.ParseTable(table);
                if (content.ContainsKey("Title") && content["Title"].Count > 0)
                {
                    product = product ?? content["Title"][0];
                }
                foreach (var item in content)
                {
                    if (Regex.IsMatch(item.Key, "商|单位|公司"))
                    {
                        facilities = item.Value;
                    }
                    else if (Regex.IsMatch(item.Key, "金额"))
                    {
                        money = item.Value.Count > 0 ? HtmlParse.ParseMoney(item.Value[0]) : 0;
                    }
                }
            }
            else if (contentType == ContentType.Paragraph)
            {
                //按文本型解析
                var text = contentNode.InnerText;
                facilities = HtmlParse.ParseFacilities(text);
                money = HtmlParse.ParseMoney(text);
            }
            var count = facilities.Count;
            if (product == null)
            {
                //Console.WriteLine($"\n{title}\n");
                return null;
            }
            for (int i = 0; i != facilities.Count; ++i)
            {
                facilities[i] = Regex.Replace(facilities[i], "&nbsp;", "");
                if (facilities[i].Length < 5) {
                    facilities[i] = null;
                }
            }
            for (var i = 0; i < count; i++)
            {
                try
                {
                    result.Add(new Model.Data()
                    {
                        ProjectName = (product ?? "NULL").Trim(),
                        WinCom = (facilities[i] ?? "NULL").Trim(),
                        Money = money,
                        Time = date,
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


    public enum SiteType { Unknown, JiangXi, NanChang, JingDeZhen, PinXiang, GanZhou, ShangRao, FuZhou };
    public class CrawlArgs
    {
        public static int CrawlDepth(SiteType siteType)
        {
            switch (siteType)
            {
                case SiteType.NanChang:
                    return 3;
                default:
                    return 10;
            }
        }
        public static List<RegexFilter> IncludeFilter(SiteType siteType)
        {
            var filter = new List<RegexFilter>();
            switch (siteType)
            {
                case SiteType.JiangXi:
                    filter.Add(new RegexFilter(new Regex(@"zfcg/017002/017002004/")));
                    filter.Add(new RegexFilter(new Regex(@"ZtbInfo")));
                    break;
                case SiteType.FuZhou:
                    filter.Add(new RegexFilter(new Regex(@"index_629")));
                    filter.Add(new RegexFilter(new Regex(@"zbgs")));
                    break;
                case SiteType.ShangRao:
                    filter.Add(new RegexFilter(new Regex(@"/tzgg/zggg/")));
                    filter.Add(new RegexFilter(new Regex(@"db/")));
                    break;
                case SiteType.GanZhou:
                    filter.Add(new RegexFilter(new Regex(@"products.asp")));
                    filter.Add(new RegexFilter(new Regex(@"Productsviwe.asp")));
                    break;
                case SiteType.JingDeZhen:
                    filter.Add(new RegexFilter(new Regex(@"html/2")));
                    filter.Add(new RegexFilter(new Regex(@"html/list_1433")));
                    break;
                case SiteType.NanChang:
                    filter.Add(new RegexFilter(new Regex(@"more.cfm\?sid=100002011")));
                    filter.Add(new RegexFilter(new Regex(@"article.cfm")));
                    break;
            }
            return filter;
        }

        public static List<RegexFilter> ExcludeFilter(SiteType siteType)
        {
            var filter = new List<RegexFilter>();
            switch (siteType)
            {
                default:
                    filter.Add(new RegexFilter(new Regex(@".*")));
                    break;
            }
            return filter;
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
                    // TODO Console.WriteLine($"Repeat Key: {items[0]}");
                    continue;
                }
                string key = items[0];
                items.RemoveAt(0);
                content.Add(key, items);
            }
            return content;
        }

        public static List<string> ParseFacilities(string text)
        {
            var regex = new Regex(@".{2}(市|县|省).{0,10}(公司|部)");
            var content = new List<string>();
            List<string> lines = new List<string>(text.Split('\n'));
            lines.RemoveAll(x => x.Length < 5);
            foreach (var line in lines)
            {
                var match = regex.Match(line);
                if (match.Success)
                {
                    content.Add(match.Value);
                }
            }
            if (content.Count == 0)
            {
                foreach (var line in lines)
                {
                    if (Regex.IsMatch(line, "(公司|部)") && line.Length < 20)
                    {
                        var t = Regex.Replace(line, @"(\s|\t)+", "");
                        content.Add(t);
                    }
                }
            }
            return content;
        }

        public static int ParseMoney(string text)
        {
            var money = 0;
            var regex = new Regex(@"[0-9.,]+千?万?元");
            var lines = text.Split('\n');
            foreach (var line in lines)
            {
                var match = regex.Match(line);
                if (match.Success)
                {
                    float n;
                    var t = Regex.Match(match.Value, @"[0-9.,]+").Value;
                    t = Regex.Replace(t, ",", "");
                    float.TryParse(t, out n);
                    if (match.Value.Contains("万"))
                    {
                        n *= 10000;
                    }
                    else if (match.Value.Contains("千"))
                    {
                        n *= 1000;
                    }
                    if (n > money)
                        money = (int)n;
                }
            }
            return money;
        }

        public static string GetFirstLine(string text, Regex regex)
        {
            var lines = text.Split('\n');
            foreach (var line in lines)
            {
                if (regex.IsMatch(line))
                {
                    return line;
                }
            }
            return null;
        }

        public static SiteType RecogSite(Uri aUri)
        {
            string uri = aUri.AbsoluteUri;
            SiteType type = SiteType.Unknown;
            if (uri.Contains("ncszfcg.gov"))
            {
                type = SiteType.NanChang;
            }
            else if (uri.Contains("caigou.jdzol"))
            {
                type = SiteType.JingDeZhen;
            }
            else if (uri.Contains("bmwz.pingxiang"))
            {
                type = SiteType.PinXiang;
            }
            else if (uri.Contains("gzzfcg.gov"))
            {
                type = SiteType.GanZhou;
            }
            else if (uri.Contains("srzfcg.gov"))
            {
                type = SiteType.ShangRao;
            }
            else if (uri.Contains("fzztb.gov"))
            {
                type = SiteType.FuZhou;
            }
            else if (uri.Contains("ggzy.jiangxi"))
            {
                type = SiteType.JiangXi;
            }
            return type;
        }

        public static string ParseTitle(HtmlDocument htmlDoc, SiteType site)
        {
            var rootNode = htmlDoc.DocumentNode;
            HtmlNode titleNode = null;
            switch (site)
            {
                case SiteType.JiangXi:
                    titleNode = rootNode.SelectSingleNode("//*[@id='lblTitle']");
                    break;
                case SiteType.JingDeZhen:
                    titleNode = rootNode.SelectSingleNode("//h2");
                    break;
                case SiteType.GanZhou:
                    titleNode = rootNode.SelectSingleNode("//div[@align='center']");
                    break;
                case SiteType.FuZhou:
                    titleNode = rootNode.SelectSingleNode("//span[@class='news_title']");
                    break;
                case SiteType.NanChang:
                    titleNode = rootNode.SelectSingleNode("//td[@class='article_bt']");
                    break;
                default:
                    break;
            }
            return titleNode == null ? "" : titleNode.InnerText;
        }

        public static DateTime ParseDate(HtmlDocument htmlDoc, SiteType site)
        {
            var root = htmlDoc.DocumentNode;
            DateTime date = DateTime.MinValue;
            var dateStr = root.SelectSingleNode("//body").InnerText;
            var regs = new List<Regex>();
            regs.Add(new Regex("[0-9]{4}-[0-9]{1,2}-[0-9]{2}"));
            regs.Add(new Regex("[0-9]{4}/[0-9]{1,2}/[0-9]{2}"));
            switch (site)
            {
                case SiteType.JingDeZhen:
                    dateStr = root.SelectSingleNode("//*[@class='info']").InnerText;
                    break;
                case SiteType.GanZhou:
                    //dateStr = root.SelectSingleNode("//td[@class='main']//tbody/tr[2]").InnerText;
                    dateStr = GetFirstLine(root.InnerText, new Regex("时间"));
                    break;
                case SiteType.FuZhou:
                    dateStr = GetFirstLine(root.InnerText, new Regex("来源"));
                    break;
                case SiteType.NanChang:
                    dateStr = root.SelectSingleNode("//td[@class='article_date']").InnerText;
                    break;
                case SiteType.JiangXi:
                    dateStr = root.SelectSingleNode("//*[@id='tdTitle']").InnerText;
                    break;
            }
            if (dateStr != null)
            {
                foreach (var regex in regs)
                {
                    var match = regex.Match(dateStr);
                    if (match.Success)
                    {
                        date = DateTime.Parse(match.Value);
                        break;
                    }
                }
            }
            return date;
        }

        public static string ParseProjectName(string text)
        {
            var match = Regex.Match(text, @"[关于,市]\S{0,20}[系统,设备,工程,项目]");
            var product = Regex.Replace(match.Value, "[关于,市]", "");
            return product == "" ? null : product;
        }
    }
}