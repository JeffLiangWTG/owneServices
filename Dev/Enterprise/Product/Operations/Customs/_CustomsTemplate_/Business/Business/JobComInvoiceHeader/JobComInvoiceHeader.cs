using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs._CustomsTemplate_.Business
{
	public partial class JobComInvoiceHeader : Customs.Business.BaseJobComInvoiceHeader
	{
		public JobComInvoiceHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override CargoWise.Types.ZString LocalCurrencyCodeCore
		{
			get
			{
				if (!GetType().FullName.Contains("_CustomsTemplate_"))
				{
					throw new NotImplementedException();
				}
				return base.LocalCurrencyCodeCore;
			}
		}
	}
}
