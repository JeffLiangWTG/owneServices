using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer
{
	public class CompanyTariffValueObjectDataAdapter : RatingValueObjectDataAdapter<CompanyTariff>
	{
		#region Properties

		const int BaseCompanyTariffLvl = 1;

		protected override ZString ExpectedRateType
		{
			get { return RatingConstants.RatingHeaderTypes.Tariff; }
		}

		protected override bool ShouldCheckifOwnerIsSpecified
		{
			get { return false; }
		}

		#endregion

		#region Validation

		protected override bool Validate(Xsd.Rate rateXSD, ZString humanReadableName, IValueObjectImportContext context)
		{
			bool result = base.Validate(rateXSD, humanReadableName, context);

			if (result)
			{
				if (rateXSD.CompanyTariff.Level.IsEmpty)
				{
					context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("72651190-b5de-4ac2-9ea9-80514a41f095", "Company Tariff Level is not Provided.")));
					result = false;
				}

				if (!rateXSD.CompanyTariff.Level.IsEmpty && rateXSD.CompanyTariff.Level != BaseCompanyTariffLvl)
				{
					context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("4e2afad1-3634-4b62-8aa9-0388b1b4b186", "Only Base Company Tariff(Level One) Import is Allowed.")));
					result = false;
				}
			}
			return result;
		}

		#endregion

		#region Helpers

		protected override ZQuery QueryForFindingBusinessObject(Xsd.Rate rateXSD, IValueObjectImportContext context)
		{
			ZQuery query = base.QueryForFindingBusinessObject(rateXSD, context);
			query.AddToFilter(RatingHeaderSchema.TH_GlobalRateLevel, (ZByte)BaseCompanyTariffLvl);

			return query;
		}

		#endregion

		#region Import

		protected override void ImportRatingHeader(CompanyTariff ratingHeader, Xsd.Rate rateXSD, IValueObjectImportContext context)
		{
			base.ImportRatingHeader(ratingHeader, rateXSD, context);

			context.SetPropertyInfoValueIfValueNotEmpty(ratingHeader.TH_GlobalRateDescriptionInfo, rateXSD.CompanyTariff.Description);
			ratingHeader.TH_GlobalRateLevel = Convert.ToByte(rateXSD.CompanyTariff.Level);
		}

		#endregion

		#region Export

		protected override void ExportRatingHeader(CompanyTariff ratingHeader, Xsd.Rate rateXSD, IValueObjectExportContext context)
		{
			base.ExportRatingHeader(ratingHeader, rateXSD, context);

			rateXSD.CompanyTariff.Description = ratingHeader.TH_GlobalRateDescriptionMultilingual;
			rateXSD.CompanyTariff.Level = ratingHeader.TH_GlobalRateLevel;
		}

		#endregion
	}
}

