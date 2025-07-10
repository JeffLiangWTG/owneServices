using System;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.Freight.CFS.GUI.Testing
{
	sealed class CaptureUltimateConsignmentDetailsTest : WizardPageTest
	{
		protected override Type GetExpectedWizardType()
		{
			return typeof(CaptureUltimateConsignmentDetails);
		}

		protected override CoLoadWizardSteps GetExpectedWizardStep()
		{
			return CoLoadWizardSteps.UltimateDetails;
		}
	}
}
