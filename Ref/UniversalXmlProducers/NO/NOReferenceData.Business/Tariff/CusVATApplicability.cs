using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Avgiftliste;

namespace CargoWise.RefDbRepo.NOReferenceData.Business.Tariff
{
	public static class CusVATApplicability
	{
		public static RefCusVATApplicability[] GetRefCusVATApplicabilities(AvgiftListe fees, string tariffId)
		{
			var result = new List<RefCusVATApplicability>();
			var taxes = (from goods in fees?.Goods ?? Array.Empty<Vare>()
						 where goods.id == tariffId
						 from dutyRate in goods.DutyRates
						 from dutyTypes in dutyRate.DutyTypes
						 where dutyTypes.DutyType == Constants.Types.VAT
						 orderby dutyTypes.DutyType, dutyRate.CountryGroup
						 select new
						 {
							 dutyTypes.DutyType,
							 dutyTypes.DutyTypeDescription,
							 grpDetails = (from dutyGroups in dutyTypes.DutyGroups
										   from dutyGroup in dutyTypes.DutyGroups
										   orderby dutyGroup.DutyGroup
										   select new
										   {
											   dutyGroup.DateStart,
											   dutyGroup.DateEnd,
											   dutyGroup.DutyGroup,
											   dutyGroup.DutyGroupDescription,
											   taxOrFeeCode = string.Concat(Constants.Types.VAT, dutyGroup.DutyGroup)
										   })
						 });
			foreach (var lines in taxes.Distinct())
			{
				foreach (var details in lines.grpDetails.Distinct())
				{
					var cusVATApplicability = ConvertRefCusVATApplicability(details.DateStart, details.DateEnd, details.DutyGroupDescription, details.taxOrFeeCode);
					if (cusVATApplicability != null)
					{
						result.Add(cusVATApplicability);
					}
				}
			}

			return result.ToArray();
		}

		public static RefCusVATApplicability ConvertRefCusVATApplicability(string startDate, string endDate, string description, string taxOrFeeCode)
		{
			var (startDateOk, startDateDtm) = startDate.TryParseDateTime();
			var (endDateOk, endDateDtm) = endDate.TryParseEndDateTime();

			if (startDateOk && endDateOk && startDateDtm <= endDateDtm && !string.IsNullOrEmpty(taxOrFeeCode) && !string.IsNullOrEmpty(description))
			{
				return new RefCusVATApplicability
				{
					ZX5_Description = DataHelpers.CleanHtmlStringIfApplicable(description),
					ZX5_StartDate = startDateDtm,
					ZX5_EndDate = endDateDtm,
					ZX5_ZZF_NKTaxOrFeeCode = taxOrFeeCode
				};
			}

			TariffParser.ErrorBuilder.AppendLine("Unable to parse VAT code due to empty code, empty description or invalid Dates.");
			TariffParser.ErrorBuilder.AppendLine("DETAILS:");
			TariffParser.ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "VAT code: {0}", taxOrFeeCode).AppendLine();
			TariffParser.ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Description: {0}", description).AppendLine();
			TariffParser.ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Start Date: {0}", startDate).AppendLine();
			TariffParser.ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "End Date: {0}", endDate).AppendLine();
			return null;
		}
	}
}
