using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(CoLoadShipmentCollection))]
	public class ColoadShipmentCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Master Shipments Pack Lines

		public void TestMasterShipmentPackLinesDeleted()
		{
			CommonShipment superMasterShipment = Factory.New<CommonShipment>();
			superMasterShipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			superMasterShipment.JS_HouseBill = "Super Master";
			PackLine superMasterPackLine = superMasterShipment.OuterPackLines.AddNew();
			superMasterPackLine.JL_PackageCount = 401;
			AssertEquals("Precondition", 1, superMasterShipment.OuterPackLines.Count);

			CommonShipment masterShipment1 = Factory.New<CommonShipment>();
			masterShipment1.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			masterShipment1.JS_HouseBill = "Master 1";
			PackLine masterPackLine1 = masterShipment1.OuterPackLines.AddNew();
			masterPackLine1.JL_PackageCount = 300;
			AssertEquals("Precondition", 1, masterShipment1.OuterPackLines.Count);

			CommonShipment subShipment1 = Factory.New<CommonShipment>();
			subShipment1.JS_HouseBill = "Sub 1";
			PackLine subShipment1PackLine = subShipment1.OuterPackLines.AddNew();
			subShipment1PackLine.JL_PackageCount = 209;

			CommonShipment subShipment2 = Factory.New<CommonShipment>();
			subShipment2.JS_HouseBill = "Sub 2";
			PackLine subShipment2PackLine = subShipment2.OuterPackLines.AddNew();
			subShipment2PackLine.JL_PackageCount = 113;

			CommonShipment subShipment3 = Factory.New<CommonShipment>();
			subShipment3.JS_HouseBill = "Sub 3";
			PackLine subShipment3PackLine = subShipment3.OuterPackLines.AddNew();
			subShipment3PackLine.JL_PackageCount = 249;

			masterShipment1.CoLoadShipments.Add(subShipment1);
			AssertEquals(1, masterShipment1.OuterPackLines.Count);
			AssertEquals(subShipment1PackLine, masterShipment1.OuterPackLines[0]);
			AssertEquals("Master shipment pack line itself is deleted", true, masterPackLine1.IsDeleted);

			superMasterShipment.CoLoadShipments.Add(masterShipment1);
			AssertEquals(1, superMasterShipment.OuterPackLines.Count);
			AssertEquals(subShipment1PackLine, superMasterShipment.OuterPackLines[0]);
			AssertEquals(subShipment1PackLine, masterShipment1.OuterPackLines[0]);
			AssertEquals("Super master shipment pack line itself is deleted", true, superMasterPackLine.IsDeleted);

			superMasterShipment.CoLoadShipments.Add(subShipment3);
			AssertEquals(2, superMasterShipment.OuterPackLines.Count);
			AssertCollectionContains(subShipment1PackLine, superMasterShipment.OuterPackLines);
			AssertCollectionContains(subShipment3PackLine, superMasterShipment.OuterPackLines);

			masterShipment1.CoLoadShipments.Add(subShipment2);
			AssertEquals(3, superMasterShipment.OuterPackLines.Count);
			AssertCollectionContains(subShipment1PackLine, superMasterShipment.OuterPackLines);
			AssertCollectionContains(subShipment2PackLine, superMasterShipment.OuterPackLines);
			AssertCollectionContains(subShipment3PackLine, superMasterShipment.OuterPackLines);
			AssertEquals(2, masterShipment1.OuterPackLines.Count);
			AssertCollectionContains(subShipment1PackLine, masterShipment1.OuterPackLines);
			AssertCollectionContains(subShipment2PackLine, masterShipment1.OuterPackLines);
			AssertEquals("Super master shipment packline itself is still deleted", true, superMasterPackLine.IsDeleted);
			AssertEquals("Master shipment pack line itself is still deleted", true, masterPackLine1.IsDeleted);
			AssertEquals("Sub shipment pack-lines are not touched", false, subShipment1PackLine.IsDeleted);
			AssertEquals("Sub shipment pack-lines are not touched", false, subShipment2PackLine.IsDeleted);
			AssertEquals("Sub shipment pack-lines are not touched", false, subShipment3PackLine.IsDeleted);
		}

		public void TestInnerAndOuterPackLinesOnMasterShipmentsDeleted()
		{
			var masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_TotalPackageCount = 20;
			masterShipment.JS_ActualVolume = 5000m;

			AssertEquals("Expected to have created outer pack line", 1, masterShipment.OuterPackLines.Count);
			AssertEquals("Expected to have created inner pack line", 1, masterShipment.InnerPackLines.Count);

			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			AssertEquals("Expected to have deleted outer pack line", 0, masterShipment.OuterPackLines.Count);
			AssertEquals("Expected to have deleted inner pack line", 0, masterShipment.InnerPackLines.Count);

			masterShipment.JS_TotalPackageCount = 15;
			masterShipment.JS_ActualVolume = 3000m;

			AssertEquals("Expected to once again add outer pack as collection is not read only", 1, masterShipment.OuterPackLines.Count);
			AssertEquals("Expected to once again add inner pack as collection is not read only", 1, masterShipment.InnerPackLines.Count);

			masterShipment.CoLoadShipments.AddNew();

			AssertEquals("Expected to have deleted outer pack line now master has sub shipment", 0, masterShipment.OuterPackLines.Count);
			AssertEquals("Expected to have deleted inner pack line now master has sub shipment", 0, masterShipment.InnerPackLines.Count);
		}

		public void TestPacklinesWouldNotBeSetAsReadonlyWhenAddToMasterShipment()
		{
			var subShipment = Factory.New<CommonShipment>();
			subShipment.JS_HouseBill = "Sub 1";
			var packLine1 = subShipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 209;
			var packLine2 = subShipment.InnerPackLines.AddNew();
			packLine1.JL_PackageCount = 210;

			var masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			masterShipment.JS_HouseBill = "Master 1";
			masterShipment.IsRoot = true;

			masterShipment.CoLoadShipments.Add(subShipment);

			AssertEquals(1, masterShipment.OuterPackLines.Count);
			AssertEquals(1, masterShipment.InnerPackLines.Count);
			Assert(!masterShipment.OuterPackLines.ReadOnly);
			Assert(!masterShipment.InnerPackLines.ReadOnly);

			Assert(!subShipment.OuterPackLines.ReadOnly);
			Assert(!subShipment.InnerPackLines.ReadOnly);
			Assert("packLine1 should not be readonly", !packLine1.ReadOnly);
			Assert("packLine2 should not be readonly", !packLine2.ReadOnly);
		}

		#endregion

		[ExpectNoExceptions()]
		public void TestInvalidUnitOnNewShipment()
		{
			CommonShipment masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			CommonShipment newShipment = Factory.New<CommonShipment>();
			newShipment.JS_ActualVolume = 20m;
			newShipment.JS_ActualWeight = 30m;

			newShipment.JS_UnitOfWeight = "20";
			newShipment.JS_UnitOfVolume = "30";

			masterShipment.CoLoadShipments.Add(newShipment);
		}

		#region Performance

		public void TestLoadingSubsDoesntUpdateZeroMasterValues()
		{
			CommonShipment masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			masterShipment.CoLoadShipments.AddNew().JS_ActualVolume = 11m;
			masterShipment.CoLoadShipments.AddNew().JS_ActualVolume = 8m;

			Factory.Save();
			AssertEquals(19m, masterShipment.JS_ActualVolume);

			masterShipment.JS_ActualVolume = 0m;
			Factory.Save();

			masterShipment = new BusinessObjectFactory().Load<CommonShipment>(masterShipment.PK);
			AssertEquals(2, masterShipment.CoLoadShipments.Count);
			AssertEquals(0m, masterShipment.JS_ActualVolume);
		}

		public void TestLoadSubsPerformance()
		{
			var shipmentMaster = Factory.New<CommonShipment>();
			shipmentMaster.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			for (int i = 0; i < 20; i++)
			{
				var shipmentSub = shipmentMaster.CoLoadShipments.AddNew();
				for (int j = 0; j < 20; j++)
				{
					shipmentSub.InnerPackLines.AddNew();
					shipmentSub.OuterPackLines.AddNew();
				}
			}

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			shipmentMaster = newFactory.Load<CommonShipment>(shipmentMaster.PK);
			AssertEquals(20, shipmentMaster.CoLoadShipments.Count);
			AssertMaxDbHits(46, newFactory);
			AssertEquals(400, shipmentMaster.InnerPackLines.Count);
			AssertEquals(400, shipmentMaster.OuterPackLines.Count);
			AssertMaxDbHits(47, newFactory);
		}

		#endregion

		public void TestLoadingShipmentWithSubsDoesntSetHasChanges()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			CommonShipment sub = shipment.CoLoadShipments.AddNew();
			sub.JS_ActualVolume = 23;
			sub.OuterPackLines.AddNew().JL_ActualVolume = 11;
			sub.InnerPackLines.AddNew().JL_ActualVolume = 11;

			Factory.Save();

			shipment = new BusinessObjectFactory().Load<CommonShipment>(shipment.PK);
			AssertEquals("loading a shipment with subs shouldn't set has changes on it or its packs", false, shipment.HasChanges);
			AssertEquals("loading a shipment with subs shouldn't set has changes on it or its packs", false, shipment.InnerPackLines.HasChanges);
			AssertEquals("loading a shipment with subs shouldn't set has changes on it or its packs", false, shipment.OuterPackLines.HasChanges);
		}

		public void TestSettingUnitsRefreshesValueOnMaster()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			CommonShipment sub1 = shipment.CoLoadShipments.AddNew();
			CommonShipment sub2 = shipment.CoLoadShipments.AddNew();

			sub1.JS_ActualVolume = 13;
			sub2.JS_ActualVolume = 22;

			sub1.JS_ActualWeight = 10;
			sub2.JS_ActualWeight = 20;

			AssertEquals(35m, shipment.JS_ActualVolume);
			AssertEquals(30m, shipment.JS_ActualWeight);

			AssertEquals(sub2.JS_UnitOfVolume, shipment.JS_UnitOfVolume);
			AssertEquals(sub2.JS_UnitOfWeight, shipment.JS_UnitOfWeight);
		}

		public void TestAddingNewShipmentDoesntChangePacklineOnAnExistingRelatedShipment()
		{
			CommonShipment masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			masterShipment.JS_UniqueConsignRef = "master";
			CommonShipment subShipment1 = masterShipment.CoLoadShipments.AddNew();
			subShipment1.JS_UniqueConsignRef = "sub1";
			PackLine innerPackLine = subShipment1.InnerPackLines.AddNew();
			innerPackLine.JL_ActualWeight = 11m;
			innerPackLine.JL_ActualVolume = 11m;

			subShipment1.UpdateShipmentFromInnerPackLines();

			PackLine outerPackLine = subShipment1.OuterPackLines.AddNew();
			outerPackLine.JL_ActualVolume = 12m;
			outerPackLine.JL_ActualWeight = 12m;
			subShipment1.UpdateShipmentFromOuterPackLines();
			Factory.Save();

			AssertEquals("Weight was not updated.", 12m, subShipment1.JS_ActualWeight);
			AssertEquals("Volume was not updated.", 12m, subShipment1.JS_ActualVolume);
			AssertEquals(11m, subShipment1.InnerPackLines[0].JL_ActualVolume);
			AssertEquals(11m, subShipment1.InnerPackLines[0].JL_ActualWeight);

			CommonShipment subShipment2 = Factory.New<CommonShipment>();
			subShipment2.JS_ActualWeight = 3m;
			subShipment2.JS_ActualVolume = 3m;

			masterShipment.CoLoadShipments.Add(subShipment2);

			AssertEquals(12m, subShipment1.JS_ActualWeight);
			AssertEquals(12m, subShipment1.JS_ActualVolume);
			AssertEquals(11m, subShipment1.InnerPackLines[0].JL_ActualVolume);
			AssertEquals(11m, subShipment1.InnerPackLines[0].JL_ActualWeight);
		}

		public void TestColoadMasterTotals_WeightWithAddRange()
		{
			var masterShipment = Factory.New<CommonShipment>();
			var childShipment1 = Factory.New<CommonShipment>();
			var childShipment2 = Factory.New<CommonShipment>();
			var childShipment3 = Factory.New<CommonShipment>();
			var childShipment4 = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			childShipment1.JS_ActualWeight = 7;
			childShipment2.JS_ActualWeight = 4;
			childShipment3.JS_ActualWeight = 5;
			childShipment4.JS_ActualWeight = 5;

			var businessObjects = new List<BusinessObject>();
			businessObjects.Add(childShipment1);
			businessObjects.Add(childShipment2);

			masterShipment.CoLoadShipments.AddRange(businessObjects);

			AssertEquals(11m, masterShipment.JS_ActualWeight);

			businessObjects.Clear();
			businessObjects.Add(childShipment3);
			businessObjects.Add(childShipment4);

			masterShipment.CoLoadShipments.AddRange(businessObjects);

			AssertEquals(21m, masterShipment.JS_ActualWeight);

			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			AssertEquals(0m, masterShipment.JS_ActualWeight);
		}

		public void TestColoadMasterTotals_GoodsAndCurrencyWithRemoveRange()
		{
			CommonShipment masterShipment = Factory.New<CommonShipment>();
			CommonShipment childShipment1 = Factory.New<CommonShipment>();
			CommonShipment childShipment2 = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			childShipment1.JS_GoodsValue = 700;
			childShipment2.JS_GoodsValue = 400;

			RefCurrency curr1 = Factory.New<RefCurrency>();
			curr1.RX_Code = "ART";
			RefExchangeRate rate1 = curr1.ExchangeRates.AddNew();
			rate1.RE_ExRateType = "SEL";
			rate1.RE_SellRate = 2;
			rate1.RE_StartDate = ZDateTime.Now.AddDays(-1);
			rate1.RE_ExpiryDate = ZDateTime.Now.AddDays(2);

			RefCurrency curr2 = Factory.New<RefCurrency>();
			curr2.RX_Code = "XXX";
			RefExchangeRate rate2 = curr2.ExchangeRates.AddNew();
			rate2.RE_ExRateType = "SEL";
			rate2.RE_SellRate = 10;
			rate2.RE_StartDate = ZDateTime.Now.AddDays(-1);
			rate2.RE_ExpiryDate = ZDateTime.Now.AddDays(2);

			childShipment1.JS_RX_NKGoodsValueCurr = curr1.RX_Code;
			childShipment2.JS_RX_NKGoodsValueCurr = curr2.RX_Code;

			masterShipment.CoLoadShipments.Add(childShipment1);
			masterShipment.CoLoadShipments.Add(childShipment2);

			AssertEquals(390m, masterShipment.JS_GoodsValue);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, masterShipment.JS_RX_NKGoodsValueCurr);

			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			AssertEquals(0m, masterShipment.JS_GoodsValue);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, masterShipment.JS_RX_NKGoodsValueCurr);
		}

		public void TestColoadMasterTotals_RefreshBindingsWithAddRange()
		{
			var masterShipment = Factory.New<CommonShipment>();
			var childShipment1 = Factory.New<CommonShipment>();
			var childShipment2 = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			var valueChangedTriggered = false;
			masterShipment.JS_UnitOfWeightInfo.ValueChanged += (sender, args) => { valueChangedTriggered = true; };

			var businessObjects = new List<BusinessObject>();
			businessObjects.Add(childShipment1);
			businessObjects.Add(childShipment2);

			masterShipment.CoLoadShipments.AddRange(businessObjects);

			Assert(valueChangedTriggered);
		}

		public void TestColoadMasterTotals_RefreshTotalsValidationWithAddRange()
		{
			var masterShipment = Factory.New<CommonShipment>();
			var childShipment1 = Factory.New<CommonShipment>();
			var childShipment2 = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			childShipment1.JS_ActualWeight = -7;
			childShipment2.JS_ActualWeight = -4;

			var businessObjects = new List<BusinessObject>();
			businessObjects.Add(childShipment1);
			businessObjects.Add(childShipment2);

			masterShipment.CoLoadShipments.AddRange(businessObjects);

			Assert(masterShipment.HasErrors);
		}

		#region Coload Master Totals

		#region Weight

		public void TestColoadMasterTotals_Weight()
		{
			CommonShipment masterShipment = Factory.New<CommonShipment>();
			CommonShipment childShipment1 = Factory.New<CommonShipment>();
			CommonShipment childShipment2 = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			childShipment1.JS_ActualWeight = 7;
			childShipment2.JS_ActualWeight = 4;
			masterShipment.CoLoadShipments.Add(childShipment1);
			masterShipment.CoLoadShipments.Add(childShipment2);

			AssertEquals(11m, masterShipment.JS_ActualWeight);

			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			AssertEquals(0m, masterShipment.JS_ActualWeight);
		}

		public void TestColoadMasterTotals_Weight_NotWithinSqlPrecisionAndScale()
		{
			CommonShipment masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			using (masterShipment.GetValidationSuspender())
			{
				CommonShipment childShipment1 = Factory.New<CommonShipment>();
				childShipment1.JS_ActualWeight = 700000m;
				childShipment1.JS_UnitOfWeight = Constants.Weight.Kilograms;
				childShipment1.JS_JS_ColoadMasterShipment = masterShipment.PK;

				AssertEquals(masterShipment.JS_ActualWeight, 700000m);
				AssertEquals(Constants.Weight.Kilograms, masterShipment.JS_UnitOfWeight);

				CommonShipment childShipment2 = Factory.New<CommonShipment>();
				childShipment2.JS_ActualWeight = 400000m;
				childShipment2.JS_UnitOfWeight = Constants.Weight.Kilograms;
				childShipment2.JS_JS_ColoadMasterShipment = masterShipment.PK;

				AssertEquals("Should change to Tonnes", Constants.Weight.Tonnes, masterShipment.JS_UnitOfWeight);
				AssertEquals("Actual weight in Tonnes", 1100m, masterShipment.JS_ActualWeight);
			}
		}

		public void TestColoadMasterTotals_WeightAndChargeableErrors()
		{
			var masterShipment = Factory.New<CommonShipment>();
			var subShipment1 = Factory.New<CommonShipment>();
			var subShipment2 = Factory.New<CommonShipment>();

			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			masterShipment.JS_TransportMode = Constants.TransportModes.Air;
			subShipment1.JS_TransportMode = Constants.TransportModes.Air;
			subShipment2.JS_TransportMode = Constants.TransportModes.Air;

			subShipment1.JS_ActualWeight = 777777.777;
			subShipment1.JS_JS_ColoadMasterShipment = masterShipment.PK;

			AssertNoErrors("Pre-condition", masterShipment.JS_ActualWeightInfo);
			AssertNoErrors("Pre-condition", masterShipment.JS_ActualChargeableInfo);

			Factory.Save();

			subShipment2.JS_JS_ColoadMasterShipment = masterShipment.PK;
			subShipment2.JS_ActualWeight = 999999.999;

			AssertHasErrors("Expected an error as total weight of combined sub-shipments is too large", masterShipment.JS_ActualWeightInfo);
			AssertHasErrors("Expected an error as total chargeable of combined sub-shipments is too large", masterShipment.JS_ActualChargeableInfo);
			AssertHasErrors("Expected an error as master has errors", subShipment2.JS_JS_ColoadMasterShipmentInfo);
		}

		public void TestColoadMasterTotals_Weight_DifferentUnits()
		{
			Env.Registry.FreightWeightUnit = Constants.Weight.Kilograms;
			CommonShipment masterShipment = Factory.New<CommonShipment>();
			CommonShipment childShipment1 = Factory.New<CommonShipment>();
			CommonShipment childShipment2 = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			childShipment1.JS_ActualWeight = 700;
			childShipment1.JS_UnitOfWeight = Constants.Weight.Milligrams;
			childShipment2.JS_ActualWeight = 4;
			childShipment2.JS_UnitOfWeight = Constants.Weight.Pounds;

			masterShipment.CoLoadShipments.Add(childShipment1);
			masterShipment.CoLoadShipments.Add(childShipment2);

			AssertEquals(1.815m, masterShipment.JS_ActualWeight);
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			AssertEquals(0m, masterShipment.JS_ActualWeight);
			AssertEquals(Constants.Weight.Kilograms, masterShipment.JS_UnitOfWeight);
		}

		#endregion

		#region Volume

		public void TestColoadMasterTotals_Volume()
		{
			CommonShipment mother = Factory.New<CommonShipment>();
			CommonShipment kid1 = Factory.New<CommonShipment>();
			CommonShipment kid2 = Factory.New<CommonShipment>();
			mother.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			kid1.JS_ActualVolume = 7;
			kid2.JS_ActualVolume = 4;

			mother.CoLoadShipments.Add(kid1);
			mother.CoLoadShipments.Add(kid2);

			AssertEquals(11m, mother.JS_ActualVolume);
			mother.CoLoadShipments.Remove(kid1);
			mother.CoLoadShipments.Remove(kid2);
			AssertEquals(0m, mother.JS_ActualVolume);
		}

		public void TestColoadMasterTotals_Volume_NotWithinSqlPrecisionAndScale()
		{
			CommonShipment masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			using (masterShipment.GetValidationSuspender())
			{
				CommonShipment childShipment1 = Factory.New<CommonShipment>();
				childShipment1.JS_ActualVolume = 700000m;
				childShipment1.JS_UnitOfVolume = Constants.Volume.Litre;
				childShipment1.JS_JS_ColoadMasterShipment = masterShipment.PK;

				AssertEquals(masterShipment.JS_ActualVolume, 700000m);
				AssertEquals(Constants.Volume.Litre, masterShipment.JS_UnitOfVolume);

				CommonShipment childShipment2 = Factory.New<CommonShipment>();
				childShipment2.JS_ActualVolume = 400000m;
				childShipment2.JS_UnitOfVolume = Constants.Volume.Litre;
				childShipment2.JS_JS_ColoadMasterShipment = masterShipment.PK;

				AssertEquals("Should change to CubicMetres", Constants.Volume.CubicMetres, masterShipment.JS_UnitOfVolume);
				AssertEquals("Actual weight in CubicMetres", 1100m, masterShipment.JS_ActualVolume);
			}
		}

		public void TestColoadMasterTotals_VolumeAndChargeableErrors()
		{
			var masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			var subShipment1 = Factory.New<CommonShipment>();
			var subShipment2 = Factory.New<CommonShipment>();

			subShipment1.JS_ActualVolume = 444444.444;
			subShipment1.JS_JS_ColoadMasterShipment = masterShipment.PK;

			AssertNoErrors("Pre-condition", masterShipment.JS_ActualVolumeInfo);
			AssertNoErrors("Pre-condition", masterShipment.JS_ActualChargeableInfo);

			Factory.Save();

			subShipment2.JS_ActualVolume = 888888.888;
			subShipment2.JS_JS_ColoadMasterShipment = masterShipment.PK;

			AssertHasErrors("Expected an error as total volume of combined sub-shipments is too large", masterShipment.JS_ActualVolumeInfo);
			AssertHasErrors("Expected an error as total chargeable of combined sub-shipments is too large", masterShipment.JS_ActualChargeableInfo);
			AssertHasErrors("Expected an error as master has errors", subShipment2.JS_JS_ColoadMasterShipmentInfo);
		}

		public void TestColoadMasterTotals_Volume_DifferentUnits()
		{
			Env.Registry.FreightVolumeUnit = Constants.Volume.CubicMetres;
			CommonShipment mother = Factory.New<CommonShipment>();
			CommonShipment kid1 = Factory.New<CommonShipment>();
			CommonShipment kid2 = Factory.New<CommonShipment>();
			mother.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			kid1.JS_ActualVolume = 700;
			kid1.JS_UnitOfVolume = Constants.Volume.Litre;
			kid2.JS_ActualVolume = 40;
			kid2.JS_UnitOfVolume = Constants.Volume.MegaLitre;

			mother.CoLoadShipments.Add(kid1);
			mother.CoLoadShipments.Add(kid2);

			AssertEquals(40000.7m, mother.JS_ActualVolume);
			mother.CoLoadShipments.Remove(kid1);
			mother.CoLoadShipments.Remove(kid2);
			AssertEquals(0m, mother.JS_ActualVolume);
			AssertEquals(Constants.Volume.CubicMetres, mother.JS_UnitOfVolume);
		}

		#endregion

		#region Outer Packs

		public void TestColoadMasterTotals_OuterPacks()
		{
			CommonShipment mother = Factory.New<CommonShipment>();
			CommonShipment kid1 = Factory.New<CommonShipment>();
			CommonShipment kid2 = Factory.New<CommonShipment>();

			mother.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			kid1.JS_OuterPacks = 7;
			kid2.JS_OuterPacks = 4;

			mother.CoLoadShipments.Add(kid1);
			mother.CoLoadShipments.Add(kid2);

			AssertEquals(11, mother.JS_OuterPacks);
			mother.CoLoadShipments.Remove(kid1);
			mother.CoLoadShipments.Remove(kid2);
			AssertEquals(0, mother.JS_OuterPacks);
		}

		public void TestColoadMasterTotals_OuterPacks_DifferentUnits()
		{
			CommonShipment mother = Factory.New<CommonShipment>();
			CommonShipment kid1 = Factory.New<CommonShipment>();
			CommonShipment kid2 = Factory.New<CommonShipment>();

			mother.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			kid1.JS_OuterPacks = 7;
			kid1.JS_F3_NKPackType = "PLT";
			kid2.JS_OuterPacks = 4;
			kid2.JS_F3_NKPackType = "BOX";

			mother.CoLoadShipments.Add(kid1);
			mother.CoLoadShipments.Add(kid2);

			AssertEquals(11, mother.JS_OuterPacks);
			mother.CoLoadShipments.Remove(kid1);
			mother.CoLoadShipments.Remove(kid2);
			AssertEquals(0, mother.JS_OuterPacks);
			AssertEquals("PKG", mother.JS_F3_NKPackType);
		}

		#endregion

		#region Goods Value

		public void TestColoadMasterTotals_GoodsValue()
		{
			CommonShipment mother = Factory.New<CommonShipment>();
			CommonShipment kid1 = Factory.New<CommonShipment>();
			CommonShipment kid2 = Factory.New<CommonShipment>();
			mother.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			kid1.JS_GoodsValue = 7;
			kid2.JS_GoodsValue = 4;

			mother.CoLoadShipments.Add(kid1);
			mother.CoLoadShipments.Add(kid2);

			AssertEquals(11m, mother.JS_GoodsValue);
			mother.CoLoadShipments.Remove(kid1);
			mother.CoLoadShipments.Remove(kid2);
			AssertEquals(0m, mother.JS_GoodsValue);
		}

		public void TestColoadMasterTotals_GoodsValue_DifferentCurrency()
		{
			CommonShipment mother = Factory.New<CommonShipment>();
			CommonShipment kid1 = Factory.New<CommonShipment>();
			CommonShipment kid2 = Factory.New<CommonShipment>();
			mother.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			kid1.JS_GoodsValue = 700;
			kid2.JS_GoodsValue = 400;

			RefCurrency curr1 = Factory.New<RefCurrency>();
			curr1.RX_Code = "ART";
			RefExchangeRate rate1 = curr1.ExchangeRates.AddNew();
			rate1.RE_ExRateType = "SEL";
			rate1.RE_SellRate = 2;
			rate1.RE_StartDate = ZDateTime.Now.AddDays(-1);
			rate1.RE_ExpiryDate = ZDateTime.Now.AddDays(2);

			RefCurrency curr2 = Factory.New<RefCurrency>();
			curr2.RX_Code = "XXX";
			RefExchangeRate rate2 = curr2.ExchangeRates.AddNew();
			rate2.RE_ExRateType = "SEL";
			rate2.RE_SellRate = 10;
			rate2.RE_StartDate = ZDateTime.Now.AddDays(-1);
			rate2.RE_ExpiryDate = ZDateTime.Now.AddDays(2);

			kid1.JS_RX_NKGoodsValueCurr = curr1.RX_Code;
			kid2.JS_RX_NKGoodsValueCurr = curr2.RX_Code;

			mother.CoLoadShipments.Add(kid1);
			mother.CoLoadShipments.Add(kid2);

			AssertEquals(390m, mother.JS_GoodsValue);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, mother.JS_RX_NKGoodsValueCurr);

			kid2.JS_RX_NKGoodsValueCurr = curr1.RX_Code;
			AssertEquals(1100m, mother.JS_GoodsValue);
			AssertEquals(curr1.RX_Code, mother.JS_RX_NKGoodsValueCurr);

			kid2.JS_RX_NKGoodsValueCurr = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals(750m, mother.JS_GoodsValue);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, mother.JS_RX_NKGoodsValueCurr);

			kid1.JS_RX_NKGoodsValueCurr = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals(1100m, mother.JS_GoodsValue);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, mother.JS_RX_NKGoodsValueCurr);

			mother.CoLoadShipments.Remove(kid1);
			mother.CoLoadShipments.Remove(kid2);
			AssertEquals(0m, mother.JS_GoodsValue);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, mother.JS_RX_NKGoodsValueCurr);
		}

		public void TestColoadMasterTotals_GoodsValue_DoesntupdateDifferentMaster()
		{
			CommonShipment mother = Factory.New<CommonShipment>();
			CommonShipment kid1 = Factory.New<CommonShipment>();
			CommonShipment kid2 = Factory.New<CommonShipment>();
			mother.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			kid1.JS_GoodsValue = 700;
			kid2.JS_GoodsValue = 400;

			RefCurrency curr1 = Factory.New<RefCurrency>();
			curr1.RX_Code = "ART";
			RefExchangeRate rate1 = curr1.ExchangeRates.AddNew();
			rate1.RE_ExRateType = "SEL";
			rate1.RE_SellRate = 2;
			rate1.RE_StartDate = ZDateTime.Now.AddDays(-1);
			rate1.RE_ExpiryDate = ZDateTime.Now.AddDays(2);

			RefCurrency curr2 = Factory.New<RefCurrency>();
			curr2.RX_Code = "XXX";
			RefExchangeRate rate2 = curr2.ExchangeRates.AddNew();
			rate2.RE_ExRateType = "SEL";
			rate2.RE_SellRate = 10;
			rate2.RE_StartDate = ZDateTime.Now.AddDays(-1);
			rate2.RE_ExpiryDate = ZDateTime.Now.AddDays(2);

			RefCurrency curr3 = Factory.New<RefCurrency>();
			curr3.RX_Code = "YYY";

			kid1.JS_RX_NKGoodsValueCurr = curr1.RX_Code;
			kid2.JS_RX_NKGoodsValueCurr = curr2.RX_Code;

			mother.CoLoadShipments.Add(kid1);
			mother.CoLoadShipments.Add(kid2);

			AssertEquals(390m, mother.JS_GoodsValue);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, mother.JS_RX_NKGoodsValueCurr);

			mother.JS_RX_NKGoodsValueCurr = curr3.RX_Code;
			AssertEquals(390m, mother.JS_GoodsValue);
			AssertEquals(curr3.RX_Code, mother.JS_RX_NKGoodsValueCurr);
			kid1.JS_GoodsValue = 1000m;

			AssertEquals(390m, mother.JS_GoodsValue);
			AssertEquals(curr3.RX_Code, mother.JS_RX_NKGoodsValueCurr);

			kid1.JS_GoodsValue = 700m;
			mother.JS_RX_NKGoodsValueCurr = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			mother.JS_GoodsValue = 400m;

			kid1.JS_GoodsValue = 800m;
			AssertEquals(400m, mother.JS_GoodsValue);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, mother.JS_RX_NKGoodsValueCurr);

			mother.CoLoadShipments.Remove(kid1);
			mother.CoLoadShipments.Remove(kid2);
			AssertEquals(400m, mother.JS_GoodsValue);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, mother.JS_RX_NKGoodsValueCurr);
		}

		#endregion

		#region Insurance Value

		public void TestColoadMasterTotals_InsuranceValue()
		{
			CommonShipment mother = Factory.New<CommonShipment>();
			CommonShipment kid1 = Factory.New<CommonShipment>();
			CommonShipment kid2 = Factory.New<CommonShipment>();
			mother.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			kid1.JS_RX_NKInsuranceCurrency = GlbBranch.CurrentBranch.Country.RN_RX_NKLocalCurrency;
			kid2.JS_RX_NKInsuranceCurrency = GlbBranch.CurrentBranch.Country.RN_RX_NKLocalCurrency;

			mother.CoLoadShipments.Add(kid1);
			mother.CoLoadShipments.Add(kid2);

			kid1.JS_InsuranceValue = 7;
			kid2.JS_InsuranceValue = 4;

			AssertEquals(11m, mother.JS_InsuranceValue);
			mother.CoLoadShipments.Remove(kid1);
			mother.CoLoadShipments.Remove(kid2);
			AssertEquals(0m, mother.JS_InsuranceValue);
		}

		public void TestColoadMasterTotals_InsuranceValue_DifferentCurrency()
		{
			CommonShipment mother = Factory.New<CommonShipment>();
			CommonShipment kid1 = Factory.New<CommonShipment>();
			CommonShipment kid2 = Factory.New<CommonShipment>();
			mother.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			mother.JS_RX_NKInsuranceCurrency = mother.JS_RX_NKGoodsValueCurr;
			kid1.JS_RX_NKInsuranceCurrency = mother.JS_RX_NKGoodsValueCurr;
			kid2.JS_RX_NKInsuranceCurrency = mother.JS_RX_NKGoodsValueCurr;

			kid1.JS_InsuranceValue = 700;
			kid2.JS_InsuranceValue = 400;

			RefCurrency curr1 = Factory.New<RefCurrency>();
			curr1.RX_Code = "ART";
			RefExchangeRate rate1 = curr1.ExchangeRates.AddNew();
			rate1.RE_ExRateType = "SEL";
			rate1.RE_SellRate = 2;
			rate1.RE_StartDate = ZDateTime.Now.AddDays(-1);
			rate1.RE_ExpiryDate = ZDateTime.Now.AddDays(2);

			RefCurrency curr2 = Factory.New<RefCurrency>();
			curr2.RX_Code = "XXX";
			RefExchangeRate rate2 = curr2.ExchangeRates.AddNew();
			rate2.RE_ExRateType = "SEL";
			rate2.RE_SellRate = 10;
			rate2.RE_StartDate = ZDateTime.Now.AddDays(-1);
			rate2.RE_ExpiryDate = ZDateTime.Now.AddDays(2);

			kid1.JS_RX_NKInsuranceCurrency = curr1.RX_Code;
			kid2.JS_RX_NKInsuranceCurrency = curr2.RX_Code;

			mother.CoLoadShipments.Add(kid1);
			mother.CoLoadShipments.Add(kid2);

			AssertEquals(390m, mother.JS_InsuranceValue);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, mother.JS_RX_NKInsuranceCurrency);

			kid2.JS_RX_NKInsuranceCurrency = curr1.RX_Code;
			AssertEquals(1100m, mother.JS_InsuranceValue);
			AssertEquals(curr1.RX_Code, mother.JS_RX_NKInsuranceCurrency);

			kid2.JS_RX_NKInsuranceCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals(750m, mother.JS_InsuranceValue);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, mother.JS_RX_NKInsuranceCurrency);

			kid1.JS_RX_NKInsuranceCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals(1100m, mother.JS_InsuranceValue);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, mother.JS_RX_NKInsuranceCurrency);

			mother.CoLoadShipments.Remove(kid1);
			mother.CoLoadShipments.Remove(kid2);
			AssertEquals(0m, mother.JS_InsuranceValue);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, mother.JS_RX_NKInsuranceCurrency);
		}

		public void TestColoadMasterTotals_InsuranceValue_DoesntupdateDifferentMaster()
		{
			CommonShipment mother = Factory.New<CommonShipment>();
			CommonShipment kid1 = Factory.New<CommonShipment>();
			CommonShipment kid2 = Factory.New<CommonShipment>();
			mother.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			kid1.JS_InsuranceValue = 700;
			kid2.JS_InsuranceValue = 400;

			RefCurrency curr1 = Factory.New<RefCurrency>();
			curr1.RX_Code = "ART";
			RefExchangeRate rate1 = curr1.ExchangeRates.AddNew();
			rate1.RE_ExRateType = "SEL";
			rate1.RE_SellRate = 2;
			rate1.RE_StartDate = ZDateTime.Now.AddDays(-1);
			rate1.RE_ExpiryDate = ZDateTime.Now.AddDays(2);

			RefCurrency curr2 = Factory.New<RefCurrency>();
			curr2.RX_Code = "XXX";
			RefExchangeRate rate2 = curr2.ExchangeRates.AddNew();
			rate2.RE_ExRateType = "SEL";
			rate2.RE_SellRate = 10;
			rate2.RE_StartDate = ZDateTime.Now.AddDays(-1);
			rate2.RE_ExpiryDate = ZDateTime.Now.AddDays(2);

			RefCurrency curr3 = Factory.New<RefCurrency>();
			curr3.RX_Code = "YYY";

			kid1.JS_RX_NKInsuranceCurrency = curr1.RX_Code;
			kid2.JS_RX_NKInsuranceCurrency = curr2.RX_Code;

			mother.CoLoadShipments.Add(kid1);
			mother.CoLoadShipments.Add(kid2);

			AssertEquals(390m, mother.JS_InsuranceValue);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, mother.JS_RX_NKInsuranceCurrency);

			mother.JS_RX_NKInsuranceCurrency = curr3.RX_Code;
			AssertEquals(390m, mother.JS_InsuranceValue);
			AssertEquals(curr3.RX_Code, mother.JS_RX_NKInsuranceCurrency);
			kid1.JS_InsuranceValue = 1000m;

			AssertEquals(390m, mother.JS_InsuranceValue);
			AssertEquals(curr3.RX_Code, mother.JS_RX_NKInsuranceCurrency);

			kid1.JS_InsuranceValue = 700m;
			mother.JS_RX_NKInsuranceCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			mother.JS_InsuranceValue = 400m;

			kid1.JS_InsuranceValue = 800m;
			AssertEquals(400m, mother.JS_InsuranceValue);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, mother.JS_RX_NKInsuranceCurrency);

			mother.CoLoadShipments.Remove(kid1);
			mother.CoLoadShipments.Remove(kid2);
			AssertEquals(400m, mother.JS_InsuranceValue);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, mother.JS_RX_NKInsuranceCurrency);
		}

		#endregion

		#region Inner Packs

		public void TestColoadMasterTotals_PackageCount()
		{
			CommonShipment mother = Factory.New<CommonShipment>();
			CommonShipment kid1 = Factory.New<CommonShipment>();
			CommonShipment kid2 = Factory.New<CommonShipment>();
			mother.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			kid1.JS_TotalPackageCount = 7;
			kid2.JS_TotalPackageCount = 4;

			mother.CoLoadShipments.Add(kid1);
			mother.CoLoadShipments.Add(kid2);

			AssertEquals(11, mother.JS_TotalPackageCount);
			mother.CoLoadShipments.Remove(kid1);
			mother.CoLoadShipments.Remove(kid2);
			AssertEquals(0, mother.JS_TotalPackageCount);
		}

		public void TestColoadMasterTotals_PackageCount_DifferentUnits()
		{
			CommonShipment mother = Factory.New<CommonShipment>();
			CommonShipment kid1 = Factory.New<CommonShipment>();
			CommonShipment kid2 = Factory.New<CommonShipment>();
			mother.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			kid1.JS_TotalPackageCount = 7;
			kid2.JS_TotalPackageCount = 4;
			kid1.JS_F3_NKTotalCountPackType = "PLT";
			kid2.JS_F3_NKTotalCountPackType = "BOX";

			mother.CoLoadShipments.Add(kid1);
			mother.CoLoadShipments.Add(kid2);

			AssertEquals(11, mother.JS_TotalPackageCount);
			AssertEquals(Constants.PkgUnit.Package, mother.JS_F3_NKTotalCountPackType);

			mother.CoLoadShipments.Remove(kid1);
			mother.CoLoadShipments.Remove(kid2);
			AssertEquals(0, mother.JS_TotalPackageCount);
			AssertEquals(Constants.PkgUnit.Package, mother.JS_F3_NKTotalCountPackType);
		}

		public void TestColoadMasterTotals_PackageCountPackType()
		{
			CommonShipment mother = Factory.New<CommonShipment>();
			CommonShipment kid = Factory.New<CommonShipment>();
			mother.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			kid.JS_TotalPackageCount = 7;
			kid.JS_F3_NKTotalCountPackType = Constants.PkgUnit.Box;

			mother.CoLoadShipments.Add(kid);

			AssertEquals(7, mother.JS_TotalPackageCount);
			AssertEquals(Constants.PkgUnit.Box, mother.JS_F3_NKTotalCountPackType);
		}

		#endregion

		public void TestColoadMasterTotals_Saved()
		{
			CommonShipment masterShipment = Factory.NewWithValidTestData<CommonShipment>();
			CommonShipment childShipment1 = Factory.NewWithValidTestData<CommonShipment>();
			CommonShipment childShipment2 = Factory.NewWithValidTestData<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			Factory.Save();
			AssertEquals(false, masterShipment.HasChanges);

			childShipment1.JS_ActualWeight = 7;
			childShipment2.JS_ActualWeight = 4;
			masterShipment.CoLoadShipments.Add(childShipment1);
			masterShipment.CoLoadShipments.Add(childShipment2);

			AssertEquals(11m, masterShipment.JS_ActualWeight);
			AssertEquals(true, masterShipment.HasChanges);

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CommonShipment masterShipmentReloaded = newFactory.Load<CommonShipment>(masterShipment.PK);
			AssertEquals(11m, masterShipmentReloaded.JS_ActualWeight);
		}

		public void TestColoadMasterTotals_Validation()
		{
			CommonShipment masterShipment = Factory.NewWithValidTestData<CommonShipment>();
			CommonShipment childShipment1 = Factory.NewWithValidTestData<CommonShipment>();
			CommonShipment childShipment2 = Factory.NewWithValidTestData<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			Factory.Save();
			AssertEquals(false, masterShipment.HasChanges);

			childShipment1.JS_OuterPacks = 1;
			childShipment1.JS_ActualWeight = 2;
			childShipment1.JS_ActualVolume = 3;
			childShipment2.JS_OuterPacks = 4;
			childShipment2.JS_ActualWeight = 5;
			childShipment2.JS_ActualVolume = 6;
			masterShipment.CoLoadShipments.Add(childShipment1);
			masterShipment.CoLoadShipments.Add(childShipment2);

			AssertEquals(5, masterShipment.JS_OuterPacks);
			AssertEquals(7m, masterShipment.JS_ActualWeight);
			AssertEquals(9m, masterShipment.JS_ActualVolume);
			Assert(!masterShipment.JS_OuterPacksInfo.HasWarnings());
			Assert(!masterShipment.JS_ActualWeightInfo.HasWarnings());
			Assert(!masterShipment.JS_ActualVolumeInfo.HasWarnings());

			masterShipment.CoLoadShipments.Remove(childShipment2);
			AssertEquals(1, masterShipment.JS_OuterPacks);
			AssertEquals(2m, masterShipment.JS_ActualWeight);
			AssertEquals(3m, masterShipment.JS_ActualVolume);
			Assert(!masterShipment.JS_OuterPacksInfo.HasWarnings());
			Assert(!masterShipment.JS_ActualWeightInfo.HasWarnings());
			Assert(!masterShipment.JS_ActualVolumeInfo.HasWarnings());
		}

		#endregion

		#region Commons

		public void TestLeadWeightIsCalculatedFromSubs()
		{
			Env.Registry.FreightWeightUnit = Constants.Weight.Kilograms;

			CommonShipment leadShipment = Factory.New<CommonShipment>();
			leadShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			CommonShipment sub1 = Factory.New<CommonShipment>();
			CommonShipment sub2 = Factory.New<CommonShipment>();

			leadShipment.CoLoadShipments.Add(sub1);
			leadShipment.CoLoadShipments.Add(sub2);

			sub1.JS_ActualWeight = 2m;
			sub2.JS_ActualWeight = 3m;

			AssertEquals(5m, leadShipment.JS_ActualWeight);
			AssertEquals(Constants.Weight.Kilograms, leadShipment.JS_UnitOfWeight);

			sub1.JS_ActualWeight = 8m;
			AssertEquals(11m, leadShipment.JS_ActualWeight);
			AssertEquals(Constants.Weight.Kilograms, leadShipment.JS_UnitOfWeight);

			sub2.JS_ActualWeight = 5m;
			AssertEquals(13m, leadShipment.JS_ActualWeight);
			AssertEquals(Constants.Weight.Kilograms, leadShipment.JS_UnitOfWeight);

			sub2.JS_UnitOfWeight = Constants.Weight.Grams;
			AssertEquals(8.005m, leadShipment.JS_ActualWeight);
			AssertEquals(Constants.Weight.Kilograms, leadShipment.JS_UnitOfWeight);

			sub1.JS_UnitOfWeight = Constants.Weight.Grams;
			AssertEquals(13m, leadShipment.JS_ActualWeight);
			AssertEquals(Constants.Weight.Grams, leadShipment.JS_UnitOfWeight);

			sub2.JS_UnitOfWeight = Constants.Weight.Kilograms;
			AssertEquals(5.008m, leadShipment.JS_ActualWeight);
			AssertEquals(Constants.Weight.Kilograms, leadShipment.JS_UnitOfWeight);
		}

		public void TestLeadVolumeIsCalculatedFromSubs()
		{
			Env.Registry.FreightVolumeUnit = Constants.Volume.MegaLitre;

			CommonShipment leadShipment = Factory.New<CommonShipment>();
			leadShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			CommonShipment sub1 = Factory.New<CommonShipment>();
			CommonShipment sub2 = Factory.New<CommonShipment>();

			leadShipment.CoLoadShipments.Add(sub1);
			leadShipment.CoLoadShipments.Add(sub2);

			sub1.JS_ActualVolume = 2m;
			sub2.JS_ActualVolume = 3m;

			AssertEquals(5m, leadShipment.JS_ActualVolume);
			AssertEquals(Constants.Volume.MegaLitre, leadShipment.JS_UnitOfVolume);

			sub1.JS_ActualVolume = 8m;
			AssertEquals(11m, leadShipment.JS_ActualVolume);
			AssertEquals(Constants.Volume.MegaLitre, leadShipment.JS_UnitOfVolume);

			sub2.JS_ActualVolume = 5m;
			AssertEquals(13m, leadShipment.JS_ActualVolume);
			AssertEquals(Constants.Volume.MegaLitre, leadShipment.JS_UnitOfVolume);

			sub2.JS_UnitOfVolume = Constants.Volume.Litre;
			AssertEquals(8.000m, leadShipment.JS_ActualVolume);
			AssertEquals(Constants.Volume.MegaLitre, leadShipment.JS_UnitOfVolume);

			sub1.JS_UnitOfVolume = Constants.Volume.Litre;
			AssertEquals(13m, leadShipment.JS_ActualVolume);
			AssertEquals(Constants.Volume.Litre, leadShipment.JS_UnitOfVolume);

			sub2.JS_UnitOfVolume = Constants.Volume.MegaLitre;
			AssertEquals(5.000m, leadShipment.JS_ActualVolume);
			AssertEquals(Constants.Volume.MegaLitre, leadShipment.JS_UnitOfVolume);
		}

		#endregion

		#region Read Only

		public void TestReadonlyDependsOnMaster()
		{
			AssertEquals(true, Helper.MasterShipment.CoLoadShipments.ReadOnly);
			Helper.MasterShipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			AssertEquals(false, Helper.MasterShipment.CoLoadShipments.ReadOnly);
			Helper.MasterShipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			AssertEquals(true, Helper.MasterShipment.CoLoadShipments.ReadOnly);
			Helper.MasterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			AssertEquals(false, Helper.MasterShipment.CoLoadShipments.ReadOnly);
			Helper.MasterShipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			AssertEquals(true, Helper.MasterShipment.CoLoadShipments.ReadOnly);
			Helper.MasterShipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValueLegacy;
			AssertEquals(true, Helper.MasterShipment.CoLoadShipments.ReadOnly);
			Helper.MasterShipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
			AssertEquals(true, Helper.MasterShipment.CoLoadShipments.ReadOnly);
		}

		#endregion

		#region Adding Element Tests

		#region Collection tests

		public void TestAddAddsToParentConsol()
		{
			CommonConsol consol = Factory.NewWithValidTestData<CommonConsol>();
			CommonShipment masterShipment = consol.Shipments.AddNew();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			AssertEquals(1, consol.Shipments.Count);

			CommonShipment coload1 = masterShipment.CoLoadShipments.AddNew();
			AssertCollectionContains(coload1, consol.Shipments);

			CommonShipment coload2 = Factory.New<CommonShipment>();
			masterShipment.CoLoadShipments.Add(coload2);

			AssertCollectionContains(coload2, consol.Shipments);

			Factory.Save();

			consol.Shipments.Remove(coload2);
			masterShipment.CoLoadShipments.Load();
			AssertCollectionNotContains("Not re-added during load", coload2, consol.Shipments);
		}

		public void TestAddNewCopiesConsignee()
		{
			Helper.MasterShipment.ConsigneePK = ZGuid.NewZGuid();
			CommonShipment relatedShipment = Helper.MasterShipment.CoLoadShipments.AddNew();
			AssertEquals("Consignee defaulted from master", Helper.MasterShipment.ConsigneePK, relatedShipment.ConsigneePK);

			Helper.MasterShipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			relatedShipment = Helper.MasterShipment.CoLoadShipments.AddNew();
			AssertEquals("Consignee defaulted from master", Helper.MasterShipment.ConsigneePK, relatedShipment.ConsigneePK);

			Helper.MasterShipment.JS_ShipmentType = Constants.ShipmentTypes.ShippersConsolLead;
			relatedShipment = Helper.MasterShipment.CoLoadShipments.AddNew();
			AssertEquals("Consignee defaulted from master", Helper.MasterShipment.ConsigneePK, relatedShipment.ConsigneePK);

			Helper.MasterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			relatedShipment = Helper.MasterShipment.CoLoadShipments.AddNew();
			AssertEquals("Consignee not defaulted if master is co-load master", ZGuid.Empty, relatedShipment.ConsigneePK);

			Helper.MasterShipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			relatedShipment = Helper.MasterShipment.CoLoadShipments.AddNew();
			AssertEquals("Consignee defaulted from master", Helper.MasterShipment.ConsigneePK, relatedShipment.ConsigneePK);
		}

		public void TestAddNewCopiesPackingModeOriginDestinationMode()
		{
			Helper.MasterShipment.JS_RL_NKOrigin = "123";
			Helper.MasterShipment.JS_RL_NKDestination = "456";
			Helper.MasterShipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Road;
			Helper.MasterShipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.BuyersConsol;

			CommonShipment newShip = Helper.MasterShipment.CoLoadShipments.AddNew();
			AssertEquals(Helper.MasterShipment.JS_PackingMode, newShip.JS_PackingMode);
			AssertEquals(Helper.MasterShipment.JS_RL_NKOrigin, newShip.JS_RL_NKOrigin);
			AssertEquals(Helper.MasterShipment.JS_RL_NKDestination, newShip.JS_RL_NKDestination);
			AssertEquals(Helper.MasterShipment.JS_TransportMode, newShip.JS_TransportMode);
		}

		public void TestColoadSubShipmentCollectionUpdatesWhenAdding()
		{
			Helper.MasterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			Helper.SubShipment1.JS_JS_ColoadMasterShipment = Helper.MasterShipment.PK;
			Assert("Master Shipment's coload sub shipments collection contains CommonShipment form factory X", Helper.MasterShipment.CoLoadShipments.Contains(Helper.SubShipment1.PK));
		}

		public void TestColoadSubShipmentCollectionUpdatesWhenAddingAcrossFactories()
		{
			Helper.MasterShipmentOnX.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			AssertEquals("Master Shipment initialised with not sub shipments", true, Helper.MasterShipmentOnX.CoLoadShipments.Count == 0);

			Helper.SubShipmentOnY.JS_JS_ColoadMasterShipment = Helper.MasterShipmentOnX.PK;
			Helper.FactoryY.Save();
			AssertEquals("Master Shipment's coload sub shipments collection gets the right CommonShipment when it's added from a different factory", true, Helper.MasterShipmentOnX.CoLoadShipments.Contains(Helper.SubShipmentOnY.PK));
		}

		public void TestAddCanBeCalledDuringAddNew()
		{
			Helper.MasterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			Helper.MasterShipment.CoLoadShipments.AddNew();
			AssertEquals("MasterShipmentShould have 1", 1, Helper.MasterShipment.CoLoadShipments.Count);
		}

		#endregion

		#endregion

		#region Removing Element Tests

		#region Collection test

		public void TestColoadSubShipmentCollectionUpdatesWhenRemoving()
		{
			Helper.MasterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			Helper.SubShipment1.JS_JS_ColoadMasterShipment = Helper.MasterShipment.PK;

			Assert("Master Shipment's coload sub shipments collection contains shipment", Helper.MasterShipment.CoLoadShipments.Contains(Helper.SubShipment1.PK));

			Helper.SubShipment1.JS_JS_ColoadMasterShipment = ZGuid.Empty;
			Assert("Master Shipment's coload sub shipments collection no longer contains shipment", !Helper.MasterShipment.CoLoadShipments.Contains(Helper.SubShipment1.PK));
		}

		public void TestColoadSubShipmentCollectionUpdatesWhenRemovingAcrossFactories()
		{
			Helper.MasterShipmentOnX.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			AssertEquals("Master Shipment initialised with not sub shipments", true, Helper.MasterShipmentOnX.CoLoadShipments.Count == 0);

			Helper.SubShipmentOnY.JS_JS_ColoadMasterShipment = Helper.MasterShipmentOnX.PK;
			Helper.FactoryY.Save();
			AssertEquals("Master Shipment's coload sub shipments collection gets the right CommonShipment when it's added from a different factory", true, Helper.MasterShipmentOnX.CoLoadShipments.Contains(Helper.SubShipmentOnY.PK));

			Helper.SubShipmentOnY.JS_JS_ColoadMasterShipment = ZGuid.Empty;
			Helper.FactoryY.Save();
			AssertEquals("Master Shipment's coload sub shipments collection removes when saved from a different factory", false, Helper.MasterShipmentOnX.CoLoadShipments.Contains(Helper.SubShipmentOnY.PK));
		}

		public void TestRemovingFromCollectionRemovesMastersPK()
		{
			CommonShipment masterShipment = Factory.New<CommonShipment>();
			CommonShipment subShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			masterShipment.CoLoadShipments.Add(subShipment);
			AssertEquals(masterShipment.PK, subShipment.JS_JS_ColoadMasterShipment);
			masterShipment.CoLoadShipments.RemoveAll();
			AssertEquals(ZGuid.Empty, subShipment.JS_JS_ColoadMasterShipment);
		}

		#endregion

		#endregion

		#region Moving Element From One collection to another

		public void ColoadSubShipmentCollectionUpdatesWhenMovingAShipmentFromOneMasterToAnother()
		{
			CommonShipment masterShipment1 = CommonShipment.New(Factory);
			CommonShipment masterShipment2 = CommonShipment.New(Factory);
			masterShipment1.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			masterShipment2.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			CommonShipment subShipment1 = CommonShipment.New(Factory);
			CommonShipment subShipment2 = CommonShipment.New(Factory);

			subShipment1.JS_JS_ColoadMasterShipment = masterShipment1.PK;
			subShipment2.JS_JS_ColoadMasterShipment = masterShipment1.PK;
			Assert("Master1 Shipment's coload sub shipments collection contains shipment1.", masterShipment1.CoLoadShipments.Contains(subShipment1.PK));
			Assert("Master1 Shipment's coload sub shipments collection contains shipment2.", masterShipment1.CoLoadShipments.Contains(subShipment2.PK));
			Assert("Master2 Shipment's coload sub shipments collection contains nothing.", masterShipment2.CoLoadShipments.Count == 0);

			subShipment2.JS_JS_ColoadMasterShipment = masterShipment2.PK;
			Assert("Master1 no longer contains shipment2.", !masterShipment1.CoLoadShipments.Contains(subShipment2.PK));
			Assert("Master2 now contains shipment2.", masterShipment2.CoLoadShipments.Contains(subShipment2.PK));
		}

		public void ColoadSubShipmentCollectionUpdatesWhenMovingAShipmentFromOneMasterToAnother_SetMasterLast()
		{
			CommonShipment masterShipment1 = CommonShipment.New(Factory);
			CommonShipment masterShipment2 = CommonShipment.New(Factory);

			CommonShipment subShipment1 = CommonShipment.New(Factory);
			CommonShipment subShipment2 = CommonShipment.New(Factory);

			subShipment1.JS_JS_ColoadMasterShipment = masterShipment1.PK;
			subShipment2.JS_JS_ColoadMasterShipment = masterShipment1.PK;
			masterShipment1.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			Assert("Master1 Shipment's coload sub shipments collection contains shipment1.", masterShipment1.CoLoadShipments.Contains(subShipment1.PK));
			Assert("Master1 Shipment's coload sub shipments collection contains shipment2.", masterShipment1.CoLoadShipments.Contains(subShipment2.PK));
			Assert("Master2 Shipment's coload sub shipments collection contains nothing.", masterShipment2.CoLoadShipments.Count == 0);

			subShipment2.JS_JS_ColoadMasterShipment = masterShipment2.PK;
			masterShipment2.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			Assert("Master1 no longer contains shipment2.", !masterShipment1.CoLoadShipments.Contains(subShipment2.PK));
			Assert("Master2 now contains shipment2.", masterShipment2.CoLoadShipments.Contains(subShipment2.PK));
		}

		#endregion

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CoLoadShipmentCollection(Helper.MasterShipment, Factory);
		}

		protected ColoadShipmentTestHelper Helper
		{
			get { return helper ?? (helper = GetHelper()); }
		}

		protected virtual ColoadShipmentTestHelper GetHelper()
		{
			return new ColoadShipmentTestHelper(Factory);
		}

		ColoadShipmentTestHelper helper;

		#region Class ColoadShipmentsTestHelpers

		public class ColoadShipmentTestHelper
		{
			public ColoadShipmentTestHelper(BusinessObjectFactory factory)
			{
				this.factory = factory;
			}

			// Performs explicit cast otherwise couldn't factor tests using ZPropertyInfo.Value
			public IZType ConvertToZIntOrZDecimal(int value, bool convertToInt)
			{
				return convertToInt ? (ZInt)value : (ZDecimal)value;
			}

			public CommonShipment GetNewShipment()
			{
				return GetNewShipmentCore();
			}

			protected virtual CommonShipment GetNewShipmentCore()
			{
				return CommonShipment.New(factory);
			}

			protected BusinessObjectFactory factory;

			public void ResetTestingShipments()
			{
				fMasterShipment = null;
				fSubShipment1 = null;
				fSubShipment2 = null;
				fSubShipment3 = null;
				fSubShipment4 = null;
			}

			public void ResetCrossFactoryMasterAndChild()
			{
				fMasterShipmentOnX = null;
				fSubShipmentOnY = null;
			}

			#region Factories X & Y

			public void ResetFactoryX()
			{
				fFactoryX = null;
			}

			public void ResetFactoryY()
			{
				fFactoryY = null;
			}

			public BusinessObjectFactory FactoryX
			{
				get { return fFactoryX ?? (fFactoryX = new BusinessObjectFactory()); }
			}

			public BusinessObjectFactory FactoryY
			{
				get { return fFactoryY ?? (fFactoryY = new BusinessObjectFactory()); }
			}

			BusinessObjectFactory fFactoryX;
			BusinessObjectFactory fFactoryY;

			#endregion

			#region Master & Sub-Shipments

			public CommonShipment MasterShipment
			{
				get { return fMasterShipment ?? (fMasterShipment = GetNewShipment()); }
			}

			public CommonShipment SubShipment1
			{
				get { return fSubShipment1 ?? (fSubShipment1 = GetNewShipment()); }
			}

			public CommonShipment SubShipment2
			{
				get { return fSubShipment2 ?? (fSubShipment2 = GetNewShipment()); }
			}

			public CommonShipment SubShipment3
			{
				get { return fSubShipment3 ?? (fSubShipment3 = GetNewShipment()); }
			}

			public CommonShipment SubShipment4
			{
				get { return fSubShipment4 ?? (fSubShipment4 = GetNewShipment()); }
			}

			public CommonShipment MasterShipmentOnX
			{
				get
				{
					if (fMasterShipmentOnX == null)
					{
						fMasterShipmentOnX = CommonShipment.New(FactoryX);
						FactoryX.Save();
					}
					return fMasterShipmentOnX;
				}
			}

			public CommonShipment SubShipmentOnY
			{
				get { return fSubShipmentOnY ?? (fSubShipmentOnY = CommonShipment.New(FactoryY)); }
			}

			CommonShipment fMasterShipment;
			CommonShipment fSubShipment1;
			CommonShipment fSubShipment2;
			CommonShipment fSubShipment3;
			CommonShipment fSubShipment4;
			CommonShipment fMasterShipmentOnX;
			CommonShipment fSubShipmentOnY;

			public readonly bool ConvertToZInt = true;
			public readonly bool ConvertToZDecimal;

			#endregion
		}
		#endregion

		#endregion
	}
}
