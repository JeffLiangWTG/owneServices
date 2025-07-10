using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using static Enterprise.MasterFiles.Business.AccChargeCreditorOverrideCollection;

namespace Enterprise.MasterFiles.Business.Testing.Accounting.Helpers
{
	sealed class AccChargeCreditorOverrideCopierTest : TestCaseWithFactory
	{
		public void TestConvertToNestedList()
		{
			var copier = new AccChargeCreditorOverrideCopier();
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var collection = new AccChargeCreditorOverrideCollection(chargeCode);

			var nestedList = copier.ConvertToNestedList(collection);
			AssertEquals("Empty collection, so nothing in the nested list", 0, nestedList.Length);

			var departmentPK1 = Guid.NewGuid();
			var accChargeCreditorOverride1 = AddToCollection(collection, "JT1", "DI1", "TM1", "PT1", departmentPK1, "DR1", "CR1", Guid.NewGuid());
			nestedList = copier.ConvertToNestedList(collection);
			AssertEquals("One item in outer list", 1, nestedList.Length);
			AssertEquals("First inner list contains the correct number of fields", 8, nestedList[0].Length);
			AssertInnerListMatchesBizObj("The converted list should should match the fields in the business object", accChargeCreditorOverride1, nestedList[0]);

			var accChargeCreditorOverride2 = AddToCollection(collection, "JT2", "DI2", "TM2", "PT2", Guid.NewGuid(), "DR2", "CR2", Guid.NewGuid());
			nestedList = copier.ConvertToNestedList(collection);
			AssertEquals("Two items in outer list", 2, nestedList.Length);
			AssertEquals("First inner list contains the correct number of fields", 8, nestedList[0].Length);
			AssertInnerListMatchesBizObj("The converted list should be sorted based on the key columns", accChargeCreditorOverride1, nestedList[0]);
			AssertEquals("Second inner list contains the correct number of fields", 8, nestedList[1].Length);
			AssertInnerListMatchesBizObj("The converted list should be sorted based on the key columns", accChargeCreditorOverride2, nestedList[1]);

			accChargeCreditorOverride1.ACC_JobType = "JT3"; // Will affect ordering making the first item in collection appear second in the list
			accChargeCreditorOverride1.ACC_Direction = "DI1"; // ACC_JobType's setter clears out some other fields, so we reset them too.
			accChargeCreditorOverride1.ACC_TransportMode = "TM1";
			accChargeCreditorOverride1.ACC_PaymentTerm = "PT1";
			accChargeCreditorOverride1.ACC_GE_Department = departmentPK1;

			nestedList = copier.ConvertToNestedList(collection);
			AssertEquals("Two items in outer list", 2, nestedList.Length);
			AssertEquals("First inner list contains the correct number of fields", 8, nestedList[0].Length);
			AssertInnerListMatchesBizObj($"The first item in the converted nested list should match {nameof(accChargeCreditorOverride2)}", accChargeCreditorOverride2, nestedList[0]);
			AssertEquals("Second inner list contains the correct number of fields", 8, nestedList[1].Length);
			AssertInnerListMatchesBizObj($"The second item in the converted nested list should match {nameof(accChargeCreditorOverride1)}", accChargeCreditorOverride1, nestedList[1]);

			accChargeCreditorOverride2.Delete();

			nestedList = copier.ConvertToNestedList(collection);
			AssertEquals("Only one item in outer list", 1, nestedList.Length);
			AssertEquals("First inner list contains the correct number of fields", 8, nestedList[0].Length);
			AssertInnerListMatchesBizObj("ConvertToNestedList will not convert items that were deleted", accChargeCreditorOverride1, nestedList[0]);

			var itemWithNulls = AddToCollection(collection, "JT4", "DI4", null, null, ZGuid.Empty, null, null, ZGuid.Empty);
			nestedList = copier.ConvertToNestedList(collection);
			AssertEquals("The collection and nested list have the same size", collection.Count, nestedList.Length);
			AssertEquals("Second inner list contains the correct number of fields", 8, nestedList[1].Length);
			AssertInnerListMatchesBizObj("The bizObj with nulls was converted properly", itemWithNulls, nestedList[1]);
		}

		void AssertInnerListMatchesBizObj(string message, AccChargeCreditorOverride bizObj, IComparable[] actualValues)
		{
			var expectedValues = BizObjToArray(bizObj);
			AssertArrayEqualsByElements(message, expectedValues, actualValues);
		}

		public void TestCopyTo()
		{
			var copier = new AccChargeCreditorOverrideCopier();
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var collection = new AccChargeCreditorOverrideCollection(chargeCode);

			var nestedList = new List<List<IComparable>>();
			copier.CopyTo(AccChargeTypeOverrideCopierTest.ListOListToArrayOArray(nestedList), collection);
			AssertEquals("Nothing in collection", 0, collection.Count);

			collection.AddNew();
			AssertEquals("One item in collection", 1, collection.Count);

			copier.CopyTo(AccChargeTypeOverrideCopierTest.ListOListToArrayOArray(nestedList), collection);
			AssertEquals("Nothing in collection", 0, collection.Count);

			var item1 = new List<IComparable> { "JT1", "DI1", "TM1", "PT1", Guid.NewGuid(), "DR1", "CR1", Guid.NewGuid() };
			nestedList.Add(item1);
			copier.CopyTo(AccChargeTypeOverrideCopierTest.ListOListToArrayOArray(nestedList), collection);
			AssertEquals("One item in collection", 1, collection.Count);
			AssertBizObjMatchesInnerList("The items in the nested list should be copied to the collection", item1, collection[0]);
			copier.CopyTo(AccChargeTypeOverrideCopierTest.ListOListToArrayOArray(nestedList), collection);
			AssertEquals("One item in collection", 1, collection.Count);
			AssertBizObjMatchesInnerList("Copying the same source twice does not cause duplicate copies in the collection.", nestedList[0], collection[0]);

			var item2 = new List<IComparable> { "JT2", "DI2", "TM2", "PT2", Guid.NewGuid(), "DR2", "CR2", Guid.NewGuid() };
			nestedList.Add(item2);
			copier.CopyTo(AccChargeTypeOverrideCopierTest.ListOListToArrayOArray(nestedList), collection);
			AssertBizObjCollectionMatchesOuterList("CopyTo should be able to copy multiple items", nestedList, collection);

			var department3 = Guid.NewGuid();
			var creditor3 = Guid.NewGuid();
			nestedList = new List<List<IComparable>>();
			var item3 = new List<IComparable> { "JT3", "DI3", "TM3", "PT3", department3, "DR3", "CR3", creditor3 };
			var thisOneWillBeDeleted = collection[0];
			nestedList.Add(item2);
			nestedList.Add(item3);
			Assert("Precondition: The first item in the collection is not deleted", !thisOneWillBeDeleted.IsDeleted);
			copier.CopyTo(AccChargeTypeOverrideCopierTest.ListOListToArrayOArray(nestedList), collection);
			AssertBizObjCollectionMatchesOuterList("After using CopyTo, the two collections should match. Any items that were in the collection but not the outer list, will be removed from the collection",
				nestedList,
				collection);
			Assert("When we copy an item to the collection that was already in the collection, that item will not be deleted", !collection[0].IsDeleted);
			Assert("When we copy to a collection, any bizObjs in the collection that are not being copied should be marked for deletion", thisOneWillBeDeleted.IsDeleted);

			// Test it can handle deleting the same item in the collection twice.
			AddToCollection(collection, "JT3", "DI3", "TM3", "PT3", department3, "DR3", "CR3", creditor3);
			AssertEquals("Precondition: Three items in collection", 3, collection.Count);
			nestedList = new List<List<IComparable>>();
			nestedList.Add(item2);
			copier.CopyTo(AccChargeTypeOverrideCopierTest.ListOListToArrayOArray(nestedList), collection);
			AssertEquals("One item in collection. Items not copied should be removed. If duplicate items exist in the collection and the item wasn't copied, then both should be removed.", 1, collection.Count);
			AssertBizObjCollectionMatchesOuterList("The item that was copied is still in the collection", nestedList, collection);
		}

		void AssertBizObjMatchesInnerList(string message, IEnumerable<IComparable> expectedInnerList, AccChargeCreditorOverride actualBizObj)
		{
			var actualValues = BizObjToArray(actualBizObj);
			AssertArrayEqualsByElements(message, expectedInnerList.ToArray(), actualValues);
		}

		void AssertBizObjCollectionMatchesOuterList(string message, IEnumerable<IEnumerable<IComparable>> expectedOuterList, AccChargeCreditorOverrideCollection actualCollection)
		{
			AssertEquals("The collection has the same number of items as the outer list", expectedOuterList.Count(), actualCollection.Count);
			for (int i = 0; i < expectedOuterList.Count(); i++)
			{
				AssertBizObjMatchesInnerList(message, expectedOuterList.ElementAt(i), actualCollection[i]);
			}
		}

		#region Implementation

		object[] BizObjToArray(AccChargeCreditorOverride bizObj)
		{
			return new object[]
			{
				bizObj.ACC_JobType,
				bizObj.ACC_Direction,
				bizObj.ACC_TransportMode,
				bizObj.ACC_PaymentTerm,
				bizObj.ACC_GE_Department.IsEmpty ? null : bizObj.ACC_GE_Department,
				bizObj.ACC_DefaultingRule,
				bizObj.ACC_CreditorRole,
				bizObj.ACC_OH_Creditor.IsEmpty ? null : bizObj.ACC_OH_Creditor
			};
		}

		AccChargeCreditorOverride AddToCollection
		(
			AccChargeCreditorOverrideCollection collection,
			ZString jobType,
			ZString direction,
			ZString transportMode,
			ZString paymentTerm,
			ZGuid department,
			ZString defaultingRule,
			ZString creditorRole,
			ZGuid creditor
		)
		{
			var ovr = collection.AddNew();
			ovr.ACC_JobType = jobType;
			ovr.ACC_CreditorRole = creditorRole;
			ovr.ACC_DefaultingRule = defaultingRule;
			ovr.ACC_Direction = direction;
			ovr.ACC_GE_Department = department;
			ovr.ACC_OH_Creditor = creditor;
			ovr.ACC_PaymentTerm = paymentTerm;
			ovr.ACC_TransportMode = transportMode;

			return ovr;
		}

		#endregion
	}
}
