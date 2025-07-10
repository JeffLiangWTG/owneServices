using System;
using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class RedefaultRelatedPartiesTests : TestCaseWithFactory
	{
		public void TestGetCorrectReasons_AgentConsol()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Maersk";
			carrier.OH_IsCreditor = false;

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = ZGuid.Empty;

			Factory.Save();

			Assert("Consol is Export", consol.IsExport());

			var reasons = RelatedPartiesHelper.TryRedefaultCreditor(consol);
			AssertContainsExactLinesInAnyOrder("• Shipping Line's address is not valid.", reasons);

			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			Factory.Save();
			reasons = RelatedPartiesHelper.TryRedefaultCreditor(consol);
			Assert("There should not be any error as Creditor has value", reasons.IsEmpty);

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			Factory.Save();
			reasons = RelatedPartiesHelper.TryRedefaultCreditor(consol);
			Assert("Unable to set Creditor from Carrier because Carrier is not an AP Org", consol.JK_OA_CreditorAddress.IsEmpty);
			AssertContainsExactLinesInAnyOrder("• Unable to default Export Creditor Address of Organization <Maersk> as no matching Related Parties with type of 'SPC' found or the Related Party is NOT an AP Organization.", reasons);

			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			Assert("Consol is Import", consol.IsImport());
			reasons = RelatedPartiesHelper.TryRedefaultCreditor(consol);
			Assert("Unable to set Creditor from Carrier because Carrier is not an AP Org", consol.JK_OA_CreditorAddress.IsEmpty);
			AssertContainsExactLinesInAnyOrder("• Unable to default Import Creditor Address of Organization <Maersk> as no matching Related Parties with type of 'SPC' found or the Related Party is NOT an AP Organization.", reasons);

			carrier.OH_IsCreditor = true;
			Factory.Save();
			reasons = RelatedPartiesHelper.TryRedefaultCreditor(consol);
			AssertEquals("Creditor is correctly set", carrier.MainAddress.PK, consol.JK_OA_CreditorAddress);
			Assert("There should not be any error as Creditor has value", reasons.IsEmpty);

			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			relatedParty.OH_IsCreditor = true;

			carrier.SetRelatedParty(relatedParty
				, RelatedPartyTypeList.Codes.ServiceProviderCreditor
				, RelatedPartyDirectionList.Codes.PickupAndDelivery);

			Factory.Save();

			reasons = RelatedPartiesHelper.TryRedefaultCreditor(consol);
			AssertEquals("Creditor is correctly set", relatedParty.MainAddress.PK, consol.JK_OA_CreditorAddress);
			Assert("There should not be any error as Creditor has value", reasons.IsEmpty);

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			Assert("Consol is Export", consol.IsExport());
			reasons = RelatedPartiesHelper.TryRedefaultCreditor(consol);
			AssertEquals("Creditor is correctly set", relatedParty.MainAddress.PK, consol.JK_OA_CreditorAddress);
			Assert("There should not be any error as Creditor has value", reasons.IsEmpty);
		}

		public void TestGetCorrectReasons_CoLoadConsol()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Maersk";
			carrier.OH_IsCreditor = true;

			var coloadWith = Factory.NewWithValidTestData<OrgHeader>();
			coloadWith.OH_Code = "Rohling";
			coloadWith.OH_IsCreditor = false;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = ZGuid.Empty;

			Factory.Save();

			Assert("Consol is Export", consol.IsExport());

			var reasons = RelatedPartiesHelper.TryRedefaultCreditor(consol);
			AssertContainsExactLinesInAnyOrder("• Co-Load's address is not valid.", reasons);

			consol.JK_OA_CreditorAddress = coloadWith.MainAddress.PK;
			reasons = RelatedPartiesHelper.TryRedefaultCreditor(consol);
			AssertContainsExactLinesInAnyOrder("• Unable to default Export Creditor Address of Organization <Rohling> as no matching Related Parties with type of 'SPC' found or the Related Party is NOT an AP Organization.", reasons);

			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			Assert("Consol is Import", consol.IsImport());
			reasons = RelatedPartiesHelper.TryRedefaultCreditor(consol);
			AssertContainsExactLinesInAnyOrder("• Unable to default Import Creditor Address of Organization <Rohling> as no matching Related Parties with type of 'SPC' found or the Related Party is NOT an AP Organization.", reasons);

			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			relatedParty.OH_IsCreditor = true;

			coloadWith.SetRelatedParty(relatedParty
				, RelatedPartyTypeList.Codes.ServiceProviderCreditor
				, RelatedPartyDirectionList.Codes.PickupAndDelivery);

			Factory.Save();

			reasons = RelatedPartiesHelper.TryRedefaultCreditor(consol);
			Assert("Creditor is correctly set", consol.CarrierImportCreditorAddress.IsValidAddress);
			Assert("There should not be any error as Creditor has value", reasons.IsEmpty);

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			Assert("Consol is Export", consol.IsExport());
			reasons = RelatedPartiesHelper.TryRedefaultCreditor(consol);
			Assert("Creditor is correctly set", consol.CarrierExportCreditorAddress.IsValidAddress);
			Assert("There should not be any error as Creditor has value", reasons.IsEmpty);
		}

		#region RecalculateRelatedParties

		public void TestRecalculateRelatedParties_ShouldSetDefaultValueForPickupDeliveryProperties()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Namibia))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "CNSHA";

				var result = RelatedPartiesHelper.RecalculateRelatedParties(consol);
				Assert("When login company is not origin nor destination", !result.WasSuccessful);
				AssertEquals("When login company is not origin nor destination", "You cannot Recalculate Related Parties for this company because the company does not match Pickup or Delivery direction of this job.", result.Log);

				consol.JK_RL_NKLoadPort = "NASWP";

				consol.AttemptToUpdateExistingValueInRecalculation += Shipment_AttempToUpdateExistingValueInRecalculation;

				AssertProperty(consol,
					value => consol.SendingForwarderWithContact.OrgPK = value,
					RelatedPartyTypeList.Codes.ForwarderCFS,
					RelatedPartyDirectionList.Codes.Forwarder,
					"Departure CFS Address",
					() => consol.JK_OA_PackDepotAddress_ZAddress.OrgPK,
					value => consol.JK_OA_PackDepotAddress_ZAddress.OrgPK = value);

				consol.JK_OA_PackDepotAddress_ZAddress.OrgPK = ZGuid.Empty;

				AssertProperty(consol,
					value => consol.SendingForwarderWithContact.OrgPK = value,
					RelatedPartyTypeList.Codes.ForwarderLocalTransport,
					RelatedPartyDirectionList.Codes.Forwarder,
					"Departure Port Transport",
					() => consol.JK_OA_DeparturePackCFSTransportAddress_ZAddress.OrgPK,
					value => consol.JK_OA_DeparturePackCFSTransportAddress_ZAddress.OrgPK = value);

				consol.JK_OA_DeparturePackCFSTransportAddress_ZAddress.OrgPK = ZGuid.Empty;
				consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
				AssertProperty(consol,
					value => consol.SendingForwarderWithContact.OrgPK = value,
					RelatedPartyTypeList.Codes.ForwarderCoLoadWith,
					RelatedPartyDirectionList.Codes.Pickup,
					"Co-Load With",
					() => consol.JK_OA_CreditorAddress_ZAddress.OrgPK,
					value => consol.JK_OA_CreditorAddress_ZAddress.OrgPK = value);

				consol.JK_AgentType = Core.Constants.AgentType.Agent;
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = ZGuid.Empty;

				consol.JK_RL_NKLoadPort = "CNSHA";
				consol.JK_RL_NKDischargePort = "NASWP";

				AssertProperty(consol,
					value => consol.ReceivingForwarderWithContact.OrgPK = value,
					RelatedPartyTypeList.Codes.ForwarderCFS,
					RelatedPartyDirectionList.Codes.Forwarder,
					"Arrival CFS Address",
					() => consol.JK_OA_UnpackDepotAddress_ZAddress.OrgPK,
					value => consol.JK_OA_UnpackDepotAddress_ZAddress.OrgPK = value);

				consol.JK_OA_UnpackDepotAddress_ZAddress.OrgPK = ZGuid.Empty;

				AssertProperty(consol,
					value => consol.ReceivingForwarderWithContact.OrgPK = value,
					RelatedPartyTypeList.Codes.ForwarderLocalTransport,
					RelatedPartyDirectionList.Codes.Forwarder,
					"Arrival Port Transport",
					() => consol.JK_OA_ArrivalUnpackCFSTransportAddress_ZAddress.OrgPK,
					value => consol.JK_OA_ArrivalUnpackCFSTransportAddress_ZAddress.OrgPK = value);

				consol.JK_OA_ArrivalUnpackCFSTransportAddress_ZAddress.OrgPK = ZGuid.Empty;
			}
		}

		void AssertProperty(ForwardingConsol consol,
				Action<ZGuid> sendingReceivingAgentSetter,
				ZString partyType, ZString direction,
				string propertyName, Func<ZGuid> propertyGetter, Action<ZGuid> propertySetter)
		{
			var oldValue = Factory.NewWithValidTestData<OrgHeader>();
			var newValue = Factory.NewWithValidTestData<OrgHeader>();

			AddRelatedPartyAndAssignOrg(sendingReceivingAgentSetter, partyType, direction, newValue);

			propertySetter(oldValue.PK);

			attemptToUpdateEventRaised = false;
			attemptToUpdateUsersAnswer = true;
			var result = RelatedPartiesHelper.RecalculateRelatedParties(consol);
			Assert("Attempt to update event should be raised", attemptToUpdateEventRaised);
			AssertEquals($"{propertyName} shouldn't be changed", oldValue.PK, propertyGetter());
			Assert("When login company is shipment's origin", result.WasSuccessful);
			AssertEquals("When login company is not origin nor destination", ZString.Empty, result.Log);

			attemptToUpdateEventRaised = false;
			attemptToUpdateUsersAnswer = false;
			result = RelatedPartiesHelper.RecalculateRelatedParties(consol);
			Assert("Attempt to update event should be raised", attemptToUpdateEventRaised);
			AssertEquals($"{propertyName} should be changed", newValue.PK, propertyGetter());
			Assert("When login company is shipment's origin", result.WasSuccessful);
			AssertEquals("When login company is not origin nor destination", ZString.Empty, result.Log);

			propertySetter(ZGuid.Empty);
			attemptToUpdateEventRaised = false;
			attemptToUpdateUsersAnswer = false;
			result = RelatedPartiesHelper.RecalculateRelatedParties(consol);
			Assert("Attempt to update event should not be raised", !attemptToUpdateEventRaised);
			AssertEquals($"{propertyName} should be changed", newValue.PK, propertyGetter());
			Assert("When login company is shipment's origin", result.WasSuccessful);
			AssertEquals("When login company is not origin nor destination", ZString.Empty, result.Log);
		}

		void AddRelatedPartyAndAssignOrg(
				Action<ZGuid> sendingReceivingAgentSetter,
				ZString partyType,
				ZString direction,
				OrgHeader newValue)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.AddRelatedParty(newValue.PK, partyType, direction, Core.Constants.TransportModes.All, ZString.Empty, GlbCompany.CurrentCompany);
			sendingReceivingAgentSetter(org.PK);
		}

		bool attemptToUpdateUsersAnswer;
		bool attemptToUpdateEventRaised;

		void Shipment_AttempToUpdateExistingValueInRecalculation(object sender, CancelEventArgs e)
		{
			e.Cancel = attemptToUpdateUsersAnswer;
			attemptToUpdateEventRaised = true;
		}

		#endregion
	}
}
