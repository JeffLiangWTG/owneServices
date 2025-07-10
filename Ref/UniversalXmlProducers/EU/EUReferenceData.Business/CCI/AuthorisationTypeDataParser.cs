using System;
using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class AuthorisationTypeDataParser : CommonDataParser
	{
		public AuthorisationTypeDataParser() :
			base(Constants.AuthorisationType.RDEntityAttributeValue,
				Constants.AuthorisationType.RDEntityAttributeValue)
		{
		}

		protected override void SetRefCusCode(RefCusCodeList refCusCodeList, XElement entity)
		{
			base.SetRefCusCode(refCusCodeList, entity);

			var descriptions = entity.Descendants(Constants.Common.Description);
			var defaultDescription = descriptions.SingleOrDefault(descriptionElement =>
			{
				var language = descriptionElement.Attribute(Constants.Common.DescriptionAttributeValue)?.Value;
				return string.Equals(Constants.Common.DefaultLanguage, language, StringComparison.OrdinalIgnoreCase);
			});

			if (defaultDescription != null)
			{
				var descriptionValue = defaultDescription.Value;
				var descriptionParts = descriptionValue.Split('-');

				if (descriptionParts.Length > 1)
				{
					var codePart = descriptionParts[0].Trim();
					if (codePart.Length <= 4)
					{
						refCusCodeList.ZZD_Code = codePart;
					}
				}
			}
		}

		protected override string PreProcessDescription(string description, string code)
		{
			var prefix = $"{code} -";
			if (description.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
			{
				return description.Remove(0, prefix.Length).Trim();
			}

			return description;
		}
	}
}
