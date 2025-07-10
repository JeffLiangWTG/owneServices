using System;
using System.Linq;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	[TestFixture]
	class CsvToItemCodeSetsConverterBaseOnlyTests
	{
		[Test]
		public void TestItemsHandleDataConvertionToString()
		{
			var converter = new CsvToItemCodeSetsConverterTestHelper
			{
				Code1 = "CT1",
				Code2 = null,
				DateAsDateTime = new DateTime(2019, 11, 15, 23, 45, 24, 354),
				DateAsString = "2019-12-25T16:35:24.532",
				IntData = 3425
			};
			var items = converter.Items.ToArray();
			Assert.AreEqual(5, items.Length);
			AssertItem(items[0], "CODE1", CodeSetValueType.@string, "CT1");
			AssertItem(items[1], "CODE2", CodeSetValueType.@string, "");
			AssertItem(items[2], "DATEASDATETIME", CodeSetValueType.dateTime, "2019-11-15T23:45:24.354");
			AssertItem(items[3], "DATEASSTRING", CodeSetValueType.dateTime, "2019-12-25T16:35:24.532");
			AssertItem(items[4], "INTDATA", CodeSetValueType.integer, "3425");
		}

		void AssertItem(IItemCodeSet itemCodeSet, string key, CodeSetValueType valueType, string value)
		{
			Assert.AreEqual(key, itemCodeSet.Key, "Key");
			Assert.AreEqual(valueType, itemCodeSet.ValueType, "ValueType");
			Assert.AreEqual(value, itemCodeSet.Value, "Value");
		}

		class CsvToItemCodeSetsConverterTestHelper : CsvToItemCodeSetsConverter
		{
			[CodeSetName("Code1", CodeSetValueType.@string)]
			public string Code1 { get; set; }

			[CodeSetName("Code2", CodeSetValueType.@string)]
			public string Code2 { get; set; }

			[CodeSetName("DateAsDateTime", CodeSetValueType.dateTime)]
			public DateTime DateAsDateTime { get; set; }

			[CodeSetName("DateAsString", CodeSetValueType.dateTime)]
			public string DateAsString { get; set; }

			[CodeSetName("IntData", CodeSetValueType.integer)]
			public int IntData { get; set; }
		}
	}

	[TestFixture]
	abstract class CsvToItemCodeSetsConverterTests<T>
		where T : CsvToItemCodeSetsConverter
	{
		[Test]
		public void TestItems()
		{
			var converter = GetFullyPopulatedConverter();
			var actualData = string.Join(System.Environment.NewLine, converter.Items.Select(x => string.Join("|", x.Key, x.ValueType, x.Value)));
			Assert.AreEqual(ExpectedResult, actualData);
		}

		protected abstract T GetFullyPopulatedConverter();
		protected abstract string ExpectedResult { get; }
	}
}
