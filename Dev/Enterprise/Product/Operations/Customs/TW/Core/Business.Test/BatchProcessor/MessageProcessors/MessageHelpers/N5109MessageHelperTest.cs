using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.MessageProcessors;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(N5109MessageHelper))]
	sealed class N5109MessageHelperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var testMessage = Factory.NewWithValidTestData<TWMessage>();
			testMessage.EM_MessageText = TestMessageText;
			testMessage.EM_MessageType = "IEM";
			return TWMessageHelper.NewIncomingHelper(testMessage);
		}

		string TestMessageText => TWXmlTestCaseWithFactory.GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.WI00225584.N5109.xml");
		string TestHtml => TWXmlTestCaseWithFactory.GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.WI00225584.N5109.html");
		protected override void SetUp()
		{
			base.SetUp();
			var datahelper = new UniversalReferenceTestDataHelper(Factory);
			datahelper.CreateNewOrGetExistingCusCodeType("ICI", "ICI");
			datahelper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, "ICI", "DAX1", "第一貨櫃中心儀檢站", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			datahelper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, "ICI", "DA01", "臺中關驗貨課查驗一區", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
		}

		[ExpectNoExceptions]
		public void TestToHtml()
		{
			var helper = (N5109MessageHelper)GetNewBusinessObject();
			NUnit.Framework.Assert.That(helper.ToHtml(), NUnit.Framework.Is.EqualTo(TestHtml));
		}
	}
}
