using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
	public class Data
	{
		public Guid Id { get; set; }
		public string ProjectName { get; set; }
		public string WinCom { get; set; }
		public int Money { get; set; }
		public DateTime Time { get; set; }
	}
}
