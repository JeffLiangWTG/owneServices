using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Cartonisation.Integration;
using NUnit.Framework;

namespace Enterprise.Warehouse.Cartonisation.Diagnostic.Testing
{
	[TestedType(typeof(DummyCartonisableItemDefinition))]
	public class DummyCartonisableItemDefinitionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestICartonisableItemDefinition_PK()
		{
			var definition = new DummyCartonisableItemDefinition();
			var definitionSame = new DummyCartonisableItemDefinition();
			var definitionLowerCase = new DummyCartonisableItemDefinition();
			var definitionDifferent = new DummyCartonisableItemDefinition();

			definition.ProductName = "PRODUCT";
			definitionSame.ProductName = "PRODUCT";
			definitionLowerCase.ProductName = "product";
			definitionDifferent.ProductName = "OTHER";

			AssertEquals(((ICartonisableItemDefinition)definition).PK, ((ICartonisableItemDefinition)definitionSame).PK);
			AssertEquals(((ICartonisableItemDefinition)definition).PK, ((ICartonisableItemDefinition)definitionLowerCase).PK);
			AssertNotEquals(((ICartonisableItemDefinition)definition).PK, ((ICartonisableItemDefinition)definitionDifferent).PK);
		}
	}
}
