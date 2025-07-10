using CargoWise.Common;
namespace Enterprise.Customs.US.AIM.Messaging.Generators
{
	public class FRXBillBlockGenerator : AIMBlockGenerator
	{
		public FRXBillBlockGenerator(IAIMMessageHeader billMessageHeader)
		{
			this.billMessageHeader = (IBillMessageHeader)Argument.NotNull(billMessageHeader, nameof(billMessageHeader));
		}
		protected override void PopulateMessageBlocks()
		{
			PopulateStandardMessageIdentifier(billMessageHeader);
			PopulateAirWayBill(billMessageHeader.AirWaybill);
			PopulateReasonForAmendment(billMessageHeader.ReasonForAmendment);
		}
		readonly IBillMessageHeader billMessageHeader;
	}
}
