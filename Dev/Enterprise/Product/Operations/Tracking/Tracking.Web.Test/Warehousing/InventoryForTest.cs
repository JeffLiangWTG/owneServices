using System;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Module;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class InventoryForTest : Inventory
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		public InventoryForTest()
		{
			SwitchHyperLink = new HyperLink();

			IsCreateNewAppInstanceIfNullForTest = true;
			SiteUser.LoginSupportForTest("TEST");

			UnauthorisedDiv = new System.Web.UI.HtmlControls.HtmlGenericControl();
			UnauthorisedLabel = new ZTextLabel();
			AuthorisedContent = new System.Web.UI.HtmlControls.HtmlGenericControl();

			SearchControlHolder = new System.Web.UI.HtmlControls.HtmlGenericControl();
			Controls.Add(SearchControlHolder);

			SetupSearchControl();
		}

		public void DetailedExportButtonClickForTesting() => DetailedExportButton_Click(this, EventArgs.Empty);

		public void CustomNewButtonClickForTesting() => SearchControl_CustomNewButtonClick(this, EventArgs.Empty);

		public void ShoppingCartControl_ClearButtonClickedForTesting() => ShoppingCartControl_ClearButtonClicked();

		public ShoppingCartUserControl ShoppingCartControlExposed => ShoppingCartControl;

		protected override ShoppingCartUserControl GetNewShoppingCartControl()
		{
			var shoppingcart = new ShoppingCartUserControl();
			shoppingcart.OrderLinesGrid = new ZDataGrid();

			return shoppingcart;
		}

		public void OnLoadForTest() => OnLoad(EventArgs.Empty);

		public void SetupBusinessObjectForValidationForTest() => SetupBusinessObjectForValidation();

		protected override BusinessObject GetNewDataSource() => new InventoryFilterBusinessObject();

		public override void Dispose() => SearchControl?.Module?.Dispose();
	}
}
