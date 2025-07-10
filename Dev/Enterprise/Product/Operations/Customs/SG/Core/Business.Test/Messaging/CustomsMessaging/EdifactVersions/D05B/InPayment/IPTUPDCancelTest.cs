using Enterprise.Edifact.D05B.Messages.CUSDEC;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing
{
	public class IPTUPDCancelTest : IPTUPDTest
	{
		public void TestMessageSubType()
		{
			AssertEquals(CUSDECEDIMessage.Cancellation, TestMessageBuilder.MessageSubType);
		}

		[ExpectNoExceptions]
		public void TestGenerateIPTUPDCancel()
		{
			CUSDECMessage testMessage = TestMessageBuilder.CusdecMessage;
			AssertNotNull(testMessage);
		}

		public override void TestSegmentsGenerated()
		{
			SGCUSDECTestClass.SGCUSDECTestData testData = new SGCUSDECTestClass.SGCUSDECTestData();
			dataProvider = testData.SetFullTestData(DeclarationTypeCodeList.Codes.DUT);
			dataProvider.AdditionalMessageInfo.UpdateIndicator = "CNL";
			dataProvider.AdditionalMessageInfo.CancellationCode = ReasonForCancellationCodeList.Codes.C09;
			dataProvider.PermitNoToUpdateOrCancel = "IG8B619828I";
			dataProvider.ReplacementPermitNumber = "REPLACE";
			CUSDECMessage testMessage = TestMessageBuilder.CusdecMessage;
			string messageTextResult = TestMessageBuilder.MessageText;
			Assert("Message requires Message Header", messageTextResult.Contains("UNH+"));
			AssertEquals("Message is IPTUPD message", "IPTUPD", testMessage.UNH[0].CommonAccessReference);
			Assert("Message BGM segment", messageTextResult.Contains("BGM+915+"));
			AssertEquals("Cancellation should not contain any LOC segments", 0, testMessage.LOC.Count);
			AssertEquals("Cancellation should not contain any DTM segments", 0, testMessage.DTM.Count);
			Assert("Message requires Declaration segment", messageTextResult.Contains("GEI+5+:Y'"));
			AssertEquals("Cancellation should not contain any MEA segments", 0, testMessage.MEA.Count);
			AssertEquals("Cancellation should not contain any Container segments", 0, testMessage.EQD.Count);
			Assert("Message requires Cancellation Reason code", messageTextResult.Contains("FTX+AES+++C09'"));
			Assert("Cancellation should contain at least 3 SG1 segments", testMessage.Group1.Count > 2);
			Assert("Message requires Permit No to be Cancelled", messageTextResult.Contains("RFF+ABT:IG8B619828I'"));
			Assert("Message requires Replacement Permit No if entered", messageTextResult.Contains("RFF+AAE:REPLACE'"));
			AssertEquals("Cancellation should not have a SG4", 0, testMessage.Group4.Count);
			AssertEquals("Cancellation should not have a SG10", 0, testMessage.Group10.Count);
			AssertEquals("Cancellation should not have any Line segments (SG30)", 0, testMessage.Group30.Count);
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			dataProvider = new SGCUSDECTestClass();
		}

		SGCUSDECTestClass dataProvider;
		IPTUPDCancel TestMessageBuilder
		{
			get
			{
				if (fTestMessageBuilder == null)
				{
					fTestMessageBuilder = new IPTUPDCancel(dataProvider);
				}

				return fTestMessageBuilder;
			}
		}

		IPTUPDCancel fTestMessageBuilder;
		#endregion
	}
}
