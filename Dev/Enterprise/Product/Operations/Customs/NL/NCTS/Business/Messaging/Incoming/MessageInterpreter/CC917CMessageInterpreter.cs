using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC917CMessageInterpreter : IMessageInterpreter<ICC917CDataProvider>
{
	public string Interpret(ICC917CDataProvider dataProvider)
	{
		var note = new ZStringBuilder();
		var xmlErrorCodes = new CC917CXmlErrorCodeDescriptions();
		note.Append((NoResString)"New transaction status: XML error. Xml gives xsd errors.");
		note.Append(ZString.Empty);

		foreach (var xmlError in dataProvider.XmlErrors)
		{
			note.Append(ZString.Format((NoResString)"Error line number: {0}", xmlError.SequenceNumeric));
			note.Append(ZString.Format((NoResString)"Error column number: {0}", xmlError.ErrorColumnNumber));
			note.Append(ZString.Format((NoResString)"Error pointer: {0}", xmlError.ErrorPointer));
			note.Append(ZString.Format((NoResString)"Error code: {0} {1}", xmlError.ErrorCode, xmlErrorCodes.GetDescriptionFromCode(xmlError.ErrorCode)));
			note.Append(ZString.Format((NoResString)"Error text: {0}", xmlError.ErrorReason));
			note.Append(ZString.Format((NoResString)"Original attribute value: {0}", xmlError.OriginalAttributeValue));
			note.Append(ZString.Empty);
		}

		return note.ToStringWithDelimiterBetweenAppends(NLConstants.HtmlContent.Break);
	}
}
