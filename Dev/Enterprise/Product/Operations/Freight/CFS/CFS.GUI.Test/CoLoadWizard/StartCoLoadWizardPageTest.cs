using System;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.Freight.CFS.GUI.Testing
{
	sealed class StartCoLoadWizardPageTest : WizardPageTest
	{
		protected override Type GetExpectedWizardType()
		{
			return typeof(StartCoLoadWizardPage);
		}

		protected override CoLoadWizardSteps GetExpectedWizardStep()
		{
			return CoLoadWizardSteps.WelcomeStep;
		}
	}
}
