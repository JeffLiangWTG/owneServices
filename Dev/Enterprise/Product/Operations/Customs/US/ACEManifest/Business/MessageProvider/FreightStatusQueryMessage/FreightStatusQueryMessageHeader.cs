using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class FreightStatusQueryMessageHeader : IFreightStatusQueryMessageHeader
	{
		public FreightStatusQueryMessageHeader(AsycudaBill bill, AsycudaArrivalHeader arrivalHeader)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
			this.arrivalHeader = arrivalHeader;
		}

		protected readonly AsycudaBill bill;
		readonly AsycudaArrivalHeader arrivalHeader;

		public ZString MessageType => Constants.AIMMessageSubTypes.FSQ;

		public ZString Reference => bill.ABL_BillNumber;

		public IAIMCargoControlLocation CargoControlLine => cargoControlLine ?? (cargoControlLine = new AIMCargoControlLocation(bill.Header));
		IAIMCargoControlLocation cargoControlLine;

		public IAIMAirWaybill AirWaybill => airWaybill ?? (airWaybill = new AIMAirWaybillForFSQ(bill, arrivalHeader));
		IAIMAirWaybill airWaybill;

		public IFreightStatusQuery FreightStatusQuery => freightStatusQuery ?? (freightStatusQuery = new FreightStatusQuery());
		IFreightStatusQuery freightStatusQuery;

		public void SetFreightStatusQueryRequestCode(ZString freightStatusCode)
		{
			if (!freightStatusCode.IsEmpty)
			{
				((FreightStatusQuery)FreightStatusQuery).StatusRequestCode = freightStatusCode;
			}
		}
	}
}
