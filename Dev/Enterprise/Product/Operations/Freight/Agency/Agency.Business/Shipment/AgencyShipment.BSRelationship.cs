using System;
using System.Diagnostics;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	partial class AgencyShipment : IBuyerSupplierRelationshipConsumer
	{
		public BuyerSupplierLinksHelper<AgencyShipment> BuyerSupplierLinksHelper
		{
			[DebuggerStepThrough]
			get { return buyerSupplierLinksHelper; }
		}

		#region IBuyerSupplierRelationshipConsumer Members

		ZBool IBuyerSupplierRelationshipConsumer.ShouldRestorePickupDeliveryAndNotifyPartyAddress
		{
			get { return true; }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ZString IBuyerSupplierRelationshipConsumer.Origin
		{
			get { return JS_RL_NKOrigin; }
			set { JS_RL_NKOrigin = value; }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ZString IBuyerSupplierRelationshipConsumer.Destination
		{
			get { return JS_RL_NKDestination; }
			set { JS_RL_NKDestination = value; }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ZString IBuyerSupplierRelationshipConsumer.LoadPort
		{
			get { return JS_NKLoadPort; }
			set
			{
				if (Sailing == null)
				{
					JS_NKLoadPort = value;
				}
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ZString IBuyerSupplierRelationshipConsumer.DischargePort
		{
			get { return JS_NKDischargePort; }
			set
			{
				if (Sailing == null)
				{
					JS_NKDischargePort = value;
				}
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ZString IBuyerSupplierRelationshipConsumer.ContainerMode
		{
			get { return JS_PackingMode; }
			set { JS_PackingMode = value; }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ZString IBuyerSupplierRelationshipConsumer.TransportMode
		{
			get { return Constants.TransportModes.Sea; }
			set { }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ZString IBuyerSupplierRelationshipConsumer.ServiceLevel
		{
			get { return JS_RS_NKServiceLevel; }
			set { JS_RS_NKServiceLevel = value; }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ZGuid IBuyerSupplierRelationshipConsumer.PickupCartageCoPK
		{
			get { return ZGuid.Empty; }
			set { }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ZGuid IBuyerSupplierRelationshipConsumer.ShippingLinePK
		{
			get { return ZGuid.Empty; }
			set { }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ZGuid IBuyerSupplierRelationshipConsumer.DeliveryCartageCoPK
		{
			get { return ZGuid.Empty; }
			set { }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ZGuid IBuyerSupplierRelationshipConsumer.ImportBrokerPK
		{
			get { return ZGuid.Empty; }
			set { }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ZGuid IBuyerSupplierRelationshipConsumer.ReceivingAgentPK
		{
			get { return ZGuid.Empty; }
			set { }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ZGuid IBuyerSupplierRelationshipConsumer.SendingAgentPK
		{
			get { return ZGuid.Empty; }
			set { }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ZString IBuyerSupplierRelationshipConsumer.GoodsCurrency
		{
			get { return ""; }
			set { }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ZString IBuyerSupplierRelationshipConsumer.GoodsDescription
		{
			get { return JS_GoodsDescription; }
			set { JS_GoodsDescription = value; }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ZBool IBuyerSupplierRelationshipConsumer.ShouldPromptToSaveBuyerSupplierRelationship
		{
			get { return Env.Registry.PromptToSaveBuyerSupplier; }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ZByte IBuyerSupplierRelationshipConsumer.NoOriginalBills
		{
			get { return JS_NoOriginalBills; }
			set { JS_NoOriginalBills = value; }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ZByte IBuyerSupplierRelationshipConsumer.NoCopyBills
		{
			get { return JS_NoCopyBills; }
			set { JS_NoCopyBills = value; }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ZString IBuyerSupplierRelationshipConsumer.PaymentTerms
		{
			get { return JS_INCO; }
			set
			{
				if (Lookups.JS_INCO_List.ContainsCode(value))
				{
					JS_INCO = value;
				}
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ZBool IBuyerSupplierRelationshipConsumer.ShouldRestorePaymentTerm
		{
			get { return true; }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ZBool IBuyerSupplierRelationshipConsumer.ShouldRestoreHandlingInformation
		{
			get { return true; }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ZString IBuyerSupplierRelationshipConsumer.ReleaseType
		{
			get { return JS_ReleaseType; }
		}

		void IBuyerSupplierRelationshipConsumer.RestoreImportBrokerFallback()
		{
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		OrgHeader IBuyerSupplierRelationshipConsumer.Consignor
		{
			get { return Consignor; }
			set { ConsignorPK = value == null ? ZGuid.Empty : value.PK; }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		OrgHeader IBuyerSupplierRelationshipConsumer.Consignee
		{
			get { return Consignee; }
			set { ConsigneePK = value == null ? ZGuid.Empty : value.PK; }
		}

		void IBuyerSupplierRelationshipConsumer.RestoreGoodsCurrencyFallback()
		{
			SetDefaultGoodsCurrency();
		}

		void IBuyerSupplierRelationshipConsumer.RestoreReceivingAgentFallback()
		{
		}

		void IBuyerSupplierRelationshipConsumer.RestoreSendingAgentFallback()
		{
		}

		void IBuyerSupplierRelationshipConsumer.RestoreServiceLevelFallback()
		{
			if (!fIsImportingData)
			{
				SetServiceLevel();
			}
		}

		void IBuyerSupplierRelationshipConsumer.RestoreNumberOfBillsWithFallback()
		{
			DefaultNumberOfBillsWithFallback(this.JS_ReleaseType);
		}

		event EventHandler IBuyerSupplierRelationshipConsumer.ModesChanged
		{
			add { JS_PackingModeInfo.ValueChanged += value; }
			remove { JS_PackingModeInfo.ValueChanged -= value; }
		}

		event EventHandler IBuyerSupplierRelationshipConsumer.ConsignorChanged
		{
			add { ConsignorChanged += value; }
			remove { ConsigneeChanged -= value; }
		}

		event EventHandler IBuyerSupplierRelationshipConsumer.ConsigneeChanged
		{
			add { ConsigneeChanged += value; }
			remove { ConsigneeChanged -= value; }
		}

		void IBuyerSupplierRelationshipConsumer.RestoreEFreightStatusFallback(ZString defaultStatus)
		{
		}

		ZBool IBuyerSupplierRelationshipConsumer.PreventBuyerSupplierRelationships
		{
			get { return false; }
		}

		ZBool IBuyerSupplierRelationshipConsumer.ShouldDefaultContainerModeAndIsContainerised(ZString containerMode) => false;

		#endregion

		#region Implementation

		void SetupBuyerSupplierLinkHelper()
		{
			buyerSupplierLinksHelper = new BuyerSupplierLinksHelper<AgencyShipment>(this);
			buyerSupplierLinksHelper.Register();
		}

		BuyerSupplierLinksHelper<AgencyShipment> buyerSupplierLinksHelper;

		void RaiseConsignorDocumentaryAddressChanged()
		{
			if (ConsignorChanged != null)
			{
				ConsignorChanged(this, EventArgs.Empty);
			}
		}

		void RaiseConsigneeDocumentaryAddressChanged()
		{
			if (ConsigneeChanged != null)
			{
				ConsigneeChanged(this, EventArgs.Empty);
			}
		}

		event EventHandler ConsignorChanged;
		event EventHandler ConsigneeChanged;

		#endregion
	}
}
