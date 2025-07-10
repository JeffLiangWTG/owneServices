using System;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	public abstract class BaseInvoiceLineUserControlForVirtualPropertiesTest<T> : TestCaseWithFactory
		where T : BaseInvoiceLineUserControl, new()
	{
		[RequiresSTA]
		public void TestUniversalTariffType()
		{
			using (var control = new T())
			{
				AssertEquals("UniversalTariffType", DefaultUniversalTariffType, typeof(T).GetProperty("UniversalTariffType", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(control));
			}
		}

		protected virtual ZString DefaultUniversalTariffType => Universal.Constants.TariffTypes.HarmonizedSystem;

		[RequiresSTA]
		public void TestNewLineDetailsTabPage()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new ZForm(declaration))
			using (var control = new T())
			{
				form.Controls.Add(control);
				if (control is DeclarationInvoiceLineUserControl declarationInvoiceLineUserControl)
				{
					declarationInvoiceLineUserControl.JobDeclaration = declaration;
				}
				form.Show();

				var lineDetailTabControl = control.FindSingle<ZTabControl>(c => c.Name == "LineDetailTabControl");
				var lineDetailsTabPage = lineDetailTabControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "LineDetailsTabPage");
				var newLineDetailsTabPage = lineDetailTabControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "NewLineDetailsTabPage");

				CombineAssertions(() =>
				{
					AssertEquals("DynamicLayoutApplied", DefaultDynamicLayoutApplied, typeof(T).GetProperty("DynamicLayoutApplied", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(control));
					if (DefaultDynamicLayoutApplied)
					{
						AssertEquals($"{DefaultDynamicLayoutApplied}-NewLineDetailsTabPage", true, newLineDetailsTabPage.TabVisible);
						AssertNull($"{DefaultDynamicLayoutApplied}-LineDetailsTabPage", lineDetailsTabPage);
					}
					else
					{
						AssertNull($"{DefaultDynamicLayoutApplied}-NewLineDetailsTabPage", newLineDetailsTabPage);
						AssertEquals($"{DefaultDynamicLayoutApplied}-LineDetailsTabPage", true, lineDetailsTabPage.TabVisible);
					}
				});
			}
		}

		public void TestLineComponentsTabPage()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.SetSupportsBondedWarehousingForTesting(true);
			var invoiceHeader = declaration.Invoices.AddNew();

			var invoiceLineMock = Factory.NewMoq<BaseJobComInvoiceLine>();
			invoiceLineMock.Setup(m => m.HasOutOfInwardProcessingProcedure).Returns(true);
			var invoiceLine = invoiceLineMock.Object;
			invoiceLine.JI_JZ = invoiceHeader.PK;

			invoiceHeader.InvoiceLines.AddNew();
			using (CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var testForm = new BaseJobDeclarationForm(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var testUserControl = testForm.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				testUserControl.CustomsInvoiceLinesBoundGrid.Select(0);

				if (SupportsInwardProcessing.IsSupported())
				{
					AssertEquals("LineComponentsTabPage should show for a InvoiceLine IsInvoiceLineSupportedForProcessing.", true, testUserControl.LineComponentsTabPage.TabVisible);

					testUserControl.CustomsInvoiceLinesBoundGrid.Select(1);
					AssertEquals("LineComponentsTabPage should hide for a InvoiceLine IsInvoiceLineSupportedForProcessing false.", false, testUserControl.LineComponentsTabPage.TabVisible);
				}
				else
				{
					AssertNull(testUserControl.LineComponentsTabPage);
				}
			}
		}

		protected virtual ZBool DefaultDynamicLayoutApplied => ZBool.False;
	}
}
