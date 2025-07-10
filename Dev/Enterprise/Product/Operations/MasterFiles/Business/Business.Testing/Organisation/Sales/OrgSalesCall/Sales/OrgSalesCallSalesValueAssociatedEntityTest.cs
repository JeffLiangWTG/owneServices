using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSalesCall))]
	public class OrgSalesCallSalesValueAssociatedEntityTest : SalesValueAssociatedEntityTestCase
	{
		protected override ISalesValueAssociatedEntity GetNewEntity()
		{
			return Factory.New<OrgSalesCall>();
		}
	}
}
