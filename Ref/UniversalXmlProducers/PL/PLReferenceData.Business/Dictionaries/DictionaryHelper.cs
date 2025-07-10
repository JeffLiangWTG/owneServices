using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries.DictionaryPuesc;
using CargoWise.RefDbRepo.PLReferenceData.Business.Helpers;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries
{
	public static class DictionaryHelper
	{
		public static class RefDataConstants
		{
			public const string Export = "EXPORT";
			public const string Import = "IMPORT";
			public const string Direction = "Direction";
			public const string DefaultDescription = "N/A";
		}

		public enum RefDataType
		{
			RefCusCodeList,
			RefCusCodeListWithAttributeExport,
			RefCusCodeListWithAttributeImport,
			RefCusCodeListWithLanguage,
			RefCusCodeListMerged,
		}

		public static XmlWriterConfiguration GetXmlWriterConfiguration(this RefDataType type)
		{
			switch (type)
			{
				case RefDataType.RefCusCodeListWithAttributeExport:
				case RefDataType.RefCusCodeListWithAttributeImport:
					return XmlWriterConfig.GetRefCusCodeListWithAttributeWriterConfiguration();
				case RefDataType.RefCusCodeListWithLanguage:
					return XmlWriterConfig.GetRefCusCodeListWithLanguageWriterConfiguration();
				default:
					return XmlWriterConfig.GetSimpleRefCusCodeListWriterConfiguration();
			}
		}

		public static IRefCusCodeListMapper GetPuescRefCusCodeListMapper(this RefDataType type)
		{
			switch (type)
			{
				case RefDataType.RefCusCodeListWithAttributeExport:
					return new RefCusCodeListWithAttributeExportMapper();
				case RefDataType.RefCusCodeListWithAttributeImport:
					return new RefCusCodeListWithAttributeImportMapper();
				case RefDataType.RefCusCodeListWithLanguage:
					return new RefCusCodeListWithLanguageMapper();
				case RefDataType.RefCusCodeListMerged:
					return new MergedSimpleRefCusCodeListMapper();
				default:
					return new SimpleRefCusCodeListMapper();
			}
		}

		public static DateTime GetDataTime(DateTime? dateTime, bool isStartDate)
		{
			if (dateTime == null)
			{
				return isStartDate ? DictionariesConstants.DefaultStartDateDateTime : DictionariesConstants.DefaultEndDateDateTime;
			}
			if (dateTime < DictionariesConstants.DefaultStartDateDateTime)
			{
				return DictionariesConstants.DefaultStartDateDateTime;
			}
			if (dateTime > DictionariesConstants.DefaultEndDateDateTime)
			{
				return DictionariesConstants.DefaultEndDateDateTime;
			}
			return (DateTime)dateTime;
		}

		public static string TruncateString(string str, int maxLen)
		{
			return string.IsNullOrEmpty(str) ? str : str.Substring(0, Math.Min(str.Length, maxLen));
		}
	}
}
