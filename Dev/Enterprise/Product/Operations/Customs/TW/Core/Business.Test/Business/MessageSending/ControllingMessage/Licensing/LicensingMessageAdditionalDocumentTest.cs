using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(LicensingMessageAdditionalDocument))]
	sealed class LicensingMessageAdditionalDocumentTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestID()
		{
			NUnit.Framework.Assert.That(((IDeclarationAdditionalDocument)additionalDocument).ID, NUnit.Framework.Is.EqualTo("HELLO").Using(CustomComparers.TypeComparison), "ID");
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			header.PermitNumber = "HELLO";
			additionalDocument = new LicensingMessageAdditionalDocument(header);
		}

		CusTWControllingMessageHeader header;
		LicensingMessageAdditionalDocument additionalDocument;
	}
}
