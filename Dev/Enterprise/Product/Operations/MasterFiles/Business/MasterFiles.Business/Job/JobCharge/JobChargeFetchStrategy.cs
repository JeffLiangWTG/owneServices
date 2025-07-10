using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class JobChargeFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public JobChargeFetchStrategy(JobCharge charge) : base(charge)
		{
		}

		JobCharge Charge
		{
			get { return BusinessObject as JobCharge; }
		}

		protected override void FetchForLoadCore()
		{
			var charge = Charge;
			var pk = charge.PK;
			var sellAccount = charge.JR_OH_SellAccount;
			var costAccount = charge.JR_OH_CostAccount;

			Factory.AddFetchHint(typeof(AccTransactionLines), charge.JR_AL_ARLine);
			Factory.AddFetchHint(typeof(AccTransactionLines), charge.JR_AL_APLine);

			Factory.AddFetchHint(typeof(OrgHeader), sellAccount);
			Factory.AddFetchHint(OrgMiscServSchema.OM_OH, sellAccount);
			Factory.AddFetchHint(OrgCompanyDataSchema.OB_OH, sellAccount);

			Factory.AddFetchHint(typeof(OrgHeader), costAccount);
			Factory.AddFetchHint(OrgMiscServSchema.OM_OH, costAccount);
			Factory.AddFetchHint(OrgCompanyDataSchema.OB_OH, costAccount);

			Factory.AddFetchHint(JobExRateSchema.JF_JH, charge.JR_JH);
			Factory.AddFetchHint(AccChargeCodeSchema.PK, charge.JR_AC);

			Factory.AddFetchHint(JobChargeAttribSchema.EC_JR, pk);
			Factory.AddFetchHint(JobChargeTargetSchema.JRT_JR, pk);

			base.FetchForLoadCore();
		}
	}
}
