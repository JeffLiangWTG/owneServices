using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Messaging.Business
{
	public class WeightValidator
	{
		public void ValidateWeight(ZWeight weight, ZPropertyInfo info)
		{
			if (Math.Ceiling(weight.InKilogramsSafe) <= 0m)
			{
				info.AddMessageError(ValidationConstants.Weight.WeightMustBeGreaterThanZero);
			}
			else
			{
				if (weight.Unit == Core.Constants.Weight.Pounds)
				{
					ZDecimal weightInPoundsSafe = weight.InPoundsSafe;
					if (weightInPoundsSafe > MaximumWholeWeightAllowed)
					{
						info.AddMessageError(ValidationConstants.Weight.WeightGreaterThanMaximumPoundsAllowed(weightInPoundsSafe, MaximumWholeWeightAllowed));
					}
					if (Math.Ceiling(weightInPoundsSafe) != weightInPoundsSafe)
					{
						info.AddWarning(ValidationConstants.Weight.WeightUQInWholePounds);
					}
				}
				else
				{
					ZDecimal weightInKilograms = weight.InKilogramsSafe;
					if (weightInKilograms > MaximumWholeWeightAllowed)
					{
						info.AddMessageError(ValidationConstants.Weight.WeightGreaterThanMaximumKilogramsAllowed(weightInKilograms, MaximumWholeWeightAllowed));
					}
					if (weight.Unit == Core.Constants.Weight.Kilograms && Math.Ceiling(weightInKilograms) != weightInKilograms)
					{
						info.AddWarning(ValidationConstants.Weight.WeightUQInWholeKilograms);
					}
				}
			}
		}
		public decimal MaximumWholeWeightAllowed = 9999999999m;

		public void ValidateWeightUQ(ZWeight weight, ZPropertyInfo info)
		{
			if (weight.Amount > ZDecimal.Zero)
			{
				MandatoryValidation.MessageErrorIfNotEntered(info);
			}
			if (!weight.Unit.IsEmpty && weight.Unit != Core.Constants.Weight.Kilograms && weight.Unit != Core.Constants.Weight.Pounds)
			{
				info.AddWarning(ValidationConstants.Weight.WeightUQTypeAllowed);
			}
		}
	}
}
