using System;
using CargoWise.Windows.UI.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	[SuppressFormDesignerAnalysis]
	public partial class OrganisationContainerControl : ZUserControl
	{
		public OrganisationContainerControl()
		{
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (IsClientIntelligenceForm)
			{
				ToggleClientIntelligenceForm();
			}

			if (IsCompetitorIntelligenceForm)
			{
				ToggleCompetitorIntelligenceForm();
			}
		}

		protected virtual void ToggleClientIntelligenceForm()
		{
		}

		protected virtual void ToggleCompetitorIntelligenceForm()
		{
		}

		bool IsClientIntelligenceForm
		{
			get { return ParentForm is ZClientIntelligenceForm; }
		}

		bool IsCompetitorIntelligenceForm
		{
			get { return ParentForm is ZCompetitorIntelligenceForm; }
		}
	}
}
