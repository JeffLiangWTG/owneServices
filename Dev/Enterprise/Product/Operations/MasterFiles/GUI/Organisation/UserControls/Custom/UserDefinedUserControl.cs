using System;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	public partial class UserDefinedUserControl : OrganisationSecurityContainerControl, IReadOnlyToggleControl
	{
		public UserDefinedUserControl()
		{
			InitializeComponent();
		}

		#region Binding

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			UserDefinedCustomLabels.BindToMember = "MiscServ";
			UserDefinedCustomLabels.CustomLabelsProvider = Organisation;
			DocumentLogoButton.Enabled = OrderStatusListEditButton.Enabled = OrderLineStatusListEditButton.Enabled =
				(Organisation != null && Organisation.SecurityProvider.HasModifyCustomSecurity);
		}

		OrgHeader Organisation
		{
			get { return (OrgHeader)CurrentDataItem; }
		}

		#endregion

		#region IReadOnlyToggleControl Members

		public bool ReadOnly
		{
			get { return readOnly; }
			set
			{
				readOnly = value;
				DocumentLogoButton.ReadOnly = value;
				OrderStatusListEditButton.ReadOnly = value;
				OrderLineStatusListEditButton.ReadOnly = value;
			}
		}
		bool readOnly;

		#endregion

		#region Button Clicks

		void OrderStatusListEditButton_Click(object sender, EventArgs e)
		{
			using (CodeDescriptionListEditForm form = new CodeDescriptionListEditForm(Res.GetString("d74e2e9f-5893-404f-83b2-1ce5a321b6ce", "Customize Order Status List"), JobOrderHeaderSchema.JD_OrderStatus.MaxLength))
			{
				form.CodeDescriptionList = Organisation.MiscServ.OrderStatusList;
				if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
				{
					Organisation.MiscServ.OrderStatusList = form.CodeDescriptionList;
				}
			}
		}

		void OrderLineStatusListEditButton_Click(object sender, EventArgs e)
		{
			using (CodeDescriptionListEditForm form = new CodeDescriptionListEditForm(Res.GetString("5e749aab-2d86-444b-8553-8a75d5db6d8f", "Customize Order Line Status List"), JobOrderLineSchema.JO_LineStatus.MaxLength))
			{
				form.CodeDescriptionList = Organisation.MiscServ.OrderLineStatusList;
				if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
				{
					Organisation.MiscServ.OrderLineStatusList = form.CodeDescriptionList;
				}
			}
		}

		void DocumentLogoButton_Click(object sender, EventArgs e)
		{
			using (ImageSelectionForm form = new ImageSelectionForm())
			{
				form.Logo = Organisation.MiscServ.ClientDocumentLogo;
				if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
				{
					Organisation.MiscServ.ClientDocumentLogo = form.Logo;
				}
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
					UserDefinedCustomLabels.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
