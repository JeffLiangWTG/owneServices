using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsSerialNumberPivotCollection))]
	class WhsSerialNumberPivotCollectionTest : WhsActiveBusinessObjectCollectionTestCase<WhsSerialNumberPivotCollection>
	{
		#region TestAddNew

		public override void TestAddNew()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receiveLine1 = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R01", data.Part1, 1m, true, false).Lines[0];
			var receiveLine2 = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R02", data.Part1, 1m, true, false).Lines[0];
			var collection1 = new WhsSerialNumberPivotCollection(receiveLine1);
			var collection2 = new WhsSerialNumberPivotCollection(receiveLine2);
			AssertEquals("Precondition", 0, collection1.Count);
			AssertEquals("Precondition", 0, collection2.Count);

			AddSerialNumber(receiveLine1.SerialNumbers, "SN1");
			AddSerialNumber(receiveLine1.SerialNumbers, "SN2");
			AddSerialNumber(receiveLine2.SerialNumbers, "SN3");

			AssertContainsExactElementsInAnyOrder(new[] { "SN1", "SN2" }, collection1.Select(c => c.SerialNumberValue));
			AssertEquals(true, collection1.All(c => c.WSV_ParentID == receiveLine1.PK && c.WSV_ParentTableCode == "WE"));
			AssertEquals("SN3", collection2.Single().SerialNumberValue);
			AssertEquals(true, collection2.All(c => c.WSV_ParentID == receiveLine2.PK && c.WSV_ParentTableCode == "WE"));

			void AddSerialNumber(WhsSerialNumberPivotCollection serialNumbers, string sn)
			{
				var pivot = serialNumbers.AddNew();
				pivot.SerialNumberValue = sn;
			}
		}

		#endregion

		#region TestAllowNew

		public void TestAllowNew()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R01");
			var receiveLine = receive.Lines.AddNew();
			AssertEquals("Precondition", false, ((IBindingList)new WhsSerialNumberPivotCollection(receiveLine)).AllowNew);
			receiveLine.WE_OP = data.Part1.PK;
			AssertEquals("Attribute serial number not set for client.", false, ((IBindingList)new WhsSerialNumberPivotCollection(receiveLine)).AllowNew);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			AssertEquals(true, ((IBindingList)new WhsSerialNumberPivotCollection(receiveLine)).AllowNew);
			receive.WD_OH_Client = ZGuid.Invalid;
			AssertEquals("client not set.", false, ((IBindingList)new WhsSerialNumberPivotCollection(receiveLine)).AllowNew);
		}

		#endregion

		#region TestSerialNumberCollection_Add

		public void TestSerialNumberCollection_Add()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receiveLine = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R01", data.Part1, 1m, true, false).Lines[0];
			receiveLine.WE_SerialNumber = "SN1";
			var snc = receiveLine.SerialNumbers;
			AssertEquals("Precondition", 0, snc.Count);
			var snNew1 = snc.AddNew();
			snNew1.SerialNumberValue = "SN1";
			var snNew2 = snc.AddNew();
			snNew2.SerialNumberValue = "SN2";

			AssertEquals("Precondition", 0, Factory.GetDatabaseCount(typeof(WhsSerialNumberPivot)));

			Factory.Save();
			var serialNumbers = Factory.Load<WhsSerialNumber>(new ZQuery());
			var serialNumberPivots = Factory.Load<WhsSerialNumberPivot>(new ZQuery());
			AssertContainsExactElementsInAnyOrder(new[] { "SN1", "SN2" }, serialNumbers.Select(c => c.WSN_SerialNumber));
			AssertEquals(true, serialNumberPivots.All(c => c.WSV_ParentID == receiveLine.PK && c.WSV_ParentTableCode == "WE"));
		}

		#endregion

		#region TestSerialNumberCollection_DbHits

		public void TestSerialNumberCollection_DbHits()
		{
			var numberOfSerialnumbers = 10;
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R01", data.Part1, numberOfSerialnumbers, true, false);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var receiveLine = newFactory.Load<WhsReceiveLine>(receive.Lines[0].PK);
			newFactory.ResetDatabaseLoadCount();

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsSerialNumberPivotSchema.Constants.TableName, 1 },
				{ WhsSerialNumberSchema.Constants.TableName, numberOfSerialnumbers + 1 }, // 10 SNs + 1 New
				{ WhsWarehouseSchema.Constants.TableName, 1 },
			};

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			using (RowFactory.SetCachedTables())
			{
				var snc = receiveLine.SerialNumbers;
				for (var i = 0; i < numberOfSerialnumbers; i++)
				{
					var snNew = snc.AddNew();
					snNew.SerialNumberValue = $"SN{i + 1}";
				}

				foreach (var pv in snc)
				{
					pv.SerialNumberValue = "New";
				}
			}
		}

		#endregion

		#region TestGetFetchStrategy

		public void TestGetFetchStrategy()
		{
			var collection = GetCollectionToTest();
			AssertType<WhsSerialNumberPivotCollectionFetchStrategy>(((IBusinessObjectCollection)collection).FetchStrategy);
		}

		#endregion

		#region Implementation

		protected override WhsSerialNumberPivotCollection GetCollectionToTest()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R01", data.Part1, 100m, true, false);
			return new WhsSerialNumberPivotCollection(receive.Lines[0]);
		}

		protected override void SetUp()
		{
			base.SetUp();
			enableSchemaRedesignChanges = WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override void TearDown()
		{
			enableSchemaRedesignChanges.Dispose();
			base.TearDown();
		}

		IDisposable enableSchemaRedesignChanges;

		#endregion
	}
}
