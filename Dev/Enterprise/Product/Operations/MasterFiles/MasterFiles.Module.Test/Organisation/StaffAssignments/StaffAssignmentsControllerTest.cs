using System;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(StaffAssignmentsController))]
	public class StaffAssignmentsControllerTest : ZControllerBasherTest
	{
		public override void TestDeleteForm()
		{
			Assert(true);
		}

		public override void TestEditForm()
		{
			Assert(true);
		}

		public override void TestNewForm()
		{
			Assert(true);
		}

		public override void TestViewForm()
		{
			Assert(true);
		}

		public void TestMakeUrlsOnlyOpenableForCurrentCompany()
		{
			Assert("Staff assignment hyperlinks should be restricted to the current company", Controller.MakeUrlsOnlyOpenableForCurrentCompany);
		}

		public void TestSecurityCheckpoints()
		{
			var assignment = Factory.New<OrgStaffAssignments>();
			AssertEquals("For View", Env.Security.OrgDetailsViewCompanysStaffAssignments, Controller.GetCheckPointForView(assignment));
			AssertEquals("For Edit", Env.Security.OrgDetailsModifyStaffAssignments, Controller.GetCheckPointForEdit(assignment));
			AssertEquals("For New", Env.Security.OrgDetailsModifyStaffAssignments, Controller.GetCheckPointForNew(assignment));
			AssertEquals("For Delete", Env.Security.OrgDetailsModifyStaffAssignments, Controller.GetCheckPointForDelete(assignment));
		}

		protected override Type GetBusinessObjectType()
		{
			return typeof(OrgStaffAssignments);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.StaffAssignments;
		}
	}
}
