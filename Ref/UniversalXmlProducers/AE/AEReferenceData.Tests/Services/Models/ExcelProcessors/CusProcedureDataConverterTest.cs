using CargoWise.RefDbRepo.AEReferenceData.Services;
using System.Collections.Generic;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AEReferenceData.Tests;

[TestFixture]
sealed class CusProcedureDataConverterTest
{
	[Test]
	public void TestGetExcelColumns()
	{
		var columns = converter.GetExcelColumns();
		Assert.That(columns, Is.EquivalentTo(new[] { "Short Name", "Declaration Type", "Declaration Type Code" }));
	}

	[Test]
	public void TestSheetName()
	{
		Assert.That(converter.SheetName, Is.EqualTo("Declaration_Type"));
	}

	[Test]
	public void TestConvertExcelColumns()
	{
		var rows = new List<List<(string columnName, string value)>>
		{
			new() {
				("Short Name", "IM1"),
				("Declaration Type", "Name1"),
				("Declaration Type Code", "101")
			},
			new() {
				("Short Name", "TR2"),
				("Declaration Type", "Name2"),
				("Declaration Type Code", "102")
			},
		};

		var cusProcedure = converter.ConvertExcelColumns(rows);
		var cusProcedure1 = cusProcedure[0];
		var cusProcedure2 = cusProcedure[1];
		Assert.Multiple(() =>
		{
			Assert.That(cusProcedure1.ShipmentType, Is.EqualTo("IMP"));
			Assert.That(cusProcedure1.ProcedureCode, Is.EqualTo("IM1"));
			Assert.That(cusProcedure1.DeclarationDescription, Is.EqualTo("Name1"));
			Assert.That(cusProcedure1.IsTransit, Is.EqualTo("N"));
			Assert.That(cusProcedure1.CalculateDuty, Is.EqualTo(true));
			Assert.That(cusProcedure1.Category, Is.EqualTo("IM"));
			Assert.That(cusProcedure1.IntoTemporaryImport, Is.EqualTo("N"));
			Assert.That(cusProcedure2.ShipmentType, Is.EqualTo("TRF"));
			Assert.That(cusProcedure2.ProcedureCode, Is.EqualTo("TR2"));
			Assert.That(cusProcedure2.DeclarationDescription, Is.EqualTo("Name2"));
			Assert.That(cusProcedure2.IsTransit, Is.EqualTo("Y"));
			Assert.That(cusProcedure2.CalculateDuty, Is.EqualTo(false));
			Assert.That(cusProcedure2.Category, Is.EqualTo("TR"));
			Assert.That(cusProcedure2.IntoTemporaryImport, Is.EqualTo("N"));
		});
	}

	[SetUp]
	public void Setup()
	{
		converter = new CusProcedureDataConverter();
	}
	CusProcedureDataConverter converter;
}

