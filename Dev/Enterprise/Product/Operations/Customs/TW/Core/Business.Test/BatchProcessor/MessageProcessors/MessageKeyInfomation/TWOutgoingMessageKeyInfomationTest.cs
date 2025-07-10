using System.Collections.Generic;
using Enterprise.Customs.TW.Business.MessageProcessors;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class TWOutgoingMessageKeyInfomationTest : TWXmlTestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestReadFromXML()
		{
			var expectedMessageTypes = new Dictionary<string, string>();
			expectedMessageTypes["WI00333496.N5203"] = "ECD";
			expectedMessageTypes["WI00336218.NX5105"] = "ICD";
			expectedMessageTypes["WI00505316.NX5105"] = "CAA";
			CombineAssertions(() =>
			{
				foreach (var expectedMessageType in expectedMessageTypes)
				{
					var messageText = TWXmlTestCaseWithFactory.GetExpectedMessageXML($"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.{expectedMessageType.Key}.xml");
					var infomation = new TWOutgoingMessageKeyInfomation(expectedMessageType.Value, messageText);
					NUnit.Framework.Assert.That(infomation.ErrorText.ToString(), NUnit.Framework.Is.Null.Or.Empty, "expectedMessageType.Key +  can read From XML - should be [null] or [empty]");
				}
			});
		}
	}
}
