using System.Collections.Generic;
using CargoWise.RefDbRepo.AEReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AEReferenceData.Tests;

[TestFixture]
sealed class AmendCancelResaonDataConverterTest
{
	[Test]
	public void TestGetExcelColumns()
	{
		var columns = converter.GetExcelColumns();
		Assert.That(columns, Is.EquivalentTo(new[] { "ID", "NAME", "Amend", "Cancel", "TransferAmend", "TransferCancel" }));
	}

	[Test]
	public void TestSheetName()
	{
		var iDataConverter = (IDataConverter<AmendCancelReason, RefCusCodeList>)converter;
		Assert.That(iDataConverter.SheetName, Is.EqualTo("Amendment_Cancel Reason"));
	}

	[Test]
	public void TestConvertExcelColumns()
	{
		var rows = new List<List<(string columnName, string value)>> { };

		rows.Add(CreateRow("ID1", "Name1", "YES", "", "", ""));
		rows.Add(CreateRow("ID2", "Name2", "", "YES", "", ""));
		rows.Add(CreateRow("ID3", "Name3", "", "", "YES", ""));
		rows.Add(CreateRow("ID4", "Name4", "", "", "", "YES"));
		rows.Add(CreateRow("ID5", "Name5", "YES", "YES", "YES", "YES"));
		rows.Add(CreateRow("ID6", "Name6", "", "", "", ""));

		var reasons = converter.ConvertExcelColumns(rows);

		Assert.Multiple(() =>
		{
			Assert.That(reasons[0].ID, Is.EqualTo("ID1"));
			Assert.That(reasons[0].Name, Is.EqualTo("Name1"));
			Assert.That(reasons[0].NKCodeTypeList, Is.EquivalentTo(new[] { "AMEND" }));

			Assert.That(reasons[1].ID, Is.EqualTo("ID2"));
			Assert.That(reasons[1].Name, Is.EqualTo("Name2"));
			Assert.That(reasons[1].NKCodeTypeList, Is.EquivalentTo(new[] { "CANCL" }));

			Assert.That(reasons[2].ID, Is.EqualTo("ID3"));
			Assert.That(reasons[2].Name, Is.EqualTo("Name3"));
			Assert.That(reasons[2].NKCodeTypeList, Is.EquivalentTo(new[] { "TRAMD" }));

			Assert.That(reasons[3].ID, Is.EqualTo("ID4"));
			Assert.That(reasons[3].Name, Is.EqualTo("Name4"));
			Assert.That(reasons[3].NKCodeTypeList, Is.EquivalentTo(new[] { "TRCAN" }));

			Assert.That(reasons[4].ID, Is.EqualTo("ID5"));
			Assert.That(reasons[4].Name, Is.EqualTo("Name5"));
			Assert.That(reasons[4].NKCodeTypeList, Is.EquivalentTo(new[] { "AMEND", "CANCL", "TRAMD", "TRCAN" }));

			Assert.That(reasons.Count, Is.EqualTo(5));
		});
	}

	List<(string columnName, string value)> CreateRow(string id, string name, string amend, string cancel, string transferAmend, string transferCancel)
	{
		return new List<(string columnName, string value)>
		{
			("ID", id),
			("NAME", name),
			("Amend", amend),
			("Cancel", cancel),
			("TransferAmend", transferAmend),
			("TransferCancel", transferCancel)
		};
	}

	[SetUp]
	public void Setup()
	{
		converter = new AmendCancelReasonDataConverter();
	}
	AmendCancelReasonDataConverter converter;
}
