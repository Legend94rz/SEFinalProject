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

        public static List<Model.User> Get()
        {
            List<Model.User> res = new List<Model.User>();
            string cmd = "select * from [User]";
            DataSet ds = helper.Query(cmd);
            if (ds.Tables.Count > 0)
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    res.Add(DataRowToModel(dr));
                }
            return res;
        }

        private static Model.User DataRowToModel(DataRow dataRow)
        {
            return new Model.User()
            {
                username = (string)dataRow["username"],
                password = (string)dataRow["password"],
                name = (string)dataRow["name"],
                permission = (int)dataRow["permission"]
            };
        }
        public static bool Add(Model.User user)
        {
            string cmd = "insert into [User] (username,password,name) VALUES (@username,@password,@name)";
            SqlParameter[] p = new SqlParameter[] {
                new SqlParameter("username",SqlDbType.NVarChar,50),
                new SqlParameter("password",SqlDbType.NVarChar,50),
                new SqlParameter("name",SqlDbType.NVarChar,50),
            };
            p[0].Value = user.username;
            p[1].Value = user.password;
            p[2].Value = user.name;
            return helper.ExecuteSql(cmd, p) > 0;
        }

        public static bool Del(String username)
        {
            string cmd = "delete from [User] where username=@username";
            SqlParameter[] p = new SqlParameter[] {
                new SqlParameter("username",SqlDbType.NVarChar,50)
            };
            p[0].Value = username;
            return helper.ExecuteSql(cmd, p) > 0;
        }
        public static bool Update(Model.User user)
        {
            string cmd = "update [User] set password=@password,name=@name,permission=@permission where username=@username ";
            SqlParameter[] p = new SqlParameter[] {
                new SqlParameter("username",SqlDbType.NVarChar,50),
                new SqlParameter("password",SqlDbType.NVarChar,50),
                new SqlParameter("name",SqlDbType.NVarChar,50),
                new SqlParameter("permission",SqlDbType.Int),
            };
            p[0].Value = user.username;
            p[1].Value = user.password;
            p[2].Value = user.name;
            p[3].Value = user.permission;
            return helper.ExecuteSql(cmd, p) > 0;
        }
    }
}
