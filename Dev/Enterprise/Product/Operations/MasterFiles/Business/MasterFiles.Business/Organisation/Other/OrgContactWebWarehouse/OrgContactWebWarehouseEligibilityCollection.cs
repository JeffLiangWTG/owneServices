using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public sealed class OrgContactWebWarehouseEligibilityCollection : NonPersistentBusinessObjectCollection<OrgContactWebWarehouseEligibility>
	{
		public OrgContactWebWarehouseEligibilityCollection(BusinessObjectFactory factory, OrgContact contact)
			: base(factory)
		{
			LoadAllWarehouseEligibility(factory, Argument.NotNull(contact, "contact"));
		}

		#region LoadAllWarehouses

		void LoadAllWarehouseEligibility(BusinessObjectFactory factory, OrgContact contact)
		{
			var genPivotQuery = new ZQuery();
			genPivotQuery.AddToFilter(GenPivotSchema.XX_RelationType, Constants.GenPivotTypes.OrgContactDeniedWarehouse);
			genPivotQuery.AddToFilter(GenPivotSchema.XX_Relation1TableCode, OrgContactSchema.Constants.Prefix);
			genPivotQuery.AddToFilter(GenPivotSchema.XX_Relation1ID, contact.PK);
			genPivotQuery.AddToFilter(GenPivotSchema.XX_Relation2TableCode, WhsWarehouseSchema.Constants.Prefix);
			var deniedWarehouses = factory.Load<GenPivot>(genPivotQuery);

			var activeWarehouses = factory.Load<IWhsWarehouse>(new ZQuery(WhsWarehouseSchema.WW_IsActive, true));
			foreach (var warehouse in activeWarehouses.OrderBy(w => w.WW_WarehouseCode))
			{
				var newOrgContactWebWarehouseEligibility = new OrgContactWebWarehouseEligibility(factory, warehouse, contact, deniedWarehouses.SingleOrDefault(x => x.XX_Relation2ID == warehouse.PK));
				Add(newOrgContactWebWarehouseEligibility);
			}
		}

		#endregion

		#region Overrides 

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return null;
		}
	}
}
