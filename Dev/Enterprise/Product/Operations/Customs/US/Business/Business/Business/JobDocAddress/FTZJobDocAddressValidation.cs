using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class FTZJobDocAddressValidation : JobDocAddressValidation
	{
		public FTZJobDocAddressValidation(AutoJobDocAddress parent)
		: base(parent)
		{
		}

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();
			if (Parent != null && Parent.E2_AddressType == DocAddressTypes.Codes.CustomsWarehouseAddress)
			{
				ValidateWarehouseAddress();
			}
		}

		void ValidateWarehouseAddress()
		{
			if (Parent?.OrganisationPK.IsEmpty ?? true)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.OrganisationPKInfo, "FTZ operator");
			}
		}
	}
}
