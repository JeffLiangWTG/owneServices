using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCommissionCalculationQueueCollection))]
	sealed class OrgCommissionCalculationQueueCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgCommissionCalculationQueueCollection>
	{
	}
}
