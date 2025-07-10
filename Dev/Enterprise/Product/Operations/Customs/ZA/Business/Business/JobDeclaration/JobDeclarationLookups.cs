using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public class JobDeclarationLookups : AutoZAJobDeclarationLookups
	{
		public JobDeclarationLookups(JobDeclaration parent)
			: base(parent)
		{
		}

		public JobDeclaration Declaration
		{
			get { return (JobDeclaration)Parent; }
		}

		public override CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<ZAMessageStatusList>();

		public override IBusinessObjectCollection CarrierCodeCollection
		{
			get
			{
				return Declaration.Lookups.CarrierCodeList;
			}
		}

		public RadioCallSignCodeFindBoxCollection RadioCallSignVessels => radioCallSignVessels ?? (radioCallSignVessels = new RadioCallSignCodeFindBoxCollection(Factory));
		RadioCallSignCodeFindBoxCollection radioCallSignVessels;

		#region CodeDescriptionPairLists
		public override CodeDescriptionPairList TransportTypeList
		{
			get { return Factory.GetCachedValue<TransportModeList>(); }
		}

		public ICodeDescriptionPairList RemovalTransportCodeList
		{
			get { return Factory.GetCachedValue<RemovalTransportModeList>(); }
		}

		protected override IEnumerable<ZString> GetNonSupportedMessageTypeCodes()
		{
			if (Declaration.JE_ApplicationCode != Customs.Business.DeclarationApplicationCodeList.Codes.Builtin)
			{
				yield return ZAJobMessageTypeList.Codes.ImportByExternalBroker;
			}
		}

		public override CodeDescriptionPairList PaymentPartyList
		{
			get { return Factory.GetCachedValue<PaidByCodeList>(); }
		}

		public override CodeDescriptionPairList ApplicationCodeList
		{
			get { return Factory.GetCachedValue<Customs.Business.DeclarationApplicationCodeList>(); }
		}

		public override CodeDescriptionPairList MergeByList
		{
			get
			{
				var filteredMergeByList = new CodeDescriptionPairList(base.MergeByList);

				if (!Declaration.IsMergeByValidForPreviousProcedureCode)
				{
					const string discouragedPrefix = "[Discouraged]: ";
					filteredMergeByList.RemoveCode(Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff);
					filteredMergeByList.RemoveCode(Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.TariffAndDescription);
					filteredMergeByList.RemoveCode(Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.Classification);
					filteredMergeByList.RemoveCode(Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.ClassificationUsingClassificationDescriptionAlways);
					filteredMergeByList.RemoveCode(Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.PartNumber);
					filteredMergeByList.RemoveCode(Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.PartNumberUsingProductNumberInDescription);
					filteredMergeByList.AddPair(Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff, ResString.GetMultilingualString("MasterFiles|MergeInvoiceLines|DiscouragedTariff", discouragedPrefix + "Tariff"));
					filteredMergeByList.AddPair(Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.TariffAndDescription, ResString.GetMultilingualString("MasterFiles|MergeInvoiceLines|DiscouragedTariffAndDescription", discouragedPrefix + "Tariff and Description"));
					filteredMergeByList.AddPair(Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.Classification, ResString.GetMultilingualString("MasterFiles|MergeInvoiceLines|DiscouragedClassification", discouragedPrefix + "Classification"));
					filteredMergeByList.AddPair(Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.ClassificationUsingClassificationDescriptionAlways, ResString.GetMultilingualString("MasterFiles|DiscouragedMergeInvoiceLines|ClassificationUsingClassificationDescriptionAlways", discouragedPrefix + "Classification (use classification description always)"));
					filteredMergeByList.AddPair(Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.PartNumber, ResString.GetMultilingualString("MasterFiles|MergeInvoiceLines|DiscouragedPartNumber", discouragedPrefix + "Product Number"));
					filteredMergeByList.AddPair(Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.PartNumberUsingProductNumberInDescription, ResString.GetMultilingualString("MasterFiles|MergeInvoiceLines|DiscouragedPartNumberUsingProductNumberInDescription", discouragedPrefix + "Product Number (include product number in description)"));
				}

				return filteredMergeByList;
			}
		}

		public override IBusinessObjectCollection LocationOfGoodsCollection
		{
			get
			{
				var collection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory,
					Core.Constants.CountryCodes.SouthAfrica,
					Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities,
					ZDateTime.Today);

				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.AttributeName, "Property", new ZString(RefCusCodeListAttributeTypes.Codes.DistrictOffices)));
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.AttributeValue, "Property", Declaration.JE_CustomsOffice));

				return collection;
			}
		}

		#endregion

		#region AddInfo Lookups

		public override ICodeDescriptionPairList CustomsOfficeList => ZARefCusCodeListTypes.GetCustomsOfficeList(Factory);

		public RefUNLOCOCollection UZ_RL_NKMasterBillIssuedAtList => Declaration.Lookups.MasterBillIssuedAts;

		public ICodeDescriptionPairList ROOTypesList => ZARefCusCodeListTypes.GetAddInWithROOTypeAttribute(Factory, ZDateTime.Today);

		public ZZRefCarrierCombinedCollection CarrierCodeList
			=> ZZRefCarrierCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.SouthAfrica, RefCarrierTypeList.Codes.Master, Declaration.JE_TransportMode);

		public ZZRefCarrierCombinedCollection CargoCarrierCodeList
			=> ZZRefCarrierCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.SouthAfrica, RefCarrierTypeList.Codes.CargoCarrier, Declaration.JE_TransportMode);

		public ZZRefCusCodeListCombinedCollection VesselAgentList
		{
			get
			{
				return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory
					, Core.Constants.CountryCodes.SouthAfrica
					, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.VesselAgent
					, ZDateTime.Today);
			}
		}

		public CodeDescriptionPairList VATClaimBackIndicator => Factory.GetCachedValue<VATClaimBackIndicatorCodeList>();

		public ICodeDescriptionPairList PortsOfExit => ZARefCusCodeListTypes.GetCustomsOfficeList(Factory);

		public ICodeDescriptionPairList BankCodes => ZARefCusCodeListTypes.GetBankCodeList(Factory);

		public CodeDescriptionPairList EntityTypeList => Factory.GetCachedValue<EntityTypeList>();

		public CodeDescriptionPairList RefTypeList => Factory.GetCachedValue<RefTypeList>();

		public CodeDescriptionPairList ScopeList => Factory.GetCachedValue<ScopeList>();

		public HeaderLevelProvisionalPayments ProvisionalPaymentTypes => Factory.GetCachedValue<HeaderLevelProvisionalPayments>();

		#endregion
	}
}
