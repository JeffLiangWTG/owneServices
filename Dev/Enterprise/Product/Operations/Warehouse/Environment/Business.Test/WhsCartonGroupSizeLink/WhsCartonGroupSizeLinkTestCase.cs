using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Warehouse.Cartonisation.Integration;
using Enterprise.Warehouse.Environment.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsCartonGroupSizeLink))]
	class WhsCartonGroupSizeLinkTestCase : WhsEnvBusinessObjectTestCase
	{
		#region TestCartonGroup

		public void TestCartonGroup()
		{
			var link = Factory.New<WhsCartonGroupSizeLink>();
			AssertNull("Precondition", link.CartonGroup);

			var cartonGroup = Factory.New<WhsCartonGroup>();
			link.WCV_WCG = cartonGroup.PK;
			AssertEquals(cartonGroup, link.CartonGroup);
		}

		#endregion

		#region TestCartonSize

		public void TestCartonSize()
		{
			var link = Factory.New<WhsCartonGroupSizeLink>();
			AssertNull("Precondition", link.CartonSize);

			var cartonSize = Factory.New<WhsCartonSize>();
			link.WCV_WCS = cartonSize.PK;
			AssertEquals(cartonSize, link.CartonSize);
		}

		#endregion

		#region TestWCV_OptimizationCost_ReadOnly

		public void TestWCV_OptimizationCost_ReadOnly()
		{
			var group = Helper.CreateWhsCartonGroup("G1", "Group 1");
			var size = Helper.CreateWhsCartonSize("S1", 1, 1, 1, 1, 2, 1, new ZByte(80), Constants.Length.Metres, Constants.Weight.Kilograms);

			var link = Factory.New<WhsCartonGroupSizeLink>();
			link.WCV_WCS = size.PK;
			AssertEquals("Should be readonly.", true, link.WCV_OptimizationCostInfo.ReadOnly);

			link.WCV_WCG = group.PK;

			group.OptimizationMode = CartonizationOptimizationModes.Codes.MinimizeCartons;
			AssertEquals("Should be readonly.", true, link.WCV_OptimizationCostInfo.ReadOnly);

			group.OptimizationMode = CartonizationOptimizationModes.Codes.MinimizeVolume;
			AssertEquals("Should be readonly.", true, link.WCV_OptimizationCostInfo.ReadOnly);

			group.OptimizationMode = CartonizationOptimizationModes.Codes.CustomOptimizationCosts;
			AssertEquals("Should *not* be readonly.", false, link.WCV_OptimizationCostInfo.ReadOnly);
		}

		#endregion

		#region TestGetOptimizationCostBasedOnCartonSizeVolume_NullCartonSize

		public void TestGetOptimizationCostBasedOnCartonSizeVolume_NullCartonSize()
		{
			var link = Factory.New<WhsCartonGroupSizeLink>();
			link.WCV_OptimizationCost = 42;

			AssertEquals(1, link.GetOptimizationCostBasedOnCartonSizeVolume());
		}

		#endregion

		#region TestGetOptimizationCostBasedOnCartonSizeVolume

		public void TestGetOptimizationCostBasedOnCartonSizeVolume()
		{
			var group = Helper.CreateWhsCartonGroup("G1", "Group 1");
			var size = Helper.CreateWhsCartonSize("S1", 1, 1, 1, 1, 2, 1, new ZByte(80), Constants.Length.Centimetres, Constants.Weight.Kilograms, Constants.Volume.CubicCentimeters);
			var link = Helper.CreateWhsCartonGroupSizeLink(group, size);
			AssertEquals("Cost based on volume should be 1.", 1, link.GetOptimizationCostBasedOnCartonSizeVolume());

			size.WCS_Volume = 2m;
			AssertEquals("Cost based on volume should be 2.", 2, link.GetOptimizationCostBasedOnCartonSizeVolume());
		}

		#endregion

		#region TestGetOptimizationCostBasedOnCartonSizeVolume_Rounding

		public void TestGetOptimizationCostBasedOnCartonSizeVolume_Rounding()
		{
			var group = Helper.CreateWhsCartonGroup("G1", "Group 1");
			var size = Helper.CreateWhsCartonSize("S1", 1, 1, 1, 1, 2, 1, new ZByte(80), Constants.Length.Centimetres, Constants.Weight.Kilograms, Constants.Volume.CubicCentimeters);
			var link = Helper.CreateWhsCartonGroupSizeLink(group, size);
			AssertEquals("Cost based on volume should be 1.", 1, link.GetOptimizationCostBasedOnCartonSizeVolume());

			size.WCS_Volume = 2.49m;
			AssertEquals("Cost based on volume should be 2.", 2, link.GetOptimizationCostBasedOnCartonSizeVolume());

			size.WCS_Volume = 2.51m;
			AssertEquals("Cost based on volume should be 3.", 3, link.GetOptimizationCostBasedOnCartonSizeVolume());
		}

		#endregion

		#region TestGetOptimizationCostBasedOnCartonSizeVolume_Truncated

		public void TestGetOptimizationCostBasedOnCartonSizeVolume_Truncated()
		{
			var group = Helper.CreateWhsCartonGroup("G1", "Group 1");
			var size = Helper.CreateWhsCartonSize("S1", 1, 1, 1, 1, 2, 1, new ZByte(80), Constants.Length.Centimetres, Constants.Weight.Kilograms, Constants.Volume.CubicCentimeters);
			var link = Helper.CreateWhsCartonGroupSizeLink(group, size);
			AssertEquals("Cost based on volume should be 1.", 1, link.GetOptimizationCostBasedOnCartonSizeVolume());

			size.WCS_Volume = (decimal)int.MaxValue + 10;
			AssertEquals("Cost based on volume should be int.MaxValue.", int.MaxValue, link.GetOptimizationCostBasedOnCartonSizeVolume());

			size.WCS_Volume = 0.01;
			AssertEquals("Cost based on low volume should be 1.", 1, link.GetOptimizationCostBasedOnCartonSizeVolume());

			size.WCS_Volume = 0;
			AssertEquals("Cost based on zero volume should be 1.", 1, link.GetOptimizationCostBasedOnCartonSizeVolume());
		}

		#endregion

		#region TestGetOptimizationCostBasedOnCartonSizeVolume_Conversions

		public void TestGetOptimizationCostBasedOnCartonSizeVolume_Conversions()
		{
			var group = Helper.CreateWhsCartonGroup("G1", "Group 1");
			var size = Helper.CreateWhsCartonSize("S1", 1, 1, 1, 1, 2, 1, new ZByte(80), Constants.Length.Centimetres, Constants.Weight.Kilograms, Constants.Volume.CubicCentimeters);
			var link = Helper.CreateWhsCartonGroupSizeLink(group, size);
			AssertEquals("Cost based on volume should be 1.", 1, link.GetOptimizationCostBasedOnCartonSizeVolume());

			size.WCS_VolumeUQ = Constants.Volume.CubicInches;
			size.WCS_Volume = 0.5m;
			AssertEquals("Cost based on volume should be 8.", 8, link.GetOptimizationCostBasedOnCartonSizeVolume());
		}

		#endregion

		#region TestICartonDefinitionMembers

		public void TestICartonDefinitionMembers_MinimizeCartons()
		{
			TestICartonDefinitionMembers(CartonizationOptimizationModes.Codes.MinimizeCartons);
		}

		public void TestICartonDefinitionMembers_MinimizeVolume()
		{
			TestICartonDefinitionMembers(CartonizationOptimizationModes.Codes.MinimizeVolume);
		}

		public void TestICartonDefinitionMembers_MinimizeVolume_DifferentUnits()
		{
			TestICartonDefinitionMembers(CartonizationOptimizationModes.Codes.MinimizeVolume, useCubicMetres: true);
		}

		public void TestICartonDefinitionMembers_CustomOptimizationCosts()
		{
			TestICartonDefinitionMembers(CartonizationOptimizationModes.Codes.CustomOptimizationCosts);
		}

		void TestICartonDefinitionMembers(string optimizationMode, bool useCubicMetres = false)
		{
			var group = Helper.CreateWhsCartonGroup("1", "1");
			group.OptimizationMode = optimizationMode;

			var size = Helper.CreateWhsCartonSize("1", 1m, 2m, 3m, 4m, 6m, 10, new ZByte(80), Constants.Length.Centimetres, Constants.Weight.Kilograms, useCubicMetres ? Constants.Volume.CubicMetres : Constants.Volume.CubicCentimeters);
			size.WCS_Volume = 3.14m;
			group.CartonSizes.Add(size);

			var link = group.CartonGroupSizeLinks.Single();

			if (optimizationMode == CartonizationOptimizationModes.Codes.CustomOptimizationCosts)
			{
				link.WCV_OptimizationCost = 42;
			}

			var cartonDefinition = (ICartonDefinition)link;
			AssertEquals("PK", size.PK, cartonDefinition.PK);

			AssertEquals("Length", 1m, cartonDefinition.Length);
			AssertEquals("Width", 2m, cartonDefinition.Width);
			AssertEquals("Height", 3m, cartonDefinition.Height);
			AssertEquals("DimensionUQ", Constants.Length.Centimetres, cartonDefinition.DimensionUQ);

			AssertEquals("Volume", 3.14m, cartonDefinition.Volume);
			AssertEquals("VolumeUQ", useCubicMetres ? Constants.Volume.CubicMetres : Constants.Volume.CubicCentimeters, cartonDefinition.VolumeUQ);

			AssertEquals("EmptyWeight", 4m, cartonDefinition.EmptyWeight);
			AssertEquals("MaxWeight", 6m, cartonDefinition.MaxWeight);
			AssertEquals("WeightUQ", Constants.Weight.Kilograms, cartonDefinition.WeightUQ);

			AssertEquals("MaxNumberOfUnits", 10m, cartonDefinition.MaxNumberOfUnits);

			AssertEquals("MaxFillPercent", 0.80m, cartonDefinition.MaxFillPercent);

			var expectedCost = (decimal)link.WCV_OptimizationCost;
			if (optimizationMode == CartonizationOptimizationModes.Codes.MinimizeVolume)
			{
				expectedCost = size.WCS_Volume * (useCubicMetres ? 1000000 : 1);
			}

			AssertEquals("Cost", expectedCost, cartonDefinition.Cost);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var newHelper = new WhsTestHelperFunctionsEnv(factory);
			var group = newHelper.CreateWhsCartonGroup("G1", "Group 1");
			var size = newHelper.CreateWhsCartonSize("S1", 1, 1, 1, 1, 2, 1, new ZByte(80), Constants.Length.Metres, Constants.Weight.Kilograms);
			var link = factory.New<WhsCartonGroupSizeLink>();
			link.WCV_WCG = group.PK;
			link.WCV_WCS = size.PK;
			return link;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		#endregion
	}
}
