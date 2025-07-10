using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(WorkRequestCollection))]
	class WorkRequestCollectionTest : ActiveBusinessObjectCollectionTestCase<WorkRequestCollection>
	{
	}
}
