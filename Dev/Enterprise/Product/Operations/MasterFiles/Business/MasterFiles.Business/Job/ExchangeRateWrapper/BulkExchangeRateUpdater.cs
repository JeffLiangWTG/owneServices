using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Summary description for BulkExchangeRateUpdater.
	/// </summary>
	public class BulkExchangeRateUpdater : NonPersistentBusinessObject, IObsoleteValidation
	{
		public BulkExchangeRateUpdater(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override void RunPreSaveValidationCore()
		{
			ExchangeRateWrappers.RunPreSaveValidation();
		}

		#region New Bound Properties

		public ZInt ExchangeRateDecimalPlaces
		{
			get { return GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces; }
		}

		public ZPropertyInfo ExchangeRateDecimalPlacesInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(ExchangeRateDecimalPlaces)); }
		}

		#endregion

		#region List Properties

		public ExchangeRateWrapperCollection ExchangeRateWrappers
		{
			get
			{
				if (fExchangeRateWrappers == null)
				{
					fExchangeRateWrappers = new ExchangeRateWrapperCollection(Factory);
					RegisterEditableChildObject(fExchangeRateWrappers);
				}
				return fExchangeRateWrappers;
			}
		}
		protected ExchangeRateWrapperCollection fExchangeRateWrappers;

		#endregion

	}
}
