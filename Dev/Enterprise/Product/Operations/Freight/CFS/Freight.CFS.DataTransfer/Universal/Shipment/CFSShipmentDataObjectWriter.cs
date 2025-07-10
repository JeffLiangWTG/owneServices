using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.CFS.DataTransfer.Universal
{
	sealed class CFSShipmentDataObjectWriter : TopLevelDataObjectWriter<CFSShipment, Shipment>
	{
		public CFSShipmentDataObjectWriter(IDataWritingManager manager, bool includeChildren, bool includeParents, ContainerLinkManager<CFSLoadListConsol> linkManager = null, CFSLoadListConsol consolToExclude = null)
			: base(manager)
		{
			this.includeChildren = includeChildren;
			this.includeParents = includeParents;
			this.linkManager = linkManager;
			this.consolToExclude = consolToExclude;
		}

		readonly bool includeChildren;
		readonly bool includeParents;
		ContainerLinkManager<CFSLoadListConsol> linkManager;
		readonly CFSLoadListConsol consolToExclude;

		List<CFSShipment> parentShipments = new List<CFSShipment>();
		List<CFSLoadListConsol> parentConsols = new List<CFSLoadListConsol>();
		CFSLoadListConsol currentConsol;

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.CFSShipment;
		}

		protected override void PopulateDataObject(CFSShipment shipmentBO, Shipment dataObject)
		{
			linkManager = linkManager ?? new ContainerLinkManager<CFSLoadListConsol>(null);

			var listCache = BindToLists.GetCachedLists(shipmentBO.Factory);

			FindParentsAndCurrentConsol(shipmentBO);

			dataObject.ShipmentType = ListHelper.GetWithDescription<CodeDescriptionPair>(shipmentBO.JS_ShipmentType, shipmentBO.Lookups.JS_ShipmentType_List);
			dataObject.TransportMode = ListHelper.GetWithDescription<CodeDescriptionPair>(shipmentBO.JS_TransportMode, shipmentBO.Lookups.JS_TransportMode_List);
			dataObject.WayBillNumber = shipmentBO.JS_HouseBill;
			dataObject.PortOfOrigin = ListHelper.GetWithName(shipmentBO.JS_RL_NKOrigin, shipmentBO.Lookups.RefUNLOCO_List);
			dataObject.PortOfDestination = ListHelper.GetWithName(shipmentBO.JS_RL_NKDestination, shipmentBO.Lookups.RefUNLOCO_List);
			dataObject.AgentsReference = shipmentBO.JS_ConsolReference;
			dataObject.InterimReceiptNumber = shipmentBO.JS_InterimReceipt;
			dataObject.CartageWaybillNumber = shipmentBO.JS_CartageWaybill;
			dataObject.GoodsDescription = shipmentBO.JS_GoodsDescription;
			dataObject.ServiceLevel = ListHelper.GetWithDescription<ServiceLevel>(shipmentBO.JS_RS_NKServiceLevel, shipmentBO.Lookups.RefServiceLevel_List);
			dataObject.WarehouseLocation = shipmentBO.JS_WarehouseLocation;
			dataObject.OuterPacks = shipmentBO.JS_OuterPacks;
			dataObject.OuterPacksPackageType = ListHelper.GetWithDescription<PackageType>(shipmentBO.JS_F3_NKPackType, shipmentBO.Lookups.JS_PackType_List);
			dataObject.TotalVolume = shipmentBO.JS_ActualVolume;
			dataObject.TotalVolumeUnit = ListHelper.GetWithDescription<UnitOfVolume>(shipmentBO.JS_UnitOfVolume, listCache.VolumeUnits);
			dataObject.TotalWeight = shipmentBO.JS_ActualWeight;
			dataObject.TotalWeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(shipmentBO.JS_UnitOfWeight, listCache.WeightUnits);
			dataObject.TranshipToOtherCFS = shipmentBO.JS_TranshipToOtherCFS;

			dataObject.SetDateCollection(() => new List<Date> { Date.New(DateType.BookingConfirmed, false, shipmentBO.JS_A_BKD) });

			var entryNumberBO = shipmentBO.ShipmentCustomsEntryNumber;
			if (entryNumberBO != null && !entryNumberBO.EntryNumber.IsEmpty)
			{
				dataObject.SetEntryNumberCollection(() =>
				{
					var entryNumber = new EntryNumber();
					entryNumber.Type = ListHelper.GetWithDescription<EntryType>(entryNumberBO.EntryType, entryNumberBO.EntryType_List);
					entryNumber.Number = entryNumberBO.EntryNumber;

					return new List<EntryNumber> { entryNumber };
				});
			}

			var docsBO = shipmentBO.DocsAndCartage;
			if (docsBO != null)
			{
				dataObject.LocalProcessing = new LocalProcessing(writeManager.WriterStrategy);
				dataObject.LocalProcessing.LCLAvailable = docsBO.JP_LCLAvailable;
				dataObject.LocalProcessing.LCLStorageCommences = docsBO.JP_LCLStorageCommences;
				dataObject.LocalProcessing.SetAdditionalServiceCollection(() => ProcessCollection(docsBO.Services, new AdditionalServiceDataObjectWriter(writeManager), CollectionContent.Complete));
			}

			PopulateOrganizations(shipmentBO, dataObject);

			var packLineWriter = new PackingLineWithContainerDataObjectWriter<CFSPackLine, CFSLoadListConsol>(linkManager, listCache, writeManager);
			dataObject.SetPackingLineCollection(() => ProcessCollection(shipmentBO.OuterPackLines, packLineWriter, CollectionContent.Complete));

			if (includeChildren)
			{
				var data = ProcessCollection(shipmentBO.CoLoadShipments, new CFSShipmentDataObjectWriter(writeManager, true, false, linkManager));
				dataObject.SetSubShipmentCollection(() => data != null ? new DataObjectList<Shipment>(data) : null);
			}
		}

		void FindParentsAndCurrentConsol(CFSShipment shipmentBO)
		{
			parentShipments = FindParentShipments(shipmentBO);
			currentConsol = FindCurrentConsol(shipmentBO);
			parentConsols = FindParentConsols(shipmentBO);
		}

		void PopulateOrganizations(CFSShipment shipmentBO, Shipment dataObject)
		{
			dataObject.AddOrgAddress(writeManager, shipmentBO.HandledOnBehalfOfForwarder, AddressTypes.Forwarder);
			dataObject.AddOrgAddress(writeManager, shipmentBO.ConsignorDocumentaryAddress);
			dataObject.AddOrgAddress(writeManager, shipmentBO.ConsigneeDocumentaryAddress);
			dataObject.AddOrgAddress(writeManager, shipmentBO.CartageCoAddr, DocAddressType.LocalCartageAddress1);
			dataObject.AddOrgAddress(writeManager, shipmentBO.ConsignorPickupAddress);
			dataObject.AddOrgAddress(writeManager, shipmentBO.ConsigneeDeliveryAddress);
		}

		protected override void InsertParents(CFSShipment shipment, ref Shipment shipmentData)
		{
			if (includeParents)
			{
				var originalTopLevelShipmentData = shipmentData;

				shipmentData = InsertParentShipmentCollection(shipment, shipmentData);

				if (!IsUsingUniversalSchemaVersion2012 && currentConsol != null)
				{
					shipmentData = InsertParentConsol(shipmentData);
				}

				if (shipmentData != originalTopLevelShipmentData)
				{
					var reference = shipment.JS_UniqueConsignRef;
					var foundShipmentBOSource = false;
					var dataContext = originalTopLevelShipmentData.DataContext;
					if (dataContext?.DataSourceCollection != null)
					{
						foreach (var dataSource in dataContext.DataSourceCollection)
						{
							if (System.Enum.TryParse(dataSource.Type, out DataContextType dataContextType))
							{
								var key = dataSource.Key.GetValueOrDefault();
								foundShipmentBOSource |= !foundShipmentBOSource && DataContextType.CFSShipment == dataContextType && reference == key;
								shipmentData.DataContext.AddDataSource(dataContextType, key);
							}
						}
					}

					if (!foundShipmentBOSource)
					{
						shipmentData.DataContext.AddDataSource(DataContextType.CFSShipment, reference);
					}
				}
			}
		}

		Shipment InsertParentShipmentCollection(CFSShipment shipment, Shipment shipmentData)
		{
			if (shipment.CoLoadMasterShipment != null)
			{
				shipmentData.SetParentShipmentCollection(() => ProcessCollection(new[] { shipment.CoLoadMasterShipment },
					new CFSShipmentDataObjectWriter(writeManager, false, true, linkManager)));
			}
			else
			{
				var consolsToWrite = shipment.Consols.Where(x => x != consolToExclude).ToArray();
				linkManager = linkManager ??
					new ContainerLinkManager<CFSLoadListConsol>(
						consolsToWrite.Cast<CFSLoadListConsol>().FirstOrDefault());

				shipmentData.SetParentShipmentCollection(() => ProcessCollection(consolsToWrite,
					new CFSLoadListConsolDataObjectWriter(writeManager, false, linkManager)));
			}

			return shipmentData;
		}

		Shipment InsertParentConsol(Shipment shipmentData)
		{
			foreach (var parentConsolBO in parentConsols)
			{
				shipmentData = InsertParentConsol(parentConsolBO, shipmentData);
			}

			shipmentData = InsertParentConsol(currentConsol, shipmentData);

			return shipmentData;
		}

		Shipment InsertParentConsol(CFSLoadListConsol parentConsolBO, Shipment shipmentData)
		{
			using (writeManager.UseNewListForDuplicatePKCheck())
			{
				var writer = new CFSLoadListConsolDataObjectWriter(writeManager, false, linkManager);
				var parentDataObject = writer.GetDataObject(parentConsolBO);

				parentDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>(new[] { shipmentData }));
				return parentDataObject;
			}
		}

		List<CFSShipment> FindParentShipments(CFSShipment shipmentBO)
		{
			var result = new List<CFSShipment>();

			if (!shipmentBO.JS_JS_ColoadMasterShipmentForBinding.IsEmpty)
			{
				var parentShipmentBO = shipmentBO.Factory.Load<CFSShipment>(shipmentBO.JS_JS_ColoadMasterShipmentForBinding);
				if (parentShipmentBO != null && !result.Contains(parentShipmentBO))
				{
					result.Add(parentShipmentBO);

					if (!IsUsingUniversalSchemaVersion2012)
					{
						result.AddRange(FindParentShipments(parentShipmentBO));
					}
				}
			}

			return result;
		}

		CFSLoadListConsol FindCurrentConsol(CFSShipment shipmentBO)
		{
			if (IsUsingUniversalSchemaVersion2012)
			{
				return (CFSLoadListConsol)shipmentBO.DepartureConsol;
			}

			var topLevelParentShipment = parentShipments.FirstOrDefault(shipment => shipment.JS_JS_ColoadMasterShipmentForBinding.IsEmpty);

			var topLevelShipment = topLevelParentShipment ?? shipmentBO;

			if (topLevelShipment.Consols.Count == 0)
			{
				return null;
			}

			if (topLevelShipment.Consols.Count == 1)
			{
				return topLevelShipment.Consols[0];
			}

			return FindConsolUsingExplicitRecipientRoleType(topLevelShipment) ??
				   FindConsolUsingTriggeringEvent(topLevelShipment) ??
				   FindDefaultConsol(topLevelShipment);
		}

		List<CFSLoadListConsol> FindParentConsols(CFSShipment shipmentBO)
		{
			var result = new List<CFSLoadListConsol>();

			if (IsUsingUniversalSchemaVersion2012 && parentShipments.Any())
			{
				return result;
			}

			foreach (var consol in shipmentBO.Consols.Cast<CFSLoadListConsol>())
			{
				var shouldBeAdded = consolToExclude == null || consolToExclude.PK != consol.PK;

				if (!IsUsingUniversalSchemaVersion2012)
				{
					shouldBeAdded &= currentConsol == null || currentConsol.PK != consol.PK;
				}

				if (shouldBeAdded)
				{
					result.Add(consol);
				}
			}

			return result;
		}

		CFSLoadListConsol FindConsolUsingExplicitRecipientRoleType(CFSShipment shipmentBO)
		{
			var recipientRoleDetails = writeManager.Action.RecipientRoleDetails;
			var isPickup = recipientRoleDetails != null && recipientRoleDetails.Any(r => r.Type == RecipientRoleType.PCA);
			if (isPickup)
			{
				return shipmentBO.Consols.GetEarliestConsol();
			}

			var isDelivery = recipientRoleDetails != null && recipientRoleDetails.Any(r => r.Type == RecipientRoleType.DCA);
			return isDelivery ? shipmentBO.Consols.GetLatestConsol() : null;
		}

		CFSLoadListConsol FindConsolUsingTriggeringEvent(CFSShipment shipmentBO)
		{
			var triggeringEvent = writeManager?.Action?.TriggeringEvent;
			if (triggeringEvent != null)
			{
				return (CFSLoadListConsol)new ConsolByEventMatcher().MatchConsolByEvent(shipmentBO, triggeringEvent);
			}

			return null;
		}

		CFSLoadListConsol FindDefaultConsol(CFSShipment shipmentBO)
		{
			return shipmentBO.Consols.GetEarliestConsol();
		}

		bool IsUsingUniversalSchemaVersion2012 => writeManager.Schema == UniversalXmlSchema.Version_2012_11_DO_NOT_USE;
	}
}
