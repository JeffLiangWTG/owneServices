using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(ReconInterestRateCollection))]
	sealed class ReconInterestRateCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ReconInterestRateCollection>
	{
		public void TestAddNewWithParameters()
		{
			ReconInterestRateCollection collection = new ReconInterestRateCollection();
			ReconInterestRate rate = collection.AddNew(new ZDateTime(2008, 5, 1), new ZDateTime(2008, 6, 1), 6m);
			AssertEquals("StartDate", new ZDateTime(2008, 5, 1), rate.StartDate);
			AssertEquals("EndDate", new ZDateTime(2008, 6, 1), rate.EndDate);
			AssertEquals("Rate", 6m, rate.Rate);
		}

		public void TestAddDefaultValues()
		{
			ReconInterestRateCollection collection = new ReconInterestRateCollection();
			AssertEquals(0, collection.Count);
			collection.AddDefaultValues();
			AssertEquals(35, collection.Count);
			AssertEquals("StartDate", new ZDateTime(2012, 01, 01), collection[0].StartDate);
			AssertEquals("EndDate", new ZDateTime(2012, 03, 31), collection[0].EndDate);
			AssertEquals("Rate", 3m, collection[0].Rate);
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override ReconInterestRateCollection GetCollectionToTest()
		{
			return new ReconInterestRateCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ReconInterestRate();
		}
	}
}
