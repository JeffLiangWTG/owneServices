using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(GlbAccreditationAttemptController))]
	public class GlbAccreditationAttemptControllerTest : ZControllerBasherTest
	{
		public void TestModuleID()
		{
			var controller = new GlbAccreditationAttemptController();
			AssertEquals(ModuleIDs.GlbAccreditationAttempt, controller.ModuleID);
		}

		public void TestGetForm()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var accred = Factory.NewWithValidTestData<GlbAccreditation>();
			var attempt = Factory.New<GlbAccreditationAttempt>();
			attempt.HAA_PER = person.PK;
			attempt.HAA_HAC = accred.PK;
			var controller = new GlbAccreditationAttemptControllerForTest();
			using (var form = controller.GetFormExposed(attempt))
			{
				AssertEquals(typeof(GlbPersonForm), form.GetType());
			}
		}

		public override void TestNewForm()
		{
			AssertControllerNotNull();
			try
			{
				AssertEquals("Should not show new form", null, Controller.ShowNewForm());
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
		}

		public override void TestDeleteForm()
		{
			AssertControllerNotNull();
			try
			{
				AssertEquals("Should not show new form", null, Controller.ShowDeleteForm(null));
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
		}

		#region Implementation
		class GlbAccreditationAttemptControllerForTest : GlbAccreditationAttemptController
		{
			public IZForm GetFormExposed(IBusiness businessEntity)
			{
				return GetForm(businessEntity);
			}
		}

		protected override Type GetBusinessObjectType()
		{
			return typeof(GlbAccreditationAttempt);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.GlbAccreditationAttempt;
		}
		#endregion
	}
}
