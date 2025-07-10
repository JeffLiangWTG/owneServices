using System;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruiter.GUI
{
	public partial class GlbAccreditationGroupForm : ZTemplateForm
	{
		public GlbAccreditationGroupForm(GlbAccreditationGroup accreditation)
			: base(accreditation)
		{
			InitializeComponent();
		}

		protected override bool SupportsEDocs => false;

		protected override bool ShowNotesTab => false;

		public override string FormCaption => Res.GetString("68c9afca-31e7-4070-a042-408d06374440", "Accreditation Program");

		protected void PreReqGrid_DoubleClick(object sender, EventArgs e)
		{
			if (PreReqGrid.InnerGrid.SelectedElements == null || PreReqGrid.InnerGrid.SelectedElements.Length == 0)
			{
				return;
			}

			var accreditation = PreReqGrid.InnerGrid.SelectedElements[0] as GlbAccreditation;

			if (accreditation != null)
			{
				var controller = ZControllerFactory.Create(ControllerIDs.GlbAccreditation);
				controller.SetFormsModalTo(this);
				controller.ShowChildrenAsDialog = true;
				controller.ShowEditForm(accreditation);
			}
		}
	}
}
