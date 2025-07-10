using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusSupportingInfoCollection<CusSupportingInfo>))]
	sealed class CusSupportingInfoCollectionBaseOnlyTest : CusSupportingInfoCollectionTest<CusSupportingInfo>
	{
		public void TestAddCloneFrom()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var collection = new CusSupportingInfoCollection<CusSupportingInfo>(invoice, "OTH");
			var data = collection.AddNew();
			data.CSI_Code = "YO";

			var invoice2 = Factory.New<BaseJobComInvoiceHeader>();
			var collection2 = new CusSupportingInfoCollection<CusSupportingInfo>(invoice2, "OTH");
			AssertEquals(0, collection2.Count);
			collection2.AddCloneFrom(collection, new BusinessObjectCloneArgs());
			AssertEquals(1, collection2.Count);
			var clonedValue = collection2[0];
			AssertEquals(invoice2.PK, clonedValue.CSI_ParentID);
			AssertEquals(invoice2.TablePrefix, clonedValue.CSI_ParentTableCode);
			AssertEquals("OTH", clonedValue.CSI_Type);
			AssertEquals("YO", clonedValue.CSI_Code);
			AssertEquals(false, clonedValue.HasChanges);
		}

		public void TestCreateRelationshipFilter_WithCSI_CSI_SupportingInfo()
		{
			var coll = new CusSupportingInfoCollection<CusSupportingInfo>(Factory.New<BaseJobComInvoiceHeader>(), "OTH", ZGuid.NewZGuid());

			var element1 = (CusSupportingInfo)GetNewElementToAddToTheCollection();
			var element2 = (CusSupportingInfo)GetNewElementToAddToTheCollection();
			var element3 = (CusSupportingInfo)GetNewElementToAddToTheCollection();
			coll.Add(element1);
			coll.Add(element2);
			coll.Add(element3);
			CombineAssertions(() =>
			{
				element2.CSI_Type = "AGA";
				element3.CSI_CSI_SupportingInfo = ZGuid.NewZGuid();
				AssertNotEquals("PreCondition:CSI_Type passed into the collection is not the same as element2's", element2.CSI_Type, coll.CSI_Type);
				AssertNotEquals("PreCondition:CSI_CSI_SupportingInfo passed into the collection is not the same as element3's", element3.CSI_CSI_SupportingInfo, coll.CSI_CSI_SupportingInfo);

				coll.Load();

				Assert("contains element1", coll.Contains(element1));
				Assert("should not contain element2", !coll.Contains(element2));
				Assert("contains element3", !coll.Contains(element3));
			});
		}

		public void TestCreateRelationshipFilter_WithCSI_SubType()
		{
			var coll = new CusSupportingInfoCollection<CusSupportingInfo>(Factory.New<BaseJobComInvoiceHeader>(), "OTH", "INF");

			var element1 = (CusSupportingInfo)GetNewElementToAddToTheCollection();
			var element2 = (CusSupportingInfo)GetNewElementToAddToTheCollection();
			var element3 = (CusSupportingInfo)GetNewElementToAddToTheCollection();
			coll.Add(element1);
			coll.Add(element2);
			coll.Add(element3);
			element2.CSI_Type = "AGA";
			element3.CSI_SubType = "AGA";
			AssertNotEquals("PreCondition:CSI_Type passed into the collection is not the same as element2's", element2.CSI_Type, coll.CSI_Type);
			AssertNotEquals("PreCondition:CSI_SubType passed into the collection is not the same as element3's", element3.CSI_SubType, coll.CSI_SubType);

			coll.Load();

			AssertEquals("contains element1", true, coll.Contains(element1));
			AssertEquals("should not contain element2", false, coll.Contains(element2));
			AssertEquals("contains element3", false, coll.Contains(element3));
		}

		public void TestSetCollectionRelationships_CSI_Type_Only()
		{
			var coll = new CusSupportingInfoCollection<CusSupportingInfo>(Factory.New<BaseJobComInvoiceHeader>(), "OTH");

			var element1 = (CusSupportingInfo)GetNewElementToAddToTheCollection();
			element1.CSI_Type = "XXX";
			element1.CSI_SubType = "AAA";

			coll.Add(element1);

			AssertEquals("CSI_Type of the added element1 should match the collection's one", coll.CSI_Type, element1.CSI_Type);

			AssertEquals("CSI_SubType of the added element1 should remain", "AAA", element1.CSI_SubType);
		}

		public void TestSetCollectionRelationships_WithCSI_SubType()
		{
			var coll = new CusSupportingInfoCollection<CusSupportingInfo>(Factory.New<BaseJobComInvoiceHeader>(), "OTH", "INF");

			var element1 = (CusSupportingInfo)GetNewElementToAddToTheCollection();
			element1.CSI_Type = "XXX";
			element1.CSI_SubType = "AAA";

			coll.Add(element1);

			AssertEquals("CSI_Type of the added element1 should match the collection's one", coll.CSI_Type, element1.CSI_Type);
			AssertEquals("CSI_SubType of the added element1 should match the collection's one", coll.CSI_SubType, element1.CSI_SubType);
		}

		public void TestSetCollectionRelationships_WithCSI_CSI_SupportingInfo()
		{
			var coll = new CusSupportingInfoCollection<CusSupportingInfo>(Factory.New<BaseJobComInvoiceHeader>(), "OTH", ZGuid.NewZGuid());

			var element1 = (CusSupportingInfo)GetNewElementToAddToTheCollection();
			element1.CSI_Type = "XXX";
			element1.CSI_CSI_SupportingInfo = ZGuid.NewZGuid();

			coll.Add(element1);

			CombineAssertions(() =>
			{
				AssertEquals("CSI_Type of the added element1 should match the collection's one", coll.CSI_Type, element1.CSI_Type);
				AssertEquals("CSI_CSI_SupportingInfo of the added element1 should match the collection's one", coll.CSI_CSI_SupportingInfo, element1.CSI_CSI_SupportingInfo);
			});
		}

		protected override CusSupportingInfoCollection<CusSupportingInfo> GetCusSupportingInfoCollection()
		{
			return new CusSupportingInfoCollection<CusSupportingInfo>(Factory.New<BaseJobComInvoiceHeader>(), "OTH");
		}
	}
}
