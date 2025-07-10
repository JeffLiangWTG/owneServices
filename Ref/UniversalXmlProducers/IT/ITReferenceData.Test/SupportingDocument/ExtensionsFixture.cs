using System;
using CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.SupportingDocument
{
	[TestFixture]
	sealed class ExtensionsFixture
	{
		[Test]
		public void GuardClause()
		{
			Assert.Throws<ArgumentNullException>(
				() => (null as IRawSupportingDocument).BelongsToUnitedNationEdifactCategory(),
				"When rawSupportingDocument is null");
		}

		[TestCase(null, ExpectedResult = false)]
		[TestCase("", ExpectedResult = false)]
		[TestCase("N", ExpectedResult = true)]
		[TestCase("0", ExpectedResult = false)]
		public bool BelongsToUnitedNationEdifactCategory(string documentCode)
		{
			var rawSupportingDocument = new Mock<IRawSupportingDocument>();
			rawSupportingDocument.Setup(x => x.Code)
				.Returns(documentCode);

			return rawSupportingDocument.Object.BelongsToUnitedNationEdifactCategory();
		}
	}
}
