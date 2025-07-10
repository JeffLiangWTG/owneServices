using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HRHiringRequestCollection))]
	sealed class HRHiringRequestCollectionTest : ActiveBusinessObjectCollectionTestCase<HRHiringRequestCollection>
	{
	}
}
