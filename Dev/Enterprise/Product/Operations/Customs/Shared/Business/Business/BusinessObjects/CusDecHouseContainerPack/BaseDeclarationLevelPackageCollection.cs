using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public interface IDeclarationLevelPackageCollection<out TPackage> : IBusinessObjectCollection<TPackage>
			where TPackage : BasePackage
	{
		new TPackage this[int index] { get; }
		new TPackage AddNew();
		void SetReadOnlyIncludingChildren(bool readOnly);
		void MarkAsDeleteForShipmentSynch();
		List<ZGuid> GetPKs();
		IEnumerable<TPackage> LowestPackages { get; }
		bool HasDangerousGoods { get; }
		TPackage GetMatchingElementForShipmentSynch(Bill bill, BaseCusContainer container);
		TPackage GetElementWithNoHouseBill();
		TPackage GetElementWithNoContainer();
		TPackage GetElementWithHouseBillAndContainer(ZString houseBillUniqueCode, ZString containerNo);
	}

	public class BaseDeclarationLevelPackageCollection<TPackage> : BusinessObjectCollection<TPackage>, IPackingInformationCollection, IDeclarationLevelPackageCollection<TPackage>
				where TPackage : BasePackage
	{
		public BaseDeclarationLevelPackageCollection(BaseJobDeclaration declaration)
			: base(declaration.Factory)
		{
			Declaration = declaration;
		}
		protected readonly BaseJobDeclaration Declaration;

		public void MarkAsDeleteForShipmentSynch()
		{
			foreach (TPackage package in this)
			{
				try
				{
					package.IsSynchronising = true;
					package.MarkAsDeleteForShipmentSynch();
				}
				finally
				{
					package.IsSynchronising = false;
				}
			}
		}

		public TPackage GetMatchingElementForShipmentSynch(Bill bill, BaseCusContainer container)
		{
			foreach (TPackage package in this)
			{
				if (package.IsGoingToBeDeletedAfterShipmentSynch)
				{
					var packingGroup = package.PackingGroup;

					if (packingGroup != null && packingGroup.Bill == bill && packingGroup.Container == container)
					{
						package.UnmarkAsDeleteForShipmentSynch();
						return package;
					}
				}
			}
			return null;
		}

		public TPackage GetElementWithHouseBillAndContainer(ZString houseBillUniqueCode, ZString containerNo)
		{
			foreach (TPackage package in this)
			{
				if (package.CW_HouseBill == houseBillUniqueCode && package.CW_ContainerNoOrEquipmentNo == containerNo)
				{
					return package;
				}
			}
			return null;
		}

		public TPackage GetElementWithNoContainer()
		{
			foreach (TPackage package in this)
			{
				if (package.PackingGroup == null || package.PackingGroup.Container == null)
				{
					return package;
				}
			}
			return null;
		}

		public TPackage GetElementWithNoHouseBill()
		{
			foreach (TPackage package in this)
			{
				if (package.PackingGroup == null || package.PackingGroup.Bill == null)
				{
					return package;
				}
			}
			return null;
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			if (Declaration != null && !Declaration.IsDeleted)
			{
				(Declaration.Validation as BaseJobDeclarationValidation)?.ValidatePackagesActualPackageCount();
			}
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			base.SetCollectionRelationships(child);
			var package = child as TPackage;
			package.Declaration = Declaration;
			if (!Declaration.IsDeleted)
			{
				package.CW_ClusterKey = Declaration.JE_ClusterKey;
			}
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			if (Declaration.IsInDatabase)
			{
				result.AddToFilter(CusDecHouseContainerPackSchema.CW_ClusterKey, Declaration.JE_ClusterKey);
			}

			//Do not use JobDeclaration.Bills or JobDeclaration.PackingGroups
			//If this collection's Load() happens while JobDeclaration.Bills are being loaded, 
			//then only elements that belong to the house bills loaded so far will be loaded.
			var query = new ZQuery(CusDecHouseBillSchema.CU_ClusterKey, Declaration.JE_ClusterKey);
			query.AddToFilter(CusDecHouseBillSchema.CU_ClusterKey, Declaration.JE_ClusterKey);

			Bill[] bills = Factory.Load<Bill>(query);
			var billPKs = new List<ZGuid>();

			foreach (var bill in bills)
			{
				billPKs.Add(bill.PK);
			}

			BasePackingGroup[] packGroups = Factory.Load<BasePackingGroup>(new ZQuery(CusDecHouseContainerPivotSchema.CR_CU_HouseBill, billPKs));
			var packingGroupPKs = new List<ZGuid>();

			foreach (var packingGroup in packGroups)
			{
				packingGroupPKs.Add(packingGroup.PK);
			}

			if (packingGroupPKs.Count > 0)
			{
				result.AddToFilter(CusDecHouseContainerPackSchema.CW_CR_HouseContainer, SQLComparisonOperator.Equal, packingGroupPKs);
			}
			else
			{
				result.IsNoResultQuery = true;
			}
			return result;
		}

		protected override bool AllowNewCore
		{
			get { return Declaration != null && !Declaration.ShouldSynchroniseWithShipment(); }
		}

		public bool HasDangerousGoods
		{
			get
			{
				foreach (TPackage package in this)
				{
					foreach (var dgItem in package.UNDGs)
					{
						if (dgItem.Substance != null)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		#region IPackingInformationCollection Members

		IPackingInformation IPackingInformationCollection.AddNew()
		{
			return AddNew();
		}

		IPackingInformation IPackingInformationCollection.GetElementWithNoContainer()
		{
			return GetElementWithNoContainer();
		}

		IPackingInformation IPackingInformationCollection.GetElementWithNoHouseBill()
		{
			return GetElementWithNoHouseBill();
		}

		IPackingInformation IPackingInformationCollection.GetElement(int index)
		{
			return this[index];
		}

		IPackingInformation IPackingInformationCollection.GetMatchingElement(Bill bill, BaseCusContainer container)
		{
			return GetMatchingElementForShipmentSynch(bill, container);
		}
		#endregion

		public IEnumerable<TPackage> LowestPackages
		{
			get
			{
				return this.Cast<TPackage>().Where(x => x.IsLowestPackage);
			}
		}

		public void ForEach(Action<TPackage> action)
		{
			foreach (var item in Elements)
			{
				action((TPackage)item);
			}
		}

		public TPackage FirstOrDefault()
		{
			if (Elements.Any())
			{
				return (TPackage)Elements[0];
			}
			else
			{
				return default(TPackage);
			}
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var package = (TPackage)child;
			using (child.GetValidationSuspender())
			{
				if (Count == 0)
				{
					SetDefaultsForFirstPackage(package);
				}
				else if (Count > 0)
				{
					var previousPackage = this[Count - 1];
					SetDefaultFromPreviousPackage(previousPackage, package);
				}
			}
		}

		protected virtual void SetDefaultsForFirstPackage(TPackage firstPackage)
		{
		}

		protected virtual void SetDefaultFromPreviousPackage(TPackage previousPackage, TPackage currentPackage)
		{
			var netWeightUQ = previousPackage.CW_NetWeightUQ;
			if (!netWeightUQ.IsEmpty)
			{
				currentPackage.CW_NetWeightUQ = netWeightUQ;
			}

			var grossWeightUQ = previousPackage.CW_GrossWeightUQ;
			if (!grossWeightUQ.IsEmpty)
			{
				currentPackage.CW_GrossWeightUQ = grossWeightUQ;
			}

			var volumeUQ = previousPackage.CW_VolumeUQ;
			if (!volumeUQ.IsEmpty)
			{
				currentPackage.CW_VolumeUQ = volumeUQ;
			}
		}

		public IEnumerator<TPackage> GetEnumerator() => Elements.Cast<TPackage>().GetEnumerator();
	}
}
