using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CommonConsolDocumentSupporterForTest : CommonConsolDocumentSupporter
	{
		public CommonConsolDocumentSupporterForTest(CommonConsol consol)
			: base(consol)
		{
		}

		public new ICommonConsolDocumentSupporterQueryProvider QueryProvider
		{
			get { return base.QueryProvider; }
		}

		public ContainerToSelectFromForPrintingCollection ContainersToSelectFromForTest
		{
			get { return base.ContainersToSelectFrom; }
		}

		public DocumentWrapper[] GetContainerDocBusinessObjects()
		{
			return base.GetContainerDocBusinessObjects(Factory.New<IStmMenuItem>());
		}

		public DocumentWrapper[] GetWrappersByContainerForTest(ContainerToSelectFromForPrintingCollection containersToSelectFrom)
		{
			return base.GetWrappersByContainer(containersToSelectFrom);
		}

		public DocumentWrapper CreateContainerWrapperForTest(CommonContainer container)
		{
			return base.CreateContainerWrapper(container);
		}

		public IDocumentSupportable[] GetChildCollectionByContainerForTest(ContainerToSelectFromForPrintingCollection containersToSelectFrom)
		{
			return base.GetChildCollectionByContainer(containersToSelectFrom);
		}

		public CommonContainer CreateContainerDocumentSupportableForTest(CommonContainer container)
		{
			return base.CreateContainerDocumentSupportable(container);
		}
	}
}
