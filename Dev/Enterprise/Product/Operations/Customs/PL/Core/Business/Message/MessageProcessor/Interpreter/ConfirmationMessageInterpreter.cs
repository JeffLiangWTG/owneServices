using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.PL.Business.Constants.InterpretationStrings;

namespace Enterprise.Customs.PL.Business;

public class ConfirmationMessageInterpreter<TLinkedObject>(TLinkedObject linkedObject)
	: MessageInterpreterBase<TLinkedObject, IConfirmation>(linkedObject)
	where TLinkedObject : EnterpriseBusinessObject
{
	protected override ZString InterpretCore(IConfirmation upp)
	{
		var note = new ZStringBuilder();
		note.Append(MessageTitles.UPP);
		if (!string.IsNullOrWhiteSpace(upp.ExternalSystemID))
		{
			note.Append("IdentyfikatorPoswiadczenia: " + upp.ExternalSystemID);
		}
		if (!string.IsNullOrWhiteSpace(upp.ReferenceToExternalSystemID))
		{
			note.Append("idDokumentuSEAP: " + upp.ReferenceToExternalSystemID);
		}

		return note.ToStringWithDelimiterBetweenAppends((NoResString)"</br>");
	}
}
