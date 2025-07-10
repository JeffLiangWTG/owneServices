using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class BaseJobVoyageExRateValidation : JobVoyageExRateValidation
	{
		public BaseJobVoyageExRateValidation(AutoJobVoyageExRate parent) : base(parent)
		{
		}

		#region E8_RX_NKExCurrency

		protected override void CheckE8_RX_NKExCurrency()
		{
			base.CheckE8_RX_NKExCurrency();

			MandatoryValidation.CheckEntered(Parent.E8_RX_NKExCurrencyInfo);

			if (Parent.E8_RX_NKExCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
			{
				Parent.E8_RX_NKExCurrencyInfo.AddError(Res.GetString("fe2df7c2-bf4d-4442-920b-3ea7d0fb1ce2", "You cant select your local currency here."));
			}
			else
			{
				ZQuery query = new ZQuery(JobVoyageExRateSchema.E8_JV, Parent.E8_JV);
				query.AddToFilter(JobVoyageExRateSchema.E8_GC, Parent.E8_GC);
				query.AddToFilter(JobVoyageExRateSchema.E8_RL_NKPort, Parent.E8_RL_NKPort);
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.E8_RX_NKExCurrencyInfo, Parent.Factory.Load<VoyageExRate>(query), Res.GetString("bf456cac-1465-41c1-87a7-fa2f42bddbc1", "Duplicate Currency."));
			}
		}

		#endregion

		#region E8_VoyageExchangeRate

		protected override void CheckE8_VoyageExchangeRate()
		{
			base.CheckE8_VoyageExchangeRate();

			if (Parent.E8_VoyageExchangeRate == 0)
			{
				Parent.E8_VoyageExchangeRateInfo.AddError(Res.GetString("4eb559f1-c015-4116-994a-873d4f57a9d7", "You must enter a non-zero exchange rate."));
			}
			else if (Parent.E8_VoyageExchangeRate < 0)
			{
				Parent.E8_VoyageExchangeRateInfo.AddError(Res.GetString("4d47cec1-a245-44f1-a5e7-fea95b5f8f33", "You cant have a negative exchange rate."));
			}
		}

		#endregion

		#region E8_RL_NKPort

		protected override void CheckE8_RL_NKPort()
		{
			base.CheckE8_RL_NKPort();

			if (Parent.E8_RL_NKPort == ZString.Empty)
			{
				return;
			}

			var voyage = Parent.Factory.Load<JobVoyage>(Parent.E8_JV);
			var origins = voyage.Origins.Cast<VoyageOrigin>().Select(x => x.JA_RL_NKPortOfLoading);
			var destinations = voyage.Destinations.Cast<VoyageDestination>().Select(x => x.JB_RL_NKPortOfDischarge);

			if (!origins.Concat(destinations).Contains(Parent.E8_RL_NKPort))
			{
				Parent.E8_RL_NKPortInfo.AddError(Res.GetString("a9649f21-a308-46e7-9750-938078fe6482",
				"Specified Port UNLOCO must be a Port of Loading or Discharge."));
			}
		}

		#endregion
	}
}
