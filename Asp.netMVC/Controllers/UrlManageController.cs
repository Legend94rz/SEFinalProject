using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DAL;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Text;

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
            DataTable dataOverall = new DataTable();
            if (fileName.EndsWith(".xls"))
            {
                filePath = "~/App_Data/Excel/temp" + Guid.NewGuid() + ".xls";
                //上传文件
                fileBase.SaveAs(Server.MapPath(filePath));
                //Excel TO DataBase
                try
                {
                    dataOverall = GetList(Server.MapPath(filePath));
                    if (dataOverall == null)
                    {
                        string error = "空表";
                    }
                    else
                    {
                        UrlInfo ul = new UrlInfo();
                        ul.BatchInsert(dataOverall);
                    }
                }
                catch { }
            }
            else if(fileName.EndsWith(".txt"))
            {//Txt TO DataBase
                filePath = "~/App_Data/Txt/temp" + Guid.NewGuid() + ".txt";
                fileBase.SaveAs(Server.MapPath(filePath));
                try
                {
                    dataOverall = ReadTxt(Server.MapPath(filePath));
                    if (dataOverall == null)
                    {
                        string error = "空表";
                    }
                    else
                    {
                        UrlInfo ul = new UrlInfo();
                        ul.BatchInsert(dataOverall);
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
                string k = "Excel表获取失败";
            }
            if (rowNum == 0)
            {
                string kk = "Excel为空";
            }
            return RedirectToAction("ImportFile");
        }
        #region 读取Excel数据
        private DataTable GetList(string filePath)
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
        #endregion

        #region 读取Txt数据
        public DataTable ReadTxt(string filePath)
        {
            StreamReader sr = new StreamReader(filePath, System.Text.Encoding.Default);
            DataTable dt = new DataTable();
            dt.Columns.Add("Region", Type.GetType("System.String"));
            dt.Columns.Add("Url", Type.GetType("System.String"));
            string sLine = "";
            while (sLine != null)
            {
                sLine = sr.ReadLine();
                if((sLine!=null)&&(!sLine.Equals("")))
                    {
                    string[] split = sLine.Split(new char[] { ' '},StringSplitOptions.RemoveEmptyEntries);
                    DataRow dr = dt.NewRow();
                    dr["Region"] = split[0];
                    dr["Url"] = split[1];
                    dt.Rows.Add(dr);
                }
            }
            sr.Close();
            return dt;
        }
        #endregion
        public ActionResult EditUrl()
        {
            return View();
        }
    }
}
    
