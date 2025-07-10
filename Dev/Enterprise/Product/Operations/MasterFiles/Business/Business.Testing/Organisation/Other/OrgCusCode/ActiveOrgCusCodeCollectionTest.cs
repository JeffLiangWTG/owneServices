using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ActiveOrgCusCodeCollection))]
	sealed class ActiveOrgCusCodeCollectionTest : ActiveBusinessObjectCollectionTestCase<ActiveOrgCusCodeCollection>
	{
		protected override ActiveOrgCusCodeCollection GetCollectionToTest()
		{
			return new ActiveOrgCusCodeCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<OrgCusCode>();
		}
	}
}
