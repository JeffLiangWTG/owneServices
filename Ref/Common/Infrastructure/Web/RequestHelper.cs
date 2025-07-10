using System;
using System.Net.Http.Headers;
using System.Text;

namespace CargoWise.RefDbRepo.Common.Web
{
	public static class RequestHelper
	{
		public static Tuple<string, string> GetUserIdAndPassword(string authHeaderString)
		{
			if (!string.IsNullOrEmpty(authHeaderString))
			{
				var authenticationHeader = AuthenticationHeaderValue.Parse(authHeaderString);
				if (authenticationHeader != null)
				{
					if ("basic".Equals(authenticationHeader.Scheme, StringComparison.OrdinalIgnoreCase) && authenticationHeader.Parameter != null)
					{
						try
						{
							var encoding = Encoding.GetEncoding("iso-8859-1");
							var credentials = authenticationHeader.Parameter;
							credentials = encoding.GetString(Convert.FromBase64String(credentials));

							int separator = credentials.IndexOf(':');
							var name = credentials.Substring(0, separator);
							var password = credentials.Substring(separator + 1);
							return Tuple.Create(name, password);
						}
						catch (FormatException)
						{ }
					}
				}
			}
			return null;
		}
	}
}
