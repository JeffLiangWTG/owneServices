//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobTradeLaneVoyageValidation
//
//    This class should be used for overriding validation in AutoJobTradeLaneVoyageValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public class JobTradeLaneVoyageValidation : AutoJobTradeLaneVoyageValidation
	{
		public JobTradeLaneVoyageValidation(AutoJobTradeLaneVoyage parent) : base(parent)
		{
		}

		protected override void CheckNB_OH()
		{
			base.CheckNB_OH();
			ListValidation.ErrorIfInvalidPK(TradeLaneVoyage.NB_OHInfo, TradeLaneVoyage.Lookups.Headers);
			if (TradeLaneVoyage.Voyage != null && TradeLanePrincipalExist)
			{
				TradeLaneVoyage.NB_OHInfo.AddError(Res.GetString("955206fe-e2f9-4c67-bf82-4d2f4eacfb4e", "This principal already exists on this schedule."));
			}
		}

		bool TradeLanePrincipalExist
		{
			get
			{
				foreach (JobTradeLaneVoyage currentTradeLaneVoyage in TradeLaneVoyage.Voyage.TradeLanes)
				{
					if (TradeLaneVoyage != currentTradeLaneVoyage && TradeLaneVoyage.NB_OH == currentTradeLaneVoyage.NB_OH)
					{
						return true;
					}
				}
				return false;
			}
		}

		public JobTradeLaneVoyage TradeLaneVoyage
		{
			get { return (JobTradeLaneVoyage)base.Parent; }
		}
	}
}
