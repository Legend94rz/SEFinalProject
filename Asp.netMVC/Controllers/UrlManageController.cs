using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Asp.netMVC.Models;
using System.Data;
using System.Net;

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
            string where = null;
            List<Model.UrlInfo> allRecord = DAL.UrlInfo.Get(where).ToList();
            return View(allRecord);
		}
        [HttpGet]
        public ActionResult EditTheRecord(Guid ? id)
        {
            string where = null;
            List<string> cond = new List<string>();
            if (!id.Equals(null))
            {
                cond.Add($"Id='{id}'");
            }
            where = cond[0];
            List<Model.UrlInfo> allRecord = DAL.UrlInfo.Get(where).ToList();
            Models.UrlInfo model = new Models.UrlInfo();
            model.Id = allRecord[0].Id;
            model.Region = allRecord[0].Region;
            model.Url = allRecord[0].Url;

            return View(model);
        }
        [HttpPost]
        public ActionResult EditTheRecord(Guid id,UrlInfo theModel)
        {
            Model.UrlInfo model = new Model.UrlInfo();
            if (theModel.Region == null || theModel.Url == null)
            {
                return View();
            }
            model.Id = id;
            model.Region = theModel.Region;
            model.Url = theModel.Url;
            if (DAL.UrlInfo.Update(model))
                return RedirectToAction("EditUrl");
            return View();
        }
        public ActionResult AddOneRecord( )
        {

            return View();
        }
        [HttpPost]
        public ActionResult AddOneRecord(UrlInfo theModel )
        {
            Model.UrlInfo model = new Model.UrlInfo();
            if (model==null) return View();
            if (theModel.Region == null|| theModel.Url == null)
            {
                return View();
            }
            model.Id = theModel.Id;
            model.Region = theModel.Region;
            model.Url = theModel.Url;
            if(DAL.UrlInfo.Add(model))
                return RedirectToAction("EditUrl");
            return View();
        }

        public ActionResult DeleteTheRecord(Guid ? id)
        {
            string where = null;
            List<string> cond = new List<string>();
            if (!id.Equals(null))
            {
                cond.Add($"Id='{id}'");
            }
            where = cond[0];
            List<Model.UrlInfo> allRecord = DAL.UrlInfo.Get(where).ToList();
            Models.UrlInfo model = new Models.UrlInfo();
            model.Id = allRecord[0].Id;
            model.Region = allRecord[0].Region;
            model.Url = allRecord[0].Url;
            return View(model);
        }
        [HttpPost, ActionName("DeleteTheRecord")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteTheRecord(String id)
        {
            string where =null;
            List<string> cond = new List<string>();
            if (id != null)
            {
                cond.Add($"Id='{id}'");
            }
            where = cond[0];
            Model.UrlInfo model = new Model.UrlInfo();
            if (DAL.UrlInfo.Delete(where) != 0)
                return RedirectToAction("EditUrl");
            return View(model);
        }
    }

}