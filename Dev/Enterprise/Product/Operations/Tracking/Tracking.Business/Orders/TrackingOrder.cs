using System.Collections.Generic;
using System.Data;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Shared;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Tracking.Business
{
	/// <summary>
	/// Provides access to tracking related order details
	/// </summary>
	public class TrackingOrder : Order,
		IWebDocumentsWithUploadSupport,
		IBizOChangesEmailNotification,
		IWebUserVisibleNotesSupport,
		IWebUserEditableNoteSupport,
		IUpdatableMilestoneEventsProvider,
		IMilestonesProvider,
		ITrackingEventsProvider,
		IEventReferenceProvider
	{
		#region Schema

		public abstract new class Schema : Order.Schema
		{
			public const string Supplier_FullAddress = "Supplier_FullAddress";
			public const string SupplierCode = "SupplierCode";
			public const string SupplierName = "SupplierName";
			public const string BuyerName = "BuyerName";
			public const string JD_ActualWeightWithUnits = "JD_ActualWeightWithUnits";
			public const string JD_ActualVolumeWithUnits = "JD_ActualVolumeWithUnits";
			public const string JD_PacksWithUnits = "JD_PacksWithUnits";
			public const string ShipDecActualWeightWithUnits = "ShipDecActualWeightWithUnits";
			public const string ShipDecActualVolumeWithUnits = "ShipDecActualVolumeWithUnits";
			public const string ShipDecPacksWithUnits = "ShipDecPacksWithUnits";
			public const string JD_OrderStatusDesc = "JD_OrderStatusDesc";
			public const string JD_CalcShipmentBrokeragePK = "JD_CalcShipmentBrokeragePK";
			public const string JD_CalcShipmentBrokerageTable = "JD_CalcShipmentBrokerageTable";
			public const string ETDWithSuppression = "ETDWithSuppression";
			public const string ETAWithSuppression = "ETAWithSuppression";
			public const string ATDWithSuppression = "ATDWithSuppression";
			public const string ATAWithSuppression = "ATAWithSuppression";

			public const string CurrentVessel = "CurrentVessel";
			public const string CurrentVoyageWithSuppression = "CurrentVoyageWithSuppression";
			public const string MainVessel = "MainVessel";
			public const string MainVoyageWithSuppression = "MainVoyageWithSuppression";

			public const string ArrivalVoyageWithSuppression = "ArrivalVoyageWithSuppression";
			public const string ArrivalETDWithSuppression = "ArrivalETDWithSuppression";
			public const string IntermediateETDWithSuppression = "IntermediateETDWithSuppression";
			public const string IntermediateETAWithSuppression = "IntermediateETAWithSuppression";
			public const string IntermediateVoyageWithSuppression = "IntermediateVoyageWithSuppression";
			public const string DepartureETAWithSuppression = "DepartureETAWithSuppression";
			public const string DepartureVoyageWithSuppression = "DepartureVoyageWithSuppression";

			public const string ShipOrDecPK = "ShipOrDecPK";
			public const string ShipOrDecNumber = "ShipOrDecNumber";
			public const string ShipOrDecTableName = "ShipOrDecTableName";
			public const string PickupAddressAsString = "PickupAddressAsString";
			public const string DeliveryAddressAsString = "DeliveryAddressAsString";
			public const string ConsolsAsString = "ConsolsAsString";
			public const string CreatedOn = "CreatedOn";
			public const string ServiceLevel = "ServiceLevel";
		}

		#endregion Schema

		public TrackingOrder(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			WebServiceLevelCollection = () => new WebServiceLevelCollection(factory, GetTemporaryPublishedServiceLevelQuery(), JoinCondition.Or);
		}

		internal ZQuery GetTemporaryPublishedServiceLevelQuery()
		{
			ZQuery result = new ZQuery();
			if (!TemporaryServiceLevelDescription.IsEmpty)
			{
				result.AddToFilter(RefServiceLevelSchema.RS_Description, TemporaryServiceLevelDescription);
			}
			return result;
		}

		public ZString TemporaryServiceLevelDescription = ZString.Empty;

		#region OrderNumber

		public bool JD_OrderNumber_ReadOnly => JD_OrderNumberSplit != 0 || OrderSplitSiblings.Count > 0;

		#endregion

		#region Helper

		BusinessHelper Helper
		{
			get
			{
				if (fHelper == null)
				{
					fHelper = new BusinessHelper(Factory);
				}
				return fHelper;
			}
		}
		BusinessHelper fHelper;

		#endregion

		public static OrderCollection GetOrdersToBeAttachedForWebModule(BusinessObjectFactory factory, IAttachOrders orderParent)
		{
			ZGuid consignee = orderParent.ConsigneeDocumentaryAddress.OrganisationPK;
			ZGuid consignor = orderParent.ConsignorDocumentaryAddress.OrganisationPK;
			ZString transportMode = orderParent.TransportMode;

			ZQuery filter = new ZQuery(JobOrderHeaderSchema.JD_JS, null);
			filter.AddToFilter(JobOrderHeaderSchema.JD_IsCancelled, SQLComparisonOperator.Equal, ZBool.False);
			if (!transportMode.IsEmpty)
			{
				filter.AddToFilter(JoinCondition.And, JobOrderHeaderSchema.JD_TransportMode, SQLComparisonOperator.Equal, transportMode);
			}

			OrderCollection result = new OrderCollection(orderParent.Factory, filter);
			if (!consignee.IsValid && !consignor.IsValid)
			{
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("JD_OrgFilterType", new ZString(OrdersConstants.OrgFilterTypes.None))); // This is not a database field. It is a property on the FilterBusinessObject which we cannot access using the schema.
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("JD_OH_Org1", ZGuid.Empty)); // This is not a database field. It is a property on the FilterBusinessObject which we cannot access using the schema.
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("JD_OH_Org2", ZGuid.Empty)); // This is not a database field. It is a property on the FilterBusinessObject which we cannot access using the schema.
			}
			else
			{
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("JD_OrgFilterType", new ZString(OrdersConstants.OrgFilterTypes.BuyerSupplier))); // This is not a database field. It is a property on the FilterBusinessObject which we cannot access using the schema.
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("JD_OH_Org1", consignee)); // This is not a database field. It is a property on the FilterBusinessObject which we cannot access using the schema.
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("JD_OH_Org2", consignor)); // This is not a database field. It is a property on the FilterBusinessObject which we cannot access using the schema.
			}
			if (!transportMode.IsEmpty)
			{
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("JD_TransportMode", transportMode)); // This is not a database field. It is a property on the FilterBusinessObject which we cannot access using the schema.
			}

			result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("ShowUnAttachedOrders", new ZBool(true)));

			return result;
		}

		protected override OrderLineCollection GetOrderLinesCore()
		{
			OrderLineCollection orderLinesCollection = base.GetOrderLinesCore();
			orderLinesCollection.AdditionalFilter = new ZQuery(JobOrderLineSchema.JO_LineStatus, SQLComparisonOperator.NotEqual, Constants.OrderStatus.Cancelled);
			return orderLinesCollection;
		}

		public static TrackingOrder FromPKFilteredByContact(BusinessObjectFactory factory, ZGuid pK, TrackingSiteUser siteUser)
		{
			return FromFieldFilteredByContact(factory, JobOrderHeaderSchema.PK, pK, siteUser);
		}

		public static TrackingOrder FromNumberFilteredByContact(BusinessObjectFactory factory, ZString number, TrackingSiteUser siteUser)
		{
			return FromFieldFilteredByContact(factory, JobOrderHeaderSchema.JD_OrderNumber, number, siteUser);
		}

		protected static TrackingOrder FromFieldFilteredByContact(BusinessObjectFactory factory, SchemaColumn column, object value, TrackingSiteUser siteUser)
		{
			TrackingOrder result = null;
			if (siteUser != null && siteUser.IsShipmentQuickViewUser) // We use it to enable direct view of SHipment Detalils by Housebill Number
			{
				result = factory.LoadTop1<TrackingOrder>(new ZQuery(column, value));
			}
			else
			{
				result = OrgRestrictionFilterFactory.LoadFilteredByContact<TrackingOrder>(factory, column, value);
			}
			if (result != null)
			{
				result.loggedInContact = siteUser.LoggedInUser;
			}
			return result;
		}

		public CustomLabelInfoList GetAdditionalInformationFields()
		{
			CustomLabelsProvider provider = new CustomLabelsProvider(this, false);
			CustomLabelInfoList customLabels = provider.GetCustomFields(provider.ConfigOrgProvider.ConfigOrg, Factory);

			CustomLabelInfoList customLabelInfoList = new CustomLabelInfoList(typeof(Order), provider.ConfigOrgProvider.ConfigOrg, ResString.GetMultilingualString("a455b412-3f77-4f9b-b8a1-1d80f32a587a", "the buyer of the order"), Factory);

			foreach (CustomLabelInfo field in customLabels)
			{
				if (field.IsEnabled && field.LabelName.StartsWith("OrderHeader.")) // Custom Label Prefix
				{
					customLabelInfoList.Add(field);
				}
			}

			return customLabelInfoList;
		}

		#region PlannedVoyages

		enum Num { Zero, One, Two, Three }

		Num NumberOfVoyages
		{
			get
			{
				ZString departureVesselAndVoyage = JD_RV_NKDepartureVessel + JD_DepartureVoyage;
				ZString intermediateVesselAndVoyage = JD_RV_NKIntermediateVessel + JD_IntermediateVoyage;
				ZString arrivalVesselAndVoyage = JD_RV_NKArrivalVessel + JD_ArrivalVoyage;

				bool noVoyageDataEntered = departureVesselAndVoyage.IsEmpty && intermediateVesselAndVoyage.IsEmpty && arrivalVesselAndVoyage.IsEmpty;

				if (Suppression.EnabledForAnyOfFieldsWeb(this, new[] { SuppressFields.FlightNumber }) || noVoyageDataEntered)
				{
					return Num.Zero;
				}

				if (departureVesselAndVoyage == arrivalVesselAndVoyage && intermediateVesselAndVoyage.IsEmpty)
				{
					return Num.One;
				}

				return intermediateVesselAndVoyage.IsEmpty ? Num.Two : Num.Three;
			}
		}

		PlannedVoyage departure
		{
			get
			{
				return new PlannedVoyage
				{
					VoyageType = Res.GetString("d651abc6-da17-46dd-ac78-49e1f2476f03", "Departure"),
					Vessel = JD_RV_NKDepartureVessel,
					Voyage = DepartureVoyageWithSuppression,
					ETD = ETDWithSuppression,
					ETA = DepartureETAWithSuppression
				};
			}
		}

		PlannedVoyage intermediate
		{
			get
			{
				return new PlannedVoyage
				{
					VoyageType = Res.GetString("b8385188-f697-4fd8-a011-1e56b76f3c71", "Intermediate"),
					Vessel = JD_RV_NKIntermediateVessel,
					Voyage = IntermediateVoyageWithSuppression,
					ETD = IntermediateETDWithSuppression,
					ETA = IntermediateETAWithSuppression
				};
			}
		}

		PlannedVoyage arrival
		{
			get
			{
				return new PlannedVoyage
				{
					VoyageType = Res.GetString("89ea9c95-0acf-4d41-940a-4483b98ab5b0", "Arrival"),
					Vessel = JD_RV_NKArrivalVessel,
					Voyage = ArrivalVoyageWithSuppression,
					ETD = ArrivalETDWithSuppression,
					ETA = ETAWithSuppression
				};
			}
		}

		public PlannedVoyagesCollection PlannedVoyages
		{
			get
			{
				switch (NumberOfVoyages)
				{
					case Num.One:
						return new PlannedVoyagesCollection
		{
			new PlannedVoyage
				{
					VoyageType = Res.GetString("6d2b5d55-0b5f-410b-90f3-157f3bac1038", "Arrival"),
					Vessel = JD_RV_NKDepartureVessel,
					Voyage = DepartureVoyageWithSuppression,
					ETD = ETDWithSuppression,
					ETA = ETAWithSuppression
				}
		};

					case Num.Two:
						return new PlannedVoyagesCollection { departure, arrival };

					case Num.Three:
						return new PlannedVoyagesCollection { departure, intermediate, arrival };

					default:
						return new PlannedVoyagesCollection();
				}
			}
		}

		#endregion

		protected override ZString CurrentCountryCode
		{
			get { return LoggedInContact != null ? LoggedInContact.ParentOrg.CountryCode : ZString.Empty; }
		}

		#region Pre Save Validation

		protected override void RunPreSaveValidationCore()
		{
			FixHiddenFieldsValuesBeforeSaving();
		}

		protected virtual void FixHiddenFieldsValuesBeforeSaving()
		{
			if (JD_OH_SendingAgentInfo.HasErrors())
			{
				JD_OH_SendingAgent = ZGuid.Empty;
			}
			if (JD_OH_ReceivingAgentInfo.HasErrors())
			{
				JD_OH_ReceivingAgent = ZGuid.Empty;
			}
		}

		#endregion

		#region Related Business Object Overrides

		public ZGuid JD_CalcShipmentBrokeragePK
		{
			get
			{
				if (IsShipmentAttached && Shipment != null)
				{
					return Shipment.PK;
				}
				else if (IsDeclarationAttached && Declaration != null)
				{
					return Declaration.PK;
				}

				return ZGuid.Empty;
			}
		}

		public ZPropertyInfo JD_CalcShipmentBrokeragePKInfo
		{
			get { return GetZPropertyInfo(Schema.JD_CalcShipmentBrokeragePK); }
		}

		public ZString JD_CalcShipmentBrokerageTable
		{
			get
			{
				if (IsShipmentAttached && Shipment != null)
				{
					return JobShipmentSchema.Constants.TableName;
				}
				else if (IsDeclarationAttached && Declaration != null)
				{
					return JobDeclarationSchema.Constants.TableName;
				}

				return ZString.Empty;
			}
		}

		public ZPropertyInfo JD_CalcShipmentBrokerageTableInfo
		{
			get { return GetZPropertyInfo(Schema.JD_CalcShipmentBrokerageTable); }
		}

		public new TrackingShipment Shipment
		{
			get { return (TrackingShipment)base.Shipment; }
		}

		protected override ForwardingShipment LoadShipment()
		{
			return Factory.Load<TrackingShipment>(JD_JS);
		}

		#endregion Related Business Object Overrides

		#region Lookups

		protected override JobOrderHeaderLookups GetNewLookups()
		{
			return new TrackingOrderLookups(this);
		}

		#endregion Lookups

		#region Proxy Properties

		#region IShipmentDeclaration
		/// <summary>
		/// Shipment or delcaration we bind to
		/// </summary>
		public IShipmentDeclaration ShipOrDec
		{
			get
			{
				if (shipOrDec == null)
				{
					if (Shipment != null)
					{
						shipOrDec = Shipment;
					}
					else if (Declaration != null)
					{
						shipOrDec = new TrackingDeclaration((BaseJobDeclaration)Declaration);
					}
				}
				return shipOrDec;
			}
		}
		internal protected IShipmentDeclaration shipOrDec;

		#endregion ShipDec

		#region Supplier_FullAddress

		public ZString Supplier_FullAddress
		{
			get
			{
				StringBuilder result = new StringBuilder();
				if (Supplier != null)
				{
					IOrgHeader organisation = Supplier;

					if (!organisation.Address1.IsEmpty)
					{
						result.Append(organisation.Address1 + System.Environment.NewLine);
					}

					if (!organisation.Address2.IsEmpty)
					{
						result.Append(organisation.Address2 + System.Environment.NewLine);
					}

					if (!organisation.UNLOCO.IsEmpty)
					{
						result.Append(organisation.UNLOCO);
					}
				}
				return (result.Length == 0) ? OrganisationMessages.NoAddressFoundOnFile : result.ToString();
			}
		}

		public ZPropertyInfo Supplier_FullAddressInfo
		{
			get { return GetZPropertyInfo(Schema.Supplier_FullAddress); }
		}

		abstract class OrganisationMessages
		{
			public static string NoFullNameFoundOnFile
			{
				get { return Res.GetString("a120ca13-65ae-4f78-9152-83678f10ed8a", "*NO NAME FOUND*"); }
			}
			public static string NoAddressFoundOnFile
			{
				get { return Res.GetString("952e89f9-327a-4974-bff6-668bb4971613", "*NO ADDRESS FOUND*"); }
			}
			public static string NoPhoneFoundOnFile
			{
				get { return Res.GetString("0cf74748-1994-4ead-add5-6457183714fe", "Ph: *NOT FOUND*"); }
			}
			public static string NoFaxFoundOnFile
			{
				get { return Res.GetString("a8832b05-7fdc-499c-8d81-81346f3d99fb", "Fax: *NOT FOUND*"); }
			}
			public static string NoEmailFoundOnFile
			{
				get { return Res.GetString("4206fe78-715e-42d2-81e1-21783ccaf367", "Email: *NOT FOUND*"); }
			}
			public static string NoWebFoundOnFile
			{
				get { return Res.GetString("453a2170-4fca-448d-b8f9-05feb9cabcea", "Web: *NOT FOUND*"); }
			}
			public static string NoOrgIsSelected
			{
				get { return Res.GetString("a425c385-74cd-43da-8e27-8e2f94aa593b", "* NO ORGANIZATION IS SELECTED"); }
			}
		}

		#endregion Supplier_FullAddress

		#region SupplierCode

		[MaxLength(AutoOrgHeader.Schema.OH_CodeMaxLength)]
		public ZString SupplierCode
		{
			get
			{
				return (supplierCode.IsEmpty && Supplier != null) ? Supplier.OH_Code : supplierCode;
			}
			set
			{
				if (supplierCode != value)
				{
					CheckMaximumLength(SupplierCodeInfo, value);
					supplierCode = value;
					SupplierPK = Helper.GetPKFromOrgCode(value);
					SupplierCodeInfo.RefreshBinding();
					RefreshBinding();
				}
			}
		}
		ZString supplierCode;

		public ZPropertyInfo SupplierCodeInfo
		{
			get { return GetZPropertyInfo(Schema.SupplierCode); }
		}

		#endregion SupplierCode

		#region SupplierName

		public ZString SupplierName => SupplierAddress?.EffectiveCompanyName ?? ZString.Empty;

		public ZPropertyInfo SupplierNameInfo => GetZPropertyInfo(Schema.SupplierName);

		#endregion

		#region BuyerName

		public ZString BuyerName => BuyerAddress?.EffectiveCompanyName ?? ZString.Empty;

		public ZPropertyInfo BuyerNameInfo => GetZPropertyInfo(Schema.BuyerName);

		#endregion

		#endregion Proxy Properties

		#region Properties for the grid on the Orders Search Page

		#region Containers

		public PKDescriptionCollection Containers
		{
			get
			{
				PKDescriptionCollection result = new PKDescriptionCollection();

				if (Shipment != null)
				{
					foreach (ForwardingContainer container in Shipment.Containers)
					{
						result.Add(new PKDescription(container.PK, container.JC_ContainerNum));
					}
				}
				else if (Declaration != null)
				{
					foreach (BaseCusContainer container in ((BaseJobDeclaration)Declaration).CusContainers)
					{
						result.Add(new PKDescription(container.PK, container.CO_ContainerNumber) { DoNotCreateHyperLink = true });
					}
				}
				else
				{
					foreach (OrderContainer container in PlannedContainers)
					{
						result.Add(new PKDescription(container.PK, container.J1_ContainerNumber) { DoNotCreateHyperLink = true });
					}
				}

				return result;
			}
		}

		#endregion

		#region PlannedContainerNumbers

		public PKDescriptionCollection PlannedContainerNumbers
		{
			get
			{
				PKDescriptionCollection result = new PKDescriptionCollection();

				foreach (OrderContainer container in PlannedContainers)
				{
					result.Add(new PKDescription(container.PK, container.J1_ContainerNumber) { DoNotCreateHyperLink = true });
				}

				return result;
			}
		}

		#endregion

		#region Products

		public MasterFiles.Business.OrgSupplierPartCollection Products
		{
			get
			{
				var products = new MasterFiles.Business.OrgSupplierPartCollection(Factory);
				if (this.OrderLines != null)
				{
					foreach (OrderLine order in this.OrderLines)
					{
						if (order.Product != null)
						{
							products.AddRange(order.Product);
						}
					}
				}
				return products;
			}
		}

		#endregion

		#region ShipOrDecPK

		public ZGuid ShipOrDecPK
		{
			get { return ShipOrDec != null ? ShipOrDec.PersistentBizOPK : ZGuid.Empty; }
		}

		public ZPropertyInfo ShipOrDecPKInfo
		{
			get { return GetZPropertyInfo(Schema.ShipOrDecPK); }
		}

		#endregion

		#region ShipOrDecNumber

		public ZString ShipOrDecNumber
		{
			get { return ShipOrDec != null ? ShipOrDec.Number : ZString.Empty; }
		}

		public ZPropertyInfo ShipOrDecNumberInfo
		{
			get { return GetZPropertyInfo(Schema.ShipOrDecNumber); }
		}

		#endregion

		#region ShipOrDecTableName

		public ZString ShipOrDecTableName
		{
			get { return ShipOrDec != null ? ((BusinessObject)ShipOrDec).TableName : string.Empty; }
		}

		public ZPropertyInfo ShipOrDecTableNameInfo
		{
			get { return GetZPropertyInfo(Schema.ShipOrDecTableName); }
		}

		#endregion

		#region PickupAddressAsString

		public ZString PickupAddressAsString
		{
			get
			{
				ZString result = ZString.Empty;
				if (GoodsAvailableAtAddress.Address != null)
				{
					var formatter = new WebAddressFormatter(GoodsAvailableAtAddress);
					result = formatter.FormattedAddressWithCompanyName();
				}
				else if (Shipment != null && !Shipment.PickupFromFullAddress.IsEmpty)
				{
					result = Shipment.PickupFromFullAddress;
				}
				else if (ShipOrDec != null)
				{
					result = ShipOrDec.ConsignorFullAddress;
				}

				return result;
			}
		}

		public ZPropertyInfo PickupAddressAsStringInfo
		{
			get { return GetZPropertyInfo(Schema.PickupAddressAsString); }
		}

		#endregion

		#region DeliveryAddressAsString

		public ZString DeliveryAddressAsString
		{
			get
			{
				ZString result = ZString.Empty;
				if (GoodsDeliveredToAddress.Address != null)
				{
					var formatter = new WebAddressFormatter(GoodsDeliveredToAddress);
					result = formatter.FormattedAddressWithCompanyName();
				}
				else if (Shipment != null && !Shipment.DeliverToFullAddress.IsEmpty)
				{
					result = Shipment.DeliverToFullAddress;
				}
				else if (ShipOrDec != null)
				{
					result = ShipOrDec.ConsigneeFullAddress;
				}

				return result;
			}
		}

		public ZPropertyInfo DeliveryAddressAsStringInfo
		{
			get { return GetZPropertyInfo(Schema.DeliveryAddressAsString); }
		}

		#endregion

		#region ConsolsAsString

		public ZString ConsolsAsString
		{
			get
			{
				if (Shipment != null)
				{
					return ArrayToTextConverter.ConvertToCommaSeparatedMultilineText(Shipment.Consols, JobConsolSchema.JK_UniqueConsignRef.Name);
				}

				return ZString.Empty;
			}
		}

		public ZPropertyInfo ConsolsAsStringInfo
		{
			get { return GetZPropertyInfo(Schema.ConsolsAsString); }
		}

		#endregion

		#region CreatedOn

		public ZString CreatedOn
		{
			get { return JD_SystemCreateUser == "ZZ" ? (NoResString)"Web" : (NoResString)"Internal"; } // Audit Logging
		}

		public ZPropertyInfo CreatedOnInfo
		{
			get { return GetZPropertyInfo(Schema.CreatedOn); }
		}

		#endregion

		#region ServiceLevel

		public ZString ServiceLevel
		{
			get { return ShipOrDec != null ? ShipOrDec.ServiceLevelCode : base.JD_RS_NKServiceLevel_NI; }
		}

		public ZPropertyInfo ServiceLevelInfo
		{
			get { return GetZPropertyInfo(Schema.ServiceLevel); }
		}

		#endregion

		#endregion

		#region Property overrides

		/// <summary>
		/// If shipment/declaration is attached, get it from Shipment, otherwise from order planning tab
		/// </summary>

		#region Locations

		#region JD_RL_NKPortOfLoading

		public override ZString JD_RL_NKPortOfLoading
		{
			get
			{
				if (Shipment != null)
				{
					return Shipment.FirstTransportLoadPort;
				}

				if (Declaration != null)
				{
					return ((BaseJobDeclaration)Declaration).JE_RL_NKPortOfLoading;
				}

				return base.JD_RL_NKPortOfLoading;
			}
		}

		#endregion

		#region JD_RL_NKPortOfDischarge

		public override ZString JD_RL_NKPortOfDischarge
		{
			get
			{
				if (Shipment != null)
				{
					return Shipment.LastTransportDischargePort;
				}

				if (Declaration != null)
				{
					return ((BaseJobDeclaration)Declaration).JE_RL_NKPortOfArrival;
				}

				return base.JD_RL_NKPortOfDischarge;
			}
		}

		#endregion

		protected override void FixOverridesForSplitOrder(Order clonedOrder)
		{
			((TrackingOrder)clonedOrder).shipOrDec = null;

			clonedOrder.JD_RL_NKPortOfLoading = base.JD_RL_NKPortOfLoading;
			clonedOrder.JD_RL_NKPortOfDischarge = base.JD_RL_NKPortOfDischarge;
			clonedOrder.JD_RL_NKGoodsAvailableAt = base.JD_RL_NKGoodsAvailableAt;
			clonedOrder.JD_RL_NKGoodsDeliveredTo = base.JD_RL_NKGoodsDeliveredTo;

			clonedOrder.JD_OH_SendingAgent = base.JD_OH_SendingAgent;
			clonedOrder.JD_OH_ReceivingAgent = base.JD_OH_ReceivingAgent;

			clonedOrder.JD_Waybill = base.JD_Waybill;
			clonedOrder.JD_MasterWaybill = base.JD_MasterWaybill;
		}

		#endregion

		#region Organisations

		#region JD_OH_SendingAgent

		public override ZGuid JD_OH_SendingAgent
		{
			get { return ShipOrDec != null ? ShipOrDec.SendingForwarderPK : base.JD_OH_SendingAgent; }
			set { base.JD_OH_SendingAgent = value; }
		}

		OrgHeader OrderSendingAgent => Factory.Load<OrgHeader>(base.JD_OH_SendingAgent);

		#endregion

		#region JD_OH_ReceivingAgent

		public override ZGuid JD_OH_ReceivingAgent
		{
			get { return ShipOrDec != null ? ShipOrDec.ReceivingForwarderPK : base.JD_OH_ReceivingAgent; }
			set { base.JD_OH_ReceivingAgent = value; }
		}

		OrgHeader OrderReceivingAgent => Factory.Load<OrgHeader>(base.JD_OH_ReceivingAgent);

		#endregion

		#endregion

		#region PickupAddressLine

		[BusinessObjectTestExclude]
		public ZString PickupAddressLine
		{
			get
			{
				return GoodsAvailableAtAddress.AddressAsASingleLine;
			}
			set
			{
				var query = new ZQuery(OrgAddressSchema.OA_Code, value);
				var address = Factory.LoadTop1<OrgAddress>(query);
				GoodsAvailableAtAddress.E2_OA_Address = address?.PK ?? ZGuid.Empty;

				PickupAddressLineInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo PickupAddressLineInfo
		{
			get { return GetZPropertyInfo(nameof(PickupAddressLine)); }
		}

		#endregion

		#region DeliverAddressLine

		[BusinessObjectTestExclude]
		public ZString DeliverAddressLine
		{
			get
			{
				return GoodsDeliveredToAddress.AddressAsASingleLine;
			}
			set
			{
				var query = new ZQuery(OrgAddressSchema.OA_Code, value);
				var address = Factory.LoadTop1<OrgAddress>(query);
				GoodsDeliveredToAddress.E2_OA_Address = address?.PK ?? ZGuid.Empty;

				DeliverAddressLineInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DeliverAddressLineInfo
		{
			get { return GetZPropertyInfo(nameof(DeliverAddressLine)); }
		}

		#endregion

		#region JD_RL_NKGoodsAvailableAt

		public override ZString JD_RL_NKGoodsAvailableAt
		{
			get { return ShipOrDec != null ? ShipOrDec.OriginPortCode : base.JD_RL_NKGoodsAvailableAt; }
			set { base.JD_RL_NKGoodsAvailableAt = value; }
		}

		#endregion

		#region JD_RL_NKGoodsDeliveredTo

		public override ZString JD_RL_NKGoodsDeliveredTo
		{
			get { return ShipOrDec != null ? ShipOrDec.DestinationPortCode : base.JD_RL_NKGoodsDeliveredTo; }
			set { base.JD_RL_NKGoodsDeliveredTo = value; }
		}

		#endregion

		#region JD_Waybill

		public override ZString JD_Waybill
		{
			get { return ShipOrDec != null ? ShipOrDec.HouseBill : base.JD_Waybill; }
			set { base.JD_Waybill = value; }
		}

		#endregion

		#region JD_MasterWaybill

		public override ZString JD_MasterWaybill
		{
			get { return ShipOrDec != null ? ShipOrDec.MasterBill : base.JD_MasterWaybill; }
		}

		#endregion

		#region PacksWeightVolume

		#region JD_PacksWithUnits

		public ZString JD_PacksWithUnits
		{
			get
			{
				return string.Format("{0} {1}", JD_Packs, JD_F3_NKPackType);
			}
		}

		public ZPropertyInfo JD_PacksWithUnitsInfo
		{
			get { return GetZPropertyInfo(Schema.JD_PacksWithUnits); }
		}

		#endregion

		#region JD_ActualWeightWithUnits

		public ZString JD_ActualWeightWithUnits
		{
			get
			{
				var result = ZDecimal.Zero;
				var unitOfWeight = ZString.Empty;

				if (Shipment != null)
				{
					result = Shipment.JS_ActualWeight;
					unitOfWeight = Shipment.JS_UnitOfWeight;
				}
				else if (Declaration != null)
				{
					result = new ZDecimal(Declaration[JobDeclarationSchema.Constants.JE_TotalWeight]);
					unitOfWeight = new ZString(Declaration[JobDeclarationSchema.Constants.JE_TotalWeightUnit]);
				}
				else
				{
					result = JD_ActualWeight;
					unitOfWeight = JD_UnitOfWeight;
				}

				var roundedDecimal = this.GetRoundedValue(JobOrderHeaderSchema.JD_ActualWeight, JD_ActualWeightInfo, result);
				var formattedDecimal = FormatNumber(roundedDecimal, DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(this, JD_ActualWeightInfo.PropertyDescriptor));
				return string.Format("{0} {1}", formattedDecimal, unitOfWeight);
			}
		}

		protected string FormatNumber(ZDecimal number, int decimalsToShow)
		{
			return Utilities.FormatNumber(number, decimalsToShow, WebEnvShared.ClientCulture);
		}

		public ZPropertyInfo JD_ActualWeightWithUnitsInfo
		{
			get { return GetZPropertyInfo(Schema.JD_ActualWeightWithUnits); }
		}

		#endregion

		#region JD_ActualVolumeWithUnits

		public ZString JD_ActualVolumeWithUnits
		{
			get
			{
				var result = ZDecimal.Zero;
				var unitOfVolume = ZString.Empty;

				if (Shipment != null)
				{
					result = Shipment.JS_ActualVolume;
					unitOfVolume = Shipment.JS_UnitOfVolume;
				}
				else if (Declaration != null)
				{
					result = new ZDecimal(Declaration[JobDeclarationSchema.Constants.JE_TotalVolume]);
					unitOfVolume = new ZString(Declaration[JobDeclarationSchema.Constants.JE_TotalVolumeUnit]);
				}
				else
				{
					result = JD_ActualVolume;
					unitOfVolume = JD_UnitOfVolume;
				}

				var roundedDecimal = this.GetRoundedValue(JobOrderHeaderSchema.JD_ActualVolume, JD_ActualVolumeInfo, result);
				var formattedDecimal = FormatNumber(roundedDecimal, DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(this, JD_ActualVolumeInfo.PropertyDescriptor));
				return string.Format("{0} {1}", formattedDecimal, unitOfVolume);
			}
		}

		public ZPropertyInfo JD_ActualVolumeWithUnitsInfo
		{
			get { return GetZPropertyInfo(Schema.JD_ActualVolumeWithUnits); }
		}

		#endregion

		#region ShipDecPacksWithUnits

		public ZString ShipDecPacksWithUnits
		{
			get
			{
				return ShipOrDec != null
				? ShipOrDec.PacksWithUnits
				: (ZString)string.Format("{0} {1}", JD_Packs, JD_F3_NKPackType);
			}
		}

		public ZPropertyInfo ShipDecPacksWithUnitsInfo
		{
			get { return GetZPropertyInfo(Schema.ShipDecPacksWithUnits); }
		}

		#endregion

		#region ShipDecActualWeightWithUnits

		public ZString ShipDecActualWeightWithUnits
		{
			get
			{
				return ShipOrDec != null ? ShipOrDec.WeightWithUnits : JD_ActualWeightWithUnits;
			}
		}

		public ZPropertyInfo ShipDecActualWeightWithUnitsInfo
		{
			get { return GetZPropertyInfo(Schema.ShipDecActualWeightWithUnits); }
		}

		#endregion

		#region ShipDecActualVolumeWithUnits

		public ZString ShipDecActualVolumeWithUnits
		{
			get
			{
				return ShipOrDec != null ? ShipOrDec.VolumeWithUnits : JD_ActualVolumeWithUnits;
			}
		}

		public ZPropertyInfo ShipDecActualVolumeWithUnitsInfo
		{
			get { return GetZPropertyInfo(Schema.ShipDecActualVolumeWithUnits); }
		}

		#endregion

		#endregion

		#region JD_StatusDesc

		public virtual ZString JD_OrderStatusDesc
		{
			get { return JD_OrderStatus_List.GetDescriptionFromCode(JD_OrderStatus); }
		}

		public ZPropertyInfo JD_OrderStatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.JD_OrderStatusDesc); }
		}
		#endregion

		#endregion

		#region Documents

		public DocumentSupport DocumentHelper
		{
			get
			{
				if (documentHelper == null)
				{
					documentHelper = new DocumentSupport(this);
				}

				return documentHelper;
			}
		}
		DocumentSupport documentHelper;

		#region LoggedInContact

		public OrgContact LoggedInContact
		{
			get { return loggedInContact ?? WebEnv.CurrentUser as OrgContact; }
			set { loggedInContact = value; }
		}
		protected OrgContact loggedInContact;

		#endregion

		public ZGuid DocParentPK
		{
			get { return PK; }
		}

		public List<ZGuid> DocRelatedPKs
		{
			get { return new List<ZGuid>(); }
		}

		#endregion

		#region IBizOChangesEmailNotification Members

		public ZString Number
		{
			get { return JD_OrderNumber; }
		}

		public new ZBool IsCancelled => JD_OrderStatus == Constants.OrderStatus.Cancelled || JD_IsCancelled;

		GlbBranch IBizOChangesEmailNotification.EventBranch
		{
			get
			{
				var result = (GlbBranch.FindControllingBranchWithFallBackToAnyCompany(Buyer)
					?? GlbBranch.FindControllingBranchWithFallBackToAnyCompany(ControllingCustomerDocAddress.Organisation))
					?? GlbBranch.FindByHomePortWithFallBackToRelatedPort(Factory, PortOfDischarge);

				if (result == null && Buyer != null)
				{
					result = GlbBranch.FindByHomePortWithFallBackToRelatedPort(Factory, Buyer.ClosestPort);
				}

				if (result == null && ControllingCustomerDocAddress.Organisation != null)
				{
					result = GlbBranch.FindByHomePortWithFallBackToRelatedPort(Factory, ControllingCustomerDocAddress.Organisation.ClosestPort);
				}

				return result;
			}
		}

		GuidRegistryItem IBizOChangesEmailNotification.EmailGroupRegistryItem
		{
			get { return WebDataRegistry.Instance.OrderNotificationEmailGroup; }
		}

		OrgHeader IBizOChangesEmailNotification.RelatedOrg
		{
			get { return Buyer; }
		}

		CodeDescriptionBoolRegistryItem IBizOChangesEmailNotification.StaffRolesToNotify { get { return WebDataRegistry.Instance.OrderNotificationStaffRoles; } }

		CodePairRegistryItem IBizOChangesEmailNotification.NotificationSendingRule
		{
			get { return WebDataRegistry.Instance.OrderNotificationOptions; }
		}

		ZGuid IBizOChangesEmailNotification.GetStaffGuid(OrgStaffAssignmentsCollection staffAssignments, CodeDescriptionBool role)
		{
			ZString staffNK = staffAssignments.GetStaffAssignment(role.Code, OrgStaffAssignmentsCollection.Direction.Import, IsAirTransport ? OrgStaffAssignmentsCollection.AirSea.Air : OrgStaffAssignmentsCollection.AirSea.Sea);
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

		ControllerID IBizOChangesEmailNotification.ControllerForEnterpriseUrl
		{
			get { return ControllerIDs.Orders; }
		}

		#region Email Reporting

		void IBizOChangesEmailNotification.AddPropertiesForEmailReporting(DataState state)
		{
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("377c6cc3-6cd9-4b74-8858-0ea0806faae7", "Order Status"), JD_OrderStatus);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("98a1598d-07a1-4ed8-a84b-b0179a2ee694", "Supplier"), (Supplier != null) ? Supplier.OH_FullNameTruncated : ZString.Empty);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("881b92d8-8083-42bf-9c80-8d27b399f032", "Buyer"), (Buyer != null) ? Buyer.OH_FullNameTruncated : ZString.Empty);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("6d368873-4be9-4103-acfe-e2548296d199", "Order Number"), JD_OrderNumberAndSplit);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("28a90cad-a72d-404d-9a36-ab78248d3d8d", "Currency"), OrderCurrency != null ? ResString.GetMultilingualString("2cf8daa1-3c38-4f34-bf0a-cc5e7355fd7f", "{0} ({1})", OrderCurrency.RX_Code, OrderCurrency.RX_DescMultilingual) : (NoResString)ZString.Empty);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("b16cdb9f-0797-467d-8678-05502acde433", "Service Level"), JD_RS_NKServiceLevel_NI);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("a0834014-3a4c-44aa-4acd-e755994e968e", "Incoterms"), JD_IncoTerm);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("1b02f7d1-1432-4876-87f1-dbd0c86a33aa", "Transport Mode"), JD_TransportMode);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("f2d28693-a01d-4cdf-be2d-6f9574ae3b72", "Container Mode"), JD_ContainerMode);

			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("fd57e2f5-c79a-4436-bdde-d86a9d84ad5f", "Order Date"), JD_OrderDate);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("87e786e1-b981-4292-9d8d-eaa87a1f8084", "Req. Ex Works"), JD_ExWorksRequiredBy);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("bac023a8-1f18-44d4-b4bb-83a0fefef5f2", "Req. In Store"), JD_DeliveryRequiredBy);

			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("ad5b9d25-a0e3-4005-b1ec-94721b2e8cff", "Port of Loading"), JD_RL_NKPortOfLoading);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("33740e20-8aed-454c-accf-bb94d3effc19", "Port of Discharge"), JD_RL_NKPortOfDischarge);

			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("36d44897-57bd-4a67-a6b4-dbb6166060eb", "Packs"), JD_Packs, JD_F3_NKPackType, JD_F3_NKPackType_List);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("8cf29647-9ba6-4b77-b162-0be0208c4b04", "Volume"), JD_ActualVolume, JD_UnitOfVolume, JD_UnitOfVolume_List);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("d956b604-5597-4c00-b94e-f1bbd747cdb4", "Weight"), JD_ActualWeight, JD_UnitOfWeight, JD_UnitOfWeight_List);

			AddOrderLinesForEmailReporting(state, OrderLines);
			DocumentHelper.AddDocumentsForEmailReporting(state, PropertiesForEmailReporting);
			EditableMilestones.AddForEmailReporting(state, PropertiesForEmailReporting);
		}

		void AddOrderLinesForEmailReporting(DataState state, OrderLineCollection collection)
		{
			foreach (OrderLine line in collection)
			{
				var value = GenerateOrderLineDetailsForEmailReporting(line);

				var propertyName = ResString.GetMultilingualString("1f0d029f-dc5c-4307-b4e0-6d41f7c5f97f", "Order Line {0}", line.JO_LineNo);

				PropertiesForEmailReporting.Add(state, propertyName, value);
			}
		}

		public MultilingualString GenerateOrderLineDetailsForEmailReporting(OrderLine line)
		{
			return MultilingualString.Join(System.Environment.NewLine,
				ResString.GetMultilingualString("87de7d5e-fff6-4e06-9604-462024061ab8", "Part #: {0}", line.JO_Partno),
				ResString.GetMultilingualString("fef20bfc-a848-479d-8476-e106170c8f06", "Description: {0}", line.JO_Description),
				ResString.GetMultilingualString("189ff00a-155f-44e1-849c-e47ad6d84cdf", "Inner Packs: {0}", line.JO_InnerPacks),
				ResString.GetMultilingualString("a556b3ba-e3d9-4df4-823b-cf5bb2fa017e", "Outer Packs: {0}", line.JO_OuterPacks),
				ResString.GetMultilingualString("4364cbef-0664-49b1-b251-32ff34057b19", "Qty Ordered: {0}", line.JO_Quantity),
				ResString.GetMultilingualString("e07b0d49-c34f-4179-9e6a-7f747c162af9", "Qty Invoiced: {0}", line.JO_QtyInvoiced),
				ResString.GetMultilingualString("662fd9bf-8e6d-44a2-afb9-23fb027a8728", "Qty Received: {0}", line.JO_QtyReceived),
				ResString.GetMultilingualString("7990f843-74f8-4b37-9935-f92302dff74f", "Qty Remaining: {0}", line.JO_QuantityRemaining),
				ResString.GetMultilingualString("01439809-96ce-40a5-a02e-34abbe6239d9", "Unit of Qty: {0}", line.JO_F3_NKPackType),
				ResString.GetMultilingualString("d5345fb6-536c-44c1-b1cb-3896b6069521", "Item Price: {0}", line.JO_ItemPrice),
				ResString.GetMultilingualString("b4bdfeb8-9489-4c6b-9925-e5a4e3386921", "Total Price: {0}", line.JO_LinePrice));
		}

		public PropertyChangeInfo[] GetPropertiesForEmailReporting()
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

		#region VisibleNotes

		public IStmNoteParent NotesParentBO
		{
			get { return this; }
		}

		public WebUserVisibleNotes NotesHelper
		{
			get
			{
				if (notesHelper == null)
				{
					notesHelper = new WebUserVisibleNotes(this);
				}

				return notesHelper;
			}
		}

		WebUserVisibleNotes notesHelper;

		#endregion

		#region UserEditableNote

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

		protected override bool EnableLightValidationIfAvailable
		{
			get { return false; }
		}

		#region Suppress Flight Details

		public ZDateTime ArrivalETDWithSuppression
		{
			get { return Suppression.GetWebValue(JD_E_DEP_3, this, SuppressFields.ETD); }
		}

		public ZPropertyInfo ArrivalETDWithSuppressionInfo
		{
			get { return GetZPropertyInfo(Schema.ArrivalETDWithSuppression); }
		}

		public ZDateTime ETDWithSuppression
		{
			get { return Suppression.GetWebValue(JD_Milestone_E_DEP, this, SuppressFields.ETD); }
		}

		public ZPropertyInfo ETDWithSuppressionInfo
		{
			get { return GetZPropertyInfo(Schema.ETDWithSuppression); }
		}

		public ZDateTime ETAWithSuppression
		{
			get { return Suppression.GetWebValue(JD_Milestone_E_ARV, this, SuppressFields.ETA); }
		}

		public ZPropertyInfo ETAWithSuppressionInfo
		{
			get { return GetZPropertyInfo(Schema.ETAWithSuppression); }
		}

		public ZDateTime ATDWithSuppression
		{
			get { return Suppression.GetWebValue(JD_Milestone_A_DEP, this, SuppressFields.ATD); }
		}

		public ZPropertyInfo ATDWithSuppressionInfo
		{
			get { return GetZPropertyInfo(Schema.ATDWithSuppression); }
		}

		public ZDateTime ATAWithSuppression
		{
			get { return Suppression.GetWebValue(JD_Milestone_A_ARV, this, SuppressFields.ATA); }
		}

		public ZPropertyInfo ATAWithSuppressionInfo
		{
			get { return GetZPropertyInfo(Schema.ATAWithSuppression); }
		}

		public ZDateTime IntermediateETDWithSuppression
		{
			get { return Suppression.GetWebValue(JD_E_DEP_2, this, SuppressFields.ETD); }
		}

		public ZPropertyInfo IntermediateETDWithSuppressionInfo
		{
			get { return GetZPropertyInfo(Schema.IntermediateETDWithSuppression); }
		}

		public ZDateTime IntermediateETAWithSuppression
		{
			get { return Suppression.GetWebValue(JD_E_ARV_2ndIntermediate, this, SuppressFields.ETA); }
		}

		public ZPropertyInfo IntermediateETAWithSuppressionInfo
		{
			get { return GetZPropertyInfo(Schema.IntermediateETAWithSuppression); }
		}

		public ZString ArrivalVoyageWithSuppression
		{
			get { return Suppression.GetWebValue(JD_ArrivalVoyage, this, SuppressFields.FlightNumber); }
		}

		public ZPropertyInfo ArrivalVoyageWithSuppressionInfo
		{
			get { return GetZPropertyInfo(Schema.ArrivalVoyageWithSuppression); }
		}

		public ZString IntermediateVoyageWithSuppression
		{
			get { return Suppression.GetWebValue(JD_IntermediateVoyage, this, SuppressFields.FlightNumber); }
		}

		public ZPropertyInfo IntermediateVoyageWithSuppressionInfo
		{
			get { return GetZPropertyInfo(Schema.IntermediateVoyageWithSuppression); }
		}

		public ZString DepartureVoyageWithSuppression
		{
			get { return Suppression.GetWebValue(JD_DepartureVoyage, this, SuppressFields.FlightNumber); }
		}

		public ZPropertyInfo DepartureVoyageWithSuppressionInfo
		{
			get { return GetZPropertyInfo(Schema.DepartureVoyageWithSuppression); }
		}

		public ZDateTime DepartureETAWithSuppression
		{
			get { return Suppression.GetWebValue(JD_E_ARV_1stIntermediate, this, SuppressFields.ETA); }
		}

		public ZPropertyInfo DepartureETAWithSuppressionInfo
		{
			get { return GetZPropertyInfo(Schema.DepartureETAWithSuppression); }
		}

		public ZString MainVoyageWithSuppression
		{
			get { return ShipOrDec != null ? ShipOrDec.MainVoyageWithSuppression : ArrivalVoyageWithSuppression; }
		}

		public ZPropertyInfo MainVoyageWithSuppressionInfo
		{
			get { return GetZPropertyInfo(Schema.MainVoyageWithSuppression); }
		}

		public ZString MainVessel
		{
			get { return ShipOrDec != null ? ShipOrDec.MainVessel : JD_RV_NKArrivalVessel; }
		}

		public ZPropertyInfo MainVesselInfo
		{
			get { return GetZPropertyInfo(Schema.MainVessel); }
		}

		public ZString CurrentVoyageWithSuppression
		{
			get { return Shipment != null ? Shipment.CurrentVoyageWithSuppression : ZString.Empty; }
		}

		public ZPropertyInfo CurrentVoyageWithSuppressionInfo
		{
			get { return GetZPropertyInfo(Schema.CurrentVoyageWithSuppression); }
		}

		public ZString CurrentVessel
		{
			get { return Shipment != null ? Shipment.CurrentVessel : ZString.Empty; }
		}

		public ZPropertyInfo CurrentVesselInfo
		{
			get { return GetZPropertyInfo(Schema.CurrentVessel); }
		}

		#region Property overrides

		[RequiresSuppression]
		public override ZDateTime JD_E_DEP_3
		{
			get { return base.JD_E_DEP_3; }
			set { base.JD_E_DEP_3 = value; }
		}

		[RequiresSuppression]
		public override ZDateTime JD_Milestone_E_DEP
		{
			get { return base.JD_Milestone_E_DEP; }
			set { base.JD_Milestone_E_DEP = value; }
		}

		[RequiresSuppression]
		public override ZDateTime JD_Milestone_E_ARV
		{
			get { return base.JD_Milestone_E_ARV; }
			set { base.JD_Milestone_E_ARV = value; }
		}

		[RequiresSuppression]
		public override ZDateTime JD_Milestone_A_DEP
		{
			get { return base.JD_Milestone_A_DEP; }
			set { base.JD_Milestone_A_DEP = value; }
		}

		[RequiresSuppression]
		public override ZDateTime JD_Milestone_A_ARV
		{
			get { return base.JD_Milestone_A_ARV; }
			set { base.JD_Milestone_A_ARV = value; }
		}

		[RequiresSuppression]
		public override ZDateTime JD_E_DEP_2
		{
			get { return base.JD_E_DEP_2; }
			set { base.JD_E_DEP_2 = value; }
		}

		[RequiresSuppression]
		public override ZDateTime JD_E_ARV_2ndIntermediate
		{
			get { return base.JD_E_ARV_2ndIntermediate; }
			set { base.JD_E_ARV_2ndIntermediate = value; }
		}

		[RequiresSuppression]
		public override ZString JD_ArrivalVoyage
		{
			get { return base.JD_ArrivalVoyage; }
			set { base.JD_ArrivalVoyage = value; }
		}

		[RequiresSuppression]
		public override ZString JD_IntermediateVoyage
		{
			get { return base.JD_IntermediateVoyage; }
			set { base.JD_IntermediateVoyage = value; }
		}

		#endregion

		#endregion

		#region IWebUserVisibleNotesSupport Members

		public bool ShowAgentNotes
		{
			get
			{
				return LoggedInOrgIsTheAgentForThisJob ||
				(Shipment != null && Shipment.LoggedInOrgIsTheAgentForThisJob);
			}
		}

		public bool LoggedInOrgIsTheAgentForThisJob
		{
			get
			{
				OrgHeader loggedInOrg = LoggedInOrganisation == null && Shipment != null ? Shipment.LoggedInOrganisation : LoggedInOrganisation;
				return loggedInOrg != null &&
				(SendingAgent != null && SendingAgent.PK == loggedInOrg.PK ||
				ReceivingAgent != null && ReceivingAgent.PK == loggedInOrg.PK);
			}
		}

		#endregion

		#region IWebDocumentsWithUploadSupport

		public DocManagerInfo DocManagerInfo
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
			documentHelper = null;
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

		TrackingSiteUser SiteUser
		{
			get { return WebEnv.AppInstance != null ? WebEnv.AppInstance.SiteUser as TrackingSiteUser : null; }
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

		protected override OrderProcessTasksCollection CreateProcessTaskCollection()
		{
			return new TrackingOrderProcessTasksCollection(this);
		}

		#endregion

		#region IUpdatableMilestoneEventsProvider

		public List<string> UpdatableMilestoneEventCodes
		{
			get
			{
				if (WebEnv.AppInstance != null && WebEnv.AppInstance.SiteUser != null)
				{
					updatableMilestoneEventCodes = new UpdateableMilestoneEventsHelper(WebEnv.AppInstance.SiteUser as OrgContactWebUser).GetUpdateableMilestoneEvents(WebDataRegistry.Instance.OrderMilestoneEventUpdates.Value, WebParties);
				}
				return updatableMilestoneEventCodes;
			}
		}

		List<string> updatableMilestoneEventCodes = new List<string>();

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
					webParties.Add(WebPartyType.OrderedBy, Buyer);
					webParties.Add(WebPartyType.Supplier, Supplier);
					webParties.Add(WebPartyType.SendingAgent, SendingAgent);
					webParties.Add(WebPartyType.SendingAgent, OrderSendingAgent);
					webParties.Add(WebPartyType.ReceivingAgent, ReceivingAgent);
					webParties.Add(WebPartyType.ReceivingAgent, OrderReceivingAgent);
				}

				return webParties;
			}
		}
		WebPartyTypeOrgPairCollection webParties;

		#endregion

		public OrgHeader LoggedInOrganisation
		{
			get { return (LoggedInContact == null) ? null : LoggedInContact.ParentOrg; }
		}

		#region DeliverAddress_List

		public OrgAddressDependentCollection DeliverAddress_List
		{
			get
			{
				if (GoodsDeliveredToAddress.Organisation != null)
				{
					var collection = new OrgAddressDependentCollection(GoodsDeliveredToAddress.Organisation, new ZQuery(OrgAddressSchema.OA_IsActive, ZBool.True));
					collection.Load();
					return collection;
				}
				else
				{
					return new OrgAddressDependentCollection((OrgHeader)Factory.LoadTop1(typeof(OrgHeader), new ZQuery()), new ZQuery(OrgAddressSchema.PK, ZGuid.Empty));
				}
			}
		}

		#endregion

		#region PickupAddress_List

		public OrgAddressDependentCollection PickupAddress_List
		{
			get
			{
				if (GoodsAvailableAtAddress.Organisation != null)
				{
					var collection = new OrgAddressDependentCollection(GoodsAvailableAtAddress.Organisation, new ZQuery(OrgAddressSchema.OA_IsActive, ZBool.True));
					collection.Load();
					return collection;
				}
				else
				{
					return new OrgAddressDependentCollection((OrgHeader)Factory.LoadTop1(typeof(OrgHeader), new ZQuery()), new ZQuery(OrgAddressSchema.PK, ZGuid.Empty));
				}
			}
		}

		#endregion

		#region TestCase
#if DEBUG

		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper()
		{
			return new TrackingOrderTestDataHelper();
		}

		class TrackingOrderTestDataHelper : BusinessObjectTestDataHelper
		{
			protected override void FillDependentCollectionWithData(IBusinessObjectCollection collection, TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor collectionProperty, System.ComponentModel.PropertyDescriptor[] propertyPath)
			{
				if (collectionProperty.Name != "DeliverAddress_List" &&
					collectionProperty.Name != "PickupAddress_List" &&
					collectionProperty.Name != "RequiredDocuments")
				{
					base.FillDependentCollectionWithData(collection, kind, collectionProperty, propertyPath);
				}
			}
		}

#endif

		#endregion
	}
}
