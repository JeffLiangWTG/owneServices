using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class NPBOCusGuaranteeLineTransaction : AutoNPBOCusGuaranteeLineTransaction
	{
		public NPBOCusGuaranteeLineTransaction(BaseCusGuaranteeHeader cusGuaranteeHeader) : base(Argument.NotNull(cusGuaranteeHeader, nameof(cusGuaranteeHeader)).Factory)
		{
			GuaranteeHeader = cusGuaranteeHeader;
		}

		public BaseCusGuaranteeHeader GuaranteeHeader;

		[List(nameof(Lookups) + "." + nameof(NPBOCusGuaranteeLineTransactionLookups.TransactionTypes))]
		public override ZString CPL_TransactionType
		{
			get => base.CPL_TransactionType;
			set => base.CPL_TransactionType = value;
		}

		public override ZDecimal CPL_TranValue
		{
			get => base.CPL_TranValue;
			set
			{
				var oldValue = CPL_TranValue;
				base.CPL_TranValue = value;
				if (oldValue != CPL_TranValue)
				{
					if (CPL_TransactionType.IsEmpty)
					{
						CPL_TransactionType = !GuaranteeHeader.CusGuaranteeLineTransactions.Any(x => x.CPL_TransactionType == PermitTransactionTypeList.Codes.OBL)
							? PermitTransactionTypeList.Codes.OBL
							: PermitTransactionTypeList.Codes.ADJ;
					}
					CPL_TransactionDate = ZDateTime.Now;
				}
			}
		}

		bool IsTypeEmpty => CPL_TransactionType.IsEmpty;

		protected override bool CPL_TransactionDate_ReadOnly => IsTypeEmpty;

		protected override bool CPL_Reference_ReadOnly => IsTypeEmpty;

		protected override bool CPL_Comment_ReadOnly => IsTypeEmpty;

		protected override bool CPL_TranValue_ReadOnly => IsTypeEmpty;

		public NPBOCusGuaranteeLineTransactionLookups Lookups => fLookups ?? (fLookups = GetNewLookups());
		NPBOCusGuaranteeLineTransactionLookups fLookups;

		protected NPBOCusGuaranteeLineTransactionLookups GetNewLookups() => new NPBOCusGuaranteeLineTransactionLookups();
	}
}
