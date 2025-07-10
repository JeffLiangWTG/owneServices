using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusEntryPayInfoCollection<CusEntryPayInfo>))]
	public class CusEntryPayInfoCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestGetItemByMessageNum()
		{
			var collection = new CusEntryPayInfoCollection<CusEntryPayInfo>(EntryHeader);
			AssertNull("No item by '0001'", collection.GetItemByMessageNum("0001"));

			CusEntryPayInfo payInfo = collection.AddNew();
			payInfo.C9_IncomingPayResponseNo = "0001";
			AssertEquals("Item by '0001'", payInfo, collection.GetItemByMessageNum("0001"));
		}

		public void TestGetPendingItems()
		{
			var collection = new CusEntryPayInfoCollection<CusEntryPayInfo>(EntryHeader);
			AssertEquals("No pending item", 0, collection.GetPendingItems().Length);

			CusEntryPayInfo payInfo = collection.AddNew();
			payInfo.C9_PaymentStatus = CusEntryPayInfoStatusList.Codes.Pending;
			AssertEquals("Pending item", 1, collection.GetPendingItems().Length);
			AssertEquals("Pending item", payInfo, collection.GetPendingItems()[0]);

			payInfo.C9_PaymentStatus = CusEntryPayInfoStatusList.Codes.Clear;
			AssertEquals("No pending item", 0, collection.GetPendingItems().Length);
		}

		public void TestAllowNew()
		{
			AssertEquals("this collection does not allow new", false, new CusEntryPayInfoCollection<CusEntryPayInfo>(EntryHeader).AllowNew);
		}

		public void TestInsertOrUpdateEntryPayInfo()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var entryPayInfoCollection = new CusEntryPayInfoCollection<CusEntryPayInfo>(entryHeader);
			AssertEquals("PRE-CONDITION", 0, entryPayInfoCollection.Count);

			var entryPayInfo1 = entryPayInfoCollection.InsertOrUpdateEntryPayInfo(x => x != null, 1m, "4 T", new ZDateTime(2021, 01, 01), "000001", "G");
			CombineAssertions("entryPayInfo1 added", () =>
			{
				entryPayInfo1.AssertEntryPayInfo("000001", "G", 1m, "4 T", new ZDateTime(2021, 01, 01), CusEntryPayInfoStatusList.Codes.Pending);
			});

			CombineAssertions("entryPayInfo1 updated", () =>
			{
				var entryPayInfo2 = entryPayInfoCollection.InsertOrUpdateEntryPayInfo(x => x != null, 999m, "4 T", new ZDateTime(2021, 12, 31), "000001", "G");
				entryPayInfo2.AssertEntryPayInfo("000001", "G", 999m, "4 T", new ZDateTime(2021, 12, 31), CusEntryPayInfoStatusList.Codes.Pending);
				AssertSame("Same object", entryPayInfo1, entryPayInfo2);
			});

			CombineAssertions("Edge Case 1: add element with empty method of payment", () =>
			{
				var entryPayInfo3 = entryPayInfoCollection.InsertOrUpdateEntryPayInfo(x => false, 999m, "2", new ZDateTime(2021, 12, 31), "999999", "");
				entryPayInfo3.AssertEntryPayInfo("999999", "", 999m, "2", new ZDateTime(2021, 12, 31), CusEntryPayInfoStatusList.Codes.Pending);
				AssertEquals("Collection count", 2, entryPayInfoCollection.Count);
			});

			CombineAssertions("Edge Case 2: add element with empty A93 number", () =>
			{
				var entryPayInfo4 = entryPayInfoCollection.InsertOrUpdateEntryPayInfo(x => false, 999m, "2", new ZDateTime(2021, 12, 31), "", "G");
				entryPayInfo4.AssertEntryPayInfo("", "G", 999m, "2", new ZDateTime(2021, 12, 31), CusEntryPayInfoStatusList.Codes.Pending);
				AssertEquals("Collection count", 3, entryPayInfoCollection.Count);
			});
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusEntryPayInfoCollection<CusEntryPayInfo>(EntryHeader);
		}

		BaseJobDeclaration TestDec
		{
			get
			{
				if (fTestDec == null)
				{
					fTestDec = Factory.New<BaseJobDeclaration>();
				}

				return fTestDec;
			}
		}
		BaseJobDeclaration fTestDec;

		CusEntryHeader EntryHeader
		{
			get
			{
				if (fEntryHeader == null)
				{
					fEntryHeader = TestDec.CustomsEntryHeaders.AddNew();
				}

				return fEntryHeader;
			}
		}
		CusEntryHeader fEntryHeader;
	}
}
