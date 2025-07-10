using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSalesCallAdditionalAttendeeContactCollection))]
	sealed class OrgSalesCallAdditionalAttendeeContactCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDefaults()
		{
			var salesCall = Factory.New<OrgSalesCall>();
			var additionalContact = salesCall.AdditionalAttendeesContact.AddNew();
			AssertEquals(OrgContactSchema.Constants.Prefix, additionalContact.O6_AttendeeTableCode);
			AssertEquals(false, additionalContact.O6_ReceiverReminder);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrgSalesCall salesCall = Factory.New<OrgSalesCall>();
			return new OrgSalesCallAdditionalAttendeeContactCollection(salesCall);
		}

		public void TestReceiveReminderHasValueChanged()
		{
			var salesCall = Factory.NewWithValidTestData<OrgSalesCall>();
			var isEventFired = false;
			salesCall.AdditionalAttendeesContact.ReceiveReminderHasValueChanged += (sender, e) =>
			{
				isEventFired = true;
			};

			var additionalContact = salesCall.AdditionalAttendeesContact.AddNew();
			AssertEquals("Event fires after adding an element", true, isEventFired);

			isEventFired = false;
			additionalContact.O6_ReceiverReminder = true;
			AssertEquals("Event fires after changing O6_ReceiverReminder value of element", true, isEventFired);

			isEventFired = false;
			additionalContact.O6_AttendeeName = "Jenny";
			AssertEquals("Event fires after changing non-O6_ReceiverReminder value of element", false, isEventFired);

			salesCall.AdditionalAttendeesContact.Remove(additionalContact);
			AssertEquals("Event fires after removing the element", false, isEventFired);

			additionalContact.O6_ReceiverReminder = false;
			AssertEquals("Event fires after changing O6_ReceiverReminder value of removed element", false, isEventFired);
		}
	}
}
