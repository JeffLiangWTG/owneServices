using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.AMS.Messaging.Interface;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class JobDocAddressPartyTestCase : TestCaseWithFactory
	{
		public void TestIInterfaceMembers()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var docAddress = bill.ForeignShipper;
			docAddress.E2_AddressOverride = true;
			docAddress.E2_CompanyName = "*BOB* THE BUILDER LTD".PadRight(JobDocAddress.Schema.E2_CompanyNameMaxLength, '1');
			docAddress.E2_Address1 = "ADDRESS LINE *1*".PadRight(JobDocAddress.Schema.E2_Address1MaxLength, '2');
			docAddress.E2_Address2 = "ADDRESS LINE *2*".PadRight(JobDocAddress.Schema.E2_Address2MaxLength, '3');
			docAddress.E2_City = "*SYDNEY*".PadRight(JobDocAddress.Schema.E2_CityMaxLength, '4');
			docAddress.E2_State = "NSW*".PadRight(JobDocAddress.Schema.E2_StateMaxLength, '5');
			docAddress.E2_Postcode = "*2100*".PadRight(JobDocAddress.Schema.E2_PostcodeMaxLength, '6');
			docAddress.E2_RN_NKCountryCode = "AU";
			docAddress.E2_Phone = "23493432432".PadRight(JobDocAddress.Schema.E2_PhoneMaxLength, '7');
			docAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.CarrierCode;
			docAddress.E2_GovRegNum = "SK34";
			var address = new JobDocAddressParty(docAddress, EntityIDCodeList.Codes.Shipper);
			IParty party = address;
			AssertEquals(" BOB  THE BUILDER LTD".PadRight(35, '1'), party.Name);
			AssertEquals("ADDRESS LINE  1 2222222222222222222", party.AddressLine1);
			AssertEquals("222222222222222 ADDRESS LINE  2 333", party.AddressLine2);
			AssertEquals("23493432432777777777", party.TelephoneOrTelexNumberOrAddressLine4);
			IEntity entity = address;
			AssertEquals(EntityIDCodeList.Codes.Shipper, entity.EntityCode);
			AssertEquals(" BOB  THE BUILDER LTD".PadRight(35, '1'), entity.EntityName);
			AssertEquals(CusInBondMoveDetail.SCACOrFIRMSOfSNPQualifier, entity.CodeQualifier);
			AssertEquals("SK34", entity.IDCode);
			AssertEquals("ADDRESS LINE  1 ".PadRight(35, '2'), entity.AddressLine1);
			AssertEquals("".PadRight(15, '2'), entity.AddressLine1Part2);
			AssertEquals("ADDRESS LINE  2 ".PadRight(35, '3'), entity.AddressLine2);
			AssertEquals("".PadRight(15, '3'), entity.AddressLine2Part2);
			AssertEquals(" SYDNEY ".PadRight(19, '4'), entity.CityName);
			AssertEquals("", entity.StateProvince);
			AssertEquals("", entity.PostalCode);
			AssertEquals("AU", entity.CountryCode);
			docAddress.E2_Address1 = "";
			address = new JobDocAddressParty(docAddress, EntityIDCodeList.Codes.Consignee);
			party = address;
			AssertEquals(" BOB  THE BUILDER LTD".PadRight(35, '1'), party.Name);
			AssertEquals("ADDRESS LINE  2 3333333333333333333", party.AddressLine1);
			AssertEquals("333333333333333  SYDNEY 44444444444", party.AddressLine2);
			AssertEquals("23493432432777777777", party.TelephoneOrTelexNumberOrAddressLine4);
			entity = address;
			AssertEquals(EntityIDCodeList.Codes.Consignee, entity.EntityCode);
			AssertEquals(" BOB  THE BUILDER LTD".PadRight(35, '1'), entity.EntityName);
			AssertEquals(CusInBondMoveDetail.SCACOrFIRMSOfSNPQualifier, entity.CodeQualifier);
			AssertEquals("SK34", entity.IDCode);
			AssertEquals("", entity.AddressLine1);
			AssertEquals("", entity.AddressLine1Part2);
			AssertEquals("ADDRESS LINE  2 ".PadRight(35, '3'), entity.AddressLine2);
			AssertEquals("".PadRight(15, '3'), entity.AddressLine2Part2);
			AssertEquals(" SYDNEY ".PadRight(19, '4'), entity.CityName);
			AssertEquals("", entity.StateProvince);
			AssertEquals("", entity.PostalCode);
			AssertEquals("AU", entity.CountryCode);
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "WENDY THE DESTROYER".PadRight(OrgHeader.Schema.OH_FullNameMaxLength, '1');
			org.OH_RL_NKClosestPort = "USLAX";
			org.MainAddress.OA_Address1 = "ADDRESS 1".PadRight(OrgAddress.Schema.OA_Address1MaxLength, '2');
			org.MainAddress.OA_Address2 = "ADDRESS 2".PadRight(OrgAddress.Schema.OA_Address2MaxLength, '3');
			org.MainAddress.OA_City = "LOS ANGELES".PadRight(OrgAddress.Schema.OA_CityMaxLength, '4');
			org.MainAddress.OA_PostCode = "45663".PadRight(OrgAddress.Schema.OA_PostCodeMaxLength, '5');
			org.MainAddress.OA_State = "CA".PadRight(OrgAddress.Schema.OA_StateMaxLength, '6');
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "KDTE", Core.Constants.CountryCodes.UnitedStates);
			docAddress.E2_OA_Address = org.MainAddress.PK;
			address = new JobDocAddressParty(docAddress, EntityIDCodeList.Codes.NotifyParty1);
			party = address;
			AssertEquals("WENDY THE DESTROYER".PadRight(35, '1'), party.Name);
			AssertEquals("ADDRESS 122222222222222222222222222", party.AddressLine1);
			AssertEquals("222222222222222 ADDRESS 23333333333", party.AddressLine2);
			AssertEquals("3333333333333333333333333333333 LOS", party.AddressLine3);
			AssertEquals(" ANGELES444444444444444444444444444", party.TelephoneOrTelexNumberOrAddressLine4);
			entity = address;
			AssertEquals(EntityIDCodeList.Codes.NotifyParty1, entity.EntityCode);
			AssertEquals("WENDY THE DESTROYER".PadRight(35, '1'), entity.EntityName);
			AssertEquals(CusInBondMoveDetail.SCACOrFIRMSOfSNPQualifier, entity.CodeQualifier);
			AssertEquals("KDTE", entity.IDCode);
			AssertEquals("ADDRESS 1".PadRight(35, '2'), entity.AddressLine1);
			AssertEquals("".PadRight(15, '2'), entity.AddressLine1Part2);
			AssertEquals("ADDRESS 2".PadRight(35, '3'), entity.AddressLine2);
			AssertEquals("".PadRight(15, '3'), entity.AddressLine2Part2);
			AssertEquals("LOS ANGELES".PadRight(19, '4'), entity.CityName);
			AssertEquals("CA", entity.StateProvince);
			AssertEquals("456635555", entity.PostalCode);
			AssertEquals("US", entity.CountryCode);
			docAddress.E2_AddressOverride = true;
			docAddress.E2_Phone = ZString.Empty;
			docAddress.E2_Email = "test".PadRight(28, 'A');
			address = new JobDocAddressParty(docAddress, EntityIDCodeList.Codes.Shipper);
			entity = address;
			AssertEquals(ZString.Empty, entity.AdminContact.CommNumberQualifier);
			AssertEquals(ZString.Empty, entity.AdminContact.CommunicationsNumber);
			AssertEquals(ZString.Empty, entity.AdminContact.CommNumberQualifier2);
			AssertEquals(ZString.Empty, entity.AdminContact.CommunicationsNumber2);
			docAddress.E2_Email = "test@email.com";
			address = new JobDocAddressParty(docAddress, EntityIDCodeList.Codes.Shipper);
			entity = address;
			AssertEquals(CommunicationsNumberQualifierList.Codes.ElectronicMail, entity.AdminContact.CommNumberQualifier);
			AssertEquals("TEST@EMAIL.COM", entity.AdminContact.CommunicationsNumber);
			AssertEquals(ZString.Empty, entity.AdminContact.CommNumberQualifier2);
			AssertEquals(ZString.Empty, entity.AdminContact.CommunicationsNumber2);
		}

		public void TestABIRouting()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var docAddress = bill.ForeignShipper;
			var orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "USCHI"));
			orgHeader.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ABIRoutingCode, "KDTE", Core.Constants.CountryCodes.UnitedStates);
			docAddress.E2_OA_Address = orgHeader.MainAddress.PK;
			var address = new JobDocAddressParty(docAddress, EntityIDCodeList.Codes.CustomsBroker);
			AssertEquals("should have been retrieved from OrgAddress.CustomsCodes", "KDTE", ((IEntity)address).IDCode);
		}
	}
}
