using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public abstract class TRBaseIncoTermAndCustomsChargeFactory : CommonIncoTermAndCustomsChargeFactory
	{
		protected override ICustomsChargeCode[] GetCharges()
		{
			return new ICustomsChargeCode[]
				{
					ChargeProvider.InternationalFreight,
					ChargeProvider.InternationalInsurance,
					ChargeProvider.Commission,
					ChargeProvider.Demurrage,
					ChargeProvider.Royalty,
					ChargeProvider.Interest,
					ChargeProvider.Other,
					ChargeProvider.Observation,
					ChargeProvider.Surveillance,

					ChargeProvider.LocalBankCharge,
					ChargeProvider.LocalCultureCharge,
					ChargeProvider.LocalEnvironmentCharge,
					ChargeProvider.LocalResourceUtilizationSupportFund,
					ChargeProvider.LocalStorageCharge,
					ChargeProvider.LocalDischargeCharge,
					ChargeProvider.LocalPortCharge,
					ChargeProvider.LocalOther,
					ChargeProvider.TotalForeignCharges,
					ChargeProvider.LocalTotalCharge
				};
		}

		protected override void SetupIncotermChargeConfigurations()
		{
			SetupDeliveredAtPlaceUnloadedConfiguration();
			SetupCostAndFreightConfiguration();
			SetupCostInsuranceAndFreightConfiguration();
			SetupCarriageAndInsurancePaidToConfiguration();
			SetupCarriagePaidToConfiguration();
			SetupDeliveredAtPlaceConfiguration();
			SetupDeliveredDutyPaidConfiguration();
			SetupFreeOnBoardConfiguration();
			SetupExWorksConfiguration();
			SetupFreeAlongsideShipConfiguration();
			SetupFreeCarrierConfiguration();
		}

		protected abstract CustomsChargeCode InternationalFreight { get; }

		protected abstract CustomsChargeCode InternationalInsurance { get; }

		#region Setup per INCOTerm

		protected override void SetupCostAndFreightConfiguration()
		{
			var incoTerm = Core.Constants.IncoTerms.CostAndFreight;
			SetUpCommon(incoTerm);

			AddChargeConfiguration(incoTerm, InternationalFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, InternationalInsurance, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = true, IsRecommended = true });
		}

		protected override void SetupCostInsuranceAndFreightConfiguration()
		{
			var incoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			SetUpCommon(incoTerm);

			AddChargeConfiguration(incoTerm, InternationalFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, InternationalInsurance, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
		}

		protected override void SetupCarriageAndInsurancePaidToConfiguration()
		{
			var incoTerm = Core.Constants.IncoTerms.CarriageAndInsurancePaidTo;
			SetUpCommon(incoTerm);

			AddChargeConfiguration(incoTerm, InternationalFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, InternationalInsurance, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
		}

		protected override void SetupCarriagePaidToConfiguration()
		{
			var incoTerm = Core.Constants.IncoTerms.CarriagePaidTo;
			SetUpCommon(incoTerm);

			AddChargeConfiguration(incoTerm, InternationalFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, InternationalInsurance, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = true, IsRecommended = true });
		}

		protected override void SetupDeliveredAtPlaceConfiguration()
		{
			var incoTerm = Core.Constants.IncoTerms.DeliveredAtPlace;
			SetUpCommon(incoTerm);

			AddChargeConfiguration(incoTerm, InternationalFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, InternationalInsurance, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = true, IsRecommended = true });
		}

		protected void SetupDeliveredAtPlaceUnloadedConfiguration()
		{
			var incoTerm = Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded;
			SetUpCommon(incoTerm);

			AddChargeConfiguration(incoTerm, InternationalFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, InternationalInsurance, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = true, IsRecommended = true });
		}

		protected override void SetupDeliveredDutyPaidConfiguration()
		{
			var incoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			SetUpCommon(incoTerm);

			AddChargeConfiguration(incoTerm, InternationalFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, InternationalInsurance, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = true, IsRecommended = true });
		}

		protected override void SetupExWorksConfiguration()
		{
			var incoTerm = Core.Constants.IncoTerms.ExWorks;
			SetUpCommon(incoTerm);

			AddChargeConfiguration(incoTerm, InternationalFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, InternationalInsurance, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = true, IsRecommended = true });
		}

		protected override void SetupFreeAlongsideShipConfiguration()
		{
			var incoTerm = Core.Constants.IncoTerms.FreeAlongsideShip;
			SetUpCommon(incoTerm);

			AddChargeConfiguration(incoTerm, InternationalFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, InternationalInsurance, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = true, IsRecommended = true });
		}

		protected override void SetupFreeCarrierConfiguration()
		{
			var incoTerm = Core.Constants.IncoTerms.FreeCarrier;
			SetUpCommon(incoTerm);

			AddChargeConfiguration(incoTerm, InternationalFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, InternationalInsurance, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = true, IsRecommended = true });
		}

		protected override void SetupFreeOnBoardConfiguration()
		{
			var incoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			SetUpCommon(incoTerm);

			AddChargeConfiguration(incoTerm, InternationalFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, InternationalInsurance, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = true, IsRecommended = true });
		}

		protected virtual void SetUpCommon(ZString incoTerm)
		{
			AddChargeConfiguration(incoTerm, ChargeProvider.Commission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, ChargeProvider.Demurrage, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, ChargeProvider.Royalty, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, ChargeProvider.Interest, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, ChargeProvider.Other, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, ChargeProvider.Observation, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, ChargeProvider.Surveillance, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, ChargeProvider.TotalForeignCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });

			AddChargeConfiguration(incoTerm, ChargeProvider.LocalBankCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, ChargeProvider.LocalStorageCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, ChargeProvider.LocalDischargeCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, ChargeProvider.LocalPortCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, ChargeProvider.LocalCultureCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, ChargeProvider.LocalResourceUtilizationSupportFund, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, ChargeProvider.LocalEnvironmentCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, ChargeProvider.LocalOther, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, ChargeProvider.LocalTotalCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
		}

		#endregion
	}
}
