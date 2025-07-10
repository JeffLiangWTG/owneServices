using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EntityStaffRestrictionCollection))]
	public class EntityStaffRestrictionCollectionTest : ActiveBusinessObjectCollectionTestCase<EntityStaffRestrictionCollection>
	{
		protected override EntityStaffRestrictionCollection GetCollectionToTest()
		{
			return new EntityStaffRestrictionCollection(Parent);
		}

		BusinessObject Parent
		{
			get
			{
				return Factory.New<DummyBusinessObject>();
			}
		}
	}
}
