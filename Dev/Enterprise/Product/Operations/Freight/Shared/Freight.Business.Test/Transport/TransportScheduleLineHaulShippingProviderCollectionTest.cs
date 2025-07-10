using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(TransportScheduleLineHaulShippingProviderCollection))]
	sealed class TransportScheduleLineHaulShippingProviderCollectionTest : LineHaulShippingProviderTest
	{
		public void TestFilterIncludeSeaAndAirLine()
		{
			var shippingLine = Factory.New<OrgHeader>();
			shippingLine.OH_IsShippingProvider = true;
			shippingLine.OH_IsShippingLine = true;

			var airLine = Factory.New<OrgHeader>();
			airLine.OH_IsShippingProvider = true;
			airLine.OH_IsAirLine = true;

			var haulLine = Factory.New<OrgHeader>();
			haulLine.OH_IsShippingProvider = true;
			haulLine.OH_IsLineHaulProvider = true;

			var header = Factory.New<OrgHeader>();
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_IsShippingProvider = true;

			var collection = new TransportScheduleLineHaulShippingProviderCollection(Factory);
			collection.Load();

			Assert("Header should not be in list", !collection.Contains(header));
			Assert("Carrier should not be in list", !collection.Contains(carrier));
			Assert("Haul transport provider should be in list", collection.Contains(haulLine));
			Assert("Sea transport provider should be in list", collection.Contains(shippingLine));
			Assert("Air transport provider should be in list", collection.Contains(airLine));
		}

		public void TestGetAllNotificationsWhenAdditionalFilterNotMet()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_IsShippingProvider = true;

			var collection = new TransportScheduleLineHaulShippingProviderCollection(Factory);
			AssertEquals("An Organization selected from here must have Line Haul or Shipping/Air Line selected.", collection.GetAllNotificationsWhenAdditionalFilterNotMet(carrier));
		}

		protected override LineHaulShippingProviderCollection GetNewLineHaulShippingProviderCollection()
		{
			return new TransportScheduleLineHaulShippingProviderCollection(Factory);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new TransportScheduleLineHaulShippingProviderCollection(Factory, new OrganisationDefaults());
		}
	}
}
