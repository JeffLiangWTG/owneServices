using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class AsycudaTaxCollection : ASYCUDA.Business.AsycudaTaxCollection<AsycudaTax, AsycudaBill>
	{
		public AsycudaTaxCollection(AsycudaBill master) : base(master)
		{
		}

		public AsycudaTax AddNew(ZString type)
		{
			var result = base.AddNew();
			result.AET_ChargeType = type;
			return result;
		}

		public AsycudaTax FindByType(ZString chargeType) => Find(charge => charge.AET_ChargeType == chargeType).FirstOrDefault();

		protected override bool AllowNewCore => false;

		public bool IsExistOverrideTax(ZString taxCode)
		{
			return this.ToList<AsycudaTax>().Any(x => x.AET_ChargeType == taxCode && x.AET_RateOverrideReasonCode == RateOverrideReasonCodeList.Codes.Override);
		}
	}
}
