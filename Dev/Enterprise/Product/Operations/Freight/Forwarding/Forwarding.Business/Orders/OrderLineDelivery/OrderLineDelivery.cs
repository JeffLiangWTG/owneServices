using System;
using System.ComponentModel;
using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	[DependentBusinessObject(typeof(OrderLine), "Deliveries")]
	public class OrderLineDelivery : AutoJobOrderLineDelivery, Integration.Forwarding.IOrderLineDelivery, ISupportDataImporting
	{
		public OrderLineDelivery(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Delete

		public override void Delete()
		{
			this.Containers.DeleteAll();
			base.Delete();
		}

		#endregion

		#region Clone

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			OrderLineDelivery result = (OrderLineDelivery)base.CloneInternal(args);
			foreach (OrderLineDeliverContainer container in Containers.ToArray())
			{
				result.Containers.Add((OrderLineDeliverContainer)container.Clone());
			}
			return result;
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region Test Data
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			OrgAddress deliverPoint = Factory.New<OrgAddress>();
			deliverPoint.FillWithValidTestData(kind, propertyPath);
			J4_OA_NKDeliveryPoint = deliverPoint.OA_Code;
		}

#endif
		#endregion

		#region Related Business Objects

		public Order Order
		{
			get { return OrderLine == null ? null : OrderLine.Order; }
		}

		[ChildEditable(true)]
		public OrderLineDeliverContainerCollection Containers
		{
			get
			{
				if (fContainers == null)
				{
					fContainers = NewContainerCollection();
					((IBusinessObjectCollection)fContainers).ListChanged += delegate(object sender, ListChangedEventArgs e)
					{
						if (!IsValidationSuspended && !IsDeleted)
						{
							Validation.ValidateJ4_Allocated();
						}
					};

					RegisterEditableChildObject(fContainers);
				}
				return fContainers;
			}
		}
		OrderLineDeliverContainerCollection fContainers;

		protected virtual OrderLineDeliverContainerCollection NewContainerCollection()
		{
			return new OrderLineDeliverContainerCollection(this);
		}

		public OrderLine OrderLine
		{
			get { return Factory.Load<OrderLine>(J4_JO); }
		}

		#endregion

		#region New Properties

		#region J4_DeliverPointAddress1

		public ZPropertyInfo J4_DeliverPointAddress1Info
		{
			get { return GetZPropertyInfo(nameof(J4_DeliverPointAddress1)); }
		}

		public ZString J4_DeliverPointAddress1
		{
			get { return (DeliveryPoint == null) ? ZString.Empty : DeliveryPoint.OA_Address1; }
		}

		#endregion

		#region J4_DeliverPointAddress2

		public ZPropertyInfo J4_DeliverPointAddress2Info
		{
			get { return GetZPropertyInfo(nameof(J4_DeliverPointAddress2)); }
		}

		public ZString J4_DeliverPointAddress2
		{
			get { return (DeliveryPoint == null) ? ZString.Empty : DeliveryPoint.OA_Address2; }
		}

		#endregion

		#region J4_Calc_TotalQuantityAllocated

		public ZDecimal J4_Calc_TotalQuantityAllocated
		{
			get
			{
				ZDecimal result = 0;
				foreach (OrderLineDeliverContainer container in Containers)
				{
					result += container.J5_QuantityInStore;
				}
				return result;
			}
		}

		public ZPropertyInfo J4_Calc_TotalQuantityAllocatedInfo
		{
			get { return GetZPropertyInfo(nameof(J4_Calc_TotalQuantityAllocated)); }
		}

		#endregion

		#endregion

		#region Property Overrides

		[BusinessObjectTestExclude]
		public override ZGuid J4_JO
		{
			get { return base.J4_JO; }
			set
			{
				base.J4_JO = value;
				Containers.MarkAsNeedingValidation();
			}
		}

		public override ZDecimal J4_Allocated
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.J4_Allocated; }
			set
			{
				base.J4_Allocated = value;
				Containers.MarkAsNeedingValidation();
				if (OrderLine != null)
				{
					OrderLine.MarkAsNeedingValidation();
					OrderLine.Deliveries.MarkAsNeedingValidation();
				}
			}
		}

		public override OrgAddress DeliveryPoint
		{
			get
			{
				OrgAddress result = null;
				if (OrderLine != null && OrderLine.Order != null && OrderLine.Order.Buyer != null)
				{
					ZQuery filter = new ZQuery(OrgAddressSchema.OA_Code, SQLComparisonOperator.Equal, J4_OA_NKDeliveryPoint);
					OrgAddress[] possibleResults = (OrgAddress[])OrderLine.Order.Buyer.Addresses.Find(filter);
					result = (possibleResults.Length == 0) ? null : possibleResults[0];
				}
				return result;
			}
		}

		#endregion

		#region Default DeliveryPoint from DestinationPort

		[List("J4_RL_NKDestinationPort_List")]
		public override ZString J4_RL_NKDestinationPort
		{
			get { return base.J4_RL_NKDestinationPort; }
			set
			{
				base.J4_RL_NKDestinationPort = value;
				if (!IsCopying && !fIsImportingData)
				{
					DefaultDeliveryPointFromDestinationPort();
				}

				// refresh J4_DeliverPoint_List
				fJ4_DeliverPoint_List = null;
				if (!IsValidationSuspended)
				{
					Validation.ValidateJ4_OA_NKDeliveryPoint();
				}
			}
		}

		void DefaultDeliveryPointFromDestinationPort()
		{
			if (!J4_RL_NKDestinationPort.IsEmpty &&
				OrderLine != null &&
				!OrderLine.Order.BuyerPK.IsEmpty)
			{
				OrgAddress deliveryPoint = FindSingleMatchOfBuyerAddressFromPort(J4_RL_NKDestinationPort);
				if (deliveryPoint != null)
				{
					J4_OA_NKDeliveryPoint = deliveryPoint.OA_Code;
				}
			}
		}

		OrgAddress FindSingleMatchOfBuyerAddressFromPort(ZString port)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(OrgAddressSchema.OA_RL_NKRelatedPortCode, SQLComparisonOperator.Equal, port);
			filter.AddToFilter(OrgAddressSchema.OA_OH, SQLComparisonOperator.Equal, OrderLine.Order.BuyerPK);

			OrgAddress[] possibleAddresses = (OrgAddress[])Factory.Load(typeof(OrgAddress), filter);
			return (possibleAddresses.Length == 1) ? possibleAddresses[0] : null;
		}

		#endregion

		#region Default DestinationPort from DeliveryPoint

		[List("J4_DeliverPoint_List")]
		public override ZString J4_OA_NKDeliveryPoint
		{
			get { return base.J4_OA_NKDeliveryPoint; }
			set
			{
				if (value != J4_OA_NKDeliveryPoint)
				{
					base.J4_OA_NKDeliveryPoint = value;
					if (!IsCopying && !fIsImportingData &&
						DeliveryPoint != null &&
						!DeliveryPoint.OA_RL_NKRelatedPortCode.IsEmpty)
					{
						J4_RL_NKDestinationPort = DeliveryPoint.OA_RL_NKRelatedPortCode;
					}
				}
			}
		}

		#endregion

		#region Lookups

		protected BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		public OrgAddressDependentCollection J4_DeliverPoint_List
		{
			get
			{
				if (fJ4_DeliverPoint_List == null)
				{
					if (OrderLine.Order != null && OrderLine.Order.Buyer != null)
					{
						fJ4_DeliverPoint_List = new OrgAddressDependentCollection(OrderLine.Order.Buyer);
					}
					else
					{
						fJ4_DeliverPoint_List = new OrgAddressDependentCollection(Factory);
					}
					ZQuery filter = new ZQuery();

					if (!J4_RL_NKDestinationPort.IsEmpty)
					{
						var destinationPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.Equal, J4_RL_NKDestinationPort));
						if (destinationPort != null)
						{
							filter = new ZQuery(OrgAddressSchema.OA_RL_NKRelatedPortCode, destinationPort.RL_Code);
						}
					}
					fJ4_DeliverPoint_List.Load(filter);
					fJ4_DeliverPoint_List.IsManagedForDataRefresh = true;
				}
				return fJ4_DeliverPoint_List;
			}
		}
		OrgAddressDependentCollection fJ4_DeliverPoint_List;

		public RefUNLOCOCollection J4_RL_NKDestinationPort_List
		{
			get { return BindingLists.RefUNLOCO_List; }
		}

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

		static readonly Overridable<Type> customLabelsProviderType = new Overridable<Type>(typeof(CustomLabelsProvider));

		protected static Type CustomLabelsProviderType
		{
			get
			{
				return customLabelsProviderType.Value;
			}
			set { customLabelsProviderType.Value = value; }
		}

		public static ICustomLabelsProvider NewCustomLabelsProvider(ICustomLabelsConfigOrgProvider configOrgProvider)
		{
			return (ICustomLabelsProvider)CustomLabelsProviderType.GetConstructor(new Type[] { typeof(ICustomLabelsConfigOrgProvider) }).Invoke(new object[] { configOrgProvider });
		}

		protected internal class CustomLabelsProvider : ICustomLabelsProvider
		{
			public CustomLabelsProvider(ICustomLabelsConfigOrgProvider configOrgProvider)
			{
				this.fConfigOrgProvider = configOrgProvider;
			}

			public ICustomLabelsConfigOrgProvider ConfigOrgProvider
			{
				get { return fConfigOrgProvider; }
			}

			public virtual CustomLabelInfoList GetCustomFields(OrgHeader configOrg, BusinessObjectFactory factory)
			{
				CustomLabelInfoList result = new CustomLabelInfoList(typeof(OrderLineDelivery), configOrg, ResString.GetMultilingualString("c5e35933-d678-4f6f-8157-080bf7c56a33", "the buyer of the order"), factory);
				result.Add(Constants.CustomLabels.OrderLineDelivery.CustomAttribute1, Schema.J4_CustomAttribute1, Constants.CustomLabels.Descriptions.CustomAttribute(1));
				result.Add(Constants.CustomLabels.OrderLineDelivery.CustomAttribute2, Schema.J4_CustomAttribute2, Constants.CustomLabels.Descriptions.CustomAttribute(2));
				result.Add(Constants.CustomLabels.OrderLineDelivery.CustomAttribute3, Schema.J4_CustomAttribute3, Constants.CustomLabels.Descriptions.CustomAttribute(3));
				result.Add(Constants.CustomLabels.OrderLineDelivery.CustomAttribute4, Schema.J4_CustomAttribute4, Constants.CustomLabels.Descriptions.CustomAttribute(4));
				result.Add(Constants.CustomLabels.OrderLineDelivery.CustomAttribute5, Schema.J4_CustomAttribute5, Constants.CustomLabels.Descriptions.CustomAttribute(5));
				result.Add(Constants.CustomLabels.OrderLineDelivery.CustomFlag1, Schema.J4_CustomFlag1, Constants.CustomLabels.Descriptions.CustomFlag(1));
				result.Add(Constants.CustomLabels.OrderLineDelivery.CustomFlag2, Schema.J4_CustomFlag2, Constants.CustomLabels.Descriptions.CustomFlag(2));
				result.Add(Constants.CustomLabels.OrderLineDelivery.CustomFlag3, Schema.J4_CustomFlag3, Constants.CustomLabels.Descriptions.CustomFlag(3));
				result.Add(Constants.CustomLabels.OrderLineDelivery.CustomFlag4, Schema.J4_CustomFlag4, Constants.CustomLabels.Descriptions.CustomFlag(4));
				result.Add(Constants.CustomLabels.OrderLineDelivery.CustomFlag5, Schema.J4_CustomFlag5, Constants.CustomLabels.Descriptions.CustomFlag(5));
				result.Add(Constants.CustomLabels.OrderLineDelivery.CustomDate1, Schema.J4_CustomDate1, Constants.CustomLabels.Descriptions.CustomDate(1));
				result.Add(Constants.CustomLabels.OrderLineDelivery.CustomDate2, Schema.J4_CustomDate2, Constants.CustomLabels.Descriptions.CustomDate(2));
				result.Add(Constants.CustomLabels.OrderLineDelivery.CustomDate3, Schema.J4_CustomDate3, Constants.CustomLabels.Descriptions.CustomDate(3));
				result.Add(Constants.CustomLabels.OrderLineDelivery.CustomDate4, Schema.J4_CustomDate4, Constants.CustomLabels.Descriptions.CustomDate(4));
				result.Add(Constants.CustomLabels.OrderLineDelivery.CustomDate5, Schema.J4_CustomDate5, Constants.CustomLabels.Descriptions.CustomDate(5));
				result.Add(Constants.CustomLabels.OrderLineDelivery.CustomDecimal1, Schema.J4_CustomDecimal1, Constants.CustomLabels.Descriptions.CustomNumber(1));
				result.Add(Constants.CustomLabels.OrderLineDelivery.CustomDecimal2, Schema.J4_CustomDecimal2, Constants.CustomLabels.Descriptions.CustomNumber(2));
				result.Add(Constants.CustomLabels.OrderLineDelivery.CustomDecimal3, Schema.J4_CustomDecimal3, Constants.CustomLabels.Descriptions.CustomNumber(3));
				result.Add(Constants.CustomLabels.OrderLineDelivery.CustomDecimal4, Schema.J4_CustomDecimal4, Constants.CustomLabels.Descriptions.CustomNumber(4));
				result.Add(Constants.CustomLabels.OrderLineDelivery.CustomDecimal5, Schema.J4_CustomDecimal5, Constants.CustomLabels.Descriptions.CustomNumber(5));
				return result;
			}

			protected ICustomLabelsConfigOrgProvider fConfigOrgProvider;
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new OrderLineDeliveryFetchStrategy(this);
		}

		#endregion
	}
}
