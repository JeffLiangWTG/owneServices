using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CusBrokerStaffLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestMailboxList()
		{
			parent.BrokerStaffCode = "CYO";
			var list = parent.Lookups.MailboxList;
			NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode("TBK0461-0"), NUnit.Framework.Is.EqualTo(PasswordTypesList.Codes.UVC));
		}

		[ExpectNoExceptions]
		public void TestBrokerStaffList()
		{
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			resource.GS_IsResource = true;
			var cantLoginStaff = Factory.NewWithValidTestData<GlbStaff>();
			cantLoginStaff.GS_CanLogin = false;
			Factory.Save();
			var list = parent.Lookups.BrokerStaffList;
			NUnit.Framework.Assert.That(list, NUnit.Framework.Has.Some.EqualTo(cantLoginStaff).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(list, NUnit.Framework.Has.None.EqualTo(resource).Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			new TestTWCreator(Factory).CreateBrokerStaff();
			parent = new CusBrokerStaff(new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK), Factory);
		}

		CusBrokerStaff parent;
	}
}
