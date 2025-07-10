using System;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.Freight.CFS.GUI.Testing
{
	sealed class ConfirmCreateSubHouseWizardPageTest : WizardPageTest
	{
		protected override Type GetExpectedWizardType()
		{
			return typeof(ConfirmCreateSubHouseWizardPage);
		}

		protected override CoLoadWizardSteps GetExpectedWizardStep()
		{
			return CoLoadWizardSteps.AddColoadShipment;
		}
	}
}
