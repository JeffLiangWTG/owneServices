using System;

namespace Enterprise.Rating.Integration
{
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class RateTypeAttribute : Attribute
	{
		public RateTypeAttribute(RateType rateType)
		{
			this.RateType = rateType;
		}

		public readonly RateType RateType;
	}

	[Flags]
	public enum RateCategoryGroup
	{
		Freight = 1,
		Origin = 2,
		Destination = 4,
		OtherSupplementary = 8,
		NonFreight = Origin | Destination | OtherSupplementary,
		All = Freight | NonFreight,
	}

	[AttributeUsage(AttributeTargets.Field)]
	public sealed class IsFreightAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Field)]
	public sealed class IsOriginAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Field)]
	public sealed class IsDestinationAttribute : Attribute
	{
	}
}
