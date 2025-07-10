using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.GUI
{
	public class GenerateQuoteSettings : AutoGenerateQuoteSettings
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public GenerateQuoteSettings(QuoteSelectionItemCollection quoteSelectionItems, TradeDetailSelectionItemCollection tradeDetailSelectionItems)
		{
			Argument.NotNull(quoteSelectionItems, "quoteSelectionItems");
			Argument.NotNull(tradeDetailSelectionItems, "tradeDetailSelectionItems");

			this.quoteSelectionItems = quoteSelectionItems;
			this.quoteSelectionItems.SelectedItemChanged += QuoteSelectionItems_SelectedItemChanged;
			this.tradeDetailSelectionItems = tradeDetailSelectionItems;

			RefreshQuoteSelectionItemsReadOnly();
		}

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ShouldCreateNew = true;
		}

		#endregion

		#region Properties

		public override ZBool ShouldCreateAmendment
		{
			get { return base.ShouldCreateAmendment; }
			set
			{
				base.ShouldCreateAmendment = value;
				RefreshQuoteSelectionItemsReadOnly();
			}
		}

		public ZBool ShouldCreateAmendmentForBinding
		{
			get { return ShouldCreateAmendment; }
			set
			{
				using (value ? GetValidationSuspender() : null)
				{
					ShouldCreateAmendment = value;
				}
			}
		}

		public ZPropertyInfo ShouldCreateAmendmentForBindingInfo
		{
			get
			{
				return GetWrappedZPropertyInfo(Schema.ShouldCreateAmendment, (x) => ShouldCreateAmendmentInfo);
			}
		}

		#endregion

		#region QuoteSelectionItems

		public QuoteSelectionItemCollection QuoteSelectionItems
		{
			get { return quoteSelectionItems; }
		}

		void QuoteSelectionItems_SelectedItemChanged(object sender, EventArgs e)
		{
			if (ShouldCreateAmendment)
			{
				Validation.ValidateShouldCreateAmendment();
			}
		}

		readonly QuoteSelectionItemCollection quoteSelectionItems;

		void RefreshQuoteSelectionItemsReadOnly()
		{
			QuoteSelectionItems.SetReadOnlyIncludingChildren(!ShouldCreateAmendment);
		}

		public IRelatableActivity GetQuoteToAmend()
		{
			return QuoteSelectionItems.SelectedItem?.Quote;
		}

		#endregion

		#region TradeDetailSelectionItems

		public TradeDetailSelectionItemCollection TradeDetailSelectionItems
		{
			get { return tradeDetailSelectionItems; }
		}
		readonly TradeDetailSelectionItemCollection tradeDetailSelectionItems;

		public IEnumerable<OrgTradeDetail> SelectedTradeDetails
		{
			get { return tradeDetailSelectionItems.Cast<TradeDetailSelectionItem>().Where(x => x.Selected).Select(x => x.TradeDetail); }
		}

		#endregion
	}
}
