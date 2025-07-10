using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.OfficeCodes.Business
{
	public class OfficeCodeRoleProvider : IOfficeCodeRole
	{
		public OfficeCodeRoleProvider(IEnumerable<XElement> equalRolesWitTransportModes)
		{
			this.equalRolesWitTransportModes = equalRolesWitTransportModes;
		}
		readonly IEnumerable<XElement> equalRolesWitTransportModes;

		public string Name => equalRolesWitTransportModes.FirstOrDefault().GetValueFromDefaultElementWithAttribute(Constants.OfficeCodes.RoleAttributeValue);

		public IEnumerable<string> TransportModes
		{
			get
			{
				if (transportModes == null)
				{
					transportModes = equalRolesWitTransportModes.Elements(Constants.OfficeCodes.DefaultElementName)
						.Where(x => x.Attribute("name").Value == Constants.OfficeCodes.TrafficTypeAttributeValue)
						.Select(x => SourceXmlConverterHelper.GetTransportMode(x.Value))
						.Where(x => !string.IsNullOrEmpty(x))
						.Distinct()
						.ToArray();
				}
				return transportModes;
			}
		}
		string[] transportModes;
	}
}
