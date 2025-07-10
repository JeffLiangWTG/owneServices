using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Messaging.Business
{
	public class VolumeValidator
	{
		public void ValidateVolume(ZVolume volume, ZPropertyInfo info)
		{
			if (volume.Amount < 0m)
			{
				info.AddMessageError(ValidationConstants.Volume.VolumeMustBeGreaterThanZero);
			}
			else
			{
				if (volume.Unit == Core.Constants.Volume.CubicFeet)
				{
					ZDecimal volumeInCubicFeetSafe = volume.ConvertTo(Core.Constants.Volume.CubicFeet);
					if (volumeInCubicFeetSafe > MaximumWholeVolumeAllowed)
					{
						info.AddMessageError(ValidationConstants.Volume.VolumeGreaterThanMaximumCubicFeetAllowed(volumeInCubicFeetSafe, MaximumWholeVolumeAllowed));
					}
					if (Math.Ceiling(volumeInCubicFeetSafe) != volumeInCubicFeetSafe)
					{
						info.AddWarning(ValidationConstants.Volume.VolumeUQInWholeCubicFeet);
					}
				}
				else
				{
					ZDecimal volumeInCubicMetres = volume.InCubicMetres;
					if (volumeInCubicMetres > MaximumWholeVolumeAllowed)
					{
						info.AddMessageError(ValidationConstants.Volume.VolumeGreaterThanMaximumCubicMetresAllowed(volumeInCubicMetres, MaximumWholeVolumeAllowed));
					}
					if (volume.Unit == Core.Constants.Volume.CubicMetres && Math.Ceiling(volumeInCubicMetres) != volumeInCubicMetres)
					{
						info.AddWarning(ValidationConstants.Volume.VolumeUQInWholeCubicMetres);
					}
				}
			}
		}
		public decimal MaximumWholeVolumeAllowed = 9999999999m;

		public void ValidateVolumeUQ(ZVolume volume, ZPropertyInfo info)
		{
			if (volume.Amount > ZDecimal.Zero)
			{
				MandatoryValidation.MessageErrorIfNotEntered(info);
			}
		}
	}
}
