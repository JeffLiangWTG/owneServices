using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business
{
	public abstract class DetentionStrategy
	{
		public static DetentionStrategy New(ZString detentionType)
		{
#if DEBUG
			if (Globals.IsTest && useDummyDetentionStrategy_ForTesting.Value)
			{
				return DummyDetentionStrategy.Instance;
			}
#endif

			switch (detentionType)
			{
				case DetentionInvoiceType.Codes.Import:
					return new DetentionImportStrategy();

				case DetentionInvoiceType.Codes.Export:
					return new DetentionExportStrategy();

				default:
					return new DetentionNullStrategy();
			}
		}

		public short? GetDefaultDetentionDays(ContainerMovement movement)
		{
			if (movement == null)
			{
				throw new ArgumentNullException(nameof(movement));
			}

			return GetDefaultDetentionDaysCore(movement);
		}

		public ZDateTime GetStartOfDetentionFreePeriod(ContainerMovement movement)
		{
			if (movement == null)
			{
				throw new ArgumentNullException(nameof(movement));
			}

			return GetStartOfDetentionFreePeriodCore(movement);
		}

		public ZDateTime GetStartOfDetentionPeriod(ContainerMovement movement)
		{
			if (movement == null)
			{
				throw new ArgumentNullException(nameof(movement));
			}

			return GetStartOfDetentionPeriodCore(movement);
		}

		public ZShort GetDetentionFreeDays(ContainerMovement movement)
		{
			if (movement == null)
			{
				throw new ArgumentNullException(nameof(movement));
			}

			return GetDetentionFreeDaysCore(movement);
		}

		protected abstract short? GetDefaultDetentionDaysCore(ContainerMovement movement);
		protected abstract ZDateTime GetStartOfDetentionFreePeriodCore(ContainerMovement movement);
		protected abstract ZDateTime GetStartOfDetentionPeriodCore(ContainerMovement movement);
		protected abstract ZShort GetDetentionFreeDaysCore(ContainerMovement movement);

		internal static Overridable<bool> useDummyDetentionStrategy_ForTesting = new Overridable<bool>();
	}
}
