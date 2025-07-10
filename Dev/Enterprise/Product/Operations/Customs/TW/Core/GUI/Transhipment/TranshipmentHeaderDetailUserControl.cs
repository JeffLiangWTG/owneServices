using System;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI
{
	public partial class TranshipmentHeaderDetailUserControl : ZUserControl
	{
		public TranshipmentHeaderDetailUserControl()
		{
			InitializeComponent();
			this.Load += TWInBondHeaderDetailUserControl_Load;
		}

		void TWInBondHeaderDetailUserControl_Load(object sender, EventArgs e)
		{
			ResetControlsWidth();
		}

		public const string IsVisibleForBindingString = "IsVisibleForBinding";

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			Import_MasterBillForAirBoundTextBox.DataBindings.RemoveBinding(IsVisibleForBindingString);
			Import_MasterBillForSeaBoundTextBox.DataBindings.RemoveBinding(IsVisibleForBindingString);
			Export_MasterBillForAirBoundTextBox.DataBindings.RemoveBinding(IsVisibleForBindingString);
			Export_MasterBillForSeaBoundTextBox.DataBindings.RemoveBinding(IsVisibleForBindingString);

			if (dataSource is CusInBondHeader header)
			{
				Import_MasterBillForAirBoundTextBox.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "IsReceiptOfficeAir", false, DataSourceUpdateMode.Never));
				Import_MasterBillForSeaBoundTextBox.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "IsReceiptOfficeSea", false, DataSourceUpdateMode.Never));
				Export_MasterBillForAirBoundTextBox.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "IsUnladingOfficeAir", false, DataSourceUpdateMode.Never));
				Export_MasterBillForSeaBoundTextBox.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "IsUnladingOfficeSea", false, DataSourceUpdateMode.Never));
				header.BH_ReleaseStatusInfo.ValueChanged -= BH_ReleaseStatusInfo_ValueChanged;
				header.BH_ReleaseStatusInfo.ValueChanged += BH_ReleaseStatusInfo_ValueChanged;
			}
		}

		public new CusInBondHeader CurrentDataItem => (CusInBondHeader)base.CurrentDataItem;

		void BH_ReleaseStatusInfo_ValueChanged(object sender, EventArgs e)
		{
			var status = CurrentDataItem?.BH_ReleaseStatus ?? ZString.Empty;
			AllocateEntryNumberButton.Enabled = !(status == ClearanceStatusCodeList.Codes.C1 || status == ClearanceStatusCodeList.Codes.C2 || status == ClearanceStatusCodeList.Codes.C3M || status == ClearanceStatusCodeList.Codes.C3X);
		}

		protected void ResetControlsWidth()
		{
			this.SuspendLayout();
			var widthLength = BH_ReleaseStatusDropEdit.Width - BH_ReleaseStatusDropEdit.DescriptionBox.Width;
			ReceiptOfficeDropEdit.SetControlWidth(widthLength);
			UnladingOfficeDropEdit.SetControlWidth(widthLength);
			BM_InBondEntryTypeDropEdit.SetControlWidth(widthLength);
			BM_ExportTransportModeDropEdit.SetControlWidth(widthLength);
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		void AllocateEntryNumberButton_Click(object sender, EventArgs e)
		{
			if (FindForm() is ZForm mainForm && DataSource is CusInBondHeader cusInBondHeader)
			{
				new AllocateNumberButtonClickEventHandler().Modify(new CusInBondHeaderEntryNumberSupporter(cusInBondHeader), new AllocateEventHandlerArgs
					(cusInBondHeader.IsWaitingForResponse,
					mainForm.BusinessEntityForHasChanges,
					() => mainForm.FireSaveButton()));
			}
		}
	}
}
