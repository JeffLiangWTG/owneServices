using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyShipmentPackLineAdapter))]
	internal class AgencyShipmentPackLineAdapterTest : BusinessObjectBaseTestCase
	{
		public void TestNonPersistent()
		{
			var packLinesInDatabase = Factory.GetDatabaseCount(typeof(AgencyShipmentPackLine));
			var adapter = Factory.New<AgencyShipmentPackLineAdapter>();
			Factory.Save();
			AssertEquals("should not save adapter", false, adapter.IsInDatabase);
			AssertEquals("should not save adapter", packLinesInDatabase, Factory.GetDatabaseCount(typeof(AgencyShipmentPackLine)));
		}

		public void TestGetProperties()
		{
			var container = Factory.New<AgencyShipmentContainer>();
			container.JC_ContainerNum = "AAAA0000007";
			container.JC_ContainerCount = 2;
			container.JC_Description = "mushrooms";
			container.JC_MarksAndNumbers = "marks & nums";
			container.JC_GrossWeight = 2500m;
			container.JC_GrossWeightUQ = Core.Constants.Weight.Pounds;
			container.JC_GrossVolume = 1.5m;
			container.JC_GrossVolumeUQ = Core.Constants.Volume.CubicFeet;
			container.JC_TotalLength = 1;
			container.JC_TotalWidth = 2;
			container.JC_TotalHeight = 3;
			container.JC_TotalUnitOfMeasure = Core.Constants.Length.Feet;
			container.JC_RH_NKContainerCommodityCode = "GEN";
			container.JC_HarmonisedCode = "XXX";
			var adapter = AgencyShipmentPackLineAdapter.New(container);
			AssertEquals(ZGuid.Empty, adapter.JL_JC);
			AssertEquals("AAAA0000007", adapter.JL_RefNumber);
			AssertEquals(2, adapter.JL_PackageCount);
			AssertEquals("mushrooms", adapter.JL_DetailedDescription);
			AssertEquals("marks & nums", adapter.JL_MarksAndNumbers);
			AssertEquals(2500m, adapter.JL_ActualWeight);
			AssertEquals(Core.Constants.Weight.Pounds, adapter.JL_ActualWeightUQ);
			AssertEquals(1.5m, adapter.JL_ActualVolume);
			AssertEquals(Core.Constants.Volume.CubicFeet, adapter.JL_ActualVolumeUQ);
			AssertEquals(1m, adapter.JL_Length);
			AssertEquals(2m, adapter.JL_Width);
			AssertEquals(3m, adapter.JL_Height);
			AssertEquals(Core.Constants.Length.Feet, adapter.JL_UnitOfDimension);
			AssertEquals("GEN", adapter.JL_RH_NKCommodityCode);
			AssertEquals("XXX", adapter.JL_HarmonisedCode);
		}

		public void TestSetProperties()
		{
			var container = Factory.New<AgencyShipmentContainer>();
			var adapter = AgencyShipmentPackLineAdapter.New(container);
			adapter.JL_RefNumber = "AAAA0000007";
			adapter.JL_PackageCount = 2;
			adapter.JL_DetailedDescription = "mushrooms";
			adapter.JL_MarksAndNumbers = "marks & nums";
			adapter.JL_ActualWeight = 2500m;
			adapter.JL_ActualWeightUQ = Core.Constants.Weight.Pounds;
			adapter.JL_ActualVolume = 1.5m;
			adapter.JL_ActualVolumeUQ = Core.Constants.Volume.CubicFeet;
			adapter.JL_Length = 1;
			adapter.JL_Width = 2;
			adapter.JL_Height = 3;
			adapter.JL_UnitOfDimension = Core.Constants.Length.Feet;
			adapter.JL_RH_NKCommodityCode = "GEN";
			adapter.JL_HarmonisedCode = "XXX";
			AssertEquals("AAAA0000007", container.JC_ContainerNum);
			AssertEquals(2, (int)container.JC_ContainerCount);
			AssertEquals("mushrooms", container.JC_Description);
			AssertEquals("marks & nums", container.JC_MarksAndNumbers);
			AssertEquals(2500m, container.JC_GrossWeight);
			AssertEquals(Core.Constants.Weight.Pounds, container.JC_GrossWeightUQ);
			AssertEquals(1.5m, container.JC_GrossVolume);
			AssertEquals(Core.Constants.Volume.CubicFeet, container.JC_GrossVolumeUQ);
			AssertEquals(1m, container.JC_TotalLength);
			AssertEquals(2m, container.JC_TotalWidth);
			AssertEquals(3m, container.JC_TotalHeight);
			AssertEquals(Core.Constants.Length.Feet, container.JC_TotalUnitOfMeasure);
			AssertEquals("GEN", container.JC_RH_NKContainerCommodityCode);
			AssertEquals("XXX", container.JC_HarmonisedCode);
		}

		public void TestDelete()
		{
			var container = Factory.New<AgencyShipmentContainer>();
			var adapter = AgencyShipmentPackLineAdapter.New(container);
			adapter.Delete();
			AssertEquals("container should be deleted", true, container.IsDeleted);
		}

		public void TestDangerousGoods()
		{
			var container = Factory.New<AgencyShipmentContainer>();
			var dangerousGoods1 = container.UNDGs.AddNew();
			var adapter = AgencyShipmentPackLineAdapter.New(container);
			AssertContainsExactElementsInAnyOrder(new[] { dangerousGoods1 }, adapter.UNDGs);
			var dangerousGoods2 = adapter.UNDGs.AddNew();
			AssertContainsExactElementsInAnyOrder(new[] { dangerousGoods1, dangerousGoods2 }, adapter.UNDGs);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return AgencyShipmentPackLineAdapter.New(Factory.NewWithValidTestData<AgencyShipmentContainer>());
		}
	}
}
