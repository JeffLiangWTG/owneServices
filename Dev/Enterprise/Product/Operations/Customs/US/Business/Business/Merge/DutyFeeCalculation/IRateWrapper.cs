using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public interface IRateWrapper
	{
		ZDecimal Specific { get; }
		ZDecimal Advalorem { get; }
		ZDecimal Other { get; }
	}

	static class IRateWrapperExtension
	{
		public static bool IsInvalidDutyRate(this IRateWrapper rate)
		{
			return InvalidDutyRate.IsInvalid(rate.Specific) ||
					InvalidDutyRate.IsInvalid(rate.Advalorem) ||
					InvalidDutyRate.IsInvalid(rate.Other);
		}
	}
}
