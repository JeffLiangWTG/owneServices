namespace Enterprise.MasterFiles.Business
{
	public interface IExcelPasswordRetrieverForDocumentDelivery
	{
		string ExcelPasswordForOpening { get; }

		string ExcelPasswordForModifying { get; }
	}
}
