using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Environment;

namespace Enterprise.Customs.US.AMS.Messaging.Testing
{
	sealed class AMSMessageSendingValidationTest : TestCaseWithFactory
	{
		public void TestTreatMessageErrorsAsErrorsWhenSendingMessageErrorsIsAllowed()
		{
			var oldAllowed = Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed;
			try
			{
				Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;
				var declaration = Factory.NewWithValidTestData<DummyDeclaration>();
				var validation = AMSMessageSendingValidation.New(declaration, false, null);
				var notifications = validation.CheckBusinessObjectLevelValidation();
				AssertEquals(5, notifications.Count);
				Assert(notifications.All(n => n.IsError));
				Assert(notifications.Any(n => n.Message.Contains("Some error message")));
			}
			finally
			{
				Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = oldAllowed;
			}
		}

		public void TestTreatMessageErrorsAsWarningsWhenSendingMessageErrorsIsAllowed()
		{
			var oldAllowed = Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed;
			try
			{
				Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;
				var declaration = Factory.NewWithValidTestData<DummyDeclaration>();
				var validation = AMSMessageSendingValidation.New(declaration, true, null);
				var notifications = validation.CheckBusinessObjectLevelValidation();
				AssertEquals(1, notifications.Count);
				var notification = notifications[0];
				Assert(notification.IsWarning);
				AssertContains("Some error message", notification.Message);
			}
			finally
			{
				Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = oldAllowed;
			}
		}

		class DummyDeclaration : BaseJobDeclaration
		{
			public DummyDeclaration(BusinessObjectFactory factory, System.Data.DataRow row) : base(factory, row)
			{
			}

			protected override JobDeclarationValidation GetNewValidation()
			{
				return new DummyDeclarationValidation(this);
			}

			protected override void RunPreSaveValidationCore()
			{
				Validation.ValidateAll();
			}

			public override void RegisterEditableChildObject(IBusiness child)
			{
			}
		}

		sealed class DummyDeclarationValidation : BaseJobDeclarationValidation
		{
			public DummyDeclarationValidation(DummyDeclaration parent)
				: base(parent)
			{
				this.parent = parent;
			}

			public override Type AutoValidationType => typeof(DummyDeclaration);
			public override void ValidateAll()
			{
				parent.AddRowMessageError("Some error message");
			}

			readonly DummyDeclaration parent;
		}
	}
}
