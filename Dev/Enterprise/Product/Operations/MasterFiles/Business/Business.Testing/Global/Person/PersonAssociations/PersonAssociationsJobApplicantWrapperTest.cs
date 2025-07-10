using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Recruiter;
using Enterprise.Security;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(PersonAssociationsJobApplicantWrapper))]
	sealed class PersonAssociationsJobApplicantWrapperTest : PersonAssociationsTreeBizObjWrapperTestCase<PersonAssociationsJobApplicantWrapper>
	{
		public void TestActive()
		{
			var wrapper = new PersonAssociationsJobApplicantWrapper(TreeModel, JobApplicant);
			AssertEquals(true, wrapper.Active);
		}

		GlbSecurity GetSecurity(SecurityCheckpoint checkpoint, bool granted, ZGuid staffPk)
		{
			var security = Factory.New<GlbSecurity>();
			security.GU_SecurityRight = checkpoint.Code;
			security.GU_SecurityItemIsAllowed = granted;
			security.GU_GS = staffPk;

			return security;
		}

		void AssertDeniedSecurity(PersonAssociationsJobApplicantWrapper wrapper)
		{
			var staffDenied = Factory.NewWithValidTestData<GlbStaff>();
			staffDenied.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.HRJobApplicantView, false, staffDenied.PK));

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffDenied.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					AssertEquals(wrapper.ViewDeniedMessage, wrapper.City);
					AssertEquals(wrapper.ViewDeniedMessage, wrapper.State);
				}
			}
		}

		public void TestCity()
		{
			var wrapper = new PersonAssociationsJobApplicantWrapper(TreeModel, JobApplicant);
			AssertEquals(JobApplicant.HA_City, wrapper.City);

			AssertDeniedSecurity(wrapper);
		}

		public void TestEmail()
		{
			var wrapper = new PersonAssociationsJobApplicantWrapper(TreeModel, JobApplicant);
			AssertEquals(JobApplicant.HA_EmailAddress, wrapper.Email);

			AssertDeniedSecurity(wrapper);
		}

		public void TestDescription()
		{
			var wrapper = new PersonAssociationsJobApplicantWrapper(TreeModel, JobApplicant);
			AssertEquals(JobApplicant.HA_FullName, wrapper.Description);

			AssertDeniedSecurity(wrapper);
		}

		public void TestGrouping()
		{
			var wrapper = new PersonAssociationsJobApplicantWrapper(TreeModel, JobApplicant);
			AssertEquals("Applicant", wrapper.Grouping);

			AssertDeniedSecurity(wrapper);
		}

		public void TestState()
		{
			var wrapper = new PersonAssociationsJobApplicantWrapper(TreeModel, JobApplicant);
			AssertEquals(JobApplicant.HA_State, wrapper.State);

			AssertDeniedSecurity(wrapper);
		}

		#region Overrides

		protected override PersonAssociationsJobApplicantWrapper GetNewWrapper(PersonAssociationsTreeModel treeModel,
			IEnumerable<PersonAssociationsTreeBizObjWrapper> children)
		{
			return new PersonAssociationsJobApplicantWrapper(treeModel, Factory.New<IHRJobApplicant>());
		}

		protected override PersonAssociationsTreeBizObjWrapper GetNewChildWrapper(PersonAssociationsTreeModel treeModel)
		{
			return null;
		}

		#endregion
	}
}
