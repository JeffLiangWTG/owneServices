using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefCountryStatesCollection))]
	sealed class RefCountryStatesCollectionTest : ActiveBusinessObjectCollectionTestCase<RefCountryStatesCollection>
	{
	}
}
