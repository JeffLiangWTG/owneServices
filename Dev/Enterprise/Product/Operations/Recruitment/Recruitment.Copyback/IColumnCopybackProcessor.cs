using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Recruitment.Copyback
{
	public interface IColumnCopybackProcessor
	{
		void StaffWorkingBasis(BusinessObjectFactory factory, GlbStaff staff, IQueuedLog log);
		void EmploymentHistory(BusinessObjectFactory factory, GlbStaff staff, IQueuedLog log);
		void WorkPattern(BusinessObjectFactory factory, GlbStaff staff, IQueuedLog log);
		void StaffManager(BusinessObjectFactory factory, GlbStaff staff, IQueuedLog log);
	}
}
