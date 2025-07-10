using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusStatus))]
	public class CusStatusTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCode()
		{
			Dummy.Status = "foo";
			AssertEquals("vaue of code reflects dummy.Status", "foo", Status.Code);
		}

		public void TestCodeInfo()
		{
			AssertNotNull("nullness", Status.CodeInfo);
			AssertEquals("type", typeof(ZWrappedPropertyInfo), Status.CodeInfo.GetType());
			AssertEquals("readonly", true, Status.CodeInfo.ReadOnly);
		}

		public void TestDescription()
		{
			Dummy.Status = "bar";
			AssertEquals("description is correctly looked up", "bar description", Status.Description);
		}

		public void TestDescriptionInfo()
		{
			AssertNotNull("nullness", Status.DescriptionInfo);
			AssertEquals("type", typeof(ZPropertyInfoString), Status.DescriptionInfo.GetType());
		}

		public void TestUserFriendlyStatuses()
		{
			AssertEquals(ZString.Empty, Status.UserFriendlyStatuses);
		}

		public void TestUserFriendlyStatusesInfo()
		{
			AssertNotNull(Status.UserFriendlyStatusesInfo);
			AssertEquals("type", typeof(ZPropertyInfoString), Status.UserFriendlyStatusesInfo.GetType());
		}

		public virtual void TestStatusList()
		{
			AssertNotNull("nullness", Status.StatusList);
			AssertEquals("type", typeof(CodeDescriptionPairList), Status.StatusList.GetType());
		}

		public void TestDefaultValue()
		{
			AssertEquals("dummy initially has no value set", ZString.Empty, Dummy.Status);
			AssertEquals("status return default value", "bar", Status.Code);
			AssertEquals("dummy is still empty", ZString.Empty, Dummy.Status);
		}

		#region Implementation

		#region TestHelpers

		protected class DummyBizOWithStatus : DummyBusinessObject
		{
			public DummyBizOWithStatus(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public abstract new class Schema : DummyBaseBusinessObject.Schema
			{
				public const string Status = "Status";
			}

			public ZString Status
			{
				set { SetNonPersistentPropertyValue(StatusInfo, ref fStatus, value); }
				get { return fStatus; }
			}
			ZString fStatus;

			public ZPropertyInfo StatusInfo
			{
				get { return GetZPropertyInfo(Schema.Status); }
			}
		}

		#endregion

		#region Properties

		protected CodeDescriptionPairList List
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair("foo", "foo description");
				result.AddPair("bar", "bar description");
				return result;
			}
		}

		CusStatus Status
		{
			get
			{
				if (fStatus == null)
				{
					fStatus = new CusStatus(Dummy.StatusInfo, List, "bar");
				}
				return fStatus;
			}
		}
		CusStatus fStatus;

		protected DummyBizOWithStatus Dummy
		{
			get
			{
				if (fDummy == null)
				{
					fDummy = Factory.New<DummyBizOWithStatus>();
				}
				return fDummy;
			}
		}
		DummyBizOWithStatus fDummy;

		#endregion

		#region TestCase Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return Status;
		}

		#endregion

		#endregion
	}
}
