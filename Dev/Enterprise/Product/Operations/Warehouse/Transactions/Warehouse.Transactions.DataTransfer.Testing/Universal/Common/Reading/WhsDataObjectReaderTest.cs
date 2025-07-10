using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using WarehouseDO = Enterprise.UniversalDataBuss.DataObjects.Universal.Warehouse;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class WhsDataObjectReaderTest : WhsUniversalTestCase
	{
		#region TestPopulateBusinessObject_SuccessfulImport

		public void TestPopulateBusinessObject_SuccessfulImport()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			data.Whs1.WW_WarehouseName = "SOMEWAREHOUSE";
			var clientAddress = new OrganisationDataObjectReader(GetNewAddressData_WUFSHIJNB(nameof(OrganisationTypes.WarehouseClient)), new TestErrorLogger(), Factory).GetMatchedOrNewForTesting();

			// Setup DataObjects
			var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.Order = new Order();
			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>(new[] { GetNewAddressData_WUFSHIJNB(nameof(OrganisationTypes.WarehouseClient)) }));
			dataObject.Order.Warehouse = new WarehouseDO { Code = "1", Name = "SOMEWAREHOUSE" };

			var readerForTest1 = new WhsDataObjectReader_ForTest(dataObject, Logger, Factory);
			AssertEquals(clientAddress.PK, readerForTest1.ClientAddress_ForTest.GetValue(OrgAddressSchema.PK));
			AssertEquals(data.Whs1.PK, readerForTest1.WarehousePK_ForTest);

			clientAddress.Delete();
			dataObject.Order.Warehouse = new WarehouseDO { Code = "2", Name = "OTHERWHS" };
			var readerForTest2 = new WhsDataObjectReader_ForTest(dataObject, Logger, Factory);
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Unable to match Warehouse: 2 - OTHERWHS", () => readerForTest2.WarehousePK_ForTest.Equals("POKE"));

			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Unable to match Client.", () => readerForTest2.ClientAddress_ForTest.Equals("POKE"));
		}

		public void TestPopulateBusinessObject_SuccessfulImport_WarehouseTypes()
		{
			var productWarehouse = Helper.CreateWarehouse("1", "A");
			productWarehouse.WW_WarehouseCode = "5";
			var transitWarehouse = Helper.CreateTRWWarehouse("2", "B");
			transitWarehouse.WW_WarehouseCode = "6";
			var ftzWarehouse = Helper.CreateFTZWarehouse("3", "C");
			ftzWarehouse.WW_WarehouseCode = "7";
			var cydWarehouse = Helper.CreateCYDWarehouse("4", "D");
			cydWarehouse.WW_WarehouseCode = "8";

			// Setup DataObjects
			var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.Order = new Order();
			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>(new[] { GetNewAddressData_WUFSHIJNB(nameof(OrganisationTypes.WarehouseClient)) }));
			dataObject.Order.Warehouse = new WarehouseDO { Code = "5", Name = "WAREHOUSE1" };

			var readerForTest1 = new WhsDataObjectReader_ForTest(dataObject, Logger, Factory);
			AssertEquals(productWarehouse.PK, readerForTest1.WarehousePK_ForTest);

			dataObject.Order.Warehouse = new WarehouseDO { Code = "6", Name = "WAREHOUSE2" };
			var readerForTest2 = new WhsDataObjectReader_ForTest(dataObject, Logger, Factory);
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Unable to match Warehouse: 6 - WAREHOUSE2", () => readerForTest2.WarehousePK_ForTest.Equals("POKE"));

			dataObject.Order.Warehouse = new WarehouseDO { Code = "7", Name = "WAREHOUSE3" };
			var readerForTest3 = new WhsDataObjectReader_ForTest(dataObject, Logger, Factory);
			AssertEquals(ftzWarehouse.PK, readerForTest3.WarehousePK_ForTest);

			dataObject.Order.Warehouse = new WarehouseDO { Code = "8", Name = "WAREHOUSE4" };
			var readerForTest4 = new WhsDataObjectReader_ForTest(dataObject, Logger, Factory);
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Unable to match Warehouse: 8 - WAREHOUSE4", () => readerForTest4.WarehousePK_ForTest.Equals("POKE"));
		}

		#endregion

		#region WhsDataObjectReader_ForTest

		class WhsDataObjectReader_ForTest : WhsDataObjectReader<BusinessObject>
		{
			public WhsDataObjectReader_ForTest(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
				: base(dataObject, logger, factory)
			{
			}

			#region ClientAddress_ForTest

			public IColumnIndexer ClientAddress_ForTest
			{
				get { return ClientAddress; }
			}

			#endregion

			#region WarehousePK_ForTest

			public ZGuid WarehousePK_ForTest
			{
				get { return WarehousePK; }
			}

			#endregion

			#region Unimplemented 

			public override DataContextType DataContextType
			{
				get { throw new System.NotImplementedException(); }
			}

			protected override IMatchingBusinessEntityFinder<BusinessObject> GetCombinedReferenceMatcher()
			{
				throw new System.NotImplementedException();
			}

			protected override BusinessObject GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
			{
				throw new System.NotImplementedException();
			}

			protected override void PopulateBusinessObject(BusinessObject targetBO)
			{
				throw new System.NotImplementedException();
			}

			#endregion
		}

		#endregion

		protected override TestDataForUniversal GetNewTestData() => new TestDataForUniversal(Factory, Logger, DataContextType.WarehouseOrder);
	}
}
