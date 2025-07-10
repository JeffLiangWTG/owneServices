using System;
using System.ComponentModel;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class ZOrganisationControlForTesting : ZOrganisationControl
	{
		public new void ShowWebLink(string target)
		{
			base.ShowWebLink(target);
		}

		public new OrgAddress OrgAddress
		{
			get { return base.OrgAddress; }
		}

		public new string OrgAddressFormatted
		{
			get { return base.OrgAddressFormatted; }
		}

		public new ZLinkLabel ContactsLink
		{
			get { return base.ContactsLink; }
		}

		public new ZLinkLabel AddressesLink
		{
			get { return base.AddressesLink; }
		}

		public new void AddressLabel_MouseHover(object sender, EventArgs e)
		{
			base.AddressLabel_MouseHover(sender, e);
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new bool IsAddressLabelToolTipRequired
		{
			get { return base.IsAddressLabelToolTipRequired; }
		}

		public new ZDocAdditionalAddressInfoControl AdditionalAddressInfoControl
		{
			get { return base.AdditionalAddressInfoControl; }
		}
	}
}
