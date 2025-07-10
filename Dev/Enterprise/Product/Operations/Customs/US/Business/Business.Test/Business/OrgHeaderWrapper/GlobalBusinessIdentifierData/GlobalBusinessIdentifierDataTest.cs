using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(GlobalBusinessIdentifierData))]
	public sealed class GlobalBusinessIdentifierDataTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPropertyReadonly()
		{
			AssertPropertyReadOnly(AutoGlobalBusinessIdentifierData.Schema.US_OA_AddressDetails, false);

			AssertPropertyReadOnly(AutoGlobalBusinessIdentifierData.Schema.US_FirmName, true);
			AssertPropertyReadOnly(AutoGlobalBusinessIdentifierData.Schema.US_Address1, true);
			AssertPropertyReadOnly(AutoGlobalBusinessIdentifierData.Schema.US_Address2, true);
			AssertPropertyReadOnly(AutoGlobalBusinessIdentifierData.Schema.US_Country, true);
			AssertPropertyReadOnly(AutoGlobalBusinessIdentifierData.Schema.US_City, true);
			AssertPropertyReadOnly(AutoGlobalBusinessIdentifierData.Schema.US_PostCode, true);
			AssertPropertyReadOnly(AutoGlobalBusinessIdentifierData.Schema.US_City, true);
			AssertPropertyReadOnly(AutoGlobalBusinessIdentifierData.Schema.US_State, true);
			AssertPropertyReadOnly(AutoGlobalBusinessIdentifierData.Schema.US_WebsiteURL, true);
			AssertPropertyReadOnly(AutoGlobalBusinessIdentifierData.Schema.US_DUNS, false);
			AssertPropertyReadOnly(AutoGlobalBusinessIdentifierData.Schema.US_LEI, false);
			AssertPropertyReadOnly(AutoGlobalBusinessIdentifierData.Schema.US_GLN, false);
			AssertPropertyReadOnly(AutoGlobalBusinessIdentifierData.Schema.US_IsManufacturer, false);
			AssertPropertyReadOnly(AutoGlobalBusinessIdentifierData.Schema.US_IsShipper, false);
			AssertPropertyReadOnly(AutoGlobalBusinessIdentifierData.Schema.US_IsSeller, false);
			AssertPropertyReadOnly(AutoGlobalBusinessIdentifierData.Schema.US_IsExporter, false);
			AssertPropertyReadOnly(AutoGlobalBusinessIdentifierData.Schema.US_IsPackager, false);
			AssertPropertyReadOnly(AutoGlobalBusinessIdentifierData.Schema.US_IsDistributor, false);
		}

		void AssertPropertyReadOnly(string propertyName, bool shouldBeReadOnly)
		{
			System.ComponentModel.ReadOnlyAttribute[] attributes = (System.ComponentModel.ReadOnlyAttribute[])typeof(AutoGlobalBusinessIdentifierData).GetProperty(propertyName).GetCustomAttributes(typeof(System.ComponentModel.ReadOnlyAttribute), false);
			if (shouldBeReadOnly)
			{
				AssertNotEquals(0, attributes.Length);
				AssertEquals(true, attributes[0].IsReadOnly);
			}
			else
			{
				AssertEquals(0, attributes.Length);
			}
		}

		public void TestSetDefaultValuesWithWrapper()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "ABC Company";
			organisation.OH_IsConsignor = true;
			organisation.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "12345678", Core.Constants.CountryCodes.UnitedStates);

			var mainAddress = organisation.MainAddress;
			mainAddress.OA_Address1 = "978 Main Road";
			mainAddress.OA_Address2 = "(back of Eat me restaurant)";
			mainAddress.OA_City = "Vancouver";
			mainAddress.OA_State = "AB";
			mainAddress.OA_PostCode = "1B1 A3B";
			mainAddress.OA_Phone = "+0123456";
			mainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			mainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "00000001", Core.Constants.CountryCodes.UnitedStates);
			mainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.GlobalLocationNumber, "00000002", Core.Constants.CountryCodes.UnitedStates);
			mainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.LegalEntityIdentifier, "00000003", Core.Constants.CountryCodes.UnitedStates);

			var secondAddress = organisation.Addresses.AddNew();
			secondAddress.OA_CompanyNameOverride = "Override Company";
			secondAddress.OA_Address1 = "123 Second St";
			secondAddress.OA_City = "HH";
			secondAddress.OA_State = "Hamburg";
			secondAddress.OA_PostCode = "10001";
			secondAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			secondAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ManufacturerID, "00000004", Core.Constants.CountryCodes.UnitedStates);

			var wrapper = OrgHeaderWrapper.New(organisation);
			var messageData = new GlobalBusinessIdentifierData(wrapper);
			AssertEquals(mainAddress.PK, messageData.AddressDetails.PK);
			AssertEquals("ABC Company", messageData.US_FirmName);
			AssertEquals("978 Main Road", messageData.US_Address1);
			AssertEquals("(back of Eat me restaurant)", messageData.US_Address2);
			AssertEquals("Vancouver", messageData.US_City);
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, messageData.US_Country);
			AssertEquals("postCode", "1B1 A3B", messageData.US_PostCode);
			AssertEquals("0123456", messageData.US_Phone);
			AssertEquals("00000001", messageData.US_DUNS);
			AssertEquals("00000002", messageData.US_GLN);
			AssertEquals("00000003", messageData.US_LEI);
			AssertEquals(true, messageData.US_IsShipper);
			AssertEquals(false, messageData.US_IsManufacturer);
			AssertEquals(false, messageData.US_IsPackager);
			AssertEquals(false, messageData.US_IsSeller);
			AssertEquals(false, messageData.US_IsDistributor);
			AssertEquals(false, messageData.US_IsExporter);
			AssertEquals(ZString.Empty, messageData.ManufacturerID);
			AssertEquals("12345678", messageData.AuthorisedEconomicOperator);

			messageData.US_OA_AddressDetails = secondAddress.PK;
			AssertEquals(secondAddress.PK, messageData.AddressDetails.PK);
			AssertEquals("Override Company", messageData.US_FirmName);
			AssertEquals("123 Second St", messageData.US_Address1);
			AssertEquals(string.Empty, messageData.US_Address2);
			AssertEquals("HH", messageData.US_City);
			AssertEquals(Core.Constants.CountryCodes.Germany, messageData.US_Country);
			AssertEquals("10001", messageData.US_PostCode);
			AssertEquals(string.Empty, messageData.US_DUNS);
			AssertEquals(string.Empty, messageData.US_GLN);
			AssertEquals(string.Empty, messageData.US_LEI);
			AssertEquals(true, messageData.US_IsShipper);
			AssertEquals(true, messageData.US_IsManufacturer);
			AssertEquals(false, messageData.US_IsPackager);
			AssertEquals(false, messageData.US_IsSeller);
			AssertEquals(false, messageData.US_IsDistributor);
			AssertEquals(false, messageData.US_IsExporter);
			AssertEquals("00000004", messageData.ManufacturerID);
			AssertEquals("12345678", messageData.AuthorisedEconomicOperator);
		}

		public void TestSaveGlobalBusinessIdentifiers()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "ABC Company";

			var secondAddress = organisation.Addresses.AddNew();
			var wrapper = OrgHeaderWrapper.New(organisation);
			var messageData = new GlobalBusinessIdentifierData(wrapper);
			AssertEquals(organisation.MainAddress.PK, messageData.AddressDetails.PK);
			messageData.US_OA_AddressDetails = secondAddress.PK;
			AssertEquals(secondAddress.PK, messageData.AddressDetails.PK);

			messageData.US_DUNS = "00000001";
			messageData.US_GLN = "00000002";
			messageData.US_LEI = "00000003";

			messageData.SaveGlobalBusinessIdentifiers();
			AssertEquals(3, organisation.CustomsCodes.Count);
			AssertEquals(0, organisation.MainAddress.CustomsCodes.Count);
			AssertEquals(3, secondAddress.CustomsCodes.Count);
			AssertEquals("00000001", secondAddress.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, Core.Constants.CountryCodes.UnitedStates));
			AssertEquals("00000002", secondAddress.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.GlobalLocationNumber, Core.Constants.CountryCodes.UnitedStates));
			AssertEquals("00000003", secondAddress.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.LegalEntityIdentifier, Core.Constants.CountryCodes.UnitedStates));

			secondAddress.CustomsCodes.DeleteAll();
			organisation.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "00000001", Core.Constants.CountryCodes.UnitedStates);
			organisation.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.GlobalLocationNumber, "00000002", Core.Constants.CountryCodes.UnitedStates);

			messageData = new GlobalBusinessIdentifierData(wrapper);
			messageData.US_OA_AddressDetails = secondAddress.PK;
			AssertEquals("00000001", messageData.US_DUNS);
			AssertEquals(ZString.Empty, messageData.US_GLN);

			messageData.US_GLN = "00000002";
			messageData.US_LEI = "00000003";
			messageData.SaveGlobalBusinessIdentifiers();
			AssertEquals(4, organisation.CustomsCodes.Count);
			AssertEquals(1, organisation.MainAddress.CustomsCodes.Count);
			AssertEquals(2, secondAddress.CustomsCodes.Count);
			AssertEquals("00000002", secondAddress.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.GlobalLocationNumber, Core.Constants.CountryCodes.UnitedStates));
			AssertEquals("00000003", secondAddress.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.LegalEntityIdentifier, Core.Constants.CountryCodes.UnitedStates));
		}

		public void TestIMessageAttachee()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "ABC Company";
			organisation.FillWithValidTestData();
			organisation.CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;

			var wrapper = OrgHeaderWrapper.New(organisation);
			var attachee = new GlobalBusinessIdentifierData(wrapper) as IMessageAttachee;
			AssertEquals(GlbBranch.CurrentBranch.PK, attachee.Branch.PK);
			AssertEquals(ZString.Empty, attachee.TopLevelBizObjReferenceNumber);
			AssertEquals(ControllerIDs.Organisation, attachee.ControllerID);
			AssertEquals(organisation.PK.ToGuid(), attachee.BusinessObjectPK);
			AssertEquals(organisation.Logs.PK, attachee.TopLevelBusinessObjectLogs.PK);
			AssertSame(wrapper.Messages, attachee.Messages);
			AssertSame(organisation.Factory, attachee.Factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();
			OrgHeaderWrapper wrapper = OrgHeaderWrapper.New(organisation);
			return new GlobalBusinessIdentifierData(wrapper);
		}
	}
}
