using Enterprise.Accounting.Integration.Testing;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Module.Testing
{
	internal class VoyageAccountingFilterStrip_AccountingFilterStripTest : AccountingFilterStripTest<VoyageAccount>
	{
		protected override VoyageAccount GetNewBusinessObjectForFilterCollection()
		{
			return Factory.NewWithValidTestData<VoyageAccount>();
		}

		protected override ModuleIdentifier FilterStripModuleID
		{
			get
			{
				return ModuleIDs.AgencyVoyageAccounting;
			}
		}
	}
}
