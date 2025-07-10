using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC906CMessageInterpreter : IMessageInterpreter<ICC906CDataProvider>
{
	public string Interpret(ICC906CDataProvider dataProvider)
	{
		var note = new ZStringBuilder();
		var errorCodes = new FunctionalErrorCodes();
		note.Append((NoResString)"New transaction status: XML error. Xml gives xsd errors.");
		note.Append(ZString.Empty);

		foreach (var functionalError in dataProvider.FunctionalErrors)
		{
			note.Append(ZString.Format((NoResString)"Error Pointer: {0}", functionalError.ErrorPointer));
			note.Append(ZString.Format((NoResString)"Error Code: {0} {1}", functionalError.ErrorCode, errorCodes.GetDescriptionFromCode(functionalError.ErrorCode)));
			note.Append(ZString.Format((NoResString)"Error Reason: {0}", functionalError.ErrorReason));
			note.Append(ZString.Format((NoResString)"Original Attribute Value: {0}", functionalError.OriginalAttributeValue));
			note.Append(ZString.Empty);
		}

		return note.ToStringWithDelimiterBetweenAppends(NL.Business.Common.NLConstants.HtmlContent.Break);
	}
}
