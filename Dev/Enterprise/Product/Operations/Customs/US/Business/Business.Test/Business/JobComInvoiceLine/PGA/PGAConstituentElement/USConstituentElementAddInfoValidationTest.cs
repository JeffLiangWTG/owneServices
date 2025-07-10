using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	public class USConstituentElementAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_OA_ProducerAddress()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			var pga = invoiceLine.LaceyActLines.AddNew();
			var element = pga.PG04ConstituentElements.AddNew();
			var addInfo = new ConstituentElementAddInfo(element.B7_AddInfoDataInfo);
			element.B7_AddInfoData = null;

			AssertNoExceptionThrown(() => addInfo.Validation.ValidateUS_OA_ProducerAddress());
		}

		public void TestConstituentValidateCharactorsForAddressDescription()
		{
			string addressDescriptionWarning = "Address Description : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with an asterisk '*'.";
			string addressCodeWarning = "Address Code : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with an asterisk '*'.";

			var party1 = Factory.New<OrgHeader>();
			var orgAddress1 = party1.MainAddress;
			orgAddress1.OA_City = "KYIV";
			orgAddress1.OA_Address1 = "éééÄöß";
			orgAddress1.OA_Address2 = "Address2Äöß";
			orgAddress1.OA_Code = "öß";

			var party2 = Factory.New<OrgHeader>();
			var orgAddress2 = party2.MainAddress;
			orgAddress2.OA_City = "KYIV";
			orgAddress2.OA_Address1 = "address1";
			orgAddress2.OA_Address2 = "address2";
			orgAddress2.OA_Code = "code";

			ConstituentElement.US_OA_ProducerAddress = orgAddress1.PK;
			AssertHasWarning(ConstituentElement.US_OA_ProducerAddressInfo, addressDescriptionWarning);
			AssertHasWarning(ConstituentElement.US_OA_ProducerAddressInfo, addressCodeWarning);
			ConstituentElement.US_OA_ProducerAddress = orgAddress2.PK;
			AssertNoWarning(ConstituentElement.US_OA_ProducerAddressInfo, addressDescriptionWarning);
			AssertNoWarning(ConstituentElement.US_OA_ProducerAddressInfo, addressCodeWarning);
		}

		public void TestCheckUS_PGAPercentOfConstituentElement()
		{
			ConstituentElement.US_PGAPercentOfConstituentElement = 150.23m;
			AssertHasMessageError(ConstituentElement.US_PGAPercentOfConstituentElementInfo, USConstituentElementAddInfoValidation.MoreThan100Percents);

			ConstituentElement.US_PGAPercentOfConstituentElement = 99.999m;
			AssertNoMessageError(ConstituentElement.US_PGAPercentOfConstituentElementInfo, USConstituentElementAddInfoValidation.MoreThan100Percents);
		}
		public void TestCheckUS_PGAPercentOfConstituentElementForProduct()
		{
			ConstituentElementForProduct.US_PGAPercentOfConstituentElement = 150.23m;
			AssertHasMessageError(ConstituentElementForProduct.US_PGAPercentOfConstituentElementInfo, USConstituentElementAddInfoValidation.MoreThan100Percents);

			ConstituentElementForProduct.US_PGAPercentOfConstituentElement = 99.999m;
			AssertNoMessageError(ConstituentElementForProduct.US_PGAPercentOfConstituentElementInfo, USConstituentElementAddInfoValidation.MoreThan100Percents);
		}

		public void TestCheckUS_PGAQuantityOfConstituentElementForInvoiceLine()
		{
			ConstituentElement.AddInfoValidation.ValidateUS_PGAQuantityOfConstituentElement();
			AssertHasMessageError(ConstituentElement.US_PGAQuantityOfConstituentElementInfo, USConstituentElementAddInfoValidation.QuantityRequired);

			ConstituentElement.US_PGAQuantityOfConstituentElement = 150m;
			AssertNoMessageError(ConstituentElement.US_PGAQuantityOfConstituentElementInfo, USConstituentElementAddInfoValidation.QuantityRequired);
		}

		public void TestCheckUS_PGAQuantityOfConstituentElementForProduct()
		{
			ConstituentElementForProduct.AddInfoValidation.ValidateUS_PGAQuantityOfConstituentElement();
			AssertNoMessageError("PGA Qty is not required for product", ConstituentElementForProduct.US_PGAQuantityOfConstituentElementInfo, USConstituentElementAddInfoValidation.QuantityRequired);

			ConstituentElementForProduct.US_PGAQuantityOfConstituentElement = 150m;
			AssertNoMessageError("PGA Qty is not required for product, but can be entered", ConstituentElementForProduct.US_PGAQuantityOfConstituentElementInfo, USConstituentElementAddInfoValidation.QuantityRequired);

			ConstituentElement.US_PGAQuantityOfConstituentElement = 0m;
			AssertHasMessageError(ConstituentElement.US_PGAQuantityOfConstituentElementInfo, USConstituentElementAddInfoValidation.QuantityRequired);
		}

		public void TestCheckUS_PGAUnitOfMeasureForInvoiceLine()
		{
			ConstituentElement.US_PGAQuantityOfConstituentElement = 10m;
			ConstituentElement.US_PGAUnitOfMeasure = LaceyActUnitsOfMeasureList.Codes.CubicMeters;
			AssertNoMessageError(ConstituentElement.US_PGAUnitOfMeasureInfo, USConstituentElementAddInfoValidation.UQAllow5Characters);
			AssertNoMessageError(ConstituentElement.US_PGAUnitOfMeasureInfo, ListValidation.InvalidCodeMessageError);

			ConstituentElement.US_PGAUnitOfMeasure = "";
			AssertHasMessageError(ConstituentElement.US_PGAUnitOfMeasureInfo, USConstituentElementAddInfoValidation.UQRequired);
			AssertNoMessageError(ConstituentElement.US_PGAUnitOfMeasureInfo, USConstituentElementAddInfoValidation.UQNotRequired);

			ConstituentElement.US_PGAQuantityOfConstituentElement = 0m;
			ConstituentElement.US_PGAUnitOfMeasure = LaceyActUnitsOfMeasureList.Codes.Dozen;
			AssertNoMessageError(ConstituentElement.US_PGAUnitOfMeasureInfo, USConstituentElementAddInfoValidation.UQNotRequired);

			ConstituentElement.US_PGAUnitOfMeasure = "ABCDE";
			AssertNoMessageError(ConstituentElement.US_PGAUnitOfMeasureInfo, USConstituentElementAddInfoValidation.UQAllow5Characters);
			AssertNoMessageError(ConstituentElement.US_PGAUnitOfMeasureInfo, USConstituentElementAddInfoValidation.UQRequired);
			AssertHasMessageError(ConstituentElement.US_PGAUnitOfMeasureInfo, ListValidation.InvalidCodeMessageError);

			var invoiceLine = PGA.InvoiceLine;
			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.DRU;
			fda.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._130000;
			var constElement = fda.ProductConstituentElements.AddNew();
			constElement.US_PGAUnitOfMeasure = LaceyActUnitsOfMeasureList.Codes.Kilograms;
			AssertNoMessageError(constElement.US_PGAUnitOfMeasureInfo, USConstituentElementAddInfoValidation.UQRequired);
			AssertHasMessageError(constElement.US_PGAUnitOfMeasureInfo, USConstituentElementAddInfoValidation.UQNotRequired);

			constElement.US_PGAQuantityOfConstituentElement = 20m;
			constElement.US_PGAUnitOfMeasure = "";
			AssertHasMessageError(constElement.US_PGAUnitOfMeasureInfo, USConstituentElementAddInfoValidation.UQRequired);

			constElement.US_PGAUnitOfMeasure = "AB# ";
			AssertHasMessageError(constElement.US_PGAUnitOfMeasureInfo, USConstituentElementAddInfoValidation.UQAllow5Characters);
			constElement.US_PGAUnitOfMeasure = "ABCDE";
			AssertNoMessageError(constElement.US_PGAUnitOfMeasureInfo, USConstituentElementAddInfoValidation.UQAllow5Characters);
			AssertNoMessageError(constElement.US_PGAUnitOfMeasureInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_PGAUnitOfMeasureForProduct()
		{
			ConstituentElementForProduct.US_PGAUnitOfMeasure = "!";
			AssertHasMessageError("Invalid Code", ConstituentElementForProduct.US_PGAUnitOfMeasureInfo, USConstituentElementAddInfoValidation.UQAllow5Characters);

			ConstituentElementForProduct.US_PGAUnitOfMeasure = "ABCDE";
			AssertNoMessageError("Invalid Code", ConstituentElementForProduct.US_PGAUnitOfMeasureInfo, USConstituentElementAddInfoValidation.UQAllow5Characters);
			AssertNoMessageError("Constituent Element UQ is not required for product, but can be entered", ConstituentElementForProduct.US_PGAUnitOfMeasureInfo, USConstituentElementAddInfoValidation.UQRequired);

			ConstituentElementForProduct.US_PGAUnitOfMeasure = "";
			AssertNoMessageError("Constituent Element UQ is not required for product", ConstituentElementForProduct.US_PGAUnitOfMeasureInfo, USConstituentElementAddInfoValidation.UQRequired);

			ConstituentElement.US_PGAQuantityOfConstituentElement = 10m;
			ConstituentElement.US_PGAUnitOfMeasure = "";
			AssertHasMessageError(ConstituentElement.US_PGAUnitOfMeasureInfo, USConstituentElementAddInfoValidation.UQRequired);
		}

		public void TestCheckUS_PGANameOfTheConstituentElement()
		{
			ConstituentElement.US_PGANameOfTheConstituentElement = "";
			AssertHasMessageError(ConstituentElement.US_PGANameOfTheConstituentElementInfo, "Name Of The Constituent Element is mandatory.");

			ConstituentElement.US_PGANameOfTheConstituentElement = "TEST";
			AssertNoMessageError(ConstituentElement.US_PGANameOfTheConstituentElementInfo, "Name Of The Constituent Element is mandatory.");

			ConstituentElementForProduct.US_PGANameOfTheConstituentElement = "";
			AssertHasMessageError(ConstituentElementForProduct.US_PGANameOfTheConstituentElementInfo, "Name Of The Constituent Element is mandatory.");

			ConstituentElementForProduct.US_PGANameOfTheConstituentElement = "TEST";
			AssertNoMessageError(ConstituentElementForProduct.US_PGANameOfTheConstituentElementInfo, "Name Of The Constituent Element is mandatory.");
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
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
					declaration.US_EnableENS = true;
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
