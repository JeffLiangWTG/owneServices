using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business;
using static Enterprise.Customs.PL.NCTS.Business.Constants;
using static Enterprise.Customs.PL.NCTS.Business.Constants.InterpretationStrings;

namespace Enterprise.Customs.PL.NCTS.Business;

sealed class CC025CMessageInterpreter(NctsCommonMovementHeader movementHeader) : NctsMessageInterpreterBase<IIE025>(movementHeader)
{
	protected override bool UseExtendedGlobalStyle => true;

	protected override void InterpretCore(IIE025 dataProvider, HtmlBlockWriter htmlWriter)
	{
		htmlWriter.WriteHeader(dataProvider.TransitOperation?.ReleaseIndicator switch
		{
			"1" => MessageTitles.IE025Full,
			"2" or "3" => MessageTitles.IE025Part,
			"4" => MessageTitles.IE025No,
			_ => string.Empty,
		});

		htmlWriter.WriteThematicBreak();

		var collection = new ParamValueCollection {
			{ CommonStrings.MRN, dataProvider.MRN },
			{ TransitOperation.MessageSentOn, dataProvider.PreparationDateAndTime },
			{ TransitOperation.ReleaseDate, dataProvider.TransitOperation?.ReleaseDate.ToShortDateString() },
			{ TransitOperation.ReleaseIndicator, GetReleaseIndicatorWithDescription(dataProvider.TransitOperation?.ReleaseIndicator) },
			{ TransitOperation.CustomsOfficeOfDestination, ArrivalMovementHeader.Factory.GetOfficeCodeWithDescription(dataProvider.CustomsOfficeOfDestinationActual) },
		};

		if (DestinationTraderIsRequired(dataProvider.TransitOperation?.ReleaseIndicator))
		{
			collection.Add(TransitOperation.DestinationTrader, dataProvider.TraderAtDestination);
		}

		htmlWriter.WriteParamValueTable(collection);
		htmlWriter.WriteThematicBreak();

		if (PartialReleaseIsRequired(dataProvider.TransitOperation?.ReleaseIndicator))
		{
			htmlWriter.WriteParamValueTable(@class: "fixed-table",
				caption: PartialRelease.Caption,
				paramValues: new ParamValueCollection(GetPartialRelease(dataProvider.Consignment)));
			htmlWriter.WriteThematicBreak();
		}
	}

	string GetReleaseIndicatorWithDescription(string releaseIndicator) => releaseIndicator != null
		? ArrivalMovementHeader.Factory.GetCodeWithDescription(RefCusCodeListType.CL164, releaseIndicator)
		: string.Empty;

	bool DestinationTraderIsRequired(string indicator) =>
		ReleaseType.ClosedFullRelease.Equals(indicator) || ReleaseType.DiscrepancyResolutionNoRelease.Equals(indicator);

	bool PartialReleaseIsRequired(string indicator) =>
		ReleaseType.DiscrepancyResolutionPartialRelease.Equals(indicator) || ReleaseType.ClosedPartialRelease.Equals(indicator);

	IEnumerable<IParamValue> GetPartialRelease(IReadOnlyCollection<ICC025CHouseConsignment> consignments)
	{
		yield return new ParamValue(PartialRelease.HouseSequenceNumber, PartialRelease.TotalItemsReleased);

		foreach (var paramValue in consignments?.SelectMany(houseConsignment =>
			houseConsignment.ConsignmentItem?.SelectMany(consignmentItem =>
				consignmentItem.Packaging?.Select(package =>
					new ParamValue(houseConsignment.SequenceNumber, $"{consignmentItem.GoodsItemNumber} = {package.NumberOfPackages}"))))
			?? [])
		{
			yield return paramValue;
		}
	}
}
