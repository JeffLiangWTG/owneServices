using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class BasePackingGroupContainerCollectionTest<T> : BusinessObjectCollectionViewTestCase<T> where T : BasePackingGroupContainerCollection
	{
		public void TestHasElementsToBeDeletedWhenContainerDeleted()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;

			BaseCusContainer container = declaration.CusContainers.AddNew();
			Bill bill = declaration.Bills.AddNew();

			BasePackingGroup packGroup1 = declaration.PackingGroups.AddNew();
			packGroup1.CR_CO_Container = container.PK;

			AssertEquals(false, container.PackingGroups.WillBeDuplicateAfterReferenceToParentContainerIsRemovedFrom(packGroup1));
			AssertEquals("But packGroup1 is not linked to housebill, thus will be deleted", true, container.PackingGroups.HasElementsToBeDeletedWhenContainerDeleted);
			AssertEquals(true, container.PackingGroups.ShouldBeDeletedWhenDeletingParentContainer(packGroup1));

			packGroup1.CR_CU_HouseBill = bill.PK;
			AssertEquals(false, container.PackingGroups.WillBeDuplicateAfterReferenceToParentContainerIsRemovedFrom(packGroup1));
			AssertEquals("packGroup1 is linked to housebill now", false, container.PackingGroups.HasElementsToBeDeletedWhenContainerDeleted);
			AssertEquals(false, container.PackingGroups.ShouldBeDeletedWhenDeletingParentContainer(packGroup1));

			BasePackingGroup packGroup2 = declaration.PackingGroups.AddNew();
			packGroup2.CR_CU_HouseBill = bill.PK;
			AssertEquals("packGroup2 will be duplicate with packGroup1 after reference to container is removed from packGroup1", true, container.PackingGroups.WillBeDuplicateAfterReferenceToParentContainerIsRemovedFrom(packGroup1));
			AssertEquals("packGroup1 should be deleted", true, container.PackingGroups.HasElementsToBeDeletedWhenContainerDeleted);
			AssertEquals(true, container.PackingGroups.ShouldBeDeletedWhenDeletingParentContainer(packGroup1));
		}

		public void TestSetDefaultValues()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseCusContainer container = declaration.CusContainers.AddNew();
			BasePackingGroup packGroup = container.PackingGroups.AddNew();
			AssertEquals(container.PK, packGroup.CR_CO_Container);
		}

		public void TestDeclarationIsSet()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseCusContainer container = declaration.CusContainers.AddNew();
			BasePackingGroup packGroup = container.PackingGroups.AddNew();
			AssertEquals(container.PK, packGroup.CR_CO_Container);
			AssertEquals(declaration, packGroup.Declaration);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			BasePackingGroup result = Factory.New<BasePackingGroup>();
			result.CR_CO_Container = Container.PK;
			return result;
		}

		protected BaseJobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<BaseJobDeclaration>();
				}
				return fDeclaration;
			}
		}
		BaseJobDeclaration fDeclaration;

		protected BaseCusContainer Container
		{
			get
			{
				if (fContainer == null)
				{
					fContainer = Declaration.CusContainers.AddNew();
				}
				return fContainer;
			}
		}
		BaseCusContainer fContainer;
	}
}
