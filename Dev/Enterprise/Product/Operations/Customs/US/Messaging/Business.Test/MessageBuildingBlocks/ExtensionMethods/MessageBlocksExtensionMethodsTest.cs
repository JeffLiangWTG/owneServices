using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Testing
{
	sealed class MessageBlocksExtensionMethodsTest : TestCaseWithFactory
	{
		public void TestGetAmount()
		{
			var list = new List<AENS89>();
			var aENS89 = new AENS89();
			list.Add(aENS89);
			aENS89.AccountingClassCode1 = "107";
			aENS89.TotalFeeAmount1 = 1m;
			aENS89.AccountingClassCode2 = "053";
			aENS89.TotalFeeAmount2 = 2m;
			aENS89.AccountingClassCode3 = "106";
			aENS89.TotalFeeAmount3 = 3m;
			aENS89.AccountingClassCode4 = "056";
			aENS89.TotalFeeAmount4 = 4m;
			aENS89.AccountingClassCode5 = "102";
			aENS89.TotalFeeAmount5 = 5m;
			aENS89 = new AENS89();
			list.Add(aENS89);
			aENS89.AccountingClassCode1 = "501";
			aENS89.TotalFeeAmount1 = 6m;
			aENS89.AccountingClassCode2 = "055";
			aENS89.TotalFeeAmount2 = 7m;
			aENS89.AccountingClassCode3 = "108";
			aENS89.TotalFeeAmount3 = 8m;
			aENS89.AccountingClassCode4 = "103";
			aENS89.TotalFeeAmount4 = 9m;
			aENS89.AccountingClassCode5 = "057";
			aENS89.TotalFeeAmount5 = 10m;
			AssertEquals(1m, list.GetAmount("107"));
			AssertEquals(2m, list.GetAmount("053"));
			AssertEquals(3m, list.GetAmount("106"));
			AssertEquals(4m, list.GetAmount("056"));
			AssertEquals(5m, list.GetAmount("102"));
			AssertEquals(6m, list.GetAmount("501"));
			AssertEquals(7m, list.GetAmount("055"));
			AssertEquals(8m, list.GetAmount("108"));
			AssertEquals(9m, list.GetAmount("103"));
			AssertEquals(10m, list.GetAmount("057"));
		}
	}
}
