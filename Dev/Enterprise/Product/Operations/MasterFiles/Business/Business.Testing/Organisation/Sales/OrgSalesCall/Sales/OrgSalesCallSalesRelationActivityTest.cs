using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSalesCall))]
	sealed class OrgSalesCallSalesRelationActivityTest : SalesRelationActivityTestCase<OrgSalesCall>
	{
		protected override ITableSchema TableSchema
		{
			get { return OrgSalesCallSchema.Instance; }
		}

		protected override OrgSalesCall GetNewActivity()
		{
			return Factory.NewWithValidTestData<OrgSalesCall>();
		}
	}
}
