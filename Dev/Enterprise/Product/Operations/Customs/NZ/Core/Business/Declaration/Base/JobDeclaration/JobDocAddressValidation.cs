using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class JobDocAddressValidation : Enterprise.MasterFiles.Business.JobDocAddressValidation
	{
		public JobDocAddressValidation(JobDocAddress parent, JobDeclaration declaration)
			: base(parent)
		{
			this.declaration = declaration;
		}
		readonly JobDeclaration declaration;

		protected override void CheckE2_OA_Address()
		{
			base.CheckE2_OA_Address();

			switch (Parent.E2_AddressType)
			{
				case DocAddressTypes.Codes.CustomsWarehouseAddress:
					if (declaration.WarehouseAddress == null)
					{
						if (declaration.IsExcise)
						{
							Parent.E2_OA_AddressInfo.AddMessageError("This is an excise entry and you need to enter a Bonded Warehouse.");
						}
					}
					else
					{
						if (declaration.WarehouseAddress.LocalControlledPremisesID.IsEmpty)
						{
							Parent.E2_OA_AddressInfo.AddMessageError("Bonded Warehouse Address selected does not have a Customs Controlled Premises code entered against it.");
						}
					}
					break;

				case DocAddressTypes.Codes.ImporterPickupDeliveryAddress:
					if (declaration.DeliveryDestinationPartyDocAddressVisible && declaration.DeliveryDestinationPartyDocAddress != null && Parent.Country != null)
					{
						if (declaration.FinalDestination != null && declaration.FinalDestination.Code.Substring(0, 2) != Parent.Country.Code)
						{
							Parent.E2_OA_AddressInfo.AddMessageError("The country/region for the Delivery Address differs from the port of final destination country/region.");
						}
					}
					break;

				case DocAddressTypes.Codes.NotifyParty2:
					var address = Parent.Address;
					if (address != null)
					{
						if (Parent.GetCCPOrATFCode().IsEmpty)
						{
							Parent.E2_OA_AddressInfo.AddMessageError(OrganisationShouldHaveCCPOrATFCode);
						}
					}
					break;
			}
		}

		public const string OrganisationShouldHaveCCPOrATFCode = "The Org Address should have either an CCP or ATF config code.";
	}
}
