using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	internal class AgencyShipmentDocAddressValidation : ShipmentDocAddressValidation
	{
		public AgencyShipmentDocAddressValidation(JobDocAddress addressToValidate)
			: base(addressToValidate)
		{
		}

		protected void MandatoryAddress(ZString description)
		{
			if (!Parent.IsValidAddress)
			{
				Parent.OrganisationPKInfo.AddError(Res.GetString("41290690-0863-4328-9937-665d5b3c91a2", "Please enter a {0}", description));
			}
		}
	}
}
