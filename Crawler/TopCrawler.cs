using NCrawler;
using NCrawler.HtmlProcessor;
using NCrawler.Services;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WebCrawler
{
    public enum SiteType { Unknown, JiangXi, NanChang, JingDeZhen, PinXiang, GanZhou, ShangRao, FuZhou };
    public class CrawlArgs
    {
        public static int CrawlDepth(SiteType siteType)
        {
            switch (siteType)
            {
                case SiteType.NanChang:
                    return 4;
                default:
                    return 10;
            }
        }
        public static List<RegexFilter> IncludeFilter(SiteType siteType)
        {
            var filter = new List<RegexFilter>();
            switch (siteType)
            {
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

        void Run(string[] args)
        {
            //var uri = new Uri("http://www.fzztb.gov.cn/index_629.htm");
            //var uri = new Uri("http://caigou.jdzol.com/html/list_1433.html");
            //var uri = new Uri("http://www.gzzfcg.gov.cn/products.asp?BigClassID=34&SmallClassID=1");
            var uri = new Uri("http://www.ncszfcg.gov.cn/more.cfm?sid=100002011&c_code=791");

            var siteType = HtmlParse.RecogSite(uri);

            Crawler c = new Crawler(uri, new HtmlDocumentProcessor(), new CrawlProcessor())
            {
                MaximumCrawlDepth = 5,
                MaximumThreadCount = 5,
                IncludeFilter = IncludeFilter(siteType),
                ExcludeFilter = ExcludeFilter(siteType)
            };
            c.Crawl();

            Console.Write("End");
            Console.ReadKey();
        }
    }
}
