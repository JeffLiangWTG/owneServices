using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ImportNonCondensedDeclarationDutyTaxFeeTest : NX5105Commodity_DutyTaxFeeAbstractTest<ImportNonCondensedDeclarationDutyTaxFee>
	{
		protected override ImportNonCondensedDeclarationDutyTaxFee GetDutyTaxFee(CusEntryLine entryLine, JobComInvoiceLine invoiceLine)
		{
			return new ImportNonCondensedDeclarationDutyTaxFee(entryLine, invoiceLine);
		}

		[ExpectNoExceptions]
		public override void TestCommodity_DutyTaxFee()
		{
			var date = new ZDateTime(2020, 01, 01);

			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			Factory.Save();

			var jobDeclaration = Factory.New<JobDeclaration>();

			jobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			jobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			jobDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var entryInstruction = jobDeclaration.CusEntryInstruction;
			entryInstruction.CEI_DateForDuty = date;

			var groupInvoice = jobDeclaration.JobComInvoiceGroupHeaders[0];

			var invoice = groupInvoice.JobComInvoiceHeaders.AddNew();

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_TariffAdditionalCode = "ADD";
			invoiceLine.JI_CusValueConvRatio = 234m;
			invoiceLine.JI_CustomsQuantity = 345m;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_CustomsSecondQuantity = 456m;
			invoiceLine.JI_CustomsSecondUnitQty = "TNE";

			jobDeclaration.ResumeApportionment();
			jobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();

			var entryHeader = jobDeclaration.CustomsEntryHeaders[0];
			var entryLine = invoiceLine.CusEntryLine;
			entryLine.CL_CustomsValue = 567m;

			var duty = entryLine.Fees.AddNew();
			duty.CF_ChargeType = "DTS";
			duty.CF_MethodOfCalculation = "KGM";

			var dutyTaxFee = GetDutyTaxFee(entryLine, invoiceLine);

			CombineAssertions(() =>
			{
				AssertEquals("SpecificTaxBaseQuantity", 345m, dutyTaxFee.SpecificTaxBaseQuantity);
				invoiceLine.JI_CVAfterRecon = 123m;
				AssertEquals("AdValoremTaxBaseAount", 123m, dutyTaxFee.AdValoremTaxBaseAmount);
				AssertEquals("PercentageNumeric", 234m, dutyTaxFee.PercentageNumeric);
				AssertEquals("DutyRegimeCode", "ADD", dutyTaxFee.DutyRegimeCode);
				duty.CF_MethodOfCalculation = "TNE";
				AssertEquals("SpecificTaxBaseQuantity", 456m, dutyTaxFee.SpecificTaxBaseQuantity);
			});
		}

		[ExpectNoExceptions]
		public void TestSpecificTaxBaseQuantity()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_CustomsProfile = "AAA-BBB";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumber;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 3000m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_IncoTerm = "FOB";
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 2000m;
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_PartNo = "PARTNO1";
			invoiceLine.JI_CustomsQuantity = 4;
			invoiceLine.JI_CustomsUnitQty = "PCE";
			invoiceLine.JI_CustomsSecondQuantity = 10;
			invoiceLine.JI_CustomsSecondUnitQty = "KGM";
			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryLine = invoiceLine.CusEntryLine;
			var fee = entryLine.Fees.AddNew();
			fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DTS;
			fee.CF_MethodOfCalculation = "PCE";
			var dutyTaxFee = GetDutyTaxFee(entryLine, invoiceLine);
			NUnit.Framework.Assert.That(dutyTaxFee.SpecificTaxBaseQuantity, NUnit.Framework.Is.EqualTo(4m).Using(CustomComparers.TypeComparison), "DutyTaxFee.SpecificTaxBaseQuantity should be");
			fee.CF_MethodOfCalculation = "KGM";
			NUnit.Framework.Assert.That(dutyTaxFee.SpecificTaxBaseQuantity, NUnit.Framework.Is.EqualTo(10m).Using(CustomComparers.TypeComparison), "DutyTaxFee.SpecificTaxBaseQuantity should be");
		}
	}
}
