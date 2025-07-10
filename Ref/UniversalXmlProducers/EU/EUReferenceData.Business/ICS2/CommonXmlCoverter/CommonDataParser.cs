using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.Services;

namespace CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business
{
	public class CommonDataParser
	{
		public CommonDataParser(string rdEntityAttributeValue, string dataItemAttributeValue)
		{
			fRDEntityAttributeValue = rdEntityAttributeValue;
			fDataItemAttributeValue = dataItemAttributeValue;
			fAdditionalTranslationsProvider = this is IAdditionalTranslationSupporter additionalTranslationSupporter
				? new AdditionalTranslationsProvider(new AdditionalTranslationsReader(ApplicationConfig.Instance.RefCusCodeListAdditionalTranslationsPath), additionalTranslationSupporter)
				: null;
		}

		public virtual IEnumerable<RefCusCodeList> ParseToRefCusCodeLists(XDocument sourceXml)
		{
			errorStr = string.Empty;

			var result = Enumerable.Empty<RefCusCodeList>();
			if (sourceXml != null)
			{
				var entities = ValidationElementAndGetRDEntry(sourceXml);
				var refCusCodeLists = PopulateRefCusCodeList(entities);

				if (string.IsNullOrWhiteSpace(errorStr))
				{
					result = refCusCodeLists;
				}
			}
			else
			{
				errorStr = "XML file not found!";
			}
			return result;
		}

		protected virtual IEnumerable<XElement> ValidationElementAndGetRDEntry(XDocument sourceXml)
		{
			var result = Enumerable.Empty<XElement>();

			var entityElement = Utils.FindXElementByAttribute(sourceXml, Constants.Common.RDEntity, Constants.Common.RDEntityAttributeName, fRDEntityAttributeValue);
			if (entityElement == null)
			{
				errorStr = $"Can not found RDEntity element name is {fRDEntityAttributeValue}!";
				return result;
			}

			result = entityElement.Descendants(Constants.Common.RDEntry).Where(x => string.Equals(Utils.FindXElementValueFirstOne(x, Constants.Common.State), Constants.Common.Valid, StringComparison.Ordinal));

			if (!result.Any())
			{
				errorStr = "This file is invalid!";
			}

			return result;
		}

		protected virtual IEnumerable<RefCusCodeList> PopulateRefCusCodeList(IEnumerable<XElement> entities)
		{
			foreach (var entity in entities)
			{
				var refCusCodeList = new RefCusCodeList();
				SetRefCusCode(refCusCodeList, entity);
				SetRefCusCodeListLanguages(refCusCodeList, entity);
				SetCusCodeListAttributes(refCusCodeList, entity);
				yield return refCusCodeList;
			}
		}

		protected virtual void SetRefCusCodeListLanguages(RefCusCodeList refCusCodeList, XElement entity)
		{
			var refCusCodeListLanguages = new List<RefCusCodeListLanguage>();

			var descriptions = entity.Descendants(Constants.Common.Description);
			var code = refCusCodeList.ZZD_Code;
			foreach (var description in descriptions)
			{
				var language = description.Attribute(Constants.Common.DescriptionAttributeValue)?.Value;
				if (string.Equals(Constants.Common.DefaultLanguage, language, StringComparison.OrdinalIgnoreCase))
				{
					refCusCodeList.ZZD_Description = PreProcessDescription(description.Value, code);
					continue;
				}

				var descriptionStr = description.Value;
				refCusCodeListLanguages.Add(new RefCusCodeListLanguage
				{
					ZXA_Description = PreProcessDescription(descriptionStr, code),
					ZXA_ZX6_NKLanguage = language?.ToUpper(CultureInfo.InvariantCulture)
				});
			}

			if (string.IsNullOrEmpty(refCusCodeList.ZZD_Description))
			{
				refCusCodeList.ZZD_Description = code;
			}

			MergeAdditionalTranslations(refCusCodeList, refCusCodeListLanguages);

			refCusCodeList.RefCusCodeListLanguages = refCusCodeListLanguages.ToArray();
		}

		protected virtual string PreProcessDescription(string description, string code) => description;

		protected virtual void SetRefCusCode(RefCusCodeList refCusCodeList, XElement entity)
		{
			var activeFrom = Utils.FindXElementValueFirstOne(entity, Constants.Common.ActiveFrom);
			var (success, date) = activeFrom.GetDateTimeFromString(Constants.Common.SourceDateFormatXML);
			if (success)
			{
				refCusCodeList.ZZD_StartDate = date;
				refCusCodeList.ZZD_Code = Utils.FindXElementValueByAttribute(
					entity,
					Constants.Common.DataItem,
					Constants.Common.RDEntityAttributeName,
					fDataItemAttributeValue
				);
			}
		}

		protected virtual void SetCusCodeListAttributes(RefCusCodeList refCusCodeList, XElement entity)
		{
		}

		void MergeAdditionalTranslations(RefCusCodeList refCusCodeList, List<RefCusCodeListLanguage> refCusCodeListLanguages)
		{
			if (fAdditionalTranslationsProvider == null || this is not IAdditionalTranslationSupporter)
			{
				return;
			}

			foreach (var (language, description) in fAdditionalTranslationsProvider.GetTranslations(refCusCodeList.ZZD_Code))
			{
				var index = GetInsertIndex(refCusCodeListLanguages, language);

				if (index == -1)
				{
					continue;
				}

				refCusCodeListLanguages.Insert(index, new RefCusCodeListLanguage
				{
					ZXA_Description = PreProcessDescription(description, refCusCodeList.ZZD_Code),
					ZXA_ZX6_NKLanguage = language,
				});
			}
		}

		static int GetInsertIndex(List<RefCusCodeListLanguage> refCusCodeListLanguages, string language)
		{
			var index = 0;

			while (index < refCusCodeListLanguages.Count)
			{
				var compare = string.Compare(refCusCodeListLanguages[index].ZXA_ZX6_NKLanguage, language, StringComparison.Ordinal);
				if (compare == 0)
				{
					return -1;
				}
				else if (compare > 0)
				{
					break;
				}

				index++;
			}

			return index;
		}

		string errorStr;
		readonly string fRDEntityAttributeValue;
		readonly string fDataItemAttributeValue;
		readonly AdditionalTranslationsProvider fAdditionalTranslationsProvider;

		public string ErrorStr { get { return errorStr; } }
	}
}
