using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.ESReferenceData.Business
{
	public class C44AddInfoParser : CodeListParser<C44AddInfoItem, C44AddInfoItemMap>
	{
		public C44AddInfoParser(IDateTimeProvider dateTimeProvider) : base(dateTimeProvider) { }

		protected override List<RefCusCodeList> GetRefCodeListToExport(string inputCsvFile)
		{
			var records = GetRecordsFromCsv(inputCsvFile);
			var result = new List<RefCusCodeList>();
			foreach (var record in records)
			{
				var code = record.Code;
				var description = record.Description;
				var specialIndication = record.SpecialIndication;
				var characterIndication = record.CharacterIndication;
				var startDate = Helper.GetDateTime(record.StartDate, C44AddInfoDateFormat);
				var endDate = Helper.GetDateTime(record.EndDate, C44AddInfoDateFormat);
				if (CheckDataIsValid(code, characterIndication, specialIndication, description, startDate.SuccessfullyParsed, record.StartDate, endDate.SuccessfullyParsed, record.EndDate))
				{
					AddToRefList(result, code, characterIndication, specialIndication, description, startDate.DateTime, endDate.DateTime);
				}
			}
			return result;
		}

		bool CheckDataIsValid(string code, string characterIndication, string specialIndication, string description, bool startDateSuccessfullyParsed, string startDateString, bool endDateSuccessfullyParsed, string endDateString)
		{
			var result = true;
			if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(characterIndication) || string.IsNullOrEmpty(specialIndication) || string.IsNullOrEmpty(description) || !startDateSuccessfullyParsed || !endDateSuccessfullyParsed)
			{
				ErrorBuilder.AppendLine("Unable to import C44 AddInfo due to empty 'code', 'special indication' 'description', 'start date', 'end date'. Details:");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Code: {code}");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Description: {description}");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Special Indication: {specialIndication}");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Character Indication: {characterIndication}");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Start Date: {startDateString}");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"End Date: {endDateString}");
				result = false;
			}
			return result;
		}

		static void AddToRefList(List<RefCusCodeList> result, string code, string characterIndication, string specialIndication, string description, DateTime startDate, DateTime endDate)
		{
			result.Add(new RefCusCodeList
			{
				ZZD_Code = code,
				ZZD_Description = string.Join(" - ", specialIndication, description).ToUpperInvariant(),
				ZZD_StartDate = startDate,
				ZZD_EndDate = endDate,
				RefCusCodeListAttributes = GetAttributes(characterIndication)
			});
		}

		static RefCusCodeListAttribute[] GetAttributes(string characterIndication)
		{
			var refCusCodeListsAttributes = new List<RefCusCodeListAttribute>();
			switch (characterIndication.ToUpper(CultureInfo.CurrentCulture))
			{
				case "IMPORTACION":
					refCusCodeListsAttributes.Add(new RefCusCodeListAttribute() { ZZE_ZXE_NKName = "Direction", ZZE_Value = "IMPORT" });
					break;
				case "EXPORTACION":
					refCusCodeListsAttributes.Add(new RefCusCodeListAttribute() { ZZE_ZXE_NKName = "Direction", ZZE_Value = "EXPORT" });
					break;
				default:
					refCusCodeListsAttributes.Add(new RefCusCodeListAttribute() { ZZE_ZXE_NKName = "Direction", ZZE_Value = "IMPORT" });
					refCusCodeListsAttributes.Add(new RefCusCodeListAttribute() { ZZE_ZXE_NKName = "Direction", ZZE_Value = "EXPORT" });
					break;
			}
			return refCusCodeListsAttributes.ToArray();
		}

		protected override string DataSource => Constants.DataSources.C44AddInfo_Codes;

		protected override string ErrorMessage => "Unable to locate any records for C44 AddInfo CSV. File may only contain header record";

		protected override XmlWriterConfiguration XMLWriterConfiguration
		{
			get
			{
				var writerConfiguration = new XmlWriterConfiguration();
				var codeListConfiguration = new EntityTypeConfiguration<RefCusCodeList>(true);
				codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, "ADDIN");
				codeListConfiguration.IncludeColumn(x => x.ZZD_Code, true);
				codeListConfiguration.IncludeColumn(x => x.ZZD_Description, false);
				codeListConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, Constants.MinimumDateTime);
				codeListConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, Constants.MaximumDateTime);
				codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Constants.CountryCode);
				codeListConfiguration.IncludeColumn(x => x.RefCusCodeListAttributes, false);
				writerConfiguration.IncludeEntityTypeConfiguration(codeListConfiguration);

				var codelistAttributeConfiguration = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
				codelistAttributeConfiguration.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
				codelistAttributeConfiguration.IncludeColumn(x => x.ZZE_Value, true);
				writerConfiguration.IncludeEntityTypeConfiguration(codelistAttributeConfiguration);

				return writerConfiguration;
			}
		}

		const string C44AddInfoDateFormat = "dd/MM/yyyy";
	}
}
