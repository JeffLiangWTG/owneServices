using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.NOReferenceData.Services.TradeGroups.LandGruppe;
using CargoWise.RefDbRepo.NOReferenceData.Services.TradeGroups.MedlemsLand;

namespace CargoWise.RefDbRepo.NOReferenceData.Business.TradeGroups
{
	public static class TradeGroupsParser
	{
		public static string ConvertToXMLFile(LandgruppeListe countryXmlData, MedlemLandListe memberXmlData, string countryXmlLastModified, string memberXmlLastModified, string outputFileWithPath)
		{
			_ = countryXmlData ?? throw new ArgumentNullException(nameof(countryXmlData));
			_ = memberXmlData ?? throw new ArgumentNullException(nameof(memberXmlData));
			ErrorBuilder.Clear();
			var writerConfiguration = GetRefTradeGroupsWriterConfiguration();

			var resultTradeGroup = new List<RefCusTradeGroup>();
			foreach (var tradeGroup in from countryGroup in countryXmlData.CountryGroup
									   select new
									   {
										   countryGroup.CountryGroupCode,
										   countryGroup.CountryGroupName,
										   countryGroup.DateStart,
										   countryGroup.DateEnd
									   })
			{
				var (validGroup, cusTradeGroupZZ) = ConvertTradeGroup(tradeGroup.CountryGroupCode, tradeGroup.CountryGroupName, tradeGroup.DateStart, tradeGroup.DateEnd);
				if (validGroup)
				{
					var cusTradeGroupMembers = new List<RefCusTradeGroupCountry>();
					foreach (var members in (from countries in memberXmlData.Member
											 from groups in countries.CountryGroups
											 let filterResult = FilterTradeGroup(groups, tradeGroup.CountryGroupCode)
											 where filterResult.IsValid
											 select new
											 {
												 countries.CountryCode,
												 filterResult.DateStart,
												 filterResult.DateEnd
											 }).ToHashSet())
					{
						var (validMember, cusTradeGroupCountryZZ) = ConvertTradeGroupMember(members.CountryCode, members.DateStart, members.DateEnd, tradeGroup.CountryGroupCode);
						if (validMember)
						{
							cusTradeGroupMembers.Add(cusTradeGroupCountryZZ);
						}
					}

					cusTradeGroupZZ.RefCusTradeGroupCountries = cusTradeGroupMembers.ToArray();
					resultTradeGroup.Add(cusTradeGroupZZ);
				}
			}

			if (resultTradeGroup.Any())
			{
				var modifiedDateTime = FindLatestValidTimestamp(countryXmlLastModified, memberXmlLastModified);
				FileHelper.ExportToXMLFile("NO TradeGroups", outputFileWithPath, writerConfiguration, modifiedDateTime, resultTradeGroup);
			}

			return ErrorBuilder.ToString();
		}

		static (bool Valid, RefCusTradeGroup CusTradeGroupZZ) ConvertTradeGroup(string tradeGroup, string description, string startDate, string endDate)
		{
			var (isValidDates, startDateParsed, endDateParsed) = DataHelpers.IsValidDates(startDate, endDate);
			if (!string.IsNullOrEmpty(tradeGroup) && !string.IsNullOrEmpty(description) && isValidDates)
			{
				return (true, new RefCusTradeGroup
				{
					ZZA_TradeGroup = tradeGroup,
					ZZA_Description = description,
					ZZA_StartDate = startDateParsed,
					ZZA_EndDate = endDateParsed,
				});
			}

			ErrorBuilder.AppendLine("Unable to parse TradeGroup due to empty code, empty description or invalid Dates.");
			ErrorBuilder.AppendLine("DETAILS:");
			ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Trade Group: {0}", tradeGroup).AppendLine();
			ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Description: {0}", description).AppendLine();
			ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Start Date: {0}", startDate).AppendLine();
			ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "End Date: {0}", endDate).AppendLine();
			return (false, null);
		}

		static (bool ValidMember, RefCusTradeGroupCountry CusTradeGroupCountryZZ) ConvertTradeGroupMember(string tradeGroupCountry, string startDate, string endDate, string tradeGroup)
		{
			var (isValidDates, startDateParsed, endDateParsed) = DataHelpers.IsValidDates(startDate, endDate);
			if (!string.IsNullOrEmpty(tradeGroupCountry) && isValidDates)
			{
				return (true, new RefCusTradeGroupCountry
				{
					ZZB_RN_NKTradeGroupCountryCode = tradeGroupCountry,
					ZZB_StartDate = startDateParsed,
					ZZB_EndDate = endDateParsed
				});
			}

			ErrorBuilder.AppendLine("Unable to parse TradeGroup Member due to empty code, empty description or invalid Dates.");
			ErrorBuilder.AppendLine("DETAILS:");
			ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Trade Group: {0}", tradeGroup).AppendLine();
			ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Member: {0}", tradeGroupCountry).AppendLine();
			ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Start Date: {0}", startDate).AppendLine();
			ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "End Date: {0}", endDate).AppendLine();
			return (false, null);
		}

		static DateTime FindLatestValidTimestamp(string groupLastModified, string memberLastModified)
		{
			var (groupDateTimeOk, groupDateTime) = groupLastModified.TryParseDateTime("MM/dd/yyyy HH:mm:ss");
			if (!groupDateTimeOk)
			{
				ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Failed to parse lastUpdated DateTime {0} from Tradegroups file", groupLastModified).AppendLine();
			}
			var (memberDateTimeOk, memberDateTime) = memberLastModified.TryParseDateTime("MM/dd/yyyy HH:mm:ss");
			if (!memberDateTimeOk)
			{
				ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Failed to parse lastUpdated DateTime {0} from Tradegroup Members file", memberLastModified).AppendLine();
			}

			var result = DateTime.Now;
			if (groupDateTimeOk && memberDateTimeOk)
			{
				result = DateTime.Compare(groupDateTime, memberDateTime) > 0 ? groupDateTime : memberDateTime;
			}
			return result;
		}

		static XmlWriterConfiguration GetRefTradeGroupsWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var tradeGroupsConfiguration = new EntityTypeConfiguration<RefCusTradeGroup>(true);
			tradeGroupsConfiguration.IncludeColumnWithConstantValue(x => x.ZZA_ZZZ_NKDataGrouping, isKeyColumn: true, Constants.CountryCodes.Norway);
			tradeGroupsConfiguration.IncludeColumn(x => x.ZZA_TradeGroup, isKeyColumn: true);
			tradeGroupsConfiguration.IncludeColumn(x => x.ZZA_Description, isKeyColumn: false);
			tradeGroupsConfiguration.IncludeColumn(x => x.ZZA_StartDate, isKeyColumn: false);
			tradeGroupsConfiguration.IncludeColumn(x => x.ZZA_EndDate, isKeyColumn: false);
			tradeGroupsConfiguration.IncludeColumn(x => x.RefCusTradeGroupCountries);
			writerConfiguration.IncludeEntityTypeConfiguration(tradeGroupsConfiguration);

			var tradeGroupsCountryConfiguration = new EntityTypeConfiguration<RefCusTradeGroupCountry>(true);
			tradeGroupsCountryConfiguration.IncludeColumn(x => x.ZZB_RN_NKTradeGroupCountryCode, isKeyColumn: true);
			tradeGroupsCountryConfiguration.IncludeColumn(x => x.ZZB_StartDate, isKeyColumn: false);
			tradeGroupsCountryConfiguration.IncludeColumn(x => x.ZZB_EndDate, isKeyColumn: false);
			writerConfiguration.IncludeEntityTypeConfiguration(tradeGroupsCountryConfiguration);

			return writerConfiguration;
		}

		static (bool IsValid, string DateStart, string DateEnd) FilterTradeGroup(Services.TradeGroups.MedlemsLand.Landgruppe groups, string tradeGroupCode)
		{
			if (groups.CountryGroupCode == tradeGroupCode)
			{
				return (true, groups.DateStart, groups.DateEnd);
			}

			if (tradeGroupCode == Constants.TradeGroupCodes.OrdinaryCustoms)
			{
				return (true, Constants.MinimumDateTime.ToString(Constants.YearMonthDateFormat), Constants.MaximumDateTime.ToString(Constants.YearMonthDateFormat));
			}

			return (false, string.Empty, string.Empty);
		}

		static StringBuilder ErrorBuilder => errorBuilder ?? (errorBuilder = new StringBuilder());
		static StringBuilder errorBuilder;
	}
}
