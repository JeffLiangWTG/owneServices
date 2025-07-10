using System;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class DivideAllLegacyEstimatedValuesByTwelveController_WithAConcurrentDivideAllLegacyEstimatedValuesBy12 : DivideAllLegacyEstimatedValuesByTwelveController
	{
		protected override void DivideAllLegacyEstimatedValuesByTwelve()
		{
			// Emulate a diffrent user running the divide function before this divide
			// (i.e. after this user has been shown the 'Divide?' confirmation popup, but before the actual dividing has occured)
			OrganisationsDataRegistry.Instance.DivideAllOpportunityValuesByTwelveHasRun.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			base.DivideAllLegacyEstimatedValuesByTwelve();
		}
	}
}
