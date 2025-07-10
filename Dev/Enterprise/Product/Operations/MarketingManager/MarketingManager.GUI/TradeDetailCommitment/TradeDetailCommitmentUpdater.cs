using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.MarketingManager.GUI
{
	public class TradeDetailCommitmentUpdater : NonPersistentBusinessObject
	{
		public TradeDetailCommitmentUpdater(IOrgOpportunity opportunity)
		{
			Argument.NotNull(opportunity, nameof(opportunity));
			orgOpportunity = ((OrgOpportunity)opportunity);
		}
		readonly OrgOpportunity orgOpportunity;

		public TradeDetailCommitmentItemCollection TradeDetailItems
		{
			get
			{
				if (tradeDetailItems == null)
				{
					tradeDetailItems = new TradeDetailCommitmentItemCollection();
					RegisterEditableChildObject(tradeDetailItems);

					var opportunityStatus = orgOpportunity.P8_Status;
					if (orgOpportunity is ISalesValueAssociatedEntity entity)
					{
						foreach (SalesHeader salesHeader in entity.ProspectiveSalesHeaderCollection)
						{
							salesHeader.EntitySalesCollectionProductView.Rebuild();
							foreach (EntitySalesWrapper sales in salesHeader.EntitySalesCollectionProductView)
							{
								sales.EntityTradeDetailsCollection.Reload(false);
								foreach (EntityTradeDetailWrapper tradeDetail in sales.EntityTradeDetailsCollection)
								{
									tradeDetailItems.AddNew(tradeDetail, opportunityStatus);
								}
							}
						}
					}
				}
				return tradeDetailItems;
			}
		}
		TradeDetailCommitmentItemCollection tradeDetailItems;

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			foreach (TradeDetailCommitmentItem item in TradeDetailItems)
			{
				item.RunPreSaveValidation();
			}
		}

		public void ConfirmChange()
		{
			foreach (TradeDetailCommitmentItem item in TradeDetailItems)
			{
				item.ConfirmChange();
			}
			orgOpportunity.HasChanges = true;
		}

		public ZString SupercedingWarningMessage
		{
			get
			{
				var builder = new ZStringBuilder();

				foreach (var item in TradeDetailItems.Cast<TradeDetailCommitmentItem>().Where(x => !x.IsSuperceded && x.TradeDetailStatus == OpportunityTradeStatus.Codes.Successful))
				{
					var superceder = new OrgTradeDetailValueSuperceder(item.TradeDetail, item.ProspectPeriodStart);
					var message = superceder.SupercedingWarningMessage;

					if (!message.IsEmpty)
					{
						builder.Append(message);
					}
				}
				return builder.ToString();
			}
		}
	}
}
