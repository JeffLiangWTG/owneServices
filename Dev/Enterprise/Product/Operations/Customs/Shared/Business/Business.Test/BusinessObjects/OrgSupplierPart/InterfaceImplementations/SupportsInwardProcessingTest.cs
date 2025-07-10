using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DataRegistry.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class SupportsInwardProcessingTest : TestCaseWithFactory
	{
		public void TestIsSupportedForProcessing_InwardProcessingEnabled()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			using (CustomsDataRegistry.Instance.EnableInwardProcessing.SetTemporaryValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var supporter = new SupportsInwardProcessing();

				Assert(supporter.IsSupportedForProcessing());
			}
		}

		public void TestIsSupportedForProcessing_InwardProcessingDisabled()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			using (CustomsDataRegistry.Instance.EnableInwardProcessing.SetTemporaryValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var supporter = new SupportsInwardProcessing();

				Assert(!supporter.IsSupportedForProcessing());
			}
		}
	}
}
