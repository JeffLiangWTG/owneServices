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
	class WhsSerialNumberHelperTest : TestCaseWithFactory
	{
		#region TestAddWhsSerialNumberAndPivotFetchHint()

		public void TestAddWhsSerialNumberAndPivotFetchHint()
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var today = ZDateTimeOffset.Today;
				int numberOfDocketLines = 10;
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R01", data.Part1, 5, true, false);

				var seialNumbers = new List<ZString>();
				for (int n = 0; n < numberOfDocketLines; n++)
				{
					var receiveLine = receive.Lines[0];
					receiveLine.WE_SerialNumber = $"SN{n}";
					receive.WD_ArrivalDate = today;
					for (int i = 1; i <= 5; i++)
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
					{ WhsSerialNumberSchema.Constants.TableName, 1 },
					{ WhsSerialNumberPivotSchema.Constants.TableName, 1 },
				};

				AssertDBHits(n: 1);

				AssertDBHits(n: 2);

				void AssertDBHits(int n)
				{
					var otherFactory = new BusinessObjectFactory();
					using (AssertDbHitsWithUsefulQueryInformation(expectedDBHits, otherFactory))
					using (RowFactory.SetCachedTables())
					{
						var receiveInOtherFactory = otherFactory.Load<WhsReceive>(receive.PK);
						var receiveLines = receiveInOtherFactory.Lines.Cast<WhsReceiveLine>().ToArray();
						WhsSerialNumberHelper.AddWhsSerialNumberAndPivotFetchHint(otherFactory, receiveLines.Take(n).Select(r => r.PK));
						AssertContainsExactElementsInAnyOrder(seialNumbers,
							receiveLines.SelectMany(l => l.SerialNumbers).Cast<WhsSerialNumberPivot>().Select(s => _ = s.SerialNumberValue));
					}
				}
			}
		}

		#endregion

		#region Helper 

		protected WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion
	}
}
