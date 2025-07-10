using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business.UniversalData.Testing
{
	sealed class ManualDataExportLookupsTest : TestCaseWithFactory
	{
		public void TestEventCodeList()
		{
			var lookups = GetLookups();
			var eventCodeList = lookups.EventCodeList;

			Assert("lookups.EventCodeList contains 'ATH'", eventCodeList.ContainsCode(Events.AuthorisedCode));

			AssertEquals("Arrival trigger", true, eventCodeList.ContainsCode(Events.Arrival.Code));
			AssertEquals("Record Added trigger", true, eventCodeList.ContainsCode(Events.AddedARecordToTheSystem.Code));
			AssertEquals("Record Edited trigger", true, eventCodeList.ContainsCode(Events.EditedARecord.Code));
			AssertEquals("Set to Active change log is a trigger", true, eventCodeList.ContainsCode(Events.SetToActive.Code));
			AssertEquals("Set to Inactive change log is a trigger", true, eventCodeList.ContainsCode(Events.SetToInactive.Code));
			AssertEquals("Should not contain WTE event", false, eventCodeList.ContainsCode(Events.WorkflowTriggerEvent.Code));
			AssertEquals("Exception Raised trigger", true, eventCodeList.ContainsCode(Events.ExceptionRaised.Code));
		}

		public void TestRecipientTypeList()
		{
			DummyWorkflowDescriptor.Instance.SupportedMessageRecipientPartiesExposed = MessageRecipientPartyType.OrgProxy;
			DummyWorkflowDescriptor.Instance.SupportedMessageRecipientPartiesForSpecificActionExposed = MessageRecipientPartyType.CarrierMessagingDebtor;

			var lookups = GetLookups();
			var recipientTypes = lookups.RecipientTypeList;
			AssertNotNull("lookups.RecipientTypeList", recipientTypes);
			Assert("lookups.RecipientTypeList should contain 'ORP'. Contained: " + recipientTypes.GetHumanReadableListOfElements(", "), recipientTypes.ContainsCode("ORP"));
			Assert("lookups.RecipientTypeList should contain 'CMD'. Contained: " + recipientTypes.GetHumanReadableListOfElements(", "), recipientTypes.ContainsCode("CMD"));
			AssertEquals("OTH", "Other", recipientTypes.GetDescriptionFromCode(MessageRecipientPartyTypeList.SpecialCodes.Other));

			AssertEquals("RecipientTypeList should be cached", recipientTypes, lookups.RecipientTypeList);
		}

		public void TestRecipientServices()
		{
			DummyWorkflowDescriptor.Instance.SupportedTriggerPartyServicesExposed = new ZString[] { "ABC", "DEF" };

			var dummyBO = Factory.New<DummyWithWorkflow>();
			var dataExport = new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalShipment) { RecipientType = nameof(RecipientRoleType.ORP) };
			var lookups = dataExport.Lookups;
			var recipientServices = lookups.RecipientServices;
			AssertNotNull("lookups.RecipientServices", recipientServices);
			AssertContainsExactElementsInAnyOrder(new[] { "ABC", "DEF" }, recipientServices.GetAllCodes());

			AssertEquals("RecipientServices should be cached.", recipientServices, lookups.RecipientServices);
		}

		public void TestPurposeCodeList()
		{
			var lookups = GetLookups();
			var purposeCodes = lookups.PurposeCodeList;
			AssertNotNull("lookups.PurposeCodeList", purposeCodes);
			Assert("lookups.PurposeCodeList contains 'APP'", purposeCodes.ContainsCode("APP"));
		}

		#region Implementation

		ManualDataExportLookups GetLookups()
		{
			AssertNotNull(DummyWorkflowDescriptor.Instance);
			var dummyBO = Factory.New<DummyWithWorkflow>();
			var dataExport = new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalShipment);
			var lookups = dataExport.Lookups;
			return lookups;
		}

		#endregion
	}
}
