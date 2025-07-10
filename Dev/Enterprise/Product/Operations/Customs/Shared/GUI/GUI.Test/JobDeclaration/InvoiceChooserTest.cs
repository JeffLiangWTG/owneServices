using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class InvoiceChooserTest : TestCaseWithFactory
	{
		public void TestShowModal()
		{
			using (var form = new ZTestForm())
			{
				form.Show();
				BaseJobDeclaration declaration;
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
				{
					declaration = Factory.New<BaseJobDeclaration>();
					var invoice = declaration.Invoices.AddNew();
				}

				var invoiceChooser = new InvoiceChooser(declaration.Invoices);
				invoiceChooser.ShowModal(form, (BaseJobComInvoiceHeader[] selectedBusinessObjects) => { });
				var popupForm = (EmbeddedModulePopup)typeof(ZRecordChooser<BaseJobComInvoiceHeader>).GetField("popupForm", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(invoiceChooser);
				var control = ((KUserControl)(popupForm.ActiveControl));
				AssertEquals("EnterpriseEnterprise.Customs.Business.BaseJobComInvoiceHeader", ((ZUserControl)popupForm.ActiveControl).DataSourceTypeName);
			}
		}
	}
}
