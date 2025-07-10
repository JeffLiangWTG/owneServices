using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(PersonAssociationsOrganizationWrapper))]
	sealed class PersonAssociationsOrganizationWrapperTest : PersonAssociationsTreeBizObjWrapperTestCase<PersonAssociationsOrganizationWrapper>
	{
		GlbSecurity GetSecurity(SecurityCheckpoint checkpoint, bool granted, ZGuid staffPk)
		{
			var security = Factory.New<GlbSecurity>();
			security.GU_SecurityRight = checkpoint.Code;
			security.GU_SecurityItemIsAllowed = granted;
			security.GU_GS = staffPk;

			return security;
		}

		public void TestProperties()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = ZGuid.NewZGuid().ToString().Substring(0, 5);
			org.OH_RL_NKClosestPort = "AUCAP";
			var matchingAddress = org.Addresses.Cast<OrgAddress>().FirstOrDefault(x => x.OA_RL_NKRelatedPortCode == "AUCAP");
			AssertNotNull(matchingAddress);
			matchingAddress.OA_Address1 = "address1";
			matchingAddress.OA_City = "city1";
			matchingAddress.OA_State = "state1";
			var nonMatchingAddress = org.Addresses.AddNew();
			nonMatchingAddress.OA_Address1 = "address2";
			nonMatchingAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			nonMatchingAddress.OA_City = "city2";
			nonMatchingAddress.OA_State = "state2";

			var contact = org.Contacts.AddNew();
			contact.OC_Mobile = "0499702888";
			contact.OC_Phone = "0280002200";
			contact.OC_Email = "email@addr.ess";

			Factory.Save();

			var wrapper = new PersonAssociationsOrganizationWrapper(TreeModel, org, contact);
			AssertEquals("city1", wrapper.City);
			AssertEquals("state1", wrapper.State);
			AssertEquals("email@addr.ess", wrapper.Email);

			var staffDenied = Factory.NewWithValidTestData<GlbStaff>();
			staffDenied.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.OrgContactView, false, staffDenied.PK));

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffDenied.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					AssertEquals(wrapper.ViewDeniedMessage, wrapper.City);
					AssertEquals(wrapper.ViewDeniedMessage, wrapper.State);
					AssertEquals(wrapper.ViewDeniedMessage, wrapper.Email);
				}
			}
		}

		public void TestDescription()
		{
			var wrapper = new PersonAssociationsOrganizationWrapper(TreeModel, Organization, Organization.Contacts.AddNew());
			AssertEquals(Organization.OH_FullName, wrapper.Description);
		}

		public void TestGrouping()
		{
			var wrapper = new PersonAssociationsOrganizationWrapper(TreeModel, Organization, Factory.New<OrgContact>());
			AssertEquals(Organization.OH_Code, wrapper.Grouping);
		}

		public void TestGetIsPrimary()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var treeModel = new PersonAssociationsTreeModel(person);
			var contact = Organization.Contacts.AddNew();
			contact.OC_PER = person.PK;
			person.SetPrimaryRelationship(contact);

			var wrapper = new PersonAssociationsOrganizationWrapper(treeModel, Organization, contact);
			AssertEquals(true, wrapper.IsPrimary);
		}

		public void TestSetIsPrimary()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var treeModel = new PersonAssociationsTreeModel(person);
			var contact = Organization.Contacts.AddNew();
			contact.OC_PER = person.PK;
			var wrapper = new PersonAssociationsOrganizationWrapper(treeModel, Organization, contact);
			AssertEquals(false, wrapper.IsPrimary);

			wrapper.IsPrimary = true;

			AssertEquals(true, wrapper.IsPrimary);
			var primaryPivot = Factory.LoadFromUniqueKey<GlbPersonPrimaryRelationship>(GlbPersonPrimaryRelationshipSchema.PPR_PER, contact.Person.PK);
			AssertEquals(contact.PK, primaryPivot.PPR_PrimaryId);
		}

		public void TestWorkingAddressUNLOCO()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var contact = Organization.Contacts.AddNew();
			contact.OC_OA_OrgAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			contact.OrgAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			contact.OC_PER = person.PK;
			person.SetPrimaryRelationship(contact);

			var treeModel = new PersonAssociationsTreeModel(person);
			var wrapper = new PersonAssociationsOrganizationWrapper(treeModel, Organization, contact);
			AssertEquals(contact.OrgAddress.OA_RL_NKRelatedPortCode, wrapper.WorkingAddressUNLOCO);
		}

		public void TestCountry()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var contact = Organization.Contacts.AddNew();
			contact.OC_OA_OrgAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			contact.OrgAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			contact.OC_PER = person.PK;
			person.SetPrimaryRelationship(contact);

			var treeModel = new PersonAssociationsTreeModel(person);
			var wrapper = new PersonAssociationsOrganizationWrapper(treeModel, Organization, contact);
			AssertEquals(contact.OrgAddress.EffectiveRelatedPortCode.Country.RN_Desc, wrapper.Country);
		}

		public void TestActive()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var contact = Organization.Contacts.AddNew();
			contact.OC_IsActive = false;

			var treeModel = new PersonAssociationsTreeModel(person);
			var wrapper = new PersonAssociationsOrganizationWrapper(treeModel, Organization, contact);
			Assert(!wrapper.Active);

			contact.OC_IsActive = true;
			Assert(wrapper.Active);
		}

		#region Overrides

		protected override PersonAssociationsOrganizationWrapper GetNewWrapper(PersonAssociationsTreeModel treeModel,
			IEnumerable<PersonAssociationsTreeBizObjWrapper> children)
		{
			return new PersonAssociationsOrganizationWrapper(treeModel, Factory.New<OrgHeader>(), Factory.New<OrgContact>());
		}

		protected override PersonAssociationsTreeBizObjWrapper GetNewChildWrapper(PersonAssociationsTreeModel treeModel)
		{
			return null;
		}

		#endregion
	}
}
