using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbPersonLanguageCollection))]
	sealed class GlbPersonLanguageCollectionTest : ActiveBusinessObjectCollectionTestCase<GlbPersonLanguageCollection>
	{
		protected override GlbPersonLanguageCollection GetCollectionToTest()
		{
			GlbPerson person = Factory.New<GlbPerson>();
			return new GlbPersonLanguageCollection(person);
		}
	}
}
