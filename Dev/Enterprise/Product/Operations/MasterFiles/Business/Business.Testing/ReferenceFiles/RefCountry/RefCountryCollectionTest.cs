using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefCountryCollection))]
	sealed class RefCountryCollectionTest : ActiveBusinessObjectCollectionTestCase<RefCountryCollection>
	{
	}
}
