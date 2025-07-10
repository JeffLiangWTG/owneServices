using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Common.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(CFSLoadListConsol))]
	public class CFSLoadListConsolICartageParentTestCase : ICartageParentTestCase
	{
		protected override ICartageParent GetNewParent()
		{
			return Factory.New<CFSLoadListConsol>();
		}
	}
}
