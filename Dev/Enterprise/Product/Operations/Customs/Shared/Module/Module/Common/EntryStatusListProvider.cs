using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants.Customs.Universal;
using IntegratedCountryEntryStatus = Enterprise.Customs.Common.Shared.IntegratedCountryCommonEntryStatusList;

namespace Enterprise.Customs.Module
{
	public class EntryStatusListProvider : Integration.Customs.Shared.IEntryStatusListProvider
	{
		public EntryStatusListProvider()
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String comparison")]
		public ICodeDescriptionPairList EntryStatusList(BusinessObjectFactory factory, ZString countryCode, ZString messageType)
		{
			var listToClone = EntryStatusListCore(factory);
			if (listToClone.Count == 0)
			{
				if (messageType == JobMessageTypeList.Codes.Export)
				{
					listToClone = EntryStatus_Export_List;
				}
				else if (messageType == JobMessageTypeList.Codes.Import)
				{
					listToClone = EntryStatus_Import_List;
				}
				else
				{
					listToClone = EntryStatus_ImportExport_List;
				}

				if (listToClone.Count == 0)
				{
					listToClone = new CodeDescriptionPairList(Universal.RefCusCodeListTypes.GetCachedList(factory, countryCode, RefCusCodeListTypes.Codes.CustomsStatus, ZDateTime.Today));
				}

				var result = new CodeDescriptionPairList();
				foreach (ICodeDescription pair in listToClone)
				{
					if (pair.Description == "Not Sent")
					{
						result.Add(new CodeDescriptionPair(DeclarationFilterConstants.EntryStatus.NotSentForFilter, pair.Description));
					}
					else
					{
						result.Add(new CodeDescriptionPair(pair.Code, pair.Description));
					}
				}
				var matchCurrentCompnayCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode.EqualsIgnoringCase(countryCode);
				if (matchCurrentCompnayCountry && IntegratedCountryHelper.CustomsWareInstallations(countryCode) && !string.IsNullOrEmpty(CustomsWare.Business.CustomsWareRegistry.Instance.Password.Value))
				{
					foreach (ICodeDescription pair in EntryStatusListForCustomsWare)
					{
						result.AddPairIfNotExist(pair.Code, pair.Description);
					}
				}

				if (result.Count == 0)
				{
					foreach (ICodeDescription pair in EntryStatusListForDefaultFallBack)
					{
						result.AddPairIfNotExist(pair.Code, pair.Description);
					}
				}

				if (matchCurrentCompnayCountry)
				{
					var customsInterface = CustomsDataRegistry.Instance.LocalCountryCustomsInterface.Value;
					if (IntegratedCountryHelper.CustomsInterfaceIsActivated(customsInterface) && ValidSubmissionTypes.Contains(customsInterface.SubmissionType))
					{
						var listForInterface = Universal.RefCusCodeListTypes.GetCachedList(factory, countryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatusForInterface, ZDateTime.Today);
						result.AddPairsIfNotExist(listForInterface.Cast<ICodeDescription>());
					}
				}

				result.AddPairIfNotExist(IntegratedCountryEntryStatus.Codes.Submitted, IntegratedCountryEntryStatus.Descriptions.Submitted);
				result.AddPairIfNotExist(IntegratedCountryEntryStatus.Codes.Acknowledged, IntegratedCountryEntryStatus.Descriptions.Acknowledged);

				return ReplaceSpecialCodeDescription(result);
			}
			else
			{
				return listToClone;
			}
		}

		public ICodeDescriptionPairList EntryStatusListForShipments(BusinessObjectFactory factory, ZString countryCode) => EntryStatusListForShipmentsCore(factory, countryCode);

		protected virtual ICodeDescriptionPairList EntryStatusListCore(BusinessObjectFactory factory) => new CodeDescriptionPairList();

		protected virtual ICodeDescriptionPairList EntryStatusListForShipmentsCore(BusinessObjectFactory factory, ZString countryCode) => EntryStatusList(factory, countryCode, ZString.Empty);

		protected virtual CodeDescriptionPairList ReplaceSpecialCodeDescription(CodeDescriptionPairList codePairList) => codePairList;

		protected virtual CodeDescriptionPairList EntryStatus_Import_List => new CodeDescriptionPairList();

		protected virtual CodeDescriptionPairList EntryStatus_Export_List => new CodeDescriptionPairList();

		protected virtual CodeDescriptionPairList EntryStatus_ImportExport_List => new CodeDescriptionPairList();

		protected virtual CodeDescriptionPairList EntryStatusListForCustomsWare => new CodeDescriptionPairList();

		protected virtual CodeDescriptionPairList EntryStatusListForDefaultFallBack => new CodeDescriptionPairList();

		ZString[] ValidSubmissionTypes => new ZString[]
		{
			DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted,
			DeclarationApplicationCodeList.Codes.Interfaced,
			DeclarationApplicationCodeListForRegistry.Codes.BothInterfaceDefaulted
		};
	}
}
