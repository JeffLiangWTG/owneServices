using System;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.Freight.CFS.GUI.Testing
{
	sealed class AddConsignorWizardPageTest : WizardPageTest
	{
		protected override Type GetExpectedWizardType()
		{
			return typeof(AddConsignorWizardPage);
		}

		protected override CoLoadWizardSteps GetExpectedWizardStep()
		{
			return CoLoadWizardSteps.AddConsignor;
		}
	}
}
