using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public partial class JobComInvoiceGroupHeader : AutoJobComInvoiceGroupHeader
	{
		public JobComInvoiceGroupHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override string GetStandaloneIncoTermAndChargeFactoryCountryContext()
		{
			return base.GetStandaloneIncoTermAndChargeFactoryCountryContext() + this.GetCustomsChargeTypeListCacheKey();
		}

		protected override ExchangeRateType RateTypeCore
		{
			get
			{
				if (Master is JobDeclaration declaration && declaration.IsExport)
				{
					return ExchangeRateType.CustomsSecondary;
				}
				else
				{
					return base.RateTypeCore;
				}
			}
		}
	}
}
