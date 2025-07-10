using System;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.MasterFiles.Business.OrgPatternMatching.Testing
{
	sealed class PatternMatchDeleterTest : TestCaseWithFactory
	{
		public void TestConstructorChecksForNulls()
		{
			var mockDataManager = new Mock<IPatternMatchDataManager>();

			AssertExceptionThrown(typeof(ArgumentNullException), delegate
			{ new PatternMatchDeleter(null); });
			AssertNoExceptionThrown(delegate
			{ new PatternMatchDeleter(mockDataManager.Object); });
		}
	}
}
