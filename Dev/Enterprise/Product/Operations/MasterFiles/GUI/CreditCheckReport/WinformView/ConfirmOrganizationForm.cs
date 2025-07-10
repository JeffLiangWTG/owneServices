using System;
using System.Windows.Forms;
using CargoWise.BrandManager;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ConfirmOrganizationForm : KForm, ICaptionRenderingSupport
	{
		public ConfirmOrganizationForm(CompanyLookupModel companyLookupModel)
		{
			InitializeComponent();
			Icon = BrandingFactory.Instance.ProductIcon;
			Text = Res.GetString("D7970139-1C71-472C-8007-5B6D7EFA8292", "Confirm the organization");
			CompanyLookupModel = companyLookupModel;

			foreach (var companyModel in companyLookupModel.CompanyLookupItemModels)
			{
				var organizationInfo = new OrganizationInfoControl();
				organizationInfo.SetDataBinding(companyModel, string.Empty);
				organizationInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
				orgsListKTableLayout.Controls.Add(organizationInfo);
			}
		}

		public bool NeedToGetReport { get; private set; }

		internal CompanyLookupModel CompanyLookupModel { get; }

		void CancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void ConfirmButton_Click(object sender, EventArgs e)
		{
			NeedToGetReport = true;
			Close();
		}

		#region ICaptionRenderingSupport

		bool? ICaptionRenderingSupport.CaptionRenderingEnabled
		{
			get { return true; }
		}

		event EventHandler ICaptionRenderingSupport.CaptionRenderingEnabledChanged
		{
			add { }
			remove { }
		}

		#endregion
	}
}
