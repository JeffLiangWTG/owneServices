using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class MonthlyStatementMessageTest : TestCaseWithFactory
	{
		[TestDate(2018, 02, 05)]
		public void TestMessageProcessing_ACE()
		{
			var message = BuildMessage();

			var statementHeader = Factory.New<CusStatementHeader>();
			statementHeader.B2_ProcessDate = ZDateTime.Now.AddDays(1);

			var header = new MonthlyStatementMessageHeader(statementHeader, message);
			AssertEquals("TEST", header.StatementFor);
			AssertEquals(new ZDateTime(2009, 06, 19), header.B2_PrintDate);
			AssertEquals(ZString.Empty, header.B2_BranchDesignation);
			AssertEquals("GFS", header.B2_EntryFilerCode);
			AssertEquals("1704", header.B2_ProcessPort);
			AssertEquals("11-358469900", header.B2_ImporterCustomsID);
			AssertEquals(statementHeader.B2_ProcessDate, header.B2_ProcessDate);

			CombineAssertions(() =>
			{
				AssertEquals("TotalDuty", 573.69m, header.TotalDuty);
				AssertEquals("TotalAmountDue", 760.39m, header.TotalAmountDue);
				AssertEquals("TotalMerchandiseProcessingFee", 121.73m, header.TotalMerchandiseProcessingFee);
				AssertEquals("TotalHarborMaintenanceFeeWaterways", 64.97m, header.TotalHarborMaintenanceFeeWaterways);

				AssertEquals("FinalTotalDuty", 652.18m, header.FinalTotalDuty);
				AssertEquals("FinalTotalAmountDue", 972.34m, header.FinalTotalAmountDue);
				AssertEquals("FinalTotalMerchandiseProcessingFee", 121.73m, header.FinalTotalMerchandiseProcessingFee);
				AssertEquals("FinalTotalHarborMaintenanceFeeWaterways", 81.35m, header.FinalTotalHarborMaintenanceFeeWaterways);
			});

			AssertEquals(4, header.DailyStatements.Count);
			var stms = header.DailyStatements.Cast<MonthlyStatementMessageLine>();
			var stm1 = stms.FirstOrDefault(x => x.B2_StatementNumber == "1709134338");
			AssertNotNull(stm1);
			AssertEquals(new ZDateTime(2009, 05, 14), stm1.B2_PrintDate);
			AssertEquals(new ZDateTime(2009, 06, 01), stm1.B2_ProcessDate);
			AssertEquals(0m, stm1.FinalTotalDuty);
			AssertEquals(0m, stm1.FinalTotalPayableTax);
			AssertEquals(0m, stm1.FinalTotalADD);
			AssertEquals(0m, stm1.FinalTotalCVD);
			AssertEquals(47.19m, stm1.FinalTotalUserFees);

			var stm2 = stms.FirstOrDefault(x => x.B2_StatementNumber == "1709140423");
			AssertNotNull(stm2);
			AssertEquals(new ZDateTime(2009, 05, 20), stm2.B2_PrintDate);
			AssertEquals(new ZDateTime(2009, 06, 01), stm2.B2_ProcessDate);
			AssertEquals(0m, stm2.FinalTotalDuty);
			AssertEquals(0m, stm2.FinalTotalPayableTax);
			AssertEquals(0m, stm2.FinalTotalADD);
			AssertEquals(0m, stm2.FinalTotalCVD);
			AssertEquals(32.39m, stm2.FinalTotalUserFees);

			var stm3 = stms.FirstOrDefault(x => x.B2_StatementNumber == "1709141343");
			AssertNotNull(stm3);
			AssertEquals(new ZDateTime(2009, 05, 21), stm3.B2_PrintDate);
			AssertEquals(new ZDateTime(2009, 06, 01), stm3.B2_ProcessDate);
			AssertEquals(87.85m, stm3.FinalTotalDuty);
			AssertEquals(0m, stm3.FinalTotalPayableTax);
			AssertEquals(0m, stm3.FinalTotalADD);
			AssertEquals(0m, stm3.FinalTotalCVD);
			AssertEquals(51m, stm3.FinalTotalUserFees);

			var stm4 = stms.FirstOrDefault(x => x.B2_StatementNumber == "1709148415");
			AssertNotNull(stm4);
			AssertEquals(new ZDateTime(2009, 05, 28), stm4.B2_PrintDate);
			AssertEquals(new ZDateTime(2009, 06, 01), stm4.B2_ProcessDate);
			AssertEquals(485.84m, stm4.FinalTotalDuty);
			AssertEquals(0m, stm4.FinalTotalPayableTax);
			AssertEquals(0m, stm4.FinalTotalADD);
			AssertEquals(0m, stm4.FinalTotalCVD);
			AssertEquals(56.12m, stm4.FinalTotalUserFees);

			AssertEquals("4 deleted lines exist", 4, header.MonthlyDeletedLines.Count);

			AssertEquals("Deleted via SU application by the filer.: NEW1", "ABI", header.MonthlyDeletedLines[0].B3_DeletedByParty);
			AssertEquals("FormattedEntryNumber : NEW1", "1013579-7", header.MonthlyDeletedLines[0].FormattedEntryNumber);

			AssertEquals("Deleted via SU application by the filer.: NEW2", "ABI", header.MonthlyDeletedLines[1].B3_DeletedByParty);
			AssertEquals("FormattedEntryNumber : NEW2", "1013569-8", header.MonthlyDeletedLines[1].FormattedEntryNumber);

			AssertEquals("Deleted by CBP", "CBP", header.MonthlyDeletedLines[2].B3_DeletedByParty);
			AssertEquals("FormattedEntryNumber : OLD1", "XXX2013-5795", header.MonthlyDeletedLines[2].FormattedEntryNumber);

			AssertEquals("Deleted by CBP", "CBP", header.MonthlyDeletedLines[3].B3_DeletedByParty);
			AssertEquals("FormattedEntryNumber : OLD2", "XXX2013-5696", header.MonthlyDeletedLines[3].FormattedEntryNumber);
		}

		[TestDate(2017, 07, 17)]
		public void TestMessageProcessing_ACS()
		{
			var message = BuildMessage();
			var statementHeader = Factory.New<CusStatementHeader>();
			statementHeader.B2_ProcessDate = ZDateTime.Now.AddDays(1);

			var header = new MonthlyStatementMessageHeader(statementHeader, message);

			CombineAssertions(() =>
			{
				AssertEquals("TotalDuty", 652.18m, header.TotalDuty);
				AssertEquals("TotalAmountDue", 972.34m, header.TotalAmountDue);
				AssertEquals("TotalMerchandiseProcessingFee", 121.73m, header.TotalMerchandiseProcessingFee);
				AssertEquals("TotalHarborMaintenanceFeeWaterways", 81.35m, header.TotalHarborMaintenanceFeeWaterways);

				AssertEquals("FinalTotalDuty", 573.69m, header.FinalTotalDuty);
				AssertEquals("FinalTotalAmountDue", 760.39m, header.FinalTotalAmountDue);
				AssertEquals("FinalTotalMerchandiseProcessingFee", 121.73m, header.FinalTotalMerchandiseProcessingFee);
				AssertEquals("FinalTotalHarborMaintenanceFeeWaterways", 64.97m, header.FinalTotalHarborMaintenanceFeeWaterways);
			});

			AssertEquals(4, header.DailyStatements.Count);
			var stms = header.DailyStatements.Cast<MonthlyStatementMessageLine>();
			var stm1 = stms.FirstOrDefault(x => x.B2_StatementNumber == "1709134338");
			AssertNotNull(stm1);
			AssertEquals(new ZDateTime(2009, 05, 14), stm1.B2_PrintDate);
			AssertEquals(new ZDateTime(2009, 06, 01), stm1.B2_ProcessDate);
			AssertEquals(0m, stm1.FinalTotalDuty);
			AssertEquals(0m, stm1.FinalTotalPayableTax);
			AssertEquals(0m, stm1.FinalTotalADD);
			AssertEquals(0m, stm1.FinalTotalCVD);
			AssertEquals(47.19m, stm1.FinalTotalUserFees);

			var stm2 = stms.FirstOrDefault(x => x.B2_StatementNumber == "1709140423");
			AssertNotNull(stm2);
			AssertEquals(new ZDateTime(2009, 05, 20), stm2.B2_PrintDate);
			AssertEquals(new ZDateTime(2009, 06, 01), stm2.B2_ProcessDate);
			AssertEquals(0m, stm2.FinalTotalDuty);
			AssertEquals(0m, stm2.FinalTotalPayableTax);
			AssertEquals(0m, stm2.FinalTotalADD);
			AssertEquals(0m, stm2.FinalTotalCVD);
			AssertEquals(32.39m, stm2.FinalTotalUserFees);

			var stm3 = stms.FirstOrDefault(x => x.B2_StatementNumber == "1709141343");
			AssertNotNull(stm3);
			AssertEquals(new ZDateTime(2009, 05, 21), stm3.B2_PrintDate);
			AssertEquals(new ZDateTime(2009, 06, 01), stm3.B2_ProcessDate);
			AssertEquals(87.85m, stm3.FinalTotalDuty);
			AssertEquals(0m, stm3.FinalTotalPayableTax);
			AssertEquals(0m, stm3.FinalTotalADD);
			AssertEquals(0m, stm3.FinalTotalCVD);
			AssertEquals(51m, stm3.FinalTotalUserFees);

			var stm4 = stms.FirstOrDefault(x => x.B2_StatementNumber == "1709148415");
			AssertNotNull(stm4);
			AssertEquals(new ZDateTime(2009, 05, 28), stm4.B2_PrintDate);
			AssertEquals(new ZDateTime(2009, 06, 01), stm4.B2_ProcessDate);
			AssertEquals(485.84m, stm4.FinalTotalDuty);
			AssertEquals(0m, stm4.FinalTotalPayableTax);
			AssertEquals(0m, stm4.FinalTotalADD);
			AssertEquals(0m, stm4.FinalTotalCVD);
			AssertEquals(56.12m, stm4.FinalTotalUserFees);
		}

		MQEDIMessage BuildMessage()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_ApplicationCode = "USI";
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement;
			message.EM_MessageSubType = "STM";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_SystemCreateTimeUtc = ZDateTime.Now;

			message.EM_MessageText =
				"B011704GFSMSF1709P06772061909711-358469900    1704GFS                           "
				+ "Q117091343381704GFS11-3584699000514090601090000000000000000000000               "
				+ "Q2000000000000000000000000000004719                                             "
				+ "QA014990000000295850100000001761                                                "
				+ "Q117091404231704GFS11-3584699000520090601090000000000000000000000               "
				+ "Q2000000000000000000000000000003239                                             "
				+ "QA024990000000250050100000000739                                                "
				+ "Q117091413431704GFS11-3584699000521090601090000000878500000000000               "
				+ "Q2000000000000000000000000000013885                                             "
				+ "QA034990000000319750100000001903                                                "
				+ "Q117091484151704GFS11-3584699000528090601090000004858400000000000               "
				+ "Q2000000000000000000000000000054196                                             "
				+ "QA044990000000351850100000002094                                                "
				+ "Q31709P06772061909061909GFS11-3584699000000005736900000000000                   "
				+ "Q4000000000000000000000000000076039                                             "
				+ "QE014990000001217350100000006497                                                "
				+ "Q51709P06772061909061909GFS11-3584699000000006521800000000000                   "
				+ "Q6000000000000000000000000000097234                                             "
				+ "QJ014990000001217350100000008135                                                "
				+ "Q71709148415GFS  10135797ABIGFS  10135698ABI                                    "
				+ "Q78804091002XXX20135795CBPXXX20135696CBP                                        "
				+ "Y  1704GFSMS00018";

			Factory.Save();

			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "TEST";
			org.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "11-358469900");

			return message;
		}
	}
}
