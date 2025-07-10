using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class SerialNumberUniquenessCheckerTest : TestCaseWithFactory
	{
		#region TestSerialNumberUniquenessChecker_Ctor_DbHits()

		public void TestSerialNumberUniquenessChecker_Ctor_DbHits()
		{
			var today = ZDateTimeOffset.Today;
			var numberOfDocketLines = 3;
			var numberOfSerialNumberPerLine = 5;
			var numberOfAllSerialNumbers = numberOfDocketLines * numberOfSerialNumberPerLine;
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R01", data.Part1, numberOfAllSerialNumbers, true, false);

			var seialNumbers = new List<ZString>();
			for (int n = 0; n < numberOfDocketLines; n++)
			{
				var receiveLine = receive.Lines[0];
				for (int i = 1; i <= numberOfSerialNumberPerLine; i++)
				{
					var pivot = receiveLine.SerialNumbers.AddNew();
					var sn = $"SN{n}{i}";
					pivot.SerialNumberValue = sn;
					seialNumbers.Add(sn);
				}
			}
			Factory.Save();

			var expectedDBHits = new Dictionary<string, int>
				{
					{ WhsDocketSchema.Constants.TableName, 1 },
					{ WhsDocketLineSchema.Constants.TableName, 1 },
					{ WhsSerialNumberSchema.Constants.TableName, numberOfAllSerialNumbers },
					{ WhsSerialNumberPivotSchema.Constants.TableName, 1 },
				};

			var otherFactory = new BusinessObjectFactory();
			using (AssertDbHitsWithUsefulQueryInformation(expectedDBHits, otherFactory))
			using (RowFactory.SetCachedTables())
			{
				var receiveInOtherFactory = otherFactory.Load<WhsReceive>(receive.PK);
				foreach (var snp in receiveInOtherFactory.Lines.Cast<WhsReceiveLine>().SelectMany(l => l.SerialNumbers.Cast<WhsSerialNumberPivot>()))
				{
					for (int i = 0; i < 5; i++)
					{
						// Repeated validation does not trigger an additional call.
						receiveInOtherFactory.IsSerialNumberAlreadyInUse(snp);
					}
				}
			}
		}

		#endregion

		#region TestSerialNumberUniquenessChecker_Ctor_WhsReceive_DbHits()

		public void TestSerialNumberUniquenessChecker_Ctor_WhsReceive_DbHits()
		{
			var today = ZDateTimeOffset.Today;
			var numberOfDocketLines = 3;
			var numberOfSerialNumberPerLine = 5;
			var numberOfAllSerialNumbers = numberOfDocketLines * numberOfSerialNumberPerLine;
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R01", data.Part1, numberOfAllSerialNumbers, true, false);
			for (var n = 0; n < numberOfDocketLines; n++)
			{
				var receiveLine = receive.Lines[0];
				for (var i = 1; i <= numberOfSerialNumberPerLine; i++)
				{
					var pivot = receiveLine.SerialNumbers.AddNew();
					pivot.SerialNumberValue = $"SN{n}{i}";
				}
			}

			var expectedDBHits = new Dictionary<string, int>
				{
					{ WhsSerialNumberSchema.Constants.TableName, 1 },
				};
			Factory.ResetDatabaseLoadCount();

			var serialNumberUniquenessChecker = new SerialNumberUniquenessChecker(receive);
			using (AssertDbHitsWithUsefulQueryInformation(expectedDBHits, Factory))
			using (RowFactory.SetCachedTables())
			{
				foreach (var snp in receive.Lines.Cast<WhsReceiveLine>().SelectMany(l => l.SerialNumbers.Cast<WhsSerialNumberPivot>()))
				{
					for (var i = 0; i < 5; i++)
					{
						serialNumberUniquenessChecker.IsSerialNumberAlreadyInUse(snp);
					}
				}
			}
		}

		#endregion

		#region TestSerialNumberUniquenessChecker()

		public void TestSerialNumberUniquenessChecker_SameParent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R01", data.Part1, 2, true, false);
			var receiveLine1 = receive1.Lines[0];

			var pivot1 = receiveLine1.SerialNumbers.AddNew();
			pivot1.SerialNumberValue = "SN1";
			AssertEquals(false, receive1.IsSerialNumberAlreadyInUse(pivot1));

			var pivot2 = receiveLine1.SerialNumbers.AddNew();
			pivot2.SerialNumberValue = "SN1";
			AssertEquals(true, receive1.IsSerialNumberAlreadyInUse(pivot2));
		}

		public void TestSerialNumberUniquenessChecker_InDB_SameClient()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R01", data.Part1, 1, true, false);
			var receiveLine1 = receive1.Lines[0];
			var pivot1 = receiveLine1.SerialNumbers.AddNew();
			pivot1.SerialNumberValue = "SN1";
			AssertEquals(false, receive1.IsSerialNumberAlreadyInUse(pivot1));
			receive1.FinaliseDocket();
			AssertEquals("Precondition", true, receive1.IsFinalised);
			Factory.Save();

			var newFactory = NewFactory();
			var helper = new WhsTestHelperFunctions(newFactory);
			var receive2 = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R02", data.Part1, 1, true, false);
			var receiveLine2 = receive2.Lines[0];
			var pivot2 = receiveLine2.SerialNumbers.AddNew();
			pivot2.SerialNumberValue = "SN1";
			AssertEquals(true, receive2.IsSerialNumberAlreadyInUse(pivot2));
		}

		public void TestSerialNumberUniquenessChecker_InDB_OtherClient()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var otherClient = Helper.CreateClient("C2");
			Helper.CreateProductClientRelationShip(otherClient, data.Part1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(otherClient, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(otherClient, data.Part1, AttributeNumber.Serial, true);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R01", data.Part1, 1, true, false);
			var receiveLine1 = receive1.Lines[0];
			var pivot1 = receiveLine1.SerialNumbers.AddNew();
			pivot1.SerialNumberValue = "SN1";
			AssertEquals(false, receive1.IsSerialNumberAlreadyInUse(pivot1));
			receive1.FinaliseDocket();
			AssertEquals("Precondition", true, receive1.IsFinalised);
			Factory.Save();

			var newFactory = NewFactory();
			var helper = new WhsTestHelperFunctions(newFactory);
			var receive2 = helper.CreateWhsReceiveWithInventory(otherClient, data.Whs1, "R02", data.Part1, 1, true, false);
			var receiveLine2 = receive2.Lines[0];
			var pivot2 = receiveLine2.SerialNumbers.AddNew();
			pivot2.SerialNumberValue = "SN1";
			AssertEquals(false, receive2.IsSerialNumberAlreadyInUse(pivot2));
		}

		public void TestSerialNumberUniquenessChecker_Edit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R01", data.Part1, 2, true, false);
			var receiveLine1 = receive1.Lines[0];
			var pivot1 = receiveLine1.SerialNumbers.AddNew();
			pivot1.SerialNumberValue = "SN1";
			AssertEquals(false, receive1.IsSerialNumberAlreadyInUse(pivot1));
			var pivot2 = receiveLine1.SerialNumbers.AddNew();
			pivot2.SerialNumberValue = "SN2";
			AssertEquals(false, receive1.IsSerialNumberAlreadyInUse(pivot2));
			pivot2.SerialNumberValue = "SN1";
			AssertEquals(true, receive1.IsSerialNumberAlreadyInUse(pivot2));
		}

		public void TestSerialNumberUniquenessChecker_Edit_DBCheck()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R01", data.Part1, 1, true, false);
			var receiveLine1 = receive1.Lines[0];
			var pivot1 = receiveLine1.SerialNumbers.AddNew();
			pivot1.SerialNumberValue = "SN1";
			AssertEquals(false, receive1.IsSerialNumberAlreadyInUse(pivot1));
			receive1.FinaliseDocket();
			AssertEquals("Precondition", true, receive1.IsFinalised);
			Factory.Save();

			var newFactory = NewFactory();
			var helper = new WhsTestHelperFunctions(newFactory);
			var receive2 = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R02", data.Part1, 1, true, false);
			var receiveLine2 = receive2.Lines[0];
			var pivot2 = receiveLine2.SerialNumbers.AddNew();
			pivot2.SerialNumberValue = "SN2";
			AssertEquals(false, receive2.IsSerialNumberAlreadyInUse(pivot2));
			pivot2.SerialNumberValue = "SN1";
			AssertEquals(true, receive2.IsSerialNumberAlreadyInUse(pivot2));
		}

		public void TestSerialNumberUniquenessChecker_Edit_AfterSave()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R01", data.Part1, 2, true, false);
			var receiveLine1 = receive.Lines[0];
			var pivot1 = receiveLine1.SerialNumbers.AddNew();
			pivot1.SerialNumberValue = "SN1";
			var pivot2 = receiveLine1.SerialNumbers.AddNew();
			pivot2.SerialNumberValue = "SN2";
			Factory.Save();

			AssertEquals(false, receive.IsSerialNumberAlreadyInUse(pivot2));
			pivot2.SerialNumberValue = "SN1";
			var serialNumberUniquenessChecker = new SerialNumberUniquenessChecker(receive);
			AssertEquals(true, serialNumberUniquenessChecker.IsSerialNumberAlreadyInUse(pivot2));
		}

		public void TestSerialNumberUniquenessChecker_Edit_NotUsed()
		{
			TestSerialNumberUniquenessChecker_Edit_NotUsed_Core(isInUseinDB: false);
		}

		public void TestSerialNumberUniquenessChecker_Edit_Used()
		{
			TestSerialNumberUniquenessChecker_Edit_NotUsed_Core(isInUseinDB: true);
		}

		public void TestSerialNumberUniquenessChecker_Edit_NotUsed_Core(bool isInUseinDB)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R01", data.Part1, 1, true, false);
			var receiveLine1 = receive1.Lines[0];
			var pivot1 = receiveLine1.SerialNumbers.AddNew();
			pivot1.SerialNumberValue = "SN1";

			pivot1.SerialNumber.WSN_IsInUse = isInUseinDB;

			AssertEquals(false, receive1.IsSerialNumberAlreadyInUse(pivot1));
			receive1.FinaliseDocket();
			AssertEquals("Precondition", true, receive1.IsFinalised);
			Factory.Save();

			var newFactory = NewFactory();
			var helper = new WhsTestHelperFunctions(newFactory);
			var receive2 = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R02", data.Part1, 1, true, false);
			var receiveLine2 = receive2.Lines[0];
			var pivot2 = receiveLine2.SerialNumbers.AddNew();
			pivot2.SerialNumberValue = "SN1";
			AssertEquals("When Serial number is active (inUsed) should return exiusts.", isInUseinDB, receive2.IsSerialNumberAlreadyInUse(pivot2));
		}

		#endregion

		#region Helper 

		protected WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion

		#region Implementation

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
