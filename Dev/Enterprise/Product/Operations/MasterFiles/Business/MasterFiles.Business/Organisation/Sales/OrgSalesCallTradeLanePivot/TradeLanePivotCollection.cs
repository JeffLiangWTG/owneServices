using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class TradeLanePivotCollection<T> : ActiveBusinessObjectCollection<OrgSalesValueAssociationPivot> where T : BusinessObject
	{
		public TradeLanePivotCollection(T master)
			: base(master.Factory, master, new ZQuery(), OrgSalesValueAssociationPivotSchema.SVP_ActivityId)
		{
			this.master = master;
		}
		readonly T master;

		#region Implementation

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			result.AddToFilter(OrgSalesValueAssociationPivotSchema.SVP_ActivityTableCode, master.TablePrefix);
			return result;
		}

		protected override void SetRelationshipDefaultsForElementCore(OrgSalesValueAssociationPivot newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
			newElement.SVP_ActivityId = master.PK;
			newElement.SVP_ActivityTableCode = master.TablePrefix;
		}

		#endregion

		public OrgSales[] GetTradeLanes()
		{
			List<ZGuid> tradeLanesPKs = new List<ZGuid>();
			foreach (var pivot in this)
			{
				if (!pivot.IsDeleted)
				{
					tradeLanesPKs.Add(pivot.SVP_TradeId);
				}
			}

			ZQuery loadAllQuery = new ZQuery(OrgSalesSchema.PK, tradeLanesPKs.ToArray());
			return Factory.Load<OrgSales>(loadAllQuery);
		}

		public OrgSalesValueAssociationPivot GetRelatedPivot(ISalesValue salesValue)
		{
			if (salesValue != null)
			{
				foreach (var pivot in this)
				{
					if (!pivot.IsDeleted && pivot.SVP_TradeId == salesValue.Identifier)
					{
						return pivot;
					}
				}
			}

			return null;
		}

		public OrgSalesValueAssociationPivot AddPivotFor(ISalesValue salesValue)
		{
			OrgSalesValueAssociationPivot result = null;
			var pivot = GetRelatedPivot(salesValue);
			if (pivot == null && salesValue != null)
			{
				result = AddNew();
				result.SVP_TradeId = salesValue.Identifier;
				result.SVP_TradeTableCode = salesValue.TablePrefix;
			}

			return result;
		}

		public void DeletePivotFor(ISalesValue salesValue)
		{
			OrgSalesValueAssociationPivot pivot = GetRelatedPivot(salesValue);
			if (pivot != null)
			{
				RemoveFromRelationship(pivot);
				Delete(pivot);
			}
		}

		public bool Contains(ISalesValue salesValue)
		{
			return GetRelatedPivot(salesValue) != null;
		}

		public void DeleteObsoletePivots(ISalesValue[] existingTradeLanes)
		{
			List<OrgSalesValueAssociationPivot> obsoletePivots = new List<OrgSalesValueAssociationPivot>();
			foreach (var pivot in this)
			{
				bool found = false;
				foreach (var tradeLane in existingTradeLanes)
				{
					if (tradeLane.Identifier == pivot.SVP_TradeId)
					{
						found = true;
						break;
					}
				}
				if (!found)
				{
					obsoletePivots.Add(pivot);
				}
			}

			foreach (var pivot in obsoletePivots)
			{
				RemoveFromRelationship(pivot);
				Delete(pivot);
			}
		}
	}
}
