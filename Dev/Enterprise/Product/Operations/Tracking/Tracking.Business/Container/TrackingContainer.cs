using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Shared;
using Constants = Enterprise.Core.Constants;
using ContainerModes = Enterprise.Core.Constants.ContainerModes;
using StatusList = Enterprise.Tracking.Business.ContainerStatusList.Codes;

namespace Enterprise.Tracking.Business
{
	public class TrackingContainer : ForwardingContainer,
		IWebDocumentsWithUploadSupport,
		IBizOChangesEmailNotification,
		IWebUserEditableNoteSupport,
		IUpdatableMilestoneEventsProvider,
		IMilestonesProvider,
		ITrackingEventsProvider,
		IEventReferenceProvider
	{
		#region Schema

		[SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Follow same inheritance as the containing class")]
		public new abstract class Schema : ForwardingContainer.Schema
		{
			public const string JC_TareWeightWithSuppression = "JC_TareWeightWithSuppression";
			public const string JC_TotalWeightWithSuppression = "JC_TotalWeightWithSuppression";
			public const string Shipments = "Shipments";
			public const string ShipmentNumbers = "ShipmentNumbers";
			public const string ShipmentStatuses = "ShipmentStatuses";
			public const string ContainerNumber = "ContainerNumber";
			public const string Type = "Type";
			public const string TypeDescription = "TypeDescription";
			public const string Mode = "Mode";
			public const string Packs = "Packs";
			public const string Arrival = "Arrival";
			internal const string ETA = "ETA";
			internal const string ATA = "ATA";
			public const string Departure = "Departure";
			internal const string ETD = "ETD";
			internal const string ATD = "ATD";
			public const string Status = "Status";
			public const string StatusDescription = "StatusDescription";
			public const string QuarantineCode = "QuarantineCode";
			public const string Available = "Available";
			public const string EmptyReturnRequired = "EmptyReturnRequired";
			public const string SlotDate = "SlotDate";
			public const string RequiredDelivery = "RequiredDelivery";
			public const string RequiredDeliveryStatus = "RequiredDeliveryStatus";
			public const string ConfirmedDelivery = "ConfirmedDelivery";
			public const string ConfirmedDeliveryStatus = "ConfirmedDeliveryStatus";
			public const string ActualDelivery = "ActualDelivery";
			public const string ActualDeliveryStatus = "ActualDeliveryStatus";
			public const string Consignees = "Consignees";
			public const string ConsigneesExtended = "ConsigneesExtended";
			public const string ConsignorsExtended = "ConsignorsExtended";
			public const string EmptyReady = "EmptyReady";
			public const string EmptyReadyStatus = "EmptyReadyStatus";
			public const string EmptyPickup = "EmptyPickup";
			public const string ActualDehire = "ActualDehire";
			public const string ActualDehireStatus = "ActualDehireStatus";
			public const string ConsolNumber = "ConsolNumber";
			public const string MasterBillNumber = "MasterBillNumber";
			public const string Weight = "Weight";
			public const string WeightUQ = "WeightUQ";
			public const string VesselName = "VesselName";
			public const string Voyage = "Voyage";
			public const string PortOfDischarge = "PortOfDischarge";
			public const string PortOfLoading = "PortOfLoading";
			public const string ContainerStatus = "ContainerStatus";
			public const string StorageBegins = "StorageBegins";
			public const string WeightWithUnits = "WeightWithUnits";
			public const string VerifiedByCompany = "VerifiedByCompany";
			public const string VerifiedByPerson = "VerifiedByPerson";
			public const string VerifiedByPhone = "VerifiedByPhone";
			public const string VerifiedByEmail = "VerifiedByEmail";
			public const string VerifiedMethod = "VerifiedMethod";
		}

		#endregion

		public TrackingContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			if (SiteUser != null)
			{
				Logs.AutoCreatedLogDefaultSL_Reference = SiteUser.ContactAndCompanyReference;
			}
		}

		public static TrackingContainer FromPKFilteredBySiteUser(BusinessObjectFactory factory, ZGuid containerPK, TrackingSiteUser siteUser)
		{
			TrackingContainer container = null;

			if (siteUser != null && siteUser.LoggedInOrganisation != null)
			{
				ZDBOnlyQuery filter = new ZDBOnlyQuery(typeof(TrackingContainer));
				if (!siteUser.IsShipmentQuickViewUser) // We use it to enable direct view of Container Details by Container Number
				{
					filter.AddToFilter(OrgRestrictionFilterFactory.Instance.GetFilter<TrackingContainer>());
				}
				filter.AddToFilter(JobContainerSchema.PK, containerPK);
				filter.IgnoreActiveFilter = true;
				container = factory.LoadTop1<TrackingContainer>(filter);

				if (container != null)
				{
					container.SiteUser = siteUser;
				}
			}

			return container;
		}

		#region Weight Suppression

		public ZString JC_TareWeightWithSuppression
		{
			get { return SuppressUtil.GetSuppressedValue(JC_TareWeight.ToString(), !ShowWeightDetails); }
		}

		public ZPropertyInfo JC_TareWeightWithSuppressionInfo
		{
			get { return GetZPropertyInfo(Schema.JC_TareWeightWithSuppression); }
		}

		public ZString JC_TotalWeightWithSuppression
		{
			get { return SuppressUtil.GetSuppressedValue(JC_Calc_TotalWeight.ToString(), !ShowWeightDetails); }
		}

		public ZPropertyInfo JC_TotalWeightWithSuppressionInfo
		{
			get { return GetZPropertyInfo(Schema.JC_TotalWeightWithSuppression); }
		}

		bool ShowWeightDetails
		{
			get
			{
				return ContainerModeForBinding == ContainerModes.FCL
				|| ContainerModeForBinding == ContainerModes.BuyersConsol;
			}
		}

		#endregion

		#region Properties

		#region Container_List

		public RefContainerCollection Container_List
		{
			get { return ListProvider != null ? ListProvider.Container_List : new ContainerHelper(Factory).List(string.Empty); }
		}

		#endregion

		#region ListProvider

		public IContainerListProvider ListProvider { get; set; }

		#endregion

		#region Container Number

		public ZString ContainerNumber
		{
			get { return JC_ContainerNum; }
		}

		public ZPropertyInfo ContainerNumberInfo => GetZPropertyInfo(Schema.ContainerNumber);

		#endregion

		#region Orders

		public TrackingLegacyOrderCollection Orders
		{
			get
			{
				if (orders == null)
				{
					orders = new TrackingLegacyOrderCollection(Factory);

					foreach (TrackingShipment shipment in Shipments)
					{
						orders.AddRange(shipment.AttachedOrders);
					}

					var mainFilter = new ZDBOnlyQuery(typeof(Order));
					var restrictionFilter = OrgRestrictionFilterFactory.Instance.GetFilter<TrackingOrder>();
					var orderLineFilter = new ZDBOnlySubQuery(typeof(OrderLine), JobOrderLineSchema.JO_JD);
					var orderContainerFilter = new ZDBOnlySubQuery(typeof(OrderContainer), JobOrderContainerSchema.J1_ParentID);
					orderContainerFilter.AddToFilter(JobOrderContainerSchema.J1_ParentTableCode, JobOrderHeaderSchema.Constants.Prefix);

					orderLineFilter.AddToFilter(new ZQuery(JobOrderLineSchema.JO_ContainerNumber, this.JC_ContainerNum), JoinCondition.And);
					orderContainerFilter.AddToFilter(new ZQuery(JobOrderContainerSchema.J1_ContainerNumber, this.JC_ContainerNum), JoinCondition.And);

					mainFilter.AddSubQuery(orderLineFilter, JoinCondition.And);
					mainFilter.AddSubQuery(orderContainerFilter, JoinCondition.And);
					mainFilter.AddToFilter(restrictionFilter, JoinCondition.And);

					OrderCollection collection = new OrderCollection(Factory);
					collection.AdditionalFilter = mainFilter;

					foreach (var order in collection)
					{
						if (!orders.Contains(order.PK))
						{
							orders.Add(order);
						}
					}
				}

				return orders;
			}
		}

		TrackingLegacyOrderCollection orders;

		#endregion

		#region OrderLines

		public TrackingOrderLineCollection OrderLines
		{
			get
			{
				if (orderLines == null)
				{
					orderLines = new TrackingOrderLineCollection(Factory);

					foreach (Order order in Orders)
					{
						if (order.BuyerPK == CurrentOrg || order.SupplierPK == CurrentOrg || order.JD_OH_Carrier == CurrentOrg)
						{
							foreach (OrderLine orderLine in order.OrderLines)
							{
								bool hasContainer = orderLine.JO_ContainerNumber == ContainerNumber;

								if (!hasContainer)
								{
									foreach (OrderLineDelivery delivery in orderLine.Deliveries)
									{
										foreach (OrderLineDeliverContainer container in delivery.Containers)
										{
											hasContainer = container.J5_ContainerNum == ContainerNumber;
											if (hasContainer)
											{
												break;
											}
										}
										if (hasContainer)
										{
											break;
										}
									}
								}

								if (hasContainer)
								{
									TrackingOrderLine line = Factory.Load<TrackingOrderLine>(orderLine.PK);
									line.SetContainerNumber(ContainerNumber);
									OrderLines.Add(line);
								}
							}
						}
					}
				}

				return orderLines;
			}
		}

		TrackingOrderLineCollection orderLines;

		#endregion

		#region Shipments

		public TrackingShipmentCollection Shipments
		{
			get
			{
				if (shipments == null)
				{
					shipments = new TrackingShipmentCollection(Factory);
					List<ZGuid> shipmentPKs = GetShipmentPKsFromPackLines();

					if (shipmentPKs.Count > 0 && !CurrentOrg.IsEmpty)
					{
						ZQuery filter = new ZQuery(JobShipmentSchema.PK, shipmentPKs);
						filter.AddToFilter(OrgRestrictionFilterFactory.Instance.GetFilter<TrackingShipment>());
						shipments.Load(filter);
					}
				}

				return shipments;
			}
		}
		TrackingShipmentCollection shipments;

#if DEBUG
		virtual
#endif
 protected List<ZGuid> GetShipmentPKsFromPackLines()
		{
			List<ZGuid> shipmentPKs = new List<ZGuid>();

			foreach (PackLine pack in PackLines)
			{
				if (pack.Shipment != null)
				{
					shipmentPKs.Add(pack.Shipment.PK);
				}
			}

			return shipmentPKs;
		}

		#endregion

		#region Type

		public ZString Type
		{
			get { return Container != null ? Container.RC_Code : ZString.Empty; }
		}

		public ZPropertyInfo TypeInfo => GetZPropertyInfo(Schema.Type);

		public ZString TypeDescription
		{
			get { return Container != null ? Container.RC_DescriptionMultilingual : ZString.Empty; }
		}

		public ZPropertyInfo TypeDescriptionInfo => GetZPropertyInfo(Schema.TypeDescription);

		#endregion

		#region Mode

		public ZString Mode
		{
			get { return ContainerModeForBinding; }
		}

		public ZString ModeDescription
		{
			get { return Container != null ? Container.RC_DescriptionMultilingual : ZString.Empty; }
		}

		public ZPropertyInfo ModeInfo => GetZPropertyInfo(Schema.Mode);

		#endregion

		#region VerifiedMethod

		public ZString VerifiedMethod
		{
			get { return Lookups.GrossWeightVerificationTypeList.GetDescriptionFromCode(JC_GrossWeightVerificationType); }
		}

		public ZPropertyInfo VerifiedMethodInfo => GetZPropertyInfo(Schema.VerifiedMethod);

		#endregion

		#region VerifiedByCompany

		public ZString VerifiedByCompany
		{
			get { return GrossWeightVerifiedByAddress.E2_CompanyName; }
		}

		public ZPropertyInfo VerifiedByCompanyInfo => GetZPropertyInfo(Schema.VerifiedByCompany);

		#endregion

		#region VerifiedByPerson

		public ZString VerifiedByPerson
		{
			get { return GrossWeightVerifiedByAddress.E2_Contact; }
		}

		public ZPropertyInfo VerifiedByPersonInfo => GetZPropertyInfo(Schema.VerifiedByPerson);

		#endregion

		#region VerifiedByPhone

		public ZString VerifiedByPhone
		{
			get { return GrossWeightVerifiedByAddress.E2_Phone; }
		}

		public ZPropertyInfo VerifiedByPhoneInfo => GetZPropertyInfo(Schema.VerifiedByPhone);

		#endregion

		#region VerifiedByEmail

		public ZString VerifiedByEmail
		{
			get { return GrossWeightVerifiedByAddress.E2_Email; }
		}

		public ZPropertyInfo VerifiedByEmailInfo => GetZPropertyInfo(Schema.VerifiedByEmail);

		#endregion

		#region Packs

		public ZInt Packs
		{
			get { return JC_Calc_TotalPackages; }
		}

		public ZPropertyInfo PacksInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.Packs, x => JC_Calc_TotalPackagesInfo); }
		}

		#endregion

		#region Port Of Discharge

		/// <summary>
		/// Port container was loaded
		/// </summary>
		public ZString PortOfDischarge
		{
			get
			{
				ZString result = ZString.Empty;
				if (Consol != null && Consol.DischargePort != null)
				{
					result = Consol.DischargePort.Description;
				}
				else if (Declaration != null && Declaration.PortOfArrival != null)
				{
					result = Declaration.PortOfArrival.Description;
				}
				return result;
			}
		}

		public ZPropertyInfo PortOfDischargeInfo
		{
			get { return GetZPropertyInfo(Schema.PortOfDischarge); }
		}

		#endregion

		#region Arrival

		public ZDateTime Arrival
		{
			get
			{
				ZDateTime result = ATA.IsEmpty ? ETA : ATA;
				if (result == ZDateTime.Empty && CustomsContainer != null)
				{
					result = CustomsContainer.Declaration.JE_DateOfArrival.IsEmpty ? CustomsContainer.Declaration.JE_DateOfFirstArrival : CustomsContainer.Declaration.JE_DateOfArrival;
				}
				return result;
			}
		}

		public ZPropertyInfo ArrivalInfo => GetZPropertyInfo(Schema.Arrival);

		public ZDateTime ETA
		{
			get { return Consol != null ? Consol.JK_JX_JB_E_ARV : ZDateTime.Empty; }
		}

		ZDateTime ATA
		{
			get { return Consol != null ? Consol.JK_JX_JB_A_ARV : ZDateTime.Empty; }
		}

		#endregion

		#region Departure

		public ZDateTime Departure
		{
			get
			{
				ZDateTime result = ATD.IsEmpty ? ETD : ATD;
				if (result.IsEmpty && CustomsContainer != null)
				{
					result = CustomsContainer.Declaration.JE_ExportDate;
				}
				return result;
			}
		}

		public ZPropertyInfo DepartureInfo => GetZPropertyInfo(Schema.Departure);

		ZDateTime ETD
		{
			get { return Consol != null ? Consol.JK_JX_JA_E_DEP : ZDateTime.Empty; }
		}

		ZDateTime ATD
		{
			get { return Consol != null ? Consol.JK_JX_JA_A_DEP : ZDateTime.Empty; }
		}

		#endregion

		#region Status

		bool DateHasPassed(ZDateTime datetime)
		{
			return !datetime.IsEmpty && datetime <= Now;
		}

		ZDateTime Now
		{
			get
			{
				if (now.IsEmpty)
				{
					now = ZDateTime.Today.AddDays(1).AddMinutes(-1); // End of Today
				}

				return now;
			}
		}
		ZDateTime now;

		public ZString Status
		{
			get
			{
				if (status.IsEmpty)
				{
					bool hasDeparted = DateHasPassed(ATD);
					bool hasArrived = hasDeparted && DateHasPassed(ATA);
					bool onWharf = hasArrived && !DateHasPassed(Available);
					bool isAvailable = hasArrived && DateHasPassed(Available);
					bool isDelivered = DateHasPassed(ActualDelivery);
					bool hasEmptyReady = DateHasPassed(EmptyReady);
					bool hasExceededFreeDaysOnWharf = DateHasPassed(JC_LastFreeDay);
					bool detentionHasStarted = DateHasPassed(EmptyReturnRequired);
					bool isDehired = DateHasPassed(ActualDehire);
					bool isPickedUp = DateHasPassed(EmptyPickup);

					if (isDehired)
					{
						status = StatusList.Dehired;
					}
					else if (!hasDeparted)
					{
						status = StatusList.OnOrder;
					}
					else if (!hasArrived)
					{
						status = StatusList.Shipped;
					}
					else if (!isAvailable)
					{
						status = StatusList.OnWharf;
					}
					else if (!isDelivered)
					{
						status = hasExceededFreeDaysOnWharf ? StatusList.OnStorage : StatusList.Available;
					}
					else if (hasEmptyReady)
					{
						status = detentionHasStarted ? StatusList.OnDetention : StatusList.InDepot;
					}
					else if (!isPickedUp)
					{
						status = detentionHasStarted ? StatusList.OnDetention : StatusList.ForDehire;
					}
				}

				return status;
			}
		}
		ZString status;

		public ZPropertyInfo StatusInfo => GetZPropertyInfo(Schema.Status);

		public ZString StatusDescription
		{
			get { return new ContainerStatusList().GetDescriptionFromCode(Status); }
		}

		public ZPropertyInfo StatusDescriptionInfo => GetZPropertyInfo(Schema.StatusDescription);

		public ZString ContainerStatus
		{
			get { return Lookups.ContainerStatuses.GetDescriptionFromCode(JC_ContainerStatus); }
		}

		public ZPropertyInfo ContainerStatusInfo => GetZPropertyInfo(Schema.ContainerStatus);

		#endregion

		#region Quarantine

		public ZString QuarantineCode
		{
			get
			{
				if (quarantineCode.IsEmpty && MostRecentCARSTMessage != null)
				{
					if (MostRecentCARSTMessage.ConsolidatedStatus == CMRMessage.CMRMessageStatusDescription.WITHDRAWN && MoreRecentCLRMessageExists())
					{
						quarantineCode = string.Empty;
					}
					else
					{
						quarantineCode = MostRecentCARSTMessage.ConsolidatedStatus;
					}
				}

				return quarantineCode;
			}
		}
		ZString quarantineCode;

		public ZPropertyInfo QuarantineCodeInfo => GetZPropertyInfo(Schema.QuarantineCode);

		CMRCARSTMessage MostRecentCARSTMessage
		{
			get
			{
				if (mostRecentCARSTMessage == null)
				{
					var messages = new List<EDIMessage>();
					Bills.ForEach(bill => { messages = messages.Concat(bill.Messages.OfType<EDIMessage>()).ToList(); });
					Pivots.ForEach(pivot => { messages = messages.Concat(pivot.Messages.OfType<EDIMessage>()).ToList(); });

					var matchedCARSTMessages = new List<CMRCARSTMessage>();
					foreach (var message in messages)
					{
						if (message.EM_MessageType == CMRMessage.CMRMessageTypes.CARST)
						{
							var carstMessage = Factory.Load<CMRCARSTMessage>(message.PK);
							if (carstMessage.ContainerNumber.Contains(ContainerNumber))
							{
								matchedCARSTMessages.Add(carstMessage);
							}
						}
					}

					if (matchedCARSTMessages.Count > 0)
					{
						SortMessages(matchedCARSTMessages);
						mostRecentCARSTMessage = matchedCARSTMessages.LastOrDefault();
					}
				}

				return mostRecentCARSTMessage;
			}
		}

		CMRCARSTMessage mostRecentCARSTMessage;

		bool MoreRecentCLRMessageExists()
		{
			foreach (CusSCAHouse housebill in Bills)
			{
				foreach (EDIMessage message in housebill.Messages)
				{
					if (message.EM_MessageType == CMRMessage.ManifestResponseSubTypes.Clear)
					{
						if (message.EM_SystemCreateTimeUtc > MostRecentCARSTMessage.EM_SystemCreateTimeUtc)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		protected void SortMessages(List<CMRCARSTMessage> messages)
		{
			messages.Sort((x, y) =>
				{
					int dateCompare = x.EM_SystemCreateTimeUtc.CompareTo(y.EM_SystemCreateTimeUtc);

					if (dateCompare == 0)
					{
						int interchangeNumCompare = x.EM_InterchangeNumber.CompareTo(y.EM_InterchangeNumber);

						if (interchangeNumCompare == 0)
						{
							return x.EM_MessageNum.CompareTo(y.EM_MessageNum);
						}

						return interchangeNumCompare;
					}

					return dateCompare;
				});
		}

		CusSCAHouse[] Bills
		{
			get
			{
				if (bills == null)
				{
					ZGuid[] pks = new ZGuid[Shipments.Count];
					int i = 0;
					foreach (TrackingShipment shipment in Shipments)
					{
						pks[i] = shipment.PK;
						i++;
					}
					bills = Factory.Load<CusSCAHouse>(new ZQuery(CusSCAHouseSchema.CA_JS, pks));
				}

				return bills;
			}
		}
		CusSCAHouse[] bills;

		CusSCAPivot[] Pivots
		{
			get
			{
				if (pivots == null)
				{
					var pks = Bills.Select(bill => bill.PK);
					pivots = Factory.Load<CusSCAPivot>(new ZQuery(CusSCAPivotSchema.CV_CA, pks));
				}

				return pivots;
			}
		}
		CusSCAPivot[] pivots;

		#endregion

		#region Available

		/// <summary>
		/// Date time container is available
		/// </summary>
		public ZDateTime Available
		{
			get { return JC_FCLAvailable; }
		}

		public ZPropertyInfo AvailableInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.Available, x => JC_FCLAvailableInfo); }
		}

		#endregion

		#region Storage Begins

		/// <summary>
		/// The last day before the container will exceed free days on the wharf
		/// </summary>
		public ZDateTime StorageBegins
		{
			get { return JC_ArrivalCTOStorageStartDate; }
		}

		public ZPropertyInfo StorageBeginsInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.StorageBegins, x => JC_ArrivalCTOStorageStartDateInfo); }
		}

		#endregion

		#region Slot Date

		/// <summary>
		/// Date & Time slot container is scheduled for uplift from the wharf
		/// </summary>
		public ZDateTime SlotDate
		{
			get { return JC_ArrivalSlotDateTime; }
		}

		public ZPropertyInfo SlotDateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.SlotDate, x => JC_ArrivalSlotDateTimeInfo); }
		}

		#endregion

		#region Required Delivery

		/// <summary>
		/// Required Delivery Date - date container required 
		/// to be delivered into appropriate McPherson's warehouse.
		/// This field is modifiable to allow the warehouses to schedule deliveries the carriers.
		/// </summary>
		public ZDateTime RequiredDelivery
		{
			get { return JC_ArrivalEstimatedDelivery; }
			set { JC_ArrivalEstimatedDelivery = value; } // Updated by XML Import
		}

		public ZPropertyInfo RequiredDeliveryInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.RequiredDelivery, x => JC_ArrivalEstimatedDeliveryInfo); }
		}

		public ZString RequiredDeliveryStatus
		{
			get
			{
				ZString result = ZString.Empty;
				if (!JC_LastFreeDay.IsEmpty)
				{
					if (JC_LastFreeDay <= ZDateTime.Now && RequiredDelivery.IsEmpty)
					{
						result = Constants.DateTimeStatus.Overdue;
					}
					else if (JC_LastFreeDay < RequiredDelivery)
					{
						result = Constants.DateTimeStatus.Late;
					}
					else if (JC_LastFreeDay >= RequiredDelivery)
					{
						result = Constants.DateTimeStatus.OnTime;
					}
				}
				return result;
			}
		}

		public ZPropertyInfo RequiredDeliveryStatusInfo
		{
			get { return GetZPropertyInfo(Schema.RequiredDeliveryStatus); }
		}

		#endregion

		#region Confirmed Delivery

		/// <summary>
		/// Confirmed delivery date - date the carrier has confirmed 
		/// they will deliver the container into appropriate McPherson's warehouse.
		/// This field is updateable to allow carriers to confirm delivery times.
		/// </summary>
		public ZDateTime ConfirmedDelivery
		{
			get { return JC_ArrivalCartageAdvised; }
			set { JC_ArrivalCartageAdvised = value; }
		}

		public ZPropertyInfo ConfirmedDeliveryInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ConfirmedDelivery, x => JC_ArrivalCartageAdvisedInfo); }
		}

		public ZString ConfirmedDeliveryStatus
		{
			get
			{
				ZString result = ZString.Empty;
				if (!RequiredDelivery.IsEmpty)
				{
					if (ConfirmedDelivery.IsEmpty)
					{
						result = Constants.DateTimeStatus.Overdue;
					}
					else if (RequiredDelivery.Date != ConfirmedDelivery.Date)
					{
						result = Constants.DateTimeStatus.Late;
					}
					else if (RequiredDelivery.Date == ConfirmedDelivery.Date)
					{
						result = Constants.DateTimeStatus.OnTime;
					}
				}
				return result;
			}
		}

		public ZPropertyInfo ConfirmedDeliveryStatusInfo
		{
			get { return GetZPropertyInfo(Schema.ConfirmedDeliveryStatus); }
		}

		#endregion

		#region Actual Delivery

		/// <summary>
		/// Actual delivery date - date the container actually arrived 
		/// into McPherson's warehouse.
		/// This field is updateable by McPherson's to confirm delivery times.
		/// </summary>
		public ZDateTime ActualDelivery
		{
			get { return JC_ArrivalCartageComplete; }
			set { JC_ArrivalCartageComplete = value; }
		}

		public ZPropertyInfo ActualDeliveryInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ActualDelivery, x => JC_ArrivalCartageCompleteInfo); }
		}

		public ZString ActualDeliveryStatus
		{
			get
			{
				ZString result = ZString.Empty;
				if (!ConfirmedDelivery.IsEmpty)
				{
					if (ActualDelivery.IsEmpty)
					{
						result = Constants.DateTimeStatus.Overdue;
					}
					else if (ActualDelivery > ConfirmedDelivery)
					{
						result = Constants.DateTimeStatus.Late;
					}
					else if (ActualDelivery <= ConfirmedDelivery)
					{
						result = Constants.DateTimeStatus.OnTime;
					}
				}
				return result;
			}
		}

		public ZPropertyInfo ActualDeliveryStatusInfo
		{
			get { return GetZPropertyInfo(Schema.ActualDeliveryStatus); }
		}

		#endregion

		#region Shipment Status

		/// <summary>
		/// Shipment Numbers
		/// </summary>
		public ZString ShipmentStatuses
		{
			get
			{
				if (!shipmentStatusesHaveBeenCalculated)
				{
					List<ZString> statuses = new List<ZString>();

					foreach (TrackingShipment shipment in Shipments)
					{
						if (!shipment.IsDeleted &&
							!shipment.DocsAndCartage.JP_CustomAttrib1.IsEmpty &&
							!statuses.Contains(shipment.DocsAndCartage.JP_CustomAttrib1))
						{
							statuses.Add(shipment.DocsAndCartage.JP_CustomAttrib1);
						}
					}

					shipmentStatuses = ZString.Join(", ", statuses.ToArray());
					shipmentStatusesHaveBeenCalculated = true;
				}

				return shipmentStatuses;
			}
		}

		ZString shipmentStatuses;
		bool shipmentStatusesHaveBeenCalculated;

		public ZPropertyInfo ShipmentStatusesInfo => GetZPropertyInfo(Schema.ShipmentStatuses);

		#endregion

		#region Declaration

		public BaseJobDeclaration Declaration
		{
			get
			{
				if (declaration == null && CustomsContainer != null && CustomsContainer.Declaration != null)
				{
					declaration = CustomsContainer.Declaration;
				}
				return declaration;
			}
		}
		BaseJobDeclaration declaration;

		#endregion

		#region CustomsContainer

		public BaseCusContainer CustomsContainer
		{
			get
			{
				if (customsContainer == null)
				{
					customsContainer = Factory.LoadTop1<BaseCusContainer>(new ZQuery(CusContainerSchema.CO_JC, this.PK));
				}
				return customsContainer;
			}
		}
		BaseCusContainer customsContainer;

		#endregion

		#region ShipmentNumbers

		/// <summary>
		/// Shipment Numbers
		/// </summary>
		public ZString ShipmentNumbers
		{
			get
			{
				if (!ShipmentNumbersHasBeenCalculated)
				{
					List<ZString> listofNumbers = new List<ZString>();

					foreach (TrackingShipment shipment in Shipments)
					{
						listofNumbers.Add(shipment.JS_UniqueConsignRef);
					}
					if (listofNumbers.Count == 0 && CustomsContainer != null && CustomsContainer.Declaration != null)
					{
						listofNumbers.Add(CustomsContainer.Declaration.JE_DeclarationReference);
					}
					shipmentNumbers = ZString.Join(", ", listofNumbers.ToArray());
					ShipmentNumbersHasBeenCalculated = true;
				}

				return shipmentNumbers;
			}
		}

		ZString shipmentNumbers;
#if DEBUG
		public
#endif
 bool ShipmentNumbersHasBeenCalculated;

		public ZPropertyInfo ShipmentNumbersInfo => GetZPropertyInfo(Schema.ShipmentNumbers);

		#endregion

		#region Consignees

		/// <summary>
		/// McPherson's Warehouse code
		/// </summary>
		public ZString Consignees
		{
			get
			{
				return DestinationConfirm.ConfirmAddress.IsEmpty ? (CustomsContainer != null && CustomsContainer.Declaration != null ? FormatAddress(CustomsContainer.Declaration.ImporterDeliveryAddress) : ZString.Empty) : ConsigneesExtended;
			}
		}

		public ZPropertyInfo ConsigneesInfo => GetZPropertyInfo(Schema.Consignees);

		#endregion

		#region Consignees Extended

		public ZString ConsigneesExtended
		{
			get { return FormatAddress(DestinationConfirm.ConfirmAddress); }
		}

		public ZPropertyInfo ConsigneesExtendedInfo => GetZPropertyInfo(Schema.ConsigneesExtended);

		#endregion

		#region Consignors Extended

		public ZString ConsignorsExtended
		{
			get { return FormatAddress(OriginConfirm.ConfirmAddress); }
		}

		ZString FormatAddress(JobDocAddress address)
		{
			return address.E2_CompanyNameTruncated + System.Environment.NewLine +
			address.AddressSummary + System.Environment.NewLine;
		}

		public ZPropertyInfo ConsignorsExtendedInfo => GetZPropertyInfo(Schema.ConsignorsExtended);

		#endregion

		#region Detention

		/// <summary>
		/// Date and time container goes on detention
		/// </summary>
		public ZDateTime EmptyReturnRequired
		{
			get { return JC_EmptyReturnedBy; }
		}

		public ZPropertyInfo EmptyReturnRequiredInfo => GetZPropertyInfo(Schema.EmptyReturnRequired);

		#endregion

		#region Empty Ready

		/// <summary>
		/// This field is modifiable to allow the warehouses to notify carriers to pickup empties.
		/// </summary>
		public ZDateTime EmptyReady
		{
			get { return JC_EmptyReadyForReturn; }
			set { JC_EmptyReadyForReturn = value; }
		}

		public ZPropertyInfo EmptyReadyInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.EmptyReady, x => JC_EmptyReadyForReturnInfo); }
		}

		public ZString EmptyReadyStatus
		{
			get
			{
				ZString result = ZString.Empty;
				if (!EmptyReturnRequired.IsEmpty)
				{
					if (EmptyReturnRequired <= ZDateTime.Now && EmptyReady.IsEmpty)
					{
						result = Constants.DateTimeStatus.Overdue;
					}
					else if (EmptyReturnRequired < EmptyReady)
					{
						result = Constants.DateTimeStatus.Late;
					}
					else if (EmptyReturnRequired >= EmptyReady)
					{
						result = Constants.DateTimeStatus.OnTime;
					}
				}
				return result;
			}
		}

		public ZPropertyInfo EmptyReadyStatusInfo
		{
			get { return GetZPropertyInfo(Schema.EmptyReadyStatus); }
		}

		#endregion

		#region Pickup

		/// <summary>
		/// Date and time container uplifted from the wharf
		/// </summary>
		public ZDateTime EmptyPickup
		{
			get { return JC_EmptyReturnedBy; }
			set { JC_EmptyReturnedBy = value; }
		}

		public ZPropertyInfo EmptyPickupInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.EmptyPickup, x => JC_EmptyReturnedByInfo); }
		}

		#endregion

		#region Actual Dehire

		/// <summary>
		/// Actual date and time container was dehired with the shipping line
		/// </summary>
		public ZDateTime ActualDehire
		{
			get { return JC_ContainerYardEmptyReturnGateIn; }
			set { JC_ContainerYardEmptyReturnGateIn = value; }
		}

		public ZPropertyInfo ActualDehireInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ActualDehire, x => JC_ContainerYardEmptyReturnGateInInfo); }
		}

		public ZString ActualDehireStatus
		{
			get
			{
				ZString result = ZString.Empty;
				if (!EmptyReturnRequired.IsEmpty)
				{
					if (EmptyReturnRequired <= ZDateTime.Now && JC_FCLWharfGateIn.IsEmpty && ActualDehire.IsEmpty)
					{
						result = Constants.DateTimeStatus.Overdue;
					}
					else if (EmptyReturnRequired < JC_FCLWharfGateIn || EmptyReturnRequired < ActualDehire)
					{
						result = Constants.DateTimeStatus.Late;
					}
					else if (EmptyReturnRequired >= JC_FCLWharfGateIn || EmptyReturnRequired >= ActualDehire)
					{
						result = Constants.DateTimeStatus.OnTime;
					}
				}
				return result;
			}
		}

		public ZPropertyInfo ActualDehireStatusInfo
		{
			get { return GetZPropertyInfo(Schema.ActualDehireStatus); }
		}

		#endregion

		#region Consol Number

		public ZString ConsolNumber
		{
			get { return Consol != null ? Consol.JK_UniqueConsignRef : ZString.Empty; }
		}

		public ZPropertyInfo ConsolNumberInfo
		{
			get { return GetZPropertyInfo(Schema.ConsolNumber); }
		}

		#endregion

		#region Master Bill Number

		public ZString MasterBillNumber
		{
			get { return Consol != null ? Consol.JK_MasterBillNum : ZString.Empty; }
		}

		public ZPropertyInfo MasterBillNumberInfo => GetZPropertyInfo(Schema.MasterBillNumber);

		#endregion

		#region Weight

		/// <summary>
		/// Total Weight of Container
		/// </summary>
		public ZDecimal Weight
		{
			get { return JC_GrossWeight; }
		}

		public ZPropertyInfo WeightInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.Weight, x => JC_GrossWeightInfo); }
		}

		public ZString WeightUQ
		{
			get { return JC_GrossWeightUQ; }
		}

		public ZPropertyInfo WeightUQInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.WeightUQ, x => JC_GrossWeightUQInfo); }
		}

		#endregion

		#region WeightWithUnits

		public ZString WeightWithUnits
		{
			get
			{
				var roundedDecimal = this.GetRoundedValue(WeightInfo, Weight);
				var formattedDecimal = FormatNumber(roundedDecimal, DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(this, WeightInfo.PropertyDescriptor));
				return string.Format("{0} {1}", formattedDecimal, WeightUQ);
			}
		}

		public ZPropertyInfo WeightWithUnitsInfo
		{
			get { return GetZPropertyInfo(Schema.WeightWithUnits); }
		}

		protected string FormatNumber(ZDecimal number, int decimalsToShow)
		{
			return Utilities.FormatNumber(number, decimalsToShow, WebEnvShared.ClientCulture);
		}

		#endregion

		#region Port Of Loading

		/// <summary>
		/// Port container was loaded
		/// </summary>
		public ZString PortOfLoading
		{
			get
			{
				ZString result = ZString.Empty;
				if (Consol != null && Consol.LoadPort != null)
				{
					result = Consol.LoadPort.Description;
				}
				else if (Declaration != null && Declaration.PortOfLoading != null)
				{
					result = Declaration.PortOfLoading.Description;
				}
				return result;
			}
		}

		public ZPropertyInfo PortOfLoadingInfo
		{
			get { return GetZPropertyInfo(Schema.PortOfLoading); }
		}

		#endregion

		#region Vessel Name

		/// <summary>
		/// Vessel goods are being shipped on
		/// </summary>
		public ZString VesselName
		{
			get
			{
				ZString result = ZString.Empty;
				var mostInterestingTransport = Consol?.Transports?.MostInterestingTransport;
				if (mostInterestingTransport != null)
				{
					result = mostInterestingTransport.JW_Vessel;
				}
				else if (Declaration != null)
				{
					result = Declaration.JE_VesselName;
				}

				return result;
			}
		}

		public ZPropertyInfo VesselNameInfo => GetZPropertyInfo(Schema.VesselName);

		#endregion

		#region Voyage

		/// <summary>
		/// Voyage goods are being shipped on
		/// </summary>
		public ZString Voyage
		{
			get
			{
				var mostInterestingTransport = Consol?.Transports?.MostInterestingTransport;
				if (mostInterestingTransport != null)
				{
					return mostInterestingTransport.JW_VoyageFlight;
				}

				if (Consol?.Voyage != null)
				{
					return Consol.Voyage.JV_VoyageFlight;
				}

				return Declaration?.JE_VoyageFlightNo ?? ZString.Empty;
			}
		}

		public ZPropertyInfo VoyageInfo => GetZPropertyInfo(Schema.Voyage);

		#endregion

		#endregion

		#region UserEditableNote

		public IStmNoteParent NotesParentBO
		{
			get { return this; }
		}

		public WebUserEditableNote UserEditableNoteHelper
		{
			get
			{
				if (userEditableNoteHelper == null)
				{
					userEditableNoteHelper = GetNewUserEditableNoteHelper();
				}
				return userEditableNoteHelper;
			}
		}
		WebUserEditableNote userEditableNoteHelper;

		protected WebUserEditableNote GetNewUserEditableNoteHelper()
		{
			return new WebUserEditableNote(this, PredefinedNoteTypes.Instance.SpecialInstructions);
		}

		#endregion

		#region Implementation

		protected override CommonConsol LoadParentConsol()
		{
			return Factory.Load<TrackingConsol>(JC_JK);
		}

		protected override PackLineManyToManyCollection GetNewPackLineCollection()
		{
			return new TrackingPackLineManyToManyCollection(this);
		}

		protected override CommonPickupDeliveryConfirm GetOrCreateConfirm(string pickupDeliveryType, bool isAllowToBeCreated)
		{
			CommonPickupDeliveryConfirm result = base.GetOrCreateConfirm(pickupDeliveryType, isAllowToBeCreated);
			if (!CurrentOrg.IsEmpty && Shipments.Count > 0)
			{
				result.FirstShipment = Shipments[0];
			}

			return result;
		}

		public TrackingSiteUser SiteUser
		{
			get
			{
				if (siteUser == null)
				{
					if (WebEnv.AppInstance != null && WebEnv.AppInstance.SiteUser != null)
					{
						siteUser = (WebEnv.AppInstance.SiteUser as TrackingSiteUser);
					}
				}
				return siteUser;
			}
			set
			{
				siteUser = value;

				if (SiteUser != null &&
				!(Logs.AutoCreatedLogDefaultSL_Reference == SiteUser.ContactAndCompanyReference))
				{
					Logs.AutoCreatedLogDefaultSL_Reference = SiteUser.ContactAndCompanyReference;
				}
			}
		}
		TrackingSiteUser siteUser;

		public ZGuid CurrentOrg
		{
			get { return SiteUser != null && SiteUser.IsLoggedIn ? SiteUser.LoggedInOrganisation.PK : ZGuid.Empty; }
		}

		#endregion

		#region IWebDocumentsSupport Members

		public ZGuid DocParentPK
		{
			get { return PK; }
		}

		public List<ZGuid> DocRelatedPKs
		{
			get { return new List<ZGuid>(); }
		}

		public OrgContact LoggedInContact
		{
			get { return SiteUser != null ? SiteUser.LoggedInUser : null; }
		}

		public DocumentSupport DocumentHelper
		{
			get
			{
				return fDocumentHelper ?? (fDocumentHelper = new DocumentSupport(this));
			}
		}
		DocumentSupport fDocumentHelper;

		#endregion

		#region IWebDocumentsWithUploadSupport

		public new DocManagerInfo DocManagerInfo
		{
			get { return ((IDocManagerSupport)this).DocManagerInfo; }
		}

		public DocumentUploadSupport DocumentUploadHelper
		{
			get
			{
				if (documentUploadHelper == null)
				{
					documentUploadHelper = new DocumentUploadSupport(Factory);
				}
				return documentUploadHelper;
			}
		}

		DocumentUploadSupport documentUploadHelper;

		public void ResetDocumentHelper()
		{
		}

		#endregion

		#region TrackingEvents

		public StmALogCollection TrackingEvents
		{
			get { return this.GetTrackingEvents(SiteUser); }
		}

		public bool CanViewTrackingEvents
		{
			get { return SiteUser?.CanViewEvents ?? false; }
		}

		#endregion

		#region Milestones

		public void ReloadMilestones()
		{
			milestones = null;
		}

		public TrackingMilestoneCollection Milestones
		{
			get { return milestones ?? (milestones = new TrackingMilestoneCollection(this)); }
		}
		TrackingMilestoneCollection milestones;

		public TrackingMilestoneCollection EditableMilestones
		{
			get { return editableMilestones ?? (editableMilestones = new TrackingMilestoneCollection(this, true)); }
		}
		TrackingMilestoneCollection editableMilestones;

		#endregion

		#region IUpdatableMilestoneEventsProvider

		public List<string> UpdatableMilestoneEventCodes
		{
			get
			{
				return (new UpdateableMilestoneEventsHelper(SiteUser)).GetUpdateableMilestoneEvents(WebDataRegistry.Instance.ContainerMilestoneEventUpdates.Value, WebParties);
			}
		}

		#endregion

		#region IEventReferenceProvider

		public string EventReference
		{
			get
			{
				if (WebEnv.AppInstance != null && WebEnv.AppInstance.SiteUser != null)
				{
					return (new EventReferenceHelper(WebEnv.AppInstance.SiteUser as TrackingSiteUser)).GetEventReferences(WebParties);
				}
				return string.Empty;
			}
		}

		#endregion

		#region WebParties

		WebPartyTypeOrgPairCollection WebParties
		{
			get
			{
				if (webParties == null)
				{
					webParties = new WebPartyTypeOrgPairCollection();

					if (Booking != null)
					{
						webParties.Add(WebPartyType.ExportBroker, Booking.ExportBroker);
						webParties.Add(WebPartyType.ImportBroker, Booking.ImportBroker);
						webParties.Add(WebPartyType.DeliveryAgent, Booking.DeliveryAgent);
						webParties.Add(WebPartyType.Shipper, Booking.Consignor);
						webParties.Add(WebPartyType.Consignee, Booking.Consignee);

						if (Booking.Job != null)
						{
							webParties.Add(WebPartyType.LocalClient, Booking.Job.LocalCharges);
						}
					}

					if (Consol != null)
					{
						webParties.Add(WebPartyType.ReceivingAgent, Consol.ReceivingForwarder);
						webParties.Add(WebPartyType.SendingAgent, Consol.SendingForwarder);
					}

					if (Declaration != null)
					{
						webParties.Add(WebPartyType.Supplier, Declaration.Supplier);
						webParties.Add(WebPartyType.Forwarder, Declaration.Forwarder);
						webParties.Add(WebPartyType.Carrier, Declaration.ShippingLine);
						webParties.Add(WebPartyType.Importer, Declaration.Importer);
						if (Declaration is Customs.US.Business.JobDeclaration)
						{
							webParties.Add(WebPartyType.UltimateConsignee, ((Customs.US.Business.JobDeclaration)Declaration).ConsigneeOrgAddress);
							webParties.Add(WebPartyType.ExternalBroker, ((Customs.US.Business.JobDeclaration)Declaration).ExternalBroker);
						}
					}

					foreach (PackLine packLine in PackLines)
					{
						if (packLine.Shipment != null)
						{
							webParties.Add(WebPartyType.DeliveryAgent, packLine.Shipment.DeliveryAgent);
						}
					}
				}

				return webParties;
			}
		}
		WebPartyTypeOrgPairCollection webParties;

		#endregion

		#region IEmailNotification Members

		ZString IBizOChangesEmailNotification.Number
		{
			get { return ContainerNumber; }
		}

		ZBool IBizOChangesEmailNotification.IsCancelled
		{
			get { return false; }
		}

		GlbBranch IBizOChangesEmailNotification.EventBranch
		{
			get { return GlbBranch.FindControllingBranchWithFallBackToAnyCompany(((IBizOChangesEmailNotification)this).RelatedOrg); }
		}

		GuidRegistryItem IBizOChangesEmailNotification.EmailGroupRegistryItem
		{
			get { return WebDataRegistry.Instance.ContainerNotificationEmailGroup; }
		}

		ControllerID IBizOChangesEmailNotification.ControllerForEnterpriseUrl
		{
			get { return ControllerIDs.Containers; }
		}

		OrgHeader IBizOChangesEmailNotification.RelatedOrg
		{
			get
			{
				if (Consol != null)
				{
					if ((Consol.JK_ConsolMode == ContainerModes.FCL || Consol.JK_ConsolMode == ContainerModes.BuyersConsol) && Consol.Shipments.Count == 1)
					{
						var shipment = Consol.Shipments[0];
						if (shipment.IsImport())
						{
							return shipment.ConsigneeDeliveryAddress.Organisation;
						}
					}

					return Consol.ReceivingForwarder;
				}

				return null;
			}
		}

		ZGuid IBizOChangesEmailNotification.GetStaffGuid(OrgStaffAssignmentsCollection staffAssignments, CodeDescriptionBool role)
		{
			ZString staffNK = staffAssignments.GetStaffAssignment(role.Code, OrgStaffAssignmentsLookups.ContainerYardServices);
			GlbStaff staff = staffAssignments.Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, staffNK);
			if (staff != null)
			{
				return staff.PK;
			}
			else
			{
				return ZGuid.Empty;
			}
		}

		CodeDescriptionBoolRegistryItem IBizOChangesEmailNotification.StaffRolesToNotify
		{
			get { return WebDataRegistry.Instance.ContainerNotificationStaffRoles; }
		}

		CodePairRegistryItem IBizOChangesEmailNotification.NotificationSendingRule
		{
			get { return WebDataRegistry.Instance.ContainerNotificationOptions; }
		}

		#region Email Reporting

		void IBizOChangesEmailNotification.AddPropertiesForEmailReporting(DataState state)
		{
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("5a3c0d09-c5ac-42ac-a40d-2e4a77402aa0", "Delivery Sequence"), JC_DeliverySequence); // GetMultilingualString method requires Engligh Text as parameter
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("96b5da15-cbfb-4c0e-99fb-6b573d73526a", "Required Delivery"), RequiredDelivery); // GetMultilingualString method requires Engligh Text as parameter
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("06418ce9-8756-4f3b-90ca-842f3611b4eb", "Confirmed Delivery"), ConfirmedDelivery); // GetMultilingualString method requires Engligh Text as parameter
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("f6aa1288-214f-454e-906c-68d34badb8c1", "Actual Delivery"), ActualDelivery); // GetMultilingualString method requires Engligh Text as parameter
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("17e7cada-6c03-48e2-bd02-eeb1766c1758", "Empty Ready"), EmptyReady); // GetMultilingualString method requires Engligh Text as parameter
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("ae4f26fc-56f6-4637-b9c0-e9a1e3490295", "Empty Pickup"), EmptyPickup); // GetMultilingualString method requires Engligh Text as parameter
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("c9c45ecc-4950-470a-ba06-b9b88a882732", "Actual De-hire"), ActualDehire); // GetMultilingualString method requires Engligh Text as parameter
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("bf472d06-e302-4ee6-8980-7df2a7d6191f", "Port Transport Ref"), JC_DepartureCartageRef); // GetMultilingualString method requires Engligh Text as parameter
			DocumentHelper.AddDocumentsForEmailReporting(state, PropertiesForEmailReporting);
			Milestones.AddForEmailReporting(state, PropertiesForEmailReporting);
		}

		PropertyChangeInfo[] IBizOChangesEmailNotification.GetPropertiesForEmailReporting()
		{
			return PropertiesForEmailReporting.GetValuesAsArray();
		}

		PropertyChangeInfoCollection PropertiesForEmailReporting
		{
			get
			{
				if (propertiesForEmailReporting == null)
				{
					propertiesForEmailReporting = new PropertyChangeInfoCollection();
				}

				return propertiesForEmailReporting;
			}
		}
		PropertyChangeInfoCollection propertiesForEmailReporting;

		#endregion

		#endregion
	}
}
