using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(FilteredWorkTaskRelatedItemCollection))]
	public class FilteredWorkTaskRelatedItemCollectionTest : BusinessObjectCollectionViewTestCase<FilteredWorkTaskRelatedItemCollection>
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(FilteredWorkTaskRelatedItemCollection);
		}

		protected override FilteredWorkTaskRelatedItemCollection GetCollectionToTest()
		{
			var master = Factory.NewWithValidTestData<Project>();
			return master.FilteredRelatedItems;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData<WorkItem>();
		}
	}
}
