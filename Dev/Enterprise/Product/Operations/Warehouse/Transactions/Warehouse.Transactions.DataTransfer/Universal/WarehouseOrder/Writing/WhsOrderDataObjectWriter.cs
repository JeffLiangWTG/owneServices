using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using Constants = Enterprise.Core.Constants;
using UniversalIncoTerm = Enterprise.UniversalDataBuss.DataObjects.Universal.IncoTerm;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class WhsOrderDataObjectWriter : WhsOrderAndReceiveDataObjectWriter<WhsOrder>
	{
		public WhsOrderDataObjectWriter(IDataWritingManager manager, ExcludeElement elementsToExclude = ExcludeElement.None, Dictionary<ZGuid, ZInt> orderLineDictionary = null)
			: base(manager)
		{
			ElementsToExclude = elementsToExclude;
			OrderLineDictionary = orderLineDictionary ?? new Dictionary<ZGuid, ZInt>();
		}

		ExcludeElement ElementsToExclude { get; }
		Dictionary<ZGuid, ZInt> OrderLineDictionary { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "no need to be translated")]
		protected override void PopulateDataObject(WhsOrder order, UniversalShipment shipmentDataObject)
		{
			base.PopulateDataObject(order, shipmentDataObject);

			var containerMode = order.WD_ContainerMode.IsEmpty ? (ZString)Constants.ContainerModes.LTL : order.WD_ContainerMode;
			shipmentDataObject.ContainerMode = ListHelper.GetWithDescription<ContainerMode>(containerMode, order.Lookups.ContainerModes);
			shipmentDataObject.ShipperCODAmount = order.WD_ShipperCODAmount;

			var incoTerm = order.WD_INCO.IsEmpty && order.Lookups.INCOTerms.ContainsCode(Constants.IncoTerms.FreeOnBoard) ? (ZString)Constants.IncoTerms.FreeOnBoard : order.WD_INCO;
			shipmentDataObject.ShipmentIncoTerm = ListHelper.GetWithDescription<UniversalIncoTerm>(incoTerm, order.Lookups.INCOTerms);
			shipmentDataObject.ShipperCODPayMethod = ListHelper.GetWithDescription<CodeDescriptionPair>(order.WD_CODPayMethod, order.Lookups.ShipperCODPaymentTypes);

			ReplaceOverFlowExceptionWithDataObjectValidationException(
				ResString.GetMultilingualString("1B6FD25A-F71A-4CE8-A163-D9A89B4F6C93", "The value entered into Units Sent on the Release attached to Order {0} has overflowed capacity. Please validate your setup and restart the process.", order.WD_DocketID),
				() => shipmentDataObject.TotalNoOfPacks = order.WD_UnitsSent.ToZInt());

			shipmentDataObject.TotalNoOfPacksPackageType = ListHelper.GetWithDescription<PackageType>(Constants.PkgUnit.Piece, order.Lookups.TotalPackTypes);

			var transportMode = order.WD_TransportMode.IsEmpty ? (ZString)Constants.TransportModes.Road : order.WD_TransportMode;
			shipmentDataObject.TransportMode = ListHelper.GetWithDescription<CodeDescriptionPair>(transportMode, order.Lookups.TransportModes);

			if (shipmentDataObject.LocalProcessing == null)
			{
				shipmentDataObject.LocalProcessing = new LocalProcessing(writeManager.WriterStrategy);
			}
			shipmentDataObject.LocalProcessing.DeliveryRequiredBy = order.WD_RequiredDate.ToZDateTime();

			var unlocos = BindToLists.GetCachedLists(order.Factory).RefUNLOCOs;
			shipmentDataObject.PortOfOrigin = ListHelper.GetWithName(order.PortOfOrigin, unlocos);
			shipmentDataObject.PortOfDestination = ListHelper.GetWithName(order.PortOfDestination, unlocos);

			shipmentDataObject.AddOrgAddress(writeManager, order.Forwarder, DocAddressType.SendingForwarderAddress);

			if (order.Warehouse != null)
			{
				shipmentDataObject.AddOrgAddress(writeManager, order.Warehouse.WarehouseAddress, DocAddressType.ConsignorPickupDeliveryAddress);
				shipmentDataObject.AddOrgAddress(writeManager, order.Warehouse.WarehouseAddress, DocAddressType.CustomsWarehouseAddress);
			}
			shipmentDataObject.IsAuthorizedToLeave = order.WD_IsAuthorisedToLeave;

			var orderDataObject = shipmentDataObject.Order;
			orderDataObject.AddPalletWeightToOrder = order.WD_AddPalletWeightToOrder;
			orderDataObject.RequiresQualityAudit = order.WD_QualityAuditRequired;
			orderDataObject.ExcludeFromTotePicking = order.WD_ExcludeFromTotePicking;
			orderDataObject.UseDirectedPackingConsolidation = order.WD_UseDirectedPackingConsolidation;
			orderDataObject.FulfillmentRule = ListHelper.GetWithDescription<CodeDescriptionPair>(order.WD_WhsOrderFulfillmentRule, order.Lookups.WhsOrderFulfillmentRules);
			orderDataObject.LocalCartageInsuranceValue = order.WD_LocalCartInsuranceCost;
			orderDataObject.RequiresPacking = order.WD_PackingAfterPickingRequired;
			orderDataObject.PickOption = ListHelper.GetWithDescription<CodeDescriptionPair>(order.WD_PickOption, order.Lookups.PickOptions);
			orderDataObject.TotalNetWeightSent = order.WD_WeightSentUserEntered;
			orderDataObject.UnitsSent = order.WD_UnitsSent;
			orderDataObject.PickPriority = order.WD_PickPriority;

			var crossDockLocation = order.CrossDockLocation;
			if (crossDockLocation != null)
			{
				orderDataObject.StagingArea = crossDockLocation.ToLocationString();
			}

			var salesChannel = order.SalesChannel;
			PopulateCarrierAccountRelatedInfo(order, shipmentDataObject, salesChannel, writeManager);
			if (salesChannel != null)
			{
				orderDataObject.SalesChannel = new CodeDescriptionPair() { Code = salesChannel.WSH_Code, Description = salesChannel.WSH_Description };
			}

			FillCollections(order, shipmentDataObject);
		}

		protected override CodeDescriptionPair GetDocketStatus(WhsOrder order)
		{
			return ListHelper.GetWithDescription<CodeDescriptionPair>(order.WarehouseOrderStatus, WhsOrderHelper.OrderStatuses);
		}

		protected override ZShort GetPallets(WhsOrder order)
		{
			return order.WD_PalletsSent;
		}

		protected override bool PopulateOuterPacksQty(WhsOrder order)
		{
			return order.WD_PalletsSent > 0 || order.WD_PackagesSent > 0 || order.PackageJob == null || order.PackageJob.Packages.Count == 0;
		}

		void FillCollections(WhsOrder whsOrderBO, UniversalShipment shipmentDataObject)
		{
			WhsOrderLineDataObjectWriter orderLineWriter = null;

			var orderDataObject = shipmentDataObject.Order;
			orderLineWriter = new WhsOrderLineDataObjectWriter(writeManager, OrderLineDictionary);
			orderDataObject.SetOrderLineCollection(() => ProcessCollection(whsOrderBO.ParentLines, orderLineWriter, CollectionContent.Complete));

			if (!ElementsToExclude.HasFlag(ExcludeElement.Packages))
			{
				var helper = new PkgPackageJobDataObjectWriterHelper(writeManager, whsOrderBO, whsOrderBO.PackageJob, orderLineWriter?.LinksDictionary);
				helper.PopulateDataObject(shipmentDataObject);

				PopulateContainers(whsOrderBO, shipmentDataObject);
			}
		}

		static void PopulateCarrierAccountRelatedInfo(WhsOrder order, UniversalShipment shipmentDataObject, WhsSalesChannel salesChannel, IDataWritingManager writeManager)
		{
			var client = order.Client;
			var orgWhsAccountAssociation = OrgWhsClientAccountAssociation.GetWhsClientAccountAssociation(order.GetTransportCo(), client, order.Warehouse, salesChannel);
			if (orgWhsAccountAssociation != null)
			{
				var carrierAccountNumber = orgWhsAccountAssociation.CarrierAccount;
				var tpcReference = order.References
					.Cast<WhsDocketReference>()
					.FirstOrDefault(r => r.WX_RefType == WarehouseAdditionalReferenceTypes.Codes.ThirdPartyCarrierAccountNumber);

				var billingType = tpcReference != null ? (ZString)CarrierBillingType.BillThirdParty : orgWhsAccountAssociation.OWC_BillingType;
				shipmentDataObject.CarrierAccountBillingType = billingType;
				shipmentDataObject.CarrierAccount = GetCarrierAccountDataObject(carrierAccountNumber);

				if (billingType == CarrierBillingType.BillThirdParty || billingType == CarrierBillingType.BillReceiver)
				{
					if (tpcReference != null)
					{
						var transportBillToOrg = order.TransportBillTo;
						AddCarrierAccountDataObject(
							shipmentDataObject,
							new CarrierAccount
							{
								AccountNumber = tpcReference.WX_Reference,
								BillToParty = transportBillToOrg?.OH_Code ?? order.TransportBillToDocAddress.CompanyName,
								CarrierAccountType = CarrierAccountType.BillPayer
							});
					}
					else
					{
						AddBillToCarrierAccountsFromOrgWhsAccountAssociation(order, shipmentDataObject, writeManager, client, orgWhsAccountAssociation, billingType);
					}
				}

				var dutyPayerCarrierAccountNumber = orgWhsAccountAssociation.DutyBillToCarrierAccount;
				if (dutyPayerCarrierAccountNumber != null)
				{
					AddCarrierAccountDataObject(
						shipmentDataObject,
						GetCarrierAccountDataObject(dutyPayerCarrierAccountNumber, billToParty: dutyPayerCarrierAccountNumber.BillToParty.OH_Code, accountType: CarrierAccountType.DutyBillPayer));
				}
			}
		}

		static void AddBillToCarrierAccountsFromOrgWhsAccountAssociation(WhsOrder order, UniversalShipment shipmentDataObject, IDataWritingManager writeManager, OrgHeader client, OrgWhsClientAccountAssociation orgWhsAccountAssociation, ZString billingType)
		{
			var payerCarrierAccountNumber = orgWhsAccountAssociation.BillToCarrierAccount;
			var billPayer = billingType == CarrierBillingType.BillReceiver
				? order.Consignee?.OH_Code ?? client.OH_Code
				: payerCarrierAccountNumber.BillToParty.OH_Code;

			AddCarrierAccountDataObject(
				shipmentDataObject,
				GetCarrierAccountDataObject(payerCarrierAccountNumber, billToParty: billPayer, accountType: CarrierAccountType.BillPayer));

			if (order.TransportBillToDocAddress.IsEmpty)
			{
				var transportBillToAddress = billingType == CarrierBillingType.BillReceiver ? order.ConsigneeAddress : payerCarrierAccountNumber.BillToParty.MainAddress;
				if (transportBillToAddress != null)
				{
					shipmentDataObject.AddOrgAddress(writeManager, transportBillToAddress, DocAddressType.TransportBillToAddress);
				}
			}
		}

		static CarrierAccount GetCarrierAccountDataObject(OrgCarrierAccount carrierAccount, string billToParty = null, string accountType = "")
		{
			var carrierAccountDO = new CarrierAccount()
			{
				AccountNumber = carrierAccount.OAN_AccountNumber,
				MerchantNumber = carrierAccount.OAN_MerchantNumber,
				DepotID = carrierAccount.OAN_DepotID
			};

			if (billToParty != null)
			{
				carrierAccountDO.BillToParty = billToParty;
			}

			if (!string.IsNullOrEmpty(accountType))
			{
				carrierAccountDO.CarrierAccountType = accountType;
			}

			return carrierAccountDO;
		}

		static void AddCarrierAccountDataObject(UniversalShipment shipmentDataObject, CarrierAccount carrierAccountDO)
		{
			shipmentDataObject.SetAdditionalCarrierAccountCollection(() => shipmentDataObject.AdditionalCarrierAccountCollection ?? new List<CarrierAccount>());
			shipmentDataObject.AdditionalCarrierAccountCollection?.Add(carrierAccountDO);
		}

		void PopulateContainers(WhsOrder whsOrderBO, UniversalShipment shipmentDataObject)
		{
			if (shipmentDataObject.ContainerCollection != null)
			{
				var containersToAdd = ProcessCollection(whsOrderBO.Containers, new WhsDocketContainerDataObjectWriter(writeManager, shipmentDataObject.ContainerCollection), CollectionContent.Complete);
				if (containersToAdd != null)
				{
					shipmentDataObject.ContainerCollection.AddRange(containersToAdd);
				}
			}
			else
			{
				shipmentDataObject.SetContainerCollection(() => ProcessCollection(whsOrderBO.Containers, new WhsDocketContainerDataObjectWriter(writeManager), CollectionContent.Complete));
			}

			if (shipmentDataObject.ContainerCollection != null)
			{
				foreach (var container in shipmentDataObject.ContainerCollection)
				{
					// If the container Mode is empty, set the container mode as the order's container mode.
					if (container.FCL_LCL_AIR.GetCodeAsUpperCase().IsEmpty)
					{
						container.FCL_LCL_AIR = shipmentDataObject.ContainerMode;
					}
				}
			}
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.WarehouseOrder;
		}

		protected override bool PopulateJobDocAddressIsResidential => true;
	}
}
