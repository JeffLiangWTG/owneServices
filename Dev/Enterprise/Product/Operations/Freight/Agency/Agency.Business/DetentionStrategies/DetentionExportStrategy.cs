using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	internal sealed class DetentionExportStrategy : DetentionStrategy
	{
		protected override short? GetDefaultDetentionDaysCore(ContainerMovement movement)
		{
			ZDateTime fromDate = GetReleaseDate(movement);
			ZDateTime toDate = movement.E9_MovementDate;

			if (fromDate.IsEmpty || toDate.IsEmpty)
			{
				return 0;
			}

			int days = (int)(toDate.Date - fromDate.Date).TotalDays + 1;
			int freeDays = movement.RelatedInfo.ExportDetentionFreeDays;

			return (short)Math.Max(days - freeDays, 0);
		}

		protected override ZDateTime GetStartOfDetentionFreePeriodCore(ContainerMovement movement)
		{
			return GetReleaseDate(movement);
		}

		protected override ZDateTime GetStartOfDetentionPeriodCore(ContainerMovement movement)
		{
			ZDateTime released = GetReleaseDate(movement);

			return released.IsEmpty ? released : released.AddDays(movement.RelatedInfo.ExportDetentionFreeDays);
		}

		protected override ZShort GetDetentionFreeDaysCore(ContainerMovement movement)
		{
			return movement.RelatedInfo.ExportDetentionFreeDays;
		}

		static ZDateTime GetReleaseDate(ContainerMovement movement)
		{
			var result = ZDateTime.Empty;

			if (movement.E9_MovementDate.IsValidSmallDateTime)
			{
				var filter = new ZQuery(JobContainerMoveSchema.E9_R6, movement.E9_R6);
				filter.AddToFilter(JobContainerMoveSchema.E9_MovementType, ContainerMovementTypes.GetMovementsCodesThatStartExportDetention());
				filter.AddToFilter(JobContainerMoveSchema.E9_MovementDate, SQLComparisonOperator.LessThan, movement.E9_MovementDate);
				filter.OrderBy = JobContainerMoveSchema.Constants.E9_MovementDate + " desc";

				var releaseMovement = movement.Factory.LoadTop1<ContainerMovement>(filter);
				if (releaseMovement != null)
				{
					var findDateOutOfOrderQuery = new ZQuery(JobContainerMoveSchema.E9_R6, movement.E9_R6);
					findDateOutOfOrderQuery.AddToFilter(JobContainerMoveSchema.PK, SQLComparisonOperator.NotEqual, movement.PK);
					findDateOutOfOrderQuery.AddToFilter(JobContainerMoveSchema.E9_MovementType, ContainerMovementTypes.MovementsCodesThatRestrictDetentionDaysDefaulting);
					findDateOutOfOrderQuery.AddToFilter(JobContainerMoveSchema.E9_MovementDate, SQLComparisonOperator.GreaterThan, releaseMovement.E9_MovementDate);
					findDateOutOfOrderQuery.AddToFilter(JobContainerMoveSchema.E9_MovementDate, SQLComparisonOperator.LessThanOrEqualTo, movement.E9_MovementDate);

					var dateOutOfOrder = movement.Factory.LoadTop1<ContainerMovement>(findDateOutOfOrderQuery);
					if (dateOutOfOrder == null)
					{
						result = releaseMovement.E9_MovementDate;
					}
				}
			}

			return result;
		}
	}
}


