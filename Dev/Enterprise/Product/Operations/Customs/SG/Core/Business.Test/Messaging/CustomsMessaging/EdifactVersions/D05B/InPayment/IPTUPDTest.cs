using CargoWise.EntityFramework.Testing;
using Enterprise.Edifact;
using Enterprise.Edifact.D05B.Messages.CUSDEC;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing
{
	public class IPTUPDTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGenerateIPTUPD()
		{
			CUSDECMessage testMessage = TestMessageBuilder.CusdecMessage;
			AssertNotNull(testMessage);
		}

		public void TestMandatorySegmentsGenerated()
		{
			SGCUSDECTestClass.SGCUSDECTestData testData = new SGCUSDECTestClass.SGCUSDECTestData();
			dataProvider = testData.SetTestData();
			CUSDECMessage testMessage = TestMessageBuilder.CusdecMessage;
			string messageTextResult = TestMessageBuilder.MessageText;
			Assert("Message requires Message Header", messageTextResult.Contains("UNH+"));
			AssertEquals("Message is IPTUPD message", "IPTUPD", testMessage.UNH[0].CommonAccessReference);
			Assert("Message requires valid BGM Segment", messageTextResult.Contains("BGM+914:::GST+<<MSGNO PLACEHOLDER>>+13"));
			Assert("Message requires Cargo Packing Type", messageTextResult.Contains("CST++1+"));
			Assert("Message requires Port of Loading", messageTextResult.Contains("LOC+9+HKHKG'"));
			Assert("Message requires Place of Release", messageTextResult.Contains("LOC+11+CZ'"));
			Assert("Message requires Place of Receipt", messageTextResult.Contains("LOC+88+CZ'"));
			Assert("Message requires Arrival Date", messageTextResult.Contains("DTM+178:20061212:102'"));
			Assert("Message requires Declaration segment", messageTextResult.Contains("GEI+5+:Y'"));
			Assert("Message requires Total Outer Pack", messageTextResult.Contains("MEA+ABK++CTN:18.0000'"));
			Assert("Message requires Total Gross Weight", messageTextResult.Contains("MEA+AAH++KGM:85.7500'"));
			Assert("Message must have Segment Group 10", testMessage.Group10.Count > 0);
			Assert("Message must have Segment Group 11", testMessage.Group10[0].Group11.Count > 0);
		}

		public virtual void TestSegmentsGenerated()
		{
			SGCUSDECTestClass.SGCUSDECTestData testData = new SGCUSDECTestClass.SGCUSDECTestData();
			dataProvider = testData.SetFullTestData(DeclarationTypeCodeList.Codes.DUT);
			dataProvider.AdditionalMessageInfo.UpdateIndicator = "AME";
			dataProvider.AdditionalMessageInfo.ReasonForAmending = "Test IPTUPD message";
			dataProvider.NumberOfRequestsForUpdate = 2;
			CUSDECMessage testMessage = TestMessageBuilder.CusdecMessage;
			string messageTextResult = TestMessageBuilder.MessageText;
			Assert("Port of Loading", messageTextResult.Contains("LOC+9+HKHKG'"));
			Assert("Place of Release", messageTextResult.Contains("LOC+11+CCJ'"));
			Assert("Place of Receipt", messageTextResult.Contains("LOC+88+CW'"));
			Assert("Arrival Date", messageTextResult.Contains("DTM+178:20061213:102'"));
			AssertEquals("Summary Section: CNT segments expected", 2, testMessage.CNT.Count);
			AssertEquals("CNT+5:3'", testMessage.CNT[0].ToString(new UNOASGCharacterSet()));
			AssertEquals("CNT+6:2'", testMessage.CNT[1].ToString(new UNOASGCharacterSet()));
		}

		public virtual void TestFTXSegments()
		{
			SGCUSDECTestClass.SGCUSDECTestData testData = new SGCUSDECTestClass.SGCUSDECTestData();
			dataProvider = testData.SetFullTestData(DeclarationTypeCodeList.Codes.DUT);
			dataProvider.AdditionalMessageInfo.ReasonForAmending = "A123456789012345678901234567890123456789012345678901234567890123456789B123456789012345678901234567890123456789012345678901234567890123456789C123456789012345678901234567890123456789012345678901234567890123456789D123456789012345678901234567890123456789012345678901234567890123456789";
			CUSDECMessage testMessage = TestMessageBuilder.CusdecMessage;
			AssertEquals("FTX segments expected", 2, testMessage.FTX.Count);
			AssertEquals("FTX+AAI+++TEST DECLARATION MESSAGE?: EXCH RATE 0.9565'", testMessage.FTX[0].ToString(new UNOASGCharacterSet()));
			AssertEquals("FTX+ACF+++A123456789012345678901234567890123456789012345678901234567890123456789:B123456789012345678901234567890123456789012345678901234567890123456789:C123456789012345678901234567890123456789012345678901234567890123456789:D123456789012345678901234567890123456789012345678901234567890123456789'", testMessage.FTX[1].ToString(new UNOASGCharacterSet()));
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			dataProvider = new SGCUSDECTestClass();
		}

		SGCUSDECTestClass dataProvider;
		IPTUPD TestMessageBuilder
		{
			get
			{
				if (fTestMessageBuilder == null)
				{
					fTestMessageBuilder = new IPTUPD(dataProvider);
				}

				return fTestMessageBuilder;
			}
		}

		IPTUPD fTestMessageBuilder;
		#endregion
	}
}
