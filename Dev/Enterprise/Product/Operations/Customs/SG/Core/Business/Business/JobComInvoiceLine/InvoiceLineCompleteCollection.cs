
using CargoWise.EntityFramework;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.SG.V4.Business
{
	public class InvoiceLineCompleteCollection : TypeSafeInvoiceLineCompleteCollection
	{
		public InvoiceLineCompleteCollection(JobDeclaration jobDeclaration)
			: base(jobDeclaration)
		{
		}

		#region Fetch Hints

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
		{
			return new InvoiceLineCompleteCollectionFetchStrategy(this);
		}

		class InvoiceLineCompleteCollectionFetchStrategy : Customs.Business.FetchStrategies.InvoiceLineCompleteCollectionFetchStrategy
		{
			public InvoiceLineCompleteCollectionFetchStrategy(InvoiceLineCompleteCollection collection)
				: base(collection)
			{
			}

			protected override void FetchForValidateCore()
			{
				base.FetchForValidateCore();
				var factory = Collection.Factory;
				foreach (JobComInvoiceLine invoiceLine in Collection)
				{
					if (!invoiceLine.JI_Tariff.IsEmpty)
					{
						factory.AddFetchHint(typeof(TariffView), TariffView.Loader.GetEffectiveTariffFilter(factory, Core.Constants.CountryCodes.Singapore, Constants.TariffTypes.HarmonizedSystem, invoiceLine.JI_Tariff, invoiceLine.EffectiveDateForDutyRate));
					}
				}
			}
		}

		#endregion
	}
}
