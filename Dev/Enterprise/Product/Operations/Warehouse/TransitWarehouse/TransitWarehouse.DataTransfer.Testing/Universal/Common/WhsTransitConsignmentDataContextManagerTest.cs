using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	abstract class WhsTransitConsignmentDataContextManagerTest<TContextManager, TBizO> : ShipmentDataContextManagerTestCase<TContextManager, TBizO>
		where TContextManager : DataContextManager<TBizO>, IShipmentDataContextManager, new()
		where TBizO : BusinessObject, IConsignment
	{
		public void TestDataContextKey()
		{
			TestDataContextKeyCore();
		}

		public void TestDataContextType()
		{
			AssertEquals(ExpectedDataContextType, new TContextManager().DataContextType);
		}

		public void TestDefaultOutputDirectory()
		{
			AssertEquals("DefaultOutputDirectory should have no value", null, new TContextManager().DefaultOutputDirectory);
		}

		public void TestEventContextValues_NonAIR()
		{
			var warehouse1 = CreateWarehouse("W1");
			var warehouse2 = CreateWarehouse("W2");

			var consignmentWithAll = Factory.New<TBizO>();
			var consignmentWithMasterBill = Factory.New<TBizO>();
			var consignmentWithHouseBill = Factory.New<TBizO>();
			var consignmentWithWarehouse = Factory.New<TBizO>();
			SetJobID(consignmentWithAll, "ALL");
			SetJobID(consignmentWithMasterBill, "WithMAB");
			SetJobID(consignmentWithHouseBill, "WithHSB");
			SetJobID(consignmentWithWarehouse, "WithWHSOnly");

			SetHouseBillNumber(consignmentWithAll, "HSB1");
			SetHouseBillNumber(consignmentWithHouseBill, "HSBOnly");

			PopulateAdditionalReference(consignmentWithAll.PK, consignmentWithAll.TablePrefix, TransportAdditionalReferenceTypes.Codes.MasterBill, "MAB1");
			PopulateAdditionalReference(consignmentWithMasterBill.PK, consignmentWithMasterBill.TablePrefix, TransportAdditionalReferenceTypes.Codes.MasterBill, "MABOnly");

			SetWarehouse(consignmentWithAll, warehouse1);
			SetWarehouse(consignmentWithMasterBill, warehouse2);
			SetWarehouse(consignmentWithHouseBill, warehouse2);
			SetWarehouse(consignmentWithWarehouse, warehouse2);

			AssertEventContextValues_SEA(consignmentWithAll, "W1", "HSB1", "MAB1", "ALL");
			AssertEventContextValues_SEA(consignmentWithMasterBill, "W2", "", "MABOnly", "WithMAB");
			AssertEventContextValues_SEA(consignmentWithHouseBill, "W2", "HSBOnly", "", "WithHSB");
			AssertEventContextValues_SEA(consignmentWithWarehouse, "W2", "", "", "WithWHSOnly");
		}

		public void TestManagesShipments()
		{
			AssertEquals("ManagesShipments should be true", true, new TContextManager().ManagesShipments);
		}

		public void TestRecipientRoleTargettedToThisModule()
		{
			foreach (var supportedRecipientRoleType in SupportedRecipientRoleTypes)
			{
				AssertDataTargetCollection(DataContextType.TransportConsignmentRunSheet, supportedRecipientRoleType, null);
				AssertDataTargetCollection(DataContextType.ForwardingShipment, supportedRecipientRoleType, ExpectedDataContextType.ToString());
				AssertDataTargetCollection(DataContextType.ForwardingConsol, supportedRecipientRoleType, null);
			}

			AssertDataTargetCollection(DataContextType.ForwardingConsol, RecipientRoleType.ACR, null);
		}

		protected abstract void TestDataContextKeyCore();

		protected abstract DataContextType ExpectedDataContextType { get; }

		protected abstract void SetWarehouse(TBizO header, WhsWarehouse warehouse);

		protected abstract void SetJobID(TBizO header, string jobID);
		protected abstract void SetHouseBillNumber(TBizO header, string houseBillNumber);

		protected WhsWarehouse CreateWarehouse(string code)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_Code = code;
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_OA_WarehouseAddress = org.MainAddress.PK;
			return warehouse;
		}

		protected static void AssertEventContextValues_AIR(TBizO headerWithRunSheetNumber, string depotCode, string houseBill = "", string masterBillNumber = "", string jobID = "")
		{
			string eventContextValues = $@"";

			if (!string.IsNullOrEmpty(houseBill) && !string.IsNullOrEmpty(masterBillNumber))
			{
				eventContextValues = $@"
HAWBNumber - {houseBill}
MAWBNumber - {masterBillNumber}
CFSReference - {jobID}
DepotCode - {depotCode}";
			}
			else if (!string.IsNullOrEmpty(houseBill))
			{
				eventContextValues = $@"
HAWBNumber - {houseBill}
CFSReference - {jobID}
DepotCode - {depotCode}";
			}
			else if (!string.IsNullOrEmpty(masterBillNumber))
			{
				eventContextValues = $@"
MAWBNumber - {masterBillNumber}
CFSReference - {jobID}
DepotCode - {depotCode}";
			}
			else
			{
				eventContextValues = $@"
CFSReference - {jobID}
DepotCode - {depotCode}";
			}

			AssertMultilineASCIIEquals("manager.EventContextValues", eventContextValues.Trim(), (headerWithRunSheetNumber.GetUniversalDataContextManager() as IEventDataContextManager).EventContextValues.ToStringContents(e => e.Key + " - " + e.Value));
		}

		protected void PopulateAdditionalReference(ZGuid parentPK, string tableName, string refType, ZString value)
		{
			var entryNum = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNum.CE_ParentID = parentPK;
			entryNum.CE_ParentTable = tableName;
			entryNum.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			entryNum.CE_EntryType = refType;
			entryNum.CE_EntryNum = value;
		}

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => new[] { RecipientRoleType.DTW, RecipientRoleType.ATW };

		protected override string ValidPopulatedUniversalShipmentXML
		{
			get
			{
				return @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment Version=""0.1"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Shipment>
	  <DataContext>
		  <DataSourceCollection>
			<DataSource>
			  <Type>ForwardingShipment</Type>
			  <Key>S00001000</Key>
			</DataSource>
		  </DataSourceCollection>
		</DataContext>
		<WayBillNumber>HouseBill123</WayBillNumber>
		<WayBillType>
		  <Code>HWB</Code>
		  <Description>House Waybill</Description>
		</WayBillType>
   
		<OrganizationAddressCollection>
			<OrganizationAddress>
				<AddressType>ConsignorDocumentaryAddress</AddressType>
				<OrganizationCode>CRAHOLSYD</OrganizationCode>
				<CompanyName>CRACKERJACK HOLDINGS</CompanyName>
				<Address1>1804 Fudrucker Way</Address1>
			</OrganizationAddress>
			<OrganizationAddress>
				<AddressType>ConsigneeDocumentaryAddress</AddressType>
				<OrganizationCode>INTHEMSYD</OrganizationCode>
				<CompanyName>In The Moment</CompanyName>
				<Address1>Unit 12, Level 3</Address1>
			</OrganizationAddress>
			<OrganizationAddress>
				<AddressType>LocalCartageCFS</AddressType>
				<OrganizationCode>WUFSHIJNB</OrganizationCode>
				<CompanyName>WUFU SHIPPING LINE</CompanyName>
				<Address1>Level 2, Building G</Address1>
			</OrganizationAddress>
		</OrganizationAddressCollection>

		<PackingLineCollection>
		  <PackingLine>    
			<Height>1.000</Height>
			<Length>1.000</Length>
			<LengthUnit>
			  <Code>M</Code>
			  <Description>Metres</Description>
			</LengthUnit>
			<PackQty>2</PackQty>
			<PackType>
			  <Code>PLT</Code>
			  <Description>Pallet</Description>
			</PackType>
			<ReferenceNumber></ReferenceNumber>
			<Volume>2.000</Volume>
			<VolumeUnit>
			  <Code>M3</Code>
			  <Description>Cubic Metres</Description>
			</VolumeUnit>
			<Weight>150.000</Weight>
			<WeightUnit>
			  <Code>KG</Code>
			  <Description>Kilograms</Description>
			</WeightUnit>
			<Width>1.000</Width>
			<PackedItemCollection>
			</PackedItemCollection>
		  </PackingLine>
		</PackingLineCollection>
  </Shipment>
</UniversalShipment>
";
			}
		}

		protected override void MakeShipmentUsableForThisRole(RecipientRoleType recipientRoleType, Shipment shipmentWithRecipientRole)
		{
			shipmentWithRecipientRole.DataContext.ClearDataSourceCollection();
			shipmentWithRecipientRole.DataContext.AddDataSource(DataContextType.ForwardingShipment, null);
		}

		protected override void SetupDataForDataContextManagerTestCase()
		{
			base.SetupDataForDataContextManagerTestCase();
			_ = Data.Orgs.CRAHOLSYD;
			_ = Data.Warehouse;
		}

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		}

		protected Lazy<EmbeddedResourceRetriever> resourceRetriever;

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		protected TestDataForUniversal Data => data ?? (data = new TestDataForUniversal(Factory, new TestErrorLogger()));
		TestDataForUniversal data;

		static void AssertEventContextValues_SEA(TBizO headerWithRunSheetNumber, string depotCode, string houseBill = "", string masterBillNumber = "", string jobID = "")
		{
			string eventContextValues = $@"";

			if (!string.IsNullOrEmpty(houseBill) && !string.IsNullOrEmpty(masterBillNumber))
			{
				eventContextValues = $@"
HBOLNumber - {houseBill}
MBOLNumber - {masterBillNumber}
CFSReference - {jobID}
DepotCode - {depotCode}";
			}
			else if (!string.IsNullOrEmpty(houseBill))
			{
				eventContextValues = $@"
HBOLNumber - {houseBill}
CFSReference - {jobID}
DepotCode - {depotCode}";
			}
			else if (!string.IsNullOrEmpty(masterBillNumber))
			{
				eventContextValues = $@"
MBOLNumber - {masterBillNumber}
CFSReference - {jobID}
DepotCode - {depotCode}";
			}
			else
			{
				eventContextValues = $@"
CFSReference - {jobID}
DepotCode - {depotCode}";
			}

			AssertMultilineASCIIEquals("manager.EventContextValues", eventContextValues.Trim(), (headerWithRunSheetNumber.GetUniversalDataContextManager() as IEventDataContextManager).EventContextValues.ToStringContents(e => e.Key + " - " + e.Value));
		}

		void AssertDataTargetCollection(DataContextType dataSourceDataContext, RecipientRoleType roleType, string expectedDataContextType)
		{
			IShipmentDataContextManager manager = new TContextManager();
			var runSheetDataSource = DataContextFactory.New();
			runSheetDataSource.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = roleType, ServiceCode = SupportedRecipientServices(roleType).FirstOrDefault() } } });
			runSheetDataSource.AddDataSource(dataSourceDataContext, "");
			manager.DefaultDataTargetFromRecipientRole(runSheetDataSource, new DummyXmlSessionTracker(null));
			AssertEquals(expectedDataContextType, runSheetDataSource.DataTargetCollection?.SingleOrDefault()?.Type);
		}
	}
}
