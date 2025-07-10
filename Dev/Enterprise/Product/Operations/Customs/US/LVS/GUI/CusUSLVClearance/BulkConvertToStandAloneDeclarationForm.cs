using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.LVS.GUI
{
	public partial class BulkConvertToStandAloneDeclarationForm : ZChildForm
	{
		public BulkConvertToStandAloneDeclarationForm(CusUSLVClearance shipment) : base(shipment)
		{
			InitializeComponent();
			((IPostingButtonsProvider)this).AssignButtonsInternal(null, buttonCancel, buttonSend);
			shipment.NonApplicableConsignments.ConvertSelectionChanged += NonApplicableConsignments_ConvertSelectionChanged;
		}

		void NonApplicableConsignments_ConvertSelectionChanged(object sender, ConvertSelectionChangedEventArgs e)
		{
			checkedChangedEventSuspended = true;
			var shouldEnableSendButton = true;
			try
			{
				if (e.SelectedAll)
				{
					checkBoxSelectAll.CheckState = CheckState.Checked;
				}
				else if (e.SelectedNone)
				{
					shouldEnableSendButton = false;
					checkBoxSelectAll.CheckState = CheckState.Unchecked;
				}
				else
				{
					checkBoxSelectAll.CheckState = CheckState.Indeterminate;
				}
				buttonSend.Enabled = shouldEnableSendButton;
			}
			finally
			{
				checkedChangedEventSuspended = false;
			}
		}

		bool checkedChangedEventSuspended;

		public new CusUSLVClearance BusinessEntity => (CusUSLVClearance)base.BusinessEntity;

		protected override void OnApplyButtonClick(object sender, EventArgs e)
		{
			try
			{
				var consignmentsToConvert = BusinessEntity.NonApplicableConsignments.Where(c => c.ShouldConvertToStandaloneDeclaration).ToArray();
				ConvertToStandAloneDeclarationHelper.ConvertToStandAloneDeclarationIndividual(BusinessEntity.Factory, consignmentsToConvert, BusinessEntity.Logs, false);
				Close();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError(ex.GetInnermostException().Message);
			}
		}

		public override string FormVerb => string.Empty;

		void CheckBoxSelectAll_CheckedChanged(object sender, EventArgs e)
		{
			if (!checkedChangedEventSuspended)
			{
				gridHouseBills.SuspendLayout();
				if (checkBoxSelectAll.Checked)
				{
					BusinessEntity.NonApplicableConsignments.SelectedAll();
					buttonSend.Enabled = true;
				}
				else
				{
					BusinessEntity.NonApplicableConsignments.UnSelectedAll();
					buttonSend.Enabled = false;
				}
				gridHouseBills.ResumeLayout(false);
				gridHouseBills.PerformLayout();
			}
		}
	}

	public class GridWithoutSettingHasChanges : ZGrid
	{
		protected override void NotifyColumnEditStart()
		{
		}
	}
}
