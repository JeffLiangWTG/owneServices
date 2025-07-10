using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business.US
{
	public class WhsOrderDocAddressValidation : Business.WhsOrderDocAddressValidation
	{
		public WhsOrderDocAddressValidation(JobDocAddress addressToValidate)
			: base(addressToValidate)
		{
		}

		protected override void ValidateTransportCo()
		{
			if (Docket != null)
			{
				ValidateIfTransportCoIsMandatory();
			}
		}

		void ValidateIfTransportCoIsMandatory()
		{
			var order = (WhsOrder)Docket;
			if (!order.TransportCoDocAddress.IsValidAddress)
			{
				if (order.Warehouse.IsApprovedKnown && (order.IsFinalised || order.IsFinalising))
				{
					Parent.OrganisationPKInfo.AddError(TransportCompanyNotEnteredWithReasonErrorMsg);
				}
				else
				{
					Parent.OrganisationPKInfo.AddWarning(TransportCompanyNotEntered);
				}
			}
		}

		#region Implementation

		protected ZString TransportCoNotKnownByTSA => Res.GetString("4ef9f01d-e8f2-42e8-bdd8-53e9e89484e8", "Carrier is not known by TSA.");

		ZString TransportCompanyNotEnteredWithReasonErrorMsg
			=> Res.GetString("30b355a9-ef36-47ee-971e-22280db20661", "The Transport Company has no address entered. A Transport Company with a valid address is required when finalizing orders in a TSA known warehouse.");

		#endregion
	}
}
