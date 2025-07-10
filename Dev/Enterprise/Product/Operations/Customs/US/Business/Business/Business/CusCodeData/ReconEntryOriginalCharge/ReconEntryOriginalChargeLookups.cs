using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class ReconEntryOriginalChargeLookups : CusCodeDataLookups
	{
		public ReconEntryOriginalChargeLookups(ReconEntryOriginalCharge charge)
			: base(charge)
		{
		}

		public new ReconEntryOriginalCharge Parent
		{
			get { return (ReconEntryOriginalCharge)base.Parent; }
		}

		public override CodeDescriptionPairList CY_CodeList
		{
			get
			{
				ReconEntryOriginalCharge charge = Parent;
				IReconOriginalChargeParent chargeParent = charge.Parent;

				return chargeParent != null ? chargeParent.FeeAndChargeList : new CodeDescriptionPairList();
			}
		}

		public RateTypeList CY_SelectedRateTypeList
		{
			get { return Factory.GetCachedValue<RateTypeList>(); }
		}
	}
}
