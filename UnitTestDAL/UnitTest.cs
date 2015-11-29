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
	}
}
