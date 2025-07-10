using System;
using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Customs.Universal.GUI;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class PackLinesContol : ZUserControl
	{
		public PackLinesContol()
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				SetupHSCodeEffectiveDate();
				SetupInnerPacklinesForm();

				RemoveBookingLineIdColumnIfNotEnableAdvOrmFeature();
			}
		}

		void RemoveBookingLineIdColumnIfNotEnableAdvOrmFeature()
		{
			if (!AdvOrmFeatureHelper.IsEnabled)
			{
				var bookingLineColumn = packLinesGrid.ColumnStyles.OfType<ZGridColumnInfo>().FirstOrDefault(column => column.ColumnName == nameof(JobPackLinesSchema.JL_JSL_BookingLine));
				packLinesGrid.ColumnStyles.Remove(bookingLineColumn);
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			if (!DesignModeFinder.IsDesigning)
			{
				var currentCountryCode = GlbCompany.CurrentCompany.Country.Code;
				if (currentCountryCode == Core.Constants.CountryCodes.China
					|| currentCountryCode == Core.Constants.CountryCodes.Taiwan
					|| currentCountryCode == Core.Constants.CountryCodes.HongKong)
				{
					UpdateExportRefNumberHeader();
					Shipment.JS_RL_NKOriginInfo.ValueChanged += (s, args) => UpdateExportRefNumberHeader();
					Shipment.JS_TransportModeInfo.ValueChanged += (s, args) => UpdateExportRefNumberHeader();
				}
			}
		}

		void UpdateExportRefNumberHeader()
		{
			if (Shipment != null)
			{
				var origin = Shipment.JS_RL_NKOrigin;
				if (Shipment.JS_TransportMode == Core.Constants.TransportModes.Sea
					&& (origin.StartsWith(Core.Constants.CountryCodes.China, StringComparison.OrdinalIgnoreCase)
						|| origin.StartsWith(Core.Constants.CountryCodes.Taiwan, StringComparison.OrdinalIgnoreCase)
						|| origin.StartsWith(Core.Constants.CountryCodes.HongKong, StringComparison.OrdinalIgnoreCase)))
				{
					Grid.Columns[PackLine.Schema.JL_ExportRefNumber].ColumnStyle.HeaderText = Res.GetString("c58a0e61-ac25-4928-9f13-86706a19f160", "Shipping Order/Shi Lian Dan");
				}
				else
				{
					Grid.Columns[PackLine.Schema.JL_ExportRefNumber].ColumnStyle.HeaderText = Res.GetString("ac2c15d4-3d12-4578-8fd7-ad3d4d696831", "Export Reference Number");
				}
			}
		}

		public ZGrid Grid
		{
			get { return packLinesGrid; }
		}

		CommonShipment Shipment
		{
			get { return (CommonShipment)this.DataSource; }
		}

		#region HS Code Effective Date

		void SetupHSCodeEffectiveDate()
		{
			var tariffColumnStyleInfo = Grid.GetColumnStyle(PackLine.Schema.JL_HarmonisedCode) as TariffColumnStyleInfo;
			TariffFindHelper.AddDefaultPropertyToTariffControl(tariffColumnStyleInfo, Shipment);
		}

		void SetupInnerPacklinesForm()
		{
			var menuItem = new ZMenuItem(Res.GetString("e4424249-70e9-ca97-4713-08d832e7eeb8", "Inner Packages"), InnerPackagesMenuItem_Click);
			var packLinesGrid = Grid;
			packLinesGrid.ContextMenu.MenuItems.Add("-");
			packLinesGrid.ContextMenu.MenuItems.Add(menuItem);
		}

		void InnerPackagesMenuItem_Click(object sender, EventArgs e)
		{
			var selectedPackLines = Grid.SelectedElements;
			if (selectedPackLines.Length != 1)
			{
				Globals.Message.ShowError(Res.GetString("8a5c3574-7ecf-6bbf-4eb8-afad1ac98d17", "Please select a pack line."));
				return;
			}

			var outerPackLine = Grid.GetCurrent() as ForwardingPackLine;
			ZFormModaliser.ShowDialogAndDispose(new InnerPackLinesOfOuterPackLineForm(outerPackLine));
		}

		#endregion
	}
}
