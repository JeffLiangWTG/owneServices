using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsAsnLine))]
	class WhsAsnLineTest : WhsBusinessObjectTestCase
	{
		#region Test Cases

		#region Properties

		#region CommodityCode

		public void TestCommodityCode()
		{
			var asnLine = Factory.New<WhsAsnLine>();
			AssertEquals(ZString.Empty, asnLine.CommodityCode);

			var part = Helper.CreateProduct(Helper.CreateClient("xxx"), "P1");
			asnLine.WN_OP = part.PK;
			AssertEquals(ZString.Empty, asnLine.CommodityCode);

			part.OP_RH_NKCommodityCode = "11";
			AssertEquals(part.OP_RH_NKCommodityCode, asnLine.CommodityCode);
		}

		public void TestCommodityCodeInfo()
		{
			AssertEquals("CommodityCode", Factory.New<WhsAsnLine>().CommodityCodeInfo.Name);
		}

		#endregion

		#region WI_OP_Desc

		public void TestWN_OP_Desc()
		{
			var asnLine = Factory.New<WhsAsnLine>();
			AssertEquals(ZString.Empty, asnLine.WN_OP_Desc);

			var part = Helper.CreateProduct(Helper.CreateClient("xxx"), "P1");
			part.OP_Desc = "Part Description";
			asnLine.WN_OP = part.PK;
			AssertEquals("Part Description", asnLine.WN_OP_Desc);

			part.OP_Desc = "12345";
			AssertEquals("12345", asnLine.WN_OP_Desc);
		}

		public void TestWN_OP_DescInfo()
		{
			var asnLine = Factory.New<WhsAsnLine>();
			AssertEquals("WN_OP_Desc", asnLine.WN_OP_DescInfo.Name);
			AssertEquals(true, asnLine.WN_OP_DescInfo.ReadOnly);
		}

		#endregion

		#region TestWN_LineNoInfo

		public void TestWN_LineNoInfo()
		{
			var asnLine = Factory.New<WhsAsnLine>();
			AssertEquals("WN_LineNo", asnLine.WN_LineNoInfo.Name);
			AssertEquals(true, asnLine.WN_LineNoInfo.ReadOnly);
		}

		#endregion

		#region TestWN_OPInfo

		public void TestWN_OPInfo()
		{
			var asnLine = Factory.New<WhsAsnLine>();
			AssertEquals("WN_OP", asnLine.WN_OPInfo.Name);
			AssertEquals(true, asnLine.WN_OPInfo.ReadOnly);
		}

		#endregion

		#region TestWN_QuantityInfo

		public void TestWN_QuantityInfo()
		{
			var asnLine = Factory.New<WhsAsnLine>();
			AssertEquals("WN_Quantity", asnLine.WN_QuantityInfo.Name);
			AssertEquals(true, asnLine.WN_QuantityInfo.ReadOnly);
		}

		#endregion

		#region TestWN_QuantityUQInfo

		public void TestWN_QuantityUQInfo()
		{
			var asnLine = Factory.New<WhsAsnLine>();
			AssertEquals("WN_QuantityUQ", asnLine.WN_QuantityUQInfo.Name);
			AssertEquals(true, asnLine.WN_QuantityUQInfo.ReadOnly);
		}

		#endregion

		#region TestWN_SubLineNoInfo

		public void TestWN_SubLineNoInfo()
		{
			var asnLine = Factory.New<WhsAsnLine>();
			AssertEquals("WN_SubLineNo", asnLine.WN_SubLineNoInfo.Name);
			AssertEquals(true, asnLine.WN_SubLineNoInfo.ReadOnly);
		}

		#endregion

		#region TestWN_ExpiryDateInfo

		public void TestWN_ExpiryDateInfo()
		{
			AssertEquals(true, Factory.New<WhsAsnLine>().WN_ExpiryDateInfo.ReadOnly);
		}

		#endregion

		#region TestWN_PackingDateInfo

		public void TestWN_PackingDateInfo()
		{
			AssertEquals(true, Factory.New<WhsAsnLine>().WN_PackingDateInfo.ReadOnly);
		}

		#endregion

		#region TestWN_PalletId_MaxLength

		public void TestWN_PalletId_MaxLength()
		{
			var asnLine = Factory.New<WhsAsnLine>();
			AssertNoExceptionThrown(() => asnLine.WN_PalletId = "123456789012345678901234567890");
		}

		#endregion

		#region TestWN_PartAttrib1Info

		public void TestWN_PartAttrib1Info()
		{
			AssertEquals(true, Factory.New<WhsAsnLine>().WN_PartAttrib1Info.ReadOnly);
		}

		#endregion

		#region TestWN_PartAttrib1_MaxLength

		public void TestWN_PartAttrib1_MaxLength()
		{
			var asnLine = Factory.New<WhsAsnLine>();
			AssertNoExceptionThrown(() =>
				asnLine.WN_PartAttrib1 = "".PadLeft(WhsAsnLineSchema.WN_PartAttrib1.MaxLength, 'A'));
		}

		#endregion

		#region TestWN_PartAttrib1_Exceed_MaxLength

		public void TestWN_PartAttrib1_Exceed_MaxLength()
		{
			var asnLine = Factory.New<WhsAsnLine>();
			try
			{
				AssertExceptionThrown<MaxLengthExceededException>(() =>
					asnLine.WN_PartAttrib1 = ZString.Replicate('A', WhsAsnLineSchema.WN_PartAttrib1.MaxLength + 1));
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		#endregion

		#region TestWN_PartAttrib2Info

		public void TestWN_PartAttrib2Info()
		{
			AssertEquals(true, Factory.New<WhsAsnLine>().WN_PartAttrib2Info.ReadOnly);
		}

		#endregion

		#region TestWN_PartAttrib2_MaxLength

		public void TestWN_PartAttrib2_MaxLength()
		{
			var asnLine = Factory.New<WhsAsnLine>();
			AssertNoExceptionThrown(() => asnLine.WN_PartAttrib2 = "".PadLeft(WhsAsnLineSchema.WN_PartAttrib2.MaxLength, 'A'));
		}

		#endregion

		#region TestWN_PartAttrib2_Exceed_MaxLength

		public void TestWN_PartAttrib2_Exceed_MaxLength()
		{
			var asnLine = Factory.New<WhsAsnLine>();
			try
			{
				AssertExceptionThrown<MaxLengthExceededException>(() =>
					asnLine.WN_PartAttrib2 = ZString.Replicate('A', WhsAsnLineSchema.WN_PartAttrib2.MaxLength + 1));
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		#endregion

		#region TestWN_PartAttrib3Info

		public void TestWN_PartAttrib3Info()
		{
			AssertEquals(true, Factory.New<WhsAsnLine>().WN_PartAttrib3Info.ReadOnly);
		}

		#endregion

		#region TestWN_PartAttrib3_MaxLength

		public void TestWN_PartAttrib3_MaxLength()
		{
			var asnLine = Factory.New<WhsAsnLine>();
			AssertNoExceptionThrown(() => asnLine.WN_PartAttrib3 = "".PadLeft(WhsAsnLineSchema.WN_PartAttrib3.MaxLength, 'A'));
		}

		#endregion

		#region TestWN_PartAttrib3_Exceed_MaxLength

		public void TestWN_PartAttrib3_Exceed_MaxLength()
		{
			var asnLine = Factory.New<WhsAsnLine>();
			try
			{
				AssertExceptionThrown<MaxLengthExceededException>(() =>
					asnLine.WN_PartAttrib3 = ZString.Replicate('A', WhsAsnLineSchema.WN_PartAttrib3.MaxLength + 1));
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		#endregion

		#region TestWN_SerialNumberInfo

		public void TestWN_SerialNumberInfo()
		{
			AssertEquals(true, Factory.New<WhsAsnLine>().WN_SerialNumberInfo.ReadOnly);
		}

		#endregion

		#region TestWN_AddedAfterReceiveStarted

		public void TestWN_AddedAfterReceiveStarted()
		{
			var asnLine = Factory.New<WhsAsnLine>();
			AssertEquals(false, asnLine.WN_AddedAfterReceiveStarted);

			asnLine.WN_AddedAfterReceiveStarted = true;
			AssertEquals(true, asnLine.WN_AddedAfterReceiveStarted);
		}

		#endregion

		#endregion

		#endregion

		#region Related Entities

		public void TestDocket()
		{
			var asnLine = Factory.New<WhsAsnLine>();
			AssertNull(asnLine.Docket);

			var receive = Factory.New<WhsReceive>();
			asnLine.WN_WD = receive.PK;
			AssertNotNull(asnLine.Docket);
			AssertEquals(typeof(WhsReceive), asnLine.Docket.GetType());
			AssertEquals(receive, asnLine.Docket);
		}

		#endregion

		#region TestSupportsNotes

		public void TestSupportsNotes()
		{
			AssertEquals("WhsAsnLine should not support notes.", false, Factory.New<WhsAsnLine>().SupportsNotes);
		}

		#endregion

		#region TestUsedAttributesCount

		public void TestUsedAttributesCount()
		{
			var asnLine = (WhsAsnLine)GetNewBusinessObject();
			asnLine.WN_PartAttrib1 = "";
			asnLine.WN_PartAttrib2 = "";
			asnLine.WN_PartAttrib3 = "";
			asnLine.WN_SerialNumber = "";
			asnLine.WN_PackingDate = ZDate.Empty;
			asnLine.WN_ExpiryDate = ZDate.Empty;
			asnLine.WN_PalletId = "";
			AssertEquals(0, asnLine.UsedAttributesAndPalletIdCount);

			asnLine.WN_PartAttrib1 = "1";
			AssertEquals(1, asnLine.UsedAttributesAndPalletIdCount);

			asnLine.WN_PartAttrib2 = "2";
			AssertEquals(2, asnLine.UsedAttributesAndPalletIdCount);

			asnLine.WN_PartAttrib3 = "3";
			AssertEquals(3, asnLine.UsedAttributesAndPalletIdCount);

			asnLine.WN_PackingDate = ZDate.Today;
			AssertEquals(4, asnLine.UsedAttributesAndPalletIdCount);

			asnLine.WN_ExpiryDate = ZDate.Today;
			AssertEquals(5, asnLine.UsedAttributesAndPalletIdCount);

			asnLine.WN_PalletId = "ABC";
			AssertEquals(6, asnLine.UsedAttributesAndPalletIdCount);

			asnLine.WN_SerialNumber = "SN1";
			AssertEquals(7, asnLine.UsedAttributesAndPalletIdCount);
		}

		#endregion

		#region TestSerialNumbers

		public void TestSerialNumbers()
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Serial, true);
				Factory.Save();

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 3m);
				var asnLine1 = receive.AsnLines.Add(receiveLine1);
				var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 3m);
				var asnLine2 = receive.AsnLines.Add(receiveLine2);

				for (int i = 1; i <= 3; i++)
				{
					var pivotReceiveLine = receive.Lines[0].SerialNumbers.AddNew();
					pivotReceiveLine.SerialNumberValue = $"SN{i}";
					CreateWhsSerialNumberToAsnLine(asnLine1, pivotReceiveLine.WSV_WSN_SerialNumber);
				}

				AssertContainsExactElementsInAnyOrder(new[] { "SN1", "SN2", "SN3" }, asnLine1.SerialNumbers.Select(c => c.SerialNumberValue));
				AssertEquals(0, asnLine2.SerialNumbers.Count);

				void CreateWhsSerialNumberToAsnLine(WhsAsnLine asnLine, ZGuid serialNumberPK)
				{
					var serialNumberPivot = Factory.New<WhsSerialNumberPivot>();
					serialNumberPivot.WSV_ParentID = asnLine.PK;
					serialNumberPivot.WSV_ParentTableCode = WhsAsnLineSchema.Constants.Prefix;
					serialNumberPivot.WSV_IsReleaseCaptured = false;
					serialNumberPivot.WSV_WSN_SerialNumber = serialNumberPK;
				}
			}
		}

		#endregion

		#region TestISerialNumberParentMembers

		public void TestISerialNumberParentMembers()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			ISerialNumberParent asnLine = receive.AsnLines.Add(receiveLine);

			AssertEquals(asnLine.PK, asnLine.PK);
			AssertEquals(data.Org1.PK, asnLine.ClientPK);
			AssertEquals(data.Part1.PK, asnLine.ProductPK);
			AssertEquals(WhsAsnLineSchema.Constants.Prefix, asnLine.TablePrefix);
			AssertEquals(true, asnLine.SerialNumberReadOnly);
			AssertEquals(false, asnLine.IsInDatabase);
			AssertEquals(false, asnLine.IsAllowedToCreateOriginalSerialNumberRecord);
			AssertEquals(receive.Factory, asnLine.Factory);
			AssertEquals(typeof(WhsSerialNumberPivotCollection), asnLine.SerialNumbers.GetType());
			Factory.Save();

			var pivot = receiveLine.SerialNumbers.AddNew();
			AssertEquals(false, asnLine.IsSerialNumberAlreadyInUse(pivot));
			AssertEquals(true, asnLine.IsInDatabase);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			AssertEquals(true, asnLine.SerialNumberReadOnly);
		}

		#endregion

		#region Lookups

		public void TestLookups()
		{
			AssertEquals(typeof(WhsAsnLineLookups), Factory.New<WhsAsnLine>().Lookups.GetType());
		}

		#endregion

		#region Validation

		public void TestValidation()
		{
			AssertEquals(typeof(WhsAsnLineValidation), Factory.New<WhsAsnLine>().Validation.GetType());
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			WhsReceive receive = Helper.CreateWhsReceive(Helper.CreateClient(), Helper.CreateWarehouse("TST"));
			WhsAsnLine result = Factory.New<WhsAsnLine>();
			result.WN_WD = receive.PK;
			return result;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		#endregion
	}
}
