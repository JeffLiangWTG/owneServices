using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.Environment;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer
{
	public class QuotationValueObjectDataAdapter : RatingValueObjectDataAdapter<Quote>
	{
		#region Properties

		protected override ZString ExpectedRateType
		{
			get { return RatingConstants.RatingHeaderTypes.Quote; }
		}

		protected override bool ShouldCheckifOwnerIsSpecified
		{
			get { return true; }
		}

		#endregion

		#region Validation

		protected override bool Validate(Xsd.Rate rateXSD, ZString humanReadableName, IValueObjectImportContext context)
		{
			bool result = base.Validate(rateXSD, humanReadableName, context);

			if (result)
			{
				if (rateXSD.Quote.StartDate.IsEmpty)
				{
					context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("f69f4462-00c6-4f93-94bc-561d431c7beb", "Quotation Start Date is not Provided")));
					result = false;
				}
			}
			return result;
		}

		#endregion

		#region Helpers

		protected override Quote[] LoadExistingBusinessObject(Xsd.Rate rateXSD, IValueObjectImportContext context)
		{
			Quote[] result = System.Array.Empty<Quote>();

			if (rateXSD.Quote != null && !rateXSD.Quote.Number.IsEmpty)
			{
				result = base.LoadExistingBusinessObject(rateXSD, context);
			}

			return result;
		}

		protected override ZQuery QueryForFindingBusinessObject(Xsd.Rate rateXSD, IValueObjectImportContext context)
		{
			ZQuery query = base.QueryForFindingBusinessObject(rateXSD, context);
			query.AddToFilter(RatingHeaderSchema.TH_QuoteNumber, rateXSD.Quote.Number);
			query.AddToFilter(RatingHeaderSchema.TH_OneTimeQuote, false);

			return query;
		}

		#endregion

		#region Import

		protected override void ImportRatingHeader(Quote ratingHeader, Xsd.Rate rateXSD, IValueObjectImportContext context)
		{
			base.ImportRatingHeader(ratingHeader, rateXSD, context);

			ImportClientDetails(ratingHeader, rateXSD, context);

			if (rateXSD.Quote.IsSpecified)
			{
				ratingHeader.TH_QuoteDate = (ZDate)rateXSD.Quote.StartDate;
				ratingHeader.TH_QuoteNumber = rateXSD.Quote.Number;

				if (!rateXSD.Quote.EndDate.IsEmpty)
				{
					ratingHeader.TH_QuoteEndDate = (ZDate)rateXSD.Quote.EndDate;
				}
				else if (Env.Registry.Rating.QuoteEndDateMandatory)
				{
					ratingHeader.TH_QuoteEndDate = ratingHeader.DefaultQuoteEndDate;
				}

				ratingHeader.TH_Accepted = (rateXSD.Quote.AcceptedDateTime.IsValid) ? rateXSD.Quote.AcceptedDateTime : ZDateTime.Empty;

				if (rateXSD.CFX.ImportSeaSpecified)
				{
					context.SetPropertyInfoValue(ratingHeader.TH_SeaCFXInfo, rateXSD.CFX.ImportSea.ToString(), rateXSD.CFX.ImportSeaSpecified);
				}

				if (rateXSD.CFX.ImportAirSpecified)
				{
					context.SetPropertyInfoValue(ratingHeader.TH_AirCFXInfo, rateXSD.CFX.ImportAir.ToString(), rateXSD.CFX.ImportAirSpecified);
				}

				if (rateXSD.CFX.ExportAirSpecified)
				{
					context.SetPropertyInfoValue(ratingHeader.TH_ExportAirCFXInfo, rateXSD.CFX.ExportAir.ToString(), rateXSD.CFX.ExportAirSpecified);
				}

				if (rateXSD.CFX.ExportSeaSpecified)
				{
					context.SetPropertyInfoValue(ratingHeader.TH_ExportSeaCFXInfo, rateXSD.CFX.ExportSea.ToString(), rateXSD.CFX.ExportSeaSpecified);
				}
			}
		}

		#endregion

		#region Export

		protected override void ExportRatingHeader(Quote ratingHeader, Xsd.Rate rateXSD, IValueObjectExportContext context)
		{
			base.ExportRatingHeader(ratingHeader, rateXSD, context);

			ExportClientDetails(ratingHeader, rateXSD, context);

			rateXSD.Quote.IsSpecified = true;
			rateXSD.Quote.StartDate = ratingHeader.TH_QuoteDate;

			rateXSD.Quote.EndDate = ratingHeader.TH_QuoteEndDate;
			rateXSD.Quote.AcceptedDateTime = ratingHeader.TH_Accepted;
			rateXSD.Quote.Number = ratingHeader.TH_QuoteNumber;

			if (!ratingHeader.TH_SeaCFX.IsEmpty)
			{
				rateXSD.CFX.ImportSea = ratingHeader.TH_SeaCFX;
			}

			if (!ratingHeader.TH_AirCFX.IsEmpty)
			{
				rateXSD.CFX.ImportAir = ratingHeader.TH_AirCFX;
			}

			if (!ratingHeader.TH_ExportAirCFX.IsEmpty)
			{
				rateXSD.CFX.ExportAir = ratingHeader.TH_ExportAirCFX;
			}

			if (!ratingHeader.TH_ExportSeaCFX.IsEmpty)
			{
				rateXSD.CFX.ExportSea = ratingHeader.TH_ExportSeaCFX;
			}
		}

		protected override void PostProcessExportedRateEntry(Quote ratingHeader, RateEntry entry, Xsd.RateEntry rateEntryXSD, IValueObjectExportContext context)
		{
			// Required entry according to the schema definition, but not needed for quotes.
			if (rateEntryXSD.StartDate.IsEmpty)
			{
				rateEntryXSD.StartDate = ratingHeader.TH_QuoteDate;
			}
		}

		#endregion
	}
}

