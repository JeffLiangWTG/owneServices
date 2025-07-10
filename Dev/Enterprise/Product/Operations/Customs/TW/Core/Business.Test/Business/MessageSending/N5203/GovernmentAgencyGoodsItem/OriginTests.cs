using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class OriginTests : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCountryCode()
		{
			NUnit.Framework.Assert.That(origin.CountryCode, NUnit.Framework.Is.EqualTo(Core.Constants.CountryCodes.Taiwan).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAdditionalDocument()
		{
			NUnit.Framework.Assert.That(origin.AdditionalDocument.GetType(), NUnit.Framework.Is.EqualTo(typeof(AdditionalDocumentWrapper)));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var testHelper = new TestTWCreator(Factory);
			var entryHeader = testHelper.CreateEntryHeaderForN5203();
			var invoiceLine = testHelper.CreateInvoiceLineForN5203(entryHeader);
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Taiwan;
			origin = new Origin(invoiceLine);
		}

		IOrigin origin;
	}
}
