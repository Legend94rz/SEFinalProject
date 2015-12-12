using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
namespace Asp.netMVC.Controllers
{
	public class DataAnalysisController : Controller
	{
		// GET: DataAnalysis
		public ActionResult Analysis(string name, string com, string lowTime, string highTime, int? lowMoney, int? highMoney)
		{
			if (Session["user"] == null)
				return RedirectToAction("LogIn", "Home");
			else
				if (((Model.User)Session["user"]).permission < (int)Model.User.PERMISSION.Ordinary)
					return RedirectToAction("Index", "Home");

			string where = null;
			List<string> cond = new List<string>();
			if (name != null && name != "ALL" && name!="")
			{
				cond.Add($"ProjectName='{name}'");
			}
			if (com != null && com != "ALL" && com != "")
				cond.Add($"WinCom='{com}'");
			if (lowTime != null && lowTime != "")
			{
				cond.Add($"Time>='{ lowTime }'");
			}
			if (highTime != null && highTime != "")
			{
				cond.Add($"Time<='{highTime}'");
			}
			if (lowMoney != null)
			{
				cond.Add($"Money>={lowMoney}");
			}
			if (highMoney != null)
			{
				cond.Add($"Money<={highMoney}");
			}
			if (cond.Count > 0)
			{
				where = cond[0];
				for (int i = 1; i < cond.Count; i++)
				{
					where = where + " and " + cond[i];
				}
			}
			List<Model.Data> allData = DAL.Data.Get(where).OrderBy(x=>x.WinCom).ToList();
			var allName = DAL.Data.GetDistinctName();
			allName.Insert(0, "ALL");
			var allCom = DAL.Data.GetDistinctCom();
			allCom.Insert(0, "ALL");
			ViewBag.name = new SelectList(allName);
			ViewBag.com = new SelectList(allCom);
			return View(allData);
		}
	}
}