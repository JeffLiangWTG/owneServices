using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CargoWise.eHub.Products.GBCustoms.CDS.CredentialWebService
{
	public enum RequestType
	{
		RequestAccessKey,
		RefreshAccessKey,
		RequestAppWideAccessKey
	}

	static class RequestTypeExtensions
	{
		public static String GetName(this RequestType request)
		{
			return Enum.GetName(typeof(RequestType), request);
		}

		public static String GetAction(this RequestType request)
		{
			switch (request)
			{
				case RequestType.RequestAccessKey:
					return "inserted";
				case RequestType.RequestAppWideAccessKey:
				case RequestType.RefreshAccessKey:
					return "updated";
				default:
					return "unknown action";
			}
		}
	}
}
