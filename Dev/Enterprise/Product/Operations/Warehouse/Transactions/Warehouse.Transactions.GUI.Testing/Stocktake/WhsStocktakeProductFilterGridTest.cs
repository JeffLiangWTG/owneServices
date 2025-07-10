using System;
using CargoWise.ComponentModel;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.GUI.Stocktake;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public class WhsStocktakeProductFilterGridTest : WhsGuiTestCaseWithFactory
	{
		#region TestEditOnModuleButtonGrid

		public void TestAttachShowsMessageWhenClientIsEmpty()
		{
			var stockTake = Factory.New<WhsStocktake>();
			using (var form = new TestForm(stockTake))
			{
				form.Show();
				form.UserControl.AttachButton_ClickForTest(null, EventArgs.Empty);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("Set the client before attaching any products."));
			}
		}

		#endregion

		#region Implementation

		class TestForm : ZForm, INotifications
		{
			public TestForm(WhsStocktake stocktake)
				: base(stocktake)
			{
				Stocktake = stocktake;
			}

			public WhsStocktakeProductFilterGrid UserControl;

			protected override void InitializeComponent()
			{
				this.UserControl = new WhsStocktakeProductFilterGrid();
				this.BindingSource.SetBindingMember(UserControl, "ProductFilterCollection");
				this.UserControl.BindToFindBoxList = "Lookups.SupplierParts";
				this.Controls.Add(UserControl);

				var column = new ZTextBoxColumnStyleInfo();
				column.Caption = "ProductCode";
				column.ColumnName = "ProductCode";
				this.UserControl.InnerGrid.ColumnStyles.Add(column);

				this.DataSourceAssemblyName = "Enterprise.Warehouse.Transactions.Business";
				this.DataSourceTypeName = "Enterprise.Warehouse.Transactions.Business.WhsStocktake";
			}

			#region INotifications Members

			void INotifications.Add(INotification notification)
			{
				NotificationsHelper.Add(notification);
			}

			WhsNotificationsHelper NotificationsHelper
			{
				get { return notificationsHelper ?? (notificationsHelper = new WhsNotificationsHelper(Stocktake)); }
			}

			WhsNotificationsHelper notificationsHelper;
			readonly WhsStocktake Stocktake;

			#endregion
		}

		#endregion
	}
}
