using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;
using ResString = Enterprise.Freight.Forwarding.Business.ResString;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	[DependentBusinessObject(typeof(OrderLineDelivery), "Containers")]
	[ProvideMetaDataProperty("DefaultNumberOfDecimals", MetaDataTypes.DecimalPlaces)]
	public class OrderLineDeliverContainer : AutoJobOrderLineDeliverContainer,
		Integration.Forwarding.IOrderLineDeliverContainer,
		ISupportDataImporting,
		IDefaultNumberOfDecimalsSupporterWithSchemaColumn
	{
		public OrderLineDeliverContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public Order Order
		{
			get { return (OrderLineDelivery != null && OrderLineDelivery.OrderLine != null) ? OrderLineDelivery.OrderLine.Order : null; }
		}

		#region Container Key

		public void SetContainerKey(ZString containerNum, ZString arrivalVessel, ZString voyage)
		{
			base.J5_ContainerNum = containerNum;
			base.J5_RV_NKArrivalVessel = arrivalVessel;
			base.J5_Voyage = voyage;
			UpdateContainerDetailsFromRelatedContainers();
		}

		#endregion

		#region Test Data
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			if ((kind & TestBusinessObjectKind.PopulateStrings) != 0)
			{
				this.J5_F3_NKPackType = Core.Constants.PkgUnit.Case;
				this.J5_VolumeUQ = Core.Constants.Volume.CubicYards;
				this.J5_WeightUQ = Core.Constants.Weight.PoundsTroy;
			}
		}

#endif
		#endregion

		#region Try Attach To Delivery

		public bool TryAttachToDelivery(out string errorMessage)
		{
			var matchingOrder = LoadAndGetMatchingOrder();
			if (matchingOrder == null)
			{
				errorMessage = Res.GetString("1a4a063d-1818-988d-4a22-45f5c966b5ba", "No order found matching order number: {0}", J5_OrderNumberForAttach);
				return false;
			}

			var matchingLines = matchingOrder
				.OrderLines
				.Where(ol => ol.JO_Partno.EqualsIgnoringCase(J5_PartnoForAttach)).ToList();

			if (matchingLines.Count == 0)
			{
				errorMessage = Res.GetString("4488aa8a-95dd-c4aa-4ba9-2ca9610800b0", "Could not find order line with product #: {0}", J5_PartnoForAttach);
				return false;
			}

			var matchingDelivery = matchingLines
				.SelectMany(ml => ml.Deliveries)
				.FirstOrDefault(dl => dl.J4_RL_NKDestinationPort.EqualsIgnoringCase(J5_RL_DestinationPortForAttach));

			if (matchingDelivery == null)
			{
				errorMessage = Res.GetString("3ce1d6fa-8101-feb4-4a8c-fdce812b7438", "Could not find delivery with destination port: {0}", J5_RL_DestinationPortForAttach);
				return false;
			}

			if (matchingDelivery.Containers.Any(c => c.J5_ContainerNum.EqualsIgnoringCase(J5_ContainerNum)))
			{
				errorMessage = Res.GetString("c095ecd1-b667-a2a7-4135-09f956289c2e", "Container with number '{0}' is already attached", J5_ContainerNum);
				return false;
			}

			AttachContainerToDelivery(matchingDelivery);
			RelatedContainers.Load();

			errorMessage = string.Empty;
			return true;
		}

		Order LoadAndGetMatchingOrder()
		{
			var buyerAddresses = Order?.Buyer == null
				? (object)ZGuid.Empty
				: Order.Buyer.Addresses.Select(x => x.PK);

			var orderFilter = new ZQuery();
			orderFilter.AddToFilter(JobOrderHeaderSchema.JD_OrderNumber, SQLComparisonOperator.Equal, J5_OrderNumberForAttach);
			orderFilter.AddToFilter(JoinCondition.And, JobOrderHeaderSchema.JD_OA_BuyerAddress, SQLComparisonOperator.Equal, buyerAddresses);
			orderFilter.AddToFilter(JoinCondition.And, JobOrderHeaderSchema.JD_OrderNumberSplit, SQLComparisonOperator.Equal, (byte)0);

			return Factory.LoadTop1<Order>(orderFilter);
		}

		void AttachContainerToDelivery(OrderLineDelivery matchingDelivery)
		{
			var attachedContainer = matchingDelivery.Containers.AddNew();
			attachedContainer.J5_ContainerNum = J5_ContainerNum;
			attachedContainer.J5_Voyage = J5_Voyage;
			attachedContainer.J5_RV_NKArrivalVessel = J5_RV_NKArrivalVessel;
			attachedContainer.J5_RL_NKLoadPort = J5_RL_NKLoadPort;
			attachedContainer.J5_MasterBill = J5_MasterBill;
			attachedContainer.J5_SightedDate = J5_SightedDate;
			attachedContainer.J5_ETA = J5_ETA;
			attachedContainer.J5_ETD = J5_ETD;
		}

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			J5_F3_NKPackType = Enterprise.Core.Constants.PkgUnit.Unit;
			J5_WeightUQ = Enterprise.Core.Constants.Weight.Kilograms;
			J5_VolumeUQ = Enterprise.Core.Constants.Volume.CubicMetres;
		}

		#endregion

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region Related Business Objects

		public CommonContainer JobContainer
		{
			get
			{
				if (J5_JC.IsEmpty)
				{
					J5_JC = Factory.New(typeof(CommonContainer)).PK;
				}
				return Factory.Load<CommonContainer>(J5_JC);
			}
		}

		public OrderLineDelivery OrderLineDelivery
		{
			get { return Factory.Load<OrderLineDelivery>(J5_J4); }
		}

		public RelatedOrderLineDeliverContainerCollection RelatedContainers
		{
			get
			{
				RelatedOrderLineDeliverContainerCollection lRelatedContainers;
				if (fRelatedContainers == null)
				{
					lRelatedContainers = new RelatedOrderLineDeliverContainerCollection(this);
					if (OrderLineDelivery != null)
					{
						OrderLineDelivery.RegisterEditableChildObject(lRelatedContainers);
					}
				}
				else
				{
					lRelatedContainers = fRelatedContainers;
				}
				lRelatedContainers.Load();
				fRelatedContainers = lRelatedContainers;
				return fRelatedContainers;
			}
		}

		RelatedOrderLineDeliverContainerCollection fRelatedContainers;

		void UpdateContainerDetailsFromRelatedContainers()
		{
			if (RelatedContainers.Count > 0)
			{
				J5_ContainerSeal = RelatedContainers[0].J5_ContainerSeal;
				J5_SightedDate = RelatedContainers[0].J5_SightedDate;
				J5_RC_NKContainerType = RelatedContainers[0].J5_RC_NKContainerType;
				J5_ETA = RelatedContainers[0].J5_ETA;
				J5_ETD = RelatedContainers[0].J5_ETD;
				J5_MasterBill = RelatedContainers[0].J5_MasterBill;
			}
			else if (OrderLineDelivery != null && OrderLineDelivery.OrderLine != null &&
				OrderLineDelivery.OrderLine.ContainerNumbersList.ContainsCode(J5_ContainerNum))
			{
				J5_RC_NKContainerType = OrderLineDelivery.OrderLine.ContainerNumbersList.GetDescriptionFromCode(J5_ContainerNum);
			}
		}

		bool fInSetPropertyOnRelatedContainers;

		void SetPropertyOnRelatedContainers(string propName, object value)
		{
			if (!fInSetPropertyOnRelatedContainers)
			{
				fInSetPropertyOnRelatedContainers = true;
				try
				{
					foreach (OrderLineDeliverContainer relatedContainer in this.RelatedContainers)
					{
						relatedContainer[propName] = value;
					}
				}
				finally
				{
					fInSetPropertyOnRelatedContainers = false;
				}
			}
		}

		#endregion

		#region New Properties

		public virtual bool IsDelivered
		{
			get { return J5_InstoreDate.IsValid; }
		}

		#region J5_JD_OrderNumber

		public ZString J5_JD_OrderNumber
		{
			get { return (OrderLineDelivery == null) ? ZString.Empty : OrderLineDelivery.OrderLine.Order.JD_OrderNumber; }
		}

		public ZPropertyInfo J5_JD_OrderNumberInfo
		{
			get { return GetZPropertyInfo(nameof(J5_JD_OrderNumber)); }
		}

		#endregion

		#region J5_JO_Partno

		public ZString J5_JO_Partno
		{
			get { return (this.OrderLineDelivery == null) ? ZString.Empty : this.OrderLineDelivery.OrderLine.JO_Partno; }
		}

		public ZPropertyInfo J5_JO_PartnoInfo
		{
			get { return GetZPropertyInfo(nameof(J5_JO_Partno)); }
		}

		#endregion

		#region J5_J4_RL_NKDestinationPort

		public ZString J5_J4_RL_NKDestinationPort
		{
			get { return (this.OrderLineDelivery == null) ? ZString.Empty : this.OrderLineDelivery.J4_RL_NKDestinationPort; }
		}

		public ZPropertyInfo J5_J4_RL_NKDestinationPortInfo
		{
			get { return GetZPropertyInfo(nameof(J5_J4_RL_NKDestinationPort)); }
		}

		#endregion

		#region J5_OrderNumberForAttach

		ZString fJ5_OrderNumberForAttach;

		[MaxLength(Order.Schema.JD_OrderNumberMaxLength)]
		public ZString J5_OrderNumberForAttach
		{
			get { return fJ5_OrderNumberForAttach; }
			set
			{
				if (fJ5_OrderNumberForAttach != value)
				{
					CheckMaximumLength(J5_OrderNumberForAttachInfo, value);
					fJ5_OrderNumberForAttach = value;
					J5_OrderNumberForAttachInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo J5_OrderNumberForAttachInfo
		{
			get { return GetZPropertyInfo(nameof(J5_OrderNumberForAttach)); }
		}

		#endregion

		#region J5_PartnoForAttach

		ZString fJ5_PartnoForAttach;

		[MaxLength(OrgSupplierPart.Schema.OP_PartNumMaxLength)]
		public ZString J5_PartnoForAttach
		{
			get { return fJ5_PartnoForAttach; }
			set
			{
				if (fJ5_PartnoForAttach != value)
				{
					CheckMaximumLength(J5_PartnoForAttachInfo, value);
					fJ5_PartnoForAttach = value;
					J5_PartnoForAttachInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo J5_PartnoForAttachInfo
		{
			get { return GetZPropertyInfo(nameof(J5_PartnoForAttach)); }
		}

		#endregion

		#region J5_RL_DestinationPortForAttach

		ZString fJ5_RL_DestinationPortForAttach;

		[MaxLength(Schema.J5_RL_NKLoadPortMaxLength)]
		public ZString J5_RL_DestinationPortForAttach
		{
			get { return fJ5_RL_DestinationPortForAttach; }
			set
			{
				if (fJ5_RL_DestinationPortForAttach != value)
				{
					CheckMaximumLength(J5_RL_DestinationPortForAttachInfo, value);
					fJ5_RL_DestinationPortForAttach = value;
					J5_RL_DestinationPortForAttachInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo J5_RL_DestinationPortForAttachInfo
		{
			get { return GetZPropertyInfo(nameof(J5_RL_DestinationPortForAttach)); }
		}

		#endregion

		#endregion

		#region Lookups

		public JobVoyageCollection J5_Voyage_List
		{
			get
			{
				JobVoyageCollection result = new JobVoyageCollection(Factory);
				if (!J5_RV_NKArrivalVessel.IsEmpty)
				{
					result.Load(new ZQuery(JobVoyageSchema.JV_RV_NKVessel, SQLComparisonOperator.Equal, J5_RV_NKArrivalVessel));
				}
				return result;
			}
		}

		RefVesselCollection fJ5_Vessel_List;
		public RefVesselCollection J5_Vessel_List
		{
			get
			{
				if (fJ5_Vessel_List == null)
				{
					fJ5_Vessel_List = new RefVesselCollection(Factory);
				}
				return fJ5_Vessel_List;
			}
		}

		RefContainerCollection fJ5_RC_List;
		public RefContainerCollection J5_RC_List
		{
			get
			{
				if (fJ5_RC_List == null)
				{
					fJ5_RC_List = new RefContainerCollection(Factory);
				}
				return fJ5_RC_List;
			}
		}

		public CodeDescriptionPairList J5_F3_NKPackType_List
		{
			get { return new RefPackTypeCollection(Factory).GetAsCodeDescriptionPairWithStandardUnits(); }
		}

		public CodeDescriptionPairList J5_VolumeUQ_List
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Volume); }
		}

		public CodeDescriptionPairList J5_WeightUQ_List
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		public OrgSupplierPartCollection J5_JO_Partno_List
		{
			get { return OrderLineDelivery.OrderLine.JO_Partno_List; }
		}

		#endregion

		#region Property Overrides

		[BusinessObjectTestExclude]
		public override ZGuid J5_J4
		{
			get { return base.J5_J4; }
		}

		public override ZDecimal J5_QuantityInStore
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.J5_QuantityInStore; }
			set
			{
				base.J5_QuantityInStore = value;
				OrderLineDelivery.MarkAsNeedingValidation();
				if (OrderLineDelivery != null)
				{
					OrderLineDelivery.Containers.MarkAsNeedingValidation();
				}
			}
		}

		[MeasureUnit(Schema.J5_WeightUQ, MeasureUnitType.Weight)]
		public override ZDecimal J5_Weight
		{
			get { return base.J5_Weight; }
			set { base.J5_Weight = this.GetRoundedValue(JobOrderLineDeliverContainerSchema.J5_Weight, J5_WeightInfo, value); }
		}

		[MeasureUnit(Schema.J5_VolumeUQ, MeasureUnitType.Volume)]
		public override ZDecimal J5_Volume
		{
			get { return base.J5_Volume; }
			set { base.J5_Volume = this.GetRoundedValue(JobOrderLineDeliverContainerSchema.J5_Volume, J5_VolumeInfo, value); }
		}

		[List("J5_WeightUQ_List")]
		public override ZString J5_WeightUQ
		{
			get { return base.J5_WeightUQ; }
			set
			{
				base.J5_WeightUQ = value;
				this.SetRoundedValue(JobOrderLineDeliverContainerSchema.J5_Weight, J5_WeightInfo);
			}
		}

		[List("J5_VolumeUQ_List")]
		public override ZString J5_VolumeUQ
		{
			get { return base.J5_VolumeUQ; }
			set
			{
				base.J5_VolumeUQ = value;
				this.SetRoundedValue(JobOrderLineDeliverContainerSchema.J5_Volume, J5_VolumeInfo);
			}
		}

		[List("J5_F3_NKPackType_List")]
		public override ZString J5_F3_NKPackType
		{
			get { return base.J5_F3_NKPackType; }
			set { base.J5_F3_NKPackType = value; }
		}

		[List("OrderLineDelivery.OrderLine.ContainerNumbersList")]
		public override ZString J5_ContainerNum
		{
			get { return base.J5_ContainerNum; }
			set
			{
				base.J5_ContainerNum = value.ToUpper();
				UpdateContainerDetailsFromRelatedContainers();
			}
		}

		[List("J5_Voyage_List")]
		public override ZString J5_Voyage
		{
			get { return base.J5_Voyage; }
			set
			{
				base.J5_Voyage = value;
				UpdateContainerDetailsFromRelatedContainers();
			}
		}

		[List("J5_Vessel_List")]
		public override ZString J5_RV_NKArrivalVessel
		{
			get { return base.J5_RV_NKArrivalVessel; }
			set
			{
				base.J5_RV_NKArrivalVessel = value;
				UpdateContainerDetailsFromRelatedContainers();
			}
		}

		public override ZString J5_ContainerSeal
		{
			get { return base.J5_ContainerSeal; }
			set
			{
				if (base.J5_ContainerSeal != value)
				{
					base.J5_ContainerSeal = value;
					SetPropertyOnRelatedContainers(Schema.J5_ContainerSeal, value);
				}
			}
		}

		public override ZDateTime J5_SightedDate
		{
			get { return base.J5_SightedDate; }
			set
			{
				if (base.J5_SightedDate != value)
				{
					base.J5_SightedDate = value;
					SetPropertyOnRelatedContainers(Schema.J5_SightedDate, value);
				}
			}
		}

		[List("J5_RC_List")]
		public override ZString J5_RC_NKContainerType
		{
			get { return base.J5_RC_NKContainerType; }
			set
			{
				if (base.J5_RC_NKContainerType != value)
				{
					base.J5_RC_NKContainerType = value;
					SetPropertyOnRelatedContainers(Schema.J5_RC_NKContainerType, value);
				}
			}
		}

		public override ZDateTime J5_ETA
		{
			get { return base.J5_ETA; }
			set
			{
				if (base.J5_ETA != value)
				{
					base.J5_ETA = value;
					SetPropertyOnRelatedContainers(Schema.J5_ETA, value);
				}
			}
		}

		public override ZDateTime J5_ETD
		{
			get { return base.J5_ETD; }
			set
			{
				if (base.J5_ETD != value)
				{
					base.J5_ETD = value;
					SetPropertyOnRelatedContainers(Schema.J5_ETD, value);
				}
			}
		}

		public override ZString J5_MasterBill
		{
			get { return base.J5_MasterBill; }
			set
			{
				if (base.J5_MasterBill != value)
				{
					base.J5_MasterBill = value;
					SetPropertyOnRelatedContainers(Schema.J5_MasterBill, value);
				}
			}
		}

		#endregion

		#region ICustomLabelsProvider Members

		public class CustomLabelsProvider : ICustomLabelsProvider
		{
			public CustomLabelsProvider(ICustomLabelsConfigOrgProvider configOrgProvider)
			{
				fConfigOrgProvider = configOrgProvider;
			}

			public ICustomLabelsConfigOrgProvider ConfigOrgProvider
			{
				get { return fConfigOrgProvider; }
			}

			public CustomLabelInfoList GetCustomFields(OrgHeader configOrg, BusinessObjectFactory factory)
			{
				CustomLabelInfoList result = new CustomLabelInfoList(typeof(OrderLineDeliverContainer), configOrg, ResString.GetMultilingualString("ed8d37b5-8574-4f62-85ab-2249ddddec7b", "the buyer of the order"), factory);
				result.Add(Constants.CustomLabels.OrderLineDeliverContainer.CustomAttribute1, Schema.J5_CustomAttribute1, Constants.CustomLabels.Descriptions.CustomAttribute(1));
				result.Add(Constants.CustomLabels.OrderLineDeliverContainer.CustomAttribute2, Schema.J5_CustomAttribute2, Constants.CustomLabels.Descriptions.CustomAttribute(2));
				result.Add(Constants.CustomLabels.OrderLineDeliverContainer.CustomAttribute3, Schema.J5_CustomAttribute3, Constants.CustomLabels.Descriptions.CustomAttribute(3));
				result.Add(Constants.CustomLabels.OrderLineDeliverContainer.CustomFlag1, Schema.J5_CustomFlag1, Constants.CustomLabels.Descriptions.CustomFlag(1));
				result.Add(Constants.CustomLabels.OrderLineDeliverContainer.CustomFlag2, Schema.J5_CustomFlag2, Constants.CustomLabels.Descriptions.CustomFlag(2));
				result.Add(Constants.CustomLabels.OrderLineDeliverContainer.CustomFlag3, Schema.J5_CustomFlag3, Constants.CustomLabels.Descriptions.CustomFlag(3));
				result.Add(Constants.CustomLabels.OrderLineDeliverContainer.CustomDate1, Schema.J5_CustomDate1, Constants.CustomLabels.Descriptions.CustomDate(1));
				result.Add(Constants.CustomLabels.OrderLineDeliverContainer.CustomDate2, Schema.J5_CustomDate2, Constants.CustomLabels.Descriptions.CustomDate(2));
				result.Add(Constants.CustomLabels.OrderLineDeliverContainer.CustomDate3, Schema.J5_CustomDate3, Constants.CustomLabels.Descriptions.CustomDate(3));
				result.Add(Constants.CustomLabels.OrderLineDeliverContainer.CustomDecimal1, Schema.J5_CustomDecimal1, Constants.CustomLabels.Descriptions.CustomNumber(1));
				result.Add(Constants.CustomLabels.OrderLineDeliverContainer.CustomDecimal2, Schema.J5_CustomDecimal2, Constants.CustomLabels.Descriptions.CustomNumber(2));
				result.Add(Constants.CustomLabels.OrderLineDeliverContainer.CustomDecimal3, Schema.J5_CustomDecimal3, Constants.CustomLabels.Descriptions.CustomNumber(3));

				result.Add(Constants.CustomLabels.OrderLineDeliverContainer.QuantityInvoiced, Schema.J5_QuantityInvoiced, Constants.CustomLabels.OrderLineDeliverContainer.Descriptions.QuantityInvoiced, CustomLabelStyles.ShowByDefault);
				result.Add(Constants.CustomLabels.OrderLineDeliverContainer.QuantityDelivered, Schema.J5_QuantityInStore, Constants.CustomLabels.OrderLineDeliverContainer.Descriptions.QuantityDelivered, CustomLabelStyles.ShowByDefault);
				return result;
			}

			readonly ICustomLabelsConfigOrgProvider fConfigOrgProvider;
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

		#region IDefaultNumberOfDecimalsSupporter Members

		ZString IDefaultNumberOfDecimalsSupporter.TransportMode
		{
			get
			{
				IDefaultNumberOfDecimalsSupporter parentOrder = Order;
				var result = (parentOrder != null) ? parentOrder.TransportMode : ZString.Empty;

				return result;
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
				case Schema.J5_Volume:
					unitOfMeasure = J5_VolumeUQ;
					break;

				case Schema.J5_Weight:
					unitOfMeasure = J5_WeightUQ;
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
			this.SetRoundedValue(JobOrderLineDeliverContainerSchema.J5_Weight, J5_WeightInfo);
			this.SetRoundedValue(JobOrderLineDeliverContainerSchema.J5_Volume, J5_VolumeInfo);
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new OrderLineDeliverContainerFetchStrategy(this);
		}

		#endregion
	}
}
