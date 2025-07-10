using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsReceiveDocAddressValidation : WhsDocketDocAddressValidation
	{
		public WhsReceiveDocAddressValidation(JobDocAddress addressToValidate)
			: base(addressToValidate)
		{
		}

		#region Validation

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();

			if (!Parent.OrganisationPKInfo.HasErrors())
			{
				switch (Parent.E2_AddressType)
				{
					case TransportCoConstants.AddressTypeCode:
						ValidateTransportCo();
						break;
				}
			}
		}

		protected virtual void ValidateTransportCo()
		{
			var receive = (WhsReceive)Docket;
			if (receive != null && !receive.TransportCoDocAddress.IsValidAddress)
			{
				Parent.OrganisationPKInfo.AddWarning(TransportCompanyNotEntered);
			}
		}

		#endregion

		#region Implementation

		ZString TransportCompanyNotEntered => Res.GetString("a01b992c-d62d-4305-a542-700ff69951ca", "The Transport Company has no address entered");

		#endregion
	}
}
