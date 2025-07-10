using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class ContainerLoadListDataObjectHelper : OrganizationAddressTestHelper
	{
		public static void SetDataSouceForSupplierBooking(UniversalShipment uShipment, IDataObjectWriterStrategy strategy, ZString supplierBookingId)
		{
			var supplierBookingDataObject = new UniversalShipment(strategy);
			supplierBookingDataObject.DataContext = DataContextFactory.New();
			supplierBookingDataObject.DataContext.AddDataSource(DataContextType.JobSupplierBooking, supplierBookingId);
			uShipment.SetRelatedShipmentCollection(() => new List<UniversalShipment> { supplierBookingDataObject });
		}

		public static CommonContainerLoadList BuildDataForTest(UniversalObjectFactory factory, string loadMode = SupplierBookingLoadModeList.Codes.CY, bool shouldLinkContainer = true)
		{
			var supplierBooking = JobSupplierBookingDataObjectHelperTest.BuildJobSupplierBookingForTest(factory, loadMode);

			var bookingParty = CreateOrgWithCode(factory.BOFactory, "BKT001");
			var loadList = CreateContainerLoadList(factory.BOFactory, "CLL0001", supplierBooking, bookingParty, ContainerLoadListHeaderStatus.Incomplete, loadMode);

			var container = BuildContainerForSupplierBooking(factory, supplierBooking);

			var loadListLine = AddLoadListLine(loadList, supplierBooking.SupplierBookingLines[0], container, shouldLinkContainer, volume: 12.3m, weight: 34.5m, packages: 45, quantity: 67, sequence: 78);
			BuildShipmentFromLoadList(factory, loadListLine);

			return loadList;
		}

		public static ForwardingContainer BuildContainerForSupplierBooking(UniversalObjectFactory factory, JobSupplierBooking supplierBooking)
		{
			var container = CreateContainer(factory, CreateConsol(factory, "CON001"), "MWLF9771112");

			container.JC_DeliveryMode = "CY/CY";
			container.JC_ContainerMode = "LCL";
			container.JC_JSB_SupplierBooking = supplierBooking.PK;
			return container;
		}

		public static OrgHeader CreateOrgWithCode(BusinessObjectFactory factory, string code)
		{
			var org = factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = code;
			return org;
		}

		public static ForwardingConsol CreateConsol(UniversalObjectFactory factory, string consolID = "CON001")
		{
			var consol = factory.BOFactory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_UniqueConsignRef = consolID;

			var transport = consol.Transports[0];
			transport.JW_ETD = new ZDateTime(2022, 2, 1);
			transport.JW_ETA = new ZDateTime(2022, 2, 3);
			return consol;
		}

		public static ForwardingContainer CreateContainer(UniversalObjectFactory factory, ForwardingConsol consol, string containerNumber = "OOVQ3027364")
		{
			var container = consol.Containers.AddNew();
			container.FillWithValidTestData();
			container.JC_ContainerNum = containerNumber;
			var twentyGP = factory.BOFactory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			if (twentyGP == null)
			{
				twentyGP = factory.BOFactory.New<RefContainer>();
				twentyGP.RC_Code = "20GP";
				twentyGP.RC_ISOType = "1234";
			}
			container.JC_RC = twentyGP.PK;
			container.JC_ContainerCount = 1;

			container.JC_ContainerMode = "LCL";
			return container;
		}

		public static CommonContainerLoadList CreateContainerLoadList(BusinessObjectFactory factory, string loadListID, JobSupplierBooking booking, OrgHeader bookingParty, string status = ContainerLoadListHeaderStatus.Shipped, string loadMode = ContainerLoadListHeaderLoadMode.ContainerYard)
		{
			CommonContainerLoadList containerLoadList = factory.New<CYContainerLoadList>();
			containerLoadList.CLH_LoadListId = loadListID;
			containerLoadList.CLH_Status = status;
			containerLoadList.CLH_LoadMode = loadMode;
			containerLoadList.CLH_PlannedTransportMode = TransportModes.Sea;
			containerLoadList.CLH_GoodsDescription = "Goods Desc";
			containerLoadList.CLH_DetailedGoodsDescription = "Detailed Good Desc";
			containerLoadList.CLH_MarksAndNumbers = "Marks&Nos";

			containerLoadList.CLH_JSB_Booking = booking.PK;
			containerLoadList.CLH_OH_LoadListParty = bookingParty.PK;
			
			return containerLoadList;
		}

		static ContainerLoadListLine AddLoadListLine(
			CommonContainerLoadList containerLoadList,
			JobSupplierBookingLine bookingLine,
			ForwardingContainer container,
			bool shouldLinkContainer,
			decimal volume = 0.0m,
			string volumeUnit = Volume.CubicMetres,
			decimal weight = 0.0m,
			string weightUnit = Weight.Kilograms,
			int packages = 0,
			string packagesUnit = PkgUnit.Piece,
			int quantity = 0,
			int sequence = 0)
		{
			var loadListLine = containerLoadList.LoadListLines.AddNew();
			loadListLine.CLL_JSL_BookingLine = bookingLine.PK;
			loadListLine.CLL_LoadMode = containerLoadList.CLH_LoadMode;
			loadListLine.CLL_JC_Container = shouldLinkContainer ? (container?.PK ?? ZGuid.Empty) : ZGuid.Empty;
			if (containerLoadList is CYContainerLoadList)
			{
				loadListLine.CLL_Volume = volume;
				loadListLine.CLL_Weight = weight;
				loadListLine.CLL_PackedQuantity = quantity;
				loadListLine.CLL_Packages = packages;
			}

			loadListLine.CLL_WeightUnit = weightUnit;
			loadListLine.CLL_LoadSequence = sequence;
			loadListLine.CLL_VolumeUnit = volumeUnit;
			loadListLine.CLL_F3_NKPackagesUnit = packagesUnit;
			loadListLine.CLL_HarmonizedCode = "HC0001";
			loadListLine.CLL_RH_NKCommodityCode = "GEN";
			loadListLine.CLL_ReferenceNumber = "RNXX87";
			loadListLine.CLL_Description = "Desc";
			loadListLine.CLL_MarksAndNumbers = "Line Marks&Nos";

			return loadListLine;
		}

		static ForwardingShipment BuildShipmentFromLoadList(UniversalObjectFactory factory, ContainerLoadListLine loadListLine)
		{
			var shipment = factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001001";

			var packLine = shipment.OuterPackLines.AddNew();
			loadListLine.CLL_JL_PackLine = packLine.PK;
			packLine.JL_JC = loadListLine.CLL_JC_Container;
			return shipment;
		}
	}
}
