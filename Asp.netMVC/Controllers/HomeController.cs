using Asp.netMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Asp.netMVC.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			return View();
		}
		public ActionResult LogIn()
		{
			return View();
		}
		[HttpPost]
		public ActionResult LogIn(UserViewModel model)
		{
			Model.User user = DAL.User.Get(model.Username);
			if (user != null && user.password == model.Password)
			{
				Session["user"] = user;
				return RedirectToAction("Index");
			}
			else
			{
				Session["user"] = null;
				return View();
			}
		}

		public ActionResult Register()
		{
			return View();
		}
		public ActionResult LogOff()
		{
			Session["user"] = null;
			return RedirectToAction("Index");
		}
		public ActionResult UserManage()
		{
			return View();
		}
	}
}