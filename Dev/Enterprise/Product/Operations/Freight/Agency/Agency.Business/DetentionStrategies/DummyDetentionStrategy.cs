using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business
{
	sealed class DummyDetentionStrategy : DetentionStrategy
	{
		public static DummyDetentionStrategy Instance
		{
			get
			{
				if (!Globals.IsTest)
				{
					throw new InvalidOperationException("WHAT?!?! Your using the dummy movement type in production code?!?!");
				}
				else if (!DetentionStrategy.useDummyDetentionStrategy_ForTesting.Value)
				{
					throw new InvalidOperationException("You need to apply the [UseDummyMovementStrategy] attribute to your test if you want to use the DummyMovementTypeStrategy");
				}
				else if (!instance.IsOverriden)
				{
					instance.Value = new DummyDetentionStrategy();
				}

				return instance.Value;
			}
		}

		public static void ResetInstance()
		{
			instance.ResetValue();
		}

		static readonly Overridable<DummyDetentionStrategy> instance = new Overridable<DummyDetentionStrategy>();

		public DummyDetentionStrategy()
		{
		}

		protected override short? GetDefaultDetentionDaysCore(ContainerMovement movement)
		{
			if (GetDefaultDetentionDaysOverride == null)
			{
				throw new InvalidOperationException("GetDefaultDetentionDays not set yet");
			}
			else
			{
				return GetDefaultDetentionDaysOverride(movement);
			}
		}

		protected override ZDateTime GetStartOfDetentionFreePeriodCore(ContainerMovement movement)
		{
			if (GetStartOfDetentionFreePeriodOverride == null)
			{
				throw new InvalidOperationException("GetStartOfDetentionFreePeriodOverride not set yet");
			}
			else
			{
				return GetStartOfDetentionFreePeriodOverride(movement);
			}
		}

		protected override ZDateTime GetStartOfDetentionPeriodCore(ContainerMovement movement)
		{
			if (GetStartOfDetentionPeriodOverride == null)
			{
				throw new InvalidOperationException("GetStartOfDetentionPeriodOverride not set yet");
			}
			else
			{
				return GetStartOfDetentionPeriodOverride(movement);
			}
		}

		protected override ZShort GetDetentionFreeDaysCore(ContainerMovement movement)
		{
			if (GetDetentionFreeDaysOverride == null)
			{
				throw new InvalidOperationException("GetDetentionFreeDaysOverride not set yet");
			}
			else
			{
				return GetDetentionFreeDaysOverride(movement);
			}
		}

		public Converter<ContainerMovement, short?> GetDefaultDetentionDaysOverride { get; set; }

		public Converter<ContainerMovement, ZDateTime> GetStartOfDetentionFreePeriodOverride { get; set; }

		public Converter<ContainerMovement, ZDateTime> GetStartOfDetentionPeriodOverride { get; set; }

		public Converter<ContainerMovement, ZShort> GetDetentionFreeDaysOverride { get; set; }
	}
}
