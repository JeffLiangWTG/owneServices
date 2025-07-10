using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class CustomsSupportingInformationCollectionDataObjectReader : DataObjectReader
	{
		public CustomsSupportingInformationCollectionDataObjectReader(IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, string dataContext = "")
			: base(logger)
		{
			this.helper = helper;
			this.dataContext = dataContext;
		}

		public IColumnIndexer[] ReadIntoDataRows(ZGuid parentPK, ZString parentTableCode, bool? isParentInDatabase, ICustomsSupportingInformationCollectionParent customsSupportingInformationContainer, ZString[] supportedTypes = null, GetMatchingDataPredicate getMatchingData = null)
		{
			IColumnIndexer[] result = null;
			var customsSupportingInformationCollection = customsSupportingInformationContainer.CustomsSupportingInformationCollection;
			IDictionary<ZString, List<CustomsSupportingInformation>> customsSupportingInformationGroupByCategory = null;
			customsSupportingInformationCollection.GetOrCreateCodeDictionary(ref customsSupportingInformationGroupByCategory, (CustomsSupportingInformation x) => x.Category);
			if (customsSupportingInformationGroupByCategory != null && customsSupportingInformationGroupByCategory.Count > 0)
			{
				if (!parentPK.IsValid || parentTableCode.IsEmpty)
				{
					logger.Log(LogType.Error, Enterprise.Customs.DataTransfer.Res.GetString("{4B171C49-3BCC-445E-A20C-97A5F192761A}", "Cannot process Customs Supporting Information as parent PK '{0}' or parent Table Code '{1}' is either empty or invalid.", parentPK, parentTableCode));
				}
				else
				{
					var newRows = new List<IColumnIndexer>();

					supportedTypes = supportedTypes ?? helper.GetSupportedCusSupportingInfoCSI_TypesFor(parentTableCode, dataContext);
					if (supportedTypes != null && supportedTypes.Length > 0)
					{
						var matchedTypes = customsSupportingInformationGroupByCategory.Keys.Intersect(supportedTypes).ToHashSet();
						if (matchedTypes.Count > 0)
						{
							var query = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, parentPK);
							query.AddToFilter(CusSupportingInfoSchema.CSI_Type, matchedTypes); // only delete type that is specified in XML and is supported
							query.FetchOnlyFromLocalCache = isParentInDatabase.HasValue && !isParentInDatabase.Value;
							var existingSupportingInfos = helper.Factory.Load<CusSupportingInfo>(query);
							CustomsSupportingInformationDataObjectReader.GetMatchingDataPredicate matchExisting = null;
							Dictionary<ZString, List<CusSupportingInfo>> existingDataDictionary = null;
							if (getMatchingData == null)
							{
								existingSupportingInfos.DeleteAll(true);
							}
							else
							{
								existingDataDictionary = existingSupportingInfos.GroupBy(x => x.CSI_Type).ToDictionary(x => x.Key, y => y.OrderBy(o => o.CSI_SystemCreateTimeUtc).ToList());
								matchExisting = new CustomsSupportingInformationDataObjectReader.GetMatchingDataPredicate(customsSupportingInformation =>
								{
									CusSupportingInfo matched = null;
									var type = customsSupportingInformation.Category.GetCodeAsUpperCase();
									if (existingDataDictionary.TryGetValue(type, out var dataDictionary))
									{
										matched = getMatchingData(dataDictionary, type, customsSupportingInformation);
										if (matched != null && dataDictionary.Remove(matched) && dataDictionary.Count == 0)
										{
											existingDataDictionary.Remove(type);
										}
									}
									return GetColumnIndexer(matched);
								});
							}

							foreach (var matchedType in matchedTypes)
							{
								foreach (var customsSupportingInformation in customsSupportingInformationGroupByCategory[matchedType])
								{
									var row = CreateNewCustomsSupportingInformationDataObjectReader(customsSupportingInformation, parentPK, parentTableCode, matchExisting).ReadIntoDataRow();
									if (row != null)
									{
										newRows.Add(row);
									}
								}
							}

							if (existingDataDictionary != null)
							{
								existingDataDictionary.Values.SelectMany(x => x).DeleteAll(true);
							}
						}
					}
					result = newRows.ToArray();
				}
			}
			return result;
		}

		public delegate CusSupportingInfo GetMatchingDataPredicate(IEnumerable<CusSupportingInfo> existingSupportingInfos, ZString type, CustomsSupportingInformation customsSupportingInformation);

		protected virtual CustomsSupportingInformationDataObjectReader CreateNewCustomsSupportingInformationDataObjectReader(CustomsSupportingInformation customsSupportingInformation, ZGuid parentPK, ZString parentTableCode, CustomsSupportingInformationDataObjectReader.GetMatchingDataPredicate matchExisting) => new CustomsSupportingInformationDataObjectReader(customsSupportingInformation, logger, helper.Factory, parentPK, parentTableCode, matchExisting);

		protected readonly UniversalDataObjectReaderHelper helper;
		readonly string dataContext;
	}
}
