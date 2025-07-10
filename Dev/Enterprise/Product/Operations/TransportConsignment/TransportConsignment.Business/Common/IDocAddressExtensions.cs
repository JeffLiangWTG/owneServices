using Enterprise.MasterFiles.Integration;

namespace Enterprise.TransportConsignment.Business
{
	public static class IDocAddressExtensions
	{
		/// <summary>
		/// Concatenates company name, address1, address2, city, postcode and state, trimming each portion and converting the result UCASE.
		/// </summary>
		public static string GetAddressUniqueKey(this IDocAddress address)
		{
			string result;

			if (address != null)
			{
				result = string.Concat
				(
					address.E2_CompanyName.Trim(),
					address.E2_Address1.Trim(),
					address.E2_Address2.Trim(),
					address.E2_City.Trim(),
					address.E2_Postcode.Trim(),
					address.E2_State.Trim()
				)
				.ToUpperInvariant();
			}
			else
			{
				result = "";
			}

			return result;
		}
	}
}
