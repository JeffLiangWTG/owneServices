using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.SupportingDocument
{
	[TestFixture]
	class SupportingDocumentsProducerFixture
	{
		[Test]
		public void ConstructorGuardClause()
		{
			Assert.Throws<ArgumentNullException>(() => new SupportingDocumentsProducer(loader: null, mapper: new RawSupportingDocumentMapper()), "When loader is null");
			Assert.Throws<ArgumentNullException>(() => new SupportingDocumentsProducer(loader: rawSupportingDocumentsLoaderMock.Object, mapper: null), "When mapper is null");
		}

		[Test]
		public void ProduceEntities()
		{
			var rawDocuments = new List<IRawSupportingDocument>()
			{
				new Mock<IRawSupportingDocument>().Object,
				new Mock<IRawSupportingDocument>().Object,
			};

			rawSupportingDocumentsLoaderMock.Setup(x => x.GetRawSupportingDocuments())
				.Returns(rawDocuments);

			var supportingDocumentsProducer = new SupportingDocumentsProducer(rawSupportingDocumentsLoaderMock.Object, new RawSupportingDocumentMapper());
			Assert.AreEqual(6, supportingDocumentsProducer.ProduceEntities().Count(), "Number of produced RefCusCodeList elements");
		}

		[Test]
		public void ProduceEntities_WhenDocumentCategoryIsUnitedNationEdifact()
		{
			var rawSupportingDocument1 = new Mock<IRawSupportingDocument>();
			rawSupportingDocument1.Setup(x => x.Code)
				.Returns("0XXX");

			var rawSupportingDocument2 = new Mock<IRawSupportingDocument>();
			rawSupportingDocument2.Setup(x => x.Code)
				.Returns("NXXX");

			rawSupportingDocumentsLoaderMock.Setup(x => x.GetRawSupportingDocuments())
				.Returns(new[] { rawSupportingDocument1.Object, rawSupportingDocument2.Object });

			var supportingDocumentsProducer = new SupportingDocumentsProducer(rawSupportingDocumentsLoaderMock.Object, new RawSupportingDocumentMapper());
			Assert.AreEqual(7, supportingDocumentsProducer.ProduceEntities().Count(), "Number of produced RefCusCodeList elements");
		}

		[SetUp]
		protected void SetUp()
		{
			rawSupportingDocumentsLoaderMock = new Mock<IRawSupportingDocumentsLoader>();
		}

		Mock<IRawSupportingDocumentsLoader> rawSupportingDocumentsLoaderMock;
	}
}
