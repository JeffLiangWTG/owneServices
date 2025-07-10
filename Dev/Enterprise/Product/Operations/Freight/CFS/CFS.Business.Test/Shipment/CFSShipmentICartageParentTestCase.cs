using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Common.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(CFSShipment))]
	public class CFSShipmentICartageParentTestCase : ICartageParentTestCase
	{
		protected override ICartageParent GetNewParent()
		{
			return Factory.New<CFSShipment>();
		}
	}
}
