using System;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.Common
{
	internal class CommonHelpers
	{
		public class ManifestClientWrapper : IWebClientWrapper
		{
			public DateTime TestDate { get; set; } = DateTime.Now;

			public string GetContent(string url) => GetDatedContent(url).Content;

			public byte[] GetContentAsByteArray(string url) => GetDatedContentAsByteArray(url).Content;

			public (DateTime LastModified, string Content) GetDatedContent(string url) => (TestDate, TestHelper.ReadManifestResourceContent(url));

			public (DateTime LastModified, byte[] Content) GetDatedContentAsByteArray(string url) => (TestDate, TestHelper.ReadManifestResourceContentBytes(url));
		}
	}
}
