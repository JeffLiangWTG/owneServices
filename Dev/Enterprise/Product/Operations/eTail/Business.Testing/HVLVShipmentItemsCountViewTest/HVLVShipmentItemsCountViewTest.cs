using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVShipmentItemsCountView))]
	public class HVLVShipmentItemsCountViewTest : EnterpriseBusinessObjectTestCase
	{
		protected override bool IsDeleteSupported() => false;

		protected override bool CanPersistedObjectBeDeleted => false;
	}
}
