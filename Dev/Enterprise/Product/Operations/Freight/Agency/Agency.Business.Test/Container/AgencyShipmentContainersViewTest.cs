using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyShipmentContainersView))]
	internal class AgencyShipmentContainersViewTest : BusinessObjectCollectionViewTestCase<AgencyShipmentContainersView>
	{
		public void TestPredicate()
		{
			var shipment = Factory.New<AgencyShipment>();
			var collection = new AgencyShipmentContainerDependentCollection(shipment, "FOO");
			var container1 = collection.AddNew();
			container1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			var container2 = collection.AddNew();
			container2.JC_ContainerMode = ZString.Empty;
			var view = new AgencyShipmentContainersView(collection, (container) => container.JC_ContainerMode == Core.Constants.ContainerModes.FCL);
			AssertContainsExactElementsInAnyOrder(new[] { container1 }, view);
			view = new AgencyShipmentContainersView(collection, (container) => container.JC_ContainerMode == ZString.Empty);
			AssertContainsExactElementsInAnyOrder(new[] { container2 }, view);
		}

		#region Totals
		public void TestTotalContainers()
		{
			var shipment = Factory.New<AgencyShipment>();
			var collection = new AgencyShipmentContainerDependentCollection(shipment, ContainerBookedStatus.Codes.Booked);
			var view = new AgencyShipmentContainersView(collection);
			var container1 = collection.AddNew();
			container1.JC_ContainerCount = 1;
			var container2 = collection.AddNew();
			container2.JC_ContainerCount = 2;
			AssertEquals(3, view.TotalContainers);
		}

		public void TestTotalWeightUnit()
		{
			var shipment = Factory.New<AgencyShipment>();
			var collection = new AgencyShipmentContainerDependentCollection(shipment, ContainerBookedStatus.Codes.Booked);
			var view = new AgencyShipmentContainersView(collection);
			shipment.JS_UnitOfWeight = "XX";
			AssertEquals(shipment.RegistryWeightUnit, view.TotalWeightUnit);
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			AssertEquals(Constants.Weight.Kilograms, view.TotalWeightUnit);
			var container1 = collection.AddNew();
			container1.JC_GrossWeightUQ = Constants.Weight.Pounds;
			var container2 = collection.AddNew();
			container2.JC_GrossWeightUQ = "XX";
			AssertEquals(Constants.Weight.Kilograms, view.TotalWeightUnit);
			container2.JC_GrossWeightUQ = Constants.Weight.Pounds;
			AssertEquals(Constants.Weight.Pounds, view.TotalWeightUnit);
		}

		public void TestTotalWeight()
		{
			var shipment = Factory.New<AgencyShipment>();
			var collection = new AgencyShipmentContainerDependentCollection(shipment, ContainerBookedStatus.Codes.Booked);
			var view = new AgencyShipmentContainersView(collection);
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			AssertEquals(Constants.Weight.Kilograms, view.TotalWeightUnit);
			var container1 = collection.AddNew();
			container1.JC_GrossWeight = 100m;
			container1.JC_GrossWeightUQ = Constants.Weight.Pounds;
			var container2 = collection.AddNew();
			container2.JC_GrossWeight = 200m;
			container2.JC_GrossWeightUQ = Constants.Weight.Pounds;
			AssertEquals(Constants.Weight.Pounds, view.TotalWeightUnit);
			AssertEquals(300m, view.TotalWeight);
			container2.JC_GrossWeightUQ = "XX";
			AssertEquals(Constants.Weight.Kilograms, view.TotalWeightUnit);
			AssertEquals(245.359m, view.TotalWeight);
		}

		public void TestTotalWeightInShipmentWeightUnit()
		{
			var shipment = Factory.New<AgencyShipment>();
			var collection = new AgencyShipmentContainerDependentCollection(shipment, ContainerBookedStatus.Codes.Booked);
			var view = new AgencyShipmentContainersView(collection);
			var container1 = collection.AddNew();
			container1.JC_GrossWeight = 100m;
			container1.JC_GrossWeightUQ = Constants.Weight.Pounds;
			var container2 = collection.AddNew();
			container2.JC_GrossWeight = 200m;
			container2.JC_GrossWeightUQ = Constants.Weight.Pounds;
			shipment.JS_UnitOfWeight = Constants.Weight.Pounds;
			AssertEquals(300m, view.TotalWeightInShipmentWeightUnit);
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			AssertEquals(136.078m, view.TotalWeightInShipmentWeightUnit);
			shipment.JS_UnitOfWeight = Constants.Weight.Tonnes;
			AssertEquals(0.136m, view.TotalWeightInShipmentWeightUnit);
		}

		public void TestTotalVolumeUnit()
		{
			var shipment = Factory.New<AgencyShipment>();
			var collection = new AgencyShipmentContainerDependentCollection(shipment, ContainerBookedStatus.Codes.Booked);
			var view = new AgencyShipmentContainersView(collection);
			shipment.JS_UnitOfVolume = "XX";
			AssertEquals(shipment.RegistryVolumeUnit, view.TotalVolumeUnit);
			shipment.JS_UnitOfVolume = Constants.Volume.CubicDecimetres;
			AgencyRegistry.Instance.DefaultBookingVolumeUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Volume.MegaLitre);
			AssertEquals(Constants.Volume.CubicDecimetres, view.TotalVolumeUnit);
			var container1 = collection.AddNew();
			container1.JC_GrossVolumeUQ = Constants.Volume.CubicFeet;
			var container2 = collection.AddNew();
			container2.JC_GrossVolumeUQ = "XX";
			AssertEquals(Constants.Volume.CubicDecimetres, view.TotalVolumeUnit);
			container2.JC_GrossVolumeUQ = Constants.Volume.CubicFeet;
			AssertEquals(Constants.Volume.CubicFeet, view.TotalVolumeUnit);
		}

		public void TestTotalVolume()
		{
			var shipment = Factory.New<AgencyShipment>();
			var collection = new AgencyShipmentContainerDependentCollection(shipment, ContainerBookedStatus.Codes.Booked);
			var view = new AgencyShipmentContainersView(collection);
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			AssertEquals(Constants.Volume.CubicMetres, view.TotalVolumeUnit);
			var container1 = collection.AddNew();
			container1.JC_GrossVolume = 10m;
			container1.JC_GrossVolumeUQ = Constants.Volume.CubicFeet;
			var container2 = collection.AddNew();
			container2.JC_GrossVolume = 20m;
			container2.JC_GrossVolumeUQ = Constants.Volume.CubicFeet;
			AssertEquals(Constants.Volume.CubicFeet, view.TotalVolumeUnit);
			AssertEquals(30m, view.TotalVolume);
			container2.JC_GrossVolumeUQ = "XX";
			AssertEquals(Constants.Volume.CubicMetres, view.TotalVolumeUnit);
			AssertEquals(20.283m, view.TotalVolume);
		}

		public void TestTotalVolumeInShipmentVolumeUnit()
		{
			var shipment = Factory.New<AgencyShipment>();
			var collection = new AgencyShipmentContainerDependentCollection(shipment, ContainerBookedStatus.Codes.Booked);
			var view = new AgencyShipmentContainersView(collection);
			var container1 = collection.AddNew();
			container1.JC_GrossVolume = 10m;
			container1.JC_GrossVolumeUQ = Constants.Volume.CubicFeet;
			var container2 = collection.AddNew();
			container2.JC_GrossVolume = 20m;
			container2.JC_GrossVolumeUQ = Constants.Volume.CubicFeet;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicFeet;
			AssertEquals(30m, view.TotalVolumeInShipmentVolumeUnit);
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			AssertEquals(0.850m, view.TotalVolumeInShipmentVolumeUnit);
			shipment.JS_UnitOfVolume = Constants.Volume.CubicDecimetres;
			AssertEquals(849.505m, view.TotalVolumeInShipmentVolumeUnit);
		}

		public void TestTotalPackagesUnit()
		{
			var shipment = Factory.New<AgencyShipment>();
			var collection = new AgencyShipmentContainerDependentCollection(shipment, ContainerBookedStatus.Codes.Booked);
			var view = new AgencyShipmentContainersView(collection);
			shipment.JS_F3_NKPackType = Constants.PkgUnit.Pallet;
			AssertEquals(Constants.PkgUnit.Pallet, view.TotalPackagesUnit);
			var container1 = collection.AddNew();
			container1.JC_F3_NKPackType = Constants.PkgUnit.Drum;
			var container2 = collection.AddNew();
			container2.JC_F3_NKPackType = Constants.PkgUnit.Drum;
			AssertEquals(Constants.PkgUnit.Drum, view.TotalPackagesUnit);
			container2.JC_F3_NKPackType = Constants.PkgUnit.Dozen;
			AssertEquals(Constants.PkgUnit.Package, view.TotalPackagesUnit);
		}

		#endregion
		#region Implementation
		protected override AgencyShipmentContainersView GetCollectionToTest()
		{
			var shipment = Factory.New<AgencyShipment>();
			var collection = new AgencyShipmentContainerDependentCollection(shipment, "FOO");
			return new AgencyShipmentContainersView(collection);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<AgencyShipmentContainer>();
		}
		#endregion
	}
}
