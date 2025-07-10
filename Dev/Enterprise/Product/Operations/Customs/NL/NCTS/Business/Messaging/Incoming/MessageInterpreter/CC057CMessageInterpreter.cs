using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public sealed class CC057CMessageInterpreter : IMessageInterpreter<ICC057CDataProvider>
{
	public string Interpret(ICC057CDataProvider dataProvider)
	{
		var messageTypeCodes = new NLNctsOutgoingMessageTypes();
		var rejectionCodes = new RejectionCodes();
		var rejectionType = dataProvider.BusinessRejectionType;
		var rejectionCode = dataProvider.RejectionCode;
		var note = new ZStringBuilder();
		note.Append($"Declaration received an error for type {rejectionType} ({messageTypeCodes.GetDescriptionFromCode(rejectionType)}) on {dataProvider.RejectionDateAndTime.ToString("dd/MM/yyyy hh:mm:ss")}");
		note.Append($"Reason: {rejectionCode} ({rejectionCodes.GetDescriptionFromCode(rejectionCode)}) {dataProvider.RejectionReason}");

		var functionalErrorCodes = new FunctionalErrorCodes();
		foreach (var functionalErrorCode in dataProvider.FunctionalErrors)
		{
			var errorCode = functionalErrorCode.ErrorCode;
			note.Append($"Functional error code: {errorCode} ({functionalErrorCodes.GetDescriptionFromCode(errorCode)})");
			note.Append($"Reason: {functionalErrorCode.ErrorReason}");
			note.Append($"Attribute: {functionalErrorCode.ErrorPointer}");
			note.Append($"Element in declaration contains now the value: {functionalErrorCode.OriginalAttributeValue}");
			note.Append(ZString.Empty);
		}

		return note.ToStringWithDelimiterBetweenAppends(NL.Business.Common.NLConstants.HtmlContent.Break);
	}
}
