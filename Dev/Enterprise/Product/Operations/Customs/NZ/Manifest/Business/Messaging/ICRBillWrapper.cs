using System;
using System.Collections.Generic;
using System.Linq;
#if NETFRAMEWORK 
using CargoWise.Common;
#endif
using CargoWise.Types;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Manifest.Business
{
	public class ICRBillWrapper : IICRConsignment
	{
		public ICRBillWrapper(AsycudaBill bill)
		{
			this.bill = CargoWise.Common.Argument.NotNull(bill, "bill cannot be null");
		}
		readonly AsycudaBill bill;

		ZBool IICRConsignment.IsConsolidation => false;

		ZBool IICRConsignment.WriteOffRequest => false;

		ZBool IICRConsignment.IsLinkEmptyContainer => false;

		ITranshipmentDetails IICRConsignment.TranshipmentDetails => null;

		ZInt IICRConsignment.SequenceNumber => 0;

		ZDecimal IICRConsignment.ConsignmentValueInNZD => ZDecimal.Zero;

		IEnumerable<ZString> IICRConsignment.Permits => Enumerable.Empty<ZString>();

		ZString IICRConsignment.MasterBill => bill.Header?.MasterBill.ABL_BillNumber ?? ZString.Empty;

		IPartyInformation IICRConsignment.Consignee => consignee ?? (consignee = new PartyInformationWrapper(bill.ABL_ConsigneeName,
																											bill.ABL_ConsigneeCity,
																											bill.ABL_RN_NKConsigneeCountry,
																											bill.ABL_ConsigneeState,
																											bill.ABL_ConsigneeStreet1,
																											bill.ABL_ConsigneeStreet2,
																											bill.ABL_ConsigneePostcode,
																											bill.ABL_ConsigneePhone,
																											bill.Consignee?.Header));
		IPartyInformation consignee;

		IPartyInformation IICRConsignment.Consignor => consignor ?? (consignor = new PartyInformationWrapper(bill.ABL_ShipperName,
																											bill.ABL_ShipperCity,
																											bill.ABL_RN_NKShipperCountry,
																											bill.ABL_ShipperState,
																											bill.ABL_ShipperStreet1,
																											bill.ABL_ShipperStreet2,
																											bill.ABL_ShipperPostcode,
																											bill.ABL_ShipperPhone,
																											bill.Shipper?.Header));
		IPartyInformation consignor;

		IPartyInformation IICRConsignment.NotifyParty => notifyParty ?? (notifyParty = new PartyInformationWrapper(bill.ABL_NotifyPartyName,
																											bill.ABL_NotifyPartyCity,
																											bill.ABL_RN_NKNotifyPartyCountry,
																											bill.ABL_NotifyPartyState,
																											bill.ABL_NotifyPartyStreet1,
																											bill.ABL_NotifyPartyStreet2,
																											bill.ABL_NotifyPartyPostcode,
																											bill.ABL_NotifyPartyPhone,
																											bill.NotifyParty?.Header));
		PartyInformationWrapper notifyParty;

		ZString IICRConsignment.FreightPaymentMethod => bill.ABL_PrepaidCollect;

		ZString IICRConsignment.PortOfOrigin => bill.ABL_RL_NKPortOfLoading;

		// TODO: Extend ManifestHeader to provide GenAddOnColumn location
		ZString IICRConsignment.GoodsLocation => ZString.Empty;

		ZString IICRConsignment.PortOfLoading => bill.Header?.AMA_RL_NKPortOfLoading ?? ZString.Empty;

		ZString IICRConsignment.BillNumber => bill.ABL_BillNumber;

		ZString IICRConsignment.BillType
		{
			get
			{
				var result = ZString.Empty;
				if (bill.IsAir)
				{
					result = NZ.TradeSingleWindow.BillTypeList.Codes.HWB;
				}
				else if (bill.IsSea)
				{
					result = NZ.TradeSingleWindow.BillTypeList.Codes.BM;
				}
				return result;
			}
		}

		IOrganisationSimple IICRConsignment.Deconsolidator => deconsolidator ?? (deconsolidator = new OrgHeaderWrapper(GlbCompany.CurrentCompany.OrgProxy));
		IOrganisationSimple deconsolidator;

		ZBool IICRConsignment.HasContainers => DistinctContainers.Any();

		IEnumerable<ITransportEquipment> IICRConsignment.Containers
		{
			get
			{
				if (bill.IsSea)
				{
					foreach (var container in DistinctContainers)
					{
						yield return new AsycudaContainerWrapper(container);
					}
				}
			}
		}

		IEnumerable<AsycudaContainer> DistinctContainers => distinctContainers ?? (distinctContainers = bill.Packs.Cast<AsycudaPack>().Select(x => x.Container).Where(x => x != null).DistinctBy(x => x).ToArray());
		AsycudaContainer[] distinctContainers;

		ZString IICRConsignment.PortOfDischarge => bill.Header?.AMA_RL_NKPortOfDischarge ?? ZString.Empty;

		ZString IICRConsignment.HandlingInformation => ZString.Empty;

		ZString IICRConsignment.MPIAccountDetails => ZString.Empty;

		IEnumerable<IICRConsignmentItem> IICRConsignment.ConsignmentItems
		{
			get
			{
				foreach (AsycudaPack pack in bill.Packs)
				{
					yield return new ICRPackWrapper(pack);
				}
			}
		}

		IOrganisation IICRConsignment.DeliverToParty => null;

		IEnumerable<IOrganisationSimple> IICRConsignment.DeliveryNotifyParties => Enumerable.Empty<IOrganisationSimple>();

		IEnumerable<ZString> IICRConsignment.NotifyPartyCodes => Enumerable.Empty<ZString>();

		IEnumerable<ZString> IICRConsignment.TranshipmentPorts => Enumerable.Empty<ZString>();

		// Packing Location not yet implemented in ASYCUDA
		IEnumerable<IOrganisation> IICRConsignment.ContainerPackingLocations => Enumerable.Empty<IOrganisation>();

		// MAF not yet implemented in ASYCUDA
		ZBool IICRConsignment.MAFContainerDeclaration => false;
		IEnumerable<ZString> IICRConsignment.MAFContainerStatements => Enumerable.Empty<ZString>();
		IEnumerable<ZString> IICRConsignment.MPIApprovedSystemNumbers => Enumerable.Empty<ZString>();

		ZString IICRConsignment.IsGSTPrePaid => ZString.Empty;

		ZString IICRConsignment.VendorIdentifier => ZString.Empty;

		ZString IICRConsignment.ApprovedTransitionalFacilityCode => ZString.Empty;

		IEnumerable<ITSWAttachment> IICRConsignment.SupportingDocuments => Array.Empty<ITSWAttachment>();
	}
}
