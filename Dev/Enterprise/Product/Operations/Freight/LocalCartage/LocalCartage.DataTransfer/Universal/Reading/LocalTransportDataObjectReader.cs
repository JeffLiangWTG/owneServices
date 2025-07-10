using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Integration;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.LocalCartage.DataTransfer.Universal
{
	using Constants = Core.Constants;

	public class LocalTransportDataObjectReader : ShipmentDataObjectReader<CommonCartage>
	{
		public LocalTransportDataObjectReader(UniversalShipment topLevelDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, UniversalShipment sourceDO = null)
			: base(topLevelDataObject, logger, factory)
		{
			this.sourceDO = sourceDO;
		}

		UniversalShipment TopLevelDO
		{
			get { return topLevelDO ?? (topLevelDO = GetTopLevelDataObject()); }
		}
		UniversalShipment topLevelDO;

		UniversalShipment GetTopLevelDataObject()
		{
			return SchemaVersionManager.Current == UniversalXmlSchema.Version_2012_11_DO_NOT_USE
				? GetTopLevelDataObjectFromNewNameSpace(dataObject)
				: dataObject;
		}

		UniversalShipment GetTopLevelDataObjectFromNewNameSpace(UniversalShipment dataObject)
		{
			UniversalShipment result = dataObject;

			if (dataObject.ParentShipmentCollection != null && dataObject.ParentShipmentCollection.Count == 1)
			{
				result = GetTopLevelDataObjectFromNewNameSpace(dataObject.ParentShipmentCollection[0]);
			}

			return result;
		}

		UniversalShipment SourceDO
		{
			get { return sourceDO ?? (sourceDO = UniversalShipment.GetSourceDataObject(dataObject)); }
		}
		UniversalShipment sourceDO;

		protected override CommonCartage GetNewBusinessObject()
		{
			var cartage = base.GetNewBusinessObject();

			// clear Set Default Values
			cartage.JJ_E3_NKJobType = "";
			cartage.JJ_ShippingTransportMode = "";
			cartage.JJ_Direction = "";
			cartage.JJ_ContainerMode = "";

			return cartage;
		}

		protected override CommonCartage GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			return null; // There are apparently no hard and fast reference rules define here. Good target for implementing Reference and Party ID matching.
		}

		protected override IMatchingBusinessEntityFinder<CommonCartage> GetCombinedReferenceMatcher()
		{
			var client = GetClient(SourceDO, TopLevelDO);
			if (client != null)
			{
				var cartageReferences = new CartageReferences();
				var orgReader = new OrganisationDataObjectReader(client, logger, factory);
				var clientAddress = orgReader.GetMatched();
				if (clientAddress != null)
				{
					cartageReferences.ClientOrganization = clientAddress.OA_OH;
				}

				cartageReferences.ClientOrderNumber = GetClientReferenceNumber(SourceDO).GetValueOrDefault();
				cartageReferences.WaybillNumber = SourceDO.WayBillNumber.GetValueOrDefault();
				cartageReferences.QuoteNumber = SourceDO.QuoteNumber.GetValueOrDefault();
				cartageReferences.References = GetValidEventReferences(); // could include Transport Ref

				return new CartageMatcher(factory.BOFactory, cartageReferences, logger);
			}

			return null;
		}

		List<KeyValuePair<ZString, ZString>> GetValidEventReferences()
		{
			var result = new List<KeyValuePair<ZString, ZString>>();

			var additionalReferences = SourceDO.AdditionalReferenceCollection;
			if (additionalReferences != null)
			{
				foreach (var additionalReferenceItem in additionalReferences)
				{
					var referenceNumber = additionalReferenceItem.ReferenceNumber.GetValueOrDefault();
					var referenceType = additionalReferenceItem.Type.GetCodeAsUpperCase();

					if (!referenceNumber.IsEmpty && ReferenceTypes.ContainsCode(referenceType))
					{
						result.Add(new KeyValuePair<ZString, ZString>(referenceType, referenceNumber));
					}
				}
			}

			return result;
		}

		protected override void PopulateBusinessObject(CommonCartage targetBO)
		{
			// transport booking consolidation - TopLevelDO
			// ╚═ transport booking -SourceDO

			var iSupportDataImporting = (ISupportDataImporting)targetBO;

			try
			{
				iSupportDataImporting.IsImportingData = true;

				PopulateModes(targetBO, SourceDO);
				PopulateJobDetails(targetBO, SourceDO, TopLevelDO); // booking party stored on the consolidation
				PopulateContainers(targetBO, TopLevelDO); // packages stored on the consolidation
				PopulatePackages(targetBO, TopLevelDO); // packages stored on the consolidation
				InferContainerMode(targetBO);           // populating package/container counts requires a correct container mode to be set before PopulateGoodsAndTotals is called
				PopulateReferences(targetBO, SourceDO, TopLevelDO);
				PopulateSchedule(targetBO, SourceDO, TopLevelDO);
				PopulateInstructions(targetBO, SourceDO);
				PopulateNotes(targetBO, SourceDO);
				PopulateDates(targetBO, SourceDO);
				PopulateGoodsAndTotals(targetBO, SourceDO);
				PopulateAddresses(targetBO, TopLevelDO); // booking party stored on the consolidation
				PopulateCustomizedFields(targetBO, SourceDO);
				LinkParent(targetBO);
				CleanLooseBookedMovesWithoutLegs(targetBO);
				SplitContainersForContainerisedJobIfCountainerCountIsGreaterThanOne(targetBO);
			}
			finally
			{
				iSupportDataImporting.IsImportingData = false;
			}
		}

		void LinkParent(CommonCartage targetBO)
		{
			if (logger.IsInternalImport())
			{
				var localTransportRow = GetColumnIndexerFromRow(targetBO);
				if (localTransportRow.GetValue(JobCartageSchema.JJ_ParentID).IsEmpty)
				{
					var bookingDataSource = logger.TopLevelDataContext.GetMatchingDataSource(DataContextType.TransportBooking);
					var parentBooking = bookingDataSource.GetLoadedJobFromDataContextType(logger.TopLevelDataObject, factory.BOFactory);
					if (parentBooking != null)
					{
						var parentBookingRow = GetColumnIndexerFromRow(parentBooking);
						SetValue(localTransportRow, JobCartageSchema.JJ_ParentID, parentBookingRow.GetValue(DtbBookingSchema.PK));
						SetValue(localTransportRow, JobCartageSchema.JJ_ParentTableCode, DtbBookingSchema.Constants.Prefix);
						logger.LogLinkCreated(factory, parentBooking.GetUniversalDataContextManager(), targetBO.GetUniversalDataContextManager());

						if (targetBO.Job != null)
						{
							targetBO.Job.JH_JH_ParentJob = ((IDtbBooking)parentBooking).JobHeaderPK;
						}
					}
				}
			}
		}

		void CleanLooseBookedMovesWithoutLegs(CommonCartage targetBO)
		{
			var bookedMoves = targetBO.LooseBookedMoves;
			var hasContainers = targetBO.Containers.Any();
			var hasLoosePackages = bookedMoves.Any();
			if (hasContainers && hasLoosePackages && bookedMoves.All(b => b.CartageLegs.Count == 0))
			{
				targetBO.LooseBookedMoves.DeleteAll();
				SetValue(targetBO, JobCartageSchema.JJ_ContainerMode, Constants.CartageContainerMode.Containerized);
			}
		}

		void SplitContainersForContainerisedJobIfCountainerCountIsGreaterThanOne(CommonCartage targetBO)
		{
			if (targetBO.JJ_ContainerMode == Constants.ContainerModes.Containerised)
			{
				var containersToSplit = targetBO.Containers.Where(c => c.JC_ContainerCount > 1 && c.JC_ContainerNum.IsEmpty).ToArray();
				if (containersToSplit.Length > 0)
				{
					var propertiesToExcludeFromCloning = new[] { JobContainerSchema.Constants.JC_ContainerCount, JobContainerSchema.Constants.JC_TareWeight };
					var cloneArgs = new BusinessObjectCloneArgs(propertiesToExcludeFromCloning);

					foreach (var containerToSplit in containersToSplit)
					{
						// Exclude Gross Weight when cloning, only the first Container should include the Goods Weight. Unfortunately to achieve
						// this, we have to remove the gross weight temporarily, as the CommonContainer.Clone() has no way to prevent cloning Goods Weight.
						decimal previousGrossWeight = containerToSplit.JC_GrossWeight;
						var containerRow = ((IBusinessObjectInternals)containerToSplit).Row;
						containerRow[JobContainerSchema.Constants.JC_GrossWeight] = (decimal)containerToSplit.JC_TareWeight;

						var numberOfContainersToCreate = containerToSplit.JC_ContainerCount - 1;
						var containerLink = LinksDictionary.TryGetValue(containerToSplit.PK, out var result) ? result : (int?)null;
						var updatedOrCreated = new List<CommonContainer>(new[] { containerToSplit });

						for (int index = 0; index < numberOfContainersToCreate; index++)
						{
							var newContainer = (CommonContainer)containerToSplit.Clone(cloneArgs);
							CreateOrUpdateBookedMovesAndLink(targetBO, updatedOrCreated, containerLink, newContainer);
							if (SourceDO.InstructionCollection != null)
							{
								PopulateContainerInstruction(targetBO, SourceDO, newContainer);
							}
						}

						// reset gross weight after cloning container
						containerRow[JobContainerSchema.Constants.JC_GrossWeight] = previousGrossWeight;

						var tareWeight = containerToSplit.JC_TareWeight;
						containerToSplit.JC_ContainerCount = 1;

						foreach (var container in updatedOrCreated)
						{
							container.JC_TareWeight = tareWeight / updatedOrCreated.Count;
						}
					}
				}
			}
		}

		void PopulateJobDetails(CommonCartage targetBO, UniversalShipment dataObject, UniversalShipment topLevelDataObject)
		{
			var transportCompanyBranchPK = GetTransportCompanyControllingBranch(dataObject);
			if (transportCompanyBranchPK.IsValid)
			{
				SetValue(targetBO, JobCartageSchema.JJ_GB, transportCompanyBranchPK);
			}
			else
			{
				var branchCode = dataObject.Branch.GetCodeAsUpperCase();
				if (!branchCode.IsEmpty)
				{
					var branch = factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, branchCode));
					if (branch != null)
					{
						SetValue(targetBO, JobCartageSchema.JJ_GB, branch.PK);
					}
				}
			}

			var client = GetClient(dataObject, topLevelDataObject);
			if (client != null)
			{
				var job = new JobHeader.Loader(targetBO).TryLoadOrCreateWithMutex();
				if (job != null)
				{
					SetValue(job, JobHeaderSchema.JH_OA_LocalChargesAddr, client);
				}
			}
		}

		ZGuid GetTransportCompanyControllingBranch(UniversalShipment dataObject)
		{
			ZGuid result = ZGuid.Empty;

			if (dataObject.OrganizationAddressCollection != null)
			{
				var companyAddress = dataObject.OrganizationAddressCollection.FirstOrDefault(a => a.AddressType.Equals(nameof(DocAddressType.TransportCompanyDocumentaryAddress)));
				if (companyAddress != null)
				{
					var companyOrgAddress = new OrganisationDataObjectReader(companyAddress, logger, factory).GetMatched(false);
					if (companyOrgAddress != null)
					{
						var companyData = companyOrgAddress.Header.CompanyData;
						result = companyData != null ? companyData.OB_GB_ControllingBranch : ZGuid.Empty;
					}
				}
			}

			return result;
		}

		OrganizationAddress GetClient(UniversalShipment dataObject, UniversalShipment topLevelDataObject)
		{
			OrganizationAddress client = null;

			if (dataObject.OrganizationAddressCollection != null)
			{
				client = dataObject.OrganizationAddressCollection.FirstOrDefault(a => a.AddressType.Equals(nameof(DocAddressType.LocalClient)));
				if (client == null)
				{
					client = dataObject.OrganizationAddressCollection.FirstOrDefault(a => a.AddressType.Equals(AddressTypes.SendersLocalClient));
				}
			}
			else if (topLevelDataObject.OrganizationAddressCollection != null)
			{
				client = topLevelDataObject.OrganizationAddressCollection.FirstOrDefault(a => a.AddressType.Equals(nameof(DocAddressType.BookingPartyDocumentaryAddress)));
			}

			return client;
		}

		void PopulateModes(CommonCartage targetBO, UniversalShipment dataObject)
		{
			SetValue(targetBO, JobCartageSchema.JJ_ShippingTransportMode, dataObject.TransportMode);
			SetValue(targetBO, JobCartageSchema.JJ_Direction, dataObject.TransportBookingDirection);

			var jobType = dataObject.LocalTransportJobType.GetCodeAsUpperCase();
			if (targetBO.BindToLists.NewAllCartageJobTypes.ContainsCode(jobType))
			{
				SetValue(targetBO, JobCartageSchema.JJ_E3_NKJobType, dataObject.LocalTransportJobType);
			}

			SetValue(targetBO, JobCartageSchema.JJ_RS_NKServiceLevel, dataObject.ServiceLevel);

			PopulateDropMode(targetBO, dataObject);
		}

		void PopulateDropMode(CommonCartage targetBO, UniversalShipment dataObject)
		{
			if (dataObject.LocalTransportEquipmentNeeded != null)
			{
				SetValue(targetBO, JobCartageSchema.JJ_DropMode, dataObject.LocalTransportEquipmentNeeded);
			}

			if (targetBO.JJ_DropMode.IsEmpty && dataObject.LocalProcessing != null)
			{
				if (targetBO.IsExportOrOrigin)
				{
					SetValue(targetBO, JobCartageSchema.JJ_DropMode, dataObject.LocalProcessing.FCLPickupEquipmentNeeded);
				}
				else
				{
					SetValue(targetBO, JobCartageSchema.JJ_DropMode, dataObject.LocalProcessing.FCLDeliveryEquipmentNeeded);
				}
			}

			// fall back to first booked movement drop mode (after booked movements are populated)
		}

		void PopulateDropModeFromFirstBookedMovementIfRequired(CommonCartage targetBO)
		{
			if (targetBO.JJ_DropMode.IsEmpty)
			{
				var firstBookedMovement = targetBO.BookedMovesCollection.Where(bm => !bm.EW_DisplayOrder.IsEmpty).OrderBy(bm => bm.EW_DisplayOrder).FirstOrDefault();
				if (firstBookedMovement != null)
				{
					SetValue(targetBO, JobCartageSchema.JJ_DropMode, firstBookedMovement.EW_DropMode);
				}
			}
		}

		void InferContainerMode(CommonCartage targetBO)
		{
			var hasContainers = targetBO.Containers.Any();
			var hasLoosePackages = targetBO.LooseBookedMoves.Any();
			if (hasContainers && hasLoosePackages)
			{
				SetValue(targetBO, JobCartageSchema.JJ_ContainerMode, Constants.CartageContainerMode.Mixed);
			}
			else if (!hasContainers && !hasLoosePackages)
			{
				SetValue(targetBO, JobCartageSchema.JJ_ContainerMode, Constants.CartageContainerMode.Mixed);

				// special case ... if no packages and loose, add one loose movement so dates can be imported
				var move = targetBO.LooseBookedMoves.AddNew();
				move.EW_DisplayOrder = Convert.ToInt16(targetBO.BookedMovesCollection.Count);
				move.DefaultAddresses();
				LinksDictionary.Add(move.PK, LinkPackagesToAllInstructions_SpecialCase);
			}
			else if (hasContainers && !hasLoosePackages)
			{
				SetValue(targetBO, JobCartageSchema.JJ_ContainerMode, Constants.CartageContainerMode.Containerized);
			}
			else     // (!hasContainers && hasLoosePackages)
			{
				SetValue(targetBO, JobCartageSchema.JJ_ContainerMode, Constants.CartageContainerMode.Loose);
			}
		}

		void PopulateGoodsAndTotals(CommonCartage targetBO, UniversalShipment dataObject)
		{
			SetValue(targetBO, JobCartageSchema.JJ_GoodsDescription, dataObject.GoodsDescription);

			SetValue(targetBO, JobCartageSchema.JJ_OuterPacks, dataObject.OuterPacks);
			SetValue(targetBO, JobCartageSchema.JJ_F3_NKPackType, dataObject.OuterPacksPackageType);

			SetValue(targetBO, JobCartageSchema.JJ_Volume, dataObject.TotalVolume);
			SetValue(targetBO, JobCartageSchema.JJ_VolumeUQ, dataObject.TotalVolumeUnit);

			SetValue(targetBO, JobCartageSchema.JJ_Weight, dataObject.TotalWeight);
			SetValue(targetBO, JobCartageSchema.JJ_WeightUQ, dataObject.TotalWeightUnit);

			if (targetBO.JJ_OuterPacks.IsEmpty && targetBO.JJ_Volume.IsEmpty && targetBO.JJ_Weight.IsEmpty)
			{
				targetBO.UpdateCartageFromLooseBookedMovesOrContainers();
			}
		}

		void PopulateContainers(CommonCartage targetBO, UniversalShipment dataObject)
		{
			if (dataObject.ContainerCollection != null)
			{
				var updatedOrCreated = new List<CommonContainer>();
				var findContainer = new Func<Container, CommonContainer>((containerDO) =>
				{
					var containerNumber = containerDO.ContainerNumber.GetValueOrDefault();
					return !containerNumber.IsEmpty ? targetBO.Containers.FirstOrDefault(c => c.JC_ContainerNum == containerNumber && !updatedOrCreated.Contains(c)) : null;
				});

				foreach (var containerDataObject in dataObject.ContainerCollection)
				{
					var container = new ContainerDataObjectReader<CommonContainer>(containerDataObject, logger, factory, findContainer).ReadIntoBusinessObject();
					CreateOrUpdateBookedMovesAndLink(targetBO, updatedOrCreated, containerDataObject.Link, container);
				}

				foreach (var container in targetBO.Containers.ToArray())
				{
					if (!updatedOrCreated.Contains(container))
					{
						var move = targetBO.ContainerBookedMoves.FirstOrDefault(m => m.Container == container);
						if (move != null)
						{
							move.Delete();
						}
					}
				}
			}
		}

		void CreateOrUpdateBookedMovesAndLink(CommonCartage targetBO, List<CommonContainer> updatedOrCreated, ZInt? link, CommonContainer container)
		{
			updatedOrCreated.Add(container);
			if (!targetBO.Containers.Contains(container))
			{
				var move = targetBO.Factory.New<CommonBookedCtgMove>();
				move.EW_JC_Container = container.PK;
				targetBO.ContainerBookedMoves.Add(move);
			}
			else
			{
				var existingMove = targetBO.GetBookedMoves(container);
				existingMove.Single().CartageLegs.DeleteAll();
			}

			if (link.HasValue)
			{
				LinksDictionary.Add(container.PK, link.Value);
			}
		}

		void PopulatePackages(CommonCartage targetBO, UniversalShipment dataObject)
		{
			if (dataObject.PackingLineCollection != null)
			{
				targetBO.LooseBookedMoves.DeleteAll();
				foreach (var packageDataObject in dataObject.PackingLineCollection)
				{
					new BookedMovePackageDataObjectReader(packageDataObject, logger, factory, targetBO, LinksDictionary).ReadIntoBusinessObject();
				}
			}
		}

		readonly ZInt LinkPackagesToAllInstructions_SpecialCase = -99;

		void PopulateReferences(CommonCartage targetBO, UniversalShipment sourceDataObject, UniversalShipment topLevelDataObject)
		{
			AdditionalReference orderRefNumber = null;
			if (sourceDataObject.AdditionalReferenceCollection != null)
			{
				orderRefNumber = sourceDataObject.AdditionalReferenceCollection.FirstOrDefault(r => r.Type.Code.ToString() == TransportAdditionalReferenceTypes.Codes.ExternalTransportBookingNumber);
			}

			if (orderRefNumber != null)
			{
				SetValue(targetBO, JobCartageSchema.JJ_OrderReferenceNumber, orderRefNumber.ReferenceNumber);
			}
			else
			{
				SetValue(targetBO, JobCartageSchema.JJ_OrderReferenceNumber, GetClientReferenceNumber(sourceDataObject));
			}

			SetValue(targetBO, JobCartageSchema.JJ_QuoteNumber, sourceDataObject.QuoteNumber);
			SetValue(targetBO, JobCartageSchema.JJ_WaybillNumber, sourceDataObject.WayBillNumber ?? topLevelDataObject.WayBillNumber);

			if (sourceDataObject.AdditionalReferenceCollection != null)
			{
				var additionalReferenceCollectionReader = new LocalTransportAdditionalReferenceCollectionReader(
					MergeSourceAndTargetDOAdditionalReferences(sourceDataObject.AdditionalReferenceCollection, topLevelDataObject.AdditionalReferenceCollection),
					logger,
					factory,
					targetBO);
				additionalReferenceCollectionReader.ReadIntoCollection();
			}

			var entryNum = GetClientReferenceNumber(sourceDataObject).GetValueOrDefault("");
			if (!targetBO.AdditionalReferenceNumbers.Find(rn => rn.CE_EntryType == TransportAdditionalReferenceTypes.Codes.TransportReference && rn.CE_EntryNum == entryNum).Any())
			{
				var result = targetBO.AdditionalReferenceNumbers.AddNew();
				result.CE_EntryType = TransportAdditionalReferenceTypes.Codes.TransportReference;
				result.CE_EntryNum = entryNum;
			}
		}

		DataObjectList<AdditionalReference> MergeSourceAndTargetDOAdditionalReferences(
			DataObjectList<AdditionalReference> sourceAdditionalReferences,
			DataObjectList<AdditionalReference> topLevelAdditionalReferences)
		{
			DataObjectList<AdditionalReference> result = sourceAdditionalReferences ?? topLevelAdditionalReferences;

			if (sourceAdditionalReferences != null && topLevelAdditionalReferences != null)
			{
				foreach (var additionalReference in topLevelAdditionalReferences.Where(a => a.Type.Code.HasValue && a.ReferenceNumber.HasValue))
				{
					if (!result.Any(a => a.Type.Code.Value.EqualsIgnoringCase(additionalReference.Type.Code) && a.ReferenceNumber.Value.EqualsIgnoringCase(additionalReference.ReferenceNumber)))
					{
						result.Add(additionalReference);
					}
				}
			}
			return result;
		}

		ZString? GetClientReferenceNumber(UniversalShipment dataObject)
		{
			ZString? result = null;

			var dataSourceForDataObject = dataObject.GetMatchingDataSource(DataContextType.TransportBooking);
			if (dataSourceForDataObject != null)
			{
				result = dataSourceForDataObject.Key;
			}

			if (!result.HasValue && dataObject.Order != null)
			{
				result = dataObject.Order.OrderNumber;
			}

			return result;
		}

		void PopulateSchedule(CommonCartage targetBO, UniversalShipment sourceDataObject, UniversalShipment topLevelDOShipment)
		{
			var populatedScheduleFromSource = TryPopulateSchedule(targetBO, sourceDataObject.TransportLegCollection, topLevelDOShipment);
			if (!populatedScheduleFromSource)
			{
				populatedScheduleFromSource = TryPopulateSchedule(targetBO, topLevelDOShipment.TransportLegCollection, topLevelDOShipment);
			}

			if (!populatedScheduleFromSource)
			{
				ImportScheduleFromParent(targetBO, topLevelDOShipment);
			}
		}

		/// <summary>
		/// Find Relevant (NON TB) SubShipments
		/// Find Source for that SubShipment (ie a Consol may have a SubShipment Forwarding Shipment) - but look in appropriate
		/// place depending whether using format UXML 2011 or UXML 2012
		/// If the NON TB Source doesn't have routings, look at the NON TB Top Level Routings (ie Consol) 
		/// </summary>
		void ImportScheduleFromParent(CommonCartage targetBO, UniversalShipment topLevelDOShipment)
		{
			UniversalShipment relevantTopLevelDO = null;
			List<UniversalShipment> collectionToExamine = FindCollectionToBeginSearchForRelevantShipment();
			if (collectionToExamine != null)
			{
				relevantTopLevelDO = FindRelevantShipmentWithLegs(collectionToExamine);
			}

			UniversalShipment relevantSourceDO = null;
			if (relevantTopLevelDO != null)
			{
				relevantSourceDO = UniversalShipment.GetSourceDataObject(relevantTopLevelDO);
			}

			if (relevantSourceDO != null)
			{
				var populatedScheduleFromNonTBSource = TryPopulateSchedule(targetBO, relevantSourceDO.TransportLegCollection, topLevelDOShipment);
				if (!populatedScheduleFromNonTBSource)
				{
					TryPopulateSchedule(targetBO, relevantTopLevelDO.TransportLegCollection, topLevelDOShipment);
				}
			}

			List<UniversalShipment> FindCollectionToBeginSearchForRelevantShipment()
			{
				if (SchemaVersionManager.Current == UniversalXmlSchema.Version_2012_11_DO_NOT_USE)
				{
					var direction = GetTransportBookingConsolidationDirectionCode(topLevelDOShipment);
					if (direction.HasValue)
					{
						if (direction.Value == "DLV")
						{
							return topLevelDOShipment.PreCarriageShipmentCollection;
						}
						else if (direction.Value == "PIC")
						{
							return topLevelDOShipment.PostCarriageShipmentCollection;
						}
					}
				}
				else
				{
					return topLevelDOShipment.SubShipmentCollection?.ToList();
				}

				return null;
			}

			UniversalShipment FindRelevantShipmentWithLegs(List<UniversalShipment> collection)
			{
				UniversalShipment result = null;
				foreach (UniversalShipment shipment in collection)
				{
					result = CheckShipmentIsRelevantAndHasLegsOrSearchParent(shipment);
					if (result != null)
					{
						break;
					}
				}
				return result;
			}

			UniversalShipment CheckShipmentIsRelevantAndHasLegsOrSearchParent(UniversalShipment shipment)
			{
				UniversalShipment result = null;

				if (shipment.DataContext.GetDataSourceKey() != sourceDO.DataContext.GetDataSourceKey())
				{
					if (shipment.TransportLegCollection != null)
					{
						result = shipment;
					}
					else if (shipment.ParentShipmentCollection != null)
					{
						result = FindRelevantShipmentWithLegs(shipment.ParentShipmentCollection);
					}
				}

				return result;
			}
		}

		ZString? GetTransportBookingConsolidationDirectionCode(UniversalShipment topLevelDOShipment)
		{
			return topLevelDOShipment?.TransportBookingDirection?.Code;
		}

		ZBool TryPopulateSchedule(CommonCartage targetBO, DataObjectList<TransportLeg> transportLegs, UniversalShipment topLevelShipmentDO)
		{
			TransportLeg scheduleLeg = null;

			if (transportLegs != null)
			{
				var lookForLastLeg = (GetTransportBookingConsolidationDirectionCode(topLevelShipmentDO) == (ZString?)"DLV");
				var orderedLegs = lookForLastLeg ? transportLegs.OrderByDescending(l => l.LegOrder) : transportLegs.OrderBy(l => l.LegOrder);
				scheduleLeg = orderedLegs
					.FirstOrDefault(l =>
						!l.LegType.Equals(UniversalDataBuss.DataObjects.Universal.LegType.LocalTransport));

				if (scheduleLeg != null)
				{
					if (nameof(TransportMode.Air).Equals(topLevelShipmentDO.TransportMode?.Code, System.StringComparison.InvariantCultureIgnoreCase))
					{
						if (scheduleLeg.EstimatedArrival.HasValue && scheduleLeg.EstimatedArrival.Value.IsValid && scheduleLeg.EstimatedArrival.Value.AddDays(1) < scheduleLeg.EstimatedDeparture)
						{
							logger.Log(LogType.Warning, Res.GetString("25AC56C9-6509-4544-B042-6E5E68D4FB88", "ETA cannot be more than a day before ETD. Transport Leg information not updated."));
							return false;
						}
					}
					else
					{
						if (scheduleLeg.EstimatedArrival < scheduleLeg.EstimatedDeparture)
						{
							logger.Log(LogType.Warning, Res.GetString("C97A1448-87C0-45F2-98E5-B60F6BDD9E9B", "ETA cannot be before ETD. Transport Leg information not updated."));
							return false;
						}
					}

					var vesselName = scheduleLeg.GetVesselName(factory.BOFactory);
					if (!vesselName.IsEmpty)
					{
						targetBO.Vessel = vesselName;
					}

					if (scheduleLeg.VoyageFlightNo.HasValue)
					{
						targetBO.VoyageFlight = scheduleLeg.VoyageFlightNo.Value;
					}

					if (scheduleLeg.PortOfLoading != null)
					{
						targetBO.PortOfLoading = scheduleLeg.PortOfLoading.GetUNLOCOAsUpperCase(factory.BOFactory);
					}

					if (scheduleLeg.PortOfDischarge != null)
					{
						targetBO.PortOfDischarge = scheduleLeg.PortOfDischarge.GetUNLOCOAsUpperCase(factory.BOFactory);
					}

					if (scheduleLeg.EstimatedDeparture.HasValue)
					{
						targetBO.E_DEP = scheduleLeg.EstimatedDeparture.Value;
					}

					if (scheduleLeg.EstimatedArrival.HasValue)
					{
						targetBO.E_ARV = scheduleLeg.EstimatedArrival.Value;
					}

					if (scheduleLeg.FCLReceivalCommences.HasValue)
					{
						targetBO.FCLReceivalCommences = scheduleLeg.FCLReceivalCommences.Value;
					}

					if (scheduleLeg.FCLCutOff.HasValue)
					{
						targetBO.FCLCutOff = scheduleLeg.FCLCutOff.Value;
					}

					if (scheduleLeg.LCLReceivalCommences.HasValue)
					{
						targetBO.LCLReceivalCommences = scheduleLeg.LCLReceivalCommences.Value;
					}

					if (scheduleLeg.LCLCutOff.HasValue)
					{
						targetBO.LCLCutOff = scheduleLeg.LCLCutOff.Value;
					}

					if (scheduleLeg.FCLAvailability.HasValue)
					{
						targetBO.FCLAvailabilityDate = scheduleLeg.FCLAvailability.Value;
					}

					if (scheduleLeg.FCLStorage.HasValue)
					{
						targetBO.FCLStorageDate = scheduleLeg.FCLStorage.Value;
					}

					if (scheduleLeg.LCLAvailability.HasValue)
					{
						var shipmentWithOveriddenLCLAvailabilityDate = topLevelShipmentDO.SubShipmentCollection?.FirstOrDefault(s => s.LocalProcessing != null && s.LocalProcessing.LCLAvailable != null && s.LocalProcessing.LCLAvailable != ZDateTime.Empty);
						if (shipmentWithOveriddenLCLAvailabilityDate != null)
						{
							targetBO.LCLAvailabilityDate = (ZDateTime)shipmentWithOveriddenLCLAvailabilityDate.LocalProcessing.LCLAvailable;
						}
						else
						{
							targetBO.LCLAvailabilityDate = scheduleLeg.LCLAvailability.Value;
						}
					}

					if (scheduleLeg.LCLStorageDate.HasValue)
					{
						var shipmentWithOveriddenLCLStorageDate = topLevelShipmentDO.SubShipmentCollection?.FirstOrDefault(s => s.LocalProcessing != null && s.LocalProcessing.LCLStorageCommences != null && s.LocalProcessing.LCLStorageCommences != ZDateTime.Empty);

						if (shipmentWithOveriddenLCLStorageDate != null)
						{
							targetBO.LCLStorageDate = (ZDateTime)shipmentWithOveriddenLCLStorageDate.LocalProcessing.LCLStorageCommences;
						}
						else
						{
							targetBO.LCLStorageDate = scheduleLeg.LCLStorageDate.Value;
						}
					}
				}
			}

			return scheduleLeg != null;
		}

		void PopulateInstructions(CommonCartage targetBO, UniversalShipment dataObject)
		{
			var instructions = dataObject.InstructionCollection;
			if (instructions != null)
			{
				BuildLocalTransportLegLookups(instructions, dataObject.TransportLegCollection);

				PopulateContainerInstructions(targetBO, dataObject);
				PopulateLooseInstructions(targetBO, dataObject);
				PopulateDropModeFromFirstBookedMovementIfRequired(targetBO);
			}
		}

		void PopulateContainerInstructions(CommonCartage targetBO, UniversalShipment dataObject)
		{
			foreach (var container in targetBO.Containers)
			{
				PopulateContainerInstruction(targetBO, dataObject, container);
			}
		}

		void PopulateContainerInstruction(CommonCartage targetBO, UniversalShipment dataObject, CommonContainer container)
		{
			if (LinksDictionary.TryGetValue(container.PK, out var containerLink))
			{
				var move = targetBO.GetBookedMoves(container)[0];
				move.EW_DisplayOrder = Convert.ToInt16(targetBO.BookedMovesCollection.Count);
				move.DefaultAddresses();

				if (HasLocalTransportLegs(dataObject.TransportLegCollection))
				{
					PopulateLegsUsingInstructionsAndLegs(move, TransportLegsByContainerLink, containerLink);
				}
				else
				{
					PopulateContainerLegsUsingInstructionsOnly(move, containerLink, dataObject);
				}
			}
		}

		void PopulateContainerLegsUsingInstructionsOnly(CommonBookedCtgMove move, ZInt containerLink, UniversalShipment dataObject)
		{
			if (dataObject.InstructionCollection != null)
			{
				var orderedContainerInstructions =
				from i in dataObject.InstructionCollection
				where i.InstructionContainerLinkCollection != null
				from l in i.InstructionContainerLinkCollection
				where l.ContainerLink == containerLink
				orderby i.Sequence
				select i;

				CommonCartageLeg lastLeg = null;

				var instructionCount = orderedContainerInstructions.Count();
				foreach (var instruction in orderedContainerInstructions)
				{
					var isPickup = instruction.Type.GetCodeAsUpperCase().Equals(InstructionTypes.Codes.PickUp);
					var isMulti = instruction.Type.GetCodeAsUpperCase().Equals(InstructionTypes.Codes.Multi);
					var isDelivery = instruction.Type.GetCodeAsUpperCase().Equals(InstructionTypes.Codes.Delivery);
					var isWaitPoint = instruction.DropMode.GetCodeAsUpperCase().Equals(Constants.FCLEquipmentNeeded.WaitForUnpack);

					if (lastLeg == null || isPickup)
					{
						lastLeg = CreateCartageLeg(move);
					}

					if (isPickup)
					{
						PopulateLegUsingInstructionsOnly(lastLeg, instruction, AddressPoint.Pickup, instructionCount);
					}
					else if (isDelivery)
					{
						PopulateLegUsingInstructionsOnly(lastLeg, instruction, AddressPoint.Delivery, instructionCount);
					}
					else if (isMulti)
					{
						if (isWaitPoint)
						{
							PopulateLegUsingInstructionsOnly(lastLeg, instruction, AddressPoint.WaitPointArrival, instructionCount);
						}
						else
						{
							PopulateLegUsingInstructionsOnly(lastLeg, instruction, AddressPoint.Delivery, instructionCount);

							lastLeg = CreateCartageLeg(move);
							PopulateLegUsingInstructionsOnly(lastLeg, instruction, AddressPoint.Pickup, instructionCount);
						}
					}
				}
			}
		}

		void PopulateLooseInstructions(CommonCartage targetBO, UniversalShipment dataObject)
		{
			foreach (var package in targetBO.LooseBookedMoves)
			{
				ZInt link;
				if (LinksDictionary.TryGetValue(package.PK, out link))
				{
					if (HasLocalTransportLegs(dataObject.TransportLegCollection))
					{
						PopulateLegsUsingInstructionsAndLegs(package, TransportLegsByPackingLineLink, link);
					}
					else
					{
						PopulateLooseLegsUsingInstructionsOnly(dataObject, package, link);
					}
				}
			}
		}

		void PopulateLooseLegsUsingInstructionsOnly(UniversalShipment dataObject, CommonBookedCtgMove package, ZInt looseLink)
		{
			var orderedLooseInstructions =
				from i in dataObject.InstructionCollection
				where i.InstructionPackingLineLinkCollection != null && GetMatchingPackageLink(i, looseLink) != null
				orderby i.Sequence
				select i;

			CommonCartageLeg lastLeg = null;
			AddressPoint lastPoint = AddressPoint.None;

			var instructionCount = orderedLooseInstructions.Count();
			foreach (var instructionDataObject in orderedLooseInstructions)
			{
				var isPickup = instructionDataObject.Type.GetCodeAsUpperCase().Equals(InstructionTypes.Codes.PickUp);
				var isMulti = instructionDataObject.Type.GetCodeAsUpperCase().Equals(InstructionTypes.Codes.Multi);
				var isDelivery = instructionDataObject.Type.GetCodeAsUpperCase().Equals(InstructionTypes.Codes.Delivery);

				if (lastLeg == null || isPickup)
				{
					lastLeg = CreateCartageAndUpdateBookedCtgMove(instructionDataObject, package, looseLink);
				}

				// Currently does not handle mutli 

				if (isPickup)
				{
					if (lastPoint != AddressPoint.None)
					{
						lastLeg = CreateCartageAndUpdateBookedCtgMove(instructionDataObject, package, looseLink);
						// should set qty ? or split bkd move?
					}

					PopulateLegUsingInstructionsOnly(lastLeg, instructionDataObject, AddressPoint.Pickup, instructionCount);
					lastPoint = AddressPoint.Pickup;
				}
				else if (isDelivery)
				{
					if (lastPoint == AddressPoint.Delivery)
					{
						lastLeg = CreateCartageLeg(package);
						// should set qty ? or split bkd move?
					}

					PopulateLegUsingInstructionsOnly(lastLeg, instructionDataObject, AddressPoint.Delivery, instructionCount);
					lastPoint = AddressPoint.Delivery;
				}
				else if (isMulti)
				{
					if (lastPoint == AddressPoint.None)
					{
						PopulateLegUsingInstructionsOnly(lastLeg, instructionDataObject, AddressPoint.Pickup, instructionCount);
						lastPoint = AddressPoint.Pickup;
					}
					else // if (lastPoint == AddressPoint.Pickup)
					{
						PopulateLegUsingInstructionsOnly(lastLeg, instructionDataObject, AddressPoint.Delivery, instructionCount);
						lastPoint = AddressPoint.Delivery;
					}
				}
			}
		}

		/// <summary>
		/// The movement the leg belongs to has already been populated from the PackingLine Collection (Packs etc)
		/// But the instructions (leg) may only be for part of the PackingLine, so reduce the Pack/Weight/Volume using the InstructionPackageLink.Quantity
		/// </summary>
		CommonCartageLeg CreateCartageAndUpdateBookedCtgMove(Instruction instructionDataObject, CommonBookedCtgMove movement, ZInt looseLink)
		{
			var result = CreateCartageLeg(movement);
			var movementQty = movement.EW_BookedPackCount;
			if (movementQty != 0) // nothing to move
			{
				var packageLink = GetMatchingPackageLink(instructionDataObject, looseLink);
				if (packageLink != null && packageLink.Quantity.HasValue) // only attempt to update movement packs if Quantity is specified
				{
					var packageQty = packageLink.Quantity.Value;
					if (movementQty != packageQty)
					{
						movement.EW_BookedPackCount = packageQty;

						var fraction = (ZDecimal)packageQty / (ZDecimal)movementQty;
						movement.EW_BookedVolume = fraction * movement.EW_BookedVolume;
						movement.EW_BookedWeight = fraction * movement.EW_BookedWeight;
					}
				}
			}

			return result;
		}

		InstructionPackingLineLink GetMatchingPackageLink(Instruction instructionDO, ZInt looseLink)
		{
			return instructionDO.InstructionPackingLineLinkCollection.FirstOrDefault(
					l =>
						l.PackingLineLink == looseLink ||
						(!l.PackingLineLink.HasValue && looseLink == LinkPackagesToAllInstructions_SpecialCase));
		}

		void PopulateLegsUsingInstructionsAndLegs(CommonBookedCtgMove move, Dictionary<ZInt, List<TransportLeg>> transportLegsByPackageLink, ZInt link)
		{
			List<TransportLeg> legs;
			if (transportLegsByPackageLink.TryGetValue(link, out legs))
			{
				foreach (var transportLeg in legs)
				{
					List<Confirmation> confirmations;
					if (ConfirmationsByLeg.TryGetValue(transportLeg, out confirmations))
					{
						var legBO = CreateCartageLeg(move);
						PopulateLegUsingInstructionsAndLegs(legBO, confirmations);

						new LocalTransportLegDataObjectReader(transportLeg, logger, factory, legBO).ReadIntoBusinessObject(); // only for Custom Fields
					}
				}
			}
		}

		void PopulateLegUsingInstructionsOnly(CommonCartageLeg targetLeg, Instruction instruction, AddressPoint addressPoint, int instructionsCount)
		{
			PopulateAddress(targetLeg, instruction, addressPoint, instructionsCount);
			PopulateFromConfirmations(targetLeg, instruction, addressPoint);
			PopulateDropMode(targetLeg, instruction, addressPoint);

			if (instruction.ServiceInstruction.HasValue && !targetLeg.JU_LegNotes.Contains(instruction.ServiceInstruction.Value, StringComparison.CurrentCulture))
			{
				SetValue(targetLeg, JobContainerLegsSchema.JU_LegNotes, instruction.ServiceInstruction);
			}
		}

		void PopulateLegUsingInstructionsAndLegs(CommonCartageLeg targetLeg, List<Confirmation> confirmations)
		{
			Confirmation picConfirm = null;
			Confirmation waitDlvConfirm = null;
			Confirmation waitPicConfirm = null;
			Confirmation dlvConfirm = null;

			foreach (var confirmation in confirmations)
			{
				var isPickup = confirmation.DateDescription.Equals(ConfirmationTypes.Codes.PickUp);
				var isDelivery = confirmation.DateDescription.Equals(ConfirmationTypes.Codes.Delivery);
				var noPic = picConfirm == null;
				var noWaitDlv = waitDlvConfirm == null;
				var noWaitPic = waitPicConfirm == null;
				var noDlv = dlvConfirm == null;

				if (isPickup)
				{
					if (noPic && noWaitDlv && noWaitPic && noDlv)
					{
						picConfirm = confirmation;
					}
					else if (noWaitPic && noDlv)
					{
						waitPicConfirm = confirmation;
					}
				}
				else if (isDelivery)
				{
					if (noWaitDlv && noWaitPic && noDlv)
					{
						waitDlvConfirm = confirmation;
					}
					else if (noDlv)
					{
						dlvConfirm = confirmation;
					}
				}
			}

			if (waitDlvConfirm != null && waitPicConfirm == null && dlvConfirm == null)
			{
				dlvConfirm = waitDlvConfirm;
				waitDlvConfirm = null;
			}

			if (picConfirm != null)
			{
				PopulateAddress(targetLeg, InstructionByConfirmation[picConfirm], AddressPoint.Pickup, confirmations.Count);
				PopulateFromPickup(targetLeg, new List<Confirmation>() { picConfirm });
				PopulateDropMode(targetLeg, InstructionByConfirmation[picConfirm], AddressPoint.Pickup);
			}

			if (waitDlvConfirm != null || waitPicConfirm != null)
			{
				var waitConfirms = new List<Confirmation>();
				if (waitDlvConfirm != null)
				{
					waitConfirms.Add(waitDlvConfirm);
					PopulateAddress(targetLeg, InstructionByConfirmation[waitDlvConfirm], AddressPoint.WaitPointArrival, confirmations.Count);
					PopulateDropMode(targetLeg, InstructionByConfirmation[waitDlvConfirm], AddressPoint.WaitPointArrival);
				}
				else
				{
					waitConfirms.Add(waitPicConfirm);
					PopulateAddress(targetLeg, InstructionByConfirmation[waitPicConfirm], AddressPoint.WaitPointDeparture, confirmations.Count);
					PopulateDropMode(targetLeg, InstructionByConfirmation[waitPicConfirm], AddressPoint.WaitPointDeparture);
				}

				PopulateFromWaitPoint(targetLeg, waitConfirms);
			}

			if (dlvConfirm != null)
			{
				PopulateAddress(targetLeg, InstructionByConfirmation[dlvConfirm], AddressPoint.Delivery, confirmations.Count);
				PopulateFromDelivery(targetLeg, new List<Confirmation>() { dlvConfirm });
				PopulateDropMode(targetLeg, InstructionByConfirmation[dlvConfirm], AddressPoint.Delivery);
			}
		}

		void PopulateAddress(CommonCartageLeg targetLeg, Instruction instruction, AddressPoint addressPoint, int instructionsCount)
		{
			var address = instruction.Address;
			if (address != null)
			{
				var orgReader = new OrganisationDataObjectReader(instruction.Address, logger, factory);
				var docType = OrganisationDataObjectReader.GetDocAddressType(instruction.Address.AddressType.Value);
				var organisationType = docType.GetOrganisationType();
				var organisationSubType = docType.GetOrganisationSubType();
				var orgAddress = orgReader.GetMatched(targetLeg.Cartage, organisationType, organisationSubType);

				var docAddresses = targetLeg.Cartage.DocAddresses.Cast<JobDocAddress>();
				var docAddress = docAddresses.FirstOrDefault(jobDocAddress =>
																jobDocAddress.DocAddressType == docType &&
																(jobDocAddress.IsEmpty || orgReader.IsJobDocAddressMatchingOrgAddress(orgAddress, jobDocAddress)));
				if (docAddress == null)
				{
					docAddress = targetLeg.Cartage.DocAddresses.AddNew(docType);
					docAddress.MakePersistentEvenIfEmpty(); // We want Job Doc Address to be saved even if it was empty 
					orgReader.PopulateJobDocAddress(orgAddress, docAddress);
					if (docAddress.IsOverridenButEmpty)
					{
						docAddress.E2_AddressOverride = false;
					}
				}
				else
				{
					orgReader.PopulateJobDocAddress(orgAddress, docAddress);
				}

				PopulateAddress(targetLeg, addressPoint, instructionsCount, docAddress);
			}
		}

		void PopulateAddress(CommonCartageLeg targetLeg, AddressPoint addressPoint, int instructionsCount, JobDocAddress docAddress)
		{
			if (docAddress != null)
			{
				var move = targetLeg.BookedCtgMove;
				var hasNoMovePickup = move.PickupFromDocAddress == null || (move.PickupFromDocAddress.IsEmpty && move.PickupFromDocAddress.DocAddressType == DocAddressType.None);
				var hasNoMoveDelivery = move.WaitPointDocAddress == null || (move.WaitPointDocAddress.IsEmpty && move.WaitPointDocAddress.DocAddressType == DocAddressType.None);
				var isCYD = docAddress.DocAddressType == DocAddressType.LocalCartageYard;
				var defaultOnMove = !isCYD || instructionsCount <= 2; // Only default on the move when the CYD is the only movement on the job. It's included free if part of FULL job.
				var isPickupPoint = addressPoint == AddressPoint.Pickup;
				var isWaitPoint = addressPoint == AddressPoint.WaitPointArrival || addressPoint == AddressPoint.WaitPointDeparture;
				var isDeliveryPoint = addressPoint == AddressPoint.Delivery;

				// setup leg
				if (addressPoint == AddressPoint.Pickup)
				{
					SetValue(targetLeg, JobContainerLegsSchema.JU_E2PickupAddressID, docAddress.PK);
				}
				else if (addressPoint == AddressPoint.WaitPointArrival || addressPoint == AddressPoint.WaitPointDeparture)
				{
					SetValue(targetLeg, JobContainerLegsSchema.JU_E2WaitPointAddressID, docAddress.PK);
				}
				else if (addressPoint == AddressPoint.Delivery)
				{
					SetValue(targetLeg, JobContainerLegsSchema.JU_E2DeliveryAddressID, docAddress.PK);
				}

				// setup move
				if (defaultOnMove)
				{
					if (hasNoMovePickup && (isPickupPoint || isWaitPoint))
					{
						SetValue(move, JobBookedCtgMoveSchema.EW_E2PickupAddressID, docAddress.PK);
					}
					else if (!hasNoMovePickup && (hasNoMoveDelivery && (isWaitPoint || isDeliveryPoint) && move.EW_E2PickupAddressID != docAddress.PK))
					{
						SetValue(move, JobBookedCtgMoveSchema.EW_E2WaitPointAddressID, docAddress.PK);
					}
				}
			}
		}

		void PopulateFromConfirmations(CommonCartageLeg targetLeg, Instruction instruction, AddressPoint addressPoint)
		{
			List<Confirmation> confirmationCollection = null;

			if (targetLeg.IsContainerised)
			{
				var link = LinksDictionary[targetLeg.Container.PK];
				var containerLink = instruction.InstructionContainerLinkCollection.FirstOrDefault(l => l.ContainerLink == link);
				if (containerLink != null)
				{
					confirmationCollection = containerLink.ConfirmationCollection;
				}
			}
			else
			{
				var link = LinksDictionary[targetLeg.JU_EW];
				var packageLink = instruction.InstructionPackingLineLinkCollection.FirstOrDefault(l => l.PackingLineLink == link || (!l.PackingLineLink.HasValue && link == LinkPackagesToAllInstructions_SpecialCase));
				if (packageLink != null)
				{
					confirmationCollection = packageLink.ConfirmationCollection;
				}
			}

			if (confirmationCollection != null)
			{
				switch (addressPoint)
				{
					case AddressPoint.Pickup:
						PopulateFromPickup(targetLeg, confirmationCollection);
						break;
					case AddressPoint.WaitPointArrival:
						PopulateFromWaitPoint(targetLeg, confirmationCollection);
						break;
					case AddressPoint.Delivery:
						PopulateFromDelivery(targetLeg, confirmationCollection);
						break;
				}
			}
		}

		void PopulateFromPickup(CommonCartageLeg targetLeg, List<Confirmation> confirmationCollection)
		{
			var picConfirmation = confirmationCollection.FirstOrDefault(c => c.DateDescription.Equals(ConfirmationTypes.Codes.PickUp));
			if (picConfirmation != null)
			{
				SetValue(targetLeg, JobContainerLegsSchema.JU_PlannedPickupTime, picConfirmation.EstimatedDate);
				SetValue(targetLeg, JobContainerLegsSchema.JU_PlannedPickupTimeEnd, picConfirmation.EstimatedOutDate);
				SetValue(targetLeg, JobContainerLegsSchema.JU_PickupTimeIn, picConfirmation.ActualDate);
				SetValue(targetLeg, JobContainerLegsSchema.JU_PickupTimeOut, picConfirmation.ActualOutDate);
				SetValue(targetLeg, JobContainerLegsSchema.JU_CartagePickupDemurrage, picConfirmation.Demurrage);
				SetValue(targetLeg, JobContainerLegsSchema.JU_IsEmptyContainer, picConfirmation.IsEmptyContainer);

				if (targetLeg.IsRequestedAddress(targetLeg.PickupFromDocAddress))
				{
					SetValue(targetLeg.BookedCtgMove, JobBookedCtgMoveSchema.EW_RequestedPickupTimeStart, picConfirmation.RequiredFromDate);
					SetValue(targetLeg.BookedCtgMove, JobBookedCtgMoveSchema.EW_RequestedPickupTimeEnd, picConfirmation.RequiredToDate);
					SetValue(targetLeg.BookedCtgMove, JobBookedCtgMoveSchema.EW_Distance, picConfirmation.Distance);
					SetValue(targetLeg.BookedCtgMove, JobBookedCtgMoveSchema.EW_DistanceUnit, picConfirmation.DistanceUnit);
				}

				if (picConfirmation.ServiceInstruction.HasValue && !targetLeg.JU_LegNotes.Contains(picConfirmation.ServiceInstruction.Value, StringComparison.CurrentCulture))
				{
					SetValue(targetLeg, JobContainerLegsSchema.JU_LegNotes, picConfirmation.ServiceInstruction);
				}
			}

			var slotConfirmation = confirmationCollection.FirstOrDefault(c => c.DateDescription.Equals(ConfirmationTypes.Codes.PickUp));
			if (slotConfirmation != null && targetLeg.PickupDocAddressType == DocAddressType.LocalCartageCTO)
			{
				var container = targetLeg.Container;
				if (container != null)
				{
					SetValue(container, JobContainerSchema.JC_ArrivalSlotDateTime, slotConfirmation.SlotDate);
					SetValue(container, JobContainerSchema.JC_ArrivalSlotReference, slotConfirmation.SlotReference);
				}
			}
		}

		void PopulateFromWaitPoint(CommonCartageLeg targetLeg, List<Confirmation> confirmationCollection)
		{
			var dlvConfirmation = confirmationCollection.FirstOrDefault(c => c.DateDescription.Equals(ConfirmationTypes.Codes.Delivery));
			var picConfirmation = confirmationCollection.FirstOrDefault(c => c.DateDescription.Equals(ConfirmationTypes.Codes.PickUp));
			var anyConfirmation = dlvConfirmation ?? picConfirmation;

			if (anyConfirmation != null)
			{
				SetValue(targetLeg, JobContainerLegsSchema.JU_EstimatedDeliveryTime, anyConfirmation.EstimatedDate);
				SetValue(targetLeg, JobContainerLegsSchema.JU_EstimatedDeliveryTimeEnd, anyConfirmation.EstimatedOutDate);
				SetValue(targetLeg, JobContainerLegsSchema.JU_WaitPointTimeIn, anyConfirmation.ActualDate);
				SetValue(targetLeg, JobContainerLegsSchema.JU_WaitPointTimeOut, anyConfirmation.ActualOutDate);
				SetValue(targetLeg, JobContainerLegsSchema.JU_DeliverySignedFor, anyConfirmation.ReceivedBy);
				SetValue(targetLeg, JobContainerLegsSchema.JU_Distance, anyConfirmation.Distance);
				SetValue(targetLeg, JobContainerLegsSchema.JU_DistanceUnit, anyConfirmation.DistanceUnit);
				SetValue(targetLeg, JobContainerLegsSchema.JU_CartageWaitPointDemurrage, anyConfirmation.Demurrage);
				SetValue(targetLeg, JobContainerLegsSchema.JU_IsEmptyContainer, anyConfirmation.IsEmptyContainer);

				if (targetLeg.IsRequestedAddress(targetLeg.WaitPointDocAddress))
				{
					SetValue(targetLeg.BookedCtgMove, JobBookedCtgMoveSchema.EW_Distance, anyConfirmation.Distance);
					SetValue(targetLeg.BookedCtgMove, JobBookedCtgMoveSchema.EW_DistanceUnit, anyConfirmation.DistanceUnit);

					if (dlvConfirmation != null)
					{
						SetValue(targetLeg.BookedCtgMove, JobBookedCtgMoveSchema.EW_RequestedDeliveryTimeStart, dlvConfirmation.RequiredFromDate);
						SetValue(targetLeg.BookedCtgMove, JobBookedCtgMoveSchema.EW_RequestedDeliveryTimeEnd, dlvConfirmation.RequiredToDate);
					}

					if (picConfirmation != null)
					{
						SetValue(targetLeg.BookedCtgMove, JobBookedCtgMoveSchema.EW_RequestedPickupTimeStart, picConfirmation.RequiredFromDate);
						SetValue(targetLeg.BookedCtgMove, JobBookedCtgMoveSchema.EW_RequestedPickupTimeEnd, picConfirmation.RequiredToDate);
					}
				}

				if (anyConfirmation.ServiceInstruction.HasValue && !targetLeg.JU_LegNotes.Contains(anyConfirmation.ServiceInstruction.Value, StringComparison.CurrentCulture))
				{
					SetValue(targetLeg, JobContainerLegsSchema.JU_LegNotes, anyConfirmation.ServiceInstruction);
				}
			}
		}

		void PopulateFromDelivery(CommonCartageLeg targetLeg, List<Confirmation> confirmationCollection)
		{
			var dlvConfirmation = confirmationCollection.FirstOrDefault(c => c.DateDescription.Equals(ConfirmationTypes.Codes.Delivery));
			if (dlvConfirmation != null)
			{
				SetValue(targetLeg, JobContainerLegsSchema.JU_EstimatedDeliveryTime, dlvConfirmation.EstimatedDate);
				SetValue(targetLeg, JobContainerLegsSchema.JU_EstimatedDeliveryTimeEnd, dlvConfirmation.EstimatedOutDate);
				SetValue(targetLeg, JobContainerLegsSchema.JU_DeliverTimeIn, dlvConfirmation.ActualDate);
				SetValue(targetLeg, JobContainerLegsSchema.JU_DeliverTimeOut, dlvConfirmation.ActualOutDate);
				SetValue(targetLeg, JobContainerLegsSchema.JU_DeliverySignedFor, dlvConfirmation.ReceivedBy);
				SetValue(targetLeg, JobContainerLegsSchema.JU_Distance, dlvConfirmation.Distance);
				SetValue(targetLeg, JobContainerLegsSchema.JU_DistanceUnit, dlvConfirmation.DistanceUnit);
				SetValue(targetLeg, JobContainerLegsSchema.JU_CartageDeliveryDemurrage, dlvConfirmation.Demurrage);
				SetValue(targetLeg, JobContainerLegsSchema.JU_IsEmptyContainer, dlvConfirmation.IsEmptyContainer);

				if (targetLeg.IsRequestedAddress(targetLeg.DeliverToDocAddress))
				{
					SetValue(targetLeg.BookedCtgMove, JobBookedCtgMoveSchema.EW_RequestedDeliveryTimeStart, dlvConfirmation.RequiredFromDate);
					SetValue(targetLeg.BookedCtgMove, JobBookedCtgMoveSchema.EW_RequestedDeliveryTimeEnd, dlvConfirmation.RequiredToDate);
					SetValue(targetLeg.BookedCtgMove, JobBookedCtgMoveSchema.EW_Distance, dlvConfirmation.Distance);
					SetValue(targetLeg.BookedCtgMove, JobBookedCtgMoveSchema.EW_DistanceUnit, dlvConfirmation.DistanceUnit);
				}

				if (dlvConfirmation.ServiceInstruction.HasValue && !targetLeg.JU_LegNotes.Contains(dlvConfirmation.ServiceInstruction.Value, StringComparison.CurrentCulture))
				{
					SetValue(targetLeg, JobContainerLegsSchema.JU_LegNotes, dlvConfirmation.ServiceInstruction);
				}
			}

			var slotConfirmation = confirmationCollection.FirstOrDefault(c => c.DateDescription.Equals(ConfirmationTypes.Codes.Delivery));
			if (slotConfirmation != null && targetLeg.DeliverToDocAddressType == DocAddressType.LocalCartageCTO)
			{
				var container = targetLeg.Container;
				if (container != null)
				{
					SetValue(container, JobContainerSchema.JC_DepartureSlotDateTime, slotConfirmation.SlotDate);
					SetValue(container, JobContainerSchema.JC_DepartureSlotReference, slotConfirmation.SlotReference);
				}
			}
		}

		void PopulateDropMode(CommonCartageLeg targetLeg, Instruction instruction, AddressPoint addressPoint)
		{
			var isRequestedInstruction = false;

			switch (addressPoint)
			{
				case AddressPoint.Pickup:
					isRequestedInstruction = targetLeg.IsRequestedAddress(targetLeg.PickupFromDocAddress);
					break;
				case AddressPoint.WaitPointArrival:
				case AddressPoint.WaitPointDeparture:
					isRequestedInstruction = targetLeg.IsRequestedAddress(targetLeg.WaitPointDocAddress);
					break;
				case AddressPoint.Delivery:
					isRequestedInstruction = targetLeg.IsRequestedAddress(targetLeg.DeliverToDocAddress);
					break;
			}

			if (isRequestedInstruction)
			{
				SetValue(targetLeg.BookedCtgMove, JobBookedCtgMoveSchema.EW_DropMode, instruction.DropMode);
			}
		}

		void PopulateNotes(CommonCartage cartageBO, UniversalShipment dataObject)
		{
			if (dataObject.NoteCollection != null)
			{
				new NotesCollectionReader(dataObject.NoteCollection, logger, factory, cartageBO).ReadIntoCollection();
			}
		}

		void PopulateDates(CommonCartage sourceBO, UniversalShipment dataObject)
		{
			if (dataObject.DateCollection != null)
			{
				PopulateDate(sourceBO, dataObject, DateType.LocalTransportPickup, true, JobCartageSchema.JJ_EstimatedPickup);
				PopulateDate(sourceBO, dataObject, DateType.LocalTransportDelivery, true, JobCartageSchema.JJ_EstimatedDelivery);
				PopulateDate(sourceBO, dataObject, DateType.LocalTransportCompleted, false, JobCartageSchema.JJ_A_JCL);
			}

			PopulateEstimateDatesOnCartage(sourceBO);
		}

		void PopulateDate(CommonCartage sourceBO, UniversalShipment dataObject, DateType dateType, bool isEstimate, SchemaDateTimeColumn column)
		{
			var date = dataObject.DateCollection.FirstOrDefault(d => d.Type.Equals(dateType) && d.IsEstimate.Equals(isEstimate));
			if (date != null)
			{
				SetValue(sourceBO, column, date.Value);
			}
		}

		void PopulateEstimateDatesOnCartage(CommonCartage targetBO)
		{
			var firstLeg = targetBO.CartageLegs.OrderBy(leg => leg.JU_DisplayOrder).FirstOrDefault();
			if (firstLeg != null)
			{
				var row = GetColumnIndexerFromRow(targetBO);
				if (row[JobCartageSchema.Constants.JJ_EstimatedPickup] == DBNull.Value)
				{
					SetValue(row, JobCartageSchema.JJ_EstimatedPickup, firstLeg.JU_PlannedPickupTime);
				}
				if (row[JobCartageSchema.Constants.JJ_EstimatedDelivery] == DBNull.Value)
				{
					SetValue(row, JobCartageSchema.JJ_EstimatedDelivery, firstLeg.JU_EstimatedDeliveryTime);
				}
			}
		}

		void PopulateAddresses(CommonCartage sourceBO, UniversalShipment dataObject)
		{
			if (dataObject.OrganizationAddressCollection != null)
			{
				foreach (var organisationAddressDataObject in dataObject.OrganizationAddressCollection)
				{
					new OrganisationDataObjectReader(organisationAddressDataObject, logger, factory).GetMatchedOrNew(sourceBO);
				}
			}
			if (dataObject.SubShipmentCollection?.Count == 1 && dataObject.SubShipmentCollection[0].ShipmentType == null)
			{
				var subShipment = dataObject.SubShipmentCollection[0];
				if (subShipment.OrganizationAddressCollection != null)
				{
					var address = subShipment.OrganizationAddressCollection.Find(oa => (string)oa.AddressType == nameof(DocAddressType.ClientRequestedBillingParty));
					if (address != null)
					{
						new OrganisationDataObjectReader(address, logger, factory).GetMatchedOrNew(sourceBO);
					}
				}
			}
		}

		void PopulateCustomizedFields(CommonCartage cartageBO, UniversalShipment dataObject)
		{
			if (dataObject.CustomizedFieldCollection != null && dataObject.CustomizedFieldCollection.Count > 0)
			{
				PopulateWorkflowCustomFields(cartageBO, dataObject);
			}
		}

		void BuildLocalTransportLegLookups(DataObjectList<Instruction> instructions, DataObjectList<TransportLeg> legs)
		{
			if (HasLocalTransportLegs(legs))
			{
				ConfirmationsByLeg = new Dictionary<TransportLeg, List<Confirmation>>();
				InstructionByConfirmation = new Dictionary<Confirmation, Instruction>();
				TransportLegsByContainerLink = new Dictionary<ZInt, List<TransportLeg>>();
				TransportLegsByPackingLineLink = new Dictionary<ZInt, List<TransportLeg>>();

				foreach (var instruction in instructions)
				{
					BuildContainerLegLookups(legs, instruction);
					BuildLooseLegLookups(legs, instruction);
				}
			}
		}

		void BuildContainerLegLookups(DataObjectList<TransportLeg> legs, Instruction instruction)
		{
			var containerLinks = instruction.InstructionContainerLinkCollection;
			if (containerLinks != null)
			{
				foreach (var containerLink in containerLinks)
				{
					BuildLegConfirmationLookups(legs, instruction, TransportLegsByContainerLink, containerLink.ContainerLink, containerLink.ConfirmationCollection);
				}
			}
		}

		void BuildLooseLegLookups(DataObjectList<TransportLeg> legs, Instruction instruction)
		{
			var looseLinks = instruction.InstructionPackingLineLinkCollection;
			if (looseLinks != null)
			{
				foreach (var looseLink in looseLinks)
				{
					BuildLegConfirmationLookups(legs, instruction, TransportLegsByPackingLineLink, looseLink.PackingLineLink, looseLink.ConfirmationCollection);
				}
			}
		}

		void BuildLegConfirmationLookups(DataObjectList<TransportLeg> legs, Instruction instruction, Dictionary<ZInt, List<TransportLeg>> transportLegsByPackageLinks, ZInt? packageOrContainerLink, List<Confirmation> confirmations)
		{
			if (confirmations != null)
			{
				foreach (var confirmation in confirmations)
				{
					var confirmationsLeg = legs.FirstOrDefault(l => l.Link == confirmation.LegLink);
					BuildLegConfirmationLookups(instruction, transportLegsByPackageLinks, packageOrContainerLink, confirmation, confirmationsLeg);
				}
			}
		}

		void BuildLegConfirmationLookups(Instruction instruction, Dictionary<ZInt, List<TransportLeg>> transportLegsByPackagesLinks, ZInt? packageOrContainerLink, Confirmation confirmation, TransportLeg leg)
		{
			if (leg != null)
			{
				BuildConfirmationsByLeg(confirmation, leg);
				BuildInstructionByConfirmation(instruction, confirmation);
				BuildTransportLegsByPackagesLinks(transportLegsByPackagesLinks, packageOrContainerLink, leg);
			}
		}

		void BuildConfirmationsByLeg(Confirmation confirmation, TransportLeg leg)
		{
			List<Confirmation> legConfirmations;

			if (!ConfirmationsByLeg.TryGetValue(leg, out legConfirmations))
			{
				legConfirmations = new List<Confirmation>();
				ConfirmationsByLeg.Add(leg, legConfirmations);
			}

			legConfirmations.Add(confirmation);
		}

		void BuildInstructionByConfirmation(Instruction instruction, Confirmation confirmation)
		{
			InstructionByConfirmation.Add(confirmation, instruction);
		}

		static void BuildTransportLegsByPackagesLinks(Dictionary<ZInt, List<TransportLeg>> transportLegsByPackagesLinks, ZInt? packageOrContainerLink, TransportLeg leg)
		{
			if (packageOrContainerLink.HasValue)
			{
				List<TransportLeg> packageLegs;
				if (!transportLegsByPackagesLinks.TryGetValue(packageOrContainerLink.Value, out packageLegs))
				{
					packageLegs = new List<TransportLeg>();
					transportLegsByPackagesLinks.Add(packageOrContainerLink.Value, packageLegs);
				}

				if (!packageLegs.Contains(leg))
				{
					packageLegs.Add(leg);
				}
			}
		}

		bool HasLocalTransportLegs(DataObjectList<TransportLeg> legs)
		{
			return legs != null && legs.Any(l => l.LegType == Enterprise.UniversalDataBuss.DataObjects.Universal.LegType.LocalTransport);
		}

		Dictionary<TransportLeg, List<Confirmation>> ConfirmationsByLeg;
		Dictionary<Confirmation, Instruction> InstructionByConfirmation;
		Dictionary<ZInt, List<TransportLeg>> TransportLegsByContainerLink;
		Dictionary<ZInt, List<TransportLeg>> TransportLegsByPackingLineLink;

		CommonCartageLeg CreateCartageLeg(CommonBookedCtgMove move)
		{
			var leg = move.CartageLegs.AddNew();
			leg.JU_DisplayOrder = move.CartageLegs.Count;
			return leg;
		}

		void SetValue(BusinessObject targetBO, SchemaColumn addressColumn, OrganizationAddress organizationAddress)
		{
			if (organizationAddress != null && targetBO != null)
			{
				var orgAddress = new OrganisationDataObjectReader(organizationAddress, logger, factory).GetMatched();
				if (orgAddress != null)
				{
					targetBO[addressColumn] = orgAddress.PK;
				}
			}
		}

		public Dictionary<ZGuid, ZInt> LinksDictionary
		{
			get { return linksDictionary ?? (linksDictionary = new Dictionary<ZGuid, ZInt>()); }
		}

		Dictionary<ZGuid, ZInt> linksDictionary;

		ICodeDescriptionPairListWithDefaultCode ReferenceTypes
		{
			get { return referencesTypes ?? (referencesTypes = WarehouseDataRegistry.Instance.AdditionalReferenceType.Value); }
		}

		ICodeDescriptionPairListWithDefaultCode referencesTypes;

		public override DataContextType DataContextType
		{
			get { return DataContextType.LocalTransport; }
		}
	}
}
