using System;

namespace CargoWise.eHub.Products.GBCustoms.CDS.CredentialWebSite.Models
{
	public class NaturalKey
	{
		public NaturalKey(string state)
		{
			if (state == null)
			{
				throw new ArgumentNullException(nameof(state));
			}

			var stateArray = state.Split('.');

			if (stateArray.Length != 3)
			{
				throw new ArgumentException("Invalid value", nameof(state));
			}

			EnterpriseDbCode = stateArray[0];
			Eori = stateArray[1];
			Profile = stateArray[2];
		}

		public string EnterpriseDbCode { get; set; }

		public string Eori { get; set; }

		public string Profile { get; set; }
	}
}