using System;
using Enterprise.Environment;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.GUI.RateSelector.Models;
using WiseRates.Api.Model;

namespace Enterprise.Rating.Testing.GUITests.RateSelector.Models
{
	public class BookingInfoViewModelTests : RatingTestCase
	{
		public void TestPopulateFromRate()
		{
			var baseDate = Env.Time.CurrentLocalDateTime;
			var rate = new Rate()
			{
				ProviderRateId = "P_129339874",
				BookingInfo = new BookingInfo()
				{
					Schedule = new Schedule()
					{
						DepartureDate = baseDate,
						ArrivalDate = baseDate.AddDays(21).AddHours(11),
						VesselName = "V For Vendetta",
						VoyageNumber = "V9829",

						ScheduleDetails = new[]
						{
							new ScheduleDetail()
							{
								Origin = "AUSYD",
								Destination = "HKHKG",
							},
							new ScheduleDetail()
							{
								Origin = "HKHKG",
								Destination = "SGSIN",
							},
							new ScheduleDetail()
							{
								Origin = "SGSIN",
								Destination = "USLAX",
							}
						}
					},
				}
			};

			var viewModel = BookingInfoViewModel.PopulateFromRate(rate);

			AssertEquals("RateId should be set correctly", "P_129339874", viewModel.RateId);
			AssertEquals("TransitTime should match total seconds", new TimeSpan(21, 11, 0, 0).TotalSeconds, viewModel.TransitTime.TotalSeconds);
			AssertEquals("Origin should match the first segment's Origin", "AUSYD", viewModel.Origin);
			AssertEquals("Destination should match the last segment's Destination", "USLAX", viewModel.Destination);
			AssertEquals("VesselName should be set correctly", "V For Vendetta", viewModel.VesselName);
			AssertEquals("VoyageNumber should be set correctly", "V9829", viewModel.VoyageNumber);
			AssertEquals("DepartureTime should be set correctly", baseDate, viewModel.DepartureTime);
			AssertEquals("ArrivalTime should be set correctly", baseDate.AddDays(21).AddHours(11), viewModel.ArrivalTime);
		}

		public void TestPopulateBookingTerms()
		{
			var rate = new Rate()
			{
				BookingInfo = new BookingInfo()
				{
					BookingTerms = new BookingTerms()
					{
						Items = new[]
						{
							new BookingTermItem()
							{
								Type = "Fee",
								Name = "Some Fee",
								Fee = 10,
								Currency = "USD"
							}
						}
					}
				}
			};

			var bookingTermViewModel = BookingInfoViewModel.PopulateFromRate(rate).BookingTerms[0];

			AssertEquals("Currency should match", "USD", bookingTermViewModel.Currency);
			AssertEquals("Name should match", "Some Fee", bookingTermViewModel.Name);
			AssertEquals("Fee should match", "10", bookingTermViewModel.Fee);
		}

		public void TestPopulatePenalties()
		{
			var rate = new Rate()
			{
				BookingInfo = new BookingInfo()
				{
					Penalties = new[]
					{
						new Penalty()
						{
							Type = "Some Type",
							Name = "Some Penalty",
							Currency = "USD",
							Direction = "Import",
							StartDay = 6,
							EndDay = 16,
							PerUnitRate = 12.4m
						}
					}
				}
			};

			var penaltyViewModel = BookingInfoViewModel.PopulateFromRate(rate).Penalties[0];
			AssertEquals("Penalty type mismatch", "Some Type", penaltyViewModel.Type);
			AssertEquals("Penalty name mismatch", "Some Penalty", penaltyViewModel.Name);
			AssertEquals("Penalty currency mismatch", "USD", penaltyViewModel.Currency);
			AssertEquals("Penalty direction mismatch", "Import", penaltyViewModel.Direction);
			AssertEquals("Free time mismatch", 5m, penaltyViewModel.FreeTime);
			AssertEquals("Per unit rate mismatch", 12.4m, penaltyViewModel.PerUnitRate);
		}

		public void TestPopulateTransportLegs()
		{
			var baseDate = Env.Time.CurrentLocalDateTime;
			var rate = new Rate()
			{
				BookingInfo = new BookingInfo()
				{
					Schedule = new Schedule()
					{
						ScheduleDetails = new[]
						{
							new ScheduleDetail()
							{
								IMONumber = "12345",
								ServiceName = "Some Service Name",
								ServiceCode = "SC12345",
								TradeLane = "FAR/EUR",
								TransitTime = new TimeSpan(12, 21, 0, 0),
								ArrivalDate = baseDate,
								DepartureDate = baseDate.AddDays(-12).AddHours(-21),
								VesselName = "MAERSK RIDE",
								VoyageNumber = "45WTG",
								Destination = "USLAX",
								Origin = "HKHKG",
								FlagCode = "UK",
								DateInfos = new[]
								{
									new ScheduleDateInfo()
									{
										Code = "012",
										Name = "Some Deadline",
										Type = "Documentation",
										Date = baseDate.AddDays(-5)
									}
								}
							}
						}
					}
				}
			};

			var transportLegViewModel = BookingInfoViewModel.PopulateFromRate(rate).TransportLegs[0];
			AssertEquals(
				"TransitTime value should match expected total seconds.",
				new TimeSpan(12, 21, 0, 0).TotalSeconds,
				transportLegViewModel.TransitTime.Value.TotalSeconds
			);
			AssertEquals("LegOrder should be 1.", 1, transportLegViewModel.LegOrder);
			AssertEquals("VoyageNumber should be '45WTG'.", "45WTG", transportLegViewModel.VoyageNumber);
			AssertEquals(
				"EstimatedDeparture should match expected value.",
				baseDate.AddDays(-12).AddHours(-21),
				transportLegViewModel.EstimatedDeparture
			);
			AssertEquals(
				"EstimatedArrival should match expected value.",
				baseDate,
				transportLegViewModel.EstimatedArrival
			);
			AssertEquals(
				"PortOfLoadingCode should be 'HKHKG'.",
				"HKHKG",
				transportLegViewModel.PortOfLoadingCode
			);
			AssertEquals(
				"PortOfDischargeCode should be 'USLAX'.",
				"USLAX",
				transportLegViewModel.PortOfDischargeCode
			);
			AssertEquals(
				"TransportMode should be TransportMode.Sea.",
				TransportMode.Sea,
				transportLegViewModel.TransportMode
			);
			AssertEquals(
				"VesselName should be 'MAERSK RIDE'.",
				"MAERSK RIDE",
				transportLegViewModel.VesselName
			);
			AssertEquals(
				"TradeLane should be 'FAR/EUR'.",
				"FAR/EUR",
				transportLegViewModel.TradeLane
			);
			AssertEquals(
				"FlagCode should be 'UK'.",
				"UK",
				transportLegViewModel.FlagCode
			);

			var transportLegDateInfoViewModel = transportLegViewModel.DateInfos[0];
			AssertEquals("Code should be '012'.", "012", transportLegDateInfoViewModel.Code);
			AssertEquals("Name should be 'Some Deadline'.", "Some Deadline", transportLegDateInfoViewModel.Name);
			AssertEquals("Type should be 'Documentation'.", "Documentation", transportLegDateInfoViewModel.Type);
			AssertEquals(
				"Date should match expected value.",
				baseDate.AddDays(-5),
				transportLegDateInfoViewModel.Date
			);
		}
	}
}
