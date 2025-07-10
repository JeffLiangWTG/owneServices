using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.NCTS.Business;

public sealed class CC060CMessageInterpreter : IMessageInterpreter<ICC060CDataProvider>
{
	public string Interpret(ICC060CDataProvider dataProvider)
	{
		var note = new ZStringBuilder();
		note.Append((NoResString)"New Customs Status: Decision to Control Notification");
		note.Append((NoResString)$"Status granted on: {dataProvider.ControlNotificationDateAndTime.ToString("dd/MM/yyyy HH:mm:ss")}");
		note.Append((NoResString)$"Type of Notification: {dataProvider.NotificationType} {new NCTS5NotificationTypes().GetDescriptionFromCode(dataProvider.NotificationType)}");
		note.Append(string.Empty);

		var typeOfControlTypes = new NCTS5TypeOfControlTypes();
		foreach (var typeOfControl in dataProvider.TypeOfControls)
		{
			note.Append((NoResString)$"Type of Control {typeOfControl.SequenceNumeric}: {typeOfControl.Type} {typeOfControlTypes.GetDescriptionFromCode(typeOfControl.Type)} {typeOfControl.Text}");
		}
		foreach (var requestedDocument in dataProvider.RequestedDocument)
		{
			note.Append((NoResString)$"Document {requestedDocument.SequenceNumeric}: {requestedDocument.DocumentType} {requestedDocument.Description}");
		}

		return note.ToStringWithDelimiterBetweenAppends(NL.Business.Common.NLConstants.HtmlContent.Break);
	}
}
