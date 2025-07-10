using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	static class OrgHeaderExtensions
	{
		public static ZString GetRegistrationNumber(this OrgHeader org, ZString countryCode, ZString type)
		{
			var orgCusCode = org.GetRegistrationNumberObject(countryCode, type);

			return orgCusCode != null
				? orgCusCode.OK_CustomsRegNo
				: ZString.Empty;
		}

		public static OrgCusCode GetRegistrationNumberObject(this OrgHeader org, ZString countryCode, ZString type)
		{
			return org != null
				? org.CustomsCodes.OfType<OrgCusCode>()
					.FirstOrDefault(c => (countryCode.IsEmpty || c.OK_RN_NKCodeCountry == countryCode) && c.OK_CodeType == type)
				: null;
		}

		public static RefShippingLineMessagingRequirement GetShippingLineMessagingRequirement(this OrgHeader org, ZString shippingLineMessagingRequirementType)
		{
			return org?.ShippingLine?.ShippingLineMessagingRequirements?.FirstOrDefault(x => x.RSR_RST_NKType == shippingLineMessagingRequirementType);
		}
	}
}
