using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CusEntryHeaderFee))]
sealed class CusEntryHeaderFeeTest : NonPersistentBusinessObjectTestCase
{
	public void Test_Duty()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Duty", DataBoundResourceStrings.GetDataForProperty(entryHeaderFee.DutyInfo).Caption);
			AssertEquals(expected: true, entryHeaderFee.DutyInfo.ReadOnly);
		});
	}

	public void Test_Amount()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Amount", DataBoundResourceStrings.GetDataForProperty(entryHeaderFee.AmountInfo).Caption);
			AssertEquals(expected: true, entryHeaderFee.AmountInfo.ReadOnly);
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(CusEntryHeaderFee), nameof(CusEntryHeaderFee.Amount), false, x => x.DecimalPlaces == 0);
		});
	}

	readonly CusEntryHeaderFee entryHeaderFee = new CusEntryHeaderFee();
}
