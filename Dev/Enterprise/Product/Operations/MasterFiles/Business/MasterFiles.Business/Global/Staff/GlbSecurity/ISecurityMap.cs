using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public interface ISecurityMap
	{
		IEnumerable<string[]> GetStaffSecurity(GlbStaff staff);
		bool HasAccessForAllBranchesAndDepartments(BusinessObjectFactory factory, GlbSecurityCollection securities, SecurityCore securityCore, GlbStaff staff, GlbSecurity security);
		bool HasAccessForAllBranchesAndDepartments(BusinessObjectFactory factory, GlbSecurityCollection securities, SecurityCore securityCore, GlbStaff staff, GlbSecurity security, CheckpointLookupKey lookupKey);
	}
}
