using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(PersonAssociationsOrgGroupWrapper))]
	sealed class PersonAssociationsOrgGroupWrapperTest : PersonAssociationsTreeBizObjWrapperTestCase<PersonAssociationsOrgGroupWrapper>
	{
		public void TestActive()
		{
			var wrapper = new PersonAssociationsOrgGroupWrapper(TreeModel, OrgGroup, null);
			AssertEquals(OrgGroup.OH_IsActive, wrapper.Active);
		}

		public void TestCity()
		{
			var wrapper = new PersonAssociationsOrgGroupWrapper(TreeModel, OrgGroup, null);
			AssertEquals(ZString.Empty, wrapper.City);
		}

		public void TestDescription()
		{
			var wrapper = new PersonAssociationsOrgGroupWrapper(TreeModel, OrgGroup, null);
			AssertEquals(OrgGroup.OH_FullName, wrapper.Description);
		}

		public void TestGrouping()
		{
			var wrapper = new PersonAssociationsOrgGroupWrapper(TreeModel, OrgGroup, null);
			AssertEquals("Management Group", wrapper.Grouping);
		}

		public void TestState()
		{
			var wrapper = new PersonAssociationsOrgGroupWrapper(TreeModel, OrgGroup, null);
			AssertEquals(ZString.Empty, wrapper.State);
		}

		#region Overrides

		protected override PersonAssociationsOrgGroupWrapper GetNewWrapper(PersonAssociationsTreeModel treeModel,
			IEnumerable<PersonAssociationsTreeBizObjWrapper> children)
		{
			return new PersonAssociationsOrgGroupWrapper(treeModel, Factory.New<OrgHeader>(),
				children?.Cast<PersonAssociationsOrganizationWrapper>());
		}

		protected override PersonAssociationsTreeBizObjWrapper GetNewChildWrapper(PersonAssociationsTreeModel treeModel)
		{
			var org = Factory.New<OrgHeader>();
			return new PersonAssociationsOrganizationWrapper(treeModel, org, org.Contacts.AddNew());
		}

		#endregion
	}
}
