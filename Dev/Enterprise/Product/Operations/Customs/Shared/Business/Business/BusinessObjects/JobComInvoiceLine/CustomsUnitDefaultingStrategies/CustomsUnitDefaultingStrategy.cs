using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public delegate bool IsConvertibleFrom(ZString unitQty, IEnumerable<ZString> list, ZString countryCode, BusinessObjectFactory factory);

	public abstract class CustomsUnitDefaultingStrategy<TJobComInvoiceLine> : ICustomsUnitDefaultingStrategy<TJobComInvoiceLine>
		where TJobComInvoiceLine : BaseJobComInvoiceLine
	{
		public abstract void Initialise(TJobComInvoiceLine invoiceLine);

		public void Initialise(BaseJobComInvoiceLine invoiceLine)
		{
			if (invoiceLine is TJobComInvoiceLine tInvoiceLine)
			{
				Initialise(tInvoiceLine);
			}
		}
		public virtual void Deinitialise(TJobComInvoiceLine invoiceLine)
		{
		}
		public void Deinitialise(BaseJobComInvoiceLine invoiceLine)
		{
			if (invoiceLine is TJobComInvoiceLine tInvoiceLine)
			{
				Deinitialise(tInvoiceLine);
			}
		}

		public abstract void DefaultUOMs(TJobComInvoiceLine invoiceLine);
		public void DefaultUOMs(BaseJobComInvoiceLine invoiceLine)
		{
			if (invoiceLine is TJobComInvoiceLine tInvoiceLine)
			{
				DefaultUOMs(tInvoiceLine);
			}
		}
	}
}
