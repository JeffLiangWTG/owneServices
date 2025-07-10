using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	class ContainerStockMatcher : CombinationKeyMatcher<RefContainerStock, ContainerStockReferences>
	{
		public ContainerStockMatcher(BusinessObjectFactory factory, ContainerStockReferences references, IXmlImportLogger logger)
			: base(factory, references, logger)
		{
		}

		protected override bool CheckLatestParent(RefContainerStock container, RefContainerStock containerToCompareTo)
		{
			return container.Logs.AddedLog.SL_EventTime > containerToCompareTo.Logs.AddedLog.SL_EventTime;
		}

		protected override void BuildMatchingQueryAndMatchDelegates(ContainerStockReferences references)
		{
			AddPossibleMatch(RefContainerStockSchema.R6_ContainerNum, references.ContainerNumber, container => GetMatchCount(container.R6_ContainerNum, references.ContainerNumber));
		}

		protected override void BuildFallbackMatchDelegates(ContainerStockReferences references)
		{
		}
	}
}
