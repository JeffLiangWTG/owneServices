using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class OrganisationToUseForAviationSecurity
	{
		public OrganisationToUseForAviationSecurity(string organisationCode, string validationCode)
		{
			Argument.NotNull(organisationCode, "organisationCode");
			Argument.NotNull(validationCode, "validationCode");

			OrganisationCode = organisationCode;
			ValidationCode = validationCode;
		}

		#region Properties

		public string OrganisationCode { get; private set; }

		public string ValidationCode { get; private set; }

		public ZString OrganisationDescription
		{
			get { return SupplyChainSecurityOrganisationTypes.GetDescription(OrganisationCode); }
		}

		#endregion
	}
}
