using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.DataTransfer.MessageProcessing
{
	[CargoWise.Common.Testing.SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	internal static class CMMRelatedMovementHelper
	{
		public static JobVoyage GuessVoyageFromContainerHistory(RefContainerStock stock, ZDateTime reference)
		{
			Argument.NotNull(stock, "stock");

			if (reference.IsEmpty)
			{
				return null;
			}

			string[] blockingMovementTypes = new string[]
			{
				ContainerMovementTypes.Codes.Load,
				ContainerMovementTypes.Codes.OffHire,
				ContainerMovementTypes.Codes.OnHire,
				ContainerMovementTypes.Codes.RePositionIntoYard,
				ContainerMovementTypes.Codes.ReShipRequested,
				ContainerMovementTypes.Codes.ReturnedUnshipped,
				ContainerMovementTypes.Codes.YardGateIn,
			};

			ZQuery subFilter = new ZQuery();
			subFilter.DefaultJoinCondition = JoinCondition.Or;
			subFilter.AddToFilter(JobContainerMoveSchema.E9_MovementType, blockingMovementTypes);
			subFilter.AddToFilter(JobContainerMoveSchema.E9_JV, SQLComparisonOperator.NotEqual, ZGuid.Empty);

			ZQuery filter = new ZQuery();
			filter.AddToFilter(JobContainerMoveSchema.E9_R6, stock.PK);
			filter.AddToFilter(subFilter);
			filter.AddToFilter(JobContainerMoveSchema.E9_MovementDate, SQLComparisonOperator.NotEqual, ZDateTime.Empty);
			filter.AddToFilter(JobContainerMoveSchema.E9_MovementDate, SQLComparisonOperator.LessThan, reference);
			filter.AddToFilter(JobContainerMoveSchema.E9_MovementDate, SQLComparisonOperator.GreaterThan, reference.AddMonths(-3));
			filter.OrderBy = JobContainerMoveSchema.Constants.E9_MovementDate + OrderByClause.Descending;

			var prevMovement = stock.Factory.LoadTop1<ContainerMovement>(filter);

			if (prevMovement == null || Array.IndexOf<string>(blockingMovementTypes, prevMovement.E9_MovementType) >= 0)
			{
				return null;
			}
			else
			{
				return prevMovement.Voyage;
			}
		}
	}
}


