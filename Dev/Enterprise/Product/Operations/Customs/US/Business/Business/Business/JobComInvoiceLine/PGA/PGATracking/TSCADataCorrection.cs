using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	class TSCADataCorrection : InvoiceLineLevelPGADataCorrection, ITSCAData
	{
		public TSCADataCorrection(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		protected override ZPropertyInfo GetTrackingStatusInfo()
		{
			return InvoiceLine.US_TSCATrackingStatusInfo;
		}

		protected override string[] GetRelatedInvoiceLineFields()
		{
			return new[] { JobComInvoiceLine.Schema.US_TSCACertification, JobComInvoiceLine.Schema.US_FDAContactEmail, JobComInvoiceLine.Schema.US_FDAContactName, JobComInvoiceLine.Schema.US_FDAContactPhoneNo };
		}

		protected override string[] GetIndicatorFields()
		{
			return new[] { JobComInvoiceLine.Schema.US_TSCAInd };
		}

		protected override string[] GetDislaimReasonFields()
		{
			return new[] { JobComInvoiceLine.Schema.US_TSCADisclaimReason };
		}

		#region ITSCAData Members
		ITSCAData TSCAData
		{
			get { return InvoiceLine; }
		}

		ZString ITSCAData.ContactName
		{
			get { return TSCAData.ContactName; }
		}

		ZString ITSCAData.ContactPhone
		{
			get { return TSCAData.ContactPhone; }
		}

		ZString ITSCAData.ContactEmail
		{
			get { return TSCAData.ContactEmail; }
		}

		ZString ITSCAData.TSCACertificationCode
		{
			get { return TSCAData.TSCACertificationCode; }
		}

		ZDate ITSCAData.CertifySignatureDate
		{
			get
			{
				var invoiceHeader = InvoiceHeader;
				return invoiceHeader != null ? invoiceHeader.US_TSCASignDate.Date : ZDate.Empty;
			}
			set
			{
				var invoiceHeader = InvoiceHeader;
				if (invoiceHeader != null)
				{
					invoiceHeader.US_TSCASignDate = value;
				}
			}
		}

		ZString ITSCAData.DeclarationCertificate
		{
			get
			{
				return ((ITSCAData)this).CertifySignatureDate.IsValid ? "Y" : "";
			}
		}

		ZInt ITSCAData.TSCALineNumber
		{
			get { return TSCAData.TSCALineNumber; }
			set { TSCAData.TSCALineNumber = value; }
		}

		ZInt ITSCAData.ODSLineNumber
		{
			get { return TSCAData.ODSLineNumber; }
			set { TSCAData.ODSLineNumber = value; }
		}

		#endregion

		protected override ZString IPGALineStatusGovernmentAgencyCode
		{
			get { return ACEGovernmentAgenciesCodeList.Codes.EPA; }
		}

		protected override ZInt IPGALineNumber
		{
			get { return InvoiceLine.US_TSCALineNumber; }
		}
	}
}
