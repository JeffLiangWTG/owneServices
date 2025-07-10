using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public interface IConfirmedCusEntryHeaderChargesCollection<out T> : IBusinessObjectCollection<T>
	where T : CusEntryHeaderCharges
	{
		new T this[int index] { get; }
		T this[string chargeType] { get; }

		T AddOrUpdate(string chargeType);

		void SetReadOnlyIncludingChildren(bool readOnly);
	}

	[DependentBusinessObject(typeof(CusEntryHeader), "ConfirmedCharges")]
	public class ConfirmedCusEntryHeaderChargesCollection<T> : DependentBusinessObjectCollection<T, CusEntryHeader>, IConfirmedCusEntryHeaderChargesCollection<T>
		where T : CusEntryHeaderCharges
	{
		public ConfirmedCusEntryHeaderChargesCollection(CusEntryHeader entryHeader)
			: base(entryHeader, new ZQuery(CusEntryHeaderChargesSchema.C1_Source, CusEntryHeaderChargesSourceCodeList.Codes.CUS))
		{
		}

		protected CusEntryHeader EntryHeader => Master;

		public IEnumerator<T> GetEnumerator() => Elements.Cast<T>().GetEnumerator();

		public T this[string chargeType] => GetChargeAndReturnNewIfNotFound(chargeType);

		public T AddOrUpdate(string chargeType) => GetChargeAndReturnNewIfNotFound(chargeType);

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

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var charge = child as T;
			charge.C1_Source = CusEntryLineFeeSourceCodeList.Codes.CUS;
		}

		T GetChargeWithThisCode(string chargeType) => GetCharge(x => x.C1_ChargeType == chargeType && !x.C1_IsLandedCostOnly);

		T GetCharge(System.Func<T, bool> predicate) => Elements.Cast<T>().FirstOrDefault(predicate);
	}
}
