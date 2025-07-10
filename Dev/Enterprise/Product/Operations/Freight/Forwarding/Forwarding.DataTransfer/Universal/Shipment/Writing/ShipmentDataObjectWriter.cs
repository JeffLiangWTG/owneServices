using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ShipmentDataObjectWriter : BaseShipmentDataObjectWriter, IHierarchicalDataObjectWriter
	{
		public ShipmentDataObjectWriter(IDataWritingManager manager, bool checkSubShipments, bool checkForParent, bool alwaysExportTB = false)
			: this(manager, null, checkSubShipments, checkForParent, alwaysExportTB: alwaysExportTB)
		{
		}

		public ShipmentDataObjectWriter(IDataWritingManager manager, bool checkSubShipments, bool checkForParent, bool includeAdditionalReferenceCollectionForSubshipments, bool alwaysExportTB = false)
			: this(manager, null, checkSubShipments, checkForParent, null, null, includeAdditionalReferenceCollectionForSubshipments, alwaysExportTB: alwaysExportTB)
		{
		}

		public ShipmentDataObjectWriter(IDataWritingManager manager, IContainerLinkManager<ForwardingConsol> linkManager, bool checkSubShipments, bool checkForParent, ForwardingConsol consolToExcludeFromParents = null, PackLineLinkManager packLineLinkManager = null, bool includeAdditionalReferenceCollectionForSubshipments = false, bool alwaysExportTB = false)
			: base(manager)
		{
			this.linkManager = linkManager;
			this.IncludeChildren = checkSubShipments;
			this.IncludeParent = checkForParent;
			this.consolToExcludeFromParents = consolToExcludeFromParents;
			this.packLineLinkManager = packLineLinkManager;
			this.includeAdditionalReferenceCollectionForSubshipments = includeAdditionalReferenceCollectionForSubshipments;
			this.alwaysExportTB = alwaysExportTB;
		}

		protected IContainerLinkManager<ForwardingConsol> linkManager;

		ForwardingConsol currentConsol;

		readonly ForwardingConsol consolToExcludeFromParents;
		List<ForwardingShipment> parentShipments = new List<ForwardingShipment>();
		List<ForwardingConsol> parentConsols = new List<ForwardingConsol>();
		readonly bool includeAdditionalReferenceCollectionForSubshipments;
		readonly bool alwaysExportTB;

		#region IHierarchicalDataObjectWriter

		public bool IncludeParent { get; set; }
		public bool IncludeChildren { get; set; }

		#endregion

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.ForwardingShipment;
		}

		bool IsUsingUniversalSchemaVersion_2012
		{
			get { return writeManager.Schema == UniversalXmlSchema.Version_2012_11_DO_NOT_USE; }
		}

		protected override IContainerLinkManager<ForwardingConsol> GetContainerLinkManager()
		{
			return linkManager;
		}

		protected override PackLineLinkManager PackLineLinkManager
		{
			get { return packLineLinkManager ?? (packLineLinkManager = new PackLineLinkManager()); }
		}
		PackLineLinkManager packLineLinkManager;

		protected override void PopulateDataObject(ForwardingShipment shipmentBO, UniversalShipment shipmentData)
		{
			writeManager.AddPK(shipmentBO.PK);

			var listCache = BindToLists.GetCachedLists(shipmentBO.Factory);

			FindParentsAndCurrentConsol(shipmentBO);

			EnsureLinkManagerIsInitialized();

			if (!IsUsingUniversalSchemaVersion_2012)
			{
				var containerDataObjectWriter = new ContainerWithPackLinesDataObjectWriter<ForwardingConsol>(linkManager, listCache, writeManager);
				shipmentData.SetContainerCollection(() => ProcessCollection(shipmentBO.Containers, containerDataObjectWriter, CollectionContent.Complete, deDuplicate: true));
			}

			base.PopulateDataObject(shipmentBO, shipmentData);

			PopulateShipmentData(shipmentData, shipmentBO);
			PopulateConsolidatedCargoStatus(shipmentBO, shipmentData);
			PopulateAdditionalAddressInfoCollection(shipmentBO, shipmentData);

			var docsBO = shipmentBO.DocsAndCartage;
			if (docsBO != null)
			{
				var writer = new LocalProcessingDataObjectWriter(writeManager);
				shipmentData.LocalProcessing = writer.GetDataObject(docsBO);
			}

			var transportLegs = IsUsingUniversalSchemaVersion_2012
				? (BusinessObjectCollection)shipmentBO.Transports
				: shipmentBO.TransportsIncludingRelated;

			transportLegs.Sort(MovementLegComparer.PortsAndDatesBased(transportLegs));
			shipmentData.SetTransportLegCollection(() => ProcessCollection(transportLegs, new TransportLegDataObjectWriter(writeManager, shipmentBO), CollectionContent.Complete, true));

			var notes = shipmentBO.Notes.GetAllNotesVisibleToCurrentCompany().OrderBy(x => x.ST_Description);
			shipmentData.SetNoteCollection(() => ProcessCollection(notes, new NoteDataObjectWriter(writeManager), CollectionContent.Partial));

			shipmentData.DateCollection?.Add(Date.New(DateType.ShippedOnBoard, ZBool.False, shipmentBO.JS_ShippedOnBoardDate));
			shipmentData.DateCollection?.Add(Date.New(DateType.BillIssued, ZBool.False, shipmentBO.JS_HouseBillIssueDate));
			shipmentData.DateCollection?.Add(Date.New(DateType.PickupReceiptRequested, ZBool.False, shipmentBO.JS_ExportReceivingDepotReceiptRequested));
			shipmentData.DateCollection?.Add(Date.New(DateType.DeliveryReceiptRequested, ZBool.False, shipmentBO.JS_ImportReleaseDepotReceiptRequested));
			shipmentData.DateCollection?.Add(Date.New(DateType.PickupDispatchRequested, ZBool.False, shipmentBO.JS_ExportReceivingDepotDispatchRequested));
			shipmentData.DateCollection?.Add(Date.New(DateType.DeliveryDispatchRequested, ZBool.False, shipmentBO.JS_ImportReleaseDepotDispatchRequested));

			shipmentData.AddOrgAddress(writeManager, shipmentBO.ImportReleaseDepot, DocAddressType.ArrivalCFSAddress);
			shipmentData.AddOrgAddress(writeManager, shipmentBO.DocsAndCartage.DeliveryCartageCoAddr, AddressTypes.DeliveryLocalCartage);

			AddSubShipments(shipmentBO, shipmentData);

			MergeDeclarationForDocumentsData(shipmentBO, shipmentData);

			shipmentData.SetRelatedShipmentCollection(() => ProcessCollection(
				shipmentBO.AttachedOrders.Union(shipmentBO.AttachedOrdersFromSupplierBooking),
				new OrderDataObjectWriter(writeManager, OrderLineLinkManager)));

			PopulateCarriageShipmentCollection(shipmentBO, shipmentData);

			MergeUSInBondData(shipmentBO, shipmentData);
			shipmentData.RateCommodity = ListHelper.GetWithDescription<Commodity>(shipmentBO.JS_RH_NKRateCommodity, shipmentBO.JS_RH_NKRateCommodity_List);
			shipmentData.FMCTariffID = shipmentBO.JS_FMCTariffID;

			PopulateCO2eFields(shipmentBO, shipmentData);

			// Please keep this one the last to call as it will override Freight data.
			MergeAUCusHAWBDataIfNeeded(shipmentBO, shipmentData);
		}

		void PopulateCO2eFields(ForwardingShipment shipmentBO, UniversalShipment shipmentData)
		{
			shipmentData.GreenhouseGasEmission = new GreenhouseGasEmission
			{
				CO2e = shipmentBO.GetTotalCO2e(),
				CO2eUnit = ListHelper.GetWithDescription<UnitOfWeight>(Constants.Weight.Kilograms, shipmentBO.Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight)),
				CO2eDescriptiveStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(shipmentBO.GetCO2eStatus(), new CO2eStatusList())
					.AdditionalSetup(cdp => cdp.Description = CO2eHelper.GetCO2eStatusShortDescription(cdp.Code)),
			};
		}

		void PopulateAdditionalAddressInfoCollection(ForwardingShipment shipmentBO, UniversalShipment shipmentData)
		{
			shipmentData.SetAdditionalAddressInfoCollection(() => ProcessCollection(shipmentBO.JobAddressAdditionalInfoCollection, new JobAddressAdditionalInfoDataObjectWriter(writeManager)));
		}

		void PopulateConsolidatedCargoStatus(ForwardingShipment shipmentBO, UniversalShipment shipmentData)
		{
			if (shipmentBO.IsAir)
			{
				var auCusHawb = shipmentBO.AUCusHAWB;
				if (auCusHawb != null)
				{
					shipmentData.ConsolidatedCargoStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(auCusHawb.CS_CustomsStatus, auCusHawb.ConsolidatedCargoStatusesList);
				}
			}
			else if (shipmentBO.IsSea)
			{
				var auCusSCAHouse = shipmentBO.AUCusSCAHouse;
				if (auCusSCAHouse != null)
				{
					shipmentData.ConsolidatedCargoStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(auCusSCAHouse.CA_ShipmentStatus, auCusSCAHouse.ShipmentStatusesList);
				}
			}
		}

		void PopulateCarriageShipmentCollection(ForwardingShipment shipmentBO, UniversalShipment shipmentData)
		{
			if (shipmentBO.ShowContainerisedConfirms(ConfirmTimesSyncHelper.ConfirmType.Pickup))
			{
				var originConfirms = shipmentBO.Containers.Select(x => x.OriginGetConfirm);
				shipmentData.SetPreCarriageShipmentCollection(() => PopulateCarriageShipments(originConfirms, true));
			}
			else
			{
				shipmentData.SetPreCarriageShipmentCollection(() => PopulateCarriageShipments(shipmentBO.PickupConfirms, false));
			}

			if (shipmentBO.ShowContainerisedConfirms(ConfirmTimesSyncHelper.ConfirmType.Delivery))
			{
				var destinationConfirm = shipmentBO.Containers.Select(x => x.DestinationGetConfirm);
				shipmentData.SetPostCarriageShipmentCollection(() => PopulateCarriageShipments(destinationConfirm, true));
			}
			else
			{
				shipmentData.SetPostCarriageShipmentCollection(() => PopulateCarriageShipments(shipmentBO.DeliveryConfirms, false));
			}

			if ((!writeManager.FilteredDataContextType.HasValue
				|| writeManager.FilteredDataContextType != DataContextType.TransportBookingConsolidation)
				&& (alwaysExportTB || SystemDataRegistry.Instance.IncludeTransportBookingInShipmentXML.Value))
			{
				PopulateTransportBookings(shipmentBO, shipmentData);
			}
		}

		List<UniversalShipment> PopulateCarriageShipments(IEnumerable<CommonPickupDeliveryConfirm> confirms, bool isFCL)
		{
			var carriageShipments = new List<UniversalShipment>();
			var groups = confirms.Where(x => x != null && !x.IsEmpty).GroupBy(x => x.TransportProvider);

			foreach (var group in groups)
			{
				var shipment = new UniversalShipment(writeManager.WriterStrategy);
				shipment.AddOrgAddress(writeManager, group.Key, DocAddressType.TransportCompanyDocumentaryAddress);

				shipment.SetInstructionCollection(() =>
				{
					var result = new DataObjectList<Instruction>();

					var sources = isFCL ? GetFCLInstructionsForMapping(group) : GetLCLInstructionsForMapping(group);

					foreach (var source in sources)
					{
						var instruction = new Instruction(writeManager.WriterStrategy);

						if (source.PickupDeliveryType == Constants.PickupDeliveryConfirmTypes.OriginPickup)
						{
							instruction.Type = new CodeDescriptionPair()
							{
								Code = InstructionTypes.Codes.PickUp,
								Description = InstructionTypes.Descriptions.PickUp
							};
						}
						else if (source.PickupDeliveryType == Constants.PickupDeliveryConfirmTypes.DestinationDelivery)
						{
							instruction.Type = new CodeDescriptionPair()
							{
								Code = InstructionTypes.Codes.Delivery,
								Description = InstructionTypes.Descriptions.Delivery
							};
						}

						if (source.ConfirmAddressOverride)
						{
							instruction.Address = new JobDocAddressDataObjectWriter(writeManager).GetDataObject(source.ConfirmAddress);
						}

						if (!string.IsNullOrWhiteSpace(source.DropMode))
						{
							instruction.DropMode = new DropMode()
							{
								Code = source.DropMode,
								Description = source.Confirms.First().Lookups.DropModes.GetDescriptionFromCode(source.DropMode)
							};
						}

						instruction.Equipment = source.VehicleRegistration;
						instruction.ServiceInstruction = source.PickupDeliveryInstruction;

						if (isFCL)
						{
							instruction.SetInstructionContainerLinkCollection(() =>
							{
								var linkWriter = new InstructionContainerLinkDataObjectWriter(writeManager, linkManager);
								return ProcessCollection(source.Confirms, linkWriter);
							});
						}
						else
						{
							PopulatePacklineLinkForNonFCLShipment(source, instruction);
						}

						result.Add(instruction);
					}
					return result;
				});

				carriageShipments.Add(shipment);
			}

			return carriageShipments.Count == 0 ? null : carriageShipments;
		}

		void PopulateTransportBookings(ForwardingShipment shipmentBO, UniversalShipment shipmentData)
		{
			var dtbBookingConsolidations = TransportBookingLoader.GetBookingConsolidations(shipmentBO);

			foreach (BusinessObject bookingConsolidation in dtbBookingConsolidations)
			{
				if (writeManager.PKAlreadyExported(bookingConsolidation.PK))
				{
					return;
				}

				var dataContextManager = bookingConsolidation.GetUniversalDataContextManager() as IShipmentDataContextManager;
				if (dataContextManager != null)
				{
					var writer = dataContextManager.GetShipmentDataObjectWriter(writeManager);
					var bookingUShipment = writer.GetDataObject(bookingConsolidation) as UniversalShipment;
					var bookingConsolidationDirection = ((IDtbBookingConsolidation)bookingConsolidation).KB_JobDirection;

					if (bookingConsolidationDirection == nameof(DtbBookingDirection.DLV))
					{
						shipmentData.SetPostCarriageShipmentCollection(() => shipmentData.PostCarriageShipmentCollection.AddSafe(bookingUShipment));
					}
					else if (bookingConsolidationDirection == nameof(DtbBookingDirection.PIC))
					{
						shipmentData.SetPreCarriageShipmentCollection(() => shipmentData.PreCarriageShipmentCollection.AddSafe(bookingUShipment));
					}
				}
			}
		}

		void PopulatePacklineLinkForNonFCLShipment(InstructionForMapping source, Instruction instruction)
		{
			instruction.SetInstructionPackingLineLinkCollection(() => new List<InstructionPackingLineLink>());

			var confirmation = new PickupDeliveryConfirmationDataObjectWriter(writeManager).GetDataObject(source.Confirms.First());

			foreach (var divot in source.Confirms.First().Divots)
			{
				if (divot.PackLine != null)
				{
					var link = new InstructionPackingLineLink();
					link.Quantity = divot.J8_PackagesDelivered;
					link.PackingLineLink = PackLineLinkManager.GetPacklineLink(divot.PackLine);
					link.ConfirmationCollection = new List<Confirmation>();
					link.ConfirmationCollection.Add(confirmation);
					instruction.InstructionPackingLineLinkCollection?.Add(link);
				}
			}
		}

		List<InstructionForMapping> GetFCLInstructionsForMapping(IGrouping<OrgAddress, CommonPickupDeliveryConfirm> group)
		{
			var groupedConfirms = group
				.Where(x => x.IsConfirmAddressSameAsParentAddress)
				.GroupBy(x => new
				{
					x.EU_PickupDeliveryType,
					x.EU_DropMode,
					x.EU_VehicleRegistration,
					x.EU_PickupDeliveryInstruction
				})
				.Select(x => new InstructionForMapping()
				{
					PickupDeliveryType = x.Key.EU_PickupDeliveryType,
					DropMode = x.Key.EU_DropMode,
					VehicleRegistration = x.Key.EU_VehicleRegistration,
					PickupDeliveryInstruction = x.Key.EU_PickupDeliveryInstruction,
					ConfirmAddressOverride = false,
					ConfirmAddress = x.First().ExistingConfirmAddressFallbackToParent,
					Confirms = x.ToList()
				});

			var overridenConfirms = group
				.Where(x => !x.IsConfirmAddressSameAsParentAddress)
				.Select(x => new InstructionForMapping()
				{
					PickupDeliveryType = x.EU_PickupDeliveryType,
					DropMode = x.EU_DropMode,
					VehicleRegistration = x.EU_VehicleRegistration,
					PickupDeliveryInstruction = x.EU_PickupDeliveryInstruction,
					ConfirmAddressOverride = true,
					ConfirmAddress = x.ExistingConfirmAddressFallbackToParent,
					Confirms = new List<CommonPickupDeliveryConfirm>() { x }
				});

			return groupedConfirms.Union(overridenConfirms).ToList();
		}

		List<InstructionForMapping> GetLCLInstructionsForMapping(IGrouping<OrgAddress, CommonPickupDeliveryConfirm> group)
		{
			var result = new List<InstructionForMapping>();
			foreach (var confirm in group)
			{
				var mapping = new InstructionForMapping();
				mapping.PickupDeliveryType = confirm.EU_PickupDeliveryType;
				mapping.DropMode = confirm.EU_DropMode;
				mapping.VehicleRegistration = confirm.EU_VehicleRegistration;
				mapping.PickupDeliveryInstruction = confirm.EU_PickupDeliveryInstruction;
				mapping.ConfirmAddressOverride = !confirm.IsConfirmAddressSameAsParentAddress;
				mapping.ConfirmAddress = confirm.ExistingConfirmAddressFallbackToParent;
				mapping.Confirms = new List<CommonPickupDeliveryConfirm>() { confirm };

				result.Add(mapping);
			}

			return result;
		}

		void MergeUSInBondData(ForwardingShipment shipmentBO, UniversalShipment shipmentData)
		{
			if (Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) == Core.Constants.CountryCodes.UnitedStates)
			{
				MergeDataObjectWriterManager.AddBizObjData(writeManager, shipmentData, shipmentBO.InBondHeader as BusinessObject, includeParent: true, includeChildren: true);
			}
		}

		void MergeAUCusHAWBDataIfNeeded(ForwardingShipment shipmentBO, UniversalShipment shipmentData)
		{
			if (ShouldMergeWithAUCusHAWB)
			{
				MergeDataObjectWriterManager.AddBizObjData(writeManager, shipmentData, shipmentBO.AUCusHAWB as BusinessObject, includeParent: true, includeChildren: true);
			}
		}

		bool ShouldMergeWithAUCusHAWB
		{
			get { return HasRecipientRole(RecipientRoleType.AAD); }
		}

		bool ShouldMergeWithDeclarationForDocuments => !(HasRecipientRole(RecipientRoleType.PCA) || HasRecipientRole(RecipientRoleType.DCA));

		void MergeDeclarationForDocumentsData(ForwardingShipment shipmentBO, UniversalShipment shipmentData)
		{
			if (ShouldMergeWithDeclarationForDocuments)
			{
				MergeDataObjectWriterManager.AddBizObjData(writeManager, shipmentData, (BusinessObject)shipmentBO.DeclarationForDocuments, includeParent: true, includeChildren: true);
			}
		}

		void AddSubShipments(ForwardingShipment shipmentBO, UniversalShipment shipmentData)
		{
			if (IncludeChildren)
			{
				if (shipmentBO.JS_ShipmentType == Constants.ShipmentTypes.HighVolumeLowValue)
				{
					PopulateHVLVData(shipmentBO, shipmentData);
				}
				else
				{
					var data = ProcessCollection(shipmentBO.CoLoadShipments, new ShipmentDataObjectWriter(writeManager, linkManager, true, false, null, packLineLinkManager));
					shipmentData.SetSubShipmentCollection(() => data != null ? new DataObjectList<UniversalShipment>(data) : null);
				}
			}
		}

		void PopulateHVLVData(ForwardingShipment shipmentBO, UniversalShipment shipmentData)
		{
			var hvlWritingHelper = ObjectFactory.Get<IHVLShipmentWritingHelper>(nameof(IHVLShipmentWritingHelper), writeManager, shipmentBO, shipmentData, includeAdditionalReferenceCollectionForSubshipments);
			if (writeManager.HasRecipientRoleDetail(RecipientRoleType.BRI) && writeManager.FilteredDataContextType == DataContextType.CustomsDeclaration)
			{
				hvlWritingHelper.WriteCommercialInfo();
			}
			else
			{
				hvlWritingHelper.WriteConsignmentsAsSubShipments();
			}
		}

		#region Managing Parents and Current Consol/LinkManager

		void FindParentsAndCurrentConsol(ForwardingShipment shipmentBO)
		{
			var triggeringEvent = writeManager?.Action?.TriggeringEvent;
			var triggeringConsolPK = triggeringEvent != null && triggeringEvent.SL_Table == JobConsolSchema.Constants.TableName
				? triggeringEvent.ParentID
				: ZGuid.Empty;

			parentShipments = FindParentShipments(shipmentBO, triggeringConsolPK);

			currentConsol = FindCurrentConsol(shipmentBO);

			parentConsols = FindParentConsols(shipmentBO);
		}

		void EnsureLinkManagerIsInitialized()
		{
			if (linkManager == null)
			{
				linkManager = new ContainerLinkManager<ForwardingConsol>(currentConsol);
			}
		}

		List<ForwardingShipment> FindParentShipments(ForwardingShipment shipmentBO, ZGuid triggeringConsolPK)
		{
			var result = new List<ForwardingShipment>();

			if (!shipmentBO.JS_JS_ColoadMasterShipmentForBinding.IsEmpty)
			{
				var parentShipmentBO = shipmentBO.Factory.Load<ForwardingShipment>(shipmentBO.JS_JS_ColoadMasterShipmentForBinding);
				if (parentShipmentBO != null && !parentShipmentBO.IsHighVolumeLowValueMaster && !result.Contains(parentShipmentBO))
				{
					if (triggeringConsolPK.IsEmpty || parentShipmentBO.Consols.Any(consol => consol.PK == triggeringConsolPK))
					{
						result.Add(parentShipmentBO);
					}

					if (!IsUsingUniversalSchemaVersion_2012)
					{
						result.AddRange(FindParentShipments(parentShipmentBO, triggeringConsolPK));
					}
				}
			}

			return result;
		}

		protected virtual ForwardingConsol FindCurrentConsol(ForwardingShipment shipmentBO)
		{
			if (IsUsingUniversalSchemaVersion_2012)
			{
				return shipmentBO.DepartureConsol;
			}

			var topLevelShipment = GetTopmostParentShipment() ?? shipmentBO;

			if (topLevelShipment.Consols.Count == 0)
			{
				return null;
			}

			if (topLevelShipment.Consols.Count == 1)
			{
				return topLevelShipment.Consols[0];
			}

			return FindConsolUsingExplicitRecipientRoleType(topLevelShipment)
				?? FindConsolUsingTriggeringEvent(topLevelShipment)
				?? FindDefaultConsol(topLevelShipment);

			ForwardingShipment GetTopmostParentShipment()
			{
				var topLevelParentShipment = parentShipments.FirstOrDefault(shipment => shipment.JS_JS_ColoadMasterShipmentForBinding.IsEmpty);

				if (topLevelParentShipment == null)
				{
					// In the case when the USXML fires from an event added on a consol, parentShipments may exclude the topmost ASM shipment.
					// this block ensures we still find the highest ASM in this scenario.
					var parentShipmentPKs = parentShipments.Select(s => s.PK);
					topLevelParentShipment = parentShipments
						.FirstOrDefault(shipment => !parentShipmentPKs.Contains(shipment.JS_JS_ColoadMasterShipment));
				}

				return topLevelParentShipment;
			}
		}

		ForwardingConsol FindConsolUsingExplicitRecipientRoleType(ForwardingShipment shipmentBO)
		{
			var recipientRoleDetails = writeManager.Action.RecipientRoleDetails;

			bool isPickUp = recipientRoleDetails != null && recipientRoleDetails.Any(r => r.Type == RecipientRoleType.PCA);
			if (isPickUp)
			{
				return shipmentBO.Consols.GetEarliestConsol();
			}

			bool isDelivery = recipientRoleDetails != null && recipientRoleDetails.Any(r => r.Type == RecipientRoleType.DCA);
			if (isDelivery)
			{
				return shipmentBO.Consols.GetLatestConsol();
			}

			return null;
		}

		ForwardingConsol FindConsolUsingTriggeringEvent(ForwardingShipment shipmentBO)
		{
			var triggeringEvent = writeManager?.Action?.TriggeringEvent;
			if (triggeringEvent != null)
			{
				return (ForwardingConsol)new ConsolByEventMatcher().MatchConsolByEvent(shipmentBO, triggeringEvent);
			}

			return null;
		}

		ForwardingConsol FindDefaultConsol(ForwardingShipment shipmentBO)
		{
			return shipmentBO.Consols.GetEarliestConsol();
		}

		List<ForwardingConsol> FindParentConsols(ForwardingShipment shipmentBO)
		{
			var result = new List<ForwardingConsol>();

			if (IsUsingUniversalSchemaVersion_2012 && parentShipments.Any())
			{
				return result;
			}

			foreach (var consol in shipmentBO.Consols.Cast<ForwardingConsol>())
			{
				bool shouldBeAdded = consolToExcludeFromParents == null || consolToExcludeFromParents.PK != consol.PK;

				if (!IsUsingUniversalSchemaVersion_2012)
				{
					shouldBeAdded &= (currentConsol == null || currentConsol.PK != consol.PK);
				}

				if (shouldBeAdded)
				{
					result.Add(consol);
				}
			}

			return result;
		}

		protected override void InsertParents(ForwardingShipment shipmentBO, ref UniversalShipment shipmentData)
		{
			if (IncludeParent)
			{
				var originalTopLevelShipmentData = shipmentData;

				foreach (var parentConsolBO in parentConsols)
				{
					bool forceAddToParentShipmentCollection = !IsUsingUniversalSchemaVersion_2012;
					shipmentData = InsertParentConsol(parentConsolBO, shipmentData, forceAddToParentShipmentCollection);
				}

				foreach (var parentShipmentBO in parentShipments)
				{
					shipmentData = InsertParentShipment(parentShipmentBO, shipmentData);
				}

				if (!IsUsingUniversalSchemaVersion_2012 && currentConsol != null)
				{
					shipmentData = InsertParentConsol(currentConsol, shipmentData);
				}

				if (shipmentData != originalTopLevelShipmentData)
				{
					var foundshipmentBOSource = false;
					var reference = shipmentBO.JS_UniqueConsignRef;
					var dataContext = originalTopLevelShipmentData.DataContext;
					if (dataContext != null && dataContext.DataSourceCollection != null)
					{
						foreach (var dataSource in dataContext.DataSourceCollection)
						{
							DataContextType dataContextType;
							if (System.Enum.TryParse(dataSource.Type, out dataContextType))
							{
								var key = dataSource.Key.GetValueOrDefault();
								foundshipmentBOSource |= !foundshipmentBOSource && DataContextType.ForwardingShipment == dataContextType && reference == key;
								shipmentData.DataContext.AddDataSource(dataContextType, key);
							}
						}
					}

					if (!foundshipmentBOSource)
					{
						shipmentData.DataContext.AddDataSource(DataContextType.ForwardingShipment, reference);
					}
				}
			}
		}

		UniversalShipment InsertParentShipment(ForwardingShipment parentShipmentBO, UniversalShipment shipmentData)
		{
			bool shouldCheckForParent = IsUsingUniversalSchemaVersion_2012;
			var writer = new ShipmentDataObjectWriter(writeManager, linkManager, checkSubShipments: false, checkForParent: shouldCheckForParent);
			var parentDataObject = writer.GetDataObject(parentShipmentBO);

			return GetTopLevelDataObjectWithParentLinked(shipmentData, parentDataObject);
		}

		UniversalShipment InsertParentConsol(ForwardingConsol parentConsolBO, UniversalShipment shipmentData, bool forceAddToParentShipmentCollection = false)
		{
			using (writeManager.UseNewListForDuplicatePKCheck())
			{
				var dataWriterOptions = new DataWriterOptions()
				{
					IncludeSubShipments = false
				};

				if (forceAddToParentShipmentCollection)
				{
					dataWriterOptions.IncludeContainers = false;
				}

				var writer = GetConsolDataObjectWriter(dataWriterOptions);

				var parentDataObject = writer.GetDataObject(parentConsolBO);
				return GetTopLevelDataObjectWithParentLinked(shipmentData, parentDataObject, forceAddToParentShipmentCollection);
			}
		}

		UniversalShipment GetTopLevelDataObjectWithParentLinked(UniversalShipment shipmentData, UniversalShipment parentDataObject, bool forceAddToParentShipmentCollection = false)
		{
			if (IsUsingUniversalSchemaVersion_2012 || forceAddToParentShipmentCollection)
			{
				shipmentData.SetParentShipmentCollection(() => shipmentData.ParentShipmentCollection.AddSafe(parentDataObject));
				return shipmentData;
			}

			parentDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>(new[] { shipmentData }));
			return parentDataObject;
		}

		protected virtual ConsolDataObjectWriter GetConsolDataObjectWriter(DataWriterOptions dataWriterOptions)
		{
			return new ConsolDataObjectWriter(writeManager, linkManager, dataWriterOptions);
		}

		#endregion

		void PopulateShipmentData(UniversalShipment shipmentData, ForwardingShipment shipmentBO)
		{
			shipmentData.ScreeningStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(shipmentBO.JS_ScreeningStatus, shipmentBO.Lookups.ScreeningStatusesList);
			shipmentData.CartageWaybillNumber = shipmentBO.JS_CartageWaybill;
			shipmentData.DocumentedChargeable = shipmentBO.JS_DocumentedChargeable;
			shipmentData.DocumentedVolume = shipmentBO.JS_DocumentedVolume;
			shipmentData.DocumentedWeight = shipmentBO.JS_DocumentedWeight;
			shipmentData.HBLAWBChargesDisplay = ListHelper.GetWithDescription<CodeDescriptionPair>(shipmentBO.JS_HBLAWBChargesDisplay, shipmentBO.Lookups.JS_HBLAWBChargesDisplay_List);
			shipmentData.HBLContainerPackModeOverride = shipmentBO.JS_HBLContainerPackModeOverride;
			shipmentData.InsuranceValue = shipmentBO.JS_InsuranceValue;
			shipmentData.InsuranceValueCurrency = ListHelper.GetWithDescription<Currency>(shipmentBO.JS_RX_NKInsuranceCurrency, shipmentBO.Lookups.RefCurrency_List);
			shipmentData.IsBooking = shipmentBO.JS_IsBooking;
			shipmentData.IsCFSRegistered = shipmentBO.JS_IsCFSRegistered;
			shipmentData.IsNeutralMaster = new IsNeutralMaster() { Value = shipmentBO.JS_IsNeutralMaster };
			shipmentData.IsShipping = shipmentBO.JS_IsShipping;
			shipmentData.IsSplitShipment = shipmentBO.JS_IsSplitShipment;
			shipmentData.IsHighRisk = shipmentBO.JS_IsHighRisk;

			shipmentData.CompanyTariffLevelOverride = shipmentBO.JS_CompanyTariffLevelOverride;
			shipmentData.ManifestedChargeable = shipmentBO.JS_ManifestedChargeable;
			shipmentData.ManifestedVolume = shipmentBO.JS_ManifestedVolume;
			shipmentData.ManifestedWeight = shipmentBO.JS_ManifestedWeight;
			shipmentData.NoCopyBills = shipmentBO.JS_NoCopyBills;
			shipmentData.NoOriginalBills = shipmentBO.JS_NoOriginalBills;

			shipmentData.ReleaseType = ListHelper.GetWithDescription<CodeDescriptionPair>(shipmentBO.JS_ReleaseType, shipmentBO.Lookups.JS_ReleaseType_List);
			shipmentData.ShipmentType = ListHelper.GetWithDescription<CodeDescriptionPair>(shipmentBO.JS_ShipmentType, shipmentBO.Lookups.JS_ShipmentType_List);
			shipmentData.ShippedOnBoard = ListHelper.GetWithDescription<CodeDescriptionPair>(shipmentBO.JS_ShippedOnBoard, shipmentBO.Lookups.JS_ShippedOnBoard_List);

			shipmentData.ShipperCODAmount = shipmentBO.JS_ShipperCODAmount;
			shipmentData.ShipperCODPayMethod = ListHelper.GetWithDescription<CodeDescriptionPair>(shipmentBO.JS_ShipperCODPayMethod, shipmentBO.Lookups.ShipperCODPaymentTypes);

			shipmentData.TotalNoOfPacks = shipmentBO.JS_TotalPackageCount;
			shipmentData.TotalNoOfPacksPackageType = ListHelper.GetWithDescription<PackageType>(shipmentBO.JS_F3_NKTotalCountPackType, shipmentBO.Lookups.JS_PackType_List);

			shipmentData.TranshipToOtherCFS = shipmentBO.JS_TranshipToOtherCFS;

			shipmentData.CommunityTransitStatus = ListHelper.GetWithDescription<CodeDescriptionPair10Char>(shipmentBO.JS_CommunityTransitStatus, shipmentBO.Lookups.CommunityTransitStatusCodes);

			if (FreightDataRegistry.Instance.ExportDestinationValueInUXML.Value)
			{
				shipmentData.DestinationGoodsValue = shipmentBO.DestinationGoodsValue;
				shipmentData.DestinationGoodsValueCurrency = ListHelper.GetWithDescription<Currency>(shipmentBO.DestinationCurrencyCode, shipmentBO.Lookups.RefCurrency_List);
				shipmentData.DestinationExchangeRate = shipmentBO.DestinationExchangeRate;
			}

			if (shipmentBO.TransportsIncludingRelated.Count > 0)
			{
				var firstConsol = shipmentBO.Consols.GetEarliestConsol();
				if (firstConsol != null)
				{
					shipmentData.PortOfLoading = ListHelper.GetWithName(firstConsol.JK_RL_NKLoadPort, shipmentBO.Lookups.RefUNLOCO_List);
				}
				var lastConsol = shipmentBO.Consols.GetLatestConsol();
				if (lastConsol != null)
				{
					shipmentData.PortOfFirstArrival = ListHelper.GetWithName(lastConsol.JK_RL_NKPortOfFirstArrival, shipmentBO.Lookups.RefUNLOCO_List);
					shipmentData.PortOfDischarge = ListHelper.GetWithName(lastConsol.JK_RL_NKDischargePort, shipmentBO.Lookups.RefUNLOCO_List);
				}
			}
			else
			{
				shipmentData.PortOfLoading = !shipmentBO.JS_RL_NKLoadPort.IsEmpty
					? ListHelper.GetWithName(shipmentBO.JS_RL_NKLoadPort, shipmentBO.Lookups.RefUNLOCO_List)
					: null;
				shipmentData.PortOfDischarge = !shipmentBO.JS_RL_NKDischargePort.IsEmpty
					? ListHelper.GetWithName(shipmentBO.JS_RL_NKDischargePort, shipmentBO.Lookups.RefUNLOCO_List)
					: null;
			}

			shipmentData.AddOrgAddress(writeManager, shipmentBO.BookedShippingLineAddress, DocAddressType.ShippingLineAddress);
			shipmentData.AddOrgAddress(writeManager, shipmentBO.Creditor, DocAddressType.Creditor);

			var leg = shipmentBO.MostInterestingTransport;
			if (leg != null)
			{
				shipmentData.VesselName = leg.JW_Vessel;
				shipmentData.LloydsIMO = leg.Vessel.GetLloydsIMO();
				shipmentData.VoyageFlightNo = leg.JW_VoyageFlight;
			}

			shipmentData.WarehouseLocation = shipmentBO.JS_WarehouseLocation;

			try
			{
				if (shipmentBO.IsAir && shipmentBO.AWBHeader != null)
				{
					shipmentData.CarrierDocumentsOverride = new CarrierDocumentsOverride();
					shipmentData.CarrierDocumentsOverride.AWBHeader = new AWBHeaderDataObjectWriter(writeManager).GetDataObject(shipmentBO.AWBHeader);
				}
			}
			catch (ExportAWBHeaderReplaceMacrosException e)
			{
				throw new DataObjectValidationException(e.Message);
			}
			catch (Exception e) when (e.InnerException is ExportAWBHeaderReplaceMacrosException)
			{
				throw new DataObjectValidationException(e.InnerException.Message);
			}

			if (shipmentBO.IsAir)
			{
				if (shipmentBO.JS_InspectionTypeCode != FreightDataRegistry.AviationSecurity_Unknown_Code)
				{
					shipmentData.AviationSecurityInspectionType = ListHelper.GetWithDescription<CodeDescriptionPair>(shipmentBO.JS_InspectionTypeCode, shipmentBO.Lookups.InspectionTypes);
				}
				if (shipmentBO.JS_AdditionalInspectionTypeCode != FreightDataRegistry.AviationSecurity_Unknown_Code)
				{
					shipmentData.AviationSecurityAdditionalInspectionType = ListHelper.GetWithDescription<CodeDescriptionPair>(shipmentBO.JS_AdditionalInspectionTypeCode, shipmentBO.Lookups.AdditionalInspectionTypes);
				}
			}

			if (shipmentBO.Gateways.Count > 0)
			{
				shipmentData.SetGatewayInfoCollection(() => ProcessCollection(shipmentBO.Gateways, new ShipmentGatewayInfoDataObjectWriter(writeManager)));
			}
		}

		protected override IEnumerable<IPropertyValue> GetUserDefinedValues(ForwardingShipment shipmentBO)
		{
			var result = shipmentBO.GetUserDefinedValues();

			var docsBO = shipmentBO.DocsAndCartage;
			if (docsBO != null)
			{
				var customFieldsWritingHelper = new CustomFieldsDataObjectWritingHelper<JobDocsAndCartage>(docsBO, new JobDocsAndCartageCustomFieldsDescriptor());

				var docsAndCartageCustomValues = customFieldsWritingHelper.GetUserDefinedValues().ToArray();

				if (docsAndCartageCustomValues.Any())
				{
					result = result != null
						? result.Concat(docsAndCartageCustomValues)
						: docsAndCartageCustomValues;
				}
			}

			return result;
		}
	}
}
