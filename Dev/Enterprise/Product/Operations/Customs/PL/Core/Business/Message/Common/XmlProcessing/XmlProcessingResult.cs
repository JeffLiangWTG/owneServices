namespace Enterprise.Customs.PL.Business;

public enum XmlProcessingResult
{
	Processed = 0,
	NotSupportedContent = 1,
	XmlParsingError = 2,
	AlreadyProcessed = 4,
	TransmitMessageNotFound = 5,
	MessageIdentificationMaxLengthExceeded = 6,
}
