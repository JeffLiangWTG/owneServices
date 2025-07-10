using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Accounting.Helpers;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.AccChargeGLPostingOverrideLookups;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccChargeTypeOverrideCopierTest : TestCaseWithFactory
	{
		#region TestArrayComparer

		public void TestArrayComparer()
		{
			var actual = new[] {
				new IComparable[] { 2 },
				new IComparable[] { 1 },
				new IComparable[] { 3,2 },
				new IComparable[] { 3,1 },
				new IComparable[] { 4,1 },
				new IComparable[] { 4,1,1,1 },
				new IComparable[] { 4,1,1 },
				Array.Empty<IComparable>(),
			};

			var expected = new[] {
				Array.Empty<IComparable>(),
				new IComparable[] { 1 },
				new IComparable[] { 2 },
				new IComparable[] { 3,1 },
				new IComparable[] { 3,2 },
				new IComparable[] { 4,1 },
				new IComparable[] { 4,1,1 },
				new IComparable[] { 4,1,1,1 },
			};

			TestArrayComparerActAssert(expected, actual);

			actual = Array.Empty<IComparable[]>();
			expected = Array.Empty<IComparable[]>();

			TestArrayComparerActAssert(expected, actual);

			actual = new[] {
				Array.Empty<IComparable>(),
			};
			expected = new[] {
				Array.Empty<IComparable>(),
			};

			TestArrayComparerActAssert(expected, actual);
		}

		void TestArrayComparerActAssert(IComparable[][] expected, IComparable[][] actual)
		{
			var comparer = new BusinessObjectCollectionCopier.ArrayComparer();

			Array.Sort(actual, comparer);

			for (int i = 0; i < actual.Length; i++)
			{
				Assert(String.Format("Array is sorted correctly, checking element: {0}", i), expected[i].SequenceEqual(actual[i]));
			}
		}

		#endregion

		public void TestConvertToNestedList()
		{
			var copier = new AccChargeTypeOverrideCopier();
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var collection = new AccChargeTypeOverrideCollection(chargeCode);

			var nestedList = copier.ConvertToNestedList(collection);
			AssertEquals("Empty collection, so nothing in nested listed", 0, nestedList.Length);

			var i1 = AddToCollection(collection, "CT1", "IT1", "JT1", "D1", 100M);
			nestedList = copier.ConvertToNestedList(collection);
			AssertEquals("One item in outer list", 1, nestedList.Length);
			AssertEquals("Five field items in first inner list", 5, nestedList[0].Length);
			AssertInnerListFields(nestedList[0], "CT1", "IT1", "JT1", "D1", 100M);

			var i2 = AddToCollection(collection, "CT2", "IT2", "JT2", "D2", 99M);
			nestedList = copier.ConvertToNestedList(collection);
			AssertEquals("Two items in outer list", 2, nestedList.Length);
			AssertEquals("Five field items in first inner list", 5, nestedList[0].Length);
			AssertInnerListFields(nestedList[0], "CT1", "IT1", "JT1", "D1", 100M);
			AssertEquals("Five field items in second inner list", 5, nestedList[1].Length);
			AssertInnerListFields(nestedList[1], "CT2", "IT2", "JT2", "D2", 99M);

			i1.AN_ChargeType = "CT3"; // Will affect ordering making the first item in collection appear second in the list
			i1.AN_MarginPercentage = 98M;

			nestedList = copier.ConvertToNestedList(collection);
			AssertEquals("Two items in outer list", 2, nestedList.Length);
			AssertEquals("Five field items in first inner list", 5, nestedList[0].Length);
			AssertInnerListFields(nestedList[0], "CT2", "IT2", "JT2", "D2", 99M);
			AssertEquals("Five field items in second inner list", 5, nestedList[1].Length);
			AssertInnerListFields(nestedList[1], "CT3", "IT1", "JT1", "D1", 98M);

			i2.Delete();

			nestedList = copier.ConvertToNestedList(collection);
			AssertEquals("One item in outer list", 1, nestedList.Length);
			AssertEquals("Five field items in first inner list", 5, nestedList[0].Length);
			AssertInnerListFields(nestedList[0], "CT3", "IT1", "JT1", "D1", 98M);
		}

		internal static IComparable[][] ListOListToArrayOArray(List<List<IComparable>> a)
		{
			return a.Select(l => l.ToArray()).ToArray();
		}

		bool TestAreEqualUsingLists(List<List<IComparable>> a, List<List<IComparable>> b)
		{
			return BusinessObjectCollectionCopier.AreEqual(ListOListToArrayOArray(a), ListOListToArrayOArray(b));
		}

		public void TestAreEqual()
		{
			var copier = new AccChargeTypeOverrideCopier();
			var nestedList1 = new List<List<IComparable>>();
			var nestedList2 = new List<List<IComparable>>();
			AssertEquals("Two empty lists are equal", true, TestAreEqualUsingLists(nestedList1, nestedList2));

			nestedList1.Add(new List<IComparable> { "CT1", "IT1", "JT1", "D1", 100M });
			AssertEquals("Empty list not equal to one element list", false, TestAreEqualUsingLists(nestedList1, nestedList2));

			nestedList2.Add(new List<IComparable> { "CT1", "IT1", "JT1", "D1", 100M });
			AssertEquals("Two equal one element lists", true, TestAreEqualUsingLists(nestedList1, nestedList2));

			nestedList2 = new List<List<IComparable>>();
			nestedList2.Add(new List<IComparable> { "CT2", "IT1", "JT1", "D1", 100M });
			AssertEquals("Two different one element lists", false, TestAreEqualUsingLists(nestedList1, nestedList2));

			nestedList2 = new List<List<IComparable>>();
			nestedList2.Add(new List<IComparable> { "CT2", "IT1", "JT1", "D1", 100M, "Another" });
			AssertEquals("Two different lenght one element lists", false, TestAreEqualUsingLists(nestedList1, nestedList2));

			nestedList1 = new List<List<IComparable>>();
			nestedList1.Add(new List<IComparable> { "CT1", "IT1", "JT1", "D1", 100M, "Another" });
			nestedList1.Add(new List<IComparable> { "CT2", "IT1", "JT1", "D1", 100M, "Another" });
			nestedList2 = new List<List<IComparable>>();
			nestedList2.Add(new List<IComparable> { "CT2", "IT1", "JT1", "D1", 100M, "Another" });
			nestedList2.Add(new List<IComparable> { "CT1", "IT1", "JT1", "D1", 100M, "Another" });
			AssertEquals("We do care about order", false, TestAreEqualUsingLists(nestedList1, nestedList2));
			// We do care about order... Therefore ConvertToNestedList needs to order them correctly for this comparison to be compatiable. This is covered in TestConvertToNestedList.
		}

		public void TestWithNulls()
		{
			var revPK = AccChargeCodeTest.AccGLHeaderPK(Factory, AccChargeCodeTest.GLHeader_Revenue);
			var copier = new AccChargeGLPostingOverrideCopier();
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_ChargeType = "REV";
			var collection = new AccChargeGLPostingOverrideCollection(chargeCode);
			var newItem = collection.AddNew();
			newItem.Y1_AG_REV = revPK;
			newItem.Y1_GE = GlbDepartment.CurrentDepartment.PK;

			var nestedList = copier.ConvertToNestedList(collection);
			AssertInnerListFields(nestedList[0], Env.CurrentDepartment.PK,
												null,
												null,
												revPK.ToGuid(),
												null,
												null,
												null,
												ConsolidatedAccountingCategoryClassList.Codes.All,
												JobTypeAdditionalCodes.All,
												TransportModeAdditionalCodes.All,
												Core.Constants.FreightShipmentDirection.Code.All,
												ConsolContainerModeAdditionalCodes.All,
												MasterPaymentTypeAdditionalCodes.All,
												HousePaymentTypeAdditionalCodes.All);

			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_ChargeType = "REV";
			var collection2 = new AccChargeGLPostingOverrideCollection(chargeCode);
			copier.CopyTo(nestedList, collection2);
			AssertEquals("One item in collection", 1, collection2.Count);
			AssertEquals(ZGuid.Empty, collection2[0].Y1_AG_ACR);
			AssertEquals(ZGuid.Empty, collection2[0].Y1_AG_CST);
			AssertEquals(revPK, collection2[0].Y1_AG_REV);
			AssertEquals(ZGuid.Empty, collection2[0].Y1_AG_WIP);
			AssertEquals(new ZGuid(GlbDepartment.CurrentDepartment.PK), collection2[0].Y1_GE);
		}

		public void TestCopyTo()
		{
			var copier = new AccChargeTypeOverrideCopier();
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var collection = new AccChargeTypeOverrideCollection(chargeCode);

			var nestedList = new List<List<IComparable>>();

			copier.CopyTo(ListOListToArrayOArray(nestedList), collection);
			AssertEquals("Nothing in collection", 0, collection.Count);

			AddToCollection(collection, "CT1", "IT1", "JT1", "D1", 100M);
			AssertEquals("One item in collection", 1, collection.Count);

			copier.CopyTo(ListOListToArrayOArray(nestedList), collection);
			AssertEquals("Nothing in collection", 0, collection.Count);

			nestedList.Add(new List<IComparable> { "CT1", "IT1", "JT1", "D1", 100M });
			copier.CopyTo(ListOListToArrayOArray(nestedList), collection);
			AssertEquals("One item in collection", 1, collection.Count);
			AssertOverrideFields(collection[0], "CT1", "IT1", "JT1", "D1", 100M);
			copier.CopyTo(ListOListToArrayOArray(nestedList), collection);
			AssertEquals("One item in collection", 1, collection.Count);
			AssertOverrideFields(collection[0], "CT1", "IT1", "JT1", "D1", 100M);

			nestedList.Add(new List<IComparable> { "CT2", "IT2", "JT2", "D2", 200M });
			copier.CopyTo(ListOListToArrayOArray(nestedList), collection);
			AssertEquals("Two items in collection", 2, collection.Count);
			AssertOverrideFields(collection[0], "CT1", "IT1", "JT1", "D1", 100M);
			AssertOverrideFields(collection[1], "CT2", "IT2", "JT2", "D2", 200M);

			nestedList = new List<List<IComparable>>();
			var thisOneWillBeDeleted = collection[0];
			nestedList.Add(new List<IComparable> { "CT2", "IT2", "JT2", "D2", 55M });
			nestedList.Add(new List<IComparable> { "CT3", "IT3", "JT3", "D3", 55M });
			copier.CopyTo(ListOListToArrayOArray(nestedList), collection);
			AssertEquals("Two items in collection", 2, collection.Count);
			AssertOverrideFields(collection[0], "CT2", "IT2", "JT2", "D2", 55M);
			AssertOverrideFields(collection[1], "CT3", "IT3", "JT3", "D3", 55M);
			Assert("Item should be marked for deletion", thisOneWillBeDeleted.IsDeleted);

			// Test it can handle deleting the same element in the collection twice. 
			AddToCollection(collection, "CT3", "IT3", "JT3", "D3", 55M);
			AssertEquals("Three items in collection", 3, collection.Count);
			nestedList = new List<List<IComparable>>();
			nestedList.Add(new List<IComparable> { "CT2", "IT2", "JT2", "D2", 55M });
			copier.CopyTo(ListOListToArrayOArray(nestedList), collection);
			AssertEquals("One item in collection", 1, collection.Count);
			AssertOverrideFields(collection[0], "CT2", "IT2", "JT2", "D2", 55M);
		}

		#region Helper Methods

		void AssertInnerListFields(IEnumerable<IComparable> actualValues, params object[] expectedValues)
		{
			AssertArrayEqualsByElements(expectedValues, actualValues.ToArray());
		}

		void AssertOverrideFields(AccChargeTypeOverride ovr, string chargeType, string invoiceType, string jobType, string jobDirection, decimal margin)
		{
			AssertEquals("Override field AN_ChargeType matches", chargeType, ovr.AN_ChargeType);
			AssertEquals("Override field AN_InvoiceType matches", invoiceType, ovr.AN_InvoiceType);
			AssertEquals("Override field AN_JobType matches", jobType, ovr.AN_JobType);
			AssertEquals("Override field AN_JobDirection matches", jobDirection, ovr.AN_JobDirection);
			AssertEquals("Override field AN_MarginPercentage matches", margin, ovr.AN_MarginPercentage);
		}

		AccChargeTypeOverride AddToCollection(AccChargeTypeOverrideCollection collection,
			ZString chargeType,
			ZString invoiceType,
			ZString jobType,
			ZString jobDirection,
			ZDecimal margin)
		{
			var ovr = collection.AddNew();
			ovr.AN_ChargeType = chargeType;
			ovr.AN_InvoiceType = invoiceType;
			ovr.AN_JobType = jobType;
			ovr.AN_JobDirection = jobDirection;
			ovr.AN_MarginPercentage = margin;
			return ovr;
		}

		#endregion
	}
}
