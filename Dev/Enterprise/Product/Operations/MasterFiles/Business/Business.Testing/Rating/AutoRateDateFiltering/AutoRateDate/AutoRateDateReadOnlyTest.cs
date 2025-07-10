using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AutoRateDateReadOnlyTest : JobConfigurationSelectorReadOnlyTest
	{
		public void TestLocationReadOnlyMode()
		{
			var bizO = BizObj as AutoRateDate;
			AssertNotNull(bizO);

			bizO.JobType = JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All;
			AssertEquals(true, bizO.LocationInfo.ReadOnly);

			bizO.JobType = JobInvoicingConsumerTypes.ShipmentCode;
			AssertEquals(false, bizO.LocationInfo.ReadOnly);

			bizO.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			AssertEquals(true, bizO.LocationInfo.ReadOnly);

			bizO.DirectionCode = Constants.FreightShipmentDirection.Code.Export;
			AssertEquals(false, bizO.LocationInfo.ReadOnly);

			bizO.JobType = JobInvoicingConsumerTypes.ShipmentCode;
			bizO.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			AssertEquals(true, bizO.LocationInfo.ReadOnly);
		}

		public void TestContainerModeReadOnly_ForShipment()
		{
			var bizO = BizObj as AutoRateDate;
			AssertNotNull(bizO);

			AssertContainerModeReadOnly(JobInvoicingConsumerTypes.ShipmentCode, Constants.TransportModes.Road, false);
			AssertContainerModeReadOnly(JobInvoicingConsumerTypes.ShipmentCode, Constants.TransportModes.Sea, false);
			AssertContainerModeReadOnly(JobInvoicingConsumerTypes.ShipmentCode, Constants.TransportModes.Air, false);
			AssertContainerModeReadOnly(JobInvoicingConsumerTypes.ShipmentCode, Constants.TransportModes.Rail, false);
			AssertContainerModeReadOnly(JobInvoicingConsumerTypes.ShipmentCode, Constants.TransportModes.AirSea, false);
			AssertContainerModeReadOnly(JobInvoicingConsumerTypes.ShipmentCode, Constants.TransportModes.SeaAir, false);
			AssertContainerModeReadOnly(JobInvoicingConsumerTypes.ShipmentCode, Constants.TransportModes.Courier, false);
			AssertContainerModeReadOnly(JobInvoicingConsumerTypes.ShipmentCode, Constants.TransportModes.Mail, true);
		}

		public void TestContainerModeReadOnly_ForConsol()
		{
			var bizO = BizObj as AutoRateDate;
			AssertNotNull(bizO);

			AssertContainerModeReadOnly(JobInvoicingConsumerTypes.ForwardingConsolCode, Constants.TransportModes.Road, false);
			AssertContainerModeReadOnly(JobInvoicingConsumerTypes.ForwardingConsolCode, Constants.TransportModes.Sea, false);
			AssertContainerModeReadOnly(JobInvoicingConsumerTypes.ForwardingConsolCode, Constants.TransportModes.Air, false);
			AssertContainerModeReadOnly(JobInvoicingConsumerTypes.ForwardingConsolCode, Constants.TransportModes.Rail, false);
			AssertContainerModeReadOnly(JobInvoicingConsumerTypes.ForwardingConsolCode, Constants.TransportModes.Pedestrian, true);

			AssertContainerModeReadOnly(JobInvoicingConsumerTypes.GatewayConsolCode, Constants.TransportModes.Road, false);
			AssertContainerModeReadOnly(JobInvoicingConsumerTypes.GatewayConsolCode, Constants.TransportModes.Sea, false);
			AssertContainerModeReadOnly(JobInvoicingConsumerTypes.GatewayConsolCode, Constants.TransportModes.Air, false);
			AssertContainerModeReadOnly(JobInvoicingConsumerTypes.GatewayConsolCode, Constants.TransportModes.Rail, false);
			AssertContainerModeReadOnly(JobInvoicingConsumerTypes.GatewayConsolCode, Constants.TransportModes.Pedestrian, true);
		}

		public void TestContainerModeReadOnly_ForQuotedBooking()
		{
			var bizO = BizObj as AutoRateDate;
			AssertNotNull(bizO);

			AssertContainerModeReadOnly(JobInvoicingConsumerTypes.QuotedBookingCode, Constants.TransportModes.Sea, false);
			AssertContainerModeReadOnly(JobInvoicingConsumerTypes.QuotedBookingCode, Constants.TransportModes.Air, false);
			AssertContainerModeReadOnly(JobInvoicingConsumerTypes.QuotedBookingCode, Constants.TransportModes.Road, false);
			AssertContainerModeReadOnly(JobInvoicingConsumerTypes.QuotedBookingCode, Constants.TransportModes.Rail, false);
			AssertContainerModeReadOnly(JobInvoicingConsumerTypes.QuotedBookingCode, Constants.TransportModes.Courier, false);
			AssertContainerModeReadOnly(JobInvoicingConsumerTypes.QuotedBookingCode, Constants.TransportModes.All, true);
		}

		public void TestContainerModeReadOnly_ForUnsupportedJobType()
		{
			var bizO = BizObj as AutoRateDate;
			AssertNotNull(bizO);

			AssertContainerModeReadOnly(JobInvoicingConsumerTypes.FCLStorageCode, Constants.TransportModes.Sea, true);
			AssertContainerModeReadOnly(JobInvoicingConsumerTypes.FCLStorageCode, Constants.TransportModes.Air, true);
			AssertContainerModeReadOnly(JobInvoicingConsumerTypes.FCLStorageCode, Constants.TransportModes.Road, true);
			AssertContainerModeReadOnly(JobInvoicingConsumerTypes.FCLStorageCode, Constants.TransportModes.Rail, true);
			AssertContainerModeReadOnly(JobInvoicingConsumerTypes.FCLStorageCode, Constants.TransportModes.Courier, true);
			AssertContainerModeReadOnly(JobInvoicingConsumerTypes.FCLStorageCode, Constants.TransportModes.Mail, true);
		}

		void AssertContainerModeReadOnly(string jobType, string mode, bool expectedReadOnly)
		{
			var bizO = BizObj as AutoRateDate;
			bizO.Mode = mode;
			bizO.JobType = jobType;
			AssertEquals($"Incorrect containerMode readonly for JobType {jobType} and Mode {mode}", expectedReadOnly, bizO.ContainerModeInfo.ReadOnly);
		}

		public void TestContainerModeRestore()
		{
			AssertContainerModeRestore(Constants.ContainerModes.All,
				JobInvoicingConsumerTypes.ForwardingConsolCode,
				Constants.TransportModes.Sea);

			AssertContainerModeRestore(Constants.ContainerModes.AIR,
				JobInvoicingConsumerTypes.ForwardingConsolCode,
				Constants.TransportModes.Air);

			AssertContainerModeRestore(Constants.ContainerModes.FCL,
				JobInvoicingConsumerTypes.ForwardingConsolCode,
				Constants.TransportModes.Road);

			AssertContainerModeRestore(Constants.ContainerModes.LCL,
				JobInvoicingConsumerTypes.ForwardingConsolCode,
				Constants.TransportModes.Rail);

			AssertContainerModeRestore(Constants.ContainerModes.All,
				JobInvoicingConsumerTypes.GatewayConsolCode,
				Constants.TransportModes.Sea);

			AssertContainerModeRestore(Constants.ContainerModes.AIR,
				JobInvoicingConsumerTypes.GatewayConsolCode,
				Constants.TransportModes.Air);

			AssertContainerModeRestore(Constants.ContainerModes.FCL,
				JobInvoicingConsumerTypes.GatewayConsolCode,
				Constants.TransportModes.Road);

			AssertContainerModeRestore(Constants.ContainerModes.LCL,
				JobInvoicingConsumerTypes.GatewayConsolCode,
				Constants.TransportModes.Rail);

			void AssertContainerModeRestore(ZString containerMode, ZString jobType, ZString mode)
			{
				var bizO = new AutoRateDate();
				AssertNotNull(bizO);
				bizO.ContainerMode = containerMode;
				bizO.JobType = jobType;
				AssertEquals("ContainerMode is assigned as empty after assigning JobType, because Mode is empty at the stage", ZString.Empty, bizO.ContainerMode);
				bizO.Mode = mode;
				AssertEquals("ContainerMode is restored as origin value after assigning Mode", containerMode, bizO.ContainerMode);
			}
		}

		protected override IJobConfigurationSelector GetNewBizObj => new AutoRateDate();
	}
}
