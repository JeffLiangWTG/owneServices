using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Manifest.MessageProcessors;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	sealed class TWManifestOutgoingMessageKeyInfomationTest : TestCaseWithFactory
	{
		public void TestReadFromXML()
		{
			var expectedMessageTypes = new Dictionary<string, string>();
			expectedMessageTypes["WI00533326.N5101H"] = "FHM";
			CombineAssertions(() =>
			{
				foreach (var expectedMessageType in expectedMessageTypes)
				{
					var messageText = TWManifestXmlTestCaseWithFactory.GetExpectedMessageXML($"Enterprise.Customs.TW.Manifest.Business.Testing.BatchProcessor.TestFile.{expectedMessageType.Key}.xml");
					var infomation = new TWManifestOutgoingMessageKeyInfomation(expectedMessageType.Value, messageText);
					AssertNullOrEmpty(expectedMessageType.Key + " can read From XML", infomation.ErrorText);
				}
			});
		}
	}
}
