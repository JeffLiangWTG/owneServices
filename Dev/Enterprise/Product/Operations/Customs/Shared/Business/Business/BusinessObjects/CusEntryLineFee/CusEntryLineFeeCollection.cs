using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public interface ICusEntryLineFeeCollection<out TFee, out TLine> : IDependentBusinessObjectCollection
		where TFee : CusEntryLineFee
		where TLine : CusEntryLine
	{
		new TFee this[int index] { get; }
		new TLine Master { get; }
		TFee GetOrAddFeeByFeeType(ZString feeType);
		new TFee AddNew();
		void CopyChargesValuesFrom(ICusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine> sourceCollection);
		TFee AddOrUpdate(ZString feeType, ZDecimal amount);
		TFee AddOrUpdate(ZString feeType, ZDecimal amount, ZBool isLandedCostOnly);
		TFee GetElementWithThisCode(ZString feeType, bool isLandedCostOnly);
		TFee GetElementWithThisCode(ZString code);
		ZDecimal GetAmount(ZString feeType);
		ZDecimal GetAmountIncludingLCOnly(ZString feeType);
		ZDecimal GetTotalAmount(ZString feeType, bool includeLandedCostOnly);
		void SetAmount(string feeType, ZDecimal amount);
		void RemoveAndDeleteAll();
		void MarkAsNeedingValidation();
		void RemoveAndDelete(BusinessObject obj);
		void Load();
		bool IsManagedForDataRefresh { get; set; }
		bool IsLoading { get; }

		event CollectionCountChangedEventHandler CountChanged;
	}

	[DependentBusinessObject(typeof(CusEntryLine), "Fees")]
	public class CusEntryLineFeeCollection<TFee, TLine> : DependentBusinessObjectCollection<TFee, TLine>, ICusEntryLineFeeCollection<TFee, TLine>
		where TFee : CusEntryLineFee
		where TLine : CusEntryLine
	{
		public CusEntryLineFeeCollection(TLine master, BusinessObjectFactory factory)
			: base(master, new ZQuery(CusEntryLineFeeSchema.CF_Source, CusEntryLineFeeSourceCodeList.Codes.CW1).AddToFilter(JoinCondition.Or, CusEntryLineFeeSchema.CF_Source, null))
		{
		}

		public TFee GetOrAddFeeByFeeType(ZString feeType)
		{
			var fee = GetElementWithThisCode(feeType) ?? AddNewCore(feeType, false, 0m);

			return fee;
		}

		public void CopyChargesValuesFrom(ICusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine> sourceCollection)
		{
			foreach (TFee fee in this)
			{
				fee.CF_ChargeAmount = 0m;
			}

			foreach (TFee source in sourceCollection)
			{
				AddOrUpdate(source.CF_ChargeType, source.CF_ChargeAmount, source.CF_IsLandedCostOnly);
			}
		}

		public TFee AddOrUpdate(ZString feeType, ZDecimal amount)
		{
			return AddOrUpdate(feeType, amount, false);
		}

		public TFee AddOrUpdate(ZString feeType, ZDecimal amount, ZBool isLandedCostOnly)
		{
			var fee = GetElementWithThisCode(feeType, isLandedCostOnly);
			if (fee != null)
			{
				fee.CF_ChargeAmount = amount;
			}
			else
			{
				fee = AddNewCore(feeType, isLandedCostOnly, amount);
			}

			return fee;
		}

		public ZDecimal GetAmount(ZString feeType)
		{
			return GetElementWithThisCode(feeType)?.CF_ChargeAmount ?? ZDecimal.Zero;
		}

		public ZDecimal GetAmountIncludingLCOnly(ZString feeType)
		{
			return GetTotalAmount(feeType, includeLandedCostOnly: true);
		}

		public ZDecimal GetTotalAmount(ZString feeType, bool includeLandedCostOnly)
		{
			return Elements.Cast<CusEntryLineFee>().CalculateTotalFeeAmount(feeType, includeLandedCostOnly);
		}

		public TFee GetElementWithThisCode(ZString feeType)
		{
			return GetElementWithThisCode(feeType, isLandedCostOnly: false);
		}

		public TFee GetElementWithThisCode(ZString feeType, bool isLandedCostOnly)
		{
			return Elements.Cast<TFee>().FirstOrDefault(x => x.CF_ChargeType == feeType && x.CF_IsLandedCostOnly == isLandedCostOnly);
		}

		public void SetAmount(string feeType, ZDecimal amount)
		{
			CusEntryLineFee fee = GetElementWithThisCode(feeType);

			if (fee != null)
			{
				fee.CF_ChargeAmount = amount;
			}
			else if (amount > 0m)
			{
				fee = AddNewCore(feeType, false, amount);
			}
		}

		protected TFee AddNewCore(ZString feeType, ZBool isLandedCostOnly, ZDecimal amount)
		{
			var fee = AddNew();
			fee.CF_ChargeType = feeType;
			fee.CF_IsLandedCostOnly = isLandedCostOnly;
			fee.CF_ChargeAmount = amount;

			return fee;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var entryLineFee = child as CusEntryLineFee;
			entryLineFee.CF_Source = CusEntryLineFeeSourceCodeList.Codes.CW1;
		}
	}
}
