using System.Collections.Immutable;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging
{
	public static class MessageBuilderHelper
	{
		public static readonly ImmutableArray<ZString> EffectiveAEOCountryCodes = ImmutableArray.Create<ZString>(
			Core.Constants.CountryCodes.Taiwan,
			Core.Constants.CountryCodes.Singapore,
			Core.Constants.CountryCodes.Israel,
			Core.Constants.CountryCodes.China,
			Core.Constants.CountryCodes.KoreaSouth,
			Core.Constants.CountryCodes.Australia,
			Core.Constants.CountryCodes.India,
			Core.Constants.CountryCodes.Japan);
	}
}
