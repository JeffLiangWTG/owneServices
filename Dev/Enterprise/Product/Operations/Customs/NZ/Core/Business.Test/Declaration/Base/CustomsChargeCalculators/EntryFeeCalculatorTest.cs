using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.NZ.Business.Declaration.CustomsChargeCalculators.Testing
{
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Customs.Universal.Testing;
	using NUnit.Framework;

	class ImportTransactionFeeChangesFromJuly2010Test : TestCaseWithFactory
	{
		public void TestFeeCharges()
		{
			CombineAssertions(() =>
			{
				AssertFees(new ZDate(2013, 08, 01), 15.33m, 25.4435m, 312.8889m, 26.66m, 10.4435m, 15.6m, 10.01m, 25.07m, 10.01m, 25.07m, 400m, 1000m);
				AssertFees(new ZDate(2015, 07, 01), 17.37m, 25.4435m, 312.8889m, 26.66m, 10.4435m, 15.6m, 10.01m, 25.07m, 10.01m, 25.07m, 400m, 1000m);
				AssertFees(new ZDate(2018, 06, 30), 17.37m, 25.4435m, 312.8889m, 26.66m, 10.4435m, 15.6m, 10.01m, 25.07m, 10.01m, 25.07m, 400m, 1000m);
				AssertFees(new ZDate(2018, 07, 01), 20.36m, 25.4435m, 312.8889m, 26.66m, 10.4435m, 15.6m, 10.01m, 25.07m, 10.01m, 25.07m, 400m, 1000m);
				AssertFees(new ZDate(2021, 07, 01), 20.36m, 25.4435m, 312.8889m, 26.66m, 10.4435m, 15.6m, 10.01m, 25.07m, 12.48m, 16.16m, 1000m, 1000m);
			});
		}

		void AssertFees(ZDate transmitDate, decimal biosecurityLevy, decimal importEntry, decimal inwardCargoSea, decimal inwardCargoAir, decimal secureExportPartner, decimal nonSecureExportPartner, decimal outwardCargoAir, decimal outwardCargoSea, decimal outwardReportAir, decimal outwardReportSea, decimal dem, decimal lvt)
		{
			var calculator = new EntryFeeCalculator.FeeChargeCalculator(transmitDate, Factory);

			AssertEquals(transmitDate + " BiosecuritySystemEntryLevy", biosecurityLevy, calculator.BiosecuritySystemEntryLevy);
			AssertEquals(transmitDate + " ImportEntryTransactionFee", importEntry, calculator.ImportEntryTransactionFee);
			AssertEquals(transmitDate + " InwardCargoTransactionFeeSea", inwardCargoSea, calculator.InwardCargoTransactionFeeSea);
			AssertEquals(transmitDate + " InwardCargoTransactionFeeAir", inwardCargoAir, calculator.InwardCargoTransactionFeeAir);
			AssertEquals(transmitDate + " ExportEntryTransactionFeeSecureExportPartners", secureExportPartner, calculator.ExportEntryTransactionFeeSecureExportPartners);
			AssertEquals(transmitDate + " ExportEntryTransactionFeeNonSecureExportPartners", nonSecureExportPartner, calculator.ExportEntryTransactionFeeNonSecureExportPartners);
			AssertEquals(transmitDate + " OutwardCargoTransactionFeeAir", outwardCargoAir, calculator.OutwardCargoTransactionFeeAir);
			AssertEquals(transmitDate + " OutwardCargoTransactionFeeSea", outwardCargoSea, calculator.OutwardCargoTransactionFeeSea);
			AssertEquals(transmitDate + " OutwardReportTransactionFeeAir", outwardReportAir, calculator.OutwardReportTransactionFeeAir);
			AssertEquals(transmitDate + " OutwardReportTransactionFeeSea", outwardReportSea, calculator.OutwardReportTransactionFeeSea);
			AssertEquals(transmitDate + " Deminimus", dem, calculator.Deminimus);
			AssertEquals(transmitDate + " LowValue", lvt, calculator.LowValue);
		}

		public void TestEntryFeesIncludingGSTAfterJBMSFeeChangesEffectiveDate()
		{
			var feeChargeCal = new EntryFeeCalculator.FeeChargeCalculator(new DateTime(2021, 07, 01), Factory);

			AssertFeePlusGSTMatchesAdvisedNewRate(feeChargeCal.ImportEntryTransactionFee, 29.26m);
			AssertFeePlusGSTMatchesAdvisedNewRate(feeChargeCal.BiosecuritySystemEntryLevy, 23.41m);

			AssertFeePlusGSTMatchesAdvisedNewRate(feeChargeCal.InwardCargoTransactionFeeAir, 30.66m);
			AssertFeePlusGSTMatchesAdvisedNewRate(feeChargeCal.InwardCargoTransactionFeeSea, 359.82m);

			AssertFeePlusGSTMatchesAdvisedNewRate(feeChargeCal.ExportEntryTransactionFeeSecureExportPartners, 12.01m);
			AssertFeePlusGSTMatchesAdvisedNewRate(feeChargeCal.ExportEntryTransactionFeeNonSecureExportPartners, 17.94m);

			AssertFeePlusGSTMatchesAdvisedNewRate(feeChargeCal.OutwardCargoTransactionFeeAir, 11.51m);
			AssertFeePlusGSTMatchesAdvisedNewRate(feeChargeCal.OutwardCargoTransactionFeeSea, 28.83m);

			AssertFeePlusGSTMatchesAdvisedNewRate(feeChargeCal.OutwardReportTransactionFeeAir, 14.35m);
			AssertFeePlusGSTMatchesAdvisedNewRate(feeChargeCal.OutwardReportTransactionFeeSea, 18.58m);
		}

		void AssertFeePlusGSTMatchesAdvisedNewRate(ZDecimal feeAmount, ZDecimal advisedRate)
		{
			ZDecimal gst = feeAmount * 15 / 100;
			ZDecimal result = feeAmount + gst;
			AssertEquals(advisedRate, result.Round(2));
		}

		public void TestTemporaryImportEntries()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Temporary;
			AssertEquals("Pre-condition: declaration.IsTemporary", true, declaration.IsTemporary);

			var entryLine = declaration.CusEntryHeader.MergedLines.AddNew();
			entryLine.DutyAmount = 10.00m;
			entryLine.GSTAmount = 40.00m;
			AssertEquals("Pre-condition: declaration.IsDutyDeminimus", false, declaration.IsDutyDeminimus);

			CombineAssertions(() =>
			{
				AssertEntryFeesAndLeviesOnCusEntryHeader(declaration, new ZDate(2010, 06, 30), 0.00m, 0.00m);
				AssertEntryFeesAndLeviesOnCusEntryHeader(declaration, new ZDate(2010, 07, 01), 36.55m, 4.57m);
				AssertEntryFeesAndLeviesOnCusEntryHeader(declaration, new ZDate(2018, 06, 30), 42.81m, 6.43m);
				AssertEntryFeesAndLeviesOnCusEntryHeader(declaration, new ZDate(2018, 07, 01), 45.80m, 6.87m);
			});
		}

		public void TestPrivateImportConsignmentsOnWhichDutyAndGstOfMoreThanThe50DollarsDeMinimisIsPayable()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Temporary;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration.OtherInfos.AddNew(HeaderOtherInfoList.Codes.PrivateImportTransactionFee, "");
			AssertEquals("Pre-condition: declaration.IsPrivateImport", true, declaration.IsPrivateImport);

			var entryLine = declaration.CusEntryHeader.MergedLines.AddNew();
			entryLine.DutyAmount = 10.00m;
			entryLine.GSTAmount = 40.00m;
			AssertEquals("Pre-condition: declaration.IsDutyDeminimus", false, declaration.IsDutyDeminimus);

			CombineAssertions(() =>
			{
				AssertEntryFeesAndLeviesOnCusEntryHeader(declaration, new ZDate(2010, 06, 30), 0.00m, 0.00m);
				AssertEntryFeesAndLeviesOnCusEntryHeader(declaration, new ZDate(2010, 07, 01), 36.55m, 4.57m);
				AssertEntryFeesAndLeviesOnCusEntryHeader(declaration, new ZDate(2018, 06, 30), 42.81m, 6.43m);
				AssertEntryFeesAndLeviesOnCusEntryHeader(declaration, new ZDate(2018, 07, 01), 45.80m, 6.87m);
			});
		}

		void AssertEntryFeesAndLeviesOnCusEntryHeader(JobDeclaration declaration, ZDate transmitDate, ZDecimal entryFeeAmount, ZDecimal entryFeeGST)
		{
			declaration.JE_EDITransmitDate = transmitDate;
			var calculator = new EntryFeeCalculator(declaration);
			var entryHeader = declaration.CusEntryHeader;
			calculator.SetEntryFeesAndLeviesOn(entryHeader);

			AssertEquals(transmitDate + " EntryFeeAmount", entryFeeAmount, entryHeader.EntryFeeAmount);
			AssertEquals(transmitDate + " EntryFeeGST", entryFeeGST, entryHeader.EntryFeeGST);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TaxOrFeeTestHelper.SetUp(Factory);
		}
	}

	public class EntryFeeCalculatorTest : TestCaseWithFactory
	{
		[TestDate(2015, 7, 1)]
		public void TestCalculatorUsesCachedTodaysDateIfEDITransmitDateIsEmpty()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Temporary;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration.JE_EDITransmitDate = ZDateTime.Empty;
			CusEntryLine entryLine = declaration.CusEntryHeader.MergedLines.AddNew();
			entryLine.DutyAmount = 100.00m;
			entryLine.GSTAmount = 100.00m;

			EntryFeeCalculator calculator = new EntryFeeCalculator(declaration);
			CusEntryHeader entryHeader = declaration.CusEntryHeader;
			calculator.SetEntryFeesAndLeviesOn(declaration.CusEntryHeader);
			CombineAssertions(delegate
			{
				AssertEquals("EntryFeeAmount", 42.81m, entryHeader.EntryFeeAmount);
				AssertEquals("EntryFeeGST", 6.43m, entryHeader.EntryFeeGST);
			});
		}

		public void TestSetEntryFeesAndLeviesOn()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Completion;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration.JE_EDITransmitDate = new ZDateTime(2006, 10, 1);
			CusEntryLine entryLine = declaration.CusEntryHeader.MergedLines.AddNew();
			entryLine.DutyAmount = 100.00m;
			entryLine.GSTAmount = 100.00m;

			EntryFeeCalculator calculator = new EntryFeeCalculator(declaration);
			CusEntryHeader entryHeader = declaration.CusEntryHeader;
			calculator.SetEntryFeesAndLeviesOn(declaration.CusEntryHeader);
			CombineAssertions(delegate
			{
				AssertEquals("EntryFeeValue", 29.00m, entryHeader.EntryFeeAmount);
				AssertEquals("EntryFeeGST", 3.62m, entryHeader.EntryFeeGST);
			});

			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Temporary;
			declaration.JE_EDITransmitDate = new ZDateTime(2006, 10, 1);
			calculator = new EntryFeeCalculator(declaration);
			entryHeader = declaration.CusEntryHeader;
			calculator.SetEntryFeesAndLeviesOn(declaration.CusEntryHeader);
			AssertEquals("EntryFeeValue", 0.00m, entryHeader.EntryFeeAmount);
			AssertEquals("EntryFeeGST", 0.00m, entryHeader.EntryFeeGST);
		}

		public void TestEntryFeeForImportECIAir()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;

			CombineAssertions(() =>
			{
				AssertFees(declaration, Jul_2008, 26.67m, 3.33m, 0.00m, 0.00m, 30.00m);
				AssertFees(declaration, Oct_2010, 26.66m, 4.00m, 0.00m, 0.00m, 30.66m);
			});
		}

		public void TestEntryFeeForImportECISea()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			CombineAssertions(() =>
			{
				AssertFees(declaration, Jul_2008, 312.89m, 39.11m, 0.00m, 0.00m, 352.00m);
				AssertFees(declaration, Oct_2010, 312.89m, 46.93m, 0.00m, 0.00m, 359.82m);
			});
		}

		public void TestEntryFeeForExportECIAir()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;

			CombineAssertions(() =>
			{
				AssertFees(declaration, Jul_2008, 6.67m, 0.83m, 0.00m, 0.00m, 7.50m);
				AssertFees(declaration, Oct_2010, 10.01m, 1.50m, 0.00m, 0.00m, 11.51m);
			});
		}

		public void TestEntryFeeForExportECISea()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			CombineAssertions(() =>
			{
				AssertFees(declaration, Jul_2008, 25.07m, 3.13m, 0.00m, 0.00m, 28.20m);
				AssertFees(declaration, Oct_2010, 25.07m, 3.76m, 0.00m, 0.00m, 28.83m);
			});
		}

		public void TestEntryFeeForExportFormalSEP()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.OtherInfos.AddNew(HeaderOtherInfoList.Codes.SecureExportPartnership, "");

			CombineAssertions(() =>
			{
				AssertFees(declaration, Jul_2008, 10.44m, 1.31m, 0.00m, 0.00m, 11.75m);
				AssertFees(declaration, Oct_2010, 10.44m, 1.57m, 0.00m, 0.00m, 12.01m);
			});
		}

		public void TestEntryFeeForExportFormalNonSEP()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;

			CombineAssertions(() =>
			{
				AssertFees(declaration, Jul_2008, 12.67m, 1.58m, 0.00m, 0.00m, 14.25m);
				AssertFees(declaration, Oct_2010, 15.6m, 2.34m, 0.00m, 0.00m, 17.94m);
			});
		}

		// Import, Normal job is EntryFeeUnPayable.
		public void TestEntryFeeForImportFormalNormal()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			CusEntryLine entryLine = declaration.CusEntryHeader.MergedLines.AddNew();
			entryLine.DutyAmount = 100.00m;
			entryLine.GSTAmount = 100.00m;

			CombineAssertions(() =>
			{
				AssertFees(declaration, Jul_2008, 0m, 0m, 0m, 0m, 0m);
				AssertFees(declaration, Jul_2010, 0m, 0m, 0m, 0m, 0m);
				AssertFees(declaration, Oct_2010, 0m, 0m, 0m, 0m, 0m);
				AssertFees(declaration, Jul_2015, 0m, 0m, 0m, 0m, 0m);
				AssertFees(declaration, Jul_2018, 0m, 0m, 0m, 0m, 0m);
			});
		}

		public void TestEntryFeeForImportFormalTemporary()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Temporary;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			AssertEquals("Precondition: Declaration.IsTemporary", true, declaration.IsTemporary);
			CusEntryLine entryLine = declaration.CusEntryHeader.MergedLines.AddNew();
			entryLine.DutyAmount = 100.00m;
			entryLine.GSTAmount = 100.00m;

			CombineAssertions(() =>
			{
				AssertFees(declaration, Jul_2008, 0.00m, 0.00m, 0.00m, 0.00m, 0.00m);
				AssertFees(declaration, Jul_2010, 25.44m, 3.18m, 11.11m, 1.39m, 41.12m);
				AssertFees(declaration, Oct_2010, 25.44m, 3.82m, 15.33m, 2.30m, 46.89m);
				AssertFees(declaration, Jul_2015, 25.44m, 3.82m, 17.37m, 2.61m, 49.24m);
				AssertFees(declaration, Jul_2018, 25.44m, 3.82m, 20.36m, 3.05m, 52.67m);
			});
		}

		public void TestEntryFeeForImportFormalIntoBond()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Temporary;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration.WarehouseDocAddress.E2_OA_Address = ZGuid.NewZGuid();
			AssertEquals("Precondition: Declaration.IsBond", true, declaration.IsBond);
			CusEntryLine entryLine = declaration.CusEntryHeader.MergedLines.AddNew();
			entryLine.DutyAmount = 100.00m;
			entryLine.GSTAmount = 100.00m;

			CombineAssertions(() =>
			{
				AssertFees(declaration, Jul_2008, 0.00m, 0.00m, 0.00m, 0.00m, 0.00m);
				AssertFees(declaration, Jul_2010, 25.44m, 3.18m, 11.11m, 1.39m, 41.12m);
				AssertFees(declaration, Oct_2010, 25.44m, 3.82m, 15.33m, 2.30m, 46.89m);
				AssertFees(declaration, Jul_2015, 25.44m, 3.82m, 17.37m, 2.61m, 49.24m);
				AssertFees(declaration, Jul_2018, 25.44m, 3.82m, 20.36m, 3.05m, 52.67m);
			});
		}

		public void TestEntryFeeForImportFormalSight()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Sight;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			AssertEquals("Precondition: Declaration.IsSight", true, declaration.IsSight);
			CusEntryLine entryLine = declaration.CusEntryHeader.MergedLines.AddNew();
			entryLine.DutyAmount = 100.00m;
			entryLine.GSTAmount = 100.00m;

			CombineAssertions(() =>
			{
				AssertFees(declaration, Jul_2008, 0.00m, 0.00m, 0.00m, 0.00m, 0.00m);
				AssertFees(declaration, Jul_2010, 0.00m, 0.00m, 0.00m, 0.00m, 0.00m);
				AssertFees(declaration, Oct_2010, 0.00m, 0.00m, 0.00m, 0.00m, 0.00m);
				AssertFees(declaration, Jul_2015, 0.00m, 0.00m, 0.00m, 0.00m, 0.00m);
				AssertFees(declaration, Jul_2018, 0.00m, 0.00m, 0.00m, 0.00m, 0.00m);
			});
		}

		public void TestEntryFeeForImportFormalPrivateImport()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Temporary;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration.OtherInfos.AddNew(HeaderOtherInfoList.Codes.PrivateImportTransactionFee, "");
			AssertEquals("Precondition: Declaration.IsPrivateImport", true, declaration.IsPrivateImport);
			CusEntryLine entryLine = declaration.CusEntryHeader.MergedLines.AddNew();
			entryLine.DutyAmount = 100.00m;
			entryLine.GSTAmount = 100.00m;

			CombineAssertions(() =>
			{
				AssertFees(declaration, Jul_2008, 0.00m, 0.00m, 0.00m, 0.00m, 0.00m);
				AssertFees(declaration, Jul_2010, 25.44m, 3.18m, 11.11m, 1.39m, 41.12m);
				AssertFees(declaration, Oct_2010, 25.44m, 3.82m, 15.33m, 2.30m, 46.89m);
				AssertFees(declaration, Jul_2015, 25.44m, 3.82m, 17.37m, 2.61m, 49.24m);
				AssertFees(declaration, Jul_2018, 25.44m, 3.82m, 20.36m, 3.05m, 52.67m);
			});
		}

		public void TestEntryFeeForImportFormalDutyDeminimus()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Temporary;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			CusEntryLine entryLine = declaration.CusEntryHeader.MergedLines.AddNew();
			entryLine.DutyAmount = 0.00m;
			entryLine.GSTAmount = 0.00m;

			CombineAssertions(() =>
			{
				AssertFees(declaration, Jul_2008, 0.00m, 0.00m, 0.00m, 0.00m, 0.00m);
				AssertFees(declaration, Jul_2010, 25.44m, 3.18m, 11.11m, 1.39m, 41.12m);
				AssertFees(declaration, Oct_2010, 25.44m, 3.82m, 15.33m, 2.30m, 46.89m);
				AssertFees(declaration, Jul_2015, 25.44m, 3.82m, 17.37m, 2.61m, 49.24m);
				AssertFees(declaration, Jul_2018, 25.44m, 3.82m, 20.36m, 3.05m, 52.67m);
			});
		}

		public void TestEntryFeeForImportFormalIPI()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			CusEntryLine entryLine = declaration.CusEntryHeader.MergedLines.AddNew();
			entryLine.DutyAmount = 10.00m;
			entryLine.GSTAmount = 5.00m;

			CombineAssertions(() =>
			{
				AssertFees(declaration, Jul_2008, 0.00m, 0.00m, 0.00m, 0.00m, 0.00m);
				AssertFees(declaration, Jul_2018, 0.00m, 0.00m, 0.00m, 0.00m, 0.00m);
			});
		}

		public void TestDoNotCalculateEntryFeeWhenUnPayable()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			var entryLine = declaration.CusEntryHeader.MergedLines.AddNew();
			entryLine.DutyAmount = 100.00m;
			entryLine.GSTAmount = 100.00m;

			AssertEquals("The declaration is low value, so all fees are zero.", true, declaration.EntryFeeUnPayable);
			AssertFees(declaration, Jul_2018, 0m, 0m, 0m, 0m, 0m);
		}

		void AssertFees(JobDeclaration declaration, ZDate transmitDate, ZDecimal entryFeeValue, ZDecimal entryFeeGST, ZDecimal mafLevyValue, ZDecimal mafLevyGST, ZDecimal totalFeePayable)
		{
			declaration.JE_EDITransmitDate = transmitDate;
			var calculator = new EntryFeeCalculatorForTest(declaration);

			AssertEquals(transmitDate + " EntryFeeValue", entryFeeValue, calculator.EntryFeeValueExposed);
			AssertEquals(transmitDate + " EntryFeeGST", entryFeeGST, calculator.EntryFeeGSTExposed);
			AssertEquals(transmitDate + " MAFLevyValue", mafLevyValue, calculator.MAFLevyValueExposed);
			AssertEquals(transmitDate + " MAFLevyGST", mafLevyGST, calculator.MAFLevyGSTExposed);
			AssertEquals(transmitDate + " TotalFeePayable", totalFeePayable, calculator.TotalFeePayableExposed);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TaxOrFeeTestHelper.SetUp(Factory);
		}

		internal static readonly ZDate Jul_2008 = new ZDate(2008, 07, 01);
		internal static readonly ZDate Jul_2010 = new ZDate(2010, 07, 01);
		internal static readonly ZDate Oct_2010 = new ZDate(2010, 10, 01);
		internal static readonly ZDate Jul_2015 = new ZDate(2015, 07, 01);
		internal static readonly ZDate Jul_2018 = new ZDate(2018, 07, 01);

		internal class EntryFeeCalculatorForTest : EntryFeeCalculator
		{
			public EntryFeeCalculatorForTest(JobDeclaration declaration)
				: base(declaration)
			{
			}

			public ZDecimal EntryFeeValueExposed => EntryFeeValue;
			public ZDecimal EntryFeeGSTExposed => EntryFeeGST;
			public ZDecimal MAFLevyValueExposed => MAFLevyValue;
			public ZDecimal MAFLevyGSTExposed => MAFLevyGST;
			public ZDecimal TotalFeePayableExposed => TotalFeePayable;
		}
	}

	public static class TaxOrFeeTestHelper
	{
		public static void SetUp(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var testTaxOrFee_ICS = helper.CreateTaxOrFee("ICS", 312.8889m, "NZ", new ZDateTime(2001, 1, 1), new ZDateTime(2079, 6, 6), "Inward Cargo Transaction Fee (Sea)");
			var testTaxOrFee_ICA_Before20101001 = helper.CreateTaxOrFee("ICA", 26.6667m, "NZ", new ZDateTime(2001, 1, 1), new ZDateTime(2010, 9, 30), "Inward Cargo Transaction Fee (Air)");
			var testTaxOrFee_ICA = helper.CreateTaxOrFee("ICA", 26.66m, "NZ", new ZDateTime(2010, 10, 1), new ZDateTime(2079, 6, 6), "Inward Cargo Transaction Fee (Air)");
			var testTaxOrFee_OCS = helper.CreateTaxOrFee("OCS", 25.07m, "NZ", new ZDateTime(2001, 1, 1), new ZDateTime(2079, 6, 6), "Outward Cargo Transaction Fee (Sea)");
			var testTaxOrFee_OCA_Before20101001 = helper.CreateTaxOrFee("OCA", 6.6667m, "NZ", new ZDateTime(2001, 1, 1), new ZDateTime(2010, 9, 30), "Outward Cargo Transaction Fee (Air)");
			var testTaxOrFee_OCA = helper.CreateTaxOrFee("OCA", 10.01m, "NZ", new ZDateTime(2010, 10, 1), new ZDateTime(2079, 6, 6), "Outward Cargo Transaction Fee (Air)");
			var testTaxOrFee_ORS_Before20210701 = helper.CreateTaxOrFee("ORS", 25.07m, "NZ", new ZDateTime(2001, 1, 1), new ZDateTime(2021, 7, 1), "Outward Report Transaction Fee (Sea)");
			var testTaxOrFee_ORS = helper.CreateTaxOrFee("ORS", 16.16m, "NZ", new ZDateTime(2021, 7, 1), new ZDateTime(2079, 6, 6), "Outward Report Transaction Fee (Sea)");
			var testTaxOrFee_ORA_Before20210701 = helper.CreateTaxOrFee("ORA", 10.01m, "NZ", new ZDateTime(2001, 1, 1), new ZDateTime(2021, 7, 1), "Outward Report Transaction Fee (Air)");
			var testTaxOrFee_ORA = helper.CreateTaxOrFee("ORA", 12.48m, "NZ", new ZDateTime(2021, 7, 1), new ZDateTime(2079, 6, 6), "Outward Report Transaction Fee (Air)");
			var testTaxOrFee_IET = helper.CreateTaxOrFee("IET", 25.4435m, "NZ", new ZDateTime(2001, 10, 1), new ZDateTime(2079, 6, 6), "Import Entry Transaction Fee (IETF)");
			var testTaxOrFee_BSL_Before20100701 = helper.CreateTaxOrFee("BSL", 3.5556m, "NZ", new ZDateTime(2001, 1, 1), new ZDateTime(2010, 6, 30), "Biosecurity System Entry Levy (BSEL)");
			var testTaxOrFee_BSL_Before20101001 = helper.CreateTaxOrFee("BSL", 11.1111m, "NZ", new ZDateTime(2010, 7, 1), new ZDateTime(2010, 9, 30), "Biosecurity System Entry Levy (BSEL)");
			var testTaxOrFee_BSL_Before20150701 = helper.CreateTaxOrFee("BSL", 15.33m, "NZ", new ZDateTime(2010, 10, 1), new ZDateTime(2015, 6, 30), "Biosecurity System Entry Levy (BSEL)");
			var testTaxOrFee_BSL_Before20180701 = helper.CreateTaxOrFee("BSL", 17.37m, "NZ", new ZDateTime(2015, 7, 1), new ZDateTime(2018, 6, 30), "Biosecurity System Entry Levy (BSEL)");
			var testTaxOrFee_BSL = helper.CreateTaxOrFee("BSL", 20.36m, "NZ", new ZDateTime(2018, 7, 1), new ZDateTime(2079, 6, 6), "Biosecurity System Entry Levy (BSEL)");
			var testTaxOrFee_ESP = helper.CreateTaxOrFee("ESP", 10.4435m, "NZ", new ZDateTime(2001, 10, 1), new ZDateTime(2079, 6, 6), "Export Entry Transaction Fee Secure Export Partners");
			var testTaxOrFee_ENP_Before20101001 = helper.CreateTaxOrFee("ENP", 12.6667m, "NZ", new ZDateTime(2001, 1, 1), new ZDateTime(2010, 9, 30), "Export Entry Transaction Fee Non Secure Export Partners");
			var testTaxOrFee_ENP = helper.CreateTaxOrFee("ENP", 15.6m, "NZ", new ZDateTime(2010, 10, 1), new ZDateTime(2079, 6, 6), "Export Entry Transaction Fee Non Secure Export Partners");
			var testTaxOrFee_GST_Before20101001 = helper.CreateTaxOrFee("GST", 0.1250m, "NZ", new ZDateTime(2001, 1, 1), new ZDateTime(2010, 9, 30), "Goods and Services Tax");
			var testTaxOrFee_GST = helper.CreateTaxOrFee("GST", 0.1500m, "NZ", new ZDateTime(2010, 10, 1), new ZDateTime(2079, 6, 6), "Goods and Services Tax");
			var testTaxOrFee_DEM_Before20191130 = helper.CreateTaxOrFee("DEM", 400m, "NZ", new ZDateTime(2010, 1, 1), new ZDateTime(2019, 11, 30), "Deminimus");
			var testTaxOrFee_DEM_After20191201 = helper.CreateTaxOrFee("DEM", 1000m, "NZ", new ZDateTime(2019, 12, 1), new ZDateTime(2079, 6, 6), "Deminimus");
			var testTaxOrFee_VET = helper.CreateTaxOrFee("EVT", 1000m, "NZ", new ZDateTime(2010, 1, 1), new ZDateTime(2079, 6, 6), "Export Value Threshold");
			var testTaxOrFee_LVT = helper.CreateTaxOrFee("LVT", 1000m, "NZ", new ZDateTime(2010, 1, 1), new ZDateTime(2079, 6, 6), "Low Value");
			factory.Save();
		}
	}
}
