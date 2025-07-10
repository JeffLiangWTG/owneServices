using CargoWise.Common;
using CargoWise.Customs.TR.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NCTSOrganizationProvider : INCTSOrganization
	{
		public NCTSOrganizationProvider(JobDocAddress jobDocAddress)
		{
			this.jobDocAddress = Argument.NotNull(jobDocAddress, nameof(JobDocAddress));
			orgAddress = jobDocAddress.Address;
			orgHeader = jobDocAddress.Address?.Header;
		}
		protected readonly JobDocAddress jobDocAddress;
		protected readonly OrgAddress orgAddress;
		protected readonly OrgHeader orgHeader;

		public string CompanyName => orgHeader?.OH_FullName.SubstringSafe(0, 35) ?? ZString.Empty;
		public string Address
		{
			get
			{
				ZString addresses = string.Join(" ", orgAddress?.Address1.TrimEnd(), orgAddress?.Address2.TrimEnd());
				return addresses.SubstringSafe(0, 35);
			}
		}

		public string PostCode => orgAddress?.Postcode ?? ZString.Empty;
		public string City => orgAddress?.City ?? ZString.Empty;
		public string CountryCode => orgAddress?.OA_RN_NKCountryCode ?? ZString.Empty;
		public string Language => orgHeader?.OH_Language.SubstringSafe(0, 2) ?? ZString.Empty;
		public string VATID => orgHeader?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.Turkey) ?? ZString.Empty;
	}
}
