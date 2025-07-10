using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class ShipmentDataObjectReaderDueDateTest : OrganizationAddressTestHelper
	{
		// 1.1: DDD value is present in XML and is not empty => Use the DDD value from the XML

		public void TestCreateShipmentImportXml_WithValidDDD()
		{
			using (SetRegistryDDD(enableAir: true))
			{
				ImportShipment(TransportModeAir, ServiceLevelExp, PickupRequiredByOne, DeliveryDueDateValid);

				ValidateDeliveryDueDateValue("Imported shipment", ExpectedDeliveryDueDateValid, reason: "Changed by Data Import");
			}
		}

		public void TestModifyShipmentImportXml_WithValidDDD()
		{
			using (SetRegistryDDD(enableAir: true))
			{
				CreateShipment("AIR", "EXP", PickupRequiredByDateOne);

				ValidateDeliveryDueDateValue("Created shipment", ExpectedDeliveryDueDateOneExp, reason: "Automatically calculated for new shipment");

				ImportShipment(TransportModeAir, ServiceLevelStd, PickupRequiredByTwo, DeliveryDueDateValid);

				ValidateDeliveryDueDateValue("Imported shipment", ExpectedDeliveryDueDateValid, reason: "Changed by Data Import");
			}
		}

		public void TestCreateShipmentImportXml_WithValidDDD_WhenRegistryIsDisabledForShipmentTransportMode()
		{
			using (SetRegistryDDD(enableAir: true))
			{
				ImportShipment(TransportModeSea, ServiceLevelExp, PickupRequiredByOne, DeliveryDueDateValid);

				ValidateDeliveryDueDateValue("Imported shipment", ExpectedDeliveryDueDateValid);
			}
		}

		public void TestModifyShipmentImportXml_WithValidDDD_WhenRegistryIsDisabledForShipmentTransportMode()
		{
			using (SetRegistryDDD(enableAir: true))
			{
				CreateShipment("SEA", "EXP", PickupRequiredByDateOne);

				ValidateDeliveryDueDateValue("Created shipment");

				ImportShipment(TransportModeSea, ServiceLevelStd, PickupRequiredByOne, DeliveryDueDateValid);

				ValidateDeliveryDueDateValue("Imported shipment", ExpectedDeliveryDueDateValid);
			}
		}

		// 1.2.1: DDD value is present in XML and is empty + active + All necessary parameters are present => Calculate DDD

		public void TestCreateShipmentImportXml_WithCompleteParametersAndBlankDDD()
		{
			using (SetRegistryDDD(enableAir: true))
			{
				ImportShipment(TransportModeAir, ServiceLevelExp, PickupRequiredByOne, DeliveryDueDateBlank);

				ValidateDeliveryDueDateValue("Imported shipment", ExpectedDeliveryDueDateOneExp, reason: "Automatically calculated for new shipment");
			}
		}

		public void TestModifyShipmentImportXml_WithCompleteParametersAndBlankDDD()
		{
			using (SetRegistryDDD(enableAir: true))
			{
				CreateShipment("AIR", "EXP", PickupRequiredByDateOne);

				ValidateDeliveryDueDateValue("Created shipment", ExpectedDeliveryDueDateOneExp, reason: "Automatically calculated for new shipment");

				ImportShipment(TransportModeAir, ServiceLevelStd, PickupRequiredByTwo, DeliveryDueDateBlank);

				ValidateDeliveryDueDateValue("Imported shipment", ExpectedDeliveryDueDateTwoStd);
			}
		}

		// 1.2.2: DDD value is present in XML and is empty + active + Not all necessary parameters are present => The DDD value will be cleared

		public void TestCreateShipmentImportXml_WithIncompleteParametersAndBlankDDD()
		{
			using (SetRegistryDDD(enableAir: true))
			{
				ImportShipment(TransportModeAir, ServiceLevelExp, PickupRequiredByBlank, DeliveryDueDateBlank);

				ValidateDeliveryDueDateValue("Imported shipment");
			}
		}

		public void TestModifyShipmentImportXml_WithIncompleteParametersAndBlankDDD()
		{
			using (SetRegistryDDD(enableAir: true))
			{
				CreateShipment("AIR", "EXP", PickupRequiredByDateOne);

				ValidateDeliveryDueDateValue("Created shipment", ExpectedDeliveryDueDateOneExp, reason: "Automatically calculated for new shipment");

				ImportShipment(TransportModeAir, ServiceLevelStd, PickupRequiredByBlank, DeliveryDueDateBlank);

				ValidateDeliveryDueDateValue("Imported shipment", reason: "Changed by Data Import");
			}
		}

		// 1.3: DDD value is present in XML and is empty + inactive => The DDD value will be cleared

		public void TestCreateShipmentImportXml_WithCompleteParametersAndBlankDDD_WhenRegistryIsDisabledForShipmentTransportMode()
		{
			using (SetRegistryDDD(enableAir: true))
			{
				ImportShipment(TransportModeSea, ServiceLevelExp, PickupRequiredByOne, DeliveryDueDateBlank);

				ValidateDeliveryDueDateValue("Imported shipment");
			}
		}

		public void TestModifyShipmentImportXml_WithCompleteParametersAndBlankDDD_WhenRegistryIsDisabledForShipmentTransportMode()
		{
			using (SetRegistryDDD(enableAir: true))
			{
				CreateShipment("SEA", "EXP", PickupRequiredByDateOne);

				ValidateDeliveryDueDateValue("Created shipment");

				ImportShipment(TransportModeSea, ServiceLevelStd, PickupRequiredByTwo, DeliveryDueDateBlank);

				ValidateDeliveryDueDateValue("Imported shipment");
			}
		}

		public void TestCreateShipmentImportXml_WithIncompleteParametersAndBlankDDD_WhenRegistryIsDisabledForShipmentTransportMode()
		{
			using (SetRegistryDDD(enableAir: true))
			{
				ImportShipment(TransportModeSea, ServiceLevelExp, PickupRequiredByBlank, DeliveryDueDateBlank);

				ValidateDeliveryDueDateValue("Imported shipment");
			}
		}

		// 2.1.1: DDD value is not present in XML + A new shipment is being created + active + All necessary parameters are present => Calculate DDD for AIR

		public void TestCreateShipmentImportXml_WithCompleteParametersAndNoDDD()
		{
			using (SetRegistryDDD(enableAir: true))
			{
				ImportShipment(TransportModeAir, ServiceLevelExp, PickupRequiredByOne, DeliveryDueDateNone);

				ValidateDeliveryDueDateValue("Imported shipment", ExpectedDeliveryDueDateOneExp, reason: "Automatically calculated for new shipment");
			}
		}

		// 2.1.2: DDD value is not present in XML + A new shipment is being created + active + Not all necessary parameters are present => DDD value will be left blank

		public void TestCreateShipmentImportXml_WithIncompleteParametersAndNoDDD()
		{
			using (SetRegistryDDD(enableAir: true))
			{
				ImportShipment(TransportModeAir, ServiceLevelExp, default, DeliveryDueDateNone);

				ValidateDeliveryDueDateValue("Imported shipment");
			}
		}

		// 2.2.1: DDD value is not present in XML + An existing shipment is being modified + active + No necessary parameters have changed => DDD value will be unchanged

		public void TestModifyShipmentImportXml_WithNoChangedParametersAndNoDDD()
		{
			using (SetRegistryDDD(enableAir: true))
			{
				CreateShipment("AIR", "EXP", PickupRequiredByDateTwo);

				ValidateDeliveryDueDateValue("Created shipment", ExpectedDeliveryDueDateTwoExp, reason: "Automatically calculated for new shipment");

				ImportShipment(TransportModeAir, ServiceLevelExp, PickupRequiredByTwo, DeliveryDueDateNone);

				// event and notes have not changed
				ValidateDeliveryDueDateValue("Imported shipment", ExpectedDeliveryDueDateTwoExp, reason: "Automatically calculated for new shipment");
			}
		}

		public void TestModifyShipmentImportXml_WithNoChangedParametersAndNoDDD_DDDSet()
		{
			using (SetRegistryDDD(enableAir: true))
			{
				CreateShipment("AIR", "EXP", PickupRequiredByDateOne, ExpectedDeliveryDueDateValid);

				ValidateDeliveryDueDateValue("Created shipment", ExpectedDeliveryDueDateValid, reason: "Initial condition");

				ImportShipment(TransportModeAir, ServiceLevelExp, PickupRequiredByOne, DeliveryDueDateNone);

				// event and notes have not changed
				ValidateDeliveryDueDateValue("Imported shipment", ExpectedDeliveryDueDateValid, reason: "Initial condition");
			}
		}

		// 2.2.2.1: DDD value is not present in XML + An existing shipment is being modified + active + Necessary parameters have changed + All necessary parameters are present => DDD value will be calculated

		public void TestModifyShipmentImportXml_WithChangedCompleteParametersAndNoDDD()
		{
			using (SetRegistryDDD(enableAir: true))
			{
				CreateShipment("AIR", "EXP", PickupRequiredByDateTwo);

				ValidateDeliveryDueDateValue("Created shipment", ExpectedDeliveryDueDateTwoExp, reason: "Automatically calculated for new shipment");

				ImportShipment(TransportModeAir, ServiceLevelStd, PickupRequiredByOne, DeliveryDueDateNone);

				ValidateDeliveryDueDateValue("Imported shipment", ExpectedDeliveryDueDateOneStd, reason: "Service Level, Pickup Required By changed");
			}
		}

		// 2.2.2.1: DDD value is not present in XML + An existing shipment is being modified + active + Necessary parameters have changed + Not all necessary parameters are present => DDD value will be left unchanged

		public void TestModifyShipmentImportXml_WithChangedIncompleteParametersAndNoDDD()
		{
			using (SetRegistryDDD(enableAir: true))
			{
				CreateShipment("AIR", "EXP", PickupRequiredByDateTwo);

				ValidateDeliveryDueDateValue("Created shipment", ExpectedDeliveryDueDateTwoExp, reason: "Automatically calculated for new shipment");

				ImportShipment(TransportModeAir, ServiceLevelStd, PickupRequiredByBlank, DeliveryDueDateNone);

				ValidateDeliveryDueDateValue("Imported shipment", ExpectedDeliveryDueDateTwoExp);
			}
		}

		// 2.3: DDD value is not present in XML + A new shipment is being created + inactive => DDD value will be left blank

		public void TestCreateShipmentImportXml_WithCompleteParametersAndNoDDD_WhenRegistryIsDisabledForShipmentTransportMode()
		{
			using (SetRegistryDDD(enableAir: true))
			{
				ImportShipment(TransportModeSea, ServiceLevelExp, PickupRequiredByOne, DeliveryDueDateNone);

				ValidateDeliveryDueDateValue("Imported shipment");
			}
		}

		// 2.4: DDD value is not present in XML + An existing shipment is being modified + inactive => DDD value will be left unchanged

		public void TestModifyShipmentImportXml_WithChangedCompleteParametersAndNoDDD_WhenRegistryIsDisabledForShipmentTransportMode()
		{
			using (SetRegistryDDD(enableAir: true))
			{
				CreateShipment("SEA", "EXP", PickupRequiredByDateTwo);

				ValidateDeliveryDueDateValue("Created shipment");

				ImportShipment(TransportModeSea, ServiceLevelStd, PickupRequiredByOne, DeliveryDueDateNone);

				ValidateDeliveryDueDateValue("Imported shipment");
			}
		}

		// End of test cases

		void ValidateDeliveryDueDateValue(string message, ZDateTime expectedDateTime = default, string reason = default)
		{
			var shipment = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, "JOHN235478923"));

			AssertNotNull("shipment", shipment);
			if (expectedDateTime.IsEmpty || expectedDateTime == default)
			{
				Assert($"{message} shipment.JS_DeliveryDueDate is not empty", shipment.JS_DeliveryDueDate.IsEmpty);
			}
			else
			{
				AssertEquals(message, expectedDateTime, shipment.JS_DeliveryDueDate);
			}

			if (reason != null)
			{
				var ddeEvent = shipment.Logs.MostRecentLogByEventTime(AutoEvents.DeliveryDateUpdated);
				AssertNotNull($"{message}: No Delivery Date Updated Event", ddeEvent);
				Assert($"{message}: DDE has incorrect Reason\nExpected:\n{reason}\n\nDDE was:\n{ddeEvent.SL_Reference}", ddeEvent.SL_Reference.Contains($"|RES={reason}|"));

				var deliveryDueDateNoteText = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.DeliveryDueDateCalculationLog.Description).FirstOrDefault()?.ST_NoteText ?? "Note not found";
				Assert($"{message}: Note has incorrect Reason\nExpected:\n{reason}\n\nNote was:\n{deliveryDueDateNoteText}", deliveryDueDateNoteText.Contains($"Reason: {reason}\r\n"));
			}
		}

		void CreateShipment(string transportMode, string serviceLevel, ZDateTime pickupRequiredByDate)
		{
			var newFactory = new BusinessObjectFactory();

			var shipment = newFactory.NewWithValidTestData<ForwardingShipment>();

			shipment.JS_TransportMode = transportMode;
			shipment.JS_HouseBill = "JOHN235478923";
			shipment.DocsAndCartage.JP_PickupRequiredBy = pickupRequiredByDate;
			shipment.JS_HBLContainerPackModeOverride = Constants.HBLDeliveryModes.Codes.DOOR_DOOR;
			shipment.JS_OA_ExportReceivingDepot = pickupCFSAddress.PK;
			shipment.JS_OA_ImportReleaseDepot = deliveryCFSAddress.PK;
			shipment.ConsignorPickupAddress.E2_OA_Address = consignorAddress.PK;
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = consigneeAddress.PK;
			shipment.JS_RS_NKServiceLevel = serviceLevel;

			if (transportMode == "AIR")
			{
				Env.Security.MaintainShipmentDeliveryDueDateOverride.IsAllowed = true;
				shipment.CalculateDeliveryDueDate(ForwardingShipment.DeliveryDueDateCalculationMode.Automatic);
			}

			newFactory.Save();
		}

		void CreateShipment(string transportMode, string serviceLevel, ZDateTime pickupRequiredByDate, ZDateTime deliveryDueDate)
		{
			Env.Security.MaintainShipmentDeliveryDueDateOverride.IsAllowed = true;
			var newFactory = new BusinessObjectFactory();

			var shipment = newFactory.NewWithValidTestData<ForwardingShipment>();

			shipment.GetReasonForChangingDeliveryDueDateEventHandler += (sender, arg) => arg.Reason = "Initial condition";

			shipment.JS_TransportMode = transportMode;
			shipment.JS_HouseBill = "JOHN235478923";
			shipment.DocsAndCartage.JP_PickupRequiredBy = pickupRequiredByDate;
			shipment.JS_DeliveryDueDate = deliveryDueDate;
			shipment.JS_HBLContainerPackModeOverride = Constants.HBLDeliveryModes.Codes.DOOR_DOOR;
			shipment.JS_OA_ExportReceivingDepot = pickupCFSAddress.PK;
			shipment.JS_OA_ImportReleaseDepot = deliveryCFSAddress.PK;
			shipment.ConsignorPickupAddress.E2_OA_Address = consignorAddress.PK;
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = consigneeAddress.PK;
			shipment.JS_RS_NKServiceLevel = serviceLevel;

			newFactory.Save();
		}

		ForwardingShipment ImportShipment(string transportModeReplacement, string serviceLevelReplacement,
			string pickupRequiredByReplacement, string deliveryDueDateReplacement)
		{
			var shipmentData = ReadXmlShipment(GetResourcePathFor("DeliveryDueDate.xml"), transportModeReplacement, serviceLevelReplacement, deliveryDueDateReplacement, pickupRequiredByReplacement: pickupRequiredByReplacement);
			var reader = new ShipmentDataObjectReader(shipmentData, Logger, Factory, null);

			var shipment = reader.ReadIntoBusinessObject();

			Factory.SaveForTesting();

			return shipment;
		}

		UniversalShipment ReadXmlShipment(string resourcePath, string transportModeReplacement, string serviceLevelReplacement, string deliveryDueDateReplacement, string pickupRequiredByReplacement)
		{
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			using (var resourceRetriever = new EmbeddedResourceRetriever())
			using (var resourceStream = resourceRetriever.GetStream(resourcePath))
			{
				string text = new StreamReader(resourceStream).ReadToEnd();

				// modify text
				text = text.Replace(TransportModeTag, transportModeReplacement);
				text = text.Replace(ServiceLevelTag, serviceLevelReplacement);
				text = text.Replace(PickupRequiredByTag, pickupRequiredByReplacement);
				text = text.Replace(DeliveryDueDateTag, deliveryDueDateReplacement);

				using (var subStreamableStream = new SubStreamableStream(new MemoryStream(Encoding.ASCII.GetBytes(text)), DisposableLeakListener.Instance))
				{
					ObjectFactory.Get<IXmlReader>().ReadXML(shipmentDataObject, subStreamableStream, Logger);
				}
			}

			return shipmentDataObject;
		}

		const string TransportModeTag = "-- ##TransportMode##";
		const string TransportModeAir = "<TransportMode><Code>AIR</Code><Description>Air Freight</Description></TransportMode>";
		const string TransportModeSea = "<TransportMode><Code>SEA</Code><Description>Sea Freight</Description></TransportMode>";

		const string ServiceLevelTag = "-- ##ServiceLevel##";
		const string ServiceLevelStd = "<ServiceLevel><Code>STD</Code><Description>Standard</Description></ServiceLevel>";
		const string ServiceLevelExp = "<ServiceLevel><Code>EXP</Code><Description>Expedited</Description></ServiceLevel>";

		const string PickupRequiredByTag = "-- ##PickupRequiredBy##";
		const string PickupRequiredByOne = "<PickupRequiredBy>2024-11-19T10:00:00</PickupRequiredBy>";
		const string PickupRequiredByTwo = "<PickupRequiredBy>2024-11-18T11:30:00</PickupRequiredBy>";
		const string PickupRequiredByBlank = "<PickupRequiredBy></PickupRequiredBy>";

		const string DeliveryDueDateTag = "-- ##DeliveryDueDate##";
		const string DeliveryDueDateNone = "";
		const string DeliveryDueDateBlank = "<Date><Type>DeliveryDueDate</Type><IsEstimate>true</IsEstimate><Value></Value></Date>";
		const string DeliveryDueDateValid = "<Date><Type>DeliveryDueDate</Type><IsEstimate>true</IsEstimate><Value>2024-11-23T15:15:00</Value></Date>";

		readonly ZDateTime PickupRequiredByDateOne = new ZDateTime(2024, 11, 19, 10, 0, 0);
		readonly ZDateTime PickupRequiredByDateTwo = new ZDateTime(2024, 11, 18, 11, 30, 0);

		readonly ZDateTime ExpectedDeliveryDueDateOneExp = new ZDateTime(2024, 11, 21, 10, 0, 0);
		readonly ZDateTime ExpectedDeliveryDueDateOneStd = new ZDateTime(2024, 11, 22, 10, 0, 0);
		readonly ZDateTime ExpectedDeliveryDueDateTwoExp = new ZDateTime(2024, 11, 20, 11, 30, 0);
		readonly ZDateTime ExpectedDeliveryDueDateTwoStd = new ZDateTime(2024, 11, 21, 11, 30, 0);
		readonly ZDateTime ExpectedDeliveryDueDateValid = new ZDateTime(2024, 11, 23, 15, 15, 0);

		IDisposable SetRegistryDDD(bool enableAir)
		{
			return FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetCalculateDeliveryDueDateOptions(enableAir));
		}

		CalculateDeliveryDueDateOptions GetCalculateDeliveryDueDateOptions(bool enableAir)
		{
			return new CalculateDeliveryDueDateOptions
			{
				IsActive = enableAir,
				TransportModes = new CalculateDeliveryDueDateTransportModeCollection()
				{
					{ Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air, enableAir },
					{ Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea, false },
					{ Core.Constants.TransportModes.Road, Core.Constants.TransportModeDescriptions.Road, false },
					{ Core.Constants.TransportModes.Rail, Core.Constants.TransportModeDescriptions.Rail, false }
				}
			};
		}

		public CalculateDeliveryDueDateTransportModeCollection ActiveTransportModesForCalculateDeliveryDateOption(bool enableAir) => new CalculateDeliveryDueDateTransportModeCollection()
			{
				{ Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air, enableAir },
				{ Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea, false },
				{ Core.Constants.TransportModes.Road, Core.Constants.TransportModeDescriptions.Road, false },
				{ Core.Constants.TransportModes.Rail, Core.Constants.TransportModeDescriptions.Rail, false }
			};

		static string GetResourcePathFor(string fileName)
		{
			return $"Enterprise.Freight.Forwarding.DataTransfer.Test.Universal.Shipment.TestFiles.{fileName}";
		}

		protected override void SetUp()
		{
			base.SetUp();

			var newFactory = new BusinessObjectFactory();

			pickupCFSOrg = newFactory.NewWithValidTestData<OrgHeader>();
			pickupCFSOrg.OH_Code = "DEMCLISYD";
			pickupCFSOrg.OH_FullName = "DEMO CLIENT SYDNEY";
			pickupCFSAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(pickupCFSOrg, "46 Douglas St", "2064", Core.Constants.CountryCodes.Australia, "NSW", "Sydney","AUSYD");

			deliveryCFSOrg = newFactory.NewWithValidTestData<OrgHeader>();
			deliveryCFSOrg.OH_Code = "SENAGESIN";
			deliveryCFSOrg.OH_FullName = "Send Agent Singapore";
			deliveryCFSAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryCFSOrg, "1 Test Street.", "600033", Core.Constants.CountryCodes.Singapore, "01", "Singapore", "SGSIN");

			consignorOrg = newFactory.NewWithValidTestData<OrgHeader>();
			consignorOrg.OH_Code = "FREPLUSYD";
			consignorOrg.OH_FullName = "FREIGHT PLUS CUSTOMS PTY LIMITED";
			consignorAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(consignorOrg, "P O BOX 2206, TAREN POINT", "2206", Core.Constants.CountryCodes.Australia, "NSW", "Sydney", "AUSYD");

			var consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			consigneeOrg.OH_Code = "TESSINSIN";
			consigneeOrg.OH_FullName = "TEST SIN IMPORTER";
			consigneeAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(consigneeOrg, "2 ORANGE ROAD", "900001", Core.Constants.CountryCodes.Singapore, "01", "Singapore", "SGSIN");

			originZone = DeliveryDueDateCalculationTestHelper.SetupTransportZone(newFactory, pickupCFSOrg, Core.Constants.CountryCodes.Australia);
			destinationZone = DeliveryDueDateCalculationTestHelper.SetupTransportZone(newFactory, deliveryCFSOrg, Core.Constants.CountryCodes.Singapore);

			SetupTransitTime(newFactory, "STD", 72);
			SetupTransitTime(newFactory, "EXP", 48);

			newFactory.Save();
			Factory.SaveForTesting();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		RefTransitTime SetupTransitTime(BusinessObjectFactory factory, String serviceLevel, int totalTransitHours, string mode = "ALL", int defaultTransitHours = 0)
		{
			var sl = DeliveryDueDateCalculationTestHelper.GetOrCreateRefServiceLevelIfNotExist(factory, serviceLevel);
			sl.RS_DefaultTransitHours = defaultTransitHours;
			return  DeliveryDueDateCalculationTestHelper.SetupTransitTime(factory, originZone.PK, destinationZone.PK, serviceLevel, mode, totalTransitHours);
		}

		OrgHeader pickupCFSOrg, deliveryCFSOrg, consignorOrg;
		OrgAddress pickupCFSAddress, deliveryCFSAddress, consignorAddress, consigneeAddress;
		RateTransportZone originZone, destinationZone;
	}
}
