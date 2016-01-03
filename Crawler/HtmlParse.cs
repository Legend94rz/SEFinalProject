using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using NCrawler;
using NCrawler.Extensions;
using System.IO;
using System.Text.RegularExpressions;

namespace WebCrawler
{
    public class HtmlParse
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
            var regex = new Regex(@"[0-9.]+千?万?元");
            var lines = text.Split('\n');
            foreach (var line in lines)
            {
                var match = regex.Match(line);
                if (match.Success)
                {
                    float n;
                    var t = Regex.Match(match.Value, @"[0-9.]+").Value;
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
                case SiteType.PinXiang:
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
            //titleNode = titleNode ?? rootNode.SelectSingleNode("//h2");
            //titleNode = titleNode ?? rootNode.SelectSingleNode("//span[@id='lblTitle']");
            //titleNode = titleNode ?? rootNode.SelectSingleNode("//td[@class='article_bt']");
            //titleNode = titleNode ?? rootNode.SelectSingleNode("//span[@class='news_title']");
            //titleNode = titleNode ?? rootNode.SelectSingleNode("//p[@align='center']");
            //titleNode = titleNode ?? rootNode.SelectSingleNode("//div[@align='center']");
            return titleNode == null ? "" : titleNode.InnerText;
        }

        public static DateTime ParseDate(HtmlDocument htmlDoc, SiteType site)
        {
            var root = htmlDoc.DocumentNode;
            DateTime date = DateTime.MinValue;
            var dateStr = root.SelectSingleNode("//body").InnerText;
            var regs = new List<Regex>();
            regs.Add(new Regex("[0-9]{4}-[0-9]{1,2}-[0-9]{2}"));
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
