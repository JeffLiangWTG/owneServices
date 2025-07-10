using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;

namespace Enterprise.Customs.Business
{
	public class MultipleCollectionRelationship : CollectionRelationship
	{
		public MultipleCollectionRelationship(Type elementType, ICollectionRelationship mainRelationship, ICollectionRelationship additionalRelationship, bool supportAdditionalRelationship)
			: base(elementType)
		{
			this.mainRelationship = Argument.NotNull(mainRelationship, "mainRelationship");
			this.additionalRelationship = Argument.NotNull(additionalRelationship, "additionalRelationship");
			SupportAdditionalRelationship = supportAdditionalRelationship;

			this.mainRelationship.RelationshipFilterChanged += MainRelationship_RelationshipFilterChanged;
			this.additionalRelationship.RelationshipFilterChanged += AdditionalRelationship_RelationshipFilterChanged;
		}

		void AdditionalRelationship_RelationshipFilterChanged(object sender, EventArgs e)
		{
			OnRelationshipFilterChanged(e);
		}

		void MainRelationship_RelationshipFilterChanged(object sender, EventArgs e)
		{
			OnRelationshipFilterChanged(e);
		}

		ICollectionRelationship mainRelationship;
		ICollectionRelationship additionalRelationship;
		public bool SupportAdditionalRelationship;

		#region Equals / GetHashCode

		public override bool Equals(object obj)
		{
			return object.ReferenceEquals(this, obj);
		}

		public override int GetHashCode()
		{
			return hashObject.GetHashCode();
		}

		readonly object hashObject = new object();

		#endregion

		#region Clone

		protected override CollectionRelationship Clone()
		{
			MultipleCollectionRelationship result = (MultipleCollectionRelationship)base.Clone();
			var mainRelationship = (ICloneable)this.mainRelationship;
			if (mainRelationship != null)
			{
				result.mainRelationship = (CollectionRelationship)mainRelationship.Clone();
			}
			var additionalRelationship = (ICloneable)this.additionalRelationship;
			if (additionalRelationship != null)
			{
				result.additionalRelationship = (CollectionRelationship)additionalRelationship.Clone();
			}
			return result;
		}

		#endregion

		#region AddToRelationship / RemoveFromRelationship / Clear

		protected override bool SupportsAddToRelationshipCore()
		{
			return true;
		}

		protected override void AddToRelationship(BusinessObject businessObject)
		{
			OnBeforeCollectionCountChange(false, businessObject);
			mainRelationship.AddToRelationship(businessObject);
			businessObject.HasChangesChanged -= BusinessObjectChanged;
			businessObject.HasChangesChanged += BusinessObjectChanged;
			OnRelationshipFilterChanged(EventArgs.Empty);
		}

		protected override void RemoveFromRelationship(BusinessObject businessObject)
		{
			businessObject.HasChangesChanged -= BusinessObjectChanged;
			OnBeforeCollectionCountChange(false, businessObject);

			if (mainRelationship.MatchesRelationshipFilter(businessObject, false, false))
			{
				mainRelationship.RemoveFromRelationship(businessObject);
			}
			if (SupportAdditionalRelationship && additionalRelationship.MatchesRelationshipFilter(businessObject, false, false))
			{
				additionalRelationship.RemoveFromRelationship(businessObject);
			}
			OnRelationshipFilterChanged(EventArgs.Empty);
		}

		protected void OnBeforeCollectionCountChange(bool isAdded, BusinessObject businessObject)
		{
			if (BeforeCollectionCountChange != null)
			{
				BeforeCollectionCountChange(this, new CollectionCountChangedEventArgs(isAdded, businessObject));
			}
		}

		public event CollectionCountChangedEventHandler BeforeCollectionCountChange;

		protected override void Clear()
		{
			mainRelationship.Clear();
			if (SupportAdditionalRelationship)
			{
				additionalRelationship.Clear();
			}

			OnRelationshipFilterChanged(EventArgs.Empty);
		}

		#endregion

		#region RelationshipFilter

		protected override ZQuery RelationshipFilterCore
		{
			get
			{
				ZQuery query = new ZQuery();
				query.AddToFilter(mainRelationship.RelationshipFilter, JoinCondition.Or);
				if (SupportAdditionalRelationship)
				{
					query.AddToFilter(additionalRelationship.RelationshipFilter, JoinCondition.Or);
				}

				return query;
			}
		}

		#endregion

		#region Implementation

		protected override BusinessObject[] LoadBusinessObjectsCore(BusinessObjectFactory factory, ZQuery filter)
		{
			List<BusinessObject> result = new List<BusinessObject>();
			result.AddRange(mainRelationship.LoadBusinessObjects(factory, filter));
			if (SupportAdditionalRelationship)
			{
				result.AddRange(additionalRelationship.LoadBusinessObjects(factory, filter));
			}

			return result.ToArray();
		}

		protected override bool MatchesRelationshipFilterCore(BusinessObject businessObject, bool ignoreActiveFilter, bool fetchOnlyFromLocalCache)
		{
			return base.MatchesRelationshipFilterCore(businessObject, ignoreActiveFilter, true)
				&& (mainRelationship.MatchesRelationshipFilter(businessObject, ignoreActiveFilter, fetchOnlyFromLocalCache)
				|| (SupportAdditionalRelationship && additionalRelationship.MatchesRelationshipFilter(businessObject, ignoreActiveFilter, fetchOnlyFromLocalCache)));
		}

		void BusinessObjectChanged(object o, EventArgs e)
		{
			BusinessObject businessObject = (BusinessObject)o;
			if (businessObject.IsDeleted)
			{
				RemoveFromRelationship(businessObject);
			}
		}

		#endregion

		#region Methods

		public void Refresh()
		{
			OnRelationshipFilterChanged(EventArgs.Empty);
		}

		public bool MatchesAdditionalRelationshipFilter(BusinessObject businessObject)
		{
			return SupportAdditionalRelationship && additionalRelationship.MatchesRelationshipFilter(businessObject, false, false);
		}

		#endregion
	}

	public class ManyToManyRelationshipWithRelationType : ManyToManyRelationship
	{
		public ManyToManyRelationshipWithRelationType(BusinessObject master, Type elementType, Type pivotObjectType, string relationType)
			: base(master, elementType, pivotObjectType)
		{
			this.relationType = relationType;
		}

		public ManyToManyRelationshipWithRelationType(BusinessObject master, Type elementType, Type pivotObjectType, ZQuery filter, SchemaGuidColumn pivotTableFKToMaster, SchemaGuidColumn pivotTableFKToElements, string relationType)
			: base(master, elementType, pivotObjectType, filter, pivotTableFKToMaster, pivotTableFKToElements)
		{
			this.relationType = relationType;
		}

		readonly string relationType;

		protected override string GetPivotDataViewRowFilter()
		{
			return string.Format("{0} AND XX_RelationType ='{1}'", base.GetPivotDataViewRowFilter(), relationType);
		}
	}
}
