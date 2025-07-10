using System;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AccOrgTaxConfigurationTemplateForm : ZForm, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		public AccOrgTaxConfigurationTemplateForm(AccOrgTaxConfigurationTemplate businessEntity) : base(businessEntity)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
			Saved += AccOrgTaxConfigurationTemplateForm_Saved;
		}

		AccOrgTaxConfigurationTemplate Template => BusinessEntity as AccOrgTaxConfigurationTemplate;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		void AccOrgTaxConfigurationTemplateForm_Saved(object sender, EventArgs e)
		{
			FilterLinkedOrganizationsControl.ResetStatus();
		}

		public bool AllowTabBackward(Control control, Control previousControl)
		{
			return (previousControl is LinkedOrganizationsControl) || (control is LinkedOrganizationsFilterControl);
		}

		public override string FormCaption => Template != null && Template.OCT_IsReceivable ?
			AccountingMasterFilesConstants.AccOrgTaxConfigurationTemplateTypes.ReceivablesOrganizationsTemplate.Description :
			AccountingMasterFilesConstants.AccOrgTaxConfigurationTemplateTypes.PayablesOrganizationsTemplate.Description;
	}
}
