using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.Business
{
	public interface ICusContainerCollection<out TCusContainer> : IBusinessObjectCollection<TCusContainer>
		where TCusContainer : BaseCusContainer
	{
		BaseJobDeclaration Declaration { get; }
		ZDecimal TotalGoodsWeight { get; }
		IEnumerable<ZString> ContainerNumbers { get; }
		ContainerNonDependentCollection JobContainers { get; }
		BaseCusContainer GetElementWithoutPackingGroups();
		bool HasContainerModeOf(string mode);
		bool HasContainerOfType(string containerMode);
		TCusContainer Find(ZString containerNumber);
		TCusContainer FindOrCreate(ZString containerNumber);
		IDisposable SuspendCusContainerListChanged();
		new TCusContainer this[int index] { get; }
	}

	public class BaseCusContainerCollection<TCusContainer> : DependentBusinessObjectCollection<TCusContainer, BaseJobDeclaration>, ICusContainerCollection<TCusContainer>
		where TCusContainer : BaseCusContainer
	{
		public BaseCusContainerCollection(BaseJobDeclaration jobDeclaration, BusinessObjectFactory factory) : base(jobDeclaration, factory)
		{
			Declaration = jobDeclaration;
		}

		public BaseCusContainerCollection(BaseJobDeclaration jobDeclaration, ZQuery filter) : base(jobDeclaration, filter)
		{
			Declaration = jobDeclaration;
		}

		public BaseJobDeclaration Declaration { get; }

		public IEnumerator<TCusContainer> GetEnumerator() => Elements.Cast<TCusContainer>().GetEnumerator();

		public IDisposable SuspendCusContainerListChanged()
		{
			return SuspendListChanged();
		}

		public BaseCusContainer GetElementWithoutPackingGroups()
			=> this.FirstOrDefault<TCusContainer>(container => container.PackingGroups.Count == 0);

		public void DoDefaultSort()
		{
			this.Sort(new DefaultComparer());
		}

		class DefaultComparer : IComparer
		{
			#region IComparer Members
			public int Compare(object x, object y)
			{
				BaseCusContainer containerX = (BaseCusContainer)x;
				BaseCusContainer containerY = (BaseCusContainer)y;
				int result = containerX.CO_ContainerNumber.CompareTo(containerY.CO_ContainerNumber);
				if (result == 0)
				{
					result = containerX.CO_FCL_LCL_AIR.CompareTo(containerY.CO_FCL_LCL_AIR);
				}
				return result;
			}

			#endregion
		}

		public bool HasContainerOfType(string containerMode) => this.Any<TCusContainer>(container => container.CO_FCL_LCL_AIR == containerMode);

		public TCusContainer Find(ZString containerNumber)
			=> this.FirstOrDefault<TCusContainer>((container => container.CO_ContainerNumber == containerNumber));

		public bool HasContainerModeOf(string mode) => this.Any<TCusContainer>(container => container.CO_FCL_LCL_AIR == mode);

		public IEnumerable<ZString> ContainerNumbers => this.Select<TCusContainer, ZString>(container => container.CO_ContainerNumber);

		public ZDecimal TotalGoodsWeight => this.Sum<TCusContainer>(container => container.CO_Weight);

		public TCusContainer FindOrCreate(ZString containerNumber)
		{
			var result = Find(containerNumber);

			if (result == null)
			{
				result = AddNew();
				result.CO_ContainerNumber = containerNumber;
			}

			return result;
		}

		protected override bool EnableRemovingDependentWithoutDeletingErrorReport { get { return true; } }

		public ContainerNonDependentCollection JobContainers
		{
			get
			{
				var result = new ContainerNonDependentCollection(Factory);
				result.AddRange(this.Select<TCusContainer, CommonContainer>(container => container.JobContainer));
				return result;
			}
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			if (!bizOAdded.IsInDatabase && !Declaration.IsContainerised)
			{
				Declaration.JE_ContainerMode = Declaration.GetDefaultContainerisedContainerMode();
			}
		}

		protected override void OnRemoving(BusinessObject bizO)
		{
			if (Declaration.IsStandAlone)
			{
				new ContainerTrackingSubscriptionRequestedManager(Declaration) { ContainerWasRemoved = true }
					.UpdateIfNecessary();
			}

			base.OnRemoving(bizO);
		}

		public override bool ReadOnly => base.ReadOnly || Declaration.ShouldSynchroniseWithShipment();

		#region Cloning

		public void Clone(BaseCusContainerCollection<TCusContainer> collectionToClone)
		{
			RemoveAndDeleteAll();
			foreach (var container in collectionToClone)
			{
				Add(container.Clone());
			}
		}

		#endregion
	}
}
