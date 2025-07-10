using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Web.WebService.Common.Testing
{
	public abstract class DataObjectInfoCollectionTestCase<ObjectInfo> : TestCaseWithFactory
			where ObjectInfo : DataObjectInfo
	{
		#region Test Cases

		public void TestConstructors()
		{
			AssertNotNull(Parent);
			AssertNotEquals(Parent, GetNewObjectInfoCollection());
			AssertEquals(GetExpectedCollectionType(), Parent.GetType());
		}

		public void TestAdd()
		{
			ObjectInfo info = GetNewObjectInfo();
			AssertEquals(0, Parent.Count);
			Parent.Add(info);
			AssertEquals(1, Parent.Count);
			AssertEquals(info, Parent[0]);
			AssertEquals(GetExpectedObjectInfoType(), Parent[0].GetType());
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			parent = GetNewObjectInfoCollection();
		}

		protected abstract Type GetExpectedCollectionType();
		protected abstract Type GetExpectedObjectInfoType();
		protected abstract ObjectInfo GetNewObjectInfo();
		protected abstract DataObjectInfoCollection<ObjectInfo> GetNewObjectInfoCollection();

		protected DataObjectInfoCollection<ObjectInfo> Parent
		{
			get
			{
				return parent;
			}
		}

		DataObjectInfoCollection<ObjectInfo> parent;

		#region Helper

		protected WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		#endregion

		#endregion
	}
}
