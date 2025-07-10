using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class ContainersUserControl : ZUserControl
	{
		public ContainersUserControl()
		{
			InitializeComponent();
			CreateVisibilityDependencies();

			if (!DesignModeFinder.IsDesigning)
			{
				SetupCustomFields();
				TariffFindHelper.AddDefaultPropertyToTariffControl(HarmonisedCodeFindBox, (ForwardingShipment)packLinesContol.Grid.DataSource);
				UpdateHSCountry();
			}

			if (!FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.Value)
			{
				PackagesDetailTabControl.Controls.Remove(PkgPackageDetailsTabPage);
				PkgPackageDetailsTabPage.Dispose();
			}

			this.Load += ContainersUserControl_Load;
		}

		void ContainersUserControl_Load(object sender, EventArgs e)
		{
			packLinesContol.Grid.SelectedRowsChangedInMouseDown += Grid_SelectedRowsChangedInMouseDown;
			if (Shipment?.OuterPackLines.Any() ?? false)
			{
				packLinesContol.Grid.Select(0);
				Grid_SelectedRowsChangedInMouseDown(null, null);
			}
		}

		void Grid_SelectedRowsChangedInMouseDown(object sender, EventArgs e)
		{
			if (currentPackLine != null)
			{
				currentPackLine.LastKnownTransitWarehouseStatusChanging -= PackLine_LastKnownTransitWarehouseStatusChanging;
			}

			if (packLinesContol.Grid.SelectedElements.Any())
			{
				currentPackLine = packLinesContol.Grid.SelectedElements[0] as ForwardingPackLine;

				if (currentPackLine != null)
				{
					currentPackLine.LastKnownTransitWarehouseStatusChanging += PackLine_LastKnownTransitWarehouseStatusChanging;
				}
			}
		}

		ForwardingPackLine currentPackLine;

		void PackLine_LastKnownTransitWarehouseStatusChanging(object sender, System.ComponentModel.CancelEventArgs e)
		{
			var dialogContext = new DialogDefaultContext(
					new ZGuid("5e9ca4d7-0038-4621-a93b-94754fa19667"),
					ResString.GetMultilingualString("1bf83744-35eb-4362-a80f-1961d783abf9", "Confirm"),
					ZMessageBoxButtons.OKCancel,
					ZMessageBoxIcon.Question,
					null,
					showCheckboxOnly: true
				);

			var dialogString = ResString.GetMultilingualString("1eb34bc1-e9ed-4bfc-808b-0cf58bf73a94", "TW Matching Status is not confirmed.  Are you sure you want to proceed?");
			var dialogResult = Globals.Message.ShowOrDefault(dialogContext, dialogString);

			if (dialogResult == ZDialogResult.OK)
			{
				e.Cancel = false;
			}
			else
			{
				e.Cancel = true;
			}
		}

		UNDGDataItemFormManager undgManager;
		HarmonisedCodeFormManager harmonisedCodeManager;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			var shipment = DataSource as ForwardingShipment;
			var shouldHidePSAGroup = !shipment.IsLoadingIn(Core.Constants.CountryCodes.Singapore) && !shipment.IsDischargingIn(Core.Constants.CountryCodes.Singapore);

			undgManager = new UNDGDataItemFormManager(packLinesContol.Grid, "", UNDGDataItemFormManagerConfig.DynamicHidePSAGroup(shouldHidePSAGroup));
			if (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.Value)
			{
				undgManager.Initialize(DGLinkLabel, DGGuidFindBox, IMOClassDropEdit, FlashPointCalcEdit, DGContactGuidFindBox, DGDetailsLinkLabel, FlashPointUnitLabel, DIIsCombustibleCheckBox);
			}
			else
			{
				undgManager.Initialize(DGLinkLabel, DGGuidFindBox, IMOClassDropEdit, FlashPointCalcEdit, DGContactGuidFindBox, DGDetailsLinkLabel, FlashPointUnitLabel);
			}

			harmonisedCodeManager = new HarmonisedCodeFormManager(packLinesContol.Grid);
			harmonisedCodeManager.Initialize(HSLinkLabel, HSCountryFindBox, HSCodeFindBox, HSDetailsLinkLabel);
			HSCountryFindBox.Resize += HSCountryFindBox_Resize;
			HSCountryFindBox.CodeBox.TextChanged += HSCountryFindBox_TextChanged;

			new ReferenceNumberFormManager(packLinesContol.Grid).Initialize();

			new PackProductFormManager(packLinesContol.Grid).Initialize();
		}

		void CreateVisibilityDependencies()
		{
			this.VisibilityRelationshipProvider.SetDependency(this.JS_Calc_TotalVolumeUnit_ImperialBoundTextBox2, this.JS_ActualWeight_ImperialBoundTextBox);
			this.VisibilityRelationshipProvider.SetDependency(this.JS_ActualVolume_ImperialBoundTextBox, this.JS_ActualWeight_ImperialBoundTextBox);
			this.VisibilityRelationshipProvider.SetDependency(this.JS_ActualVolume_ImperialBoundTextBox, this.JS_ActualWeight_ImperialBoundTextBox);
			this.VisibilityRelationshipProvider.SetDependency(this.JS_Calc_TotalVolumeUnit_ImperialBoundTextBox, this.JS_ActualWeight_ImperialBoundTextBox);
			this.VisibilityRelationshipProvider.SetDependency(this.JS_Calc_TotalVolume_ImperialBoundTextBox, this.JS_ActualWeight_ImperialBoundTextBox);
			this.VisibilityRelationshipProvider.SetDependency(this.ShipmentWeightUnit_ImperialTextBox, this.JS_ActualWeight_ImperialBoundTextBox);
			this.VisibilityRelationshipProvider.SetDependency(this.PacklineTotalWeightUnit_ImperialTextBox, this.JS_ActualWeight_ImperialBoundTextBox);
			this.VisibilityRelationshipProvider.SetDependency(this.JS_Calc_TotalWeight_ImperialBoundTextBox, this.JS_ActualWeight_ImperialBoundTextBox);
		}
		ZUserControl GetPackagesDetailControl()
		{
			if (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.Value)
			{
				var control = new PackagesDetailControl();
				control.OnUpdatePackLineButtonClickHandler += OnUpdatePackLineButtonClick;
				return control;
			}
			else
			{
				var control = new PackagesDetailLegacyControl();
				control.OnUpdatePackLineButtonClickHandler += OnUpdatePackLineButtonClick;
				return control;
			}
		}

		#region Custom Fields

		void SetupCustomFields()
		{
			new CustomFieldColumnCreator().Set(packLinesContol.Grid, new PackLineCustomFieldsDescriptor());

			if (!string.IsNullOrEmpty(Env.Registry.Freight.PackLine.PackLineCustomAttribute1Caption))
			{
				JL_CustomAttrib1TextBox.GetExtension<LabelCaptionRenderer>().Caption = Env.Registry.Freight.PackLine.PackLineCustomAttribute1Caption;
			}

			if (!string.IsNullOrEmpty(Env.Registry.Freight.PackLine.PackLineCustomAttribute1Hint) && Env.Registry.Freight.PackLine.IsPackLineCustomAttribute1HintOverridden)
			{
				JL_CustomAttrib1TextBox.CaptionResourceString =
					new ResourceStringData("", Env.Registry.Freight.PackLine.PackLineCustomAttribute1Hint);
			}

			if (!string.IsNullOrEmpty(Env.Registry.Freight.PackLine.PackLineCustomAttribute2Caption))
			{
				JL_CustomAttrib2TextBox.GetExtension<LabelCaptionRenderer>().Caption = Env.Registry.Freight.PackLine.PackLineCustomAttribute2Caption;
			}

			if (!string.IsNullOrEmpty(Env.Registry.Freight.PackLine.PackLineCustomAttribute2Hint) && Env.Registry.Freight.PackLine.IsPackLineCustomAttribute2HintOverridden)
			{
				JL_CustomAttrib2TextBox.CaptionResourceString =
					new ResourceStringData("", Env.Registry.Freight.PackLine.PackLineCustomAttribute2Hint);
			}

			if (!string.IsNullOrEmpty(Env.Registry.Freight.PackLine.PackLineCustomAttribute3Caption))
			{
				JL_CustomAttrib3TextBox.GetExtension<LabelCaptionRenderer>().Caption = Env.Registry.Freight.PackLine.PackLineCustomAttribute3Caption;
			}

			if (!string.IsNullOrEmpty(Env.Registry.Freight.PackLine.PackLineCustomAttribute3Hint) && Env.Registry.Freight.PackLine.IsPackLineCustomAttribute3HintOverridden)
			{
				JL_CustomAttrib3TextBox.CaptionResourceString =
					new ResourceStringData("", Env.Registry.Freight.PackLine.PackLineCustomAttribute3Hint);
			}

			if (!string.IsNullOrEmpty(Env.Registry.Freight.PackLine.PackLineCustomAttribute4Caption))
			{
				JL_CustomAttrib4TextBox.GetExtension<LabelCaptionRenderer>().Caption = Env.Registry.Freight.PackLine.PackLineCustomAttribute4Caption;
			}

			if (!string.IsNullOrEmpty(Env.Registry.Freight.PackLine.PackLineCustomAttribute4Hint) && Env.Registry.Freight.PackLine.IsPackLineCustomAttribute4HintOverridden)
			{
				JL_CustomAttrib4TextBox.CaptionResourceString =
					new ResourceStringData("", Env.Registry.Freight.PackLine.PackLineCustomAttribute4Hint);
			}

			if (!string.IsNullOrEmpty(Env.Registry.Freight.PackLine.PackLineCustomDecimal1Caption))
			{
				JL_CustomDecimal1CalcEdit.GetExtension<LabelCaptionRenderer>().Caption = Env.Registry.Freight.PackLine.PackLineCustomDecimal1Caption;
			}

			if (!string.IsNullOrEmpty(Env.Registry.Freight.PackLine.PackLineCustomDecimal1Hint) && Env.Registry.Freight.PackLine.IsPackLineCustomDecimal1HintOverridden)
			{
				JL_CustomDecimal1CalcEdit.CaptionResourceString =
					new ResourceStringData("", Env.Registry.Freight.PackLine.PackLineCustomDecimal1Hint);
			}

			if (!string.IsNullOrEmpty(Env.Registry.Freight.PackLine.PackLineCustomDecimal2Caption))
			{
				JL_CustomDecimal2.GetExtension<LabelCaptionRenderer>().Caption = Env.Registry.Freight.PackLine.PackLineCustomDecimal2Caption;
			}

			if (!string.IsNullOrEmpty(Env.Registry.Freight.PackLine.PackLineCustomDecimal2Hint) && Env.Registry.Freight.PackLine.IsPackLineCustomDecimal2HintOverridden)
			{
				JL_CustomDecimal2.CaptionResourceString =
					new ResourceStringData("", Env.Registry.Freight.PackLine.PackLineCustomDecimal2Hint);
			}

			if (!string.IsNullOrEmpty(Env.Registry.Freight.PackLine.PackLineCustomFlag1Caption))
			{
				JL_CustomFlag1CheckBox.GetExtension<LabelCaptionRenderer>().Caption = Env.Registry.Freight.PackLine.PackLineCustomFlag1Caption;
			}

			if (!string.IsNullOrEmpty(Env.Registry.Freight.PackLine.PackLineCustomFlag1Hint) && Env.Registry.Freight.PackLine.IsPackLineCustomFlag1HintOverridden)
			{
				JL_CustomFlag1CheckBox.CaptionResourceString =
					new ResourceStringData("", Env.Registry.Freight.PackLine.PackLineCustomFlag1Hint);
			}

			if (!string.IsNullOrEmpty(Env.Registry.Freight.PackLine.PackLineCustomFlag2Caption))
			{
				JL_CustomFlag2CheckBox.GetExtension<LabelCaptionRenderer>().Caption = Env.Registry.Freight.PackLine.PackLineCustomFlag2Caption;
			}

			if (!string.IsNullOrEmpty(Env.Registry.Freight.PackLine.PackLineCustomFlag2Hint) && Env.Registry.Freight.PackLine.IsPackLineCustomFlag2HintOverridden)
			{
				JL_CustomFlag2CheckBox.CaptionResourceString =
					new ResourceStringData("", Env.Registry.Freight.PackLine.PackLineCustomFlag2Hint);
			}

			if (!string.IsNullOrEmpty(Env.Registry.Freight.PackLine.PackLineCustomDate1Caption))
			{
				CustomDate1DateEdit.GetExtension<LabelCaptionRenderer>().Caption = Env.Registry.Freight.PackLine.PackLineCustomDate1Caption;
			}

			if (!string.IsNullOrEmpty(Env.Registry.Freight.PackLine.PackLineCustomDate1Hint) && Env.Registry.Freight.PackLine.IsPackLineCustomDate1HintOverridden)
			{
				CustomDate1DateEdit.CaptionResourceString =
					new ResourceStringData("", Env.Registry.Freight.PackLine.PackLineCustomDate1Hint);
			}

			if (!string.IsNullOrEmpty(Env.Registry.Freight.PackLine.PackLineCustomDate2Caption))
			{
				CustomDate2DateEdit.GetExtension<LabelCaptionRenderer>().Caption = Env.Registry.Freight.PackLine.PackLineCustomDate2Caption;
			}

			if (!string.IsNullOrEmpty(Env.Registry.Freight.PackLine.PackLineCustomDate2Hint) && Env.Registry.Freight.PackLine.IsPackLineCustomDate2HintOverridden)
			{
				CustomDate2DateEdit.CaptionResourceString =
					new ResourceStringData("", Env.Registry.Freight.PackLine.PackLineCustomDate2Hint);
			}
		}

		void SetPackLineCustomText()
		{
			if (!DesignModeFinder.IsDesigning && !string.IsNullOrEmpty(Env.Registry.Freight.PackLine.PackLineCustomHeadingCaption))
			{
				this.CustomFieldsTabPage.Text = Env.Registry.Freight.PackLine.PackLineCustomHeadingCaption;
			}
		}

		void JS_TransportModeInfo_ValueChanged(object sender, EventArgs e)
		{
			SetupControlsBasedOnTransportMode();
		}

		void SetupControlsBasedOnTransportMode()
		{
			SetNMFCVisibility();
			SetLoadingMetersVisibility();
		}

		void JS_PackingModeInfo_ValueChanged(object sender, EventArgs e)
		{
			SetupControlsBasedOnPackingMode();
		}

		void SetupControlsBasedOnPackingMode()
		{
			var shipment = CurrentDataItem as ForwardingShipment;
			if (shipment != null)
			{
				var columns = packLinesContol.Grid.Columns;
				if (columns.Contains(AutoJobPackLines.Schema.JL_RefNumber))
				{
					var newCaption = (shipment.JS_PackingMode == Enterprise.Core.Constants.ContainerModes.RollOnRollOff) ? Res.GetString("b8a82069-7592-40e6-adf8-e5b91157f6a1", "VIN") : Res.GetString("a5466625-8465-446d-ac81-f3016a34bae2", "Ref Number");
					this.zTextBox1.GetExtension<ILabelCaptionRenderer>().Caption = columns[AutoJobPackLines.Schema.JL_RefNumber].ColumnStyle.HeaderText = newCaption;
				}

				var vehicleColumnList = new string[]
					{
						AutoJobPackLines.Schema.JL_VehicleMake,
						AutoJobPackLines.Schema.JL_VehicleModel,
						AutoJobPackLines.Schema.JL_VehicleYear,
						AutoJobPackLines.Schema.JL_VehicleColor,
						AutoJobPackLines.Schema.JL_VehicleNumberOfDoors,
						AutoJobPackLines.Schema.JL_VehicleTransmission
					};

				if (shipment.JS_PackingMode == Enterprise.Core.Constants.ContainerModes.RollOnRollOff)
				{
					this.packLinesContol.Grid.AddToAvailableColumns(vehicleColumnList);
				}
				else
				{
					this.packLinesContol.Grid.RemoveFromAvailableColumns(vehicleColumnList);
				}
			}
		}

		void OnUpdatePackLineButtonClick(object sender, EventArgs e)
		{
			var packLine = this.packLinesContol
				.Grid
				.GetCurrent()
				as PackLine;

			if (packLine != null)
			{
				var action = PackLineConfirmDiscrepancyAction.NoAction;
				if (packLine.JL_OriginTransitWarehouseStatus == FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies)
				{
					action = new PackLineConfirmDiscrepanciesDialogProvider().ConditionallyShowDialogAndGetResult(packLine);
				}
				packLine.UpdateTotalsFromPkgPackageCollection(action);
			}
		}

		void ChangeImperialWeightAndVolumeVisibility(object sender, EventArgs e)
		{
			SetImperialWeightAndVolumeVisibility();
		}

		void SetNMFCVisibility()
		{
			ForwardingShipment shipment = CurrentDataItem as ForwardingShipment;
			if (shipment != null)
			{
				string columnName = "CommodityCode+RH_FN_NKNMFC";
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Enterprise.Core.Constants.CountryCodes.UnitedStates)
				{
					packLinesContol.Grid.SetAvailability(shipment.IsRoad, columnName);
				}
				else
				{
					packLinesContol.Grid.SetAvailability(false, columnName);
				}
			}
		}

		void SetImperialWeightAndVolumeVisibility()
		{
			ForwardingShipment shipment = CurrentDataItem as ForwardingShipment;
			if (shipment != null)
			{
				JS_ActualWeight_ImperialBoundTextBox.Visible = Enterprise.Core.Constants.Weight.IsImperial(shipment.JS_UnitOfWeight);
			}
		}

		void SetLoadingMetersVisibility()
		{
			ForwardingShipment shipment = CurrentDataItem as ForwardingShipment;
			if (shipment != null)
			{
				JL_LoadingMetersCalcDropEdit.Visible = shipment.IsRoadLoadingMetersEnabled;
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetPackLineCustomText();
			SetImperialWeightAndVolumeVisibility();
			SetupControlsBasedOnTransportMode();
			SetupControlsBasedOnPackingMode();
		}

		#endregion

		#region Pack Lines Management

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			ForwardingShipment shipment = CurrentDataItem as ForwardingShipment;
			if (shipment != null)
			{
				if (ConsolsListManager != null)
				{
					ConsolsListManager.PositionChanged -= new EventHandler(SelectedConsolChanged);
				}

				if (shipment != null)
				{
					shipment.JS_TransportModeInfo.ValueChanged -= JS_TransportModeInfo_ValueChanged;
					shipment.JS_UnitOfWeightInfo.ValueChanged -= ChangeImperialWeightAndVolumeVisibility;
					shipment.JS_PackingModeInfo.ValueChanged -= JS_PackingModeInfo_ValueChanged;
				}
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			ForwardingShipment shipment = CurrentDataItem as ForwardingShipment;
			if (shipment != null)
			{
				if (ConsolsListManager != null)
				{
					ConsolsListManager.PositionChanged += new EventHandler(SelectedConsolChanged);
				}

				if (shipment != null)
				{
					shipment.JS_TransportModeInfo.ValueChanged += JS_TransportModeInfo_ValueChanged;
					shipment.JS_UnitOfWeightInfo.ValueChanged += ChangeImperialWeightAndVolumeVisibility;
					shipment.JS_PackingModeInfo.ValueChanged += JS_PackingModeInfo_ValueChanged;
					SetupControlsBasedOnTransportMode();
					SetupControlsBasedOnPackingMode();
				}
			}
		}

		CurrencyManager ConsolsListManager
		{
			get { return (CurrencyManager)GetBindingManager("AllMasterConsols"); }
		}

		void SelectedConsolChanged(object sender, EventArgs e)
		{
			if (JobConsolBoundGrid != null && JobConsolBoundGrid.ListManager != null)
			{
				if (JobConsolBoundGrid.ListManager.Position < 0)
				{
					Shipment.OuterPackLines.CurrentConsol = null;
				}
				else
				{
					Shipment.OuterPackLines.CurrentConsol = (ForwardingConsol)JobConsolBoundGrid.ListManager.GetCurrent();
				}
			}
		}

		#endregion

		#region Harmonised System Codes Management

		void HSCodeManagementLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			harmonisedCodeManager?.ShowMultipleItemForm();
		}

		void HSCountryFindBox_Resize(object sender, EventArgs e)
		{
			HSCodeFindBox.Left = ControlDpiScalingHelper.ScaleToCurrentDpiX(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(HSCountryFindBox.Left) + ControlDpiScalingHelper.UnscaleFromCurrentDpiX(HSCountryFindBox.Width) + 2);
		}

		void HSCountryFindBox_TextChanged(object sender, EventArgs e)
		{
			UpdateHSCountry();
		}

		void UpdateHSCountry()
		{
			var countryCode = HSCountryFindBox.CodeBox.Text.ToUpper(CultureInfo.CurrentCulture);
			var customsCountryOfJurisdiction = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode);
			HSCodeFindBox.GetCountryCode = () => countryCode;
			HSCodeFindBox.GetDataGrouping = () => customsCountryOfJurisdiction;

			var refCountry = Shipment?.Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, countryCode);
			if (refCountry == null)
			{
				HSCodeFindBox.ErrorForUnsupportedCountry = Res.GetString("e84dce88-c64d-4a7b-84fd-e25a496e2560", "A valid Country/Region must be specified for Tariff lookup.");
			}
			else
			{
				HSCodeFindBox.ErrorForUnsupportedCountry = Res.GetString("affe4d05-70b0-48ec-8947-c6a0d9e96271", "Tariff lookup is not supported for country/region {0}. Enter the WCO Harmonized Code or enter the country/region specific HS code manually.", countryCode);
			}
		}

		#endregion

		#region Dangerous Goods Management

		void DGManagementLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			undgManager?.ShowMultipleItemForm();
		}

		#endregion
	}
}
