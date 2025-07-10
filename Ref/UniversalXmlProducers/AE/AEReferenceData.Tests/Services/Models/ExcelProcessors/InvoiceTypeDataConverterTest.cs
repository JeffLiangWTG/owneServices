using System.Collections.Generic;
using CargoWise.RefDbRepo.AEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AEReferenceData.Tests;

[TestFixture]
sealed class InvoiceTypeDataConverterTest
{
	[Test]
	public void TestGetExcelColumns()
	{
		var columns = converter.GetExcelColumns();
		Assert.That(columns, Is.EquivalentTo(new[] { "Code", "Invoice type" }));
	}

	[Test]
	public void TestSheetName()
	{
		Assert.That(converter.SheetName, Is.EqualTo("Invoice_type"));
	}

	[Test]
	public void TestConvertExcelColumns()
	{
		var rows = new List<List<(string columnName, string value)>>
		{
			new List<(string columnName, string value)>
			{
				("Code", "Code1"),
				("Invoice type", "Invoice1")
			},
			new List<(string columnName, string value)>
			{
				("Code", "Code2"),
				("Invoice type", "Invoice2")
			}
		};

		var invoiceTypes = converter.ConvertExcelColumns(rows);
		var invoiceType1 = invoiceTypes[0];
		var invoiceType2 = invoiceTypes[1];
		Assert.Multiple(() =>
		{
			Assert.That(invoiceType1.Code, Is.EqualTo("Code1"));
			Assert.That(invoiceType1.Type, Is.EqualTo("Invoice1"));
			Assert.That(invoiceType2.Code, Is.EqualTo("Code2"));
			Assert.That(invoiceType2.Type, Is.EqualTo("Invoice2"));
		});
	}

	[SetUp]
	public void Setup()
	{
		converter = new InvoiceTypeDataConverter();
	}
	InvoiceTypeDataConverter converter;
}
