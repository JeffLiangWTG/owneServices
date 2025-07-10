using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	internal class BillOfLadingDocAddressValidation : AgencyShipmentDocAddressValidation
	{
		public BillOfLadingDocAddressValidation(JobDocAddress addressToValidate)
			: base(addressToValidate) { }

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();

			switch (Parent.E2_AddressType)
			{
				case DocAddressTypes.Codes.ConsignorDocumentaryAddress:
					MandatoryAddress(Res.GetString("d7bcd8b3-27f2-4664-bfd7-cb4848d085b2", "consignor"));
					break;

				case DocAddressTypes.Codes.ConsigneeDocumentaryAddress:
					MandatoryAddress(Res.GetString("bbac7368-d80f-4088-918d-e24b28ad9a01", "consignee"));
					break;
			}
		}
	}
}


