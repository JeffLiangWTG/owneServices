using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsCartonSizeCollection))]
	class WhsCartonSizeCollectionTestCase : WhsActiveBusinessObjectCollectionTestCase<WhsCartonSizeCollection>
	{
		#region TestCollectionUsingMaster

		public void TestCollectionUsingMaster()
		{
			var group1 = Helper.CreateWhsCartonGroup("G1", "Group 1");
			var group2 = Helper.CreateWhsCartonGroup("G2", "Group 2");
			var size1 = Helper.CreateWhsCartonSize("S1");
			var size2 = Helper.CreateWhsCartonSize("S2");
			Factory.Save();

			AssertExceptionThrown<ArgumentNullException>(() => new WhsCartonSizeCollection((WhsCartonGroup)null));
			var group1_Collection = new WhsCartonSizeCollection(group1);
			var group2_Collection = new WhsCartonSizeCollection(group2);

			AssertEquals("Precondition", 0, group1_Collection.Count);
			AssertEquals("Precondition", 0, group2_Collection.Count);

			group1_Collection.Add(size1);
			AssertEquals("Should have added to Group1.", 1, group1_Collection.Count);
			AssertEquals(0, group2_Collection.Count);

			group2_Collection.Add(size1);
			AssertEquals("Should be many to many.", 1, group1_Collection.Count);
			AssertEquals("Should be many to many.", 1, group2_Collection.Count);

			group1_Collection.Add(size2);
			AssertEquals("Should be many to many.", 2, group1_Collection.Count);
			AssertEquals("Should be many to many.", 1, group2_Collection.Count);
			AssertNoExceptionThrown(Factory.Save);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var group1_InOtherFactory = factory2.Load<WhsCartonGroup>(group1.PK);
			var group2_Collection_InOtherFactory = new WhsCartonSizeCollection(group1_InOtherFactory);
			AssertEquals("Should persist the many-to-many relationship.", 2, group2_Collection_InOtherFactory.Count);
			AssertTableHitCount(1, WhsCartonSizeSchema.Constants.TableName, factory2);

			group2_Collection_InOtherFactory.RemoveFromRelationship(group2_Collection_InOtherFactory[0]);
			AssertEquals("Should support remove.", 1, group2_Collection_InOtherFactory.Count);
		}

		#endregion

		#region TestCollectionUsingMaster_Add

		public void TestCollectionUsingMaster_Add_MinimizeCartons()
		{
			var group1 = Helper.CreateWhsCartonGroup("G1", "Group 1");
			group1.OptimizationMode = CartonizationOptimizationModes.Codes.MinimizeCartons;

			var size1 = Helper.CreateWhsCartonSize("S1");
			Factory.Save();

			var group1_Collection = new WhsCartonSizeCollection(group1);

			AssertEquals("Precondition", 0, group1_Collection.Count);
			group1_Collection.Add(size1);

			var link = group1.CartonGroupSizeLinks[0];
			AssertEquals(1, link.WCV_OptimizationCost);
		}

		public void TestCollectionUsingMaster_Add_CustomOptimizationCosts()
		{
			var group1 = Helper.CreateWhsCartonGroup("G1", "Group 1");
			group1.OptimizationMode = CartonizationOptimizationModes.Codes.CustomOptimizationCosts;

			var size1 = Helper.CreateWhsCartonSize("S1");
			Factory.Save();

			var group1_Collection = new WhsCartonSizeCollection(group1);

			AssertEquals("Precondition", 0, group1_Collection.Count);
			group1_Collection.Add(size1);

			var link = group1.CartonGroupSizeLinks[0];
			AssertEquals(1, link.WCV_OptimizationCost);
		}

		public void TestCollectionUsingMaster_Add_MinimizeVolume()
		{
			var group1 = Helper.CreateWhsCartonGroup("G1", "Group 1");
			var size1 = Helper.CreateWhsCartonSize("S1");
			size1.WCS_Volume = 3.14m;
			var link1_1 = Helper.CreateWhsCartonGroupSizeLink(group1, size1);

			var group2 = Helper.CreateWhsCartonGroup("G2", "Group 2");
			group2.OptimizationMode = CartonizationOptimizationModes.Codes.MinimizeVolume;

			var size2 = Helper.CreateWhsCartonSize("S2");
			var link2_2 = Helper.CreateWhsCartonGroupSizeLink(group2, size2);
			link2_2.WCV_OptimizationCost = 1000000;
			Factory.Save();

			var group2_Collection = new WhsCartonSizeCollection(group2);

			AssertEquals("Precondition", 1, group2_Collection.Count);
			group2_Collection.Add(size1);
			AssertEquals("Precondition", 2, group2_Collection.Count);

			var link2_1 = group2.CartonGroupSizeLinks.First(l => l.WCV_WCS == size1.PK);
			AssertEquals(3140000, link2_1.WCV_OptimizationCost);

			AssertEquals(1, link1_1.WCV_OptimizationCost);
			AssertEquals(1000000, link2_2.WCV_OptimizationCost);
		}

		#endregion

		#region Implementation

		protected new WhsTestHelperFunctionsEnv Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctionsEnv(Factory)); }
		}

		WhsTestHelperFunctionsEnv helper;

		#region GetCollectionToTest

		protected override WhsCartonSizeCollection GetCollectionToTest()
		{
			return new WhsCartonSizeCollection(Factory);
		}

		#endregion 

		#endregion
	}
}
