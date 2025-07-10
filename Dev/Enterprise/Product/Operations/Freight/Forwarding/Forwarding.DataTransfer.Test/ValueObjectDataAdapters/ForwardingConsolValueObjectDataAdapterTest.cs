using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using IBaseJobDeclaration = Enterprise.Integration.Customs.IBaseJobDeclaration;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(ForwardingConsolValueObjectDataAdapter))]
	public class ForwardingConsolValueObjectDataAdapterTest : BaseConsolValueObjectDataAdapterTest<ForwardingConsol>
	{
		public void TestExportContainers()
		{
			var consol = Factory.New<ForwardingConsol>();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "AAAA256426";
			consol.Containers.AddNew();
			consol.Containers.AddNew();

			var shipment = GetNewShipment(consol, "blah");
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_JC = container1.PK;

			var adapter = new ForwardingConsolValueObjectDataAdapterForTest(shipment, null);
			var consolValue = new Xsd.Consol();
			adapter.ExportContainers(consol, consolValue, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals(1, consolValue.ConsolDetail.Containers.Count);
			AssertEquals("AAAA256426", consolValue.ConsolDetail.Containers[0].ContainerNumber);
		}

		public void TestAutomaticallySendCargoMessage()
		{
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_OH_OrgProxy = Factory.NewWithValidTestData<OrgHeader>().PK;
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			OrgCusCode cusCode = company.OrgProxy.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			cusCode.OK_CustomsRegNo = "14 001 592 650";
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			GlbBranch branch = company.Branches.AddNew();
			branch.GB_Code = "TMP";
			branch.GB_RL_NKHomePort = "AUSYD";
			Factory.Save();

			SystemDataRegistry.Instance.AutomaticallySendAirCargoMessage.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			Xsd.Consol consolValue = new Xsd.Consol();
			consolValue.ConsolDetail.TransportMode = Xsd.ConsolTransportMode.AIR;
			Xsd.ConsolIdentifier identifier = consolValue.ConsolIdentifier.AddNew();
			identifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			identifier.Value = "32236346553";
			consolValue.ConsolDetail.PortOfLoading.Port.Value = "UAIEV";
			consolValue.ConsolDetail.PortOfLoading.EstimatedDateTime = ZDateTime.Today;
			consolValue.ConsolDetail.PortOfDischarge.Port.Value = "AUSYD";
			consolValue.ConsolDetail.PortOfDischarge.EstimatedDateTime = ZDateTime.Today.AddDays(1);
			Xsd.PlannedLeg leg = consolValue.ConsolDetail.PlannedLegs.AddNew();
			leg.PortOfLoading.Port.Value = "UAIEV";
			leg.PortOfLoading.EstimatedDateTime = ZDateTime.Today;
			leg.PortOfDischarge.Port.Value = "AUSYD";
			leg.PortOfDischarge.EstimatedDateTime = ZDateTime.Today.AddDays(1);
			leg.TransportMode = Xsd.TransportMode.AIR;
			Xsd.FlightWithFlightNumber flight = new Xsd.FlightWithFlightNumber();
			flight.FlightNoJourneyNoTruckRegNo = "VV2346";
			leg.Item = flight;
			consolValue.ConsolDetail.PaymentType = Xsd.PaymentType.PPD;
			Xsd.Shipment shipmentValue = consolValue.Shipments.AddNew();
			shipmentValue.ShipmentDetailsSpecified = true;
			shipmentValue.ShipmentDetails.Incoterm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			shipmentValue.ShipmentDetails.PackingMode = Xsd.ContainerMode.LCL;
			shipmentValue.ShipmentDetails.TransportMode = Xsd.TransportMode.AIR;
			shipmentValue.ShipmentDetails.Consignor = new Xsd.Organisation();
			shipmentValue.ShipmentDetails.Consignor.EDICode = "CONSNOR";
			shipmentValue.ShipmentDetails.Consignor.OrganisationDetails.Name = "consignor";
			shipmentValue.ShipmentDetails.Consignor.OrganisationDetails.Location.Value = "UAIEV";
			shipmentValue.ShipmentDetails.Consignee = new Xsd.Organisation();
			shipmentValue.ShipmentDetails.Consignee.EDICode = "CONSNEE";
			shipmentValue.ShipmentDetails.Consignee.OrganisationDetails.Name = "consignee";
			shipmentValue.ShipmentDetails.GoodsDescription = "Downsized Developers";
			Xsd.ShipmentIdentifier shipIdentifier = shipmentValue.ShipmentIdentifier.AddNew();
			shipIdentifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			shipIdentifier.Value = "363463634";
			shipmentValue.ShipmentDetails.PortOfOrigin.Port.Value = "UAIEV";
			shipmentValue.ShipmentDetails.PortofDestination.Port.Value = "AUSYD";
			shipmentValue.ShipmentDetails.Weight.Value = 1;
			shipmentValue.ShipmentDetails.Weight.DimensionType = "KG";
			shipmentValue.ShipmentDetails.Volume.Value = 1;
			shipmentValue.ShipmentDetails.Volume.DimensionType = "M3";
			shipmentValue.ShipmentDetails.GoodsValue.Value = 1;
			shipmentValue.ShipmentDetails.GoodsValue.CurrencyCode = "USD";
			shipmentValue.ShipmentDetails.ShipmentType = Xsd.ShipmentType.EXP;
			shipmentValue.ShipmentDetails.TotalOuterPacksQty.Value = 1;
			shipmentValue.ShipmentDetails.TotalInnerPacksQty.Value = 1;

			ForwardingConsolValueObjectDataAdapterForTest dataAdapter = new ForwardingConsolValueObjectDataAdapterForTest(true);
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Buffer);

			int messageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
			using (branch.SetAsTemporaryContext())
			{
				dataAdapter.CreateOrUpdateFromValueObject(consolValue, context);
			}
			AssertEquals(messageCount + 1, Factory.GetDatabaseCount(typeof(EDIMessage)));
		}

		public void TestManualImportForShipment()
		{
			ForwardingShipment shipment = GetShipment();
			Xsd.Shipment shipmentValue = GetShipmentValue();

			ForwardingConsolValueObjectDataAdapterForTest adapter = new ForwardingConsolValueObjectDataAdapterForTest(true);
			Buffer.Clear();

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Buffer);

			SystemDataRegistry.Instance.AutoCreateDeclarationWithinShipment.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("Precondition: shipment has no declaration", true, shipment.Declarations.Length == 0);

			adapter.GenerateDeclarationForShipment(shipmentValue, shipment, context);

			AssertEquals("shipment has no declaration as registry is set to false", true, shipment.Declarations.Length == 0);

			SystemDataRegistry.Instance.AutoCreateDeclarationWithinShipment.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			Buffer.Clear();
			AssertNotEquals("Current branch is not the branch with homeport = 'AUBNE'", GlbBranch.CurrentBranch.GB_Code, AnotherNewBranch.GB_Code);
			adapter.GenerateDeclarationForShipment(shipmentValue, shipment, context);
			AssertEquals("shipment has no declaration as registry is set to false", true, shipment.Declarations.Length == 0);

			GlbBranch currentBranch = GlbBranch.CurrentBranch;
			GlbStaff currentStaff = GlbStaff.CurrentUser;

			using (Env.SetTemporaryUserContext(currentStaff.GS_LoginName, AnotherNewBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				bool branchInNZOrAUOrUS = GlbBranch.CurrentBranch.Country.Code == Core.Constants.CountryCodes.Australia || GlbBranch.CurrentBranch.Country.Code == Core.Constants.CountryCodes.NewZealand || GlbBranch.CurrentBranch.Country.Code == Core.Constants.CountryCodes.UnitedStates;
				AssertEquals("Precondition: current environment is in NZ or AU or US", true, branchInNZOrAUOrUS);

				Buffer.Clear();
				adapter.GenerateDeclarationForShipment(shipmentValue, shipment, context);

				AssertEquals("shipment has declaration ", true, shipment.Declarations.Length == 1);
				AssertEquals("declaraton 's branch", GlbBranch.CurrentBranch.PK, shipment.Declarations[0].JE_GB);
			}
		}

		public void TestAutomaticImportForShipment()
		{
			ForwardingShipment shipment = GetShipment();
			Xsd.Shipment shipmentValue = GetShipmentValue();

			ForwardingConsolValueObjectDataAdapterForTest adapter = new ForwardingConsolValueObjectDataAdapterForTest(false);
			Buffer.Clear();

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Buffer);

			SystemDataRegistry.Instance.AutoCreateDeclarationWithinShipment.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("Precondition: shipment has no declaration", true, shipment.Declarations.Length == 0);
			adapter.GenerateDeclarationForShipment(shipmentValue, shipment, context);
			AssertEquals("shipment has no declaration", true, shipment.Declarations.Length == 0);

			SystemDataRegistry.Instance.AutoCreateDeclarationWithinShipment.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			Buffer.Clear();
			SystemDataRegistry.Instance.ConsolsDataImportDirectory.SetValue(AnotherNewBranch.PK.ToGuid(), Guid.Empty, Guid.Empty, ConsolImportPath);
			AssertNotEquals("Precondition: Current branch is not the branch with homeport = 'AUBNE'", GlbBranch.CurrentBranch.GB_Code, AnotherNewBranch.GB_Code);
			adapter.GenerateDeclarationForShipment(shipmentValue, shipment, context);
			AssertEquals("shipment has declaration", true, shipment.Declarations.Length > 0);
			AssertEquals("declaraton 's branch", AnotherNewBranch.PK, shipment.Declarations[0].JE_GB);
		}

		public void TestCreateImportAndExportDeclarationAsPerPort()
		{
			ForwardingShipment shipment = GetShipment();
			shipment.Consols[0].JK_RL_NKDischargePort = "NZAKL";
			shipment.JS_OH_ImportBroker = NZBroker.PK;

			Xsd.Shipment shipmentValue = GetShipmentValue();

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Buffer);
			ForwardingConsolValueObjectDataAdapterForTest adapter = new ForwardingConsolValueObjectDataAdapterForTest(false);

			AssertEquals("Precondition: shipment has no declaration", true, shipment.Declarations.Length == 0);
			adapter.GenerateDeclarationForShipment(shipmentValue, shipment, context);
			AssertEquals("shipment has 2 declarations", true, shipment.Declarations.Length == 2);
			AssertJobDecExist(NewBranch, shipment.Declarations);
			AssertJobDecExist(AnotherNewBranch, shipment.Declarations);

			shipment = GetShipment();
			shipment.Consols[0].JK_RL_NKDischargePort = "USCHI";
			shipment.JS_OH_ImportBroker = USBroker.PK;

			shipmentValue = GetShipmentValue();
			adapter = new ForwardingConsolValueObjectDataAdapterForTest(false);

			AssertEquals("Precondition: shipment has no declaration", true, shipment.Declarations.Length == 0);
			adapter.GenerateDeclarationForShipment(shipmentValue, shipment, context);
			AssertEquals("shipment has declarations", true, shipment.Declarations.Length == 2);
			AssertJobDecExist(USBranch, shipment.Declarations);
			AssertJobDecExist(AnotherNewBranch, shipment.Declarations);
		}

		void AssertJobDecExist(GlbBranch branch, IBaseJobDeclaration[] declarations)
		{
			Assert(
				"declaration of branch " + branch.GB_Code + " exists",
				declarations.Any(declaration => declaration.JE_GB.Equals(branch.PK)));
		}

		public void TestExportConsolRelatedInvoice()
		{
			ForwardingConsol consol = SetupConsolWithInvoices();
			Xsd.Consol xsdConsol = new Xsd.Consol();

			ForwardingConsolValueObjectDataAdapter dataAdapter = new ForwardingConsolValueObjectDataAdapter();
			AssertEquals("the system registry is set to false", false, SystemDataRegistry.Instance.IncludeConsolOrShipmentARInvoices.Value);
			dataAdapter.ExportToValueObject(consol, xsdConsol, new ValueObjectExportContext(Buffer));
			AssertEquals("xsdConsol 's ARInvoices is specified", false, xsdConsol.ARInvoices.IsSpecified);

			Buffer.Clear();

			SystemDataRegistry.Instance.IncludeConsolOrShipmentARInvoices.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("the system registry is set to true", true, SystemDataRegistry.Instance.IncludeConsolOrShipmentARInvoices.Value);
			xsdConsol = new Xsd.Consol();
			dataAdapter.ExportToValueObject(consol, xsdConsol, new ValueObjectExportContext(Buffer));
			AssertEquals("xsdConsol 's ARInvoices is specified", true, xsdConsol.ARInvoices.IsSpecified);
			AssertEquals("no of arinvoices attached", 1, xsdConsol.ARInvoices.Count);

			Buffer.Clear();
			consol = Factory.NewWithValidTestData<ForwardingConsol>();
			dataAdapter.ExportToValueObject(consol, xsdConsol, new ValueObjectExportContext(Buffer));
			AssertEquals("xsdConsol 's ARInvoices is specified", false, xsdConsol.ARInvoices.IsSpecified);
		}

		public void TestExportAWBHeaderOnlyWorkForExportAirConsol()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			ForwardingConsol consolAirExport = Factory.NewWithValidTestData<ForwardingConsol>();
			consolAirExport.JK_TransportMode = Core.Constants.TransportModes.Air;
			consolAirExport.JK_RL_NKLoadPort = "AUSYD";
			consolAirExport.JK_RL_NKDischargePort = "USLAX";
			consolAirExport.PopulateAWB();

			ForwardingConsol consolSeaExport = Factory.NewWithValidTestData<ForwardingConsol>();
			consolSeaExport.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consolSeaExport.JK_RL_NKLoadPort = "AUSYD";
			consolSeaExport.JK_RL_NKDischargePort = "USLAX";
			consolSeaExport.PopulateAWB();

			ForwardingConsol consolAirImport = Factory.NewWithValidTestData<ForwardingConsol>();
			consolAirImport.JK_TransportMode = Core.Constants.TransportModes.Air;
			consolAirImport.JK_RL_NKLoadPort = "NZAKL";
			consolAirImport.JK_RL_NKDischargePort = "AUSYD";
			consolAirImport.PopulateAWB();

			Factory.Save();

			ProcessTaskNotification task = Factory.New<ProcessTaskNotification>();
			task.PQ_P9 = Factory.New<ProcessTask>().PK;
			task.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLWithAWB;
			EventsWithSourceType triggeredByEvents = new EventsWithSourceType(EventsWithSourceType.SourceType.Consol, task, null);

			VerifyExportAWBHeader(consolAirExport, triggeredByEvents, true);
			VerifyExportAWBHeader(consolSeaExport, triggeredByEvents, false);
			VerifyExportAWBHeader(consolAirImport, triggeredByEvents, true);

			task.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXML;
			VerifyExportAWBHeader(consolAirExport, triggeredByEvents, false);
		}

		void VerifyExportAWBHeader(ForwardingConsol consol, EventsWithSourceType triggeredByEvents, bool expectAWBHeadersExported)
		{
			Xsd.Consol xsdConsol = new Xsd.Consol();
			ForwardingConsolValueObjectDataAdapter dataAdapter = new ForwardingConsolValueObjectDataAdapter(triggeredByEvents);
			var context = new ValueObjectExportContext(new NotificationBuffer());
			dataAdapter.ExportToValueObject(consol, xsdConsol, context);
			AssertEquals("xsdConsol 's AWBHeader is specified", expectAWBHeadersExported, xsdConsol.AWBHeaders.IsSpecified);
		}

		public void TestReturnWarningMessageWhenNormalExceptionThrowForDeclarationCreation()
		{
			ForwardingShipment shipment = GetShipment();
			Xsd.Shipment shipmentValue = GetShipmentValue();

			ForwardingConsolValueObjectDataAdapterForTest adapter = new ForwardingConsolValueObjectDataAdapterForTest(false);
			adapter.ThrowException = true;
			Buffer.Clear();

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Buffer);
			AssertEquals("Precondition: shipment has no declaration", true, shipment.Declarations.Length == 0);

			adapter.GenerateDeclarationForShipment(shipmentValue, shipment, context);

			AssertEquals("shipment has no declaration after import", true, shipment.Declarations.Length == 0);
			AssertEquals("notification has errors", false, Buffer.HasErrors);
			AssertEquals("notification has warnings", true, Buffer.HasWarnings);
			ZString expectedwarningmessage = "Cannot Create Declaration for Shipment with House Bill HOUSEBILL - test";
			AssertContains("expected warning message", expectedwarningmessage, Buffer.AsString);
		}

		[ExpectException(typeof(DatabaseUpgradeException))]
		public void TestThrowCriticalExceptionForDeclarationCreation()
		{
			var shipment = GetShipment();
			var shipmentValue = GetShipmentValue();

			var adapter = new ForwardingConsolValueObjectDataAdapterForTest(false)
			{
				ThrowCriticalException = true
			};

			Buffer.Clear();

			var context = new ValueObjectImportContext(Factory, Buffer);
			adapter.GenerateDeclarationForShipment(shipmentValue, shipment, context);
		}

		ForwardingConsol SetupConsolWithInvoices()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>(TestBusinessObjectKind.MinimumRequiredToSave);

			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>(TestBusinessObjectKind.MinimumRequiredToSave);
			Factory.Save();

			JobHeader jobheader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobheader.JH_ParentID = shipment.PK;
			jobheader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			BusinessObject aRInvoice = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Accounting.Integration.IARInvoice)));
			aRInvoice[AccTransactionHeaderSchema.AH_JH] = ZGuid.Empty;
			aRInvoice[AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef] = consol.JK_UniqueConsignRef;

			BusinessObject aRInvoiceLine = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Accounting.Integration.IARInvoiceLine)));
			aRInvoiceLine[AccTransactionLinesSchema.AL_AH] = aRInvoice.PK;
			aRInvoiceLine[AccTransactionLinesSchema.AL_JH] = jobheader.PK;
			aRInvoiceLine[AccTransactionLinesSchema.AL_AG] = Factory.NewWithValidTestData<AccGLHeader>().PK;

			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AL_ARLine = aRInvoiceLine.PK;
			charge.JR_JH = jobheader.PK;

			Factory.Save();

			return consol;
		}

		public void TestExportOnlyRelevantShipmentWhenSpecified()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = GetNewShipment(consol, "SHIPMENT1");
			ForwardingShipment shipment2 = GetNewShipment(consol, "SHIPMENT2");
			ForwardingShipment shipment3 = GetNewShipment(consol, "SHIPMENT3");
			Factory.Save();

			Xsd.Consol xsdConsol = new Xsd.Consol();
			ForwardingConsolValueObjectDataAdapter standardAdapter = new ForwardingConsolValueObjectDataAdapter();
			standardAdapter.ExportToValueObject(consol, xsdConsol, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("ShipmentToExport NOT specified => There should be 3 Shipments", 3, xsdConsol.Shipments.Count);

			xsdConsol = new Xsd.Consol();
			ForwardingConsolValueObjectDataAdapter singleShipmentAdapter = new ForwardingConsolValueObjectDataAdapter(shipment3);
			singleShipmentAdapter.ExportToValueObject(consol, xsdConsol, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("ShipmentToExport specified => There should be only 1 Shipment", 1, xsdConsol.Shipments.Count);
			AssertEquals("Exported Shipment UniqueConsignRef", shipment3.JS_HouseBill, xsdConsol.Shipments[0].Housebill);
		}

		public void TestExportOnlyRelevantOrderAndItsShipmentWhenSpecified()
		{
			// Create test Consol
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			// Add Shipments to the Consol
			ForwardingShipment shipment1 = GetNewShipment(consol, "SHIPMENT1");
			ForwardingShipment shipment2 = GetNewShipment(consol, "SHIPMENT2");
			ForwardingShipment shipment3 = GetNewShipment(consol, "SHIPMENT3");
			// Add some Orders to each Shipment
			Order order11 = AddNewOrderToShipment(shipment1, "ORDER11");
			Order order12 = AddNewOrderToShipment(shipment1, "ORDER12");
			Order order21 = AddNewOrderToShipment(shipment2, "ORDER21");
			Order order22 = AddNewOrderToShipment(shipment2, "ORDER22");
			Order order31 = AddNewOrderToShipment(shipment3, "ORDER31");
			Factory.Save();

			Xsd.Consol xsdConsol = new Xsd.Consol();
			ForwardingConsolValueObjectDataAdapter standardAdapter = new ForwardingConsolValueObjectDataAdapter();
			standardAdapter.ExportToValueObject(consol, xsdConsol, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("ShipmentToExport NOT specified => There should be 3 Shipments", 3, xsdConsol.Shipments.Count);
			int totalOrderCount = xsdConsol.Shipments[0].Orders.Count + xsdConsol.Shipments[1].Orders.Count + xsdConsol.Shipments[2].Orders.Count;
			AssertEquals("ShipmentToExport NOT specified => There should be 5 Orders", 5, totalOrderCount);

			xsdConsol = new Xsd.Consol();
			ForwardingConsolValueObjectDataAdapter singleOrderAdapter = new ForwardingConsolValueObjectDataAdapter(order12);
			singleOrderAdapter.ExportToValueObject(consol, xsdConsol, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("OrderToExport specified => There should be only 1 Shipment", 1, xsdConsol.Shipments.Count);
			AssertEquals("OrderToExport specified => There should be only 1 Order", 1, xsdConsol.Shipments[0].Orders.Count);
			AssertEquals("Exported Shipment UniqueConsignRef", shipment1.JS_HouseBill, xsdConsol.Shipments[0].Housebill);
			AssertEquals("Exported Order ReferenceNumber", order12.JD_OrderNumber, xsdConsol.Shipments[0].Orders[0].OrderIdentifier.OrderNumber);
		}

		ForwardingShipment GetNewShipment(ForwardingConsol consol, string houseBill)
		{
			ForwardingShipment newShipment = consol.Shipments.AddNew();
			newShipment.JS_HouseBill = houseBill;
			newShipment.ConsignorPK = GlbCompany.CurrentCompany.OrgProxy.PK;
			return newShipment;
		}

		Order AddNewOrderToShipment(ForwardingShipment shipment, string newOrderNumber)
		{
			Order newOrder = Factory.New<Order>();
			newOrder.JD_OrderNumber = newOrderNumber;
			newOrder.JD_JS = shipment.PK;
			newOrder.BuyerPK = shipment.ConsignorPK;
			return newOrder;
		}

		ForwardingShipment GetShipment()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_RL_NKDischargePort = "HKHKG";
			consol.JK_RL_NKLoadPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "HOUSEBILL";
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_RL_NKDestination = "HKHKG";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_OH_ExportBroker = Broker.PK;

			return shipment;
		}

		Xsd.Shipment GetShipmentValue(Xsd.Consol consolValue)
		{
			Xsd.ConsolIdentifier consolIdentifier = consolValue.ConsolIdentifier.AddNew();
			consolIdentifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			consolIdentifier.Value = "MASTER";
			consolValue.ConsolDetail.TransportMode = Xsd.ConsolTransportMode.SEA;

			consolValue.ConsolDetail.PortOfLoading.Port = Xsd.UNLOCO.FromPortCode(Factory, GlbBranch.CurrentBranch.GB_RL_NKHomePort);
			consolValue.ConsolDetail.PortOfDischarge.Port = Xsd.UNLOCO.FromPortCode(Factory, "HKHKG");

			Xsd.Container container = consolValue.ConsolDetail.Containers.AddNew();
			container.ContainerNumber = "CONT1";
			container.ContainerType.ContainerCode = "20GP";

			Xsd.Shipment shipmentValue = consolValue.Shipments.AddNew();
			Xsd.ShipmentIdentifier identifier = shipmentValue.ShipmentIdentifier.AddNew();
			identifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			identifier.Value = "HOUSEBILL";

			shipmentValue.ShipmentDetails.TransportMode = Xsd.TransportMode.SEA;
			shipmentValue.ShipmentDetails.PortOfOrigin.Port.Value = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipmentValue.ShipmentDetails.PortofDestination.Port.Value = "HKHKG";
			shipmentValue.ShipmentDetails.TransportMode = Xsd.TransportMode.SEA;

			Xsd.Organisation brokerXSD = new Xsd.Organisation();
			brokerXSD.OrganisationDetails.Name = "Broker";
			brokerXSD.OrganisationDetails.Location.Value = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			brokerXSD.OrganisationDetails.WebAddress = "For match";

			shipmentValue.ShipmentDetails.ExportBroker = brokerXSD;

			Xsd.Package packLine = shipmentValue.ShipmentDetails.Packages.AddNew();
			packLine.PackType = Core.Constants.PkgUnit.Bag;
			packLine.NumberOfPacks = 100;
			packLine.ContainerNumber = "CONT1";

			return shipmentValue;
		}

		Xsd.Shipment GetShipmentValue()
		{
			Xsd.Consol consol = new Xsd.Consol();
			return GetShipmentValue(consol);
		}

		public void TestExportNotifyParty()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			AssertEquals(false, consol.NotifyPartyDocumentaryAddress.IsValidAddress);

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "bbb";
			org.Addresses.AddNew();
			org.Addresses[0].OA_Address1 = "ccc";
			org.Addresses[0].OA_City = "ddd";
			org.Addresses[0].OA_PostCode = "eee";
			org.Addresses[0].OA_State = "fff";
			org.Addresses.AddNew();
			org.Addresses[1].OA_Address1 = "ggg";
			org.Addresses[1].OA_City = "hhh";
			org.Addresses[1].OA_PostCode = "iii";
			org.Addresses[1].OA_State = "jjj";
			org.Addresses.AddNew();
			org.Addresses[2].OA_Address1 = "kkk";
			org.Addresses[2].OA_City = "lll";
			org.Addresses[3].OA_PostCode = "mmm";
			org.Addresses[3].OA_State = "nnn";

			consol.NotifyPartyDocumentaryAddress.OrganisationPK = org.PK;

			Xsd.Consol xsdConsol = new Xsd.Consol();
			ForwardingConsolValueObjectDataAdapter standardAdapter = new ForwardingConsolValueObjectDataAdapter();
			standardAdapter.ExportToValueObject(consol, xsdConsol, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals(true, xsdConsol.ConsolDetail.Addresses.DocAddress.IsSpecified);
			AssertEquals("bbb", xsdConsol.ConsolDetail.Addresses.DocAddress[0].AddressReference.Organisation.OrganisationDetails.Name);
			AssertEquals("ccc", xsdConsol.ConsolDetail.Addresses.DocAddress[0].AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine1);
			AssertEquals("ddd", xsdConsol.ConsolDetail.Addresses.DocAddress[0].AddressReference.Organisation.OrganisationDetails.Addresses[0].CityOrSuburb);
			AssertEquals("eee", xsdConsol.ConsolDetail.Addresses.DocAddress[0].AddressReference.Organisation.OrganisationDetails.Addresses[0].PostCode);
			AssertEquals("fff", xsdConsol.ConsolDetail.Addresses.DocAddress[0].AddressReference.Organisation.OrganisationDetails.Addresses[0].StateOrProvince);
		}

		public void TestImportAddresses()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			AssertEquals(false, consol.NotifyPartyDocumentaryAddress.IsValidAddress);

			Xsd.Consol xsdConsol = NewXsdAirConsol();

			ForwardingConsolValueObjectDataAdapter standardAdapter = new ForwardingConsolValueObjectDataAdapter();
			standardAdapter.ImportFromValueObject(consol, xsdConsol, new ValueObjectImportContext(Factory, new NotificationBuffer()));

			AssertEquals(true, consol.DocAddresses[0].IsValidAddress);
			AssertEquals("NPP", consol.DocAddresses[0].E2_AddressType);
			AssertEquals("bbb", consol.DocAddresses[0].E2_CompanyName);
			AssertEquals("ccc", consol.DocAddresses[0].E2_Address1);
			AssertEquals("ddd", consol.DocAddresses[0].E2_City);
			AssertEquals("eee", consol.DocAddresses[0].E2_Postcode);
			AssertEquals("fff", consol.DocAddresses[0].E2_State);

			AssertEquals(true, consol.DocAddresses[1].IsValidAddress);
			AssertEquals("N2D", consol.DocAddresses[1].E2_AddressType);
			AssertEquals("111", consol.DocAddresses[1].E2_CompanyName);
			AssertEquals("222", consol.DocAddresses[1].E2_Address1);
			AssertEquals("333", consol.DocAddresses[1].E2_City);
			AssertEquals("444", consol.DocAddresses[1].E2_Postcode);
			AssertEquals("555", consol.DocAddresses[1].E2_State);
		}

		Xsd.Consol NewXsdAirConsol()
		{
			Xsd.Consol consol = new Xsd.Consol();
			Xsd.ConsolIdentifier consolIdentifier = consol.ConsolIdentifier.AddNew();
			consolIdentifier.ConsolIdentifierType = Enterprise.DataTransfer.Xml.XsdVersion1.ConsolIdentifierType.MasterWaybill;
			consolIdentifier.Value = "Masterbill";
			consol.ConsolDetail.ConsolType = Xsd.ConsolType.Direct;
			consol.ConsolDetail.TransportMode = Xsd.ConsolTransportMode.AIR;

			var docAddress = consol.ConsolDetail.Addresses.DocAddress.AddNew();
			docAddress.AddressType = Xsd.DocAddressAddressType.NPP;
			docAddress.CompanyName = "bbb";
			docAddress.AddressLine1 = "ccc";
			docAddress.CityOrSuburb = "ddd";
			docAddress.PostCode = "eee";
			docAddress.StateOrProvince = "fff";

			docAddress = consol.ConsolDetail.Addresses.DocAddress.AddNew();
			docAddress.AddressType = Xsd.DocAddressAddressType.N2D;
			docAddress.CompanyName = "111";
			docAddress.AddressLine1 = "222";
			docAddress.CityOrSuburb = "333";
			docAddress.PostCode = "444";
			docAddress.StateOrProvince = "555";

			return consol;
		}

		NotificationBuffer Buffer
		{
			get { return buffer ?? (buffer = new NotificationBuffer()); }
		}
		NotificationBuffer buffer;

		void SetupOrgProxyAndBrokers()
		{
			DummyCompany = Factory.New<GlbCompany>();
			DummyCompany.GC_Code = "DDD";
			DummyCompany.GC_Name = "Dummy Co";
			Factory.Save();

			Broker = Factory.New<OrgHeader>();
			Broker.OH_Code = "BROKER";
			Broker.OH_FullName = "Broker";
			Broker.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Broker.OH_IsBroker = true;
			Broker.MainWebURL.PU_URL = "For match";

			NZBroker = Factory.New<OrgHeader>();
			NZBroker.OH_Code = "NZBroker";
			NZBroker.OH_FullName = "NZBroker";
			NZBroker.OH_RL_NKClosestPort = "NZAKL";
			NZBroker.OH_IsBroker = true;
			NZBroker.MainWebURL.PU_URL = "For matching";

			USBroker = Factory.New<OrgHeader>();
			USBroker.OH_Code = "USBroker";
			USBroker.OH_FullName = "USBroker";
			USBroker.OH_RL_NKClosestPort = "USCHI";
			USBroker.OH_IsBroker = true;
			USBroker.MainWebURL.PU_URL = "For matching";

			AnotherNewBranch = Factory.New<GlbBranch>();
			AnotherNewBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			AnotherNewBranch.GB_RL_NKHomePort = "AUBNE";
			AnotherNewBranch.GB_OH_OrgProxy = Broker.PK;
			AnotherNewBranch.GB_Code = "DEF";

			GlbBranchExtraPorts pER = AnotherNewBranch.ExtraPorts.AddNew();
			pER.GY_RL_NKAdditionalBranchRelatedPort = "AUPER";

			NewBranch = Factory.New<GlbBranch>();
			NewBranch.GB_GC = DummyCompany.PK;
			NewBranch.GB_RL_NKHomePort = "NZAKL";
			NewBranch.GB_OH_OrgProxy = NZBroker.PK;
			NewBranch.GB_Code = "ABC";

			USBranch = Factory.New<GlbBranch>();
			USBranch.GB_GC = DummyCompany.PK;
			USBranch.GB_RL_NKHomePort = "USCHI";
			USBranch.GB_OH_OrgProxy = USBroker.PK;
			USBranch.GB_Code = "KJH";

			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUBNE";
			SetupOrgProxyAndBrokers();
			ConsolImportPath = Env.TempPath;
			SystemDataRegistry.Instance.ConsolsDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ConsolImportPath);
		}

		ZString ConsolImportPath;
		OrgHeader Broker, NZBroker, USBroker;
		GlbBranch NewBranch, AnotherNewBranch, USBranch;
		GlbCompany DummyCompany;

		protected override ValueObjectDataAdapter<ForwardingConsol, Xsd.Consol> GetNewBizObjXmlDataAdapter()
		{
			return new ForwardingConsolValueObjectDataAdapter();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			ForwardingConsol emptyConsol = NewBusinessObject();
			emptyConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			emptyConsol.JK_UniqueConsignRef = "";
			emptyConsol.JK_AgentsReference = "";
			var emptyConsolXmlPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.Forwarding.DataTransfer.Test.TestFiles.EmptyConsol.xml");
			return new BusinessObjectAndExpectedOutputFileName(emptyConsol, emptyConsolXmlPath, ValidationKind.None, "Empty consol");
		}

		protected override string FullyPopulatedAirSamplePath => resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.Forwarding.DataTransfer.Test.TestFiles.AirConsol.xml");

		protected override string FullyPopulatedSeaSamplePath => resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.Forwarding.DataTransfer.Test.TestFiles.SeaConsol.xml");

		protected override string FullyPopulatedConsolWithEmptyFieldsPath => resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.Forwarding.DataTransfer.Test.TestFiles.PopulatedConsolWithEmptyFields.xml");

		protected override string FullyPopulatedRailSamplePath => resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.Forwarding.DataTransfer.Test.TestFiles.RailConsol.xml");

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		class ForwardingConsolValueObjectDataAdapterForTest : ForwardingConsolValueObjectDataAdapter
		{
			public ForwardingConsolValueObjectDataAdapterForTest()
			{
			}

			public ForwardingConsolValueObjectDataAdapterForTest(ForwardingShipment shipment, EventsWithSourceType triggeredByEvents)
				: base(shipment, triggeredByEvents)
			{ }

			public bool ThrowException;

			public bool ThrowCriticalException;

			public ForwardingConsolValueObjectDataAdapterForTest(bool isManualImport)
				: base(isManualImport)
			{
			}

			public new void ExportContainers(ForwardingConsol consol, Xsd.Consol consolValue, IValueObjectExportContext context)
			{
				base.ExportContainers(consol, consolValue, context);
			}

			public new void GenerateDeclarationForShipment(Xsd.Shipment shipmentValue, CommonShipment shipment, IValueObjectImportContext context)
			{
				base.GenerateDeclarationForShipment(shipmentValue, shipment, context);
			}

			protected override void CreateDeclarationFromDeclarationGenerator(ForwardingShipment shipment, Xsd.InvoiceHeaderCollection invoices, IValueObjectImportContext context)
			{
				if (ThrowException)
				{
					throw new Exception("test");
				}

				if (ThrowCriticalException)
				{
					throw new DatabaseUpgradedException();
				}

				base.CreateDeclarationFromDeclarationGenerator(shipment, invoices, context);
			}
		}
	}
}
