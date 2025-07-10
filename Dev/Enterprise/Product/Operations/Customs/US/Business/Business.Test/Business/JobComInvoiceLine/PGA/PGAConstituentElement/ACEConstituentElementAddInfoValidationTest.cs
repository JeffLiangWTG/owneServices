using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	public class ACEConstituentElementAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_PGANameOfTheConstituentElement()
		{
			var laceyConstElement = PGA.PG04ConstituentElements.AddNew();
			laceyConstElement.AddInfoValidation.ValidateUS_PGANameOfTheConstituentElement();
			AssertHasMessageErrorContaining(laceyConstElement.US_PGANameOfTheConstituentElementInfo, "Name Of The Constituent Element is mandatory.");

			PGA.US_UnknownBreakdownTotal = true;
			laceyConstElement.AddInfoValidation.ValidateUS_PGANameOfTheConstituentElement();
			AssertNoMessageErrorContaining(laceyConstElement.US_PGANameOfTheConstituentElementInfo, "Name Of The Constituent Element is mandatory.");

			PGA.US_UnknownBreakdownTotal = false;
			laceyConstElement.US_PGANameOfTheConstituentElement = "1~";
			AssertNoMessageErrorContaining(laceyConstElement.US_PGANameOfTheConstituentElementInfo, "Name Of The Constituent Element is mandatory.");

			var invoiceLine = PGA.InvoiceLine;
			invoiceLine.Declaration.US_EnableCRL = true;
			invoiceLine.Declaration.US_CertifyCargoRelease = true;
			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.VME;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.VME_ADR;
			fda.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._150007;
			var constElement = fda.ProductConstituentElements.AddNew();
			constElement.AddInfoValidation.ValidateUS_PGANameOfTheConstituentElement();
			AssertHasMessageErrorContaining(constElement.US_PGANameOfTheConstituentElementInfo, "Name Of Active Ingredient is mandatory.");
		}

		public void TestCheckUS_PGAPercentOfConstituentElement()
		{
			var invoiceLine = PGA.InvoiceLine;
			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.DRU;
			fda.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._150007;
			var constElement = fda.ProductConstituentElements.AddNew();
			constElement.US_PGAPercentOfConstituentElement = ZDecimal.Zero;
			constElement.AddInfoValidation.ValidateUS_PGAPercentOfConstituentElement();
			AssertHasMessageError(constElement.US_PGAPercentOfConstituentElementInfo, ACEConstituentElementAddInfoValidation.PercentRequiredIfActiveIngredient);

			constElement.US_PGAPercentOfConstituentElement = 21.5m;
			AssertNoMessageError(constElement.US_PGAPercentOfConstituentElementInfo, ACEConstituentElementAddInfoValidation.PercentRequiredIfActiveIngredient);

			fda.US_ProgramCode = FDAProgramCodeList.Codes.VME;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.VME_ADR;
			constElement.US_PGAPercentOfConstituentElement = ZDecimal.Zero;
			AssertHasMessageError(constElement.US_PGAPercentOfConstituentElementInfo, ACEConstituentElementAddInfoValidation.QuantityOrPercentIsRequired);

			constElement.US_PGAQuantityOfConstituentElement = 12m;
			constElement.AddInfoValidation.ValidateUS_PGAPercentOfConstituentElement();
			AssertNoMessageError(constElement.US_PGAPercentOfConstituentElementInfo, ACEConstituentElementAddInfoValidation.QuantityOrPercentIsRequired);

			constElement.US_PGAQuantityOfConstituentElement = ZDecimal.Zero;
			constElement.US_PGAPercentOfConstituentElement = 0.231m;
			AssertNoMessageError(constElement.US_PGAPercentOfConstituentElementInfo, ACEConstituentElementAddInfoValidation.QuantityOrPercentIsRequired);
		}

		public void TestCheckUS_PGAPercentOfConstituentElementForProduct()
		{
			var invoiceLine = PGA.InvoiceLine;
			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.DRU;
			fda.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._150007;
			var constElement = fda.ProductConstituentElements.AddNew();
			constElement.US_PGAPercentOfConstituentElement = ZDecimal.Zero;
			constElement.AddInfoValidation.ValidateUS_PGAPercentOfConstituentElement();
			AssertHasMessageError(constElement.US_PGAPercentOfConstituentElementInfo, ACEConstituentElementAddInfoValidation.PercentRequiredIfActiveIngredient);

			ConstituentElementForProduct.US_PGAPercentOfConstituentElement = 21.5m;
			AssertNoMessageError(ConstituentElementForProduct.US_PGAPercentOfConstituentElementInfo, ACEConstituentElementAddInfoValidation.PercentRequiredIfActiveIngredient);
		}

		public void TestCheckUS_UnknownBreakdownCountryCode()
		{
			var invoiceLine = PGA.InvoiceLine;
			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.DRU;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_OTC;
			var constElement = fda.ProductConstituentElements.AddNew();
			AssertNoMessageErrorContaining(constElement.US_UnknownBreakdownCountryCodeInfo, MandatoryValidation.YouHaveNotEntered);

			var laceyConstElement = PGA.PG04ConstituentElements.AddNew();
			laceyConstElement.AddInfoValidation.ValidateUS_UnknownBreakdownCountryCode();
			AssertHasMessageErrorContaining(laceyConstElement.US_UnknownBreakdownCountryCodeInfo, MandatoryValidation.YouHaveNotEntered);

			PGA.US_UnknownBreakdownTotal = true;
			laceyConstElement.AddInfoValidation.ValidateUS_UnknownBreakdownCountryCode();
			AssertNoMessageErrorContaining(laceyConstElement.US_UnknownBreakdownCountryCodeInfo, MandatoryValidation.YouHaveNotEntered);

			laceyConstElement.US_UnknownBreakdownCountryCode = Core.Constants.CountryCodes.UnitedStates;
			AssertNoMessageErrorContaining(laceyConstElement.US_UnknownBreakdownCountryCodeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_GenusName()
		{
			ConstituentElement.US_GenusName = "Test";
			AssertNoMessageErrorContaining(ConstituentElement.US_GenusNameInfo, MandatoryValidation.YouHaveNotEntered);

			ConstituentElement.US_GenusName = "";
			AssertHasMessageErrorContaining(ConstituentElement.US_GenusNameInfo, MandatoryValidation.YouHaveNotEntered);

			ConstituentElementForProduct.US_GenusName = "Test";
			AssertNoMessageErrorContaining(ConstituentElementForProduct.US_GenusNameInfo, MandatoryValidation.YouHaveNotEntered);

			ConstituentElementForProduct.US_GenusName = "";
			AssertNoMessageErrorContaining(ConstituentElementForProduct.US_GenusNameInfo, MandatoryValidation.YouHaveNotEntered);

			var fdaLine = PGA.InvoiceLine.ACE_FDALines.AddNew();
			var fdaConstituentElement = fdaLine.ProductConstituentElements.AddNew();
			fdaConstituentElement.AddInfoValidation.ValidateUS_GenusName();
			AssertNoMessageErrorContaining(fdaConstituentElement.US_GenusNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestUS_SpeciesName()
		{
			ConstituentElement.US_SpecialUseDesignation = true;

			ConstituentElement.US_SpeciesName = "~";
			AssertHasMessageErrorContaining(ConstituentElement.US_SpeciesNameInfo, ListValidation.InvalidCodeMessageError);

			ConstituentElement.US_SpeciesName = PGALaceySpeciesNameCodeList.Codes.Composite;
			AssertNoMessageErrorContaining(ConstituentElement.US_SpeciesNameInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(ConstituentElement.US_SpeciesNameInfo, MandatoryValidation.YouHaveNotEntered);

			ConstituentElement.US_SpeciesName = "";
			AssertHasMessageErrorContaining(ConstituentElement.US_SpeciesNameInfo, MandatoryValidation.YouHaveNotEntered);

			ConstituentElementForProduct.US_SpecialUseDesignation = true;
			ConstituentElementForProduct.US_SpeciesName = "~";
			AssertNoMessageErrorContaining(ConstituentElementForProduct.US_SpeciesNameInfo, ListValidation.InvalidCodeMessageError);

			ConstituentElementForProduct.US_SpeciesName = PGALaceySpeciesNameCodeList.Codes.Composite;
			AssertNoMessageErrorContaining(ConstituentElementForProduct.US_SpeciesNameInfo, ListValidation.InvalidCodeMessageError);

			ConstituentElementForProduct.US_SpeciesName = "";
			AssertNoMessageErrorContaining(ConstituentElementForProduct.US_SpeciesNameInfo, MandatoryValidation.YouHaveNotEntered);

			var fdaLine = PGA.InvoiceLine.ACE_FDALines.AddNew();
			var fdaConstituentElement = fdaLine.ProductConstituentElements.AddNew();
			fdaConstituentElement.AddInfoValidation.ValidateUS_SpeciesName();
			AssertNoMessageErrorContaining(fdaConstituentElement.US_SpeciesNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_SpecialUseDesignation()
		{
			ConstituentElement.US_GenusName = "";
			ConstituentElement.US_SpecialUseDesignation = true;
			AssertNoMessageErrorContaining(ConstituentElement.US_GenusNameInfo, MandatoryValidation.YouHaveNotEntered);

			ConstituentElement.US_SpecialUseDesignation = false;
			AssertHasMessageErrorContaining(ConstituentElement.US_GenusNameInfo, MandatoryValidation.YouHaveNotEntered);

			ConstituentElementForProduct.US_GenusName = "";
			ConstituentElementForProduct.US_SpecialUseDesignation = true;
			AssertNoMessageErrorContaining(ConstituentElementForProduct.US_GenusNameInfo, MandatoryValidation.YouHaveNotEntered);

			ConstituentElementForProduct.US_SpecialUseDesignation = false;
			AssertNoMessageErrorContaining(ConstituentElementForProduct.US_GenusNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_ProducerAddress()
		{
			var invoiceLine = PGA.InvoiceLine;
			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.DRU;
			fda.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._080000;
			var constElement = fda.ProductConstituentElements.AddNew();
			constElement.US_OA_ProducerAddress = ZGuid.Empty;
			AssertHasWarning(constElement.US_OA_ProducerAddressInfo, ACEConstituentElementAddInfoValidation.ProducerRequired);

			var party = Factory.New<OrgHeader>();
			var address = party.Addresses.AddNew();

			constElement.US_OA_ProducerAddress = address.PK;
			AssertNoWarning(constElement.US_OA_ProducerAddressInfo, ACEConstituentElementAddInfoValidation.ProducerRequired);

			ConstituentElementForProduct.US_OA_ProducerAddress = ZGuid.Empty;
			AssertNoWarning(ConstituentElementForProduct.US_OA_ProducerAddressInfo, ACEConstituentElementAddInfoValidation.ProducerRequired);

			ConstituentElementForProduct.US_OA_ProducerAddress = address.PK;
			AssertNoWarning(ConstituentElementForProduct.US_OA_ProducerAddressInfo, ACEConstituentElementAddInfoValidation.ProducerRequired);
		}

		ConstituentElement ConstituentElement
		{
			get { return constituentElement ?? (constituentElement = PGA.PG04ConstituentElements.AddNew()); }
		}
		ConstituentElement constituentElement;

		ConstituentElement ConstituentElementForProduct
		{
			get
			{
				if (constituentElementForProduct == null)
				{
					var lookup = Factory.New<CusClassification>();
					lookup.CC_LookupCode = "LOOK434";
					var product = Factory.New<OrgSupplierPart>();
					product.OP_PartNum = "PART434";

					var pivot = Factory.New<CusClassPartPivot>();
					pivot.CI_CC = lookup.PK;
					pivot.CI_OP = product.PK;

					var pga = pivot.PGAs.AddNew();
					constituentElementForProduct = pga.PG04ConstituentElements.AddNew();
				}
				return constituentElementForProduct;
			}
		}
		ConstituentElement constituentElementForProduct;

		PGA PGA
		{
			get
			{
				if (pga == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
					declaration.US_CertifyCargoRelease = true;
					declaration.US_EnableCRL = true;

					var invoiceHeader = declaration.Invoices.AddNew();
					var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					pga = invoiceLine.LaceyActLines.AddNew();
				}

				return pga;
			}
		}

		PGA pga;
	}
}
