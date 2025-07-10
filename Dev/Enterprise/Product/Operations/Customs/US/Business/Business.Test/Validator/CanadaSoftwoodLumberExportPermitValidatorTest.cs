using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CanadaSoftwoodLumberExportPermitValidatorTest : PermitValidatorTest<CanadaSoftwoodLumberExportPermitValidator>
	{
		public void TestPermitNotRequired()
		{
			var factory = new BusinessObjectFactory();
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfOrigin = "XO";
			invoiceLine.JI_Tariff = "4409102000";
			var validator = new CanadaSoftwoodLumberExportPermitValidator(invoiceLine);
			AssertEquals("", validator.GetErrorTextForRequirement(ZString.Empty));
		}

		protected override string[] ValidNumbers => new string[] { "123456789", "1234HNTWX" };

		protected override string[] InvalidNumbers => new string[] { "1234567890", "12345ABCDE", "1234 ABCDE" };

		protected override string LicenseTypeCode => LicencePermitTypeList.Codes._11;

		protected override string LicenseTypeDescription => "Atlantic Lumber Board (ALB) Certificate";

		protected override string[] PermitRequiredEntryTypes
		{
			get
			{
				return new string[]
				{
					EntryTypeList.Codes.ConsumptionFreeDutiable,
					EntryTypeList.Codes.ConsumptionQuotaVisa,
					EntryTypeList.Codes.ConsumptionADDCVD,
					EntryTypeList.Codes.ConsumptionFTZ,
					EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa,
					EntryTypeList.Codes.Warehouse,
					EntryTypeList.Codes.ReWarehouse,
					EntryTypeList.Codes.InformalFreeDutiable,
					EntryTypeList.Codes.InformalQuotaVisa
				};
			}
		}

		protected override string[] PermitNotRequiredEntryTypes => new string[] { EntryTypeList.Codes.PermanentExhibition };

		protected override void SetupExtraTestDataForInvoiceLine(JobComInvoiceLine invoiceLine)
		{
			base.SetupExtraTestDataForInvoiceLine(invoiceLine);
			invoiceLine.US_UC_NKCountryOfOrigin = "XO";
			invoiceLine.JI_Tariff = "4409102000";
			invoiceLine.US_LumberImporterDeclaration = YesNoDefaultList.Codes.Yes;
		}
	}
}
