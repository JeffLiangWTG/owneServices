using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.ACE;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
{
	public partial class AENS40 : Abstract.AENS40, IACEBIRDLineIDRecord
	{
		#region IACEBIRDLineIDRecord Members

		ZInt IACEBIRDLineIDRecord.LineNumber
		{
			get { return ZInt.ParseSafe(this.LineItemIdentifier.KeepNumericCharacters(), 0); }
		}

		#endregion

		#region IACEBIRDLineRecord

		void IACEBIRDLineRecord.Update(JobComInvoiceLine invoiceLine, INotifications notifications)
		{
			invoiceLine.US_SetInd = ArticleSetIndicator;
			invoiceLine.US_UC_NKCountryOfOrigin = CountryOfOriginCode;
			invoiceLine.US_UC_NKCountryOfExport = CountryOfExportCode;
			invoiceLine.US_DateOfExport = DateOfExportation;
			invoiceLine.US_DateOfExportFromCountryOfOrigin = DateOfExportationforTextiles;
			invoiceLine.US_SPI = TradeAgreementSpecialProgramClaimCode.IsEmpty ? SPICompleteList.MoreCodes.NotApplicable : (string)TradeAgreementSpecialProgramClaimCode;
			invoiceLine.US_SecondarySPI = ProductClaimCode;

			var charges = invoiceLine.Charges.GetCharge(USCustomsChargeTypeList.Codes.OverseasFreight);
			var charge = charges.Length > 0 ? charges[0] : null;
			if (charge == null && ChargesAmount > 0m)
			{
				charge = invoiceLine.Charges.AddNew();
				charge.J7_ChargeType = USCustomsChargeTypeList.Codes.OverseasFreight;
			}

			if (charge != null)
			{
				charge.J7_Amount = ChargesAmount;
				charge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			}

			if (!invoiceLine.IsSecondaryTariffLine)
			{
				invoiceLine.US_SchDLoading = ForeignPortOfLadingCode;
			}

			var weight = GrossShippingWeight;
			var weightUQ = weight > 0 ? Enterprise.Core.Constants.Weight.Kilograms : string.Empty;
			if (weight > 999999.999m)
			{
				weight = Enterprise.Core.Constants.Weight.Convert(weight, weightUQ, Enterprise.Core.Constants.Weight.Tonnes);
				weightUQ = Enterprise.Core.Constants.Weight.Tonnes;
			}

			invoiceLine.JI_Weight = weight;
			invoiceLine.JI_WeightUQ = weightUQ;
			invoiceLine.US_TextileCategoryNo = CategoryCodeforTextiles;
			invoiceLine.US_TransactionsRelated = RelatedPartyIndicator;
			invoiceLine.US_IsNAFTANet = NAFTANetCostIndicator == "Y";
			invoiceLine.US_ADCVDStat = ADCVDNonReimbursementStatement == "Y" ? ADDCVDNonReimbursementList.Codes.OnceOff : string.Empty;
		}

		#endregion
	}
}
