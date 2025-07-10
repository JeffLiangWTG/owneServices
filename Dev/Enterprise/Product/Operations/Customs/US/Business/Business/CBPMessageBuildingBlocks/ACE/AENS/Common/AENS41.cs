using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.ACE;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
{
	public partial class AENS41 : Abstract.AENS41, IACEBIRDLineRecord
	{
		#region IACEBIRDLineRecord Members

		void IACEBIRDLineRecord.Update(JobComInvoiceLine invoiceLine, INotifications notifications)
		{
			invoiceLine.US_ZoneStatus = FTZMerchandiseStatusCode;
			invoiceLine.US_PrivilegedStatusDate = PrivilegedFTZMerchandiseFilingDate;
			invoiceLine.US_ManifestQty = FTZLineItemQuantity;
		}

		#endregion
	}
}
