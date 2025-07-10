using System;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.Common
{
	[TestFixture]
	class DeveloperTests
	{
		[Test]
		[Explicit("Debug/Developer Test")]
		public void GetDatedContentAsString()
		{
			var webClientWrapper = new Services.Common.WebClientWrapper();

			var result = webClientWrapper.GetDatedContent("http://www.hmrc.gov.uk/softwaredevelopers/rates/exrates-monthly-0222.XML");
			Console.WriteLine("Exchange Rate XML URL");
			Console.WriteLine($"Last Modified Date: {result.LastModified}");
			Console.WriteLine($"Content Length:\n{result.Content.Length}");

			result = webClientWrapper.GetDatedContent("https://assets.publishing.service.gov.uk/government/uploads/system/uploads/attachment_data/file/931643/Maritime_ports_and_wharves_location_codes_for_Data_Element_5_23_of_the_Customs_Declaration_Service.csv");
			Console.WriteLine("Port Data CSV URL");
			Console.WriteLine($"Last Modified Date: {result.LastModified}");
			Console.WriteLine($"Content Length:\n{result.Content.Length}");
		}

		[Test]
		[Explicit("Debug/Developer Test")]
		public void GetDatedContentAsBytes()
		{
			var webClientWrapper = new Services.Common.WebClientWrapper();

			var result = webClientWrapper.GetDatedContentAsByteArray("http://www.hmrc.gov.uk/softwaredevelopers/rates/exrates-monthly-0222.XML");
			Console.WriteLine("Exchange Rate XML URL");
			Console.WriteLine($"Last Modified Date: {result.LastModified}");
			Console.WriteLine($"Content Length:\n{result.Content.Length}");

			result = webClientWrapper.GetDatedContentAsByteArray("https://assets.publishing.service.gov.uk/government/uploads/system/uploads/attachment_data/file/931643/Maritime_ports_and_wharves_location_codes_for_Data_Element_5_23_of_the_Customs_Declaration_Service.csv");
			Console.WriteLine("Port Data CSV URL");
			Console.WriteLine($"Last Modified Date: {result.LastModified}");
			Console.WriteLine($"Content Length:\n{result.Content.Length}");
		}
	}
}
