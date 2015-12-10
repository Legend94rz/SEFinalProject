using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model;
using Maticsoft.DBUtility;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
	public class UrlInfo
	{
        private static DbHelperSQLP helper;
        static UrlInfo()
        {
            helper = new DbHelperSQLP(Dal_config.ConnStr);
        }
        public static List<Model.UrlInfo> Get(string where = null)
		{
			string cmd = "select * from [UrlInfo] ";
			if (where != null && where.Trim() != "")
			{
				cmd += "where " + where;
			}
			List<Model.UrlInfo> res = new List<Model.UrlInfo>();
			DataSet ds = helper.Query(cmd);
			if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
			{
				foreach (DataRow dr in ds.Tables[0].Rows)
				{
					res.Add(DataRowToModel(dr));
				}
			}
			return res;
		}

		private static Model.UrlInfo DataRowToModel(DataRow dr)
		{
			return new Model.UrlInfo() {
				Id = Guid.Parse(dr["Id"].ToString()),
				Region = dr["Region"].ToString(),
				Url = dr["Url"].ToString(),
			};
		}

		public static bool Add(Model.UrlInfo model)
		{
			string cmd = "insert into [UrlInfo] (Id,Region,Url) VALUES (@id,@region,@url) ";
			SqlParameter[] p = new SqlParameter[] {
				new SqlParameter("id",SqlDbType.UniqueIdentifier,16),
				new SqlParameter("region",SqlDbType.NVarChar,50),
				new SqlParameter("url",SqlDbType.NVarChar,-1)
			};
			p[0].Value = model.Id = Guid.NewGuid();
			p[1].Value = model.Region;
			p[2].Value = model.Url;
			return helper.ExecuteSql(cmd, p) > 0;
		}
		public static bool Update(Model.UrlInfo model)
		{
			string cmd = "update [UrlInfo] set Region=@region,Url=@url where Id=@id ";
			SqlParameter[] p = new SqlParameter[] {
				new SqlParameter("id",SqlDbType.UniqueIdentifier,16),
				new SqlParameter("region",SqlDbType.NVarChar,50),
				new SqlParameter("url",SqlDbType.NVarChar,-1),
			};
			p[0].Value = model.Id;
			p[1].Value = model.Region;
			p[2].Value = model.Url;
			return helper.ExecuteSql(cmd, p)>0;
		}
		public static int Delete(string where = null)
		{
			string cmd = "delete from [UrlInfo] ";
			if (where != null && where.Trim() != "")
			{
				cmd = cmd + " where " + where;
			}
			return helper.ExecuteSql(cmd);
		}
	}
}
