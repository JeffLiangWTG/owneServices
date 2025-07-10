using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	[TestedType(typeof(AdditionalMessageInformation))]
	class AdditionalMessageInformationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPreSaveValidation()
		{
			AssertEquals("Pre-condition", true, AdditionalMessageInformation.AM_BoardedQty_ReadOnly);
			AssertEquals("Pre-condition", true, AdditionalMessageInformation.AM_BoardedWeight_ReadOnly);
			AssertEquals("Pre-condition", true, AdditionalMessageInformation.AM_BoardedWeightUQ_ReadOnly);
			AdditionalMessageInformation.AM_IsSplitShipment = true;
			AdditionalMessageInformation.RunPreSaveValidation();
			AssertEquals(true, AdditionalMessageInformation.AM_BoardedQtyInfo.HasMessageErrors());
			AssertEquals(true, AdditionalMessageInformation.AM_BoardedWeightInfo.HasMessageErrors());
			AssertEquals(true, AdditionalMessageInformation.AM_BoardedWeightUQInfo.HasMessageErrors());
			AdditionalMessageInformation.AM_BoardedQty = 144;
			AdditionalMessageInformation.AM_BoardedWeight = 25.75m;
			AdditionalMessageInformation.AM_BoardedWeightUQ = "L";
			AdditionalMessageInformation.RunPreSaveValidation();
			AssertEquals(false, AdditionalMessageInformation.AM_BoardedQtyInfo.HasMessageErrors());
			AssertEquals(false, AdditionalMessageInformation.AM_BoardedWeightInfo.HasMessageErrors());
			AssertEquals(false, AdditionalMessageInformation.AM_BoardedWeightUQInfo.HasMessageErrors());
		}

		public void TestFlightDepartureTime()
		{
			Header.AMA_RL_NKPortOfLoading = "NZAKL";
			Header.AMA_E_DEP = new ZDateTime(2020, 07, 24, 19, 25, 00, 00);
			AdditionalMessageInformation.InitialiseFlightDepartureTime();
			AssertEquals("AM_FlightDepartureTime", "2020-07-24T19:25:00", AdditionalMessageInformation.AM_FlightDepartureTime.ToISO8601String());
			AssertEquals("FlightDepartureTimeUTC is UTC from Port Local Time (-12) when Port is specified.", "2020-07-24T07:25:00", AdditionalMessageInformation.FlightDepartureTimeUTC.ToISO8601String());
			AdditionalMessageInformation.AM_FlightDepartureTime = new ZDateTime(2020, 10, 19, 19, 25, 00, 00);
			AssertEquals("AM_FlightDepartureTime", "2020-10-19T19:25:00", AdditionalMessageInformation.AM_FlightDepartureTime.ToISO8601String());
			AssertEquals("Timezone conversion considers daylight savings difference at time of year (-13).", "2020-10-19T06:25:00", AdditionalMessageInformation.FlightDepartureTimeUTC.ToISO8601String());
			Header.AMA_RL_NKPortOfLoading = "";
			Header.AMA_E_DEP = new ZDateTime(2020, 07, 24, 19, 25, 00, 00);
			AdditionalMessageInformation.InitialiseFlightDepartureTime();
			AssertEquals("AM_FlightDepartureTime", "2020-07-24T19:25:00", AdditionalMessageInformation.AM_FlightDepartureTime.ToISO8601String());
			AssertEquals("FlightDepartureTimeUTC is as entered when Port is not specified.", "2020-07-24T19:25:00", AdditionalMessageInformation.FlightDepartureTimeUTC.ToISO8601String());
			AdditionalMessageInformation.AM_FlightDepartureTime = ZDateTime.Invalid;
			AssertEquals("AM_FlightDepartureTime", "<Invalid>", AdditionalMessageInformation.AM_FlightDepartureTime.ToISO8601String());
			AssertEquals("FlightDepartureTimeUTC is empty when FlightDepartureTime is Invalid", "", AdditionalMessageInformation.FlightDepartureTimeUTC.ToISO8601String());
		}

		[TestTimeZoneUNLOCO("USLAX")]
		[TestDate(2020, 07, 24, 07, 30, 00, 00)]
		public void TestValidateAM_FlightDepartureTime()
		{
			TestDateAttribute.UseUNLOCO = true;
			GlbCompany.CurrentCompany.SetCountry("US");
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "USLAX";
			AssertEquals("Precondition to verify timezone is being applied", "2020-07-24T00:30:00", ZDateTime.Now.ToISO8601String());
			var header = Header;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_RL_NKPortOfLoading = "NZAKL";
			header.AMA_RL_NKPortOfDischarge = "USCHI";
			header.AMA_ManifestType = "IAM";
			header.AMA_Voyage = "KLM325";
			header.AMA_E_DEP = ZDateTime.Empty;
			header.AMA_E_ARV = ZDateTime.Empty;
			AdditionalMessageInformation.RunPreSaveValidation();
			AssertEquals(false, AdditionalMessageInformation.AM_FlightDepartureTimeInfo.HasErrors());
			AdditionalMessageInformation.InitialiseFlightDepartureTime();
			Assert(AdditionalMessageInformation.AM_FlightDepartureTime.IsEmpty);
			AdditionalMessageInformation.RunPreSaveValidation();
			AssertHasError(AdditionalMessageInformation.AM_FlightDepartureTimeInfo, "Lift Off Time is required.");
			AdditionalMessageInformation.AM_FlightDepartureTime = new ZDateTime(2020, 07, 24, 19, 35, 00, 00);
			AssertNoError(AdditionalMessageInformation.AM_FlightDepartureTimeInfo, "Lift Off Time is required.");
			AssertHasError(AdditionalMessageInformation.AM_FlightDepartureTimeInfo, "Lift Off Time cannot be in the future.");
			AdditionalMessageInformation.AM_FlightDepartureTime = new ZDateTime(2020, 07, 24, 19, 25, 00, 00);
			AssertNoError(AdditionalMessageInformation.AM_FlightDepartureTimeInfo, "Lift Off Time is required.");
			AssertNoError(AdditionalMessageInformation.AM_FlightDepartureTimeInfo, "Lift Off Time cannot be in the future.");
			AdditionalMessageInformation.AM_FlightDepartureTime = ZDateTime.Invalid;
			AssertHasError(AdditionalMessageInformation.AM_FlightDepartureTimeInfo, "Lift Off Time is required.");
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return new AdditionalMessageInformation(header);
		}

		AsycudaManifestHeader Header
		{
			get
			{
				if (header == null)
				{
					header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
					header.AMA_MasterBill = "SHA-123456789";
				}

				return header;
			}
		}

		AsycudaManifestHeader header;
		AdditionalMessageInformation AdditionalMessageInformation => additionalMessageInformation ?? (additionalMessageInformation = new AdditionalMessageInformation(Header));
		AdditionalMessageInformation additionalMessageInformation;
	}
}
