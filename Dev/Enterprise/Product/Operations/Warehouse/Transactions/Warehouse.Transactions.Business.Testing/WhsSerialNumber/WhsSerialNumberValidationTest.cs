using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsSerialNumberValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region TestCheckWSN_SerialNumber_Unique_SameInventory

		public void TestCheckWSN_SerialNumber_Unique_SameInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R01");

			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 2m, data.Whs1.DefaultLocation);
			var pivot1 = receiveLine.SerialNumbers.AddNew();
			pivot1.SerialNumberValue = "SN1";
			Factory.Save();

			var pivot2 = receiveLine.SerialNumbers.AddNew();
			pivot2.SerialNumberValue = "SN1";
			AssertHasError(pivot2.SerialNumberValueInfo, "Serial # already used.");

			pivot2.SerialNumberValue = "SN2";
			AssertNoErrors(pivot2.SerialNumberValueInfo);
		}

		#endregion

		#region TestCheckWSN_SerialNumber_Unique_notUsed

		public void TestCheckWSN_SerialNumber_Unique_notUsed()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R01");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 2m, data.Whs1.DefaultLocation);
			var pivot1 = receiveLine.SerialNumbers.AddNew();
			pivot1.SerialNumberValue = "SN1";
			AssertNoErrors(pivot1.SerialNumberValueInfo);
			var pivot2 = receiveLine.SerialNumbers.AddNew();
			pivot2.SerialNumberValue = "SN2";
			AssertNoErrors(pivot2.SerialNumberValueInfo);

			receive.FinaliseDocket();
			AssertEquals("Precondition", true, receive.IsFinalised);
			Factory.Save();

			var serialNumbers = Factory.Load<WhsSerialNumber>(new ZQuery());
			serialNumbers.Single(s => s.WSN_SerialNumber.Equals("SN1")).WSN_IsInUse = ZBool.False;

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R02");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 2m, data.Whs1.DefaultLocation);
			var pivot12 = receiveLine2.SerialNumbers.AddNew();
			pivot12.SerialNumberValue = "SN1";
			var pivot22 = receiveLine2.SerialNumbers.AddNew();
			pivot22.SerialNumberValue = "SN2";
			AssertHasError(pivot22.SerialNumberValueInfo, "Serial # already used.");
		}

		#endregion

		#region TestCheckWSN_SerialNumber_Unique_UseAgain

		public void TestCheckWSN_SerialNumber_Unique_UseAgain()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R01");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			var pivot1 = receiveLine.SerialNumbers.AddNew();
			pivot1.SerialNumberValue = "SN1";
			AssertNoErrors(pivot1.SerialNumberValueInfo);

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseOrder(order);
			pick.FinalisePick();
			Factory.Save();

			// WSN_IsInUse is not used if all job are finished
			Factory.Load<WhsSerialNumber>(new ZQuery()).Single(s => s.WSN_SerialNumber.Equals("SN1")).WSN_IsInUse = ZBool.False;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(newFactory);
			var receive2 = helper.CreateWhsReceive(data.Org1, data.Whs1, "R02");
			var receiveLine2 = helper.CreateWhsReceiveLine(receive2, data.Part1, 2m, data.Whs1.DefaultLocation);
			var pivot12 = receiveLine2.SerialNumbers.AddNew();
			pivot12.SerialNumberValue = "SN1";
			AssertNoErrors(pivot12.SerialNumberValueInfo);
		}

		#endregion

		#region TestCheckWSN_SerialNumber_Unique_OtherInventory

		public void TestCheckWSN_SerialNumber_Unique_OtherInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R01");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			var pivot1 = receiveLine.SerialNumbers.AddNew();
			pivot1.SerialNumberValue = "SN1";

			receive.FinaliseDocket();
			AssertEquals("Precondition", true, receive.IsFinalised);
			Factory.Save();

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R02");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 1m, data.Whs1.DefaultLocation);
			var pivot2 = receiveLine2.SerialNumbers.AddNew();
			pivot2.SerialNumberValue = "SN1";
			AssertHasError(pivot2.SerialNumberValueInfo, "Serial # already used.");

			pivot2.SerialNumberValue = "SN2";
			AssertNoErrors(pivot2.SerialNumberValueInfo);
		}

		#endregion

		#region TestCheckWSN_SerialNumber_Unique_OtherProduct

		public void TestCheckWSN_SerialNumber_Unique_OtherProduct_Product()
		{
			TestCheckWSN_SerialNumber_Unique_OtherProduct_Core("PRO");
		}

		public void TestCheckWSN_SerialNumber_Unique_OtherProduct_Client()
		{
			TestCheckWSN_SerialNumber_Unique_OtherProduct_Core("CLI");
		}

		void TestCheckWSN_SerialNumber_Unique_OtherProduct_Core(string enforceSerialUniquenessBy)
		{
			using (WarehouseDataRegistry.Instance.EnforceSerialUniqueness.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enforceSerialUniquenessBy))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Serial, true);
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R01");
				var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
				var pivot1 = receiveLine1.SerialNumbers.AddNew();
				pivot1.SerialNumberValue = "SN1";
				var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m, data.Whs1.DefaultLocation);
				var pivot2 = receiveLine2.SerialNumbers.AddNew();
				pivot2.SerialNumberValue = "SN1";
				var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
				var pivot3 = receiveLine3.SerialNumbers.AddNew();
				pivot3.SerialNumberValue = "SN1";

				AssertNoErrors(pivot1.SerialNumberValueInfo);
				if (enforceSerialUniquenessBy.Equals("PRO"))
				{
					AssertNoErrors(pivot2.SerialNumberValueInfo);
				}
				else
				{
					AssertHasError(pivot2.SerialNumberValueInfo, "Serial # already used.");
				}
				AssertHasError(pivot3.SerialNumberValueInfo, "Serial # already used.");
			}
		}

		#endregion

		#region TestCheckWSN_SerialNumber_Unique_DbHits

		public void TestCheckWSN_SerialNumber_Unique_DbHits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R01");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			var pivot1 = receiveLine.SerialNumbers.AddNew();
			pivot1.SerialNumberValue = "SN1";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R02");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 2m, data.Whs1.DefaultLocation);
			var pivot2 = receiveLine2.SerialNumbers.AddNew();
			pivot2.SerialNumberValue = "SN2";
			Factory.Save();

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsSerialNumberSchema.Constants.TableName, 1 },
				{ WhsSerialNumberPivotSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
			};

			var newFactory = new BusinessObjectFactory();
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			using (RowFactory.SetCachedTables())
			{
				var receiveLine2InNewFactory = newFactory.Load<WhsReceiveLine>(receiveLine2.PK);
				var pivot21 = receiveLine2InNewFactory.SerialNumbers.AddNew();
				pivot21.SerialNumberValue = "SN1";
				AssertHasError(pivot21.SerialNumberValueInfo, "Serial # already used.");
			}
		}

		#endregion

		#region TestSerialNumberValueInfoSyncWithWSV_WSN_SerialNumberInfo

		public void TestSerialNumberValueInfoSyncWithWSV_WSN_SerialNumberInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R01");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			var pivot1 = receiveLine.SerialNumbers.AddNew();
			pivot1.SerialNumberValue = "SN1";

			receive.FinaliseDocket();
			AssertEquals("Precondition", true, receive.IsFinalised);
			Factory.Save();

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R02");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 1m, data.Whs1.DefaultLocation);
			var pivot2 = receiveLine2.SerialNumbers.AddNew();
			pivot2.SerialNumberValue = "SN1";
			AssertHasError(pivot2.SerialNumberValueInfo, "Serial # already used.");
			AssertHasError(pivot2.WSV_WSN_SerialNumberInfo, "Serial # already used.");

			pivot2.SerialNumberValue = "SN2";
			AssertNoErrors(pivot2.SerialNumberValueInfo);
			AssertNoErrors(pivot2.WSV_WSN_SerialNumberInfo);

			using (pivot2.SuspendValidationTesting())
			{
				var error = "This is an error!";
				pivot2.WSV_WSN_SerialNumberInfo.AddError(error);
				AssertHasError(pivot2.SerialNumberValueInfo, error);
				AssertHasError(pivot2.WSV_WSN_SerialNumberInfo, error);
			}
		}

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
