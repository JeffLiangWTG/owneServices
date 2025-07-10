using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(UNDGCountryReferenceFilterBusinessObject))]
	sealed class UNDGCountryReferenceBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new UNDGCountryReferenceFilterBusinessObject();

		#region Text filters

		public void TestType()
		{
			var countryReference1 = Factory.NewWithValidTestData<UNDGCountryReference>();
			var countryReference2 = Factory.NewWithValidTestData<UNDGCountryReference>();
			var countryReference3 = Factory.NewWithValidTestData<UNDGCountryReference>();

			countryReference1.DCR_Type = "A";
			countryReference2.DCR_Type = "A";
			countryReference3.DCR_Type = "B";

			Factory.Save();

			var filter = new UNDGCountryReferenceFilterBusinessObject();
			((ModuleTextFilter)filter["Type"]).Property = "A";
			((ModuleTextFilter)filter["Type"]).IsActive = true;

			var collection = new UNDGCountryReferenceCollection(Factory, filter.Filter);
			AssertEquals(2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { countryReference1.PK, countryReference2.PK }, collection.Select(t => t.PK));
		}

		#endregion

		#region NKFilter

		public void TestCountry()
		{
			var countryReference1 = Factory.NewWithValidTestData<UNDGCountryReference>();
			var countryReference2 = Factory.NewWithValidTestData<UNDGCountryReference>();
			var countryReference3 = Factory.NewWithValidTestData<UNDGCountryReference>();

			countryReference1.DCR_RN_NKCountry = Core.Constants.CountryCodes.Malaysia;
			countryReference2.DCR_RN_NKCountry = Core.Constants.CountryCodes.Malaysia;
			countryReference2.DCR_Type = "B";
			countryReference3.DCR_RN_NKCountry = Core.Constants.CountryCodes.Australia;

			Factory.Save();

			var filter = new UNDGCountryReferenceFilterBusinessObject();
			((ModuleNkFilter)filter["Country"]).Property = Core.Constants.CountryCodes.Malaysia;
			((ModuleNkFilter)filter["Country"]).IsActive = true;

			var collection = new UNDGCountryReferenceCollection(Factory, filter.Filter);
			AssertEquals(2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { countryReference1.PK, countryReference2.PK }, collection.Select(t => t.PK));
		}

		#endregion
	}
}
