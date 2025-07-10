using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business.MessageProcessors;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(N5108MessageHelper))]
	sealed class N5108MessageHelperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			string testMessageText = TWXmlTestCaseWithFactory.GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.N5108.N5108.xml");
			var testMessage = Factory.NewWithValidTestData<TWMessage>();
			testMessage.EM_MessageText = testMessageText;
			testMessage.EM_MessageType = MessageTypeList.Codes.FHR;
			return new N5108MessageHelper(testMessage);
		}

		[ExpectNoExceptions]
		public void TestTypeCode()
		{
			var helper = (N5108MessageHelper)GetNewBusinessObject();
			NUnit.Framework.Assert.That(helper.TypeCode, NUnit.Framework.Is.EqualTo("5101H").Using(CustomComparers.TypeComparison), "TypeCode should be 5101H");
		}

		[ExpectNoExceptions]
		public void TestStatusCodeWithValidCode()
		{
			var helper = (N5108MessageHelper)GetNewBusinessObject();
			NUnit.Framework.Assert.That(helper.StatusNameCode, NUnit.Framework.Is.EqualTo("RE").Using(CustomComparers.TypeComparison), "StatusNameCode should be RE");
		}

		[ExpectNoExceptions]
		public void TestStatusCodeWithNoCode()
		{
			string messageText = TWXmlTestCaseWithFactory.GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.N5108.N5108_NoStatusNameCode.xml");
			var testMessage = Factory.NewWithValidTestData<TWMessage>();
			testMessage.EM_MessageText = messageText;
			testMessage.EM_MessageType = MessageTypeList.Codes.FHR;
			var helper = new N5108MessageHelper(testMessage);
			NUnit.Framework.Assert.That(helper.StatusNameCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "StatusNameCode should be Empty - should be [null] or [empty]");
		}

		string TestHtml => TWXmlTestCaseWithFactory.GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.WI00533044.N5108.html");
		[ExpectNoExceptions]
		public void TestToHtml()
		{
			new TestTWCreator(Factory).CreateCustomsManifestStatus();
			var helper = (N5108MessageHelper)GetNewBusinessObject();
			NUnit.Framework.Assert.That(helper.ToHtml(), NUnit.Framework.Is.EqualTo(TestHtml));
		}
	}
}
