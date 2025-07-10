using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Testing
{
	public class TestWhsReceiveErrorHandler : WhsDocketErrorHandlerTest<WhsReceive>
	{
		#region Overrides

		public void TestLineWE_ClientOrderedUnitsInvalid1()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			var line = Docket.Lines[0];
			var xsdLine = XsdDocket.DocketLines[0];

			line.WE_OP = ZGuid.Empty;
			DocketErrorHandler.SetDocketTypeAndLogDataErrorsFromLine(Docket, line, xsdLine);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Line 1.1 Error (Exp. Qty): No Conversion UM to UNT");
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Line 1.1 Original Unit Qty: 100");
			Docket = GetNewBusinessObject();
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);
			line = Docket.Lines[0];
			var part = Helper.CreateProduct(Docket.Client, "TST1");
			line.WE_OP = part.PK;
			DocketErrorHandler.SetDocketTypeAndLogDataErrorsFromLine(Docket, line, xsdLine);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Line 1.1 Error (Exp. Qty): No Conversion UM to UNT");
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Line 1.1 Original Unit Qty: 100");

			Docket = GetNewBusinessObject();
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);
			line = Docket.Lines[0];
			line.WE_OP = part.PK;
			Helper.CreateProductUnit(part, "UM", "UNT", 10);
			line.WE_ClientOrderedUnits = 20m;
			DocketErrorHandler.SetDocketTypeAndLogDataErrorsFromLine(Docket, line, xsdLine);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Line 1.1 Error (Exp. Qty): 10");

			Docket = GetNewBusinessObject();
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);
			line = Docket.Lines[0];
			Helper.CreateProductUnit(line.SupplierPart, "UM", "UNT", 10);
			line.WE_ClientOrderedUnits = 10m;
			DocketErrorHandler.SetDocketTypeAndLogDataErrorsFromLine(Docket, line, xsdLine);
			AssertNotEquals(DocketStatus.Codes.Error, Docket.WD_DocketStatus);
			AssertEquals(0, GetDocketLogEvents(Docket, Events.DataImport.Description, "Line 1.1 Error (Exp. Qty): 10").Count);
			AssertEquals(0, GetDocketLogEvents(Docket, Events.DataImport.Description, "Line 1.1 Error (Exp. Qty): No Conversion UM to UNT").Count);
			AssertEquals(0, GetDocketLogEvents(Docket, Events.DataImport.Description, "Line 1.1 Original Unit Qty: 100").Count);
		}

		public override void TestLineWE_PackQuantityInvalid1() => Assert("Not required", true);

		public override void TestLineWE_TransactionQuantityInvalid2() => Assert("Not required", true);

		#endregion

		#region Implementation

		public void TestWD_ETAInvalid1()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			XsdInwardsDetail.ETA = ZDate.Empty;
			AssertNotEquals("Precondition", XsdInwardsDetail.ETA, Docket.WD_ETA.ToLocalZDateTime());

			DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Error (ETA): Empty");
		}

		public void TestWD_ETAInvalid2()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			Docket.WD_ETA = ZDateTimeOffset.Now.AddDays(2);
			AssertNotEquals("Precondition", XsdInwardsDetail.ETA, Docket.WD_ETA.ToLocalZDateTime());

			DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Error (ETA): " + XsdInwardsDetail.ETA.ToShortDateString() + " " + XsdInwardsDetail.ETA.ToShortTimeString());
		}

		public void TestWD_ETDInvalid1()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			XsdInwardsDetail.ETD = ZDate.Empty;
			AssertNotEquals("Precondition", XsdInwardsDetail.ETD, Docket.WD_ETD.ToLocalZDateTime());

			DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Error (ETD): Empty");
		}

		public void TestWD_ETDInvalid2()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			Docket.WD_ETD = ZDateTimeOffset.Now.AddDays(2);
			AssertNotEquals("Precondition", XsdInwardsDetail.ETD, Docket.WD_ETD.ToLocalZDateTime());

			DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Error (ETD): " + XsdInwardsDetail.ETD.ToShortDateString() + " " + XsdInwardsDetail.ETD.ToShortTimeString());
		}

		public void TestWD_BookingDateInvalid1()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			XsdInwardsDetail.BookingDate = ZDate.Empty;
			AssertNotEquals("Precondition", XsdInwardsDetail.BookingDate, Docket.WD_BookingDate.ToLocalZDateTime());

			DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Error (Booking Date): Empty");
		}

		public void TestWD_BookingDateInvalid2()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			Docket.WD_BookingDate = ZDateTimeOffset.Now.AddDays(2);
			AssertNotEquals("Precondition", XsdInwardsDetail.BookingDate, Docket.WD_BookingDate.ToLocalZDateTime());

			DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Error (Booking Date): " + XsdInwardsDetail.BookingDate.ToShortDateString() + " " + XsdInwardsDetail.BookingDate.ToShortTimeString());
		}

		public void TestWD_ArrivalDateInvalid1()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			XsdInwardsDetail.ArrivalDate = ZDate.Empty;
			AssertNotEquals("Precondition", XsdInwardsDetail.ArrivalDate, Docket.WD_ArrivalDate);

			DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Error (Arrival Date): Empty");
		}

		public void TestWD_ArrivalDateInvalid2()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			Docket.WD_ArrivalDate = ZDateTimeOffset.Now.AddDays(2);
			AssertNotEquals("Precondition", XsdInwardsDetail.ArrivalDate, Docket.WD_ArrivalDate);

			DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Error (Arrival Date): " + XsdInwardsDetail.ArrivalDate.ToShortDateString() + " " + XsdInwardsDetail.ArrivalDate.ToShortTimeString());
		}

		protected override void PopulateXsdDocketWithValidDataFromDocketCore(Xsd.WhsDocket xsdDocket, WhsReceive docket)
		{
			base.PopulateXsdDocketWithValidDataFromDocketCore(xsdDocket, docket);
			xsdDocket.DocketDetail.Item = new Xsd.WhsCustomerInwardsDetail();

			var xsdInwardsDetail = (Xsd.WhsCustomerInwardsDetail)xsdDocket.DocketDetail.Item;
			xsdInwardsDetail.ArrivalDate = docket.WD_ArrivalDate.ToLocalZDateTime();
			xsdInwardsDetail.BookingDate = docket.WD_BookingDate.ToLocalZDateTime();
			xsdInwardsDetail.ETD = docket.WD_ETD.ToLocalZDateTime();
			xsdInwardsDetail.ETA = docket.WD_ETA.ToLocalZDateTime();
		}

		protected override void PopulateDocketWithValidDataCore(WhsReceive docket, MasterFiles.Business.OrgHeader client, Environment.Business.WhsWarehouse warehouse)
		{
			base.PopulateDocketWithValidDataCore(docket, client, warehouse);

			docket.WD_ArrivalDate = ZDateTimeOffset.Now;
			docket.WD_BookingDate = ZDateTimeOffset.Now.AddDays(-1);
			docket.WD_ETD = ZDateTimeOffset.Now.AddDays(-2);
			docket.WD_ETA = ZDateTimeOffset.Now.AddDays(1);
		}

		protected override ZString GetDocketType() => DocketType.Codes.Receive;

		protected Xsd.WhsCustomerInwardsDetail XsdInwardsDetail
		{
			get
			{
				Assert("Xsd schema does not carry WhsCustomerReceiveDetail information", XsdDocket.DocketDetail.Item is Xsd.WhsCustomerInwardsDetail);
				return (Xsd.WhsCustomerInwardsDetail)XsdDocket.DocketDetail.Item;
			}
		}

		protected override WhsDocketErrorHandler<WhsReceive> GetNewDocketErrorHandler() => new WhsReceiveErrorHandler();

		#endregion
	}
}
