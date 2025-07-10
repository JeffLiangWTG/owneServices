using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Common.Utils;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test;

[TestFixture]
[TransactionedTestCase]
class MetaDataConsistencyFixture
{
	[TestCaseSource(nameof(ViewNames))]
	public void ShouldNotChangeMetaData_ForExposedObjects(string viewName)
	{
		var jsonPath = $"MetaData/{viewName}.metadata.json";
		var allLines = File.ReadAllLines(jsonPath);
		var json = string.Join(Environment.NewLine, allLines.Where(line => !line.TrimStart().StartsWith("//")));
		var expected = JsonSerializer.Deserialize<List<ColumnMetaData>>(json);
		var actual = columnMetaDataProvider.GetMetaData(viewName);

		var expectedSorted = expected.OrderBy(c => c.ColumnName).ToList();
		var actualSorted = actual.OrderBy(c => c.ColumnName).ToList();

		Assert.Multiple(() =>
		{
			Assert.That(actualSorted.Count, Is.EqualTo(expectedSorted.Count),
				$"New schema change in RemoteDb result in Column count mismatch against current {viewName}, we need to make sure it's intact for backward compatibility.");
			for (int i = 0; i < expectedSorted.Count; i++)
			{
				Assert.That(actualSorted[i], Is.EqualTo(expectedSorted[i]),
					$"New schema change in RemoteDb result in Column metadata mismatch against current {viewName}, we need to make sure it's intact for backward compatibility.");
			}
		});
	}

	[OneTimeSetUp]
	public void OneTimeSetUp()
	{
		columnMetaDataProvider = new ColumnMetaDataProvider(TestConnectionString.GetAdmin(null),
			TransactionedTestCaseAttribute.GetDbName(DbSchema.RemoteDb));
	}

	[OneTimeTearDown]
	public void OneTimeTearDown()
	{
		columnMetaDataProvider.Dispose();
	}

	ColumnMetaDataProvider columnMetaDataProvider;

	static IEnumerable<string> ViewNames =>
		Directory.GetFiles("MetaData", "*.metadata.json")
			.Select(f => Path.GetFileNameWithoutExtension(f).Replace(".metadata", ""));
}
