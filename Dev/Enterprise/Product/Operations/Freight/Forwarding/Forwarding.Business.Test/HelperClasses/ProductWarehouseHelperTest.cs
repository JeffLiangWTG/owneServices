using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ProductWarehouseHelperTest : TestCaseWithFactory
	{
		public void TestCreateProductWarehouseReceive_Success()
		{
			TestCreateProductWarehouseReceiveCore(new InfoNotification("Instruction successfully updated related Warehouse Receive."), CargoWise.ComponentModel.NotificationType.Information);
		}

		public void TestCreateProductWarehouseReceive_Error()
		{
			TestCreateProductWarehouseReceiveCore(new ErrorNotification(ErrorType.Error, "Instruction had an issue while processing (DCD - Discarded). Check DEX logs for details."), CargoWise.ComponentModel.NotificationType.Error);
		}

		void TestCreateProductWarehouseReceiveCore(INotification notificationForInstruction, INotificationType expectedNotificationType)
		{
			var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			var shipment = CreateShipment(orgProxy);
			if (expectedNotificationType == NotificationType.Information)
			{
				BuildWarehouse(orgProxy);
			}
			shipment.DocAddresses.CreateWithAddressType(DocAddressType.Warehouse).E2_OA_Address = orgProxy.MainAddress.PK;
			shipment.DocAddresses.CreateWithAddressType(DocAddressType.WarehouseClient).E2_OA_Address = orgProxy.MainAddress.PK;

			CreateCommunicationMode(orgProxy, EDICommunicationsModeCommunicationsTransportList.Codes.UniversalDataBuss, "SHP", EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment);
			Factory.Save();

			var notifier = new DummyNotifications();
			var helper = new ProductWarehouseHelper(shipment, notifier, null);
			helper.CreateProductWarehouseReceive();

			AssertEquals(expectedNotificationType, notifier.LastNotification.Type);
			AssertEquals($@"The Warehouse Receive Instruction has been sent.
Processing Shipment S00001000
Universal Shipment sent internally for Organization [EDICUS].

{notificationForInstruction.Message}", notifier.LastNotification.Message);

			var dataExportLog = Factory.LoadTop1<StmALog>(new ZQuery(StmALogSchema.SL_Parent, shipment.PK).AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.DataExportCode));

			AssertNotNull("DEX event against shipment should be created.", dataExportLog);
			AssertNotNull("Related EDI message should be created.", dataExportLog.RelatedEDIMessage);

			var notes = ((EDIMessage)dataExportLog.RelatedEDIMessage.Message).Notes.GetAllNotes().Cast<StmNote>().ToArray();

			AssertEquals("Related Notes should be created.", 1, notes.Length);
			AssertEquals("Data import log should be created", "Data Import Log Text", notes[0].ST_Description);
		}

		IWhsWarehouse BuildWarehouse(OrgHeader client)
		{
			client.OH_IsWarehouseClient = true;
			var proxyCompany = Factory.New<GlbCompany>();
			proxyCompany.GC_Code = "Z1C";
			proxyCompany.GC_Name = "WENDY THE DESTROYER";
			proxyCompany.GC_IsActive = true;
			proxyCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			proxyCompany.GC_OH_OrgProxy = client.PK;

			var branch = proxyCompany.Branches.AddNew();
			branch.FillWithValidTestData();
			branch.GB_Code = "C01";
			branch.GB_RL_NKHomePort = "AUSYD";

			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse = (IWhsWarehouse)helper.CreateWarehouse("WHS");
			warehouse.WW_OA_WarehouseAddress = client.MainAddress.PK;
			warehouse.WW_GB_RelatedCompanyBranch = branch.PK;

			return warehouse;
		}

		ForwardingShipment CreateShipment(OrgHeader consignee)
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001000";
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_OA_ExportReceivingDepot = consignee.MainAddress.PK;
			shipment.ConsigneeDeliveryAddress.OrganisationPK = consignee.PK;

			return shipment;
		}

		void CreateCommunicationMode(OrgHeader client, ZString communicationTransport, string module, string fileFormat)
		{
			var communicationMode = client.EDICommunicationsModes.AddNew();
			communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationMode.EK_CommunicationsTransport = communicationTransport;
			communicationMode.EK_Destination = "Blah";
			communicationMode.EK_FileFormat = fileFormat;
			communicationMode.EK_Module = module;
		}

		class DummyNotifications : INotifications
		{
			public void Add(INotification notification)
			{
				LastNotification = notification;
				notifications.Add(notification);
			}

			public INotification LastNotification { get; private set; }

			readonly List<INotification> notifications = new List<INotification>();

			public bool HasError
			{
				get => notifications.Any(n => n.Type == CargoWise.ComponentModel.NotificationType.Error || n.Type == CargoWise.ComponentModel.NotificationType.Warning);
			}
		}
	}
}
