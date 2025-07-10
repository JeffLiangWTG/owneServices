using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(MessageUserControl))]
sealed class MessageUserControlTest : TestCaseWithFactory
{
	record ColumnProperties(string caption, string columnName, int width)
	{
		public static implicit operator ColumnProperties((string caption, string columnName, int width) tuple) => new (tuple.caption, tuple.columnName, tuple.width);
	}

	public void TestEntriesBoundGrid_Columns()
	{
		CombineAssertions(() =>
		{
			AssertColumnProperties(messageUserControl.EntriesBoundGrid, ExpectedColumns_EntriesBoundGrid);
		});
	}

	IEnumerable<ColumnProperties> ExpectedColumns_EntriesBoundGrid
	{
		get
		{
			yield return (null, nameof(CusEntryHeader.Schema.EntryNumber), 100);
			yield return ("Ref. No.", nameof(CusEntryHeader.Schema.CH_BGMReference), 65);
			yield return ("IMP/EXP", nameof(CusEntryHeader.Schema.CH_MessageType), 55);
			yield return ("Type", nameof(CusEntryHeader.Schema.Style), 40);
			yield return ("Description", nameof(CusEntryHeader.Schema.Description), 180);
			yield return ("Procedure", nameof(CusEntryHeader.Schema.Procedure), 60);
			yield return ("Date", nameof(CusEntryHeader.Schema.CH_EntryReleaseDate), 60);
			yield return ("Customs status", nameof(CusEntryHeader.Schema.CH_EntryStatus), 85);
			yield return ("Customs status description", nameof(CusEntryHeader.Schema.EntryHeaderStatusDescription), 145);
			yield return ("Phase Status", nameof(CusEntryHeader.Schema.CH_PhaseStatus), 85);
			yield return ("Phase Status Description", nameof(CusEntryHeader.Schema.PhaseStatusDescription), 145);
			yield return ("Approval ID", nameof(CusEntryHeader.Schema.MovementReferenceNumber), 80);
			yield return ("Pay", nameof(CusEntryHeader.Schema.CH_PaymentMethod), 30);
			yield return ("CIF", nameof(CusEntryHeader.Schema.CIFAmount), 80);
			yield return ("Customs Duty", nameof(CusEntryHeader.Schema.TotalDutyAmount), 82);
			yield return ("Excise duties", nameof(CusEntryHeader.Schema.ExciseDutyAmount), 82);
			yield return ("VAT", nameof(CusEntryHeader.Schema.VatAmount), 82);
			yield return ("Total", nameof(CusEntryHeader.Schema.TotalAmount), 82);
		}
	}

	public void TestEntryLineGrid_Columns()
	{
		CombineAssertions(() =>
		{
			AssertColumnProperties(messageUserControl.FindSingle<ZGrid>("EntryLineGrid"), ExpectedColumns_EntryLineGrid);
		});
	}

	IEnumerable<ColumnProperties> ExpectedColumns_EntryLineGrid
	{
		get
		{
			yield return ("Line No.", nameof(CusEntryLine.CL_LineNumber), 80);
			yield return ("Tariff No", nameof(CusEntryLine.CL_AdValoremTariff), 70);
			yield return ("Ctry of Origin", nameof(CusEntryLine.Schema.CtryOfOrigin), 75);
			yield return ("Procedure", nameof(CusEntryLine.Procedure), 65);
			yield return ("Pref.code", nameof(CusEntryLine.Schema.Preference), 65);
			yield return ("Reduced customs flag", nameof(CusEntryLine.Schema.ReducedCustomsFlag), 120);
			yield return ("Customs Duty", nameof(CusEntryLine.Schema.DutyAmount), 75);
			yield return ("Excise duties", nameof(CusEntryLine.Schema.ExciseDutyAmount), 75);
			yield return ("VAT amount", nameof(CusEntryLine.Schema.VatAmount), 75);
			yield return ("VAT code", nameof(CusEntryLine.Schema.VatCode), 66);
			yield return ("Total", nameof(CusEntryLine.Schema.TotalAmount), 75);
		}
	}

	void AssertColumnProperties(ZGrid grid, IEnumerable<ColumnProperties> expectedColumns)
	{
		var columns = grid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();
		var index = 0;
		foreach (var expectedColumn in expectedColumns)
		{
			AssertColumnProperties(columns[index], index, expectedColumn);
			index++;
		}
	}

	void AssertColumnProperties(ZGridColumnInfo column, int index, ColumnProperties expectedColumn)
	{
		(string caption, string columnName, int width) = expectedColumn;
		AssertEquals($"{caption} column {index} caption", caption, column.CaptionResourceString.Caption);
		AssertEquals($"{caption} column {index} columnName", columnName, column.ColumnName);
		AssertEquals($"{caption} column {index} width", width, column.Width);
	}

	protected override void SetUp()
	{
		base.SetUp();
		messageUserControl = new MessageUserControl();
	}

	protected override void TearDown()
	{
		messageUserControl.Dispose();
		base.TearDown();
	}

	MessageUserControl messageUserControl;
}
