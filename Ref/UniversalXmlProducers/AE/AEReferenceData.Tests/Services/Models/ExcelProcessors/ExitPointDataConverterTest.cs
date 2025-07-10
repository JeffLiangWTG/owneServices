using System.Collections.Generic;
using CargoWise.RefDbRepo.AEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AEReferenceData.Tests;

[TestFixture]
sealed class ExitPointDataConverterTest
{
	[Test]
	public void TestGetExcelColumns()
	{
		var columns = converter.GetExcelColumns();
		Assert.That(columns, Is.EquivalentTo(new[] { "Customs Location Code", "Description" }));
	}

	[Test]
	public void TestSheetName()
	{
		Assert.That(converter.SheetName, Is.EqualTo("Exit Point"));
	}

	[Test]
	public void TestConvertExcelColumns()
	{
		var rows = new List<List<(string columnName, string value)>>
		{
			new List<(string columnName, string value)>
			{
				("Customs Location Code", "Code1"),
				("Description", "Description1")
			},
			new List<(string columnName, string value)>
			{
				("Customs Location Code", "Code2"),
				("Description", "Description2")
			}
		};

		var exitPoints = converter.ConvertExcelColumns(rows);
		var exitPoint1 = exitPoints[0];
		var exitPoint2 = exitPoints[1];
		Assert.Multiple(() =>
		{
			Assert.That(exitPoint1.LocationCode, Is.EqualTo("Code1"));
			Assert.That(exitPoint1.LocationDescription, Is.EqualTo("Description1"));
			Assert.That(exitPoint2.LocationCode, Is.EqualTo("Code2"));
			Assert.That(exitPoint2.LocationDescription, Is.EqualTo("Description2"));
		});
	}

	[SetUp]
	public void Setup()
	{
		converter = new ExitPointDataConverter();
	}
	ExitPointDataConverter converter;
}
