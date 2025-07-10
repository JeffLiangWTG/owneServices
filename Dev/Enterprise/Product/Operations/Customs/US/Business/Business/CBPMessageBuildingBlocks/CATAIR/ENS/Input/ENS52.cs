using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.BIRD.Common;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public partial class ENS52 : Messaging.Business.MessageBuildingBlocks.Input.Abstract.ENS52, IBIRDLineRecord
	{
		#region IBIRDLineRecord Members

		void IBIRDLineRecord.Update(JobComInvoiceLine invoiceLine, INotifications notifications)
		{
			invoiceLine.US_SWPMIndicator = ChinaHongKongSWPMIndicator;
			invoiceLine.US_CAExportCertificate = CanadianExportCertificateSugar;
			invoiceLine.US_WoolLicenceNo = WoolLicense;
			invoiceLine.US_CBTPACertificateNo = CBTPACertificationNumber;
			invoiceLine.US_MiscPermitNo = MiscellaneousPermitLicenseNumber;
			invoiceLine.US_LumberExportPrice = ZDecimal.ParseSafe(OtherDataElement1, 0);
			invoiceLine.US_LumberImporterDeclaration = OtherDataElement2.SubstringSafe(0, 1);
			invoiceLine.US_LumberExportCharges = ZDecimal.ParseSafe(OtherDataElement2.SubstringSafe(1), 0);
		}

		ZString IBIRDTariffRecord.Tariff
		{
			get { return ZString.Empty; }
		}

		#endregion
	}
}
