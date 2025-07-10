using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgOpportunity))]
	sealed class OrgOpportunitySalesRelationActivityTest : SalesRelationActivityTestCase<OrgOpportunity>
	{
		protected override ITableSchema TableSchema
		{
			get { return OrgOpportunitySchema.Instance; }
		}

		protected override OrgOpportunity GetNewActivity()
		{
			return Factory.NewWithValidTestData<OrgOpportunity>();
		}
	}
}
