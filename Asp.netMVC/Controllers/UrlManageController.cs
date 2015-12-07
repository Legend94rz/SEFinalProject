using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DAL;
using System.Data.OleDb;

namespace Asp.netMVC.Controllers
{
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
            if (fileName.EndsWith(".xls"))
            {
                filePath = "~/App_Data/Excel/temp" + Guid.NewGuid() + ".xls"; 
                fileBase.SaveAs(Server.MapPath(filePath));
            }
            else {
                filePath = "~/App_Data/Txt/temp" + Guid.NewGuid() + ".txt";
                fileBase.SaveAs(Server.MapPath(filePath));
            }
            DataSet dsCal = new DataSet();
            dsCal = GetDataSet(filePath);
            return Content("");
        }
        public ActionResult EditUrl()
		{
			return View();
		}
		#region Excel数据转换为DataSet
        protected DataSet GetDataSet(string filePath)
				{

										string strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Server.MapPath(filePath) + ";Extended Properties='Excel 12.0;HDR=Yes;IMEX=1';";
										OleDbConnection objConn = null;
										objConn = new OleDbConnection(strConn);
										objConn.Open();
										DataSet ds = new DataSet();
										List<string> List = new List<string> { };
										DataTable dtSheetName = objConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
										foreach (DataRow dr in dtSheetName.Rows)
										{
											if (dr["Table_Name"].ToString().Contains("$") && !dr[2].ToString().EndsWith("$"))
											{
												continue;
											}
											string s = dr["Table_Name"].ToString();
											List.Add(s);
										}
										try
										{
											for (int i = 0; i < List.Count; i++)
											{
												ds.Tables.Add(List[i]);
												string SheetName = List[i];
												string strSql = "select * from [" + SheetName + "]";
												OleDbDataAdapter odbcCSVDataAdapter = new OleDbDataAdapter(strSql, objConn);
												DataTable dt = ds.Tables[i];
												odbcCSVDataAdapter.Fill(dt);
											}
											objConn.Close();
											return ds;
										}
										catch (Exception ex)
										{
											return null;
										}
			}
		#endregion
	}
}
