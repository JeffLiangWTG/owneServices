using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;
using Enterprise.Rating.Integration;
using NUnit.Framework;

namespace Enterprise.Rating.CarrierConnect.Test
{
	[TestedType(typeof(RateQueryBusinessObject))]
	public class RateQueryBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new RateQueryBusinessObject(new RateQueryDto());
		}

		public void TestRateTypeConversion()
		{
			var rateQuery = new RateQueryDto();
			var rateQueryBizo = new RateQueryBusinessObject(rateQuery);
			rateQuery.RateTypes = new [] { "Forwarding" };
			AssertEquals(RateType.Forwarding, rateQueryBizo.RateType);

			rateQuery.RateTypes = new [] { "Forwarding", "Customs" };
			AssertEquals(RateType.Forwarding | RateType.Customs, rateQueryBizo.RateType);
		}

		public void TestIsContainerized()
		{
			var rateQuery = new RateQueryDto();
			var rateQueryBizo = new RateQueryBusinessObject(rateQuery);

			rateQuery.TransportMode = "SEA";
			rateQuery.ContainerMode = "FCL";
			Assert(rateQueryBizo.IsContainerized);

			rateQuery.ContainerMode = "LCL";
			Assert(!rateQueryBizo.IsContainerized);
		}
	}
}
