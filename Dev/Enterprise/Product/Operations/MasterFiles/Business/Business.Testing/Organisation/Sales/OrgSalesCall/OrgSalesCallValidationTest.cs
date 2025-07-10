using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgSalesCallValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateOQ_OH()
		{
			var call = Factory.New<OrgSalesCall>();

			call.LinkedInquiry = null;
			call.Validation.ValidateOQ_OH();
			AssertMandatoryValidationError(call.OQ_OHInfo, true);

			call.LinkedInquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			call.Validation.ValidateOQ_OH();
			AssertMandatoryValidationError(call.OQ_OHInfo, false);
		}

		public void TestValidateOQ_TypeOfCall()
		{
			var communicationTypes = new CodeDescriptionBoolCollection();
			communicationTypes.Add("AAA", (NoResString)"AAA", true);
			communicationTypes.Add("BBB", (NoResString)"BBB", false);
			OrganisationsDataRegistry.Instance.CommunicationType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, communicationTypes);

			var salesOrg = Factory.NewWithValidTestData<OrgHeader>();
			salesOrg.OH_IsSalesLead = true;
			var call = salesOrg.SalesCalls.AddNew();

			SetCommunicationMandatoryFieldsRegistry(OrgSalesCallSchema.Constants.OQ_TypeOfCall, false);
			CombineAssertions(() =>
			{
				AssertNoErrors(call.OQ_TypeOfCallInfo, "");
				AssertNoErrors(call.OQ_TypeOfCallInfo, "AAA");

				AssertListValidationInvalidCodeError(call.OQ_TypeOfCallInfo, "BBB", true);
				Factory.Save();
				call.Validation.ValidateOQ_TypeOfCall();
				AssertHasWarnings(call.OQ_TypeOfCallInfo);

				AssertListValidationInvalidCodeError(call.OQ_TypeOfCallInfo, "CCC", true);
			});

			SetCommunicationMandatoryFieldsRegistry(OrgSalesCallSchema.Constants.OQ_TypeOfCall, true);
			CombineAssertions(() =>
			{
				AssertMandatoryValidationError(call.OQ_TypeOfCallInfo, "", true);
				AssertNoErrors(call.OQ_TypeOfCallInfo, "AAA");

				Factory.Save();
				AssertListValidationInvalidCodeError(call.OQ_TypeOfCallInfo, "BBB", true);
				Factory.Save();
				call.Validation.ValidateOQ_TypeOfCall();
				AssertHasWarnings(call.OQ_TypeOfCallInfo);

				AssertListValidationInvalidCodeError(call.OQ_TypeOfCallInfo, "CCC", true);
			});
		}

		public void TestValidateLocation()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var address1 = org.Addresses.AddNew();
			address1.OA_Address1 = "Address 1";
			var address2 = org.Addresses.AddNew();
			address2.OA_Address1 = "Address 2";

			var call = org.SalesCalls.AddNew();
			call.Location = address2.OA_Address1;

			AssertNoErrors(call.LocationInfo, "");
			AssertNoErrors(call.LocationInfo, "Address 2");
			address2.OA_IsActive = false;
			Factory.Save();

			call.Validation.ValidateLocation();
			AssertHasWarnings(call.LocationInfo);
		}

		public void TestValidateLocationFreeType()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var address1 = org.Addresses.AddNew();
			address1.OA_Address1 = "Address 1";
			var address2 = org.Addresses.AddNew();
			address2.OA_Address1 = "Address 2";

			var call = org.SalesCalls.AddNew();
			call.Location = "Alpha Centauri";

			call.Validation.ValidateLocation();
			AssertNoNotifications(call.LocationInfo);
		}

		public void TestValidateOQ_CallSummary()
		{
			var call = Factory.New<OrgSalesCall>();

			SetCommunicationMandatoryFieldsRegistry(OrgSalesCallSchema.Constants.OQ_CallSummary, false);
			CombineAssertions(() =>
			{
				AssertNoErrors(call.OQ_CallSummaryInfo, "");
				AssertNoErrors(call.OQ_CallSummaryInfo, "My Call Summary");
			});

			SetCommunicationMandatoryFieldsRegistry(OrgSalesCallSchema.Constants.OQ_CallSummary, true);
			CombineAssertions(() =>
			{
				AssertMandatoryValidationError(call.OQ_CallSummaryInfo, "", true);
				AssertNoErrors(call.OQ_CallSummaryInfo, "My Call Summary");
			});
		}

		public void TestValidateOQ_Status()
		{
			var communicationStatuses = new CommunicationStatusCollection();
			communicationStatuses.Add("AAA", (NoResString)"AAA", true);
			communicationStatuses.Add("BBB", (NoResString)"BBB", false);
			OrganisationsDataRegistry.Instance.CommunicationStatusList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, communicationStatuses);

			var salesOrg = Factory.NewWithValidTestData<OrgHeader>();
			salesOrg.OH_IsSalesLead = true;
			var call = salesOrg.SalesCalls.AddNew();

			SetCommunicationMandatoryFieldsRegistry(OrgSalesCallSchema.Constants.OQ_Status, false);
			CombineAssertions(() =>
			{
				AssertNoErrors(call.OQ_StatusInfo, "");
				AssertNoErrors(call.OQ_StatusInfo, "AAA");

				AssertListValidationInvalidCodeError(call.OQ_StatusInfo, "BBB", true);
				Factory.Save();
				call.Validation.ValidateOQ_Status();
				AssertHasWarnings(call.OQ_StatusInfo);

				AssertListValidationInvalidCodeError(call.OQ_StatusInfo, "CCC", true);
			});

			SetCommunicationMandatoryFieldsRegistry(OrgSalesCallSchema.Constants.OQ_Status, true);
			CombineAssertions(() =>
			{
				AssertMandatoryValidationError(call.OQ_StatusInfo, "", true);
				AssertNoErrors(call.OQ_StatusInfo, "AAA");

				Factory.Save();
				AssertListValidationInvalidCodeError(call.OQ_StatusInfo, "BBB", true);
				Factory.Save();
				call.Validation.ValidateOQ_Status();
				AssertHasWarnings(call.OQ_StatusInfo);

				AssertListValidationInvalidCodeError(call.OQ_StatusInfo, "CCC", true);
			});
		}

		public void TestValidateOQ_StatusWithActualDateEntered()
		{
			var communicationStatuses = new CommunicationStatusCollection();
			communicationStatuses.Add("AAA", (NoResString)"AAA", true, true);
			communicationStatuses.Add("BBB", (NoResString)"BBB", false, true);
			OrganisationsDataRegistry.Instance.CommunicationStatusList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, communicationStatuses);

			var salesOrg = Factory.NewWithValidTestData<OrgHeader>();
			salesOrg.OH_IsSalesLead = true;

			var call = salesOrg.SalesCalls.AddNew();
			call.OQ_CallDate = ZDateTime.Empty;

			SetCommunicationMandatoryFieldsRegistry(OrganisationsDataRegistry.CommunicationMandatoryFieldsClosedStatusRequiredCode, true);

			call.OQ_Status = "AAA";
			AssertNoErrors(call.OQ_StatusInfo, "AAA");
			call.OQ_Status = "BBB";
			AssertNoErrors(call.OQ_StatusInfo, "BBB");

			call.OQ_CallDate = ZDateTime.BrettsBirthday;
			call.Validation.ValidateOQ_Status();

			call.OQ_Status = "AAA";
			AssertNoErrors(call.OQ_StatusInfo, "AAA");
			call.OQ_Status = "BBB";
			AssertHasErrors(call.OQ_StatusInfo);

			SetCommunicationMandatoryFieldsRegistry(OrganisationsDataRegistry.CommunicationMandatoryFieldsClosedStatusRequiredCode, false);

			call.OQ_Status = "AAA";
			AssertNoErrors(call.OQ_StatusInfo, "AAA");
			call.OQ_Status = "BBB";
			AssertNoErrors(call.OQ_StatusInfo, "BBB");
		}

		[TestDate(2013, 7, 26)]
		public void TestValidateDate()
		{
			var salesCall = Factory.New<OrgSalesCall>();
			salesCall.Validation.ValidateOQ_CallDate();
			salesCall.Validation.ValidateOQ_NextCall();
			AssertHasError(salesCall.OQ_CallDateLocalInfo, "Please enter a scheduled date or actual date.");
			AssertHasError(salesCall.OQ_NextCallLocalInfo, "Please enter a scheduled date or actual date.");

			salesCall.OQ_NextCall = new ZDateTime(2013, 7, 26, 10, 0, 0);
			salesCall.Validation.ValidateOQ_CallDate();
			salesCall.Validation.ValidateOQ_NextCall();
			AssertNoErrors(salesCall.OQ_CallDateLocalInfo);
			AssertNoErrors(salesCall.OQ_NextCallLocalInfo);

			salesCall.OQ_NextCall = ZDateTime.Empty;
			salesCall.OQ_CallDate = new ZDateTime(2013, 7, 26, 12, 0, 0);
			salesCall.Validation.ValidateOQ_CallDate();
			salesCall.Validation.ValidateOQ_NextCall();
			AssertNoErrors(salesCall.OQ_CallDateLocalInfo);
			AssertNoErrors(salesCall.OQ_NextCallLocalInfo);
		}

		[TestTimeZoneUNLOCO("AUBNE")]
		[TestDate(2013, 7, 26)]
		public void TestCheckOQ_CallDateIsValidZDateTimeRange()
		{
			TestDateAttribute.UseUNLOCO = true;
			AssertEquals("Precondition to verify timezone is being applied", "2013-07-26T10:00:00", ZDateTime.Now.ToISO8601String());

			var salesCall = Factory.New<OrgSalesCall>();
			salesCall.OQ_CallDateLocal = ZDateTime.Now.AddHours(25);
			salesCall.Validation.ValidateOQ_CallDate();
			AssertHasError(salesCall.OQ_CallDateInfo, "Actual date cannot be in the future by more than 24 hours.");
		}

		public void TestValidateOQ_Category()
		{
			var communicationCategory = new CodeDescriptionBoolCollection();
			communicationCategory.Add("AAA", (NoResString)"AAA", true);
			communicationCategory.Add("BBB", (NoResString)"BBB", false);
			OrganisationsDataRegistry.Instance.CategoryList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, communicationCategory);

			var salesOrg = Factory.NewWithValidTestData<OrgHeader>();
			salesOrg.OH_IsSalesLead = true;
			var call = salesOrg.SalesCalls.AddNew();

			SetCommunicationMandatoryFieldsRegistry(OrgSalesCallSchema.Constants.OQ_Category, false);
			CombineAssertions(() =>
			{
				AssertNoErrors(call.OQ_CategoryInfo, "");
				AssertNoErrors(call.OQ_CategoryInfo, "AAA");

				AssertListValidationInvalidCodeError(call.OQ_CategoryInfo, "BBB", true);
				Factory.Save();
				call.Validation.ValidateOQ_Category();
				AssertHasWarnings(call.OQ_CategoryInfo);

				AssertListValidationInvalidCodeError(call.OQ_CategoryInfo, "CCC", true);
			});

			SetCommunicationMandatoryFieldsRegistry(OrgSalesCallSchema.Constants.OQ_Category, true);
			CombineAssertions(() =>
				{
					AssertMandatoryValidationError(call.OQ_CategoryInfo, "", true);
					AssertNoErrors(call.OQ_CategoryInfo, "AAA");

					Factory.Save();
					AssertListValidationInvalidCodeError(call.OQ_CategoryInfo, "BBB", true);
					Factory.Save();
					call.Validation.ValidateOQ_Category();
					AssertHasWarnings(call.OQ_CategoryInfo);

					AssertListValidationInvalidCodeError(call.OQ_CategoryInfo, "CCC", true);
				});
		}

		public void TestValidateShouldSendInvitationWhenSalesRepIsNull()
		{
			var call = Factory.New<OrgSalesCall>();
			call.OQ_GS_NKSalesRep = "_!_";

			call.ShouldSendInvitation = true;
			AssertHasErrors(call.ShouldSendInvitationInfo);

			call.ShouldSendInvitation = false;
			AssertNoErrors("There shouldn't be any errors", call.ShouldSendInvitationInfo);
		}

		public void TestValidateShouldSendInvitation()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ASX";

			var call = Factory.New<OrgSalesCall>();

			call.OQ_GS_NKSalesRep = "ASX";
			call.ShouldSendInvitation = true;
			AssertHasErrors(call.ShouldSendInvitationInfo);

			staff.GS_EmailAddress = "test@gmail.com";
			call.OQ_GS_NKSalesRep = "ASX";
			call.ShouldSendInvitation = true;
			AssertNoErrors(call.ShouldSendInvitationInfo);

			call.ShouldSendInvitation = false;
			AssertNoErrors("There shouldn't be any errors", call.ShouldSendInvitationInfo);
		}

		[TestDate(2013, 8, 26, 0, 0, 0)]
		public void TestValidateOQ_CallDateIsValidZDateTimeRange()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var salesCall = org.SalesCalls.AddNew();

			Factory.Save();

			salesCall.OQ_CallDate = new ZDateTime(2013, 8, 26, 10, 0, 0);
			AssertNoErrors(salesCall.OQ_CallDateInfo);

			salesCall.OQ_CallDate = new ZDateTime(2013, 9, 26, 10, 0, 0);
			AssertHasError(salesCall.OQ_CallDateInfo, "Actual date cannot be in the future by more than 24 hours.");

			Factory.Save();
			salesCall.Validation.ValidateOQ_CallDate();
			AssertNoErrors(salesCall.OQ_CallDateInfo);
		}

		public void TestValidateOQ_IsReminderClientFacing()
		{
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			var attendee = communication.AdditionalAttendeesContact.AddNew();
			attendee.O6_AttendeeName = "B1";
			attendee.O6_ReceiverReminder = false;
			communication.Validation.ValidateOQ_IsReminderClientFacing();
			AssertNoWarnings("No invited attendee", communication.OQ_IsReminderClientFacingInfo);

			attendee.O6_ReceiverReminder = true;
			communication.OQ_IsReminderClientFacing = false;
			communication.Validation.ValidateOQ_IsReminderClientFacing();
			AssertHasWarnings("1 invited attendee but unticked client visible invitation", communication.OQ_IsReminderClientFacingInfo);

			communication.OQ_IsReminderClientFacing = true;
			communication.Validation.ValidateOQ_IsReminderClientFacing();
			AssertNoWarnings("1 invited attendee and ticked client visible invitation", communication.OQ_IsReminderClientFacingInfo);

			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var reloadedCommunication = newFactory.Load<OrgSalesCall>(communication.PK);
			reloadedCommunication.OQ_IsReminderClientFacing = false;
			reloadedCommunication.Validation.ValidateOQ_IsReminderClientFacing();
			AssertHasWarnings("1 invited attendee but unticked client version invitation", reloadedCommunication.OQ_IsReminderClientFacingInfo);

			var reloadedAttendee = newFactory.Load<OrgSalesCallAdditionalAttendee>(attendee.PK);
			reloadedAttendee.O6_ReceiverReminder = false;
			reloadedCommunication.Validation.ValidateOQ_IsReminderClientFacing();
			AssertHasWarnings("0 UNSAVED invited attendee and unticked client visible invitation", reloadedCommunication.OQ_IsReminderClientFacingInfo);
		}

		#region Implementation

		void SetCommunicationMandatoryFieldsRegistry(string field, bool value)
		{
			var mandatoryFieldsCollection = new CodeDescriptionBoolDisallowNewCollection();
			mandatoryFieldsCollection.Add(50, field, (NoResString)field, value);
			OrganisationsDataRegistry.Instance.CommunicationMandatoryFields.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mandatoryFieldsCollection);
		}

		void AssertMandatoryValidationError(ZPropertyInfo info, ZString value, bool isExpectingError)
		{
			info.Value = value;
			AssertMandatoryValidationError(info, isExpectingError);
		}

		void AssertListValidationInvalidCodeError(ZPropertyInfo info, ZString value, bool isExpectingError)
		{
			info.Value = value;
			AssertListValidationInvalidCodeError(info, isExpectingError);
		}

		void AssertNoErrors(ZPropertyInfo info, ZString value)
		{
			info.Value = value;
			AssertNoErrors(info);
		}

		#endregion
	}
}
