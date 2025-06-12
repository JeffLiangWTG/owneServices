using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.eHub.BizTalkAdapters.Transferrer.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.BizTalkAdapters.Tests.Transferrer.Core
{
	[TestClass]
	public class LocationTests
	{
		[TestMethod]
		public void TransferrerCore_Location()
		{
			TestLocation(String.Empty, null, null, null, null, null, null);
			TestLocation("ftp://servername", null, null, "servername", null, null, null);
			TestLocation("ftp://servername:99", null, null, "servername", 99, null, null);
			TestLocation("ftp://servername\\folder\\", null, null, "servername", null, "folder/", null);
			TestLocation("ftp://servername/.*", null, null, "servername", null, null, ".*");
			TestLocation("ftp://servername:99/folder/*", null, null, "servername", 99, "folder", "*");
			TestLocation("sftpex://username@servername:99/folder/file*.*", "username", null, "servername", 99, "folder", "file*.*");
			TestLocation("sftpex://username:password@servername:99/folder/file*.*", "username", "password", "servername", 99, "folder", "file*.*");
		}

		void TestLocation(string uri, string username, string password, string server, int? port, string folder, string filemask)
		{
			var result = new Location(uri);
			Assert.AreEqual(uri.Replace("\\", "/"), result.Uri);
			Assert.AreEqual(username, result.UserName);
			Assert.AreEqual(password, result.Password);
			Assert.AreEqual(server, result.Server);
			Assert.AreEqual(port, result.Port);
			Assert.AreEqual(folder, result.Folder);
			Assert.AreEqual(filemask, result.FileMask);
		}
	}
}
