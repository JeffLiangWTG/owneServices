using System;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI.Declaration
{
	public partial class CusEntryLineSyncrhoniserForm : ZChildForm
	{
		public CusEntryLineSyncrhoniserForm(CusEntryLineSyncroniser syncroniser)
			: base(syncroniser)
		{
			this.syncroniser = syncroniser;
		}
		readonly CusEntryLineSyncroniser syncroniser;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			syncroniser.SyncAllInvoiceLinesOnEntryLine();
			Close();
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		public override string FormCaption => ResString.GetMultilingualString("ec886779-9de4-4e5f-a0a5-2c0c0e74b2fd", "Modify Codes");
	}
}
