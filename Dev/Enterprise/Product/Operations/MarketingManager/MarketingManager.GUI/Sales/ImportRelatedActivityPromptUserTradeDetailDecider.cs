using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public class ImportRelatedActivityPromptUserTradeDetailDecider : ImportRelatedActivityPromptUserDecider, IImportRelatedActivityTradeDetailDecider
	{
		public ImportRelatedActivityPromptUserTradeDetailDecider(KForm parentForm)
			: base(parentForm)
		{
		}

		public ImportRelatedActivityTradeDetailDecision GetDecision(ISalesValueAssociatedEntity entity)
		{
			var entityTradeDetailWrappers = new EntityTradeDetailWrapperCollection(entity);
			var tradeDetails =
				from SalesHeader salesHeader in entity.ActualAndProspectiveSalesHeaderCollection
				from sales in salesHeader.EntitySales
				from tradeDetail in sales.EntityTradeDetails
				where
					salesHeader.SalesProduct != null
					&& salesHeader.SalesProduct.IsAutoGenerateSpotQuoteSupported
				select tradeDetail;

			entityTradeDetailWrappers.AddRange(tradeDetails);

			if (entityTradeDetailWrappers.Count == 0)
			{
				return new ImportRelatedActivityTradeDetailDecision(false, null);
			}

			using (var form = new TradeDetailSelectionForm(entityTradeDetailWrappers))
			{
				form.AllowContinueWithoutSelection = true;

				ZFormModaliser.ShowDialogAndDispose(form);

				if (form.DialogResult == DialogResult.Cancel)
				{
					return new ImportRelatedActivityTradeDetailDecision(true, null);
				}
				else
				{
					return new ImportRelatedActivityTradeDetailDecision(false, form.SelectedTradeDetail);
				}
			}
		}
	}
}
