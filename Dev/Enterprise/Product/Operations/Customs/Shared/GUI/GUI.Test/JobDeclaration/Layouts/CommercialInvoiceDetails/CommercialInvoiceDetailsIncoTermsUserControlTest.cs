using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class CommercialInvoiceDetailsIncoTermsUserControlTest : TestCaseWithFactory
	{
		public void TestIncoTermBoundDropEdit()
		{
			var incoTermBoundDropEdit = control.IncoTermBoundDropEdit;

			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>(incoTermBoundDropEdit);

				AssertNull("Caption", incoTermBoundDropEdit.CaptionResourceString.Caption);
				AssertEquals("Caption Visible", false, new LabelCaptionRenderProvider().GetLabelCaptionVisible(incoTermBoundDropEdit));
			});
		}

		public void TestIncoTermExplainButton()
		{
			AssertType<ZButton>(control.IncoTermExplainButton);
		}

		public void TestShowIncoTermDescriptionForm()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			using (var form = new ZForm())
			using (var controlForTest = new CommercialInvoiceDetailsIncoTermsUserControl())
			{
				controlForTest.SetDataBinding(invoice, "");
				form.Controls.Add(controlForTest);
				form.Show();

				controlForTest.IncoTermExplainButton.PerformClick();
				AssertType<IncoTermDescriptionForm>(ZFormModaliser.ActiveForm);
			}
		}

		public void TestIExtendedControl()
		{
			AssertEquals("Control implements IExtendedControl", true, control is IExtendedControl);

			IExtendedControl extendedControl = control;
			CombineAssertions(() =>
			{
				AssertSame("Host", control, extendedControl.Host);
				AssertType<DefaultControlExtensionCollection>("Extensions", extendedControl.Extensions);
			});
		}

		public void TestIResourceStringBindingMember()
		{
			AssertEquals("Control implements IResourceStringBindingMember", true, control is IResourceStringBindingMember);

			IResourceStringBindingMember resourceStringBindingMember = control;
			AssertEquals("ResourceStringBindingMember", "JZ_IncoTerm", resourceStringBindingMember.ResourceStringBindingMember);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new CommercialInvoiceDetailsIncoTermsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		CommercialInvoiceDetailsIncoTermsUserControl control;
	}
}
