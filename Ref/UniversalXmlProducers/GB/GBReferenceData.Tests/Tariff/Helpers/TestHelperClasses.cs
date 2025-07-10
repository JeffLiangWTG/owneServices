using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.GBReferenceData.Services.Tariff;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff.Helpers
{
	internal class TestHelperClasses
	{
		public class DownloadManagerTester : DownloadManager
		{
			public DownloadManagerTester(string workingFolder) : base(workingFolder)
			{
				ConsoleOuput = new StringWriter();
				Console.SetOut(ConsoleOuput);
			}

			public new string GetAuthorisationHeader(ITariffWebClientWrapper webClientWrapper) => DownloadManager.GetAuthorisationHeader(webClientWrapper);
			public new List<FileDetails> GetFileList(ITariffWebClientWrapper webClientWrapper) => DownloadManager.GetFileList(webClientWrapper);

			public override ITariffWebClientWrapper GetWebClientWrapper() => TestWebClientWrapper;

			public ITariffWebClientWrapper TestWebClientWrapper { get; set; }

			protected override TimeSpan RetryInterval => TimeSpan.FromMilliseconds(1);
			public StringWriter ConsoleOuput;
		}
	}
}
