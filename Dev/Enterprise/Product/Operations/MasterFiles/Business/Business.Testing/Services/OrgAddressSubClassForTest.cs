using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgAddressSubClassForTest : OrgAddress
	{
		public OrgAddressSubClassForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
