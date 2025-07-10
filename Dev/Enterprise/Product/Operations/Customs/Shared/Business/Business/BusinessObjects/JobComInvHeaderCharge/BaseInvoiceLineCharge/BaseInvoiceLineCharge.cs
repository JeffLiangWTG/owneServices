using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class BaseInvoiceLineCharge : CommonNonApportionedCharge
	{
		public BaseInvoiceLineCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		public new static readonly TypeDecider TypeDecider = new InvoiceLineChargeTypeDecider();

		#region Overrides

		[DecimalPlaces(2)]
		public override ZDecimal J7_Amount
		{
			get { return base.J7_Amount; }
			set { base.J7_Amount = value; }
		}

		[RelatedBusinessObject("InvoiceLine")]
		[BusinessObjectTestExclude]
		public override ZGuid J7_ParentID
		{
			get { return base.J7_ParentID; }
			set { base.J7_ParentID = value; }
		}

		[DecimalPlaces(3)]
		public override ZDecimal J7_Percentage
		{
			get { return base.J7_Percentage; }
			set
			{
				bool hasChanged = base.J7_Percentage != value;
				base.J7_Percentage = value;
				if (hasChanged && !IsCopying)
				{
					CalculateAmountBasedOnPercentageIfNecessary();
				}
			}
		}

		protected override bool GetIncludedInITOTReadOnly()
		{
			return base.GetIncludedInITOTReadOnly() || (IsIncludedInInvoiceAmountFixed && J7_IsNotIncludedInInvoice);
		}

		protected override ZBool GetNeedCheckChargeType()
		{
			return !(InvoiceLine?.Declaration?.IsInterface ?? ZBool.False);
		}

		public void CalculateAmountBasedOnPercentageIfNecessary()
		{
			BaseJobComInvoiceLine invoiceLine = InvoiceLine;

			if (invoiceLine != null)
			{
				invoiceLine.CalculateAmountBasedOnPercentageIfNecessary(this);
			}
		}

		public Money MoneyInInvoiceCurrency
		{
			get
			{
				Money result = Money.Empty;

				RefCurrency invoiceCurrency = null;
				CurrencyConverter currencyConverter = null;

				BaseJobComInvoiceLine invoiceLine = this.InvoiceLine;
				if (invoiceLine != null)
				{
					currencyConverter = ((ICommonInvoice)invoiceLine).CurrencyConverter;

					BaseJobComInvoiceHeader invoice = invoiceLine.InvoiceHeader;
					invoiceCurrency = invoice != null ? invoice.Invoice_Currency : null;
				}

				if (currencyConverter != null && invoiceCurrency != null)
				{
					result = currencyConverter.ConvertExact(Money, invoiceCurrency);
				}

				return result;
			}
		}

		#endregion

		#region Related Objects

		public BaseJobComInvoiceLine InvoiceLine
		{
			get { return (BaseJobComInvoiceLine)base.Parent; }
		}

		#endregion

		#region Implementation

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation()
		{
			return new InvoiceLineChargeValidation(this);
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion
	}
}
