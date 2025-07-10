using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.ACE;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	public partial class ASESE41 : Abstract.ASESE41, IACEBIRDLineRecord
	{
		#region IACEBIRDLineRecord Members

		void IACEBIRDLineRecord.Update(JobComInvoiceLine invoiceLine, INotifications notifications)
		{
			invoiceLine.US_ZoneStatus = ZoneStatus;
			invoiceLine.US_PrivilegedStatusDate = PrivilegedFTZMerchandiseFilingDate;
			invoiceLine.US_ManifestQty = FTZLineItemQuantity;
		}

		#endregion
	}
}
