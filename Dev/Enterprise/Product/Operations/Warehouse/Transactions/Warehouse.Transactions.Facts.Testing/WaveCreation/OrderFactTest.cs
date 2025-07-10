using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Moq;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Core.Facts;

namespace Enterprise.Warehouse.Transactions.Facts.Testing
{
	class OrderFactTest : TestCaseWithFactory
	{
		#region TestOrderFactConstructor_NullArguments_Throws

		public void TestOrderFactConstructor_NullArguments_Throws()
		{
			var order = new Mock<IWhsOrder>();
			order.Setup(o => o.PK).Returns(ZGuid.BrettsGuid);
			order.Setup(o => o.WD_RequiredDate).Returns(ZDateTimeOffset.Today);
			order.Setup(ol => ol.CreateDate).Returns(ZDate.Today);
			var organisation = Mock.Of<IOrganisationFact>();
			var consigneeAddress = Mock.Of<IDocAddressFact>();

			AssertExceptionThrown<ArgumentNullException>(() => new OrderFact(order.Object, null, null, null, null, null, CurrencyConverterForTesting, consigneeAddress, null));
			AssertExceptionThrown<ArgumentNullException>(() => new OrderFact(null, organisation, null, null, null, null, CurrencyConverterForTesting, consigneeAddress, null));
			AssertExceptionThrown<ArgumentNullException>(() => new OrderFact(order.Object, organisation, null, null, null, null, null, consigneeAddress, null));
			AssertExceptionThrown<ArgumentNullException>(() => new OrderFact(order.Object, organisation, null, null, null, null, CurrencyConverterForTesting, null, null));

			AssertNoExceptionThrown(() => new OrderFact(order.Object, organisation, null, null, null, null, CurrencyConverterForTesting, consigneeAddress, null));
		}

		#endregion

		#region TestOrderFactProperties

		public void TestOrderFactProperties()
		{
			var pk = Guid.NewGuid();
			var createdDate = ZDate.Today.AddDays(-1);
			var requiredDate = ZDateTimeOffset.Today;

			var order = new Mock<IWhsOrder>();
			order.Setup(o => o.Factory).Returns(new BusinessObjectFactory());
			order.Setup(o => o.WD_PickPriority).Returns(1);
			order.Setup(o => o.PK).Returns(ZGuid.BrettsGuid.ToGuid());
			order.Setup(o => o.WD_DocketID).Returns("A01");
			order.Setup(o => o.WD_ExternalReference).Returns("B02");
			order.Setup(o => o.WD_DocketSubType).Returns("C03");
			order.Setup(o => o.SalesChannelCode).Returns("D04");
			order.Setup(o => o.WD_CustomerReference).Returns("E05");
			order.Setup(o => o.TransportZoneName).Returns("F06");
			order.Setup(o => o.WD_RS_NKServiceLevel).Returns("G07");
			order.Setup(o => o.WD_PL_NKCarrierServiceLevel).Returns("H08");
			order.Setup(o => o.WD_PackingAfterPickingRequired).Returns(true);
			order.Setup(o => o.WD_IsAuthorisedToLeave).Returns(true);
			order.Setup(o => o.WD_QualityAuditRequired).Returns(true);
			order.Setup(o => o.HasDangerousGoods).Returns(true);
			order.Setup(o => o.WD_RequiredDate).Returns(requiredDate);
			order.Setup(o => o.CreateDate).Returns(createdDate);
			order.Setup(o => o.WD_TotalUnitsFromLines).Returns(123);
			order.Setup(o => o.TotalOrderLines).Returns(20);

			var clientFact = Mock.Of<IOrganisationFact>();
			var consigneeFact = Mock.Of<IOrganisationFact>();
			var docAddressFact = Mock.Of<IDocAddressFact>();

			var orderFact = new OrderFact(order.Object, clientFact, consigneeFact, null, null, "XYZ", CurrencyConverterForTesting, docAddressFact, null);
			orderFact.WaveFactPK = pk;

			// Loop twice to make sure we store these values rather than wrapping the bizo values (bad for performance)
			for (var i = 0; i < 2; i++)
			{
				CombineAssertions(() =>
				{
					AssertEquals(nameof(orderFact.WaveFactPK), pk, orderFact.WaveFactPK);

					AssertEquals(nameof(orderFact.Client), clientFact, orderFact.Client.Fact);
					AssertEquals(nameof(orderFact.Consignee), consigneeFact, orderFact.Consignee.Fact);
					AssertNull(nameof(orderFact.TransportCompany), orderFact.TransportCompany.Fact);
					AssertNull(nameof(orderFact.CarrierBookingAgent), orderFact.CarrierBookingAgent.Fact);

					AssertEquals(nameof(orderFact.PickPriority), 1, orderFact.PickPriority);
					AssertEquals(nameof(orderFact.PK), ZGuid.BrettsGuid.ToGuid(), orderFact.PK);
					AssertEquals(nameof(orderFact.DocketID), "A01", orderFact.DocketID);
					AssertEquals(nameof(orderFact.OrderNumber), "B02", orderFact.OrderNumber);
					AssertEquals(nameof(orderFact.OrderType), "C03", orderFact.OrderType);
					AssertEquals(nameof(orderFact.SalesChannelCode), "D04", orderFact.SalesChannelCode);
					AssertEquals(nameof(orderFact.CustomerReference), "E05", orderFact.CustomerReference);
					AssertEquals(nameof(orderFact.TransportZone), "F06", orderFact.TransportZone);
					AssertEquals(nameof(orderFact.ServiceLevel), "G07", orderFact.ServiceLevel);
					AssertEquals(nameof(orderFact.CarrierServiceLevel), "H08", orderFact.CarrierServiceLevel);
					AssertEquals(nameof(orderFact.ConsigneeDeliveryRoute), "XYZ", orderFact.ConsigneeDeliveryRoute);
					AssertEquals(nameof(orderFact.PackingRequired), true, orderFact.PackingRequired);
					AssertEquals(nameof(orderFact.AuthorisedToLeave), true, orderFact.AuthorisedToLeave);
					AssertEquals(nameof(orderFact.QualityAuditRequired), true, orderFact.QualityAuditRequired);
					AssertEquals(nameof(orderFact.HasDangerousGoods), true, orderFact.HasDangerousGoods);
					AssertEquals(nameof(orderFact.RequiredDate), requiredDate.ToZDateTime(), orderFact.RequiredDate);
					AssertEquals(nameof(orderFact.CreatedDate), createdDate, orderFact.CreatedDate);
					AssertEquals(nameof(orderFact.TotalLineUnits), (decimal)123, orderFact.TotalLineUnits);
					AssertEquals(nameof(orderFact.TotalOrderLines), 20, orderFact.TotalOrderLines);
					AssertEquals(nameof(orderFact.TotalWeight), MeasureFact.Empty, orderFact.TotalWeight);
					AssertEquals(nameof(orderFact.TotalVolume), MeasureFact.Empty, orderFact.TotalVolume);
				});
			}

			order.Verify(o => o.WD_PickPriority, Times.Once);
			order.Verify(o => o.PK, Times.Once);
			order.Verify(o => o.WD_DocketID, Times.Once);
			order.Verify(o => o.WD_ExternalReference, Times.Once);
			order.Verify(o => o.WD_DocketSubType, Times.Once);
			order.Verify(o => o.SalesChannelCode, Times.Once);
			order.Verify(o => o.WD_CustomerReference, Times.Once);
			order.Verify(o => o.TransportZoneName, Times.Once);
			order.Verify(o => o.WD_RS_NKServiceLevel, Times.Once);
			order.Verify(o => o.WD_PL_NKCarrierServiceLevel, Times.Once);
			order.Verify(o => o.WD_PackingAfterPickingRequired, Times.Once);
			order.Verify(o => o.WD_IsAuthorisedToLeave, Times.Once);
			order.Verify(o => o.WD_QualityAuditRequired, Times.Once);
			order.Verify(o => o.HasDangerousGoods, Times.Once);
			order.Verify(o => o.WD_RequiredDate, Times.Once);
			order.Verify(o => o.CreateDate, Times.Once);
			order.Verify(o => o.WD_TotalUnitsFromLines, Times.Once);
			order.Verify(o => o.TotalOrderLines, Times.Once);
			order.Verify(o => o.WD_TotalWeight, Times.Once);
			order.Verify(o => o.WD_TotalWeightUnit, Times.Once);
			order.Verify(o => o.WD_TotalCubic, Times.Once);
			order.Verify(o => o.WD_TotalCubicUnit, Times.Once);
			order.Verify(o => o.WD_TotalOrderValue, Times.Once);
			order.Verify(o => o.WD_RX_NKTotalOrderCurrency, Times.Once);
		}

		#endregion

		#region TestOrderFactOrganisationFact

		public void TestOrderFactOrganisationFact()
		{
			var clientFact = Mock.Of<IOrganisationFact>();
			var consigneeFact = Mock.Of<IOrganisationFact>();
			var transportCompanyFact = Mock.Of<IOrganisationFact>();
			var carrierBookingAgentFact = Mock.Of<IOrganisationFact>();
			var consigneeAddress = Mock.Of<IDocAddressFact>();
			var order = GetMockOrderWithDefaultSetting();

			var orderFact = new OrderFact(
				order.Object,
				clientFact,
				consigneeFact,
				transportCompanyFact,
				carrierBookingAgentFact,
				string.Empty,
				CurrencyConverterForTesting,
				consigneeAddress,
				null);

			AssertEquals(nameof(OrderFact.Client), clientFact, orderFact.Client.Fact);
			AssertEquals(nameof(OrderFact.Consignee), consigneeFact, orderFact.Consignee.Fact);
			AssertEquals(nameof(OrderFact.TransportCompany), transportCompanyFact, orderFact.TransportCompany.Fact);
			AssertEquals(nameof(OrderFact.CarrierBookingAgent), carrierBookingAgentFact, orderFact.CarrierBookingAgent.Fact);
		}

		#endregion

		#region TestOrderFactDocAddressFact

		public void TestOrderFactDocAddressFact()
		{
			var clientFact = Mock.Of<IOrganisationFact>();
			var consigneeFact = Mock.Of<IOrganisationFact>();
			var transportCompanyFact = Mock.Of<IOrganisationFact>();
			var consigneeAddressFact = Mock.Of<IDocAddressFact>();
			var distributionCentreAddressFact = Mock.Of<IDocAddressFact>();
			var order = GetMockOrderWithDefaultSetting();

			var orderFact = new OrderFact(
				order.Object,
				clientFact,
				consigneeFact,
				transportCompanyFact,
				null,
				string.Empty,
				CurrencyConverterForTesting,
				consigneeAddressFact,
				distributionCentreAddressFact);

			AssertEquals(nameof(OrderFact.ConsigneeAddress), consigneeAddressFact, orderFact.ConsigneeAddress.Fact);
			AssertEquals(nameof(OrderFact.DistributionCentreAddress), distributionCentreAddressFact, orderFact.DistributionCentreAddress.Fact);
		}

		#endregion

		#region TestOrderFactWaveFactPK

		public void TestOrderFactWaveFactPK()
		{
			var clientFact = Mock.Of<IOrganisationFact>();
			var order = GetMockOrderWithDefaultSetting();
			var docAddressFact = Mock.Of<IDocAddressFact>();

			var orderFact = new OrderFact(order.Object, clientFact, null, null, null, null, CurrencyConverterForTesting, docAddressFact, null);

			AssertEquals("Precondition " + nameof(OrderFact.WaveFactPK), ZGuid.Empty, orderFact.WaveFactPK);

			var pk1 = Guid.NewGuid();
			orderFact.WaveFactPK = pk1;
			AssertEquals(nameof(OrderFact.WaveFactPK), pk1, orderFact.WaveFactPK);

			var pk2 = Guid.NewGuid();
			orderFact.WaveFactPK = pk2;
			AssertEquals(nameof(OrderFact.WaveFactPK), pk2, orderFact.WaveFactPK);
		}

		#endregion

		#region TestOrderFactTransportCompany

		public void TestOrderFactTransportCompany()
		{
			var order = GetMockOrderWithDefaultSetting();
			var clientFact = Mock.Of<IOrganisationFact>();
			var consigneeFact = Mock.Of<IOrganisationFact>();
			var transportCompanyFact = Mock.Of<IOrganisationFact>();
			var docAddressFact = Mock.Of<IDocAddressFact>();

			var orderFact = new OrderFact(
				order.Object,
				clientFact,
				consigneeFact,
				transportCompany: transportCompanyFact,
				null,
				null,
				CurrencyConverterForTesting,
				docAddressFact,
				null);

			AssertEquals(nameof(orderFact.TransportCompany), transportCompanyFact, orderFact.TransportCompany.Fact);
		}

		#endregion

		#region TestOrderFactCarrierBookingAgent

		public void TestOrderFactCarrierBookingAgent()
		{
			var order = GetMockOrderWithDefaultSetting();
			var clientFact = Mock.Of<IOrganisationFact>();
			var consigneeFact = Mock.Of<IOrganisationFact>();
			var carrierBookingAgentFact = Mock.Of<IOrganisationFact>();
			var consigneeAddress = Mock.Of<IDocAddressFact>();

			var orderFact = new OrderFact(
				order.Object,
				clientFact,
				consigneeFact,
				null,
				carrierBookingAgent: carrierBookingAgentFact,
				null,
				CurrencyConverterForTesting,
				consigneeAddress,
				null);

			AssertEquals(nameof(orderFact.CarrierBookingAgent), carrierBookingAgentFact, orderFact.CarrierBookingAgent.Fact);
		}

		#endregion

		#region TestOrderFactDistributionCentreAddress

		public void TestOrderFactDistributionCentreAddress()
		{
			var order = GetMockOrderWithDefaultSetting();
			var clientFact = Mock.Of<IOrganisationFact>();
			var consigneeFact = Mock.Of<IOrganisationFact>();
			var carrierBookingAgentFact = Mock.Of<IOrganisationFact>();
			var consigneeAddress = Mock.Of<IDocAddressFact>();
			var distributionCentreAddress = Mock.Of<IDocAddressFact>();

			var orderFact = new OrderFact(
				order.Object,
				clientFact,
				consigneeFact,
				null,
				carrierBookingAgent: carrierBookingAgentFact,
				null,
				CurrencyConverterForTesting,
				consigneeAddress,
				distributionCentreAddress);

			AssertEquals(nameof(orderFact.DistributionCentreAddress), distributionCentreAddress, orderFact.DistributionCentreAddress.Fact);
		}

		#endregion

		#region TestOrderFactTotalWeight

		public void TestOrderFactTotalWeight_10KG()
		{
			TestOrderFactTotalWeightCore(10, "KG", new MeasureFact(10, "KG"));
		}

		public void TestOrderFactTotalWeight_2204Pound()
		{
			TestOrderFactTotalWeightCore(22.04, "pound", new MeasureFact(22.04m, "pound"));
		}

		void TestOrderFactTotalWeightCore(ZDecimal totalWeight, ZString totalWeightUnit, MeasureFact expectedResult)
		{
			var order = GetMockOrderWithDefaultSetting();
			var consigneeAddress = Mock.Of<IDocAddressFact>();
			order.Setup(ol => ol.WD_TotalWeight).Returns(totalWeight);
			order.Setup(ol => ol.WD_TotalWeightUnit).Returns(totalWeightUnit);

			var clientFact = Mock.Of<IOrganisationFact>();
			var consigneeFact = Mock.Of<IOrganisationFact>();

			var carrierBookingAgentFact = Mock.Of<IOrganisationFact>();

			var orderFact = new OrderFact(order.Object, clientFact, consigneeFact, null, null, null, CurrencyConverterForTesting, consigneeAddress, null);

			AssertEquals(nameof(orderFact.TotalWeight), expectedResult, orderFact.TotalWeight);
		}

		#endregion

		#region TestOrderFactTotalCubic

		public void TestOrderFactTotalCubic_10liter()
		{
			TestOrderFactTotalCubicCore(10, "liter", new MeasureFact(10, "liter"));
		}

		public void TestOrderFactTotalCubic_5gallon()
		{
			TestOrderFactTotalCubicCore(5, "gallon", new MeasureFact(5, "gallon"));
		}

		void TestOrderFactTotalCubicCore(ZDecimal totalCubic, ZString totalCubicUnit, MeasureFact expectedResult)
		{
			var order = GetMockOrderWithDefaultSetting();
			var consigneeAddress = Mock.Of<IDocAddressFact>();
			order.Setup(ol => ol.WD_TotalCubic).Returns(totalCubic);
			order.Setup(ol => ol.WD_TotalCubicUnit).Returns(totalCubicUnit);

			var clientFact = Mock.Of<IOrganisationFact>();
			var consigneeFact = Mock.Of<IOrganisationFact>();

			var orderFact = new OrderFact(order.Object, clientFact, consigneeFact, null, null, null, CurrencyConverterForTesting, consigneeAddress, null);

			AssertEquals(nameof(orderFact.TotalVolume), expectedResult, orderFact.TotalVolume);
		}

		#endregion

		#region TestOrderFactGetTotalOrderValue

		public void TestOrderFactGetTotalOrderValueASDToUSD()
		{
			TestOrderFactGetTotalOrderValueCore(rateStartDate: ZDateTime.Now.AddYears(-1), sellRate: 1.5, currencyCode: "USD", expectedValue: 15);
		}

		public void TestOrderFactGetTotalOrderValueASDToUSD_NotValidDate()
		{
			TestOrderFactGetTotalOrderValueCore(rateStartDate: ZDateTime.Now.AddYears(+1), sellRate: 1.5, currencyCode: "USD", expectedValue: 0);
		}

		public void TestOrderFactGetTotalOrderValueASDToABC_InvalidCurrency()
		{
			TestOrderFactGetTotalOrderValueCore(rateStartDate: ZDateTime.Now.AddYears(-1), sellRate: 1.5, currencyCode: "ABC", expectedValue: 0);
		}

		void TestOrderFactGetTotalOrderValueCore(ZDateTime rateStartDate, ZDecimal sellRate, ZString currencyCode, ZDecimal expectedValue)
		{
			var order = GetMockOrderWithDefaultSetting();
			var consigneeAddress = Mock.Of<IDocAddressFact>();
			order.Setup(ol => ol.WD_RX_NKTotalOrderCurrency).Returns("AUD");
			order.Setup(ol => ol.WD_TotalOrderValue).Returns(10);

			var clientFact = Mock.Of<IOrganisationFact>();
			var orderFact = new OrderFact(order.Object, clientFact, null, null, null, null, CurrencyConverterForTesting, consigneeAddress, null);

			AssertEquals("No Default Rate.", 0m, orderFact.GetTotalOrderValue(currencyCode));

			SpecifyBuyingRate();

			AssertEquals("Should be able to convert", expectedValue, orderFact.GetTotalOrderValue(currencyCode));

			void SpecifyBuyingRate()
			{
				var rate = OrderFactFactory.New<RefExchangeRate>();
				rate.RE_RX_NKExCurrency = "USD";
				rate.RE_StartDate = rateStartDate;
				rate.RE_ExpiryDate = DateTime.Now.AddYears(2);
				rate.RE_GC = GlbCompany.CurrentCompany.PK;
				rate.RE_SellRate = sellRate;
				rate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
				OrderFactFactory.Save();
			}
		}

		#endregion

		#region TestOrderFactInputFactWithUserDefinedProperties

		public void TestOrderFactInputFactWithUserDefinedProperties()
		{
			var order = GetMockOrderWithDefaultSetting();
			var consigneeAddress = Mock.Of<IDocAddressFact>();
			order.Setup(ol => ol.WD_TotalOrderValue).Returns(10);

			var clientFact = Mock.Of<IOrganisationFact>();
			var orderFact = new OrderFact(order.Object, clientFact, null, null, null, null, CurrencyConverterForTesting, consigneeAddress, null);

			AssertEquals(false, orderFact.HasRuleAlreadySetUserDefinedProperty("Property", "ABC"));

			orderFact.SetUserDefinedProperty("Property", "ABC", 1);
			AssertEquals(true, orderFact.HasRuleAlreadySetUserDefinedProperty("Property", "ABC"));
		}

		#endregion

		#region TestOrderFactPickPriority

		public void TestOrderFactPickPriority_Zero()
		{
			TestOrderFactPickPriorityCore(0, int.MaxValue);
		}

		public void TestOrderFactPickPriority_One()
		{
			TestOrderFactPickPriorityCore(1, 1);
		}

		public void TestOrderFactPickPriority_Max()
		{
			TestOrderFactPickPriorityCore(byte.MaxValue, byte.MaxValue);
		}

		void TestOrderFactPickPriorityCore(ZByte priority, int expectedValue)
		{
			var order = GetMockOrderWithDefaultSetting();
			order.Setup(ol => ol.WD_PickPriority).Returns(priority);

			var clientFact = Mock.Of<IOrganisationFact>();
			var consigneeFact = Mock.Of<IOrganisationFact>();
			var docAddressFact = Mock.Of<IDocAddressFact>();

			var orderFact = new OrderFact(order.Object, clientFact, consigneeFact, null, null, null, CurrencyConverterForTesting, docAddressFact, null);

			AssertEquals(nameof(orderFact.PickPriority), expectedValue, orderFact.PickPriority);
		}

		#endregion

		#region Helper/Setup

		Mock<IWhsOrder> GetMockOrderWithDefaultSetting()
		{
			var order = new Mock<IWhsOrder>();
			order.Setup(o => o.Factory).Returns(OrderFactFactory);
			order.Setup(o => o.PK).Returns(ZGuid.BrettsGuid);
			order.Setup(o => o.WD_RequiredDate).Returns(ZDateTimeOffset.Today);
			order.Setup(ol => ol.CreateDate).Returns(ZDate.Today);
			return order;
		}

		BusinessObjectFactory OrderFactFactory => orderFactFactory ?? (orderFactFactory = new BusinessObjectFactory());
		BusinessObjectFactory orderFactFactory;

		CurrencyConverter CurrencyConverterForTesting => currencyConverterForTesting ?? (currencyConverterForTesting = CurrencyConverter.New(Factory));
		CurrencyConverter currencyConverterForTesting;

		#endregion
	}
}
