using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US.AMS;
using Enterprise.Customs.US.AMS.Messaging.Business.StowPlan;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class StowPlanSailingData : AutoStowPlanSailingData, IStowPlanSailingData, IStowPlanNotificationProvider, INotificationProvider, IDisposable
	{
		public StowPlanSailingData(JobVoyage voyage)
			: base(voyage.Factory)
		{
			this.voyage = voyage;
			if (Lookups.USPorts.Count == 1)
			{
				this.Arrival = ((ICodeDescription)Lookups.USPorts[0]).Code;
			}
			InitializeVessel();
		}
		internal readonly JobVoyage voyage;

		#region Override Properties

		public override ZString VoyageNumber
		{
			get { return voyage.JV_VoyageFlight; }
		}

		[List(nameof(Lookups) + "." + nameof(StowPlanSailingDataLookups.USPorts))]
		public override ZString Arrival
		{
			get { return base.Arrival; }
			set
			{
				var oldValue = Arrival;
				base.Arrival = value;
				var arrivalVoyagePort = ArrivalVoyagePort;
				if (arrivalVoyagePort != null)
				{
					this.ArrivalTime = arrivalVoyagePort.ArrivalTime;
					this.IsArrivalTimeEstimated = true;
					var departureVoyagePort = GetDepartureVoyagePort(arrivalVoyagePort);
					if (departureVoyagePort != null)
					{
						this.Departure = departureVoyagePort.Port;
						this.DepartureTime = departureVoyagePort.DepartureTime;
						this.IsDepartureTimeEstimated = true;
					}
					else
					{
						this.Departure = ZString.Empty;
						this.DepartureTime = ZDateTime.Empty;
						this.IsDepartureTimeEstimated = false;
					}
				}
				if (oldValue != Arrival)
				{
					OnEventsThatIssuesNeedRebuilding(this, EventArgs.Empty);
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(StowPlanSailingDataLookups.ForeignPorts))]
		public override ZString Departure
		{
			get { return base.Departure; }
			set { base.Departure = value; }
		}

		[List(nameof(Lookups) + "." + nameof(StowPlanSailingDataLookups.IssueTypes))]
		public override ZString IssueFilter
		{
			get { return base.IssueFilter; }
			set { base.IssueFilter = value; }
		}

		#endregion

		#region IStowPlanSailingData members

		IEnumerable<IStowPlanShipmentData> IStowPlanSailingData.Shipments
		{
			get { return Shipments.Where(x => x.Checked).Cast<IStowPlanShipmentData>(); }
		}

		EDIMessageCollection IEDIMessageCollectionProvider.Messages
		{
			get
			{
				var arrivalEndPoint = ArrivalVoyagePort;
				return arrivalEndPoint != null ? arrivalEndPoint.Messages : null;
			}
		}

		IStowPlanVesselData IStowPlanSailingData.Vessel
		{
			get { return Vessel; }
		}

		StowPlanVesselData Vessel
		{
			get
			{
				InitializeVessel();
				return fVessel;
			}
		}
		StowPlanVesselData fVessel;

		void InitializeVessel()
		{
			var vessel = voyage.Vessel;
			if (fVessel == null && vessel != null)
			{
				fVessel = new StowPlanVesselData(vessel);
				RegisterEditableChildObject(fVessel);
			}
		}

		#region Shipments

		public StowPlanDataBusinessObjectCollection<StowPlanShipmentData> Shipments
		{
			get
			{
				if (fShipments == null)
				{
					fShipments = new StowPlanDataBusinessObjectCollection<StowPlanShipmentData>(Factory);
					RegisterEditableChildObject(fShipments);
					RebuildShipmentsCollection();
				}
				return fShipments;
			}
		}
		StowPlanDataBusinessObjectCollection<StowPlanShipmentData> fShipments;

		public void RebuildShipmentsCollection()
		{
			if (fShipments != null)
			{
				var visitingPortBills = Enumerable.Empty<BillOfLading>();
				var arrivalPort = ArrivalVoyagePort;
				if (arrivalPort != null)
				{
					visitingPortBills = new BillsOfLadingAtPortStrategy(voyage).GetBillsOnVesselAt(arrivalPort.ArrivalTime, false);
				}

				using (fShipments.SuspendListChanged())
				{
					fShipments.SuspendValidation();
					fShipments.RemoveAll();
					fShipments.AddRange(visitingPortBills.Where(x => x.JS_PackingMode == Core.Constants.ContainerModes.FCL).Select(x => GetOrCreate(x)));
					fShipments.ResumeValidation();
				}
			}
		}

		StowPlanShipmentData GetOrCreate(BillOfLading shipment)
		{
			StowPlanShipmentData result = null;
			if (!shipmentCache.TryGetValue(shipment.PK, out result))
			{
				result = new StowPlanShipmentData(shipment);
				shipmentCache.Add(shipment.PK, result);
			}
			return result;
		}

		readonly IDictionary<ZGuid, StowPlanShipmentData> shipmentCache = new Dictionary<ZGuid, StowPlanShipmentData>();

		#endregion

		#endregion

		#region INotificationProvider members

		IEnumerable<INotification> INotificationProvider.Notifications
		{
			get { return NotificationsIncludingChildren; }
		}

		public new IEnumerable<INotification> NotificationsIncludingChildren
		{
			get { return new StowPlanNotificationCollector(this, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName); }
		}

		public new IEnumerable<INotification> Notifications
		{
			get { return new StowPlanNotificationCollector(this, false, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName); }
		}

		#endregion

		#region IStowPlanNotificationProvider members

		Guid IStowPlanNotificationProvider.TargetPK
		{
			get { return Guid.Empty; }
		}

		string IStowPlanNotificationProvider.TargetCode
		{
			get { return ZString.Empty; }
		}

		string IStowPlanNotificationProvider.TargetSubject
		{
			get { return voyage.HumanReadableName; }
		}

		#endregion

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			this.IssueFilter = STWIssueFilterList.Codes.MessageErrorOnly;
		}

		#endregion

		#region Issues

		public void RebuidIssues()
		{
			this.Validation.ValidateAll();
			if (!this.HasErrors && issuesNeedRebuilding)
			{
				issuesNeedRebuilding = false;
				this.RunPreSaveValidation();
				IssueCollection.RemoveAndDeleteAll();
				foreach (StowPlanNotification notification in this.NotificationsIncludingChildren)
				{
					IssueCollection.AddNew(notification.TargetPK, notification.TargetCode, notification.Subject, notification.Message, notification.Detail, notification.Type);
				}
			}
		}

		#endregion

		#region New Properties

		public bool HasMessageErrorIssues
		{
			get { return IssueCollection.Cast<StowPlanMessageIssue>().Any(x => x.notificationType == CargoWise.EntityFramework.NotificationType.MessageError); }
		}

		internal IEnumerable<VoyagePort> VoyagePorts
		{
			get
			{
				if (fVoyagePorts == null)
				{
					fVoyagePorts = new VoyagePortCollection(voyage);
				}
				return fVoyagePorts.Cast<VoyagePort>();
			}
		}
		VoyagePortCollection fVoyagePorts;

		VoyagePort ArrivalVoyagePort
		{
			get { return VoyagePorts.FirstOrDefault(x => x.Port == this.Arrival); }
		}

		VoyagePort GetDepartureVoyagePort(VoyagePort arrivalVoyagePort)
		{
			return VoyagePorts.Where(x => !IsUSOrPR(x.Port)).OrderByDescending(x => x.DepartureTime).FirstOrDefault(x => x.DepartureTime < arrivalVoyagePort.ArrivalTime);
		}

		bool IsUSOrPR(ZString port)
		{
			return port.StartsWith(Core.Constants.CountryCodes.UnitedStates) || port.StartsWith(Core.Constants.CountryCodes.PuertoRico);
		}

		public StowPlanMessageIssueCollectionView IssueCollectionView
		{
			get { return fIssueCollectionView ?? (fIssueCollectionView = new StowPlanMessageIssueCollectionView(IssueCollection, this)); }
		}
		StowPlanMessageIssueCollectionView fIssueCollectionView;

		public StowPlanMessageIssueCollection IssueCollection
		{
			get { return fIssueCollection ?? (fIssueCollection = new StowPlanMessageIssueCollection()); }
		}
		StowPlanMessageIssueCollection fIssueCollection;

		public StowPlanSailingDataLookups Lookups
		{
			get { return fLookups ?? (fLookups = new StowPlanSailingDataLookups(this)); }
		}
		StowPlanSailingDataLookups fLookups;

		#endregion

		public void OnEventsThatIssuesNeedRebuilding(object sender, EventArgs e)
		{
			RebuildShipmentsCollection();
			issuesNeedRebuilding = true;
		}
		bool issuesNeedRebuilding = true;

		public void CreateStowPlanMessage()
		{
			var arrivalEndPoint = ArrivalVoyagePort;
			if (arrivalEndPoint != null)
			{
				new StowPlanBaplie211MessageBuilder(this).PopulateMessages();
				arrivalEndPoint.MessageStatusCode = MessageStatusListSTW.Codes.Sending;
			}
		}

		#region Dispoable

		void IDisposable.Dispose()
		{
			if (fVoyagePorts != null)
			{
				fVoyagePorts.Dispose();
			}
		}

		#endregion
	}
}
