using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseCusPermitHeaderLookupsTest : SharedCusPermitHeaderLookupsTest<BaseCusPermitHeaderLookups, BaseCusPermitHeader>
	{
		#region Implementation

		protected override BaseCusPermitHeader GetNewPermitHeader(BusinessObjectFactory factory)
		{
			return Factory.NewWithValidTestData<BaseCusPermitHeader>();
		}

		#endregion
	}
}
