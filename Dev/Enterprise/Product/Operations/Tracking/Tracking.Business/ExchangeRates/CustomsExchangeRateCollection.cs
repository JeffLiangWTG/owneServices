using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Tracking.Business
{
	public class CustomsExchangeRateCollection : NonPersistentBusinessObjectCollection<CustomsExchangeRate>
	{
		public CustomsExchangeRateCollection(GlbCompany company, DynamicBusinessObjectCollection rates) : base(rates.Factory)
		{
			foreach (DynamicBusinessObject rate in rates)
			{
				Add(new CustomsExchangeRate(rate, company.GC_IsReciprocal));
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("The method or operation is not supported.");
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}
	}
}
