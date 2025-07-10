namespace Enterprise.Customs.Business
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Schema;

	public interface ICusInBondCargoDescCollection<out T> : IActiveBusinessObjectCollection<T>
		where T : CusInBondCargoDesc
	{
		new T this[int index] { get; }
		new T[] ToArray();
		IEnumerable<T> Find(Func<T, bool> predicate);
		new T[] Find(ZQuery filter);
		void ApplySort(string propertyName, ListSortDirection direction);
		void MarkAsNeedingValidation();
		void RefreshBinding();
		void SetReadOnlyIncludingChildren(bool readOnly);
	}

	public abstract class CusInBondCargoDescCollection<T> : ActiveBusinessObjectCollection<T>, ICusInBondCargoDescCollection<T>
		where T : CusInBondCargoDesc
	{
		protected CusInBondCargoDescCollection(BusinessObject master)
			: base(master.Factory, master, new ZQuery(CusInBondCargoDescSchema.BY_ParentTableCode, master.TablePrefix), CusInBondCargoDescSchema.BY_ParentID)
		{
		}

		public new T[] Find(ZQuery filter) => base.Find(filter).Cast<T>().ToArray();

		IEnumerable<T> ICusInBondCargoDescCollection<T>.Find(Func<T, bool> predicate) => base.Find(new FindPredicate(predicate));
	}
}
