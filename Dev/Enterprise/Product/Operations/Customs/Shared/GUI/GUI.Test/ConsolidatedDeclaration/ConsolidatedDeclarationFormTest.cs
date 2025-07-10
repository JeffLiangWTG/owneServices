using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	public abstract class ConsolidatedDeclarationFormTest<T> : ZFormBasherTest where T : ConsolidatedDeclaration
	{
		public void TestAttachDeclaration_ConsolidationWithMessages()
		{
			ConsolidatedDeclaration.Messages.AddNew();
			using (var form = GetFormToBash())
			{
				form.Show();
				var declarationGrid = form.FindSingle<ZGrid>("JobDeclarationsGrid");
				var menuItem = MenuAssertion.AssertHasMenu(declarationGrid.ContextMenu, "&Attach Declaration");
				AssertEquals("Attach disabled", false, menuItem.Enabled);
			}
		}

		public void TestAttachDeclaration()
		{
			using (var form = GetFormToBash())
			{
				form.Show();
				var declarationGrid = form.FindSingle<ZGrid>("JobDeclarationsGrid");
				var menuItem = MenuAssertion.AssertHasMenu(declarationGrid.ContextMenu, "&Attach Declaration");
				ConsolidatedDeclaration.CRD_ApplicationCode = "123";
				menuItem.PerformClick();
				AssertEquals("Cannot attach before save", "You must save the current Consolidated Declaration details before attaching Declarations.", UnitTestUserNotification.Instance.LastMessage.Text);
				Factory.Save();
				menuItem.PerformClick();
				var openedForm = Application.OpenForms.OfType<NewOrAttachConsolidatedDeclarationForm>().Single();
				AssertNotNull("Attach Declaration form should be opened", openedForm);
				openedForm.Close();
			}
		}

		public void TestRemoveDeclaration()
		{
			using (var form = GetFormToBash())
			{
				form.Show();
				var declarationLead = ConsolidatedDeclaration.LeadDeclaration;
				var header = declarationLead.Invoices.AddNew();
				header.InvoiceLines.AddNew();
				var entryHeader = (CusEntryHeader)declarationLead.ActiveEntryHeaders.First();
				entryHeader.Charges.SetAmount("APC", 35m);
				entryHeader.Charges.SetAmount("DPC", 50m);
				Factory.Save();
				var declarations = ConsolidatedDeclaration.JobDeclarations;
				var declarationGrid = form.FindSingle<ZGrid>("JobDeclarationsGrid");
				var declarationCount = declarations.Count;
				foreach (var declaration in declarations)
				{
					declarationGrid.SelectSingleElement(declaration);
					var menuItem = MenuAssertion.AssertHasMenu(declarationGrid.ContextMenu, "Remove from Conso&lidation");
					declarationGrid.ContextMenu.ShowPopupMenu();

					if (declaration.ConsolidatedEntryProvider?.CanRemove ?? false)
					{
						AssertEquals("Remove is visible", true, menuItem.Visible);
						AssertNoExceptionThrown(() =>
						{
							menuItem.PerformClick();
							AssertEquals(declarationCount - 1, ConsolidatedDeclaration.JobDeclarations.Count);
						});
						return;
					}

					AssertEquals("Remove is not visible", false, menuItem.Visible);
				}
			}
		}

		protected override Form GetFormToBashCore()
		{
			var form = new ConsolidatedDeclarationForm(ConsolidatedDeclaration, new DefaultConsolidatedDeclarationFormAdaptationsProvider());
			form.ControllerID = DummyControllerIDs.Dummy;
			return form;
		}

		protected abstract T ConsolidatedDeclaration { get; }
	}
}
