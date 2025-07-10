using System;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.HealthChecker.Test
{
	[TestFixture]
	class SourceDataCheckerFixture
	{
		[Test]
		public void CheckLongRunningSourceData()
		{
			var now = DateTime.UtcNow;
			var stagingRepo = new Mock<IStagingRepository>();
			var sourceData1 = new SourceData { SDA_PK = Guid.NewGuid(), SDA_Source = "UPL", SDA_Filetype = "XML", SDA_SubSource = "SourceA", SDA_ContentText = "AAA", SDA_Status = "PRS", SDA_CreatedTime = now.AddDays(-2) };
			var sourceData2 = new SourceData { SDA_PK = Guid.NewGuid(), SDA_Source = "INT", SDA_Filetype = "TXT", SDA_SubSource = "SourceB", SDA_ContentText = "BBB", SDA_Status = "PRS", SDA_CreatedTime = now.AddHours(-25) };
			var sourceData3 = new SourceData { SDA_PK = Guid.NewGuid(), SDA_Source = "INT", SDA_Filetype = "XML", SDA_SubSource = "SourceC", SDA_ContentText = "CCC", SDA_Status = "PRS", SDA_CreatedTime = now };
			var sourceData4 = new SourceData { SDA_PK = Guid.NewGuid(), SDA_Source = "INT", SDA_Filetype = "XML", SDA_SubSource = "SourceD", SDA_ContentText = "DDD", SDA_Status = "MER", SDA_CreatedTime = now.AddHours(-25) };
			stagingRepo.Setup(x => x.Get<SourceData>()).Returns(new[] { sourceData1, sourceData2, sourceData3, sourceData4 }.AsQueryable());

			var originalOut = Console.Out;
			var originalError = Console.Error;
			var sourceDataChecker = new SourceDataChecker(stagingRepo.Object);
			try
			{
				using (var outWriter = new StringWriter())
				using (var errorWriter = new StringWriter())
				{
					Console.SetOut(outWriter);
					Console.SetError(errorWriter);
					sourceDataChecker.CheckLongRunningSourceData();
					Assert.True(outWriter.ToString().Contains("All xml files finished processing within 24 hours."));
					Assert.True(string.IsNullOrEmpty(errorWriter.ToString()));

					var sda_PK = Guid.Parse("E3635ACC-976B-441B-ABE4-67E40317EB52");
					var sourceData5 = new SourceData { SDA_PK = sda_PK, SDA_Source = "INT", SDA_Filetype = "XML", SDA_SubSource = "SourceE", SDA_ContentText = "EEE", SDA_Status = "PRS", SDA_CreatedTime = now.AddHours(-25) };
					stagingRepo.Setup(x => x.Get<SourceData>()).Returns(new[] { sourceData1, sourceData2, sourceData3, sourceData4, sourceData5 }.AsQueryable());
					sourceDataChecker.CheckLongRunningSourceData();
					Assert.True(errorWriter.ToString().Contains($"The following xml files did not finish processing after 24 hours. SDA_PK: {sda_PK}"));
				}
			}
			finally
			{
				Console.SetOut(originalOut);
				Console.SetError(originalError);
			}
		}

		[Test]
		public void CheckSourceDataStatus()
		{
			var now = DateTime.UtcNow;
			var stagingRepo = new Mock<IStagingRepository>();
			var sourceData1 = new SourceData { SDA_PK = Guid.NewGuid(), SDA_Source = "INT", SDA_SubSource = "SourceA", SDA_ContentText = "AAA", SDA_Status = "ERR", SDA_CreatedTime = now.AddHours(-5.1) };
			var sourceData2 = new SourceData { SDA_PK = Guid.NewGuid(), SDA_Source = "INT", SDA_SubSource = "SourceA", SDA_ContentText = "BBB", SDA_Status = "MER", SDA_CreatedTime = now.AddHours(-5.1) };
			var sourceData3 = new SourceData { SDA_PK = Guid.NewGuid(), SDA_Source = "INT", SDA_SubSource = "SourceA", SDA_ContentText = "BBB", SDA_Status = "PRS", SDA_CreatedTime = now };
			stagingRepo.Setup(x => x.Get<SourceData>()).Returns(new[] { sourceData1, sourceData2, sourceData3 }.AsQueryable());

			var originalOut = Console.Out;
			var originalError = Console.Error;
			var sourceDataChecker = new SourceDataChecker(stagingRepo.Object);
			try
			{
				using (var outWriter = new StringWriter())
				using (var errorWriter = new StringWriter())
				{
					Console.SetOut(outWriter);
					Console.SetError(errorWriter);
					sourceDataChecker.CheckSourceDataStatus(5, 1.0);
					Assert.True(outWriter.ToString().Contains("SourceData process results in last 5 hours: 0 succeeded, 0 failed."));
					Assert.True(string.IsNullOrEmpty(errorWriter.ToString()));

					var sourceData4 = new SourceData { SDA_PK = Guid.NewGuid(), SDA_Source = "INT", SDA_SubSource = "SourceB", SDA_ContentText = "BBB", SDA_Status = "ERR", SDA_CreatedTime = now.AddHours(-1) };
					var sourceData5 = new SourceData { SDA_PK = Guid.NewGuid(), SDA_Source = "INT", SDA_SubSource = "SourceB", SDA_ContentText = "BBB", SDA_Status = "FIE", SDA_CreatedTime = now.AddHours(-1) };
					var sourceData6 = new SourceData { SDA_PK = Guid.NewGuid(), SDA_Source = "INT", SDA_SubSource = "SourceC", SDA_ContentText = "CCC", SDA_Status = "MER", SDA_CreatedTime = now.AddHours(-2) };
					var sourceData7 = new SourceData { SDA_PK = Guid.NewGuid(), SDA_Source = "INT", SDA_SubSource = "SourceC", SDA_ContentText = "CCC", SDA_Status = "FIN", SDA_CreatedTime = now.AddHours(-2) };
					stagingRepo.Setup(x => x.Get<SourceData>()).Returns(new[] { sourceData4, sourceData5, sourceData6, sourceData7 }.AsQueryable());
					sourceDataChecker.CheckSourceDataStatus(3, 1.1);
					Assert.True(outWriter.ToString().Contains("SourceData process results in last 3 hours: 2 succeeded, 2 failed."));
					Assert.True(string.IsNullOrEmpty(errorWriter.ToString()));

					sourceDataChecker.CheckSourceDataStatus(3, 1.0);
					Assert.True(errorWriter.ToString().Contains("SourceData failure ratio reached 1 in last 3 hours. 2 succeeded, 2 failed."));
				}
			}
			finally
			{
				Console.SetOut(originalOut);
				Console.SetError(originalError);
			}
		}
	}
}
