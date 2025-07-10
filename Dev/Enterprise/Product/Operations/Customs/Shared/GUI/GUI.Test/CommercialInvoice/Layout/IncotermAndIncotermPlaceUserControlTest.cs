using CargoWise.EntityFramework;
using Enterprise.Customs.GUI.CommercialInvoice;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	internal class IncotermAndIncotermPlaceUserControlTest : TestCase
	{
		public void TestIncoTermBoundDropDownEdit()
		{
			var incoTermBoundDropDownEdit = control.JZ_IncoTermBoundDropDownEdit;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true), incoTermBoundDropDownEdit.Location);
				AssertEquals("Tab", 0, incoTermBoundDropDownEdit.TabIndex);
				AssertEquals("Binding", "JZ_IncoTerm", incoTermBoundDropDownEdit.BindTo);
			});
		}

		public void TestIncoTermPlaceTextBox()
		{
			var incoTermPlaceTextBox = control.JZ_IncoTermPlaceTextBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 0, true), incoTermPlaceTextBox.Location);
				AssertEquals("Tab", 2, incoTermPlaceTextBox.TabIndex);
				AssertEquals("Binding", "JZ_IncoTermPlace", incoTermPlaceTextBox.BindTo);
			});
		}

		public void TestDataSourceTypeDelegatesToParentPanelWhenParentIsDynamicLayoutPanel()
		{
			var expectedType = typeof(string);
			using (var panel = new DynamicLayoutPanel { DataSourceType = expectedType })
			{
				panel.Controls.Add(control);
				var actualType = ((ITopLevelDataSourceType)control).DataSourceType;

				AssertEquals("TopLevelDataSourceType", expectedType, actualType);
			}
		}

		public void TestDataSourceTypeReturnsOwnTypeWhenNoDynamicLayoutPanelParent()
		{
			var expectedType = control.DataSourceType;
			var actualType = ((ITopLevelDataSourceType)control).DataSourceType;

			AssertEquals(expectedType, actualType);
		}

		IncotermAndIncotermPlaceUserControl control;

		protected override void SetUp()
		{
			base.SetUp();
			control = new IncotermAndIncotermPlaceUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
