using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsProductParamsByWhsAndClientCollection : ActiveBusinessObjectCollection<WhsProductParamsByWhsAndClient>, IWhsProductParamsByWhsAndClientCollection
	{
		public WhsProductParamsByWhsAndClientCollection(OrgSupplierPart master, BusinessObjectFactory factory)
			: base(factory, master, null, WhsProductParamsByWhsAndClientSchema.W3_OP)
		{
		}

		protected override void SetDefaultsForNewElementCore(WhsProductParamsByWhsAndClient productParams)
		{
			base.SetDefaultsForNewElementCore(productParams);

			var part = (OrgSupplierPart)Relationship.Master;
			if (part != null)
			{
				var buyerRelation = part.RelatedOrganisations.BuyerRelations.FirstOrDefault();

				if (buyerRelation != null)
				{
					productParams.W3_OH = buyerRelation.OU_OH;
				}
			}
		}

		public WhsProductParamsByWhsAndClient FindWhsProductParamsByWhsAndClient(ZString headerEDICode, ZGuid warehousePK)
			=> FindProductParams((client, productParams) => client.OH_Code == headerEDICode && productParams.W3_WW == warehousePK);

		public IWhsProductParamsByWhsAndClient FindWhsProductParamsByWhsAndClient(ZGuid clientPK, ZGuid warehousePK)
			=> FindProductParams((client, productParams) => client.PK == clientPK && productParams.W3_WW == warehousePK);

		WhsProductParamsByWhsAndClient FindProductParams(Func<OrgHeader, WhsProductParamsByWhsAndClient, bool> matchingLogic)
		{
			foreach (var paramsByWhsAndClient in this)
			{
				var client = paramsByWhsAndClient.Header;
				if (client != null && matchingLogic(client, paramsByWhsAndClient))
				{
					return paramsByWhsAndClient;
				}
			}

			return null;
		}

		public IEnumerable<IWhsProductParamsByWhsAndClient> FindWhsProductParamsByWhsAndClients(IEnumerable<ZGuid> clientPks, ZGuid warehousePK)
		{
			var hashedClientPks = clientPks.ToHashSet();
			return this.Where(p => hashedClientPks.Contains(p.Header.PK) && p.W3_WW == warehousePK);
		}

		protected override IComparer GetSortComparerForProperty(PropertyDescriptor property, ListSortDirection direction)
		{
			IComparer comparer;
			if (property.Name == nameof(WhsProductParamsByWhsAndClient.W3_WL_StagingLocationBOM))
			{
				comparer = new LocationComparer<WhsProductParamsByWhsAndClient>(property, direction, productParam => productParam.StagingLocationBOM);
			}
			else if (property.Name == nameof(WhsProductParamsByWhsAndClient.W3_WL_InwardsProcessingStagingLocationBOM))
			{
				comparer = new LocationComparer<WhsProductParamsByWhsAndClient>(property, direction, productParam => productParam.InwardProcessingStagingLocationBOM);
			}
			else
			{
				comparer = base.GetSortComparerForProperty(property, direction);
			}

			return comparer;
		}
	}
}
