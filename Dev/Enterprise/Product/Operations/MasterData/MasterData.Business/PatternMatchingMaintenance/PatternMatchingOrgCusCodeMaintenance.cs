using System;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class PatternMatchingOrgCusCodeMaintenance : PatternMatchingSource<OrgCusCode>
	{
		public PatternMatchingOrgCusCodeMaintenance(OrgCusCode bizO)
			: base(bizO)
		{
		}

		protected override bool CreateOrUpdateRegCode()
		{
			var regCode = bizO.OK_CustomsRegNo;
			var codeToHash = TextStandardizerHelper.StandardizeRegCode(regCode);
			var patternMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingRegCode>(bizO, false, GetHash(codeToHash), bizO.Header.PK, Guid.Empty, bizO.Header.CountryCode);

			return patternMaintenance.CreateOrUpdatePatternRecord(PatternMatchingRegCodeSchema.PMR_ParentId, bizO.PK);
		}

		protected override void QueueMasterForProcessing()
		{
			QueueOrgForDeduplicationProcessing(bizO.Header);
		}
	}
}
