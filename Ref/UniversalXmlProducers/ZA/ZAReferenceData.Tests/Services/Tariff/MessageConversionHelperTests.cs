using System;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common.Models;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Helpers;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Helpers;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Common;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff
{
	[TestFixture]
	public class MessageConversionHelperTests
	{
		[Test]
		public void Convert_Valid()
		{
			var msgContent = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff.TestFiles.Input.D96B_Valid_01.txt");
			var logger = new TestLogger();

			var msg = CreateMessage(msgContent);

			var header = MessageConversionHelper.Convert(msg, logger);

			Assert.That(header, Is.Not.Null);
			Assert.That(logger.ErrorString, Is.EqualTo(string.Empty));
			Assert.That(header.Tariffs, Is.Not.Null.And.Not.Empty);
		}

		[Test]
		public void Convert_Invalid()
		{
			var msgContent = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff.TestFiles.Input.D96B_Invalid.txt");
			var logger = new TestLogger();

			var msg = CreateMessage(msgContent);

			var header = MessageConversionHelper.Convert(msg, logger);

			Assert.That(header, Is.Null);
			Assert.That(logger.ErrorString, Contains.Substring("Expected 1 occurrence(s) of 'PIA' message section. LineNumber: 7141"));
		}

		SourceDataMessage CreateMessage(string content)
		{
			return new SourceDataMessage
			{
				Content = content,
				CreatedDate = DateTime.Now,
				ID = Guid.NewGuid(),
				PublishDate = DateTime.Today,
				Status = "QUE"
			};
		}
	}
}
