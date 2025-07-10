using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.NCTS.Business;

public sealed class CC140CMessageInterpreter : IMessageInterpreter<ICC140CDataProvider>
{
	public string Interpret(ICC140CDataProvider dataProvider)
	{
		const string format = "dd/MM/yyyy";
		var note = new ZStringBuilder();
		note.Append((NoResString)"New Customs Status: 'Request on Non-Arrived Movement'");
		note.Append((NoResString)"Status granted on " + dataProvider.RequestOnNonArrivedMovementDate?.ToString(format));
		note.Append((NoResString)"Response on the Request for info on Non-Arrived Movement, must be sent to customs before " + dataProvider.LimitForResponseDate?.ToString(format));

		return note.ToStringWithDelimiterBetweenAppends(NL.Business.Common.NLConstants.HtmlContent.Break);
	}
}
