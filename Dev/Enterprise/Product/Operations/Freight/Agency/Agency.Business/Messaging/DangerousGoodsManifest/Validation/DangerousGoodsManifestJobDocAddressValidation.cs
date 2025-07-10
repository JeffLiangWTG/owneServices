using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class DangerousGoodsManifestJobDocAddressValidation : AutoJobDocAddressValidation
	{
		public DangerousGoodsManifestJobDocAddressValidation(AutoJobDocAddress parent) : base(parent)
		{
		}

		protected override void CheckE2_OA_Address()
		{
			base.CheckE2_OA_Address();

			if (Parent.E2_CompanyName.IsEmpty)
			{
				switch (Parent.E2_AddressType)
				{
					case DocAddressTypes.Codes.ConsigneeDocumentaryAddress:
						Parent.E2_OA_AddressInfo.AddMessageError(Res.GetString("0CB59FDC-E7CA-4877-A1F2-E429D65D2A3B", "Consignee Company Name is required"));
						break;

					case DocAddressTypes.Codes.ConsignorDocumentaryAddress:
						Parent.E2_OA_AddressInfo.AddMessageError(Res.GetString("313A588A-6ED7-4BA8-8A45-044220FE8D6B", "Consignor Company Name is required"));
						break;
				}
			}
		}

		protected override void CheckE2_Contact()
		{
			base.CheckE2_Contact();

			if (Parent.E2_Contact.IsEmpty)
			{
				switch (Parent.E2_AddressType)
				{
					case DocAddressTypes.Codes.ConsigneeDocumentaryAddress:
						Parent.E2_ContactInfo.AddMessageError(Res.GetString("584BE120-B907-4BA9-B760-A3F0070B5751", "Consignee Contact Name is required"));
						break;

					case DocAddressTypes.Codes.ConsignorDocumentaryAddress:
						Parent.E2_ContactInfo.AddMessageError(Res.GetString("AF239291-EC2D-4FF4-967B-DED33B516287", "Consignor Contact Name is required"));
						break;
				}
			}

			if (Parent.E2_Phone.IsEmpty)
			{
				switch (Parent.E2_AddressType)
				{
					case DocAddressTypes.Codes.ConsigneeDocumentaryAddress:
						Parent.E2_ContactInfo.AddMessageError(Res.GetString("F0407683-AD7D-4E6C-BD85-14D339951848", "Consignee Phone Number is required"));
						break;

					case DocAddressTypes.Codes.ConsignorDocumentaryAddress:
						Parent.E2_ContactInfo.AddMessageError(Res.GetString("587D8E23-8B8A-4632-9CEC-D9B2B1B2144E", "Consignor Phone Number is required"));
						break;
				}
			}
		}
	}
}
