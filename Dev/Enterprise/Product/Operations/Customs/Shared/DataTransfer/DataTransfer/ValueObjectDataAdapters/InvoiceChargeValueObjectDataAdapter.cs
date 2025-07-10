using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.DataTransfer.Integration;
using Enterprise.ZArchitecture.Core;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.DataTransfer
{
	public abstract class InvoiceChargeValueObjectDataAdapter<TBusinessObject, TValueObject> : CustomsBusinessObjectValueObjectDataAdapter<TBusinessObject, TValueObject>
		where TBusinessObject : BusinessObject
		where TValueObject : Xsd.InvoiceHeader
	{
		protected void ImportInvoiceChargesDetails(Xsd.InvoiceChargeCollection invoiceCharges, IJobComInvChargeCollection<JobComInvCharge> charges, IValueObjectImportContext context)
		{
			Xsd.XmlInterchange interchange = (Xsd.XmlInterchange)context.Interchange;
			INotifications notification = context;

			if (invoiceCharges != null)
			{
				foreach (Xsd.InvoiceCharge xmlInvoiceCharge in invoiceCharges)
				{
					JobComInvCharge invoiceCharge = null;
					foreach (JobComInvCharge existingInvoiceCharges in charges)
					{
						if (existingInvoiceCharges.J7_ChargeType == xmlInvoiceCharge.ChargeType)
						{
							invoiceCharge = existingInvoiceCharges;
							break;
						}
					}
					if (invoiceCharge == null)
					{
						invoiceCharge = charges.AddNew();
					}

					context.SetPropertyInfoValue(invoiceCharge.J7_ChargeTypeInfo, xmlInvoiceCharge.ChargeType, xmlInvoiceCharge.ChargeTypeSpecified);
					if (xmlInvoiceCharge.ChargeValue != null)
					{
						invoiceCharge.J7_Amount = xmlInvoiceCharge.ChargeValue.Value;
						context.SetPropertyInfoValue(invoiceCharge.J7_RX_NKCurrencyInfo, xmlInvoiceCharge.ChargeValue.CurrencyCode, xmlInvoiceCharge.ChargeValue.CurrencyCodeSpecified, (NoResString)"Invoice Charge Currency");
					}

					if (xmlInvoiceCharge.DutyAppliesSpecified)
					{
						invoiceCharge.J7_IsDutiable = (xmlInvoiceCharge.DutyApplies == Xsd.TrueFalse.@true);
					}

					if (xmlInvoiceCharge.GstAppliesSpecified)
					{
						invoiceCharge.J7_IsGSTApplicable = (xmlInvoiceCharge.GstApplies == Xsd.TrueFalse.@true);
					}

					if (xmlInvoiceCharge.IsIncludedInTotalSpecified)
					{
						invoiceCharge.J7_IsIncludedInITOT = (xmlInvoiceCharge.IsIncludedInTotal == Xsd.TrueFalse.@true);
					}

					if (xmlInvoiceCharge.IsIncludedInInvoiceSpecified)
					{
						invoiceCharge.J7_IsNotIncludedInInvoice = (xmlInvoiceCharge.IsIncludedInInvoice == Xsd.TrueFalse.@false);
					}
				}
			}
		}

		protected virtual Xsd.InvoiceChargeCollection ExportInvoiceCharges(IJobComInvChargeCollection<JobComInvCharge> charges)
		{
			Xsd.InvoiceChargeCollection xmlInvoiceCharges = new Xsd.InvoiceChargeCollection();
			foreach (JobComInvCharge charge in charges)
			{
				Xsd.InvoiceCharge newCharge = xmlInvoiceCharges.AddNew();
				newCharge.ChargeValue = Xsd.FinancialValue.FromAmountAndCurrency(charge.J7_Amount, charge.Currency);

				newCharge.ChargeType = charge.J7_ChargeType;
				newCharge.DutyApplies = charge.J7_IsDutiable ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
				newCharge.DutyAppliesSpecified = true;
				newCharge.GstApplies = charge.J7_IsGSTApplicable ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
				newCharge.GstAppliesSpecified = true;
				newCharge.IsIncludedInTotal = charge.J7_IsIncludedInITOT ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
				newCharge.IsIncludedInTotalSpecified = true;
				newCharge.IsIncludedInInvoice = charge.J7_IsNotIncludedInInvoice ? Xsd.TrueFalse.@false : Xsd.TrueFalse.@true;
				newCharge.IsIncludedInInvoiceSpecified = true;
			}

			return (xmlInvoiceCharges.Count > 0) ? xmlInvoiceCharges : null;
		}
	}
}
