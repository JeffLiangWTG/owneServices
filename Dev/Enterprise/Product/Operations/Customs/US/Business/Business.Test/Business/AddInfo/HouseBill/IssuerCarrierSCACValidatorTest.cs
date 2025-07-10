using CargoWise.BrandManager;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class IssuerCarrierSCACValidatorTest : TestCaseWithFactory
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Testing")]
		public void TestValidateBillIssuerSCACCode()
		{
			var declaration = GetImportDeclaration();
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			var bill = declaration.Bills.AddNew();
			bill.US_UI_NKBillIssuerSCAC = "~";
			AssertHasMessageError("4 character code for truck, rail and vessel shipments and an abbreviation (2 characters) for air", bill.US_UI_NKBillIssuerSCACInfo, IssuerCarrierSCACValidator.InvalidSCACFormat);
			bill.US_UI_NKBillIssuerSCAC = "~DFR";
			AssertNoMessageError("4 character code for truck, rail and vessel shipments and an abbreviation (2 characters) for air", bill.US_UI_NKBillIssuerSCACInfo, IssuerCarrierSCACValidator.InvalidSCACFormat);
			AssertHasMessageErrorContaining(bill.US_UI_NKBillIssuerSCACInfo, IssuerCarrierSCACValidator.InvalidSCAC + string.Format(IssuerCarrierSCACValidator.SendSCACRequestMessagesAdvice, BrandingFactory.Instance.ProductName, "Issuer"));
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "MKAR";
			carrier.UI_ModeOfTransportation = "40";
			bill.US_UI_NKBillIssuerSCAC = carrier.UI_Code;
			AssertNoMessageErrorContaining(bill.US_UI_NKBillIssuerSCACInfo, IssuerCarrierSCACValidator.InvalidSCAC + string.Format(IssuerCarrierSCACValidator.SendSCACRequestMessagesAdvice, BrandingFactory.Instance.ProductName, "Issuer"));
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			bill.AddInfoValidation.ValidateUS_UI_NKBillIssuerSCAC();
			AssertHasMessageError("4 character code for truck, rail and vessel shipments and an abbreviation (2 characters) for air", bill.US_UI_NKBillIssuerSCACInfo, IssuerCarrierSCACValidator.InvalidSCACFormat);
			bill.US_UI_NKBillIssuerSCAC = "QF";
			AssertNoMessageError("4 character code for truck, rail and vessel shipments and an abbreviation (2 characters) for air", bill.US_UI_NKBillIssuerSCACInfo, IssuerCarrierSCACValidator.InvalidSCACFormat);
			carrier.UI_Code = "AA";
			declaration.JE_TransportMode = TransportTypeList.Codes.PassengerHandCarried;
			bill.US_UI_NKBillIssuerSCAC = carrier.UI_Code;
			AssertNoMessageError("No Format validation message errors, because 'PassengerHandCarried' allows any SCAC (Air SCAC also)", bill.US_UI_NKBillIssuerSCACInfo, IssuerCarrierSCACValidator.InvalidSCACFormat);
			bill.US_UI_NKBillIssuerSCAC = "APLU";
			AssertNoMessageError("No Format validation message errors, because 'PassengerHandCarried' allows Sea SCAC", bill.US_UI_NKBillIssuerSCACInfo, IssuerCarrierSCACValidator.InvalidSCACFormat);
		}

		public void TestIsserSCACValidationWithCode()
		{
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "LMAG";
			carrier.UI_ModeOfTransportation = "10";
			var carrier2 = Factory.New<USCarrierCombined>();
			carrier2.UI_Code = "DDKK";
			carrier2.UI_ModeOfTransportation = "10";

			var declaration = GetImportDeclaration();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;

			var bill = declaration.Bills.AddNew();
			bill.US_UI_NKBillIssuerSCAC = "LMAG";
			AssertEquals(true, new IssuerCarrierSCACValidator(bill.Factory).IsValidSCACCode("LMAG"));
			AssertEquals(true, new IssuerCarrierSCACValidator(Factory).IsValidSCACCode("DDKK"));
			AssertEquals(false, new IssuerCarrierSCACValidator(Factory).IsValidSCACCode("ABA2"));
		}

		public void TestRepressIssuerAndCarrierSCACValidation()
		{
			var declaration = GetImportDeclaration();
			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			var bill = declaration.Bills.AddNew();
			bill.US_UI_NKBillIssuerSCAC = "5555";
			declaration.JE_MasterBillIssuerSCAC = "5555";
			declaration.US_UI_NKCarrierSCAC = "5555";
			string issuerError = $"Issuer Standard Carrier Alpha Code (SCAC) unable to be found.When the Declaration is saved, {BrandingFactory.Instance.ProductName} will automatically submit a request for the latest information relating to the Issuer SCAC that has been entered. A response should be available in a few minutes. Note that SCAC Requests can also be sent manually, at any time, by selecting Customs Declarations > Actions > Reference Files Request > Carrier Codes.";
			string carrierError = $"Carrier Standard Carrier Alpha Code (SCAC) unable to be found.When the Declaration is saved, {BrandingFactory.Instance.ProductName} will automatically submit a request for the latest information relating to the Carrier SCAC that has been entered. A response should be available in a few minutes. Note that SCAC Requests can also be sent manually, at any time, by selecting Customs Declarations > Actions > Reference Files Request > Carrier Codes.";
			new IssuerCarrierSCACValidator(bill.Factory).ValidateSCACCode(bill.US_UI_NKBillIssuerSCACInfo, declaration.JE_TransportMode, "Issuer", !declaration.IsACEAutoRoadAndPedTransportMode, declaration.IsExport);
			AssertNoError(bill.US_UI_NKBillIssuerSCACInfo, issuerError);
			AssertNoError(declaration.US_UI_NKCarrierSCACInfo, carrierError);
			AssertNoError(declaration.JE_MasterBillIssuerSCACInfo, issuerError);
			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			bill.US_UI_NKBillIssuerSCAC = "3333";
			declaration.JE_MasterBillIssuerSCAC = "3333";
			declaration.US_UI_NKCarrierSCAC = "3333";
			AssertHasMessageError(declaration.US_UI_NKCarrierSCACInfo, carrierError);
			AssertHasMessageError(declaration.JE_MasterBillIssuerSCACInfo, issuerError);
		}

		public void TestValidateDeclarationCarrierSCAC()
		{
			var declaration = GetImportDeclaration();
			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			declaration.US_UI_NKCarrierSCAC = "QF";
			AssertHasMessageError("4 character code for truck, rail and vessel shipments and an abbreviation (2 characters) for air", declaration.US_UI_NKCarrierSCACInfo, IssuerCarrierSCACValidator.InvalidSCACFormat);
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.AddInfoValidation.ValidateUS_UI_NKCarrierSCAC();
			AssertNoMessageError("4 character code for truck, rail and vessel shipments and an abbreviation (2 characters) for air", declaration.US_UI_NKCarrierSCACInfo, IssuerCarrierSCACValidator.InvalidSCACFormat);
		}

		JobDeclaration GetImportDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			return declaration;
		}
	}
}
