using CargoWise.RefDbRepo.AEReferenceData.Services;
using System.Collections.Generic;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AEReferenceData.Tests;

[TestFixture]
sealed class CustomsOfficesDataConverterTest
{
	[Test]
	public void TestGetExcelColumns()
	{
		var columns = converter.GetExcelColumns();
		Assert.That(columns, Is.EquivalentTo(new[] { "Cus. Location Code", "Location Name", "Location Arabic Name", "GCC Code", "Is Active" }));
	}

	[Test]
	public void TestSheetName()
	{
		Assert.That(converter.SheetName, Is.EqualTo("Customs Location"));
	}

	[Test]
	public void TestConvertExcelColumns()
	{
		var rows = new List<List<(string columnName, string value)>>
		{
			new() {
				("Cus. Location Code", "Code1"),
				("Location Name", "Name1"),
				("Location Arabic Name", "Arabic Name1"),
				("GCC Code", "1"),
				("Is Active", "Yes")
			},
			new() {
				("Cus. Location Code", "Code2"),
				("Location Name", "Name2"),
				("Location Arabic Name", "Arabic Name2"),
				("GCC Code", "2"),
				("Is Active", "No")
			},
		};

		var customsOffices = converter.ConvertExcelColumns(rows);
		var customsOffice1 = customsOffices[0];
		var customsOffice2 = customsOffices[1];
		Assert.Multiple(() =>
		{
			Assert.That(customsOffice1.LocationCode, Is.EqualTo("Code1"));
			Assert.That(customsOffice1.LocationName, Is.EqualTo("Name1"));
			Assert.That(customsOffice1.LocationArabicName, Is.EqualTo("Arabic Name1"));
			Assert.That(customsOffice1.GCCCode, Is.EqualTo("1"));
			Assert.That(customsOffice1.IsActive, Is.EqualTo("Yes"));
			Assert.That(customsOffice2.LocationCode, Is.EqualTo("Code2"));
			Assert.That(customsOffice2.LocationName, Is.EqualTo("Name2"));
			Assert.That(customsOffice2.LocationArabicName, Is.EqualTo("Arabic Name2"));
			Assert.That(customsOffice2.GCCCode, Is.EqualTo("2"));
			Assert.That(customsOffice2.IsActive, Is.EqualTo("No"));
		});
	}

	[SetUp]
	public void Setup()
	{
		converter = new CustomsOfficeDataConverter();
	}
	CustomsOfficeDataConverter converter;
}

