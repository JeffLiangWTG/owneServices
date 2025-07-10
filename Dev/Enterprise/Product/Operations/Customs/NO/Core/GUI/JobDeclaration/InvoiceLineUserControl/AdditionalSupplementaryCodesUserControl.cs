using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI
{
	partial class AdditionalSupplementaryCodesUserControl : ZUserControl, IExtendedControl, IResourceStringBindingMember
	{
		public AdditionalSupplementaryCodesUserControl()
		{
			InitializeComponent();
			SetEditButtonCaption();
			extensions = new DefaultControlExtensionCollection(this);
		}
		readonly DefaultControlExtensionCollection extensions;

		Control IExtendedControl.Host => this;

		IControlExtensionCollection IExtendedControl.Extensions => extensions;

		string IResourceStringBindingMember.ResourceStringBindingMember => nameof(JobComInvoiceLine.JI_AdditionalSupplements);

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				extensions.Dispose();
			}
			base.Dispose(disposing);
		}

		void AdditionalSupplementaryCodesEditButton_Click(object sender, EventArgs e)
		{
			if (CurrentDataItem is ISupplementaryCodeSupporter supplementaryCodeSupporter)
			{
				ZFormModaliser.ShowDialogAndDispose(new AdditionalSupplementaryCodesForm(supplementaryCodeSupporter));
			}
		}

		void SetEditButtonCaption()
		{
			AdditionalSupplementaryCodesEditButton.CaptionResourceString = Res.GetData("dc9ba692-3fa5-484f-a217-ba44d4b893f2",
				"More..",
				"Select excise codes");
		}
	}
}
