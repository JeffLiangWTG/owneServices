using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(TransportScheduleRailShippingProviderCollection))]
	sealed class TransportScheduleRailShippingProviderCollectionTest : RailShippingProviderCollectionTest
	{
		public void TestFilterIncludeSeaAndAirLine()
		{
			var shippingLine = Factory.New<OrgHeader>();
			shippingLine.OH_IsShippingProvider = true;
			shippingLine.OH_IsShippingLine = true;

			var airLine = Factory.New<OrgHeader>();
			airLine.OH_IsShippingProvider = true;
			airLine.OH_IsAirLine = true;

			var railProvider = Factory.New<OrgHeader>();
			railProvider.OH_IsShippingProvider = true;
			railProvider.OH_IsRailProvider = true;

			var header = Factory.New<OrgHeader>();
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_IsShippingProvider = true;

			var collection = new TransportScheduleRailShippingProviderCollection(Factory);
			collection.Load();

			Assert("Header should not be in list", !collection.Contains(header));
			Assert("Carrier should not be in list", !collection.Contains(carrier));
			Assert("Rail transport provider should be in list", collection.Contains(railProvider));
			Assert("Sea transport provider should be in list", collection.Contains(shippingLine));
			Assert("Air transport provider should be in list", collection.Contains(airLine));
		}

		public void TestGetAllNotificationsWhenAdditionalFilterNotMet()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_IsShippingProvider = true;

			var collection = new TransportScheduleRailShippingProviderCollection(Factory);
			AssertEquals("An Organization selected from here must have Rail/Sea/Air Provider selected.", collection.GetAllNotificationsWhenAdditionalFilterNotMet(carrier));
		}

		protected override RailShippingProviderCollection GetNewRailShippingProviderCollection()
		{
			return new TransportScheduleRailShippingProviderCollection(Factory);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new TransportScheduleRailShippingProviderCollection(Factory, new OrganisationDefaults());
		}
	}
}
