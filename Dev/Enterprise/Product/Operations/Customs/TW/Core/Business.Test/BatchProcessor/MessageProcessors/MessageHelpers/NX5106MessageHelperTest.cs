using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.MessageProcessors;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX5106MessageHelper))]
	sealed class NX5106MessageHelperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var testMessage = Factory.NewWithValidTestData<TWMessage>();
			testMessage.EM_MessageText = TestMessageText;
			testMessage.EM_MessageType = "ARM";
			return TWMessageHelper.NewIncomingHelper(testMessage);
		}

		string TestMessageText => TWXmlTestCaseWithFactory.GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.WI00225584.NX5106.xml");

		string TestHtml => TWXmlTestCaseWithFactory.GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.WI00225584.NX5106.html");

		protected override void SetUp()
		{
			base.SetUp();
			var datahelper = new UniversalReferenceTestDataHelper(Factory);
			datahelper.CreateNewOrGetExistingCusCodeType("TWRR", "Rejection Reason");
			datahelper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, "TWRR", "B33", "申報之稅則不適用", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
		}

		public void TestToHtml()
		{
			var helper = (NX5106MessageHelper)GetNewBusinessObject();
			AssertXMLEquals(TestHtml, helper.ToHtml());
		}

		[ExpectNoExceptions]
		public void TestGetRejectionReason()
		{
			var helper = new NX5106MessageHelperForTest(Factory.New<TWMessage>());
			NUnit.Framework.Assert.That(helper.ResponseCodeList, NUnit.Framework.Is.TypeOf<CPT_016_RejectionReasons>());
			NUnit.Framework.Assert.That(helper.ResponseCodeList.Count, NUnit.Framework.Is.EqualTo(500));
			NUnit.Framework.Assert.That(helper.GetRejectionReason("A02"), NUnit.Framework.Is.EqualTo("A02" + " " + CPT_016_RejectionReasons.Descriptions.A02));
		}

		class NX5106MessageHelperForTest : NX5106MessageHelper
		{
			public NX5106MessageHelperForTest(TWMessage message) : base(message)
			{
			}

			public CodeDescriptionPairList ResponseCodeList => fResponseCodeList;
		}
	}
}
