using System;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Packing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsCartonSize))]
	public class WhsCartonSizeTestCase : WhsEnvBusinessObjectTestCase
	{
		#region TestAttachedToACartonGroup

		public void TestAttachedToACartonGroup()
		{
			var cartonSize = Helper.CreateWhsCartonSize("S1");
			AssertEquals("Precondition", false, cartonSize.AttachedToACartonGroup);

			var cartonGroup = Helper.CreateWhsCartonGroup("G1", "Group 1");
			cartonGroup.CartonSizes.Add(cartonSize);
			AssertEquals(true, cartonSize.AttachedToACartonGroup);

			cartonGroup.CartonSizes.RemoveFromRelationship(cartonSize);
			AssertEquals(false, cartonSize.AttachedToACartonGroup);
		}

		#endregion

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			var cartonSize = Factory.New<WhsCartonSize>();
			AssertEquals("Carton Size", cartonSize.HumanReadableName);
			AssertEquals("Carton Size", cartonSize.HumanReadableShortcutName);

			cartonSize.WCS_Code = "Box";
			AssertEquals("Box", cartonSize.HumanReadableName);
			AssertEquals("Box", cartonSize.HumanReadableShortcutName);
		}

		#endregion

		#region TestIsAutoLogged

		public void TestIsAutoLogged()
		{
			AssertEquals(true, Factory.New<WhsCartonSize>().IsAutoAdminBusinessObjectLoggerEnabled);
		}

		#endregion

		#region TestDelete_RemovesPivots

		public void TestDelete_RemovesPivots()
		{
			var cartonSize = Helper.CreateWhsCartonSize("S1");
			var cartonGroup = Helper.CreateWhsCartonGroup("G1", "Group 1");
			cartonGroup.CartonSizes.Add(cartonSize);
			Factory.Save();

			cartonSize.Delete();
			AssertNoExceptionThrown(Factory.Save);
			AssertEquals("Should have deleted pivots.", 0, Factory.Load<WhsCartonGroupSizeLink>(new ZQuery()).Length);
		}

		#endregion

		#region TestSetDefaultValues

		public void TestSetDefaultValues()
		{
			var cartonSize1 = Factory.New<WhsCartonSize>();
			AssertEquals(new ZByte(80), cartonSize1.WCS_MaxFillPercent);
			AssertEquals(999999, cartonSize1.WCS_MaxUnits);

			var defaultUnits1 = ObjectFactory.Get<IPackageDefaultUQs>();
			AssertEquals(defaultUnits1.DefaultDimensionUnit, cartonSize1.WCS_DimensionUQ);
			AssertEquals(defaultUnits1.DefaultVolumeUnit, cartonSize1.WCS_VolumeUQ);
			AssertEquals(defaultUnits1.DefaultWeightUnit, cartonSize1.WCS_WeightUQ);

			var mock = new Mock<IPackageDefaultUQs>();
			mock.Setup(m => m.DefaultDimensionUnit).Returns(Constants.Length.Yards);
			mock.Setup(m => m.DefaultVolumeUnit).Returns(Constants.Volume.TeaChest);
			mock.Setup(m => m.DefaultWeightUnit).Returns(Constants.Weight.MetricCarat);
			using (ObjectFactory.Substitute(mock.Object))
			{
				var cartonSize2 = Factory.New<WhsCartonSize>();
				AssertEquals(Constants.Length.Yards, cartonSize2.WCS_DimensionUQ);
				AssertEquals(Constants.Volume.TeaChest, cartonSize2.WCS_VolumeUQ);
				AssertEquals(Constants.Weight.MetricCarat, cartonSize2.WCS_WeightUQ);
			}
			mock.VerifyAll();
		}

		#endregion

		#region TestVolumeCalculation

		public void TestVolumeCalculation_WCS_Length()
		{
			TestVolumeCalculation_Core((cartonSize, dim) => cartonSize.WCS_Length = dim);
		}

		public void TestVolumeCalculation_WCS_Width()
		{
			TestVolumeCalculation_Core((cartonSize, dim) => cartonSize.WCS_Width = dim);
		}

		public void TestVolumeCalculation_WCS_Height()
		{
			TestVolumeCalculation_Core((cartonSize, dim) => cartonSize.WCS_Height = dim);
		}

		void TestVolumeCalculation_Core(Action<WhsCartonSize, decimal> setDim)
		{
			var group = Helper.CreateWhsCartonGroup("CG", "CG");
			group.OptimizationMode = CartonizationOptimizationModes.Codes.MinimizeVolume;

			var cartonSize = Factory.New<WhsCartonSize>();
			var link = Helper.CreateWhsCartonGroupSizeLink(group, cartonSize);

			cartonSize.WCS_DimensionUQ = "";
			cartonSize.WCS_Length = 1m;
			cartonSize.WCS_Width = 1m;
			cartonSize.WCS_Height = 1m;
			cartonSize.WCS_VolumeUQ = "M3";
			AssertEquals("Precondition - shouldn't try calculating volume until all dims + UQs are entered.", 0m, cartonSize.WCS_Volume);

			cartonSize.WCS_DimensionUQ = "M";
			AssertEquals("Should calculate volume.", 1m, cartonSize.WCS_Volume);
			AssertEquals("Should update cost.", 1000000, link.WCV_OptimizationCost);

			setDim(cartonSize, 2m);
			AssertEquals("Should recalculate volume when dimension changed.", 2m, cartonSize.WCS_Volume);
			AssertEquals("Should update cost.", 2000000, link.WCV_OptimizationCost);

			cartonSize.WCS_DimensionUQ = "FT";
			AssertEquals("Should recalculate and change according to unit conversions.", 0.057m, cartonSize.WCS_Volume);
			AssertEquals("Should update cost.", 57000, link.WCV_OptimizationCost);

			cartonSize.WCS_VolumeUQ = "";
			setDim(cartonSize, 3m);
			AssertEquals("Shouldn't try to recalculate if not all dims or UQs are entered.", 0.057m, cartonSize.WCS_Volume);
			AssertEquals("Shouldn't try to recalculate if not all dims or UQs are entered.", 57000, link.WCV_OptimizationCost);

			cartonSize.WCS_VolumeUQ = "M3";
			setDim(cartonSize, 3m);
			AssertEquals("Should recalculate volume when dimension/UQ changed.", 0.085m, cartonSize.WCS_Volume);
			AssertEquals("Should update cost.", 85000, link.WCV_OptimizationCost);

			setDim(cartonSize, 0m);
			AssertEquals("Shouldn't try to recalculate if not all dims or UQs are entered.", 0.085m, cartonSize.WCS_Volume);
			AssertEquals("Shouldn't try to recalculate if not all dims or UQs are entered.", 85000, link.WCV_OptimizationCost);
		}

		public void TestVolumeCalculation_VolumeUnit()
		{
			var group = Helper.CreateWhsCartonGroup("CG", "CG");
			group.OptimizationMode = CartonizationOptimizationModes.Codes.MinimizeVolume;

			var cartonSize = Factory.New<WhsCartonSize>();
			var link = Helper.CreateWhsCartonGroupSizeLink(group, cartonSize);

			cartonSize.WCS_VolumeUQ = "";
			cartonSize.WCS_Length = 1m;
			cartonSize.WCS_Width = 1m;
			cartonSize.WCS_Height = 1m;
			cartonSize.WCS_DimensionUQ = "M";
			AssertEquals("Precondition - shouldn't try calculating volume until all dims + UQs are entered.", 0m, cartonSize.WCS_Volume);

			cartonSize.WCS_VolumeUQ = "M3";
			AssertEquals("Should calculate volume.", 1m, cartonSize.WCS_Volume);
			AssertEquals("Should update cost.", 1000000, link.WCV_OptimizationCost);

			cartonSize.WCS_VolumeUQ = "CC";
			AssertEquals("Should calculate volume.", 1000000m, cartonSize.WCS_Volume);
			AssertEquals("Should *not* change cost.", 1000000, link.WCV_OptimizationCost);
		}

		public void TestVolumeCalculation_CanCalculateVolume()
		{
			var cartonSize = Factory.New<WhsCartonSize>();
			AssertEquals("Precondition: Should not be able to calculate volume by default.", false, cartonSize.CanCalculateVolume);

			cartonSize.WCS_Length = 1m;
			AssertEquals("Should not be able to calculate volume as only Length has been entered.", false, cartonSize.CanCalculateVolume);

			cartonSize.WCS_Width = 1m;
			AssertEquals("Should not be able to calculate volume until all required dimensions are entered.", false, cartonSize.CanCalculateVolume);

			cartonSize.WCS_Height = 1m;
			AssertEquals("Should calculate volume now, as dimentions are entered and UQs are defaulted to Registry settings.", true, cartonSize.CanCalculateVolume);

			// clear defaults
			cartonSize.WCS_DimensionUQ = "";
			cartonSize.WCS_VolumeUQ = "";
			AssertEquals("Should not be able to calculate volume as UQs are blank.", false, cartonSize.CanCalculateVolume);

			cartonSize.WCS_DimensionUQ = "M";
			AssertEquals("Should not be able to calculate volume until all required dimensions are entered.", false, cartonSize.CanCalculateVolume);

			cartonSize.WCS_VolumeUQ = "M3";
			AssertEquals("Should calculate volume now that all required dimensions have been entered.", true, cartonSize.CanCalculateVolume);

			cartonSize.WCS_VolumeUQ = "";
			AssertEquals("Missing Volume Dimension and can no longer calculate volume.", false, cartonSize.CanCalculateVolume);
		}

		public void TestVolumeCalculation_CalculatedVolume()
		{
			var cartonSize = Factory.New<WhsCartonSize>();

			cartonSize.WCS_Length = 1m;
			cartonSize.WCS_Width = 2m;
			cartonSize.WCS_Height = 3m;
			cartonSize.WCS_DimensionUQ = "M";
			cartonSize.WCS_VolumeUQ = "M3";
			AssertEquals("Should calculate volume in Cubic Metres.", 6.0m, cartonSize.CalculatedVolume);
		}

		#endregion

		#region TestWCS_DimensionUQ

		public void TestWCS_DimensionUQ()
		{
			AssertHasCustomAttribute<ListAttribute>(
				typeof(WhsCartonSize), WhsCartonSizeSchema.Constants.WCS_DimensionUQ, false,
				la => la.ListDataSourceMember == "Lookups.DimensionUQs");
		}

		#endregion

		#region TestWCS_WeightUQ

		public void TestWCS_WeightUQ()
		{
			AssertHasCustomAttribute<ListAttribute>(
				typeof(WhsCartonSize), WhsCartonSizeSchema.Constants.WCS_WeightUQ, false,
				la => la.ListDataSourceMember == "Lookups.WeightUQs");
		}

		#endregion

		#region TestWCS_VolumeUQ

		public void TestWCS_VolumeUQ()
		{
			AssertHasCustomAttribute<ListAttribute>(
				typeof(WhsCartonSize), WhsCartonSizeSchema.Constants.WCS_VolumeUQ, false,
				la => la.ListDataSourceMember == "Lookups.VolumeUQs");
		}

		#endregion

		#region TestWCS_VolumeUQ_UpdatesLinksVolume

		public void TestWCS_VolumeUQ_UpdatesLinksVolume()
		{
			var group1 = Helper.CreateWhsCartonGroup("1", "1");
			group1.OptimizationMode = CartonizationOptimizationModes.Codes.MinimizeVolume;

			var group2 = Helper.CreateWhsCartonGroup("2", "2");
			group2.OptimizationMode = CartonizationOptimizationModes.Codes.CustomOptimizationCosts;

			var group3 = Helper.CreateWhsCartonGroup("3", "3");
			group3.OptimizationMode = CartonizationOptimizationModes.Codes.MinimizeCartons;

			var cartonSize = Helper.CreateWhsCartonSize("S1");
			var otherCartonSize = Helper.CreateWhsCartonSize("S2");
			var link1 = Helper.CreateWhsCartonGroupSizeLink(group1, cartonSize);
			var otherLink = Helper.CreateWhsCartonGroupSizeLink(group1, otherCartonSize);
			var link2 = Helper.CreateWhsCartonGroupSizeLink(group2, cartonSize);
			var link3 = Helper.CreateWhsCartonGroupSizeLink(group3, cartonSize);

			cartonSize.WCS_VolumeUQ = Constants.Volume.CubicCentimeters;
			cartonSize.WCS_Volume = 42m;

			otherCartonSize.WCS_VolumeUQ = Constants.Volume.CubicCentimeters;
			otherCartonSize.WCS_Volume = 9.81m;

			link1.WCV_OptimizationCost = 42;
			otherLink.WCV_OptimizationCost = 10;
			link2.WCV_OptimizationCost = 5;
			link3.WCV_OptimizationCost = 1;
			Factory.Save();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var cartonSizeInFactory2 = factory2.Load<WhsCartonSize>(cartonSize.PK);
			cartonSizeInFactory2.WCS_Volume = 36m;

			var link1InFactory2 = factory2.Load<WhsCartonGroupSizeLink>(link1.PK);
			var otherLinkFactory2 = factory2.Load<WhsCartonGroupSizeLink>(otherLink.PK);
			var link2InFactory2 = factory2.Load<WhsCartonGroupSizeLink>(link2.PK);
			var link3InFactory2 = factory2.Load<WhsCartonGroupSizeLink>(link3.PK);
			AssertEquals("Should have updated cost.", 36, link1InFactory2.WCV_OptimizationCost);
			AssertEquals("Should *not* have updated cost.", 10, otherLinkFactory2.WCV_OptimizationCost);
			AssertEquals("Should *not* have updated cost.", 5, link2InFactory2.WCV_OptimizationCost);
			AssertEquals("Should *not* have updated cost.", 1, link3InFactory2.WCV_OptimizationCost);

			var group1InFactory2 = factory2.Load<WhsCartonGroup>(group1.PK);
			var group2InFactory2 = factory2.Load<WhsCartonGroup>(group2.PK);
			var group3InFactory2 = factory2.Load<WhsCartonGroup>(group3.PK);
			AssertEquals(CartonizationOptimizationModes.Codes.MinimizeVolume, group1InFactory2.OptimizationMode);
			AssertEquals(CartonizationOptimizationModes.Codes.CustomOptimizationCosts, group2InFactory2.OptimizationMode);
			AssertEquals(CartonizationOptimizationModes.Codes.MinimizeCartons, group3InFactory2.OptimizationMode);

			cartonSizeInFactory2.WCS_Volume = 1.6m;
			AssertEquals("Should have updated cost.", 2, link1InFactory2.WCV_OptimizationCost);

			cartonSizeInFactory2.WCS_Volume = 0m;
			AssertEquals("Should have updated cost.", 1, link1InFactory2.WCV_OptimizationCost);

			cartonSizeInFactory2.WCS_Volume = (decimal)int.MaxValue + 10;
			AssertEquals("Should have updated cost.", int.MaxValue, link1InFactory2.WCV_OptimizationCost);

			cartonSizeInFactory2.WCS_VolumeUQ = Constants.Volume.CubicInches;
			cartonSizeInFactory2.WCS_Volume = 0.5m;
			AssertEquals("Should have updated cost.", 8, link1InFactory2.WCV_OptimizationCost);
		}

		#endregion

		#region TestIPackageTemplateMembers

		public void TestIPackageTemplateMembers()
		{
			var packageTemplateBizO = Helper.CreateWhsCartonSize("1", 1m, 2m, 3m, 4m, 6m, 10, new ZByte(80), Constants.Length.Metres, Constants.Weight.Kilograms);
			var packageTemplate = (IPackageTemplate)packageTemplateBizO;
			AssertEquals("Length", 1m, packageTemplate.Length);
			AssertEquals("Width", 2m, packageTemplate.Width);
			AssertEquals("Height", 3m, packageTemplate.Height);
			AssertEquals("DimensionUQ", Constants.Length.Metres, packageTemplate.DimensionUQ);

			AssertEquals("EmptyWeight", 4m, packageTemplate.TareWeight);
			AssertEquals("WeightUQ", Constants.Weight.Kilograms, packageTemplate.WeightUQ);

			packageTemplateBizO.WCS_Volume = 11m;
			AssertEquals("Volume", 11m, packageTemplate.Volume);
			AssertEquals("VolumeUQ", Constants.Volume.CubicMetres, packageTemplate.VolumeUQ);
		}

		#endregion

		#region TestWCS_F3_NKPackType

		public void TestWCS_F3_NKPackType()
		{
			AssertHasCustomAttribute<ListAttribute>(
				typeof(WhsCartonSize), WhsCartonSizeSchema.Constants.WCS_F3_NKPackType, true,
				attrib => attrib.ListDataSourceMember == "Lookups.PackTypes");
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var newHelper = new WhsTestHelperFunctionsEnv(factory);
			return newHelper.CreateWhsCartonSize("S1", 1, 1, 1, 1, 2, 1, new ZByte(80), Constants.Length.Metres, Constants.Weight.Kilograms);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		#endregion
	}
}
