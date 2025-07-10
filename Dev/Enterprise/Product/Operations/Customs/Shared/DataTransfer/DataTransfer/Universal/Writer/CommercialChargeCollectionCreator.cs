using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public static class CommercialChargeCollectionCreator
	{
		public static List<UniversalCustoms.CommercialCharge> CreateCollection(UniversalDataObjectWriterHelper helper, ZGuid parentPK)
		{
			var commercialInvoiceChargeCollection = new List<UniversalCustoms.CommercialCharge>();
			PopulateCommercialChargeData(helper, GetCharges(helper, parentPK, ZBool.True), commercialInvoiceChargeCollection);
			PopulateCommercialChargeData(helper, GetCharges(helper, parentPK, ZBool.False), commercialInvoiceChargeCollection);
			commercialInvoiceChargeCollection.Sort(SortByChargeType);
			return (commercialInvoiceChargeCollection.Count > 0) ? commercialInvoiceChargeCollection : null;
		}

		static BaseJobComInvHeaderCharge[] GetCharges(UniversalDataObjectWriterHelper helper, ZGuid parentPK, ZBool isApportionedCharge)
		{
			var query = new ZQuery(JobComInvHeaderChargeSchema.J7_ParentID, parentPK);
			query.AddToFilter(JobComInvHeaderChargeSchema.J7_IsApportionedCharge, isApportionedCharge);
			return helper.Load<BaseJobComInvHeaderCharge>(query);
		}

		static int SortByChargeType(UniversalCustoms.CommercialCharge x, UniversalCustoms.CommercialCharge y)
		{
			int result = 0;
			if (x == null && y != null)
			{
				result = -1;
			}
			else if (x != null && y == null)
			{
				result = 1;
			}
			else if (x.ChargeType == null && y.ChargeType != null)
			{
				result = -1;
			}
			else if (x.ChargeType != null && y.ChargeType == null)
			{
				result = 1;
			}
			else if (!x.ChargeType.Code.HasValue && y.ChargeType.Code.HasValue)
			{
				result = -1;
			}
			else if (x.ChargeType.Code.HasValue && !y.ChargeType.Code.HasValue)
			{
				result = 1;
			}
			else
			{
				result = x.ChargeType.Code.Value.CompareTo(y.ChargeType.Code.Value);
			}
			return result;
		}

		static void PopulateCommercialChargeData(UniversalDataObjectWriterHelper helper, BaseJobComInvHeaderCharge[] charges, List<UniversalCustoms.CommercialCharge> commercialInvoiceChargeCollection)
		{
			foreach (var chargeBO in charges)
			{
				var chargeType = chargeBO.J7_ChargeType;
				string description = chargeBO.J7_ChargeDescription;
				if (string.IsNullOrEmpty(description))
				{
					description = chargeBO.Lookups.ChargeTypeList.GetDescriptionFromCode(chargeType);
				}

				var chargeData = new UniversalCustoms.CommercialCharge()
				{
					ChargeType = new CodeDescriptionPair() { Code = chargeType, Description = description },
					ApportionmentType = ListHelper.GetWithDescription<CodeDescriptionPair>(chargeBO.J7_FullOrPartialApportionment, chargeBO.Lookups.ApportionmentTypeList),
					Amount = chargeBO.J7_Amount,
					Currency = ListHelper.GetWithDescription<Currency>(chargeBO.J7_RX_NKCurrency, chargeBO.Lookups.Currencies),
					IsDutiable = chargeBO.J7_IsDutiable,
					IsGSTApplicable = chargeBO.J7_IsGSTApplicable,
					IsIncludedInITOT = chargeBO.J7_IsIncludedInITOT,
					IsNotIncludedInInvoice = chargeBO.J7_IsNotIncludedInInvoice,
					IsApportionedCharge = chargeBO.J7_IsApportionedCharge,
					PrepaidCollect = ListHelper.GetWithDescription<CodeDescriptionPair>(chargeBO.J7_PrepaidCollect, chargeBO.Lookups.PrepaidCollectList),
					PercentageOfLinePrice = chargeBO.J7_Percentage,
					DistributeBy = ListHelper.GetWithDescription<CodeDescriptionPair>(chargeBO.J7_DistributeBy, chargeBO.Lookups.ChargeDistributionBy),
					AdjustedCharge = chargeBO.J7_AdjustedCharge,
					ExchangeRateType = ListHelper.GetWithDescription<CodeDescriptionPair>(chargeBO.J7_ExchangeRateType, chargeBO.Lookups.ExchangeRateTypeList),
					AgreedExchangeRate = chargeBO.J7_ExchangeRate,
					IsStatisticalValueApplicable = chargeBO.J7_IsStatisticalValueApplicable
				};

				commercialInvoiceChargeCollection.Add(chargeData);
			}
		}
	}
}
