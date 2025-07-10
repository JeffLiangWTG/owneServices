using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccAlternateGLAccountAttributeCollection))]
	sealed class AccAlternateGLAccountAttributeCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccAlternateGLAccountAttributeCollection(Factory);
		}
	}
}
