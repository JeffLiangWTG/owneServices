using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class InvoiceLineJobDocAddressValidation : JobDocAddressValidation
	{
		public InvoiceLineJobDocAddressValidation(AutoJobDocAddress parent)
			: base(parent)
		{
		}

		protected override void CheckE2_Address1()
		{
			if (Parent.E2_AddressOverride)
			{
				if (!Parent.E2_Address1.IsEmpty)
				{
					ABICharactersValidator.ValidateCharacters(Parent.E2_Address1Info, Parent.E2_Address1, true, "Address 1");
				}
			}
		}

		protected override void CheckE2_OA_Address()
		{
			base.CheckE2_OA_Address();
			if (!Parent.E2_AddressOverride && Parent.Address != null)
			{
				OrganisationValidation.ValidateCharactorsForAddressDescription(Parent.E2_OA_AddressInfo, Parent.Address);
			}
		}
	}
}
