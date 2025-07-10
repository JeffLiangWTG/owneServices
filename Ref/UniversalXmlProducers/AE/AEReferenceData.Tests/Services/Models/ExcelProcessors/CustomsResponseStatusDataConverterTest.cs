using CargoWise.RefDbRepo.AEReferenceData.Services;
using NUnit.Framework;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.AEReferenceData.Tests;

[TestFixture]
sealed class CustomsResponseStatusDataConverterTest
{
	[Test]
	public void TestGetExcelColumns()
	{
		var columns = converter.GetExcelColumns();
		Assert.That(columns, Is.EquivalentTo(new[] { "Declaration Status", "Declaration Status Name" }));
	}

	[Test]
	public void TestSheetName()
	{
		Assert.That(converter.SheetName, Is.EqualTo("Declaration Status"));
	}

	[Test]
	public void TestConvertExcelColumns()
	{
		var rows = new List<List<(string columnName, string value)>>
		{
			new List<(string columnName, string value)>
			{
				("Declaration Status", "Code1"),
				("Declaration Status Name", "Description1")
			},
			new List<(string columnName, string value)>
			{
				("Declaration Status", "Code2"),
				("Declaration Status Name", "Description2")
			}
		};

		var customsResponseStatuses = converter.ConvertExcelColumns(rows);
		var csta1 = customsResponseStatuses[0];
		var csta2 = customsResponseStatuses[1];
		Assert.Multiple(() =>
		{
			Assert.That(csta1.DeclarationStatus, Is.EqualTo("Code1"));
			Assert.That(csta1.DeclarationStatusName, Is.EqualTo("Description1"));
			Assert.That(csta2.DeclarationStatus, Is.EqualTo("Code2"));
			Assert.That(csta2.DeclarationStatusName, Is.EqualTo("Description2"));
		});
	}

	[SetUp]
	public void Setup()
	{
		converter = new CustomsResponseStatusDataConverter();
	}
	CustomsResponseStatusDataConverter converter;
}
