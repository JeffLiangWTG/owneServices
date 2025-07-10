using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	internal class MovementsFilterRelationship : CollectionRelationship
	{
		public MovementsFilterRelationship(ICollectionRelationship baseRelationship)
			: base(typeof(ContainerMovement))
		{
			this.pkList = new List<ZGuid>();
			this.baseRelationship = baseRelationship;
		}

		public MovementsFilterRelationship(ICollectionRelationship baseRelationship, ZQuery filter)
			: base(typeof(ContainerMovement), filter)
		{
			this.pkList = new List<ZGuid>();
			this.baseRelationship = baseRelationship;
		}

		public override bool Equals(object obj)
		{
			return object.ReferenceEquals(this, obj);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Required because Equals is overridden")]
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public void Load(BusinessObjectFactory factory, ZQuery filter)
		{
			BusinessObject[] bizos = baseRelationship.LoadBusinessObjects(factory, filter);

			pkList.Clear();
			pkList.Capacity = bizos.Length;
			pkList.AddRange(Array.ConvertAll(bizos, (m) => m.PK));
			OnRelationshipFilterChanged(EventArgs.Empty);
		}

		protected override CollectionRelationship Clone()
		{
			MovementsFilterRelationship result = (MovementsFilterRelationship)base.Clone();
			result.pkList = new List<ZGuid>(pkList.Count);
			result.pkList.AddRange(pkList);
			return result;
		}

		protected override bool SupportsAddToRelationshipCore()
		{
			return true;
		}

		protected override void AddToRelationship(BusinessObject businessObject)
		{
			pkList.Add(businessObject.PK);
			baseRelationship.AddToRelationship(businessObject);
			OnRelationshipFilterChanged(EventArgs.Empty);
		}

		protected override void RemoveFromRelationship(BusinessObject businessObject)
		{
			pkList.Remove(businessObject.PK);
			baseRelationship.RemoveFromRelationship(businessObject);
			OnRelationshipFilterChanged(EventArgs.Empty);
		}

		protected override void Clear()
		{
			base.Clear();
			OnRelationshipFilterChanged(EventArgs.Empty);
		}

		protected override ZQuery RelationshipFilterCore
		{
			get { return new ZQuery(JobContainerMoveSchema.PK, pkList.ToArray()); }
		}

		protected override BusinessObject[] LoadBusinessObjectsCore(BusinessObjectFactory factory, ZQuery filter)
		{
			List<BusinessObject> result = new List<BusinessObject>();
			foreach (ZGuid pk in pkList)
			{
				BusinessObject businessObject = factory.Load(ElementType, pk);
				if (businessObject != null && businessObject.MatchesFilter(filter))
				{
					result.Add(businessObject);
				}
			}
			return result.ToArray();
		}
		SchemaPKColumn PkColumn => ObjectFactory.Get<IApplicationSchemaResolver>().GetPkColumn(BusinessObjectFactory.GetTableNameFromType(ElementType));

		protected override bool MatchesRelationshipFilterCore(BusinessObject businessObject, bool ignoreActiveFilter, bool fetchOnlyFromLocalCache)
		{
			return
				pkList.Contains(businessObject.PK) &&
				// Calling base may require evaluation of an expensive query for large collections - do the below to retain previous behaviour with a simpler query
				businessObject.MatchesFilter(new ZQuery(PkColumn, businessObject.PK) { IgnoreActiveFilter = ignoreActiveFilter, FetchOnlyFromLocalCache = true });
		}

		List<ZGuid> pkList;
		readonly ICollectionRelationship baseRelationship;
	}
}


