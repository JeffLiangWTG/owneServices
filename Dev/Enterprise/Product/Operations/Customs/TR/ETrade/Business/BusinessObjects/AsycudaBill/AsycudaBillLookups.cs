using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Internal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class AsycudaBillLookups : ASYCUDA.Business.AsycudaBillLookups
	{
		public AsycudaBillLookups(AsycudaBill parent)
			: base(parent)
		{
		}

		AsycudaBill Bill => (AsycudaBill)this.Parent;
		AsycudaManifestHeader Header => header ?? (header = Bill.Header);
		AsycudaManifestHeader header;

		public CodeDescriptionPairList SpecialCargoCodes => Factory.GetCachedValue<SpecialCargoCodes>();

		public CodeDescriptionPairList NatureOfBusinessList => GetCachedRefCusCodeList(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NatureOfBusiness);

		public CodeDescriptionPairList ExemptionCodeList => Factory.GetCachedValue("TR.ETrade.ExemptionList-" + Bill.ExportCountry + Header.DateAtCustomsOffice, GetExemptionCodeList);
		public override CodeDescriptionPairList CustomsStatusList => Factory.GetCachedValue<BillStatusList>();
		protected override CodeDescriptionPairList CargoStatusListCore => Factory.GetCachedValue<TRETradeCargoStatusList>();
		public override CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<TRMessageStatusCodeList>();
		CodeDescriptionPairList GetExemptionCodeList()
		{
			var result = new CodeDescriptionPairList();
			if (!Header.DateAtCustomsOffice.IsEmpty)
			{
				var tariffQuery = new ZDBOnlyQuery(typeof(TariffView));
				tariffQuery.AddToFilter(RefCusTariffSchema.ZZ1_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.Turkey);
				tariffQuery.AddToFilter(RefCusTariffSchema.ZZ1_StartDate, SQLComparisonOperator.LessThanOrEqualTo, Header.DateAtCustomsOffice);
				tariffQuery.AddToFilter(RefCusTariffSchema.ZZ1_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, Header.DateAtCustomsOffice);

				var tariffType = new ZDBOnlySubQuery(typeof(RefCusTariffType), RefCusTariffTypeSchema.PK);
				tariffType.AddToFilter(RefCusTariffTypeSchema.ZZI_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.Turkey);
				tariffType.AddToFilter(RefCusTariffTypeSchema.ZZI_TariffType, TaxCodeList.RelatedMiscCodes.ExemptionType);
				tariffQuery.AddSubQuery(RefCusTariffSchema.ZZ1_ZZI_TariffType, RefCusTariffTypeSchema.PK, tariffType, JoinCondition.And);

				if (!Bill.ExportCountry.IsEmpty)
				{
					var rateCodeQuery = new ZDBOnlySubQuery(typeof(CusRefRateCodeView), CusRefRateCodeViewSchema.PK);
					rateCodeQuery.AddToFilter(CusRefRateCodeViewSchema.ZY1_RateType, TaxCodeList.RelatedMiscCodes.ExemptionRateType);
					rateCodeQuery.AddToFilter(CusRefRateCodeViewSchema.ZY1_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.Turkey);

					var tradeGroupCountry = new ZDBOnlySubQuery(typeof(CusRefTradeGroupCountryView), CusRefTradeGroupCountryViewSchema.PK);
					tradeGroupCountry.AddToFilter(CusRefTradeGroupCountryViewSchema.ZZB_RN_NKTradeGroupCountryCode, Bill.ExportCountry);
					var tradeGroup = new ZDBOnlySubQuery(typeof(CusRefTradeGroupView), CusRefTradeGroupViewSchema.PK);
					tradeGroup.AddToFilter(CusRefTradeGroupViewSchema.ZZA_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.Turkey);
					tradeGroup.AddToFilter_PossiblyCommaSeparated(CusRefTradeGroupViewSchema.ZZA_TradeGroup, new string[] { (NoResString)"All Countries", "EU", "Non-EU" });// Query is done with this value.
					tradeGroup.AddSubQuery(CusRefTradeGroupViewSchema.PK, CusRefTradeGroupCountryViewSchema.ZZB_ZZA_TradeGroup, tradeGroupCountry, JoinCondition.And);

					var applicability = new ZDBOnlySubQuery(typeof(CusRefApplicabilityView), RefCusApplicabilitySchema.PK);
					applicability.AddSubQuery(RefCusApplicabilitySchema.ZZT_ZZA_TradeGroup, CusRefTradeGroupViewSchema.PK, tradeGroup, JoinCondition.And);

					var rate = new ZDBOnlySubQuery(typeof(RefCusRate), RefCusRateSchema.ZZ2_ZZ1_Tariff);
					rate.AddSubQuery(RefCusRateSchema.PK, RefCusApplicabilitySchema.ZZT_ZZ2_Rate, applicability, JoinCondition.And);
					rate.AddSubQuery(RefCusRateSchema.ZZ2_ZY1_RateCode, CusRefRateCodeViewSchema.PK, rateCodeQuery, JoinCondition.And);
					tariffQuery.AddSubQuery(RefCusTariffSchema.PK, RefCusRateSchema.ZZ2_ZZ1_Tariff, rate, JoinCondition.And);
				}
				var list = Factory.Load<TariffView>(tariffQuery);

				foreach (var item in list)
				{
					if (!result.ContainsCode(item.ZZ1_TariffCode))
					{
						result.AddPair(item.ZZ1_TariffCode, item.ZZ1_Description);
					}
				}
			}
			return result;
		}

		CodeDescriptionPairList GetCachedRefCusCodeList(ZString codeType)
		{
			return AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Turkey, codeType);
		}

		public CodeDescriptionPairList BondTypeList => Factory.GetCachedValue<GuaranteeTypeCodeList>();

		public RefCountryCollection CountryList
		{
			get { return new RefCountryCollection(Factory); }
		}

		public CodeDescriptionPairList PaymentMethodList => Factory.GetCachedValue("TR.ETradeAsycudaBillLookups.PaymentMethodList", () => new PaymentMethodList());
		protected override CodeDescriptionPairList PackageTypeListCore => GetCachedRefCusCodeList(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes);

		public override ICodeDescriptionPairList Procedures => ((AsycudaManifestHeader)Parent?.Header)?.Lookups?.Procedures ?? new CodeDescriptionPairList();

		public override IAsycudaContainerCollection<ManifestBase.AsycudaContainer, ManifestBase.AsycudaManifestHeader> Containers
		{
			get
			{
				var query = new ZQuery();
				var containersPK = Header.Containers.Cast<AsycudaContainer>().Where(x => x.ContainerLevel == AsycudaBill.BillContainerCode).Select(x => x.PK);
				query.AddToFilter(AsycudaContainerSchema.PK, containersPK);
				query.OrderBy = AsycudaContainerSchema.ACN_ContainerNumber.Name;

				var list = new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>(Header, query);
				list.Load();
				return list;
			}
		}
	}
}

