using System;
using CargoWise.RefDbRepo.TRReferenceData.Services;
using NUnit.Framework;
using static CargoWise.RefDbRepo.TRReferenceData.Services.RefDataLoader;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.RefDbService
{
	class RefDataLoaderTest
	{
		[Test]
		public async Task GetRefDataAsync()
		{
			var result = await GetRefDataAsync<RefCusTradeGroup>("RefCusTradeGroupUpdate", new[] { "RefCusTradeGroupCountries" },
				("ZZA_ZZZ_NKDataGrouping", RefDataComparisonOperator.Equal, "TR"),
				("ZZA_TradeGroup", RefDataComparisonOperator.Equal, "KOS")
				);

			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual("KOS", result[0].ZZA_TradeGroup);
			Assert.AreEqual("Kosova Tek Taraflı Taviz", result[0].ZZA_Description);
		}

		[Test]
		public async Task GetRefDataAsyncWithoutExpand()
		{
			var result = await GetRefDataAsync<RefCusTradeGroup>("RefCusTradeGroupUpdate",
				("ZZA_ZZZ_NKDataGrouping", RefDataComparisonOperator.Equal, "TR"),
				("ZZA_TradeGroup", RefDataComparisonOperator.Equal, "FAR")
				);

			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual("FAR", result[0].ZZA_TradeGroup);
			Assert.AreEqual("Faroe STA", result[0].ZZA_Description);
		}

		[Test]
		public void GetExpands()
		{
			var expands = new[] { "property1", "property2" };
			var result = RefDataLoader.GetExpands(expands);
			Assert.AreEqual("$expand=property1($expand=property2)", result);

			expands = new[] { "property1" };
			result = RefDataLoader.GetExpands(expands);
			Assert.AreEqual("$expand=property1", result);

			expands = Array.Empty<string>();
			result = RefDataLoader.GetExpands(expands);
			Assert.AreEqual("", result);
		}

		[Test]
		public void GetFiltersString()
		{
			var filters = new[]	{ (FieldName: "name", ComparisonOperator: RefDataComparisonOperator.Equal, ComparisonValue: (object)"value") };
			var result = RefDataLoader.GetFiltersString(filters);

			Assert.AreEqual("$filter=name eq 'value'", result);

			filters = new[] { (FieldName: "country", ComparisonOperator: RefDataComparisonOperator.In, ComparisonValue: "DE"),
							  (FieldName: "description", ComparisonOperator: RefDataComparisonOperator.Equal, ComparisonValue: (object)"Germany")};
			result = RefDataLoader.GetFiltersString(filters);

			Assert.AreEqual("$filter=country in 'DE' and description eq 'Germany'", result);

			filters = null;
			result = RefDataLoader.GetFiltersString(filters);
			Assert.AreEqual("", result);
		}

		[Test]
		public void GetFilterOperator()
		{
			var result = RefDataLoader.GetFilterOperator(RefDataComparisonOperator.Equal);
			Assert.AreEqual("eq", result);

			result = RefDataLoader.GetFilterOperator(RefDataComparisonOperator.LessThan);
			Assert.AreEqual("le", result);

			result = RefDataLoader.GetFilterOperator(RefDataComparisonOperator.GreaterThan);
			Assert.AreEqual("ge", result);

			result = RefDataLoader.GetFilterOperator(RefDataComparisonOperator.In);
			Assert.AreEqual("in", result);
		}

		[Test]
		public void ToFilterString()
		{
			var dateTime = new DateTime(2024, 8, 28, 14, 30, 00);
			var result = RefDataLoader.ToFilterString(dateTime);
			Assert.AreEqual("2024-08-28T14:30:00", result);

			var dateTimeOffset = new DateTimeOffset(2024, 8, 28, 14, 30, 00, TimeSpan.Zero);
			result = RefDataLoader.ToFilterString(dateTimeOffset);
			Assert.AreEqual("2024-08-28T14:30:00Z", result);

			var array = new object[] { 1, "aa", true };
			result = RefDataLoader.ToFilterString(array);
			Assert.AreEqual("('1','aa','True')", result);

			var input = "Ulukom, WTG!";
			result = RefDataLoader.ToFilterString(input);
			Assert.AreEqual("'Ulukom, WTG!'", result);

			input = null;
			result = RefDataLoader.ToFilterString(input);
			Assert.AreEqual("''", result);
		}
	}
}
