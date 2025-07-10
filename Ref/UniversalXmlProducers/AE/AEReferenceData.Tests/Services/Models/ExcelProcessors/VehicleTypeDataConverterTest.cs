using System.Collections.Generic;
using CargoWise.RefDbRepo.AEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AEReferenceData.Tests;

[TestFixture]
sealed class VehicleTypeDataConverterTest
{
	[Test]
	public void TestGetExcelColumns()
	{
		var columns = converter.GetExcelColumns();
		Assert.That(columns, Is.EquivalentTo(new[] { "Type code", "Type Name" }));
	}

	[Test]
	public void TestSheetName()
	{
		Assert.That(converter.SheetName, Is.EqualTo("Vehicle Type"));
	}

	[Test]
	public void TestConvertExcelColumns()
	{
		var rows = new List<List<(string columnName, string value)>>
		{
			new() {
				("Type code", "Code1"),
				("Type Name", "Name1")
			},
			new() {
				("Type code", "Code2"),
				("Type Name", "Name2")
			}
		};

		var vehicleTypes = converter.ConvertExcelColumns(rows);
		var vehicleType1 = vehicleTypes[0];
		var vehicleType2 = vehicleTypes[1];
		Assert.Multiple(() =>
		{
			Assert.That(vehicleType1.VehicleTypeCode, Is.EqualTo("Code1"));
			Assert.That(vehicleType1.VehicleTypeDescription, Is.EqualTo("Name1"));
			Assert.That(vehicleType2.VehicleTypeCode, Is.EqualTo("Code2"));
			Assert.That(vehicleType2.VehicleTypeDescription, Is.EqualTo("Name2"));
		});
	}

	[SetUp]
	public void Setup()
	{
		converter = new VehicleTypeDataConverter();
	}
	VehicleTypeDataConverter converter;
}
