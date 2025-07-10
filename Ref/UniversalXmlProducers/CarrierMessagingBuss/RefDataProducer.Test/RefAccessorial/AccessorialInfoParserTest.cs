using System.Collections.Generic;
using CargoWise.RefDbRepo.CarrierMessagingBuss.Shared.Model;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CarrierMessagingBuss.RefDataProducer.Test
{
	class AccessorialInfoParserTest
	{
		[Test]
		public void Convert()
		{
			//Setup  
			var responseResults = new List<ResponseResult<AccessorialInfo[]>>
			{
				new()
				{
					Value =
					[
						new AccessorialInfo
						{
							Code = "Code1",
							Description = "Description1"
						}
					],
					IsSuccess = true,
					Message = "Success"
				},
				new()
				{
					Value =
					[
						new AccessorialInfo
						{
							Code = "Code2",
							Description = "Description2"
						},
						new AccessorialInfo
						{
							Code = "Code3",
							Description = "Description3"
						}
					],
					IsSuccess = true,
					Message = "Success"
				}
			};

			//Action  
			var data = AccessorialInfoParser.Convert(responseResults);

			//Assert  
			Assert.That(data.Count, Is.EqualTo(3));
			Assert.That(data[0].ASI_Code, Is.EqualTo("Code1"));
			Assert.That(data[0].ASI_Description, Is.EqualTo("Description1"));
			Assert.That(data[1].ASI_Code, Is.EqualTo("Code2"));
			Assert.That(data[1].ASI_Description, Is.EqualTo("Description2"));
			Assert.That(data[2].ASI_Code, Is.EqualTo("Code3"));
			Assert.That(data[2].ASI_Description, Is.EqualTo("Description3"));
		}
	}
}
