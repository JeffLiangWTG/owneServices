using System;
using System.Net.Http.Headers;
using CargoWise.RefDbRepo.Staging.Common;
using Moq;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Helpers
{
	internal static class MockFileDownloader
	{
		public static IFileDownloader Create(string resourceName, DateTime fileDate)
		{
			var mockResponseStream = new Mock<IResponseStream>();
			mockResponseStream.Setup(x => x.GetResponseStream()).Returns(TestHelper.GetResourceStream(resourceName));

			var mockDownloader = new Mock<IFileDownloader>();
			mockDownloader.Setup(x => x.GetCreationTime(It.IsAny<AuthenticationHeaderValue>())).Returns(fileDate);
			mockDownloader.Setup(x => x.GetFileStream(It.IsAny<AuthenticationHeaderValue>())).Returns(mockResponseStream.Object);

			return mockDownloader.Object;
		}
	}
}
