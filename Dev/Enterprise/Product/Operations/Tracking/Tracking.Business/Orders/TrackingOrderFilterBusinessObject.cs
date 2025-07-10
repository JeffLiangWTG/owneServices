using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Tracking.Business
{
	/// <summary>
	/// Filter business object for order search page.
	/// </summary>
	public class TrackingOrderFilterBusinessObject_Old_ForWeb : OrdersFilterBusinessObject_Old_ForWeb
	{
		#region Schema

		[SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Follow same inheritance as the containing class")]
		public abstract new class Schema : OrdersFilterBusinessObject_Old_ForWeb.Schema
		{
			public const string Org1Code = "Org1Code";
			public const string Org2Code = "Org2Code";
			public const string Org1Caption = "Org1Caption";
			public const string Org2Caption = "Org2Caption";
			public const string Location1Caption = "Location1Caption";
			public const string Location2Caption = "Location2Caption";
		}

		#endregion

		public TrackingOrderFilterBusinessObject_Old_ForWeb(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			Helper = new BusinessHelper(factory);
		}

		readonly BusinessHelper Helper;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			this.JD_IsCancelled = ZBool.False;
		}

		#region Filtering

		public override ZQuery Filter
		{
			get
			{
				ZQuery result = base.Filter;
				result.AddToFilter(CurrentOrgOrderFilter);
				result.IsNoResultQuery |= HasErrors || CurrentOrg.IsEmpty;

				return result;
			}
		}

		protected override ZQuery JD_OrderStatusDropEditPanelFilter
		{
			get
			{
				ZQuery filter = new ZQuery();
				if (JD_OrderStatus == "UND")
				{
					filter.AddToFilter(JobOrderHeaderSchema.JD_OrderStatus, SQLComparisonOperator.NotEqual, Core.Constants.OrderStatus.Delivered);
				}
				else
				{
					filter = base.JD_OrderStatusDropEditPanelFilter;
				}
				return filter;
			}
		}

		#region Additional filters

		protected ZQuery fCurrentOrgOrderFilter;
		public ZQuery CurrentOrgOrderFilter
		{
			get
			{
				if ((fCurrentOrgOrderFilter == null))
				{
					fCurrentOrgOrderFilter = new ZQuery();
					if (!CurrentOrg.IsEmpty)
					{
						// Order related Organisations
						ZDBOnlyQuery onlyMyOrgFilter = new ZDBOnlyQuery(typeof(Order));
						onlyMyOrgFilter.AddToFilter(OrgRestrictionFilterFactory.Instance.GetFilter<TrackingOrder>());

						// Shipment related Organisations
						ZDBOnlySubQuery shipmentOrgs = new ZDBOnlySubQuery(typeof(TrackingShipment), JobOrderHeaderSchema.JD_JS);
						shipmentOrgs.AddToFilter(OrgRestrictionFilterFactory.Instance.GetFilter<TrackingShipment>());

						onlyMyOrgFilter.AddSubQuery(shipmentOrgs, JoinCondition.Or);
						fCurrentOrgOrderFilter.AddToFilter(onlyMyOrgFilter);
					}
				}
				return fCurrentOrgOrderFilter;
			}
		}

		#endregion

		#endregion

		/// <summary>
		/// Required to filter all the results by organisation 
		/// current user belongs to
		/// </summary>
		public ZGuid CurrentOrg
		{
			get
			{
				if (fCurrentOrg.IsMissing || fCurrentOrg.IsEmpty)
				{
					if (WebEnv.AppInstance != null)
					{
						var siteUser = WebEnv.AppInstance.SiteUser as OrgContactWebUser;
						if (siteUser != null && siteUser.IsLoggedIn)
						{
							fCurrentOrg = siteUser.LoggedInOrganisation.PK;
						}
					}
				}
				return fCurrentOrg;
			}
#if DEBUG
			set { fCurrentOrg = value; }
#endif
		}
		ZGuid fCurrentOrg;

		#region Overrides

		#region Filter drop downs

		public override ZQueryProviderCodeDescriptionList JD_NumberFilterType_List
		{
			get
			{
				ZQueryProviderCodeDescriptionList list = base.JD_NumberFilterType_List;
				list.RemoveCode((NoResString)"None"); // Programmatic constant

				if (!CurrentOrg.IsEmpty && CurrentOrg.IsValid)
				{
					OrgCustomLabelsCollection customLabels = new OrgCustomLabelsCollection(Factory.Load<OrgHeader>(CurrentOrg), Factory);
					customLabels.Load(new ZQuery(OrgCustomLabelsSchema.OT_FieldName, SQLComparisonOperator.StartsWith, "OrderHeader"));

					if (customLabels.Count > 0)
					{
						foreach (OrgCustomLabels label in customLabels)
						{
							SchemaColumn column = GetCustomLabelSchemaColumn(label.OT_FieldName);
							if (column != null)
							{
								list.Add(label.OT_FieldName, (NoResString)label.OT_Caption, column);
							}
						}
					}
				}

				return list;
			}
		}

		SchemaColumn GetCustomLabelSchemaColumn(ZString customLabelField)
		{
			SchemaColumn result = null;
			switch (customLabelField)
			{
				case "OrderHeader.CustomAttrib1":
					result = JobOrderHeaderSchema.JD_CustomAttrib1;
					break;
				case "OrderHeader.CustomAttrib2":
					result = JobOrderHeaderSchema.JD_CustomAttrib2;
					break;
				case "OrderHeader.CustomAttrib3":
					result = JobOrderHeaderSchema.JD_CustomAttrib3;
					break;
				case "OrderHeader.CustomAttrib4":
					result = JobOrderHeaderSchema.JD_CustomAttrib4;
					break;
				case "OrderHeader.CustomAttrib5":
					result = JobOrderHeaderSchema.JD_CustomAttrib5;
					break;
			}

			return result;
		}

		public override ZQueryProviderCodeDescriptionListWith2FilterArguments JD_DateFilterType_List
		{
			get
			{
				ZQueryProviderCodeDescriptionListWith2FilterArguments list = base.JD_DateFilterType_List;
				list.RemoveCode((NoResString)"None"); // Programmatic constant
				return list;
			}
		}

		public override ZQueryProviderCodeDescriptionListWith2FilterArguments JD_OrgFilterType_List
		{
			get
			{
				ZQueryProviderCodeDescriptionListWith2FilterArguments list = base.JD_OrgFilterType_List;
				list.RemoveCode((NoResString)"None"); // Programmatic constant
				return list;
			}
		}

		public override ZQueryProviderCodeDescriptionListWith2FilterArguments JD_PortFilterType_List
		{
			get
			{
				ZQueryProviderCodeDescriptionListWith2FilterArguments list = base.JD_PortFilterType_List;
				list.RemoveCode((NoResString)"None"); // Programmatic constant
				return list;
			}
		}

		public override CodeDescriptionPairList JD_OrderStatus_List
		{
			get
			{
				CodeDescriptionPairList list = base.JD_OrderStatus_List;
				list.Insert(0, new CodeDescriptionPair(Core.Constants.OrderStatus.All, Res.GetString("f487b3d5-3aa4-4f3c-b26e-45979e1de4d6", "All Orders")));
				list.AddPair("UND", Res.GetString("13bf0c48-eb3c-48f2-9f36-4cbf2acfeba5", "Undelivered"));
				return list;
			}
		}
		#endregion

		#endregion

		#region New Properties

		#region Org1Code

		[BusinessObjectTestExclude]
		public ZString Org1Code
		{
			get { return Helper.GetOrgCode(JD_OH_Org1); }
			set { JD_OH_Org1 = Helper.GetPKFromOrgCode(value); }
		}

		public virtual ZPropertyInfo Org1CodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.Org1Code, x => JD_OH_Org1Info); }
		}

		#endregion

		#region Org2Code

		[BusinessObjectTestExclude]
		public ZString Org2Code
		{
			get { return Helper.GetOrgCode(JD_OH_Org2); }
			set { JD_OH_Org2 = Helper.GetPKFromOrgCode(value); }
		}

		public virtual ZPropertyInfo Org2CodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.Org2Code, x => JD_OH_Org2Info); }
		}

		#endregion OrgCodes

		#region Org Caption

		public ZString Org1Caption
		{
			get
			{
				ZString result;

				switch (JD_OrgFilterType)
				{
					case OrdersConstants.OrgFilterTypes.BuyerSupplier:
						result = Res.GetString("568f1e57-ca63-477c-af8c-db854c93f6df", "Buyer");
						break;

					case OrdersConstants.OrgFilterTypes.SendingRecvAgent:
						result = Res.GetString("5836e625-e051-4726-aed7-d83db74c934e", "Send. Agent");
						break;

					default:
						result = Res.GetString("11e102a1-41d8-408c-b580-cc09de90a90c", "Org. 1");
						break;
				}

				return result + ": ";
			}
		}

		public ZPropertyInfo Org1CaptionInfo
		{
			get { return GetZPropertyInfo(Schema.Org1Caption); }
		}

		public ZString Org2Caption
		{
			get
			{
				ZString result;

				switch (JD_OrgFilterType)
				{
					case OrdersConstants.OrgFilterTypes.BuyerSupplier:
						result = Res.GetString("253fd4ef-e30c-4dcc-b3ce-04b05dd47d41", "Supplier");
						break;

					case OrdersConstants.OrgFilterTypes.SendingRecvAgent:
						result = Res.GetString("b5e5ed55-3ec9-4b5f-8495-94c271081a9b", "Recv. Agent");
						break;

					default:
						result = Res.GetString("873a3b93-dad6-4234-9b34-4a1691b119b2", "Org. 2");
						break;
				}

				return result + ": ";
			}
		}

		public ZPropertyInfo Org2CaptionInfo
		{
			get { return GetZPropertyInfo(Schema.Org2Caption); }
		}

		#endregion

		#region Location Caption

		public ZString Location1Caption
		{
			get
			{
				ZString result;

				switch (JD_PortFilterType)
				{
					case OrdersConstants.PortFilterTypes.LoadDischargeCode:
						result = Res.GetString("c4ef6171-18c1-4431-a964-b0740be05059", "Load");
						break;

					case OrdersConstants.PortFilterTypes.AvailableAtDeliveredToCode:
						result = Res.GetString("f57ad36d-4f26-4d14-87cc-0f6839025a44", "Origin");
						break;

					default:
						result = Res.GetString("3bb08074-cbbc-4cf3-9899-6c9004bd1408", "From");
						break;
				}

				return result + ": ";
			}
		}

		public ZPropertyInfo Location1CaptionInfo
		{
			get { return GetZPropertyInfo(Schema.Location1Caption); }
		}

		public ZString Location2Caption
		{
			get
			{
				ZString result;

				switch (JD_PortFilterType)
				{
					case OrdersConstants.PortFilterTypes.LoadDischargeCode:
						result = Res.GetString("45b00ccf-a8b2-4071-9371-ed84e39a210a", "Discharge");
						break;

					case OrdersConstants.PortFilterTypes.AvailableAtDeliveredToCode:
						result = Res.GetString("9ec05b9a-8e8e-426c-a0a5-a58f9b928c07", "Destination");
						break;

					default:
						result = Res.GetString("94f00675-726e-4867-9517-f4f223860f97", "To");
						break;
				}

				return result + ": ";
			}
		}

		public ZPropertyInfo Location2CaptionInfo
		{
			get { return GetZPropertyInfo(Schema.Location2Caption); }
		}

		#endregion

		#endregion
	}
}
#region Web Filter

namespace Enterprise.Tracking.Business
{
	using System;
	using System.Data;
	using CargoWise.Integration;
	using Enterprise.Core;
	using Enterprise.Freight.Business;
	using Enterprise.Freight.Common.Business;
	using Enterprise.Freight.Forwarding.Business;
	using Enterprise.Freight.Forwarding.Orders.Business;
	using Enterprise.MasterFiles.Business;

	#region OrdersFilterBusinessObject_Old_ForWeb
	public class OrdersFilterBusinessObject_Old_ForWeb : AutoOrdersFilterBusinessObject, ICustomLabelsConfigOrgProvider
	{
		public OrdersFilterBusinessObject_Old_ForWeb(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			UpdateComparisonOperators();
			UpdateQueryDeciderTypes();
		}

		#region Expensive Query Handling

		public override bool IsExpensiveQuery
		{
			get
			{
				return (FilterOperator == SQLComparisonOperator.Contains ||
					JD_NumberFilterType == OrdersConstants.NumberFilterTypes.All);
			}
		}

		public override string ExpensiveQueryWarning
		{
			get
			{
				string warnings = "";

				if (FilterOperator == SQLComparisonOperator.Contains)
				{
					warnings += "   " + Res.GetString("4588f059-fade-4d45-a5d7-cd06ec9f5cfe", "'Contains' for Number Types") + "\n";
				}

				if (JD_NumberFilterType == OrdersConstants.NumberFilterTypes.All)
				{
					warnings += "   " + Res.GetString("45dccd36-ae7e-4438-9bf4-0095b03bfd37", "'All' for Number Types") + "\n";
				}

				return Res.GetString("4e96894e-8cc4-4bf6-96ca-e695ac7d58cc", "The following selected options can cause your search to take a long time to complete:\r\n\r\n{0}\r\nAre you sure you want to perform this search?", warnings);
			}
		}

		#endregion

		#region Voyage / Vessel Changing Stuff Based on Transport Mode

		public override ZString JD_TransportMode
		{
			get
			{
				return base.JD_TransportMode;
			}
			set
			{
				base.JD_TransportMode = value;
				VesselInformationVisibleInfo.RefreshBinding();
			}
		}

		public ZBool VesselInformationVisible
		{
			get
			{
				bool result = true;

				switch (JD_TransportMode)
				{
					case Constants.TransportModes.Air:
					case Constants.TransportModes.AirSea:
						result = false;
						break;
				}

				return result;
			}
		}

		public ZPropertyInfo VesselInformationVisibleInfo
		{
			get { return GetZPropertyInfo(nameof(VesselInformationVisible)); }
		}

		#region ShowAttachedOrders
		public override ZBool ShowAttachedOrders
		{
			get { return base.ShowAttachedOrders; }
			set
			{
				base.ShowAttachedOrders = value;
				if (value)
				{ ShowUnAttachedOrders = false; }
			}
		}
		#endregion

		#region ShowUnAttachedOrders
		public override ZBool ShowUnAttachedOrders
		{
			get { return base.ShowUnAttachedOrders; }
			set
			{
				base.ShowUnAttachedOrders = value;
				if (value)
				{ ShowAttachedOrders = false; }
			}
		}
		#endregion

		public bool IsTransportModeSea
		{
			get { return (JD_TransportMode == Constants.TransportModes.Sea || JD_TransportMode == Constants.TransportModes.SeaAir); }
		}

		public bool IsTransportModeAir
		{
			get { return (JD_TransportMode == Constants.TransportModes.Air || JD_TransportMode == Constants.TransportModes.AirSea); }
		}

		#endregion

		#region New Properties

		public ZGuid BuyerPK
		{
			get { return (JD_OrgFilterType == OrdersConstants.OrgFilterTypes.BuyerSupplier) ? JD_OH_Org1 : ZGuid.Empty; }
			set
			{
				JD_OrgFilterType = OrdersConstants.OrgFilterTypes.BuyerSupplier;
				JD_OH_Org1 = value;
			}
		}
		public event EventHandler JD_OA_BuyerAddressChanged
		{
			add
			{
				this.JD_OrgFilterTypeInfo.ValueChanged += value;
				this.JD_OH_Org1Info.ValueChanged += value;
			}
			remove
			{
				this.JD_OrgFilterTypeInfo.ValueChanged -= value;
				this.JD_OH_Org1Info.ValueChanged -= value;
			}
		}

		public ZGuid SupplierPK
		{
			get { return (JD_OrgFilterType == OrdersConstants.OrgFilterTypes.BuyerSupplier) ? JD_OH_Org2 : ZGuid.Empty; }
			set
			{
				JD_OrgFilterType = OrdersConstants.OrgFilterTypes.BuyerSupplier;
				JD_OH_Org2 = value;
			}
		}

		#endregion

		#region Validation

		public override void ValidateJD_RL_NKPort1()
		{
			base.ValidateJD_RL_NKPort1();
			ListValidation.ErrorIfInvalidCode(JD_RL_NKPort1Info, JD_RL_List);
		}

		public override void ValidateJD_RL_NKPort2()
		{
			base.ValidateJD_RL_NKPort2();
			ListValidation.ErrorIfInvalidCode(JD_RL_NKPort2Info, JD_RL_List);
		}

		#endregion

		#region Filter

		public override ZQuery Filter
		{
			get
			{
				ZQuery result = base.Filter;
				if (!JD_JO_LineStatus.IsEmpty)
				{
					AddLineFilter(result);
				}
				if (JD_IsCancelled)
				{
					result.IgnoreActiveFilter = true;
				}
				if (ShowAttachedOrders)
				{
					result.AddToFilter(OrdersFilter(result, SQLComparisonOperator.NotEqual, JoinCondition.Or));
				}
				if (ShowUnAttachedOrders)
				{
					result.AddToFilter(OrdersFilter(result, SQLComparisonOperator.Equal, JoinCondition.And));
				}
				return result;
			}
		}

		#endregion

		#region Related Business Objects

		public OrgHeader Buyer
		{
			get { return (OrgHeader)Factory.Load(typeof(OrgHeader), BuyerPK); }
		}

		#endregion

		#region List Properties

		public OrgHeaderCollection JD_OH_List1
		{
			get
			{
				OrgHeaderCollection result = BindingLists.OrgHeader_List;

				switch (JD_OrgFilterType)
				{
					case OrdersConstants.OrgFilterTypes.BuyerSupplier:
						result = BindingLists.OrgConsignee_List;
						break;

					case OrdersConstants.OrgFilterTypes.SendingRecvAgent:
						result = BindingLists.ShippingProvider_List;
						break;
				}

				return result;
			}
		}

		public OrgHeaderCollection JD_OH_List2
		{
			get
			{
				OrgHeaderCollection result = BindingLists.OrgHeader_List;

				switch (JD_OrgFilterType)
				{
					case OrdersConstants.OrgFilterTypes.BuyerSupplier:
						result = BindingLists.OrgConsignor_List;
						break;

					case OrdersConstants.OrgFilterTypes.SendingRecvAgent:
						result = BindingLists.ShippingProvider_List;
						break;
				}

				return result;
			}
		}

		public OrgSupplierPartCollection JD_JO_Partno_List
		{
			get { return new OrgSupplierPartCollection(Factory, Buyer, (OrgHeader)Factory.Load(typeof(OrgHeader), SupplierPK), false); }
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
			get { return new OrderStatusLists().GetOrderStatusList(); }
		}

		public CodeDescriptionPairList JD_JO_LineStatus_List
		{
			get { return new OrderStatusLists().GetOrderLineStatusList(); }
		}

		public ShipmentCollection JD_JS_List
		{
			get
			{
				if (fJD_JS_List == null)
				{
					fJD_JS_List = new ShipmentCollection(Factory);
				}

				return fJD_JS_List;
			}
		}

		public RefUNLOCOCollection JD_RL_List
		{
			get
			{
				if (fJD_RL_List == null)
				{
					fJD_RL_List = new RefUNLOCOCollection(Factory);
				}

				return fJD_RL_List;
			}
		}

		public RefVesselCollection Vessel_List
		{
			get { return BindingLists.RefVessel_List; }
		}

		#endregion

		#region Staff_List

		public GlbStaffCollection Staff_List
		{
			get
			{
				if (fStaff_List == null)
				{
					fStaff_List = new GlbStaffCollection(Factory);
				}

				return fStaff_List;
			}
		}

		GlbStaffCollection fStaff_List;

		#endregion

		#region User

		protected override ZQuery StaffFilterPanelFilter
		{
			get
			{
				ZQuery result = new ZQuery();
				if (StaffFilter.IsValid && StaffFilterOption == OrdersConstants.StaffFilterTypes.StaffFilterUserRegistered)
				{
					AddCreatingUserToFilter(result, StaffFilter);
				}
				else
				{
					result.AddToFilter(base.StaffFilterPanelFilter);
				}
				return result;
			}
		}

		public override void ValidateStaffFilterOption()
		{
			base.ValidateStaffFilterOption();
			ListValidation.ErrorIfInvalidCode(StaffFilterOptionInfo, StaffFilterOptions);
			if (!StaffFilter.IsEmpty)
			{
				MandatoryValidation.CheckEntered(StaffFilterOptionInfo);
			}
		}

		public override ZGuid StaffFilter
		{
			get { return base.StaffFilter; }
			set
			{
				base.StaffFilter = value;
				if (!IsValidationSuspended)
				{
					ValidateStaffFilterOption();
				}
			}
		}

		#endregion

		#region Staff Filter Options

		public CodeDescriptionPairList StaffFilterOptions
		{
			get
			{
				if (fStaffFilterOptions == null)
				{
					fStaffFilterOptions = new CodeDescriptionPairList();
					fStaffFilterOptions.AddPair(OrdersConstants.StaffFilterTypes.StaffFilterUserRegistered, OrdersConstants.StaffFilterTypes.StaffFilterUserRegistered);
				}

				return fStaffFilterOptions;
			}
		}

		CodeDescriptionPairList fStaffFilterOptions;

		#endregion

		#region Query Decider Lists

		public virtual ZQueryProviderCodeDescriptionList JD_NumberFilterType_List
		{
			get
			{
				ZQueryProviderCodeDescriptionList result = new ZQueryProviderCodeDescriptionList();

				result.AddEmptySelection();
				result.Add(OrdersConstants.NumberFilterTypes.OrderNumber, ResString.GetMultilingualString("c61ea6e8-55a3-4f16-b37e-b7df448c0092", "Order #"), FilterOperator, JobOrderHeaderSchema.JD_OrderNumber);
				result.Add(OrdersConstants.NumberFilterTypes.BookingConfRef, ResString.GetMultilingualString("7ca45b3a-9f53-4925-9381-7bf9dffe08d7", "Confirm #"), FilterOperator, JobOrderHeaderSchema.JD_BookingConfRef);
				result.Add(OrdersConstants.NumberFilterTypes.InvoiceNumber, ResString.GetMultilingualString("5b3ca513-dd28-419b-8b8c-09f38fc416fa", "Invoice #"), FilterOperator, JobOrderHeaderSchema.JD_InvoiceNumber);
				result.Add(OrdersConstants.NumberFilterTypes.MasterBill, ResString.GetMultilingualString("9f5b97ab-b000-40dc-b5a6-1af4d3ce18bd", "Master Bill"), FilterOperator, new AddToQueryDelegate(AddMasterBillFilter));
				result.Add(OrdersConstants.NumberFilterTypes.HouseBill, ResString.GetMultilingualString("e8e1ec8c-406c-4b72-bfe8-ae7ca8014781", "House Bill"), FilterOperator, new AddToQueryDelegate(AddHouseBillFilter));
				result.Add(OrdersConstants.NumberFilterTypes.ProductNo, ResString.GetMultilingualString("c47052e2-79b8-4108-9be5-cce786d17751", "Product #"), FilterOperator, new AddToQueryDelegate(AddProductFilter));
				result.Add(OrdersConstants.NumberFilterTypes.ShipmentNo, ResString.GetMultilingualString("3b2a6fe6-b1f6-4885-9501-bb54114f9d8b", "Shipment #"), FilterOperator, new AddToQueryDelegate(AddShipmentNumFilter));
				result.Add(OrdersConstants.NumberFilterTypes.ContainerNo, ResString.GetMultilingualString("1c2c6e25-95cb-4b23-99ce-21b554eeefca", "Container #"), FilterOperator, new AddToQueryDelegate(AddContainerNumFilter));

				result.AddQueryProviderCompositionForAll(1);
				result.AddQueryProviderComposition(
					OrdersConstants.NumberFilterTypes.MostCommon, ResString.GetMultilingualString("168e2486-0239-40ef-ac5f-2ade9c5e1287", "Common"), 2,
					OrdersConstants.NumberFilterTypes.OrderNumber,
					OrdersConstants.NumberFilterTypes.BookingConfRef,
					OrdersConstants.NumberFilterTypes.InvoiceNumber);

				return result;
			}
		}

		public virtual ZQueryProviderCodeDescriptionListWith2FilterArguments JD_DateFilterType_List
		{
			get
			{
				ZQueryProviderCodeDescriptionListWith2FilterArguments result = new ZQueryProviderCodeDescriptionListWith2FilterArguments();

				result.AddEmptySelection();
				result.Add(OrdersConstants.DateFilterTypes.OrderDate, ResString.GetMultilingualString("1291c8a7-ea8b-4db5-8851-db43595afbc5", "Order Date"), JobOrderHeaderSchema.JD_OrderDate, JobOrderHeaderSchema.JD_OrderDate);
				result.Add(OrdersConstants.DateFilterTypes.ConfirmedDate, ResString.GetMultilingualString("7a510c7c-a2e6-495e-8112-d5257d35d6d6", "Confirmed Date"), JobOrderHeaderSchema.JD_BookingConfDate, JobOrderHeaderSchema.JD_BookingConfDate);
				result.Add(OrdersConstants.DateFilterTypes.FollowUpDate, ResString.GetMultilingualString("42a1e1a7-4172-4fb6-86b9-ee3b0af09917", "Follow Up Date"), JobOrderHeaderSchema.JD_FollowUpDate, JobOrderHeaderSchema.JD_FollowUpDate);
				result.Add(OrdersConstants.DateFilterTypes.ReqInStore, ResString.GetMultilingualString("f8f64735-6f33-49c5-8e24-517513cdf56f", "Required In Store"), JobOrderHeaderSchema.JD_DeliveryRequiredBy, JobOrderHeaderSchema.JD_DeliveryRequiredBy);
				result.Add(OrdersConstants.DateFilterTypes.ReqExWorks, ResString.GetMultilingualString("f1a10a6d-4e8d-4727-b221-fa5094b6ebc6", "Required Ex-Works"), JobOrderHeaderSchema.JD_ExWorksRequiredBy, JobOrderHeaderSchema.JD_ExWorksRequiredBy);

				result.AddQueryProviderCompositionForAll(1);
				result.AddQueryProviderComposition(
					OrdersConstants.DateFilterTypes.MostCommon, ResString.GetMultilingualString("03a5567f-24d3-45b1-9a31-2e240e311af7", "Common"), 2,
					OrdersConstants.DateFilterTypes.OrderDate,
					OrdersConstants.DateFilterTypes.ConfirmedDate);
				return result;
			}
		}

		public virtual ZQueryProviderCodeDescriptionListWith2FilterArguments JD_OrgFilterType_List
		{
			get
			{
				ZQueryProviderCodeDescriptionListWith2FilterArguments result = new ZQueryProviderCodeDescriptionListWith2FilterArguments();

				result.AddEmptySelection();
				result.Add(
					OrdersConstants.OrgFilterTypes.BuyerSupplier, ResString.GetMultilingualString("0416cf94-16ee-469c-99a9-5c4e5fad26be", "Buyer / Supplier"),
					new AddToQueryDelegate(AddBuyerFilter),
					new AddToQueryDelegate(AddSupplierFilter));
				result.Add(
					OrdersConstants.OrgFilterTypes.SendingRecvAgent, ResString.GetMultilingualString("e0b49a4b-e6a6-4e02-ac22-59f891373646", "Send. / Recv. Agent"),
					JobOrderHeaderSchema.JD_OH_SendingAgent, JobOrderHeaderSchema.JD_OH_ReceivingAgent);

				result.AddQueryProviderCompositionForAll(1);

				return result;
			}
		}

		void AddBuyerFilter(ZQuery query, SQLComparisonOperator sqlOperator, object value)
		{
			AddAddressSubQueryFilter(query, sqlOperator, value, JobOrderHeaderSchema.JD_OA_BuyerAddress);
		}

		void AddSupplierFilter(ZQuery query, SQLComparisonOperator sqlOperator, object value)
		{
			AddAddressSubQueryFilter(query, sqlOperator, value, JobOrderHeaderSchema.JD_OA_SupplierAddress);
		}

		void AddAddressSubQueryFilter(ZQuery query, SQLComparisonOperator sqlOperator, object value, SchemaGuidColumn fkColumn)
		{
			if (!((ZGuid)value).IsEmpty)
			{
				var orgPK = (ZGuid)value;

				var addressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), fkColumn);
				addressSubQuery.AddToFilter(JoinCondition.And, OrgAddressSchema.OA_OH, sqlOperator, orgPK);

				var dbOnlyResult = new ZDBOnlyQuery(typeof(Order));
				dbOnlyResult.AddSubQuery(addressSubQuery, JoinCondition.And);

				query.AddToFilter(dbOnlyResult);
			}
		}

		public virtual ZQueryProviderCodeDescriptionListWith2FilterArguments JD_PortFilterType_List
		{
			get
			{
				ZQueryProviderCodeDescriptionListWith2FilterArguments result = new ZQueryProviderCodeDescriptionListWith2FilterArguments();

				result.AddEmptySelection();
				result.Add(
					OrdersConstants.PortFilterTypes.LoadDischargeCode, ResString.GetMultilingualString("61f54792-a982-43a1-be09-ce1489b36ff5", "Consol Load / Discharge"),
					JobOrderHeaderSchema.JD_RL_NKPortOfLoading, JobOrderHeaderSchema.JD_RL_NKPortOfDischarge);
				result.Add(
					OrdersConstants.PortFilterTypes.AvailableAtDeliveredToCode, ResString.GetMultilingualString("cef3f59d-8f86-4abe-ae80-02b67c8afc28", "Shipment Origin / Dest."),
					JobOrderHeaderSchema.JD_RL_NKGoodsAvailableAt, JobOrderHeaderSchema.JD_RL_NKGoodsDeliveredTo);

				result.AddQueryProviderCompositionForAll(1);

				return result;
			}
		}

		#endregion

		#region ICustomLabelsConfigOrgProvider Members

		event EventHandler ICustomLabelsConfigOrgProvider.ConfigOrgChanged
		{
			add { this.JD_OA_BuyerAddressChanged += value; }
			remove { JD_OA_BuyerAddressChanged -= value; }
		}

		OrgHeader ICustomLabelsConfigOrgProvider.ConfigOrg
		{
			get { return this.Buyer; }
		}

		#endregion

		#region Implementation

		protected BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		protected ShipmentCollection fJD_JS_List;
		protected RefUNLOCOCollection fJD_RL_List;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			JD_OrderStatus = Constants.OrderStatus.All;
			JD_NumberFilterType = OrdersConstants.NumberFilterTypes.MostCommon;
			JD_DateFilterType = OrdersConstants.DateFilterTypes.MostCommon;
			JD_OrgFilterType = OrdersConstants.OrgFilterTypes.All;
			JD_PortFilterType = OrdersConstants.PortFilterTypes.All;
			if (StaffFilterOption.IsEmpty)
			{
				StaffFilterOption = OrdersConstants.StaffFilterTypes.StaffFilterUserRegistered;
			}

			FilterOperator = SQLComparisonOperator.StartsWith;
		}

		protected void UpdateComparisonOperators()
		{
			if (FilterOperator == SQLComparisonOperator.Contains)
			{
				FilterOperator = SQLComparisonOperator.StartsWith;
			}
		}

		protected void UpdateQueryDeciderTypes()
		{
			if (JD_NumberFilterType == OrdersConstants.NumberFilterTypes.All)
			{
				JD_NumberFilterType = OrdersConstants.NumberFilterTypes.MostCommon;
			}
		}

		#region Filter

		protected ZQuery OrdersFilter(ZQuery result, SQLComparisonOperator @operator, JoinCondition join)
		{
			ZQuery query = new ZQuery();
			ZQuery shipmentAttachedOrdersFilter = new ZQuery(JobOrderHeaderSchema.JD_JS, @operator, null);
			ZQuery declarationAttachedOrdersFilter = new ZQuery(JobOrderHeaderSchema.JD_JE, @operator, null);
			query.AddToFilter(declarationAttachedOrdersFilter);
			query.AddToFilter(shipmentAttachedOrdersFilter, join);
			return query;
		}

		protected override ZQuery JD_OrderStatusDropEditPanelFilter
		{
			get
			{
				ZQuery result = new ZQuery();

				if (JD_OrderStatus != Constants.OrderStatus.All)
				{
					result.AddToFilter(JobOrderHeaderSchema.JD_OrderStatus, SQLComparisonOperator.Equal, JD_OrderStatus);
				}

				return result;
			}
		}

		protected void AddLineFilter(ZQuery query)
		{
			ZDBOnlySubQuery subQueries = new ZDBOnlySubQuery(typeof(OrderLine), JobOrderLineSchema.JO_JD);
			subQueries.AddToFilter(JoinCondition.And, JobOrderLineSchema.JO_LineStatus, SQLComparisonOperator.Contains, JD_JO_LineStatus);

			ZDBOnlyQuery dbOnlyResult = new ZDBOnlyQuery(typeof(Order));
			dbOnlyResult.AddSubQuery(subQueries, JoinCondition.And);
			dbOnlyResult.AddToFilter(base.Filter, JoinCondition.And);
			query.AddToFilter(dbOnlyResult);
		}

		protected void AddMasterBillFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			if (!((ZString)value).IsEmpty)
			{
				ZString masterBill = ((ZString)value).Replace(" ", "").Replace("-", "");
				ZDBOnlyQuery dbOnlyResult = new ZDBOnlyQuery(typeof(Order));

				ZDBOnlySubQuery shipmentSubQuery = new ZDBOnlySubQuery(typeof(CommonShipment), JobOrderHeaderSchema.JD_JS);
				ZDBOnlySubQuery pivotSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
				ZDBOnlySubQuery consolSubQuery = new ZDBOnlySubQuery(typeof(ForwardingConsol), JobConShipLinkSchema.JN_JK);

				consolSubQuery.AddToFilter(
					JoinCondition.And,
					JobConsolSchema.JK_MasterBillNum, @operator, masterBill);
				pivotSubQuery.AddSubQuery(consolSubQuery, JoinCondition.And);
				shipmentSubQuery.AddSubQuery(pivotSubQuery, JoinCondition.And);
				dbOnlyResult.AddSubQuery(shipmentSubQuery, JoinCondition.And);

				query.AddToFilter(dbOnlyResult, JoinCondition.And);
			}
		}

		protected void AddHouseBillFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			if (value is ZString houseBill && !houseBill.IsEmpty)
			{
				ZDBOnlySubQuery subQueries = new ZDBOnlySubQuery(typeof(CommonShipment), JobOrderHeaderSchema.JD_JS);
				subQueries.AddToFilter(JoinCondition.And, JobShipmentSchema.JS_HouseBill, @operator, houseBill.Left(AutoJobShipment.Schema.JS_HouseBillMaxLength));

				ZDBOnlyQuery dbOnlyResult = new ZDBOnlyQuery(typeof(Order));
				dbOnlyResult.AddSubQuery(subQueries, JoinCondition.And);

				query.AddToFilter(dbOnlyResult);
			}
		}

		protected void AddProductFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			if (!((ZString)value).IsEmpty)
			{
				ZDBOnlySubQuery subQueries = new ZDBOnlySubQuery(typeof(OrderLine), JobOrderLineSchema.JO_JD);
				subQueries.AddToFilter(JoinCondition.And, JobOrderLineSchema.JO_Partno, @operator, value);

				ZDBOnlyQuery dbOnlyResult = new ZDBOnlyQuery(typeof(Order));
				dbOnlyResult.AddSubQuery(subQueries, JoinCondition.And);

				query.AddToFilter(dbOnlyResult);
			}
		}

		protected internal void AddContainerNumFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			var containerNum = (ZString)value;
			if (!containerNum.IsEmpty)
			{
				ZDBOnlyQuery dbOnlyResult = new ZDBOnlyQuery(typeof(Order));

				ZDBOnlyQuery dbOnlyResult1 = new ZDBOnlyQuery(typeof(Order));

				ZDBOnlySubQuery linesSubQuery = new ZDBOnlySubQuery(typeof(OrderLine), JobOrderLineSchema.JO_JD);
				ZDBOnlySubQuery deliveriesSubQuery = new ZDBOnlySubQuery(typeof(OrderLineDelivery), JobOrderLineDeliverySchema.J4_JO);
				ZDBOnlySubQuery containersSubQuery = new ZDBOnlySubQuery(typeof(OrderLineDeliverContainer), JobOrderLineDeliverContainerSchema.J5_J4);

				containersSubQuery.AddToFilter(
					JoinCondition.And,
					JobOrderLineDeliverContainerSchema.J5_ContainerNum, @operator, containerNum.SubstringSafe(0, JobOrderLineDeliverContainerSchema.J5_ContainerNum.MaxLength));

				deliveriesSubQuery.AddSubQuery(containersSubQuery, JoinCondition.And);
				linesSubQuery.AddSubQuery(deliveriesSubQuery, JoinCondition.And);
				dbOnlyResult1.AddSubQuery(linesSubQuery, JoinCondition.And);

				dbOnlyResult.AddToFilter(dbOnlyResult1, JoinCondition.Or);

				ZDBOnlyQuery dbOnlyResult2 = new ZDBOnlyQuery(typeof(Order));
				ZDBOnlySubQuery orderContainerSubQuery = new ZDBOnlySubQuery(typeof(OrderContainer), JobOrderContainerSchema.J1_ParentID);
				orderContainerSubQuery.AddToFilter(JobOrderContainerSchema.J1_ParentTableCode, JobOrderHeaderSchema.Constants.Prefix);
				orderContainerSubQuery.AddToFilter(
					JoinCondition.And,
					JobOrderContainerSchema.J1_ContainerNumber, @operator, containerNum.SubstringSafe(0, JobOrderContainerSchema.J1_ContainerNumber.MaxLength));
				dbOnlyResult2.AddSubQuery(orderContainerSubQuery, JoinCondition.And);

				dbOnlyResult.AddToFilter(dbOnlyResult2, JoinCondition.Or);

				ZDBOnlyQuery dbOnlyResult3 = new ZDBOnlyQuery(typeof(Order));

				ZDBOnlySubQuery jobContainerSubQuery = new ZDBOnlySubQuery(typeof(CommonContainer), JobContainerPackPivotSchema.J6_JC);
				jobContainerSubQuery.AddToFilter(JoinCondition.And, JobContainerSchema.JC_ContainerNum, @operator, containerNum.SubstringSafe(0, JobContainerSchema.JC_ContainerNum.MaxLength));

				ZDBOnlySubQuery shipmentQuery = new ZDBOnlySubQuery(typeof(CommonShipment), JobOrderHeaderSchema.JD_JS);
				ZDBOnlySubQuery packLineSubQuery = new ZDBOnlySubQuery(typeof(PackLine), JobPackLinesSchema.JL_JS);
				ZDBOnlySubQuery jobContainerPackPivotSubQuery = new ZDBOnlySubQuery(typeof(JobContainerPackPivot), JobContainerPackPivotSchema.J6_JL);

				jobContainerPackPivotSubQuery.AddSubQuery(jobContainerSubQuery, JoinCondition.And);
				packLineSubQuery.AddSubQuery(jobContainerPackPivotSubQuery, JoinCondition.And);
				shipmentQuery.AddSubQuery(packLineSubQuery, JoinCondition.And);

				dbOnlyResult3.AddSubQuery(shipmentQuery, JoinCondition.And);

				dbOnlyResult.AddToFilter(dbOnlyResult3, JoinCondition.Or);

				query.AddToFilter(dbOnlyResult, JoinCondition.And);
			}
		}

		protected void AddShipmentNumFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			if (value is ZString shipmentNumber && !shipmentNumber.IsEmpty)
			{
				if (shipmentNumber.Length < 9 && !shipmentNumber.Contains('S'))
				{
					shipmentNumber = 'S' + shipmentNumber.PadLeft(8, '0');
				}

				ZDBOnlySubQuery subQueries = new ZDBOnlySubQuery(typeof(CommonShipment), JobOrderHeaderSchema.JD_JS);
				subQueries.AddToFilter(JoinCondition.And, JobShipmentSchema.JS_UniqueConsignRef, @operator, shipmentNumber.Left(AutoJobShipment.Schema.JS_UniqueConsignRefMaxLength));

				ZDBOnlyQuery dbOnlyResult = new ZDBOnlyQuery(typeof(Order));
				dbOnlyResult.AddSubQuery(subQueries, JoinCondition.And);

				query.AddToFilter(dbOnlyResult);
			}
		}

		protected override ZQuery VesselInformationFilter
		{
			get
			{
				if (!Vessel.IsEmpty)
				{
					ZDBOnlySubQuery voyageSubQuery = new ZDBOnlySubQuery(typeof(JobVoyage), JobVoyOriginSchema.JA_JV);
					voyageSubQuery.AddToFilter(JobVoyageSchema.JV_RV_NKVessel, SQLComparisonOperator.Equal, Vessel);

					ZDBOnlySubQuery originSubQuery = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
					originSubQuery.AddSubQuery(voyageSubQuery, JoinCondition.And);

					ZDBOnlySubQuery sailingSubQuery = new ZDBOnlySubQuery(typeof(JobSailing), JobSailingSchema.PK);
					sailingSubQuery.AddSubQuery(originSubQuery, JoinCondition.And);

					ZDBOnlySubQuery transportQuery = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
					transportQuery.AddSubQuery(JobConsolTransportSchema.JW_JX, sailingSubQuery, JoinCondition.And);

					ZDBOnlySubQuery consolSubQuery = new ZDBOnlySubQuery(typeof(ForwardingConsol), JobConShipLinkSchema.JN_JK);
					consolSubQuery.AddSubQuery(JobConsolSchema.PK, transportQuery, JoinCondition.And);

					ZDBOnlySubQuery pivotSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
					pivotSubQuery.AddSubQuery(consolSubQuery, JoinCondition.And);

					ZDBOnlySubQuery shipmentSubQuery = new ZDBOnlySubQuery(typeof(CommonShipment), JobOrderHeaderSchema.JD_JS);
					shipmentSubQuery.AddSubQuery(pivotSubQuery, JoinCondition.And);

					ZDBOnlyQuery dbOnlyResult = new ZDBOnlyQuery(typeof(Order));
					dbOnlyResult.AddSubQuery(shipmentSubQuery, JoinCondition.And);
					dbOnlyResult.AddToFilter(JoinCondition.Or, JobOrderHeaderSchema.JD_RV_NKArrivalVessel, SQLComparisonOperator.Equal, Vessel);
					dbOnlyResult.AddToFilter(JoinCondition.Or, JobOrderHeaderSchema.JD_RV_NKIntermediateVessel, SQLComparisonOperator.Equal, Vessel);
					dbOnlyResult.AddToFilter(JoinCondition.Or, JobOrderHeaderSchema.JD_RV_NKDepartureVessel, SQLComparisonOperator.Equal, Vessel);

					return dbOnlyResult;
				}
				else
				{
					return base.VesselInformationFilter;
				}
			}
		}

		protected override ZQuery VoyageInformationFilter
		{
			get
			{
				if (!Voyage.IsEmpty)
				{
					ZDBOnlySubQuery voyageSubQuery = new ZDBOnlySubQuery(typeof(JobVoyage), JobVoyOriginSchema.JA_JV);
					voyageSubQuery.AddToFilter(JobVoyageSchema.JV_VoyageFlight, SQLComparisonOperator.Equal, Voyage);

					ZDBOnlySubQuery originSubQuery = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
					originSubQuery.AddSubQuery(voyageSubQuery, JoinCondition.And);

					ZDBOnlySubQuery sailingQuery = new ZDBOnlySubQuery(typeof(JobSailing), JobSailingSchema.PK);
					sailingQuery.AddSubQuery(originSubQuery, JoinCondition.And);

					ZDBOnlySubQuery transportSubQuery = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
					transportSubQuery.AddSubQuery(JobConsolTransportSchema.JW_JX, sailingQuery, JoinCondition.And);

					ZDBOnlySubQuery consolSubQuery = new ZDBOnlySubQuery(typeof(ForwardingConsol), JobConShipLinkSchema.JN_JK);
					consolSubQuery.AddSubQuery(JobConsolSchema.PK, transportSubQuery, JoinCondition.And);

					ZDBOnlySubQuery pivotSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
					pivotSubQuery.AddSubQuery(consolSubQuery, JoinCondition.And);

					ZDBOnlySubQuery shipmentSubQuery = new ZDBOnlySubQuery(typeof(CommonShipment), JobOrderHeaderSchema.JD_JS);
					shipmentSubQuery.AddSubQuery(pivotSubQuery, JoinCondition.And);

					ZDBOnlyQuery dbOnlyResult = new ZDBOnlyQuery(typeof(Order));
					dbOnlyResult.AddSubQuery(shipmentSubQuery, JoinCondition.And);

					return dbOnlyResult;
				}
				else
				{
					return base.VoyageInformationFilter;
				}
			}
		}

		public void AddCreatingUserToFilter(ZQuery query, ZGuid staff)
		{
			GlbStaff filteringStaff = Factory.Load<GlbStaff>(staff);
			query.AddToFilter(JobOrderHeaderSchema.JD_SystemCreateUser, filteringStaff.GS_Code);
		}

		#endregion

		#endregion
	}

	#endregion

	#region Auto (Only for Web)

	public abstract class AutoOrdersFilterBusinessObject : FilterBusinessObject, IObsoleteValidation
	{
		#region Schema

		public abstract class Schema
		{
			public const string TableName = "OrdersFilterBusinessObject";
			public const string PK = "PK";

			public const string FilterOperator = "FilterOperator";
			public const string JD_ContainerMode = "JD_ContainerMode";
			public const string JD_DateFilterType = "JD_DateFilterType";
			public const string JD_FromDate = "JD_FromDate";
			public const string JD_IsCancelled = "JD_IsCancelled";
			public const string JD_JO_LineStatus = "JD_JO_LineStatus";
			public const string JD_Number = "JD_Number";
			public const string JD_NumberFilterType = "JD_NumberFilterType";
			public const string JD_OH_Org1 = "JD_OH_Org1";
			public const string JD_OH_Org2 = "JD_OH_Org2";
			public const string JD_OrderStatus = "JD_OrderStatus";
			public const string JD_OrgFilterType = "JD_OrgFilterType";
			public const string JD_PortFilterType = "JD_PortFilterType";
			public const string JD_RL_NKPort1 = "JD_RL_NKPort1";
			public const string JD_RL_NKPort2 = "JD_RL_NKPort2";
			public const string JD_ToDate = "JD_ToDate";
			public const string JD_TransportMode = "JD_TransportMode";
			public const string Vessel = "Vessel";
			public const string Voyage = "Voyage";
			public const string ShowAttachedOrders = "ShowAttachedOrders";
			public const string ShowUnAttachedOrders = "ShowUnAttachedOrders";
			public const string StaffFilter = "StaffFilter";
			public const string StaffFilterOption = "StaffFilterOption";
		}

		#endregion

		protected AutoOrdersFilterBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region PK

		public override string PK_ColumnName
		{
			get { return Schema.PK; }
		}

		#endregion

		#region Properties

		#region FilterOperator

		public virtual SQLComparisonOperator FilterOperator
		{
			get { return ((SQLComparisonOperator)((IBusinessObjectInternals)this).GetValueFromRowSafely(FilterOperatorInfo)); }
			set
			{
				SetPropertyValue(FilterOperatorInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateFilterOperator();
				}
			}
		}

		public virtual void ValidateFilterOperator()
		{
			FilterOperatorInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo FilterOperatorInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.FilterOperator); }
		}

		#endregion

		#region JD_ContainerMode

		[MaxLength(3)]
		public virtual ZString JD_ContainerMode
		{
			get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(JD_ContainerModeInfo)); }
			set
			{
				value = value.TrimEnd(' ');
				CheckMaximumLength(JD_ContainerModeInfo, value);
				SetPropertyValue(JD_ContainerModeInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_ContainerMode();
				}
			}
		}

		public virtual void ValidateJD_ContainerMode()
		{
			JD_ContainerModeInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo JD_ContainerModeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.JD_ContainerMode); }
		}

		#endregion

		#region JD_DateFilterType

		[MaxLength(40)]
		public virtual ZString JD_DateFilterType
		{
			get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(JD_DateFilterTypeInfo)); }
			set
			{
				value = value.TrimEnd(' ');
				CheckMaximumLength(JD_DateFilterTypeInfo, value);
				SetQueryProviderParameterPropertyValue(JD_DateFilterTypeInfo, value, Schema.JD_DateFilterType, Schema.JD_FromDate, Schema.JD_ToDate);
				if (!IsValidationSuspended)
				{
					ValidateJD_DateFilterType();
				}
			}
		}

		public virtual void ValidateJD_DateFilterType()
		{
			JD_DateFilterTypeInfo.ClearAllNotifications();
			System.ComponentModel.PropertyDescriptor listProperty = System.ComponentModel.TypeDescriptor.GetProperties(this)["JD_DateFilterType_List"];     // This code is auto-generated
			if (listProperty == null)
			{
				ZArchitecture.Environment.Globals.Message.ShowDeveloperErrorOnce("ListPropertyNotFoundJD_DateFilterType_List", "List property JD_DateFilterType_List could not be found. Make sure this is declared in your Filter Business Object", "Error");
			}
			else
			{
				ICodeDescriptionPairList list = (ICodeDescriptionPairList)listProperty.GetValue(this);
				ListValidation.ErrorIfInvalidCode(JD_DateFilterTypeInfo, list);
			}
		}

		public virtual ZPropertyInfo JD_DateFilterTypeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.JD_DateFilterType); }
		}

		#endregion

		#region JD_FromDate

		public virtual ZDateTime JD_FromDate
		{
			get { return new ZDateTime(((IBusinessObjectInternals)this).GetValueFromRowSafely(JD_FromDateInfo)); }
			set
			{
				SetPropertyValue(JD_FromDateInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_FromDate();
				}
			}
		}

		public virtual void ValidateJD_FromDate()
		{
			JD_FromDateInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(JD_FromDateInfo);
		}

		public virtual ZPropertyInfo JD_FromDateInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.JD_FromDate); }
		}

		protected bool JD_FromDate_ReadOnly
		{
			get { return JD_DateFilterType == QueryDeciderNoSelectionCode; }
		}

		#endregion

		#region JD_IsCancelled

		public virtual ZBool JD_IsCancelled
		{
			get { return new ZBool(((IBusinessObjectInternals)this).GetValueFromRowSafely(JD_IsCancelledInfo)); }
			set
			{
				SetPropertyValue(JD_IsCancelledInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_IsCancelled();
				}
			}
		}

		public virtual void ValidateJD_IsCancelled()
		{
			JD_IsCancelledInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo JD_IsCancelledInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.JD_IsCancelled); }
		}

		#endregion

		#region JD_JO_LineStatus

		[MaxLength(3)]
		public virtual ZString JD_JO_LineStatus
		{
			get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(JD_JO_LineStatusInfo)); }
			set
			{
				value = value.TrimEnd(' ');
				CheckMaximumLength(JD_JO_LineStatusInfo, value);
				SetPropertyValue(JD_JO_LineStatusInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_JO_LineStatus();
				}
			}
		}

		public virtual void ValidateJD_JO_LineStatus()
		{
			JD_JO_LineStatusInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo JD_JO_LineStatusInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.JD_JO_LineStatus); }
		}

		#endregion

		#region JD_Number

		public virtual ZString JD_Number
		{
			get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(JD_NumberInfo)); }
			set
			{
				value = value.TrimEnd(' ');
				CheckMaximumLength(JD_NumberInfo, value);
				SetPropertyValue(JD_NumberInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_Number();
				}
			}
		}

		public virtual void ValidateJD_Number()
		{
			JD_NumberInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo JD_NumberInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.JD_Number); }
		}

		#endregion

		#region JD_NumberFilterType

		[MaxLength(40)]
		public virtual ZString JD_NumberFilterType
		{
			get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(JD_NumberFilterTypeInfo)); }
			set
			{
				value = value.TrimEnd(' ');
				CheckMaximumLength(JD_NumberFilterTypeInfo, value);
				SetQueryProviderParameterPropertyValue(JD_NumberFilterTypeInfo, value, Schema.JD_NumberFilterType, Schema.JD_Number);
				if (!IsValidationSuspended)
				{
					ValidateJD_NumberFilterType();
				}
			}
		}

		public virtual void ValidateJD_NumberFilterType()
		{
			JD_NumberFilterTypeInfo.ClearAllNotifications();
			System.ComponentModel.PropertyDescriptor listProperty = System.ComponentModel.TypeDescriptor.GetProperties(this)["JD_NumberFilterType_List"];       // This code is auto-generated
			if (listProperty == null)
			{
				ZArchitecture.Environment.Globals.Message.ShowDeveloperErrorOnce("ListPropertyNotFoundJD_NumberFilterType_List", "List property JD_NumberFilterType_List could not be found. Make sure this is declared in your Filter Business Object", "Error");
			}
			else
			{
				ICodeDescriptionPairList list = (ICodeDescriptionPairList)listProperty.GetValue(this);
				ListValidation.ErrorIfInvalidCode(JD_NumberFilterTypeInfo, list);
			}
		}

		public virtual ZPropertyInfo JD_NumberFilterTypeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.JD_NumberFilterType); }
		}

		#endregion

		#region JD_OH_Org1

		public virtual ZGuid JD_OH_Org1
		{
			get { return new ZGuid(((IBusinessObjectInternals)this).GetValueFromRowSafely(JD_OH_Org1Info)); }
			set
			{
				SetPropertyValue(JD_OH_Org1Info, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_OH_Org1();
				}
			}
		}

		public virtual void ValidateJD_OH_Org1()
		{
			JD_OH_Org1Info.ClearAllNotifications();
			TypeValidation.CheckValidGuid(JD_OH_Org1Info);
		}

		public virtual ZPropertyInfo JD_OH_Org1Info
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.JD_OH_Org1); }
		}

		#endregion

		#region JD_OH_Org2

		public virtual ZGuid JD_OH_Org2
		{
			get { return new ZGuid(((IBusinessObjectInternals)this).GetValueFromRowSafely(JD_OH_Org2Info)); }
			set
			{
				SetPropertyValue(JD_OH_Org2Info, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_OH_Org2();
				}
			}
		}

		public virtual void ValidateJD_OH_Org2()
		{
			JD_OH_Org2Info.ClearAllNotifications();
			TypeValidation.CheckValidGuid(JD_OH_Org2Info);
		}

		public virtual ZPropertyInfo JD_OH_Org2Info
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.JD_OH_Org2); }
		}

		#endregion

		#region JD_OrderStatus

		[MaxLength(3)]
		public virtual ZString JD_OrderStatus
		{
			get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(JD_OrderStatusInfo)); }
			set
			{
				value = value.TrimEnd(' ');
				CheckMaximumLength(JD_OrderStatusInfo, value);
				SetPropertyValue(JD_OrderStatusInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_OrderStatus();
				}
			}
		}

		public virtual void ValidateJD_OrderStatus()
		{
			JD_OrderStatusInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo JD_OrderStatusInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.JD_OrderStatus); }
		}

		#endregion

		#region JD_OrgFilterType

		[MaxLength(40)]
		public virtual ZString JD_OrgFilterType
		{
			get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(JD_OrgFilterTypeInfo)); }
			set
			{
				value = value.TrimEnd(' ');
				CheckMaximumLength(JD_OrgFilterTypeInfo, value);
				SetQueryProviderParameterPropertyValue(JD_OrgFilterTypeInfo, value, Schema.JD_OrgFilterType, Schema.JD_OH_Org1, Schema.JD_OH_Org2);
				if (!IsValidationSuspended)
				{
					ValidateJD_OrgFilterType();
				}
			}
		}

		public virtual void ValidateJD_OrgFilterType()
		{
			JD_OrgFilterTypeInfo.ClearAllNotifications();
			System.ComponentModel.PropertyDescriptor listProperty = System.ComponentModel.TypeDescriptor.GetProperties(this)["JD_OrgFilterType_List"];      // This code is auto-generated
			if (listProperty == null)
			{
				ZArchitecture.Environment.Globals.Message.ShowDeveloperErrorOnce("ListPropertyNotFoundJD_OrgFilterType_List", "List property JD_OrgFilterType_List could not be found. Make sure this is declared in your Filter Business Object", "Error");
			}
			else
			{
				ICodeDescriptionPairList list = (ICodeDescriptionPairList)listProperty.GetValue(this);
				ListValidation.ErrorIfInvalidCode(JD_OrgFilterTypeInfo, list);
			}
		}

		public virtual ZPropertyInfo JD_OrgFilterTypeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.JD_OrgFilterType); }
		}

		#endregion

		#region JD_PortFilterType

		[MaxLength(40)]
		public virtual ZString JD_PortFilterType
		{
			get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(JD_PortFilterTypeInfo)); }
			set
			{
				value = value.TrimEnd(' ');
				CheckMaximumLength(JD_PortFilterTypeInfo, value);
				SetQueryProviderParameterPropertyValue(JD_PortFilterTypeInfo, value, Schema.JD_PortFilterType, Schema.JD_RL_NKPort1, Schema.JD_RL_NKPort2);
				if (!IsValidationSuspended)
				{
					ValidateJD_PortFilterType();
				}
			}
		}

		public virtual void ValidateJD_PortFilterType()
		{
			JD_PortFilterTypeInfo.ClearAllNotifications();
			System.ComponentModel.PropertyDescriptor listProperty = System.ComponentModel.TypeDescriptor.GetProperties(this)["JD_PortFilterType_List"];     // This code is auto-generated
			if (listProperty == null)
			{
				ZArchitecture.Environment.Globals.Message.ShowDeveloperErrorOnce("ListPropertyNotFoundJD_PortFilterType_List", "List property JD_PortFilterType_List could not be found. Make sure this is declared in your Filter Business Object", "Error");
			}
			else
			{
				ICodeDescriptionPairList list = (ICodeDescriptionPairList)listProperty.GetValue(this);
				ListValidation.ErrorIfInvalidCode(JD_PortFilterTypeInfo, list);
			}
		}

		public virtual ZPropertyInfo JD_PortFilterTypeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.JD_PortFilterType); }
		}

		#endregion

		#region JD_RL_NKPort1

		public virtual ZString JD_RL_NKPort1
		{
			get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(JD_RL_NKPort1Info)); }
			set
			{
				value = value.TrimEnd(' ');
				CheckMaximumLength(JD_RL_NKPort1Info, value);
				SetPropertyValue(JD_RL_NKPort1Info, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_RL_NKPort1();
				}
			}
		}

		public virtual void ValidateJD_RL_NKPort1()
		{
			JD_RL_NKPort1Info.ClearAllNotifications();
		}

		public virtual ZPropertyInfo JD_RL_NKPort1Info
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.JD_RL_NKPort1); }
		}

		#endregion

		#region JD_RL_NKPort2

		public virtual ZString JD_RL_NKPort2
		{
			get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(JD_RL_NKPort2Info)); }
			set
			{
				value = value.TrimEnd(' ');
				CheckMaximumLength(JD_RL_NKPort2Info, value);
				SetPropertyValue(JD_RL_NKPort2Info, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_RL_NKPort2();
				}
			}
		}

		public virtual void ValidateJD_RL_NKPort2()
		{
			JD_RL_NKPort2Info.ClearAllNotifications();
		}

		public virtual ZPropertyInfo JD_RL_NKPort2Info
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.JD_RL_NKPort2); }
		}

		#endregion

		#region JD_ToDate

		public virtual ZDateTime JD_ToDate
		{
			get { return new ZDateTime(((IBusinessObjectInternals)this).GetValueFromRowSafely(JD_ToDateInfo)); }
			set
			{
				SetPropertyValue(JD_ToDateInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_ToDate();
				}
			}
		}

		public virtual void ValidateJD_ToDate()
		{
			JD_ToDateInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(JD_ToDateInfo);
		}

		public virtual ZPropertyInfo JD_ToDateInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.JD_ToDate); }
		}

		protected bool JD_ToDate_ReadOnly
		{
			get { return JD_DateFilterType == QueryDeciderNoSelectionCode; }
		}

		#endregion

		#region JD_TransportMode

		[MaxLength(3)]
		public virtual ZString JD_TransportMode
		{
			get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(JD_TransportModeInfo)); }
			set
			{
				value = value.TrimEnd(' ');
				CheckMaximumLength(JD_TransportModeInfo, value);
				SetPropertyValue(JD_TransportModeInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateJD_TransportMode();
				}
			}
		}

		public virtual void ValidateJD_TransportMode()
		{
			JD_TransportModeInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo JD_TransportModeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.JD_TransportMode); }
		}

		#endregion

		#region Vessel

		public virtual ZString Vessel
		{
			get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(VesselInfo)); }
			set
			{
				value = value.TrimEnd(' ');
				CheckMaximumLength(VesselInfo, value);
				SetPropertyValue(VesselInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateVessel();
				}
			}
		}

		public virtual void ValidateVessel()
		{
			VesselInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo VesselInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.Vessel); }
		}

		#endregion

		#region Voyage

		public virtual ZString Voyage
		{
			get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(VoyageInfo)); }
			set
			{
				value = value.TrimEnd(' ');
				CheckMaximumLength(VoyageInfo, value);
				SetPropertyValue(VoyageInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateVoyage();
				}
			}
		}

		public virtual void ValidateVoyage()
		{
			VoyageInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo VoyageInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.Voyage); }
		}

		#endregion

		#region ShowAttachedOrders

		public virtual ZBool ShowAttachedOrders
		{
			get { return new ZBool(((IBusinessObjectInternals)this).GetValueFromRowSafely(ShowAttachedOrdersInfo)); }
			set
			{
				SetPropertyValue(ShowAttachedOrdersInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateShowAttachedOrders();
				}
			}
		}

		public virtual void ValidateShowAttachedOrders()
		{
			ShowAttachedOrdersInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo ShowAttachedOrdersInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.ShowAttachedOrders); }
		}

		#endregion

		#region ShowUnAttachedOrders

		public virtual ZBool ShowUnAttachedOrders
		{
			get { return new ZBool(((IBusinessObjectInternals)this).GetValueFromRowSafely(ShowUnAttachedOrdersInfo)); }
			set
			{
				SetPropertyValue(ShowUnAttachedOrdersInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateShowUnAttachedOrders();
				}
			}
		}

		public virtual void ValidateShowUnAttachedOrders()
		{
			ShowUnAttachedOrdersInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo ShowUnAttachedOrdersInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.ShowUnAttachedOrders); }
		}
		#endregion

		#endregion

		#region Set Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			((IBusinessObjectInternals)this).Row[Schema.FilterOperator] = SQLComparisonOperator.NotSpecified;
			((IBusinessObjectInternals)this).Row[Schema.JD_ContainerMode] = "";
			((IBusinessObjectInternals)this).Row[Schema.JD_DateFilterType] = "";
			((IBusinessObjectInternals)this).Row[Schema.JD_FromDate] = DBNull.Value;
			((IBusinessObjectInternals)this).Row[Schema.JD_IsCancelled] = false;
			((IBusinessObjectInternals)this).Row[Schema.JD_JO_LineStatus] = "";
			((IBusinessObjectInternals)this).Row[Schema.JD_Number] = "";
			((IBusinessObjectInternals)this).Row[Schema.JD_NumberFilterType] = "";
			((IBusinessObjectInternals)this).Row[Schema.JD_OH_Org1] = DBNull.Value;
			((IBusinessObjectInternals)this).Row[Schema.JD_OH_Org2] = DBNull.Value;
			((IBusinessObjectInternals)this).Row[Schema.JD_OrderStatus] = "";
			((IBusinessObjectInternals)this).Row[Schema.JD_OrgFilterType] = "";
			((IBusinessObjectInternals)this).Row[Schema.JD_PortFilterType] = "";
			((IBusinessObjectInternals)this).Row[Schema.JD_RL_NKPort1] = "";
			((IBusinessObjectInternals)this).Row[Schema.JD_RL_NKPort2] = "";
			((IBusinessObjectInternals)this).Row[Schema.JD_ToDate] = DBNull.Value;
			((IBusinessObjectInternals)this).Row[Schema.JD_TransportMode] = "";
			((IBusinessObjectInternals)this).Row[Schema.Vessel] = "";
			((IBusinessObjectInternals)this).Row[Schema.Voyage] = "";
			((IBusinessObjectInternals)this).Row[Schema.ShowUnAttachedOrders] = false;
			((IBusinessObjectInternals)this).Row[Schema.ShowAttachedOrders] = false;
			((IBusinessObjectInternals)this).Row[Schema.StaffFilter] = DBNull.Value;
			((IBusinessObjectInternals)this).Row[Schema.StaffFilterOption] = "";
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			ValidateFilterOperator();
			ValidateJD_ContainerMode();
			ValidateJD_DateFilterType();
			ValidateJD_FromDate();
			ValidateJD_IsCancelled();
			ValidateJD_JO_LineStatus();
			ValidateJD_Number();
			ValidateJD_NumberFilterType();
			ValidateJD_OH_Org1();
			ValidateJD_OH_Org2();
			ValidateJD_OrderStatus();
			ValidateJD_OrgFilterType();
			ValidateJD_PortFilterType();
			ValidateJD_RL_NKPort1();
			ValidateJD_RL_NKPort2();
			ValidateJD_ToDate();
			ValidateJD_TransportMode();
			ValidateVessel();
			ValidateVoyage();
			ValidateShowAttachedOrders();
			ValidateShowUnAttachedOrders();
			ValidateStaffFilter();
			ValidateStaffFilterOption();

			base.RunPreSaveValidationCore(); // call RunPreSaveValidation() on all children then fire OnNotificationsChanged()
		}

		#endregion

		public override void OnLoaded()
		{
			base.OnLoaded();
			UpdateReadOnlyOnQueryDeciderParameter(Schema.JD_NumberFilterType, Schema.JD_Number);
			UpdateReadOnlyOnQueryDeciderParameter(Schema.JD_PortFilterType, Schema.JD_RL_NKPort1, Schema.JD_RL_NKPort2);
			UpdateReadOnlyOnQueryDeciderParameter(Schema.JD_OrgFilterType, Schema.JD_OH_Org1, Schema.JD_OH_Org2);
			UpdateReadOnlyOnQueryDeciderParameter(Schema.JD_DateFilterType, Schema.JD_FromDate, Schema.JD_ToDate);
		}

		#region Filters

		public override ZQuery Filter
		{
			get
			{
				ZQuery query = new ZQuery();
				query.DefaultJoinCondition = JoinCondition.And;

				query.AddToFilter(StaffFilterPanelFilter);
				query.AddToFilter(ShowCancelledPanelFilter);
				query.AddToFilter(JD_NumberBoundNumberFilterControlFilter);
				query.AddToFilter(VesselPanelFilter);
				query.AddToFilter(VoyageInformationFilter);
				query.AddToFilter(JD_PortFilterTypeCodeFilterControlFilter);
				query.AddToFilter(JD_OrgFilterTypeGuidFilterControlFilter);
				query.AddToFilter(JD_DateTimeFilterControlFilter);
				query.AddToFilter(JD_OrderStatusDropEditPanelFilter);
				AddIfNotEmpty(query, JobOrderHeaderSchema.JD_ContainerMode, SQLComparisonOperator.Equal, JD_ContainerMode);
				AddIfNotEmpty(query, JobOrderHeaderSchema.JD_TransportMode, SQLComparisonOperator.Equal, JD_TransportMode);

				return query;
			}
		}

		protected virtual ZQuery StaffFilterPanelFilter
		{
			get
			{
				ZQuery query = new ZQuery();
				query.DefaultJoinCondition = JoinCondition.And;

				return query;
			}
		}

		protected virtual ZQuery ShowCancelledPanelFilter
		{
			get
			{
				ZQuery query = new ZQuery();
				query.DefaultJoinCondition = JoinCondition.And;

				return query;
			}
		}

		protected virtual ZQuery JD_NumberBoundNumberFilterControlFilter
		{
			get
			{
				ZQuery query = new ZQuery();
				query.DefaultJoinCondition = JoinCondition.And;

				AddQueryProviderFilter(query, Schema.JD_NumberFilterType, "JD_NumberFilterType_List", SQLComparisonOperator.Contains, Schema.JD_Number, 0); // This code is auto-generated

				return query;
			}
		}

		protected virtual ZQuery VesselPanelFilter
		{
			get
			{
				ZQuery query = new ZQuery();
				query.DefaultJoinCondition = JoinCondition.And;

				query.AddToFilter(VesselInformationFilter);

				return query;
			}
		}

		protected virtual ZQuery VesselInformationFilter
		{
			get
			{
				ZQuery query = new ZQuery();
				query.DefaultJoinCondition = JoinCondition.And;

				return query;
			}
		}

		protected virtual ZQuery VoyageInformationFilter
		{
			get
			{
				ZQuery query = new ZQuery();
				query.DefaultJoinCondition = JoinCondition.And;

				return query;
			}
		}

		protected virtual ZQuery JD_PortFilterTypeCodeFilterControlFilter
		{
			get
			{
				ZQuery query = new ZQuery();
				query.DefaultJoinCondition = JoinCondition.And;

				AddQueryProviderFilter(query, Schema.JD_PortFilterType, "JD_PortFilterType_List", SQLComparisonOperator.Equal, Schema.JD_RL_NKPort1, 0); // This code is auto-generated
				AddQueryProviderFilter(query, Schema.JD_PortFilterType, "JD_PortFilterType_List", SQLComparisonOperator.Equal, Schema.JD_RL_NKPort2, 1); // This code is auto-generated

				return query;
			}
		}

		protected virtual ZQuery JD_OrgFilterTypeGuidFilterControlFilter
		{
			get
			{
				ZQuery query = new ZQuery();
				query.DefaultJoinCondition = JoinCondition.And;

				AddQueryProviderFilter(query, Schema.JD_OrgFilterType, "JD_OrgFilterType_List", SQLComparisonOperator.Equal, Schema.JD_OH_Org1, 0); // This code is auto-generated
				AddQueryProviderFilter(query, Schema.JD_OrgFilterType, "JD_OrgFilterType_List", SQLComparisonOperator.Equal, Schema.JD_OH_Org2, 1); // This code is auto-generated

				return query;
			}
		}

		protected virtual ZQuery JD_DateTimeFilterControlFilter
		{
			get
			{
				ZQuery query = new ZQuery();
				query.DefaultJoinCondition = JoinCondition.And;

				AddQueryProviderFilter(query, Schema.JD_DateFilterType, "JD_DateFilterType_List", SQLComparisonOperator.GreaterThanOrEqualTo, Schema.JD_FromDate, 0); // This code is auto-generated
				AddQueryProviderFilter(query, Schema.JD_DateFilterType, "JD_DateFilterType_List", SQLComparisonOperator.LessThanOrEqualToDatePartOnly, Schema.JD_ToDate, 1); // This code is auto-generated

				return query;
			}
		}

		protected virtual ZQuery JD_OrderStatusDropEditPanelFilter
		{
			get
			{
				ZQuery query = new ZQuery();
				query.DefaultJoinCondition = JoinCondition.And;

				AddIfNotEmpty(query, JobOrderHeaderSchema.JD_OrderStatus, SQLComparisonOperator.Equal, JD_OrderStatus);

				return query;
			}
		}

		#region StaffFilter

		public virtual ZGuid StaffFilter
		{
			get { return new ZGuid(((IBusinessObjectInternals)this).GetValueFromRowSafely(StaffFilterInfo)); }
			set
			{
				SetPropertyValue(StaffFilterInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateStaffFilter();
				}
			}
		}

		public virtual void ValidateStaffFilter()
		{
			StaffFilterInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(StaffFilterInfo);
		}

		public virtual ZPropertyInfo StaffFilterInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.StaffFilter); }
		}

		#endregion

		#region StaffFilterOption

		public virtual ZString StaffFilterOption
		{
			get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(StaffFilterOptionInfo)); }
			set
			{
				value = value.TrimEnd(' ');
				CheckMaximumLength(StaffFilterOptionInfo, value);
				SetPropertyValue(StaffFilterOptionInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateStaffFilterOption();
				}
			}
		}

		public virtual void ValidateStaffFilterOption()
		{
			StaffFilterOptionInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo StaffFilterOptionInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.StaffFilterOption); }
		}

		#endregion

		#endregion
	}

	#endregion
}

#endregion
