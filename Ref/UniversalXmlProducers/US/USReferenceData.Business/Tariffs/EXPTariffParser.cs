using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.USReferenceData.Services;

namespace CargoWise.RefDbRepo.USReferenceData.Business
{
	public class EXPTariffParser : TariffParser
	{
		public EXPTariffParser(IDownLoadService serviceClient, DateTime processDate)
		: base(serviceClient, processDate, null, null)
		{
		}

		public EXPTariffParser(IDownLoadService serviceClient, DateTime processDate, IReadOnlyDictionary<string, Tariff4PGA[]> tariff4PGAMap, List<EV1> ev1List)
		: base(serviceClient, processDate, tariff4PGAMap, ev1List)
		{
		}

		protected override string OutPutFileName => "RefCusTariffZZ_US_EXP_TYPE.xml";

		protected override string TariffType => Constants.TariffTypes.EXP;

		protected override string UpdateInfoNodeKeyword => Constants.FileStructureAndUpdateInfoNodeKeywords.EXP;

		protected override string DownloadFileName => Constants.DownloadFileNames.EXP;

		protected override string XMLWriterDataSource => "AES Import Concordance (HTS)";
	}
}
