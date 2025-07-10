using System;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class PatternMatchingOrgHeaderMaintenance : PatternMatchingSource<OrgHeader>
	{
		public PatternMatchingOrgHeaderMaintenance(OrgHeader bizO)
			: base(bizO)
		{
		}

		protected override bool CreateOrUpdateName()
		{
			var name = bizO.OH_FullName;
			var nameToHash = TextStandardizerHelper.StandardizeCompanyName(name, bizO.CountryCode);
			var patternMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingName>(bizO, false, GetHash(nameToHash), bizO.PK, Guid.Empty, bizO.CountryCode);

			return patternMaintenance.CreateOrUpdatePatternRecord(PatternMatchingNameSchema.PMN_ParentId, bizO.PK);
		}

		protected override void QueueMasterForProcessing()
		{
			QueueOrgForDeduplicationProcessing(bizO);
		}
	}
}
