using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.VNReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.VNReferenceData.Tests
{
	[TestFixture]
	public class ParsePublicationTimeFixture
	{
		[TestCaseSource(nameof(ParsePublicationTimeData))]
		public void TestParsePublicationTime(
			CodeListMetadataItem metadataItem,
			DateTime defaultPublicationTime,
			DateTime expectedValue)
		{
			Assert.That(
				MetadataHelper.ParsePublicationTime(metadataItem, defaultPublicationTime),
				Is.EqualTo(expectedValue));
		}

		public static IEnumerable<TestCaseData> ParsePublicationTimeData()
		{
			yield return new TestCaseData(
				new CodeListMetadataItem { UpdatedDate = "May 26, 2025" },
				DateTime.MinValue,
				new DateTime(2025, 05, 26)
			).SetName("Metadata with valid UpdatedDate");

			yield return new TestCaseData(
				new CodeListMetadataItem
				{
					UpdatedDate = "",
					// https://files.customs.gov.vn/CustomsCMS/TONG_CUC/Bangma2025/5/26/Bang ma don vi HQ 2025 14052025.xlsx
					FileDownloadUrl = "https://files.customs.gov.vn/CustomsCMS/TONG_CUC/Bangma2025/5/26/Bang%20ma%20don%20vi%20HQ%202025%2014052025.xlsx"
				},
				DateTime.MinValue,
				new DateTime(2025, 5, 14, 0, 0, 0)
			).SetName("Empty UpdatedDate in metadata, fallback to time in file name");

			yield return new TestCaseData(
				new CodeListMetadataItem
				{
					UpdatedDate = "haha hehe",
					// https://files.customs.gov.vn/CustomsCMS/TONG_CUC/Bangma2025/5/26/Bang ma don vi HQ 2025 14052025.xlsx
					FileDownloadUrl = "https://files.customs.gov.vn/CustomsCMS/TONG_CUC/Bangma2025/5/26/Bang%20ma%20don%20vi%20HQ%202025%2014052025.xlsx"
				},
				DateTime.MinValue,
				new DateTime(2025, 5, 14, 0, 0, 0)
			).SetName("Invalid UpdatedDate format in metadata, fallback to time in file name");

			yield return new TestCaseData(
				new CodeListMetadataItem
				{
					UpdatedDate = "",
					// https://files.customs.gov.vn/CustomsCMS/TONG_CUC/Bangma2025/5/26/Bang ma don vi HQ 2025 33052025.xlsx
					FileDownloadUrl = "https://files.customs.gov.vn/CustomsCMS/TONG_CUC/Bangma2025/5/26/Bang%20ma%20don%20vi%20HQ%202025%2033052025.xlsx"
				},
				DateTime.MinValue,
				new DateTime(2025, 5, 26, 0, 0, 0)
			).SetName("Invalid UpdatedDate + invalid date in file name, fallback to time in url");

			yield return new TestCaseData(
				new CodeListMetadataItem
				{
					UpdatedDate = "",
					// https://files.customs.gov.vn/CustomsCMS/TONG_CUC/Bangma2025/5/26/Bang ma don vi HQ.xlsx
					FileDownloadUrl = "https://files.customs.gov.vn/CustomsCMS/TONG_CUC/Bangma2025/7/26/Bang%20ma%20don%20vi.xlsx",
				},
				DateTime.MinValue,
				new DateTime(2025, 7, 26, 0, 0, 0)
			).SetName("Parse publicationTime from url with format 2025/7/26");

			yield return new TestCaseData(
				new CodeListMetadataItem
				{
					UpdatedDate = "",
					// https://files.customs.gov.vn/CustomsCMS/TONG_CUC/Bangma2025/03/26/Bang ma don vi HQ.xlsx
					FileDownloadUrl = "https://files.customs.gov.vn/CustomsCMS/TONG_CUC/Bangma2025/03/26/Bang%20ma%20don%20vi.xlsx",
				},
				DateTime.MinValue,
				new DateTime(2025, 3, 26, 0, 0, 0)
			).SetName("Parse publicationTime from url with format 2025/03/26");

			yield return new TestCaseData(
				new CodeListMetadataItem
				{
					UpdatedDate = "",
					// https://files.customs.gov.vn/CustomsCMS/TONG_CUC/Bangma2025/03/26/Bang ma don vi HQ.xlsx
					FileDownloadUrl = "https://files.customs.gov.vn/CustomsCMS/TONG_CUC/Bangma2025/02/04/Bang%20ma%20don%20vi.xlsx",
				},
				DateTime.MinValue,
				new DateTime(2025, 2, 4, 0, 0, 0)
			).SetName("Parse publicationTime from url with format 2025/02/04");

			yield return new TestCaseData(
				new CodeListMetadataItem
				{
					UpdatedDate = "",
					// https://files.customs.gov.vn/CustomsCMS/TONG_CUC/Bangma2025/03/26/Bang ma don vi HQ.xlsx
					FileDownloadUrl = "https://files.customs.gov.vn/CustomsCMS/TONG_CUC/Bangma2025/6/8/Bang%20ma%20don%20vi.xlsx",
				},
				DateTime.MinValue,
				new DateTime(2025, 6, 8, 0, 0, 0)
			).SetName("Parse publicationTime from url with format 2025/6/8");

			yield return new TestCaseData(
				new CodeListMetadataItem
				{
					UpdatedDate = "",
					// https://files.customs.gov.vn/CustomsCMS/TONG_CUC/Bang ma don vi HQ.xlsx
					FileDownloadUrl = "https://files.customs.gov.vn/CustomsCMS/TONG_CUC/Bang%20ma%20don%20vi.xlsx",
				},
				DateTime.MinValue,
				DateTime.MinValue
			).SetName("Fallback to defaultPublicationTime");
		}
	}
}
