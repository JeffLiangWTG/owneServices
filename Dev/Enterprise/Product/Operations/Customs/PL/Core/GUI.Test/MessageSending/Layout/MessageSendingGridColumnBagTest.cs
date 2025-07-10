using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(MessageSendingGridColumnBag))]
sealed class MessageSendingGridColumnBagTest : TestCase
{
	public void TestShouldSendCheckBox() => CombineAssertions(() =>
	{
		AssertNotNull(columnBags.ShouldSendCheckBoxColumn);
		var columnInfo = columnBags.ShouldSendCheckBoxColumn.CreateGridColumnInfo() as ZCheckBoxColumnStyleInfo;
		AssertNotNull(columnInfo);
		AssertEquals("ColumnName", "ShouldSend", columnInfo.ColumnName);
		AssertEquals("Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(40), columnInfo.Width);
	});

	public void TestActionDropEdit() => CombineAssertions(() =>
	{
		AssertNotNull(columnBags.ActionDropEditColumnStyle);
		var columnInfo = columnBags.ActionDropEditColumnStyle.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
		AssertNotNull(columnInfo);
		AssertEquals("ColumnName", "Action", columnInfo.ColumnName);
		AssertEquals("Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(40), columnInfo.Width);
		AssertEquals("Mandatory", true, columnInfo.IsMandatory);
	});

	public void TestDeclarationDateDateEdit() => CombineAssertions(() =>
	{
		AssertNotNull(columnBags.DeclarationDateDateEditColumn);
		var columnInfo = columnBags.DeclarationDateDateEditColumn.CreateGridColumnInfo() as ZDateEditColumnStyleInfo;
		AssertNotNull(columnInfo);
		AssertEquals("ColumnName", "DeclarationDate", columnInfo.ColumnName);
		AssertEquals("Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(70), columnInfo.Width);
	});

	public void TestReferenceNumberTextBox() => CombineAssertions(() =>
	{
		AssertNotNull(columnBags.ReferenceNumberTextBoxColumn);
		var columnInfo = columnBags.ReferenceNumberTextBoxColumn.CreateGridColumnInfo() as ZTextBoxColumnStyleInfo;
		AssertNotNull(columnInfo);
		AssertEquals("ColumnName", "LocalReferenceNumber", columnInfo.ColumnName);
		AssertEquals("Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(150), columnInfo.Width);
	});

	public void TestEntryNumberTextBox() => CombineAssertions(() =>
	{
		AssertNotNull(columnBags.EntryNumberTextBoxColumn);
		var columnInfo = columnBags.EntryNumberTextBoxColumn.CreateGridColumnInfo() as ZTextBoxColumnStyleInfo;
		AssertNotNull(columnInfo);
		AssertEquals("ColumnName", "EntryNumber", columnInfo.ColumnName);
		AssertEquals("Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(150), columnInfo.Width);
		AssertEquals("Character Casing", System.Windows.Forms.CharacterCasing.Upper, columnInfo.CharacterCasing);
	});

	public void TestEntryStatusTextBox() => CombineAssertions(() =>
	{
		AssertNotNull(columnBags.EntryStatusTextBoxColumn);
		var columnInfo = columnBags.EntryStatusTextBoxColumn.CreateGridColumnInfo() as ZTextBoxColumnStyleInfo;
		AssertNotNull(columnInfo);
		AssertEquals("ColumnName", "EntryStatus", columnInfo.ColumnName);
		AssertEquals("Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(80), columnInfo.Width);
	});

	public void TestStatusTextBoxColumn() => CombineAssertions(() =>
	{
		AssertNotNull(columnBags.StatusTextBoxColumn);
		var columnInfo = columnBags.StatusTextBoxColumn.CreateGridColumnInfo() as ZTextBoxColumnStyleInfo;
		AssertNotNull(columnInfo);
		AssertEquals("ColumnName", "MessageStatus", columnInfo.ColumnName);
		AssertEquals("Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(80), columnInfo.Width);
	});

	public void TestEntryDescriptionTextBoxColumn() => CombineAssertions(() =>
	{
		AssertNotNull(columnBags.EntryDescriptionTextBoxColumn);
		var columnInfo = columnBags.EntryDescriptionTextBoxColumn.CreateGridColumnInfo() as ZTextBoxColumnStyleInfo;
		AssertNotNull(columnInfo);
		AssertEquals("ColumnName", "EntryDescription", columnInfo.ColumnName);
		AssertEquals("Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(120), columnInfo.Width);
	});

	public void TestResponseMessage()
	{
		AssertNotNull(columnBags.ResponseMessageDropEditColumn);
		var columnInfo = columnBags.ResponseMessageDropEditColumn.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
		AssertNotNull(columnInfo);
		AssertEquals("ColumnName", "ResponseMessage", columnInfo.ColumnName);
		AssertEquals("Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(150), columnInfo.Width);
	}

	readonly MessageSendingGridColumnBag columnBags = MessageSendingGridColumnBag.Instance;
}
