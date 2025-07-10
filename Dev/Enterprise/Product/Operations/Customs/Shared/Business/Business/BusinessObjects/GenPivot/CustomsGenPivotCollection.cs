using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public interface ICustomsGenPivotCollection<out TPivot, TMaster, TParent> : IDependentBusinessObjectCollection, IBusinessObjectCollection<TPivot>
		where TPivot : CustomsGenPivot
		where TMaster : BusinessObject
		where TParent : BusinessObject
	{
		new TMaster Master { get; }
		bool IsManagedForDataRefresh { set; }
		TPivot GetRelatedPivot(TParent parent);
		TPivot AddPivotFor(TParent parent);
		void DeletePivotFor(TParent parent);
		bool Contains(TParent parent);
	}

	public abstract class CustomsGenPivotCollection<TPivot, TMaster, TParent> : DependentBusinessObjectCollection<TPivot, TMaster>, ICustomsGenPivotCollection<TPivot, TMaster, TParent>
		where TPivot : CustomsGenPivot
		where TMaster : BusinessObject
		where TParent : BusinessObject
	{
		protected CustomsGenPivotCollection(TMaster master)
			: base(master, master.Factory)
		{
		}

		public IEnumerator<TPivot> GetEnumerator() => Elements.Cast<TPivot>().GetEnumerator();

		public TPivot GetRelatedPivot(TParent parent)
		{
			if (parent != null)
			{
				foreach (var pivot in this)
				{
					if (pivot.Relation2Object == parent)
					{
						return pivot;
					}
				}
			}
			return null;
		}

		public TPivot AddPivotFor(TParent parent)
		{
			TPivot result = GetRelatedPivot(parent);

			if (result == null)
			{
				result = AddNew();

				result.Relation2Object = parent;
			}

			return result;
		}

		public void DeletePivotFor(TParent parent)
		{
			var pivot = GetRelatedPivot(parent);

			if (pivot != null)
			{
				pivot.Delete();
			}
		}

		public bool Contains(TParent parent)
		{
			return GetRelatedPivot(parent) != null;
		}

		protected override string FkColumnName
		{
			get { return GenPivotSchema.Constants.XX_Relation1ID; }
		}

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			if (EnsureHasMaster())
			{
				var pivot = (CustomsGenPivot)dependent;
				pivot.Relation1Object = Master;
				pivot.XX_RelationType = RelationType;
			}
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			result.AddToFilter(GenPivotSchema.XX_RelationType, RelationType);
			return result;
		}

		protected abstract string RelationType { get; }
	}
}
