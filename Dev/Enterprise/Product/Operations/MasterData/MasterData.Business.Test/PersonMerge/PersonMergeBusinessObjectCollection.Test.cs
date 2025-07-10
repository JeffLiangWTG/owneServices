using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterData.Business.Tests
{
	[TestedType(typeof(PersonMergeBusinessObjectCollection))]
	public class PersonMergeBusinessObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<PersonMergeBusinessObjectCollection>
	{
		protected override PersonMergeBusinessObjectCollection GetCollectionToTest()
		{
			return new PersonMergeBusinessObjectCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PersonMergeBusinessObject();
		}
	}
}
