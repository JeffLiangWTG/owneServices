using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class PreviousDocumentTests : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestFunctionalReferenceID()
		{
			NUnit.Framework.Assert.That(previousDocument.FunctionalReferenceID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestID()
		{
			NUnit.Framework.Assert.That(previousDocument.ID, NUnit.Framework.Is.EqualTo("DXD2").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestLineNumeric()
		{
			NUnit.Framework.Assert.That(previousDocument.LineNumeric, NUnit.Framework.Is.EqualTo(3).Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var testHelper = new TestTWCreator(Factory);
			var entryHeader = testHelper.CreateEntryHeaderForN5203();
			var invoiceLine = testHelper.CreateInvoiceLineForN5203(entryHeader);
			invoiceLine.JI_PreviousEntryNumber = "DXD2";
			invoiceLine.JI_PreviousEntryLineNumber = 3;
			previousDocument = new GovernmentAgencyGoodsItem(entryHeader.MergedLines.Cast<CusEntryLine>().First(), invoiceLine).PreviousDocument;
		}

		IPreviousDocument previousDocument;
	}
}
