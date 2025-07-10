using System.Linq;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCommissionAgreementItem))]
	sealed class OrgCommissionAgreementItemTest : EnterpriseBusinessObjectTestCase
	{
		#region Properties

		public void TestCAI_TypeDescription()
		{
			var agreementItem = Factory.New<OrgCommissionAgreementItem>();
			agreementItem.CAI_Type = OrgCommissionAgreementItemTypes.Codes.Product;
			AssertEquals(OrgCommissionAgreementItemTypes.Descriptions.Product, agreementItem.CAI_TypeDescription);

			agreementItem.CAI_Type = OrgCommissionAgreementItemTypes.Codes.Service;
			AssertEquals(OrgCommissionAgreementItemTypes.Descriptions.Service, agreementItem.CAI_TypeDescription);

			agreementItem.CAI_Type = OrgCommissionAgreementItemTypes.Codes.SubModule;
			AssertEquals(OrgCommissionAgreementItemTypes.Descriptions.SubModule, agreementItem.CAI_TypeDescription);
		}

		public void TestCommissionAgreement()
		{
			var agreement = Factory.New<OrgCommissionAgreement>();
			var productItem = agreement.ProductItems.AddNew();
			AssertEquals(agreement, productItem.CommissionAgreement);

			var serviceItem = productItem.ChildServiceItems.AddNew();
			AssertEquals(agreement, serviceItem.CommissionAgreement);

			var subModuleItem = serviceItem.ChildSubModuleItems.AddNew();
			AssertEquals(agreement, subModuleItem.CommissionAgreement);
		}

		public void TestCAI_IsInclude_ResetsChildItemsOnChange()
		{
			var agreement = Factory.New<OrgCommissionAgreement>();
			var item_AAA = agreement.ProductItems.AddNew(true, "AAA");
			var item_AAA_AAA = item_AAA.ChildServiceItems.AddNew(true, "AAA");
			var item_AAA_AAA_AAA = item_AAA_AAA.ChildSubModuleItems.AddNew(true, "AAA");

			item_AAA_AAA.CAI_IsInclude = false;
			AssertEquals("Should delete all sub-module items", 0, item_AAA_AAA.ChildSubModuleItems.Count);

			item_AAA_AAA.CAI_IsInclude = true;
			AssertEquals("Should reset to a single 'ALL' sub-module item", 1, item_AAA_AAA.ChildSubModuleItems.Count);
			AssertEquals(true, item_AAA_AAA.ChildSubModuleItems[0].CAI_IsInclude);
			AssertEquals(OrgCommissionAgreementItemLookups.AllSubModulesCode, item_AAA_AAA.ChildSubModuleItems[0].CAI_Code);

			item_AAA.CAI_IsInclude = false;
			AssertEquals("Should delete all service items", 0, item_AAA.ChildServiceItems.Count);

			item_AAA.CAI_IsInclude = true;
			AssertEquals("Should reset to a single 'ALL' service item", 1, item_AAA.ChildServiceItems.Count);
			AssertEquals(true, item_AAA.ChildServiceItems[0].CAI_IsInclude);
			AssertEquals(OrgCommissionAgreementItemLookups.AllServicesCode, item_AAA.ChildServiceItems[0].CAI_Code);
		}

		public void TestCAI_Code_SetToIncludeWhenChangedToAll()
		{
			var item = Factory.New<OrgCommissionAgreementItem>();
			item.CAI_IsInclude = false;
			item.CAI_Code = "AAA";

			item.CAI_Code = "ALL";
			AssertEquals(true, item.CAI_IsInclude);
		}

		public void TestCAI_Code_ResetsChildItemsOnChange()
		{
			var agreement = Factory.New<OrgCommissionAgreement>();
			var item_AAA = agreement.ProductItems.AddNew(true, "AAA");
			var item_AAA_AAA = item_AAA.ChildServiceItems.AddNew(true, "AAA");
			var item_AAA_AAA_AAA = item_AAA_AAA.ChildSubModuleItems.AddNew(true, "AAA");

			item_AAA_AAA.CAI_Code = "BBB";
			AssertEquals("Should reset to a single 'ALL' subModule item", 1, item_AAA_AAA.ChildSubModuleItems.Count);
			AssertEquals(true, item_AAA_AAA.ChildSubModuleItems[0].CAI_IsInclude);
			AssertEquals(OrgCommissionAgreementItemLookups.AllSubModulesCode, item_AAA_AAA.ChildSubModuleItems[0].CAI_Code);

			item_AAA.CAI_Code = "BBB";
			AssertEquals("Should reset to a single 'ALL' service item", 1, item_AAA.ChildServiceItems.Count);
			AssertEquals(true, item_AAA.ChildServiceItems[0].CAI_IsInclude);
			AssertEquals(OrgCommissionAgreementItemLookups.AllServicesCode, item_AAA.ChildServiceItems[0].CAI_Code);
		}

		public void TestIsAllItem()
		{
			var agreement = Factory.New<OrgCommissionAgreement>();

			var productItem = agreement.ProductItems.AddNew(true, OrgCommissionAgreementItemLookups.AllProductsCode);
			AssertEquals(true, productItem.IsAllItem);

			var serviceItem = productItem.ChildServiceItems.AddNew(true, OrgCommissionAgreementItemLookups.AllServicesCode);
			AssertEquals(true, serviceItem.IsAllItem);

			var subModuleItem = serviceItem.ChildSubModuleItems.AddNew(true, OrgCommissionAgreementItemLookups.AllSubModulesCode);
			AssertEquals(true, subModuleItem.IsAllItem);
		}

		#endregion

		#region Draft

		public void TestCreateAndMergeDraft()
		{
			var productItem = Factory.New<OrgCommissionAgreementItem>();
			productItem.CAI_Type = OrgCommissionAgreementItemTypes.Codes.Product;
			productItem.CAI_Code = "AAA";

			var condition = productItem.ConditionCollection.AddNew();
			condition.CIC_Mode = "CON";

			productItem.ChildServiceItems.DeleteAll();
			var serviceItem = productItem.ChildServiceItems.AddNew();
			serviceItem.CAI_Code = "BBB";

			serviceItem.ChildSubModuleItems.DeleteAll();
			var subModuleItem = serviceItem.ChildSubModuleItems.AddNew();
			subModuleItem.CAI_Code = "CCC";

			var productItemDraft = productItem.CreateDraft();
			var newServiceItemDraft = productItemDraft.ChildServiceItems.AddNew();
			newServiceItemDraft.CAI_Code = "YYY";
			AssertEquals(1, productItemDraft.ConditionCollection.Count);

			var initialConditionCopy = productItemDraft.ConditionCollection[0];
			var newCondition = productItemDraft.ConditionCollection.AddNew();
			newCondition.CIC_Mode = "SHP";

			var newSubModuleItemDraft = productItemDraft.ChildServiceItems.First(x => x.CAI_Code == "BBB").ChildSubModuleItems.AddNew();
			newSubModuleItemDraft.CAI_Code = "XXX";

			productItemDraft.MergeDraft();

			AssertContainsExactElementsInAnyOrder(new[] { initialConditionCopy, newCondition }, productItem.ConditionCollection);
			AssertContainsExactElementsInAnyOrder(new[] { serviceItem, newServiceItemDraft }, productItem.ChildServiceItems);
			AssertContainsExactElementsInAnyOrder(new[] { subModuleItem, newSubModuleItemDraft }, serviceItem.ChildSubModuleItems);
		}

		#endregion

		#region Item Path

		public void TestItemPath()
		{
			var agreement = Factory.New<OrgCommissionAgreement>();
			var item_AAA = agreement.ProductItems.AddNew(true, "AAA");
			var item_AAA_noBBB = item_AAA.ChildServiceItems.AddNew(false, "BBB");
			var item_AAA_noBBB_CCC = item_AAA_noBBB.ChildSubModuleItems.AddNew(true, "CCC");

			AssertArrayEqualsByElements(
				new[]
				{
					"Y AAA"
				},
				item_AAA.GetItemPath().Select(x => (x.Item1 ? "Y" : "N") + " " + x.Item2).ToArray());

			AssertArrayEqualsByElements(
				new[]
				{
					"Y AAA",
					"N BBB"
				},
				item_AAA_noBBB.GetItemPath().Select(x => (x.Item1 ? "Y" : "N") + " " + x.Item2).ToArray());

			AssertArrayEqualsByElements(
				new[]
				{
					"Y AAA",
					"N BBB",
					"Y CCC"
				},
				item_AAA_noBBB_CCC.GetItemPath().Select(x => (x.Item1 ? "Y" : "N") + " " + x.Item2).ToArray());
		}

		#endregion

		#region ConditionCollection

		public void TestConditionCollection()
		{
			var agreement = Factory.New<OrgCommissionAgreement>();
			agreement.ProductItems.DeleteAll();

			var item_AAA = agreement.ProductItems.AddNew(true, "AAA");
			var item_BBB = agreement.ProductItems.AddNew(true, "BBB");

			AssertNotNull(item_AAA.ConditionCollection);
			AssertNotNull(item_BBB.ConditionCollection);

			AssertNotEquals(item_AAA.ConditionCollection, item_BBB.ConditionCollection);

			var cond1 = item_AAA.ConditionCollection.AddNew();
			var cond2 = item_BBB.ConditionCollection.AddNew();

			AssertNotEquals(cond1.PK, cond2.PK);
			AssertEquals(item_AAA.PK, cond1.CIC_CAI);
			AssertEquals(item_BBB.PK, cond2.CIC_CAI);
		}

		#endregion

		#region Sibling Items

		public void TestSiblingItems()
		{
			var agreement = Factory.New<OrgCommissionAgreement>();
			agreement.ProductItems.DeleteAll();
			var item_AAA = agreement.ProductItems.AddNew(true, "AAA");
			var item_BBB = agreement.ProductItems.AddNew(true, "BBB");

			item_AAA.ChildServiceItems.DeleteAll();
			var item_AAA_AAA = item_AAA.ChildServiceItems.AddNew(true, "AAA");
			var item_AAA_BBB = item_AAA.ChildServiceItems.AddNew(true, "BBB");

			item_AAA_AAA.ChildSubModuleItems.DeleteAll();
			var item_AAA_AAA_AAA = item_AAA_AAA.ChildSubModuleItems.AddNew(true, "AAA");

			item_AAA_BBB.ChildSubModuleItems.DeleteAll();
			var item_AAA_BBB_AAA = item_AAA_BBB.ChildSubModuleItems.AddNew(true, "AAA");

			AssertContainsExactElementsInAnyOrder(
				new[] { item_AAA, item_BBB, },
				item_AAA.SiblingItems);

			AssertContainsExactElementsInAnyOrder(
				new[] { item_AAA_AAA, item_AAA_BBB, },
				item_AAA_AAA.SiblingItems);

			AssertContainsExactElementsInAnyOrder(
				new[] { item_AAA_AAA_AAA },
				item_AAA_AAA_AAA.SiblingItems);

			AssertContainsExactElementsInAnyOrder(
				new[] { item_AAA_AAA, item_AAA_BBB },
				item_AAA_BBB.SiblingItems);

			AssertContainsExactElementsInAnyOrder(
				new[] { item_AAA_BBB_AAA },
				item_AAA_BBB_AAA.SiblingItems);

			AssertContainsExactElementsInAnyOrder(
				new[] { item_AAA, item_BBB },
				item_BBB.SiblingItems);
		}

		public void TestSiblingAllItem()
		{
			var agreement = Factory.New<OrgCommissionAgreement>();

			agreement.ProductItems.DeleteAll();
			var item_ALL = agreement.ProductItems.AddNew(true, "ALL");
			var item_BBB = agreement.ProductItems.AddNew(true, "BBB");

			item_ALL.ChildServiceItems.DeleteAll();
			var item_ALL_ALL = item_ALL.ChildServiceItems.AddNew(true, "ALL");
			var item_ALL_BBB = item_ALL.ChildServiceItems.AddNew(true, "BBB");

			item_ALL_ALL.ChildSubModuleItems.DeleteAll();
			var item_ALL_ALL_BBB = item_ALL_ALL.ChildSubModuleItems.AddNew(true, "BBB");

			item_ALL_BBB.ChildSubModuleItems.DeleteAll();
			var item_ALL_BBB_ALL = item_ALL_BBB.ChildSubModuleItems.AddNew(true, "ALL");

			AssertEquals(item_ALL, item_ALL.SiblingAllItem);
			AssertEquals(item_ALL_ALL, item_ALL_ALL.SiblingAllItem);
			AssertEquals(null, item_ALL_ALL_BBB.SiblingAllItem);
			AssertEquals(item_ALL_ALL, item_ALL_BBB.SiblingAllItem);
			AssertEquals(item_ALL_BBB_ALL, item_ALL_BBB_ALL.SiblingAllItem);
			AssertEquals(item_ALL, item_BBB.SiblingAllItem);
		}

		#endregion

		#region HumanReadableName

		public void TestHumanReadableName()
		{
			var item = Factory.New<OrgCommissionAgreementItem>();
			item.CAI_Type = OrgCommissionAgreementItemTypes.Codes.Product;
			item.CAI_Code = "AAA";
			AssertEquals("Product Agreement Item 'AAA'", item.HumanReadableName);
		}

		#endregion

		#region Logs

		public void TestBusinessObjectsWithRelatedEvents()
		{
			var agreement = Factory.New<OrgCommissionAgreement>();
			agreement.ProductItems.DeleteAll();
			var productItem = agreement.ProductItems.AddNew();

			productItem.ChildServiceItems.DeleteAll();
			AssertEquals(0, productItem.BusinessObjectsWithRelatedEvents.Length);

			var serviceItem = productItem.ChildServiceItems.AddNew();
			serviceItem.ChildSubModuleItems.DeleteAll();
			AssertEquals(1, productItem.BusinessObjectsWithRelatedEvents.Length);
			AssertEquals(0, serviceItem.BusinessObjectsWithRelatedEvents.Length);

			var subModuleItem = serviceItem.ChildSubModuleItems.AddNew();
			AssertEquals(2, productItem.BusinessObjectsWithRelatedEvents.Length);
			AssertEquals(1, serviceItem.BusinessObjectsWithRelatedEvents.Length);
			AssertEquals(0, subModuleItem.BusinessObjectsWithRelatedEvents.Length);
		}

		#endregion

		#region Implementation

		public static string GetItemPathText(OrgCommissionAgreementItem item)
		{
			if (item == null)
			{
				return "NULL";
			}

			var itemPath = item.GetItemPath();
			var paddedItemPath = itemPath.Select(path => (path.Item1 ? "[Include]" : "[Exclude]") + " " + path.Item2);
			return string.Join(" > ", paddedItemPath);
		}

		#endregion
	}
}
