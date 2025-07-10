using System;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class AccountingEmailDefTest : TestCaseWithFactory
	{
		protected abstract Type EmailDefType { get; }

		protected string GetBody(AccountingEmailDef email)
		{
			MethodInfo info = EmailDefType.GetMethod("GetBody", BindingFlags.NonPublic | BindingFlags.Instance);
			return info.Invoke(email, null) as string;
		}

		protected string GetSubject(AccountingEmailDef email)
		{
			MethodInfo info = EmailDefType.GetMethod("GetSubject", BindingFlags.NonPublic | BindingFlags.Instance);
			return info.Invoke(email, null) as string;
		}

		public void TestDoesNotCacheBusinessObjects()
		{
			var fields = EmailDefType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
			if (fields.Length == 0)
			{
				Assert(true);
			}
			else
			{
				foreach (var field in fields)
				{
					Assert(string.Format("Field '{0}' can not be a busines object", field.Name), !typeof(IBusiness).IsAssignableFrom(field.FieldType));
				}
			}
		}

		public virtual void TestSend()
		{
			GlbStaff staffPluto = Factory.New<GlbStaff>();
			staffPluto.GS_LoginName = @"test";
			staffPluto.GS_Code = "TE";
			staffPluto.GS_EmailAddress = "test@cargowiseone";
			staffPluto.Groups.Add(Factory.Load<GlbGroup>(Core.Constants.Groups.AllPK));
			Factory.Save();
			AccountingMasterFilesRegistry.Instance.DebtorGlobalCreditLimitNotifyGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.Groups.AllPK);

			var accountingEmailDef = new AccountingEmailDef_ForTest();
			accountingEmailDef.Send();
			AssertEquals("Should have a email", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var mailQueryText = "SELECT * FROM dbo.MailDBItems";
			var collection = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			collection.Load(mailQueryText);
			AssertEquals("Create and save", 1, collection.Count);
		}

		public virtual void TestCreate()
		{
			ReleaseFactory();
			GlbStaff staffPluto = Factory.New<GlbStaff>();
			staffPluto.GS_LoginName = @"test";
			staffPluto.GS_Code = "TE";
			staffPluto.GS_EmailAddress = "test@cargowiseone";
			staffPluto.Groups.Add(Factory.Load<GlbGroup>(Core.Constants.Groups.AllPK));
			Factory.Save();
			AccountingMasterFilesRegistry.Instance.DebtorGlobalCreditLimitNotifyGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.Groups.AllPK);

			var accountingEmailDef = new AccountingEmailDef_ForTest();
			accountingEmailDef.Create(Factory);
			AssertEquals("Should have a email", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var mailQueryText = "SELECT * FROM dbo.MailDBItems";
			var collection = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			collection.Load(mailQueryText);
			AssertEquals("Create but not save", 0, collection.Count);
		}

		public virtual void TestRender()
		{
			var email = new AccountingEmailDef_ForTest()
			{
				TheSubject = "This is the email Subject",
				TheBody = "I am an email body!",
			};
			AssertEquals("Until you Render() the email, the Subject is empty", string.Empty, email.Subject);
			AssertEquals("Until you Render() the email, the Body is empty", string.Empty, email.Body);

			var returnedEmail = email.Render();
			AssertEquals("When you Render() the email, the Subject is set", "This is the email Subject", email.Subject);
			AssertEquals("When you Render() the email, the Body is set", "I am an email body!", email.Body);
			AssertSame("Render() returns the same object, so that you can create and render the email in one line.", email, returnedEmail);

			email.TheSubject = "A different subject";
			email.TheBody = "The body changed because reasons";

			email.Render();
			AssertEquals("When you Render() the email again, the Subject is set again", "A different subject", email.Subject);
			AssertEquals("When you Render() the email again, the Body is set again", "The body changed because reasons", email.Body);

			AssertEquals("Render() does not send the email", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public virtual void TestRenderAndSend()
		{
			GlbStaff staffPluto = Factory.New<GlbStaff>();
			staffPluto.GS_LoginName = @"test";
			staffPluto.GS_Code = "TE";
			staffPluto.GS_EmailAddress = "test@cargowiseone";
			staffPluto.Groups.Add(Factory.Load<GlbGroup>(Core.Constants.Groups.AllPK));
			Factory.Save();
			AccountingMasterFilesRegistry.Instance.DebtorGlobalCreditLimitNotifyGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.Groups.AllPK);

			var accountingEmailDef = new AccountingEmailDef_ForTest()
			{
				TheSubject = "This is the email Subject",
				TheBody = "I am an email body!",
			};
			accountingEmailDef.Render();
			AssertEquals("Precondition: When you Render() the email, the Subject is set", "This is the email Subject", accountingEmailDef.Subject);
			AssertEquals("Precondition: When you Render() the email, the Body is set", "I am an email body!", accountingEmailDef.Body);

			accountingEmailDef.TheSubject = "Changed Subject";
			accountingEmailDef.TheBody = "Changed Body";

			accountingEmailDef.Send();
			AssertEquals("Should have a email", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			AssertEquals("Sending the email after Render() will not re-render the subject", "This is the email Subject", accountingEmailDef.Subject);
			AssertEquals("Sending the email after Render() will not re-render the body", "I am an email body!", accountingEmailDef.Body);

			var mailQueryText = "SELECT * FROM dbo.MailDBItems";
			var collection = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			collection.Load(mailQueryText);
			AssertEquals("Create and save", 1, collection.Count);
			var mailItem = collection[0];
			AssertEquals("Email subject should be saved to database", "This is the email Subject", mailItem["MI_Subject"]);
			AssertEquals("Email body should be saved to database", "I am an email body!", mailItem["MI_Body"]);
		}
	}
}
