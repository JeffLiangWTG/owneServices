using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.Customs.US.Business
{
	public class DrawbackInvoiceLineImportWizard : ImportWizard
	{
		public DrawbackInvoiceLineImportWizard(IImportCollectionInfo collectionInfo, ISettingsStorage settingsStorage, IFileMapper fileMapper)
			: base(collectionInfo, settingsStorage, fileMapper)
		{
		}

		protected override ImportWizardMapping[] GetSortedMappedRecordsIfNeeded(ImportWizardMapping[] mappedRecords)
		{
			var reversedMappingRecordsLength = mappedRecords.Length * -1;
			var result = new SortedList<int, ImportWizardMapping>();
			var i = 0;
			foreach (var mappedRecord in mappedRecords)
			{
				var key = i++;
				switch (mappedRecord.MappingName)
				{
					case JobComInvoiceLine.Schema.US_DRWIsForImportSection:
						key = reversedMappingRecordsLength + 1;
						break;
					case JobComInvoiceLine.Schema.US_DRWIsForExportSection:
						key = reversedMappingRecordsLength + 2;
						break;
					case JobComInvoiceLine.Schema.US_ImportEntryNo:
						key = reversedMappingRecordsLength + 3;
						break;
					case JobComInvoiceLine.Schema.US_DRWImportEntryLine:
						key = reversedMappingRecordsLength + 4;
						break;
					case JobComInvoiceLine.Schema.JI_Tariff:
						key = reversedMappingRecordsLength + 5;
						break;
					case JobComInvoiceLine.Schema.US_DRWClaimAmountOverriden_New:
						key = reversedMappingRecordsLength + 6;
						break;
				}
				result.Add(key, mappedRecord);
			}

			return result.Values.ToArray();
		}
	}
}
