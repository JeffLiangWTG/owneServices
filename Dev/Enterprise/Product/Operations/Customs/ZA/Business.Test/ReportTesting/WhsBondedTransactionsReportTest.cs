using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ReportTesting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business.Report.Testing
{
	sealed class WhsBondedTransactionsReportTest : ReportFunctionalTestCase
	{
		protected override ZString ObjectName => "WhsBondedTransactionsReport";

		protected override List<ReportSchemaColumn> ExpectedColumnsInAnyOrder => new List<ReportSchemaColumn>
		{
			new ReportSchemaColumn(typeof(string), "InwardsEntryNo"),
			new ReportSchemaColumn(typeof(short), "InwardsEntryLineNo"),
			new ReportSchemaColumn(typeof(string), "OutwardsEntryNo"),
			new ReportSchemaColumn(typeof(short), "OutwardsEntryLineNo"),
			new ReportSchemaColumn(typeof(string), "DeclarationReference"),
			new ReportSchemaColumn(typeof(string), "ProdCode"),
			new ReportSchemaColumn(typeof(string), "ProdDesc"),
			new ReportSchemaColumn(typeof(string), "Type"),
			new ReportSchemaColumn(typeof(DateTime), "EntryDate"),
			new ReportSchemaColumn(typeof(decimal), "ValueForDuty"),
			new ReportSchemaColumn(typeof(string), "Origin"),
			new ReportSchemaColumn(typeof(decimal), "CustomsQty"),
			new ReportSchemaColumn(typeof(string), "CustomsUQ"),
			new ReportSchemaColumn(typeof(decimal), "InvoiceQty"),
			new ReportSchemaColumn(typeof(string), "InvoiceUQ"),
			new ReportSchemaColumn(typeof(string), "PartAttrib1"),
			new ReportSchemaColumn(typeof(string), "PartAttrib2"),
			new ReportSchemaColumn(typeof(string), "PartAttrib3"),
			new ReportSchemaColumn(typeof(string), "SerialNumber"),
			new ReportSchemaColumn(typeof(string), "PartAttrib1Value"),
			new ReportSchemaColumn(typeof(string), "PartAttrib2Value"),
			new ReportSchemaColumn(typeof(string), "PartAttrib3Value"),
			new ReportSchemaColumn(typeof(string), "SerialNumberValue"),
			new ReportSchemaColumn(typeof(string), "ReasonCode"),
			new ReportSchemaColumn(typeof(Guid), "CommodityPK"),
			new ReportSchemaColumn(typeof(Guid), "WarehouseAddressPK"),
			new ReportSchemaColumn(typeof(Guid), "WarehousePK"),
			new ReportSchemaColumn(typeof(string), "WarehouseName"),
			new ReportSchemaColumn(typeof(Guid), "ClientPK"),
			new ReportSchemaColumn(typeof(string), "ClientCode"),
			new ReportSchemaColumn(typeof(Guid), "ProductPK"),
			new ReportSchemaColumn(typeof(decimal), "RelativeInvoiceQty"),
			new ReportSchemaColumn(typeof(decimal), "RelativeCustomsQty"),
			new ReportSchemaColumn(typeof(decimal), "RelativeValueForDuty"),
			new ReportSchemaColumn(typeof(decimal), "RelativeSecondCustomsQty"),
			new ReportSchemaColumn(typeof(string), "SecondUQ"),
			new ReportSchemaColumn(typeof(decimal), "RelativeThirdCustomsQty"),
			new ReportSchemaColumn(typeof(string), "ThirdUQ"),
			new ReportSchemaColumn(typeof(string), "Tariff"),
			new ReportSchemaColumn(typeof(string), "PrimaryPreference"),
			new ReportSchemaColumn(typeof(DateTime), "CustomsDeadline"),
			new ReportSchemaColumn(typeof(string), "CustomsInwardStyle"),
			new ReportSchemaColumn(typeof(string), "CustomsInwardProcedure"),
			new ReportSchemaColumn(typeof(decimal), "StockOnHand"),
			new ReportSchemaColumn(typeof(string), "ClientName"),
			new ReportSchemaColumn(typeof(string), "WarehouseCode"),
			new ReportSchemaColumn(typeof(decimal), "GTRelativeValueForDuty"),
			new ReportSchemaColumn(typeof(decimal), "GTRelativeInvoiceQty"),
			new ReportSchemaColumn(typeof(decimal), "GTRelativeCustomsQty"),
			new ReportSchemaColumn(typeof(decimal), "GTRelativeSecondCustomsQty"),
			new ReportSchemaColumn(typeof(decimal), "GTRelativeThirdCustomsQty"),
		};

		protected override List<string> ParametersValuesList => new List<string> { "null", "null", "null", "null", "null", "null", "null", "null", "null", "null", "null", "null", "null", "null", "null" };

		protected override SqlObjectType SqlObjectType => SqlObjectType.FunctionTable;

		protected override void AssertTestResults(DataTable results)
		{
			AssertEquals(2, results.Rows.Count);
			var row1 = FormatRowsValues(results.Rows[0], results, ignoreGuid: true);
			var row2 = FormatRowsValues(results.Rows[1], results, ignoreGuid: true);
			CombineAssertions(() =>
			{
				AssertContains("Row1", "[ValueForDuty]='1000.004900'; [Origin]='CN'; [CustomsQty]='1000.000000'; [CustomsUQ]='LI'; [InvoiceQty]='1000.000'; [InvoiceUQ]='UNT'; [PartAttrib1]=''; [PartAttrib2]=''; [PartAttrib3]=''; [SerialNumber]=''; [PartAttrib1Value]=''; [PartAttrib2Value]=''; [PartAttrib3Value]=''; [SerialNumberValue]=''; [ReasonCode]=''; [WarehouseName]='WZA NAME'; [ClientCode]='IMP'; [RelativeInvoiceQty]='1000.000'; [RelativeCustomsQty]='1000.000000'; [RelativeValueForDuty]='1000.004900'; [RelativeSecondCustomsQty]='2000.000000'; [SecondUQ]='KG'; [RelativeThirdCustomsQty]='3000.000000'; [ThirdUQ]='NO'; [Tariff]='1111111'; [PrimaryPreference]='200'; [CustomsDeadline]=''; [CustomsInwardStyle]=''; [CustomsInwardProcedure]=''; [StockOnHand]='1000.000'; [ClientName]='TestImp'; [WarehouseCode]='WZA'; [GTRelativeValueForDuty]='1400.006900'; [GTRelativeInvoiceQty]='1400.000'; [GTRelativeCustomsQty]='1400.000000'; [GTRelativeSecondCustomsQty]='2800.000000'; [GTRelativeThirdCustomsQty]='4200.000000'", row1);
				AssertContains("Row2", "[ValueForDuty]='400.002000'; [Origin]='CN'; [CustomsQty]='400.000000'; [CustomsUQ]='LI'; [InvoiceQty]='400.000'; [InvoiceUQ]='UNT'; [PartAttrib1]=''; [PartAttrib2]=''; [PartAttrib3]=''; [SerialNumber]=''; [PartAttrib1Value]=''; [PartAttrib2Value]=''; [PartAttrib3Value]=''; [SerialNumberValue]=''; [ReasonCode]='AMD'; [WarehouseName]='WZA NAME'; [ClientCode]='IMP'; [RelativeInvoiceQty]='400.000'; [RelativeCustomsQty]='400.000000'; [RelativeValueForDuty]='400.002000'; [RelativeSecondCustomsQty]='800.000000'; [SecondUQ]='KG'; [RelativeThirdCustomsQty]='1200.000000'; [ThirdUQ]='NO'; [Tariff]=''; [PrimaryPreference]=''; [CustomsDeadline]=''; [CustomsInwardStyle]=''; [CustomsInwardProcedure]=''; [StockOnHand]='400.000'; [ClientName]='TestImp'; [WarehouseCode]='WZA'; [GTRelativeValueForDuty]='1400.006900'; [GTRelativeInvoiceQty]='1400.000'; [GTRelativeCustomsQty]='1400.000000'; [GTRelativeSecondCustomsQty]='2800.000000'; [GTRelativeThirdCustomsQty]='4200.000000'", row2);
			});
		}

		protected override void PrepareTestData()
		{
			var helper = new ZAWhsDataTestHelper(Factory);
			var dutyHelper = new ZAWhsInventoryDutyAndTaxCalculatorTestHelper();
			var warehouse = helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, isVirtualWarehouse: true, "WZA");
			var cusCode = Factory.New<OrgCusCode>();
			cusCode.OK_CustomsRegNo = "XXBOS 05901";
			cusCode.OK_OH = helper.Importer.PK;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.WarehouseControlledPremisesID;
			helper.Importer.CustomsCodes.Add(cusCode);
			Factory.Save();

			var receive = helper.GetNewWhsReceive(warehouse.PK, helper.Importer.PK, "WA0000182");
			var whsReceiveLine1 = helper.GetNewWhsReceiveLine(
				receive.PK,
				helper.Part.PK,
				"PACKAGE1",
				1m,
				1000m,
				1000m,
				bondedEntryKey: "EN00123-1");
			whsReceiveLine1.WE_LineNo = 2;
			whsReceiveLine1.WE_SubLineNo = 2;
			var whsBondedWarehouseAttribute1 = helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine1.PK, 1000.0049m, 1000m, "LI", Core.Constants.CountryCodes.China, 600m, "NO", "", "EN00123", 1);
			whsBondedWarehouseAttribute1.WB_PrimaryPreference = "200";
			whsBondedWarehouseAttribute1.WB_Tariff = dutyHelper.TariffCode;
			whsBondedWarehouseAttribute1.WB_CustomsSecondQuantity = 2000m;
			whsBondedWarehouseAttribute1.WB_CustomsSecondUnitQty = "KG";
			whsBondedWarehouseAttribute1.WB_CustomsThirdQuantity = 3000m;
			whsBondedWarehouseAttribute1.WB_CustomsThirdUnitQty = "NO";
			helper.WhsHelper.WhsReceiveAllocateLocationsMock(receive.PK);
			receive.FinaliseDocketWithoutUserConfirmation();

			helper.WhsHelper.CreateRowAndGenerateLocations(warehouse, "LOC");

			var messageText = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Shipment>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>WarehouseAdjustment</Type>
					<Key></Key>
				</DataTarget>
			</DataTargetCollection>
			<Company>
				<Code>{GlbCompany.CurrentCompany.GC_Code}</Code>
			</Company>
			<EnterpriseID>SAF</EnterpriseID>
			<ServerID>TST</ServerID>
		</DataContext>
		<Order>
			<OrderNumber>ORDERME</OrderNumber>
			<Status>
				<Code>ENT</Code>
			</Status>
			<Type>
				<Code>CUS</Code>
			</Type>
			<Warehouse>
				<Code>WZA</Code>
			</Warehouse>
			<OrderLineCollection Content=""Complete"">
				<OrderLine>
					<AdjustmentReason>
						<Code>AMD</Code>
						<Description>Customs Amendmen</Description>
					</AdjustmentReason>
					<ArrivalDate>{ZDate.Today.AddDays(-2).ToString("s", CultureInfo.InvariantCulture)}</ArrivalDate>
					<CustomsData>
						<CountryOfOrigin>
							<Code>CN</Code>
						</CountryOfOrigin>
						<CustomsQuantity>400.00</CustomsQuantity>
						<CustomsQuantityUnit>
							<Code>LI</Code>
						</CustomsQuantityUnit>
						<CustomsSecondQuantity>800</CustomsSecondQuantity>
						<CustomsSecondUnitQty>
							<Code>KG</Code>
							<Description>KG</Description>
						</CustomsSecondUnitQty>
						<CustomsThirdQuantity>1200</CustomsThirdQuantity>
						<CustomsThirdUnitQty>
							<Code>NO</Code>
							<Description>NUMBER</Description>
						</CustomsThirdUnitQty>
						<DeclarationReference>AD1</DeclarationReference>
						<EntryDate>{ZDate.Today.AddDays(-2).ToString("s", CultureInfo.InvariantCulture)}</EntryDate>
						<EntryKey>EN00123-1</EntryKey>
						<EntryLineNumber>1</EntryLineNumber>
						<ValueForDuty>400.00196</ValueForDuty>
					</CustomsData>
					<InventoryStatus>
						<Code>AVL</Code>
						<Description>Available</Description>
					</InventoryStatus>
					<LineComment>Balances From Compuclearing</LineComment>
					<LineNumber>000000001</LineNumber>
					<Location>
						<Column>1</Column>
						<Level>1</Level>
						<Row>RR1</Row>
						<Tray>1</Tray>
					</Location>
					<OrderedQty>400.00</OrderedQty>
					<OrderedQtyUnit>
						<Code>NO</Code>
						<Description>NUMBER</Description>
					</OrderedQtyUnit>
					<PackageQty>400.00</PackageQty>
					<PackageQtyUnit>
						<Code>UNT</Code>
					</PackageQtyUnit>
					<Product>
						<Code>~~1</Code>
					</Product>
					<SubLineNumber>0</SubLineNumber>
				</OrderLine>
			</OrderLineCollection>
		</Order>
		<OrganizationAddressCollection>
			<OrganizationAddress>
				<AddressType>ConsignorDocumentaryAddress</AddressType>
				<OrganizationCode>IMP</OrganizationCode>
			</OrganizationAddress>
		</OrganizationAddressCollection>
	</Shipment>
</UniversalShipment>";

			var uoFactory = new UniversalObjectFactory(Factory);
			var message = TestCaseWithFactoryAndMessagingHelpers.GetQueuedUniversalShipmentMessage(uoFactory, messageText);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var adjustment = Factory.LoadTop1<WhsAdjustment>(new ZQuery(WhsDocketSchema.WD_ExternalReference, "ORDERME"));
			helper.WhsHelper.FinaliseDocketWithoutUserConfirmation(adjustment.PK);
			var adjustmentLine = adjustment.Lines[0];
			var adjustmentAttr = Factory.LoadTop1<IWhsBondedWarehouseAttribute>(new ZQuery(WhsBondedWarehouseAttributeSchema.WB_ParentID, adjustmentLine.PK));
		}
	}
}
