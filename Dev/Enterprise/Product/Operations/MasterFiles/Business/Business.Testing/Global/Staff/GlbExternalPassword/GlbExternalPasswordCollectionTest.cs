using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbExternalPasswordCollection))]
	sealed class GlbExternalPasswordCollectionTest : ActiveBusinessObjectCollectionTestCase<GlbExternalPasswordCollection>
	{
		protected override GlbExternalPasswordCollection GetCollectionToTest() => new GlbExternalPasswordCollection(Factory);
	}
}
