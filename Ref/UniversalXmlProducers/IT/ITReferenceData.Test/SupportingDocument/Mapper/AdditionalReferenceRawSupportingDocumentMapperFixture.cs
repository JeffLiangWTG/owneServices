using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument;
using Moq;
using NUnit.Framework;
using Types = CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument.Constants.RefCusCodeListCodeTypes;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.SupportingDocument
{
	[TestFixture]
	sealed class AdditionalReferenceRawSupportingDocumentMapperFixture
	{
		[Test]
		public void Constructor_WithNullCodesCollection_ShouldThrowException()
		{
			Assert.Throws<ArgumentNullException>(() => new AdditionalReferenceRawSupportingDocumentMapper(null));
		}

		[Test]
		public void GetMappings_WithNullItem_ShouldThrowException()
		{
			var mapper = new AdditionalReferenceRawSupportingDocumentMapper(Enumerable.Empty<string>()) as IRawSupportingDocumentMapper;
			Assert.Throws<ArgumentNullException>(() => mapper.GetMappings(null));
		}

		[Test]
		public void GetMappings_WithValidRawSupportingDocument_ShouldMapToAdditionalReferenceRefCusCodeList()
		{
			var testCode = "Y006";
			var testDescription = "Stamp (at beginning/end of each piece) and directly transported";
			var testStartDate = new DateTime(2018, 2, 1);
			var testEndDate = new DateTime(2038, 12, 31, 23, 59, 59);

			var rawSupportingDocumentMock = new Mock<IRawSupportingDocument>();
			rawSupportingDocumentMock.Setup(x => x.Code).Returns(testCode);
			rawSupportingDocumentMock.Setup(x => x.Description).Returns(testDescription);
			rawSupportingDocumentMock.Setup(x => x.StartDate).Returns(testStartDate);
			rawSupportingDocumentMock.Setup(x => x.EndDate).Returns(testEndDate);
			rawSupportingDocumentMock.Setup(x => x.IsCertificateIdRequired).Returns(true);

			var mapper = new AdditionalReferenceRawSupportingDocumentMapper(new[] { testCode }) as IRawSupportingDocumentMapper;

			var result = mapper.GetMappings(rawSupportingDocumentMock.Object);
			Assert.That(result.Count(), Is.EqualTo(1));

			var refCusCodeList = result.First();
			Assert.Multiple(() =>
			{
				Assert.That(refCusCodeList.ZZD_ZZK_NKCodeType, Is.EqualTo(Types.SupportingDocumentAdditionalReference));
				Assert.That(refCusCodeList.ZZD_Code, Is.EqualTo(testCode));
				Assert.That(refCusCodeList.ZZD_Description, Is.EqualTo(testDescription));
				Assert.That(refCusCodeList.ZZD_StartDate, Is.EqualTo(testStartDate));
				Assert.That(refCusCodeList.ZZD_EndDate, Is.EqualTo(testEndDate));
				AssertHasAttribute(refCusCodeList, "ReferenceNumber", "Y");
			});
		}

		[Test]
		public void GetMappings_WithCodeNotInEuAdditionalReferenceCodes_ShouldReturnEmptySet()
		{
			var rawSupportingDocumentMock = new Mock<IRawSupportingDocument>();
			rawSupportingDocumentMock.Setup(x => x.Code).Returns("XXX");

			var mapper = new AdditionalReferenceRawSupportingDocumentMapper(new[] { "XYZ" }) as IRawSupportingDocumentMapper;

			var result = mapper.GetMappings(rawSupportingDocumentMock.Object);
			Assert.That(result, Is.Empty);
		}

		void AssertHasAttribute(RefCusCodeList refCusCodeList, string expectedName, string expectedValue)
		{
			var attribute = refCusCodeList
				.RefCusCodeListAttributes.Cast<RefCusCodeListAttribute>()
				.SingleOrDefault(x => x.ZZE_ZXE_NKName == expectedName && x.ZZE_Value == expectedValue);
			Assert.IsNotNull(attribute, $"Attributes['{expectedName}']");
		}
	}
}
