using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.CFS.DataTransfer.Universal
{
	class CFSShipmentDataObjectReader : BaseShipmentDataObjectReader<CFSShipment>
	{
		public CFSShipmentDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ChildShipmentsParent parent, IContainerLinkManager<CFSLoadListConsol> linkManager, IUniversalFreightHelper helper = null)
			: base(dataObject, logger, factory, helper ?? new UniversalCFSHelper())
		{
			this.parent = parent;
			this.linkManager = linkManager;
		}

		readonly ChildShipmentsParent parent;
		IContainerLinkManager<CFSLoadListConsol> linkManager;

		public override DataContextType DataContextType
		{
			get { return DataContextType.CFSShipment; }
		}

		protected override IMatchingBusinessEntityFinder<CFSShipment> GetCombinedReferenceMatcher()
		{
			var references = new ShipmentReferences();

			if (dataObject.TransportMode.GetCodeAsUpperCase() == Constants.TransportModes.Air)
			{
				references.HAWBNumber = dataObject.WayBillNumber.GetValueOrDefault();
			}
			else
			{
				references.HBOLNumber = dataObject.WayBillNumber.GetValueOrDefault();
			}

			references.InterimReceipt = dataObject.InterimReceiptNumber.GetValueOrDefault();
			references.OriginUNLOCO = dataObject.PortOfOrigin.GetUNLOCOAsUpperCase(factory.BOFactory);
			references.DestinationUNLOCO = dataObject.PortOfDestination.GetUNLOCOAsUpperCase(factory.BOFactory);

			return new ShipmentMatcher<CFSShipment>(factory.BOFactory, references, logger, helper);
		}

		protected override CFSShipment GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			var wayBillNumber = dataObject.WayBillNumber.GetValueOrDefault();

			if (parent != null
				&& dataObject.WayBillType.GetCodeAsUpperCase() == WayBillTypeList.Codes.House
				&& !wayBillNumber.IsEmpty)
			{
				return parent.Shipments.FirstOrDefault(x => x.JS_HouseBill == wayBillNumber);
			}

			return null;
		}

		protected override void PopulateBusinessObject(CFSShipment shipmentBO)
		{
			InitializeLinkManager(shipmentBO);

			AttachToParentConsol(shipmentBO);

			SetValue(shipmentBO, JobShipmentSchema.JS_ShipmentType, dataObject.ShipmentType);
			SetValue(shipmentBO, JobShipmentSchema.JS_TransportMode, dataObject.TransportMode);
			SetValue(shipmentBO, JobShipmentSchema.JS_HouseBill, dataObject.WayBillNumber);
			SetValue(shipmentBO, JobShipmentSchema.JS_RL_NKOrigin, dataObject.PortOfOrigin);
			SetValue(shipmentBO, JobShipmentSchema.JS_RL_NKDestination, dataObject.PortOfDestination);
			SetValue(shipmentBO, JobShipmentSchema.JS_ConsolReference, dataObject.AgentsReference);
			SetValue(shipmentBO, JobShipmentSchema.JS_InterimReceipt, dataObject.InterimReceiptNumber);
			SetValue(shipmentBO, JobShipmentSchema.JS_CartageWaybill, dataObject.CartageWaybillNumber);
			SetValue(shipmentBO, JobShipmentSchema.JS_GoodsDescription, dataObject.GoodsDescription);
			SetValue(shipmentBO, JobShipmentSchema.JS_RS_NKServiceLevel, dataObject.ServiceLevel);
			SetValue(shipmentBO, JobShipmentSchema.JS_WarehouseLocation, dataObject.WarehouseLocation);
			SetValue(shipmentBO, JobShipmentSchema.JS_OuterPacks, dataObject.OuterPacks);
			SetValue(shipmentBO, JobShipmentSchema.JS_F3_NKPackType, dataObject.OuterPacksPackageType);
			SetValue(shipmentBO, JobShipmentSchema.JS_ActualVolume, dataObject.TotalVolume);
			SetValue(shipmentBO, JobShipmentSchema.JS_UnitOfVolume, dataObject.TotalVolumeUnit);
			SetValue(shipmentBO, JobShipmentSchema.JS_ActualWeight, dataObject.TotalWeight);
			SetValue(shipmentBO, JobShipmentSchema.JS_UnitOfWeight, dataObject.TotalWeightUnit);
			SetValue(shipmentBO, JobShipmentSchema.JS_TranshipToOtherCFS, dataObject.TranshipToOtherCFS);

			var bookingDate = dataObject.DateCollection != null ? dataObject.DateCollection.FirstOrDefault(DateType.BookingConfirmed, false) : null;

			if (bookingDate != null)
			{
				SetValue(shipmentBO, JobShipmentSchema.JS_A_BKD, bookingDate.Value);
			}

			var entryNumber = dataObject.EntryNumberCollection != null && dataObject.EntryNumberCollection.Count == 1 ? dataObject.EntryNumberCollection[0] : null;

			if (entryNumber != null)
			{
				shipmentBO.CustomsEntryNumber = entryNumber.Number.GetValueOrDefault();
			}

			if (dataObject.LocalProcessing != null)
			{
				SetValue(shipmentBO.DocsAndCartage, JobDocsAndCartageSchema.JP_LCLAvailable, dataObject.LocalProcessing.LCLAvailable);
				SetValue(shipmentBO.DocsAndCartage, JobDocsAndCartageSchema.JP_LCLStorageCommences, dataObject.LocalProcessing.LCLStorageCommences);

				if (dataObject.LocalProcessing.AdditionalServiceCollection != null)
				{
					var additionalservicesCollectionReader = new AdditionalServiceDataObjectCollectionReader(dataObject.LocalProcessing.AdditionalServiceCollection, logger, factory, shipmentBO.DocsAndCartage);
					additionalservicesCollectionReader.ReadIntoCollection();
				}
			}

			SetOrgAddresses(shipmentBO);
			SetContainers();
			SetSubShipments(shipmentBO);
			SetPackLines(shipmentBO);
		}

		void InitializeLinkManager(CFSShipment shipmentBO)
		{
			if (linkManager == null || linkManager.Consol == null)
			{
				CFSLoadListConsol parentConsol = null;

				if (shipmentBO.Consols.Count == 0 && parent != null && parent.Consol != null)
				{
					parentConsol = parent.Consol;
				}
				else
				{
					var shouldCreateNewConsol = dataObject.ContainerCollection != null && dataObject.ContainerCollection.Count > 0;
					parentConsol = new ConsolFinder<CFSLoadListConsol>(shipmentBO).GetBestMatchingConsolForShipmentIfExistsOtherwiseAddNew(dataObject, shouldCreateNewConsol);
				}

				linkManager = new ContainerLinkManager<CFSLoadListConsol>(parentConsol);
			}
		}

		void AttachToParentConsol(CFSShipment shipmentBO)
		{
			if (parent != null)
			{
				if (parent.Consol != null)
				{
					parent.Consol.Shipments.Add(shipmentBO);
					CheckConsolShipmentsLimitNotExceeded(parent.Consol);
				}
				else if (parent.Consol == null && linkManager.Consol != null)
				{
					linkManager.Consol.Shipments.Add(shipmentBO);
					CheckConsolShipmentsLimitNotExceeded(linkManager.Consol);
				}
			}
		}

		void CheckConsolShipmentsLimitNotExceeded(CFSLoadListConsol consol)
		{
			var notification = new ShipmentsOnConsolLimitHelper(consol).CreateNotification();

			if (notification != null && notification.Type == CargoWise.ComponentModel.NotificationType.Error)
			{
				throw new DataObjectReadFailureException(notification.Message);
			}
		}

		void SetOrgAddresses(CFSShipment shipmentBO)
		{
			if (dataObject.OrganizationAddressCollection != null && dataObject.OrganizationAddressCollection.Count > 0)
			{
				var orgLookup = new Dictionary<string, OrganizationAddress>();

				foreach (var orgAddress in dataObject.OrganizationAddressCollection)
				{
					if (orgLookup.ContainsKey(orgAddress.AddressType.GetValueOrDefault()))
					{
						string errorMessage = Res.GetString("37399F55-35C0-423C-B6EF-DFEBD4E09166", "Attempted to import a CFS shipment with duplicate organization address: '{0}'.", orgAddress.AddressType.GetValueOrDefault());
						throw new DataObjectReadFailureException(errorMessage);
					}

					orgLookup.Add(orgAddress.AddressType.GetValueOrDefault(), orgAddress);
				}

				Action<string, string, bool> setOrg = (addressType, propertyName, isOrgHeaderColumn) =>
				{
					if (orgLookup.ContainsKey(addressType))
					{
						var orgAddress = orgLookup[addressType];
						var addressBO = new OrganisationDataObjectReader(orgAddress, logger, factory).GetMatched();

						if (addressBO != null)
						{
							shipmentBO[propertyName] = isOrgHeaderColumn ? addressBO.OA_OH : addressBO.PK;
						}
					}
				};

				Action<string, OrganisationTypes> setJobDocAddress = (addressType, orgType) =>
				{
					if (orgLookup.ContainsKey(addressType))
					{
						var organizationAddress = orgLookup[addressType];
						var jobDocAddress = new OrganisationDataObjectReader(organizationAddress, logger, factory).GetMatchedOrNew(shipmentBO, orgType);

						if (jobDocAddress != null)
						{
							shipmentBO.DocAddresses.Add(jobDocAddress);
						}
					}
				};

				setOrg(AddressTypes.Forwarder, CFSShipment.Schema.JS_OH_HandledOnBehalfOfForwarder, true);
				setJobDocAddress(nameof(DocAddressType.ConsignorDocumentaryAddress), OrganisationTypes.Consignor);
				setJobDocAddress(nameof(DocAddressType.ConsigneeDocumentaryAddress), OrganisationTypes.Consignee);
				setOrg(nameof(DocAddressType.LocalCartageAddress1), CFSShipment.Schema.JS_OA_CartageCoAddr, false);
				setJobDocAddress(nameof(DocAddressType.ConsignorPickupDeliveryAddress), OrganisationTypes.Consignor);
				setJobDocAddress(nameof(DocAddressType.ConsigneePickupDeliveryAddress), OrganisationTypes.Consignee);
			}
		}

		void SetContainers()
		{
			var consol = linkManager.Consol;

			if (dataObject.ContainerCollection != null && dataObject.ContainerCollection.Count > 0)
			{
				foreach (var container in dataObject.ContainerCollection)
				{
					Func<Container, CFSContainer> findContainer = containerDO =>
					{
						var containerNum = containerDO.ContainerNumber.GetValueOrDefault();
						return !containerNum.IsEmpty ? (CFSContainer)consol.Containers.FindAnyByContainerNumber(containerNum) : null;
					};

					new ContainerWithPackLinesDataObjectReader<CFSContainer, CFSLoadListConsol>(container, logger, factory, linkManager, findContainer, data => consol.Containers.AddNew()).ReadIntoBusinessObject();
				}
			}
		}

		void SetSubShipments(CFSShipment shipmentBO)
		{
			if (dataObject.SubShipmentCollection != null && dataObject.SubShipmentCollection.Count > 0)
			{
				foreach (var subShipment in dataObject.SubShipmentCollection)
				{
					var coloadShipment = new CFSShipmentDataObjectReader(subShipment, logger, factory, ChildShipmentsParent.FromShipment(shipmentBO), linkManager, helper).ReadIntoBusinessObject();

					if (coloadShipment != null && !shipmentBO.CoLoadShipments.Contains(coloadShipment))
					{
						if (coloadShipment.PK == shipmentBO.PK)
						{
							var errorMessage = Res.GetString("699c8a91-3372-4a57-af63-be69f67270dc", "Shipment ({0}) cannot import itself as a sub shipment.", coloadShipment.JobNumber);
							throw new DataObjectReadFailureException(errorMessage);
						}
						else if (coloadShipment.JS_JS_ColoadMasterShipment.IsValid && coloadShipment.JS_JS_ColoadMasterShipment != shipmentBO.PK)
						{
							var errorMessage = Res.GetString("7e9131e4-15da-4336-bc45-ff20f6487263", "Shipment ({0}) is already linked to another job. XML rejected as an invalid link would be created between CLD and STD shipments.", coloadShipment.JobNumber);
							throw new DataObjectReadFailureException(errorMessage);
						}
						else
						{
							shipmentBO.CoLoadShipments.Add(coloadShipment);
						}
					}
				}
			}
		}

		void SetPackLines(CFSShipment shipmentBO)
		{
			if (dataObject.PackingLineCollection != null && dataObject.PackingLineCollection.Count > 0)
			{
				shipmentBO.OuterPackLines.RemoveAndDeleteAll();

				foreach (var packingLine in dataObject.PackingLineCollection)
				{
					var packLineBO = new PackingLineWithContainerDataObjectReader<CFSPackLine, CFSShipment, CFSLoadListConsol>(packingLine, logger, factory, shipmentBO, linkManager).ReadIntoBusinessObject();
					shipmentBO.OuterPackLines.Add(packLineBO);
				}
			}
		}
	}
}
