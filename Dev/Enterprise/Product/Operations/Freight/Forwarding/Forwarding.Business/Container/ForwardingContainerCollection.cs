using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.Forwarding.Business
{
	[ModuleID(Enterprise.ZArchitecture.Modules.ModuleId.Containers)]
	public class ForwardingContainerCollection : CommonContainerCollection, IForwardingContainerCollection
	{
		public ForwardingContainerCollection(ForwardingConsol consolParent, BusinessObjectFactory factory)
			: base(consolParent, factory)
		{
		}

		public ForwardingContainerCollection(JobSailing sailingParent, BusinessObjectFactory factory)
			: base(sailingParent, factory)
		{
		}

		public new ForwardingContainer this[int index]
		{
			get { return (ForwardingContainer)Elements[index]; }
		}

		public new ForwardingContainer AddNew()
		{
			return (ForwardingContainer)base.AddNew();
		}

		IForwardingContainer IForwardingContainerCollection.this[int index] => this[index];

		protected override TableColumn[] GetColumnsToFetchForEnumerate()
		{
			var columns = new[]
			{
				new TableColumn(CusEntryNumSchema.Constants.TableName, CusEntryNumSchema.Constants.CE_ParentID)
			};

			return base.GetColumnsToFetchForEnumerate().Concat(columns).ToArray();
		}

		protected override void OnRemoving(BusinessObject bizO)
		{
			if (bizO.IsInDatabase && !IsDeletingForDataRefresh)
			{
				if (((ForwardingContainer)bizO).ContainerParent is IContainerTrackingProvider containerParent)
				{
					new ContainerTrackingSubscriptionRequestedManager(containerParent) { ContainerWasRemoved = true }
						.UpdateIfNecessary();
				}

				if (bizO is ForwardingContainer container
					&& !container.JC_RCA_AllocationLine.IsEmpty)
				{
					container.Consol?.AllocationConsumptionLogger.TryCaptureInitialPersistedConsumption();
					container.Consol?.AllocationRouteContainerWeightLimitHelper.SetRequireApprovalEventCancellationCheck();
				}
			}

			base.OnRemoving(bizO);
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			var container = bizOAdded as ForwardingContainer;

			if (!IsLoading && container != null)
			{
				if (!IsSettingHasChangesSuspended)
				{
					container.ContainerPenaltyCalculateHandlers?.ForEach(handler => handler.HandleContainerAdded());
				}

				var parentAllocationLine = ParentConsol?.JK_RCA_AllocationLine ?? ZGuid.Empty;
				if (!parentAllocationLine.IsEmpty)
				{
					container.JC_RCA_AllocationLine = parentAllocationLine;
				}
			}
		}

		#region FindBoxListProvider

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new ForwardingContainerFindBoxListProvider(this); }
		}

		class ForwardingContainerFindBoxListProvider : FindBoxListProvider
		{
			public ForwardingContainerFindBoxListProvider(ForwardingContainerCollection collection)
				: base(collection)
			{
			}

			protected override string GetCodePropertyName(string code)
			{
				return ForwardingContainer.Schema.JC_ContainerNum;
			}
		}

		#endregion
	}
}
