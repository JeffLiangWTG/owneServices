using System;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	class JobOrderHeaderValidationTest : BusinessObjectValidationTestCase
	{
		#region Validation

		public void TestAttachOrderToShipmentWithNonMatchingControllingCustomer()
		{
			var securityInstance = SecurityTestHelper.CreateSecurityInstance(Factory);
			securityInstance.AllowAttachOrdersWithNonMatchingControllingCustomerForBooking.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(securityInstance))
			{
				var order = Factory.New<Order>();
				var shipment = Factory.New<ForwardingShipment>();
				var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
				controllingCustomer.OH_Code = "CCM";
				var controllingCustomerAddress = Factory.NewWithValidTestData<OrgAddress>();
				controllingCustomerAddress.OA_OH = controllingCustomer.PK;
				controllingCustomerAddress.OA_Code = "CCM";
				order.ControllingCustomerDocAddress.E2_OA_Address = controllingCustomerAddress.PK;
				order.JD_JS = shipment.PK;

				order.RunPreSaveValidation();
				AssertHasError(order.JD_JSInfo, "You do not have security rights to allow order and its shipment have non-matching Controlling Customers.");
			}
		}

		public void TestValidateJD_OrderNumber()
		{
			Order order = Factory.New<Order>();
			order.JD_OrderNumber = "213,das";
			AssertEquals("Order number invalid, should have error", true, order.HasErrors);

			order.JD_OrderNumber = "adsjk231/\\";
			AssertEquals("Order number valid, shouldn't have errors", false, order.HasErrors);
		}

		public void TestValidateJD_RN_NKCountryOfSupply()
		{
			Order order = Factory.New<Order>();

			order.JD_RN_NKCountryOfSupply = "YY";
			AssertHasErrors(order.JD_RN_NKCountryOfSupplyInfo);

			order.JD_RN_NKCountryOfSupply = "";
			AssertNoErrors(order.JD_RN_NKCountryOfSupplyInfo);

			order.JD_RN_NKCountryOfSupply = "AU";
			AssertNoErrors(order.JD_RN_NKCountryOfSupplyInfo);
		}

		public void TestValidateBuyerAndSupplierNotSame()
		{
			var org = BO.Factory.LoadTop1<OrgHeader>(new ZQuery());

			BO.BuyerPK = org.PK;
			BO.SupplierPK = org.PK;
			AssertEquals("Buyer and supplier cannot be the same", true, BO.JD_OA_SupplierAddressInfo.HasErrors());

			BO.BuyerPK = ZGuid.NewZGuid();
			AssertEquals("Error should be cleared", false, BO.JD_OA_SupplierAddressInfo.HasErrors());

			BO.BuyerPK = org.PK;
			BO.SupplierPK = org.PK;

			BO.SupplierPK = ZGuid.NewZGuid();
			AssertEquals("Error should be cleared", false, BO.JD_OA_BuyerAddressInfo.HasErrors());
		}

		public void TestJD_OH_BuyerRestrictedOrderManagerError()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MiscServ.OM_IMDisallowOrders = true;

			BO.BuyerPK = org.PK;
			AssertHasError(BO.JD_OA_BuyerAddressInfo, "This buyer is restricted from using Order Manager (Organization > Consignee > Disallow Order Manager flag is on)");

			org.MiscServ.OM_IMDisallowOrders = false;
			BO.BuyerPK = ZGuid.Empty;

			BO.BuyerPK = org.PK;
			AssertNoErrors(BO.JD_OA_BuyerAddressInfo);
		}

		public void TestValidateSenderAndReceiverAgentNotSame()
		{
			var org = BO.Factory.LoadTop1<OrgHeader>(new ZQuery());
			BO.JD_OH_ReceivingAgent = org.PK;
			BO.JD_OH_SendingAgent = org.PK;
			if (BO.JD_OH_ReceivingAgent.IsEmpty || BO.JD_OH_SendingAgent.IsEmpty)
			{
				BO.JD_OH_ReceivingAgent = org.PK;
				BO.JD_OH_SendingAgent = org.PK;
			}
			AssertEquals("Buyer and supplier cannot be the same. Org PK is:" + org.PK + ", SendingAgent is: " + BO.JD_OH_SendingAgent + ", Receiving Agnet is:" + BO.JD_OH_ReceivingAgent + "", true, BO.JD_OH_ReceivingAgentInfo.HasErrors());
			BO.JD_OH_SendingAgent = ZGuid.NewZGuid();
			AssertEquals("Error should be cleared", false, BO.JD_OH_ReceivingAgentInfo.HasErrors());

			BO.JD_OH_SendingAgent = org.PK;
			BO.JD_OH_ReceivingAgent = org.PK;

			BO.JD_OH_ReceivingAgent = ZGuid.NewZGuid();
			AssertEquals("Error should be cleared", false, BO.JD_OH_SendingAgentInfo.HasErrors());
		}

		public void TestValidateTransportAndContainerMode()
		{
			Assert(!BO.HasErrors);

			BO.JD_TransportMode = Constants.TransportModes.Air;
			BO.JD_ContainerMode = Constants.ContainerModes.Loose;
			Assert(!BO.HasErrors);

			BO.JD_TransportMode = Constants.TransportModes.Air;
			BO.JD_ContainerMode = Constants.ContainerModes.AIR;
			Assert(BO.HasErrors);

			BO.JD_TransportMode = Constants.TransportModes.Air;
			BO.JD_ContainerMode = "";
			Assert(BO.HasErrors);

			BO.JD_TransportMode = Constants.TransportModes.Sea;
			BO.JD_ContainerMode = Constants.ContainerModes.FCL;
			Assert(!BO.HasErrors);

			BO.JD_TransportMode = "";
			BO.JD_ContainerMode = Constants.ContainerModes.AIR;
			Assert(BO.HasErrors);

			BO.JD_TransportMode = Constants.TransportModes.Unknown;
			BO.JD_ContainerMode = Constants.ContainerModes.FCL;
			Assert(!BO.HasErrors);
		}

		public void TestBuyerAndOrderNumAndSplitIsUniqueValidation()
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.BuyerPK = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			order.JD_OrderNumber = "someorder";
			order.JD_OrderNumberSplit = 1;

			Factory.Save();

			var order2 = Factory.NewWithValidTestData<Order>();
			order2.BuyerPK = order.BuyerPK;
			order2.JD_OrderNumber = "someorder";
			order2.JD_OrderNumberSplit = 1;

			AssertEquals("Should have an error about uniqueness validation", true, order2.JD_OrderNumberInfo.HasErrors());
			order2.JD_OrderNumberSplit = 2;
			AssertEquals("Should no longer have an error about uniqueness validation", false, order2.JD_OrderNumberInfo.HasErrors());
		}

		public void TestSuspendValidation_WhenDepArvDatesNotVisible()
		{
			BO.JD_DepartureVoyage = "x";
			BO.JD_IntermediateVoyage = "y";
			BO.JD_ArrivalVoyage = "z";

			BO.JD_Milestone_E_DEP = ZDateTime.Now;
			BO.JD_E_ARV_1stIntermediate = ZDateTime.Now.AddDays(-1);
			BO.JD_E_DEP_2 = ZDateTime.Now.AddDays(-2);
			BO.JD_E_ARV_2ndIntermediate = ZDateTime.Now.AddDays(-3);
			BO.JD_E_DEP_3 = ZDateTime.Now.AddDays(-4);
			BO.JD_Milestone_E_ARV = ZDateTime.Now.AddDays(-5);

			BO.RunPreSaveValidation();
			AssertEquals(true, BO.JD_E_ARV_1stIntermediateInfo.HasNotifications());
			AssertEquals(true, BO.JD_E_ARV_2ndIntermediateInfo.HasNotifications());

			BO.JD_IntermediateVoyage = "";
			BO.RunPreSaveValidation();
			AssertEquals(true, BO.JD_E_ARV_1stIntermediateInfo.HasNotifications());
			AssertEquals(false, BO.JD_E_ARV_2ndIntermediateInfo.HasNotifications());
			AssertEquals(false, BO.JD_E_DEP_2Info.HasNotifications());
			AssertEquals(false, BO.JD_E_DEP_3Info.HasNotifications());

			BO.JD_ArrivalVoyage = "";
			BO.RunPreSaveValidation();
			AssertEquals(false, BO.JD_E_ARV_1stIntermediateInfo.HasNotifications());
			AssertEquals(false, BO.JD_E_ARV_2ndIntermediateInfo.HasNotifications());
			AssertEquals(false, BO.JD_E_DEP_2Info.HasNotifications());
			AssertEquals(false, BO.JD_E_DEP_3Info.HasNotifications());
		}

		public void TestValidateTransportArrivalDateValid()
		{
			BO.JD_TransportMode = Constants.TransportModes.Air;
			BO.JD_Milestone_E_DEP = ZDateTime.Now;
			BO.JD_Milestone_E_ARV = ZDateTime.Now.AddSeconds(5);
			BO.Validation.ValidateJD_Milestone_E_ARV();
			AssertEquals("Air, arv slightly after dep", false, BO.JD_Milestone_E_ARVInfo.HasErrors());

			BO.JD_TransportMode = Constants.TransportModes.Air;
			BO.JD_Milestone_E_DEP = ZDateTime.Now.AddSeconds(5);
			BO.JD_Milestone_E_ARV = ZDateTime.Now;
			BO.Validation.ValidateJD_Milestone_E_ARV();
			AssertEquals("Air, arv slightly before dep", false, BO.JD_Milestone_E_ARVInfo.HasErrors());

			BO.JD_TransportMode = Constants.TransportModes.Sea;
			BO.JD_Milestone_E_DEP = ZDateTime.Now.AddDays(1).AddSeconds(5);
			BO.JD_Milestone_E_ARV = ZDateTime.Now;
			BO.Validation.ValidateJD_Milestone_E_ARV();
			AssertEquals("Sea, arv 1 day before dep (allowed for air)", true, BO.JD_Milestone_E_ARVInfo.HasErrors());

			BO.JD_TransportMode = Constants.TransportModes.AirSea;
			BO.JD_Milestone_E_DEP = ZDateTime.Now.AddSeconds(5);
			BO.JD_Milestone_E_ARV = ZDateTime.Now;
			BO.Validation.ValidateJD_Milestone_E_ARV();
			AssertEquals("AirSea, arv slightly before dep", false, BO.JD_Milestone_E_ARVInfo.HasErrors());

			BO.JD_TransportMode = Constants.TransportModes.Air;
			BO.JD_Milestone_E_DEP = ZDateTime.Now.AddDays(1).AddMinutes(1);
			BO.JD_Milestone_E_ARV = ZDateTime.Now;
			BO.Validation.ValidateJD_Milestone_E_ARV();
			AssertEquals("Air, arv slightly before dep+1day", true, BO.JD_Milestone_E_ARVInfo.HasErrors());

			BO.JD_TransportMode = Constants.TransportModes.Air;
			BO.JD_Milestone_E_DEP = ZDateTime.Now;
			BO.JD_Milestone_E_ARV = ZDateTime.Now.AddMonths(6).AddMinutes(-1);
			BO.Validation.ValidateJD_Milestone_E_ARV();
			AssertEquals("Air, not quite arv and dep 6 months apart", false, BO.JD_Milestone_E_ARVInfo.HasErrors());

			BO.JD_TransportMode = Constants.TransportModes.Air;
			BO.JD_Milestone_E_DEP = ZDateTime.Now;
			BO.JD_Milestone_E_ARV = ZDateTime.Now.AddMonths(6).AddMinutes(1);
			BO.Validation.ValidateJD_Milestone_E_ARV();
			AssertEquals("Air, arv and dep 6 months apart", true, BO.JD_Milestone_E_ARVInfo.HasErrors());

			BO.JD_TransportMode = Constants.TransportModes.Sea;
			BO.JD_Milestone_E_DEP = ZDateTime.Now;
			BO.JD_Milestone_E_ARV = ZDateTime.Now.AddMonths(6).AddMinutes(-1);
			BO.Validation.ValidateJD_Milestone_E_ARV();
			AssertEquals("Sea, not quite arv and dep 6 months apart", false, BO.JD_Milestone_E_ARVInfo.HasErrors());

			BO.JD_TransportMode = Constants.TransportModes.Sea;
			BO.JD_Milestone_E_DEP = ZDateTime.Now;
			BO.JD_Milestone_E_ARV = ZDateTime.Now.AddMonths(6).AddMinutes(1);
			BO.Validation.ValidateJD_Milestone_E_ARV();
			AssertEquals("Sea, arv and dep 6 months apart", true, BO.JD_Milestone_E_ARVInfo.HasErrors());
		}

		public void TestValidateExchangeRate()
		{
			BO.JD_Calc_Currency.Currency = "USD";
			BO.JD_Calc_Currency.Rate = 0.65m;
			AssertNoErrors("Should be no error on Currency", BO.JD_RX_NKOrderCurrencyInfo);
			AssertNoErrors("Should be no error on exchange rate", BO.JD_EstimatedExchangeRateInfo);

			BO.JD_Calc_Currency.Rate = 0m;
			AssertNoErrors("Should be no error on currency", BO.JD_RX_NKOrderCurrencyInfo);
			AssertNoErrors("Should be no error on exchange rate", BO.JD_EstimatedExchangeRateInfo);

			BO.JD_Calc_Currency.Rate = 0.65m;
			BO.JD_Calc_Currency.Currency = "XXX";
			AssertHasErrors("Should be error on currency - invalid", BO.JD_RX_NKOrderCurrencyInfo);
			AssertNoErrors("Should be no error on exchange rate", BO.JD_EstimatedExchangeRateInfo);
		}

		[TestDate(2019, 1, 1)]
		public void TestValidateJD_IncoTerm_Pre2020()
		{
			BO.JD_IncoTerm = "XXX";
			AssertHasError(BO.JD_IncoTermInfo, "Enter a valid Incoterm.");

			BO.JD_IncoTerm = "DDU";
			AssertHasError(BO.JD_IncoTermInfo, "Enter a valid Incoterm.");

			BO.JD_IncoTerm = "DAT";
			AssertNoNotifications(BO.JD_IncoTermInfo);

			BO.JD_IncoTerm = "FOB";
			AssertNoNotifications(BO.JD_IncoTermInfo);
		}

		[TestDate(2020, 1, 1)]
		public void TestValidateJD_IncoTerm_Post2020()
		{
			BO.JD_IncoTerm = "XXX";
			AssertHasError(BO.JD_IncoTermInfo, "Enter a valid Incoterm.");

			BO.JD_IncoTerm = "DDU";
			AssertHasError(BO.JD_IncoTermInfo, "Enter a valid Incoterm.");

			BO.JD_IncoTerm = "DAT";
			AssertHasWarning(BO.JD_IncoTermInfo, "This Incoterm is obsolete from 1 January 2020 according to the International Chamber of Commerce rules.");

			BO.JD_IncoTerm = "FOB";
			AssertNoNotifications(BO.JD_IncoTermInfo);
		}

		public void TestValidateJD_BookingConfDate()
		{
			ZDateTime testDate1 = ZDateTime.Now.AddDays(-1);
			ZDateTime testDate2 = ZDateTime.Now.AddDays(1);

			OrderLine line1 = BO.OrderLines.AddNew();
			OrderLine line2 = BO.OrderLines.AddNew();
			OrderLine line3 = BO.OrderLines.AddNew();

			line1.JO_ConfirmationDate = testDate1;
			line2.JO_ConfirmationDate = testDate2;

			BO.JD_BookingConfDate = ZDateTime.Now;
			AssertHasWarning(BO.JD_BookingConfDateInfo, "One or more Order Lines have a Confirmation Date after the Order Confirmation Date.");

			BO.JD_BookingConfDate = ZDateTime.Now.AddDays(2);
			AssertNoWarnings(BO.JD_BookingConfDateInfo);

			BO.JD_BookingConfDate = testDate2;
			AssertNoWarnings(BO.JD_BookingConfDateInfo);
		}

		public void TestValidateJD_Packs()
		{
			BO.JD_Packs = 2;
			AssertNoErrors(BO.JD_PacksInfo);

			BO.JD_Packs = -2;
			AssertHasErrors("Should have an error when JD_Packs is negative", BO.JD_PacksInfo);
		}

		public void TestValidateJD_ActualVolume()
		{
			BO.JD_ActualVolume = 2;
			AssertNoErrors(BO.JD_ActualVolumeInfo);

			BO.JD_ActualVolume = -2;
			AssertHasErrors("Should have an error when JD_ActualVolume is negative", BO.JD_ActualVolumeInfo);
		}

		public void TestJD_ActualWeight()
		{
			BO.JD_ActualWeight = 2;
			AssertNoErrors(BO.JD_ActualWeightInfo);

			BO.JD_ActualWeight = -2;
			AssertHasErrors("Should have an error when JD_ActualWeight is negative", BO.JD_ActualWeightInfo);
		}

		public void TestJD_F3_NKPackType()
		{
			BO.JD_F3_NKPackType = "BAG";
			AssertNoErrors(BO.JD_F3_NKPackTypeInfo);

			BO.JD_F3_NKPackType = "xx";
			AssertHasErrors("Should have an error when JD_F3_NKPackType is invalid", BO.JD_F3_NKPackTypeInfo);

			BO.JD_F3_NKPackType = string.Empty;
			AssertNoErrors("Should have no error if empty", BO.JD_F3_NKPackTypeInfo);
		}

		public void TestJD_UnitOfVolume()
		{
			BO.JD_UnitOfVolume = "L";
			AssertNoErrors(BO.JD_UnitOfVolumeInfo);

			BO.JD_UnitOfVolume = "xx";
			AssertHasErrors("Should have an error when JD_UnitOfVolume is invalid", BO.JD_UnitOfVolumeInfo);

			BO.JD_UnitOfVolume = string.Empty;
			AssertNoErrors("Should have no error if empty", BO.JD_UnitOfVolumeInfo);
		}

		public void TestJD_UnitWeight()
		{
			BO.JD_UnitOfWeight = "KG";
			AssertNoErrors(BO.JD_UnitOfWeightInfo);

			BO.JD_UnitOfWeight = "xx";
			AssertHasErrors("Should have an error when JD_UnitOfWeight is invalid", BO.JD_UnitOfWeightInfo);

			BO.JD_UnitOfWeight = string.Empty;
			AssertNoErrors("Should have no error if empty", BO.JD_UnitOfWeightInfo);
		}

		public void TestJD_JS()
		{
			var shipment = Factory.New<ForwardingShipment>();
			TestParentOrderCollectionLimit(shipment, order => order.JD_JSInfo, FreightDataRegistry.Instance.OrdersPerShipmentLimit, FreightDataRegistry.Instance.OrdersPerShipmentLimitIntroductionTimeUTC);
		}

		public void TestJD_JE()
		{
			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			TestParentOrderCollectionLimit(declaration, order => order.JD_JEInfo, ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().OrdersPerDeclarationLimit, ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().OrdersPerDeclarationLimitIntroductionTimeUTC);
		}

		public void TestJD_RS_NKServiceLevel_NI()
		{
			var order = Factory.New<Order>();
			order.JD_RS_NKServiceLevel_NI = "AAA";

			AssertHasError(order.JD_RS_NKServiceLevel_NIInfo, "Please enter a valid Service Level.");

			order.JD_RS_NKServiceLevel_NI = "STD";
			AssertNoError(order.JD_RS_NKServiceLevel_NIInfo, "Please enter a valid Service Level.");
		}

		public void TestJD_ShipmentWindowEnd()
		{
			var order = Factory.New<Order>();
			order.JD_ShipmentWindowStart = new ZDate(2024, 4, 1);
			order.JD_ShipmentWindowEnd = new ZDate(2024, 3, 1);

			AssertHasError(order.JD_ShipmentWindowEndInfo, "Ship window start date must be earlier than or equal to ship window end date.");

			order.JD_ShipmentWindowEnd = new ZDate(2024, 5, 1);
			AssertNoError(order.JD_ShipmentWindowEndInfo, "Ship window start date must be earlier than or equal to ship window end date.");

			order.JD_ShipmentWindowEnd = new ZDate(2024, 4, 1);
			AssertNoError(order.JD_ShipmentWindowEndInfo, "Ship window start date must be earlier than or equal to ship window end date.");
		}

		public void TestJD_ShipmentWindowStart()
		{
			var order = Factory.New<Order>();
			order.JD_ShipmentWindowEnd = new ZDate(2024, 3, 1);
			order.JD_ShipmentWindowStart = new ZDate(2024, 4, 1);

			AssertHasError(order.JD_ShipmentWindowStartInfo, "Ship window start date must be earlier than or equal to ship window end date.");

			order.JD_ShipmentWindowStart = new ZDate(2024, 2, 1);
			AssertNoError(order.JD_ShipmentWindowStartInfo, "Ship window start date must be earlier than or equal to ship window end date.");

			order.JD_ShipmentWindowStart = new ZDate(2024, 3, 1);
			AssertNoError(order.JD_ShipmentWindowStartInfo, "Ship window start date must be earlier than or equal to ship window end date.");
		}

		public void TestValidateControllingAgent()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var notControllingAgent1 = Factory.NewWithValidTestData<OrgHeader>();
			notControllingAgent1.OH_Code = "org1";

			var notControllingAgent2 = Factory.NewWithValidTestData<OrgHeader>();
			notControllingAgent2.OH_Code = "org2";

			var controllingAgent = Factory.NewWithValidTestData<OrgHeader>();
			controllingAgent.OH_Code = "org3";
			controllingAgent.OH_IsControllingAgent = true;

			using (OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				order.ControllingAgentDocAddress.OrganisationPK = notControllingAgent1.PK;

				Assert("Should not have any error.", !order.HasErrors());
				Factory.Save();
			}

			using (OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Factory.Save();

				order.ControllingAgentDocAddress.OrganisationPK = notControllingAgent2.PK;
				order.Validation.ValidateAll();
				Assert("Should have error.", order.HasErrors());

				var error = order.GetErrors().GetFirst();
				AssertEquals("Error - OrganisationPK: This organization is not a valid Controlling Agent", error.Message);

				order.ControllingAgentDocAddress.OrganisationPK = controllingAgent.PK;
				Assert("Should not have any error.", !order.HasErrors());
			}
		}

		void TestParentOrderCollectionLimit(BusinessObject parent, Func<Order, ZPropertyInfo> getFKInfo, IRegistryItem limitRegistry, IRegistryItem introductionTimeRegistry)
		{
			Action<bool, bool> assertNotificationsUponAttachment = (hasWarnings, hasErrors) =>
			{
				var order = Factory.New<Order>();
				var fkInfo = getFKInfo(order);
				fkInfo.Value = parent.PK;
				AssertEquals(hasWarnings, fkInfo.HasWarnings());
				AssertEquals(hasErrors, fkInfo.HasErrors());
			};

			using (limitRegistry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 4))
			using (introductionTimeRegistry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Now.AddDays(-1)))
			{
				assertNotificationsUponAttachment(false, false);
				assertNotificationsUponAttachment(false, false);
				assertNotificationsUponAttachment(true, false);
				assertNotificationsUponAttachment(true, false);
				assertNotificationsUponAttachment(false, true);
			}
		}

		#endregion

		#region Vessel/Voyage Validation Tests

		public void TestIntermediateAndArrivalVoyagesReadOnly()
		{
			AssertEquals(true, BO.JD_IntermediateVoyageInfo.ReadOnly);

			BO.JD_DepartureVoyage = "x";
			AssertEquals(false, BO.JD_IntermediateVoyageInfo.ReadOnly);
			AssertEquals(false, BO.JD_ArrivalVoyageInfo.ReadOnly);

			BO.JD_ArrivalVoyage = "";
			BO.JD_IntermediateVoyage = "";
			BO.JD_DepartureVoyage = "";
			AssertEquals(true, BO.JD_IntermediateVoyageInfo.ReadOnly);
			AssertEquals(true, BO.JD_ArrivalVoyageInfo.ReadOnly);

			BO.JD_DepartureVoyage = "x";
			AssertEquals(false, BO.JD_IntermediateVoyageInfo.ReadOnly);
			AssertEquals(false, BO.JD_ArrivalVoyageInfo.ReadOnly);

			BO.JD_ArrivalVoyage = "";
			BO.JD_DepartureVoyage = "";
			AssertEquals(true, BO.JD_IntermediateVoyageInfo.ReadOnly);
			AssertEquals(true, BO.JD_ArrivalVoyageInfo.ReadOnly);

			BO.JD_ArrivalVoyage = "x";
			BO.JD_DepartureVoyage = "x";
			AssertEquals(false, BO.JD_IntermediateVoyageInfo.ReadOnly);
			AssertEquals(false, BO.JD_ArrivalVoyageInfo.ReadOnly);

			BO.JD_ArrivalVoyage = "";
			BO.JD_DepartureVoyage = "";
			AssertEquals(true, BO.JD_IntermediateVoyageInfo.ReadOnly);
			AssertEquals(true, BO.JD_ArrivalVoyageInfo.ReadOnly);
		}

		public void TestVesselsNotEnterableOnSea()
		{
			AssertEquals(false, BO.JD_RV_NKDepartureVesselInfo.HasErrors());
			AssertEquals(false, BO.JD_RV_NKIntermediateVesselInfo.HasErrors());
			AssertEquals(false, BO.JD_RV_NKArrivalVesselInfo.HasErrors());

			BO.JD_TransportMode = Constants.TransportModes.Sea;
			AssertEquals(false, BO.JD_RV_NKDepartureVesselInfo.HasErrors());
			AssertEquals(false, BO.JD_RV_NKIntermediateVesselInfo.HasErrors());
			AssertEquals(false, BO.JD_RV_NKArrivalVesselInfo.HasErrors());
			BO.JD_RV_NKDepartureVessel = "";
			AssertEquals(false, BO.JD_RV_NKDepartureVesselInfo.HasErrors());
			BO.JD_RV_NKDepartureVessel = "";
			AssertEquals(false, BO.JD_RV_NKIntermediateVesselInfo.HasErrors());
			BO.JD_RV_NKArrivalVessel = "";
			AssertEquals(false, BO.JD_RV_NKArrivalVesselInfo.HasErrors());

			BO.JD_TransportMode = Constants.TransportModes.Sea;
			AssertEquals(false, BO.JD_RV_NKDepartureVesselInfo.HasErrors());
			AssertEquals(false, BO.JD_RV_NKIntermediateVesselInfo.HasErrors());
			AssertEquals(false, BO.JD_RV_NKArrivalVesselInfo.HasErrors());
		}

		public void TestMustHaveVoyageIfVesselEntered()
		{
			AssertEquals(false, BO.JD_DepartureVoyageInfo.HasWarnings());
			BO.JD_RV_NKDepartureVessel = "X";
			AssertEquals(true, BO.JD_DepartureVoyageInfo.HasWarnings());
			BO.JD_RV_NKDepartureVessel = "";
			AssertEquals(false, BO.JD_DepartureVoyageInfo.HasWarnings());

			AssertEquals(false, BO.JD_IntermediateVoyageInfo.HasWarnings());
			BO.JD_RV_NKIntermediateVessel = "X";
			AssertEquals(true, BO.JD_IntermediateVoyageInfo.HasWarnings());
			BO.JD_RV_NKIntermediateVessel = "";
			AssertEquals(false, BO.JD_IntermediateVoyageInfo.HasWarnings());

			BO.JD_RV_NKArrivalVessel = "";
			AssertEquals(false, BO.JD_ArrivalVoyageInfo.HasWarnings());
			BO.JD_RV_NKArrivalVessel = "X";
			AssertEquals(true, BO.JD_ArrivalVoyageInfo.HasWarnings());
			BO.JD_RV_NKArrivalVessel = "";
			AssertEquals(false, BO.JD_ArrivalVoyageInfo.HasWarnings());
		}

		public void TestUnusedVoyagesAreNotUsedForValidation()
		{
			AssertEquals("Pre-condition JD_Milestone_E_ARV", false, BO.JD_Milestone_E_ARVInfo.HasErrors());
			AssertEquals("Pre-condition JD_E_ARV_2ndIntermediate", false, BO.JD_E_ARV_2ndIntermediateInfo.HasErrors());

			BO.JD_DepartureVoyage = "A123";
			BO.JD_Milestone_E_DEP = ZDateTime.Now;
			BO.JD_IntermediateVoyage = "B123";
			BO.JD_E_DEP_2 = new ZDateTime(DateTime.Now.AddMonths(-9));
			BO.JD_E_ARV_2ndIntermediate = new ZDateTime(DateTime.Now.AddDays(15));
			BO.JD_ArrivalVoyage = "C123";
			BO.JD_E_DEP_3 = new ZDateTime(DateTime.Now.AddYears(-1));
			BO.JD_Milestone_E_ARV = ZDateTime.Now.AddMonths(1);

			AssertHasErrorContaining(BO.JD_E_ARV_2ndIntermediateInfo, "This voyage length is far too long for any commercial transport");
			AssertHasErrorContaining(BO.JD_Milestone_E_ARVInfo, "This voyage length is far too long for any commercial transport");

			BO.JD_IntermediateVoyage = ZString.Empty;

			AssertEquals("Intermediate voyage is not used for validation", false, BO.JD_E_ARV_2ndIntermediateInfo.HasErrors());
			AssertHasErrorContaining(BO.JD_Milestone_E_ARVInfo, "This voyage length is far too long for any commercial transport");

			BO.JD_ArrivalVoyage = ZString.Empty;
			BO.Validation.ValidateJD_Milestone_E_ARV();

			AssertEquals("Arrival voyage is not used for validation", false, BO.JD_Milestone_E_ARVInfo.HasErrors());
		}

		public void TestConsignorIsSameAsSupplier()
		{
			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			var supplier2 = Factory.NewWithValidTestData<OrgHeader>();
			var shipment1 = Factory.New<ForwardingShipment>();
			var order = Factory.New<Order>();

			shipment1.ConsignorPK = supplier1.PK;
			order.JD_JS = shipment1.PK;
			order.SupplierPK = supplier2.PK;

			AssertHasWarnings(order.JD_OA_SupplierAddressInfo);
			order.SupplierPK = supplier1.PK;
			AssertNoWarnings(order.JD_OA_SupplierAddressInfo);
		}

		public void TestBuyerIsSameAsConsignee()
		{
			var buyer1 = Factory.NewWithValidTestData<OrgHeader>();
			var buyer2 = Factory.NewWithValidTestData<OrgHeader>();
			var shipment1 = Factory.New<ForwardingShipment>();
			var order = Factory.New<Order>();

			shipment1.ConsigneePK = buyer1.PK;
			order.JD_JS = shipment1.PK;
			order.BuyerPK = buyer2.PK;

			AssertHasWarnings(order.JD_OA_BuyerAddressInfo);
			order.BuyerPK = buyer1.PK;
			AssertNoWarnings(order.JD_OA_BuyerAddressInfo);
		}

		public void TestGoodsAvailableAtIsSameAsOrigin()
		{
			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_RL_NKOrigin = "AUSYD";

			Order order = Factory.New<Order>();
			order.JD_JS = shipment1.PK;
			order.JD_RL_NKGoodsAvailableAt = "USLAX";

			AssertHasWarnings(order.JD_RL_NKGoodsAvailableAtInfo);
			order.JD_RL_NKGoodsAvailableAt = shipment1.JS_RL_NKOrigin;
			AssertNoWarnings(order.JD_RL_NKGoodsAvailableAtInfo);
		}

		public void TestDestinationIsSameAsGoodsDeliveredTo()
		{
			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_RL_NKDestination = "USLAX";

			Order order = Factory.New<Order>();
			order.JD_JS = shipment1.PK;
			order.JD_RL_NKGoodsDeliveredTo = "AUSYD";

			AssertHasWarnings(order.JD_RL_NKGoodsDeliveredToInfo);
			order.JD_RL_NKGoodsDeliveredTo = shipment1.JS_RL_NKDestination;
			AssertNoWarnings(order.JD_RL_NKGoodsDeliveredToInfo);
		}

		public void TestVesselsEnteredValidation()
		{
			BO.JD_TransportMode = Constants.TransportModes.Sea;

			BO.JD_RV_NKDepartureVessel = "a";
			AssertEquals(false, BO.JD_RV_NKDepartureVesselInfo.HasErrors());
			AssertEquals(false, BO.JD_RV_NKIntermediateVesselInfo.HasErrors());
			AssertEquals(false, BO.JD_RV_NKArrivalVesselInfo.HasErrors());

			BO.JD_RV_NKIntermediateVessel = "b";
			BO.Validation.ValidateJD_RV_NKIntermediateVessel();
			AssertEquals(false, BO.JD_RV_NKDepartureVesselInfo.HasErrors());
			AssertEquals(false, BO.JD_RV_NKIntermediateVesselInfo.HasErrors());
			AssertEquals("Should not validate until tabbed into arrival control", false, BO.JD_RV_NKArrivalVesselInfo.HasErrors());
			BO.Validation.ValidateJD_RV_NKArrivalVessel();
			AssertEquals("Intermediate vessel specified, must specify arrival vessel", true, BO.JD_RV_NKArrivalVesselInfo.HasErrors());

			BO.JD_RV_NKArrivalVessel = "c";
			BO.Validation.ValidateJD_RV_NKArrivalVessel();
			AssertEquals(false, BO.JD_RV_NKDepartureVesselInfo.HasErrors());
			AssertEquals(false, BO.JD_RV_NKIntermediateVesselInfo.HasErrors());
			AssertEquals(false, BO.JD_RV_NKArrivalVesselInfo.HasErrors());

			BO.JD_RV_NKArrivalVessel = "";
			BO.Validation.ValidateJD_RV_NKArrivalVessel();
			AssertEquals(false, BO.JD_RV_NKDepartureVesselInfo.HasErrors());
			AssertEquals(false, BO.JD_RV_NKIntermediateVesselInfo.HasErrors());
			AssertEquals("Intermediate vessel specified, must specify arrival vessel", true, BO.JD_RV_NKArrivalVesselInfo.HasErrors());

			BO.JD_RV_NKDepartureVessel = "x";
			BO.JD_RV_NKIntermediateVessel = "";
			BO.Validation.ValidateJD_RV_NKDepartureVessel();
			BO.Validation.ValidateJD_RV_NKIntermediateVessel();
			BO.Validation.ValidateJD_RV_NKArrivalVessel();
			AssertEquals(false, BO.JD_RV_NKDepartureVesselInfo.HasErrors());
			AssertEquals(false, BO.JD_RV_NKIntermediateVesselInfo.HasErrors());
			AssertEquals(false, BO.JD_RV_NKArrivalVesselInfo.HasErrors());

			BO.JD_RV_NKIntermediateVessel = "";
			BO.JD_RV_NKArrivalVessel = "x";
			BO.Validation.ValidateJD_RV_NKIntermediateVessel();
			BO.Validation.ValidateJD_RV_NKArrivalVessel();
			AssertEquals(false, BO.JD_RV_NKArrivalVesselInfo.HasErrors());

			BO.JD_RV_NKIntermediateVessel = "i";
			BO.Validation.ValidateJD_RV_NKIntermediateVessel();
			AssertEquals("Should not validate until tabbed into arrival control", false, BO.JD_RV_NKArrivalVesselInfo.HasErrors());
			BO.Validation.ValidateJD_RV_NKArrivalVessel();
			AssertEquals("Intermediate vessel specified, must specify arrival vessel", true, BO.JD_RV_NKArrivalVesselInfo.HasErrors());

			BO.JD_RV_NKArrivalVessel = "z";
			BO.Validation.ValidateJD_RV_NKArrivalVessel();
			AssertEquals(false, BO.JD_RV_NKIntermediateVesselInfo.HasErrors());

			BO.JD_RV_NKDepartureVessel = "i";
			BO.Validation.ValidateJD_RV_NKDepartureVessel();
			AssertEquals("Intermediate vessel cannot be departure vessel", true, BO.JD_RV_NKIntermediateVesselInfo.HasErrors());

			BO.JD_RV_NKDepartureVessel = "t";
			BO.Validation.ValidateJD_RV_NKDepartureVessel();
			AssertEquals(false, BO.JD_RV_NKIntermediateVesselInfo.HasErrors());

			BO.JD_RV_NKArrivalVessel = "i";
			BO.Validation.ValidateJD_RV_NKArrivalVessel();
			AssertEquals("Intermediate vessel cannot be arrival vessel", true, BO.JD_RV_NKIntermediateVesselInfo.HasErrors());
		}

		public void TestSettingTransportModeAirClearsVessel()
		{
			Order order1 = Factory.New<Order>();
			ZString vessel1 = "Vessel1";
			ZString vessel2 = "Vessel2";
			ZString vessel3 = "Vessel3";
			order1.JD_RV_NKArrivalVessel = vessel1;
			order1.JD_RV_NKDepartureVessel = vessel2;
			order1.JD_RV_NKIntermediateVessel = vessel3;
			order1.JD_TransportMode = Core.Constants.TransportModes.Sea;

			AssertEquals("Order should have vessel name", vessel1, order1.JD_RV_NKArrivalVessel);
			AssertEquals("Order should have vessel name", vessel2, order1.JD_RV_NKDepartureVessel);
			AssertEquals("Order should have vessel name", vessel3, order1.JD_RV_NKIntermediateVessel);

			order1.JD_TransportMode = Core.Constants.TransportModes.Air;

			AssertEquals("Order should have blank vessel name", ZString.Empty, order1.JD_RV_NKArrivalVessel);
			AssertEquals("Order should have blank vessel name", ZString.Empty, order1.JD_RV_NKDepartureVessel);
			AssertEquals("Order should have blank vessel name", ZString.Empty, order1.JD_RV_NKIntermediateVessel);
		}

		public void TestValidateControllingCustomer()
		{
			using (OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var notControllingCustomer1 = Factory.NewWithValidTestData<OrgHeader>();
				notControllingCustomer1.OH_Code = "org1";

				var notControllingCustomer2 = Factory.NewWithValidTestData<OrgHeader>();
				notControllingCustomer2.OH_Code = "org2";

				var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
				controllingCustomer.OH_Code = "org3";
				controllingCustomer.OH_IsControllingCustomer = true;

				var order = Factory.NewWithValidTestData<Order>();
				order.ControllingCustomerDocAddress.OrganisationPK = notControllingCustomer1.PK;

				Assert("Should not have any error.", !order.HasErrors());
				Factory.Save();

				using (OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					order.Validation.ValidateAll();
					Assert("Should not have any error.", !order.HasErrors());
					Factory.Save();

					order.ControllingCustomerDocAddress.OrganisationPK = notControllingCustomer2.PK;
					order.Validation.ValidateAll();
					Assert("Should have an error.", order.HasErrors());

					var error = order.GetErrors().GetFirst();
					AssertEquals("Error - OrganisationPK: This organization is not a valid Controlling Customer", error.Message);

					order.ControllingCustomerDocAddress.OrganisationPK = controllingCustomer.PK;
					Assert("Should not have any error.", !order.HasErrors());
				}
			}
		}

		public void TestJD_RL_NKGoodsAvailableAt()
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.JD_RL_NKGoodsAvailableAt = "";
			AssertNoErrors(order.JD_RL_NKGoodsAvailableAtInfo);

			order.JD_RL_NKGoodsAvailableAt = "111";
			AssertHasErrors(order.JD_RL_NKGoodsAvailableAtInfo);

			order.JD_RL_NKGoodsAvailableAt = "USLAX";
			AssertNoErrors(order.JD_RL_NKGoodsAvailableAtInfo);
		}

		public void TestJD_RL_NKGoodsDeliveredTo()
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.JD_RL_NKGoodsDeliveredTo = "";
			AssertNoErrors(order.JD_RL_NKGoodsDeliveredToInfo);

			order.JD_RL_NKGoodsDeliveredTo = "111";
			AssertHasErrors(order.JD_RL_NKGoodsDeliveredToInfo);

			order.JD_RL_NKGoodsDeliveredTo = "USLAX";
			AssertNoErrors(order.JD_RL_NKGoodsDeliveredToInfo);
		}

		public void TestJD_RL_NKPortOfLoading()
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.JD_RL_NKPortOfLoading = "";
			AssertNoErrors(order.JD_RL_NKPortOfLoadingInfo);

			order.JD_RL_NKPortOfLoading = "111";
			AssertHasErrors(order.JD_RL_NKPortOfLoadingInfo);

			order.JD_RL_NKPortOfLoading = "USLAX";
			AssertNoErrors(order.JD_RL_NKPortOfLoadingInfo);
		}

		public void TestJD_RL_NKPortOfDischarge()
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.JD_RL_NKPortOfDischarge = "";
			AssertNoErrors(order.JD_RL_NKPortOfDischargeInfo);

			order.JD_RL_NKPortOfDischarge = "111";
			AssertHasErrors(order.JD_RL_NKPortOfDischargeInfo);

			order.JD_RL_NKPortOfDischarge = "USLAX";
			AssertNoErrors(order.JD_RL_NKPortOfDischargeInfo);
		}
		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			GlbDepartment.CurrentDepartment.GE_Sea = true;
			BO = Factory.NewWithValidTestData<Order>();
			BO.SupplierPK = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			BO.Supplier.OH_RL_NKClosestPort = "AUSYD";
			BO.Buyer.OH_RL_NKClosestPort = "AUMEL";
		}

		protected virtual Order NewOrder()
		{
			OrgHeader buyer = CreateNewOrg(Factory, "TBY");
			Order result = Factory.New<Order>();
			result.BuyerPK = buyer.PK;
			return result;
		}

		OrgHeader CreateNewOrg(BusinessObjectFactory factory, ZString code)
		{
			OrgHeader result = factory.New<OrgHeader>();
			result.OH_FullName = "some full name";
			result.OH_Code = code;
			result.MainAddress.OA_Address1 = "some place";
			return result;
		}

		Order BO;

		#endregion
	}
}
