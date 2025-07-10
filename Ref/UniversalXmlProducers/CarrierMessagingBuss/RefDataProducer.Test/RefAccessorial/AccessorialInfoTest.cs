using System;
using System.Text.Json;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CarrierMessagingBuss.RefDataProducer.Test
{
	[TestFixture]
	public class AccessorialInfoTest
	{
		[Test]
		public void SerializeAccessorialInfo_ProducesCorrectJson()
		{
			// Arrange
			var accessorialInfo = new AccessorialInfo
			{
				Code = "AC1",
				Description = "Accessorial 1",
				LastModifiedDateTime = new DateTime(2023, 10, 1, 12, 0, 0)
			};

			var expectedJson = @"{""code"":""AC1"",""description"":""Accessorial 1"",""lastModifiedDateTime"":""2023-10-01T12:00:00""}";

			// Act
			var json = JsonSerializer.Serialize(accessorialInfo);

			// Assert
			Assert.That(json, Is.EqualTo(expectedJson));
		}

		[Test]
		public void DeserializeAccessorialInfo_ParsesCorrectly()
		{
			// Arrange
			var json = @"{
                ""code"": ""AC1"",
                ""description"": ""Accessorial 1"",
                ""lastModifiedDateTime"": ""2023-10-01T12:00:00""
            }";

			// Act
			var accessorialInfo = JsonSerializer.Deserialize<AccessorialInfo>(json);

			// Assert
			Assert.That(accessorialInfo, Is.Not.Null);
			Assert.That(accessorialInfo.Code, Is.EqualTo("AC1"));
			Assert.That(accessorialInfo.Description, Is.EqualTo("Accessorial 1"));
			Assert.That(accessorialInfo.LastModifiedDateTime, Is.EqualTo(new DateTime(2023, 10, 1, 12, 0, 0)));
		}
	}
}
