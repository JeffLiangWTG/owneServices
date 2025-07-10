using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IOrgLicencesHelper
	{
		IEnumerable<IOrgLicenceInfo> GetOrgLicences(ZGuid orgPk);
	}

	public interface IOrgLicenceInfo
	{
		int CompanyNumber { get; set; }
		int DatabaseNumber { get; set; }
		string Product {  get; set; }
		string LicenseType { get; set; }
	}
}
