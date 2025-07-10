using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	[TestedType(typeof(OutturnAndGateInOutMessageUserControl))]
	sealed class OutturnAndGateInOutMessageUserControlTest : TestCaseWithFactory
	{
		public void TestMessageGridColumns()
		{
			var expectedCaptions = new[]
			{
				"Message No.",
				"Message Time",
				"Create Time (UTC)",
				"User",
				"Direction",
				"Type",
				"Sub Type",
				"CUSRES Status",
				"Interchange No.",
				"Interchange Time",
				"eHub Tracking ID"
			};

			var header = Factory.New<AsycudaManifestHeader>();
			using var form = new ZForm();
			using var userControl = new OutturnAndGateInOutMessageUserControl();
			form.Controls.Add(userControl);
			userControl.SetDataBinding(header, "");
			form.Show();

			var messageGrid = userControl.FindSingle<ZGrid>("zGridMessage");
			AssertContainsExactElementsInExactOrder("Caption", expectedCaptions, messageGrid.Columns.Select(c => messageGrid.GetColumnCaption(c.ColumnName)));
			AssertEquals("Visibility", expected: true, messageGrid.Columns.All(c => c.IsVisible));
		}

		public void TestMessageGridQueryInterchangeContextMenuItem()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			using var form = new ZForm();
			using var userControl = new OutturnAndGateInOutMessageUserControl();
			form.Controls.Add(userControl);
			userControl.SetDataBinding(header, "");
			form.Show();

			var messageGrid = userControl.FindSingle<ZGrid>("zGridMessage");
			var menuItem = messageGrid.ContextMenu.MenuItems.FindByText("Query Interchange On eHub");
			AssertNotNull("Menu", menuItem);
			AssertEquals("Visibility", expected: true, menuItem.Visible);
		}
	}
}
