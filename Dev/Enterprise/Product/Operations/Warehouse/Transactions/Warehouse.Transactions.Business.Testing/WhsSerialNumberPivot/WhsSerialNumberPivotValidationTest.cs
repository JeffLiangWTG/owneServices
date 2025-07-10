using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsSerialNumberPivotValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region TestCheckWSV_WSN_SerialNumber_Unique_DbHits

		public void TestCheckWSV_WSN_SerialNumber_Unique_DbHits()
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
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 10m, data.Whs1.DefaultLocation);
			Factory.Save();

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsSerialNumberPivotSchema.Constants.TableName, 1 },
				{ WhsSerialNumberSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
			};

			var newFactory = new BusinessObjectFactory();
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			using (RowFactory.SetCachedTables())
			{
				var receiveLine2InNewFactory = newFactory.Load<WhsReceiveLine>(receiveLine2.PK);
				for (var i = 0; i < 10; i++)
				{
					var pivotNew = receiveLine2InNewFactory.SerialNumbers.AddNew();
					pivotNew.SerialNumberValue = "SN1";
					AssertHasError(pivotNew.SerialNumberValueInfo, "Serial # already used.");
				}
			}
		}

		#endregion

		#region TestFKsToNotValidateForCancelledRecords

		public void TestShouldValidateFKToCancelledRecord()
		{
			var pivot = Factory.New<WhsSerialNumberPivot>();
			var validation = new TestWhsSerialNumberPivotValidation(pivot);

			foreach (var propertyInfo in pivot.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(p => p.IsPersistent))
			{
				if (propertyInfo.Name == WhsSerialNumberPivotSchema.Constants.WSV_WSN_SerialNumber)
				{
					AssertEquals("NK/FK which cannot be cancelled.", false, validation.ShouldValidateFKToCancelledRecordExposed(propertyInfo));
				}
				else
				{
					AssertEquals("All other properties should just return base condition of true.", true, validation.ShouldValidateFKToCancelledRecordExposed(propertyInfo));
				}
			}
		}

		#endregion

		#region TestWhsSerialNumberPivotValidation

		class TestWhsSerialNumberPivotValidation : WhsSerialNumberPivotValidation
		{
			public TestWhsSerialNumberPivotValidation(WhsSerialNumberPivot parent)
				: base(parent)
			{
			}

			public bool ShouldValidateFKToCancelledRecordExposed(ZPropertyInfo info) => ShouldValidateFKToCancelledRecord(info);
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
