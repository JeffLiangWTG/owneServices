using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class MergeOrgElementTest : NonPersistentBusinessObjectTestCase
	{
		public void TestNewOrgElementCollection()
		{
			IMergeOrgElement dummy = NewTestMergeElement();
			BusinessObjectCollection col1 = dummy.GetNewObjectsCollection(Factory, new ZQuery());
			BusinessObject obj1 = Factory.NewWithValidTestData(MasterBusinessObjectType);
			col1.Add(obj1);

			IMergeOrgElement testObj1 = NewTestMergeElement();
			IMergeOrgElement testObj2 = NewTestMergeElement(col1);

			List<BusinessObject> list1 = new List<BusinessObject>(testObj1.NewObjectsCollection);
			List<BusinessObject> list2 = new List<BusinessObject>(testObj2.NewObjectsCollection);

			Assert(!list1.Contains(obj1));
			Assert(list2.Contains(obj1));
		}

		public void TestConstructor()
		{
			IMergeOrgElement testObj = NewTestMergeElement();

			AssertNotEquals("New should be added", 0, new List<BusinessObject>(testObj.NewObjectsCollection).Count);
		}

		public void TestValidateNewObjectPK()
		{
			IMergeOrgElement testObj = NewTestMergeElement();
			AssertNoErrors(testObj.NewObjectPKInfo);
			(testObj as BusinessObject).RunPreSaveValidation();

			testObj.NewObjectPK = ZGuid.Invalid;
			AssertHasErrors(testObj.NewObjectPKInfo);

			BusinessObjectCollection col = GetMergeOrgElementCollection();
			BusinessObject obj1 = NewTestMergeElement() as BusinessObject;
			BusinessObject obj2 = NewTestMergeElement() as BusinessObject;
			col.Add(obj1);
			col.Add(obj2);
			(col as IParentCollectionSetter).SetParentCollection();

			obj1.RunPreSaveValidation();
			Assert(!obj1.HasErrors);

			(obj1 as IMergeOrgElement).NewObjectPK = ZGuid.Invalid;
			obj1.RunPreSaveValidation();
			Assert(obj1.HasErrors);
		}

		public void TestNewBusinessObjectCollection()
		{
			BusinessObjectCollection col = GetMergeOrgElementCollection();
			BusinessObject obj1 = NewTestMergeElement() as BusinessObject;
			BusinessObject obj2 = NewTestMergeElement() as BusinessObject;
			col.Add(obj1);
			col.Add(obj2);
			int count = new List<BusinessObject>((obj1 as IMergeOrgElement).NewObjectsCollection).Count;
			(col as IParentCollectionSetter).SetParentCollection();
			(obj1 as IMergeOrgElement).Action = "ADD";
			(obj2 as IMergeOrgElement).Action = "ADD";
			AssertEquals("should be one more object as parent collection is set", count + 1, new List<BusinessObject>((obj1 as IMergeOrgElement).NewObjectsCollection).Count);
		}

		public void TestNewObjectPkReadOnly()
		{
			IMergeOrgElement testObj = NewTestMergeElement();
			testObj.Action = "MRG";
			Assert("should be editable for MRG action", !testObj.NewObjectPKInfo.ReadOnly);
			testObj.Action = "ADD";
			Assert("should be readonly for ADD action", testObj.NewObjectPKInfo.ReadOnly);
		}

		public void TestColumnsForMatching()
		{
			IMergeOrgElement element = NewTestMergeElement();
			foreach (SchemaColumn col in ColumnsFotMatching)
			{
				AssertCollectionContains(col, element.ColumnsForMatching);
			}
		}

		protected abstract Type MasterBusinessObjectType { get; }

		protected abstract SchemaColumn[] ColumnsFotMatching { get; }

		public abstract void TestValidateFuzzyMatching();

		protected IMergeOrgElement NewTestMergeElement()
		{
			BusinessObjectCollection col = null;
			return NewTestMergeElement(col);
		}

		protected abstract IMergeOrgElement NewTestMergeElement(BusinessObjectCollection newOrgElementCollection);

		protected virtual IMergeOrgElement NewTestMergeElement(ZString property1)
		{
			return NewTestMergeElement();
		}

		protected virtual IMergeOrgElement NewTestMergeElement(ZString property1, ZString property2)
		{
			return NewTestMergeElement();
		}

		protected abstract BusinessObjectCollection GetMergeOrgElementCollection();
	}
}
