using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.OfficeCodes.Business
{
	public class RDEntryListCustomsOfficesParser
	{
		public RDEntryListCustomsOfficesParser(StringBuilder errorBuilder)
		{
			this.errorBuilder = errorBuilder;
		}
		readonly StringBuilder errorBuilder;

		public IEnumerable<RefCusCodeList> ParseXML(XDocument sourceXML)
		{
			var refCusCodeLists = Enumerable.Empty<RefCusCodeList>();
			if (sourceXML != null)
			{
				var officeCodeData = ExtractOfficeCodeData(sourceXML);
				refCusCodeLists = PopulateRefCusCodeList(officeCodeData);
			}
			return refCusCodeLists;
		}

		IEnumerable<OfficeCodeDataProvider> ExtractOfficeCodeData(XDocument sourceXML)
		{
			var rootElement = sourceXML.GetCustomsOfficesRootElement();
			var customsOfficeElements = rootElement.Descendants(Constants.OfficeCodes.CustomsOfficeElementName);
			var result = new List<OfficeCodeDataProvider>();
			foreach (var customsOffice in customsOfficeElements)
			{
				var officeCodeData = new OfficeCodeDataProvider(customsOffice);
				if (ValidateData(officeCodeData))
				{
					result.Add(officeCodeData);
				}
				else
				{
					AppendInvalidDataErrorDetails(customsOffice, officeCodeData);
				}
			}
			return result;
		}

		static IEnumerable<RefCusCodeList> PopulateRefCusCodeList(IEnumerable<OfficeCodeDataProvider> officeCodeData)
		{
			var result = new List<RefCusCodeList>();
			foreach (var officeCode in officeCodeData)
			{
				var attributes = new List<RefCusCodeListAttribute>();
				AddAttribute(attributes, officeCode.City, "CITY");
				AddAttribute(attributes, officeCode.PostalCode, "PostCode");
				AddAttribute(attributes, officeCode.StreetAndNumber, "Street");
				AddAttribute(attributes, officeCode.EMailAddress, "EmailAddress");

				foreach (var role in officeCode.Roles)
				{
					attributes.Add(new RefCusCodeListAttribute
					{
						ZZE_ZXE_NKName = "ROLE",
						ZZE_Value = role.Name,
						RefCusCodeOrAttributeTransportModes = role.TransportModes.Select(x => new RefCusCodeOrAttributeTransportMode { ZZU_TransportMode = x }).ToArray()
					});
				}

				result.Add(new RefCusCodeList
				{
					ZZD_Code = officeCode.ReferenceNumber,
					ZZD_ZZZ_NKDataGrouping = officeCode.CountryCode,
					ZZD_Description = officeCode.UsualName,
					ZZD_StartDate = officeCode.StartDate,
					ZZD_EndDate = officeCode.EndDate,
					RefCusCodeListAttributes = attributes.ToArray()
				});
			}
			return result;
		}

		void AppendInvalidDataErrorDetails(XElement invalidXMLElement, IOfficeCode invalidOfficeCodeData)
		{
			errorBuilder.AppendLine("Unable to import CustomsOffice-record due to missing ReferenceNumber, CountryCode, UsualName, Role or invalid Start-/EndDate.");
			errorBuilder.AppendLine("DETAILS:");
			errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"ReferenceNumber: {invalidOfficeCodeData.ReferenceNumber}");
			errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"CountryCode: {invalidOfficeCodeData.CountryCode}");
			errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"UsualName: {invalidOfficeCodeData.UsualName}");
			errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"StartDate: {invalidXMLElement.GetDateValueFromCustomsOfficeElement(Constants.OfficeCodes.StartDateElement)}");
			errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"EndDate: {invalidXMLElement.GetDateValueFromCustomsOfficeElement(Constants.OfficeCodes.EndDateElement)}");
			errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Roles count: {invalidOfficeCodeData.Roles.Count()}");
		}

		static bool ValidateData(IOfficeCode officeCodeData) =>
			!string.IsNullOrEmpty(officeCodeData.ReferenceNumber)
			&& !string.IsNullOrEmpty(officeCodeData.CountryCode)
			&& !string.IsNullOrEmpty(officeCodeData.UsualName)
			&& officeCodeData.StartDateSuccessfullyParsed
			&& officeCodeData.EndDateSuccessfullyParsed
			&& officeCodeData.Roles.Any();

		static void AddAttribute(List<RefCusCodeListAttribute> attributes, string value, string name)
		{
			if (!string.IsNullOrWhiteSpace(value))
			{
				attributes.Add(new RefCusCodeListAttribute
				{
					ZZE_ZXE_NKName = name,
					ZZE_Value = value
				});
			}
		}
	}
}
