using System;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IGlbCompany
	{
		ZGuid PK { get; }
		ZString GC_Code { get; }
		ZString GC_Name { get; }
		ZString GC_RN_NKCountryCode { get; set; }
		ZGuid GC_OH_OrgProxy { get; set; }
		ZBool GC_IsActive { get; set; }

#if DEBUG
		void SetCountry(string countryCode);
		IDisposable TemporarilySetCountry(string countryCode);
#endif
		IRefCountry Country { get; }
		IRefCurrency Currency { get; }
		ZString LicenceKeyIdentifier { get; }
		ZString LicenceEnterpriseCode { get; }
		ZString LicenceServerID { get; }

		string FirstActiveBranchCode { get; }

		bool HasOnlyOneActiveBranch();
		bool IsBranchActive(string branchCode);

		IEnumerable<IGlbBranch> GetActiveBranches();
	}
}
