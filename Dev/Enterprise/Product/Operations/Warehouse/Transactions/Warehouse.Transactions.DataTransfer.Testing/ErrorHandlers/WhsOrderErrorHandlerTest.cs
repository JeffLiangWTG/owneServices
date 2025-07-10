using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Testing
{
	public class TestWhsOrderErrorHandler : WhsDocketErrorHandlerTest<WhsOrder>
	{
		#region Test Cases

		#region Order

		[TestDate(2007, 6, 1)]
		public void TestWD_ShipperCODAmountInvalid()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			Docket.WD_ShipperCODAmount = 200m;
			AssertNotEquals("Precondition", XsdDocket.DocketDetail.ShipperCODAmount, Docket.WD_ShipperCODAmount);

			DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Error (COD Amount): 100");
		}

		[TestDate(2007, 6, 1)]
		public void TestWD_CODPayMethodInvalid()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			Docket.WD_CODPayMethod = "ABC";
			AssertNotEquals("Precondition", XsdDocket.DocketDetail.ShipperCODType, Docket.WD_CODPayMethod);

			DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Error (COD Method): " + Docket.Lookups.ShipperCODPaymentTypes[0].Code);
		}

		[TestDate(2007, 6, 1)]
		public void TestWD_LocalCartInsuranceCostInvalid()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			Docket.WD_LocalCartInsuranceCost = 300m;
			AssertNotEquals("Precondition", XsdDocket.DocketDetail.TransportInsurance, Docket.WD_LocalCartInsuranceCost);

			DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Error (Transport Ins.): 200");
		}

		[TestDate(2007, 6, 1)]
		public void TestWD_PackagesSentInvalid()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			Docket.WD_PackagesSent = 60;
			AssertNotEquals("Precondition", XsdDocket.DocketDetail.Packages, Docket.WD_PackagesSent);

			DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Error (Packages Sent): 30");
		}

		[TestDate(2007, 6, 1)]
		public void TestWD_F3_NKTotalPackTypeInvalid()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			Docket.WD_F3_NKTotalPackType = "SCD";
			AssertNotEquals("Precondition", XsdDocket.DocketDetail.Packages.DimensionType, Docket.WD_F3_NKTotalPackType);

			DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Error (Packages Sent UQ): BCD");
		}

		[TestDate(2007, 6, 1)]
		public void TestWD_PalletsSentInvalid()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			Docket.WD_PalletsSent = 70;
			AssertNotEquals("Precondition", new ZInt(XsdDocket.DocketDetail.Pallets), Docket.WD_PalletsSent);

			DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Error (Pallets Sent): 20");
		}

		[TestDate(2007, 6, 1)]
		public void TestWD_TotalWeightInvalid()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			Docket.CalculateTotalsEnabled = false;
			Docket.WD_TotalWeight = 80m;
			Docket.CalculateTotalsEnabled = true;
			AssertNotEquals("Precondition", XsdDocket.DocketDetail.Weight, Docket.WD_TotalWeight);

			DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Error (Total Weight): 40");
		}

		[TestDate(2007, 6, 1)]
		public void TestWD_TotalWeightUnitInvalid()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			Docket.CalculateTotalsEnabled = false;
			Docket.WD_TotalWeightUnit = "AB";
			Docket.CalculateTotalsEnabled = true;
			AssertNotEquals("Precondition", XsdDocket.DocketDetail.Weight.DimensionType, Docket.WD_TotalWeightUnit);

			DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Error (Total Weight UQ): " + Core.Constants.Weight.Kilograms);
		}

		[TestDate(2007, 6, 1)]
		public void TestWD_TotalCubicInvalid()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			Docket.CalculateTotalsEnabled = false;
			Docket.WD_TotalCubic = 90m;
			Docket.CalculateTotalsEnabled = true;
			AssertNotEquals("Precondition", XsdDocket.DocketDetail.Cubic, Docket.WD_TotalCubic);

			DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Error (Total Cubic): 50");
		}

		[TestDate(2007, 6, 1)]
		public void TestWD_TotalCubicUnitInvalid()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			Docket.CalculateTotalsEnabled = false;
			Docket.WD_TotalCubicUnit = "BC";
			Docket.CalculateTotalsEnabled = true;
			AssertNotEquals("Precondition", XsdDocket.DocketDetail.Cubic.DimensionType, Docket.WD_TotalCubicUnit);

			DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Error (Total Cubic UQ): " + Core.Constants.Volume.CubicMetres);
		}

		[TestDate(2007, 6, 1)]
		public void TestWD_DocketSubTypeInvalid()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			Docket.WD_DocketSubType = OrderType.Codes.Order;
			AssertNotEquals("Precondition", XsdOrderDetail.OrderType, Docket.WD_DocketSubType);

			DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Error (Order Type): " + OrderType.Codes.BackOrder);
		}

		[TestDate(2007, 6, 1)]
		public void TestWD_RequiredDateInvalid2()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			Docket.WD_RequiredDate = ZDateTimeOffset.Now.AddDays(2);
			AssertNotEquals("Precondition", XsdOrderDetail.DateRequired, Docket.WD_RequiredDate.ToZDateTime());

			DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			var dateRequired = XsdOrderDetail.DateRequired;
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Error (Required Date): " + dateRequired.ToShortDateString() + " " + dateRequired.ToShortTimeString());
		}

		#endregion

		#endregion

		#region Implementation

		protected override void PopulateXsdDocketWithValidDataFromDocketCore(Xsd.WhsDocket xsdDocket, WhsOrder docket)
		{
			base.PopulateXsdDocketWithValidDataFromDocketCore(xsdDocket, docket);
			xsdDocket.DocketDetail.Item = new Xsd.WhsCustomerOrderDetail();

			var xsdOrderDetail = (Xsd.WhsCustomerOrderDetail)xsdDocket.DocketDetail.Item;
			xsdOrderDetail.DateRequired = docket.WD_RequiredDate.ToZDateTime();
			xsdOrderDetail.OrderType = docket.WD_DocketSubType;

			xsdDocket.DocketDetail.ShipperCODAmount = docket.WD_ShipperCODAmount;
			xsdDocket.DocketDetail.ShipperCODType = docket.WD_CODPayMethod;
			xsdDocket.DocketDetail.TransportInsurance = docket.WD_LocalCartInsuranceCost;
			xsdDocket.DocketDetail.Units = docket.WD_UnitsSent;
			xsdDocket.DocketDetail.Packages.Value = (ZDecimal)docket.WD_PackagesSent;
			xsdDocket.DocketDetail.Packages.DimensionType = docket.WD_F3_NKTotalPackType;
			xsdDocket.DocketDetail.Pallets = docket.WD_PalletsSent;
			xsdDocket.DocketDetail.Weight.Value = docket.WD_TotalWeight;
			xsdDocket.DocketDetail.Weight.DimensionType = docket.WD_TotalWeightUnit;
			xsdDocket.DocketDetail.Cubic.Value = docket.WD_TotalCubic;
			xsdDocket.DocketDetail.Cubic.DimensionType = docket.WD_TotalCubicUnit;
		}

		protected override void PopulateDocketWithValidDataCore(WhsOrder docket, MasterFiles.Business.OrgHeader client, Environment.Business.WhsWarehouse warehouse)
		{
			base.PopulateDocketWithValidDataCore(docket, client, warehouse);
			docket.WD_RequiredDate = new ZDateTimeOffset(2007, 5, 14);
			docket.WD_DocketSubType = OrderType.Codes.BackOrder;

			var consignee = Helper.CreateClient("CNG", "CONSIGNEE NAME");
			docket.ConsigneePK = consignee.PK;
			docket.ConsigneeAddressPK = consignee.MainAddress.PK;

			var goodsBillTo = Helper.CreateClient("GBT", "GOODS BILL TO NAME");
			docket.GoodsBillToPK = goodsBillTo.PK;
			docket.GoodsBillToAddressPK = goodsBillTo.MainAddress.PK;

			docket.WD_ShipperCODAmount = 100m;
			docket.WD_CODPayMethod = docket.Lookups.ShipperCODPaymentTypes[0].Code;
			docket.WD_LocalCartInsuranceCost = 200m;
			docket.CalculateTotalsEnabled = false;
			docket.WD_UnitsSent = 10;
			docket.WD_PalletsSent = 20;
			docket.WD_PackagesSent = 30;
			docket.WD_F3_NKTotalPackType = "BCD";
			docket.WD_TotalWeight = 40m;
			docket.WD_TotalWeightUnit = Core.Constants.Weight.Kilograms;
			docket.WD_TotalCubic = 50m;
			docket.WD_TotalCubicUnit = Core.Constants.Volume.CubicMetres;
			docket.CalculateTotalsEnabled = true;
		}

		protected Xsd.WhsCustomerOrderDetail XsdOrderDetail
		{
			get
			{
				Assert("Xsd schema does not carry WhsCustomerOrderDetail information", XsdDocket.DocketDetail.Item is Xsd.WhsCustomerOrderDetail);
				return (Xsd.WhsCustomerOrderDetail)XsdDocket.DocketDetail.Item;
			}
		}

		protected override ZString GetDocketType() => DocketType.Codes.Order;

		protected override WhsDocketErrorHandler<WhsOrder> GetNewDocketErrorHandler() => new WhsOrderErrorHandler();

		#endregion
	}
}
