using System;
using CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.SupportingDocument
{
	[TestFixture]
	abstract class RawSupportingDocumentFixture<TRawSupportingDocument> where TRawSupportingDocument : IRawSupportingDocument
	{
		[Test]
		public void Constructor()
		{
			Assert.Throws<ArgumentNullException>(() => GetNewRawSupportingDocument(rawHtml: null), "When html is null");
			Assert.Throws<ArgumentException>(() => GetNewRawSupportingDocument(rawHtml: ""), "When html is empty");
		}

		protected abstract IRawSupportingDocument GetNewRawSupportingDocument(string rawHtml);
	}
}
