using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.AIM.Messaging;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AIMWaybill : IAIMWaybill
	{
		public AIMWaybill(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, "bill");
		}

		readonly AsycudaBill bill;

		public ZString AirportOfOrigin => bill.Origin?.RL_IATA ?? ZString.Empty;

		public ZDecimal NumberOfPieces => (ZDecimal)(bill.ABL_ManifestQty);

		public ZDecimal Weight => BillWeight;

		public ZString WeightCode => BillWeightCode;

		public ZString CargoDescription => bill.ABL_GoodsDescription;

		public ZString PermitToProceedDestinationAirport => ZString.Empty;
		public ZDate DateOfArrivalAtThePermitToProceedDestinationAirport => ZDate.Empty;

		string BillWeightCode => bill.ABL_GrossWeightUQ == WeightUnits.Codes.Pounds || bill.ABL_GrossWeightUQ == Core.Constants.Weight.Pounds
			? WeightUnits.Codes.Pounds
			: WeightUnits.Codes.Kilograms;

		ZDecimal BillWeight => bill.ABL_GrossWeightUQ == WeightUnits.Codes.Pounds || bill.ABL_GrossWeightUQ == Core.Constants.Weight.Pounds || bill.ABL_GrossWeightUQ == WeightUnits.Codes.Kilograms || bill.ABL_GrossWeightUQ == Core.Constants.Weight.Kilograms
			? bill.ABL_GrossWeight
			: WeightUnits.ConvertWeightToKilogramsIfRequired(bill.ABL_GrossWeight, bill.ABL_GrossWeightUQ);
	}
}
