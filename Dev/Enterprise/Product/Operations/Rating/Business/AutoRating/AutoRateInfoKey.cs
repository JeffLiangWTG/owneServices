using CargoWise.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	public class AutoRateInfoKey
	{
		public AutoRateInfoKey(AutoRateInfo info)
		{
			this.info = Argument.NotNull(info, "info");
		}

		readonly AutoRateInfo info;

		public AccChargeCode ChargeCode
		{
			get { return info.ChargeCode; }
		}

		public override bool Equals(object obj)
		{
			var infoToCheck = (AutoRateInfoKey)obj;

			return infoToCheck != null && info.CanBeMergedWith(infoToCheck.info);
		}

		public override int GetHashCode()
		{
			var chargeCode = info.ChargeCode;

			return chargeCode == null ? 0 : chargeCode.PK.GetHashCode();
		}
	}
}
