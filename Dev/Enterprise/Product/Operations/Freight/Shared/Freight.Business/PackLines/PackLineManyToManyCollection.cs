using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class PackLineManyToManyCollection : ManyToManyBusinessObjectCollection<PackLine, CommonContainer>, IPackLineCollection, IGoodsCollection
	{
		public PackLineManyToManyCollection(CommonContainer parentContainer)
			: base(parentContainer)
		{
			this.parentContainer = parentContainer;
		}

		protected readonly CommonContainer parentContainer;

		#region Find

		public new PackLine[] Find(ZQuery sQLFilter)
		{
			return (PackLine[])base.Find(sQLFilter);
		}

		#endregion

		#region Adding

		public override void Add(BusinessObject bizObj)
		{
			var packLine = (PackLine)bizObj;

			if (packLine.JL_FreightMode != FreightConstants.InnerPackType)
			{
				base.Add(packLine);
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

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override Type TypeOfRelationshipBusinessObject
		{
			get { return typeof(JobContainerPackPivot); }
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			if (suspendOnAdded > 0)
			{
				return;
			}

			if (!IsLoading && !IsUpdatingByDataRefreshBus)
			{
				var packLine = (PackLine)bizOAdded;

				if (packLine.JL_FreightMode != FreightConstants.InnerPackType)
				{
					ContainerPackLineRelationshipHelper.ReportAbsentConShipLink("PackLineManyToManyCollection.OnAdded()", packLine, parentContainer, Factory);

					packLine.Containers.AddWithSuspension(parentContainer);
					ContainerPackLineRelationshipHelper.PackLineAddedToContainer(parentContainer, packLine);

					parentContainer?.ContainerPenaltyCalculateHandlers?.ForEach(handler => handler.HandleContainerAllocation(packLine));
					packLine.Shipment?.OnPackLinePackedOrUnpacked(packLine, parentContainer, true);
				}
			}
		}

		int suspendOnAdded;

		#endregion

		#region Removing

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

			removeStackTrace.Append($"ForwardingPackLine with PK: {bizObj.PK} has been removed here: {System.Environment.StackTrace}\n");

			if (suspendOnRemoved > 0)
			{
				return;
			}

			if (!IsLoading && !IsRefreshingByDataRefreshBus)
			{
				var packLine = (PackLine)bizObj;
				packLine.Containers.RemoveWithSuspension(parentContainer.PK);
				ContainerPackLineRelationshipHelper.PackLineRemovedFromContainer(parentContainer, packLine);

				parentContainer?.ContainerPenaltyCalculateHandlers?.ForEach(handler => handler.HandleContainerDeallocation(packLine));
			}
		}

		int suspendOnRemoved;

		protected override void OnRemoving(BusinessObject bizO)
		{
			base.OnRemoving(bizO);
			PackLine packLine = (PackLine)bizO;
			if (!IsLoading && !IsRefreshingByDataRefreshBus && !parentContainer.IsRefreshingByDataRefreshBus && !packLine.IsDeleted && packLine.Shipment != null)
			{
				packLine.Shipment.DocsAndCartage.Services.SetServicesForContainerUnpack(parentContainer);
				packLine.Shipment?.OnPackLinePackedOrUnpacked(packLine, parentContainer, false);
			}
		}

		#endregion

		#region FilterByShipment

		/// <summary>
		/// Returns collection of Packlines on the parent Container for the specified Shipment.
		/// </summary>
		/// <param name="shipment"></param>
		/// <returns></returns>
		public OuterPackLineCollection FilterByShipment(CommonShipment shipment)
		{
			OuterPackLineCollection packLines = new OuterPackLineCollection(shipment, Factory);
			packLines.AddRange(Find(new ZQuery(JobPackLinesSchema.JL_JS, shipment.PK)));
			return packLines;
		}

		#endregion

		#region IPackLineCollection Members

		PackLineCollectionCalculator IPackLineCollection.Totals
		{
			get
			{
				if (fTotals == null)
				{
					fTotals = new PackLineCollectionCalculator(this, PackLineCollectionCalculator.FilterMode.RemoveColoadMasterPackLines);
				}
				return fTotals;
			}
		}
		PackLineCollectionCalculator fTotals;

		ZString IPackLineCollection.MasterPackagesUnit
		{
			get { return parentContainer.ParentPackageUnit; }
		}

		ZString IPackLineCollection.MasterWeightUnit
		{
			get { return parentContainer.ContainerWeightUnit; }
		}

		ZString IPackLineCollection.MasterVolumeUnit
		{
			get { return parentContainer.ParentVolumeUnit; }
		}

		#endregion

		protected override void OnLoaded()
		{
			base.OnLoaded();
			ContainerPackLineConcurrencyCheck.Register(Factory);
		}

		readonly ZStringBuilder removeStackTrace = new ZStringBuilder();

		public ZString RemoveStackTrace
		{
			get { return removeStackTrace.ToString(); }
		}
	}
}
