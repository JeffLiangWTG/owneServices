using System;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer.Testing
{
	[TestedType(typeof(ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>))]
	sealed class ConsolContainerXmlDataAdapterTest : JobContainerXmlDataAdapterTest
	{
		public void TestImportContainerMatchingByContainerCodeFirst()
		{
			RefContainer container2060 = new RefContainer.Loader(Factory).LoadFromISOType("22P0");
			AssertNotNull("Precondition 20PL", container2060);

			RefContainer container20GP = new RefContainer.Loader(Factory).LoadFromCode("20GP");
			AssertNotNull("Precondition 20GP", container20GP);

			CommonConsol consol = Factory.New<CommonConsol>();
			CommonContainer container = Factory.New<CommonContainer>();
			consol.Containers.Add(container);

			Xsd.Container containerValue = new Xsd.Container();
			containerValue.ContainerType.ContainerCode = "ABC";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container> adapter = new ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>(consol);

			adapter.ImportFromValueObject(container, containerValue, context);

			AssertEquals("Ref Container could not be found", true, container.JC_RC.IsEmpty);
			containerValue.ContainerType.ISOCode = "22P0";

			adapter.ImportFromValueObject(container, containerValue, context);
			AssertEquals("Ref Container matched by ISOCOde", container2060.PK, container.JC_RC);

			containerValue.ContainerType.ContainerCode = "20GP";
			adapter.ImportFromValueObject(container, containerValue, context);
			AssertEquals("Ref Container matched by ContainerCode", container20GP.PK, container.JC_RC);
		}

		public void TestImportContainerProperties()
		{
			Xsd.Container xsdContainer = new Xsd.Container();
			xsdContainer.IsArrivingAtCTOByRail = true;
			xsdContainer.IsArrivingAtCTOByRailSpecified = true;
			xsdContainer.IsDamaged = true;
			xsdContainer.IsDamagedSpecified = true;
			xsdContainer.IsEmptyContainer = false;
			xsdContainer.IsEmptyContainerSpecified = true;
			xsdContainer.ReleaseNumber = "ContainerBooking";
			xsdContainer.CommodityCode = "ABCD";
			xsdContainer.SetPointTemperature = 36.6M;
			xsdContainer.SetPointTemperatureSpecified = true;
			xsdContainer.SetPointTemperatureUnit = "C";
			xsdContainer.Weight = 1000m;

			#region Export Process
			xsdContainer.ExportProcess.EmptyRequiredBy = testDate.AddDays(1);
			xsdContainer.ExportProcess.EstimatedFullPickup = testDate.AddDays(2);
			xsdContainer.ExportProcess.CartageAdvised = testDate.AddDays(3);
			xsdContainer.ExportProcess.SlotBookingRef = "export book ref";
			xsdContainer.ExportProcess.SlotDate = testDate.AddDays(4);
			xsdContainer.ExportProcess.CartageRef = "export cart ref";
			xsdContainer.ExportProcess.ContainerYardGateOut = testDate.AddDays(5);
			xsdContainer.ExportProcess.WharfGateIn = testDate.AddDays(6);
			xsdContainer.ExportProcess.CartageComplete = testDate.AddDays(7);
			xsdContainer.ExportProcess.ShippedOnboard = testDate.AddDays(8);
			xsdContainer.ExportProcess.DemurrageTime = testDate.AddDays(9).AddYears(ZDateTime.DefaultDurationEpoch.Year - testDate.Year);
			xsdContainer.ExportProcess.DemurrageCharge = 10M;
			xsdContainer.ExportProcess.DemurrageChargeSpecified = true;
			xsdContainer.ExportProcess.IsArrivingAtCTOByRail = ZBool.True;

			Xsd.Organisation xsdPickupFromOrg = xsdContainer.ExportProcess.PickupEmptyFrom.Organisation;
			xsdPickupFromOrg.OrganisationDetails.Name = "pickup from org";
			Xsd.OrgAddress xsdPickupFromOrgAddress = xsdPickupFromOrg.OrganisationDetails.Addresses.AddNew();
			xsdPickupFromOrgAddress.AddressLine1 = "pickup from org addr 1";
			xsdContainer.ExportProcess.PickupEmptyFrom.AddressSequenceRef = 1;
			#endregion

			#region Import Process
			xsdContainer.ImportProcess.ReleaseNumber = "ImportRelease";
			xsdContainer.ImportProcess.FCLAvailable = testDate.AddDays(11);
			xsdContainer.ImportProcess.FCLStorage = testDate.AddDays(12);
			xsdContainer.ImportProcess.LCLAvailable = testDate.AddDays(13);
			xsdContainer.ImportProcess.LCLStorage = testDate.AddDays(14);
			xsdContainer.ImportProcess.WharfUnload = testDate.AddDays(15);
			xsdContainer.ImportProcess.SlotBookingRef = "import book ref";
			xsdContainer.ImportProcess.SlotDate = testDate.AddDays(16);
			xsdContainer.ImportProcess.CartageRef = "import cart ref";
			xsdContainer.ImportProcess.WharfGateOut = testDate.AddDays(17);
			xsdContainer.ImportProcess.EstimatedDelivery = testDate.AddDays(18);
			xsdContainer.ImportProcess.CartageAdvised = testDate.AddDays(19);
			xsdContainer.ImportProcess.CartageComplete = testDate.AddDays(20);

			xsdContainer.ImportProcess.EmptyReady = testDate.AddDays(21);
			xsdContainer.ImportProcess.EmptyReturnRequiredBy = testDate.AddDays(22);
			xsdContainer.ImportProcess.EmptyReturnedOn = testDate.AddDays(23);

			xsdContainer.ImportProcess.PickupByRail = false;
			xsdContainer.ImportProcess.PickupByRailSpecified = true;
			xsdContainer.ImportProcess.HeldForFCLTransitStaging = true;
			xsdContainer.ImportProcess.HeldForFCLTransitStagingSpecified = true;

			xsdContainer.ImportProcess.StorageDays = "24";
			xsdContainer.ImportProcess.StorageCharge = 25M;
			xsdContainer.ImportProcess.StorageChargeSpecified = true;
			xsdContainer.ImportProcess.DemurrageTime = testDate.AddDays(26).AddYears(ZDateTime.DefaultDurationEpoch.Year - testDate.Year);
			xsdContainer.ImportProcess.DemurrageCharge = 27M;
			xsdContainer.ImportProcess.DemurrageChargeSpecified = true;
			xsdContainer.ImportProcess.DetentionDays = "28";
			xsdContainer.ImportProcess.DetentionCharge = 29M;
			xsdContainer.ImportProcess.DetentionChargeSpecified = true;

			Xsd.Organisation xsdDeliverEmptyToOrg = xsdContainer.ImportProcess.DeliverEmptyTo.Organisation;
			xsdDeliverEmptyToOrg.OrganisationDetails.Name = "deliver empty to org";
			Xsd.OrgAddress xsdDeliverEmptyToOrgAddress = xsdDeliverEmptyToOrg.OrganisationDetails.Addresses.AddNew();
			xsdDeliverEmptyToOrgAddress.AddressLine1 = "deliver empty to addr 1";
			xsdContainer.ImportProcess.DeliverEmptyTo.AddressSequenceRef = 1;
			#endregion

			CommonConsol consol = Factory.New<CommonConsol>();
			CommonContainer freightContainer = Factory.New<CommonContainer>();
			consol.Containers.Add(freightContainer);
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container> adapter = new ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>(consol);
			adapter.ImportFromValueObject(freightContainer, xsdContainer, context);

			AssertEquals("Container JC_IsDamaged", ZBool.True, freightContainer.JC_IsDamaged);
			AssertEquals("Container JC_IsEmptyContainer", ZBool.False, freightContainer.JC_IsEmptyContainer);
			AssertEquals("Container JC_DepartureDeliveryByRail", ZBool.True, freightContainer.JC_DepartureDeliveryByRail);
			AssertEquals("Container JC_ReleaseNum", "ContainerBooking", freightContainer.JC_ReleaseNum);
			AssertEquals("Container JC_RH_NKContainerCommodityCode", "ABCD", freightContainer.JC_RH_NKContainerCommodityCode);
			AssertEquals("Container JC_SetPointTemp", 36.6M, freightContainer.JC_SetPointTemp);
			AssertEquals("Container JC_SetPointTempUnit", "C", freightContainer.JC_SetPointTempUnit);
			AssertEquals("Container JC_GrossWeight", 1000m, freightContainer.JC_GrossWeight);

			#region Export Process
			AssertEquals("EmptyRequiredBy", xsdContainer.ExportProcess.EmptyRequiredBy, freightContainer.JC_EmptyRequired);
			AssertEquals("EstimatedFullPickup", xsdContainer.ExportProcess.EstimatedFullPickup, freightContainer.JC_DepartureEstimatedPickup);
			AssertEquals("CartageAdvised", xsdContainer.ExportProcess.CartageAdvised, freightContainer.JC_DepartureCartageAdvised);
			AssertEquals("SlotBookingRef", xsdContainer.ExportProcess.SlotBookingRef, freightContainer.JC_DepartureSlotReference);
			AssertEquals("SlotDate", xsdContainer.ExportProcess.SlotDate, freightContainer.JC_DepartureSlotDateTime);
			AssertEquals("CartageRef", xsdContainer.ExportProcess.CartageRef, freightContainer.JC_DepartureCartageRef);
			AssertEquals("ContainerYardGateOut", xsdContainer.ExportProcess.ContainerYardGateOut, freightContainer.JC_ContainerYardEmptyPickupGateOut);
			AssertEquals("WharfGateIn", xsdContainer.ExportProcess.WharfGateIn, freightContainer.JC_FCLWharfGateIn);
			AssertEquals("CartageComplete", xsdContainer.ExportProcess.CartageComplete, freightContainer.JC_DepartureCartageComplete);
			AssertEquals("ShippedOnboard", xsdContainer.ExportProcess.ShippedOnboard, freightContainer.JC_FCLOnBoardVessel);
			AssertEquals("DemurrageTime", xsdContainer.ExportProcess.DemurrageTime, freightContainer.DepartureTruckWaitTime);
			AssertEquals("DemurrageCharge", xsdContainer.ExportProcess.DemurrageCharge, freightContainer.DepartureTruckWaitCost);

			AssertEquals("IsArrivingAtCTOByRail", xsdContainer.ExportProcess.IsArrivingAtCTOByRail, freightContainer.JC_DepartureDeliveryByRail);

			AssertNotNull(freightContainer.DepartureContainerYardAddress);
			OrgHeader pickupFromOrg = Factory.Load<OrgHeader>(freightContainer.DepartureContainerYardAddress.OA_OH);
			AssertEquals("pick up from org", xsdPickupFromOrg.OrganisationDetails.Name, pickupFromOrg.OH_FullName);
			AssertEquals("pick up from org addr 1", xsdPickupFromOrgAddress.AddressLine1, freightContainer.DepartureContainerYardAddress.OA_Address1);
			#endregion

			#region Import Process
			AssertEquals("ReleaseNumber", xsdContainer.ImportProcess.ReleaseNumber, freightContainer.JC_ContainerImportDORelease);
			AssertEquals("FCLAvailable", xsdContainer.ImportProcess.FCLAvailable, freightContainer.JC_FCLAvailable);
			AssertEquals("FCLStorage", xsdContainer.ImportProcess.FCLStorage, freightContainer.JC_ArrivalCTOStorageStartDate);
			AssertEquals("LCLAvailable", xsdContainer.ImportProcess.LCLAvailable, freightContainer.JC_LCLAvailable);
			AssertEquals("LCLStorage", xsdContainer.ImportProcess.LCLStorage, freightContainer.JC_LCLStorageCommences);
			AssertEquals("WharfUnload", xsdContainer.ImportProcess.WharfUnload, freightContainer.JC_FCLUnloadFromVessel);
			AssertEquals("SlotBookingRef", xsdContainer.ImportProcess.SlotBookingRef, freightContainer.JC_ArrivalSlotReference);
			AssertEquals("SlotDate", xsdContainer.ImportProcess.SlotDate, freightContainer.JC_ArrivalSlotDateTime);
			AssertEquals("CartageRef", xsdContainer.ImportProcess.CartageRef, freightContainer.JC_ArrivalCartageRef);
			AssertEquals("WharfGateOut", xsdContainer.ImportProcess.WharfGateOut, freightContainer.JC_FCLWharfGateOut);
			AssertEquals("EstimatedDelivery", xsdContainer.ImportProcess.EstimatedDelivery, freightContainer.JC_ArrivalEstimatedDelivery);
			AssertEquals("CartageAdvised", xsdContainer.ImportProcess.CartageAdvised, freightContainer.JC_ArrivalCartageAdvised);
			AssertEquals("CartageComplete", xsdContainer.ImportProcess.CartageComplete, freightContainer.JC_ArrivalCartageComplete);

			AssertEquals("EmptyReady", xsdContainer.ImportProcess.EmptyReady, freightContainer.JC_EmptyReadyForReturn);
			AssertEquals("EmptyReturnRequiredBy", xsdContainer.ImportProcess.EmptyReturnRequiredBy, freightContainer.JC_EmptyReturnedBy);
			AssertEquals("EmptyReturnedOn", xsdContainer.ImportProcess.EmptyReturnedOn, freightContainer.JC_ContainerYardEmptyReturnGateIn);

			AssertEquals("PickupByRail", xsdContainer.ImportProcess.PickupByRail, freightContainer.JC_ArrivalPickupByRail);
			AssertEquals("HeldForFCLTransitStaging", xsdContainer.ImportProcess.HeldForFCLTransitStaging, freightContainer.JC_FCLHeldInTransitStaging);

			AssertEquals("StorageDays", xsdContainer.ImportProcess.StorageDays, freightContainer.ArrivalCTOStorageDays.ToString());
			AssertEquals("StorageCharge", xsdContainer.ImportProcess.StorageCharge, freightContainer.ArrivalCTOStorageCost);
			AssertEquals("DemurrageTime", xsdContainer.ImportProcess.DemurrageTime, freightContainer.ArrivalTruckWaitTime);
			AssertEquals("DemurrageCharge", xsdContainer.ImportProcess.DemurrageCharge, freightContainer.ArrivalTruckWaitCost);
			AssertEquals("DetentionDays", xsdContainer.ImportProcess.DetentionDays, freightContainer.ArrivalCarrierDetentionDays.ToString());
			AssertEquals("DetentionCharge", xsdContainer.ImportProcess.DetentionCharge, freightContainer.ArrivalCarrierDetentionCost);

			AssertNotNull(freightContainer.ArrivalContainerYardAddress);
			OrgHeader deliverEmptyToOrg = Factory.Load<OrgHeader>(freightContainer.ArrivalContainerYardAddress.OA_OH);
			AssertEquals("deliver empty to org", xsdDeliverEmptyToOrg.OrganisationDetails.Name, deliverEmptyToOrg.OH_FullName);
			AssertEquals("deliver empty to addr 1", xsdDeliverEmptyToOrgAddress.AddressLine1, freightContainer.ArrivalContainerYardAddress.OA_Address1);
			#endregion
		}

		public void TestExport_ArrivalEstimatedDelivery_Empty()
		{
			Xsd.Container xsdCont = new Xsd.Container();
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonContainer cont = Factory.New<CommonContainer>();
			consol.Containers.Add(cont);
			ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container> adapter = new ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>(consol);

			Xsd.Container xmlCont = new Xsd.Container();
			adapter.ExportToValueObject(cont, xmlCont, new ValueObjectExportContext(notify));
			Assert(xmlCont.EstimatedDelivery.IsEmpty);
		}

		public void TestExport_ArrivalEstimatedDelivery()
		{
			Xsd.Container xsdCont = new Xsd.Container();
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonContainer cont = Factory.New<CommonContainer>();
			consol.Containers.Add(cont);
			ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container> adapter = new ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>(consol);

			Xsd.Container xmlCont = new Xsd.Container();
			cont.JC_ArrivalEstimatedDelivery = new ZDateTime(2005, 5, 5);

			adapter.ExportToValueObject(cont, xmlCont, new ValueObjectExportContext(notify));
			AssertEquals(new ZDateTime(2005, 5, 5), xmlCont.EstimatedDelivery);
		}

		public void TestExport_DeliveryMode_Empty()
		{
			Xsd.Container xsdCont = new Xsd.Container();
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonContainer cont = Factory.New<CommonContainer>();
			consol.Containers.Add(cont);
			ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container> adapter = new ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>(consol);

			cont.JC_DeliveryMode = String.Empty;
			Xsd.Container xmlCont = new Xsd.Container();

			adapter.ExportToValueObject(cont, xmlCont, new ValueObjectExportContext(notify));
			Assert(!xmlCont.DeliveryModeSpecified);
		}

		public void TestExport_DeliveryMode()
		{
			CommonContainer cont = Factory.New<CommonContainer>();
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.Containers.Add(cont);
			ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container> adapter = new ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>(consol);

			cont.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CFS_CFS;
			Xsd.Container xmlCont = new Xsd.Container();

			adapter.ExportToValueObject(cont, xmlCont, new ValueObjectExportContext(notify));
			AssertEquals(Core.Constants.DeliveryModes.Codes.CFS_CFS, xmlCont.DeliveryMode);
		}

		public void TestExport_LCLAvailable()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.Containers.Add(container);
			ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container> adapter = new ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>(consol);

			container.JC_LCLAvailable = new ZDateTime(2005, 11, 23, 16, 24, 43);
			Xsd.Container xsdContainer = new Xsd.Container();

			adapter.ExportToValueObject(container, xsdContainer, new ValueObjectExportContext(notify));
			AssertEquals("LCLAvailable", new ZDateTime(2005, 11, 23, 16, 24, 43), xsdContainer.LCLAvailable);
		}

		public void TestExport_FCLAvailable()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.Containers.Add(container);
			ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container> adapter = new ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>(consol);

			container.JC_FCLAvailable = new ZDateTime(2005, 11, 23, 16, 24, 43);
			Xsd.Container xsdContainer = new Xsd.Container();

			adapter.ExportToValueObject(container, xsdContainer, new ValueObjectExportContext(notify));
			AssertEquals("FCLAvailable", new ZDateTime(2005, 11, 23, 16, 24, 43), xsdContainer.FCLAvailable);
		}

		public void TestExport_ContainerCount()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.Containers.Add(container);
			ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container> adapter = new ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>(consol);

			container.JC_ContainerCount = 2;
			Xsd.Container xsdContainer = new Xsd.Container();

			adapter.ExportToValueObject(container, xsdContainer, new ValueObjectExportContext(notify));
			AssertEquals("ContainerCount", 2, xsdContainer.ContainerCount);
		}

		public void TestImport_ContainerCount()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonContainer container = Factory.New<CommonContainer>();
			consol.Containers.Add(container);
			ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container> adapter = new ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>(consol);

			Xsd.Container xsdContainer = new Xsd.Container();
			xsdContainer.ContainerCount = 2;
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			adapter.ImportFromValueObject(container, xsdContainer, context);
			AssertEquals("ContainerCount", (ZShort)2, container.JC_ContainerCount);
		}

		public void TestImport_ArrivalEstimatedDelivery_DeliveryMode_Empty()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonContainer cont = Factory.New<CommonContainer>();
			consol.Containers.Add(cont);
			ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container> adapter = new ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>(consol);
			AssertEquals(ZDateTime.Empty, cont.JC_ArrivalEstimatedDelivery);
			AssertEquals(String.Empty, cont.JC_DeliveryMode);
		}

		public void TestImport_ArrivalEstimatedDelivery_Invalid()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonContainer cont = Factory.New<CommonContainer>();
			consol.Containers.Add(cont);
			ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container> adapter = new ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>(consol);

			Xsd.Container xsdCont = new Xsd.Container();
			xsdCont.DeliveryModeSpecified = true;
			xsdCont.DeliveryMode = Core.Constants.DeliveryModes.Codes.CFS_CFS;

			xsdCont.EstimatedDelivery = ZDateTime.Invalid;
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			adapter.ImportFromValueObject(cont, xsdCont, context);
			Assert(!xsdCont.EstimatedDelivery.IsValid);
		}

		public void TestImport_ArrivalEstimatedDelivery_DeliveryMode()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonContainer cont = Factory.New<CommonContainer>();
			consol.Containers.Add(cont);
			ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container> adapter = new ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>(consol);

			Xsd.Container xsdCont = new Xsd.Container();
			xsdCont.DeliveryModeSpecified = true;
			xsdCont.DeliveryMode = Core.Constants.DeliveryModes.Codes.CFS_CFS;

			xsdCont.EstimatedDelivery = new DateTime(2005, 5, 5);
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			adapter.ImportFromValueObject(cont, xsdCont, context);
			AssertEquals(new ZDateTime(2005, 5, 5), cont.JC_ArrivalEstimatedDelivery);
			AssertEquals(Core.Constants.DeliveryModes.Codes.CFS_CFS, cont.JC_DeliveryMode);
		}

		public void TestImport_LCLAvailableDate()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonContainer container = Factory.New<CommonContainer>();
			consol.Containers.Add(container);
			ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container> adapter = new ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>(consol);

			Xsd.Container xsdContainer = new Xsd.Container();
			xsdContainer.LCLAvailable = new ZDateTime(2005, 11, 23, 16, 20, 23);
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			adapter.ImportFromValueObject(container, xsdContainer, context);
			AssertEquals("LCL Available Date", new ZDateTime(2005, 11, 23, 16, 20, 23), container.JC_LCLAvailable);
		}

		public void TestImport_FCLAvailable()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonContainer container = Factory.New<CommonContainer>();
			consol.Containers.Add(container);
			ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container> adapter = new ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>(consol);

			Xsd.Container xsdContainer = new Xsd.Container();
			xsdContainer.FCLAvailable = new ZDateTime(2005, 11, 23, 16, 20, 23);
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			adapter.ImportFromValueObject(container, xsdContainer, context);
			AssertEquals("FCL Available Date", new ZDateTime(2005, 11, 23, 16, 20, 23), container.JC_FCLAvailable);
		}

		public void TestExportContainerProperties()
		{
			Xsd.Container xsdContainer = new Xsd.Container();

			CommonConsol consol = Factory.New<CommonConsol>();
			CommonContainer freightContainer = Factory.New<CommonContainer>();
			freightContainer.JC_RH_NKContainerCommodityCode = "ABCD";
			freightContainer.JC_DepartureDeliveryByRail = ZBool.True;
			freightContainer.JC_IsEmptyContainer = ZBool.False;
			freightContainer.JC_IsDamaged = ZBool.False;
			freightContainer.JC_ReleaseNum = "BookingReference";
			freightContainer.JC_SetPointTemp = 36.6M;
			freightContainer.JC_SetPointTempUnit = "C";
			freightContainer.JC_GrossWeight = 1000m;

			#region Export Process
			freightContainer.JC_EmptyRequired = testDate.AddDays(20);
			freightContainer.JC_DepartureEstimatedPickup = testDate.AddDays(21);
			freightContainer.JC_DepartureCartageAdvised = testDate.AddDays(22);
			freightContainer.JC_DepartureSlotReference = "export slot ref";
			freightContainer.JC_DepartureSlotDateTime = testDate.AddDays(23);
			freightContainer.JC_DepartureCartageRef = "export cart ref";
			freightContainer.JC_ContainerYardEmptyPickupGateOut = testDate.AddDays(24);
			freightContainer.JC_FCLWharfGateIn = testDate.AddDays(25);
			freightContainer.JC_DepartureCartageComplete = testDate.AddDays(26);
			freightContainer.JC_FCLOnBoardVessel = testDate.AddDays(27);
			freightContainer.DepartureTruckWaitTime = testDate.AddDays(28);
			freightContainer.DepartureTruckWaitCost = 29M;

			freightContainer.JC_DepartureDeliveryByRail = ZBool.True;

			OrgHeader pickupFromOrg = Factory.NewWithValidTestData<OrgHeader>();
			pickupFromOrg.OH_FullName = "pickup from org";
			OrgAddress pickupFromOrgAddress = pickupFromOrg.Addresses.AddNew(OrgAddressType.Office, true);
			pickupFromOrgAddress.OA_Address1 = "pickup from addr 1";
			freightContainer.JC_OA_DepartureContainerYardAddress = pickupFromOrgAddress.PK;
			#endregion

			#region Import Process
			freightContainer.JC_ContainerImportDORelease = "ImportRelease";
			freightContainer.JC_FCLAvailable = testDate.AddDays(1);
			freightContainer.JC_ArrivalCTOStorageStartDate = testDate.AddDays(2);
			freightContainer.JC_LCLAvailable = testDate.AddDays(3);
			freightContainer.JC_LCLStorageCommences = testDate.AddDays(4);
			freightContainer.JC_FCLUnloadFromVessel = testDate.AddDays(5);
			freightContainer.JC_ArrivalSlotDateTime = testDate.AddDays(6);
			freightContainer.JC_ArrivalSlotReference = "import slot ref";
			freightContainer.JC_ArrivalCartageRef = "import cart ref";
			freightContainer.JC_FCLWharfGateOut = testDate.AddDays(7);
			freightContainer.JC_ArrivalEstimatedDelivery = testDate.AddDays(8);
			freightContainer.JC_ArrivalCartageAdvised = testDate.AddDays(9);
			freightContainer.JC_ArrivalCartageComplete = testDate.AddDays(10);
			freightContainer.JC_EmptyReadyForReturn = testDate.AddDays(11);
			freightContainer.JC_EmptyReturnedBy = testDate.AddDays(12);
			freightContainer.JC_ContainerYardEmptyReturnGateIn = testDate.AddDays(13);

			freightContainer.JC_FCLHeldInTransitStaging = false;

			freightContainer.ArrivalCTOStorageDays = 11;
			freightContainer.ArrivalCTOStorageCost = 11M;
			freightContainer.ArrivalTruckWaitTime = testDate.AddDays(14);
			freightContainer.ArrivalTruckWaitCost = 14M;
			freightContainer.ArrivalCarrierDetentionDays = 12;
			freightContainer.ArrivalCarrierDetentionCost = 12M;

			OrgHeader deliverEmptyToOrg = Factory.NewWithValidTestData<OrgHeader>();
			deliverEmptyToOrg.OH_FullName = "deliver empty to org";
			OrgAddress deliverEmptyToOrgAddress = deliverEmptyToOrg.Addresses.AddNew(OrgAddressType.Office, true);
			deliverEmptyToOrgAddress.OA_Address1 = "deliver empty to add 1";
			freightContainer.JC_OA_ArrivalContainerYardAddress = deliverEmptyToOrgAddress.PK;
			#endregion

			consol.Containers.Add(freightContainer);
			ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container> adapter = new ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>(consol);
			adapter.ExportToValueObject(freightContainer, xsdContainer, new ValueObjectExportContext(notify));

			AssertEquals("XsdContainer.CommodityCode", "ABCD", xsdContainer.CommodityCode);
			AssertEquals("XsdContainer.ReleaseNumber", "BookingReference", xsdContainer.ReleaseNumber);
			AssertEquals("XsdContainer.IsDamaged", ZBool.False, xsdContainer.IsDamaged);
			AssertEquals("XsdContainer.IsEmptyContainer", ZBool.False, xsdContainer.IsEmptyContainer);
			AssertEquals("XsdContainer.IsArrivingAtCTOByRail", ZBool.True, xsdContainer.IsArrivingAtCTOByRail);
			AssertEquals("XsdContainer.SetPointTemperature", 36.6M, xsdContainer.SetPointTemperature);
			AssertEquals("XsdContainer.SetPointTemperatureUnit", "C", xsdContainer.SetPointTemperatureUnit);
			AssertEquals("XsdContainer.SetPointTemperatureUnit", 1000m, xsdContainer.Weight);

			#region Export Process
			AssertEquals("EmptyRequiredBy", freightContainer.JC_EmptyRequired, xsdContainer.ExportProcess.EmptyRequiredBy);
			AssertEquals("EstimatedFullPickup", freightContainer.JC_DepartureEstimatedPickup, xsdContainer.ExportProcess.EstimatedFullPickup);
			AssertEquals("CartageAdvised", freightContainer.JC_DepartureCartageAdvised, xsdContainer.ExportProcess.CartageAdvised);
			AssertEquals("SlotBookingRef", freightContainer.JC_DepartureSlotReference, xsdContainer.ExportProcess.SlotBookingRef);
			AssertEquals("SlotDate", freightContainer.JC_DepartureSlotDateTime, xsdContainer.ExportProcess.SlotDate);
			AssertEquals("CartageRef", freightContainer.JC_DepartureCartageRef, xsdContainer.ExportProcess.CartageRef);
			AssertEquals("ContainerYardGateOut", freightContainer.JC_ContainerYardEmptyPickupGateOut, xsdContainer.ExportProcess.ContainerYardGateOut);
			AssertEquals("WharfGateIn", freightContainer.JC_FCLWharfGateIn, xsdContainer.ExportProcess.WharfGateIn);
			AssertEquals("CartageComplete", freightContainer.JC_DepartureCartageComplete, xsdContainer.ExportProcess.CartageComplete);
			AssertEquals("ShippedOnboard", freightContainer.JC_FCLOnBoardVessel, xsdContainer.ExportProcess.ShippedOnboard);
			AssertEquals("DemurrageTime", freightContainer.DepartureTruckWaitTime, xsdContainer.ExportProcess.DemurrageTime);
			AssertEquals("DemurrageCharge", freightContainer.DepartureTruckWaitCost, xsdContainer.ExportProcess.DemurrageCharge);

			AssertEquals("IsArrivingAtCTOByRail", freightContainer.JC_DepartureDeliveryByRail, xsdContainer.ExportProcess.IsArrivingAtCTOByRail);

			AssertEquals("Pick up from Org", pickupFromOrg.OH_FullName, xsdContainer.ExportProcess.PickupEmptyFrom.Organisation.OrganisationDetails.Name);
			AssertNotNull(xsdContainer.ImportProcess.DeliverEmptyTo.Organisation.OrganisationDetails.Addresses[0]);
			AssertEquals("Pick up from address", pickupFromOrgAddress.OA_Address1, xsdContainer.ExportProcess.PickupEmptyFrom.Organisation.OrganisationDetails.Addresses[0].AddressLine1);
			#endregion

			#region Import Process
			AssertEquals("ReleaseNumber", freightContainer.JC_ContainerImportDORelease, xsdContainer.ImportProcess.ReleaseNumber);
			AssertEquals("FCLAvailable", freightContainer.JC_FCLAvailable, xsdContainer.ImportProcess.FCLAvailable);
			AssertEquals("FCLStorage", freightContainer.JC_ArrivalCTOStorageStartDate, xsdContainer.ImportProcess.FCLStorage);
			AssertEquals("LCLAvailable", freightContainer.JC_LCLAvailable, xsdContainer.ImportProcess.LCLAvailable);
			AssertEquals("LCLStorage", freightContainer.JC_LCLStorageCommences, xsdContainer.ImportProcess.LCLStorage);
			AssertEquals("WharfUnload", freightContainer.JC_FCLUnloadFromVessel, xsdContainer.ImportProcess.WharfUnload);
			AssertEquals("SlotBookingRef", freightContainer.JC_ArrivalSlotDateTime, xsdContainer.ImportProcess.SlotDate);
			AssertEquals("SlotDate", freightContainer.JC_ArrivalSlotReference, xsdContainer.ImportProcess.SlotBookingRef);
			AssertEquals("CartageRef", freightContainer.JC_ArrivalCartageRef, xsdContainer.ImportProcess.CartageRef);
			AssertEquals("WharfGateOut", freightContainer.JC_FCLWharfGateOut, xsdContainer.ImportProcess.WharfGateOut);
			AssertEquals("EstimatedDelivery", freightContainer.JC_ArrivalEstimatedDelivery, xsdContainer.ImportProcess.EstimatedDelivery);
			AssertEquals("CartageAdvised", freightContainer.JC_ArrivalCartageAdvised, xsdContainer.ImportProcess.CartageAdvised);
			AssertEquals("CartageComplete", freightContainer.JC_ArrivalCartageComplete, xsdContainer.ImportProcess.CartageComplete);

			AssertEquals("EmptyReady", freightContainer.JC_EmptyReadyForReturn, xsdContainer.ImportProcess.EmptyReady);
			AssertEquals("EmptyReturnRequiredBy", freightContainer.JC_EmptyReturnedBy, xsdContainer.ImportProcess.EmptyReturnRequiredBy);
			AssertEquals("EmptyReturnedOn", freightContainer.JC_ContainerYardEmptyReturnGateIn, xsdContainer.ImportProcess.EmptyReturnedOn);

			AssertEquals("PickupByRail", freightContainer.JC_ArrivalPickupByRail, xsdContainer.ImportProcess.PickupByRail);
			AssertEquals("HeldForFCLTransitStaging", freightContainer.JC_FCLHeldInTransitStaging, xsdContainer.ImportProcess.HeldForFCLTransitStaging);

			AssertEquals("StorageDays", freightContainer.ArrivalCTOStorageDays.ToString(), xsdContainer.ImportProcess.StorageDays);
			AssertEquals("StorageCharge", freightContainer.ArrivalCTOStorageCost, xsdContainer.ImportProcess.StorageCharge);
			AssertEquals("DemurrageTime", freightContainer.ArrivalTruckWaitTime, xsdContainer.ImportProcess.DemurrageTime);
			AssertEquals("DemurrageCharge", freightContainer.ArrivalTruckWaitCost, xsdContainer.ImportProcess.DemurrageCharge);
			AssertEquals("DetentionDays", freightContainer.ArrivalCarrierDetentionDays.ToString(), xsdContainer.ImportProcess.DetentionDays);
			AssertEquals("DetentionCharge", freightContainer.ArrivalCarrierDetentionCost, xsdContainer.ImportProcess.DetentionCharge);

			AssertEquals("Deliver Empty to Org", deliverEmptyToOrg.OH_FullName, xsdContainer.ImportProcess.DeliverEmptyTo.Organisation.OrganisationDetails.Name);
			AssertNotNull(xsdContainer.ImportProcess.DeliverEmptyTo.Organisation.OrganisationDetails.Addresses[0]);
			AssertEquals("Deliver Empty to address", deliverEmptyToOrgAddress.OA_Address1, xsdContainer.ImportProcess.DeliverEmptyTo.Organisation.OrganisationDetails.Addresses[0].AddressLine1);
			#endregion
		}

		public void TestValueOfRegistryDefaultForImporting()
		{
			try
			{
				SystemRegistry.UpdateConsolContainersDuringAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				CommonConsol consol = Factory.New<CommonConsol>();
				ConsolContainerValueObjectDataAdapterForTest adapter = new ConsolContainerValueObjectDataAdapterForTest(consol);
				AssertEquals("adapter should be using shipment registry item which is currently true", true, adapter.ValueOfRegistryDefaultForImporting());

				SystemRegistry.UpdateConsolContainersDuringAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				AssertEquals("adapter should be using shipment registry item which is currently true", false, adapter.ValueOfRegistryDefaultForImporting());
			}
			finally
			{
				((IRegistryItemInternals)SystemRegistry.UpdateConsolContainersDuringAutomaticImport).ClearCache();
			}
		}

		protected override ValueObjectDataAdapter<CommonContainer, Xsd.Container> GetNewBizObjXmlDataAdapter()
		{
			return new ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>(Factory.New<CommonConsol>());
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.DataTransfer.Test.Freight.Testing.EmptyConsolContainer.xml", "EmptyConsolContainer.xml");
			return new BusinessObjectAndExpectedOutputFileName(Factory.New<CommonContainer>(), expectedOutputFilename, ValidationKind.None, "Empty container");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			CommonContainer freightContainer = Factory.New<CommonContainer>();

			freightContainer.JC_ContainerNum = "containernum";
			RefContainer refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20PL");
			refContainer.RC_USContainerCode = "U1";
			refContainer.SetCountrySpecificContainerCode("U2", Enterprise.Core.Constants.CountryCodes.UnitedStates);
			freightContainer.JC_RC = refContainer.PK;
			freightContainer.JC_SealNum = "SealNum";
			freightContainer.JC_AdditionalSealNum = "Seal2";
			freightContainer.JC_Additional2SealNum = "Seal3";
			freightContainer.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			freightContainer.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CY_CY;
			freightContainer.JC_ArrivalEstimatedDelivery = new ZDateTime(2005, 1, 2);
			freightContainer.JC_LCLAvailable = new ZDateTime(2005, 11, 23, 16, 27, 54);
			freightContainer.JC_FCLAvailable = new ZDateTime(2005, 11, 24, 10, 20, 44);
			freightContainer.JC_ReleaseNum = "BookingRef";
			freightContainer.JC_RH_NKContainerCommodityCode = "ABCD";
			freightContainer.JC_SetPointTemp = 36.6M;
			freightContainer.JC_SetPointTempUnit = "C";
			freightContainer.JC_HumidityPercent = 20;
			freightContainer.JC_AirVentFlow = 10;
			freightContainer.JC_AirVentFlowRateUnit = "2L";
			freightContainer.JC_IsShipperOwned = true;

			#region Export Process

			freightContainer.JC_EmptyRequired = testDate.AddDays(20);
			freightContainer.JC_DepartureEstimatedPickup = testDate.AddDays(21);
			freightContainer.JC_DepartureCartageAdvised = testDate.AddDays(22);
			freightContainer.JC_DepartureSlotReference = "export slot ref";
			freightContainer.JC_DepartureSlotDateTime = testDate.AddDays(23);
			freightContainer.JC_DepartureCartageRef = "export cart ref";
			freightContainer.JC_ContainerYardEmptyPickupGateOut = testDate.AddDays(24);
			freightContainer.JC_FCLWharfGateIn = testDate.AddDays(25);
			freightContainer.JC_DepartureCartageComplete = testDate.AddDays(26);
			freightContainer.JC_FCLOnBoardVessel = testDate.AddDays(27);
			freightContainer.DepartureTruckWaitTime = testDate.AddDays(28);
			freightContainer.DepartureTruckWaitCost = 29M;

			freightContainer.JC_DepartureDeliveryByRail = ZBool.True;

			OrgHeader pickupFromOrg = Factory.NewWithValidTestData<OrgHeader>();
			pickupFromOrg.OH_FullName = "pickup from org";
			OrgAddress pickupFromOrgAddress = pickupFromOrg.Addresses.AddNew(OrgAddressType.Office, true);
			pickupFromOrgAddress.OA_Address1 = "pickup from addr 1";
			freightContainer.JC_OA_DepartureContainerYardAddress = pickupFromOrgAddress.PK;

			#endregion

			#region Import Process

			freightContainer.JC_ContainerImportDORelease = "ImportRelease";
			freightContainer.JC_FCLAvailable = testDate.AddDays(1);
			freightContainer.JC_ArrivalCTOStorageStartDate = testDate.AddDays(2);
			freightContainer.JC_LCLAvailable = testDate.AddDays(3);
			freightContainer.JC_LCLStorageCommences = testDate.AddDays(4);
			freightContainer.JC_FCLUnloadFromVessel = testDate.AddDays(5);
			freightContainer.JC_ArrivalSlotDateTime = testDate.AddDays(6);
			freightContainer.JC_ArrivalSlotReference = "import slot ref";
			freightContainer.JC_ArrivalCartageRef = "import cart ref";
			freightContainer.JC_FCLWharfGateOut = testDate.AddDays(7);
			freightContainer.JC_ArrivalEstimatedDelivery = testDate.AddDays(8);
			freightContainer.JC_ArrivalCartageAdvised = testDate.AddDays(9);
			freightContainer.JC_ArrivalCartageComplete = testDate.AddDays(10);
			freightContainer.JC_EmptyReadyForReturn = testDate.AddDays(11);
			freightContainer.JC_EmptyReturnedBy = testDate.AddDays(12);
			freightContainer.JC_ContainerYardEmptyReturnGateIn = testDate.AddDays(13);

			freightContainer.JC_FCLHeldInTransitStaging = false;

			freightContainer.ArrivalCTOStorageDays = 11;
			freightContainer.ArrivalCTOStorageCost = 11M;
			freightContainer.ArrivalTruckWaitTime = testDate.AddDays(14);
			freightContainer.ArrivalTruckWaitCost = 14M;
			freightContainer.ArrivalCarrierDetentionDays = 12;
			freightContainer.ArrivalCarrierDetentionCost = 12M;

			OrgHeader deliverEmptyToOrg = Factory.NewWithValidTestData<OrgHeader>();
			deliverEmptyToOrg.OH_FullName = "deliver empty to org";
			OrgAddress deliverEmptyToOrgAddress = deliverEmptyToOrg.Addresses.AddNew(OrgAddressType.Office, true);
			deliverEmptyToOrgAddress.OA_Address1 = "deliver empty to add 1";
			freightContainer.JC_OA_ArrivalContainerYardAddress = deliverEmptyToOrgAddress.PK;

			#endregion

			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.DataTransfer.Test.Freight.Testing.PopulatedConsolContainer.xml", "PopulatedConsolContainer.xml");
			return new BusinessObjectAndExpectedOutputFileName(freightContainer, expectedOutputFilename, ValidationKind.Xsd, "Populated container");
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				return new string[]
				{
					"BookingReference",	// deprecated
					"ImportProcess/DeliverEmptyTo/Organisation",
					"ExportProcess/PickupEmptyFrom/Organisation",
					"ExportProcess/BookingReference",	// deprecated
					"Custom",
					"AirVentFlow",
					"AirVentFlowRateUnit",
				};
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
