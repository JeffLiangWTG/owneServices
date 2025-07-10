using System;
using System.Linq;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Schema_New.Test
{
	[TestFixture]
	class UpdatedTariffWatermarkFixture
	{
		[Test]
		public void MultipleUpdatesResultInOnlyOneObject()
		{
			var now = DateTime.Now;
			var waterMark = new ProcessDataHelper(stagingMock.Object, "XX", "YY");
			waterMark.Update(now.AddHours(-2));
			waterMark.Update(now);
			stagingMock.Verify(x => x.Add(It.IsAny<ProcessData>()), Times.Once);
		}

		[Test]
		public void Reset()
		{
			var waterMarkObj = new ProcessData
			{
				Name = "XX",
				Type = "YY"
			};
			stagingMock.Setup(x => x.Get<ProcessData>()).Returns(new[] { waterMarkObj }.AsQueryable());
			var waterMark = new ProcessDataHelper(stagingMock.Object, "XX", "YY");
			waterMark.Reset();
			stagingMock.Verify(x => x.Remove(waterMarkObj));
		}

		[Test]
		public void Update()
		{
			var now = DateTime.Now;
			stagingMock.Setup(x => x.Get<ProcessData>()).Returns(new ProcessData[0].AsQueryable());
			var waterMark = new ProcessDataHelper(stagingMock.Object, "XX", "YY");
			waterMark.Update(now);
			stagingMock.Verify(x => x.Add(It.Is<ProcessData>(w => w.Name == "XX"
				&& w.Type == "YY" && w.Data == JsonConvert.ToString(now))));
			var waterMarkObj = new ProcessData
			{
				Name = "XX",
				Type = "YY"
			};
			waterMark = new ProcessDataHelper(stagingMock.Object, "XX", "YY");
			stagingMock.Setup(x => x.Get<ProcessData>()).Returns(new[] { waterMarkObj }.AsQueryable());
			waterMark.Update(now);
			Assert.That(waterMarkObj.Data, Is.EqualTo(JsonConvert.ToString(now)));
		}

		[Test]
		public void Get()
		{
			var now = DateTime.Now;
			stagingMock.Setup(x => x.Get<ProcessData>()).Returns(new ProcessData[0].AsQueryable());
			var waterMark = new ProcessDataHelper(stagingMock.Object, "XX", "YY");
			var value = DateTime.MinValue;
			Assert.That(waterMark.TryToGet(out value), Is.False);
			var waterMarkObj = new ProcessData
			{
				Name = "XX",
				Type = "YY",
				Data = JsonConvert.ToString(now)
			};
			stagingMock.Setup(x => x.Get<ProcessData>()).Returns(new[] { waterMarkObj }.AsQueryable());
			waterMark.TryToGet(out value);
			Assert.That(value, Is.EqualTo(now));
		}

		Mock<IStagingRepository> stagingMock;

		[SetUp]
		public void SetUp()
		{
			stagingMock = new Mock<IStagingRepository>();
		}
	}
}
