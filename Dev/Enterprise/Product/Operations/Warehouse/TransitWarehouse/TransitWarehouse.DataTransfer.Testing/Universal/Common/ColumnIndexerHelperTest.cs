using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.Warehouse.Transit.Business.Testing;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public abstract class ColumnIndexerHelperTest<BOType> : TestCaseWithFactory where BOType : BusinessObject
	{
		protected void AssertGetValue<InputType, ValueType, ExpectedValueType>(Func<InputType, BOType> funcCreateTestData, Func<BOType, ValueType> funcGetExpected, Func<UniversalObjectFactory, IColumnIndexer, ValueType> funcGetActualIndexer, (InputType input, ExpectedValueType expected) testCase)
		{
			var testBO = funcCreateTestData(testCase.input);
			Factory.Save();

			var testIndexer = GetIndexer(testBO);

			AssertEquals(testCase.expected, funcGetActualIndexer(UniversalFactory, testIndexer));
		}

		protected void AssertGetRelatedIndexer<RelatedType>(Func<BOType> funcCreateTestData, Func<BOType, RelatedType> funcGetRelatedBO, Func<UniversalObjectFactory, IColumnIndexer, IColumnIndexer> funcGetActualIndexer) where RelatedType : BusinessObject
		{
			var testBO = funcCreateTestData();
			Factory.Save();

			var testIndexer = GetIndexer(testBO);

			var relatedBO = funcGetRelatedBO(testBO);
			var expectedIndexer = GetIndexer(relatedBO);
			AssertNotNull(relatedBO);
			AssertEquals(expectedIndexer, funcGetActualIndexer(UniversalFactory, testIndexer));
		}

		protected void AssertGetRelatedIndexerCollection<RelatedType>(Func<BOType> fillTestData, Func<BOType, RelatedType> funcGetRelatedBOCollection, Func<UniversalObjectFactory, IColumnIndexer, IColumnIndexer[]> funcGetActualIndexerCollection) where RelatedType : BusinessObjectCollection
		{
			var testData = fillTestData();
			Factory.Save();

			var testIndexer = GetIndexer(testData);
			var relatedBOCollection = funcGetRelatedBOCollection(testData);

			var expected = relatedBOCollection.Select(relatedBO => GetIndexer(relatedBO));
			AssertContainsExactElementsInAnyOrder(expected, funcGetActualIndexerCollection(UniversalFactory, testIndexer));
		}

		protected WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;

		protected IColumnIndexer GetIndexer(BusinessObject bo)
		{
			return UniversalFactory.RowFactory.LoadFromPK(bo.TableName, bo.PK) as IColumnIndexer;
		}

		protected override void SetUp()
		{
			base.SetUp();
			UniversalFactory = new UniversalObjectFactory();
		}

		protected UniversalObjectFactory UniversalFactory { get; set; }
	}
}
