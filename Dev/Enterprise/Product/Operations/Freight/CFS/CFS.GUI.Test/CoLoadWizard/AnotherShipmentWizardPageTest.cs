using System;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.Freight.CFS.GUI.Testing
{
	sealed class AnotherShipmentWizardPageTest : WizardPageTest
	{
		protected override Type GetExpectedWizardType()
		{
			return typeof(AnotherShipmentWizardPage);
		}

		protected override CoLoadWizardSteps GetExpectedWizardStep()
		{
			return CoLoadWizardSteps.AnotherShipment;
		}
	}
}
