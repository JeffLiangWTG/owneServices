using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Integration
{
	public interface IGlbBranch
	{
		ZGuid PK { get; }
		ZString GB_Code { get; }
		ZString GB_BranchName { get; }
		string NKUNLOCO { get; }
		ZGuid GB_GC { get; set; }
		ZGuid GB_OH_OrgProxy { get; set; }
		IGlbCompany Company { get; }
		ITimeZone HomeTimeZone { get; }

#if DEBUG
		void SetCountryIncludingCompanyWithoutCreatingAccountingData(string countryCode);
#endif
	}
}
