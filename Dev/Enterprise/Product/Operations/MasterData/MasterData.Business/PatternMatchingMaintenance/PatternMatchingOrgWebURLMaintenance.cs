using System;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class PatternMatchingOrgWebURLMaintenance : PatternMatchingSource<OrgWebURL>
	{
		public PatternMatchingOrgWebURLMaintenance(OrgWebURL bizO)
			: base(bizO)
		{
		}

		protected override bool CreateOrUpdateDomain()
		{
			var url = bizO.PU_URL;
			var domainToHash = TextStandardizerHelper.StandardizeUrlDomain(url);
			var patternMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingDomain>(bizO, false, GetHash(domainToHash), bizO.Header.PK, Guid.Empty, bizO.Header.CountryCode);

			return patternMaintenance.CreateOrUpdatePatternRecord(PatternMatchingDomainSchema.PMD_ParentId, bizO.PK);
		}

		protected override void QueueMasterForProcessing()
		{
			QueueOrgForDeduplicationProcessing(bizO.Header);
		}
	}
}
