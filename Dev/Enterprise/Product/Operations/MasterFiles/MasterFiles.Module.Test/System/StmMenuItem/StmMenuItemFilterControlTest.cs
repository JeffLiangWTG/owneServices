using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class StmMenuItemFilterControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestMenuTypeColumnHeaderTextAndCaption_WithRegistryOn()
		{
			using (DocumentsDataRegistry.Instance.EnableFormSupportforDocumentDeliveryCompletionActions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var collection = new DummyBusinessObjectCollection(Factory);
				using (ZForm testForm = new ZForm(collection))
				{
					var filterbizo = new StmMenuItemFilterBusinessObject();
					var control = new StmMenuItemFilterControl(collection, filterbizo);
					testForm.Controls.Add(control);

					var grid = (ZFilterGrid)control.Controls.Find("FilteredGrid", false)[0];
					Assert("Column SU_MenuType should be in grid", grid.ColumnStyles.Cast<ZGridColumnInfo>().Any(info => info.ColumnName == "SU_MenuType"));

					testForm.Show();

					AssertEquals("HeaderText should be 'Menu Type'", "Menu Type", grid.Columns["SU_MenuType"].ColumnStyle.HeaderText);
				}
			}
		}

		[RequiresSTA]
		public void TestMenuTypeColumnHeaderTextAndCaption_WithRegistryOff()
		{
			using (DocumentsDataRegistry.Instance.EnableFormSupportforDocumentDeliveryCompletionActions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var collection = new DummyBusinessObjectCollection(Factory);
				using (ZForm testForm = new ZForm(collection))
				{
					var filterbizo = new StmMenuItemFilterBusinessObject();
					var control = new StmMenuItemFilterControl(collection, filterbizo);
					testForm.Controls.Add(control);

					var grid = (ZFilterGrid)control.Controls.Find("FilteredGrid", false)[0];
					Assert("Column SU_MenuType shouldn't be in grid", !grid.ColumnStyles.Cast<ZGridColumnInfo>().Any(info => info.ColumnName == "SU_MenuType"));
				}
			}
		}
	}
}
