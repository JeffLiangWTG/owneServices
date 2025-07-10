using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportCommon.Business
{
	public class DtbTransportDocAddressValidation : JobDocAddressValidation
	{
		public DtbTransportDocAddressValidation(JobDocAddress addressToValidate)
			: base(addressToValidate)
		{ }

		#region CheckE2_OA_Address

		protected override void CheckE2_OA_Address()
		{
			base.CheckE2_OA_Address();
			AddBillingPartyReceivablesWarning(Parent, j => j.E2_OA_AddressInfo);
		}

		#endregion

		#region CheckOrganisationPK

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();
			AddBillingPartyReceivablesWarning(Parent, j => j.OrganisationPKInfo);
		}

		#endregion

		static void AddBillingPartyReceivablesWarning(JobDocAddress jobDocAddress, Func<JobDocAddress, ZPropertyInfo> propertyInfo)
		{
			if (jobDocAddress.E2_AddressType == DocAddressTypes.Codes.ClientRequestedBillingParty && jobDocAddress.Organisation != null && !jobDocAddress.Organisation.OH_IsDebtor)
			{
				propertyInfo(jobDocAddress).AddWarning(Res.GetString("DtbTransportDocAddressValidation|ClientRequestedBillingParty|ShouldBeADebtor",
					"The Billing Party should be marked as a Receivables organization."));
			}
		}
	}
}
