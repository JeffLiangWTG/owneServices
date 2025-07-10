using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.LVS.Business
{
	public static class CusUSLVItemValidationHelper
	{
		internal static void CheckProductCode(CusUSLVItem cusUSLVItem, ZPropertyInfo propertyInfo)
		{
			if (!propertyInfo.Value.IsEmpty && cusUSLVItem.PartSyncManager.Enabled)
			{
				InvoiceLineProductValidationHelper.ValidateProductCodeWhenPartSyncManagerEnabled(
					propertyInfo,
					cusUSLVItem.Product,
					cusUSLVItem.Consignment?.Seller?.Header,
					cusUSLVItem.Consignment?.Consignee?.Header,
					cusUSLVItem.PartSyncManager,
					WarningPartCodeFoundButNotRelatedToSellerConsigneeCombination,
					WarningPartCodesFoundButNotRelatedToSellerConsigneeCombination);
			}
		}

		internal static void CheckLineValue(CusUSLVItem cusUSLVItem, ZPropertyInfo propertyInfo)
		{
			MandatoryValidation.CheckNotNegative(propertyInfo);

			if (propertyInfo.Value.IsEmpty)
			{
				propertyInfo.AddMessageError(Res.GetString("E96B76D2-0F2A-44B1-9E0D-DAD90E499622", "Line price is mandatory for entry type '86'."));
			}
			else if (0m < cusUSLVItem.ULI_GoodsValueInUSD && cusUSLVItem.ULI_GoodsValueInUSD < 0.5m)
			{
				propertyInfo.AddWarning(Res.GetString("D9A767CB-F00B-41D5-B0FE-855F4EE8FA0E", "Line value will be rounded to $1 USD."));
			}
		}

		public static string WarningPartCodeFoundButNotRelatedToSellerConsigneeCombination
		{
			get => Res.GetString("2e1bc3ad-ee78-48b0-94a2-5e548ba092d3", "A Product with this code exists but is inactive or the Supplier (Exporter)/Importer (Owner) relationship on that Product does not match this consignment. Either add a new Product (F3), make it active (F4, add a filter to show inactive records, search and edit), or change the Supplier (Exporter)/Importer (Owner) relationship on the existing Product (F4 then edit a Product).");
		}

		public static string WarningPartCodesFoundButNotRelatedToSellerConsigneeCombination
		{
			get => Res.GetString("bf9f282d-511c-462f-a1d0-d294d7d36e55", "Several Products with this code exist but are inactive or the Supplier (Exporter)/Importer (Owner) relationship on that Product does not match this consignment. Either add a new Product (F3), make it active (F4, add a filter to show inactive records, search and edit), or change the Supplier (Exporter)/Importer (Owner) relationship on the existing Product (F4 then edit a Product).");
		}

		internal static void CheckTariff(CusUSLVItem cusUSLVItem, ZPropertyInfo propertyInfo)
		{
			if (propertyInfo.Value.IsEmpty)
			{
				propertyInfo.AddMessageError(Res.GetString("C8B02FC0-8A3D-4327-89C7-B4BAEC9AA596", "Tariff may not be empty."));
			}
			else
			{
				var tariff = new USCTariff.Loader(cusUSLVItem.Factory).LoadBestMatch(propertyInfo.Value.ToString(), ZDate.Today);
				if (tariff == null)
				{
					propertyInfo.AddMessageError(Res.GetString("7D2E8A18-4AEC-4D45-9164-30A9EFA49C05", "Tariff unable to be found."));
				}
				else
				{
					if (!tariff.TaxFeeCode.IsEmpty)
					{
						var taxFeeCodes = tariff.TaxFeeCode.Split(",");
						var ircCodes = ZString.Empty;
						foreach (var taxFeeCode in taxFeeCodes)
						{
							if (CusFeeCodeConstants.IsExciseTax(taxFeeCode))
							{
								ircCodes = ircCodes.IsEmpty ? taxFeeCode : ZString.Join(",", new[] { ircCodes, taxFeeCode });
							}
						}
						if (!ircCodes.IsEmpty)
						{
							propertyInfo.AddMessageError(Res.GetString("4B5D6F39-6C3B-4EDF-BDC3-FAA6886548CB", "Tariff has an IRC tax code of {0}.", ircCodes));
						}
					}

					if (tariff.UE_QuotaIndicator && cusUSLVItem.Consignment?.Shipment is CusUSLVClearance clearance)
					{
						var quotaWarning = Res.GetString("6DBAE365-D760-4959-A2BA-996BC34DC839", "According to CBP reference files, this tariff may be subject to quota.");
						var quota = new USCQuota.Loader(cusUSLVItem.Factory).LoadBestMatchFor(tariff.UE_Tariff, ZString.Empty, cusUSLVItem.ULI_RN_NKCountryOfOrigin, ZDateTime.Today, clearance.ULH_DepartureDate);
						if (quota != null)
						{
							quotaWarning += "\r\n" + Res.GetString("5AC84327-952B-4760-B487-D0BE0AFD910C", @"There is a query record which indicates that the quota for this tariff was updated at Customs on {0}, 
its quota limit was {1}, quantities to date was {2} and its status was '{3}'.
You can view more details in Main Menu > Maintain > Customs > Quotas.", quota.UT_LastUpdateDate.ToLongTimeString(), quota.UT_QuotaLimit + " " + quota.UT_QuotaLimitType, quota.UT_QtyToDate, quota.QuotaStatusDesc);
						}

						propertyInfo.AddWarning(quotaWarning);
					}
				}
			}

			cusUSLVItem.ItemPGAWrapperCollection.OfType<CusUSLVItemPGAWrapper>().ForEach(w => w.ValidateDisclaimReason());
		}

		internal static void CheckCountryOfOrigin(CusUSLVItem cusUSLVItem, ZPropertyInfo propertyInfo)
		{
			if (propertyInfo.Value.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
			}
			else
			{
				string errorTextForOrigin = ExternalValidation.GetErrorTextForCanadaCountryOfOrigin(cusUSLVItem.ULI_RN_NKCountryOfOrigin, Core.Constants.CountryCodes.Canada);
				if (!string.IsNullOrEmpty(errorTextForOrigin))
				{
					propertyInfo.AddMessageError(errorTextForOrigin);
				}
				if (!CanadaProvinceTerritoryCodes.IsCanadianProvince(cusUSLVItem.ULI_RN_NKCountryOfOrigin))
				{
					ListValidation.MessageErrorIfInvalidCode(propertyInfo, cusUSLVItem.Lookups.CountryOfOrigins);
				}
			}
		}

		internal static void CheckCurrency(CusUSLVItem cusUSLVItem, ZPropertyInfo propertyInfo)
		{
			if (propertyInfo.Value.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(propertyInfo, cusUSLVItem.Lookups.Currencies);
			}
		}

		internal static void CheckAntiDumping(CusUSLVItem cusUSLVItem, ZPropertyInfo propertyInfo, bool isAntiDumping)
		{
			if (!isAntiDumping)
			{
				new ADD_CVDLiabilityChecker().ValidateLiability(propertyInfo, cusUSLVItem, ADD_CVDLiabilityChecker.ADD_CVD.ADD, cusUSLVItem.Tariff);
			}
		}

		internal static void CheckCountervailing(CusUSLVItem cusUSLVItem, ZPropertyInfo propertyInfo, bool isCountervailing)
		{
			if (!isCountervailing)
			{
				new ADD_CVDLiabilityChecker().ValidateLiability(propertyInfo, cusUSLVItem, ADD_CVDLiabilityChecker.ADD_CVD.CVD, cusUSLVItem.Tariff);
			}
		}

		internal static void CheckGoodsDescription(CusUSLVItem cusUSLVItem, ZPropertyInfo propertyInfo)
		{
			if (cusUSLVItem.CusUSLVItemPGAs.Count > 0)
			{
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
			}
		}
	}
}
