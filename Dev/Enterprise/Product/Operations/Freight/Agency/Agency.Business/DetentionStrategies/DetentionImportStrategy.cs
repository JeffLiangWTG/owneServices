using CargoWise.Types;

namespace Enterprise.Freight.Agency.Business
{
	internal sealed class DetentionImportStrategy : DetentionStrategy
	{
		protected override short? GetDefaultDetentionDaysCore(ContainerMovement movement)
		{
			ZDateTime receivedDate = movement.E9_MovementDate;
			ZDateTime requiredDate = movement.RelatedInfo.ReturnByDate;

			if (!receivedDate.IsEmpty && !requiredDate.IsEmpty && requiredDate < receivedDate)
			{
				return (short)(receivedDate.Date - requiredDate.Date).TotalDays;
			}
			else
			{
				return 0;
			}
		}

		protected override ZDateTime GetStartOfDetentionFreePeriodCore(ContainerMovement movement)
		{
			return movement.RelatedInfo.AvailabilityDate;
		}

		protected override ZDateTime GetStartOfDetentionPeriodCore(ContainerMovement movement)
		{
			ZDateTime lastFreeDay = movement.RelatedInfo.ReturnByDate;
			return lastFreeDay.IsEmpty ? ZDateTime.Empty : lastFreeDay.AddDays(1);
		}

		protected override ZShort GetDetentionFreeDaysCore(ContainerMovement movement)
		{
			return movement.RelatedInfo.ImportDetentionFreeDays;
		}
	}
}


