using System.Collections.Generic;
using CargoWise.RefDbRepo.AEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AEReferenceData.Tests;

[TestFixture]
sealed class DeclarationPurposeDataConverterTests
{
	[Test]
	public void TestGetExcelColumns()
	{
		var columns = converter.GetExcelColumns();
		Assert.That(columns, Is.EquivalentTo(new[] { "Purpose_ID", "Purpose_Name" }));
	}

	[Test]
	public void TestSheetName()
	{
		Assert.That(converter.SheetName, Is.EqualTo("Declaration Purpose"));
	}

	[Test]
	public void TestConvertExcelColumns()
	{
		var rows = new List<List<(string columnName, string value)>>
		{
			new List<(string columnName, string value)>
			{
				("Purpose_ID", "1"),
				("Purpose_Name", "Name1")
			},
			new List<(string columnName, string value)>
			{
				("Purpose_ID", "2"),
				("Purpose_Name", "Name2")
			}
		};

		var DeclarationPurposes = converter.ConvertExcelColumns(rows);
		var DeclarationPurpose1 = DeclarationPurposes[0];
		var DeclarationPurpose2 = DeclarationPurposes[1];
		Assert.Multiple(() =>
		{
			Assert.That(DeclarationPurpose1.PurposeId, Is.EqualTo("1"));
			Assert.That(DeclarationPurpose1.PurposeName, Is.EqualTo("Name1"));
			Assert.That(DeclarationPurpose2.PurposeId, Is.EqualTo("2"));
			Assert.That(DeclarationPurpose2.PurposeName, Is.EqualTo("Name2"));
		});
	}

	[SetUp]
	public void Setup()
	{
		converter = new DeclarationPurposeDataConverter();
	}
	DeclarationPurposeDataConverter converter;
}

