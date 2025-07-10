using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgOpportunity))]
	public class OrgOpportunitySalesValueAssociatedEntityTest : SalesValueAssociatedEntityTestCase
	{
		protected override ISalesValueAssociatedEntity GetNewEntity()
		{
			return Factory.New<OrgOpportunity>();
		}
	}
}
