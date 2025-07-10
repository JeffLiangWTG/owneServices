using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.SupportingDocument
{
	[TestFixture]
	class RawSupportingDocumentMapperFixture
	{
		[Test]
		public void MapGuardClause()
		{
			Assert.Throws<ArgumentNullException>(() => mapper.GetMappings(rawSupportingDocument: null).ToArray(), "When rawSupportingDocument is null");
		}

		[Test]
		public void MapCode()
		{
			rawSupportingDocumentMock.Setup(x => x.Code).Returns("CODE");
			var refCusCodeList = mapper.GetMappings(rawSupportingDocumentMock.Object).First();
			Assert.AreEqual("CODE", refCusCodeList.ZZD_Code, nameof(refCusCodeList.ZZD_Code));
		}

		[Test]
		public void MapDescription()
		{
			rawSupportingDocumentMock.Setup(x => x.Description).Returns("DESCRIPTION");
			var refCusCodeList = mapper.GetMappings(rawSupportingDocumentMock.Object).First();
			Assert.AreEqual("DESCRIPTION", refCusCodeList.ZZD_Description, nameof(refCusCodeList.ZZD_Description));
		}

		[Test]
		public void MapStartDate()
		{
			rawSupportingDocumentMock.Setup(x => x.StartDate).Returns(new DateTime(2022, 01, 01));
			var refCusCodeList = mapper.GetMappings(rawSupportingDocumentMock.Object).First();
			Assert.AreEqual(new DateTime(2022, 01, 01), refCusCodeList.ZZD_StartDate, nameof(refCusCodeList.ZZD_StartDate));
		}

		[Test]
		public void MapStartDateWhenNull()
		{
			rawSupportingDocumentMock.Setup(x => x.StartDate).Returns((DateTime?)null);
			var refCusCodeList = mapper.GetMappings(rawSupportingDocumentMock.Object).First();
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusCodeList.ZZD_StartDate, nameof(refCusCodeList.ZZD_StartDate));
		}

		[Test]
		public void MapStartDateDefaultedToMinSmallDateTime()
		{
			rawSupportingDocumentMock.Setup(x => x.StartDate).Returns(new DateTime(1899, 12, 31));
			var refCusCodeList = mapper.GetMappings(rawSupportingDocumentMock.Object).First();
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusCodeList.ZZD_StartDate, nameof(refCusCodeList.ZZD_StartDate));
		}

		[Test]
		public void MapEndDate()
		{
			rawSupportingDocumentMock.Setup(x => x.EndDate).Returns(new DateTime(2022, 12, 31));
			var refCusCodeList = mapper.GetMappings(rawSupportingDocumentMock.Object).First();
			Assert.AreEqual(new DateTime(2022, 12, 31), refCusCodeList.ZZD_EndDate, nameof(refCusCodeList.ZZD_EndDate));
		}

		[Test]
		public void MapEndDateWhenNull()
		{
			rawSupportingDocumentMock.Setup(x => x.EndDate).Returns((DateTime?)null);
			var refCusCodeList = mapper.GetMappings(rawSupportingDocumentMock.Object).First();
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusCodeList.ZZD_EndDate, nameof(refCusCodeList.ZZD_EndDate));
		}

		[Test]
		public void MapEndDateDefaultedToMaxSmallDateTime()
		{
			rawSupportingDocumentMock.Setup(x => x.EndDate).Returns(new DateTime(9999, 12, 31));
			var refCusCodeList = mapper.GetMappings(rawSupportingDocumentMock.Object).First();
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusCodeList.ZZD_EndDate, nameof(refCusCodeList.ZZD_EndDate));
		}

		[Test]
		public void MapRetroactiveAttribute()
		{
			Assert.Multiple(() =>
			{
				rawSupportingDocumentMock.Setup(x => x.IsRetroActiveRequired).Returns(false);
				var refCusCodeList = mapper.GetMappings(rawSupportingDocumentMock.Object).First();
				AssertDoesNotHaveAttribute(refCusCodeList, "Retroactive");

				rawSupportingDocumentMock.Setup(x => x.IsRetroActiveRequired).Returns(true);
				refCusCodeList = mapper.GetMappings(rawSupportingDocumentMock.Object).First();
				AssertHasAttribute(refCusCodeList, "Retroactive", "Y");
			});
		}

		[Test]
		public void MapYearAttribute()
		{
			Assert.Multiple(() =>
			{
				rawSupportingDocumentMock.Setup(x => x.IsYearRequired).Returns(false);
				var refCusCodeList = mapper.GetMappings(rawSupportingDocumentMock.Object).First();
				AssertDoesNotHaveAttribute(refCusCodeList, "Year");

				rawSupportingDocumentMock.Setup(x => x.IsYearRequired).Returns(true);
				refCusCodeList = mapper.GetMappings(rawSupportingDocumentMock.Object).First();
				AssertHasAttribute(refCusCodeList, "Year", "Y");
			});
		}

		[Test]
		public void MapCountryAttribute()
		{
			Assert.Multiple(() =>
			{
				rawSupportingDocumentMock.Setup(x => x.IsCountryRequired).Returns(false);
				var refCusCodeList = mapper.GetMappings(rawSupportingDocumentMock.Object).First();
				AssertDoesNotHaveAttribute(refCusCodeList, "Country");

				rawSupportingDocumentMock.Setup(x => x.IsCountryRequired).Returns(true);
				refCusCodeList = mapper.GetMappings(rawSupportingDocumentMock.Object).First();
				AssertHasAttribute(refCusCodeList, "Country", "Y");
			});
		}

		[Test]
		public void MapReferenceNumberAttributeForNationalSupportingDocument()
		{
			Assert.Multiple(() =>
			{
				rawSupportingDocumentMock.Setup(x => x.Type).Returns(SupportingDocumentType.National);
				rawSupportingDocumentMock.Setup(x => x.IsCertificateIdRequired).Returns(false);
				var refCusCodeList = mapper.GetMappings(rawSupportingDocumentMock.Object).First();
				AssertDoesNotHaveAttribute(refCusCodeList, "ReferenceNumber");

				rawSupportingDocumentMock.Setup(x => x.Type).Returns(SupportingDocumentType.National);
				rawSupportingDocumentMock.Setup(x => x.IsCertificateIdRequired).Returns(true);
				refCusCodeList = mapper.GetMappings(rawSupportingDocumentMock.Object).First();
				AssertHasAttribute(refCusCodeList, "ReferenceNumber", "Y");
			});
		}

		[TestCase("DC44I", TestName = "DoNotMapReferenceNumberAttributeForEuropeanSupportingDocument_WhenCodeTypeIsDC44I")]
		[TestCase("DC44E", TestName = "DoNotMapReferenceNumberAttributeForEuropeanSupportingDocument_WhenCodeTypeIsDC44E")]
		public void DoNotMapReferenceNumberAttributeForEuropeanSupportingDocument(string codeType)
		{
			rawSupportingDocumentMock.Setup(x => x.Type).Returns(SupportingDocumentType.European);

			Assert.Multiple(() =>
				{
					rawSupportingDocumentMock.Setup(x => x.IsCertificateIdRequired).Returns(false);
					var refCusCodeList = mapper.GetMappings(rawSupportingDocumentMock.Object).Single(x => x.ZZD_ZZK_NKCodeType == codeType);
					AssertDoesNotHaveAttribute(refCusCodeList, "ReferenceNumber");

					rawSupportingDocumentMock.Setup(x => x.IsCertificateIdRequired).Returns(true);
					refCusCodeList = mapper.GetMappings(rawSupportingDocumentMock.Object).Single(x => x.ZZD_ZZK_NKCodeType == codeType);
					AssertDoesNotHaveAttribute(refCusCodeList, "ReferenceNumber");
				});
		}

		[TestCase("DC44N", TestName = "MapReferenceNumberAttributeForEuropeanSupportingDocument_WhenCodeTypeIsDC44N")]
		[TestCase("DC44T", TestName = "MapReferenceNumberAttributeForEuropeanSupportingDocument_WhenCodeTypeIsDC44T")]
		public void MapReferenceNumberAttributeForEuropeanSupportingDocument(string codeType)
		{
			mapper = new RawSupportingDocumentMapper(validEuNctsSupportingDocumentCodes: new string[] { "NXXX" });

			rawSupportingDocumentMock.Setup(x => x.Code).Returns("NXXX");
			rawSupportingDocumentMock.Setup(x => x.Type).Returns(SupportingDocumentType.European);

			Assert.Multiple(() =>
			{
				rawSupportingDocumentMock.Setup(x => x.IsCertificateIdRequired).Returns(false);
				var refCusCodeList = mapper.GetMappings(rawSupportingDocumentMock.Object).Single(x => x.ZZD_ZZK_NKCodeType == codeType);
				AssertDoesNotHaveAttribute(refCusCodeList, "ReferenceNumber");

				rawSupportingDocumentMock.Setup(x => x.IsCertificateIdRequired).Returns(true);
				refCusCodeList = mapper.GetMappings(rawSupportingDocumentMock.Object).Single(x => x.ZZD_ZZK_NKCodeType == codeType);
				AssertHasAttribute(refCusCodeList, "ReferenceNumber", "Y");
			});
		}

		[Test]
		public void MapQuantityAttribute()
		{
			Assert.Multiple(() =>
			{
				rawSupportingDocumentMock.Setup(x => x.IsQuantityRequired).Returns(false);
				var refCusCodeList = mapper.GetMappings(rawSupportingDocumentMock.Object).First();
				AssertDoesNotHaveAttribute(refCusCodeList, "Quantity");

				rawSupportingDocumentMock.Setup(x => x.IsQuantityRequired).Returns(true);
				refCusCodeList = mapper.GetMappings(rawSupportingDocumentMock.Object).First();
				AssertHasAttribute(refCusCodeList, "Quantity", "Y");
			});
		}

		[Test]
		public void MapUnitOfQuantityAttribute()
		{
			Assert.Multiple(() =>
			{
				rawSupportingDocumentMock.Setup(x => x.IsUnitOfQuantityRequired).Returns(false);
				var refCusCodeList = mapper.GetMappings(rawSupportingDocumentMock.Object).First();
				AssertDoesNotHaveAttribute(refCusCodeList, "UnitOfQuantity");

				rawSupportingDocumentMock.Setup(x => x.IsUnitOfQuantityRequired).Returns(true);
				refCusCodeList = mapper.GetMappings(rawSupportingDocumentMock.Object).First();
				AssertHasAttribute(refCusCodeList, "UnitOfQuantity", "Y");
			});
		}

		[Test]
		public void MapElectronicFolderAttribute()
		{
			Assert.Multiple(() =>
			{
				rawSupportingDocumentMock.Setup(x => x.IsElectronicFolderRequired).Returns(false);
				var refCusCodeList = mapper.GetMappings(rawSupportingDocumentMock.Object).First();
				AssertDoesNotHaveAttribute(refCusCodeList, "ElectronicFolder");

				rawSupportingDocumentMock.Setup(x => x.IsElectronicFolderRequired).Returns(true);
				refCusCodeList = mapper.GetMappings(rawSupportingDocumentMock.Object).First();
				AssertHasAttribute(refCusCodeList, "ElectronicFolder", "Y");
			});
		}

		[Test]
		public void MapElectronicFolderNoteAttribute()
		{
			Assert.Multiple(() =>
			{
				rawSupportingDocumentMock.Setup(x => x.ElectronicFolderNote).Returns("");
				var refCusCodeList = mapper.GetMappings(rawSupportingDocumentMock.Object).First();
				AssertDoesNotHaveAttribute(refCusCodeList, "ElectronicFolderNote");

				rawSupportingDocumentMock.Setup(x => x.ElectronicFolderNote).Returns("some notes");
				refCusCodeList = mapper.GetMappings(rawSupportingDocumentMock.Object).First();
				AssertHasAttribute(refCusCodeList, "ElectronicFolderNote", "some notes");
			});
		}

		[Test]
		public void MapPaperFolderAttribute()
		{
			Assert.Multiple(() =>
			{
				rawSupportingDocumentMock.Setup(x => x.IsPaperFolderRequired).Returns(false);
				var refCusCodeList = mapper.GetMappings(rawSupportingDocumentMock.Object).First();
				AssertDoesNotHaveAttribute(refCusCodeList, "PaperFolder");

				rawSupportingDocumentMock.Setup(x => x.IsPaperFolderRequired).Returns(true);
				refCusCodeList = mapper.GetMappings(rawSupportingDocumentMock.Object).First();
				AssertHasAttribute(refCusCodeList, "PaperFolder", "Y");
			});
		}

		[Test]
		public void MapPaperFolderNoteAttribute()
		{
			Assert.Multiple(() =>
			{
				rawSupportingDocumentMock.Setup(x => x.PaperFolderNote).Returns("");
				var refCusCodeList = mapper.GetMappings(rawSupportingDocumentMock.Object).First();
				AssertDoesNotHaveAttribute(refCusCodeList, "PaperFolderNote");

				rawSupportingDocumentMock.Setup(x => x.PaperFolderNote).Returns("some notes");
				refCusCodeList = mapper.GetMappings(rawSupportingDocumentMock.Object).First();
				AssertHasAttribute(refCusCodeList, "PaperFolderNote", "some notes");
			});
		}

		[Test]
		public void OneEntryEachSupportedCodeType()
		{
			Assert.Multiple(() =>
			{
				var refCusCodeList = mapper.GetMappings(rawSupportingDocumentMock.Object).ToArray();
				CollectionAssert.AreEqual(
					new[] { "DC44I", "DC44E", "DC44N" },
					refCusCodeList.Select(x => x.ZZD_ZZK_NKCodeType),
					"Mapped RefCusCodeList Items");
			});
		}

		[Test]
		public void TestSupportingDocumentTypeForDC44N()
		{
			rawSupportingDocumentMock.Setup(x => x.Type).Returns(SupportingDocumentType.National);

			Assert.Multiple(() =>
			{
				var refCusCodeList = mapper.GetMappings(rawSupportingDocumentMock.Object).ToArray();
				CollectionAssert.AreEqual(
					new[] { "DC44I", "DC44E", "DC44N" },
					refCusCodeList.Select(x => x.ZZD_ZZK_NKCodeType),
					"Mapped RefCusCodeList Items for National Supporting Document");
			});

			rawSupportingDocumentMock.Setup(x => x.Type).Returns(SupportingDocumentType.European);
			rawSupportingDocumentMock.Setup(x => x.Code).Returns("XXX");
			var refDataRepoEuropeanCodeCollection = new string[] { "C085", "C400" };

			mapper = new RawSupportingDocumentMapper(refDataRepoEuropeanCodeCollection);

			Assert.Multiple(() =>
			{
				var refCusCodeList = mapper.GetMappings(rawSupportingDocumentMock.Object).ToArray();
				CollectionAssert.AreEqual(
					new[] { "DC44I", "DC44E" },
					refCusCodeList.Select(x => x.ZZD_ZZK_NKCodeType),
					"Mapped RefCusCodeList Items for European Supporting Document");
			});

			rawSupportingDocumentMock.Setup(x => x.Code).Returns("C400");
			Assert.Multiple(() =>
			{
				var refCusCodeList = mapper.GetMappings(rawSupportingDocumentMock.Object).ToArray();
				CollectionAssert.AreEqual(
					new[] { "DC44I", "DC44E", "DC44N" },
					refCusCodeList.Select(x => x.ZZD_ZZK_NKCodeType),
					"Mapped RefCusCodeList Items for European Supporting Document");
			});
		}

		[Test]
		public void OneEntryEachSupportedCodeType_WhenDocumentCategoryIsUnitedNationEdifact()
		{
			rawSupportingDocumentMock.Setup(x => x.Code)
				.Returns("NXXX");

			Assert.Multiple(() =>
			{
				var refCusCodeList = mapper.GetMappings(rawSupportingDocumentMock.Object).ToArray();
				CollectionAssert.AreEqual(
					new[] { "DC44I", "DC44E", "DC44N", "DC44T" },
					refCusCodeList.Select(x => x.ZZD_ZZK_NKCodeType),
					"Mapped RefCusCodeList Items");
			});
		}

		void AssertDoesNotHaveAttribute(RefCusCodeList refCusCodeList, string expectedName)
		{
			var attribute = refCusCodeList
				.RefCusCodeListAttributes.Cast<RefCusCodeListAttribute>()
				.SingleOrDefault(x => x.ZZE_ZXE_NKName == expectedName);
			Assert.IsNull(attribute, $"Attributes['{expectedName}']");
		}

		void AssertHasAttribute(RefCusCodeList refCusCodeList, string expectedName, string expectedValue)
		{
			var attribute = refCusCodeList
				.RefCusCodeListAttributes.Cast<RefCusCodeListAttribute>()
				.SingleOrDefault(x => x.ZZE_ZXE_NKName == expectedName && x.ZZE_Value == expectedValue);
			Assert.IsNotNull(attribute, $"Attributes['{expectedName}']");
		}

		[SetUp]
		protected void SetUp()
		{
			rawSupportingDocumentMock = new Mock<IRawSupportingDocument>();
			mapper = new RawSupportingDocumentMapper();
		}

		Mock<IRawSupportingDocument> rawSupportingDocumentMock;
		IRawSupportingDocumentMapper mapper;
	}
}
