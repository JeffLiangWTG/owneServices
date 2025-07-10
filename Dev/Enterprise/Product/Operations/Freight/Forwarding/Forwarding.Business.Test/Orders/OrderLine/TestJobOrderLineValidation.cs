using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class TestJobOrderLineValidation : BusinessObjectValidationTestCase
	{
		public void TestValidateJO_QtyInvoiced()
		{
			BO.JO_QtyInvoiced = 10;
			OrderLineDelivery delivery1 = BO.Deliveries.AddNew();
			OrderLineDeliverContainer delivery1Container1 = delivery1.Containers.AddNew();
			OrderLineDeliverContainer delivery1Container2 = delivery1.Containers.AddNew();
			OrderLineDelivery delivery2 = BO.Deliveries.AddNew();
			OrderLineDeliverContainer delivery2Container1 = delivery1.Containers.AddNew();

			delivery1Container1.J5_QuantityInvoiced = 5m;
			delivery1Container2.J5_QuantityInvoiced = 1m;
			delivery2Container1.J5_QuantityInvoiced = 2m;

			BO.Validation.ValidateJO_QtyInvoiced();
			AssertHasWarnings(BO.JO_QtyInvoicedInfo);

			delivery1Container2.J5_QuantityInvoiced = 3m;
			BO.Validation.ValidateJO_QtyInvoiced();
			AssertNoWarnings(BO.JO_QtyInvoicedInfo);
		}

		public void TestValidateJO_QtyReceived()
		{
			BO.JO_QtyInvoiced = 3;
			OrderLineDelivery delivery1 = BO.Deliveries.AddNew();
			OrderLineDelivery delivery2 = BO.Deliveries.AddNew();
			delivery1.J4_Allocated = 1;
			delivery2.J4_Allocated = 2;

			AssertEquals("Total received correct, should be no warning", false, BO.JO_QtyReceivedInfo.HasWarnings());
			delivery2.J4_Allocated = 1;
			AssertEquals("Total received wrong, should be a warning", true, BO.JO_QtyReceivedInfo.HasWarnings());

			BO.Deliveries.DeleteAll();
			BO.Validation.ValidateJO_QtyReceived();
			AssertEquals("No delivery exist any more, there should be no more warning", false, BO.JO_QuantityInfo.HasWarnings());
			BO.Deliveries.AddNew();
			BO.Validation.ValidateJO_QtyReceived();
			AssertEquals("Delivery now exist, there should be a warning", true, BO.JO_QtyReceivedInfo.HasWarnings());
		}

		public void TestJO_OrderLineNoValidation()
		{
			Order order1 = Factory.New<Order>();
			order1.JD_OrderNumber = "Test1";
			OrderLine line1 = order1.OrderLines.AddNew();

			line1.JO_LineNo = 0;
			line1.JO_SubLineNo = 1;
			AssertHasErrors("Should have error", line1.JO_LineNoInfo);
			AssertNoErrors("Should have no errors", line1.JO_SubLineNoInfo);

			line1.JO_LineNo = 1;
			AssertEquals("LineNo should have no errors", false, line1.JO_LineNoInfo.HasErrors());
			AssertEquals("SubLineNo should have no errors", false, line1.JO_SubLineNoInfo.HasErrors());

			OrderLine line2 = order1.OrderLines.AddNew();
			line2.JO_LineNo = 1;
			line2.JO_SubLineNo = 1;
			AssertEquals("Should have error on line no 2", true, line2.JO_LineNoInfo.HasErrors());
			AssertEquals("Should have error on Subline no 2", true, line2.JO_SubLineNoInfo.HasErrors());

			line2.JO_SubLineNo = 2;
			AssertEquals("Should have no errors on line no 2", false, line2.JO_LineNoInfo.HasErrors());
			AssertEquals("Should have no errors on Subline no 2", false, line2.JO_SubLineNoInfo.HasErrors());
		}

		public void TestJO_ContainerNumberValidation()
		{
			Order order1 = Factory.New<Order>();
			order1.JD_OrderNumber = "Test1";
			OrderContainer container1 = order1.PlannedContainers.AddNew();
			OrderContainer container2 = order1.PlannedContainers.AddNew();
			OrderContainer container3 = order1.PlannedContainers.AddNew();
			container1.J1_ContainerNumber = "XYZ";
			container2.J1_ContainerNumber = "DFDF1111114";
			container3.J1_ContainerNumber = "DFDF1111116";

			OrderLine line1 = order1.OrderLines.AddNew();

			line1.JO_ContainerNumber = "XYZ";
			AssertNoErrors(line1.JO_ContainerNumberInfo);

			line1.Deliveries.AddNew();
			line1.JO_ContainerNumber = "";
			AssertNoErrors(line1.JO_ContainerNumberInfo);

			line1.JO_ContainerNumber = "XYZ";
			AssertNoErrors(line1.JO_ContainerNumberInfo);

			line1.Deliveries[0].Containers.AddNew();
			line1.JO_ContainerNumber = "";
			AssertNoErrors(line1.JO_ContainerNumberInfo);

			line1.JO_ContainerNumber = "XYZ";
			AssertHasErrors("Cannot specify container at the order line level IF containers exist on deliveries", line1.JO_ContainerNumberInfo);

			line1.JO_ContainerNumber = "XYZ";
			AssertHasWarnings("Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.", line1.JO_ContainerNumberInfo);

			line1.JO_ContainerNumber = "DFDF1111114";
			AssertHasWarnings("Container number does not have a valid check (last) digit. The check digit should be {0}.", line1.JO_ContainerNumberInfo);

			line1.JO_ContainerNumber = "DFDF1111116";
			AssertNoWarnings(line1.JO_ContainerNumberInfo);

			line1.JO_ContainerNumber = "ZUBIN";
			AssertHasErrors(line1.JO_ContainerNumberInfo);

			order1.JD_JS = Factory.New<ForwardingShipment>().PK;
			ForwardingConsol consol = order1.Shipment.Consols.AddNew();

			line1.JO_ContainerNumber = "ABCD0000000";
			AssertHasWarning(line1.JO_ContainerNumberInfo, "The specified container does not exist on the attached shipment's consols.");

			var consolContainer = consol.Containers.AddNew();
			consolContainer.JC_ContainerNum = "ABCD0000016";
			line1.JO_ContainerNumber = "ABCD0000016";
			AssertNoWarning(line1.JO_ContainerNumberInfo, "The specified container does not exist on the attached shipment's consols.");

			OrderLine orderline = Factory.New<OrderLine>();
			AssertNoExceptionThrown("Has No Order on Orderline, no exception for null Order", () => orderline.Validation.ValidateJO_ContainerNumber());
		}

		public void TestNoNullExceptionCheckJO_ContainerNumber()
		{
			Order order1 = Factory.New<Order>();
			OrderLine line1 = order1.OrderLines.AddNew();
			line1.JO_ContainerNumber = "ASD1231237";
			Order order2;
			AssertNoExceptionThrown(() => order2 = (Order)order1.TemplateCopy());
		}

		public void TestJO_INCOValidation()
		{
			Order order = Factory.New<Order>();
			order.JD_OrderNumber = "Test1";

			OrderLine line = order.OrderLines.AddNew();

			line.JO_INCO = "XYZ";
			AssertHasError(line.JO_INCOInfo, "Enter a valid Incoterm.");

			line.JO_INCO = "FOB";
			AssertNoErrors(line.JO_INCOInfo);
		}

		public void TestJO_InnerPacksValidation()
		{
			var order = Factory.New<Order>();
			var line = order.OrderLines.AddNew();

			line.JO_InnerPacks = 20m;
			AssertNoErrors(line.JO_InnerPacksInfo);

			line.JO_InnerPacks = -20m;
			AssertHasErrors("Should have an error when JO_InnerPacks is negative", line.JO_InnerPacksInfo);
		}

		public void TestJO_OuterPacksValidation()
		{
			var order = Factory.New<Order>();
			var line = order.OrderLines.AddNew();

			line.JO_OuterPacks = 20m;
			AssertNoErrors(line.JO_OuterPacksInfo);

			line.JO_OuterPacks = -20m;
			AssertHasErrors("Should have an error when JO_OuterPacks is negative", line.JO_OuterPacksInfo);
		}

		public void TestJO_QuantityValidation()
		{
			var order = Factory.New<Order>();
			var line = order.OrderLines.AddNew();

			line.JO_Quantity = 20m;
			AssertNoErrors(line.JO_QuantityInfo);

			line.JO_Quantity = -20m;
			AssertHasErrors("Should have an error when JO_Quantity is negative", line.JO_QuantityInfo);
		}

		public void TestJO_ItemPriceValidation()
		{
			var order = Factory.New<Order>();
			var line = order.OrderLines.AddNew();

			line.JO_ItemPrice = 20m;
			AssertNoErrors(line.JO_ItemPriceInfo);

			line.JO_ItemPrice = -20m;
			AssertHasErrors("Should have an error when JO_ItemPrice is negative", line.JO_ItemPriceInfo);
		}

		public void TestJO_ActualWeightValidation()
		{
			var order = Factory.New<Order>();
			var line = order.OrderLines.AddNew();

			line.JO_ActualWeight = 20m;
			AssertNoErrors(line.JO_ActualWeightInfo);

			line.JO_ActualWeight = -20m;
			AssertHasErrors("Should have an error when JO_ActualWeight is negative", line.JO_ActualWeightInfo);
		}

		public void TestJO_ActualVolumeValidation()
		{
			var order = Factory.New<Order>();
			var line = order.OrderLines.AddNew();

			line.JO_ActualVolume = 20m;
			AssertNoErrors(line.JO_ActualVolumeInfo);

			line.JO_ActualVolume = -20m;
			AssertHasErrors("Should have an error when JO_ActualVolume is negative", line.JO_ActualVolumeInfo);
		}

		public void TestJO_ContainerPackingOrderValidation()
		{
			var order = Factory.New<Order>();
			var line = order.OrderLines.AddNew();

			line.JO_ContainerPackingOrder = 20;
			AssertNoErrors(line.JO_ContainerPackingOrderInfo);

			line.JO_ContainerPackingOrder = -20;
			AssertHasErrors("Should have an error when JO_ContainerPackingOrder is negative", line.JO_ContainerPackingOrderInfo);
		}

		public void TestJO_InnerPacksUQValidation()
		{
			var order = Factory.New<Order>();
			var line = order.OrderLines.AddNew();

			line.JO_InnerPacksUQ = "BAG";
			AssertNoErrors(line.JO_InnerPacksUQInfo);

			line.JO_InnerPacksUQ = "XYZ";
			AssertHasErrors("Should have an error when JO_InnerPacksUQ is invalid", line.JO_InnerPacksUQInfo);

			line.JO_InnerPacksUQ = string.Empty;
			AssertNoErrors("Should have no error if empty", line.JO_InnerPacksUQInfo);
		}

		public void TestJO_OuterPacksUQValidation()
		{
			var order = Factory.New<Order>();
			var line = order.OrderLines.AddNew();

			line.JO_OuterPacksUQ = "BAG";
			AssertNoErrors(line.JO_OuterPacksUQInfo);

			line.JO_OuterPacksUQ = "XYZ";
			AssertHasErrors("Should have an error when JO_OuterPacksUQ is invalid", line.JO_OuterPacksUQInfo);

			line.JO_OuterPacksUQ = string.Empty;
			AssertNoErrors("Should have no error if empty", line.JO_OuterPacksUQInfo);
		}

		public void TestJO_F3_NKPackTypeValidation()
		{
			var order = Factory.New<Order>();
			var line = order.OrderLines.AddNew();

			line.JO_F3_NKPackType = "BAG";
			AssertNoErrors(line.JO_F3_NKPackTypeInfo);

			line.JO_F3_NKPackType = "XYZ";
			AssertHasErrors("Should have an error when JO_F3_NKPackType is invalid", line.JO_F3_NKPackTypeInfo);

			line.JO_F3_NKPackType = string.Empty;
			AssertNoErrors("Should have no error if empty", line.JO_F3_NKPackTypeInfo);
		}

		public void TestJO_RN_NKCountryOfOriginValidation()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var line = order.OrderLines.AddNew();

			line.JO_RN_NKCountryOfOrigin = "AU";
			AssertNoErrors(line.JO_F3_NKPackTypeInfo);

			line.JO_RN_NKCountryOfOrigin = "11";
			AssertHasErrors("Should have an error when JO_RN_NKCountryOfOrigin is invalid", line.JO_RN_NKCountryOfOriginInfo);

			line.JO_RN_NKCountryOfOrigin = string.Empty;
			AssertNoErrors("Should have no error if empty", line.JO_RN_NKCountryOfOriginInfo);
		}

		public void TestJO_UnitOfWeightValidation()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var line = order.OrderLines.AddNew();

			line.JO_UnitOfWeight = "KG";
			AssertNoErrors(line.JO_UnitOfWeightInfo);

			line.JO_UnitOfWeight = "11";
			AssertHasErrors("Should have an error when JO_UnitOfWeight is invalid", line.JO_UnitOfWeightInfo);

			line.JO_UnitOfWeight = string.Empty;
			AssertNoErrors("Should have no error if empty", line.JO_UnitOfWeightInfo);
		}

		public void TestJO_UnitOfVolumeValidation()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var line = order.OrderLines.AddNew();

			line.JO_UnitOfVolume = "L";
			AssertNoErrors(line.JO_UnitOfVolumeInfo);

			line.JO_UnitOfVolume = "11";
			AssertHasErrors("Should have an error when JO_UnitOfVolume is invalid", line.JO_UnitOfVolumeInfo);

			line.JO_UnitOfVolume = string.Empty;
			AssertNoErrors("Should have no error if empty", line.JO_UnitOfVolumeInfo);
		}

		public void TestJO_ShipmentWindowEnd()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var line = order.OrderLines.AddNew();

			line.JO_ShipmentWindowStart = new ZDate(2024, 4, 1);
			line.JO_ShipmentWindowEnd = new ZDate(2024, 3, 1);

			AssertHasError(line.JO_ShipmentWindowEndInfo, "Ship window start date must be earlier than or equal to ship window end date.");

			line.JO_ShipmentWindowEnd = new ZDate(2024, 5, 1);
			AssertNoError(line.JO_ShipmentWindowEndInfo, "Ship window start date must be earlier than or equal to ship window end date.");

			line.JO_ShipmentWindowEnd = new ZDate(2024, 4, 1);
			AssertNoError(line.JO_ShipmentWindowEndInfo, "Ship window start date must be earlier than or equal to ship window end date.");
		}

		public void TestJO_ShipmentWindowStart()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var line = order.OrderLines.AddNew();

			line.JO_ShipmentWindowEnd = new ZDate(2024, 3, 1);
			line.JO_ShipmentWindowStart = new ZDate(2024, 4, 1);

			AssertHasError(line.JO_ShipmentWindowStartInfo, "Ship window start date must be earlier than or equal to ship window end date.");

			line.JO_ShipmentWindowStart = new ZDate(2024, 2, 1);
			AssertNoError(line.JO_ShipmentWindowStartInfo, "Ship window start date must be earlier than or equal to ship window end date.");

			line.JO_ShipmentWindowStart = new ZDate(2024, 3, 1);
			AssertNoError(line.JO_ShipmentWindowStartInfo, "Ship window start date must be earlier than or equal to ship window end date.");
		}

		public void TestPartAttributeValidation()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();

			OrgPartRelation relation = part.RelatedOrganisations.AddNew();
			part.OP_PartNum = "BOB";
			relation.OU_OH = org.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			relation.OU_UsePartAttrib1 = true;

			org.MiscServ.OM_IMPartAttrib1Name = "Peanut";
			org.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.Mandatory;
			Factory.Save();
			Order order = Factory.New<Order>();
			order.BuyerPK = org.PK;
			order.SupplierPK = org.PK;
			OrderLine line = order.OrderLines.AddNew();
			line.JO_Partno = "BOB";
			line.RunPreSaveValidation();
			AssertEquals("Should have error on PartAttrib1", true, line.JO_PartAttrib1Info.HasErrors());
		}

		public void TestAllowNonWesternEuropeanCharacters()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var supportedColumns = new string[]
			{
				nameof(JobOrderLineSchema.JO_CustomAttrib1),
				nameof(JobOrderLineSchema.JO_CustomAttrib2),
				nameof(JobOrderLineSchema.JO_CustomAttrib3),
				nameof(JobOrderLineSchema.JO_CustomAttrib4),
				nameof(JobOrderLineSchema.JO_CustomAttrib5),
				nameof(JobOrderLineSchema.JO_CustomAttrib6),
				nameof(JobOrderLineSchema.JO_CustomTextBlob1),
				nameof(JobOrderLineSchema.JO_SpecialInstructions),
				nameof(JobOrderLineSchema.JO_AdditionalInformation),
			};

			var line = order.OrderLines.AddNew();

			foreach (var column in supportedColumns)
			{
				line[column] = "你好";
			}

			AssertNoErrors("There should be no error with non Western-European characters on any of these columns", line);
		}

		public void TestJO_UnderQuantityPercentageLimitCannotBeNegative()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var line = order.OrderLines.AddNew();

			line.JO_UnderQuantityPercentageLimit = -2.718;

			AssertHasError(line.JO_UnderQuantityPercentageLimitInfo,
				"Please enter a valid value for Quantity Allowable Under. Value should be from 0-100.");
		}

		public void TestJO_UnderQuantityPercentageLimitCannotBeGreater100()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var line = order.OrderLines.AddNew();

			line.JO_UnderQuantityPercentageLimit = 210.34;

			AssertHasError(line.JO_UnderQuantityPercentageLimitInfo,
				"Please enter a valid value for Quantity Allowable Under. Value should be from 0-100.");
		}

		public void TestJO_OverQuantityPercentageLimitCannotBeNegative()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var line = order.OrderLines.AddNew();

			line.JO_OverQuantityPercentageLimit = -2.718;

			AssertHasError(line.JO_OverQuantityPercentageLimitInfo, "Please enter a valid value for Quantity Allowable Over. Value should be greater than 0.");
		}

		public void TestJO_LineReference_CannotHaveDuplicateLineReferenceCombination()
		{
			var order = Factory.NewWithValidTestData<Order>();

			var orderLine1 = order.OrderLines.AddNew();
			orderLine1.JO_LineReference = string.Empty;

			AssertNoErrors(orderLine1.JO_LineReferenceInfo);

			Factory.Save();

			var orderLine2 = order.OrderLines.AddNew();
			orderLine2.JO_LineReference = string.Empty;

			AssertNoErrors("Even though they are duplicates, they should not error as Line Reference is empty", orderLine1.JO_LineReferenceInfo);

			orderLine1.JO_LineReference = "ABC";
			orderLine2.JO_LineReference = "ABC";

			AssertHasError(orderLine2.JO_LineReferenceInfo, "A Line with this Line Reference already exists on this Order");
		}

		public void TestJO_LineReference_CheckChars_WhenEnableOrderLineReferenceMatchingIsOff()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var line = order.OrderLines.AddNew();

			using (OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var allowed = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@(){}_#-!%^&*\"'<>=;:+./$?|\\[]";
				foreach (var c in allowed)
				{
					var testCase = $"C{c}1";
					line.JO_LineReference = testCase;
					AssertNoErrors(line.JO_LineReferenceInfo);
				}

				var notAllowed = " ,`~abcdefg";
				var errorMessage = "Must be upper-case and alphanumeric: Space( ) Comma(,) Back-tick(`) and Tilde(~) are not allowed.";
				foreach (var c in notAllowed)
				{
					var testCase = $"C{c}2";
					line.JO_LineReference = testCase;
					AssertHasError(line.JO_LineReferenceInfo, errorMessage);
				}
			}
		}

		public void TestJO_LineReference_CheckChars_WhenEnableOrderLineReferenceMatchingIsOn()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var line = order.OrderLines.AddNew();

			using (OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var allowed = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@(){}_#-!%^&*\"'<>=;:+./$?|\\[]~";
				foreach (var c in allowed)
				{
					var testCase = $"C{c}1";
					line.JO_LineReference = testCase;
					AssertNoErrors(line.JO_LineReferenceInfo);
				}

				var notAllowed = " ,`hijklmn";
				var errorMessage = "Must be upper-case and alphanumeric: Space( ) Comma(,) and Back-tick(`) are not allowed.";
				foreach (var c in notAllowed)
				{
					var testCase = $"C{c}1";
					line.JO_LineReference = testCase;
					AssertHasError(line.JO_LineReferenceInfo, errorMessage);
				}
			}
		}

		#region Implementation

		OrderLine BO;

		protected override void SetUp()
		{
			base.SetUp();
			BO = (OrderLine)GetNewBusinessObject();
		}

		BusinessObject GetNewBusinessObject()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Order order = Factory.New<Order>();

			order.JD_OrderNumber = "x";
			order.BuyerPK = org.PK;
			order.SupplierPK = org.PK;

			OrderLine line = Factory.New<OrderLine>();
			line.JO_JD = order.PK;
			line.JO_LineNo = 99;
			return line;
		}
		#endregion
	}
}
