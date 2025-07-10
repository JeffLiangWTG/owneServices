using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	internal sealed class NumbersControlTest : TestCaseWithFactory
	{
		public void TestDisplayDetailPanel()
		{
			var defaultValueAttribute = (DefaultValueAttribute)Attribute.GetCustomAttribute(typeof(NumbersControl).GetProperty("DisplayDetailPanel"), typeof(DefaultValueAttribute));
			AssertEquals("Requires default value or the public property will serialize incorrectly to false in the designer.", true, defaultValueAttribute.Value);

			using (var numbersControl = new NumbersControl())
			{
				AssertEquals(true, numbersControl.Controls.Find("numberDetailsPanel", true)[0].Visible);
				AssertEquals(true, numbersControl.DisplayDetailPanel);

				numbersControl.DisplayDetailPanel = false;
				AssertEquals(false, numbersControl.Controls.Find("numberDetailsPanel", true)[0].Visible);

				numbersControl.DisplayDetailPanel = true;
				AssertEquals(true, numbersControl.Controls.Find("numberDetailsPanel", true)[0].Visible);
			}
		}

		public void TestDefaultVisibleColumns()
		{
			var consol = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			var collection = consol["Numbers"];

			using (var frm = new ZForm(collection))
			using (var control = new NumbersControl())
			{
				frm.Controls.Add(control);
				frm.Show();

				Application.DoEvents();

				var grid = (ZArchitecture.ZGrid)control.Controls.Find("NumbersGrid", true)[0];

				var expectedColumns = new[] { "CE_RN_NKCountryCode", "CE_EntryType", "CE_EntryNum" };
				var actualColumns = grid.Columns.Where(c => c.IsVisible).Select(c => c.ColumnName);

				AssertContainsExactElementsInAnyOrder(expectedColumns, actualColumns);
			}
		}

		public void TestNumbersGridDockStyle()
		{
			using (var numbersControl = new NumbersControl())
			{
				var grid = (ZArchitecture.ZGrid)numbersControl.Controls.Find("NumbersGrid", true)[0];
				AssertEquals("Numbers grid should have Fill dock style to expand to maximum size", DockStyle.Fill, grid.Dock);
			}
		}
	}
}
