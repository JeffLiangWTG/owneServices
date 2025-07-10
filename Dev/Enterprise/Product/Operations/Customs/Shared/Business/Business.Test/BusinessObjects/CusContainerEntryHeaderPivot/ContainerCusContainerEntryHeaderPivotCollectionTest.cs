using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(ContainerCusContainerEntryHeaderPivotCollection))]
	sealed class ContainerCusContainerEntryHeaderPivotCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSetRelationshipDefault()
		{
			var container = Declaration.CusContainers[0];
			var coll = new ContainerCusContainerEntryHeaderPivotCollection(container);
			AssertEquals(container.PK, coll.AddNew().CCE_CO_Container);
		}

		public void TestGetOrCreatePivotFor()
		{
			var entry = Declaration.CustomsEntryHeaders[0];
			var container = Declaration.CusContainers[0];
			var coll = new ContainerCusContainerEntryHeaderPivotCollection(container);
			var newElement = coll.GetOrCreatePivotFor(entry);
			AssertEquals(entry.PK, newElement.CCE_CH_EntryHeader);
			var newElement2 = coll.GetOrCreatePivotFor(entry);
			AssertEquals(newElement, newElement2);
		}
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ContainerCusContainerEntryHeaderPivotCollection(Declaration.CusContainers[0]);
		}

		protected override Type GetExpectedCollectionType() => typeof(ContainerCusContainerEntryHeaderPivotCollection);

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
