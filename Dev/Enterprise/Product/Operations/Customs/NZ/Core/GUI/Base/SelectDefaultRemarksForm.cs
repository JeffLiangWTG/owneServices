using System;
using Enterprise.Customs.NZ.Business.MessageBuilders;
using Enterprise.Customs.NZ.Registry;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI.Base
{
	public partial class SelectDefaultRemarksForm : ZChildForm
	{
		public SelectDefaultRemarksForm(MessageManagerForClearance messageManager)
			: base(messageManager)
		{
			LoadRemarks();
		}

		void LoadRemarks()
		{
			RemarksListBox.Items.AddRange(NZCustomsDataRegistry.Instance.DefaultResendingRemarks.Value);
		}

		protected MessageManagerForClearance MessageManager
		{
			get { return (MessageManagerForClearance)base.BusinessEntity; }
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		void CancelBtn_Click(object sender, EventArgs e)
		{
			Close();
		}

		void OkButton_Click(object sender, EventArgs e)
		{
			MessageManager.EnteredRemarks = (string)RemarksListBox.SelectedItem;
			Close();
		}
	}
}
