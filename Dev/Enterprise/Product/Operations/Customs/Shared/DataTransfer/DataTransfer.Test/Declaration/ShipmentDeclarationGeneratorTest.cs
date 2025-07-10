using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.DataTransfer.Testing
{
	public class ShipmentDeclarationGeneratorTest : TestCaseWithFactory
	{
		public void TestNewForPuertoRico()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.PuertoRico);
			Assert("Same as us", ShipmentDeclarationGenerator.New() is Integration.Customs.US.IUSShipmentDeclarationGenerator);
		}

		public virtual void TestInvoiceGenerator()
		{
			AssertEquals("Invoice generator type", ExpecteInvoiceGeneratorType, (new ShipmentDeclarationGeneratorForTest()).GetNewInvoiceGenerator(Factory.New<BaseJobDeclaration>()).GetType());
		}

		public void TestCreateDeclarationForShipment()
		{
			Xsd.Shipment shipmentValue = GetShipmnetXSD();
			ForwardingShipment shipment = GetShipment();

			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);
			DeclarationGenerator.CreateDeclarationForShipment(shipment, shipmentValue.Invoices, context);

			AssertEquals("shipment has declaration", true, shipment.Declarations.Length == 1);
			AssertEquals("type of declaration", TypeOfDeclaration, shipment.Declarations[0].GetType());
			BaseJobDeclaration jobDec = (BaseJobDeclaration)shipment.Declarations[0];
			AssertEquals("declaration has commercial invoices", 1, jobDec.Invoices.Count);
			AssertEquals("invoice 's number", "ABC", jobDec.Invoices[0].JZ_InvoiceNumber);
			AssertEquals("declaraton 's branch", GlbBranch.CurrentBranch.PK, jobDec.JE_GB);
		}

		protected virtual Type TypeOfDeclaration => ObjectFactory.GetType<Integration.Customs.AU.IJobDeclaration>();

		protected virtual Type ExpecteInvoiceGeneratorType => typeof(InvoicesGeneratorFromXSD);

		IShipmentDeclarationGenerator declarationGenerator;
		protected virtual IShipmentDeclarationGenerator DeclarationGenerator => declarationGenerator ?? (declarationGenerator = ShipmentDeclarationGeneratorForTest.New());

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("AU");
		}

		protected ForwardingShipment GetShipment()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consol.JK_RL_NKLoadPort = "HKHKG";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "HOUSEBILL";
			shipment.JS_RL_NKOrigin = "HKHKG";
			shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			return shipment;
		}

		protected Xsd.Shipment GetShipmnetXSD()
		{
			var consolValue = new Xsd.Consol();
			var consolIdentifier = consolValue.ConsolIdentifier.AddNew();
			consolIdentifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			consolIdentifier.Value = "MASTER";
			consolValue.ConsolDetail.TransportMode = Xsd.ConsolTransportMode.SEA;

			consolValue.ConsolDetail.PortOfLoading.Port = Xsd.UNLOCO.FromPortCode(Factory, "HKHKG");
			consolValue.ConsolDetail.PortOfDischarge.Port = Xsd.UNLOCO.FromPortCode(Factory, GlbBranch.CurrentBranch.GB_RL_NKHomePort);

			var shipmentValue = consolValue.Shipments.AddNew();
			var identifier = shipmentValue.ShipmentIdentifier.AddNew();
			identifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			identifier.Value = "HOUSEBILL";

			shipmentValue.ShipmentDetails.TransportMode = Xsd.TransportMode.SEA;
			shipmentValue.ShipmentDetails.PortOfOrigin.Port.Value = "HKHKG";
			shipmentValue.ShipmentDetails.PortofDestination.Port.Value = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			var invoice = shipmentValue.Invoices.AddNew();
			invoice.InvoiceAmount.Value = 100m;
			invoice.InvoiceAmount.CurrencyCode = "AUD";
			invoice.InvoiceNumber = "ABC";

			return shipmentValue;
		}

		sealed class ShipmentDeclarationGeneratorForTest : ShipmentDeclarationGenerator
		{
			public new static ShipmentDeclarationGeneratorForTest New() => new ShipmentDeclarationGeneratorForTest();

			public new InvoicesGeneratorFromXSD GetNewInvoiceGenerator(BaseJobDeclaration declaration) => base.GetNewInvoiceGenerator(declaration);
		}
	}
}
