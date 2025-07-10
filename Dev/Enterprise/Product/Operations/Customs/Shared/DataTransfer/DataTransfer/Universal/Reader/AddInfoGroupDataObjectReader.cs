using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class AddInfoGroupDataObjectReader : DataObjectReader<UniversalCustoms.AddInfoGroup>
	{
		internal AddInfoGroupDataObjectReader(UniversalCustoms.AddInfoGroup addInfoGroupDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, ZGuid parentPK, ZString parentTableCode, bool? isParentInDatabase, IAddInfoGroupToOtherTableDataObjectReader insertIntoOhterTableReader = null)
			: base(addInfoGroupDataObject, logger, helper.Factory)
		{
			this.parentPK = parentPK;
			this.parentTableCode = parentTableCode;
			this.helper = helper;
			this.isParentInDatabase = isParentInDatabase;
			this.insertIntoOhterTableReader = insertIntoOhterTableReader;
		}

		public IColumnIndexer ReadIntoDataRow()
		{
			IColumnIndexer row = null;
			if (dataObject.Type != null && (dataObject.AddInfoCollection != null || dataObject.AddInfoGroupCollection != null || dataObject.CustomsReferenceCollection != null || dataObject.OrganizationAddressCollection != null))
			{
				if (insertIntoOhterTableReader != null)
				{
					row = insertIntoOhterTableReader.ReadIntoRow(parentPK, parentTableCode);
					if (row != null)
					{
						ReadDataFromCustomsReferenceCollectionAndAddInfoGroupCollection(dataObject.Type.ToString(), parentPK, parentTableCode);
					}
				}
				else
				{
					Type type = null;
					try
					{
						type = new CusAddInfoTypeDecider().GetTypeForLoad(dataObject.Type.GetCodeAsUpperCase(), parentTableCode, parentPK, factory.BOFactory);
					}
					catch(InvalidOperationException e)
					{
						logger.Log(Integration.LogType.Error, e.Message);
					}

					if (type != null)
					{
						row = CreateNewColumnIndexer(CusAddInfoSchema.PK, type);
						if (row != null)
						{
							SetValue(row, CusAddInfoSchema.B7_ParentID, parentPK);
							SetValue(row, CusAddInfoSchema.B7_ParentTableCode, parentTableCode);
							SetValue(row, CusAddInfoSchema.B7_Type, dataObject.Type);
							IAddInfoManager addInfoManager = null;
							AddInfoDataObjectReader addInfoReader = null;
							var isAddInfoSerialisationEnabled = false;
							try
							{
								addInfoReader = GetNewAddInfoDataObjectReader(row, type, out addInfoManager);
								if (addInfoReader != null)
								{
									var delaySetters = IsDefaultingEnabled ? new Dictionary<string, ValueSetter>() : null;
									isAddInfoSerialisationEnabled = addInfoManager != null && !IsDefaultingEnabled;
									if (isAddInfoSerialisationEnabled)
									{
										addInfoManager.SetUpdateFromAddInfoSerialisationFlag(false);
									}
									addInfoReader.ReadIntoRow(addInfoManager, row, dataObject, delaySetters, dataObject);
									delaySetters.SetValueInSpecificOrder(helper.GetMatchingKeysInSettingOrder(row));
									if (IsDefaultingEnabled)
									{
										addInfoManager.UpdateRelatedPropertyInfo();
									}
								}
								var addInfoPK = row.GetValue(CusAddInfoSchema.PK);
								ReadDataFromCustomsReferenceCollectionAndAddInfoGroupCollection(row.GetValue(CusAddInfoSchema.B7_Type), addInfoPK, CusAddInfoSchema.Constants.Prefix);
							}
							finally
							{
								if (isAddInfoSerialisationEnabled)
								{
									addInfoManager.UpdateAddInfoFromString(row.GetValue(CusAddInfoSchema.B7_AddInfoData));
									addInfoManager.SetUpdateFromAddInfoSerialisationFlag(true);
								}
								if (addInfoReader != null)
								{
									addInfoReader.AfterUpdateRelatedPropertyCompleted(addInfoManager);
								}
							}
						}
					}
				}
			}
			return row;
		}

		void ReadDataFromCustomsReferenceCollectionAndAddInfoGroupCollection(ZString addInfoGroupType, ZGuid addInfoGroupRelatedBOPK, ZString addInfoGroupRelatedBOPrefix)
		{
			new CustomsReferenceCollectionDataObjectReader(logger, helper).ReadIntoDataRows(addInfoGroupRelatedBOPK, addInfoGroupRelatedBOPrefix, isParentInDatabase, dataObject);
			if (dataObject.AddInfoGroupCollection != null)
			{
				new AddInfoGroupCollectionDataObjectReader(logger, helper).ReadIntoDataRows(addInfoGroupRelatedBOPK, addInfoGroupRelatedBOPrefix, isParentInDatabase, dataObject);
				var additionalAddInfoGroupCollectionSupport = helper.GetAdditionalAddInfoGroupCollectionSupportFor(addInfoGroupRelatedBOPrefix, addInfoGroupType, logger);
				if (additionalAddInfoGroupCollectionSupport != null)
				{
					foreach (var additionalAddInfoGroupSupport in additionalAddInfoGroupCollectionSupport)
					{
						additionalAddInfoGroupSupport.Value.Process(dataObject.AddInfoGroupCollection.Where(x => additionalAddInfoGroupSupport.Key == x.Type.GetCodeAsUpperCase()), addInfoGroupRelatedBOPK, addInfoGroupRelatedBOPrefix, isParentInDatabase);
					}
				}
			}
		}

		AddInfoDataObjectReader GetNewAddInfoDataObjectReader(IColumnIndexer row, Type type, out IAddInfoManager addInfoManager)
		{
			AddInfoDataObjectReader result = null;
			addInfoManager = row as IAddInfoManager ?? GetAddInfoManager(row, type);
			if (addInfoManager != null)
			{
				var addInfo = addInfoManager.AddInfo;
				if (addInfo != null)
				{
					result = helper.GetNewAddInfoDataObjectReader(addInfoManager, logger, helper, CusAddInfoSchema.B7_AddInfoData);
				}
			}
			return result;
		}

		IAddInfoManager GetAddInfoManager(IColumnIndexer row, Type type)
		{
			IAddInfoManager addInfoManager = null;
			try
			{
				var addInfo = (CusAddInfo)factory.Load(type, row.GetValue(CusAddInfoSchema.PK));
				addInfoManager = addInfo as IAddInfoManager;
			}
			catch (NoConcreteTypeException)
			{
				// Use default
			}
			return addInfoManager;
		}

		readonly UniversalDataObjectReaderHelper helper;
		readonly ZGuid parentPK;
		readonly bool? isParentInDatabase;
		readonly ZString parentTableCode;
		readonly IAddInfoGroupToOtherTableDataObjectReader insertIntoOhterTableReader;
	}
}
