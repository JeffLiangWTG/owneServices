using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ProcessManagement.Integration;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestsSubclassesOf(typeof(IWorkTaskRelatedItem))]
	public abstract class WorkTaskRelatedItemTestCase : TestCaseWithFactory
	{
		public void TestSelectionCriteria()
		{
			var item = GetItemForSelectionCriteriaTest();
			AssertEquals(ExpectedSelectionCriterion1, item.SelectionCriterion1);
			AssertEquals(ExpectedSelectionCriterion2, item.SelectionCriterion2);
			AssertEquals(ExpectedSelectionCriterion3, item.SelectionCriterion3);
			AssertEquals(ExpectedSelectionCriterion4, item.SelectionCriterion4);
			AssertEquals(ExpectedSelectionCriterion5, item.SelectionCriterion5);
		}

		public void TestPivotCollectionType()
		{
			AssertEquals(ExpectedPivotCollectionType, GetItemForSelectionCriteriaTest().PivotCollectionType);
		}

		protected abstract string ExpectedSelectionCriterion1 { get; }

		protected abstract string ExpectedSelectionCriterion2 { get; }

		protected abstract string ExpectedSelectionCriterion3 { get; }

		protected abstract string ExpectedSelectionCriterion4 { get; }

		protected abstract string ExpectedSelectionCriterion5 { get; }

		protected abstract IWorkTaskRelatedItem GetItemForSelectionCriteriaTest();

		protected abstract Type ExpectedPivotCollectionType { get; }
	}
}
