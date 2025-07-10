using System;
using CargoWise.Integration;
using Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using ICodeDescriptionPairListProvider = Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsWarehouseJobTypeWithSubTypeCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		#region TestIsReturningCorrectCollection

		public override void TestIsReturningCorrectCollection()
		{
			var provider = new WhsWarehouseJobTypeWithSubTypeCodeDescriptionPairProvider();
			var generatedList = ((ICodeDescriptionPairListProvider)provider).GetCodeDescriptionPairList();
			var expectedList = GetWhsDocketTypeAndSubTypeCodeDescriptioPairList();
			AssertEquals(expectedList.Count, generatedList.Count);
			AssertEquals(expectedList.CodesAsString, generatedList.CodesAsString);
		}

		ReadOnlyCodeDescriptionPairList GetWhsDocketTypeAndSubTypeCodeDescriptioPairList()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("INWREC", "Receive - REC (GOODS RECEIPT)");
			list.AddPair("INWCUS", "Receive - CUS (CUSTOMS RECEIPT)");
			list.AddPair("INWRET", "Receive - RET (RETURNED GOODS)");
			list.AddPair("ORDORD", "Order - ORD (ORDER)");
			list.AddPair("ORDBAK", "Order - BAK (BACK ORDER)");
			list.AddPair("ORDCUS", "Order - CUS (CUSTOMS RELEASE)");
			list.AddPair("ORDCPS", "Order - CPS (CCUSTOMS RELEASE WITH PERMIT)");
			list.AddPair("ORDREP", "Order - REP (REPEAT ORDER)");
			list.AddPair("ADJNEA", "Adjustment - NEA (ADJUSTMENT)");
			list.AddPair("ADJCUS", "Adjustment - CUS (CUSTOMS AMENDMENT)");
			list.AddPair("ADJIWA", "Adjustment - IWA (INTERNAL WAREHOUSE ADJUSTMENT)");
			list.AddPair("ADJNOA", "Adjustment - NOA (OWNERSHIP ADJUSTMENT)");
			list.AddPair("TFRTFR", "Transfer - TFR (INTERNAL)");
			list.AddPair("TFRIWD", "Transfer - IWD (INTER-WAREHOUSE (DESTINATION))");
			list.AddPair("TFRIWS", "Transfer - IWS (INTER-WAREHOUSE (SOURCE))");
			list.AddPair("ADH", "Ad Hoc Service Job - ADH (AD HOC SERVICE JOB)");
			list.AddPair("WVO", "VAS Order - WVO (VALUE-ADDED SERVICE ORDER)");
			return list;
		}

		#endregion

		#region TestDoesNotIncludeWorkOrder

		public void TestDoesNotIncludeWorkOrder()
		{
			var generatedList = CreateCodeDescriptionPairListProvider()
				.GetCodeDescriptionPairList();
			var workOrderSubType = new WorkOrderType();
			foreach (ICodeDescription workorderSubType in workOrderSubType)
			{
				AssertEquals(false, generatedList.ContainsCode(DocketType.Codes.WorkOrder + workorderSubType.Code));
			}
		}

		#endregion

		#region TestGetCombinedDocketTypeSubTypeList_ThrowsException

		public void TestGetCombinedDocketTypeSubTypeList_ThrowsException()
		{
			var provider = new WhsWarehouseJobTypeWithSubTypeCodeDescriptionPairProvider();
			AssertExceptionThrown("When default code is not in the list of sub types exception should be thrown.",
				typeof(ArgumentException), "defaultSubTypeCode is not in subTypeList.",
				() => provider.GetCombinedDocketTypeSubTypeList("BLA", "BLA", new ReceiveType(),
					OrderType.Codes.Order));
		}

		#endregion

		#region Implementation

		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new WhsWarehouseJobTypeWithSubTypeCodeDescriptionPairProvider();
		}

		#endregion
	}
}
