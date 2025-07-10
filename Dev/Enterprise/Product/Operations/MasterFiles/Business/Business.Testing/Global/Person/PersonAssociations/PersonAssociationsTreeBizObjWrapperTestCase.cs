using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Recruiter;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class PersonAssociationsTreeBizObjWrapperTestCase<T> : NonPersistentBusinessObjectTestCase
			where T : PersonAssociationsTreeBizObjWrapper
	{
		protected PersonAssociationsTreeBizObjWrapperTestCase()
		{
			CreateTestData();
		}

		#region Properties

		protected RefCountry Country { get; private set; }

		protected IHRJobApplicant JobApplicant { get; private set; }

		protected OrgHeader OrgGroup { get; private set; }

		protected OrgHeader Organization { get; private set; }

		protected GlbPerson Person { get; private set; }

		protected GlbStaff Staff { get; private set; }

		protected PersonAssociationsTreeModel TreeModel { get; private set; }

		#endregion

		#region Implementation

		protected abstract T GetNewWrapper(PersonAssociationsTreeModel treeModel,
			IEnumerable<PersonAssociationsTreeBizObjWrapper> children);

		protected abstract PersonAssociationsTreeBizObjWrapper GetNewChildWrapper(PersonAssociationsTreeModel treeModel);

		protected override BusinessObject GetNewBusinessObject()
		{
			var childWrapper = GetNewChildWrapper(TreeModel);
			var children = childWrapper != null ? new[] { childWrapper } : null;
			return GetNewWrapper(TreeModel, children);
		}

		void CreateTestData()
		{
			Country = Factory.NewWithValidTestData<RefCountry>();
			JobApplicant = Factory.New<IHRJobApplicant>();
			JobApplicant.HA_FullName = "name";
			OrgGroup = Factory.NewWithValidTestData<OrgHeader>();
			Organization = Factory.NewWithValidTestData<OrgHeader>();
			Person = Factory.NewWithValidTestData<GlbPerson>();
			Person.PER_FullName = "name2";
			Staff = Factory.NewWithValidTestData<GlbStaff>();
			Staff.GS_GB_HomeBranch = Factory.NewWithValidTestData<GlbBranch>().PK;
			TreeModel = new PersonAssociationsTreeModel(Person);
		}

		#endregion
	}
}
