using Asp.netMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
			if (model == null) return View();
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
		[HttpPost]
		public ActionResult Register(UserViewModel model)
		{
			if (model == null) return View();
			Model.User user = DAL.User.Get(model.Username);
			if (user == null)
			{
				Model.User newuser = new Model.User()
				{
					username = model.Username,
					password = model.Password,
					name = model.Name,
					permission = (int)Model.User.PERMISSION.Ordinary
				};
				if (DAL.User.Add(newuser))
					return RedirectToAction("Index");
				else
				{
					ViewBag.msg = "注册失败";
					return View();
				}
			}
			else
			{
				ViewBag.msg = "用户名重复";
				return View();
			}
		}
		public ActionResult LogOff()
		{
			Session["user"] = null;
			return RedirectToAction("Index");
		}
		public ActionResult UserManage()
		{
			if (Session["user"] == null)
				return RedirectToAction("LogIn", "Home");
			else
				if (((Model.User)Session["user"]).permission < (int)Model.User.PERMISSION.Admin)
					return RedirectToAction("Index", "Home");

			List<Model.User> allUser = DAL.User.Get().OrderBy(x => x.username).ToList();
			return View(allUser);
		}

		public ActionResult UserDelete(string username)
		{
			if (Session["user"] == null)
				return RedirectToAction("LogIn", "Home");
			else
				if (((Model.User)Session["user"]).permission < (int)Model.User.PERMISSION.Admin)
					return RedirectToAction("Index", "Home");

			Model.User user = DAL.User.Get(username);
			UserViewModel model = new UserViewModel();
			model.Username = user.username;
			model.Name = user.name;
			model.Password = user.password;
			if (user.permission == 0) { model.Permission = "Unknown"; }
			else if (user.permission == 1) { model.Permission = "Ordinary"; }
			else { model.Permission = "Admin"; }
			return View(model);
		}
		[HttpPost, ActionName("UserDelete")]
		[ValidateAntiForgeryToken]
		public ActionResult UserDelete(string username, UserViewModel theModel)
		{
			if (Session["user"] == null)
				return RedirectToAction("LogIn", "Home");
			else
				if (((Model.User)Session["user"]).permission < (int)Model.User.PERMISSION.Admin)
					return RedirectToAction("Index", "Home");


			UserViewModel model = new UserViewModel();
			if (DAL.User.Del(username))
				return RedirectToAction("UserManage");
			return View(model);
		}

		public ActionResult UserEdit(string username)
		{
			if (Session["user"] == null)
				return RedirectToAction("LogIn", "Home");
			else
				if (((Model.User)Session["user"]).permission < (int)Model.User.PERMISSION.Admin)
					return RedirectToAction("Index", "Home");


			Model.User user = DAL.User.Get(username);
			UserViewModel model = new UserViewModel();
			List<string> permission = new List<string>();
			permission.Add("Unknown");
			permission.Add("Ordinary");
			permission.Add("Admin");

			model.Username = user.username;
			model.Name = user.name;
			model.Password = user.password;
			if (user.permission == 0)
			{
				model.Permission = "Unknown";
			}
			else if (user.permission == 1)
			{
				model.Permission = "Ordinary";
				permission.RemoveAt(1);
				permission.Insert(0, model.Permission);
			}
			else
			{
				model.Permission = "Admin";
				permission.RemoveAt(2);
				permission.Insert(0, model.Permission);
			}

			ViewBag.permission = new SelectList(permission);
			return View(model);
		}

		[HttpPost]
		public ActionResult UserEdit(string username, string name, string password, string permission)
		{
			if (Session["user"] == null)
				return RedirectToAction("LogIn", "Home");
			else
				if (((Model.User)Session["user"]).permission < (int)Model.User.PERMISSION.Admin)
				return RedirectToAction("Index", "Home");


			Model.User model = new Model.User();
			List<string> allPermission = new List<string>();
			allPermission.Add("Unknown");
			allPermission.Add("Ordinary");
			allPermission.Add("Admin");
			allPermission.Remove(permission);
			allPermission.Insert(0, permission);
			ViewBag.permission = new SelectList(allPermission);
			if (password == null || password == "")
			{
				ViewBag.msg = "密码不能为空";
				return View();
			}
			model.username = username;
			model.name = name;
			model.password = password;
			if (permission == "Unknown") { model.permission = 0; }
			else if (permission == "Ordinary") { model.permission = 1; }
			else if (permission == "Admin") { model.permission = 2; }
			else { return View(); }
			if (DAL.User.Update(model))
				return RedirectToAction("UserManage");
			return View();
		}
	}
}