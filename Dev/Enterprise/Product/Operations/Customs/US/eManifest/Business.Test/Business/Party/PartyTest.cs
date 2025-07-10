using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	[TestedType(typeof(Party))]
	sealed class PartyTest : EnterpriseBusinessObjectTestCase
	{
		public void TestABIRoutingCode()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = org.MainAddress;
			var address2 = org.Addresses.AddNew();
			address2.FillWithValidTestData();
			org.CustomsCodes.AddNew(PartyIdTypes.Codes.FilerCode, "0000001", Constants.CountryCodes.UnitedStates);
			org.CustomsCodes.AddNew(PartyIdTypes.Codes.ABIRoutingCode, "0000002", Constants.CountryCodes.UnitedStates).OK_OA_PremisesAddress = address1.PK;
			org.CustomsCodes.AddNew(PartyIdTypes.Codes.ABIRoutingCode, "0000003", Constants.CountryCodes.UnitedStates).OK_OA_PremisesAddress = address2.PK;
			var party = (Party)GetNewBusinessObject();
			party.E2_AddressType = PartyTypes.Codes.NotifyParty;
			party.OrganisationPK = org.PK;
			AssertEquals("NotifyParty.ABIRoutingCode", ZString.Empty, party.ABIRoutingCode);
			party.E2_AddressType = PartyTypes.Codes.CustomsBroker;
			AssertEquals("CustomsBroker.ABIRoutingCode", "0000002", party.ABIRoutingCode);
			party.E2_AddressType = PartyTypes.Codes.Importer;
			party.E2_OA_Address = address2.PK;
			AssertEquals("Importer.ABIRoutingCode", "0000003", party.ABIRoutingCode);
			org.CustomsCodes[0].OK_CodeType = PartyIdTypes.Codes.ACE;
			AssertEquals("Importer.ABIRoutingCode when no FilerCode", ZString.Empty, party.ABIRoutingCode);
		}

		public void TestRegNumber()
		{
			AssertNumbersWithRelevance(
				PartyTypes.Codes.IntermediateCarrier,
				OrgCusCode.USACodeTypes.ACEAssignedNumber,
				OrgCusCode.USACodeTypes.FreeAndSecureTradeCode,
				OrgCusCode.USACodeTypes.FIRMSCode,
				OrgCusCode.USACodeTypes.EntryFilerCode,
				OrgCusCode.CodeTypes.CarrierCode,
				OrgCusCode.CodeTypes.DataUniversalNumberingSystem,
				OrgCusCode.USACodeTypes.EmployerIdentificationNumber,
				OrgCusCode.USACodeTypes.SocialSecurityNumber,
				OrgCusCode.USACodeTypes.CBPAssignedNumber);

			AssertNumbersWithRelevance(
				PartyTypes.Codes.NotifyParty,
				OrgCusCode.USACodeTypes.FIRMSCode,
				OrgCusCode.USACodeTypes.EntryFilerCode,
				OrgCusCode.USACodeTypes.ACEAssignedNumber,
				OrgCusCode.USACodeTypes.FreeAndSecureTradeCode,
				OrgCusCode.CodeTypes.CarrierCode,
				OrgCusCode.CodeTypes.DataUniversalNumberingSystem,
				OrgCusCode.USACodeTypes.EmployerIdentificationNumber,
				OrgCusCode.USACodeTypes.SocialSecurityNumber,
				OrgCusCode.USACodeTypes.CBPAssignedNumber);

			AssertNumbersWithRelevance(
				PartyTypes.Codes.Importer,
				OrgCusCode.USACodeTypes.EntryFilerCode,
				OrgCusCode.USACodeTypes.ACEAssignedNumber,
				OrgCusCode.USACodeTypes.FreeAndSecureTradeCode,
				OrgCusCode.USACodeTypes.FIRMSCode,
				OrgCusCode.CodeTypes.CarrierCode,
				OrgCusCode.CodeTypes.DataUniversalNumberingSystem,
				OrgCusCode.USACodeTypes.EmployerIdentificationNumber,
				OrgCusCode.USACodeTypes.SocialSecurityNumber,
				OrgCusCode.USACodeTypes.CBPAssignedNumber);

			AssertNumbersWithRelevance(
				PartyTypes.Codes.CustomsBroker,
				OrgCusCode.USACodeTypes.EntryFilerCode,
				OrgCusCode.USACodeTypes.ACEAssignedNumber,
				OrgCusCode.USACodeTypes.FreeAndSecureTradeCode,
				OrgCusCode.USACodeTypes.FIRMSCode,
				OrgCusCode.CodeTypes.CarrierCode,
				OrgCusCode.CodeTypes.DataUniversalNumberingSystem,
				OrgCusCode.USACodeTypes.EmployerIdentificationNumber,
				OrgCusCode.USACodeTypes.SocialSecurityNumber,
				OrgCusCode.USACodeTypes.CBPAssignedNumber);
		}

		public void TestDefaultRegNumberType()
		{
			var party = (Party)GetNewBusinessObject();
			AssertEquals("", party.E2_GovRegNumType);
			party.Validation.ValidateE2_GovRegNumType();
			AssertEquals(false, party.E2_GovRegNumTypeInfo.HasNotifications());
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<Trip>().Shipments.AddNew().Parties.AddNew();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var party = factory.New<Trip>().Shipments.AddNew().Parties.AddNew();
			party.E2_AddressOverride = true;
			return party;
		}

		void AssertNumbersWithRelevance(string partyType, params string[] types)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			for (var i = 0; i < types.Length; i++)
			{
				org.CustomsCodes.AddNew(types[i], "000000" + (i + 1), Constants.CountryCodes.UnitedStates);
			}

			var party = (Party)GetNewBusinessObject();
			party.E2_AddressType = partyType;
			party.OrganisationPK = org.PK;
			for (var i = 0; i < types.Length; i++)
			{
				AssertRegNumber(party, types[i], "000000" + (i + 1));
				org.CustomsCodes[0].Delete();
			}

			AssertRegNumber(party, string.Empty, string.Empty);
		}

		void AssertRegNumber(Party party, string type, string number)
		{
			AssertEquals("E2_GovRegNumType", type, party.E2_GovRegNumType);
			AssertEquals("E2_GovRegNum", number, party.E2_GovRegNum);
		}
	}
}
