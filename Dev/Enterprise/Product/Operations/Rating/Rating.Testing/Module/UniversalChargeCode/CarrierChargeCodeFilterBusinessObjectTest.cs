using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business.Rating;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using WiseRates.Constants;

namespace Enterprise.Rating.Module.Testing;

[TestedType(typeof(CarrierChargeCodeFilterBusinessObject))]
public class CarrierChargeCodeFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
{
	public void TestModuleFilters()
	{
		var filterBizo = new CarrierChargeCodeFilterBusinessObject();
		filterBizo.QueryObjectType = typeof(CarrierChargeCodeBizo);
		var moduleFilters = filterBizo.ModuleFilters;
		AssertEquals(10, moduleFilters.Count());

		var expectedFilters = new (string description, SchemaColumn column)[]
		{
			("Foreign Code", MappedChargeCodeSchema.UCC_ForeignCode),
			("Foreign Name", MappedChargeCodeSchema.UCC_ForeignName),
			("Carrier", MappedChargeCodeSchema.UCC_Carrier),
			("Rate Provider", MappedChargeCodeSchema.UCC_RateProvider),
			("Code", MappedChargeCodeSchema.UCC_Code),
			("Description", MappedChargeCodeSchema.UCC_Description),
			("Global Charge Code", MappedChargeCodeSchema.UCC_GlobalChargeCode),
			("Global Charge Code Description", MappedChargeCodeSchema.UCC_GlobalChargeCodeDescription),
			("Charge Code", MappedChargeCodeSchema.UCC_LocalChargeCode),
			("Charge Code Description", MappedChargeCodeSchema.UCC_LocalChargeCodeDescription),
		};

		var visibleFilters = new HashSet<string> { "Foreign Code", "Foreign Name" };

		CombineAssertions(() =>
		{
			foreach (var (description, column) in expectedFilters)
			{
				var actualFilter = moduleFilters.SingleOrDefault(x => x.Description == description);
				AssertNotNull($"Expected filter strip '{description}' is loaded", actualFilter);
				AssertEquals(column.Name, actualFilter?.FilterColumn.Name);

				if (visibleFilters.Contains(description))
				{
					AssertEquals($"Filter strip '{description}' has expected visibility", FilterVisibility.AlwaysVisible, actualFilter?.Visibility);
				}
			}
		});
	}

	public void TestRateProviderList()
	{
		var filterBizo = new CarrierChargeCodeFilterBusinessObject();
		filterBizo.QueryObjectType = typeof(CarrierChargeCodeBizo);
		var filter = (ModuleTextFilter)filterBizo.ModuleFilters.Single(x => x.Description == "Rate Provider");

		var expectedProviders = new (string code, string description)[]
		{
			(WRConstants.RateProviders.CargoSphere, WRConstants.RateProviders.Description.CargoSphere),
			(WRConstants.RateProviders.CargoGuide, WRConstants.RateProviders.Description.CargoGuide),
		};

		var actualList = filter.List as CodeDescriptionPairList;
		AssertNotNull(actualList);
		AssertEquals(expectedProviders.Length, actualList.Count);

		foreach (var (code, description) in expectedProviders)
		{
			AssertEquals(description, actualList.GetDescriptionFromCode(code));
		}
	}

	protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new CarrierChargeCodeFilterBusinessObject();
}
