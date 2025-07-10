using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing
{
	static class NUnitHelpers
	{
		public static void AssertCodeListIsOrdered(this CodeDescriptionPairList self)
		{
			var actual = self.ToArray();
			self.Sort();
			var sorted = self.ToArray();
			Assertion.AssertSequencesEqual("codes should be sorted", actual, sorted);
		}
	}
}
