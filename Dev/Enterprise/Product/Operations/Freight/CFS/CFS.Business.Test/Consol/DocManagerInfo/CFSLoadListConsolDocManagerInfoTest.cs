using CargoWise.EntityFramework;
using Enterprise.Freight.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(CFSLoadListConsolDocManagerInfo))]
	public class CFSLoadListConsolDocManagerInfoTest : ConsolDocManagerInfoTest
	{
		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<CFSLoadListConsol>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			CFSLoadListConsol loadListConsol = (CFSLoadListConsol)base.GetPopulatedParentBusinessObject();
			return loadListConsol;
		}
	}
}
