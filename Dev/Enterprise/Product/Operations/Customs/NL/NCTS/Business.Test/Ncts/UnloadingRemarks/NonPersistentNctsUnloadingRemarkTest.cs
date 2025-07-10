using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(NonPersistentNctsUnloadingRemark))]
sealed class NonPersistentNctsUnloadingRemarkTest : NonPersistentBusinessObjectTestCase
{
	public void TestItemNumber_Attributes()
	{
		_ = AssertEntity<AutoNonPersistentNctsUnloadingRemark>()
			.HasProperty(x => x.ItemNumber)
			.WithCaption("Item Number")
			.WithMediumCaption("Item Num.")
			.WithShortCaption("Item");
	}

	public void TestEoriNumber_Attributes()
	{
		_ = AssertEntity<AutoNonPersistentNctsUnloadingRemark>()
			.HasProperty(x => x.EoriNumber)
			.WithMaxLength(17)
			.WithCaption("EORI Number")
			.WithMediumCaption("EORI Num.")
			.WithShortCaption("EORI");
	}

	public void TestCode_Attributes() => CombineAssertions(() =>
	{
		_ = AssertEntity<NonPersistentNctsUnloadingRemark>()
			.HasProperty(x => x.Code)
			.WithList("Lookups.CodeList");

		_ = AssertEntity<AutoNonPersistentNctsUnloadingRemark>()
			.HasProperty(x => x.Code)
			.WithMaxLength(1)
			.WithCaption("Code")
			.WithMediumCaption("Code")
			.WithShortCaption("Code");
	});

	public void TestNumber_Attributes()
	{
		_ = AssertEntity<AutoNonPersistentNctsUnloadingRemark>()
			.HasProperty(x => x.Number)
			.WithMaxLength(19)
			.WithCaption("Number")
			.WithMediumCaption("Num.")
			.WithShortCaption("#");
	}

	public void TestToFormattedString()
	{
		var remark = new NonPersistentNctsUnloadingRemark(Factory);
		remark.ItemNumber = 0;
		remark.EoriNumber = "NL1234567890";
		remark.Code = "1";
		remark.Number = "123";
		CombineAssertions(() =>
		{
			AssertEquals("ItemNumber 0", "<00;NL1234567890;1;123>", remark.ToFormattedString());
			remark.ItemNumber = 1;
			AssertEquals("ItemNumber 1", "<1;NL1234567890;1;123>", remark.ToFormattedString());
		});
	}
}
