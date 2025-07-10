namespace Enterprise.Freight.Business
{
	public interface ICommonConsolDocumentSupporterQueryProvider
	{
		bool ConfirmBOLPrinting(CommonShipment shipment);
		DocumentCommonConsol GetConsolToPrint(DocumentCommonConsol documentConsol);
		ContainersToPrintOptions GetContainersToPrint(ContainerToSelectFromForPrintingCollection containersToSelectFrom, bool includeUnContainerised);
	}
}
