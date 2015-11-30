using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
	public class User
	{
		public enum PERMISSION
		{
			Unknown,Ordinary,Admin
		}
		public string username { get; set; }
		public string password { get; set; }
		public string name { get; set; }
		public int permission { get; set; }
	}
}
