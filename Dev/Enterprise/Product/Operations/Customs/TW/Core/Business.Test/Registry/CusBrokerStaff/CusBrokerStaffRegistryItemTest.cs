using System;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusBrokerStaffRegistryItem))]
	sealed class CusBrokerStaffRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<CusBrokerStaff>
	{
		protected override StronglyTypedRegistryItem<CusBrokerStaff, CusBrokerStaff> GetNewRegistryItem() => new CusBrokerStaffRegistryItem("", null, null, null, RegistryStorageFlags.BranchDepartment);
		protected override CusBrokerStaff ValidValue
		{
			get
			{
				var result = new CusBrokerStaff(new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK), Factory);
				result.BrokerStaffCode = "CYO";
				result.Mailbox = "TBK0461-0";
				return result;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			new TestTWCreator(Factory).CreateBrokerStaff();
		}
	}
}
