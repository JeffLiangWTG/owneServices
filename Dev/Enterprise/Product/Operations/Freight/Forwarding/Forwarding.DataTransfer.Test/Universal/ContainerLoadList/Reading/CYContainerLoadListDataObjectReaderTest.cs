using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Core.Constants;
using UniversalCodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using UniversalContainer = Enterprise.UniversalDataBuss.DataObjects.Universal.Container;
using UniversalContainerType = Enterprise.UniversalDataBuss.DataObjects.Universal.ContainerType;
using UniversalLoadMode = Enterprise.UniversalDataBuss.DataObjects.Universal.LoadMode;
using UniversalOrganizationAddress = Enterprise.UniversalDataBuss.DataObjects.Universal.OrganizationAddress;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class CYContainerLoadListDataObjectReaderTest : OrganizationAddressTestHelper
	{
		public void TestBasicFieldMappings()
		{
			var bookingParty = Factory.NewWithValidTestData<OrgHeader>();
			bookingParty.OH_Code = "BKXXX";

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var uShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			uShipment.DataContext = DataContextFactory.New();

			var originalContainerLoadList = ContainerLoadListDataObjectHelper.BuildDataForTest(Factory, SupplierBookingLoadModeList.Codes.CY);
			var supplierBooking = originalContainerLoadList.Booking;
			uShipment.DataContext.AddDataTarget(DataContextType.ContainerLoadList, originalContainerLoadList.CLH_LoadListId);

			Factory.SaveForTesting();

			TestCommonCheck(uShipment, ContainerLoadListHeaderLoadMode.ContainerYard);

			ContainerLoadListDataObjectHelper.SetDataSouceForSupplierBooking(uShipment, DefaultDataObjectWriterStrategy.TestInstance, supplierBooking.JSB_BookingId);
			uShipment.ShipmentStatus = ListHelper.GetWithDescription<UniversalCodeDescriptionPair>(CommonContainerLoadListStatusList.Codes.INC, new CommonContainerLoadListStatusList());
			uShipment.SetOrganizationAddressCollection(() => new List<UniversalOrganizationAddress>());
			uShipment.OrganizationAddressCollection.Add(new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.BookingPartyDocumentaryAddress)).GetDataObject(bookingParty.MainAddress));
			uShipment.SetNoteCollection(() => new DataObjectList<UniversalDataBuss.DataObjects.Universal.Note>());
			uShipment.NoteCollection.Add(new UniversalDataBuss.DataObjects.Universal.Note
			{
				Description = "Detailed Goods Description",
				NoteContext = new UniversalDataBuss.DataObjects.Universal.NoteContext { Code = "AAA", Description = "Module: A - All, Direction: A - All, Freight: A - All" },
				Visibility = new UniversalCodeDescriptionPair { Code = "PUB", Description = "CLIENT-VISIBLE" },
				NoteText = "DetailedGoodsDescription Test",
				IsCustomDescription = false
			});

			var containerLoadList = TryReadIntoBusinessObject(uShipment);
			AssertEquals(supplierBooking.PK, containerLoadList.CLH_JSB_Booking);
			AssertEquals(CommonContainerLoadListStatusList.Codes.INC, containerLoadList.CLH_Status);
			AssertEquals(bookingParty.PK, containerLoadList.CLH_OH_LoadListParty);
			AssertEquals(originalContainerLoadList.PK, containerLoadList.PK);
		}

		public void TestReadFromExportCLL()
		{
			var manager = new ContainerLoadListDataContextManager();
			var originalContainerLoadList = ContainerLoadListDataObjectHelper.BuildDataForTest(Factory);
			var writer = (manager as IShipmentDataContextManager).GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, originalContainerLoadList)));
			var universalShipment = writer.GetDataObject(originalContainerLoadList) as UniversalShipment;
			universalShipment.DataContext.AddDataTarget(DataContextType.ContainerLoadList, originalContainerLoadList.CLH_LoadListId);
			var resultContainerLoadList = new CYContainerLoadListDataObjectReader(universalShipment, Logger, Factory).ReadIntoBusinessObject();

			AssertEquals(originalContainerLoadList.CLH_LoadListId, resultContainerLoadList.CLH_LoadListId);
			AssertEquals(originalContainerLoadList.CLH_JSB_Booking, resultContainerLoadList.CLH_JSB_Booking);
			AssertEquals(originalContainerLoadList.CLH_OH_LoadListParty, resultContainerLoadList.CLH_OH_LoadListParty);
			AssertEquals(originalContainerLoadList.CLH_Status, resultContainerLoadList.CLH_Status);
			AssertEquals(originalContainerLoadList.LoadListLines.Count, resultContainerLoadList.LoadListLines.Count);

			var originalContainerLoadListLine = originalContainerLoadList.LoadListLines[0];
			var resultContainerLoadListLine = resultContainerLoadList.LoadListLines[0];

			AssertEquals(originalContainerLoadListLine.CLL_Description, resultContainerLoadListLine.CLL_Description);
			AssertEquals(originalContainerLoadListLine.CLL_MarksAndNumbers, resultContainerLoadListLine.CLL_MarksAndNumbers);
			AssertEquals(originalContainerLoadListLine.CLL_F3_NKPackagesUnit, resultContainerLoadListLine.CLL_F3_NKPackagesUnit);
			AssertEquals(originalContainerLoadListLine.CLL_HarmonizedCode, resultContainerLoadListLine.CLL_HarmonizedCode);
			AssertEquals(originalContainerLoadListLine.CLL_JC_Container, resultContainerLoadListLine.CLL_JC_Container);
			AssertEquals(originalContainerLoadListLine.CLL_JSL_BookingLine, resultContainerLoadListLine.CLL_JSL_BookingLine);
			AssertEquals(originalContainerLoadListLine.CLL_LoadSequence, resultContainerLoadListLine.CLL_LoadSequence);
			AssertEquals(originalContainerLoadListLine.CLL_Packages, resultContainerLoadListLine.CLL_Packages);
			AssertEquals(originalContainerLoadListLine.CLL_PackedQuantity, resultContainerLoadListLine.CLL_PackedQuantity);
			AssertEquals(originalContainerLoadListLine.CLL_ReferenceNumber, resultContainerLoadListLine.CLL_ReferenceNumber);
			AssertEquals(originalContainerLoadListLine.CLL_RH_NKCommodityCode, resultContainerLoadListLine.CLL_RH_NKCommodityCode);
			AssertEquals(originalContainerLoadListLine.CLL_Volume, resultContainerLoadListLine.CLL_Volume);
			AssertEquals(originalContainerLoadListLine.CLL_VolumeUnit, resultContainerLoadListLine.CLL_VolumeUnit);
			AssertEquals(originalContainerLoadListLine.CLL_Weight, resultContainerLoadListLine.CLL_Weight);
			AssertEquals(originalContainerLoadListLine.CLL_WeightUnit, resultContainerLoadListLine.CLL_WeightUnit);
		}

		void TestCommonCheck(UniversalShipment uShipment, string loadMode)
		{
			ReadAndCheckError(uShipment, "load mode should be required", "There is no load mode.");

			uShipment.LoadMode = new UniversalLoadMode { Code = ContainerLoadListHeaderLoadMode.ContainerFreightStation };
			ReadAndCheckError(uShipment, "valid load mode should be required", "Load mode only supports container yard.");

			uShipment.LoadMode = new UniversalLoadMode { Code = loadMode };
			ReadAndCheckError(uShipment, "status shold be required", "Status should not be null or empty.");

			uShipment.ShipmentStatus = new UniversalCodeDescriptionPair { Code = "CNV" };
			ReadAndCheckError(uShipment, "should only import while status is appliable", "Status should not be Canceled, Shipped or Converted.");

			uShipment.ShipmentStatus = new UniversalCodeDescriptionPair { Code = "INC" };
			ReadAndCheckError(uShipment, "supplier booking should be required", "There is no supplier booking key.");

			ContainerLoadListDataObjectHelper.SetDataSouceForSupplierBooking(uShipment, DefaultDataObjectWriterStrategy.TestInstance, "SBKXXX");
			AssertExceptionThrown<DataObjectReadFailureException>("supplier booking should be valid", "The supplier booking SBKXXX does not exist.", () => TryReadIntoBusinessObject(uShipment));

			ContainerLoadListDataObjectHelper.SetDataSouceForSupplierBooking(uShipment, DefaultDataObjectWriterStrategy.TestInstance, "SBK001");

			uShipment.SetParentShipmentCollection(() => new List<UniversalShipment>());
			TryReadIntoBusinessObject(uShipment);
			AssertContains("no containers warning", "There are no containers.", Logger.GetWarnings());

			uShipment.ParentShipmentCollection.Add(new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance));
			uShipment.ParentShipmentCollection[0].DataContext = DataContextFactory.New();
			uShipment.ParentShipmentCollection[0].DataContext.AddDataSource(DataContextType.ForwardingConsol, "");
			ReadAndCheckError(uShipment, "consol key should be required", "There is no consol key.");

			uShipment.ParentShipmentCollection[0].DataContext = DataContextFactory.New();
			uShipment.ParentShipmentCollection[0].DataContext.AddDataSource(DataContextType.ForwardingConsol, "AAA");
			ReadAndCheckError(uShipment, "consol should exist", "The consol AAA does not exist.");

			uShipment.ParentShipmentCollection[0].DataContext.AddDataSource(DataContextType.ForwardingConsol, "CON001");
			uShipment.ParentShipmentCollection[0].SetContainerCollection(() => new DataObjectList<UniversalContainer> { new UniversalContainer { ContainerNumber = "AAA", ContainerType = new UniversalContainerType { Code = "20GP" }, Link = 1 }, new UniversalContainer { ContainerNumber = "BBB", ContainerType = new UniversalContainerType { Code = "40GP" }, Link = 2 } });
			AssertNoExceptionThrown(() => TryReadIntoBusinessObject(uShipment));
		}

		CommonContainerLoadList TryReadIntoBusinessObject(UniversalShipment uShipment)
		{
			Logger.ClearLogs();
			return new CYContainerLoadListDataObjectReader(uShipment, Logger, Factory).ReadIntoBusinessObject();
		}

		void ReadAndCheckError(UniversalShipment uShipment, string message, string expectedError)
		{
			Logger.ClearLogs();
			TryReadIntoBusinessObject(uShipment);
			AssertContains(message, expectedError, Logger.GetErrors());
		}
	}
}
