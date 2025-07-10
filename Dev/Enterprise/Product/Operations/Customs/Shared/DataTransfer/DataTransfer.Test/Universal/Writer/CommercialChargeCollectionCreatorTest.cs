using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	partial class JobDeclarationDataObjectWriterTest
	{
		public void TestCommercialChargeCollectionCreation()
		{
			var declarationMock = CreateJobDeclarationMock();
			var declaration = declarationMock.Object;

			var invoice = declaration.Invoices.AddNew();
			BaseJobComInvHeaderCharge charge = invoice.GroupCharges.AddNew();
			var groupCharge1 = SetupJobComInvHeaderCharge(charge, ZBool.True, 150.45m, Common.CustomsChargeTypeList.Codes.OtherCharges, Core.Constants.CurrencyCodes.UnitedStates, ChargeDistributeByList.Codes.Value, Common.ChargeExchangeRateTypeList.Codes.FixedRate, 1.25m, ApportionmentTypeList.Codes.FullApportionment, ZBool.False, ZBool.True, ZBool.False, ZBool.True, 0.2m, Core.Constants.PaymentType.Collect, false, ZBool.True);
			charge = invoice.GroupCharges.AddNew();
			var groupCharge2 = SetupJobComInvHeaderCharge(charge, ZBool.False, 685.36m, Common.CustomsChargeTypeList.Codes.DeductionCharge, Core.Constants.CurrencyCodes.Australia, ChargeDistributeByList.Codes.Weight, ZString.Empty, 1m, ApportionmentTypeList.Codes.PartialApportionment, ZBool.True, ZBool.False, ZBool.True, ZBool.True, 0.8m, Core.Constants.PaymentType.Prepaid, false, ZBool.False);
			charge = invoice.Charges.AddNew();
			var charge1 = SetupJobComInvHeaderCharge(charge, ZBool.False, 685.36m, Common.CustomsChargeTypeList.Codes.DeductionCharge, Core.Constants.CurrencyCodes.Australia, ChargeDistributeByList.Codes.Weight, ZString.Empty, 1m, ApportionmentTypeList.Codes.PartialApportionment, ZBool.True, ZBool.False, ZBool.True, ZBool.True, 0.8m, Core.Constants.PaymentType.Prepaid, false, ZBool.True);
			charge = invoice.Charges.AddNew();
			var charge2 = SetupJobComInvHeaderCharge(charge, ZBool.True, 150.45m, Common.CustomsChargeTypeList.Codes.OtherCharges, Core.Constants.CurrencyCodes.UnitedStates, ChargeDistributeByList.Codes.Value, Common.ChargeExchangeRateTypeList.Codes.FixedRate, 1.25m, ApportionmentTypeList.Codes.FullApportionment, ZBool.False, ZBool.True, ZBool.False, ZBool.True, 0.2m, Core.Constants.PaymentType.Collect, false, ZBool.False);
			charge = invoice.Charges.AddNew();
			var charge_ToTestExchangeRatesOfAllTypesOutput = SetupJobComInvHeaderCharge(charge, ZBool.True, 150.45m, Common.CustomsChargeTypeList.Codes.OverseasFreight, Core.Constants.CurrencyCodes.UnitedStates, ChargeDistributeByList.Codes.Value, Common.ChargeExchangeRateTypeList.Codes.IATARate, 1.39m, ApportionmentTypeList.Codes.FullApportionment, ZBool.False, ZBool.True, ZBool.False, ZBool.True, 0.2m, Core.Constants.PaymentType.Collect, false, ZBool.False);

			declaration.ResumeApportionment();
			var collection = CommercialChargeCollectionCreator.CreateCollection(CurrentCompanyHelper, invoice.PK);
			AssertEquals(3, collection.Count);
			AssertContents2(collection.FirstOrDefault(x => x.ChargeType.GetCodeAsUpperCase() == Common.CustomsChargeTypeList.Codes.DeductionCharge && !x.IsApportionedCharge.GetValueOrDefault() && !x.IsIncludedInITOT.GetValueOrDefault()), ZBool.False, ZBool.False, ZBool.True);
			AssertContents(collection.FirstOrDefault(x => x.ChargeType.GetCodeAsUpperCase() == Common.CustomsChargeTypeList.Codes.OtherCharges && !x.IsApportionedCharge.GetValueOrDefault() && !x.IsIncludedInITOT.GetValueOrDefault()), ZBool.False, ZBool.False, ZBool.False);
			AssertContents_ExchangeRatesOfAllTypesOutput(collection.FirstOrDefault(x => x.ChargeType.GetCodeAsUpperCase() == Common.CustomsChargeTypeList.Codes.OverseasFreight && !x.IsApportionedCharge.GetValueOrDefault() && !x.IsIncludedInITOT.GetValueOrDefault()), ZBool.False, ZBool.False, ZBool.False);
		}

		BaseJobComInvHeaderCharge SetupJobComInvHeaderCharge(BaseJobComInvHeaderCharge charge, ZBool adjustedCharge, ZDecimal amount, ZString chargeType, ZString currency, ZString distributeBy, ZString exchangeRateType, ZDecimal exchangeRate, ZString fullOrPartialApportionment, ZBool isDutiable, ZBool isGSTApplicable, ZBool isIncludedInITOT, ZBool isNotIncludedInInvoice, ZDecimal percentage, ZString prepaidCollect, bool reapportion = true, ZBool? isStatisticalValueApplicable = null)
		{
			charge.J7_Percentage = percentage;
			charge.J7_AdjustedCharge = adjustedCharge;
			charge.J7_Amount = amount;
			charge.J7_ChargeType = chargeType;
			charge.J7_ChargeDescription = new ZString(charge.Lookups.ChargeTypeList.GetDescriptionFromCode(chargeType)).Left(35);
			charge.J7_RX_NKCurrency = currency;
			charge.J7_DistributeBy = distributeBy;
			charge.J7_FullOrPartialApportionment = fullOrPartialApportionment;
			charge.J7_IsDutiable = isDutiable;
			charge.J7_IsGSTApplicable = isGSTApplicable;
			charge.J7_IsIncludedInITOT = isIncludedInITOT;
			charge.J7_IsNotIncludedInInvoice = isNotIncludedInInvoice;
			if (isStatisticalValueApplicable.HasValue)
			{
				charge.J7_IsStatisticalValueApplicable = isStatisticalValueApplicable.Value;
			}
			if (reapportion)
			{
				charge.Parent.JobDeclaration.ResumeApportionment();
			}
			charge.J7_ExchangeRateType = exchangeRateType;
			charge.J7_ExchangeRate = exchangeRate;
			charge.J7_PrepaidCollect = prepaidCollect;
			return charge;
		}

		void AssertContents(UniversalCustoms.CommercialCharge chargeData, ZBool isApportionedCharge, ZBool isIncludedInITOT, ZBool? isStatisticalValueApplicable = null)
		{
			AssertContents(chargeData, ZBool.True, 150.45m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OtherCharges, Common.CustomsChargeTypeList.Descriptions.OtherCharges, 35), GetCodeDescriptionPair(Core.Constants.CurrencyCodes.UnitedStates, "United States Dollar"), GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(Common.ChargeExchangeRateTypeList.Codes.FixedRate, Common.ChargeExchangeRateTypeList.Descriptions.FixedRate), 1.25m, GetCodeDescriptionPair(ApportionmentTypeList.Codes.FullApportionment, ApportionmentTypeList.Descriptions.FullApportionment), isApportionedCharge, ZBool.False, ZBool.True, isIncludedInITOT, ZBool.True, 0.2m, GetCodeDescriptionPair(Core.Constants.PaymentType.Collect, "Collect"), isStatisticalValueApplicable);
		}

		void AssertContents2(UniversalCustoms.CommercialCharge chargeData, ZBool isApportionedCharge, ZBool isIncludedInITOT, ZBool? isStatisticalValueApplicable = null)
		{
			AssertContents(chargeData, ZBool.False, 685.36m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.DeductionCharge, Common.CustomsChargeTypeList.Descriptions.DeductionCharge, 35), GetCodeDescriptionPair(Core.Constants.CurrencyCodes.Australia, "Australian Dollar"), GetCodeDescriptionPair(ChargeDistributeByList.Codes.Weight, ChargeDistributeByList.Descriptions.Weight), GetCodeDescriptionPair(ZString.Empty, null), 2.04d, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), isApportionedCharge, ZBool.True, ZBool.False, isIncludedInITOT, ZBool.True, 0.8m, GetCodeDescriptionPair(Core.Constants.PaymentType.Prepaid, "Prepaid"), isStatisticalValueApplicable);
		}

		void AssertContents_ExchangeRatesOfAllTypesOutput(UniversalCustoms.CommercialCharge chargeData, ZBool isApportionedCharge, ZBool isIncludedInITOT, ZBool? isStatisticalValueApplicable = null)
		{
			AssertContents(chargeData, ZBool.True, 150.45m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OverseasFreight, Common.CustomsChargeTypeList.Descriptions.OverseasFreight, 35), GetCodeDescriptionPair(Core.Constants.CurrencyCodes.UnitedStates, "United States Dollar"), GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(Common.ChargeExchangeRateTypeList.Codes.IATARate, null), 1.39m, GetCodeDescriptionPair(ApportionmentTypeList.Codes.FullApportionment, ApportionmentTypeList.Descriptions.FullApportionment), isApportionedCharge, ZBool.False, ZBool.True, isIncludedInITOT, ZBool.True, 0.2m, GetCodeDescriptionPair(Core.Constants.PaymentType.Collect, "Collect"), isStatisticalValueApplicable);
		}

		void AssertContents(UniversalCustoms.CommercialCharge chargeData, ZBool adjustedCharge, ZDecimal amount, ICodeDescription chargeType, ICodeDescription currency, ICodeDescription distributeBy, ICodeDescription exchangeRateType, ZDecimal? agreedExchangeRate, ICodeDescription apportionmentType, ZBool isApportionedCharge, ZBool isDutiable, ZBool isGSTApplicable, ZBool isIncludedInITOT, ZBool isNotIncludedInInvoice, ZDecimal percentage, ICodeDescription prepaidCollect, ZBool? isStatisticalValueApplicable = null)
		{
			AssertNotNull("Precondition: charge", chargeData);

			CombineAssertions(delegate
			{
				AssertEquals("chargeData.adjustedCharge", adjustedCharge, chargeData.AdjustedCharge);
				AssertEquals("chargeData.amount", amount, chargeData.Amount);
				AssertNotNull("chargeData.ChargeType", chargeData.ChargeType);
				AssertEquals("chargeData.ChargeType.Code", chargeType.Code, chargeData.ChargeType.Code);
				AssertEquals("chargeData.ChargeType.Description", chargeType.Description, chargeData.ChargeType.Description);
				AssertNotNull("chargeData.Currency", chargeData.Currency);
				AssertEquals("chargeData.currency.Code", currency.Code, chargeData.Currency.Code);
				AssertEquals("chargeData.currency.Description", currency.Description, chargeData.Currency.Description);
				AssertNotNull("chargeData.DistributeBy", chargeData.DistributeBy);
				AssertEquals("chargeData.DistributeBy.Code", distributeBy.Code, chargeData.DistributeBy.Code);
				AssertEquals("chargeData.DistributeBy.Description", distributeBy.Description, chargeData.DistributeBy.Description);
				AssertNotNull("chargeData.ExchangeRateType", chargeData.ExchangeRateType);
				AssertEquals("chargeData.ExchangeRateType.Code", exchangeRateType.Code, chargeData.ExchangeRateType.Code);
				AssertEquals("chargeData.ExchangeRateType.Description", exchangeRateType.Description, chargeData.ExchangeRateType.Description);
				AssertEquals("chargeData.AgreedExchangeRate", agreedExchangeRate, chargeData.AgreedExchangeRate);
				AssertNotNull("chargeData.ApportionmentType", chargeData.ApportionmentType);
				AssertEquals("chargeData.ApportionmentType.Code", apportionmentType.Code, chargeData.ApportionmentType.Code);
				AssertEquals("chargeData.ApportionmentType.Description", apportionmentType.Description, chargeData.ApportionmentType.Description);
				AssertEquals("chargeData.isApportionedCharge", isApportionedCharge, chargeData.IsApportionedCharge);
				AssertEquals("chargeData.isDutiable", isDutiable, chargeData.IsDutiable);
				AssertEquals("chargeData.isGSTApplicable", isGSTApplicable, chargeData.IsGSTApplicable);
				AssertEquals("chargeData.isIncludedInITOT", isIncludedInITOT, chargeData.IsIncludedInITOT);
				AssertEquals("chargeData.isNotIncludedInInvoice", isNotIncludedInInvoice, chargeData.IsNotIncludedInInvoice);
				AssertEquals("chargeData.PercentageOfLinePrice", percentage, chargeData.PercentageOfLinePrice);
				AssertNotNull("chargeData.PrepaidCollect", chargeData.PrepaidCollect);
				AssertEquals("chargeData.PrepaidCollect.Code", prepaidCollect.Code, chargeData.PrepaidCollect.Code);
				AssertEquals("chargeData.PrepaidCollect.Description", prepaidCollect.Description, chargeData.PrepaidCollect.Description);
				if (isStatisticalValueApplicable.HasValue)
				{
					AssertEquals("chargeData.IsStatisticalValueApplicable", isStatisticalValueApplicable.Value, chargeData.IsStatisticalValueApplicable);
				}
			});
		}
	}
}
