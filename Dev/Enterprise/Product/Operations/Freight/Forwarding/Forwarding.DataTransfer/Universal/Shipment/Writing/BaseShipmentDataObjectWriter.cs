using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Freight.Forwarding.DataTransfer.ForwardingPkgPackageDataObjectWriter;
using static Enterprise.Packing.DataTransfer.Universal.PkgPackageJobDataObjectWriterHelper;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using UniversalIncoTerm = Enterprise.UniversalDataBuss.DataObjects.Universal.IncoTerm;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public abstract class BaseShipmentDataObjectWriter : TopLevelDataObjectWriter<ForwardingShipment, UniversalShipment>
	{
		protected BaseShipmentDataObjectWriter(IDataWritingManager manager) : base(manager) { }

		protected virtual IContainerLinkManager<ForwardingConsol> GetContainerLinkManager()
		{
			return new ContainerLinkManager<ForwardingConsol>(null);
		}

		protected virtual IOrderLineLinkManager OrderLineLinkManager
		{
			get
			{
				return orderLineLinkManager ?? (orderLineLinkManager = new OrderLineLinkManager());
			}
		}
		IOrderLineLinkManager orderLineLinkManager;

		protected virtual PackLineLinkManager PackLineLinkManager
		{
			get { return null; }
		}

		protected virtual DocAddressType ExportReceivingDepotAddressType
		{
			get
			{
				return DocAddressType.DepartureCFSAddress;
			}
		}

		protected override void PopulateDataObject(ForwardingShipment shipmentBO, UniversalShipment shipmentData)
		{
			var listCache = BindToLists.GetCachedLists(shipmentBO.Factory);

			PopulateShipmentData(shipmentData, shipmentBO, listCache);

			var containerLinkManager = GetContainerLinkManager();

			if (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.Value && (writeManager.HasRecipientRoleDetail(RecipientRoleType.DTW) || writeManager.HasRecipientRoleDetail(RecipientRoleType.ATW)))
			{
				try
				{
					shipmentData.SetPackingLineCollection(() =>
					{
						var packingLineDataObjectWriter = GetPackLineDataObjectWriter(containerLinkManager, listCache, shipmentBO);
						var packLineList = ProcessCollection(GetForwardingPackLines(shipmentBO, excludePackLinesWithPackages: true), packingLineDataObjectWriter, CollectionContent.Complete) ?? new DataObjectList<PackingLine>() { Content = CollectionContent.Complete };
						var packagesList = ProcessPackLinesWithPackages(GetPackLinesWithPackages(shipmentBO), GetPkgPackageDataObjectWriter(containerLinkManager, packingLineDataObjectWriter.PackLineLinkManager?.LastLink ?? 1), packingLineDataObjectWriter) ?? new DataObjectList<PackingLine>() { Content = CollectionContent.Complete };
						packLineList.AddRange(packagesList);
						return packLineList;
					});
				}
				catch (DuplicatePackageException ex)
				{
					ReportDuplicatePackageException(ex, shipmentBO);
				}
			}
			else
			{
				shipmentData.SetPackingLineCollection(() => ProcessCollection(GetForwardingPackLines(shipmentBO), GetPackLineDataObjectWriter(containerLinkManager, listCache, shipmentBO), CollectionContent.Complete)
					?? new DataObjectList<PackingLine>() { Content = CollectionContent.Complete });
			}

			PopulateOrganizations(shipmentBO, shipmentData);
			PopulateDates(shipmentBO, shipmentData);

			shipmentData.SetEntryNumberCollection(() => ProcessCollection(shipmentBO.CusEntryNumbers, new EntryNumberDataObjectWriter(writeManager)));
			shipmentData.SetAdditionalReferenceCollection(() => ProcessCollection(shipmentBO.Numbers, new AdditionalReferenceDataObjectWriter(writeManager), CollectionContent.Complete));

			shipmentBO.Factory.SetContext(BusinessContext.NonAccountingCode);
		}

		void ReportDuplicatePackageException(DuplicatePackageException ex, ForwardingShipment shipmentBO)
		{
			string PackageToDescription(PkgPackage package)
			{
				var result = $"PK: {package.PK} ID: {package.KP_PackageID} Edit Time: {package.KP_SystemLastEditTimeUtc}";
				var innerPackages = package.HandlingUnitPackedPackages.Any() ? package.HandlingUnitPackedPackages : package.Packages;
				if (innerPackages.Any())
				{
					result += $" Inner Packages: ({string.Join(", ", innerPackages.Select(PackageToDescription))})";
				}
				return result;
			}

			var packLineDescriptions = shipmentBO.OuterPackLines.Select(packLine =>
				$"Pack line PK: {packLine.PK} ID: {packLine.JL_PackLineId} Packages: {string.Join(", ", packLine.PkgPackageCollection.Select(PackageToDescription))}"
			);
			var errorDescription =
$@"Cannot add duplicate package {ex.Package.PK} to dictionary in PkgPackageJobDataObjectWriterHelper.

Shipment PK: {shipmentBO.PK} ID: {shipmentBO.JS_UniqueConsignRef} Create Time: {shipmentBO.JS_SystemCreateTimeUtc}
Shipment contains pack lines and packages:
{string.Join("\r\n", packLineDescriptions)}";
			ErrorReporter.ReportOnce(errorDescription, ex);
		}

		IEnumerable GetForwardingPackLines(ForwardingShipment shipmentBO, bool excludePackLinesWithPackages = false)
		{
			var packLines = GetAllPackingLinesIncludeCoLoad(shipmentBO);
			var isTransit = writeManager.HasRecipientRoleDetail(RecipientRoleType.DTW) || writeManager.HasRecipientRoleDetail(RecipientRoleType.ATW);
			if (!isTransit)
			{
				return packLines;
			}

			return GetPackLinesNotExcluded(packLines).Where(x => !excludePackLinesWithPackages || x.PkgPackageCollection.Count == 0);
		}

		IEnumerable<ForwardingPackLine> GetPackLinesNotExcluded(IEnumerable<ForwardingPackLine> packLines)
		{
			return writeManager.HasRecipientRoleDetail(RecipientRoleType.DTW) ? packLines.Where(x => !x.JL_DepartureTransitWarehouseExcluded) : packLines;
		}

		protected virtual IEnumerable<ForwardingPackLine> GetAllPackingLinesIncludeCoLoad(ForwardingShipment shipmentBO)
		{
			var packLines = new List<ForwardingPackLine>();
			shipmentBO.OuterPackLines.CopyToList(packLines);
			return packLines;
		}

		IEnumerable<ForwardingPackLine> GetPackLinesWithPackages(ForwardingShipment shipmentBO)
		{
			var packLines = new List<ForwardingPackLine>();
			shipmentBO.OuterPackLines.CopyToList(packLines);
			return GetPackLinesNotExcluded(packLines).Where(x => x.PkgPackageCollection.Count > 0);
		}

		static IEnumerable<PkgPackageWrapper> GetPackageWrappers(ForwardingPackLine packLine)
		{
			return packLine?.PkgPackageCollection.Select(pkg => new PkgPackageWrapper
			{
				Package = pkg,
				PackLine = packLine
			}) ?? Enumerable.Empty<PkgPackageWrapper>();
		}

		IEnumerable<PackingLine> ProcessPackLinesWithPackages(IEnumerable<ForwardingPackLine> packLines, ForwardingPkgPackageDataObjectWriter packageDataObjectWriter, ForwardingPackingLineDataObjectWriter packingLineDataObjectWriter)
		{
			var packingLineList = new DataObjectList<PackingLine>() { Content = CollectionContent.Complete };
			foreach (var packLine in packLines)
			{
				var packingLines = ProcessCollection(GetPackageWrappers(packLine), packageDataObjectWriter);
				if (packingLines == null || packingLines.Count == 0)
				{
					continue;
				}

				if (packingLines.All(p => !p.HasInnerPackingLines()))
				{
					foreach (var innerPackLine in packLine.InnerPackLines)
					{
						var innerPackingLines = packingLines.Select(packingLine => packingLineDataObjectWriter.PopulateInnerPackLine(innerPackLine, packingLine)).WhereNotNull().ToList();
						if (innerPackingLines.Count == 0)
						{
							continue;
						}

						var averagePackQty = innerPackLine.JL_PackageCount / innerPackingLines.Count;
						foreach (var innerPackingLine in innerPackingLines)
						{
							innerPackingLine.PackQty = averagePackQty;
						}
						innerPackingLines.Last().PackQty += innerPackLine.JL_PackageCount % innerPackingLines.Count;
					}
				}

				packingLineList.AddRange(packingLines);
			}
			return packingLineList;
		}

		void PopulateOrganizations(ForwardingShipment shipmentBO, UniversalShipment shipmentData)
		{
			var addresses = new List<JobDocAddress>
			{
				shipmentBO.ConsignorDocumentaryAddress,
				shipmentBO.ConsignorPickupAddress,
				shipmentBO.ConsigneeDocumentaryAddress,
				shipmentBO.ConsigneeDeliveryAddress,
				shipmentBO.BuyerDocAddress,
				shipmentBO.InsuredByDocAddress,
				shipmentBO.AssuredPartyDocAddress,
				shipmentBO.ClaimsPayableByDocAddress,
				shipmentBO.SurveyReportPartyDocAddress,
				shipmentBO.NotifyPartyDocumentaryAddress,
				shipmentBO.NotifyParty2DocumentaryAddress,
				shipmentBO.NotifyParty3DocumentaryAddress,
				shipmentBO.ControllingCustomerAddress,
				shipmentBO.PickupAgentDocumentaryAddress,
				shipmentBO.ManufacturerDocAddress,
				shipmentBO.ControllingAgentDocumentaryAddress,
				shipmentBO.BookingPartyDocumentaryAddress,
				shipmentBO.HouseBillIssuingPartyDocumentaryAddress,
				shipmentBO.SupplierDocAddress
			};

			var warehouseAddress = shipmentBO.DocAddresses.FindByDocAddressType(DocAddressType.Warehouse);
			if (warehouseAddress != null)
			{
				addresses.Add(warehouseAddress);
			}
			var warehouseClientAddress = shipmentBO.DocAddresses.FindByDocAddressType(DocAddressType.WarehouseClient);
			if (warehouseClientAddress != null)
			{
				addresses.Add(warehouseClientAddress);
			}
			shipmentData.SetOrganizationAddressCollection(() => ProcessCollection(addresses, new JobDocAddressDataObjectWriter(writeManager)));

			var jobHeader = shipmentBO.ShipmentJobHeader;
			if (jobHeader != null)
			{
				shipmentData.AddOrgAddress(writeManager, jobHeader.LocalChargesAddr, AddressTypes.SendersLocalClient);
				shipmentData.AddOrgAddress(writeManager, jobHeader.AgentCollectAddr, AddressTypes.SendersOverseasAgent);
			}

			shipmentData.AddOrgAddress(writeManager, shipmentBO.ExportBroker, DocAddressType.ExportBroker);
			shipmentData.AddOrgAddress(writeManager, shipmentBO.ImportBroker, DocAddressType.ImportBroker);
			shipmentData.AddOrgAddress(writeManager, shipmentBO.ExportReceivingDepot, ExportReceivingDepotAddressType);
			shipmentData.AddOrgAddress(writeManager, shipmentBO.DeliveryAgent, DocAddressType.DeliveryAgent);
			shipmentData.AddOrgAddress(writeManager, shipmentBO.DocsAndCartage.PickupCartageCoAddr, AddressTypes.PickupLocalCartage);

			if (writeManager.Schema == UniversalXmlSchema.Version_2011_11 && shipmentBO.ControllingCustomerAddress != null)
			{
				var address = shipmentData.AddOrgAddress(writeManager, shipmentBO.ControllingCustomerAddress);

				if (address != null)
				{
					address.AddressType = LegacyUniversalAddressTypes.LegacyShipmentControllingPartyAddressType;
				}
			}
		}

		void PopulateDates(ForwardingShipment shipmentBO, UniversalShipment shipmentData)
		{
			shipmentData.SetDateCollection(() =>
			{
				var dates = new List<Date>();
				dates.Add(Date.New(DateType.BookingConfirmed, ZBool.False, shipmentBO.JS_A_BKD));
				dates.Add(Date.New(DateType.Received, ZBool.False, shipmentBO.JS_A_RCV));
				dates.Add(Date.New(DateType.Departure, ZBool.True, shipmentBO.JS_E_DEP));
				dates.Add(Date.New(DateType.Arrival, ZBool.True, shipmentBO.JS_E_ARV));
				dates.Add(Date.New(DateType.DeliveryDueDate, ZBool.False, shipmentBO.JS_DeliveryDueDate));
				dates.Add(Date.New(DateType.RevisedDeliveryDueDate, ZBool.False, shipmentBO.JS_RevisedDeliveryDueDate.ToZDateTime()));

				return dates;
			});
		}

		protected virtual ForwardingPackingLineDataObjectWriter GetPackLineDataObjectWriter(IContainerLinkManager<ForwardingConsol> containerLinkManager, BindToLists listCache, ForwardingShipment sourceBO)
		{
			return new ForwardingPackingLineDataObjectWriter(containerLinkManager, OrderLineLinkManager, PackLineLinkManager, listCache, writeManager, new LineRelatedDataWriterHelper(sourceBO));
		}

		protected virtual ForwardingPkgPackageDataObjectWriter GetPkgPackageDataObjectWriter(IContainerLinkManager<ForwardingConsol> containerLinkManager, int startPacklineLink)
		{
			return new ForwardingPkgPackageDataObjectWriter(containerLinkManager, writeManager, startPacklineLink);
		}

		void PopulateShipmentData(UniversalShipment shipmentData, ForwardingShipment shipmentBO, BindToLists listCache)
		{
			shipmentData.ActualChargeable = shipmentBO.JS_ActualChargeable;
			shipmentData.AdditionalTerms = shipmentBO.JS_AdditionalTerms;

			shipmentData.BookingConfirmationReference = shipmentBO.JS_BookingReference;
			shipmentData.CFSReference = shipmentBO.JS_CFSReference;
			shipmentData.ContainerMode = ListHelper.GetWithDescription<ContainerMode>(shipmentBO.JS_PackingMode, shipmentBO.Lookups.JS_PackingMode_List);
			shipmentData.ContainerCount = shipmentBO.Containers.Count();
			shipmentData.FreightRate = shipmentBO.JS_UnitFreightRate;
			shipmentData.FreightRateCurrency = ListHelper.GetWithDescription<Currency>(shipmentBO.JS_RX_NKFrtRateCurrency, shipmentBO.Lookups.FrtRateCurrencies);
			shipmentData.GoodsDescription = shipmentBO.JS_GoodsDescription;
			shipmentData.GoodsValue = shipmentBO.JS_GoodsValue;
			shipmentData.GoodsValueCurrency = ListHelper.GetWithDescription<Currency>(shipmentBO.JS_RX_NKGoodsValueCurr, shipmentBO.Lookups.RefCurrency_List);
			shipmentData.InsuranceValue = shipmentBO.JS_InsuranceValue;
			shipmentData.InsuranceValueCurrency = ListHelper.GetWithDescription<Currency>(shipmentBO.JS_RX_NKInsuranceCurrency, shipmentBO.Lookups.RefCurrency_List);
			shipmentData.InterimReceiptNumber = shipmentBO.JS_InterimReceipt;
			shipmentData.IsDirectBooking = shipmentBO.JS_IsDirectBooking;
			shipmentData.IsForwardRegistered = shipmentBO.JS_IsForwardRegistered;
			shipmentData.OuterPacks = shipmentBO.JS_OuterPacks;
			shipmentData.OuterPacksPackageType = ListHelper.GetWithDescription<PackageType>(shipmentBO.JS_F3_NKPackType, shipmentBO.Lookups.JS_PackType_List);

			shipmentData.PackingOrder = shipmentBO.JS_PackingOrder;
			shipmentData.ServiceLevel = ListHelper.GetWithDescription<ServiceLevel>(shipmentBO.JS_RS_NKServiceLevel, shipmentBO.Lookups.RefServiceLevel_List);
			shipmentData.ShipmentIncoTerm = ListHelper.GetWithDescription<UniversalIncoTerm>(shipmentBO.JS_INCO, shipmentBO.Lookups.JS_INCO_List);

			shipmentData.TotalVolume = shipmentBO.JS_ActualVolume;
			shipmentData.TotalVolumeUnit = ListHelper.GetWithDescription<UnitOfVolume>(shipmentBO.JS_UnitOfVolume, listCache.VolumeUnits);
			shipmentData.TotalWeight = shipmentBO.JS_ActualWeight;
			shipmentData.TotalWeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(shipmentBO.JS_UnitOfWeight, listCache.WeightUnits);
			shipmentData.TransportMode = ListHelper.GetWithDescription<CodeDescriptionPair>(shipmentBO.JS_TransportMode, shipmentBO.Lookups.JS_TransportMode_List);

			if (!string.IsNullOrEmpty(GlbBranch.CurrentBranch?.GB_RL_NKHomePort))
			{
				shipmentData.EventBranchHomePort = ListHelper.GetWithName(GlbBranch.CurrentBranch.GB_RL_NKHomePort, shipmentBO.Lookups.RefUNLOCO_List);
			}

			shipmentData.PortOfOrigin = ListHelper.GetWithName(shipmentBO.JS_RL_NKOrigin, shipmentBO.Lookups.RefUNLOCO_List);

			shipmentData.PortOfDestination = ListHelper.GetWithName(shipmentBO.JS_RL_NKDestination, shipmentBO.Lookups.RefUNLOCO_List);

			shipmentData.WayBillNumber = shipmentBO.JS_HouseBill;
			shipmentData.WayBillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.House, new WayBillTypeList());
			shipmentData.IsCancelled = shipmentBO.JS_IsCancelled;
			shipmentData.HouseBillOfLadingType = ListHelper.GetWithDescription<CodeDescriptionPair>(shipmentBO.JS_HouseBillOfLadingType, shipmentBO.Lookups.JS_HouseBillOfLadingType_List);

			if (shipmentBO.JS_LoadingMeters > 0)
			{
				shipmentData.TotalLoadingMeters = shipmentBO.JS_LoadingMeters;
			} 
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override IEnumerable<IPropertyValue> GetUserDefinedValues(ForwardingShipment shipmentBO)
		{
			return shipmentBO.GetUserDefinedValues();
		}
	}
}

