using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IJobReferenceNumberGenerator
	{
		ZString GenerateLocalJobReferenceNumber(ZGuid companyPK, ZGuid branchPK, ZGuid department, BusinessObjectFactory factory);
		void AddJobDeletedRecordToParentJobLog(ZGuid parentJobPK, ZString parentTableCode, ZString jobLocalReference, BusinessObjectFactory factory);
		void AddJobDeactivatedRecordToParentJobLog(ZGuid parentJobPK, ZString parentTableCode, ZString jobLocalReference, BusinessObjectFactory factory);
		void AddJobActivatedRecordToParentJobLog(ZGuid parentJobPK, ZString parentTableCode, ZString jobLocalReference, BusinessObjectFactory factory, ZDateTime eventTime);
	}
}
