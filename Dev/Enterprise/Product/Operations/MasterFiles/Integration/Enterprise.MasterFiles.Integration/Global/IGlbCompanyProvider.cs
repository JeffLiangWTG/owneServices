using System.Collections.Generic;

namespace Enterprise.MasterFiles.Integration
{
	public interface IGlbCompanyProvider
	{
		IEnumerable<IGlbCompany> GetActiveCompanies();
	}
}
