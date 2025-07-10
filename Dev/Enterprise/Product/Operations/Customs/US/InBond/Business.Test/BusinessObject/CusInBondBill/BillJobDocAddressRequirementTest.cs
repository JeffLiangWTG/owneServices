using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class BillJobDocAddressRequirementTest : TestCaseWithFactory
	{
		public void TestValidationForForeignShipper()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			AssertDocAddress(bill.ForeignShipper);
			header.BH_ImportTransportMode = Enterprise.Customs.US.Business.InBondTransportModeCodes.Codes.AirNonContainer;
			var bill2 = header.Bills.AddNew();
			AssertDocAddress(bill2.ForeignShipper, true);
		}

		public void TestValidationForConsignee()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			AssertDocAddress(bill.Consignee);
			header.BH_ImportTransportMode = Enterprise.Customs.US.Business.InBondTransportModeCodes.Codes.AirNonContainer;
			var bill2 = header.Bills.AddNew();
			AssertDocAddress(bill2.Consignee, true);
		}

		public void TestValidationForNotifyParty()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			AssertDocAddress(bill.NotifyParty);
			header.BH_ImportTransportMode = Enterprise.Customs.US.Business.InBondTransportModeCodes.Codes.AirNonContainer;
			var bill2 = header.Bills.AddNew();
			AssertDocAddress(bill2.NotifyParty, true);
		}

		void AssertDocAddress(JobDocAddress docAddress, bool isAir = false)
		{
			var errorMessage = "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.";
			if (!isAir)
			{
				docAddress.E2_AddressOverride = true;
				docAddress.E2_CompanyName = "BOB THE BUILDER";
				AssertNoMessageError(docAddress.E2_CompanyNameInfo, ValidationConstants.JobDocAddress.CompanyNameRequired.ToString());
				docAddress.E2_CompanyName = ZString.Empty;
				AssertHasMessageError(docAddress.E2_CompanyNameInfo, ValidationConstants.JobDocAddress.CompanyNameRequired.ToString());
				docAddress.E2_Address1 = "ADDRESS 1";
				AssertNoMessageError(docAddress.E2_Address1Info, ValidationConstants.JobDocAddress.AddressRequired.ToString());
				docAddress.E2_Address1 = ZString.Empty;
				AssertHasMessageError(docAddress.E2_Address1Info, ValidationConstants.JobDocAddress.AddressRequired.ToString());
				Env.Instance.Registry.EnableAddressValidationWebService = true;
				docAddress.E2_ValidationStatus = AddressValidationStatus.Invalid;
				AssertHasError(docAddress.E2_ValidationStatusInfo, errorMessage);
			}
			else
			{
				docAddress.E2_CompanyName = ZString.Empty;
				AssertNoMessageError(docAddress.E2_CompanyNameInfo, ValidationConstants.JobDocAddress.CompanyNameRequired.ToString());
				Env.Instance.Registry.EnableAddressValidationWebService = true;
				docAddress.E2_ValidationStatus = AddressValidationStatus.Invalid;
				AssertNoError(docAddress.E2_ValidationStatusInfo, errorMessage);
			}
		}
	}
}
