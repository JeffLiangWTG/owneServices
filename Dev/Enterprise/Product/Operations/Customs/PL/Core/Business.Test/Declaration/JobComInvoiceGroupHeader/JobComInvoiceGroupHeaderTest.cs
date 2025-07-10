using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(JobComInvoiceGroupHeader))]
class JobComInvoiceGroupHeaderTest : EU.Business.Declaration.Testing.JobComInvoiceGroupHeaderTest
{
	#region TestChargesToImportForLandedCosting
	public override void TestChargesToImportForLandedCosting()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_TransportMode = Core.Constants.TransportModes.Air;
		var groupHeader = dec.JobComInvoiceGroupHeaders[0];
		var fobInvoice = groupHeader.JobComInvoiceHeaders.AddNew();
		fobInvoice.JZ_IncoTerm = "FOB";
		fobInvoice.JZ_InvoiceAmount = 10000m;
		fobInvoice.JZ_RX_NKInvoice_Currency = dec.LocalCurrencyCode;

		var groupCharges = groupHeader.Charges;
		groupCharges.AddNew(OverseasFreightCode, 1000m, dec.LocalCurrencyCode).J7_DistributeBy = Common.ChargeDistributeByList.Codes.Value;
		var groupCommission = groupCharges.AddNew(PLCustomsChargeTypeList.Codes.AB, 100m, dec.LocalCurrencyCode);
		groupCommission.J7_DistributeBy = Common.ChargeDistributeByList.Codes.Value;

		dec.ResumeApportionment();
		AssertEquals("FOB Invoice has two charges apportioned", 2, fobInvoice.GroupCharges.Count);
		AssertEquals("First row is 004", OverseasFreightCode, fobInvoice.GroupCharges[0].J7_ChargeType);
		AssertEquals("Apportioned 004 not included in lines", false, fobInvoice.GroupCharges[0].J7_IsIncludedInITOT);

		groupCommission.J7_IsIncludedInITOT = true;
		dec.ResumeApportionment();

		var result = new List<IDefaultLandedCostInput>(((ILandedCostChargeHolder)groupHeader).ChargesToImportForLandedCosting).ToArray();
		AssertEquals("two charges in Result", 0, result.Length);
	}
	#endregion

	protected override CodeDescriptionPairList GetExpectedCustomsChargeTypeList()
	{
		return new PLCustomsChargeTypeList();
	}

	public void TestGroupCharges() => AssertType<GroupInvoiceChargeCollection<GroupInvoiceCharge>>(header.Charges);

	public override void TestChargeTypeList()
	{
		var dec = Factory.New<JobDeclaration>();
		Customs.Business.ICommonInvoice commonInvoice = dec.Invoices.AddNew();

		var chargeTypeList = commonInvoice.ChargeTypeList;
		var customsChargeTypeListForInvoiceHeader = "071, 1ST, 2ST, AB, AD, AE, AF, AG, AH, AI, AJ, AK, AL, AN, BA, BB, BC, BD, BE, BF, BG";
		CombineAssertions(() =>
		{
			AssertEquals("CodesAsString", customsChargeTypeListForInvoiceHeader, chargeTypeList.CodesAsString);
			AssertSame("Cached", chargeTypeList, commonInvoice.ChargeTypeList);
		});
	}

	protected override Type ExpectedTypeOfCharges => typeof(GroupInvoiceChargeCollection<GroupInvoiceCharge>);

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		header = declaration.JobComInvoiceGroupHeaders[0];
	}
	JobComInvoiceGroupHeader header;
}
