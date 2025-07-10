using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsPickFaceCollection : ActiveBusinessObjectCollection<WhsPickFace>
	{
		public WhsPickFaceCollection(OrgSupplierPart master, BusinessObjectFactory factory)
			: base(factory, master, null, WhsPickFaceSchema.WF_OP)
		{
		}

		protected override IComparer GetSortComparerForProperty(PropertyDescriptor property, ListSortDirection direction)
		{
			switch (property.Name)
			{
				case nameof(WhsPickFace.LocationString):
				case nameof(WhsPickFace.WF_WL):
					return new LocationComparer<WhsPickFace>(property, direction, pickFace => pickFace.Location);
				default:
					return base.GetSortComparerForProperty(property, direction);
			}
		}

		#region ContainsPickFace

		public bool ContainsPickFace(OrgHeader client, WhsLocation location)
		{
			return this.Any(f => f.WF_OH_Client == client.PK && f.WF_WL == location.PK);
		}

		#endregion

		#region FindByLocation

		public WhsPickFace FindByLocation(ZGuid locationPK)
		{
			return this.Cast<WhsPickFace>().FirstOrDefault(f => f.WF_WL == locationPK);
		}

		public WhsPickFace FindByLocation(ZGuid clientPK, ZGuid locationPK)
		{
			return this.Cast<WhsPickFace>().FirstOrDefault(f => f.WF_OH_Client == clientPK && f.WF_WL == locationPK);
		}

		#endregion

		#region HasPickFace

		public bool HasPickFace(OrgHeader client, OrgSupplierPart part, WhsWarehouse warehouse) => HasPickFace(client.PK, part.PK, warehouse.PK);

		public bool HasPickFace(ZGuid clientPk, ZGuid partPk, ZGuid warehousePk) => GetPickFaces(clientPk, partPk, warehousePk).Any();

		IEnumerable<WhsPickFace> GetPickFaces(ZGuid clientPk, ZGuid partPk, ZGuid warehousePk)
		{
			return this.Where(
				p => p.WF_OH_Client == clientPk
				&& p.WF_OP == partPk
				&& p.LocationWhsGuid == warehousePk);
		}

		#endregion

		#region FindAllPickFaceLocations

		public IEnumerable<WhsLocation> FindAllPickFaceLocations(ZGuid clientPk, ZGuid partPk, ZGuid warehousePk)
		{
			return GetPickFaces(clientPk, partPk, warehousePk).Select(p => p.Location);
		}

		#endregion

		#region SetDefaultsForNewChild

		protected override void SetDefaultsForNewElementCore(WhsPickFace pickFace)
		{
			base.SetDefaultsForNewElementCore(pickFace);

			var part = (OrgSupplierPart)Relationship.Master;
			if (part != null)
			{
				var buyerRelation = part.RelatedOrganisations.BuyerRelations.FirstOrDefault();
				if (buyerRelation != null)
				{
					pickFace.WF_OH_Client = buyerRelation.OU_OH;
				}

				pickFace.WF_ReplenishmentMultiple = part.SmallestStockKeepingUnitSize;
			}
		}

		#endregion
	}
}
