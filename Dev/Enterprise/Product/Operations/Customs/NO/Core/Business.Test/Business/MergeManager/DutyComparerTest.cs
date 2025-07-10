using CargoWise.Common.Collections;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(DutyComparer))]
sealed class DutyComparerTest : TestCaseWithFactory
{
	public void TestComparison()
	{
		var customsDuty = CreateFee("TL1");
		var agriculturalFee = CreateFee("RT100");
		var exciseDuty1 = CreateFee("AB100");
		var exciseDuty2 = CreateFee("AB200");
		var exciseDuty3 = CreateFee("OL720");
		var vatFee1 = CreateFee("MV1");
		var vatFee2 = CreateFee("MV2");
		var vatFee = CreateFee("MV1");
		var anotherVatFee = CreateFee("MVK");

		CombineAssertions("Assert duty comparison", () =>
		{
			AssertEquals("Customs shows before agricultural fee", -1, DutyComparer.Comparison(customsDuty, agriculturalFee));
			AssertEquals("Customs shows before excises", -1, DutyComparer.Comparison(customsDuty, exciseDuty2));
			AssertEquals("Customs shows before VAT", 1, DutyComparer.Comparison(vatFee1, customsDuty));

			AssertEquals("Agricultural fee shows before other excises", 1, DutyComparer.Comparison(exciseDuty1, agriculturalFee));
			AssertEquals("Agricultural fee shows before VAT", -1, DutyComparer.Comparison(agriculturalFee, vatFee2));

			AssertEquals("Excise duties are ordered alphabetically", -1, DutyComparer.Comparison(exciseDuty1, exciseDuty2));
			AssertEquals("Excise duties are ordered alphabetically", 1, DutyComparer.Comparison(exciseDuty3, exciseDuty2));
			AssertEquals("Excise duties show before VAT", 1, DutyComparer.Comparison(vatFee2, exciseDuty1));
			AssertEquals("Excise duties show before VAT", -1, DutyComparer.Comparison(exciseDuty3, vatFee1));

			var unsortedDuties = new[] { exciseDuty1, customsDuty, exciseDuty3, vatFee1, exciseDuty2, agriculturalFee };
			var sortedDuties = new[] { customsDuty, agriculturalFee, exciseDuty1, exciseDuty2, exciseDuty3, vatFee1 };
			var actualSorting = unsortedDuties.StableSort(DutyComparer.Comparison);
			AssertSequencesEqual("Sorted correctly", sortedDuties, actualSorting);
		});
	}

	CusEntryLineFee CreateFee(string chargeType)
	{
		var fee = entryLine.Fees.AddNew();
		fee.CF_ChargeType = chargeType;
		return fee;
	}

	protected override void SetUp()
	{
		base.SetUp();
		RefCusTaxOrFeeHelper.CreateRefCusTaxOrFeeList(Factory);
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_RX_NKInvoice_Currency = "NOK";
		invoice.InvoiceLines.AddNew();
		var merger = new LineMerger(declaration);
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		merger.DoMerge();
		entryLine = declaration.CustomsEntryHeaders[0].AllEntryLines[0];
	}

	CusEntryLine entryLine;
}
