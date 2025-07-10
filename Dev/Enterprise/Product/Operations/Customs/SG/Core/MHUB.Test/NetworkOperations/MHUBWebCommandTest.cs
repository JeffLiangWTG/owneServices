using System.Collections.Generic;
using System.Net;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.MHUB;
using Moq;

namespace Enterprise.Customs.SG.V4.MHUB.Testing
{
	sealed class MHUBWebCommandTest : TestCaseWithFactory
	{
		public void TestUserAgentOfHttpWebRequest()
		{
			var settings = new Mock<IMHUBSettings>();
			settings.Setup(x => x.JreVersion).Returns("1.8.0_131");
			var command = new MHUBWebCommandTestClass("TEST", settings.Object, new LoggingInformation(), true);
			var webRequest = command.GetHttpWebRequestExtend("https://trial.tradenet.gov.sg/txmhbweb/mhb/EDIServlet");
			AssertEquals("JAVA/1.8.0_131", webRequest.UserAgent);
		}

		sealed class MHUBWebCommandTestClass : MHUBWebCommand
		{
			public MHUBWebCommandTestClass(string ediServlet, IMHUBSettings settingsProvider, LoggingInformation logger, bool verboseLogging) : base(ediServlet, settingsProvider, logger, verboseLogging)
			{
			}

			public HttpWebRequest GetHttpWebRequestExtend(string httpRequestUrl)
			{
				return base.GetHttpWebRequest(httpRequestUrl);
			}

			protected override Dictionary<string, object> InputParameterList => new Dictionary<string, object>();

			protected override MHUBConstants.CommandType CommandToExecute => MHUBConstants.CommandType.None;
		}
	}
}
