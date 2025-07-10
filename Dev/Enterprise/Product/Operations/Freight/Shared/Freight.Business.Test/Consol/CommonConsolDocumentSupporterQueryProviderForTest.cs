namespace Enterprise.Freight.Business.Testing
{
	public class CommonConsolDocumentSupporterQueryProviderForTest : ICommonConsolDocumentSupporterQueryProvider
	{
		bool ICommonConsolDocumentSupporterQueryProvider.ConfirmBOLPrinting(CommonShipment shipment)
		{
			return true;
		}

		DocumentCommonConsol ICommonConsolDocumentSupporterQueryProvider.GetConsolToPrint(DocumentCommonConsol documentConsol)
		{
			return null;
		}

		ContainersToPrintOptions ICommonConsolDocumentSupporterQueryProvider.GetContainersToPrint(ContainerToSelectFromForPrintingCollection containersToSelectFrom, bool includeUnContainerised)
		{
			return null;
		}
	}
}
