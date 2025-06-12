using System;

namespace CargoWise.eHub.Products.GBCustoms.CDS.CredentialWebService
{
	public class NaturalKey
	{
		public NaturalKey(string naturalKey)
		{
			if (naturalKey == null)
			{
				throw new ArgumentNullException(nameof(naturalKey));
			}

			var naturalKeyArray = naturalKey.Split('.');

			if (naturalKeyArray.Length != 3 || naturalKeyArray[0].Length != 6)
			{
				throw new ArgumentException("Invalid naturalKey", nameof(naturalKey));
			}

			EnterpriseDbCode = naturalKeyArray[0];
			EoriBadge = naturalKeyArray[1] + '.' + naturalKeyArray[2];
		}

		public string EnterpriseDbCode { get; private set; }

		public string EoriBadge { get; private set; }

		public string EnterpriseCode
		{
			get { return EnterpriseDbCode.Substring(0, 3); }
		}

		public string DBCode
		{
			get { return EnterpriseDbCode.Substring(3); }
		}
	}
}