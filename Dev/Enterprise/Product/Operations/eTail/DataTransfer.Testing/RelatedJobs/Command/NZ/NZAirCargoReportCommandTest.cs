using System;
using CargoWise.Application;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs.NZ;

namespace Enterprise.eTail.DataTransfer.Testing
{
	public class NZAirCargoReportCommandTest : BaseHVLVRelatedJobCommandTest
	{
		public override void TestGetRelatedJobName()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.NewZealand))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "NZAKL";
				shipment.JS_TransportMode = TransportModes.Air;
				var command = new NZAirCargoReportCommand(shipment);
				AssertEquals("HVLV AirCargo ICR", command.RelatedJobName);

				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "AUSYD";
				shipment.JS_TransportMode = TransportModes.Air;
				AssertEquals("HVLV AirCargo CRE", command.RelatedJobName);
			}
		}

		protected override string[] ExpectedApplicableLoginCountries => new[] { CountryCodes.NewZealand };

		protected override string[] ExpectedShipmentTransportModes => new[] { TransportModes.Air };

		protected override Directions[] ExpectedShipmentDirections => new[] { Directions.Import, Directions.Export };

		protected override Type ExpectedRelatedJobConverterType => typeof(NZAirCargoReportConverter);

		protected override bool ExpectedShouldValidateWaybill => false;

		protected override bool ExpectedShouldTrackPrimaryFieldChanges => true;

		protected override bool ExpectedNeedPreScreening => true;

		protected override Type ExpectedCustomsJobsType => ObjectFactory.GetType<ICusMAWB>();

		public override void TestUsageCodeAndCategory()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.NewZealand))
			{
				var command = GetCommandForTest();
				var shipment = command.Shipment;

				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "NZAKL";
				AssertUsageCodeAndCategory("Air Import", command, "ANI", "LVD");

				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "AUSYD";
				AssertUsageCodeAndCategory("Air Export", command, "ANC", "LVD");
			}
		}

		protected override BaseHVLVRelatedJobCommand GetCommandForTest()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			HVLVConsignmentHeader.GetOrCreate(shipment);
			return new NZAirCargoReportCommand(shipment);
		}
	}
}
