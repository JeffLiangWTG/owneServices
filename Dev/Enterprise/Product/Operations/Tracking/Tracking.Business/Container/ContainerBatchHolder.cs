using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Business
{
	public class ContainerBatchHolder : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public abstract class Schema
		{
			public const string RequiredDeliveryDate = "RequiredDeliveryDate";
			public const string ConfirmedDeliveryDate = "ConfirmedDeliveryDate";
			public const string ActualDeliveryDate = "ActualDeliveryDate";
			public const string EstimatedDehireDate = "EstimatedDehireDate";
			public const string EmptyPickup = "EmptyPickup";
			public const string ActualDehireDate = "ActualDehireDate";
		}

		#endregion

		public ContainerBatchHolder(TrackingContainerStandaloneCollection containers)
			: base(containers.Factory)
		{
			this.containers = containers;
		}

		public ContainerBatchHolder(BusinessObjectFactory factory, params ZGuid[] containerPKs)
			: base(factory)
		{
			var lcontainers = new TrackingContainerStandaloneCollection(factory);
			lcontainers.Load(new ZQuery(JobContainerSchema.PK, containerPKs));
			containers = lcontainers;

			foreach (TrackingContainer container in containers)
			{
				var bizOWithNotification = container as IBizOChangesEmailNotification;
				if (bizOWithNotification != null)
				{
					new BusinessObjectChangesEmailNotifier(container);
				}
			}
		}

		#region Containers

		public TrackingContainerStandaloneCollection Containers
		{
			get { return containers; }
		}

		readonly TrackingContainerStandaloneCollection containers;

		#endregion

		#region Batch change Properties

		#region RequiredDeliveryDate

		public ZDateTime RequiredDeliveryDate
		{
			get { return requiredDeliveryDate; }
			set
			{
				requiredDeliveryDate = value;
				RequiredDeliveryDateInfo.RefreshBinding();
			}
		}
		ZDateTime requiredDeliveryDate;

		public ZPropertyInfo RequiredDeliveryDateInfo
		{
			get { return GetZPropertyInfo(Schema.RequiredDeliveryDate); }
		}

		#endregion

		#region ConfirmedDeliveryDate

		public ZDateTime ConfirmedDeliveryDate
		{
			get { return confirmedDeliveryDate; }
			set
			{
				confirmedDeliveryDate = value;
				ConfirmedDeliveryDateInfo.RefreshBinding();
			}
		}
		ZDateTime confirmedDeliveryDate;

		public ZPropertyInfo ConfirmedDeliveryDateInfo
		{
			get { return GetZPropertyInfo(Schema.ConfirmedDeliveryDate); }
		}

		#endregion

		#region ActualDeliveryDate

		public ZDateTime ActualDeliveryDate
		{
			get { return actualDeliveryDate; }
			set
			{
				actualDeliveryDate = value;
				ActualDeliveryDateInfo.RefreshBinding();
			}
		}
		ZDateTime actualDeliveryDate;

		public ZPropertyInfo ActualDeliveryDateInfo
		{
			get { return GetZPropertyInfo(Schema.ActualDeliveryDate); }
		}

		#endregion

		#region EstimatedDehireDate

		public ZDateTime EstimatedDehireDate
		{
			get { return estimatedDehireDate; }
			set
			{
				estimatedDehireDate = value;
				EstimatedDehireDateInfo.RefreshBinding();
			}
		}
		ZDateTime estimatedDehireDate;

		public ZPropertyInfo EstimatedDehireDateInfo
		{
			get { return GetZPropertyInfo(Schema.EstimatedDehireDate); }
		}

		#endregion

		#region PickupDate

		public ZDateTime EmptyPickup
		{
			get { return emptyPickup; }
			set
			{
				emptyPickup = value;
				EmptyPickupInfo.RefreshBinding();
			}
		}
		ZDateTime emptyPickup;

		public ZPropertyInfo EmptyPickupInfo
		{
			get { return GetZPropertyInfo(Schema.EmptyPickup); }
		}

		#endregion

		#region ActualDehireDate

		public ZDateTime ActualDehireDate
		{
			get { return actualDehireDate; }
			set
			{
				actualDehireDate = value;
				ActualDehireDateInfo.RefreshBinding();
			}
		}
		ZDateTime actualDehireDate;

		public ZPropertyInfo ActualDehireDateInfo
		{
			get { return GetZPropertyInfo(Schema.ActualDehireDate); }
		}

		#endregion

		#endregion

		#region Apply

		public void Apply()
		{
			foreach (TrackingContainer container in Containers)
			{
				if (RequiredDeliveryDate.IsValid && !RequiredDeliveryDate.IsEmpty)
				{
					container.RequiredDelivery = RequiredDeliveryDate;
				}

				if (ConfirmedDeliveryDate.IsValid && !ConfirmedDeliveryDate.IsEmpty)
				{
					container.ConfirmedDelivery = ConfirmedDeliveryDate;
				}

				if (ActualDeliveryDate.IsValid && !ActualDeliveryDate.IsEmpty)
				{
					container.ActualDelivery = ActualDeliveryDate;
				}

				if (EstimatedDehireDate.IsValid && !EstimatedDehireDate.IsEmpty)
				{
					container.EmptyReady = EstimatedDehireDate;
				}

				if (EmptyPickup.IsValid && !EmptyPickup.IsEmpty)
				{
					container.EmptyPickup = EmptyPickup;
				}

				if (ActualDehireDate.IsValid && !ActualDehireDate.IsEmpty)
				{
					container.ActualDehire = ActualDehireDate;
				}

				container.RunPreSaveValidation();
			}
		}

		#endregion
	}
}
