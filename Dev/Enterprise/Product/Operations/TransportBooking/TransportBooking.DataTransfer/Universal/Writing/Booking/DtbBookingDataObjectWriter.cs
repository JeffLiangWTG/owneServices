using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Core;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportBookings.DataTransfer.Universal
{
	public class DtbBookingDataObjectWriter : TopLevelDataObjectWriter<DtbBooking, UniversalShipment>
	{
		internal DtbBookingDataObjectWriter(Dictionary<ZGuid, ZInt> linksDictionary, IDataWritingManager manager, bool includeParentConsolidation)
			: base(manager)
		{
			LinksDictionary = linksDictionary;
			IncludeParentConsolidation = includeParentConsolidation;
		}

		readonly bool IncludeParentConsolidation;

		protected override bool ShouldSendJobCostingData(DtbBooking sourceBO)
		{
			var recipientRoleDetails = writeManager.Action.RecipientRoleDetails;
			var sentToTransportCompany = recipientRoleDetails != null && recipientRoleDetails.Any(r => r.Type == RecipientRoleType.TPC);
			var transportCompany = sourceBO.Address.Organisation;
			return (sentToTransportCompany && transportCompany != null && transportCompany.IsProxyOrgOfAnyCompany()) || base.ShouldSendJobCostingData(sourceBO);
		}

		protected override void AddImportMetaDataOnJobCosting(JobCosting jobCosting, DtbBooking bookingBO)
		{
			if (jobCosting != null && jobCosting.ChargeLineCollection != null && bookingBO.ParentJob == null && bookingBO.Instructions.DeliveryInstructions.Count() == 1)
			{
				foreach (var chargeLine in jobCosting.ChargeLineCollection)
				{
					chargeLine.ImportMetaData = new ImportMetaData(writeManager.WriterStrategy) { Instruction = InstructionType.Insert };
				}
			}
		}

		protected override void PopulateDataObject(DtbBooking bookingBO, UniversalShipment bookingDataObject)
		{
			Argument.NotNull(bookingBO, "DtbBooking bookingBO");

			ValidateForSendingToCTOIfRequired(bookingBO);

			ConsolidationDataObject = GetConsolidationDataObject(bookingBO, bookingDataObject);

			PopulateData(bookingBO, ConsolidationDataObject, bookingDataObject);
			PopulateRelatedEntities(bookingBO, bookingDataObject);
			PopulateDateCollection(bookingDataObject, bookingBO);
		}

		void ValidateForSendingToCTOIfRequired(DtbBooking bookingBO)
		{
			if (IsRecipientTypeCBA() && IsExportingToCTO(bookingBO))
			{
				ValidateForSendingToCTO(bookingBO);
			}
		}

		void ValidateForSendingToCTO(DtbBooking bookingBO)
		{
			if (bookingBO.IsAvailable)
			{
				try
				{
					bookingBO.IsValidatingSendingXUSToCTO = true;
					bookingBO.Validation.ValidateAll();
				}
				finally
				{
					bookingBO.IsValidatingSendingXUSToCTO = false;
				}

				var bookingInFactoryForUniversalDataHook = writeManager.Action.FactoryForProcessing.Load<DtbBooking>(bookingBO.PK);
				bookingInFactoryForUniversalDataHook.NotificationBufferForSendingXUSToCTO = bookingBO.NotificationBufferForSendingXUSToCTO;

				if (bookingBO.NotificationBufferForSendingXUSToCTO.Events.HasMessageErrors() || bookingBO.NotificationBufferForSendingXUSToCTO.Events.HasErrors())
				{
					var eventMessageStringBuilder = new ZStringBuilder();
					foreach (var errorEvent in bookingBO.NotificationBufferForSendingXUSToCTO.Events.Where(e => e.Type == CargoWise.EntityFramework.NotificationType.MessageError || e.Type == CargoWise.EntityFramework.NotificationType.Error))
					{
						eventMessageStringBuilder.AppendLine(errorEvent.Message);
					}

					throw new DataObjectValidationException(eventMessageStringBuilder.ToString());
				}
			}
			else
			{
				throw new DataObjectValidationException(Res.GetString("e617e84b-43fe-4fcc-911a-9d71547026fd", "Only available bookings can be sent to Container Transport Optimization"));
			}
		}

		bool IsRecipientTypeCBA()
		{
			var recipientRoleDetails = writeManager.Action.RecipientRoleDetails;
			var sentToCarrierBookingAgent = recipientRoleDetails != null && recipientRoleDetails.Any(r => r.Type == RecipientRoleType.CBA);
			return sentToCarrierBookingAgent;
		}

		bool IsExportingToCTO(DtbBooking bookingBO)
		{
			var isSendingToCTO = false;

			if (bookingBO.CarrierBookingAgent != null)
			{
				foreach (EDICommunicationsMode communicationMode in bookingBO.CarrierBookingAgent.EDICommunicationsModes)
				{
					if (communicationMode.EK_Destination == DtbAgentBooking.ContainerTransportOptimizationCBA && communicationMode.EK_Module == WorkflowDescriptors.DtbBookingWorkflowDescriptorCode && communicationMode.EK_FileFormat == EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment && communicationMode.EK_CommsDirection != EDICommunicationsModeCommsDirectionList.Codes.Receive && communicationMode.EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.EHubService)
					{
						isSendingToCTO = true;
					}
				}
			}

			return isSendingToCTO;
		}

		static void PopulateData(DtbBooking bookingBO, UniversalShipment consolidationDataObject, UniversalShipment bookingDataObject)
		{
			var parentJob = bookingBO.ParentJob;

			bookingDataObject.Branch = new Branch()
			{
				Code = bookingBO.Branch?.GB_Code,
				Name = bookingBO.Branch?.GB_BranchName
			};

			bookingDataObject.LocalTransportJobType = ListHelper.GetWithDescription<CodeDescriptionPair4Char>(bookingBO.KM_KT_NKBookingTemplate, bookingBO.Lookups.BookingTemplates);
			bookingDataObject.TransportBookingDirection = ListHelper.GetWithDescription<TransportBookingDirection>(bookingBO.KM_Direction, new BindToLists(bookingBO.Factory).Directions);

			if (parentJob != null && parentJob.TransportMode.HasValue)
			{
				bookingDataObject.TransportMode = ListHelper.GetWithDescription<UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair>(parentJob.TransportMode.Value, new CodeDescriptionPairList(Enterprise.ZArchitecture.Core.OLookUpEditType.LocalCartageTransportModes));
			}
			else if (consolidationDataObject != null && consolidationDataObject.TransportMode != null)
			{
				bookingDataObject.TransportMode = consolidationDataObject.TransportMode;
			}

			bookingDataObject.ContainerMode = GetContainerMode(bookingBO);

			bookingDataObject.CarrierServiceLevel = ListHelper.GetWithDescription<ServiceLevel>(bookingBO.KM_PL_NKCarrierServiceLevel, bookingBO.Lookups.CarrierServiceLevels);
			bookingDataObject.ServiceLevel = ListHelper.GetWithDescription<ServiceLevel>(bookingBO.KM_RS_NKServiceLevel, bookingBO.Lookups.ServiceLevels);

			bookingDataObject.IsHazardous = bookingBO.KM_IsHazardous;
			bookingDataObject.LocalProcessing = new LocalProcessing { ArrivalCartageRef = bookingBO.KM_TransportReference };
			bookingDataObject.RatingTransportMode = ListHelper.GetWithDescription<UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair>(bookingBO.KM_RatingFreightMode, bookingBO.Lookups.RatingFreightModes);
			bookingDataObject.BookingTransportMode = ListHelper.GetWithDescription<UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair>(bookingBO.KM_TransportMode, bookingBO.Lookups.BookingTransportModes);
			bookingDataObject.RequiresRefrigeration = bookingBO.KM_RequiresRefrigeration;
			bookingDataObject.ShipmentStatus = ListHelper.GetWithDescription<UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair>(bookingBO.KM_Status, bookingBO.Lookups.BindToLists.Statuses);

			var carrierAccount = bookingBO.CarrierAccount;
			if (carrierAccount != null)
			{
				bookingDataObject.CarrierAccount = new CarrierAccount()
				{
					AccountNumber = carrierAccount.OAN_AccountNumber,
					MerchantNumber = carrierAccount.OAN_MerchantNumber,
					DepotID = carrierAccount.OAN_DepotID
				};
			}

			if (bookingBO.KM_OverrideChargeable)
			{
				bookingDataObject.TotalWeightUnit = new UnitOfWeight() { Code = bookingBO.ChargeableUnits };
				bookingDataObject.ActualChargeable = bookingBO.KM_Chargeable;
			}

			if (bookingBO.ConsolidationSingleJob != null && !bookingBO.ConsolidationSingleJob.KB_GoodsDescription.IsEmpty)
			{
				bookingDataObject.GoodsDescription = bookingBO.ConsolidationSingleJob.KB_GoodsDescription;
			}
		}

		void PopulateDateCollection(UniversalShipment dataObject, DtbBooking bookingBO)
		{
			if (dataObject.DateCollection == null)
			{
				dataObject.SetDateCollection(() => new List<Date>());
			}
			dataObject.DateCollection.Add(DateType.JobCreated, ZBool.False, bookingBO.KM_SystemCreateTimeUtc);
		}

		static ContainerMode GetContainerMode(DtbBooking bookingBO)
		{
			ContainerMode containerMode = null;

			var hasPackages = bookingBO.HasPackages;
			var ratingMode = bookingBO.KM_RatingFreightMode;
			if ((hasPackages && bookingBO.IsContainerisedOnly) || ratingMode == RatingFreightModes.Codes.Containerised)
			{
				containerMode = new ContainerMode() { Code = Core.Constants.CartageContainerMode.Containerized, Description = Core.Constants.CartageContainerModeDescription.Containerized };
			}
			else if ((hasPackages && bookingBO.IsLooseOnly) || ratingMode == RatingFreightModes.Codes.Loose)
			{
				containerMode = new ContainerMode() { Code = Core.Constants.CartageContainerMode.Loose, Description = Core.Constants.CartageContainerModeDescription.Loose };
			}
			else if ((hasPackages && bookingBO.IsContainerised && bookingBO.IsLoose) || ratingMode == RatingFreightModes.Codes.Both)
			{
				containerMode = new ContainerMode() { Code = Core.Constants.CartageContainerMode.Mixed, Description = Core.Constants.CartageContainerModeDescription.Mixed };
			}

			return containerMode;
		}

		protected override IEnumerable<IPropertyValue> GetUserDefinedValues(DtbBooking sourceBO)
		{
			return sourceBO.GetUserDefinedValues();
		}

		void PopulateRelatedEntities(DtbBooking bookingBO, UniversalShipment bookingDataObject)
		{
			bookingDataObject.SetOrganizationAddressCollection(() => ProcessCollection(bookingBO.DocAddresses, new JobDocAddressDataObjectWriter(writeManager)
			{
				PopulateGeoLocation = true,
				PopulateValidationStatus = true,
			}));
			PopulateLocalClient(bookingBO, bookingDataObject);
			bookingDataObject.SetInstructionCollection(() => GetInstructionCollection(bookingBO));

			PopulateAdditionalReferences(bookingBO, bookingDataObject);

			bookingDataObject.SetNoteCollection(() =>
			{
				var notes = bookingBO.Notes.GetAllNotesVisibleToCurrentCompany().OrderBy(x => x.ST_Description);
				return ProcessCollection(notes, new NoteDataObjectWriter(writeManager), CollectionContent.Partial);
			});
		}

		DataObjectList<Instruction> GetInstructionCollection(DtbBooking bookingBO)
		{
			return ProcessCollection(bookingBO.Instructions, new DtbBookingInstructionDataObjectWriter(LinksDictionary, writeManager), CollectionContent.Complete);
		}

		void PopulateLocalClient(DtbBooking bookingBO, UniversalShipment bookingDataObject)
		{
			var job = bookingBO.Job;
			if (job != null)
			{
				bookingDataObject.AddOrgAddress(
					manager: writeManager,
					orgAddress: job.LocalChargesAddr,
					addressType: AddressTypes.SendersLocalClient,
					populateGeolocation: true,
					populateValidationStatus: true);
			}
		}

		void PopulateAdditionalReferences(DtbBooking bookingBO, UniversalShipment bookingDataObject)
		{
			var additionalReferenceCollection = ProcessCollection(bookingBO.AdditionalReferenceNumbers,
				new AdditionalReferenceDataObjectWriter(writeManager), CollectionContent.Complete)
				?? new DataObjectList<AdditionalReference>();

			PopulateWayBill(bookingDataObject, additionalReferenceCollection);
			PopulateParentID(bookingBO, additionalReferenceCollection);

			if (additionalReferenceCollection.Any())
			{
				bookingDataObject.SetAdditionalReferenceCollection(() => additionalReferenceCollection);
			}
		}

		void PopulateParentID(DtbBooking bookingBO, DataObjectList<AdditionalReference> additionalReferenceCollection)
		{
			if (bookingBO.ParentJob != null)
			{
				CreateOrUpdateReference(bookingBO.ParentJob.JobNumber,
					new EntryType() { Code = TransportCommonAdditionalReferenceTypes.Codes.BookingPartyReference, Description = TransportCommonAdditionalReferenceTypes.Descriptions.BookingPartyReference },
					additionalReferenceCollection);
			}
		}

		void CreateOrUpdateReference(ZString referenceNumber, EntryType referenceType, DataObjectList<AdditionalReference> referenceCollection)
		{
			if (!referenceNumber.IsEmpty)
			{
				var additionalReference = new AdditionalReference() { Type = referenceType };
				referenceCollection.Add(additionalReference);
				additionalReference.ReferenceNumber = referenceNumber;
			}
		}

		void PopulateWayBill(UniversalShipment bookingDataObject, DataObjectList<AdditionalReference> additionalReferenceCollection)
		{
			var populatedHouseBill = TryPopulateWayBill(TransportCommonAdditionalReferenceTypes.Codes.HouseBill, bookingDataObject, additionalReferenceCollection, WayBillTypeList.Codes.House);

			if (!populatedHouseBill)
			{
				TryPopulateWayBill(TransportCommonAdditionalReferenceTypes.Codes.MasterBill, bookingDataObject, additionalReferenceCollection, WayBillTypeList.Codes.Master);
			}
		}

		bool TryPopulateWayBill(string additionalReferenceTypeCode, UniversalShipment bookingDataObject, DataObjectList<AdditionalReference> additionalReferenceCollection, string uxmlWayBillTypeCode)
		{
			var wayBill = additionalReferenceCollection.FirstOrDefault(r => r.Type != null && r.Type.GetCodeAsUpperCase() == additionalReferenceTypeCode);

			if (wayBill != null)
			{
				var wayBillNumber = wayBill.ReferenceNumber.GetValueOrDefault();
				if (!wayBillNumber.IsEmpty)
				{
					bookingDataObject.WayBillNumber = wayBillNumber;
					bookingDataObject.WayBillType = ListHelper.GetWithDescription<WayBillType>(uxmlWayBillTypeCode, new WayBillTypeList());
				}
			}

			return !bookingDataObject.WayBillNumber.GetValueOrDefault().IsEmpty;
		}

		protected override void InsertParents(DtbBooking bookingBO, ref UniversalShipment bookingDataObject)
		{
			if (writeManager.Schema != UniversalXmlSchema.Version_2012_11_DO_NOT_USE && ConsolidationDataObject != null)
			{
				bookingDataObject = ConsolidationDataObject;
			}
		}

		UniversalShipment ConsolidationDataObject;

		UniversalShipment GetConsolidationDataObject(DtbBooking bookingBO, UniversalShipment bookingDataObject)
		{
			UniversalShipment consolidationDataObject = null;
			if (IncludeParentConsolidation)
			{
				var consolidation = bookingBO.ConsolidationSingleJob;
				if (consolidation != null)
				{
					var bookingConsolidationWriter = new DtbBookingConsolidationDataObjectWriter(writeManager, false);
					consolidationDataObject = bookingConsolidationWriter.GetDataObject(consolidation);

					if (writeManager.Schema == UniversalXmlSchema.Version_2012_11_DO_NOT_USE)
					{
						bookingDataObject.SetParentShipmentCollection(() => bookingDataObject.ParentShipmentCollection.AddSafe(consolidationDataObject));
					}
					else
					{
						consolidationDataObject.DataContext.AddDataSource(DataContextType.TransportBooking, bookingBO.KM_JobID);

						if (consolidationDataObject.SubShipmentCollection == null)
						{
							consolidationDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
						}
						if (consolidationDataObject.SubShipmentCollection != null)
						{
							consolidationDataObject.SubShipmentCollection.Add(bookingDataObject);
						}
					}

					LinksDictionary = bookingConsolidationWriter.LinksDictionary;

					var containerLinks = new Dictionary<int, PkgPackage>();
					var packingLinks = new Dictionary<int, PkgPackage>();
					PrepareContainerAndPackageLinks(bookingBO, containerLinks, packingLinks);
					RemoveUnrelatedContainers(consolidationDataObject, containerLinks, bookingBO.AssignedPackages);
					RemoveUnrelatedPackingLines(consolidationDataObject, packingLinks, bookingBO.AssignedPackages.Concat(bookingBO.AssignedPackages.SelectMany(p => p.GetAllPackages())));
				}
			}
			return consolidationDataObject;
		}

		void PrepareContainerAndPackageLinks(DtbBooking bookingBO, Dictionary<int, PkgPackage> containerLinks, Dictionary<int, PkgPackage> packingLinks)
		{
			var consolidationPackages = bookingBO.PackageJob.Packages;
			var consolidationPackagesIncludingChildren = consolidationPackages.Concat(consolidationPackages.SelectMany(p => p.GetAllPackages()));
			foreach (var link in LinksDictionary)
			{
				var package = consolidationPackagesIncludingChildren.FirstOrDefault(p => p.PK == link.Key);
				if (package != null)
				{
					var links = package.IsContainer ? containerLinks : packingLinks;
					links.Add(link.Value, package);
				}
			}
		}

		void RemoveUnrelatedContainers(UniversalShipment consolidationDataObject, Dictionary<int, PkgPackage> containerLinks, IEnumerable<PkgPackage> assignedPackages)
		{
			var containerCollection = consolidationDataObject.ContainerCollection;
			if (containerCollection != null)
			{
				foreach (var container in containerCollection.ToArray())
				{
					var link = container.Link;
					PkgPackage pkgContainer;

					if (link.HasValue && containerLinks.TryGetValue(link.Value, out pkgContainer) && !assignedPackages.Contains(pkgContainer))
					{
						containerCollection.Remove(container);
						LinksDictionary.Remove(pkgContainer.PK);
					}
				}
			}
		}

		void RemoveUnrelatedPackingLines(UniversalShipment consolidationDataObject, Dictionary<int, PkgPackage> packingLinks, IEnumerable<PkgPackage> assignedPackages)
		{
			var packingCollection = consolidationDataObject.PackingLineCollection;
			if (packingCollection != null)
			{
				foreach (var packLine in packingCollection.ToArray())
				{
					var link = packLine.Link;
					PkgPackage pkgPackage;

					if (link.HasValue && packingLinks.TryGetValue(link.Value, out pkgPackage) && !assignedPackages.Contains(pkgPackage))
					{
						packingCollection.Remove(packLine);
						LinksDictionary.Remove(pkgPackage.PK);

						// inners that are assigned, will need to be added to the Consolidation PackingLine collection now that it's parent has been removed.
						AddInnerPackagesIfAssigned(packLine, packingCollection, packingLinks, assignedPackages);
					}
				}
			}
		}

		void AddInnerPackagesIfAssigned(PackingLine parentPackLine, DataObjectList<PackingLine> consolidationPackingCollection, Dictionary<int, PkgPackage> packingLinks, IEnumerable<PkgPackage> assignedPackages)
		{
			var packingLines = parentPackLine.PackingLineCollection;
			if (packingLines != null)
			{
				foreach (var packLine in packingLines)
				{
					var link = packLine.Link;
					PkgPackage pkgPackage;

					if (link.HasValue && packingLinks.TryGetValue(link.Value, out pkgPackage) && assignedPackages.Contains(pkgPackage))
					{
						consolidationPackingCollection.Add(packLine);
					}
					else
					{
						AddInnerPackagesIfAssigned(packLine, consolidationPackingCollection, packingLinks, assignedPackages);
					}
				}
			}
		}

		public Dictionary<ZGuid, ZInt> LinksDictionary
		{
			get { return linksDictionary ?? (linksDictionary = new Dictionary<ZGuid, ZInt>()); }
			private set
			{
				if (linksDictionary != null)
				{
					throw new InvalidOperationException("Should not override Links Dictionary if it already has a value.");
				}

				linksDictionary = value;
			}
		}

		Dictionary<ZGuid, ZInt> linksDictionary;

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.TransportBooking;
		}
	}
}
