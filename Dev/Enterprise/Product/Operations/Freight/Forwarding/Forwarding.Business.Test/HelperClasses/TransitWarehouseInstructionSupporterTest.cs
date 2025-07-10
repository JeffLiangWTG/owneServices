using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.Testing;
using Enterprise.Freight.Integration;
using Enterprise.Integration.TransitWarehouse;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Freight.Forwarding.Business.TransitWarehouseInstructionHelper;

namespace Enterprise.Freight.Business.Testing
{
	sealed class TransitWarehouseInstructionSupporterTest : TestCaseWithFactory
	{
		#region TestSendTransitWarehouseInstruction

		public void TestSendTransitWarehouseInstruction_Internally_Success()
		{
			TestSendTransitWarehouseInstruction_InternallyCore(new InfoNotification("Instruction successfully updated related job."), CargoWise.ComponentModel.NotificationType.Information);
		}

		public void TestSendTransitWarehouseInstruction_Internally_Error()
		{
			TestSendTransitWarehouseInstruction_InternallyCore(new ErrorNotification(ErrorType.Error, "Fail to update. Check DEX logs for detail."), CargoWise.ComponentModel.NotificationType.Error);
		}

		public void TestSendTransitWarehouseInstruction_Internally_Warning()
		{
			TestSendTransitWarehouseInstruction_InternallyCore(new WarningNotification("Instruction has been queued to send. Check DEX logs for details."), CargoWise.ComponentModel.NotificationType.Warning);
		}

		void TestSendTransitWarehouseInstruction_InternallyCore(INotification notificationForInstruction, INotificationType expectedNotificationType)
		{
			var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			var shipment = CreateShipment(orgProxy.MainAddress);

			CreateCommunicationMode(orgProxy, EDICommunicationsModeCommunicationsTransportList.Codes.UniversalDataBuss, "SHP", EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment);
			Factory.Save();

			var transitUniversalServiceMock = new Mock<ITransitUniversalService>();
			transitUniversalServiceMock.Setup(m => m.GetNotificationForInstruction(It.IsAny<string>(), It.IsAny<string>())).Returns(notificationForInstruction);

			using (ObjectFactory.Substitute(nameof(ITransitUniversalService), _ => transitUniversalServiceMock.Object))
			{
				var notifier = new DummyNotifications();
				var helper = new TransitWarehouseInstructionHelper(shipment, notifier);
				helper.SendTransitWarehouseInstruction(Direction.Pickup, ServiceRequest.Receipt);

				AssertEquals(expectedNotificationType, notifier.LastNotification.Type);
				AssertEquals($@"The Transit Warehouse Instruction has been sent.
Processing Shipment S00001000
Universal Shipment sent internally for Organization [EDICUS].

{notificationForInstruction.Message}", notifier.LastNotification.Message);
			}
		}

		public void TestSendTransitWarehouseInstruction_UsingAnotherFactoryForIndependentWork()
		{
			var notificationForInstruction = new InfoNotification("Instruction successfully updated related job.");
			var expectedNotificationType = CargoWise.ComponentModel.NotificationType.Information;

			var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			var shipment = CreateShipment(orgProxy.MainAddress);

			CreateCommunicationMode(orgProxy, EDICommunicationsModeCommunicationsTransportList.Codes.UniversalDataBuss, "SHP", EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment);
			Factory.Save();

			var transitUniversalServiceMock = new Mock<ITransitUniversalService>();
			transitUniversalServiceMock.Setup(m => m.GetNotificationForInstruction(It.IsAny<string>(), It.IsAny<string>())).Returns(() =>
			{
				throw new Exception();
			});

			using (ObjectFactory.Substitute(nameof(ITransitUniversalService), _ => transitUniversalServiceMock.Object))
			{
				var helper = new TransitWarehouseInstructionHelper(shipment, null);
				try
				{
					helper.SendTransitWarehouseInstruction(Direction.Pickup, ServiceRequest.Receipt);
				}
				catch (Exception)
				{
					AssertNoExceptionThrown(() => shipment.Factory.Save());
				}
			}
		}

		public void TestSendTransitWarehouseInstruction_Externally()
		{
			var depot = Factory.LoadTop1<OrgAddress>(new ZQuery());
			var shipment = CreateShipment(depot);

			CreateCommunicationMode(depot.Header, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, "SHP", EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment);

			var notifier = new DummyNotifications();

			var helper = new TransitWarehouseInstructionHelper(shipment, notifier);
			helper.SendTransitWarehouseInstruction(TransitWarehouseInstructionHelper.Direction.Pickup, TransitWarehouseInstructionHelper.ServiceRequest.Receipt);

			AssertEquals(CargoWise.ComponentModel.NotificationType.Error, notifier.LastNotification.Type);
			AssertEquals("Please save your changes before sending the Transit Warehouse Instruction.", notifier.LastNotification.Message);

			Factory.Save();

			var transitUniversalServiceMock = new Mock<ITransitUniversalService>();
			transitUniversalServiceMock
				.Setup(m => m.GetNotificationForInstruction(It.IsAny<string>(), It.IsAny<string>()))
				.Returns(new WarningNotification("Instruction has been queued to send. Check DEX logs for details."));

			using (ObjectFactory.Substitute(nameof(ITransitUniversalService), _ => transitUniversalServiceMock.Object))
			{
				helper.SendTransitWarehouseInstruction(TransitWarehouseInstructionHelper.Direction.Pickup, TransitWarehouseInstructionHelper.ServiceRequest.Receipt);
			}

			AssertEquals(CargoWise.ComponentModel.NotificationType.Warning, notifier.LastNotification.Type);
			AssertEquals(@"The Transit Warehouse Instruction has been sent.
Processing Shipment S00001000
Universal Shipment queued for sending to Organization [MIDINT].

Warning: Instruction has been queued to send. Check DEX logs for details.", notifier.LastNotification.Message);
		}

		public void TestSendTransitWarehouseInstructionForBoth_Externally()
		{
			var depot = Factory.LoadTop1<OrgAddress>(new ZQuery());
			var shipment = CreateShipment(depot);

			CreateCommunicationMode(depot.Header, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, "SHP", EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment);

			var notifier = new DummyNotifications();

			var helper = new TransitWarehouseInstructionHelper(shipment, notifier);
			helper.SendTransitWarehouseInstruction(TransitWarehouseInstructionHelper.Direction.Pickup, TransitWarehouseInstructionHelper.ServiceRequest.ReceiveAndDispatch);

			AssertEquals(CargoWise.ComponentModel.NotificationType.Error, notifier.LastNotification.Type);
			AssertEquals("Please save your changes before sending the Transit Warehouse Instruction.", notifier.LastNotification.Message);

			Factory.Save();

			var transitUniversalServiceMock = new Mock<ITransitUniversalService>();
			transitUniversalServiceMock
				.Setup(m => m.GetNotificationForInstruction(It.IsAny<string>(), It.IsAny<string>()))
				.Returns(new WarningNotification("Instruction has been queued to send. Check DEX logs for details."));

			using (ObjectFactory.Substitute(nameof(ITransitUniversalService), _ => transitUniversalServiceMock.Object))
			{
				helper.SendTransitWarehouseInstruction(TransitWarehouseInstructionHelper.Direction.Pickup, TransitWarehouseInstructionHelper.ServiceRequest.ReceiveAndDispatch);
			}

			AssertEquals(CargoWise.ComponentModel.NotificationType.Warning, notifier.LastNotification.Type);
			AssertEquals(@"The Transit Warehouse Instruction has been sent.
Processing Shipment S00001000
Universal Shipment queued for sending to Organization [MIDINT].

Warning: Instruction has been queued to send. Check DEX logs for details.
Processing Shipment S00001000
Universal Shipment queued for sending to Organization [MIDINT].

Warning: Instruction has been queued to send. Check DEX logs for details.", notifier.LastNotification.Message);
		}

		public void TestSendTransitWarehouseInstruction_DisposableManagerServiceInitialised()
		{
			var depot = Factory.LoadTop1<OrgAddress>(new ZQuery());
			var shipment = CreateShipment(depot);

			CreateCommunicationMode(depot.Header, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, "SHP", EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment);
			CreateCommunicationMode(depot.Header, EDICommunicationsModeCommunicationsTransportList.Codes.UniversalDataBuss, "SHP", EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment);

			var notifier = new DummyNotifications();

			Factory.Save();

			var helper = new TransitWarehouseInstructionHelper(shipment, notifier);
			AssertNoExceptionThrown("Disposable Manager Service should be initialised for factory",
				() => helper.SendTransitWarehouseInstruction(TransitWarehouseInstructionHelper.Direction.Pickup, TransitWarehouseInstructionHelper.ServiceRequest.Receipt));
		}

		public void TestCheckForGeneratePackagesWithIDs()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_TransportMode = "SEA";
			var notifier = new DummyNotifications();
			new TransitWarehouseInstructionHelper(shipment, notifier).GeneratePackagesWithIDs();
			AssertEquals("Please save your changes before generating packages with IDs.", notifier.LastNotification.Message);

			Factory.Save();
			notifier = new DummyNotifications();
			new TransitWarehouseInstructionHelper(shipment, notifier).GeneratePackagesWithIDs();
			AssertEquals(@"The House Bill number must be populated in the Basic Registration.", notifier.LastNotification.Message);

			shipment.JS_HouseBill = "SSYD0000";
			Factory.Save();
			notifier = new DummyNotifications();
			new TransitWarehouseInstructionHelper(shipment, notifier).GeneratePackagesWithIDs();
			AssertEquals("Please ensure that at least one pack line is entered in the Packing tab, with no linked package IDs against it.", notifier.LastNotification.Message);

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 3;
			packLine.JL_F3_NKPackType = "BAG";
			Factory.Save();
			notifier = new DummyNotifications();
			new TransitWarehouseInstructionHelper(shipment, notifier).GeneratePackagesWithIDs();
			AssertEquals(@"Transit Warehouse module is used to manage packages for your shipment.

Please ensure that Shipment > Pickup > CFS/Transit Warehouse organization is not blank and is set up as the proxy for a Transit Warehouse with valid EDI Communications setup (Organization > Config > Details > Config > EDI Communications).", notifier.LastNotification.Message);

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG";
			shipment.JS_OA_ExportReceivingDepot = orgHeader.MainAddress.PK;
			Factory.Save();
			notifier = new DummyNotifications();
			new TransitWarehouseInstructionHelper(shipment, notifier).GeneratePackagesWithIDs();
			AssertEquals(@"Transit Warehouse module is used to manage packages for your shipment.

Please ensure that Shipment > Pickup > CFS/Transit Warehouse organization is set up as the proxy for a Transit Warehouse and has a valid EDI Communications setup.
(Organization > Config > Details > Config > EDI Communication)", notifier.LastNotification.Message);

			var proxyCompany = Factory.New<GlbCompany>();
			proxyCompany.GC_Code = "Z1C";
			proxyCompany.GC_Name = "WENDY THE DESTROYER";
			proxyCompany.GC_IsActive = true;
			proxyCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			var branch = proxyCompany.Branches.AddNew();
			branch.FillWithValidTestData();
			branch.GB_Code = "C01";
			branch.GB_RL_NKHomePort = "AUSYD";

			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse = (IWhsWarehouse)helper.CreateTRWWarehouse();
			warehouse.WW_OA_WarehouseAddress = orgHeader.MainAddress.PK;
			warehouse.WW_GB_RelatedCompanyBranch = branch.PK;
			notifier = new DummyNotifications();
			new TransitWarehouseInstructionHelper(shipment, notifier).GeneratePackagesWithIDs();
			AssertEquals(@"Transit Warehouse module is used to manage packages for your shipment.

Please ensure that Shipment > Pickup > CFS/Transit Warehouse organization TESTORG is set up as the proxy for a Transit Warehouse and has a valid EDI Communications setup.
(Organization > Config > Details > Config > EDI Communication)", notifier.LastNotification.Message);

			proxyCompany.GC_OH_OrgProxy = orgHeader.PK;
			notifier = new DummyNotifications();
			new TransitWarehouseInstructionHelper(shipment, notifier).GeneratePackagesWithIDs();
			AssertEquals($@"3 Package IDs successfully created.

Use Transit Warehouse module to manage packages for this shipment (for example, manage outer and/or inner packs, references, seals, etc.)", notifier.LastNotification.Message);
		}

		public void TestCheckForGeneratePackagesWithIDsForSuccess()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_TransportMode = "SEA";
			shipment.JS_HouseBill = "SSYD0000";

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 3;
			packLine1.JL_F3_NKPackType = "BAG";
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 3;
			packLine2.JL_F3_NKPackType = "BAG";

			var warehouse = BuildWarehouse();
			shipment.JS_OA_ExportReceivingDepot = warehouse.WW_OA_WarehouseAddress;
			Factory.Save();
			var receiveConsignment = Factory.Load<ITransitReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, shipment.JS_HouseBill)).FirstOrDefault();
			AssertNull(receiveConsignment);
			var notifier = new DummyNotifications();
			new TransitWarehouseInstructionHelper(shipment, notifier).GeneratePackagesWithIDs();

			Assert("there should be no error", !notifier.HasError);

			receiveConsignment = Factory.Load<ITransitReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, shipment.JS_HouseBill)).FirstOrDefault();
			AssertNotNull(receiveConsignment);
			AssertEquals($@"6 Package IDs successfully created.

Use Transit Warehouse module to manage packages for this shipment (for example, manage outer and/or inner packs, references, seals, etc.)", notifier.LastNotification.Message);
		}

		public void TestGeneratePackagesWithIDs_UsingAnotherFactoryForSendTransitWarehouseInstruction()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_TransportMode = "SEA";
			shipment.JS_HouseBill = "SSYD0000";

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 3;
			packLine1.JL_F3_NKPackType = "BAG";
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 3;
			packLine2.JL_F3_NKPackType = "BAG";

			var warehouse = BuildWarehouse();
			shipment.JS_OA_ExportReceivingDepot = warehouse.WW_OA_WarehouseAddress;

			Factory.Save();

			var transitUniversalServiceMock = new Mock<ITransitUniversalService>();
			transitUniversalServiceMock.Setup(m => m.GetNotificationForInstruction(It.IsAny<string>(), It.IsAny<string>())).Returns(() =>
			{
				throw new Exception();
			});

			var notifier = new DummyNotifications();

			using (ObjectFactory.Substitute(nameof(ITransitUniversalService), _ => transitUniversalServiceMock.Object))
			{
				try
				{
					new TransitWarehouseInstructionHelper(shipment, notifier).GeneratePackagesWithIDs();
				}
				catch (Exception)
				{
					AssertNoExceptionThrown(() => shipment.Factory.Save());
				}
			}
		}

		#endregion

		public void TestSendTransitWarehouseInstruction_NotSend_WhenBlindPackageAttached()
		{
			InitializeTest(out var orgProxy, out var shipment, out var notifier);
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var warehouse = Factory.LoadTop1<IWhsWarehouse>(new ZQuery(WhsWarehouseSchema.WW_WarehouseCode, "TWH")) ?? (IWhsWarehouse)helper.CreateWarehouse("WAREHOUSE NAME", "TWH", "A");
			var receiveConsignment = helper.CreateReceiveConsignment("RC01", warehouse.PK);
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_JobID = "RC01";
			packageJob.KJ_ParentID = receiveConsignment.PK;
			packageJob.KJ_ParentTableCode = receiveConsignment.TablePrefix;
			var blindPackline = ForwardingShipmentTest.ReadOnlyPackageForLink.CreatePkgPackageSample(packageJob, "blindPKG");
			((ITransitWarehouseParent)shipment).AttachPackages(new ITransitPackage[] { blindPackline });
			var shipmentATCEventLogs = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, shipment.PK));
			Factory.Save();

			AssertEquals(1, shipmentATCEventLogs.Length);
			AssertEquals("|REF=1x Blind Transit Package(s) attached to Shipment.", shipmentATCEventLogs.First().SL_Reference);

			new TransitWarehouseInstructionHelper(shipment, notifier).SendTransitWarehouseInstruction(TransitWarehouseInstructionHelper.Direction.Pickup, TransitWarehouseInstructionHelper.ServiceRequest.Receipt);

			AssertEquals(CargoWise.ComponentModel.NotificationType.Error, notifier.LastNotification.Type);
			AssertEquals("Unable to send a receive instruction for Shipment with blind packages. Users need to send either a prepare to dispatch or dispatch instruction.", notifier.LastNotification.Message);
		}

		public void TestSendTransitWarehouseInstruction_NotSend_WhenBlindPackageAttached_multiplePacklines()
		{
			InitializeTest(out var orgProxy, out var shipment, out var notifier);
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var warehouse = Factory.LoadTop1<IWhsWarehouse>(new ZQuery(WhsWarehouseSchema.WW_WarehouseCode, "TWH")) ?? (IWhsWarehouse)helper.CreateWarehouse("WAREHOUSE NAME", "TWH", "A");
			var receiveConsignment = helper.CreateReceiveConsignment("RC01", warehouse.PK);
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_JobID = "RC01";
			packageJob.KJ_ParentID = receiveConsignment.PK;
			packageJob.KJ_ParentTableCode = receiveConsignment.TablePrefix;
			var blindPackline = ForwardingShipmentTest.ReadOnlyPackageForLink.CreatePkgPackageSample(packageJob, "blindPKG");
			shipment.AddPackLine();
			((ITransitWarehouseParent)shipment).AttachPackages(new ITransitPackage[] { blindPackline });
			var shipmentATCEventLogs = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, shipment.PK));

			Factory.Save();
			AssertEquals(1, shipmentATCEventLogs.Length);
			AssertEquals("|REF=1x Blind Transit Package(s) attached to Shipment.", shipmentATCEventLogs.First().SL_Reference);

			new TransitWarehouseInstructionHelper(shipment, notifier).SendTransitWarehouseInstruction(TransitWarehouseInstructionHelper.Direction.Pickup, TransitWarehouseInstructionHelper.ServiceRequest.Receipt);

			AssertEquals(CargoWise.ComponentModel.NotificationType.Error, notifier.LastNotification.Type);
			AssertEquals("Unable to send a receive instruction for Shipment with blind packages. Users need to send either a prepare to dispatch or dispatch instruction.", notifier.LastNotification.Message);
		}

		#region TestSendTransitWarehouseDispatchStopLoadInstruction

		public void TestSendTransitWarehouseDispatchStopLoadInstruction_Internally_Success()
		{
			TestSendTransitWarehouseDispatchStopLoadInstruction_InternallyCore(new InfoNotification("Successfully stopped Load."), CargoWise.ComponentModel.NotificationType.Information);
		}

		public void TestSendTransitWarehouseDispatchStopLoadInstruction_Internally_Error()
		{
			TestSendTransitWarehouseDispatchStopLoadInstruction_InternallyCore(new ErrorNotification(ErrorType.Error, "Could not stop Load. Check DEX logs for details."), CargoWise.ComponentModel.NotificationType.Error);
		}

		public void TestSendTransitWarehouseDispatchStopLoadInstruction_Internally_Warning()
		{
			TestSendTransitWarehouseDispatchStopLoadInstruction_InternallyCore(new WarningNotification("Instruction has been queued to send. Check DEX logs for details."), CargoWise.ComponentModel.NotificationType.Warning);
		}

		void TestSendTransitWarehouseDispatchStopLoadInstruction_InternallyCore(INotification notificationForInstruction, INotificationType expectedNotificationType)
		{
			var depot = Factory.LoadTop1<OrgAddress>(new ZQuery());
			var consol = CreateConsol(depot);

			CreateCommunicationMode(depot.Header, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, "CON", EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent);
			Factory.Save();

			var transitUniversalServiceMock = new Mock<ITransitUniversalService>();
			transitUniversalServiceMock
				.Setup(m => m.GetNotificationForStopLoadInstructionEvent(It.IsAny<string>(), It.IsAny<bool>()))
				.Returns(notificationForInstruction);

			using (ObjectFactory.Substitute(nameof(ITransitUniversalService), _ => transitUniversalServiceMock.Object))
			{
				var notifier = new DummyNotifications();
				var helper = new TransitWarehouseInstructionHelper(consol, notifier);
				helper.SendTransitWarehouseDispatchStopLoadInstruction(TransitWarehouseInstructionHelper.Direction.Pickup, false, true);

				AssertEquals(expectedNotificationType, notifier.LastNotification.Type);
				AssertEquals($@"The Transit Warehouse Instruction has been sent.
Processing Consol C00001111
Universal Event queued for sending to Organization [MIDINT].

{notificationForInstruction.Message}", notifier.LastNotification.Message);
			}
		}

		public void TestSendTransitWarehouseDispatchStopLoadInstruction_Externally()
		{
			var depot = Factory.LoadTop1<OrgAddress>(new ZQuery());
			var consol = CreateConsol(depot);

			CreateCommunicationMode(depot.Header, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, "CON", EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent);

			var notifier = new DummyNotifications();

			var helper = new TransitWarehouseInstructionHelper(consol, notifier);
			helper.SendTransitWarehouseDispatchStopLoadInstruction(TransitWarehouseInstructionHelper.Direction.Pickup, false, true);

			AssertEquals(CargoWise.ComponentModel.NotificationType.Error, notifier.LastNotification.Type);
			AssertEquals("Please save your changes before sending the Transit Warehouse Instruction.", notifier.LastNotification.Message);

			Factory.Save();

			var transitUniversalServiceMock = new Mock<ITransitUniversalService>();
			transitUniversalServiceMock
				.Setup(m => m.GetNotificationForStopLoadInstructionEvent(It.IsAny<string>(), It.IsAny<bool>()))
				.Returns(new WarningNotification("Instruction has been queued to send. Check DEX logs for details."));

			using (ObjectFactory.Substitute(nameof(ITransitUniversalService), _ => transitUniversalServiceMock.Object))
			{
				helper.SendTransitWarehouseDispatchStopLoadInstruction(TransitWarehouseInstructionHelper.Direction.Pickup, false, true);
			}

			AssertEquals(CargoWise.ComponentModel.NotificationType.Warning, notifier.LastNotification.Type);
			AssertEquals(@"The Transit Warehouse Instruction has been sent.
Processing Consol C00001111
Universal Event queued for sending to Organization [MIDINT].

Warning: Instruction has been queued to send. Check DEX logs for details.", notifier.LastNotification.Message);
		}

		public void TestSendTransitWarehouseDispatchStopLoadInstruction_DispatchRequestedInvalid()
		{
			var depot = Factory.LoadTop1<OrgAddress>(new ZQuery());
			var consol = CreateConsol(depot);

			CreateCommunicationMode(depot.Header, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, "CON", EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent);
			Factory.Save();

			var notifier = new DummyNotifications();

			var helper = new TransitWarehouseInstructionHelper(consol, notifier);
			helper.SendTransitWarehouseDispatchStopLoadInstruction(TransitWarehouseInstructionHelper.Direction.Pickup, false, false);

			AssertEquals(CargoWise.ComponentModel.NotificationType.Error, notifier.LastNotification.Type);
			AssertEquals("The Departure Dispatch Instructions must be sent before a Load can be stopped.", notifier.LastNotification.Message);
		}

		public void TestSendTransitWarehouseDispatchStopLoadInstruction_DisposableManagerServiceInitialised()
		{
			var depot = Factory.LoadTop1<OrgAddress>(new ZQuery());
			var consol = CreateConsol(depot);

			CreateCommunicationMode(depot.Header, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, "CON", EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent);
			CreateCommunicationMode(depot.Header, EDICommunicationsModeCommunicationsTransportList.Codes.UniversalDataBuss, "CON", EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent);

			var notifier = new DummyNotifications();

			Factory.Save();

			var helper = new TransitWarehouseInstructionHelper(consol, notifier);
			AssertNoExceptionThrown("Disposable Manager Service should be initialised for factory",
				() => helper.SendTransitWarehouseDispatchStopLoadInstruction(TransitWarehouseInstructionHelper.Direction.Pickup, false, true));
		}

		public void TestSendTransitWarehouseDispatchStopLoadInstruction_LogCanBeGenerated()
		{
			var depot = Factory.LoadTop1<OrgAddress>(new ZQuery());
			var consol = CreateConsol(depot);
			CreateCommunicationMode(depot.Header, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, "CON", EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent);
			Factory.Save();

			var notifier = new DummyNotifications();
			var helper = new TransitWarehouseInstructionHelper(consol, notifier);
			helper.SendTransitWarehouseDispatchStopLoadInstruction(TransitWarehouseInstructionHelper.Direction.Pickup, false, true);
			consol.Refresh();

			var logs = consol.Logs.GetAllLogs();
			var log = logs.Cast<StmALog>().Single(l => l.SL_SE_NKEvent == AutoEvents.ServiceSuspendedCode);
			var expectedReference = $"Load Service Suspended at Depot {depot.PortName}, {depot.OA_State}, {depot.OA_RN_NKCountryCode} {TransitUniversalServiceResult.Queued}";
			AssertNotNull("SVS or SVR event created", log.DisplayEventReference);
			AssertEquals("SVS or SVR event reference", expectedReference, log.DisplayEventReference);

			helper.SendTransitWarehouseDispatchStopLoadInstruction(TransitWarehouseInstructionHelper.Direction.Pickup, true, true);
			consol.Refresh();

			logs = consol.Logs.GetAllLogs();
			log = logs.Cast<StmALog>().Single(l => l.SL_SE_NKEvent == AutoEvents.ServiceRequestedCode);
			expectedReference = $"Load Service Requested at Depot {depot.PortName}, {depot.OA_State}, {depot.OA_RN_NKCountryCode} {TransitUniversalServiceResult.Queued}";
			AssertNotNull("SVS or SVR event created", log.DisplayEventReference);
			AssertEquals("SVS or SVR event reference", expectedReference, log.DisplayEventReference);
		}

		#endregion

		#region TestSendTransitWarehousePrepareDispatchInstruction

		public void TestSendTransitWarehousePrepareDispatchInstructionForBothDirection_Externally()
		{
			var originDepot = Factory.LoadTop1<OrgAddress>(new ZQuery());
			var destinationDepot = Factory.LoadTop1<OrgAddress>(new ZQuery());
			var shipment = CreateShipment(originDepot);
			shipment.JS_OA_ImportReleaseDepot = destinationDepot.PK;

			CreateCommunicationMode(originDepot.Header, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, "SHP", EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment);
			CreateCommunicationMode(destinationDepot.Header, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, "SHP", EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment);

			var notifier = new DummyNotifications();

			var helper = new TransitWarehouseInstructionHelper(shipment, notifier);
			helper.SendTransitWarehouseInstruction(Direction.Both, ServiceRequest.PrepareDispatch);

			AssertEquals(CargoWise.ComponentModel.NotificationType.Error, notifier.LastNotification.Type);
			AssertEquals("Please save your changes before sending the Transit Warehouse Instruction.", notifier.LastNotification.Message);

			Factory.Save();

			var transitUniversalServiceMock = new Mock<ITransitUniversalService>();
			transitUniversalServiceMock
				.Setup(m => m.GetNotificationForInstruction(It.IsAny<string>(), It.IsAny<string>()))
				.Returns(new WarningNotification("Instruction has been queued to send. Check DEX logs for details."));

			using (ObjectFactory.Substitute(nameof(ITransitUniversalService), _ => transitUniversalServiceMock.Object))
			{
				helper.SendTransitWarehouseInstruction(Direction.Both, ServiceRequest.PrepareDispatch);
			}

			AssertEquals(CargoWise.ComponentModel.NotificationType.Warning, notifier.LastNotification.Type);
			AssertEquals(@"The Transit Warehouse Instruction has been sent.
Processing Shipment S00001000
Universal Shipment queued for sending to Organization [MIDINT].

Warning: Instruction has been queued to send. Check DEX logs for details.
Processing Shipment S00001000
Universal Shipment queued for sending to Organization [MIDINT].

Warning: Instruction has been queued to send. Check DEX logs for details.", notifier.LastNotification.Message);
		}

		public void TestSendTransitWarehousePrepareDispatchInstruction_UsingAnotherFactory()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_TransportMode = "SEA";
			shipment.JS_HouseBill = "SSYD0000";

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 3;
			packLine1.JL_F3_NKPackType = "BAG";
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 3;
			packLine2.JL_F3_NKPackType = "BAG";

			var warehouse = BuildWarehouse();
			shipment.JS_OA_ExportReceivingDepot = warehouse.WW_OA_WarehouseAddress;

			Factory.Save();

			var transitUniversalServiceMock = new Mock<ITransitUniversalService>();
			transitUniversalServiceMock.Setup(m => m.GetNotificationForInstruction(It.IsAny<string>(), It.IsAny<string>())).Returns(() =>
			{
				throw new Exception();
			});

			var notifier = new DummyNotifications();

			using (ObjectFactory.Substitute(nameof(ITransitUniversalService), _ => transitUniversalServiceMock.Object))
			{
				try
				{
					new TransitWarehouseInstructionHelper(shipment, notifier).SendTransitWarehouseInstruction(Direction.Pickup, ServiceRequest.PrepareDispatch);
				}
				catch (Exception)
				{
					AssertNoExceptionThrown(() => shipment.Factory.Save());
				}
			}
		}

		public void TestSendTransitWarehousePrepareDispatchInstruction_LogCanBeGenerated()
		{
			var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			var shipment = CreateShipment(orgProxy.MainAddress);

			CreateCommunicationMode(orgProxy, EDICommunicationsModeCommunicationsTransportList.Codes.UniversalDataBuss, "SHP", EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment);
			Factory.Save();
			CombineAssertions("Pre-conditions", () =>
			{
				var supporter = shipment as ITransitWarehouseInstructionSupporter;
				AssertEquals(ZDateTime.Empty, supporter.PickupReceiptRequestedDate);
				AssertEquals(ZDateTime.Empty, supporter.PickupDispatchRequestedDate);
				AssertEquals(ZDateTime.Empty, supporter.DeliveryReceiptRequestedDate);
				AssertEquals(ZDateTime.Empty, supporter.DeliveryDispatchRequestedDate);
			});

			var transitUniversalServiceMock = new Mock<ITransitUniversalService>();
			var notificationForInstruction = new InfoNotification("Instruction successfully updated related job.");
			transitUniversalServiceMock.Setup(m => m.GetNotificationForInstruction(It.IsAny<string>(), It.IsAny<string>())).Returns(notificationForInstruction);

			using (ObjectFactory.Substitute(nameof(ITransitUniversalService), _ => transitUniversalServiceMock.Object))
			{
				var notifier = new DummyNotifications();
				var helper = new TransitWarehouseInstructionHelper(shipment, notifier);
				helper.SendTransitWarehouseInstruction(Direction.Pickup, ServiceRequest.PrepareDispatch);

				AssertEquals(CargoWise.ComponentModel.NotificationType.Information, notifier.LastNotification.Type);
				AssertEquals($@"The Transit Warehouse Instruction has been sent.
Processing Shipment S00001000
Universal Shipment sent internally for Organization [EDICUS].

{notificationForInstruction.Message}", notifier.LastNotification.Message);
			}

			var newFactory = Factory.CreateNewFactory();
			shipment = newFactory.Load<ForwardingShipment>(shipment.PK);
			AssertSVREventIsLoggedOnShipment(shipment, orgProxy);
			CombineAssertions("No requested dates are updated", () =>
			{
				var supporter = shipment as ITransitWarehouseInstructionSupporter;
				AssertEquals(ZDateTime.Empty, supporter.PickupReceiptRequestedDate);
				AssertEquals(ZDateTime.Empty, supporter.PickupDispatchRequestedDate);
				AssertEquals(ZDateTime.Empty, supporter.DeliveryReceiptRequestedDate);
				AssertEquals(ZDateTime.Empty, supporter.DeliveryDispatchRequestedDate);
			});
		}

		public void TestSendTransitWarehouseShipmentsPrepareDispatchInstructions_FromConsol()
		{
			var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);

			var consol = CreateConsol(orgProxy.MainAddress);
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			shipment1.JS_OA_ExportReceivingDepot = orgProxy.MainAddress.PK;
			shipment2.JS_OA_ExportReceivingDepot = orgProxy.MainAddress.PK;

			Factory.Save();

			var transitUniversalServiceMock = new Mock<ITransitUniversalService>();
			var notificationForInstruction = new InfoNotification("Instruction successfully updated related job.");
			transitUniversalServiceMock.Setup(m => m.GetNotificationForInstruction(It.IsAny<string>(), It.IsAny<string>())).Returns(notificationForInstruction);

			using (ObjectFactory.Substitute(nameof(ITransitUniversalService), _ => transitUniversalServiceMock.Object))
			{
				var notifier = new DummyNotifications();
				var helper = new TransitWarehouseInstructionHelper(consol, notifier,
					factory =>
					{
						var consolForPrepareDispatch = new ConsolPrepareForDispatchInstruction(consol, TransitWarehouseInstructionHelper.Direction.Pickup);
						var shipmentsForSelection = consolForPrepareDispatch.ShipmentsForSelection;
						shipmentsForSelection[0].SelectedForDelivery = true;
						shipmentsForSelection[1].SelectedForDelivery = true;
						return new ManualDataExport(factory, new[] { consol }, UniversalDataType.UniversalShipment, null, null, null,
							manager => ObjectFactory.Get<IConsolPrepareForDispatchDataObjectWriter>(nameof(IConsolPrepareForDispatchDataObjectWriter), manager, consolForPrepareDispatch));
					},
					new ITransitWarehouseInstructionSupporter[] { shipment1, shipment2 });
				AssertNoExceptionThrown(() => helper.SendTransitWarehouseInstruction(Direction.Pickup, ServiceRequest.PrepareDispatch));

				AssertEquals(CargoWise.ComponentModel.NotificationType.Information, notifier.LastNotification.Type);
				AssertEquals($@"The Transit Warehouse Instruction has been sent.
Processing Consol C00001111
Universal Shipment sent internally for Organization [EDICUS].

{notificationForInstruction.Message}", notifier.LastNotification.Message);

				var newFactory = Factory.CreateNewFactory();
				shipment1 = newFactory.Load<ForwardingShipment>(shipment1.PK);
				shipment2 = newFactory.Load<ForwardingShipment>(shipment2.PK);
				AssertSVREventIsLoggedOnShipment(shipment1, orgProxy);
				AssertSVREventIsLoggedOnShipment(shipment2, orgProxy);
			}
		}

		void AssertSVREventIsLoggedOnShipment(ForwardingShipment shipment, OrgHeader warehouse)
		{
			shipment.Refresh();
			var logs = shipment.Logs.GetAllLogs();
			var log = logs.Cast<StmALog>().Single(l => l.SL_SE_NKEvent == AutoEvents.ServiceRequestedCode);
			Assert("SVR event saved", log.IsInDatabase);
			var expectedReference = "Prepare Dispatch Service Requested at Depot " +
				$"{warehouse.OH_Code} " +
				$"{warehouse.MainAddress.EffectiveRelatedPortCode?.RL_PortName ?? ZString.Empty}, " +
				$"{warehouse.MainAddress.EffectiveRelatedPortCode?.CountryStates?.RW_Code ?? ZString.Empty}, " +
				$"{warehouse.MainAddress?.EffectiveRelatedPortCode?.RL_RN_NKCountryCode ?? ZString.Empty}";
			AssertEquals("SVR event reference", expectedReference, log.DisplayEventReference);
		}

		#endregion

		void InitializeTest(out OrgHeader orgProxy, out ForwardingShipment shipment, out DummyNotifications notifier)
		{
			orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			shipment = CreateShipment(orgProxy.MainAddress);
			CreateCommunicationMode(orgProxy, EDICommunicationsModeCommunicationsTransportList.Codes.UniversalDataBuss, "SHP", EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment);
			notifier = new DummyNotifications();
		}

		ForwardingConsol CreateConsol(OrgAddress depotAddress)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001111";
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_OA_PackDepotAddress = depotAddress.PK;

			return consol;
		}

		public void TestSplitForGeneratePackagesWithIDs()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_HouseBill = "SSYD0000";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_TransportMode = "SEA";
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.FillWithValidTestData();
			packLine1.JL_PackageCount = 1;
			packLine1.JL_PackLineId = "PID0001";
			packLine1.JL_F3_NKPackType = "PLT";
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.FillWithValidTestData();
			packLine2.JL_PackageCount = 3;
			packLine2.JL_PackLineId = "PID0002";
			packLine2.JL_F3_NKPackType = "BAG";
			var packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.FillWithValidTestData();
			packLine3.JL_PackageCount = 1;
			packLine3.JL_PackLineId = "PID0003";
			packLine3.JL_F3_NKPackType = "PLT";
			packLine3.GeneratePackageWithIDs();
			packLine3.PkgPackageCollection[0].KP_PackageID = "003-01";
			var packLine4 = shipment.OuterPackLines.AddNew();
			packLine4.FillWithValidTestData();
			packLine4.JL_PackageCount = 3;
			packLine4.JL_PackLineId = "PID0004";
			packLine4.JL_F3_NKPackType = "PLT";
			packLine4.GeneratePackageWithIDs();
			packLine4.GeneratePackageWithIDs();
			packLine4.GeneratePackageWithIDs();
			packLine4.PkgPackageCollection[0].KP_PackageID = "004-01";
			packLine4.PkgPackageCollection[1].KP_PackageID = "004-02";
			packLine4.PkgPackageCollection[2].KP_PackageID = "004-03";
			var packLine5 = shipment.OuterPackLines.AddNew();
			packLine5.FillWithValidTestData();
			packLine5.JL_PackageCount = 3;
			packLine5.JL_PackLineId = "PID0005";
			packLine5.JL_F3_NKPackType = "PLT";
			packLine5.GeneratePackageWithIDs();
			packLine5.PkgPackageCollection[0].KP_PackageID = "005-01";

			var warehouse = BuildWarehouse();
			shipment.JS_OA_ExportReceivingDepot = warehouse.WW_OA_WarehouseAddress;

			Factory.Save();

			var receiveConsignment = Factory.Load<ITransitReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, shipment.JS_HouseBill)).FirstOrDefault();
			AssertNull(receiveConsignment);

			var notifier = new DummyNotifications();
			new TransitWarehouseInstructionHelper(shipment, notifier).GeneratePackagesWithIDs();

			receiveConsignment = Factory.Load<ITransitReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, shipment.JS_HouseBill)).FirstOrDefault();
			AssertNotNull(receiveConsignment);

			Assert("there should be no error", !notifier.HasError);
			AssertEquals(1, packLine1.PkgPackageCollection.Count);
			AssertContainsExactElementsInAnyOrder(new ZString[] { shipment.JS_UniqueConsignRef + "-001" }, packLine1.PkgPackageCollection.Select(pkg => pkg.KP_PackageID));
			AssertEquals(3, packLine2.PkgPackageCollection.Count);
			AssertContainsExactElementsInAnyOrder(new ZString[] { shipment.JS_UniqueConsignRef + "-002", shipment.JS_UniqueConsignRef + "-003", shipment.JS_UniqueConsignRef + "-004" }, packLine2.PkgPackageCollection.Select(pkg => pkg.KP_PackageID));
			AssertEquals(1, packLine3.PkgPackageCollection.Count);
			AssertEquals(3, packLine4.PkgPackageCollection.Count);
			AssertEquals(1, packLine5.PkgPackageCollection.Count);
		}

		IWhsWarehouse BuildWarehouse()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG2";
			var proxyCompany = Factory.New<GlbCompany>();
			proxyCompany.GC_Code = "Z1C";
			proxyCompany.GC_Name = "WENDY THE DESTROYER";
			proxyCompany.GC_IsActive = true;
			proxyCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			proxyCompany.GC_OH_OrgProxy = orgHeader.PK;

			var branch = proxyCompany.Branches.AddNew();
			branch.FillWithValidTestData();
			branch.GB_Code = "C01";
			branch.GB_RL_NKHomePort = "AUSYD";

			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse = (IWhsWarehouse)helper.CreateTRWWarehouse();
			warehouse.WW_OA_WarehouseAddress = orgHeader.MainAddress.PK;
			warehouse.WW_GB_RelatedCompanyBranch = branch.PK;

			return warehouse;
		}

		ForwardingShipment CreateShipment(OrgAddress depotAddress)
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001000";
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_OA_ExportReceivingDepot = depotAddress.PK;

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
