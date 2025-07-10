using System;
using System.IO;
using System.Xml;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer;
using Enterprise.Customs.DataTransfer.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.DataTransfer.Testing
{
	sealed class USShipmentDeclarationGeneratorTest : ShipmentDeclarationGeneratorTest
	{
		public override void TestInvoiceGenerator()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("Invoice generator type", ExpecteInvoiceGeneratorType, (new USShipmentDeclarationGeneratorForTest()).GetNewInvoiceGenerator(declaration).GetType());
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportShipmentWithUSDeclaration()
		{
			var shipment = GetShipment();
			var buffer = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, buffer);
			var inputXmlContent = File.ReadAllText(Path.Combine(BaseSourcePath + @"Enterprise\Product\Operations\Customs\US\DataTransfer\DataTransfer.Test\Testing\USConsolWithShipmentAndDeclaration.xml"));
			var reader = new XmlTextReader(new StringReader(inputXmlContent));
			var consolSerialiser = new XmlValueObjectSerializer(typeof(Xsd.Consols));
			var consols = ((Xsd.Consols)Xsd.XmlInterchange.DeserializeInterchangeAndPayload(reader, consolSerialiser, out var _)).Consol;
			DeclarationGenerator.CreateDeclarationForShipment(shipment, consols[0].Shipments[0].Invoices, context);
			AssertEquals("shipment has declaration", true, shipment.Declarations.Length == 1);
			AssertEquals("type of declaration", TypeOfDeclaration, shipment.Declarations[0].GetType());
			var declaration = (JobDeclaration)shipment.Declarations[0];
			AssertEquals("declaration has commercial invoices", 2, declaration.Invoices.Count);
			AssertEquals("1-st invoice number", "19", declaration.Invoices[0].JZ_InvoiceNumber);
			AssertEquals("1-st invoice number", "24", declaration.Invoices[1].JZ_InvoiceNumber);
		}

		protected override Type ExpecteInvoiceGeneratorType => typeof(USInvoicesGeneratorFromXSD);

		IShipmentDeclarationGenerator declarationGenerator;
		protected override IShipmentDeclarationGenerator DeclarationGenerator => declarationGenerator ?? (declarationGenerator = new USShipmentDeclarationGenerator());

		protected override Type TypeOfDeclaration => typeof(JobDeclaration);

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
		}

		sealed class USShipmentDeclarationGeneratorForTest : USShipmentDeclarationGenerator
		{
			internal new InvoicesGeneratorFromXSD GetNewInvoiceGenerator(BaseJobDeclaration declaration) => base.GetNewInvoiceGenerator(declaration);
		}
	}
}
