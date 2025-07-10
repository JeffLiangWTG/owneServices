using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class ShipmentReferences : AdditionalReferencesParent
	{
		public ZString HBOLNumber { get; set; }
		public ZString HAWBNumber { get; set; }
		public ZString ShippersReference { get; set; }
		public ZString ShipmentID { get; set; }
		public List<ZString> OrderNumbers { get; set; }
		public ZString InterimReceipt { get; set; }
		public ZString OriginUNLOCO { get; set; }
		public ZString DestinationUNLOCO { get; set; }
		public ZString CFSReference { get; set; }

		public ZBool IsVGM { get; set; }
		public ZString MBOLNumber { get; set; }
		public ZString WayBillTypeCode { get; set; }
		public ZString SCAC { get; set; }

		public ZBool IsNVOCC { get; set; }
		public ZString AgentsReference { get; set; }
		public ZString CoLoadBookingConfirmationReference { get; set; }
		public ZString CoLoadMasterBillNumber { get; set; }
		public ZGuid BookingPartyPK { get; set; }
		public ZString BookingPartyName { get; set; }

		public bool IsEmpty
		{
			get
			{
				return HBOLNumber.IsEmpty
					&& HAWBNumber.IsEmpty
					&& ShippersReference.IsEmpty
					&& (OrderNumbers == null || OrderNumbers.Count == 0)
					&& InterimReceipt.IsEmpty;
			}
		}
	}
}
