using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public enum TriState { True, False, NotDetermined }

	/// <summary>
	/// Charges that Invoice have
	/// </summary>
	public class BaseInvoiceCharge : CommonNonApportionedCharge
	{
		public BaseInvoiceCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new static readonly BaseInvoiceChargeTypeDecider TypeDecider = new BaseInvoiceChargeTypeDecider();

		#region Overrides

		[DecimalPlaces(2)]
		public override ZDecimal J7_Amount
		{
			get { return base.J7_Amount; }
			set
			{
				base.J7_Amount = value;

				if (J7_Calc_IsIncludedInInvoiceAmount)
				{
					RefreshBindingOnInvoiceCalculatedFields();
				}
			}
		}

		public override ZBool J7_Calc_IsIncludedInInvoiceAmount
		{
			get { return base.J7_Calc_IsIncludedInInvoiceAmount; }
			set
			{
				base.J7_Calc_IsIncludedInInvoiceAmount = value;

				RefreshBindingOnInvoiceCalculatedFields();
			}
		}

		public override ZBool J7_IsIncludedInITOT
		{
			get { return base.J7_IsIncludedInITOT; }
			set
			{
				base.J7_IsIncludedInITOT = value;

				RefreshBindingOnInvoiceCalculatedFields();
			}
		}

		public override ZString J7_ChargeType
		{
			get { return base.J7_ChargeType; }
			set
			{
				bool hasChanges = base.J7_ChargeType != value;
				base.J7_ChargeType = value;
				if (!IsCopying && hasChanges)
				{
					DefaultPrepaidCollect(Invoice);
				}
			}
		}

		public override ZDecimal J7_Percentage
		{
			get { return base.J7_Percentage; }
			set
			{
				base.J7_Amount = 0m;
				base.J7_RX_NKCurrency = ZString.Empty;
				base.J7_Percentage = value;
			}
		}

		public override ZBool J7_AdjustedCharge
		{
			get { return base.J7_AdjustedCharge; }
			set
			{
				base.J7_AdjustedCharge = value;
				if (Invoice != null)
				{
					Invoice.MarkAsNeedingValidation();

					RefreshBindingOnInvoiceCalculatedFields();
				}
			}
		}

		void RefreshBindingOnInvoiceCalculatedFields()
		{
			if (Invoice != null)
			{
				Invoice.JZ_Calc_BalanceInfo.RefreshBinding();

				Invoice.JZ_Calc_ChargesExcludedFromITOTInfo.RefreshBinding();

				Invoice.JZ_Calc_LinesEnteredInfo.RefreshBinding();
			}
		}

		[RelatedBusinessObject("Invoice")]
		[BusinessObjectTestExclude]
		public override ZGuid J7_ParentID
		{
			get { return base.J7_ParentID; }
			set { base.J7_ParentID = value; }
		}

		protected override bool GetIncludedInITOTReadOnly()
		{
			return base.GetIncludedInITOTReadOnly() || (IsIncludedInInvoiceAmountFixed && J7_IsNotIncludedInInvoice);
		}

		protected override void MarkAsNeedingValidationCore()
		{
			base.MarkAsNeedingValidationCore();
			if (Invoice != null)
			{
				Invoice.MarkAsNeedingValidation();
				MarkParentGroupHeadersAsNeedingValidation();
			}
		}

		protected override ZBool GetNeedCheckChargeType()
		{
			return !(Invoice?.JobDeclaration?.IsInterface ?? ZBool.False);
		}

		void MarkParentGroupHeadersAsNeedingValidation()
		{
			BaseJobComInvoiceGroupHeader groupHeader = Invoice.GroupHeader;
			while (groupHeader != null)
			{
				groupHeader.Charges.MarkAsNeedingValidation();
				groupHeader = groupHeader.GroupHeader;
			}
		}

		#endregion

		#region Related Objects

		public BaseJobComInvoiceHeader Invoice
		{
			get { return (BaseJobComInvoiceHeader)base.Parent; }
		}

		#endregion

		#region Implementation

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation()
		{
			return new BaseInvoiceChargeValidation(this);
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

	}
}
