using System.Globalization;
using Enterprise.Freight.Forwarding.AWB.Messaging;
using Enterprise.Freight.Integration.AWB;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	class EgyptVATCountryHandler : VATCountryHandler
	{
		public EgyptVATCountryHandler(IFBaseMessageDetailsProvider provider) : base(provider)
		{
		}

		public override bool ConsigneeApplicable() => IsEgyptVAT(Provider.ConsigneeCountryCode, Provider.ConsigneeTraderNoType);

		public override string GetConsigneeTraderCode()
		{
			return Provider.ConsigneeTraderNo;
		}

		public override bool ShipperApplicable() => IsFWB && Provider.ConsigneeCountryCode == CountryCodes.Egypt || IsEgyptVAT(Provider.ShipperCountryCode, Provider.ShipperTraderNoType);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String comparison")]
		public override string GetShipperTraderCode()
		{
			if (IsFWB && Provider.ConsigneeCountryCode == CountryCodes.Egypt)
			{
				var shipperIDCode = Provider.DestinationShipperComment == "Tax ID"
				? "01"
				: Provider.DestinationShipperComment == "Registration ID"
					? "02"
					: string.Empty;

				return string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2}",
					Provider.ShipperCountryCode,
					shipperIDCode,
					Provider.ShipperTraderNo);
			}
			return Provider.ShipperTraderNo;
		}

		public override bool AlsoNotifyApplicable() => IsEgyptVAT(Provider.AlsoNotifyCountryCode, Provider.AlsoNotifyTraderNoType);

		public override string GetAlsoNotifyTraderCode()
		{
			return Provider.AlsoNotifyTraderNo;
		}

		bool IsEgyptVAT(string countryCode, string type) => countryCode == CountryCodes.Egypt && type == OrgCusCode.CodeTypes.VATCode;
	}
}
