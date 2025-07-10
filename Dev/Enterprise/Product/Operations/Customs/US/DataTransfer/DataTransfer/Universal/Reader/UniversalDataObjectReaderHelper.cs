using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class UniversalDataObjectReaderHelper : Customs.DataTransfer.Universal.UniversalDataObjectReaderHelper
	{
		public UniversalDataObjectReaderHelper(UniversalObjectFactory factory, ZString sourceCountryCode)
			: base(factory, Core.Constants.CountryCodes.UnitedStates, sourceCountryCode)
		{
		}

		public void Update(Dictionary<ZString, ZString> addInfos, SchemaGuidColumn column, ZGuid pk)
		{
			var key = column.Name.Substring(3);
			addInfos.Update(column.Name.Substring(3), pk);
		}

		protected override IDictionary<string, string> CreateSupportedCodeMappingRelationshipCodeDictionary()
		{
			var supportedCodeMappingRelationshipCodeDictionary = base.CreateSupportedCodeMappingRelationshipCodeDictionary();
			supportedCodeMappingRelationshipCodeDictionary.Add(USAddInfoSchema.Constants.US_UC_NKCountryOfExport.Substring(3), Core.Constants.OrgPatternMatchOverrideRelationships.Country);
			supportedCodeMappingRelationshipCodeDictionary.Add(USAddInfoSchema.Constants.US_UC_NKCountryOfOrigin.Substring(3), Core.Constants.OrgPatternMatchOverrideRelationships.Country);
			return supportedCodeMappingRelationshipCodeDictionary;
		}

		protected override IEnumerable<ZString> GetMatchingKeysInSettingOrderCore(IColumnIndexer row)
		{
			var fda = row as FDA;
			if (fda != null)
			{
				return SettingOrderDeterminer.GetSettingOrder(fda);
			}
			var acefda = row as ACEFDA;
			if (acefda != null)
			{
				return SettingOrderDeterminer.GetSettingOrder(acefda);
			}
			var aiiLine = row as AIILine;
			if (aiiLine != null)
			{
				return SettingOrderDeterminer.GetSettingOrder(aiiLine);
			}
			var nmfsLine = row as NMFSHarvestingDetail;
			if (nmfsLine != null)
			{
				return SettingOrderDeterminer.GetSettingOrder(nmfsLine);
			}
			return base.GetMatchingKeysInSettingOrderCore(row);
		}

		protected override AddInfoDataObjectReader GetNewAddInfoDataObjectReaderCore(IAddInfoManager addInfoManager, IXmlImportLogger logger, Customs.DataTransfer.Universal.UniversalDataObjectReaderHelper helper, SchemaStringColumn column)
		{
			AddInfoDataObjectReader result;
			var type = addInfoManager.GetType();

			var universalDataObjectReaderHelper = helper as UniversalDataObjectReaderHelper;

			if (typeof(OGADispositionData) == type)
			{
				result = new AddInfoDataObjectReader<OGADispositionData>(logger, helper, column, USOGADispositionDataAddInfoSchema.Instance);
			}
			else if (typeof(LinkedEntry) == type)
			{
				result = new AddInfoDataObjectReader<LinkedEntry>(logger, helper, column, USLinkedEntryAddInfoSchema.Instance);
			}
			else if (typeof(ITDoc) == type)
			{
				result = new AddInfoDataObjectReader<ITDoc>(logger, helper, column, USITDocAddInfoSchema.Instance);
			}
			else if (typeof(DispositionData) == type)
			{
				result = new AddInfoDataObjectReader<DispositionData>(logger, helper, column, USDispositionDataAddInfoSchema.Instance);
			}
			else if (typeof(DOTVIN) == type)
			{
				result = new AddInfoDataObjectReader<DOTVIN>(logger, helper, column, USDOTVINAddInfoSchema.Instance);
			}
			else if (typeof(ScientificData) == type)
			{
				result = new AddInfoDataObjectReader<ScientificData>(logger, helper, column, USScientificDataAddInfoSchema.Instance);
			}
			else if (typeof(ITAndSplitDetails) == type)
			{
				result = new AddInfoDataObjectReader<ITAndSplitDetails>(logger, helper, column, USITNumberAddInfoSchema.Instance);
			}
			else if (typeof(AIILine) == type)
			{
				result = new AddInfoDataObjectReader<AIILine>(logger, helper, column, USAIILineAddInfoSchema.Instance);
			}
			else if (typeof(FDA) == type)
			{
				result = new AddInfoDataObjectReaderForFDA(logger, universalDataObjectReaderHelper);
			}
			else if (typeof(DOT) == type)
			{
				result = new AddInfoDataObjectReader<DOT>(logger, helper, column, USDOTAddInfoSchema.Instance);
			}
			else if (typeof(FCC) == type)
			{
				result = new AddInfoDataObjectReader<FCC>(logger, helper, column, USFCCAddInfoSchema.Instance);
			}
			else if (typeof(DOT) == type)
			{
				result = new AddInfoDataObjectReader<DOT>(logger, helper, column, USDOTAddInfoSchema.Instance);
			}
			else if (typeof(DOT) == type)
			{
				result = new AddInfoDataObjectReader<DOT>(logger, helper, column, USDOTAddInfoSchema.Instance);
			}
			else if (typeof(PGA) == type)
			{
				result = new AddInfoDataObjectReader<PGA>(logger, helper, column, USPGAAddInfoSchema.Instance);
			}
			else if (typeof(DrawbackNAFTA) == type)
			{
				result = new AddInfoDataObjectReader<DrawbackNAFTA>(logger, helper, column, USDrawbackNAFTAAddInfoSchema.Instance);
			}
			else if (typeof(USInvoiceLineFSISLine) == type)
			{
				result = new AddInfoDataObjectReader<USInvoiceLineFSISLine>(logger, helper, column, USFSISLineAddInfoSchema.Instance);
			}
			else if (typeof(USDeclarationFSISLine) == type)
			{
				result = new AddInfoDataObjectReader<USDeclarationFSISLine>(logger, helper, column, USFSISLineAddInfoSchema.Instance);
			}
			else if (typeof(USFSISLot) == type)
			{
				result = new AddInfoDataObjectReader<USFSISLot>(logger, helper, column, USFSISLotAddInfoSchema.Instance);
			}
			else
			{
				result = GetOtherAddInfoDataObjectReaders(addInfoManager, logger, helper, column);
			}
			return result;
		}

		AddInfoDataObjectReader GetOtherAddInfoDataObjectReaders(IAddInfoManager addInfoManager, IXmlImportLogger logger, Customs.DataTransfer.Universal.UniversalDataObjectReaderHelper helper, SchemaStringColumn column)
		{
			AddInfoDataObjectReader result;
			var type = addInfoManager.GetType();

			var universalDataObjectReaderHelper = helper as UniversalDataObjectReaderHelper;

			if (typeof(AMSLine) == type)
			{
				result = new AddInfoDataObjectReaderForAMSLine(logger, universalDataObjectReaderHelper);
			}
			else if (typeof(APHISHeader) == type)
			{
				result = new AddInfoDataObjectReaderForAPHIS(logger, universalDataObjectReaderHelper);
			}
			else if (typeof(ACEFDA) == type)
			{
				result = new AddInfoDataObjectReaderForACE_FDA(logger, universalDataObjectReaderHelper);
			}
			else if (typeof(NHTSAHeader) == type)
			{
				result = new AddInfoDataObjectReaderForNHTSA(logger, universalDataObjectReaderHelper);
			}
			else if (typeof(Pesticide) == type)
			{
				result = new AddInfoDataObjectReaderForPesticide(logger, universalDataObjectReaderHelper);
			}
			else if (typeof(TTBLine) == type)
			{
				result = new AddInfoDataObjectReaderForTTB(logger, universalDataObjectReaderHelper);
			}
			else if (typeof(OMCHeader) == type)
			{
				result = new AddInfoDataObjectReaderForOMCHeader(logger, universalDataObjectReaderHelper);
			}
			else if (typeof(USOMCAquacultureFacility) == type)
			{
				result = new AddInfoDataObjectReaderForOMCLine(logger, universalDataObjectReaderHelper);
			}
			else if (typeof(Vehicle) == type)
			{
				result = new AddInfoDataObjectReaderForVehicle(logger, universalDataObjectReaderHelper);
			}
			else if (typeof(ConstituentElement) == type)
			{
				result = new AddInfoDataObjectReaderForConstituentElement(logger, universalDataObjectReaderHelper);
			}
			else if (typeof(DEAHeader) == type)
			{
				result = new AddInfoDataObjectReaderForDEA(logger, universalDataObjectReaderHelper);
			}
			else if (typeof(CPSCHeader) == type)
			{
				result = new AddInfoDataObjectReaderForCPSCHeader(logger, universalDataObjectReaderHelper);
			}
			else if (typeof(CPSCRule) == type)
			{
				result = new AddInfoDataObjectReaderForCPSCLine(logger, universalDataObjectReaderHelper);
			}
			else if (typeof(NMFSHarvestingDetail) == type)
			{
				result = new AddInfoDataObjectReaderForNMFS(logger, universalDataObjectReaderHelper);
			}
			else if (typeof(DrawbackAdditionalImportTariffNumber) == type)
			{
				result = new AddInfoDataObjectReaderForDrawbackAdditionalImportTariffNumber(logger, universalDataObjectReaderHelper);
			}
			else
			{
				result = base.GetNewAddInfoDataObjectReaderCore(addInfoManager, logger, helper, column);
			}
			return result;
		}

		protected override ZString? GetCustomsUnitForPackTypeCore(ZString? packType)
		{
			if (packType.HasValue)
			{
				var mappings = USCustomsDataRegistry.Instance.USPackageTypesMapping.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				return mappings.GetMappedPackageType(packType.Value);
			}
			else
			{
				return base.GetCustomsUnitForPackTypeCore(packType);
			}
		}

		protected override ZString? GetFreightUnitForPackTypeCore(ZString? packType)
		{
			if (packType.HasValue
				&& USCustomsDataRegistry.Instance.USPackageTypesMapping.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)
					.GetMappedFreightPackagePair(packType.Value) is PackageTypePair packageTypePair)
			{
				return packageTypePair.FreightPackageType;
			}
			else
			{
				return base.GetFreightUnitForPackTypeCore(packType);
			}
		}

		protected override IEnumerable<KeyValuePair<ZString, IAdditionalAddInfoGroupCollectionDataObjectReader>> GetAdditionalAddInfoGroupCollectionSupportForCore(ZString parentTableCode, ZString type, IXmlImportLogger logger)
		{
			if (parentTableCode == CusAddInfoSchema.Constants.Prefix && type == CusAddInfoTypeAttribute.Codes.USFDA)
			{
				yield return new KeyValuePair<ZString, IAdditionalAddInfoGroupCollectionDataObjectReader>(CusAddInfoTypeListProvider.AdditionalAddInfoType.FDARelatedBill.Type, new AdditionalAddInfoGroupCollectionDataObjectReaderForFDARelatedBill(logger, this));
				yield return new KeyValuePair<ZString, IAdditionalAddInfoGroupCollectionDataObjectReader>(CusAddInfoTypeListProvider.AdditionalAddInfoType.FDARelatedContainer.Type, new AdditionalAddInfoGroupCollectionDataObjectReaderForFDARelatedContainer(logger, this));
			}
			else if (parentTableCode == JobComInvoiceLineSchema.Constants.Prefix && type == CusAddInfoTypeAttribute.Codes.USWHSPackLine)
			{
				yield return new KeyValuePair<ZString, IAdditionalAddInfoGroupCollectionDataObjectReader>(CusAddInfoTypeAttribute.Codes.USWHSPackLine, new AdditionalAddInfoGroupCollectionDataObjectReaderForInvoiceLineWHSPackLine(logger, this));
			}
		}
	}
}
