using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.ACE;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
{
	public partial class AENS54 : Abstract.AENS54, IACEBIRDLineRecord
	{
		#region IACEBIRDLineRecord Members

		void IACEBIRDLineRecord.Update(JobComInvoiceLine invoiceLine, INotifications notifications)
		{
			invoiceLine.US_LumberImporterDeclaration = ImportersAdditionalDeclarationInformation.SubstringSafe(0, 1);
			invoiceLine.US_LumberExportCharges = ZDecimal.ParseSafe(ImportersAdditionalDeclarationInformation.SubstringSafe(1, 10), 0);
			invoiceLine.US_LumberExportPrice = ZDecimal.ParseSafe(ImportersAdditionalDeclarationInformation.SubstringSafe(11), 0);
		}

		#endregion

	}
}
