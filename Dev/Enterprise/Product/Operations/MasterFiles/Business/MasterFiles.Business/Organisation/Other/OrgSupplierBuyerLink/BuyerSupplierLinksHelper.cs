using System;
using System.Linq;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	#region IBuyerSupplierRelationshipConsumer Interface

	/// <summary>
	/// Implement this interface on your host business object if your business object supports saving and defaulting
	/// of buyer / supplier relationships. You must also then call BuyerSupplierLinksHelper.Register() on your host.
	/// </summary>
	public interface IBuyerSupplierRelationshipConsumer
	{
		OrgHeader Consignor { get; set; }
		OrgHeader Consignee { get; set; }

		ZString TransportMode { get; set; }
		ZString ContainerMode { get; set; }

		event EventHandler ConsigneeChanged;
		event EventHandler ConsignorChanged;
		event EventHandler ModesChanged;

		JobDocAddress ConsignorPickupAddress { get; }
		JobDocAddress ConsigneeDeliveryAddress { get; }
		JobDocAddress NotifyPartyDocumentaryAddress { get; }
		ZString GoodsDescription { get; set; }
		ZString PaymentTerms { get; set; }
		ZString ServiceLevel { get; set; }
		void RestoreServiceLevelFallback();
		ZString GoodsCurrency { get; set; }
		void RestoreGoodsCurrencyFallback();
		ZString Origin { get; set; }
		ZString Destination { get; set; }
		ZString LoadPort { get; set; }
		ZString DischargePort { get; set; }
		ZString ReleaseType { get; }
		ZGuid ImportBrokerPK { get; set; }
		void RestoreImportBrokerFallback();

		ZGuid ShippingLinePK { get; set; }

		ZGuid ReceivingAgentPK { get; set; }
		void RestoreReceivingAgentFallback();

		ZGuid SendingAgentPK { get; set; }
		void RestoreSendingAgentFallback();

		ZGuid PickupCartageCoPK { get; set; }
		ZGuid DeliveryCartageCoPK { get; set; }

		ZByte NoOriginalBills { get; set; }
		ZByte NoCopyBills { get; set; }
		void RestoreNumberOfBillsWithFallback();

		ZBool IsSettingDefaultValues { get; set; }
		ZBool ShouldPromptToSaveBuyerSupplierRelationship { get; }
		ZBool ShouldRestorePaymentTerm { get; }
		ZBool ShouldRestoreHandlingInformation { get; }

		ZBool ShouldRestorePickupDeliveryAndNotifyPartyAddress { get; }

		ZBool ShouldDefaultContainerModeAndIsContainerised(ZString containerMode);

		void RestoreEFreightStatusFallback(ZString defaultStatus);

		ZBool PreventBuyerSupplierRelationships { get; }
	}

	#endregion

	public class BuyerSupplierLinksHelper<T> where T : BusinessObject, IBuyerSupplierRelationshipConsumer, ISupportDataImporting
	{
		public BuyerSupplierLinksHelper(T businessEntity)
		{
			this.BusinessEntity = businessEntity;
		}

		readonly T BusinessEntity;

		public void Register()
		{
			BusinessEntity.ModesChanged += OnModesChanged;
			BusinessEntity.ConsigneeChanged += OnConsigneeChanged;
			BusinessEntity.ConsignorChanged += OnConsignorChanged;
			BusinessEntity.Factory.Saved += OnFactoryOnSaved;
		}

		void OnFactoryOnSaved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			consignorChanged = false;
			consigneeChanged = false;
		}

		void OnConsignorChanged(object sender, EventArgs e)
		{
			consignorChanged = true;
			SetBuyerFromSupplier();
			RestoreBuyerSupplierLink();
		}

		void OnConsigneeChanged(object sender, EventArgs e)
		{
			consigneeChanged = true;
			SetSupplierFromBuyer();
			RestoreBuyerSupplierLink();
		}

		void OnModesChanged(object sender, EventArgs e)
		{
			RestoreBuyerSupplierLink();
		}

		public void Deregister()
		{
			BusinessEntity.ModesChanged -= OnModesChanged;
			BusinessEntity.ConsigneeChanged -= OnConsigneeChanged;
			BusinessEntity.ConsignorChanged -= OnConsignorChanged;
			BusinessEntity.Factory.Saved -= OnFactoryOnSaved;
		}

		bool consigneeChanged;
		bool consignorChanged;

		#region Suspenders
		// Add more property suspender as required

		#region Transport Mode Suspender
		public IDisposable SuspendTransportModeRestoration()
		{
			return new TransportModeRestorationSuspender(this);
		}

		bool IsTransportModeRestorationSuspended
		{
			get { return transportModeRestorationSuspendedIndex > 0; }
		}

		class TransportModeRestorationSuspender : IDisposable
		{
			public TransportModeRestorationSuspender(BuyerSupplierLinksHelper<T> helper)
			{
				this.helper = helper;
				helper.transportModeRestorationSuspendedIndex++;
				DisposableLeakListener.Instance.RegisterDisposable(this);
			}
			readonly BuyerSupplierLinksHelper<T> helper;

			public void Dispose()
			{
				try
				{
					this.helper.transportModeRestorationSuspendedIndex--;
				}
				finally
				{
					DisposableLeakListener.Instance.UnRegisterDisposable(this);
				}
			}
		}
		int transportModeRestorationSuspendedIndex;

		#endregion

		#region Restore Port Suspender

		public IDisposable SuspendPortRestoration()
		{
			return new PortRestorationSuspender(this);
		}

		bool IsPortRestorationSuspended
		{
			get { return portSuspendedIndex > 0; }
		}

		class PortRestorationSuspender : IDisposable
		{
			public PortRestorationSuspender(BuyerSupplierLinksHelper<T> helper)
			{
				this.helper = helper;
				helper.portSuspendedIndex++;
				DisposableLeakListener.Instance.RegisterDisposable(this);
			}
			readonly BuyerSupplierLinksHelper<T> helper;

			public void Dispose()
			{
				try
				{
					this.helper.portSuspendedIndex--;
				}
				finally
				{
					DisposableLeakListener.Instance.UnRegisterDisposable(this);
				}
			}
		}

		int portSuspendedIndex;

		#endregion

		#endregion

		#region Link

		OrgSupplierBuyerLink SupplierBuyerLink
		{
			get
			{
				if (SupplierBuyerLinkCached == null)
				{
					SupplierBuyerLinkCached = new CachedProperty<OrgSupplierBuyerLink>(Factory, delegate
						{
							return OrgSupplierBuyerLink.GetExistingOrgSupplierBuyerLink(BusinessEntity.Consignor, BusinessEntity.Consignee, BusinessEntity.Destination.Left(2));
						});
				}
				return SupplierBuyerLinkCached.Value;
			}
		}
		CachedProperty<OrgSupplierBuyerLink> SupplierBuyerLinkCached;

		OrgSupBuyLinkTrnMode SupplierBuyerLinkDetails
		{
			get
			{
				if (SupplierBuyerLinkDetailsCached == null)
				{
					SupplierBuyerLinkDetailsCached = new CachedProperty<OrgSupBuyLinkTrnMode>(Factory, delegate
						{
							return SupplierBuyerLink != null ? SupplierBuyerLink.OrgSupBuyLinkTrnModes.Find(BusinessEntity.TransportMode, BusinessEntity.ContainerMode) : null;
						});
				}
				return SupplierBuyerLinkDetailsCached.Value;
			}
		}
		CachedProperty<OrgSupBuyLinkTrnMode> SupplierBuyerLinkDetailsCached;

		public OrgHeader GetControllingCustomer()
		{
			OrgHeader controllingCustomer = null;

			if (BusinessEntity.Consignee != null && BusinessEntity.Consignor != null)
			{
				BusinessEntity.Consignee.SupplierLinks.Reload(true, true);
				var supplierLink = BusinessEntity.Consignee.SupplierLinks.Cast<OrgSupplierBuyerLink>().FirstOrDefault(link => link.OL_OH_Supplier == BusinessEntity.Consignor.PK);

				if (supplierLink != null)
				{
					Func<OrgSupBuyLinkTrnMode, bool>[] predicateOrder =
					{
						o => o.PF_TransportMode == BusinessEntity.TransportMode && o.PF_ContainerMode == BusinessEntity.ContainerMode,
						o => o.PF_TransportMode == BusinessEntity.TransportMode && o.PF_ContainerMode.IsEmpty,
						o => o.PF_TransportMode == Constants.TransportModes.All && o.PF_ContainerMode == BusinessEntity.ContainerMode,
						o => o.PF_TransportMode == Constants.TransportModes.All && o.PF_ContainerMode.IsEmpty
					};

					OrgSupBuyLinkTrnMode orgSupBuyLinkTrnMode = null;
					foreach (var pred in predicateOrder)
					{
						orgSupBuyLinkTrnMode = supplierLink.OrgSupBuyLinkTrnModes.Cast<OrgSupBuyLinkTrnMode>().FirstOrDefault(pred);
						if (orgSupBuyLinkTrnMode?.ControllingCustomer != null)
						{
							break;
						}
					}
					controllingCustomer = orgSupBuyLinkTrnMode?.ControllingCustomer ?? supplierLink.ControllingCustomer;
				}
			}

			return controllingCustomer;
		}

		BusinessObjectFactory Factory
		{
			get { return BusinessEntity.Factory; }
		}

		#endregion

		#region Save

		public bool ShouldPromptToSaveSupplierBuyerRelationship
		{
			get
			{
				return
					Env.Registry.PromptToSaveBuyerSupplier
					&& BusinessEntity.ShouldPromptToSaveBuyerSupplierRelationship
					&& !BusinessEntity.IsImportingData
					&& (consignorChanged || consigneeChanged)
					&& IsValidOrganisation(BusinessEntity.Consignee)
					&& IsValidOrganisation(BusinessEntity.Consignor)
					&& SupplierBuyerLink == null;
			}
		}

		public OrgSupplierBuyerLink AddNewBuyerSupplierLink()
		{
			return AddNewBuyerSupplierLinkGivenOrgs(BusinessEntity.Consignee, BusinessEntity.Consignor);
		}

		public OrgSupplierBuyerLink AddNewBuyerSupplierLinkGivenOrgs(OrgHeader buyer, OrgHeader supplier)
		{
			return AddNewBuyerSupplierLinkGivenOrgs(buyer, supplier, ZString.Empty);
		}

		public OrgSupplierBuyerLink AddNewBuyerSupplierLinkGivenOrgs(OrgHeader buyer, OrgHeader supplier, ZString importCountryCode)
		{
			if (IsValidOrganisation(buyer) && IsValidOrganisation(supplier))
			{
				OrgSupplierBuyerLink supplierBuyer = buyer.SupplierLinks.AddNew();
				supplierBuyer.OL_OH_Supplier = supplier.PK;

				SetDefaultImportBroker(supplierBuyer);
				SetDefaultCurrency(supplierBuyer);
				SetDefaultImporterCountry(supplierBuyer, importCountryCode);
				SetDefaultIncoterm(supplierBuyer);
				SetDefaultTransportMode(supplierBuyer);
				SetDefaultContainerMode(supplierBuyer);
				SetDefaultNoOriginalBills(supplierBuyer);
				SetDefaultNoCopyBills(supplierBuyer);
				return supplierBuyer;
			}
			return null;
		}

		bool IsValidOrganisation(OrgHeader organisation)
		{
			return organisation != null && !organisation.IsSystemDefinedOrganisation;
		}

		#region Implementation

		void SetDefaultImportBroker(OrgSupplierBuyerLink supplierBuyer)
		{
			if (IsDefaultingAllowed(OrganisationsDataRegistry.BuyerSupplierRelationshipFields.ImportBroker))
			{
				supplierBuyer.OL_OH_ImportBroker = BusinessEntity.ImportBrokerPK;
			}
			else
			{
				supplierBuyer.OL_OH_ImportBroker = ZGuid.Empty;
			}
		}

		void SetDefaultCurrency(OrgSupplierBuyerLink supplierBuyer)
		{
			if (IsDefaultingAllowed(OrganisationsDataRegistry.BuyerSupplierRelationshipFields.DefaultCurrency))
			{
				if (!BusinessEntity.GoodsCurrency.IsEmpty)
				{
					RefCurrency goodsCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, BusinessEntity.GoodsCurrency);

					if (goodsCurrency != null)
					{
						supplierBuyer.OL_RX_NKDefaultCurrency = goodsCurrency.RX_Code;
					}
				}
			}
			else
			{
				supplierBuyer.OL_RX_NKDefaultCurrency = ZString.Empty;
			}
		}

		void SetDefaultImporterCountry(OrgSupplierBuyerLink supplierBuyer, ZString importCountryCode)
		{
			if (!importCountryCode.IsEmpty)
			{
				supplierBuyer.OL_RN_NKImporterCountry = importCountryCode;
			}
			else if (!BusinessEntity.Destination.IsEmpty)
			{
				RefUNLOCO unloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, BusinessEntity.Destination);

				if (unloco != null && unloco.Country != null)
				{
					supplierBuyer.OL_RN_NKImporterCountry = unloco.Country.RN_Code;
				}
			}
		}

		void SetDefaultIncoterm(OrgSupplierBuyerLink supplierBuyer)
		{
			if (IsDefaultingAllowed(OrganisationsDataRegistry.BuyerSupplierRelationshipFields.Incoterm))
			{
				if (!BusinessEntity.PaymentTerms.IsEmpty)
				{
					supplierBuyer.OrgSupBuyLinkTrnModes[0].PF_IncoTerm = BusinessEntity.PaymentTerms;
				}
			}
			else
			{
				supplierBuyer.OrgSupBuyLinkTrnModes[0].PF_IncoTerm = ZString.Empty;
			}
		}

		void SetDefaultTransportMode(OrgSupplierBuyerLink supplierBuyer)
		{
			if (!BusinessEntity.TransportMode.IsEmpty)
			{
				string transportMode = supplierBuyer.OrgSupBuyLinkTrnModes[0].Lookups.TransportModeList
					.Cast<ICodeDescription>()
					.Where((codeDesc) => codeDesc.Code == BusinessEntity.TransportMode)
					.Select((codeDesc) => codeDesc.Code)
					.FirstOrDefault();
				supplierBuyer.OrgSupBuyLinkTrnModes[0].PF_TransportMode = (!string.IsNullOrEmpty(transportMode)) ? transportMode : Core.Constants.TransportModes.All;
			}
		}

		void SetDefaultContainerMode(OrgSupplierBuyerLink supplierBuyer)
		{
			if (IsDefaultingAllowed(OrganisationsDataRegistry.BuyerSupplierRelationshipFields.ContainerMode))
			{
				supplierBuyer.OrgSupBuyLinkTrnModes[0].PF_ContainerMode = BusinessEntity.ContainerMode;
			}
			else
			{
				supplierBuyer.OrgSupBuyLinkTrnModes[0].PF_ContainerMode = ZString.Empty;
			}
		}

		void SetDefaultNoOriginalBills(OrgSupplierBuyerLink supplierBuyer)
		{
			if (!BusinessEntity.NoOriginalBills.IsEmpty
				&& IsDefaultingAllowed(OrganisationsDataRegistry.BuyerSupplierRelationshipFields.OriginalBills))
			{
				supplierBuyer.OrgSupBuyLinkTrnModes[0].PF_NoOfOriginalBills = BusinessEntity.NoOriginalBills;
			}
		}

		void SetDefaultNoCopyBills(OrgSupplierBuyerLink supplierBuyer)
		{
			if (!BusinessEntity.NoCopyBills.IsEmpty
				&& IsDefaultingAllowed(OrganisationsDataRegistry.BuyerSupplierRelationshipFields.CopyBills))
			{
				supplierBuyer.OrgSupBuyLinkTrnModes[0].PF_NoOfCopyBills = BusinessEntity.NoCopyBills;
			}
		}

		bool IsDefaultingAllowed(string code)
		{
			return !OrganisationsDataRegistry.Instance.BuyerSupplierRelationshipFieldsToExclude.Value.GetBoolFromCode(code);
		}

		#endregion

		#endregion

		#region Restore

		void RestoreBuyerSupplierLink()
		{
			if (!(Globals.IsWeb && BusinessEntity.IsInDatabase))
			{
				RestoreImportBroker();
				RestorePickupCartageCompany();
				RestoreDeliveryCartageCompany();
				RestoreCarrier();
				RestoreSendingAgent();
				RestoreReceivingAgent();
			}

			RestoreTransportMode();
			RestoreGoodsCurrency();
			RestoreServiceLevel();
			RestoreIncoTerms();
			RestoreOverrideNotifyPartyAddress();
			RestoreOverrideNotifyPartyContact();
			RestoreNumberOfBills();
			RestoreGoodsDescription();
			RestoreHandlingInformation();
			RestoreOverridePickupAddress();
			RestoreOverrideDeliveryAddress();
			RestoreOverridePickupContact();
			RestoreOverrideDeliveryContact();
			TryRestoreOriginPort();
			TryRestoreDestinationPort();
			TryRestoreLoadPort();
			TryRestoreDischargePort();
			RestoreEFreightStatus();
		}

		#region TransportMode

		internal void RestoreTransportMode()
		{
			if (!IsTransportModeRestorationSuspended)
			{
				if (SupplierBuyerLink != null && SupplierBuyerLink.OrgSupBuyLinkTrnModes.Count == 1
					&& BusinessEntity.TransportMode.IsEmpty && SupplierBuyerLink.OrgSupBuyLinkTrnModes[0].PF_TransportMode.ToUpper() != "ALL")
				{
					var transportMode = SupplierBuyerLink.OrgSupBuyLinkTrnModes[0].PF_TransportMode;
					var containerMode = SupplierBuyerLink.OrgSupBuyLinkTrnModes[0].PF_ContainerMode;

					if (BusinessEntity.ShouldDefaultContainerModeAndIsContainerised(containerMode))
					{
						BusinessEntity.TransportMode = Core.Constants.TransportModes.Sea;
						BusinessEntity.ContainerMode = containerMode;
					}
					else
					{
						BusinessEntity.TransportMode = transportMode;
					}
				}
			}
		}

		#endregion

		#region Import Broker

		public void RestoreImportBroker()
		{
			var brokerPK = GetImportBrokerFromBuyerSupplierLink();
			if (brokerPK.HasValue)
			{
				BusinessEntity.ImportBrokerPK = brokerPK.Value;
			}
			else
			{
				BusinessEntity.RestoreImportBrokerFallback();
			}
		}

		public ZGuid? GetImportBrokerFromBuyerSupplierLink()
		{
			if (SupplierBuyerLinkDetails != null && !SupplierBuyerLinkDetails.PF_OH_ImportCustomsAgent.IsEmpty)
			{
				return SupplierBuyerLinkDetails.PF_OH_ImportCustomsAgent;
			}
			else if (SupplierBuyerLink != null && !SupplierBuyerLink.OL_OH_ImportBroker.IsEmpty)
			{
				return SupplierBuyerLink.OL_OH_ImportBroker;
			}

			return null;
		}

		#endregion

		#region Goods Currency

		void RestoreGoodsCurrency()
		{
			if (SupplierBuyerLink != null)
			{
				if (SupplierBuyerLink.DefaultCurrency != null && !SupplierBuyerLink.OL_RX_NKDefaultCurrency.IsEmpty)
				{
					BusinessEntity.GoodsCurrency = SupplierBuyerLink.OL_RX_NKDefaultCurrency;
				}
				else
				{
					BusinessEntity.RestoreGoodsCurrencyFallback();
				}
			}
		}

		#endregion

		#region Service Level

		void RestoreServiceLevel()
		{
			if (SupplierBuyerLinkDetails != null && !SupplierBuyerLinkDetails.PF_RS_NKDefaultServiceLevel.IsEmpty)
			{
				BusinessEntity.ServiceLevel = SupplierBuyerLinkDetails.PF_RS_NKDefaultServiceLevel;
			}
			else
			{
				BusinessEntity.RestoreServiceLevelFallback();
			}
		}

		#endregion

		#region Inco Terms

		void RestoreIncoTerms()
		{
			if (BusinessEntity.ShouldRestorePaymentTerm)
			{
				ZString incoTerm = SupplierBuyerLinkDetails != null
				? SupplierBuyerLinkDetails.PF_IncoTerm
				: OrgSupplierBuyerLink.GetDefaultINCO(BusinessEntity.Consignor, BusinessEntity.Consignee, BusinessEntity.Destination.Left(2), ConsolTransportMode(BusinessEntity.TransportMode), BusinessEntity.ContainerMode);

				if (!incoTerm.IsEmpty && (SupplierBuyerLink != null || BusinessEntity.PaymentTerms.IsEmpty))
				{
					BusinessEntity.PaymentTerms = incoTerm;
				}
			}
		}

		static ZString ConsolTransportMode(ZString shipmentTransportMode)
		{
			var transportMode = shipmentTransportMode;
			switch (shipmentTransportMode)
			{
				case Core.Constants.TransportModes.SeaAir:
					transportMode = Core.Constants.TransportModes.Sea;
					break;
				case Core.Constants.TransportModes.AirSea:
					transportMode = Core.Constants.TransportModes.Air;
					break;
			}
			return transportMode;
		}

		#endregion

		#region Notify Party Address

		public bool RestoreOverrideNotifyPartyAddress()
		{
			if (BusinessEntity.ShouldRestorePickupDeliveryAndNotifyPartyAddress && BusinessEntity.NotifyPartyDocumentaryAddress != null && SupplierBuyerLinkDetails != null && !SupplierBuyerLinkDetails.PF_OA_OverrideNotifyPartyAddress.IsEmpty)
			{
				BusinessEntity.NotifyPartyDocumentaryAddress.E2_OA_Address = SupplierBuyerLinkDetails.PF_OA_OverrideNotifyPartyAddress;
				return true;
			}

			return false;
		}

		#endregion

		#region Notify Party Contact

		void RestoreOverrideNotifyPartyContact()
		{
			if (BusinessEntity.NotifyPartyDocumentaryAddress == null)
			{
				return;
			}

			if (SupplierBuyerLinkDetails != null && !SupplierBuyerLinkDetails.PF_OC_OverrideNotifyParty.IsEmpty)
			{
				BusinessEntity.NotifyPartyDocumentaryAddress.ContactPK = SupplierBuyerLinkDetails.PF_OC_OverrideNotifyParty;
			}
			else if (SupplierBuyerLink != null && !SupplierBuyerLink.OL_OC_NotifyPartyContact.IsEmpty)
			{
				BusinessEntity.NotifyPartyDocumentaryAddress.ContactPK = SupplierBuyerLink.OL_OC_NotifyPartyContact;
			}
		}

		#endregion

		#region Number Of Bills

		public void RestoreNumberOfBills()
		{
			if (SupplierBuyerLinkDetails != null && BusinessEntity.ReleaseType != Core.Constants.ShipmentReleaseTypes.ExpressBofL
				&& (SupplierBuyerLinkDetails.PF_NoOfOriginalBills > 0 || SupplierBuyerLinkDetails.PF_NoOfCopyBills > 0))
			{
				BusinessEntity.NoOriginalBills = SupplierBuyerLinkDetails.PF_NoOfOriginalBills;
				BusinessEntity.NoCopyBills = SupplierBuyerLinkDetails.PF_NoOfCopyBills;
			}
			else
			{
				BusinessEntity.RestoreNumberOfBillsWithFallback();
			}
		}

		#endregion

		#region Goods Description

		protected void RestoreGoodsDescription()
		{
			if (SupplierBuyerLinkDetails != null && !SupplierBuyerLinkDetails.PF_GoodsDescription.IsEmpty)
			{
				BusinessEntity.GoodsDescription = SupplierBuyerLinkDetails.PF_GoodsDescription;
			}
			else if (BusinessEntity.Consignor != null && BusinessEntity.Consignor.MiscServ != null && !BusinessEntity.Consignor.MiscServ.OM_EXGoodsDescription.IsEmpty)
			{
				BusinessEntity.GoodsDescription = BusinessEntity.Consignor.MiscServ.OM_EXGoodsDescription;
			}
		}

		#endregion

		#region Handling Information

		protected void RestoreHandlingInformation()
		{
			if (!BusinessEntity.ShouldRestoreHandlingInformation)
			{
				return;
			}

			ZString handlingInstructionText = "";

			if (SupplierBuyerLinkDetails != null && !SupplierBuyerLinkDetails.PF_HandlingInstructions.IsEmpty)
			{
				handlingInstructionText = SupplierBuyerLinkDetails.PF_HandlingInstructions;
			}
			else if (BusinessEntity.Consignor != null && BusinessEntity.Consignor.MiscServ != null && !BusinessEntity.Consignor.MiscServ.OM_EXHandlingInstuctions.IsEmpty)
			{
				handlingInstructionText = BusinessEntity.Consignor.MiscServ.OM_EXHandlingInstuctions;
			}

			if (!handlingInstructionText.IsEmpty)
			{
				StmNote[] notes = BusinessEntity.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.HandlingInstructions.Description);
				if (notes.Length > 0)
				{
					notes[0].ST_NoteDataAsText = handlingInstructionText;
				}
				else
				{
					BusinessEntity.GetNotes().AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, handlingInstructionText);
				}
			}
		}

		#endregion

		#region Pickup Address

		public bool RestoreOverridePickupAddress()
		{
			if (BusinessEntity.ShouldRestorePickupDeliveryAndNotifyPartyAddress)
			{
				if (BusinessEntity.ConsignorPickupAddress != null && SupplierBuyerLinkDetails != null && !SupplierBuyerLinkDetails.PF_OA_OverridePickupAddress.IsEmpty)
				{
					BusinessEntity.ConsignorPickupAddress.E2_OA_Address = SupplierBuyerLinkDetails.PF_OA_OverridePickupAddress;
					return true;
				}
			}

			return false;
		}

		#endregion

		#region Delivery Address

		public bool RestoreOverrideDeliveryAddress()
		{
			if (BusinessEntity.ShouldRestorePickupDeliveryAndNotifyPartyAddress)
			{
				if (BusinessEntity.ConsigneeDeliveryAddress != null && SupplierBuyerLinkDetails != null && !SupplierBuyerLinkDetails.PF_OA_OverrideDeliveryAddress.IsEmpty)
				{
					BusinessEntity.ConsigneeDeliveryAddress.E2_OA_Address = SupplierBuyerLinkDetails.PF_OA_OverrideDeliveryAddress;
					return true;
				}
			}

			return false;
		}

		#endregion

		#region Pickup Contact

		void RestoreOverridePickupContact()
		{
			if (BusinessEntity.ConsignorPickupAddress != null && SupplierBuyerLinkDetails != null && !SupplierBuyerLinkDetails.PF_OC_OverrideSupplierContact.IsEmpty)
			{
				BusinessEntity.ConsignorPickupAddress.ContactPK = SupplierBuyerLinkDetails.PF_OC_OverrideSupplierContact;
			}
		}

		#endregion

		#region Delivery Contact

		void RestoreOverrideDeliveryContact()
		{
			if (BusinessEntity.ConsigneeDeliveryAddress != null && SupplierBuyerLinkDetails != null && !SupplierBuyerLinkDetails.PF_OC_OverrideConsigneeContact.IsEmpty)
			{
				BusinessEntity.ConsigneeDeliveryAddress.ContactPK = SupplierBuyerLinkDetails.PF_OC_OverrideConsigneeContact;
			}
		}

		#endregion

		#region Carrier

		void RestoreCarrier()
		{
			if (SupplierBuyerLinkDetails != null && !SupplierBuyerLinkDetails.PF_OH_CarrierLine.IsEmpty)
			{
				BusinessEntity.ShippingLinePK = SupplierBuyerLinkDetails.PF_OH_CarrierLine;
			}
		}

		#endregion

		#region Pickup Cartage Company

		void RestorePickupCartageCompany()
		{
			if (SupplierBuyerLinkDetails != null && !SupplierBuyerLinkDetails.PF_OH_PickupCartageContractor.IsEmpty)
			{
				BusinessEntity.PickupCartageCoPK = SupplierBuyerLinkDetails.PF_OH_PickupCartageContractor;
			}
		}

		#endregion

		#region Delivery Cartage Company

		void RestoreDeliveryCartageCompany()
		{
			if (SupplierBuyerLinkDetails != null && !SupplierBuyerLinkDetails.PF_OH_DeliveryCartageContractor.IsEmpty)
			{
				BusinessEntity.DeliveryCartageCoPK = SupplierBuyerLinkDetails.PF_OH_DeliveryCartageContractor;
			}
		}

		#endregion

		#region Sending Agent

		void RestoreSendingAgent()
		{
			if (SupplierBuyerLinkDetails != null && !SupplierBuyerLinkDetails.PF_OH_SendingAgent.IsEmpty)
			{
				BusinessEntity.SendingAgentPK = SupplierBuyerLinkDetails.PF_OH_SendingAgent;
			}
			else
			{
				BusinessEntity.RestoreSendingAgentFallback();
			}
		}

		#endregion

		#region Receiving Agent

		void RestoreReceivingAgent()
		{
			if (SupplierBuyerLinkDetails != null && !SupplierBuyerLinkDetails.PF_OH_ReceivingAgent.IsEmpty)
			{
				BusinessEntity.ReceivingAgentPK = SupplierBuyerLinkDetails.PF_OH_ReceivingAgent;
			}
			else
			{
				BusinessEntity.RestoreReceivingAgentFallback();
			}
		}

		#endregion

		#region Origin Port

		public bool TryRestoreOriginPort()
		{
			if (!IsPortRestorationSuspended)
			{
				if (SupplierBuyerLinkDetails != null && !SupplierBuyerLinkDetails.PF_RL_NKPlaceOfReceivalPort.IsEmpty)
				{
					BusinessEntity.Origin = SupplierBuyerLinkDetails.PF_RL_NKPlaceOfReceivalPort;
					return true;
				}
			}

			return false;
		}

		#endregion

		#region Destination Port

		public bool TryRestoreDestinationPort()
		{
			if (!IsPortRestorationSuspended)
			{
				if (SupplierBuyerLinkDetails != null && !SupplierBuyerLinkDetails.PF_RL_NKPlaceOfDeliveryPort.IsEmpty)
				{
					BusinessEntity.Destination = SupplierBuyerLinkDetails.PF_RL_NKPlaceOfDeliveryPort;
					return true;
				}
			}

			return false;
		}

		#endregion

		#region Load Port

		void TryRestoreLoadPort()
		{
			if (!IsPortRestorationSuspended && SupplierBuyerLinkDetails != null && !SupplierBuyerLinkDetails.PF_RL_NKLoadPort.IsEmpty)
			{
				BusinessEntity.LoadPort = SupplierBuyerLinkDetails.PF_RL_NKLoadPort;
			}
		}

		#endregion

		#region Discharge Port

		void TryRestoreDischargePort()
		{
			if (!IsPortRestorationSuspended && SupplierBuyerLinkDetails != null && !SupplierBuyerLinkDetails.PF_RL_NKDischargePort.IsEmpty)
			{
				BusinessEntity.DischargePort = SupplierBuyerLinkDetails.PF_RL_NKDischargePort;
			}
		}

		#endregion

		#region EFreight Status

		public void RestoreEFreightStatus()
		{
			var defaultValue = SupplierBuyerLink != null ? SupplierBuyerLink.OL_EFreightStatus : ZString.Empty;
			BusinessEntity.RestoreEFreightStatusFallback(defaultValue);
		}

		#endregion

		#endregion

		#region Relationship Partner

		bool UseBuyerSupplierRelationships
		{
			get { return Env.Registry.UseBuyerSupplierRelationships && !BusinessEntity.PreventBuyerSupplierRelationships; }
		}

		void SetSupplierFromBuyer()
		{
			if (UseBuyerSupplierRelationships)
			{
				if (!BusinessEntity.IsSettingDefaultValues && BusinessEntity.Consignee != null && BusinessEntity.Consignor == null && BusinessEntity.Consignee.SupplierLinks.Count == 1)
				{
					BusinessEntity.Consignor = BusinessEntity.Consignee.SupplierLinks[0].Supplier;
				}
			}
		}

		void SetBuyerFromSupplier()
		{
			if (UseBuyerSupplierRelationships)
			{
				if (!BusinessEntity.IsSettingDefaultValues && BusinessEntity.Consignor != null && BusinessEntity.Consignee == null && BusinessEntity.Consignor.BuyerLinks.Count == 1)
				{
					BusinessEntity.Consignee = BusinessEntity.Consignor.BuyerLinks[0].Buyer;
				}
			}
		}

		#endregion

		#region Should Show Related Consignees/ors

		public bool ShouldShowRelatedConsignors
		{
			get
			{
				bool result = false;
				if (UseBuyerSupplierRelationships)
				{
					result = !BusinessEntity.IsSettingDefaultValues && BusinessEntity.Consignee != null && BusinessEntity.Consignor == null && BusinessEntity.Consignee.SupplierLinks.Count > 1;
				}

				return result;
			}
		}

		public bool ShouldShowRelatedConsignees
		{
			get
			{
				bool result = false;
				if (UseBuyerSupplierRelationships)
				{
					result = !BusinessEntity.IsSettingDefaultValues && BusinessEntity.Consignor != null && BusinessEntity.Consignee == null && BusinessEntity.Consignor.BuyerLinks.Count > 1;
				}

				return result;
			}
		}

		#endregion
	}
}
