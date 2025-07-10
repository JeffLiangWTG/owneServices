using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Testing
{
	class LandTransportConsignmentImportValidationHelperTest : LandTransportImportValidationHelperTest<DtbConsignment>
	{
		#region Implementation
		protected override DtbConsignment CreateConsignmentWithPickupAndDeliveryInstructionWithoutOrgAddress()
		{
			var consignment = HelperLT.CreateConsignment("LT001");
			HelperLT.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			HelperLT.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);
			return consignment;
		}

		protected override DtbConsignment CreateConsignmentWithPickupAndDeliveryInstructionWithOrgAddress()
		{
			var pickupOrg = Helper.CreateOrganisation("PCK1", address1: "123 Test st");
			var deliveryOrg = Helper.CreateOrganisation("Dlb1", address1: "456 Test st");
			var consignment = HelperLT.CreateConsignment("LT001");
			HelperLT.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp, pickupOrg.MainAddress);
			HelperLT.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery, deliveryOrg.MainAddress);
			return consignment;
		}

		protected override void CreateAdditionalConsignmentWithPickupAndDeliveryInstruction(string bookingID, DtbConsignment consignment)
		{
			var secondConsignment = HelperLT.CreateConsignment("LT002");
			var secondPickupAddress = HelperLT.CreateConsignmentAddress(secondConsignment, ConsignmentAddressTypes.Codes.PickUp, consignment.PickupAddress.Address.Address);
			HelperLT.CreateConsignmentAddress(secondConsignment, ConsignmentAddressTypes.Codes.Delivery);
			var reference = secondConsignment.AdditionalReferenceNumbers.AddNew();
			reference.CE_EntryType = TransportCommonAdditionalReferenceTypes.Codes.BookingJobId;
			reference.CE_EntryNum = bookingID;
		}

		protected override void SetDeliveryAddressEmpty(DtbConsignment consignment)
		{
			consignment.DeliveryAddress.Address.E2_OA_Address = ZGuid.Empty;
		}

		protected override OrgAddress GetGetPickupOrgAddress(DtbConsignment consignment)
		{
			return consignment.PickupAddress.Address.Address;
		}

		protected override void LinkConsignmentWithBooking(IDtbBooking booking, DtbConsignment consignment)
		{
			var reference = consignment.AdditionalReferenceNumbers.AddNew();
			reference.CE_EntryType = TransportCommonAdditionalReferenceTypes.Codes.BookingJobId;
			reference.CE_EntryNum = booking.KM_JobID.ToString();
		}

		protected override LandTransportImportValidationHelper GetImportValidationHelper()
		{
			return new LandTransportConsignmentImportValidationHelper();
		}

		protected override IColumnIndexer[] GetExistingConsignmentsCore(IDtbBooking booking, DtbConsignment consignment)
		{
			IColumnIndexer[] result = Array.Empty<IColumnIndexer>();
			if (booking != null)
			{
				var consignmentQuery = new ZDBOnlyQuery(typeof(DtbConsignment));
				var additionalReferencesQuery = new ZDBOnlySubQuery(typeof(ICusEntryNumber), CusEntryNumSchema.CE_ParentID);
				additionalReferencesQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, TransportCommonAdditionalReferenceTypes.Codes.BookingJobId);
				additionalReferencesQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, booking.KM_JobID);
				consignmentQuery.AddSubQuery(additionalReferencesQuery, JoinCondition.And);
				var existingConsignments = Factory.RowFactory.Load(DtbConsignmentSchema.Constants.TableName, consignmentQuery);
				result = Array.ConvertAll(existingConsignments, DataObjectReader.GetColumnIndexerFromRow);
			}

			return result;
		}
		#endregion
	}
}
