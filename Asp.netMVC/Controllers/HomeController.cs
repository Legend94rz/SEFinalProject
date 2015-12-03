using Asp.netMVC.Models;
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
			if (model==null) return View();
			Model.User user = DAL.User.Get(model.Username);
			if (user != null && user.password == model.Password)
			{
				Session["user"] = user;
				return RedirectToAction("Index");
			}
			else
			{
				ViewBag.msg = "用户名或密码错误";
				Session["user"] = null;
				return View(model);
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