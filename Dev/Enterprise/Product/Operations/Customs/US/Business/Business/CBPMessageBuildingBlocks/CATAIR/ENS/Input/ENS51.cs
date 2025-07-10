using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.BIRD.Common;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public partial class ENS51 : Messaging.Business.MessageBuildingBlocks.Input.Abstract.ENS51, IBIRDLineRecord
	{
		#region IBIRDLineRecord Members

		void IBIRDLineRecord.Update(JobComInvoiceLine invoiceLine, INotifications notifications)
		{
			invoiceLine.US_DateOfExportFromCountryOfOrigin = DateOfExportationTextiles;
			invoiceLine.US_VisaNo = VisaNumber;
			invoiceLine.US_TextileCategoryNo = CategoryNumber;
			invoiceLine.US_VisaQty = VisaQuantity;
			invoiceLine.US_VisaUQ = VisaUnitOfMeasure;
			invoiceLine.US_AgricultureLicNo = AgricultureLicenseNumber;
			invoiceLine.US_CottonCertificateNo = CottonCertificateNumberOrganicExemptionCertificateNumber;
		}

		ZString IBIRDTariffRecord.Tariff
		{
			get { return ZString.Empty; }
		}

		#endregion
	}
}
