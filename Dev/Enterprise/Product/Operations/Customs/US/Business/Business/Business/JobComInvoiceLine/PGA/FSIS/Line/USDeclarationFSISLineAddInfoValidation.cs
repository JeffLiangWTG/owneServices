using System.Linq;

namespace Enterprise.Customs.US.Business
{
	public class USDeclarationFSISLineAddInfoValidation : USFSISLineAddInfoValidation
	{
		public USDeclarationFSISLineAddInfoValidation(AutoUSFSISLineAddInfo parent)
			: base(parent)
		{ }

		new USFSISLineAddInfo Parent
		{
			get { return base.Parent; }
		}

		USDeclarationFSISLine Line
		{
			get { return (USDeclarationFSISLine)Parent.Parent; }
		}

		protected override void CheckUS_HealthCertificateNumber()
		{
			base.CheckUS_HealthCertificateNumber();

			if (!Parent.US_HealthCertificateNumber.IsEmpty)
			{
				if (Line.Declaration.FSISLines.Count(line => ((USDeclarationFSISLine)line).US_HealthCertificateNumber == Parent.US_HealthCertificateNumber) > 1)
				{
					Parent.US_HealthCertificateNumberInfo.AddMessageError(CertificateIsDuplicate);
				}
			}
		}
		internal const string CertificateIsDuplicate = "You have already entered this Certificate Number.";

		protected override void CheckUS_UC_NKCountryOfOrigin()
		{
			if (HasInvoiceLineUsedThisCertificate)
			{
				base.CheckUS_UC_NKCountryOfOrigin();
			}
		}

		protected override void CheckUS_UC_NKCertificateIssuerCountry()
		{
			if (HasInvoiceLineUsedThisCertificate)
			{
				base.CheckUS_UC_NKCertificateIssuerCountry();
			}
		}

		protected override void CheckUS_ImportingEstNo()
		{
			if (HasInvoiceLineUsedThisCertificate)
			{
				base.CheckUS_ImportingEstNo();
			}
		}

		protected override void CheckUS_CommercialDescription()
		{
			if (HasInvoiceLineUsedThisCertificate)
			{
				base.CheckUS_CommercialDescription();
			}
		}

		protected override void CheckUS_ExportingEstNo()
		{
			if (HasInvoiceLineUsedThisCertificate)
			{
				base.CheckUS_ExportingEstNo();
			}
		}

		protected override void CheckUS_ProductID()
		{
			if (HasInvoiceLineUsedThisCertificate)
			{
				base.CheckUS_ProductID();
			}
		}

		protected override void CheckUS_ProductIDQualifier()
		{
			if (HasInvoiceLineUsedThisCertificate)
			{
				base.CheckUS_ProductIDQualifier();
			}
		}

		protected override void CheckUS_IntendedUseCode()
		{
			if (HasInvoiceLineUsedThisCertificate)
			{
				base.CheckUS_IntendedUseCode();
			}
		}

		protected override void CheckUS_DateOfInspection()
		{
			if (HasInvoiceLineUsedThisCertificate)
			{
				base.CheckUS_DateOfInspection();
			}
		}

		protected override bool IsPGAValidationOn
		{
			get
			{
				var result = false;
				var line = Line;
				if (line != null)
				{
					var declaration = (JobDeclaration)line.Parent;
					result = declaration != null && declaration.IsPGAValidationOn();
				}
				return result;
			}
		}

		protected override void CheckUS_CertifyingIndividual()
		{
			if (HasInvoiceLineUsedThisCertificate)
			{
				base.CheckUS_CertifyingIndividual();
			}
		}

		protected override void CheckUS_PGAContactName()
		{
			if (HasInvoiceLineUsedThisCertificate)
			{
				base.CheckUS_PGAContactName();
			}
		}

		protected override void CheckUS_PGAContactPhoneNo()
		{
			if (HasInvoiceLineUsedThisCertificate)
			{
				base.CheckUS_PGAContactPhoneNo();
			}
		}

		protected override void CheckUS_PGAContactEmail()
		{
			if (HasInvoiceLineUsedThisCertificate)
			{
				base.CheckUS_PGAContactEmail();
			}
		}

		bool HasInvoiceLineUsedThisCertificate
		{
			get { return Line.Declaration.InvoiceLines.Cast<JobComInvoiceLine>().Any(invoiceLine => invoiceLine.FSISLines.Cast<USInvoiceLineFSISLine>().Any(fsisLine => fsisLine.US_HealthCertificateNumber == Parent.US_HealthCertificateNumber)); }
		}
	}
}
