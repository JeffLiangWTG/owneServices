using System;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class ModeAndPartyControl : ZUserControl
	{
		public ModeAndPartyControl()
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				ConsignorDocumentaryDocAddressControl.Enter += ConsignorDocumentaryDocAddressControl_Enter;
				ConsigneeDocumentaryDocAddressControl.Enter += ConsigneeDocumentaryDocAddressControl_Enter;
				UpdateConsigneeConsignorCaptions();

				ConsigneeDocumentaryDocAddressControl.AllowOverlap(LocalClientOrgControl);
				ConsignorDocumentaryDocAddressControl.AllowOverlap(ConsigneeDocumentaryDocAddressControl);
			}
		}

		#region Binding

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (Shipment != null)
			{
				Shipment.JS_ShipmentTypeInfo.ValueChanged -= JS_ShipmentTypeInfo_ValueChanged;
				Shipment.JS_TransportModeInfo.ValueChanged -= JS_TransportModeInfo_ValueChanged;

				Shipment.ConsigneeDocumentaryAddress.E2_AddressOverrideInfo.ValueChanged -= ConsigneeConsignorAddressOverrideInfo_ValueChanged;
				Shipment.ConsignorDocumentaryAddress.E2_AddressOverrideInfo.ValueChanged -= ConsigneeConsignorAddressOverrideInfo_ValueChanged;

				Shipment.JobCreated -= ShipmentOnJobCreated;
				Shipment.JobDeleting -= ShipmentOnJobDeleted;
				Shipment.CrossTradeChanged -= ShipmentOnCrossTradeChanged;
				VisibilityRelationshipProvider.ClearDependency(JobHeaderClientCoveringLabel);
				VisibilityRelationshipProvider.ClearDependency(LocalClientOrgControl);
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (Shipment != null)
			{
				Shipment.JS_ShipmentTypeInfo.ValueChanged += JS_ShipmentTypeInfo_ValueChanged;

				Shipment.ConsigneeDocumentaryAddress.E2_AddressOverrideInfo.ValueChanged += ConsigneeConsignorAddressOverrideInfo_ValueChanged;
				Shipment.ConsignorDocumentaryAddress.E2_AddressOverrideInfo.ValueChanged += ConsigneeConsignorAddressOverrideInfo_ValueChanged;

				Shipment.JobCreated += ShipmentOnJobCreated;
				Shipment.JobDeleted += ShipmentOnJobDeleted;
				Shipment.CrossTradeChanged += ShipmentOnCrossTradeChanged;
				VisibilityRelationshipProvider.SetDependency(LocalClientOrgControl, new JobPresenceBasedVisibilityProvider(Shipment, true));
				VisibilityRelationshipProvider.SetDependency(JobHeaderClientCoveringLabel, new JobPresenceBasedVisibilityProvider(Shipment, false));

				JobHeaderClientCoveringLabel.Text = JobHandler != null ? JobHandler.InitializationMessage : "";

				SetConsignorConsigneeOrder();
				UpdateConsigneeConsignorCaptions();

				SetACILabelsVisibility();

				Shipment.JS_TransportModeInfo.ValueChanged += JS_TransportModeInfo_ValueChanged;

				if (ShouldChangeCaptionOnCrossTrade)
				{
					LocalClientOrgControl.Text = Shipment.PrepaidBillToPartyText;
					LocalClientOrgControl.CaptionResourceString = Shipment.PrepaidBillToPartyCaption;
				}
			}
		}

		void JS_TransportModeInfo_ValueChanged(object sender, EventArgs e)
		{
			ConfirmReversal();
		}

		protected virtual void ConfirmReversal()
		{
			CommissionReversalConfirmationHelper.ConfirmReversal(Shipment, Shipment.JS_TransportModeInfo, new[]
							{
								Shipment.JS_RL_NKOriginInfo,
								Shipment.JS_RL_NKDestinationInfo,
								Shipment.JS_TransportModeInfo,
							});
		}

		ForwardingShipment Shipment
		{
			get { return (ForwardingShipment)CurrentDataItem; }
		}

		internal LocalClientJobHandler JobHandler { get; set; }

		#endregion

		#region Job Local Client

		void ShipmentOnJobCreated(object sender, EventArgs e)
		{
			// We create the job regardless the current company but the shipment loads its job only if the job is created for current company, otherwise, returns
			// null.
			if (Shipment.IsShipmentJobHeaderForCurrentCompany)
			{
				// Enforcing databinding to newly created JobHeader; overwise system will return "blank" from the previous cached binding
				LocalClientOrgControl.SetDataBinding(Shipment.ShipmentJobHeader.JH_OA_LocalChargesAddr_ZAddress, "");
			}
		}

		void ShipmentOnCrossTradeChanged(object sender, EventArgs e)
		{
			if (Shipment != null)
			{
				if (ShouldChangeCaptionOnCrossTrade)
				{
					LocalClientOrgControl.Text = Shipment.PrepaidBillToPartyText;
					LocalClientOrgControl.CaptionResourceString = Shipment.PrepaidBillToPartyCaption;
				}
				else
				{
					LocalClientOrgControl.Text = Shipment.LocalClientText;
					LocalClientOrgControl.CaptionResourceString = Shipment.LocalClientCaption;
				}
			}
		}

		void ShipmentOnJobDeleted(object sender, EventArgs e)
		{
			JobHeaderClientCoveringLabel.Text = Res.GetString("a3e076c0-e065-4c25-8dea-e434c87f7178", "Billing job is deleted.");
		}

		bool ShouldChangeCaptionOnCrossTrade => AccountingMasterFilesRegistry.Instance.EnableCrossTradeDebtorDefaultingFunctionality.Value && Shipment.IsCrossTrade();

		#endregion

		#region Consignor/Consignee

		void ConsigneeConsignorAddressOverrideInfo_ValueChanged(object sender, EventArgs e)
		{
			SetACILabelsVisibility();
		}

		void SetACILabelsVisibility()
		{
			bool aCIZoneLabelsVisibility = (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedStates || GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Canada);

			ACIConsignorOriginZoneLabel.Visible = aCIZoneLabelsVisibility && !Shipment.ConsignorDocumentaryAddress.E2_AddressOverride;
			ACIConsignorOriginZoneBoundLabel.Visible = ACIConsignorOriginZoneLabel.Visible;

			ACIConsigneeDestinationZoneLabel.Visible = aCIZoneLabelsVisibility && !Shipment.ConsigneeDocumentaryAddress.E2_AddressOverride;
			ACIConsigneeDestinationZoneBoundLabel.Visible = ACIConsigneeDestinationZoneLabel.Visible;
		}

		void ConsigneeDocumentaryDocAddressControl_Enter(object sender, EventArgs e)
		{
			if (Shipment != null && Shipment.BuyerSupplierLinksHelper != null && !Shipment.ConsigneeDocumentaryAddress.ReadOnly && Shipment.BuyerSupplierLinksHelper.ShouldShowRelatedConsignees)
			{
				ConsigneeDocumentaryDocAddressControl.SelectFromPopupForm();
			}
		}

		void ConsignorDocumentaryDocAddressControl_Enter(object sender, EventArgs e)
		{
			if (Shipment != null && Shipment.BuyerSupplierLinksHelper != null && !Shipment.ConsignorDocumentaryAddress.ReadOnly && Shipment.BuyerSupplierLinksHelper.ShouldShowRelatedConsignors)
			{
				ConsignorDocumentaryDocAddressControl.SelectFromPopupForm();
			}
		}

		void SetConsignorConsigneeOrder()
		{
			if (Env.Registry.ShipmentScreenLayout == Constants.ShipmentScreenOptions.Consignor)
			{
				// Do nothing, already showing Consignor - Consignee
			}
			else if (Env.Registry.ShipmentScreenLayout == Constants.ShipmentScreenOptions.Consignee)
			{
				SwapConsignorConsignee();
			}
			else if (Shipment.Consols.Count > 0 && ImportExportHelper.IsBranchCountry(Shipment.Consols[0].JK_RL_NKDischargePort)) // Auto
			{
				SwapConsignorConsignee();
			}
		}

		void SwapConsignorConsignee()
		{
			int oldConsigneeControl = ConsigneeDocumentaryDocAddressControl.Top;
			ControlDpiScalingHelper.SetTop(ref ConsigneeDocumentaryDocAddressControl, ConsignorDocumentaryDocAddressControl.Top, false);
			ControlDpiScalingHelper.SetTop(ref ConsignorDocumentaryDocAddressControl, oldConsigneeControl, false);

			int oldConsigneeTabIndex = ConsigneeDocumentaryDocAddressControl.TabIndex;
			ConsigneeDocumentaryDocAddressControl.TabIndex = ConsignorDocumentaryDocAddressControl.TabIndex;
			ConsignorDocumentaryDocAddressControl.TabIndex = oldConsigneeTabIndex;
		}

		#endregion

		#region Co-Load

		void JS_ShipmentTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateConsigneeConsignorCaptions();
		}

		void UpdateConsigneeConsignorCaptions()
		{
			if (Shipment != null && (Shipment.IsCoLoadMaster || Shipment.IsBlindCoLoadMaster))
			{
				ConsignorDocumentaryDocAddressControl.Text = Res.GetString("Forwarding|ModeAndPartyControl|SendingForwarder", "Sending Forwarder");
			}
			else if (Shipment != null && (Shipment.IsHighVolumeLowValue || Shipment.IsHighVolumeLowValueLegacy))
			{
				ConsignorDocumentaryDocAddressControl.Text = Res.GetString("Forwarding|ModeAndPartyControl|eTailer", "eTailer");
			}
			else if (Shipment != null && Shipment.IsThirdPartyOwnershipHouse)
			{
				ConsignorDocumentaryDocAddressControl.Text = Res.GetString("Forwarding|ModeAndPartyControl|Trader", "Trader/Supplier");
			}
			else
			{
				ConsignorDocumentaryDocAddressControl.Text = FreightDataRegistry.Instance.ConsignorShipperTerminology.Value;
			}

			if (Shipment != null && (Shipment.IsCoLoadMaster || Shipment.IsBlindCoLoadMaster))
			{
				ConsigneeDocumentaryDocAddressControl.Text = Res.GetString("Forwarding|ModeAndPartyControl|ReceivingForwarder", "Receiving Forwarder");
			}
			else
			{
				ConsigneeDocumentaryDocAddressControl.Text = Res.GetString("Forwarding|ModeAndPartyControl|Consignee", "Consignee");
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				ConsignorDocumentaryDocAddressControl.Enter -= new EventHandler(ConsignorDocumentaryDocAddressControl_Enter);
				ConsigneeDocumentaryDocAddressControl.Enter -= new EventHandler(ConsigneeDocumentaryDocAddressControl_Enter);
				if (Shipment != null)
				{
					Shipment.JS_ShipmentTypeInfo.ValueChanged -= new EventHandler(JS_ShipmentTypeInfo_ValueChanged);
					Shipment.JS_TransportModeInfo.ValueChanged -= JS_TransportModeInfo_ValueChanged;
				}
			}

			if (isNotFinalizing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(isNotFinalizing);
		}

		#endregion
	}
}
