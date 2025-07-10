using CargoWise.Common;
namespace Enterprise.Customs.US.AIM.Messaging.Generators
{
	public class FXXBillBlockGenerator : AIMBlockGenerator
	{
		public FXXBillBlockGenerator(IAIMMessageHeader billMessageHeader)
		{
			this.billMessageHeader = (IBillMessageHeader)Argument.NotNull(billMessageHeader, nameof(billMessageHeader));
		}
		protected override void PopulateMessageBlocks()
		{
			PopulateStandardMessageIdentifier(billMessageHeader);
			PopulateAirWayBill(billMessageHeader.AirWaybill);
			PopulateCBPEntryDetailsForCancellation();
			PopulateReasonForAmendment(billMessageHeader.ReasonForAmendment);
		}
		readonly IBillMessageHeader billMessageHeader;
	}
}
