using CargoWise.Types;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class CommonConsolReferences : AdditionalReferencesParent
	{
		public ZString MBOLNumber { get; set; }
		public ZString MAWBNumber { get; set; }
		public ZString CarriersBookingReference { get; set; }
		public ZString AgentsReference { get; set; }
		public ZString LoadPort { get; set; }
		public ZString DischargePort { get; set; }
		public ZString CarrierC1CCode { get; set; }

		public ZBool MatchMainCarrierReferencesToCoLoader { get; set; }
		public ZString SCAC { get; set; }
		public ZString CoLoadBookingConfirmationReference { get; set; }
		public ZString CoLoadMasterBillNumber { get; set; }
		public ZString CoLoadSCAC { get; set; }

		public ZBool IsConsolidationAdvice { get; set; }
		public ZString DocumentaryPurpose { get; set; }

		internal bool IsEmpty
		{
			get
			{
				return MBOLNumber.IsEmpty
					&& MAWBNumber.IsEmpty
					&& AgentsReference.IsEmpty;
			}
		}
	}
}
