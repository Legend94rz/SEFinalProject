using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Asp.netMVC.Controllers
{

	class Crawler
	{
		public static void fetch(string url)
		{
			//Todo :对该url进行采集
			Thread.Sleep(1000);	//模拟采集
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
		public ActionResult Collect(FormCollection fc)
		{
			foreach (var url in fc)
			{
				Crawler.fetch((string)url);
			}
			return RedirectToAction("Index","Home");
		}
	}
}