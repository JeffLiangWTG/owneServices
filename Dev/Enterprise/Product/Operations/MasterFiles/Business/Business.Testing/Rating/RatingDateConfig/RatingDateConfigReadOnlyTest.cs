using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business.Rating.Test
{
	public class RatingDateConfigReadOnlyTest : TestCaseWithFactory
	{
		public void TestDirectionReadOnly()
		{
			RatingDateConfig.RDT_Direction = Constants.FreightShipmentDirection.Code.All;
			RatingDateConfig.RDT_JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;
			AssertEquals("DirectionInfo.ReadOnly", expected: false, actual: RatingDateConfig.RDT_DirectionInfo.ReadOnly);
			AssertEquals("DirectionInfo.IsEmpty", expected: false, actual: RatingDateConfig.RDT_Direction.IsEmpty);

			RatingDateConfig.RDT_JobType = JobInvoicingConsumerTypes.GatewayConsolCode;
			AssertEquals("DirectionInfo.ReadOnly", expected: false, actual: RatingDateConfig.RDT_DirectionInfo.ReadOnly);
			AssertEquals("DirectionInfo.IsEmpty", expected: false, actual: RatingDateConfig.RDT_Direction.IsEmpty);

			using (NUnit.Framework.TestingState.SuspendIsRunningTests())
			{
				RatingDateConfig.RDT_JobType = "!@#";
				AssertEquals("DirectionInfo.ReadOnly", expected: true, actual: RatingDateConfig.RDT_DirectionInfo.ReadOnly);
				AssertEquals("DirectionInfo.IsEmpty", expected: true, actual: RatingDateConfig.RDT_Direction.IsEmpty);
			}

			RatingDateConfig.RDT_JobType = "";
			AssertEquals("DirectionInfo.ReadOnly", expected: false, actual: RatingDateConfig.RDT_DirectionInfo.ReadOnly);
			AssertEquals("DirectionInfo.IsEmpty", expected: false, actual: RatingDateConfig.RDT_Direction.IsEmpty);
		}

		public void TestModeReadOnly()
		{
			RatingDateConfig.RDT_TransportMode = Constants.TransportModes.Sea;
			RatingDateConfig.RDT_JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;
			AssertEquals("RDT_TransportModeInfo.ReadOnly", expected: false, actual: RatingDateConfig.RDT_TransportModeInfo.ReadOnly);
			AssertEquals("RDT_TransportModeInfo.IsEmpty", expected: false, actual: RatingDateConfig.RDT_TransportMode.IsEmpty);

			RatingDateConfig.RDT_JobType = JobInvoicingConsumerTypes.GatewayConsolCode;
			AssertEquals("RDT_TransportModeInfo.ReadOnly", expected: false, actual: RatingDateConfig.RDT_TransportModeInfo.ReadOnly);
			AssertEquals("RDT_TransportModeInfo.IsEmpty", expected: false, actual: RatingDateConfig.RDT_TransportMode.IsEmpty);

			RatingDateConfig.RDT_JobType = "";
			AssertEquals("RDT_TransportModeInfo.ReadOnly", expected: false, actual: RatingDateConfig.RDT_TransportModeInfo.ReadOnly);
			AssertEquals("RDT_TransportModeInfo.IsEmpty", expected: false, actual: RatingDateConfig.RDT_TransportMode.IsEmpty);

			using (NUnit.Framework.TestingState.SuspendIsRunningTests())
			{
				RatingDateConfig.RDT_JobType = "!@#";
				AssertEquals("RDT_TransportModeInfo.ReadOnly", expected: true, actual: RatingDateConfig.RDT_TransportModeInfo.ReadOnly);
				AssertEquals("RDT_TransportModeInfo.IsEmpty", expected: true, actual: RatingDateConfig.RDT_TransportMode.IsEmpty);
			}
		}

		public void TestLocationReadOnlyMode()
		{
			RatingDateConfig.RDT_Direction = "";

			RatingDateConfig.RDT_JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;
			AssertEquals(expected: false, actual: RatingDateConfig.RDT_LocationInfo.ReadOnly);

			RatingDateConfig.RDT_Direction = Constants.FreightShipmentDirection.Code.All;
			AssertEquals(expected: true, actual: RatingDateConfig.RDT_LocationInfo.ReadOnly);

			RatingDateConfig.RDT_Direction = Constants.FreightShipmentDirection.Code.Export;
			AssertEquals(expected: false, actual: RatingDateConfig.RDT_LocationInfo.ReadOnly);

			RatingDateConfig.RDT_JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;
			RatingDateConfig.RDT_Direction = Constants.FreightShipmentDirection.Code.All;
			AssertEquals(expected: true, actual: RatingDateConfig.RDT_LocationInfo.ReadOnly);
		}

		public void TestContainerModeReadOnly()
		{
			RatingDateConfig.RDT_JobType = JobInvoicingConsumerTypes.GatewayConsolCode;
			AssertContainerModeReadOnly(isReadOnlyJobType: false);

			RatingDateConfig.RDT_JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;
			AssertContainerModeReadOnly(isReadOnlyJobType: false);

			void AssertContainerModeReadOnly(ZBool isReadOnlyJobType)
			{
				RatingDateConfig.RDT_TransportMode = Constants.TransportModes.Sea;
				AssertEquals(expected: isReadOnlyJobType, actual: RatingDateConfig.RDT_ContainerModeInfo.ReadOnly);
			}
		}

		public void TestIsFallbackDisabled_WhenDateTypeIsChangedToUnsupportedValue_ThenFallbackIsAllowed()
		{
			RatingDateConfig.RDT_JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;
			RatingDateConfig.RDT_TransportMode = Constants.TransportModes.All;
			RatingDateConfig.RDT_AutoratingDate = JobDateTypes.Codes.HouseBillIssueDate;
			AssertEquals("Precondition: IsFallbackDisabled is not read-only", expected: false, actual: RatingDateConfig.RDT_NoFallbackInfo.ReadOnly);
			RatingDateConfig.RDT_NoFallback = true;
			AssertEquals("Precondition: IsFallbackDisabled is true", expected: true, actual: RatingDateConfig.RDT_NoFallback);
			RatingDateConfig.RDT_AutoratingDate = JobDateTypes.Codes.ArrivalDate;
			AssertEquals("IsFallbackDisabled is reset to false after changing the DateType to a value that does not support IsFallbackDisabled setting", false, RatingDateConfig.RDT_NoFallback);
		}

		public void TestIsFallbackDisabled_ReadOnly()
		{
			RatingDateConfig.RDT_JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;
			RatingDateConfig.RDT_TransportMode = Constants.TransportModes.All;

			AssertReadOnly(JobDateTypes.Codes.ArrivalDate, expectedReadOnlyValue: true);
			AssertReadOnly(JobDateTypes.Codes.DepartureDate, expectedReadOnlyValue: true);
			AssertReadOnly(JobDateTypes.Codes.HouseBillIssueDate, expectedReadOnlyValue: false);
			AssertReadOnly(JobDateTypes.Codes.JobOpenDate, expectedReadOnlyValue: true);
			AssertReadOnly(JobDateTypes.Codes.AWBIssueDate, expectedReadOnlyValue: true);

			void AssertReadOnly(string dateType, bool expectedReadOnlyValue)
			{
				RatingDateConfig.RDT_AutoratingDate = dateType;
				AssertEquals($"IsFallbackDisabled is expected to be {expectedReadOnlyValue} when DateType is {dateType}", expectedReadOnlyValue, RatingDateConfig.RDT_NoFallbackInfo.ReadOnly);
			}
		}

		public void TestContainerModeValueSetToEmpty_OnlyWhenContainerModeIsSetToReadOnly_AndAlreadyHasSomeValueSet()
		{
			RatingDateConfig.RDT_JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;
			RatingDateConfig.RDT_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Container Mode", expected: false, actual: RatingDateConfig.RDT_ContainerModeInfo.ReadOnly);
		}

		public void TestContainerModeDisabled_WhenModeIsChangedToUnsupportedValue_ThenContainerModeIsAllowed()
		{
			RatingDateConfig.RDT_JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;
			RatingDateConfig.RDT_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Precondition: Container Mode is not read-only", expected: false, actual: RatingDateConfig.RDT_ContainerModeInfo.ReadOnly);
			RatingDateConfig.RDT_TransportMode = Constants.TransportModes.Courier;
			AssertEquals("Container Mode should be read only when Mode is changed to an unsupported value", expected: true, actual: RatingDateConfig.RDT_ContainerModeInfo.ReadOnly);
			RatingDateConfig.RDT_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Container Mode should not be read only when Mode is changed to a supported value", expected: false, actual: RatingDateConfig.RDT_ContainerModeInfo.ReadOnly);
		}

		public void TestContainerModeDisabled_WhenJobTypeIsChangedToUnsupportedValue_ThenContainerModeIsAllowed()
		{
			RatingDateConfig.RDT_JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;
			RatingDateConfig.RDT_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Precondition: Container Mode is not read-only", expected: false, actual: RatingDateConfig.RDT_ContainerModeInfo.ReadOnly);
			RatingDateConfig.RDT_JobType = JobInvoicingConsumerTypes.ImporterSecurityFilingCode;
			AssertEquals("Container Mode should be read only when Job Type is changed to an unsupported value", expected: true, actual: RatingDateConfig.RDT_ContainerModeInfo.ReadOnly);
			RatingDateConfig.RDT_JobType = JobInvoicingConsumerTypes.GatewayConsolCode;
			AssertEquals("Container Mode should not be read only when Job Type is changed to a supported value", expected: false, actual: RatingDateConfig.RDT_ContainerModeInfo.ReadOnly);
		}

		#region Implementation

		RatingDateConfig RatingDateConfig => ratingDateConfig ?? (ratingDateConfig = Factory.NewWithValidTestData<RatingDateConfig>());
		RatingDateConfig ratingDateConfig;

		#endregion
	}
}

