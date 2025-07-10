using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(PersonAssociationsStaffWrapper))]
	sealed class PersonAssociationsStaffWrapperTest : PersonAssociationsTreeBizObjWrapperTestCase<PersonAssociationsStaffWrapper>
	{
		GlbSecurity GetSecurity(SecurityCheckpoint checkpoint, bool granted, ZGuid staffPk)
		{
			var security = Factory.New<GlbSecurity>();
			security.GU_SecurityRight = checkpoint.Code;
			security.GU_SecurityItemIsAllowed = granted;
			security.GU_GS = staffPk;

			return security;
		}

		void AssertDeniedSecurity(PersonAssociationsStaffWrapper wrapper)
		{
			var staffDenied = Factory.NewWithValidTestData<GlbStaff>();
			staffDenied.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.StaffViewOtherStaffDetails, false, staffDenied.PK));

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffDenied.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					AssertEquals(wrapper.ViewDeniedMessage, wrapper.City);
					AssertEquals(wrapper.ViewDeniedMessage, wrapper.State);
				}
			}
		}

		public void TestActive()
		{
			var wrapper = new PersonAssociationsStaffWrapper(TreeModel, Staff);
			AssertEquals(Staff.GS_IsActive, wrapper.Active);
		}

		public void TestCity()
		{
			var person = SetupStaffAndAddress();
			Staff.HomeBranch.GB_City = "city";

			var wrapper = new PersonAssociationsStaffWrapper(TreeModel, Staff);
			AssertEquals("city", wrapper.City);

			AssertDeniedSecurity(wrapper);
		}

		public void TestDescription()
		{
			var wrapper = new PersonAssociationsStaffWrapper(TreeModel, Staff);
			AssertEquals(Staff.GS_FullName, wrapper.Description);

			var staffFullName = Staff.GS_FullName;
			var initials = wrapper.GetInitials(staffFullName);
			var officeLocation = wrapper.GetOfficeLocation(Staff.HomeBranch);
			AssertEquals($"{staffFullName}{initials}{officeLocation}", wrapper.Description);

			var wrapper1 = new PersonAssociationsStaffWrapper(TreeModel, null);
		}

		public void TestGrouping()
		{
			var wrapper = new PersonAssociationsStaffWrapper(TreeModel, Staff);
			AssertEquals("Staff", wrapper.Grouping);
		}

		public void TestState()
		{
			var person = SetupStaffAndAddress();
			Staff.HomeBranch.GB_State = "state";

			var wrapper = new PersonAssociationsStaffWrapper(TreeModel, Staff);
			AssertEquals("state", wrapper.State);

			AssertDeniedSecurity(wrapper);
		}

		public void TestGetInitials()
		{
			var wrapper = new PersonAssociationsStaffWrapper(TreeModel, Staff);

			var initials1 = wrapper.GetInitials("John Smith");
			AssertEquals("J.S.", initials1);

			var initials2 = wrapper.GetInitials("John");
			AssertEquals(ZString.Empty, initials2);
		}

		public void TestOfficeLocation()
		{
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.City = "Sydney";

			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.City = ZString.Empty;

			var wrapper = new PersonAssociationsStaffWrapper(TreeModel, Staff);

			var officeLocation1 = wrapper.GetOfficeLocation(branch1);
			AssertEquals($"{branch1.City} Office", officeLocation1);

			var city2 = ZString.Empty;
			var officeLocation2 = wrapper.GetOfficeLocation(branch2);
			AssertEquals(ZString.Empty, officeLocation2);
		}

		public void TestGetIsPrimary()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var treeModel = new PersonAssociationsTreeModel(person);
			Staff.GS_PER = person.PK;
			person.SetPrimaryRelationship(Staff);

			var wrapper = new PersonAssociationsStaffWrapper(treeModel, Staff);
			AssertEquals(true, wrapper.IsPrimary);
		}

		public void TestSetIsPrimary()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var treeModel = new PersonAssociationsTreeModel(person);
			Staff.GS_PER = person.PK;
			var wrapper = new PersonAssociationsStaffWrapper(treeModel, Staff);
			AssertEquals(false, wrapper.IsPrimary);

			wrapper.IsPrimary = true;

			var primaryPivot = Factory.LoadFromUniqueKey<GlbPersonPrimaryRelationship>(GlbPersonPrimaryRelationshipSchema.PPR_PER, Staff.Person.PK);
			AssertEquals(Staff.PK, primaryPivot.PPR_PrimaryId);
		}

		public void TestWorkingAddressUNLOCO()
		{
			var person = SetupStaffAndAddress();

			var treeModel = new PersonAssociationsTreeModel(person);
			var wrapper = new PersonAssociationsStaffWrapper(treeModel, Staff);
			AssertEquals(Staff.HomeBranch.GB_RL_NKHomePort, wrapper.WorkingAddressUNLOCO);
		}

		public void TestCountry()
		{
			var person = SetupStaffAndAddress();

			var treeModel = new PersonAssociationsTreeModel(person);
			var wrapper = new PersonAssociationsStaffWrapper(treeModel, Staff);
			AssertEquals(Staff.HomeBranch.Country.RN_Desc, wrapper.Country);
		}

		public void TestEmail()
		{
			SetupStaffAndAddress();
			Staff.GS_EmailAddress = "email@addr.ess";

			var wrapper = new PersonAssociationsStaffWrapper(TreeModel, Staff);
			AssertEquals("email@addr.ess", wrapper.Email);

			AssertDeniedSecurity(wrapper);
		}

		#region Implementation

		GlbPerson SetupStaffAndAddress()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			Staff.GS_GB_HomeBranch = Factory.NewWithValidTestData<GlbBranch>().PK;
			Staff.HomeBranch.GB_RL_NKHomePort = "AUSYD";
			Staff.GS_PER = person.PK;
			person.SetPrimaryRelationship(Staff);

			return person;
		}

		#endregion

		#region Overrides

		protected override PersonAssociationsStaffWrapper GetNewWrapper(PersonAssociationsTreeModel treeModel, IEnumerable<PersonAssociationsTreeBizObjWrapper> children)
		{
			return new PersonAssociationsStaffWrapper(treeModel, Factory.New<GlbStaff>());
		}

		protected override PersonAssociationsTreeBizObjWrapper GetNewChildWrapper(PersonAssociationsTreeModel treeModel)
		{
			return null;
		}

		#endregion
	}
}
