using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class BaseDeclarationLevelPackingGroupCollection : BusinessObjectCollection<BasePackingGroup>
	{
		public BaseDeclarationLevelPackingGroupCollection(BaseJobDeclaration declaration)
			: base(declaration.Factory)
		{
			this.Declaration = declaration;
		}

		protected readonly BaseJobDeclaration Declaration;

		public bool IsSynchronising;

		public List<BasePackingGroup> GetPackingGroupsWithNoPackages()
		{
			List<BasePackingGroup> result = new List<BasePackingGroup>();
			if (!IsSynchronising)
			{
				foreach (BasePackingGroup packGroup in this)
				{
					if (packGroup.Packages.Count == 0)
					{
						result.Add(packGroup);
					}
				}
			}
			return result;
		}

		public BasePackingGroup GetElementWithHouseBillAndContainerOrEquipment(Bill bill, BusinessObject containerOrEquipment)
		{
			var isContainer = containerOrEquipment is BaseCusContainer;
			var isEqupment = !isContainer && containerOrEquipment is CusEquipment;
			var pk = containerOrEquipment?.PK ?? ZGuid.Empty;
			var billPK = bill?.PK ?? ZGuid.Empty;
			Func<BasePackingGroup, ZGuid, bool> doesPackingGroupMatch = null;
			if (isContainer)
			{
				doesPackingGroupMatch = (BasePackingGroup packingGroup, ZGuid containerPK) => packingGroup.CR_CO_Container == containerPK;
			}
			else if (isEqupment)
			{
				doesPackingGroupMatch = (BasePackingGroup packingGroup, ZGuid equipmentPK) => packingGroup.CR_CEQ_Equipment == equipmentPK;
			}
			else
			{
				doesPackingGroupMatch = (BasePackingGroup packingGroup, ZGuid equipmentPK) => packingGroup.CR_CO_Container.IsEmpty && packingGroup.CR_CEQ_Equipment.IsEmpty;
			}

			return this.Cast<BasePackingGroup>().FirstOrDefault(x => x.CR_CU_HouseBill == billPK && doesPackingGroupMatch(x, pk));
		}

		public BasePackingGroup GetElementWithNoContainer()
		{
			var hasHouseBill = false;
			var resultIsHouse = false;
			BasePackingGroup result = null;

			foreach (BasePackingGroup packGroup in this)
			{
				var groupIsHouse = packGroup.BillType == BillTypeList.Codes.HouseBill;
				hasHouseBill |= groupIsHouse;

				if (packGroup.CR_CO_Container.IsEmpty)
				{
					result = packGroup;
					resultIsHouse = groupIsHouse;
					if (resultIsHouse)
					{
						break;
					}
				}
			}

			return resultIsHouse || !hasHouseBill ? result : null;
		}

		public BasePackingGroup GetElementWithNoHouseBill()
		{
			BasePackingGroup result = null;

			foreach (BasePackingGroup packGroup in this)
			{
				if (packGroup.CR_CU_HouseBill.IsEmpty)
				{
					result = packGroup;
					break;
				}
			}
			return result;
		}

		protected override bool FetchOnlyFromLocalCache
		{
			get { return !Declaration.IsInDatabase; }
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			if (Declaration.IsInDatabase)
			{
				result.AddToFilter(CusDecHouseContainerPivotSchema.CR_ClusterKey, Declaration.JE_ClusterKey);
			}

			//DO NOT use Declaration.Bills
			//We want Load() of this collection and Declaration.Bills to be indepedent.
			var billsQuery = new ZQuery(CusDecHouseBillSchema.CU_ClusterKey, Declaration.JE_ClusterKey);
			billsQuery.AddToFilter(CusDecHouseBillSchema.CU_JE, Declaration.PK);

			if (!Declaration.IsInDatabase)
			{
				billsQuery.FetchOnlyFromLocalCache = true;
			}
			Bill[] bills = Factory.Load<Bill>(billsQuery);
			List<ZGuid> billPKs = new List<ZGuid>();

			if (bills.Length > 0)
			{
				foreach (Bill bill in bills)
				{
					billPKs.Add(bill.PK);
				}
				result.AddToFilter(CusDecHouseContainerPivotSchema.CR_CU_HouseBill, billPKs);
			}
			else
			{
				result.IsNoResultQuery = true;
			}
			return result;
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			base.SetCollectionRelationships(child);
			((BasePackingGroup)child).Declaration = Declaration;
			if (!Declaration.IsDeleted)
			{
				((BasePackingGroup)child).CR_ClusterKey = Declaration.JE_ClusterKey;
			}
		}

		protected override bool AllowNewCore
		{
			get { return Declaration != null && (!Declaration.IsPluggedIntoShipment || Declaration.JE_OverrideFreightDefaults); }
		}
	}
}
