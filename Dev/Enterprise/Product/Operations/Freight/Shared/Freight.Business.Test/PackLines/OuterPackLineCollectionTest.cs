using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(OuterPackLineCollection))]
	sealed class OuterPackLineCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			return shipment.OuterPackLines;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			PackLine packLine = Factory.New<PackLine>();
			packLine.JL_FreightMode = FreightConstants.OuterPackType;
			return packLine;
		}

		public void TestSetDefaultsForNewChild()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_GoodsDescription = "Goods Description";
			shipment.JS_F3_NKPackType = "PKG";
			PackLine childPackLine = shipment.OuterPackLines.AddNew();
			AssertEquals("Freight Mode is same as FreightConstants.OuterPackType", FreightConstants.OuterPackType, childPackLine.JL_FreightMode);
			AssertEquals("Unit of Dimension is same as registry", Env.Registry.OuterPacklinesMeasurementDefaultUnit, childPackLine.JL_UnitOfDimension);
			AssertEquals("Pack type is same as CommonShipment Outer Pack Unit", shipment.JS_F3_NKPackType, childPackLine.JL_F3_NKPackType);
			AssertEquals("Pack Description should be same as Master", shipment.JS_GoodsDescription, childPackLine.JL_Description);
			AssertEquals("Pack line Current Consol = the Current Consol of the collection", shipment.OuterPackLines.CurrentConsol, childPackLine.CurrentConsol);
		}

		public void TestSetCurrentConsol()
		{
			var masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var shipmentInNewFac = newFactory.Load<CommonShipment>(masterShipment.PK);
			shipmentInNewFac.Consols.AddNew();

			var subShipment = masterShipment.CoLoadShipments.AddNew();
			subShipment.JS_ActualWeight = 5;
			subShipment.JS_ActualVolume = 5;

			Factory.Save();
			newFactory.Save();

			AssertEquals("Pre-condition: sub-shipment has 1 pack", 1, subShipment.OuterPackLines.Count);
			AssertEquals("Pre-condition: master shipment pack collection contains sub-shipment's pack", 1, masterShipment.OuterPackLines.Count);
			AssertEquals("Pre-condition: master shipment pack collection contains sub-shipment's pack", subShipment.OuterPackLines[0].PK, masterShipment.OuterPackLines[0].PK);
			AssertEquals("Pre-condition: consol is attached to master shipment", 1, masterShipment.Consols.Count);
			var consol = masterShipment.Consols[0];
			AssertNull("Pre-condition: consol is not attached to sub-shipment", subShipment.Consols.GetRelationshipBusinessObject(masterShipment.Consols[0]));

			AssertEquals("Master shipment pack collection's CurrentConsol is set to the consol", consol.PK, masterShipment.OuterPackLines.CurrentConsol.PK);
			AssertEquals("Sub-shipment pack's CurrentConsol is not set to the consol", null, subShipment.OuterPackLines[0].CurrentConsol);
			AssertEquals("Sub-shipment pack collection's CurrentConsol is not set to the consol", null, subShipment.OuterPackLines.CurrentConsol);

			subShipment.OuterPackLines[0].CurrentConsol = consol;
			AssertEquals("Sub-shipment pack's CurrentConsol cannot be set to the consol", null, subShipment.OuterPackLines[0].CurrentConsol);
			subShipment.OuterPackLines.CurrentConsol = consol;
			AssertEquals("Sub-shipment pack collection's CurrentConsol cannot be set to the consol", null, subShipment.OuterPackLines.CurrentConsol);
		}

		public void TestSuppressingAutomaticallyUpdatePackLineContainers_AutoPackFreightContainers_True()
		{
			using (FreightConfigurationRegistry.Instance.AutoPackFreightContainers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = Factory.New<CommonConsol>();
				var container = consol.Containers.AddNew();
				var shipment = consol.Shipments.AddNew();
				var packLine = shipment.OuterPackLines.AddNew();

				Assert("Pack Line is allocated", container.PackLines.Contains(packLine));
			}
		}

		public void TestSuppressingAutomaticallyUpdatePackLineContainers_AutoPackFreightContainers_False()
		{
			using (FreightConfigurationRegistry.Instance.AutoPackFreightContainers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var consol = Factory.New<CommonConsol>();
				var container = consol.Containers.AddNew();
				var shipment = consol.Shipments.AddNew();
				var packLine = shipment.OuterPackLines.AddNew();

				Assert("Pack Line is not allocated", !container.PackLines.Contains(packLine));
			}
		}

		public void TestSuppressingAutomaticallyUpdatePackLineContainers_TemporarilyDisableAutomaticPackingIntoContainer()
		{
			using (FreightConfigurationRegistry.Instance.AutoPackFreightContainers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = Factory.New<CommonConsol>();
				var container = consol.Containers.AddNew();
				var shipment = consol.Shipments.AddNew();

				using (shipment.OuterPackLines.TemporarilyDisableAutomaticPackingIntoContainer())
				{
					var packLine1 = shipment.OuterPackLines.AddNew();
					Assert("Pack Line is not allocated", !container.PackLines.Contains(packLine1));

					using (shipment.OuterPackLines.TemporarilyDisableAutomaticPackingIntoContainer())
					{
						var packLine2 = shipment.OuterPackLines.AddNew();
						Assert("Pack Line is not allocated", !container.PackLines.Contains(packLine2));
					}

					var packLine3 = shipment.OuterPackLines.AddNew();
					Assert("Pack Line is not allocated", !container.PackLines.Contains(packLine3));
				}

				var packLine4 = shipment.OuterPackLines.AddNew();
				Assert("Pack Line is allocated", container.PackLines.Contains(packLine4));
			}
		}

		public void TestImportingShipmentDataAllocatePackLineToConsol()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var shipment = consol.Shipments.AddNew();

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "SDFG5656543";

			Factory.Save();

			try
			{
				((ISupportDataImporting)shipment).IsImportingData = true;
				shipment.OuterPackLines.AddNew();

				AssertEquals(container.PK, shipment.OuterPackLines[0].GetContainer(consol).PK);
			}
			finally
			{
				((ISupportDataImporting)shipment).IsImportingData = false;
			}
		}

		public void TestTotalPackagesWeightVolumeToDeliver()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			PackLine line1 = shipment.OuterPackLines.AddNew();
			line1.JL_PackageCount = 10;
			line1.JL_ActualWeight = 220m;
			line1.JL_ActualWeightUQ = Constants.Weight.Pounds;
			line1.JL_ActualVolume = 90m;
			line1.JL_ActualVolumeUQ = Constants.Volume.CubicYards;
			PackLine line2 = shipment.OuterPackLines.AddNew();
			line2.JL_PackageCount = 10;
			line2.JL_Outturn = 9;
			line2.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			line2.JL_OutturnedWeight = 1000m;
			line2.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;
			line2.JL_OutturnedVolume = 10m;

			AssertEquals("Expecting TotalPackagesToDeliver to be 19", 19, shipment.OuterPackLines.TotalPackagesToDeliver);
		}

		#region Fetch Hints for Enumerate

		public void TestJobContainerPackPivotFetchHints()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var shipment = consol.Shipments.AddNew();

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "SDFG5656543";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "IUUF4475774";

			var pack1 = shipment.OuterPackLines.AddNew();
			var pack2 = shipment.OuterPackLines.AddNew();

			container1.AddPackLine(pack1);
			container2.AddPackLine(pack2);

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var shipment2 = factory2.Load<CommonShipment>(shipment.PK);
			AssertEquals("Pre-condition", 0, factory2.GetTableHitCount(JobContainerPackPivotSchema.Constants.TableName));

			var reloadedPack1 = factory2.Load<PackLine>(pack1.PK);
			var reloadedPack2 = factory2.Load<PackLine>(pack2.PK);
			AssertEquals("JobContainerPackPivot is not hit on load", 0, factory2.GetTableHitCount(JobContainerPackPivotSchema.Constants.TableName));

			foreach (PackLine pack in shipment2.OuterPackLines)
			{
				AssertEquals(1, pack.Containers.Count);
			}

			AssertEquals("JobContainerPackPivot is hit once only on enumerate", 1, factory2.GetTableHitCount(JobContainerPackPivotSchema.Constants.TableName));
		}

		public void TestJobTransportLegPackLineDivotFetchHints()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUBNE";

			var consol = shipment.Consols.AddNew();
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 10;
			container1.PackLines.Add(packline1);

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_PackageCount = 20;
			container2.PackLines.Add(packline2);

			var deliveryConfirm1 = shipment.DeliveryConfirms.AddNew();
			deliveryConfirm1.GetDivot(packline1).J8_PackagesDelivered = 7;
			deliveryConfirm1.EU_Distance = 32m;
			deliveryConfirm1.EU_DistanceUnit = Constants.Length.Kilometres;

			var deliveryConfirm2 = shipment.DeliveryConfirms.AddNew();
			deliveryConfirm2.GetDivot(packline2).J8_PackagesDelivered = 12;
			deliveryConfirm2.EU_Distance = 14m;
			deliveryConfirm2.EU_DistanceUnit = Constants.Length.Kilometres;

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var shipment2 = factory2.Load<CommonShipment>(shipment.PK);
			AssertEquals("Pre-condition", 0, factory2.GetTableHitCount(JobTransportLegPackLineDivotSchema.Constants.TableName));

			var reloadedPack1 = factory2.Load<PackLine>(packline1.PK);
			var reloadedPack2 = factory2.Load<PackLine>(packline2.PK);
			AssertEquals("JobTransportLegPackLineDivot is not hit on load", 0, factory2.GetTableHitCount(JobTransportLegPackLineDivotSchema.Constants.TableName));

			foreach (PackLine pack in shipment2.OuterPackLines)
			{
				AssertNotNull(pack.DeliveryConfirms[0]);
			}

			AssertEquals("JobTransportLegPackLineDivot is hit only once on enumerate", 1, factory2.GetTableHitCount(JobTransportLegPackLineDivotSchema.Constants.TableName));
		}

		#endregion

		#region Defaulting Commodity Code

		public void TestPackLineCommoditySetToConsigneeCommodity()
		{
			CommonShipment shipment = CommonShipment.New(Factory);

			var consignee = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignee, ZBool.True));
			shipment.ConsigneePK = consignee.PK;
			consignee.MiscServ.OM_RH_NKCMMainImportCmdty = "ABC";
			shipment.JS_RL_NKOrigin = "HKHKG";
			shipment.JS_RL_NKDestination = "AUSYD";
			consignee.OH_IsSalesLead = true;
			shipment.Consignee.MiscServ.OM_CMDoesImports = true;
			OuterPackLineCollection packLines = new OuterPackLineCollection(shipment, Factory);
			PackLine packLine1 = Factory.New<PackLine>();
			PackLine packLine2 = packLines.AddNew();
			AssertEquals("PackLine Commodity set to Consignee Commodity", "ABC", packLine2.JL_RH_NKCommodityCode);
		}

		public void TestPackLineCommoditySetToConsignorCommodity()
		{
			CommonShipment shipment = CommonShipment.New(Factory);

			var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignor, ZBool.True));
			shipment.ConsignorPK = consignor.PK;
			consignor.MiscServ.OM_RH_NKCMMainExportCmdty = "ABC";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "HKHKG";
			consignor.OH_IsSalesLead = true;
			shipment.Consignor.MiscServ.OM_CMDoesExports = true;
			PackLine line = shipment.OuterPackLines.AddNew();
			OuterPackLineCollection collectionForShipment = new OuterPackLineCollection(shipment, Factory);
			collectionForShipment.Load();
			AssertEquals("PackLine Commodity set to Consignor Commodity", "ABC", line.JL_RH_NKCommodityCode);
		}

		public void TestConsigneeConsignorDefaultCommodity()
		{
			RefCommodityCode commodityCode = Factory.New<RefCommodityCode>();
			commodityCode.RH_Code = "XXX";
			Env.Registry.CommodityCode = commodityCode.PK.ToGuid();
			CommonShipment shipment = CommonShipment.New(Factory);
			PackLine line = shipment.OuterPackLines.AddNew();
			AssertEquals("PackLine Commodity set to New Commodity XXX", "XXX", line.JL_RH_NKCommodityCode);
		}

		public void TestSetDefaultCommodityForCollection_SetToRegistry()
		{
			var commodityCode = Factory.New<RefCommodityCode>();
			commodityCode.RH_Code = "ABC";
			Env.Registry.CommodityCode = commodityCode.PK.ToGuid();

			var shipment = CommonShipment.New(Factory);

			var line1 = Factory.New<PackLine>();
			line1.JL_FreightMode = FreightConstants.OuterPackType;
			line1.JL_RH_NKCommodityCode = "XYZ";

			var line2 = Factory.New<PackLine>();
			line2.JL_FreightMode = FreightConstants.OuterPackType;

			shipment.OuterPackLines.Add(line1);
			shipment.OuterPackLines.Add(line2);

			shipment.OuterPackLines.SetDefaultCommodityForCollection();
			AssertEquals("PackLine 1 Commodity is Unchanged", "XYZ", line1.JL_RH_NKCommodityCode);
			AssertEquals("PackLine 2 Commodity set to Default Commodity", "ABC", line2.JL_RH_NKCommodityCode);
		}

		public void TestSetDefaultCommodityForCollection_SetToConsignorCommodity()
		{
			var shipment = CommonShipment.New(Factory);
			var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignor, ZBool.True));
			shipment.ConsignorPK = consignor.PK;
			consignor.MiscServ.OM_RH_NKCMMainExportCmdty = "ABC";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "HKHKG";
			consignor.OH_IsSalesLead = true;
			shipment.Consignor.MiscServ.OM_CMDoesExports = true;

			var line1 = Factory.New<PackLine>();
			line1.JL_FreightMode = FreightConstants.OuterPackType;
			line1.JL_RH_NKCommodityCode = "XYZ";

			var line2 = Factory.New<PackLine>();
			line2.JL_FreightMode = FreightConstants.OuterPackType;

			shipment.OuterPackLines.Add(line1);
			shipment.OuterPackLines.Add(line2);

			shipment.OuterPackLines.SetDefaultCommodityForCollection();
			AssertEquals("PackLine 1 Commodity is Unchanged", "XYZ", line1.JL_RH_NKCommodityCode);
			AssertEquals("PackLine 2 Commodity set to Default Commodity", "ABC", line2.JL_RH_NKCommodityCode);
		}

		public void TestSetDefaultCommodityForCollection_SetToConsigneeCommodity()
		{
			var shipment = CommonShipment.New(Factory);
			var consignee = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignee, ZBool.True));
			shipment.ConsigneePK = consignee.PK;
			consignee.MiscServ.OM_RH_NKCMMainImportCmdty = "ABC";
			shipment.JS_RL_NKOrigin = "HKHKG";
			shipment.JS_RL_NKDestination = "AUSYD";
			consignee.OH_IsSalesLead = true;
			shipment.Consignee.MiscServ.OM_CMDoesImports = true;

			var line1 = Factory.New<PackLine>();
			line1.JL_FreightMode = FreightConstants.OuterPackType;
			line1.JL_RH_NKCommodityCode = "XYZ";

			var line2 = Factory.New<PackLine>();
			line2.JL_FreightMode = FreightConstants.OuterPackType;

			shipment.OuterPackLines.Add(line1);
			shipment.OuterPackLines.Add(line2);

			shipment.OuterPackLines.SetDefaultCommodityForCollection();
			AssertEquals("PackLine 1 Commodity is Unchanged", "XYZ", line1.JL_RH_NKCommodityCode);
			AssertEquals("PackLine 2 Commodity set to Default Commodity", "ABC", line2.JL_RH_NKCommodityCode);
		}

		public void TestSetDefaultCommodityForCollection_InvalidCommodityCode()
		{
			Env.Registry.CommodityCode = Guid.NewGuid();

			var line1 = Factory.New<PackLine>();
			line1.JL_FreightMode = FreightConstants.OuterPackType;
			line1.JL_RH_NKCommodityCode = "XYZ";

			var line2 = Factory.New<PackLine>();
			line2.JL_FreightMode = FreightConstants.OuterPackType;

			var outerPackLineCollection = new OuterPackLineCollection(null, Factory);
			outerPackLineCollection.Add(line1);
			outerPackLineCollection.Add(line2);

			outerPackLineCollection.SetDefaultCommodityForCollection();
			AssertEquals("PackLine 1 Commodity is Unchanged", "XYZ", line1.JL_RH_NKCommodityCode);
			AssertEquals("PackLine 2 Commodity set to Default Commodity", ZString.Empty, line2.JL_RH_NKCommodityCode);
		}

		#endregion
	}
}
