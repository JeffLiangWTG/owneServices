using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportConsignment.DataTransfer.Universal
{
	public abstract class LandTransportImportValidationHelper
	{
		#region ReasonForNotAbleToUpdateFromDataSourceOrTargetBO

		public ZString ReasonForNotAbleToUpdateFromDataSourceOrTargetBO(UniversalShipment consignmentDataObject, UniversalObjectFactory factory, IColumnIndexer[] existingConsignments = null)
		{
			ZString reason = ZString.Empty;

			if (IsPickUpConfirmed(consignmentDataObject))
			{
				reason = Res.GetString("9e9075ef-d99a-4fd9-b78b-4e60138b5030", "Cannot Import Pick Up information as a Pick Up Consignment already exists for this Booking.");
			}
			else if (consignmentDataObject.ShipmentStatus.GetCodeAsUpperCase() != TransportStatuses.Codes.Available) // Available is considered Delivery Confirmed in GLOW
			{
				reason = Res.GetString("efcfb8a6-1845-4ea0-9690-6c6098516bbc", "Cannot Update as a Confirmed Consignment already exists for this Booking.");
			}
			else
			{
				reason = CheckPickUpAndDelivery(consignmentDataObject, factory, existingConsignments);
			}

			return reason;
		}

		#endregion

		#region CheckPickUpAndDelivery

		ZString CheckPickUpAndDelivery(UniversalShipment consignmentDataObject, UniversalObjectFactory factory, IColumnIndexer[] existingConsignments)
		{
			ZString reason = ZString.Empty;

			var deliveryInstructions = GetInstructions(consignmentDataObject, InstructionTypeDelivery);
			if (!deliveryInstructions.Any())
			{
				reason = Res.GetString("2acea7c8-4223-4b34-a91c-1b1284691829", "Cannot Update Pick Up Consignment as there is no Delivery Information.");
			}
			else
			{
				var pickUpInstructions = consignmentDataObject.InstructionCollection != null
					? consignmentDataObject.InstructionCollection.Where(i => i.Type.GetCodeAsUpperCase() == InstructionTypePickup).ToArray()
					: System.Array.Empty<Instruction>();

				if (pickUpInstructions.Length != existingConsignments.Length)
				{
					reason = Res.GetString("3ea8caa9-d456-432a-baa7-6c2bd231186f", "Cannot Update as the number of Pick Up Consignments and Booking Pick Up Instructions are different.");
				}
				else if (pickUpInstructions.Length != 1)
				{
					reason = Res.GetString("3b7b9ae5-0d3f-476e-969f-2db3ff164e34", "Cannot Update as there should only be one Pick Up Instruction.");
				}
				else
				{
					reason = CheckPickUpAndDeliveryAddress(pickUpInstructions.Single(), factory, existingConsignments.Single());
				}
			}

			return reason;
		}

		#endregion

		#region CheckPickUpAndDeliveryAddress

		ZString CheckPickUpAndDeliveryAddress(Instruction pickUpInstruction, UniversalObjectFactory factory, IColumnIndexer pickUpConsignment)
		{
			ZString reason = ZString.Empty;

			var pickUpJobDocAddressRow = GetPickUpAddress(pickUpConsignment, factory);
			var pickUpJobDocAddress = factory.Load<JobDocAddress>(pickUpJobDocAddressRow.GetValue(JobDocAddressSchema.PK)); // bizO required for matching logic

			var organisationReader = new OrganisationDataObjectReader(pickUpInstruction.Address, new UniversalDataBuss.Integration.DummyLogger(), factory);
			var pickUpAddress = organisationReader.GetMatched();
			if (!organisationReader.IsJobDocAddressMatchingOrgAddress(pickUpAddress, pickUpJobDocAddress))
			{
				reason = Res.GetString("22682cf7-03e0-4902-84e2-b1f900123394", "Cannot Update as Existing Consignment has a different Pick Up Address.");
			}
			else
			{
				var deliveryJobDocAddress = GetDeliveryAddress(pickUpConsignment, factory);
				if (deliveryJobDocAddress != null
					&& (deliveryJobDocAddress.GetValue(JobDocAddressSchema.E2_AddressOverride) || deliveryJobDocAddress.GetValue(JobDocAddressSchema.E2_OA_Address).IsValid))
				{
					reason = Res.GetString("fdd0f485-23e2-4b97-b3df-32f11f7d2e5b", "Cannot Update as Existing Consignment already has Delivery Information.");
				}
			}

			return reason;
		}

		IColumnIndexer GetDeliveryAddress(IColumnIndexer consignment, UniversalObjectFactory factory)
		{
			return GetAddressFromInstruction(consignment, InstructionTypeDelivery, factory);
		}

		IColumnIndexer GetPickUpAddress(IColumnIndexer consignment, UniversalObjectFactory factory)
		{
			return GetAddressFromInstruction(consignment, InstructionTypePickup, factory);
		}

		IColumnIndexer GetAddressFromInstruction(IColumnIndexer consignment, string instructionType, UniversalObjectFactory factory)
		{
			var consignmentAddress = GetConsignmentInstruction(consignment, instructionType, factory);
			var jobDocAddressQuery = new ZQuery();
			jobDocAddressQuery.AddToFilter(JobDocAddressSchema.E2_ParentID, consignmentAddress.GetValue(ConsignmentInstructionPKSchemaColumn));
			jobDocAddressQuery.MaximumRows = 1;

			return DataObjectReader.GetColumnIndexerFromRow(factory.RowFactory.Load(JobDocAddressSchema.Constants.TableName, jobDocAddressQuery).SingleOrDefault()); // only one Address for a Instruction
		}

		IColumnIndexer GetConsignmentInstruction(IColumnIndexer consignmentRow, ZString addressType, UniversalObjectFactory factory)
		{
			var query = GetQueryToLoadInstruction(consignmentRow, addressType);
			return DataObjectReader.GetColumnIndexerFromRow(factory.RowFactory.Load(InstructionTableName, query).Single()); // only one Instruction of each type should exist.
		}

		#endregion

		#region GetInstructions

		IEnumerable<Instruction> GetInstructions(UniversalShipment consignmentDataObject, string instructionType)
		{
			return !IsPickUpConfirmed(consignmentDataObject) && consignmentDataObject.InstructionCollection != null
				? consignmentDataObject.InstructionCollection.Where(i => i.Type.GetCodeAsUpperCase() == instructionType)
				: Enumerable.Empty<Instruction>();
		}

		#endregion

		#region IsPickUpConfirmed

		bool IsPickUpConfirmed(UniversalShipment consignmentDataObject)
		{
			return consignmentDataObject.ShipmentStatus.GetCodeAsUpperCase() == TransportStatuses.Codes.PickUpConfirmed;
		}

		#endregion

		#region Abstract Methods

		public abstract SchemaPKColumn ConsignmentInstructionPKSchemaColumn { get; }

		public abstract string InstructionTypeDelivery { get; }

		public abstract string InstructionTypePickup { get; }

		public abstract ZString InstructionTableName { get; }

		public abstract ZQuery GetQueryToLoadInstruction(IColumnIndexer consignmentRow, ZString addressType);

		#endregion

	}
}


