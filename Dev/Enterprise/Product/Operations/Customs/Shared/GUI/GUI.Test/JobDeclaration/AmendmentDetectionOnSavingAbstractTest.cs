using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	internal abstract class AmendmentDetectionOnSavingAbstractTest : TestCaseWithFactory
	{
		public void TestAmendmentDetectionIsNotTriggeredForSupervisorOverrides()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			using (IDecFormOrPlugIn testObject = GetDeclarationFormOrPlugIn(declaration))
			{
				var mockController = new Mock<SendsMessagesToCustomsGUI>();
				testObject.Controller = mockController.Object;
				mockController.Verify(m => m.DetermineRequiredMessagesAndSendThem(It.IsAny<IMessageManager>()), Times.Never);
				ContinueWithSave result = testObject.ShowPreSaveDialogs();
				mockController.VerifyAll();
				AssertEquals("ContinueWithSave", ContinueWithSave.Yes, result);
			}
			declaration.JE_MergeBy = "ZZZ";
			declaration.Validation.ValidateAll();
			using (IDecFormOrPlugIn testObject = GetDeclarationFormOrPlugIn(declaration))
			{
				bool oldAllowed = Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed;
				bool oldIsController = GlbStaff.CurrentUser.GS_IsController;
				bool oldSupervisorOverridens = Env.Security.SupervisorOverrides.IsAllowed;
				bool oldAllowMessageErrors = Env.Security.AllowMessageErrors.IsAllowed;
				bool oldMergeByDefaultAllowed = Env.Security.MergeByDefault.IsAllowed;

				try
				{
					Env.Security.SupervisorOverrides.IsAllowed = false;
					Env.Security.AllowMessageErrors.IsAllowed = false;
					GlbStaff.CurrentUser.GS_IsController = false;
					Env.Security.MergeByDefault.IsAllowed = false;
					GlbStaff staff = Factory.New<GlbStaff>();
					staff.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
					staff.GS_IsActive = true;

					GlbSecurity se = Factory.New<GlbSecurity>();
					se.GU_SecurityRight = Env.Security.MergeByDefault.Code;
					se.GU_SecurityItemIsAllowed = true;
					se.GU_GS = staff.PK;
					Factory.Save();

					declaration.JE_MergeBy = "YYY";
					var mockController = new Mock<SendsMessagesToCustomsGUI>();
					testObject.Controller = mockController.Object;
					mockController.Verify(m => m.DetermineRequiredMessagesAndSendThem(It.IsAny<IMessageManager>()), Times.Never);
					ContinueWithSave result = testObject.ShowPreSaveDialogs();
					mockController.VerifyAll();
					AssertEquals("ContinueWithSave", ContinueWithSave.No, result);
				}
				finally
				{
					Env.Security.SupervisorOverrides.IsAllowed = oldSupervisorOverridens;
					Env.Security.AllowMessageErrors.IsAllowed = oldAllowMessageErrors;
					Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = oldAllowed;
					GlbStaff.CurrentUser.GS_IsController = oldIsController;
					Env.Security.MergeByDefault.IsAllowed = oldMergeByDefaultAllowed;
				}
			}
		}

		public void TestAmendmentDetectionIsNotTriggeredForNonManageableDeclaration()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals("PreCondition", false, declaration is IMessageManageableBizObj);

			using (IDecFormOrPlugIn testObject = GetDeclarationFormOrPlugIn(declaration))
			{
				var mockController = new Mock<SendsMessagesToCustomsGUI>();
				testObject.Controller = mockController.Object;

				mockController.Verify(m => m.DetermineRequiredMessagesAndSendThem(It.IsAny<IMessageManager>()), Times.Never);

				ContinueWithSave result = testObject.ShowPreSaveDialogs();

				mockController.VerifyAll();

				AssertEquals("ContinueWithSave", ContinueWithSave.Yes, result);
			}
		}

		public void TestAmendmentDetectionIsNotTriggeredWhenDetectionIsSuspended()
		{
			DeclarationForTest declaration = Factory.New<DeclarationForTest>();
			using (declaration.SuspendAmendmentDetection())
			using (IDecFormOrPlugIn testObject = GetDeclarationFormOrPlugIn(declaration))
			{
				var mockController = new Mock<SendsMessagesToCustomsGUI>();
				testObject.Controller = mockController.Object;

				mockController.Verify(m => m.DetermineRequiredMessagesAndSendThem(It.IsAny<IMessageManager>()), Times.Never);

				ContinueWithSave result = testObject.ShowPreSaveDialogs();

				mockController.VerifyAll();

				AssertEquals("ContinueWithSave", ContinueWithSave.Yes, result);
			}
		}

		[ExpectNoExceptions]
		public void TestAmendmentDetectionIsTriggeredForManageableDeclaration()
		{
			DeclarationForTest declaration = Factory.New<DeclarationForTest>();

			using (IDecFormOrPlugIn testObject = GetDeclarationFormOrPlugIn(declaration))
			{
				declaration.HasChanges = true;

				var mockManager = new Mock<IMessageManager>();
				declaration.manager = mockManager.Object;
				mockManager.Setup(m => m.GetRequiredMessagesInformation()).Returns(new RequiredMessagesInformation(declaration));

				testObject.ShowPreSaveDialogs();
				mockManager.VerifyAll();
			}
		}

		protected abstract IDecFormOrPlugIn GetDeclarationFormOrPlugIn(BaseJobDeclaration declaration);

		public interface IDecFormOrPlugIn : IShowPreSaveDialog
		{
			SendsMessagesToCustomsGUI Controller { get; set; }
		}

		internal class DeclarationFormForTest : BaseJobDeclarationForm, IDecFormOrPlugIn
		{
			public DeclarationFormForTest(BaseJobDeclaration declaration)
				: base(declaration)
			{
			}

			public SendsMessagesToCustomsGUI controller;

			protected override SendsMessagesToCustomsGUI GetNewMessagingActionsController() => controller ?? base.GetNewMessagingActionsController();

			ContinueWithSave IShowPreSaveDialog.ShowPreSaveDialogs() => ShowPreSaveDialogs();

			SendsMessagesToCustomsGUI IDecFormOrPlugIn.Controller
			{
				get => GetNewMessagingActionsController();
				set => controller = value;
			}
		}

		sealed class DeclarationForTest : BaseJobDeclaration, IBackDoorSavingSupportableBizObj
		{
			public DeclarationForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region IMessageManageableBizObj Members

			AmendmentWithdrawalReason IBackDoorSavingSupportableBizObj.GetAmendmentWithdrawalReason()
			{
				return new AmendmentWithdrawalReason();
			}

			public IMessageManager manager;
			IMessageManager IMessageManageableBizObj.GetMessageManagerForAmendmentDetection()
			{
				return manager;
			}

			ContinueWithDetection IMessageManageableBizObj.ProcessBeforeDetectingAmendmentAndContinue()
			{
				return ContinueWithDetection.Yes;
			}

			bool IMessageManageableBizObj.IsInAStatusAmendmentSendable
			{
				get { return true; }
			}

			bool IBackDoorSavingSupportableBizObj.SupportBackDoorForSavingWhenAmendmentDetected
			{
				get { return true; }
			}

			#endregion
		}
	}
}
