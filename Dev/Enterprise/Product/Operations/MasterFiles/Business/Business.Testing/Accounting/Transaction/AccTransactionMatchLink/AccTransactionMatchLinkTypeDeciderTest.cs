using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccTransactionMatchLinkTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForNew()
		{
			Type type = new AccTransactionMatchLinkTypeDecider().GetTypeForNew();
			AssertEquals("Enterprise.Accounting.Business.Base.Matching.TransactionMatchLink", type.FullName);
		}

		public void TestGetTypeForLoad()
		{
			Type type = new AccTransactionMatchLinkTypeDecider().GetTypeForLoad(null, null);
			AssertEquals("Enterprise.Accounting.Business.Base.Matching.TransactionMatchLink", type.FullName);
		}

		public void TestGetTypeForBinding()
		{
			Type type = new AccTransactionMatchLinkTypeDecider().GetTypeForBinding();
			AssertEquals("Enterprise.Accounting.Business.Base.Matching.TransactionMatchLink", type.FullName);
		}
	}
}
