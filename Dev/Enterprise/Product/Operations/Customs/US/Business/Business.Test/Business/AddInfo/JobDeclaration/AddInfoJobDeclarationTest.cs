using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AddInfoJobDeclaration))]
	sealed class AddInfoJobDeclarationTest : AddInfoAbstractTest
	{
		public void TestActuallyBizObj()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertEquals(declaration, declaration.GetAddInfo().ActuallyBizObj);
		}

		public void TestReportUnexpectedProperties()
		{
			var declaration = Factory.New<JobDeclaration>();

			CombineAssertions(() =>
			{
				ErrorReporter.Clear();
				declaration.US_SuretyCode = "1";
				declaration.OnSaving();
				AssertEquals("Setting common property when IsRecon = false should not report error", string.Empty, ErrorReporter.LastMessageReported);

				ErrorReporter.Clear();
				declaration.US_CargoReleaseType = "1";
				declaration.OnSaving();
				AssertEquals("Setting non-recon property when IsRecon = false should not report error", string.Empty, ErrorReporter.LastMessageReported);

				ErrorReporter.Clear();
				declaration.US_ClaimID = "1";
				declaration.OnSaving();
				AssertEquals("Setting recon property when IsRecon = false should report error", $"AddInfo properties [{nameof(declaration.US_ClaimID)}] related to ReconDeclaration should not be set for other types of declarations.", ErrorReporter.LastMessageReported);

				ErrorReporter.Clear();
				declaration.US_ClaimID = "";
				declaration.OnSaving();
				AssertEquals("Setting recon property to default when IsRecon = false should not report error", string.Empty, ErrorReporter.LastMessageReported);

				declaration.ReconDeclaration = new ReconDeclaration(declaration);
				AssertEquals("IsRecon should be true", true, declaration.IsRecon);
				AssertEquals("IsReconMessageType", true, declaration.IsReconMessageType);

				ErrorReporter.Clear();
				declaration.US_SuretyCode = "2";
				declaration.OnSaving();
				AssertEquals("Setting common property when IsRecon = true should not report error", string.Empty, ErrorReporter.LastMessageReported);

				ErrorReporter.Clear();
				declaration.US_CargoReleaseType = "2";
				declaration.OnSaving();
				AssertEquals("Setting non-recon property when IsRecon = true should report error", $"AddInfo properties [{nameof(declaration.US_CargoReleaseType)}] not in ReconDeclaration should not be set when the Declaration is a Recon.", ErrorReporter.LastMessageReported);

				ErrorReporter.Clear();
				declaration.US_CargoReleaseType = "";
				declaration.OnSaving();
				AssertEquals("Setting non-recon property to default when IsRecon = true should not report error", string.Empty, ErrorReporter.LastMessageReported);

				ErrorReporter.Clear();
				declaration.US_ClaimID = "2";
				declaration.OnSaving();
				AssertEquals("Setting recon property when IsRecon = true should not report error", string.Empty, ErrorReporter.LastMessageReported);
			});
		}

		public void TestUS_PaymentDate_ReadOnly()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			Assert(declaration.US_PaymentDateInfo.ReadOnly);
			declaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			Assert(!declaration.US_PaymentDateInfo.ReadOnly);
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporterWithSuffixes;
			Assert(declaration.US_PaymentDateInfo.ReadOnly);
		}

		public void TestUS_BondAmount_ReadOnly()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals("default", true, declaration.US_BondAmountInfo.ReadOnly);
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			AssertEquals("Bond Amount should not be ReadOnly for Continuous Bond if no Continuous Bond for IOR", false, declaration.US_BondAmountInfo.ReadOnly);
			OrgHeader organisation = Factory.New<OrgHeader>();
			declaration.IOROrgPK = organisation.PK;
			CusBondDetail bondData = declaration.IORWrapper.BondDetails.AddNew();
			bondData.PW_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			bondData.PW_ActivityCode = ActivityCodeList.Codes._1;
			bondData.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-1);
			bondData.PW_BondExpiryDate = ZDateTime.Today.AddDays(2);
			bondData.PW_SuretyCode = "555";
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			AssertEquals("Continuous Bond should be ReadOnly", true, declaration.US_BondAmountInfo.ReadOnly);
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			AssertEquals("Single Transaction Bond should be ReadOnly except for manual calc code", true, declaration.US_BondAmountInfo.ReadOnly);
			declaration.US_BondCalcCode = SEBCalculationList.Codes.MAN;
			AssertEquals("Single Transaction Bond should NOT be ReadOnly for MAN calc code", false, declaration.US_BondAmountInfo.ReadOnly);
			declaration.US_BondCalcCode = SEBCalculationList.Codes.DEF;
			AssertEquals("should be ReadOnly again", true, declaration.US_BondAmountInfo.ReadOnly);
			declaration.US_BondCalcCode = SEBCalculationList.Codes.TIB;
			AssertEquals("should be ReadOnly again", true, declaration.US_BondAmountInfo.ReadOnly);
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			AssertEquals("Continuous Bond should be ReadOnly again", true, declaration.US_BondAmountInfo.ReadOnly);
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			AssertEquals("Bond amount should still be ReadOnly for Continuous Bond", true, declaration.US_BondAmountInfo.ReadOnly);
			declaration.US_BondCalcCode = SEBCalculationList.Codes.MAN;
			AssertEquals("Bond amount should still be accessable for Continuous Bond for a TIB entry when BondCalcCode is Manual", false, declaration.US_BondAmountInfo.ReadOnly);
		}

		public void TestOtherBondDetails_ReadOnly()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals("default", false, declaration.US_BondProducerAccNoInfo.ReadOnly);
			AssertEquals("default", false, declaration.US_SuretyCodeInfo.ReadOnly);
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			AssertEquals("Producer Acc No should not be ReadOnly for Continuous Bond if no Continuous Bond for IOR", false, declaration.US_BondProducerAccNoInfo.ReadOnly);
			AssertEquals("Surety Code should not be ReadOnly for Continuous Bond if no Continuous Bond for IOR", false, declaration.US_SuretyCodeInfo.ReadOnly);
			OrgHeader organisation = Factory.New<OrgHeader>();
			declaration.IOROrgPK = organisation.PK;
			CusBondDetail bondData = declaration.IORWrapper.BondDetails.AddNew();
			bondData.PW_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			bondData.PW_ActivityCode = ActivityCodeList.Codes._1;
			bondData.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-1);
			bondData.PW_BondExpiryDate = ZDateTime.Today.AddDays(2);
			bondData.PW_SuretyCode = "555";
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			AssertEquals("Producer Acc No should be ReadOnly for Continuous Bond", true, declaration.US_BondProducerAccNoInfo.ReadOnly);
			AssertEquals("Surety Code should be ReadOnly for Continuous Bond", true, declaration.US_SuretyCodeInfo.ReadOnly);
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			AssertEquals("Producer Acc No should not be ReadOnly for Single Transaction Bond", false, declaration.US_BondProducerAccNoInfo.ReadOnly);
			AssertEquals("Surety Code should not be ReadOnly for Single Transaction Bond", false, declaration.US_SuretyCodeInfo.ReadOnly);
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			AssertEquals("Producer Acc No should be ReadOnly again for Continuous Bond", true, declaration.US_BondProducerAccNoInfo.ReadOnly);
			AssertEquals("Surety Code should be ReadOnly again for Continuous Bond", true, declaration.US_SuretyCodeInfo.ReadOnly);
			declaration.ReconDeclaration = new ReconDeclaration(declaration);
			AssertEquals("default", false, declaration.US_SuretyCodeInfo.ReadOnly);
		}

		public void TestDrawbackSuretyCode_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
			declaration.US_BondType = ZString.Empty;
			Assert("default", declaration.US_SuretyCodeInfo.ReadOnly);
			declaration.US_BondType = BondTypeList.Codes.NoBondRequired;
			Assert("default", declaration.US_SuretyCodeInfo.ReadOnly);
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			Assert("Surety Code should not be ReadOnly for Continuous Bond if no Continuous Bond for Claimant", !declaration.US_SuretyCodeInfo.ReadOnly);
			var organisation = Factory.New<OrgHeader>();
			declaration.JE_OH_Importer = organisation.PK;
			var bondData = declaration.IORWrapper.BondDetails.AddNew();
			bondData.PW_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			bondData.PW_ActivityCode = ActivityCodeList.Codes._1;
			bondData.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-1);
			bondData.PW_BondExpiryDate = ZDateTime.Today.AddDays(2);
			bondData.PW_SuretyCode = "555";
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			Assert("Surety Code should be ReadOnly for Continuous Bond", declaration.US_SuretyCodeInfo.ReadOnly);
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			Assert("Surety Code should not be ReadOnly for Single Transaction Bond", !declaration.US_SuretyCodeInfo.ReadOnly);
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			Assert("Surety Code should be ReadOnly again for Continuous Bond", declaration.US_SuretyCodeInfo.ReadOnly);
		}

		public void TestUS_EntryFilerCodeInfo()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			AssertEquals(false, declaration.US_EntryFilerCodeInfo.ReadOnly);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals(true, declaration.US_EntryFilerCodeInfo.ReadOnly);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_PSC = true;
			AssertEquals(false, declaration.US_EntryFilerCodeInfo.ReadOnly);
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.JE_EntryAuthorisationDate = ZDateTime.Today.AddDays(-1); //entry is lodged by this company in the first place
			declaration.US_PSC = true;
			AssertEquals("If entry is lodged by this company, then entry filer code and entry number should remain readonly", true, declaration.US_EntryFilerCodeInfo.ReadOnly);
		}

		public void TestEnableMessageModesForExWarehouse()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(false, declaration.US_EnableCRLInfo.ReadOnly);
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			AssertEquals(true, declaration.US_EnableCRLInfo.ReadOnly);
		}

		public void TestValidationType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals("IsImport", true, declaration.IsImport);
			AssertEquals("AddInfoJobDeclaration validation type", typeof(FormalImportAddInfoJobDeclarationValidation), declaration.AddInfoValidation.GetType());
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			AssertEquals("AddInfoJobDeclaration validation type", typeof(ACEImportAddInfoJobDeclarationValidation), declaration.AddInfoValidation.GetType());
			declaration.ReconDeclaration = new ReconDeclaration(declaration);
			AssertEquals(true, declaration.IsRecon);
			AssertEquals(typeof(ReconAddInfoJobDeclarationValidation), declaration.AddInfoValidation.GetType());
		}

		public void TestProtestValidationType()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			Protest.Protest protest = new Protest.Protest(declaration);
			AddInfoJobDeclaration addInfoJobDeclaration = new AddInfoJobDeclaration(declaration.JE_AddInfoInfo);
			AssertEquals(true, declaration.IsProtest);
			AssertEquals(typeof(Protest.ProtestAddInfoJobDeclarationValidation), addInfoJobDeclaration.Validation.GetType());
		}

		public void TestValidationTypeForDrawback()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals("IsDrawback", true, declaration.IsDrawback);
			var addInfoJobDeclaration = new AddInfoJobDeclaration(declaration.JE_AddInfoInfo);
			AssertEquals("AddInfoJobDeclaration validation type", typeof(ACSDrawbackAddInfoJobDeclarationValidation), addInfoJobDeclaration.Validation.GetType());
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			AssertEquals("IsACEDrawback", true, declaration.IsACEDrawback);
			AssertEquals("AddInfoJobDeclaration validation type", typeof(ACEDrawbackAddInfoJobDeclarationValidation), addInfoJobDeclaration.Validation.GetType());
		}

		public override void TestIsExport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AddInfoJobDeclaration addInfo = new AddInfoJobDeclaration(declaration.JE_AddInfoInfo);
			AssertEquals("IsExport", true, addInfo.IsExport);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("IsExport", false, addInfo.IsExport);
		}

		public override void TestIsDrawback()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			AddInfoJobDeclaration addInfo = new AddInfoJobDeclaration(declaration.JE_AddInfoInfo);
			Assert("IsDrawback", addInfo.IsDrawback);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert("IsDrawback", !addInfo.IsDrawback);
		}

		[TestDate(2008, 3, 1)]
		public void TestUS_EstimatedEntryDate()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			DefaultStatementPrintDate statementData = new DefaultStatementPrintDate();
			statementData.DoDefaultPrelimStatementPrintDate = true;
			statementData.NumberOfDays = 8;
			USCustomsDataRegistry.Instance.DefaultPrelimStatementPrintDate.SetValue(Guid.Empty, declaration.Branch.PK.ToGuid(), Guid.Empty, statementData);
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			declaration.US_EstimatedEntryDate = new ZDateTime(2008, 3, 7);
			AssertEquals(new ZDateTime(2008, 3, 19), declaration.US_PreliminaryStatementPrintDate);
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Empty;
			declaration.US_EstimatedEntryDate = new ZDateTime(2008, 3, 8); // Saturday
			AssertEquals("Generated Stmt date should be 19th - count starts on Monday (next working day)", new ZDateTime(2008, 3, 19), declaration.US_PreliminaryStatementPrintDate);
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Empty;
			declaration.US_EstimatedEntryDate = new ZDateTime(2008, 3, 9); // Sunday
			AssertEquals("Generated Stmt date should still be 19th - count starts on Monday (next working day)", new ZDateTime(2008, 3, 19), declaration.US_PreliminaryStatementPrintDate);
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Empty;
			declaration.US_EstimatedEntryDate = new ZDateTime(2008, 3, 10); // Monday
			AssertEquals("Generated Stmt date should still be 20th count starts on Tuesday (next working day)", new ZDateTime(2008, 3, 20), declaration.US_PreliminaryStatementPrintDate);
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Empty;
			declaration.US_EstimatedEntryDate = new ZDateTime(2008, 3, 11);
			AssertEquals(new ZDateTime(2008, 3, 21), declaration.US_PreliminaryStatementPrintDate);
			declaration.US_EstimatedEntryDate = ZDateTime.Empty;
			AssertEquals("If entry date is removed, PSD should re-calculate based on current date", new ZDateTime(2008, 3, 12), declaration.US_PreliminaryStatementPrintDate);
			// Test with hard coded Public Holidays
			// (2009, 09, 07)); Monday
			// (2009, 10, 12)); Monday
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Empty;
			declaration.US_EstimatedEntryDate = new ZDateTime(2009, 10, 11); // Sunday
			AssertEquals(new ZDateTime(2009, 10, 22), declaration.US_PreliminaryStatementPrintDate);
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Empty;
			declaration.US_EstimatedEntryDate = new ZDateTime(2009, 9, 5); // Saturday
			AssertEquals(new ZDateTime(2009, 9, 17), declaration.US_PreliminaryStatementPrintDate);
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Empty;
			declaration.US_EstimatedEntryDate = new ZDateTime(2009, 9, 6); // Sunday
			AssertEquals("Generated Stmt date should still be 17th", new ZDateTime(2009, 9, 17), declaration.US_PreliminaryStatementPrintDate);
		}

		public void TestGenAddOnColumnForReconIssues()
		{
			JobDeclaration declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_NAFTAReconIndicator = true;
			JobDeclaration declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_OtherReconIndicator = ReconIssueCodeList.Codes._9802Recon;
			JobDeclaration declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration3.US_NAFTAReconIndicator = true;
			JobDeclaration declaration4 = Factory.New<JobDeclaration>();
			Factory.Save();
			declaration3.US_NAFTAReconIndicator = false;
			Factory.Save();
			Assert(declaration1.GetSystemDefinedValue<ZBool>(USAddInfoSchema.Constants.US_NAFTAReconIndicator));
			AssertEquals(ReconIssueCodeList.Codes._9802Recon, declaration2.GetSystemDefinedValue<ZString>(USAddInfoSchema.Constants.US_OtherReconIndicator));
			AssertEquals(ZBool.False, declaration3.GetSystemDefinedValue<ZBool>(USAddInfoSchema.Constants.US_NAFTAReconIndicator));
			AssertEquals(ZBool.False, declaration4.GetSystemDefinedValue<ZBool>(USAddInfoSchema.Constants.US_NAFTAReconIndicator));
		}

		public void TestGenAddOnColumnForEntryMode()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryMode = EntryModeList.Codes.RLF;
			Factory.Save();
			AssertEquals(EntryModeList.Codes.RLF, declaration.GetSystemDefinedValue<ZString>(USAddInfoSchema.Constants.US_EntryMode));
		}

		public void TestGenAddOnColumnForUS_SchDExport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_SchDExport = "2704";
			Factory.Save();
			AssertEquals("2704", declaration.GetSystemDefinedValue<ZString>(USAddInfoSchema.Constants.US_SchDExport));
		}

		public void TestGenAddOnColumnForUS_BondProducerAccNo()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_BondProducerAccNo = "1337";
			Factory.Save();
			AssertEquals("1337", declaration.GetSystemDefinedValue<ZString>(USAddInfoSchema.Constants.US_BondProducerAccNo));
		}

		public void TestGenAddOnColumnForUS_DateOfExport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_DateOfExport = ZDateTime.BrettsBirthday;
			Factory.Save();
			AssertEquals(ZDateTime.BrettsBirthday, declaration.GetSystemDefinedValue<ZDateTime>(USAddInfoSchema.Constants.US_DateOfExport));
		}

		public void TestUS_WHSEntryNumberMaxLength()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("declaration.US_WHSEntryNumberInfo.MaxLength", 8, declaration.US_WHSEntryNumberInfo.MaxLength);
		}

		public void TestGenAddOnColumnForUS_RN_NKCountryOfDestination()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_RN_NKCountryOfDestination = "Z!";
			Factory.Save();
			AssertEquals("Z!", declaration.GetSystemDefinedValue<ZString>(USAddInfoSchema.Constants.US_RN_NKCountryOfDestination));
		}

		public void TestGenAddOnColumnForUS_TransportReference()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_TransportReference = "Z!";
			Factory.Save();
			AssertEquals("Z!", declaration.GetSystemDefinedValue<ZString>(USAddInfoSchema.Constants.US_TransportReference));
		}

		public void TestGenAddOnColumnForUS_ALDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.US_ALDate = new ZDateTime(2019, 07, 10);
			Factory.Save();
			AssertEquals(new ZDateTime(2019, 07, 10), declaration.GetSystemDefinedValue<ZDateTime>(USAddInfoSchema.Constants.US_ALDate));
		}

		public void TestGenAddOnColumnForUS_BondType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			Factory.Save();
			AssertEquals(BondTypeList.Codes.ContinuousBond, declaration.GetSystemDefinedValue<ZString>(USAddInfoSchema.Constants.US_BondType));
		}

		public void TestGenAddOnColumnForUS_BondDispositionCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.US_BondDispositionCode = BondDispositionCodeList.Codes.CVB;
			Factory.Save();
			AssertEquals(BondDispositionCodeList.Codes.CVB, declaration.GetSystemDefinedValue<ZString>(USAddInfoSchema.Constants.US_BondDispositionCode));
		}

		public void TestGenAddOnColumnForUS_BondDispositionCode2()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.US_BondDispositionCode2 = BondDispositionCodeList.Codes.CAB;
			Factory.Save();
			AssertEquals(BondDispositionCodeList.Codes.CAB, declaration.GetSystemDefinedValue<ZString>(USAddInfoSchema.Constants.US_BondDispositionCode2));
		}

		public void TestGenAddOnColumnForUS_InsuranceDisposition()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.US_InsuranceDisposition = InsuranceDispositionCodeList.Codes.AcceptedByCBP;
			Factory.Save();
			AssertEquals(InsuranceDispositionCodeList.Codes.AcceptedByCBP, declaration.GetSystemDefinedValue<ZString>(USAddInfoSchema.Constants.US_InsuranceDisposition));
		}

		public void TestGenAddOnColumnForUS_FTZNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.US_FTZNo = "123456";
			Factory.Save();
			AssertEquals("123456", declaration.GetSystemDefinedValue<ZString>(USAddInfoSchema.Constants.US_FTZNo));
		}

		public void TestSetValueForUS_TransactionsRelated()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_TransactionsRelated = YesNoDefaultList.Codes.Yes;
			AssertEquals("US_TransactionsRelated field has value 'Y'.", YesNoDefaultList.Codes.Yes, declaration.US_TransactionsRelated);

			declaration.US_TransactionsRelated = ZString.Empty;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_TransactionsRelated = YesNoDefaultList.Codes.No;
			AssertEquals("US_TransactionsRelated field has value 'N'.", YesNoDefaultList.Codes.No, declaration.US_TransactionsRelated);

			declaration.US_TransactionsRelated = ZString.Empty;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Recon;
			declaration.US_TransactionsRelated = YesNoDefaultList.Codes.Yes;
			AssertEquals("US_TransactionsRelated field shouldn't be set for Recon declaration", ZString.Empty, declaration.US_TransactionsRelated);
		}

		public void TestBondDetailsReadonlyForLowValueEntries()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.LowValue;
			AssertEquals(BondTypeList.Codes.NoBondRequired, declaration.US_BondType);
			AssertEquals(true, declaration.US_BondTypeInfo.ReadOnly);
			AssertEquals(true, declaration.US_SuretyCodeInfo.ReadOnly);
			AssertEquals(true, declaration.US_BondWaiverCodeInfo.ReadOnly);
			AssertEquals(true, declaration.US_BondType2Info.ReadOnly);
			AssertEquals(true, declaration.US_ADDCVDSuretyCodeInfo.ReadOnly);
			AssertEquals(true, declaration.US_BondAmount2Info.ReadOnly);
			AssertEquals(true, declaration.US_BondProducerAccNo2Info.ReadOnly);
			AssertEquals(true, declaration.US_BondDispositionCode2Info.ReadOnly);
			AssertEquals(true, declaration.US_CBPBondNo2Info.ReadOnly);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			AssertEquals(BondTypeList.Codes.NoBondRequired, declaration.US_BondType);
			AssertEquals(false, declaration.US_BondTypeInfo.ReadOnly);
			AssertEquals(false, declaration.US_SuretyCodeInfo.ReadOnly);
			AssertEquals(false, declaration.US_BondWaiverCodeInfo.ReadOnly);
			AssertEquals(false, declaration.US_BondType2Info.ReadOnly);
			AssertEquals(false, declaration.US_ADDCVDSuretyCodeInfo.ReadOnly);
			AssertEquals(false, declaration.US_BondAmount2Info.ReadOnly);
			AssertEquals(false, declaration.US_BondProducerAccNo2Info.ReadOnly);
			AssertEquals(false, declaration.US_BondDispositionCode2Info.ReadOnly);
			AssertEquals(false, declaration.US_CBPBondNo2Info.ReadOnly);
		}

		protected override Type GetExpectedLookupsType() => typeof(AddInfoJobDeclarationLookups);

		protected override Type GetExpectedValidationType() => typeof(ExportAddInfoJobDeclarationValidation);

		protected override BusinessObject GetNewBusinessObject() => Factory.New<JobDeclaration>().GetAddInfo();

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice);
			var officeCode1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2772", "2772 CUSOF", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode1.PK.ToGuid(), Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "EXP");
			Factory.Save();
		}
	}
}
