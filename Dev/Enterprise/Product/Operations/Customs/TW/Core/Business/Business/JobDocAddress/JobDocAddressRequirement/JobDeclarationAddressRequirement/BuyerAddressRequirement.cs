using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.TW.Business
{
	public class BuyerAddressRequirement : TWJobDocAddressRequirement
	{
		public BuyerAddressRequirement(DocAddressType defaultDocAddressType, ContactType defaultContactType)
			: base(defaultDocAddressType, defaultContactType)
		{
		}

		protected override void Initialize()
		{
			base.Initialize();
			this.ValidateCompanyName += ValidateE2_CompanyName;
			this.ValidateOrganisationPK += ValidateOrganization;
		}

		void ValidateE2_CompanyName(JobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent && parent.E2_AddressOverride)
			{
				ValidateE2_CompanyNameCore(parent, parent.E2_CompanyNameInfo);
			}
		}

		void ValidateE2_CompanyNameCore(TWJobDocAddress parent, ZPropertyInfo propertyInfo)
		{
			if (parent.E2_CompanyName.IsEmpty)
			{
				propertyInfo.AddWarning(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Buyer.CompanyNameResString));
			}
		}

		void ValidateOrganization(JobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent && !parent.E2_AddressOverride)
			{
				ValidateE2_CompanyNameCore(parent, parent.OrganisationPKInfo);
			}
		}
	}
}
