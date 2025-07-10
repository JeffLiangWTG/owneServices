using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CustomValues.Testing
{
	[TestedType(typeof(GenCustomColumnDefinitionCollection))]
	sealed class GenCustomColumnDefinitionCollectionTest : ActiveBusinessObjectCollectionTestCase<GenCustomColumnDefinitionCollection>
	{
		public void TestDefaultValues()
		{
			var parent = Factory.New<ProcessTaskTemplate>();
			var collection = new GenCustomColumnDefinitionCollection(parent);
			var element = collection.AddNew();
			AssertEquals(parent.PK, element.XC_ParentID);
			AssertEquals(parent.TablePrefix, element.XC_ParentTableCode);
		}

		public void TestRelationshipDefaults()
		{
			var genCustomColumn = Factory.New<GenCustomColumnDefinition>();
			var parent = Factory.New<ProcessTaskTemplate>();
			var collection = new GenCustomColumnDefinitionCollection(parent);
			collection.Add(genCustomColumn);

			AssertEquals(parent.PK, genCustomColumn.XC_ParentID);
			AssertEquals(parent.TablePrefix, genCustomColumn.XC_ParentTableCode);
		}

		protected override GenCustomColumnDefinitionCollection GetCollectionToTest()
		{
			var parent = Factory.New<ProcessTaskTemplate>();
			return new GenCustomColumnDefinitionCollection(parent);
		}
	}
}
