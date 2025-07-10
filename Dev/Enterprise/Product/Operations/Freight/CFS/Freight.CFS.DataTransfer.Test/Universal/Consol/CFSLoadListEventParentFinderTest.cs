using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.CFS.DataTransfer.Universal;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.DataTransfer.Universal.Testing;

namespace Enterprise.Freight.CFS.DataTransfer.Testing
{
	class CFSLoadListEventParentFinderTest : ConsolEventParentFinderTest<CFSLoadListConsol, CFSShipment, CFSContainer>
	{
		protected override ConsolAndShipmentEventParentFinder<CFSLoadListConsol, CFSShipment, CFSContainer> GetNewConsolEventParentFinder()
		{
			return new ConsolEventParentFinder<CFSLoadListConsol, CFSShipment, CFSContainer>(Factory, new CFSLoadListConsolDataContextManager(), new TestLogger(), new UniversalCFSHelper());
		}
	}
}
