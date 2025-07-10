using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(GroupInvoiceCharge))]
class GroupInvoiceChargeTest : EU.Business.Declaration.Testing.GroupInvoiceChargeTest
{
	protected override string GetOverseasFreightChargeCodeForTest() => PLCustomsChargeTypeList.Codes.AK;

	protected override BaseJobDeclaration GetNewDeclarationForTest() => Factory.New<JobDeclaration>();

	public override void TestApportionChargeWithSameChargeTypeWithDifferntKeys()
	{
		var invoice = GroupHeader.JobComInvoiceHeaders.AddNew();
		invoice.JZ_InvoiceAmount = 1000;
		invoice.JZ_RX_NKInvoice_Currency = GroupHeader.JobDeclaration.LocalCurrencyCode;

		GroupCharge.J7_ChargeType = ChargeCodeForTestApportionChargeWithSameChargeTypeWithDifferntKeys;
		GroupCharge.J7_Amount = 100;
		GroupCharge.J7_RX_NKCurrency = GroupHeader.JobDeclaration.LocalCurrencyCode;
		GroupCharge.J7_DistributeBy = ZString.Empty;
		TestDec.ResumeApportionment();
		AssertEquals("PreCondition: Dutiable", true, GroupCharge.J7_IsDutiable);

		var nonDutyFIFT = GroupHeader.Charges.AddNew(ChargeCodeForTestApportionChargeWithSameChargeTypeWithDifferntKeys, 200, GroupHeader.JobDeclaration.LocalCurrencyCode);
		nonDutyFIFT.J7_IsDutiable = false;
		TestDec.ResumeApportionment();
		AssertEquals("Two apportioned Charges", 0, invoice.GroupCharges.Count);
	}

	public override void TestDutiableVATableSTATableFlags()
	{
		var declaration = GetNewDeclarationForTest();
		declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
		var groupInv = declaration.TopGroupInvoice;
		var charge = groupInv.Charges.AddNew();
		charge.J7_ChargeType = "123";

		Assert(!charge.J7_IsStatisticalValueApplicable);
		Assert(!charge.J7_IsDutiable);
		Assert(!charge.J7_IsGSTApplicable);

		charge.J7_IsDutiable = true;
		Assert(!charge.J7_IsStatisticalValueApplicable);
		Assert(charge.J7_IsGSTApplicable);

		charge.J7_IsDutiable = false;
		charge.J7_IsGSTApplicable = false;
		charge.J7_IsStatisticalValueApplicable = false;
		charge.J7_IsStatisticalValueApplicable = true;
		Assert(!charge.J7_IsGSTApplicable);

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		groupInv = declaration.TopGroupInvoice;
		charge = groupInv.Charges.AddNew();
		charge.J7_ChargeType = "123";

		Assert(!charge.J7_IsStatisticalValueApplicable);
		Assert(!charge.J7_IsDutiable);
		Assert(!charge.J7_IsGSTApplicable);

		charge.J7_IsDutiable = true;
		Assert(!charge.J7_IsStatisticalValueApplicable);
		Assert(charge.J7_IsGSTApplicable);

		charge.J7_IsDutiable = false;
		charge.J7_IsGSTApplicable = false;
		charge.J7_IsStatisticalValueApplicable = false;
		charge.J7_IsStatisticalValueApplicable = true;
		Assert(!charge.J7_IsGSTApplicable);
	}

	public override void TestChargePrepaidCollectCommittedToApportionedCharge()
	{
		var invoice1 = GroupHeader.JobComInvoiceHeaders.AddNew();
		invoice1.JZ_IncoTerm = "FOB";
		invoice1.JZ_InvoiceAmount = 1000;
		invoice1.JZ_RX_NKInvoice_Currency = GroupHeader.JobDeclaration.LocalCurrencyCode;

		var invoice2 = GroupHeader.JobComInvoiceHeaders.AddNew();
		invoice2.JZ_IncoTerm = "CIF";
		invoice2.JZ_InvoiceAmount = 1000;
		invoice2.JZ_RX_NKInvoice_Currency = GroupHeader.JobDeclaration.LocalCurrencyCode;

		var oFT = GroupHeader.Charges.AddNew();
		oFT.J7_ChargeType = GetOverseasFreightChargeCodeForTest();
		oFT.J7_Amount = 100;
		oFT.J7_RX_NKCurrency = GroupHeader.JobDeclaration.LocalCurrencyCode;

		TestDec.ResumeApportionment();
		CombineAssertions(() =>
		{
			AssertEquals("PreCondition:OFT is Prepaid", Core.Constants.PaymentType.Prepaid, oFT.J7_PrepaidCollect);
			AssertEquals("0 apportioned Charge", 0, invoice1.GroupCharges.Count);
			AssertEquals("0 apportioned Charge", 0, invoice2.GroupCharges.Count);

			oFT.J7_PrepaidCollect = Core.Constants.PaymentType.Collect;
			TestDec.ResumeApportionment();
			AssertEquals("0 apportioned Charge", 0, invoice2.GroupCharges.Count);
			AssertEquals("0 apportioned Charge", 0, invoice2.GroupCharges.Count);
		});
	}

	protected override string ChargeCodeForTestApportionChargeWithSameChargeTypeWithDifferntKeys => PLCustomsChargeTypeList.Codes.AK;

	protected override ICustomsChargeCode GetOverseasFreightCharge() => new IncoTermAndCustomsChargeFactory().GetCharge(PLCustomsChargeTypeList.Codes.AK);
}
