using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HRJobApplicationCollection))]
	sealed class HRJobApplicationCollectionTest : ActiveBusinessObjectCollectionTestCase<HRJobApplicationCollection>
	{
	}
}
