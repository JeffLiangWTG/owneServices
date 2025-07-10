using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class ShipmentDetailsIncoTermsUserControlTest : TestCaseWithFactory
	{
		public void TestCaptionRenderingEnabled()
		{
			AssertEquals("The layout needs the caption resource strings", true, control.CaptionRenderingEnabled);
		}

		public void TestIncoTermDropEdit()
		{
			var incoTermDropEdit = control.IncoTermDropEdit;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true), incoTermDropEdit.Location);
				AssertEquals("Before the Explain button", 0, incoTermDropEdit.TabIndex);
			});
		}

		public void TestIncoTermExplainButton()
		{
			var incoTermExplainButton = control.IncoTermExplainButton;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(293, 0, true), incoTermExplainButton.Location);
				AssertEquals("After the Inco Term", 1, incoTermExplainButton.TabIndex);
			});
		}

		public void TestShowIncoTermDescriptionForm()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			using (var form = new ZForm())
			using (var controlForTest = new ShipmentDetailsIncoTermsUserControl())
			{
				controlForTest.SetDataBinding(declaration, "");
				form.Controls.Add(controlForTest);
				form.Show();
				controlForTest.IncoTermExplainButton.PerformClick();
				AssertType<IncoTermDescriptionForm>(ZFormModaliser.ActiveForm);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new ShipmentDetailsIncoTermsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		ShipmentDetailsIncoTermsUserControl control;
	}
}
