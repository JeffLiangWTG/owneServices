using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(SupervisorOverrides))]
	public class SupervisorOverridesTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSecurityCheckNotExecuteWhenNotNecessary()
		{
			var originalMerged = Env.Security.MergeByDefault.IsAllowed;
			try
			{
				Env.Security.MergeByDefault.IsAllowed = false;
				var cleanFactory = new BusinessObjectFactory();
				var declaration = cleanFactory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

				var supervisorOverrides = new SupervisorOverrides(declaration, SupervisorOverridesContext.SavingDeclaration);
				supervisorOverrides.CreateMessages();
				Assert(!declaration.IsInDatabase);
				AssertEquals("No UnAuthorised Message Logged", 0, supervisorOverrides.UnAuthorisedMessagesForLog.Count);

				cleanFactory.Save();
				declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
				supervisorOverrides = new SupervisorOverrides(declaration, SupervisorOverridesContext.SavingDeclaration);
				supervisorOverrides.CreateMessages();
				Assert(declaration.IsInDatabase);
				Assert(declaration.HasChanges);
				AssertEquals("One UnAuthorised Message is Logged", 1, supervisorOverrides.UnAuthorisedMessagesForLog.Count);
			}
			finally
			{
				Env.Security.MergeByDefault.IsAllowed = originalMerged;
			}
		}

		public void TestStaffsHaveSecurityRight()
		{
			var factory = new BusinessObjectFactory();
			var group = factory.New<GlbGroup>();
			group.GG_Code = "ABC";
			var user = factory.New<GlbStaff>();
			user.FillWithValidTestData();
			user.GS_Code = "AMA";
			user.GS_LoginName = "anton";
			user.StaffPlainTextPassword = "password";
			user.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			group.Staff.Add(user);

			var se = factory.New<GlbSecurity>();
			se.GU_SecurityRight = Env.Security.AllowMessageErrors.Code;
			se.GU_SecurityItemIsAllowed = true;
			se.GU_GS = user.PK;
			se.GU_GB = GlbBranch.CurrentBranch.PK;
			factory.Save();

			var mockForDeclaration = factory.NewMoq<BaseJobDeclaration>();
			using (mockForDeclaration.Object.GetValidationSuspender())
			{
				mockForDeclaration.Object.JE_MessageType = JobMessageTypeList.Codes.Import;
				var supervisorOverrides = new SupervisorOverrides(mockForDeclaration.Object, SupervisorOverridesContext.SavingDeclaration);

				Assert(supervisorOverrides.StaffsHaveSecurityRight(Env.Security.AllowMessageErrors.Code, new[] { user.PK }));
				se.GU_SecurityItemIsAllowed = false;
				factory.Save();

				var newFactory = new BusinessObjectFactory();
				mockForDeclaration = newFactory.NewMoq<BaseJobDeclaration>();
				using (mockForDeclaration.Object.GetValidationSuspender())
				{
					mockForDeclaration.Object.JE_MessageType = JobMessageTypeList.Codes.Import;

					supervisorOverrides = new SupervisorOverrides(mockForDeclaration.Object, SupervisorOverridesContext.SavingDeclaration);
					user = newFactory.Load<GlbStaff>(user.PK);
					Assert(!supervisorOverrides.StaffsHaveSecurityRight(Env.Security.AllowMessageErrors.Code, new[] { user.PK }));
				}
			}
		}

		public void TestMessageErrorsForIMessageNotificationsProvider()
		{
			SetSupervisorOverrides(SupervisorOverridesContext.SendingMessages);
			var transport = Factory.New<Transport>();
			transport.AddRowMessageError("A message error");
			Declaration.RegisterEditableChildObject(transport);
			AssertCollectionHasNoMessage(supervisorOverrides, "A message error");
			Assert(true);
		}

		public void TestContextIsUnknown()
		{
			SetSupervisorOverrides(SupervisorOverridesContext.Unknown);
			Assert(!supervisorOverrides.ContextIsSavingDeclaration);
			Assert(!supervisorOverrides.ContextIsSendingMessages);
			Assert(supervisorOverrides.ContextIsUnknown);
		}

		public void TestContextIsSavingDeclaration()
		{
			SetSupervisorOverrides(SupervisorOverridesContext.SavingDeclaration);
			Assert(supervisorOverrides.ContextIsSavingDeclaration);
			Assert(!supervisorOverrides.ContextIsSendingMessages);
			Assert(!supervisorOverrides.ContextIsUnknown);
		}

		public void TestContextIsSendingMessages()
		{
			SetSupervisorOverrides(SupervisorOverridesContext.SendingMessages);
			Assert(!supervisorOverrides.ContextIsSavingDeclaration);
			Assert(supervisorOverrides.ContextIsSendingMessages);
			Assert(!supervisorOverrides.ContextIsUnknown);
		}

		public void TestSupervisor()
		{
			SetSupervisorOverrides(SupervisorOverridesContext.SavingDeclaration);
			supervisorOverrides.SupervisorName = "~";
			AssertNull(supervisorOverrides.Supervisor);

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ABC";
			supervisorOverrides.SupervisorName = staff.GS_Code;

			AssertEquals(staff, supervisorOverrides.Supervisor);
		}

		public void TestHasDeclarationAsBusinessEntity()
		{
			SetSupervisorOverrides(SupervisorOverridesContext.SavingDeclaration);
			Assert("Business entity is JobDeclaration", supervisorOverrides.HasDeclarationAsBusinessEntity);
			BaseJobComInvoiceHeader invoice = Factory.New<BaseJobComInvoiceHeader>();
			supervisorOverrides = new SupervisorOverrides(invoice, SupervisorOverridesContext.SavingDeclaration);
			Assert("Business entity is not JobDeclaration", !supervisorOverrides.HasDeclarationAsBusinessEntity);
		}

		#region SetDataToDeclarationAndImporterOfRecord

		#region Implementation

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return new SupervisorOverrides(Declaration, SupervisorOverridesContext.SavingDeclaration);
		}

		#endregion

		void SetSupervisorOverrides(string context)
		{
			supervisorOverrides = new SupervisorOverrides(Declaration, context);
		}

		protected SupervisorOverrides supervisorOverrides;

		#region Declaration
		protected BaseJobDeclaration Declaration
		{
			get { return Declaration_Mock.Object; }
		}

		Mock<BaseJobDeclaration> Declaration_Mock
		{
			get
			{
				if (mockForDeclaration == null)
				{
					mockForDeclaration = Factory.NewMoq<BaseJobDeclaration>();
					mockForDeclaration.Object.SuspendValidation();
					mockForDeclaration.Object.JE_MessageType = JobMessageTypeList.Codes.Import;
				}

				return mockForDeclaration;
			}
		}
		Mock<BaseJobDeclaration> mockForDeclaration;
		#endregion

		void AssertCollectionHasNoMessage(SupervisorOverrides supervisorOverrides, string message)
		{
			foreach (MessageLog messageLog in supervisorOverrides.UnAuthorisedMessagesForLog)
			{
				if (messageLog.Message.ToUpper() == message.ToUpper())
				{
					Fail("MessagesForLog should NOT contain the following message: " + message);
				}
			}
		}

		public void TestAuthorisedMessagesForLog()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var supervisorOverrides = new SupervisorOverridesForTesting(declaration, SupervisorOverridesContext.SavingDeclaration);
			supervisorOverrides.AddMessageLogForTesting("CD2", "Test Message2", false);
			AssertEquals(0, supervisorOverrides.AuthorisedMessagesForLog.Count);
			Assert(!supervisorOverrides.ShouldLogAuthorisedChanges);
			Factory.Save();

			supervisorOverrides.AddMessageLogForTesting("CD1", "Test Message1", true);
			AssertEquals(1, supervisorOverrides.AuthorisedMessagesForLog.Count);
			AssertEquals("CD1", supervisorOverrides.AuthorisedMessagesForLog.Cast<MessageLog>().First().TargetCode);
			Assert(supervisorOverrides.ShouldLogAuthorisedChanges);
		}

		public void TestUnAuthorisedMessagesForLog()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var supervisorOverrides = new SupervisorOverridesForTesting(declaration, SupervisorOverridesContext.SavingDeclaration);
			supervisorOverrides.AddMessageLogForTesting("CD1", "Test Message1", true);
			AssertEquals(0, supervisorOverrides.UnAuthorisedMessagesForLog.Count);
			Assert(!supervisorOverrides.SupervisorShouldApproveChanges);

			supervisorOverrides.AddMessageLogForTesting("CD2", "Test Message2", false);
			AssertEquals(1, supervisorOverrides.UnAuthorisedMessagesForLog.Count);
			AssertEquals("CD2", supervisorOverrides.UnAuthorisedMessagesForLog.Cast<MessageLog>().First().TargetCode);
			Assert(supervisorOverrides.SupervisorShouldApproveChanges);
		}

		#endregion

		#endregion
	}
}
