using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefComplianceListForTest : RefComplianceList
	{
		public RefComplianceListForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		public bool GetReadOnlySecurityForTest(PropertyDescriptor property) => base.GetReadOnlySecurity(property);
	}
}
