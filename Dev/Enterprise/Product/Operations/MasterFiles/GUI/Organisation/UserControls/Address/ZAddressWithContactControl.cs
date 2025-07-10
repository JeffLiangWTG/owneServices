using System;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI.Organisation.UserControls.Address
{
	public partial class ZAddressWithContactControl : ZOrgAddressControl
	{
		public ZAddressWithContactControl()
		{
			InitializeComponent();
			DataSourceType = typeof(ZAddressWithContact);
			invoiceContactContactControl = new ContactControl();
			invoiceContactContactControl.DataSourceType = typeof(ZAddressWithContact);
			invoiceContactContactControl.CaptionRenderingEnabled = true;
			BindingSource.SetBindingMember(invoiceContactContactControl, ".");
			InvoiceContactTabPage.Controls.Add(invoiceContactContactControl);
			invoiceContactContactControl.Dock = DockStyle.Fill;
		}
		readonly ContactControl invoiceContactContactControl;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (OnlyStopOnDebtor)
			{
				invoiceContactContactControl.TabStop = false;
			}
		}

		public bool ContactInfoTabVisible
		{
			get { return ContactInfoTab.TabVisible; }
			set { ContactInfoTab.TabVisible = value; }
		}

		public ResourceStringData InvoiceContactTabCaption
		{
			get { return InvoiceContactTabPage.CaptionResourceString; }
			set { InvoiceContactTabPage.CaptionResourceString = value; }
		}
	}
}
