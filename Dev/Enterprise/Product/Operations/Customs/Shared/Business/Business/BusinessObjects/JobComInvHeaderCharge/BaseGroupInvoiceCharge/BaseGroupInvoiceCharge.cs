using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using static System.FormattableString;

namespace Enterprise.Customs.Business
{
	/// <summary>
	/// Charges that Group Invoice Have
	/// </summary>
	public class BaseGroupInvoiceCharge : CommonNonApportionedCharge
	{
		public BaseGroupInvoiceCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : JobComInvCharge.Schema
		{
			public const string J7_Calc_IsIncludedInITOT = "J7_Calc_IsIncludedInITOT";
		}

		public new static readonly BaseGroupInvoiceChargeTypeDecider TypeDecider = new BaseGroupInvoiceChargeTypeDecider();

		#region Overrides

		public override ZString J7_PrepaidCollect
		{
			get { return base.J7_PrepaidCollect; }
			set
			{
				base.J7_PrepaidCollect = value;
				UpdatePrepaidCollect();
			}
		}

		public override ZDecimal J7_Percentage
		{
			get { return base.J7_Percentage; }
			set
			{
				bool isDiff = base.J7_Percentage != value;
				if (isDiff)
				{
					base.J7_Amount = 0m;
					base.J7_RX_NKCurrency = ZString.Empty;
				}
				base.J7_Percentage = value;
			}
		}

		[RelatedBusinessObject("GroupInvoice")]
		public override ZGuid J7_ParentID
		{
			get { return base.J7_ParentID; }
			set
			{
				bool hasChanges = base.J7_ParentID != value;
				base.J7_ParentID = value;
				if (hasChanges && !IsCopying)
				{
					MarkAllInvoicesAsNeedingValidation();
				}
			}
		}

		protected override ZBool GetNeedCheckChargeType()
		{
			return !(GroupInvoice?.JobDeclaration?.IsInterface ?? ZBool.False);
		}

		#endregion

		#region Overrides for apportionment

		public override ZBool J7_IsIncludedInITOT
		{
			get { return base.J7_IsIncludedInITOT; }
			set
			{
				bool hasChanges = base.J7_IsIncludedInITOT != value;
				base.J7_IsIncludedInITOT = value;
				if (hasChanges && !IsCopying)
				{
					MarkAllInvoicesAsNeedingValidation();
				}
			}
		}

		void MarkAllInvoicesAsNeedingValidation()
		{
			BaseJobComInvoiceGroupHeader groupInvoice = GroupInvoice;
			if (groupInvoice != null)
			{
				BaseJobDeclaration declaration = groupInvoice.JobDeclaration;

				if (declaration != null)
				{
					declaration.Invoices.MarkAsNeedingValidation();
				}
			}
		}

		void MarkDeclarationAsNeedingValidation()
		{
			GroupInvoice?.JobDeclaration?.MarkAsNeedingValidation();
		}

		protected override bool GetIncludedInITOTReadOnly()
		{
			return base.GetIncludedInITOTReadOnly() || IsJ7_IsIncludedInITOTCalculated;
		}

		internal bool IsJ7_IsIncludedInITOTCalculated
		{
			get
			{
				var readOnlyFromChargeConfiguration = IncoTermAndChargeFactory?.IsIncludedInITOTReadOnlyForGroupCharge(J7_ChargeType) ?? false;
				var includedInInvoiceFixed = IncludedInInvoiceFixedForAllInvoices();
				return readOnlyFromChargeConfiguration && (!HasAtLeastOneValidIncoTerm || includedInInvoiceFixed.HasValue && includedInInvoiceFixed.Value);
			}
		}

		internal bool? IncludedInInvoiceFixedForAllInvoices()
		{
			var incoTermFactory = IncoTermAndChargeFactory;
			var charge = ChargeCode;
			bool? result = null;

			if (HasAtLeastOneValidIncoTerm && incoTermFactory != null && charge != null)
			{
				result = GetConfigurationValueForAllInvoices((x) => incoTermFactory.IsIncludedInInvoiceAmountFixed(x, charge));
			}

			return result;
		}

		protected override void DefaultIsIncludedInInvoice(IncoTermAndCustomsChargeFactory incoTermAndChargeFactory, ICustomsChargeCode charge)
		{
			// do nothing for Included in invoice

			if (!IsJ7_IsIncludedInITOTCalculated && charge != null && incoTermAndChargeFactory != null && !charge.IsIncludedInITOTDeemedForThisCharge)
			{
				if (HasAtLeastOneValidIncoTerm)
				{
					bool? defaultValue = GetConfigurationValueForAllInvoices((x) => incoTermAndChargeFactory.GetDefaultIsIncludedInInvoice(x, charge));

					if (defaultValue.HasValue)
					{
						J7_IsIncludedInITOT = !charge.IsIncoTermNeutral && defaultValue.Value;
					}
				}
			}
		}

		bool? GetConfigurationValueForAllInvoices(Func<string, bool> getConfigurationFunc)
		{
			bool? result = null;

			var incoterms = AllInvoiceIncoTerms();
			foreach (var incoTerm in incoterms)
			{
				var configurationValue = getConfigurationFunc(incoTerm);
				if (result == null)
				{
					result = configurationValue;
				}
				else if (result.Value != configurationValue)
				{
					result = null;
					break;
				}
			}

			return result;
		}

		internal bool HasAtLeastOneValidIncoTerm
		{
			get
			{
				var incoterms = AllInvoiceIncoTerms();
				var allIncoTerms = IncoTermAndChargeFactory?.GetAllIncoTerms() ?? Enumerable.Empty<ZString>();
				return incoterms.Any(x => allIncoTerms.Contains(x));
			}
		}

		ZString[] AllInvoiceIncoTerms()
		{
			var result = Enumerable.Empty<ZString>();
			var groupInvoice = GroupInvoice;
			if (groupInvoice != null)
			{
				result = groupInvoice.AllJobComInvoiceHeaders.Cast<BaseJobComInvoiceHeader>().Where(x => !x.JZ_IncoTerm.IsEmpty).Select(x => x.JZ_IncoTerm).Distinct();
			}
			return result.ToArray();
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvHeaderChargeLookups.GroupIsIncludedInLineOptionList))]
		[BusinessObjectTestExclude]
		[MaxLength(3)]
		public ZString J7_Calc_IsIncludedInITOT
		{
			get
			{
				ZString result = ZString.Empty;

				if (IsJ7_IsIncludedInITOTCalculated)
				{
					result = GroupIsIncludedInLinesOptionList.Codes.NotApplicable;
				}
				else
				{
					result = J7_IsIncludedInITOT ? GroupIsIncludedInLinesOptionList.Codes.Yes : GroupIsIncludedInLinesOptionList.Codes.No;
				}

				return result;
			}
			set
			{
				J7_IsIncludedInITOT = value == GroupIsIncludedInLinesOptionList.Codes.Yes;

				if (!IsValidationSuspended)
				{
					((BaseGroupInvoiceChargeValidation)Validation).ValidateJ7_Calc_IsIncludedInITOT();
				}

				J7_Calc_IsIncludedInITOTInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo J7_Calc_IsIncludedInITOTInfo
		{
			get { return GetZPropertyInfo(Schema.J7_Calc_IsIncludedInITOT); }
		}

		public bool J7_Calc_IsIncludedInITOT_ReadOnly
		{
			get { return GetIncludedInITOTReadOnly(); }
		}

		[BusinessObjectTestExclude]
		public ZString J7_Calc_IsIncludedInInvoiceString
		{
			get
			{
				ZString result = ZString.Empty;

				if (IsJ7_IsIncludedInITOTCalculated)
				{
					result = GroupIsIncludedInLinesOptionList.Codes.NotApplicable;
				}
				else
				{
					result = J7_Calc_IsIncludedInInvoiceAmount ? GroupIsIncludedInLinesOptionList.Codes.Yes : GroupIsIncludedInLinesOptionList.Codes.No;
				}

				return result;
			}
			set
			{
				J7_IsNotIncludedInInvoice = value == GroupIsIncludedInLinesOptionList.Codes.No;
			}
		}

		public override ApportionChargeKey ApportionChargeKey
		{
			get { return new ApportionChargeKey(ChargeKey, J7_FullOrPartialApportionment, J7_Calc_IsIncludedInITOT, J7_Calc_IsIncludedInInvoiceString, J7_DistributeBy, J7_Percentage, J7_AdjustedCharge, J7_ChargeDescription, J7_IsSystem); }
		}

		public override ZString J7_ChargeType
		{
			get { return base.J7_ChargeType; }
			set
			{
				bool hasChanges = base.J7_ChargeType != value;
				base.J7_ChargeType = value;
				if (hasChanges && !IsCopying)
				{
					MarkDeclarationAsNeedingValidation();
					MarkAllInvoicesAsNeedingValidation();
					DefaultPrepaidCollectForGroupCharge();
				}
			}
		}
		public override ZBool J7_IsDutiable
		{
			get => base.J7_IsDutiable;
			set
			{
				bool hasChanges = base.J7_IsDutiable != value;
				base.J7_IsDutiable = value;
				if (hasChanges && !IsCopying)
				{
					MarkDeclarationAsNeedingValidation();
				}
			}
		}

		[DecimalPlaces(2)]
		public override ZDecimal J7_Amount
		{
			get { return base.J7_Amount; }
			set { base.J7_Amount = value; }
		}

		[BusinessObjectTestExclude]
		public override ZBool J7_Calc_IsIncludedInInvoiceAmount
		{
			get { return J7_IsIncludedInITOT; }
			set
			{
				throw new NotSupportedException(Invariant($"{J7_ChargeType} setter for GroupInvoice is not supported. It is always calculated"));
			}
		}

		protected override bool GetJ7_Calc_IsIncludedInInvoiceAmountReadOnly()
		{
			return true;
		}

		#endregion

		#region Related Business Objects

		public BaseJobComInvoiceGroupHeader GroupInvoice
		{
			get { return (BaseJobComInvoiceGroupHeader)base.Parent; }
		}

		#endregion

		#region Implementation

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation()
		{
			return new BaseGroupInvoiceChargeValidation(this);
		}

		protected void DefaultPrepaidCollectForGroupCharge()
		{
			if (GroupInvoice != null)
			{
				bool isPrepaid = false;
				foreach (BaseJobComInvoiceHeader invoice in GroupInvoice.AllJobComInvoiceHeaders)
				{
					var incoTerm = invoice.IncoTerm;
					if (!incoTerm.IsEmpty)
					{
						isPrepaid = invoice.IncoTermAndChargeFactory.CanThisIncoTermHaveThisCharge(incoTerm, ChargeCode);
						if (isPrepaid)
						{
							break;
						}
					}
				}
				base.J7_PrepaidCollect = isPrepaid ? Core.Constants.PaymentType.Prepaid : Core.Constants.PaymentType.Collect;
			}
		}

		protected void UpdatePrepaidCollect()
		{
			if (GroupInvoice != null)
			{
				foreach (BaseJobComInvoiceHeader invoice in GroupInvoice.AllJobComInvoiceHeaders)
				{
					invoice.GroupCharges.UpdatePrepaidCollect(ChargeKey, J7_PrepaidCollect);
				}
			}
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion
	}
}
