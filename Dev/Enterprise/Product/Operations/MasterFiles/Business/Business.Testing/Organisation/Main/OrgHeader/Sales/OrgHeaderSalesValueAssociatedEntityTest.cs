using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgHeader))]
	public class OrgHeaderSalesValueAssociatedEntityTest : SalesValueAssociatedEntityTestCase
	{
		protected override ISalesValueAssociatedEntity GetNewEntity()
		{
			return Factory.New<OrgHeader>();
		}
	}
}
