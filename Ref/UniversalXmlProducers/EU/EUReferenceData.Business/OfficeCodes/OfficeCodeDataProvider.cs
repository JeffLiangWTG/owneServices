using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.OfficeCodes.Business
{
	public class OfficeCodeDataProvider : IOfficeCode
	{
		public OfficeCodeDataProvider(XElement customsOffice)
		{
			if (customsOffice != null)
			{
				this.customsOffice = customsOffice;
				SetDateTime(Constants.OfficeCodes.StartDateElement, ref startDateDetails);
				SetDateTime(Constants.OfficeCodes.EndDateElement, ref endDateDetails);
			}
			else
			{
				throw new ArgumentNullException(nameof(customsOffice));
			}
		}
		readonly XElement customsOffice;
		(bool SuccessfullyParsed, DateTime DateTime) startDateDetails;
		(bool SuccessfullyParsed, DateTime DateTime) endDateDetails;

		public string ReferenceNumber => customsOffice.GetValueFromDefaultElementWithAttribute(Constants.OfficeCodes.ReferenceNumberAttributeValue);

		public string CountryCode => customsOffice.GetValueFromDefaultElementWithAttribute(Constants.OfficeCodes.CountryCodeAttributeValue);

		public DateTime StartDate => startDateDetails.DateTime;

		public bool StartDateSuccessfullyParsed => startDateDetails.SuccessfullyParsed;

		public DateTime EndDate => endDateDetails.DateTime;

		public bool EndDateSuccessfullyParsed => endDateDetails.SuccessfullyParsed;

		public string PostalCode => customsOffice.GetValueFromDefaultElementWithAttribute(Constants.OfficeCodes.PostalCodeAttributeValue);

		public string EMailAddress => customsOffice.GetValueFromDefaultElementWithAttribute(Constants.OfficeCodes.EMailAddressAttributeValue);

		public string UsualName => LanguageElement.GetValueFromDefaultElementWithAttribute(Constants.OfficeCodes.UsualNameAttributeValue);

		public string City => LanguageElement.GetValueFromDefaultElementWithAttribute(Constants.OfficeCodes.CityAttributeValue);

		public string StreetAndNumber => LanguageElement.GetValueFromDefaultElementWithAttribute(Constants.OfficeCodes.StreetAndNumberAttributeValue);

		public IEnumerable<IOfficeCodeRole> Roles
		{
			get
			{
				if (roles == null)
				{
					roles = new List<IOfficeCodeRole>();
					var roleParents = customsOffice.Descendants(Constants.OfficeCodes.RoleParentElementName).Where(x => x.Attribute("name").Value == Constants.OfficeCodes.RoleParentAttributeValue).ToArray();
					var distinctRoles = roleParents.Elements(Constants.OfficeCodes.DefaultElementName)
						.Where(x => x.Attribute("name").Value == Constants.OfficeCodes.RoleAttributeValue)
						.Select(x => x.Value)
						.Distinct();

					foreach (var role in distinctRoles)
					{
						roles.Add(new OfficeCodeRoleProvider(roleParents.Where(x => x.GetValueFromDefaultElementWithAttribute(Constants.OfficeCodes.RoleAttributeValue) == role)));
					}
				}
				return roles;
			}
		}
		List<IOfficeCodeRole> roles;

		XElement LanguageElement
		{
			get
			{
				if (languageElementCached == null)
				{
					var languages = customsOffice.Descendants(Constants.OfficeCodes.LanguagesElementName).Where(x => x.Attribute("name").Value == Constants.OfficeCodes.LanguagesAttributeValue).ToArray();
					languageElementCached = languages.FirstOrDefault(x => x.GetValueFromDefaultElementWithAttribute(Constants.OfficeCodes.LanguageCodeAttributeValue) == Constants.Common.DefaultLanguage);
					if (languageElementCached == null)
					{
						var languageFromCountry = SourceXmlConverterHelper.GetLanguageOfCountry(CountryCode);
						languageElementCached = languages.FirstOrDefault(x => x.GetValueFromDefaultElementWithAttribute(Constants.OfficeCodes.LanguageCodeAttributeValue) == languageFromCountry);
						if (languageElementCached == null)
						{
							languageElementCached = languages.FirstOrDefault();
						}
					}
				}
				return languageElementCached;
			}
		}
		XElement languageElementCached;

		void SetDateTime(XName elementName, ref (bool SuccessfullyParsed, DateTime DateTime) propertyToSet)
		{
			var dateString = customsOffice.GetDateValueFromCustomsOfficeElement(elementName);
			if (string.IsNullOrWhiteSpace(dateString))
			{
				if (elementName == Constants.OfficeCodes.StartDateElement)
				{
					propertyToSet.DateTime = Constants.MinimumDateTime;
				}
				else if (elementName == Constants.OfficeCodes.EndDateElement)
				{
					propertyToSet.DateTime = Constants.MaximumDateTime;
				}
				propertyToSet.SuccessfullyParsed = true;
			}
			else
			{
				propertyToSet = Utils.GetDateTimeFromString(dateString, Constants.Common.SourceDateFormatXML);
			}
		}
	}
}
