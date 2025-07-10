using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using static Enterprise.ZArchitecture.GUI.ZForm;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(BaseClassificationForm))]
	sealed class BaseClassificationFormTest : ZFormBasherTest
	{
		public void TestShowPreSaveDialogs()
		{
			var cusClassification = Factory.New<BaseCusClassification>();
			using (var form = new BaseClassificationFormForTesting(cusClassification))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals("ShowPreSaveDialogs", ContinueWithSave.Yes, form.ShowPreSaveDialogs_Exposed());
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}

			using (var form = new BaseClassificationFormAuditEnabledForTesting(cusClassification))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var auditMessage = "Auditing has not yet been run on this classification. Do you want to audit now?";
				Env.Security.CusClassificationAudit.IsAllowed = false;
				AssertEquals("ShowPreSaveDialogs", ContinueWithSave.Yes, form.ShowPreSaveDialogs_Exposed());
				Assert("IsPromptAuditOnSaved", !form.isPromptAuditOnSaved);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				Env.Security.CusClassificationAudit.IsAllowed = true;
				AssertEquals("ShowPreSaveDialogs", ContinueWithSave.No, form.ShowPreSaveDialogs_Exposed());
				Assert("IsPromptAuditOnSaved", !form.isPromptAuditOnSaved);
				AssertEquals("Auditing", auditMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				AssertEquals("ShowPreSaveDialogs", ContinueWithSave.Yes, form.ShowPreSaveDialogs_Exposed());
				Assert("IsPromptAuditOnSaved", !form.isPromptAuditOnSaved);
				AssertEquals("Auditing", auditMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AssertEquals("ShowPreSaveDialogs", ContinueWithSave.Yes, form.ShowPreSaveDialogs_Exposed());
				Assert("IsPromptAuditOnSaved", form.isPromptAuditOnSaved);
				AssertEquals("Auditing", auditMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShowPreDeleteDialogs()
		{
			var cusClassification = Factory.New<BaseCusClassification>();
			using (var form = new BaseClassificationFormForTesting(cusClassification))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals("ShowPreDeleteDialogs", ContinueWithDelete.Yes, form.ShowPreDeleteDialogs_Exposed());
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				var declaration = form.BusinessEntity.Factory.New<BaseJobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CC = ((BaseCusClassification)form.BusinessEntity).PK;
				AssertEquals("ShowPreDeleteDialogs", ContinueWithDelete.No, form.ShowPreDeleteDialogs_Exposed());
				AssertEquals("This classification lookup cannot be deleted. It has been entered on a declaration with lines referenced. Instead, if the classification is no longer valid, please untick the Active box within the Import Classification Lookup screen for the same effect.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAuditMenuItem()
		{
			var cusClassification = Factory.New<BaseCusClassification>();
			using (var form = new BaseClassificationFormForTesting(cusClassification))
			{
				Tester.Test(
					(WriteToLogMenuItem)form.ActionsMenuItem_Exposed.MenuItems[form.ActionsMenuItem_Exposed.MenuItems.Count - 1],
					(BaseCusClassification)form.BusinessEntity,
					Env.Security.CusClassificationAudit,
					"Audit Classification");
			}
		}

		public void TestMenuIsNotNull()
		{
			var classification = Factory.New<BaseCusClassification>();
			using (var form = new BaseClassificationForm(classification))
			{
				AssertNotNull("Please check auto-generated codes, 'Menu=null;' and remove the line", form.Menu);
			}
		}

		public void TestAuditPluginEnabled()
		{
			var classification = Factory.New<BaseCusClassification>();
			using var form = new BaseClassificationForm(classification);
			Assert(form.PlugIns.IsPlugInAvailable(ControllerIDs.Audit));
		}

		protected override Form GetFormToBashCore() => new BaseClassificationForm(Factory.New<BaseCusClassification>());

		sealed class BaseClassificationFormAuditEnabledForTesting : BaseClassificationForm
		{
			public BaseClassificationFormAuditEnabledForTesting(BaseCusClassification businessEntity)
				: base(businessEntity)
			{ }

			internal ContinueWithSave ShowPreSaveDialogs_Exposed() => ShowPreSaveDialogs();

			protected override ZBool IsPromptAuditOnSavedEnabled => true;
		}

		sealed class BaseClassificationFormForTesting : BaseClassificationForm
		{
			public BaseClassificationFormForTesting(BaseCusClassification businessEntity)
				: base(businessEntity)
			{ }

			internal ContinueWithSave ShowPreSaveDialogs_Exposed() => ShowPreSaveDialogs();

			internal ContinueWithDelete ShowPreDeleteDialogs_Exposed() => ShowPreDeleteDialogs();

			internal MenuItem ActionsMenuItem_Exposed => ActionsMenuItem;
		}
	}
}
