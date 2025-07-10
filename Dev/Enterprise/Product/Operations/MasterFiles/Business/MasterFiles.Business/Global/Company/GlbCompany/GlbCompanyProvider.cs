using System.Collections.Generic;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class GlbCompanyProvider : IGlbCompanyProvider
	{
		public IEnumerable<IGlbCompany> GetActiveCompanies()
		{
			return GlbCompany.GetActiveCompanies();
		}
	}
}
