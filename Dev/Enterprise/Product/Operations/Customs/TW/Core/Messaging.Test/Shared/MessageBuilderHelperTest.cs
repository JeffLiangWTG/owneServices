using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Messaging.Testing.Shared
{
	sealed class MessageBuilderHelperTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestEffectiveAEOCountryCodes()
		{
			var effectiveAEOCountryCodes = MessageBuilderHelper.EffectiveAEOCountryCodes;
			NUnit.Framework.Assert.That(effectiveAEOCountryCodes, NUnit.Framework.Has.Some.EqualTo(Core.Constants.CountryCodes.Taiwan).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(effectiveAEOCountryCodes, NUnit.Framework.Has.Some.EqualTo(Core.Constants.CountryCodes.Singapore).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(effectiveAEOCountryCodes, NUnit.Framework.Has.Some.EqualTo(Core.Constants.CountryCodes.Israel).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(effectiveAEOCountryCodes, NUnit.Framework.Has.Some.EqualTo(Core.Constants.CountryCodes.China).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(effectiveAEOCountryCodes, NUnit.Framework.Has.Some.EqualTo(Core.Constants.CountryCodes.KoreaSouth).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(effectiveAEOCountryCodes, NUnit.Framework.Has.Some.EqualTo(Core.Constants.CountryCodes.Australia).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(effectiveAEOCountryCodes, NUnit.Framework.Has.Some.EqualTo(Core.Constants.CountryCodes.India).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(effectiveAEOCountryCodes, NUnit.Framework.Has.Some.EqualTo(Core.Constants.CountryCodes.Japan).Using(CustomComparers.TypeComparison));
		}
	}
}
