using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(BuyerLocalAddressRequirement))]
	sealed class BuyerLocalAddressRequirementTest : TestCaseWithFactory
	{
		public void TestCheckE2_CompanyNameMaxLength()
		{
			var warningMessage = "Only the first 70 characters will be sent to the customs.";
			var targetInfo = buyerLocalAddress.E2_CompanyNameInfo;
			buyerLocalAddress.E2_CompanyName = new ZString('A', 90);
			AssertHasWarning(targetInfo, warningMessage);

			buyerLocalAddress.E2_CompanyName = new ZString('A', 69);
			AssertNoWarning(targetInfo, warningMessage);
		}

		public void TestCheckE2_Address1Maxlength()
		{
			var warningMessage = "Chinese Address Only the first 100 characters will be sent to the customs.";
			var targetInfo = buyerLocalAddress.E2_Address1Info;
			buyerLocalAddress.Address2 = new string('A', 50);
			buyerLocalAddress.AdditionalAddressInformation = new string('A', 50);
			buyerLocalAddress.Address1 = new string('A', 50);
			AssertHasWarning(targetInfo, warningMessage);

			buyerLocalAddress.Address2 = ZString.Empty;
			buyerLocalAddress.Address1 = new string('A', 49);
			AssertNoWarning(targetInfo, warningMessage);
		}

		public void TestCheckE2_City()
		{
			buyerLocalAddress.E2_AddressOverride = true;
			buyerLocalAddress.E2_RN_NKCountryCode = "AU";
			buyerLocalAddress.Validation.ValidateE2_City();
			AssertNoErrors(buyerLocalAddress.E2_CityInfo);
		}

		public void TestCheckE2_Postcode()
		{
			buyerLocalAddress.E2_AddressOverride = true;
			buyerLocalAddress.Validation.ValidateE2_Postcode();
			AssertNoErrors(buyerLocalAddress.E2_PostcodeInfo);
		}

		public void TestCheckE2_State()
		{
			var targetInfo = buyerLocalAddress.E2_StateInfo;
			var warningMessage = "This state code needs to be followed by valid country code.";
			buyerLocalAddress.E2_AddressOverride = true;
			buyerLocalAddress.E2_RN_NKCountryCode = "78";
			buyerLocalAddress.E2_State = "EE";
			AssertHasWarning(targetInfo, warningMessage);
			buyerLocalAddress.E2_RN_NKCountryCode = "DE";
			buyerLocalAddress.Validation.ValidateE2_State();
			AssertNoWarning(targetInfo, warningMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var buyerDocumentaryAddress = invoice.BuyerDocumentaryAddress;
			buyerDocumentaryAddress.E2_AddressOverride = true;
			buyerLocalAddress = buyerDocumentaryAddress.LocalAddress;
		}

		JobDocAddress buyerLocalAddress;
	}
}
