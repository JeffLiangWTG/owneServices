using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC051CMessageInterpreter : IMessageInterpreter<ICC051CDataProvider>
{
	public string Interpret(ICC051CDataProvider dataProvider)
	{
		var note = new ZStringBuilder();
		var motivationCode = dataProvider.NoReleaseMotivationCode ?? string.Empty;
		note.Append((NoResString)"Declaration is NOT RELEASED FOR TRANSIT AT DEPARTURE.");
		note.Append(ZString.Format((NoResString)"Motivation code: {0} {1}", motivationCode, new NCTS5NoReleaseMotivation().GetDescriptionFromCode(motivationCode)));
		if (!string.IsNullOrEmpty(dataProvider.NoReleaseMotivationText))
		{
			note.Append(dataProvider.NoReleaseMotivationText);
		}

		return note.ToStringWithDelimiterBetweenAppends(NL.Business.Common.NLConstants.HtmlContent.Break);
	}
}
