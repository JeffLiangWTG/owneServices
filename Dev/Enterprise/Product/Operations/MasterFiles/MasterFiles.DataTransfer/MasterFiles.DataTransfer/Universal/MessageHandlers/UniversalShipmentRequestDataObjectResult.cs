using Enterprise.UniversalDataBuss.Integration;

public class UniversalShipmentRequestDataObjectResult
{
	public ITopLevelDataObject DataObject { get; set; }
	public bool Success { get; set; }
	public IDataWritingManager WritingManager { get; set; }

	public UniversalShipmentRequestDataObjectResult(ITopLevelDataObject dataObject, bool success, IDataWritingManager writingManager)
	{
		DataObject = dataObject;
		Success = success;
		WritingManager = writingManager;
	}
}
