using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class CYDMaintenanceUserControl : ZUserControl
	{
		public CYDMaintenanceUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			this.CedexRadioButton.Checked = IsCedex;
			this.MercRadioButton.Checked = IsMerc;
		}

		void MRCodeGroupRadioButton_CheckedChanged(object sender, EventArgs e)
		{
			if (CedexRadioButton.Checked)
			{
				Org.CompanyData.OB_YardMNRCodeGroup = "CDX";
			}
			if (MercRadioButton.Checked)
			{
				Org.CompanyData.OB_YardMNRCodeGroup = "MRC";
			}
		}

		bool IsCedex => Org.CompanyData.OB_YardMNRCodeGroup == "CDX";

		bool IsMerc => Org.CompanyData.OB_YardMNRCodeGroup == "MRC";

		OrgHeader ParentOrg;
		OrgHeader Org
		{
			get
			{
				if (ParentOrg == null)
				{
					ParentOrg = (OrgHeader)((ZForm)ParentForm).BusinessEntity;
				}
				return ParentOrg;
			}
		}
	}
}
