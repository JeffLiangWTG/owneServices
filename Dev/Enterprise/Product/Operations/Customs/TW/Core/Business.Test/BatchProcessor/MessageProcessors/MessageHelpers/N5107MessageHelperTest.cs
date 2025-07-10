using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.MessageProcessors;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(N5107MessageHelper))]
	sealed class N5107MessageHelperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var testMessage = Factory.NewWithValidTestData<TWMessage>();
			testMessage.EM_MessageText = TestMessageText;
			testMessage.EM_MessageType = "RFM";
			return TWMessageHelper.NewIncomingHelper(testMessage);
		}

		string TestMessageText => TWXmlTestCaseWithFactory.GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.WI00225584.N5107.xml");

		string TestHtml => TWXmlTestCaseWithFactory.GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.WI00225584.N5107.html");

		protected override void SetUp()
		{
			base.SetUp();
			var datahelper = new UniversalReferenceTestDataHelper(Factory);
			datahelper.CreateNewOrGetExistingCusCodeType("TWRF", "Required formalities");
			datahelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, "TWRF", "A04", "請補送簽審或代查文件(代號：       )", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			datahelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, "TWRF", "G01", "待簽審", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			datahelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, "TWRF", "J22", "報單申報ECFA產證編號，海關未收到該產證電子資料", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
		}

		public void TestToHtml()
		{
			var helper = (N5107MessageHelper)GetNewBusinessObject();
			AssertXMLEquals(TestHtml, helper.ToHtml());
		}

		[ExpectNoExceptions]
		public void TestGetRequiredFormalities()
		{
			var helper = new N5107MessageHelperForTest(Factory.New<TWMessage>());
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(helper.ValidationCodeList, NUnit.Framework.Is.TypeOf<CTP_017_ErrorDocumentsOrRequiredFormalities>());
				NUnit.Framework.Assert.That(helper.ValidationCodeList.Count, NUnit.Framework.Is.EqualTo(238));
				NUnit.Framework.Assert.That(helper.GetRequiredFormalities("A02"), NUnit.Framework.Is.EqualTo("A02" + " " + CTP_017_ErrorDocumentsOrRequiredFormalities.Descriptions.A02));
				NUnit.Framework.Assert.That(helper.GetRequiredFormalities("A59"), NUnit.Framework.Is.EqualTo("A59" + " " + CTP_017_ErrorDocumentsOrRequiredFormalities.Descriptions.A59));
			});
		}

		class N5107MessageHelperForTest(TWMessage message) : N5107MessageHelper(message)
		{
			public CodeDescriptionPairList ValidationCodeList => fValidationCodeList;
		}
	}
}
