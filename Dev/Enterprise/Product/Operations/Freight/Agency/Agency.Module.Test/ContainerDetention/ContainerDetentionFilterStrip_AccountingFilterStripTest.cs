using Enterprise.Accounting.Integration.Testing;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Module.Testing
{
	internal class ContainerDetentionFilterStrip_AccountingFilterStripTest : AccountingFilterStripTest<ContainerDetention>
	{
		protected override ContainerDetention GetNewBusinessObjectForFilterCollection()
		{
			return Factory.NewWithValidTestData<ContainerDetention>();
		}

		protected override ModuleIdentifier FilterStripModuleID
		{
			get
			{
				return ModuleIDs.AgencyContainerDetention;
			}
		}
	}
}
