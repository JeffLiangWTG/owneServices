using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.eHubMessaging.Tests;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Testing
{
	class WhsDocketMessageActionTest : ImportMessageActionTest
	{
		#region TestExecuteAction_ImportWhsDocket

		public void TestExecuteAction_ImportWhsDocket()
		{
			ExecuteAction_ImportWhsDocket();
		}

		#endregion

		#region TestExecuteAction_ImportWhsDocket_NoExceptionThrownWhenReImport

		public void TestExecuteAction_ImportWhsDocket_NoExceptionThrownWhenReImport()
		{
			ExecuteAction_ImportWhsDocket();

			var factoryProvider = new BusinessObjectFactoryProvider(Factory);
			var message = Factory.New<Messaging.Testing.TestEdiMessage>();
			message.EM_MessageText = messageBody;
			var buffer = new NotificationBuffer();
			var action = new WhsDocketMessageAction(factoryProvider);
			List<ITransactionParticipant> forSave;
			action.ExecuteAction(message, buffer, out forSave);

			AssertNoExceptionThrown(() =>
			{
				Factory.Save();
			});
		}

		#endregion

		#region AssertExecuteAction_ImportWhsDocket

		void ExecuteAction_ImportWhsDocket()
		{
			var factoryProvider = new BusinessObjectFactoryProvider(Factory);
			int noOfWhsDockets = Factory.GetDatabaseCount(typeof(WhsDocket));

			var buffer = new NotificationBuffer();
			var message = Factory.New<Messaging.Testing.TestEdiMessage>();
			message.EM_MessageText = messageBody;
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_HeaderText = interchangeHeader;
			interchange.EI_FooterText = interchangeFooter;
			message.EM_EI = interchange.PK;
			interchange.EI_To = "test";
			interchange.EI_From = "test";

			var address = Factory.NewWithValidTestData<OrgAddress>();
			var helper = new WhsTestHelperFunctions(Factory);
			helper.CreateWarehouse("DEM", address, GlbBranch.CurrentBranch);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "ABIMOTBNE1";
			org1.OH_FullName = "ABIMOTBNE1";
			org1.OH_IsWarehouseClient = true;

			var part1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			part1.OP_PartNum = "PRODUCT1";

			var orgPartRel1 = Factory.NewWithValidTestData<OrgPartRelation>();
			orgPartRel1.OU_OH = org1.PK;
			orgPartRel1.OU_OP = part1.PK;

			Factory.Save();

			var action = new WhsDocketMessageAction(factoryProvider);
			List<ITransactionParticipant> forSave;
			Assert(action.ExecuteAction(message, buffer, out forSave));
			Factory.Save();
			AssertEquals(noOfWhsDockets + 1, Factory.GetDatabaseCount(typeof(WhsDocket)));
		}

		#endregion

		#region TestExecuteAction_ImportWhsDocket_Error

		public void TestExecuteAction_ImportWhsDocket_Error()
		{
			var factoryProvider = new BusinessObjectFactoryProvider(Factory);
			var buffer = new NotificationBuffer();
			var message = Factory.New<Messaging.Testing.TestEdiMessage>();
			message.EM_MessageText = messageBody;
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_HeaderText = interchangeHeader;
			interchange.EI_FooterText = interchangeFooter;
			message.EM_EI = interchange.PK;
			interchange.EI_To = "test";
			interchange.EI_From = "test";

			var address = Factory.NewWithValidTestData<OrgAddress>();
			var helper = new WhsTestHelperFunctions(Factory);
			helper.CreateWarehouse("DEM", address, GlbBranch.CurrentBranch);

			var action = new WhsDocketMessageAction(factoryProvider);
			List<ITransactionParticipant> forSave;
			Assert(action.ExecuteAction(message, buffer, out forSave));
			Assert("Import has error.", buffer.HasErrors);
			Factory.Save();

			var importedOrder = Factory.Load<WhsDocket>(new ZQuery()).Single();
			AssertEquals(DocketStatus.Codes.Error, importedOrder.WD_DocketStatus);
		}

		public void TestExecuteAction_ImportWhsDocket_Error_ReImport()
		{
			TestExecuteAction_ImportWhsDocket_Error();

			var factory = new BusinessObjectFactory();
			var factoryProvider = new BusinessObjectFactoryProvider(factory);
			var message = factory.New<Messaging.Testing.TestEdiMessage>();
			message.EM_MessageText = messageBody;
			var buffer = new NotificationBuffer();
			var action = new WhsDocketMessageAction(factoryProvider);
			List<ITransactionParticipant> forSave;
			Assert(action.ExecuteAction(message, buffer, out forSave));
			Assert("Import has error.", buffer.HasErrors);
			factory.Save();

			var importedOrder = factory.Load<WhsDocket>(new ZQuery()).Single();
			AssertEquals(DocketStatus.Codes.Error, importedOrder.WD_DocketStatus);
		}

		#endregion

		#region XML

		const string interchangeHeader = @"<?xml version=""1.0"" encoding=""utf-8""?><XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Version=""1"" xmlns=""http://www.edi.com.au/EnterpriseService/""><InterchangeInfo><Date>2011-02-15T09:26:06.847+11:00</Date><XmlType>LightWeight</XmlType><Source><EnterpriseCode>EDI</EnterpriseCode><CompanyCode>EDI</CompanyCode><OriginServer>DAT</OriginServer><LoginName>EDISupport</LoginName></Source><Target /><EDIOrganisation EDICode=""EDICUS"" OwnerCode=""EDICUS""><OrganisationDetails><Name>EDI CUSTOMS BROKERS</Name><Location Country=""Australia"" City=""Brisbane"">AUBNE</Location><Addresses><Address AddressType=""MAIN""><AddressLine1>10 HUTCHESON STREET</AddressLine1><AddressLine2>ALBION  QLD</AddressLine2><AddressCode>PST: 10 HUTCHESON STREET</AddressCode><PostCode>4010</PostCode><Language>EN</Language><Location>AUBNE</Location><Sequence>1</Sequence><AddressCapabilities><AddressCapability AddressType=""MAIN"" /><AddressCapability IsMainAddress=""true"" AddressType=""OFC"" /></AddressCapabilities></Address></Addresses></OrganisationDetails></EDIOrganisation></InterchangeInfo><Payload><WhsDockets>";
		const string interchangeFooter = @"</WhsDockets></Payload></XmlInterchange>";
		const string messageBody = @"<WhsDocket><Identifier><Client EDICode=""ABIMOTBNE1""><OrganisationDetails><Name>ABIMOTBNE1</Name><Location Country=""Australia"" City=""Sydney"">AUSYD</Location><Addresses><Address AddressType=""MAIN""><AddressLine1>NO ADDRESS SPECIFIED</AddressLine1><AddressLine2>SYSTEM DEFINED ORGANISATION</AddressLine2><AddressCode>OFC: NO ADDRESS SPECIFIED</AddressCode><CityOrSuburb>NA</CityOrSuburb><StateOrProvince>NSW</StateOrProvince><Language>EN</Language><Location>AUSYD</Location><Sequence>1</Sequence><AddressCapabilities><AddressCapability AddressType=""MAIN"" /><AddressCapability IsMainAddress=""true"" AddressType=""OFC"" /></AddressCapabilities></Address></Addresses></OrganisationDetails></Client><Reference>W00000001</Reference><DocketID>W00000001</DocketID><DocketType>WOH</DocketType></Identifier><DocketDetail><WarehouseCode>DEM</WarehouseCode><Units>0.000</Units><Packages>0</Packages><Pallets>0</Pallets><Weight DimensionType=""KG"">0.000</Weight><Cubic DimensionType=""M3"">0.000</Cubic><TransportInsurance>0.0000</TransportInsurance><ShipperCODAmount>0.0000</ShipperCODAmount><CustomerOrderDetail><OrderType>ORD</OrderType><DateRequired>2011-02-15T00:00:00+11:00</DateRequired><Consignee AddressType=""CEA""><AddressReference><AddressSequenceRef>1</AddressSequenceRef><Organisation EDICode=""ABIMOTBNE1"" OwnerCode=""ABIMOTBNE1""><OrganisationDetails><Name>ABINGDON MOTORS</Name><Location Country=""Australia"" City=""Brisbane"">AUBNE</Location><Addresses><Address AddressType=""MAIN""><AddressLine1>192 ANNERLY ROAD</AddressLine1><AddressLine2>DUTTON</AddressLine2><AddressCode>Pickup and Delivery Addre</AddressCode><CityOrSuburb>PARK</CityOrSuburb><StateOrProvince>QLD</StateOrProvince><Location>AUBNE</Location><Sequence>1</Sequence><AddressCapabilities><AddressCapability AddressType=""MAIN"" /><AddressCapability IsMainAddress=""true"" AddressType=""OFC"" /></AddressCapabilities></Address></Addresses></OrganisationDetails></Organisation></AddressReference></Consignee></CustomerOrderDetail><Status>ENT</Status><CustomAttributes /></DocketDetail><DocketLines><DocketLine><Product>PRODUCT1</Product><Description>PRODUCT1</Description><QuantityFromClientOrder>0.00</QuantityFromClientOrder><QuantityActuallyOrdered>0.00</QuantityActuallyOrdered><ProductUQ>UNT</ProductUQ><LineAttributes /><CustomerOrderLineDetail><Pricing><RecommendedUnitPrice>0.0000</RecommendedUnitPrice><UnitDiscount>0.00</UnitDiscount><UnitDiscountAmount>0.0000</UnitDiscountAmount><UnitPriceAfterDiscount>0.0000</UnitPriceAfterDiscount><ExtendedPrice>0.0000</ExtendedPrice></Pricing><ProjectedShortfallQuantity>0</ProjectedShortfallQuantity></CustomerOrderLineDetail><LineNumber>1</LineNumber><SubLineNumber>0</SubLineNumber><CustomsData><EntryLineNumber>0</EntryLineNumber><CustomsQuantity>0</CustomsQuantity><BondedWhsQuantity>0</BondedWhsQuantity><ValueForDuty>0</ValueForDuty><TILVAmount>0</TILVAmount></CustomsData><Confirmation><Quantity>0</Quantity></Confirmation></DocketLine></DocketLines></WhsDocket>";

		#endregion
	}
}
