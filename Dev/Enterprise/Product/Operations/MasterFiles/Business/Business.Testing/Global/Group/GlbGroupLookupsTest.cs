using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	class GlbGroupLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCompleteGroupList()
		{
			AssertNotNull("CompleteGroupList", Group.Lookups.CompleteGroupList);
		}

		public void TestCompleteStaffList()
		{
			AssertNotNull(Group.Lookups.CompleteStaffList);
		}

		public void TestCompleteOrganisationList()
		{
			AssertNotNull(Group.Lookups.CompleteOrganisationList);
		}

		public void TestNationalityTypes()
		{
			AssertNotNull(Group.Lookups.Branches);
		}

		public void TestDepartments()
		{
			AssertNotNull(Group.Lookups.Departments);
		}

		public void TestDomainNames()
		{
			var domain1 = ObjectFactory.Get<IDomainCredentials>();
			domain1.DomainName = "domain1";
			var domain2 = ObjectFactory.Get<IDomainCredentials>();
			domain2.DomainName = "domain2";
			var domain3 = ObjectFactory.Get<IDomainCredentials>();
			domain3.DomainName = "domain3";

			var adRegistry = ObjectFactory.Get<IADRegistry>();
			adRegistry.DomainCredentialsCollection = new List<IDomainCredentials>() { domain1, domain2, domain3 };

			AssertNotNull(Group.Lookups.DomainNames);
			AssertEquals(3, Group.Lookups.DomainNames.Count);
			Assert(Group.Lookups.DomainNames.ContainsCode("domain1"));
			Assert(Group.Lookups.DomainNames.ContainsCode("domain2"));
			Assert(Group.Lookups.DomainNames.ContainsCode("domain3"));
		}

		public void TestTypes()
		{
			AssertEquals(2, Group.Lookups.Types.Count);
			Assert(Group.Lookups.Types.ContainsCode(GlbGroupTypeList.Codes.Staff));
			Assert(Group.Lookups.Types.ContainsCode(GlbGroupTypeList.Codes.Organisation));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Group = Factory.New<GlbGroup>();
		}

		GlbGroup Group;

		#endregion
	}
}
