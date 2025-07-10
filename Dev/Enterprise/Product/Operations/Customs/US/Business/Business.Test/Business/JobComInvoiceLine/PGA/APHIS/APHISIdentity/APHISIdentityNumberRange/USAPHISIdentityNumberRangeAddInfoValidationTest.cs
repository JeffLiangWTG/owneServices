using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USAPHISIdentityNumberRangeAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_StartNumber()
		{
			Identity.UseMultipleNumbers = true;
			var range = Identity.NumberRanges.AddNew();
			range.US_StartNumber = "A";
			AssertNoMessageErrorContaining(range.US_StartNumberInfo, MandatoryValidation.YouHaveNotEntered);
			range.US_StartNumber = ZString.Empty;
			AssertHasMessageErrorContaining(range.US_StartNumberInfo, MandatoryValidation.YouHaveNotEntered);
			Declaration.ValidationModes = ValidationModes.None;
			range.AddInfoValidation.ValidateUS_StartNumber();
			AssertNoMessageErrorContaining(range.US_StartNumberInfo, MandatoryValidation.YouHaveNotEntered);
			Declaration.RecalculateValidationModesOnDeclaration();
			range.AddInfoValidation.ValidateUS_StartNumber();
			AssertHasMessageErrorContaining(range.US_StartNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_EndNumber()
		{
			Identity.UseMultipleNumbers = true;
			var range = Identity.NumberRanges.AddNew();
			range.US_StartNumber = "A";
			range.US_EndNumber = "B";
			AssertNoMessageError(range.US_EndNumberInfo, ValidationConstants.APHIS.IdentityEndNumberRequiresStartNumber);
			range.US_StartNumber = ZString.Empty;
			AssertHasMessageError(range.US_EndNumberInfo, ValidationConstants.APHIS.IdentityEndNumberRequiresStartNumber);
			Declaration.ValidationModes = ValidationModes.None;
			range.AddInfoValidation.ValidateUS_EndNumber();
			AssertNoMessageError(range.US_EndNumberInfo, ValidationConstants.APHIS.IdentityEndNumberRequiresStartNumber);
			Declaration.RecalculateValidationModesOnDeclaration();
			range.AddInfoValidation.ValidateUS_EndNumber();
			AssertHasMessageError(range.US_EndNumberInfo, ValidationConstants.APHIS.IdentityEndNumberRequiresStartNumber);
			range.US_EndNumber = ZString.Empty;
			AssertNoMessageErrors(range.US_EndNumberInfo);
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

		APHISProduct Product
		{
			get { return product ?? (product = Header.Products.AddNew()); }
		}
		APHISProduct product;

		APHISIdentity Identity
		{
			get { return identity ?? (identity = Product.Identities.AddNew()); }
		}
		APHISIdentity identity;

		#endregion
	}
}
