using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class StandaloneCommonInvoiceProviderValidation : ZValidation
	{
		public StandaloneCommonInvoiceProviderValidation(StandaloneCommonInvoiceProvider parent)
			: base(parent)
		{
			this.parent = parent;
			this.parentListInternals = parent;
		}

		public override void ValidateAll()
		{
			using (parentListInternals.SuspendListChanged())
			{
			}
		}

		public override Type AutoValidationType => typeof(StandaloneCommonInvoiceProviderValidation);

		protected readonly StandaloneCommonInvoiceProvider parent;
		readonly ISingleElementListInternal parentListInternals;
	}
}
