using System.ComponentModel;
using CargoWise.ComponentModel;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CusEntryLineFee))]
sealed class CusEntryLineFeeTest : Customs.Business.Testing.CusEntryLineFeeTest
{
	public void Test_DutyCode() => CombineAssertions(() =>
	{
		AssertEntity<CusEntryLineFee>()
			.HasProperty(f => f.CF_DutyCode)
			.WithCaption("Duty Code")
			.WithAttribute<ReadOnlyAttribute>(r => r.IsReadOnly);
	});

	public void Test_Sequence() => CombineAssertions(() =>
	{
		AssertEntity<CusEntryLineFee>()
			.HasProperty(f => f.CF_Sequence)
			.WithCaption("Sequence")
			.WithAttribute<ReadOnlyAttribute>(r => r.IsReadOnly);
	});

	public void Test_RateType() => CombineAssertions(() =>
	{
		AssertEntity<CusEntryLineFee>()
			.HasProperty(f => f.CF_RateType)
			.WithCaption("Rate type")
			.WithAttribute<ReadOnlyAttribute>(r => r.IsReadOnly);
	});

	public void Test_Rate()	=> CombineAssertions(() =>
	{
		AssertEntity<CusEntryLineFee>()
			.HasProperty(f => f.CF_Rate)
			.WithCaption("Rate")
			.WithAttribute<ReadOnlyAttribute>(r => r.IsReadOnly)
			.WithAttribute<DecimalPlacesAttribute>(d => d.DecimalPlaces == 4);
	});

	public void Test_BaseValue() => CombineAssertions(() =>
	{
		AssertEntity<CusEntryLineFee>()
			.HasProperty(f => f.CF_BaseValue)
			.WithCaption("Base value")
			.WithAttribute<ReadOnlyAttribute>(r => r.IsReadOnly)
			.WithAttribute<DecimalPlacesAttribute>(d => d.DecimalPlaces == 3);
	});

	public void Test_DutyAmount() => CombineAssertions(() =>
	{
		AssertEntity<CusEntryLineFee>()
			.HasProperty(f => f.CF_ChargeAmount)
			.WithCaption("Duty amt in NOK")
			.WithAttribute<ReadOnlyAttribute>(r => r.IsReadOnly)
			.WithAttribute<DecimalPlacesAttribute>(d => d.DecimalPlaces == 0);
	});

	public void TestIsLandedCostOnlyAsTextCaptions() => CombineAssertions(() =>
	{
		AssertEntity<CusEntryLineFee>()
			.HasProperty(f => f.IsLandedCostOnlyAsText)
			.WithCaption("Payable to Customs/Tax authorities")
			.WithShortCaption("Payable to");
	});

	public void TestIsLandedCostOnlyAsText()
	{
		CombineAssertions(() =>
		{
			entryLineFee.CF_IsLandedCostOnly = false;
			AssertEquals("IsLandedCostOnly : false", "Customs", entryLineFee.IsLandedCostOnlyAsText);

			entryLineFee.CF_IsLandedCostOnly = true;
			AssertEquals("IsLandedCostOnly : true", "Tax Auth.", entryLineFee.IsLandedCostOnlyAsText);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		var dec = Factory.New<JobDeclaration>();
		var entryHeader = dec.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.AllEntryLines.AddNew();
		entryLineFee = entryLine.Fees.AddNew();
	}

	CusEntryLineFee entryLineFee;
}
