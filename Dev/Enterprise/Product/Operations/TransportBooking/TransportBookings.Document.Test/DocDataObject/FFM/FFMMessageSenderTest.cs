using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.TransportBookings.Document.Testing
{
	public sealed class FFMMessageSenderTest : DtbBookingTestCaseWithFactory
	{
		readonly string voyageNumber = "CS0001A";
		readonly string goodsDescription = "Test Goods";
		readonly string wayBillNumber = "724-47177421";
		readonly string vessel = "SHIPPER";
		readonly string shipmentOrigin = "ITMXP";
		readonly string shipmentDestination = "THBKK";
		readonly string documentID = "12 123 123 123";
		readonly string regulatedAgentID = "0079-00";
		readonly string approvalCountryCode = "IT";
		readonly string direction = "DLV";

		readonly ZDate leaveDate = new(2077, 1, 1);
		readonly ZDate arriveDate = new(2077, 1, 2);

		readonly string packagePackageType = "PKG";
		readonly string containerType = "AMA-2Q";

		readonly string containerID1 = "CNT-1";

		readonly ZInt container1SubPackage1Quantity = 1;
		readonly ZDecimal container1SubPackage1Volume = 7.5m;
		readonly ZString container1SubPackage1VolumeUnit = "M3";
		readonly ZDecimal container1SubPackage1Weight = 55.5m;
		readonly ZString container1SubPackage1WeightUnit = "KG";

		TestContainerData containerDetails1;

		TestFFMDetails ffmMessageMissingDetails;

		readonly string expectedPortOfLoading = "MXP";
		readonly string expectedPortOfDestination = "BKK";

		protected override void SetUp()
		{
			base.SetUp();
			containerDetails1 = new()
			{
				ContainerID = containerID1,
				ContainerType = containerType,
				SubPackages = new()
				{
					new TestPackageData()
					{
						Quantity = container1SubPackage1Quantity,
						Type = packagePackageType,
						Volume = container1SubPackage1Volume,
						Weight = container1SubPackage1Weight,
						VolumeMetric = container1SubPackage1VolumeUnit,
						WeightMetric = container1SubPackage1WeightUnit
					}
				}
			};

			ffmMessageMissingDetails = new(Factory.New<DummyWithDtbBookingAndConfirmMessage>())
			{
				Packages = new List<IFFMPackageDetails>()
				{
					new TestPackageDetails()
				}
			};
		}

		public void TestSendFFM()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var confirmation = CreateTestData(containers: new List<TestContainerData> { containerDetails1 });
			Factory.Save();

			using (Factory.AddDisposableService())
			{
				var wasSent = FFMMessageSender.SendMessage(new DtbBookingConfirmationFFMDetailsProvider(confirmation), out var notifications);

				CombineAssertions("FFM message failed to send", () =>
				{
					Assert("FFM Message could not be sent", wasSent);

					if (notifications.Count > 0)
					{
						var errorMessage = string.Join(",", notifications.Select(n => n.Message));
						Fail(errorMessage);
					}
				});
			}

			var documentDataStorageQuery = new ZQuery();
			documentDataStorageQuery.AddToFilter(JobDocumentDataSchema.JDD_Name, BookingsDocumentDataStoreNames.FFMMessageRequest);
			documentDataStorageQuery.AddToFilter(JobDocumentDataSchema.JDD_ParentID, confirmation.PK);

			var documentDataStorage = Factory.LoadTop1<IVisualizerDocumentData>(documentDataStorageQuery);

			var logs = (documentDataStorage as IStmALogParent)
				.Logs
				.GetAllLogs()
				.Cast<StmALog>()
				.Where(l => l.SL_SE_NKEvent == Events.MessageSentCode
					|| l.SL_SE_NKEvent == Events.DataExportCode)
				.ToArray();

			var messageSentEvent = logs.Single(l => l.SL_SE_NKEvent == Events.MessageSentCode);
			AssertEquals("Message sent code reference was not found", "|DEP=Terminal|MST=FFM send request", messageSentEvent.SL_Reference);

			var dataExportEvent = logs.Single(l => l.SL_SE_NKEvent == Events.DataExportCode);
			var message = dataExportEvent.RelatedEDIMessage;

			AssertNotNull("EDI FFM message has not been created successfully.", message);

			var ediTransport = message.Message.Interchange.EI_TransportType;
			AssertEquals("FFM Messages need to be sent via direct xT.", ediTransport, EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface);

			var messageText = message.Message.EM_MessageText;

			var xus = new XmlDocument();
			xus.LoadXml(messageText);
			var ffmInstructions = new FFMMessageMessageInstructions();

			var xmlNamespaceManager = new XmlNamespaceManager(xus.NameTable);
			xmlNamespaceManager.AddNamespace("ns", $"http://www.cargowise.com/Schemas/Universal/2012/11/{ffmInstructions.XmlNamespace}");

			CombineAssertions("Main shipment Data for XUS is wrong.", () =>
			{
				var query = $"//ns:Shipment//ns:PortOfDestination[. = '{expectedPortOfDestination}']";
				var portOfDestinationNode = xus.SelectSingleNode(query, xmlNamespaceManager);
				AssertNotNull("XUS Port of Destination is wrong.", portOfDestinationNode);

				query = $"//ns:Shipment//ns:PortOfLoading[. = '{expectedPortOfLoading}']";
				var portOfLoadingNode = xus.SelectSingleNode(query, xmlNamespaceManager);
				AssertNotNull("XUS Port of Loading is wrong.", portOfLoadingNode);

				query = $"//ns:Date[./ns:Type = 'Departure']/ns:Value[. = '{leaveDate.ToString("yyyy-MM-ddTHH:mm:ss")}']";
				var departTimeNode = xus.SelectSingleNode(query, xmlNamespaceManager);
				AssertNotNull("XUS Departure time is wrong.", departTimeNode);

				query = $"//ns:VoyageFlightNo[. = '{voyageNumber}']";
				var voyageNumberNode = xus.SelectSingleNode(query, xmlNamespaceManager);
				AssertNotNull("XUS Voyage Number is wrong.", voyageNumberNode);
			});

			CombineAssertions("AWB Header exported information is wrong", () =>
			{
				var query = $"//ns:CarrierDocumentsOverride//ns:AWBHeader/ns:AgentName[. = '{regulatedAgentID}']";
				var regulatedAgentNode = xus.SelectSingleNode(query, xmlNamespaceManager);
				AssertNotNull("XUS Regulated agent ID is wrong.", regulatedAgentNode);

				query = $"//ns:CarrierDocumentsOverride//ns:AWBHeader/ns:AgentPlace[. = '{approvalCountryCode}']";
				var regulatedCountryNode = xus.SelectSingleNode(query, xmlNamespaceManager);
				AssertNotNull("XUS Country of regulated agent  is wrong.", regulatedCountryNode);

				query = $"//ns:CarrierDocumentsOverride//ns:SpecialHandlingCode[. = 'SPX']";
				var secureCargoStatus = xus.SelectSingleNode(query, xmlNamespaceManager);
				AssertNotNull("XUS cargo secure status is wrong.", secureCargoStatus);
			});

			var query = "//ns:SubShipment";
			var subShipmentNode = xus.SelectSingleNode(query, xmlNamespaceManager);
			AssertNotNull("XUS is missing the sub shipment that is a representation of its packages for container 1.", subShipmentNode);

			CombineAssertions("Information about the container 1's packages is wrong.", () =>
			{
				query = $"//ns:GoodsDescription[. = '{goodsDescription}']";
				var goodsDescriptionNode = subShipmentNode.SelectSingleNode(query, xmlNamespaceManager);
				AssertNotNull("Package good description is wrong.", goodsDescriptionNode);

				query = $"//ns:PortOfDestination[. = '{expectedPortOfDestination}']";
				var portOfDischarge = subShipmentNode.SelectSingleNode(query, xmlNamespaceManager);
				AssertNotNull("Package Port of Discharge is wrong.", portOfDischarge);

				query = $"//ns:PortOfLoading[. = '{expectedPortOfLoading}']";
				var portOfLoadingNode = subShipmentNode.SelectSingleNode(query, xmlNamespaceManager);
				AssertNotNull("Package Port of Loading is wrong.", portOfLoadingNode);

				query = $"//ns:TotalNoOfPacks[. = '{container1SubPackage1Quantity}']";
				var quantityNode = subShipmentNode.SelectSingleNode(query, xmlNamespaceManager);
				AssertNotNull("Package Quantity is wrong.", quantityNode);

				query = $"//ns:TotalVolume[. = '{container1SubPackage1Volume}']";
				var volumeNode = subShipmentNode.SelectSingleNode(query, xmlNamespaceManager);
				AssertNotNull("Package Volume is wrong.", volumeNode);

				query = $"//ns:TotalVolumeUnit[. = '{container1SubPackage1VolumeUnit}']";
				var volumeUnitNode = subShipmentNode.SelectSingleNode(query, xmlNamespaceManager);
				AssertNotNull("Package Volume Unit is wrong.", volumeUnitNode);

				query = $"//ns:TotalWeight[. = '{container1SubPackage1Weight}']";
				var weightNode = subShipmentNode.SelectSingleNode(query, xmlNamespaceManager);
				AssertNotNull("Package Weight is wrong.", weightNode);

				query = $"//ns:TotalWeightUnit[. = '{container1SubPackage1WeightUnit}']";
				var weightUnitNode = subShipmentNode.SelectSingleNode(query, xmlNamespaceManager);
				AssertNotNull("Package Weight is wrong.", weightUnitNode);

				query = $"//ns:WayBillNumber[. = '{wayBillNumber}']";
				var wayBillNumberNode = subShipmentNode.SelectSingleNode(query, xmlNamespaceManager);
				AssertNotNull("Package Way Bill Number is wrong.", wayBillNumberNode);

				query = $"//ns:ContainerNumber[. = '{containerID1}']";
				var containerNameNode = subShipmentNode.SelectSingleNode(query, xmlNamespaceManager);
				AssertNotNull("Package Container is wrong.", containerNameNode);
			});

			query = $"//ns:DriverDocumentID[. = '{documentID}' ]";
			var driverDocumentNode = xus.SelectSingleNode(query, xmlNamespaceManager);
			AssertNotNull("Driver Document ID is wrong.", driverDocumentNode);
		}

		public void TestInvalidSendFFM()
		{
			var wasSent = FFMMessageSender.SendMessage(ffmMessageMissingDetails, out var notifications);

			Assert("The FFM message is malformed but the sender believe it was sent", !wasSent);

			CombineAssertions("The sender failed to report on the malform FFM message", () =>
			{
				AssertCollectionContains("Failed to return missing Airport Code.", "Error - IATACode: Missing The Airport Code for this FFM Message.", notifications.Select(n => n.Message));
				AssertCollectionContains("Failed to return missing Port of Loading.", "Error - IATACode: Missing The Port of Loading for this FFM Message.", notifications.Select(n => n.Message));
				AssertCollectionContains("Failed to return missing Voyage Flight Number.", "Error - VoyageFlightNo: Missing the Voyage Flight Number.", notifications.Select(n => n.Message));
				AssertCollectionContains("Failed to return missing Flight Date.", "Error - FlightDate: Missing the Flight Date.", notifications.Select(n => n.Message));

				AssertCollectionContains("Failed to return missing package Port of Destination.", "Error - IATACode: Missing a package's Port of Destination.", notifications.Select(n => n.Message));
				AssertCollectionContains("Failed to return missing package Port of Loading.", "Error - IATACode: Missing a package's Port of Loading.", notifications.Select(n => n.Message));
				AssertCollectionContains("Failed to return missing package Way Bill Number.", "Error - WayBillNumber: A package is missing its Way Bill Number.", notifications.Select(n => n.Message));
				AssertCollectionContains("Failed to return missing package Goods Description.", "Error - GoodsDescription: A package is missing the descriptions for its goods.", notifications.Select(n => n.Message));
				AssertCollectionContains("Failed to return missing package Quantity.", "Error - Quantity: A package's quantity must be set above zero.", notifications.Select(n => n.Message));
				AssertCollectionContains("Failed to return missing package Weight.", "Error - Weight: A package's weight must be set above zero.", notifications.Select(n => n.Message));
				AssertCollectionContains("Failed to return missing package Weight Metric.", "Error - WeightMetric: A package's weight did not specify a metric.", notifications.Select(n => n.Message));
				AssertCollectionContains("Failed to return missing package Volume.", "Error - Volume: A package's volume must be set above zero.", notifications.Select(n => n.Message));
				AssertCollectionContains("Failed to return missing package Volume Metric.", "Error - VolumeMetric: A package's volume did not specify a metric.", notifications.Select(n => n.Message));

				AssertCollectionContains("Failed to return missing Regulated Agent ID.", "Error - RegulatedAgentID: Missing the ID of the Regulated Agent.", notifications.Select(n => n.Message));
				AssertCollectionContains("Failed to return missing Regulated Agent Country.", "Error - RegulatedAgentCountry: Missing the Country of the Regulated Agent.", notifications.Select(n => n.Message));
			});
		}

		DtbBookingConfirmation CreateTestData(
			List<TestContainerData> containers = null,
			List<TestPackageData> loosePackages = null
		)
		{
			var booking = CreateDtbBookingWithShipmentConsolidation();

			var selectedConfirmation = CreateConfirmationWithPackagesOnBooking(
				booking: booking,
				containers: containers,
				loosePackages: loosePackages);

			return selectedConfirmation;
		}

		DtbBooking CreateDtbBookingWithShipmentConsolidation()
		{
			var bookingShipment = (BusinessObject)Factory.New<IForwardingShipment>();
			bookingShipment[JobShipmentSchema.JS_IsShipping] = true;
			bookingShipment[JobShipmentSchema.JS_IsForwardRegistered] = false;
			bookingShipment[JobShipmentSchema.JS_IsBooking] = true;
			bookingShipment[JobShipmentSchema.JS_RL_NKOrigin] = shipmentOrigin;
			bookingShipment[JobShipmentSchema.JS_RL_NKDestination] = shipmentDestination;
			bookingShipment[JobShipmentSchema.JS_RL_NKLoadPort] = shipmentOrigin;
			bookingShipment[JobShipmentSchema.JS_RL_NKDischargePort] = shipmentDestination;

			var bookingConsol = Factory.New<IForwardingConsol>();
			bookingConsol.AddShipment((IForwardingShipment)bookingShipment);

			var consolBusinessObject = (BusinessObject)bookingConsol;
			consolBusinessObject[JobConsolSchema.JK_RL_NKLoadPort] = shipmentOrigin;
			consolBusinessObject[JobConsolSchema.JK_RL_NKDischargePort] = shipmentDestination;
			consolBusinessObject[JobConsolSchema.JK_TransportMode] = Core.Constants.TransportModes.Air;
			consolBusinessObject[JobConsolSchema.JK_OverrideWaybillDefaults] = ZBool.True;

			var awbSpecialHandlingItems = ((IBusinessObjectCollection)(consolBusinessObject)["AWBSpecialHandlingItems"]).AddNew();
			awbSpecialHandlingItems[JobConsolAWBSpecialHandlingSchema.JKH_Code] = "SPX";

			var voyage = (BusinessObject)Factory.New<IJobVoyage>();
			voyage[JobVoyageSchema.JV_RV_NKVessel] = vessel;
			voyage[JobVoyageSchema.JV_VoyageFlight] = voyageNumber;
			var origin = (BusinessObject)Factory.New<IVoyageOrigin>();
			origin[JobVoyOriginSchema.JA_E_DEP] = leaveDate;
			origin[JobVoyOriginSchema.JA_JV] = voyage.PK;
			origin[JobVoyOriginSchema.JA_RL_NKPortOfLoading] = shipmentOrigin;

			var destination = (BusinessObject)Factory.New<IVoyageDestination>();
			destination[JobVoyDestinationSchema.JB_E_ARV] = arriveDate;
			destination[JobVoyDestinationSchema.JB_JV] = voyage.PK;
			destination[JobVoyDestinationSchema.JB_RL_NKPortOfDischarge] = shipmentDestination;

			var sailing = (BusinessObject)Factory.New<IJobSailing>();
			sailing[JobSailingSchema.JX_JA] = origin.PK;
			sailing[JobSailingSchema.JX_JB] = destination.PK;

			var transport = (BusinessObject)((IBusinessObjectCollection)((BusinessObject)bookingConsol)["Transports"])[0];
			transport[JobConsolTransportSchema.JW_RL_NKDiscPort] = shipmentOrigin;
			transport[JobConsolTransportSchema.JW_RL_NKLoadPort] = shipmentDestination;
			transport[JobConsolTransportSchema.JW_ETD] = leaveDate;
			transport[JobConsolTransportSchema.JW_VoyageFlight] = voyageNumber;
			transport[JobConsolTransportSchema.JW_ETA] = arriveDate;
			transport[JobConsolTransportSchema.JW_JX] = sailing.PK;

			var tbConsol = Helper.CreateConsolidation((IDtbBookingParent)consolBusinessObject);
			tbConsol.KB_GoodsDescription = goodsDescription;
			tbConsol.KB_JobDirection = direction;

			var booking = Helper.CreateBooking(tbConsol);

			_ = booking.ConsolidationSingleJob.AdditionalReferenceNumbers.AddNewIfNotExist(CargoWise.Definitions.AdditionalReferenceTypes.Codes.HouseBill, wayBillNumber);
			_ = booking.ConsolidationSingleJob.AdditionalReferenceNumbers.AddNewIfNotExist(CargoWise.Definitions.AdditionalReferenceTypes.Codes.MasterBill, wayBillNumber);

			var awbHeader = (BusinessObject)consolBusinessObject["AWBHeader"];
			awbHeader[ExportAWBHeaderSchema.EH_ParentID] = bookingConsol.PK;
			awbHeader[ExportAWBHeaderSchema.EH_AgentApprovalNumber] = regulatedAgentID;
			awbHeader[ExportAWBHeaderSchema.EH_RN_NKAgentApprovalCountryCode] = approvalCountryCode;

			Factory.Save();
			return booking;
		}

		DtbBookingConfirmation CreateConfirmationWithPackagesOnBooking(
			DtbBooking booking,
			List<TestContainerData> containers = null,
			List<TestPackageData> loosePackages = null)
		{
			var packageJob = booking.PackageJob;
			List<PkgPackage> pkgPackages = new();

			if (containers != null)
			{
				foreach (var container in containers)
				{
					var pkgPackage = Helper.CreatePackage(container.ContainerID, 1, "CNT");
					pkgPackage.Container.K0_RC_ContainerType = Helper.LoadRefContainer(container.ContainerType).PK;
					pkgPackage.KP_KJ_ParentPackageJob = packageJob.PK;
					pkgPackages.Add(pkgPackage);

					foreach (var containerSubPackage in container.SubPackages)
					{
						var innerPkgPackage = Helper.CreatePackage(ZString.Empty, containerSubPackage.Quantity, containerSubPackage.Type);
						pkgPackage.Packages.Add(innerPkgPackage);
						innerPkgPackage.KP_Weight = containerSubPackage.Weight;
						innerPkgPackage.KP_Volume = containerSubPackage.Volume;
						innerPkgPackage.KP_VolumeUQ = "M3";
						innerPkgPackage.KP_WeightUQ = "KG";
					}
				}
			}

			if (loosePackages != null)
			{
				foreach (var loosePackage in loosePackages)
				{
					var pkgPackage = Helper.CreatePackage(ZString.Empty, loosePackage.Quantity, loosePackage.Type);
					pkgPackage.KP_KJ_ParentPackageJob = packageJob.PK;
					pkgPackage.KP_Weight = loosePackage.Weight;
					pkgPackage.KP_Volume = loosePackage.Volume;
					pkgPackages.Add(pkgPackage);
				}
			}

			var orgType = "CFS";
			var instruction = Helper.CreateInstruction(booking, direction, orgType, null);

			foreach (var pkgPackage in pkgPackages)
			{
				_ = Helper.CreatePackageDivot(instruction, pkgPackage);
			}

			var confirmation = Helper.CreateConfirmation(instruction, direction);
			confirmation.KK_DocumentID = documentID;

			Factory.Save();
			return confirmation;
		}
	}
}
