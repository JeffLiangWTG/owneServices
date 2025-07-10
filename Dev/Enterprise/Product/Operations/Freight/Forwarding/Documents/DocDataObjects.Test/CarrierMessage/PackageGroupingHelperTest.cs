using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	class PackageGroupingHelperTest : TestCaseWithFactory
	{
		#region GetTopLevelShipmentPackType

		public void TestGetTopLevelShipmentPackType()
		{
			var standardServiceLevel = Factory.LoadFromUniqueKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, new ZString("STD"));
			CreateRefCountryRules(Constants.CountryCodes.UnitedStates, Constants.CountryCodes.China, Constants.TransportModes.Sea, standardServiceLevel.PK, true);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "CNSHG";
			consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONTAINER1";
			container.JC_IsEmptyContainer = false;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_F3_NKTotalCountPackType = Constants.PkgUnit.Bag;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			shipment.JS_F3_NKPackType = Constants.PkgUnit.Bag;
			shipment.JS_F3_NKTotalCountPackType = Constants.PkgUnit.BulkBag;

			var outerPackLine1 = shipment.OuterPackLines.AddNew();
			outerPackLine1.JL_PackageCount = 1;
			outerPackLine1.JL_F3_NKPackType = Constants.PkgUnit.BaleCompressed;

			var innerPackLine1 = shipment.InnerPackLines.AddNew();
			innerPackLine1.JL_PackageCount = 11;
			innerPackLine1.JL_F3_NKPackType = Constants.PkgUnit.Unit;
			innerPackLine1.JL_JL_OuterPackLine = outerPackLine1.PK;

			var innerPackLine2 = shipment.InnerPackLines.AddNew();
			innerPackLine2.JL_PackageCount = 12;
			innerPackLine2.JL_F3_NKPackType = Constants.PkgUnit.Tube;
			innerPackLine2.JL_JL_OuterPackLine = outerPackLine1.PK;

			var packageGroupingHelper = new PackageGroupingHelper(consol, null);

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Assert(!packageGroupingHelper.IsShowInnerPackLines(shipment));
				AssertEquals(Constants.PkgUnit.Bag, packageGroupingHelper.GetTopLevelShipmentPackType(shipment));
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert(packageGroupingHelper.IsShowInnerPackLines(shipment));
				AssertEquals(Constants.PkgUnit.Package, packageGroupingHelper.GetTopLevelShipmentPackType(shipment));
			}
		}

		#endregion

		#region PopulatePackingQuantityAndPackageType

		public void TestPopulatePackingQuantityAndPackageType()
		{
			var standardServiceLevel = Factory.LoadFromUniqueKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, new ZString("STD"));
			CreateRefCountryRules(Constants.CountryCodes.UnitedStates, Constants.CountryCodes.China, Constants.TransportModes.Sea, standardServiceLevel.PK, true);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "CNSHG";
			consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONTAINER1";
			container.JC_IsEmptyContainer = false;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Bag;
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
			shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Bag;
			shipment.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.BulkBag;

			var subShipment = shipment.CoLoadShipments.AddNew();
			subShipment.JS_F3_NKPackType = Core.Constants.PkgUnit.BreakBulk;
			subShipment.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.BaleCompressed;

			var outerPackLine1 = subShipment.OuterPackLines.AddNew();
			outerPackLine1.JL_PackageCount = 1;
			outerPackLine1.JL_F3_NKPackType = Core.Constants.PkgUnit.BaleCompressed;

			var innerPackLine1 = subShipment.InnerPackLines.AddNew();
			innerPackLine1.JL_PackageCount = 11;
			innerPackLine1.JL_F3_NKPackType = Core.Constants.PkgUnit.Unit;
			innerPackLine1.JL_JL_OuterPackLine = outerPackLine1.PK;

			var innerPackLine2 = subShipment.InnerPackLines.AddNew();
			innerPackLine2.JL_PackageCount = 12;
			innerPackLine2.JL_F3_NKPackType = Core.Constants.PkgUnit.Tube;
			innerPackLine2.JL_JL_OuterPackLine = outerPackLine1.PK;

			var outerPackLine2 = subShipment.OuterPackLines.AddNew();
			outerPackLine2.JL_PackageCount = 2;
			outerPackLine2.JL_F3_NKPackType = Core.Constants.PkgUnit.BaleCompressed;

			var innerPackLine3 = subShipment.InnerPackLines.AddNew();
			innerPackLine3.JL_PackageCount = 13;
			innerPackLine3.JL_F3_NKPackType = Core.Constants.PkgUnit.Tote;
			innerPackLine3.JL_JL_OuterPackLine = outerPackLine2.PK;

			var innerPackLine4 = subShipment.InnerPackLines.AddNew();
			innerPackLine4.JL_PackageCount = 14;
			innerPackLine4.JL_F3_NKPackType = Core.Constants.PkgUnit.Tote;
			innerPackLine4.JL_JL_OuterPackLine = outerPackLine2.PK;

			var outerPackLine3 = subShipment.OuterPackLines.AddNew();
			outerPackLine3.JL_PackageCount = 4;
			outerPackLine3.JL_F3_NKPackType = Core.Constants.PkgUnit.Bundle;

			var packageGroupingHelper = new PackageGroupingHelper(consol, null);
			var packingLineDO = new PackingLine(ZGuid.NewZGuid(), Factory);

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				packageGroupingHelper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.GroupByShipment, outerPackLine1, packingLineDO, Core.Constants.PkgUnit.Box);
				AssertEquals(0, packingLineDO.Quantity);
				AssertNullOrEmpty(packingLineDO.PackageType?.Code);
				Assert(packingLineDO.HasInnerPackLines);

				packageGroupingHelper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.GroupByShipment, outerPackLine2, packingLineDO, Core.Constants.PkgUnit.Basket);
				AssertEquals(0, packingLineDO.Quantity);
				AssertNullOrEmpty(packingLineDO.PackageType?.Code);
				Assert(packingLineDO.HasInnerPackLines);

				packageGroupingHelper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.GroupByShipment, outerPackLine3, packingLineDO, Core.Constants.PkgUnit.Case);
				AssertEquals(0, packingLineDO.Quantity);
				AssertNullOrEmpty(packingLineDO.PackageType?.Code);
				Assert(!packingLineDO.HasInnerPackLines);

				packageGroupingHelper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.GroupByPackLine, outerPackLine1, packingLineDO, Core.Constants.PkgUnit.Box);
				AssertEquals(0, packingLineDO.Quantity);
				AssertNullOrEmpty(packingLineDO.PackageType?.Code);
				Assert(packingLineDO.HasInnerPackLines);

				packageGroupingHelper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.GroupByPackLine, outerPackLine2, packingLineDO, Core.Constants.PkgUnit.Basket);
				AssertEquals(0, packingLineDO.Quantity);
				AssertNullOrEmpty(packingLineDO.PackageType?.Code);
				Assert(packingLineDO.HasInnerPackLines);

				packageGroupingHelper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.GroupByPackLine, outerPackLine3, packingLineDO, Core.Constants.PkgUnit.Case);
				AssertEquals(0, packingLineDO.Quantity);
				AssertNullOrEmpty(packingLineDO.PackageType?.Code);
				Assert(!packingLineDO.HasInnerPackLines);

				packageGroupingHelper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.DoNotGroup, outerPackLine1, packingLineDO, Core.Constants.PkgUnit.Box);
				AssertEquals(0, packingLineDO.Quantity);
				AssertNullOrEmpty(packingLineDO.PackageType?.Code);
				Assert(packingLineDO.HasInnerPackLines);

				packageGroupingHelper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.DoNotGroup, outerPackLine2, packingLineDO, Core.Constants.PkgUnit.Basket);
				AssertEquals(0, packingLineDO.Quantity);
				AssertNullOrEmpty(packingLineDO.PackageType?.Code);
				Assert(packingLineDO.HasInnerPackLines);

				packageGroupingHelper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.DoNotGroup, outerPackLine3, packingLineDO, Core.Constants.PkgUnit.Case);
				AssertEquals(0, packingLineDO.Quantity);
				AssertNullOrEmpty(packingLineDO.PackageType?.Code);
				Assert(!packingLineDO.HasInnerPackLines);
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				packageGroupingHelper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.GroupByShipment, outerPackLine1, packingLineDO, Core.Constants.PkgUnit.Box);
				AssertEquals(23, packingLineDO.Quantity);
				AssertEquals(Core.Constants.PkgUnit.Box, packingLineDO.PackageType.Code);
				Assert(packingLineDO.HasInnerPackLines);

				packageGroupingHelper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.GroupByShipment, outerPackLine2, packingLineDO, Core.Constants.PkgUnit.Basket);
				AssertEquals(27, packingLineDO.Quantity);
				AssertEquals(Core.Constants.PkgUnit.Basket, packingLineDO.PackageType.Code);
				Assert(packingLineDO.HasInnerPackLines);

				packingLineDO.Quantity = 0;
				packageGroupingHelper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.GroupByShipment, outerPackLine3, packingLineDO, Core.Constants.PkgUnit.Case);
				AssertEquals(0, packingLineDO.Quantity);
				AssertEquals(Core.Constants.PkgUnit.Case, packingLineDO.PackageType.Code);
				Assert(!packingLineDO.HasInnerPackLines);

				packageGroupingHelper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.GroupByPackLine, outerPackLine1, packingLineDO, Core.Constants.PkgUnit.Box);
				AssertEquals(23, packingLineDO.Quantity);
				AssertEquals(Core.Constants.PkgUnit.Package, packingLineDO.PackageType.Code);
				Assert(packingLineDO.HasInnerPackLines);

				packageGroupingHelper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.GroupByPackLine, outerPackLine2, packingLineDO, Core.Constants.PkgUnit.Basket);
				AssertEquals(27, packingLineDO.Quantity);
				AssertEquals(Core.Constants.PkgUnit.Tote, packingLineDO.PackageType.Code);
				Assert(packingLineDO.HasInnerPackLines);

				packingLineDO.Quantity = 0;
				packingLineDO.PackageType.Code = Core.Constants.PkgUnit.Container;
				packageGroupingHelper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.GroupByPackLine, outerPackLine3, packingLineDO, Core.Constants.PkgUnit.Case);
				AssertEquals(0, packingLineDO.Quantity);
				AssertEquals(Core.Constants.PkgUnit.Container, packingLineDO.PackageType.Code);
				Assert(!packingLineDO.HasInnerPackLines);

				packageGroupingHelper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.DoNotGroup, outerPackLine1, packingLineDO, Core.Constants.PkgUnit.Box);
				AssertEquals(23, packingLineDO.Quantity);
				AssertEquals(Core.Constants.PkgUnit.Package, packingLineDO.PackageType.Code);
				Assert(packingLineDO.HasInnerPackLines);

				packageGroupingHelper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.DoNotGroup, outerPackLine2, packingLineDO, Core.Constants.PkgUnit.Basket);
				AssertEquals(27, packingLineDO.Quantity);
				AssertEquals(Core.Constants.PkgUnit.Tote, packingLineDO.PackageType.Code);
				Assert(packingLineDO.HasInnerPackLines);

				packingLineDO.Quantity = 0;
				packingLineDO.PackageType.Code = Core.Constants.PkgUnit.Container;
				packageGroupingHelper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.DoNotGroup, outerPackLine3, packingLineDO, Core.Constants.PkgUnit.Case);
				AssertEquals(0, packingLineDO.Quantity);
				AssertEquals(Core.Constants.PkgUnit.Container, packingLineDO.PackageType.Code);
				Assert(!packingLineDO.HasInnerPackLines);

				subShipment.InnerPackLines.Cast<ForwardingPackLine>().ForEach(x => x.JL_JL_OuterPackLine = ZGuid.Empty);
				packingLineDO.Quantity = 0;

				packageGroupingHelper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.GroupByShipment, outerPackLine1, packingLineDO, Core.Constants.PkgUnit.Box);
				AssertEquals(0, packingLineDO.Quantity);
				AssertEquals(Core.Constants.PkgUnit.Box, packingLineDO.PackageType.Code);
				Assert(!packingLineDO.HasInnerPackLines);

				packageGroupingHelper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.GroupByShipment, outerPackLine2, packingLineDO, Core.Constants.PkgUnit.Basket);
				AssertEquals(0, packingLineDO.Quantity);
				AssertEquals(Core.Constants.PkgUnit.Basket, packingLineDO.PackageType.Code);
				Assert(!packingLineDO.HasInnerPackLines);

				packageGroupingHelper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.GroupByShipment, outerPackLine3, packingLineDO, Core.Constants.PkgUnit.Case);
				AssertEquals(0, packingLineDO.Quantity);
				AssertEquals(Core.Constants.PkgUnit.Case, packingLineDO.PackageType.Code);
				Assert(!packingLineDO.HasInnerPackLines);

				packingLineDO.PackageType.Code = Core.Constants.PkgUnit.Container;

				packageGroupingHelper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.GroupByPackLine, outerPackLine1, packingLineDO, Core.Constants.PkgUnit.Box);
				AssertEquals(0, packingLineDO.Quantity);
				AssertEquals(Core.Constants.PkgUnit.Container, packingLineDO.PackageType.Code);
				Assert(!packingLineDO.HasInnerPackLines);

				packageGroupingHelper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.GroupByPackLine, outerPackLine2, packingLineDO, Core.Constants.PkgUnit.Basket);
				AssertEquals(0, packingLineDO.Quantity);
				AssertEquals(Core.Constants.PkgUnit.Container, packingLineDO.PackageType.Code);
				Assert(!packingLineDO.HasInnerPackLines);

				packageGroupingHelper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.GroupByPackLine, outerPackLine3, packingLineDO, Core.Constants.PkgUnit.Case);
				AssertEquals(0, packingLineDO.Quantity);
				AssertEquals(Core.Constants.PkgUnit.Container, packingLineDO.PackageType.Code);
				Assert(!packingLineDO.HasInnerPackLines);

				packageGroupingHelper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.DoNotGroup, outerPackLine1, packingLineDO, Core.Constants.PkgUnit.Box);
				AssertEquals(0, packingLineDO.Quantity);
				AssertEquals(Core.Constants.PkgUnit.Container, packingLineDO.PackageType.Code);
				Assert(!packingLineDO.HasInnerPackLines);

				packageGroupingHelper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.DoNotGroup, outerPackLine2, packingLineDO, Core.Constants.PkgUnit.Basket);
				AssertEquals(0, packingLineDO.Quantity);
				AssertEquals(Core.Constants.PkgUnit.Container, packingLineDO.PackageType.Code);
				Assert(!packingLineDO.HasInnerPackLines);

				packageGroupingHelper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.DoNotGroup, outerPackLine3, packingLineDO, Core.Constants.PkgUnit.Case);
				AssertEquals(0, packingLineDO.Quantity);
				AssertEquals(Core.Constants.PkgUnit.Container, packingLineDO.PackageType.Code);
				Assert(!packingLineDO.HasInnerPackLines);

				subShipment.InnerPackLines.RemoveAndDeleteAll();
				packingLineDO.Quantity = 0;

				packageGroupingHelper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.GroupByShipment, outerPackLine1, packingLineDO, Core.Constants.PkgUnit.Box);
				AssertEquals(0, packingLineDO.Quantity);
				AssertEquals(Core.Constants.PkgUnit.Box, packingLineDO.PackageType.Code);
				Assert(!packingLineDO.HasInnerPackLines);

				packageGroupingHelper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.GroupByShipment, outerPackLine2, packingLineDO, Core.Constants.PkgUnit.Basket);
				AssertEquals(0, packingLineDO.Quantity);
				AssertEquals(Core.Constants.PkgUnit.Basket, packingLineDO.PackageType.Code);
				Assert(!packingLineDO.HasInnerPackLines);

				packageGroupingHelper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.GroupByShipment, outerPackLine3, packingLineDO, Core.Constants.PkgUnit.Case);
				AssertEquals(0, packingLineDO.Quantity);
				AssertEquals(Core.Constants.PkgUnit.Case, packingLineDO.PackageType.Code);
				Assert(!packingLineDO.HasInnerPackLines);

				packingLineDO.PackageType.Code = Core.Constants.PkgUnit.Container;

				packageGroupingHelper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.GroupByPackLine, outerPackLine1, packingLineDO, Core.Constants.PkgUnit.Box);
				AssertEquals(0, packingLineDO.Quantity);
				AssertEquals(Core.Constants.PkgUnit.Container, packingLineDO.PackageType.Code);
				Assert(!packingLineDO.HasInnerPackLines);

				packageGroupingHelper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.GroupByPackLine, outerPackLine2, packingLineDO, Core.Constants.PkgUnit.Basket);
				AssertEquals(0, packingLineDO.Quantity);
				AssertEquals(Core.Constants.PkgUnit.Container, packingLineDO.PackageType.Code);
				Assert(!packingLineDO.HasInnerPackLines);

				packageGroupingHelper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.GroupByPackLine, outerPackLine3, packingLineDO, Core.Constants.PkgUnit.Case);
				AssertEquals(0, packingLineDO.Quantity);
				AssertEquals(Core.Constants.PkgUnit.Container, packingLineDO.PackageType.Code);
				Assert(!packingLineDO.HasInnerPackLines);

				packageGroupingHelper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.DoNotGroup, outerPackLine1, packingLineDO, Core.Constants.PkgUnit.Box);
				AssertEquals(0, packingLineDO.Quantity);
				AssertEquals(Core.Constants.PkgUnit.Container, packingLineDO.PackageType.Code);
				Assert(!packingLineDO.HasInnerPackLines);

				packageGroupingHelper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.DoNotGroup, outerPackLine2, packingLineDO, Core.Constants.PkgUnit.Basket);
				AssertEquals(0, packingLineDO.Quantity);
				AssertEquals(Core.Constants.PkgUnit.Container, packingLineDO.PackageType.Code);
				Assert(!packingLineDO.HasInnerPackLines);

				packageGroupingHelper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.DoNotGroup, outerPackLine3, packingLineDO, Core.Constants.PkgUnit.Case);
				AssertEquals(0, packingLineDO.Quantity);
				AssertEquals(Core.Constants.PkgUnit.Container, packingLineDO.PackageType.Code);
				Assert(!packingLineDO.HasInnerPackLines);
			}
		}

		#endregion

		#region GroupedAndConsolidatedPackingLines

		public void TestPopulateGroupedAndConsolidatedPackingLines()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "CNSHG";
			consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONTAINER1";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONTAINER2";

			var container3 = consol.Containers.AddNew();
			container3.JC_ContainerNum = "CONTAINER3";

			var container4 = consol.Containers.AddNew();
			container4.JC_ContainerNum = ZString.Empty;

			var container5 = consol.Containers.AddNew();
			container5.JC_ContainerNum = ZString.Empty;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
			shipment1.JS_UniqueConsignRef = "JSASM";
			shipment1.JS_GoodsDescription = "ASM JS_GoodsDescription";
			shipment1.JS_MarksAndNumbers = "ASM JS_MarksAndNumbers";

			var shipment1SubShipment1 = shipment1.CoLoadShipments.AddNew();
			shipment1SubShipment1.JS_F3_NKPackType = Core.Constants.PkgUnit.BreakBulk;
			shipment1SubShipment1.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.BaleCompressed;
			shipment1SubShipment1.JS_UniqueConsignRef = "Sub01";
			shipment1SubShipment1.JS_GoodsDescription = "Sub1 JS_GoodsDescription";
			shipment1SubShipment1.JS_MarksAndNumbers = "Sub1 JS_MarksAndNumbers";

			shipment1SubShipment1.InnerPackLines.RemoveAndDeleteAll();
			shipment1SubShipment1.OuterPackLines.RemoveAndDeleteAll();
			shipment1SubShipment1.CusEntryNumbers.RemoveAndDeleteAll();

			var cusEntryNumber = shipment1SubShipment1.CusEntryNumbers.AddNew();
			cusEntryNumber.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG;
			cusEntryNumber.CE_EntryNum = "11321";

			var shipment1SubShipment1OuterPackLine1 = PopulatePackLine(shipment1SubShipment1.OuterPackLines.AddNew()
				, 100, Constants.PkgUnit.BaleCompressed
				, 100, Constants.Weight.Kilograms
				, 100, Constants.Volume.CubicMetres
				, "BaleCompressed1 MarksAndNumbers", "BaleCompressed1 DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, container1.PK, "HC01"
				, "Ref01", "ImportRef01", "ExportRef01", "OuturnComment01", Core.Constants.CargoTypes.Hazardous);

			var shipment1SubShipment1OuterPackLine1HsCode1 = shipment1SubShipment1OuterPackLine1.HarmonisedCodes.AddNew();
			shipment1SubShipment1OuterPackLine1HsCode1.JLH_RN_NKCountry = "US";
			shipment1SubShipment1OuterPackLine1HsCode1.JLH_Code = "USHS";

			var shipment1SubShipment1OuterPackLine1HsCode2 = shipment1SubShipment1OuterPackLine1.HarmonisedCodes.AddNew();
			shipment1SubShipment1OuterPackLine1HsCode2.JLH_RN_NKCountry = "CN";
			shipment1SubShipment1OuterPackLine1HsCode2.JLH_Code = "CNHS";

			var shipment1SubShipment1OuterPackLine2 = PopulatePackLine(shipment1SubShipment1.OuterPackLines.AddNew()
				, 110, Constants.PkgUnit.Tube
				, 110, Constants.Weight.Kilograms
				, 110, Constants.Volume.CubicMetres
				, "Tube MarksAndNumbers", "Tube DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, container2.PK, "HC04"
				, "Ref04", "ImportRef04", "ExportRef04", "OuturnComment04", Core.Constants.CargoTypes.Hazardous);

			PopulatePackLine(shipment1SubShipment1.OuterPackLines.AddNew()
				, 11000, Constants.PkgUnit.Tube
				, 11000, Constants.Weight.Grams
				, 11000, Constants.Volume.CubicMetres
				, "Tube MarksAndNumbers", "Tube DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, ZGuid.Empty, "HC07"
				, "Ref07", "ImportRef07", "ExportRef07", "OuturnComment07", Core.Constants.CargoTypes.Hazardous);

			var shipment1SubShipment2 = shipment1.CoLoadShipments.AddNew();
			shipment1SubShipment2.JS_F3_NKPackType = Core.Constants.PkgUnit.BreakBulk;
			shipment1SubShipment2.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.BaleCompressed;
			shipment1SubShipment2.JS_UniqueConsignRef = "Sub02";
			shipment1SubShipment2.JS_GoodsDescription = "Sub1 JS_GoodsDescription";
			shipment1SubShipment2.JS_MarksAndNumbers = "Sub1 JS_MarksAndNumbers";

			shipment1SubShipment2.InnerPackLines.RemoveAndDeleteAll();
			shipment1SubShipment2.OuterPackLines.RemoveAndDeleteAll();

			PopulatePackLine(shipment1SubShipment2.OuterPackLines.AddNew()
				, 120, Constants.PkgUnit.BaleCompressed
				, 120, Constants.Weight.Kilograms
				, 120, Constants.Volume.CubicMetres
				, "BaleCompressed2 MarksAndNumbers", "BaleCompressed2 DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, container4.PK, "HC07"
				, "Ref07", "ImportRef07", "ExportRef07", "OuturnComment07", Core.Constants.CargoTypes.Hazardous);

			PopulatePackLine(shipment1SubShipment2.OuterPackLines.AddNew()
				, 121, Constants.PkgUnit.Tote
				, 121, Constants.Weight.Kilograms
				, 121, Constants.Volume.CubicMetres
				, "Tote MarksAndNumbers", "Tote DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, container5.PK, "HC08"
				, "Ref08", "ImportRef08", "ExportRef08", "OuturnComment08", Core.Constants.CargoTypes.Hazardous);

			PopulatePackLine(shipment1SubShipment2.OuterPackLines.AddNew()
				, 12000, Constants.PkgUnit.Tube
				, 12000, Constants.Weight.Kilograms
				, 12000, Constants.Volume.CubicMetres
				, "Tube MarksAndNumbers", "Tube DetailedDescription"
				, true, -2, 2, Constants.Temperature.Centigrade
				, container2.PK, "HC09",
				"Ref01", "ImportRef09", "ExportRef01", "OuturnComment09", Core.Constants.CargoTypes.Hazardous);

			var shipment1SubShipment2OuterPackLines1 = shipment1SubShipment2.OuterPackLines.AddNew();

			PopulatePackLine(shipment1SubShipment2OuterPackLines1
				, 12100, Constants.PkgUnit.Tube
				, 12100, Constants.Weight.Kilograms
				, 12100, Constants.Volume.CubicMetres
				, "Tube MarksAndNumbers", "Tube DetailedDescription"
				, true, 1, 3, Constants.Temperature.Centigrade,
				ZGuid.Empty, "HC10",
				"Ref10", "ImportRef10", "ExportRef10", "OuturnComment10", Core.Constants.CargoTypes.Hazardous);

			var shipment1SubShipment2OuterPackLines1HsCode1 = shipment1SubShipment2OuterPackLines1.HarmonisedCodes.AddNew();
			shipment1SubShipment2OuterPackLines1HsCode1.JLH_RN_NKCountry = "US";
			shipment1SubShipment2OuterPackLines1HsCode1.JLH_Code = "HSUS";

			var shipment1SubShipment2OuterPackLines1HsCode2 = shipment1SubShipment2OuterPackLines1.HarmonisedCodes.AddNew();
			shipment1SubShipment2OuterPackLines1HsCode2.JLH_RN_NKCountry = "CN";
			shipment1SubShipment2OuterPackLines1HsCode2.JLH_Code = "CNHS";

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var shipmentDOs = GetShipmentDOs(consol);

				AssertEquals(1, shipmentDOs.Count);
				AssertEquals(7, shipmentDOs.First().AllPackingLinesIncludeCoLoad.Count());

				new PackageGroupingHelper(consol, Context)
					.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByPackLine, true, DataContext.BookingRequest, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				AssertEquals(7, shipmentDOs.First().AllPackingLinesIncludeCoLoad.Count());
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipmentDOs = GetShipmentDOs(consol);

				AssertEquals(7, shipmentDOs.First().AllPackingLinesIncludeCoLoad.Count());

				new PackageGroupingHelper(consol, Context)
					.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.DoNotGroup, true, DataContext.BookingRequest, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				AssertEquals(7, shipmentDOs.First().AllPackingLinesIncludeCoLoad.Count());
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipmentDOs = GetShipmentDOs(consol);
				AssertEquals(1, shipmentDOs.Count);

				new PackageGroupingHelper(consol, Context)
					.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByPackLine, true, DataContext.BookingRequest, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				var shipmentDO = shipmentDOs.First();
				AssertEquals(4, shipmentDO.PackingLines.Count);

				var groupedPackingLineBaleCompressed1 = shipmentDO.PackingLines.First(x => x.GoodsDescription.StartsWith("BaleCompressed1"));
				var groupedPackingLineBaleCompressed2 = shipmentDO.PackingLines.First(x => x.GoodsDescription.StartsWith("BaleCompressed2"));
				var groupedPackingLineTote = shipmentDO.PackingLines.First(x => x.GoodsDescription.StartsWith("Tote"));
				var groupedPackingLineTube = shipmentDO.PackingLines.First(x => x.GoodsDescription.StartsWith("Tube"));

				AssertEquals(1, groupedPackingLineBaleCompressed1.PackingLines.Count);
				AssertEquals(1, groupedPackingLineBaleCompressed2.PackingLines.Count);
				AssertEquals(1, groupedPackingLineTote.PackingLines.Count);
				AssertEquals(1, groupedPackingLineTube.PackingLines.Count);

				AssertPackingLineDetails(groupedPackingLineBaleCompressed1, ZString.Empty
					, 100, Constants.PkgUnit.BaleCompressed
					, 100M, shipment1.JS_UnitOfWeight
					, 100M, shipment1.JS_UnitOfVolume
					, "HC01", "HS (US): USHS", "HS (CN): CNHS"
					, new[] { "HS (US): USHS", "HS (CN): CNHS" }
					, "BKG:11321", "Sub01"
					, "Ref01", "ImportRef01", "ExportRef01"
					, "OuturnComment01", Core.Constants.CargoTypes.Hazardous
					, "BaleCompressed1 MarksAndNumbers", "BaleCompressed1 DetailedDescription"
					, true, -2M, 3M, Constants.Temperature.Centigrade);

				AssertPackingLineDetails(groupedPackingLineBaleCompressed2, ZString.Empty
					, 120, Constants.PkgUnit.BaleCompressed
					, 120M, shipment1.JS_UnitOfWeight
					, 120M, shipment1.JS_UnitOfVolume
					, "HC07", ZString.Empty, ZString.Empty
					, null
					, ZString.Empty, "Sub02"
					, "Ref07", "ImportRef07", "ExportRef07"
					, "OuturnComment07", Core.Constants.CargoTypes.Hazardous
					, "BaleCompressed2 MarksAndNumbers", "BaleCompressed2 DetailedDescription"
					, true, -2M, 3M, Constants.Temperature.Centigrade);

				AssertPackingLineDetails(groupedPackingLineTote, ZString.Empty
					, 121, Constants.PkgUnit.Tote
					, 121M, shipment1.JS_UnitOfWeight
					, 121M, shipment1.JS_UnitOfVolume
					, "HC08", ZString.Empty, ZString.Empty
					, null
					, ZString.Empty, "Sub02"
					, "Ref08", "ImportRef08", "ExportRef08"
					, "OuturnComment08", Core.Constants.CargoTypes.Hazardous
					, "Tote MarksAndNumbers", "Tote DetailedDescription"
					, true, -2M, 3M, Constants.Temperature.Centigrade);

				AssertPackingLineDetails(groupedPackingLineTube, ZString.Empty
					, 12110, Constants.PkgUnit.Tube
					, 12110M, shipment1.JS_UnitOfWeight
					, 12110M, shipment1.JS_UnitOfVolume
					, "HC04, HC09", ZString.Empty, ZString.Empty
					, null
					, "BKG:11321", "Sub01, Sub02"
					, "Ref04, Ref01", "ImportRef04, ImportRef09", "ExportRef04, ExportRef01"
					, "OuturnComment04, OuturnComment09", Core.Constants.CargoTypes.Hazardous
					, "Tube MarksAndNumbers", "Tube DetailedDescription"
					, true, -2M, 2M, Constants.Temperature.Centigrade);

				AssertPackingLineDetails(groupedPackingLineBaleCompressed1.PackingLines.First(), "CONTAINER1"
					, 100, Constants.PkgUnit.BaleCompressed
					, 100M, shipment1.JS_UnitOfWeight
					, 100M, shipment1.JS_UnitOfVolume
					, "HC01", "HS (US): USHS", "HS (CN): CNHS"
					, new[] { "HS (US): USHS", "HS (CN): CNHS" }
					, "BKG:11321", "Sub01"
					, "Ref01", "ImportRef01", "ExportRef01"
					, "OuturnComment01", Core.Constants.CargoTypes.Hazardous
					, ZString.Empty, ZString.Empty
					, true, -2M, 3M, Constants.Temperature.Centigrade);

				AssertPackingLineDetails(groupedPackingLineBaleCompressed2.PackingLines.First(), ZString.Empty
					, 120, Constants.PkgUnit.BaleCompressed
					, 120M, shipment1.JS_UnitOfWeight
					, 120M, shipment1.JS_UnitOfVolume
					, "HC07", ZString.Empty, ZString.Empty
					, null
					, ZString.Empty, "Sub02"
					, "Ref07", "ImportRef07", "ExportRef07"
					, "OuturnComment07", Core.Constants.CargoTypes.Hazardous
					, ZString.Empty, ZString.Empty
					, true, -2M, 3M, Constants.Temperature.Centigrade);

				AssertPackingLineDetails(groupedPackingLineTote.PackingLines.First(), ZString.Empty
					, 121, Constants.PkgUnit.Tote
					, 121M, shipment1.JS_UnitOfWeight
					, 121M, shipment1.JS_UnitOfVolume
					, "HC08", ZString.Empty, ZString.Empty
					, null
					, ZString.Empty, "Sub02"
					, "Ref08", "ImportRef08", "ExportRef08"
					, "OuturnComment08", Core.Constants.CargoTypes.Hazardous
					, ZString.Empty, ZString.Empty
					, true, -2M, 3M, Constants.Temperature.Centigrade);

				AssertPackingLineDetails(groupedPackingLineTube.PackingLines.First(), "CONTAINER2"
					, 12110, Constants.PkgUnit.Tube
					, 12110M, shipment1.JS_UnitOfWeight
					, 12110M, shipment1.JS_UnitOfVolume
					, "HC04, HC09", ZString.Empty, ZString.Empty
					, null
					, "BKG:11321", "Sub01, Sub02"
					, "Ref04, Ref01", "ImportRef04, ImportRef09", "ExportRef04, ExportRef01"
					, "OuturnComment04, OuturnComment09", Core.Constants.CargoTypes.Hazardous
					, ZString.Empty, ZString.Empty
					, true, -2M, 2M, Constants.Temperature.Centigrade);
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipmentDOs = GetShipmentDOs(consol);
				AssertEquals(1, shipmentDOs.Count);

				new PackageGroupingHelper(consol, Context)
					.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByPackLine, false, DataContext.BookingRequest, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				var shipmentDO = shipmentDOs.First();
				AssertEquals(4, shipmentDO.PackingLines.Count);

				var groupedPackingLineBaleCompressed1 = shipmentDO.PackingLines.First(x => x.GoodsDescription.StartsWith("BaleCompressed1"));
				var groupedPackingLineBaleCompressed2 = shipmentDO.PackingLines.First(x => x.GoodsDescription.StartsWith("BaleCompressed2"));
				var groupedPackingLineTote = shipmentDO.PackingLines.First(x => x.GoodsDescription.StartsWith("Tote"));
				var groupedPackingLineTube = shipmentDO.PackingLines.First(x => x.GoodsDescription.StartsWith("Tube"));

				AssertEquals(1, groupedPackingLineBaleCompressed1.PackingLines.Count);
				AssertEquals(1, groupedPackingLineBaleCompressed2.PackingLines.Count);
				AssertEquals(1, groupedPackingLineTote.PackingLines.Count);
				AssertEquals(2, groupedPackingLineTube.PackingLines.Count);

				AssertPackingLineDetails(groupedPackingLineBaleCompressed1, ZString.Empty
					, 100, Constants.PkgUnit.BaleCompressed
					, 100M, shipment1.JS_UnitOfWeight
					, 100M, shipment1.JS_UnitOfVolume
					, "HC01", "HS (US): USHS", "HS (CN): CNHS"
					, new[] { "HS (US): USHS", "HS (CN): CNHS" }
					, "BKG:11321", "Sub01"
					, "Ref01", "ImportRef01", "ExportRef01"
					, "OuturnComment01", Core.Constants.CargoTypes.Hazardous
					, "BaleCompressed1 MarksAndNumbers", "BaleCompressed1 DetailedDescription"
					, true, -2M, 3M, Constants.Temperature.Centigrade);

				AssertPackingLineDetails(groupedPackingLineBaleCompressed2, ZString.Empty
					, 120, Constants.PkgUnit.BaleCompressed
					, 120M, shipment1.JS_UnitOfWeight
					, 120M, shipment1.JS_UnitOfVolume
					, "HC07", ZString.Empty, ZString.Empty
					, null
					, ZString.Empty, "Sub02"
					, "Ref07", "ImportRef07", "ExportRef07"
					, "OuturnComment07", Core.Constants.CargoTypes.Hazardous
					, "BaleCompressed2 MarksAndNumbers", "BaleCompressed2 DetailedDescription"
					, true, -2M, 3M, Constants.Temperature.Centigrade);

				AssertPackingLineDetails(groupedPackingLineTote, ZString.Empty
					, 121, Constants.PkgUnit.Tote
					, 121M, shipment1.JS_UnitOfWeight
					, 121M, shipment1.JS_UnitOfVolume
					, "HC08", ZString.Empty, ZString.Empty
					, null
					, ZString.Empty, "Sub02"
					, "Ref08", "ImportRef08", "ExportRef08"
					, "OuturnComment08", Core.Constants.CargoTypes.Hazardous
					, "Tote MarksAndNumbers", "Tote DetailedDescription"
					, true, -2M, 3M, Constants.Temperature.Centigrade);

				AssertPackingLineDetails(groupedPackingLineTube, ZString.Empty
					, 35210, Constants.PkgUnit.Tube
					, 24221M, shipment1.JS_UnitOfWeight
					, 35210M, shipment1.JS_UnitOfVolume
					, "HC04, HC07, HC09, HC10", "HS (US): HSUS", "HS (CN): CNHS"
					, new[] { "HS (US): HSUS", "HS (CN): CNHS" }
					, "BKG:11321", "Sub01, Sub02"
					, "Ref04, Ref07, Ref01, Ref10", "ImportRef04, ImportRef07, ImportRef09, ImportRef10", "ExportRef04, ExportRef07, ExportRef01, ExportRef10"
					, "OuturnComment04, OuturnComment07, OuturnComment09, OuturnComment10", Core.Constants.CargoTypes.Hazardous
					, "Tube MarksAndNumbers", "Tube DetailedDescription"
					, true, 1M, 2M, Constants.Temperature.Centigrade);

				AssertPackingLineDetails(groupedPackingLineBaleCompressed1.PackingLines.First(), "CONTAINER1"
					, 100, Constants.PkgUnit.BaleCompressed
					, 100M, shipment1.JS_UnitOfWeight
					, 100M, shipment1.JS_UnitOfVolume
					, "HC01", "HS (US): USHS", "HS (CN): CNHS"
					, new[] { "HS (US): USHS", "HS (CN): CNHS" }
					, "BKG:11321", "Sub01"
					, "Ref01", "ImportRef01", "ExportRef01"
					, "OuturnComment01", Core.Constants.CargoTypes.Hazardous
					, ZString.Empty, ZString.Empty
					, true, -2M, 3M, Constants.Temperature.Centigrade);

				AssertPackingLineDetails(groupedPackingLineBaleCompressed2.PackingLines.First(), ZString.Empty
					, 120, Constants.PkgUnit.BaleCompressed
					, 120M, shipment1.JS_UnitOfWeight
					, 120M, shipment1.JS_UnitOfVolume
					, "HC07", ZString.Empty, ZString.Empty
					, null
					, ZString.Empty, "Sub02"
					, "Ref07", "ImportRef07", "ExportRef07"
					, "OuturnComment07", Core.Constants.CargoTypes.Hazardous
					, ZString.Empty, ZString.Empty
					, true, -2M, 3M, Constants.Temperature.Centigrade);

				AssertPackingLineDetails(groupedPackingLineTote.PackingLines.First(), ZString.Empty
					, 121, Constants.PkgUnit.Tote
					, 121M, shipment1.JS_UnitOfWeight
					, 121M, shipment1.JS_UnitOfVolume
					, "HC08", ZString.Empty, ZString.Empty
					, null
					, ZString.Empty, "Sub02"
					, "Ref08", "ImportRef08", "ExportRef08"
					, "OuturnComment08", Core.Constants.CargoTypes.Hazardous
					, ZString.Empty, ZString.Empty
					, true, -2M, 3M, Constants.Temperature.Centigrade);

				AssertPackingLineDetails(groupedPackingLineTube.PackingLines.First(x => x.ContainerNumber == "CONTAINER2"), "CONTAINER2"
					, 12110, Constants.PkgUnit.Tube
					, 12110M, shipment1.JS_UnitOfWeight
					, 12110M, shipment1.JS_UnitOfVolume
					, "HC04, HC09", ZString.Empty, ZString.Empty
					, null
					, "BKG:11321", "Sub01, Sub02"
					, "Ref04, Ref01", "ImportRef04, ImportRef09", "ExportRef04, ExportRef01"
					, "OuturnComment04, OuturnComment09", Core.Constants.CargoTypes.Hazardous
					, ZString.Empty, ZString.Empty
					, true, -2M, 2M, Constants.Temperature.Centigrade);

				AssertPackingLineDetails(groupedPackingLineTube.PackingLines.First(x => x.ContainerNumber.IsEmpty), ZString.Empty
					, 23100, Constants.PkgUnit.Tube
					, 12111M, shipment1.JS_UnitOfWeight
					, 23100M, shipment1.JS_UnitOfVolume
					, "HC07, HC10", "HS (US): HSUS", "HS (CN): CNHS"
					, new[] { "HS (US): HSUS", "HS (CN): CNHS" }
					, "BKG:11321", "Sub01, Sub02"
					, "Ref07, Ref10", "ImportRef07, ImportRef10", "ExportRef07, ExportRef10"
					, "OuturnComment07, OuturnComment10", Core.Constants.CargoTypes.Hazardous
					, ZString.Empty, ZString.Empty
					, true, 1M, 3M, Constants.Temperature.Centigrade);
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipmentDOs = GetShipmentDOs(consol);
				AssertEquals(1, shipmentDOs.Count);

				new PackageGroupingHelper(consol, Context)
					.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByShipment, true, DataContext.BookingRequest, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				var shipmentDO = shipmentDOs.First();
				AssertEquals(1, shipmentDO.PackingLines.Count);

				var groupedPackingLine = shipmentDO.PackingLines.First();
				AssertEquals(4, groupedPackingLine.PackingLines.Count);

				AssertPackingLineDetails(groupedPackingLine, ZString.Empty
					, 12451, Constants.PkgUnit.BaleCompressed
					, 12451M, shipment1.JS_UnitOfWeight
					, 12451M, shipment1.JS_UnitOfVolume
					, "HC01, HC04, HC07, HC08, HC09", "HS (US): USHS", "HS (CN): CNHS"
					, new[] { "HS (US): USHS", "HS (CN): CNHS" }
					, "BKG:11321", "Sub01, Sub02"
					, "Ref01, Ref04, Ref07, Ref08", "ImportRef01, ImportRef04, ImportRef07, ImportRef08, ImportRef09", "ExportRef01, ExportRef04, ExportRef07, ExportRef08"
					, "OuturnComment01, OuturnComment04, OuturnComment07, OuturnComment08, OuturnComment09", Core.Constants.CargoTypes.Hazardous
					, "ASM JS_MarksAndNumbers", "ASM JS_GoodsDescription"
					, true, -2M, 2M, Constants.Temperature.Centigrade);

				var consolidatedPackingLineContainer1 = groupedPackingLine.PackingLines.First(x => x.ContainerNumber == "CONTAINER1");
				var consolidatedPackingLineContainer2 = groupedPackingLine.PackingLines.First(x => x.ContainerNumber == "CONTAINER2");
				var consolidatedPackingLineContainer4 = groupedPackingLine.PackingLines.First(x => x.Identifier.ToString().Substring(0, 36) == container4.PK.ToString());
				var consolidatedPackingLineContainer5 = groupedPackingLine.PackingLines.First(x => x.Identifier.ToString().Substring(0, 36) == container5.PK.ToString());

				AssertPackingLineDetails(consolidatedPackingLineContainer1, "CONTAINER1"
					, 100, Constants.PkgUnit.BaleCompressed
					, 100M, shipment1.JS_UnitOfWeight
					, 100M, shipment1.JS_UnitOfVolume
					, "HC01", "HS (US): USHS", "HS (CN): CNHS"
					, new[] { "HS (US): USHS", "HS (CN): CNHS" }
					, "BKG:11321", "Sub01"
					, "Ref01", "ImportRef01", "ExportRef01"
					, "OuturnComment01", Core.Constants.CargoTypes.Hazardous
					, ZString.Empty, ZString.Empty
					, true, -2M, 3M, Constants.Temperature.Centigrade);
				AssertPackingLineDetails(consolidatedPackingLineContainer2, "CONTAINER2"
					, 12110, Constants.PkgUnit.BaleCompressed
					, 12110M, shipment1.JS_UnitOfWeight
					, 12110M, shipment1.JS_UnitOfVolume
					, "HC04, HC09", ZString.Empty, ZString.Empty
					, null
					, "BKG:11321", "Sub01, Sub02"
					, "Ref04, Ref01", "ImportRef04, ImportRef09", "ExportRef04, ExportRef01"
					, "OuturnComment04, OuturnComment09", Core.Constants.CargoTypes.Hazardous
					, ZString.Empty, ""
					, true, -2M, 2M, Constants.Temperature.Centigrade);
				AssertPackingLineDetails(consolidatedPackingLineContainer4, ZString.Empty
					, 120, Constants.PkgUnit.BaleCompressed
					, 120M, shipment1.JS_UnitOfWeight
					, 120M, shipment1.JS_UnitOfVolume
					, "HC07", ZString.Empty, ZString.Empty
					, null
					, ZString.Empty, "Sub02"
					, "Ref07", "ImportRef07", "ExportRef07"
					, "OuturnComment07", Core.Constants.CargoTypes.Hazardous
					, ZString.Empty, ZString.Empty
					, true, -2M, 3M, Constants.Temperature.Centigrade);
				AssertPackingLineDetails(consolidatedPackingLineContainer5, ZString.Empty
					, 121, Constants.PkgUnit.BaleCompressed
					, 121M, shipment1.JS_UnitOfWeight
					, 121M, shipment1.JS_UnitOfVolume
					, "HC08", ZString.Empty, ZString.Empty
					, null
					, ZString.Empty, "Sub02"
					, "Ref08", "ImportRef08", "ExportRef08"
					, "OuturnComment08", Core.Constants.CargoTypes.Hazardous
					, ZString.Empty, ZString.Empty
					, true, -2M, 3M, Constants.Temperature.Centigrade);
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipmentDOs = GetShipmentDOs(consol);
				AssertEquals(1, shipmentDOs.Count);

				new PackageGroupingHelper(consol, Context)
					.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByShipment, false, DataContext.BookingRequest, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				var shipmentDO = shipmentDOs.First();
				AssertEquals(1, shipmentDO.PackingLines.Count);

				var groupedPackingLine = shipmentDO.PackingLines.First();
				AssertEquals(5, groupedPackingLine.PackingLines.Count);

				AssertPackingLineDetails(groupedPackingLine, ZString.Empty
					, 35551, Constants.PkgUnit.BaleCompressed
					, 24562M, shipment1.JS_UnitOfWeight
					, 35551M, shipment1.JS_UnitOfVolume
					, "HC01, HC04, HC07, HC08, HC09, HC10", "HS (US): USHS, HSUS", "HS (CN): CNHS"
					, new[] { "HS (US): USHS", "HS (CN): CNHS", "HS (US): HSUS" }
					, "BKG:11321", "Sub01, Sub02"
					, "Ref01, Ref04, Ref07, Ref08, Ref10", "ImportRef01, ImportRef04, ImportRef07, ImportRef08, ImportRef09, ImportRef10", "ExportRef01, ExportRef04, ExportRef07, ExportRef08, ExportRef10"
					, "OuturnComment01, OuturnComment04, OuturnComment07, OuturnComment08, OuturnComment09, OuturnComment10", Core.Constants.CargoTypes.Hazardous
					, "ASM JS_MarksAndNumbers", "ASM JS_GoodsDescription"
					, true, 1M, 2M, Constants.Temperature.Centigrade);

				var consolidatedPackingLineContainer1 = groupedPackingLine.PackingLines.First(x => x.ContainerNumber == "CONTAINER1");
				var consolidatedPackingLineContainer2 = groupedPackingLine.PackingLines.First(x => x.ContainerNumber == "CONTAINER2");
				var consolidatedPackingLineContainer4 = groupedPackingLine.PackingLines.First(x => x.Identifier.ToString().Substring(0, 36) == container4.PK.ToString());
				var consolidatedPackingLineContainer5 = groupedPackingLine.PackingLines.First(x => x.Identifier.ToString().Substring(0, 36) == container5.PK.ToString());
				var consolidatedPackingLineNoContainer = groupedPackingLine.PackingLines.First(x => x.Identifier.ToString().Substring(0, 36) == ZGuid.Empty.ToString());

				AssertPackingLineDetails(consolidatedPackingLineContainer1, "CONTAINER1"
					, 100, Constants.PkgUnit.BaleCompressed
					, 100M, shipment1.JS_UnitOfWeight
					, 100M, shipment1.JS_UnitOfVolume
					, "HC01", "HS (US): USHS", "HS (CN): CNHS"
					, new[] { "HS (US): USHS", "HS (CN): CNHS" }
					, "BKG:11321", "Sub01"
					, "Ref01", "ImportRef01", "ExportRef01"
					, "OuturnComment01", Core.Constants.CargoTypes.Hazardous
					, "", ZString.Empty
					, true, -2M, 3M, Constants.Temperature.Centigrade);
				AssertPackingLineDetails(consolidatedPackingLineContainer2, "CONTAINER2"
					, 12110, Constants.PkgUnit.BaleCompressed
					, 12110M, shipment1.JS_UnitOfWeight
					, 12110M, shipment1.JS_UnitOfVolume
					, "HC04, HC09", ZString.Empty, ZString.Empty
					, null
					, "BKG:11321", "Sub01, Sub02"
					, "Ref04, Ref01", "ImportRef04, ImportRef09", "ExportRef04, ExportRef01"
					, "OuturnComment04, OuturnComment09", Core.Constants.CargoTypes.Hazardous
					, ZString.Empty, ZString.Empty
					, true, -2M, 2M, Constants.Temperature.Centigrade);
				AssertPackingLineDetails(consolidatedPackingLineContainer4, ZString.Empty
					, 120, Constants.PkgUnit.BaleCompressed
					, 120M, shipment1.JS_UnitOfWeight
					, 120M, shipment1.JS_UnitOfVolume
					, "HC07", ZString.Empty, ZString.Empty
					, null
					, ZString.Empty, "Sub02"
					, "Ref07", "ImportRef07", "ExportRef07"
					, "OuturnComment07", Core.Constants.CargoTypes.Hazardous
					, ZString.Empty, ZString.Empty
					, true, -2M, 3M, Constants.Temperature.Centigrade);
				AssertPackingLineDetails(consolidatedPackingLineContainer5, ZString.Empty
					, 121, Constants.PkgUnit.BaleCompressed
					, 121M, shipment1.JS_UnitOfWeight
					, 121M, shipment1.JS_UnitOfVolume
					, "HC08", ZString.Empty, ZString.Empty
					, null
					, ZString.Empty, "Sub02"
					, "Ref08", "ImportRef08", "ExportRef08"
					, "OuturnComment08", Core.Constants.CargoTypes.Hazardous
					, ZString.Empty, ZString.Empty
					, true, -2M, 3M, Constants.Temperature.Centigrade);
				AssertPackingLineDetails(consolidatedPackingLineNoContainer, ZString.Empty
					, 23100, Constants.PkgUnit.BaleCompressed
					, 12111M, shipment1.JS_UnitOfWeight
					, 23100M, shipment1.JS_UnitOfVolume
					, "HC07, HC10", "HS (US): HSUS", "HS (CN): CNHS"
					, new[] { "HS (CN): CNHS", "HS (US): HSUS" }
					, "BKG:11321", "Sub01, Sub02"
					, "Ref07, Ref10", "ImportRef07, ImportRef10", "ExportRef07, ExportRef10"
					, "OuturnComment07, OuturnComment10", Core.Constants.CargoTypes.Hazardous
					, ZString.Empty, ZString.Empty
					, true, 1M, 3M, Constants.Temperature.Centigrade);
			}
		}

		void AssertPackingLineDetails(PackingLine packingLine, ZString containerNumber
			, ZInt quantity, ZString packageType
			, ZDecimal weight, ZString unitOfWeight
			, ZDecimal volume, ZString unitOfVolume
			, ZString harmonizedCode, ZString exportHarmonizedCode, ZString importHarmonizedCode
			, string[] harmonizedCodes
			, ZString shipmentEntryNumbers, ZString shipmentID
			, ZString referenceNumber, ZString importReferenceNumber, ZString exportReferenceNumber
			, ZString outturnComment, ZString commodityCode
			, ZString marksAndNumbers, ZString goodsDescription
			, ZBool requiresTemperatureControl, ZDecimal requiredTemperatureMinimum, ZDecimal requiredTemperatureMaximum, ZString requiredTemperatureUnit)
		{
			CombineAssertions(() =>
			{
				AssertEquals("ContainerNumber", containerNumber, packingLine.ContainerNumber);

				AssertEquals("Quantity", quantity, packingLine.Quantity);
				AssertEquals("PackageType", packageType, packingLine.PackageType.Code);

				AssertEquals("Weight", weight, packingLine.Weight.Value);
				AssertEquals("Weight Unit", unitOfWeight, packingLine.Weight.Unit.Code);

				AssertEquals("Volume", volume, packingLine.Volume.Value);
				AssertEquals("Volume Unit", unitOfVolume, packingLine.Volume.Unit.Code);

				AssertEquals("HarmonizedCode", harmonizedCode, packingLine.HarmonizedCode.ToString());
				AssertEquals("ExportHarmonizedCode", exportHarmonizedCode, packingLine.ExportHarmonizedCode.ToString());
				AssertEquals("ImportHarmonizedCode", importHarmonizedCode, packingLine.ImportHarmonizedCode.ToString());

				if ((packingLine.HarmonizedCodes?.Any() ?? false) || harmonizedCodes != null)
				{
					AssertContainsExactElementsInAnyOrder("HarmonizedCodes", harmonizedCodes, packingLine.HarmonizedCodes.Select(x => x.ToString()));
				}

				AssertEquals("ShipmentEntryNumbers", shipmentEntryNumbers, packingLine.ShipmentEntryNumbers);
				AssertEquals("ShipmentID", shipmentID, packingLine.ShipmentID);

				AssertEquals("ReferenceNumber", referenceNumber, packingLine.ReferenceNumber);
				AssertEquals("ImportReferenceNumber", importReferenceNumber, packingLine.ImportReferenceNumber);
				AssertEquals("ExportReferenceNumber", exportReferenceNumber, packingLine.ExportReferenceNumber);

				AssertEquals("OutturnComment", outturnComment, packingLine.OutturnComment);

				if (packingLine.Commodity != null || !commodityCode.IsEmpty)
				{
					AssertEquals("Commodity", commodityCode, packingLine.Commodity.Code);
				}

				AssertEquals("MarksAndNumbers", marksAndNumbers, packingLine.MarksAndNumbers);
				AssertEquals("GoodsDescription", goodsDescription, packingLine.GoodsDescription);

				AssertEquals("RequiresTemperatureControl", requiresTemperatureControl, packingLine.RequiresTemperatureControl);
				AssertEquals("TemperatureMinimum", requiredTemperatureMinimum, packingLine.TemperatureMinimum.Value);
				AssertEquals("TemperatureMaximum", requiredTemperatureMaximum, packingLine.TemperatureMaximum.Value);
				AssertEquals("Temperature unit", requiredTemperatureUnit, packingLine.TemperatureMinimum.Unit.Code);
			});
		}

		public void TestPopulatePackingLinesHouseBillPaymentType_GroupedAndConsolidated()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "Fudge burners";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.MainAddress.Address1 = "line 1";
			org.MainAddress.Address2 = "line 2";
			org.MainAddress.City = "Sydney";
			org.MainAddress.Postcode = "2000";

			var contact = Factory.New<OrgContact>();
			contact.OC_OH = org.PK;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "CNSHG";
			consol.JK_ConsolMode = Constants.ContainerModes.LCL;

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONTAINER1";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONTAINER2";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
			shipment1.JS_GoodsDescription = "ASM JS_GoodsDescription";
			shipment1.JS_MarksAndNumbers = "ASM JS_MarksAndNumbers";

			var job = new JobHeader.Loader(Factory, shipment1).TryLoadOrCreate();
			job.JH_OA_LocalChargesAddr = org.MainAddress.PK;
			job.JH_OC_LocalBillingContact = contact.PK;

			var orgHeader = (OrgHeader)job.LocalZAddressWithContact.OrgHeader;
			orgHeader.CompanyData.OB_ARCreditAgreedPaymentMethod = "CBC";

			var shipment1SubShipment1 = shipment1.CoLoadShipments.AddNew();
			var cusEntryNumber = shipment1SubShipment1.CusEntryNumbers.AddNew();
			cusEntryNumber.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG;
			cusEntryNumber.CE_EntryNum = "11321";

			PopulatePackLine(shipment1SubShipment1.OuterPackLines.AddNew()
				, 100, Constants.PkgUnit.BaleCompressed
				, 100, Constants.Weight.Kilograms
				, 100, Constants.Volume.CubicMetres
				, "BaleCompressed1 MarksAndNumbers", "BaleCompressed1 DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, container1.PK, "HC01"
				, "Ref01", "ImportRef01", "ExportRef01", "OutTurnComment01", Core.Constants.CargoTypes.Hazardous);

			PopulatePackLine(shipment1SubShipment1.OuterPackLines.AddNew()
				, 110, Constants.PkgUnit.Tube
				, 110, Constants.Weight.Kilograms
				, 110, Constants.Volume.CubicMetres
				, "Tube MarksAndNumbers", "Tube DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, container2.PK, "HC04"
				, "Ref04", "ImportRef04", "ExportRef04", "OutTurnComment04", Core.Constants.CargoTypes.Hazardous);

			var shipment1SubShipment2 = shipment1.CoLoadShipments.AddNew();

			PopulatePackLine(shipment1SubShipment2.OuterPackLines.AddNew()
				, 12000, Constants.PkgUnit.Tube
				, 12000, Constants.Weight.Kilograms
				, 12000, Constants.Volume.CubicMetres
				, "Tube MarksAndNumbers", "Tube DetailedDescription"
				, true, -2, 2, Constants.Temperature.Centigrade
				, container2.PK, "HC09",
				"Ref01", "ImportRef09", "ExportRef01", "OutTurnComment09", Core.Constants.CargoTypes.Hazardous);

			shipment1SubShipment2.OuterPackLines.AddNew();

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var packageGroupingHelper = new PackageGroupingHelper(consol, Context);
				var shipmentDOs = GetShipmentDOs(consol);

				packageGroupingHelper.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByPackLine, false, DataContext.ShippingInstruction, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
				shipmentDOs.First().PackingLines.ForEach(packingLine =>
				{
					AssertEquals("A", packingLine.HBLPaymentType);
				});

				shipmentDOs = GetShipmentDOs(consol);
				packageGroupingHelper.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByShipment, false, DataContext.ShippingInstruction, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
				shipmentDOs.First().PackingLines.ForEach(packingLine =>
				{
					AssertEquals("A", packingLine.HBLPaymentType);
				});

				orgHeader.CompanyData.OB_ARCreditAgreedPaymentMethod = "";

				shipmentDOs = GetShipmentDOs(consol);
				packageGroupingHelper.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByPackLine, false, DataContext.ShippingInstruction, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
				AssertEquals("D", shipmentDOs.First().PackingLines.First().HBLPaymentType);

				shipmentDOs = GetShipmentDOs(consol);
				packageGroupingHelper.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByShipment, false, DataContext.ShippingInstruction, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
				AssertEquals("D", shipmentDOs.First().PackingLines.First().HBLPaymentType);
			}
		}

		#endregion

		#region PopulateConsolidatedPackingLineDangerousGoodsForDoNotGroup

		public void TestPopulateConsolidatedPackingLineDangerousGoodsForDoNotGroup()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "CNSHG";
			consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONTAINER1";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "JSASM";
			shipment.JS_GoodsDescription = "ASM JS_GoodsDescription";
			shipment.JS_MarksAndNumbers = "ASM JS_MarksAndNumbers";

			shipment.InnerPackLines.RemoveAndDeleteAll();
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var outerPackLine1 = shipment.OuterPackLines.AddNew();
			PopulatePackLine(outerPackLine1
				, 120, Constants.PkgUnit.BaleCompressed
				, 120, Constants.Weight.Kilograms
				, 120, Constants.Volume.CubicMetres
				, "BaleCompressed MarksAndNumbers", "BaleCompressed DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, container.PK, "HC07"
				, "Ref07", "ImportRef07", "ExportRef07", "OuturnComment07", Core.Constants.CargoTypes.Hazardous);

			var outerPackLine2 = shipment.OuterPackLines.AddNew();
			PopulatePackLine(outerPackLine2
				, 121, Constants.PkgUnit.Tote
				, 121, Constants.Weight.Kilograms
				, 121, Constants.Volume.CubicMetres
				, "Tote MarksAndNumbers", "Tote DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, container.PK, "HC08"
				, "Ref08", "ImportRef08", "ExportRef08", "OuturnComment08", Core.Constants.CargoTypes.Hazardous);

			var undgSubstance1 = CreatUNDGSubstance("9999", "12", "9.9Z", "27.0 c.c", "9,9", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, UNDGSubstanceLookups.StateTypes.Code.Liquid
				, "E0", UNDGSubstanceLookups.PackingGroupTypes.HighDangerCode, "AIR BAG MODULES", "F-I,S-S");
			var undgSubstance2 = CreatUNDGSubstance("9999", "13", "9.8Z", "28.0 c.c", "1,1", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO, UNDGSubstanceLookups.StateTypes.Code.ExplosiveSubstance
				, "E1", UNDGSubstanceLookups.PackingGroupTypes.MediumDangerCode, "SEA BAG MODULES", "F-B,S-A");

			var undgContact1 = Factory.NewWithValidTestData<OrgHeader>();
			var dgContact1 = undgContact1.Contacts.AddNew();
			dgContact1.OC_ContactName = "ABC";
			dgContact1.OC_Phone = "123456";

			var undgContact2 = Factory.NewWithValidTestData<OrgHeader>();
			var dgContact2 = undgContact2.Contacts.AddNew();
			dgContact2.OC_ContactName = "ABC";
			dgContact2.OC_Phone = "78919";

			PopulateDangerousGoods(outerPackLine1.UNDGs.AddNew(), 101, true, undgSubstance1.PK, dgContact1.PK, 10, Constants.Weight.Kilograms, "1", 2M);
			PopulateDangerousGoods(outerPackLine1.UNDGs.AddNew(), 102, true, undgSubstance2.PK, dgContact1.PK, 130, Constants.Weight.Kilograms, "1", 2M);
			PopulateDangerousGoods(outerPackLine1.UNDGs.AddNew(), 103, false, undgSubstance1.PK, dgContact2.PK, 30, Constants.Weight.Grams, "1", 3M);
			PopulateDangerousGoods(outerPackLine1.UNDGs.AddNew(), 104, false, undgSubstance1.PK, dgContact2.PK, 30, Constants.Weight.Grams, "1", 3M);

			PopulateDangerousGoods(outerPackLine2.UNDGs.AddNew(), 105, true, undgSubstance1.PK, dgContact1.PK, 20, Constants.Weight.Kilograms, "1", 2M);
			PopulateDangerousGoods(outerPackLine2.UNDGs.AddNew(), 106, true, undgSubstance1.PK, dgContact1.PK, 20, Constants.Weight.Kilograms, "2", 2M);
			PopulateDangerousGoods(outerPackLine2.UNDGs.AddNew(), 107, true, undgSubstance2.PK, dgContact1.PK, 120, Constants.Weight.Kilograms, "1", 2M);

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var shipmentDOs = GetShipmentDOs(consol);
				AssertEquals(1, shipmentDOs.Count);

				new PackageGroupingHelper(consol, Context)
					.PopulateConsolidatedPackingLineDangerousGoodsForDoNotGroup(shipmentDOs, Constants.PackageGrouping.Codes.DoNotGroup, DataContext.BookingRequest, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				var shipmentDO = shipmentDOs.First();
				var packingLine1DangerousGoodsDO = shipmentDO.AllPackingLinesIncludeCoLoad.First(x => x.Quantity == 120).DangerousGoods;
				var packingLine2DangerousGoodsDO = shipmentDO.AllPackingLinesIncludeCoLoad.First(x => x.Quantity == 121).DangerousGoods;

				AssertEquals(4, packingLine1DangerousGoodsDO.Count);
				AssertEquals(3, packingLine2DangerousGoodsDO.Count);

				AssertDangerousGoodDetails(packingLine1DangerousGoodsDO.First(x => x.Quantity == 101), 101, 10M, Constants.Weight.Kilograms, "", true
					, "9.9Z", "9,9", "999912", 2M, UNDGSubstanceLookups.PackingGroupTypes.HighDangerCode, "AIR BAG MODULES", "1", ""
					, "ABC", "123456", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, UNDGSubstanceLookups.StateTypes.Code.Liquid, "E0", "F-I", "S-S", "9999", "12");

				AssertDangerousGoodDetails(packingLine1DangerousGoodsDO.First(x => x.Quantity == 102), 102, 130M, Constants.Weight.Kilograms, "", true
					, "9.8Z", "1,1", "999913", 2M, UNDGSubstanceLookups.PackingGroupTypes.MediumDangerCode, "SEA BAG MODULES", "1", ""
					, "ABC", "123456", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO, UNDGSubstanceLookups.StateTypes.Code.ExplosiveSubstance, "E1", "F-B", "S-A", "9999", "13");

				AssertDangerousGoodDetails(packingLine1DangerousGoodsDO.First(x => x.Quantity == 103), 103, 30M, Constants.Weight.Grams, "", false
					, "9.9Z", "9,9", "999912", 3M, UNDGSubstanceLookups.PackingGroupTypes.HighDangerCode, "AIR BAG MODULES", "1", ""
					, "ABC", "78919", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, UNDGSubstanceLookups.StateTypes.Code.Liquid, "E0", "F-I", "S-S", "9999", "12");

				AssertDangerousGoodDetails(packingLine1DangerousGoodsDO.First(x => x.Quantity == 104), 104, 30M, Constants.Weight.Grams, "", false
					, "9.9Z", "9,9", "999912", 3M, UNDGSubstanceLookups.PackingGroupTypes.HighDangerCode, "AIR BAG MODULES", "1", ""
					, "ABC", "78919", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, UNDGSubstanceLookups.StateTypes.Code.Liquid, "E0", "F-I", "S-S", "9999", "12");

				AssertDangerousGoodDetails(packingLine2DangerousGoodsDO.First(x => x.Quantity == 105), 105, 20M, Constants.Weight.Kilograms, "", true
					, "9.9Z", "9,9", "999912", 2M, UNDGSubstanceLookups.PackingGroupTypes.HighDangerCode, "AIR BAG MODULES", "1", ""
					, "ABC", "123456", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, UNDGSubstanceLookups.StateTypes.Code.Liquid, "E0", "F-I", "S-S", "9999", "12");

				AssertDangerousGoodDetails(packingLine2DangerousGoodsDO.First(x => x.Quantity == 106), 106, 20M, Constants.Weight.Kilograms, "", true
					, "9.9Z", "9,9", "999912", 2M, UNDGSubstanceLookups.PackingGroupTypes.HighDangerCode, "AIR BAG MODULES", "2", ""
					, "ABC", "123456", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, UNDGSubstanceLookups.StateTypes.Code.Liquid, "E0", "F-I", "S-S", "9999", "12");

				AssertDangerousGoodDetails(packingLine2DangerousGoodsDO.First(x => x.Quantity == 107), 107, 120M, Constants.Weight.Kilograms, "", true
					, "9.8Z", "1,1", "999913", 2M, UNDGSubstanceLookups.PackingGroupTypes.MediumDangerCode, "SEA BAG MODULES", "1", ""
					, "ABC", "123456", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO, UNDGSubstanceLookups.StateTypes.Code.ExplosiveSubstance, "E1", "F-B", "S-A", "9999", "13");
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipmentDOs = GetShipmentDOs(consol);
				AssertEquals(1, shipmentDOs.Count);

				new PackageGroupingHelper(consol, Context)
					.PopulateConsolidatedPackingLineDangerousGoodsForDoNotGroup(shipmentDOs, Constants.PackageGrouping.Codes.DoNotGroup, DataContext.BookingRequest, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				var shipmentDO = shipmentDOs.First();
				var packingLine1DangerousGoodsDO = shipmentDO.AllPackingLinesIncludeCoLoad.First(x => x.Quantity == 120).DangerousGoods;
				var packingLine2DangerousGoodsDO = shipmentDO.AllPackingLinesIncludeCoLoad.First(x => x.Quantity == 121).DangerousGoods;

				AssertEquals(3, packingLine1DangerousGoodsDO.Count);
				AssertEquals(3, packingLine2DangerousGoodsDO.Count);

				AssertDangerousGoodDetails(packingLine1DangerousGoodsDO.First(x => x.Quantity == 101), 101, 10M, Constants.Weight.Kilograms, "", true
					, "9.9Z", "9,9", "999912", 2M, UNDGSubstanceLookups.PackingGroupTypes.HighDangerCode, "AIR BAG MODULES", "1", ""
					, "ABC", "123456", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, UNDGSubstanceLookups.StateTypes.Code.Liquid, "E0", "F-I", "S-S", "9999", "12");

				AssertDangerousGoodDetails(packingLine1DangerousGoodsDO.First(x => x.Quantity == 102), 102, 130M, Constants.Weight.Kilograms, "", true
					, "9.8Z", "1,1", "999913", 2M, UNDGSubstanceLookups.PackingGroupTypes.MediumDangerCode, "SEA BAG MODULES", "1", ""
					, "ABC", "123456", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO, UNDGSubstanceLookups.StateTypes.Code.ExplosiveSubstance, "E1", "F-B", "S-A", "9999", "13");

				AssertDangerousGoodDetails(packingLine1DangerousGoodsDO.First(x => x.Quantity == 207), 207, 0.06M, Constants.Weight.Kilograms, "", false
					, "9.9Z", "9,9", "999912", 3M, UNDGSubstanceLookups.PackingGroupTypes.HighDangerCode, "AIR BAG MODULES", "1", ""
					, "ABC", "78919", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, UNDGSubstanceLookups.StateTypes.Code.Liquid, "E0", "F-I", "S-S", "9999", "12");

				AssertDangerousGoodDetails(packingLine2DangerousGoodsDO.First(x => x.Quantity == 105), 105, 20M, Constants.Weight.Kilograms, "", true
					, "9.9Z", "9,9", "999912", 2M, UNDGSubstanceLookups.PackingGroupTypes.HighDangerCode, "AIR BAG MODULES", "1", ""
					, "ABC", "123456", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, UNDGSubstanceLookups.StateTypes.Code.Liquid, "E0", "F-I", "S-S", "9999", "12");

				AssertDangerousGoodDetails(packingLine2DangerousGoodsDO.First(x => x.Quantity == 106), 106, 20M, Constants.Weight.Kilograms, "", true
					, "9.9Z", "9,9", "999912", 2M, UNDGSubstanceLookups.PackingGroupTypes.HighDangerCode, "AIR BAG MODULES", "2", ""
					, "ABC", "123456", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, UNDGSubstanceLookups.StateTypes.Code.Liquid, "E0", "F-I", "S-S", "9999", "12");

				AssertDangerousGoodDetails(packingLine2DangerousGoodsDO.First(x => x.Quantity == 107), 107, 120M, Constants.Weight.Kilograms, "", true
					, "9.8Z", "1,1", "999913", 2M, UNDGSubstanceLookups.PackingGroupTypes.MediumDangerCode, "SEA BAG MODULES", "1", ""
					, "ABC", "123456", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO, UNDGSubstanceLookups.StateTypes.Code.ExplosiveSubstance, "E1", "F-B", "S-A", "9999", "13");
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipmentDOs = GetShipmentDOs(consol);
				AssertEquals(1, shipmentDOs.Count);

				new PackageGroupingHelper(consol, Context)
					.PopulateConsolidatedPackingLineDangerousGoodsForDoNotGroup(shipmentDOs, Constants.PackageGrouping.Codes.GroupByShipment, DataContext.BookingRequest, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				var shipmentDO = shipmentDOs.First();
				var packingLine1DangerousGoodsDO = shipmentDO.AllPackingLinesIncludeCoLoad.First(x => x.Quantity == 120).DangerousGoods;
				var packingLine2DangerousGoodsDO = shipmentDO.AllPackingLinesIncludeCoLoad.First(x => x.Quantity == 121).DangerousGoods;

				AssertEquals(4, packingLine1DangerousGoodsDO.Count);
				AssertEquals(3, packingLine2DangerousGoodsDO.Count);

				AssertDangerousGoodDetails(packingLine1DangerousGoodsDO.First(x => x.Quantity == 101), 101, 10M, Constants.Weight.Kilograms, "", true
					, "9.9Z", "9,9", "999912", 2M, UNDGSubstanceLookups.PackingGroupTypes.HighDangerCode, "AIR BAG MODULES", "1", ""
					, "ABC", "123456", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, UNDGSubstanceLookups.StateTypes.Code.Liquid, "E0", "F-I", "S-S", "9999", "12");

				AssertDangerousGoodDetails(packingLine1DangerousGoodsDO.First(x => x.Quantity == 102), 102, 130M, Constants.Weight.Kilograms, "", true
					, "9.8Z", "1,1", "999913", 2M, UNDGSubstanceLookups.PackingGroupTypes.MediumDangerCode, "SEA BAG MODULES", "1", ""
					, "ABC", "123456", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO, UNDGSubstanceLookups.StateTypes.Code.ExplosiveSubstance, "E1", "F-B", "S-A", "9999", "13");

				AssertDangerousGoodDetails(packingLine1DangerousGoodsDO.First(x => x.Quantity == 103), 103, 30M, Constants.Weight.Grams, "", false
					, "9.9Z", "9,9", "999912", 3M, UNDGSubstanceLookups.PackingGroupTypes.HighDangerCode, "AIR BAG MODULES", "1", ""
					, "ABC", "78919", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, UNDGSubstanceLookups.StateTypes.Code.Liquid, "E0", "F-I", "S-S", "9999", "12");

				AssertDangerousGoodDetails(packingLine1DangerousGoodsDO.First(x => x.Quantity == 104), 104, 30M, Constants.Weight.Grams, "", false
					, "9.9Z", "9,9", "999912", 3M, UNDGSubstanceLookups.PackingGroupTypes.HighDangerCode, "AIR BAG MODULES", "1", ""
					, "ABC", "78919", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, UNDGSubstanceLookups.StateTypes.Code.Liquid, "E0", "F-I", "S-S", "9999", "12");

				AssertDangerousGoodDetails(packingLine2DangerousGoodsDO.First(x => x.Quantity == 105), 105, 20M, Constants.Weight.Kilograms, "", true
					, "9.9Z", "9,9", "999912", 2M, UNDGSubstanceLookups.PackingGroupTypes.HighDangerCode, "AIR BAG MODULES", "1", ""
					, "ABC", "123456", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, UNDGSubstanceLookups.StateTypes.Code.Liquid, "E0", "F-I", "S-S", "9999", "12");

				AssertDangerousGoodDetails(packingLine2DangerousGoodsDO.First(x => x.Quantity == 106), 106, 20M, Constants.Weight.Kilograms, "", true
					, "9.9Z", "9,9", "999912", 2M, UNDGSubstanceLookups.PackingGroupTypes.HighDangerCode, "AIR BAG MODULES", "2", ""
					, "ABC", "123456", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, UNDGSubstanceLookups.StateTypes.Code.Liquid, "E0", "F-I", "S-S", "9999", "12");

				AssertDangerousGoodDetails(packingLine2DangerousGoodsDO.First(x => x.Quantity == 107), 107, 120M, Constants.Weight.Kilograms, "", true
					, "9.8Z", "1,1", "999913", 2M, UNDGSubstanceLookups.PackingGroupTypes.MediumDangerCode, "SEA BAG MODULES", "1", ""
					, "ABC", "123456", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO, UNDGSubstanceLookups.StateTypes.Code.ExplosiveSubstance, "E1", "F-B", "S-A", "9999", "13");
			}
		}

		public void TestPopulateConsolidatedPackingLineDangerousGoodsForDoNotGroup_SubShipments()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "CNSHG";
			consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONTAINER1";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
			shipment.JS_UniqueConsignRef = "JSASM";
			shipment.JS_GoodsDescription = "ASM JS_GoodsDescription";
			shipment.JS_MarksAndNumbers = "ASM JS_MarksAndNumbers";

			var subShipment = shipment.CoLoadShipments.AddNew();
			subShipment.JS_UniqueConsignRef = "JSASM";
			subShipment.JS_GoodsDescription = "ASM JS_GoodsDescription";
			subShipment.JS_MarksAndNumbers = "ASM JS_MarksAndNumbers";

			subShipment.InnerPackLines.RemoveAndDeleteAll();
			subShipment.OuterPackLines.RemoveAndDeleteAll();

			var outerPackLine1 = subShipment.OuterPackLines.AddNew();
			PopulatePackLine(outerPackLine1
				, 120, Constants.PkgUnit.BaleCompressed
				, 120, Constants.Weight.Kilograms
				, 120, Constants.Volume.CubicMetres
				, "BaleCompressed MarksAndNumbers", "BaleCompressed DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, container.PK, "HC07"
				, "Ref07", "ImportRef07", "ExportRef07", "OuturnComment07", Core.Constants.CargoTypes.Hazardous);

			var outerPackLine2 = subShipment.OuterPackLines.AddNew();
			PopulatePackLine(outerPackLine2
				, 121, Constants.PkgUnit.Tote
				, 121, Constants.Weight.Kilograms
				, 121, Constants.Volume.CubicMetres
				, "Tote MarksAndNumbers", "Tote DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, container.PK, "HC08"
				, "Ref08", "ImportRef08", "ExportRef08", "OuturnComment08", Core.Constants.CargoTypes.Hazardous);

			var undgSubstance1 = CreatUNDGSubstance("9999", "12", "9.9Z", "27.0 c.c", "9,9", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, UNDGSubstanceLookups.StateTypes.Code.Liquid
				, "E0", UNDGSubstanceLookups.PackingGroupTypes.HighDangerCode, "AIR BAG MODULES", "F-I,S-S");
			var undgSubstance2 = CreatUNDGSubstance("9999", "13", "9.8Z", "28.0 c.c", "1,1", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO, UNDGSubstanceLookups.StateTypes.Code.ExplosiveSubstance
				, "E1", UNDGSubstanceLookups.PackingGroupTypes.MediumDangerCode, "SEA BAG MODULES", "F-B,S-A");

			var undgContact1 = Factory.NewWithValidTestData<OrgHeader>();
			var dgContact1 = undgContact1.Contacts.AddNew();
			dgContact1.OC_ContactName = "ABC";
			dgContact1.OC_Phone = "123456";

			var undgContact2 = Factory.NewWithValidTestData<OrgHeader>();
			var dgContact2 = undgContact2.Contacts.AddNew();
			dgContact2.OC_ContactName = "ABC";
			dgContact2.OC_Phone = "78919";

			PopulateDangerousGoods(outerPackLine1.UNDGs.AddNew(), 101, true, undgSubstance1.PK, dgContact1.PK, 10, Constants.Weight.Kilograms, "1", 2M);
			PopulateDangerousGoods(outerPackLine1.UNDGs.AddNew(), 102, true, undgSubstance2.PK, dgContact1.PK, 130, Constants.Weight.Kilograms, "1", 2M);
			PopulateDangerousGoods(outerPackLine1.UNDGs.AddNew(), 103, false, undgSubstance1.PK, dgContact2.PK, 30, Constants.Weight.Grams, "1", 3M);
			PopulateDangerousGoods(outerPackLine1.UNDGs.AddNew(), 104, false, undgSubstance1.PK, dgContact2.PK, 30, Constants.Weight.Grams, "1", 3M);

			PopulateDangerousGoods(outerPackLine2.UNDGs.AddNew(), 105, true, undgSubstance1.PK, dgContact1.PK, 20, Constants.Weight.Kilograms, "1", 2M);
			PopulateDangerousGoods(outerPackLine2.UNDGs.AddNew(), 106, true, undgSubstance1.PK, dgContact1.PK, 20, Constants.Weight.Kilograms, "2", 2M);
			PopulateDangerousGoods(outerPackLine2.UNDGs.AddNew(), 107, true, undgSubstance2.PK, dgContact1.PK, 120, Constants.Weight.Kilograms, "1", 2M);

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipmentDOs = GetShipmentDOs(consol);
				AssertEquals(1, shipmentDOs.Count);

				new PackageGroupingHelper(consol, Context)
					.PopulateConsolidatedPackingLineDangerousGoodsForDoNotGroup(shipmentDOs, Constants.PackageGrouping.Codes.DoNotGroup, DataContext.BookingRequest, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				var shipmentDO = shipmentDOs.First();
				var packingLine1DangerousGoodsDO = shipmentDO.AllPackingLinesIncludeCoLoad.First(x => x.Quantity == 120).DangerousGoods;
				var packingLine2DangerousGoodsDO = shipmentDO.AllPackingLinesIncludeCoLoad.First(x => x.Quantity == 121).DangerousGoods;

				AssertEquals(3, packingLine1DangerousGoodsDO.Count);
				AssertEquals(3, packingLine2DangerousGoodsDO.Count);

				AssertDangerousGoodDetails(packingLine1DangerousGoodsDO.First(x => x.Quantity == 101), 101, 10M, Constants.Weight.Kilograms, "", true
					, "9.9Z", "9,9", "999912", 2M, UNDGSubstanceLookups.PackingGroupTypes.HighDangerCode, "AIR BAG MODULES", "1", ""
					, "ABC", "123456", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, UNDGSubstanceLookups.StateTypes.Code.Liquid, "E0", "F-I", "S-S", "9999", "12");

				AssertDangerousGoodDetails(packingLine1DangerousGoodsDO.First(x => x.Quantity == 102), 102, 130M, Constants.Weight.Kilograms, "", true
					, "9.8Z", "1,1", "999913", 2M, UNDGSubstanceLookups.PackingGroupTypes.MediumDangerCode, "SEA BAG MODULES", "1", ""
					, "ABC", "123456", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO, UNDGSubstanceLookups.StateTypes.Code.ExplosiveSubstance, "E1", "F-B", "S-A", "9999", "13");

				AssertDangerousGoodDetails(packingLine1DangerousGoodsDO.First(x => x.Quantity == 207), 207, 0.06M, Constants.Weight.Kilograms, "", false
					, "9.9Z", "9,9", "999912", 3M, UNDGSubstanceLookups.PackingGroupTypes.HighDangerCode, "AIR BAG MODULES", "1", ""
					, "ABC", "78919", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, UNDGSubstanceLookups.StateTypes.Code.Liquid, "E0", "F-I", "S-S", "9999", "12");

				AssertDangerousGoodDetails(packingLine2DangerousGoodsDO.First(x => x.Quantity == 105), 105, 20M, Constants.Weight.Kilograms, "", true
					, "9.9Z", "9,9", "999912", 2M, UNDGSubstanceLookups.PackingGroupTypes.HighDangerCode, "AIR BAG MODULES", "1", ""
					, "ABC", "123456", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, UNDGSubstanceLookups.StateTypes.Code.Liquid, "E0", "F-I", "S-S", "9999", "12");

				AssertDangerousGoodDetails(packingLine2DangerousGoodsDO.First(x => x.Quantity == 106), 106, 20M, Constants.Weight.Kilograms, "", true
					, "9.9Z", "9,9", "999912", 2M, UNDGSubstanceLookups.PackingGroupTypes.HighDangerCode, "AIR BAG MODULES", "2", ""
					, "ABC", "123456", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, UNDGSubstanceLookups.StateTypes.Code.Liquid, "E0", "F-I", "S-S", "9999", "12");

				AssertDangerousGoodDetails(packingLine2DangerousGoodsDO.First(x => x.Quantity == 107), 107, 120M, Constants.Weight.Kilograms, "", true
					, "9.8Z", "1,1", "999913", 2M, UNDGSubstanceLookups.PackingGroupTypes.MediumDangerCode, "SEA BAG MODULES", "1", ""
					, "ABC", "123456", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO, UNDGSubstanceLookups.StateTypes.Code.ExplosiveSubstance, "E1", "F-B", "S-A", "9999", "13");
			}
		}

		#endregion

		#region PopulateConsolidatedPackingLineDangerousGoods

		public void TestPopulateConsolidatedPackingLineDangerousGoods()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "CNSHG";
			consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONTAINER1";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "JSASM";
			shipment.JS_GoodsDescription = "ASM JS_GoodsDescription";
			shipment.JS_MarksAndNumbers = "ASM JS_MarksAndNumbers";

			shipment.InnerPackLines.RemoveAndDeleteAll();
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var outerPackLine1 = shipment.OuterPackLines.AddNew();
			PopulatePackLine(outerPackLine1
				, 120, Constants.PkgUnit.BaleCompressed
				, 120, Constants.Weight.Kilograms
				, 120, Constants.Volume.CubicMetres
				, "BaleCompressed MarksAndNumbers", "BaleCompressed DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, container.PK, "HC07"
				, "Ref07", "ImportRef07", "ExportRef07", "OuturnComment07", Core.Constants.CargoTypes.Hazardous);

			var outerPackLine2 = shipment.OuterPackLines.AddNew();
			PopulatePackLine(outerPackLine2
				, 121, Constants.PkgUnit.Tote
				, 121, Constants.Weight.Kilograms
				, 121, Constants.Volume.CubicMetres
				, "Tote MarksAndNumbers", "Tote DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, container.PK, "HC08"
				, "Ref08", "ImportRef08", "ExportRef08", "OuturnComment08", Core.Constants.CargoTypes.Hazardous);

			var undgSubstance1 = CreatUNDGSubstance("9999", "12", "9.9Z", "27.0 c.c", "9,9", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, UNDGSubstanceLookups.StateTypes.Code.Liquid
				, "E0", UNDGSubstanceLookups.PackingGroupTypes.HighDangerCode, "AIR BAG MODULES", "F-I,S-S");
			var undgSubstance2 = CreatUNDGSubstance("9999", "13", "9.8Z", "28.0 c.c", "1,1", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO, UNDGSubstanceLookups.StateTypes.Code.ExplosiveSubstance
				, "E1", UNDGSubstanceLookups.PackingGroupTypes.MediumDangerCode, "SEA BAG MODULES", "F-B,S-A");

			var undgContact1 = Factory.NewWithValidTestData<OrgHeader>();
			var dgContact1 = undgContact1.Contacts.AddNew();
			dgContact1.OC_ContactName = "ABC";
			dgContact1.OC_Phone = "123456";

			var undgContact2 = Factory.NewWithValidTestData<OrgHeader>();
			var dgContact2 = undgContact2.Contacts.AddNew();
			dgContact2.OC_ContactName = "ABC";
			dgContact2.OC_Phone = "78919";

			PopulateDangerousGoods(outerPackLine1.UNDGs.AddNew(), 100, true, undgSubstance1.PK, dgContact1.PK, 10, Constants.Weight.Kilograms, "1", 2M);
			PopulateDangerousGoods(outerPackLine2.UNDGs.AddNew(), 101, true, undgSubstance1.PK, dgContact1.PK, 20, Constants.Weight.Kilograms, "1", 2M);
			PopulateDangerousGoods(outerPackLine2.UNDGs.AddNew(), 101, true, undgSubstance1.PK, dgContact1.PK, 20, Constants.Weight.Kilograms, "2", 2M);
			PopulateDangerousGoods(outerPackLine1.UNDGs.AddNew(), 101, false, undgSubstance1.PK, dgContact2.PK, 30, Constants.Weight.Grams, "1", 3M);
			PopulateDangerousGoods(outerPackLine1.UNDGs.AddNew(), 101, false, undgSubstance1.PK, dgContact2.PK, 30, Constants.Weight.Grams, "1", 3M);
			PopulateDangerousGoods(outerPackLine2.UNDGs.AddNew(), 109, true, undgSubstance2.PK, dgContact1.PK, 120, Constants.Weight.Kilograms, "1", 2M);
			PopulateDangerousGoods(outerPackLine1.UNDGs.AddNew(), 110, true, undgSubstance2.PK, dgContact1.PK, 130, Constants.Weight.Kilograms, "1", 2M);

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipmentDOs = GetShipmentDOs(consol);
				AssertEquals(1, shipmentDOs.Count);

				new PackageGroupingHelper(consol, Context)
					.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByShipment, false, DataContext.BookingRequest, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				var shipmentDO = shipmentDOs.First();
				var dangerousGoodsDO = shipmentDO.PackingLines.First().PackingLines.First().DangerousGoods;

				AssertEquals(4, dangerousGoodsDO.Count);

				AssertDangerousGoodDetails(dangerousGoodsDO.First(x => x.Quantity == 201), 201, 30M, Constants.Weight.Kilograms, "", true
					, "9.9Z", "9,9", "999912", 2M, UNDGSubstanceLookups.PackingGroupTypes.HighDangerCode, "AIR BAG MODULES", "1", ""
					, "ABC", "123456", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, UNDGSubstanceLookups.StateTypes.Code.Liquid, "E0", "F-I", "S-S", "9999", "12");

				AssertDangerousGoodDetails(dangerousGoodsDO.First(x => x.Quantity == 202), 202, 0.06M, Constants.Weight.Kilograms, "", false
					, "9.9Z", "9,9", "999912", 3M, UNDGSubstanceLookups.PackingGroupTypes.HighDangerCode, "AIR BAG MODULES", "1", ""
					, "ABC", "78919", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, UNDGSubstanceLookups.StateTypes.Code.Liquid, "E0", "F-I", "S-S", "9999", "12");

				AssertDangerousGoodDetails(dangerousGoodsDO.First(x => x.Quantity == 219), 219, 250M, Constants.Weight.Kilograms, "", true
					, "9.8Z", "1,1", "999913", 2M, UNDGSubstanceLookups.PackingGroupTypes.MediumDangerCode, "SEA BAG MODULES", "1", ""
					, "ABC", "123456", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO, UNDGSubstanceLookups.StateTypes.Code.ExplosiveSubstance, "E1", "F-B", "S-A", "9999", "13");

				AssertDangerousGoodDetails(dangerousGoodsDO.First(x => x.Quantity == 101), 101, 20M, Constants.Weight.Kilograms, "", true
					, "9.9Z", "9,9", "999912", 2M, UNDGSubstanceLookups.PackingGroupTypes.HighDangerCode, "AIR BAG MODULES", "2", ""
					, "ABC", "123456", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, UNDGSubstanceLookups.StateTypes.Code.Liquid, "E0", "F-I", "S-S", "9999", "12");
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipmentDOs = GetShipmentDOs(consol);
				AssertEquals(1, shipmentDOs.Count);

				new PackageGroupingHelper(consol, Context)
					.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByPackLine, false, DataContext.BookingRequest, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				var shipmentDO = shipmentDOs.First();
				var baleCompressedPackingLineDangerousGoodsDO = shipmentDO.PackingLines.First(x => x.GoodsDescription.StartsWith("BaleCompressed")).PackingLines.First().DangerousGoods;
				var totePackingLineDangerousGoodsDO = shipmentDO.PackingLines.First(x => x.GoodsDescription.StartsWith("Tote")).PackingLines.First().DangerousGoods;

				AssertEquals(3, baleCompressedPackingLineDangerousGoodsDO.Count);
				AssertEquals(3, totePackingLineDangerousGoodsDO.Count);

				AssertDangerousGoodDetails(baleCompressedPackingLineDangerousGoodsDO.First(x => x.Quantity == 100), 100, 10M, Constants.Weight.Kilograms, "", true
					, "9.9Z", "9,9", "999912", 2M, UNDGSubstanceLookups.PackingGroupTypes.HighDangerCode, "AIR BAG MODULES", "1", ""
					, "ABC", "123456", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, UNDGSubstanceLookups.StateTypes.Code.Liquid, "E0", "F-I", "S-S", "9999", "12");

				AssertDangerousGoodDetails(baleCompressedPackingLineDangerousGoodsDO.First(x => x.Quantity == 202), 202, 0.06M, Constants.Weight.Kilograms, "", false
					, "9.9Z", "9,9", "999912", 3M, UNDGSubstanceLookups.PackingGroupTypes.HighDangerCode, "AIR BAG MODULES", "1", ""
					, "ABC", "78919", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, UNDGSubstanceLookups.StateTypes.Code.Liquid, "E0", "F-I", "S-S", "9999", "12");

				AssertDangerousGoodDetails(baleCompressedPackingLineDangerousGoodsDO.First(x => x.Quantity == 110), 110, 130M, Constants.Weight.Kilograms, "", true
					, "9.8Z", "1,1", "999913", 2M, UNDGSubstanceLookups.PackingGroupTypes.MediumDangerCode, "SEA BAG MODULES", "1", ""
					, "ABC", "123456", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO, UNDGSubstanceLookups.StateTypes.Code.ExplosiveSubstance, "E1", "F-B", "S-A", "9999", "13");

				AssertDangerousGoodDetails(totePackingLineDangerousGoodsDO.First(x => x.Quantity == 101 && x.TechnicalName == "1"), 101, 20M, Constants.Weight.Kilograms, "", true
					, "9.9Z", "9,9", "999912", 2M, UNDGSubstanceLookups.PackingGroupTypes.HighDangerCode, "AIR BAG MODULES", "1", ""
					, "ABC", "123456", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, UNDGSubstanceLookups.StateTypes.Code.Liquid, "E0", "F-I", "S-S", "9999", "12");

				AssertDangerousGoodDetails(totePackingLineDangerousGoodsDO.First(x => x.Quantity == 101 && x.TechnicalName == "2"), 101, 20M, Constants.Weight.Kilograms, "", true
					, "9.9Z", "9,9", "999912", 2M, UNDGSubstanceLookups.PackingGroupTypes.HighDangerCode, "AIR BAG MODULES", "2", ""
					, "ABC", "123456", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, UNDGSubstanceLookups.StateTypes.Code.Liquid, "E0", "F-I", "S-S", "9999", "12");

				AssertDangerousGoodDetails(totePackingLineDangerousGoodsDO.First(x => x.Quantity == 109), 109, 120M, Constants.Weight.Kilograms, "", true
					, "9.8Z", "1,1", "999913", 2M, UNDGSubstanceLookups.PackingGroupTypes.MediumDangerCode, "SEA BAG MODULES", "1", ""
					, "ABC", "123456", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO, UNDGSubstanceLookups.StateTypes.Code.ExplosiveSubstance, "E1", "F-B", "S-A", "9999", "13");
			}
		}

		void AssertDangerousGoodDetails(DangerousGood dangerousGood, ZInt quantity, ZDecimal weight, ZString unitOfWeight, ZString packageType, ZBool packedInLimitedQuantity
			, ZString imoClass, ZString subLabel1, ZString code, ZDecimal flashPoint, ZString packingGroup, ZString properShippingName, ZString technicalName, ZString marinePollutant
			, ZString fullName, ZString phone, ZString subLabel2, ZString standard, ZString state, ZString exceptedQuantityCode, ZString emergencyScheduleFire, ZString emergencyScheduleSpillage, ZString unno, ZString variant)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Quantity", quantity, dangerousGood.Quantity);
				AssertEquals("Weight", weight, dangerousGood.Weight.Value);
				AssertEquals("Weight Unit", unitOfWeight, dangerousGood.Weight.Unit.Code);
				AssertEquals("PackageType", packageType, dangerousGood.PackageType.Code);
				AssertEquals("PackedInLimitedQuantity", packedInLimitedQuantity, dangerousGood.PackedInLimitedQuantity);
				AssertEquals("IMOClass", imoClass, dangerousGood.IMOClass);
				AssertEquals("SubLabel1", subLabel1, dangerousGood.SubLabel1);
				AssertEquals("Unno", unno, dangerousGood.Unno);
				AssertEquals("Variant", variant, dangerousGood.Variant);
				AssertEquals("Code", code, dangerousGood.Code);
				AssertEquals("FlashPoint", flashPoint, dangerousGood.FlashPoint.Value);
				AssertEquals("PackingGroup", packingGroup, dangerousGood.PackingGroup);
				AssertEquals("ProperShippingName", properShippingName, dangerousGood.ProperShippingName);
				AssertEquals("TechnicalName", technicalName, dangerousGood.TechnicalName);
				AssertEquals("MarinePollutant", marinePollutant, dangerousGood.MarinePollutant.Code);
				AssertEquals("Contact FullName", fullName, dangerousGood.Contact.FullName);
				AssertEquals("Contact Phone", phone, dangerousGood.Contact.Phone);
				AssertEquals("SubLabel2", subLabel2, dangerousGood.SubLabel2);
				AssertEquals("Standard", standard, dangerousGood.Standard);
				AssertEquals("State", state, dangerousGood.State);
				AssertEquals("ExceptedQuantityCode", exceptedQuantityCode, dangerousGood.ExceptedQuantityCode.Code);
				AssertEquals("EmergencyScheduleFire", emergencyScheduleFire, dangerousGood.EmergencyScheduleFire.Code);
				AssertEquals("EmergencyScheduleSpillage", emergencyScheduleSpillage, dangerousGood.EmergencyScheduleSpillage.Code);
			});
		}

		#endregion

		#region DangerousGoodWeightFallbackAndValidation

		public void TestDangerousGoodWeightFallbackAndValidation_BookingRequest_CarrierDoNotHaveDGWFlag_ShouldHaveWariningMessage() => TestDangerousGoodWeightFallbackAndValidation(DataContext.BookingRequest, dgwForBookingRequiest: false, isError: false);

		public void TestDangerousGoodWeightFallbackAndValidation_BookingRequest_CarrierHaveDGWFlag_ShouldHaveErrorMessage() => TestDangerousGoodWeightFallbackAndValidation(DataContext.BookingRequest, dgwForBookingRequiest: true, isError: true);

		public void TestDangerousGoodWeightFallbackAndValidation_ShippingOrder_CarrierDoNotHaveDGWFlag_ShouldHaveWariningMessage() => TestDangerousGoodWeightFallbackAndValidation(DataContext.ShippingOrder, dgwForShippingOrder: false, isError: false);

		public void TestDangerousGoodWeightFallbackAndValidation_ShippingOrder_CarrierHaveDGWFlag_ShouldHaveErrorMessage() => TestDangerousGoodWeightFallbackAndValidation(DataContext.ShippingOrder, dgwForShippingOrder: true, isError: true);

		public void TestDangerousGoodWeightFallbackAndValidation_EManifest_CarrierDoNotHaveDGWFlag_ShouldHaveWariningMessage() => TestDangerousGoodWeightFallbackAndValidation(DataContext.EManifest, dgwForEManifest: false, isError: false);

		public void TestDangerousGoodWeightFallbackAndValidation_EManifest_CarrierHaveDGWFlag_ShouldHaveErrorMessage() => TestDangerousGoodWeightFallbackAndValidation(DataContext.EManifest, dgwForEManifest: true, isError: true);

		public void TestDangerousGoodWeightFallbackAndValidation_VGM_CarrierDoNotHaveDGWFlag_ShouldHaveWariningMessage() => TestDangerousGoodWeightFallbackAndValidation(DataContext.VerifiedGrossMass, dgwForVGM: false, isError: false);

		public void TestDangerousGoodWeightFallbackAndValidation_VGM_CarrierHaveDGWFlag_ShouldHaveErrorMessage() => TestDangerousGoodWeightFallbackAndValidation(DataContext.VerifiedGrossMass, dgwForVGM: true, isError: true);

		public void TestDangerousGoodWeightFallbackAndValidation_ShippingInstruction_CarrierHaveDGWFlag_ShouldHaveErrorMessage() => TestDangerousGoodWeightFallbackAndValidation(DataContext.ShippingInstruction, dgwForShippingInstruction: true, isError: true);

		public void TestDangerousGoodWeightFallbackAndValidation_ShippingInstruction_CarrierDoNotHaveDGWFlag_NoErrorMessage()
		{
			var message = @"There are DG records with zero (0) weight.
Please verify in Shipment>Packing>Pack Line>Dangerous Goods> Weight & Unit on following Shipments:
JSASM.";

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();

			var messagingRequireemnt = shippingLine.ShippingLineMessagingRequirements.AddNew();
			messagingRequireemnt.RSR_RST_NKType = "DGW";
			messagingRequireemnt.RSR_IsShippingInstruction = true;

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_RSL_ShippingLine = shippingLine.PK;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "CNSHG";
			consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONTAINER1";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "JSASM";
			shipment.JS_GoodsDescription = "ASM JS_GoodsDescription";
			shipment.JS_MarksAndNumbers = "ASM JS_MarksAndNumbers";

			shipment.InnerPackLines.RemoveAndDeleteAll();
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var outerPackLine1 = shipment.OuterPackLines.AddNew();
			PopulatePackLine(outerPackLine1
				, 120, Constants.PkgUnit.BaleCompressed
				, 0, Constants.Weight.Kilograms
				, 120, Constants.Volume.CubicMetres
				, "BaleCompressed MarksAndNumbers", "BaleCompressed DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, container.PK, "HC07"
				, "Ref07", "ImportRef07", "ExportRef07", "OuturnComment07", Core.Constants.CargoTypes.Hazardous);

			var undgSubstance1 = CreatUNDGSubstance("9999", "12", "9.9Z", "27.0 c.c", "9,9", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, UNDGSubstanceLookups.StateTypes.Code.Liquid
				, "E0", UNDGSubstanceLookups.PackingGroupTypes.HighDangerCode, "AIR BAG MODULES", "F-I,S-S");

			var undgContact1 = Factory.NewWithValidTestData<OrgHeader>();
			var dgContact1 = undgContact1.Contacts.AddNew();
			dgContact1.OC_ContactName = "ABC";
			dgContact1.OC_Phone = "123456";

			PopulateDangerousGoods(outerPackLine1.UNDGs.AddNew(), 100, true, undgSubstance1.PK, dgContact1.PK, 0, Constants.Weight.Kilograms, "1", 2M);

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipmentDOs = GetShipmentDOs(consol);
				AssertEquals(1, shipmentDOs.Count);

				new PackageGroupingHelper(consol, Context)
					.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByShipment, false, DataContext.ShippingInstruction, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				var shipmentDO = shipmentDOs.First();
				var dangerousGoodsDO = shipmentDO.PackingLines.First().PackingLines.First().DangerousGoods;

				AssertEquals(1, dangerousGoodsDO.Count);
				AssertEquals(ZDecimal.Zero, dangerousGoodsDO.First().Weight.Value);

				dangerousGoodsDO.First().Weight.ValidateAllIncludingChildren();

				AssertHasExpectedNotificationMessage(dangerousGoodsDO.First().Weight.ValueInfo, message, true);
			}

			messagingRequireemnt.RSR_IsShippingInstruction = false;

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipmentDOs = GetShipmentDOs(consol);
				AssertEquals(1, shipmentDOs.Count);

				new PackageGroupingHelper(consol, Context)
					.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByShipment, false, DataContext.ShippingInstruction, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				var shipmentDO = shipmentDOs.First();
				var dangerousGoodsDO = shipmentDO.PackingLines.First().PackingLines.First().DangerousGoods;

				AssertEquals(1, dangerousGoodsDO.Count);
				AssertEquals(ZDecimal.Zero, dangerousGoodsDO.First().Weight.Value);

				dangerousGoodsDO.First().Weight.ValidateAllIncludingChildren();

				AssertNoMessageError(dangerousGoodsDO.First().Weight.ValueInfo, message);
				AssertNoWarning(dangerousGoodsDO.First().Weight.ValueInfo, message);
			}
		}

		void TestDangerousGoodWeightFallbackAndValidation(ZString documentName, bool dgwForBookingRequiest = false, bool dgwForShippingInstruction = false, bool dgwForShippingOrder = false, bool dgwForVGM = false, bool dgwForEManifest = false, bool isError = false)
		{
			var message = @"There are DG records with zero (0) weight.
Please verify in Shipment>Packing>Pack Line>Dangerous Goods> Weight & Unit on following Shipments:
JSASM.";

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();

			var messagingRequireemnt = shippingLine.ShippingLineMessagingRequirements.AddNew();
			messagingRequireemnt.RSR_RST_NKType = ShippingLineMessagingRequirement.Types.DGNetWeightMandatory;
			messagingRequireemnt.RSR_IsBookingRequest = dgwForBookingRequiest;
			messagingRequireemnt.RSR_IsShippingInstruction = dgwForShippingInstruction;
			messagingRequireemnt.RSR_IsShippingOrder = dgwForShippingOrder;
			messagingRequireemnt.RSR_IsEManifest = dgwForEManifest;
			messagingRequireemnt.RSR_IsVerifiedGrossContainerWeight = dgwForVGM;

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_RSL_ShippingLine = shippingLine.PK;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "CNSHG";
			consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONTAINER1";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "JSASM";
			shipment.JS_GoodsDescription = "ASM JS_GoodsDescription";
			shipment.JS_MarksAndNumbers = "ASM JS_MarksAndNumbers";

			shipment.InnerPackLines.RemoveAndDeleteAll();
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var outerPackLine1 = shipment.OuterPackLines.AddNew();
			PopulatePackLine(outerPackLine1
				, 120, Constants.PkgUnit.BaleCompressed
				, 0, Constants.Weight.Kilograms
				, 120, Constants.Volume.CubicMetres
				, "BaleCompressed MarksAndNumbers", "BaleCompressed DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, container.PK, "HC07"
				, "Ref07", "ImportRef07", "ExportRef07", "OuturnComment07", Core.Constants.CargoTypes.Hazardous);

			var outerPackLine2 = shipment.OuterPackLines.AddNew();
			PopulatePackLine(outerPackLine2
				, 121, Constants.PkgUnit.Tote
				, 0, Constants.Weight.Kilograms
				, 121, Constants.Volume.CubicMetres
				, "Tote MarksAndNumbers", "Tote DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, container.PK, "HC08"
				, "Ref08", "ImportRef08", "ExportRef08", "OuturnComment08", Core.Constants.CargoTypes.Hazardous);

			var undgSubstance1 = CreatUNDGSubstance("9999", "12", "9.9Z", "27.0 c.c", "9,9", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, UNDGSubstanceLookups.StateTypes.Code.Liquid
				, "E0", UNDGSubstanceLookups.PackingGroupTypes.HighDangerCode, "AIR BAG MODULES", "F-I,S-S");
			var undgSubstance2 = CreatUNDGSubstance("9999", "13", "9.8Z", "28.0 c.c", "1,1", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO, UNDGSubstanceLookups.StateTypes.Code.ExplosiveSubstance
				, "E1", UNDGSubstanceLookups.PackingGroupTypes.MediumDangerCode, "SEA BAG MODULES", "F-B,S-A");

			var undgContact1 = Factory.NewWithValidTestData<OrgHeader>();
			var dgContact1 = undgContact1.Contacts.AddNew();
			dgContact1.OC_ContactName = "ABC";
			dgContact1.OC_Phone = "123456";

			var undgContact2 = Factory.NewWithValidTestData<OrgHeader>();
			var dgContact2 = undgContact2.Contacts.AddNew();
			dgContact2.OC_ContactName = "ABC";
			dgContact2.OC_Phone = "78919";

			PopulateDangerousGoods(outerPackLine1.UNDGs.AddNew(), 100, true, undgSubstance1.PK, dgContact1.PK, 0, Constants.Weight.Kilograms, "1", 2M);
			PopulateDangerousGoods(outerPackLine2.UNDGs.AddNew(), 101, true, undgSubstance1.PK, dgContact1.PK, 0, Constants.Weight.Kilograms, "1", 2M);

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipmentDOs = GetShipmentDOs(consol);
				AssertEquals(1, shipmentDOs.Count);

				new PackageGroupingHelper(consol, Context)
					.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByShipment, false, documentName, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				var shipmentDO = shipmentDOs.First();
				var dangerousGoodsDO = shipmentDO.PackingLines.First().PackingLines.First().DangerousGoods;

				AssertEquals(1, dangerousGoodsDO.Count);
				AssertEquals(ZDecimal.Zero, dangerousGoodsDO.First().Weight.Value);

				dangerousGoodsDO.First().Weight.ValidateAllIncludingChildren();

				AssertHasExpectedNotificationMessage(dangerousGoodsDO.First().Weight.ValueInfo, message, isError);
			}

			outerPackLine1.JL_ActualWeight = 100;
			outerPackLine2.JL_ActualWeight = 200;

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipmentDOs = GetShipmentDOs(consol);
				AssertEquals(1, shipmentDOs.Count);

				new PackageGroupingHelper(consol, Context)
					.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByShipment, false, documentName, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				var shipmentDO = shipmentDOs.First();
				var dangerousGoodsDO = shipmentDO.PackingLines.First().PackingLines.First().DangerousGoods;

				AssertEquals(1, dangerousGoodsDO.Count);
				AssertEquals(300M, dangerousGoodsDO.First().Weight.Value);

				dangerousGoodsDO.First().Weight.ValidateAllIncludingChildren();

				AssertNoExpectedNotificationMessage(dangerousGoodsDO.First().Weight.ValueInfo, message, isError);
			}

			var dg1 = PopulateDangerousGoods(outerPackLine2.UNDGs.AddNew(), 101, true, undgSubstance1.PK, dgContact1.PK, 1, Constants.Weight.Kilograms, "1", 2M);

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipmentDOs = GetShipmentDOs(consol);
				AssertEquals(1, shipmentDOs.Count);

				new PackageGroupingHelper(consol, Context)
					.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByShipment, false, documentName, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				var shipmentDO = shipmentDOs.First();
				var dangerousGoodsDO = shipmentDO.PackingLines.First().PackingLines.First().DangerousGoods;

				AssertEquals(1, dangerousGoodsDO.Count);
				AssertEquals(1M, dangerousGoodsDO.First().Weight.Value);

				dangerousGoodsDO.First().Weight.ValidateAllIncludingChildren();

				AssertHasExpectedNotificationMessage(dangerousGoodsDO.First().Weight.ValueInfo, message, isError);
			}

			var dg2 = PopulateDangerousGoods(outerPackLine2.UNDGs.AddNew(), 101, true, undgSubstance1.PK, dgContact1.PK, 0, Constants.Weight.Kilograms, "2", 2M);

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipmentDOs = GetShipmentDOs(consol);
				AssertEquals(1, shipmentDOs.Count);

				new PackageGroupingHelper(consol, Context)
					.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByShipment, false, documentName, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				var shipmentDO = shipmentDOs.First();
				var dangerousGoodsDO = shipmentDO.PackingLines.First().PackingLines.First().DangerousGoods;

				AssertEquals(2, dangerousGoodsDO.Count);

				var consolidatedDangerousGood1 = dangerousGoodsDO.First(x => x.TechnicalName == "1");
				var consolidatedDangerousGood2 = dangerousGoodsDO.First(x => x.TechnicalName == "2");

				consolidatedDangerousGood1.Weight.ValidateAllIncludingChildren();
				consolidatedDangerousGood2.Weight.ValidateAllIncludingChildren();

				AssertEquals(1M, consolidatedDangerousGood1.Weight.Value);
				AssertEquals(0M, consolidatedDangerousGood2.Weight.Value);

				AssertHasExpectedNotificationMessage(consolidatedDangerousGood1.Weight.ValueInfo, message, isError);
				AssertHasExpectedNotificationMessage(consolidatedDangerousGood2.Weight.ValueInfo, message, isError);
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipmentDOs = GetShipmentDOs(consol);
				AssertEquals(1, shipmentDOs.Count);

				new PackageGroupingHelper(consol, Context)
					.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByPackLine, false, documentName, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				var shipmentDO = shipmentDOs.First();
				var baleCompressedPackingLineDangerousGoodsDO = shipmentDO.PackingLines.First(x => x.GoodsDescription.StartsWith("BaleCompressed")).PackingLines.First().DangerousGoods;
				var totePackingLineDangerousGoodsDO = shipmentDO.PackingLines.First(x => x.GoodsDescription.StartsWith("Tote")).PackingLines.First().DangerousGoods;

				AssertEquals(1, baleCompressedPackingLineDangerousGoodsDO.Count);
				AssertEquals(2, totePackingLineDangerousGoodsDO.Count);

				var consolidatedDangerousGood1 = baleCompressedPackingLineDangerousGoodsDO.First();
				var consolidatedDangerousGood2 = totePackingLineDangerousGoodsDO.First(x => x.TechnicalName == "1");
				var consolidatedDangerousGood3 = totePackingLineDangerousGoodsDO.First(x => x.TechnicalName == "2");

				consolidatedDangerousGood1.Weight.ValidateAllIncludingChildren();
				consolidatedDangerousGood2.Weight.ValidateAllIncludingChildren();
				consolidatedDangerousGood3.Weight.ValidateAllIncludingChildren();

				AssertEquals(100M, consolidatedDangerousGood1.Weight.Value);
				AssertEquals(1M, consolidatedDangerousGood2.Weight.Value);
				AssertEquals(0M, consolidatedDangerousGood3.Weight.Value);

				AssertNoExpectedNotificationMessage(consolidatedDangerousGood1.Weight.ValueInfo, message, isError);
				AssertHasExpectedNotificationMessage(consolidatedDangerousGood2.Weight.ValueInfo, message, isError);
				AssertHasExpectedNotificationMessage(consolidatedDangerousGood3.Weight.ValueInfo, message, isError);
			}

			dg1.DI_DGWeight = ZDecimal.Zero;
			dg1.DI_IsLimitedQuantity = true;

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipmentDOs = GetShipmentDOs(consol);
				AssertEquals(1, shipmentDOs.Count);

				new PackageGroupingHelper(consol, Context)
					.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByShipment, false, documentName, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				var shipmentDO = shipmentDOs.First();
				var dangerousGoodsDO = shipmentDO.PackingLines.First().PackingLines.First().DangerousGoods;

				AssertEquals(2, dangerousGoodsDO.Count);

				var consolidatedDangerousGood1 = dangerousGoodsDO.First(x => x.TechnicalName == "1");
				var consolidatedDangerousGood2 = dangerousGoodsDO.First(x => x.TechnicalName == "2");

				consolidatedDangerousGood1.Weight.ValidateAllIncludingChildren();
				consolidatedDangerousGood2.Weight.ValidateAllIncludingChildren();

				AssertEquals(0M, consolidatedDangerousGood1.Weight.Value);
				AssertEquals(0M, consolidatedDangerousGood2.Weight.Value);

				AssertHasExpectedNotificationMessage(consolidatedDangerousGood1.Weight.ValueInfo, message, isError);
				AssertHasExpectedNotificationMessage(consolidatedDangerousGood2.Weight.ValueInfo, message, isError);
			}
		}

		void AssertHasExpectedNotificationMessage(ZPropertyInfo info, string notificationMessage, bool isError)
		{
			if (isError)
			{
				AssertHasMessageError(info, notificationMessage);
			}
			else
			{
				AssertHasWarning(info, notificationMessage);
			}
		}

		void AssertNoExpectedNotificationMessage(ZPropertyInfo info, string notificationMessage, bool isError)
		{
			if (isError)
			{
				AssertNoMessageError(info, notificationMessage);
			}
			else
			{
				AssertNoWarning(info, notificationMessage);
			}
		}

		public void TestDangerousGoodWeightFallbackAndValidation_ShipmentList()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();

			var messagingRequireemnt = shippingLine.ShippingLineMessagingRequirements.AddNew();
			messagingRequireemnt.RSR_RST_NKType = ShippingLineMessagingRequirement.Types.DGNetWeightMandatory;
			messagingRequireemnt.RSR_IsBookingRequest = true;

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_RSL_ShippingLine = shippingLine.PK;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "CNSHG";
			consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONTAINER1";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "JSASM";
			shipment.JS_GoodsDescription = "ASM JS_GoodsDescription";
			shipment.JS_MarksAndNumbers = "ASM JS_MarksAndNumbers";

			shipment.InnerPackLines.RemoveAndDeleteAll();
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var shipmentSubShipment1 = shipment.CoLoadShipments.AddNew();
			shipmentSubShipment1.JS_F3_NKPackType = Core.Constants.PkgUnit.BreakBulk;
			shipmentSubShipment1.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.BaleCompressed;
			shipmentSubShipment1.JS_UniqueConsignRef = "Sub01";
			shipmentSubShipment1.JS_GoodsDescription = "Sub1 JS_GoodsDescription";
			shipmentSubShipment1.JS_MarksAndNumbers = "Sub1 JS_MarksAndNumbers";

			shipmentSubShipment1.InnerPackLines.RemoveAndDeleteAll();
			shipmentSubShipment1.OuterPackLines.RemoveAndDeleteAll();
			shipmentSubShipment1.CusEntryNumbers.RemoveAndDeleteAll();

			var shipmentSubShipment2 = shipment.CoLoadShipments.AddNew();
			shipmentSubShipment2.JS_F3_NKPackType = Core.Constants.PkgUnit.BreakBulk;
			shipmentSubShipment2.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.BaleCompressed;
			shipmentSubShipment2.JS_UniqueConsignRef = "Sub02";
			shipmentSubShipment2.JS_GoodsDescription = "Sub2 JS_GoodsDescription";
			shipmentSubShipment2.JS_MarksAndNumbers = "Sub2 JS_MarksAndNumbers";

			shipmentSubShipment2.InnerPackLines.RemoveAndDeleteAll();
			shipmentSubShipment2.OuterPackLines.RemoveAndDeleteAll();
			shipmentSubShipment2.CusEntryNumbers.RemoveAndDeleteAll();

			var outerPackLine1 = shipmentSubShipment1.OuterPackLines.AddNew();
			PopulatePackLine(outerPackLine1
				, 120, Constants.PkgUnit.BaleCompressed
				, 0, Constants.Weight.Kilograms
				, 120, Constants.Volume.CubicMetres
				, "BaleCompressed MarksAndNumbers", "BaleCompressed DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, container.PK, "HC07"
				, "Ref07", "ImportRef07", "ExportRef07", "OuturnComment07", Core.Constants.CargoTypes.Hazardous);

			var outerPackLine2 = shipmentSubShipment2.OuterPackLines.AddNew();
			PopulatePackLine(outerPackLine2
				, 121, Constants.PkgUnit.Tote
				, 0, Constants.Weight.Kilograms
				, 121, Constants.Volume.CubicMetres
				, "Tote MarksAndNumbers", "Tote DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, container.PK, "HC08"
				, "Ref08", "ImportRef08", "ExportRef08", "OuturnComment08", Core.Constants.CargoTypes.Hazardous);

			var undgSubstance1 = CreatUNDGSubstance("9999", "12", "9.9Z", "27.0 c.c", "9,9", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, UNDGSubstanceLookups.StateTypes.Code.Liquid
				, "E0", UNDGSubstanceLookups.PackingGroupTypes.HighDangerCode, "AIR BAG MODULES", "F-I,S-S");

			var undgContact1 = Factory.NewWithValidTestData<OrgHeader>();
			var dgContact1 = undgContact1.Contacts.AddNew();
			dgContact1.OC_ContactName = "ABC";
			dgContact1.OC_Phone = "123456";

			var undg1 = PopulateDangerousGoods(outerPackLine1.UNDGs.AddNew(), 100, false, undgSubstance1.PK, dgContact1.PK, 0, Constants.Weight.Kilograms, "1", 2M);
			PopulateDangerousGoods(outerPackLine1.UNDGs.AddNew(), 100, true, undgSubstance1.PK, dgContact1.PK, 0, Constants.Weight.Kilograms, "2", 2M);
			var undg2 = PopulateDangerousGoods(outerPackLine2.UNDGs.AddNew(), 101, false, undgSubstance1.PK, dgContact1.PK, 0, Constants.Weight.Kilograms, "1", 2M);

			var errorMessage_Sub01 = @"There are DG records with zero (0) weight.
Please verify in Shipment>Packing>Pack Line>Dangerous Goods> Weight & Unit on following Shipments:
Sub01.";

			var errorMessage_Sub0102 = @"There are DG records with zero (0) weight.
Please verify in Shipment>Packing>Pack Line>Dangerous Goods> Weight & Unit on following Shipments:
Sub01
Sub02.";
			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipmentDOs = GetShipmentDOs(consol);
				AssertEquals(1, shipmentDOs.Count);

				new PackageGroupingHelper(consol, Context)
					.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByShipment, false, DataContext.BookingRequest, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				var shipmentDO = shipmentDOs.First();
				var dangerousGoodsDO = shipmentDO.PackingLines.First().PackingLines.First().DangerousGoods;

				AssertEquals(2, dangerousGoodsDO.Count);
				var consolidatedDangerousGood1 = dangerousGoodsDO.First(x => x.TechnicalName == "1");

				consolidatedDangerousGood1.Weight.ValidateAllIncludingChildren();

				AssertHasMessageError(consolidatedDangerousGood1.Weight.ValueInfo, errorMessage_Sub0102);
			}

			undg2.DI_DGWeight = 1;

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipmentDOs = GetShipmentDOs(consol);
				AssertEquals(1, shipmentDOs.Count);

				new PackageGroupingHelper(consol, Context)
					.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByShipment, false, DataContext.BookingRequest, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				var shipmentDO = shipmentDOs.First();
				var dangerousGoodsDO = shipmentDO.PackingLines.First().PackingLines.First().DangerousGoods;

				AssertEquals(2, dangerousGoodsDO.Count);
				var consolidatedDangerousGood1 = dangerousGoodsDO.First(x => x.TechnicalName == "1");

				consolidatedDangerousGood1.Weight.ValidateAllIncludingChildren();

				AssertHasMessageError(consolidatedDangerousGood1.Weight.ValueInfo, errorMessage_Sub01);
			}

			undg1.DI_DGWeight = 1;
			undg2.DI_DGWeight = 1;

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipmentDOs = GetShipmentDOs(consol);
				AssertEquals(1, shipmentDOs.Count);

				new PackageGroupingHelper(consol, Context)
					.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByShipment, false, DataContext.BookingRequest, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				var shipmentDO = shipmentDOs.First();
				var dangerousGoodsDO = shipmentDO.PackingLines.First().PackingLines.First().DangerousGoods;

				AssertEquals(2, dangerousGoodsDO.Count);
				var consolidatedDangerousGood1 = dangerousGoodsDO.First(x => x.TechnicalName == "1");

				consolidatedDangerousGood1.Weight.ValidateAllIncludingChildren();

				AssertNoMessageError(consolidatedDangerousGood1.Weight.ValueInfo, errorMessage_Sub01);
				AssertNoMessageError(consolidatedDangerousGood1.Weight.ValueInfo, errorMessage_Sub0102);
			}
		}

		#endregion

		#region WeightAndVolume

		public void TestWeightAndVolume()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONTAINER1";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONTAINER2";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment1.JS_UniqueConsignRef = "JS1";
			shipment1.JS_GoodsDescription = "JS_GoodsDescription 1";
			shipment1.JS_MarksAndNumbers = "JS_MarksAndNumbers 2";
			shipment1.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment1.JS_UnitOfVolume = Constants.Volume.CubicMetres;

			shipment1.OuterPackLines.RemoveAndDeleteAll();

			var shipment1OuterPackLine1 = PopulatePackLine(shipment1.OuterPackLines.AddNew()
				, 100, Constants.PkgUnit.BaleCompressed
				, 100, Constants.Weight.Kilograms
				, 100, Constants.Volume.CubicMetres
				, "BaleCompressed1 MarksAndNumbers", "BaleCompressed1 DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, container1.PK, "HC01"
				, "Ref01", "ImportRef01", "ExportRef01", "OuturnComment01", Core.Constants.CargoTypes.Hazardous);

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment2.JS_UniqueConsignRef = "JS2";
			shipment2.JS_GoodsDescription = "JS_GoodsDescription 2";
			shipment2.JS_MarksAndNumbers = "JS_MarksAndNumbers 2";
			shipment2.JS_UnitOfWeight = Constants.Weight.Milligrams;
			shipment2.JS_UnitOfVolume = Constants.Volume.MegaLitre;

			shipment2.OuterPackLines.RemoveAndDeleteAll();

			var shipment2OuterPackLine2 = PopulatePackLine(shipment2.OuterPackLines.AddNew()
				, 110, Constants.PkgUnit.Tube
				, 11000M, Constants.Weight.Milligrams
				, 0.002M, Constants.Volume.MegaLitre
				, "Tube MarksAndNumbers", "Tube DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, container2.PK, "HC04"
				, "Ref04", "ImportRef04", "ExportRef04", "OuturnComment04", Core.Constants.CargoTypes.Hazardous);

			var undgSubstance = CreatUNDGSubstance("9999", "12", "9.9Z", "27.0 c.c", "9,9", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, UNDGSubstanceLookups.StateTypes.Code.Liquid
				, "E0", UNDGSubstanceLookups.PackingGroupTypes.HighDangerCode, "AIR BAG MODULES", "F-I,S-S");

			var undgContact = Factory.NewWithValidTestData<OrgHeader>();
			var dgContact1 = undgContact.Contacts.AddNew();
			dgContact1.OC_ContactName = "ABC";
			dgContact1.OC_Phone = "123456";

			var dangerousGoods = PopulateDangerousGoods(shipment2OuterPackLine2.UNDGs.AddNew(), 100, true, undgSubstance.PK, dgContact1.PK, 1000, Constants.Weight.Milligrams, "1", 2M);
			dangerousGoods.DI_DGVolume = 0.001M;
			dangerousGoods.DI_UnitOfVolume = Constants.Volume.MegaLitre;
			dangerousGoods.DI_IsLimitedQuantity = true;
			PopulateDangerousGoods(shipment2OuterPackLine2.UNDGs.AddNew(), 100, true, undgSubstance.PK, dgContact1.PK, 1000, Constants.Weight.Milligrams, "1", 2M);

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipmentDOs = GetShipmentDOs(consol);

				AssertEquals(2, shipmentDOs.Count);

				new PackageGroupingHelper(consol, Context)
					.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByPackLine, true, DataContext.BookingRequest, Constants.Weight.Grams, Constants.Volume.Litre);

				var shipment1DO = shipmentDOs.First(x => x.ShipmentID == "JS1");
				var shipment2DO = shipmentDOs.First(x => x.ShipmentID == "JS2");

				AssertEquals(1, shipment1DO.AllPackingLinesIncludeCoLoad.Count());
				AssertEquals(1, shipment2DO.AllPackingLinesIncludeCoLoad.Count());

				AssertEquals(100000M, shipment1DO.AllPackingLinesIncludeCoLoad.First().Weight.Value);
				AssertEquals(Constants.Weight.Grams, shipment1DO.AllPackingLinesIncludeCoLoad.First().Weight.Unit.Code);
				AssertEquals(100000M, shipment1DO.AllPackingLinesIncludeCoLoad.First().Volume.Value);
				AssertEquals(Constants.Volume.Litre, shipment1DO.AllPackingLinesIncludeCoLoad.First().Volume.Unit.Code);

				AssertEquals(11M, shipment2DO.AllPackingLinesIncludeCoLoad.First().Weight.Value);
				AssertEquals(Constants.Weight.Grams, shipment2DO.AllPackingLinesIncludeCoLoad.First().Weight.Unit.Code);
				AssertEquals(2000M, shipment2DO.AllPackingLinesIncludeCoLoad.First().Volume.Value);
				AssertEquals(Constants.Volume.Litre, shipment2DO.AllPackingLinesIncludeCoLoad.First().Volume.Unit.Code);

				AssertEquals(1, shipment2DO.AllPackingLinesIncludeCoLoad.First().DangerousGoods.Count);

				var dangerousGoodsDO = shipment2DO.AllPackingLinesIncludeCoLoad.First().DangerousGoods.First();

				AssertEquals(2M, dangerousGoodsDO.Weight.Value);
				AssertEquals(Constants.Weight.Grams, dangerousGoodsDO.Weight.Unit.Code);
				AssertEquals(1000M, dangerousGoodsDO.Volume.Value);
				AssertEquals(Constants.Volume.Litre, dangerousGoodsDO.Volume.Unit.Code);
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipmentDOs = GetShipmentDOs(consol);

				AssertEquals(2, shipmentDOs.Count);

				new PackageGroupingHelper(consol, Context)
					.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByPackLine, true, DataContext.BookingRequest, Constants.Weight.Tonnes, Constants.Volume.CubicInches);

				var shipment1DO = shipmentDOs.First(x => x.ShipmentID == "JS1");
				var shipment2DO = shipmentDOs.First(x => x.ShipmentID == "JS2");

				AssertEquals(1, shipment1DO.AllPackingLinesIncludeCoLoad.Count());
				AssertEquals(1, shipment2DO.AllPackingLinesIncludeCoLoad.Count());

				AssertEquals(0.1M, shipment1DO.AllPackingLinesIncludeCoLoad.First().Weight.Value);
				AssertEquals(Constants.Weight.Tonnes, shipment1DO.AllPackingLinesIncludeCoLoad.First().Weight.Unit.Code);

				AssertEquals(6102374.409473M, shipment1DO.AllPackingLinesIncludeCoLoad.First().Volume.Value);
				AssertEquals(Constants.Volume.CubicInches, shipment1DO.AllPackingLinesIncludeCoLoad.First().Volume.Unit.Code);

				AssertEquals(0.000011M, shipment2DO.AllPackingLinesIncludeCoLoad.First().Weight.Value);
				AssertEquals(Constants.Weight.Tonnes, shipment2DO.AllPackingLinesIncludeCoLoad.First().Weight.Unit.Code);

				AssertEquals(122047.488189M, shipment2DO.AllPackingLinesIncludeCoLoad.First().Volume.Value);
				AssertEquals(Constants.Volume.CubicInches, shipment2DO.AllPackingLinesIncludeCoLoad.First().Volume.Unit.Code);

				AssertEquals(1, shipment2DO.AllPackingLinesIncludeCoLoad.First().DangerousGoods.Count);

				var dangerousGoodsDO = shipment2DO.AllPackingLinesIncludeCoLoad.First().DangerousGoods.First();

				AssertEquals(0.000002M, dangerousGoodsDO.Weight.Value);
				AssertEquals(Constants.Weight.Tonnes, dangerousGoodsDO.Weight.Unit.Code);
				AssertEquals(61023.744095M, dangerousGoodsDO.Volume.Value);
				AssertEquals(Constants.Volume.CubicInches, dangerousGoodsDO.Volume.Unit.Code);
			}
		}

		#endregion

		#region NoInnerPackLineValidation

		public void TestNoInnerPackLineValidation()
		{
			var warningMessage = @"There are pack lines with no inner quantity, outer pack quantity has been taken as inners.
Please verify in Shipment>Packing>Packs on following Shipments:
JSASM.";
			var standardServiceLevel = Factory.LoadFromUniqueKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, new ZString("STD"));
			var rule = CreateRefCountryRules(Constants.CountryCodes.UnitedStates, Constants.CountryCodes.China, Constants.TransportModes.Sea, standardServiceLevel.PK, false);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "CNSHG";
			consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONTAINER1";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONTAINER2";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "JSASM";
			shipment.JS_GoodsDescription = "ASM JS_GoodsDescription";
			shipment.JS_MarksAndNumbers = "ASM JS_MarksAndNumbers";

			shipment.InnerPackLines.RemoveAndDeleteAll();
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var outerPackLine1 = shipment.OuterPackLines.AddNew();
			PopulatePackLine(outerPackLine1
				, 120, Constants.PkgUnit.BaleCompressed
				, 120, Constants.Weight.Kilograms
				, 120, Constants.Volume.CubicMetres
				, "BaleCompressed MarksAndNumbers", "BaleCompressed DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, container1.PK, "HC07"
				, "Ref07", "ImportRef07", "ExportRef07", "OuturnComment07", Core.Constants.CargoTypes.Hazardous);

			var innerPackLine1 = shipment.InnerPackLines.AddNew();
			innerPackLine1.JL_PackageCount = 11;
			innerPackLine1.JL_F3_NKPackType = Core.Constants.PkgUnit.Unit;
			innerPackLine1.JL_JL_OuterPackLine = outerPackLine1.PK;

			var outerPackLine2 = shipment.OuterPackLines.AddNew();
			PopulatePackLine(outerPackLine2
				, 120, Constants.PkgUnit.BaleCompressed
				, 120, Constants.Weight.Kilograms
				, 120, Constants.Volume.CubicMetres
				, "BaleCompressed MarksAndNumbers", "BaleCompressed DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, container1.PK, "HC07"
				, "Ref07", "ImportRef07", "ExportRef07", "OuturnComment07", Core.Constants.CargoTypes.Hazardous);

			var innerPackLine2 = shipment.InnerPackLines.AddNew();
			innerPackLine2.JL_PackageCount = 11;
			innerPackLine2.JL_F3_NKPackType = Core.Constants.PkgUnit.Unit;
			innerPackLine2.JL_JL_OuterPackLine = outerPackLine2.PK;

			var outerPackLine3 = shipment.OuterPackLines.AddNew();
			PopulatePackLine(outerPackLine2
				, 121, Constants.PkgUnit.Tote
				, 121, Constants.Weight.Kilograms
				, 121, Constants.Volume.CubicMetres
				, "Tote MarksAndNumbers", "Tote DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, container2.PK, "HC08"
				, "Ref08", "ImportRef08", "ExportRef08", "OuturnComment08", Core.Constants.CargoTypes.Hazardous);

			var innerPackLine3 = shipment.InnerPackLines.AddNew();
			innerPackLine3.JL_PackageCount = 11;
			innerPackLine3.JL_F3_NKPackType = Core.Constants.PkgUnit.Unit;
			innerPackLine3.JL_JL_OuterPackLine = outerPackLine3.PK;

			PopulatePackLine(shipment.OuterPackLines.AddNew()
				, 121, Constants.PkgUnit.Tote
				, 121, Constants.Weight.Kilograms
				, 121, Constants.Volume.CubicMetres
				, "Tote MarksAndNumbers", "Tote DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, container1.PK, "HC08"
				, "Ref08", "ImportRef08", "ExportRef08", "OuturnComment08", Core.Constants.CargoTypes.Hazardous);

			PopulatePackLine(shipment.OuterPackLines.AddNew()
				, 121, Constants.PkgUnit.Tote
				, 121, Constants.Weight.Kilograms
				, 121, Constants.Volume.CubicMetres
				, "Tote MarksAndNumbers", "Tote DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, container1.PK, "HC08"
				, "Ref08", "ImportRef08", "ExportRef08", "OuturnComment08", Core.Constants.CargoTypes.Hazardous);

			PopulatePackLine(shipment.OuterPackLines.AddNew()
				, 121, Constants.PkgUnit.Tube
				, 121, Constants.Weight.Kilograms
				, 121, Constants.Volume.CubicMetres
				, "Tube MarksAndNumbers", "Tube DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, container2.PK, "HC08"
				, "Ref08", "ImportRef08", "ExportRef08", "OuturnComment08", Core.Constants.CargoTypes.Hazardous);

			PopulatePackLine(shipment.OuterPackLines.AddNew()
				, 121, Constants.PkgUnit.Tube
				, 121, Constants.Weight.Kilograms
				, 121, Constants.Volume.CubicMetres
				, "Tube MarksAndNumbers", "Tube DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, container2.PK, "HC08"
				, "Ref08", "ImportRef08", "ExportRef08", "OuturnComment08", Core.Constants.CargoTypes.Hazardous);

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipmentDOs = GetShipmentDOs(consol);
				AssertEquals(1, shipmentDOs.Count);

				new PackageGroupingHelper(consol, Context)
					.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByPackLine, false, DataContext.BookingRequest, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				var shipmentDO = shipmentDOs.First();
				shipmentDO.ValidateAllIncludingChildren();

				var baleCompressedGroupedPackingLineDO = shipmentDO.PackingLines.First(x => x.GoodsDescription.StartsWith("BaleCompressed"));
				var baleCompressedConsolidatedPackingLineDO = baleCompressedGroupedPackingLineDO.PackingLines.First();

				var tubeGroupedPackingLineDO = shipmentDO.PackingLines.First(x => x.GoodsDescription.StartsWith("Tube"));
				var tubeConsolidatedPackingLineDO = tubeGroupedPackingLineDO.PackingLines.First();

				var toteGroupedPackingLineDO = shipmentDO.PackingLines.First(x => x.GoodsDescription.StartsWith("Tote"));
				var toteConsolidatedPackingLineDO1 = toteGroupedPackingLineDO.PackingLines.First(x => x.ContainerNumber == "CONTAINER1");
				var toteConsolidatedPackingLineDO2 = toteGroupedPackingLineDO.PackingLines.First(x => x.ContainerNumber == "CONTAINER2");

				baleCompressedGroupedPackingLineDO.ValidateAll();
				baleCompressedConsolidatedPackingLineDO.ValidateAll();
				tubeGroupedPackingLineDO.ValidateAll();
				tubeConsolidatedPackingLineDO.ValidateAll();
				toteGroupedPackingLineDO.ValidateAll();
				toteConsolidatedPackingLineDO1.ValidateAll();
				toteConsolidatedPackingLineDO2.ValidateAll();

				AssertNoWarning(baleCompressedGroupedPackingLineDO.QuantityInfo, warningMessage);
				AssertNoWarning(baleCompressedConsolidatedPackingLineDO.QuantityInfo, warningMessage);

				AssertNoWarning(tubeGroupedPackingLineDO.QuantityInfo, warningMessage);
				AssertNoWarning(tubeConsolidatedPackingLineDO.QuantityInfo, warningMessage);

				AssertNoWarning(toteGroupedPackingLineDO.QuantityInfo, warningMessage);
				AssertNoWarning(toteConsolidatedPackingLineDO1.QuantityInfo, warningMessage);
				AssertNoWarning(toteConsolidatedPackingLineDO1.QuantityInfo, warningMessage);
			}

			rule.R7_ShowInner = true;

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipmentDOs = GetShipmentDOs(consol);
				AssertEquals(1, shipmentDOs.Count);

				new PackageGroupingHelper(consol, Context)
					.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByPackLine, false, DataContext.BookingRequest, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				var shipmentDO = shipmentDOs.First();

				var baleCompressedGroupedPackingLineDO = shipmentDO.PackingLines.First(x => x.GoodsDescription.StartsWith("BaleCompressed"));
				var baleCompressedConsolidatedPackingLineDO = baleCompressedGroupedPackingLineDO.PackingLines.First();

				var tubeGroupedPackingLineDO = shipmentDO.PackingLines.First(x => x.GoodsDescription.StartsWith("Tube"));
				var tubeConsolidatedPackingLineDO = tubeGroupedPackingLineDO.PackingLines.First();

				var toteGroupedPackingLineDO = shipmentDO.PackingLines.First(x => x.GoodsDescription.StartsWith("Tote"));
				var toteConsolidatedPackingLineDO1 = toteGroupedPackingLineDO.PackingLines.First(x => x.ContainerNumber == "CONTAINER1");
				var toteConsolidatedPackingLineDO2 = toteGroupedPackingLineDO.PackingLines.First(x => x.ContainerNumber == "CONTAINER2");

				baleCompressedGroupedPackingLineDO.ValidateAll();
				baleCompressedConsolidatedPackingLineDO.ValidateAll();
				tubeGroupedPackingLineDO.ValidateAll();
				tubeConsolidatedPackingLineDO.ValidateAll();
				toteGroupedPackingLineDO.ValidateAll();
				toteConsolidatedPackingLineDO1.ValidateAll();
				toteConsolidatedPackingLineDO2.ValidateAll();

				AssertNoWarning(baleCompressedGroupedPackingLineDO.QuantityInfo, warningMessage);
				AssertNoWarning(baleCompressedConsolidatedPackingLineDO.QuantityInfo, warningMessage);

				AssertHasWarning(tubeGroupedPackingLineDO.QuantityInfo, warningMessage);
				AssertNoWarning(tubeConsolidatedPackingLineDO.QuantityInfo, warningMessage);

				AssertHasWarning(toteGroupedPackingLineDO.QuantityInfo, warningMessage);
				AssertNoWarning(toteConsolidatedPackingLineDO1.QuantityInfo, warningMessage);
				AssertNoWarning(toteConsolidatedPackingLineDO2.QuantityInfo, warningMessage);
			}
		}

		public void TestNoInnerPackLineValidation_ShipmentList()
		{
			var standardServiceLevel = Factory.LoadFromUniqueKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, new ZString("STD"));
			CreateRefCountryRules(Constants.CountryCodes.UnitedStates, Constants.CountryCodes.China, Constants.TransportModes.Sea, standardServiceLevel.PK, true);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "CNSHG";
			consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONTAINER1";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONTAINER2";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "JSASM";
			shipment.JS_GoodsDescription = "ASM JS_GoodsDescription";
			shipment.JS_MarksAndNumbers = "ASM JS_MarksAndNumbers";

			shipment.InnerPackLines.RemoveAndDeleteAll();
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var shipmentSubShipment1 = shipment.CoLoadShipments.AddNew();
			shipmentSubShipment1.JS_F3_NKPackType = Core.Constants.PkgUnit.BreakBulk;
			shipmentSubShipment1.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.BaleCompressed;
			shipmentSubShipment1.JS_UniqueConsignRef = "Sub01";
			shipmentSubShipment1.JS_GoodsDescription = "Sub1 JS_GoodsDescription";
			shipmentSubShipment1.JS_MarksAndNumbers = "Sub1 JS_MarksAndNumbers";

			shipmentSubShipment1.InnerPackLines.RemoveAndDeleteAll();
			shipmentSubShipment1.OuterPackLines.RemoveAndDeleteAll();
			shipmentSubShipment1.CusEntryNumbers.RemoveAndDeleteAll();

			var shipmentSubShipment2 = shipment.CoLoadShipments.AddNew();
			shipmentSubShipment2.JS_F3_NKPackType = Core.Constants.PkgUnit.BreakBulk;
			shipmentSubShipment2.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.BaleCompressed;
			shipmentSubShipment2.JS_UniqueConsignRef = "Sub02";
			shipmentSubShipment2.JS_GoodsDescription = "Sub2 JS_GoodsDescription";
			shipmentSubShipment2.JS_MarksAndNumbers = "Sub2 JS_MarksAndNumbers";

			shipmentSubShipment2.InnerPackLines.RemoveAndDeleteAll();
			shipmentSubShipment2.OuterPackLines.RemoveAndDeleteAll();
			shipmentSubShipment2.CusEntryNumbers.RemoveAndDeleteAll();

			var outerPackLine1 = shipmentSubShipment1.OuterPackLines.AddNew();
			PopulatePackLine(outerPackLine1
				, 120, Constants.PkgUnit.BaleCompressed
				, 120, Constants.Weight.Kilograms
				, 120, Constants.Volume.CubicMetres
				, "BaleCompressed MarksAndNumbers", "BaleCompressed DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, container1.PK, "HC07"
				, "Ref07", "ImportRef07", "ExportRef07", "OuturnComment07", Core.Constants.CargoTypes.Hazardous);

			var outerPackLine2 = shipmentSubShipment2.OuterPackLines.AddNew();
			PopulatePackLine(outerPackLine1
				, 120, Constants.PkgUnit.BaleCompressed
				, 120, Constants.Weight.Kilograms
				, 120, Constants.Volume.CubicMetres
				, "BaleCompressed MarksAndNumbers", "BaleCompressed DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, container1.PK, "HC07"
				, "Ref07", "ImportRef07", "ExportRef07", "OuturnComment07", Core.Constants.CargoTypes.Hazardous);

			var warningMessage_Sub01 = @"There are pack lines with no inner quantity, outer pack quantity has been taken as inners.
Please verify in Shipment>Packing>Packs on following Shipments:
Sub01.";

			var warningMessage_Sub0102 = @"There are pack lines with no inner quantity, outer pack quantity has been taken as inners.
Please verify in Shipment>Packing>Packs on following Shipments:
Sub01
Sub02.";

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var packingLineDO = new PackingLine(ZGuid.NewZGuid(), Factory);

				var packLineBuilder = new PackingLineBuilder();
				var packingLineDO1 = packLineBuilder.Build(outerPackLine1, consol: consol);
				var packingLineDO2 = packLineBuilder.Build(outerPackLine2, consol: consol);

				var helper = new PackageGroupingHelper(consol, Context);
				helper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.DoNotGroup, outerPackLine1, packingLineDO1, Core.Constants.PkgUnit.Box);
				helper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.DoNotGroup, outerPackLine2, packingLineDO2, Core.Constants.PkgUnit.Box);

				helper.AddNoInnerPackLineValidation(packingLineDO, new List<PackingLine>() { packingLineDO1, packingLineDO2 });

				packingLineDO.ValidateAll();
				AssertHasWarning(packingLineDO.QuantityInfo, warningMessage_Sub0102);
			}

			var innerPackLine2 = shipmentSubShipment2.InnerPackLines.AddNew();
			innerPackLine2.JL_PackageCount = 11;
			innerPackLine2.JL_F3_NKPackType = Core.Constants.PkgUnit.Unit;
			innerPackLine2.JL_JL_OuterPackLine = outerPackLine2.PK;

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var packingLineDO = new PackingLine(ZGuid.NewZGuid(), Factory);

				var packLineBuilder = new PackingLineBuilder();
				var packingLineDO1 = packLineBuilder.Build(outerPackLine1, consol: consol);
				var packingLineDO2 = packLineBuilder.Build(outerPackLine2, consol: consol);

				var helper = new PackageGroupingHelper(consol, Context);
				helper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.DoNotGroup, outerPackLine1, packingLineDO1, Core.Constants.PkgUnit.Box);
				helper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.DoNotGroup, outerPackLine2, packingLineDO2, Core.Constants.PkgUnit.Box);

				helper.AddNoInnerPackLineValidation(packingLineDO, new List<PackingLine>() { packingLineDO1, packingLineDO2 });

				packingLineDO.ValidateAll();
				AssertHasWarning(packingLineDO.QuantityInfo, warningMessage_Sub01);
			}

			var innerPackLine1 = shipmentSubShipment1.InnerPackLines.AddNew();
			innerPackLine1.JL_PackageCount = 11;
			innerPackLine1.JL_F3_NKPackType = Core.Constants.PkgUnit.Unit;
			innerPackLine1.JL_JL_OuterPackLine = outerPackLine1.PK;

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var packingLineDO = new PackingLine(ZGuid.NewZGuid(), Factory);

				var packLineBuilder = new PackingLineBuilder();
				var packingLineDO1 = packLineBuilder.Build(outerPackLine1, consol: consol);
				var packingLineDO2 = packLineBuilder.Build(outerPackLine2, consol: consol);

				var helper = new PackageGroupingHelper(consol, Context);
				helper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.DoNotGroup, outerPackLine1, packingLineDO1, Core.Constants.PkgUnit.Box);
				helper.PopulatePackingQuantityAndPackageType(Constants.PackageGrouping.Codes.DoNotGroup, outerPackLine2, packingLineDO2, Core.Constants.PkgUnit.Box);

				helper.AddNoInnerPackLineValidation(packingLineDO, new List<PackingLine>() { packingLineDO1, packingLineDO2 });

				packingLineDO.ValidateAll();
				AssertNoWarning(packingLineDO.QuantityInfo, warningMessage_Sub01);
				AssertNoWarning(packingLineDO.QuantityInfo, warningMessage_Sub0102);
			}
		}

		#endregion

		#region HasUnAllocatedPackLines

		public void TestHasUnAllocatedPackLines()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "CNSHG";
			consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONTAINER1";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "JSASM";
			shipment.JS_GoodsDescription = "ASM JS_GoodsDescription";
			shipment.JS_MarksAndNumbers = "ASM JS_MarksAndNumbers";

			shipment.InnerPackLines.RemoveAndDeleteAll();
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var outerPackLine1 = shipment.OuterPackLines.AddNew();
			PopulatePackLine(outerPackLine1
				, 120, Constants.PkgUnit.BaleCompressed
				, 120, Constants.Weight.Kilograms
				, 120, Constants.Volume.CubicMetres
				, "BaleCompressed MarksAndNumbers", "BaleCompressed DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, container.PK, "HC07"
				, "Ref07", "ImportRef07", "ExportRef07", "OuturnComment07", Core.Constants.CargoTypes.Hazardous);

			var outerPackLine2 = shipment.OuterPackLines.AddNew();
			PopulatePackLine(outerPackLine2
				, 121, Constants.PkgUnit.Tote
				, 121, Constants.Weight.Kilograms
				, 121, Constants.Volume.CubicMetres
				, "Tote MarksAndNumbers", "Tote DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, container.PK, "HC08"
				, "Ref08", "ImportRef08", "ExportRef08", "OuturnComment08", Core.Constants.CargoTypes.Hazardous);

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipmentDOs = GetShipmentDOs(consol);
				AssertEquals(1, shipmentDOs.Count);

				var packageGroupingHelper = new PackageGroupingHelper(consol, Context);
				packageGroupingHelper.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByShipment, false, DataContext.BookingRequest, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				Assert(!packageGroupingHelper.HasUnAllocatedPackLines);
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				outerPackLine2.JL_JC = ZGuid.Empty;

				var shipmentDOs = GetShipmentDOs(consol);
				AssertEquals(1, shipmentDOs.Count);

				var packageGroupingHelper = new PackageGroupingHelper(consol, Context);
				packageGroupingHelper.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByShipment, true, DataContext.BookingRequest, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				Assert(packageGroupingHelper.HasUnAllocatedPackLines);
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				outerPackLine2.JL_JC = container.PK;

				var shipmentDOs = GetShipmentDOs(consol);
				AssertEquals(1, shipmentDOs.Count);

				var packageGroupingHelper = new PackageGroupingHelper(consol, Context);
				packageGroupingHelper.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByPackLine, true, DataContext.BookingRequest, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				Assert(!packageGroupingHelper.HasUnAllocatedPackLines);
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				outerPackLine2.JL_JC = ZGuid.Empty;

				var shipmentDOs = GetShipmentDOs(consol);
				AssertEquals(1, shipmentDOs.Count);

				var packageGroupingHelper = new PackageGroupingHelper(consol, Context);
				packageGroupingHelper.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByPackLine, false, DataContext.BookingRequest, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				Assert(packageGroupingHelper.HasUnAllocatedPackLines);
			}
		}

		#endregion

		#region HasPackLinesWithEmptyContainerNumberAndInvalidQuantity

		public void TestHasPackLinesWithEmptyContainerNumberAndInvalidQuantity()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "CNSHG";
			consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONTAINER1";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "JSASM";
			shipment.JS_GoodsDescription = "ASM JS_GoodsDescription";
			shipment.JS_MarksAndNumbers = "ASM JS_MarksAndNumbers";

			shipment.InnerPackLines.RemoveAndDeleteAll();
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var outerPackLine1 = shipment.OuterPackLines.AddNew();
			PopulatePackLine(outerPackLine1
				, 120, Constants.PkgUnit.BaleCompressed
				, 120, Constants.Weight.Kilograms
				, 120, Constants.Volume.CubicMetres
				, "BaleCompressed MarksAndNumbers", "BaleCompressed DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, container.PK, "HC07"
				, "Ref07", "ImportRef07", "ExportRef07", "OuturnComment07", Core.Constants.CargoTypes.Hazardous);

			var outerPackLine2 = shipment.OuterPackLines.AddNew();
			PopulatePackLine(outerPackLine2
				, 121, Constants.PkgUnit.Tote
				, 121, Constants.Weight.Kilograms
				, 121, Constants.Volume.CubicMetres
				, "Tote MarksAndNumbers", "Tote DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, container.PK, "HC08"
				, "Ref08", "ImportRef08", "ExportRef08", "OuturnComment08", Core.Constants.CargoTypes.Hazardous);

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipmentDOs = GetShipmentDOs(consol);
				AssertEquals(1, shipmentDOs.Count);

				var packageGroupingHelper = new PackageGroupingHelper(consol, Context);
				packageGroupingHelper.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByShipment, false, DataContext.BookingRequest, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				Assert(!packageGroupingHelper.HasPackLinesWithEmptyContainerNumberAndInvalidQuantity);
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				container.JC_ContainerNum = ZString.Empty;
				outerPackLine2.JL_PackageCount = 0;

				var shipmentDOs = GetShipmentDOs(consol);
				AssertEquals(1, shipmentDOs.Count);

				var packageGroupingHelper = new PackageGroupingHelper(consol, Context);
				packageGroupingHelper.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByShipment, true, DataContext.BookingRequest, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				Assert(packageGroupingHelper.HasPackLinesWithEmptyContainerNumberAndInvalidQuantity);
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				container.JC_ContainerNum = "CONTAINER1";
				outerPackLine2.JL_PackageCount = 100;

				var shipmentDOs = GetShipmentDOs(consol);
				AssertEquals(1, shipmentDOs.Count);

				var packageGroupingHelper = new PackageGroupingHelper(consol, Context);
				packageGroupingHelper.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByPackLine, true, DataContext.BookingRequest, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				Assert(!packageGroupingHelper.HasPackLinesWithEmptyContainerNumberAndInvalidQuantity);
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				container.JC_ContainerNum = ZString.Empty;
				outerPackLine2.JL_PackageCount = 0;

				var shipmentDOs = GetShipmentDOs(consol);
				AssertEquals(1, shipmentDOs.Count);

				var packageGroupingHelper = new PackageGroupingHelper(consol, Context);
				packageGroupingHelper.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByPackLine, false, DataContext.BookingRequest, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				Assert(packageGroupingHelper.HasPackLinesWithEmptyContainerNumberAndInvalidQuantity);
			}
		}

		#endregion

		#region AddDimensionsMandatoryErrorForOOG

		public void TestAddDimensionsMandatoryErrorForOOG()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "CNSHG";
			consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONTAINER1";

			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_ISOType = "UUUU";
			container.JC_RC = refContainer.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "JSASM";
			shipment.JS_GoodsDescription = "ASM JS_GoodsDescription";
			shipment.JS_MarksAndNumbers = "ASM JS_MarksAndNumbers";

			shipment.InnerPackLines.RemoveAndDeleteAll();
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var outerPackLine1 = shipment.OuterPackLines.AddNew();
			PopulatePackLine(outerPackLine1
				, 120, Constants.PkgUnit.BaleCompressed
				, 120, Constants.Weight.Kilograms
				, 120, Constants.Volume.CubicMetres
				, "BaleCompressed MarksAndNumbers", "BaleCompressed DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, container.PK, "HC07"
				, "Ref07", "ImportRef07", "ExportRef07", "OuturnComment07", Core.Constants.CargoTypes.Hazardous);
			outerPackLine1.JL_Length = 1;
			outerPackLine1.JL_Width = 1;
			outerPackLine1.JL_Height = 1;

			var outerPackLine2 = shipment.OuterPackLines.AddNew();
			PopulatePackLine(outerPackLine2
				, 121, Constants.PkgUnit.Tote
				, 121, Constants.Weight.Kilograms
				, 121, Constants.Volume.CubicMetres
				, "Tote MarksAndNumbers", "Tote DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, container.PK, "HC08"
				, "Ref08", "ImportRef08", "ExportRef08", "OuturnComment08", Core.Constants.CargoTypes.Hazardous);
			outerPackLine2.JL_Length = 1;
			outerPackLine2.JL_Width = 1;
			outerPackLine2.JL_Height = 1;

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipmentDOs = GetShipmentDOs(consol);
				AssertEquals(1, shipmentDOs.Count);

				var packageGroupingHelper = new PackageGroupingHelper(consol, Context);
				packageGroupingHelper.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByShipment, false, DataContext.BookingRequest, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				Assert(!packageGroupingHelper.AddDimensionsMandatoryErrorForOOG);
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				outerPackLine2.JL_Length = 0;
				outerPackLine2.JL_Width = 1;
				outerPackLine2.JL_Height = 1;

				var shipmentDOs = GetShipmentDOs(consol);
				AssertEquals(1, shipmentDOs.Count);

				var packageGroupingHelper = new PackageGroupingHelper(consol, Context);
				packageGroupingHelper.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByShipment, true, DataContext.BookingRequest, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				Assert(packageGroupingHelper.AddDimensionsMandatoryErrorForOOG);
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				outerPackLine2.JL_Length = 1;
				outerPackLine2.JL_Width = 0;
				outerPackLine2.JL_Height = 1;

				var shipmentDOs = GetShipmentDOs(consol);
				AssertEquals(1, shipmentDOs.Count);

				var packageGroupingHelper = new PackageGroupingHelper(consol, Context);
				packageGroupingHelper.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByPackLine, true, DataContext.BookingRequest, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				Assert(packageGroupingHelper.AddDimensionsMandatoryErrorForOOG);
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				outerPackLine2.JL_Length = 1;
				outerPackLine2.JL_Width = 1;
				outerPackLine2.JL_Height = -0;

				var shipmentDOs = GetShipmentDOs(consol);
				AssertEquals(1, shipmentDOs.Count);

				var packageGroupingHelper = new PackageGroupingHelper(consol, Context);
				packageGroupingHelper.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByPackLine, false, DataContext.BookingRequest, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				Assert(packageGroupingHelper.AddDimensionsMandatoryErrorForOOG);
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				container.JC_RC = ZGuid.Empty;

				outerPackLine2.JL_Length = 0;
				outerPackLine2.JL_Width = 0;
				outerPackLine2.JL_Height = -0;

				var shipmentDOs = GetShipmentDOs(consol);
				AssertEquals(1, shipmentDOs.Count);

				var packageGroupingHelper = new PackageGroupingHelper(consol, Context);
				packageGroupingHelper.PopulateGroupedAndConsolidatedPackingLines(shipmentDOs, Constants.PackageGrouping.Codes.GroupByPackLine, false, DataContext.BookingRequest, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				Assert(!packageGroupingHelper.AddDimensionsMandatoryErrorForOOG);
			}
		}

		#endregion

		#region TestIsShowInnerPackLines

		public void TestIsShowInnerPackLines()
		{
			var standardServiceLevel = Factory.LoadFromUniqueKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, new ZString("STD"));
			var directServiceLevel = Factory.LoadFromUniqueKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, new ZString("TSP"));

			var matchedRule = CreateRefCountryRules(Constants.CountryCodes.Australia, Constants.CountryCodes.China, Constants.TransportModes.Sea, standardServiceLevel.PK, true);
			CreateRefCountryRules(Constants.CountryCodes.Australia, Constants.CountryCodes.China, Constants.TransportModes.Sea, directServiceLevel.PK, false);
			CreateRefCountryRules(Constants.CountryCodes.Australia, Constants.CountryCodes.China, Constants.TransportModes.Air, standardServiceLevel.PK, false);
			CreateRefCountryRules(Constants.CountryCodes.UnitedStates, Constants.CountryCodes.China, Constants.TransportModes.Air, standardServiceLevel.PK, false);
			CreateRefCountryRules(Constants.CountryCodes.Australia, Constants.CountryCodes.UnitedStates, Constants.TransportModes.Air, standardServiceLevel.PK, false);
			var matchedRuleDestinationCountryIsEmpty = CreateRefCountryRules(Constants.CountryCodes.Australia, ZString.Empty, Constants.TransportModes.Sea, standardServiceLevel.PK, true);
			var matchedRuleOriginCountryIsEmpty = CreateRefCountryRules(ZString.Empty, Constants.CountryCodes.China, Constants.TransportModes.Sea, standardServiceLevel.PK, true);
			var matchedRuleTransportModeIsEmpty = CreateRefCountryRules(Constants.CountryCodes.Australia, Constants.CountryCodes.China, ZString.Empty, standardServiceLevel.PK, true);
			var matchedRuleServiceLevelIsEmpty = CreateRefCountryRules(Constants.CountryCodes.Australia, Constants.CountryCodes.China, Constants.TransportModes.Sea, ZGuid.Empty, true);
			var destinationCountryOnlyRule = CreateRefCountryRules(ZString.Empty, Constants.CountryCodes.China, ZString.Empty, ZGuid.Empty, true);
			var originCountryCountryOnlyRule = CreateRefCountryRules(Constants.CountryCodes.Australia, ZString.Empty, ZString.Empty, ZGuid.Empty, true);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			consol.Shipments.AddNew();

			Factory.Save();
			var shipment = consol.Shipments.OfType<ForwardingShipment>().First();

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Assert("The registry is disabled", !new PackageGroupingHelper(consol, null).IsShowInnerPackLines(shipment));
			}

			void AssertMatchRule(RefCountryRules rule)
			{
				var message = $"{rule.R7_RN_NKOrigin}-{rule.R7_RN_NKDestination}-{rule.R7_TransportMode}-{rule.ServiceLevel?.RS_Code ?? ZString.Empty}";
				Assert(message, new PackageGroupingHelper(consol, null).IsShowInnerPackLines(shipment));

				rule.R7_ShowInner = false;
				Assert(message, !new PackageGroupingHelper(consol, null).IsShowInnerPackLines(shipment));
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert("The subShipment has no inner pack line.", !shipment.InnerPackLines.Any());
				Assert("The subShipment has no inner pack line.", !new PackageGroupingHelper(consol, null).IsShowInnerPackLines(shipment));

				var innerPackLine = shipment.InnerPackLines.AddNew();
				Factory.Save();

				Assert("The subShipment has inner pack line.", shipment.InnerPackLines.Any());
				Assert("The subShipment has inner pack line, but the inner pack line is not linked to the outer pack line", !new PackageGroupingHelper(consol, null).IsShowInnerPackLines(shipment));

				innerPackLine.JL_JL_OuterPackLine = shipment.OuterPackLines.AddNew().PK;
				AssertMatchRule(matchedRule);

				matchedRule.Delete();
				Factory.Save();

				AssertMatchRule(matchedRuleServiceLevelIsEmpty);

				matchedRuleServiceLevelIsEmpty.Delete();
				Factory.Save();

				AssertMatchRule(matchedRuleTransportModeIsEmpty);

				matchedRuleTransportModeIsEmpty.Delete();
				Factory.Save();

				AssertMatchRule(matchedRuleOriginCountryIsEmpty);

				matchedRuleOriginCountryIsEmpty.Delete();
				Factory.Save();

				AssertMatchRule(destinationCountryOnlyRule);

				destinationCountryOnlyRule.Delete();
				Factory.Save();

				AssertMatchRule(matchedRuleDestinationCountryIsEmpty);

				matchedRuleDestinationCountryIsEmpty.Delete();
				Factory.Save();

				AssertMatchRule(originCountryCountryOnlyRule);

				originCountryCountryOnlyRule.Delete();
				Factory.Save();

				Assert(!new PackageGroupingHelper(consol, null).IsShowInnerPackLines(shipment));
			}
		}

		#endregion

		#region Implementation

		List<Shipment> GetShipmentDOs(ForwardingConsol consol)
		{
			var shipmentDOs = new List<Shipment>();
			var shipmentBuilder = new ShipmentBuilder(Context);

			foreach (ForwardingShipment shipment in consol.TopLevelShipments)
			{
				shipmentDOs.Add(shipmentBuilder.Build(shipment, s => GetProcessedPackingLines(consol, s)));
			}

			return shipmentDOs;
		}

		IEnumerable<PackingLine> GetProcessedPackingLines(ForwardingConsol consol, ForwardingShipment shipment)
		{
			var packLineBuilder = new PackingLineBuilder();
			var packLineDOs = new List<PackingLine>();

			foreach (PackLine packLineBO in shipment.OuterPackLines)
			{
				var packingLineDO = packLineBuilder.Build(packLineBO, Constants.Weight.Kilograms, Constants.Volume.CubicMetres, consol: consol);

				packingLineDO.ShipmentEntryNumbers = ZString.Join(", ", packLineBO.Shipment.CusEntryNumbers.Cast<CusEntryNumber>().Select(c => ZString.Format("{0}:{1}", c.CE_EntryType, c.CE_EntryNum)).ToArray());
				packingLineDO.HasInnerPackLines = packLineBO.Shipment.InnerPackLines.Cast<ForwardingPackLine>().Any(x => x.JL_JL_OuterPackLine == packLineBO.PK);
				packLineDOs.Add(packingLineDO);
			}

			return packLineDOs;
		}

		RefCountryRules CreateRefCountryRules(ZString originCountry, ZString destinationCountry, ZString transportMode, ZGuid serviceLevelPK, ZBool isShowInner)
		{
			var rule = Factory.NewWithValidTestData<RefCountryRules>();

			rule.R7_RN_NKOrigin = originCountry;
			rule.R7_RN_NKDestination = destinationCountry;
			rule.R7_TransportMode = transportMode;
			rule.R7_RS = serviceLevelPK;
			rule.R7_ShowInner = isShowInner;

			return rule;
		}

		PackLine PopulatePackLine(PackLine packLine, ZInt packageCount, ZString packType, ZDecimal weight, ZString unitOfWeight, ZDecimal volume, ZString unitOfVolume, ZString marksAndNumbers, ZString detailedDescription
			, ZBool requiresTemperatureControl, int requiredTemperatureMinimum, int requiredTemperatureMaximum, ZString requiredTemperatureUnit, ZGuid containerPK, ZString harmonisedCode
			, ZString referenceNumber, ZString importReferenceNumber, ZString exportReferenceNumber, ZString outturnComment, ZString commodityCode)
		{
			packLine.JL_PackageCount = packageCount;
			packLine.JL_F3_NKPackType = packType;
			packLine.JL_ActualWeight = weight;
			packLine.JL_ActualWeightUQ = unitOfWeight;
			packLine.JL_ActualVolume = volume;
			packLine.JL_ActualVolumeUQ = unitOfVolume;
			packLine.JL_MarksAndNumbers = marksAndNumbers;
			packLine.JL_DetailedDescription = detailedDescription;
			packLine.JL_RequiredTemperatureMinimum = requiredTemperatureMinimum;
			packLine.JL_RequiredTemperatureMaximum = requiredTemperatureMaximum;
			packLine.JL_RequiredTemperatureUnit = requiredTemperatureUnit;
			packLine.JL_RequiresTemperatureControl = requiresTemperatureControl;
			packLine.JL_JC = containerPK;
			packLine.JL_HarmonisedCode = harmonisedCode;
			packLine.JL_RefNumber = referenceNumber;
			packLine.JL_ImportRefNumber = importReferenceNumber;
			packLine.JL_ExportRefNumber = exportReferenceNumber;
			packLine.JL_OutturnComment = outturnComment;
			packLine.JL_RH_NKCommodityCode = commodityCode;

			return packLine;
		}

		UNDGDataItem PopulateDangerousGoods(UNDGDataItem dangerousGood, ZInt packageCount, ZBool isLimitedQuantity, ZGuid substancePK, ZGuid contactPK, ZDecimal weight, ZString unitOfWeight, ZString technicalName, ZDecimal flashPoint)
		{
			dangerousGood.DI_PackageCount = packageCount;
			dangerousGood.DI_DG = substancePK;
			dangerousGood.DI_OC_DGContact = contactPK;
			dangerousGood.DI_DGWeight = weight;
			dangerousGood.DI_UnitOfWeight = unitOfWeight;
			dangerousGood.DI_TechnicalName = technicalName;
			dangerousGood.DI_DGFlashPoint = flashPoint;
			dangerousGood.DI_IsLimitedQuantity = isLimitedQuantity;

			return dangerousGood;
		}

		UNDGSubstance CreatUNDGSubstance(ZString unno, ZString variant, ZString dgClass, ZString flashPoint, ZString subLabel1, ZString standard, ZString state
			, ZString exceptedQuantityCode, ZString pg, ZString psn, ZString ems)
		{
			var undgSubstance = Factory.New<UNDGSubstance>();

			undgSubstance.DG_UNNO = unno;
			undgSubstance.DG_Variant = variant;
			undgSubstance.DG_Code = unno + variant;
			undgSubstance.DG_Class = dgClass;
			undgSubstance.DG_FlashPoint = flashPoint;
			undgSubstance.DG_SubLabel1 = subLabel1;
			undgSubstance.DG_Standard = standard;
			undgSubstance.DG_State = state;
			undgSubstance.DG_ExceptedQuantityCode = exceptedQuantityCode;
			undgSubstance.DG_PG = pg;
			undgSubstance.DG_PSN = psn;
			undgSubstance.DG_EMS = ems;

			return undgSubstance;
		}

		IContext Context
		{
			get
			{
				if (context == null)
				{
					context = new CommonContext(Factory);
				}

				return context;
			}
		}
		IContext context;

		#endregion
	}
}
