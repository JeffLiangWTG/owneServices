using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class TaxOrFeeCollectionDataObjectReader  // Thanks to Dong "Universal King" Nguyen for this :)
	{
		public TaxOrFeeCollectionDataObjectReader(IXmlImportLogger logger, UniversalDataObjectReaderHelper helper)
		{
			this.logger = Argument.NotNull(logger, "logger");
			this.helper = helper;
		}

		public IColumnIndexer[] ReadIntoDataRows(ZGuid parentPK, ZString parentTableCode, bool? isParentInDatabase, UniversalCustoms.ITaxOrFeeCollectionParent iTaxOrFeeCollectionParent)
		{
			IColumnIndexer[] result = null;
			var taxOrFeeCollection = iTaxOrFeeCollectionParent.TaxOrFeeCollection;
			if (!parentPK.IsValid || parentTableCode.IsEmpty)
			{
				logger.Log(LogType.Error, Enterprise.Customs.DataTransfer.Res.GetString("{12345678-3BCC-445E-A20C-97A5F192761A}", "Cannot process Tax Or Fee as parent PK '{0}' or parent Table Code '{1}' is either empty or invalid.", parentPK, parentTableCode));
			}
			else
			{
				if (taxOrFeeCollection != null)
				{
					var newRows = new List<IColumnIndexer>();
					var query = new ZQuery(JobComInvoiceLineTaxSchema.JLT_JI, parentPK);
					query.FetchOnlyFromLocalCache = isParentInDatabase.HasValue && !isParentInDatabase.Value;
					var existingTaxes = helper.Factory.Load<JobComInvoiceLineTax>(query);
					existingTaxes.DeleteAll(true);

					foreach (var taxOrFee in taxOrFeeCollection)
					{
						var row = new TaxOrFeeDataObjectReader(taxOrFee, logger, helper.Factory, parentPK).ReadIntoDataRow();
						if (row != null)
						{
							newRows.Add(row);
						}
					}

					result = newRows.ToArray();
				}
			}

			return result;
		}

		readonly IXmlImportLogger logger;
		readonly UniversalDataObjectReaderHelper helper;
	}
}
