using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.N5167;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class AdditionalDeclarationTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestID()
		{
			NUnit.Framework.Assert.That(additionalDeclaration.ID, NUnit.Framework.Is.EqualTo("XXX1").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTypeCode()
		{
			NUnit.Framework.Assert.That(additionalDeclaration.TypeCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		protected override void SetUp()
		{
			base.SetUp();
			additionalDeclaration = new AdditionalDeclaration("XXX1");
		}

		IAdditionalDeclaration additionalDeclaration;
	}
}
