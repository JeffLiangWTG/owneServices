using CargoWise.RefDbRepo.ILReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ILReferenceData.Tests.CmdLine
{
	[TestFixture]
	public class ApplicationConfigTest
	{
		[Test]
		public void TestApplicationSettings()
		{
			ApplicationConfig.ReLoad();
			Assert.AreEqual(1, ApplicationConfig.Instance.RecieverID);
			Assert.AreEqual(1, ApplicationConfig.Instance.SenderID);
			Assert.AreEqual("560038416", ApplicationConfig.Instance.ConsumerId);
			Assert.AreEqual("xttest-eservices.wisegrid.net:61002", ApplicationConfig.Instance.MsgClientConnect);

			Assert.AreEqual("xt-application:{9937e9cc-868d-4de6-812d-e999135c1f08}", ApplicationConfig.Instance.MsgClientURI);
			Assert.AreEqual(30, ApplicationConfig.Instance.MsgClientTimeoutInSeconds);
			Assert.AreEqual("..\\..\\UxmlFiles", ApplicationConfig.Instance.OutputDirectory);
			Assert.AreEqual("..\\..\\UxmlFiles\\Downloads", ApplicationConfig.Instance.DownloadsDirectory);
		}

		[Test]
		public void TestReload()
		{
			ApplicationConfig.Instance.OutputDirectory = "test";
			Assert.That("test", Is.EqualTo(ApplicationConfig.Instance.OutputDirectory));

			ApplicationConfig.ReLoad();
			Assert.That("..\\..\\UxmlFiles", Is.EqualTo(ApplicationConfig.Instance.OutputDirectory));
		}
	}
}
