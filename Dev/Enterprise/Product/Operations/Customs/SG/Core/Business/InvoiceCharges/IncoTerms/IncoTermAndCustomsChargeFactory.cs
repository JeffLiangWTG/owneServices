using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.ITOTIncoTerm;

namespace Enterprise.Customs.SG.V4.Business
{
	public partial class IncoTermAndCustomsChargeFactory : Common.IncoTermAndCustomsChargeFactory
	{
		protected override void SetupIncotermChargeConfigurations()
		{
			SetupCFRConfiguration();
			SetupCIFConfiguration();
			SetupCNIConfiguration();
			SetupEXWConfiguration();
			SetupFASConfiguration();
			SetupFOBConfiguration();
		}

		void SetupFOBConfiguration()
		{
			AddChargeConfiguration(UnitPriceTermTypeCodeList.Codes.FOB, OverseasFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(UnitPriceTermTypeCodeList.Codes.FOB, OverseasInsurance, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(UnitPriceTermTypeCodeList.Codes.FOB, Other, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(UnitPriceTermTypeCodeList.Codes.FOB, OptionalItem, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		void SetupFASConfiguration()
		{
			AddChargeConfiguration(UnitPriceTermTypeCodeList.Codes.FAS, OverseasFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(UnitPriceTermTypeCodeList.Codes.FAS, OverseasInsurance, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(UnitPriceTermTypeCodeList.Codes.FAS, Other, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(UnitPriceTermTypeCodeList.Codes.FAS, OptionalItem, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		void SetupEXWConfiguration()
		{
			AddChargeConfiguration(UnitPriceTermTypeCodeList.Codes.EXW, OverseasFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(UnitPriceTermTypeCodeList.Codes.EXW, OverseasInsurance, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(UnitPriceTermTypeCodeList.Codes.EXW, Other, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(UnitPriceTermTypeCodeList.Codes.EXW, OptionalItem, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		void SetupCNIConfiguration()
		{
			AddChargeConfiguration(UnitPriceTermTypeCodeList.Codes.CNI, OverseasFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(UnitPriceTermTypeCodeList.Codes.CNI, OverseasInsurance, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(UnitPriceTermTypeCodeList.Codes.CNI, Other, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(UnitPriceTermTypeCodeList.Codes.CNI, OptionalItem, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		void SetupCIFConfiguration()
		{
			AddChargeConfiguration(UnitPriceTermTypeCodeList.Codes.CIF, OverseasFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(UnitPriceTermTypeCodeList.Codes.CIF, OverseasInsurance, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(UnitPriceTermTypeCodeList.Codes.CIF, Other, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(UnitPriceTermTypeCodeList.Codes.CIF, OptionalItem, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		void SetupCFRConfiguration()
		{
			AddChargeConfiguration(UnitPriceTermTypeCodeList.Codes.CFR, OverseasFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(UnitPriceTermTypeCodeList.Codes.CFR, OverseasInsurance, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(UnitPriceTermTypeCodeList.Codes.CFR, OptionalItem, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(UnitPriceTermTypeCodeList.Codes.CFR, Other, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupErrorConfiguration()
		{
			AddChargeConfiguration(ErrorIncoTermCode, OverseasFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(ErrorIncoTermCode, OverseasInsurance, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(ErrorIncoTermCode, OptionalItem, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(ErrorIncoTermCode, Other, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override IEnumerable<IITOTIncoTermCalculator> GetIITOTIncoTermCalculators()
		{
			yield return new CFRITOTIncoTermCalculator();
			yield return new CIFITOTIncoTermCalculator();
			yield return new CNIITOTIncoTermCalculator();
			yield return new FASITOTIncoTermCalculator();
			yield return new FOBITOTIncoTermCalculator();
		}

		public override bool IsThisChargeRecommendedForThisInvoice(ICommonInvoice invoice, ZString incoterm, ZString chargeCode)
		{
			bool result;
			if (incoterm == UnitPriceTermTypeCodeList.Codes.CIF && (chargeCode == CustomsChargeTypeList.Codes.OverseasInsurance || chargeCode == CustomsChargeTypeList.Codes.OverseasFreight))
			{
				var invoiceHeader = (JobComInvoiceHeader)invoice;
				result = !invoiceHeader.JobDeclaration?.IsImportOnly ?? true;
			}
			else
			{
				result = base.IsThisChargeRecommendedForThisInvoice(invoice, incoterm, chargeCode);
			}
			return result;
		}
	}
}
