using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.Business.CustomsMessaging.D09B.Testing
{
	public class Iptupd09bRefundTest : IPTUPDTest
	{
		[ExpectNoExceptions]
		public void TestGenerateIPTUPDRefund()
		{
			var testMessage = TestMessageBuilder.CusdecMessage;
			AssertNotNull(testMessage);
		}

		public void TestMessageSubType()
		{
			AssertEquals(CUSDECEDIMessage.Refund, TestMessageBuilder.MessageSubType);
		}

		public void TestFullRefund()
		{
			var testData = new SGCUSDECTestClass.SGCUSDECTestData();
			dataProvider = testData.SetTestData();
			dataProvider.AdditionalMessageInfo.UpdateIndicator = UpdateIndicatorCodeList.Codes.FRF;
			dataProvider.AdditionalMessageInfo.RefundCode = ReasonForRefundCodeList.Codes.RF01;
			dataProvider.DeclarantId = "V13T002";
			dataProvider.PermitNoToUpdateOrCancel = "PERMIT";
			dataProvider.ReplacementPermitNumber = "REPLPERMIT";
			dataProvider.AdditionalMessageInfo.ExciseRefundAmount = 1.1m;
			dataProvider.AdditionalMessageInfo.DutyRefundAmount = 2.2m;
			dataProvider.AdditionalMessageInfo.GSTRefundAmount = 3.3m;
			var message = TestMessageBuilder.CusdecMessage;
			string actual = TestMessageBuilder.MessageText;
			Assert("Message Header", actual.Contains("UNH+"));
			AssertEquals("IPTUPD message", "IPTUPD", message.UNH[0].CommonAccessReference);
			Assert("valid BGM Segment", actual.Contains("BGM+916+<<MSGNO PLACEHOLDER>>+13"));
			Assert("Update indicator", actual.Contains("CST+++FRF"));
			Assert("Declaration segment", actual.Contains("GEI+5+:Y'"));
			Assert("Refund Reason", actual.Contains("FTX+ACD+++RF01'"));
			Assert("Message Sender", actual.Contains("RFF+MS:V13T.V13T002'"));
			Assert("Permit", actual.Contains("RFF+ABT:PERMIT'"));
			Assert("Previous Permit", actual.Contains("RFF+AAE:REPLPERMIT'"));
			Assert("Declarant", actual.Contains("NAD+DT'CTA+IC'COM+:TE'"));
			Assert("UNS", actual.Contains("UNS+D'UNS+S'"));
			Assert("Excise", actual.Contains("TAX+5+EXC'MOA+530:1.10'"));
			Assert("GST", actual.Contains("TAX+7+GST'MOA+530:3.30'"));
			Assert("Duty", actual.Contains("TAX+5+CUD'MOA+530:2.20'"));
			string expected = "UNH+WTG+CUSDEC:D:09B:UN:041+IPTUPD'BGM+916+<<MSGNO PLACEHOLDER>>+13'CST+++FRF'GEI+5+:Y'FTX+ACD+++RF01'RFF+MS:V13T.V13T002'RFF+ABT:PERMIT'RFF+AAE:REPLPERMIT'NAD+DT'CTA+IC'COM+:TE'UNS+D'UNS+S'CNT+6:0'TAX+5+CUD'MOA+530:2.20'TAX+5+EXC'MOA+530:1.10'TAX+7+GST'MOA+530:3.30'UNT+21+WTG'";
			AssertEquals("Full Message (ie no additional segments)", expected, actual);
		}

		public void TestFullRefund_NoDuty()
		{
			var testData = new SGCUSDECTestClass.SGCUSDECTestData();
			dataProvider = testData.SetTestData();
			dataProvider.AdditionalMessageInfo.UpdateIndicator = UpdateIndicatorCodeList.Codes.FRF;
			dataProvider.AdditionalMessageInfo.ExciseRefundAmount = 1.1m;
			dataProvider.AdditionalMessageInfo.GSTRefundAmount = 3.3m;
			var message = TestMessageBuilder.CusdecMessage;
			string actual = TestMessageBuilder.MessageText;
			Assert("Update indicator", actual.Contains("CST+++FRF"));
			Assert("Excise", actual.Contains("TAX+5+EXC'MOA+530:1.10'"));
			Assert("GST", actual.Contains("TAX+7+GST'MOA+530:3.30'"));
			Assert("Duty", !actual.Contains("TAX+5+CUD'MOA+530:2.20'"));
		}

		public void TestRefundReasonText()
		{
			var testData = new SGCUSDECTestClass.SGCUSDECTestData();
			dataProvider = testData.SetTestData();
			dataProvider.AdditionalMessageInfo.UpdateIndicator = UpdateIndicatorCodeList.Codes.FRF;
			dataProvider.AdditionalMessageInfo.RefundCode = ReasonForRefundCodeList.Codes.RF35;
			dataProvider.AdditionalMessageInfo.ReasonForRefund = "reason for refund is 4 lines of 70, not 4 lines of 512 - pls check all cases of reason for amendment - to ensure that this is also 4 lines of 70 not 512";
			dataProvider.AdditionalMessageInfo.GSTRefundAmount = 3.3m;
			var message = TestMessageBuilder.CusdecMessage;
			string testMsg1 = TestMessageBuilder.MessageText;
			Assert("Refund Reason Code", testMsg1.Contains("FTX+ACD+++RF35'"));
			Assert("Reason for Refund", testMsg1.Contains("FTX+ABO+++REASON FOR REFUND IS 4 LINES OF 70, NOT 4 LINES OF 512 - PLS CHECK ALL:CASES OF REASON FOR AMENDMENT - TO ENSURE THAT THIS IS ALSO 4 LINES OF:70 NOT 512'"));
			testData = new SGCUSDECTestClass.SGCUSDECTestData();
			dataProvider = testData.SetTestData();
			dataProvider.AdditionalMessageInfo.UpdateIndicator = UpdateIndicatorCodeList.Codes.FRF;
			dataProvider.AdditionalMessageInfo.RefundCode = ReasonForRefundCodeList.Codes.RF35;
			dataProvider.AdditionalMessageInfo.ReasonForRefund = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDEEEEEEEEE";
			dataProvider.AdditionalMessageInfo.GSTRefundAmount = 3.3m;
			ResetMessageBuilder();
			message = TestMessageBuilder.CusdecMessage;
			string testMsg2 = TestMessageBuilder.MessageText;
			Assert("Refund Reason Code", testMsg2.Contains("FTX+ACD+++RF35'"));
			Assert("Reason for Refund", testMsg2.Contains("FTX+ABO+++AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABBBBBBB:BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBCCCCCCCCCCCCCCCCCCCC:CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCDDDDDDDDDDDDDDDDDDDDDDDDDD:DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDEEEEEEEEE'"));
		}

		public void TestPartialGeneral()
		{
			var testData = new SGCUSDECTestClass.SGCUSDECTestData();
			dataProvider = testData.SetTestData();
			dataProvider.AdditionalMessageInfo.UpdateIndicator = UpdateIndicatorCodeList.Codes.PRG;
			dataProvider.AdditionalMessageInfo.RefundCode = ReasonForRefundCodeList.Codes.RF01;
			dataProvider.DeclarantId = "V13T002";
			dataProvider.PermitNoToUpdateOrCancel = "PERMIT";
			dataProvider.ReplacementPermitNumber = "REPLPERMIT";
			dataProvider.AdditionalMessageInfo.GSTRefundAmount = 3.3m;
			var message = TestMessageBuilder.CusdecMessage;
			string actual = TestMessageBuilder.MessageText;
			Assert("Message Header", actual.Contains("UNH+"));
			AssertEquals("IPTUPD message", "IPTUPD", message.UNH[0].CommonAccessReference);
			Assert("valid BGM Segment", actual.Contains("BGM+916+<<MSGNO PLACEHOLDER>>+13"));
			Assert("Update indicator", actual.Contains("CST+++PRG"));
			Assert("Declaration segment", actual.Contains("GEI+5+:Y'"));
			Assert("Refund Reason", actual.Contains("FTX+ACD+++RF01'"));
			Assert("Message Sender", actual.Contains("RFF+MS:V13T.V13T002'"));
			Assert("Permit", actual.Contains("RFF+ABT:PERMIT'"));
			Assert("Previous Permit", actual.Contains("RFF+AAE:REPLPERMIT'"));
			Assert("Declarant", actual.Contains("NAD+DT'CTA+IC'COM+:TE'"));
			Assert("UNS", actual.Contains("UNS+D'UNS+S'"));
			Assert("GST", actual.Contains("TAX+7+GST'MOA+530:3.30'"));
			string expected = "UNH+WTG+CUSDEC:D:09B:UN:041+IPTUPD'BGM+916+<<MSGNO PLACEHOLDER>>+13'CST+++PRG'GEI+5+:Y'FTX+ACD+++RF01'RFF+MS:V13T.V13T002'RFF+ABT:PERMIT'RFF+AAE:REPLPERMIT'NAD+DT'CTA+IC'COM+:TE'UNS+D'UNS+S'CNT+6:0'TAX+7+GST'MOA+530:3.30'UNT+17+WTG'";
			AssertEquals("Full Message (ie no additional segments)", expected, actual);
		}

		public void TestPartialSpecific()
		{
			var testData = new SGCUSDECTestClass.SGCUSDECTestData();
			dataProvider = testData.SetTestData();
			dataProvider.AdditionalMessageInfo.UpdateIndicator = UpdateIndicatorCodeList.Codes.PRS;
			dataProvider.AdditionalMessageInfo.RefundCode = ReasonForRefundCodeList.Codes.RF02;
			dataProvider.DeclarantId = "V13T002";
			dataProvider.PermitNoToUpdateOrCancel = "PERMIT";
			dataProvider.ReplacementPermitNumber = "REPLPERMIT";
			var item1 = new ItemsTestClass();
			item1.SerialNumber = "1";
			item1.HSCode = "12345678";
			item1.ItemGSTRefund = 1;
			item1.ItemDutyRefund = 2;
			item1.ItemExciseRefund = 3;
			var item2 = new ItemsTestClass();
			item2.SerialNumber = "2";
			item2.HSCode = "87654321";
			item2.ItemGSTRefund = 4;
			item2.ItemDutyRefund = 5;
			item2.ItemExciseRefund = 6;
			dataProvider.Items = new ItemsTestClass[] { item1, item2 };
			var message = TestMessageBuilder.CusdecMessage;
			string actual = TestMessageBuilder.MessageText;
			Assert("Message Header", actual.Contains("UNH+"));
			AssertEquals("IPTUPD message", "IPTUPD", message.UNH[0].CommonAccessReference);
			Assert("valid BGM Segment", actual.Contains("BGM+916+<<MSGNO PLACEHOLDER>>+13"));
			Assert("Update indicator", actual.Contains("CST+++PRS"));
			Assert("Declaration segment", actual.Contains("GEI+5+:Y'"));
			Assert("Refund Reason", actual.Contains("FTX+ACD+++RF02'"));
			Assert("Message Sender", actual.Contains("RFF+MS:V13T.V13T002'"));
			Assert("Permit", actual.Contains("RFF+ABT:PERMIT'"));
			Assert("Previous Permit", actual.Contains("RFF+AAE:REPLPERMIT'"));
			Assert("Declarant", actual.Contains("NAD+DT'CTA+IC'COM+:TE'"));
			Assert("UNSD", actual.Contains("UNS+D'"));
			Assert("CST1", actual.Contains("CST+1+12345678'"));
			Assert("TAX1", actual.Contains("TAX+5+CUD'MOA+530:2.00'TAX+5+EXC'MOA+530:3.00'TAX+7+GST'MOA+530:1.00'"));
			Assert("CST2", actual.Contains("CST+2+87654321'"));
			Assert("TAX2", actual.Contains("TAX+5+CUD'MOA+530:5.00'TAX+5+EXC'MOA+530:6.00'TAX+7+GST'MOA+530:4.00'"));
			Assert("UNSS", actual.Contains("UNS+S'CNT+6:0'"));
			Assert("GST", actual.Contains("TAX+7+GST'MOA+530:5.00'"));
			Assert("Excise", actual.Contains("TAX+5+EXC'MOA+530:9.00'"));
			Assert("Duty", actual.Contains("TAX+5+CUD'MOA+530:7.00'"));
			string expected = "UNH+WTG+CUSDEC:D:09B:UN:041+IPTUPD'BGM+916+<<MSGNO PLACEHOLDER>>+13'CST+++PRS'GEI+5+:Y'FTX+ACD+++RF02'RFF+MS:V13T.V13T002'RFF+ABT:PERMIT'RFF+AAE:REPLPERMIT'NAD+DT'CTA+IC'COM+:TE'UNS+D'CST+1+12345678'TAX+5+CUD'MOA+530:2.00'TAX+5+EXC'MOA+530:3.00'TAX+7+GST'MOA+530:1.00'CST+2+87654321'TAX+5+CUD'MOA+530:5.00'TAX+5+EXC'MOA+530:6.00'TAX+7+GST'MOA+530:4.00'UNS+S'CNT+6:0'TAX+5+CUD'MOA+530:7.00'TAX+5+EXC'MOA+530:9.00'TAX+7+GST'MOA+530:5.00'UNT+35+WTG'";
			AssertEquals("Full Message (ie no additional segments)", expected, actual);
		}

		public void TestPartialSpecificOnlySendsRefundLines()
		{
			var testData = new SGCUSDECTestClass.SGCUSDECTestData();
			dataProvider = testData.SetTestData();
			dataProvider.AdditionalMessageInfo.UpdateIndicator = UpdateIndicatorCodeList.Codes.PRS;
			dataProvider.AdditionalMessageInfo.RefundCode = ReasonForRefundCodeList.Codes.RF02;
			dataProvider.DeclarantId = "V13T002";
			dataProvider.PermitNoToUpdateOrCancel = "PERMIT";
			dataProvider.ReplacementPermitNumber = "REPLPERMIT";
			var item1 = new ItemsTestClass();
			item1.SerialNumber = "1";
			item1.HSCode = "22042113";
			item1.ItemGSTRefund = 0;
			item1.ItemDutyRefund = 0;
			item1.ItemExciseRefund = 0;
			var item2 = new ItemsTestClass();
			item2.SerialNumber = "2";
			item2.HSCode = "22042111";
			item2.ItemGSTRefund = 10;
			item2.ItemDutyRefund = 5;
			item2.ItemExciseRefund = 0;
			var item3 = new ItemsTestClass();
			item3.SerialNumber = "3";
			item3.HSCode = "22042111";
			item3.ItemGSTRefund = 30;
			item3.ItemDutyRefund = 55;
			item3.ItemExciseRefund = 12;
			var item4 = new ItemsTestClass();
			item4.SerialNumber = "4";
			item4.HSCode = "22042113";
			item4.ItemGSTRefund = 100;
			item4.ItemDutyRefund = 20;
			item4.ItemExciseRefund = 0;
			var item5 = new ItemsTestClass();
			item5.SerialNumber = "5";
			item5.HSCode = "22042113";
			item5.ItemGSTRefund = 0;
			item5.ItemDutyRefund = 0;
			item5.ItemExciseRefund = 0;
			var item6 = new ItemsTestClass();
			item6.SerialNumber = "6";
			item6.HSCode = "12345678";
			item6.ItemGSTRefund = 0;
			item6.ItemDutyRefund = 0;
			item6.ItemExciseRefund = 0;
			dataProvider.Items = new ItemsTestClass[] { item1, item2, item3, item4, item5 };
			var message = TestMessageBuilder.CusdecMessage;
			string actual = TestMessageBuilder.MessageText;
			Assert("Message Header", actual.Contains("UNH+"));
			AssertEquals("IPTUPD message", "IPTUPD", message.UNH[0].CommonAccessReference);
			Assert("valid BGM Segment", actual.Contains("BGM+916+<<MSGNO PLACEHOLDER>>+13"));
			Assert("Update indicator", actual.Contains("CST+++PRS"));
			Assert("Declaration segment", actual.Contains("GEI+5+:Y'"));
			Assert("Refund Reason", actual.Contains("FTX+ACD+++RF02'"));
			Assert("Message Sender", actual.Contains("RFF+MS:V13T.V13T002'"));
			Assert("Permit", actual.Contains("RFF+ABT:PERMIT'"));
			Assert("Previous Permit", actual.Contains("RFF+AAE:REPLPERMIT'"));
			Assert("Declarant", actual.Contains("NAD+DT'CTA+IC'COM+:TE'"));
			Assert("UNSD", actual.Contains("UNS+D'"));
			AssertEquals("CST1 should not be included - no refund amounts for this line", false, actual.Contains("CST+1+22042113'"));
			AssertEquals("CST5 should not be included - no refund amounts for this line", false, actual.Contains("CST+5+22042113'"));
			AssertEquals("CST6 should not be included - no refund amounts for this line", false, actual.Contains("CST+6+12345678'"));
			Assert("CST2", actual.Contains("CST+2+22042111'"));
			Assert("TAX2", actual.Contains("TAX+5+CUD'MOA+530:5.00'TAX+7+GST'MOA+530:10.00'"));
			Assert("CST3", actual.Contains("CST+3+22042111'"));
			Assert("TAX1", actual.Contains("TAX+5+CUD'MOA+530:55.00'TAX+5+EXC'MOA+530:12.00'TAX+7+GST'MOA+530:30.00'"));
			Assert("CST4", actual.Contains("CST+4+22042113'"));
			Assert("TAX1", actual.Contains("TAX+5+CUD'MOA+530:20.00'TAX+7+GST'MOA+530:100.00"));
			Assert("UNSS", actual.Contains("UNS+S'CNT+6:0'"));
			Assert("GST", actual.Contains("TAX+7+GST'MOA+530:140.00'"));
			Assert("Excise", actual.Contains("TAX+5+EXC'MOA+530:12.00'"));
			Assert("Duty", actual.Contains("TAX+5+CUD'MOA+530:80.00'"));
			string expected = "UNH+WTG+CUSDEC:D:09B:UN:041+IPTUPD'BGM+916+<<MSGNO PLACEHOLDER>>+13'CST+++PRS'GEI+5+:Y'FTX+ACD+++RF02'RFF+MS:V13T.V13T002'RFF+ABT:PERMIT'RFF+AAE:REPLPERMIT'NAD+DT'CTA+IC'COM+:TE'UNS+D'CST+2+22042111'TAX+5+CUD'MOA+530:5.00'TAX+7+GST'MOA+530:10.00'CST+3+22042111'TAX+5+CUD'MOA+530:55.00'TAX+5+EXC'MOA+530:12.00'TAX+7+GST'MOA+530:30.00'CST+4+22042113'TAX+5+CUD'MOA+530:20.00'TAX+7+GST'MOA+530:100.00'UNS+S'CNT+6:0'TAX+5+CUD'MOA+530:80.00'TAX+5+EXC'MOA+530:12.00'TAX+7+GST'MOA+530:140.00'UNT+38+WTG'";
			AssertEquals("Full Message for Partial Refund - should only include SG32 for refund lines that have a refund amount", expected, actual);
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			dataProvider = new SGCUSDECTestClass();
		}

		SGCUSDECTestClass dataProvider;
		Iptupd09bRefund TestMessageBuilder
		{
			get
			{
				if (fTestMessageBuilder == null)
				{
					fTestMessageBuilder = new Iptupd09bRefund(dataProvider);
				}

				return fTestMessageBuilder;
			}
		}

		Iptupd09bRefund fTestMessageBuilder;
		void ResetMessageBuilder()
		{
			fTestMessageBuilder = null;
		}
		#endregion
	}
}
