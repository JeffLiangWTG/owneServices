using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public class CommonContainerManyToManyCollection : ManyToManyBusinessObjectCollection<CommonContainer, PackLine>
	{
		public CommonContainerManyToManyCollection(PackLine parentPackLine)
			: base(parentPackLine)
		{
			this.parentPackLine = parentPackLine;
		}

		protected readonly PackLine parentPackLine;
		public PackLine ParentPackLine => parentPackLine;

		protected override Type TypeOfRelationshipBusinessObject
		{
			get { return typeof(JobContainerPackPivot); }
		}

		public override void Add(BusinessObject bizObj)
		{
			if (!parentPackLine.IsDeleted && parentPackLine.JL_FreightMode != FreightConstants.InnerPackType)
			{
				base.Add(bizObj);
			}
		}

		internal void AddWithSuspension(BusinessObject bizObj)
		{
			suspendOnAdded++;

			try
			{
				Add(bizObj);
			}
			finally
			{
				suspendOnAdded--;
			}
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			if (suspendOnAdded > 0)
			{
				return;
			}

			if (!IsLoading && !IsUpdatingByDataRefreshBus && parentPackLine.JL_FreightMode != FreightConstants.InnerPackType)
			{
				var container = (CommonContainer)bizOAdded;

				ContainerPackLineRelationshipHelper.ReportAbsentConShipLink("CommonContainerManyToManyCollection.OnAdded()", parentPackLine, container, Factory);

				container.PackLines.AddWithSuspension(parentPackLine);
				ContainerPackLineRelationshipHelper.PackLineAddedToContainer(container, parentPackLine);

				container?.ContainerPenaltyCalculateHandlers?.ForEach(handler => handler.HandleContainerAllocation(parentPackLine));
			}
		}

		int suspendOnAdded;

		internal void RemoveWithSuspension(ZGuid pk)
		{
			suspendOnRemoved++;

			try
			{
				Remove(pk);
			}
			finally
			{
				suspendOnRemoved--;
			}
		}

		protected override void OnRemoved(BusinessObject bizObj)
		{
			base.OnRemoved(bizObj);

			if (suspendOnRemoved > 0)
			{
				return;
			}

			if (!IsLoading && !IsRefreshingByDataRefreshBus)
			{
				var container = (CommonContainer)bizObj;

				container.PackLines.RemoveWithSuspension(parentPackLine.PK);
				ContainerPackLineRelationshipHelper.PackLineRemovedFromContainer(container, parentPackLine);

				container?.ContainerPenaltyCalculateHandlers?.ForEach(handler => handler.HandleContainerDeallocation(parentPackLine));
			}
		}

		int suspendOnRemoved;

		public HashedBizOList ContainerElements
		{
			get { return Elements; }
		}

		protected override void OnLoaded()
		{
			base.OnLoaded();
			ContainerPackLineConcurrencyCheck.Register(Factory);
		}
	}
}
