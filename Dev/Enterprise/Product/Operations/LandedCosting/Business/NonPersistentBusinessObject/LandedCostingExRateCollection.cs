using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.LandedCosting.Business
{
	public class LandedCostingExRateCollection : NonPersistentBusinessObjectCollection<LandedCostingExRate>
	{
		public LandedCostingExRateCollection(ILandedCostHeader lCHeaderHost) : base(lCHeaderHost.Factory)
		{
			this.LCHeaderHost = lCHeaderHost;
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public void LoadFromLCHeaderHost()
		{
			RemoveAll();
			if (LCHeaderHost.ExchangeRateHolders != null)
			{
				foreach (ILandedCostExchangeRateHolder exRateHolder in LCHeaderHost.ExchangeRateHolders)
				{
					Add(new LandedCostingExRate(exRateHolder));
				}
			}
		}

		public ZDecimal GetCostingExchangeRate(ZString xR)
		{
			ZDecimal result = 0;
			RefCurrency fRX = RefCurrency.LoadFromCurrencyCode(Factory, xR);
			if (fRX != null && this.Count > 0)
			{
				foreach (LandedCostingExRate exRate in this)
				{
					if (exRate.CurrencyCode == fRX.RX_Code)
					{
						result = exRate.ExchangeRate;
						break;
					}
				}
			}
			return result;
		}

		#region Implementation

		readonly ILandedCostHeader LCHeaderHost;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new LandedCostingExRate(Factory);
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		#endregion
	}
}
