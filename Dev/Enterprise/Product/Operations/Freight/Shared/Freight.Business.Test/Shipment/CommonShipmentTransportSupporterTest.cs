using System;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CommonShipmentTransportSupporterTest : TransportSupporterTestCase<CommonShipmentTransportSupporter<CommonShipment>>
	{
		[RunInExtraTransaction]
		public void TestConsignmentRef()
		{
			ITransportParent parent = Factory.New<CommonShipment>();
			TransportSupporter supporter = parent.TransportSupporter;

			AssertEquals("", supporter.ConsignmentRef);

			parent.TransportSupporter.SetConsignmentRefIfNotSet();
			Assert("ConsignmentRef should now be populated from the number fountain", Regex.IsMatch(supporter.ConsignmentRef, "S[0-9]{8}"));
		}

		public void TestGetNewTransportValidator()
		{
			CommonShipment parent = Factory.New<CommonShipment>();
			Transport transport = parent.Transports.AddNew();
			TransportSupporter supporter = ((ITransportParent)parent).TransportSupporter;

			AssertType(typeof(ShipmentTransportValidation), supporter.GetNewTransportValidator(transport));
		}

		public void TestNotifyVoyageUpdated_ShouldCreateOrUpdateCargoAvailableEventOnShipment()
		{
			Action<ZString, ZDateTime, ZDateTime, ZDateTime> assert = (containerMode, fclDate, lclDate, expectedDate) =>
			{
				var shipment = Factory.New<CommonShipment>();
				shipment.JS_PackingMode = containerMode;
				shipment.DocsAndCartage.JP_FCLAvailable = fclDate;
				shipment.DocsAndCartage.JP_LCLAvailable = lclDate;
				shipment.Logs.RemoveAndDeleteAll();

				var supporter = new CommonShipmentTransportSupporter<CommonShipment>(shipment);
				supporter.NotifyVoyageUpdated(null);

				var log = shipment.Logs.MostRecentLogByEventTime(Events.CargoAvailable);

				if (!expectedDate.IsEmpty)
				{
					AssertNotNull(string.Format("{0} Log", Events.CargoAvailable), log);
					AssertEquals("Event time", expectedDate, log.SL_EventTime);
				}
				else
				{
					AssertNull(string.Format("{0} Log", Events.CargoAvailable), log);
				}
			};

			assert(Constants.ContainerModes.FCL, ZDateTime.Empty, 2.DaysAgo(), ZDateTime.Empty);
			assert(Constants.ContainerModes.FCL, 1.DaysAgo(), 2.DaysAgo(), 1.DaysAgo());
			assert(Constants.ContainerModes.LCL, 1.DaysAgo(), ZDateTime.Empty, ZDateTime.Empty);
			assert(Constants.ContainerModes.LCL, 1.DaysAgo(), 2.DaysAgo(), 2.DaysAgo());
		}

		public void TestIsArrivalContainerModeFCLorULD_ComplexTest()
		{
			var consol = Factory.New<CommonConsol>();
			var shipment = consol.Shipments.AddNew();
			var supporter = new CommonShipmentTransportSupporter<CommonShipment>(shipment);

			foreach (var mode in Constants.ContainerModes.FCLTypes)
			{
				shipment.JS_PackingMode = mode;
				AssertEquals(string.Format("The value when the container mode is '{0}'", mode), true, supporter.IsArrivalContainerModeFCLorULD);
			}

			foreach (var mode in Constants.ContainerModes.LCLTypes)
			{
				shipment.JS_PackingMode = mode;
				AssertEquals(string.Format("The value when the container mode is '{0}'", mode), false, supporter.IsArrivalContainerModeFCLorULD);
			}

			shipment.JS_PackingMode = Constants.ContainerModes.LCLTypes[0];
			consol.JK_ConsolMode = Constants.ContainerModes.BuyersConsol;
			AssertEquals(string.Format("The value when the consol is '{0}'", Constants.ContainerModes.BuyersConsol), true, supporter.IsArrivalContainerModeFCLorULD);
		}

		public void TestIsDepartureContainerModeFCLorULD_ComplexTest()
		{
			var consol = Factory.New<CommonConsol>();
			var shipment = consol.Shipments.AddNew();
			var supporter = new CommonShipmentTransportSupporter<CommonShipment>(shipment);

			foreach (var mode in Constants.ContainerModes.FCLTypes)
			{
				shipment.JS_PackingMode = mode;
				AssertEquals(string.Format("The value when the container mode is '{0}'", mode), true, supporter.IsDepartureContainerModeFCLorULD);
			}

			foreach (var mode in Constants.ContainerModes.LCLTypes)
			{
				shipment.JS_PackingMode = mode;
				AssertEquals(string.Format("The value when the container mode is '{0}'", mode), false, supporter.IsDepartureContainerModeFCLorULD);
			}

			shipment.JS_PackingMode = Constants.ContainerModes.LCLTypes[0];
			consol.JK_ConsolMode = Constants.ContainerModes.BuyersConsol;
			AssertEquals(string.Format("The value when the consol is '{0}'", Constants.ContainerModes.BuyersConsol), false, supporter.IsDepartureContainerModeFCLorULD);
		}

		public void TestShipmentInspectionTypeIsRecalculatedOnLoadOrModeChange()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			{
				var shipment = (CommonShipment)Factory.New<IForwardingShipment>();

				var frAccountConsignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.StartsWith, "FR"));
				var approval = frAccountConsignor.MainAddress.KnownShipperDetails.AddNew();
				approval.OV_OH_OrgHeader = frAccountConsignor.PK;
				approval.OV_EXApprovedOrMajorExporter = "AC";

				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "FRPAR";
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = frAccountConsignor.MainAddress.PK;

				var transport = shipment.Transports.AddNew();
				transport.JW_TransportMode = "AIR";
				transport.JW_RL_NKLoadPort = "FRPAR";
				transport.JW_RL_NKDiscPort = "SGSIN";

				AssertEquals("Shipment is approved for export from France", "APP", shipment.JS_InspectionTypeCode);

				transport.JW_RL_NKLoadPort = "BEBRU";
				AssertEquals("Shipment is not approved for uplift in Belgium", "UNK", shipment.JS_InspectionTypeCode);

				transport.JW_TransportMode = "ROA";
				AssertEquals("Shipment is approved for road freight in Belgium", "APP", shipment.JS_InspectionTypeCode);
			}
		}

		public void TestShipmentInspectionTypeIsNotRecalculatedOnLoadChangeForUK()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var shipment = (CommonShipment)Factory.New<IForwardingShipment>();

				var ukAccountConsignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.StartsWith, "GB"));
				var approval = ukAccountConsignor.MainAddress.KnownShipperDetails.AddNew();
				approval.OV_OH_OrgHeader = ukAccountConsignor.PK;
				approval.OV_EXApprovedOrMajorExporter = "KC";
				approval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(5);

				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "GBLON";
				shipment.JS_InspectionTypeCode = "APP";
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = ukAccountConsignor.MainAddress.PK;

				var transport = shipment.Transports.AddNew();
				transport.JW_TransportMode = Core.Constants.TransportModes.Air;
				transport.JW_RL_NKLoadPort = "GBLON";

				AssertEquals("Shipment is approved for export from United Kingdom", "APP", shipment.JS_InspectionTypeCode);

				transport.JW_RL_NKLoadPort = "FRPAR";
				AssertEquals("Shipment inspection type remains APP", "APP", shipment.JS_InspectionTypeCode);
			}
		}

		public void TestShipmentInspectionTypeIsNotRecalculatedOnModeChangeForUK()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var shipment = (CommonShipment)Factory.New<IForwardingShipment>();

				var ukAccountConsignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.StartsWith, "GB"));
				var approval = ukAccountConsignor.MainAddress.KnownShipperDetails.AddNew();
				approval.OV_OH_OrgHeader = ukAccountConsignor.PK;
				approval.OV_EXApprovedOrMajorExporter = "KC";
				approval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(5);

				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "GBLON";
				shipment.JS_InspectionTypeCode = "APP";
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = ukAccountConsignor.MainAddress.PK;

				var transport = shipment.Transports.AddNew();
				transport.JW_TransportMode = Core.Constants.TransportModes.Air;
				transport.JW_RL_NKLoadPort = "GBLON";

				AssertEquals("Shipment is approved for export from United Kingdom", "APP", shipment.JS_InspectionTypeCode);

				transport.JW_TransportMode = "ROA";
				AssertEquals("Shipment inspection type remains APP", "APP", shipment.JS_InspectionTypeCode);
			}
		}

		protected override SecurityCheckpoint ExpectedDistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceForwarding; }
		}

		protected override TransportSupporter GetNewTransportSupporter()
		{
			ITransportParent parent = Factory.New<CommonShipment>();
			return parent.TransportSupporter;
		}

		protected override ZString TestingCountry
		{
			get { return Core.Constants.CountryCodes.Australia; }
		}
	}
}
