using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.PlugIn.Internal;

namespace Enterprise.Customs.GUI.Testing
{
	public class OrgSupplierPartFormCustomsPluginTest : TestCaseWithFactory
	{
		public virtual void TestConstructor()
		{
			using (OrgSupplierPartFormCustomsPlugin plugin = GetNewPlugIn(Part))
			{
				AssertEquals(Part, plugin.part);
			}
		}

		public virtual void TestName()
		{
			using (OrgSupplierPartFormCustomsPlugin plugin = GetNewPlugIn(Part))
			{
				AssertEquals("Customs", plugin.Name);
			}
		}

		public virtual void TestLicenceCheckPoint()
		{
			using (OrgSupplierPartFormCustomsPlugin plugin = GetNewPlugIn(Part))
			{
				AssertEquals(Env.Licence.Broker, ((IPlugInInternals)plugin).LicenceCheckPoint);
			}
		}

		public virtual void TestGetNewUserControl()
		{
			using (OrgSupplierPartFormCustomsPlugin plugin = GetNewPlugIn(Part))
			{
				using (Control control = plugin.UserControl)
				{
					AssertEquals(typeof(OrgSupplierPartFormCustomsControl), control.GetType());
				}
			}
		}

		public virtual void TestGetBusinessEntityForPlugIn()
		{
			using (OrgSupplierPartFormCustomsPlugin plugin = GetNewPlugIn(Part))
			{
				AssertEquals(Part, plugin.BusinessEntity);
			}
		}

		public virtual void TestHasUserControl()
		{
			using (OrgSupplierPartFormCustomsPlugin plugin = GetNewPlugIn(Part))
			{
				AssertNotNull(plugin.UserControl);
			}
		}

		public void TestShowPreSaveDialogs()
		{
			var orgSupplierPart = Factory.New<OrgSupplierPart>();
			using (OrgSupplierPartFormCustomsPlugin form = new OrgSupplierPartFormCustomsPlugin(orgSupplierPart))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals("ShowPreSaveDialogs", ContinueWithSave.Yes, form.ShowPreSaveDialogsCore());
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}

			using (var form = new OrgSupplierPartFormCustomsPluginForTesting(orgSupplierPart))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var auditMessage = "Auditing has not yet been run on this product. Do you want to audit now?";
				Env.Security.CustomsSupplierPartAudit.IsAllowed = false;
				AssertEquals("ShowPreSaveDialogs", ContinueWithSave.Yes, form.ShowPreSaveDialogsCore_Exposed());
				Assert("IsPromptAuditOnSaved", !form.IsPromptAuditOnSaved_Exposed);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				Env.Security.CustomsSupplierPartAudit.IsAllowed = true;
				AssertEquals("ShowPreSaveDialogs", ContinueWithSave.No, form.ShowPreSaveDialogsCore_Exposed());
				Assert("IsPromptAuditOnSaved", !form.IsPromptAuditOnSaved_Exposed);
				AssertEquals("Auditing", auditMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				AssertEquals("ShowPreSaveDialogs", ContinueWithSave.Yes, form.ShowPreSaveDialogsCore_Exposed());
				Assert("IsPromptAuditOnSaved", !form.IsPromptAuditOnSaved_Exposed);
				AssertEquals("Auditing", auditMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AssertEquals("ShowPreSaveDialogs", ContinueWithSave.Yes, form.ShowPreSaveDialogsCore_Exposed());
				Assert("IsPromptAuditOnSaved", form.IsPromptAuditOnSaved_Exposed);
				AssertEquals("Auditing", auditMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected virtual OrgSupplierPartFormCustomsPlugin GetNewPlugIn(OrgSupplierPart part)
		{
			return new OrgSupplierPartFormCustomsPlugin(part);
		}

		protected virtual OrgSupplierPart GetNewPart()
		{
			return Factory.New<OrgSupplierPart>();
		}

		OrgSupplierPart part;
		protected OrgSupplierPart Part
		{
			get { return part ?? (part = GetNewPart()); }
			set { part = value; }
		}

		sealed class OrgSupplierPartFormCustomsPluginForTesting : OrgSupplierPartFormCustomsPlugin
		{
			public OrgSupplierPartFormCustomsPluginForTesting(OrgSupplierPart businessEntity)
				: base(businessEntity)
			{ }

			public ContinueWithSave ShowPreSaveDialogsCore_Exposed() => ShowPreSaveDialogsCore();

			protected override ZBool IsPromptAuditOnSavedEnabled => true;
		}
	}
}
