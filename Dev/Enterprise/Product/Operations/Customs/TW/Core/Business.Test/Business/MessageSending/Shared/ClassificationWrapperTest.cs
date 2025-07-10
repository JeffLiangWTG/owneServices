using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ClassificationWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestClassification()
		{
			IClassification classification = new ClassificationWrapper("ID", "TYP");
			NUnit.Framework.Assert.That(classification.ID, NUnit.Framework.Is.EqualTo("ID").Using(CustomComparers.TypeComparison), "Classification.ID should be");
			NUnit.Framework.Assert.That(classification.IdentificationTypeCode, NUnit.Framework.Is.EqualTo("TYP").Using(CustomComparers.TypeComparison), "Classification.IdentificationTypeCode should be");
		}
	}
}
