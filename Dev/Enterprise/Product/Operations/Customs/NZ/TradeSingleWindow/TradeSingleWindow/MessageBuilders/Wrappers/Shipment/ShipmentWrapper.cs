using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders
{
	public class ShipmentWrapper : IOCRConsignment, IICRConsignment, ICREConsignment, ICurrencyConverterDataProvider
	{
		public ShipmentWrapper(ForwardingShipment shipment)
		{
			this.shipment = Argument.NotNull(shipment, "Shipment cannot be null");
		}

		readonly ForwardingShipment shipment;

		#region IOCRConsignment members

		ZString IOCRConsignment.CustomsClearanceNo
		{
			get { return shipment.CustomsEntryNumber; }
		}

		IAssociatedTransportDocument IOCRConsignment.BillNumber
		{
			get { return new BillNumberWrapper(shipment); }
		}

		#endregion

		#region IICRConsignment members

		ZBool IICRConsignment.WriteOffRequest
		{
			get { return true; }
		}

		ZBool IICRConsignment.IsLinkEmptyContainer => false;

		ITranshipmentDetails IICRConsignment.TranshipmentDetails => null;

		ZBool IICRConsignment.IsConsolidation
		{
			get { return false; }
		}

		ZInt IICRConsignment.SequenceNumber => 0;

		ZDecimal IICRConsignment.ConsignmentValueInNZD
		{
			get
			{
				var consignmentCurrency = shipment.Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, shipment.JS_RX_NKGoodsValueCurr));
				var consignmentValue = new Money(shipment.JS_GoodsValue, consignmentCurrency);
				return CurrencyConverter.ConvertExact(consignmentValue, CurrencyConverter.LocalCurrency).Amount;
			}
		}

		IEnumerable<ZString> IICRConsignment.Permits
		{
			get { yield return shipment.CustomsEntryNumber; }
		}

		ZBool IICRConsignment.MAFContainerDeclaration
		{
			get { return false; }
		}

		IEnumerable<ZString> IICRConsignment.MAFContainerStatements => Enumerable.Empty<ZString>();

		IEnumerable<ZString> IICRConsignment.MPIApprovedSystemNumbers => Enumerable.Empty<ZString>();

		ZString IICRConsignment.MasterBill
		{
			get { return shipment.JS_JK_MasterBillNum; }
		}

		IPartyInformation IICRConsignment.Consignee => new OrgHeaderWrapper(shipment.Consignee);

		IPartyInformation IICRConsignment.Consignor => new OrgHeaderWrapper(shipment.Consignor);

		IOrganisation IICRConsignment.DeliverToParty
		{
			get { return OrgHeaderWrapper.New(shipment.ConsigneeDeliveryAddress.Organisation, shipment.ConsigneeDeliveryAddress); }
		}

		ZString IICRConsignment.FreightPaymentMethod
		{
			get { return shipment.JS_INCO; }
		}

		ZString IICRConsignment.PortOfOrigin
		{
			get { return shipment.JS_RL_NKOrigin; }
		}

		ZString IICRConsignment.GoodsLocation
		{
			get { return ZString.Empty; } //TODO: GoodsLocation
		}

		ZString IICRConsignment.PortOfLoading
		{
			get { return shipment.MostInterestingTransport.JW_RL_NKLoadPort; }
		}

		IPartyInformation IICRConsignment.NotifyParty
		{
			get { return OrgHeaderWrapper.New(shipment.NotifyParty); }
		}

		IEnumerable<IOrganisationSimple> IICRConsignment.DeliveryNotifyParties => Enumerable.Empty<IOrganisationSimple>();

		IEnumerable<ZString> IICRConsignment.NotifyPartyCodes => Enumerable.Empty<ZString>();

		IEnumerable<IOrganisation> IICRConsignment.ContainerPackingLocations => Enumerable.Empty<IOrganisation>();

		IEnumerable<ZString> IICRConsignment.TranshipmentPorts
		{
			get
			{
				foreach (ZString port in shipment.CountriesOfRouting)
				{
					yield return port;
				}
			}
		}

		ZString IICRConsignment.BillNumber
		{
			get { return shipment.JS_HouseBill; }
		}

		ZString IICRConsignment.BillType
		{
			get { return BillTypeList.Codes.HWB; }
		}

		IOrganisationSimple IICRConsignment.Deconsolidator
		{
			get { return null; }    //TODO: Deconsolidator
		}

		ZBool IICRConsignment.HasContainers => shipment.Containers.Any();

		IEnumerable<ITransportEquipment> IICRConsignment.Containers
		{
			get
			{
				foreach (ForwardingContainer container in shipment.Containers)
				{
					yield return new ContainerWrapper(container);
				}
			}
		}

		ZString IICRConsignment.PortOfDischarge
		{
			get { return shipment.MostInterestingTransport.JW_RL_NKDiscPort; }
		}

		ZString IICRConsignment.HandlingInformation => ZString.Empty;

		ZString IICRConsignment.MPIAccountDetails => ZString.Empty;

		IEnumerable<IICRConsignmentItem> IICRConsignment.ConsignmentItems
		{
			get
			{
				foreach (ForwardingPackLine packLine in shipment.OuterPackLines)
				{
					yield return new PackLineWrapper(packLine);
				}
			}
		}

		ZString IICRConsignment.IsGSTPrePaid => ZString.Empty;
		ZString IICRConsignment.VendorIdentifier => ZString.Empty;
		ZString IICRConsignment.ApprovedTransitionalFacilityCode => ZString.Empty;

		IEnumerable<ITSWAttachment> IICRConsignment.SupportingDocuments => Array.Empty<ITSWAttachment>();

		#endregion

		#region ICREConsignment Implementation

		//TODO: CRE Consignment values....
		ZBool ICREConsignment.WriteOffRequest
		{
			get { return true; }    //Cargo Report Export is a WriteOffRequest
		}

		ZShort ICREConsignment.SequenceNumber => 0;

		ITranshipmentDetails ICREConsignment.TranshipmentDetails => null;

		ZString ICREConsignment.HandlingInfo
		{
			get { return ZString.Empty; } //TODO: Handling info
		}

		ZDecimal ICREConsignment.ConsignmentValueInNZD
		{
			get { return shipment.JS_GoodsValue; }
		}

		IPartyInformation ICREConsignment.Consignee => new OrgHeaderWrapper(shipment.Consignee);

		IPartyInformation ICREConsignment.Consignor => new OrgHeaderWrapper(shipment.Consignor);

		IEnumerable<ICREConsignmentItem> ICREConsignment.ConsignmentItems => Enumerable.Empty<ICREConsignmentItem>();

		IOrganisation ICREConsignment.DeliverToParty
		{
			get { return null; }    //TODO: DeliverTo
		}

		ZString ICREConsignment.FreightPaymentMethod
		{
			get { return shipment.JS_INCO; }
		}

		ZString ICREConsignment.GoodsLocation
		{
			get { return ZString.Empty; }   //TODO: where from?
		}

		ZString ICREConsignment.PortOfLoading
		{
			get { return shipment.JS_RL_NKOrigin; }
		}

		IEnumerable<IPartyInformation> ICREConsignment.NotifyParties
		{
			get { return null; }    //TODO: NotifyParties
		}

		IEnumerable<IOrganisationSimple> ICREConsignment.DeliveryNotifyParties
		{
			get { return null; }    //TODO: DeliveryNotifyParties
		}

		IEnumerable<ZString> ICREConsignment.NotifyPartyCodes => Enumerable.Empty<ZString>();

		IAssociatedTransportDocument ICREConsignment.BillNumber
		{
			get { return new BillNumberWrapper(shipment); }
		}

		IOrganisationSimple ICREConsignment.Consolidator
		{
			get { return null; }    //TODO: Consolidator
		}

		ZBool ICREConsignment.HasContainers => shipment.Containers.Any();

		IEnumerable<ITransportEquipment> ICREConsignment.Containers
		{
			get
			{
				foreach (ForwardingContainer container in shipment.Containers)
				{
					yield return new ContainerWrapper(container);
				}
			}
		}

		ZString ICREConsignment.PortOfDischarge
		{
			get { return shipment.JS_RL_NKDestination; }
		}

		#endregion

		#region CurrencyConverter

		public CurrencyConverter CurrencyConverter
		{
			get
			{
				if (fCurrencyConverter == null)
				{
					fCurrencyConverter = GetNewCurrencyConverter();
				}
				return fCurrencyConverter;
			}
		}
		CurrencyConverter fCurrencyConverter;

		protected virtual CurrencyConverter GetNewCurrencyConverter()
		{
			return new CurrencyConverterWithDataProvider(shipment.Factory, this);
		}

		GlbCompany ICurrencyConverterDataProvider.Company
		{
			get { return GlbCompany.CurrentCompany; }
		}

		ZDateTime ICurrencyConverterDataProvider.DateOfValuation
		{
			get { return ZDateTime.Today; }
		}

		int ICurrencyConverterDataProvider.MaximumDaysToFallback
		{
			get { return 7; }
		}

		ZArchitecture.Core.ExchangeRateType ICurrencyConverterDataProvider.RateType
		{
			get { return ZArchitecture.Core.ExchangeRateType.Customs; }
		}

		ZString ICurrencyConverterDataProvider.LocalCurrencyCodeOverride
		{
			get { return Enterprise.Core.Constants.CurrencyCodes.NewZealand; }
		}

		ZBool? ICurrencyConverterDataProvider.IsReciprocalOverride
		{
			get { return false; }
		}

		#endregion
	}
}
