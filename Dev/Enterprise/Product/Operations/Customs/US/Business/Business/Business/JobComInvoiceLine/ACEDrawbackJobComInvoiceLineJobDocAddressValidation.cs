using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class ACEDrawbackJobComInvoiceLineJobDocAddressValidation : JobDocAddressValidation
	{
		public ACEDrawbackJobComInvoiceLineJobDocAddressValidation(JobDocAddress parent, JobComInvoiceLine invoiceLine)
			: base(parent)
		{
			this.invoiceLine = invoiceLine;
		}
		readonly JobComInvoiceLine invoiceLine;

		ZBool IsOrganizationMandatory
		{
			get { return Parent.E2_AddressType == DocAddressTypes.Codes.DrawbackExporterOrDestroyer; }
		}

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();

			if (invoiceLine.US_DRWIsForExportSection && !Parent.E2_AddressOverride)
			{
				if (IsOrganizationMandatory)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.OrganisationPKInfo, "Exporter/Destroyer");
				}

				if (Parent.E2_CompanyName.Length > JobComInvoiceLine.ExporterNameMaxLength)
				{
					Parent.OrganisationPKInfo.AddWarning(ZString.Format(CompanyNameTruncated, JobComInvoiceLine.ExporterNameMaxLength));
				}
			}
		}

		protected override void CheckE2_CompanyName()
		{
			base.CheckE2_CompanyName();

			if (invoiceLine.US_DRWIsForExportSection && Parent.E2_AddressOverride)
			{
				if (IsOrganizationMandatory)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.E2_CompanyNameInfo);
				}

				if (Parent.E2_CompanyName.Length > JobComInvoiceLine.ExporterNameMaxLength)
				{
					Parent.E2_CompanyNameInfo.AddWarning(ZString.Format(CompanyNameTruncated, JobComInvoiceLine.ExporterNameMaxLength));
				}
			}
		}
		internal const string CompanyNameTruncated = "Exporter/Destroyer company name is too long. Drawback messages will be sent using only the first {0} characters.";
	}
}
