using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2012_11;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;
using UniversalDate = Enterprise.UniversalDataBuss.DataObjects.Universal.Date;
using UniversalDateType = Enterprise.UniversalDataBuss.DataObjects.Universal.DateType;
using UniversalEntryType = Enterprise.UniversalDataBuss.DataObjects.Universal.EntryType;
using UniversalOrganizationAddress = Enterprise.UniversalDataBuss.DataObjects.Universal.OrganizationAddress;
using UniversalPackingLine = Enterprise.UniversalDataBuss.DataObjects.Universal.PackingLine;
using UniversalReference = Enterprise.UniversalDataBuss.DataObjects.Universal.Reference;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;
using UniversalWayBillType = Enterprise.UniversalDataBuss.DataObjects.Universal.WayBillType;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class ShipmentDataObjectSplitPackageHelperTest : OrganizationAddressTestHelper
	{
		public void TestFilterValidImportedPackagesForDispatch()
		{
			var warehouse1AddressBO = Factory.NewWithValidTestData<OrgAddress>();
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.DataContext = DataContextFactory.New();
			shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitDispatch, "DC00000001");
			shipmentDataObject.WayBillNumber = "BACON PANCAKES";
			shipmentDataObject.WayBillType = new UniversalWayBillType { Code = WayBillTypeList.Codes.House };

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<UniversalOrganizationAddress> { new UniversalOrganizationAddress()
			{
				AddressShortCode = warehouse1AddressBO.AddressCode,
				AddressType = nameof(DocAddressType.LocalCartageCFS),
				OrganizationCode = warehouse1AddressBO.Header.OH_Code
			} });

			var header1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			header1.DataContext = new DataContext();
			header1.DataContext.AddDataSource(DataContextType.TransitDispatchHeader, "TD00000001");
			header1.SetDateCollection(() => new List<UniversalDate> { new UniversalDate { Type = UniversalDateType.Pack, Value = new ZDateTime(2020, 6, 1, 10, 17, 0) } });
			var header2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			header2.DataContext = new DataContext();
			header2.DataContext.AddDataSource(DataContextType.TransitDispatchHeader, "TD00000002");
			header2.SetDateCollection(() => new List<UniversalDate> { new UniversalDate { Type = UniversalDateType.Pack, Value = new ZDateTime(2020, 6, 1, 10, 6, 0) } });
			shipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>() { header1, header2 });

			var loadList = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			loadList.DataContext = new DataContext();
			loadList.DataContext.AddDataSource(DataContextType.TransitDispatchLoadList, "DLL00000011");
			shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { loadList });

			var packingLine1 = ShipmentDataObjectReaderTest.CreatePacklineForDispatch();
			packingLine1.ReferenceNumber = ZString.Empty;
			packingLine1.OutturnQty = 1;
			packingLine1.LoadDate = ZDateTime.Now;
			packingLine1.PackingLineID = ZString.Empty;
			var packingLine2 = ShipmentDataObjectReaderTest.CreatePacklineForDispatch();
			packingLine2.ReferenceNumber = "S00001557-CTN2";
			packingLine2.OutturnQty = 0;
			packingLine2.LoadDate = ZDateTime.Now;
			var packingLine3 = ShipmentDataObjectReaderTest.CreatePacklineForDispatch();
			packingLine3.ReferenceNumber = "S00001557-CTN3";
			packingLine3.OutturnQty = 1;
			packingLine3.LoadDate = new ZDateTime(2020, 6, 1, 10, 17, 0);
			packingLine3.SetReferenceNumberCollection(() => new List<UniversalReference>() { new UniversalReference { Type = new UniversalEntryType { Code = "TDU" }, ReferenceNumber = "TD00000001" } });
			var packingLine4 = ShipmentDataObjectReaderTest.CreatePacklineForDispatch();
			packingLine4.ReferenceNumber = "S00001557-CTN4";
			packingLine4.OutturnQty = 1;
			packingLine4.LoadDate = ZDateTime.Empty;
			packingLine4.SetReferenceNumberCollection(() => new List<UniversalReference>() { new UniversalReference { Type = new UniversalEntryType { Code = "TDU" }, ReferenceNumber = "TD00000002" } });
			var packingLine5 = ShipmentDataObjectReaderTest.CreatePacklineForDispatch();
			packingLine5.ReferenceNumber = ZString.Empty;
			packingLine5.OutturnQty = 3;
			packingLine5.PackingLineID = "PLID00001";
			packingLine5.LoadDate = new ZDateTime(2020, 6, 1, 10, 17, 0);
			packingLine5.SetReferenceNumberCollection(() => new List<UniversalReference>() { new UniversalReference { Type = new UniversalEntryType { Code = "TDU" }, ReferenceNumber = "TD00000002" } });

			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine> { packingLine1, packingLine2, packingLine3, packingLine4, packingLine5 });

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_LastKnownTransitWarehouseStatusDateTime = new ZDateTime(2020, 6, 1, 10, 1, 0);
			packLine1.PkgPackageCollection.AddNew().KP_PackageID = "S00001557-CTN3";
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_LastKnownTransitWarehouseStatusDateTime = new ZDateTime(2020, 6, 1, 10, 10, 0);
			packLine2.PkgPackageCollection.AddNew().KP_PackageID = "S00001557-CTN4";
			var helper = GetShipmentDataObjectSplitPackageHelper(shipmentDataObject, shipment, warehouse1AddressBO);
			var importedPackages = helper.GetValidImportedPackages();

			AssertContainsExactElementsInAnyOrder(new[] { packingLine3, packingLine5 }, importedPackages);
		}

		public void TestFilterValidImportedPackagesByTimeAndAddressForDispatch()
		{
			var warehouse1AddressBO = Factory.NewWithValidTestData<OrgAddress>();
			warehouse1AddressBO.OA_RL_NKRelatedPortCode = "AUSYD";

			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.DataContext = DataContextFactory.New();
			shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitDispatch, "DC00000001");
			shipmentDataObject.WayBillNumber = "BACON PANCAKES";
			shipmentDataObject.WayBillType = new UniversalWayBillType { Code = WayBillTypeList.Codes.House };

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<UniversalOrganizationAddress> { new UniversalOrganizationAddress()
			{
				AddressShortCode = warehouse1AddressBO.AddressCode,
				AddressType = nameof(DocAddressType.LocalCartageCFS),
				OrganizationCode = warehouse1AddressBO.Header.OH_Code
			} });

			var header1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			header1.DataContext = new DataContext();
			header1.DataContext.AddDataSource(DataContextType.TransitDispatchHeader, "TD00000001");
			header1.SetDateCollection(() => new List<UniversalDate> { new UniversalDate { Type = UniversalDateType.Pack, Value = new ZDateTime(2024, 1, 1, 10, 17, 0) } });

			var header2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			header2.DataContext = new DataContext();
			header2.DataContext.AddDataSource(DataContextType.TransitDispatchHeader, "TD00000002");
			header2.SetDateCollection(() => new List<UniversalDate> { new UniversalDate { Type = UniversalDateType.Pack, Value = new ZDateTime(2024, 1, 1, 10, 6, 0) } });

			shipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>() { header1, header2 });

			var loadList = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			loadList.DataContext = new DataContext();
			loadList.DataContext.AddDataSource(DataContextType.TransitDispatchLoadList, "DLL00000011");
			shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { loadList });

			// From WI00702640: The XML showed the unload date at AU is 2024-01-16T17:11:38
			var packingLine1 = CreatePacklineForDispatch("S00001557-CTN1", 1, ZDateTime.Now, null, null);
			var packingLine2 = CreatePacklineForDispatch("S00001557-CTN2", 0, ZDateTime.Now, null, null);
			var packingLine3 = CreatePacklineForDispatch("S00001557-CTN3", 1, ZDateTime.Empty, new ZDateTime(2024, 1, 6, 17, 11, 0), new List<UniversalReference>() { new UniversalReference { Type = new UniversalEntryType { Code = "TDU" }, ReferenceNumber = "TD00000001" } });
			var packingLine4 = CreatePacklineForDispatch("S00001557-CTN4", 1, ZDateTime.Empty, new ZDateTime(2024, 1, 6, 17, 11, 0), new List<UniversalReference>() { new UniversalReference { Type = new UniversalEntryType { Code = "TDU" }, ReferenceNumber = "TD00000002" } });
			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine> { packingLine1, packingLine2, packingLine3, packingLine4 });

			// From WI00702640: The last known TW date recorded in the packline (at NZ) is 2024-01-06T19:02
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var warehouse2AddressBO = Factory.NewWithValidTestData<OrgAddress>();
			warehouse2AddressBO.OA_RL_NKRelatedPortCode = "NZAKL";

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.PkgPackageCollection.AddNew().KP_PackageID = "S00001557-CTN3";
			packLine1.JL_LastKnownTransitWarehouseStatusDateTime = new ZDateTime(2024, 1, 6, 19, 2, 0);
			packLine1.JL_OA_LastKnownTransitWarehouseAddress = warehouse2AddressBO.PK;

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.PkgPackageCollection.AddNew().KP_PackageID = "S00001557-CTN4";
			packLine2.JL_LastKnownTransitWarehouseStatusDateTime = new ZDateTime(2024, 1, 6, 19, 2, 0);
			packLine2.JL_OA_LastKnownTransitWarehouseAddress = warehouse2AddressBO.PK;

			var shipmentDataObjectSplitPackageHelper = GetShipmentDataObjectSplitPackageHelper(shipmentDataObject, shipment, warehouse1AddressBO);
			var importedPackages = shipmentDataObjectSplitPackageHelper.GetValidImportedPackages();

			// If address co-exist with time, then it should be converted to UTC based on address then filter
			AssertContainsExactElementsInAnyOrder(new[] { packingLine1, packingLine3, packingLine4 }, importedPackages);

			// If address is not present, then filter by the local time (possible different timezone)
			// This can simulate the issue in scenario (WI00702640 - TWH: Incorrect time validation when update last known TW status)
			// Local data does not have address
			packLine1.JL_OA_LastKnownTransitWarehouseAddress = ZGuid.Empty;
			packLine2.JL_OA_LastKnownTransitWarehouseAddress = ZGuid.Empty;

			importedPackages = shipmentDataObjectSplitPackageHelper.GetValidImportedPackages();
			AssertContainsExactElementsInAnyOrder(new[] { packingLine1 }, importedPackages);

			// Import XML data does not have address
			warehouse1AddressBO.OA_RL_NKRelatedPortCode = ZString.Empty;
			shipmentDataObject.SetOrganizationAddressCollection(() => new List<UniversalOrganizationAddress> { new UniversalOrganizationAddress()
			{
				AddressShortCode = warehouse1AddressBO.AddressCode,
				AddressType = nameof(DocAddressType.LocalCartageCFS),
				OrganizationCode = warehouse1AddressBO.Header.OH_Code
			} });

			importedPackages = shipmentDataObjectSplitPackageHelper.GetValidImportedPackages();
			AssertContainsExactElementsInAnyOrder(new[] { packingLine1 }, importedPackages);
		}

		public void TestFilterValidImportedPackagesForPrepareDispatch()
		{
			var warehouse1AddressBO = Factory.NewWithValidTestData<OrgAddress>();
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.DataContext = DataContextFactory.New();
			shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitDispatch, "DC00000001");
			shipmentDataObject.WayBillNumber = "BACON PANCAKES";
			shipmentDataObject.WayBillType = new UniversalWayBillType { Code = WayBillTypeList.Codes.House };

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<UniversalOrganizationAddress> { new UniversalOrganizationAddress()
			{
				AddressShortCode = warehouse1AddressBO.AddressCode,
				AddressType = nameof(DocAddressType.LocalCartageCFS),
				OrganizationCode = warehouse1AddressBO.Header.OH_Code
			} });

			var packingLine1 = ShipmentDataObjectReaderTest.CreatePacklineForReceive();
			packingLine1.ReferenceNumber = ZString.Empty;
			packingLine1.OutturnQty = 1;
			packingLine1.UnloadDate = ZDateTime.Now;
			packingLine1.PackingLineID = ZString.Empty;
			var packingLine2 = ShipmentDataObjectReaderTest.CreatePacklineForReceive();
			packingLine2.ReferenceNumber = "S00001557-CTN2";
			packingLine2.OutturnQty = 0;
			packingLine2.UnloadDate = ZDateTime.Now;
			var packingLine3 = ShipmentDataObjectReaderTest.CreatePacklineForReceive();
			packingLine3.ReferenceNumber = "S00001557-CTN3";
			packingLine3.OutturnQty = 1;
			packingLine3.UnloadDate = new ZDateTime(2020, 6, 1, 10, 17, 0);
			var packingLine4 = ShipmentDataObjectReaderTest.CreatePacklineForReceive();
			packingLine4.ReferenceNumber = "S00001557-CTN4";
			packingLine4.OutturnQty = 1;
			packingLine4.UnloadDate = ZDateTime.Empty;
			var packingLine5 = ShipmentDataObjectReaderTest.CreatePacklineForReceive();
			packingLine5.ReferenceNumber = ZString.Empty;
			packingLine5.OutturnQty = 3;
			packingLine5.PackingLineID = "PLID001";
			packingLine5.UnloadDate = new ZDateTime(2020, 6, 1, 10, 17, 0);
			packingLine5.SetReferenceNumberCollection(() => new List<UniversalReference>() { new UniversalReference { Type = new UniversalEntryType { Code = "TRU" }, ReferenceNumber = "TR00000001" } });

			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine> { packingLine1, packingLine2, packingLine3, packingLine4, packingLine5 });

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_LastKnownTransitWarehouseStatusDateTime = new ZDateTime(2020, 6, 1, 10, 1, 0);
			packLine1.PkgPackageCollection.AddNew().KP_PackageID = "S00001557-CTN3";
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_LastKnownTransitWarehouseStatusDateTime = new ZDateTime(2020, 6, 1, 10, 10, 0);
			packLine2.PkgPackageCollection.AddNew().KP_PackageID = "S00001557-CTN4";
			var importedPackages = GetShipmentDataObjectSplitPackageHelper(shipmentDataObject, shipment, warehouse1AddressBO).GetValidImportedPackages();

			AssertContainsExactElementsInAnyOrder("Should return packages with ReferenceNumber or PackingLineID", new[] { packingLine3, packingLine5 }, importedPackages);
		}

		public void TestFilterValidImportedPackagesForReceive()
		{
			var warehouse1AddressBO = Factory.NewWithValidTestData<OrgAddress>();
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.DataContext = DataContextFactory.New();
			shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitReceive, "RC00000001");
			shipmentDataObject.WayBillNumber = "BACON PANCAKES";
			shipmentDataObject.WayBillType = new UniversalWayBillType { Code = WayBillTypeList.Codes.House };

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<UniversalOrganizationAddress> { new UniversalOrganizationAddress()
			{
				AddressShortCode = warehouse1AddressBO.AddressCode,
				AddressType = nameof(DocAddressType.LocalCartageCFS),
				OrganizationCode = warehouse1AddressBO.Header.OH_Code
			} });

			var header1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			header1.DataContext = new DataContext();
			header1.DataContext.AddDataSource(DataContextType.TransitReceiveHeader, "TR00000001");
			header1.SetDateCollection(() => new List<UniversalDate> { new UniversalDate { Type = UniversalDateType.Unpack, Value = new ZDateTime(2020, 6, 1, 10, 17, 0) } });
			var header2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			header2.DataContext = new DataContext();
			header2.DataContext.AddDataSource(DataContextType.TransitReceiveHeader, "TR00000002");
			header2.SetDateCollection(() => new List<UniversalDate> { new UniversalDate { Type = UniversalDateType.Unpack, Value = new ZDateTime(2020, 6, 1, 10, 6, 0) } });
			shipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>() { header1, header2 });

			var packingLine1 = ShipmentDataObjectReaderTest.CreatePacklineForReceive();
			packingLine1.ReferenceNumber = ZString.Empty;
			packingLine1.OutturnQty = 1;
			packingLine1.PackingLineID = ZString.Empty;
			packingLine1.UnloadDate = ZDateTime.Now;
			var packingLine2 = ShipmentDataObjectReaderTest.CreatePacklineForReceive();
			packingLine2.ReferenceNumber = "S00001557-CTN2";
			packingLine2.OutturnQty = 0;
			packingLine2.UnloadDate = ZDateTime.Now;
			var packingLine3 = ShipmentDataObjectReaderTest.CreatePacklineForReceive();
			packingLine3.ReferenceNumber = "S00001557-CTN3";
			packingLine3.OutturnQty = 1;
			packingLine3.UnloadDate = new ZDateTime(2020, 6, 1, 10, 17, 0);
			packingLine3.SetReferenceNumberCollection(() => new List<UniversalReference>() { new UniversalReference { Type = new UniversalEntryType { Code = "TRU" }, ReferenceNumber = "TR00000001" } });
			var packingLine4 = ShipmentDataObjectReaderTest.CreatePacklineForReceive();
			packingLine4.ReferenceNumber = "S00001557-CTN4";
			packingLine4.OutturnQty = 1;
			packingLine4.UnloadDate = ZDateTime.Empty;
			packingLine4.SetReferenceNumberCollection(() => new List<UniversalReference>() { new UniversalReference { Type = new UniversalEntryType { Code = "TRU" }, ReferenceNumber = "TR00000002" } });
			var packingLine5 = ShipmentDataObjectReaderTest.CreatePacklineForReceive();
			packingLine5.ReferenceNumber = ZString.Empty;
			packingLine5.OutturnQty = 3;
			packingLine5.PackingLineID = "PLID001";
			packingLine5.UnloadDate = new ZDateTime(2020, 6, 1, 10, 17, 0);
			packingLine5.SetReferenceNumberCollection(() => new List<UniversalReference>() { new UniversalReference { Type = new UniversalEntryType { Code = "TRU" }, ReferenceNumber = "TR00000001" } });

			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine> { packingLine1, packingLine2, packingLine3, packingLine4, packingLine5 });

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_LastKnownTransitWarehouseStatusDateTime = new ZDateTime(2020, 6, 1, 10, 1, 0);
			packLine1.PkgPackageCollection.AddNew().KP_PackageID = "S00001557-CTN3";
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_LastKnownTransitWarehouseStatusDateTime = new ZDateTime(2020, 6, 1, 10, 10, 0);
			packLine2.PkgPackageCollection.AddNew().KP_PackageID = "S00001557-CTN4";
			var importedPackages = GetShipmentDataObjectSplitPackageHelper(shipmentDataObject, shipment, warehouse1AddressBO).GetValidImportedPackages();

			AssertContainsExactElementsInAnyOrder(new[] { packingLine3, packingLine5 }, importedPackages);
		}

		public void TestProcessTransitReceive_UpdateDGInfo()
		{
			var warehouse1AddressBO = Factory.NewWithValidTestData<OrgAddress>();
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.DataContext = DataContextFactory.New();
			shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitReceive, "RC00000001");

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<UniversalOrganizationAddress> { new UniversalOrganizationAddress()
			{
				AddressShortCode = warehouse1AddressBO.AddressCode,
				AddressType = nameof(DocAddressType.LocalCartageCFS),
				OrganizationCode = warehouse1AddressBO.Header.OH_Code
			} });

			var packageUNDG = new UNDG();
			packageUNDG.UNDGCode = "1234";
			packageUNDG.IMOClass = "1";
			packageUNDG.FlashPoint = "2.00";
			packageUNDG.PackQty = 1;
			packageUNDG.Volume = 3;
			packageUNDG.VolumeUQ = new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres };
			packageUNDG.Weight = 1;
			packageUNDG.WeightUQ = new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms };
			packageUNDG.PackingInstructionSection = "PM";

			var packline = new UniversalPackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packline.ReferenceNumber = "S00001000-001";
			packline.PackQty = 1;
			packline.OutturnQty = 1;
			packline.UnloadDate = new ZDateTime(2020, 6, 1, 10, 6, 0);
			packline.GoodsDescription = "Goods 123";
			packline.PackingLineID = "TEST001";
			packline.PackType = new PackageType() { Code = Core.Constants.PkgUnit.Pallet };
			packline.LengthUnit = new UnitOfLength() { Code = Core.Constants.Length.Metres };
			packline.WeightUnit = new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms };
			packline.VolumeUnit = new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres };
			packline.SetUNDGCollection(() => new List<UNDG>() { packageUNDG });
			packline.SetReferenceNumberCollection(() => new List<UniversalReference>() { new UniversalReference { Type = new UniversalEntryType { Code = "TRU" }, ReferenceNumber = "TR00000001" } });
			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine> { packline });

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var forwardingPackLine = Factory.New<ForwardingPackLine>();
			forwardingPackLine.JL_PackageCount = 1;
			forwardingPackLine.JL_RefNumber = "REF001";
			forwardingPackLine.JL_PackLineId = "TEST001";
			forwardingPackLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			forwardingPackLine.JL_Description = "Goods\r\n123";
			forwardingPackLine.JL_FreightMode = FreightConstants.OuterPackType;

			var mockUNDGSubstance = Factory.New<UNDGSubstance>();
			mockUNDGSubstance.DG_Code = "1234";
			mockUNDGSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var packlineUNDG = forwardingPackLine.UNDGs.AddNew();
			packlineUNDG.DI_DG = mockUNDGSubstance.PK;
			packlineUNDG.DI_DGFlashPoint = 1m;
			packlineUNDG.DI_IMOClass = "1";
			shipment.OuterPackLines.Add(forwardingPackLine);

			AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown, forwardingPackLine.JL_OriginTransitWarehouseStatus);
			AssertEquals(ZString.Empty, packlineUNDG.DI_UnitOfVolume);
			AssertEquals(new ZDecimal(0), packlineUNDG.DI_DGVolume);
			AssertEquals(ZString.Empty, packlineUNDG.DI_UnitOfWeight);
			AssertEquals(new ZDecimal(0), packlineUNDG.DI_DGWeight);
			AssertEquals(new ZDecimal(1), packlineUNDG.DI_DGFlashPoint);
			AssertEquals(ZString.Empty, packlineUNDG.DI_PackingInstructionSection);

			var helper = GetShipmentDataObjectSplitPackageHelper(shipmentDataObject, shipment, warehouse1AddressBO);
			helper.ProcessTransitReceive();
			AssertEquals("No discrepancy in package attributes", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, forwardingPackLine.JL_OriginTransitWarehouseStatus);
			packlineUNDG = forwardingPackLine.UNDGs.FirstOrDefault();
			AssertEquals("UnitOfVolume was updated", Core.Constants.Volume.CubicMetres, packlineUNDG.DI_UnitOfVolume);
			AssertEquals("Volume was updated", new ZDecimal(3), packlineUNDG.DI_DGVolume);
			AssertEquals("UnitOfWeight was updated", Core.Constants.Weight.Kilograms, packlineUNDG.DI_UnitOfWeight);
			AssertEquals("Weight was updated", new ZDecimal(1), packlineUNDG.DI_DGWeight);
			AssertEquals("FlashPoint was updated", new ZDecimal(2), packlineUNDG.DI_DGFlashPoint);
			AssertEquals("PI Section was updated", packageUNDG.PackingInstructionSection, packlineUNDG.DI_PackingInstructionSection);

			var secondPackageUNDG = new UNDG();
			secondPackageUNDG.UNDGCode = "1234";
			secondPackageUNDG.IMOClass = "1";
			secondPackageUNDG.FlashPoint = "2.00";
			secondPackageUNDG.PackQty = 1;
			secondPackageUNDG.Volume = 3;
			secondPackageUNDG.VolumeUQ = new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres };
			secondPackageUNDG.Weight = 1;
			secondPackageUNDG.WeightUQ = new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms };
			secondPackageUNDG.PackingInstructionSection = "IA";

			packline.SetUNDGCollection(() => new List<UNDG>() { packageUNDG, secondPackageUNDG });

			helper.ProcessTransitReceive();

			AssertEquals(2, forwardingPackLine.UNDGs.Count);

			AssertEquals("PM", forwardingPackLine.UNDGs[0].DI_PackingInstructionSection);
			AssertEquals("IA", forwardingPackLine.UNDGs[1].DI_PackingInstructionSection);

			secondPackageUNDG.PackingInstructionSection = "PM";
			packline.SetUNDGCollection(() => new List<UNDG>() { packageUNDG, secondPackageUNDG });

			helper.ProcessTransitReceive();
			AssertEquals(1, forwardingPackLine.UNDGs.Count);
			AssertEquals("PM", forwardingPackLine.UNDGs[0].DI_PackingInstructionSection);
		}

		public void TestProcessTransitReceive_ForwardingPackLineGoodsDescriptionHasNewLine()
		{
			var warehouse1AddressBO = Factory.NewWithValidTestData<OrgAddress>();
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.DataContext = DataContextFactory.New();
			shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitReceive, "RC00000001");
			shipmentDataObject.WayBillNumber = "BACON PANCAKES";
			shipmentDataObject.WayBillType = new UniversalWayBillType { Code = WayBillTypeList.Codes.House };

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<UniversalOrganizationAddress> { new UniversalOrganizationAddress()
			{
				AddressShortCode = warehouse1AddressBO.AddressCode,
				AddressType = nameof(DocAddressType.LocalCartageCFS),
				OrganizationCode = warehouse1AddressBO.Header.OH_Code
			} });

			var packline = new UniversalPackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packline.ReferenceNumber = "S00001000-001";
			packline.PackQty = 1;
			packline.OutturnQty = 1;
			packline.UnloadDate = new ZDateTime(2020, 6, 1, 10, 6, 0);
			packline.GoodsDescription = "Goods 123";
			packline.PackingLineID = "TEST001";
			packline.PackType = new PackageType() { Code = Core.Constants.PkgUnit.Pallet };
			packline.LengthUnit = new UnitOfLength() { Code = Core.Constants.Length.Metres };
			packline.WeightUnit = new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms };
			packline.VolumeUnit = new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres };
			packline.SetReferenceNumberCollection(() => new List<UniversalReference>() { new UniversalReference { Type = new UniversalEntryType { Code = "TRU" }, ReferenceNumber = "TR00000001" } });
			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine> { packline });

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var forwardingPackLine = Factory.New<ForwardingPackLine>();
			forwardingPackLine.JL_PackageCount = 1;
			forwardingPackLine.JL_RefNumber = "REF001";
			forwardingPackLine.JL_PackLineId = "TEST001";
			forwardingPackLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			forwardingPackLine.JL_Description = "Goods\r\n123";
			forwardingPackLine.JL_FreightMode = FreightConstants.OuterPackType;
			shipment.OuterPackLines.Add(forwardingPackLine);

			var helper = GetShipmentDataObjectSplitPackageHelper(shipmentDataObject, shipment, warehouse1AddressBO);
			AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown, forwardingPackLine.JL_OriginTransitWarehouseStatus);
			helper.ProcessTransitReceive();
			AssertEquals("No discrepancy in package attributes", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, forwardingPackLine.JL_OriginTransitWarehouseStatus);

			shipmentDataObject.PackingLineCollection[0].GoodsDescription = "Goods 1234";
			helper.ProcessTransitReceive();
			AssertEquals("No discrepancy in package attributes", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies, forwardingPackLine.JL_OriginTransitWarehouseStatus);
		}

		public void TestProcessTransitReceive_NoPackageID_Qty1()
		{
			var warehouse1AddressBO = Factory.NewWithValidTestData<OrgAddress>();

			var shipmentDO = PrepareUniversalShipment(warehouse1AddressBO, DataContextType.TransitReceive);
			var packline = new UniversalPackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packline.ReferenceNumber = ZString.Empty;
			packline.PackingLineID = "TEST001";
			packline.PackQty = 1;
			packline.OutturnQty = 1;
			packline.UnloadDate = new ZDateTime(2020, 6, 1, 10, 6, 0);
			packline.GoodsDescription = "Goods 123";
			packline.PackType = new PackageType() { Code = Core.Constants.PkgUnit.Pallet };
			packline.LengthUnit = new UnitOfLength() { Code = Core.Constants.Length.Metres };
			packline.WeightUnit = new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms };
			packline.VolumeUnit = new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres };
			packline.SetReferenceNumberCollection(() => new List<UniversalReference>() { new UniversalReference { Type = new UniversalEntryType { Code = "TRU" }, ReferenceNumber = "TR00000001" } });
			shipmentDO.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine> { packline });

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var forwardingPackLine = Factory.New<ForwardingPackLine>();
			forwardingPackLine.JL_PackageCount = 1;
			forwardingPackLine.JL_RefNumber = ZString.Empty;
			forwardingPackLine.JL_PackLineId = "TEST001";
			forwardingPackLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			forwardingPackLine.JL_Description = "Goods 123";
			forwardingPackLine.JL_FreightMode = FreightConstants.OuterPackType;
			shipment.OuterPackLines.Add(forwardingPackLine);

			var helper = GetShipmentDataObjectSplitPackageHelper(shipmentDO, shipment, warehouse1AddressBO);
			AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown, forwardingPackLine.JL_OriginTransitWarehouseStatus);
			helper.ProcessTransitReceive();
			var currentPackeLineBO = shipment.OuterPackLines.Cast<ForwardingPackLine>().SingleOrDefault();
			AssertNotNull("Still has 1 Packline", currentPackeLineBO);
			AssertEquals("Same as original", forwardingPackLine, currentPackeLineBO);
			AssertEquals("No discrepancy in package attributes", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, forwardingPackLine.JL_OriginTransitWarehouseStatus);
			var package = currentPackeLineBO.PkgPackageCollection.SingleOrDefault();
			AssertPackage(package, "", "PLT", 1, 0, "KG", 0, "M3", "");
			AssertEquals("TEST001", package.KP_ExternalReference);
		}

		public void TestProcessTransitReceive_NoPackageID_Qty10()
		{
			var warehouse1AddressBO = Factory.NewWithValidTestData<OrgAddress>();

			var shipmentDO = PrepareUniversalShipment(warehouse1AddressBO, DataContextType.TransitReceive);
			var packline = new UniversalPackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packline.ReferenceNumber = ZString.Empty;
			packline.PackingLineID = "TEST001";
			packline.PackQty = 10;
			packline.OutturnQty = 10;
			packline.UnloadDate = new ZDateTime(2020, 6, 1, 10, 6, 0);
			packline.GoodsDescription = "Goods 123";
			packline.PackType = new PackageType() { Code = Core.Constants.PkgUnit.Pallet };
			packline.LengthUnit = new UnitOfLength() { Code = Core.Constants.Length.Metres };
			packline.WeightUnit = new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms };
			packline.VolumeUnit = new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres };
			packline.SetReferenceNumberCollection(() => new List<UniversalReference>() { new UniversalReference { Type = new UniversalEntryType { Code = "TRU" }, ReferenceNumber = "TR00000001" } });
			shipmentDO.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine> { packline });

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var forwardingPackLine = Factory.New<ForwardingPackLine>();
			forwardingPackLine.JL_PackageCount = 10;
			forwardingPackLine.JL_RefNumber = ZString.Empty;
			forwardingPackLine.JL_PackLineId = "TEST001";
			forwardingPackLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			forwardingPackLine.JL_Description = "Goods 123";
			forwardingPackLine.JL_FreightMode = FreightConstants.OuterPackType;
			shipment.OuterPackLines.Add(forwardingPackLine);

			var helper = GetShipmentDataObjectSplitPackageHelper(shipmentDO, shipment, warehouse1AddressBO);
			AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown, forwardingPackLine.JL_OriginTransitWarehouseStatus);
			helper.ProcessTransitReceive();
			var currentPackeLineBO = shipment.OuterPackLines.Cast<ForwardingPackLine>().SingleOrDefault();
			AssertNotNull("Still has 1 Packline", currentPackeLineBO);
			AssertEquals("Same as original", forwardingPackLine, currentPackeLineBO);
			AssertEquals("No discrepancy in package attributes", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, forwardingPackLine.JL_OriginTransitWarehouseStatus);
			var package = currentPackeLineBO.PkgPackageCollection.SingleOrDefault();
			AssertPackage(package, "", "PLT", 10, 0, "KG", 0, "M3", "");
			AssertEquals("TEST001", package.KP_ExternalReference);
		}

		public void TestProcessTransitReceive_NoPackageID_Qty7of10_Discrepencies()
		{
			var warehouse1AddressBO = Factory.NewWithValidTestData<OrgAddress>();

			var shipmentDO = PrepareUniversalShipment(warehouse1AddressBO, DataContextType.TransitReceive);
			var packline = new UniversalPackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packline.ReferenceNumber = ZString.Empty;
			packline.PackingLineID = "TEST001";
			packline.PackQty = 7;
			packline.OutturnQty = 7;
			packline.UnloadDate = new ZDateTime(2020, 6, 1, 10, 6, 0);
			packline.GoodsDescription = "Goods 123";
			packline.PackType = new PackageType() { Code = Core.Constants.PkgUnit.Pallet };
			packline.LengthUnit = new UnitOfLength() { Code = Core.Constants.Length.Metres };
			packline.WeightUnit = new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms };
			packline.VolumeUnit = new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres };
			packline.SetReferenceNumberCollection(() => new List<UniversalReference>() { new UniversalReference { Type = new UniversalEntryType { Code = "TRU" }, ReferenceNumber = "TR00000001" } });
			shipmentDO.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine> { packline });

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var forwardingPackLine = Factory.New<ForwardingPackLine>();
			forwardingPackLine.JL_PackageCount = 10;
			forwardingPackLine.JL_RefNumber = ZString.Empty;
			forwardingPackLine.JL_PackLineId = "TEST001";
			forwardingPackLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			forwardingPackLine.JL_Description = "Goods 123";
			forwardingPackLine.JL_FreightMode = FreightConstants.OuterPackType;
			shipment.OuterPackLines.Add(forwardingPackLine);

			var helper = GetShipmentDataObjectSplitPackageHelper(shipmentDO, shipment, warehouse1AddressBO);
			AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown, forwardingPackLine.JL_OriginTransitWarehouseStatus);
			helper.ProcessTransitReceive();
			var currentPackeLineBO = shipment.OuterPackLines.Cast<ForwardingPackLine>().SingleOrDefault();
			AssertNotNull("Still has 1 Packline", currentPackeLineBO);
			AssertEquals("Same as original", forwardingPackLine, currentPackeLineBO);
			AssertEquals("Discrepancy in package attributes", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies, forwardingPackLine.JL_OriginTransitWarehouseStatus);
			var package = currentPackeLineBO.PkgPackageCollection.SingleOrDefault();
			AssertPackage(package, "", "PLT", 7, 0, "KG", 0, "M3", "");
			AssertEquals("TEST001", package.KP_ExternalReference);
		}

		public void TestProcessTransitReceive_NoPackageID_Qty7of10_WithQty3Surplus_Discrepencies()
		{
			var warehouse1AddressBO = Factory.NewWithValidTestData<OrgAddress>();

			var shipmentDO = PrepareUniversalShipment(warehouse1AddressBO, DataContextType.TransitReceive);
			var packline = new UniversalPackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packline.ReferenceNumber = ZString.Empty;
			packline.PackingLineID = "TEST001";
			packline.PackQty = 7;
			packline.OutturnQty = 7;
			packline.UnloadDate = new ZDateTime(2020, 6, 1, 10, 6, 0);
			packline.GoodsDescription = "Goods 123";
			packline.PackType = new PackageType() { Code = Core.Constants.PkgUnit.Pallet };
			packline.LengthUnit = new UnitOfLength() { Code = Core.Constants.Length.Metres };
			packline.WeightUnit = new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms };
			packline.VolumeUnit = new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres };
			packline.SetReferenceNumberCollection(() => new List<UniversalReference>() { new UniversalReference { Type = new UniversalEntryType { Code = "TRU" }, ReferenceNumber = "TR00000001" } });
			var surplusPackline = new UniversalPackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			surplusPackline.ReferenceNumber = ZString.Empty;
			surplusPackline.PackingLineID = "RC00001000-002";
			surplusPackline.PackQty = 3;
			surplusPackline.OutturnQty = 3;
			surplusPackline.UnloadDate = new ZDateTime(2020, 6, 1, 10, 6, 0);
			surplusPackline.GoodsDescription = "Goods 123";
			surplusPackline.PackType = new PackageType() { Code = Core.Constants.PkgUnit.Pallet };
			surplusPackline.LengthUnit = new UnitOfLength() { Code = Core.Constants.Length.Metres };
			surplusPackline.WeightUnit = new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms };
			surplusPackline.VolumeUnit = new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres };
			surplusPackline.SetReferenceNumberCollection(() => new List<UniversalReference>() { new UniversalReference { Type = new UniversalEntryType { Code = "TRU" }, ReferenceNumber = "TR00000001" } });
			shipmentDO.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine> { packline, surplusPackline });

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var forwardingPackLine = Factory.New<ForwardingPackLine>();
			forwardingPackLine.JL_PackageCount = 10;
			forwardingPackLine.JL_RefNumber = ZString.Empty;
			forwardingPackLine.JL_PackLineId = "TEST001";
			forwardingPackLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			forwardingPackLine.JL_Description = "Goods 123";
			forwardingPackLine.JL_FreightMode = FreightConstants.OuterPackType;
			shipment.OuterPackLines.Add(forwardingPackLine);

			var helper = GetShipmentDataObjectSplitPackageHelper(shipmentDO, shipment, warehouse1AddressBO);
			AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown, forwardingPackLine.JL_OriginTransitWarehouseStatus);
			helper.ProcessTransitReceive();
			AssertEquals("A new surplus PL should have been added", 2, shipment.OuterPackLines.Count);
			var originalPackLineBO = shipment.OuterPackLines.Cast<ForwardingPackLine>().SingleOrDefault(p => p.JL_PackLineId == "TEST001");
			var surplusPackLineBO = shipment.OuterPackLines.Cast<ForwardingPackLine>().SingleOrDefault(p => p.JL_PackLineId == "RC00001000-002");
			AssertNotNull("Still has original Packline", originalPackLineBO);
			AssertNotNull("Has new surplus Packline", surplusPackLineBO);
			AssertEquals("Same as original", forwardingPackLine, originalPackLineBO);
			AssertEquals("Discrepancy", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies, originalPackLineBO.JL_OriginTransitWarehouseStatus);
			AssertEquals("Surplus", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Surplus, surplusPackLineBO.JL_OriginTransitWarehouseStatus);
			var packageQty7 = originalPackLineBO.PkgPackageCollection.SingleOrDefault();
			var packageQty3 = surplusPackLineBO.PkgPackageCollection.SingleOrDefault();
			AssertPackage(packageQty7, "", "PLT", 7, 0, "KG", 0, "M3", "");
			AssertPackage(packageQty3, "", "PLT", 3, 0, "KG", 0, "M3", "");
		}

		public void TestProcessTransitReceive_NoPackageID_Qty10_ProcessReceiveAgain()
		{
			var warehouse1AddressBO = Factory.NewWithValidTestData<OrgAddress>();
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.DataContext = DataContextFactory.New();
			shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitReceive, "RC00000001");
			shipmentDataObject.WayBillNumber = "BACON PANCAKES";
			shipmentDataObject.WayBillType = new UniversalWayBillType { Code = WayBillTypeList.Codes.House };

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<UniversalOrganizationAddress> { new UniversalOrganizationAddress()
			{
				AddressShortCode = warehouse1AddressBO.AddressCode,
				AddressType = nameof(DocAddressType.LocalCartageCFS),
				OrganizationCode = warehouse1AddressBO.Header.OH_Code
			} });

			var packline = new UniversalPackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packline.ReferenceNumber = ZString.Empty;
			packline.PackingLineID = "TEST001";
			packline.PackQty = 10;
			packline.OutturnQty = 10;
			packline.UnloadDate = new ZDateTime(2020, 6, 1, 10, 6, 0);
			packline.GoodsDescription = "Goods 123";
			packline.PackType = new PackageType() { Code = Core.Constants.PkgUnit.Pallet };
			packline.LengthUnit = new UnitOfLength() { Code = Core.Constants.Length.Metres };
			packline.WeightUnit = new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms };
			packline.VolumeUnit = new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres };
			packline.SetReferenceNumberCollection(() => new List<UniversalReference>() { new UniversalReference { Type = new UniversalEntryType { Code = "TRU" }, ReferenceNumber = "TR00000001" } });
			var packingLineCollection = new DataObjectList<UniversalPackingLine> { packline };
			packingLineCollection.Content = CollectionContent.Complete;
			shipmentDataObject.SetPackingLineCollection(() => packingLineCollection);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var forwardingPackLine = Factory.New<ForwardingPackLine>();
			forwardingPackLine.JL_PackageCount = 10;
			forwardingPackLine.JL_RefNumber = ZString.Empty;
			forwardingPackLine.JL_PackLineId = "TEST001";
			forwardingPackLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			forwardingPackLine.JL_Description = "Goods 123";
			forwardingPackLine.JL_FreightMode = FreightConstants.OuterPackType;
			shipment.OuterPackLines.Add(forwardingPackLine);

			var helper = GetShipmentDataObjectSplitPackageHelper(shipmentDataObject, shipment, warehouse1AddressBO);
			AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown, forwardingPackLine.JL_OriginTransitWarehouseStatus);
			helper.ProcessTransitReceive();
			var currentPackeLineBO = shipment.OuterPackLines.Cast<ForwardingPackLine>().SingleOrDefault();
			AssertNotNull("Still has 1 Packline", currentPackeLineBO);
			AssertEquals("Same as original", forwardingPackLine, currentPackeLineBO);
			AssertEquals("No discrepancy in package attributes", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, forwardingPackLine.JL_OriginTransitWarehouseStatus);
			var package = currentPackeLineBO.PkgPackageCollection.SingleOrDefault();
			AssertPackage(package, "", "PLT", 10, 0, "KG", 0, "M3", "");
			AssertEquals("TEST001", package.KP_ExternalReference);

			Factory.SaveForTesting();
			helper.ProcessTransitReceive();
			currentPackeLineBO = shipment.OuterPackLines.Cast<ForwardingPackLine>().SingleOrDefault();
			AssertNotNull("Still has 1 Packline", currentPackeLineBO);
			AssertEquals("Same as original", forwardingPackLine, currentPackeLineBO);
			AssertEquals("No discrepancy in package attributes", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, forwardingPackLine.JL_OriginTransitWarehouseStatus);
			package = currentPackeLineBO.PkgPackageCollection.SingleOrDefault();
			AssertPackage(package, "", "PLT", 10, 0, "KG", 0, "M3", "");
			AssertEquals("TEST001", package.KP_ExternalReference);
		}

		public void TestProcessTransitReceive_NoPackageID_Qty10_ProcessReceiveAgainWithPackageIDs()
		{
			var warehouse1AddressBO = Factory.NewWithValidTestData<OrgAddress>();

			var shipmentDO = PrepareUniversalShipment(warehouse1AddressBO, DataContextType.TransitReceive);
			var packline = new UniversalPackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packline.ReferenceNumber = ZString.Empty;
			packline.PackingLineID = "TEST001";
			packline.PackQty = 3;
			packline.OutturnQty = 3;
			packline.UnloadDate = new ZDateTime(2020, 6, 1, 10, 6, 0);
			packline.GoodsDescription = "Goods 123";
			packline.PackType = new PackageType() { Code = Core.Constants.PkgUnit.Pallet };
			packline.LengthUnit = new UnitOfLength() { Code = Core.Constants.Length.Metres };
			packline.WeightUnit = new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms };
			packline.VolumeUnit = new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres };
			packline.SetReferenceNumberCollection(() => new List<UniversalReference>() { new UniversalReference { Type = new UniversalEntryType { Code = "TRU" }, ReferenceNumber = "TR00000001" } });
			var packingLineCollection = new DataObjectList<UniversalPackingLine>();
			packingLineCollection.Content = CollectionContent.Complete;
			packingLineCollection.Add(packline);
			shipmentDO.SetPackingLineCollection(() => packingLineCollection);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_OA_ExportReceivingDepot = warehouse1AddressBO.PK;
			var forwardingPackLine = Factory.New<ForwardingPackLine>();
			forwardingPackLine.JL_PackageCount = 3;
			forwardingPackLine.JL_RefNumber = ZString.Empty;
			forwardingPackLine.JL_PackLineId = "TEST001";
			forwardingPackLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			forwardingPackLine.JL_Description = "Goods 123";
			forwardingPackLine.JL_FreightMode = FreightConstants.OuterPackType;
			shipment.OuterPackLines.Add(forwardingPackLine);

			var helper = GetShipmentDataObjectSplitPackageHelper(shipmentDO, shipment, warehouse1AddressBO);
			AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown, forwardingPackLine.JL_OriginTransitWarehouseStatus);
			helper.ProcessTransitReceive();
			var currentPackeLineBO = shipment.OuterPackLines.Cast<ForwardingPackLine>().SingleOrDefault();
			AssertNotNull("Still has 1 Packline", currentPackeLineBO);
			AssertEquals("Same as original", forwardingPackLine, currentPackeLineBO);
			AssertEquals("No discrepancy in package attributes", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, currentPackeLineBO.JL_OriginTransitWarehouseStatus);
			var package = currentPackeLineBO.PkgPackageCollection.SingleOrDefault();
			AssertPackage(package, "", "PLT", 3, 0, "KG", 0, "M3", "");
			AssertEquals("TEST001", package.KP_ExternalReference);

			Factory.SaveForTesting();
			shipmentDO = PrepareUniversalShipment(warehouse1AddressBO, DataContextType.TransitReceive);
			var packingLineCollection1 = new DataObjectList<UniversalPackingLine>();
			for (var i = 1; i <= 3; i++)
			{
				packline = new UniversalPackingLine(DefaultDataObjectWriterStrategy.TestInstance);
				packline.ReferenceNumber = "PKG" + i;
				packline.PackingLineID = "TEST001";
				packline.PackQty = 1;
				packline.OutturnQty = 1;
				packline.UnloadDate = new ZDateTime(2020, 6, 1, 10, 7, 0);
				packline.GoodsDescription = "Goods 123";
				packline.PackType = new PackageType() { Code = Core.Constants.PkgUnit.Pallet };
				packline.LengthUnit = new UnitOfLength() { Code = Core.Constants.Length.Metres };
				packline.WeightUnit = new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms };
				packline.VolumeUnit = new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres };
				packline.SetReferenceNumberCollection(() => new List<UniversalReference>() { new UniversalReference { Type = new UniversalEntryType { Code = "TRU" }, ReferenceNumber = "TR00000001" } });
				packingLineCollection1.Add(packline);
			}

			packingLineCollection1.Content = CollectionContent.Complete;
			shipmentDO.SetPackingLineCollection(() => packingLineCollection1);
			helper = GetShipmentDataObjectSplitPackageHelper(shipmentDO, shipment, warehouse1AddressBO);
			helper.ProcessTransitReceive();

			AssertEquals("There is 1 Packline", 1, shipment.OuterPackLines.Count);
			currentPackeLineBO = shipment.OuterPackLines.Cast<ForwardingPackLine>().SingleOrDefault(p => p.JL_PackLineId == "TEST001");
			AssertNotNull("The original Packline exists", forwardingPackLine);
			AssertEquals("Same as original", forwardingPackLine, currentPackeLineBO);
			AssertPackLineInformations(currentPackeLineBO,
				warehouse1AddressBO.PK,
				FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
				new ZDateTime(2020, 6, 1, 10, 7, 0),
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
				new ZString[] { "PKG1", "PKG2", "PKG3" });
		}

		public void TestProcessTransitReceive_WithPackageIDs_ProcessReceiveAgainWithPackageIDs()
		{
			var warehouse1AddressBO = Factory.NewWithValidTestData<OrgAddress>();

			var shipmentDO = PrepareUniversalShipment(warehouse1AddressBO, DataContextType.TransitReceive);
			var packingLineCollection = new DataObjectList<UniversalPackingLine>();
			packingLineCollection.Content = CollectionContent.Complete;
			for (var i = 1; i <= 3; i++)
			{
				var packline = new UniversalPackingLine(DefaultDataObjectWriterStrategy.TestInstance);
				packline.ReferenceNumber = "PKG" + i;
				packline.PackingLineID = "TEST001";
				packline.PackQty = 1;
				packline.OutturnQty = 1;
				packline.UnloadDate = new ZDateTime(2020, 6, 1, 10, 7, 0);
				packline.GoodsDescription = "Goods 123";
				packline.PackType = new PackageType() { Code = Core.Constants.PkgUnit.Pallet };
				packline.LengthUnit = new UnitOfLength() { Code = Core.Constants.Length.Metres };
				packline.WeightUnit = new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms };
				packline.VolumeUnit = new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres };
				packline.SetReferenceNumberCollection(() => new List<UniversalReference>() { new UniversalReference { Type = new UniversalEntryType { Code = "TRU" }, ReferenceNumber = "TR00000001" } });
				packingLineCollection.Add(packline);
			}

			shipmentDO.SetPackingLineCollection(() => packingLineCollection);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_OA_ExportReceivingDepot = warehouse1AddressBO.PK;
			var forwardingPackLine = Factory.New<ForwardingPackLine>();
			forwardingPackLine.JL_PackageCount = 3;
			forwardingPackLine.JL_RefNumber = ZString.Empty;
			forwardingPackLine.JL_PackLineId = "TEST001";
			forwardingPackLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			forwardingPackLine.JL_Description = "Goods 123";
			forwardingPackLine.JL_FreightMode = FreightConstants.OuterPackType;
			shipment.OuterPackLines.Add(forwardingPackLine);

			var helper = GetShipmentDataObjectSplitPackageHelper(shipmentDO, shipment, warehouse1AddressBO);
			AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown, forwardingPackLine.JL_OriginTransitWarehouseStatus);
			helper.ProcessTransitReceive();
			var currentPackeLineBO = shipment.OuterPackLines.Cast<ForwardingPackLine>().SingleOrDefault();
			AssertNotNull("Still has 1 Packline", currentPackeLineBO);

			AssertPackLineInformations(currentPackeLineBO,
				warehouse1AddressBO.PK,
				FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
				new ZDateTime(2020, 6, 1, 10, 7, 0),
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
				new ZString[] { "PKG1", "PKG2", "PKG3" });

			Factory.SaveForTesting();
			shipmentDO = PrepareUniversalShipment(warehouse1AddressBO, DataContextType.TransitReceive);
			var packingLineCollection1 = new DataObjectList<UniversalPackingLine>();
			for (var i = 1; i <= 3; i++)
			{
				var packline = new UniversalPackingLine(DefaultDataObjectWriterStrategy.TestInstance);
				packline.ReferenceNumber = "CTN" + i;
				packline.PackingLineID = "TEST001";
				packline.PackQty = 1;
				packline.OutturnQty = 1;
				packline.UnloadDate = new ZDateTime(2020, 6, 1, 10, 7, 0);
				packline.GoodsDescription = "Goods 123";
				packline.PackType = new PackageType() { Code = Core.Constants.PkgUnit.Pallet };
				packline.LengthUnit = new UnitOfLength() { Code = Core.Constants.Length.Metres };
				packline.WeightUnit = new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms };
				packline.VolumeUnit = new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres };
				packline.SetReferenceNumberCollection(() => new List<UniversalReference>() { new UniversalReference { Type = new UniversalEntryType { Code = "TRU" }, ReferenceNumber = "TR00000001" } });
				packingLineCollection1.Add(packline);
			}

			packingLineCollection1.Content = CollectionContent.Complete;
			shipmentDO.SetPackingLineCollection(() => packingLineCollection1);
			helper = GetShipmentDataObjectSplitPackageHelper(shipmentDO, shipment, warehouse1AddressBO);
			helper.ProcessTransitReceive();

			currentPackeLineBO = shipment.OuterPackLines.Cast<ForwardingPackLine>().SingleOrDefault();
			AssertNotNull("Still has 1 Packline", currentPackeLineBO);
			AssertNotNull("The original Packline exists", forwardingPackLine);
			AssertEquals("Same as original", forwardingPackLine, currentPackeLineBO);
			AssertPackLineInformations(currentPackeLineBO,
				warehouse1AddressBO.PK,
				FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
				new ZDateTime(2020, 6, 1, 10, 7, 0),
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
				new ZString[] { "CTN1", "CTN2", "CTN3" });
		}

		void AssertPackLineInformations(ForwardingPackLine packLine, ZGuid warehouseAddressPK, ZString lastKnownTransitWarehouseStatus, ZDateTime lastKnownTransitWarehouseStatusDateTime, ZString originTransitWarehouseStatus, ZString[] referenceNumbers)
		{
			AssertArrayEqualsByElements(referenceNumbers, packLine.PkgPackageCollection.Select(p => p.KP_PackageID).ToArray());
			AssertEquals("JL_OA_LastKnownTransitWarehouseAddress", warehouseAddressPK, packLine.JL_OA_LastKnownTransitWarehouseAddress);
			AssertEquals("JL_OriginTransitWarehouseStatus", originTransitWarehouseStatus, packLine.JL_OriginTransitWarehouseStatus);
			AssertEquals("JL_LastKnownTransitWarehouseStatus", lastKnownTransitWarehouseStatus, packLine.JL_LastKnownTransitWarehouseStatus);
			AssertEquals("JL_LastKnownTransitWarehouseStatusDateTime", lastKnownTransitWarehouseStatusDateTime, packLine.JL_LastKnownTransitWarehouseStatusDateTime);
		}

		UniversalShipment PrepareUniversalShipment(OrgAddress warehouseAddressBO, DataContextType dataContextType)
		{
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.DataContext = DataContextFactory.New();
			shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			switch (dataContextType)
			{
				case DataContextType.TransitReceive:
					shipmentDataObject.DataContext.AddDataSource(dataContextType, "RC00000001");
					break;
				case DataContextType.TransitDispatch:
					shipmentDataObject.DataContext.AddDataSource(dataContextType, "DC00000001");
					break;
				default:
					throw new ArgumentException($"Unsupported DataContextType: {nameof(dataContextType)}");
			}
			shipmentDataObject.WayBillNumber = "BACON PANCAKES";
			shipmentDataObject.WayBillType = new UniversalWayBillType { Code = WayBillTypeList.Codes.House };

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<UniversalOrganizationAddress> { new UniversalOrganizationAddress()
			{
				AddressShortCode = warehouseAddressBO.AddressCode,
				AddressType = nameof(DocAddressType.LocalCartageCFS),
				OrganizationCode = warehouseAddressBO.Header.OH_Code
			} });
			return shipmentDataObject;
		}

		public void TestProcessTransitReceive_NoPackageID_Qty10_ProcessDispatchWithNoPackageIDs()
		{
			var warehouse1AddressBO = Factory.NewWithValidTestData<OrgAddress>();
			var shipmentDO = PrepareUniversalShipment(warehouse1AddressBO, DataContextType.TransitReceive);

			var packline = new UniversalPackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packline.ReferenceNumber = ZString.Empty;
			packline.PackingLineID = "TEST001";
			packline.PackQty = 10;
			packline.OutturnQty = 10;
			packline.UnloadDate = new ZDateTime(2020, 6, 1, 10, 6, 0);
			packline.GoodsDescription = "Goods 123";
			packline.PackType = new PackageType() { Code = Core.Constants.PkgUnit.Pallet };
			packline.LengthUnit = new UnitOfLength() { Code = Core.Constants.Length.Metres };
			packline.WeightUnit = new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms };
			packline.VolumeUnit = new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres };
			packline.SetReferenceNumberCollection(() => new List<UniversalReference>() { new UniversalReference { Type = new UniversalEntryType { Code = "TRU" }, ReferenceNumber = "TR00000001" } });
			var packingLineCollection = new DataObjectList<UniversalPackingLine> { packline };
			packingLineCollection.Content = CollectionContent.Complete;
			shipmentDO.SetPackingLineCollection(() => packingLineCollection);

			var shipmentBO = Factory.NewWithValidTestData<ForwardingShipment>();
			var forwardingPackLine = Factory.New<ForwardingPackLine>();
			forwardingPackLine.JL_PackageCount = 10;
			forwardingPackLine.JL_RefNumber = ZString.Empty;
			forwardingPackLine.JL_PackLineId = "TEST001";
			forwardingPackLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			forwardingPackLine.JL_Description = "Goods 123";
			forwardingPackLine.JL_FreightMode = FreightConstants.OuterPackType;
			shipmentBO.OuterPackLines.Add(forwardingPackLine);

			var helper = GetShipmentDataObjectSplitPackageHelper(shipmentDO, shipmentBO, warehouse1AddressBO);
			AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown, forwardingPackLine.JL_OriginTransitWarehouseStatus);
			helper.ProcessTransitReceive();
			var outerPackLine = shipmentBO.OuterPackLines.Cast<ForwardingPackLine>().SingleOrDefault();
			AssertNotNull("Still has 1 Packline", outerPackLine);
			AssertEquals("Same as original", forwardingPackLine, outerPackLine);
			AssertEquals("No discrepancy in package attributes", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, forwardingPackLine.JL_OriginTransitWarehouseStatus);
			var package = outerPackLine.PkgPackageCollection.SingleOrDefault();
			AssertPackage(package, "", Core.Constants.PkgUnit.Pallet, 10, 0, "KG", 0, "M3", "");
			AssertEquals("TEST001", package.KP_ExternalReference);

			Factory.SaveForTesting();
			shipmentDO = PrepareUniversalShipment(warehouse1AddressBO, DataContextType.TransitDispatch);
			var packingLineCollection1 = new DataObjectList<UniversalPackingLine>();
			packline = new UniversalPackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packline.ReferenceNumber = ZString.Empty;
			packline.PackingLineID = "TEST001";
			packline.PackQty = 10;
			packline.OutturnQty = 10;
			packline.LoadDate = new ZDateTime(2020, 6, 1, 10, 7, 0);
			packline.GoodsDescription = "Goods 123";
			packline.PackType = new PackageType() { Code = Core.Constants.PkgUnit.Pallet };
			packline.LengthUnit = new UnitOfLength() { Code = Core.Constants.Length.Metres };
			packline.WeightUnit = new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms };
			packline.VolumeUnit = new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres };
			packline.SetReferenceNumberCollection(() => new List<UniversalReference>() { new UniversalReference { Type = new UniversalEntryType { Code = "TRU" }, ReferenceNumber = "TR00000001" } });
			packingLineCollection1.Add(packline);
			packingLineCollection1.Content = CollectionContent.Complete;
			shipmentDO.SetPackingLineCollection(() => packingLineCollection1);

			helper = GetShipmentDataObjectSplitPackageHelper(shipmentDO, shipmentBO, warehouse1AddressBO);
			helper.ProcessTransitDispatch();

			outerPackLine = shipmentBO.OuterPackLines.Cast<ForwardingPackLine>().SingleOrDefault();
			AssertNotNull("Still has 1 Packline", outerPackLine);
			AssertEquals("Same as original", forwardingPackLine, outerPackLine);
			AssertEquals("Packline Status has been updated", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, outerPackLine.JL_OriginTransitWarehouseStatus);
			package = outerPackLine.PkgPackageCollection.SingleOrDefault();
			AssertPackage(package, "", Core.Constants.PkgUnit.Pallet, 10, 0, "KG", 0, "M3", "");
			AssertEquals("TEST001", package.KP_ExternalReference);
		}

		public void TestProcessTransitReceive_NoPackageID_Qty10_ProcessDispatchWithPackageIDs()
		{
			var warehouse1AddressBO = Factory.NewWithValidTestData<OrgAddress>();
			var shipmentDO = PrepareUniversalShipment(warehouse1AddressBO, DataContextType.TransitReceive);

			var packline = new UniversalPackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packline.ReferenceNumber = ZString.Empty;
			packline.PackingLineID = "TEST001";
			packline.PackQty = 3;
			packline.OutturnQty = 3;
			packline.UnloadDate = new ZDateTime(2020, 6, 1, 10, 6, 0);
			packline.GoodsDescription = "Goods 123";
			packline.PackType = new PackageType() { Code = Core.Constants.PkgUnit.Pallet };
			packline.LengthUnit = new UnitOfLength() { Code = Core.Constants.Length.Metres };
			packline.WeightUnit = new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms };
			packline.VolumeUnit = new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres };
			packline.SetReferenceNumberCollection(() => new List<UniversalReference>() { new UniversalReference { Type = new UniversalEntryType { Code = "TRU" }, ReferenceNumber = "TR00000001" } });
			var packlineCollection = new DataObjectList<UniversalPackingLine> { packline };
			packlineCollection.Content = CollectionContent.Complete;
			shipmentDO.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine> { packline });

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var forwardingPackLine = Factory.New<ForwardingPackLine>();
			forwardingPackLine.JL_PackageCount = 3;
			forwardingPackLine.JL_RefNumber = ZString.Empty;
			forwardingPackLine.JL_PackLineId = "TEST001";
			forwardingPackLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			forwardingPackLine.JL_Description = "Goods 123";
			forwardingPackLine.JL_FreightMode = FreightConstants.OuterPackType;
			shipment.OuterPackLines.Add(forwardingPackLine);

			var helper = GetShipmentDataObjectSplitPackageHelper(shipmentDO, shipment, warehouse1AddressBO);
			AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown, forwardingPackLine.JL_OriginTransitWarehouseStatus);
			helper.ProcessTransitReceive();
			var outerPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().SingleOrDefault();
			AssertNotNull("Still has 1 Packline", outerPackLine);
			AssertEquals("Same as original", forwardingPackLine, outerPackLine);
			AssertEquals("No discrepancy in package attributes", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, forwardingPackLine.JL_OriginTransitWarehouseStatus);
			var package = outerPackLine.PkgPackageCollection.SingleOrDefault();
			AssertPackage(package, "", "PLT", 3, 0, "KG", 0, "M3", "");
			AssertEquals("TEST001", package.KP_ExternalReference);

			Factory.SaveForTesting();
			shipmentDO = PrepareUniversalShipment(warehouse1AddressBO, DataContextType.TransitDispatch);
			var packingLineCollection1 = new DataObjectList<UniversalPackingLine>();
			for (var i = 1; i <= 3; i++)
			{
				packline = new UniversalPackingLine(DefaultDataObjectWriterStrategy.TestInstance);
				packline.ReferenceNumber = "PKG" + i;
				packline.PackingLineID = "TEST001";
				packline.PackQty = 1;
				packline.OutturnQty = 1;
				packline.LoadDate = new ZDateTime(2020, 6, 1, 10, 7, 0);
				packline.GoodsDescription = "Goods 123";
				packline.PackType = new PackageType() { Code = Core.Constants.PkgUnit.Pallet };
				packline.LengthUnit = new UnitOfLength() { Code = Core.Constants.Length.Metres };
				packline.WeightUnit = new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms };
				packline.VolumeUnit = new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres };
				packline.SetReferenceNumberCollection(() => new List<UniversalReference>() { new UniversalReference { Type = new UniversalEntryType { Code = "TRU" }, ReferenceNumber = "TR00000001" } });
				packingLineCollection1.Add(packline);
			}
			packingLineCollection1.Content = CollectionContent.Complete;
			shipmentDO.SetPackingLineCollection(() => packingLineCollection1);
			helper = GetShipmentDataObjectSplitPackageHelper(shipmentDO, shipment, warehouse1AddressBO);
			helper.ProcessTransitDispatch();

			AssertEquals("There is 1 Packline", 1, shipment.OuterPackLines.Count);
			var currentPackline = shipment.OuterPackLines.Cast<ForwardingPackLine>().SingleOrDefault(p => p.JL_PackLineId == "TEST001");
			AssertNotNull("The original Packline exists", forwardingPackLine);
			AssertEquals("Same as original", forwardingPackLine, currentPackline);
			AssertPackLineInformations(currentPackline,
				warehouse1AddressBO.PK,
				FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched,
				new ZDateTime(2020, 6, 1, 10, 7, 0),
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
				new ZString[] { "PKG1", "PKG2", "PKG3" });
		}

		public void TestProcessTransitReceive_NoPackageID_SameExternalReference_ReportPackageExistInAnyPacklines()
		{
			var warehouse1AddressBO = Factory.NewWithValidTestData<OrgAddress>();
			var shipmentDO = PrepareUniversalShipment(warehouse1AddressBO, DataContextType.TransitReceive);

			var packline = new UniversalPackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packline.ReferenceNumber = ZString.Empty;
			packline.PackingLineID = "TEST001";
			packline.PackQty = 10;
			packline.OutturnQty = 10;
			packline.UnloadDate = new ZDateTime(2020, 6, 1, 10, 6, 0);
			packline.GoodsDescription = "Goods 123";
			packline.PackType = new PackageType() { Code = Core.Constants.PkgUnit.Pallet };
			packline.LengthUnit = new UnitOfLength() { Code = Core.Constants.Length.Metres };
			packline.WeightUnit = new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms };
			packline.VolumeUnit = new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres };
			packline.SetReferenceNumberCollection(() => new List<UniversalReference>() { new UniversalReference { Type = new UniversalEntryType { Code = "TRU" }, ReferenceNumber = "TR00000001" } });
			var packingLineCollection = new DataObjectList<UniversalPackingLine> { packline };
			packingLineCollection.Content = CollectionContent.Complete;
			shipmentDO.SetPackingLineCollection(() => packingLineCollection);

			var shipmentBO = Factory.NewWithValidTestData<ForwardingShipment>();
			var forwardingPackLine = Factory.New<ForwardingPackLine>();
			forwardingPackLine.JL_PackageCount = 10;
			forwardingPackLine.JL_RefNumber = ZString.Empty;
			forwardingPackLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			forwardingPackLine.JL_Description = "Goods 123";
			forwardingPackLine.JL_FreightMode = FreightConstants.OuterPackType;
			shipmentBO.OuterPackLines.Add(forwardingPackLine);
			var package = forwardingPackLine.PkgPackageCollection.AddNew();
			package.KP_PackageID = ZString.Empty;
			package.KP_ExternalReference = "TEST001";

			var helper = GetShipmentDataObjectSplitPackageHelper(shipmentDO, shipmentBO, warehouse1AddressBO);
			AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown, forwardingPackLine.JL_OriginTransitWarehouseStatus);
			helper.ProcessTransitReceive();

			AssertEquals("Package exist in any PackLines", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestProcessTransitReceive_SecurityModifiedEvent_InspectionStatus()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var forwardingPackLine = CreatePackLine("REF001", "TEST001", Core.Constants.PkgUnit.Pallet, FreightConstants.OuterPackType, 1, 3000m, "KG", 300m, "M3", "M", "GEN");
				forwardingPackLine.JL_InspectionTypeCode = "CMD";

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "GBFXT";
				shipment.JS_RL_NKDestination = "AUSYD";
				shipment.JS_InspectionTypeCode = "CMD";
				shipment.OuterPackLines.Add(forwardingPackLine);

				Factory.SaveForTesting();

				var transportationUnits = new List<(ZString Reference, ZDateTime LastKnownTime)> { ("TR00000001", new ZDateTime(2020, 6, 1, 10, 17, 0)) };
				var (warehouse1AddressBO, shipmentDataObject) = SetupShipmentDataObject(DataContextType.TransitReceive, "RC00000001", transportationUnits, ZString.Empty);
				shipmentDataObject.TransportMode = new CodeDescriptionPair { Code = Core.Constants.TransportModes.Air };
				shipmentDataObject.PortOfOrigin = new UNLOCO { Code = "GBFXT" };
				shipmentDataObject.PortOfDischarge = new UNLOCO { Code = "AUSYD" };

				var packline = CreatePackingLine("S00001000-001", 1, Core.Constants.PkgUnit.Pallet, 3000m, "KG", 300m, "M3", "M", "TEST001", "GEN");
				packline.UnloadDate = new ZDateTime(2020, 6, 1, 10, 17, 0);
				packline.ScreeningMethod = "VCK";
				SetReferenceNumber(packline, "TRU", "TR00000001");
				shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine> { packline });

				var helper = GetShipmentDataObjectSplitPackageHelper(shipmentDataObject, shipment, warehouse1AddressBO);
				helper.ProcessTransitReceive();

				Factory.SaveForTesting();

				AssertEquals("JS_InspectionTypeCode has been updated", "VCK", shipment.JS_InspectionTypeCode);
				AssertEquals(1, shipment.OuterPackLines.Count);
				AssertEquals("JL_InspectionTypeCode has been updated", "VCK", shipment.OuterPackLines[0].JL_InspectionTypeCode);
				Assert(shipment.Logs.GetAllLogs().Cast<StmALog>().Any(l
					=> l.SL_SE_NKEvent == AutoEvents.SecurityModified.Code
					&& l.Parameters.TryGetValue(Params.Old, out var oldParam)
					&& oldParam == "CMD"
					&& l.Parameters.TryGetValue(Params.New, out var newParam)
					&& newParam == "VCK"
					&& l.Parameters.TryGetValue(Params.Reason, out var reasonParam)
					&& reasonParam == "Inspection at Shipment level Changed by Transit Warehouse package update"));
			}
		}

		public void TestProcessTransitReceive_ProcessDispatchedPackages()
		{
			var transportationUnits = new List<(ZString Reference, ZDateTime LastKnownTime)> { ("TR00000001", new ZDateTime(2020, 6, 1, 10, 17, 0)) };
			var (warehouse1AddressBO, shipmentDataObject) = SetupShipmentDataObject(DataContextType.TransitReceive, "RC00000001", transportationUnits, ZString.Empty);

			var packingLine1 = CreatePackingLine("S00001000-BOX1", 1, "BOX", 3000m, "KG", 300m, "M3", "M", "TEST001", "GEN");
			packingLine1.UnloadDate = new ZDateTime(2020, 6, 3, 10, 17, 0);
			packingLine1.LoadDate = ZDateTime.Empty;
			var packingLine2 = CreatePackingLine("S00001000-PLT1", 1, "PLT", 4000m, "KG", 400m, "M3", "M", "TEST002", "GEN");
			packingLine2.LoadDate = new ZDateTime(2020, 6, 4, 10, 17, 0);
			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine> { packingLine1, packingLine2 });

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = shipment.PK;
			var forwardingPackLine1 = CreatePackLine("REF001", "TEST001", "BOX", FreightConstants.OuterPackType, 1, 1000m, "KG", 100m, "M3", "M", "GEN");
			CreatePackages(
				forwardingPackLine1, 1, warehouse1AddressBO, new ZDateTime(2020, 6, 1, 10, 17, 0),
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
				FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
				"S00001000-BOX", packageJob);
			var forwardingPackLine2 = CreatePackLine("REF001", "TEST002", "PLT", FreightConstants.OuterPackType, 1, 2000m, "KG", 200m, "M3", "M", "GEN");
			CreatePackages(
				forwardingPackLine2, 1, warehouse1AddressBO, new ZDateTime(2020, 6, 2, 10, 17, 0),
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
				FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
				"S00001000-PLT", packageJob);
			shipment.OuterPackLines.Add(forwardingPackLine1);
			shipment.OuterPackLines.Add(forwardingPackLine2);

			var readingHelper = Process(DataContextType.TransitReceive, shipment, warehouse1AddressBO, shipmentDataObject);
			AssertEquals("2 outer packlines", 2, shipment.OuterPackLines.Count);

			var outerPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(l => l.JL_F3_NKPackType == "BOX");
			AssertPackLine(outerPackLine, 1, "BOX", FreightConstants.OuterPackType, 1000m, "KG", 100m, "M3", "M", "GEN", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies, new ZDateTime(2020, 6, 3, 10, 17, 0));
			AssertEquals("1 outer package", 1, outerPackLine.PkgPackageCollection.Count);
			var package = outerPackLine.PkgPackageCollection.FirstOrDefault(p => p.KP_PackageID == "S00001000-BOX1");
			AssertPackage(package, "S00001000-BOX1", "BOX", 1, 3000m, "KG", 300m, "M3", "GEN");

			outerPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(l => l.JL_F3_NKPackType == "PLT");
			AssertPackLine(outerPackLine, 1, "PLT", FreightConstants.OuterPackType, 2000m, "KG", 200m, "M3", "M", "GEN", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies, new ZDateTime(2020, 6, 4, 10, 17, 0));
			AssertEquals("1 outer package", 1, outerPackLine.PkgPackageCollection.Count);
			package = outerPackLine.PkgPackageCollection.FirstOrDefault(p => p.KP_PackageID == "S00001000-PLT1");
			AssertPackage(package, "S00001000-PLT1", "PLT", 1, 4000m, "KG", 400m, "M3", "GEN");
		}

		[ExpectNoExceptions]
		public void TestProcessTransitReceive_ProcessDispatchedPackages_IndexOutOfRange()
		{
			var transportationUnits = new List<(ZString Reference, ZDateTime LastKnownTime)> { ("TR00000001", new ZDateTime(2020, 6, 1, 10, 17, 0)) };
			var (warehouse1AddressBO, shipmentDataObject) = SetupShipmentDataObject(DataContextType.TransitReceive, "RC00000001", transportationUnits, ZString.Empty);

			var packingLine1 = CreatePackingLine("", 1, "BOX", 3000m, "KG", 300m, "M3", "M", "TEST001", "GEN");
			packingLine1.UnloadDate = new ZDateTime(2020, 6, 3, 10, 17, 0);
			packingLine1.LoadDate = ZDateTime.Empty;

			var packingLine2 = CreatePackingLine("", 1, "BOX", 3000m, "KG", 300m, "M3", "M", "TEST002", "GEN");
			packingLine2.UnloadDate = new ZDateTime(2020, 6, 3, 10, 17, 0);
			packingLine2.LoadDate = ZDateTime.Empty;
			packingLine2.PreviousPackingLineID = "TEST001";

			var packingLine3 = CreatePackingLine("", 1, "CTN", 3000m, "KG", 300m, "M3", "M", "TEST003", "GEN");
			packingLine3.UnloadDate = new ZDateTime(2020, 6, 3, 10, 17, 0);
			packingLine3.LoadDate = new ZDateTime(2020, 6, 3, 10, 17, 0);
			packingLine3.PreviousPackingLineID = "TEST001";
			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine> { packingLine1, packingLine2, packingLine3 });

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = shipment.PK;
			var forwardingPackLine = CreatePackLine("REF001", "TEST001", "BOX", FreightConstants.OuterPackType, 1, 1000m, "KG", 100m, "M3", "M", "GEN");
			CreatePackages(
				forwardingPackLine, 1, warehouse1AddressBO, new ZDateTime(2020, 6, 1, 10, 17, 0),
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
				FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
				"", packageJob);
			forwardingPackLine.PkgPackageCollection[0].KP_PackageID = ZString.Empty;
			forwardingPackLine.PkgPackageCollection[0].KP_ExternalReference = "TEST002";
			forwardingPackLine.PkgPackageCollection[0].KP_PreviousPackLineID = "TEST001";

			shipment.OuterPackLines.Add(forwardingPackLine);

			var readingHelper1 = Process(DataContextType.TransitReceive, shipment, warehouse1AddressBO, shipmentDataObject);
			AssertEquals("2 outer packlines", 2, shipment.OuterPackLines.Count);
		}

		public void TestProcessTransitReceive_ReceivePackagesDispatchedInOtherWarehouses()
		{
			var transportationUnits = new List<(ZString Reference, ZDateTime LastKnownTime)> { ("TR00000001", new ZDateTime(2020, 6, 3, 10, 17, 0)) };
			var (warehouse1AddressBO, shipmentDataObject) = SetupShipmentDataObject(DataContextType.TransitReceive, "RC00000001", transportationUnits, ZString.Empty);
			var warehouse2AddressBO = Factory.NewWithValidTestData<OrgAddress>();

			var packingLineCollection = new DataObjectList<UniversalPackingLine>();
			for (var index = 9; index <= 10; index++)
			{
				var packingLine = CreatePackingLine("S00001000-BOX" + index, 1, "BOX", 100m, "KG", 10m, "M3", "M", "TEST001", "GEN");
				packingLine.UnloadDate = new ZDateTime(2020, 6, 3, 10, 17, 0);
				packingLineCollection.Add(packingLine);
			}
			shipmentDataObject.SetPackingLineCollection(() => packingLineCollection);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = shipment.PK;
			var forwardingPackLine1 = CreatePackLine("REF001", "TEST001", "BOX", FreightConstants.OuterPackType, 4, 400m, "KG", 40m, "M3", "M", "GEN");
			CreatePackages(
				forwardingPackLine1, 4, warehouse2AddressBO, new ZDateTime(2020, 6, 1, 10, 17, 0),
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
				FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
				"S00001000-BOX", packageJob);
			var forwardingPackLine2 = CreatePackLine("REF002", "TEST002", "BOX", FreightConstants.OuterPackType, 6, 600m, "KG", 60m, "M3", "M", "GEN");
			CreatePackages(
				forwardingPackLine2, 6, warehouse2AddressBO, new ZDateTime(2020, 6, 2, 10, 17, 0),
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
				FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched,
				"S00001000-BOX", packageJob, 5);
			shipment.OuterPackLines.Add(forwardingPackLine1);
			shipment.OuterPackLines.Add(forwardingPackLine2);

			var readingHelper = Process(DataContextType.TransitDispatch, shipment, warehouse1AddressBO, shipmentDataObject);
			AssertEquals("3 outer packlines", 3, shipment.OuterPackLines.Count);

			var outerPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(l => l.JL_PackLineId == "TEST001");
			AssertPackLine(outerPackLine, 4, "BOX", FreightConstants.OuterPackType, 400m, "KG", 40m, "M3", "M", "GEN", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, new ZDateTime(2020, 6, 1, 10, 17, 0));
			AssertEquals(nameof(outerPackLine.JL_LastKnownTransitWarehouseStatus), FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received, outerPackLine.JL_LastKnownTransitWarehouseStatus);
			AssertEquals("4 outer packages", 4, outerPackLine.PkgPackageCollection.Count);
			for (var index = 1; index <= 4; index++)
			{
				var package = outerPackLine.PkgPackageCollection.FirstOrDefault(p => p.KP_PackageID == "S00001000-BOX" + index);
				AssertPackage(package, "S00001000-BOX" + index, "BOX", 1, 100m, "KG", 10m, "M3", "GEN");
			}

			outerPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(l => l.JL_PackLineId == "TEST002");
			AssertPackLine(outerPackLine, 4, "BOX", FreightConstants.OuterPackType, 400m, "KG", 40m, "M3", "M", "GEN", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, new ZDateTime(2020, 6, 2, 10, 17, 0));
			AssertEquals(nameof(outerPackLine.JL_LastKnownTransitWarehouseStatus), FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched, outerPackLine.JL_LastKnownTransitWarehouseStatus);
			AssertEquals("4 outer packages", 4, outerPackLine.PkgPackageCollection.Count);
			for (var index = 5; index <= 8; index++)
			{
				var package = outerPackLine.PkgPackageCollection.FirstOrDefault(p => p.KP_PackageID == "S00001000-BOX" + index);
				AssertPackage(package, "S00001000-BOX" + index, "BOX", 1, 100m, "KG", 10m, "M3", "GEN");
			}

			outerPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(l => l.JL_PackageCount == 2);
			AssertPackLine(outerPackLine, 2, "BOX", FreightConstants.OuterPackType, 200m, "KG", 20m, "M3", "M", "GEN", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, new ZDateTime(2020, 6, 3, 10, 17, 0));
			AssertEquals(nameof(outerPackLine.JL_LastKnownTransitWarehouseStatus), FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received, outerPackLine.JL_LastKnownTransitWarehouseStatus);
			AssertEquals("2 outer packages", 2, outerPackLine.PkgPackageCollection.Count);
			for (var index = 9; index <= 10; index++)
			{
				var package = outerPackLine.PkgPackageCollection.FirstOrDefault(p => p.KP_PackageID == "S00001000-BOX" + index);
				AssertPackage(package, "S00001000-BOX" + index, "BOX", 1, 100m, "KG", 10m, "M3", "GEN");
			}
		}

		public void TestProcessTransitDispatch_ProcessReceivedPackages()
		{
			var transportationUnits = new List<(ZString Reference, ZDateTime LastKnownTime)> { ("TD00000001", new ZDateTime(2020, 6, 1, 10, 17, 0)) };
			var (warehouse1AddressBO, shipmentDataObject) = SetupShipmentDataObject(DataContextType.TransitDispatch, "DC00000001", transportationUnits, "DLL00000011");

			var packingLine1 = CreatePackingLine("S00001000-BOX1", 1, "BOX", 3000m, "KG", 300m, "M3", "M", "TEST001", "GEN");
			packingLine1.UnloadDate = new ZDateTime(2020, 6, 3, 10, 17, 0);
			packingLine1.LoadDate = ZDateTime.Empty;
			var packingLine2 = CreatePackingLine("S00001000-PLT1", 1, "PLT", 4000m, "KG", 400m, "M3", "M", "TEST002", "GEN");
			packingLine2.LoadDate = new ZDateTime(2020, 6, 4, 10, 17, 0);
			var packingLine3 = CreatePackingLine("", 10, "CTN", 4000m, "KG", 400m, "M3", "M", "TEST003", "GEN", 10);
			packingLine3.LoadDate = new ZDateTime(2020, 6, 4, 10, 17, 0);
			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine> { packingLine1, packingLine2, packingLine3 });

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = shipment.PK;
			packageJob.KJ_ParentTableCode = shipment.TablePrefix;
			var forwardingPackLine1 = CreatePackLine("REF001", "TEST001", "BOX", FreightConstants.OuterPackType, 1, 1000m, "KG", 100m, "M3", "M", "GEN");
			CreatePackages(
				forwardingPackLine1, 1, warehouse1AddressBO, new ZDateTime(2020, 6, 1, 10, 17, 0),
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
				FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
				"S00001000-BOX", packageJob);
			var forwardingPackLine2 = CreatePackLine("REF001", "TEST002", "PLT", FreightConstants.OuterPackType, 1, 2000m, "KG", 200m, "M3", "M", "GEN");
			CreatePackages(
				forwardingPackLine2, 1, warehouse1AddressBO, new ZDateTime(2020, 6, 2, 10, 17, 0),
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
				FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
				"S00001000-PLT", packageJob);
			var forwardingPackLine3 = CreatePackLine("REF001", "TEST003", "CTN", FreightConstants.OuterPackType, 10, 2000m, "KG", 200m, "M3", "M", "GEN");
			CreateAPackageWithQty(
				packageJob, forwardingPackLine3, 10, warehouse1AddressBO, new ZDateTime(2020, 6, 2, 10, 17, 0),
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
				FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
				ZString.Empty, "TEST003", ZString.Empty);
			shipment.OuterPackLines.Add(forwardingPackLine1);
			shipment.OuterPackLines.Add(forwardingPackLine2);
			shipment.OuterPackLines.Add(forwardingPackLine3);
			Factory.SaveForTesting();
			var readingHelper = Process(DataContextType.TransitDispatch, shipment, warehouse1AddressBO, shipmentDataObject);
			AssertEquals("3 outer packlines", 3, shipment.OuterPackLines.Count);

			var outerPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(l => l.JL_F3_NKPackType == "BOX");
			AssertPackLine(outerPackLine, 1, "BOX", FreightConstants.OuterPackType, 1000m, "KG", 100m, "M3", "M", "GEN", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies, new ZDateTime(2020, 6, 3, 10, 17, 0));
			AssertEquals("1 outer package", 1, outerPackLine.PkgPackageCollection.Count);
			var package = outerPackLine.PkgPackageCollection.FirstOrDefault(p => p.KP_PackageID == "S00001000-BOX1");
			AssertPackage(package, "S00001000-BOX1", "BOX", 1, 3000m, "KG", 300m, "M3", "GEN");

			outerPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(l => l.JL_F3_NKPackType == "PLT");
			AssertPackLine(outerPackLine, 1, "PLT", FreightConstants.OuterPackType, 2000m, "KG", 200m, "M3", "M", "GEN", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies, new ZDateTime(2020, 6, 4, 10, 17, 0));
			AssertEquals("1 outer package", 1, outerPackLine.PkgPackageCollection.Count);
			package = outerPackLine.PkgPackageCollection.FirstOrDefault(p => p.KP_PackageID == "S00001000-PLT1");
			AssertPackage(package, "S00001000-PLT1", "PLT", 1, 4000m, "KG", 400m, "M3", "GEN");

			outerPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(l => l.JL_F3_NKPackType == "CTN");
			AssertPackLine(outerPackLine, 10, "CTN", FreightConstants.OuterPackType, 2000m, "KG", 200m, "M3", "M", "GEN", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies, new ZDateTime(2020, 6, 4, 10, 17, 0));
			AssertEquals("1 outer package", 1, outerPackLine.PkgPackageCollection.Count);
			package = outerPackLine.PkgPackageCollection.SingleOrDefault(); // p => p.KP_PackageID == "S00001000-PLT1");
			AssertPackage(package, "", "CTN", 10, 4000m, "KG", 400m, "M3", "GEN");
			AssertEquals("TEST003", package.KP_ExternalReference);
		}

		public void TestProcessTransitDispatch_PartiallyDispatchPackages()
		{
			var transportationUnits = new List<(ZString Reference, ZDateTime LastKnownTime)> { ("TD00000001", new ZDateTime(2020, 6, 1, 10, 17, 0)) };
			var (warehouse1AddressBO, shipmentDataObject) = SetupShipmentDataObject(DataContextType.TransitDispatch, "DC00000001", transportationUnits, "DLL00000011");

			var packingLineCollection = new DataObjectList<UniversalPackingLine>();
			for (var index = 1; index <= 4; index++)
			{
				var packingLine = CreatePackingLine("S00001000-BOX" + index, 1, "BOX", 100m, "KG", 10m, "M3", "M", "TEST001", "GEN");
				packingLine.UnloadDate = new ZDateTime(2020, 6, 3, 10, 17, 0);
				packingLine.LoadDate = ZDateTime.Empty;
				packingLineCollection.Add(packingLine);
			}
			for (var index = 5; index <= 10; index++)
			{
				var packingLine = CreatePackingLine("S00001000-BOX" + index, 1, "BOX", 100m, "KG", 10m, "M3", "M", "TEST001", "GEN");
				packingLine.LoadDate = new ZDateTime(2020, 6, 4, 10, 17, 0);
				packingLineCollection.Add(packingLine);
			}
			shipmentDataObject.SetPackingLineCollection(() => packingLineCollection);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = shipment.PK;
			var forwardingPackLine = CreatePackLine("REF001", "TEST001", "BOX", FreightConstants.OuterPackType, 10, 1000m, "KG", 100m, "M3", "M", "GEN");
			CreatePackages(
				forwardingPackLine, 10, warehouse1AddressBO, new ZDateTime(2020, 6, 1, 10, 17, 0),
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
				FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
				"S00001000-BOX", packageJob);
			shipment.OuterPackLines.Add(forwardingPackLine);

			var readingHelper = Process(DataContextType.TransitDispatch, shipment, warehouse1AddressBO, shipmentDataObject);
			AssertEquals("2 outer packlines", 2, shipment.OuterPackLines.Count);

			var outerPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(l => l.JL_PackageCount == 4);
			AssertPackLine(outerPackLine, 4, "BOX", FreightConstants.OuterPackType, 400m, "KG", 40m, "M3", "M", "GEN", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, new ZDateTime(2020, 6, 3, 10, 17, 0));
			AssertEquals(nameof(outerPackLine.JL_LastKnownTransitWarehouseStatus), FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received, outerPackLine.JL_LastKnownTransitWarehouseStatus);
			AssertEquals("4 outer packages", 4, outerPackLine.PkgPackageCollection.Count);
			for (var index = 1; index <= 4; index++)
			{
				var package = outerPackLine.PkgPackageCollection.FirstOrDefault(p => p.KP_PackageID == "S00001000-BOX" + index);
				AssertPackage(package, "S00001000-BOX" + index, "BOX", 1, 100m, "KG", 10m, "M3", "GEN");
			}

			outerPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(l => l.JL_PackageCount == 6);
			AssertPackLine(outerPackLine, 6, "BOX", FreightConstants.OuterPackType, 600m, "KG", 60m, "M3", "M", "GEN", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, new ZDateTime(2020, 6, 4, 10, 17, 0));
			AssertEquals(nameof(outerPackLine.JL_LastKnownTransitWarehouseStatus), FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched, outerPackLine.JL_LastKnownTransitWarehouseStatus);
			AssertEquals("6 outer packages", 6, outerPackLine.PkgPackageCollection.Count);
			for (var index = 5; index <= 10; index++)
			{
				var package = outerPackLine.PkgPackageCollection.FirstOrDefault(p => p.KP_PackageID == "S00001000-BOX" + index);
				AssertPackage(package, "S00001000-BOX" + index, "BOX", 1, 100m, "KG", 10m, "M3", "GEN");
			}
		}

		public void TestProcessTransitDispatch_DispatchPackagesReceivedInOtherWarehouses()
		{
			var transportationUnits = new List<(ZString Reference, ZDateTime LastKnownTime)> { ("TD00000001", new ZDateTime(2020, 6, 4, 10, 17, 0)) };
			var (warehouse1AddressBO, shipmentDataObject) = SetupShipmentDataObject(DataContextType.TransitDispatch, "DC00000001", transportationUnits, "DLL00000011");
			var warehouse2AddressBO = Factory.NewWithValidTestData<OrgAddress>();

			var packingLineCollection = new DataObjectList<UniversalPackingLine>();
			for (var index = 1; index <= 4; index++)
			{
				var packingLine = CreatePackingLine("S00001000-BOX" + index, 1, "BOX", 100m, "KG", 10m, "M3", "M", "TEST001", "GEN");
				packingLine.LoadDate = new ZDateTime(2020, 6, 4, 10, 17, 0);
				packingLineCollection.Add(packingLine);
			}
			for (var index = 5; index <= 10; index++)
			{
				var packingLine = CreatePackingLine("S00001000-BOX" + index, 1, "BOX", 100m, "KG", 10m, "M3", "M", "TEST001", "GEN");
				packingLine.LoadDate = new ZDateTime(2020, 6, 2, 10, 17, 0);
				packingLineCollection.Add(packingLine);
			}
			shipmentDataObject.SetPackingLineCollection(() => packingLineCollection);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = shipment.PK;
			var forwardingPackLine1 = CreatePackLine("REF001", "TEST001", "BOX", FreightConstants.OuterPackType, 4, 400m, "KG", 40m, "M3", "M", "GEN");
			CreatePackages(
				forwardingPackLine1, 4, warehouse1AddressBO, new ZDateTime(2020, 6, 1, 10, 17, 0),
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
				FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
				"S00001000-BOX", packageJob);
			var forwardingPackLine2 = CreatePackLine("REF002", "TEST002", "BOX", FreightConstants.OuterPackType, 4, 400m, "KG", 40m, "M3", "M", "GEN");
			CreatePackages(
				forwardingPackLine2, 4, warehouse1AddressBO, new ZDateTime(2020, 6, 2, 10, 17, 0),
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
				FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched,
				"S00001000-BOX", packageJob, 5);
			var forwardingPackLine3 = CreatePackLine("REF003", "TEST003", "BOX", FreightConstants.OuterPackType, 2, 200m, "KG", 20m, "M3", "M", "GEN");
			CreatePackages(
				forwardingPackLine3, 2, warehouse2AddressBO, new ZDateTime(2020, 6, 3, 10, 17, 0),
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
				FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
				"S00001000-BOX", packageJob, 9);
			shipment.OuterPackLines.Add(forwardingPackLine1);
			shipment.OuterPackLines.Add(forwardingPackLine2);
			shipment.OuterPackLines.Add(forwardingPackLine3);

			var readingHelper = Process(DataContextType.TransitDispatch, shipment, warehouse1AddressBO, shipmentDataObject);
			AssertEquals("3 outer packlines", 3, shipment.OuterPackLines.Count);

			var outerPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(l => l.JL_PackLineId == "TEST001");
			AssertPackLine(outerPackLine, 4, "BOX", FreightConstants.OuterPackType, 400m, "KG", 40m, "M3", "M", "GEN", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, new ZDateTime(2020, 6, 4, 10, 17, 0));
			AssertEquals(nameof(outerPackLine.JL_LastKnownTransitWarehouseStatus), FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched, outerPackLine.JL_LastKnownTransitWarehouseStatus);
			AssertEquals("4 outer packages", 4, outerPackLine.PkgPackageCollection.Count);
			for (var index = 1; index <= 4; index++)
			{
				var package = outerPackLine.PkgPackageCollection.FirstOrDefault(p => p.KP_PackageID == "S00001000-BOX" + index);
				AssertPackage(package, "S00001000-BOX" + index, "BOX", 1, 100m, "KG", 10m, "M3", "GEN");
			}

			outerPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(l => l.JL_PackLineId == "TEST002");
			AssertPackLine(outerPackLine, 4, "BOX", FreightConstants.OuterPackType, 400m, "KG", 40m, "M3", "M", "GEN", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, new ZDateTime(2020, 6, 2, 10, 17, 0));
			AssertEquals(nameof(outerPackLine.JL_LastKnownTransitWarehouseStatus), FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched, outerPackLine.JL_LastKnownTransitWarehouseStatus);
			AssertEquals("4 outer packages", 4, outerPackLine.PkgPackageCollection.Count);
			for (var index = 5; index <= 8; index++)
			{
				var package = outerPackLine.PkgPackageCollection.FirstOrDefault(p => p.KP_PackageID == "S00001000-BOX" + index);
				AssertPackage(package, "S00001000-BOX" + index, "BOX", 1, 100m, "KG", 10m, "M3", "GEN");
			}

			outerPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(l => l.JL_PackLineId == "TEST003");
			AssertPackLine(outerPackLine, 2, "BOX", FreightConstants.OuterPackType, 200m, "KG", 20m, "M3", "M", "GEN", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, new ZDateTime(2020, 6, 3, 10, 17, 0));
			AssertEquals(nameof(outerPackLine.JL_LastKnownTransitWarehouseStatus), FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received, outerPackLine.JL_LastKnownTransitWarehouseStatus);
			AssertEquals("2 outer packages", 2, outerPackLine.PkgPackageCollection.Count);
			for (var index = 9; index <= 10; index++)
			{
				var package = outerPackLine.PkgPackageCollection.FirstOrDefault(p => p.KP_PackageID == "S00001000-BOX" + index);
				AssertPackage(package, "S00001000-BOX" + index, "BOX", 1, 100m, "KG", 10m, "M3", "GEN");
			}
		}

		public void TestProcessTransitPrepareDispatch_ProcessDispatchedPackages()
		{
			var transportationUnits = new List<(ZString Reference, ZDateTime LastKnownTime)> { ("TD00000001", new ZDateTime(2020, 6, 1, 10, 17, 0)) };
			var (warehouse1AddressBO, shipmentDataObject) = SetupShipmentDataObject(DataContextType.TransitDispatch, "DC00000001", transportationUnits, ZString.Empty);

			var packingLine1 = CreatePackingLine("S00001000-BOX1", 1, "BOX", 3000m, "KG", 300m, "M3", "M", "TEST001", "GEN");
			packingLine1.UnloadDate = new ZDateTime(2020, 6, 3, 10, 17, 0);
			packingLine1.LoadDate = ZDateTime.Empty;
			var packingLine2 = CreatePackingLine("S00001000-PLT1", 1, "PLT", 4000m, "KG", 400m, "M3", "M", "TEST002", "GEN");
			packingLine2.LoadDate = new ZDateTime(2020, 6, 4, 10, 17, 0);
			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine> { packingLine1, packingLine2 });

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = shipment.PK;
			var forwardingPackLine1 = CreatePackLine("REF001", "TEST001", "BOX", FreightConstants.OuterPackType, 1, 1000m, "KG", 100m, "M3", "M", "GEN");
			CreatePackages(
				forwardingPackLine1, 1, warehouse1AddressBO, new ZDateTime(2020, 6, 1, 10, 17, 0),
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
				FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
				"S00001000-BOX", packageJob);
			var forwardingPackLine2 = CreatePackLine("REF001", "TEST002", "PLT", FreightConstants.OuterPackType, 1, 2000m, "KG", 200m, "M3", "M", "GEN");
			CreatePackages(
				forwardingPackLine2, 1, warehouse1AddressBO, new ZDateTime(2020, 6, 2, 10, 17, 0),
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
				FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
				"S00001000-PLT", packageJob);
			shipment.OuterPackLines.Add(forwardingPackLine1);
			shipment.OuterPackLines.Add(forwardingPackLine2);

			var readingHelper = Process(DataContextType.TransitDispatch, shipment, warehouse1AddressBO, shipmentDataObject);
			AssertEquals("2 outer packlines", 2, shipment.OuterPackLines.Count);

			var outerPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(l => l.JL_F3_NKPackType == "BOX");
			AssertPackLine(outerPackLine, 1, "BOX", FreightConstants.OuterPackType, 1000m, "KG", 100m, "M3", "M", "GEN", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies, new ZDateTime(2020, 6, 3, 10, 17, 0));
			AssertEquals("1 outer package", 1, outerPackLine.PkgPackageCollection.Count);
			var package = outerPackLine.PkgPackageCollection.FirstOrDefault(p => p.KP_PackageID == "S00001000-BOX1");
			AssertPackage(package, "S00001000-BOX1", "BOX", 1, 3000m, "KG", 300m, "M3", "GEN");

			outerPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(l => l.JL_F3_NKPackType == "PLT");
			AssertPackLine(outerPackLine, 1, "PLT", FreightConstants.OuterPackType, 2000m, "KG", 200m, "M3", "M", "GEN", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies, new ZDateTime(2020, 6, 4, 10, 17, 0));
			AssertEquals("1 outer package", 1, outerPackLine.PkgPackageCollection.Count);
			package = outerPackLine.PkgPackageCollection.FirstOrDefault(p => p.KP_PackageID == "S00001000-PLT1");
			AssertPackage(package, "S00001000-PLT1", "PLT", 1, 4000m, "KG", 400m, "M3", "GEN");
		}

		public void TestProcessTransitReceive_GetReceiveConsignmentNumberFromForwardingPackingLine_DoesNotThrowException()
		{
			var transportationUnits = new List<(ZString Reference, ZDateTime LastKnownTime)> { ("TD00000001", new ZDateTime(2020, 6, 1, 10, 17, 0)) };
			var (warehouse1AddressBO, shipmentDataObject) = SetupShipmentDataObject(DataContextType.TransitDispatch, "DC00000001", transportationUnits, "DLL00000011");

			var packingLine1 = CreatePackingLine("S00001000-BOX1", 1, "BOX", 3000m, "KG", 300m, "M3", "M", "TEST002", "GEN");
			packingLine1.UnloadDate = new ZDateTime(2020, 6, 3, 10, 17, 0);
			var packingLine2 = CreatePackingLine("OVP00001", 1, "PLT", 4000m, "KG", 400m, "M3", "M", string.Empty, "GEN");
			packingLine2.UnloadDate = new ZDateTime(2020, 6, 3, 10, 17, 0);
			var packingLineCollection1 = new List<UniversalPackingLine>();
			for (var i = 2; i <= 3; i++)
			{
				var packingLineInner = CreatePackingLine("S00001000-BOX" + i, 1, "BOX", 100m, "KG", 10m, "M3", "M", "TEST002", "GEN");
				packingLineInner.UnloadDate = new ZDateTime(2020, 6, 3, 10, 17, 0);
				packingLineCollection1.Add(packingLineInner);
			}
			packingLine2.SetPackingLineCollection(() => packingLineCollection1);
			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine> { packingLine2, packingLine1 });
			shipmentDataObject.PackingLineCollection.Content = CollectionContent.Complete;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = shipment.PK;
			packageJob.KJ_ParentTableCode = shipment.TablePrefix;
			var forwardingPackLine = CreatePackLine("REF001", "TEST002", "BOX", FreightConstants.OuterPackType, 3, 2000m, "KG", 200m, "M3", "M", "GEN");
			shipment.OuterPackLines.Add(forwardingPackLine);

			Factory.SaveForTesting();

			AssertNoExceptionThrown(() => { Process(DataContextType.TransitReceive, shipment, warehouse1AddressBO, shipmentDataObject); });

			Factory.SaveForTesting();

			var packingLineCollection2 = new List<UniversalPackingLine>();
			for (var i = 2; i <= 2; i++)
			{
				var packingLineInner = CreatePackingLine("S00001000-BOX" + i, 1, "BOX", 100m, "KG", 10m, "M3", "M", "TEST002", "GEN");
				packingLineInner.UnloadDate = new ZDateTime(2020, 6, 3, 10, 17, 0);
				packingLineCollection2.Add(packingLineInner);
			}
			packingLine2.SetPackingLineCollection(() => packingLineCollection2);

			var packingLine3 = CreatePackingLine("S00001000-BOX3", 1, "BOX", 3000m, "KG", 300m, "M3", "M", "TEST002", "GEN");
			packingLine3.UnloadDate = new ZDateTime(2020, 6, 3, 10, 17, 0);
			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine> { packingLine1, packingLine2, packingLine3 });
			shipmentDataObject.PackingLineCollection.Content = CollectionContent.Complete;

			AssertNoExceptionThrown(() => { Process(DataContextType.TransitReceive, shipment, warehouse1AddressBO, shipmentDataObject); });

			AssertEquals("2 outer packlines", 2, shipment.OuterPackLines.Count);
		}

		public void TestSetContainer_DoesNotReportError()
		{
			var transportationUnits = new List<(ZString Reference, ZDateTime LastKnownTime)> { ("TD00000001", new ZDateTime(2020, 6, 1, 10, 17, 0)) };
			var (warehouse1AddressBO, shipmentDataObject) = SetupShipmentDataObject(DataContextType.TransitDispatch, "DC00000001", transportationUnits, "DLL00000011");

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONTAINER";
			container.JC_TareWeight = 0m;
			container.JC_GrossWeightUQ = "KG";
			container.JC_F3_NKPackType = "AKE";

			var shipment = consol.Shipments.AddNew();
			var forwardingPackLine1 = CreatePackLine("REF001", "TEST001", "BOX", FreightConstants.OuterPackType, 1, 999999m, "KG", 200m, "M3", "M", "GEN");
			var forwardingPackLine2 = CreatePackLine("REF002", "TEST002", "PLT", FreightConstants.OuterPackType, 1, 999999m, "KG", 200m, "M3", "M", "GEN");
			shipment.OuterPackLines.Add(forwardingPackLine1);
			shipment.OuterPackLines.Add(forwardingPackLine2);
			forwardingPackLine1.JL_JC = ZGuid.Empty;
			forwardingPackLine2.JL_JC = ZGuid.Empty;
			Factory.SaveForTesting();

			var packingLine1 = CreatePackingLine("S00001000-BOX1", 1, "BOX", 999999m, "KG", 300m, "M3", "M", "TEST001", "GEN");
			packingLine1.ContainerLink = 0;
			packingLine1.UnloadDate = new ZDateTime(2020, 6, 3, 10, 17, 0);
			var packingLine2 = CreatePackingLine("S00001000-BOX2", 1, "PLT", 999999m, "KG", 300m, "M3", "M", "TEST002", "GEN");
			packingLine2.ContainerLink = 0;
			packingLine2.UnloadDate = new ZDateTime(2020, 6, 3, 10, 17, 0);
			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine> { packingLine1, packingLine2 });
			shipmentDataObject.VesselName = "CONTAINER";

			var containerDataObject = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			containerDataObject.ContainerNumber = "CONTAINER";
			containerDataObject.ContainerType = new ContainerType() { Code = "AKE" };
			containerDataObject.WeightUnit = new UnitOfWeight() { Code = "KG" };
			containerDataObject.ContainerCount = 1;
			containerDataObject.Link = 0;
			shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container> { containerDataObject });

			var readingHelper = new ShipmentDataObjectReadingHelper(shipmentDataObject, Logger, Factory);
			readingHelper.LinkManager = new ContainerLinkManager<ForwardingConsol>(consol);
			var helper = new ShipmentDataObjectSplitPackageHelper(shipmentDataObject, shipment, warehouse1AddressBO, Logger, Factory, readingHelper);
			helper.ProcessTransitReceive();
			AssertNoExceptionThrown(() => Factory.SaveForTesting());
		}

		public void TestSplitPackline_DoesNotReportError()
		{
			var transportationUnits = new List<(ZString Reference, ZDateTime LastKnownTime)> { ("TD00000001", new ZDateTime(2020, 6, 1, 10, 17, 0)) };
			var (warehouse1AddressBO, shipmentDataObject) = SetupShipmentDataObject(DataContextType.TransitDispatch, "DC00000001", transportationUnits, "DLL00000011");

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONTAINER";
			container.JC_TareWeight = 0m;
			container.JC_GrossWeightUQ = "KG";
			container.JC_F3_NKPackType = "AKE";

			var shipment = consol.Shipments.AddNew();
			var forwardingPackLine1 = CreatePackLine("REF001", "TEST001", "BOX", FreightConstants.OuterPackType, 1, 1000m, "KG", 200m, "M3", "M", "GEN");
			var forwardingPackLine2 = CreatePackLine("REF002", "TEST002", "PLT", FreightConstants.OuterPackType, 3, 2000m, "KG", 200m, "M3", "M", "GEN");
			shipment.OuterPackLines.Add(forwardingPackLine1);
			shipment.OuterPackLines.Add(forwardingPackLine2);
			Factory.SaveForTesting();

			var packingLine1 = CreatePackingLine("S00001000-BOX1", 1, "BOX", 999999m, "KG", 300m, "M3", "M", "TEST001", "GEN");
			packingLine1.ContainerLink = 0;
			packingLine1.UnloadDate = new ZDateTime(2020, 6, 3, 10, 17, 0);
			var packingLine2 = CreatePackingLine("S00001000-PLT2", 1, "PLT", 999999m, "KG", 300m, "M3", "M", "TEST002", "GEN");
			packingLine2.ContainerLink = 0;
			packingLine2.UnloadDate = new ZDateTime(2020, 6, 3, 10, 17, 0);
			var packingLine3 = CreatePackingLine("S00001000-BOX3", 2, "BOX", 999999m, "KG", 300m, "M3", "M", "TEST002", "GEN");
			packingLine3.ContainerLink = 0;
			packingLine3.UnloadDate = new ZDateTime(2020, 6, 3, 10, 17, 0);
			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine> { packingLine1, packingLine2, packingLine3 });
			shipmentDataObject.VesselName = "CONTAINER";

			var containerDataObject = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			containerDataObject.ContainerNumber = "CONTAINER";
			containerDataObject.ContainerType = new ContainerType() { Code = "AKE" };
			containerDataObject.WeightUnit = new UnitOfWeight() { Code = "KG" };
			containerDataObject.ContainerCount = 1;
			containerDataObject.Link = 0;
			shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container> { containerDataObject });

			var readingHelper = new ShipmentDataObjectReadingHelper(shipmentDataObject, Logger, Factory);
			readingHelper.LinkManager = new ContainerLinkManager<ForwardingConsol>(consol);
			var helper = new ShipmentDataObjectSplitPackageHelper(shipmentDataObject, shipment, warehouse1AddressBO, Logger, Factory, readingHelper);
			helper.ProcessTransitReceive();
			AssertNoExceptionThrown(() => Factory.SaveForTesting());
		}

		public void TestSetQuantityWeightAndVolumeFromPackageTotals_DoesNotReportError()
		{
			var transportationUnits = new List<(ZString Reference, ZDateTime LastKnownTime)> { ("TD00000001", new ZDateTime(2020, 6, 1, 10, 17, 0)) };
			var (warehouse1AddressBO, shipmentDataObject) = SetupShipmentDataObject(DataContextType.TransitDispatch, "DC00000001", transportationUnits, "DLL00000011");

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONTAINER";
			container.JC_TareWeight = 0m;
			container.JC_GrossWeightUQ = "KG";
			container.JC_F3_NKPackType = "AKE";

			var shipment = consol.Shipments.AddNew();
			var forwardingPackLine1 = CreatePackLine("REF001", "TEST001", "BOX", FreightConstants.OuterPackType, 1, 1000m, "KG", 200m, "M3", "M", "GEN");
			var forwardingPackLine2 = CreatePackLine("REF002", "TEST002", "PLT", FreightConstants.OuterPackType, 2, 2000m, "KG", 200m, "M3", "M", "GEN");
			shipment.OuterPackLines.Add(forwardingPackLine1);
			shipment.OuterPackLines.Add(forwardingPackLine2);
			Factory.SaveForTesting();

			var packingLine1 = CreatePackingLine("S00001000-BOX1", 1, "BOX", 999999m, "KG", 300m, "M3", "M", "TEST001", "GEN");
			packingLine1.ContainerLink = 0;
			packingLine1.UnloadDate = new ZDateTime(2020, 6, 3, 10, 17, 0);
			var packingLine2 = CreatePackingLine("S00001000-PLT2", 1, "PLT", 999999m, "KG", 300m, "M3", "M", "TEST002", "GEN");
			packingLine2.ContainerLink = 0;
			packingLine2.UnloadDate = new ZDateTime(2020, 6, 3, 10, 17, 0);
			var packingLine3 = CreatePackingLine("S00001000-BOX3", 1, "BOX", 999999m, "KG", 300m, "M3", "M", "TEST002", "GEN");
			packingLine3.ContainerLink = 0;
			packingLine3.UnloadDate = new ZDateTime(2020, 6, 3, 10, 17, 0);
			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine> { packingLine1, packingLine2, packingLine3 });
			shipmentDataObject.VesselName = "CONTAINER";

			var containerDataObject = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			containerDataObject.ContainerNumber = "CONTAINER";
			containerDataObject.ContainerType = new ContainerType() { Code = "AKE" };
			containerDataObject.WeightUnit = new UnitOfWeight() { Code = "KG" };
			containerDataObject.ContainerCount = 1;
			containerDataObject.Link = 0;
			shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container> { containerDataObject });

			var readingHelper = new ShipmentDataObjectReadingHelper(shipmentDataObject, Logger, Factory);
			readingHelper.LinkManager = new ContainerLinkManager<ForwardingConsol>(consol);
			var helper = new ShipmentDataObjectSplitPackageHelper(shipmentDataObject, shipment, warehouse1AddressBO, Logger, Factory, readingHelper);
			helper.ProcessTransitReceive();
			AssertNoExceptionThrown(() => Factory.SaveForTesting());
		}

		public void TestImportCompletePacklineWithInvalidPackLine()
		{
			var warehouse1AddressBO = Factory.NewWithValidTestData<OrgAddress>();
			var warehouse2AddressBO = Factory.NewWithValidTestData<OrgAddress>();
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.DataContext = DataContextFactory.New();
			shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitDispatch, "DC00000001");
			shipmentDataObject.WayBillNumber = "BACON PANCAKES";
			shipmentDataObject.WayBillType = new UniversalWayBillType { Code = WayBillTypeList.Codes.House };

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<UniversalOrganizationAddress> { new UniversalOrganizationAddress()
			{
				AddressShortCode = warehouse1AddressBO.AddressCode,
				AddressType = nameof(DocAddressType.LocalCartageCFS),
				OrganizationCode = warehouse1AddressBO.Header.OH_Code
			} });

			var header1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			header1.DataContext = new DataContext();
			header1.DataContext.AddDataSource(DataContextType.TransitDispatchHeader, "TD00000001");
			header1.SetDateCollection(() => new List<UniversalDate> { new UniversalDate { Type = UniversalDateType.Pack, Value = new ZDateTime(2020, 6, 1, 10, 17, 0) } });
			var header2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			header2.DataContext = new DataContext();
			header2.DataContext.AddDataSource(DataContextType.TransitDispatchHeader, "TD00000002");
			header2.SetDateCollection(() => new List<UniversalDate> { new UniversalDate { Type = UniversalDateType.Pack, Value = new ZDateTime(2020, 6, 1, 10, 6, 0) } });
			shipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>() { header1, header2 });

			var loadList = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			loadList.DataContext = new DataContext();
			loadList.DataContext.AddDataSource(DataContextType.TransitDispatchLoadList, "DLL00000011");
			shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { loadList });

			var packingLine1 = ShipmentDataObjectReaderTest.CreatePackline();
			packingLine1.ReferenceNumber = "S00001557-CTN1";
			packingLine1.LoadDate = new ZDateTime(2020, 6, 1, 10, 6, 0);
			packingLine1.SetReferenceNumberCollection(() => new List<UniversalReference>() { new UniversalReference { Type = new UniversalEntryType { Code = "TDU" }, ReferenceNumber = "TD00000002" } });
			var packingLine2 = ShipmentDataObjectReaderTest.CreatePackline();
			packingLine2.ReferenceNumber = "S00001557-CTN2";
			packingLine2.LoadDate = new ZDateTime(2020, 6, 1, 10, 17, 0);
			packingLine2.SetReferenceNumberCollection(() => new List<UniversalReference>() { new UniversalReference { Type = new UniversalEntryType { Code = "TDU" }, ReferenceNumber = "TD00000001" } });
			var packingLine3 = ShipmentDataObjectReaderTest.CreatePackline();
			packingLine3.ReferenceNumber = "S00001557-CTN3";
			packingLine3.LoadDate = new ZDateTime(2020, 6, 1, 10, 17, 0);
			packingLine3.SetReferenceNumberCollection(() => new List<UniversalReference>() { new UniversalReference { Type = new UniversalEntryType { Code = "TDU" }, ReferenceNumber = "TD00000001" } });

			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine>() { packingLine1, packingLine2, packingLine3 });
			shipmentDataObject.PackingLineCollection.Content = CollectionContent.Complete;

			var shipment = Factory.New(ObjectFactory.GetType(typeof(IForwardingShipment))) as CommonShipment;
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackLineId = "Test001";
			packLine1.JL_LastKnownTransitWarehouseStatusDateTime = new ZDateTime(2020, 6, 1, 10, 10, 0);
			packLine1.PkgPackageCollection.AddNew().KP_PackageID = "S00001557-CTN1";
			packLine1.PkgPackageCollection.AddNew().KP_PackageID = "S00001557-CTN2";
			packLine1.PkgPackageCollection.AddNew().KP_PackageID = "S00001557-CTN3";

			var job = shipment.Factory.New<PkgPackageJob>();
			job.KJ_JobID = shipment.JobNumber;
			job.KJ_ParentID = shipment.PK;
			job.KJ_ParentTableCode = shipment.TablePrefix;
			job.Packages.Add(packLine1.PkgPackageCollection[0]);
			job.Packages.Add(packLine1.PkgPackageCollection[1]);
			job.Packages.Add(packLine1.PkgPackageCollection[2]);

			Process(DataContextType.TransitDispatch, shipment as ForwardingShipment, warehouse1AddressBO, shipmentDataObject);
			AssertEquals("should not remove invalid package", 3, packLine1.PkgPackageCollection.Count);
			AssertEquals("should not remove invalid package", 3, job.Packages.Count);
		}

		public void TestImportCompletePacklineWithValidPackLine()
		{
			var warehouse1AddressBO = Factory.NewWithValidTestData<OrgAddress>();
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.DataContext = DataContextFactory.New();
			shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitDispatch, "DC00000001");
			shipmentDataObject.WayBillNumber = "BACON PANCAKES";
			shipmentDataObject.WayBillType = new UniversalWayBillType { Code = WayBillTypeList.Codes.House };

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<UniversalOrganizationAddress> { new UniversalOrganizationAddress()
			{
				AddressShortCode = warehouse1AddressBO.AddressCode,
				AddressType = nameof(DocAddressType.LocalCartageCFS),
				OrganizationCode = warehouse1AddressBO.Header.OH_Code
			} });

			var header1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			header1.DataContext = new DataContext();
			header1.DataContext.AddDataSource(DataContextType.TransitDispatchHeader, "TD00000001");
			header1.SetDateCollection(() => new List<UniversalDate> { new UniversalDate { Type = UniversalDateType.Pack, Value = new ZDateTime(2020, 6, 1, 10, 17, 0) } });
			var header2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			header2.DataContext = new DataContext();
			header2.DataContext.AddDataSource(DataContextType.TransitDispatchHeader, "TD00000002");
			header2.SetDateCollection(() => new List<UniversalDate> { new UniversalDate { Type = UniversalDateType.Pack, Value = new ZDateTime(2020, 6, 1, 10, 6, 0) } });
			shipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>() { header1, header2 });

			var loadList = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			loadList.DataContext = new DataContext();
			loadList.DataContext.AddDataSource(DataContextType.TransitDispatchLoadList, "DLL00000011");
			shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { loadList });

			var packingLine2 = ShipmentDataObjectReaderTest.CreatePackline();
			packingLine2.ReferenceNumber = "S00001557-CTN2";
			packingLine2.LoadDate = new ZDateTime(2020, 6, 1, 10, 17, 0);
			packingLine2.SetReferenceNumberCollection(() => new List<UniversalReference>() { new UniversalReference { Type = new UniversalEntryType { Code = "TDU" }, ReferenceNumber = "TD00000001" } });
			var packingLine3 = ShipmentDataObjectReaderTest.CreatePackline();
			packingLine3.ReferenceNumber = "S00001557-CTN3";
			packingLine3.LoadDate = new ZDateTime(2020, 6, 1, 10, 6, 0);
			packingLine3.SetReferenceNumberCollection(() => new List<UniversalReference>() { new UniversalReference { Type = new UniversalEntryType { Code = "TDU" }, ReferenceNumber = "TD00000002" } });

			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine>() { packingLine2, packingLine3 });
			shipmentDataObject.PackingLineCollection.Content = CollectionContent.Complete;

			var shipment = Factory.New(ObjectFactory.GetType(typeof(IForwardingShipment))) as CommonShipment;
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackLineId = "Test001";
			packLine1.JL_OA_LastKnownTransitWarehouseAddress = warehouse1AddressBO.PK;
			packLine1.JL_LastKnownTransitWarehouseStatusDateTime = new ZDateTime(2020, 6, 1, 10, 1, 0);
			packLine1.PkgPackageCollection.AddNew().KP_PackageID = "S00001557-CTN1";
			packLine1.PkgPackageCollection.AddNew().KP_PackageID = "S00001557-CTN2";
			packLine1.PkgPackageCollection.AddNew().KP_PackageID = "S00001557-CTN3";

			var job = shipment.Factory.New<PkgPackageJob>();
			job.KJ_JobID = shipment.JobNumber;
			job.KJ_ParentID = shipment.PK;
			job.KJ_ParentTableCode = shipment.TablePrefix;
			job.Packages.Add(packLine1.PkgPackageCollection[0]);
			job.Packages.Add(packLine1.PkgPackageCollection[1]);
			job.Packages.Add(packLine1.PkgPackageCollection[2]);

			Process(DataContextType.TransitDispatch, shipment as ForwardingShipment, warehouse1AddressBO, shipmentDataObject);
			AssertEquals("should remove unimported package", 2, packLine1.PkgPackageCollection.Count);
			AssertEquals("should remove unimported package", 2, job.Packages.Count);
		}

		public void TestImportCompletePacklineWithMissingPackages()
		{
			var warehouse1AddressBO = Factory.NewWithValidTestData<OrgAddress>();
			var warehouse2AddressBO = Factory.NewWithValidTestData<OrgAddress>();
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.DataContext = DataContextFactory.New();
			shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitDispatch, "DC00000001");
			shipmentDataObject.WayBillNumber = "BACON PANCAKES";
			shipmentDataObject.WayBillType = new UniversalWayBillType { Code = WayBillTypeList.Codes.House };

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<UniversalOrganizationAddress> { new UniversalOrganizationAddress()
			{
				AddressShortCode = warehouse1AddressBO.AddressCode,
				AddressType = nameof(DocAddressType.LocalCartageCFS),
				OrganizationCode = warehouse1AddressBO.Header.OH_Code
			} });

			var header1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			header1.DataContext = new DataContext();
			header1.DataContext.AddDataSource(DataContextType.TransitDispatchHeader, "TD00000001");
			header1.SetDateCollection(() => new List<UniversalDate> { new UniversalDate { Type = UniversalDateType.Pack, Value = new ZDateTime(2020, 6, 1, 10, 17, 0) } });
			var header2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			header2.DataContext = new DataContext();
			header2.DataContext.AddDataSource(DataContextType.TransitDispatchHeader, "TD00000002");
			header2.SetDateCollection(() => new List<UniversalDate> { new UniversalDate { Type = UniversalDateType.Pack, Value = new ZDateTime(2020, 6, 1, 10, 6, 0) } });
			shipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>() { header1, header2 });

			var loadList = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			loadList.DataContext = new DataContext();
			loadList.DataContext.AddDataSource(DataContextType.TransitDispatchLoadList, "DLL00000011");
			shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { loadList });

			var packingLine1 = ShipmentDataObjectReaderTest.CreatePackline();
			packingLine1.OutturnQty = 0;
			packingLine1.ReferenceNumber = "S00001557-CTN1";
			packingLine1.LoadDate = ZDateTime.Empty;
			packingLine1.SetReferenceNumberCollection(() => new List<UniversalReference>() { new UniversalReference { Type = new UniversalEntryType { Code = "TDU" }, ReferenceNumber = "TD00000002" } });
			var packingLine2 = ShipmentDataObjectReaderTest.CreatePackline();
			packingLine2.ReferenceNumber = "S00001557-CTN2";
			packingLine2.LoadDate = new ZDateTime(2020, 6, 1, 10, 17, 0);
			packingLine2.SetReferenceNumberCollection(() => new List<UniversalReference>() { new UniversalReference { Type = new UniversalEntryType { Code = "TDU" }, ReferenceNumber = "TD00000001" } });
			var packingLine3 = ShipmentDataObjectReaderTest.CreatePackline();
			packingLine3.ReferenceNumber = "S00001557-CTN3";
			packingLine3.LoadDate = new ZDateTime(2020, 6, 1, 10, 17, 0);
			packingLine3.SetReferenceNumberCollection(() => new List<UniversalReference>() { new UniversalReference { Type = new UniversalEntryType { Code = "TDU" }, ReferenceNumber = "TD00000001" } });

			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine>() { packingLine1, packingLine2, packingLine3 });
			shipmentDataObject.PackingLineCollection.Content = CollectionContent.Complete;

			var shipment = Factory.New(ObjectFactory.GetType(typeof(IForwardingShipment))) as CommonShipment;
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackLineId = "Test001";
			packLine1.JL_OA_LastKnownTransitWarehouseAddress = warehouse1AddressBO.PK;
			packLine1.JL_LastKnownTransitWarehouseStatusDateTime = new ZDateTime(2020, 6, 1, 10, 10, 0);
			packLine1.PkgPackageCollection.AddNew().KP_PackageID = "S00001557-CTN1";
			packLine1.PkgPackageCollection.AddNew().KP_PackageID = "S00001557-CTN2";
			packLine1.PkgPackageCollection.AddNew().KP_PackageID = "S00001557-CTN3";

			var job = shipment.Factory.New<PkgPackageJob>();
			job.KJ_JobID = shipment.JobNumber;
			job.KJ_ParentID = shipment.PK;
			job.KJ_ParentTableCode = shipment.TablePrefix;
			job.Packages.Add(packLine1.PkgPackageCollection[0]);
			job.Packages.Add(packLine1.PkgPackageCollection[1]);
			job.Packages.Add(packLine1.PkgPackageCollection[2]);

			Process(DataContextType.TransitDispatch, shipment as ForwardingShipment, warehouse1AddressBO, shipmentDataObject);
			CombineAssertions("Missing packages should be deleted", () =>
			{
				AssertEquals(2, packLine1.PkgPackageCollection.Count);
				AssertEquals(2, job.Packages.Count);
			});
		}

		public void TestFliterPacklineByOutturnQtyAndReferenceNumber()
		{
			var warehouse1AddressBO = Factory.NewWithValidTestData<OrgAddress>();
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.DataContext = DataContextFactory.New();
			shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitDispatch, "DC00000001");

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<UniversalOrganizationAddress> { new UniversalOrganizationAddress()
			{
				AddressShortCode = warehouse1AddressBO.AddressCode,
				AddressType = nameof(DocAddressType.LocalCartageCFS),
				OrganizationCode = warehouse1AddressBO.Header.OH_Code
			} });

			var loadList = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			loadList.DataContext = new DataContext();
			loadList.DataContext.AddDataSource(DataContextType.TransitDispatchLoadList, "DLL00000011");
			shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { loadList });

			var packingLine1 = ShipmentDataObjectReaderTest.CreatePackline();
			packingLine1.ReferenceNumber = "S00001557-CTN1";
			packingLine1.OutturnQty = 0;
			packingLine1.SetReferenceNumberCollection(() => new List<UniversalReference>() { new UniversalReference { Type = new UniversalEntryType { Code = "TDC" }, ReferenceNumber = "DC00000001" } });
			var packingLine2 = ShipmentDataObjectReaderTest.CreatePackline();
			packingLine2.ReferenceNumber = "S00001557-CTN2";
			packingLine2.OutturnQty = 1;
			packingLine2.LoadDate = new ZDateTime(2020, 6, 1, 10, 6, 0);
			packingLine2.SetReferenceNumberCollection(() => new List<UniversalReference>() { new UniversalReference { Type = new UniversalEntryType { Code = "TDC" }, ReferenceNumber = "DC00000002" } });
			var packingLine3 = ShipmentDataObjectReaderTest.CreatePackline();
			packingLine3.ReferenceNumber = "S00001557-CTN3";
			packingLine3.OutturnQty = 1;
			packingLine3.LoadDate = new ZDateTime(2020, 6, 1, 10, 6, 0);
			packingLine3.SetReferenceNumberCollection(() => new List<UniversalReference>()
			{
				new UniversalReference { Type = new UniversalEntryType { Code = "TDC" }, ReferenceNumber = "DC00000003" },
				new UniversalReference { Type = new UniversalEntryType { Code = "TDU" }, ReferenceNumber = "TD00000001" }
			});

			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine>() { packingLine1, packingLine2, packingLine3 });
			shipmentDataObject.PackingLineCollection.Content = CollectionContent.Complete;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_LastKnownTransitWarehouseStatusDateTime = new ZDateTime(2020, 6, 1, 10, 1, 0);
			packLine1.PkgPackageCollection.AddNew().KP_PackageID = "S00001557-CTN1";
			packLine1.PkgPackageCollection.AddNew().KP_PackageID = "S00001557-CTN2";
			packLine1.PkgPackageCollection.AddNew().KP_PackageID = "S00001557-CTN3";

			var importedPackages = GetShipmentDataObjectSplitPackageHelper(shipmentDataObject, shipment, warehouse1AddressBO).GetValidImportedPackages();

			AssertContainsExactElementsInAnyOrder("PackingLine1 is not imported", new[] { packingLine2, packingLine3 }, importedPackages);
		}

		public void TestProcessTransitReceive_SplitPacklinesIntoMultipleHandlingUnits()
		{
			var transportationUnits = new List<(ZString Reference, ZDateTime LastKnownTime)> { ("TR00000001", new ZDateTime(2020, 6, 1, 10, 17, 0)) };
			var (warehouse1AddressBO, shipmentDataObject) = SetupShipmentDataObject(DataContextType.TransitReceive, "RC00000001", transportationUnits, ZString.Empty);

			var packingLine1 = CreatePackingLine(string.Empty, 2, "BOX", 400m, "KG", 40m, "M3", "M", "TEST001", "GEN");
			packingLine1.PreviousPackingLineID = "TEST001";
			packingLine1.UnloadDate = new ZDateTime(2020, 6, 3, 10, 17, 0);
			packingLine1.LoadDate = ZDateTime.Empty;
			var packingLine2 = CreatePackingLine(string.Empty, 2, "CTN", 800m, "KG", 80m, "M3", "M", "TEST002", "GEN");
			packingLine2.UnloadDate = new ZDateTime(2020, 6, 4, 10, 17, 0);
			packingLine2.LoadDate = ZDateTime.Empty;
			var packingLine3 = CreatePackingLine(string.Empty, 2, "BOX", 600m, "KG", 60m, "M3", "M", "TEST003", "GEN");
			packingLine3.UnloadDate = new ZDateTime(2020, 6, 3, 10, 17, 0);
			packingLine3.LoadDate = ZDateTime.Empty;
			packingLine3.PreviousPackingLineID = "TEST001";
			var packingLine4 = CreatePackingLine(string.Empty, 2, "CTN", 1200m, "KG", 120m, "M3", "M", "TEST004", "GEN");
			packingLine4.UnloadDate = new ZDateTime(2020, 6, 4, 10, 17, 0);
			packingLine4.LoadDate = ZDateTime.Empty;
			packingLine4.PreviousPackingLineID = "TEST002";
			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine> { packingLine1, packingLine2, packingLine3, packingLine4 });

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = shipment.PK;
			var forwardingPackLine1 = CreatePackLine("REF001", "TEST001", "BOX", FreightConstants.OuterPackType, 4, 1000m, "KG", 100m, "M3", "M", "GEN");
			var forwardingPackLine2 = CreatePackLine("REF001", "TEST002", "CTN", FreightConstants.OuterPackType, 4, 2000m, "KG", 200m, "M3", "M", "GEN");
			shipment.OuterPackLines.Add(forwardingPackLine1);
			shipment.OuterPackLines.Add(forwardingPackLine2);

			var readingHelper = Process(DataContextType.TransitReceive, shipment, warehouse1AddressBO, shipmentDataObject);
			AssertEquals("2 outer packlines", 2, shipment.OuterPackLines.Count);

			var outerPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(l => l.JL_F3_NKPackType == "BOX");
			AssertPackLine(outerPackLine, 4, "BOX", FreightConstants.OuterPackType, 1000m, "KG", 100m, "M3", "M", "GEN", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, new ZDateTime(2020, 6, 3, 10, 17, 0));
			AssertEquals("2 outer BOXs", 2, outerPackLine.PkgPackageCollection.Count);
			var package = outerPackLine.PkgPackageCollection.FirstOrDefault(p => p.KP_ExternalReference == "TEST001");
			AssertPackage(package, string.Empty, "BOX", 2, 400m, "KG", 40m, "M3", "GEN");
			package = outerPackLine.PkgPackageCollection.FirstOrDefault(p => p.KP_ExternalReference == "TEST003");
			AssertPackage(package, string.Empty, "BOX", 2, 600m, "KG", 60m, "M3", "GEN");

			outerPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(l => l.JL_F3_NKPackType == "CTN");
			AssertPackLine(outerPackLine, 4, "CTN", FreightConstants.OuterPackType, 2000m, "KG", 200m, "M3", "M", "GEN", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, new ZDateTime(2020, 6, 4, 10, 17, 0));
			AssertEquals("2 outer CTNs", 2, outerPackLine.PkgPackageCollection.Count);
			package = outerPackLine.PkgPackageCollection.FirstOrDefault(p => p.KP_ExternalReference == "TEST002");
			AssertPackage(package, string.Empty, "CTN", 2, 800m, "KG", 80m, "M3", "GEN");
			package = outerPackLine.PkgPackageCollection.FirstOrDefault(p => p.KP_ExternalReference == "TEST004");
			AssertPackage(package, string.Empty, "CTN", 2, 1200m, "KG", 120m, "M3", "GEN");
		}

		public void TestProcessTransitReceive_TryMatchSpecialHandlingUnits()
		{
			var transportationUnits = new List<(ZString Reference, ZDateTime LastKnownTime)> { ("TR00000001", new ZDateTime(2020, 6, 1, 10, 17, 0)) };

			var (warehouseAddr, shipmentDataObject) = SetupShipmentDataObject(DataContextType.TransitReceive, "RC00000001", transportationUnits, ZString.Empty);
			var packingLine1 = CreatePackingLine("", 10, "PKG", 500m, "KG", 50m, "M3", "M", "TEST003", "GEN");
			packingLine1.UnloadDate = new ZDateTime(2020, 6, 3, 10, 17, 0);
			packingLine1.LoadDate = ZDateTime.Empty;
			packingLine1.PreviousPackingLineID = "TEST001";
			var packingLine2 = CreatePackingLine("", 1, "PKG", 50m, "KG", 5m, "M3", "M", "TEST004", "GEN");
			packingLine2.UnloadDate = new ZDateTime(2020, 6, 3, 10, 17, 0);
			packingLine2.LoadDate = ZDateTime.Empty;
			packingLine2.PreviousPackingLineID = "TEST002";
			var packingLine3 = CreatePackingLine("", 5, "PKG", 250, "KG", 25m, "M3", "M", "TEST005", "GEN");
			packingLine3.UnloadDate = new ZDateTime(2020, 6, 3, 10, 17, 0);
			packingLine3.LoadDate = ZDateTime.Empty;
			packingLine3.PreviousPackingLineID = "TEST001";
			var packingLineCollection = new DataObjectList<UniversalPackingLine> { packingLine1, packingLine2, packingLine3 };
			packingLineCollection.Content = CollectionContent.Complete;
			shipmentDataObject.SetPackingLineCollection(() => packingLineCollection);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = shipment.PK;
			var forwardingPackLine1 = CreatePackLine("REF001", "TEST002", "PKG", FreightConstants.OuterPackType, 2, 50m, "KG", 5m, "M3", "M", "GEN");
			CreateAPackageWithQty(
				packageJob, forwardingPackLine1, 1, warehouseAddr, new ZDateTime(2020, 6, 2, 10, 17, 0),
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies,
				FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
				ZString.Empty, "TEST004", "TEST002");
			var forwardingPackLine2 = CreatePackLine("REF002", "TEST003", "PKG", FreightConstants.OuterPackType, 15, 1250m, "KG", 125m, "M3", "M", "GEN");
			CreateAPackageWithQty(
				packageJob, forwardingPackLine2, 10, warehouseAddr, new ZDateTime(2020, 6, 2, 10, 17, 0),
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies,
				FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
				ZString.Empty, "TEST003", "TEST001");
			CreateAPackageWithQty(
				packageJob, forwardingPackLine2, 5, warehouseAddr, new ZDateTime(2020, 6, 2, 10, 17, 0),
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies,
				FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
				ZString.Empty, "TEST005", "TEST001");
			shipment.OuterPackLines.Add(forwardingPackLine1);
			shipment.OuterPackLines.Add(forwardingPackLine2);

			var readingHelper = Process(DataContextType.TransitReceive, shipment, warehouseAddr, shipmentDataObject);
			AssertEquals(2, shipment.OuterPackLines.Count);
			AssertEquals(1, shipment.OuterPackLines[0].PkgPackageCollection.Count);
			AssertEquals("TEST004", shipment.OuterPackLines[0].PkgPackageCollection[0].KP_ExternalReference);
			AssertEquals("TEST002", shipment.OuterPackLines[0].PkgPackageCollection[0].KP_PreviousPackLineID);
			AssertEquals(2, shipment.OuterPackLines[1].PkgPackageCollection.Count);
			AssertEquals("TEST003", shipment.OuterPackLines[1].PkgPackageCollection[0].KP_ExternalReference);
			AssertEquals("TEST001", shipment.OuterPackLines[1].PkgPackageCollection[0].KP_PreviousPackLineID);
			AssertEquals("TEST005", shipment.OuterPackLines[1].PkgPackageCollection[1].KP_ExternalReference);
			AssertEquals("TEST001", shipment.OuterPackLines[1].PkgPackageCollection[1].KP_PreviousPackLineID);
		}

		public void TestProcessTransitReceive_WithHandlingUnitsRelabelled()
		{
			var transportationUnits = new List<(ZString Reference, ZDateTime LastKnownTime)> { ("TR00000001", new ZDateTime(2020, 6, 1, 10, 17, 0)) };

			var originWarehouseAddr = Factory.NewWithValidTestData<OrgAddress>();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = shipment.PK;
			var forwardingPackLine = CreatePackLine("REF001", "TEST001", "CTN", FreightConstants.OuterPackType, 2, 1000m, "KG", 100m, "M3", "M", "GEN");
			CreateAPackageWithQty(
				packageJob, forwardingPackLine, 2, originWarehouseAddr, new ZDateTime(2020, 6, 2, 10, 17, 0),
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
				FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched,
				ZString.Empty, "TEST002", "TEST001");
			shipment.OuterPackLines.Add(forwardingPackLine);

			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertEquals("TEST001", shipment.OuterPackLines[0].JL_PackLineId);
			AssertEquals(1, shipment.OuterPackLines[0].PkgPackageCollection.Count);
			AssertEquals(originWarehouseAddr.PK, shipment.OuterPackLines[0].JL_OA_LastKnownTransitWarehouseAddress);

			var (destinationWareHouseAddr, shipmentDataObject) = SetupShipmentDataObject(DataContextType.TransitReceive, "RC00000001", transportationUnits, ZString.Empty);
			var packingLine1 = CreatePackingLine("RC00000001-001", 1, "CTN", 500m, "KG", 50m, "M3", "M", "TEST002", "GEN");
			packingLine1.UnloadDate = new ZDateTime(2020, 6, 3, 10, 17, 0);
			packingLine1.LoadDate = ZDateTime.Empty;
			packingLine1.PreviousPackingLineID = "TEST001";
			var packingLine2 = CreatePackingLine("RC00000001-002", 1, "CTN", 500m, "KG", 50m, "M3", "M", "TEST002", "GEN");
			packingLine2.UnloadDate = new ZDateTime(2020, 6, 3, 10, 17, 0);
			packingLine2.LoadDate = ZDateTime.Empty;
			packingLine2.PreviousPackingLineID = "TEST001";
			var packingLineCollection = new DataObjectList<UniversalPackingLine> { packingLine1, packingLine2 };
			packingLineCollection.Content = CollectionContent.Complete;
			shipmentDataObject.SetPackingLineCollection(() => packingLineCollection);

			var readingHelper = Process(DataContextType.TransitReceive, shipment, destinationWareHouseAddr, shipmentDataObject);
			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertEquals("TEST001", shipment.OuterPackLines[0].JL_PackLineId);
			AssertEquals(2, shipment.OuterPackLines[0].PkgPackageCollection.Count);
			AssertEquals("RC00000001-001", shipment.OuterPackLines[0].PkgPackageCollection[0].KP_PackageID);
			AssertEquals("RC00000001-002", shipment.OuterPackLines[0].PkgPackageCollection[1].KP_PackageID);
		}

		public void TestProcessTransitReceive_PackLineTWExcluded_DuplicatePackages_ImportFailed()
		{
			var warehouse1AddressBO = Factory.NewWithValidTestData<OrgAddress>();

			var shipmentDO = PrepareUniversalShipment(warehouse1AddressBO, DataContextType.TransitReceive);
			var packingLineCollection = new DataObjectList<UniversalPackingLine>();
			packingLineCollection.Content = CollectionContent.Complete;
			for (var i = 1; i <= 2; i++)
			{
				var packline = new UniversalPackingLine(DefaultDataObjectWriterStrategy.TestInstance);
				packline.ReferenceNumber = "PKG" + i;
				packline.PackingLineID = "TEST001";
				packline.PackQty = 1;
				packline.OutturnQty = 1;
				packline.UnloadDate = new ZDateTime(2020, 6, 1, 10, 7, 0);
				packline.GoodsDescription = "Goods 123";
				packline.PackType = new PackageType() { Code = Core.Constants.PkgUnit.Pallet };
				packline.LengthUnit = new UnitOfLength() { Code = Core.Constants.Length.Metres };
				packline.WeightUnit = new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms };
				packline.VolumeUnit = new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres };
				packline.SetReferenceNumberCollection(() => new List<UniversalReference>() { new UniversalReference { Type = new UniversalEntryType { Code = "TRU" }, ReferenceNumber = "TR00000001" } });
				packingLineCollection.Add(packline);
			}

			shipmentDO.SetPackingLineCollection(() => packingLineCollection);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_OA_ExportReceivingDepot = warehouse1AddressBO.PK;
			var forwardingPackLine = Factory.New<ForwardingPackLine>();
			forwardingPackLine.JL_PackageCount = 2;
			forwardingPackLine.JL_RefNumber = ZString.Empty;
			forwardingPackLine.JL_PackLineId = "TEST001";
			forwardingPackLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			forwardingPackLine.JL_Description = "Goods 123";
			forwardingPackLine.JL_FreightMode = FreightConstants.OuterPackType;
			shipment.OuterPackLines.Add(forwardingPackLine);

			var helper = GetShipmentDataObjectSplitPackageHelper(shipmentDO, shipment, warehouse1AddressBO);
			AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown, forwardingPackLine.JL_OriginTransitWarehouseStatus);
			helper.ProcessTransitReceive();
			var currentPackeLineBO = shipment.OuterPackLines.Cast<ForwardingPackLine>().SingleOrDefault();
			AssertNotNull("Still has 1 Packline", currentPackeLineBO);

			AssertPackLineInformations(currentPackeLineBO,
				warehouse1AddressBO.PK,
				FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
				new ZDateTime(2020, 6, 1, 10, 7, 0),
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
				new ZString[] { "PKG1", "PKG2" });

			Factory.SaveForTesting();

			forwardingPackLine.JL_DepartureTransitWarehouseExcluded = true;
			shipment.OuterPackLines.RemoveAll();
			shipment.OuterPackLines.Add(forwardingPackLine);

			helper = GetShipmentDataObjectSplitPackageHelper(shipmentDO, shipment, warehouse1AddressBO);

			var exception = AssertExceptionThrown<DataObjectReadFailureException>(() => helper.ProcessTransitReceive());
			AssertEquals($"Package is already attached to an excluded pack line. See Package ID: PKG1, Pack Line ID: TEST001, Shipment ID: {shipment.JS_UniqueConsignRef}", exception.Message);
		}

		#region Overpack

		public void TestOverpack_TransitReceive_WithMultipleShipmentPackLines()
		{
			TestOverpack_WithMultipleShipmentPackLines(DataContextType.TransitReceive, "RC00000001", "TRU", "TR00000001", ZString.Empty);
		}

		public void TestOverpack_TransitReceive_WithMixedShipmentPackLines()
		{
			TestOverpack_WithMixedShipmentPackLines(DataContextType.TransitReceive, "RC00000001", "TRU", "TR00000001", ZString.Empty);
		}

		public void TestOverpack_TransitReceive_OfAShipmentWhichAlreadyHadInnersAndOutersBeforeTheOverpack()
		{
			TestOverpack_OfAShipmentWhichAlreadyHadInnersAndOutersBeforeTheOverpack(DataContextType.TransitReceive, "RC00000001", "TRU", "TR00000001", ZString.Empty);
		}

		public void TestOverpack_TransitDispatch_WithMultipleShipmentPackLines()
		{
			TestOverpack_WithMultipleShipmentPackLines(DataContextType.TransitDispatch, "DC00000001", "TDU", "TD00000001", "DLL00000011");
		}

		public void TestOverpack_TransitDispatch_WithMixedShipmentPackLines()
		{
			TestOverpack_WithMixedShipmentPackLines(DataContextType.TransitDispatch, "DC00000001", "TDU", "TD00000001", "DLL00000011");
		}

		public void TestOverpack_TransitDispatch_OfAShipmentWhichAlreadyHadInnersAndOutersBeforeTheOverpack()
		{
			TestOverpack_OfAShipmentWhichAlreadyHadInnersAndOutersBeforeTheOverpack(DataContextType.TransitDispatch, "DC00000001", "TDU", "TD00000001", "DLL00000011");
		}

		public void TestOverpack_TransitPrepareDispatch_WithMultipleShipmentPackLines()
		{
			TestOverpack_WithMultipleShipmentPackLines(DataContextType.TransitDispatch, "DC00000001", "TDU", "TD00000001", ZString.Empty);
		}

		public void TestOverpack_TransitPrepareDispatch_WithMixedShipmentPackLines()
		{
			TestOverpack_WithMixedShipmentPackLines(DataContextType.TransitDispatch, "DC00000001", "TDU", "TD00000001", ZString.Empty);
		}

		public void TestOverpack_TransitPrepareDispatch_OfAShipmentWhichAlreadyHadInnersAndOutersBeforeTheOverpack()
		{
			TestOverpack_OfAShipmentWhichAlreadyHadInnersAndOutersBeforeTheOverpack(DataContextType.TransitDispatch, "DC00000001", "TDU", "TD00000001", ZString.Empty);
		}

		void TestOverpack_WithMultipleShipmentPackLines(DataContextType consignmentType, ZString consignmentReference, ZString transportationUnitType, ZString transportationUnitReference, ZString dispatchLoadListReference)
		{
			DGSubstanceTestHelper.Create("1234", "a", "IMO");
			DGSubstanceTestHelper.Create("1234", "b", "IMO");
			var substance1 = UNDGSubstanceLoader.LoadSubstances(Factory.BOFactory, "1234", "a", "IMO").First();
			var substance2 = UNDGSubstanceLoader.LoadSubstances(Factory.BOFactory, "1234", "b", "IMO").First();

			var transportationUnits = new List<(ZString Reference, ZDateTime LastKnownTime)> { (transportationUnitReference, new ZDateTime(2020, 6, 1, 10, 17, 0)) };
			var (warehouse1AddressBO, shipmentDataObject) = SetupShipmentDataObject(consignmentType, consignmentReference, transportationUnits, dispatchLoadListReference);

			var packingLineCollection1 = new List<UniversalPackingLine>();
			for (var i = 1; i <= 1; i++)
			{
				var packingLineInner = CreatePackingLine("S00001000-BOX" + i, 1, "BOX", 100m, "KG", 10m, "M3", "M", "TEST001", "GEN");
				SetLastKnownTransitWarehouseStatusDateTime(packingLineInner, consignmentType, dispatchLoadListReference, new ZDateTime(2020, 6, 1, 10, 17, 0));
				SetReferenceNumber(packingLineInner, transportationUnitType, transportationUnitReference);
				packingLineInner.SetUNDGCollection(() => new List<UNDG> { CreateUNDG(substance1.DG_Code, i) });
				packingLineCollection1.Add(packingLineInner);
			}
			for (var i = 1; i <= 2; i++)
			{
				var packingLineInner = CreatePackingLine("S00001000-CTN" + i, 1, "CTN", 200m, "KG", 20m, "M3", "M", "TEST002", "GEN");
				SetLastKnownTransitWarehouseStatusDateTime(packingLineInner, consignmentType, dispatchLoadListReference, new ZDateTime(2020, 6, 1, 10, 17, 0));
				SetReferenceNumber(packingLineInner, transportationUnitType, transportationUnitReference);
				packingLineInner.SetUNDGCollection(() => new List<UNDG> { CreateUNDG(substance2.DG_Code, i) });
				packingLineCollection1.Add(packingLineInner);
			}
			var packingLine1 = CreatePackingLine("OVP00001", 1, "PLT", 3000m, "KG", 300m, "M3", "M", null, "GEN");
			SetLastKnownTransitWarehouseStatusDateTime(packingLine1, consignmentType, dispatchLoadListReference, new ZDateTime(2020, 6, 1, 10, 17, 0));
			packingLine1.SetPackingLineCollection(() => packingLineCollection1);

			var packingLineCollection2 = new List<UniversalPackingLine>();
			for (var i = 1; i <= 3; i++)
			{
				var packingLineInner = CreatePackingLine("S00001000-CAS" + i, 1, "CAS", 300m, "KG", 30m, "M3", "M", "TEST003", "GEN");
				SetLastKnownTransitWarehouseStatusDateTime(packingLineInner, consignmentType, dispatchLoadListReference, new ZDateTime(2020, 6, 1, 10, 17, 0));
				SetReferenceNumber(packingLineInner, transportationUnitType, transportationUnitReference);
				packingLineInner.SetUNDGCollection(() => new List<UNDG> { CreateUNDG(substance1.DG_Code, i) });
				packingLineCollection2.Add(packingLineInner);
			}
			for (var i = 1; i <= 4; i++)
			{
				var packingLineInner = CreatePackingLine("S00001000-TUB" + i, 1, "TUB", 400m, "KG", 40m, "M3", "M", "TEST004", "GEN");
				SetLastKnownTransitWarehouseStatusDateTime(packingLineInner, consignmentType, dispatchLoadListReference, new ZDateTime(2020, 6, 1, 10, 17, 0));
				SetReferenceNumber(packingLineInner, transportationUnitType, transportationUnitReference);
				packingLineInner.SetUNDGCollection(() => new List<UNDG> { CreateUNDG(substance2.DG_Code, i) });
				packingLineCollection2.Add(packingLineInner);
			}
			var packingLine2 = CreatePackingLine("OVP00002", 1, "PLT", 4000m, "KG", 400m, "M3", "M", null, "GEN");
			SetLastKnownTransitWarehouseStatusDateTime(packingLine2, consignmentType, dispatchLoadListReference, new ZDateTime(2020, 6, 1, 10, 17, 0));
			packingLine2.SetPackingLineCollection(() => packingLineCollection2);
			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine> { packingLine1, packingLine2 });

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = shipment.PK;
			var forwardingPackLine1 = CreatePackLine("REF001", "TEST001", "BOX", FreightConstants.OuterPackType, 1, 100m, "KG", 10m, "M3", "M", "GEN");
			CreatePackages(
				forwardingPackLine1, 1, warehouse1AddressBO, new ZDateTime(2020, 6, 1, 6, 17, 0),
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
				FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
				"S00001000-BOX", packageJob);
			var forwardingPackLine2 = CreatePackLine("REF002", "TEST002", "CTN", FreightConstants.OuterPackType, 2, 400m, "KG", 40m, "M3", "M", "GEN");
			CreatePackages(
				forwardingPackLine2, 2, warehouse1AddressBO, new ZDateTime(2020, 6, 1, 7, 17, 0),
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
				FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
				"S00001000-CTN", packageJob);
			var forwardingPackLine3 = CreatePackLine("REF003", "TEST003", "CAS", FreightConstants.OuterPackType, 3, 900m, "KG", 90m, "M3", "M", "GEN");
			CreatePackages(
				forwardingPackLine3, 3, warehouse1AddressBO, new ZDateTime(2020, 6, 1, 8, 17, 0),
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
				FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
				"S00001000-CAS", packageJob);
			var forwardingPackLine4 = CreatePackLine("REF004", "TEST004", "TUB", FreightConstants.OuterPackType, 4, 1600m, "KG", 160m, "M3", "M", "GEN");
			CreatePackages(
				forwardingPackLine4, 4, warehouse1AddressBO, new ZDateTime(2020, 6, 1, 9, 17, 0),
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
				FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
				"S00001000-TUB", packageJob);
			shipment.OuterPackLines.Add(forwardingPackLine1);
			shipment.OuterPackLines.Add(forwardingPackLine2);
			shipment.OuterPackLines.Add(forwardingPackLine3);
			shipment.OuterPackLines.Add(forwardingPackLine4);

			var readingHelper = Process(consignmentType, shipment, warehouse1AddressBO, shipmentDataObject);
			AssertEquals("1 outer packline", 1, shipment.OuterPackLines.Count);
			var outerPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(l => l.JL_RH_NKCommodityCode == "GEN");
			AssertPackLine(
				outerPackLine, 2, "PLT", FreightConstants.OuterPackType, 7000m, "KG", 700m, "M3", "M", "GEN",
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, new ZDateTime(2020, 6, 1, 10, 17, 0));

			AssertEquals("2 outer packages", 2, outerPackLine.PkgPackageCollection.Count);
			var package = outerPackLine.PkgPackageCollection.FirstOrDefault(p => p.KP_PackageID == "OVP00001");
			AssertPackage(package, "OVP00001", "PLT", 1, 3000m, "KG", 300m, "M3", "GEN");
			package = outerPackLine.PkgPackageCollection.FirstOrDefault(p => p.KP_PackageID == "OVP00002");
			AssertPackage(package, "OVP00002", "PLT", 1, 4000m, "KG", 400m, "M3", "GEN");

			AssertEquals("4 inner packlines", 4, outerPackLine.InnerPackLines.Count);
			var innerPackLine = outerPackLine.InnerPackLines.FirstOrDefault(l => l.JL_F3_NKPackType == "BOX");
			AssertPackLine(innerPackLine, 1, "BOX", FreightConstants.InnerPackType, 100m, "KG", 10m, "M3", "M", "GEN", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown, ZDateTime.Empty);
			innerPackLine = outerPackLine.InnerPackLines.FirstOrDefault(l => l.JL_F3_NKPackType == "CTN");
			AssertPackLine(innerPackLine, 2, "CTN", FreightConstants.InnerPackType, 400m, "KG", 40m, "M3", "M", "GEN", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown, ZDateTime.Empty);
			innerPackLine = outerPackLine.InnerPackLines.FirstOrDefault(l => l.JL_F3_NKPackType == "CAS");
			AssertPackLine(innerPackLine, 3, "CAS", FreightConstants.InnerPackType, 900m, "KG", 90m, "M3", "M", "GEN", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown, ZDateTime.Empty);
			innerPackLine = outerPackLine.InnerPackLines.FirstOrDefault(l => l.JL_F3_NKPackType == "TUB");
			AssertPackLine(innerPackLine, 4, "TUB", FreightConstants.InnerPackType, 1600m, "KG", 160m, "M3", "M", "GEN", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown, ZDateTime.Empty);

			AssertEquals("2 undgs", 2, outerPackLine.UNDGs.Count);
			var undg = outerPackLine.UNDGs.FirstOrDefault(g => g.DI_DG == substance1.PK);
			AssertUNDG(undg, substance1, 7, true);
			undg = outerPackLine.UNDGs.FirstOrDefault(g => g.DI_DG == substance2.PK);
			AssertUNDG(undg, substance2, 13, true);

			AssertShipment(shipment, 2, "PLT", 7000m, "KG", 700m, "M3", 0m, 10, "PKG");
		}

		void TestOverpack_WithMixedShipmentPackLines(DataContextType consignmentType, ZString consignmentReference, ZString transportationUnitType, ZString transportationUnitReference, ZString dispatchLoadListReference)
		{
			var transportationUnits = new List<(ZString Reference, ZDateTime LastKnownTime)> { (transportationUnitReference, new ZDateTime(2020, 6, 1, 10, 17, 0)) };
			var (warehouse1AddressBO, shipmentDataObject) = SetupShipmentDataObject(consignmentType, consignmentReference, transportationUnits, dispatchLoadListReference);

			var packingLineCollection1 = new List<UniversalPackingLine>();
			for (var i = 1; i <= 2; i++)
			{
				var packingLineInner = CreatePackingLine("S00001000-BOX" + i, 1, "BOX", 100m, "KG", 10m, "M3", "M", "TEST001", "GEN");
				SetLastKnownTransitWarehouseStatusDateTime(packingLineInner, consignmentType, dispatchLoadListReference, new ZDateTime(2020, 6, 1, 10, 17, 0));
				SetReferenceNumber(packingLineInner, transportationUnitType, transportationUnitReference);
				packingLineCollection1.Add(packingLineInner);
			}
			for (var i = 1; i <= 2; i++)
			{
				var packingLineInner = CreatePackingLine("S00001000-CTN" + i, 1, "CTN", 200m, "KG", 20m, "M3", "M", "TEST002", "GEN");
				SetLastKnownTransitWarehouseStatusDateTime(packingLineInner, consignmentType, dispatchLoadListReference, new ZDateTime(2020, 6, 1, 10, 17, 0));
				SetReferenceNumber(packingLineInner, transportationUnitType, transportationUnitReference);
				packingLineCollection1.Add(packingLineInner);
			}
			var packingLine1 = CreatePackingLine("OVP00001", 1, "PLT", 3000m, "KG", 300m, "M3", "M", null, "GEN");
			SetLastKnownTransitWarehouseStatusDateTime(packingLine1, consignmentType, dispatchLoadListReference, new ZDateTime(2020, 6, 1, 10, 17, 0));
			packingLine1.SetPackingLineCollection(() => packingLineCollection1);

			var packingLineCollection2 = new List<UniversalPackingLine>();
			for (var i = 3; i <= 4; i++)
			{
				var packingLineInner = CreatePackingLine("S00001000-BOX" + i, 1, "BOX", 100m, "KG", 10m, "M3", "M", "TEST001", "CRMC");
				SetLastKnownTransitWarehouseStatusDateTime(packingLineInner, consignmentType, dispatchLoadListReference, new ZDateTime(2020, 6, 1, 10, 17, 0));
				SetReferenceNumber(packingLineInner, transportationUnitType, transportationUnitReference);
				packingLineCollection2.Add(packingLineInner);
			}
			for (var i = 3; i <= 4; i++)
			{
				var packingLineInner = CreatePackingLine("S00001000-CTN" + i, 1, "CTN", 200m, "KG", 20m, "M3", "M", "TEST002", "CRMC");
				SetLastKnownTransitWarehouseStatusDateTime(packingLineInner, consignmentType, dispatchLoadListReference, new ZDateTime(2020, 6, 1, 10, 17, 0));
				SetReferenceNumber(packingLineInner, transportationUnitType, transportationUnitReference);
				packingLineCollection2.Add(packingLineInner);
			}
			var packingLine2 = CreatePackingLine("OVP00002", 1, "PLT", 4000m, "KG", 400m, "M3", "M", null, "CRMC");
			SetLastKnownTransitWarehouseStatusDateTime(packingLine2, consignmentType, dispatchLoadListReference, new ZDateTime(2020, 6, 1, 10, 17, 0));
			packingLine2.SetPackingLineCollection(() => packingLineCollection2);
			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine> { packingLine1, packingLine2 });

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = shipment.PK;
			var forwardingPackLine1 = CreatePackLine("REF001", "TEST001", "BOX", FreightConstants.OuterPackType, 4, 400m, "KG", 40m, "M3", "M", "GEN");
			CreatePackages(
				forwardingPackLine1, 4, warehouse1AddressBO, ZDateTime.Empty,
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
				FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
				"S00001000-BOX", packageJob);
			var forwardingPackLine2 = CreatePackLine("REF002", "TEST002", "CTN", FreightConstants.OuterPackType, 4, 800m, "KG", 80m, "M3", "M", "GEN");
			CreatePackages(
				forwardingPackLine2, 4, warehouse1AddressBO, new ZDateTime(2020, 6, 1, 9, 17, 0),
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
				FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
				"S00001000-CTN", packageJob);
			shipment.OuterPackLines.Add(forwardingPackLine1);
			shipment.OuterPackLines.Add(forwardingPackLine2);

			var readingHelper = Process(consignmentType, shipment, warehouse1AddressBO, shipmentDataObject);
			AssertEquals("2 outer packlines", 2, shipment.OuterPackLines.Count);

			var outerPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(l => l.JL_RH_NKCommodityCode == "GEN");
			AssertPackLine(
				outerPackLine, 1, "PLT", FreightConstants.OuterPackType, 3000m, "KG", 300m, "M3", "M", "GEN",
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, new ZDateTime(2020, 6, 1, 10, 17, 0));
			AssertEquals("1 outer package", 1, outerPackLine.PkgPackageCollection.Count);
			var package = outerPackLine.PkgPackageCollection.FirstOrDefault(p => p.KP_PackageID == "OVP00001");
			AssertPackage(package, "OVP00001", "PLT", 1, 3000m, "KG", 300m, "M3", "GEN");
			AssertEquals("2 inner packlines", 2, outerPackLine.InnerPackLines.Count);
			var innerPackLine = outerPackLine.InnerPackLines.FirstOrDefault(l => l.JL_F3_NKPackType == "BOX");
			AssertPackLine(innerPackLine, 2, "BOX", FreightConstants.InnerPackType, 200m, "KG", 20m, "M3", "M", "GEN", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown, ZDateTime.Empty);
			innerPackLine = outerPackLine.InnerPackLines.FirstOrDefault(l => l.JL_F3_NKPackType == "CTN");
			AssertPackLine(innerPackLine, 2, "CTN", FreightConstants.InnerPackType, 400m, "KG", 40m, "M3", "M", "GEN", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown, ZDateTime.Empty);

			outerPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(l => l.JL_RH_NKCommodityCode == "CRMC");
			AssertPackLine(
				outerPackLine, 1, "PLT", FreightConstants.OuterPackType, 4000m, "KG", 400m, "M3", "M", "CRMC",
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, new ZDateTime(2020, 6, 1, 10, 17, 0));
			AssertEquals("1 outer package", 1, outerPackLine.PkgPackageCollection.Count);
			package = outerPackLine.PkgPackageCollection.FirstOrDefault(p => p.KP_PackageID == "OVP00002");
			AssertPackage(package, "OVP00002", "PLT", 1, 4000m, "KG", 400m, "M3", "CRMC");
			AssertEquals("2 inner packlines", 2, outerPackLine.InnerPackLines.Count);
			innerPackLine = outerPackLine.InnerPackLines.FirstOrDefault(l => l.JL_F3_NKPackType == "BOX");
			AssertPackLine(innerPackLine, 2, "BOX", FreightConstants.InnerPackType, 200m, "KG", 20m, "M3", "M", "CRMC", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown, ZDateTime.Empty);
			innerPackLine = outerPackLine.InnerPackLines.FirstOrDefault(l => l.JL_F3_NKPackType == "CTN");
			AssertPackLine(innerPackLine, 2, "CTN", FreightConstants.InnerPackType, 400m, "KG", 40m, "M3", "M", "CRMC", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown, ZDateTime.Empty);

			AssertShipment(shipment, 2, "PLT", 7000m, "KG", 700m, "M3", 0m, 8, "PKG");
		}

		void TestOverpack_OfAShipmentWhichAlreadyHadInnersAndOutersBeforeTheOverpack(DataContextType consignmentType, ZString consignmentReference, ZString transportationUnitType, ZString transportationUnitReference, ZString dispatchLoadListReference)
		{
			var transportationUnits = new List<(ZString Reference, ZDateTime LastKnownTime)> { (transportationUnitReference, new ZDateTime(2020, 6, 1, 10, 17, 0)) };
			var (warehouse1AddressBO, shipmentDataObject) = SetupShipmentDataObject(consignmentType, consignmentReference, transportationUnits, dispatchLoadListReference);

			var packingLineCollection1 = new List<UniversalPackingLine>();
			for (var i = 1; i <= 2; i++)
			{
				var packingLineInner = CreatePackingLine("S00001000-CTN" + i, 1, "CTN", 600m, "KG", 60m, "M3", "M", "TEST004", "GEN");
				SetLastKnownTransitWarehouseStatusDateTime(packingLineInner, consignmentType, dispatchLoadListReference, new ZDateTime(2020, 6, 1, 10, 17, 0));
				SetReferenceNumber(packingLineInner, transportationUnitType, transportationUnitReference);
				packingLineCollection1.Add(packingLineInner);
			}
			var packingLine1 = CreatePackingLine("OVP00001", 1, "PLT", 3000m, "KG", 300m, "M3", "M", null, "GEN");
			SetLastKnownTransitWarehouseStatusDateTime(packingLine1, consignmentType, dispatchLoadListReference, new ZDateTime(2020, 6, 1, 10, 17, 0));
			packingLine1.SetPackingLineCollection(() => packingLineCollection1);

			var packingLineCollection2 = new List<UniversalPackingLine>();
			for (var i = 3; i <= 5; i++)
			{
				var packingLineInner = CreatePackingLine("S00001000-CTN" + i, 1, "CTN", 600m, "KG", 60m, "M3", "M", "TEST004", "GEN");
				SetLastKnownTransitWarehouseStatusDateTime(packingLineInner, consignmentType, dispatchLoadListReference, new ZDateTime(2020, 6, 1, 10, 17, 0));
				SetReferenceNumber(packingLineInner, transportationUnitType, transportationUnitReference);
				packingLineCollection2.Add(packingLineInner);
			}
			var packingLine2 = CreatePackingLine("OVP00002", 1, "PLT", 4000m, "KG", 400m, "M3", "M", null, "GEN");
			SetLastKnownTransitWarehouseStatusDateTime(packingLine2, consignmentType, dispatchLoadListReference, new ZDateTime(2020, 6, 1, 10, 17, 0));
			packingLine2.SetPackingLineCollection(() => packingLineCollection2);
			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine> { packingLine1, packingLine2 });

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = shipment.PK;
			var forwardingPackLine = CreatePackLine("REF004", "TEST004", "CTN", FreightConstants.OuterPackType, 5, 3000m, "KG", 300m, "M3", "M", "GEN");
			CreatePackages(
				forwardingPackLine, 5, warehouse1AddressBO, ZDateTime.Empty,
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
				FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
				"S00001000-CTN", packageJob);
			var forwardingPackLineInner1 = CreatePackLine("REF001", "TEST001", "BOX", FreightConstants.InnerPackType, 3, 300m, "KG", 30m, "M3", "M", "GEN", forwardingPackLine);
			var forwardingPackLineInner2 = CreatePackLine("REF002", "TEST002", "BAG", FreightConstants.InnerPackType, 3, 600m, "KG", 60m, "M3", "M", "GEN", forwardingPackLine);
			var forwardingPackLineInner3 = CreatePackLine("REF003", "TEST003", "CTN", FreightConstants.InnerPackType, 3, 900m, "KG", 90m, "M3", "M", "GEN", forwardingPackLine);
			shipment.OuterPackLines.Add(forwardingPackLine);
			shipment.InnerPackLines.Add(forwardingPackLineInner1);
			shipment.InnerPackLines.Add(forwardingPackLineInner2);
			shipment.InnerPackLines.Add(forwardingPackLineInner3);

			var readingHelper = Process(consignmentType, shipment, warehouse1AddressBO, shipmentDataObject);
			AssertEquals("1 outer packline", 1, shipment.OuterPackLines.Count);
			var outerPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(l => l.JL_RH_NKCommodityCode == "GEN");
			AssertPackLine(
				outerPackLine, 2, "PLT", FreightConstants.OuterPackType, 7000m, "KG", 700m, "M3", "M", "GEN",
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, new ZDateTime(2020, 6, 1, 10, 17, 0));

			AssertEquals("2 outer packages", 2, outerPackLine.PkgPackageCollection.Count);
			var package = outerPackLine.PkgPackageCollection.FirstOrDefault(p => p.KP_PackageID == "OVP00001");
			AssertPackage(package, "OVP00001", "PLT", 1, 3000m, "KG", 300m, "M3", "GEN");
			package = outerPackLine.PkgPackageCollection.FirstOrDefault(p => p.KP_PackageID == "OVP00002");
			AssertPackage(package, "OVP00002", "PLT", 1, 4000m, "KG", 400m, "M3", "GEN");

			AssertEquals("1 inner packline", 1, outerPackLine.InnerPackLines.Count);
			var innerPackLine = outerPackLine.InnerPackLines.FirstOrDefault(l => l.JL_F3_NKPackType == "CTN");
			AssertPackLine(innerPackLine, 5, "CTN", FreightConstants.InnerPackType, 3000m, "KG", 300m, "M3", "M", "GEN", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown, ZDateTime.Empty);

			AssertShipment(shipment, 2, "PLT", 7000m, "KG", 700m, "M3", 0m, 5, "CTN");
		}

		public void TestOverpack_TransitReceive_OfAShipmentWhichAlreadyHadInnersAndOuters_ReceiveOutersOnly()
		{
			var transportationUnits = new List<(ZString Reference, ZDateTime LastKnownTime)> { ("TR00000001", new ZDateTime(2020, 6, 1, 10, 17, 0)) };
			var (warehouse1AddressBO, shipmentDataObject) = SetupShipmentDataObject(DataContextType.TransitReceive, "RC00000001", transportationUnits, ZString.Empty);

			var packingLineCollection1 = new List<UniversalPackingLine>();
			for (var i = 1; i <= 2; i++)
			{
				var packingLineInner = CreatePackingLine(null, 1, "CTN", 600m, "KG", 60m, "M3", "M", null, "GEN");
				packingLineInner.UnloadDate = new ZDateTime(2020, 6, 1, 10, 17, 0);
				SetReferenceNumber(packingLineInner, "TRU", "TR00000001");
				packingLineCollection1.Add(packingLineInner);
			}
			var packingLine1 = CreatePackingLine("PLT00001", 1, "PLT", 3000m, "KG", 300m, "M3", "M", "TEST003", "GEN");
			packingLine1.UnloadDate = new ZDateTime(2020, 6, 1, 10, 17, 0);
			packingLine1.SetPackingLineCollection(() => packingLineCollection1);

			var packingLineCollection2 = new List<UniversalPackingLine>();
			for (var i = 3; i <= 5; i++)
			{
				var packingLineInner = CreatePackingLine(null, 1, "BAG", 600m, "KG", 60m, "M3", "M", null, "GEN");
				packingLineInner.UnloadDate = new ZDateTime(2020, 6, 1, 10, 17, 0);
				SetReferenceNumber(packingLineInner, "TRU", "TR00000001");
				packingLineCollection2.Add(packingLineInner);
			}
			var packingLine2 = CreatePackingLine("PLT00002", 1, "PLT", 4000m, "KG", 400m, "M3", "M", "TEST003", "GEN");
			packingLine2.UnloadDate = new ZDateTime(2020, 6, 1, 10, 17, 0);
			packingLine2.SetPackingLineCollection(() => packingLineCollection2);
			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine> { packingLine1, packingLine2 });

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = shipment.PK;
			var forwardingPackLine = CreatePackLine("REF003", "TEST003", "PLT", FreightConstants.OuterPackType, 2, 7000m, "KG", 700m, "M3", "M", "GEN");
			var forwardingPackLineInner1 = CreatePackLine("REF001", "TEST001", "CTN", FreightConstants.InnerPackType, 2, 300m, "KG", 30m, "M3", "M", "GEN", forwardingPackLine);
			var forwardingPackLineInner2 = CreatePackLine("REF002", "TEST002", "BAG", FreightConstants.InnerPackType, 3, 600m, "KG", 60m, "M3", "M", "GEN", forwardingPackLine);
			shipment.OuterPackLines.Add(forwardingPackLine);
			shipment.InnerPackLines.Add(forwardingPackLineInner1);
			shipment.InnerPackLines.Add(forwardingPackLineInner2);

			var readingHelper = Process(DataContextType.TransitReceive, shipment, warehouse1AddressBO, shipmentDataObject);
			AssertEquals("1 outer packline", 1, shipment.OuterPackLines.Count);
			var outerPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(l => l.JL_RH_NKCommodityCode == "GEN");
			AssertPackLine(outerPackLine, 2, "PLT", FreightConstants.OuterPackType, 7000m, "KG", 700m, "M3", "M", "GEN", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, new ZDateTime(2020, 6, 1, 10, 17, 0));

			AssertEquals("2 outer packages", 2, outerPackLine.PkgPackageCollection.Count);
			var package = outerPackLine.PkgPackageCollection.FirstOrDefault(p => p.KP_PackageID == "PLT00001");
			AssertPackage(package, "PLT00001", "PLT", 1, 3000m, "KG", 300m, "M3", "GEN");
			package = outerPackLine.PkgPackageCollection.FirstOrDefault(p => p.KP_PackageID == "PLT00002");
			AssertPackage(package, "PLT00002", "PLT", 1, 4000m, "KG", 400m, "M3", "GEN");

			AssertEquals("2 inner packlines", 2, outerPackLine.InnerPackLines.Count);
			var innerPackLine = outerPackLine.InnerPackLines.FirstOrDefault(l => l.JL_F3_NKPackType == "CTN");
			AssertPackLine(innerPackLine, 2, "CTN", FreightConstants.InnerPackType, 1200m, "KG", 120m, "M3", "M", "GEN", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown, ZDateTime.Empty);
			innerPackLine = outerPackLine.InnerPackLines.FirstOrDefault(l => l.JL_F3_NKPackType == "BAG");
			AssertPackLine(innerPackLine, 3, "BAG", FreightConstants.InnerPackType, 1800m, "KG", 180m, "M3", "M", "GEN", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown, ZDateTime.Empty);
		}

		public void TestOverpack_TransitReceive_NotAllPackagesAreOverpacked()
		{
			TestOverpack_NotAllPackagesAreOverpacked(DataContextType.TransitReceive, "RC00000001", "TRU", "TR00000001", ZString.Empty);
		}

		public void TestOverpack_TransitDispatch_NotAllPackagesAreOverpacked()
		{
			TestOverpack_NotAllPackagesAreOverpacked(DataContextType.TransitDispatch, "DC00000001", "TDU", "TD00000001", "DLL00000011");
		}

		public void TestOverpack_TransitPrepareDispatch_NotAllPackagesAreOverpacked()
		{
			TestOverpack_NotAllPackagesAreOverpacked(DataContextType.TransitDispatch, "DC00000001", "TDU", "TD00000001", ZString.Empty);
		}

		void TestOverpack_NotAllPackagesAreOverpacked(DataContextType consignmentType, ZString consignmentReference, ZString transportationUnitType, ZString transportationUnitReference, ZString dispatchLoadListReference)
		{
			var transportationUnits = new List<(ZString Reference, ZDateTime LastKnownTime)> { (transportationUnitReference, new ZDateTime(2020, 6, 1, 10, 17, 0)) };
			var (warehouse1AddressBO, shipmentDataObject) = SetupShipmentDataObject(consignmentType, consignmentReference, transportationUnits, dispatchLoadListReference);

			var packingLineCollection1 = new List<UniversalPackingLine>();
			var packingLineInner11 = CreatePackingLine("S00001000-BOX1", 1, "BOX", 100m, "KG", 10m, "M3", "M", "TEST001", "GEN");
			SetLastKnownTransitWarehouseStatusDateTime(packingLineInner11, consignmentType, dispatchLoadListReference, new ZDateTime(2020, 6, 1, 10, 17, 0));
			SetReferenceNumber(packingLineInner11, transportationUnitType, transportationUnitReference);
			packingLineCollection1.Add(packingLineInner11);
			var packingLine1 = CreatePackingLine("OVP00001", 1, "PLT", 3000m, "KG", 300m, "M3", "M", null, "GEN");
			SetLastKnownTransitWarehouseStatusDateTime(packingLine1, consignmentType, dispatchLoadListReference, new ZDateTime(2020, 6, 1, 10, 17, 0));
			packingLine1.SetPackingLineCollection(() => packingLineCollection1);

			var packingLineCollection2 = new List<UniversalPackingLine>();
			var packingLineInner21 = CreatePackingLine("S00001000-CTN1", 1, "CTN", 200m, "KG", 20m, "M3", "M", "TEST002", "GEN");
			SetLastKnownTransitWarehouseStatusDateTime(packingLineInner21, consignmentType, dispatchLoadListReference, new ZDateTime(2020, 6, 1, 10, 17, 0));
			SetReferenceNumber(packingLineInner21, transportationUnitType, transportationUnitReference);
			packingLineCollection2.Add(packingLineInner21);
			var packingLine2 = CreatePackingLine("OVP00002", 1, "PLT", 4000m, "KG", 400m, "M3", "M", null, "GEN");
			SetLastKnownTransitWarehouseStatusDateTime(packingLine2, consignmentType, dispatchLoadListReference, new ZDateTime(2020, 6, 1, 10, 17, 0));
			packingLine2.SetPackingLineCollection(() => packingLineCollection2);

			var packingLine3 = CreatePackingLine("S00001000-BOX2", 1, "BOX", 300m, "KG", 30m, "M3", "M", "TEST001", "GEN", 0);
			SetLastKnownTransitWarehouseStatusDateTime(packingLine3, consignmentType, dispatchLoadListReference, new ZDateTime(2020, 6, 1, 10, 17, 0));
			SetReferenceNumber(packingLine3, transportationUnitType, transportationUnitReference);
			var packingLine4 = CreatePackingLine("S00001000-CTN2", 1, "CTN", 400m, "KG", 40m, "M3", "M", "TEST002", "GEN");
			SetLastKnownTransitWarehouseStatusDateTime(packingLine4, consignmentType, dispatchLoadListReference, new ZDateTime(2020, 6, 1, 10, 17, 0));
			SetReferenceNumber(packingLine4, transportationUnitType, transportationUnitReference);
			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine> { packingLine1, packingLine2, packingLine3, packingLine4 });
			shipmentDataObject.PackingLineCollection.Content = CollectionContent.Complete;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = shipment.PK;
			var forwardingPackLine1 = CreatePackLine("REF001", "TEST001", "BOX", FreightConstants.OuterPackType, 2, 100m, "KG", 10m, "M3", "M", "GEN");
			CreatePackages(
				forwardingPackLine1, 2, warehouse1AddressBO, new ZDateTime(2020, 6, 1, 6, 17, 0),
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
				FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
				"S00001000-BOX", packageJob);
			var forwardingPackLine2 = CreatePackLine("REF002", "TEST002", "CTN", FreightConstants.OuterPackType, 2, 400m, "KG", 40m, "M3", "M", "GEN");
			CreatePackages(
				forwardingPackLine2, 2, warehouse1AddressBO, new ZDateTime(2020, 6, 1, 7, 17, 0),
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
				FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
				"S00001000-CTN", packageJob);
			shipment.OuterPackLines.Add(forwardingPackLine1);
			shipment.OuterPackLines.Add(forwardingPackLine2);

			var readingHelper = Process(consignmentType, shipment, warehouse1AddressBO, shipmentDataObject);
			AssertEquals("2 outer packlines", 2, shipment.OuterPackLines.Count);

			var outerPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(l => l.JL_F3_NKPackType == "PLT");
			AssertPackLine(outerPackLine, 2, "PLT", FreightConstants.OuterPackType, 7000m, "KG", 700m, "M3", "M", "GEN", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, new ZDateTime(2020, 6, 1, 10, 17, 0));
			AssertEquals("2 outer packages", 2, outerPackLine.PkgPackageCollection.Count);
			var package = outerPackLine.PkgPackageCollection.FirstOrDefault(p => p.KP_PackageID == "OVP00001");
			AssertPackage(package, "OVP00001", "PLT", 1, 3000m, "KG", 300m, "M3", "GEN");
			package = outerPackLine.PkgPackageCollection.FirstOrDefault(p => p.KP_PackageID == "OVP00002");
			AssertPackage(package, "OVP00002", "PLT", 1, 4000m, "KG", 400m, "M3", "GEN");
			AssertEquals("2 inner packlines", 2, outerPackLine.InnerPackLines.Count);
			var innerPackLine = outerPackLine.InnerPackLines.FirstOrDefault(l => l.JL_F3_NKPackType == "BOX");
			AssertPackLine(innerPackLine, 1, "BOX", FreightConstants.InnerPackType, 100m, "KG", 10m, "M3", "M", "GEN", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown, ZDateTime.Empty);
			innerPackLine = outerPackLine.InnerPackLines.FirstOrDefault(l => l.JL_F3_NKPackType == "CTN");
			AssertPackLine(innerPackLine, 1, "CTN", FreightConstants.InnerPackType, 200m, "KG", 20m, "M3", "M", "GEN", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown, ZDateTime.Empty);

			outerPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(l => l.JL_F3_NKPackType == "CTN");
			AssertPackLine(outerPackLine, 2, "CTN", FreightConstants.OuterPackType, 400m, "KG", 40m, "M3", "M", "GEN", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies, new ZDateTime(2020, 6, 1, 10, 17, 0));
			AssertEquals("1 outer package", 1, outerPackLine.PkgPackageCollection.Count);
			package = outerPackLine.PkgPackageCollection.FirstOrDefault(p => p.KP_PackageID == "S00001000-CTN2");
			AssertPackage(package, "S00001000-CTN2", "CTN", 1, 400m, "KG", 40m, "M3", "GEN");
			AssertEquals("0 inner packlines", 0, outerPackLine.InnerPackLines.Count);

			AssertShipment(shipment, 0, "PLT", 0m, "KG", 0m, "M3", 0m, 0, "CTN");
		}

		public void TestOverpack_OverpackIdIsSet()
		{
			DGSubstanceTestHelper.Create("1234", "a", "IMO");
			DGSubstanceTestHelper.Create("1234", "b", "IMO");
			DGSubstanceTestHelper.Create("2345", "a", "IMO");
			DGSubstanceTestHelper.Create("2345", "b", "IMO");
			var substance1 = UNDGSubstanceLoader.LoadSubstances(Factory.BOFactory, "1234", "a", "IMO").First();
			var substance2 = UNDGSubstanceLoader.LoadSubstances(Factory.BOFactory, "1234", "b", "IMO").First();
			var substance3 = UNDGSubstanceLoader.LoadSubstances(Factory.BOFactory, "2345", "a", "IMO").First();
			var substance4 = UNDGSubstanceLoader.LoadSubstances(Factory.BOFactory, "2345", "b", "IMO").First();

			var transportationUnits = new List<(ZString Reference, ZDateTime LastKnownTime)> { ("TD00000001", new ZDateTime(2020, 6, 1, 10, 17, 0)) };
			var (warehouse1AddressBO, shipmentDataObject) = SetupShipmentDataObject(DataContextType.TransitDispatch, "DC00000001", transportationUnits, ZString.Empty);

			var packingLineCollection1 = new List<UniversalPackingLine>();
			for (var i = 1; i <= 1; i++)
			{
				var packingLineInner = CreatePackingLine("S00001000-BOX" + i, 1, "BOX", 100m, "KG", 10m, "M3", "M", "TEST001", "GEN");
				SetLastKnownTransitWarehouseStatusDateTime(packingLineInner, DataContextType.TransitDispatch, ZString.Empty, new ZDateTime(2020, 6, 1, 10, 17, 0));
				SetReferenceNumber(packingLineInner, "TDU", "TD00000001");
				packingLineInner.SetUNDGCollection(() => new List<UNDG> { CreateUNDG(substance1.DG_Code, i) });
				packingLineCollection1.Add(packingLineInner);
			}
			for (var i = 1; i <= 2; i++)
			{
				var packingLineInner = CreatePackingLine("S00001000-CTN" + i, 1, "CTN", 200m, "KG", 20m, "M3", "M", "TEST002", "GEN");
				SetLastKnownTransitWarehouseStatusDateTime(packingLineInner, DataContextType.TransitDispatch, ZString.Empty, new ZDateTime(2020, 6, 1, 10, 17, 0));
				SetReferenceNumber(packingLineInner, "TDU", "TD00000001");
				packingLineInner.SetUNDGCollection(() => new List<UNDG> { CreateUNDG(substance2.DG_Code, i) });
				packingLineCollection1.Add(packingLineInner);
			}
			var packingLine1 = CreatePackingLine("OVP00001", 1, "PLT", 3000m, "KG", 300m, "M3", "M", null, "GEN");
			SetLastKnownTransitWarehouseStatusDateTime(packingLine1, DataContextType.TransitDispatch, ZString.Empty, new ZDateTime(2020, 6, 1, 10, 17, 0));
			packingLine1.SetPackingLineCollection(() => packingLineCollection1);

			var packingLineCollection2 = new List<UniversalPackingLine>();
			for (var i = 1; i <= 3; i++)
			{
				var packingLineInner = CreatePackingLine("S00001000-CAS" + i, 1, "CAS", 300m, "KG", 30m, "M3", "M", "TEST003", "GEN");
				SetLastKnownTransitWarehouseStatusDateTime(packingLineInner, DataContextType.TransitDispatch, ZString.Empty, new ZDateTime(2020, 6, 1, 10, 17, 0));
				SetReferenceNumber(packingLineInner, "TDU", "TD00000001");
				packingLineInner.SetUNDGCollection(() => new List<UNDG> { CreateUNDG(substance3.DG_Code, i) });
				packingLineCollection2.Add(packingLineInner);
			}
			for (var i = 1; i <= 4; i++)
			{
				var packingLineInner = CreatePackingLine("S00001000-TUB" + i, 1, "TUB", 400m, "KG", 40m, "M3", "M", "TEST004", "GEN");
				SetLastKnownTransitWarehouseStatusDateTime(packingLineInner, DataContextType.TransitDispatch, ZString.Empty, new ZDateTime(2020, 6, 1, 10, 17, 0));
				SetReferenceNumber(packingLineInner, "TDU", "TD00000001");
				packingLineInner.SetUNDGCollection(() => new List<UNDG> { CreateUNDG(substance4.DG_Code, i) });
				packingLineCollection2.Add(packingLineInner);
			}
			var packingLine2 = CreatePackingLine("OVP00002", 1, "PLT", 4000m, "KG", 400m, "M3", "M", null, "GEN");
			SetLastKnownTransitWarehouseStatusDateTime(packingLine2, DataContextType.TransitDispatch, ZString.Empty, new ZDateTime(2020, 6, 1, 10, 17, 0));
			packingLine2.SetPackingLineCollection(() => packingLineCollection2);
			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine> { packingLine1, packingLine2 });

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = shipment.PK;
			var forwardingPackLine1 = CreatePackLine("REF001", "TEST001", "BOX", FreightConstants.OuterPackType, 1, 100m, "KG", 10m, "M3", "M", "GEN");
			var forwardingPackLine2 = CreatePackLine("REF002", "TEST002", "CTN", FreightConstants.OuterPackType, 2, 400m, "KG", 40m, "M3", "M", "GEN");
			var forwardingPackLine3 = CreatePackLine("REF003", "TEST003", "CAS", FreightConstants.OuterPackType, 3, 900m, "KG", 90m, "M3", "M", "GEN");
			var forwardingPackLine4 = CreatePackLine("REF004", "TEST004", "TUB", FreightConstants.OuterPackType, 4, 1600m, "KG", 160m, "M3", "M", "GEN");
			shipment.OuterPackLines.Add(forwardingPackLine1);
			shipment.OuterPackLines.Add(forwardingPackLine2);
			shipment.OuterPackLines.Add(forwardingPackLine3);
			shipment.OuterPackLines.Add(forwardingPackLine4);

			var readingHelper = Process(DataContextType.TransitDispatch, shipment, warehouse1AddressBO, shipmentDataObject);
			AssertEquals("1 outer packline", 1, shipment.OuterPackLines.Count);
			var outerPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(l => l.JL_RH_NKCommodityCode == "GEN");
			AssertPackLine(
				outerPackLine, 2, "PLT", FreightConstants.OuterPackType, 7000m, "KG", 700m, "M3", "M", "GEN",
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, new ZDateTime(2020, 6, 1, 10, 17, 0));

			AssertEquals("2 outer packages", 2, outerPackLine.PkgPackageCollection.Count);
			AssertEquals("4 inner packlines", 4, outerPackLine.InnerPackLines.Count);
			AssertEquals("4 undgs", 4, outerPackLine.UNDGs.Count);
			var undg1 = outerPackLine.UNDGs.FirstOrDefault(g => g.DI_DG == substance1.PK);
			AssertUNDG(undg1, substance1, 1, true, "OVP00001");
			var undg2 = outerPackLine.UNDGs.FirstOrDefault(g => g.DI_DG == substance2.PK);
			AssertUNDG(undg2, substance2, 3, true, "OVP00001");
			var undg3 = outerPackLine.UNDGs.FirstOrDefault(g => g.DI_DG == substance3.PK);
			AssertUNDG(undg3, substance3, 6, true, "OVP00002");
			var undg4 = outerPackLine.UNDGs.FirstOrDefault(g => g.DI_DG == substance4.PK);
			AssertUNDG(undg4, substance4, 10, true,"OVP00002");
		}

		[ExpectNoExceptions]
		public void TestOverpack_TransitReceive_PackPartialPacklines()
		{
			var transportationUnits = new List<(ZString Reference, ZDateTime LastKnownTime)> { ("TR00000001", new ZDateTime(2020, 6, 1, 10, 17, 0)) };
			var (warehouse1AddressBO, shipmentDataObject) = SetupShipmentDataObject(DataContextType.TransitReceive, "RC00000001", transportationUnits, ZString.Empty);

			var packingLine1Inner = CreatePackingLine(null, 2, "BAG", 600m, "KG", 60m, "M3", "M", "TEST001", "GEN");
			packingLine1Inner.UnloadDate = new ZDateTime(2020, 6, 1, 10, 17, 0);
			SetReferenceNumber(packingLine1Inner, "TRU", "TR00000001");
			var packingLine1 = CreatePackingLine("PLT00001", 1, "PLT", 3000m, "KG", 300m, "M3", "M", null, "GEN");
			packingLine1.UnloadDate = new ZDateTime(2020, 6, 1, 10, 17, 0);
			packingLine1.SetPackingLineCollection(() => new List<UniversalPackingLine> { packingLine1Inner });

			var packingLine2Inner = CreatePackingLine(null, 2, "BAG", 600m, "KG", 60m, "M3", "M", "TEST001", "GEN");
			packingLine2Inner.UnloadDate = new ZDateTime(2020, 6, 1, 10, 17, 0);
			SetReferenceNumber(packingLine2Inner, "TRU", "TR00000001");
			var packingLine2 = CreatePackingLine("PLT00002", 1, "PLT", 4000m, "KG", 400m, "M3", "M", null, "GEN");
			packingLine2.UnloadDate = new ZDateTime(2020, 6, 1, 10, 17, 0);
			packingLine2.SetPackingLineCollection(() => new List<UniversalPackingLine> { packingLine2Inner });

			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine> { packingLine1, packingLine2 });

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = shipment.PK;
			packageJob.KJ_ParentTableCode = shipment.TablePrefix;
			var forwardingPackLine = CreatePackLine("REF001", "TEST001", "BAG", FreightConstants.OuterPackType, 4, 600m, "KG", 60m, "M3", "M", "GEN");
			shipment.OuterPackLines.Add(forwardingPackLine);

			var readingHelper = Process(DataContextType.TransitReceive, shipment, warehouse1AddressBO, shipmentDataObject);

			Factory.SaveForTesting();

			AssertEquals("1 outer packline", 1, shipment.OuterPackLines.Count);
			var outerPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(l => l.JL_RH_NKCommodityCode == "GEN");
			AssertPackLine(outerPackLine, 2, "PLT", FreightConstants.OuterPackType, 7000m, "KG", 700m, "M3", "M", "GEN", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, new ZDateTime(2020, 6, 1, 10, 17, 0));

			AssertEquals("2 outer packages", 2, outerPackLine.PkgPackageCollection.Count);
			var package = outerPackLine.PkgPackageCollection.FirstOrDefault(p => p.KP_PackageID == "PLT00001");
			AssertPackage(package, "PLT00001", "PLT", 1, 3000m, "KG", 300m, "M3", "GEN");
			package = outerPackLine.PkgPackageCollection.FirstOrDefault(p => p.KP_PackageID == "PLT00002");
			AssertPackage(package, "PLT00002", "PLT", 1, 4000m, "KG", 400m, "M3", "GEN");

			AssertEquals("1 inner packline", 1, outerPackLine.InnerPackLines.Count);
			var innerPackLine = outerPackLine.InnerPackLines.FirstOrDefault(l => l.JL_F3_NKPackType == "BAG");
			AssertPackLine(innerPackLine, 4, "BAG", FreightConstants.InnerPackType, 1200m, "KG", 120m, "M3", "M", "GEN", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown, ZDateTime.Empty);
		}

		public void TestOverpack_TransitReceive_PackagesOnSplitPacklinesAreOverpacked()
		{
			var transportationUnits = new List<(ZString Reference, ZDateTime LastKnownTime)> { ("TR00000001", new ZDateTime(2020, 6, 1, 10, 17, 0)) };
			var (warehouse1AddressBO, shipmentDataObject) = SetupShipmentDataObject(DataContextType.TransitReceive, "RC00000001", transportationUnits, ZString.Empty);

			var packingLineInner1 = CreatePackingLine("TEST001-BOX1", 1, "BOX", 400m, "KG", 40m, "M3", "M", "TEST001", "GEN");
			SetLastKnownTransitWarehouseStatusDateTime(packingLineInner1, DataContextType.TransitReceive, ZString.Empty, new ZDateTime(2020, 6, 1, 10, 17, 0));
			SetReferenceNumber(packingLineInner1, "TRU", "TR00000001");
			var packingLineInner2 = CreatePackingLine("TEST002-BOX1", 1, "BOX", 400m, "KG", 40m, "M3", "M", "TEST001", "HAZ");
			SetLastKnownTransitWarehouseStatusDateTime(packingLineInner2, DataContextType.TransitReceive, ZString.Empty, new ZDateTime(2020, 6, 1, 10, 17, 0));
			SetReferenceNumber(packingLineInner2, "TRU", "TR00000001");

			var packingLine = CreatePackingLine("OVP00001", 1, "PLT", 800m, "KG", 80m, "M3", "M", null, "CRMC");
			SetLastKnownTransitWarehouseStatusDateTime(packingLine, DataContextType.TransitReceive, ZString.Empty, new ZDateTime(2020, 6, 1, 10, 17, 0));
			packingLine.SetPackingLineCollection(() => new List<UniversalPackingLine> { packingLineInner1, packingLineInner2 });
			var packingLineCollection = new DataObjectList<UniversalPackingLine> { packingLine };
			packingLineCollection.Content = CollectionContent.Complete;
			shipmentDataObject.SetPackingLineCollection(() => packingLineCollection);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = shipment.PK;
			var forwardingPackLine1 = CreatePackLine("REF001", "TEST001", "BOX", FreightConstants.OuterPackType, 1, 400m, "KG", 40m, "M3", "M", "GEN");
			CreatePackages(
				forwardingPackLine1, 1, warehouse1AddressBO, new ZDateTime(2020, 6, 1, 9, 17, 0),
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
				FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
				"TEST001-BOX", packageJob);
			var forwardingPackLine2 = CreatePackLine("REF002", "TEST002", "BOX", FreightConstants.OuterPackType, 1, 400m, "KG", 40m, "M3", "M", "HAZ");
			CreatePackages(
				forwardingPackLine2, 1, warehouse1AddressBO, new ZDateTime(2020, 6, 1, 9, 17, 0),
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
				FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
				"TEST002-BOX", packageJob);
			shipment.OuterPackLines.Add(forwardingPackLine1);
			shipment.OuterPackLines.Add(forwardingPackLine2);

			var readingHelper = Process(DataContextType.TransitReceive, shipment, warehouse1AddressBO, shipmentDataObject);
			AssertEquals("1 outer packline", 1, shipment.OuterPackLines.Count);

			var outerPackLine = shipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(l => l.JL_RH_NKCommodityCode == "CRMC");
			AssertPackLine(
				outerPackLine, 1, "PLT", FreightConstants.OuterPackType, 800m, "KG", 80m, "M3", "M", "CRMC",
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, new ZDateTime(2020, 6, 1, 10, 17, 0));
			AssertEquals("1 outer package", 1, outerPackLine.PkgPackageCollection.Count);
			var package = outerPackLine.PkgPackageCollection.FirstOrDefault(p => p.KP_PackageID == "OVP00001");
			AssertPackage(package, "OVP00001", "PLT", 1, 800m, "KG", 80m, "M3", "CRMC");
			AssertEquals("2 inner packlines", 2, outerPackLine.InnerPackLines.Count);
			var innerPackLine = outerPackLine.InnerPackLines.FirstOrDefault(l => l.JL_RH_NKCommodityCode == "GEN");
			AssertPackLine(innerPackLine, 1, "BOX", FreightConstants.InnerPackType, 400m, "KG", 40m, "M3", "M", "GEN", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown, ZDateTime.Empty);
			innerPackLine = outerPackLine.InnerPackLines.FirstOrDefault(l => l.JL_RH_NKCommodityCode == "HAZ");
			AssertPackLine(innerPackLine, 1, "BOX", FreightConstants.InnerPackType, 400m, "KG", 40m, "M3", "M", "HAZ", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown, ZDateTime.Empty);
		}

		(OrgAddress Warehouse1AddressBO, UniversalShipment ShipmentDataObject) SetupShipmentDataObject(DataContextType consignmentType, ZString consignmentReference, List<(ZString Reference, ZDateTime LastKnownTime)> transportationUnits, ZString dispatchLoadListReference)
		{
			var warehouse1AddressBO = Factory.NewWithValidTestData<OrgAddress>();
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.DataContext = DataContextFactory.New();
			shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			shipmentDataObject.DataContext.AddDataSource(consignmentType, consignmentReference);
			shipmentDataObject.WayBillNumber = "BACON PANCAKES";
			shipmentDataObject.WayBillType = new UniversalWayBillType { Code = WayBillTypeList.Codes.House };

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<UniversalOrganizationAddress> { new UniversalOrganizationAddress()
			{
				AddressShortCode = warehouse1AddressBO.AddressCode,
				AddressType = nameof(DocAddressType.LocalCartageCFS),
				OrganizationCode = warehouse1AddressBO.Header.OH_Code
			} });

			DataContextType consignmentHeader;
			UniversalDateType dateType;
			if (consignmentType == DataContextType.TransitReceive)
			{
				consignmentHeader = DataContextType.TransitReceiveHeader;
				dateType = UniversalDateType.Unpack;
			}
			else
			{
				consignmentHeader = DataContextType.TransitDispatchHeader;
				dateType = UniversalDateType.Pack;
			}

			var relatedShipmentCollection = transportationUnits.Select(u =>
			{
				var header = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				header.DataContext = new DataContext();
				header.DataContext.AddDataSource(consignmentHeader, u.Reference);
				header.SetDateCollection(() => new List<UniversalDate> { new UniversalDate { Type = dateType, Value = u.LastKnownTime } });
				return header;
			});
			shipmentDataObject.SetRelatedShipmentCollection(() => relatedShipmentCollection.ToList());

			if (!dispatchLoadListReference.IsEmpty)
			{
				var loadList = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				loadList.DataContext = new DataContext();
				loadList.DataContext.AddDataSource(DataContextType.TransitDispatchLoadList, dispatchLoadListReference);
				shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { loadList });
			}

			return (warehouse1AddressBO, shipmentDataObject);
		}

		ShipmentDataObjectReadingHelper Process(DataContextType consignmentType, ForwardingShipment shipment, OrgAddress warehouse1AddressBO, UniversalShipment shipmentDataObject)
		{
			var readingHelper = new ShipmentDataObjectReadingHelper(shipmentDataObject, Logger, Factory);
			readingHelper.LinkManager = new Mock<IContainerLinkManager<ForwardingConsol>>().Object;
			var shipmentDataObjectSplitPackageHelper = new ShipmentDataObjectSplitPackageHelper(shipmentDataObject, shipment, warehouse1AddressBO, Logger, Factory, readingHelper);
			if (consignmentType == DataContextType.TransitReceive)
			{
				shipmentDataObjectSplitPackageHelper.ProcessTransitReceive();
			}
			else
			{
				shipmentDataObjectSplitPackageHelper.ProcessTransitDispatch();
			}
			return readingHelper;
		}

		ShipmentDataObjectSplitPackageHelper GetShipmentDataObjectSplitPackageHelper(UniversalShipment shipmentDataObject, ForwardingShipment shipment, OrgAddress warehouse1AddressBO)
		{
			var readingHelper = new ShipmentDataObjectReadingHelper(shipmentDataObject, Logger, Factory);
			readingHelper.LinkManager = new Mock<IContainerLinkManager<ForwardingConsol>>().Object;
			return new ShipmentDataObjectSplitPackageHelper(shipmentDataObject, shipment, warehouse1AddressBO, Logger, Factory, readingHelper);
		}

		ForwardingPackLine CreatePackLine(
			ZString refNumber, ZString packLineId, ZString packType, ZString freightMode, ZInt packageCount, ZDecimal actualWeight,
			ZString actualWeightUQ, ZDecimal actualVolume, ZString actualVolumeUQ, ZString unitOfDimension, ZString commodityCode, ForwardingPackLine outerPackLine = null)
		{
			var packLine = Factory.New<ForwardingPackLine>();
			packLine.JL_RefNumber = refNumber;
			packLine.JL_PackLineId = packLineId;
			packLine.JL_F3_NKPackType = packType;
			packLine.JL_FreightMode = freightMode;
			packLine.JL_PackageCount = packageCount;
			packLine.JL_ActualWeight = actualWeight;
			packLine.JL_ActualWeightUQ = actualWeightUQ;
			packLine.JL_ActualVolume = actualVolume;
			packLine.JL_ActualVolumeUQ = actualVolumeUQ;
			packLine.JL_UnitOfDimension = unitOfDimension;
			packLine.JL_RH_NKCommodityCode = commodityCode;
			packLine.JL_JL_OuterPackLine = outerPackLine?.PK ?? ZGuid.Empty;
			return packLine;
		}

		void CreatePackages(ForwardingPackLine packLine, ZInt packageCount, OrgAddress warehouse1AddressBO, ZDateTime lastKnownTransitWarehouseStatusDateTime, ZString originTransitWarehouseStatus, ZString lastKnownTransitWarehouseStatus, string packageIDPrefix, PkgPackageJob packageJob, int startPackageID = 1)
		{
			packLine.JL_PackageCount = packageCount;
			packLine.JL_OA_LastKnownTransitWarehouseAddress = warehouse1AddressBO.PK;
			packLine.JL_LastKnownTransitWarehouseStatusDateTime = lastKnownTransitWarehouseStatusDateTime;
			packLine.JL_OriginTransitWarehouseStatus = originTransitWarehouseStatus;
			packLine.JL_LastKnownTransitWarehouseStatus = lastKnownTransitWarehouseStatus;
			var weight = packLine.JL_ActualWeight / packageCount;
			var volume = packLine.JL_ActualVolume / packageCount;
			for (var i = 0; i < packageCount; i++)
			{
				var package = packLine.PkgPackageCollection.AddNew();
				package.KP_PackageID = packageIDPrefix + (startPackageID + i);
				package.KP_KJ_ParentPackageJob = packageJob.PK;
				package.KP_F3_NKPackType = packLine.JL_F3_NKPackType;
				package.KP_PackageQty = 1;
				package.KP_Weight = weight;
				package.KP_WeightUQ = packLine.JL_ActualWeightUQ;
				package.KP_Volume = volume;
				package.KP_VolumeUQ = packLine.JL_ActualVolumeUQ;
				package.KP_RH_NKCommodityCode = packLine.JL_RH_NKCommodityCode;
			}
		}

		void CreateAPackageWithQty(PkgPackageJob packageJob, ForwardingPackLine packLine, ZInt packageCount,
			OrgAddress warehouse1AddressBO, ZDateTime lastKnownTransitWarehouseStatusDateTime, ZString originTransitWarehouseStatus, ZString lastKnownTransitWarehouseStatus,
			string packageID, ZString externalReference, ZString previousPacklineID)
		{
			packLine.JL_PackageCount = packageCount;
			packLine.JL_OA_LastKnownTransitWarehouseAddress = warehouse1AddressBO.PK;
			packLine.JL_LastKnownTransitWarehouseStatusDateTime = lastKnownTransitWarehouseStatusDateTime;
			packLine.JL_OriginTransitWarehouseStatus = originTransitWarehouseStatus;
			packLine.JL_LastKnownTransitWarehouseStatus = lastKnownTransitWarehouseStatus;
			var weight = packLine.JL_ActualWeight / packageCount;
			var volume = packLine.JL_ActualVolume / packageCount;

			var package = packLine.PkgPackageCollection.AddNew();
			package.KP_PackageID = packageID;
			package.KP_KJ_ParentPackageJob = packageJob.PK;
			package.KP_F3_NKPackType = packLine.JL_F3_NKPackType;
			package.KP_PackageQty = packageCount;
			package.KP_Weight = weight;
			package.KP_WeightUQ = packLine.JL_ActualWeightUQ;
			package.KP_Volume = volume;
			package.KP_VolumeUQ = packLine.JL_ActualVolumeUQ;
			package.KP_RH_NKCommodityCode = packLine.JL_RH_NKCommodityCode;
			package.KP_ExternalReference = externalReference;
			package.KP_PreviousPackLineID = previousPacklineID;
		}

		static UniversalPackingLine CreatePackingLine(
			string referenceNumber, long packQty, string packType, decimal weight, string weightUnit, decimal volume,
			string volumeUnit, string lengthUnit, string packingLineID, string commodity, int outturnQty = 1)
		{
			var packingLine = new UniversalPackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packingLine.ReferenceNumber = referenceNumber;
			packingLine.PackQty = packQty;
			packingLine.PackType = new PackageType() { Code = packType };
			packingLine.Weight = weight;
			packingLine.WeightUnit = new UnitOfWeight() { Code = weightUnit };
			packingLine.Volume = volume;
			packingLine.VolumeUnit = new UnitOfVolume() { Code = volumeUnit };
			packingLine.LengthUnit = new UnitOfLength() { Code = lengthUnit };
			packingLine.PackingLineID = packingLineID;
			packingLine.Commodity = new Commodity() { Code = commodity };
			packingLine.OutturnQty = outturnQty;
			return packingLine;
		}

		static UNDG CreateUNDG(string undgCode, int packQty)
		{
			var undg = new UNDG(DefaultDataObjectWriterStrategy.TestInstance);
			undg.UNDGCode = undgCode;
			undg.PackQty = packQty;
			return undg;
		}

		static void SetReferenceNumber(UniversalPackingLine packingLine, string code, string number)
		{
			packingLine.SetReferenceNumberCollection(() => new List<UniversalReference>() { new UniversalReference { Type = new UniversalEntryType { Code = code }, ReferenceNumber = number } });
		}

		static void SetLastKnownTransitWarehouseStatusDateTime(UniversalPackingLine packingLine, DataContextType consignmentType, ZString dispatchLoadListReference, ZDateTime lastKnownTransitWarehouseStatusDateTime)
		{
			if (packingLine == null)
			{
				return;
			}

			if (consignmentType == DataContextType.TransitDispatch && !dispatchLoadListReference.IsEmpty)
			{
				packingLine.LoadDate = lastKnownTransitWarehouseStatusDateTime;
			}
			else
			{
				packingLine.UnloadDate = lastKnownTransitWarehouseStatusDateTime;
			}
		}

		static void AssertPackLine(
			ForwardingPackLine packLine, ZInt packageCount, ZString packType, ZString freightMode,
			ZDecimal actualWeight, ZString actualWeightUQ, ZDecimal actualVolume,ZString actualVolumeUQ, ZString unitOfDimension,
			ZString commodityCode, ZString originTransitWarehouseStatus, ZDateTime lastKnownTransitWarehouseStatusDateTime)
		{
			AssertNotNull($"packageCount: {packageCount}, packType: {packType}, freightMode: {freightMode}, commodityCode: {commodityCode}", packLine);
			AssertEquals(nameof(packLine.JL_PackageCount), packageCount, packLine.JL_PackageCount);
			AssertEquals(nameof(packLine.JL_F3_NKPackType), packType, packLine.JL_F3_NKPackType);
			AssertEquals(nameof(packLine.JL_FreightMode), freightMode, packLine.JL_FreightMode);
			AssertEquals(nameof(packLine.JL_ActualWeight), actualWeight, packLine.JL_ActualWeight);
			AssertEquals(nameof(packLine.JL_ActualWeightUQ), actualWeightUQ, packLine.JL_ActualWeightUQ);
			AssertEquals(nameof(packLine.JL_ActualVolume), actualVolume, packLine.JL_ActualVolume);
			AssertEquals(nameof(packLine.JL_ActualVolumeUQ), actualVolumeUQ, packLine.JL_ActualVolumeUQ);
			AssertEquals(nameof(packLine.JL_UnitOfDimension), unitOfDimension, packLine.JL_UnitOfDimension);
			AssertEquals(nameof(packLine.JL_RH_NKCommodityCode), commodityCode, packLine.JL_RH_NKCommodityCode);
			AssertEquals(nameof(packLine.JL_OriginTransitWarehouseStatus), originTransitWarehouseStatus, packLine.JL_OriginTransitWarehouseStatus);
			AssertEquals(nameof(packLine.JL_LastKnownTransitWarehouseStatusDateTime), lastKnownTransitWarehouseStatusDateTime, packLine.JL_LastKnownTransitWarehouseStatusDateTime);

			if (packType == FreightConstants.InnerPackType)
			{
				AssertEquals(0, packLine.InnerPackLines.Count);
				AssertEquals(0, packLine.UNDGs.Count);
			}
		}

		static void AssertPackage(
			PkgPackage package, string packageID, string packType, int packageQty,
			decimal weight, string weightUQ, decimal volume, string volumeUQ, string commodityCode)
		{
			AssertNotNull($"packageID: {packageID}", package);
			AssertEquals(nameof(package.KP_PackageID), packageID, package.KP_PackageID);
			AssertEquals(nameof(package.KP_F3_NKPackType), packType, package.KP_F3_NKPackType);
			AssertEquals(nameof(package.KP_PackageQty), packageQty, package.KP_PackageQty);
			AssertEquals(nameof(package.KP_Weight), weight, package.KP_Weight);
			AssertEquals(nameof(package.KP_WeightUQ), weightUQ, package.KP_WeightUQ);
			AssertEquals(nameof(package.KP_Volume), volume, package.KP_Volume);
			AssertEquals(nameof(package.KP_VolumeUQ), volumeUQ, package.KP_VolumeUQ);
			AssertEquals(nameof(package.KP_RH_NKCommodityCode), commodityCode, package.KP_RH_NKCommodityCode);
		}

		static void AssertShipment(
			ForwardingShipment shipment, int outerPacks, string packType, decimal actualWeight, string unitOfWeight,
			decimal actualVolume, string unitOfVolume, decimal loadingMeters, int totalPackageCount, string totalCountPackType)
		{
			AssertNotNull($"outerPacks: {outerPacks}, packType: {packType}, totalPackageCount: {totalPackageCount}, totalCountPackType: {totalCountPackType}", shipment);
			AssertEquals(nameof(shipment.JS_OuterPacks), outerPacks, shipment.JS_OuterPacks);
			AssertEquals(nameof(shipment.JS_F3_NKPackType), packType, shipment.JS_F3_NKPackType);
			AssertEquals(nameof(shipment.JS_ActualWeight), actualWeight, shipment.JS_ActualWeight);
			AssertEquals(nameof(shipment.JS_UnitOfWeight), unitOfWeight, shipment.JS_UnitOfWeight);
			AssertEquals(nameof(shipment.JS_ActualVolume), actualVolume, shipment.JS_ActualVolume);
			AssertEquals(nameof(shipment.JS_UnitOfVolume), unitOfVolume, shipment.JS_UnitOfVolume);
			AssertEquals(nameof(shipment.JS_LoadingMeters), loadingMeters, shipment.JS_LoadingMeters);
			AssertEquals(nameof(shipment.JS_TotalPackageCount), totalPackageCount, shipment.JS_TotalPackageCount);
			AssertEquals(nameof(shipment.JS_F3_NKTotalCountPackType), totalCountPackType, shipment.JS_F3_NKTotalCountPackType);
		}

		static void AssertUNDG(UNDGDataItem undg, UNDGSubstance substance, int packageCount, bool hasOverPack = false, ZString? overpackID = null)
		{
			AssertNotNull($"substance.DG_Code: {substance.DG_Code}, packageCount: {packageCount}", undg);
			AssertEquals(nameof(undg.DI_DG), substance.PK, undg.DI_DG);
			AssertEquals(nameof(undg.DI_PackageCount), packageCount, undg.DI_PackageCount);
			AssertEquals(nameof(undg.DI_HasOverpack), hasOverPack, undg.DI_HasOverpack);
			if (overpackID.HasValue)
			{
				AssertEquals(nameof(undg.DI_OverpackID), overpackID.Value, undg.DI_OverpackID);
			}
		}

		static UniversalPackingLine CreatePacklineForDispatch(
			ZString referenceNumber,
			ZInt? outTurnQty,
			ZDateTime? loadDate,
			ZDateTime? unloadDate,
			List<UniversalReference> referenceNumberCollection)
		{
			var packingLine = ShipmentDataObjectReaderTest.CreatePacklineForDispatch();
			packingLine.ReferenceNumber = referenceNumber;
			packingLine.OutturnQty = outTurnQty;
			packingLine.LoadDate = loadDate;
			packingLine.UnloadDate = unloadDate;

			if (referenceNumberCollection != null)
			{
				packingLine.SetReferenceNumberCollection(() => referenceNumberCollection);
			}

			return packingLine;
		}

		#endregion

		public void TestGetScreeningMethodKey_NoNRE()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var forwardingPackLine = CreatePackLine("REF001", "TEST001", Core.Constants.PkgUnit.Pallet, FreightConstants.OuterPackType, 1, 3000m, "KG", 300m, "M3", "M", "GEN");
				forwardingPackLine.JL_InspectionTypeCode = "CMD";

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "GBFXT";
				shipment.JS_RL_NKDestination = "AUSYD";
				shipment.JS_InspectionTypeCode = "CMD";
				shipment.OuterPackLines.Add(forwardingPackLine);

				Factory.SaveForTesting();

				var transportationUnits = new List<(ZString Reference, ZDateTime LastKnownTime)> { ("TR00000001", new ZDateTime(2020, 6, 1, 10, 17, 0)) };
				var (warehouse1AddressBO, shipmentDataObject) = SetupShipmentDataObject(DataContextType.TransitReceive, "RC00000001", transportationUnits, ZString.Empty);
				shipmentDataObject.TransportMode = new CodeDescriptionPair { Code = Core.Constants.TransportModes.Air };
				shipmentDataObject.PortOfOrigin = new UNLOCO { Code = "GBFXT" };
				shipmentDataObject.PortOfDischarge = new UNLOCO { Code = "AUSYD" };

				var packline = CreatePackingLine("S00001000-001", 1, Core.Constants.PkgUnit.Pallet, 3000m, "KG", 300m, "M3", "M", "TEST001", "GEN");
				packline.UnloadDate = new ZDateTime(2020, 6, 1, 10, 17, 0);
				packline.ScreeningMethod = "VCK";
				SetReferenceNumber(packline, "TRU", "TR00000001");
				shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine> { packline });

				var helper = GetShipmentDataObjectSplitPackageHelper(shipmentDataObject, shipment, warehouse1AddressBO);
				Factory.SaveForTesting();

				AssertEquals("Should return empty string and not NRE", ZString.Empty, helper.GetScreeningMethodKey(null));
			}
		}

		[TestDate(2023, 11, 18)]
		public void TestDefaultInspectionStatus()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (WarehouseDataRegistry.Instance.DriverSecurityCertificationCheckingActivated.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var transportationUnits = new List<(ZString Reference, ZDateTime LastKnownTime)> { ("TR00000001", new ZDateTime(2020, 6, 1, 10, 17, 0)) };
				var (warehouse1AddressBO, shipmentDataObject) = SetupShipmentDataObject(DataContextType.TransitReceive, "RC00000001", transportationUnits, ZString.Empty);
				shipmentDataObject.TransportMode = new CodeDescriptionPair { Code = Core.Constants.TransportModes.Air };
				shipmentDataObject.PortOfOrigin = new UNLOCO { Code = "GBFXT" };
				shipmentDataObject.PortOfDischarge = new UNLOCO { Code = "USLAX" };

				var packline = CreatePackingLine("S00001000-001", 1, Core.Constants.PkgUnit.Pallet, 3000m, Core.Constants.Weight.Kilograms, 300m, Core.Constants.Volume.CubicMetres, Core.Constants.Length.Metres, "TEST001", "GEN");
				packline.UnloadDate = new ZDateTime(2020, 6, 1, 10, 17, 0);
				packline.ScreeningMethod = ZString.Empty;
				SetReferenceNumber(packline, "TRU", "TR00000001");
				shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine> { packline });

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "GBFXT";
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_InspectionTypeCode = ZString.Empty;
				shipment.JS_HouseBill = "BACON PANCAKES";
				shipment.JS_OA_ExportReceivingDepot = warehouse1AddressBO.PK;

				var forwardingPackLine = CreatePackLine("REF001", "TEST001", Core.Constants.PkgUnit.Pallet, FreightConstants.OuterPackType, 1, 3000m, Core.Constants.Weight.Kilograms, 300m, Core.Constants.Volume.CubicMetres, Core.Constants.Length.Metres, "GEN");
				forwardingPackLine.JL_InspectionTypeCode = ZString.Empty;
				shipment.OuterPackLines.Add(forwardingPackLine);

				Factory.SaveForTesting();

				shipmentDataObject.AviationSecurityInspectionType = new CodeDescriptionPair { Code = "CMD" };
				new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				AssertEquals("JS_InspectionTypeCode has been updated", "CMD", shipment.JS_InspectionTypeCode);
				AssertEquals(1, shipment.OuterPackLines.Count);
				AssertEquals("JL_InspectionTypeCode has been updated", "UNK", shipment.OuterPackLines[0].JL_InspectionTypeCode);

				shipmentDataObject.AviationSecurityInspectionType = new CodeDescriptionPair { Code = "APP" };
				new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				AssertEquals("JS_InspectionTypeCode has been updated", "APP", shipment.JS_InspectionTypeCode);
				AssertEquals(1, shipment.OuterPackLines.Count);
				AssertEquals("JL_InspectionTypeCode has been updated", ZString.Empty, shipment.OuterPackLines[0].JL_InspectionTypeCode);
			}
		}
	}
}
