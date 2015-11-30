using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Asp.netMVC.Controllers
{
	public class UrlManageController : Controller
	{
		// GET: Default
		public ActionResult ImportFile()
		{
			return View();
		}
		public ActionResult EditUrl()
		{
			return View();
		}
	}
}