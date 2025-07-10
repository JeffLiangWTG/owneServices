using System.Collections.Specialized;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.PL.Business;

public sealed class NPPMessageInterpreter<TLinkedObject>(TLinkedObject linkedObject)
	: MessageInterpreterBase<TLinkedObject, IConfirmation>(linkedObject)
	where TLinkedObject : EnterpriseBusinessObject
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Html strings")]
	protected override ZString InterpretCore(IConfirmation upp)
	{
		var htmlBody = new ZStringBuilder();

		var tableCreator = new HtmlTableCreator(TableInterpretation.Attributes.NoBorder);
		tableCreator.WriteRowWithFormatting(new CellWithFormatting("Komunikat NPP (Poświadczenie Nieprzedłożenia Dokumentu)", GetCellAttributes("800"), true));
		htmlBody.Append(tableCreator.ToHtml());

		htmlBody.Append("<hr />");
		htmlBody.Append("<br />");

		tableCreator = new HtmlTableCreator(TableInterpretation.Attributes.NoBorder);
		if (!string.IsNullOrWhiteSpace(upp.ExternalSystemID))
		{
			AddTableRow("IdentyfikatorPoswiadczenia", " : " + upp.ExternalSystemID);
		}
		if (!string.IsNullOrWhiteSpace(upp.ReferenceToExternalSystemID))
		{
			AddTableRow("idDokumentuSEAP", " : " + upp.ReferenceToExternalSystemID);
		}
		tableCreator.WriteRowWithFormatting(new CellWithFormatting("Przyczyna błędu", GetCellAttributes("300"), true));
		htmlBody.Append(tableCreator.ToHtml());

		tableCreator = new HtmlTableCreator();
		tableCreator.WriteRowWithFormatting(new CellWithFormatting(upp.ErrorCause, GetCellAttributes("800")));
		htmlBody.Append(tableCreator.ToHtml());

		htmlBody.Append("<br />");
		htmlBody.Append("<hr />");

		return htmlBody.ToString();

		void AddTableRow(string caption, string value) => tableCreator.WriteRowWithFormatting(
			new CellWithFormatting(caption, GetCellAttributes("300"), true), new CellWithFormatting(value, GetCellAttributes("500")));

		NameValueCollection GetCellAttributes(string width) => new() { { "align", "left" }, { "width", width } };
	}
}
