using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.DataTransfer.Universal.Common;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportConsignment.DataTransfer.Universal
{
	internal class LTConsignmentConsolidationDataObjectReader : ShipmentDataObjectReader<LTConsignmentConsolidation>
	{
		internal LTConsignmentConsolidationDataObjectReader(UniversalShipment consolidationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(consolidationDataObject, logger, factory)
		{
		}

		#region Context

		public override DataContextType DataContextType
		{
			get { return DataContextType.LandTransportConsignmentConsol; }
		}

		#endregion

		#region Matching Job

		protected override LTConsignmentConsolidation GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			return new LTConsignmentConsolidation();
		}

		protected override IMatchingBusinessEntityFinder<LTConsignmentConsolidation> GetCombinedReferenceMatcher()
		{
			return null;
		}

		#endregion

		#region GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(LTConsignmentConsolidation targetBO)
		{
			ZString reason = ZString.Empty;

			if (ParentBooking != null) // we only need the following validation when updating a Pick Up Consignment for a Booking
			{
				var existingConsignment = GetExistingConsignments();
				if (existingConsignment != null && existingConsignment.Any())
				{
					var consignmentDataObject = GetConsignmentDataObject();
					reason = new LandTransportConsignmentImportValidationHelper().ReasonForNotAbleToUpdateFromDataSourceOrTargetBO(consignmentDataObject, factory, GetExistingConsignments());
				}
			}

			return reason.IsEmpty ? base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(targetBO) : reason;
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

		#region Create / Update

		protected override void PopulateBusinessObject(LTConsignmentConsolidation targetBO)
		{
			PopulateRelatedEntities(targetBO);
		}

		void PopulateRelatedEntities(LTConsignmentConsolidation targetBO)
		{
			PopulateConsignments(targetBO);
		}

		void PopulateLTConsolidation(LTConsignmentConsolidation targetBO)
		{
#if DEBUG
			targetBO.ConsignmentsCreatedDuringImport_ForTesting = consignmentsForTest;
#endif
		}

		#region AddConsignmentsForTest

		void AddConsignmentsForTest(DtbConsignment consignment)
		{
#if DEBUG
			if (consignmentsForTest == null)
			{
				consignmentsForTest = new List<DtbConsignment>();
			}

			consignmentsForTest.Add(consignment);
#endif
		}

#if DEBUG
		List<DtbConsignment> consignmentsForTest;
#endif

		#endregion

		#region PopulateConsignments

		void PopulateConsignments(LTConsignmentConsolidation targetBO)
		{
			var consignmentDataObject = GetConsignmentDataObject();

			var deliveryInstructions = GetInstructions(consignmentDataObject, InstructionTypes.Codes.Delivery).ToArray();
			var pickupInstructions = GetInstructions(consignmentDataObject, InstructionTypes.Codes.PickUp).ToArray();

			CheckForPackagesAndContainers();

			var instructionsToUse = Array.Empty<Instruction>();
			Instruction singleInstruction = null;
			if (deliveryInstructions.Length <= 1 && pickupInstructions.Length > 1)
			{
				instructionsToUse = pickupInstructions;
				singleInstruction = deliveryInstructions.FirstOrDefault();
			}
			else if (pickupInstructions.Length == 1)
			{
				instructionsToUse = deliveryInstructions;
				singleInstruction = pickupInstructions.First();
			}

			var consignmentCollectionReader = new LTConsignmentDataObjectCollectionReaderFromInstructions(this, dataObject, consignmentDataObject, instructionsToUse, singleInstruction);
			consignmentCollectionReader.ReadIntoCollection();

			PopulateLTConsolidation(targetBO);
		}

		void CheckForPackagesAndContainers()
		{
			if (
				(dataObject.PackingLineCollection == null || !dataObject.PackingLineCollection.Any()) &&
				(dataObject.ContainerCollection == null || !dataObject.ContainerCollection.Any())
				)
			{
				throw new DataObjectReadFailureException(Res.GetString("7616A2BC-C49B-4183-BCAC-78AE0288EC1F", "Packages are not assigned to any instructions"));
			}
		}

		IColumnIndexer[] GetExistingConsignments()
		{
			IColumnIndexer[] result = Array.Empty<IColumnIndexer>();
			if (ParentBooking != null)
			{
				var consignmentQuery = new ZDBOnlyQuery(typeof(DtbConsignment));
				var additionalReferencesQuery = new ZDBOnlySubQuery(typeof(ICusEntryNumber), CusEntryNumSchema.CE_ParentID);
				additionalReferencesQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, TransportCommonAdditionalReferenceTypes.Codes.BookingJobId);
				additionalReferencesQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, ((IDtbBooking)ParentBooking).KM_JobID);

				consignmentQuery.AddSubQuery(additionalReferencesQuery, JoinCondition.And);

				var existingConsignments = factory.RowFactory.Load(DtbConsignmentSchema.Constants.TableName, consignmentQuery);
				result = Array.ConvertAll(existingConsignments, GetColumnIndexerFromRow);
			}

			return result;
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

		bool IsPickUpConfirmed(UniversalShipment consignmentDataObject)
		{
			return consignmentDataObject.ShipmentStatus.GetCodeAsUpperCase() == TransportStatuses.Codes.PickUpConfirmed;
		}

		#region LTConsignmentDataObjectCollectionReaderFromInstructions

		class LTConsignmentDataObjectCollectionReaderFromInstructions : DataObjectCollectionReader<Instruction, DtbConsignment>
		{
			public LTConsignmentDataObjectCollectionReaderFromInstructions(LTConsignmentConsolidationDataObjectReader reader, UniversalShipment topLevelDataObject, UniversalShipment consignmentDataObject, Instruction[] multiPickupOrDeliveryInstructions, Instruction singlePickupOrDeliveryInstruction)
				: base(multiPickupOrDeliveryInstructions)
			{
				Reader = reader;
				TopLevelDataObject = topLevelDataObject;
				ConsignmentDataObject = consignmentDataObject;
				SinglePickupOrDeliveryInstruction = singlePickupOrDeliveryInstruction;
			}

			readonly Instruction SinglePickupOrDeliveryInstruction;
			readonly UniversalShipment ConsignmentDataObject;
			readonly UniversalShipment TopLevelDataObject;
			readonly LTConsignmentConsolidationDataObjectReader Reader;

			#region Matching

			protected override DtbConsignment[] BusinessObjects
			{
				get { return ExistingConsignments; }
			}

			protected override DtbConsignment FindMatchingBusinessObject(Instruction dataObject)
			{
				return null;
			}

			#region ExistingConsignments

			protected DtbConsignment[] ExistingConsignments
			{
				get
				{
					if (existingConsignments == null)
					{
						var consignmentRows = Reader.GetExistingConsignments();
						if (consignmentRows.Any())
						{
							var query = new ZQuery(DtbConsignmentSchema.PK, consignmentRows.Select(c => c.GetValue(DtbConsignmentSchema.PK)));
							existingConsignments = Reader.factory.Load<DtbConsignment>(query);
						}
						else
						{
							existingConsignments = Array.Empty<DtbConsignment>();
						}
					}

					return existingConsignments;
				}
			}

			protected DtbConsignment GetNextConsignment()
			{
				return ExistingConsignmentsQueue.Count > 0 ? ExistingConsignmentsQueue.Dequeue() : null;
			}

			Queue<DtbConsignment> ExistingConsignmentsQueue
			{
				get { return existingConsignmentsQueue ?? (existingConsignmentsQueue = new Queue<DtbConsignment>(ExistingConsignments)); }
			}

			DtbConsignment[] existingConsignments;
			Queue<DtbConsignment> existingConsignmentsQueue;

			#endregion

			#endregion

			#region Add / Remove Consignment

			protected override void AddToCollection(DtbConsignment consignment)
			{
				Reader.AddConsignmentsForTest(consignment);
			}

			protected override void RemoveFromCollection(DtbConsignment consignment)
			{
			}

			#endregion

			#region Populate

			#region ReadIntoBusinessObject

			protected override DtbConsignment ReadIntoBusinessObject(Instruction instruction, DtbConsignment consignment)
			{
				var existingConsignment = GetNextConsignment();

				// we need to copy the pickup and depot allocation information from the existing Pick Up Consignment
				// to all the new Consignments.
				var pickUpAddressToCopy = existingConsignment == null ? PickUpAddressToCopy : null;
				var depotActionToCopy = existingConsignment == null ? DepotActionToCopy : null;

				return new DtbConsignmentDataObjectReader(ConsignmentDataObject, Reader.logger, Reader.factory, TopLevelDataObject, instruction, existingConsignment, pickUpAddressToCopy, depotActionToCopy, SinglePickupOrDeliveryInstruction).ReadIntoBusinessObject();
			}

			IColumnIndexer DepotActionToCopy
			{
				get { return (depotActionToCopyCache ?? (depotActionToCopyCache = new CachedValue<IColumnIndexer>(GetDepotAction))).Value; }
			}

			IColumnIndexer GetDepotAction()
			{
				IColumnIndexer result = null;

				if (ExistingConsignments.Any())
				{
					var consignmentRow = GetColumnIndexerFromRow(ExistingConsignments.Single());
					var existingDepotAddress = GetConsignmentAddress(consignmentRow, ConsignmentAddressTypes.Codes.Multi);

					// We want the Deliver to depot action
					result = GetConsignmentAction(existingDepotAddress, ActionTypeList.Codes.Deliver);
				}

				return result;
			}

			IColumnIndexer PickUpAddressToCopy
			{
				get { return (pickUpAddressToCopyCache ?? (pickUpAddressToCopyCache = new CachedValue<IColumnIndexer>(GetPickUpAddress))).Value; }
			}

			IColumnIndexer GetPickUpAddress()
			{
				IColumnIndexer result = null;

				if (Reader.GetExistingConsignments().Any())
				{
					var consignmentRow = GetColumnIndexerFromRow(ExistingConsignments.Single());
					result = GetConsignmentAddress(consignmentRow, ConsignmentAddressTypes.Codes.PickUp);
				}

				return result;
			}

			IColumnIndexer GetConsignmentAddress(IColumnIndexer consignmentRow, ZString addressType)
			{
				var query = new ZQuery();
				query.AddToFilter(DtbConsignmentAddressSchema.LTS_LTC_Consignment, consignmentRow.GetValue(DtbConsignmentSchema.PK));
				query.AddToFilter(DtbConsignmentAddressSchema.LTS_InstructionType, addressType);

				return GetColumnIndexerFromRow(Reader.factory.RowFactory.Load(DtbConsignmentAddressSchema.Constants.TableName, query).Single()); // only one Address of each type should exist.
			}

			IColumnIndexer GetConsignmentAction(IColumnIndexer existingDepotAddress, ZString actionType)
			{
				var query = new ZQuery();
				query.AddToFilter(DtbConsignmentActionSchema.LTA_LTS_ConsignmentAddress, existingDepotAddress.GetValue(DtbConsignmentAddressSchema.PK));
				query.AddToFilter(DtbConsignmentActionSchema.LTA_ActionType, actionType);

				return GetColumnIndexerFromRow(Reader.factory.RowFactory.Load(DtbConsignmentActionSchema.Constants.TableName, query).Single()); // only one Action of each type should exist.
			}

			CachedValue<IColumnIndexer> depotActionToCopyCache;
			CachedValue<IColumnIndexer> pickUpAddressToCopyCache;

			#endregion

			protected override void ReadIntoCollectionCore()
			{
				base.ReadIntoCollectionCore();

				DeleteDetachedPackageHelper.DeleteDetachedPackages(Reader.factory);

				if (!DataObjects.Any())
				{
					AddToCollection(ReadIntoBusinessObject(default(Instruction), null));
				}
			}

			#endregion
		}

		#endregion

		#endregion

		#endregion
	}
}



