using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class CustomsReferenceCollectionDataObjectReader
	{
		public CustomsReferenceCollectionDataObjectReader(IXmlImportLogger logger, ICustomsReferenceCollectionReaderHelper helper, string dataContext = "")
		{
			this.logger = Argument.NotNull(logger, "logger");
			this.helper = Argument.NotNull(helper, "helper");
			this.dataContext = dataContext;
		}

		public IColumnIndexer[] ReadIntoDataRows(ZGuid parentPK, ZString parentTableCode, bool? isParentInDatabase, UniversalCustoms.ICustomsReferenceCollectionParent customsReferenceContainer)
		{
			IColumnIndexer[] result = null;
			var customsReferenceCollection = customsReferenceContainer.CustomsReferenceCollection;
			var distinctCustomsReferenceTypes = customsReferenceCollection == null ? System.Array.Empty<ZString>() : customsReferenceCollection.Select(x => x.Type.GetCodeAsUpperCase()).Distinct().ToArray();
			if (distinctCustomsReferenceTypes.Length > 0)
			{
				if (!parentPK.IsValid || parentTableCode.IsEmpty)
				{
					logger.Log(LogType.Error, Enterprise.Customs.DataTransfer.Res.GetString("269677C8-D6B0-4CDE-AB8C-B2800584EF34", "Cannot process Customs Reference as parent PK '{0}' or parent Table Code '{1}' is either empty or invalid.", parentPK, parentTableCode));
				}
				else
				{
					var supportedCusCodeDataTypes = helper.GetSupportedCusCodeDataCY_TypesFor(parentTableCode, dataContext);
					var supportedCusReferenceTypes = helper.GetSupportedCusReferenceCFR_TypesFor(parentTableCode, dataContext);
					if ((supportedCusCodeDataTypes == null || supportedCusCodeDataTypes.Length == 0) && (supportedCusReferenceTypes == null || supportedCusReferenceTypes.Length == 0))
					{
						logger.Log(LogType.Warning, Enterprise.Customs.DataTransfer.Res.GetString("D556E601-54B7-420E-8027-501F83115212", "Customs Reference was not processed as there is no support for Table with code '{0}'.", parentTableCode));
					}
					else
					{
						var cusCodeDataRows = ReadIntoCusCodeData(parentPK, parentTableCode, isParentInDatabase, customsReferenceCollection, distinctCustomsReferenceTypes, supportedCusCodeDataTypes);
						var cusReferenceRows = ReadIntoCusReference(parentPK, parentTableCode, isParentInDatabase, customsReferenceCollection, supportedCusReferenceTypes);
						result = cusCodeDataRows.Concat(cusReferenceRows).ToArray();
					}

					var cusAuthorizationUsageRows = ReadIntoCusAuthorizationUsage(parentPK, parentTableCode, isParentInDatabase, customsReferenceCollection).ToArray();
					result = (result?.Concat(cusAuthorizationUsageRows) ?? cusAuthorizationUsageRows).ToArray();
				}
			}
			return result;
		}

		protected virtual IEnumerable<IColumnIndexer> ReadIntoCusReference(ZGuid parentPK, ZString parentTableCode, bool? isParentInDatabase, List<UniversalCustoms.CustomsReference> customsReferenceCollection, ZString[] supportedTypes) => Enumerable.Empty<IColumnIndexer>();

		protected virtual IEnumerable<IColumnIndexer> ReadIntoCusAuthorizationUsage(ZGuid parentPK, ZString parentTableCode, bool? isParentInDatabase, List<UniversalCustoms.CustomsReference> customsReferenceCollection) => Enumerable.Empty<IColumnIndexer>();

		IEnumerable<IColumnIndexer> ReadIntoCusCodeData(ZGuid parentPK, ZString parentTableCode, bool? isParentInDatabase, List<UniversalCustoms.CustomsReference> customsReferenceCollection, ZString[] distinctCustomsReferenceTypes, ZString[] supportedTypes)
		{
			var newRows = new List<IColumnIndexer>();
			if (supportedTypes != null && supportedTypes.Length > 0)
			{
				var query = new ZQuery(CusCodeDataSchema.CY_ParentID, parentPK);
				query.AddToFilter(CusCodeDataSchema.CY_Type, supportedTypes);
				query.AddToFilter(CusCodeDataSchema.CY_Type, distinctCustomsReferenceTypes); // only delete type that is specified in XML and is supported
				query.FetchOnlyFromLocalCache = isParentInDatabase.HasValue && !isParentInDatabase.Value;
				var existingCusCodeDatas = helper.Factory.Load<CusCodeData>(query);
				existingCusCodeDatas.DeleteAll(true, getAdditionalChildren: Business.AdditionalChildrenHelper.GetAdditionalChildrenIfSupported);

				foreach (var customsReference in customsReferenceCollection.Where(x => supportedTypes.Contains(x.Type.GetCodeAsUpperCase())))
				{
					var row = new CustomsReferenceDataObjectReader(customsReference, logger, parentPK, parentTableCode, helper.Factory).ReadIntoDataRowCusCodeData();
					if (row != null)
					{
						newRows.Add(row);
					}
				}
			}
			return newRows;
		}

		protected readonly IXmlImportLogger logger;
		protected readonly ICustomsReferenceCollectionReaderHelper helper;
		protected readonly string dataContext;
	}
}
