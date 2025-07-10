using System;
using CargoWise.Types;
using Enterprise.ZArchitecture;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Messaging.Business
{
	public static class WeightCalculator
	{
		public static ZDecimal Calculate(ZWeight weight)
		{
			return Calculate(weight, new WeightValidator());
		}

		public static ZDecimal Calculate(ZWeight weight, WeightValidator validator)
		{
			ZDecimal result = ZDecimal.Zero;
			result = Math.Ceiling(weight.Unit == Core.Constants.Weight.Pounds ? weight.InPoundsSafe : weight.InKilogramsSafe);
			if (result > validator.MaximumWholeWeightAllowed)
			{
				result = ZDecimal.Zero;
			}
			return result;
		}

		public static ZString CalculateUQ(ZString weightUnit)
		{
			ZString result = ZString.Empty;
			if (new CodeDescriptionPairList(OLookUpEditType.Weight).ContainsCode(weightUnit))
			{
				result = weightUnit == Core.Constants.Weight.Pounds ? Core.Constants.Weight.Pounds : Core.Constants.Weight.Kilograms;
			}
			else
			{
				result = weightUnit;
			}
			return result;
		}
	}
}
