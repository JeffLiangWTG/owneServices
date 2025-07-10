using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Layout;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class DetailsEntryControl : ZUserControl
	{
		public DetailsEntryControl()
		{
			InitializeComponent();
			CreateVisibilityDependencies();

			if (!DesignModeFinder.IsDesigning)
			{
				SetDataSourceBinding(PaymentTerm.GetExtension<LabelCaptionRenderer>(), "Caption", "PaymentTermLabel", true);
				SetDataSourceBinding(JS_NoCopyBillsCalcEdit.GetExtension<LabelCaptionRenderer>(), "Caption", "JS_Calc_CopyBillsLabel", true);
				SetDataSourceBinding(PiecesDetailButton, nameof(ZButton.ReadOnly), "IsHighVolumeLowValue");

				PaymentTerm.VisibleChanged += UpdateIncoTermExplainButtonVisibility;
				ServiceLevel.PopupSelected += ServiceLevel_PopupSelected;
				CustomEntriesLinkLabel.LinkClicked += ShowCustomsEntryNumbersForm;

#if DEBUG
				TypeDescriptor.AddAttributes(ChargeableUnitLabel, new SuppressFormsLocalizedTestAttribute());
				TypeDescriptor.AddAttributes(JS_Calc_ActualVolumeWeightUnit, new SuppressFormsLocalizedTestAttribute());
#endif

				JS_PaymentTermLabel.AllowOverlap(Description);
				PaymentTerm.AllowOverlap(Description);
				IncoTermExplainButton.AllowOverlap(Description);
				DetailsPanel.AllowOutsideOfParent();
			}
		}

		void DetailsEntryControl_Layout(object sender, LayoutEventArgs e)
		{
			ResizeTopTabPageMinScrollHeight();
		}

		void ShowCustomsEntryNumbersForm(object sender, EventArgs e)
		{
			if (Shipment != null)
			{
				ZFormModaliser.ShowDialogAndDispose(new CustomsEntryNumbersForm(Shipment));
			}
		}

		void CreateVisibilityDependencies()
		{
			this.VisibilityRelationshipProvider.SetDependency(this.EstExportCustomsClearDate, this.EstExportCustomsClearLabel);
			this.VisibilityRelationshipProvider.SetDependency(this.ScreeningStatus, new ShipmentDpsVisibilityProvider(() => ScreeningStatus.Visible));
			this.VisibilityRelationshipProvider.SetDependency(this.ScreenButton, this.ScreeningStatus);
			this.VisibilityRelationshipProvider.SetDependency(this.customsEntryAdditionalInfoControl, this.CustomsEntryNumber);
			this.VisibilityRelationshipProvider.SetDependency(this.CODType, this.ShipperCOD);
			this.VisibilityRelationshipProvider.SetDependency(this.CustomsEntryNumberTypeBoundDropEdit, this.CustomsEntryNumber);
			this.VisibilityRelationshipProvider.SetDependency(this.CustomEntriesLinkLabel, this.CustomsEntryNumber);
			this.VisibilityRelationshipProvider.SetDependency(this.JS_PaymentTermLabel, this.PaymentTerm);
			this.VisibilityRelationshipProvider.SetDependency(this.JS_NoCopyBillsCalcEdit, this.BillDetails);
			this.VisibilityRelationshipProvider.SetDependency(this.JS_NoOriginalBillsCalcEdit, this.BillDetails);
			this.VisibilityRelationshipProvider.SetDependency(this.IncoTermExplainButton, this.PaymentTerm);
			this.VisibilityRelationshipProvider.SetDependency(this.JS_InsuranceBoundCurrencyControl, this.PacksValues);
			this.VisibilityRelationshipProvider.SetDependency(this.JS_ShippedOnBoardDateEdit, this.OnBoard);
			this.VisibilityRelationshipProvider.SetDependency(this.JS_TotalPackageCountBoundDropCalcEdit, this.PacksValues);
			this.VisibilityRelationshipProvider.SetDependency(this.JS_RL_NKDestinationCodeFindBox, this.OriginDestinationDates);
			this.VisibilityRelationshipProvider.SetDependency(this.PiecesDetailButton, this.PacksValues);
			this.VisibilityRelationshipProvider.SetDependency(this.JS_ActualVolumeBoundCalcDropEdit, this.WeightVolumeChargeable);
			this.VisibilityRelationshipProvider.SetDependency(this.JS_GoodsValueBoundCurrencyControl, this.PacksValues);
			this.VisibilityRelationshipProvider.SetDependency(this.JS_E_ARVBoundDateEdit, this.OriginDestinationDates);
			this.VisibilityRelationshipProvider.SetDependency(this.ChargeableUnitLabel, this.Chargeable);
			this.VisibilityRelationshipProvider.SetDependency(this.DomesticCheckBox, this.HouseBill);
			this.VisibilityRelationshipProvider.SetDependency(this.JS_E_DEPBoundDateEdit, this.OriginDestinationDates);
			this.VisibilityRelationshipProvider.SetDependency(this.JS_Calc_ActualVolumeWeight, this.Chargeable);
			this.VisibilityRelationshipProvider.SetDependency(this.JS_Calc_ActualVolumeWeightUnit, this.Chargeable);
			this.VisibilityRelationshipProvider.SetDependency(this.TotalCO2eUnit, this.TotalCO2e);
			this.VisibilityRelationshipProvider.SetDependency(this.DestinationExchangeRateCalcEdit, this.DestinationGoodsValueCalcFindBox);

			if (SupplyChainSecurityConfiguration.New().IsHighRiskApplicable)
			{
				this.VisibilityRelationshipProvider.SetDependency(this.IsHighRisk, this.AviationSecurity);
				this.VisibilityRelationshipProvider.SetDependency(this.AdditionalInspectionType, this.AviationSecurity);
			}
		}

		#region Details Panel Splitter

		void DetailsPanel_Layout(object sender, LayoutEventArgs e)
		{
			RefreshLayout();
		}

		public void HookLayout()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				DetailsPanel.Layout += new LayoutEventHandler(DetailsPanel_Layout);
				Layout += DetailsEntryControl_Layout;
			}
		}

		public void RefreshLayout()
		{
			ResizeTopTabPageMinScrollHeight();

			ResetParentSplitterDistance();
		}

		void ResetParentSplitterDistance()
		{
			var foundParent = DetailsPanel.Parent;
			while (foundParent != null)
			{
				SplitContainer parentSplitContainer = foundParent as SplitContainer;
				if (parentSplitContainer != null)
				{
					int totalHeight = ((RowLayout)DetailsPanel.LayoutEngine).VisibleRowCount * DetailsPanel.RowHeight;
					parentSplitContainer.SplitterDistance = totalHeight + ControlDpiScalingHelper.ScaleToCurrentDpiY(20);
					break;
				}
				else
				{
					foundParent = foundParent.Parent;
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "This method works with actual (scaled) pixel sizes.")]
		void ResizeTopTabPageMinScrollHeight()
		{
			var detailPanelHeight = ((RowLayout)DetailsPanel.LayoutEngine).VisibleRowCount * DetailsPanel.RowHeight + ControlDpiScalingHelper.ScaleToCurrentDpiY(30);
			var foundParent = DetailsPanel.Parent;
			var minSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 680, true);

			while (foundParent != null)
			{
				var topTabPage = foundParent as ZTabPage;
				if (topTabPage != null)
				{
					if (topTabPage.AutoScrollMinSize.Height < detailPanelHeight && minSize.Height < detailPanelHeight)
					{
						topTabPage.AutoScrollMinSize = new Size(minSize.Width, detailPanelHeight);
						topTabPage.PerformLayout();
					}
					else if (topTabPage.AutoScrollMinSize.Height != minSize.Height && topTabPage.AutoScrollMinSize.Height != detailPanelHeight)
					{
						topTabPage.AutoScrollMinSize = minSize;
						topTabPage.PerformLayout();
					}

					break;
				}
				else
				{
					foundParent = foundParent.Parent;
				}
			}
		}

		#endregion

		#region Binding

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (Shipment != null)
			{
				Shipment.JS_TransportModeInfo.ValueChanged -= new EventHandler(JS_TransportModeInfo_ValueChanged);
				Shipment.IsDomesticFreightInfo.ValueChanged -= new EventHandler(UpdateIncoTermExplainButtonVisibility);
				Shipment.JS_RL_NKOriginInfo.ValueChanged -= JS_RL_NKOriginInfo_ValueChanged;
				Shipment.JS_RL_NKDestinationInfo.ValueChanged -= JS_RL_NKDestinationInfo_ValueChanged;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (Shipment != null)
			{
				Shipment.JS_TransportModeInfo.ValueChanged += new EventHandler(JS_TransportModeInfo_ValueChanged);
				Shipment.IsDomesticFreightInfo.ValueChanged += new EventHandler(UpdateIncoTermExplainButtonVisibility);
				UpdateIncoTermExplainButtonVisibility();

				if (Shipment.CusEntryNumbers.Count > 0 && Shipment.CusEntryNumbers[0].Country != null && Shipment.AddCountryCodeToEntryDetailsCaption)
				{
					CustomsEntryNumberTypeBoundDropEdit.GetExtension<LabelCaptionRenderer>().Caption += " (" + Shipment.CusEntryNumbers[0].CE_RN_NKCountryCode + ")";
				}

				SetLabelAndCaptionResourceString();
				FormatETAETD();

				Shipment.JS_RL_NKOriginInfo.ValueChanged += JS_RL_NKOriginInfo_ValueChanged;
				Shipment.JS_RL_NKDestinationInfo.ValueChanged += JS_RL_NKDestinationInfo_ValueChanged;
			}
		}

		void JS_RL_NKDestinationInfo_ValueChanged(object sender, EventArgs e)
		{
			ConfirmReversal(Shipment.JS_RL_NKDestinationInfo);
		}

		void JS_RL_NKOriginInfo_ValueChanged(object sender, EventArgs e)
		{
			ConfirmReversal(Shipment.JS_RL_NKOriginInfo);
		}

		protected virtual void ConfirmReversal(ZPropertyInfo info)
		{
			CommissionReversalConfirmationHelper.ConfirmReversal(Shipment, info, new[]
							{
								Shipment.JS_RL_NKOriginInfo,
								Shipment.JS_RL_NKDestinationInfo,
								Shipment.JS_TransportModeInfo,
							});
		}

		ForwardingShipment Shipment
		{
			get { return (ForwardingShipment)base.CurrentDataItem; }
		}

		#endregion

		#region JS_TransportMode ValueChanged

		void JS_TransportModeInfo_ValueChanged(object sender, EventArgs e)
		{
			SetLabelAndCaptionResourceString();
			FormatETAETD();
		}

		void SetLabelAndCaptionResourceString()
		{
			if (Shipment != null)
			{
				JS_Calc_ActualVolumeWeight.GetExtension<ILabelCaptionRenderer>().Caption = ShipmentActualVolumeWeightLabel;
			}
		}

		string ShipmentActualVolumeWeightLabel => Shipment.IsShipmentChargeableByWeight ? Res.GetString("fd0cea7d-46b0-4583-9ebe-73bf518922fe", "VW") : Res.GetString("142cd9a9-f685-4ec1-bef9-edd0b1596f23", "WV");

		void FormatETAETD()
		{
			bool showTime = Shipment.JS_TransportMode == Constants.TransportModes.Air ||
				Shipment.JS_TransportMode == Constants.TransportModes.AirSea ||
				Shipment.JS_TransportMode == Constants.TransportModes.Rail ||
				Shipment.JS_TransportMode == Constants.TransportModes.Road ||
				Shipment.JS_TransportMode == Constants.TransportModes.Courier;

			ZDateTimePickerFormat dateFormat = showTime ? ZDateTimePickerFormat.Long : ZDateTimePickerFormat.Short;
			JS_E_DEPBoundDateEdit.DateTimeFormat = dateFormat;
			JS_E_ARVBoundDateEdit.DateTimeFormat = dateFormat;
		}

		#endregion

		#region Packages Detail

		void PiecesDetailButton_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new ShipmentPiecesDetailForm(Shipment));
		}

		#endregion

		#region IncoTerms

		void UpdateIncoTermExplainButtonVisibility(object sender, EventArgs e)
		{
			UpdateIncoTermExplainButtonVisibility();
		}

		void UpdateIncoTermExplainButtonVisibility()
		{
			if (Shipment != null)
			{
				IncoTermExplainButton.Parent.SuspendLayout();
				IncoTermExplainButton.Visible = PaymentTerm.Visible && !Shipment.IsDomesticFreight;
				IncoTermExplainButton.Parent.ResumeLayout(false);
			}
		}

		void IncoTermExplainButton_Click(object sender, EventArgs e)
		{
			if (!Shipment.JS_INCOInfo.HasErrors())
			{
				IncoTermDescriptionForm form = new IncoTermDescriptionForm(Shipment.JS_INCO);
				ZFormModaliser.Show(form, ParentForm as ZForm);
			}
		}

		#endregion

		#region ServiceLevel

		void ServiceLevel_PopupSelected(object sender, EmbeddedModulePopup.SelectedEventArgs e)
		{
			var selectedObjects = e.SelectedBusinessObjects;
			if (selectedObjects.Length == 1 && Shipment != null)
			{
				Shipment.OnServiceLevelOrTransitTimeSelected(selectedObjects[0]);
			}
		}

		#endregion

		#region Screen button

		async void ScreenButton_Click(object sender, EventArgs e)
		{
			IScreeningPartyProvider provider = Shipment;
			if (provider != null)
			{
				await new DeniedPartyScreeningPresentationManager().PerformScreening((ZForm)ParentForm, false, !DeniedPartyScreenerAsync.HasExcludedList(Shipment.Factory));
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			base.Dispose(isNotFinalizing);
			if (isNotFinalizing)
			{
				if (Shipment != null)
				{
					Shipment.JS_TransportModeInfo.ValueChanged += new EventHandler(JS_TransportModeInfo_ValueChanged);
					Shipment.JS_RL_NKOriginInfo.ValueChanged -= JS_RL_NKOriginInfo_ValueChanged;
					Shipment.JS_RL_NKDestinationInfo.ValueChanged -= JS_RL_NKDestinationInfo_ValueChanged;
				}
			}
		}

		#endregion
	}
}
