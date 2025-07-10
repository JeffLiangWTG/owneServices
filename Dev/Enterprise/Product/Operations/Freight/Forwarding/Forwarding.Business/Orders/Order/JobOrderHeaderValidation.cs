using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class JobOrderHeaderValidation : AutoJobOrderHeaderValidation
	{
		public JobOrderHeaderValidation(AutoJobOrderHeader parent) : base(parent)
		{
		}

		public new Order Parent
		{
			get { return (Order)base.Parent; }
		}

		#region Buyer and OrderNumber and Split is Unique

		protected BuyerAndOrderNumAndSplitIsUniqueValidation BuyerAndOrderNumAndSplitIsUniqueValidation
		{
			get
			{
				if (fBuyerAndOrderNumAndSplitIsUniqueValidation == null)
				{
					fBuyerAndOrderNumAndSplitIsUniqueValidation = new BuyerAndOrderNumAndSplitIsUniqueValidation(Parent);
				}
				return fBuyerAndOrderNumAndSplitIsUniqueValidation;
			}
		}

		BuyerAndOrderNumAndSplitIsUniqueValidation fBuyerAndOrderNumAndSplitIsUniqueValidation;

		#endregion

		#region Vessels Validation

		protected void CheckVesselNotEnteredBecauseTransportSea(ZPropertyInfo prop)
		{
			if (!prop.Value.IsEmpty)
			{
				prop.AddError(
					Res.GetString("39077dcc-5044-444f-9d28-321605ed4b24", "A vessel can only be transported by Sea. Either remove the vessel, or change the Transport Type."));
			}
		}

		protected override void CheckJD_RV_NKDepartureVessel()
		{
			base.CheckJD_RV_NKDepartureVessel();

			if (!Parent.IsSeaTransport)
			{
				CheckVesselNotEnteredBecauseTransportSea(Parent.JD_RV_NKDepartureVesselInfo);
			}
			else
			{
				if ((!Parent.JD_RV_NKArrivalVessel.IsEmpty || !Parent.JD_RV_NKIntermediateVessel.IsEmpty) &&
					Parent.JD_RV_NKDepartureVessel.IsEmpty)
				{
					Parent.JD_RV_NKDepartureVesselInfo.AddError(
						Res.GetString("630d94cb-dc6c-42be-bd4e-941427782ac3", "You have specified an Intermediate or Arrival vessel. You must specify the Departure vessel."));
				}
				ListValidation.WarnIfInvalidCode(Parent.JD_RV_NKDepartureVesselInfo, Parent.JD_RV_Vessel_List);
			}
		}

		protected override void CheckJD_RV_NKIntermediateVessel()
		{
			base.CheckJD_RV_NKIntermediateVessel();

			if (!Parent.IsSeaTransport)
			{
				CheckVesselNotEnteredBecauseTransportSea(Parent.JD_RV_NKIntermediateVesselInfo);
			}
			else
			{
				if (!Parent.JD_RV_NKIntermediateVessel.IsEmpty &&
					(Parent.JD_RV_NKIntermediateVessel == Parent.JD_RV_NKDepartureVessel || Parent.JD_RV_NKIntermediateVessel == Parent.JD_RV_NKArrivalVessel))
				{
					Parent.JD_RV_NKIntermediateVesselInfo.AddError(
						Res.GetString("f7d9fd45-0c5a-42ea-bc62-db790bab5d47", "The intermediate vessel cannot be the same as the arrival or departure vessel."));
				}
				ListValidation.WarnIfInvalidCode(Parent.JD_RV_NKIntermediateVesselInfo, Parent.JD_RV_Vessel_List);
			}
		}

		protected override void CheckJD_RV_NKArrivalVessel()
		{
			base.CheckJD_RV_NKArrivalVessel();

			if (!Parent.IsSeaTransport)
			{
				CheckVesselNotEnteredBecauseTransportSea(Parent.JD_RV_NKArrivalVesselInfo);
			}
			else
			{
				if (!Parent.JD_RV_NKIntermediateVessel.IsEmpty && Parent.JD_RV_NKArrivalVessel.IsEmpty)
				{
					Parent.JD_RV_NKArrivalVesselInfo.AddError(
						Res.GetString("9c185c4e-726a-4a2d-b205-d9ebd6cd2e60", "You have specified an Intermediate vessel. You must specify the Arrival vessel as well."));
				}
				if (!Parent.JD_RV_NKIntermediateVessel.IsEmpty &&
					!Parent.JD_RV_NKDepartureVessel.IsEmpty &&
					!Parent.JD_RV_NKArrivalVessel.IsEmpty &&
					Parent.JD_RV_NKDepartureVessel == Parent.JD_RV_NKArrivalVessel)
				{
					Parent.JD_RV_NKArrivalVesselInfo.AddError(
						Res.GetString("708d84f6-a695-404c-b3f6-6779ec535c02", "You have specified an Intermediate vessel. The Departure vessel cannot be the same as the Arrival vessel."));
				}
				ListValidation.WarnIfInvalidCode(Parent.JD_RV_NKArrivalVesselInfo, Parent.JD_RV_Vessel_List);
			}
		}

		#endregion

		#region Voyage Validation

		protected override void CheckJD_DepartureVoyage()
		{
			base.CheckJD_DepartureVoyage();
			if (!Parent.JD_RV_NKDepartureVessel.IsEmpty)
			{
				MandatoryValidation.WarnIfNotEntered(Parent.JD_DepartureVoyageInfo);
			}
		}

		protected override void CheckJD_IntermediateVoyage()
		{
			base.CheckJD_IntermediateVoyage();
			if (!Parent.JD_RV_NKIntermediateVessel.IsEmpty)
			{
				MandatoryValidation.WarnIfNotEntered(Parent.JD_IntermediateVoyageInfo);
			}
		}

		protected override void CheckJD_ArrivalVoyage()
		{
			base.CheckJD_ArrivalVoyage();
			if (!Parent.JD_RV_NKArrivalVessel.IsEmpty)
			{
				MandatoryValidation.WarnIfNotEntered(Parent.JD_ArrivalVoyageInfo);
			}
		}

		#endregion

		#region Date Validation

		protected override void CheckJD_OrderDate()
		{
			base.CheckJD_OrderDate();
			MandatoryValidation.CheckEntered(Parent.JD_OrderDateInfo);
		}

		public void ValidateJD_Milestone_E_DEP()
		{
			ValidateCalculatedProperty(Parent.JD_Milestone_E_DEPInfo);
		}

		protected virtual void CheckJD_Milestone_E_DEP()
		{
			TypeValidation.CheckValidZDateTimeWithoutRange(Parent.JD_Milestone_E_DEPInfo);
			TypeValidation.CheckValidZDateTimeRange(Parent.JD_Milestone_E_DEPInfo);
			if (!Parent.OrderLines.AreAllOrderLinesEmpty)
			{
				MandatoryValidation.WarnIfNotEntered(Parent.JD_Milestone_E_DEPInfo);
			}
		}

		protected override void CheckJD_E_ARV_1stIntermediate()
		{
			base.CheckJD_E_ARV_1stIntermediate();
			if (Parent.PlanningVoyageState == PlanningVoyageState.TwoVoyage ||
				Parent.PlanningVoyageState == PlanningVoyageState.ThreeVoyage)
			{
				if (!Parent.OrderLines.AreAllOrderLinesEmpty)
				{
					MandatoryValidation.WarnIfNotEntered(Parent.JD_E_ARV_1stIntermediateInfo);
				}
				CheckTransportArrivalDateValid(Parent.JD_E_ARV_1stIntermediateInfo, Parent.JD_Milestone_E_DEPInfo);
			}
		}

		protected override void CheckJD_E_DEP_2()
		{
			base.CheckJD_E_DEP_2();
			if (Parent.PlanningVoyageState == PlanningVoyageState.ThreeVoyage)
			{
				if (!Parent.OrderLines.AreAllOrderLinesEmpty)
				{
					MandatoryValidation.WarnIfNotEntered(Parent.JD_E_DEP_2Info);
				}
			}
		}

		protected override void CheckJD_E_ARV_2ndIntermediate()
		{
			base.CheckJD_E_ARV_2ndIntermediate();
			if (Parent.PlanningVoyageState == PlanningVoyageState.ThreeVoyage)
			{
				if (!Parent.OrderLines.AreAllOrderLinesEmpty)
				{
					MandatoryValidation.WarnIfNotEntered(Parent.JD_E_ARV_2ndIntermediateInfo);
				}
				CheckTransportArrivalDateValid(Parent.JD_E_ARV_2ndIntermediateInfo, Parent.JD_E_DEP_2Info);
			}
		}

		protected override void CheckJD_E_DEP_3()
		{
			base.CheckJD_E_DEP_3();
			if (Parent.PlanningVoyageState == PlanningVoyageState.TwoVoyage ||
				Parent.PlanningVoyageState == PlanningVoyageState.ThreeVoyage)
			{
				if (!Parent.OrderLines.AreAllOrderLinesEmpty)
				{
					MandatoryValidation.WarnIfNotEntered(Parent.JD_E_DEP_3Info);
				}
			}
		}

		public void ValidateJD_Milestone_E_ARV()
		{
			ValidateCalculatedProperty(Parent.JD_Milestone_E_ARVInfo);
		}

		protected virtual void CheckJD_Milestone_E_ARV()
		{
			TypeValidation.CheckValidZDateTimeWithoutRange(Parent.JD_Milestone_E_ARVInfo);
			TypeValidation.CheckValidZDateTimeRange(Parent.JD_Milestone_E_ARVInfo);
			if (!Parent.OrderLines.AreAllOrderLinesEmpty)
			{
				MandatoryValidation.WarnIfNotEntered(Parent.JD_Milestone_E_ARVInfo);
			}
			if (!Parent.IsShipmentAttached)
			{
				CheckTransportArrivalDateValid(Parent.JD_Milestone_E_ARVInfo, Parent.JD_E_DEP_3Info);
			}
			CheckTransportArrivalDateValid(Parent.JD_Milestone_E_ARVInfo, Parent.JD_Milestone_E_DEPInfo);
		}

		protected void CheckTransportArrivalDateValid(ZPropertyInfo arrival, ZPropertyInfo departure)
		{
			ZDateTime departureDate = AsLocalTime(departure.Value);
			ZDateTime arrivalDate = AsLocalTime(arrival.Value);
			if (departureDate.IsValid)
			{
				if (arrivalDate.IsValid)
				{
					if (Parent.IsAirTransport)
					{
						CompareValidation.CheckDateIsNotBeforeAnotherDate(arrival, departureDate.AddDays(-1));
					}
					else
					{
						CompareValidation.CheckDateIsNotBeforeAnotherDate(arrival, departureDate);
					}
				}
				if (arrivalDate.CompareTo(departureDate.AddMonths(6)) >= 0)
				{
					arrival.AddError(Res.GetString("b7f810de-2df0-4ccd-9002-d5316b58218f", "This voyage length is far too long for any commercial transport."));
				}
				CompareValidation.CheckDateIsBeforeAnotherDate(arrival, departureDate.AddMonths(6));
			}
		}

		ZDateTime AsLocalTime(IZType value)
		{
			if (value is ZDateTimeOffset dateTime)
			{
				return dateTime.ToZDateTime();
			}
			else
			{
				return (ZDateTime)value;
			}
		}

		protected override void CheckJD_RN_NKCountryOfSupply()
		{
			base.CheckJD_RN_NKCountryOfSupply();
			ListValidation.ErrorIfInvalidCode(Parent.JD_RN_NKCountryOfSupplyInfo, Parent.Lookups.CountryOfSupplies);
		}

		protected override void CheckJD_BookingConfDate()
		{
			base.CheckJD_BookingConfDate();

			if (Parent.JD_BookingConfDate != ZDateTime.Empty)
			{
				bool lineDateAfterOrderDate = false;

				foreach (OrderLine line in Parent.OrderLines)
				{
					if (line.JO_ConfirmationDate > Parent.JD_BookingConfDate)
					{
						lineDateAfterOrderDate = true;
						break;
					}
				}

				if (lineDateAfterOrderDate)
				{
					Parent.JD_BookingConfDateInfo.AddWarning(Res.GetString("9ee50f8a-e49f-4575-a9a8-1ce89778ac74", "One or more Order Lines have a Confirmation Date after the Order Confirmation Date."));
				}
			}
		}

		protected override void CheckJD_Packs()
		{
			base.CheckJD_Packs();
			CompareValidation.CheckGreaterThanOrEqualTo(Parent.JD_PacksInfo, 0);
		}

		protected override void CheckJD_ActualVolume()
		{
			base.CheckJD_ActualVolume();
			CompareValidation.CheckGreaterThanOrEqualTo(Parent.JD_ActualVolumeInfo, 0);
		}

		protected override void CheckJD_ActualWeight()
		{
			base.CheckJD_ActualWeight();
			CompareValidation.CheckGreaterThanOrEqualTo(Parent.JD_ActualWeightInfo, 0);
		}

		protected override void CheckJD_F3_NKPackType()
		{
			base.CheckJD_F3_NKPackType();
			ListValidation.ErrorIfInvalidCode(Parent.JD_F3_NKPackTypeInfo, Parent.JD_F3_NKPackType_List);
		}

		protected override void CheckJD_UnitOfVolume()
		{
			base.CheckJD_UnitOfVolume();
			ListValidation.ErrorIfInvalidCode(Parent.JD_UnitOfVolumeInfo, Parent.JD_UnitOfVolume_List);
		}

		protected override void CheckJD_UnitOfWeight()
		{
			base.CheckJD_UnitOfWeight();
			ListValidation.ErrorIfInvalidCode(Parent.JD_UnitOfWeightInfo, Parent.JD_UnitOfWeight_List);
		}

		protected override void CheckJD_ShipmentWindowEnd()
		{
			base.CheckJD_ShipmentWindowEnd();

			if (Parent.JD_ShipmentWindowEnd < Parent.JD_ShipmentWindowStart)
			{
				Parent.JD_ShipmentWindowEndInfo.AddError(Res.GetString("2bd63426-47ff-efbe-952e-8778c39029ae", "Ship window start date must be earlier than or equal to ship window end date."));
			}
		}

		protected override void CheckJD_ShipmentWindowStart()
		{
			base.CheckJD_ShipmentWindowStart();

			if (Parent.JD_ShipmentWindowEnd < Parent.JD_ShipmentWindowStart)
			{
				Parent.JD_ShipmentWindowStartInfo.AddError(Res.GetString("d9769e18-1d4b-4ce8-22ef-2a60de7efbe8", "Ship window start date must be earlier than or equal to ship window end date."));
			}
		}

		#endregion

		protected override void CheckJD_OA_BuyerAddress()
		{
			base.CheckJD_OA_BuyerAddress();

			if (Parent.JD_OA_SupplierAddress.IsValid && Parent.JD_OA_BuyerAddress.IsValid)
			{
				CompareValidation.CheckNotEqual(Parent.JD_OA_BuyerAddressInfo, Parent.JD_OA_SupplierAddressInfo);
			}

			BuyerAndOrderNumAndSplitIsUniqueValidation.CheckBuyerAndOrderNumAndSplitIsUnique(Parent.JD_OA_BuyerAddressInfo);

			if (Parent.Shipment != null
				&& !Parent.Shipment.ConsigneePK.IsEmpty
				&& Parent.BuyerPK != Parent.Shipment.ConsigneePK)
			{
				Parent.JD_OA_BuyerAddressInfo.AddWarning(Res.GetString("9c0979e4-8724-464d-8e70-f61570f915eb", "Buyer differs from Shipment Consignee."));
			}

			if (Parent.Buyer != null && Parent.Buyer.MiscServ != null && !Parent.Buyer.MiscServ.OM_IMAllowOrders)
			{
				Parent.JD_OA_BuyerAddressInfo.AddError(Res.GetString("c2bb399e-7da2-48ca-bc16-f05ae699ccbd", "This buyer is restricted from using Order Manager (Organization > Consignee > Disallow Order Manager flag is on)"));
			}
		}

		protected override void CheckJD_OA_SupplierAddress()
		{
			base.CheckJD_OA_SupplierAddress();
			if (Parent.JD_OA_BuyerAddress.IsValid && Parent.JD_OA_SupplierAddress.IsValid)
			{
				CompareValidation.CheckNotEqual(Parent.JD_OA_SupplierAddressInfo, Parent.JD_OA_BuyerAddressInfo);
			}

			if (Parent.Shipment != null
				&& !Parent.Shipment.ConsignorPK.IsEmpty
				&& Parent.SupplierPK != Parent.Shipment.ConsignorPK)
			{
				Parent.JD_OA_SupplierAddressInfo.AddWarning(Res.GetString("b8c5f1c1-d4dd-49d7-99e9-315c5d488e4b", "Supplier differs from Shipment Consignor."));
			}
		}

		protected override void CheckJD_OH_SendingAgent()
		{
			base.CheckJD_OH_SendingAgent();
			if (!Parent.JD_OH_SendingAgent.IsEmpty && !Parent.JD_OH_ReceivingAgent.IsEmpty)
			{
				CompareValidation.CheckNotEqual(Parent.JD_OH_SendingAgentInfo, Parent.JD_OH_ReceivingAgentInfo);
			}
			MandatoryValidation.WarnIfNotEntered(Parent.JD_OH_SendingAgentInfo);
		}

		protected override void CheckJD_OH_ReceivingAgent()
		{
			base.CheckJD_OH_ReceivingAgent();
			if (!Parent.JD_OH_SendingAgent.IsEmpty && !Parent.JD_OH_ReceivingAgent.IsEmpty)
			{
				if (!Parent.JD_OH_SendingAgent.IsEmpty && !Parent.JD_OH_ReceivingAgent.IsEmpty)
				{
					if (!Parent.JD_OH_SendingAgent.IsEmpty && !Parent.JD_OH_ReceivingAgent.IsEmpty)
					{
						CompareValidation.CheckNotEqual(Parent.JD_OH_ReceivingAgentInfo, Parent.JD_OH_SendingAgentInfo);
					}
				}
			}

			MandatoryValidation.WarnIfNotEntered(Parent.JD_OH_ReceivingAgentInfo);
		}

		protected override void CheckJD_TransportMode()
		{
			base.CheckJD_TransportMode();
			ListValidation.ErrorIfInvalidCode(Parent.JD_TransportModeInfo, Parent.JD_TransportMode_List);
			MandatoryValidation.WarnIfNotEntered(Parent.JD_TransportModeInfo);

			ValidateJD_RV_NKArrivalVessel();
			ValidateJD_RV_NKIntermediateVessel();
			ValidateJD_RV_NKDepartureVessel();
			ValidateJD_ContainerMode();
		}

		protected override void CheckJD_ContainerMode()
		{
			base.CheckJD_ContainerMode();

			if (Parent.JD_TransportMode.IsValid)
			{
				ListValidation.ErrorIfInvalidCode(Parent.JD_ContainerModeInfo, Parent.JD_ContainerMode_List);
				MandatoryValidation.CheckEntered(Parent.JD_ContainerModeInfo);
			}
			else if (Parent.JD_ContainerMode.IsValid)
			{
				string error = Res.GetString("5b19b94e-2451-4742-8e54-4b1ecd8da520", "Please do not enter a {0} when there is no {1} selected.",
					Parent.JD_ContainerModeInfo.Description, Parent.JD_TransportModeInfo.Description);

				Parent.JD_ContainerModeInfo.AddError(error);
			}
		}

		protected override void CheckJD_IncoTerm()
		{
			base.CheckJD_IncoTerm();
			ListValidation.ErrorIfInvalidCode(Parent.JD_IncoTermInfo, Parent.JD_IncoTerm_List);
			IncotermValidation.Instance.WarningIfExpired(Parent.JD_IncoTermInfo);
		}

		protected override void CheckJD_OrderNumber()
		{
			base.CheckJD_OrderNumber();
			OrderNumberValidation.ErrorIfOrderNumberNotValid(Parent.JD_OrderNumberInfo);
			BuyerAndOrderNumAndSplitIsUniqueValidation.CheckBuyerAndOrderNumAndSplitIsUnique(Parent.JD_OrderNumberInfo);
		}

		protected override void CheckJD_OrderNumberSplit()
		{
			base.CheckJD_OrderNumberSplit();
			ValidateJD_OrderNumber();
			BuyerAndOrderNumAndSplitIsUniqueValidation.CheckBuyerAndOrderNumAndSplitIsUnique(Parent.JD_OrderNumberSplitInfo);
		}

		protected override void CheckJD_OrderStatus()
		{
			base.CheckJD_OrderStatus();
			ListValidation.ErrorIfInvalidCode(Parent.JD_OrderStatusInfo, Parent.JD_OrderStatus_List);
			MandatoryValidation.CheckEntered(Parent.JD_OrderStatusInfo);
		}

		protected override void CheckJD_JS()
		{
			base.CheckJD_JS();

			if (Parent.Shipment != null)
			{
				CheckParentOrderCollectionCount(new OrdersOnShipmentLimitHelper(Parent.Shipment), Parent.JD_JSInfo);

				if (!Parent.CanAttachOrderToShipmentWithNonMatchingControllingCustomer(Parent.Shipment, out var errorMessage))
				{
					Parent.JD_JSInfo.AddError(errorMessage);
				}
			}
		}

		protected override void CheckJD_JE()
		{
			base.CheckJD_JE();

			if (Parent.Declaration != null)
			{
				CheckParentOrderCollectionCount(new OrdersOnDeclarationLimitHelper((Enterprise.Integration.Customs.IBaseJobDeclaration)Parent.Declaration), Parent.JD_JEInfo);
			}
		}

		void CheckParentOrderCollectionCount(CollectionLimitHelperForPotentialHVLV helper, ZPropertyInfo propertyInfo)
		{
			var notification = helper.CreateNotification();

			if (notification != null)
			{
				propertyInfo.AddNotification(notification.Type, notification.Message);
			}
		}

		#region Custom Label Mandatory Validation

		protected CustomLabelPropertyValidation CustomLabelPropertyValidation
		{
			get { return customLabelPropertyValidation ?? (customLabelPropertyValidation = new CustomLabelPropertyValidation(Parent.Factory)); }
		}
		CustomLabelPropertyValidation customLabelPropertyValidation;

		protected override void CheckJD_CustomAttrib1()
		{
			base.CheckJD_CustomAttrib1();
			if (!Globals.IsWeb)
			{
				CustomLabelPropertyValidation.Validate(new Order.CustomLabelsProvider(Parent, false), Parent.JD_CustomAttrib1Info);
			}
		}

		protected override void CheckJD_CustomAttrib2()
		{
			base.CheckJD_CustomAttrib2();
			if (!Globals.IsWeb)
			{
				CustomLabelPropertyValidation.Validate(new Order.CustomLabelsProvider(Parent, false), Parent.JD_CustomAttrib2Info);
			}
		}

		protected override void CheckJD_CustomAttrib3()
		{
			base.CheckJD_CustomAttrib3();
			if (!Globals.IsWeb)
			{
				CustomLabelPropertyValidation.Validate(new Order.CustomLabelsProvider(Parent, false), Parent.JD_CustomAttrib3Info);
			}
		}

		protected override void CheckJD_CustomAttrib4()
		{
			base.CheckJD_CustomAttrib4();
			if (!Globals.IsWeb)
			{
				CustomLabelPropertyValidation.Validate(new Order.CustomLabelsProvider(Parent, false), Parent.JD_CustomAttrib4Info);
			}
		}

		protected override void CheckJD_CustomAttrib5()
		{
			base.CheckJD_CustomAttrib5();
			if (!Globals.IsWeb)
			{
				CustomLabelPropertyValidation.Validate(new Order.CustomLabelsProvider(Parent, false), Parent.JD_CustomAttrib5Info);
			}
		}

		protected override void CheckJD_CustomDecimal1()
		{
			base.CheckJD_CustomDecimal1();
			if (!Globals.IsWeb)
			{
				CustomLabelPropertyValidation.Validate(new Order.CustomLabelsProvider(Parent, false), Parent.JD_CustomDecimal1Info);
			}
		}

		protected override void CheckJD_CustomDecimal2()
		{
			base.CheckJD_CustomDecimal2();
			if (!Globals.IsWeb)
			{
				CustomLabelPropertyValidation.Validate(new Order.CustomLabelsProvider(Parent, false), Parent.JD_CustomDecimal2Info);
			}
		}

		protected override void CheckJD_CustomDecimal3()
		{
			base.CheckJD_CustomDecimal3();
			if (!Globals.IsWeb)
			{
				CustomLabelPropertyValidation.Validate(new Order.CustomLabelsProvider(Parent, false), Parent.JD_CustomDecimal3Info);
			}
		}

		protected override void CheckJD_CustomDecimal4()
		{
			base.CheckJD_CustomDecimal4();
			if (!Globals.IsWeb)
			{
				CustomLabelPropertyValidation.Validate(new Order.CustomLabelsProvider(Parent, false), Parent.JD_CustomDecimal4Info);
			}
		}

		protected override void CheckJD_CustomDecimal5()
		{
			base.CheckJD_CustomDecimal5();
			if (!Globals.IsWeb)
			{
				CustomLabelPropertyValidation.Validate(new Order.CustomLabelsProvider(Parent, false), Parent.JD_CustomDecimal5Info);
			}
		}

		protected override void CheckJD_EstimateUserDate1()
		{
			base.CheckJD_EstimateUserDate1();
			if (!Globals.IsWeb)
			{
				CustomLabelPropertyValidation.Validate(new Order.CustomLabelsProvider(Parent, false), Parent.JD_EstimateUserDate1Info);
			}
		}

		protected override void CheckJD_EstimateUserDate2()
		{
			base.CheckJD_EstimateUserDate2();
			if (!Globals.IsWeb)
			{
				CustomLabelPropertyValidation.Validate(new Order.CustomLabelsProvider(Parent, false), Parent.JD_EstimateUserDate2Info);
			}
		}

		protected override void CheckJD_EstimateUserDate3()
		{
			base.CheckJD_EstimateUserDate3();
			if (!Globals.IsWeb)
			{
				CustomLabelPropertyValidation.Validate(new Order.CustomLabelsProvider(Parent, false), Parent.JD_EstimateUserDate3Info);
			}
		}

		protected override void CheckJD_EstimateUserDate4()
		{
			base.CheckJD_EstimateUserDate4();
			if (!Globals.IsWeb)
			{
				CustomLabelPropertyValidation.Validate(new Order.CustomLabelsProvider(Parent, false), Parent.JD_EstimateUserDate4Info);
			}
		}

		protected override void CheckJD_ActualUserDate1()
		{
			base.CheckJD_ActualUserDate1();
			if (!Globals.IsWeb)
			{
				CustomLabelPropertyValidation.Validate(new Order.CustomLabelsProvider(Parent, false), Parent.JD_ActualUserDate1Info);
			}
		}

		protected override void CheckJD_ActualUserDate2()
		{
			base.CheckJD_ActualUserDate2();
			if (!Globals.IsWeb)
			{
				CustomLabelPropertyValidation.Validate(new Order.CustomLabelsProvider(Parent, false), Parent.JD_ActualUserDate2Info);
			}
		}

		protected override void CheckJD_ActualUserDate3()
		{
			base.CheckJD_ActualUserDate3();
			if (!Globals.IsWeb)
			{
				CustomLabelPropertyValidation.Validate(new Order.CustomLabelsProvider(Parent, false), Parent.JD_ActualUserDate3Info);
			}
		}

		protected override void CheckJD_ActualUserDate4()
		{
			base.CheckJD_ActualUserDate4();
			if (!Globals.IsWeb)
			{
				CustomLabelPropertyValidation.Validate(new Order.CustomLabelsProvider(Parent, false), Parent.JD_ActualUserDate4Info);
			}
		}

		protected override void CheckJD_CustomDate1()
		{
			base.CheckJD_CustomDate1();
			if (!Globals.IsWeb)
			{
				CustomLabelPropertyValidation.Validate(new Order.CustomLabelsProvider(Parent, false), Parent.JD_CustomDate1Info);
			}
		}

		protected override void CheckJD_CustomDate2()
		{
			base.CheckJD_CustomDate2();
			if (!Globals.IsWeb)
			{
				CustomLabelPropertyValidation.Validate(new Order.CustomLabelsProvider(Parent, false), Parent.JD_CustomDate2Info);
			}
		}

		protected override void CheckJD_RL_NKGoodsAvailableAt()
		{
			base.CheckJD_RL_NKGoodsAvailableAt();

			if (Parent.Shipment != null &&
				!Parent.JD_RL_NKGoodsAvailableAt.IsEmpty &&
				!Parent.Shipment.JS_RL_NKOrigin.IsEmpty &&
				Parent.JD_RL_NKGoodsAvailableAt != Parent.Shipment.JS_RL_NKOrigin)
			{
				Parent.JD_RL_NKGoodsAvailableAtInfo.AddWarning(Res.GetString("565c3439-1083-449f-bed4-a1df2cd2dcf8", "Goods Available Location differs from Shipment Origin."));
			}
			ListValidation.ErrorIfInvalidCode(Parent.JD_RL_NKGoodsAvailableAtInfo, Parent.Lookups.GoodsAvailableAts);
		}

		protected override void CheckJD_RL_NKGoodsDeliveredTo()
		{
			base.CheckJD_RL_NKGoodsDeliveredTo();

			if (Parent.Shipment != null &&
				!Parent.JD_RL_NKGoodsDeliveredTo.IsEmpty &&
				!Parent.Shipment.JS_RL_NKDestination.IsEmpty &&
				Parent.JD_RL_NKGoodsDeliveredTo != Parent.Shipment.JS_RL_NKDestination)
			{
				Parent.JD_RL_NKGoodsDeliveredToInfo.AddWarning(Res.GetString("c4c91dd4-1636-458f-b6fe-c08f937cc516", "Goods Delivered Location differs from Shipment Destination."));
			}
			ListValidation.ErrorIfInvalidCode(Parent.JD_RL_NKGoodsDeliveredToInfo, Parent.Lookups.GoodsDeliveredTos);
		}

		protected override void CheckJD_RL_NKPortOfLoading()
		{
			base.CheckJD_RL_NKPortOfLoading();
			ListValidation.ErrorIfInvalidCode(Parent.JD_RL_NKPortOfLoadingInfo, Parent.Lookups.PortOfLoadings);
		}

		protected override void CheckJD_RL_NKPortOfDischarge()
		{
			base.CheckJD_RL_NKPortOfDischarge();
			ListValidation.ErrorIfInvalidCode(Parent.JD_RL_NKPortOfDischargeInfo, Parent.Lookups.PortOfDischarges);
		}

		protected override void CheckJD_RS_NKServiceLevel_NI()
		{
			base.CheckJD_RS_NKServiceLevel_NI();
			ListValidation.ErrorIfInvalidCode(Parent.JD_RS_NKServiceLevel_NIInfo, Parent.Lookups.ServiceLevel_NIs, ResString.GetMultilingualString("4597c58c-04cc-46d9-ac14-22f4429b4738", "Please enter a valid Service Level."));
		}

		protected override void CheckJD_RX_NKOrderCurrency()
		{
			base.CheckJD_RX_NKOrderCurrency();
			Parent.JD_RX_NKOrderCurrencyInfo.RunAdditionalValidation();
			ListValidation.ErrorIfInvalidCode(Parent.JD_RX_NKOrderCurrencyInfo);
		}

		protected override void CheckJD_EstimatedExchangeRate()
		{
			base.CheckJD_EstimatedExchangeRate();
			Parent.JD_EstimatedExchangeRateInfo.RunAdditionalValidation();
		}

		#endregion

		#region ValidateControllingAgentPK

		public void ValidateControllingAgentPK(JobDocAddressValidation validation)
		{
			Argument.NotNull(validation, nameof(validation));
			Argument.NotNull(validation.Parent, nameof(validation.Parent));

			var docAddress = validation.Parent;

			if (docAddress.HasChanges
				&& OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.Value
				&& docAddress.Organisation != null
				&& !docAddress.Organisation.OH_IsControllingAgent)
			{
				docAddress.OrganisationPKInfo.AddError(IsNotValidControllingAgentErrorMessage);
			}
		}

		string IsNotValidControllingAgentErrorMessage
		{
			get { return Res.GetString("98D5A138-4E04-4645-AEA9-5EF071F9BE19", "This organization is not a valid Controlling Agent"); }
		}

		#endregion

		public void RunFormShowValidation()
		{
			ValidateJD_Milestone_E_DEP();
			ValidateJD_Milestone_E_ARV();
			ValidateJD_E_ARV_1stIntermediate();
			ValidateJD_E_ARV_2ndIntermediate();
			ValidateJD_E_DEP_2();
			ValidateJD_E_DEP_3();
			ValidateJD_ActualUserDate1();
			ValidateJD_EstimateUserDate1();
			ValidateJD_ActualUserDate2();
			ValidateJD_EstimateUserDate2();
		}

		#region ValidateControllingCustomerPK

		public void ValidateControllingCustomerPK(JobDocAddressValidation validation)
		{
			Argument.NotNull(validation, nameof(validation));
			Argument.NotNull(validation.Parent, nameof(validation.Parent));

			var docAddress = validation.Parent;
			if (docAddress.HasChanges
				&& OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.Value
				&& docAddress.Organisation != null
				&& !docAddress.Organisation.OH_IsControllingCustomer)
			{
				docAddress.OrganisationPKInfo.AddError(Res.GetString("02359b99-190e-42d9-b20d-39cae0221e4c", "This organization is not a valid Controlling Customer"));
			}
		}

		#endregion

	}
}
