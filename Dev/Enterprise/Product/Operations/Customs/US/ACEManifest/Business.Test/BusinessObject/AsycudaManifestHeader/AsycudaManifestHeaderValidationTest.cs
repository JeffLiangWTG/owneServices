using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class AsycudaManifestHeaderValidationTest : TestCaseWithFactory
	{
		public virtual void TestCheckEstDateAtFirstArrival()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_RL_NKPortOfFirstArrival = "USLAX";
			header.EstDateAtFirstArrival = ZDateTime.Empty;
			AssertHasMessageErrorContaining(header.EstDateAtFirstArrivalInfo, ValidationConstants.EstDateAtFirstArrivalRequired);
			header.EstDateAtFirstArrival = ZDateTime.Now;
			AssertNoMessageErrorContaining(header.EstDateAtFirstArrivalInfo, ValidationConstants.EstDateAtFirstArrivalRequired);
		}

		public void TestCheckAMA_Voyage()
		{
			// validation reports flight number mismatch.  Input must meet (2-3AN)+(3N(N)(A))
			// does not show error for <CarrierCode>N or <CarrierCode>NN because these can be converted to 2AN+3N by prefixing zeros
			var flightNumberIsWrongLengthError = ValidationConstants.FlightNumberIsWrongLength;
			var flightNumberIsNonCompliantError = ValidationConstants.FlightNumberIsNonCompliant;
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_Voyage = "Q";
			AssertHasMessageErrorContaining(header.AMA_VoyageInfo, flightNumberIsWrongLengthError);
			header.AMA_CarrierCode = "Q1";
			header.AMA_Voyage = "Q123"; // Q1 is an indonesian airline carrier code, 23 will be converted to 023 in message.
			AssertNoMessageErrorContaining(header.AMA_VoyageInfo, flightNumberIsWrongLengthError);
			AssertNoMessageErrorContaining(header.AMA_VoyageInfo, flightNumberIsNonCompliantError);
			header.AMA_CarrierCode = "QF";
			header.AMA_Voyage = "QF";
			AssertHasMessageErrorContaining(header.AMA_VoyageInfo, flightNumberIsWrongLengthError);
			header.AMA_Voyage = "QF1";
			AssertNoMessageErrorContaining(header.AMA_VoyageInfo, flightNumberIsWrongLengthError);
			AssertNoMessageErrorContaining(header.AMA_VoyageInfo, flightNumberIsNonCompliantError);
			header.AMA_Voyage = "QF01";
			AssertNoMessageErrorContaining(header.AMA_VoyageInfo, flightNumberIsNonCompliantError);
			header.AMA_CarrierCode = "";
			header.AMA_Voyage = "QF001";
			AssertNoMessageErrorContaining(header.AMA_VoyageInfo, flightNumberIsNonCompliantError);
			header.AMA_Voyage = "081001";
			AssertNoMessageErrorContaining(header.AMA_VoyageInfo, flightNumberIsNonCompliantError);
			header.AMA_Voyage = "08101";
			AssertNoMessageErrorContaining(header.AMA_VoyageInfo, flightNumberIsNonCompliantError);
			header.AMA_Voyage = "0811";
			AssertHasMessageErrorContaining(header.AMA_VoyageInfo, flightNumberIsNonCompliantError);
			header.AMA_Voyage = "081";
			AssertHasMessageErrorContaining(header.AMA_VoyageInfo, flightNumberIsNonCompliantError);
			header.AMA_CarrierCode = "081";
			header.AMA_Voyage = "08101";
			AssertNoMessageErrorContaining(header.AMA_VoyageInfo, flightNumberIsNonCompliantError);
			header.AMA_Voyage = "0811";
			AssertNoMessageErrorContaining(header.AMA_VoyageInfo, flightNumberIsNonCompliantError);
			header.AMA_Voyage = "081";
			AssertHasMessageErrorContaining(header.AMA_VoyageInfo, flightNumberIsNonCompliantError);
			header.AMA_CarrierCode = "QUAN";
			header.AMA_Voyage = "QUAN01";
			AssertHasMessageErrorContaining(header.AMA_VoyageInfo, flightNumberIsNonCompliantError);
		}

		public void TestCheckAMA_OA_DeconsolidateAddress()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Validation.ValidateAMA_OA_DeconsolidateAddress();
			AssertNoNotifications(header.AMA_OA_DeconsolidateAddressInfo);

			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "Test1";
			var orgAddress1 = orgHeader1.Addresses.AddNew();
			orgAddress1.OA_Address1 = "Test Address 1";
			header.AMA_OA_DeconsolidateAddress = orgAddress1.PK;
			AssertHasMessageError(header.AMA_OA_DeconsolidateAddressInfo, ValidationConstants.AirAMSOriginatorCodeRequired);

			var orgAddress2 = orgHeader1.Addresses.AddNew();
			orgAddress2.OA_Address1 = "Test Address 2";
			header.AMA_OA_DeconsolidateAddress_ZAddress.IsOrgVisible = true;
			header.AMA_OA_DeconsolidateAddress_ZAddress.OrgPK = ZGuid.Empty;
			header.AMA_OA_DeconsolidateAddress_ZAddress.OrgPK = orgHeader1.PK;
			AssertHasErrorContaining(header.AMA_OA_DeconsolidateAddressInfo, ListValidation.InvalidCodeError);

			orgAddress2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.AirAMSOriginatorCode, "TEST123", Core.Constants.CountryCodes.UnitedStates);
			header.AMA_OA_DeconsolidateAddress_ZAddress.OrgPK = ZGuid.Empty;
			header.AMA_OA_DeconsolidateAddress_ZAddress.OrgPK = orgHeader1.PK;
			AssertNoNotifications(header.AMA_OA_DeconsolidateAddressInfo);
		}
	}
}
