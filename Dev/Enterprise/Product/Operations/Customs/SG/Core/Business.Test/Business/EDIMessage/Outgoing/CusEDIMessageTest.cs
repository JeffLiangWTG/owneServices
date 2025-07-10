using System;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.SG.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(CUSDECEDIMessage))]
	sealed class CusEDIMessageTest : SGEDIMessageTest
	{
		public void TestReceiveTransmit()
		{
			AssertEquals(CUSDECEDIMessage.Direction.Transmit, Message.EM_ReceiveTransmit);
		}

		[TestDate(2007, 1, 1)]
		public void TestPopulateMessageNumber()
		{
			try
			{
				Db.Connection.BeginTransaction(); // required for number generation in unit test
				GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "AABBCCDDE";
				SGCustomsDataRegistry.Instance.MessageNumberOffset.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 900);
				Env.NumberFountains.SGMessageNumberSequence.SetNext(Db.Connection, 9999);
				Env.NumberFountains.SGMessageNumberSequence.GetNextFormatted(Db.Connection);
				Message.EM_MessageText = "TEST+++<<MSGNO PLACEHOLDER>>+++TEST";
				Message.PopulateMessageNumber();
				AssertEquals("200701010901", Message.EM_MessageNum);
				AssertEquals("AABBCCDDE           200701010901", Message.EM_ApplicationReference);
				AssertEquals("TEST+++AABBCCDDE           200701010901+++TEST", Message.EM_MessageText);
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}

		public void TestCUSDEC()
		{
			CUSDECEDIMessage message = Factory.New<CUSDECEDIMessage>();
			message.EM_MessageText = "UNH+9362+CUSDEC:D:05B:UN:040+OUTDEC'BGM+914:::BKT+XXXXXXXXE47T        200609220001+9'CST++2'LOC+11+LW222:::TESTING47,28 JOO KOON CIRCLE SINGAPORE 629057'LOC+12+AEFJR'LOC+61+AEFJR'LOC+88+PPLW:::PASIR PANJANG LIGHTER WHARVES, PASIR PANJANG LIGHTER WHARVES'LOC+130+AEFJR'LOC+234+WPN:::WESTERN PETROLUME ANCHORAGE'DTM+136:20060923:102'GEI+5+:Y'MEA+ABK++PKG:1.0000'MEA+AAH++TNE:0.0150'MEA+AAN++:13712.00'RFF+MS:E47T.E47T001'TDT+12++1+:::CV++++6:::ARCTIC BREEZE'TPL+::::MV'DOC+741+NA'NAD+CA+10042360000C++INCHAPE SHIPPING SERVICES (S) PTE:LTD'NAD+EX+XXXXXXXXE47T++TESTING47'NAD+DT++USER47'CTA+IC+:1619574Z'COM+12345678:TE'NAD+BB'RFF+DAN:D'UNS+D'CST+1+24022090+169'FTX+AAA+++MARLBORO LIGHT CIGS'FTX+PRD+++MARLBORO'LOC+18+SNTB3132'LOC+27+PH'MEA+AAF++KGM:10.0000'MEA+AAE++STK:10000.0000'MEA+AAI++STK:1.0000'MEA+ABA++KGM:10.0000'PAC+1+3+CAR'PAC+50+2+BOX'PAC+10+1+PKT'PAC+20+5+STK'MOA+63:800.00'RFF+AEA:SEASTORE'GIN+AV+019+30'DOC+714+TestOut'UNS+S'CNT+5:1'TAX+1'MOA+63:800.00'UNT+47+9362'";
			AssertEquals("PERMIT APPLICATION", message.EM_MessageInterpretation);
		}

		public void TestSupportingDocuments()
		{
			CUSDECEDIMessage message = Factory.New<CUSDECEDIMessage>();
			message.EM_MessageText = "UNH+CWISEB00002403+CUSDEC:D:05B:UN:040+INPDEC'BGM+914:::BKT+12247970000Z        200709197224+9'CST++1'LOC+11+CZ'LOC+88+PUB'DTM+178:20070928:102'GEI+5+:Y'MEA+ABK++TUB:2.0000'MEA+AAH++KGM:4535.9250'FTX+AAI+++FE-CERT-CARGOWISE-BKT(WATER)- WITH SUPPORTING DOCS'RFF+MS:V13T.V13T003'DOC+:::DOCUMENT ATTACHMENT+001::ARN - WORD-SUPPORTING DOC.DOC'DOC+:::DOCUMENT ATTACHMENT+002::BDR - BANK DRAFT.PDF'DOC+:::DOCUMENT ATTACHMENT+003::CHG - 29 HEALTHIEST FOODS.PDF'DOC+:::DOCUMENT ATTACHMENT+004::COR - CERTIFICATE OF RECEIPT.PDF'DOC+:::DOCUMENT ATTACHMENT+005::WSH - WORKSHEET.PDF'NAD+AE+12247970000Z++EDI DEMONSTRATION SYSTEM SG'NAD+DT++IGOR SAFONOV'CTA+IC+:P0342222 'COM+80012200:TE'NAD+IM+51100440000G++PUBLIC UTILITIES BORAD'UNS+D'DMS+INVOICE DETAILS'MOA+39:5000.00:SGD'TOD+++FOB'NAD+SE'DOC+380+1'DTM+3:20070918:102'CST+1+22019090'FTX+AAA+++OTHER'FTX+PRD+++ENER WATER'FTX+AAC+++N'LOC+27+LC'MEA+AAF++LTR:10.0000'MEA+AAE++LTR:8.0000'MEA+AAI++LTR:0.5000'PAC+1+3+CTN'PAC+16+2+BOT'MOA+63:5000.00'MOA+146:500.0000:SGD'RFF+IV:1'RFF+SE'TAX+5++++LTR:::30.0000'MOA+161:240.00'TAX+7++++:::7'MOA+124:366.80'UNS+S'CNT+5:1'TAX+5'MOA+161:240.00'TAX+1'MOA+63:5000.00'TAX+7'MOA+124:366.80'UNT+55+CWISEB00002403'";
			AssertEquals(5, message.SupportingDocuments.Count);
			AssertEquals("ARN - WORD-SUPPORTING DOC.DOC", message.SupportingDocuments["001"].Description);
			AssertEquals("BDR - BANK DRAFT.PDF", message.SupportingDocuments["002"].Description);
			AssertEquals("CHG - 29 HEALTHIEST FOODS.PDF", message.SupportingDocuments["003"].Description);
			AssertEquals("COR - CERTIFICATE OF RECEIPT.PDF", message.SupportingDocuments["004"].Description);
			AssertEquals("WSH - WORKSHEET.PDF", message.SupportingDocuments["005"].Description);
		}

		[ExpectNoExceptions]
		public void TestSupportingDocuments_NoDocuments()
		{
			CUSDECEDIMessage message = Factory.New<CUSDECEDIMessage>();
			message.EM_MessageText = "UNH+CWISEB00002403+CUSDEC:D:05B:UN:040+INPDEC'BGM+914:::BKT+12247970000Z        200709197224+9'CST++1'LOC+11+CZ'LOC+88+PUB'DTM+178:20070928:102'GEI+5+:Y'MEA+ABK++TUB:2.0000'MEA+AAH++KGM:4535.9250'FTX+AAI+++FE-CERT-CARGOWISE-BKT(WATER)- WITH SUPPORTING DOCS'RFF+MS:V13T.V13T003'NAD+AE+12247970000Z++EDI DEMONSTRATION SYSTEM SG'NAD+DT++IGOR SAFONOV'CTA+IC+:P0342222 'COM+80012200:TE'NAD+IM+51100440000G++PUBLIC UTILITIES BORAD'UNS+D'DMS+INVOICE DETAILS'MOA+39:5000.00:SGD'TOD+++FOB'NAD+SE'DOC+380+1'DTM+3:20070918:102'CST+1+22019090'FTX+AAA+++OTHER'FTX+PRD+++ENER WATER'FTX+AAC+++N'LOC+27+LC'MEA+AAF++LTR:10.0000'MEA+AAE++LTR:8.0000'MEA+AAI++LTR:0.5000'PAC+1+3+CTN'PAC+16+2+BOT'MOA+63:5000.00'MOA+146:500.0000:SGD'RFF+IV:1'RFF+SE'TAX+5++++LTR:::30.0000'MOA+161:240.00'TAX+7++++:::7'MOA+124:366.80'UNS+S'CNT+5:1'TAX+5'MOA+161:240.00'TAX+1'MOA+63:5000.00'TAX+7'MOA+124:366.80'UNT+55+CWISEB00002403'";
			_ = message.SupportingDocuments;
		}

		[TestDate(2011, 05, 23)]
		public void TestPopulateMessageNumberForTradeNet4_1()
		{
			try
			{
				Db.Connection.BeginTransaction(); // Unit test involving number fountain
				GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "199702247W";
				SGCustomsDataRegistry.Instance.MessageNumberOffset.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 900);
				Env.NumberFountains.SGMessageNumberSequence.SetNext(Db.Connection, 9999);
				Env.NumberFountains.SGMessageNumberSequence.GetNextFormatted(Db.Connection);
				var declaration = Factory.New<JobDeclaration>();
				AssertEquals("Pre-condition - Declaration should have been created as requiring TN4.1, as registry implementation date has occurred.", SGConstants.TradeNetVersion.FourPointOne, declaration.JE_ApplicationCode);
				var entry = declaration.CustomsEntryHeaders.AddNew();
				Message.EM_LinkedObject = entry;
				Message.EM_LinkTable = entry.TablePrefix;
				Message.EM_MessageText = "TEST+++<<MSGNO PLACEHOLDER>>+++TEST";
				Message.PopulateMessageNumber();
				AssertEquals("201105230901", Message.EM_MessageNum);
				AssertEquals("UEN component of Msg Number is only 17 characters in TradeNet 4.1", "199702247W       201105230901", Message.EM_ApplicationReference);
				AssertEquals("TEST+++199702247W       201105230901+++TEST", Message.EM_MessageText);
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}

		[TestDate(2014, 04, 04)]
		public void TestMessageNumberLimit()
		{
			try
			{
				Db.Connection.BeginTransaction();
				GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "199702247W";
				SGCustomsDataRegistry.Instance.MessageNumberOffset.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5000); // Set number offset to maximimum allowed
				Env.NumberFountains.SGMessageNumberSequence.SetNext(Db.Connection, 9999);
				var declaration = Factory.New<JobDeclaration>();
				var entry = declaration.CustomsEntryHeaders.AddNew();
				Message.EM_LinkedObject = entry;
				Message.EM_LinkTable = entry.TablePrefix;
				Message.EM_MessageText = "TEST+++<<MSGNO PLACEHOLDER>>+++TEST";
				Message.PopulateMessageNumber();
				AssertEquals("Sequence number at limit with offset value", "201404045000", Message.EM_MessageNum);
				AssertEquals("Sequence number component of URN is the last 4 digits", "TEST+++199702247W       201404045000+++TEST", Message.EM_MessageText);
				Message.EM_MessageText = "TEST+++<<MSGNO PLACEHOLDER>>+++TEST";
				Message.PopulateMessageNumber();
				AssertEquals("Sequence number when rolling back to starting sequence should still be offset by registry offset value", "201404045001", Message.EM_MessageNum);
				AssertEquals("Sequence number component of URN is the last 4 digits", "TEST+++199702247W       201404045001+++TEST", Message.EM_MessageText);
				Env.NumberFountains.SGMessageNumberSequence.SetNext(Db.Connection, 1000);
				Message.EM_MessageText = "TEST+++<<MSGNO PLACEHOLDER>>+++TEST";
				Message.PopulateMessageNumber();
				AssertEquals("Sequence number in normal use offset by registry offset value", "201404046000", Message.EM_MessageNum);
				AssertEquals("Sequence number component of URN is the last 4 digits", "TEST+++199702247W       201404046000+++TEST", Message.EM_MessageText);
				Env.NumberFountains.SGMessageNumberSequence.SetNext(Db.Connection, 8000);
				Message.EM_MessageText = "TEST+++<<MSGNO PLACEHOLDER>>+++TEST";
				Message.PopulateMessageNumber();
				AssertEquals("Sequence number when offset would take value above 9999 daily limit rolls back around to start but still incorporates offest", "201404043001", Message.EM_MessageNum);
				AssertEquals("Sequence number component of URN is the last 4 digits", "TEST+++199702247W       201404043001+++TEST", Message.EM_MessageText);
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}

		[TestDate(2014, 04, 04)]
		public void TestDailyLimitOfURNs()
		{
			try
			{
				Db.Connection.BeginTransaction();
				GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "199702247W";
				Env.NumberFountains.SGMessageNumberSequence.SetNext(Db.Connection, 1);
				var declaration = Factory.New<JobDeclaration>();
				var entry = declaration.CustomsEntryHeaders.AddNew();
				int urnSequenceValuesGenerated = 0;
				Message.EM_LinkedObject = entry;
				Message.EM_LinkTable = entry.TablePrefix;
				Message.EM_MessageText = "TEST+++<<MSGNO PLACEHOLDER>>+++TEST";
				for (int i = 1; i < 10000; i++)
				{
					Message.PopulateMessageNumber();
					var expectedMsgNumGenerated = "20140404" + i.ToString().PadLeft(4, '0');
					AssertEquals("Daily URN sequence number generation", expectedMsgNumGenerated, Message.EM_MessageNum);
					urnSequenceValuesGenerated++;
				}

				AssertEquals("urnSequenceValuesGenerated - there is a maximum of 9999 daily values", 9999, urnSequenceValuesGenerated);
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}

		[TestDate(2014, 04, 04)]
		public void TestUsingOffsetStillAllowsDailyLimitOfURNs()
		{
			try
			{
				Db.Connection.BeginTransaction();
				GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "199702247W";
				SGCustomsDataRegistry.Instance.MessageNumberOffset.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 3000);
				Env.NumberFountains.SGMessageNumberSequence.SetNext(Db.Connection, 7500);
				var declaration = Factory.New<JobDeclaration>();
				var entry = declaration.CustomsEntryHeaders.AddNew();
				int urnSequenceValuesGenerated = 0;
				int startingOffsetCalculatedForExistingSequencePosition = 500;
				Message.EM_LinkedObject = entry;
				Message.EM_LinkTable = entry.TablePrefix;
				Message.EM_MessageText = "TEST+++<<MSGNO PLACEHOLDER>>+++TEST";
				for (int i = 1; i < 10000; i++)
				{
					Message.PopulateMessageNumber();
					var expectedMsgNumGenerated = "20140404" + (i + startingOffsetCalculatedForExistingSequencePosition).ToString().PadLeft(4, '0');
					if (expectedMsgNumGenerated == "2014040410000")
					{
						expectedMsgNumGenerated = "201404040001"; //sequence must start at 1 when reset, cannot be zero in message
						startingOffsetCalculatedForExistingSequencePosition = -9499;
					}

					AssertEquals("Daily URN sequence number generation with offset value", expectedMsgNumGenerated, Message.EM_MessageNum);
					urnSequenceValuesGenerated++;
				}

				AssertEquals("urnSequenceValuesGenerated - there is a maximum of 9999 daily values, this should be possible even if offset is used", 9999, urnSequenceValuesGenerated);
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}

		[TestDate(2018, 05, 23)]
		public void TestPopulateMessageNumberFromCompanySMNNumberFoutain()
		{
			GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "CON2556";
			var customsNumber = GlbCompany.CurrentCompany.CustomsNumberProvider.CustomsNumbers.AddNew();
			customsNumber.SN_Type = NumberRangeTypeList.Codes.SingaporeMessageNumber;
			customsNumber.SN_MinimumValue = 1111;
			customsNumber.SN_MaximumValue = 2222;
			customsNumber.SN_Value = 1111;
			customsNumber.SN_FountainName = "CON2556";
			customsNumber.Factory.Save();
			var numberFoutain = customsNumber.TryGetNumberFountain();
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			Message.EM_LinkedObject = entry;
			Message.EM_LinkTable = entry.TablePrefix;
			Message.EM_MessageText = "TEST+++<<MSGNO PLACEHOLDER>>+++TEST";
			Message.PopulateMessageNumber();
			AssertEquals("Sequence number begins from current value", "201805231111", Message.EM_MessageNum);
			Message.PopulateMessageNumber();
			AssertEquals("Sequence number begins from current value", "201805231112", Message.EM_MessageNum);
		}

		[TestDate(2018, 05, 23)]
		public void TestPopulateMessageNumberFromCompanySMNNumberFoutainRollover()
		{
			GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "CON2556";
			var customsNumber = GlbCompany.CurrentCompany.CustomsNumberProvider.CustomsNumbers.AddNew();
			customsNumber.SN_Type = NumberRangeTypeList.Codes.SingaporeMessageNumber;
			customsNumber.SN_MinimumValue = 1111;
			customsNumber.SN_MaximumValue = 2222;
			customsNumber.SN_Value = 1111;
			customsNumber.SN_FountainName = "CON2556";
			customsNumber.Factory.Save();
			var numberFoutain = customsNumber.TryGetNumberFountain();
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			Message.EM_LinkedObject = entry;
			Message.EM_LinkTable = entry.TablePrefix;
			Message.EM_MessageText = "TEST+++<<MSGNO PLACEHOLDER>>+++TEST";
			numberFoutain.SetNext(Db.Connection, 2222);
			Message.PopulateMessageNumber();
			AssertEquals("Sequence number begins from current value", "201805232222", Message.EM_MessageNum);
			Message.PopulateMessageNumber();
			AssertEquals("Sequence number rolling back", "201805231111", Message.EM_MessageNum);
			SGCustomsDataRegistry.Instance.MessageNumberOffset.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);
			Message.PopulateMessageNumber();
			AssertEquals("Sequence number should NOT be offset by registry offset value", "201805231112", Message.EM_MessageNum);
		}

		CusEDIMessageTestClass Message
		{
			get
			{
				return message ?? (message = Factory.New<CusEDIMessageTestClass>());
			}
		}

		CusEDIMessageTestClass message;
		#region Test Class
		class CusEDIMessageTestClass : CUSDECEDIMessage
		{
			public CusEDIMessageTestClass(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public new void PopulateMessageNumber()
			{
				base.PopulateMessageNumber();
			}
		}

		#endregion
	}
}
