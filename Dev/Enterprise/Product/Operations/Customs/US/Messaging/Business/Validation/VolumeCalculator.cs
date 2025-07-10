using System;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Messaging.Business
{
	public static class VolumeCalculator
	{
		public static ZDecimal Calculate(ZVolume volume)
		{
			return Calculate(volume, new VolumeValidator());
		}

		public static ZDecimal Calculate(ZVolume volume, VolumeValidator validator)
		{
			ZDecimal result = ZDecimal.Zero;
			result = Math.Ceiling(volume.Amount);
			if (result > validator.MaximumWholeVolumeAllowed)
			{
				result = ZDecimal.Zero;
			}
			return result;
		}
	}
}
