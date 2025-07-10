using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Rating.Business
{
	class QuoteDocAddressValidation : JobDocAddressValidation
	{
		public QuoteDocAddressValidation(JobDocAddress addressToValidate)
			: base(addressToValidate)
		{
		}

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();

			if (Parent.Organisation != null && !Parent.Organisation.OH_IsActive)
			{
				Parent.OrganisationPKInfo.AddError(Res.GetString("6d179b77-4bc0-46e8-af46-7e0b73f70cd5", "This Organization is not active."));
			}

			var quote = (Quote)Parent.Parent;

			if (!HasAddress(DocAddressType.QuotationClientAddress))
			{
				quote.TH_OHInfo.AddError(Res.GetString("8a25243c-38cc-41a3-b3f9-45e67f6c961e", "This Organization has no address entered"));
			}

			if (!quote.TH_OH.IsEmpty && quote.CurrentOneOffQuote != null)
			{
				if (quote.CurrentOneOffQuote.TT_OrgRole == Core.Constants.OrgRoles.LocalClient)
				{
					ListValidation.ErrorIfInvalidPK(quote.TH_OHInfo, quote.Lookups.Clients, ErrorMessages.InvalidClientRateHeader);
				}
				else
				{
					ListValidation.ErrorIfInvalidPK(quote.TH_OHInfo, quote.Lookups.Agents, ErrorMessages.InvalidOverseasAgentRateHeader);
				}
			}

			if (quote.TH_OneTimeQuote && (!quote.IsInDatabase || quote.QuotationClientAddress.E2_OA_AddressInfo.HasChanges))
			{
				var salesRepStaff = quote.Header?.StaffAssignments?.OverallSalesRepStaff;
				if (salesRepStaff != null && !salesRepStaff.GS_IsActive)
				{
					quote.TH_OHInfo.AddError(ErrorMessages.FirstSignatoryIsInactive);
				}
			}
		}

		bool HasAddress(DocAddressType addressType)
		{
			var address = Parent.Parent.DocAddresses.FindByDocAddressType(addressType);
			return address != null && address.IsValidAddress;
		}
	}
}

