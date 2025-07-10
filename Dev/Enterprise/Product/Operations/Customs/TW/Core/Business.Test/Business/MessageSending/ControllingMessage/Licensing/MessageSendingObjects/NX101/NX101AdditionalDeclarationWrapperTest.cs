using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX101AdditionalDeclarationWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestNX101AdditionalDeclarationWrapper()
		{
			IAdditionalDeclaration additionalDeclaration = new NX101AdditionalDeclarationWrapper(2m, "CA 0001");
			NUnit.Framework.Assert.That(additionalDeclaration.SequenceNumeric, NUnit.Framework.Is.EqualTo(2m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(additionalDeclaration.ID, NUnit.Framework.Is.EqualTo("CA 0001").Using(CustomComparers.TypeComparison));
		}
	}
}
