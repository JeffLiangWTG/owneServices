using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.ACE;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	public partial class ASESE40 : Abstract.ASESE40, IACEBIRDLineIDRecord
	{
		#region IACEBIRDLineIDRecord Members

		ZInt IACEBIRDLineIDRecord.LineNumber
		{
			get { return LineItemIdentifier; }
		}

		#endregion

		#region IACEBIRDLineRecord Members

		void IACEBIRDLineRecord.Update(JobComInvoiceLine invoiceLine, INotifications notifications)
		{
			invoiceLine.US_UC_NKCountryOfOrigin = CountryOfOrigin;

			if (!CommercialInvoiceDescription.IsEmpty)
			{
				invoiceLine.JI_Description = CommercialInvoiceDescription;
			}
		}

		#endregion
	}
}
