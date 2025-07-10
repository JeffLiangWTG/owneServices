using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.GUI.Testing;

sealed class SupportingDocumentsGridColumnsBagTest : TestCase
{
	public void TestItemNumberNullableCalcEditColumn() => CombineAssertions(() =>
	{
		AssertNotNull("Column", ColumnsBag.ItemNumberNullableCalcEditColumn);
		var columnInfo = ColumnsBag.ItemNumberNullableCalcEditColumn.CreateGridColumnInfo();
		AssertNotNull("columnInfo", columnInfo);

		AssertType<ZNullableCalcEditColumnStyleInfo>("ColumnStyleInfo type", columnInfo);
		AssertEquals("ColumnName", "CSI_ItemNumber", columnInfo.ColumnName);
		AssertEquals("Width", 80, columnInfo.Width);
	});

	static SupportingDocumentsGridColumnsBag ColumnsBag => SupportingDocumentsGridColumnsBag.Instance;
}
