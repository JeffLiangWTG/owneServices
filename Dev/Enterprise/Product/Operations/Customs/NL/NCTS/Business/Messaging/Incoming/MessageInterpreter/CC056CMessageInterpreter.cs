using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;

namespace Enterprise.Customs.NL.NCTS.Business;

public sealed class CC056CMessageInterpreter : IMessageInterpreter<ICC056CDataProvider>
{
	public string Interpret(ICC056CDataProvider dataProvider)
	{
		var note = new ZStringBuilder();

		var rejectionDscriptionCodeList = new CC056CRejectionDescriptions();
		var rejectionDescription = rejectionDscriptionCodeList.GetDescriptionFromCode(dataProvider.BusinessRejectionType);

		var rejectionCodeDescriptionList = new CC056CRejectionCodeDescriptions();
		var rejectionCodeDescription = rejectionCodeDescriptionList.GetDescriptionFromCode(dataProvider.RejectionCode);

		note.Append($"Declaration received an error for type {dataProvider.BusinessRejectionType} on {dataProvider.RejectionDateAndTime.ToString("dd-MM-yyyy HH:mm:ss")} ({translateUnkownValue(rejectionDescription)})");
		note.Append($"Reason: {dataProvider.RejectionCode} {dataProvider.RejectionReason} ({translateUnkownValue(rejectionCodeDescription)})");

		var errorCodeDescriptionList = new CC056CErrorCodeDescriptions();

		foreach (var functionalError in dataProvider.FunctionalErrors)
		{
			var errorCode = errorCodeDescriptionList.GetDescriptionFromCode(functionalError.ErrorCode);

			note.Append($"Functional error code: {functionalError.ErrorCode} ({translateUnkownValue(errorCode)})");
			note.Append($"Reason: {functionalError.ErrorReason}");
			note.Append($"Attribute: {functionalError.ErrorPointer}");
			note.Append($"Element in declaration contains now the value: {functionalError.OriginalAttributeValue}");
			note.Append("");
		}

		return note.ToStringWithDelimiterBetweenAppends(NL.Business.Common.NLConstants.HtmlContent.Break);
	}

	string translateUnkownValue(string value) => value.IsNullOrEmpty() ? "-UNKOWN-" : value;
}
