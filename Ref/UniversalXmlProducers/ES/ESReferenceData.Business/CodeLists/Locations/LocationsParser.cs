using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.ESReferenceData.Services;

namespace CargoWise.RefDbRepo.ESReferenceData.Business
{
	public class LocationsParser : CommonParser
	{
		public LocationsParser(IDateTimeProvider dateTimeProvider) : base(dateTimeProvider) { }

		public string ConvertRecordsToXMLFile(IEnumerable<ILocationsItem> locationsToExport, string outPutFileWithPath)
		{
			ErrorBuilder.Clear();
			var result = GetRefCodeListToExport(locationsToExport);
			if (result.Count > 0)
			{
				Helper.ExportToXMLFile(DataSource, outPutFileWithPath, XMLWriterConfiguration, DateTimeProvider.CurrentLocalDate, result);
			}
			else
			{
				ErrorBuilder.AppendLine(ErrorMessage);
			}

			return ErrorBuilder.ToString();
		}

		List<RefCusCodeList> GetRefCodeListToExport(IEnumerable<ILocationsItem> locationsToExport)
		{
			var codesToImport = new HashSet<string>();
			var result = new List<RefCusCodeList>();
			var actualDate = DateTime.UtcNow.Date;
			foreach (var locationsItem in locationsToExport)
			{
				var location = locationsItem.Location;
				var name = locationsItem.Name;
				var startDate = Helper.GetDateTime(locationsItem.StartDate, LocationsDateFormat);
				var endDate = Helper.GetDateTime(locationsItem.EndDate, LocationsDateFormat);
				if (CheckDataIsValid(
					location,
					name,
					locationsItem.StartDate, startDate,
					locationsItem.EndDate, endDate,
					actualDate))
				{
					if (actualDate.CompareTo(endDate.DateTime) < 0 && !codesToImport.Contains(location))
					{
						AddToRefList(result, location, name, startDate.DateTime, endDate.DateTime);
						codesToImport.Add(location);
					}
				}
			}
			return result;
		}

		bool CheckDataIsValid(
			string location,
			string name,
			string startDateString, (bool SuccessfullyParsed, DateTime ParsedValue) startDate,
			string endDateString, (bool SuccessfullyParsed, DateTime ParsedValue) endDate,
			DateTime actualDate)
		{
			var result = true;
			if (!string.IsNullOrEmpty(location)
				&& string.IsNullOrEmpty(name) &&
				startDate.SuccessfullyParsed && endDate.SuccessfullyParsed &&
				endDate.ParsedValue < actualDate)
			{
				// Only Name is missing, and the record is going to be excluded because its EndDate is in the past.
				// Skipping error logging.
				result = false;
			}
			else if (string.IsNullOrEmpty(location) || string.IsNullOrEmpty(name) || !startDate.SuccessfullyParsed || !endDate.SuccessfullyParsed)
			{
				ErrorBuilder.AppendLine("Unable to import Location as there is a missing or wrong attribute 'location', 'name', 'start date' or 'end date'. Details:");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Location: {location}");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Name: {name}");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Start Date: {startDateString}");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"End Date: {endDateString}");
				result = false;
			}

			return result;
		}

		static void AddToRefList(List<RefCusCodeList> result, string location, string name, DateTime startDate, DateTime endDate)
		{
			result.Add(new RefCusCodeList
			{
				ZZD_Code = location,
				ZZD_Description = name,
				ZZD_StartDate = startDate,
				ZZD_EndDate = endDate
			});
		}

		static string DataSource => Constants.DataSources.Location_Codes;

		static string ErrorMessage => "Unable to locate any records for Locations. The provider may not be returning data to process.";

		protected override XmlWriterConfiguration XMLWriterConfiguration
		{
			get
			{
				var writerConfiguration = new XmlWriterConfiguration();
				var locationsConfiguration = new EntityTypeConfiguration<RefCusCodeList>(true);
				locationsConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, "LOC");
				locationsConfiguration.IncludeColumn(x => x.ZZD_Code, true);
				locationsConfiguration.IncludeColumn(x => x.ZZD_Description, false);
				locationsConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, Constants.MinimumDateTime);
				locationsConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, Constants.MaximumDateTime);
				locationsConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Constants.CountryCode);
				writerConfiguration.IncludeEntityTypeConfiguration(locationsConfiguration);
				return writerConfiguration;
			}
		}

		const string LocationsDateFormat = "dd-MM-yyyy";
	}
}
