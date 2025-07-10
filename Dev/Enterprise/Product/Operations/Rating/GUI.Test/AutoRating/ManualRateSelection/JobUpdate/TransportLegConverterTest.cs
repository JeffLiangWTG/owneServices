using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using WiseRates.Api.Model;

namespace Enterprise.Rating.GUI.Testing
{
	public class TransportLegConverterTest : TestCaseWithFactory
	{
		public void TestConvert_ShouldCorrectlyMapScheduleDetailToTransportLeg()
		{
			var carrier = Factory.New<OrgHeader>();

			var scheduleDetails = new[]
			{
				new ScheduleDetail()
				{
					DepartureDate = new DateTime(2021, 01, 01),
					ArrivalDate = new DateTime(2021, 01, 12, 21, 0, 0),
					VesselName = "MAERSK RIDE",
					VoyageNumber = "45WTG",
					Destination = "USLAX",
					Origin = "HKHKG",
					VGMCutOff = new DateTime(2021, 02, 21, 16, 0, 0),
					CTOCutOff = new DateTime(2021, 02, 23, 16, 0, 0),
					DocsDue = new DateTime(2021, 02, 22, 18, 0, 0),
				}
			};

			Env.Security.VesselsModify.IsAllowed = true;
			UnitTestUserNotification.Instance.AddYesAnswer();

			var logger = new TestLogger();
			var leg = TransportLegConverter.Convert(Factory, carrier, scheduleDetails, logger).First();
			AssertEquals(new DateTime(2021, 01, 01), leg.JW_ETD);
			AssertEquals(new DateTime(2021, 01, 12, 21, 0, 0), leg.JW_ETA);
			AssertEquals("MAERSK RIDE", leg.JW_Vessel);
			AssertEquals("45WTG", leg.JW_VoyageFlight);
			AssertEquals("HKHKG", leg.JW_RL_NKLoadPort);
			AssertEquals("USLAX", leg.JW_RL_NKDiscPort);
			AssertEquals(new DateTime(2021, 02, 21, 16, 0, 0), leg.JW_VGMCutOff);
			AssertEquals(new DateTime(2021, 02, 23, 16, 0, 0), leg.JW_TerminalCutOff);
			AssertEquals(new DateTime(2021, 02, 22, 18, 0, 0), leg.JW_DocumentaryCutOff);
		}

		public void TestConvert_WhenVesselDoesNotExist_ShouldShowWarningMessage()
		{
			var carrier = Factory.New<OrgHeader>();

			var scheduleDetails = new[]
			{
				new ScheduleDetail()
				{
					IMONumber = "8734987",
					ServiceName = "Some Service Name",
					ServiceCode = "SC12345",
					TradeLane = "FAR/EUR",
					TransitTime = new TimeSpan(12, 21, 0, 0),
					DepartureDate = new DateTime(2021, 01, 01),
					ArrivalDate = new DateTime(2021, 01, 12, 21, 0, 0),
					VesselName = "MAERSK RIDE",
					VoyageNumber = "45WTG",
					Destination = "USLAX",
					Origin = "HKHKG",
					FlagCode = "UK",
				},
			};

			var vessel = RefVessel.LookupVesselByName("MAERSK RIDE", Factory, true).FirstOrDefault();

			AssertNull(vessel);

			Env.Security.VesselsModify.IsAllowed = true;
			UnitTestUserNotification.Instance.AddYesAnswer();

			var logger = new TestLogger();
			TransportLegConverter.Convert(Factory, carrier, scheduleDetails, logger);

			var expectedWarning = @"During the operation the Vessel: 'MAERSK RIDE' IMO: '8734987' could not be found. Do you want to create a new Vessel?

Click 'Yes' to proceed with creating a new Vessel

Click 'No' to proceed without creating any Vessel";

			AssertEquals(expectedWarning, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestConvert_WhenVesselDoesNotExist_UserDoesNotHaveVesselCreationAccess_ShouldShowLoginForm()
		{
			var carrier = Factory.New<OrgHeader>();

			var scheduleDetails = new[]
			{
				new ScheduleDetail()
				{
					IMONumber = "8734987",
					ServiceName = "Some Service Name",
					ServiceCode = "SC12345",
					TradeLane = "FAR/EUR",
					TransitTime = new TimeSpan(12, 21, 0, 0),
					DepartureDate = new DateTime(2021, 01, 01),
					ArrivalDate = new DateTime(2021, 01, 12, 21, 0, 0),
					VesselName = "MAERSK RIDE",
					VoyageNumber = "45WTG",
					Destination = "USLAX",
					Origin = "HKHKG",
					FlagCode = "UK",
				},
			};

			var vessel = RefVessel.LookupVesselByName("MAERSK RIDE", Factory, true).FirstOrDefault();

			AssertNull(vessel);

			Env.Security.VesselsModify.IsAllowed = false;
			UnitTestUserNotification.Instance.AddYesAnswer();

			var logger = new TestLogger();
			TransportLegConverter.Convert(Factory, carrier, scheduleDetails, logger);

			AssertEquals(typeof(LoginForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}

		public void TestConvert_WhenVesselDoesNotExist_WhenWarningAccepted_ShouldCreateANewVessel()
		{
			var carrier = Factory.New<OrgHeader>();

			var scheduleDetails = new[]
			{
				new ScheduleDetail()
				{
					IMONumber = "8734987",
					ServiceName = "Some Service Name",
					ServiceCode = "SC12345",
					TradeLane = "FAR/EUR",
					TransitTime = new TimeSpan(12, 21, 0, 0),
					DepartureDate = new DateTime(2021, 01, 01),
					ArrivalDate = new DateTime(2021, 01, 12, 21, 0, 0),
					VesselName = "MAERSK RIDE",
					VoyageNumber = "45WTG",
					Destination = "USLAX",
					Origin = "HKHKG",
					FlagCode = "UK",
				},
			};

			var vessel = RefVessel.LookupVesselByName("MAERSK RIDE", Factory, true).FirstOrDefault();

			AssertNull(vessel);

			Env.Security.VesselsModify.IsAllowed = true;
			UnitTestUserNotification.Instance.AddYesAnswer();
			var logger = new TestLogger();
			TransportLegConverter.Convert(Factory, carrier, scheduleDetails, logger);

			vessel = RefVessel.LookupVesselByName("MAERSK RIDE", Factory, true).FirstOrDefault();

			AssertNotNull(vessel);
			AssertEquals("8734987", vessel.RV_LloydsNumber);

			var expectedLog = "Info:A new vessel with Name 'MAERSK RIDE' and IMO '8734987' has been created during autorating";
			Assert(logger.Infos.Contains(expectedLog));
		}

		public void TestConvert_WhenVesselDoesNotExist_WhenWarningNotAccepted_ShouldConvertWithEmptyVesselAndFalseIsLinked()
		{
			var carrier = Factory.New<OrgHeader>();

			var scheduleDetails = new[]
			{
				new ScheduleDetail()
				{
					IMONumber = "8734987",
					ServiceName = "Some Service Name",
					ServiceCode = "SC12345",
					TradeLane = "FAR/EUR",
					TransitTime = new TimeSpan(12, 21, 0, 0),
					DepartureDate = new DateTime(2021, 01, 01),
					ArrivalDate = new DateTime(2021, 01, 12, 21, 0, 0),
					VesselName = "MAERSK RIDE",
					VoyageNumber = "45WTG",
					Destination = "USLAX",
					Origin = "HKHKG",
					FlagCode = "UK",
				},
			};

			var vessel = RefVessel.LookupVesselByName("MAERSK RIDE", Factory, true).FirstOrDefault();

			AssertNull(vessel);

			Env.Security.VesselsModify.IsAllowed = true;
			UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.No);

			var logger = new TestLogger();
			var leg = TransportLegConverter.Convert(Factory, carrier, scheduleDetails, logger).First();

			vessel = RefVessel.LookupVesselByName("MAERSK RIDE", Factory, true).FirstOrDefault();

			AssertNull(vessel);
			Assert(!leg.JW_IsLinked);
			AssertNullOrEmpty(leg.JW_Vessel);

			Assert(!logger.Infos.Any());
		}

		public void TestConvert_LegNotes()
		{
			var carrier = Factory.New<OrgHeader>();

			var scheduleDetails = new[]
			{
				new ScheduleDetail()
				{
					ServiceName = "SN Sample",
					ServiceCode = "L35",
					TradeLane = "EUR/ESA",
					TransitTime = new TimeSpan(5, 7, 0, 0),
					FlagCode = "PT",
					DateInfos = new []
					{
						new ScheduleDateInfo()
						{
							Code = "01",
							Name = "Special Cargo Documentation Deadline",
							Date = new DateTime(2021, 01, 08, 11, 0, 0)
						},
						new ScheduleDateInfo()
						{
							Code = "02",
							Name = "Final Loadlist Deadline",
							Date = new DateTime(2021, 01, 08, 18, 0, 0)
						}
					}
				},
			};

			var expectedLegNote = @"Transit Time                         5d 7h
Trade Lane                           EUR/ESA
Service Code                         L35
Service Name                         SN Sample
Flag Code                            PT
Flag Name                            Portugal
Special Cargo Documentation Deadline 2021-01-08 11:00
Final Loadlist Deadline              2021-01-08 18:00
";

			var logger = new TestLogger();
			var leg = TransportLegConverter.Convert(Factory, carrier, scheduleDetails, logger).First();
			AssertEquals(expectedLegNote, leg.JW_LegNotes);
		}

		public void TestConvert_LegNotes_DatePopulation()
		{
			var carrier = Factory.New<OrgHeader>();

			var scheduleDetails = new[]
			{
				new ScheduleDetail()
				{
					DateInfos = new []
					{
						new ScheduleDateInfo()
						{
							Code = "01",
							Name = "Special Cargo Documentation Deadline",
							Date = new DateTime(2021, 01, 08, 11, 0, 0)
						},
						new ScheduleDateInfo()
						{
							Code = "02",
							Name = "Final Loadlist Deadline",
							Date = new DateTime(2021, 01, 08, 18, 0, 0)
						},
						new ScheduleDateInfo()
						{
							Code = "CY",
							Name = "Commercial Cargo Cutoff",
							Type = "Documentation",
							Date = new DateTime(2020, 11, 06, 20, 0, 0)
						},
						new ScheduleDateInfo()
						{
							Code = "SINONAMS",
							Name = "Shipping Instructions Deadline",
							Type = "Documentation",
							Date = new DateTime(2020, 11, 05, 14, 0, 0)
						},
						new ScheduleDateInfo()
						{
							Code = "VGM",
							Name = "VGM Cut Off",
							Type = "Documentation",
							Date = new DateTime(2020, 11, 04, 12, 0, 0)
						},
					}
				},
			};

			var expectedLegNote = @"Special Cargo Documentation Deadline 2021-01-08 11:00
Final Loadlist Deadline              2021-01-08 18:00
";

			var logger = new TestLogger();
			var leg = TransportLegConverter.Convert(Factory, carrier, scheduleDetails, logger).First();
			AssertEquals("Dates that are not VGMCutOff, CTOCutOff or Docs Due will be populated in notes", expectedLegNote, leg.JW_LegNotes);
		}
	}
}
