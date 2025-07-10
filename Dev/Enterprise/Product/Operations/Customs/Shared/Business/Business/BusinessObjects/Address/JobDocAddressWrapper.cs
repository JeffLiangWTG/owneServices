using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class JobDocAddressWrapper : IAddress
	{
		public JobDocAddressWrapper(JobDocAddress docAddress)
		{
			jobDocAddress = Argument.NotNull(docAddress, nameof(docAddress));
		}
		readonly JobDocAddress jobDocAddress;

		public static JobDocAddressWrapper New(JobDocAddress docAddress) => new JobDocAddressWrapper(docAddress);

		#region IAddress Members

		public OrgHeader Organisation => jobDocAddress.E2_AddressOverride ? null : jobDocAddress.Organisation;
		public OrgAddress OrgAddress => jobDocAddress.E2_AddressOverride ? null : jobDocAddress.Address;

		public ZString CompanyName => jobDocAddress.E2_CompanyName;

		public ZString Address1 => jobDocAddress.E2_Address1;

		public ZString Address2 => jobDocAddress.E2_Address2;

		public ZString Address3 => (City + " " + State).TrimEnd();

		public ZString City => jobDocAddress.E2_City;

		public ZString State => jobDocAddress.E2_State;

		public ZString PostCode => jobDocAddress.E2_Postcode;

		public ZString CountryCode => jobDocAddress.E2_RN_NKCountryCode;

		public ZString RelatedPortCode => ZString.Empty;

		public ZString Phone => jobDocAddress.E2_Phone;

		public ZString Fax => jobDocAddress.E2_Fax;

		#endregion
	}
}
