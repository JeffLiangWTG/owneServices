using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Quotations;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class TrackingQuotationsColumnProvider : GridColumnProvider
	{
		public const string ViewQuoteCommand = "ViewQuote";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			ZBindToChecker.CheckBindTo((ZString)((TrackingQuote)null).TH_QuoteNumber);
			ZBindToChecker.CheckBindTo((ZPropertyInfo)((TrackingQuote)null).TH_QuoteNumberInfo);
			ZLinkButtonColumn quoteNumberColumn = new ZLinkButtonColumn(Res.GetString("b1cb35f3-b3cd-4b1f-90db-b63c78e0bbe5", "Quote #"), RatingHeaderSchema.Constants.TH_QuoteNumber) { ColumnKey = WebTracker.Grids.TrackingQuotations.QuoteNumber };
			quoteNumberColumn.Command = ViewQuoteCommand;
			AddToDictionaryAsRequired(quoteNumberColumn);

			ZBindToChecker.CheckBindTo((ZString)((TrackingQuote)null).CompanyName);
			ZBindToChecker.CheckBindTo((ZPropertyInfo)((TrackingQuote)null).CompanyNameInfo);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("954780e3-a512-4749-901d-3607ad6762e8", "Company"), TrackingQuote.Constants.CompanyName) { ColumnKey = WebTracker.Grids.TrackingQuotations.Company });

			ZBindToChecker.CheckBindTo((ZString)((TrackingQuote)null).QuoteStatus);
			ZBindToChecker.CheckBindTo((ZPropertyInfo)((TrackingQuote)null).QuoteStatusInfo);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("85e5a70d-a3cb-4cb4-a0c3-26b1b1691d18", "Status"), "QuoteStatus") { ColumnKey = WebTracker.Grids.TrackingQuotations.QuoteStatus });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingQuote)null).TH_QuoteDate);
			ZBindToChecker.CheckBindTo((ZPropertyInfo)((TrackingQuote)null).TH_QuoteDateInfo);
			ZDateTimeColumn quoteDateColumn = new ZDateTimeColumn(Res.GetString("41207630-66f8-4c94-b9aa-cf24291ef9ea", "Quote Date"), RatingHeaderSchema.Constants.TH_QuoteDate) { ColumnKey = WebTracker.Grids.TrackingQuotations.QuoteDate };
			quoteDateColumn.DateTimeFormat = ZDateTimePickerFormat.Short;
			AddToDictionaryAsDefault(quoteDateColumn);

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingQuote)null).TH_QuoteEndDate);
			ZBindToChecker.CheckBindTo((ZPropertyInfo)((TrackingQuote)null).TH_QuoteEndDateInfo);
			ZDateTimeColumn expiryDateColumn = new ZDateTimeColumn(Res.GetString("490b8ba5-6d76-45d8-abfc-c71b522259bf", "Expiry Date"), RatingHeaderSchema.Constants.TH_QuoteEndDate) { ColumnKey = WebTracker.Grids.TrackingQuotations.ExpiryDate };
			expiryDateColumn.DateTimeFormat = ZDateTimePickerFormat.Short;
			AddToDictionaryAsDefault(expiryDateColumn);

			ZBindToChecker.CheckBindTo((ZString)((TrackingQuote)null).TransportMode);
			ZBindToChecker.CheckBindTo((ZPropertyInfo)((TrackingQuote)null).TransportModeInfo);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((TrackingQuote)null).TransportModes);
			ZDropDownListColumn transportModeColumn =
				new ZDropDownListColumn(
					Res.GetString("9285417a-bba3-417f-ba46-773b67e0953a", "Transport Mode"),
					TrackingQuote.Constants.TransportMode, TrackingQuote.Constants.TransportModes)
				{ ColumnKey = WebTracker.Grids.TrackingQuotations.TransportMode };
			transportModeColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			AddToDictionary(transportModeColumn);

			ZBindToChecker.CheckBindTo((ZString)((TrackingQuote)null).Origin);
			ZBindToChecker.CheckBindTo((ZPropertyInfo)((TrackingQuote)null).OriginInfo);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((TrackingQuote)null).Origins);

			ZCodeFindBoxColumn originColumn =
				new ZCodeFindBoxColumn(
					Res.GetString("cc6613ab-dbdd-408a-b3a7-81f61f49a45b", "Origin"),
					TrackingQuote.Constants.Origin,
					TrackingQuote.Constants.Origins,
					typeof(TrackingQuote))
				{ ColumnKey = WebTracker.Grids.TrackingQuotations.Origin };
			originColumn.SortExpression = TrackingQuote.Constants.Origin;
			originColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			AddToDictionaryAsDefault(originColumn);

			ZBindToChecker.CheckBindTo((ZString)((TrackingQuote)null).Destination);
			ZBindToChecker.CheckBindTo((ZPropertyInfo)((TrackingQuote)null).DestinationInfo);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((TrackingQuote)null).Destinations);
			ZCodeFindBoxColumn destinationColumn =
				new ZCodeFindBoxColumn(
					Res.GetString("1933271d-843b-4e05-8091-c3dbf9600ef7", "Destination"),
					TrackingQuote.Constants.Destination,
					TrackingQuote.Constants.Destinations,
					typeof(TrackingQuote))
				{ ColumnKey = WebTracker.Grids.TrackingQuotations.Destination };
			originColumn.SortExpression = TrackingQuote.Constants.Destination;
			destinationColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			AddToDictionaryAsDefault(destinationColumn);

			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingQuote)null).Volume);
			ZBindToChecker.CheckBindTo((ZPropertyInfo)((TrackingQuote)null).VolumeInfo);
			AddToDictionary(new ZCalcEditColumn(Res.GetString("d8464f83-2c5f-4b2d-8f8e-bfd6718a2829", "Volume"), TrackingQuote.Constants.Volume) { ColumnKey = WebTracker.Grids.TrackingQuotations.Volume, Decimals = 3 });

			ZBindToChecker.CheckBindTo((ZString)((TrackingQuote)null).VolumeUnits);
			ZBindToChecker.CheckBindTo((ZPropertyInfo)((TrackingQuote)null).VolumeUnitsInfo);
			AddToDictionary(new ZTextEditColumn(Res.GetString("8503626c-9a70-4043-898f-9c12d3e9c4d3", "Vol. Units"), TrackingQuote.Constants.VolumeUnits) { ColumnKey = WebTracker.Grids.TrackingQuotations.VolumeUnit });

			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingQuote)null).Weight);
			ZBindToChecker.CheckBindTo((ZPropertyInfo)((TrackingQuote)null).WeightInfo);
			AddToDictionary(new ZCalcEditColumn(Res.GetString("ce9ccf93-8392-4824-b6e6-f0a36a8e8cb3", "Weight"), TrackingQuote.Constants.Weight) { ColumnKey = WebTracker.Grids.TrackingQuotations.Weight, Decimals = 3 });

			ZBindToChecker.CheckBindTo((ZString)((TrackingQuote)null).WeightUnits);
			ZBindToChecker.CheckBindTo((ZPropertyInfo)((TrackingQuote)null).WeightUnitsInfo);
			AddToDictionary(new ZTextEditColumn(Res.GetString("d757260a-ea45-461e-8d0c-0d71359fb834", "Weight Units"), TrackingQuote.Constants.WeightUnits) { ColumnKey = WebTracker.Grids.TrackingQuotations.WeightUnit });
		}

		protected override List<int> GetOldColumnsOrder()
		{
			List<int> result = new List<int>();
			result.Add((int)WebTracker.Grids.TrackingQuotations.QuoteNumber);
			result.Add((int)WebTracker.Grids.TrackingQuotations.Company);
			result.Add((int)WebTracker.Grids.TrackingQuotations.QuoteStatus);
			result.Add((int)WebTracker.Grids.TrackingQuotations.QuoteDate);
			result.Add((int)WebTracker.Grids.TrackingQuotations.ExpiryDate);
			result.Add((int)WebTracker.Grids.TrackingQuotations.TransportMode);
			result.Add((int)WebTracker.Grids.TrackingQuotations.Origin);
			result.Add((int)WebTracker.Grids.TrackingQuotations.Destination);
			result.Add((int)WebTracker.Grids.TrackingQuotations.Volume);
			result.Add((int)WebTracker.Grids.TrackingQuotations.VolumeUnit);
			result.Add((int)WebTracker.Grids.TrackingQuotations.Weight);
			result.Add((int)WebTracker.Grids.TrackingQuotations.WeightUnit);
			return result;
		}
	}
}
