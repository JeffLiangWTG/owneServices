using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(RelatedDeclarationCollection))]
	sealed class RelatedDeclarationCollectionTest : ActiveBusinessObjectCollectionTestCase<RelatedDeclarationCollection>
	{
		protected override RelatedDeclarationCollection GetCollectionToTest()
		{
			var parent = Factory.New<BaseJobDeclaration>();
			return RelatedDeclarationCollection.GetPivotRelatedCollection(parent);
		}

		public void TestInvalidRelationshipType()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var newDeclaration = Factory.New<BaseJobDeclaration>();
			var collection = RelatedDeclarationCollection.GetPivotRelatedCollection(declaration);
			collection.Add(newDeclaration, "ABC");
			AssertEquals("Invalid relationship type: ABC", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
			AssertNoExceptionThrown(() => collection.Add(newDeclaration, ""));
		}
	}
}
