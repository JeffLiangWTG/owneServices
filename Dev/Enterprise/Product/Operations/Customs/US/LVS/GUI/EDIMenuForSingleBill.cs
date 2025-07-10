using Enterprise.Customs.US.LVS.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.LVS.GUI
{
	public class EDIMenuForSingleBill : EDIMenu
	{
		public EDIMenuForSingleBill()
			: base()
		{
		}

		public CusUSLVConsignment Consignment
		{
			get
			{
				return consignment;
			}
			set
			{
				consignment = value;
				Header = consignment.Shipment;
			}
		}

		CusUSLVConsignment consignment;

		protected override CusUSLVClearanceMessageWrapper GetMessageWrapper() => new CusUSLVClearanceMessageWrapper(Header, Consignment);

		protected override ZMenuItem AddMessagingMenuItem => new ZMenuItem(ResString.GetMultilingualString("fb1e8349-a89a-43c1-97bb-1e76389096f7", "Send Original Message"), SendOriginalMessages_Click);
		protected override ZMenuItem ReplaceMessagingMenuItem => new ZMenuItem(ResString.GetMultilingualString("4784c401-89af-4212-9fc1-fa682d56498c", "Send Replacement Message"), SendReplacementMessages_Click);
		protected override ZMenuItem UpdateMessagingMenuItem => new ZMenuItem(ResString.GetMultilingualString("7f676846-b4cf-48ee-a8fb-645a244aa4dc", "Send Update Message"), SendUpdateMessages_Click);
		protected override ZMenuItem DeleteMessagingMenuItem => new ZMenuItem(ResString.GetMultilingualString("635cf971-693c-42cd-90a3-eb633dd9a72e", "Send Deletion Message"), SendDeletionMessages_Click);

		protected override void PrepareCusUSLVConsignmentsForUpdateAction(UpdateActionCode updateAction)
		{
			Consignment.InitAction(updateAction);
		}
	}
}
