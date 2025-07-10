using System;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	public partial class FCLPackLinesControl : ZUserControl
	{
		public FCLPackLinesControl()
		{
			InitializeComponent();
		}

		#region Additional Colums

		protected override void OnLoad(EventArgs e)
		{
			var currentCountryCode = GlbCompany.CurrentCompany.Country.Code;
			if (currentCountryCode == Core.Constants.CountryCodes.China
				|| currentCountryCode == Core.Constants.CountryCodes.Taiwan
				|| currentCountryCode == Core.Constants.CountryCodes.HongKong)
			{
				UpdateExportRefNumberHeader();

				if (Shipment != null)
				{
					Shipment.JS_RL_NKOriginInfo.ValueChanged += JS_RL_NKOrigin_ValueChanged;
				}
			}
		}

		void JS_RL_NKOrigin_ValueChanged(object sender, EventArgs e)
		{
			UpdateExportRefNumberHeader();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			if (!this.IsDesignMode())
			{
				new CustomFieldColumnCreator().Set(PackLinesGrid, new PackLineCustomFieldsDescriptor());
				new UNDGDataItemFormManager(PackLinesGrid, string.Empty, UNDGDataItemFormManagerConfig.ShowUNDGDetails()).Initialize();
				new HarmonisedCodeFormManager(PackLinesGrid).Initialize();
				TariffFindHelper.AddDefaultPropertyToTariffControl(harmonisedCodeColumnStyleInfo, Shipment);
			}
		}

		#endregion

		#region Hooks For Totals Update

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (Shipment != null)
			{
				UnHookPackLines();
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (Shipment != null)
			{
				HookPackLines();
			}
		}

		AgencyShipment Shipment
		{
			get { return (AgencyShipment)CurrentDataItem; }
		}

		#endregion

		#region Totals

		public bool ShowTotals
		{
			get { return showTotals; }
			set
			{
				if (showTotals != value)
				{
					showTotals = value;
					TotalsBottomPanel.Visible = value;
				}
			}
		}
		bool showTotals = true;

		#region UpdateTotals

		void HookPackLines()
		{
			Shipment.OuterPackLines.CountChanged += PackLines_CountChanged;
			foreach (AgencyShipmentPackLine packLine in Shipment.OuterPackLines)
			{
				HookPackline(packLine);
			}
		}

		void PackLines_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded)
			{
				HookPackline((AgencyShipmentPackLine)e.BizObject);
			}
			else if (e.ItemRemoved)
			{
				UnHookPackline((AgencyShipmentPackLine)e.BizObject);
			}

			UpdateTotals();
		}

		void UnHookPackLines()
		{
			Shipment.OuterPackLines.CountChanged -= PackLines_CountChanged;
			foreach (AgencyShipmentPackLine packLine in Shipment.OuterPackLines)
			{
				UnHookPackline(packLine);
			}
		}

		void HookPackline(AgencyShipmentPackLine packline)
		{
			packline.JL_PackageCountInfo.ValueChanged += UpdateTotals;
			packline.JL_ActualVolumeInfo.ValueChanged += UpdateTotals;
			packline.JL_ActualVolumeUQInfo.ValueChanged += UpdateTotals;
			packline.JL_ActualWeightInfo.ValueChanged += UpdateTotals;
			packline.JL_ActualWeightUQInfo.ValueChanged += UpdateTotals;
		}

		void UnHookPackline(AgencyShipmentPackLine packline)
		{
			packline.JL_PackageCountInfo.ValueChanged -= UpdateTotals;
			packline.JL_ActualVolumeInfo.ValueChanged -= UpdateTotals;
			packline.JL_ActualVolumeUQInfo.ValueChanged -= UpdateTotals;
			packline.JL_ActualWeightInfo.ValueChanged -= UpdateTotals;
			packline.JL_ActualWeightUQInfo.ValueChanged -= UpdateTotals;
		}

		void UpdateTotals(object sender, EventArgs e)
		{
			UpdateTotals();
		}

		void UpdateTotals()
		{
			if (ShowTotals)
			{
				Shipment.TotalOuterPacksInfo.RefreshBinding();
				Shipment.TotalOuterPacksVolumeInfo.RefreshBinding();
				Shipment.TotalOuterPacksWeightInfo.RefreshBinding();
			}
		}

		#endregion

		#endregion

		void UpdateExportRefNumberHeader()
		{
			if (Shipment != null)
			{
				var origin = Shipment.JS_RL_NKOrigin;
				if (origin.StartsWith(Core.Constants.CountryCodes.China, StringComparison.OrdinalIgnoreCase)
					|| origin.StartsWith(Core.Constants.CountryCodes.Taiwan, StringComparison.OrdinalIgnoreCase)
					|| origin.StartsWith(Core.Constants.CountryCodes.HongKong, StringComparison.OrdinalIgnoreCase))
				{
					PackLinesGrid.Columns[AutoJobPackLines.Schema.JL_ExportRefNumber].ColumnStyle.HeaderText = Res.GetString("17fcf1aa-9b45-4b5f-a361-3442f2afd578", "Shipping Order/Shi Lian Dan");
				}
				else
				{
					PackLinesGrid.Columns[AutoJobPackLines.Schema.JL_ExportRefNumber].ColumnStyle.HeaderText = Res.GetString("3e3599a6-2f8b-40bb-b986-9d73c713c741", "Export Reference Number");
				}
			}
		}
	}
}


