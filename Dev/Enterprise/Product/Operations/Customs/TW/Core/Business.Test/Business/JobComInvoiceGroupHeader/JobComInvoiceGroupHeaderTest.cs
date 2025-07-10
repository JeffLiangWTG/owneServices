using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using ICommonInvoice = Enterprise.Customs.Business.ICommonInvoice;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(JobComInvoiceGroupHeader))]
	sealed class JobComInvoiceGroupHeaderTest : Customs.Business.Testing.BaseJobComInvoiceGroupHeaderTest
	{
		public void TestTypeDecider()
		{
			Assert("Update BaseJobComInvoiceGroupHeaderTypeDecider to include a decider for this class", Factory.New(typeof(BaseJobComInvoiceGroupHeader)).GetType() == GetExpectedBusinessObjectType());
		}

		public override void TestChargeTypeList()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			ICommonInvoice commonInvoice = dec.JobComInvoiceGroupHeaders[0];
			var chargeTypeList1 = commonInvoice.ChargeTypeList;
			var chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			AssertNullOrEmpty("ChargeTypeList not has 'EXW'", chargeTypeList1.GetDescriptionFromCode("EXW"));
			AssertNullOrEmpty("ChargeTypeList not has 'OTH'", chargeTypeList1.GetDescriptionFromCode("OTH"));
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			chargeTypeList1 = commonInvoice.ChargeTypeList;
			chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			AssertNullOrEmpty("ChargeTypeList not has 'EXW'", chargeTypeList1.GetDescriptionFromCode("EXW"));
			AssertNullOrEmpty("ChargeTypeList not has 'OTH'", chargeTypeList1.GetDescriptionFromCode("OTH"));
		}

		[TestDate(2019, 3, 1, 13, 13, 13)]
		public void TestEffectiveValuationDateCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var subGroup = declaration.JobComInvoiceGroupHeaders[0];
			var invoice1 = subGroup.JobComInvoiceHeaders.AddNew();
			var invoice2 = subGroup.JobComInvoiceHeaders.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			var entry = Factory.New<CusEntryHeader>();
			entry.CH_JE = declaration.PK;
			var entryLine = entry.AllEntryLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;
			var entryInst1 = declaration.CusEntryInstruction;
			entryInst1.CEI_DateForDuty = new ZDateTime(2019, 4, 5);
			entry.CH_CEI_Instruction = entryInst1.PK;
			Factory.Save();
			AssertEquals(new ZDateTime(2019, 4, 5), invoice1.EffectiveValuationDate);
			AssertEquals(new ZDateTime(2019, 4, 5), invoice2.EffectiveValuationDate);
			subGroup = new BusinessObjectFactory().Load<JobComInvoiceGroupHeader>(subGroup.PK);
			AssertEquals("Group's EffectiveValuationDateCore is today", new ZDateTime(2019, 4, 5), subGroup.EffectiveValuationDate);
			entryInst1.CEI_DateForDuty = new ZDateTime(2019, 5, 5);
			Factory.Save();
			AssertEquals("Group's EffectiveValuationDateCore is CEI_DateForDuty when CEI_DateForDuty of declaration.CustomsEntryInstructions is only one", new ZDateTime(2019, 5, 5), subGroup.EffectiveValuationDate);
		}

		public void TestExchangeRateType()
		{
			var decl = Factory.New<JobDeclaration>();
			decl.AutoCreateChargesBasedOnIncoTerm = false;
			var groupHeader = decl.JobComInvoiceGroupHeaders[0];
			decl.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals(ExchangeRateType.Customs, (groupHeader as ICurrencyConverterDataProvider).RateType);
			decl.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals(ExchangeRateType.CustomsSecondary, (groupHeader as ICurrencyConverterDataProvider).RateType);
		}

		public override void TestChargesToImportForLandedCosting()
		{
			BaseJobDeclaration testDec = GetNewDeclarationForTest();
			BaseJobComInvoiceGroupHeader groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceHeader fOBInvoice = groupHeader.JobComInvoiceHeaders.AddNew();
			fOBInvoice.JZ_IncoTerm = "FOB";
			fOBInvoice.JZ_InvoiceAmount = 10000m;
			fOBInvoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;

			var groupCharges = groupHeader.Charges;
			var freightCharge = groupCharges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 1000m, testDec.LocalCurrencyCode);
			freightCharge.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			var deductionCharge = groupCharges.AddNew(CustomsChargeTypeList.Codes.DeductionCharge, 100m, testDec.LocalCurrencyCode);
			deductionCharge.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			BaseJobComInvHeaderCharge groupCommission = groupCharges.AddNew(CustomsChargeTypeList.Codes.Commission, 100m, testDec.LocalCurrencyCode);
			testDec.ResumeApportionment();
			AssertEquals("FOB Invoice has three charges apportioned", 2, fOBInvoice.GroupCharges.Count);
			AssertEquals("First row is OFT", CustomsChargeTypeList.Codes.OverseasFreight, fOBInvoice.GroupCharges[0].J7_ChargeType);
			AssertEquals("Apportioned OFT not included in lines", false, fOBInvoice.GroupCharges[0].J7_IsIncludedInITOT);

			AssertEquals("Second row is DED", CustomsChargeTypeList.Codes.DeductionCharge, fOBInvoice.GroupCharges[1].J7_ChargeType);
			AssertEquals("Apportioned DED included in lines", true, fOBInvoice.GroupCharges[1].J7_IsIncludedInITOT);

			groupCommission.J7_IsIncludedInITOT = true;
			testDec.ResumeApportionment();

			IDefaultLandedCostInput[] result = new List<IDefaultLandedCostInput>(((ILandedCostChargeHolder)groupHeader).ChargesToImportForLandedCosting).ToArray();
			AssertEquals("two charges in Result", 2, result.Length);
			AssertEquals("OFT", true, result[0].ChargeDescription.Contains(OFTChargeDescription));
			AssertEquals("DED should be brought", true, result[1].ChargeDescription.Contains("Deduction (or Discount) from Entry"));
			AssertEquals("DED should be brought as a negative amount", -100m, result[1].AmountToDistribute.Amount);
			AssertEquals("OTH charge should not be brought as this is included in lines", false, result[1].ChargeDescription.Contains(CustomsChargeTypeList.Descriptions.Commission.ToString().ToUpper()));
		}

		protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<GroupInvoiceCharge>);
	}
}
