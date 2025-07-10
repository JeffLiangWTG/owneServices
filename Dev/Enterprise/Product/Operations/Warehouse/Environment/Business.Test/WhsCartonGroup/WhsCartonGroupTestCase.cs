using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Warehouse.Environment.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsCartonGroup))]
	class WhsCartonGroupTestCase : WhsEnvBusinessObjectTestCase
	{
		#region TestAttachedOrganisationsCodes

		public void TestAttachedOrganisationsCodes()
		{
			var cartonGroup = Helper.CreateWhsCartonGroup("G1", "Group 1");
			AssertEquals("", cartonGroup.AttachedOrganisationsCodes);

			var org1 = Helper.CreateClient("1");
			var org2 = Helper.CreateClient("2");
			var org3 = Helper.CreateClient("3");
			var org4 = Helper.CreateClient("4");

			cartonGroup.ParentOrgMiscServs.Add(org1.MiscServ);
			AssertEquals("1", cartonGroup.AttachedOrganisationsCodes);

			cartonGroup.ParentOrgMiscServs.Add(org2.MiscServ);
			AssertEquals("1, 2", cartonGroup.AttachedOrganisationsCodes);

			cartonGroup.ParentOrgMiscServs.Add(org3.MiscServ);
			cartonGroup.ParentOrgMiscServs.Add(org4.MiscServ);
			AssertEquals("<Many>", cartonGroup.AttachedOrganisationsCodes);
		}

		#endregion

		#region TestOptimizationMode_MinimizeCartons

		public void TestOptimizationMode_MinimizeCartons()
		{
			var group = Helper.CreateWhsCartonGroup("G1", "Group 1");
			var size1 = Helper.CreateWhsCartonSize("S1", 1, 1, 1, 1, 2, 1, new ZByte(80), Constants.Length.Centimetres, Constants.Weight.Grams, Constants.Volume.CubicCentimeters);
			var size2 = Helper.CreateWhsCartonSize("S2", 2, 2, 2, 1, 2, 1, new ZByte(80), Constants.Length.Centimetres, Constants.Weight.Grams, Constants.Volume.CubicCentimeters);
			var size3 = Helper.CreateWhsCartonSize("S3", 3, 3, 3, 1, 2, 1, new ZByte(80), Constants.Length.Centimetres, Constants.Weight.Grams, Constants.Volume.CubicCentimeters);
			AssertEquals(CartonizationOptimizationModes.Codes.MinimizeCartons, group.OptimizationMode);

			var link1 = Helper.CreateWhsCartonGroupSizeLink(group, size1);
			var link2 = Helper.CreateWhsCartonGroupSizeLink(group, size2);
			var link3 = Helper.CreateWhsCartonGroupSizeLink(group, size3);
			AssertEquals(CartonizationOptimizationModes.Codes.MinimizeCartons, group.OptimizationMode);

			var linksChangedCount = 0;
			((IBindingList)group.CartonGroupSizeLinks).ListChanged += (s, e) => linksChangedCount++;

			group.OptimizationMode = CartonizationOptimizationModes.Codes.CustomOptimizationCosts;
			AssertEquals(1, linksChangedCount);

			link1.WCV_OptimizationCost = 100;
			link2.WCV_OptimizationCost = 50;
			link3.WCV_OptimizationCost = 200;

			group.OptimizationMode = CartonizationOptimizationModes.Codes.MinimizeCartons;
			AssertEquals(1, link1.WCV_OptimizationCost);
			AssertEquals(1, link2.WCV_OptimizationCost);
			AssertEquals(1, link3.WCV_OptimizationCost);
			AssertEquals(CartonizationOptimizationModes.Codes.MinimizeCartons, group.OptimizationMode);
			Factory.Save();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var groupInFactory2 = factory2.Load<WhsCartonGroup>(group.PK);
			AssertEquals(CartonizationOptimizationModes.Codes.MinimizeCartons, groupInFactory2.OptimizationMode);
		}

		#endregion

		#region TestOptimizationMode_MinimizeVolume

		public void TestOptimizationMode_MinimizeVolume()
		{
			var group = Helper.CreateWhsCartonGroup("G1", "Group 1");
			var size1 = Helper.CreateWhsCartonSize("S1", 1, 1, 1, 1, 2, 1, new ZByte(80), Constants.Length.Centimetres, Constants.Weight.Grams, Constants.Volume.CubicCentimeters);
			var size2 = Helper.CreateWhsCartonSize("S2", 2, 2, 2, 1, 2, 1, new ZByte(80), Constants.Length.Centimetres, Constants.Weight.Grams, Constants.Volume.CubicCentimeters);
			var size3 = Helper.CreateWhsCartonSize("S3", 3, 3, 3, 1, 2, 1, new ZByte(80), Constants.Length.Centimetres, Constants.Weight.Grams, Constants.Volume.CubicCentimeters);
			AssertEquals(CartonizationOptimizationModes.Codes.MinimizeCartons, group.OptimizationMode);

			var link1 = Helper.CreateWhsCartonGroupSizeLink(group, size1);
			var link2 = Helper.CreateWhsCartonGroupSizeLink(group, size2);
			var link3 = Helper.CreateWhsCartonGroupSizeLink(group, size3);
			AssertEquals(CartonizationOptimizationModes.Codes.MinimizeCartons, group.OptimizationMode);

			group.OptimizationMode = CartonizationOptimizationModes.Codes.MinimizeVolume;
			AssertEquals(1, link1.WCV_OptimizationCost);
			AssertEquals(8, link2.WCV_OptimizationCost);
			AssertEquals(27, link3.WCV_OptimizationCost);
			AssertEquals(CartonizationOptimizationModes.Codes.MinimizeVolume, group.OptimizationMode);
			Factory.Save();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var groupInFactory2 = factory2.Load<WhsCartonGroup>(group.PK);
			AssertEquals(CartonizationOptimizationModes.Codes.MinimizeVolume, groupInFactory2.OptimizationMode);

			group.OptimizationMode = CartonizationOptimizationModes.Codes.MinimizeCartons;
			AssertEquals(1, link1.WCV_OptimizationCost);
			AssertEquals(1, link2.WCV_OptimizationCost);
			AssertEquals(1, link3.WCV_OptimizationCost);
			AssertEquals(CartonizationOptimizationModes.Codes.MinimizeCartons, group.OptimizationMode);
			Factory.Save();

			var factory3 = new BusinessObjectFactory { RefreshEnabled = false };
			var groupInFactory3 = factory3.Load<WhsCartonGroup>(group.PK);
			AssertEquals(CartonizationOptimizationModes.Codes.MinimizeCartons, groupInFactory3.OptimizationMode);
		}

		#endregion

		#region TestOptimizationMode_MinimizeVolume_DifferentUnit

		public void TestOptimizationMode_MinimizeVolume_DifferentUnit()
		{
			var group = Helper.CreateWhsCartonGroup("G1", "Group 1");
			var size1 = Helper.CreateWhsCartonSize("S1", 1, 1, 1, 1, 2, 1, new ZByte(80), Constants.Length.Metres, Constants.Weight.Grams, Constants.Volume.CubicMetres);
			var size2 = Helper.CreateWhsCartonSize("S2", 2, 2, 2, 1, 2, 1, new ZByte(80), Constants.Length.Metres, Constants.Weight.Grams, Constants.Volume.CubicMetres);
			var size3 = Helper.CreateWhsCartonSize("S3", 3, 3, 3, 1, 2, 1, new ZByte(80), Constants.Length.Metres, Constants.Weight.Grams, Constants.Volume.CubicMetres);
			AssertEquals(CartonizationOptimizationModes.Codes.MinimizeCartons, group.OptimizationMode);

			var link1 = Helper.CreateWhsCartonGroupSizeLink(group, size1);
			var link2 = Helper.CreateWhsCartonGroupSizeLink(group, size2);
			var link3 = Helper.CreateWhsCartonGroupSizeLink(group, size3);
			AssertEquals(CartonizationOptimizationModes.Codes.MinimizeCartons, group.OptimizationMode);

			group.OptimizationMode = CartonizationOptimizationModes.Codes.MinimizeVolume;
			AssertEquals(1000000, link1.WCV_OptimizationCost);
			AssertEquals(8000000, link2.WCV_OptimizationCost);
			AssertEquals(27000000, link3.WCV_OptimizationCost);
			AssertEquals(CartonizationOptimizationModes.Codes.MinimizeVolume, group.OptimizationMode);
			Factory.Save();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var groupInFactory2 = factory2.Load<WhsCartonGroup>(group.PK);
			AssertEquals(CartonizationOptimizationModes.Codes.MinimizeVolume, groupInFactory2.OptimizationMode);

			group.OptimizationMode = CartonizationOptimizationModes.Codes.MinimizeCartons;
			AssertEquals(1, link1.WCV_OptimizationCost);
			AssertEquals(1, link2.WCV_OptimizationCost);
			AssertEquals(1, link3.WCV_OptimizationCost);
			AssertEquals(CartonizationOptimizationModes.Codes.MinimizeCartons, group.OptimizationMode);
			Factory.Save();

			var factory3 = new BusinessObjectFactory { RefreshEnabled = false };
			var groupInFactory3 = factory3.Load<WhsCartonGroup>(group.PK);
			AssertEquals(CartonizationOptimizationModes.Codes.MinimizeCartons, groupInFactory3.OptimizationMode);
		}

		#endregion

		#region TestOptimizationMode_JunkMode

		public void TestOptimizationMode_JunkMode()
		{
			var group = Helper.CreateWhsCartonGroup("G1", "Group 1");
			var size1 = Helper.CreateWhsCartonSize("S1", 1, 1, 1, 1, 2, 1, new ZByte(80), Constants.Length.Centimetres, Constants.Weight.Grams, Constants.Volume.CubicCentimeters);
			var size2 = Helper.CreateWhsCartonSize("S2", 2, 2, 2, 1, 2, 1, new ZByte(80), Constants.Length.Centimetres, Constants.Weight.Grams, Constants.Volume.CubicCentimeters);
			var size3 = Helper.CreateWhsCartonSize("S3", 3, 3, 3, 1, 2, 1, new ZByte(80), Constants.Length.Centimetres, Constants.Weight.Grams, Constants.Volume.CubicCentimeters);
			AssertEquals(CartonizationOptimizationModes.Codes.MinimizeCartons, group.OptimizationMode);

			var link1 = Helper.CreateWhsCartonGroupSizeLink(group, size1);
			var link2 = Helper.CreateWhsCartonGroupSizeLink(group, size2);
			var link3 = Helper.CreateWhsCartonGroupSizeLink(group, size3);
			AssertEquals(CartonizationOptimizationModes.Codes.MinimizeCartons, group.OptimizationMode);

			group.OptimizationMode = "LOL";
			AssertEquals(1, link1.WCV_OptimizationCost);
			AssertEquals(1, link2.WCV_OptimizationCost);
			AssertEquals(1, link3.WCV_OptimizationCost);

			group.OptimizationMode = CartonizationOptimizationModes.Codes.MinimizeVolume;
			AssertEquals(1, link1.WCV_OptimizationCost);
			AssertEquals(8, link2.WCV_OptimizationCost);
			AssertEquals(27, link3.WCV_OptimizationCost);
			AssertEquals(CartonizationOptimizationModes.Codes.MinimizeVolume, group.OptimizationMode);
			Factory.Save();

			group.OptimizationMode = "";
			AssertEquals(1, link1.WCV_OptimizationCost);
			AssertEquals(8, link2.WCV_OptimizationCost);
			AssertEquals(27, link3.WCV_OptimizationCost);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var groupInFactory2 = factory2.Load<WhsCartonGroup>(group.PK);
			AssertEquals(CartonizationOptimizationModes.Codes.MinimizeVolume, groupInFactory2.OptimizationMode);
		}

		#endregion

		#region TestOptimizationMode_CustomCosts

		public void TestOptimizationMode_CustomCosts()
		{
			var group = Helper.CreateWhsCartonGroup("G1", "Group 1");
			var size1 = Helper.CreateWhsCartonSize("S1", 1, 1, 1, 1, 2, 1, new ZByte(80), Constants.Length.Centimetres, Constants.Weight.Grams, Constants.Volume.CubicCentimeters);
			var size2 = Helper.CreateWhsCartonSize("S2", 2, 2, 2, 1, 2, 1, new ZByte(80), Constants.Length.Centimetres, Constants.Weight.Grams, Constants.Volume.CubicCentimeters);
			var size3 = Helper.CreateWhsCartonSize("S3", 3, 3, 3, 1, 2, 1, new ZByte(80), Constants.Length.Centimetres, Constants.Weight.Grams, Constants.Volume.CubicCentimeters);
			AssertEquals(CartonizationOptimizationModes.Codes.MinimizeCartons, group.OptimizationMode);

			var link1 = Helper.CreateWhsCartonGroupSizeLink(group, size1);
			var link2 = Helper.CreateWhsCartonGroupSizeLink(group, size2);
			var link3 = Helper.CreateWhsCartonGroupSizeLink(group, size3);
			AssertEquals(CartonizationOptimizationModes.Codes.MinimizeCartons, group.OptimizationMode);

			group.OptimizationMode = CartonizationOptimizationModes.Codes.CustomOptimizationCosts;
			AssertEquals(1, link1.WCV_OptimizationCost);
			AssertEquals(1, link2.WCV_OptimizationCost);
			AssertEquals(1, link3.WCV_OptimizationCost);

			link1.WCV_OptimizationCost = 100;
			link2.WCV_OptimizationCost = 50;
			link3.WCV_OptimizationCost = 200;
			AssertEquals(CartonizationOptimizationModes.Codes.CustomOptimizationCosts, group.OptimizationMode);
			Factory.Save();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var groupInFactory2 = factory2.Load<WhsCartonGroup>(group.PK);
			AssertEquals(CartonizationOptimizationModes.Codes.CustomOptimizationCosts, groupInFactory2.OptimizationMode);
		}

		#endregion

		#region TestOptimizationMode_Lookups

		public void TestOptimizationMode_Lookups()
		{
			AssertHasCustomAttribute<ListAttribute>(typeof(WhsCartonGroup), nameof(WhsCartonGroup.OptimizationMode), false, a => a.ListDataSourceMember == "Lookups.OptimizationModes");
		}

		#endregion

		#region TestOptimizationMode_MaxLength

		public void TestOptimizationMode_MaxLength()
		{
			AssertHasCustomAttribute<MaxLengthAttribute>(typeof(WhsCartonGroup), nameof(WhsCartonGroup.OptimizationMode), false, a => a.MaxLength == 3);
		}

		#endregion

		#region TestIsAutoLogged

		public void TestIsAutoLogged()
		{
			AssertEquals(true, Factory.New<WhsCartonGroup>().IsAutoAdminBusinessObjectLoggerEnabled);
		}

		#endregion

		#region TestCartonSizes

		public void TestCartonSizes()
		{
			var cartonGroup = Helper.CreateWhsCartonGroup("G1", "Group 1");
			var cartonSize1 = Helper.CreateWhsCartonSize("S1");
			var cartonSize2 = Helper.CreateWhsCartonSize("S2");

			AssertEquals("Precondition", 0, cartonGroup.CartonSizes.Count);
			cartonGroup.HasChanges = false; // To avoid unneccessary factory save
			AssertEquals("Precondition", false, cartonGroup.HasChanges);

			cartonGroup.CartonSizes.Add(cartonSize1);
			AssertEquals(1, cartonGroup.CartonSizes.Count);
			AssertEquals("Should be registered child editable", true, cartonGroup.HasChanges);

			cartonGroup.CartonSizes.Add(cartonSize2);
			AssertEquals(2, cartonGroup.CartonSizes.Count);

			cartonGroup.Delete();
			AssertNoExceptionThrown(Factory.Save);
			AssertEquals("Should have deleted pivots", 0, Factory.Load<WhsCartonGroupSizeLink>(new ZQuery()).Length);
			AssertEquals("Should not have deleted carton sizes", false, cartonSize1.IsDeleted);
		}

		#endregion

		#region TestCartonGroupSizeLinks

		public void TestCartonGroupSizeLinks()
		{
			var cartonGroup = Helper.CreateWhsCartonGroup("G1", "Group 1");
			var cartonSize1 = Helper.CreateWhsCartonSize("S1");
			var cartonSize2 = Helper.CreateWhsCartonSize("S2");

			AssertEquals("Precondition", 0, cartonGroup.CartonSizes.Count);
			AssertEquals("Precondition", 0, cartonGroup.CartonGroupSizeLinks.Count);
			AssertType<ReadOnlyWhsCartonGroupSizeLinkCollection>(cartonGroup.CartonGroupSizeLinks);
			AssertEquals(false, ((IBindingList)cartonGroup.CartonGroupSizeLinks).AllowNew);

			cartonGroup.CartonSizes.Add(cartonSize1);
			AssertEquals(1, cartonGroup.CartonSizes.Count);
			AssertEquals(1, cartonGroup.CartonGroupSizeLinks.Count);

			cartonGroup.CartonSizes.Add(cartonSize2);
			AssertEquals(2, cartonGroup.CartonSizes.Count);
			AssertEquals(2, cartonGroup.CartonGroupSizeLinks.Count);
		}

		#endregion

		#region TestParentOrgMiscServs

		public void TestParentOrgMiscServs()
		{
			var cartonGroup = Helper.CreateWhsCartonGroup("G1", "Group 1");

			AssertEquals("Precondition", 0, cartonGroup.ParentOrgMiscServs.Count);
			cartonGroup.HasChanges = false; // To avoid unneccessary factory save
			AssertEquals("Precondition", false, cartonGroup.HasChanges);

			var org = Helper.CreateClient();
			cartonGroup.ParentOrgMiscServs.Add(org.MiscServ);
			AssertEquals(1, cartonGroup.ParentOrgMiscServs.Count);
			AssertEquals("Should be registered child editable", true, cartonGroup.HasChanges);

			cartonGroup.Delete();
			AssertNoExceptionThrown(Factory.Save);
			AssertEquals(ZGuid.Empty, org.MiscServ.OM_WCG_CartonGroup);
			AssertEquals("Should not have deleted OrgHeader", false, org.IsDeleted);
			AssertEquals("Should not have deleted OrgMiscServ", false, org.MiscServ.IsDeleted);
		}

		#endregion

		#region TestFetchStrategy

		public void TestFetchStrategy()
		{
			AssertType<WhsCartonGroupFetchStrategy>(Factory.New<WhsCartonGroup>().FetchStrategy);
		}

		#endregion

		#region TestDelete_UnlinksProducts

		public void TestDelete_UnlinksProducts()
		{
			var org = Helper.CreateClient();
			var cartonGroup = Helper.CreateWhsCartonGroup("G1", "Group 1");
			var product = Helper.CreateProduct("P1", org);

			var relation = product.RelatedOrganisations.FindFirstByOrganisationPK(org.PK);
			relation.OU_WCG_CartonGroup = cartonGroup.PK;

			cartonGroup.Delete();
			AssertNoExceptionThrown(Factory.Save);
			AssertEquals(ZGuid.Empty, relation.OU_WCG_CartonGroup);
			AssertEquals("Should not have deleted Product", false, product.IsDeleted);
			AssertEquals("Should not have deleted OrgMiscServ", false, relation.IsDeleted);
		}

		#endregion
	}
}
