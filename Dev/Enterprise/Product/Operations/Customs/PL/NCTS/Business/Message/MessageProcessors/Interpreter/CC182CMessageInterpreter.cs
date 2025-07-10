using System.Collections.Generic;
using System.Text;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business;
using static Enterprise.Customs.PL.NCTS.Business.Constants.InterpretationStrings;

namespace Enterprise.Customs.PL.NCTS.Business;

sealed class CC182CMessageInterpreter(NctsCommonMovementHeader movementHeader) : NctsMessageInterpreterBase<IIE182>(movementHeader)
{
	protected override void InterpretCore(IIE182 dataProvider, HtmlBlockWriter htmlWriter)
	{
		htmlWriter.WriteHeader(MessageTitles.IE182);
		htmlWriter.WriteThematicBreak();

		htmlWriter.WriteParamValueTable(new ParamValueCollection {
			{ CommonStrings.MRN, dataProvider.MRN },
			{ TransitOperation.MessageSentOn, dataProvider.PreparationDateAndTime },
			{ TransitOperation.IncidentNotificationDateAndTime, dataProvider.IncidentNotificationDateAndTime },
			{ TransitOperation.CustomsOfficeOfDeparture, Factory.GetOfficeCodeWithDescription(dataProvider.CustomsOfficeOfDepartureReferenceNumber) },
			{ TransitOperation.CustomsOfficeOfIncidentRegistered, Factory.GetOfficeCodeWithDescription(dataProvider.CustomsOfficeOfIncidentRegistrationReferenceNumber) },
		});

		htmlWriter.WriteThematicBreak();

		var consignmentIncidents = dataProvider.ConsignmentIncidents;
		if (!consignmentIncidents.IsNullOrEmpty())
		{
			ZString[] titleColumns = [
				CommonStrings.SerialNumber,
				ConsignmentIncidentsString.IncidentLocationAndCoordinates,
				ConsignmentIncidentsString.IncidentCode,
				ConsignmentIncidentsString.AdditionalTextInfo,
			];

			htmlWriter.WriteParamValuesTableTranspose(
				paramValuesList: GetConsignmentIncidentsInformation(consignmentIncidents),
				titleColumns: titleColumns,
				rowIndex: false);
		}

		htmlWriter.WriteThematicBreak();
	}

	IEnumerable<ParamValues> GetConsignmentIncidentsInformation(IReadOnlyCollection<ICC182CConsignmentIncident> consignmentIncidents)
	{
		if (consignmentIncidents.IsNullOrEmpty())
		{
			yield break;
		}

		var index = 0;
		foreach (var consignmentIncident in consignmentIncidents)
		{
			var codeDetails = Factory.GetIncidentCodeWithDescription(consignmentIncident.Code);
			var location = consignmentIncident.Location;

			var locationDetailsBuilder = new StringBuilder();
			if (!string.IsNullOrEmpty(location.UNLocode))
			{
				locationDetailsBuilder.AppendLine(location.UNLocode);
			}
			if (!string.IsNullOrEmpty(location.Country))
			{
				locationDetailsBuilder.AppendLine($"Country Code: {location.Country}");
			}
			if (!string.IsNullOrEmpty(location.GNSS?.Latitude))
			{
				locationDetailsBuilder.AppendLine($"Latitude: {location.GNSS.Latitude}");
			}
			if (!string.IsNullOrEmpty(location.GNSS?.Longitude))
			{
				locationDetailsBuilder.AppendLine($"Longitude: {location.GNSS.Longitude}");
			}
			if (!string.IsNullOrEmpty(location.Address?.StreetAndNumber))
			{
				locationDetailsBuilder.AppendLine($"Street: {location.Address.StreetAndNumber}");
			}
			if (!string.IsNullOrEmpty(location.Address?.Postcode))
			{
				locationDetailsBuilder.AppendLine($"Postcode: {location.Address.Postcode}");
			}
			if (!string.IsNullOrEmpty(location.Address?.City))
			{
				locationDetailsBuilder.AppendLine($"City: {location.Address.City}");
			}

			++index;
			yield return new ParamValues(
				index.ToString(),
				[
					index.ToString(),
					locationDetailsBuilder.ToString().TrimEnd('\r', '\n'),
					codeDetails,
					consignmentIncident.Text,
				]);
		}
	}
}
