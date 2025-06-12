using System;
using Enterprise.Customs.FR.TransportSvc.Utilities;
using NUnit.Framework;

namespace Enterprise.Customs.FR.TransportSvc.Test
{
	[TestFixture]
	class UtilitiesTest
	{
		[Test]
		public void TestCleanBadlyEncodedString()
		{
			var dirtyString = "'ABCDÃ©Ã¨Ã Ã¯Ã´Ã§ÃªÃ¹Ã¦ÅÃ«Ã¼Ã¢â¬Â©Â¤Â°1234";
			Assert.That(ToolBox.CleanBadlyEncodedString(dirtyString), Is.EqualTo("'ABCDéèàïôçêùæœëüâ€©¤°1234"));
		}

		[Test]
		public void TestGetFtpPathForUpload()
		{
			Assert.That(FtpHelper.GetFtpPathForUpload(true, true), Is.EqualTo("RECEPTION/UCC6"));
			Assert.That(FtpHelper.GetFtpPathForUpload(true, false), Is.EqualTo("RECEPTION/UCC6_PROD"));
			Assert.That(FtpHelper.GetFtpPathForUpload(false, true), Is.EqualTo("RECEPTION"));
			Assert.That(FtpHelper.GetFtpPathForUpload(false, false), Is.EqualTo("RECEPTION"));
		}
	}
}
