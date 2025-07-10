using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	public class GlobalBusinessIdentifierDataMessageImporterTest : TestCaseWithFactory
	{
		public void TestImportDataFromMessage()
		{
			var organisation = Factory.New<OrgHeader>();
			var mainAddress = organisation.MainAddress;
			mainAddress.OA_Address1 = "TEST FIRST ADDRESS";
			mainAddress.OA_City = "CHICAGO";
			mainAddress.OA_State = "IL";
			mainAddress.OA_PostCode = "60091";
			mainAddress.OA_RN_NKCountryCode = "US";
			mainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "10000001", Core.Constants.CountryCodes.UnitedStates);
			mainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.GlobalLocationNumber, "10000002", Core.Constants.CountryCodes.UnitedStates);
			mainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.LegalEntityIdentifier, "10000003", Core.Constants.CountryCodes.UnitedStates);

			var address2 = organisation.Addresses.AddNew();
			address2.OA_Address1 = "TEST SECOND ADDRESS";
			address2.OA_Address2 = "SECONDARY ADDRESS";
			address2.OA_City = "LONDON";
			address2.OA_PostCode = "1234";
			address2.OA_RN_NKCountryCode = "GB";
			address2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "20000001", Core.Constants.CountryCodes.UnitedStates);
			address2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.GlobalLocationNumber, "20000002", Core.Constants.CountryCodes.UnitedStates);
			address2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.LegalEntityIdentifier, "20000003", Core.Constants.CountryCodes.UnitedStates);
			address2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ManufacturerID, "20000004", Core.Constants.CountryCodes.UnitedStates);

			var wrapper = OrgHeaderWrapper.New(organisation);
			var messageData = new GlobalBusinessIdentifierData(wrapper);
			messageData.US_OA_AddressDetails = address2.PK;
			messageData.US_IsExporter = true;
			messageData.US_IsDistributor = true;
			var messageBuilder = new GlobalBusinessIdentifierMessageBuilder(messageData, GlobalBusinessIdentifierMessageType.Original);
			messageBuilder.Generate();

			messageData = new GlobalBusinessIdentifierData(wrapper, true);
			AssertEquals(address2.PK, messageData.US_OA_AddressDetails);
			AssertEquals(true, messageData.US_IsManufacturer);
			AssertEquals(true, messageData.US_IsExporter);
			AssertEquals(true, messageData.US_IsDistributor);
		}
	}
}
