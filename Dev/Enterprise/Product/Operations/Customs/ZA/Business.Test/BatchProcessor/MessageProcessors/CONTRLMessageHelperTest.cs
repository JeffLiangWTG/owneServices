using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.MessageProcessor.Testing
{
	[TestedType(typeof(CONTRLMessageHelper))]
	sealed class CONTRLMessageHelperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPropertyAccess()
		{
			CombineAssertions("Property Access", () =>
			{
				var helper = GetNewHelperForTest(CONTRL_Message);
				AssertEquals("MessageReferenceNumber", "185", helper.MessageReferenceNumber);
				AssertEquals("ActionCodedList", "7", helper.ActionCodedForMessage.ToString());
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var testMessage = Factory.NewWithValidTestData<ZAMessage>();
			testMessage.EM_MessageText = CONTRL_Message;
			return CONTRLMessageHelper.New(testMessage);
		}

		const string CONTRL_Message = "UNH+1+CONTRL:D:3:UN:CONTRL'UCI+181+00505655TST::AAAAAAAAAAAAAABB:CORAS2+SARSDECT+7'UCM+185+CUSDEC:D:96B:UN:ZZZ01+7'UNT+4+1'";

		CONTRLMessageHelper GetNewHelperForTest(string messageBody)
		{
			var testInterchange = Factory.NewWithValidTestData<ZACInterchange>();
			var testMessage = (ZAMessage)testInterchange.ContainedMessages.AddNew(typeof(ZAMessage));
			testMessage.EM_MessageText = messageBody;
			return CONTRLMessageHelper.New(testMessage);
		}
		// For more unit tests see C : \Dev\Enterprise\Product\Operations\Customs\ASYCUDA\ASYCUDA.Business\EDIFACT\ZaAsycudaToCuscarTests.cs
	}
}
