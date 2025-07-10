using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Enterprise.ComplianceRisk.Integration;

namespace Enterprise.DeniedPartyScreening.Integration
{
	public interface IDeniedPartyScreeningPresentationManager
	{
		Task PerformScreening(object parentForm, List<IDpsSourceWithParties> sourceBizOs, IScreeningParty[] screeningParties, bool isScreeningEntity, bool forceAllComplianceLists, bool suppressDeveloperException = false, Func<bool> parentEntityHasChanges = null, IComplianceRiskAction complianceRiskAction = null);
	}
}
