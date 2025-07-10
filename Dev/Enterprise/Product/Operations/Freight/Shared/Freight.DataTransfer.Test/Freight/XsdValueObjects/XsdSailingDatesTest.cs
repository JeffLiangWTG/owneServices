using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer.Testing
{
	sealed class XsdSailingDatesTest : TestCaseWithFactory
	{
		readonly DateTime TestAvailabilityDate = new DateTime(2005, 1, 1);
		readonly DateTime TestCutOffDate = new DateTime(2005, 1, 1);
		readonly DateTime TestReceivalCommencesDate = new DateTime(2005, 1, 1);
		readonly DateTime TestStorageDate = new DateTime(2005, 1, 1);

		public void TestToLCLValueObject()
		{
			JobSailing sailing = Factory.New<JobSailing>();
			sailing.JX_DepotAvailabilityDate = TestAvailabilityDate;
			sailing.JX_DepotCutOff = TestCutOffDate;
			sailing.JX_DepotReceivalCommences = TestReceivalCommencesDate;
			sailing.JX_DepotStorageDate = TestStorageDate;

			Xsd.SailingDates datesValue = XsdSailingDates.ToLCLValueObject(
				sailing,
				new DateTime(2005, 1, 1),
				new DateTime(2005, 2, 2),
				new DateTime(2005, 3, 3),
				new DateTime(2005, 4, 4));
			AssertValueObjectCorrect(datesValue);
		}

		public void TestToFCLValueObject()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "NLAMS";
			origin.JA_CutOff = TestCutOffDate;
			origin.JA_ReceivalCommences = TestReceivalCommencesDate;

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUBNE";
			destination.JB_AvailabilityDate = TestAvailabilityDate;
			destination.JB_StorageDate = TestStorageDate;

			voyage.GenerateSailings();

			JobSailing sailing = voyage.Sailings[0];

			Xsd.SailingDates datesValue = XsdSailingDates.ToFCLValueObject(
				sailing,
				new DateTime(2005, 1, 1),
				new DateTime(2005, 2, 2),
				new DateTime(2005, 3, 3),
				new DateTime(2005, 4, 4));
			AssertValueObjectCorrect(datesValue);
		}

		void AssertValueObjectCorrect(Xsd.SailingDates datesValue)
		{
			AssertEquals("AvailableDate", datesValue.AvailableDate, TestAvailabilityDate);
			AssertEquals("AvailableDate", true, datesValue.AvailableDate.IsValid);
			AssertEquals("CutOffDate", datesValue.CutOffDate, TestCutOffDate);
			AssertEquals("CutOffDate", true, datesValue.CutOffDate.IsValid);
			AssertEquals("ReceivalCommencesDate", datesValue.ReceivalCommencesDate, TestReceivalCommencesDate);
			AssertEquals("ReceivalCommencesDate", true, datesValue.ReceivalCommencesDate.IsValid);
			AssertEquals("StorageDate", datesValue.StorageDate, TestStorageDate);
			AssertEquals("StorageDate", true, datesValue.StorageDate.IsValid);
		}

		public void TestFromLCLValueObject()
		{
			JobSailing sailing = Factory.New<JobSailing>();
			Xsd.SailingDates datesValue = new Xsd.SailingDates();
			datesValue.AvailableDate = TestAvailabilityDate;
			datesValue.CutOffDate = TestCutOffDate;
			datesValue.ReceivalCommencesDate = TestReceivalCommencesDate;
			datesValue.StorageDate = TestStorageDate;

			XsdSailingDates.FromLCLValueObject(sailing, datesValue);

			AssertEquals("AvailableDate", TestAvailabilityDate, sailing.JX_DepotAvailabilityDate);
			AssertEquals("CutOffDate", TestCutOffDate, sailing.JX_DepotCutOff);
			AssertEquals("ReceivalCommencesDate", TestReceivalCommencesDate, sailing.JX_DepotReceivalCommences);
			AssertEquals("StorageDate", TestStorageDate, sailing.JX_DepotStorageDate);
		}

		public void TestFromFCLValueObject()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "NLAMS";

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUBNE";

			voyage.GenerateSailings();

			JobSailing sailing = voyage.Sailings[0];

			Xsd.SailingDates datesValue = new Xsd.SailingDates();
			datesValue.AvailableDate = TestAvailabilityDate;
			datesValue.CutOffDate = TestCutOffDate;
			datesValue.ReceivalCommencesDate = TestReceivalCommencesDate;
			datesValue.StorageDate = TestStorageDate;

			XsdSailingDates.FromFCLValueObject(sailing, datesValue);

			AssertEquals("AvailableDate", TestAvailabilityDate, sailing.JX_JB_CTOAvailabilityDate);
			AssertEquals("CutOffDate", TestCutOffDate, sailing.JX_JA_CTOCutOff);
			AssertEquals("ReceivalCommencesDate", TestReceivalCommencesDate, sailing.JX_JA_CTOReceivalCommences);
			AssertEquals("StorageDate", TestStorageDate, sailing.JX_JB_CTOStorageDate);
		}
	}
}
