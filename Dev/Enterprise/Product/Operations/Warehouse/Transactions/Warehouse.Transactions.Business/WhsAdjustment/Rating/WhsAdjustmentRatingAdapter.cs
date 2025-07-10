using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Rateable;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business
{
	class WhsAdjustmentRatingAdapter : WhsDocketRatingAdapter<WhsAdjustment>, IAutoRatingFreightInfo, IAutoRatingChargeApplicabilityDecider
	{
		public WhsAdjustmentRatingAdapter(WhsAdjustment docket)
			: base(docket)
		{
		}

		public override AdapterType AdapterType => AdapterType.WarehouseAdjustment;

		protected override IEnumerable<string> UsedChargeCodeGroups
		{
			get { return new[] { ChargeCodeGroupList.Codes.WHSInwards, ChargeCodeGroupList.Codes.WHSOutwards }; }
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.WarehouseStorage; }
		}

		protected override ZPropertyInfo GetTransportCoOrganisationPropertyInfo() => null;

		public override IRateableMeasureSet RateableMeasures
		{
			get
			{
				var result = new RateableMeasureSet(AdapterType);

				// For adjustment only storage measures are calculated. Otherwise we would have to handle negative quantities.
				var data = new ClosureData(Parent, (errorMessage, measureType) => result.AddError(measureType, errorMessage));

				if (data.IsSplitMonthBilling)
				{
					SetSplitMonthBillingLineMeasures(result, data);
				}
				else
				{
					CreateDocketLineMeasure(result, data, useNormalMeasures: false, useStorageMeasures: true);
				}

				AddTimeToMeasures(result);

				return result;
			}
		}

		#region SetSplitMonthBillingLineMeasures

		void SetSplitMonthBillingLineMeasures(RateableMeasureSet rateableMeasures, ClosureData closureData)
		{
			Action<RateableMeasureSet> lazyPopulateSplitMonthBilling = measures =>
			{
				if (Parent.IsFinalised)
				{
					foreach (var productTotal in closureData.Lines.GroupBy(l => l.ProductPK))
					{
						if (closureData.Products.TryGetValue(productTotal.Key, out var product) && product != null)
						{
							var quantity = productTotal.Sum(l => l.Quantity);

							if (quantity != 0)
							{
								var isAdjustmentIn = quantity > 0;
								if (isAdjustmentIn || !closureData.ShouldChargeStorageInAdvance)
								{
									if (!isAdjustmentIn) // AdjustmentOut
									{
										quantity = GetOutwardsStorageQuantityForProduct(closureData.QuantitiesByProduct, product.Parent, Math.Abs(quantity));
									}

									if (quantity != 0)
									{
										var weightInKG = GetWeightForRating(product, quantity);
										var volumeInM3 = GetVolumeForRating(product, quantity);
										var units = GetUnitsForRating(product, quantity);
										measures.AddWarehouseDocketStorageLineAdjustment(weightInKG, volumeInM3, units,
											warehousePK: Parent.WD_WW_Whs,
											productPK: product.Parent.PK,
											commodityCode: product.Parent.OP_RH_NKCommodityCode,
											docketReference: closureData.DocketReference,
											chargeGroupToUse: isAdjustmentIn ? ChargeCodeGroupList.Codes.WHSInwards : ChargeCodeGroupList.Codes.WHSOutwards);
									}
								}
							}
						}
					}
				}
			};

			rateableMeasures.CreateWarehouseDocketLines(lazyPopulateSplitMonthBilling,
				useNormalMeasures: false,
				useStorageMeasures: true,
				RateableMeasureSet.WarehouseProductOptionalAttributes.ChargeGroupToUse);
		}

		#endregion

		#region IAutoRatingChargeApplicabilityDecider Members

		bool IAutoRatingChargeApplicabilityDecider.ShouldRemoveCharge(AccChargeCode chargeCode)
		{
			var result = true;
			if (Parent.WD_DocketSubType != AdjustmentType.Codes.InternalWarehouseAdjustment)
			{
				// All charges except split month billing storage in/out charges must be removed
				var companyData = Parent.Client.CompanyData;
				var isSplitBillingStorageCharge = companyData.OB_ARWhsStorageCalcMethod == OrgCompanyDataLookups.WarehouseSplitPeriodBilling &&
													(chargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.WHSOutwards || chargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.WHSInwards) &&
													chargeCode.AC_ChargeSubGroup == ChargeCodeSubGroupList.Storage;
				result = !isSplitBillingStorageCharge;
			}
			return result;
		}

		#endregion
	}
}
