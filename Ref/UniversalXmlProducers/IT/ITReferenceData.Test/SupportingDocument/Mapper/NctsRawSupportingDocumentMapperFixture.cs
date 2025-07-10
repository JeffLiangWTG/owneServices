using System;
using System.Linq;
using CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument;
using Moq;
using NUnit.Framework;
using Types = CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument.Constants.RefCusCodeListCodeTypes;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.SupportingDocument
{
	[TestFixture]
	sealed class NctsRawSupportingDocumentMapperFixture
	{
		[Test]
		public void GetMappings()
		{
			var excludedCodeCollection = new[] { "XXX" };
			IRawSupportingDocumentMapper mapper = new NctsRawSupportingDocumentMapper(excludedCodeCollection);

			var testCode = "YYY";
			var testDescription = "Description";
			var testStartDate = new DateTime(2021, 2, 1);
			var testEndDate = new DateTime(2021, 12, 31, 23, 59, 59);

			var rawSupportingDocumentMock = new Mock<IRawSupportingDocument>();
			rawSupportingDocumentMock.Setup(x => x.Code).Returns(testCode);
			rawSupportingDocumentMock.Setup(x => x.Description).Returns(testDescription);
			rawSupportingDocumentMock.Setup(x => x.StartDate).Returns(testStartDate);
			rawSupportingDocumentMock.Setup(x => x.EndDate).Returns(testEndDate);

			var result = mapper.GetMappings(rawSupportingDocumentMock.Object);
			Assert.That(result.Count(), Is.EqualTo(1));

			var refCusCodeList = result.First();
			Assert.Multiple(() =>
			{
				Assert.That(refCusCodeList.ZZD_ZZK_NKCodeType, Is.EqualTo(Types.SupportingDocumentNcts));
				Assert.That(refCusCodeList.ZZD_Code, Is.EqualTo(testCode));
				Assert.That(refCusCodeList.ZZD_Description, Is.EqualTo(testDescription));
				Assert.That(refCusCodeList.ZZD_StartDate, Is.EqualTo(testStartDate));
				Assert.That(refCusCodeList.ZZD_EndDate, Is.EqualTo(testEndDate));
			});
		}

		[Test]
		public void GetMappings_WithCodeInExcludedCodesList_ShouldReturnEmptySet()
		{
			var excludedCodeCollection = new[] { "XXX" };
			IRawSupportingDocumentMapper mapper = new NctsRawSupportingDocumentMapper(excludedCodeCollection);

			var rawSupportingDocumentMock = new Mock<IRawSupportingDocument>();
			rawSupportingDocumentMock.Setup(x => x.Code).Returns("XXX");

			var result = mapper.GetMappings(rawSupportingDocumentMock.Object);
			Assert.That(result, Is.Empty);
		}
	}
}
