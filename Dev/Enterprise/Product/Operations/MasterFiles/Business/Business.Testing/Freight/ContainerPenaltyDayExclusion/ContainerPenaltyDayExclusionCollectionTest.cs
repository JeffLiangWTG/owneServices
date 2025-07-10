using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ContainerPenaltyDayExclusionCollection))]
	public class ContainerPenaltyDayExclusionCollectionTest : ActiveBusinessObjectCollectionTestCase<ContainerPenaltyDayExclusionCollection>
	{
		protected override ContainerPenaltyDayExclusionCollection GetCollectionToTest()
		{
			return new ContainerPenaltyDayExclusionCollection(Factory);
		}
	}
}
