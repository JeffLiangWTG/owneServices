using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using ShipmentTypes = Enterprise.Core.Constants.ShipmentTypes;
using TransportModes = Enterprise.Core.Constants.TransportModes;

namespace Enterprise.eTail.DataTransfer.Testing
{
	public class HVLVConvertShipmentToCustomsJobProcessorTest : TestCaseWithFactory
	{
		public void TestProcessData_CheckDataIsReadyForConvertToCustomsJob()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			var processor = new HVLVConvertShipmentToCustomsJobProcessor(shipment, WorkflowTriggerActionTypeConstants.Codes.NZSeaCargoReport);
			var logs = new NotificationCollection();

			processor.Process(logs);
			Assert(logs.Contains("Please make sure there's at least one active consignment on this shipment."));

			var consignment = consignmentHeader.Consignments.AddNew();
			processor.Process(logs);
			Assert(logs.Contains("Please make sure there is a consolidation attached to this shipment."));

			var consol = Factory.New<ForwardingConsol>();
			shipment.JS_TransportMode = "SEA";
			consol.Shipments.Add(shipment);
			consignment.Items.AddNew();
			processor.Process(logs);
			Assert(logs.Contains("Please enter container number for Item(s)."));
		}

		public void TestProcessData_WillAddApplicationLockForShipment()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();

			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Sea;

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.Items.AddNew();

			Factory.Save();

			var logs = new NotificationCollection();
			var connection = Db.NewExtraConnectionToMainDb();
			var appLockKey = ("NZSeaCargoReportCommand," + shipment.PK.ToString()).ToUpperInvariant();

			Assert(connection.TryGetLock(appLockKey, out var appLock));
			using (appLock)
			{
				var processor = new HVLVConvertShipmentToCustomsJobProcessor(shipment, WorkflowTriggerActionTypeConstants.Codes.NZSeaCargoReport);
				processor.Process(logs);
			}

			AssertEquals("Shipment has been locked",
				"Failed to acquire lock for rows in table JobShipment, data being processed by other user.",
				logs.Last().Message);
		}

		public void TestProcessData_ConvertShipmentToCustomsJobSuccessed_AUAirCargoReport()
		{
			AssertConvertShipmentToCustomsJobSucceeded<CusMAWB>(CountryCodes.Australia,
				WorkflowTriggerActionTypeConstants.Codes.AUAirCargoReport,
				"HVLV AirCargo Report",
				(shipment) => {
					shipment.JS_TransportMode = TransportModes.Air;
				});
		}

		public void TestProcessData_ConvertShipmentToCustomsJobSuccessed_AUSeaCargoReport()
		{
			AssertConvertShipmentToCustomsJobSucceeded<CusSCAOceanBill>(CountryCodes.Australia,
				WorkflowTriggerActionTypeConstants.Codes.AUSeaCargoReport,
				"HVLV SeaCargo Report",
				(shipment) => {
					shipment.JS_TransportMode = TransportModes.Sea;
				});
		}

		public void TestProcessData_ConvertShipmentToCustomsJobSuccessed_HVLVAirFreightAMS()
		{
			AssertConvertShipmentToCustomsJobSucceeded<AsycudaManifestHeader>(CountryCodes.UnitedStates,
				WorkflowTriggerActionTypeConstants.Codes.CreateHVLVAirFreightAMS,
				"AMS",
				(shipment) => {
					shipment.JS_TransportMode = TransportModes.Air;
				});
		}

		public void TestProcessData_ConvertShipmentToCustomsJobSuccessed_HVLVSeaFreightAMS()
		{
			AssertConvertShipmentToCustomsJobSucceeded<CusInBondHeader>(CountryCodes.UnitedStates,
				WorkflowTriggerActionTypeConstants.Codes.CreateHVLVSeaFreightAMS,
				"AMS",
				(shipment) => {
					shipment.JS_TransportMode = TransportModes.Sea;
				});
		}

		public void TestProcessData_ConvertShipmentToCustomsJobSuccessed_HVLVUSTruckEManifest()
		{
			AssertConvertShipmentToCustomsJobSucceeded<Trip>(CountryCodes.UnitedStates,
				WorkflowTriggerActionTypeConstants.Codes.CreateHVLVUSTruckEManifest,
				"MAN",
				(shipment) => {
					shipment.JS_TransportMode = TransportModes.Road;
				});
		}

		public void TestProcessData_ConvertShipmentToCustomsJobSuccessed_H7Declaration_IE()
		{
			AssertConvertShipmentToCustomsJobSucceeded<AsycudaManifestHeader>(CountryCodes.Ireland,
				WorkflowTriggerActionTypeConstants.Codes.CreateH7Declaration,
				"Low Value (H7)",
				(shipment) =>
				{
					shipment.JS_RL_NKOrigin = "AUSYD";
					shipment.JS_RL_NKDestination = "IEDUB";
				});
		}

		public void TestProcessData_ConvertShipmentToCustomsJobSuccessed_H7Declaration_ES()
		{
			AssertConvertShipmentToCustomsJobSucceeded<AsycudaManifestHeader>(CountryCodes.Spain,
				WorkflowTriggerActionTypeConstants.Codes.CreateH7Declaration,
				"Low Value (H7)",
				(shipment) =>
				{
					shipment.JS_RL_NKOrigin = "AUSYD";
					shipment.JS_RL_NKDestination = "ESMAD";
				});
		}

		public void TestProcessData_ConvertShipmentToCustomsJobSuccessed_H7Declaration_GB()
		{
			AssertConvertShipmentToCustomsJobSucceeded<AsycudaManifestHeader>(CountryCodes.UnitedKingdom,
				WorkflowTriggerActionTypeConstants.Codes.CreateH7Declaration,
				"Low Value (H7/BIRDS)",
				(shipment) =>
				{
					shipment.JS_RL_NKOrigin = "AUSYD";
					shipment.JS_RL_NKDestination = "GBLON";
				});
		}

		public void TestProcessData_ConvertShipmentToCustomsJobSuccessed_H7Declaration_FR()
		{
			AssertConvertShipmentToCustomsJobSucceeded<AsycudaManifestHeader>(CountryCodes.France,
				WorkflowTriggerActionTypeConstants.Codes.CreateH7Declaration,
				"Low Value (H7)",
				(shipment) =>
				{
					shipment.JS_RL_NKOrigin = "AUSYD";
					shipment.JS_RL_NKDestination = "FRADE";
				});
		}

		public void TestProcessData_ConvertShipmentToCustomsJobSuccessed_H7Declaration_IT()
		{
			AssertConvertShipmentToCustomsJobSucceeded<AsycudaManifestHeader>(CountryCodes.Italy,
				WorkflowTriggerActionTypeConstants.Codes.CreateH7Declaration,
				"Low Value (H7)",
				(shipment) =>
				{
					shipment.JS_RL_NKOrigin = "AUSYD";
					shipment.JS_RL_NKDestination = "ITROM";
				});
		}

		public void TestProcessData_ConvertShipmentToCustomsJobSuccessed_H7Declaration_NotSupportedCountry()
		{
			AssertConvertShipmentToCustomsJobFailed<AsycudaManifestHeader>(CountryCodes.Australia,
				WorkflowTriggerActionTypeConstants.Codes.CreateH7Declaration,
				(shipment) =>
				{
					shipment.JS_RL_NKOrigin = "GBLON";
					shipment.JS_RL_NKDestination = "AUSYD";
				});
		}

		public void TestProcessData_ConvertShipmentToCustomsJobSuccessed_NZAirCargoReport()
		{
			AssertConvertShipmentToCustomsJobSucceeded<Enterprise.Integration.Customs.NZ.ICusMAWB>(CountryCodes.NewZealand,
				WorkflowTriggerActionTypeConstants.Codes.NZAirCargoReport,
				"HVLV AirCargo CRE",
				(shipment) => {
					shipment.JS_TransportMode = TransportModes.Air;
				});
		}

		public void TestProcessData_ConvertShipmentToCustomsJobSuccessed_NZSeaCargoReport()
		{
			AssertConvertShipmentToCustomsJobSucceeded<Enterprise.Integration.Customs.NZ.ICusSCAOceanBill>(CountryCodes.NewZealand,
				WorkflowTriggerActionTypeConstants.Codes.NZSeaCargoReport,
				"HVLV SeaCargo CRE",
				(shipment) => {
					shipment.JS_TransportMode = TransportModes.Sea;
				});
		}

		void AssertConvertShipmentToCustomsJobSucceeded<T>(string countryCode, string triggerCode, string expectedTRFLogType, Action<ForwardingShipment> setAdditionalPropertiesForShipment = null) where T : class
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var shipment = CreateAndProcessShipment<T>(triggerCode, setAdditionalPropertiesForShipment);

				var customsJob = typeof(BusinessObject).IsAssignableFrom(typeof(T))
					? Factory.LoadTop1<T>(new ZQuery()) as BusinessObject
					: Factory.LoadTop1(ObjectFactory.GetType<T>(), new ZQuery());

				AssertNotNull("Customs job created", customsJob);

				var transferredLog = shipment.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.TransferredCode).SingleOrDefault();

				transferredLog.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, out var type);
				AssertEquals(expectedTRFLogType, type);

				var genPivotCollection = Factory.Load<GenPivot>(new ZQuery(GenPivotSchema.XX_Relation1ID, shipment.HVLVConsignmentHeader.PK));
				AssertNotNull("GenPivotCollection has been created", genPivotCollection);
				AssertEquals("A gen piovt should be created", 1, genPivotCollection.Length);
			}
		}

		void AssertConvertShipmentToCustomsJobFailed<T>(string countryCode, string triggerCode, Action<ForwardingShipment> setAdditionalPropertiesForShipment = null) where T : BusinessObject
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var shipment = CreateAndProcessShipment<T>(triggerCode, setAdditionalPropertiesForShipment);

				var customsJob = Factory.LoadTop1<T>(new ZQuery());
				AssertNull("Customs job is not created", customsJob);

				var transferredLog = shipment.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.TransferredCode).SingleOrDefault();
				AssertNull("Expected no log", transferredLog);

				var genPivotCollection = Factory.Load<GenPivot>(new ZQuery(GenPivotSchema.XX_Relation1ID, shipment.HVLVConsignmentHeader.PK));
				AssertEquals("Expected empty GenPivotCollection", 0, genPivotCollection.Length);
			}
		}

		ForwardingShipment CreateAndProcessShipment<T>(string triggerCode, Action<ForwardingShipment> setAdditionalPropertiesForShipment) where T : class
		{
			var customsJob1 = typeof(BusinessObject).IsAssignableFrom(typeof(T))
					? Factory.LoadTop1<T>(new ZQuery()) as BusinessObject
					: Factory.LoadTop1(ObjectFactory.GetType<T>(), new ZQuery());

			AssertNull("Precondition: customsJob is null", customsJob1);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_MasterBillNum = "AAA001";
			var container = consol.Containers.AddNew();
			container.ContainerNumberForBinding = "Container001";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			setAdditionalPropertiesForShipment(shipment);

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			var item = consignment.Items.AddNew();
			item.HVI_ContainerNumber = container.ContainerNumberForBinding;

			Factory.Save();

			var logs = new NotificationCollection();
			var processor = new HVLVConvertShipmentToCustomsJobProcessor(shipment, triggerCode);
			processor.Process(logs);

			Factory.Save();
			return shipment;
		}
	}
}
