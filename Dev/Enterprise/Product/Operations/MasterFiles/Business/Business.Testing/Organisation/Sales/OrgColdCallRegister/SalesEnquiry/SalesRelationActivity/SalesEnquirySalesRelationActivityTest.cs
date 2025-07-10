using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(SalesEnquiry))]
	sealed class SalesEnquirySalesRelationActivityTest : SalesRelationActivityTestCase<SalesEnquiry>
	{
		protected override ITableSchema TableSchema
		{
			get { return OrgColdCallRegisterSchema.Instance; }
		}

		protected override SalesEnquiry GetNewActivity()
		{
			return Factory.NewWithValidTestData<SalesEnquiry>();
		}
	}
}
