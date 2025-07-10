using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.MasterFiles.Business
{
	public class SpotRateInfo
	{
		public SpotRateInfo(Money rate, ZString autoratedMode, AutoratedValueType autoratedValueType)
		{
			Rate = rate;
			AutoratedMode = autoratedMode;
			AutoratedValueType = autoratedValueType;
		}

		public Money Rate { get; private set; }
		public ZString AutoratedMode { get; private set; }
		public AutoratedValueType AutoratedValueType { get; private set; }
		public OrgHeader Creditor { get; set; }
	}

	public enum AutoratedValueType
	{
		[ResourceStringData("AutoratedValueType|ClientRate", Caption = "Client Rate")]
		ClientRate,

		[ResourceStringData("AutoratedValueType|Cost", Caption = "Cost")]
		Cost,

		[ResourceStringData("AutoratedValueType|NegotiatedCost", Caption = "Negotiated Cost")]
		NegotiatedCost,

		[ResourceStringData("AutoratedValueType|GatewaySell", Caption = "Gateway Sell")]
		GatewaySell,

		[ResourceStringData("AutoratedValueType|SpotRate", Caption = "Spot Rate")]
		SpotRate
	}

	public static class SpotRateInfoExtensions
	{
		public static string GetAutoratedValueTypeDescription(this SpotRateInfo spotRateInfo)
		{
			switch (spotRateInfo.AutoratedValueType)
			{
				case AutoratedValueType.SpotRate:
					return Res.GetString("47dd8ba4-5fb4-4303-90d3-dd1203d25dbe", "One Off Freight Rate");
				case AutoratedValueType.NegotiatedCost:
					return Res.GetString("d2d15837-9a8a-48bc-95dc-e2c074d64ceb", "Negotiated Cost");
				case AutoratedValueType.GatewaySell:
					return Res.GetString("8f09c946-f3fc-40f5-8e00-06d12d8a7537", "Gateway Sell");
				default:
					return string.Empty;
			}
		}
	}
}