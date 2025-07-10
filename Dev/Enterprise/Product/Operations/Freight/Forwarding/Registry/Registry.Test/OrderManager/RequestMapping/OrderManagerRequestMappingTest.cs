using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Registry.Testing
{
	[TestedType(typeof(OrderManagerRequestMapping))]
	sealed class OrderManagerRequestMappingTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestOrderManagerRequestMapping()
		{
			var collection = new OrderManagerRequestMappingCollection(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
			var config1 = collection.AddNew();
			config1.RequestType = "XXX";
			var config2 = collection.AddNew();
			config2.RequestType = "XXX";
			AssertHasErrorContaining(config2.RequestTypeInfo, OrderManagerRequestMapping.DuplicatedCodesError);

			var config3 = collection.AddNew();
			config3.RequestType = "";
			var config4 = collection.AddNew();
			config4.RequestType = "";
			AssertNoErrorContaining(config4.RequestTypeInfo, OrderManagerRequestMapping.DuplicatedCodesError);
		}

		public void TestRequestDescription()
		{
			var collection = new OrderManagerRequestMappingCollection(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
			var config1 = collection.AddNew();
			config1.Request = RequestMappingCodes.BookedQuantity;
			var config2 = collection.AddNew();
			config2.Request = RequestMappingCodes.CargoDateVsExWorksDate;

			AssertEquals("Booked Quantity vs Order Quantity", config1.RequestDescription);
			AssertEquals("Cargo Available Date vs Ex-Works Date", config2.RequestDescription);
		}

		public void TestOrderManagerRequestMapping_RequestTypeList()
		{
			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			var config = new OrderManagerRequestMapping(fallbackLevel, Factory);
			var list = config.RequestTypeList;

			AssertNotNull(list);
			Assert(list.Count > 0);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return new OrderManagerRequestMapping(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new OrderManagerRequestMapping();
		}

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		#endregion
	}
}
