using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public abstract class TradeLaneAndDetailsMenuItemsBuilder
	{
		protected TradeLaneAndDetailsMenuItemsBuilder(OrgSalesProduct salesProduct, ZGrid grid)
		{
			Argument.NotNull(salesProduct, "salesProduct");
			Argument.NotNull(grid, "grid");

			this.salesProduct = salesProduct;
			this.grid = grid;
		}

		protected readonly OrgSalesProduct salesProduct;
		protected readonly ZGrid grid;

		#region Menu Items

		public void AddMenuItems()
		{
			var form = grid.FindForm() as ZForm;
			var formEntity = form?.BusinessEntity;

			grid.ContextMenu.MenuItems.Add(new ZMenuItem("-"));

			if (salesProduct.IsAutoGenerateQuoteSupported)
			{
				AddCreateQuoteItem(grid.ContextMenu, formEntity as ISalesValueAssociatedEntity);
			}

			if (salesProduct.MP_Code == SystemDefinedSalesProductList.Codes.ForwardingShipment)
			{
				AddCreateSpotQuoteItem(grid.ContextMenu, formEntity as ISalesRelationActivity);
			}
		}

		#region Create Quote Menu Item

		void AddCreateQuoteItem(ContextMenu contextMenu, ISalesValueAssociatedEntity parentEntity)
		{
			var createQuoteMenuItem = new ZMenuItem(
				ResString.GetMultilingualString("MasterFiles.Organisation.CreateQuote", "Create Quote"),
				(sender, e) =>
					{
						var tradeDetails = GetSelectedTradeDetailsForQuote().ToList();
						if (tradeDetails.Any())
						{
							var generateQuoteController = GenerateQuoteForSalesValueAssociatedEntityController.New();
							generateQuoteController.ParentModalForm = (ZForm)grid.FindForm();
							generateQuoteController.Execute(parentEntity, tradeDetails);
						}
						else
						{
							Globals.Message.ShowInformation(Res.GetString("fee0d8ed-5e53-4cec-af42-46d1cf9758c7", "Please select the trade lane details you wish to create a quote for."));
						}
					});

			contextMenu.MenuItems.Add(createQuoteMenuItem);
		}

		protected abstract IEnumerable<OrgTradeDetail> GetSelectedTradeDetailsForQuote();

		#endregion

		#region Create Spot Quote Menu Item

		void AddCreateSpotQuoteItem(ContextMenu contextMenu, ISalesRelationActivity parentActivity)
		{
			var createSpotQuoteMenuItem = new ZMenuItem(
				ResString.GetMultilingualString("MasterFiles.Organisation.CreateOneOffQuote", "Create One Off Quote"),
				(sender, e) =>
				{
					var tradeDetail = GetSelectedTradeDetailForSpotQuote();
					if (tradeDetail != null)
					{
						var generateSpotQuoteController = GenerateSpotQuoteFromTradeDetailController.New();
						generateSpotQuoteController.ShowNewForm(tradeDetail, parentActivity);
					}
				});

			contextMenu.MenuItems.Add(createSpotQuoteMenuItem);
		}

		protected abstract OrgTradeDetail GetSelectedTradeDetailForSpotQuote();

		#endregion

		#endregion
	}
}
