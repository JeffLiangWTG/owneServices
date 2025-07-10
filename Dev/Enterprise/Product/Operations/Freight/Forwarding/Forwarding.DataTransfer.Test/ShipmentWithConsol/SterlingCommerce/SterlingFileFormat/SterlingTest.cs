using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(SterlingCommerceConsolAndShipmentExporter))]
	public class SterlingTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SterlingCommerceConsolAndShipmentExporter(Factory, new Xsd.Consol(), new Xsd.Shipment(), new Xsd.InterchangeInfo());
		}

		public void TestExport()
		{
			MemoryStream stream = new MemoryStream();
			SterlingForTest.Export(stream);

			string expectedString =
@"H01|123321SenderCode|321123ReceiverCode|APP>
SHP|12345643|HOUSEBILL|2008-12-31 23:45:59 +11:00|AIR|LSE|AUSYD|Australia|Sydney|USCHI|United States|Chicago|Status|2|BOX|1|CTN|GoodsDescription|123|KG|120|KG|200|D3|1330.2|AUD|STD|CPT|CSH|AgentRef|SHP|MarksAndNums|Owner Reference|Book Ref|InterchangeRegNo|2008-02-12 09:00:00 +11:00|2008-02-14 09:00:00 +11:00|2008-02-13 09:00:00 +11:00|2008-02-15 09:00:00 +11:00|2008-01-12 09:00:00 +11:00|2008-01-14 09:00:00 +11:00|2008-01-13 09:00:00 +11:00|2008-02-15 09:00:00 +11:00|2008-03-15 09:05:00 +11:00|2008-12-30 09:05:00 +11:00|2008-02-01 09:00:00 +11:00>
N01|SF|PickUpCompanyCODE|1|PickUpCompany|PickUpCompanyTest Address11|PickUpCompanyTest Address21|Los Angeles|PickUpCompanyST|PickUpCompany123321|US|MAIN||PickUpCompanyTelephoneNumber|PickUpCompanyemail@emailserver.com|PickUpCompany RegNo>
N01|BT|LocalClientCODE|0|LocalClient|LocalClientTest Address10|LocalClientTest Address20|Brisbane|LocalClientST|LocalClient123321|AU|MAIN|The Accounts Payable Manager|LocalClientTelephoneNumber|LocalClientemail@emailserver.com|LocalClient RegNo>
N01|ST|DeliveryCompanyCODE|1|DeliveryCompany|DeliveryCompanyTest Address11|DeliveryCompanyTest Address21|Sydney|DeliveryCompanyST|DeliveryCompany123321|AU|MAIN||DeliveryCompanyTelephoneNumber|DeliveryCompanyemail@emailserver.com|DeliveryCompany RegNo>
N01|CN|CneeCODE|1|Cnee|CneeTest Address11|CneeTest Address21|Chicago|CneeST|Cnee123321|US|MAIN|ContactName|CneeTelephoneNumber|Cneeemail@emailserver.com|Cnee RegNo>
N01|SH|ConsignorCODE|1|Consignor|ConsignorTest Address11|ConsignorTest Address21|Moskva|ConsignorST|Consignor123321|RU|MAIN|The Export Manager|ConsignorTelephoneNumber|Consignoremail@emailserver.com|Consignor RegNo>
N01|NT|NotifyCODE|1|Notify|NotifyTest Address11|NotifyTest Address21|Singapore|NotifyST|Notify123321|SG|MAIN|All Documents|NotifyTelephoneNumber|Notifyemail@emailserver.com|Notify RegNo>
N01|CR|CarrierCODE|1|Carrier|CarrierTest Address11|CarrierTest Address21|Los Angeles|CarrierST|Carrier123321|US|MAIN|All Documents|CarrierTelephoneNumber|Carrieremail@emailserver.com|Carrier RegNo>
NTS|CustomNote|NoteData1|2008-01-01 02:01:01 +11:00>
NTS|BookingNotes|NoteData2|2008-01-01 03:01:01 +11:00>
ORF|Reference1>
ORF|Reference2>
ORF|Reference3>
EVT|EventSource|EventCode|CodeDesription|2008-01-01 01:01:01 +11:00|2008-03-03 03:03:03 +11:00|EventInformation|EventUser|false>
EVT|EventSource2|EventCode2|CodeDesription2|2008-02-02 02:02:02 +11:00|2008-04-04 04:04:04 +11:00|EventInformation2|EventUser2|true>
CST|CustomAttribute1|CustomAttribute2|2008-02-02 03:03:03 +11:00|2008-03-03 04:04:04 +11:00|1|2|true|false>
RTN|AIR|AUSYD|2008-02-01 20:20:20 +11:00|2008-02-01 20:25:20 +11:00|USLAX|2008-02-02 20:20:20 +11:00|2008-02-02 20:25:20 +11:00|QF123||||S>
INV|InvEDICode|ADJ|2|InvTxnNumber|InvJobInvoiceNo|InvDescription|2008-01-01 00:00:00 +11:00|InvTerm1|InvTermDays1|2008-03-03 00:00:00 +11:00|2008-04-04 00:00:00 +11:00|InvBranch1|Department1|123|AUD|321|AUD|12.1|USD|12.2|USD|13.5|SGD|14|SGD|12|RUB|13|RUB|Y|USERID>
IIT|CST|2|IIChargeCode|IIChargeGroup|IIChargeCodeSalesGroup|IIChargeCodeExpenseGroup|IIDescription|12.1|USD|13.1|USD|1.1|USD|2.1|AUD|2|USD|2.21|SGD|13.1|USD|21.1|AUD|Cartage>
POD|PODName1|DLV>
POD|PODName2|DLV>
POD|PODName3|DLV>
PKG|PackType1|1|101|WEIGHT1|201|LENGTH1|301|WIDTH1|401|HEIGHT1>
PKG|PackType2|2|102|WEIGHT2|202|LENGTH2|302|WIDTH2|402|HEIGHT2>
PKG|PackType3|3|103|WEIGHT3|203|LENGTH3|303|WIDTH3|403|HEIGHT3>
";

			AssertMultilineASCIIEquals("", System.Text.Encoding.UTF8.GetString(stream.ToArray()), expectedString);
		}

		public void TestInitialDirectory()
		{
			AssertEquals("Should be empty", ZString.Empty, SterlingForTest.InitialDirectory);
			SterlingForTest.InitialDirectory = Env.TempPath;
			AssertEquals("Should be as assigned", Env.TempPath, SterlingForTest.InitialDirectory);
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

			#region DeliveryLegs

			Xsd.ContainerLeg leg = shipment.ShipmentDetails.Deliver.DeliveryLegs.AddNew();
			leg.GoodsRecBy = "PODName1";
			leg.LegType = Xsd.ContainerLegType.DLV;
			leg = shipment.ShipmentDetails.Deliver.DeliveryLegs.AddNew();
			leg.GoodsRecBy = "PODName2";
			leg.LegType = Xsd.ContainerLegType.DLV;
			leg = shipment.ShipmentDetails.Deliver.DeliveryLegs.AddNew();
			leg.GoodsRecBy = "PODName3";
			leg.LegType = Xsd.ContainerLegType.DLV;

			#endregion

			#region Package

			CreatePackages(shipment, 3);

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
