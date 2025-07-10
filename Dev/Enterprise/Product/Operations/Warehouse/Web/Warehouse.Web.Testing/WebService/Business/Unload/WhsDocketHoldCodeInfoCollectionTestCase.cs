using System;
using System.Linq;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	public class WhsDocketHoldCodeInfoCollectionTestCase : DataObjectInfoCollectionTestCase<WhsDocketProductHoldCodeInfo>
	{
		public void TestAdditionalConstructor()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine1.WE_CurrentInventoryStatus = "HEL";
			receiveLine1.WE_WHC_NKCurrentInventoryHeldCode = "DAM";

			Helper.CreateWhsReceiveLine(receive, data.Part1, 8m);

			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part2, 5m);
			receiveLine3.WE_CurrentInventoryStatus = "AVL";

			var receiveLine4 = Helper.CreateWhsReceiveLine(receive, data.Part2, 2m);
			receiveLine4.WE_CurrentInventoryStatus = "HEL";
			receiveLine4.WE_WHC_NKCurrentInventoryHeldCode = "LCC";

			var whsDocketHoldCodeCollectionInfo = new WhsDocketProductHoldCodeInfoCollection(receive.Lines);
			AssertNotNull(whsDocketHoldCodeCollectionInfo);
			AssertEquals("Collection has 2 records", 2, whsDocketHoldCodeCollectionInfo.Count);

			AssertEquals("1st record ProductCode correct", data.Part1.OP_PartNum, whsDocketHoldCodeCollectionInfo[0].ProductCode);
			AssertEquals("1st record HoldCodes correct count", 2, whsDocketHoldCodeCollectionInfo[0].HoldCodes.Count);
			AssertEquals("1st record HoldCodes correct first value", "DAM", whsDocketHoldCodeCollectionInfo[0].HoldCodes.First());
			AssertEquals("1st record HoldCodes correct second value", "", whsDocketHoldCodeCollectionInfo[0].HoldCodes.Last());

			AssertEquals("2nd record ProductCode correct", data.Part2.OP_PartNum, whsDocketHoldCodeCollectionInfo[1].ProductCode);
			AssertEquals("2nd record HoldCodes correct count", 2, whsDocketHoldCodeCollectionInfo[1].HoldCodes.Count);
			AssertEquals("2nd record HoldCodes correct first value", "", whsDocketHoldCodeCollectionInfo[1].HoldCodes.First());
			AssertEquals("2nd record HoldCodes correct second value", "LCC", whsDocketHoldCodeCollectionInfo[1].HoldCodes.Last());
		}

		public void TestAdditionalConstructor_NoDuplicates()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine1.WE_CurrentInventoryStatus = "HEL";
			receiveLine1.WE_WHC_NKCurrentInventoryHeldCode = "DAM";

			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 8m);
			receiveLine2.WE_CurrentInventoryStatus = "HEL";
			receiveLine2.WE_WHC_NKCurrentInventoryHeldCode = "DAM";

			var whsDocketHoldCodeCollectionInfo = new WhsDocketProductHoldCodeInfoCollection(receive.Lines);
			AssertNotNull(whsDocketHoldCodeCollectionInfo);
			AssertEquals("Collection has 1 records", 1, whsDocketHoldCodeCollectionInfo.Count);

			AssertEquals("1st record ProductCode correct", data.Part1.OP_PartNum, whsDocketHoldCodeCollectionInfo[0].ProductCode);
			AssertEquals("1st record HoldCodes correct count", 1, whsDocketHoldCodeCollectionInfo[0].HoldCodes.Count);
			AssertEquals("1st record HoldCodes correct value", "DAM", whsDocketHoldCodeCollectionInfo[0].HoldCodes.First());
		}

		public void TestAdditionalConstructor_UseCurrentHoldCodes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine1.WE_CurrentInventoryStatus = "HEL";
			receiveLine1.WE_WHC_NKCurrentInventoryHeldCode = "DAM";

			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 8m);
			receiveLine2.WE_CurrentInventoryStatus = "HEL";
			receiveLine2.WE_WHC_NKCurrentInventoryHeldCode = "QC";

			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part2, 5m);
			receiveLine3.WE_OriginalInventoryStatus = "HEL";
			receiveLine3.WE_WHC_NKOriginalInventoryHeldCode = "HEL";
			receiveLine3.WE_WHC_NKCurrentInventoryHeldCode = "";

			var receiveLine4 = Helper.CreateWhsReceiveLine(receive, data.Part2, 2m);
			receiveLine4.WE_OriginalInventoryStatus = "HEL";
			receiveLine4.WE_WHC_NKOriginalInventoryHeldCode = "LCC";
			receiveLine4.WE_WHC_NKCurrentInventoryHeldCode = "";

			var whsDocketHoldCodeCollectionInfo = new WhsDocketProductHoldCodeInfoCollection(receive.Lines);
			AssertNotNull(whsDocketHoldCodeCollectionInfo);
			AssertEquals("Collection has 1 records", 2, whsDocketHoldCodeCollectionInfo.Count);

			AssertEquals("1st record ProductCode correct", data.Part1.OP_PartNum, whsDocketHoldCodeCollectionInfo[0].ProductCode);
			AssertEquals("1st record HoldCodes correct count", 2, whsDocketHoldCodeCollectionInfo[0].HoldCodes.Count);
			AssertEquals("1st record HoldCodes correct first value", "DAM", whsDocketHoldCodeCollectionInfo[0].HoldCodes.First());
			AssertEquals("1st record HoldCodes correct second value", "QC", whsDocketHoldCodeCollectionInfo[0].HoldCodes.Last());

			AssertEquals("2nd record ProductCode correct", data.Part2.OP_PartNum, whsDocketHoldCodeCollectionInfo[1].ProductCode);
			AssertEquals("2nd record HoldCodes correct count", 1, whsDocketHoldCodeCollectionInfo[1].HoldCodes.Count);
			AssertEquals("2nd record HoldCodes correct value", "", whsDocketHoldCodeCollectionInfo[1].HoldCodes.First());
		}

		#region Implementation

		protected override Type GetExpectedObjectInfoType()
		{
			return typeof(WhsDocketProductHoldCodeInfo);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(WhsDocketProductHoldCodeInfoCollection);
		}

		protected override WhsDocketProductHoldCodeInfo GetNewObjectInfo()
		{
			return new WhsDocketProductHoldCodeInfo();
		}

		protected new WhsDocketProductHoldCodeInfoCollection Parent
		{
			get
			{
				return (WhsDocketProductHoldCodeInfoCollection)base.Parent;
			}
		}

		protected override DataObjectInfoCollection<WhsDocketProductHoldCodeInfo> GetNewObjectInfoCollection()
		{
			return new WhsDocketProductHoldCodeInfoCollection();
		}

		#endregion
	}
}
