using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Asp.netMVC.Models;
using System.Data.OleDb;
using System.IO;

namespace Asp.netMVC.Controllers
{
	class BLL
	{
		public static DataTable GetList(string filePath)
		{

			string connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + filePath + ";" + "Extended Properties='Excel 12.0 xml;HDR=Yes;IMEX=1;'";
			string strSql = string.Empty;
			//第一个工作表的名称。考虑到稳定性，就直接写死了。
			string workSheetName = "Sheet1";
			if (workSheetName != "")
			{
				strSql = "select  * ,Region,Url from [" + workSheetName + "$]";
				try
				{
					OleDbConnection conn = new OleDbConnection(connectionString);
					conn.Open();
					OleDbDataAdapter myCommand = null;
					myCommand = new OleDbDataAdapter(strSql, connectionString);
					System.Data.DataTable dt = new System.Data.DataTable();
					myCommand.Fill(dt);
					conn.Close();
					conn.Dispose();
					return dt;
				}
				catch (Exception)
				{
					return null;
				}

			}
			else
			{
				return null;
			}
		}
		public static DataTable ReadTxt(string filePath)
		{
			StreamReader sr = new StreamReader(filePath, System.Text.Encoding.Default);
			DataTable dt = new DataTable();
			dt.Columns.Add("Region", Type.GetType("System.String"));
			dt.Columns.Add("Url", Type.GetType("System.String"));
			string sLine = "";
			while (sLine != null)
			{
				sLine = sr.ReadLine();
				if ((sLine != null) && (!sLine.Equals("")))
				{
					string[] split = sLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
					DataRow dr = dt.NewRow();
					dr["Region"] = split[0];
					dr["Url"] = split[1];
					dt.Rows.Add(dr);
				}
			}
			sr.Close();
			return dt;
		}

	}
	public class UrlManageController : Controller
	{
		// GET: Default
		public ActionResult ImportFile()
		{
			return View();
		}
		[HttpPost]
		public ActionResult ImportFiles()
		{
			HttpPostedFileBase fileBase = Request.Files["upf"];
			string fileName = fileBase.FileName;
			string filePath = null;
			DataTable dataOverall = new DataTable();
			if (fileName.EndsWith(".xls"))
			{
				filePath = "~/App_Data/" + Guid.NewGuid() + ".xls";
				//上传文件
				fileBase.SaveAs(Server.MapPath(filePath));
				//Excel TO DataBase
				try
				{
					dataOverall = BLL.GetList(Server.MapPath(filePath));
					if (dataOverall == null)
					{
						return RedirectToAction("ImportFile");
					}
					else
					{
						DAL.UrlInfo.BatchInsert(dataOverall);
					}
				}
				catch { }
			}
			else if (fileName.EndsWith(".txt"))
			{//Txt TO DataBase
				filePath = "~/App_Data/Txt/temp" + Guid.NewGuid() + ".txt";
				fileBase.SaveAs(Server.MapPath(filePath));
				try
				{
					dataOverall = BLL.ReadTxt(Server.MapPath(filePath));
					if (dataOverall == null)
					{
						return RedirectToAction("ImportFile");
					}
					else
					{
						DAL.UrlInfo.BatchInsert(dataOverall);
					}
				}
				catch { }

			}
			else
			{
				return RedirectToAction("ImportFile");
			}


			int rowNum = 0;
			try
			{
				rowNum = dataOverall.Rows.Count;
			}
			catch
			{
				return RedirectToAction("ImportFile");
			}
			if (rowNum == 0)
			{
				return RedirectToAction("ImportFile");
			}
			return RedirectToAction("ImportFile");
		}
		public ActionResult EditUrl()
		{
			string where = null;
			List<Model.UrlInfo> allRecord = DAL.UrlInfo.Get(where).ToList();
			return View(allRecord);
		}
		[HttpGet]
		public ActionResult EditTheRecord(Guid? id)
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
		public ActionResult EditTheRecord(Guid id, UrlInfo theModel)
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
		public ActionResult AddOneRecord()
		{
			return View();
		}
		[HttpPost]
		public ActionResult AddOneRecord(UrlInfo theModel)
		{
			Model.UrlInfo model = new Model.UrlInfo();
			if (model == null) return View();
			if (theModel.Region == null || theModel.Url == null)
			{
				return View();
			}
			model.Id = theModel.Id;
			model.Region = theModel.Region;
			model.Url = theModel.Url;
			if (DAL.UrlInfo.Add(model))
				return RedirectToAction("EditUrl");
			return View();
		}

		public ActionResult DeleteTheRecord(Guid? id)
		{
			string where = null;
			List<string> cond = new List<string>();
			if (!id.Equals(null))
			{
				cond.Add($"Id='{id}'");
			}
			where = cond[0];
			List<Model.UrlInfo> allRecord = DAL.UrlInfo.Get(where);
			UrlInfo model = new UrlInfo();
			model.Id = allRecord[0].Id;
			model.Region = allRecord[0].Region;
			model.Url = allRecord[0].Url;
			return View(model);
		}
		[HttpPost, ActionName("DeleteTheRecord")]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteTheRecord(String id)
		{
			string where = null;
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
