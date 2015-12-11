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
                if(DAL.User.Add(newuser))
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
            List<Model.User> allUser = DAL.User.Get().OrderBy(x => x.username).ToList();
            List<SelectListItem> permission = new List<SelectListItem>();
            permission.Add(new SelectListItem { Text = "Unknown", Value = "0" });
            permission.Add(new SelectListItem { Text = "Ordinary", Value = "1" });
            permission.Add(new SelectListItem { Text = "Admin", Value = "2" });
            ViewData["permission"] = permission;
            return View(allUser);
        }

        public ActionResult UserDelete(string id)
        {           
            Model.User user = DAL.User.Get(id);
            UserViewModel model = new UserViewModel();
            model.Username = user.username;
            model.Name = user.name;
            model.Password = user.password;
            if (user.permission == 0) { model.Permissione = "Unknown"; }
            else if (user.permission == 1) { model.Permissione = "Ordinary"; }
            else { model.Permissione = "Admin"; }
            return View(model);
        }
        [HttpPost, ActionName("UserDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult UserDelete(string username, UserViewModel theModel)
        {
            UserViewModel model = new UserViewModel();
            if (DAL.User.Del(username))
                return RedirectToAction("UserManage");
            return View(model);
        }

        public ActionResult UserEdit(string username)
        {
            Model.User user = DAL.User.Get(username);
            UserViewModel model = new UserViewModel();
            model.Username = user.username;
            model.Name = user.name;
            model.Password = user.password;
            if (user.permission == 0) { model.Permissione = "Unknown"; }
            else if (user.permission == 1) { model.Permissione = "Ordinary"; }
            else { model.Permissione = "Admin"; }
            return View(model);
        }

        [HttpPost]
        public ActionResult UserEdit(string username,UserViewModel theModel)
        {
            Model.User model = new Model.User();
            if (theModel.Password == null || theModel.Permissione == null)
            {
                return View();
            }
            model.username = username;
            model.name = theModel.Name;
            model.password = theModel.Password;
            if (theModel.Permissione == "Unknown") { model.permission = 0; }
            else if (theModel.Permissione == "Ordinary") { model.permission = 1; }
            else if (theModel.Permissione == "Admin") { model.permission = 2; }
            else {
                ViewBag.mag = "用户权限只能为Unknown、Ordinary或Admin";
                return View(); }
            if (DAL.User.Update(model))
                return RedirectToAction("UserManage");
            return View();
        }
    }
}