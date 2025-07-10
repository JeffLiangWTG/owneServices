using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class OrderToBulkUpdate : NonPersistentBusinessObject, IObsoleteValidation
	{
		public OrderToBulkUpdate(BusinessObjectFactory factory, OrderDetailsBulkUpdateBusinessObject parent)
			: base(factory)
		{
			this.Parent = parent;
		}

		public readonly OrderDetailsBulkUpdateBusinessObject Parent;

		#region New Bound Properties

		#region OrderNumber

		[List("AllOrders")]
		[MaxLength(Order.Schema.JD_OrderNumberMaxLength)]
		public ZString OrderNumber
		{
			get { return fOrderNumber; }
			set
			{
				CheckMaximumLength(OrderNumberInfo, value);
				fOrderNumber = value;
				OrderNumberInfo.RefreshBinding();

				PopulateBuyerFromOrderNumberIf1Match();
				if (!IsValidationSuspended)
				{
					ValidateOrderNumber();
				}
			}
		}

		ZString fOrderNumber;

		public ZPropertyInfo OrderNumberInfo
		{
			get { return GetZPropertyInfo(nameof(OrderNumber)); }
		}

		#endregion

		#region OrderNumberSplit

		public ZByte OrderNumberSplit
		{
			get { return fOrderNumberSplit; }
			set
			{
				fOrderNumberSplit = value;
				if (!IsValidationSuspended)
				{
					ValidateOrderNumberSplit();
				}
				OrderNumberSplitInfo.RefreshBinding();

				PopulateBuyerFromOrderNumberIf1Match();
			}
		}

		public ZPropertyInfo OrderNumberSplitInfo
		{
			get { return GetZPropertyInfo(nameof(OrderNumberSplit)); }
		}

		ZByte fOrderNumberSplit;

		#endregion

		#region BuyerFK

		[List("Buyers")]
		public ZGuid BuyerFK
		{
			get { return fBuyerFK; }
			set
			{
				fBuyerFK = value;
				if (!IsValidationSuspended)
				{
					ValidateBuyerFK();
					ValidateOrderNumber();
				}
				BuyerFKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo BuyerFKInfo
		{
			get { return GetZPropertyInfo(nameof(BuyerFK)); }
		}

		ZGuid fBuyerFK;

		#endregion

		#endregion

		#region Validation

		public void ValidateOrderNumber()
		{
			OrderNumberInfo.ClearAllNotifications();
			ValidateOrderNumberAndSplitAndBuyerExists(OrderNumberInfo);
			ValidateOrderNumberSplit();
			ValidateOrderUnique();

			if (Order != null && Order.JD_TransportMode != Parent.JD_TransportMode)
			{
				OrderNumberInfo.AddError(Res.GetString("55c17c81-b6b6-42b8-ab7c-bf15d8ed2a0e", "You can only select orders with transport mode '{0}'", Parent.JD_TransportMode));
			}
		}

		public void ValidateOrderNumberSplit()
		{
			OrderNumberSplitInfo.ClearAllNotifications();
			ValidateOrderNumberAndSplitAndBuyerExists(OrderNumberSplitInfo);

			if (OrderNumberSplit == 0)
			{
				var buyer = Factory.Load<OrgHeader>(BuyerFK);
				var filter = new ZQuery();
				filter.AddToFilter(JoinCondition.And, JobOrderHeaderSchema.JD_OrderNumber, SQLComparisonOperator.Equal, OrderNumber);
				filter.AddToFilter(JoinCondition.And, JobOrderHeaderSchema.JD_OA_BuyerAddress, SQLComparisonOperator.Equal, buyer == null ? ZGuid.Empty : buyer.Addresses.Select(x => x.PK));

				var allSplits = Factory.Load<Order>(filter);
				if (allSplits.Length > 1)
				{
					OrderNumberSplitInfo.AddWarning(Res.GetString("1716cfbc-b19b-48fa-8eae-36be3d9a77df", "This order is split, only the first order will be updated"));
				}
			}
		}

		public void ValidateBuyerFK()
		{
			BuyerFKInfo.ClearAllNotifications();
			ValidateOrderNumberAndSplitAndBuyerExists(BuyerFKInfo);
		}

		void ValidateOrderUnique()
		{
			int count = 0;
			foreach (OrderToBulkUpdate next in Parent.SelectedOrders)
			{
				if (next.Order != null && this.Order != null &&
					next.Order.PK == this.Order.PK)
				{
					count++;
				}
			}
			if (count > 1)
			{
				OrderNumberInfo.AddError(Res.GetString("16a862ca-9225-44b4-910e-828ed6721b23", "You can't specify the same order more than once"));
			}
		}

		void ValidateOrderNumberAndSplitAndBuyerExists(ZPropertyInfo infoWithErrors)
		{
			if (Order == null)
			{
				string buyerFullName = Res.GetString("8874b9ca-8094-4b44-808b-18e79a2b0251", "unspecified buyer");
				if (Buyer != null)
				{
					buyerFullName = Res.GetString("6bd30945-9e80-4a6c-b3f5-4f12303b2912", "buyer {0}", Buyer.OH_FullNameTruncated);
				}
				infoWithErrors.AddError(Res.GetString("a7f44944-8235-4dfa-803a-3a9096ae2905", "Could not find order with order number {0}:{1} and {2}", OrderNumber, OrderNumberSplit, buyerFullName));
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateOrderNumber();
			ValidateOrderNumberSplit();
			ValidateBuyerFK();
		}

		#endregion

		#region List Properties

		public OrderCollection AllOrders
		{
			get
			{
				if (fAllOrders == null)
				{
					fAllOrders = new OrderCollection(Factory);
				}
				return fAllOrders;
			}
		}

		OrderCollection fAllOrders;

		public ConsigneeCollection Buyers
		{
			get
			{
				if (fBuyers == null)
				{
					fBuyers = new ConsigneeCollection(Factory);
				}
				return fBuyers;
			}
		}

		ConsigneeCollection fBuyers;

		#endregion

		#region Related Business Objects

		public Order OrderForBinding
			=> this.Order
				?? new BusinessObjectFactory().New<Order>();

		public Order Order
		{
			get
			{
				Order result = null;
				var filter = new ZQuery();
				if (OrderNumber.IsEmpty || !BuyerFK.IsValid)
				{
					filter = ZQuery.NoResultQuery;
				}
				else
				{
					var buyer = Factory.Load<OrgHeader>(BuyerFK);

					filter = new ZQuery();
					filter.AddToFilter(JoinCondition.And, JobOrderHeaderSchema.JD_OrderNumber, SQLComparisonOperator.Equal, OrderNumber);
					filter.AddToFilter(JoinCondition.And, JobOrderHeaderSchema.JD_OrderNumberSplit, SQLComparisonOperator.Equal, OrderNumberSplit);
					filter.AddToFilter(JoinCondition.And, JobOrderHeaderSchema.JD_OA_BuyerAddress, SQLComparisonOperator.Equal, buyer == null ? ZGuid.Empty : buyer.Addresses.Select(x => x.PK));
				}

				var orders = Factory.Load<Order>(filter);
				if (orders.Length == 1)
				{
					result = orders[0];
				}

				if (result != null)
				{
					result.SetReadOnlyIncludingChildren(true);
				}

				return result;
			}
		}

		public OrgHeader Buyer
		{
			get { return Factory.Load<OrgHeader>(BuyerFK); }
		}

		#endregion

		public void SetOrder(Order order)
		{
			OrderNumber = order.JD_OrderNumber;
			OrderNumberSplit = order.JD_OrderNumberSplit;
			BuyerFK = order.BuyerPK;
		}

		public void UpdateFrom(OrderDetailsBulkUpdateBusinessObject orderToUpdateFrom)
		{
			if (Order != null)
			{
				((IBusinessObjectInternals)this).IsCopying = true;
				try
				{
					foreach (string propertyName in PropertiesToUpdate)
					{
						IZType value = (IZType)orderToUpdateFrom[propertyName];
						if (value != null && value.IsValid && !value.IsEmpty)
						{
							Order[propertyName] = value;
						}
					}

					UpdateMilestones(orderToUpdateFrom);
					UpdateNotes(orderToUpdateFrom);
					UpdateAuditDetails();
				}
				finally
				{
					((IBusinessObjectInternals)this).IsCopying = false;
				}
			}
		}

		void UpdateMilestones(OrderDetailsBulkUpdateBusinessObject orderToUpdateFrom)
		{
			foreach (PropertyToMilestone propertyToMilestone in PropertiesToMilestones)
			{
				IZType value = (IZType)orderToUpdateFrom[propertyToMilestone.EstimatedProperty];
				if (value != null && value.IsValid && !value.IsEmpty && value is ZDateTime date)
				{
					Order.UpdateEventEstimate(propertyToMilestone.EventType, date.ToOffset());
				}

				value = (IZType)orderToUpdateFrom[propertyToMilestone.ActualProperty];
				if (value != null && value.IsValid && !value.IsEmpty && value is ZDateTime date2)
				{
					Order.UpdateEvent(propertyToMilestone.EventType, date2.ToOffset());
				}
			}
		}

		void UpdateNotes(OrderDetailsBulkUpdateBusinessObject orderToUpdateFrom)
		{
			foreach (StmNote noteToUpdateFrom in orderToUpdateFrom.GetNotes().GetAllNotes())
			{
				if (noteToUpdateFrom.ST_Description != PredefinedNoteTypes.Instance.OrderUpdateHistory.Description)
				{
					PredefinedNoteType noteType = PredefinedNoteTypes.Instance.NoteTypeByDescription(noteToUpdateFrom.ST_Description);
					if (noteType != null && noteType.IsOnlyOneAllowed)
					{
						foreach (StmNote stmNoteToOverride in Order.Notes.FindByDescription(noteToUpdateFrom.ST_Description))
						{
							stmNoteToOverride.Delete();
						}
					}
					Order.Notes.Add(noteToUpdateFrom.Clone());
				}
			}
		}

		void UpdateAuditDetails()
		{
			Order.JD_SystemLastEditTimeUtc = ZDateTime.UtcNow;
			Order.JD_SystemLastEditUser = StaticCurrentFetcher.Instance.CurrentUserCode;
		}

		#region Implementation 

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "Property is a IReadOnlyList")]
		static readonly IReadOnlyList<string> propertiesToUpdate = new string[]
		{
			Order.Schema.JD_OrderStatus,
			Order.Schema.JD_EstimateUserDate1,
			Order.Schema.JD_ActualUserDate1,
			Order.Schema.JD_EstimateUserDate2,
			Order.Schema.JD_ActualUserDate2,
			Order.Schema.JD_EstimateUserDate3,
			Order.Schema.JD_ActualUserDate3,
			Order.Schema.JD_EstimateUserDate4,
			Order.Schema.JD_ActualUserDate4,
			Order.Schema.JD_RV_NKDepartureVessel,
			Order.Schema.JD_DepartureVoyage,
			Order.Schema.JD_E_ARV_1stIntermediate,
			Order.Schema.JD_RV_NKIntermediateVessel,
			Order.Schema.JD_IntermediateVoyage,
			Order.Schema.JD_E_DEP_2,
			Order.Schema.JD_E_ARV_2ndIntermediate,
			Order.Schema.JD_RV_NKArrivalVessel,
			Order.Schema.JD_ArrivalVoyage,
			Order.Schema.JD_E_DEP_3,
			Order.Schema.JD_Milestone_E_DEP,
			Order.Schema.JD_Milestone_E_ARV
		};

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "Property is a IReadOnlyList")]
		[SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
		public static IReadOnlyList<string> PropertiesToUpdate = propertiesToUpdate;

		internal class PropertyToMilestone
		{
			public PropertyToMilestone(Event eventType, string estimatedProperty, string actualProperty)
			{
				EventType = eventType;
				EstimatedProperty = estimatedProperty;
				ActualProperty = actualProperty;
			}

			public Event EventType { get; set; }
			public string EstimatedProperty { get; set; }
			public string ActualProperty { get; set; }
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		internal static IReadOnlyList<PropertyToMilestone> PropertiesToMilestones = new PropertyToMilestone[]
		{
			new PropertyToMilestone(Events.ExWorks, OrderDetailsBulkUpdateBusinessObject.Schema.JD_E_EXW, OrderDetailsBulkUpdateBusinessObject.Schema.JD_A_EXW),
			new PropertyToMilestone(Events.DeliveryCartageCompleteFinalised, OrderDetailsBulkUpdateBusinessObject.Schema.JD_E_IST, OrderDetailsBulkUpdateBusinessObject.Schema.JD_A_IST),
			new PropertyToMilestone(Events.GateIn, OrderDetailsBulkUpdateBusinessObject.Schema.JD_E_RCV, OrderDetailsBulkUpdateBusinessObject.Schema.JD_A_RCV),
			new PropertyToMilestone(Events.Departure, OrderDetailsBulkUpdateBusinessObject.Schema.JD_E_DEP, OrderDetailsBulkUpdateBusinessObject.Schema.JD_A_DEP),
			new PropertyToMilestone(Events.Arrival, OrderDetailsBulkUpdateBusinessObject.Schema.JD_E_ARV, OrderDetailsBulkUpdateBusinessObject.Schema.JD_A_ARV),
			new PropertyToMilestone(Events.CustomsCommenced, OrderDetailsBulkUpdateBusinessObject.Schema.JD_E_CCC, OrderDetailsBulkUpdateBusinessObject.Schema.JD_A_CCC),
			new PropertyToMilestone(Events.CustomsCleared, OrderDetailsBulkUpdateBusinessObject.Schema.JD_E_CLR, OrderDetailsBulkUpdateBusinessObject.Schema.JD_A_CLR),
			new PropertyToMilestone(Events.CargoAvailable, OrderDetailsBulkUpdateBusinessObject.Schema.JD_E_UNP, OrderDetailsBulkUpdateBusinessObject.Schema.JD_A_UNP),
			new PropertyToMilestone(Events.DeliveryCartageAdvised, OrderDetailsBulkUpdateBusinessObject.Schema.JD_E_PUP, OrderDetailsBulkUpdateBusinessObject.Schema.JD_A_PUP),
		};

		void PopulateBuyerFromOrderNumberIf1Match()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(JoinCondition.And, JobOrderHeaderSchema.JD_OrderNumber, SQLComparisonOperator.Equal, OrderNumber);
			filter.AddToFilter(JoinCondition.And, JobOrderHeaderSchema.JD_OrderNumberSplit, SQLComparisonOperator.Equal, OrderNumberSplit);

			Order[] orders = (Order[])Factory.Load(typeof(Order), filter);
			Hashtable uniqueBuyers = new Hashtable();
			foreach (Order next in orders)
			{
				uniqueBuyers[next.BuyerPK] = null;
			}
			if (uniqueBuyers.Keys.Count == 1)
			{
				BuyerFK = orders[0].BuyerPK;
			}
		}

		#endregion
	}
}
