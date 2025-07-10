using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.USReferenceData.Services;

namespace CargoWise.RefDbRepo.USReferenceData.Business
{
	public class SHBTariffParser : TariffParser
	{
		public SHBTariffParser(IDownLoadService serviceClient, DateTime processDate)
		: base(serviceClient, processDate, null, null)
		{
		}

		public SHBTariffParser(IDownLoadService serviceClient, DateTime processDate, IReadOnlyDictionary<string, Tariff4PGA[]> tariff4PGAMap, List<EV1> ev1List)
		: base(serviceClient, processDate, tariff4PGAMap, ev1List)
		{
		}

		protected override string OutPutFileName => "RefCusTariffZZ_US_SHB_TYPE.xml";

		protected override string TariffType => Constants.TariffTypes.SHB;

		protected override string UpdateInfoNodeKeyword => Constants.FileStructureAndUpdateInfoNodeKeywords.SHB;

		protected override string DownloadFileName => Constants.DownloadFileNames.SHB;

		protected override string XMLWriterDataSource => "AES Export Concordance (SHB)";
	}
}
