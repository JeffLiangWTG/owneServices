using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(NettingSystemCollection))]
	sealed class NettingSystemCollectionTest : ActiveBusinessObjectCollectionTestCase<NettingSystemCollection>
	{
		protected override NettingSystemCollection GetCollectionToTest()
		{
			return new NettingSystemCollection(Factory);
		}
	}
}
