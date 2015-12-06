using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestDAL
{
	[TestClass]
	public class UnitTest
	{
		[TestMethod]
		public void UserTest()
		{
			var user = DAL.User.Get("user");
			Assert.IsNotNull(user);
		}
		[TestMethod]
		public void DataTest()
		{
			Model.Data model = new Model.Data() {
				ProjectName = "Project1",
				WinCom = "WinCom1",
				Money = 15,
				Time = DateTime.Now
			};
			Assert.IsTrue(DAL.Data.Add(model));
		}
		[TestMethod]
		public void UrlTest()
		{
			Assert.IsTrue(DAL.UrlInfo.Add(new Model.UrlInfo()
			{
				Url = @"http://www.ncszfcg.gov.cn/more.cfm?sid=100002011&c_code=791",
				Region = "南昌"
			}));
			Assert.IsTrue(DAL.UrlInfo.Add(new Model.UrlInfo()
			{
				Url = @"http://ggzy.jiangxi.gov.cn/jxzbw/zfcg/017002/017002004/",
				Region = "江西"
			}));
		}
	}
}
