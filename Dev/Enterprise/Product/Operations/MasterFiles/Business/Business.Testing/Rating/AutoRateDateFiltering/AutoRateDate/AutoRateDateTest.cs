using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AutoRateDate))]
	sealed class AutoRateDateTest : ChargeGroupSettingTest
	{
		public void TestDateTypeList()
		{
			var autoRateDate = new AutoRateDate();
			autoRateDate.Mode = Constants.TransportModes.Sea;
			var expectedCodes = new[] { JobDateTypes.Codes.ArrivalDate, JobDateTypes.Codes.DepartureDate, JobDateTypes.Codes.HouseBillIssueDate, JobDateTypes.Codes.JobOpenDate };
			var actualCodes = autoRateDate.Lookups.DateTypeList.Cast<CodeDescriptionPair>().Select(x => x.Code);

			AssertContainsExactElementsInAnyOrder(expectedCodes, actualCodes);

			autoRateDate.Mode = Constants.TransportModes.Air;
			expectedCodes = new[] { JobDateTypes.Codes.ArrivalDate, JobDateTypes.Codes.DepartureDate, JobDateTypes.Codes.HouseBillIssueDate, JobDateTypes.Codes.JobOpenDate, JobDateTypes.Codes.AWBIssueDate };
			actualCodes = autoRateDate.Lookups.DateTypeList.Cast<CodeDescriptionPair>().Select(x => x.Code);

			AssertContainsExactElementsInAnyOrder(expectedCodes, actualCodes);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = new AutoRateDate();

			result.JobType = "SHP";
			result.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			result.Mode = Constants.TransportModes.Air;
			result.DateType = JobDateTypes.Codes.ArrivalDate;

			return result;
		}

		public void TestDisplayCorrectDateTypeList()
		{
			var autoRateDate = new AutoRateDate();
			autoRateDate.Mode = Constants.TransportModes.All;

			autoRateDate.JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;
			var expectedCodes = new[] { JobDateTypes.Codes.ArrivalDate, JobDateTypes.Codes.DepartureDate, JobDateTypes.Codes.HouseBillIssueDate, JobDateTypes.Codes.FirstContainerGateInDate, JobDateTypes.Codes.LastContainerGateInDate, JobDateTypes.Codes.CFSReceivalStartDate, JobDateTypes.Codes.InterimReceiptDate };
			var actualCodes = autoRateDate.Lookups.DateTypeList.Cast<CodeDescriptionPair>().Select(x => x.Code);
			AssertContainsExactElementsInAnyOrder(expectedCodes, actualCodes);

			autoRateDate.JobType = JobInvoicingConsumerTypes.GatewayConsolCode;
			expectedCodes = new[] { JobDateTypes.Codes.ArrivalDate, JobDateTypes.Codes.DepartureDate, JobDateTypes.Codes.HouseBillIssueDate, JobDateTypes.Codes.FirstContainerGateInDate, JobDateTypes.Codes.LastContainerGateInDate, JobDateTypes.Codes.CFSReceivalStartDate, JobDateTypes.Codes.JobOpenDate, JobDateTypes.Codes.InterimReceiptDate };
			actualCodes = autoRateDate.Lookups.DateTypeList.Cast<CodeDescriptionPair>().Select(x => x.Code);
			AssertContainsExactElementsInAnyOrder(expectedCodes, actualCodes);

			autoRateDate.JobType = JobInvoicingConsumerTypes.CFSLoadListCode;
			expectedCodes = new[] { JobDateTypes.Codes.ArrivalDate, JobDateTypes.Codes.DepartureDate, JobDateTypes.Codes.HouseBillIssueDate, JobDateTypes.Codes.FirstContainerGateInDate, JobDateTypes.Codes.LastContainerGateInDate, JobDateTypes.Codes.CFSReceivalStartDate, JobDateTypes.Codes.JobOpenDate };
			actualCodes = autoRateDate.Lookups.DateTypeList.Cast<CodeDescriptionPair>().Select(x => x.Code);
			AssertContainsExactElementsInAnyOrder(expectedCodes, actualCodes);

			autoRateDate.JobType = JobInvoicingConsumerTypes.ShipmentCode;
			expectedCodes = new[] { JobDateTypes.Codes.ArrivalDate, JobDateTypes.Codes.DepartureDate, JobDateTypes.Codes.HouseBillIssueDate, JobDateTypes.Codes.HBLPlaceOfReceiptArrivalDate, JobDateTypes.Codes.JobOpenDate, JobDateTypes.Codes.InterimReceiptDate, JobDateTypes.Codes.FirstContainerGateInDate, JobDateTypes.Codes.LastContainerGateInDate };
			actualCodes = autoRateDate.Lookups.DateTypeList.Cast<CodeDescriptionPair>().Select(x => x.Code);
			AssertContainsExactElementsInAnyOrder(expectedCodes, actualCodes);

			autoRateDate.JobType = JobInvoicingConsumerTypes.BrokerageCode;
			expectedCodes = new[] { JobDateTypes.Codes.ArrivalDate, JobDateTypes.Codes.DepartureDate, JobDateTypes.Codes.HouseBillIssueDate, JobDateTypes.Codes.JobOpenDate, JobDateTypes.Codes.HBLPlaceOfReceiptArrivalDate };
			actualCodes = autoRateDate.Lookups.DateTypeList.Cast<CodeDescriptionPair>().Select(x => x.Code);
			AssertContainsExactElementsInAnyOrder(expectedCodes, actualCodes);

			autoRateDate.JobType = JobInvoicingConsumerTypes.CFSShipmentCode;
			expectedCodes = new[] { JobDateTypes.Codes.ArrivalDate, JobDateTypes.Codes.DepartureDate, JobDateTypes.Codes.HouseBillIssueDate, JobDateTypes.Codes.JobOpenDate, JobDateTypes.Codes.FirstContainerGateInDate, JobDateTypes.Codes.LastContainerGateInDate };
			actualCodes = autoRateDate.Lookups.DateTypeList.Cast<CodeDescriptionPair>().Select(x => x.Code);
			AssertContainsExactElementsInAnyOrder(expectedCodes, actualCodes);

			autoRateDate.JobType = JobInvoicingConsumerTypes.AgencyBookingCode;
			expectedCodes = new[] { JobDateTypes.Codes.ArrivalDate, JobDateTypes.Codes.DepartureDate, JobDateTypes.Codes.HouseBillIssueDate, JobDateTypes.Codes.JobOpenDate };
			actualCodes = autoRateDate.Lookups.DateTypeList.Cast<CodeDescriptionPair>().Select(x => x.Code);
			AssertContainsExactElementsInAnyOrder(expectedCodes, actualCodes);
		}

		public void TestIsFallbackDisabled_WhenDateTypeIsChangedToUnsupportedValue_ThenFallbackIsAllowed()
		{
			var autoRateDate = new AutoRateDate();
			autoRateDate.JobType = "SHP";
			autoRateDate.Mode = Constants.TransportModes.All;
			autoRateDate.DateType = JobDateTypes.Codes.HouseBillIssueDate;
			AssertEquals("Precondition: IsFallbackDisabled is not read-only", false, autoRateDate.IsFallbackDisabledInfo.ReadOnly);
			autoRateDate.IsFallbackDisabled = true;
			AssertEquals("Precondition: IsFallbackDisabled is true", true, autoRateDate.IsFallbackDisabled);
			autoRateDate.DateType = JobDateTypes.Codes.ArrivalDate;
			AssertEquals("IsFallbackDisabled is reset to false after changing the DateType to a value that does not support IsFallbackDisabled setting", false, autoRateDate.IsFallbackDisabled);
		}

		public void TestIsFallbackDisabled_ReadOnly()
		{
			var autoRateDate = new AutoRateDate();
			autoRateDate.JobType = "SHP";
			autoRateDate.Mode = Constants.TransportModes.All;

			AssertReadOnly(JobDateTypes.Codes.ArrivalDate, true);
			AssertReadOnly(JobDateTypes.Codes.DepartureDate, true);
			AssertReadOnly(JobDateTypes.Codes.HouseBillIssueDate, false);
			AssertReadOnly(JobDateTypes.Codes.JobOpenDate, true);
			AssertReadOnly(JobDateTypes.Codes.AWBIssueDate, true);

			void AssertReadOnly(string dateType, bool expectedReadOnlyValue)
			{
				autoRateDate.DateType = dateType;
				AssertEquals($"IsFallbackDisabled is expected to be {expectedReadOnlyValue} when DateType is {dateType}", expectedReadOnlyValue, autoRateDate.IsFallbackDisabledInfo.ReadOnly);
			}
		}

		public void TestRateTypeList()
		{
			var autoRateDate = new AutoRateDate();
			var expectedCodes = new[] { string.Empty, "REV", "CST" };
			var actualCodes = autoRateDate.Lookups.RateTypeList.Cast<CodeDescriptionPair>().Select(x => x.Code);

			AssertContainsExactElementsInAnyOrder(expectedCodes, actualCodes);
		}

		#region ContainerMode

		public void TestContainerModeList_WhenJobTypeIsConsol()
		{
			TestContainsCorrectContainerModesForJobTypeAndModeForConsol(JobInvoicingConsumerTypes.ForwardingConsolCode);
			TestContainsCorrectContainerModesForJobTypeAndModeForConsol(JobInvoicingConsumerTypes.GatewayConsolCode);

			void TestContainsCorrectContainerModesForJobTypeAndModeForConsol(string consolCode) =>
				CombineAssertions(() =>
				{
					AssertContainerModesForJobTypeAndMode(consolCode,
						Constants.TransportModes.Air,
						new[]
						{
							Constants.ContainerModes.All,
							Constants.ContainerModes.Loose,
							Constants.ContainerModes.ULD,
							Constants.ContainerModes.BuyersConsol,
							Constants.ContainerModes.ShippersConsol,
							Constants.ContainerModes.Other
						});

					AssertContainerModesForJobTypeAndMode(consolCode,
						Constants.TransportModes.Road,
						new[]
						{
							Constants.ContainerModes.All,
							Constants.ContainerModes.FCL,
							Constants.ContainerModes.FTL,
							Constants.ContainerModes.LCL,
							Constants.ContainerModes.LTL,
							Constants.ContainerModes.BuyersConsol,
							Constants.ContainerModes.ShippersConsol,
							Constants.ContainerModes.Groupage,
							Constants.ContainerModes.Other
						});

					AssertContainerModesForJobTypeAndMode(consolCode,
						Constants.TransportModes.Rail,
						new[]
						{
							Constants.ContainerModes.All,
							Constants.ContainerModes.FCL,
							Constants.ContainerModes.LCL,
							Constants.ContainerModes.Bulk,
							Constants.ContainerModes.Liquid,
							Constants.ContainerModes.BreakBulk,
							Constants.ContainerModes.BuyersConsol,
							Constants.ContainerModes.ShippersConsol,
							Constants.ContainerModes.Groupage,
							Constants.ContainerModes.RollOnRollOff,
							Constants.ContainerModes.Other
						});

					AssertContainerModesForJobTypeAndMode(consolCode,
						Constants.TransportModes.Sea,
						new[]
						{
							Constants.ContainerModes.All,
							Constants.ContainerModes.FCL,
							Constants.ContainerModes.LCL,
							Constants.ContainerModes.Bulk,
							Constants.ContainerModes.Liquid,
							Constants.ContainerModes.BreakBulk,
							Constants.ContainerModes.BuyersConsol,
							Constants.ContainerModes.ShippersConsol,
							Constants.ContainerModes.RollOnRollOff,
							Constants.ContainerModes.Other
						});

					AssertContainerModesForJobTypeAndMode(consolCode,
						Constants.TransportModes.Courier,
						Array.Empty<string>());
				});
		}

		public void TestContainerModeList_WhenJobTypeIsShipment()
		{
			CombineAssertions(() =>
			{
				AssertContainerModesForJobTypeAndMode(JobInvoicingConsumerTypes.ShipmentCode,
					Constants.TransportModes.Air,
					new[]
					{
						Constants.ContainerModes.All,
						Constants.ContainerModes.Loose,
						Constants.ContainerModes.ULD,
						Constants.ContainerModes.AgentConsol,
						Constants.ContainerModes.BuyersConsol,
						Constants.ContainerModes.ShippersConsol,
					});

				AssertContainerModesForJobTypeAndMode(JobInvoicingConsumerTypes.ShipmentCode,
					Constants.TransportModes.Sea,
					new[]
					{
						Constants.ContainerModes.All,
						Constants.ContainerModes.FCL,
						Constants.ContainerModes.LCL,
						Constants.ContainerModes.Bulk,
						Constants.ContainerModes.Liquid,
						Constants.ContainerModes.BreakBulk,
						Constants.ContainerModes.BuyersConsol,
						Constants.ContainerModes.ShippersConsol,
						Constants.ContainerModes.RollOnRollOff,
					});

				AssertContainerModesForJobTypeAndMode(JobInvoicingConsumerTypes.ShipmentCode,
					Constants.TransportModes.Road,
					new[]
					{
						Constants.ContainerModes.All,
						Constants.ContainerModes.FCL,
						Constants.ContainerModes.FTL,
						Constants.ContainerModes.LCL,
						Constants.ContainerModes.LTL,
						Constants.ContainerModes.BuyersConsol,
						Constants.ContainerModes.ShippersConsol,
					});

				AssertContainerModesForJobTypeAndMode(JobInvoicingConsumerTypes.ShipmentCode,
					Constants.TransportModes.Rail,
					new[]
					{
						Constants.ContainerModes.All,
						Constants.ContainerModes.FCL,
						Constants.ContainerModes.LCL,
						Constants.ContainerModes.Bulk,
						Constants.ContainerModes.Liquid,
						Constants.ContainerModes.BreakBulk,
						Constants.ContainerModes.BuyersConsol,
						Constants.ContainerModes.ShippersConsol,
					});

				AssertContainerModesForJobTypeAndMode(JobInvoicingConsumerTypes.ShipmentCode,
					Constants.TransportModes.SeaAir,
					new[]
					{
						Constants.ContainerModes.All,
						Constants.ContainerModes.LCL,
						Constants.ContainerModes.Loose,
						Constants.ContainerModes.ULD,
					});

				AssertContainerModesForJobTypeAndMode(JobInvoicingConsumerTypes.ShipmentCode,
					Constants.TransportModes.AirSea,
					new[]
					{
						Constants.ContainerModes.All,
						Constants.ContainerModes.Loose,
						Constants.ContainerModes.ULD,
						Constants.ContainerModes.LCL,
					});

				AssertContainerModesForJobTypeAndMode(JobInvoicingConsumerTypes.ShipmentCode,
					Constants.TransportModes.Courier,
					new[]
					{
						Constants.ContainerModes.All,
						Constants.ContainerModes.OnBoardCourier,
						Constants.ContainerModes.Unaccompanied,
					});
			});
		}

		public void TestContainerModeList_WhenJobTypeIsQuotedBooking()
		{
			CombineAssertions(() =>
			{
				AssertContainerModesForJobTypeAndMode(JobInvoicingConsumerTypes.QuotedBookingCode,
					Constants.TransportModes.Air,
					new[]
					{
						Constants.ContainerModes.All,
						Constants.ContainerModes.Loose,
						Constants.ContainerModes.ULD,
					});

				AssertContainerModesForJobTypeAndMode(JobInvoicingConsumerTypes.QuotedBookingCode,
					Constants.TransportModes.Sea,
					new[]
					{
						Constants.ContainerModes.All,
						Constants.ContainerModes.FCL,
						Constants.ContainerModes.LCL,
					});

				AssertContainerModesForJobTypeAndMode(JobInvoicingConsumerTypes.QuotedBookingCode,
					Constants.TransportModes.Road,
					new[]
					{
						Constants.ContainerModes.All,
						Constants.ContainerModes.FCL,
						Constants.ContainerModes.LCL,
						Constants.ContainerModes.FTL,
						Constants.ContainerModes.LTL,
					});

				AssertContainerModesForJobTypeAndMode(JobInvoicingConsumerTypes.QuotedBookingCode,
					Constants.TransportModes.Rail,
					new[]
					{
						Constants.ContainerModes.All,
						Constants.ContainerModes.FCL,
						Constants.ContainerModes.LCL,
					});

				AssertContainerModesForJobTypeAndMode(JobInvoicingConsumerTypes.QuotedBookingCode,
					Constants.TransportModes.SeaAir,
					Array.Empty<string>());

				AssertContainerModesForJobTypeAndMode(JobInvoicingConsumerTypes.QuotedBookingCode,
					Constants.TransportModes.AirSea,
					Array.Empty<string>());

				AssertContainerModesForJobTypeAndMode(JobInvoicingConsumerTypes.QuotedBookingCode,
					Constants.TransportModes.Courier,
					new[]
					{
						Constants.ContainerModes.All,
						Constants.ContainerModes.OnBoardCourier,
					});
			});
		}

		void AssertContainerModesForJobTypeAndMode(string jobType, string mode, string[] expectedContainerModes)
		{
			var autoRateDate = new AutoRateDate();
			autoRateDate.Mode = mode;
			autoRateDate.JobType = jobType;
			var containerModes = autoRateDate.Lookups.ContainerModeList.Cast<CodeDescriptionPair>().Select(x => x.Code);
			AssertContainsExactElementsInExactOrder($"Incorrect container modes for job type {jobType} and mode {mode}",
				expectedContainerModes,
				containerModes);
		}

		#endregion

		public void TestContainerModeValueSetToEmpty_OnlyWhenContainerModeIsSetToReadOnly_AndAlreadyHasSomeValueSet()
		{
			var autoRateDate = new AutoRateDate();
			autoRateDate.JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;
			autoRateDate.Mode = Constants.TransportModes.Sea;
			AssertEquals("Precondition: Container Mode is not read-only", false, autoRateDate.ContainerModeInfo.ReadOnly);
			autoRateDate.ContainerMode = Constants.ContainerModes.FCL;

			autoRateDate.Mode = Constants.TransportModes.Air; //Keeps ContainerMode active
			AssertEquals("Container Mode should have retained it's value after changing transport mode to another supported value", Constants.ContainerModes.FCL, autoRateDate.ContainerMode);

			autoRateDate.Mode = Constants.TransportModes.Courier; //Sets ContainerMode to Read Only
			AssertEquals("Container Mode should have cleared it's value after being set to read only", ZString.Empty, autoRateDate.ContainerMode);
		}

		public void TestContainerModeDisabled_WhenModeIsChangedToUnsupportedValue_ThenContainerModeIsAllowed()
		{
			var autoRateDate = new AutoRateDate();
			autoRateDate.JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;
			autoRateDate.Mode = Constants.TransportModes.Sea;
			AssertEquals("Precondition: Container Mode is not read-only", false, autoRateDate.ContainerModeInfo.ReadOnly);
			autoRateDate.Mode = Constants.TransportModes.Courier;
			AssertEquals("Container Mode should be read only when Mode is changed to an unsupported value", true, autoRateDate.ContainerModeInfo.ReadOnly);
			autoRateDate.Mode = Constants.TransportModes.Air;
			AssertEquals("Container Mode should not be read only when Mode is changed to a supported value", false, autoRateDate.ContainerModeInfo.ReadOnly);
		}

		public void TestContainerModeDisabled_WhenJobTypeIsChangedToUnsupportedValue_ThenContainerModeIsAllowed()
		{
			var autoRateDate = new AutoRateDate();
			autoRateDate.JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;
			autoRateDate.Mode = Constants.TransportModes.Sea;
			AssertEquals("Precondition: Container Mode is not read-only", false, autoRateDate.ContainerModeInfo.ReadOnly);
			autoRateDate.JobType = JobInvoicingConsumerTypes.ImporterSecurityFilingCode;
			AssertEquals("Container Mode should be read only when Job Type is changed to an unsupported value", true, autoRateDate.ContainerModeInfo.ReadOnly);
			autoRateDate.JobType = JobInvoicingConsumerTypes.GatewayConsolCode;
			AssertEquals("Container Mode should not be read only when Job Type is changed to a supported value", false, autoRateDate.ContainerModeInfo.ReadOnly);
		}
	}
}
