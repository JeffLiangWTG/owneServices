using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Business
{
	[SystemDefinedValues]
	[DependentBusinessObject(typeof(BaseCusGuaranteeHeader), "CusGuaranteeLineTransactions")]
	public class BaseCusGuaranteeLineTransaction : SharedCusPermitLineTransaction, ICommonCusPermitLineTransaction
	{
		public BaseCusGuaranteeLineTransaction(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Override Properties

		[DecimalPlaces(2)]
		public override ZDecimal CPL_TranValue
		{
			get => base.CPL_TranValue;
			set => base.CPL_TranValue = value;
		}

		[RelatedBusinessObject(nameof(BaseCusGuaranteeLineTransaction.GuaranteeHeader))]
		public override ZGuid CPL_CPH_PermitHeader
		{
			get => base.CPL_CPH_PermitHeader;
			set => base.CPL_CPH_PermitHeader = value;
		}

		public override ZString CPL_TransactionType
		{
			get => base.CPL_TransactionType;
			set
			{
				var oldValue = CPL_TransactionType;
				base.CPL_TransactionType = value;
				if (!IsCopying && oldValue != CPL_TransactionType)
				{
					if (CPL_TransactionStatus == ZString.Empty && value == PermitTransactionTypeList.Codes.ADJ)
					{
						CPL_TransactionStatus = PermitTransactionStatusList.Codes.Confirmed;
					}

					CPL_IsAggregated = value == PermitTransactionTypeList.Codes.OBL;
				}
			}
		}

		public override ZBool CPL_IsAggregated
		{
			get => base.CPL_IsAggregated;
			set
			{
				if (IsInDatabase && base.CPL_IsAggregated != value)
				{
					throw new InvalidOperationException($"Cannot change \"{nameof(CPL_IsAggregated)}\" as it has already saved in database!");
				}
				base.CPL_IsAggregated = value;
			}
		}

		protected override ZString ShortName => Res.GetString("389B320F-573C-44C1-8A32-48A35B49AF29", "Guarantee Transaction");

		public override void Delete()
		{
			if (IsInDatabase)
			{
				throw new InvalidOperationException($"Cannot delete \"{ShortName}\" as it has already saved in database!");
			}
			base.Delete();
		}

		#endregion Override Properties

		#region New Properties

		public BaseCusGuaranteeHeader GuaranteeHeader => Factory.Load<BaseCusGuaranteeHeader>(CPL_CPH_PermitHeader);

		#endregion New Properties

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded)
			{
				GuaranteeHeader.CPH_Calc_OpeningBalanceInfo.RefreshBinding();
				GuaranteeHeader.CPH_Calc_TotalBalanceIncludingPendingDecimalInfo.RefreshBinding();
				GuaranteeHeader.CPH_Calc_UsedBalanceInfo.RefreshBinding();
				GuaranteeHeader.CPH_Calc_PendingBalanceDecimalInfo.RefreshBinding();
			}
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			CPL_TransactionCategory = "VAL";
			CPL_Reference = "TRANSLINE";
			CPL_TransactionDate = ZDate.BrettsBirthday;
		}

#endif

		#region Validation and Lookups

		public new CusGuaranteeLineTransactionLookups Lookups => (CusGuaranteeLineTransactionLookups)base.Lookups;

		protected override CusPermitLineTransactionLookups GetNewLookups() => new CusGuaranteeLineTransactionLookups(this);

		public new CusGuaranteeLineTransactionValidation Validation => (CusGuaranteeLineTransactionValidation)base.Validation;

		protected override CusPermitLineTransactionValidation GetNewValidation() => new CusGuaranteeLineTransactionValidation(this);

		#endregion Validation and Lookups
	}
}
