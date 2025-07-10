using System.Collections.Generic;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.PL.Business;
using static Enterprise.Customs.PL.NCTS.Business.Constants.InterpretationStrings;

namespace Enterprise.Customs.PL.NCTS.Business;

sealed class CC060CMessageInterpreter(NctsCommonMovementHeader movementHeader) : NctsMessageInterpreterBase<IIE060>(movementHeader)
{
	protected override bool UseExtendedGlobalStyle => true;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Html strings")]
	protected override void InterpretCore(IIE060 dataProvider, HtmlBlockWriter htmlWriter)
	{
		htmlWriter.WriteHeader(caption: MessageTitles.IE060);
		htmlWriter.WriteThematicBreak();

		htmlWriter.WriteParamValueTable(@class: CommonStrings.NoBorderBoldFont,
			paramValues: new ParamValueCollection {
				{ CommonStrings.LRN, ": " + dataProvider.LRN },
				{ CommonStrings.MRN, ": " + dataProvider.MRN },
				{ TransitOperation.MessageSentOn, ": " + dataProvider.PreparationDateAndTime },
				{ TransitOperation.CustomsOfficeOfDeparture, ": " + MessageInterpreterHelper.GetOfficeCodeWithDescription(Factory, dataProvider.CustomsOfficeOfDeparture) },
				{ TransitOperation.ControlNotificationDateAndTime, ": " + dataProvider.TransitOperation?.ControlNotificationDateAndTime },
				{ TransitOperation.NotificationType, ": " + new NCTS5NotificationTypes().GetDescriptionFromCode(dataProvider.TransitOperation?.NotificationType ?? string.Empty) },
			});
		htmlWriter.WriteThematicBreak();

		if (dataProvider.TypeOfControls is { Count: > 0 })
		{
			htmlWriter.WriteParamValueTable(@class: "fixed-table",
				caption: TypeofControl.Caption,
				paramValues: GetTypeOfControl(dataProvider.TypeOfControls));
			htmlWriter.WriteThematicBreak();
		}

		if (dataProvider.RequestedDocuments is { Count: > 0 })
		{
			htmlWriter.WriteParamValueTable(@class: "fixed-table",
				caption: RequestedDocuments.Caption,
				paramValues: GetRequestedDocuments(dataProvider.RequestedDocuments));
			htmlWriter.WriteThematicBreak();
		}

		if (dataProvider.Representative != null)
		{
			htmlWriter.WriteParamValueSequence(
				caption: Representative.Caption,
				paramValues: GetRepresentative(dataProvider.Representative));
			htmlWriter.WriteThematicBreak();
		}

		htmlWriter.WriteParamValueSequence(
			caption: HolderOfTheTransitProcedure.Caption,
			paramValues: new HolderOfTheTransitProcedureInterpreter(dataProvider.HolderOfTheTransitProcedure).GetRows(NctsHeader));
		htmlWriter.WriteThematicBreak();
	}

	IEnumerable<IParamValue> GetRepresentative(IRepresentative representative)
	{
		if (representative.IdentificationNumber != null)
		{
			yield return new ParamValue(Representative.EORI, representative.IdentificationNumber);
		}
		if (representative.Status != null)
		{
			var status = representative.Status == "2"
				? $"{representative.Status} : {Representative.Status2Description}"
				: representative.Status;
			yield return new ParamValue(Representative.Status, status);
		}
	}

	IEnumerable<IParamValue> GetTypeOfControl(IReadOnlyCollection<ITypeOfControls> typeOfControls)
	{
		yield return new ParamValue(TypeofControl.Code, TypeofControl.Description);

		foreach (var typeOfControl in typeOfControls)
		{
			var code = typeOfControl.Type;
			var description = Factory.GetTypeOfControlDescription(code);
			yield return new ParamValue(code, description + "\n" + typeOfControl.Text);
		}
	}

	IEnumerable<IParamValue> GetRequestedDocuments(IReadOnlyCollection<IRequestedDocument> requestedDocuments)
	{
		yield return new ParamValue(RequestedDocuments.DocumentType, RequestedDocuments.Description);

		foreach (var document in requestedDocuments)
		{
			yield return new ParamValue(document.DocumentType, document.Description);
		}
	}
}
