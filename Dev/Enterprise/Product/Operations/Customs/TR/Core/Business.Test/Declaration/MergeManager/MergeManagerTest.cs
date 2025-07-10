using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using CusEntryHeader = Enterprise.Customs.TR.Business.Declaration.CusEntryHeader;
using LineMerger = Enterprise.Customs.TR.Business.Declaration.LineMerger;

namespace Enterprise.Customs.TR.Business.Testing
{
	class MergeManagerTest : EU.Business.Declaration.Testing.MergeManagerTest
	{
		protected override Type GetLineMergerType() => typeof(LineMerger);

		protected override BaseJobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();

		public void TestDoMergeShouldCreateCharge()
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

			declaration.Factory.Save();

			CusEntryHeader entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.FirstOrDefault();
			var charges = entryHeader.Charges.GetChargeWithThisCode("89");

			CombineAssertions(() =>
			{
				AssertEquals(1, declaration.ActiveEntryHeaders.Count);
				AssertEquals(1, entryHeader.Charges.Count);
				AssertEquals("C1_ChargeType", DeclarationHelper.StampDutyConstants.ChargeType, charges.C1_ChargeType);
				AssertEquals("DescriptionOfChargeType", "Stamp Duty", charges.DescriptionOfChargeType);
				AssertEquals("C1_ChargeAmount", DeclarationHelper.StampDutyConstants.ChargeAmount, charges.C1_ChargeAmount);
				AssertEquals("C1_MethodOfPayment", DeclarationHelper.StampDutyConstants.MethodOfPayment, charges.C1_MethodOfPayment);
				AssertEquals("C1_RateOverrideReasonCode", DeclarationHelper.StampDutyConstants.RateOverrideReasonCode, charges.C1_RateOverrideReasonCode);
				AssertEquals("C1_Source", DeclarationHelper.StampDutyConstants.Source, charges.C1_Source);
			});
		}
	}
}
