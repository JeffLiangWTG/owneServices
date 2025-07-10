using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgProfitShareDetailsCollection))]
	sealed class OrgProfitShareDetailsCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgProfitShareDetailsCollection(Factory);
		}
	}
}
