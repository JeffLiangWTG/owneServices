using CargoWise.Types;

namespace Enterprise.Freight.Agency.Business
{
	internal sealed class DetentionNullStrategy : DetentionStrategy
	{
		protected override short? GetDefaultDetentionDaysCore(ContainerMovement movement)
		{
			return null;
		}
		protected override ZDateTime GetStartOfDetentionFreePeriodCore(ContainerMovement movement)
		{
			return ZDateTime.Empty;
		}
		protected override ZDateTime GetStartOfDetentionPeriodCore(ContainerMovement movement)
		{
			return ZDateTime.Empty;
		}
		protected override ZShort GetDetentionFreeDaysCore(ContainerMovement movement)
		{
			return 0;
		}
	}
}


