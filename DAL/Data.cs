using Maticsoft.DBUtility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
	public class Data
	{
		private static DbHelperSQLP helper;
		static Data()
		{
			helper = new DbHelperSQLP(Dal_config.ConnStr);
		}
		private static Model.Data DataRowToModel(DataRow dr)
		{
			return new Model.Data()
			{
				Id = Guid.Parse(dr["Id"].ToString()),
				ProjectName = (string)dr["ProjectName"],
				WinCom = (string)dr["WinCom"],
				Money = (int)dr["Money"],
				Time = DateTime.Parse(dr["Time"].ToString())
			};
		}
		public static List<Model.Data> Get(string where = null)
		{
			List<Model.Data> res = new List<Model.Data>();
			string cmd = "select * from [Data] ";
			if (where != null)
			{
				cmd = cmd + " where " + where;
			}
			DataSet ds = helper.Query(cmd);
			if (ds.Tables.Count > 0)
				foreach (DataRow dr in ds.Tables[0].Rows)
				{
					res.Add(DataRowToModel(dr));
				}
			return res;
		}
		public static List<string> GetDistinctName()
		{
			string cmd = "select distinct [ProjectName] from [Data] ";
			List<string> res = new List<string>();
			DataSet ds = helper.Query(cmd);
			if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
			{
				foreach (DataRow dx in ds.Tables[0].Rows)
				{
					res.Add(dx[0].ToString());
				}
			}
			return res;
		}
		public static List<string> GetDistinctCom()
		{
			string cmd = "select distinct [WinCom] from [Data] ";
			List<string> res = new List<string>();
			DataSet ds = helper.Query(cmd);
			if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
			{
				foreach (DataRow dx in ds.Tables[0].Rows)
				{
					res.Add(dx[0].ToString());
				}
			}
			return res;
		}
	}
}
