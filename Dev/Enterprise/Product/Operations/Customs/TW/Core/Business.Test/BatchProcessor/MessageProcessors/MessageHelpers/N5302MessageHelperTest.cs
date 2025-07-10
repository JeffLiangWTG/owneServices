using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business.MessageProcessors;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(N5302MessageHelper))]
	sealed class N5302MessageHelperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestToHtml()
		{
			string expectedHTML = TWXmlTestCaseWithFactory.GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.WI00490691.N5302_Full.html");
			var helper = (N5302MessageHelper)GetNewBusinessObject();
			AssertXMLEquals(expectedHTML, helper.ToHtml());
		}

		N5302MessageHelper CareateN5302MessageHelper(string inputXML)
		{
			var testMessage = Factory.NewWithValidTestData<TWMessage>();
			testMessage.EM_MessageType = MessageTypeList.Codes.TRN;
			testMessage.EM_MessageText = inputXML;
			return new N5302MessageHelper(testMessage);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			string inputXML = TWXmlTestCaseWithFactory.GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.WI00490691.N5302_Full.xml");
			return CareateN5302MessageHelper(inputXML);
		}
	}
}
