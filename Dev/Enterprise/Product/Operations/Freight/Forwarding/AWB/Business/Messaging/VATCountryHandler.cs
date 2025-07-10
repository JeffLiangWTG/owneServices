using CargoWise.Types;
using Enterprise.Freight.Integration.AWB;

namespace Enterprise.Freight.Forwarding.AWB.Messaging
{
	public abstract class VATCountryHandler : IVATCountryHandler
	{
		public VATCountryHandler(IFBaseMessageDetailsProvider provider)
		{
			Provider = provider;
			IsFWB = provider is IFWBMessageDetailsProvider;
		}

		protected IFBaseMessageDetailsProvider Provider { get; }

		protected bool IsFWB { get; }

		public virtual bool ConsigneeApplicable() => false;
		public virtual bool ShipperApplicable() => false;
		public virtual bool AlsoNotifyApplicable() => false;

		public virtual string GetConsigneeTraderCode() => string.Empty;
		public virtual string GetShipperTraderCode() => string.Empty;
		public virtual string GetAlsoNotifyTraderCode() => string.Empty;

		public static string CombineTypeNo(string type, string number) => type.Replace(" ", string.Empty) + number;

		public static string GetTraderCodeWithoutSpecialCharacters(string traderNo) => new ZString(traderNo).KeepChars(ZString.AlphanumericCharacters);
	}
}
