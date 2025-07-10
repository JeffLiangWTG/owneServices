using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefPostCodeFilterBusinessObject))]
	sealed class RefPostCodeFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new RefPostCodeFilterBusinessObject();
		}

		public void TestCityTownFilter()
		{
			RefCityTown cityTown1 = Factory.NewWithValidTestData<RefCityTown>();
			RefCityTown cityTown2 = Factory.NewWithValidTestData<RefCityTown>();
			RefCityTown cityTown3 = Factory.NewWithValidTestData<RefCityTown>();

			RefPostCode postCode1 = Factory.NewWithValidTestData<RefPostCode>();
			RefPostCode postCode2 = Factory.NewWithValidTestData<RefPostCode>();
			RefPostCode postCode3 = Factory.NewWithValidTestData<RefPostCode>();

			postCode1.CityTowns.Add(cityTown1);
			postCode1.CityTowns.Add(cityTown2);
			postCode2.CityTowns.Add(cityTown1);

			Factory.Save();

			RefPostCodeFilterBusinessObject cityTown1Filter = new RefPostCodeFilterBusinessObject();
			((ModuleGuidFilter)cityTown1Filter["City/Town"]).Property = cityTown1.PK;
			((ModuleGuidFilter)cityTown1Filter["City/Town"]).IsActive = true;

			RefPostCodeCollection postCodeCollection = new RefPostCodeCollection(Factory, cityTown1Filter.Filter);

			Assert("Filtered for CityTown 1 PostCode, Collection should contain PostCode1", postCodeCollection.Contains(postCode1));
			Assert("Filtered for CityTown 1 PostCode, Collection should contain PostCode2", postCodeCollection.Contains(postCode2));
			Assert("Filtered for CityTown 1 PostCode, Collection should NOT contain PostCode3", !postCodeCollection.Contains(postCode3));

			RefPostCodeFilterBusinessObject cityTown2Filter = new RefPostCodeFilterBusinessObject();
			((ModuleGuidFilter)cityTown2Filter["City/Town"]).Property = cityTown2.PK;
			((ModuleGuidFilter)cityTown2Filter["City/Town"]).IsActive = true;

			postCodeCollection = new RefPostCodeCollection(Factory, cityTown2Filter.Filter);

			Assert("Filtered for CityTown 2 PostCode, Collection should contain PostCode1", postCodeCollection.Contains(postCode1));
			Assert("Filtered for CityTown 2 PostCode, Collection should NOT contain PostCode2", !postCodeCollection.Contains(postCode2));
			Assert("Filtered for CityTown 2 PostCode, Collection should NOT contain PostCode3", !postCodeCollection.Contains(postCode3));

			RefPostCodeFilterBusinessObject cityTown3Filter = new RefPostCodeFilterBusinessObject();
			((ModuleGuidFilter)cityTown3Filter["City/Town"]).Property = cityTown3.PK;
			((ModuleGuidFilter)cityTown3Filter["City/Town"]).IsActive = true;

			postCodeCollection = new RefPostCodeCollection(Factory, cityTown3Filter.Filter);

			Assert("Filtered for CityTown 3 PostCode, Collection should NOT contain PostCode1", !postCodeCollection.Contains(postCode1));
			Assert("Filtered for CityTown 3 PostCode, Collection should NOT contain PostCode2", !postCodeCollection.Contains(postCode2));
			Assert("Filtered for CityTown 3 PostCode, Collection should NOT contain PostCode3", !postCodeCollection.Contains(postCode3));

			RefPostCodeFilterBusinessObject emptyFilter = new RefPostCodeFilterBusinessObject();
			((ModuleGuidFilter)emptyFilter["City/Town"]).Property = ZGuid.Empty;
			((ModuleGuidFilter)emptyFilter["City/Town"]).IsActive = false;

			postCodeCollection = new RefPostCodeCollection(Factory, emptyFilter.Filter);

			Assert("Filtered for CityTown 3 PostCode, Collection should contain PostCode1", postCodeCollection.Contains(postCode1));
			Assert("Filtered for CityTown 3 PostCode, Collection should contain PostCode2", postCodeCollection.Contains(postCode2));
			Assert("Filtered for CityTown 3 PostCode, Collection should contain PostCode3", postCodeCollection.Contains(postCode3));
		}

		public void TestZoneSetFilter()
		{
			RefPostCode postCode1 = Factory.NewWithValidTestData<RefPostCode>();
			RefPostCode postCode2 = Factory.NewWithValidTestData<RefPostCode>();
			RefPostCode postCode3 = Factory.NewWithValidTestData<RefPostCode>();
			RefPostCode postCode4 = Factory.NewWithValidTestData<RefPostCode>();
			RefPostCode postCode5 = Factory.NewWithValidTestData<RefPostCode>();

			postCode1.RK_CityTownPostCode = "519000";
			postCode2.RK_CityTownPostCode = "519001";
			postCode3.RK_CityTownPostCode = "519002";
			postCode4.RK_CityTownPostCode = "519003";
			postCode5.RK_CityTownPostCode = "519009";
			postCode1.RK_RN_NKCountry = "CN";
			postCode2.RK_RN_NKCountry = "CN";
			postCode3.RK_RN_NKCountry = "CN";
			postCode4.RK_RN_NKCountry = "CN";
			postCode5.RK_RN_NKCountry = "CN";

			IRateTransportProvider provider = (IRateTransportProvider)Factory.NewWithValidTestData(ObjectFactory.GetType<IRateTransportProvider>());
			provider.TP_RN_NKCountry = "CN";
			provider.TP_ZoneType = "All";

			var zone = (IRateTransportZone)provider.Zones.AddNew();
			var zoneItem1 = (IRateTransportZoneItem)zone.Items.AddNew();
			zoneItem1.TQ_FromPostCode = postCode1.RK_CityTownPostCode;
			zoneItem1.TQ_ToPostCode = postCode3.RK_CityTownPostCode;

			IRateTransportProvider provider2 = (IRateTransportProvider)Factory.NewWithValidTestData(ObjectFactory.GetType<IRateTransportProvider>());
			provider2.TP_RN_NKCountry = "CN";
			provider2.TP_ZoneType = "RAT";

			var zone2 = (IRateTransportZone)provider2.Zones.AddNew();
			var zoneItem2 = (IRateTransportZoneItem)zone2.Items.AddNew();
			zoneItem2.TQ_FromPostCode = postCode5.RK_CityTownPostCode;

			Factory.Save();

			RefPostCodeFilterBusinessObject zoneSetFilter1 = new RefPostCodeFilterBusinessObject();
			((ModuleGuidFilter)zoneSetFilter1["Transport Zone Membership"]).Property = provider.PK;
			((ModuleGuidFilter)zoneSetFilter1["Transport Zone Membership"]).IsActive = true;
			((ModuleGuidFilter)zoneSetFilter1["Transport Zone Membership"]).SqlComparisonOperator = SQLComparisonOperator.Equal;

			RefPostCodeCollection postCodeCollection = new RefPostCodeCollection(Factory, zoneSetFilter1.Filter);
			Assert("Filtered for zoneSetFilter 1, Collection should contain postCode1", postCodeCollection.Contains(postCode1));
			Assert("Filtered for zoneSetFilter 1, Collection should contain postCode2", postCodeCollection.Contains(postCode2));
			Assert("Filtered for zoneSetFilter 1, Collection should contain postCode3", postCodeCollection.Contains(postCode3));
			Assert("Filtered for zoneSetFilter 1, Collection should NOT contain postCode4", !postCodeCollection.Contains(postCode4));
			Assert("Filtered for zoneSetFilter 1, Collection should NOT contain postCode5", !postCodeCollection.Contains(postCode5));

			RefPostCodeFilterBusinessObject zoneSetFilter2 = new RefPostCodeFilterBusinessObject();
			((ModuleGuidFilter)zoneSetFilter2["Transport Zone Membership"]).Property = provider.PK;
			((ModuleGuidFilter)zoneSetFilter2["Transport Zone Membership"]).IsActive = true;
			((ModuleGuidFilter)zoneSetFilter2["Transport Zone Membership"]).SqlComparisonOperator = SQLComparisonOperator.NotEqual;

			postCodeCollection = new RefPostCodeCollection(Factory, zoneSetFilter2.Filter);
			Assert("Filtered for zoneSetFilter 2, Collection should NOT contain postCode1", !postCodeCollection.Contains(postCode1));
			Assert("Filtered for zoneSetFilter 2, Collection should NOT contain postCode2", !postCodeCollection.Contains(postCode2));
			Assert("Filtered for zoneSetFilter 2, Collection should NOT contain postCode3", !postCodeCollection.Contains(postCode3));
			Assert("Filtered for zoneSetFilter 2, Collection should contain postCode4", postCodeCollection.Contains(postCode4));
			Assert("Filtered for zoneSetFilter 2, Collection should contain postCode5", postCodeCollection.Contains(postCode5));

			RefPostCodeFilterBusinessObject zoneSetFilter3 = new RefPostCodeFilterBusinessObject();
			((ModuleGuidFilter)zoneSetFilter3["Transport Zone Membership"]).Property = ZGuid.Empty;
			((ModuleGuidFilter)zoneSetFilter3["Transport Zone Membership"]).IsActive = true;
			((ModuleGuidFilter)zoneSetFilter3["Transport Zone Membership"]).SqlComparisonOperator = SpecialComparisonOperator.IsBlank;

			postCodeCollection = new RefPostCodeCollection(Factory, zoneSetFilter3.Filter);
			Assert("Filtered for zoneSetFilter 3, Collection should NOT contain postCode1", !postCodeCollection.Contains(postCode1));
			Assert("Filtered for zoneSetFilter 3, Collection should NOT contain postCode2", !postCodeCollection.Contains(postCode2));
			Assert("Filtered for zoneSetFilter 3, Collection should NOT contain postCode3", !postCodeCollection.Contains(postCode3));
			Assert("Filtered for zoneSetFilter 3, Collection should contain postCode4", postCodeCollection.Contains(postCode4));
			Assert("Filtered for zoneSetFilter 3, Collection should NOT contain postCode5", !postCodeCollection.Contains(postCode5));

			RefPostCodeFilterBusinessObject zoneSetFilter4 = new RefPostCodeFilterBusinessObject();
			((ModuleGuidFilter)zoneSetFilter4["Transport Zone Membership"]).Property = ZGuid.Empty;
			((ModuleGuidFilter)zoneSetFilter4["Transport Zone Membership"]).IsActive = true;
			((ModuleGuidFilter)zoneSetFilter4["Transport Zone Membership"]).SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;

			postCodeCollection = new RefPostCodeCollection(Factory, zoneSetFilter4.Filter);
			Assert("Filtered for zoneSetFilter 4, Collection should contain postCode1", postCodeCollection.Contains(postCode1));
			Assert("Filtered for zoneSetFilter 4, Collection should contain postCode2", postCodeCollection.Contains(postCode2));
			Assert("Filtered for zoneSetFilter 4, Collection should contain postCode3", postCodeCollection.Contains(postCode3));
			Assert("Filtered for zoneSetFilter 4, Collection should NOT contain postCode4", !postCodeCollection.Contains(postCode4));
			Assert("Filtered for zoneSetFilter 4, Collection should contain postCode5", postCodeCollection.Contains(postCode5));
		}
	}
}
