using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportConsignment.Business
{
	public static class JobDocAddressExtensions
	{
		/// <summary>
		/// AddressAsSingleLine will load the Org.MainAddress which shouldn't be fetch hinted, so build the address line manually.
		/// </summary>
		public static ZString GetAddressLine(this JobDocAddress jobDocAddress, BusinessObjectFactory factory)
		{
			return new AddressFormatter(factory, jobDocAddress.E2_CompanyName, jobDocAddress.E2_Address1, jobDocAddress.E2_Address2, jobDocAddress.E2_City,
				jobDocAddress.E2_State, jobDocAddress.E2_Postcode, (NoResString)"", true).PostalAddressAsASingleLineWithoutCompanyName();
		}
	}
}
