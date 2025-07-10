using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CommonShipmentDocumentSupporterForTest : CommonShipmentDocumentSupporter
	{
		public CommonShipmentDocumentSupporterForTest(CommonShipment shipment)
			: base(shipment)
		{
		}

		public new ICommonShipmentDocumentSupporterQueryProvider QueryProvider
		{
			get { return base.QueryProvider; }
		}

		public ContainerToSelectFromForPrintingCollection ContainersToSelectFrom(ZBool allContainers)
		{
			return base.GetContainersToSelectFrom(allContainers);
		}

		public void DocumentEventSource_DocumentPrintedTestMethod(object sender, DocumentPrintedEventArgs e)
		{
			base.DocumentEventSource_DocumentPrinted(sender, e);
		}

		public ContainerNonDependentCollection SelectedContainersToPrintTest
		{
			get { return base.SelectedContainersToPrint; }
		}

		public DocumentWrapper[] GetDocWrappersForCartageAdviceContextForTest(IStmMenuItem menu)
		{
			return base.GetDocWrappersForCartageAdviceContext(menu, Core.Constants.DataContext.CartageAdvice);
		}

		public delegate bool ContainerSelectionDelegateForTest(CommonContainer container);

		public DocumentWrapper[] GetDocWrappersForGenericFreightJobByContainerIfFCLForTest(DocumentDirection direction, ContainerSelectionDelegateForTest containerSelectionDelegate)
		{
			ContainerSelectionDelegate del = null;
			if (containerSelectionDelegate != null)
			{
				del = container => containerSelectionDelegate(container);
			}

			return GetDocWrappersForGenericFreightJobByContainerIfFCL(direction, del);
		}

		public bool IsCustomsDoc_Exposed(IStmMenuItem menu)
		{
			return IsCustomsDoc(menu);
		}

		public DocumentWrapper[] GetGenericWrapperForPacksFromDataContext(DataContext dataContext, bool returnSingleDocument)
		{
			return base.GetGenericWrapperForPacks(dataContext, returnSingleDocument);
		}

		public new DocumentWrapper[] GetGenericWrapperForPacksInRange(DataContext dataContext, bool returnSingleDocument)
		{
			return base.GetGenericWrapperForPacksInRange(dataContext, returnSingleDocument);
		}

		protected override ZString GetCartageAdviceDocumentDataStateMessage()
		{
			return "ERROR";
		}
	}
}
