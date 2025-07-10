using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AIMFDAFreightIndicator : IAIMFDAFreightIndicator
	{
		public AIMFDAFreightIndicator(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, "bill");
		}

		readonly AsycudaBill bill;

		public ZBool IsFDAFreight => bill.FDAIndicator;
	}
}
