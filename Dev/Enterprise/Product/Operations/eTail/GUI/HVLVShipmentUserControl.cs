using System;
using System.Linq;
using CargoWise.Definitions.Ecommerce;
using Enterprise.eTail.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.eTail.GUI
{
	public partial class HVLVShipmentUserControl : ZUserControl
	{
		public HVLVShipmentUserControl()
		{
			InitializeComponent();
			BindingSource.DataSourceChanged += UpdateRadiouttonBindings;
			VisibleChanged += CountFields_VisibleUpdated;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			SetCountFieldsVisible();
		}

		#region Control Visibility

		void CountFields_VisibleUpdated(object sender, EventArgs e)
		{
			SetCountFieldsVisible();
		}

		void SetCountFieldsVisible()
		{
			var header = (HVLVConsignmentHeader)DataSource;

			if (header != null)
			{
				var showImport = header.ShowImport;
				var showExport = header.ShowExport;

				lblImportClearedCount.Visible = showImport;
				lblImportHeldCount.Visible = showImport;
				lblImportNoneReportedCount.Visible = showImport;
				rbImportClearedCount.Visible = showImport;
				rbImportHeldCount.Visible = showImport;
				rbImportNoneReportedCount.Visible = showImport;

				lblExportClearedCount.Visible = showExport;
				lblExportHeldCount.Visible = showExport;
				lblExportNoneReportedCount.Visible = showExport;
				rbExportClearedCount.Visible = showExport;
				rbExportHeldCount.Visible = showExport;
				rbExportNoneReportedCount.Visible = showExport;
			}
		}

		#endregion

		public void UpdateVolumeWeightTextBox(bool isChargableByWeight)
		{
			var volumneWeightTextBox = consignmentUserControl.VolumeWeightTextBox;
			var hvlvItemsGrid = consignmentUserControl.HVLVItemsGrid;

			if (isChargableByWeight)
			{
				volumneWeightTextBox.CaptionResourceString = Res.GetData("9f176752-78ef-4346-b8cb-8144ec370caf", "Volume Weight", "Volume Wt.", "VW", "");
				hvlvItemsGrid.SetColumnCaption(nameof(HVLVItem.VolumeWeightForDisplay), Res.GetString("5378d7ea-54d3-4c65-846a-e39d8e07f4d2", "Volume Weight"));
			}
			else
			{
				volumneWeightTextBox.CaptionResourceString = Res.GetData("712ca4f7-df49-4c0f-a694-26fdfe27808b", "Weight Volume", "Weight Vol.", "WV", "");
				hvlvItemsGrid.SetColumnCaption(nameof(HVLVItem.VolumeWeightForDisplay), Res.GetString("b11ba22d-e6a9-4929-b844-875aa82f6f3b", "Weight Volume"));
			}

			volumneWeightTextBox.UpdateCaption();
		}

		void UpdateRadiouttonBindings(object sender, EventArgs args)
		{
			if (BindingSource.DataSource != null)
			{
				foreach (var rb in groupBoxCountProperties.Controls.OfType<ToggleRadioButton>())
				{
					rb.DataBindings.Clear();
					rb.DataBindings.Add((NoResString)"Text", BindingSource.DataSource, rb.Name); // Control property name
				}
			}
		}

		void UpdateRadioButtonFocusCueAndApplyFilter(object sender, HVLVConsignmentInMemoryFilter filter = null)
		{
			if (sender is ToggleRadioButton rb)
			{
				if (rb.Checked)
				{
					rb.FlatAppearance.BorderSize = SelectedBorderSize;
					rb.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
					consignmentUserControl.ApplyFilter(filter);
				}
				else
				{
					rb.FlatAppearance.BorderSize = DefaultBorderSize;
					rb.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular);
					consignmentUserControl.ApplyFilter();
				}
			}
		}

		void rbItemCount_CheckedChanged(object sender, EventArgs e) => UpdateRadioButtonFocusCueAndApplyFilter(sender);

		void rbImportClearedCount_CheckedChanged(object sender, EventArgs e) => UpdateRadioButtonFocusCueAndApplyFilter(sender, consignment => consignment.HVC_ImportReleaseStatus == HVLVReleaseStatus.Cleared);

		void rbImportHeldCount_CheckedChanged(object sender, EventArgs e) => UpdateRadioButtonFocusCueAndApplyFilter(sender, consignment => consignment.HVC_ImportReleaseStatus == HVLVReleaseStatus.Held);

		void rbImportNoneReportedCount_CheckedChanged(object sender, EventArgs e) => UpdateRadioButtonFocusCueAndApplyFilter(sender, consignment => consignment.HVC_ImportReleaseStatus == HVLVReleaseStatus.None);

		void rbExportClearedCount_CheckedChanged(object sender, EventArgs e) => UpdateRadioButtonFocusCueAndApplyFilter(sender, consignment => consignment.HVC_ExportReleaseStatus == HVLVReleaseStatus.Cleared);

		void rbExportHeldCount_CheckedChanged(object sender, EventArgs e) => UpdateRadioButtonFocusCueAndApplyFilter(sender, consignment => consignment.HVC_ExportReleaseStatus == HVLVReleaseStatus.Held);

		void rbExportNoneReportedCount_CheckedChanged(object sender, EventArgs e) => UpdateRadioButtonFocusCueAndApplyFilter(sender, consignment => consignment.HVC_ExportReleaseStatus == HVLVReleaseStatus.None);

		void rbSurplusCount_CheckedChanged(object sender, EventArgs e)
		{
			HVLVConsignmentInMemoryFilter filter = consignment =>
			{
				return consignment.ActiveItems.Any(item => item.HVI_Status == HVLVItemStatus.Codes.SurplusAtDestinationDepot);
			};
			UpdateRadioButtonFocusCueAndApplyFilter(sender, filter);
		}

		void rbShortCount_CheckedChanged(object sender, EventArgs e)
		{
			HVLVConsignmentInMemoryFilter filter = consignment =>
			{
				return consignment.ActiveItems.Any(item => item.HVI_Status == HVLVItemStatus.Codes.ShortShippedAtDestinationDepot);
			};
			UpdateRadioButtonFocusCueAndApplyFilter(sender, filter);
		}

		void rbDeliveredCount_CheckedChanged(object sender, EventArgs e)
		{
			HVLVConsignmentInMemoryFilter filter = consignment =>
			{
				return consignment.ActiveItems.Any(item => item.HVI_Status == HVLVItemStatus.Codes.Delivered);
			};
			UpdateRadioButtonFocusCueAndApplyFilter(sender, filter);
		}

		void rbScannedCount_CheckedChanged(object sender, EventArgs e)
		{
			HVLVConsignmentInMemoryFilter filter = consignment =>
			{
				return consignment.ActiveItems.Any(item => item.HVI_IsScannedAtDestination);
			};
			UpdateRadioButtonFocusCueAndApplyFilter(sender, filter);
		}

		void rbScannedCleared_CheckedChanged(object sender, EventArgs e)
		{
			HVLVConsignmentInMemoryFilter filter = consignment =>
			{
				return consignment.HVC_ImportReleaseStatus == HVLVReleaseStatus.Cleared
					&& consignment.ActiveItems.Any(item => item.HVI_IsScannedAtDestination);
			};
			UpdateRadioButtonFocusCueAndApplyFilter(sender, filter);
		}

		void rbScannedHeld_CheckedChanged(object sender, EventArgs e)
		{
			HVLVConsignmentInMemoryFilter filter = consignment =>
			{
				return consignment.HVC_ImportReleaseStatus == HVLVReleaseStatus.Held
					&& consignment.ActiveItems.Any(item => item.HVI_IsScannedAtDestination);
			};
			UpdateRadioButtonFocusCueAndApplyFilter(sender, filter);
		}

		void rbScannedNoneReported_CheckedChanged(object sender, EventArgs e)
		{
			HVLVConsignmentInMemoryFilter filter = consignment =>
			{
				return consignment.HVC_ImportReleaseStatus == HVLVReleaseStatus.None
					&& consignment.ActiveItems.Any(item => item.HVI_IsScannedAtDestination);
			};
			UpdateRadioButtonFocusCueAndApplyFilter(sender, filter);
		}

		const int DefaultBorderSize = 1;
		const int SelectedBorderSize = 2;
	}
}
