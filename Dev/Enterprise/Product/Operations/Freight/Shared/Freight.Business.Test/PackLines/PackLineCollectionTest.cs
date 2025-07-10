using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class PackLineCollectionTest : TestCaseWithFactory
	{
		public void TestReadOnly()
		{
			var shipment = Factory.New<CommonShipment>();
			var packs = new PackLineCollection(shipment, Factory);
			AssertEquals(false, packs.ReadOnly);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValueLegacy;
			AssertEquals("HLS shipments do support packlines", false, packs.ReadOnly);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
			AssertEquals("HVL shipments do support packlines", false, packs.ReadOnly);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			shipment.CoLoadShipments.AddNew();

			AssertEquals("Precondition", true, shipment.IsMasterShipmentRepresentingAllChildShipments);
			AssertEquals("Shipments representing all child shipmetns support packlines", false, packs.ReadOnly);

			shipment.CoLoadShipments.RemoveAll();
			AssertEquals(false, packs.ReadOnly);

			shipment.SetReadOnlyIncludingChildren(true);
			AssertEquals("Readonly because of master's readonly", true, packs.ReadOnly);

			shipment.SetReadOnlyIncludingChildren(false);
			AssertEquals(false, packs.ReadOnly);

			packs.SetReadOnlyIncludingChildren(true);
			AssertEquals("Readonly because of self 'readonliness'", true, packs.ReadOnly);
		}

		public void TestMasterShipmentRepresentingAllChildShipments()
		{
			var shipment = Factory.New<CommonShipment>();
			var packs = new PackLineCollection(shipment, Factory);
			AssertEquals(false, packs.ReadOnly);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			shipment.CoLoadShipments.AddNew();
			AssertEquals("Precondition", true, shipment.IsMasterShipmentRepresentingAllChildShipments);
			AssertEquals(false, packs.ReadOnly);
			AssertEquals(false, packs.AllowNew);
			AssertEquals(false, packs.AllowRemove);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			AssertEquals("Precondition", false, shipment.IsMasterShipmentRepresentingAllChildShipments);
			AssertEquals(false, packs.ReadOnly);
			AssertEquals(true, packs.AllowNew);
			AssertEquals(true, packs.AllowRemove);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			shipment.CoLoadShipments.RemoveAll();
			AssertEquals("Precondition", false, shipment.IsMasterShipmentRepresentingAllChildShipments);
			AssertEquals(false, packs.ReadOnly);
			AssertEquals(true, packs.AllowNew);
			AssertEquals(true, packs.AllowRemove);
		}

		[ExpectNoExceptions()]
		public void TestSamePacklineOnMasterShipment()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			PackLineCollection packLines = shipment.OuterPackLines;
			PackLine packLine1 = packLines.AddNew();

			CommonShipment shipment2 = CommonShipment.New(Factory);
			shipment2.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			shipment2.CoLoadShipments.AddNew();
			shipment2.OuterPackLines.Add(packLine1);
		}

		[ExpectException(typeof(Exception))]
		public void TestSamePacklineOnAnotherShipment()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			PackLineCollection packLines = shipment.OuterPackLines;
			PackLine packLine1 = packLines.AddNew();

			CommonShipment shipment2 = CommonShipment.New(Factory);
			shipment2.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment2.OuterPackLines.Add(packLine1);
		}

		public void TestSetCollectionRelationships()
		{
			CommonShipment masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			CommonShipment childShipment = Factory.New<CommonShipment>();

			PackLine outerline = childShipment.OuterPackLines.AddNew();
			PackLine innerline = childShipment.InnerPackLines.AddNew();

			AssertEquals(outerline.JL_JS, childShipment.PK);
			AssertEquals(innerline.JL_JS, childShipment.PK);

			masterShipment.CoLoadShipments.Add(childShipment);

			AssertCollectionContains(innerline, masterShipment.InnerPackLines);
			AssertCollectionContains(outerline, masterShipment.OuterPackLines);

			AssertEquals("master shipment borrows packlines from children, shouldn't change packline's JL_JS", childShipment.PK, innerline.JL_JS);
			AssertEquals("master shipment borrows packlines from children, shouldn't change packline's JL_JS", childShipment.PK, outerline.JL_JS);
		}

		public void TestNestedAssemblies()
		{
			CommonShipment superMasterShipment = Factory.New<CommonShipment>();
			superMasterShipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			CommonShipment masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			CommonShipment childShipment1 = Factory.New<CommonShipment>();
			CommonShipment childShipment2 = Factory.New<CommonShipment>();

			PackLine outerline1 = childShipment1.OuterPackLines.AddNew();
			PackLine innerline1 = childShipment1.InnerPackLines.AddNew();

			PackLine outerline2 = childShipment2.OuterPackLines.AddNew();
			PackLine innerline2 = childShipment2.InnerPackLines.AddNew();

			masterShipment.CoLoadShipments.Add(childShipment1);
			superMasterShipment.CoLoadShipments.Add(masterShipment);
			superMasterShipment.CoLoadShipments.Add(childShipment2);

			AssertCollectionContains(innerline1, masterShipment.InnerPackLines);
			AssertCollectionContains(outerline1, masterShipment.OuterPackLines);
			AssertCollectionNotContains(innerline2, masterShipment.InnerPackLines);
			AssertCollectionNotContains(outerline2, masterShipment.OuterPackLines);

			AssertCollectionContains(innerline1, superMasterShipment.InnerPackLines);
			AssertCollectionContains(outerline1, superMasterShipment.OuterPackLines);
			AssertCollectionContains(innerline2, superMasterShipment.InnerPackLines);
			AssertCollectionContains(outerline2, superMasterShipment.OuterPackLines);
		}

		[SnailTest]
		[ExpectNoExceptions]
		public void TestBulkData()
		{
			CommonShipment masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			for (int i = 0; i < 500; i++)
			{
				CommonShipment childShipment = Factory.New<CommonShipment>();
				childShipment.InnerPackLines.AddNew();
				masterShipment.CoLoadShipments.Add(childShipment);
			}
		}

		public void TestHasDangerousGoods()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			PackLineCollection packLines = shipment.OuterPackLines;

			PackLine packLine1 = packLines.AddNew();
			AssertEquals("HasDangerousGoods should return false", false, packLines.HasDangerousGoods);

			PackLine packLine2 = packLines.AddNew();
			packLine1.UNDGs.AddNew();
			AssertEquals("HasDangerousGoods should return false", false, packLines.HasDangerousGoods);

			packLine1.UNDGs[0].DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0014", "a", "IMO").First().PK;
			AssertEquals("HasDangerousGoods should return true", true, packLines.HasDangerousGoods);
		}

		public void TestPackLineCollectionTotals()
		{
			CommonShipment dummyMaster = CommonShipment.New(Factory);
			PackLineCollection packLines = new PackLineCollection(dummyMaster, Factory);

			PackLine packLine1 = Factory.New<PackLine>();
			packLine1.JL_PackageCount = 1;
			packLine1.JL_ActualWeight = 10.1m;
			packLine1.JL_ActualVolume = 1.1m;
			packLines.Add(packLine1);

			PackLine packLine2 = Factory.New<PackLine>();
			packLine2.JL_PackageCount = 2;
			packLine2.JL_ActualWeight = 20.2m;
			packLine2.JL_ActualVolume = 2.2m;
			packLines.Add(packLine2);

			AssertEquals("Incorrect Total Count.", 3, packLines.TotalPackages);
			AssertEquals("Incorrect Total Weight.", 30.3m, packLines.TotalWeight);
			AssertEquals("Incorrect Total Volume.", 3.3m, packLines.TotalVolume);
		}

		#region Packlines Totals Units

		public void TestTotalsUnitDefaultFromMasterShipment()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_UnitOfVolume = Enterprise.Core.Constants.Volume.CubicYards;
			AssertEquals("Volume Unit", Enterprise.Core.Constants.Volume.CubicYards, shipment.InnerPackLines.Totals.TotalVolumeUnit);
			shipment.JS_UnitOfWeight = Enterprise.Core.Constants.Weight.OuncesTroy;
			AssertEquals("Weight Unit", Enterprise.Core.Constants.Weight.OuncesTroy, shipment.InnerPackLines.Totals.TotalWeightUnit);
		}

		#endregion

		public void TestLoad()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			CommonShipment anotherShipment = CommonShipment.New(Factory);

			PackLine packLine1 = Factory.New<PackLine>();
			packLine1.JL_JS = shipment.PK;

			PackLine packLine2 = Factory.New<PackLine>();
			packLine2.JL_JS = shipment.PK;

			PackLine packLine3 = Factory.New<PackLine>();
			packLine3.JL_JS = anotherShipment.PK;

			PackLineCollection collectionForShipment = new PackLineCollection(shipment, Factory);
			collectionForShipment.Load();

			AssertEquals("CollectionForShipment.Count", 2, collectionForShipment.Count);

			PackLineCollection collectionForAnotherShipment = new PackLineCollection(anotherShipment, Factory);
			collectionForAnotherShipment.Load();
			AssertEquals("CollectionForAnotherShipment.Count", 1, collectionForAnotherShipment.Count);
		}

		public void TestRemoveAndDeleteAll()
		{
			var master = Factory.New<CommonShipment>();
			master.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			var childShipment = master.CoLoadShipments.AddNew();
			childShipment.OuterPackLines.AddNew();
			childShipment.OuterPackLines.AddNew();

			AssertEquals("Precondition", 2, childShipment.OuterPackLines.Count);
			AssertContainsExactElementsInAnyOrder(childShipment.OuterPackLines, master.OuterPackLines);

			master.OuterPackLines.RemoveAndDeleteAll();
			AssertEquals("Child shipment's packlines not deleted", 2, childShipment.OuterPackLines.Count);
			AssertContainsExactElementsInAnyOrder(childShipment.OuterPackLines, master.OuterPackLines);
		}

		[ExpectNoExceptions]
		public void TestAddNew()
		{
			var dummyMaster = CommonShipment.New(Factory);
			var packLines = new PackLineCollection(dummyMaster, Factory);
			var line = packLines.AddNew();

			dummyMaster.DetailedGoodsDescriptionNoteText = "";
			dummyMaster.JS_GoodsDescription = "asdas";
			line = packLines.AddNew();
			AssertEquals(dummyMaster.JS_GoodsDescription, line.JL_Description);
			AssertEquals("", line.JL_DetailedDescription);

			dummyMaster.DetailedGoodsDescriptionNoteText = "www";
			dummyMaster.JS_GoodsDescription = "asdas";
			line = packLines.AddNew();
			AssertEquals(dummyMaster.JS_GoodsDescription, line.JL_Description);
			AssertEquals("", line.JL_DetailedDescription);

			dummyMaster.DetailedGoodsDescriptionNoteText = "0123456789012345678901234567890123456789";
			dummyMaster.JS_GoodsDescription = "01234567890123456789012345678901234";
			line = packLines.AddNew();
			AssertEquals(dummyMaster.DetailedGoodsDescriptionNoteText, line.JL_Description);
			AssertEquals("", line.JL_DetailedDescription);

			dummyMaster.DetailedGoodsDescriptionNoteText = "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
				"0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
				"0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
				"0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
				"0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
				"01234567890123456789";
			dummyMaster.JS_GoodsDescription = "01234567890123456789012345678901234";
			line = packLines.AddNew();
			AssertEquals(dummyMaster.JS_GoodsDescription, line.JL_Description);
			AssertEquals(dummyMaster.DetailedGoodsDescriptionNoteText, line.JL_DetailedDescription);

			dummyMaster.DetailedGoodsDescriptionNoteText = "こんにちは"; // text has non-western european charaters
			dummyMaster.JS_GoodsDescription = ZString.Empty;
			line = packLines.AddNew();
			AssertEquals(string.Empty, line.JL_Description);
			AssertEquals(ZString.Empty, line.JL_DetailedDescription);

			dummyMaster.DetailedGoodsDescriptionNoteText = "こabちcはd?"; //text contains non-western european charaters
			dummyMaster.JS_GoodsDescription = ZString.Empty;
			line = packLines.AddNew();
			AssertEquals("abcd?", line.JL_Description);
			AssertEquals("", line.JL_DetailedDescription);

			dummyMaster.DetailedGoodsDescriptionNoteText = "01234567890こちは12345678901234567888888888888888889014545454545452こちはd3456789" + ZString.Replicate('A', 500); //text contains non-western european charaters
			dummyMaster.JS_GoodsDescription = "01234567890123456789012345678901234";
			line = packLines.AddNew();
			AssertEquals(dummyMaster.JS_GoodsDescription, line.JL_Description);
			AssertEquals("0123456789012345678901234567888888888888888889014545454545452d3456789AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", line.JL_DetailedDescription);
		}

		public void TestEnsurePackLineExistsDoesNotCreateExceptionTryingToAddPackLineForMasterShipment()
		{
			var masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			var subShipment = masterShipment.CoLoadShipments.AddNew();
			subShipment.JS_TotalPackageCount = 20;

			AssertEquals("Expected to have created inner pack line as total package count > 0", 1, subShipment.InnerPackLines.Count);
			AssertEquals("Expected master to have defaulted an inner pack line from its sub", 1, masterShipment.InnerPackLines.Count);
			AssertEquals("Expected master inner pack line to be that of its sub", subShipment.InnerPackLines[0], masterShipment.InnerPackLines[0]);

			AssertEquals("Expected not to have any outer pack lines", 0, subShipment.OuterPackLines.Count);
			AssertEquals("Expected not to have any outer pack lines", 0, masterShipment.OuterPackLines.Count);

			masterShipment.JS_TotalPackageCount = 35;
			masterShipment.JS_ActualVolume = 5000m;
			masterShipment.UpdatingShipmentVolumeFromPacks += delegate(object sender, CancelEventArgs e)
			{ e.Cancel = false; };
			masterShipment.SyncMeasuresWithInnerPackLines();

			AssertNoExceptionThrown("SyncMeasuresWithInnerPackLines should not cause an exception trying to add a new outer pack line to master shipment", Factory.Save);
		}

		public void TestShipmentDescriptionChanges()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.JS_GoodsDescription = "111";
			PackLineCollection packLines = shipment.OuterPackLines;
			PackLine line = packLines.AddNew();

			shipment.DetailedGoodsDescriptionNoteText = "";
			shipment.JS_GoodsDescription = "asdas";
			AssertEquals(shipment.JS_GoodsDescription, line.JL_Description);
			AssertEquals("", line.JL_DetailedDescription);

			shipment.DetailedGoodsDescriptionNoteText = "www";
			shipment.JS_GoodsDescription = "asdas";
			AssertEquals(shipment.JS_GoodsDescription, line.JL_Description);
			AssertEquals("", line.JL_DetailedDescription);

			shipment.DetailedGoodsDescriptionNoteText = "0123456789012345678901234567890123456789";
			shipment.JS_GoodsDescription = "01234567890123456789012345678901234";
			AssertEquals(shipment.DetailedGoodsDescriptionNoteText, line.JL_Description);
			AssertEquals("", line.JL_DetailedDescription);

			shipment.DetailedGoodsDescriptionNoteText = "0123456789012345678901234567890123456789012345678901234567890123456789";
			shipment.JS_GoodsDescription = "01234567890123456789012345678901234";
			AssertEquals("0123456789012345678901234567890123456789", line.JL_Description);
			AssertEquals("", line.JL_DetailedDescription);

			var description = "B123456789012345678901234567890123456789012345678901234567890123456789".PadRight(513, 'A');
			line.JL_Description = "01234567890123456789012345678901234";
			shipment.DetailedGoodsDescriptionNoteText = description;
			shipment.JS_GoodsDescription = "B1234567890123456789012345678901234";
			AssertEquals(shipment.JS_GoodsDescription, line.JL_Description);
			AssertEquals(shipment.DetailedGoodsDescriptionNoteText, line.JL_DetailedDescription);

			shipment.DetailedGoodsDescriptionNoteText = description;
			shipment.JS_GoodsDescription = "A1234567890123456789012345678901234";
			AssertEquals(shipment.JS_GoodsDescription, line.JL_Description);
			AssertEquals(description, line.JL_DetailedDescription);

			line.JL_Description = "222";
			shipment.DetailedGoodsDescriptionNoteText = "sss";
			shipment.JS_GoodsDescription = "asdas";
			AssertEquals("222", line.JL_Description);
			AssertEquals(description, line.JL_DetailedDescription);
		}

		public void TestAddNewDefaults()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_GoodsDescription = "hello";
			AssertEquals("Precondition", false, shipment.ShipmentInnerPacksUnit.IsEmpty);
			AssertEquals("Precondition", false, shipment.ShipmentWeightUnit.IsEmpty);
			AssertEquals("Precondition", false, shipment.ShipmentVolumeUnit.IsEmpty);
			AssertEquals("Precondition", "hello", shipment.JS_GoodsDescription);

			PackLine packLine = new PackLineCollection(shipment, Factory).AddNew();
			AssertEquals(shipment.ShipmentInnerPacksUnit, packLine.JL_F3_NKPackType);
			AssertEquals(shipment.ShipmentWeightUnit, packLine.JL_ActualWeightUQ);
			AssertEquals(shipment.ShipmentVolumeUnit, packLine.JL_ActualVolumeUQ);
			AssertEquals("hello", packLine.JL_Description);
		}

		#region NotifyChanged

		public void TestNotifyWeightChanged()
		{
			PackLineCollection packLines = new PackLineCollection(Factory.New<CommonShipment>(), Factory);
			AssertNotifyMeasureChanged(packLines, JobPackLinesSchema.JL_ActualWeight.Name, packLines.NotifyWeightChanged);
		}

		public void TestNotifyWeightUnitChanged()
		{
			PackLineCollection packLines = new PackLineCollection(Factory.New<CommonShipment>(), Factory);
			AssertNotifyMeasureUnitChanged(packLines, JobPackLinesSchema.JL_ActualWeightUQ.Name, packLines.NotifyWeightUQChanged);
		}

		public void TestNotifyVolumeChanged()
		{
			PackLineCollection packLines = new PackLineCollection(Factory.New<CommonShipment>(), Factory);
			AssertNotifyMeasureChanged(packLines, JobPackLinesSchema.JL_ActualVolume.Name, packLines.NotifyVolumeChanged);
		}

		public void TestNotifyVolumeUnitChanged()
		{
			PackLineCollection packLines = new PackLineCollection(Factory.New<CommonShipment>(), Factory);
			AssertNotifyMeasureUnitChanged(packLines, JobPackLinesSchema.JL_ActualVolumeUQ.Name, packLines.NotifyVolumeUQChanged);
		}

		public void TestNotifyPackageTypeChanged()
		{
			PackLineCollection packLines = new PackLineCollection(Factory.New<CommonShipment>(), Factory);
			AssertNotifyMeasureUnitChanged(packLines, JobPackLinesSchema.JL_F3_NKPackType.Name, packLines.NotifyPackageTypeChanged);
		}

		public void TestNotifyLoadingMetersChanged()
		{
			PackLineCollection packLines = new PackLineCollection(Factory.New<CommonShipment>(), Factory);
			AssertNotifyMeasureChanged(packLines, JobPackLinesSchema.JL_LoadingMeters.Name, packLines.NotifyLoadingMetersChanged);
		}

		void AssertNotifyMeasureChanged(PackLineCollection packLines, string propertyName, Action<ZDecimal, ZDecimal> notifyChanged)
		{
			AssertEquals("Precondition: collection empty", 0, packLines.Count);
			notifyChanged(10m, 20m);

			AssertEquals("New packline added", 1, packLines.Count);
			AssertEquals("Measure changed", 20m, packLines[0][propertyName]);

			notifyChanged(20m, 30m);
			AssertEquals("No new packlines", 1, packLines.Count);
			AssertEquals("Measure changed", 30m, packLines[0][propertyName]);

			packLines.AddNew();
			notifyChanged(30, 40);
			AssertEquals("More than one packline - measure not changed", 30m, packLines[0][propertyName]);
		}

		void AssertNotifyMeasureUnitChanged(PackLineCollection packLines, string propertyName, Action<ZString, ZString> notifyChanged)
		{
			AssertEquals("Precondition: collection empty", 0, packLines.Count);
			notifyChanged("AA", "BB");

			AssertEquals("New packline added", 1, packLines.Count);
			AssertNotEquals("Some default unit, not changed to new one", "BB", packLines[0][propertyName]);

			packLines.AddNew();
			packLines.AddNew();
			packLines[0][propertyName] = "BB";
			packLines[1][propertyName] = "AA";
			packLines[2][propertyName] = "CC";

			notifyChanged("AA", "BB");
			AssertEquals("BB", packLines[0][propertyName]);
			AssertEquals("BB", packLines[1][propertyName]);
			AssertEquals("CC", packLines[2][propertyName]);
		}

		#endregion
	}
}
