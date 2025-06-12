using System;

namespace CargoWise.eHub.Products.GBCustoms.Core.BT.Helpers
{
	[Serializable]
	public class CallbackRegistration
	{
		public static CallbackRegistration Create(string url, string authorization)
		{
			if (string.IsNullOrEmpty(url))
			{
				throw new ArgumentNullException("url");
			}

			if (string.IsNullOrEmpty(authorization))
			{
				throw new ArgumentNullException("authorization");
			}

			return new CallbackRegistration
			{
				Uri = new Uri(url),
				Authorization = authorization
			};
		}

		public Uri Uri { get; set; }

		public string Authorization { get; set; }
	}
}