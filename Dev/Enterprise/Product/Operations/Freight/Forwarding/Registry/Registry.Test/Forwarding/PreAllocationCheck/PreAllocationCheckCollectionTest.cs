using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Registry.Testing
{
	[TestedType(typeof(PreAllocationCheckCollection))]
	public class PreAllocationCheckCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<PreAllocationCheckCollection>
	{
		public void TestAllowNew()
		{
			AssertEquals("Must not allow new rows", false, this.Collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			AssertEquals("Must not allow removing rows", false, this.Collection.AllowRemove);
		}

		public void TestGetDefault()
		{
			PreAllocationCheckCollection collection = PreAllocationCheckCollection.GetDefault();
			AssertEquals(5, collection.Count);

			var expectedMeasures = new ZString[] { PreAllocationCheck.Measures.Weight, PreAllocationCheck.Measures.Volume, PreAllocationCheck.Measures.Chargeable, PreAllocationCheck.Measures.ShipmentCount, PreAllocationCheck.Measures.Dimensions };
			AssertContainsExactElementsInAnyOrder(expectedMeasures, collection.Cast<PreAllocationCheck>().Select(check => check.Measure));

			var checksThatAreNotDimensions = collection.Cast<PreAllocationCheck>().Where(check => check.Measure != PreAllocationCheck.Measures.Dimensions);
			var checksThatAreDimensions = collection.Cast<PreAllocationCheck>().Where(check => check.Measure == PreAllocationCheck.Measures.Dimensions);

			AssertContainsExactElementsInAnyOrder(new ZString[] { PreAllocationCheck.Actions.None }, collection.Cast<PreAllocationCheck>().Select(check => check.Action).Distinct());
			AssertContainsExactElementsInAnyOrder(new ZDecimal[] { 0m }, checksThatAreNotDimensions.Select(check => check.Percentage).Distinct());
			AssertContainsExactElementsInAnyOrder(new ZDecimal[] { 100m }, checksThatAreDimensions.Select(check => check.Percentage).Distinct());
		}

		public void TestQuickAccessChecks()
		{
			PreAllocationCheckCollection collection = new PreAllocationCheckCollection();

			PreAllocationCheck check = collection.AddNew();
			check.Measure = PreAllocationCheck.Measures.Weight;
			check.Percentage = 10m;

			check = collection.AddNew();
			check.Measure = PreAllocationCheck.Measures.Volume;
			check.Percentage = 20m;

			check = collection.AddNew();
			check.Measure = PreAllocationCheck.Measures.Chargeable;
			check.Percentage = 30m;

			check = collection.AddNew();
			check.Measure = PreAllocationCheck.Measures.ShipmentCount;
			check.Percentage = 40m;

			check = collection.AddNew();
			check.Measure = PreAllocationCheck.Measures.Dimensions;
			check.Percentage = 50m;

			AssertEquals(10m, collection.Weight.Percentage);
			AssertEquals(20m, collection.Volume.Percentage);
			AssertEquals(30m, collection.Chargeable.Percentage);
			AssertEquals(40m, collection.ShipmentCount.Percentage);
			AssertEquals(50m, collection.Dimensions.Percentage);
		}

		#region Implementation

		protected override PreAllocationCheckCollection GetCollectionToTest()
		{
			return new PreAllocationCheckCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PreAllocationCheck();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected new PreAllocationCheckCollection Collection
		{
			get { return base.Collection; }
		}

		#endregion
	}
}
