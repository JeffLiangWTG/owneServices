using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using BusinessOrder = Enterprise.Freight.Forwarding.Orders.Business.Order;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public abstract class UniversalCommonReaderHelper : UniversalCommonHelper, ICustomsReferenceCollectionReaderHelper
	{
		protected UniversalCommonReaderHelper(UniversalObjectFactory factory, ZString targetCountryCode, string dataProviderForCodeMapping = null)
			: base(factory.BOFactory)
		{
			this.TargetCountryCode = Argument.NotNullOrEmpty(targetCountryCode, "targetCountryCode");
			this.DataProvider = dataProviderForCodeMapping;
			this.factory = Argument.NotNull(factory, "factory");
		}

		public void DeleteAll(ITableSchema tableSchema, ZQuery query)
		{
			var rows = Load(tableSchema, query);
			if (rows != null)
			{
				foreach (var row in rows)
				{
					if (row != null && row.RowState != DataRowState.Deleted)
					{
						row.Delete();
					}
				}
			}
		}

		public DataRow[] Load(ITableSchema tableSchema, ZQuery query)
		{
			return Factory.RowFactory.Load(tableSchema.TableName, query);
		}

		public ZString[] GetSupportedCusSupportingInfoCSI_TypesFor(ZString parentTableCode, string dataContext)
		{
			return GetSupportedCusSupportingInfoCSI_TypesFor(TargetCountryCode, parentTableCode, dataContext);
		}

		public ZString[] GetSupportedCusAddInfoB7_TypesFor(ZString parentTableCode, string dataContext)
		{
			return GetSupportedCusAddInfoB7_TypesFor(TargetCountryCode, parentTableCode, dataContext);
		}

		public ZString[] GetAddInfoGroupTypesNeedInsertedToOtherTableFor(ZString parentTableCode, string dataContext)
		{
			return GetAddInfoGroupTypesNeedInsertedToOtherTableFor(TargetCountryCode, parentTableCode, dataContext);
		}

		public ZString[] GetSupportedCusCodeDataCY_TypesFor(ZString parentTableCode, string dataContext)
		{
			return GetSupportedCusCodeDataCY_TypesFor(TargetCountryCode, parentTableCode, dataContext);
		}

		public ZString[] GetSupportedCusReferenceCFR_TypesFor(ZString parentTableCode, string dataContext)
		{
			return GetSupportedCusReferenceCFR_TypesFor(TargetCountryCode, parentTableCode, dataContext);
		}

		public UniversalObjectFactory Factory
		{
			get { return factory; }
		}

		public IUniversalCodeMapper GetUniversalCodeMapper(IXmlImportLogger logger)
		{
			return Factory.GetCachedValue(string.Format("UniversalDataObjectReaderHelper.{0}UniversalCodeMapper{1}", DataProvider, logger.GetHashCode()), () => ObjectFactory.New<IUniversalCodeMapper>(DataProvider, logger, Factory.BOFactory));
		}

		public IDictionary<ZString, AddInfoPropertyNameAndValueParser> GetSupportedAddInfoList<T>(ITableSchema tableSchema)
			where T : BusinessObject
		{
			return GetSupportedAddInfoList(typeof(T), tableSchema);
		}

		public IDictionary<ZString, AddInfoPropertyNameAndValueParser> GetSupportedAddInfoList(Type businessObjectType, ITableSchema tableSchema)
		{
			return Factory.GetCachedValue(businessObjectType.FullName + "GetSupportedAddInfoList", () =>
			{
				IDictionary<ZString, AddInfoPropertyNameAndValueParser> result;
				var listKey = businessObjectType.FullName + tableSchema.GetType().Name;
				if (!SupportedAddInfoDictionary.TryGetValue(listKey, out result))
				{
					result = new Dictionary<ZString, AddInfoPropertyNameAndValueParser>();
					foreach (var pair in Factory.BOFactory.GetAddInfoSchemaWithMaxLengthDictionary(businessObjectType, tableSchema))
					{
						result.Add(pair.Key, GetAddInfoValueFunction(pair));
					}
					SupportedAddInfoDictionary.Add(listKey, result);
				}
				return result;
			});
		}

		Dictionary<string, IDictionary<ZString, AddInfoPropertyNameAndValueParser>> SupportedAddInfoDictionary
		{
			get { return supportedAddInfoDictionary ?? (supportedAddInfoDictionary = new Dictionary<string, IDictionary<ZString, AddInfoPropertyNameAndValueParser>>()); }
		}
		Dictionary<string, IDictionary<ZString, AddInfoPropertyNameAndValueParser>> supportedAddInfoDictionary;

		AddInfoPropertyNameAndValueParser GetAddInfoValueFunction(KeyValuePair<string, SchemaColumnAndMaxLength> pair)
		{
			var schemaColumn = pair.Value.Column;
			var type = schemaColumn.GetEquivalentZType();
			AddInfoValueParser function = null;
			if (typeof(ZString).IsAssignableFrom(type))
			{
				function = GetAddInfoValueParserWithCodeMapping(pair.Key, pair.Value.MaxLength);
			}

			return new AddInfoPropertyNameAndValueParser() { PropertyName = schemaColumn.Name, Parser = function };
		}

		AddInfoValueParser GetAddInfoValueParserWithCodeMapping(string key, int maxLength)
		{
			return new AddInfoValueParser((logger, stringValue) =>
			{
				var valueResult = stringValue;
				string codeMappingRelationshipCode;
				if (SupportedCodeMappingRelationshipCodeDictionary.TryGetValue(key, out codeMappingRelationshipCode))
				{
					var mapper = GetUniversalCodeMapper(logger);
					if (mapper != null)
					{
						valueResult = mapper.GetMappedOrInput(valueResult, codeMappingRelationshipCode);
					}
				}
				valueResult = valueResult.TrimEnd(' ');
				if (maxLength > 0 && valueResult.Length > maxLength)
				{
					logger.Log(LogType.Warning, BaseAddInfo.GetMaximumLengthTruncateMessage(key, maxLength, valueResult));
					valueResult = valueResult.Left(maxLength);
				}
				return valueResult;
			});
		}

		IDictionary<string, string> SupportedCodeMappingRelationshipCodeDictionary
		{
			get
			{
				if (supportedCodeMappingRelationshipCodeDictionary == null)
				{
					supportedCodeMappingRelationshipCodeDictionary = CreateSupportedCodeMappingRelationshipCodeDictionary();
				}
				return supportedCodeMappingRelationshipCodeDictionary;
			}
		}
		IDictionary<string, string> supportedCodeMappingRelationshipCodeDictionary;

		protected virtual IDictionary<string, string> CreateSupportedCodeMappingRelationshipCodeDictionary()
		{
			return new Dictionary<string, string>();
		}

		public bool IsInterfaceEnabledCompany
		{
			get { return IntegratedCountryHelper.IsInterfaceEnabledCompany(GlbCompany.CurrentCompany.PK, TargetCountryCode); }
		}

		public readonly ZString TargetCountryCode;
		public readonly string DataProvider;
		new readonly UniversalObjectFactory factory;
	}

	public class UniversalDataObjectReaderHelper : UniversalCommonReaderHelper, IUniversalDataObjectReaderHelper
	{
		public UniversalDataObjectReaderHelper(UniversalObjectFactory factory, ZString targetCountryCode, ZString sourceCountryCode, string dataProviderForCodeMapping = null)
			: base(factory, targetCountryCode, dataProviderForCodeMapping)
		{
			this.SourceCountryCode = Argument.NotNullOrEmpty(sourceCountryCode, "sourceCountryCode");
			this.SupportAdditionalInvoiceLineEntryLineLink = AdditionalInvoiceLineEntryLineLinkSupporter.DoesSupport(TargetCountryCode);
		}

		public readonly bool SupportAdditionalInvoiceLineEntryLineLink;

		public bool IsSourceAndTargetCountrySame
		{
			get { return SourceCountryCode == TargetCountryCode; }
		}

		public ICustomLabelsProvider GetJobComInvoiceLineCustomLabelsProvider(IColumnIndexer invoiceLineRow)
		{
			var invoice = Load<BaseJobComInvoiceHeader>(invoiceLineRow, JobComInvoiceLineSchema.JI_JZ);
			return GetJobComInvoiceLineCustomLabelsProviderFromProvider(invoice.JobDeclaration);
		}

		public ICustomLabelsProvider GetCusContainerCustomLabelsProvider(IColumnIndexer containerRow)
		{
			return GetCusContainerCustomLabelsProviderFromDeclaration(Load<BaseJobDeclaration>(containerRow, CusContainerSchema.CO_JE));
		}

		public readonly ZString SourceCountryCode;

		public IEnumerable<ZString> GetMatchingKeysInSettingOrder(IColumnIndexer row)
		{
			return GetMatchingKeysInSettingOrderCore(row);
		}

		protected virtual IEnumerable<ZString> GetMatchingKeysInSettingOrderCore(IColumnIndexer row)
		{
			return Enumerable.Empty<ZString>();
		}

		public void DeleteOtherTableDataViaAddInfoGroupType(ZString addInfoGroupType, ZGuid parentPK, ZString parentTableCode, bool? isParentInDatabase)
		{
			DeleteOtherTableDataViaAddInfoGroupTypeCore(addInfoGroupType, parentPK, parentTableCode, isParentInDatabase);
		}

		protected virtual void DeleteOtherTableDataViaAddInfoGroupTypeCore(ZString addInfoGroupType, ZGuid parentPK, ZString parentTableCode, bool? isParentInDatabase)
		{
		}

		public IAddInfoGroupToOtherTableDataObjectReader GetAddInfoGroupToOtherTableDataObjectReader(ZString parentTableCode, ZString addInfoGroupType, AddInfoGroup addInfoGroup, IXmlImportLogger logger)
		{
			return GetAddInfoGroupToOtherTableDataObjectReaderCore(parentTableCode, addInfoGroupType, addInfoGroup, logger);
		}

		protected virtual IAddInfoGroupToOtherTableDataObjectReader GetAddInfoGroupToOtherTableDataObjectReaderCore(ZString parentTableCode, ZString addInfoGroupType, AddInfoGroup addInfoGroup, IXmlImportLogger logger)
		{
			return null;
		}

		public AddInfoDataObjectReader GetNewAddInfoDataObjectReader(IAddInfoManager addInfoManager, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, SchemaStringColumn column)
		{
			return GetNewAddInfoDataObjectReaderCore(addInfoManager, logger, helper, column);
		}

		protected virtual AddInfoDataObjectReader GetNewAddInfoDataObjectReaderCore(IAddInfoManager addInfoManager, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, SchemaStringColumn column)
		{
			var addInfoSchema = (addInfoManager as IAddInfoManagerWithSchema)?.AddInfoSchema;
			if (addInfoSchema == null)
			{
				return new AddInfoDataObjectReader(logger, helper, column);
			}
			else
			{
				return new BusinessObjectAddInfoDataObjectReader(addInfoManager.GetType(), logger, helper, column, addInfoSchema);
			}
		}

		public ZString? GetCustomsUnitForPackType(PackageType packType)
		{
			return packType == null || !packType.Code.HasValue ? null : GetCustomsUnitForPackTypeCore(packType.GetCodeAsUpperCase());
		}

		protected virtual ZString? GetCustomsUnitForPackTypeCore(ZString? packType)
		{
			return packType;
		}

		public ZString? GetFreightUnitForPackType(ZString? packType)
		{
			return GetFreightUnitForPackTypeCore(packType);
		}

		protected virtual ZString? GetFreightUnitForPackTypeCore(ZString? packType)
		{
			if (packType.HasValue)
			{
				var query = new ZQuery(RefPacksSchema.RP_Type, RPTypeList.Codes.PackingDeclaration);
				query.AddToFilter(RefPacksSchema.RP_CustomsCountry, SourceCountryCode);
				query.AddToFilter(RefPacksSchema.RP_CustomsPack, packType);
				query.AddToFilter(RefPacksSchema.RP_OH_Supplier, DBNull.Value);
				query.OrderBy = RefPacksSchema.RP_CommercialPack.Name;
				packType = Factory.LoadTop1<BaseRefPacks>(query)?.RP_CommercialPack ?? packType;
			}

			return packType;
		}

		public virtual ZString GetCustomsBillType(WayBillType wayBillType)
		{
			return wayBillType.GetCustomsBillType();
		}

		public Dictionary<CommercialInvoiceLine, EntryLine> GetCommInvToEntryLineMapForBonded(Shipment universalShipment)
		{
			var result = new Dictionary<CommercialInvoiceLine, EntryLine>();

			var commercialInfo = universalShipment.CommercialInfo;
			if (commercialInfo != null)
			{
				Dictionary<ZString, Dictionary<ZShort, EntryLine>> entryLineMap = null;

				foreach (var invoiceHeader in commercialInfo.CommercialInvoiceCollection)
				{
					foreach (var invoiceLine in invoiceHeader.CommercialInvoiceLineCollection)
					{
						if (invoiceLine.BondedWarehouseQuantity.HasValue)
						{
							if (invoiceLine.EntryNumber.HasValue && invoiceLine.EntryLineNumber.HasValue)
							{
								if (entryLineMap == null)//build the first time only
								{
									//build the entry to entry line map so we dont have to repeatedly iterate over entry, entrynumber and entrylines (could be 1000's). 
									entryLineMap = EntryToEntryLineMap(universalShipment);
								}

								var locatedEntry = entryLineMap[invoiceLine.EntryNumber.Value];

								if (locatedEntry != null)
								{
									var locatedEntryLine = locatedEntry[invoiceLine.EntryLineNumber.Value];

									if (locatedEntryLine != null)
									{
										result.Add(invoiceLine, locatedEntryLine);
									}
								}
							}
						}
					}
				}
			}

			return result;
		}

		Dictionary<ZString, Dictionary<ZShort, EntryLine>> EntryToEntryLineMap(Shipment universalShipment)
		{
			var result = new Dictionary<ZString, Dictionary<ZShort, EntryLine>>();

			foreach (var entry in universalShipment.EntryHeaderCollection)
			{
				var entryLines = new Dictionary<ZShort, EntryLine>();

				foreach (var entryLine in entry.EntryLineCollection)
				{
					if (entryLine.LineNumber.HasValue)
					{
						//if LineNumber is repeated, this should throw an exception because something is wrong because LineNumber is the candidate (natural) key
						entryLines.Add(entryLine.LineNumber.Value, entryLine);
					}
				}

				foreach (var entryNumber in entry.EntryNumberCollection)
				{
					if (entryNumber.Number.HasValue)
					{
						if (!result.ContainsKey(entryNumber.Number.Value))//defensive. entry number should never be duplicated. If it is, then only one dictionary item is necessary
						{
							result.Add(entryNumber.Number.Value, entryLines);
						}
					}
				}
			}

			return result;
		}

		public IEnumerable<KeyValuePair<ZString, IAdditionalAddInfoGroupCollectionDataObjectReader>> GetAdditionalAddInfoGroupCollectionSupportFor(ZString parentTableCode, ZString type, IXmlImportLogger logger)
		{
			return GetAdditionalAddInfoGroupCollectionSupportForCore(parentTableCode, type, logger);
		}

		protected virtual IEnumerable<KeyValuePair<ZString, IAdditionalAddInfoGroupCollectionDataObjectReader>> GetAdditionalAddInfoGroupCollectionSupportForCore(ZString parentTableCode, ZString type, IXmlImportLogger logger)
		{
			return null;
		}

		public void AddPackageLinkMap(ZGuid packagePK, ZInt link)
		{
			ZGuid value;
			if (!PackageLinkMapDictionary.TryGetValue(link, out value))
			{
				PackageLinkMapDictionary.Add(link, packagePK);
			}
		}

		public ZGuid GetPackagePKFromPackageLinkMap(ZInt link)
		{
			ZGuid result;
			if (!PackageLinkMapDictionary.TryGetValue(link, out result))
			{
				result = ZGuid.Empty;
			}
			return result;
		}

		Dictionary<ZInt, ZGuid> PackageLinkMapDictionary
		{
			get { return packageLinkMapDictionary ?? (packageLinkMapDictionary = new Dictionary<ZInt, ZGuid>()); }
		}
		Dictionary<ZInt, ZGuid> packageLinkMapDictionary;

		public void AddPackageInvoiceLineMap(ZGuid packagePK, IEnumerable<PackedItem> packedItems)
		{
			if (packagePK.IsValid && packedItems != null)
			{
				foreach (var packedItem in packedItems)
				{
					var commercialInvoiceLineLink = packedItem.CommercialInvoiceLineLink.GetValueOrDefault();
					if (commercialInvoiceLineLink > ZInt.Zero)
					{
						Dictionary<ZGuid, PackedItem> map;
						if (!PackageInvoiceLineMapDictionary.TryGetValue(commercialInvoiceLineLink, out map))
						{
							map = new Dictionary<ZGuid, PackedItem>();
							PackageInvoiceLineMapDictionary.Add(commercialInvoiceLineLink, map);
						}
						map[packagePK] = packedItem;
					}
				}
			}
		}

		public void AddContainerInvoiceLineMap(ZGuid containerPK, IEnumerable<PackedItem> packedItems)
		{
			if (containerPK.IsValid && packedItems != null)
			{
				foreach (var packedItem in packedItems)
				{
					var commercialInvoiceLineLink = packedItem.CommercialInvoiceLineLink.GetValueOrDefault();
					if (commercialInvoiceLineLink > ZInt.Zero)
					{
						Dictionary<ZGuid, PackedItem> map;
						if (!ContainerInvoiceLineMapDictionary.TryGetValue(commercialInvoiceLineLink, out map))
						{
							map = new Dictionary<ZGuid, PackedItem>();
							ContainerInvoiceLineMapDictionary.Add(commercialInvoiceLineLink, map);
						}
						map[containerPK] = packedItem;
					}
				}
			}
		}

		public Dictionary<ZGuid, PackedItem> GetPackageInvoiceLineMapFor(ZInt commercialInvoiceLineLink)
		{
			Dictionary<ZGuid, PackedItem> map;
			return PackageInvoiceLineMapDictionary.TryGetValue(commercialInvoiceLineLink, out map) ? map : null;
		}

		Dictionary<ZInt, Dictionary<ZGuid, PackedItem>> PackageInvoiceLineMapDictionary
		{
			get { return packageInvoiceLineMapDictionary ?? (packageInvoiceLineMapDictionary = new Dictionary<ZInt, Dictionary<ZGuid, PackedItem>>()); }
		}
		Dictionary<ZInt, Dictionary<ZGuid, PackedItem>> packageInvoiceLineMapDictionary;

		public Dictionary<ZGuid, PackedItem> GetContainerInvoiceLineMapFor(ZInt commercialInvoiceLineLink)
		{
			Dictionary<ZGuid, PackedItem> map;
			return ContainerInvoiceLineMapDictionary.TryGetValue(commercialInvoiceLineLink, out map) ? map : null;
		}

		Dictionary<ZInt, Dictionary<ZGuid, PackedItem>> ContainerInvoiceLineMapDictionary
		{
			get { return containerInvoiceLineMapDictionary ?? (containerInvoiceLineMapDictionary = new Dictionary<ZInt, Dictionary<ZGuid, PackedItem>>()); }
		}
		Dictionary<ZInt, Dictionary<ZGuid, PackedItem>> containerInvoiceLineMapDictionary;

		public void AddEntryLineMap(CusEntryLine entryLine, ZString entryType, ZString entryNumber)
		{
			List<EntryDetails> entryDetails;
			if (!EntryNumberToEntryLineMapDictionary.TryGetValue(entryNumber, out entryDetails))
			{
				entryDetails = new List<EntryDetails>();
				EntryNumberToEntryLineMapDictionary.Add(entryNumber, entryDetails);
			}
			var entryDetail = entryDetails.FirstOrDefault(x => x.Type == entryType);
			if (entryDetail == null)
			{
				entryDetail = new EntryDetails() { Type = entryType };
				entryDetails.Add(entryDetail);
			}
			if (!entryDetail.EntryLines.Contains(entryLine))
			{
				entryDetail.EntryLines.Add(entryLine);
			}
		}

		Dictionary<ZString, List<EntryDetails>> EntryNumberToEntryLineMapDictionary
		{
			get { return entryNumberToEntryLineMapDictionary ?? (entryNumberToEntryLineMapDictionary = new Dictionary<ZString, List<EntryDetails>>()); }
		}
		Dictionary<ZString, List<EntryDetails>> entryNumberToEntryLineMapDictionary;

		class EntryDetails
		{
			public ZString Type;
			public List<CusEntryLine> EntryLines
			{
				get { return entryLines ?? (entryLines = new List<CusEntryLine>()); }
			}
			List<CusEntryLine> entryLines;
		}

		public CusEntryLine[] GetEntryLines(ZString entryNumber, ZShort entryLineNumber)
		{
			var entryLines = new List<CusEntryLine>();
			List<EntryDetails> entryDetails;
			if (EntryNumberToEntryLineMapDictionary.TryGetValue(entryNumber, out entryDetails))
			{
				foreach (var entryDetail in entryDetails)
				{
					entryLines.AddRange(entryDetail.EntryLines.Where(x => !x.IsDeleted && x.CL_LineNumber == entryLineNumber));
				}
			}
			return entryLines.Distinct().ToArray();
		}

		public ZGuid? GetOrganisationPK(IOrganisationDataObjectReaderSupporter dataObjectReader, IOrganizationAddressCollectionParent dataObject, BusinessObject bizObj, ZString addressType, OrganisationTypes orgCategory, string orgType = null)
		{
			ZGuid? result = null;
			ZGuid organisationPK;
			ZGuid addressPK;
			if (dataObjectReader.TryGetMatchedOrganisationData(out organisationPK, out addressPK, dataObject, bizObj, addressType, orgCategory, orgType))
			{
				result = organisationPK;
			}
			return result;
		}

		public ZGuid? GetAddressPK(IOrganisationDataObjectReaderSupporter dataObjectReader, IOrganizationAddressCollectionParent dataObject, BusinessObject bizObj, ZString addressType, OrganisationTypes orgCategory, string orgType = null)
		{
			ZGuid? result = null;
			ZGuid organisationPK;
			ZGuid addressPK;
			if (dataObjectReader.TryGetMatchedOrganisationData(out organisationPK, out addressPK, dataObject, bizObj, addressType, orgCategory, orgType))
			{
				result = addressPK;
			}
			return result;
		}

		public void FillDocAddresses(IDocAddresses docAddresses, List<OrganizationAddress> organizationAddressCollection, IXmlImportLogger logger)
		{
			if (organizationAddressCollection != null)
			{
				var supportedAddressTypes = docAddresses?.SupportedAddressTypes;
				if (supportedAddressTypes?.Any() ?? false)
				{
					foreach (var orgAddressDataObject in organizationAddressCollection)
					{
						var addressType = orgAddressDataObject.AddressType.GetValueOrDefault();
						if (Enum.TryParse(addressType, false, out DocAddressType docAddressType) && supportedAddressTypes.Contains(docAddressType))
						{
							OrganisationDataObjectReader.MatchedOrNew(docAddresses, orgAddressDataObject, logger, Factory, null);
						}
					}
				}
			}
		}

		public CommercialInfoMatcher SetupCommercialInfoMatcher(BaseJobDeclaration declaration, IXmlImportLogger logger, CommercialInfo commercialInfo, CollectionContent? commercialInvoiceCollectionContent)
		{
			CommercialInfoMatcher = new CommercialInfoMatcher(declaration, logger, Factory, commercialInfo, commercialInvoiceCollectionContent);
			return CommercialInfoMatcher;
		}

		public CommercialInfoMatcher CommercialInfoMatcher { get; private set; }

		#region Data Import Matching Keys

		public ZString[] GetDataImportMatchingKeys(CommercialInvoiceHeader invoiceHeaderDataObject)
		{
			ZString[] result = null;
			if (invoiceHeaderDataObject == null)
			{
				result = Array.Empty<ZString>();
			}
			else if (!ImportMatchingKeys.TryGetValue(invoiceHeaderDataObject, out result))
			{
				result = invoiceHeaderDataObject.CommercialInvoiceLineCollection
					?.Select(c => c.DataImportMatchingKey.GetValueOrDefault())
					.Where(c => !c.IsEmpty)
					.ToArray() ?? Array.Empty<ZString>();

				ImportMatchingKeys.Add(invoiceHeaderDataObject, result);
			}
			return result;
		}

		Dictionary<CommercialInvoiceHeader, ZString[]> ImportMatchingKeys
		{
			get
			{
				if (importMatchingKeys == null)
				{
					importMatchingKeys = new Dictionary<CommercialInvoiceHeader, ZString[]>();
				}

				return importMatchingKeys;
			}
		}
		Dictionary<CommercialInvoiceHeader, ZString[]> importMatchingKeys;

		#endregion

		#region EntryInstruction Link Dictionary

		public void RegisterEntryInstructionPK(ZInt link, ZGuid pk)
		{
			EntryInstructionDictionary[link] = pk;
		}

		public ZGuid? GetEntryInstructionPK(ZInt? link)
		{
			ZGuid? result = null;
			if (link.HasValue && EntryInstructionDictionary.ContainsKey(link.Value))
			{
				result = EntryInstructionDictionary.GetValueSafe(link.Value);
			}
			return result;
		}

		Dictionary<ZInt, ZGuid> EntryInstructionDictionary { get { return entryInstructionDictionary ?? (entryInstructionDictionary = new Dictionary<ZInt, ZGuid>()); } }
		Dictionary<ZInt, ZGuid> entryInstructionDictionary;

		#endregion

		#region Orders
		public ZGuid? GetMatchingOrderPK(CommercialInvoiceLine invoiceLineData, BaseJobComInvoiceLine invoiceLine, ITopLevelDataObject topLevelDataObject, IXmlImportLogger logger)
		{
			ZGuid? result = null;
			if (invoiceLineData.OrderNumber.HasValue && invoiceLineData.OrderLineLink.HasValue && topLevelDataObject is Shipment shipment)
			{
				var dataTarget = shipment.GetMatchingDataTarget(DataContextType.CustomsDeclaration);
				if (dataTarget != null && invoiceLine.Declaration != null)
				{
					var matchingSupplier = GetFallbackInvoiceBuyerOrganization(dataTarget, topLevelDataObject, logger);
					BusinessOrder order = null;
					if (matchingSupplier != null)
					{
						var dictionary = GetMatchKeyOrderDictionary(dataTarget.Key, shipment, invoiceLine.Declaration, matchingSupplier);
						var linaDataOrderNoAndSplit = invoiceLineData.OrderNumber.Value;
						foreach (var dic in dictionary)
						{
							var splitedKey = dic.Key.Split('~');
							if (splitedKey.Length == 3)
							{
								var orderNoAndSplit = string.Format(CultureInfo.InvariantCulture, "{0}-{1}", splitedKey[0], splitedKey[1]);
								if ((orderNoAndSplit == linaDataOrderNoAndSplit || orderNoAndSplit == string.Format(CultureInfo.InvariantCulture, "{0}-0", linaDataOrderNoAndSplit)) && splitedKey[2] == matchingSupplier.OH_Code)
								{
									order = dic.Value;
									break;
								}
							}
						}
					}
					result = order?.OrderLines.FirstOrDefault(x => x.JO_LineNo == invoiceLineData.OrderLineLink)?.PK;
				}
			}
			return result;
		}

		internal Dictionary<ZString, BusinessOrder> GetMatchKeyOrderDictionary(ZString? dataTargetKey, Shipment shipment, BaseJobDeclaration declaration, OrgHeader matchingSupplier)
		{
			var cacheKey = string.Format(CultureInfo.InvariantCulture, "GetMatchKeyOrderDictionary:{0}_{1}", dataTargetKey.HasValue ? dataTargetKey.ToString() : ZGuid.NewZGuid().ToString(), matchingSupplier.PK);
			return Factory.BOFactory.GetCachedValue(cacheKey, () =>
			{
				var dic = new Dictionary<ZString, BusinessOrder>();
				if (shipment.RelatedShipmentCollection != null && matchingSupplier != null)
				{
					foreach (var relatedShipment in shipment.RelatedShipmentCollection)
					{
						var key = relatedShipment.DataContext.GetMatchingDataTarget(DataContextType.OrderManagerOrder)?.Key ?? ZString.Empty;
						if (!key.IsEmpty)
						{
							var order = declaration.AttachedOrders.FirstOrDefault(o =>
								o.Buyer == matchingSupplier && o.JD_OrderNumber == (relatedShipment.Order?.OrderNumber.GetValueOrDefault() ?? ZString.Empty) &&
								(!relatedShipment.Order.OrderNumberSplit.HasValue || o.JD_OrderNumberSplit == relatedShipment.Order.OrderNumberSplit.Value));
							if (order != null)
							{
								if (!dic.ContainsKey(key))
								{
									dic.Add(key, order);
								}
								else
								{
									dic[key] = order;
								}
							}
						}
					}
				}
				return dic;
			});
		}

		OrgHeader GetFallbackInvoiceBuyerOrganization(IDataTargetDataObject dataTarget, ITopLevelDataObject topLevelDataObject, IXmlImportLogger logger)
		{
			return Factory.BOFactory.GetCachedValue("BuyerOrganization" + dataTarget.Key, () =>
			{
				OrgHeader buyerOrgHeader = null;
				if (topLevelDataObject is Shipment shipment)
				{
					buyerOrgHeader = GetOrganization(shipment.OrganizationAddressCollection, logger, nameof(DocAddressType.ConsigneeDocumentaryAddress)) ??
						GetOrganization(shipment.OrganizationAddressCollection, logger, nameof(DocAddressType.ImporterDocumentaryAddress));
				}
				return buyerOrgHeader;
			});
		}

		OrgHeader GetOrganization(List<OrganizationAddress> addresses, IXmlImportLogger logger, params ZString[] documentTypes)
		{
			OrgAddress address = null;
			var orgAddressData = addresses?.FirstOrDefault(documentTypes);
			if (orgAddressData != null)
			{
				var orgAddress = orgAddressData.GetMatchedUsingCodes(Factory.BOFactory);
				if (orgAddress != null)
				{
					address = orgAddress;
				}
				if (address == null)
				{
					address = new OrganisationDataObjectReader(orgAddressData, logger, Factory).GetMatched();
				}
			}
			return address?.Header;
		}
		#endregion

		#region StmNote

		public StmNote LoadOrCreateStmNoteForReaderUpdate(ZGuid parentPK, ZString tableName, bool isParentInDatabase, ZString noteDescription, StmNoteVisibility noteVisibility = StmNoteVisibility.DOC, string noteContext = "AAA")
		{
			var noteType = noteVisibility.ToString();
			return LoadStmNote(parentPK, tableName, noteDescription, noteType, noteContext, isParentInDatabase) ?? CreateNewStmNote(parentPK, tableName, noteDescription, noteType, noteContext);
		}

		StmNote LoadStmNote(ZGuid parentPK, ZString tableName, ZString noteDescription, ZString noteType, ZString noteContext, bool isParentInDatabase)
		{
			var query = new ZQuery(StmNoteSchema.ST_ParentID, parentPK);
			query.AddToFilter(StmNoteSchema.ST_Table, tableName);
			query.AddToFilter(StmNoteSchema.ST_Description, noteDescription);
			query.AddToFilter(StmNoteSchema.ST_NoteType, noteType);
			query.AddToFilter(StmNoteSchema.ST_NoteContext, noteContext);
			query.FetchOnlyFromLocalCache = !isParentInDatabase;

			return Factory.LoadTop1<StmNote>(query);
		}

		public StmNote CreateNewStmNote(ZGuid parentPK, ZString tableName, ZString noteDescription, ZString noteType, ZString noteContext)
		{
			var note = factory.New<StmNote>();
			note.ST_ParentID = parentPK;
			note.ST_Table = tableName;
			note.ST_Description = noteDescription;
			note.ST_NoteType = noteType;
			note.ST_NoteContext = noteContext;
			return note;
		}

		#endregion

	}
}
