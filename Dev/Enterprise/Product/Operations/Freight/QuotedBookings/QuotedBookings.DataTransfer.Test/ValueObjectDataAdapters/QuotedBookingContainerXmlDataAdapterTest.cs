using System;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Testing;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Test
{
	[TestedType(typeof(QuotedBookingContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>))]
	public class QuotedBookingContainerXmlDataAdapterTest : JobContainerXmlDataAdapterTest
	{
		public void TestImportContainerMatchingByContainerCodeFirst()
		{
			RefContainer container2060 = new RefContainer.Loader(Factory).LoadFromISOType("22P0");
			AssertNotNull("Precondition 20PL", container2060);
			RefContainer container20GP = new RefContainer.Loader(Factory).LoadFromCode("20GP");
			AssertNotNull("Precondition 20GP", container20GP);
			QuotedBooking quotedBooking = QuotedBooking.New(Enterprise.Freight.Integration.QuoteBookingType.QuickBooking, Factory);
			CommonContainer container = Factory.New<CommonContainer>();
			quotedBooking.QuotedBookingContainers.Add(container);
			Xsd.Container containerValue = new Xsd.Container();
			containerValue.ContainerType.ContainerCode = "ABC";
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			QuotedBookingContainerValueObjectDataAdapter<CommonContainer, Xsd.Container> adapter = new QuotedBookingContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>(quotedBooking);
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
			xsdContainer.ExportProcess.IsArrivingAtCTOByRail = ZBool.True;
			Xsd.Organisation xsdPickupFromOrg = xsdContainer.ExportProcess.PickupEmptyFrom.Organisation;
			xsdPickupFromOrg.OrganisationDetails.Name = "pickup from org";
			Xsd.OrgAddress xsdPickupFromOrgAddress = xsdPickupFromOrg.OrganisationDetails.Addresses.AddNew();
			xsdPickupFromOrgAddress.AddressLine1 = "pickup from org addr 1";
			xsdContainer.ExportProcess.PickupEmptyFrom.AddressSequenceRef = 1;
			#endregion
			#region Import Process
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
			xsdContainer.ImportProcess.DemurrageTime = testDate.AddDays(26).AddYears(ZDateTime.DefaultDurationEpoch.Year - testDate.Year);
			xsdContainer.ImportProcess.DemurrageCharge = 27M;
			xsdContainer.ImportProcess.DemurrageChargeSpecified = true;
			Xsd.Organisation xsdDeliverEmptyToOrg = xsdContainer.ImportProcess.DeliverEmptyTo.Organisation;
			xsdDeliverEmptyToOrg.OrganisationDetails.Name = "deliver empty to org";
			Xsd.OrgAddress xsdDeliverEmptyToOrgAddress = xsdDeliverEmptyToOrg.OrganisationDetails.Addresses.AddNew();
			xsdDeliverEmptyToOrgAddress.AddressLine1 = "deliver empty to addr 1";
			xsdContainer.ImportProcess.DeliverEmptyTo.AddressSequenceRef = 1;
			#endregion
			QuotedBooking quotedBooking = QuotedBooking.New(Enterprise.Freight.Integration.QuoteBookingType.QuickBooking, Factory);
			CommonContainer container = Factory.New<CommonContainer>();
			quotedBooking.QuotedBookingContainers.Add(container);
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			QuotedBookingContainerValueObjectDataAdapter<CommonContainer, Xsd.Container> adapter = new QuotedBookingContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>(quotedBooking);
			adapter.ImportFromValueObject(container, xsdContainer, context);
			AssertEquals("Container JC_IsEmptyContainer", ZBool.False, container.JC_IsEmptyContainer);
			AssertEquals("Container JC_ReleaseNum", "ContainerBooking", container.JC_ReleaseNum);
			AssertEquals("Container JC_RH_NKContainerCommodityCode", "ABCD", container.JC_RH_NKContainerCommodityCode);
			AssertEquals("Container JC_SetPointTemp", 36.6M, container.JC_SetPointTemp);
			AssertEquals("Container JC_SetPointTempUnit", "C", container.JC_SetPointTempUnit);
			AssertEquals("Container JC_GrossWeight", 1000m, container.JC_GrossWeight);
			#region Export Process
			AssertEquals("EmptyRequiredBy", xsdContainer.ExportProcess.EmptyRequiredBy, container.JC_EmptyRequired);
			AssertEquals("EstimatedFullPickup", xsdContainer.ExportProcess.EstimatedFullPickup, container.JC_DepartureEstimatedPickup);
			AssertEquals("CartageAdvised", xsdContainer.ExportProcess.CartageAdvised, container.JC_DepartureCartageAdvised);
			AssertEquals("SlotBookingRef", xsdContainer.ExportProcess.SlotBookingRef, container.JC_DepartureSlotReference);
			AssertEquals("SlotDate", xsdContainer.ExportProcess.SlotDate, container.JC_DepartureSlotDateTime);
			AssertEquals("CartageRef", xsdContainer.ExportProcess.CartageRef, container.JC_DepartureCartageRef);
			AssertEquals("ContainerYardGateOut", xsdContainer.ExportProcess.ContainerYardGateOut, container.JC_ContainerYardEmptyPickupGateOut);
			AssertEquals("WharfGateIn", xsdContainer.ExportProcess.WharfGateIn, container.JC_FCLWharfGateIn);
			AssertEquals("CartageComplete", xsdContainer.ExportProcess.CartageComplete, container.JC_DepartureCartageComplete);
			AssertEquals("ShippedOnboard", xsdContainer.ExportProcess.ShippedOnboard, container.JC_FCLOnBoardVessel);
			AssertEquals("DemurrageTime", xsdContainer.ExportProcess.DemurrageTime, container.DepartureTruckWaitTime);
			AssertEquals("DemurrageCharge", xsdContainer.ExportProcess.DemurrageCharge, container.DepartureTruckWaitCost);
			AssertNotNull(container.DepartureContainerYardAddress);
			OrgHeader pickupFromOrg = Factory.Load<OrgHeader>(container.DepartureContainerYardAddress.OA_OH);
			AssertEquals("pick up from org", xsdPickupFromOrg.OrganisationDetails.Name, pickupFromOrg.OH_FullName);
			AssertEquals("pick up from org addr 1", xsdPickupFromOrgAddress.AddressLine1, container.DepartureContainerYardAddress.OA_Address1);
			#endregion
			#region Import Process
			AssertEquals("FCLAvailable", xsdContainer.ImportProcess.FCLAvailable, container.JC_FCLAvailable);
			AssertEquals("FCLStorage", xsdContainer.ImportProcess.FCLStorage, container.JC_ArrivalCTOStorageStartDate);
			AssertEquals("LCLAvailable", xsdContainer.ImportProcess.LCLAvailable, container.JC_LCLAvailable);
			AssertEquals("LCLStorage", xsdContainer.ImportProcess.LCLStorage, container.JC_LCLStorageCommences);
			AssertEquals("WharfUnload", xsdContainer.ImportProcess.WharfUnload, container.JC_FCLUnloadFromVessel);
			AssertEquals("SlotBookingRef", xsdContainer.ImportProcess.SlotBookingRef, container.JC_ArrivalSlotReference);
			AssertEquals("SlotDate", xsdContainer.ImportProcess.SlotDate, container.JC_ArrivalSlotDateTime);
			AssertEquals("CartageRef", xsdContainer.ImportProcess.CartageRef, container.JC_ArrivalCartageRef);
			AssertEquals("WharfGateOut", xsdContainer.ImportProcess.WharfGateOut, container.JC_FCLWharfGateOut);
			AssertEquals("EstimatedDelivery", xsdContainer.ImportProcess.EstimatedDelivery, container.JC_ArrivalEstimatedDelivery);
			AssertEquals("CartageAdvised", xsdContainer.ImportProcess.CartageAdvised, container.JC_ArrivalCartageAdvised);
			AssertEquals("CartageComplete", xsdContainer.ImportProcess.CartageComplete, container.JC_ArrivalCartageComplete);
			AssertEquals("EmptyReady", xsdContainer.ImportProcess.EmptyReady, container.JC_EmptyReadyForReturn);
			AssertEquals("EmptyReturnRequiredBy", xsdContainer.ImportProcess.EmptyReturnRequiredBy, container.JC_EmptyReturnedBy);
			AssertEquals("EmptyReturnedOn", xsdContainer.ImportProcess.EmptyReturnedOn, container.JC_ContainerYardEmptyReturnGateIn);
			AssertEquals("DemurrageTime", xsdContainer.ImportProcess.DemurrageTime, container.ArrivalTruckWaitTime);
			AssertNotNull(container.ArrivalContainerYardAddress);
			OrgHeader deliverEmptyToOrg = Factory.Load<OrgHeader>(container.ArrivalContainerYardAddress.OA_OH);
			AssertEquals("deliver empty to org", xsdDeliverEmptyToOrg.OrganisationDetails.Name, deliverEmptyToOrg.OH_FullName);
			AssertEquals("deliver empty to addr 1", xsdDeliverEmptyToOrgAddress.AddressLine1, container.ArrivalContainerYardAddress.OA_Address1);
			#endregion
		}

		public void TestExport_ArrivalEstimatedDelivery_Empty()
		{
			Xsd.Container xsdCont = new Xsd.Container();
			QuotedBooking quotedBooking = QuotedBooking.New(Enterprise.Freight.Integration.QuoteBookingType.QuickBooking, Factory);
			CommonContainer cont = Factory.New<CommonContainer>();
			quotedBooking.QuotedBookingContainers.Add(cont);
			QuotedBookingContainerValueObjectDataAdapter<CommonContainer, Xsd.Container> adapter = new QuotedBookingContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>(quotedBooking);
			Xsd.Container xmlCont = new Xsd.Container();
			adapter.ExportToValueObject(cont, xmlCont, new ValueObjectExportContext(notify));
			Assert(xmlCont.EstimatedDelivery.IsEmpty);
		}

		public void TestExport_ArrivalEstimatedDelivery()
		{
			Xsd.Container xsdCont = new Xsd.Container();
			QuotedBooking quotedBooking = QuotedBooking.New(Enterprise.Freight.Integration.QuoteBookingType.QuickBooking, Factory);
			CommonContainer cont = Factory.New<CommonContainer>();
			quotedBooking.QuotedBookingContainers.Add(cont);
			QuotedBookingContainerValueObjectDataAdapter<CommonContainer, Xsd.Container> adapter = new QuotedBookingContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>(quotedBooking);
			Xsd.Container xmlCont = new Xsd.Container();
			cont.JC_ArrivalEstimatedDelivery = new ZDateTime(2005, 5, 5);
			adapter.ExportToValueObject(cont, xmlCont, new ValueObjectExportContext(notify));
			AssertEquals(new ZDateTime(2005, 5, 5), xmlCont.EstimatedDelivery);
		}

		public void TestExport_DeliveryMode_Empty()
		{
			Xsd.Container xsdCont = new Xsd.Container();
			QuotedBooking quotedBooking = QuotedBooking.New(Enterprise.Freight.Integration.QuoteBookingType.QuickBooking, Factory);
			CommonContainer cont = Factory.New<CommonContainer>();
			quotedBooking.QuotedBookingContainers.Add(cont);
			QuotedBookingContainerValueObjectDataAdapter<CommonContainer, Xsd.Container> adapter = new QuotedBookingContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>(quotedBooking);
			cont.JC_DeliveryMode = String.Empty;
			Xsd.Container xmlCont = new Xsd.Container();
			adapter.ExportToValueObject(cont, xmlCont, new ValueObjectExportContext(notify));
			Assert(!xmlCont.DeliveryModeSpecified);
		}

		public void TestExport_DeliveryMode()
		{
			CommonContainer cont = Factory.New<CommonContainer>();
			QuotedBooking quotedBooking = QuotedBooking.New(Enterprise.Freight.Integration.QuoteBookingType.QuickBooking, Factory);
			quotedBooking.QuotedBookingContainers.Add(cont);
			QuotedBookingContainerValueObjectDataAdapter<CommonContainer, Xsd.Container> adapter = new QuotedBookingContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>(quotedBooking);
			cont.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CFS_CFS;
			Xsd.Container xmlCont = new Xsd.Container();
			adapter.ExportToValueObject(cont, xmlCont, new ValueObjectExportContext(notify));
			AssertEquals(Core.Constants.DeliveryModes.Codes.CFS_CFS, xmlCont.DeliveryMode);
		}

		public void TestExport_LCLAvailable()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			QuotedBooking quotedBooking = QuotedBooking.New(Enterprise.Freight.Integration.QuoteBookingType.QuickBooking, Factory);
			quotedBooking.QuotedBookingContainers.Add(container);
			QuotedBookingContainerValueObjectDataAdapter<CommonContainer, Xsd.Container> adapter = new QuotedBookingContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>(quotedBooking);
			container.JC_LCLAvailable = new ZDateTime(2005, 11, 23, 16, 24, 43);
			Xsd.Container xsdContainer = new Xsd.Container();
			adapter.ExportToValueObject(container, xsdContainer, new ValueObjectExportContext(notify));
			AssertEquals("LCLAvailable", new ZDateTime(2005, 11, 23, 16, 24, 43), xsdContainer.LCLAvailable);
		}

		public void TestExport_FCLAvailable()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			QuotedBooking quotedBooking = QuotedBooking.New(Enterprise.Freight.Integration.QuoteBookingType.QuickBooking, Factory);
			quotedBooking.QuotedBookingContainers.Add(container);
			QuotedBookingContainerValueObjectDataAdapter<CommonContainer, Xsd.Container> adapter = new QuotedBookingContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>(quotedBooking);
			container.JC_FCLAvailable = new ZDateTime(2005, 11, 23, 16, 24, 43);
			Xsd.Container xsdContainer = new Xsd.Container();
			adapter.ExportToValueObject(container, xsdContainer, new ValueObjectExportContext(notify));
			AssertEquals("FCLAvailable", new ZDateTime(2005, 11, 23, 16, 24, 43), xsdContainer.FCLAvailable);
		}

		public void TestExport_ContainerCount()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			QuotedBooking quotedBooking = QuotedBooking.New(Enterprise.Freight.Integration.QuoteBookingType.QuickBooking, Factory);
			quotedBooking.QuotedBookingContainers.Add(container);
			QuotedBookingContainerValueObjectDataAdapter<CommonContainer, Xsd.Container> adapter = new QuotedBookingContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>(quotedBooking);
			container.JC_ContainerCount = 2;
			Xsd.Container xsdContainer = new Xsd.Container();
			adapter.ExportToValueObject(container, xsdContainer, new ValueObjectExportContext(notify));
			AssertEquals("ContainerCount", 2, xsdContainer.ContainerCount);
		}

		public void TestImport_ContainerCount()
		{
			QuotedBooking quotedBooking = QuotedBooking.New(Enterprise.Freight.Integration.QuoteBookingType.QuickBooking, Factory);
			CommonContainer container = Factory.New<CommonContainer>();
			quotedBooking.QuotedBookingContainers.Add(container);
			QuotedBookingContainerValueObjectDataAdapter<CommonContainer, Xsd.Container> adapter = new QuotedBookingContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>(quotedBooking);
			Xsd.Container xsdContainer = new Xsd.Container();
			xsdContainer.ContainerCount = 2;
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			adapter.ImportFromValueObject(container, xsdContainer, context);
			AssertEquals("ContainerCount", (ZShort)2, container.JC_ContainerCount);
		}

		public void TestImport_ArrivalEstimatedDelivery_DeliveryMode_Empty()
		{
			QuotedBooking quotedBooking = QuotedBooking.New(Enterprise.Freight.Integration.QuoteBookingType.QuickBooking, Factory);
			CommonContainer cont = Factory.New<CommonContainer>();
			quotedBooking.QuotedBookingContainers.Add(cont);
			QuotedBookingContainerValueObjectDataAdapter<CommonContainer, Xsd.Container> adapter = new QuotedBookingContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>(quotedBooking);
			AssertEquals(ZDateTime.Empty, cont.JC_ArrivalEstimatedDelivery);
			AssertEquals(String.Empty, cont.JC_DeliveryMode);
		}

		public void TestImport_ArrivalEstimatedDelivery_DeliveryMode()
		{
			QuotedBooking quotedBooking = QuotedBooking.New(Enterprise.Freight.Integration.QuoteBookingType.QuickBooking, Factory);
			CommonContainer cont = Factory.New<CommonContainer>();
			quotedBooking.QuotedBookingContainers.Add(cont);
			QuotedBookingContainerValueObjectDataAdapter<CommonContainer, Xsd.Container> adapter = new QuotedBookingContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>(quotedBooking);
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
			QuotedBooking quotedBooking = QuotedBooking.New(Enterprise.Freight.Integration.QuoteBookingType.QuickBooking, Factory);
			CommonContainer container = Factory.New<CommonContainer>();
			quotedBooking.QuotedBookingContainers.Add(container);
			QuotedBookingContainerValueObjectDataAdapter<CommonContainer, Xsd.Container> adapter = new QuotedBookingContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>(quotedBooking);
			Xsd.Container xsdContainer = new Xsd.Container();
			xsdContainer.LCLAvailable = new ZDateTime(2005, 11, 23, 16, 20, 23);
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			adapter.ImportFromValueObject(container, xsdContainer, context);
			AssertEquals("LCL Available Date", new ZDateTime(2005, 11, 23, 16, 20, 23), container.JC_LCLAvailable);
		}

		public void TestImport_FCLAvailable()
		{
			QuotedBooking quotedBooking = QuotedBooking.New(Enterprise.Freight.Integration.QuoteBookingType.QuickBooking, Factory);
			CommonContainer container = Factory.New<CommonContainer>();
			quotedBooking.QuotedBookingContainers.Add(container);
			QuotedBookingContainerValueObjectDataAdapter<CommonContainer, Xsd.Container> adapter = new QuotedBookingContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>(quotedBooking);
			Xsd.Container xsdContainer = new Xsd.Container();
			xsdContainer.FCLAvailable = new ZDateTime(2005, 11, 23, 16, 20, 23);
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			adapter.ImportFromValueObject(container, xsdContainer, context);
			AssertEquals("FCL Available Date", new ZDateTime(2005, 11, 23, 16, 20, 23), container.JC_FCLAvailable);
		}

		public void TestExportContainerProperties()
		{
			Xsd.Container xsdContainer = new Xsd.Container();
			QuotedBooking quotedBooking = QuotedBooking.New(Enterprise.Freight.Integration.QuoteBookingType.QuickBooking, Factory);
			CommonContainer freightContainer = Factory.New<CommonContainer>();
			freightContainer.JC_RH_NKContainerCommodityCode = "ABCD";
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
			OrgHeader pickupFromOrg = Factory.NewWithValidTestData<OrgHeader>();
			pickupFromOrg.OH_FullName = "pickup from org";
			OrgAddress pickupFromOrgAddress = pickupFromOrg.Addresses.AddNew(OrgAddressType.Office, true);
			pickupFromOrgAddress.OA_Address1 = "pickup from addr 1";
			freightContainer.JC_OA_DepartureContainerYardAddress = pickupFromOrgAddress.PK;
			#endregion
			#region Import Process
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
			freightContainer.ArrivalTruckWaitTime = testDate.AddDays(14);
			OrgHeader deliverEmptyToOrg = Factory.NewWithValidTestData<OrgHeader>();
			deliverEmptyToOrg.OH_FullName = "deliver empty to org";
			OrgAddress deliverEmptyToOrgAddress = deliverEmptyToOrg.Addresses.AddNew(OrgAddressType.Office, true);
			deliverEmptyToOrgAddress.OA_Address1 = "deliver empty to add 1";
			freightContainer.JC_OA_ArrivalContainerYardAddress = deliverEmptyToOrgAddress.PK;
			#endregion
			quotedBooking.QuotedBookingContainers.Add(freightContainer);
			QuotedBookingContainerValueObjectDataAdapter<CommonContainer, Xsd.Container> adapter = new QuotedBookingContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>(quotedBooking);
			adapter.ExportToValueObject(freightContainer, xsdContainer, new ValueObjectExportContext(notify));
			AssertEquals("XsdContainer.CommodityCode", "ABCD", xsdContainer.CommodityCode);
			AssertEquals("XsdContainer.ReleaseNumber", "BookingReference", xsdContainer.ReleaseNumber);
			AssertEquals("XsdContainer.IsDamaged", ZBool.False, xsdContainer.IsDamaged);
			AssertEquals("XsdContainer.IsEmptyContainer", ZBool.False, xsdContainer.IsEmptyContainer);
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
			AssertEquals("Pick up from Org", pickupFromOrg.OH_FullName, xsdContainer.ExportProcess.PickupEmptyFrom.Organisation.OrganisationDetails.Name);
			AssertNotNull(xsdContainer.ImportProcess.DeliverEmptyTo.Organisation.OrganisationDetails.Addresses[0]);
			AssertEquals("Pick up from address", pickupFromOrgAddress.OA_Address1, xsdContainer.ExportProcess.PickupEmptyFrom.Organisation.OrganisationDetails.Addresses[0].AddressLine1);
			#endregion
			#region Import Process
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
			AssertEquals("DemurrageTime", freightContainer.ArrivalTruckWaitTime, xsdContainer.ImportProcess.DemurrageTime);
			AssertEquals("Deliver Empty to Org", deliverEmptyToOrg.OH_FullName, xsdContainer.ImportProcess.DeliverEmptyTo.Organisation.OrganisationDetails.Name);
			AssertNotNull(xsdContainer.ImportProcess.DeliverEmptyTo.Organisation.OrganisationDetails.Addresses[0]);
			AssertEquals("Deliver Empty to address", deliverEmptyToOrgAddress.OA_Address1, xsdContainer.ImportProcess.DeliverEmptyTo.Organisation.OrganisationDetails.Addresses[0].AddressLine1);
			#endregion
		}

		public void TestValueOfRegistryDefaultForImporting()
		{
			try
			{
				SystemRegistry.UpdateBookingContainersDuringAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				QuotedBooking quotedBooking = QuotedBooking.New(Enterprise.Freight.Integration.QuoteBookingType.QuickBooking, Factory);
				QuotedBookingContainerValueObjectDataAdapterForTest adapter = new QuotedBookingContainerValueObjectDataAdapterForTest(quotedBooking);
				AssertEquals("adapter should be using registry item which is currently true", true, adapter.ValueOfRegistryDefaultForImporting());
				SystemRegistry.UpdateBookingContainersDuringAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				AssertEquals("adapter should be using shipment registry item which is currently false", false, adapter.ValueOfRegistryDefaultForImporting());
			}
			finally
			{
				((IRegistryItemInternals)SystemRegistry.UpdateConsolContainersDuringAutomaticImport).ClearCache();
			}
		}

		protected override ValueObjectDataAdapter<CommonContainer, Xsd.Container> GetNewBizObjXmlDataAdapter()
		{
			base.GetNewBizObjXmlDataAdapter();
			QuotedBooking quotedBooking = QuotedBooking.New(Enterprise.Freight.Integration.QuoteBookingType.QuickBooking, Factory);
			return new QuotedBookingContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>(quotedBooking);
		}
	}
}
