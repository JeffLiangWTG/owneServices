using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	internal class AgencyBookingDocAddressValidation : AgencyShipmentDocAddressValidation
	{
		public AgencyBookingDocAddressValidation(JobDocAddress addressToValidate)
			: base(addressToValidate) { }

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();

			if (Parent.E2_AddressType == DocAddressTypes.Codes.BookingPartyDocumentaryAddress)
			{
				MandatoryAddress(Res.GetString("be75aa98-8db6-44b7-97b7-5f4de5e1c3fc", "booking party"));
			}
		}
	}
}


