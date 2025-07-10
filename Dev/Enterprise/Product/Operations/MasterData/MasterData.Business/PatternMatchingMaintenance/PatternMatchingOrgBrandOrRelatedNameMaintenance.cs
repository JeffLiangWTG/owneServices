using System;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class PatternMatchingOrgBrandOrRelatedNameMaintenance : PatternMatchingSource<OrgBrandOrRelatedName>
	{
		public PatternMatchingOrgBrandOrRelatedNameMaintenance(OrgBrandOrRelatedName bizO)
			: base(bizO)
		{
		}

		protected override bool CreateOrUpdateName()
		{
			var name = bizO.P1_RelatedName;
			var nameToHash = TextStandardizerHelper.StandardizeCompanyName(name, bizO.Header.CountryCode);
			var patternMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingName>(bizO, false, GetHash(nameToHash), bizO.Header.PK, Guid.Empty, bizO.Header.CountryCode);

			return patternMaintenance.CreateOrUpdatePatternRecord(PatternMatchingNameSchema.PMN_ParentId, bizO.PK);
		}

		protected override void QueueMasterForProcessing()
		{
			QueueOrgForDeduplicationProcessing(bizO.Header);
		}
	}
}
