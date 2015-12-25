using Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

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

	static class Crawler
	{
		private static Dictionary<Guid, CrawlerInfo> dict;
		private static void DO(CrawlerInfo ci)
		{
			for (int i = 0; i < 50; i++)
			{
				if (!ci.Ct.IsCancellationRequested)
				{
					//Todo 修改为爬虫
					Thread.Sleep(500);


					ci.Count++;
				}
				else
					break;
			}
		}
		public static void Start(List<UrlInfo> urls)
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
		public static Dictionary<Guid, CrawlerInfo> GetStatus()
		{
			return dict;
		}
		public static void Stop()
		{
			foreach (var item in dict)
			{
				item.Value.Ct.Cancel();
			}
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
					Crawler.Start(urls);
				}
			}
			else if(optCode==2)	//结束
			{
				Crawler.Stop();
			}
			var res = new JsonResult();
			var dict = Crawler.GetStatus();
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
}