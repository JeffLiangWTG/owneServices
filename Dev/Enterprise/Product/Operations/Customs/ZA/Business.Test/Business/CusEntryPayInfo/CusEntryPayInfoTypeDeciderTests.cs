using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class CusEntryPayInfoTypeDeciderTests : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var typeDecider = new CusEntryPayInfoTypeDecider();
			var message = Factory.New<CusEntryPayInfo>();
			var row = ((INeedRow)message).Row;
			CombineAssertions(() =>
			{
				var lineList = new LineLevelProvisionalPayments();
				var headerList = new HeaderLevelProvisionalPayments();
				foreach (var code in lineList.GetAllCodes())
				{
					message.C9_TransactionType = code;
					AssertEquals("TransactionType For " + code, typeof(ProvisionalPaymentCusEntryPayInfo), typeDecider.GetTypeForLoad(row, Factory));
				}

				foreach (var code in headerList.GetAllCodes())
				{
					message.C9_TransactionType = code;
					AssertEquals("TransactionType For " + code, typeof(ProvisionalPaymentCusEntryPayInfo), typeDecider.GetTypeForLoad(row, Factory));
				}

				message.C9_TransactionType = "VAT";
				AssertEquals("TransactionType For VAT", typeof(CusEntryPayInfo), typeDecider.GetTypeForLoad(row, Factory));
				message.C9_TransactionType = "XXX";
				AssertEquals("TransactionType For XXX", typeof(CusEntryPayInfo), typeDecider.GetTypeForLoad(row, Factory));
				message.C9_TransactionType = "";
				AssertEquals("TransactionType For Empty", typeof(CusEntryPayInfo), typeDecider.GetTypeForLoad(row, Factory));
			});
		}
	}
}
