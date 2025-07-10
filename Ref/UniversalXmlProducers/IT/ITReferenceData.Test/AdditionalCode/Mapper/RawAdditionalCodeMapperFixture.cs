using System;
using CargoWise.RefDbRepo.ITReferenceData.Business.AdditionalCode;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.AdditionalCode
{
	[TestFixture]
	class RawAdditionalCodeMapperFixture
	{
		[Test]
		public void MapGuardClause()
		{
			Assert.Throws<ArgumentNullException>(() => mapper.GetMapping(rawAdditionalCode: null), "When rawAdditionalCode is null");
		}

		[Test]
		public void MapCode()
		{
			rawAdditionalCodeMock.Setup(x => x.Code).Returns("CODE");
			var refCusCodeList = mapper.GetMapping(rawAdditionalCodeMock.Object);
			Assert.AreEqual("CODE", refCusCodeList.ZZD_Code, nameof(refCusCodeList.ZZD_Code));
		}

		[Test]
		public void MapDescription()
		{
			rawAdditionalCodeMock.Setup(x => x.Description).Returns("DESCRIPTION");
			var refCusCodeList = mapper.GetMapping(rawAdditionalCodeMock.Object);
			Assert.AreEqual("DESCRIPTION", refCusCodeList.ZZD_Description, nameof(refCusCodeList.ZZD_Description));
		}

		[Test]
		public void MapStartDate()
		{
			rawAdditionalCodeMock.Setup(x => x.StartDate).Returns(new DateTime(2022, 01, 01));
			var refCusCodeList = mapper.GetMapping(rawAdditionalCodeMock.Object);
			Assert.AreEqual(new DateTime(2022, 01, 01), refCusCodeList.ZZD_StartDate, nameof(refCusCodeList.ZZD_StartDate));
		}

		[Test]
		public void MapStartDateWhenNull()
		{
			rawAdditionalCodeMock.Setup(x => x.StartDate).Returns((DateTime?)null);
			var refCusCodeList = mapper.GetMapping(rawAdditionalCodeMock.Object);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusCodeList.ZZD_StartDate, nameof(refCusCodeList.ZZD_StartDate));
		}

		[Test]
		public void MapStartDateDefaultedToMinSmallDateTime()
		{
			rawAdditionalCodeMock.Setup(x => x.StartDate).Returns(new DateTime(1899, 12, 31));
			var refCusCodeList = mapper.GetMapping(rawAdditionalCodeMock.Object);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusCodeList.ZZD_StartDate, nameof(refCusCodeList.ZZD_StartDate));
		}

		[Test]
		public void MapEndDate()
		{
			rawAdditionalCodeMock.Setup(x => x.EndDate).Returns(new DateTime(2022, 12, 31));
			var refCusCodeList = mapper.GetMapping(rawAdditionalCodeMock.Object);
			Assert.AreEqual(new DateTime(2022, 12, 31), refCusCodeList.ZZD_EndDate, nameof(refCusCodeList.ZZD_EndDate));
		}

		[Test]
		public void MapEndDateWhenNull()
		{
			rawAdditionalCodeMock.Setup(x => x.EndDate).Returns((DateTime?)null);
			var refCusCodeList = mapper.GetMapping(rawAdditionalCodeMock.Object);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusCodeList.ZZD_EndDate, nameof(refCusCodeList.ZZD_EndDate));
		}

		[Test]
		public void MapEndDateDefaultedToMaxSmallDateTime()
		{
			rawAdditionalCodeMock.Setup(x => x.EndDate).Returns(new DateTime(9999, 12, 31));
			var refCusCodeList = mapper.GetMapping(rawAdditionalCodeMock.Object);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusCodeList.ZZD_EndDate, nameof(refCusCodeList.ZZD_EndDate));
		}

		[SetUp]
		protected void SetUp()
		{
			rawAdditionalCodeMock = new Mock<IRawAdditionalCode>();
			mapper = new RawAdditionalCodeMapper();
		}

		Mock<IRawAdditionalCode> rawAdditionalCodeMock;
		IRawAdditionalCodeMapper mapper;
	}
}
