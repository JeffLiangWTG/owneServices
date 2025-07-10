using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.eTail.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.eTail.GUI
{
	public partial class HVLVSelectConsignmentForm : ZChildForm
	{
		public HVLVSelectConsignmentForm(HVLVCommonConsigneeConsignmentCollection commonConsigeeConsignmentCollection)
			: base(commonConsigeeConsignmentCollection)
		{
			InitializeComponent();
			FormatCaptionResourceStringsWithParentConsignmentId(commonConsigeeConsignmentCollection);
		}

		void FormatCaptionResourceStringsWithParentConsignmentId(HVLVCommonConsigneeConsignmentCollection commonConsigeeConsignmentCollection)
		{
			CaptionResourceString = CaptionResourceString.Format(commonConsigeeConsignmentCollection.ParentConsignment.HVC_ConsignmentId);
			zLabel1.CaptionResourceString = zLabel1.CaptionResourceString.Format(commonConsigeeConsignmentCollection.ParentConsignment.HVC_ConsignmentId);
		}

		public IEnumerable<HVLVConsignment> SelectedConsignments => gridConsignments.SelectedElements.OfType<HVLVConsignment>();

		void ButtonClick_Merge(object sender, EventArgs e)
		{
			if (SelectedConsignments.Any())
			{
				DialogResult = DialogResult.OK;
			}
			else
			{
				Globals.Message.ShowError(ResString.GetMultilingualString("eb7788e4-5265-4756-a668-8f409d05efdf", "Please select a consignment to merge first."));
			}
		}

		void ButtonClick_Ignore(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

		void GridConsignments_SelectedRowsChangedInMouseDown(object sender, EventArgs e)
		{
			btnMerge.Enabled = SelectedConsignments.Any();
		}

		public override string FormVerb => string.Empty;
	}
}
