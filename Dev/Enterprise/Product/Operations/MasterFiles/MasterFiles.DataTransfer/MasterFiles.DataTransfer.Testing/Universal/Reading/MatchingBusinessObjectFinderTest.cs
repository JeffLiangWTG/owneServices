using Enterprise.UniversalDataBuss.Testing.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	[TestsSubclassesOf(typeof(MatchingBusinessObjectFinder<,>))]
	public abstract class MatchingBusinessObjectFinderTest : TestCaseWithUniversalObjectFactory
	{
		public abstract void TestFind();
	}
}
