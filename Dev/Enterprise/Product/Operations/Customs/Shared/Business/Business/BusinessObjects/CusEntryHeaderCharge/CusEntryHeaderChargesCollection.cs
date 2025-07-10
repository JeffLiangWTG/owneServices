using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public interface ICusEntryHeaderChargesCollection<out T> : IBusinessObjectCollection<T>
		where T : CusEntryHeaderCharges
	{
		new T this[int index] { get; }
		T this[string chargeType] { get; }
		T AddNew(string chargeType);
		T AddNew(string chargeType, ZDecimal amount);

		void SetAmount(string chargeType, ZDecimal chargeAmount);
		ZDecimal GetAmount(string chargeType);
		ZDecimal GetAmountIncludingLCOnly(string chargeType);
		ZDecimal GetTotalAmount(ZString chargeType, bool includeLandedCostOnly);
		ZDecimal GetTotalAmount(IEnumerable<string> codeTypes);
		ZDecimal TotalAmount { get; }

		IEnumerable<T> GetCharges(bool? landCostOnly = null);
		T GetChargeWithThisCode(string chargeType);
		void CopyChargesValuesFrom(ICusEntryHeaderChargesCollection<CusEntryHeaderCharges> sourceCollection);
		IEnumerable<T> GetChargesByTypeAndPaymentMethod(ZString chargeType, ZString methodOfPayment);
		bool HasOverrideFeeOfGivenCode(ZString chargeType, ZString rateOverrideReasonCode);
	}

	[DependentBusinessObject(typeof(CusEntryHeader), "Charges")]
	public class CusEntryHeaderChargesCollection<T> : DependentBusinessObjectCollection<T, CusEntryHeader>, ICusEntryHeaderChargesCollection<T>
		where T : CusEntryHeaderCharges
	{
		public CusEntryHeaderChargesCollection(CusEntryHeader entryHeader)
			: base(entryHeader, new ZQuery(CusEntryHeaderChargesSchema.C1_Source, CusEntryHeaderChargesSourceCodeList.Codes.CW1).AddToFilter(JoinCondition.Or, CusEntryHeaderChargesSchema.C1_Source, null))
		{
		}

		protected CusEntryHeader EntryHeader => Master;

		public IEnumerator<T> GetEnumerator() => Elements.Cast<T>().GetEnumerator();

		public T this[string chargeType] => GetChargeAndReturnNewIfNotFound(chargeType);

		public T AddNew(string chargeType) => GetChargeAndReturnNewIfNotFound(chargeType);

		public T AddNew(string chargeType, ZDecimal amount)
		{
			T result = AddNew(chargeType);
			result.C1_ChargeAmount = amount;
			return result;
		}

		/// <summary>
		/// Clear existing amounts and copy values from SourceCollection. In result, there might be additional charges that are copied
		/// </summary>
		public void CopyChargesValuesFrom(ICusEntryHeaderChargesCollection<CusEntryHeaderCharges> sourceCollection)
		{
			foreach (T charge in this)
			{
				charge.C1_ChargeAmount = 0m;
			}

			foreach (T source in sourceCollection)
			{
				T destination = this[source.C1_ChargeType];
				destination.C1_ChargeAmount = source.C1_ChargeAmount;
			}
		}

		public IEnumerable<T> GetCharges(bool? landCostOnly = null)
			=> this.Cast<T>().Where(x => !landCostOnly.HasValue || x.C1_IsLandedCostOnly == landCostOnly.Value);

		public T GetCharge(string chargeType) => GetChargeWithThisCode(chargeType);

		public T GetChargeWithThisCode(string chargeType) => GetCharge(x => x.C1_ChargeType == chargeType && !x.C1_IsLandedCostOnly);

		public IEnumerable<T> GetChargesByTypeAndPaymentMethod(ZString chargeType, ZString methodOfPayment) => Elements.Cast<T>().Where(x => x.C1_ChargeType == chargeType && x.C1_MethodOfPayment == methodOfPayment);

		public ZDecimal GetAmount(string chargeType) => GetCharge(x => x.C1_ChargeType == chargeType && !x.C1_IsLandedCostOnly)?.C1_ChargeAmount ?? ZDecimal.Zero;

		public ZDecimal GetAmountIncludingLCOnly(string chargeType) => GetTotalAmount(chargeType, includeLandedCostOnly: true);

		public ZDecimal GetTotalAmount(ZString chargeType, bool includeLandedCostOnly)
			=> Elements.Cast<T>().Where(x => x.C1_ChargeType == chargeType && (includeLandedCostOnly || !x.C1_IsLandedCostOnly)).Sum(x => x.C1_ChargeAmount);

		public ZDecimal TotalAmount => Elements.Cast<T>().Sum(x => x.C1_ChargeAmount);

		public ZDecimal GetTotalAmount(IEnumerable<string> codeTypes)
		{
			var result = 0m;

			var passedCodeTypes = new List<string>(codeTypes);

			foreach (var charge in this)
			{
				if (passedCodeTypes.Contains(charge.C1_ChargeType.ToString()))
				{
					result += charge.C1_ChargeAmount;
				}
			}

			return result;
		}

		public bool HasOverrideFeeOfGivenCode(ZString chargeType, ZString rateOverrideReasonCode) => Elements.Cast<CusEntryHeaderCharges>().Any(f => f.C1_ChargeType == chargeType && f.C1_RateOverrideReasonCode == rateOverrideReasonCode);

		public void SetAmount(string chargeType, ZDecimal chargeAmount)
		{
			T charge = GetChargeWithThisCode(chargeType);

			if (charge == null && chargeAmount > 0m)
			{
				charge = AddNew(chargeType);
			}

			if (charge != null)
			{
				charge.C1_ChargeAmount = chargeAmount;
			}
		}

		protected T GetChargeAndReturnNewIfNotFound(string chargeType)
		{
			var result = GetChargeWithThisCode(chargeType);
			if (result == null)
			{
				result = AddNew();
				result.C1_ChargeType = chargeType;
			}
			return result;
		}

		T GetCharge(System.Func<T, bool> predicate) => Elements.Cast<T>().FirstOrDefault(predicate);

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var charge = child as T;
			charge.C1_Source = CusEntryLineFeeSourceCodeList.Codes.CW1;
		}
	}
}
