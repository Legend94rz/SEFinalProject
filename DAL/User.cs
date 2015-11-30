using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Maticsoft.DBUtility;
using System.Data.SqlClient;
using System.Data;
using Model;

namespace DAL
{
	public class User
	{
		private static DbHelperSQLP helper;
		static User()
		{
			helper = new DbHelperSQLP(Dal_config.ConnStr);
		}
		public static Model.User Get(string username)
		{
			string cmd = "select * from [User] where username=@username";
			SqlParameter[] p = new SqlParameter[] {
				new SqlParameter("username",SqlDbType.NVarChar,50)
			};
			p[0].Value = username;
			DataSet ds = helper.Query(cmd, p);
			if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
			{
				return DataRowToModel(ds.Tables[0].Rows[0]);
			}
			else
				return null;
		}

		private static Model.User DataRowToModel(DataRow dataRow)
		{
			return new Model.User() {
				username = (string)dataRow["username"],
				password = (string)dataRow["password"],
				name=(string)dataRow["name"]
			};
		}
		public static bool Add(Model.User user)
		{
			string cmd = "insert into [User] (username,password,name) VALUES (@username,@password,@name)";
			SqlParameter[] p = new SqlParameter[] {
				new SqlParameter("username",SqlDbType.NVarChar,50),
				new SqlParameter("password",SqlDbType.NVarChar,50),
				new SqlParameter("password",SqlDbType.NVarChar,50),
			};
			return helper.ExecuteSql(cmd, p)>0;
		}
	}
}
