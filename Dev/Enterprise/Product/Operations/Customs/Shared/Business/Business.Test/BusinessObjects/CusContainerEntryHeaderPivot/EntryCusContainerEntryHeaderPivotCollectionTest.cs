using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(EntryCusContainerEntryHeaderPivotCollection))]
	sealed class EntryCusContainerEntryHeaderPivotCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSetRelationshipDefault()
		{
			var entry = Declaration.CustomsEntryHeaders[0];
			var coll = new EntryCusContainerEntryHeaderPivotCollection(entry);
			AssertEquals(entry.PK, coll.AddNew().CCE_CH_EntryHeader);
		}

		public void TestGetOrCreatePivotFor()
		{
			var entry = Declaration.CustomsEntryHeaders[0];
			var container = Declaration.CusContainers[0];
			var coll = new EntryCusContainerEntryHeaderPivotCollection(entry);
			var newElement = coll.GetOrCreatePivotFor(container);
			AssertEquals(container.PK, newElement.CCE_CO_Container);
			var newElement2 = coll.GetOrCreatePivotFor(container);
			AssertEquals(newElement, newElement2);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new EntryCusContainerEntryHeaderPivotCollection(Declaration.CustomsEntryHeaders[0]);
		}

		protected override Type GetExpectedCollectionType() => typeof(EntryCusContainerEntryHeaderPivotCollection);

		BaseJobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<BaseJobDeclaration>();
					declaration.CustomsEntryHeaders.AddNew();
					declaration.CusContainers.AddNew();
				}
				return declaration;
			}
		}
		BaseJobDeclaration declaration;
	}
}
