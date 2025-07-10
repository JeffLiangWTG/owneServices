using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(SterlingRecord))]
	public class SterlingRecordTest : NonPersistentBusinessObjectTestCase
	{
		public void TestReplaceDelemiterAndTerminaterInRecord()
		{
			SterlingRecord record = new SterlingRecord();
			ZString fld1 = "Some|Field|1";
			ZString fld2 = "Some>Field>2";
			ZString fld3 = "Some|Field>3";
			ZString fld4 = "Special\nInstructions \r\n\r\nwith               lots\r\nof carriage returns and other stuff to replace";
			record.AddField(fld1);
			record.AddField(fld2);
			record.AddField(fld3);
			record.AddField(fld4);
			AssertEquals("RecordHeader|Some Field 1|Some Field 2|Some Field 3|Special Instructions with lots of carriage returns and other stuff to replace>\r\n", record.Record);
		}

		public void TestToTimeFormat()
		{
			SterlingRecord record = new SterlingRecord();
			ZDateTime time = new ZDateTime(2008, 2, 14, 12, 0, 0);
			ZString expectedString = "2008-02-14 12:00:00 +11:00";
			AssertEquals("date time should be in given format", expectedString, record.ToTimeFormat(time));
		}

		public void TestAddField()
		{
			SterlingRecord record = new SterlingRecord();
			ZString field = "SomeInfo";
			record.AddField(field);
			AssertEquals("Should be Header + Delimiter + Record + terminator + next string", "RecordHeader|SomeInfo>\r\n", record.Record);
		}

		public void TestTerminateRecord()
		{
			SterlingRecord record = new SterlingRecord();
			AssertEquals(">\\r\\n As record termitanor", "RecordHeader>\r\n", record.Record);
		}

		#region SetUp

		protected override void SetUp()
		{
			base.SetUp();
			Xsd.InterchangeInfo interChange = new Xsd.InterchangeInfo();
			SetUpInterChange(interChange);
			Xsd.Consol consolToParse = new Xsd.Consol();
			Xsd.Shipment shipmentToParse = consolToParse.Shipments.AddNew();
			SetUpConsol(consolToParse);
			SetUpShipment(shipmentToParse, consolToParse);
			SterlingForTest = new SterlingCommerceConsolAndShipmentExporter(Factory, consolToParse, shipmentToParse, interChange);
		}

		public SterlingCommerceConsolAndShipmentExporter SterlingForTest;

		protected void SetUpInterChange(Xsd.InterchangeInfo interChange)
		{
			interChange.Source.SenderCode = "123321SenderCode";
			interChange.Target.ReceiverCode = "321123ReceiverCode";
			interChange.Source.Purpose = "APP";
			Xsd.RegistrationNumber regNo = interChange.EDIOrganisation.OrganisationDetails.RegistrationNumbers.AddNew();
			regNo.NumberType = Xsd.RegistrationNumberTypes.CCC;
			regNo.CountryOfRegistration = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			regNo.Number = "InterchangeRegNo";
		}

		protected void SetUpConsol(Xsd.Consol consol)
		{
			Xsd.ConsolIdentifier identifier = consol.ConsolIdentifier.AddNew();
			identifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			identifier.Value = "12345643";
		}

		protected void SetUpShipment(Xsd.Shipment shipment, Xsd.Consol consol)
		{
			Xsd.DocAddress docAddress;

			#region ShipmentDetails

			Xsd.ShipmentIdentifier identifier = shipment.ShipmentIdentifier.AddNew();
			identifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			identifier.Value = "HOUSEBILL";
			shipment.ShipmentDetails.DateCreated = new ZDateTime(2008, 12, 31, 23, 45, 59);
			shipment.ShipmentDetails.TransportMode = Enterprise.DataTransfer.Xml.XsdVersion1.TransportMode.AIR;
			shipment.ShipmentDetails.PackingMode = Enterprise.DataTransfer.Xml.XsdVersion1.ContainerMode.LSE;
			shipment.ShipmentDetails.PortOfOrigin.Port = Xsd.UNLOCO.FromPortCode(Factory, "AUSYD");
			shipment.ShipmentDetails.PortofDestination.Port = Xsd.UNLOCO.FromPortCode(Factory, "USCHI");
			shipment.ShipmentDetails.ShipmentStatus = "Status";
			shipment.ShipmentDetails.TotalInnerPacksQty = Xsd.DimensionValue.FromAmountAndUnit((ZInt)2, "BOX");
			shipment.ShipmentDetails.TotalOuterPacksQty = Xsd.DimensionValue.FromAmountAndUnit((ZInt)1, "CTN");
			shipment.ShipmentDetails.GoodsDescription = "GoodsDescription";
			shipment.ShipmentDetails.Weight = Xsd.DimensionValue.FromAmountAndUnit(((ZInt)123), "KG");
			shipment.ShipmentDetails.ChargeableWeight = Xsd.DimensionValue.FromAmountAndUnit((ZInt)120, "KG");
			shipment.ShipmentDetails.ChargeableWeight.DimensionType = "KG";
			shipment.ShipmentDetails.Volume = Xsd.DimensionValue.FromAmountAndUnit((ZInt)200, "D3");
			shipment.ShipmentDetails.GoodsValue = Xsd.FinancialValue.FromAmountAndCurrencyCode((ZDecimal)1330.20, "AUD");
			shipment.ShipmentDetails.ServiceLevel = "STD";
			shipment.ShipmentDetails.Incoterm = "CPT";
			shipment.ShipmentDetails.ReleaseType = Enterprise.DataTransfer.Xml.XsdVersion1.ReleaseType.CSH;
			shipment.ShipmentDetails.AgentReference = "AgentRef";
			shipment.ShipmentDetails.ShippedOnBoardType = Enterprise.DataTransfer.Xml.XsdVersion1.ShippedOnBoardType.SHP;
			shipment.ShipmentDetails.MarksAndNumbers = "MarksAndNums";
			shipment.ShipmentDetails.OwnerReference = "Owner Reference";
			shipment.ShipmentDetails.BookingReference = "Book Ref";

			shipment.ShipmentDetails.Deliver.DeliveryFrom = new ZDateTime(2008, 2, 12, 9, 0, 0);
			shipment.ShipmentDetails.Deliver.DeliveryRequiredBy = new ZDateTime(2008, 2, 14, 9, 0, 0);
			shipment.ShipmentDetails.Deliver.CartageAdvised = new ZDateTime(2008, 2, 13, 9, 0, 0);
			shipment.ShipmentDetails.Deliver.GoodsDelivered = new ZDateTime(2008, 2, 15, 9, 0, 0);
			shipment.ShipmentDetails.Pickup.PickupFrom = new ZDateTime(2008, 1, 12, 9, 0, 0);
			shipment.ShipmentDetails.Pickup.PickupRequiredBy = new ZDateTime(2008, 1, 14, 9, 0, 0);
			shipment.ShipmentDetails.Pickup.CartageAdvised = new ZDateTime(2008, 1, 13, 9, 0, 0);
			shipment.ShipmentDetails.Pickup.GoodsPickup = new ZDateTime(2008, 2, 15, 9, 0, 0);
			shipment.ShipmentDetails.PortOfOrigin.EstimatedDateTime = new ZDateTime(2008, 3, 15, 9, 5, 0);
			shipment.ShipmentDetails.PortofDestination.EstimatedDateTime = new ZDateTime(2008, 12, 30, 9, 5, 0);
			shipment.ShipmentDetails.HBLIssueDate = new ZDateTime(2008, 2, 1, 9, 0, 0);

			#endregion

			#region Names

			#region Delivery

			CreateName(shipment.ShipmentDetails.Deliver.CartageCompany, "DeliveryCompany", "AUSYD");
			docAddress = CreateDocAddress(shipment.ShipmentDetails.DocAddresses.DocAddress.AddNew(), Xsd.DocAddressAddressType.CEG, "DeliveryCompany");
			CreateAddress(docAddress.AddressReference.Organisation.OrganisationDetails.Addresses.AddNew(), "DeliveryCompany", "AUSYD", 1);

			#endregion

			#region LocalClient
			CreateName(shipment.ShipmentDetails.LocalClient, "LocalClient", "AUBNE");
			CreateAddress(shipment.ShipmentDetails.LocalClient.OrganisationDetails.Addresses.GetOrCreateMainAddress(), "LocalClient", "AUBNE", 0);
			#endregion

			#region Pickup

			CreateName(shipment.ShipmentDetails.Pickup.CartageCompany, "PickUpCompany", "USLAX");
			docAddress = CreateDocAddress(shipment.ShipmentDetails.DocAddresses.DocAddress.AddNew(), Xsd.DocAddressAddressType.CRG, "PickUpCompany");
			CreateAddress(docAddress.AddressReference.Organisation.OrganisationDetails.Addresses.AddNew(), "PickUpCompany", "USLAX", 1);

			#endregion

			#region Cnee

			OrgHeader header = Factory.New<OrgHeader>();
			header.OH_Code = "CneeCODE";
			OrgContact contact = header.Contacts.AddNew();
			contact.OC_ContactName = "ContactName";
			contact.OC_Phone = "contactPhone";
			contact.OC_Email = "email@emailserver.com";
			OrgDocument doc = contact.Documents.AddNew();
			doc.OD_DocumentGroup = ContactType.Consignee.Code;
			doc.OD_DefaultContact = ZBool.True;
			header.Factory.Save();
			CreateName(shipment.ShipmentDetails.Consignee, "Cnee", "USCHI");
			docAddress = CreateDocAddress(shipment.ShipmentDetails.DocAddresses.DocAddress.AddNew(), Xsd.DocAddressAddressType.CED, "Cnee");
			CreateAddress(docAddress.AddressReference.Organisation.OrganisationDetails.Addresses.AddNew(), "Cnee", "USCHI", 1);

			#endregion

			#region Cnor

			CreateName(shipment.ShipmentDetails.Consignor, "Consignor", "RUMOW");
			docAddress = CreateDocAddress(shipment.ShipmentDetails.DocAddresses.DocAddress.AddNew(), Xsd.DocAddressAddressType.CRD, "Cnor");
			CreateAddress(docAddress.AddressReference.Organisation.OrganisationDetails.Addresses.AddNew(), "Consignor", "RUMOW", 1);

			#endregion

			#region Notify Party

			CreateName(shipment.ShipmentDetails.NotifyParty.Organisation, "Notify", "SGSIN");
			docAddress = CreateDocAddress(shipment.ShipmentDetails.DocAddresses.DocAddress.AddNew(), Xsd.DocAddressAddressType.NPP, "NPP");
			CreateAddress(docAddress.AddressReference.Organisation.OrganisationDetails.Addresses.AddNew(), "Notify", "SGSIN", 1);

			#endregion

			#region Carrier

			CreateName(consol.ConsolDetail.Carrier, "Carrier", "USLAX");
			CreateAddress(consol.ConsolDetail.Carrier.OrganisationDetails.Addresses.AddNew(), "Carrier", "USLAX", 2);

			#endregion

			#endregion

			#region Notes

			Xsd.NotesNote note = shipment.Notes.AddNew();
			note.NoteType = Enterprise.DataTransfer.Xml.XsdVersion1.NotesNoteNoteType.Custom;
			note.NoteData = "NoteData1";
			note.NoteCreatedDateTime = new ZDateTime(2008, 1, 1, 2, 1, 1);

			note = shipment.Notes.AddNew();
			note.NoteType = Enterprise.DataTransfer.Xml.XsdVersion1.NotesNoteNoteType.BookingNotes;
			note.NoteData = "NoteData2";
			note.NoteCreatedDateTime = new ZDateTime(2008, 1, 1, 3, 1, 1);

			#endregion

			#region Order References

			shipment.ShipmentDetails.OrderReferences = new string[3] { "Reference1", "Reference2", "Reference3" };

			#endregion

			#region EVENT

			Xsd.Event @event = shipment.Events.Event.AddNew();
			@event.Source = "EventSource";
			@event.Code = "EventCode";
			@event.CodeDescription = "CodeDesription";
			@event.DateTime = new ZDateTime(2008, 1, 1, 1, 1, 1);
			@event.PostedDateTime = new ZDateTime(2008, 3, 3, 3, 3, 3);
			@event.Information = "EventInformation";
			@event.User = "EventUser";
			@event.IsEstimatedDate = Enterprise.DataTransfer.Xml.XsdVersion1.TrueFalse.@false;
			@event.TriggeredBy = true;

			@event = shipment.Events.Event.AddNew();
			@event.Source = "EventSource2";
			@event.Code = "EventCode2";
			@event.CodeDescription = "CodeDesription2";
			@event.DateTime = new ZDateTime(2008, 2, 2, 2, 2, 2);
			@event.PostedDateTime = new ZDateTime(2008, 4, 4, 4, 4, 4);
			@event.Information = "EventInformation2";
			@event.User = "EventUser2";
			@event.IsEstimatedDate = Enterprise.DataTransfer.Xml.XsdVersion1.TrueFalse.@true;

			#endregion

			#region Custom

			shipment.ShipmentDetails.Custom.CustomAttribute1 = "CustomAttribute1";
			shipment.ShipmentDetails.Custom.CustomAttribute2 = "CustomAttribute2";
			shipment.ShipmentDetails.Custom.Date1 = new ZDateTime(2008, 2, 2, 3, 3, 3);
			shipment.ShipmentDetails.Custom.Date2 = new ZDateTime(2008, 3, 3, 4, 4, 4);
			shipment.ShipmentDetails.Custom.Decimal1 = 1;
			shipment.ShipmentDetails.Custom.Decimal2 = 2;
			shipment.ShipmentDetails.Custom.Flag1 = Enterprise.DataTransfer.Xml.XsdVersion1.TrueFalse.@true;
			shipment.ShipmentDetails.Custom.Flag2 = Enterprise.DataTransfer.Xml.XsdVersion1.TrueFalse.@false;

			#endregion

			#region Routing

			Xsd.PlannedLeg routing = shipment.ShipmentDetails.TransportPlan.AddNew();

			routing.TransportMode = Enterprise.DataTransfer.Xml.XsdVersion1.TransportMode.AIR;
			routing.PortOfLoading.Port = Xsd.UNLOCO.FromPortCode(Factory, "AUSYD");
			routing.PortOfLoading.EstimatedDateTime = new ZDateTime(2008, 2, 1, 20, 20, 20);
			routing.PortOfLoading.ActualDateTime = new ZDateTime(2008, 2, 1, 20, 25, 20);
			routing.PortOfDischarge.Port = Xsd.UNLOCO.FromPortCode(Factory, "USLAX");
			routing.PortOfDischarge.EstimatedDateTime = new ZDateTime(2008, 2, 2, 20, 20, 20);
			routing.PortOfDischarge.ActualDateTime = new ZDateTime(2008, 2, 2, 20, 25, 20);
			Xsd.FlightWithFlightNumber flight = new Xsd.FlightWithFlightNumber();
			flight.FlightNoJourneyNoTruckRegNo = "QF123";
			routing.Item = flight;

			//Routing = Shipment.ShipmentDetails.TransportPlan.AddNew();

			//Routing.TransportMode = Enterprise.DataTransfer.Xml.XsdVersion1.TransportMode.SEA;
			//Routing.PortOfLoading.Port = Xsd.UNLOCO.FromPortCode(Factory, "USLAX");
			//Routing.PortOfLoading.EstimatedDateTime = new ZDateTime(2008, 2, 2, 21, 25, 20);
			//Routing.PortOfLoading.ActualDateTime = new ZDateTime(2008, 2, 1, 21, 30, 20);
			//Routing.PortOfDischarge.Port = Xsd.UNLOCO.FromPortCode(Factory, "USCHI");
			//Routing.PortOfDischarge.EstimatedDateTime = new ZDateTime(2008, 2, 3, 20, 20, 20);
			//Routing.PortOfDischarge.ActualDateTime = new ZDateTime(2008, 2, 3, 20, 25, 20);
			//Xsd.SailingWithVesselVoyage VesselInfo = new Xsd.SailingWithVesselVoyage();
			//VesselInfo.VesselName = "Black Pearl";
			//VesselInfo.LloydsNo = "LNumber";
			//VesselInfo.VoyageNo = "1";
			//Routing.Item = VesselInfo;

			#endregion

			#region InvoiceHeader

			Xsd.TxnHeader invoice = shipment.ARInvoices.AddNew();
			invoice.DebtorOrCreditor.EDICode = "InvEDICode";
			invoice.TxnType = Enterprise.DataTransfer.Xml.XsdVersion1.TxnType.ADJ;
			invoice.TxnCount = "2";
			invoice.TxnNumber = "InvTxnNumber";
			invoice.JobInvoiceNo = "InvJobInvoiceNo";
			invoice.Description = "InvDescription";
			invoice.InvoiceDate = new ZDateTime(2008, 1, 1);
			invoice.InvTerm = "InvTerm1";
			invoice.InvTermDays = "InvTermDays1";
			invoice.DueDate = new ZDateTime(2008, 3, 3);
			invoice.PostDate = new ZDateTime(2008, 4, 4);
			invoice.Branch = "InvBranch1";
			invoice.Department = "Department1";
			invoice.LocalInvoiceAmtExclTax = Xsd.FinancialValue.FromAmountAndCurrencyCode((ZDecimal)123, "AUD");
			invoice.LocalInvoiceAmtInclTax = Xsd.FinancialValue.FromAmountAndCurrencyCode((ZDecimal)321, "AUD");
			invoice.LocalTaxAmount = Xsd.FinancialValue.FromAmountAndCurrencyCode((ZDecimal)12.1, "USD");
			invoice.LocalWHTAmount = Xsd.FinancialValue.FromAmountAndCurrencyCode((ZDecimal)12.2, "USD");
			invoice.OsInvoiceAmtExclTax = Xsd.FinancialValue.FromAmountAndCurrencyCode((ZDecimal)13.5, "SGD");
			invoice.OsInvoiceAmtInclTax = Xsd.FinancialValue.FromAmountAndCurrencyCode((ZDecimal)14, "SGD");
			invoice.OsTaxAmount = Xsd.FinancialValue.FromAmountAndCurrencyCode((ZDecimal)12, "RUB");
			invoice.OsWHTAmount = Xsd.FinancialValue.FromAmountAndCurrencyCode((ZDecimal)13, "RUB");
			invoice.CashBasisTaxIndicator = Enterprise.DataTransfer.Xml.XsdVersion1.TxnHeaderCashBasisTaxIndicator.Y;
			invoice.CreatedUserId = "USERID";

			#endregion

			#region Invoice Item

			Xsd.TxnLine iItem = invoice.TxnLines.AddNew();
			iItem.LineType = Enterprise.DataTransfer.Xml.XsdVersion1.TxnLineLineType.CST;
			iItem.Sequence = "2";
			iItem.ChargeCode = "IIChargeCode";
			iItem.ChargeGroup = "IIChargeGroup";
			iItem.ChargeCodeSalesGroup = "IIChargeCodeSalesGroup";
			iItem.ChargeCodeExpenseGroup = "IIChargeCodeExpenseGroup";
			iItem.Description = "IIDescription";
			iItem.LocalInvoiceAmtExclTax = Xsd.FinancialValue.FromAmountAndCurrencyCode((ZDecimal)12.1, "USD");
			iItem.LocalInvoiceAmtInclTax = Xsd.FinancialValue.FromAmountAndCurrencyCode((ZDecimal)13.1, "USD");
			iItem.LocalTaxAmount = Xsd.FinancialValue.FromAmountAndCurrencyCode((ZDecimal)1.1, "USD");
			iItem.LocalWHTAmount = Xsd.FinancialValue.FromAmountAndCurrencyCode((ZDecimal)2.1, "AUD");
			iItem.OsInvoiceAmtExclTax = Xsd.FinancialValue.FromAmountAndCurrencyCode((ZDecimal)2, "USD");
			iItem.OsInvoiceAmtInclTax = Xsd.FinancialValue.FromAmountAndCurrencyCode((ZDecimal)2.21, "SGD");
			iItem.OsTaxAmount = Xsd.FinancialValue.FromAmountAndCurrencyCode((ZDecimal)13.1, "USD");
			iItem.OsWHTAmount = Xsd.FinancialValue.FromAmountAndCurrencyCode((ZDecimal)21.1, "AUD");
			iItem.DepartmentActivity = Enterprise.DataTransfer.Xml.XsdVersion1.DepartmentActivity.Cartage;

			#endregion

			#region Package

			CreatePackages(shipment, 3);

			#endregion

			#region DeliveryLegs

			Xsd.ContainerLeg leg = shipment.ShipmentDetails.Deliver.DeliveryLegs.AddNew();
			leg.GoodsRecBy = "PODName1";
			leg.LegType = Enterprise.DataTransfer.Xml.XsdVersion1.ContainerLegType.AEX;
			leg = shipment.ShipmentDetails.Deliver.DeliveryLegs.AddNew();
			leg.GoodsRecBy = "PODName2";
			leg.LegType = Enterprise.DataTransfer.Xml.XsdVersion1.ContainerLegType.AIM;
			leg = shipment.ShipmentDetails.Deliver.DeliveryLegs.AddNew();
			leg.GoodsRecBy = "PODName3";
			leg.LegType = Enterprise.DataTransfer.Xml.XsdVersion1.ContainerLegType.DCD;

			#endregion
		}

		#region Helpers

		void CreatePackages(Xsd.Shipment shipment, int numbersOfPacks)
		{
			for (int i = 1; i <= numbersOfPacks; i++)
			{
				Xsd.Package pack = shipment.ShipmentDetails.Packages.AddNew();
				pack.PackType = "PackType" + i.ToString();
				pack.NumberOfPacks = (uint)i;
				pack.Weight = Xsd.DimensionValue.FromAmountAndUnit((ZDecimal)(i + 100), "WEIGHT" + i.ToString());
				pack.Length = Xsd.DimensionValue.FromAmountAndUnit((ZDecimal)(i + 200), "LENGTH" + i.ToString());
				pack.Width = Xsd.DimensionValue.FromAmountAndUnit((ZDecimal)(i + 300), "WIDTH" + i.ToString());
				pack.Height = Xsd.DimensionValue.FromAmountAndUnit((ZDecimal)(i + 400), "HEIGHT" + i.ToString());
			}
		}

		void CreateName(Xsd.Organisation orgName, ZString name, ZString loco)
		{
			orgName.EDICode = name + "CODE";
			orgName.OrganisationDetails.Name = name + "Full Name";
			orgName.OrganisationDetails.Location = Xsd.UNLOCO.FromPortCode(Factory, loco);
			Xsd.RegistrationNumber regNo = orgName.OrganisationDetails.RegistrationNumbers.AddNew();
			regNo.NumberType = Xsd.RegistrationNumberTypes.CCC;
			regNo.CountryOfRegistration = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			regNo.Number = name + " RegNo";

			regNo = orgName.OrganisationDetails.RegistrationNumbers.AddNew();
			regNo.NumberType = Xsd.RegistrationNumberTypes.CBP;
			regNo.CountryOfRegistration = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			regNo.Number = "RegNoNotInCondition";

			Xsd.OrgAddressCollection addressCollection = new Xsd.OrgAddressCollection();
			orgName.OrganisationDetails.Addresses = addressCollection;
			Xsd.OrgAddress address1 = addressCollection.AddNew(Xsd.AddressCapabilityAddressType.DLV);
			CreateAddress(address1, name, loco, 1);
			Xsd.OrgAddress address2 = addressCollection.AddNew(Xsd.AddressCapabilityAddressType.PIC);
			CreateAddress(address2, name, loco, 2);
		}

		Xsd.DocAddress CreateDocAddress(Xsd.DocAddress docAddress, Xsd.DocAddressAddressType type, ZString name)
		{
			docAddress.AddressType = type;
			docAddress.AddressTypeSpecified = true;
			docAddress.AddressLine1 = name + "DocLine1";
			docAddress.CompanyName = name + "DocName";
			docAddress.CityOrSuburb = name + "DocCity";
			docAddress.StateOrProvince = name + "DocState";
			docAddress.AddressReference.AddressSequenceRef = 1;
			docAddress.AddressReference.Organisation.EDICode = name + "CODE";
			docAddress.AddressReference.Organisation.OrganisationDetails.Name = name;
			return docAddress;
		}

		void CreateAddress(Xsd.OrgAddress orgAddress, ZString name, ZString loco, int addressN)
		{
			orgAddress.CompanyName = name;
			orgAddress.Location = Xsd.UNLOCO.FromPortCode(Factory, loco);
			orgAddress.AddressLine1 = name + "Test Address1" + addressN;
			orgAddress.AddressLine2 = name + "Test Address2" + addressN;
			orgAddress.StateOrProvince = name + "ST";
			orgAddress.CityOrSuburb = orgAddress.Location.City;
			orgAddress.Sequence = addressN;
			orgAddress.PostCode = name + "123321";
			orgAddress.Email = name + "email@emailserver.com";
			Xsd.TelephoneNumber number = orgAddress.TelephoneNumbers.AddNew();
			number.NumberType = Xsd.TelephoneNumberNumberType.Business;
			number.Value = name + "TelephoneNumber";
		}

		#endregion

		#endregion
	}
}
