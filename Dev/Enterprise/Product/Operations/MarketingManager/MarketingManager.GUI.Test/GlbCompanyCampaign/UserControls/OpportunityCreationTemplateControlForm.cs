using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI.Test.GlbCompanyCampaign.UserControls
{
	internal partial class OpportunityCreationTemplateControlForm : ZForm
	{
		public OpportunityCreationTemplateControlForm(OpportunityCreationTemplate opportunityCreationTemplate)
			: base(opportunityCreationTemplate)
		{
		}

		internal readonly OpportunityCreationTemplateControl OpportunityCreationTemplateControl = new OpportunityCreationTemplateControl();
	}
}
