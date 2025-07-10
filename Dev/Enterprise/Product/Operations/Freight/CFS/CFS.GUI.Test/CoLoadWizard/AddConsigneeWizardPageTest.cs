using System;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.Freight.CFS.GUI.Testing
{
	sealed class AddConsigneeWizardPageTest : WizardPageTest
	{
		protected override Type GetExpectedWizardType()
		{
			return typeof(AddConsigneeWizardPage);
		}

		protected override CoLoadWizardSteps GetExpectedWizardStep()
		{
			return CoLoadWizardSteps.AddConsignee;
		}
	}
}
