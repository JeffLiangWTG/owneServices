using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryHeader))]
	public class CusEntryHeaderTest : Customs.Business.Testing.CusEntryHeaderAbstractTest
	{
		public void TestDeclaration()
		{
			var entry = (CusEntryHeader)GetNewBusinessObject();
			AssertType<JobDeclaration>(entry.Declaration);
		}

		public new void TestWorkflowSupportableBusinessObject()
		{
			Assert("We no longer support workflow on CusEntryHeader", true);
		}

		public void TestExportUnionCharges()
		{
			var dec = Factory.New<JobDeclaration>();
			var entry = dec.CustomsEntryHeaders.AddNew();
			var charges = entry.Charges.AddNew();
			charges.C1_ChargeType = "EXU";
			charges.C1_ChargeAmount = 12.34m;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var dec2 = newFactory.Load<JobDeclaration>(dec.PK);
			var entry2 = dec2.CusEntryHeader;
			var charges2 = entry2.ExportUnionCharges;
			AssertEquals("From DB", 12.34m, charges2.C1_ChargeAmount);
		}

		public void TestExportUnionPayInfo()
		{
			var dec = Factory.New<JobDeclaration>();
			var entry = dec.CustomsEntryHeaders.AddNew();
			var payInfo = entry.EntryPayInfos.AddNew();
			payInfo.C9_PaymentParty = "EXU";
			payInfo.C9_PaymentAmount = 12.34m;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var dec2 = newFactory.Load<JobDeclaration>(dec.PK);
			var entry2 = dec2.CusEntryHeader;
			var payInfo2 = entry2.ExportUnionPayInfo;
			AssertEquals("From DB", 12.34m, payInfo2.C9_PaymentAmount);
		}

		public void TestEntryPayInfos()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			AssertType<CusEntryPayInfoCollection<CusEntryPayInfo>>(entryHeader.EntryPayInfos);
		}

		public void TestMessages()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.Messages.AddNew();
			AssertNotNull(cusEntryHeader.Messages[0]);
		}

		public void TestIMessageAttacheeMembers()
		{
			var declaration = Factory.New<JobDeclaration>();
			var branch = declaration.Company.Branches.AddNew();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_DeclarationReference = "JE001";
			declaration.JE_GB = branch.PK;
			cusEntryHeader.CH_Status = LogicalStatusList.Codes.Accepted;
			cusEntryHeader.CH_BGMReference = "LRN9998889991";
			cusEntryHeader.Messages.AddNew();
			var iMessageAttachee = cusEntryHeader as IMessageAttachee;

			CombineAssertions(() =>
			{
				AssertNotNull(cusEntryHeader.Messages[0]);
				AssertEquals("Branch", declaration.JE_GB, iMessageAttachee.GlobalBranchPK);
				AssertEquals("MessageStatus", LogicalStatusList.Codes.Accepted, iMessageAttachee.MessageStatus);
				AssertEquals("JE_DeclarationReference", "LRN9998889991", iMessageAttachee.JobReference);
			});
		}

		public void TestICommonGoodsItemsIntegratorProvider_CommonGoodsItemsIntegrator()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			AssertType<EU.Business.Declaration.EuCommonGoodsItemsIntegrator>(entryHeader.CommonGoodsItemsIntegrator);
		}

		public void TestCPDecs()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();

			var cpDec1 = Factory.New<CusEntryCPDec>();
			var cpDec2 = Factory.New<CusEntryCPDec>();
			cpDec1.ON_ParentID = entryHeader.PK;
			cpDec2.ON_ParentID = entryHeader.PK;

			CombineAssertions("This test is for CPDecs related with CusEntryHeader", () =>
			{
				AssertEquals(2, entryHeader.CPDecCollection.Count);
				AssertEquals(true, entryHeader.CPDecCollection.Contains(cpDec1));
				AssertEquals(true, entryHeader.CPDecCollection.Contains(cpDec2));
			});
		}

		public void TestRegistrationSetter()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();

			cusEntryHeader.RegistrationSetter("22340300IM12345678", new ZDateTime(2023, 1, 1));

			var entryNum = CusEntryNumber.LoadOrCreate(cusEntryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, cusEntryHeader.Declaration.CountryCode);

			CombineAssertions(() =>
			{
				AssertNotNull(cusEntryHeader.CusEntryNumber);
				AssertEquals("CE_EntryNum", "22340300IM12345678", entryNum.CE_EntryNum);
				AssertEquals("CE_IssueDate", new ZDateTime(2023, 1, 1), entryNum.CE_IssueDate);
			});
		}

		public void TestEntryNumberType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.RegistrationSetter("22340300IM12345678", new ZDateTime(2023, 1, 1));
			AssertEquals("EntryNumberType should be MRN", CusEntryNumberTypes.Standard.MovementReferenceNumber, cusEntryHeader.CusEntryNumber.CE_EntryType);
		}

		public void TestCreateStampDutyIfApplicableDoMerge()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("89", 624.10m, Core.Constants.CountryCodes.Turkey, 0.1, 0.1, "OTH", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "Stamp Duty");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var invoices = declaration.Invoices.AddNew();
			invoices.JZ_InvoiceDate = DateTime.Now;
			invoices.JZ_RX_NKInvoice_Currency = "EUR";
			invoices.JZ_InvoiceAmount = 1000m;

			var invoiceLine = invoices.InvoiceLines.AddNew();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();

			CusEntryHeader entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.FirstOrDefault();
			var charges = entryHeader.Charges.GetChargeWithThisCode("89");

			CombineAssertions(() =>
			{
				AssertEquals(1, declaration.ActiveEntryHeaders.Count);
				AssertEquals("C1_ChargeType", DeclarationHelper.StampDutyConstants.ChargeType, charges.C1_ChargeType);
				AssertEquals("DescriptionOfChargeType", "Stamp Duty", charges.DescriptionOfChargeType);
				AssertEquals("C1_ChargeAmount", DeclarationHelper.StampDutyConstants.ChargeAmount, charges.C1_ChargeAmount);
				AssertEquals("C1_MethodOfPayment", DeclarationHelper.StampDutyConstants.MethodOfPayment, charges.C1_MethodOfPayment);
				AssertEquals("C1_RateOverrideReasonCode", DeclarationHelper.StampDutyConstants.RateOverrideReasonCode, charges.C1_RateOverrideReasonCode);
				AssertEquals("C1_Source", DeclarationHelper.StampDutyConstants.Source, charges.C1_Source);
			});
		}

		public void TestCreateStampDutyIfApplicableDoesNotUpdateStampDutyAfterInitialCreation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateRefCusProcedure("TR", "", "22", "11", "", "des", "EXP");
			procedure.ZZ6_IntoTemporaryExport = "N";
			helper.CreateTaxOrFee("89", 624.10m, Core.Constants.CountryCodes.Turkey, 0.1, 0.1, "OTH", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "Stamp Duty");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var invoices = declaration.Invoices.AddNew();
			invoices.JZ_InvoiceDate = DateTime.Now;
			invoices.JZ_RX_NKInvoice_Currency = "EUR";
			invoices.JZ_InvoiceAmount = 1000m;

			var invoiceLine = invoices.InvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "2211";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();

			CusEntryHeader entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.FirstOrDefault();
			var charges = entryHeader.Charges.GetChargeWithThisCode("89");

			CombineAssertions(() =>
			{
				AssertEquals(1, declaration.ActiveEntryHeaders.Count);
				AssertEquals("C1_ChargeType", DeclarationHelper.StampDutyConstants.ChargeType, charges.C1_ChargeType);
				AssertEquals("DescriptionOfChargeType", "Stamp Duty", charges.DescriptionOfChargeType);
				AssertEquals("C1_ChargeAmount", DeclarationHelper.StampDutyConstants.ChargeAmount, charges.C1_ChargeAmount);
				AssertEquals("C1_MethodOfPayment", DeclarationHelper.StampDutyConstants.MethodOfPayment, charges.C1_MethodOfPayment);
				AssertEquals("C1_RateOverrideReasonCode", DeclarationHelper.StampDutyConstants.RateOverrideReasonCode, charges.C1_RateOverrideReasonCode);
				AssertEquals("C1_Source", DeclarationHelper.StampDutyConstants.Source, charges.C1_Source);
			});

			charges.C1_ChargeAmount = 100m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();

			AssertEquals("C1_ChargeAmount", 100m, charges.C1_ChargeAmount);

			invoices.JZ_InvoiceAmount = 990m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();

			AssertEquals("C1_ChargeAmount", 100m, charges.C1_ChargeAmount);

			declaration.CusEntryHeader.Charges.RemoveAndDelete(charges);
			declaration.Factory.Save();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();

			AssertEquals(0, declaration.CusEntryHeader.Charges.Count);
		}

		public void TestCreateStampDutyIfApplicableShouldNotCreateChargesWhenIsExemptFromStampDutyFalse()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateRefCusProcedure("TR", "", "22", "11", "", "des", "EXP");
			procedure.ZZ6_IntoTemporaryExport = "Y";
			helper.CreateTaxOrFee("89", 624.10m, Core.Constants.CountryCodes.Turkey, 0.1, 0.1, "OTH", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "Stamp Duty");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var invoices = declaration.Invoices.AddNew();
			invoices.JZ_InvoiceDate = DateTime.Now;
			invoices.JZ_RX_NKInvoice_Currency = "EUR";
			invoices.JZ_InvoiceAmount = 1000m;

			var invoiceLine = invoices.InvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "2211";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();

			CusEntryHeader entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.FirstOrDefault();
			var charges = entryHeader.Charges.GetChargeWithThisCode("89");

			CombineAssertions(() =>
			{
				AssertEquals(1, declaration.ActiveEntryHeaders.Count);
				AssertNull(charges);
			});
		}

		public void TestCreateStampDutyIfApplicableShouldCreateChargesWhenIsExemptFromStampDutyTrue()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateRefCusProcedure("TR", "", "22", "11", "", "des", "EXP");
			procedure.ZZ6_IntoTemporaryExport = "N";
			helper.CreateTaxOrFee("89", 624.10m, Core.Constants.CountryCodes.Turkey, 0.1, 0.1, "OTH", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "Stamp Duty");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var invoices = declaration.Invoices.AddNew();
			invoices.JZ_InvoiceDate = DateTime.Now;
			invoices.JZ_RX_NKInvoice_Currency = "EUR";
			invoices.JZ_InvoiceAmount = 1000m;

			var invoiceLine = invoices.InvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "2211";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();

			CusEntryHeader entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.FirstOrDefault();
			var charges = entryHeader.Charges.GetChargeWithThisCode("89");

			CombineAssertions(() =>
			{
				AssertEquals(1, declaration.ActiveEntryHeaders.Count);
				AssertEquals("C1_ChargeType", DeclarationHelper.StampDutyConstants.ChargeType, charges.C1_ChargeType);
				AssertEquals("DescriptionOfChargeType", "Stamp Duty", charges.DescriptionOfChargeType);
				AssertEquals("C1_ChargeAmount", DeclarationHelper.StampDutyConstants.ChargeAmount, charges.C1_ChargeAmount);
				AssertEquals("C1_MethodOfPayment", DeclarationHelper.StampDutyConstants.MethodOfPayment, charges.C1_MethodOfPayment);
				AssertEquals("C1_RateOverrideReasonCode", DeclarationHelper.StampDutyConstants.RateOverrideReasonCode, charges.C1_RateOverrideReasonCode);
				AssertEquals("C1_Source", DeclarationHelper.StampDutyConstants.Source, charges.C1_Source);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var header = declaration.CustomsEntryHeaders.AddNew();
			header.Declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			header.PendingDeletionEntryLines.AddNew();
			header.AllEntryLines.Load();
			header.PivotsToContainers.RemoveAndDeleteAll();
			header.PivotsToContainers.GetOrCreatePivotFor(header.Declaration.CusContainers.AddNew());
			return header;
		}

		protected override Type ExpectedChargeCollectionType => typeof(EU.Business.Declaration.CusEntryHeaderChargesCollection<CusEntryHeaderCharges>);

		protected override Type ExpectedChargeType => typeof(CusEntryHeaderCharges);

		protected override BaseJobDeclaration GetNewDeclaration() => Factory.New<JobDeclaration>();
	}
}
