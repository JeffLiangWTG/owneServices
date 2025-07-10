using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	class USAPHISRoutingAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_Type()
		{
			var routing = Header.Routings.AddNew();
			routing.US_Type = "!";
			AssertNoMessageErrorContaining(routing.US_TypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(routing.US_TypeInfo, ListValidation.InvalidCodeMessageError);
			routing.US_Type = ZString.Empty;
			AssertHasMessageErrorContaining(routing.US_TypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(routing.US_TypeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.ValidationModes = ValidationModes.None;
			routing.AddInfoValidation.ValidateUS_Type();
			AssertNoMessageErrorContaining(routing.US_TypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(routing.US_TypeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			routing.AddInfoValidation.ValidateUS_Type();
			AssertHasMessageErrorContaining(routing.US_TypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(routing.US_TypeInfo, ListValidation.InvalidCodeMessageError);
			foreach (ICodeDescription pair in new RoutingTypeList())
			{
				routing.US_Type = pair.Code;
				AssertNoMessageErrorContaining(routing.US_TypeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(routing.US_TypeInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		public void TestCheckUS_Country()
		{
			var routing = Header.Routings.AddNew();
			routing.US_Country = "!";
			AssertNoMessageErrorContaining(routing.US_CountryInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(routing.US_CountryInfo, ListValidation.InvalidCodeMessageError);
			routing.US_Country = ZString.Empty;
			AssertHasMessageErrorContaining(routing.US_CountryInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(routing.US_CountryInfo, ListValidation.InvalidCodeMessageError);
			Declaration.ValidationModes = ValidationModes.None;
			routing.AddInfoValidation.ValidateUS_Country();
			AssertNoMessageErrorContaining(routing.US_CountryInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(routing.US_CountryInfo, ListValidation.InvalidCodeMessageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			routing.AddInfoValidation.ValidateUS_Country();
			AssertHasMessageErrorContaining(routing.US_CountryInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(routing.US_CountryInfo, ListValidation.InvalidCodeMessageError);
			routing.US_Country = "AU";
			AssertNoMessageErrorContaining(routing.US_CountryInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(routing.US_CountryInfo, ListValidation.InvalidCodeMessageError);
		}

		#region Implementation

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableENS = true;
					declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		JobComInvoiceHeader Invoice
		{
			get { return invoice ?? (invoice = Declaration.Invoices.AddNew()); }
		}
		JobComInvoiceHeader invoice;

		JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = Invoice.JobComInvoiceLines.AddNew()); }
		}
		JobComInvoiceLine invoiceLine;

		APHISHeader Header
		{
			get
			{
				if (aphisHeader == null)
				{
					aphisHeader = InvoiceLine.APHISHeaders.AddNew();
					aphisHeader.US_ProgramType = APHISProgramCodeList.Codes.AVS;
				}
				return aphisHeader;
			}
		}
		APHISHeader aphisHeader;

		#endregion
	}
}
