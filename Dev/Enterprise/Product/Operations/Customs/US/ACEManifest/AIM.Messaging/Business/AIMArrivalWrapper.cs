using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB.AIM;

namespace Enterprise.Customs.US.AIM.Messaging
{
	public class AIMArrivalWrapper
	{
		public AIMArrivalWrapper(AIMArrival arrival)
		{
			FlightNumber = arrival.FlightNumber.Trim();
			PartArrivalReference = arrival.PartArrivalReference.Trim();
			EstimateScheduledArrivalDate = arrival.ScheduledArrivalDate;
			if (arrival.BoardedQuantityIdentifier == "B")
			{
				BoardedPieces = arrival.BoardedPieceCount;
			}
		}

		public AIMArrivalWrapper(AIMText_Continuation_FSC_SplitBillArrival splitBillArrival)
		{
			FlightNumber = splitBillArrival.FlightNumber.Trim();
			PartArrivalReference = splitBillArrival.PartArrivalReference.Trim();
			EstimateScheduledArrivalDate = splitBillArrival.ScheduledArrivalDate;
			if (splitBillArrival.BoardedQuantityIdentifier == "B")
			{
				BoardedPieces = splitBillArrival.BoardedPieceCount;
			}
		}

		public AIMArrivalWrapper(AIMErrorReportFlight errorReportFlight)
		{
			FlightNumber = errorReportFlight.FlightNumber.Trim();
			PartArrivalReference = ZString.Empty;
			EstimateScheduledArrivalDate = errorReportFlight.Date;
		}

		public ZString FlightNumber { get; private set; }

		public ZString PartArrivalReference { get; private set; }

		public ZDecimal BoardedPieces { get; private set; }

		public ZDate EstimateScheduledArrivalDate
		{
			get => scheduledArrivalDate;
			private set
			{
				scheduledArrivalDate = value;

				if (scheduledArrivalDate.IsValid)
				{
					// correct for today being on cusp of year
					if (scheduledArrivalDate.AddMonths(3) < ZDate.Today)
					{
						scheduledArrivalDate = scheduledArrivalDate.AddYears(1);
					}
					else if (scheduledArrivalDate > ZDate.Today.AddMonths(6))
					{
						scheduledArrivalDate = scheduledArrivalDate.AddYears(-1);
					}
				}
			}
		}
		ZDate scheduledArrivalDate;
	}
}
