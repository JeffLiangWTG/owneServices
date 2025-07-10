using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.EConversation.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;
using EventConstants = CargoWise.EventReference.Constants;
using ResString = Enterprise.Freight.Forwarding.Business.ResString;
using WorkflowSelectionOrgTypeCodes = Enterprise.Registry.Business.ClientInTemplateSelectionOrgTypeList.Codes;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	#region Voyage Data Interface

	public interface IOrderVoyageData : IBusiness
	{
		ZPropertyInfo JD_DepartureVoyageInfo { get; }
		ZPropertyInfo JD_IntermediateVoyageInfo { get; }
		ZPropertyInfo JD_ArrivalVoyageInfo { get; }
		ZBool IsSeaTransport { get; }
		ZPropertyInfo IsSeaTransportInfo { get; }
		ZPropertyInfo JD_TransportModeInfo { get; }
		PlanningVoyageState PlanningVoyageState { get; }
	}

	#endregion

	[UserDefinedValues]
	[UniversalDataContext(DataContextType.OrderManagerOrder)]
	[UniversalCopyAssociateElement("CustomsCharges", "Charges")]
	[UniversalCopyAssociateElement("ShipmentPrePlanning", "PreAdvice")]
	[CodeProperty(Schema.JD_OrderNumberAndSplit), DescriptionProperty(Schema.JD_OrderGoodsDescription)]
	[AutoRatingAuditLogExclude]
	[Metadata.Integration.MetadataContext(Enterprise.Metadata.Integration.MetadataContext.Order)]
	[ProvideMetaDataProperty("DefaultNumberOfDecimals", MetaDataTypes.DecimalPlaces)]
	[VisualizableDocumentsSupportable("OrderVisualizableDocumentSupporter")]
	public class Order : AutoJobOrderHeader,
		Enterprise.Integration.Forwarding.IOrder,
		IAttachedOrder,
		IJobNumber,
		ICustomLabelsConfigOrgProvider,
		ISupportDataImporting,
		ITemplateCopyable,
		ISendEmailSource,
		IDocumentSupportable,
		IFlightDetailsSuppression,
		IHaveRequiredDocuments,
		IWorkflowProvider,
		IWorkflowTriggerFieldChangeSource,
		IDocAddresses,
		IOrderVoyageData,
		IBuyerSupplierRelationshipConsumer,
		ICommonInvoice,
		IApportionInvoiceHolder,
		ICurrencyConverterDataProviderWithFixedExRates,
		IChargeApportionee,
		IRatingSupporter,
		ILandedCostHeader,
		ILandedCostExchangeRateHolder,
		ILandedCostChargeHolder,
		ILandedCostHeaderProvider,
		ILandedCostDistributeTo,
		ICustomFieldProvider,
		IModuleToModule,
		IProcessHandlingInfoProvider,
		IRelatedJob,
		IDefaultNumberOfDecimalsSupporterWithSchemaColumn,
		IRelatedItemsNameProvider,
		IRelatableActivity,
		IUniversalXMLNoteParent,
		IExternalRequestGenerationProvider,
		IConversationProvider
	{
		#region Schema

		public new class Schema : AutoJobOrderHeader.Schema
		{
			public const string AttachedShipment_RS_NKServiceLevel = "AttachedShipment_RS_NKServiceLevel";
			public const string AttachedShipment_RL_NKDestination = "AttachedShipment_RL_NKDestination";
			public const string AttachedShipment_RL_NKOrigin = "AttachedShipment_RL_NKOrigin";
			public const string JD_PlannedContainersVisible = "JD_PlannedContainersVisible";
			public const string JD_ChargesVisible = "JD_ChargesVisible";
			public const string JD_CalcShipmentBrokerageNumber = "JD_CalcShipmentBrokerageNumber";
			public const string JD_OrderNumberAndSplit = "JD_OrderNumberAndSplit";
			public const string Weight = "Weight"; // May be an identifier or GUID.
			public const string WeightUnit = "WeightUnit";
			public const string Volume = "Volume"; // May be an identifier or GUID.
			public const string VolumeUnit = "VolumeUnit";
			public const string Packs = "Packs"; // May be an identifier or GUID.
			public const string JD_Calc_PackType = "JD_Calc_PackType";
			public const string JD_JS_ThatAutoUpdatesShipmentDetailsFromOrder = "JD_JS_ThatAutoUpdatesShipmentDetailsFromOrder";
			public const string JD_VB_ThatAutoUpdatesBookingDetailsFromOrder = "JD_VB_ThatAutoUpdatesBookingDetailsFromOrder";
			public const string JD_Calc_InnerPacks = "JD_Calc_InnerPacks";
			public const string JD_Calc_InnerPackType = "JD_Calc_InnerPackType";
			public const string JD_Calc_HouseBill = "JD_Calc_HouseBill";
			public const string JD_Calc_MasterBill = "JD_Calc_MasterBill";
			public const string JD_Milestone_E_DEP = "JD_Milestone_E_DEP";
			public const string JD_Milestone_A_DEP = "JD_Milestone_A_DEP";
			public const string JD_Milestone_E_ARV = "JD_Milestone_E_ARV";
			public const string JD_Milestone_A_ARV = "JD_Milestone_A_ARV";
			public const string JD_Milestone_E_EXW = "JD_Milestone_E_EXW";
			public const string JD_Milestone_A_EXW = "JD_Milestone_A_EXW";
			public const string JD_Milestone_E_GIW = "JD_Milestone_E_GIW";
			public const string JD_Milestone_A_GIW = "JD_Milestone_A_GIW";
			public const string JD_Milestone_E_CCC = "JD_Milestone_E_CCC";
			public const string JD_Milestone_A_CCC = "JD_Milestone_A_CCC";
			public const string JD_Milestone_E_CLR = "JD_Milestone_E_CLR";
			public const string JD_Milestone_A_CLR = "JD_Milestone_A_CLR";
			public const string JD_Milestone_E_CAV = "JD_Milestone_E_CAV";
			public const string JD_Milestone_A_CAV = "JD_Milestone_A_CAV";
			public const string JD_Milestone_E_DCA = "JD_Milestone_E_DCA";
			public const string JD_Milestone_A_DCA = "JD_Milestone_A_DCA";
			public const string JD_Milestone_E_DCF = "JD_Milestone_E_DCF";
			public const string JD_Milestone_A_DCF = "JD_Milestone_A_DCF";
			public const string JD_Calc_LineCount = "JD_Calc_LineCount";
			public const string JD_Calc_OuterPacks = "JD_Calc_OuterPacks";
			public const string JD_Calc_OuterPacksType = "JD_Calc_OuterPacksType";
			public const string JD_Calc_TotalQuantity = "JD_Calc_TotalQuantity";
			public const string JD_Calc_TotalQuantityInvoiced = "JD_Calc_TotalQuantityInvoiced";
			public const string JD_Calc_TotalQuantityReceived = "JD_Calc_TotalQuantityReceived";
			public const string JD_Calc_TotalQuantityRemaining = "JD_Calc_TotalQuantityRemaining";
			public const string JD_Calc_TotalWeight = "JD_Calc_TotalWeight";
			public const string JD_Calc_TotalWeightUnit = "JD_Calc_TotalWeightUnit";
			public const string JD_Calc_TotalVolume = "JD_Calc_TotalVolume";
			public const string JD_Calc_TotalVolumeUnit = "JD_Calc_TotalVolumeUnit";
			public const string BuyerPK = "BuyerPK";
			public const string SupplierPK = "SupplierPK";
		}

		#endregion

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Virtual methods are reviewed. All possible values are expected and handled.")]
		public Order(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			fPreviousJD_OrderStatus = JD_OrderStatus;
			RefreshOrderNumberReadOnly();
			RefreshVoyagesReadOnly();

			BuyerSupplierLinksHelper = new BuyerSupplierLinksHelper<Order>(this);
			BuyerSupplierLinksHelper.Register();

			ApportionmentDirtyChangedEventHandler += new EventHandler(OnApportionmentBeingDirty);
		}

		public static Order New(BusinessObjectFactory factory)
		{
			return factory.New<Order>();
		}

		#region Validation

		public new JobOrderHeaderValidation Validation
		{
			get { return base.Validation; }
		}

		protected override JobOrderHeaderValidation GetNewValidation()
		{
			return new JobOrderHeaderValidation(this);
		}

		#endregion

		#region Order Number

		ZString TryGetNextOrderNumber()
		{
			if (Buyer != null)
			{
				OrgMiscServ miscServ = Buyer.MiscServ;

				if (miscServ != null && miscServ.OM_IMDefaultToNewOrdersToNextOrderNum && !miscServ.OM_IMLastOrderReference.IsEmpty)
				{
					return TryParseOrderNumber(miscServ.OM_IMLastOrderReference).Left(Schema.JD_OrderNumberMaxLength);
				}
			}

			return ZString.Empty;
		}

		ZString TryParseOrderNumber(ZString orderNumber)
		{
			Regex reg = new Regex("^(?<Prefix>.*)(?<Number>[0-9]+)(?<Suffix>[a-zA-Z]*)$", RegexOptions.RightToLeft);
			Match match = reg.Match(orderNumber);

			if (match.Success)
			{
				ZString value = CalculateOrderNumber(match.Groups["Number"].ToString());
				return match.Groups["Prefix"] + value + match.Groups["Suffix"];
			}

			return orderNumber + "1";
		}

		ZString CalculateOrderNumber(ZString orderNumber)
		{
			long value = 0;
			if (!long.TryParse(orderNumber, out value))
			{
				return "1";
			}

			value++;
			return value.ToString("D" + orderNumber.Length, CultureInfo.InvariantCulture);
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new OrderFetchStrategy(this);
		}

		#endregion

		#region GetPossibleOrdersForAttachment_List

		public static OrderCollection GetPossibleOrdersForAttachment_List(IAttachOrders orderParent)
		{
			OrderCollection collection = null;

			if (orderParent != null)
			{
				collection = new OrderCollection(orderParent.Factory);
				collection.AdditionalFilter.AddToFilter(new ZQuery(JobOrderHeaderSchema.JD_IsCancelled, ZBool.False));
				collection.AdditionalFilter.AddToFilter(new ZQuery(JobOrderHeaderSchema.JD_OrderStatus, SQLComparisonOperator.NotEqual, Constants.OrderStatus.Cancelled));
				collection.AdditionalFilter.AddToFilter(new ZQuery(JobOrderHeaderSchema.JD_JS, null));

				if (!orderParent.TransportMode.IsEmpty)
				{
					collection.AdditionalFilter.AddToFilter(JobOrderHeaderSchema.JD_TransportMode, orderParent.TransportMode);
				}

				collection.AddNotificationWhenAdditionalFilterNotMetOverride = (errors, bizObj) =>
				{
					Order order = (Order)bizObj;

					if (!orderParent.TransportMode.IsEmpty && orderParent.TransportMode != order.JD_TransportMode)
					{
						errors.Add(Res.GetString("FBFAB554-EEEB-412F-9CE6-C57B12A232B2", "This Order cannot be chosen here as the Transport Mode is different to that of the {0}.",
							DataBoundResourceStrings.GetTableDescriptiveName(orderParent.TableName)));
					}
					else if (order.JD_OrderStatus == Constants.OrderStatus.Cancelled)
					{
						errors.Add(Res.GetString("E3C7446B-E821-469C-AD7C-7853FE4A55DE", "This Order cannot be chosen here as it has been canceled."));
					}

					if ((orderParent is CommonShipment shipment) && !order.CanAttachOrderToShipmentWithNonMatchingControllingCustomer(shipment, out var errorMessage))
					{
						errors.Add(errorMessage);
					}
				};

				collection.GetExtraNotificationHanlder = (bizObj) =>
				{
					var order = (Order)bizObj;
					if ((orderParent is CommonShipment shipment) && !order.CanAttachOrderToShipmentWithNonMatchingControllingCustomer(shipment, out var errorMessage))
					{
						return new Notification(CargoWise.EntityFramework.NotificationType.Error, errorMessage);
					}

					if (order.IsAttachedToSupplierBooking)
					{
						return new Notification(CargoWise.EntityFramework.NotificationType.Error,
							Res.GetString("725c7263-01ba-400e-bc1a-9a5ede1cde82", @"This Order is already linked to an active Supplier Booking. Orders in use in the Supplier Bookings module cannot be linked to Shipments directly.
Please cancel all active Supplier Bookings before attaching this Order directly, or proceed to the Supplier Booking and Container Load List process to create a new Shipment from this Order."));
					}

					return null;
				};

				OrdersFilterProvider provider = new OrdersFilterProvider();
				provider.TransportMode = orderParent.TransportMode;
				provider.Buyer = orderParent.ConsigneeDocumentaryAddress?.OrganisationPK ?? ZGuid.Empty;
				provider.Supplier = orderParent.ConsignorDocumentaryAddress?.OrganisationPK ?? ZGuid.Empty;
				provider.ShowUnAttatchedOrders = true;
				provider.SetDefaultFilters(collection);
			}

			return collection;
		}

		#endregion

		public override bool CanDetach => base.CanDetach && !IsAttachedToSupplierBooking;

		public override string ReasonNotToBeAbleToDetach => CanDetach ? base.ReasonNotToBeAbleToDetach : Res.GetString("4f8563df-df03-4fb3-a226-b0140012ae8a", @"This Order is linked to an active Supplier Booking. Orders linked to a Supplier Booking cannot be detached from Shipments directly.
Please detach the order through the supplier booking function.");

		#region Active Filter

		public static ZQuery ActiveFilter
		{
			get { return new ZQuery(JobOrderHeaderSchema.JD_IsCancelled, SQLComparisonOperator.Equal, ZBool.False); }
		}

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			IsSettingDefaults = true;
			try
			{
				JD_OrderDate = ZDateTime.Now;

				JD_TransportMode = GlbDepartment.CurrentDepartment.TransportMode;
				JD_OrderStatus = Constants.OrderStatus.Incomplete;
				JD_UnitOfWeight = Env.Registry.FreightWeightUnit;
				JD_UnitOfVolume = Env.Registry.FreightVolumeUnit;
				JD_F3_NKPackType = FreightPacksDataRegistry.Instance.OuterPackUnit.Value;
				JD_Calc_Currency.Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			}
			finally
			{
				IsSettingDefaults = false;
			}
		}

		ZString fPreviousJD_OrderStatus;

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region Name

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("80694708-482c-43a1-9b13-219e562964e9", "Order {0}", this.JD_OrderNumberAndSplit); }
		}

		#endregion

		#region Load

		public override void OnLoaded()
		{
			base.OnLoaded();
			if (JD_IsCancelled)
			{
				UpdateReadOnlyForWhenCancelled();
			}

			OriginalJD_FollowUpDate = JD_FollowUpDate;
		}

		ZDateTime OriginalJD_FollowUpDate;

		#endregion

		#region Saving

		protected override void RunPreSaveValidationCore()
		{
			if (!((IAttachedOrder)this).ShouldSkipAllValidations)
			{
				base.RunPreSaveValidationCore();
			}
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (HasChanges)
			{
				AttemptToUpdateOrderStatus();
			}

			ApportionChargesIfNecessary();
		}

		public override void OnSaving()
		{
			RelatedChildActivityPivotCollection.RelinkRelatedSuperAndSubActivities();
			RelatedParentActivityPivotCollection.RelinkRelatedSuperAndSubActivities();

			base.OnSaving();

			if (JD_OrderStatus != fPreviousJD_OrderStatus && !fPreviousJD_OrderStatus.IsEmpty)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(Events.EditedARecord, string.Format(CultureInfo.InvariantCulture, "Status change from {0} to {1}", fPreviousJD_OrderStatus, JD_OrderStatus));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				fPreviousJD_OrderStatus = JD_OrderStatus;
			}

			if (JD_IsReleasedInfo.HasChanges)
			{
				static NoResString GetFlagText(bool flag) => flag ? (NoResString)"Released" : (NoResString)"On hold";

				Logs.AddNew(
					Events.StatusUpdated,
					[
						new(
							EventConstants.EventReferenceParameters.Codes.ReferenceNumber,
							(NoResString)string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}", JD_OrderNumber, JD_OrderNumberSplit, Buyer.OH_Code)
						),
						new(EventConstants.EventReferenceParameters.Codes.Old, GetFlagText((ZBool)JD_IsReleasedInfo.OriginalValue)),
						new(EventConstants.EventReferenceParameters.Codes.New, GetFlagText(JD_IsReleased)),
					]
				);
			}

			if (JD_OrderNumber.IsEmpty)
			{
				JD_OrderNumber = Env.NumberFountains.OrderNumber.GetNextFormatted(Factory);
			}

			if (Buyer != null && Buyer.MiscServ.OM_IMDefaultToNewOrdersToNextOrderNum && JD_OrderNumber == TryGetNextOrderNumber())
			{
				Buyer.MiscServ.OM_IMLastOrderReference = JD_OrderNumber;
			}

			if (Shipment != null && !Shipment.IsInDatabase)
			{
				Logs.UpdateEventReferenceNumbers(Events.Attached, Shipment.PK.ToString(), Shipment.LogReference(false));
			}

			if (PreAdvice != null && !PreAdvice.IsInDatabase)
			{
				Logs.UpdateEventReferenceNumbers(Events.Attached, PreAdvice.PK.ToString(), PreAdvice.LogReference(false));
			}

			CheckPortConsistentWithLinkedSupplierBooking();

			UpdateConfirmationDateFromOrderLines();
		}

		public void LogEventForMismatchedShipWindow()
		{
			OrderLines.ForEach(line => line.LogEventForMismatchedShipWindow(true));
		}

		void CheckPortConsistentWithLinkedSupplierBooking()
		{
			if (IsInDatabase && (JD_RL_NKGoodsAvailableAt != (ZString)JD_RL_NKGoodsAvailableAtInfo.OriginalValue || JD_RL_NKGoodsDeliveredTo != (ZString)JD_RL_NKGoodsDeliveredToInfo.OriginalValue))
			{
				var linkedSupplierBookingsInProgress = Factory.Load<JobSupplierBooking>(GetLinkedSupplierBookingInProgressQuery());
				foreach (var booking in linkedSupplierBookingsInProgress)
				{
					if (booking.JSB_RL_NKOrigin != JD_RL_NKGoodsAvailableAt)
					{
						booking.Logs.AddNew(Events.ExceptionRaised, "TYP=Origin|RES=Origin is different to the Origin of at least one of the Orders.");
					}

					if (booking.JSB_RL_NKDestination != JD_RL_NKGoodsDeliveredTo)
					{
						booking.Logs.AddNew(Events.ExceptionRaised, "TYP=Destination|RES=Destination is different to the Destination of at least one of the Orders.");
					}
				}
			}
		}

		void AddRequiredDocuments()
		{
			var departureEstimate = GetMilestoneEstimatedDate(Events.Departure).ToZDateTime();
			RequiredDocuments.AddAndAcquitRequiredDocuments(SupplierBuyerLink, null, null, null, departureEstimate);

			if (!this.IsExport())
			{
				RequiredDocuments.AddAndAcquitRequiredDocuments(Buyer, ActualPortOfLoading, ActualPortOfDischarge, Buyer, departureEstimate);
				RequiredDocuments.AddAndAcquitRequiredDocuments(Supplier, ActualPortOfLoading, ActualPortOfDischarge, Supplier, departureEstimate);
			}
			else
			{
				RequiredDocuments.AddAndAcquitRequiredDocuments(Supplier, ActualPortOfLoading, ActualPortOfDischarge, Supplier, departureEstimate);
				RequiredDocuments.AddAndAcquitRequiredDocuments(Buyer, ActualPortOfLoading, ActualPortOfDischarge, Buyer, departureEstimate);
			}

			JobRequiredDocumentDependentCollection.DirectionFilterType directionType = (this.IsDomestic()) ? JobRequiredDocumentDependentCollection.DirectionFilterType.Domestic : JobRequiredDocumentDependentCollection.DirectionFilterType.Both;
			RequiredDocuments.AddCountryRequiredDocuments(RefCountryRequiredDocumentSchema.RD_OnOrder, JD_TransportMode, JD_ContainerMode, directionType, ActualPortOfLoading, ActualPortOfDischarge);
		}

		void UpdateConfirmationDateFromOrderLines()
		{
			if (JD_BookingConfDate == ZDateTime.Empty)
			{
				bool allEntered = true;
				ZDateTime latestDate = ZDateTime.Empty;

				foreach (OrderLine line in OrderLines)
				{
					if (line.JO_ConfirmationDate.IsEmpty)
					{
						allEntered = false;
						break;
					}

					if (line.JO_ConfirmationDate > latestDate || latestDate == ZDateTime.Empty)
					{
						latestDate = line.JO_ConfirmationDate;
					}
				}

				if (allEntered)
				{
					JD_BookingConfDate = latestDate;
				}
			}
		}

		public ZString ActualPortOfLoading
		{
			get { return AttachedShipment_RL_NKOrigin != "" ? AttachedShipment_RL_NKOrigin : JD_RL_NKPortOfLoading; }
		}

		public ZString ActualPortOfDischarge
		{
			get { return AttachedShipment_RL_NKDestination != "" ? AttachedShipment_RL_NKDestination : JD_RL_NKPortOfDischarge; }
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		#endregion

		#region Delete

		protected virtual bool ShouldDeleteAllOrderSplitSiblings
		{
			get { return true; }
		}

		public override void Delete()
		{
			if (!this.IsDeleted)
			{
				RefreshOrderSplitSiblings();
				OrderCollection orderSplitSiblingsCollection = this.OrderSplitSiblings;
				byte splitNumber = this.JD_OrderNumberSplit;

				base.Delete();
				OrderLines.DeleteAll();
				PlannedContainers.RemoveAndDeleteAll();
				WorkflowItems.RemoveAndDeleteAll();
				RelatedChildActivityPivotCollection.DeleteAll();
				RelatedParentActivityPivotCollection.DeleteAll();

				if (ShouldDeleteAllOrderSplitSiblings)
				{
					foreach (Order splitSibling in new ArrayList(orderSplitSiblingsCollection))
					{
						if (!splitSibling.IsDeleted &&
							splitSibling.JD_OrderNumberSplit >= splitNumber)
						{
							splitSibling.Delete();
						}
					}
				}
			}
		}

		#endregion

		#region Clone

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			Order result = (Order)base.CloneInternal(args);
			foreach (OrderLine line in OrderLines.ToArray())
			{
				result.OrderLines.Add((OrderLine)line.Clone());
			}

			foreach (OrderContainer container in PlannedContainers.ToArray())
			{
				result.PlannedContainers.Add(container.Clone());
			}

			result.DocAddresses.RemoveAndDeleteAll(); // remove the addresses added by set default values.
			foreach (JobDocAddress address in DocAddresses)
			{
				result.DocAddresses.Add(address.Clone());
			}

			result.JD_OrderNumber = ZString.Empty;
			result.JD_OrderNumberSplit = new ZByte(0);

			return result;
		}

		void CopyMilestoneDates(Order target)
		{
			foreach (Event eventType in EventsToClone)
			{
				target.UpdateEventEstimate(eventType, GetMilestoneEstimatedDate(eventType).ToOffset());
				target.UpdateEvent(eventType, GetMilestoneActualDate(eventType).ToOffset());
			}
		}

		Event[] EventsToClone => new[]
		{
			Events.ExWorks,
			Events.GateIn,
			Events.Departure,
			Events.Arrival,
			Events.ExportCustomsCleared,
			Events.ExportCustomsCommenced,
			Events.CustomsCommenced,
			Events.CustomsCleared,
			Events.CargoAvailable,
			Events.DeliveryCartageAdvised,
			Events.DeliveryCartageCompleteFinalised
		};

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region Order Status

		public void AttemptToUpdateOrderStatus()
		{
			if (JD_OrderStatus != Constants.OrderStatus.Cancelled &&
				JD_OrderStatus != Constants.OrderStatus.Delivered &&
				IsStatusUpdateAllowed)
			{
				UpdateOrderStatus();
			}
		}

		bool IsStatusUpdateAllowed
		{
			get { return OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.Value && !IsStatusCustomisedByUser; }
		}

		bool IsStatusCustomisedByUser
		{
			get { return Buyer != null && new CodeDescriptionPairList(Buyer.MiscServ.OrderStatusList).ContainsCode(JD_OrderStatus); }
		}

		#endregion

		#region Events / Logs

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>();
				result.AddRange(OrderLines.Cast());

				if (Shipment != null)
				{
					result.AddRange(Shipment.Consols);
					result.Add(Shipment);
				}

				if (Declaration != null)
				{
					result.Add(Declaration);
				}

				if (PreAdvice != null)
				{
					result.Add(PreAdvice);
				}

				if (Notes != null)
				{
					result.AddRange(Notes.GetAllNotes());
				}

				if (RelatedWarehouseReceive != null)
				{
					result.Add(RelatedWarehouseReceive);
				}

				return result.ToArray();
			}
		}

		#endregion

		#region FillWithValidTestData
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			JD_OrderDate = new ZDateTime(2025, 1, 1);
			if (Buyer != null)
			{
				foreach (OrderLine orderLine in OrderLines)
				{
					foreach (OrderLineDelivery delivery in orderLine.Deliveries)
					{
						delivery.J4_OA_NKDeliveryPoint = Buyer.MainAddress.OA_Code;
					}
				}
			}
		}

		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper()
		{
			return new MyBusinessObjectTestDataHelper();
		}

		class MyBusinessObjectTestDataHelper : BusinessObjectTestDataHelper
		{
			protected override void FillDependentCollectionWithData(IBusinessObjectCollection collection, TestBusinessObjectKind kind, PropertyDescriptor collectionProperty, PropertyDescriptor[] propertyPath)
			{
				if (collectionProperty.Name != "RequiredDocuments")
				{
					base.FillDependentCollectionWithData(collection, kind, collectionProperty, propertyPath);
				}
			}
		}
#endif
		#endregion

		#region Related Business Objects

		#region OrderLines

		[ChildEditable(true)]
		public OrderLineCollection OrderLines
		{
			get
			{
				if (orderLines == null)
				{
					orderLines = GetOrderLinesCore();
					RegisterEditableChildObject(orderLines);
					OrderLines.CountChanged += new EventHandler(OnOrderLines_CountChanged);
				}
				return orderLines;
			}
		}
		protected OrderLineCollection orderLines;

		protected virtual OrderLineCollection GetOrderLinesCore()
		{
			return new OrderLineCollection(this);
		}

		void OnOrderLines_CountChanged(object sender, EventArgs e)
		{
			JD_Calc_LineCountInfo.RefreshBinding();
			JD_Calc_InnerPacksInfo.RefreshBinding();
			JD_Calc_OuterPacksInfo.RefreshBinding();
			JD_Calc_OuterPacksTypeInfo.RefreshBinding();
			JD_Calc_TotalQuantityInfo.RefreshBinding();
			JD_Calc_TotalQuantityInvoicedInfo.RefreshBinding();
			JD_Calc_TotalQuantityReceivedInfo.RefreshBinding();
			JD_Calc_TotalQuantityRemainingInfo.RefreshBinding();
			JD_Calc_TotalWeightInfo.RefreshBinding();
			JD_Calc_TotalWeightUnitInfo.RefreshBinding();
			JD_Calc_TotalVolumeInfo.RefreshBinding();
			JD_Calc_TotalVolumeUnitInfo.RefreshBinding();
		}

		public OrderLineCollection NonCancelledOrderLines
		{
			get
			{
				if (nonCancelledOrderLines == null)
				{
					ZQuery query = new ZQuery(JobOrderLineSchema.JO_JD, PK);
					query.AddToFilter(JobOrderLineSchema.JO_LineStatus, SQLComparisonOperator.NotEqual, Core.Constants.OrderStatus.Cancelled);
					nonCancelledOrderLines = new OrderLineCollection(Factory, query);
				}
				return nonCancelledOrderLines;
			}
		}
		OrderLineCollection nonCancelledOrderLines;

		[ChildEditable(true)]
		public OrderContainerCollection PlannedContainers
		{
			get
			{
				if (fPlannedContainers == null)
				{
					fPlannedContainers = new OrderContainerCollection(this, Factory);
					fPlannedContainers.Load();
					RegisterEditableChildObject(fPlannedContainers);
				}
				return fPlannedContainers;
			}
		}
		OrderContainerCollection fPlannedContainers;

		public bool OrderLineDeliveriesEditable = true;

		#endregion

		#region Contacts

		public OrgContactCollection Contacts
		{
			get
			{
				if (fContacts == null)
				{
					ZQuery filter = GetContactsFilter();
					filter.IsNoResultQuery = filter.IsEmpty; // don't want to load any contacts if no buyer or supplier / sending or receiving agent

					var lContacts = new OrgContactCollection(Factory, filter);
					lContacts.Load();
					fContacts = lContacts;
				}
				return fContacts;
			}
		}

		protected void InvalidateContactsList()
		{
			fContacts = null;
		}

		ZQuery GetContactsFilter()
		{
			ZQuery completeFilter = new ZQuery();
			if (BuyerPK.IsValid)
			{
				ZQuery buyerFilter = new ZQuery(OrgContactSchema.OC_OH, BuyerPK);
				completeFilter.AddToFilter(buyerFilter, JoinCondition.Or);
			}

			if (SupplierPK.IsValid)
			{
				ZQuery supplierFilter = new ZQuery(OrgContactSchema.OC_OH, SupplierPK);
				completeFilter.AddToFilter(supplierFilter, JoinCondition.Or);
			}

			if (JD_OH_SendingAgent.IsValid)
			{
				ZQuery sendingAgentFilter = new ZQuery(OrgContactSchema.OC_OH, JD_OH_SendingAgent);
				completeFilter.AddToFilter(sendingAgentFilter, JoinCondition.Or);
			}
			if (JD_OH_ReceivingAgent.IsValid)
			{
				ZQuery receivingAgentFilter = new ZQuery(OrgContactSchema.OC_OH, JD_OH_ReceivingAgent);
				completeFilter.AddToFilter(receivingAgentFilter, JoinCondition.Or);
			}
			return completeFilter;
		}

		OrgContactCollection fContacts;

		#endregion

		#region SupplierBuyerLink

		public OrgSupplierBuyerLink SupplierBuyerLink
		{
			get { return OrgSupplierBuyerLink.GetExistingOrgSupplierBuyerLink(Supplier, Buyer, CountryOfImport); }
		}

		public OrgSupplierBuyerLink SupplierBuyerLinkFromBuyerAddressCountry
		{
			get
			{
				var countryCode = BuyerAddress?.Country?.Code ?? ZString.Empty;
				return OrgSupplierBuyerLink.GetExistingOrgSupplierBuyerLink(Supplier, Buyer, countryCode, false);
			}
		}

		#endregion

		#region Voyages

		protected JobVoyage RetrieveVoyage(ZString vesselName, ZString voyage)
		{
			ZQuery filter = new ZQuery(
				JobVoyageSchema.JV_RV_NKVessel, SQLComparisonOperator.Equal, vesselName);
			filter.AddToFilter(
				JobVoyageSchema.JV_VoyageFlight, SQLComparisonOperator.Equal, voyage);

			JobVoyage result = (JobVoyage)Factory.LoadTop1(
				typeof(JobVoyage), filter);

			return result;
		}

		#endregion

		#region OrderSplitSiblings

		public virtual OrderCollection OrderSplitSiblings
		{
			get
			{
				if (orderSplitSiblings == null)
				{
					var filter = new ZQuery();
					filter.AddToFilter(JoinCondition.And, JobOrderHeaderSchema.JD_OrderNumber, SQLComparisonOperator.Equal, JD_OrderNumber);
					filter.AddToFilter(JoinCondition.And, JobOrderHeaderSchema.JD_OA_BuyerAddress, SQLComparisonOperator.Equal, Buyer == null ? Guid.Empty : Buyer.Addresses.Select(x => x.PK));
					filter.AddToFilter(JoinCondition.And, JobOrderHeaderSchema.JD_OrderNumberSplit, SQLComparisonOperator.NotEqual, JD_OrderNumberSplit);
					filter.AddToFilter(JoinCondition.And, JobOrderHeaderSchema.PK, SQLComparisonOperator.NotEqual, PK);
					orderSplitSiblings = new OrderCollection(Factory, filter);
				}

				return orderSplitSiblings;
			}
		}
		OrderCollection orderSplitSiblings;

		internal void RefreshOrderSplitSiblings()
		{
			orderSplitSiblings = null;
		}

		#endregion

		#region RequiredDocuments

		[ChildEditable(true)]
		public JobRequiredDocumentDependentCollection RequiredDocuments
		{
			get
			{
				if (fRequiredDocuments == null)
				{
					fRequiredDocuments = new JobRequiredDocumentDependentCollection(this, Factory);
					fRequiredDocuments.Load();
					RegisterEditableChildObject(fRequiredDocuments);
				}
				return fRequiredDocuments;
			}
		}
		JobRequiredDocumentDependentCollection fRequiredDocuments;

		#endregion

		#region Pre Advice

		public JobShipmentPreplanning PreAdvice
		{
			get { return Factory.Load<JobShipmentPreplanning>(JD_EF_ShipmentPrePlanning); }
		}

		[List("Lookups.ShipmentPrePlannings")]
		public override ZGuid JD_EF_ShipmentPrePlanning
		{
			get { return base.JD_EF_ShipmentPrePlanning; }
			set
			{
				if (base.JD_EF_ShipmentPrePlanning != value)
				{
					var preAdviceToBeDetached = PreAdvice;
					isDetachingFromPreAdvise = PreAdvice != null && value.IsEmpty;
					try
					{
						base.JD_EF_ShipmentPrePlanning = value;

						if (isDetachingFromPreAdvise)
						{
							Logs.AddDTCEvent(preAdviceToBeDetached.IsInDatabase, preAdviceToBeDetached.LogReference(true),
								new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Type, Constants.EventReferenceParameterTypes.ShipmentPreAdvice));

							if (ShouldDetachShipmentOrDeclarationOnDetachingPreAdvice())
							{
								if (!isDetachingFromShipment && IsShipmentAttached)
								{
									JD_JS = ZGuid.Empty;
								}
								else if (!isDetachingFromDeclaration && IsDeclarationAttached)
								{
									JD_JE = ZGuid.Empty;
								}
							}
						}
						else
						{
							Logs.CreateRecreateOrUpdateEventLog(Events.Attached, EstimateActual.Actual, ZDateTimeOffset.Now, PreAdvice != null ? PreAdvice.LogReference(true) : ZString.Empty,
								new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Type, Constants.EventReferenceParameterTypes.ShipmentPreAdvice));
						}
					}
					finally
					{
						isDetachingFromPreAdvise = false;
					}
				}
			}
		}

		bool isDetachingFromPreAdvise;

		bool ShouldDetachShipmentOrDeclarationOnDetachingPreAdvice()
		{
			if ((IsShipmentAttached && IsAllowedToDetachShipment) || IsDeclarationAttached)
			{
				var handler = OnDetachingPreAdviceAskToDetachShipmentOrDeclaration;
				if (handler != null)
				{
					var cancelArgs = new CancelEventArgs();
					handler(this, cancelArgs);
					return !cancelArgs.Cancel;
				}
			}

			return false;
		}

		public event CancelEventHandler OnDetachingPreAdviceAskToDetachShipmentOrDeclaration;

		#endregion

		#region Controlling Agent

		public JobDocAddress ControllingAgentDocAddress
		{
			get { return DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ControllingAgent); }
		}

		#endregion

		#region Controlling Customer

		public JobDocAddress ControllingCustomerDocAddress
		{
			get
			{
				JobDocAddress result = null;
				if (DocAddresses.FindByDocAddressType(DocAddressType.ControllingCustomer) == null)
				{
					result = DocAddresses.AddNew(DocAddressType.ControllingCustomer);
				}
				else
				{
					result = DocAddresses.FindByDocAddressType(DocAddressType.ControllingCustomer);
				}

				return result;
			}
		}

		public void DefaultControllingCustomer()
		{
			if (Buyer == null || ControllingCustomerDocAddress == null)
			{
				return;
			}

			if (!ControllingCustomerDocAddress.E2_AddressOverride)
			{
				var controllingCustomer = BuyerSupplierLinksHelper.GetControllingCustomer();

				if (controllingCustomer != null)
				{
					ControllingCustomerDocAddress.E2_OA_Address = controllingCustomer.MainAddress.PK;
				}
				else
				{
					var controllingCustomerParty = Buyer.AllRelatedParties.GetRelatedParty(RelatedPartyTypeList.Codes.ControllingCustomer, RelatedPartyDirectionList.Codes.Delivery);
					if (controllingCustomerParty?.RelatedParty != null)
					{
						ControllingCustomerDocAddress.E2_OA_Address = controllingCustomerParty.RelatedParty.MainAddress.PK;
					}
					else
					{
						ControllingCustomerDocAddress.Delete();
					}
				}
			}
		}

		public bool HasLinkedSupplierBookingInProgress
		{
			get => Factory.Load<JobSupplierBooking>(GetLinkedSupplierBookingInProgressQuery()).Any();
		}

		ZDBOnlyQuery GetLinkedSupplierBookingInProgressQuery()
		{
			var query = new ZDBOnlyQuery(typeof(JobSupplierBooking));
			query.AddToFilter(JobSupplierBookingSchema.JSB_Status, SQLComparisonOperator.NotEqual, Constants.SupplierBookingStatus.Cancelled);
			query.AddToFilter(JobSupplierBookingSchema.JSB_Status, SQLComparisonOperator.NotEqual, Constants.SupplierBookingStatus.Converted);

			var supplierBookingLineQuery = new ZDBOnlySubQuery(typeof(JobSupplierBookingLine), JobSupplierBookingSchema.PK, JobSupplierBookingLineSchema.JSL_JSB_Booking);

			var orderLineQuery = new ZDBOnlySubQuery(typeof(OrderLine), JobSupplierBookingLineSchema.JSL_JO_OrderLine);
			orderLineQuery.AddToFilter(JobOrderLineSchema.JO_JD, SQLComparisonOperator.Equal, PK);

			supplierBookingLineQuery.AddSubQuery(orderLineQuery, JoinCondition.And);
			query.AddSubQuery(supplierBookingLineQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#region Warehouse

		public OrgHeader Warehouse
		{
			get
			{
				OrgHeader result = null;

				var warehouseDocAddress = WarehouseDocAddress;
				if (!warehouseDocAddress.E2_AddressOverride)
				{
					result = warehouseDocAddress.Organisation;
				}

				return result;
			}
		}

		public OrgAddress WarehouseAddress
		{
			get
			{
				OrgAddress result = null;

				var warehouseDocAddress = WarehouseDocAddress;
				if (!warehouseDocAddress.E2_AddressOverride)
				{
					result = warehouseDocAddress.Address;
				}

				return result;
			}
		}

		public JobDocAddress WarehouseDocAddress
		{
			get { return DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.Warehouse); }
		}

		void DefaultWarehouseAddressFromBuyer()
		{
			if (!WarehouseDocAddress.E2_AddressOverride && WarehouseAddress == null)
			{
				var relatedPartyRecord = Buyer.AllRelatedParties.GetRelatedParty(RelatedPartyTypeList.Codes.Warehouse, RelatedPartyDirectionList.Codes.Delivery);
				if (relatedPartyRecord != null)
				{
					WarehouseDocAddress.E2_OA_Address = relatedPartyRecord.PR_OA;
				}
			}
		}

		#endregion

		#region RelatedWarehouseReceive

		public BusinessObject RelatedWarehouseReceive
		{
			get
			{
				if (relatedWarehouseReceive == null)
				{
					relatedWarehouseReceive = WarehouseReceiveHelper.GetRelatedWarehouseReceiveForPivot(Factory, PK, TablePrefix);
				}

				return relatedWarehouseReceive;
			}
		}

		BusinessObject relatedWarehouseReceive;

		#endregion

		#region ConsigneeDocumentaryAddress

		public JobDocAddress ConsigneeDocumentaryAddress
		{
			get { return DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ConsigneeDocumentaryAddress); }
		}

		#endregion

		#endregion

		#region Notify Party

		public JobDocAddress NotifyPartyDocAddress
		{
			get { return DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.NotifyParty); }
		}

		#endregion

		#region Notify Party 2

		public JobDocAddress NotifyParty2DocAddress
		{
			get { return DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.NotifyParty2); }
		}

		#endregion

		#region Notify Party 3

		public JobDocAddress NotifyParty3DocAddress
		{
			get { return DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.NotifyParty3); }
		}

		#endregion

		#region Property Overrides

		[List("JD_IncoTerm_List")]
		public override ZString JD_IncoTerm
		{
			get { return base.JD_IncoTerm; }
			set
			{
				base.JD_IncoTerm = value;

				if (!IsCopying)
				{
					SetIncludedInInvoiceFlagsForNonIncotermNeutralCharges();
				}
			}
		}

		void SetIncludedInInvoiceFlagsForNonIncotermNeutralCharges()
		{
			foreach (JobComInvCharge charge in Charges)
			{
				charge.ResetDefaultIsIncludedInAmountAndIsIncludedInITOTIfDetermined();
			}
		}

		[List("JD_RS_List")]
		public override ZString JD_RS_NKServiceLevel_NI
		{
			get { return base.JD_RS_NKServiceLevel_NI; }
			set { base.JD_RS_NKServiceLevel_NI = value; }
		}

		[List("JD_RL_List")]
		public override ZString JD_RL_NKGoodsAvailableAt
		{
			get { return base.JD_RL_NKGoodsAvailableAt; }
			set { base.JD_RL_NKGoodsAvailableAt = value; }
		}

		[List("JD_RL_List")]
		public override ZString JD_RL_NKGoodsDeliveredTo
		{
			get { return base.JD_RL_NKGoodsDeliveredTo; }
			set
			{
				var oldValue = JD_RL_NKGoodsDeliveredTo;
				base.JD_RL_NKGoodsDeliveredTo = value;
				if (!IsCopying && oldValue != JD_RL_NKGoodsDeliveredTo)
				{
					Charges.MarkAsNeedingValidation();
				}
			}
		}

		#region JD_BookingConfRef

		public override ZString JD_BookingConfRef
		{
			get { return base.JD_BookingConfRef; }
			set
			{
				base.JD_BookingConfRef = value;

				if (!IsCopying && !JD_BookingConfRefInfo.HasErrors())
				{
					if (!JD_BookingConfRef.IsEmpty && JD_BookingConfDate.IsEmpty)
					{
						JD_BookingConfDate = new ZDateTime(Env.Time.CurrentLocalDate);
					}
				}
			}
		}

		#endregion

		#region JD_InvoiceNumber

		public override ZString JD_InvoiceNumber
		{
			get { return base.JD_InvoiceNumber; }
			set
			{
				base.JD_InvoiceNumber = value;

				if (!JD_InvoiceNumberInfo.HasErrors())
				{
					if (JD_InvoiceNumber.IsValid && JD_InvoiceDate.IsEmpty && !IsCopying)
					{
						JD_InvoiceDate = Env.Time.CurrentLocalDate;
					}
				}
			}
		}

		#endregion

		#region JD_OA_BuyerAddress

		[List("BuyerList")]
		public override ZGuid JD_OA_BuyerAddress
		{
			get { return base.JD_OA_BuyerAddress; }
			set
			{
				if (base.JD_OA_BuyerAddress != value)
				{
					if (!IsCopying)
					{
						var newBuyerAddress = Factory.Load<OrgAddress>(value);
						if (newBuyerAddress != null)
						{
							JD_RL_NKPortOfDischarge = (JD_RL_NKPortOfDischarge.IsEmpty) ? newBuyerAddress.Header.OH_RL_NKClosestPort : JD_RL_NKPortOfDischarge;
							JD_RL_NKGoodsDeliveredTo = (JD_RL_NKGoodsDeliveredTo.IsEmpty) ? newBuyerAddress.Header.OH_RL_NKClosestPort : JD_RL_NKGoodsDeliveredTo;
						}
					}

					base.JD_OA_BuyerAddress = value;

					if (!IsCopying)
					{
						if (Buyer != null)
						{
							DefaultControllingCustomer();
							DefaultWarehouseAddressFromBuyer();
							DefaultGoodsDeliveredToAddress();

							if (JD_OrderNumber.IsEmpty)
							{
								JD_OrderNumber = TryGetNextOrderNumber();
							}
						}

						InvalidateContactsList();
						fJD_OrderStatus_List = null;
						if (!IsValidationSuspended)
						{
							Validation.ValidateJD_OA_SupplierAddress();
						}
					}

					RunDelayedUpdateDatesFromAttachedShipmentIfRequired();

					if (HasSufficientDataToCreateProcessTasks && !fIsImportingData)
					{
						EnsureProcessTasksAreCreated();
					}

					SetDefaultTolerancesForAllOrderLines();
				}

				OrderLines.MarkAsNeedingValidation();
				foreach (var orderLine in OrderLines)
				{
					orderLine.Deliveries.MarkAsNeedingValidation();
					foreach (var delivery in orderLine.Deliveries)
					{
						delivery.Containers.MarkAsNeedingValidation();
					}
				}
			}
		}

		protected bool JD_OA_BuyerAddress_ReadOnly
		{
			get { return JD_OrderNumberSplit > 0 || JD_EF_ShipmentPrePlanning.IsValid; }
		}

		#endregion

		#region JD_OC_BuyerContact

		[RelatedBusinessObject("BuyerContact")]
		public override ZGuid JD_OC_BuyerContact
		{
			get { return base.JD_OC_BuyerContact; }
			set { base.JD_OC_BuyerContact = value; }
		}

		public override OrgContact BuyerContact
		{
			get { return Factory.Load<OrgContact>(JD_OC_BuyerContact); }
		}

		#endregion

		#region BuyerZAddressWithContact

		public ZAddressWithContact BuyerZAddressWithContact => buyerZAddressWithContact ?? (buyerZAddressWithContact = GetBuyerZAddressWithContact());

		ZAddressWithContact buyerZAddressWithContact;

		ZAddressWithContact GetBuyerZAddressWithContact()
		{
			var result = new ZAddressWithContact(JD_OC_BuyerContactInfo, JD_OA_BuyerAddressInfo);
			result.DefaultAddressType = AddressType.OFC;
			result.GetDefaultAddress = GetDefaultBuyerAddress;
			return result;
		}

		static ZGuid GetDefaultBuyerAddress(IOrgHeader org)
		{
			OrgAddress result = null;
			var header = org as OrgHeader;
			if (header != null && header.AddressesActive.Any())
			{
				var finder = new OrgAddressDefaultFinder(IBusinessObjectCollectionExtensions.ToArray<OrgAddress>(header.AddressesActive), true, null);
				result = finder.DefaultAddressOfType(OrgAddressType.Office);
			}

			return result == null ? ZGuid.Empty : result.PK;
		}

		#endregion

		#region BuyerPK

		[List("BuyerList")]
		[RelatedBusinessObject("Buyer")]
		public virtual ZGuid BuyerPK
		{
			get { return !IsDeleted && BuyerAddress != null ? BuyerAddress.OA_OH : ZGuid.Empty; }
			set
			{
				var org = Factory.Load<OrgHeader>(value);
				JD_OA_BuyerAddress = GetDefaultBuyerAddress(org);
				BuyerPKInfo.RefreshBinding();
				RequiredDocuments.SetAllDocumentsReceivedEventLogger();
			}
		}

		public ZPropertyInfo BuyerPKInfo
		{
			get { return GetZPropertyInfo(nameof(BuyerPK)); }
		}

		public OrgHeader Buyer
		{
			get { return Factory.Load<OrgHeader>(BuyerPK); }
		}

		#endregion

		#region JD_OA_SupplierAddress

		[List("SupplierList")]
		public override ZGuid JD_OA_SupplierAddress
		{
			get { return base.JD_OA_SupplierAddress; }
			set
			{
				if (base.JD_OA_SupplierAddress != value)
				{
					if (!IsCopying)
					{
						var newSupplierAddress = Factory.Load<OrgAddress>(value);
						if (newSupplierAddress != null)
						{
							JD_RL_NKPortOfLoading = (JD_RL_NKPortOfLoading.IsEmpty) ? newSupplierAddress.Header.OH_RL_NKClosestPort : JD_RL_NKPortOfLoading;
							JD_RL_NKGoodsAvailableAt = (JD_RL_NKGoodsAvailableAt.IsEmpty) ? newSupplierAddress.Header.OH_RL_NKClosestPort : JD_RL_NKGoodsAvailableAt;
						}
					}

					SetSupplierAddressWithoutCalculatedDefaults(value);

					if (!IsCopying)
					{
						DefaultControllingCustomer();
						DefaultGoodsAvailableAtAddress();

						InvalidateContactsList();
						if (!IsValidationSuspended)
						{
							Validation.ValidateJD_OA_BuyerAddress();
						}
					}

					SetDefaultTolerancesForAllOrderLines();
				}

				OrderLines.MarkAsNeedingValidation();
			}
		}

		protected void SetSupplierAddressWithoutCalculatedDefaults(ZGuid value)
		{
			base.JD_OA_SupplierAddress = value;
		}

		#endregion

		#region SupplierZAddressWithContact

		public ZAddressWithContact SupplierZAddressWithContact => supplierZAddressWithContact ?? (supplierZAddressWithContact = GetSupplierZAddressWithContact());

		ZAddressWithContact supplierZAddressWithContact;

		ZAddressWithContact GetSupplierZAddressWithContact()
		{
			var result = new ZAddressWithContact(JD_OC_SupplierContactInfo, JD_OA_SupplierAddressInfo);
			result.DefaultAddressType = AddressType.OFC;
			result.GetDefaultAddress = GetDefaultSupplierAddress;
			return result;
		}

		static ZGuid GetDefaultSupplierAddress(IOrgHeader org)
		{
			OrgAddress result = null;
			var header = org as OrgHeader;
			if (header != null && header.AddressesActive.Any())
			{
				var finder = new OrgAddressDefaultFinder(IBusinessObjectCollectionExtensions.ToArray<OrgAddress>(header.AddressesActive), true, null);
				result = finder.DefaultAddressOfType(OrgAddressType.Office);
			}

			return result == null ? ZGuid.Empty : result.PK;
		}

		#endregion

		#region SupplierPK

		[List("SupplierList")]
		[RelatedBusinessObject("Supplier")]
		public virtual ZGuid SupplierPK
		{
			get { return !IsDeleted && SupplierAddress != null ? SupplierAddress.OA_OH : ZGuid.Empty; }
			set
			{
				var org = Factory.Load<OrgHeader>(value);
				JD_OA_SupplierAddress = GetDefaultBuyerAddress(org);
				SupplierPKInfo.RefreshBinding();
				RequiredDocuments.SetAllDocumentsReceivedEventLogger();
			}
		}

		public ZPropertyInfo SupplierPKInfo
		{
			get { return GetZPropertyInfo(nameof(SupplierPK)); }
		}

		public OrgHeader Supplier
		{
			get { return Factory.Load<OrgHeader>(SupplierPK); }
		}

		#endregion

		#region JD_OH_SendingAgent

		[ReadOnlyMember(nameof(IsLinkedToPreAdvice))]
		[List("JD_OH_SendingAgents_List")]
		public override ZGuid JD_OH_SendingAgent
		{
			get { return base.JD_OH_SendingAgent; }
			set
			{
				base.JD_OH_SendingAgent = value;
				InvalidateContactsList();
				if (!IsValidationSuspended)
				{
					Validation.ValidateJD_OH_ReceivingAgent();
				}
			}
		}

		#endregion

		#region JD_OH_ReceivingAgent

		[ReadOnlyMember(nameof(IsLinkedToPreAdvice))]
		[List("JD_OH_ReceivingAgents_List")]
		public override ZGuid JD_OH_ReceivingAgent
		{
			get { return base.JD_OH_ReceivingAgent; }
			set
			{
				base.JD_OH_ReceivingAgent = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateJD_OH_SendingAgent();
				}
			}
		}

		#endregion

		#region JD_ContainerMode

		[List("JD_ContainerMode_List")]
		public override ZString JD_ContainerMode
		{
			get { return base.JD_ContainerMode; }
			set
			{
				base.JD_ContainerMode = value;
				JD_PlannedContainersVisibleInfo.RefreshBinding();

				if (!JD_PlannedContainersVisible)
				{
					PlannedContainers.RemoveAndDeleteAll();
				}
			}
		}

		#endregion

		#region JD_RL_NKPortOfLoading

		[ReadOnlyMember(nameof(IsLinkedToPreAdvice))]
		[List("JD_RL_List")]
		public override ZString JD_RL_NKPortOfLoading
		{
			get { return base.JD_RL_NKPortOfLoading; }
			set
			{
				base.JD_RL_NKPortOfLoading = value;
				RequiredDocuments.SetAllDocumentsReceivedEventLogger();
			}
		}

		#endregion

		#region JD_RL_NKPortOfDischarge

		[ReadOnlyMember(nameof(IsLinkedToPreAdvice))]
		[List("JD_RL_List")]
		public override ZString JD_RL_NKPortOfDischarge
		{
			get { return base.JD_RL_NKPortOfDischarge; }
			set
			{
				base.JD_RL_NKPortOfDischarge = value;
				RequiredDocuments.SetAllDocumentsReceivedEventLogger();
			}
		}

		#endregion

		#region JD_TransportMode

		[List("JD_TransportMode_List")]  // This is a lookups list rather than a column name
		public override ZString JD_TransportMode
		{
			get { return base.JD_TransportMode; }
			set
			{
				if (base.JD_TransportMode != value)
				{
					base.JD_TransportMode = value;

					if (!IsSettingDefaults)
					{
						((IDefaultNumberOfDecimalsSupporter)this).RoundMeasurePropertiesOnTransportModeChanged();
					}

					if (!IsCopying)
					{
						if (JD_ContainerMode_List.Count > 0)
						{
							JD_ContainerMode = JD_ContainerMode_List[0].Code;
						}
					}

					if (IsAirTransport)
					{
						JD_RV_NKArrivalVessel = ZString.Empty;
						JD_RV_NKIntermediateVessel = ZString.Empty;
						JD_RV_NKDepartureVessel = ZString.Empty;
					}

					InvalidateContactsList();

					RunDelayedUpdateDatesFromAttachedShipmentIfRequired();

					if (HasSufficientDataToCreateProcessTasks && !fIsImportingData)
					{
						EnsureProcessTasksAreCreated();
					}

					SetDefaultTolerancesForAllOrderLines();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJD_RV_NKArrivalVessel();
						Validation.ValidateJD_RV_NKDepartureVessel();
						Validation.ValidateJD_RV_NKIntermediateVessel();
					}
				}
			}
		}

		#endregion

		#region JD_RV_NKDepartureVessel

		[List("JD_RV_Vessel_List")]
		public override ZString JD_RV_NKDepartureVessel
		{
			get { return base.JD_RV_NKDepartureVessel; }
			set
			{
				bool hasChanges = base.JD_RV_NKDepartureVessel != value;
				base.JD_RV_NKDepartureVessel = value;
				if (hasChanges)
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateJD_RV_NKIntermediateVessel();
						Validation.ValidateJD_DepartureVoyage();
					}
					RetrieveSailingForDeparture();

					if (PlanningVoyageState != PlanningVoyageState.ThreeVoyage &&
						!value.IsEmpty &&
						JD_RV_NKArrivalVessel.IsEmpty)
					{
						JD_RV_NKArrivalVessel = value;
					}
				}
			}
		}

		#endregion

		#region JD_RV_NKIntermediateVessel

		[List("JD_RV_Vessel_List")]
		public override ZString JD_RV_NKIntermediateVessel
		{
			get { return base.JD_RV_NKIntermediateVessel; }
			set
			{
				if (base.JD_RV_NKIntermediateVessel != value)
				{
					base.JD_RV_NKIntermediateVessel = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateJD_IntermediateVoyage();
					}
					RetrieveSailingForIntermediate();
				}
			}
		}

		#endregion

		#region JD_RV_NKArrivalVessel

		[List("JD_RV_Vessel_List")]
		public override ZString JD_RV_NKArrivalVessel
		{
			get { return base.JD_RV_NKArrivalVessel; }
			set
			{
				if (base.JD_RV_NKArrivalVessel != value)
				{
					base.JD_RV_NKArrivalVessel = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateJD_RV_NKDepartureVessel();
						Validation.ValidateJD_RV_NKIntermediateVessel();
						Validation.ValidateJD_ArrivalVoyage();
					}
					RetrieveSailingForArrival();

					if (PlanningVoyageState != PlanningVoyageState.ThreeVoyage &&
						!value.IsEmpty &&
						JD_RV_NKDepartureVessel.IsEmpty)
					{
						JD_RV_NKDepartureVessel = value;
					}
				}
			}
		}

		#endregion

		#region JD_DepartureVoyage

		public override ZString JD_DepartureVoyage
		{
			get { return base.JD_DepartureVoyage; }
			set
			{
				bool hasChanges = base.JD_DepartureVoyage != value;
				base.JD_DepartureVoyage = value;
				if (hasChanges)
				{
					RetrieveSailingForDeparture();
					RefreshVoyagesReadOnly();

					if (PlanningVoyageState != PlanningVoyageState.ThreeVoyage &&
						!value.IsEmpty &&
						JD_ArrivalVoyage.IsEmpty)
					{
						JD_ArrivalVoyage = value;
					}
				}
			}
		}

		#endregion

		#region JD_IntermediateVoyage

		[ReadOnlyMember(nameof(AllVoyagesEmpty))]
		public override ZString JD_IntermediateVoyage
		{
			get { return base.JD_IntermediateVoyage; }
			set
			{
				if (base.JD_IntermediateVoyage != value)
				{
					base.JD_IntermediateVoyage = value;

					ClearUnusedVoyageDetails();
					RefreshVoyagesReadOnly();
					RetrieveSailingForIntermediate();
				}
			}
		}

		#endregion

		#region JD_ArrivalVoyage

		[ReadOnlyMember(nameof(AllVoyagesEmpty))]
		public override ZString JD_ArrivalVoyage
		{
			get { return base.JD_ArrivalVoyage; }
			set
			{
				if (base.JD_ArrivalVoyage != value)
				{
					base.JD_ArrivalVoyage = value;

					ClearUnusedVoyageDetails();
					RefreshVoyagesReadOnly();
					RetrieveSailingForArrival();

					if (PlanningVoyageState != PlanningVoyageState.ThreeVoyage &&
						!value.IsEmpty &&
						JD_DepartureVoyage.IsEmpty)
					{
						JD_DepartureVoyage = value;
					}
				}
			}
		}

		#endregion

		[List("JD_UnitOfWeight_List")]
		public override ZString JD_UnitOfWeight
		{
			get { return base.JD_UnitOfWeight; }
			set
			{
				base.JD_UnitOfWeight = value;
				this.SetRoundedValue(JobOrderHeaderSchema.JD_ActualWeight, JD_ActualWeightInfo);
			}
		}

		[List("JD_UnitOfVolume_List")]
		public override ZString JD_UnitOfVolume
		{
			get { return base.JD_UnitOfVolume; }
			set
			{
				base.JD_UnitOfVolume = value;
				this.SetRoundedValue(JobOrderHeaderSchema.JD_ActualVolume, JD_ActualVolumeInfo);
			}
		}

		[List("JD_F3_NKPackType_List")]
		public override ZString JD_F3_NKPackType
		{
			get { return base.JD_F3_NKPackType; }
			set { base.JD_F3_NKPackType = value; }
		}

		#region JD_JS

		[ReadOnlyMember(nameof(IsLinkedToPreAdvice))]
		[List("JD_Shipment_List")]
		public override ZGuid JD_JS
		{
			get { return base.JD_JS; }
			set
			{
				if (base.JD_JS != value)
				{
					var shipmentToBeDetached = Shipment;

					if (!isUpdatingJD_JS)
					{
						isUpdatingJD_JS = true;
						try
						{
							isDetachingFromShipment = IsShipmentAttached && value.IsEmpty;

							base.JD_JS = value;

							try
							{
								if (isDetachingFromShipment)
								{
									UnattachWorkflowLinkBetweenOrderAndShipment(this, shipmentToBeDetached);

									foreach (var orderLine in OrderLines)
									{
										orderLine.ClearLinksToPackProducts();
									}

									ClearShipmentProxyDates(shipmentToBeDetached);

									if (!isDetachingFromPreAdvise)
									{
										JD_EF_ShipmentPrePlanning = ZGuid.Empty;
									}

									if ((JD_OrderStatus == Constants.OrderStatus.Shipped || JD_OrderStatus == Constants.OrderStatus.Delivered)
										&& IsStatusUpdateAllowed)
									{
										JD_OrderStatus = Constants.OrderStatus.Incomplete;
									}

									Logs.AddDTCEvent(shipmentToBeDetached.IsInDatabase, shipmentToBeDetached.LogReference(true)
										, new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Type, Constants.EventReferenceParameterTypes.Shipment));
								}
								else
								{
									AttachWorkflowLinkBetweenOrderAndShipment(this, Shipment);
								}
							}
							finally
							{
								isDetachingFromShipment = false;
							}

							UpdateDatesFromAttachedShipment();

							AttachMostRecentDeclaration();

							Logs.CreateRecreateOrUpdateEventLog(Events.OrderShipped, EstimateActual.Actual, value.IsValid ? ZDateTimeOffset.Now : ZDateTimeOffset.Empty);

							OrderLines.MarkAsNeedingValidation();
							Charges.MarkAsNeedingValidation();
							RefreshOrderNumberReadOnly();
							UnRegisterEditableChildObject(shipmentToBeDetached);

							JD_JS_ThatAutoUpdatesShipmentDetailsFromOrderInfo.RefreshBinding();
							JD_VB_ThatAutoUpdatesBookingDetailsFromOrderInfo.RefreshBinding();

							RefreshConsolsForBinding();
						}
						finally
						{
							isUpdatingJD_JS = false;
						}
					}
				}
			}
		}

		bool isDetachingFromShipment;
		bool isUpdatingJD_JS;

		void AttachMostRecentDeclaration()
		{
			BusinessObject declaration = null;
			ZDateTime currentDeclarationTime = ZDateTime.Empty;
			if (Shipment != null)
			{
				foreach (BusinessObject shipmentDeclaration in Shipment.Declarations)
				{
					if (declaration == null)
					{
						StmALog mostRecentEvent = shipmentDeclaration.GetLogs().MostRecentLogByEventTime(Events.AddedARecordToTheSystem);
						if (mostRecentEvent == null || currentDeclarationTime.IsEmpty || mostRecentEvent.SL_EventTime > currentDeclarationTime)
						{
							declaration = shipmentDeclaration;
							if (mostRecentEvent != null)
							{
								currentDeclarationTime = mostRecentEvent.SL_EventTime;
							}
						}
					}
				}
			}

			if (declaration != null && IsCopying)
			{
				JD_JE = declaration.PK;
			}

			if (!IsCopying)
			{
				((IBusinessObjectInternals)this).IsCopying = true;
				try
				{
					JD_JE = (declaration == null) ? ZGuid.Empty : declaration.PK;
				}
				finally
				{
					((IBusinessObjectInternals)this).IsCopying = false;
				}
			}
		}

		public bool IsAllowedToDetachShipment
		{
			get
			{
				return Shipment != null && !Shipment.HasChanges;
			}
		}

		void AttachWorkflowLinkBetweenOrderAndShipment(Order order, ForwardingShipment shipmentToAttach)
		{
			if (shipmentToAttach != null)
			{
				ObjectFactory.Get<IWorkflowProvidersLinkageService>().WorkflowProvidersLinked(order, shipmentToAttach, Factory);
			}
		}

		void UnattachWorkflowLinkBetweenOrderAndShipment(Order order, ForwardingShipment shipmentToDetach)
		{
			if (shipmentToDetach != null)
			{
				ObjectFactory.Get<IWorkflowProvidersLinkageService>().WorkflowProvidersUnLinked(order, shipmentToDetach, Factory);
			}
		}
		#endregion

		#region JD_JE

		[ReadOnlyMember(nameof(IsLinkedToPreAdvice))]
		[List("JD_Declaration_List")]
		public override ZGuid JD_JE
		{
			get { return base.JD_JE; }
			set
			{
				if (JD_JE != value)
				{
					bool wasDeclarationAttached = IsDeclarationAttached;
					isDetachingFromDeclaration = IsDeclarationAttached && value.IsEmpty;

					var declarationToBeDetached = Declaration;
					base.JD_JE = value;

					try
					{
						if (isDetachingFromDeclaration && !isDetachingFromPreAdvise)
						{
							JD_EF_ShipmentPrePlanning = ZGuid.Empty;
						}
					}
					finally
					{
						isDetachingFromDeclaration = false;
					}

					if (!IsCopying)
					{
						if (!IsShipmentAttached)
						{
							UpdateDatesFromAttachedDeclaration();
						}

						if (GetMilestoneEstimatedDate(Events.CustomsCommenced).IsEmpty)
						{
							UpdateEventEstimate(Events.CustomsCommenced, DateTimeCreationOfDeclaration.ToOffset());
						}

						BusinessObject newDeclaration = (BusinessObject)Factory.Load<Enterprise.Integration.Customs.IBaseJobDeclaration>(value);

						((IBusinessObjectInternals)this).IsCopying = true;
						try
						{
							JD_JS = (newDeclaration == null) ? ZGuid.Empty : (ZGuid)newDeclaration[JobDeclarationSchema.JE_JS.Name];
						}
						finally
						{
							((IBusinessObjectInternals)this).IsCopying = false;
						}
					}

					if (JD_JE.IsEmpty && wasDeclarationAttached)
					{
						ClearDeclarationProxiedDates(declarationToBeDetached);
						ClearDeclarationOrderLines(declarationToBeDetached);
					}

					Logs.CreateRecreateOrUpdateEventLog(Events.OrderShipped, EstimateActual.Actual, value.IsValid ? ZDateTimeOffset.Now : ZDateTimeOffset.Empty);
				}
			}
		}

		bool isDetachingFromDeclaration;

		#endregion

		#region JD_OrderNumber

		public override ZString JD_OrderNumber
		{
			get { return base.JD_OrderNumber; }
			set
			{
				if (!JD_OrderNumber.IsEmpty && OrderSplitSiblings.Count > 0 && OrderNumberChangeAttemptedWhileSplitsExist != null)
				{
					OrderNumberChangeAttemptedWhileSplitsExist(this, EventArgs.Empty);
				}
				else
				{
					base.JD_OrderNumber = value;
				}
			}
		}

		public event EventHandler OrderNumberChangeAttemptedWhileSplitsExist;

		#endregion

		#region JD_OrderNumberSplit

		[ReadOnly(true)]
		public override ZByte JD_OrderNumberSplit
		{
			get { return base.JD_OrderNumberSplit; }
			set
			{
				base.JD_OrderNumberSplit = value;
				RefreshOrderNumberReadOnly();
			}
		}

		#endregion

		[ReadOnlyMember(nameof(IsLinkedToPreAdvice))]
		public override ZString JD_MasterWaybill
		{
			get { return base.JD_MasterWaybill; }
			set { base.JD_MasterWaybill = value; }
		}

		[ReadOnlyMember(nameof(IsLinkedToPreAdvice))]
		[List("JD_OH_Carriers_List")]
		public override ZGuid JD_OH_Carrier
		{
			get { return base.JD_OH_Carrier; }
			set { base.JD_OH_Carrier = value; }
		}

		public bool IsLinkedToPreAdvice
		{
			get { return !JD_EF_ShipmentPrePlanning.IsEmpty; }
		}

		protected bool JD_Waybill_ReadOnly
		{
			get { return !JD_EF_ShipmentPrePlanning.IsEmpty && PreAdvice != null && JD_Waybill == PreAdvice.EF_HouseBill; }
		}

		[MeasureUnit(Schema.JD_UnitOfWeight, MeasureUnitType.Weight)]
		public override ZDecimal JD_ActualWeight
		{
			get { return base.JD_ActualWeight; }
			set { base.JD_ActualWeight = this.GetRoundedValue(JobOrderHeaderSchema.JD_ActualWeight, JD_ActualWeightInfo, value); }
		}

		[MeasureUnit(Schema.JD_UnitOfVolume, MeasureUnitType.Volume)]
		public override ZDecimal JD_ActualVolume
		{
			get { return base.JD_ActualVolume; }
			set { base.JD_ActualVolume = this.GetRoundedValue(JobOrderHeaderSchema.JD_ActualVolume, JD_ActualVolumeInfo, value); }
		}

		#endregion

		#region New Properties

		public bool HasBeenExported
		{
			get { return Logs.MostRecentLogByEventTime(Events.DataExport) != null; }
		}

		public bool IsExemptedFromCustomsAndQuarantineFees
		{
			get { return Buyer != null && Buyer.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.AustraliaCodeTypes.Diplomat, Core.Constants.CountryCodes.Australia) != null; }
		}

		#region Weight
		public ZPropertyInfo WeightInfo
		{
			get { return GetZPropertyInfo(nameof(Weight)); }
		}

		public ZDecimal Weight
		{
			get
			{
				var result = ZDecimal.Zero;

				if (Shipment != null)
				{
					result = Shipment.JS_ActualWeight;
				}
				else if (Declaration != null)
				{
					result = new ZDecimal(Declaration[JobDeclarationSchema.Constants.JE_TotalWeight]);
				}
				else
				{
					result = JD_ActualWeight;
				}

				return this.GetRoundedValue(WeightInfo, result);
			}
		}
		#endregion

		#region Weight Unit
		public ZPropertyInfo WeightUnitInfo
		{
			get { return GetZPropertyInfo(nameof(WeightUnit)); }
		}

		public ZString WeightUnit
		{
			get
			{
				if (Shipment != null)
				{
					return Shipment.JS_UnitOfWeight;
				}
				else if (Declaration != null)
				{
					return Declaration[JobDeclarationSchema.Constants.JE_TotalWeightUnit].ToString();
				}
				else
				{
					return JD_UnitOfWeight;
				}
			}
		}

		#endregion

		#region Volume
		public ZPropertyInfo VolumeInfo
		{
			get { return GetZPropertyInfo(nameof(Volume)); }
		}

		public ZDecimal Volume
		{
			get
			{
				var result = ZDecimal.Zero;

				if (Shipment != null)
				{
					result = Shipment.JS_ActualVolume;
				}
				else if (Declaration != null)
				{
					result = new ZDecimal(Declaration[JobDeclarationSchema.Constants.JE_TotalVolume]);
				}
				else
				{
					result = JD_ActualVolume;
				}

				return this.GetRoundedValue(VolumeInfo, result);
			}
		}
		#endregion

		#region VolumeUnit
		public ZPropertyInfo VolumeUnitInfo
		{
			get { return GetZPropertyInfo(nameof(VolumeUnit)); }
		}

		public ZString VolumeUnit
		{
			get
			{
				if (Shipment != null)
				{
					return Shipment.JS_UnitOfVolume;
				}
				else if (Declaration != null)
				{
					return Declaration[JobDeclarationSchema.Constants.JE_TotalVolumeUnit].ToString();
				}
				else
				{
					return JD_UnitOfVolume;
				}
			}
		}
		#endregion

		#region Packs
		public ZPropertyInfo PacksInfo
		{
			get { return GetZPropertyInfo(nameof(Packs)); }
		}

		public ZInt Packs
		{
			get
			{
				if (Shipment != null)
				{
					return Shipment.JS_OuterPacks;
				}
				else if (Declaration != null)
				{
					return new ZInt(Declaration[JobDeclarationSchema.Constants.JE_TotalNoOfPacks]);
				}
				else
				{
					return JD_Packs;
				}
			}
		}
		#endregion

		#region Pack Type

		public ZPropertyInfo JD_Calc_PackTypeInfo
		{
			get { return GetZPropertyInfo(Schema.JD_Calc_PackType); }
		}

		public ZString JD_Calc_PackType
		{
			get
			{
				if (Shipment != null)
				{
					return Shipment.JS_F3_NKPackType;
				}
				else if (Declaration != null)
				{
					return Declaration[JobDeclarationSchema.Constants.JE_TotalNoOfPacksPackType].ToString();
				}
				else
				{
					return JD_F3_NKPackType;
				}
			}
		}

		#endregion

		#region JD_Calc_InnerPacks

		public ZInt JD_Calc_InnerPacks
		{
			get
			{
				if (jd_Calc_InnerPacks == null)
				{
					jd_Calc_InnerPacks = new CachedProperty<ZInt>(Factory, delegate
						{
							ZInt result = 0;
							foreach (OrderLine orderLine in OrderLines)
							{
								result += (ZInt)orderLine.JO_InnerPacks;
							}

							return result;
						});
				}

				return jd_Calc_InnerPacks.Value;
			}
		}
		CachedProperty<ZInt> jd_Calc_InnerPacks;

		public ZPropertyInfo JD_Calc_InnerPacksInfo
		{
			get { return GetZPropertyInfo(Schema.JD_Calc_InnerPacks); }
		}

		#endregion

		#region JC_Calc_InnerPackType

		public ZString JD_Calc_InnerPackType
		{
			get
			{
				ZString result = "";
				foreach (OrderLine line in OrderLines)
				{
					result = CombinePackTypes(result, line.JO_InnerPacksUQ);
				}

				return result.IsEmpty ? (ZString)Constants.PkgUnit.Package : result;
			}
		}

		public ZPropertyInfo JD_Calc_InnerPackTypeInfo
		{
			get { return GetZPropertyInfo(Schema.JD_Calc_InnerPackType); }
		}

		ZString CombinePackTypes(ZString packType1, ZString packType2)
		{
			if (packType1.IsEmpty)
			{
				return packType2;
			}
			else if (packType2.IsEmpty || packType1 == packType2)
			{
				return packType1;
			}
			else
			{
				return Constants.PkgUnit.Package;
			}
		}

		#endregion

		#region JD_Calc_OuterPacks

		public ZInt JD_Calc_OuterPacks
		{
			get
			{
				if (jd_Calc_OuterPacks == null)
				{
					jd_Calc_OuterPacks = new CachedProperty<ZInt>(Factory, delegate
						{
							ZInt result = 0;
							foreach (OrderLine orderLine in OrderLines)
							{
								result += (ZInt)orderLine.JO_OuterPacks;
							}

							return result;
						});
				}

				return jd_Calc_OuterPacks.Value;
			}
		}
		CachedProperty<ZInt> jd_Calc_OuterPacks;

		public ZPropertyInfo JD_Calc_OuterPacksInfo
		{
			get { return GetZPropertyInfo(Schema.JD_Calc_OuterPacks); }
		}

		#endregion

		#region JD_Calc_OuterPacksType

		public ZString JD_Calc_OuterPacksType
		{
			get
			{
				ZString result = "";
				foreach (var orderline in OrderLines)
				{
					result = CombinePackTypes(result, orderline.JO_OuterPacksUQ);
				}

				return result.IsEmpty ? (ZString)Constants.PkgUnit.Package : result;
			}
		}

		public ZPropertyInfo JD_Calc_OuterPacksTypeInfo
		{
			get { return GetZPropertyInfo(Schema.JD_Calc_OuterPacksType); }
		}

		#endregion

		#region JD_Calc_TotalQuantity

		[DecimalPlaces(5)]
		public ZDecimal JD_Calc_TotalQuantity
		{
			get
			{
				if (jd_Calc_TotalQuantity == null)
				{
					jd_Calc_TotalQuantity = new CachedProperty<ZDecimal>(Factory, delegate
						{
							ZDecimal result = 0M;
							foreach (OrderLine orderLine in OrderLines)
							{
								result += orderLine.JO_Quantity;
							}

							return result;
						});
				}

				return jd_Calc_TotalQuantity.Value;
			}
		}
		CachedProperty<ZDecimal> jd_Calc_TotalQuantity;

		public ZPropertyInfo JD_Calc_TotalQuantityInfo
		{
			get { return GetZPropertyInfo(Schema.JD_Calc_TotalQuantity); }
		}

		#endregion

		#region JD_Calc_TotalQuantityInvoiced

		[DecimalPlaces(5)]
		public ZDecimal JD_Calc_TotalQuantityInvoiced
		{
			get
			{
				if (jd_Calc_TotalQuantityInvoiced == null)
				{
					jd_Calc_TotalQuantityInvoiced = new CachedProperty<ZDecimal>(Factory, delegate
						{
							ZDecimal result = 0M;
							foreach (OrderLine orderLine in OrderLines)
							{
								result += orderLine.JO_QtyInvoiced;
							}

							return result;
						});
				}

				return jd_Calc_TotalQuantityInvoiced.Value;
			}
		}
		CachedProperty<ZDecimal> jd_Calc_TotalQuantityInvoiced;

		public ZPropertyInfo JD_Calc_TotalQuantityInvoicedInfo
		{
			get { return GetZPropertyInfo(Schema.JD_Calc_TotalQuantityInvoiced); }
		}

		#endregion

		#region JD_Calc_TotalQuantityReceived

		[DecimalPlaces(5)]
		public ZDecimal JD_Calc_TotalQuantityReceived
		{
			get
			{
				if (jd_Calc_TotalQuantityReceived == null)
				{
					jd_Calc_TotalQuantityReceived = new CachedProperty<ZDecimal>(Factory, delegate
						{
							ZDecimal result = 0M;
							foreach (OrderLine orderLine in OrderLines)
							{
								result += orderLine.JO_QtyReceived;
							}

							return result;
						});
				}

				return jd_Calc_TotalQuantityReceived.Value;
			}
		}
		CachedProperty<ZDecimal> jd_Calc_TotalQuantityReceived;

		public ZPropertyInfo JD_Calc_TotalQuantityReceivedInfo
		{
			get { return GetZPropertyInfo(Schema.JD_Calc_TotalQuantityReceived); }
		}

		#endregion

		#region JD_Calc_TotalQuantityRemaining

		[DecimalPlaces(5)]
		public ZDecimal JD_Calc_TotalQuantityRemaining
		{
			get
			{
				if (jd_Calc_TotalQuantityRemaining == null)
				{
					jd_Calc_TotalQuantityRemaining = new CachedProperty<ZDecimal>(Factory, delegate
						{
							ZDecimal result = 0M;
							foreach (OrderLine orderLine in OrderLines)
							{
								result += orderLine.JO_QuantityRemaining;
							}

							return result;
						});
				}

				return jd_Calc_TotalQuantityRemaining.Value;
			}
		}
		CachedProperty<ZDecimal> jd_Calc_TotalQuantityRemaining;

		public ZPropertyInfo JD_Calc_TotalQuantityRemainingInfo
		{
			get { return GetZPropertyInfo(Schema.JD_Calc_TotalQuantityRemaining); }
		}

		#endregion

		#region JD_JS_ThatAutoUpdatesShipmentDetailsFromOrder

		[BusinessObjectTestExclude]
		[List(nameof(JD_Shipment_List))]
		public ZGuid JD_JS_ThatAutoUpdatesShipmentDetailsFromOrder
		{
			get { return !IsBookingAttached ? JD_JS : ZGuid.Empty; }
			set
			{
				JD_JS = value;
				if (!IsCopying && !fIsImportingData)
				{
					PopulateShipment();
				}
			}
		}

		public ZPropertyInfo JD_JS_ThatAutoUpdatesShipmentDetailsFromOrderInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JD_JS_ThatAutoUpdatesShipmentDetailsFromOrder, x => JD_JSInfo); }
		}

		public bool JD_JS_ThatAutoUpdatesShipmentDetailsFromOrder_ReadOnly => Shipment != null && (IsBookingAttached || !IsAllowedToDetachShipment);

		#endregion

		#region JD_VB_ThatAutoUpdatesBookingDetailsFromOrder

		[BusinessObjectTestExclude]
		[List(nameof(JD_Booking_List))]
		public ZGuid JD_VB_ThatAutoUpdatesBookingDetailsFromOrder
		{
			get
			{
				return IsBookingAttached ? QuotedBooking.ViewPK : ZGuid.Empty;
			}
			set
			{
				var booking = Factory.Load<IQuotedBooking>(value);
				JD_JS = booking?.ForwardingShipment != null
					? booking.ForwardingShipment.PK
					: ZGuid.Empty;

				if (!IsCopying && !fIsImportingData)
				{
					PopulateShipment();
				}
			}
		}
		public ZPropertyInfo JD_VB_ThatAutoUpdatesBookingDetailsFromOrderInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JD_VB_ThatAutoUpdatesBookingDetailsFromOrder, x => JD_JSInfo); }
		}

		public bool JD_VB_ThatAutoUpdatesBookingDetailsFromOrder_ReadOnly => Shipment != null && (IsForwardingShipmentAttached || !IsAllowedToDetachShipment);

		#endregion

		/// <summary>
		/// Total Price in JD_RX_NKOrderCurrency of non-cancelled order lines
		/// </summary>
		public ZDecimal JD_Calc_TotalPrice
		{
			get
			{
				ZDecimal result = 0m;

				foreach (OrderLine orderLine in NonCancelledOrderLines)
				{
					result += orderLine.JO_LinePrice;
				}
				return result;
			}
		}

		internal ZDecimal TotalCustomsValue
		{
			get
			{
				ZDecimal result = 0m;

				foreach (OrderLine orderLine in NonCancelledOrderLines)
				{
					result += orderLine.CustomsValue;
				}
				return result;
			}
		}

		#region JD_OrderNumberAndSplit

		public ZString JD_OrderNumberAndSplit
		{
			get { return (JD_OrderNumberSplit == 0) ? JD_OrderNumber.ToString() : (JD_OrderNumber + "-" + JD_OrderNumberSplit); }
		}

		public ZPropertyInfo JD_OrderNumberAndSplitInfo
		{
			get { return GetZPropertyInfo(Schema.JD_OrderNumberAndSplit); }
		}

		#endregion

		#region IsSeaTransport
		public ZBool IsSeaTransport
		{
			get
			{
				return
					JD_TransportMode == Constants.TransportModes.Sea ||
					JD_TransportMode == Constants.TransportModes.SeaAir ||
					JD_TransportMode == Constants.TransportModes.AirSea;
			}
		}

		public ZPropertyInfo IsSeaTransportInfo
		{
			get { return GetZPropertyInfo(nameof(IsSeaTransport)); }
		}
		#endregion

		#region IsAirTransport
		public ZBool IsAirTransport
		{
			get { return IsTransportModeAir(JD_TransportMode); }
		}

		ZBool IsTransportModeAir(ZString transportMode)
		{
			return
				transportMode == Constants.TransportModes.Air ||
				transportMode == Constants.TransportModes.SeaAir ||
				transportMode == Constants.TransportModes.AirSea;
		}

		public ZPropertyInfo IsAirTransportInfo
		{
			get { return GetZPropertyInfo(nameof(IsAirTransport)); }
		}
		#endregion

		#region JD_PlannedContainersVisible

		public ZBool JD_PlannedContainersVisible
		{
			get { return JD_ContainerMode == Constants.ContainerModes.FCL || JD_ContainerMode == Constants.ContainerModes.LCL; }
		}

		public ZPropertyInfo JD_PlannedContainersVisibleInfo
		{
			get { return GetZPropertyInfo(nameof(JD_PlannedContainersVisible)); }
		}

		#endregion

		#region JD_Calc_Currency

		public ZExchangeRate JD_Calc_Currency
		{
			get
			{
				if (fJD_Calc_Currency == null)
				{
					fJD_Calc_Currency = new ZExchangeRate(this, ZArchitecture.Core.ExchangeRateType.Buy, JD_EstimatedExchangeRateInfo, (ZPropertyInfoString)JD_RX_NKOrderCurrencyInfo);
					fJD_Calc_Currency.IsRateRequired = false;
				}
				return fJD_Calc_Currency;
			}
			set { fJD_Calc_Currency = value; }
		}
		ZExchangeRate fJD_Calc_Currency;

		#endregion

		#region JD_Calc_LineCount

		public ZInt JD_Calc_LineCount
		{
			get { return OrderLines.Count; }
		}

		public ZPropertyInfo JD_Calc_LineCountInfo
		{
			get { return GetZPropertyInfo(Schema.JD_Calc_LineCount); }
		}

		#endregion

		#region Properties used on Module grid

		public ZString JD_Calc_BuyerCode
		{
			get { return Buyer != null ? Buyer.OH_Code : ZString.Empty; }
		}

		public ZPropertyInfo JD_Calc_BuyerCodeInfo
		{
			get { return GetZPropertyInfo(nameof(JD_Calc_BuyerCode)); }
		}

		public ZString JD_Calc_SupplierCode
		{
			get { return Supplier != null ? Supplier.OH_Code : ZString.Empty; }
		}

		public ZPropertyInfo JD_Calc_SupplierCodeInfo
		{
			get { return GetZPropertyInfo(nameof(JD_Calc_SupplierCode)); }
		}

		public ZString JD_CalcShipmentBrokerageNumber
		{
			get
			{
				if (IsShipmentAttached)
				{
					return Shipment.JS_UniqueConsignRef;
				}
				else if (IsDeclarationAttached)
				{
					return (ZString)Declaration[JobDeclarationSchema.JE_DeclarationReference.Name];
				}
				return ZString.Empty;
			}
		}

		public ZPropertyInfo JD_CalcShipmentBrokerageNumberInfo
		{
			get { return GetZPropertyInfo(Schema.JD_CalcShipmentBrokerageNumber); }
		}

		public ZString JD_Calc_HouseBill
		{
			get { return IsShipmentAttached ? Shipment.JS_HouseBill : JD_Waybill; }
		}

		public ZPropertyInfo JD_Calc_HouseBillInfo
		{
			get { return GetZPropertyInfo(Schema.JD_Calc_HouseBill); }
		}

		public ZString JD_Calc_MasterBill
		{
			get { return (IsShipmentAttached && Shipment.Consols.Count > 0) ? Shipment.Consols[0].JK_MasterBillNum : JD_MasterWaybill; }
		}

		public ZPropertyInfo JD_Calc_MasterBillInfo
		{
			get { return GetZPropertyInfo(Schema.JD_Calc_MasterBill); }
		}

		#region JD_Calc_TotalWeight

		public ZDecimal JD_Calc_TotalWeight
		{
			get
			{
				if (jd_Calc_TotalWeight == null)
				{
					jd_Calc_TotalWeight = new CachedProperty<ZDecimal>(Factory, delegate
					{
						var result = ZDecimal.Zero;
						var unitToCaculateIn = JD_Calc_TotalWeightUnit;

						if (!unitToCaculateIn.IsEmpty)
						{
							foreach (var orderline in OrderLines)
							{
								if (unitToCaculateIn == orderline.JO_UnitOfWeight)
								{
									result += orderline.JO_ActualWeight;
								}
								else
								{
									result += Constants.Weight.Convert(orderline.JO_ActualWeight, orderline.JO_UnitOfWeight, unitToCaculateIn, false);
								}
							}
						}

						return this.GetRoundedValue(JD_Calc_TotalWeightInfo, result);
					});
				}

				return jd_Calc_TotalWeight.Value;
			}
		}
		CachedProperty<ZDecimal> jd_Calc_TotalWeight;

		public ZPropertyInfo JD_Calc_TotalWeightInfo
		{
			get { return GetZPropertyInfo(Schema.JD_Calc_TotalWeight); }
		}

		#endregion

		#region JD_Calc_TotalWeightUnit

		public ZString JD_Calc_TotalWeightUnit
		{
			get
			{
				ZString result = ZString.Empty;

				foreach (var orderline in OrderLines)
				{
					if (!Constants.Weight.ContainsCode(orderline.JO_UnitOfWeight))
					{
						result = ZString.Empty;
						break;
					}
					else if (result.IsEmpty)
					{
						result = orderline.JO_UnitOfWeight;
					}
					else if (orderline.JO_UnitOfWeight != result)
					{
						result = Constants.Weight.ContainsCode(JD_UnitOfWeight) ? JD_UnitOfWeight : ZString.Empty;
						break;
					}
				}

				return result;
			}
		}

		public ZPropertyInfo JD_Calc_TotalWeightUnitInfo
		{
			get { return GetZPropertyInfo(Schema.JD_Calc_TotalWeightUnit); }
		}

		#endregion

		#region JD_Calc_TotalVolume

		public ZDecimal JD_Calc_TotalVolume
		{
			get
			{
				if (jd_Calc_TotalVolume == null)
				{
					jd_Calc_TotalVolume = new CachedProperty<ZDecimal>(Factory, delegate
					{
						var result = ZDecimal.Zero;
						var unitToCaculateIn = JD_Calc_TotalVolumeUnit;

						if (!unitToCaculateIn.IsEmpty)
						{
							foreach (var orderline in OrderLines)
							{
								if (unitToCaculateIn == orderline.JO_UnitOfVolume)
								{
									result += orderline.JO_ActualVolume;
								}
								else
								{
									result += Constants.Volume.Convert(orderline.JO_ActualVolume, orderline.JO_UnitOfVolume, unitToCaculateIn, false);
								}
							}
						}

						return this.GetRoundedValue(JD_Calc_TotalVolumeInfo, result);
					});
				}

				return jd_Calc_TotalVolume.Value;
			}
		}
		CachedProperty<ZDecimal> jd_Calc_TotalVolume;

		public ZPropertyInfo JD_Calc_TotalVolumeInfo
		{
			get { return GetZPropertyInfo(Schema.JD_Calc_TotalVolume); }
		}

		#endregion

		#region JD_Calc_TotalVolumeUnit

		public ZString JD_Calc_TotalVolumeUnit
		{
			get
			{
				ZString result = ZString.Empty;

				foreach (var orderline in OrderLines)
				{
					if (!Constants.Volume.ContainsCode(orderline.JO_UnitOfVolume))
					{
						result = ZString.Empty;
						break;
					}
					else if (result.IsEmpty)
					{
						result = orderline.JO_UnitOfVolume;
					}
					else if (orderline.JO_UnitOfVolume != result)
					{
						result = Constants.Volume.ContainsCode(JD_UnitOfVolume) ? JD_UnitOfVolume : ZString.Empty;
						break;
					}
				}

				return result;
			}
		}

		public ZPropertyInfo JD_Calc_TotalVolumeUnitInfo
		{
			get { return GetZPropertyInfo(Schema.JD_Calc_TotalVolumeUnit); }
		}

		#endregion

		#endregion

		#endregion

		#region Populate Shipment

		public void PopulateShipment()
		{
			PopulateShipmentCore();
		}

		public event EventHandler<PopulateShipmentEventArgs> OnPopulateShipment;

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected virtual void PopulateShipmentCore()
		{
			if (Shipment != null)
			{
				ZInt shipmentOrdersInnerPacks = 0;
				ZInt shipmentOrdersOuterPacks = 0;
				ZDecimal shipmentOrdersWeight = 0m;
				ZDecimal shipmentOrdersVolume = 0m;

				foreach (Order order in Shipment.AttachedOrders)
				{
					if (order.PK != this.PK)
					{
						shipmentOrdersWeight += order.JD_ActualWeight;
						shipmentOrdersVolume += order.JD_ActualVolume;
						shipmentOrdersOuterPacks += order.JD_Packs;
						shipmentOrdersInnerPacks += order.JD_Calc_InnerPacks;
					}
				}

				try
				{
					((ISupportDataImporting)Shipment).IsImportingData = true;

					var needUpdateItem = new List<ResourceString>();

					if ((Shipment.JS_OuterPacks == shipmentOrdersOuterPacks) && JD_Packs > 0 && !JD_F3_NKPackType.IsEmpty)
					{
						needUpdateItem.Add(ResString.GetMultilingualString("b2e05fe8-6fd7-4768-9ddf-0b1baf08f235", "Packs"));
					}

					if (Shipment.JS_ActualWeight == shipmentOrdersWeight)
					{
						needUpdateItem.Add(ResString.GetMultilingualString("d5b9113e-373d-452c-9670-24ab899283ae", "Weight"));
					}

					if (Shipment.JS_ActualVolume == shipmentOrdersVolume)
					{
						needUpdateItem.Add(ResString.GetMultilingualString("d832674d-9a99-4da9-9810-5fb5a56d140c", "Volume"));
					}

					if (needUpdateItem.Count > 0)
					{
						var args = new PopulateShipmentEventArgs(ResString.GetMultilingualString("352040e7-fce9-4b83-9fd2-fd2196c0f62d", "Do you want to update the {0} on the Shipment from the Order?", string.Join(", ", needUpdateItem)), true); // Constant
						if (Shipment.JS_OuterPacks != 0 || Shipment.JS_ActualWeight != 0 || Shipment.JS_ActualVolume != 0)
						{
							OnPopulateShipment?.Invoke(this, args);
						}

						if (args.ShouldPopulateShipment)
						{
							if (Shipment.JS_ActualWeight == shipmentOrdersWeight)
							{
								Shipment.JS_ActualWeight = shipmentOrdersWeight + JD_ActualWeight;
							}

							if (Shipment.JS_ActualVolume == shipmentOrdersVolume)
							{
								Shipment.JS_ActualVolume = shipmentOrdersVolume + JD_ActualVolume;
							}

							if ((Shipment.JS_OuterPacks == shipmentOrdersOuterPacks) && JD_Packs > 0 && !JD_F3_NKPackType.IsEmpty)
							{
								if (Shipment.JS_F3_NKPackType != JD_F3_NKPackType)
								{
									Shipment.JS_F3_NKPackType = Shipment.JS_OuterPacks == 0 ? JD_F3_NKPackType : (ZString)Constants.PkgUnit.Package;
								}

								Shipment.JS_OuterPacks = shipmentOrdersOuterPacks + JD_Packs;
							}
						}
					}

					if ((Shipment.JS_TotalPackageCount == shipmentOrdersInnerPacks) && JD_Calc_InnerPacks > 0 && !JD_Calc_InnerPackType.IsEmpty)
					{
						if (Shipment.JS_F3_NKTotalCountPackType != JD_Calc_InnerPackType)
						{
							Shipment.JS_F3_NKTotalCountPackType = Shipment.JS_TotalPackageCount == 0 ? JD_Calc_InnerPackType : (ZString)Constants.PkgUnit.Package;
						}

						Shipment.JS_TotalPackageCount = shipmentOrdersInnerPacks + JD_Calc_InnerPacks;
					}

					bool isThisFirstAttachedOrder = Shipment.AttachedOrders.Count == 0 || (Shipment.AttachedOrders.Count == 1 && Shipment.AttachedOrders[0].PK == PK);
					if (Shipment.JS_GoodsDescription.IsEmpty && Shipment.DetailedGoodsDescriptionNoteText.IsEmpty && isThisFirstAttachedOrder && !JD_OrderGoodsDescription.IsEmpty)
					{
						Shipment.JS_GoodsDescription = JD_OrderGoodsDescription.Substring(0, JobShipmentSchema.JS_GoodsDescription.MaxLength);
					}

					if (Shipment.JS_INCO.IsEmpty && isThisFirstAttachedOrder && !JD_IncoTerm.IsEmpty)
					{
						Shipment.JS_INCO = JD_IncoTerm;
					}

					if (Shipment.JS_RS_NKServiceLevel.IsEmpty && isThisFirstAttachedOrder && !JD_RS_NKServiceLevel_NI.IsEmpty)
					{
						Shipment.JS_RS_NKServiceLevel = JD_RS_NKServiceLevel_NI;
					}
				}
				finally
				{
					((ISupportDataImporting)Shipment).IsImportingData = false;
				}
			}
		}

		#endregion

		#region Attached Declaration/Shipment Properties

		#region AttachedShipment_HouseBill

		public ZString AttachedShipment_HouseBill
		{
			get
			{
				ZString result = "";
				if (Declaration != null)
				{
					result = (ZString)Declaration[JobDeclarationSchema.JE_HouseBill.Name];
				}

				if (Shipment is ForwardingShipment attachedShipment)
				{
					result = !attachedShipment.JS_HouseBill.IsEmpty
						? attachedShipment.JS_HouseBill
						: attachedShipment.JS_UniqueConsignRef;
				}

				return result;
			}
		}

		public ZPropertyInfo AttachedShipment_HouseBillInfo
		{
			get { return GetZPropertyInfo(nameof(AttachedShipment_HouseBill)); }
		}

		#endregion

		#region AttachedShipment_RL_NKOrigin

		[List("JD_RL_List")]
		public ZString AttachedShipment_RL_NKOrigin
		{
			get
			{
				ZString result = "";
				if (Declaration != null)
				{
					result = (ZString)Declaration[JobDeclarationSchema.JE_RL_NKOrigin.Name];
				}

				if (Shipment != null)
				{
					result = Shipment.JS_RL_NKOrigin;
				}

				return result;
			}
		}

		public ZPropertyInfo AttachedShipment_RL_NKOriginInfo
		{
			get { return GetZPropertyInfo(Schema.AttachedShipment_RL_NKOrigin); }
		}

		#endregion

		#region AttachedShipment_RL_NKDestination

		[List("JD_RL_List")]
		public ZString AttachedShipment_RL_NKDestination
		{
			get
			{
				ZString result = "";
				if (Declaration != null)
				{
					result = (ZString)Declaration[JobDeclarationSchema.JE_RL_NKFinalDestination.Name];
				}

				if (Shipment != null)
				{
					result = Shipment.JS_RL_NKDestination;
				}

				return result;
			}
		}

		public ZPropertyInfo AttachedShipment_RL_NKDestinationInfo
		{
			get { return GetZPropertyInfo(Schema.AttachedShipment_RL_NKDestination); }
		}

		#endregion

		#region AttachedShipment_RL_NKServiceLevel

		[List("JD_RS_List")]
		public ZString AttachedShipment_RS_NKServiceLevel
		{
			get
			{
				ZString result = "";
				if (Declaration != null)
				{
					result = (ZString)Declaration[JobDeclarationSchema.JE_RS_NKServiceLevel.Name];
				}

				if (Shipment != null)
				{
					result = Shipment.JS_RS_NKServiceLevel;
				}

				return result;
			}
		}

		public ZPropertyInfo AttachedShipment_RS_NKServiceLevelInfo
		{
			get { return GetZPropertyInfo(Schema.AttachedShipment_RS_NKServiceLevel); }
		}

		#endregion

		#endregion

		#region Notes

		public override BusinessObject[] BusinessObjectsWithRelatedNotes
		{
			get
			{
				var result = new List<BusinessObject>()
				{
					Buyer,
					Supplier,
					SendingAgent,
					ReceivingAgent
				};

				if (ControllingCustomerDocAddress.Organisation != null)
				{
					result.Add(ControllingCustomerDocAddress.Organisation);
				}

				return result.ToArray();
			}
		}

		protected override StmNoteContexts NoteContextsForRelatedNotes
		{
			get
			{
				StmNoteContexts result = new StmNoteContexts();

				result = StmNoteContextUtils.GetContextFromTransportModeImportExport(Constants.GlobalModuleNamesConstants.Forwarding, "", JD_TransportMode, "");
				result.Module |= base.NoteContextsForRelatedNotes.Module;
				result.Direction |= base.NoteContextsForRelatedNotes.Direction;
				result.FreightMode |= base.NoteContextsForRelatedNotes.FreightMode;

				result.Module |= StmNoteContextModule.O;
				if (Shipment != null)
				{
					if (Shipment.IsImport())
					{
						result.Direction |= StmNoteContextDirection.I;
					}

					if (Shipment.IsExport())
					{
						result.Direction |= StmNoteContextDirection.E;
					}
				}

				return result;
			}
		}

		public override Notes Notes
		{
			get
			{
				if (!base.Notes.GetAllNotes().IsRefreshingByDataRefreshBus && GetOrderManagementUpdateNotes().Length > 0)
				{
					CreateOrRefreshOrderUpdateHistoryNote();
				}

				return base.Notes;
			}
		}

		#region OrderUpdateHistoryStmNote

		internal class OrderUpdateHistoryStmNote : StmNote, Enterprise.Integration.Freight.IOrderUpdateHistoryStmNote
		{
			[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Virtual methods are reviewed. All possible values are expected and handled.")]
			public OrderUpdateHistoryStmNote(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
				ReadOnly = true;
				IsReadOnlyAfterAdd = true;
				ST_Description_List = new NoteTypeCollection();
				ST_Description_List.Add(PredefinedNoteTypes.Instance.OrderUpdateHistory);
				ST_Description = PredefinedNoteTypes.Instance.OrderUpdateHistory.Code;
			}

			public override bool HasChanges
			{
				get { return false; } // we never want to save this note
				set { }
			}

			public override bool IsSavedByFactory
			{
				get { return false; }
			}

			protected override StmNoteValidation GetNewValidation()
			{
				return new OrderUpdateHistoryStmNoteValidation(this);
			}

			class OrderUpdateHistoryStmNoteValidation : StmNoteValidation
			{
				public OrderUpdateHistoryStmNoteValidation(OrderUpdateHistoryStmNote parent)
					: base(parent)
				{
				}

				protected override void CheckST_Description()
				{
				}

				protected override void CheckST_NoteText()
				{
				}

				public override void ValidateAll()
				{
				}
			}
		}

		StmNote[] GetOrderManagementUpdateNotes()
		{
			return base.Notes.FindByDescription(PredefinedNoteTypes.Instance.OrderManagementUpdate.Description);
		}

		OrderUpdateHistoryStmNote CreateOrRefreshOrderUpdateHistoryNote()
		{
			if (orderUpdateHistoryNote == null || orderUpdateHistoryNote.IsDeleted)
			{
				orderUpdateHistoryNote = Factory.New<OrderUpdateHistoryStmNote>();
				base.Notes.Add(orderUpdateHistoryNote);
			}
			ZString truncatedNote = GetOrderUpdateHistoryText();
			truncatedNote = truncatedNote.SubstringSafe(0, orderUpdateHistoryNote.ST_NoteTextInfo.MaxLength);
			orderUpdateHistoryNote.ST_NoteText = truncatedNote;
			return orderUpdateHistoryNote;
		}

		OrderUpdateHistoryStmNote orderUpdateHistoryNote;

		string GetOrderUpdateHistoryText()
		{
			var managementUpdateSummary = new StringBuilder();
			foreach (StmNote note in GetOrderManagementUpdateNotes().OrderByDescending(n => n.ST_CreatedDateUtc))
			{
				if (!note.ST_NoteText.IsEmpty)
				{
					managementUpdateSummary.Append(note.ST_CreatedByUserName);
					managementUpdateSummary.Append("\t");
					managementUpdateSummary.Append(note.ST_CreatedDateUtc.ToLongTimeString());
					managementUpdateSummary.Append(System.Environment.NewLine);
					managementUpdateSummary.Append(note.ST_NoteText);
					managementUpdateSummary.Append(System.Environment.NewLine);
				}
			}

			return managementUpdateSummary.ToString();
		}

		#endregion

		#endregion

		#region Order Split

		/// <summary>
		/// Returns true if this Order can be split (ie. it has incomplete order lines and has not already been split).
		/// </summary>
		public bool IsOrderPartiallyCompleteAndIsNotAlreadySplit
		{
			get
			{
				return
					this.OrderLines.IsOrderPartiallyComplete
					&& JD_OrderNumberSplit < byte.MaxValue
					&& !IsAlreadySplit;
			}
		}

		/// <summary>
		/// Returns true if this Order has been split.
		/// </summary>
		bool IsAlreadySplit
		{
			get
			{
				if (JD_OrderNumberSplit < byte.MaxValue)
				{
					var orderNumberFilter = new ZQuery(JobOrderHeaderSchema.JD_OrderNumber, SQLComparisonOperator.Equal, JD_OrderNumber);
					var splitNumberFilter = new ZQuery(JobOrderHeaderSchema.JD_OrderNumberSplit, SQLComparisonOperator.GreaterThan, (byte)(JD_OrderNumberSplit));

					return Factory.LoadTop1<Order>(new ZQuery(orderNumberFilter, JoinCondition.And, splitNumberFilter)) != null;
				}

				return true;
			}
		}

		public void SplitOrder(OrderSplitDialogResult splitType)
		{
			switch (splitType)
			{
				case OrderSplitDialogResult.SplitOrder:
					SplitOrder(CreateOrderType.Split);
					break;
				case OrderSplitDialogResult.CreateNewOrder:
					SplitOrder(CreateOrderType.New);
					break;
			}
		}

		public Order SplitOrder(CreateOrderType splitType)
		{
			var result = (Order)this.Clone(new BusinessObjectCloneArgs(new[] { Order.Schema.JD_EF_ShipmentPrePlanning }));

			result.OrderLines.DeleteAll();

			if (splitType == CreateOrderType.Split)
			{
				result.JD_OrderNumber = JD_OrderNumber;
				result.JD_IncoTerm = JD_IncoTerm;
				result.JD_RX_NKOrderCurrency = JD_RX_NKOrderCurrency;
				result.JD_OrderNumberSplit = FindNextOrderSplitNumber();
				result.PlannedContainers.RemoveAndDeleteAll();
				result.JD_JS = ZGuid.Empty;
				result.JD_JE = ZGuid.Empty;
				ClearSailingDetails(result);
				ClearTrackingDates(result);
				FixOverridesForSplitOrder(result);
			}
			else
			{
				result.JD_OrderNumber = TryGetNextOrderNumber();
				CopyMilestoneDates(result);
			}

			result.SetRemainingOrderLinesBasedOnOrder(this);
			RefreshOrderNumberReadOnly();
			RefreshOrderSplitSiblings();
			result.RefreshOrderSplitSiblings();

			SetHasChangesToTrueToRefreshStatusOnFactorySave();
			return result;
		}

		protected virtual void FixOverridesForSplitOrder(Order clonedOrder)
		{
		}

		void ClearSailingDetails(Order clonedOrder)
		{
			clonedOrder.JD_DepartureVesselCutoffDate = ZDateTime.Empty;
			clonedOrder.JD_RV_NKDepartureVessel = "";
			clonedOrder.JD_RV_NKIntermediateVessel = "";
			clonedOrder.JD_RV_NKArrivalVessel = "";
			clonedOrder.JD_DepartureVoyage = "";
			clonedOrder.JD_IntermediateVoyage = "";
			clonedOrder.JD_DepartureVoyage = "";
			clonedOrder.JD_ArrivalVoyage = "";
			clonedOrder.JD_E_DEP_2 = ZDateTime.Empty;
			clonedOrder.JD_E_DEP_3 = ZDateTime.Empty;
			clonedOrder.JD_E_ARV_1stIntermediate = ZDateTime.Empty;
			clonedOrder.JD_E_ARV_2ndIntermediate = ZDateTime.Empty;
			clonedOrder.JD_Waybill = "";
			clonedOrder.JD_MasterWaybill = "";

			clonedOrder.ClearMilestoneAndTriggerDates(Events.Departure, null, true);
			clonedOrder.ClearMilestoneAndTriggerDates(Events.Arrival, null, true);
		}

		void ClearTrackingDates(Order clonedOrder)
		{
			clonedOrder.JD_ActualUserDate2 = ZDateTime.Empty;
			clonedOrder.JD_EstimateUserDate2 = ZDateTime.Empty;
			clonedOrder.JD_ActualUserDate1 = ZDateTime.Empty;
			clonedOrder.JD_EstimateUserDate1 = ZDateTime.Empty;
			clonedOrder.JD_ActualUserDate4 = ZDateTime.Empty;
			clonedOrder.JD_EstimateUserDate4 = ZDateTime.Empty;
			clonedOrder.JD_ActualUserDate3 = ZDateTime.Empty;
			clonedOrder.JD_EstimateUserDate3 = ZDateTime.Empty;

			foreach (Event eventType in EventsToClone)
			{
				clonedOrder.ClearMilestoneAndTriggerDates(eventType, null, true);
			}
		}

		void SetHasChangesToTrueToRefreshStatusOnFactorySave()
		{
			HasChanges = true;
		}

		public bool IsNextOrderSplitNumberValid()
		{
			var order = GetHighestExistingSplitOrder();
			return order == null || order.JD_OrderNumberSplit < Byte.MaxValue;
		}

		byte FindNextOrderSplitNumber()
		{
			var order = GetHighestExistingSplitOrder();
			return order != null ? (byte)(order.JD_OrderNumberSplit + 1) : (byte)0;
		}

		Order GetHighestExistingSplitOrder()
		{
			var filter = new ZQuery(JobOrderHeaderSchema.JD_OrderNumber, SQLComparisonOperator.Equal, JD_OrderNumber);
			filter.AddToFilter(JoinCondition.And, JobOrderHeaderSchema.JD_OA_BuyerAddress, SQLComparisonOperator.Equal, Buyer == null ? ZGuid.Empty : Buyer.Addresses.Select(x => x.PK));
			filter.OrderBy = Order.Schema.JD_OrderNumberSplit + " DESC";
			filter.IgnoreActiveFilter = true;

			var order = Factory.LoadTop1<Order>(filter);

			return order;
		}

		/// <summary>
		/// Duplicates all incomplete OrderLines, with the remaining amount set as the new order amount.
		/// </summary>
		/// <param name="order">The Order business object from which to duplicate incomplete OrderLines. </param>
		protected void SetRemainingOrderLinesBasedOnOrder(Order order)
		{
			foreach (OrderLine orderLine in new ArrayList(order.OrderLines))
			{
				if (orderLine.JO_QuantityRemaining > 0)
				{
					OrderLine newOrderLine = (OrderLine)orderLine.Clone();
					OrderLines.Add(newOrderLine);

					newOrderLine.JO_Quantity = orderLine.JO_QuantityRemaining;
					newOrderLine.JO_QtyReceived = 0;
					newOrderLine.JO_QtyInvoiced = 0;
					newOrderLine.JO_CommercialInvoiceNo = "";
					newOrderLine.JO_ContainerNumber = "";

					if (orderLine.JO_Quantity - orderLine.JO_QuantityRemaining != 0)
					{
						newOrderLine.JO_LineSplitNumber++;
					}
				}
			}
		}

		#endregion

		#region Voyage Planning States

		public PlanningVoyageState PlanningVoyageState
		{
			get
			{
				if (!JD_IntermediateVoyage.IsEmpty)
				{
					return PlanningVoyageState.ThreeVoyage;
				}
				else if (
					!JD_DepartureVoyage.IsEmpty &&
					!JD_ArrivalVoyage.IsEmpty &&
					JD_DepartureVoyage != JD_ArrivalVoyage)
				{
					return PlanningVoyageState.TwoVoyage;
				}
				else
				{
					return PlanningVoyageState.OneVoyage;
				}
			}
		}

		#endregion

		#region Retrieving Sailing Details

		protected void RetrieveSailingForDeparture()
		{
			if (PlanningVoyageState != PlanningVoyageState.OneVoyage)
			{
				RetrieveSailing(
					JD_RV_NKDepartureVessel, JD_DepartureVoyage,
					JD_Milestone_E_DEPInfo, JD_E_ARV_1stIntermediateInfo);
			}
			else
			{
				RetrieveSailing(
					JD_RV_NKDepartureVessel, JD_DepartureVoyage,
					JD_Milestone_E_DEPInfo, JD_Milestone_E_ARVInfo);
			}
		}

		protected void RetrieveSailingForIntermediate()
		{
			RetrieveSailing(
				JD_RV_NKIntermediateVessel, JD_IntermediateVoyage,
				JD_E_DEP_2Info, JD_E_ARV_2ndIntermediateInfo);
		}

		protected void RetrieveSailingForArrival()
		{
			if (PlanningVoyageState != PlanningVoyageState.OneVoyage)
			{
				RetrieveSailing(
					JD_RV_NKArrivalVessel, JD_ArrivalVoyage,
					JD_E_DEP_3Info, JD_Milestone_E_ARVInfo);
			}
			else
			{
				RetrieveSailing(
					JD_RV_NKArrivalVessel, JD_ArrivalVoyage,
					JD_Milestone_E_DEPInfo, JD_Milestone_E_ARVInfo);
			}
		}

		protected void RetrieveSailing(
			ZString vesselName, ZString voyageNo,
			ZPropertyInfo e_DEP_Prop, ZPropertyInfo e_ARV_Prop)
		{
			bool transportIsSeaOrFSA =
				JD_TransportMode == Constants.TransportModes.Sea ||
				JD_TransportMode == Constants.TransportModes.SeaAir;
			bool vesselAndVoyageIsValid = !vesselName.IsEmpty && !voyageNo.IsEmpty;
			JobVoyage voyage = RetrieveVoyage(vesselName, voyageNo);

			if (transportIsSeaOrFSA && vesselAndVoyageIsValid && voyage != null)
			{
				OrgHeader buyer = this.Buyer;
				OrgHeader supplier = this.Supplier;

				VoyageOrigin origin = null;
				if (supplier != null && voyage != null)
				{
					foreach (VoyageOrigin next in voyage.Origins)
					{
						if (!next.JA_RL_NKPortOfLoading.IsEmpty &&
							next.JA_RL_NKPortOfLoading == supplier.OH_RL_NKClosestPort)
						{
							origin = next;
							break;
						}
					}
				}

				VoyageDestination destination = null;
				if (buyer != null && voyage != null)
				{
					foreach (VoyageDestination next in voyage.Destinations)
					{
						if (!next.JB_RL_NKPortOfDischarge.IsEmpty &&
							next.JB_RL_NKPortOfDischarge == buyer.OH_RL_NKClosestPort)
						{
							destination = next;
							break;
						}
					}
				}

				if (origin != null)
				{
					this[e_DEP_Prop.Name] = origin.JA_E_DEP;
				}
				if (destination != null)
				{
					this[e_ARV_Prop.Name] = destination.JB_E_ARV;
				}
			}
		}

		#endregion

		#region Refresh Voyages ReadOnly

		protected void RefreshVoyagesReadOnly()
		{
			JD_IntermediateVoyageInfo.RefreshBinding();
			JD_ArrivalVoyageInfo.RefreshBinding();
		}

		protected bool AllVoyagesEmpty
		{
			get { return JD_DepartureVoyage.IsEmpty && JD_IntermediateVoyage.IsEmpty && JD_ArrivalVoyage.IsEmpty; }
		}

		#endregion

		#region Pre Advice

		public enum TargetObjectForCreate
		{
			PreAdvice,
			Declaration,
			Shipment,
		}

		public delegate OrderSplitDialogResult ShowConfirmationForSplittingOrderDelegate();

		public JobShipmentPreplanning CreateAndLinkPreAdviceToOrder(INotifications notifier, TargetObjectForCreate targetObject, ShowConfirmationForSplittingOrderDelegate showConfirmation = null)
		{
			if (!IsInDatabase || HasChanges)
			{
				notifier.Notify(new InfoNotification(Res.GetString("87007d8f-788b-4891-b288-14849a18ad2e", "Please save your Order before attempting any Actions.")));
				return null;
			}

			if (PreAdvice != null)
			{
				return HandleExistingPreAdvice(notifier, targetObject);
			}

			var newPreAdvice = Factory.New<JobShipmentPreplanning>();
			newPreAdvice.EF_MasterBill = JD_MasterWaybill;
			newPreAdvice.EF_OA_BuyerAddress = JD_OA_BuyerAddress;
			newPreAdvice.EF_OC_BuyerContact = JD_OC_BuyerContact;
			newPreAdvice.EF_OH_Carrier = JD_OH_Carrier;
			newPreAdvice.EF_OH_ReceivingAgent = JD_OH_ReceivingAgent;
			newPreAdvice.EF_OH_SendingAgent = JD_OH_SendingAgent;
			newPreAdvice.EF_RL_NKPortDisch = JD_RL_NKPortOfDischarge;
			newPreAdvice.EF_RL_NKPortLoad = JD_RL_NKPortOfLoading;

			newPreAdvice.EF_ActualWeight = JD_ActualWeight;
			newPreAdvice.EF_UnitOfWeight = JD_UnitOfWeight;
			newPreAdvice.EF_ActualVolume = JD_ActualVolume;
			newPreAdvice.EF_UnitOfVolume = JD_UnitOfVolume;
			newPreAdvice.EF_Packs = JD_Packs;
			newPreAdvice.EF_F3_NKPackType = JD_F3_NKPackType;

			newPreAdvice.PreAdviceTransports[0].JW_TransportMode = JD_TransportMode;
			newPreAdvice.PreAdviceTransports[0].JW_Vessel = JD_RV_NKDepartureVessel;
			newPreAdvice.PreAdviceTransports[0].JW_VoyageFlight = JD_DepartureVoyage;
			newPreAdvice.PreAdviceTransports[0].CarrierPK = JD_OH_Carrier;

			var time = GetMilestoneEstimatedDate(Events.Departure).ToZDateTime();
			if (!time.IsEmpty)
			{
				newPreAdvice.PreAdviceTransports[0].JW_ETD = time;
			}

			time = GetMilestoneEstimatedDate(Events.Arrival).ToZDateTime();
			if (!time.IsEmpty)
			{
				newPreAdvice.PreAdviceTransports[0].JW_ETA = time;
			}

			time = GetMilestoneActualDate(Events.Departure).ToZDateTime();
			if (!time.IsEmpty)
			{
				newPreAdvice.PreAdviceTransports[0].JW_ATD = time;
			}

			time = GetMilestoneActualDate(Events.Arrival).ToZDateTime();
			if (!time.IsEmpty)
			{
				newPreAdvice.PreAdviceTransports[0].JW_ATA = time;
			}

			newPreAdvice.Orders.Add(this);

			if (IsOrderPartiallyCompleteAndIsNotAlreadySplit)
			{
				SplitOrder(showConfirmation?.Invoke() ?? OrderSplitDialogResult.DoNothing);
			}

			JobShipmentPreplanning resultFromConcurrencyHandler = null;
			bool handledConcurrencyError = false;

			Action saveRecoveryAction = () =>
			{
				// concurrency resolver would reload/merge JD_EF_ShipmentPrePlanning
				if (JD_EF_ShipmentPrePlanning != newPreAdvice.PK)
				{
					newPreAdvice.Delete();
					resultFromConcurrencyHandler = HandleExistingPreAdvice(notifier, targetObject);
					handledConcurrencyError = true;
				}
			};

			ZExceptionReporting.ProcessWithSaveExceptionHandling(Factory.Save, saveRecoveryAction, true);

			return handledConcurrencyError
				? resultFromConcurrencyHandler
				: PreAdvice;
		}

		JobShipmentPreplanning HandleExistingPreAdvice(INotifications notifier, TargetObjectForCreate targetObject)
		{
			switch (targetObject)
			{
				case TargetObjectForCreate.Declaration:
				case TargetObjectForCreate.Shipment:
					var target = targetObject == TargetObjectForCreate.Declaration
						? Res.GetString("5ea4b6fe-86f2-4073-9df1-2860d35097f2", "Declaration")
						: Res.GetString("0500a28b-f631-44a1-882f-ee11add5d7fb", "Shipment");
					return QueryUserIfExistingPreAdviceExists(notifier, target)
						? PreAdvice
						: null;

				case TargetObjectForCreate.PreAdvice:
					notifier.Notify(new InfoNotification(Res.GetString("75a48f84-beca-4982-85b5-dfb8b6cc3154", "This order is already part of a Shipment Pre Advice.")));
					return null;

				default:
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Cannot create Pre Advice for {0}", targetObject));
			}
		}

		bool QueryUserIfExistingPreAdviceExists(INotifications notifier, string targetObject)
		{
			if (PreAdvice != null)
			{
				var caption = Res.GetString("bd03200e-07cf-43b9-9ed5-5b85171a6839", "New {0} from Order", targetObject);
				var message = Res.GetString("af6a492f-ddb1-4296-a000-fdd33bba2041",
					"This order is attached to the {0} pre-advice. The created {1} will be for that pre-advice and not just this one order.\r\n\r\nDo you want to continue?",
					PreAdvice.EF_PreshipID,
					targetObject);

				var queryArgs = new QueryUserMsgBoxEventArgs(caption, message, false);
				notifier.QueryUser(queryArgs);

				return queryArgs.Response;
			}

			return true;
		}

		#endregion

		#region Attaching to a Shipment or Declaration

		public ForwardingShipment Shipment
		{
			get
			{
				if (shipment == null || shipment.PK != JD_JS)
				{
					shipment = LoadShipment();
				}

				return shipment;
			}
		}

		ForwardingShipment shipment;

		protected virtual ForwardingShipment LoadShipment()
		{
			var linkedShipment = Factory.Load<ForwardingShipment>(JD_JS);

			if (linkedShipment != null
				&& ChildEditableService.GetState(Factory) == ChildEditableServiceStates.Order)
			{
				RegisterEditableChildObject(linkedShipment);
			}

			return linkedShipment;
		}

		public IQuotedBooking QuotedBooking
		{
			get
			{
				if (quotedBooking == null
					|| quotedBooking.ForwardingShipment?.PK != JD_JS)
				{
					var query = new ZQuery(ViewQuotedBookingSchema.VB_JS, JD_JS);
					var viewQuotedBooking = Factory.LoadTop1<IViewQuotedBooking>(query);
					quotedBooking = viewQuotedBooking != null
						? Factory.Load<IQuotedBooking>(viewQuotedBooking.PK)
						: null;
				}

				return quotedBooking;
			}
		}

		IQuotedBooking quotedBooking;

		public bool IsBookingAttached
		{
			get
			{
				var attachedShipment = Shipment;
				return attachedShipment != null && attachedShipment.JS_IsBooking && !attachedShipment.JS_IsForwardRegistered;
			}
		}

		public bool IsAttachedToSupplierBooking => OrderLines.Any(line => line.SupplierBookingLines.Any(bookingLine => !(bookingLine?.SupplierBooking?.IsIncompleteOrCancelled ?? true)));

		public bool IsForwardingShipmentAttached => Shipment?.JS_IsForwardRegistered ?? false;

		public BusinessObject Declaration
		{
			get { return (BusinessObject)Factory.Load<Enterprise.Integration.Customs.IBaseJobDeclaration>(JD_JE); }
		}

		public bool IsShipmentAttached
		{
			get { return Shipment != null; }
		}

		public bool IsDeclarationAttached
		{
			get { return Declaration != null; }
		}

		public bool CanBeUpdatedByImport
		{
			get { return CanBeUpdatedByImportCore; }
		}

		protected virtual bool CanBeUpdatedByImportCore
		{
			get { return !IsDeclarationAttached && (!IsShipmentAttached || !Shipment.JS_IsForwardRegistered); }
		}

		bool IsDeclarationExportAttached
		{
			get
			{
				bool result = false;
				if (JD_JE.IsValid && Declaration != null)
				{
					result = new ZBool(Declaration["IsExport"]);
				}
				return result;
			}
		}

		public ForwardingConsolCollection ConsolsForBinding
		{
			get
			{
				RefreshConsolsForBinding();
				return consolsForBinding;
			}
		}
		ForwardingConsolCollection consolsForBinding;

		void RefreshConsolsForBinding()
		{
			if (consolsForBinding == null)
			{
				consolsForBinding = new ForwardingConsolCollection(Factory);
			}

			consolsForBinding.RemoveAll();

			if (Shipment != null)
			{
				consolsForBinding.AddRange(Shipment.Consols);
			}
		}

		#endregion

		#region Attaching to a Invoice

		public BusinessObject AppendOrdersLineToInvoice(INotifications notification)
		{
			if (!IsInDatabase || HasChanges)
			{
				notification.Notify(new InfoNotification(Res.GetString("291f7096-78a6-4881-8bdb-d8a432e17c70", "Please save your changes before performing any Actions on this Pre Advice.")));
				return null;
			}

			if (IsLinkedToPreAdvice)
			{
				notification.Notify(new InfoNotification(Res.GetString("15456635-0d1f-40ca-99ef-6ba841a7960b", "This Pre Advice has already been linked to an operations job and cannot be linked again.")));
				return null;
			}

			if (JD_InvoiceNumber.IsEmpty)
			{
				notification.Notify(new InfoNotification(Res.GetString("0a7fe115-c685-4cf2-b8e2-1450c0277a51", "You have to enter the Invoice No.")));
				return null;
			}

			var inv = GetInvoiceByInvoiceNum();
			BusinessObject invBO = QueryUserForInvoice(notification, inv);

			ZString warning = string.Empty;
			BusinessObject invoiceBO = AppendOrdersLineToInvoiceLine(invBO, out warning);

			ZExceptionReporting.ProcessWithSaveExceptionHandling(Factory.Save, null, true);

			if (!warning.IsEmpty)
			{
				notification.Notify(new InfoNotification(warning));
			}
			else
			{
				notification.Notify(new InfoNotification(Res.GetString("8a163a55-bd44-4ab3-a7e3-f8e364256a79", "Order lines were successfully appended to {0}.", invoiceBO.HumanReadableName)));
			}

			return invBO;
		}

		BusinessObject GetInvoiceByInvoiceNum()
		{
			ZQuery query = new ZQuery(JobComInvoiceHeaderSchema.JZ_InvoiceNumber, JD_InvoiceNumber);
			query.AddToFilter(JobComInvoiceHeaderSchema.JZ_OH_Supplier, SupplierPK);
			query.AddToFilter(JobComInvoiceHeaderSchema.JZ_OH_Buyer, BuyerPK);
			query.AddToFilter(JobComInvoiceHeaderSchema.JZ_InvoiceDate, JD_InvoiceDate);
			return (BusinessObject)Factory.LoadTop1<Enterprise.Integration.Customs.Shared.IBaseJobComInvoiceHeader>(query);
		}

		BusinessObject AppendOrdersLineToInvoiceLine(BusinessObject invoiceHeader, out ZString result)
		{
			bool updatedSome = false;
			bool updatedAll = true;

			if (invoiceHeader == null)
			{
				invoiceHeader = (BusinessObject)Factory.New<Enterprise.Integration.Customs.Shared.IBaseJobComInvoiceHeader>();

				invoiceHeader[JobComInvoiceHeaderSchema.JZ_InvoiceNumber] = this.JD_InvoiceNumber;
				invoiceHeader[JobComInvoiceHeaderSchema.JZ_OH_Supplier] = this.SupplierPK;
				invoiceHeader[JobComInvoiceHeaderSchema.JZ_OH_Buyer] = this.BuyerPK;
				invoiceHeader[JobComInvoiceHeaderSchema.JZ_InvoiceDate] = this.JD_InvoiceDate;
				invoiceHeader[JobComInvoiceHeaderSchema.JZ_RX_NKInvoice_Currency] = (this.OrderCurrency != null) ? this.JD_RX_NKOrderCurrency : ZString.Empty;
				invoiceHeader[JobComInvoiceHeaderSchema.JZ_RN_NKDefaultOrigin] = this.JD_RN_NKCountryOfSupply;
				invoiceHeader[JobComInvoiceHeaderSchema.JZ_IncoTerm] = this.JD_IncoTerm;
			}

			if (invoiceHeader is IAttachOrders parent)
			{
				parent.AttachedOrders.Add(this);
			}

			foreach (OrderLine orderLine in this.OrderLines)
			{
				if (orderLine.JO_QtyReceived > 0)
				{
					updatedSome = true;

					var iBaseJobComInvoiceLine = invoiceHeader.Factory.New<Enterprise.Integration.Customs.IBaseJobComInvoiceLine>();
					var iBaseJobComInvoiceLineBO = (BusinessObject)iBaseJobComInvoiceLine;
					iBaseJobComInvoiceLineBO[JobComInvoiceLineSchema.JI_JZ] = invoiceHeader.PK;
					iBaseJobComInvoiceLine.SynchroniseFromForwardingOrderLine(orderLine, false);
					invoiceHeader[JobComInvoiceHeaderSchema.JZ_InvoiceAmount] = (ZDecimal)invoiceHeader[JobComInvoiceHeaderSchema.JZ_InvoiceAmount] + (ZDecimal)(iBaseJobComInvoiceLineBO[JobComInvoiceLineSchema.JI_LinePrice]);
				}

				else
				{
					updatedAll = false;
				}
			}

			result = ZString.Empty;
			if (!updatedSome)
			{
				result = Res.GetString("cfb4d80f-b23c-4d3b-af3e-f25d4a8abe77", "No Commercial Lines were appended to {0}. Please check Quantity Received has been entered on all Order Lines.", invoiceHeader.HumanReadableName);
			}
			else if (!updatedAll)
			{
				result = Res.GetString("fd397b20-684e-4673-ad7d-84d94deff2cb", "Not all Commercial Lines were appended to {0}. Please check Quantity Received has been entered on all Order Lines.", invoiceHeader.HumanReadableName);
			}

			return invoiceHeader;
		}

		BusinessObject QueryUserForInvoice(INotifications notification, BusinessObject bo)
		{
			QueryUserFindboxEventArgs args = new QueryUserFindboxEventArgs(ModuleIDs.CommercialInvoice, InvoiceList);
			if (bo != null)
			{
				args.SelectedItemPK = bo.PK;
			}
			notification.QueryUser(args);
			return (BusinessObject)Factory.Load<Enterprise.Integration.Customs.Shared.IBaseJobComInvoiceHeader>(args.SelectedItemPK);
		}

		#endregion

		#region Proxying Shipment and Milestone Dates

		IEnumerable<OrderProcessTasks> FindMilestonesAndTriggers(Event eventToProxy)
		{
			return WorkflowItems.Cast<OrderProcessTasks>()
				.Where(x => x.IsMilestoneOrWorkflowTrigger && x.P9_SE_NKMilestoneEvent == eventToProxy.Code);
		}

		void UpdateMilestonesAndTriggers(Event eventToProxy, bool updateEstimate)
		{
			EnsureProcessTasksAreCreated();

			foreach (var task in FindMilestonesAndTriggers(eventToProxy))
			{
				if (task.IsMilestone)
				{
					if (updateEstimate)
					{
						task.UpdateScheduledDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
					}

					task.UpdateActualDateFromAttachedShipmentOrDeclaration(synchronizeWhenEmpty: false);
				}
			}
		}

		void ClearMilestoneAndTriggerDates(Event eventToProxy, OrderTrackingDatesMap orderTrackingDatesMap, bool synchronizeWhenEmpty, bool clearEstimate = true, bool clearActual = true)
		{
			foreach (var workflowItem in FindMilestonesAndTriggers(eventToProxy))
			{
				if (clearEstimate && (synchronizeWhenEmpty || (orderTrackingDatesMap != null && !orderTrackingDatesMap.GetDate(workflowItem, false).IsEmpty)))
				{
					workflowItem.SetScheduledDate(ZDateTimeOffset.Empty);
				}

				if (clearActual && (synchronizeWhenEmpty || (orderTrackingDatesMap != null && !orderTrackingDatesMap.GetDate(workflowItem, true).IsEmpty)))
				{
					// TODO: Remove in WI00147775.
					ObjectFactory.Get<IActualDateWorkAround>().SetActualDateAndIKnowIShouldNotBeCallingThis(workflowItem, ZDateTimeOffset.Empty);
				}
			}
		}

		void ClearUnusedVoyageDetails()
		{
			if (PlanningVoyageState == PlanningVoyageState.TwoVoyage)
			{
				JD_E_DEP_2 = ZDateTime.Empty;
				JD_E_ARV_2ndIntermediate = ZDateTime.Empty;
			}
			else if (PlanningVoyageState == PlanningVoyageState.OneVoyage)
			{
				JD_E_DEP_2 = ZDateTime.Empty;
				JD_E_ARV_2ndIntermediate = ZDateTime.Empty;
				JD_E_DEP_3 = ZDateTime.Empty;
				JD_E_ARV_1stIntermediate = ZDateTime.Empty;
			}
		}

		void ValidateVoyageDates()
		{
			Validation.ValidateJD_Milestone_E_DEP();
			Validation.ValidateJD_E_ARV_1stIntermediate();
			Validation.ValidateJD_E_DEP_2();
			Validation.ValidateJD_E_ARV_2ndIntermediate();
			Validation.ValidateJD_E_DEP_3();
			Validation.ValidateJD_Milestone_E_ARV();
		}

		[ReadOnlyMember(nameof(IsShipmentOrExportDeclarationAttached))]
		public virtual ZDateTime JD_Milestone_A_DEP
		{
			get { return GetMilestoneActualDate(Events.Departure).ToZDateTime(); }
			set { UpdateMilestoneActual(Events.Departure, value); }
		}

		[ReadOnlyMember(nameof(IsShipmentOrExportDeclarationAttached))]
		public virtual ZDateTime JD_Milestone_E_DEP
		{
			get { return GetMilestoneEstimatedDate(Events.Departure).ToZDateTime(); }
			set
			{
				UpdateMilestoneEstimate(Events.Departure, value);
				if (!IsValidationSuspended)
				{
					ValidateVoyageDates();
				}

				JD_Milestone_E_DEPInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JD_Milestone_E_DEPInfo
		{
			get { return GetZPropertyInfo(Schema.JD_Milestone_E_DEP); }
		}

		[ReadOnlyMember(nameof(IsShipmentOrExportDeclarationAttached))]
		public virtual ZDateTime JD_Milestone_A_ARV
		{
			get { return GetMilestoneActualDate(Events.Arrival).ToZDateTime(); }
			set { UpdateMilestoneActual(Events.Arrival, value); }
		}

		[ReadOnlyMember(nameof(IsShipmentOrExportDeclarationAttached))]
		public virtual ZDateTime JD_Milestone_E_ARV
		{
			get { return GetMilestoneEstimatedDate(Events.Arrival).ToZDateTime(); }
			set
			{
				UpdateMilestoneEstimate(Events.Arrival, value);
				if (!IsValidationSuspended)
				{
					ValidateVoyageDates();
				}

				JD_Milestone_E_ARVInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JD_Milestone_E_ARVInfo
		{
			get { return GetZPropertyInfo(Schema.JD_Milestone_E_ARV); }
		}

		public override ZDateTime JD_E_ARV_1stIntermediate
		{
			get
			{
				return base.JD_E_ARV_1stIntermediate;
			}
			set
			{
				if (JD_E_ARV_1stIntermediate != value)
				{
					base.JD_E_ARV_1stIntermediate = value;
					if (!IsValidationSuspended)
					{
						ValidateVoyageDates();
					}
				}
			}
		}

		public override ZDateTime JD_E_DEP_2
		{
			get
			{
				return base.JD_E_DEP_2;
			}
			set
			{
				if (JD_E_DEP_2 != value)
				{
					base.JD_E_DEP_2 = value;
					if (!IsValidationSuspended)
					{
						ValidateVoyageDates();
					}
				}
			}
		}

		public override ZDateTime JD_E_ARV_2ndIntermediate
		{
			get
			{
				return base.JD_E_ARV_2ndIntermediate;
			}
			set
			{
				if (JD_E_ARV_2ndIntermediate != value)
				{
					base.JD_E_ARV_2ndIntermediate = value;
					if (!IsValidationSuspended)
					{
						ValidateVoyageDates();
					}
				}
			}
		}

		public override ZDateTime JD_E_DEP_3
		{
			get
			{
				return base.JD_E_DEP_3;
			}
			set
			{
				if (JD_E_DEP_3 != value)
				{
					base.JD_E_DEP_3 = value;
					if (!IsValidationSuspended)
					{
						ValidateVoyageDates();
					}
				}
			}
		}

		[EventDateProperty(AutoEvents.OrderConfirmedCode, EstimateActual.Actual)]
		public override ZDateTime JD_BookingConfDate
		{
			get { return base.JD_BookingConfDate; }
			set
			{
				if (base.JD_BookingConfDate != value)
				{
					base.JD_BookingConfDate = value;
					Logs.CreateRecreateOrUpdateEventLog(Events.OrderConfirmed, EstimateActual.Actual, value.ToOffset());
				}
			}
		}

		public ZDateTime JD_Milestone_E_EXW
		{
			get { return GetMilestoneEstimatedDate(Events.ExWorks).ToZDateTime(); }
			set
			{
				UpdateMilestoneEstimate(Events.ExWorks, value);
				JD_Milestone_E_EXWInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JD_Milestone_E_EXWInfo
		{
			get { return GetZPropertyInfo(Schema.JD_Milestone_E_EXW); }
		}

		public ZDateTime JD_Milestone_A_EXW
		{
			get { return GetMilestoneActualDate(Events.ExWorks).ToZDateTime(); }
			set
			{
				UpdateMilestoneActual(Events.ExWorks, value);
				JD_Milestone_A_EXWInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JD_Milestone_A_EXWInfo
		{
			get { return GetZPropertyInfo(Schema.JD_Milestone_A_EXW); }
		}

		public ZDateTime JD_Milestone_E_GIW
		{
			get { return GetMilestoneEstimatedDate(Events.GateIn).ToZDateTime(); }
			set
			{
				UpdateMilestoneEstimate(Events.GateIn, value);
				JD_Milestone_E_GIWInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JD_Milestone_E_GIWInfo
		{
			get { return GetZPropertyInfo(Schema.JD_Milestone_E_GIW); }
		}

		public ZDateTime JD_Milestone_A_GIW
		{
			get { return GetMilestoneActualDate(Events.GateIn).ToZDateTime(); }
			set
			{
				UpdateMilestoneActual(Events.GateIn, value);
				JD_Milestone_A_GIWInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JD_Milestone_A_GIWInfo
		{
			get { return GetZPropertyInfo(Schema.JD_Milestone_A_GIW); }
		}

		public ZDateTime JD_Milestone_E_CCC
		{
			get { return GetMilestoneEstimatedDate(Events.CustomsCommenced).ToZDateTime(); }
			set
			{
				UpdateMilestoneEstimate(Events.CustomsCommenced, value);
				JD_Milestone_E_CCCInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JD_Milestone_E_CCCInfo
		{
			get { return GetZPropertyInfo(Schema.JD_Milestone_E_CCC); }
		}

		public ZDateTime JD_Milestone_A_CCC
		{
			get { return GetMilestoneActualDate(Events.CustomsCommenced).ToZDateTime(); }
			set
			{
				UpdateMilestoneActual(Events.CustomsCommenced, value);
				JD_Milestone_A_CCCInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JD_Milestone_A_CCCInfo
		{
			get { return GetZPropertyInfo(Schema.JD_Milestone_A_CCC); }
		}

		public ZDateTime JD_Milestone_E_CLR
		{
			get { return GetMilestoneEstimatedDate(Events.CustomsCleared).ToZDateTime(); }
			set
			{
				UpdateMilestoneEstimate(Events.CustomsCleared, value);
				JD_Milestone_E_CLRInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JD_Milestone_E_CLRInfo
		{
			get { return GetZPropertyInfo(Schema.JD_Milestone_E_CLR); }
		}

		public ZDateTime JD_Milestone_A_CLR
		{
			get { return GetMilestoneActualDate(Events.CustomsCleared).ToZDateTime(); }
			set
			{
				UpdateMilestoneActual(Events.CustomsCleared, value);
				JD_Milestone_A_CLRInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JD_Milestone_A_CLRInfo
		{
			get { return GetZPropertyInfo(Schema.JD_Milestone_A_CLR); }
		}

		public ZDateTime JD_Milestone_E_CAV
		{
			get { return GetMilestoneEstimatedDate(Events.CargoAvailable).ToZDateTime(); }
			set
			{
				UpdateMilestoneEstimate(Events.CargoAvailable, value);
				JD_Milestone_E_CAVInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JD_Milestone_E_CAVInfo
		{
			get { return GetZPropertyInfo(Schema.JD_Milestone_E_CAV); }
		}

		public ZDateTime JD_Milestone_A_CAV
		{
			get { return GetMilestoneActualDate(Events.CargoAvailable).ToZDateTime(); }
			set
			{
				UpdateMilestoneActual(Events.CargoAvailable, value);
				JD_Milestone_A_CAVInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JD_Milestone_A_CAVInfo
		{
			get { return GetZPropertyInfo(Schema.JD_Milestone_A_CAV); }
		}

		public ZDateTime JD_Milestone_E_DCA
		{
			get { return GetMilestoneEstimatedDate(Events.DeliveryCartageAdvised).ToZDateTime(); }
			set
			{
				UpdateMilestoneEstimate(Events.DeliveryCartageAdvised, value);
				JD_Milestone_E_DCAInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JD_Milestone_E_DCAInfo
		{
			get { return GetZPropertyInfo(Schema.JD_Milestone_E_DCA); }
		}

		public virtual ZDateTime JD_Milestone_A_DCA
		{
			get { return GetMilestoneActualDate(Events.DeliveryCartageAdvised).ToZDateTime(); }
			set
			{
				UpdateMilestoneActual(Events.DeliveryCartageAdvised, value);
				JD_Milestone_A_DCAInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JD_Milestone_A_DCAInfo
		{
			get { return GetZPropertyInfo(Schema.JD_Milestone_A_DCA); }
		}

		public virtual ZDateTime JD_Milestone_E_DCF
		{
			get { return GetMilestoneEstimatedDate(Events.DeliveryCartageCompleteFinalised).ToZDateTime(); }
			set
			{
				UpdateMilestoneEstimate(Events.DeliveryCartageCompleteFinalised, value);
				JD_Milestone_E_DCFInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JD_Milestone_E_DCFInfo
		{
			get { return GetZPropertyInfo(Schema.JD_Milestone_E_DCF); }
		}

		public virtual ZDateTime JD_Milestone_A_DCF
		{
			get { return GetMilestoneActualDate(Events.DeliveryCartageCompleteFinalised).ToZDateTime(); }
			set
			{
				UpdateMilestoneActual(Events.DeliveryCartageCompleteFinalised, value);
				JD_Milestone_A_DCFInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JD_Milestone_A_DCFInfo
		{
			get { return GetZPropertyInfo(Schema.JD_Milestone_A_DCF); }
		}

		public ZDateTime GetMilestoneActualDate(Event eventType)
		{
			var task = WorkflowItems.Milestones[eventType];
			return task != null ? task.P9_ActualDate.ToZDateTime() : ZDateTime.Empty;
		}

		public ZDateTime GetMilestoneEstimatedDate(Event eventType)
		{
			var task = WorkflowItems.Milestones[eventType];
			return task != null ? task.P9_ScheduledDate.ToZDateTime() : ZDateTime.Empty;
		}

		public void UpdateEventEstimate(Event eventType, ZDateTimeOffset date)
		{
			EnsureProcessTasksAreCreated();
			Logs.CreateRecreateOrUpdateEventLog(eventType, EstimateActual.Estimate, date, string.Empty, GetParametersForEvent(eventType).ToArray());
		}

		public void UpdateEvent(Event eventType, ZDateTimeOffset date)
		{
			EnsureProcessTasksAreCreated();
			Logs.CreateRecreateOrUpdateEventLog(eventType, EstimateActual.Actual, date, string.Empty, GetParametersForEvent(eventType).ToArray());
		}

		protected void UpdateMilestoneEstimate(Event eventType, ZDateTime date) => UpdateMilestoneEstimate(eventType, new ZDateTimeOffset(date));

		protected void UpdateMilestoneEstimate(Event eventType, ZDateTimeOffset date)
		{
			EnsureProcessTasksAreCreated();
			new MilestoneProxyProperty(WorkflowItems, EstimateActual.Estimate, eventType).SetProperty(date);
		}

		protected internal void UpdateMilestoneActual(Event eventType, ZDateTime date) => UpdateMilestoneActual(eventType, new ZDateTimeOffset(date));

		protected internal void UpdateMilestoneActual(Event eventType, ZDateTimeOffset date)
		{
			EnsureProcessTasksAreCreated();
			new MilestoneProxyProperty(WorkflowItems, EstimateActual.Actual, eventType).SetProperty(date);
		}

		#region Events Paramaters

		IReadOnlyDictionary<string, string> GetParametersForEvent(Event eventType)
		{
			var parameters = new Dictionary<string, string>();

			if (eventType == Events.Arrival)
			{
				if (Shipment != null)
				{
					var arrivalConsol = Shipment.ArrivalConsol;
					parameters[EventConstants.EventReferenceParameters.Codes.Location] = arrivalConsol != null ? arrivalConsol.JK_RL_NKDischargePort : null;
				}
				else
				{
					parameters[EventConstants.EventReferenceParameters.Codes.Location] = JD_RL_NKPortOfDischarge;
				}
			}
			else if (eventType == Events.Departure)
			{
				if (Shipment != null)
				{
					var departureConsol = Shipment.DepartureConsol;
					parameters[EventConstants.EventReferenceParameters.Codes.Location] = departureConsol != null ? departureConsol.JK_RL_NKLoadPort : null;
				}
				else
				{
					parameters[EventConstants.EventReferenceParameters.Codes.Location] = JD_RL_NKPortOfLoading;
				}
			}
			else if (eventType == Events.GateIn)
			{
				parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.Terminal;
			}

			return parameters;
		}

		#endregion

		internal void EnsureProcessTasksAreCreated()
		{
			if (!this.processTasksAreCreated)
			{
				this.processTasksAreCreated = true;
				ProcessTaskLoader.CreateTasksAndMilestonesFromTemplateIfRequired(this, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			}
		}

		bool processTasksAreCreated;

		void UpdateDatesFromAttachedShipment()
		{
			if (IsShipmentAttached && !IsCopying && !fIsImportingData)
			{
				if (processTasksAreCreated || HasSufficientDataToCreateProcessTasks)
				{
					foreach (var eventToProxy in GetEventsThatProxyFromShipment())
					{
						UpdateMilestonesAndTriggers(eventToProxy.EventType, eventToProxy.SyncEstimate);
					}

					Logs.AddNew(Events.Attached, Shipment.LogReference(true),
						new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Type, Constants.EventReferenceParameterTypes.Shipment));

					updateDatesFromAttachedShipmentIsDelayed = false;
				}
				else
				{
					updateDatesFromAttachedShipmentIsDelayed = true;
				}
			}
		}

		bool updateDatesFromAttachedShipmentIsDelayed;

		internal void RunDelayedUpdateDatesFromAttachedShipmentIfRequired()
		{
			if (updateDatesFromAttachedShipmentIsDelayed)
			{
				UpdateDatesFromAttachedShipment();
			}
		}

		void UpdateDatesFromAttachedDeclaration()
		{
			if (IsDeclarationAttached && !IsShipmentAttached)
			{
				foreach (var eventToProxy in GetEventsThatProxyFromDeclaration())
				{
					UpdateMilestonesAndTriggers(eventToProxy.EventType, eventToProxy.SyncEstimate);
				}
			}
		}

		ZDateTime DateTimeCreationOfDeclaration
		{
			get { return Declaration != null ? (ZDateTime)Declaration[JobDeclarationSchema.JE_SystemCreateTimeUtc] : ZDateTime.Empty; }
		}

		protected internal class EventToSyncFromAttachedObject
		{
			public EventToSyncFromAttachedObject(Event eventType, bool syncEstimate = true)
			{
				EventType = eventType;
				SyncEstimate = syncEstimate;
			}

			public Event EventType { get; set; }
			public bool SyncEstimate { get; set; }
		}

		protected static internal EventToSyncFromAttachedObject[] GetEventsThatProxyFromShipment()
		{
			return new[]
			{
				new EventToSyncFromAttachedObject(Events.Departure),
				new EventToSyncFromAttachedObject(Events.Arrival),
				new EventToSyncFromAttachedObject(Events.ExportCustomsCommenced, false),
				new EventToSyncFromAttachedObject(Events.ExportCustomsCleared, false),
				new EventToSyncFromAttachedObject(Events.CustomsCommenced, false),
				new EventToSyncFromAttachedObject(Events.CustomsCleared, false),
				new EventToSyncFromAttachedObject(Events.CargoAvailable, false),
				new EventToSyncFromAttachedObject(Events.DeliveryCartageAdvised, false),
				new EventToSyncFromAttachedObject(Events.DeliveryCartageCompleteFinalised),
			};
		}

		public static string[] GetDeclarationsProxyEventCodes()
		{
			return GetAllEventsThatProxyFromDeclaration().Select(x => x.EventType.Code).ToArray();
		}

		protected static EventToSyncFromAttachedObject[] GetAllEventsThatProxyFromDeclaration()
		{
			return new[]
			{
				new EventToSyncFromAttachedObject(Events.Departure),
				new EventToSyncFromAttachedObject(Events.Arrival),
				new EventToSyncFromAttachedObject(Events.ExportCustomsCommenced, false),
				new EventToSyncFromAttachedObject(Events.ExportCustomsCleared, false),
				new EventToSyncFromAttachedObject(Events.CustomsCommenced, false),
				new EventToSyncFromAttachedObject(Events.CustomsCleared, false),
				new EventToSyncFromAttachedObject(Events.DeliveryCartageAdvised, false),
				new EventToSyncFromAttachedObject(Events.DeliveryCartageCompleteFinalised),
			};
		}

		protected internal EventToSyncFromAttachedObject[] GetEventsThatProxyFromDeclaration()
		{
			EventToSyncFromAttachedObject[] result;
			if (IsDeclarationExportAttached)
			{
				result = new EventToSyncFromAttachedObject[]
				{
					new EventToSyncFromAttachedObject(Events.Departure),
					new EventToSyncFromAttachedObject(Events.Arrival),
					new EventToSyncFromAttachedObject(Events.ExportCustomsCommenced, false),
					new EventToSyncFromAttachedObject(Events.ExportCustomsCleared, false),
					new EventToSyncFromAttachedObject(Events.CustomsCommenced, false),
					new EventToSyncFromAttachedObject(Events.CustomsCleared, false),
				};
			}
			else
			{
				result = GetAllEventsThatProxyFromDeclaration();
			}
			return result;
		}

		protected void ClearShipmentProxyDates(ForwardingShipment forwardingShipment)
		{
			foreach (var eventToProxy in GetEventsThatProxyFromShipment())
			{
				ClearMilestoneAndTriggerDates(eventToProxy.EventType, new ForwardingShipmentOrderTrackingDatesMap(forwardingShipment), false, eventToProxy.SyncEstimate);
			}
		}

		protected void ClearDeclarationProxiedDates(BusinessObject declaration)
		{
			foreach (var eventToProxy in GetEventsThatProxyFromDeclaration())
			{
				ClearMilestoneAndTriggerDates(eventToProxy.EventType, new DeclarationOrderTrackingDatesMap(declaration), false, eventToProxy.SyncEstimate);
			}
		}

		void ClearDeclarationOrderLines(BusinessObject declaration)
		{
			var invoicePKs = Factory.Load<Enterprise.Integration.Customs.Shared.ICommonJobComInvoiceHeader>(new ZQuery(JobComInvoiceHeaderSchema.JZ_JE, declaration.PK)).Select(x => x.PK).ToArray();
			var invoiceLines = Factory.Load<Enterprise.Integration.Customs.IBaseJobComInvoiceLine>(new ZQuery(JobComInvoiceLineSchema.JI_JZ, invoicePKs));
			if (invoiceLines.Any())
			{
				var orderLinePKs = OrderLines.Select(x => x.PK).ToArray();

				foreach (var invoiceLine in invoiceLines)
				{
					if (!invoiceLine.JI_JO.IsEmpty && orderLinePKs.Contains(invoiceLine.JI_JO))
					{
						invoiceLine.JI_JO = ZGuid.Empty;
					}
				}
			}
		}

		protected bool IsShipmentOrDeclarationAttached
		{
			get { return IsShipmentAttached || IsDeclarationAttached; }
		}

		protected bool IsShipmentOrExportDeclarationAttached
		{
			get { return IsShipmentAttached || IsDeclarationExportAttached; }
		}

		protected bool IsShipmentOrNotExportDeclarationAttached
		{
			get { return IsShipmentAttached || !IsDeclarationExportAttached && IsDeclarationAttached; }
		}

		protected StmALogCollection GetLogCollection(Event @event)
		{
			ZQuery filter = new ZQuery(StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, @event.Code);
			filter.AddToFilter(JoinCondition.And, StmALogSchema.SL_IsCancelled, SQLComparisonOperator.Equal, ZBool.False.ToString());
			filter.AddToFilter(JoinCondition.And, StmALogSchema.SL_IsEstimate, SQLComparisonOperator.Equal, ZBool.False.ToString());
			if (IsShipmentAttached)
			{
				filter.AddToFilter(JoinCondition.And, StmALogSchema.SL_Parent, SQLComparisonOperator.Equal, Shipment.PK);
			}
			else if (IsDeclarationAttached)
			{
				filter.AddToFilter(JoinCondition.And, StmALogSchema.SL_Parent, SQLComparisonOperator.Equal, Declaration.PK);
			}

			StmALogCollection logCollection = new StmALogCollection(Factory);
			logCollection.Load(filter);

			return logCollection;
		}

		protected ZDateTime GetLogEventTime(Event @event)
		{
			ZDateTime result = ZDateTime.Empty;
			if (IsDeclarationAttached || IsShipmentAttached)
			{
				StmALogCollection logCollection = GetLogCollection(@event);

				if (logCollection.Count > 0)
				{
					logCollection.Sort(StmALog.Schema.SL_EventTime, ListSortDirection.Descending);

					result = GetCountrySpecificLogEventTime(logCollection);
					if (!result.IsValid)
					{
						result = GetNonCountrySpecificLogEventTime(logCollection);
					}
				}
			}

			// Log Event Time is stored in the DB as datetime whereas Order's date properties are in smalldatetime
			return (result.IsValid) ? TruncateSecondsAndMilliseconds(result) : result;
		}

		ZDateTime TruncateSecondsAndMilliseconds(ZDateTime dateTime)
		{
			return new ZDateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0);
		}

		ZDateTime GetCountrySpecificLogEventTime(StmALogCollection logCollection)
		{
			return GetCountrySpecificLogEventTime(logCollection, CurrentCountryCode);
		}

		protected ZDateTime GetCountrySpecificLogEventTime(StmALogCollection logCollection, ZString countryCode)
		{
			ZDateTime result = ZDateTime.Empty;
			for (int i = 0; i < logCollection.Count; i++)
			{
				if (logCollection[i].SL_Reference.Left(2) == countryCode)
				{
					result = logCollection[i].SL_EventTime;
					break;
				}
			}
			return result;
		}

		protected virtual ZString CurrentCountryCode
		{
			get { return GlbCompany.CurrentCompany.GC_RN_NKCountryCode; }
		}

		protected ZDateTime GetNonCountrySpecificLogEventTime(StmALogCollection logCollection)
		{
			return (logCollection.Count > 0 && logCollection[0].SL_Reference.IsEmpty) ? logCollection[0].SL_EventTime : ZDateTime.Empty;
		}

		#endregion

		#region Product Quantity Summary

		public OrderLinesTotalByProductCollection ProductQuantitySummary
		{
			get
			{
				if (this.productQuantitySummary == null)
				{
					this.productQuantitySummary = new OrderLinesTotalByProductCollection(this, Factory);
					this.productQuantitySummary.Populate();
				}

				return this.productQuantitySummary;
			}
		}
		OrderLinesTotalByProductCollection productQuantitySummary;

		#endregion

		#region Lookups

		protected BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		public OrgHeaderCollection JD_OH_SendingAgents_List
		{
			get { return BindingLists.OrgForwarder_List; }
		}

		public OrgHeaderCollection JD_OH_ReceivingAgents_List
		{
			get { return BindingLists.OrgForwarder_List; }
		}

		public ShippingProviderCollection JD_OH_Carriers_List
		{
			get { return BindingLists.ShippingProvider_List; }
		}

		public ConsignorCollection SupplierList
		{
			get
			{
				if (supplierList == null)
				{
					supplierList = new ConsignorCollection(Factory);
					supplierList.OrganisationType = OrganisationTypes.Consignor;
				}
				ZGuid buyerGuid = ZGuid.Empty;
				if (Buyer != null && Buyer.SupplierLinks.Count > 0)
				{
					buyerGuid = BuyerPK;
				}
				supplierList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Consignor - Related Consignee", "Property", buyerGuid)); // Filter related
				return supplierList;
			}
		}
		ConsignorCollection supplierList;

		public OrdersBuyerCollection BuyerList
		{
			get
			{
				if (buyerList == null)
				{
					buyerList = new OrdersBuyerCollection(Factory);
					buyerList.OrganisationType = OrganisationTypes.Consignee;
				}
				ZGuid supplierGuid = ZGuid.Empty;
				if (Supplier != null && Supplier.BuyerLinks.Count > 0)
				{
					supplierGuid = SupplierPK;
				}
				buyerList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Consignee - Related Consignor", "Property", supplierGuid)); // Filter related
				return buyerList;
			}
		}
		OrdersBuyerCollection buyerList;

		public RefVesselCollection JD_RV_Vessel_List
		{
			get { return BindingLists.RefVessel_List; }
		}

		#region ServiceLevels

		public RefServiceLevelCollection JD_RS_List
		{
			get
			{
				if (Globals.IsWeb)
				{
					return WebServiceLevelCollection != null ? WebServiceLevelCollection() : null;
				}

				return BindingLists.RefServiceLevel_List;
			}
		}

		public delegate RefServiceLevelCollection WebServiceLevels();

		public WebServiceLevels WebServiceLevelCollection;

		#endregion

		public RefUNLOCOCollection JD_RL_List
		{
			get { return BindingLists.RefUNLOCO_List; }
		}

		public RefCurrencyCollection JD_RX_List
		{
			get { return BindingLists.RefCurrency_List; }
		}

		public RefCommodityCodeCollection JD_RH_List
		{
			get { return BindingLists.RefCommodity_List; }
		}

		public CodeDescriptionPairList JD_IncoTerm_List
		{
			get { return new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms); }
		}

		public CodeDescriptionPairList JD_TransportMode_List
		{
			get { return OrdersConstants.GetTransportModeList(); }
		}

		public CodeDescriptionPairList JD_ContainerMode_List
		{
			get { return OrdersConstants.GetContainerModeList(JD_TransportMode); }
		}

		public virtual CodeDescriptionPairList JD_OrderStatus_List
		{
			get
			{
				if (fJD_OrderStatus_List == null)
				{
					fJD_OrderStatus_List = new CodeDescriptionPairList(OLookUpEditType.OrderHeaderStatus);

					fJD_OrderStatus_List.AddRange(Env.Registry.OrderHeaderStatusList);

					if (Buyer != null && Buyer.MiscServ.OrderStatusList != null)
					{
						fJD_OrderStatus_List.AddRange(new CodeDescriptionPairList(Buyer.MiscServ.OrderStatusList));
					}

					if (GlbBranch.CurrentBranch.OrgProxy != null &&
						GlbBranch.CurrentBranch.OrgProxy.MiscServ.OrderStatusList != null)
					{
						fJD_OrderStatus_List.AddRange(new CodeDescriptionPairList(GlbBranch.CurrentBranch.OrgProxy.MiscServ.OrderStatusList));
					}
				}

				return fJD_OrderStatus_List;
			}
		}
		CodeDescriptionPairList fJD_OrderStatus_List;

		public ForwardingModuleShipmentCollection JD_Shipment_List
		{
			get
			{
				var filter = new ZQuery(JobShipmentSchema.JS_TransportMode, JD_TransportMode);
				var result = new ForwardingModuleShipmentCollection(Factory.CreateNewFactory());
				result.AddRelationshipFilter(filter);

				var provider = new ForwardingShipmentDefaultFilterProvider();
				provider.TransportMode = JD_TransportMode;
				provider.ContainerMode = JD_ContainerMode;
				provider.SetDefaultFilters(result);

				result.GetExtraNotificationHanlder = (bizObj) =>
				{
					if ((bizObj is CommonShipment shipment) && !this.CanAttachOrderToShipmentWithNonMatchingControllingCustomer(shipment, out var errorMessage))
					{
						return new Notification(CargoWise.EntityFramework.NotificationType.Error, errorMessage);
					}

					return null;
				};

				return result;
			}
		}

		#region Invoices

		public IBusinessObjectCollection InvoiceList
		{
			get { return invoices ?? (invoices = (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.Integration.Customs.ICommercialInvoiceCollection>(), Factory)); }
		}
		IBusinessObjectCollection invoices;

		#endregion

		public IBusinessObjectCollection JD_Booking_List
		{
			get
			{
				var query = new ZQuery();
				query.AddToFilter(ViewQuotedBookingSchema.VB_JS, SQLComparisonOperator.NotEqual, null);
				query.AddToFilter(ViewQuotedBookingSchema.VB_IsConsolidated, 0);
				var bookingsCollection = ObjectFactory.Get<BusinessObjectCollection>(nameof(IViewQuotedBookingCollection), Factory, query, null);

				if (bookingsCollection is IViewQuotedBookingCollection viewQuotedBookingCollection)
				{
					viewQuotedBookingCollection.CanHandleBothQuoteAndShipmentCodes = true;

					viewQuotedBookingCollection.GetExtraNotificationHanlder += (bizObj) =>
					{
						if ((bizObj is IViewQuotedBooking booking) && !this.CanAttachOrderToShipmentWithNonMatchingControllingCustomer(bizObj.Factory.Load<ForwardingShipment>(booking.VB_JS), out var errorMessage))
						{
							return new Notification(CargoWise.EntityFramework.NotificationType.Error, errorMessage);
						}

						return null;
					};
				}

				return bookingsCollection;
			}
		}

		BusinessObjectCollection fJD_Declaration_List;

		[ActualDataPropertyType(typeof(Enterprise.Integration.Customs.IBaseJobDeclarationCollection))]
		public BusinessObjectCollection JD_Declaration_List
		{
			get
			{
				if (fJD_Declaration_List == null)
				{
					fJD_Declaration_List = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclarationCollection>(), new object[] { Factory, GlbBranch.CurrentBranch.Country.Code });
				}

				DeclarationDefaultFilterProvider provider = new DeclarationDefaultFilterProvider();
				provider.TransportMode = this.JD_TransportMode;
				provider.ContainerMode = this.JD_ContainerMode;
				provider.SetDefaultFilters(fJD_Declaration_List);

				return fJD_Declaration_List;
			}
		}

		public CodeDescriptionPairList JD_UnitOfVolume_List
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Volume); }
		}

		public CodeDescriptionPairList JD_UnitOfWeight_List
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		public CodeDescriptionPairList JD_F3_NKPackType_List
		{
			get { return new RefPackTypeCollection(Factory).GetAsCodeDescriptionPairWithStandardUnits(); }
		}

		#endregion

		#region UniqueIndexFailureHandler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get
			{
				yield return new OrderUniqueIndexFailureHandler(this);
			}
		}

		class OrderUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			public OrderUniqueIndexFailureHandler(Order order)
			{
				this.order = order;
			}

			readonly Order order;

			#region IUniqueIndexFailureHandler Members

			public IEnumerable<string> HandledUniqueIndexNames
			{
				get { yield return JobOrderHeaderSchema.Constants.Indexes.NR_UX__JD_OrderNumber_JD_OrderNumberSplit_JD_OA_BuyerAddress; }
			}

			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				var message = GenerateMessage();
				var caption = Res.GetString("ea9d49c5-1fc2-4c5b-a3b6-0bc3ce8446d5", "Cannot Save...");
				notifier.ReportError(message, caption, indexName);
			}

			string GenerateMessage()
			{
				var orderNumberHumanReadableName = DbErrorMatch.GetColumnDescriptiveName(JobOrderHeaderSchema.Constants.TableName, JobOrderHeaderSchema.Constants.JD_OrderNumber);
				var orderNumberSplitHumanReadableName = DbErrorMatch.GetColumnDescriptiveName(JobOrderHeaderSchema.Constants.TableName, JobOrderHeaderSchema.Constants.JD_OrderNumberSplit);
				var buyerAddressHumanReadableName = DbErrorMatch.GetColumnDescriptiveName(JobOrderHeaderSchema.Constants.TableName, JobOrderHeaderSchema.Constants.JD_OA_BuyerAddress);
				var tableName = DbErrorMatch.GetTableDescriptiveName(JobOrderHeaderSchema.Constants.TableName);

				var message = Res.GetString("4d8ae5e8-eb39-4f63-a2ed-9b60eb63f867",
					"The value of {0} + {1} + {2} must be unique on {3}. The duplicate value(s) are: ({4}, {5}, {6}).",
					orderNumberHumanReadableName,
					orderNumberSplitHumanReadableName,
					buyerAddressHumanReadableName,
					tableName,
					order.JD_OrderNumber,
					order.JD_OrderNumberSplit,
					order.JD_OA_BuyerAddress);
				return message;
			}

			#endregion
		}

		#endregion

		#region ICancellable Members

		public override string CanCancel()
		{
			string result = null;
			if (IsShipmentAttached)
			{
				result = Res.GetString("59b11d37-02b0-48e7-9677-30f3455ca34c", "You cannot deactivate an Order that has a shipment attached.");
			}
			else if (IsDeclarationAttached)
			{
				result = Res.GetString("0f028003-e267-4e89-bc03-f4fff6db0cfc", "You cannot deactivate an Order that has a declaration attached.");
			}
			else if (!BuyerPK.IsValid)
			{
				result = Res.GetString("a245c2ea-44cd-48f6-9102-f4d97132747c", "You cannot deactivate an Order. Please enter a valid {0} and try again.", BuyerPKInfo.HumanReadableName);
			}
			return result;
		}

		public override ZBool JD_IsCancelled
		{
			get { return base.JD_IsCancelled; }
			set
			{
				if (JD_IsCancelled != value)
				{
					base.JD_IsCancelled = value;
					UpdateReadOnlyForWhenCancelled();
					OrderLines.MarkAsNeedingValidation();
				}
			}
		}

		#region Cancelling an Order

		[List("JD_OrderStatus_List")]
		public override ZString JD_OrderStatus
		{
			get { return base.JD_OrderStatus; }
			set
			{
				base.JD_OrderStatus = value;

				if (value == Constants.OrderStatus.Open)
				{
					Logs.CreateRecreateOrUpdateEventLog(Events.OrderPlacedFinalised, EstimateActual.Actual, ZDateTimeOffset.Now);
				}
			}
		}

		void UpdateReadOnlyForWhenCancelled()
		{
			if (!IsDeleted)
			{
				SetReadOnlyIncludingChildren(JD_IsCancelled);
			}
		}

		#endregion

		public override void RegisterEditableChildObject(IBusiness child)
		{
			base.RegisterEditableChildObject(child);
			if (!IsDeleted && JD_IsCancelled)
			{
				child.IncrementReadOnlyIncludingChildren();
			}
		}

		#endregion

		#region IJobNumber Members

		string IJobNumber.JobNumber
		{
			get { return JD_OrderNumberAndSplit; }
		}

		#endregion

		#region ICustomLabelsConfigOrgProvider Members

		event EventHandler ICustomLabelsConfigOrgProvider.ConfigOrgChanged
		{
			add
			{
				JD_OA_BuyerAddressInfo.ValueChanged += value;
				BuyerPKInfo.ValueChanged += value;

				if (ControllingCustomerDocAddress != null)
				{
					ControllingCustomerDocAddress.E2_OA_AddressInfo.ValueChanged += value;
				}
			}
			remove
			{
				JD_OA_BuyerAddressInfo.ValueChanged -= value;
				BuyerPKInfo.ValueChanged -= value;

				if (ControllingCustomerDocAddress != null)
				{
					ControllingCustomerDocAddress.E2_OA_AddressInfo.ValueChanged -= value;
				}
			}
		}

		OrgHeader ICustomLabelsConfigOrgProvider.ConfigOrg
		{
			get
			{
				if (IsDeleted)
				{
					return null;
				}

				var workflowType = ((IWorkflowProvider)this).WorkflowType;
				var collection = WorkflowDataRegistry.Instance.ClientInTemplateSelection.Value;
				var entry = collection.GetValueByCode(workflowType);

				if (entry != null)
				{
					foreach (ClientInTemplateSelectionCriteriaOrgType orgType in entry.SelectedItems)
					{
						if (orgType.OrgTypeCode == WorkflowSelectionOrgTypeCodes.ControllingCustomer
							&& DocAddresses.ContainsDocAddressType(DocAddressType.ControllingCustomer))
						{
							var controllingCustomerOrg = ControllingCustomerDocAddress?.Organisation;
							if (controllingCustomerOrg != null && !ControllingCustomerDocAddress.E2_AddressOverride)
							{
								return controllingCustomerOrg;
							}
						}
						else if (orgType.OrgTypeCode == WorkflowSelectionOrgTypeCodes.BuyerSupplier)
						{
							return this.Buyer;
						}
					}
				}

				return null;
			}
		}

		#endregion

		#region ICustomFieldProvider Members

		CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
		{
			var properties = new UserDefinedPropertyCollection(this).WithWorkflowTemplateCustomFields(this);
			AddOrganisationCustomFields(properties);

			return new CustomBusinessObject(Factory, this, properties);
		}

		void AddOrganisationCustomFields(UserDefinedPropertyCollection properties)
		{
			var customLabelsProvider = new CustomLabelsProvider(this, false);
			var list = customLabelsProvider.GetCustomFields(customLabelsProvider.ConfigOrgProvider.ConfigOrg, Factory);
			foreach (CustomLabelInfo field in list)
			{
				if (field.IsEnabled)
				{
					properties.Add(field.PropertyName, field.Caption, field.Position);
				}
			}
		}

		#endregion

		#region ITemplateCopyable Members

		public IBusiness TemplateCopy()
		{
			Order result = (Order)base.CloneInternal(new BusinessObjectCloneArgs());

			using (result.GetValidationSuspender())
			{
				OrderLines.AddToCopiedCollection(result.OrderLines);
				PlannedContainers.AddToCopiedCollection(result.PlannedContainers);

				result.JD_JS = ZGuid.Empty;
				result.JD_JE = ZGuid.Empty;
				result.JD_EF_ShipmentPrePlanning = ZGuid.Empty;

				result.JD_InvoiceNumber = ZString.Empty;
				result.JD_InvoiceDate = ZDateTime.Empty;

				result.JD_Waybill = ZString.Empty;
				result.JD_MasterWaybill = ZString.Empty;

				result.JD_RV_NKArrivalVessel = ZString.Empty;
				result.JD_RV_NKIntermediateVessel = ZString.Empty;
				result.JD_RV_NKDepartureVessel = ZString.Empty;

				result.JD_ArrivalVoyage = ZString.Empty;
				result.JD_IntermediateVoyage = ZString.Empty;
				result.JD_DepartureVoyage = ZString.Empty;

				result.JD_OH_Carrier = ZGuid.Empty;

				result.JD_BookingConfRef = ZString.Empty;
				result.JD_OrderNumber = ZString.Empty;
				result.BuyerPK = ZGuid.Empty;
				result.BuyerPK = BuyerPK;   // causes the order number to be refreshed

				result.JD_OrderStatus = Constants.OrderStatus.Incomplete;
				result.JD_OrderNumberSplit = new ZByte(0);

				result.DocAddresses.RemoveAndDeleteAll();
				foreach (var address in DocAddresses)
				{
					result.DocAddresses.Add(address.Clone());
				}

				foreach (ZPropertyInfo propertyInfo in result.ZPropertyInfoHash)
				{
					if (propertyInfo.PropertyType == typeof(ZDateTime) && propertyInfo.HasSetter)
					{
						propertyInfo.Value = ZDateTime.Empty;
					}
				}

				result.JD_OrderDate = ZDateTime.Now;
			}

			return result;
		}

		#endregion

		#region ISendEmailSource Members

		AddressBookSelection ISendEmailSource.GetAddressBookSelection()
		{
			AddressBookSelection result = new AddressBookSelection();
			result.AddRecipient(Buyer);
			result.AddRecipient(Supplier);
			result.AddRecipient(SendingAgent);
			result.AddRecipient(ReceivingAgent);
			return result;
		}

		string ISendEmailSource.EmailSubject
		{
			get { return Res.GetString("84c5a183-ffe4-473a-9747-f61a40ec5411", "Order - {0}", this.JD_OrderNumber); }
		}

		string ISendEmailSource.TemplateCategory
		{
			get { return MailTemplateCategoryList.Codes.Orders; }
		}

		string ISendEmailSource.DefaultFromDisplayName
		{
			get { return GlbStaff.CurrentUser.GS_FullName; }
		}

		string ISendEmailSource.OverridingDefaultFromEmailAddress
		{
			get { return null; }
		}

		Type ISendEmailSource.DocWrapperType
		{
			get { return ObjectFactory.GetType<DocumentWrappers.IDocOrder>(); }
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new OrderDocManagerInfo(this)); }
		}

		OrderDocManagerInfo docManagerInfo;

		#endregion

		#region IDocumentSupportable Members
		public virtual DocumentSupporter DocumentSupporter
		{
			get { return new OrderDocumentSupporter(this); }
		}

		#endregion

		#region IHaveRequiredDocuments Members

		ZString IHaveRequiredDocuments.TableCode
		{
			get { return JobOrderHeaderSchema.Constants.Prefix; }
		}

		ZString IHaveRequiredDocuments.UniqueConsignRef
		{
			get { return JD_OrderNumber; }
		}

		ZString IHaveRequiredDocuments.HouseBill
		{
			get { return null; }
		}

		ZString IHaveRequiredDocuments.MasterBill
		{
			get { return null; }
		}

		OrgHeader IHaveRequiredDocuments.ExportBroker
		{
			get { return null; }
		}

		BusinessObject IHaveRequiredDocuments.UltimateDocumentParent
		{
			get { return this; }
		}

		public IReadOnlyList<ZString> AdditionalRefTypes
		{
			get { return Array.Empty<ZString>(); }
		}

		void IHaveRequiredDocuments.PreLogAllDocumentsReceivedEvents()
		{
			if (!IsInDatabase && !IsDeleted && !requiredDocumentsAdded)
			{
				AddRequiredDocuments();
				requiredDocumentsAdded = true;
			}
		}

		bool requiredDocumentsAdded;

		#endregion

		#region ISupportDataImporting Members

		bool ISupportDataImporting.IsImportingData
		{
			get { return fIsImportingData; }
			set { fIsImportingData = value; }
		}

		bool fIsImportingData;

		#endregion

		#region ICustomLabelsProvider Members

		public class CustomLabelsProvider : ICustomLabelsProvider
		{
			public CustomLabelsProvider(ICustomLabelsConfigOrgProvider configOrgProvider, bool labelForGrids)
			{
				this.fConfigOrgProvider = configOrgProvider;
				this.fLabelForGrids = labelForGrids;
			}

			public ICustomLabelsConfigOrgProvider ConfigOrgProvider
			{
				get { return fConfigOrgProvider; }
			}

			public CustomLabelInfoList GetCustomFields(OrgHeader configOrg, BusinessObjectFactory factory)
			{
				MultilingualString est = (fLabelForGrids ? ResString.GetMultilingualString("cc4cc715-070a-4a09-9c39-30a21a929f9f", "Est.") : ResString.GetMultilingualString("f90afff4-380b-4074-8e70-253c6b068021", "Estimated"));
				MultilingualString act = (fLabelForGrids ? ResString.GetMultilingualString("3d310095-4266-4cfc-9617-dc60adb6a305", "Act.") : ResString.GetMultilingualString("56a9cc58-f7ed-493b-b5b5-6d83fa39aaed", "Actual"));

				CustomLabelInfoList result = new CustomLabelInfoList(typeof(Order), configOrg, ResString.GetMultilingualString("89346b38-c0ef-4116-be47-66ff846f178f", "the buyer of the order"), factory);

				AddTrackDate(result, Constants.CustomLabels.Order.UserTrackDate1, Schema.JD_EstimateUserDate1, Constants.CustomLabels.Order.Descriptions.UserTrackDate1, configOrg, est, factory);
				AddTrackDate(result, Constants.CustomLabels.Order.UserTrackDate1, Schema.JD_ActualUserDate1, Constants.CustomLabels.Order.Descriptions.UserTrackDate1, configOrg, act, factory);
				AddTrackDate(result, Constants.CustomLabels.Order.UserTrackDate2, Schema.JD_EstimateUserDate2, Constants.CustomLabels.Order.Descriptions.UserTrackDate2, configOrg, est, factory);
				AddTrackDate(result, Constants.CustomLabels.Order.UserTrackDate2, Schema.JD_ActualUserDate2, Constants.CustomLabels.Order.Descriptions.UserTrackDate2, configOrg, act, factory);
				AddTrackDate(result, Constants.CustomLabels.Order.UserTrackDate3, Schema.JD_EstimateUserDate3, Constants.CustomLabels.Order.Descriptions.UserTrackDate3, configOrg, est, factory);
				AddTrackDate(result, Constants.CustomLabels.Order.UserTrackDate3, Schema.JD_ActualUserDate3, Constants.CustomLabels.Order.Descriptions.UserTrackDate3, configOrg, act, factory);
				AddTrackDate(result, Constants.CustomLabels.Order.UserTrackDate4, Schema.JD_EstimateUserDate4, Constants.CustomLabels.Order.Descriptions.UserTrackDate4, configOrg, est, factory);
				AddTrackDate(result, Constants.CustomLabels.Order.UserTrackDate4, Schema.JD_ActualUserDate4, Constants.CustomLabels.Order.Descriptions.UserTrackDate4, configOrg, act, factory);

				result.Add(Constants.CustomLabels.Order.CustomAttribute1, Schema.JD_CustomAttrib1, Constants.CustomLabels.Descriptions.CustomAttribute(1));
				result.Add(Constants.CustomLabels.Order.CustomAttribute2, Schema.JD_CustomAttrib2, Constants.CustomLabels.Descriptions.CustomAttribute(2));
				result.Add(Constants.CustomLabels.Order.CustomAttribute3, Schema.JD_CustomAttrib3, Constants.CustomLabels.Descriptions.CustomAttribute(3));
				result.Add(Constants.CustomLabels.Order.CustomAttribute4, Schema.JD_CustomAttrib4, Constants.CustomLabels.Descriptions.CustomAttribute(4));
				result.Add(Constants.CustomLabels.Order.CustomAttribute5, Schema.JD_CustomAttrib5, Constants.CustomLabels.Descriptions.CustomAttribute(5));
				result.Add(Constants.CustomLabels.Order.CustomFlag1, Schema.JD_CustomFlag1, Constants.CustomLabels.Descriptions.CustomFlag(1));
				result.Add(Constants.CustomLabels.Order.CustomFlag2, Schema.JD_CustomFlag2, Constants.CustomLabels.Descriptions.CustomFlag(2));
				result.Add(Constants.CustomLabels.Order.CustomFlag3, Schema.JD_CustomFlag3, Constants.CustomLabels.Descriptions.CustomFlag(3));
				result.Add(Constants.CustomLabels.Order.CustomFlag4, Schema.JD_CustomFlag4, Constants.CustomLabels.Descriptions.CustomFlag(4));
				result.Add(Constants.CustomLabels.Order.CustomFlag5, Schema.JD_CustomFlag5, Constants.CustomLabels.Descriptions.CustomFlag(5));
				result.Add(Constants.CustomLabels.Order.CustomDecimal1, Schema.JD_CustomDecimal1, Constants.CustomLabels.Descriptions.CustomNumber(1));
				result.Add(Constants.CustomLabels.Order.CustomDecimal2, Schema.JD_CustomDecimal2, Constants.CustomLabels.Descriptions.CustomNumber(2));
				result.Add(Constants.CustomLabels.Order.CustomDecimal3, Schema.JD_CustomDecimal3, Constants.CustomLabels.Descriptions.CustomNumber(3));
				result.Add(Constants.CustomLabels.Order.CustomDecimal4, Schema.JD_CustomDecimal4, Constants.CustomLabels.Descriptions.CustomNumber(4));
				result.Add(Constants.CustomLabels.Order.CustomDecimal5, Schema.JD_CustomDecimal5, Constants.CustomLabels.Descriptions.CustomNumber(5));
				result.Add(Constants.CustomLabels.Order.CustomDate1, Schema.JD_CustomDate1, Constants.CustomLabels.Descriptions.CustomDate(1));
				result.Add(Constants.CustomLabels.Order.CustomDate2, Schema.JD_CustomDate2, Constants.CustomLabels.Descriptions.CustomDate(2));

				result.Add(Constants.CustomLabels.Order.CustomContact1, Schema.JD_FirstBuyerContact, typeof(OrgContact), Constants.CustomLabels.Order.Descriptions.CustomContact1, (NoResString)"");
				result.Add(Constants.CustomLabels.Order.CustomContact2, Schema.JD_SecondBuyerContact, typeof(OrgContact), Constants.CustomLabels.Order.Descriptions.CustomContact2, (NoResString)"");

				return result;
			}

			protected ICustomLabelsConfigOrgProvider fConfigOrgProvider;
			protected bool fLabelForGrids;

			void AddTrackDate(CustomLabelInfoList list, string labelName, string propertyName, MultilingualString defaultCaption, OrgHeader configOrg, MultilingualString prefix, BusinessObjectFactory factory)
			{
				TrackDateCustomLabelInfo info = new TrackDateCustomLabelInfo(labelName, propertyName, typeof(ZDateTime), defaultCaption, null, CustomLabelStyles.None, configOrg, prefix, factory);
				list.Add(info);
			}

			class TrackDateCustomLabelInfo : CustomLabelInfo
			{
				public TrackDateCustomLabelInfo(string labelName, string propertyName, Type propertyType, MultilingualString defaultCaption, MultilingualString defaultHint, CustomLabelStyles styles, OrgHeader org, MultilingualString prefix, BusinessObjectFactory factory)
					: base(labelName, propertyName, propertyType, defaultCaption, defaultHint, styles, org, factory)
				{
					this.prefix = prefix;
				}

				readonly MultilingualString prefix;

				public override MultilingualString Caption
				{
					get
					{
						MultilingualString caption = base.Caption;
						if (!string.IsNullOrEmpty(caption) && !string.IsNullOrEmpty(this.prefix))
						{
							caption = MultilingualString.Join(" ", this.prefix, caption);
						}

						return caption;
					}
				}
			}
		}

		#endregion

		#region ICalendarReminder Members

		internal Reminder FollowUpDateReminder
		{
			get
			{
				string body = Res.GetString("fd386b00-739c-4e51-80f2-8fbf023a34e0", "Follow up Order {0} Ordered By: {1}", JD_OrderNumberAndSplit, Buyer.OH_FullNameTruncated) + "\r\n\r\n";
				body += Res.GetString("470ed134-a154-4a3f-b6b2-13d88f9de15a", "Buyer Contact Details: {0}", Buyer.OH_FullNameTruncated) + "\r\n";
				OrgContact buyerContact = new DefaultContactFinder(Buyer).DefaultContact(ContactType.Consignee);
				if (buyerContact != null)
				{
					body += Res.GetString("b9a41889-4998-4bcc-b32f-2167e91c8aac", "Contact: {0}\r\nEmail: {1}\r\nFax: {2}\r\nPhone: {3}", buyerContact.OC_ContactName, buyerContact.OC_Email, buyerContact.OC_Fax, buyerContact.OC_Phone) + "\r\n\r\n";
				}

				if (Supplier != null)
				{
					body += Res.GetString("0efe9903-5499-490e-90bf-31d2aa6afc77", "Supplier Contact Details: {0}", Supplier.OH_FullNameTruncated) + "\r\n";

					OrgContact supplierContact = new DefaultContactFinder(Supplier).DefaultContact(ContactType.Consignor);
					if (supplierContact != null)
					{
						body += Res.GetString("f9372254-7d31-40e2-831a-e9eb5c87d370", "Contact: {0}\r\nEmail: {1}\r\nFax: {2}\r\nPhone: {3}", supplierContact.OC_ContactName, supplierContact.OC_Email, supplierContact.OC_Fax, supplierContact.OC_Phone) + "\r\n";
					}
				}

				Reminder result = new Reminder(PK.ToString(), PK, new ZString(TableName), DateTimeKind.Local, JD_FollowUpDate, JD_FollowUpDate, Res.GetString("ed6d829c-ebaf-4833-a29a-1ebeb42fa8db", "Followup Order: {0}, Ordered By: {1}", JD_OrderNumberAndSplit, Buyer.OH_FullNameTruncated), body);
				return result;
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded && ShouldCreateReminder)
			{
				FollowUpDateReminder.CreateAppointment();
			}

			if (saveSucceeded)
			{
				OriginalJD_FollowUpDate = JD_FollowUpDate;
			}
		}

		internal bool ShouldCreateReminder
		{
			get { return !JD_FollowUpDate.IsEmpty && JD_FollowUpDate.IsValid && Buyer != null && JD_FollowUpDate != OriginalJD_FollowUpDate; }
		}

		#endregion

		#region IWorkflowProvider Members

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowDescriptors.OrderWorkflowDescriptorCode; }
		}

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get { return WorkflowItems; }
		}

		[ChildEditable(true)]
		public OrderProcessTasksCollection WorkflowItems
		{
			get
			{
				if (fWorkflowItems == null)
				{
					fWorkflowItems = this.GetOrCreateProcessTaskCollection(CreateProcessTaskCollection);
					RegisterEditableChildObject(fWorkflowItems);
				}
				return fWorkflowItems;
			}
		}
		OrderProcessTasksCollection fWorkflowItems;

		protected virtual OrderProcessTasksCollection CreateProcessTaskCollection()
		{
			return new OrderProcessTasksCollection(this);
		}

		[ChildEditable(true)]
		public MilestoneCollectionView MilestoneView
		{
			get
			{
				if (milestoneView == null)
				{
					milestoneView = new MilestoneCollectionViewConstantSize(WorkflowItems);
					RegisterEditableChildObject(milestoneView);
				}

				return milestoneView;
			}
		}
		MilestoneCollectionView milestoneView;

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			ColumnValueRanker result = new ColumnValueRanker();

			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, GetClientsInTemplateSelectionOrder());
			result.Add(ProcessTaskTemplateSchema.P0_SubType1, JD_TransportMode, ZString.Empty);

			return result;
		}

		IZType[] GetClientsInTemplateSelectionOrder()
		{
			var result = new List<IZType>();
			var workflowType = ((IWorkflowProvider)this).WorkflowType;

			var collection = WorkflowDataRegistry.Instance.ClientInTemplateSelection.Value;
			var entry = collection.GetValueByCode(workflowType);

			if (entry != null)
			{
				foreach (ClientInTemplateSelectionCriteriaOrgType orgType in entry.SelectedItems)
				{
					if (orgType.OrgTypeCode == WorkflowSelectionOrgTypeCodes.ControllingCustomer
						&& DocAddresses.ContainsDocAddressType(DocAddressType.ControllingCustomer))
					{
						if (ControllingCustomerDocAddress?.Organisation != null)
						{
							result.Add(ControllingCustomerDocAddress.Organisation.PK);
						}
					}
					else if (orgType.OrgTypeCode == WorkflowSelectionOrgTypeCodes.BuyerSupplier)
					{
						if (ImportExportHelper.IsImport(JD_RL_NKGoodsAvailableAt, JD_RL_NKGoodsDeliveredTo))
						{
							result.Add(BuyerPK);
							result.Add(SupplierPK);
						}
						else
						{
							result.Add(SupplierPK);
							result.Add(BuyerPK);
						}
					}
				}

				result.Add(ZGuid.Empty);
			}

			return result.ToArray();
		}

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return WorkflowInformationProvider;
		}

		WorkflowInformationProvider WorkflowInformationProvider
		{
			get
			{
				if (workflowInformationProvider == null)
				{
					var companies = Factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_IsActive, ZBool.True));
					var companyPks = companies.Where(x => (SendingAgent != null && SendingAgent.IsProxyOrg(x))
											|| (ReceivingAgent != null && ReceivingAgent.IsProxyOrg(x)))
										.Select(x => x.PK).ToArray();

					workflowInformationProvider = new WorkflowInformationProvider(companyPks);
				}

				workflowInformationProvider.Destination = (PortOfDischarge != null) ? PortOfDischarge.RL_PortName : ZString.Empty;
				workflowInformationProvider.Origin = (PortOfLoading != null) ? PortOfLoading.RL_PortName : ZString.Empty;
				workflowInformationProvider.BusinessContext = TrackingConstants.BusinessContext.Order;

				return workflowInformationProvider;
			}
		}
		WorkflowInformationProvider workflowInformationProvider;

		bool HasSufficientDataToCreateProcessTasks
		{
			get { return !IsCopying && !JD_TransportMode.IsEmpty && !JD_TransportModeInfo.HasErrors() && Buyer != null; }
		}
#if DEBUG
		public
#endif
		class MilestoneCollectionViewConstantSize : MilestoneCollectionView
		{
			public MilestoneCollectionViewConstantSize(ProcessTaskCollection collection)
				: base(collection)
			{
			}

			protected override bool AllowNewCore
			{
				get { return false; }
			}

			protected override bool AllowRemoveCore
			{
				get { return false; }
			}

			protected override void RebuildCore()
			{
				base.RebuildCore();
				Sort(ProcessTasksSchema.Constants.P9_Sequence);
				if (!Env.Security.FindOrCreateWorkflowItemCheckpoint(Env.Security.OrderTracking, SecurityCore.WorkflowMilestonesAutoGeneratedCode).IsAllowed)
				{
					foreach (ProcessTask milestone in this)
					{
						milestone.ReadOnly = true;
					}
				}
			}
		}

		#endregion

		#region IWorkflowTriggerFieldChangeSource Members

		IReadOnlyList<IWorkflowProvider> IWorkflowTriggerFieldChangeSource.ParentWorkflowProviders
		{
			get
			{
				List<IWorkflowProvider> result = new List<IWorkflowProvider>();
				if (PreAdvice != null)
				{
					result.Add(PreAdvice);
				}
				return result.ToArray();
			}
		}

		#endregion

		#region Implementation

		void RefreshOrderNumberReadOnly()
		{
			RefreshBinding();
		}

		ZDecimal OrderLineTotalQuantity => OrderLines.Sum(l => l.JO_Quantity);
		ZDecimal OrderLineTotalReceived => OrderLines.Sum(orderLine => orderLine.JO_QtyReceived);

		public bool HasOutstandingBalance
		{
			get
			{
				if (OrderLines.Any(orderLine => orderLine.HasOutstandingBalance))
				{
					ZQuery splitsFilter = new ZQuery();
					splitsFilter.AddToFilter(JobOrderHeaderSchema.JD_OrderNumber, JD_OrderNumber);
					splitsFilter.AddToFilter(JobOrderHeaderSchema.JD_OrderNumberSplit, SQLComparisonOperator.GreaterThan, JD_OrderNumberSplit);
					splitsFilter.AddToFilter(JobOrderHeaderSchema.JD_OA_BuyerAddress, JD_OA_BuyerAddress);

					ZDecimal orderSplitsTotalReceived = Factory.Load<Order>(splitsFilter).Sum(splitOrder => splitOrder.OrderLineTotalReceived);

					return (OrderLineTotalQuantity - OrderLineTotalReceived - orderSplitsTotalReceived) != 0;
				}

				return false;
			}
		}

		protected virtual void UpdateOrderStatus()
		{
			bool hasOrderLines = OrderLines.Count > 0;
			bool hasOrderLineContainers = false;
			bool atLeast1ContainerDelivered = false;
			bool allContainersDelivered = true;

			ZString orderStatus = "";

			if (!JD_BookingConfRef.IsEmpty)
			{
				orderStatus = Constants.OrderStatus.Confirmed;
			}

			if (JD_JS.IsValid || JD_JE.IsValid)
			{
				orderStatus = Constants.OrderStatus.Shipped;
			}

			foreach (OrderLine orderLine in OrderLines)
			{
				foreach (OrderLineDelivery delivery in orderLine.Deliveries)
				{
					foreach (OrderLineDeliverContainer container in delivery.Containers)
					{
						hasOrderLineContainers = true;
						if (container.IsDelivered)
						{
							atLeast1ContainerDelivered = true;
						}
						else
						{
							allContainersDelivered = false;
						}
					}
				}
			}

			if (atLeast1ContainerDelivered)
			{
				orderStatus = Constants.OrderStatus.PartDelivered;
			}

			var actualDeliveryTime = GetMilestoneActualDate(Events.DeliveryCartageCompleteFinalised);
			if ((hasOrderLineContainers && allContainersDelivered && !HasOutstandingBalance && !actualDeliveryTime.IsEmpty)
				|| (!hasOrderLineContainers && hasOrderLines && !HasOutstandingBalance && !actualDeliveryTime.IsEmpty)
				|| (!hasOrderLines && actualDeliveryTime.IsValid)
				|| IsAttachedShipmentDelivered
				|| IsAttachedDeclarationDelivered)
			{
				orderStatus = Constants.OrderStatus.Delivered;
			}

			if (!orderStatus.IsEmpty)
			{
				JD_OrderStatus = orderStatus;
			}
		}

		bool IsAttachedShipmentDelivered
		{
			get { return Shipment != null && !Shipment.IsDeleted && IsDeliverCompleted(Shipment.DocsAndCartage); }
		}

		bool IsAttachedDeclarationDelivered
		{
			get
			{
				bool result = false;

				if (Declaration != null)
				{
					JobDocsAndCartage decDoc = JobDocsAndCartage.Load((IShipmentWithDocsAndCartage)Declaration);
					if (decDoc != null)
					{
						result = IsDeliverCompleted(decDoc);
					}
				}

				return result;
			}
		}

		bool IsDeliverCompleted(JobDocsAndCartage docsAndCartage)
		{
			return (OrdersDataRegistry.Instance.AutoSetDeliveredForOrdersForExternalCartage.Value &&
				docsAndCartage.DeliveryCartageCo != null && !docsAndCartage.DeliveryCartageCo.IsProxyOrgOfAnyCompany())
				||
				!docsAndCartage.JP_DeliveryCartageCompleted.IsEmpty;
		}

		ProcessTask.Loader ProcessTaskLoader
		{
			get { return processTaskLoader ?? (processTaskLoader = new ProcessTask.Loader(Factory)); }
		}
		ProcessTask.Loader processTaskLoader;

		#endregion

		#region IDocAddresses Members

		[ChildEditable(true)]
		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (fDocAddresses == null)
				{
					fDocAddresses = new JobDocAddressDependentCollection(this);
					fDocAddresses.Load();
					RegisterEditableChildObject(fDocAddresses);
				}

				return fDocAddresses;
			}
		}
		JobDocAddressDependentCollection fDocAddresses;

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return new JobOrderJobDocAddressValidation(this, addressToValidate);
		}

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes => GetSupportedAddressTypes().ToArray();

		static IEnumerable<DocAddressType> GetSupportedAddressTypes()
		{
			yield return DocAddressType.ControllingCustomer;
			yield return DocAddressType.Warehouse;
			yield return DocAddressType.ControllingAgent;
			yield return DocAddressType.NotifyParty;
			yield return DocAddressType.NotifyParty2;
			yield return DocAddressType.NotifyParty3;
			yield return DocAddressType.GoodsAvailableAt;
			yield return DocAddressType.GoodsDeliveredTo;
			yield return DocAddressType.Manufacturer;

			if (AdvOrmFeatureHelper.IsEnabled)
			{
				yield return DocAddressType.ConsigneeDocumentaryAddress;
			}
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.ControllingAgent:
					{
						return ControllingAgentDocAddressRequirement;
					}
				case DocAddressType.ControllingCustomer:
					{
						return ControllingCustomerDocAddressRequirement;
					}
				case DocAddressType.GoodsAvailableAt:
					{
						return GoodsAvailableAtAddressRequirement;
					}
				case DocAddressType.GoodsDeliveredTo:
					{
						return GoodsDeliveredToAddressRequirement;
					}
				default:
					return null;
			}
		}

		JobDocAddressRequirement ControllingAgentDocAddressRequirement
		{
			get
			{
				if (controllingAgentDocAddressRequirement == null)
				{
					controllingAgentDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.ControllingAgent)
					{
						ValidateOrganisationPK = Validation.ValidateControllingAgentPK,
						ValidateDelegateToBeFiredUponValidationOf_E2_OA_Address = validation => validation.ValidateOrganisationPK()
					};
				}

				return controllingAgentDocAddressRequirement;
			}
		}
		JobDocAddressRequirement controllingAgentDocAddressRequirement;

		JobDocAddressRequirement ControllingCustomerDocAddressRequirement
		{
			get
			{
				if (controllingCustomerDocAddressRequirement == null)
				{
					controllingCustomerDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.ControllingCustomer)
					{
						ValidateOrganisationPK = Validation.ValidateControllingCustomerPK,
						ValidateDelegateToBeFiredUponValidationOf_E2_OA_Address = validation => validation.ValidateOrganisationPK(),
					};
				}

				return controllingCustomerDocAddressRequirement;
			}
		}
		JobDocAddressRequirement controllingCustomerDocAddressRequirement;

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.OrderManager;
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
			=> docAddress.DocAddressType == DocAddressType.ControllingCustomer && !HasLinkedSupplierBookingInProgress;

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			OrgHeaderCollection result = null;
			if (addressType == DocAddressType.Warehouse)
			{
				result = new OrgHeaderCollection(Factory);
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property9", ZBool.True));
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Secondary Type", "Property", OrganisationSecondaryTypes.Depot));
			}
			return result;
		}

		#endregion

		#region IBuyerSupplierRelationshipConsumer Members

		public BuyerSupplierLinksHelper<Order> BuyerSupplierLinksHelper;

		ZBool IBuyerSupplierRelationshipConsumer.IsSettingDefaultValues
		{
			get { return IsSettingDefaults; }
			set { IsSettingDefaults = value; }
		}

		bool IsSettingDefaults;

		OrgHeader IBuyerSupplierRelationshipConsumer.Consignee
		{
			get { return Buyer; }
			set { BuyerPK = value.PK; }
		}

		event EventHandler IBuyerSupplierRelationshipConsumer.ConsigneeChanged
		{
			add { JD_OA_BuyerAddressInfo.ValueChanged += value; }
			remove { JD_OA_BuyerAddressInfo.ValueChanged -= value; }
		}

		JobDocAddress IBuyerSupplierRelationshipConsumer.ConsigneeDeliveryAddress
		{
			get { return null; }
		}

		void IBuyerSupplierRelationshipConsumer.RestoreImportBrokerFallback()
		{
		}

		void IBuyerSupplierRelationshipConsumer.RestoreEFreightStatusFallback(ZString defaultValue)
		{
		}

		ZBool IBuyerSupplierRelationshipConsumer.PreventBuyerSupplierRelationships
		{
			get { return false; }
		}

		OrgHeader IBuyerSupplierRelationshipConsumer.Consignor
		{
			get { return Supplier; }
			set
			{
				if (!fIsImportingData)
				{
					SupplierPK = value.PK;
				}
			}
		}

		event EventHandler IBuyerSupplierRelationshipConsumer.ConsignorChanged
		{
			add { JD_OA_SupplierAddressInfo.ValueChanged += value; }
			remove { JD_OA_SupplierAddressInfo.ValueChanged -= value; }
		}

		JobDocAddress IBuyerSupplierRelationshipConsumer.ConsignorPickupAddress
		{
			get { return null; }
		}

		event EventHandler IBuyerSupplierRelationshipConsumer.ModesChanged
		{
			add
			{
				JD_TransportModeInfo.ValueChanged += value;
				JD_ContainerModeInfo.ValueChanged += value;
			}
			remove
			{
				JD_TransportModeInfo.ValueChanged -= value;
				JD_ContainerModeInfo.ValueChanged -= value;
			}
		}

		ZString IBuyerSupplierRelationshipConsumer.ContainerMode
		{
			get { return JD_ContainerMode; }
			set { JD_ContainerMode = value; }
		}

		ZString IBuyerSupplierRelationshipConsumer.TransportMode
		{
			get { return JD_TransportMode; }
			set { JD_TransportMode = value; }
		}

		ZGuid IBuyerSupplierRelationshipConsumer.DeliveryCartageCoPK
		{
			get { return ZGuid.Empty; }
			set { }
		}

		ZString IBuyerSupplierRelationshipConsumer.Destination
		{
			get { return JD_RL_NKGoodsDeliveredTo; }
			set
			{
				if (!(fIsImportingData && !JD_RL_NKGoodsDeliveredTo.IsEmpty))
				{
					JD_RL_NKGoodsDeliveredTo = value;
				}
			}
		}

		ZString IBuyerSupplierRelationshipConsumer.DischargePort
		{
			get { return JD_RL_NKPortOfDischarge; }
			set { JD_RL_NKPortOfDischarge = value; }
		}

		ZString IBuyerSupplierRelationshipConsumer.GoodsCurrency
		{
			get { return OrderCurrency != null ? JD_RX_NKOrderCurrency : ZString.Empty; }
			set
			{
				RefCurrency newCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, value);
				if (newCurrency != null)
				{
					JD_Calc_Currency.Currency = newCurrency.RX_Code;
				}
			}
		}

		void IBuyerSupplierRelationshipConsumer.RestoreGoodsCurrencyFallback()
		{
			if (Supplier != null)
			{
				JD_Calc_Currency.Currency = Supplier.MiscServ.OM_RX_NKEXDefCurrency;
			}
		}

		ZString IBuyerSupplierRelationshipConsumer.GoodsDescription
		{
			get { return JD_OrderGoodsDescription; }
			set { JD_OrderGoodsDescription = value; }
		}

		ZGuid IBuyerSupplierRelationshipConsumer.ImportBrokerPK
		{
			get { return ZGuid.Empty; }
			set { }
		}

		ZString IBuyerSupplierRelationshipConsumer.LoadPort
		{
			get { return JD_RL_NKPortOfLoading; }
			set { JD_RL_NKPortOfLoading = value; }
		}

		ZBool IBuyerSupplierRelationshipConsumer.ShouldPromptToSaveBuyerSupplierRelationship
		{
			get { return Env.Registry.PromptToSaveBuyerSupplier; }
		}

		ZByte IBuyerSupplierRelationshipConsumer.NoCopyBills
		{
			get { return 0; }
			set { }
		}

		ZByte IBuyerSupplierRelationshipConsumer.NoOriginalBills
		{
			get { return 0; }
			set { }
		}

		void IBuyerSupplierRelationshipConsumer.RestoreNumberOfBillsWithFallback()
		{
		}

		JobDocAddress IBuyerSupplierRelationshipConsumer.NotifyPartyDocumentaryAddress
		{
			get { return null; }
		}

		ZString IBuyerSupplierRelationshipConsumer.Origin
		{
			get { return JD_RL_NKGoodsAvailableAt; }
			set
			{
				if (!(fIsImportingData && !JD_RL_NKGoodsAvailableAt.IsEmpty))
				{
					JD_RL_NKGoodsAvailableAt = value;
				}
			}
		}

		ZString IBuyerSupplierRelationshipConsumer.PaymentTerms
		{
			get { return JD_IncoTerm; }
			set { JD_IncoTerm = value; }
		}

		ZBool IBuyerSupplierRelationshipConsumer.ShouldRestorePaymentTerm
		{
			get { return !this.IsDomestic(); }
		}

		ZBool IBuyerSupplierRelationshipConsumer.ShouldRestoreHandlingInformation
		{
			get { return true; }
		}

		ZGuid IBuyerSupplierRelationshipConsumer.PickupCartageCoPK
		{
			get { return ZGuid.Empty; }
			set { }
		}

		ZGuid IBuyerSupplierRelationshipConsumer.ReceivingAgentPK
		{
			get { return JD_OH_ReceivingAgent; }
			set { JD_OH_ReceivingAgent = value; }
		}

		ZGuid IBuyerSupplierRelationshipConsumer.SendingAgentPK
		{
			get { return JD_OH_SendingAgent; }
			set { JD_OH_SendingAgent = value; }
		}

		void IBuyerSupplierRelationshipConsumer.RestoreSendingAgentFallback()
		{
			RefUNLOCO port = PortOfLoading;
			if (port != null)
			{
				OrgAddress agent = port.GetBestAgent(JD_TransportMode, AgentDirectionList.Codes.Export);
				if (agent != null)
				{
					JD_OH_SendingAgent = agent.Header.PK;
				}
			}
		}

		void IBuyerSupplierRelationshipConsumer.RestoreReceivingAgentFallback()
		{
			RefUNLOCO port = PortOfDischarge;
			if (port != null)
			{
				OrgAddress agent = port.GetBestAgent(JD_TransportMode, AgentDirectionList.Codes.Import);
				if (agent != null)
				{
					JD_OH_ReceivingAgent = agent.Header.PK;
				}
			}
		}

		ZString IBuyerSupplierRelationshipConsumer.ReleaseType
		{
			get { return ""; }
		}

		ZString IBuyerSupplierRelationshipConsumer.ServiceLevel
		{
			get { return JD_RS_NKServiceLevel_NI; }
			set { JD_RS_NKServiceLevel_NI = value; }
		}

		void IBuyerSupplierRelationshipConsumer.RestoreServiceLevelFallback()
		{
			if (Buyer != null && !Buyer.MiscServ.OM_RS_NKIMDefaultServiceLevel.IsEmpty)
			{
				JD_RS_NKServiceLevel_NI = Buyer.MiscServ.OM_RS_NKIMDefaultServiceLevel;
			}
		}

		ZGuid IBuyerSupplierRelationshipConsumer.ShippingLinePK
		{
			get { return JD_OH_Carrier; }
			set { JD_OH_Carrier = value; }
		}

		ZBool IBuyerSupplierRelationshipConsumer.ShouldRestorePickupDeliveryAndNotifyPartyAddress
		{
			get { return true; }
		}

		ZBool IBuyerSupplierRelationshipConsumer.ShouldDefaultContainerModeAndIsContainerised(ZString containerMode) => false;

		#endregion

		#region IRelatedItemsNameProvider Members

		ZString IRelatedItemsNameProvider.GetNameOfRelatedItem(IBusiness relatedItem)
		{
			if (relatedItem is Order)
			{
				return null;
			}

			ForwardingShipment forwardingShipment = null;

			if (Shipment != null)
			{
				forwardingShipment = Shipment;
			}
			else
			{
				var declaration = Declaration as Enterprise.Integration.Customs.IBaseJobDeclaration;
				if (declaration != null && !declaration.JE_JS.IsEmpty)
				{
					forwardingShipment = Factory.Load<ForwardingShipment>(declaration.JE_JS);
				}
			}

			if (forwardingShipment != null)
			{
				if (Shipment.PK == ((BusinessObject)relatedItem).PK)
				{
					return OrderWorkflowEventContextMasterTypes.Descriptions.Master;
				}
				else
				{
					var result = ((IRelatedItemsNameProvider)forwardingShipment).GetNameOfRelatedItem(relatedItem);
					if (!result.IsEmpty)
					{
						return OrderWorkflowEventContextMasterTypes.Descriptions.Master + ", " + result;
					}
				}
			}

			return ZString.Empty;
		}

		#endregion
		#region Implementation of ICountrySpecificSuppressions

		bool IFlightDetailsSuppression.IsAir
		{
			get { return JD_TransportMode == Constants.TransportModes.Air; }
		}

		ZBool IFlightDetailsSuppression.HasActualRCVPassed
		{
			get { return (!GetMilestoneActualDate(Events.Arrival).IsEmpty && ZDateTime.Now > GetMilestoneActualDate(Events.Arrival)); }
		}

		ZBool IFlightDetailsSuppression.HasETDPassed
		{
			get
			{
				return !GetMilestoneEstimatedDate(Events.Departure).IsEmpty &&
					ZDateTime.Now > GetMilestoneEstimatedDate(Events.Departure);
			}
		}

		ZBool IFlightDetailsSuppression.IsPassengerFlight
		{
			get { return false; }
		}

		ZBool IFlightDetailsSuppression.HasFinalRoutingLegATDPassed
		{
			get { return true; }
		}

		#endregion

		#region IImportExport Members

		public Directions JobDirection
		{
			get { return ImportExportHelper.GetJobDirection(ActualPortOfLoading, ActualPortOfDischarge); }
		}

		#endregion

		#region ICommonInvoice Members

		AllChargesCollection ICommonInvoice.AllCharges
		{
			get
			{
				if (allCharges == null)
				{
					allCharges = new AllChargesCollection(this);
					allCharges.Load();
				}
				return allCharges;
			}
		}
		AllChargesCollection allCharges;

		RefCurrencyCurrencyConverter ICommonInvoice.CurrencyConverter
		{
			get { return CurrencyConverter; }
		}

		ICommonInvoice ICommonInvoice.ImmediateCommonInvoiceParent
		{
			get { return null; }
		}

		ZString ICommonInvoice.IncoTerm
		{
			get { return JD_IncoTerm; }
		}

		IApportionInvoiceHolder ICommonInvoice.InvoicesHolder
		{
			get { return this; }
		}

		ZString ICommonInvoice.LocalCurrencyCode
		{
			get
			{
				RefCountry country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryOfImport);

				return country != null ? country.RN_RX_NKLocalCurrency : GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			}
		}

		bool ICommonInvoice.HasMultipleInvoiceUQs
		{
			get { return false; }
		}

		#endregion

		#region IApportionInvoiceHolder Members

		protected void OnApportionmentBeingDirty(object sender, EventArgs e)
		{
			ApportionmentDirty = true;
		}

		event EventHandler ApportionmentDirtyChangedEventHandler
		{
			add
			{
				JD_IncoTermInfo.ValueChanged += value;
				JD_EstimatedExchangeRateInfo.ValueChanged += value;
			}
			remove
			{
				JD_IncoTermInfo.ValueChanged -= value;
				JD_EstimatedExchangeRateInfo.ValueChanged -= value;
			}
		}

		IncoTermAndCustomsChargeFactory IApportionInvoiceHolder.IncoTermAndChargeFactory
		{
			get
			{
				var countryOfImport = CountryOfImport;
				countryOfImport = countryOfImport.IsEmpty ? GlbCompany.CurrentCompany.GC_RN_NKCountryCode : countryOfImport;
				return IncoTermAndCustomsChargeFactory.GetByCountryCode(countryOfImport);
			}
		}

		IComparer IApportionInvoiceHolder.ChargeComparer
		{
			get { return null; }
		}

		IChargeHolder[] IApportionInvoiceHolder.ChargeHolders
		{
			get { return new IChargeHolder[] { this }; }
		}

		IChargeApportionee[] IApportionInvoiceHolder.Invoices
		{
			get { return new IChargeApportionee[] { this }; }
		}

		public bool ApportionmentDirty
		{
			get;
			private set;
		}

		void IApportionInvoiceHolder.MarkApportionmentDirty()
		{
			ApportionmentDirty = true;
		}

		void IApportionInvoiceHolder.UpdateApportionmentProgress()
		{
		}

		void IApportionInvoiceHolder.ValidateIncoTerms()
		{
			Validation.ValidateJD_IncoTerm();
		}

		IComparer<IChargeApportionee> IApportionInvoiceHolder.LineChargeApportioneeComparer
		{
			get { return new OrderLineComparer(); }
		}

		string IApportionInvoiceHolder.CountryContext
		{
			get { return CountryOfImport; }
		}

		#endregion

		#region IChargeHolder Members

		public void ApportionChargesIfNecessary()
		{
			if (ApportionmentDirty)
			{
				ApportionCharges();
			}
		}

		void ApportionCharges()
		{
			ApportionManager manager = new ApportionManager(this, new ApportionStrategy());
			manager.ApportionAll();

			ApportionmentDirty = false;
		}

		CurrencyConverterWithFixedExchangeRatesDataProvider CurrencyConverter
		{
			get { return currencyConverter ?? (currencyConverter = new CurrencyConverterWithFixedExchangeRatesDataProvider(Factory, this)); }
		}
		CurrencyConverterWithFixedExchangeRatesDataProvider currencyConverter;

		[ChildEditable(true)]
		public JobComInvChargeCollection<JobComInvCharge> Charges
		{
			get
			{
				if (charges == null)
				{
					charges = new JobComInvChargeCollection(this);
					RegisterEditableChildObject(charges);
				}
				return charges;
			}
		}
		JobComInvChargeCollection<JobComInvCharge> charges;

		public ZBool JD_ChargesVisible
		{
			get { return ((ILandedCostHeader)this).IsLCSupported; }
		}

		IChargeApportionee[] IChargeHolder.AllApportionees
		{
			get { return NonCancelledOrderLines.ToArray(); }
		}

		IJobComInvChargeCollection<Customs.Common.JobComInvCharge> IChargeHolder.Charges
		{
			get { return Charges; }
		}

		CurrencyConverter IChargeHolder.CurrencyConverter
		{
			get { return CurrencyConverter; }
		}

		IChargeHolder[] IChargeHolder.ImmediateChargeHolderChildren
		{
			get { return NonCancelledOrderLines.ToArray(); }
		}

		IChargeHolder IChargeHolder.ImmediateChargeHolderParent
		{
			get { return null; }
		}

		bool IChargeHolder.IsGroupInvoice
		{
			get { return false; }
		}

		ZString IChargeHolder.GetDefaultCurrencyCode(ICustomsChargeCode chargeType, Customs.Common.JobComInvCharge charge)
		{
			return JD_RX_NKOrderCurrency;
		}

		ZString IChargeHolder.GetDefaultDistributeBy() => ZString.Empty;

		#endregion

		#region ICurrencyConverterDataProviderWithFixedExRates Members

		decimal ICurrencyConverterDataProviderWithFixedExRates.FixedExchangeRate
		{
			get { return JD_EstimatedExchangeRate; }
		}

		string ICurrencyConverterDataProviderWithFixedExRates.FixedExchangeRateCurrencyCode
		{
			get { return JD_RX_NKOrderCurrency; }
		}

		#endregion

		#region OnExchangeRateHolderDeleted

		//This is so as Order is ExchangeRateHolder and when Order gets deleted, a whole LandedCostHeader is out of scope
		event EventHandler ILandedCostHeader.OnExchangeRateHolderDeleted
		{
			add { }
			remove { }
		}

		#endregion

		#region ICurrencyConverterDataProvider Members

		ZDateTime ICurrencyConverterDataProvider.DateOfValuation
		{
			get
			{
				var actual = GetMilestoneActualDate(Events.Departure);
				return (actual.IsValid ? actual : GetMilestoneEstimatedDate(Events.Departure)).ToZDateTime();
			}
		}

		int ICurrencyConverterDataProvider.MaximumDaysToFallback
		{
			get { return 7; }
		}

		ExchangeRateType ICurrencyConverterDataProvider.RateType
		{
			get { return ExchangeRateType.Buy; }
		}

		ZString ICurrencyConverterDataProvider.LocalCurrencyCodeOverride
		{
			get { return ZString.Empty; }
		}

		ZBool? ICurrencyConverterDataProvider.IsReciprocalOverride
		{
			get { return null; }
		}

		public GlbCompany Company
		{
			get
			{
				BusinessObject lcHeader = Factory.LoadTop1(ObjectFactory.GetType<LandedCosting.ILandedCostHeader>(), new LandedCostHeaderFilter(this));
				ZGuid lcCompanyPK = lcHeader != null ? (ZGuid)lcHeader[LandedCostHeaderSchema.LT_GC.Name] : ZGuid.Empty;
				return lcCompanyPK.IsValid ? Factory.Load<GlbCompany>(lcCompanyPK) : GlbCompany.CurrentCompany;
			}
		}

		#endregion

		#region IChargeApportionee Members

		[ChildEditable(true)]
		public IJobComInvApportionedChargeCollection<Customs.Common.JobComInvCharge> ApportionedCharges
		{
			get
			{
				if (apportionedCharges == null)
				{
					apportionedCharges = new JobComInvApportionedChargeCollection<JobComInvCharge>(this);
					RegisterEditableChildObject(apportionedCharges);
				}
				return apportionedCharges;
			}
		}
		IJobComInvApportionedChargeCollection<Customs.Common.JobComInvCharge> apportionedCharges;

		IJobComInvApportionedChargeCollection<Customs.Common.JobComInvCharge> IChargeApportionee.ApportionedCharges
		{
			get { return ApportionedCharges; }
		}

		bool IChargeApportionee.CanThisChargeBeApportionedBasedOnIncoterm(ApportionChargeKey apportionChargeKey)
		{
			return true;
		}

		ZDecimal IChargeApportionee.GetBaseValueToApportionOn(CurrencyConverter currencyConverterToUse, string distributeBy)
		{
			ZDecimal result = 0;
			switch (distributeBy)
			{
				case ChargeDistributeByList.Codes.Value:
					if (OrderCurrency != null)
					{
						result = currencyConverterToUse.ConvertExact(new Money(JD_Calc_TotalPrice, OrderCurrency), GlbCompany.CurrentCompany.LocalCurrency).Amount;
					}
					break;

				case ChargeDistributeByList.Codes.Volume:
					if (Core.Constants.Volume.ContainsCode(JD_UnitOfVolume))
					{
						result = Core.Constants.Volume.Convert(JD_ActualVolume, JD_UnitOfVolume, Core.Constants.Volume.CubicMetres);
					}
					break;

				case ChargeDistributeByList.Codes.Weight:
					if (Core.Constants.Weight.ContainsCode(JD_UnitOfWeight))
					{
						result = Core.Constants.Weight.Convert(JD_ActualWeight, JD_UnitOfWeight, Core.Constants.Weight.Kilograms);
					}
					break;
			}

			return result;
		}

		bool IChargeApportionee.IsValidToApportionTo
		{
			get { return true; }
		}

		void IChargeApportionee.CalculateAmountBasedOnPercentage(Customs.Common.JobComInvCharge charge)
		{
		}

		#endregion

		#region IRatingSupporter

		public RatingAdaptersProvider AdaptersProvider
		{
			get
			{
				return new OrderRatingAdaptersProvider(this);
			}
		}

		#endregion

		#region ILandedCostHeader Members

		ZBool ILandedCostHeader.SupportsNoCostApportionmentItem => false;

		IEnumerable<ILandedCostDistributeTo> ILandedCostHeader.CandidatesToDistributeCostTo
		{
			get
			{
				yield return this;

				foreach (OrderLine line in NonCancelledOrderLines)
				{
					yield return line;
				}
			}
		}

		IEnumerable<ILandedCostChargeHolder> ILandedCostHeader.ChargeHolders
		{
			get { yield return this; }
		}

		OrgHeader ILandedCostHeader.Consignee
		{
			get { return Buyer; }
		}

		ZString ILandedCostHeader.JobNumber
		{
			get { return JD_OrderNumber; }
		}

		ZGuid ILandedCostHeader.CompanyPK
		{
			get { return Company.PK; }
		}

		ZDate ILandedCostHeader.DateOfEntry
		{
			get
			{
				var actualDate = GetMilestoneActualDate(Events.Arrival);
				return (ZDate)(actualDate.IsValid ? actualDate : GetMilestoneEstimatedDate(Events.Arrival)).ToZDateTime();
			}
		}

		void ILandedCostHeader.DoStuffBeforeRunningLCDistribution()
		{
			ApportionCharges();
			DutyFeeCalculationManager.Calculate();
		}

		internal LCDutyFeeCalculationManager DutyFeeCalculationManager
		{
			get { return dutyFeeCalculationManager ?? (dutyFeeCalculationManager = new LCDutyFeeCalculationManager(this)); }
		}
		LCDutyFeeCalculationManager dutyFeeCalculationManager;

		IEnumerable<ILandedCostExchangeRateHolder> ILandedCostHeader.ExchangeRateHolders
		{
			get { yield return this; }
		}

		BusinessObjectFactory ILandedCostHeader.Factory
		{
			get { return Factory; }
		}

		bool ILandedCostHeader.HasMultiInvoices
		{
			get
			{
				ZString uniqueInvoiceNumber = ZString.Empty;

				foreach (OrderLine orderLine in NonCancelledOrderLines)
				{
					if (!orderLine.JO_CommercialInvoiceNo.IsEmpty)
					{
						if (uniqueInvoiceNumber.IsEmpty)
						{
							uniqueInvoiceNumber = orderLine.JO_CommercialInvoiceNo;
						}
						else if (uniqueInvoiceNumber != orderLine.JO_CommercialInvoiceNo)
						{
							return true;
						}
					}
				}

				return false;
			}
		}

		bool ILandedCostHeader.IsAir
		{
			get { return IsAirTransport; }
		}

		bool ILandedCostHeader.IsJobInLCRunnableState
		{
			get { return true; }
		}

		bool ILandedCostHeader.IsLCSupported
		{
			get { return this.IsImport(); }
		}

		public ZString CountryOfImport
		{
			get
			{
				return Shipment != null ? Shipment.JS_RL_NKDestination.Left(2) : JD_RL_NKGoodsDeliveredTo.Left(2);
			}
		}

		ZString ILandedCostHeader.LandedCostType
		{
			get { return LandedCostType.Estimated; }
		}

		IComparer ILandedCostHeader.LineComparer
		{
			get { return new OrderLineComparer(); }
		}

		string ILandedCostHeader.MessageShownWhenLCIsNotSupported
		{
			get { return Res.GetString("34c40f7a-8fe7-4367-8d77-add2c51d061b", "Landed Costing is supported only for AU Import Orders. Please check if you have entered a local buyer."); }
		}

		ZString ILandedCostHeader.TableCode
		{
			get { return JobOrderHeaderSchema.Constants.Prefix; }
		}

		DutyTaxEntryFee ILandedCostHeader.TotalDutyTaxEntryFeeItems
		{
			get { return DutyFeeCalculationManager.GetTotalDutyTaxEntryFee(this); }
		}

		IEnumerable<IUltimateDistributee> ILandedCostHeader.UltimateDistributees
		{
			get { return new TypedEnumerable<IUltimateDistributee>(NonCancelledOrderLines); }
		}

		ZString ILandedCostHeader.UniqueReferenceNumber
		{
			get { return JD_OrderNumberAndSplit; }
		}

		event EventHandler ILandedCostHeader.OnLCSupportedChanged
		{
			add
			{
				JD_RL_NKPortOfLoadingInfo.ValueChanged += value;
				JD_RL_NKPortOfDischargeInfo.ValueChanged += value;
				JD_RL_NKGoodsDeliveredToInfo.ValueChanged += value;
				JD_JSInfo.ValueChanged += value;
			}
			remove
			{
				JD_RL_NKPortOfLoadingInfo.ValueChanged -= value;
				JD_RL_NKPortOfDischargeInfo.ValueChanged -= value;
				JD_RL_NKGoodsDeliveredToInfo.ValueChanged -= value;
				JD_JSInfo.ValueChanged -= value;
			}
		}

		ZDecimal ILandedCostHeader.DefaultEstimatedDutyPercent
		{
			get
			{
				ZDecimal result = 0m;

				if (Buyer != null && Supplier != null)
				{
					foreach (OrgSupplierBuyerLink link in Buyer.SupplierLinks)
					{
						if (link.OL_OH_Supplier == SupplierPK)
						{
							result = link.OL_DefaultDutyRate;
							break;
						}
					}
				}

				return result;
			}
		}

		Dictionary<ZString, ZDecimal> ILandedCostHeader.GetDefaultExchangeRates()
		{
			Dictionary<ZString, ZDecimal> result = new Dictionary<ZString, ZDecimal>();

			foreach (JobComInvCharge charge in Charges)
			{
				if (charge.Currency != null && charge.J7_ExchangeRate > 0m)
				{
					if (!result.ContainsKey(charge.J7_RX_NKCurrency))
					{
						result.Add(charge.J7_RX_NKCurrency, charge.J7_ExchangeRate);
					}
				}
			}

			return result;
		}

		IHaveRequiredDocuments ILandedCostHeader.RequiredDocumentsProvider
		{
			get { return this; }
		}

		Type ILandedCostHeader.DocsAndCartageType
		{
			get { return typeof(JobDocsAndCartage); }
		}

		Type ILandedCostHeader.DocsAndCartageParentType
		{
			get { return this.GetType(); }
		}

		#endregion

		#region ILandedCostExchangeRateHolder Members

		ZString ILandedCostExchangeRateHolder.CurrencyCode
		{
			get { return JD_RX_NKOrderCurrency; }
		}

		ZDecimal ILandedCostExchangeRateHolder.LandedCostExchangeRate
		{
			get { return JD_EstimatedExchangeRate; }
			set { JD_EstimatedExchangeRate = value; }
		}

		ZDecimal ILandedCostExchangeRateHolder.LandedCostExchangeRateDefault
		{
			get { return JD_EstimatedExchangeRate; }
		}

		ZString ILandedCostExchangeRateHolder.ReferenceNumber
		{
			get { return JD_OrderNumberAndSplit; }
		}

		ZGuid ILandedCostExchangeRateHolder.CompanyPK
		{
			get { return Company.PK; }
		}

		#endregion

		#region ILandedCostChargeHolder Members

		IEnumerable<IDefaultLandedCostInput> ILandedCostChargeHolder.ChargesToImportForLandedCosting
		{
			get
			{
				foreach (IDefaultLandedCostInput charge in Charges)
				{
					if (charge.IsValidToImport)
					{
						yield return charge;
					}
				}
			}
		}

		#endregion

		#region ILandedCostHeaderProvider Members

		ILandedCostHeader ILandedCostHeaderProvider.LCHeaderHost
		{
			get { return this; }
		}

		#endregion

		#region ILandedCostDistributeTo Members

		ZString ILandedCostDistributeTo.Description
		{
			get { return Res.GetString("0d1d3b48-73f3-4833-aa41-835599f423c1", "Order"); }
		}

		ZString ILandedCostDistributeTo.TableCode
		{
			get { return JobOrderHeaderSchema.Constants.Prefix; }
		}

		IEnumerable<IUltimateDistributee> ILandedCostDistributeTo.UltimateDistributees
		{
			get { return ((ILandedCostHeader)this).UltimateDistributees; }
		}

		ZString ILandedCostDistributeTo.UniqueCode
		{
			get { return JD_OrderNumberAndSplit; }
		}

		#endregion

		#region IAttachedOrder Members

		public const string AttachedOrderType = "PUR";

		ZString IAttachedOrder.GoodsDescription
		{
			get { return JD_OrderGoodsDescription; }
		}

		ZDateTime IAttachedOrder.JobDate
		{
			get { return JD_OrderDate; }
		}

		ZString IAttachedOrder.JobType
		{
			get { return AttachedOrderType; }
		}

		ModuleIdentifier IAttachedOrder.ModuleID
		{
			get { return ModuleIDs.Orders; }
		}

		ControllerID IAttachedOrder.ControllerID
		{
			get { return ControllerIDs.Orders; }
		}

		ZString IAttachedOrder.JobDescription
		{
			get { return Res.GetString("dadb025d-84d2-4555-a88c-50b4262d3f94", "Order Manager"); }
		}

		ZString IAttachedOrder.JobNo
		{
			get { return JD_OrderNumberAndSplit; }
		}

		ZString IAttachedOrder.JobStatus
		{
			get { return JD_OrderStatus; }
		}

		bool IAttachedOrder.ShouldSkipAllValidations
		{
			get { return shouldSkipAllValidations && !Env.Security.OrderManager.IsAllowed; }
			set { shouldSkipAllValidations = value; }
		}
		bool shouldSkipAllValidations;

		ZString IAttachedOrder.TransportMode => JD_TransportMode;

		ZDateTime IAttachedOrder.ETD
		{
			get { return GetMilestoneEstimatedDate(Events.Departure); }
		}

		ZDateTime IAttachedOrder.ETA
		{
			get { return GetMilestoneEstimatedDate(Events.Arrival); }
		}

		ZDateTime IAttachedOrder.RequiredExWorks
		{
			get { return JD_ExWorksRequiredBy; }
		}

		ZDateTime IAttachedOrder.RequiredInStore
		{
			get { return JD_DeliveryRequiredBy; }
		}

		ZDecimal IAttachedOrder.TotalWeight
		{
			get { return JD_Calc_TotalWeight; }
		}

		ZString IAttachedOrder.WeightUnit
		{
			get { return JD_Calc_TotalWeightUnit; }
		}

		ZDecimal IAttachedOrder.TotalVolume
		{
			get { return JD_Calc_TotalVolume; }
		}

		ZString IAttachedOrder.VolumeUnit
		{
			get { return JD_Calc_TotalVolumeUnit; }
		}

		ZDecimal IAttachedOrder.QuantityRemaining
		{
			get { return JD_Calc_TotalQuantityRemaining; }
		}

		ZDecimal IAttachedOrder.QuantityInvoiced
		{
			get { return JD_Calc_TotalQuantityInvoiced; }
		}

		ZDecimal IAttachedOrder.QuantityOrdered
		{
			get { return JD_Calc_TotalQuantity; }
		}

		ZDecimal IAttachedOrder.QuantityReceived
		{
			get { return JD_Calc_TotalQuantityReceived; }
		}

		ZString IAttachedOrder.BuyerOrgCode
		{
			get { return JD_Calc_BuyerCode; }
		}

		ZString IAttachedOrder.SupplierOrgCode
		{
			get { return JD_Calc_SupplierCode; }
		}

		ZInt IAttachedOrder.TotalPacks
		{
			get { return JD_Calc_OuterPacks; }
		}

		ZString IAttachedOrder.PacksType
		{
			get { return JD_Calc_OuterPacksType; }
		}

		#endregion

		#region IControllerIDProvider Members

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get { return PK.ToGuid(); }
		}

		ControllerID IControllerIDProvider.ControllerID
		{
			get { return ControllerIDs.Orders; }
		}

		#endregion

		#region IRelatedJob Members

		ZString IRelatedJob.JobDescription
		{
			get { return Res.GetString("53cae0e2-edb0-4c24-8004-a4d336ed0aaf", "Forwarding Order"); }
		}

		ZString IRelatedJob.JobNumber
		{
			get { return JD_OrderNumberAndSplit; }
		}

		ZString IRelatedJob.JobStatus
		{
			get { return JD_OrderStatus; }
		}

		#endregion

		#region IModuleToModule Members

		void IModuleToModule.AddToRelatedJobs(BusinessObject loadedJob)
		{
		}

		bool IModuleToModule.CanExportData(out ZString errorMessage)
		{
			errorMessage = "";

			if (Buyer == null)
			{
				errorMessage = Res.GetString("60ad7a01-54c6-440d-8759-fdfd8e558d77", "No Buyer is specified");
			}
			else if (!Buyer.OH_IsWarehouseClient)
			{
				errorMessage = Res.GetString("c4331068-2226-425b-8ea7-00b118d9950c", "The Buyer is not flagged as a Warehouse Client.");
			}
			else if (Warehouse == null)
			{
				errorMessage = Res.GetString("14f3e568-3e0c-42d9-8530-9fd352fa7d36", "No Warehouse is specified.");
			}

			return errorMessage.IsEmpty;
		}

		BusinessObject IModuleToModule.GetRelatedObject()
		{
			return RelatedWarehouseReceive;
		}

		IOrgHeader IModuleToModule.RecipientOrganisation
		{
			get { return Warehouse; }
		}

		#endregion

		#region IDefaultNumberOfDecimalsSupporter Members

		ZString IDefaultNumberOfDecimalsSupporter.TransportMode
		{
			get
			{
				switch (JD_TransportMode)
				{
					case Constants.TransportModes.AirSea:
						return Constants.TransportModes.Air;

					case Constants.TransportModes.SeaAir:
						return Constants.TransportModes.Sea;

					default:
						return JD_TransportMode;
				}
			}
		}

		public int GetDefaultNumberOfDecimals(PropertyDescriptor property)
		{
			return DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(this, property);
		}

		ZString IDefaultNumberOfDecimalsSupporter.GetUnitOfMeasure(PropertyDescriptor property)
		{
			var unitOfMeasure = ZString.Empty;
			var propertyName = DefaultNumberOfDecimals.GetBoundPropertyName(property.Name);

			switch (propertyName)
			{
				case Schema.JD_ActualWeight:
					unitOfMeasure = JD_UnitOfWeight;
					break;

				case Schema.JD_ActualVolume:
					unitOfMeasure = JD_UnitOfVolume;
					break;

				case Schema.Weight:
					unitOfMeasure = WeightUnit;
					break;

				case Schema.Volume:
					unitOfMeasure = VolumeUnit;
					break;

				case Schema.JD_Calc_TotalWeight:
					unitOfMeasure = JD_Calc_TotalWeightUnit;
					break;

				case Schema.JD_Calc_TotalVolume:
					unitOfMeasure = JD_Calc_TotalVolumeUnit;
					break;

				default:
					break;
			}

			return unitOfMeasure;
		}

		public ZDecimal GetRoundedValue(SchemaColumn column, PropertyDescriptor property, ZDecimal value)
		{
			return DefaultNumberOfDecimalsSupporterWithSchemaColumnHelperForFreight.GetRoundedValue(this, column, property, value);
		}

		public ZDecimal GetRoundedValue(PropertyDescriptor property, ZDecimal value)
		{
			return DefaultNumberOfDecimalsSupporterHelperForFreight.GetRoundedValue(this, property, value);
		}

		void IDefaultNumberOfDecimalsSupporter.RoundMeasurePropertiesOnTransportModeChanged()
		{
			this.SetRoundedValue(JobOrderHeaderSchema.JD_ActualWeight, JD_ActualWeightInfo);
			this.SetRoundedValue(JobOrderHeaderSchema.JD_ActualVolume, JD_ActualVolumeInfo);

			if (orderLines != null)
			{
				foreach (OrderLine orderLine in orderLines)
				{
					IDefaultNumberOfDecimalsSupporter decimalsSupporter = orderLine;
					decimalsSupporter.RoundMeasurePropertiesOnTransportModeChanged();

					foreach (var delivery in orderLine.Deliveries)
					{
						foreach (var deliveryContainer in delivery.Containers)
						{
							IDefaultNumberOfDecimalsSupporter containerDecimalsSupporter = deliveryContainer;
							containerDecimalsSupporter.RoundMeasurePropertiesOnTransportModeChanged();
						}
					}
				}
			}
		}

		#endregion

		#region IRelatableActivity Members

		ZBool IRelatableActivity.ShouldIgnoreSuperAndSubActivityRelationships
		{
			get { return false; }
		}

		ZString IRelatableActivity.ActivityType
		{
			get { return RelatableActivityTypeList.Codes.Orders; }
		}

		IOrgHeader IRelatableActivity.Client
		{
			get { return null; }
		}

		ZBool IRelatableActivity.ClientHasChanges
		{
			get { return false; }
		}

		IOrgContact IRelatableActivity.Contact
		{
			get { return null; }
		}

		ZBool IRelatableActivity.ContactHasChanges
		{
			get { return false; }
		}

		ZString IRelatableActivity.Summary
		{
			get { return string.Join("; ", new[] { JD_OrderGoodsDescription, JD_OrderStatus }.Where(x => !x.IsEmpty)); }
		}

		void IRelatableActivity.OnRelatedActivitySaving(IRelatableActivity relatedActivity)
		{
		}

		ZBool IRelatableActivity.SupportViewRelatedCommunications => ZBool.True;

		public IRelatedChildActivityPivotCollection RelatedChildActivityPivotCollection
		{
			get
			{
				if (relatedChildActivityPivotCollection == null)
				{
					relatedChildActivityPivotCollection = new RelatedChildActivityPivotCollection(this);
				}
				return relatedChildActivityPivotCollection;
			}
		}
		RelatedChildActivityPivotCollection relatedChildActivityPivotCollection;

		public IRelatedParentActivityPivotCollection RelatedParentActivityPivotCollection
		{
			get
			{
				if (relatedParentActivityPivotCollection == null)
				{
					relatedParentActivityPivotCollection = new RelatedParentActivityPivotCollection(this);
				}
				return relatedParentActivityPivotCollection;
			}
		}

		RelatedParentActivityPivotCollection relatedParentActivityPivotCollection;

		#endregion

		#region GoodsAvailableAtAddressRequirement

		public JobDocAddressRequirement GoodsAvailableAtAddressRequirement
		{
			get
			{
				if (fGoodsAvailableAtAddressRequirement == null)
				{
					fGoodsAvailableAtAddressRequirement = GetGoodsAvailableAtAddressRequirement();
				}
				return fGoodsAvailableAtAddressRequirement;
			}
		}
		JobDocAddressRequirement fGoodsAvailableAtAddressRequirement;

		JobDocAddressRequirement GetGoodsAvailableAtAddressRequirement()
		{
			return new JobDocAddressRequirement(DocAddressType.GoodsAvailableAt, AddressType.PIC, ContactType.LocalTransport);
		}

		#endregion

		#region GoodsAvailableAtAddress

		public JobDocAddress GoodsAvailableAtAddress
		{
			get
			{
				if (fGoodsAvailableAtAddress == null || fGoodsAvailableAtAddress.IsDeleted)
				{
					fGoodsAvailableAtAddress = GetNewGoodsAvailableAtAddress();
				}
				return fGoodsAvailableAtAddress;
			}
		}
		JobDocAddress fGoodsAvailableAtAddress;

		JobDocAddress GetNewGoodsAvailableAtAddress()
		{
			return DocAddresses.FindOrCreateWithRequirement(GoodsAvailableAtAddressRequirement);
		}

		[MaxLength(3)]
		public ZString GoodsAvailableAtFieldType
		{
			get
			{
				return GoodsAvailableAtAddress.E2_AddressOverride ?
					nameof(FieldType.Text) :
					nameof(FieldType.OrganisationGuid);
			}
		}

		void DefaultGoodsAvailableAtAddress()
		{
			var supplier = Supplier;
			if (supplier == null)
			{
				return;
			}

			if (!GoodsAvailableAtAddress.E2_AddressOverride)
			{
				GoodsAvailableAtAddress.E2_OA_Address = supplier.MainAddress.PK;
			}
		}

		#endregion

		#region GoodsDeliveredToAddressRequirement

		public JobDocAddressRequirement GoodsDeliveredToAddressRequirement
		{
			get
			{
				if (fGoodsDeliveredToAddressRequirement == null)
				{
					fGoodsDeliveredToAddressRequirement = GetGoodsDeliveredToAddressRequirement();
				}
				return fGoodsDeliveredToAddressRequirement;
			}
		}
		JobDocAddressRequirement fGoodsDeliveredToAddressRequirement;

		JobDocAddressRequirement GetGoodsDeliveredToAddressRequirement()
		{
			return new JobDocAddressRequirement(DocAddressType.GoodsDeliveredTo, AddressType.DLV, ContactType.LocalTransport);
		}

		#endregion

		#region GoodsDeliveredToAddress

		public JobDocAddress GoodsDeliveredToAddress
		{
			get
			{
				if (fGoodsDeliveredToAddress == null || fGoodsDeliveredToAddress.IsDeleted)
				{
					fGoodsDeliveredToAddress = GetNewGoodsDeliveredToAddress();
				}
				return fGoodsDeliveredToAddress;
			}
		}
		JobDocAddress fGoodsDeliveredToAddress;

		JobDocAddress GetNewGoodsDeliveredToAddress()
		{
			return DocAddresses.FindOrCreateWithRequirement(GoodsDeliveredToAddressRequirement);
		}

		[MaxLength(3)]
		public ZString GoodsDeliveredToFieldType
		{
			get
			{
				return GoodsDeliveredToAddress.E2_AddressOverride ?
					nameof(FieldType.Text) :
					nameof(FieldType.OrganisationGuid);
			}
		}

		void DefaultGoodsDeliveredToAddress()
		{
			var buyer = Buyer;
			if (buyer == null)
			{
				return;
			}

			if (!GoodsDeliveredToAddress.E2_AddressOverride)
			{
				GoodsDeliveredToAddress.E2_OA_Address = buyer.GetAddressWithFallback(AddressType.DLV).PK;
			}
		}

		#endregion

		#region IProcessHandlingInfoProvider

		ProcessHandlingInfo IProcessHandlingInfoProvider.ProcessHandlingInfo => new OrderProcessHandlingInfo(this);

		#endregion

		#region eConversation

		JobConversation IConversationProvider.eConversation
		{
			get
			{
				if (!IsInDatabase)
				{
					return null;
				}
				return conversation ?? (conversation = GetOrCreateConversation());
			}
		}
		JobConversation conversation;

		ModuleIdentifier IConversationProvider.ParentModule => ModuleIDs.Orders;

		ControllerID IConversationProvider.ParentController => ControllerIDs.Orders;

		IEnumerable<EConversation.Business.RelatedParty> IConversationProvider.AdditionalParticipants => Enumerable.Empty<EConversation.Business.RelatedParty>();

		bool IConversationProvider.SendEmailNotificationsOnSave => true;

		string IConversationProvider.EmailSubjectContentOverride => default;

		string IConversationProvider.FromAddressOverride => default;

		NotificationEmailTemplate IConversationProvider.NotificationEmailTemplateOverride => default;

		JobConversation GetOrCreateConversation()
		{
			var result = JobConversation.GetOrCreate(this);
			RegisterEditableChildObject(result);
			return result;
		}

		void IConversationProvider.RunConversationUpdateActionBeforeSaving()
		{
		}

		#endregion

		#region Order Line Tolerance

		void SetDefaultTolerancesForAllOrderLines()
		{
			foreach (var orderLine in OrderLines)
			{
				orderLine.SetDefaultTolerances();
			}
		}

		#endregion

		public bool CanAttachOrderToShipmentWithNonMatchingControllingCustomer(CommonShipment shipment, out string errorMessage)
		{
			errorMessage = string.Empty;

			if ((ControllingCustomerDocAddress?.E2_AddressOverride ?? false)
				|| (shipment.ControllingCustomerAddress?.E2_AddressOverride ?? false)
				|| (ControllingCustomerDocAddress?.E2_OA_Address ?? ZGuid.Empty) != (shipment.ControllingCustomerAddress?.E2_OA_Address ?? ZGuid.Empty))
			{
				if (shipment.JS_IsForwardRegistered && !Environment.Env.Security.AllowAttachOrdersWithNonMatchingControllingCustomerForShipment.IsAllowed)
				{
					errorMessage = Res.GetString("282deb50-3a33-4704-87b3-2906dd7cdf44", "You do not have security rights to allow order and its shipment have non-matching Controlling Customers.");
					return false;
				}
				else if (shipment.JS_IsBooking && !Environment.Env.Security.AllowAttachOrdersWithNonMatchingControllingCustomerForBooking.IsAllowed)
				{
					errorMessage = Res.GetString("b5fef9da-801c-435f-ba65-4dcb010d4a69", "You do not have security rights to allow order and its booking have non-matching Controlling Customers.");
					return false;
				}
			}

			return true;
		}

		public void SetOrderToDeliveredIfOrderLinesDelivered()
		{
			if (OrderLines.Any() && OrderLines.All(line => line.JO_LineStatus == Constants.OrderStatus.Delivered || line.JO_LineStatus == Constants.OrderStatus.Cancelled))
			{
				JD_OrderStatus = Constants.OrderStatus.Delivered;
			}
		}

		public override string CanReactivate()
		{
			if (!BuyerAndOrderNumAndSplitIsUniqueValidation.CheckBuyerAndOrderNumAndSplitIsUnique(this))
			{
				return Res.GetString("5ea937d2-6b49-4c11-bd5f-a2f6ed081974", "An active order already exists for the buyer with this order number and the order cannot be reactivated.");
			}

			return base.CanReactivate();
		}

		#region USXML Validations

		ZDBOnlyQuery GetLinkedSupplierBookingQuery()
		{
			var query = new ZDBOnlyQuery(typeof(JobSupplierBooking));
			var supplierBookingLineQuery = new ZDBOnlySubQuery(typeof(JobSupplierBookingLine), JobSupplierBookingSchema.PK, JobSupplierBookingLineSchema.JSL_JSB_Booking);

			var orderLineQuery = new ZDBOnlySubQuery(typeof(OrderLine), JobSupplierBookingLineSchema.JSL_JO_OrderLine);
			orderLineQuery.AddToFilter(JobOrderLineSchema.JO_JD, SQLComparisonOperator.Equal, PK);

			supplierBookingLineQuery.AddSubQuery(orderLineQuery, JoinCondition.And);
			query.AddSubQuery(supplierBookingLineQuery, JoinCondition.And);
			return query;
		}

		IEnumerable<JobSupplierBooking> linkedSupplierBookings;
		public IEnumerable<JobSupplierBooking> LinkedSupplierBookings => linkedSupplierBookings ?? (linkedSupplierBookings = Array.AsReadOnly(Factory.Load<JobSupplierBooking>(GetLinkedSupplierBookingQuery())));

		#endregion

		#region IExternalRequestGenerationProvider

		public ZString GetRequestJobID() => JD_OrderNumber;

		public ZString GetRequestTypeCode() => ExternalRequestTypes.Codes.Order;

		public (ZGuid OrginzationPK, ZGuid ContactPK) GetRequestSupportedAddressInfo(ZString addressType) => addressType.ToString() switch
		{
			DocAddressTypes.Codes.BuyerDocumentaryAddress => (BuyerPK, JD_OC_BuyerContact),
			DocAddressTypes.Codes.SupplierDocumentaryAddress => (SupplierPK, JD_OC_SupplierContact),
			DocAddressTypes.Codes.ControllingCustomer => (ControllingCustomerDocAddress.OrganisationPK, ControllingCustomerDocAddress.ContactPK),
			DocAddressTypes.Codes.Manufacturer => RequestGenerationProviderHelper.GetRequestSupportedAddressInfo(DocAddresses.FindByDocAddressType(DocAddressType.Manufacturer)),
			_ => (ZGuid.Empty, ZGuid.Empty),
		};

		#endregion
	}

	#region Planning Voyage State Enum

	public enum PlanningVoyageState
	{
		OneVoyage,
		TwoVoyage,
		ThreeVoyage
	}

	#endregion

	#region Create Order Type Enum

	public enum CreateOrderType
	{
		Split,
		New
	}

	#endregion

	#region Order Split Dialog Result Enum

	public enum OrderSplitDialogResult
	{
		SplitOrder,
		CreateNewOrder,
		DoNothing
	}

	#endregion
}
