using System;
using System.Windows.Forms;
using Enterprise.eTail.Business.DeniedPartyScreening;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.eTail.GUI
{
	public partial class HVLVDpsClearingReasonForm : ZChildForm
	{
		public HVLVDpsClearingReasonForm(HVLVDpsClearingReason clearingReason) : base(clearingReason)
		{
			InitializeComponent();
		}

		HVLVDpsClearingReason ClearingReason => (HVLVDpsClearingReason)base.BusinessEntity;

		void SaveButton_Click(object sender, EventArgs e)
		{
			ClearingReason.Validation.ValidateAll();

			if (ClearingReason.HasErrors)
			{
				return;
			}

			DialogResult = DialogResult.OK;
			Close();
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}
