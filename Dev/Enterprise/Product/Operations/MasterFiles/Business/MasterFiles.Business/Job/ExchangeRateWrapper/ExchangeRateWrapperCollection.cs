using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Summary description for ExchangeRateWrapperCollection.
	/// </summary>
	public class ExchangeRateWrapperCollection : NonPersistentBusinessObjectCollection<ExchangeRateWrapper>
	{
		public ExchangeRateWrapperCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ExchangeRateWrapper(Factory);
		}

		public override void Remove(BusinessObject elementToRemove)
		{
			base.Remove(elementToRemove);
			ExchangeRateWrapper rateWrapper = (ExchangeRateWrapper)elementToRemove;
			rateWrapper.BuyExchangeRateBizO.Delete();
			rateWrapper.SellExchangeRateBizO.Delete();
		}
	}
}
