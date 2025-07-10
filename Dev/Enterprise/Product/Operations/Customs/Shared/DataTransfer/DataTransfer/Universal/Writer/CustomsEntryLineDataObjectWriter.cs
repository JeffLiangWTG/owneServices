using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class CustomsEntryLineDataObjectWriter : DataObjectWriter<CusEntryLine, UniversalCustoms.EntryLine>
	{
		public CustomsEntryLineDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper)
			: base(manager)
		{
			this.helper = Argument.NotNull(helper, "UniversalDataObjectWriterHelper helper");
		}

		protected readonly UniversalDataObjectWriterHelper helper;

		protected sealed override UniversalCustoms.EntryLine PopulateDataObject(CusEntryLine entryLineBO)
		{
			var entryLineData = new UniversalCustoms.EntryLine()
			{
				LineNumber = entryLineBO.CL_LineNumber,
				CustomsStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(entryLineBO.CL_CustomsPostedStatus, entryLineBO.Lookups.EntryLineStatusList),
				HarmonisedCode = entryLineBO.CL_AdValoremTariff,
				CustomsValue = entryLineBO.CL_CustomsValue,
				//CL_WarehouseUnitValue
				DutyRatePercent = entryLineBO.CL_DutyPercent,
				DutyRateFlatAmount = entryLineBO.CL_FlatAmount,
				DutyRateFlatAmountUnit = ListHelper.GetWithDescription<CodeDescriptionPair>(entryLineBO.CL_FlatAmountUQ, entryLineBO.Lookups.FlatAmountUQList),
				Description = entryLineBO.CL_Description,
				//CL_ParentTrailer
				//CL_ExtraInfoForClassification
				AddInfoCollection = GetEntryLineAddInfoCollection(entryLineBO),
				CustomsReferenceCollection = CustomsReferenceCollectionCreator.CreateCollection(helper, entryLineBO, writeManager),
			};

			PopulateCusEntryLineChargeData(entryLineBO, entryLineData);
			PopulateCountrySpecificCusEntryLineChargeData(entryLineBO, entryLineData);
			PopulateCusEntrySupportingInfoData(entryLineBO, entryLineData);

			return entryLineData;
		}

		protected virtual List<CusEntryLineFee> GetEntryLineFeeBOs(CusEntryLine entryLineBO)
		{
			return entryLineBO.Header.HasAnyConfirmedFeesOnAnyMergedLine
				? entryLineBO.ConfirmedFees.Cast<CusEntryLineFee>().ToList()
				: entryLineBO.Fees.Cast<CusEntryLineFee>().ToList();
		}

		void PopulateCusEntryLineChargeData(CusEntryLine entryLineBO, UniversalCustoms.EntryLine entryLineData)
		{
			var entryLineFeeBOs = GetEntryLineFeeBOs(entryLineBO);

			if (entryLineFeeBOs.Count > 0)
			{
				var entryLineChargeCollection = new List<UniversalCustoms.EntryLineCharge>(entryLineFeeBOs.Count);
				var entryChargeTypeList = entryLineBO.Header.EntryChargeTypeList;
				foreach (CusEntryLineFee feeBO in entryLineFeeBOs)
				{
					entryLineChargeCollection.Add(new UniversalCustoms.EntryLineCharge()
					{
						Amount = feeBO.CF_ChargeAmount,
						Type = ListHelper.GetWithDescription<CodeDescriptionPair>(feeBO.CF_ChargeType, entryChargeTypeList),
						IsLandedCostOnly = feeBO.CF_IsLandedCostOnly,
						BaseValue = feeBO.CF_BaseValue,
						Rate = feeBO.CF_Rate,
						Source = feeBO.CF_Source,
						RateOverrideReason = ListHelper.GetWithDescription<CodeDescriptionPair>(feeBO.CF_RateOverrideReasonCode, feeBO.Lookups.RateOverrideReasonList),
						MethodOfPayment = ListHelper.GetWithDescription<CodeDescriptionPair>(feeBO.CF_MethodOfPayment, feeBO.Lookups.MethodOfPaymentList),
						MethodOfCalculation = ListHelper.GetWithDescription<CodeDescriptionPair6Char>(feeBO.CF_MethodOfCalculation, feeBO.Lookups.MethodOfCalculationList)
					});
				}
				entryLineData.EntryLineChargeCollection = entryLineChargeCollection;
			}
		}

		protected virtual void PopulateCountrySpecificCusEntryLineChargeData(CusEntryLine entryLineBO, UniversalCustoms.EntryLine entryLineData)
		{
		}

		protected virtual void PopulateCusEntrySupportingInfoData(CusEntryLine entryLineBO, UniversalCustoms.EntryLine entryLineData)
		{
		}

		protected virtual List<AddInfo> GetEntryLineAddInfoCollection(CusEntryLine entryLineBO)
		{
			return AddInfoCollectionCreator.CreateCollection(entryLineBO, CusEntryLineSchema.CL_AddInfo);
		}
	}
}
