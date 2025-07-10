using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.MessageProcessors;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX102MessageHelper))]
	sealed class NX102MessageHelperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var testMessage = Factory.NewWithValidTestData<TWMessage>();
			testMessage.EM_MessageText = TestMessageText;
			testMessage.EM_MessageType = MessageTypeList.Codes._102;
			return TWMessageHelper.NewIncomingHelper(testMessage);
		}

		string TestMessageText => TWXmlTestCaseWithFactory.GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.NX102.NX102.xml");
		string TestHtml => TWXmlTestCaseWithFactory.GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.NX102.NX102.html");

		[ExpectNoExceptions]
		public void TestToHtml()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			refHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanCertificateOfOriginType, "Taiwan Certificate Of Origin Type");
			refHelper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanCertificateOfOriginType, "01", "一般原產地證明書", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			refHelper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanCertificateOfOriginType, "14", "臺薩宏自由貿易協定原產地證明書", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var helper = (NX102MessageHelper)GetNewBusinessObject();
			NUnit.Framework.Assert.That(helper.ToHtml(), NUnit.Framework.Is.EqualTo(TestHtml));
		}
	}
}
