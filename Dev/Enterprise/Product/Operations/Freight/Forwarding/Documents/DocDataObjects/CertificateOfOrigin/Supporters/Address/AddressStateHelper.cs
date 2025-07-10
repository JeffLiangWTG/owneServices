using System;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Address
{
	public static class AddressStateHelper
	{
		public static string GetAddressStateString(bool sameAsExporter = false, bool unknown = false, bool excludeFromCertificate = false)
		{
			if (sameAsExporter && unknown)
			{
				throw new ArgumentException($"{nameof(sameAsExporter)} and {nameof(unknown)} cannot both be true at the same time");
			}

			return  $"{sameAsExporter}\n{unknown}\n{excludeFromCertificate}";
		}

		public static AddressState GetAddressStateObjectFromString(string addressState)
		{
			try
			{
				var split = addressState?.Split('\n');

				if (split == null || split.Length != 3)
				{
					return null;
				}

				return new AddressState()
				{
					IsSameAsExporter = bool.Parse(split[0]),
					IsUnknown = bool.Parse(split[1]),
					ExcludeFromPDF = bool.Parse(split[2])
				};
			}
			catch
			{
				return null;
			}
		}

		public static string GetAddressStateString(this AddressState addressState)
			=> $"{((bool)addressState.IsSameAsExporter).ToString()}\n{((bool)addressState.IsUnknown).ToString()}\n{((bool)addressState.ExcludeFromPDF).ToString()}";
	}
}
