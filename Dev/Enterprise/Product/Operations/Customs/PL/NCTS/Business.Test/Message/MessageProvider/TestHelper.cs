using CargoWise.Types;
using Enterprise.Customs.Universal;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

static class TestHelper
{
	public static void RunAssertionInPhase5TransitionPeriod(AssertionWithHtml.VoidParameterlessDelegate assertions)
		=> RunAssertionsInPhase5TransitionPeriod(assertions, false);

	public static void RunAssertionsInPhase5TransitionPeriod(bool combineAssertions, AssertionWithHtml.VoidParameterlessDelegate assertions)
		=> RunAssertionsInPhase5TransitionPeriod(assertions, combineAssertions);

	public static void RunAssertionsInPhase5TransitionPeriod(AssertionWithHtml.VoidParameterlessDelegate assertions, bool combineAssertions = true)
	{
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
					RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
		{
			if (combineAssertions)
			{
				AssertionWithHtml.CombineAssertions(assertions);
			}
			else
			{
				assertions.Invoke();
			}
		}
	}

	public static void RunAssertionOutsidePhase5TransitionPeriod(AssertionWithHtml.VoidParameterlessDelegate assertions)
		=> RunAssertionsOutsidePhase5TransitionPeriod(assertions, false);

	public static void RunAssertionsOutsidePhase5TransitionPeriod(bool combineAssertions, AssertionWithHtml.VoidParameterlessDelegate assertions)
		=> RunAssertionsOutsidePhase5TransitionPeriod(assertions, combineAssertions);

	public static void RunAssertionsOutsidePhase5TransitionPeriod(AssertionWithHtml.VoidParameterlessDelegate assertions, bool combineAssertions = true)
	{
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
					RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
		{
			if (combineAssertions)
			{
				AssertionWithHtml.CombineAssertions(assertions);
			}
			else
			{
				assertions.Invoke();
			}
		}
	}
}
