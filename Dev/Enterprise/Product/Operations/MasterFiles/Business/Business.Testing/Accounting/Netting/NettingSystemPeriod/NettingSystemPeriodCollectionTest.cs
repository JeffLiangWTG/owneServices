using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(NettingSystemPeriodCollection))]
	sealed class NettingSystemPeriodCollectionTest : ActiveBusinessObjectCollectionTestCase<NettingSystemPeriodCollection>
	{
		protected override NettingSystemPeriodCollection GetCollectionToTest()
		{
			return new NettingSystemPeriodCollection(Factory);
		}
	}
}
