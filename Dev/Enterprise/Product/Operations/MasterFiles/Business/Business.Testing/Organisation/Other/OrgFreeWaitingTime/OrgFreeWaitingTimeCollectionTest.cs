using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgFreeWaitingTimeCollection))]
	sealed class OrgFreeWaitingTimeCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgFreeWaitingTimeCollection>
	{
		protected override OrgFreeWaitingTimeCollection GetCollectionToTest()
		{
			return new OrgFreeWaitingTimeCollection(Factory, Factory.LoadTop1<OrgAddress>(new ZQuery()));
		}
	}
}
