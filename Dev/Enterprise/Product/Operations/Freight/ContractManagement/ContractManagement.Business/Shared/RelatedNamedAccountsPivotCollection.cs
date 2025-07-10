using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ContractManagement.Business
{
	public class RelatedNamedAccountsPivotCollection : PivotBusinessObjectCollection<RatingContractNamedAccountPivot>, IRelatedNamedAccountsPivotCollection<RatingContractNamedAccountPivot>
	{
		public RelatedNamedAccountsPivotCollection(RatingContract master)
			: base(master, GetCollectionRelationship(master))
		{
		}

		public RelatedNamedAccountsPivotCollection(RatingContractAllocationLine master)
			: base(master, GetCollectionRelationship(master))
		{
		}

		public IReadOnlyCollection<IOrgHeader> GetAllNamedAccounts()
		{
			return (this as IEnumerable<IPivotBusinessObject>)?
				.Select(pivot => pivot.Relation2Object)
				.OfType<IOrgHeader>()
				.ToArray() ?? Array.Empty<IOrgHeader>();
		}

		static CollectionRelationship GetCollectionRelationship(BusinessObject master)
		{
			var query = new ZQuery(RatingContractNamedAccountPivotSchema.RNP_ParentID, master.PK);
			query.AddToFilter(new ZQuery(RatingContractNamedAccountPivotSchema.RNP_ParentTableCode, master.TablePrefix));

			return new CollectionRelationship(typeof(RatingContractNamedAccountPivot), query);
		}

		protected override void SetDefaultsForNewElementCore(RatingContractNamedAccountPivot newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.RNP_ParentTableCode = Master.TablePrefix;
		}
	}
}
