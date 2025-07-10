using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class VoyageExRate : AutoJobVoyageExRate, IExchangeRate
	{
		public VoyageExRate(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		#region GetNewLookups

		public new BaseJobVoyageExRateLookups Lookups
		{
			get { return lookups ?? (lookups = (BaseJobVoyageExRateLookups)GetNewLookups()); }
		}
		BaseJobVoyageExRateLookups lookups;

		protected override JobVoyageExRateLookups GetNewLookups()
		{
			return new BaseJobVoyageExRateLookups(this);
		}

		#endregion

		#region GetNewValidation

		protected override JobVoyageExRateValidation GetNewValidation()
		{
			return new BaseJobVoyageExRateValidation(this);
		}

		#endregion

		#region IExchangeRate Members

		ZString IExchangeRate.CurrencyCode
		{
			get
			{
				return E8_RX_NKExCurrency;
			}
		}

		ZDecimal IExchangeRate.Rate
		{
			get { return E8_VoyageExchangeRate; }
		}

#if DEBUG
		void IExchangeRate.SetBuyRate_ForTestOnly(decimal rate)
		{
			E8_VoyageExchangeRate = rate;
		}
#endif

		#endregion

		#region	UniqueIndexFailureHandler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new VoyageExRateUniqueIndexFailureHandler(this); }
		}

		class VoyageExRateUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			public VoyageExRateUniqueIndexFailureHandler(VoyageExRate voyageExRate)
			{
				Argument.NotNull(voyageExRate, nameof(voyageExRate));
				this.voyageExRate = voyageExRate;
			}

			readonly VoyageExRate voyageExRate;

			#region IUniqueIndexFailureHandler Members

			public IEnumerable<string> HandledUniqueIndexNames
			{
				get { yield return JobVoyageExRateSchema.Constants.Indexes.FK_UC__E8_JV_E8_GC_E8_RL_NKPort_E8_RX_NKExCurrency; }
			}

			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				notifier?.ReportError(GetNotificationMessage(), Res.GetString("42130e9f-ef54-4cec-af70-c977122bffff", "Duplicate Voyage Exchange Rate"));
			}

			string GetNotificationMessage()
			{
				var voyage = voyageExRate.Factory.Load<JobVoyage>(voyageExRate.E8_JV);
				var company = voyageExRate.Factory.Load<GlbCompany>(voyageExRate.E8_GC);

				return Res.GetString("582d73ac-6546-45f6-b920-eb522c1c1e04", @"While you were working, another user has created a duplicate Voyage Exchange Rate.
Voyage: {0}, currency: {1}, Port: {2}, Company: {3}.
Please cancel your changes and reload the form.",
					voyage?.JV_VoyageFlight ?? ZString.Empty,
					voyageExRate.E8_RX_NKExCurrency,
					voyageExRate.E8_RL_NKPort,
					company?.GC_Name ?? ZString.Empty);
			}

			#endregion
		}

		#endregion
	}
}
