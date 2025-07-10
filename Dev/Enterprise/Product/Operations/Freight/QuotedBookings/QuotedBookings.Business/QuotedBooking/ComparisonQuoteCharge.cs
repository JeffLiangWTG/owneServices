using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public class ComparisonQuoteCharge : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Constructors

		public ComparisonQuoteCharge()
			: this(ZString.Empty, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero)
		{
		}

		public ComparisonQuoteCharge(ZString description, ZDecimal oSSellAmount, ZString oSSellCurrency, ZDecimal localSellAmount)
			: base()
		{
			this.Description = description;
			this.OSSellAmount = oSSellAmount;
			this.OSSellCurrency = oSSellCurrency;
			this.LocalSellAmount = localSellAmount;
		}

		#endregion

		#region Properties

		public ZDecimal LocalSellAmount { get; set; }

		public ZString OSSellCurrency { get; set; }

		public ZDecimal OSSellAmount { get; set; }

		public ZString Description { get; set; }

		#endregion
	}
}
