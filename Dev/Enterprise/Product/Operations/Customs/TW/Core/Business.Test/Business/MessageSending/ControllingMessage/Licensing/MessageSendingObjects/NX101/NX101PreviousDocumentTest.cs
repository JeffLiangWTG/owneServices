using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX101PreviousDocumentTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestData()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(previousDocument.FunctionalReferenceID.ToString(), NUnit.Framework.Is.Null.Or.Empty, "FunctionalReferenceID - should be [null] or [empty]");
				NUnit.Framework.Assert.That(previousDocument.ID, NUnit.Framework.Is.EqualTo("DN81E905510000").Using(CustomComparers.TypeComparison), "ID");
				NUnit.Framework.Assert.That(previousDocument.LineNumeric, NUnit.Framework.Is.EqualTo(ZInt.Zero), "LineNumeric");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			header.TW1_PrePermitNumber = "DN81E905510000";
			previousDocument = new NX101PreviousDocument(header);
		}

		CusTWControllingMessageHeader header;
		NX101PreviousDocument previousDocument;
	}
}
