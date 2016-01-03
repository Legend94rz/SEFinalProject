using NCrawler;
using NCrawler.Interfaces;
using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace WebCrawler
{
    public class CrawlProcessor : IPipelineStep
    {
        public void Process(Crawler crawler, PropertyBag propertyBag)
        {
            var rsp = propertyBag.GetResponse();
            try
            {
                HtmlDocument htmlDoc = HtmlParse.LoadFromHtml(propertyBag);
                var siteType = HtmlParse.RecogSite(propertyBag.ResponseUri);
                var records = Parse(htmlDoc, siteType);
                if (records == null)
                    return;
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
            }
            for (var i = 0; i < count; i++)
            {
                try
                {
                    result.Add(new Model.Data()
                    {
                        ProjectName = product ?? "NULL",
                        WinCom = facilities[i] ?? "NULL",
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
}
