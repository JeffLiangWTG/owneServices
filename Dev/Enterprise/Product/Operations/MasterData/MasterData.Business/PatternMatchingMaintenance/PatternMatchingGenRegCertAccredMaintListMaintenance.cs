using System;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class PatternMatchingGenRegCertAccredMaintListMaintenance : PatternMatchingSource<GenRegCertAccredMaintList>
	{
		public PatternMatchingGenRegCertAccredMaintListMaintenance(GenRegCertAccredMaintList bizO) : base(bizO)
		{
		}

		ZGuid? personPk;
		readonly BusinessObjectFactory factory = new BusinessObjectFactory();

		protected override bool CreateOrUpdateRegCode()
		{
			if (!bizO.XZ_RefNumber.IsEmpty)
			{
				if (bizO.XZ_ParentTableCode == GlbPersonSchema.Constants.Prefix)
				{
					personPk = bizO.XZ_ParentID;
				}
				else if (bizO.XZ_ParentTableCode == OrgContactSchema.Constants.Prefix)
				{
					personPk = factory.Load<OrgContact>(bizO.XZ_ParentID)?.OC_PER;
				}
				else if (bizO.XZ_ParentTableCode == GlbStaffSchema.Constants.Prefix)
				{
					personPk = factory.Load<GlbStaff>(bizO.XZ_ParentID)?.GS_PER;
				}
				else if (bizO.XZ_ParentTableCode == HRJobApplicantSchema.Constants.Prefix)
				{
					personPk = factory.Load<HRJobApplicant>(bizO.XZ_ParentID)?.HA_PER;
				}

				if (personPk != null)
				{
					var code = bizO.XZ_RefNumber;
					var codeToHash = bizO.XZ_Type + TextStandardizerHelper.StandardizeRegCode(code);
					var codeMaintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingRegCode>(bizO, false, GetHash(codeToHash), Guid.Empty, personPk.Value, bizO.XZ_RN_NKCountryOfIssuance);

					return codeMaintenance.CreateOrUpdatePatternRecord(PatternMatchingRegCodeSchema.PMR_ParentId, bizO.PK);
				}
			}

			return false;
		}

		protected override void QueueMasterForProcessing()
		{
			if (personPk != null)
			{
				var person = factory.Load<GlbPerson>(personPk.Value);

				if (person != null)
				{
					QueuePersonForDeduplicationProcessing(person);
				}
			}
		}
	}
}
