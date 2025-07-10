using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class AddInfoGroupCollectionDataObjectReader
	{
		public AddInfoGroupCollectionDataObjectReader(IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, ZString[] addInfoTypesThatShouldNotBeImported = null, string dataContext = "")
		{
			this.logger = Argument.NotNull(logger, "logger");
			this.helper = helper;
			this.addInfoTypesThatShouldNotBeImported = addInfoTypesThatShouldNotBeImported;
			this.dataContext = dataContext;
		}

		ZString[] addInfoTypesThatShouldNotBeImported;

		public virtual IColumnIndexer[] ReadIntoDataRows(ZGuid parentPK, ZString parentTableCode, bool? isParentInDatabase, UniversalCustoms.IAddInfoGroupCollectionParent addInfoGroupContainer)
		{
			IColumnIndexer[] result = null;
			var addInfoGroupCollection = addInfoGroupContainer.AddInfoGroupCollection;
			var differentTypes = GetDistinctTypes(addInfoGroupCollection);
			if (differentTypes.Length > 0)
			{
				if (!parentPK.IsValid || parentTableCode.IsEmpty)
				{
					logger.Log(LogType.Error, Enterprise.Customs.DataTransfer.Res.GetString("C7EC14CF-87AD-4654-BB23-55E395A8BA36", "Cannot process Add Info Group as parent PK '{0}' or parent Table Code '{1}' is either empty or invalid.", parentPK, parentTableCode));
				}
				else
				{
					var newRows = new List<IColumnIndexer>();
					addInfoTypesThatShouldNotBeImported = addInfoTypesThatShouldNotBeImported ?? System.Array.Empty<ZString>();

					var supportedTypes = helper.GetSupportedCusAddInfoB7_TypesFor(parentTableCode, dataContext);
					var supportedTypesForOtherTable = helper.GetAddInfoGroupTypesNeedInsertedToOtherTableFor(parentTableCode, dataContext);

					if (addInfoTypesThatShouldNotBeImported != null && addInfoTypesThatShouldNotBeImported.Length > 0)
					{
						supportedTypes = supportedTypes.Except(addInfoTypesThatShouldNotBeImported).ToArray();
						supportedTypesForOtherTable = supportedTypesForOtherTable.Except(addInfoTypesThatShouldNotBeImported).ToArray();
					}

					var supportedTypesHaveValue = supportedTypes != null && supportedTypes.Length > 0;
					var supportedTypesForOtherTableHaveValue = supportedTypesForOtherTable != null && supportedTypesForOtherTable.Length > 0;
					if (supportedTypesHaveValue || supportedTypesForOtherTableHaveValue)
					{
						if (supportedTypesHaveValue)
						{
							var query = new ZQuery(CusAddInfoSchema.B7_ParentID, parentPK);
							query.AddToFilter(CusAddInfoSchema.B7_Type, supportedTypes);
							query.AddToFilter(CusAddInfoSchema.B7_Type, differentTypes); // only delete type that is specified in XML and is supported
							query.FetchOnlyFromLocalCache = isParentInDatabase.HasValue && !isParentInDatabase.Value;
							var existingAddInfos = helper.Factory.Load<CusAddInfo>(query);
							existingAddInfos.DeleteAll(true, getAdditionalChildren: Business.AdditionalChildrenHelper.GetAdditionalChildrenIfSupported);
						}

						if (supportedTypesForOtherTableHaveValue)
						{
							foreach (var code in supportedTypesForOtherTable)
							{
								helper.DeleteOtherTableDataViaAddInfoGroupType(code, parentPK, parentTableCode, isParentInDatabase);
							}
						}

						foreach (var addInfoGroup in addInfoGroupCollection)
						{
							var type = addInfoGroup.Type.GetCodeAsUpperCase();

							if (supportedTypesForOtherTableHaveValue && supportedTypesForOtherTable.Contains(type))
							{
								var reader = helper.GetAddInfoGroupToOtherTableDataObjectReader(parentTableCode, type, addInfoGroup, logger);
								var row = new AddInfoGroupDataObjectReader(addInfoGroup, logger, helper, parentPK, parentTableCode, isParentInDatabase, reader).ReadIntoDataRow();
								if (row != null)
								{
									newRows.Add(row);
								}
							}
							else if (supportedTypesHaveValue && supportedTypes.Contains(type))
							{
								var row = new AddInfoGroupDataObjectReader(addInfoGroup, logger, helper, parentPK, parentTableCode, isParentInDatabase).ReadIntoDataRow();
								if (row != null)
								{
									newRows.Add(row);
								}
							}
						}
					}

					result = newRows.ToArray();
				}
			}
			return result;
		}

		ZString[] GetDistinctTypes(List<UniversalCustoms.AddInfoGroup> addInfoGroupCollection)
		{
			return addInfoGroupCollection == null ? System.Array.Empty<ZString>() : addInfoGroupCollection.Select(x => x.Type.GetCodeAsUpperCase()).Distinct().ToArray();
		}

		protected readonly IXmlImportLogger logger;
		protected readonly UniversalDataObjectReaderHelper helper;
		protected readonly string dataContext;
	}
}
