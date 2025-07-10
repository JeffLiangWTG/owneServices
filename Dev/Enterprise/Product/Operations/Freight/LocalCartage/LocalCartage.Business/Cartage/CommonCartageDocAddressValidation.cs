using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CommonCartageDocAddressValidation : JobDocAddressValidation
	{
		public CommonCartageDocAddressValidation(JobDocAddress addressToValidate)
			: base(addressToValidate)
		{
		}

		protected override void CheckE2_OA_Address()
		{
			base.CheckE2_OA_Address();

			if (Parent.E2_AddressType != DocAddressTypes.Codes.NonPersistent)
			{
				RequireAddress();
			}

			if (CartageParent.HasParent)
			{
				ZString orgType = CommonCartageAddressHelper.GetOrgTypeFromCartageDocAddressType(Parent.DocAddressType);
				JobDocAddress parentAddr = CartageParent.CartageInternalType?.GetCartageAddress(orgType);

				if (parentAddr != null && !parentAddr.IsTheSameAddressAs(Parent))
				{
					Parent.E2_OA_AddressInfo.AddWarning(Res.GetString("7bae0875-25a7-45d8-baef-d1214447de43", "Address is different to parent's, see linked job."));
				}
			}
		}

		CommonCartage CartageParent
		{
			get
			{
				return Parent.Parent as CommonCartage;
			}
		}

		protected void RequireAddress()
		{
			if (!(Parent.E2_OA_Address.IsValid || Parent.E2_AddressOverride))
			{
				//use address type description
				MandatoryValidation.WarnIfNotEntered(Parent.E2_OA_AddressInfo, Parent.E2_AddressType);
			}
		}
	}
}
