using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Products.Shared.GLSHK.Maps;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
	//ToDo. To use new tools/test framework that works on .Net 4.5.
	[TestClass]
	public class GLSHKFTPDetails2FtpExPollingRequestTest
	{
		const string filePath = "GLSHKFTPDetails2FtpExPollingRequest.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestGLSHKFTPDetails2FtpExPollingRequest()
		{
			//var input = filePath + "GLSHKFTPDetails.xml";
			//var expectedOutput = filePath + "FtpExPollingRequest.xml";

			//var mapTester = new MapTester(Assembly.GetExecutingAssembly());
			//mapTester.Execute<GLSHKFTPDetails2FtpExPollingRequest>(input, expectedOutput);
		}
	}
}
