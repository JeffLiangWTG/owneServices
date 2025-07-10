using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.TransportCommon.DataTransfer.Universal;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.DataTransfer.Universal.Common;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportConsignment.DataTransfer.Universal
{
	class DtbConsignmentConsolidationDataObjectReader : DtbTransportConsolidationDataObjectReader<DtbConsignmentConsolidation>
	{
		public DtbConsignmentConsolidationDataObjectReader(UniversalShipment consolidationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, DtbBookingConsignment consignment = null)
			: base(consolidationDataObject, logger, factory)
		{
			Consignment = consignment;
		}

		readonly DtbBookingConsignment Consignment;

		#region Context

		public override DataContextType DataContextType
		{
			get { return DataContextType.TransportConsignmentConsolidation; }
		}

		#endregion

		#region Matching Job

		protected override DtbConsignmentConsolidation GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			return Consignment != null ? Consignment.ConsolidationSingleJob : GetConsolidationLinkedToTB();
		}

		DtbConsignmentConsolidation GetConsolidationLinkedToTB()
		{
			DtbConsignmentConsolidation result = null;

			var parentBookingRow = GetColumnIndexerFromRow(ParentBooking);
			if (parentBookingRow != null)
			{
				var query = new ZQuery();
				query.AddToFilter(DtbBookingConsolidationSchema.KB_JobType, TransportConsolidationJobTypes.Codes.Consignment);
				query.AddToFilter(DtbBookingConsolidationSchema.KB_ParentID, parentBookingRow.GetValue(DtbBookingSchema.PK));
				result = factory.LoadTop1<DtbConsignmentConsolidation>(query); // there should only be one consignment consolidation for a Booking
			}

			return result;
		}

		protected override IMatchingBusinessEntityFinder<DtbConsignmentConsolidation> GetCombinedReferenceMatcher()
		{
			// No references to match a consolidation
			return null;
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(DtbConsignmentConsolidation targetBO)
		{
			ZString reason = ZString.Empty;

			var existingConsolidationRow = GetColumnIndexerFromRow(targetBO);
			if (existingConsolidationRow != null && ParentBooking != null) // we only need the following validation when updating a Pick Up Consignment for a Booking
			{
				var consignmentDataObject = GetConsignmentDataObject();
				reason = new DtbConsignmentImportValidationHelper().ReasonForNotAbleToUpdateFromDataSourceOrTargetBO(consignmentDataObject, factory, GetExistingConsignments(existingConsolidationRow));
			}

			return reason.IsEmpty ? base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(targetBO) : reason;
		}

		bool IsPickUpConfirmed(UniversalShipment consignmentDataObject)
		{
			return consignmentDataObject.ShipmentStatus.GetCodeAsUpperCase() == TransportStatuses.Codes.PickUpConfirmed;
		}

		#region GetExistingConsignments

		IColumnIndexer[] GetExistingConsignments(IColumnIndexer existingConsolidationRow)
		{
			var query = new ZQuery(DtbBookingSchema.KM_KB_Booking, existingConsolidationRow.GetValue(DtbBookingConsolidationSchema.PK));
			var existingConsignments = factory.RowFactory.Load(DtbBookingSchema.Constants.TableName, query);
			return Array.ConvertAll(existingConsignments, GetColumnIndexerFromRow);
		}

		#endregion

		#region ParentBooking

		BusinessObject ParentBooking
		{
			get { return (parentBookingCache ?? (parentBookingCache = new CachedValue<BusinessObject>(GetParentBooking))).Value; }
		}

		BusinessObject GetParentBooking()
		{
			BusinessObject result = null;

			if (logger.IsInternalImport())
			{
				var parentDataSource = logger.TopLevelDataContext.GetMatchingDataSource(DataContextType.TransportBooking);
				result = parentDataSource.GetLoadedJobFromDataContextType(logger.TopLevelDataObject, factory.BOFactory);
			}

			return result;
		}

		CachedValue<BusinessObject> parentBookingCache;

		#endregion

		#endregion

		#region Create / Update

		protected override void PopulateBusinessObject(DtbConsignmentConsolidation consolidation)
		{
			LinkParent(consolidation);
			PopulateRelatedEntities(consolidation);
		}

		void LinkParent(DtbConsignmentConsolidation consolidation)
		{
			if (ParentBooking != null)
			{
				var consolidationRow = GetColumnIndexerFromRow(consolidation);
				if (consolidationRow.GetValue(DtbBookingConsolidationSchema.KB_ParentID).IsEmpty)
				{
					var parentBookingRow = GetColumnIndexerFromRow(ParentBooking);
					SetValue(consolidationRow, DtbBookingConsolidationSchema.KB_ParentID, parentBookingRow.GetValue(DtbBookingSchema.PK));
					SetValue(consolidationRow, DtbBookingConsolidationSchema.KB_ParentTableCode, DtbBookingSchema.Constants.Prefix);
					logger.LogLinkCreated(factory, ParentBooking.GetUniversalDataContextManager(), consolidation.GetUniversalDataContextManager());
				}
			}
		}

		void PopulateRelatedEntities(DtbConsignmentConsolidation consolidation)
		{
			PopulateAddresses(consolidation);
			PopulateConsignments(consolidation);
		}

		#region PopulateConsignments

		void PopulateConsignments(DtbConsignmentConsolidation consolidation)
		{
			var consignmentDataObject = GetConsignmentDataObject();

			if (Consignment != null)
			{
				var consignmentReader = new ConsignmentDataObjectReader(this, GetColumnIndexerFromRow(consolidation), dataObject, consignmentDataObject, Consignment);
				consignmentReader.ReadIntoCollection();
			}
			else
			{
				var deliveryInstructions = GetInstructions(consignmentDataObject, InstructionTypes.Codes.Delivery);
				var pickupInstructions = GetInstructions(consignmentDataObject, InstructionTypes.Codes.PickUp);
				IEnumerable<Instruction> instructionsToUse = Enumerable.Empty<Instruction>();
				Instruction singleInstruction = null;
				if (deliveryInstructions.Count() <= 1 && pickupInstructions.Count() > 1)
				{
					instructionsToUse = pickupInstructions;
					singleInstruction = deliveryInstructions.FirstOrDefault();
				}
				else if (pickupInstructions.Count() == 1)
				{
					instructionsToUse = deliveryInstructions;
					singleInstruction = pickupInstructions.First();
				}

				var consignmentCollectionReader = new ConsignmentDataObjectCollectionReaderFromInstructions(this, GetColumnIndexerFromRow(consolidation), dataObject, consignmentDataObject, instructionsToUse.ToArray(), singleInstruction);
				consignmentCollectionReader.ReadIntoCollection();
			}
		}

		UniversalShipment GetConsignmentDataObject()
		{
			return SchemaVersionManager.Current == UniversalXmlSchema.Version_2012_11_DO_NOT_USE
				? dataObject
				: UniversalShipment.GetSourceDataObject(dataObject);
		}

		IEnumerable<Instruction> GetInstructions(UniversalShipment consignmentDataObject, string instructionType)
		{
			return !IsPickUpConfirmed(consignmentDataObject) && consignmentDataObject.InstructionCollection != null
				? consignmentDataObject.InstructionCollection.Where(i => i.Type.GetCodeAsUpperCase() == instructionType)
				: Enumerable.Empty<Instruction>();
		}

		#region BaseConsignmentDataObjectReader

		abstract class ConsignmentDataObjectCollectionReader<T> : DataObjectCollectionReader<T, DtbBookingConsignment>
			where T : IDataObject
		{
			public ConsignmentDataObjectCollectionReader(DtbConsignmentConsolidationDataObjectReader reader, IColumnIndexer consolidation, UniversalShipment topLevelDataObject, UniversalShipment consignmentDataObject, T[] dataObjects)
				: base(dataObjects)
			{
				Reader = reader;
				Consolidation = consolidation;
				ConsignmentDataObject = consignmentDataObject;
				TopLevelDataObject = topLevelDataObject;
			}

			protected readonly DtbConsignmentConsolidationDataObjectReader Reader;
			protected readonly IColumnIndexer Consolidation;
			protected readonly UniversalShipment TopLevelDataObject;
			protected readonly UniversalShipment ConsignmentDataObject;

			#region Matching

			protected override DtbBookingConsignment[] BusinessObjects
			{
				get { return ExistingConsignments; }
			}

			protected override DtbBookingConsignment FindMatchingBusinessObject(T dataObject)
			{
				return null;
			}

			#region ExistingConsignments

			protected DtbBookingConsignment[] ExistingConsignments
			{
				get
				{
					if (existingConsignments == null)
					{
						if (!Reader.IsNewBO)
						{
							var consignmentRow = Reader.GetExistingConsignments(Consolidation).Single();
							existingConsignments = new[] { Reader.factory.Load<DtbBookingConsignment>(consignmentRow.GetValue(DtbBookingSchema.PK)) };
						}
						else
						{
							existingConsignments = Array.Empty<DtbBookingConsignment>();
						}
					}

					return existingConsignments;
				}
			}

			protected DtbBookingConsignment GetNextConsignment()
			{
				return ExistingConsignmentsQueue.Count > 0 ? ExistingConsignmentsQueue.Dequeue() : null;
			}

			Queue<DtbBookingConsignment> ExistingConsignmentsQueue
			{
				get { return existingConsignmentsQueue ?? (existingConsignmentsQueue = new Queue<DtbBookingConsignment>(ExistingConsignments)); }
			}

			DtbBookingConsignment[] existingConsignments;
			Queue<DtbBookingConsignment> existingConsignmentsQueue;

			#endregion

			#endregion

			#region Add / Remove Consignment

			protected override void AddToCollection(DtbBookingConsignment consignment)
			{
			}

			protected override void RemoveFromCollection(DtbBookingConsignment consignment)
			{
				// remove from relationship
				var row = GetColumnIndexerFromRow(consignment);
				Reader.SetValue(row, DtbBookingSchema.KM_KB_Booking, Guid.Empty);
			}

			#endregion

			#region Populate

			protected override void ReadIntoCollectionCore()
			{
				base.ReadIntoCollectionCore();

				if (!DataObjects.Any())
				{
					AddToCollection(ReadIntoBusinessObject(default(T), null));
				}
			}

			#endregion
		}

		#endregion

		#region ConsignmentDataObjectReader

		class ConsignmentDataObjectReader : ConsignmentDataObjectCollectionReader<UniversalShipment>
		{
			public ConsignmentDataObjectReader(DtbConsignmentConsolidationDataObjectReader reader, IColumnIndexer consolidation, UniversalShipment topLevelDataObject, UniversalShipment consignmentDataObject, DtbBookingConsignment consignment)
				: base(reader, consolidation, topLevelDataObject, consignmentDataObject, new[] { consignmentDataObject })
			{
				Consignment = consignment;
			}

			readonly DtbBookingConsignment Consignment;

			protected override DtbBookingConsignment ReadIntoBusinessObject(UniversalShipment shipment, DtbBookingConsignment consignment)
			{
				return new DtbBookingConsignmentDataObjectReader(ConsignmentDataObject, Reader.logger, Reader.factory, Consolidation, TopLevelDataObject, Consignment).ReadIntoBusinessObject();
			}
		}

		#endregion

		#region ConsignmentDataObjectCollectionReaderFromInstructions

		class ConsignmentDataObjectCollectionReaderFromInstructions : ConsignmentDataObjectCollectionReader<Instruction>
		{
			public ConsignmentDataObjectCollectionReaderFromInstructions(DtbConsignmentConsolidationDataObjectReader reader, IColumnIndexer consolidation, UniversalShipment topLevelDataObject, UniversalShipment consignmentDataObject, Instruction[] multiPickupOrDeliveryInstructions, Instruction singlePickupOrDeliveryInstruction)
				: base(reader, consolidation, topLevelDataObject, consignmentDataObject, multiPickupOrDeliveryInstructions)
			{
				SinglePickupOrDeliveryInstruction = singlePickupOrDeliveryInstruction;
			}

			readonly Instruction SinglePickupOrDeliveryInstruction;

			#region Populate

			#region ReadIntoBusinessObject

			protected override DtbBookingConsignment ReadIntoBusinessObject(Instruction instruction, DtbBookingConsignment consignment)
			{
				var existingConsignment = GetNextConsignment();

				// we need to copy the pickup and depot allocation information from the existing Pick Up Consignment
				// to all the new Consignments.
				var pickUpInstructionToCopy = existingConsignment == null ? PickUpInstructionToCopy : null;
				var depotConfirmationToCopy = existingConsignment == null ? DepotConfirmationToCopy : null;

				return new DtbBookingConsignmentDataObjectReader(
					ConsignmentDataObject, Reader.logger, Reader.factory, Consolidation, TopLevelDataObject, instruction,
					existingConsignment, pickUpInstructionToCopy, depotConfirmationToCopy, SinglePickupOrDeliveryInstruction).ReadIntoBusinessObject();
			}

			IColumnIndexer DepotConfirmationToCopy
			{
				get { return (depotConfirmationToCopyCache ?? (depotConfirmationToCopyCache = new CachedValue<IColumnIndexer>(GetDepotConfirmation))).Value; }
			}

			IColumnIndexer GetDepotConfirmation()
			{
				IColumnIndexer result = null;

				if (!Reader.IsNewBO)
				{
					var consignmentRow = GetColumnIndexerFromRow(ExistingConsignments.Single());
					var existingDepotInstruction = Reader.factory.GetInstruction(consignmentRow, InstructionTypes.Codes.Multi);

					// We want the Deliver to depot confirmation
					result = Reader.factory.GetConfirmation(existingDepotInstruction, ConfirmationTypes.Codes.Delivery);
				}

				return result;
			}

			IColumnIndexer PickUpInstructionToCopy
			{
				get { return (pickUpInstructionToCopyCache ?? (pickUpInstructionToCopyCache = new CachedValue<IColumnIndexer>(GetPickUpInstruction))).Value; }
			}

			IColumnIndexer GetPickUpInstruction()
			{
				IColumnIndexer result = null;

				if (!Reader.IsNewBO)
				{
					var consignmentRow = GetColumnIndexerFromRow(ExistingConsignments.Single());
					result = Reader.factory.GetInstruction(consignmentRow, InstructionTypes.Codes.PickUp);
				}

				return result;
			}

			CachedValue<IColumnIndexer> depotConfirmationToCopyCache;
			CachedValue<IColumnIndexer> pickUpInstructionToCopyCache;

			#endregion

			#region ReadIntoCollectionCore

			protected override void ReadIntoCollectionCore()
			{
				base.ReadIntoCollectionCore();

				// It may be the case that we pickup eg. 1 pallet and deliver its inners, eg. 2 boxes.
				// In such cases the Consignment Outers will be changed from 1 pallet to 2 boxes.
				// The unassigned packages (eg. the pallet) should then be deleted.
				DeleteDetachedPackageHelper.DeleteDetachedPackages(Reader.factory);
			}

			#endregion

			#endregion
		}

		#endregion

		#endregion

		#endregion
	}
}


