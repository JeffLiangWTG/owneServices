using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class JobDocAddressNumberForTest : JobDocAddressNumber
	{
		public JobDocAddressNumberForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public IEnumerable<string> GetPropertiesToExcludeFromCloning_Exposed() => base.GetPropertiesToExcludeFromCloning();
	}
}
