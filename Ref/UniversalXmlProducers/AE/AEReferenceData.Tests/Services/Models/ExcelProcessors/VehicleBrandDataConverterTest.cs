using System.Collections.Generic;
using CargoWise.RefDbRepo.AEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AEReferenceData.Tests;

[TestFixture]
sealed class VehicleBrandDataConverterTest
{
	[Test]
	public void TestGetExcelColumns()
	{
		var columns = converter.GetExcelColumns();
		Assert.That(columns, Is.EquivalentTo(new[] { "Code", "Vehicle Brand" }));
	}

	[Test]
	public void TestSheetName()
	{
		Assert.That(converter.SheetName, Is.EqualTo("Vehicle_brand"));
	}

	[Test]
	public void TestConvertExcelColumns()
	{
		var rows = new List<List<(string columnName, string value)>>
		{
			new() {
				("Code", "Code1"),
				("Vehicle Brand", "Brand1")
			},
			new() {
				("Code", "Code2"),
				("Vehicle Brand", "Brand2")
			}
		};

		var vehicleBrands = converter.ConvertExcelColumns(rows);
		var vehicleBrand1 = vehicleBrands[0];
		var vehicleBrand2 = vehicleBrands[1];
		Assert.Multiple(() =>
		{
			Assert.That(vehicleBrand1.VehicleBrandCode, Is.EqualTo("Code1"));
			Assert.That(vehicleBrand1.VehicleBrandDescription, Is.EqualTo("Brand1"));
			Assert.That(vehicleBrand2.VehicleBrandCode, Is.EqualTo("Code2"));
			Assert.That(vehicleBrand2.VehicleBrandDescription, Is.EqualTo("Brand2"));
		});
	}

	[SetUp]
	public void Setup()
	{
		converter = new VehicleBrandDataConverter();
	}
	VehicleBrandDataConverter converter;
}
