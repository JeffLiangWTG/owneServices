using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DummyOrgAirlineBranchAccount : OrgAirlineBranchAccount
	{
		public DummyOrgAirlineBranchAccount(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public bool GetReadOnlySecurityForTesting => GetReadOnlySecurity(null);
	}
}
