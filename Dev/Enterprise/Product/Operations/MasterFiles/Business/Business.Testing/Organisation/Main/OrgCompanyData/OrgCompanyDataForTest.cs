using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCompanyDataForTest : OrgCompanyData
	{
		public OrgCompanyDataForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override bool HasSecurityToChangeDebtor
		{
			get { return true; }
		}
	}
}
