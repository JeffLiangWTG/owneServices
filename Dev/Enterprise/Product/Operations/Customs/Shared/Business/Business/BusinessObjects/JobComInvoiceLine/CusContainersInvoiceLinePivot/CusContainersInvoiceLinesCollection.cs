using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusContainersInvoiceLinesCollection : DependentBusinessObjectCollection<CusContainerInvoiceLinePivot, BaseJobComInvoiceLine>
	{
		public CusContainersInvoiceLinesCollection(BaseJobComInvoiceLine associatedInvoiceLine)
			: base(associatedInvoiceLine)
		{
		}

		public bool Contains(BaseCusContainer container)
		{
			return GetRelatedPivot(container) != null;
		}

		public CusContainerInvoiceLinePivot AddPivotFor(BaseCusContainer container)
		{
			CusContainerInvoiceLinePivot result = GetRelatedPivot(container);
			if (result == null)
			{
				result = AddNew();
				result.C2_CO = container.PK;
				result.C2_JI = Master.PK;
			}
			return result;
		}

		public void DeletePivotFor(BaseCusContainer container)
		{
			CusContainerInvoiceLinePivot pivot = GetRelatedPivot(container);
			if (pivot != null)
			{
				pivot.Delete();
			}
		}

		public CusContainerInvoiceLinePivot GetRelatedPivot(BaseCusContainer container)
		{
			CusContainerInvoiceLinePivot result = null;
			foreach (CusContainerInvoiceLinePivot pivot in this)
			{
				if (pivot.C2_CO == container.PK)
				{
					result = pivot;
					break;
				}
			}
			return result;
		}

		public BaseCusContainer UniqueContainer
		{
			get { return Count == 1 ? this[0].Container : null; }
		}

		#region Total

		public ZDecimal TotalSplitValue
		{
			get { return this.OfType<CusContainerInvoiceLinePivot>().Sum(x => x.C2_SplitValue); }
		}

		public ZDecimal TotalNetWeightInKG
		{
			get { return this.OfType<CusContainerInvoiceLinePivot>().Sum(x => x.C2_NetWeight); }
		}

		public ZInt TotalPackQty
		{
			get { return this.OfType<CusContainerInvoiceLinePivot>().Sum(x => x.C2_PackQty); }
		}

		#endregion

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusContainerInvoiceLinePivotSchema.C2_JI; }
		}
	}

	public class CusContainersInvoiceLinesCollection<T, TInvoiceLine, TContainer> : CusContainersInvoiceLinesCollection
		where T : CusContainerInvoiceLinePivot
		where TInvoiceLine : BaseJobComInvoiceLine
		where TContainer : BaseCusContainer
	{
		public CusContainersInvoiceLinesCollection(TInvoiceLine associatedInvoiceLine)
			: base(associatedInvoiceLine)
		{
		}

		public new TInvoiceLine Master
		{
			get { return (TInvoiceLine)base.Master; }
		}

		public new T this[int i]
		{
			get { return (T)base[i]; }
		}

		public new T AddNew()
		{
			return (T)base.AddNew();
		}

		public new T AddNew(Type bizOType)
		{
			return (T)base.AddNew(bizOType);
		}

		public IEnumerable<T> Find(Func<T, bool> predicate)
		{
			foreach (T result in base.Find((CusContainerInvoiceLinePivot x) => predicate((T)x)))
			{
				yield return result;
			}
		}

		public T AddPivotFor(TContainer container)
		{
			return (T)base.AddPivotFor(container);
		}

		public T GetRelatedPivot(TContainer container)
		{
			return (T)base.GetRelatedPivot(container);
		}

		public new TContainer UniqueContainer
		{
			get { return (TContainer)base.UniqueContainer; }
		}
	}
}
