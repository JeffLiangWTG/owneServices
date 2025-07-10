using System.Collections.Generic;
using CargoWise.Common.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Testing
{
	abstract class LandTransportImportValidationHelperTest<T> : OrganizationAddressTestHelper where T : BusinessObject
	{
		#region TestRejectImport_WhenUpdatingExistingConsignmentForPickUpConfirmedBooking
		public void TestRejectImport_WhenUpdatingExistingConsignmentForPickUpConfirmedBooking()
		{
			var consignment = CreateConsignmentWithPickupAndDeliveryInstructionWithoutOrgAddress();
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				var booking = SetupConsignmentDataObjectForUpdateAndSetupLogger(consignmentDataObject, consignment, "TB123", TransportStatuses.Codes.PickUpConfirmed);
				var reason = CallReasonForNotAbleToUpdateFromDataSourceOrTargetBO(consignmentDataObject, booking, consignment);
				AssertEquals("Cannot Import Pick Up information as a Pick Up Consignment already exists for this Booking.", reason);
				consignmentDataObject.ShipmentStatus.Code = TransportStatuses.Codes.PickUpCommenced;
				reason = CallReasonForNotAbleToUpdateFromDataSourceOrTargetBO(consignmentDataObject, booking, consignment);
				AssertEquals("Cannot Update as a Confirmed Consignment already exists for this Booking.", reason);
			}
		}

		#endregion
		#region TestRejectImport_WhenUpdatingExistingConsignmentWithDeliveryAddressEntered
		public void TestRejectImport_WhenUpdatingExistingConsignmentWithDeliveryAddressEntered()
		{
			var consignment = CreateConsignmentWithPickupAndDeliveryInstructionWithOrgAddress();
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				var booking = SetupConsignmentDataObjectForUpdateAndSetupLogger(consignmentDataObject, consignment, "TB123");
				var reason = CallReasonForNotAbleToUpdateFromDataSourceOrTargetBO(consignmentDataObject, booking, consignment);
				AssertEquals("Cannot Update as Existing Consignment already has Delivery Information.", reason);
			}
		}

		#endregion
		#region TestRejectImport_WhenUpdatingExistingConsignmentWithOneConsignmentAndMultiplePickUpInstructions
		public void TestRejectImport_WhenUpdatingExistingConsignmentWithOneConsignmentAndMultiplePickUpInstructions()
		{
			var consignment = CreateConsignmentWithPickupAndDeliveryInstructionWithOrgAddress();
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				var booking = SetupConsignmentDataObjectForUpdateAndSetupLogger(consignmentDataObject, consignment, "TB123");
				consignmentDataObject.InstructionCollection.Add(UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.PickUp));
				var reason = CallReasonForNotAbleToUpdateFromDataSourceOrTargetBO(consignmentDataObject, booking, consignment);
				AssertEquals("Cannot Update as the number of Pick Up Consignments and Booking Pick Up Instructions are different.", reason);
			}
		}

		#endregion
		#region TestRejectImport_WhenUpdatingExistingConsignmentWithOneConsignmentsAndMultiplePickUpInstructions
		public void TestRejectImport_WhenUpdatingExistingConsignmentWithOneConsignmentsAndMultiplePickUpInstructions()
		{
			var bookingID = "TB456";
			var consignment = CreateConsignmentWithPickupAndDeliveryInstructionWithOrgAddress();
			CreateAdditionalConsignmentWithPickupAndDeliveryInstruction(bookingID, consignment);
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				var booking = SetupConsignmentDataObjectForUpdateAndSetupLogger(consignmentDataObject, consignment, bookingID);
				consignmentDataObject.InstructionCollection.Add(UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.PickUp));
				var reason = CallReasonForNotAbleToUpdateFromDataSourceOrTargetBO(consignmentDataObject, booking, consignment);
				AssertEquals("Cannot Update as there should only be one Pick Up Instruction.", reason);
			}
		}

		#endregion
		#region TestRejectImport_WhenUpdatingExistingConsignmentWithoutDeliveryInformation
		public void TestRejectImport_WhenUpdatingExistingConsignmentWithoutDeliveryInformation()
		{
			var consignment = CreateConsignmentWithPickupAndDeliveryInstructionWithOrgAddress();
			SetDeliveryAddressEmpty(consignment);
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				var booking = SetupConsignmentDataObjectForUpdateAndSetupLogger(consignmentDataObject, consignment, "TB123");
				consignmentDataObject.InstructionCollection.RemoveAll(i => i.Type.GetCodeAsUpperCase() == InstructionTypes.Codes.Delivery); // remove delivery instruction
				var reason = CallReasonForNotAbleToUpdateFromDataSourceOrTargetBO(consignmentDataObject, booking, consignment);
				AssertEquals("Cannot Update Pick Up Consignment as there is no Delivery Information.", reason);
			}
		}

		#endregion
		#region TestRejectImport_WhenUpdatingExistingConsignmentWithoutMatchingPickUpAddress
		public void TestRejectImport_WhenUpdatingExistingConsignmentWithoutMatchingPickUpAddress()
		{
			var consignment = CreateConsignmentWithPickupAndDeliveryInstructionWithOrgAddress();
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				var booking = SetupConsignmentDataObjectForUpdateAndSetupLogger(consignmentDataObject, consignment, "TB123", pickupAddress: GetNewAddressData_CRAHOLSYD(DocAddressType.LocalCartageExporter));
				var reason = CallReasonForNotAbleToUpdateFromDataSourceOrTargetBO(consignmentDataObject, booking, consignment);
				AssertEquals("Cannot Update as Existing Consignment has a different Pick Up Address.", reason);
			}
		}

		#endregion
		#region Abstract Methods
		protected abstract T CreateConsignmentWithPickupAndDeliveryInstructionWithoutOrgAddress();
		protected abstract T CreateConsignmentWithPickupAndDeliveryInstructionWithOrgAddress();
		protected abstract void CreateAdditionalConsignmentWithPickupAndDeliveryInstruction(string bookingID, T consignment);
		protected abstract OrgAddress GetGetPickupOrgAddress(T consignment);
		protected abstract void LinkConsignmentWithBooking(IDtbBooking booking, T consignment);
		protected abstract void SetDeliveryAddressEmpty(T consignment);
		protected abstract LandTransportImportValidationHelper GetImportValidationHelper();
		protected abstract IColumnIndexer[] GetExistingConsignmentsCore(IDtbBooking booking, T consignment);
		#endregion
		#region Helper Methods
		IDtbBooking SetupConsignmentDataObjectForUpdateAndSetupLogger(UniversalShipment consignmentDataObject, T consignment, string bookingID, string bookingStatus = TransportStatuses.Codes.Available, OrganizationAddress pickupAddress = null)
		{
			var booking = Helper.CreateBooking(bookingID);
			LinkConsignmentWithBooking(booking, consignment);
			Factory.SaveForTesting();
			var deliveryAddress = GetNewAddressData_WUFSHIJNB(DocAddressType.LocalCartageImporter);
			UniversalTestHelper.CreateAddressInDB(deliveryAddress, Factory);
			var dataWritingManager = new DataWritingManager(new DummyActionInfo());
			OrganizationAddress pickUpOrganisationAddressDataObject = null;
			if (pickupAddress != null)
			{
				pickUpOrganisationAddressDataObject = pickupAddress;
			}
			else
			{
				var pickUpOrgAddress = GetGetPickupOrgAddress(consignment);
				if (pickUpOrgAddress != null)
				{
					var writer = new OrganizationDataObjectWriter(dataWritingManager, nameof(DocAddressType.LocalCartageExporter));
					pickUpOrganisationAddressDataObject = writer.GetDataObject(pickUpOrgAddress);
				}
			}

			var consolidationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consignmentDataObject.DataContext = DataContextFactory.New();
			var pickupInstructionDataObject = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.PickUp, pickUpOrganisationAddressDataObject);
			var deliveryInstructionDataObject = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.Delivery, deliveryAddress);
			consignmentDataObject.SetInstructionCollection(() => new DataObjectList<Instruction> { pickupInstructionDataObject, deliveryInstructionDataObject });
			consignmentDataObject.ShipmentStatus = new CodeDescriptionPair { Code = bookingStatus };
			consignmentDataObject.SetParentShipmentCollection(() => new List<UniversalShipment> { consolidationDataObject });
			Logger.OutboundSessionTracker = dataWritingManager;
			Logger.TopLevelDataObject = consignmentDataObject;
			Logger.TopLevelDataContext.AddDataSource(DataContextType.TransportBooking, bookingID);
			Logger.TopLevelDataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			return booking;
		}

		IColumnIndexer[] GetExistingConsignments(UniversalObjectFactory factory, IDtbBooking booking, T consignment)
		{
			return GetExistingConsignmentsCore(booking, consignment);
		}

		ZString CallReasonForNotAbleToUpdateFromDataSourceOrTargetBO(UniversalShipment consignmentDataObject, IDtbBooking booking, T consignment)
		{
			var factory = new UniversalObjectFactory();
			var consignmentsFromBooking = GetExistingConsignments(factory, booking, consignment);
			return GetImportValidationHelper().ReasonForNotAbleToUpdateFromDataSourceOrTargetBO(consignmentDataObject, factory, consignmentsFromBooking);
		}

		#endregion
		#region Helper
		protected TransportBookingConsignmentTestHelper Helper
		{
			get
			{
				return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory.BOFactory));
			}
		}

		TransportBookingConsignmentTestHelper helper;
		protected TransportConsignmentTestHelper HelperLT
		{
			get
			{
				return helperLT ?? (helperLT = new TransportConsignmentTestHelper(Factory.BOFactory));
			}
		}

		TransportConsignmentTestHelper helperLT;
		#endregion
	}
}
