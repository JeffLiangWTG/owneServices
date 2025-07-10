using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Testing
{
	[TestedType(typeof(TransportBookingAdditionalReference))]
	class TransportBookingAdditionalReferenceTest : NonPersistentBusinessObjectTestCase
	{
		public void TestBookingOnly()
		{
			var booking = Helper.CreateBooking();
			var additionalReference = Helper.CreateAddtionalReference(booking, "HSB", "House Bill", DateTime.Now);
			var transportBookingAddtionalReference = new TransportBookingAdditionalReference(booking, additionalReference);
			AssertEquals("Precondition", true, transportBookingAddtionalReference.BookingOnly);
			AssertEquals("Precondition", 1, booking.AdditionalReferenceNumbers.Count);
			Assert("Precondition", booking.AdditionalReferenceNumbers.Contains(additionalReference));
			AssertEquals("Precondition", 0, booking.ConsolidationSingleJob.AdditionalReferenceNumbers.Count);

			transportBookingAddtionalReference.BookingOnly = false;
			AssertEquals(false, transportBookingAddtionalReference.BookingOnly);
			AssertEquals(0, booking.AdditionalReferenceNumbers.Count);
			AssertEquals(1, booking.ConsolidationSingleJob.AdditionalReferenceNumbers.Count);
			AssertEquals("HSB", booking.ConsolidationSingleJob.AdditionalReferenceNumbers[0].CE_EntryType);
			AssertEquals("House Bill", booking.ConsolidationSingleJob.AdditionalReferenceNumbers[0].CE_EntryNum);

			transportBookingAddtionalReference.BookingOnly = true;
			AssertEquals(true, transportBookingAddtionalReference.BookingOnly);
			AssertEquals(0, booking.ConsolidationSingleJob.AdditionalReferenceNumbers.Count);
			AssertEquals(1, booking.AdditionalReferenceNumbers.Count);
			AssertEquals("HSB", booking.AdditionalReferenceNumbers[0].CE_EntryType);
			AssertEquals("House Bill", booking.AdditionalReferenceNumbers[0].CE_EntryNum);
		}

		public void TestProperties()
		{
			var issueDate = DateTime.Now.AddDays(2);
			var booking = Helper.CreateBooking();
			var additionalReference = Helper.CreateAddtionalReference(booking, "HSB", "House Bill", issueDate, "LineRef", "AU");
			var transportBookingAddtionalReference = new TransportBookingAdditionalReference(booking, additionalReference);
			AssertEquals("HSB", transportBookingAddtionalReference.EntryType);
			AssertEquals(additionalReference.CE_EntryTypeInfo, transportBookingAddtionalReference.EntryTypeInfo.InnerInfo);

			AssertEquals("House Bill", transportBookingAddtionalReference.EntryNum);
			AssertEquals(additionalReference.CE_EntryNumInfo, transportBookingAddtionalReference.EntryNumInfo.InnerInfo);

			AssertEquals("House Bill", transportBookingAddtionalReference.AdditionalReferenceNumberTypeDescription);
			AssertEquals(additionalReference.AdditionalReferenceNumberTypeDescriptionInfo, transportBookingAddtionalReference.AdditionalReferenceNumberTypeDescriptionInfo.InnerInfo);

			AssertEquals(issueDate, transportBookingAddtionalReference.IssueDate);
			AssertEquals(additionalReference.CE_IssueDateInfo, transportBookingAddtionalReference.IssueDateInfo.InnerInfo);

			AssertEquals("LineRef", transportBookingAddtionalReference.EntryLineReference);
			AssertEquals(additionalReference.CE_EntryLineReferenceInfo, transportBookingAddtionalReference.EntryLineReferenceInfo.InnerInfo);

			AssertEquals("AU", transportBookingAddtionalReference.RN_NKCountryCode);
			AssertEquals(additionalReference.CE_RN_NKCountryCodeInfo, transportBookingAddtionalReference.RN_NKCountryCodeInfo.InnerInfo);
		}

		public void TestDelete()
		{
			var booking = Helper.CreateBooking();

			var additionalReference1 = Helper.CreateAddtionalReference(booking, "HSB", "House Bill 1", DateTime.Now);
			var transportBookingAddtionalReference1 = new TransportBookingAdditionalReference(booking, additionalReference1);
			transportBookingAddtionalReference1.BookingOnly = false;

			var additionalReference2 = Helper.CreateAddtionalReference(booking, "HSB", "House Bill 2", DateTime.Now);
			var transportBookingAddtionalReference2 = new TransportBookingAdditionalReference(booking, additionalReference2);
			transportBookingAddtionalReference2.BookingOnly = true;

			AssertEquals("Precondition", 1, booking.AdditionalReferenceNumbers.Count);
			AssertEquals("Precondition", 1, booking.ConsolidationSingleJob.AdditionalReferenceNumbers.Count);

			transportBookingAddtionalReference1.Delete();
			transportBookingAddtionalReference2.Delete();

			AssertEquals(0, booking.AdditionalReferenceNumbers.Count);
			AssertEquals(0, booking.ConsolidationSingleJob.AdditionalReferenceNumbers.Count);
			AssertEquals(true, transportBookingAddtionalReference1.IsDeleted);
			AssertEquals(true, transportBookingAddtionalReference2.IsDeleted);
		}

		public void TestMasterBookingHasAdditionalReferencesOfSubsInBindedList()
		{
			var issueDate = DateTime.Now.AddDays(2);
			var masterBooking = Helper.CreateBooking();
			var subBooking1 = Helper.CreateBooking();
			var subBooking2 = Helper.CreateBooking();
			var subBooking3 = Helper.CreateBooking();

			masterBooking.KM_IsMaster = true;
			subBooking1.KM_KM_MasterBooking = masterBooking.PK;
			subBooking2.KM_KM_MasterBooking = masterBooking.PK;
			subBooking3.KM_KM_MasterBooking = masterBooking.PK;

			Factory.Save();

			var additionalReferenceSubBooking1 = Helper.CreateAddtionalReference(subBooking1, "HSB", "Sub 1", issueDate, "LineRef", "AU");
			var additionalReferenceSubBooking2 = Helper.CreateAddtionalReference(subBooking1, "HSB", "Sub 1C", issueDate, "LineRef", "AU");
			var additionalReferenceSubBooking3 = Helper.CreateAddtionalReference(subBooking2, "HSB", "Sub 2", issueDate, "LineRef", "AU");
			var additionalReferenceSubBooking4 = Helper.CreateAddtionalReference(subBooking2, "HSB", "Sub 2C", issueDate, "LineRef", "AU");
			var additionalReferenceSubBooking5 = Helper.CreateAddtionalReference(subBooking3, "HSB", "Sub 3", issueDate, "LineRef", "AU");
			var additionalReferenceSubBooking6 = Helper.CreateAddtionalReference(subBooking3, "HSB", "Sub 3C", issueDate, "LineRef", "AU");

			var transportBookingAdditionalReference1 = new TransportBookingAdditionalReference(subBooking1, additionalReferenceSubBooking1);
			var transportBookingAdditionalReference2 = new TransportBookingAdditionalReference(subBooking1, additionalReferenceSubBooking2);
			var transportBookingAdditionalReference3 = new TransportBookingAdditionalReference(subBooking2, additionalReferenceSubBooking3);
			var transportBookingAdditionalReference4 = new TransportBookingAdditionalReference(subBooking2, additionalReferenceSubBooking4);
			var transportBookingAdditionalReference5 = new TransportBookingAdditionalReference(subBooking3, additionalReferenceSubBooking5);
			var transportBookingAdditionalReference6 = new TransportBookingAdditionalReference(subBooking3, additionalReferenceSubBooking6);

			transportBookingAdditionalReference1.BookingOnly = true;
			transportBookingAdditionalReference2.BookingOnly = false;
			transportBookingAdditionalReference3.BookingOnly = true;
			transportBookingAdditionalReference4.BookingOnly = false;
			transportBookingAdditionalReference5.BookingOnly = true;
			transportBookingAdditionalReference6.BookingOnly = false;

			Factory.Save();

			AssertEquals("Precondition", 1, subBooking1.AdditionalReferenceNumbers.Count);
			AssertEquals("Precondition", 1, subBooking2.AdditionalReferenceNumbers.Count);
			AssertEquals("Precondition", 1, subBooking3.AdditionalReferenceNumbers.Count);

			AssertEquals("Precondition", 1, subBooking1.ConsolidationSingleJob.AdditionalReferenceNumbers.Count);
			AssertEquals("Precondition", 1, subBooking2.ConsolidationSingleJob.AdditionalReferenceNumbers.Count);
			AssertEquals("Precondition", 1, subBooking3.ConsolidationSingleJob.AdditionalReferenceNumbers.Count);

			CombineAssertions(() =>
			{
				AssertEquals("additionalReferenceSubBooking1 is present in the master", "Sub 1", masterBooking.AdditionalReferencesForBinding[0].EntryNum);
				AssertEquals("additionalReferenceSubBooking2 is present in the master", "Sub 1C", masterBooking.AdditionalReferencesForBinding[1].EntryNum);
				AssertEquals("additionalReferenceSubBooking3 is present in the master", "Sub 2", masterBooking.AdditionalReferencesForBinding[2].EntryNum);
				AssertEquals("additionalReferenceSubBooking4 is present in the master", "Sub 2C", masterBooking.AdditionalReferencesForBinding[3].EntryNum);
				AssertEquals("additionalReferenceSubBooking5 is present in the master", "Sub 3", masterBooking.AdditionalReferencesForBinding[4].EntryNum);
				AssertEquals("additionalReferenceSubBooking6 is present in the master", "Sub 3C", masterBooking.AdditionalReferencesForBinding[5].EntryNum);
			});
		}

		public void TestSubBookingHasAdditionalReferencesOfMasterInBindedList()
		{
			var issueDate = DateTime.Now.AddDays(2);
			var masterBooking = Helper.CreateBooking();
			var subBooking = Helper.CreateBooking();

			masterBooking.KM_IsMaster = true;
			subBooking.KM_KM_MasterBooking = masterBooking.PK;

			Factory.Save();

			var additionalReferenceSubBooking1 = Helper.CreateAddtionalReference(masterBooking, "HSB", "Master", issueDate, "LineRef", "AU");
			var additionalReferenceSubBooking2 = Helper.CreateAddtionalReference(masterBooking, "HSB", "Master C", issueDate, "LineRef", "AU");

			var transportBookingAdditionalReference1 = new TransportBookingAdditionalReference(masterBooking, additionalReferenceSubBooking1);
			var transportBookingAdditionalReference2 = new TransportBookingAdditionalReference(masterBooking, additionalReferenceSubBooking2);

			transportBookingAdditionalReference1.BookingOnly = true;
			transportBookingAdditionalReference2.BookingOnly = false;

			Factory.Save();

			AssertEquals("Precondition", 1, masterBooking.AdditionalReferenceNumbers.Count);

			AssertEquals("Precondition", 1, masterBooking.ConsolidationSingleJob.AdditionalReferenceNumbers.Count);

			CombineAssertions(() =>
			{
				AssertEquals("additionalReferenceSubBooking1 is present in the sub", "Master", subBooking.AdditionalReferencesForBinding[0].EntryNum);
				AssertEquals("additionalReferenceSubBooking2 is present in the sub", "Master C", subBooking.AdditionalReferencesForBinding[1].EntryNum);
			});
		}

		public void TestDeleteForDataRefreshInnerAdditionalReference()
		{
			var booking1 = Helper.CreateBooking();
			var additionalReference1 = Helper.CreateAddtionalReference(booking1, "HSB", "House Bill 1", DateTime.Now);
			var transportBookingAddtionalReference1 = new TransportBookingAdditionalReference(booking1, additionalReference1);
			var transportBookingAdditionalReferenceCollection1 = new TransportBookingAdditionalReferenceCollection(booking1);
			transportBookingAdditionalReferenceCollection1.Add(transportBookingAddtionalReference1);
			AssertEquals("transportBookingAddtionalReference1 is not deleted", false, transportBookingAddtionalReference1.IsDeleted);
			AssertEquals("transportBookingAdditionalReferenceCollection1 contains transportBookingAddtionalReference1", true, transportBookingAdditionalReferenceCollection1.Contains(transportBookingAddtionalReference1));

			additionalReference1.DeleteForDataRefresh();

			AssertEquals("After additionalReference doesn't change in transportBookingAddtionalReference1 and the DeleteForDataRefresh() method is called on additionalReference1, transportBookingAddtionalReference1 has been deleted", true, transportBookingAddtionalReference1.IsDeleted);
			AssertEquals("After additionalReference doesn't change in transportBookingAddtionalReference1 and the DeleteForDataRefresh() method is called on additionalReference1, transportBookingAdditionalReferenceCollection1 doesn't contain transportBookingAddtionalReference1", false, transportBookingAdditionalReferenceCollection1.Contains(transportBookingAddtionalReference1));

			var booking2 = Helper.CreateBooking();
			var additionalReference2 = Helper.CreateAddtionalReference(booking2, "HSB", "House Bill 2", DateTime.Now);
			var transportBookingAddtionalReference2 = new TransportBookingAdditionalReference(booking2, additionalReference2);
			var transportBookingAdditionalReferenceCollection2 = new TransportBookingAdditionalReferenceCollection(booking2);
			transportBookingAdditionalReferenceCollection2.Add(transportBookingAddtionalReference2);

			transportBookingAddtionalReference2.BookingOnly = false;
			additionalReference2.DeleteForDataRefresh();

			AssertEquals("After additionalReference is changed(by BookingOnly = false) in transportBookingAddtionalReference2 and DeleteForDataRefresh() method is called on additionalReference2, transportBookingAddtionalReference2 has not been deleted", false, transportBookingAddtionalReference2.IsDeleted);
			AssertEquals("After additionalReference is changed(by BookingOnly = false) in transportBookingAddtionalReference2 and DeleteForDataRefresh() method is called on additionalReference2, transportBookingAdditionalReferenceCollection2 still contains transportBookingAddtionalReference2", true, transportBookingAdditionalReferenceCollection2.Contains(transportBookingAddtionalReference2));

			var booking3 = Helper.CreateBooking();
			var additionalReference3 = Helper.CreateAddtionalReference(booking3, "HSB", "House Bill 3", DateTime.Now);
			var transportBookingAddtionalReference3 = new TransportBookingAdditionalReference(booking3, additionalReference3);
			var transportBookingAdditionalReferenceCollection3 = new TransportBookingAdditionalReferenceCollection(booking3);
			transportBookingAdditionalReferenceCollection3.Add(transportBookingAddtionalReference3);

			booking3.AdditionalReferenceNumbers.Remove(additionalReference3);
			booking3.ConsolidationSingleJob.AdditionalReferenceNumbers.Add(additionalReference3);
			transportBookingAddtionalReference3.BookingOnly = true;
			additionalReference3.DeleteForDataRefresh();

			AssertEquals("After additionalReference is changed(by BookingOnly = true) in transportBookingAddtionalReference3 and DeleteForDataRefresh() method is called on additionalReference3, transportBookingAddtionalReference3 has not been deleted", false, transportBookingAddtionalReference3.IsDeleted);
			AssertEquals("After additionalReference is changed(by BookingOnly = true) in transportBookingAddtionalReference3 and DeleteForDataRefresh() method is called on additionalReference3, transportBookingAdditionalReferenceCollection3 still contains transportBookingAddtionalReference3", true, transportBookingAdditionalReferenceCollection3.Contains(transportBookingAddtionalReference3));
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;

		protected override BusinessObject GetNewBusinessObject()
		{
			var booking = Helper.CreateBooking();
			return new TransportBookingAdditionalReference(booking, booking.AdditionalReferenceNumbers.AddNew());
		}
	}
}
