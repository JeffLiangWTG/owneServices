using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class CusSupportingInfoCollectionTest<T> : BusinessObjectCollectionTestCase where T : CusSupportingInfo
	{
		public void TestSetDefaultValuesForNewChild()
		{
			var coll = GetCusSupportingInfoCollection();
			var element = coll.AddNew();
			AssertEquals(coll.CSI_Type, element.CSI_Type);
			AssertEquals(coll.Master.PK, element.Parent.PK);
		}

		public void TestCreateRelationshipFilter()
		{
			var coll = GetCusSupportingInfoCollection();
			coll.RemoveAll();

			var element1 = (CusSupportingInfo)GetNewElementToAddToTheCollection();
			var element2 = (CusSupportingInfo)GetNewElementToAddToTheCollection();
			var element3 = (CusSupportingInfo)GetNewElementToAddToTheCollection();
			var element4 = (CusSupportingInfo)GetNewElementToAddToTheCollection();
			coll.Add(element1);
			coll.Add(element2);
			coll.Add(element3);
			coll.Add(element4);

			AssertEquals("CSI_Type of the added element1 should match the collection's one", coll.CSI_Type, element1.CSI_Type);
			AssertEquals("CSI_Type of the added element2 should match the collection's one", coll.CSI_Type, element2.CSI_Type);
			AssertEquals("CSI_Type of the added element3 should match the collection's one", coll.CSI_Type, element3.CSI_Type);
			AssertEquals("CSI_Type of the added element4 should match the collection's one", coll.CSI_Type, element4.CSI_Type);

			if (!coll.CSI_SubType.IsEmpty)
			{
				AssertEquals("CSI_SubType of the added element1 should match the collection's one", coll.CSI_SubType, element1.CSI_SubType);
				AssertEquals("CSI_SubType of the added element2 should match the collection's one", coll.CSI_SubType, element2.CSI_SubType);
				AssertEquals("CSI_SubType of the added element3 should match the collection's one", coll.CSI_SubType, element3.CSI_SubType);
				AssertEquals("CSI_SubType of the added element4 should match the collection's one", coll.CSI_SubType, element4.CSI_SubType);
			}

			if (!coll.CSI_CSI_SupportingInfo.IsEmpty)
			{
				AssertEquals("CSI_CSI_SupportingInfo of the added element1 should match the collection's one", coll.CSI_CSI_SupportingInfo, element1.CSI_CSI_SupportingInfo);
				AssertEquals("CSI_CSI_SupportingInfo of the added element2 should match the collection's one", coll.CSI_CSI_SupportingInfo, element2.CSI_CSI_SupportingInfo);
				AssertEquals("CSI_CSI_SupportingInfo of the added element3 should match the collection's one", coll.CSI_CSI_SupportingInfo, element3.CSI_CSI_SupportingInfo);
				AssertEquals("CSI_CSI_SupportingInfo of the added element4 should match the collection's one", coll.CSI_CSI_SupportingInfo, element4.CSI_CSI_SupportingInfo);
			}

			element2.CSI_Type = "AGA";
			element3.CSI_SubType = "#";
			element4.CSI_CSI_SupportingInfo = ZGuid.NewZGuid();
			AssertNotEquals("PreCondition:CSI_Type passed into the collection is not the same as element2's", element2.CSI_Type, coll.CSI_Type);
			AssertNotEquals("PreCondition:CSI_SubType passed into the collection is not the same as element3's", element3.CSI_SubType, coll.CSI_SubType);
			AssertNotEquals("PreCondition:CSI_CSI_SupportingInfo passed into the collection is not the same as element4's", element4.CSI_CSI_SupportingInfo, coll.CSI_CSI_SupportingInfo);

			coll.Load();

			Assert("contains element1", coll.Contains(element1));
			Assert("should not contain element2", !coll.Contains(element2));
			AssertEquals("contains element3 if CSI_SubType is empty", coll.CSI_SubType.IsEmpty, coll.Contains(element3));
			AssertEquals("contains element4 if CSI_CSI_SupportingInfo is empty", coll.CSI_CSI_SupportingInfo.IsEmpty, coll.Contains(element4));
		}

		protected abstract CusSupportingInfoCollection<T> GetCusSupportingInfoCollection();

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return GetCusSupportingInfoCollection();
		}
	}
}
