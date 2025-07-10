using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.MessageProcessors;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX302_AXMessageHelper))]
	sealed class NX302_AXMessageHelperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var testMessage = Factory.NewWithValidTestData<TWMessage>();
			testMessage.EM_MessageText = TestMessageText;
			testMessage.EM_MessageType = MessageTypeList.Codes._32A;
			return TWMessageHelper.NewIncomingHelper(testMessage);
		}

		string TestMessageText => TWXmlTestCaseWithFactory.GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.WI00695067.NX302_AX.xml");
		string TestHtml => TWXmlTestCaseWithFactory.GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.WI00695067.NX302_AX.html");

		[ExpectNoExceptions]
		public void TestToHtml()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			refHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWReceivingUnit, "TWReceivingUnit");
			refHelper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWReceivingUnit, "20", "基隆分局", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var helper = (NX302_AXMessageHelper)GetNewBusinessObject();
			NUnit.Framework.Assert.That(helper.ToHtml(), NUnit.Framework.Is.EqualTo(TestHtml));
		}
	}
}
