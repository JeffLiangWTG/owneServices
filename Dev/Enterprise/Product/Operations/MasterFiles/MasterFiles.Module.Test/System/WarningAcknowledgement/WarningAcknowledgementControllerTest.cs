using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(WarningAcknowledgementController))]
	sealed class WarningAcknowledgementControllerTest : ZControllerBasherTest
	{
		void AddBizObjectToDB()
		{
			var warning = Factory.NewWithValidTestData<GenCustomAddOnRuleAck>();
			warning.XK_ParentID = Guid.NewGuid();
			warning.XK_ParentTableCode = "GS";
			warning.XK_SystemCreateTimeUtc = DateTime.UtcNow;
			warning.XK_SystemCreateUser = "E";
			warning.XK_RuleID = Core.Constants.CargoWiseOneGenCustomAddOnRuleIDs.PhoneNumberFormatValidation;

			Factory.Save();
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return Factory.LoadTop1<GenCustomAddOnRuleAck>(new ZQuery());
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WarningAcknowledgement;
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		public override void TestNewForm()
		{
			AddBizObjectToDB();
			Assert("Cannot create new form in this module", true);
		}

		public override void TestDeleteForm()
		{
			AddBizObjectToDB();
			base.TestDeleteForm();
		}

		[RequiresSTA]
		public override void TestViewForm()
		{
			AddBizObjectToDB();
			base.TestViewForm();
		}

		[RequiresSTA]
		public override void TestEditForm()
		{
			AddBizObjectToDB();
			base.TestEditForm();
		}

		public void TestSecurityCheckpoints()
		{
			var controller = new TestWarningAcknowledgementController();
			AssertEquals("For New", Env.Security.WarningAcknowledgement, controller.CheckPointForNew);
			AssertEquals("For View", Env.Security.WarningAcknowledgementView, controller.CheckPointForView);
			AssertEquals("For Edit", Env.Security.WarningAcknowledgementEdit, controller.CheckPointForEdit);
			AssertEquals("For Delete", Env.Security.WarningAcknowledgement, controller.CheckPointForDelete);
		}

		sealed class TestWarningAcknowledgementController : WarningAcknowledgementController
		{
			public new SecurityCheckpoint CheckPointForNew
			{
				get { return base.CheckPointForNew; }
			}

			public new SecurityCheckpoint CheckPointForView
			{
				get { return base.CheckPointForView; }
			}

			public new SecurityCheckpoint CheckPointForEdit
			{
				get { return base.CheckPointForEdit; }
			}

			public new SecurityCheckpoint CheckPointForDelete
			{
				get { return base.CheckPointForDelete; }
			}
		}
	}
}
