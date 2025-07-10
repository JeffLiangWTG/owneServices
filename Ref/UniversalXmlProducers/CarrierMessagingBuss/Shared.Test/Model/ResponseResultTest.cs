using System;
using System.Text.Json;
using CargoWise.RefDbRepo.CarrierMessagingBuss.RefDataProducer;
using CargoWise.RefDbRepo.CarrierMessagingBuss.Shared.Model;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CarrierMessagingBuss.Shared.Test
{
	[TestFixture]
	public class ResponseResultTest
	{
		[Test]
		public void SerializeResponseResult_ProducesCorrectJson()
		{
			// Arrange
			var responseResult = new ResponseResult<AccessorialInfo[]>
			{
				IsSuccess = true,
				Message = "Operation successful",
				Value =
				[
					new AccessorialInfo
					{
						Code = "AC1",
						Description = "Accessorial 1",
						LastModifiedDateTime = new DateTime(2023, 10, 1, 12, 0, 0)
					},
					new AccessorialInfo
					{
						Code = "AC2",
						Description = "Accessorial 2",
						LastModifiedDateTime = new DateTime(2023, 10, 2, 14, 30, 0)
					}
				]
			};

			var expectedJson = @"{""value"":[{""code"":""AC1"",""description"":""Accessorial 1"",""lastModifiedDateTime"":""2023-10-01T12:00:00""},{""code"":""AC2"",""description"":""Accessorial 2"",""lastModifiedDateTime"":""2023-10-02T14:30:00""}],""isSuccess"":true,""message"":""Operation successful""}";

			// Act
			var json = JsonSerializer.Serialize(responseResult);

			// Assert
			Assert.That(json, Is.EqualTo(expectedJson));
		}

		[Test]
		public void DeserializeResponseResult_ParsesCorrectly()
		{
			// Arrange
			var json = @"{
                ""value"": [
                    {
                        ""code"": ""AC1"",
                        ""description"": ""Accessorial 1"",
                        ""lastModifiedDateTime"": ""2023-10-01T12:00:00""
                    },
                    {
                        ""code"": ""AC2"",
                        ""description"": ""Accessorial 2"",
                        ""lastModifiedDateTime"": ""2023-10-02T14:30:00""
                    }
                ],
                ""isSuccess"": true,
                ""message"": ""Operation successful""
            }";

			// Act
			var responseResult = JsonSerializer.Deserialize<ResponseResult<AccessorialInfo[]>>(json);

			// Assert
			Assert.That(responseResult, Is.Not.Null);
			Assert.That(responseResult.IsSuccess, Is.True);
			Assert.That(responseResult.Message, Is.EqualTo("Operation successful"));
			Assert.That(responseResult.Value, Is.Not.Null);
			Assert.That(responseResult.Value.Length, Is.EqualTo(2));
			Assert.That(responseResult.Value[0].Code, Is.EqualTo("AC1"));
			Assert.That(responseResult.Value[0].Description, Is.EqualTo("Accessorial 1"));
			Assert.That(responseResult.Value[0].LastModifiedDateTime, Is.EqualTo(new DateTime(2023, 10, 1, 12, 0, 0)));
			Assert.That(responseResult.Value[1].Code, Is.EqualTo("AC2"));
			Assert.That(responseResult.Value[1].Description, Is.EqualTo("Accessorial 2"));
			Assert.That(responseResult.Value[1].LastModifiedDateTime, Is.EqualTo(new DateTime(2023, 10, 2, 14, 30, 0)));
		}
	}
}
