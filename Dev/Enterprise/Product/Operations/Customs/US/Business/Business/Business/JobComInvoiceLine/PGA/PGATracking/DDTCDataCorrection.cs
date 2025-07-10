using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	class DDTCDataCorrection : InvoiceLineLevelPGADataCorrection, IDDTCData
	{
		public DDTCDataCorrection(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		protected override ZPropertyInfo GetTrackingStatusInfo()
		{
			return InvoiceLine.US_DDTCTrackingStatusInfo;
		}

		protected override string[] GetRelatedInvoiceLineFields()
		{
			return new[] { JobComInvoiceLine.Schema.US_DDTCLicenseType, JobComInvoiceLine.Schema.US_DDTCArrivalDate, JobComInvoiceLine.Schema.US_DDTCExemptionCode, JobComInvoiceLine.Schema.US_DDTCLicenseNo, JobComInvoiceLine.Schema.US_DDTCRegistrationNo };
		}

		protected override string[] GetIndicatorFields()
		{
			return new[] { JobComInvoiceLine.Schema.US_DDTCInd };
		}

		protected override string[] GetDislaimReasonFields()
		{
			return System.Array.Empty<string>();
		}

		#region IDDTCData Members

		ZDateTime IDDTCData.AnticipatedArrivalDate
		{
			get { return InvoiceLine.US_DDTCArrivalDate; }
		}

		ZString IDDTCData.ExemptionCode
		{
			get { return InvoiceLine.US_DDTCExemptionCode; }
		}

		ZString IDDTCData.LicenseType
		{
			get { return InvoiceLine.US_DDTCLicenseType; }
		}

		ZString IDDTCData.LicenseNumber
		{
			get { return InvoiceLine.US_DDTCLicenseNo; }
		}

		ZString IDDTCData.RegistrationNumber
		{
			get { return InvoiceLine.US_DDTCRegistrationNo; }
		}

		#endregion

		protected override ZString IPGALineStatusGovernmentAgencyCode
		{
			get { return ACEGovernmentAgenciesCodeList.Codes.DTC; }
		}

		protected override ZInt IPGALineNumber
		{
			get { return 1; }
		}
	}
}
