using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.BIRD.Common;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public partial class ENS40 : Messaging.Business.MessageBuildingBlocks.Input.Abstract.ENS40, IBIRDLineIDRecord
	{
		#region IBIRDLineIDRecord Members

		ZInt IBIRDLineIDRecord.DelimiterInvSequence
		{
			get { return ZInt.ParseSafe(this.InvoiceDelimiter.KeepNumericCharacters(), 0); }
		}

		ZInt IBIRDLineIDRecord.LineNumber
		{
			get { return LineItemNumber; }
		}

		#endregion

		#region IBIRDLineRecord Members

		void IBIRDLineRecord.Update(JobComInvoiceLine invoiceLine, CargoWise.ComponentModel.INotifications notifications)
		{
			invoiceLine.US_UC_NKCountryOfOrigin = CountryOfOrigin;

			this.UpdateValue(invoiceLine, Value);

			var weight = GrossWeight;
			var weightUQ = weight > 0 ? Constants.Weight.Kilograms : string.Empty;
			if (weight > 999999.999m)
			{
				weight = Constants.Weight.Convert(weight, weightUQ, Constants.Weight.Tonnes);
				weightUQ = Constants.Weight.Tonnes;
			}

			invoiceLine.JI_Weight = weight;
			invoiceLine.JI_WeightUQ = weightUQ;

			invoiceLine.US_ADDDepositValue = ADDSpecificDepositValue;
			invoiceLine.US_CVDDepositValue = CVDSpecificDepositValue;

			Customs.Common.JobComInvCharge[] charges = invoiceLine.Charges.GetCharge(USCustomsChargeTypeList.Codes.OverseasFreight);

			InvoiceLineCharge charge = charges.Length > 0 ? (InvoiceLineCharge)charges[0] : null;
			if (charge == null && Charges > 0m)
			{
				charge = invoiceLine.Charges.AddNew();
				charge.J7_ChargeType = USCustomsChargeTypeList.Codes.OverseasFreight;
			}

			if (charge != null)
			{
				charge.J7_Amount = Charges;
				charge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			}

			if (!invoiceLine.IsSecondaryTariffLine)
			{
				invoiceLine.US_SchDLoading = PortOfLading;
			}

			invoiceLine.US_ZoneStatus = ZoneStatus;

			invoiceLine.US_PrivilegedStatusDate = PrivilegedStatusFilingDate;

			invoiceLine.US_IsNAFTANet = NAFTANetCostIndicator == "Y";
		}

		ZString IBIRDTariffRecord.Tariff
		{
			get { return ZString.Empty; }
		}

		#endregion
	}
}
