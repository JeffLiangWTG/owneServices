using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB.AIM;
namespace Enterprise.Customs.US.AIM.Messaging.Generators
{
	public class FSQBlockGenerator : AIMBlockGenerator
	{
		public FSQBlockGenerator(IAIMMessageHeader freightStatusQueryMessageHeader)
		{
			this.freightStatusQueryMessageHeader = (IFreightStatusQueryMessageHeader)Argument.NotNull(freightStatusQueryMessageHeader, nameof(freightStatusQueryMessageHeader));
		}
		protected override void PopulateMessageBlocks()
		{
			PopulateStandardMessageIdentifier(freightStatusQueryMessageHeader);
			PopulateCargoControlLocation(freightStatusQueryMessageHeader.CargoControlLine, freightStatusQueryMessageHeader.AirWaybill.HAWBNumber);
			PopulateAirWayBill(freightStatusQueryMessageHeader.AirWaybill);
			PopulateFreightStatusQuery(freightStatusQueryMessageHeader.FreightStatusQuery);
		}

		protected void PopulateCargoControlLocation(IAIMCargoControlLocation cargoControlLine, ZString hawbNumber)
		{
			if (hawbNumber.IsEmpty)
			{
				AddBlock(new AIMCargoControlLocation
				{
					AirportOfArrival = cargoControlLine.AirportOfArrival,
					CargoTerminalOperator = cargoControlLine.CargoTerminalOperator
				});
			}
		}

		protected new void PopulateAirWayBill(IAIMAirWaybill airWaybill)
		{
			AddBlock(new AIMAirWaybill_FSQ_FSC
			{
				AirWaybillPrefix = airWaybill.AirWaybillPrefix,
				AWBSerialNumber = airWaybill.AWBSerialNumber,
				HAWBNumber = airWaybill.HAWBNumber,
				PartArrivalReference = airWaybill.PartArrivalReference
			});
		}

		readonly IFreightStatusQueryMessageHeader freightStatusQueryMessageHeader;
	}
}
