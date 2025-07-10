using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.GUI
{
	public class QuoteSelectionItem : AutoQuoteSelectionItem
	{
		public QuoteSelectionItem(IRelatableActivity quote)
		{
			this.quote = quote;
		}

		public ZString Number
		{
			get
			{
				var quoteAsBizObj = (BusinessObject)Quote;
				return !quoteAsBizObj.IsDeleted ? CodePropertyAttribute.CodeFromBusinessObject(quoteAsBizObj) : ZString.Empty;
			}
		}

		public IRelatableActivity Quote
		{
			get { return quote; }
		}
		readonly IRelatableActivity quote;
	}
}
