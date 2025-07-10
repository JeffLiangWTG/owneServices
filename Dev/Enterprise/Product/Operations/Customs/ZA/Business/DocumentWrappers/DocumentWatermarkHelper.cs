using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	public static class DocumentWatermarkHelper
	{
		public static class DocumentCommands
		{
			public const string SADDocument = "SAD Document Pack";
		}

		public static MultilingualString GetWatermarkText(ZString documentCommand, IBODocDataProvider docDataProvider)
		{
			switch (documentCommand)
			{
				case DocumentCommands.SADDocument:
					return GetSADDocumentWatermark(docDataProvider);

				default:
					return null;
			}
		}

		static MultilingualString GetSADDocumentWatermark(IBODocDataProvider docDataProvider)
		{
			MultilingualString result = null;

			if (docDataProvider != null && docDataProvider.BusinessObjectToLogAgainst is CusEntryHeader cusEntryHeader && !cusEntryHeader.CH_EntryStatus.IsEmpty)
			{
				var watermarks = ZACustomsRegistry.Instance.SADDocumentPackWatermarks.GetFallBackValueAtAllLevels(cusEntryHeader?.Declaration?.CompanyPK.ToGuid() ?? Guid.Empty, cusEntryHeader?.Declaration?.Branch?.PK.ToGuid() ?? Guid.Empty, Guid.Empty);

				var watermark = watermarks.OfType<SADDocumentWatermark>().FirstOrDefault(x => x.EntryStatusCode == cusEntryHeader.CH_EntryStatus);

				if (watermark != null)
				{
					result = (NoResString)(watermark.WatermarkText.IsEmpty ? watermark.EntryStatusDescription : watermark.WatermarkText);
				}
			}

			return result;
		}
	}
}
