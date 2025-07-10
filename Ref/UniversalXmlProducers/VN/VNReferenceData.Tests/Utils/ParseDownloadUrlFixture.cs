using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.VNReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.VNReferenceData.Tests
{
	[TestFixture]
	public class ParseDownloadUrlFixture
	{
		[TestCaseSource(nameof(TestCases))]
		public void TestParseDownloadUrl(CodeListMetadataItem metadataItem, string expectedUrl)
		{
			var actualUri = MetadataHelper.ParseDownloadUrl(metadataItem);
			var expectedUri = new Uri(expectedUrl);
			Assert.True(expectedUri.Equals(actualUri),
				$"The url {actualUri.AbsoluteUri} does not match the expected {expectedUri.AbsoluteUri}");
		}

		public static IEnumerable<TestCaseData> TestCases()
		{
			yield return new TestCaseData(
				new CodeListMetadataItem
				{
					FileDownloadUrl =
						"https://files.customs.gov.vn/CustomsCMS/TONG_CUC/2022/3/15/Nuoc.xls"
				},
				"https://files.customs.gov.vn/CustomsCMS/TONG_CUC/2022/3/15/Nuoc.xls"
			).SetName("Normal url, no replace needed");

			yield return new TestCaseData(
				new CodeListMetadataItem
				{
					FileDownloadUrl =
						"http://10.224.128.185:8080/resources/CustomsCMS/TONG_CUC/Bangma2025/5/26/Bang ma don vi HQ 2025 14052025.xlsx"
				},
				"https://files.customs.gov.vn/CustomsCMS/TONG_CUC/Bangma2025/5/26/Bang ma don vi HQ 2025 14052025.xlsx"
			).SetName("Replace prefix for url starting with ip address");
		}
	}
}
