using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class PreviousDocumentWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestPreviousDocument()
		{
			IPreviousDocument previousDocument = new PreviousDocumentWrapper("A");
			NUnit.Framework.Assert.That(previousDocument.ID, NUnit.Framework.Is.EqualTo("A").Using(CustomComparers.TypeComparison), "PreviousDocument.ID should be");
			NUnit.Framework.Assert.That(previousDocument.LineNumeric, NUnit.Framework.Is.EqualTo(0).Using(CustomComparers.TypeComparison), "PreviousDocument.LineNumeric should be");
			previousDocument = new PreviousDocumentWrapper("XX", 12);
			NUnit.Framework.Assert.That(previousDocument.ID, NUnit.Framework.Is.EqualTo("XX").Using(CustomComparers.TypeComparison), "PreviousDocument.ID should be");
			NUnit.Framework.Assert.That(previousDocument.LineNumeric, NUnit.Framework.Is.EqualTo(12).Using(CustomComparers.TypeComparison), "PreviousDocument.LineNumeric should be");
		}

		[ExpectNoExceptions]
		public void TestCheckNotApplicableProperties()
		{
			IPreviousDocument previousDocument = new PreviousDocumentWrapper("A");
			NUnit.Framework.Assert.That(previousDocument.FunctionalReferenceID, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
		}
	}
}
