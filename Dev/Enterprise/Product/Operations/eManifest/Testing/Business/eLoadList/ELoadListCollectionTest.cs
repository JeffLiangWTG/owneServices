
namespace Enterprise.eManifest.Business.Testing
{
	using CargoWise.EntityFramework;
	using CargoWise.EntityFramework.Testing;
	using Enterprise.eManifest.Business;
	using NUnit.Framework;

	[TestedType(typeof(ELoadListCollection))]
	internal class ELoadListCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ELoadListCollection(Factory);
		}
	}
}
