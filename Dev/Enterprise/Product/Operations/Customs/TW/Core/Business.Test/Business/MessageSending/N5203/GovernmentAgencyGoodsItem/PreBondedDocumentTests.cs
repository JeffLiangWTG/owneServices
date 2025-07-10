using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class PreBondedDocumentTests : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestFunctionalReferenceID()
		{
			NUnit.Framework.Assert.That(preBondedDocument.FunctionalReferenceID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestID()
		{
			NUnit.Framework.Assert.That(preBondedDocument.ID, NUnit.Framework.Is.EqualTo("DXD2").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestLineNumeric()
		{
			NUnit.Framework.Assert.That(preBondedDocument.LineNumeric, NUnit.Framework.Is.EqualTo(3).Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var testHelper = new TestTWCreator(Factory);
			var entryHeader = testHelper.CreateEntryHeaderForN5203();
			var invoiceLine = testHelper.CreateInvoiceLineForN5203(entryHeader);
			invoiceLine.PreviousBondedEntryNumber = "DXD2";
			invoiceLine.PreviousBondedEntryLineNumber = 3;
			preBondedDocument = new GovernmentAgencyGoodsItem(entryHeader.MergedLines.Cast<CusEntryLine>().First(), invoiceLine).PreBondedDocument;
		}

		IPreviousDocument preBondedDocument;
	}
}
