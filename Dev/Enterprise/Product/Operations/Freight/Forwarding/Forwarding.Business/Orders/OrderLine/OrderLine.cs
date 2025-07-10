using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
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
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;
using Constants = Enterprise.Core.Constants;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;
using ResString = Enterprise.Freight.Forwarding.Business.ResString;
using WorkflowSelectionOrgTypeCodes = Enterprise.Registry.Business.ClientInTemplateSelectionOrgTypeList.Codes;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	[UserDefinedValues]
	[UniversalDataContext(DataContextType.OrderManagerOrderLine)]
	[DependentBusinessObject(typeof(Order), "OrderLines")]
	[CodeProperty(OrderLine.Schema.OrderAndOrderLineNumber), DescriptionProperty(AutoJobOrderLine.Schema.JO_Description)]
	[ProvideMetaDataProperty("DefaultNumberOfDecimals", MetaDataTypes.DecimalPlaces)]
	public class OrderLine :
		AutoJobOrderLine,
		ICustomFieldProvider,
		ICustomLabelsConfigOrgProvider,
		IJobNumber,
		Integration.Forwarding.IOrderLine,
		ISupportDataImporting,
		IWorkflowTriggerFieldChangeSource,
		IUNDGDataItemProvider,
		ICommonInvoice,
		IChargeApportionee,
		IChargeHolder,
		IDocAddresses,
		IDocumentSupportable,
		ILandedCostDistributeTo,
		IProcessHandlingInfoProvider,
		IUltimateDistributee,
		IDefaultNumberOfDecimalsSupporterWithSchemaColumn,
		IExternalRequestGenerationProvider,
		IWorkflowProvider
	{
		#region Schema

		public new class Schema : AutoJobOrderLine.Schema
		{
			public const string JO_Calc_OrderLineNoAndSubLineNo = "JO_Calc_OrderLineNoAndSubLineNo";
			public const string JO_QuantityRemaining = "JO_QuantityRemaining";
			public const string ManufacturerNameOrPK = "ManufacturerNameOrPK";
			public const string GoodsAvailableAtNameOrPK = "GoodsAvailableAtNameOrPK";
			public const string GoodsDeliveredToNameOrPK = "GoodsDeliveredToNameOrPK";
			public const string ConsigneeDocumentaryNameOrPK = "ConsigneeDocumentaryNameOrPK";
			public const string OrderAndOrderLineNumber = "OrderAndOrderLineNumber";
			public const string ProductPK = "ProductPK";
		}

		#endregion

		#region Construction

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Virtual methods are reviewed. All possible values are expected and handled.")]
		public OrderLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ApportionmentDirtyChangedEventHandler += new EventHandler(OnApportionmentBeingDirty);
		}

		public static OrderLine New(BusinessObjectFactory factory)
		{
			return factory.New<OrderLine>();
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new OrderLineFetchStrategy(this);
		}

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			JO_F3_NKPackType = Enterprise.Core.Constants.PkgUnit.Unit;
			JO_LineStatus = Constants.OrderStatus.Open;
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			if (!IsDeleted)
			{
				this.Deliveries.DeleteAll();
				ClearLinksToPackProducts();
				WorkflowItems.RemoveAndDeleteAll();
				base.Delete();
			}
		}

		internal void ClearLinksToPackProducts()
		{
			ZQuery query = new ZQuery(JobPackProductSchema.D2_JO, PK);
			PackProduct[] packProducts = Factory.Load<PackProduct>(query);
			foreach (var packProduct in packProducts)
			{
				packProduct.D2_JO = ZGuid.Empty;
			}
		}

		#endregion

		#region Clone

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			args.AddExcludedColumns(new List<string>() { JobOrderLineSchema.Constants.JO_ActualVolume });

			var result = (OrderLine)base.CloneInternal(args);
			result.JO_ActualVolume = JO_ActualVolume;

			foreach (OrderLineDelivery delivery in Deliveries.ToArray())
			{
				result.Deliveries.Add((OrderLineDelivery)delivery.Clone());
			}
			return result;
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			var result = new List<string>(base.GetPropertiesToExcludeFromCloning());
			result.Add(JobOrderLineSchema.JO_OpenQuantity.Name);
			return result;
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region Saving

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (!IsLineStatusCancelled &&
				OrderOrOrderLineHasChanges &&
				!IsStatusCustomisedByUser)
			{
				UpdateOrderLineStatus();
			}
		}

		public bool IsLineStatusCancelled
		{
			get { return JO_LineStatus == Constants.OrderStatus.Cancelled; }
		}

		bool OrderOrOrderLineHasChanges
		{
			get { return (Order != null && Order.HasChanges || HasChanges); }
		}

		bool IsStatusCustomisedByUser
		{
			get { return !new CodeDescriptionPairList(OLookUpEditType.OrderLineStatus).ContainsCode(JO_LineStatus); }
		}

		protected virtual void UpdateOrderLineStatus()
		{
			if (Order.PreAdvice == null)
			{
				UpdateOrderLineStatusForNoPreAdvice();
			}
			else
			{
				UpdateOrderLineStatusForPreAdvice();
			}
		}

		void UpdateOrderLineStatusForNoPreAdvice()
		{
			bool hasAtLeast1ContainerAttached = false;
			bool hasAtLeast1ContainerDelivered = false;
			bool isCompletelyDelivered = true;

			foreach (OrderLineDeliverContainer container in ContainersOnAllDeliveries)
			{
				hasAtLeast1ContainerAttached = true;
				if (container.IsDelivered)
				{
					hasAtLeast1ContainerDelivered = true;
				}
				else
				{
					isCompletelyDelivered = false;
				}
			}

			if (!hasAtLeast1ContainerAttached)
			{
				isCompletelyDelivered = false;
			}

			if (HasOutstandingBalance && hasAtLeast1ContainerDelivered)
			{
				JO_LineStatus = Constants.OrderStatus.PartDelivered;
			}
			else if (!HasOutstandingBalance && isCompletelyDelivered)
			{
				JO_LineStatus = Constants.OrderStatus.Delivered;
			}
		}

		internal bool HasOutstandingBalance
		{
			get { return JO_QtyReceived != JO_Quantity || JO_Quantity == 0; }
		}

		void UpdateOrderLineStatusForPreAdvice()
		{
			if (JO_QtyReceived > 0)
			{
				JO_LineStatus = HasOutstandingBalance
									? Constants.OrderStatus.PartDelivered
									: Constants.OrderStatus.Delivered;
			}
			else if (IsInDatabase && JO_QtyReceived == 0 && JO_QtyReceivedInfo.HasChanges)
			{
				JO_LineStatus = Constants.OrderStatus.PartDeliveredQuantityAmendedToZero;
			}
			else
			{
				JO_LineStatus = Constants.OrderStatus.Open;
			}
		}

		internal IEnumerable<OrderLineDeliverContainer> ContainersOnAllDeliveries
		{
			get
			{
				foreach (OrderLineDelivery delivery in Deliveries)
				{
					foreach (OrderLineDeliverContainer container in delivery.Containers)
					{
						yield return container;
					}
				}
			}
		}

		#endregion

		#region Related Business Objects

		public Order Order
		{
			get { return (Order)Factory.Load(GetOrderType(), JO_JD); }
		}

		protected virtual Type GetOrderType()
		{
			return typeof(Order);
		}

		[ChildEditable(true)]
		public OrderLineDeliveryCollection Deliveries
		{
			get
			{
				if (fDeliveries == null)
				{
					fDeliveries = new OrderLineDeliveryCollection(Factory, this);
					if (Order == null || Order.OrderLineDeliveriesEditable)
					{
						RegisterEditableChildObject(fDeliveries);
					}
				}
				return fDeliveries;
			}
		}
		OrderLineDeliveryCollection fDeliveries;

		[ChildEditable(true)]
		public OrderLineDeliveryCollection DeliveriesAlwaysEditable
		{
			get
			{
				if (fDeliveries == null)
				{
					fDeliveries = new OrderLineDeliveryCollection(Factory, this);
					RegisterEditableChildObject(fDeliveries);
				}
				return fDeliveries;
			}
		}

		/// <summary>
		/// Get the part referenced by JO_Partno and the order's buyer/supplier.
		/// </summary>
		public OrgSupplierPart Product
		{
			get
			{
				Order order = this.Order;
				return order != null ? new OrgSupplierPart.Loader(Factory).Load(JO_Partno, order.BuyerPK, order.SupplierPK) : null;
			}
		}

		#region UNDGs

		[ChildEditable(true)]
		public UNDGDataItemCollection UNDGs
		{
			get
			{
				if (fUNDGs == null)
				{
					fUNDGs = new UNDGDataItemCollection(this);
					RegisterEditableChildObject(fUNDGs);
				}
				return fUNDGs;
			}
		}
		UNDGDataItemCollection fUNDGs;

		bool IUNDGDataItemProvider.NeedFetchHintForLoad => true;

		[BusinessObjectTestExclude]
		public ZString FirstUNDGSubstance
		{
			get { return UNDGs.Count > 0 && UNDGs[0].Substance != null ? UNDGs[0].Substance.DG_Code : ZString.Empty; }
			set
			{
				UNDGs.TryGetOrCreate(value, UNDGSubstanceStandardTypes.IMO);
				FirstUNDGSubstanceInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo FirstUNDGSubstanceInfo
		{
			get { return GetZPropertyInfo(nameof(FirstUNDGSubstance)); }
		}

		#endregion

		#endregion

		#region Properties

		public ZString OrderAndOrderLineNumber
		{
			get { return Order.JD_OrderNumber + " - " + JO_LineNo; }
		}

		[MeasureUnit(Schema.JO_UnitOfVolume, MeasureUnitType.Volume)]
		public override ZDecimal JO_ActualVolume
		{
			get { return base.JO_ActualVolume; }
			set { base.JO_ActualVolume = this.GetRoundedValue(JobOrderLineSchema.JO_ActualVolume, JO_ActualVolumeInfo, value); }
		}

		[MeasureUnit(Schema.JO_UnitOfWeight, MeasureUnitType.Weight)]
		public override ZDecimal JO_ActualWeight
		{
			get { return base.JO_ActualWeight; }
			set { base.JO_ActualWeight = this.GetRoundedValue(JobOrderLineSchema.JO_ActualWeight, JO_ActualWeightInfo, value); }
		}

		public override ZString JO_HSCode
		{
			get => base.JO_HSCode;
			set => base.JO_HSCode = value;
		}

		[List("WeightUnit_List")]
		public override ZString JO_UnitOfWeight
		{
			get { return base.JO_UnitOfWeight; }
			set
			{
				base.JO_UnitOfWeight = value;
				this.SetRoundedValue(JobOrderLineSchema.JO_ActualWeight, JO_ActualWeightInfo);
			}
		}

		[List("VolumeUnit_List")]
		public override ZString JO_UnitOfVolume
		{
			get { return base.JO_UnitOfVolume; }
			set
			{
				base.JO_UnitOfVolume = value;
				SetJO_ActualVolume();

				this.SetRoundedValue(JobOrderLineSchema.JO_ActualVolume, JO_ActualVolumeInfo);
			}
		}

		[List("JO_LineStatus_List")]
		public override ZString JO_LineStatus
		{
			get { return base.JO_LineStatus; }
			set { base.JO_LineStatus = value; }
		}

		[List("JO_INCO_List")]
		public override ZString JO_INCO
		{
			get { return base.JO_INCO; }
			set { base.JO_INCO = value; }
		}

		[List("ContainerNumbersList")]
		public override ZString JO_ContainerNumber
		{
			get { return base.JO_ContainerNumber; }
			set { base.JO_ContainerNumber = value; }
		}

		[List("Lookups.CountryOfOrigins")]
		public override ZString JO_RN_NKCountryOfOrigin
		{
			get { return base.JO_RN_NKCountryOfOrigin; }
			set { base.JO_RN_NKCountryOfOrigin = value; }
		}

		[List("JO_F3_NKPackType_List")]
		public override ZString JO_F3_NKPackType
		{
			get { return base.JO_F3_NKPackType; }
			set { base.JO_F3_NKPackType = value; }
		}

		[List("JO_F3_NKPackType_List")]
		public override ZString JO_InnerPacksUQ
		{
			get { return base.JO_InnerPacksUQ; }
			set { base.JO_InnerPacksUQ = value; }
		}

		[List("JO_F3_NKPackType_List")]
		public override ZString JO_OuterPacksUQ
		{
			get { return base.JO_OuterPacksUQ; }
			set { base.JO_OuterPacksUQ = value; }
		}

		[List("Lookups.JO_OuterPackUnitOfDimension_List")]
		public override ZString JO_OuterPackUnitOfDimension
		{
			get
			{
				return base.JO_OuterPackUnitOfDimension;
			}
			set
			{
				base.JO_OuterPackUnitOfDimension = value;
				SetJO_ActualVolume();
			}
		}

		public override ZDecimal JO_OuterPackHeight
		{
			get
			{
				return base.JO_OuterPackHeight;
			}
			set
			{
				base.JO_OuterPackHeight = this.GetRoundedValue(JobOrderLineSchema.JO_OuterPackHeight, JO_OuterPackHeightInfo, value);
				SetJO_ActualVolume();
			}
		}

		public override ZDecimal JO_OuterPackLength
		{
			get
			{
				return base.JO_OuterPackLength;
			}
			set
			{
				base.JO_OuterPackLength = this.GetRoundedValue(JobOrderLineSchema.JO_OuterPackLength, JO_OuterPackLengthInfo, value);
				SetJO_ActualVolume();
			}
		}

		public override ZDecimal JO_OuterPackWidth
		{
			get
			{
				return base.JO_OuterPackWidth;
			}
			set
			{
				base.JO_OuterPackWidth = this.GetRoundedValue(JobOrderLineSchema.JO_OuterPackWidth, JO_OuterPackWidthInfo, value);
				SetJO_ActualVolume();
			}
		}

		void SetJO_ActualVolume()
		{
			bool shouldRecalculateVolume = !IsCopying && !fIsImportingData;

			if (shouldRecalculateVolume && JO_OuterPackWidth > 0 && JO_OuterPackHeight > 0 && JO_OuterPackLength > 0 && JO_OuterPacks > 0)
			{
				JO_ActualVolume = FreightUtilities.CalculateVolume(JO_ActualVolume, (int)JO_OuterPacks, JO_OuterPackLength, JO_OuterPackWidth, JO_OuterPackHeight, JO_OuterPackUnitOfDimension, JO_UnitOfVolume, JobOrderLineSchema.JO_ActualVolume.Scale);
			}
		}

		[BusinessObjectTestExclude]
		public override ZGuid JO_JD
		{
			get { return base.JO_JD; }
			set
			{
				var changed = base.JO_JD != value;
				base.JO_JD = value;
				Deliveries.MarkAsNeedingValidation();
				foreach (OrderLineDelivery delivery in Deliveries)
				{
					delivery.Containers.MarkAsNeedingValidation();
				}
				if (changed)
				{
					SetDefaultTolerances();
				}
			}
		}

		public override ZDecimal JO_QtyReceived
		{
			get { return base.JO_QtyReceived; }
			set
			{
				base.JO_QtyReceived = value;
				if (Order != null)
				{
					Order.JD_Calc_TotalQuantityReceivedInfo.RefreshBinding();
					Order.JD_Calc_TotalQuantityRemainingInfo.RefreshBinding();
					Order.MarkAsNeedingValidation();
				}

				Deliveries.MarkAsNeedingValidation();
				foreach (OrderLineDelivery delivery in Deliveries)
				{
					delivery.Containers.MarkAsNeedingValidation();
				}

				JO_QuantityRemainingInfo.RefreshBinding();
			}
		}

		public override ZInt JO_LineNo
		{
			get { return base.JO_LineNo; }
			set
			{
				base.JO_LineNo = value;
				JO_SubLineNo = GetHighestSubLineNoForThisLineNo();
			}
		}

		ZInt GetHighestSubLineNoForThisLineNo()
		{
			ZInt highestSubLineNo = 0;

			if (Order != null)
			{
				ZQuery filter = new ZQuery();
				filter.AddToFilter(JoinCondition.And, JobOrderLineSchema.JO_LineNo, SQLComparisonOperator.Equal, JO_LineNo);
				filter.AddToFilter(JoinCondition.And, JobOrderLineSchema.PK, SQLComparisonOperator.NotEqual, this.PK);

				foreach (OrderLine line in Order.OrderLines.Find(filter))
				{
					if (line.PK != PK && line.JO_SubLineNo > highestSubLineNo)
					{
						highestSubLineNo = line.JO_SubLineNo;
					}
				}
			}

			return highestSubLineNo + 1;
		}

		public override ZInt JO_SubLineNo
		{
			get
			{
				return base.JO_SubLineNo;
			}
			set
			{
				base.JO_SubLineNo = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateJO_LineNo();
				}
			}
		}

		public override ZDecimal JO_Quantity
		{
			get { return base.JO_Quantity; }
			set
			{
				var quantityChange = value - base.JO_Quantity;

				base.JO_Quantity = value;
				JO_OpenQuantity += quantityChange;

				Deliveries.MarkAsNeedingValidation();
				foreach (OrderLineDelivery delivery in Deliveries)
				{
					delivery.Containers.MarkAsNeedingValidation();
				}

				if (!IsCopying && !fIsImportingData)
				{
					ZDecimal newLineValue = CalculateLinePrice(JO_ItemPrice, JO_Quantity);
					if (JO_LinePrice != newLineValue)
					{
						JO_LinePrice = newLineValue;
					}
				}

				if (Order != null)
				{
					Order.JD_Calc_TotalQuantityInfo.RefreshBinding();
					Order.JD_Calc_TotalQuantityRemainingInfo.RefreshBinding();
				}
			}
		}

		public override ZDecimal JO_ItemPrice
		{
			get { return base.JO_ItemPrice; }
			set
			{
				base.JO_ItemPrice = value;
				if (!IsCopying && !fIsImportingData && shouldAutoUpdateLineAndItemPrice)
				{
					ZDecimal newLineValue = CalculateLinePrice(value, JO_Quantity);
					if (JO_LinePrice != newLineValue)
					{
						using (new ShouldAutoUpdateLineAndItemPriceSuspender(this))
						{
							JO_LinePrice = newLineValue;
						}
					}
				}
			}
		}

		public override ZDecimal JO_LinePrice
		{
			get { return base.JO_LinePrice; }
			set
			{
				base.JO_LinePrice = value;
				if (!IsCopying && !fIsImportingData && JO_Quantity > 0 && shouldAutoUpdateLineAndItemPrice)
				{
					ZDecimal newItemValue = CalculateItemPrice(value, JO_Quantity);
					if (JO_ItemPrice != newItemValue)
					{
						using (new ShouldAutoUpdateLineAndItemPriceSuspender(this))
						{
							JO_ItemPrice = newItemValue;
						}
					}
				}
			}
		}

		bool shouldAutoUpdateLineAndItemPrice = true;

		sealed class ShouldAutoUpdateLineAndItemPriceSuspender : IDisposable
		{
			public ShouldAutoUpdateLineAndItemPriceSuspender(OrderLine orderLine)
			{
				this.orderLine = orderLine;
				this.orderLine.shouldAutoUpdateLineAndItemPrice = false;
			}
			readonly OrderLine orderLine;

			public void Dispose()
			{
				orderLine.shouldAutoUpdateLineAndItemPrice = true;
			}
		}

		ZDecimal CalculateLinePrice(ZDecimal itemPrice, ZDecimal quantity)
		{
			return Utilities.Round(itemPrice * quantity, JobOrderLineSchema.JO_LinePrice.Scale);
		}

		ZDecimal CalculateItemPrice(ZDecimal linePrice, ZDecimal quantity)
		{
			return Utilities.Round(linePrice / quantity, JobOrderLineSchema.JO_ItemPrice.Scale);
		}

		public override ZDecimal JO_InnerPacks
		{
			get { return base.JO_InnerPacks; }
			set
			{
				base.JO_InnerPacks = value;
				JO_TotalInnerPacksInfo.RefreshBinding();

				if (Order != null)
				{
					Order.JD_Calc_InnerPacksInfo.RefreshBinding();
				}
			}
		}

		public override ZDecimal JO_OuterPacks
		{
			get { return base.JO_OuterPacks; }
			set
			{
				base.JO_OuterPacks = value;

				if (!IsCopying && !fIsImportingData)
				{
					SetJO_ActualVolume();
				}

				JO_TotalInnerPacksInfo.RefreshBinding();

				if (Order != null)
				{
					Order.JD_Calc_OuterPacksInfo.RefreshBinding();
				}
			}
		}

		[List("JO_Partno_List")]
		public override ZString JO_Partno
		{
			get { return base.JO_Partno; }
			set
			{
				var changed = base.JO_Partno != value;
				base.JO_Partno = value;

				OrgSupplierPart product = this.Product;

				if (!IsCopying && !fIsImportingData && Order != null && product != null)
				{
					if (JO_Description.IsEmpty)
					{
						JO_Description = product.OP_Desc;
					}
					JO_InnerPacks = product.OP_OrderMultipleQty;
					JO_OuterPacks = product.OP_VendorPackQty;
					JO_F3_NKPackType = product.OP_StockKeepingUnit;

					UNDGs.DeleteAll();
					foreach (UNDGDataItem undg in product.UNDGs)
					{
						if (undg.Substance != null)
						{
							UNDGDataItem dgItem = UNDGs.AddNew();
							dgItem.DI_DG = undg.DI_DG;
							dgItem.DI_DGFlashPoint = undg.DI_DGFlashPoint;
							dgItem.DI_OC_DGContact = undg.DI_OC_DGContact;
						}
					}
				}

				if (changed)
				{
					SetDefaultTolerances();
				}
			}
		}

		public override ZDecimal JO_QtyInvoiced
		{
			get { return base.JO_QtyInvoiced; }
			set
			{
				base.JO_QtyInvoiced = value;
				if (OrdersDataRegistry.Instance.OrderLineQtyRemainingManagement.Value)
				{
					JO_QtyReceived = value;
				}

				if (Order != null)
				{
					Order.JD_Calc_TotalQuantityInvoicedInfo.RefreshBinding();
					Order.JD_Calc_TotalQuantityRemainingInfo.RefreshBinding();
				}
			}
		}

		protected bool JO_LineStatus_ReadOnly
		{
			get { return !OrderLineStatusEditable; }
		}

		bool OrderLineStatusEditable
		{
			get { return orderLineStatusEditable ?? (bool)(orderLineStatusEditable = OrdersDataRegistry.Instance.OrderLineStatusEditable.Value); }
		}
		bool? orderLineStatusEditable;

		public override ZString JO_ConfirmationNum
		{
			get { return base.JO_ConfirmationNum; }
			set
			{
				base.JO_ConfirmationNum = value;
				if (JO_ConfirmationDate.IsEmpty && !value.IsEmpty)
				{
					JO_ConfirmationDate = ZDateTime.Now;
				}
			}
		}

		public override ZDateTime JO_ConfirmationDate
		{
			get { return base.JO_ConfirmationDate; }
			set
			{
				base.JO_ConfirmationDate = value;

				if (Order != null)
				{
					Order.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region Manufacturer

		[List("Lookups.OrgHeader_List")]
		public ZString ManufacturerNameOrPK
		{
			get { return ManufacturerAddress.OrganisationNameOrPK; }
			set { ManufacturerAddress.OrganisationNameOrPK = value; }
		}

		public ZPropertyInfo ManufacturerNameOrPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ManufacturerNameOrPK, x => ManufacturerAddress.OrganisationNameOrPKInfo); }
		}

		public JobDocAddress ManufacturerAddress
		{
			get
			{
				if (BusinessObject.IsNullOrDeleted(fManufacturerAddress))
				{
					fManufacturerAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.Manufacturer);
					fManufacturerAddress.HasChangesChanged += ManufacturerAddressChanged; // Mark as needing validation on changes 
				}

				return fManufacturerAddress;
			}
		}
		JobDocAddress fManufacturerAddress;

		protected virtual void ManufacturerAddressChanged(object sender, EventArgs e)
		{
			MarkAsNeedingValidation();
		}

		[MaxLength(3)]
		public ZString ManufacturerFieldType
		{
			get
			{
				return ManufacturerAddress.E2_AddressOverride ?
					nameof(FieldType.Text) :
					nameof(FieldType.OrganisationGuid);
			}
		}

		public ZPropertyInfo ManufacturerFieldTypeInfo
		{
			get { return GetZPropertyInfo(nameof(ManufacturerFieldType)); }
		}

		#endregion

		#region GoodsAvailableAt

		[List("Lookups.OrgHeader_List")]
		public ZString GoodsAvailableAtNameOrPK
		{
			get { return GoodsAvailableAtAddress.OrganisationNameOrPK; }
			set { GoodsAvailableAtAddress.OrganisationNameOrPK = value; }
		}

		public ZPropertyInfo GoodsAvailableAtNameOrPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.GoodsAvailableAtNameOrPK, x => GoodsAvailableAtAddress.OrganisationNameOrPKInfo); }
		}

		public JobDocAddress GoodsAvailableAtAddress
		{
			get
			{
				if (BusinessObject.IsNullOrDeleted(fGoodsAvailableAtAddress))
				{
					fGoodsAvailableAtAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.GoodsAvailableAt);
					fGoodsAvailableAtAddress.HasChangesChanged += GoodsAvailableAtAddressChanged; // Mark as needing validation on changes 
				}

				return fGoodsAvailableAtAddress;
			}
		}
		JobDocAddress fGoodsAvailableAtAddress;

		protected virtual void GoodsAvailableAtAddressChanged(object sender, EventArgs e)
		{
			MarkAsNeedingValidation();
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

		public ZPropertyInfo GoodsAvailableAtFieldTypeInfo
		{
			get { return GetZPropertyInfo(nameof(GoodsAvailableAtFieldType)); }
		}

		#endregion

		#region GoodsDeliveredTo

		[List("Lookups.OrgHeader_List")]
		public ZString GoodsDeliveredToNameOrPK
		{
			get { return GoodsDeliveredToAddress.OrganisationNameOrPK; }
			set { GoodsDeliveredToAddress.OrganisationNameOrPK = value; }
		}

		public ZPropertyInfo GoodsDeliveredToNameOrPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.GoodsDeliveredToNameOrPK, x => GoodsDeliveredToAddress.OrganisationNameOrPKInfo); }
		}

		public JobDocAddress GoodsDeliveredToAddress
		{
			get
			{
				if (BusinessObject.IsNullOrDeleted(fGoodsDeliveredToAddress))
				{
					fGoodsDeliveredToAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.GoodsDeliveredTo);
					fGoodsDeliveredToAddress.HasChangesChanged += GoodsDeliveredToAddressChanged; // Mark as needing validation on changes 
				}

				return fGoodsDeliveredToAddress;
			}
		}
		JobDocAddress fGoodsDeliveredToAddress;

		protected virtual void GoodsDeliveredToAddressChanged(object sender, EventArgs e)
		{
			MarkAsNeedingValidation();
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

		public ZPropertyInfo GoodsDeliveredToFieldTypeInfo
		{
			get { return GetZPropertyInfo(nameof(GoodsDeliveredToFieldType)); }
		}

		#endregion

		#region ConsigneeDocumentary

		[List("Lookups.OrgHeader_List")]
		public ZString ConsigneeDocumentaryNameOrPK
		{
			get { return ConsigneeDocumentaryAddress.OrganisationNameOrPK; }
			set { ConsigneeDocumentaryAddress.OrganisationNameOrPK = value; }
		}

		public ZPropertyInfo ConsigneeDocumentaryNameOrPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ConsigneeDocumentaryNameOrPK, x => ConsigneeDocumentaryAddress.OrganisationNameOrPKInfo); }
		}

		public JobDocAddress ConsigneeDocumentaryAddress
		{
			get
			{
				if (BusinessObject.IsNullOrDeleted(fConsigneeDocumentaryAddress))
				{
					fConsigneeDocumentaryAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ConsigneeDocumentaryAddress);
					fConsigneeDocumentaryAddress.HasChangesChanged += ConsigneeDocumentaryAddressChanged; // Mark as needing validation on changes 
				}

				return fConsigneeDocumentaryAddress;
			}
		}
		JobDocAddress fConsigneeDocumentaryAddress;

		protected virtual void ConsigneeDocumentaryAddressChanged(object sender, EventArgs e)
		{
			MarkAsNeedingValidation();
		}

		[MaxLength(3)]
		public ZString ConsigneeDocumentaryFieldType
		{
			get
			{
				return ConsigneeDocumentaryAddress.E2_AddressOverride ?
					nameof(FieldType.Text) :
					nameof(FieldType.OrganisationGuid);
			}
		}

		public ZPropertyInfo ConsigneeDocumentaryFieldTypeInfo
		{
			get { return GetZPropertyInfo(nameof(ConsigneeDocumentaryFieldType)); }
		}

		#endregion

		#region Apportionment

		void MarkApportionmentDirty()
		{
			if (Order != null)
			{
				((IApportionInvoiceHolder)Order).MarkApportionmentDirty();
			}
		}

		protected void OnApportionmentBeingDirty(object sender, EventArgs e)
		{
			MarkApportionmentDirty();
		}

		event EventHandler ApportionmentDirtyChangedEventHandler
		{
			add
			{
				JO_ActualVolumeInfo.ValueChanged += value;
				JO_ActualWeightInfo.ValueChanged += value;
				JO_UnitOfVolumeInfo.ValueChanged += value;
				JO_UnitOfWeightInfo.ValueChanged += value;
				JO_LinePriceInfo.ValueChanged += value;
				JO_QuantityInfo.ValueChanged += value;
			}
			remove
			{
				JO_ActualVolumeInfo.ValueChanged -= value;
				JO_ActualWeightInfo.ValueChanged -= value;
				JO_UnitOfVolumeInfo.ValueChanged -= value;
				JO_UnitOfWeightInfo.ValueChanged -= value;
				JO_LinePriceInfo.ValueChanged -= value;
				JO_QuantityInfo.ValueChanged -= value;
			}
		}

		#endregion

		#region Calculated Properties

		#region JO_Calc_OrderLineNoAndSubLineNo

		public ZString JO_Calc_OrderLineNoAndSubLineNo
		{
			get
			{
				ZString result = JO_LineNo.ToString();
				if (GetHighestSubLineNoForThisLineNo() != 1)
				{
					result += " - " + JO_SubLineNo.ToString();
				}

				return result;
			}
		}

		public ZPropertyInfo JO_Calc_OrderLineNoAndSubLineNoInfo
		{
			get { return GetZPropertyInfo(Schema.JO_Calc_OrderLineNoAndSubLineNo); }
		}

		#endregion

		#region JO_QuantityRemaining

		[DecimalPlaces(5)]
		public ZDecimal JO_QuantityRemaining
		{
			get
			{
				ZDecimal valueToBeBasedOn = OrdersDataRegistry.Instance.OrderLineQtyRemainingManagement.Value ? JO_QtyReceived : JO_QtyInvoiced;
				ZDecimal result = JO_Quantity - valueToBeBasedOn;
				return result < 0 ? 0 : result;
			}
		}

		public ZPropertyInfo JO_QuantityRemainingInfo
		{
			get { return GetZPropertyInfo(Schema.JO_QuantityRemaining); }
		}

		#endregion

		#region JO_TotalInnerPacks

		[DecimalPlaces(0)]
		public ZDecimal JO_TotalInnerPacks
		{
			get { return JO_OuterPacks * JO_InnerPacks; }
		}

		public ZPropertyInfo JO_TotalInnerPacksInfo
		{
			get { return GetZPropertyInfo(nameof(JO_TotalInnerPacks)); }
		}

		#endregion

		#region JO_Calc_TotalQtyInvoiced

		[DecimalPlaces(5)]
		public ZDecimal JO_Calc_TotalQtyInvoiced
		{
			get
			{
				ZDecimal result = 0m;
				foreach (OrderLineDelivery delivery in Deliveries)
				{
					foreach (OrderLineDeliverContainer container in delivery.Containers)
					{
						result += container.J5_QuantityInvoiced;
					}
				}
				return result;
			}
		}

		public ZPropertyInfo JO_Calc_TotalQtyInvoicedInfo
		{
			get { return GetZPropertyInfo(nameof(JO_Calc_TotalQtyInvoiced)); }
		}

		#endregion

		#region JO_LineNoAndSplitAndSubLine

		public ZString JO_LineNoAndSplitAndSubLine
		{
			get
			{
				string result = JO_LineNo.ToString();
				if (JO_LineSplitNumber != 0)
				{
					result += "." + JO_LineSplitNumber;
				}
				if (JO_SubLineNo != 1)
				{
					result += " " + Res.GetString("3cc6e172-06be-4258-a9e4-6ddc9bc285fa", "sub {0}", JO_SubLineNo);
				}
				return result;
			}
		}

		public ZPropertyInfo JO_LineNoAndSplitAndSubLineInfo
		{
			get { return GetZPropertyInfo(nameof(JO_LineNoAndSplitAndSubLine)); }
		}

		#endregion

		#region JO_Calc_TotalQtyReceived

		[DecimalPlaces(5)]
		public ZDecimal JO_Calc_TotalQtyReceived
		{
			get
			{
				ZDecimal result = 0;
				foreach (OrderLineDelivery delivery in Deliveries)
				{
					result += delivery.J4_Allocated;
				}
				return result;
			}
		}

		public ZPropertyInfo JO_Calc_TotalQtyReceivedInfo
		{
			get { return GetZPropertyInfo(nameof(JO_Calc_TotalQtyReceived)); }
		}

		#endregion

		#region JO_ContainersVisible

		public ZBool JO_ContainersVisible
		{
			get
			{
				ZBool result = false;
				if (Order != null && Order.BuyerPK.IsValid)
				{
					result = Env.Registry.GetOrderLineContainersVisible(Order.BuyerPK.ToGuid());
				}
				return result;
			}
			set
			{
				if (Order != null && Order.BuyerPK.IsValid)
				{
					Env.Registry.SetOrderLineContainersVisible(Order.BuyerPK.ToGuid(), value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateJO_Quantity();
					}
					JO_ContainersVisibleInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo JO_ContainersVisibleInfo
		{
			get { return GetZPropertyInfo(nameof(JO_ContainersVisible)); }
		}

		#endregion

		#region QtyReceivedToDate

		/// <summary>
		/// Returns the total of the Quantity Received field for all order lines 
		/// across this order and all order splits where the line number is the same as this line's number.
		/// All lines must have the same 'Quantity Unit' - otherwise 0 is returned.
		/// </summary>
		[DecimalPlaces(5)]
		public ZDecimal QtyReceivedToDate
		{
			get
			{
				ZDecimal result = 0m;

				List<Order> orderIncludingSplits = new List<Order>();
				orderIncludingSplits.Add(Order);
				Order.RefreshOrderSplitSiblings();
				foreach (Order splitOrder in Order.OrderSplitSiblings)
				{
					orderIncludingSplits.Add(splitOrder);
				}

				foreach (Order relevantOrder in orderIncludingSplits)
				{
					foreach (OrderLine line in relevantOrder.OrderLines)
					{
						if (line.JO_LineNo == JO_LineNo)
						{
							if (line.JO_F3_NKPackType != JO_F3_NKPackType)
							{
								return 0m;
							}
							result += line.JO_QtyReceived;
						}
					}
				}

				return result;
			}
		}

		public ZPropertyInfo QtyReceivedToDateInfo
		{
			get { return GetZPropertyInfo(nameof(QtyReceivedToDate)); }
		}

		#endregion

		#region Line Price

		/// <summary>
		/// This should return in a local currency converted by a currency converter in Order.
		/// The exchange rate is user-entered and this should not be returned in an order currency to be consumed by any converter which might use a standard ex-rate.
		/// </summary>
		public Money LinePriceMoney
		{
			get
			{
				Money result = Money.Empty;

				if (Order.OrderCurrency != null)
				{
					result = ((ICommonInvoice)Order).CurrencyConverter.ConvertExact(new Money(JO_LinePrice, Order.OrderCurrency), Order.Company.LocalCurrency);
				}

				return result;
			}
		}

		internal ZDecimal CustomsValue
		{
			get
			{
				ZDecimal result = LinePriceMoney.Amount;

				RefCurrency localCurrency = Order.Company.LocalCurrency;
				result += Charges.AmountToAddToITOTForDutiableCharges(localCurrency);
				result += ApportionedCharges.AmountToAddToITOTForDutiableCharges(localCurrency);

				return result;
			}
		}

		#endregion

		#region JO_QtyBooked

		[DecimalPlaces(5)]
		public ZDecimal JO_QtyBooked => JO_Quantity - JO_OpenQuantity;

		public ZPropertyInfo JO_QtyBookedInfo => GetZPropertyInfo(nameof(JO_QtyBooked));

		#endregion

		#endregion

		#region Lookups

		#region Container

		public CodeDescriptionPairList ContainerNumbersList
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				if (Order != null)
				{
					if (Order.Shipment != null && Order.Shipment.Consols.Count > 0)
					{
						foreach (ForwardingConsol consol in Order.Shipment.Consols)
						{
							foreach (CommonContainer currentShipmentContainer in consol.Containers)
							{
								string description = "";
								if (currentShipmentContainer.Container != null)
								{
									description = currentShipmentContainer.Container.RC_Code;
								}
								list.AddPair(currentShipmentContainer.JC_ContainerNum, description);
							}
						}
					}

					foreach (OrderContainer currentOrderContainer in Order.PlannedContainers)
					{
						string description = "";
						if (currentOrderContainer.Container != null)
						{
							description = currentOrderContainer.Container.RC_Code;
						}

						if (!list.ContainsCode(currentOrderContainer.J1_ContainerNumber))
						{
							list.AddPair(currentOrderContainer.J1_ContainerNumber, description);
						}
					}
				}
				return list;
			}
		}

		#endregion

		#region JO_Partno_List

		public virtual OrgSupplierPartCollection JO_Partno_List
		{
			get
			{
				OrgSupplierPartCollection result;
				if (Order != null)
				{
					result = new OrdersOrgSupplierPartCollection(Factory, Order.Supplier, Order.Buyer);
				}
				else
				{
					result = new OrdersOrgSupplierPartCollection(Factory);
				}
				return result;
			}
		}

		#endregion

		#region JO_LineStatus_List

		public CodeDescriptionPairList JO_LineStatus_List
		{
			get
			{
				if (fJO_LineStatus_List == null || (Order != null && Order.BuyerPK != LineStatusBuyer))
				{
					fJO_LineStatus_List = new CodeDescriptionPairList(OLookUpEditType.OrderLineStatus);

					fJO_LineStatus_List.AddRange(Env.Registry.OrderLineStatusList);

					if (Order != null && Order.Buyer != null && Order.Buyer.MiscServ.OrderLineStatusList != null)
					{
						fJO_LineStatus_List.AddRange(new CodeDescriptionPairList(Order.Buyer.MiscServ.OrderLineStatusList));
					}
					LineStatusBuyer = Order != null ? Order.BuyerPK : ZGuid.Empty;
					if (GlbBranch.CurrentBranch.OrgProxy != null &&
						GlbBranch.CurrentBranch.OrgProxy.MiscServ.OrderLineStatusList != null)
					{
						fJO_LineStatus_List.AddRange(new CodeDescriptionPairList(GlbBranch.CurrentBranch.OrgProxy.MiscServ.OrderLineStatusList));
					}
				}
				return fJO_LineStatus_List;
			}
		}
		CodeDescriptionPairList fJO_LineStatus_List;
		ZGuid LineStatusBuyer;

		#endregion

		#region JO_INCO_List

		public CodeDescriptionPairList JO_INCO_List
		{
			get { return new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms); }
		}

		#endregion

		#region JO_F3_NKPackType_List

		public CodeDescriptionPairList JO_F3_NKPackType_List
		{
			get { return new RefPackTypeCollection(Factory).GetAsCodeDescriptionPairWithStandardUnits(); }
		}

		#endregion

		#region WeightUnit_List

		public CodeDescriptionPairList WeightUnit_List
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		#endregion

		#region VolumeUnit_List

		public CodeDescriptionPairList VolumeUnit_List
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Volume); }
		}

		#endregion

		#endregion

		#region Receive All

		public void ReceiveAllIfNonReceived()
		{
			JO_QtyReceived = JO_Quantity;
			JO_QtyInvoiced = JO_Quantity;
		}

		#endregion

		#region SetDefaultTolerances

		public void SetDefaultTolerances()
		{
			if (!OrdersDataRegistry.Instance.EnableOrderLineShippingTolerance.Value)
			{
				return;
			}

			var supplierBuyerLinkTolerance = GetExistingOrgSupplierBuyerLinkTolerance();
			JO_UnderQuantityPercentageLimit = supplierBuyerLinkTolerance?.OLT_UnderQuantityPercentageLimit ?? 0;
			JO_OverQuantityPercentageLimit = supplierBuyerLinkTolerance?.OLT_OverQuantityPercentageLimit ?? 0;
			JO_EarlyShipmentLimitDays = supplierBuyerLinkTolerance?.OLT_EarlyShipmentLimitDays ?? 0;
			JO_LateShipmentLimitDays = supplierBuyerLinkTolerance?.OLT_LateShipmentLimitDays ?? 0;
		}

		IOrgSupplierBuyerLinkTolerance GetExistingOrgSupplierBuyerLinkTolerance()
		{
			if (Order == null)
			{
				return null;
			}

			var supplierBuyerLink = Order.SupplierBuyerLinkFromBuyerAddressCountry;
			return OrgSupplierBuyerLinkTolerance.GetExistingOrgSupplierBuyerLinkTolerance(supplierBuyerLink, Order.JD_TransportMode, JO_Partno);
		}

		#endregion

		#region IWorkflowTriggerFieldChangeSource Members

		IReadOnlyList<IWorkflowProvider> IWorkflowTriggerFieldChangeSource.ParentWorkflowProviders
		{
			get
			{
				List<IWorkflowProvider> result = new List<IWorkflowProvider>();
				if (Order != null)
				{
					result.Add(Order);
					result.AddRange(((IWorkflowTriggerFieldChangeSource)Order).ParentWorkflowProviders);
				}
				return result.ToArray();
			}
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
			set
			{
				customLabelsProviderType.Value = value;
			}
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

			[SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "no need to be translated")]
			public virtual CustomLabelInfoList GetCustomFields(OrgHeader configOrg, BusinessObjectFactory factory)
			{
				CustomLabelInfoList result = new CustomLabelInfoList(typeof(OrderLine), configOrg, ResString.GetMultilingualString("effa04cd-4928-410a-aeac-4c87aab82121", "the buyer of the order"), factory);
				result.Add(Constants.CustomLabels.OrderLine.CustomAttribute1, Schema.JO_CustomAttrib1, Constants.CustomLabels.Descriptions.CustomAttribute(1));
				result.Add(Constants.CustomLabels.OrderLine.CustomAttribute2, Schema.JO_CustomAttrib2, Constants.CustomLabels.Descriptions.CustomAttribute(2));
				result.Add(Constants.CustomLabels.OrderLine.CustomAttribute3, Schema.JO_CustomAttrib3, Constants.CustomLabels.Descriptions.CustomAttribute(3));
				result.Add(Constants.CustomLabels.OrderLine.CustomAttribute4, Schema.JO_CustomAttrib4, Constants.CustomLabels.Descriptions.CustomAttribute(4));
				result.Add(Constants.CustomLabels.OrderLine.CustomAttribute5, Schema.JO_CustomAttrib5, Constants.CustomLabels.Descriptions.CustomAttribute(5));
				result.Add(Constants.CustomLabels.OrderLine.CustomAttribute6, Schema.JO_CustomAttrib6, Constants.CustomLabels.Descriptions.CustomAttribute(6));
				result.Add(Constants.CustomLabels.OrderLine.CustomText1, Schema.JO_CustomTextBlob1, Constants.CustomLabels.Descriptions.CustomText(1), CustomLabelStyles.MultiLineTextBox);
				result.Add(Constants.CustomLabels.OrderLine.CustomFlag1, Schema.JO_CustomFlag1, Constants.CustomLabels.Descriptions.CustomFlag(1));
				result.Add(Constants.CustomLabels.OrderLine.CustomFlag2, Schema.JO_CustomFlag2, Constants.CustomLabels.Descriptions.CustomFlag(2));
				result.Add(Constants.CustomLabels.OrderLine.CustomFlag3, Schema.JO_CustomFlag3, Constants.CustomLabels.Descriptions.CustomFlag(3));
				result.Add(Constants.CustomLabels.OrderLine.CustomFlag4, Schema.JO_CustomFlag4, Constants.CustomLabels.Descriptions.CustomFlag(4));
				result.Add(Constants.CustomLabels.OrderLine.CustomFlag5, Schema.JO_CustomFlag5, Constants.CustomLabels.Descriptions.CustomFlag(5));
				result.Add(Constants.CustomLabels.OrderLine.CustomDate1, Schema.JO_CustomDate1, Constants.CustomLabels.Descriptions.CustomDate(1));
				result.Add(Constants.CustomLabels.OrderLine.CustomDate2, Schema.JO_CustomDate2, Constants.CustomLabels.Descriptions.CustomDate(2));
				result.Add(Constants.CustomLabels.OrderLine.CustomDate3, Schema.JO_CustomDate3, Constants.CustomLabels.Descriptions.CustomDate(3));
				result.Add(Constants.CustomLabels.OrderLine.CustomDate4, Schema.JO_CustomDate4, Constants.CustomLabels.Descriptions.CustomDate(4));
				result.Add(Constants.CustomLabels.OrderLine.CustomDate5, Schema.JO_CustomDate5, Constants.CustomLabels.Descriptions.CustomDate(5));
				result.Add(Constants.CustomLabels.OrderLine.CustomDecimal1, Schema.JO_CustomDecimal1, Constants.CustomLabels.Descriptions.CustomNumber(1));
				result.Add(Constants.CustomLabels.OrderLine.CustomDecimal2, Schema.JO_CustomDecimal2, Constants.CustomLabels.Descriptions.CustomNumber(2));
				result.Add(Constants.CustomLabels.OrderLine.CustomDecimal3, Schema.JO_CustomDecimal3, Constants.CustomLabels.Descriptions.CustomNumber(3));
				result.Add(Constants.CustomLabels.OrderLine.CustomDecimal4, Schema.JO_CustomDecimal4, Constants.CustomLabels.Descriptions.CustomNumber(4));
				result.Add(Constants.CustomLabels.OrderLine.CustomDecimal5, Schema.JO_CustomDecimal5, Constants.CustomLabels.Descriptions.CustomNumber(5));

				if (configOrg != null && configOrg.MiscServ != null)
				{
					result.Add(new PartCustomLabelInfo(configOrg.MiscServ.OM_IMPartAttrib1Name, configOrg.MiscServ.OM_IMPartAttrib1Type, Schema.JO_PartAttrib1, typeof(ZString), ResString.GetMultilingualString("02ec7a30-dff5-44f4-ada2-33d8a63e5775", "Part Attribute 1"), configOrg, factory));
					result.Add(new PartCustomLabelInfo(configOrg.MiscServ.OM_IMPartAttrib2Name, configOrg.MiscServ.OM_IMPartAttrib2Type, Schema.JO_PartAttrib2, typeof(ZString), ResString.GetMultilingualString("ac09330a-04f8-4a44-8dcb-989081970cf1", "Part Attribute 2"), configOrg, factory));
					result.Add(new PartCustomLabelInfo(configOrg.MiscServ.OM_IMPartAttrib3Name, configOrg.MiscServ.OM_IMPartAttrib3Type, Schema.JO_PartAttrib3, typeof(ZString), ResString.GetMultilingualString("e726db1d-0f15-412c-a60f-50d2f8c2b1ec", "Part Attribute 3"), configOrg, factory));
				}

				return result;
			}

			protected ICustomLabelsConfigOrgProvider fConfigOrgProvider;
		}

		#endregion

		#region ICustomLabelsConfigOrgProvider

		event EventHandler ICustomLabelsConfigOrgProvider.ConfigOrgChanged
		{
			add
			{
				if (Order != null)
				{
					Order.JD_OA_BuyerAddressInfo.ValueChanged += value;
					Order.BuyerPKInfo.ValueChanged += value;

					if (Order.ControllingCustomerDocAddress != null)
					{
						Order.ControllingCustomerDocAddress.E2_OA_AddressInfo.ValueChanged += value;
					}
				}
			}

			remove
			{
				if (Order != null)
				{
					Order.JD_OA_BuyerAddressInfo.ValueChanged -= value;
					Order.BuyerPKInfo.ValueChanged -= value;

					if (Order.ControllingCustomerDocAddress != null)
					{
						Order.ControllingCustomerDocAddress.E2_OA_AddressInfo.ValueChanged -= value;
					}
				}
			}
		}

		OrgHeader ICustomLabelsConfigOrgProvider.ConfigOrg
		{
			get
			{
				if (IsDeleted || Order == null)
				{
					return null;
				}

				var workflowType = ((IWorkflowProvider)Order).WorkflowType;
				var collection = WorkflowDataRegistry.Instance.ClientInTemplateSelection.Value;
				var entry = collection.GetValueByCode(workflowType);

				if (entry != null)
				{
					foreach (ClientInTemplateSelectionCriteriaOrgType orgType in entry.SelectedItems)
					{
						if (orgType.OrgTypeCode == WorkflowSelectionOrgTypeCodes.ControllingCustomer
							&& Order.DocAddresses.ContainsDocAddressType(DocAddressType.ControllingCustomer))
						{
							var controllingCustomerOrg = Order?.ControllingCustomerDocAddress?.Organisation;
							if (controllingCustomerOrg != null && !Order.ControllingCustomerDocAddress.E2_AddressOverride)
							{
								return controllingCustomerOrg;
							}
						}
						else if (orgType.OrgTypeCode == WorkflowSelectionOrgTypeCodes.BuyerSupplier)
						{
							return Order?.Buyer;
						}
					}
				}

				return null;
			}
		}

		#endregion

		#region IChargeApportionee Members

		[ChildEditable(true)]
		public JobComInvApportionedChargeCollection<JobComInvCharge> ApportionedCharges
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
		JobComInvApportionedChargeCollection<JobComInvCharge> apportionedCharges;

		IJobComInvApportionedChargeCollection<Customs.Common.JobComInvCharge> IChargeApportionee.ApportionedCharges
		{
			get { return ApportionedCharges; }
		}

		bool IChargeApportionee.CanThisChargeBeApportionedBasedOnIncoterm(ApportionChargeKey apportionChargeKey)
		{
			return true;
		}

		ZDecimal IChargeApportionee.GetBaseValueToApportionOn(CurrencyConverter currencyConverter, string distributeBy)
		{
			ZDecimal result = 0;
			switch (distributeBy)
			{
				case ChargeDistributeByList.Codes.Value:
					if (Order.OrderCurrency != null)
					{
						result = currencyConverter.ConvertExact(new Money(JO_LinePrice, Order.OrderCurrency), GlbCompany.CurrentCompany.LocalCurrency).Amount;
					}
					break;

				case ChargeDistributeByList.Codes.Volume:
					if (Core.Constants.Volume.ContainsCode(JO_UnitOfVolume))
					{
						result = Core.Constants.Volume.Convert(JO_ActualVolume, JO_UnitOfVolume, Core.Constants.Volume.CubicMetres);
					}
					break;

				case ChargeDistributeByList.Codes.Weight:
					if (Core.Constants.Weight.ContainsCode(JO_UnitOfWeight))
					{
						result = Core.Constants.Weight.Convert(JO_ActualWeight, JO_UnitOfWeight, Core.Constants.Weight.Kilograms);
					}
					break;
				case ChargeDistributeByList.Codes.Quantity:
					result = JO_Quantity;
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
			if (Order.OrderCurrency != null)
			{
				charge.J7_Amount = charge.J7_Percentage * JO_LinePrice;
				charge.J7_RX_NKCurrency = Order.JD_RX_NKOrderCurrency;
			}
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
			get { return ((ICommonInvoice)Order).CurrencyConverter; }
		}

		ICommonInvoice ICommonInvoice.ImmediateCommonInvoiceParent
		{
			get { return Order; }
		}

		ZString ICommonInvoice.IncoTerm
		{
			get { return ((ICommonInvoice)Order).IncoTerm; }
		}

		IApportionInvoiceHolder ICommonInvoice.InvoicesHolder
		{
			get { return Order; }
		}

		ZString ICommonInvoice.LocalCurrencyCode
		{
			get { return ((ICommonInvoice)Order).LocalCurrencyCode; }
		}

		bool ICommonInvoice.HasMultipleInvoiceUQs
		{
			get { return false; }
		}

		#endregion

		#region IChargeHolder Members

		[ChildEditable(true)]
		public JobComInvChargeCollection Charges
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
		JobComInvChargeCollection charges;

		IChargeApportionee[] IChargeHolder.AllApportionees
		{
			get { return Array.Empty<IChargeApportionee>(); }
		}

		IJobComInvChargeCollection<Customs.Common.JobComInvCharge> IChargeHolder.Charges
		{
			get { return Charges; }
		}

		CurrencyConverter IChargeHolder.CurrencyConverter
		{
			get { return ((IChargeHolder)Order).CurrencyConverter; }
		}

		IChargeHolder[] IChargeHolder.ImmediateChargeHolderChildren
		{
			get { return Array.Empty<IChargeHolder>(); }
		}

		IChargeHolder IChargeHolder.ImmediateChargeHolderParent
		{
			get { return Order; }
		}

		bool IChargeHolder.IsGroupInvoice
		{
			get { return false; }
		}

		ZString IChargeHolder.GetDefaultCurrencyCode(ICustomsChargeCode chargeType, Customs.Common.JobComInvCharge charge)
		{
			return Order != null ? ((IChargeHolder)Order).GetDefaultCurrencyCode(chargeType, charge) : ZString.Empty;
		}

		ZString IChargeHolder.GetDefaultDistributeBy() => ZString.Empty;

		#endregion

		#region ILandedCostDistributeTo Members

		ZString ILandedCostDistributeTo.Description
		{
			get { return Res.GetString("03a8f420-7acd-44fb-a666-8135c73b5270", "Order Line"); }
		}

		ZString ILandedCostDistributeTo.TableCode
		{
			get { return JobOrderLineSchema.Constants.Prefix; }
		}

		IEnumerable<IUltimateDistributee> ILandedCostDistributeTo.UltimateDistributees
		{
			get { yield return this; }
		}

		ZString ILandedCostDistributeTo.UniqueCode
		{
			get { return (NoResString)"Line " + JO_LineNo.ToString(); }// identifier
		}

		#endregion

		#region IUltimateDistributee Members

		ZBool IUltimateDistributee.IsCapableOfCalculatingOwnGstVatRate { get { return false; } }
		ZDecimal IUltimateDistributee.CalculateOwnGstVatRate() { return 0m; }

		IEnumerable<ICustomsFee> IUltimateDistributee.Fees
		{
			get
			{
				return Array.Empty<ICustomsFee>();
			}
		}

		ZDecimal IUltimateDistributee.Actual
		{
			get
			{
				ZDecimal result = 0m;
				if (Order.IsAirTransport)
				{
					result = ((IUltimateDistributee)this).ActualWeightInKG;
				}
				else if (Order.IsSeaTransport)
				{
					result = ((IUltimateDistributee)this).ActualVolumeInM3;
				}
				return result;
			}
		}

		ZDecimal IUltimateDistributee.ActualVolumeInM3
		{
			get
			{
				return Core.Constants.Volume.ContainsCode(JO_UnitOfVolume) ? Core.Constants.Volume.Convert(JO_ActualVolume, JO_UnitOfVolume, Core.Constants.Volume.CubicMetres) : 0m;
			}
		}

		ZDecimal IUltimateDistributee.ActualWeightInKG
		{
			get
			{
				return Core.Constants.Weight.ContainsCode(JO_UnitOfWeight) ? Core.Constants.Weight.Convert(JO_ActualWeight, JO_UnitOfWeight, Core.Constants.Weight.Kilograms) : 0m;
			}
		}

		ZDecimal IUltimateDistributee.CostInLocalCurrency
		{
			get { return LinePriceMoney.Amount; }
		}

		ZString IUltimateDistributee.CountryOfOriginCode
		{
			get { return Order.JD_RN_NKCountryOfSupply; }
		}

		ZDecimal IUltimateDistributee.CustomsValue
		{
			get { return CustomsValue; }
		}

		ZDecimal IUltimateDistributee.DutyPercent
		{
			get
			{
				DutyResult dutyResult = Order.DutyFeeCalculationManager.GetDutyResult(PK);
				return dutyResult.Percent;
			}
		}

		ZDecimal IUltimateDistributee.GSTVATAmount
		{
			get
			{
				return Order.DutyFeeCalculationManager.GetFeeResult(PK, Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount)
					+ Order.DutyFeeCalculationManager.GetFeeResult(PK, Core.Constants.Customs.CusEntryFeeTypes.GSTVATDeferred);
			}
		}

		ZString IUltimateDistributee.DutyRateDescription
		{
			get
			{
				DutyResult dutyResult = Order.DutyFeeCalculationManager.GetDutyResult(PK);
				return dutyResult.DutyRateDescription;
			}
		}

		ZDecimal IUltimateDistributee.CustomsQuantity
		{
			get
			{
				ICustomsDetails customsDetails = Order.DutyFeeCalculationManager.GetCustomsDetailsFor(this);

				return customsDetails != null ? customsDetails.CustomsQuantity : ZDecimal.Zero;
			}
		}

		ZString IUltimateDistributee.CustomsUQ
		{
			get
			{
				ICustomsDetails customsDetails = Order.DutyFeeCalculationManager.GetCustomsDetailsFor(this);

				return customsDetails != null ? customsDetails.CustomsUQ : ZString.Empty;
			}
		}

		ZString IUltimateDistributee.TariffNumber
		{
			get
			{
				ICustomsDetails customsDetails = Order.DutyFeeCalculationManager.GetCustomsDetailsFor(this);

				return customsDetails != null ? customsDetails.TariffNumber : ZString.Empty;
			}
		}

		ZGuid IUltimateDistributee.FKToProduct
		{
			get { return Product != null ? Product.PK : ZGuid.Empty; }
		}

		LCMarginPercentages IUltimateDistributee.GetProductSpecificLCMarginPercentagesForFallBack(OrgHeader consignee)
		{
			OrgSupplierPart part = Product;

			LCMarginPercentages result = null;
			if (part != null)
			{
				result = part.RelatedOrganisations.GetLCMarginPercentagesForFallBack(consignee);
			}
			else
			{
				result = new LCMarginPercentages();
			}
			return result;
		}

		ZString IUltimateDistributee.HumanReadableCode
		{
			get { return JO_LineNo.ToString(); }
		}

		ZPropertyInfo[] IUltimateDistributee.HumanReadableCodeInfos
		{
			get { return new ZPropertyInfo[] { JO_LineNoInfo }; }
		}

		ZString IUltimateDistributee.InvoiceCurrencyCode
		{
			get { return Order.JD_RX_NKOrderCurrency; }
		}

		ZString IUltimateDistributee.InvoiceNumber
		{
			get { return JO_CommercialInvoiceNo; }
		}

		ZString IUltimateDistributee.InvoiceUQ
		{
			get { return JO_F3_NKPackType; }
		}

		ZDecimal IUltimateDistributee.ItemCount
		{
			get { return JO_Quantity; }
		}

		ZDecimal IUltimateDistributee.LandedCostingExRateFallingBackToJobInvoicingForInvoiceCurrency
		{
			get { return Order.JD_EstimatedExchangeRate; }
		}

		ZString IUltimateDistributee.LineDescription
		{
			get { return JO_Description; }
		}

		DutyTaxEntryFee IUltimateDistributee.LineDutyTaxEntryFeeItems
		{
			get { return Order.DutyFeeCalculationManager.GetDutyTaxEntryFeeForLine(this); }
		}

		ZDecimal IUltimateDistributee.LinePriceInInvoiceCurrency
		{
			get { return JO_LinePrice; }
		}

		ZInt IUltimateDistributee.OrderLineNumber
		{
			get { return JO_LineNo; }
		}

		ZString IUltimateDistributee.OrderNumber
		{
			get { return Order.JD_OrderNumber; }
		}

		ZString IUltimateDistributee.ProductCode
		{
			get { return JO_Partno; }
		}

		ZString IUltimateDistributee.ProductDepartment
		{
			get
			{
				var product = this.Product;
				return product != null ? product.OP_Department : ZString.Empty;
			}
		}

		ZString IUltimateDistributee.ProductDivision
		{
			get
			{
				var product = this.Product;

				return product != null ? product.OP_Division : ZString.Empty;
			}
		}

		ZString IUltimateDistributee.SupplierName
		{
			get
			{
				var supplier = Order.Supplier;

				return supplier != null ? supplier.OH_FullNameTruncated : ZString.Empty;
			}
		}

		string IUltimateDistributee.TableCode
		{
			get { return JobOrderLineSchema.Constants.Prefix; }
		}

		ZDecimal IUltimateDistributee.UnitPriceInInvoiceCurrency
		{
			get { return JO_ItemPrice; }
		}

		ZDecimal IUltimateDistributee.Volume
		{
			get { return JO_ActualVolume; }
		}

		ZString IUltimateDistributee.VolumeUQ
		{
			get { return JO_UnitOfVolume; }
		}

		ZDecimal IUltimateDistributee.Weight
		{
			get { return JO_ActualWeight; }
		}

		ZString IUltimateDistributee.WeightUQ
		{
			get { return JO_UnitOfWeight; }
		}

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
				case Schema.JO_ActualWeight:
					unitOfMeasure = JO_UnitOfWeight;
					break;

				case Schema.JO_ActualVolume:
					unitOfMeasure = JO_UnitOfVolume;
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
			this.SetRoundedValue(JobOrderLineSchema.JO_ActualVolume, JO_ActualVolumeInfo);
			this.SetRoundedValue(JobOrderLineSchema.JO_ActualWeight, JO_ActualWeightInfo);

			if (fDeliveries != null)
			{
				foreach (var orderLineDeliverContainer in ContainersOnAllDeliveries)
				{
					IDefaultNumberOfDecimalsSupporter container = orderLineDeliverContainer;
					container.RoundMeasurePropertiesOnTransportModeChanged();
				}
			}
		}

		#endregion

		#region IDocAddress Members

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

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate) => null;

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes => GetSupportedAddressTypes().ToArray();

		static IEnumerable<DocAddressType> GetSupportedAddressTypes()
		{
			yield return DocAddressType.Manufacturer;
			yield return DocAddressType.GoodsAvailableAt;
			yield return DocAddressType.GoodsDeliveredTo;

			if (AdvOrmFeatureHelper.IsEnabled)
			{
				yield return DocAddressType.ConsigneeDocumentaryAddress;
			}
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType) => null;

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

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress) => Env.Security.OrderManager;

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress) => false;

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType) => null;

		#endregion

		#region IWorkflowProvider Members

		ZString IWorkflowProviderCore.WorkflowType => WorkflowDescriptors.OrderLineWorkflowDescriptorCode;

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

		ProcessTaskCollection IWorkflowProvider.WorkflowItems => WorkflowItems;

		[ChildEditable(true)]
		public OrderLineProcessTaskCollection WorkflowItems
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
		OrderLineProcessTaskCollection fWorkflowItems;

		protected OrderLineProcessTaskCollection CreateProcessTaskCollection()
		{
			return new OrderLineProcessTaskCollection(this);
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
					var companyPks = companies.Select(x => x.PK).ToArray();

					workflowInformationProvider = new WorkflowInformationProvider(companyPks);
				}

				workflowInformationProvider.Destination = Order?.PortOfDischarge?.RL_PortName ?? ZString.Empty;
				workflowInformationProvider.Origin = Order?.PortOfLoading?.RL_PortName ?? ZString.Empty;
				workflowInformationProvider.BusinessContext = TrackingConstants.BusinessContext.Order;

				return workflowInformationProvider;
			}
		}
		WorkflowInformationProvider workflowInformationProvider;

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();

			if (Order != null)
			{
				result.Add(ProcessTaskTemplateSchema.P0_OH_Client, GetClientsInTemplateSelectionOrder());
				result.Add(ProcessTaskTemplateSchema.P0_SubType1, Order.JD_TransportMode, ZString.Empty);
			}

			return result;
		}

		IZType[] GetClientsInTemplateSelectionOrder()
		{
			var result = new List<IZType>();
			var workflowType = ((IWorkflowProvider)Order).WorkflowType;

			var collection = WorkflowDataRegistry.Instance.ClientInTemplateSelection.Value;
			var entry = collection.GetValueByCode(workflowType);

			if (entry != null)
			{
				foreach (ClientInTemplateSelectionCriteriaOrgType orgType in entry.SelectedItems)
				{
					if (orgType.OrgTypeCode == WorkflowSelectionOrgTypeCodes.ControllingCustomer
						&& Order.DocAddresses.ContainsDocAddressType(DocAddressType.ControllingCustomer))
					{
						if (Order.ControllingCustomerDocAddress?.Organisation != null)
						{
							result.Add(Order.ControllingCustomerDocAddress.Organisation.PK);
						}
					}
					else if (orgType.OrgTypeCode == WorkflowSelectionOrgTypeCodes.BuyerSupplier)
					{
						if (ImportExportHelper.IsImport(Order.JD_RL_NKGoodsAvailableAt, Order.JD_RL_NKGoodsDeliveredTo))
						{
							result.Add(Order.BuyerPK);
							result.Add(Order.SupplierPK);
						}
						else
						{
							result.Add(Order.SupplierPK);
							result.Add(Order.BuyerPK);
						}
					}
				}

				result.Add(ZGuid.Empty);
			}

			return result.ToArray();
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
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
			var customLabelsProvider = new CustomLabelsProvider(this);
			var list = customLabelsProvider.GetCustomFields(customLabelsProvider.ConfigOrgProvider.ConfigOrg, Factory);
			foreach (var field in list.OfType<CustomLabelInfo>())
			{
				if (field.IsEnabled)
				{
					properties.Add(field.PropertyName, field.Caption, field.Position);
				}
			}
		}

		#endregion

		#region IProcessHandlingInfoProvider

		ProcessHandlingInfo IProcessHandlingInfoProvider.ProcessHandlingInfo => new OrderLineProcessHandlingInfo(this);

		#endregion

		#region IDocumentSupportable

		public DocumentSupporter DocumentSupporter => new OrderLineDocumentSupporter(this);

		#endregion

		#region IJobNumber

		string IJobNumber.JobNumber => Order?.JD_OrderNumberAndSplit ?? string.Empty;

		#endregion

		#region JobSupplierBookingLines

		public JobSupplierBookingLineCollection SupplierBookingLines => supplierBookingLines ?? (supplierBookingLines = new JobSupplierBookingLineCollection(this));
		JobSupplierBookingLineCollection supplierBookingLines;

		#endregion

		#region FormCaption

		public string FormCaption
		{
			get
			{
				var order = Res.GetString("Forwarding|OrderLineForm|FormCaption|Order", "Order: {0}", Order.JD_OrderNumber);

				if (!string.IsNullOrEmpty(JO_LineReference) && OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.Value)
				{
					return $"{order}, {Res.GetString("Forwarding|OrderLineForm|FormCaption|LineReference", "Line Reference: {0}", JO_LineReference)}";
				}
				else
				{
					var orderSplit = Res.GetString("Forwarding|OrderLineForm|FormCaption|OrderSplit", "Order Split: {0}", Order.JD_OrderNumberSplit);
					var orderLine = Res.GetString("Forwarding|OrderLineForm|FormCaption|Line", "Line: {0}", JO_LineNo);
					var orderLineSplit = Res.GetString("Forwarding|OrderLineForm|FormCaption|LineSplit", "Line Split: {0}", JO_LineSplitNumber);

					return $"{order}, {orderLine} ({orderSplit}, {orderLineSplit})";
				}
			}
		}

		#endregion

		public void SetOrderLineToDeliveredIfWithinTolerance()
		{
			if (JO_LineStatus == OrderStatus.Delivered)
			{
				return;
			}

			var minQuantity = JO_Quantity - Utilities.Round(JO_Quantity * JO_UnderQuantityPercentageLimit / 100.0m, 5);
			if (JO_QtyPacked >= minQuantity)
			{
				JO_LineStatus = OrderStatus.Delivered;
			}
		}

		void LogEventForMismatchedShipWindow(Func<JobSupplierBookingLine, bool> shipmentWindowDateDiffCheck, Action<IEnumerable<JobSupplierBooking>> addLog)
		{
			var bookings = SupplierBookingLines
				.Where(bookingLine => bookingLine?.SupplierBooking != null && bookingLine.SupplierBooking.JSB_Status == SupplierBookingStatus.Placed && shipmentWindowDateDiffCheck(bookingLine))
				.Select(bookingLine => bookingLine.SupplierBooking).Distinct().OrderBy(booking => booking.JSB_BookingId);
			if (!bookings.IsNullOrEmpty())
			{
				addLog(bookings);
			}
		}

		public void LogEventForMismatchedShipWindow(bool updateFromOrder)
		{
			if (AdvOrmFeatureHelper.IsEnabled && Order != null && IsInDatabase)
			{
				if ((updateFromOrder && Order.JD_ShipmentWindowStartInfo.HasChanges && !Order.JD_ShipmentWindowStart.IsEmpty && JO_ShipmentWindowStart.IsEmpty)
					|| (!updateFromOrder && JO_ShipmentWindowStartInfo.HasChanges && (!JO_ShipmentWindowStart.IsEmpty || !Order.JD_ShipmentWindowStart.IsEmpty)))
				{
					var shipmentWindowStart = JO_ShipmentWindowStart.IsEmpty ? Order.JD_ShipmentWindowStart : JO_ShipmentWindowStart;
					LogEventForMismatchedShipWindow(
						(bookingLine) => bookingLine.JSL_ShipmentWindowStart.IsEmpty || bookingLine.JSL_ShipmentWindowStart != shipmentWindowStart,
						AddExceptionRaisedEventForMismatchedShipWindowStart);
				}

				if ((updateFromOrder && Order.JD_ShipmentWindowEndInfo.HasChanges && !Order.JD_ShipmentWindowEnd.IsEmpty && JO_ShipmentWindowEnd.IsEmpty)
					|| (!updateFromOrder && JO_ShipmentWindowEndInfo.HasChanges && (!JO_ShipmentWindowEnd.IsEmpty || !Order.JD_ShipmentWindowEnd.IsEmpty)))
				{
					var shipmentWindowEnd = JO_ShipmentWindowEnd.IsEmpty ? Order.JD_ShipmentWindowEnd : JO_ShipmentWindowEnd;
					LogEventForMismatchedShipWindow(
						(bookingLine) => bookingLine.JSL_ShipmentWindowEnd.IsEmpty || bookingLine.JSL_ShipmentWindowEnd != shipmentWindowEnd,
						AddExceptionRaisedEventForMismatchedShipWindowEnd);
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "System event info")]
		public void AddExceptionRaisedEventForMismatchedShipWindowStart(IEnumerable<JobSupplierBooking> bookings)
		{
			var bookingIds = string.Join(",", bookings.Select(booking => booking.JSB_BookingId));
			var parameters = new KeyValuePair<string, string>[]
			{
				new KeyValuePair<string, string>(Params.Type, (NoResString)"Ship Window Start"),
				new KeyValuePair<string, string>(Params.Reason, $"This Order Line has a Ship Window Start mismatch with its linked Supplier Booking. Please check and review the linked Supplier Booking ({bookingIds}).")
			};

			Logs.CreateOrRecreateEventLog(
				Events.ExceptionRaised,
				EstimateActual.Actual,
				ZDateTimeOffset.Now,
				string.Empty,
				parameters);

			AddExceptionRaisedEventOnBookingForMismatchedShipWindow(bookings, "Ship Window Start", "This Supplier Booking has booking lines whose Ship Window Start dates do not match with their order line dates.");
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "System event info")]
		public void AddExceptionRaisedEventForMismatchedShipWindowEnd(IEnumerable<JobSupplierBooking> bookings)
		{
			var bookingIds = string.Join(",", bookings.Select(booking => booking.JSB_BookingId));
			var parameters = new KeyValuePair<string, string>[]
			{
				new KeyValuePair<string, string>(Params.Type, (NoResString)"Ship Window End"),
				new KeyValuePair<string, string>(Params.Reason, $"This Order Line has a Ship Window End mismatch with its linked Supplier Booking. Please check and review the linked Supplier Booking ({bookingIds ?? ""}).")
			};

			Logs.CreateOrRecreateEventLog(
				Events.ExceptionRaised,
				EstimateActual.Actual,
				ZDateTimeOffset.Now,
				string.Empty,
				parameters);

			AddExceptionRaisedEventOnBookingForMismatchedShipWindow(bookings, "Ship Window End", "This Supplier Booking has booking lines whose Ship Window End dates do not match with their order line dates.");
		}

		static void AddExceptionRaisedEventOnBookingForMismatchedShipWindow(IEnumerable<JobSupplierBooking> bookings, string type, string reason)
		{
			var parameters = new KeyValuePair<string, string>[]
			{
				new (Params.Type, type), // System event info
				new (Params.Reason, reason),
			};

			bookings.ForEach(booking => booking.Logs.CreateOrRecreateEventLog(
				Events.ExceptionRaised,
				EstimateActual.Actual,
				ZDateTimeOffset.Now,
				string.Empty,
				parameters));
		}

		public JobDocAddress GetValidAddressWithFallbackToOrder(DocAddressType addressType)
		{
			var found = DocAddresses.FindByDocAddressType(addressType);
			if (found == null || found.E2_AddressOverride || found.E2_OA_Address.IsEmpty)
			{
				found = Order.DocAddresses.FindByDocAddressType(addressType);
				if (found != null && !found.E2_AddressOverride && !found.E2_OA_Address.IsEmpty)
				{
					return found;
				}

				return null;
			}

			return found;
		}

		#region IExternalRequestGenerationProvider

		public ZString GetRequestJobID() => Order?.JD_OrderNumber ?? ZString.Empty;

		public ZString GetRequestTypeCode() => ExternalRequestTypes.Codes.OrderLine;

		public (ZGuid OrginzationPK, ZGuid ContactPK) GetRequestSupportedAddressInfo(ZString addressType) => addressType.ToString() switch
		{
			DocAddressTypes.Codes.Manufacturer => RequestGenerationProviderHelper.GetRequestSupportedAddressInfo(DocAddresses.FindByDocAddressType(DocAddressType.Manufacturer)),
			_ => Order?.GetRequestSupportedAddressInfo(addressType) ?? (ZGuid.Empty, ZGuid.Empty)
		};

		#endregion

		#region Test Data
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			if (Order == null)
			{
				var buyer = Factory.NewWithValidTestData<OrgHeader>();
				buyer.OH_Code = "BUY" + new Random().Next(1000000).ToString(CultureInfo.InvariantCulture);
				buyer.MainAddress.OA_Address1 = "BuyerTestAddress";

				var order = Factory.New<Order>();
				order.BuyerPK = buyer.PK;

				order.OrderLines.Add(this);
			}
		}

#endif
		#endregion
	}
}
