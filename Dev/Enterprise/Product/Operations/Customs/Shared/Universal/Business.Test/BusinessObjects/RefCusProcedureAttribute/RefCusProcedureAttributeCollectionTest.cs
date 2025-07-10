using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusProcedureAttributeCollection))]
	internal class RefCusProcedureAttributeCollectionTest : ActiveBusinessObjectCollectionTestCase<RefCusProcedureAttributeCollection>
	{
		public override void TestAddNew()
		{
			var procedure = Factory.New<RefCusProcedure>();
			var attribute = procedure.Attributes.AddNew("BOB", "B");
			AssertEquals("attribute.ZXB_Name", "BOB", attribute.ZXB_Name);
			AssertEquals("attribute.ZXB_Value", "B", attribute.ZXB_Value);
			AssertEquals("procedure.Attributes.Count", 1, procedure.Attributes.Count);
			var attribute2 = procedure.Attributes.AddNew();
			AssertEquals("procedure.Attributes.Count", 2, procedure.Attributes.Count);
			AssertCollectionContains(attribute2, procedure.Attributes);
		}

		protected override RefCusProcedureAttributeCollection GetCollectionToTest()
		{
			return new RefCusProcedureAttributeCollection(Factory.New<RefCusProcedure>());
		}
	}
}
