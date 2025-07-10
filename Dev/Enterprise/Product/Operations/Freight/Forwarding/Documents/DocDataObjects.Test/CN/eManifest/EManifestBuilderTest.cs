using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants;
using static Enterprise.Freight.Forwarding.Documents.Testing.AssertionHelper;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Documents.CN.Testing
{
	sealed class EManifestBuilderTest : TestCaseWithFactory
	{
		#region TestPopulateFromForwardingShipment

		[TestDate(2018, 1, 1)]
		public void TestPopulateFromForwardingShipment()
		{
			using (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(),
				Guid.Empty,
				Guid.Empty,
				true))
			{
				var shipment = CreateShipment();
				var parameters = new DummyDocDataObjectParameters();
				var eManifest = new EManifestBuilder(shipment, parameters).Build();

				CombineAssertions(() =>
				{
					AssertEquals("SourceType", "ForwardingShipment", eManifest.SourceType);
					AssertEquals("SourceID", "SH0001000", eManifest.SourceID);
					AssertEquals("DocumentName", "eManifest", eManifest.DocumentName);

					AssertEquals("ShipmentType", Constants.AgentType.Agent, eManifest.ShipmentType.Code);
					AssertEquals("ShipmentType", Constants.AgentTypeDescriptions.Agent,
						eManifest.ShipmentType.Description);
					AssertEquals("NumberOfOriginals", 3, eManifest.NumberOfOriginals);
					AssertEquals("NumberOfCopies", 4, eManifest.NumberOfCopies);
					AssertEquals("RequestedDateOfIssue", new ZDateTime(2018, 10, 2), eManifest.RequestedDateOfIssue);
					AssertEquals("IsDoorPickup", true, eManifest.IsDoorPickup);
					AssertEquals("IsDoorDelivery", false, eManifest.IsDoorDelivery);
					AssertEquals("SpecialInstructions", "consol special instructions", eManifest.SpecialInstructions);
					AssertEquals("SendAllBookings", true, eManifest.SendAllBookings);

					AssertEquals("ReleaseType.Code", "BOL", eManifest.ReleaseType.Code);
					AssertEquals("ReleaseType.Description", "BOL Original", eManifest.ReleaseType.Description);

					AssertEquals("ContainerMode.Code", "FCL", eManifest.ContainerMode.Code);
					AssertEquals("ContainerMode.Description", "Full Container Load",
						eManifest.ContainerMode.Description);

					AssertEquals("PlaceOfReceipt.Code", "CNSHA", eManifest.PlaceOfReceipt.Code);
					AssertEquals("PlaceOfIssue.Code", "DKAAL", eManifest.PlaceOfIssue.Code);
					AssertEquals("PlaceOfDelivery.Code", "SGSIN", eManifest.PlaceOfDelivery.Code);
					AssertEquals("FreightPayableAt.Code", "SGSIN", eManifest.FreightPayableAt.Code);
					AssertEquals("OperationalPort.Code", "CNSHA", eManifest.OperationalPort.Code);

					Assert("IsFreightPrepaid", !eManifest.IsFreightPrepaid);
					Assert("IsFreightCollect", eManifest.IsFreightCollect);

					AssertReferenceNumbers(eManifest);
					AssertTransports(eManifest);
					AssertContainers(eManifest);
					AssertBookings(eManifest);
				});

				AssertConsolAddresses(eManifest, shipment);
				AssertionHelper.AssertCurrentUserAddressData(eManifest.CurrentUser);
			}
		}

		public void TestRequestedDateOfIssueIsOptional()
		{
			using (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(),
				Guid.Empty,
				Guid.Empty,
				true))
			{
				var shipment = CreateShipment();
				shipment.Consols.GetEarliestConsol().JK_MasterBillIssueDate = ZDateTime.Empty;
				var parameters = new DummyDocDataObjectParameters();
				var eManifest = new EManifestBuilder(shipment, parameters).Build();
				var warningMessage = "The Requested Date of Issue is required by some Handling Agents.\r\nIt is advisable to provide this value for faster processing.";

				AssertEquals(false, eManifest.HasErrors);
				AssertHasWarning(eManifest.RequestedDateOfIssueInfo, warningMessage);

				shipment.Consols.GetEarliestConsol().JK_MasterBillIssueDate = new ZDateTime(2018, 10, 2);
				eManifest = new EManifestBuilder(shipment, parameters).Build();
				AssertNoWarning(eManifest.RequestedDateOfIssueInfo, warningMessage);
			}
		}

		public void TestTaxNumberValidation_Defaults()
		{
			var shipment = CreateShipment();
			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();

			CombineAssertions(() =>
			{
				AssertEquals("SendingForwarder tax number", "1234567890", eManifest.SendingAgent.TaxNumber);
				AssertEquals("SendingForwarder tax number type code", "USC", eManifest.SendingAgent.TaxNumberType.Code);
				AssertEquals("SendingForwarder tax number type description", "Unified Social Credit Identifier", eManifest.SendingAgent.TaxNumberType.Description);

				AssertEquals("ReceivingAgent tax number", "410 10 10 10", eManifest.ReceivingAgent.TaxNumber);
				AssertEquals("ReceivingAgent tax number type code", "ABN", eManifest.ReceivingAgent.TaxNumberType.Code);
				AssertEquals("ReceivingAgent tax number type description", "Australian Business Number (GST Registration Code)", eManifest.ReceivingAgent.TaxNumberType.Description);

				AssertEquals("NotifyParty tax number", "123 123 123", eManifest.NotifyParty.TaxNumber);
				AssertEquals("NotifyParty tax number type code", "VAT", eManifest.NotifyParty.TaxNumberType.Code);
				AssertEquals("NotifyParty tax number type description", "VAT Business Registration Number", eManifest.NotifyParty.TaxNumberType.Description);

				AssertEquals("NotifyParty2 tax number", "1111111111", eManifest.NotifyParty2.TaxNumber);
				AssertEquals("NotifyParty2 tax number type code", "USC", eManifest.NotifyParty2.TaxNumberType.Code);
				AssertEquals("NotifyParty2 tax number type description", "Unified Social Credit Identifier", eManifest.NotifyParty2.TaxNumberType.Description);
			});
		}

		public void TestTaxNumberValidation_NingBo()
		{
			TaxNumberTestHelper.AddNewRefDocOrgCusCode(Constants.CountryCodes.China, Constants.CountryCodes.China, OrgCusCode.ChinaCodeTypes.USC, Constants.TaxRelatedDocumentType.ShippingInstruction);
			TaxNumberTestHelper.AddNewRefDocOrgCusCode(Constants.CountryCodes.China, Constants.CountryCodes.Australia, OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, Constants.TaxRelatedDocumentType.ShippingInstruction);
			TaxNumberTestHelper.AddNewRefDocOrgCusCode(Constants.CountryCodes.China, Constants.CountryCodes.UnitedKingdom, OrgCusCode.CodeTypes.VATCode, Constants.TaxRelatedDocumentType.ShippingInstruction);

			var shipment = CreateShipment();
			var consol = shipment.Consols.Cast<ForwardingConsol>().First();
			var sendingAgentPK = consol.JK_OA_SendingForwarderAddress;
			consol.JK_RL_NKLoadPort = "CNNBO";
			consol.JK_OA_SendingForwarderAddress = sendingAgentPK;  //reset sending agent after setting load port

			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();

			eManifest.SendingAgentTaxInfo.Number = string.Empty;
			eManifest.NotifyPartyTaxInfo.Number = string.Empty;
			eManifest.ReceivingAgentTaxInfo.Number = string.Empty;

			eManifest.ValidateAllIncludingChildren();

			CombineAssertions(() =>
			{
				AssertHasMessageError(eManifest.SendingAgentTaxInfo.NumberInfo, "Company ID, USC (Unified Social Credit Identifier) is required to comply with CCAM (China Customs Advanced Manifest) reporting in line with Customs Circular No.56.");
				AssertHasMessageError(eManifest.ReceivingAgentTaxInfo.NumberInfo, "Company ID, ABN (Australian Business Number (GST Registration Code)) is required to comply with CCAM (China Customs Advanced Manifest) reporting in line with Customs Circular No.56.");
				AssertHasMessageError(eManifest.NotifyPartyTaxInfo.NumberInfo, "Company ID, VAT (VAT Business Registration Number) is required to comply with CCAM (China Customs Advanced Manifest) reporting in line with Customs Circular No.56.");
			});

			eManifest.ReceivingAgent.CompanyName = "TO ORDER";

			CombineAssertions(() =>
			{
				AssertHasMessageError(eManifest.SendingAgentTaxInfo.NumberInfo, "Company ID, USC (Unified Social Credit Identifier) is required to comply with CCAM (China Customs Advanced Manifest) reporting in line with Customs Circular No.56.");
				AssertHasMessageError(eManifest.ReceivingAgentTaxInfo.NumberInfo, "Company ID, ABN (Australian Business Number (GST Registration Code)) is required to comply with CCAM (China Customs Advanced Manifest) reporting in line with Customs Circular No.56.");
				AssertHasMessageError(eManifest.NotifyPartyTaxInfo.NumberInfo, "In line with China Customs Circular No.56, the Notify Party’s Company ID number \r\nis required when the Consignee is \"TO ORDER\".");
			});
		}

		public void TestTaxNumberValidation_NoneNingBo()
		{
			TaxNumberTestHelper.AddNewRefDocOrgCusCode(Constants.CountryCodes.China, Constants.CountryCodes.China, OrgCusCode.ChinaCodeTypes.USC, Constants.TaxRelatedDocumentType.ShippingInstruction);
			TaxNumberTestHelper.AddNewRefDocOrgCusCode(Constants.CountryCodes.China, Constants.CountryCodes.Australia, OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, Constants.TaxRelatedDocumentType.ShippingInstruction);
			TaxNumberTestHelper.AddNewRefDocOrgCusCode(Constants.CountryCodes.China, Constants.CountryCodes.UnitedKingdom, OrgCusCode.CodeTypes.VATCode, Constants.TaxRelatedDocumentType.ShippingInstruction);

			var shipment = CreateShipment();
			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();

			eManifest.SendingAgentTaxInfo.Number = string.Empty;
			eManifest.NotifyPartyTaxInfo.Number = string.Empty;
			eManifest.ReceivingAgentTaxInfo.Number = string.Empty;

			eManifest.ValidateAllIncludingChildren();

			CombineAssertions(() =>
			{
				AssertHasWarning(eManifest.SendingAgentTaxInfo.NumberInfo, "Company ID, USC (Unified Social Credit Identifier) is required to comply with CCAM (China Customs Advanced Manifest) reporting in line with Customs Circular No.56.");
				AssertHasWarning(eManifest.ReceivingAgentTaxInfo.NumberInfo, "Company ID, ABN (Australian Business Number (GST Registration Code)) is required to comply with CCAM (China Customs Advanced Manifest) reporting in line with Customs Circular No.56.");
				AssertHasWarning(eManifest.NotifyPartyTaxInfo.NumberInfo, "Company ID, VAT (VAT Business Registration Number) is required to comply with CCAM (China Customs Advanced Manifest) reporting in line with Customs Circular No.56.");
			});

			void AssertValidationsForTaxNumberWhenToOrder()
			{
				CombineAssertions(() =>
				{
					AssertHasWarning(eManifest.SendingAgentTaxInfo.NumberInfo, "Company ID, USC (Unified Social Credit Identifier) is required to comply with CCAM (China Customs Advanced Manifest) reporting in line with Customs Circular No.56.");
					AssertHasWarning(eManifest.ReceivingAgentTaxInfo.NumberInfo, "Company ID, ABN (Australian Business Number (GST Registration Code)) is required to comply with CCAM (China Customs Advanced Manifest) reporting in line with Customs Circular No.56.");
					AssertHasWarning(eManifest.NotifyPartyTaxInfo.NumberInfo, "In line with China Customs Circular No.56, the Notify Party’s Company ID number \r\nis required when the Consignee is \"TO ORDER\".");
				});
			}

			eManifest.ReceivingAgent.CompanyName = "TO ORDER";
			AssertValidationsForTaxNumberWhenToOrder();

			eManifest.ReceivingAgent.CompanyName = "TO ORDER OF";
			AssertValidationsForTaxNumberWhenToOrder();

			eManifest.ReceivingAgent.CompanyName = "TO THE ORDER";
			AssertValidationsForTaxNumberWhenToOrder();

			eManifest.ReceivingAgent.CompanyName = "TO THE ORDER OF";
			AssertValidationsForTaxNumberWhenToOrder();
		}

		public void TestPopulateFromForwardingShipmentWithDirectConsol()
		{
			var shipment = CreateShipment(true);
			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();

			CombineAssertions(() =>
			{
				Assert("IsDirect", eManifest.IsDirect);
				AssertEquals("ShipmentType", Constants.AgentType.Direct, eManifest.ShipmentType.Code);
				AssertEquals("ShipmentType", Constants.AgentTypeDescriptions.Direct, eManifest.ShipmentType.Description);
				AssertEquals("NumberOfOriginals", 3, eManifest.NumberOfOriginals);
				AssertEquals("NumberOfCopies", 3, eManifest.NumberOfCopies);
				AssertEquals("ReleaseType", ShippingInstructionReleaseTypes.Codes.SeaWaybill, eManifest.ReleaseType.Code);
				AssertEquals("ShipperReference", "BKG000001", eManifest.ShipperReference);
			});

			AssertShipmentAddresses(eManifest, shipment);
		}

		#region TestContainerDetailsArePopulatedFromSharedShippingOrderNumber

		public void TestContainerDetailsArePopulatedFromSharedShippingOrderNumber()
		{
			var shipment = CreateEmptyShipmentForEmanifest();

			#region shipment setup

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Direct;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "SGSIN";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CNT0001";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CNT0002";

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 2;
			packline1.JL_F3_NKPackType = "PLT";
			packline1.JL_ActualWeight = 123;
			packline1.JL_ActualWeightUQ = "KG";
			packline1.JL_ActualVolume = 34;
			packline1.JL_ActualVolumeUQ = "M3";
			packline1.JL_HarmonisedCode = "ABCDE";
			packline1.JL_ExportRefNumber = "REF001";
			packline1.JL_DetailedDescription = "pack1";

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_PackageCount = 4;
			packline2.JL_F3_NKPackType = "PLT";
			packline2.JL_ActualWeight = 55;
			packline2.JL_ActualWeightUQ = "KG";
			packline2.JL_ActualVolume = 21;
			packline2.JL_ActualVolumeUQ = "M3";
			packline2.JL_HarmonisedCode = "ABCDE";
			packline2.JL_ExportRefNumber = "REF002";
			packline2.JL_DetailedDescription = "pack2";

			var packline3 = shipment.OuterPackLines.AddNew();
			packline3.JL_PackageCount = 5;
			packline3.JL_F3_NKPackType = "PLT";
			packline3.JL_ActualWeight = 36;
			packline3.JL_ActualWeightUQ = "KG";
			packline3.JL_ActualVolume = 125;
			packline3.JL_ActualVolumeUQ = "M3";
			packline3.JL_HarmonisedCode = "ABCDE";
			packline3.JL_ExportRefNumber = "REF003";
			packline3.JL_DetailedDescription = "pack3";

			var packline4 = shipment.OuterPackLines.AddNew();
			packline4.JL_PackageCount = 1;
			packline4.JL_F3_NKPackType = "PLT";
			packline4.JL_ActualWeight = 34;
			packline4.JL_ActualWeightUQ = "KG";
			packline4.JL_ActualVolume = 51;
			packline4.JL_ActualVolumeUQ = "M3";
			packline4.JL_HarmonisedCode = "ABCDE";
			packline4.JL_ExportRefNumber = "REF001";
			packline4.JL_DetailedDescription = "pack4";

			container1.PackLines.Add(packline1);
			container1.PackLines.Add(packline4);

			container2.PackLines.Add(packline2);
			container2.PackLines.Add(packline3);
			container2.PackLines.Add(packline4);

			#endregion

			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();
			var bookings = eManifest.Bookings;

			AssertEquals("Precondition: Should be 3 bookings", 3, bookings.Count);

			var booking1Containers = bookings.ElementAt(0).Containers;
			AssertEquals("Precondition: First booking has 2 containers", 2, booking1Containers.Count);

			var booking2Containers = bookings.ElementAt(1).Containers;
			AssertEquals("Precondition: Second booking has 1 container", 1, booking2Containers.Count);

			var booking3Containers = bookings.ElementAt(2).Containers;
			AssertEquals("Precondition: Third booking has 1 container", 1, booking3Containers.Count);

			var container1Booking1 = booking1Containers.ElementAt(0);
			AssertEquals(2, container1Booking1.PackCount);
			AssertEquals(123, (int)container1Booking1.GoodsWeight.Value);
			AssertEquals(34, (int)container1Booking1.Volume.Value);

			var container2Booking1 = booking1Containers.ElementAt(1);
			AssertEquals(1, container2Booking1.PackCount);
			AssertEquals(34, (int)container2Booking1.GoodsWeight.Value);
			AssertEquals(51, (int)container2Booking1.Volume.Value);
		}

		#endregion

		#region PickupFrom and DeliverTo Validation tests

		public void TestPickUpFromValidation()
		{
			var shipment = CreateShipment();
			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();
			var pickupFrom = (Address)eManifest.PickupFrom;

			pickupFrom.Contact = "Pizza Hut";
			pickupFrom.Phone = "0800838383";
			eManifest.IsDoorPickup = false;

			AssertNoMessageError(pickupFrom.ContactInfo, "Contact name and Telephone number are mandatory when 'Door Pickup' is selected.");

			pickupFrom.Contact = ZString.Empty;
			pickupFrom.Phone = ZString.Empty;

			AssertNoMessageError(pickupFrom.ContactInfo, "Contact name and Telephone number are mandatory when 'Door Pickup' is selected.");

			eManifest.IsDoorPickup = true;

			AssertHasMessageError(pickupFrom.ContactInfo, "Contact name and Telephone number are mandatory when 'Door Pickup' is selected.");
		}

		public void TestDeliverToValidation()
		{
			var shipment = CreateShipment();
			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();
			var deliverTo = (Address)eManifest.DeliverTo;

			deliverTo.Contact = "Dominos Pizza";
			deliverTo.Phone = "0800304050";
			eManifest.IsDoorDelivery = false;

			AssertNoMessageError(deliverTo.ContactInfo, "Contact name and Telephone number are mandatory when 'Door Delivery' is selected.");

			deliverTo.Contact = ZString.Empty;
			deliverTo.Phone = ZString.Empty;

			AssertNoMessageError(deliverTo.ContactInfo, "Contact name and Telephone number are mandatory when 'Door Delivery' is selected.");

			eManifest.IsDoorDelivery = true;

			AssertHasMessageError(deliverTo.ContactInfo, "Contact name and Telephone number are mandatory when 'Door Delivery' is selected.");
		}

		#endregion

		#region Sending Validation Tests

		public void TestValidationNoContainersOnPacks()
		{
			var shipment = CreateEmptyShipmentForEmanifest();

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Direct;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "SGSIN";

			var packline1 = shipment.OuterPackLines.AddNew();
			var packline2 = shipment.OuterPackLines.AddNew();
			packline1.JL_JC = ZGuid.Empty;
			packline2.JL_JC = ZGuid.Empty;

			var packlines = shipment.OuterPackLines.Cast<PackLine>().ToArray();
			AssertEquals("Precondition: No Containers on Any packs", 0, packlines.Count(pl => !pl.JL_JC.IsEmpty));

			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();
			AssertHasMessageError(eManifest.SendAllBookingsInfo, "Packs need to be allocated to Containers.");
		}

		public void TestValidationAtLeastOneBookingAndPacklinesWithNoContainer()
		{
			var shipment = CreateEmptyShipmentForEmanifest();

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Direct;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "SGSIN";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CNT001";

			var packline1 = shipment.OuterPackLines.AddNew();
			var packline2 = shipment.OuterPackLines.AddNew();

			container.PackLines.Add(packline1);
			container.PackLines.Add(packline2);

			packline1.JL_JC = container.PK;
			packline2.JL_JC = ZGuid.Empty;

			var packlines = shipment.OuterPackLines.Cast<PackLine>().ToArray();

			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();

			AssertEquals("Precondition: At least one booking", true, eManifest.Bookings.Any());
			AssertEquals("Precondition: There are packlines with no container linked", true, packlines.Any(pl => pl.JL_JC.IsDefault));

			AssertHasWarning(eManifest.SendAllBookingsInfo, "There are Packs which do not have a container linked on this Shipment. Only packs shown in this eManifest form (that have a container linked) with a Shipping Order Number will be sent.");
		}

		public void TestValidationOnlyWhenBookingSelected()
		{
			var shipment = CreateEmptyShipmentForEmanifest();

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Direct;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "SGSIN";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CNT001";

			var packline1 = shipment.OuterPackLines.AddNew();
			var packline2 = shipment.OuterPackLines.AddNew();

			container.PackLines.Add(packline1);
			container.PackLines.Add(packline2);

			packline1.JL_ExportRefNumber = "SLD";

			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();
			var booking = eManifest.Bookings.ElementAt(1);

			AssertEquals("Precondition: Booking has no SLD number", ZString.Empty, booking.BookingNumber);

			booking.Send = true;

			AssertHasMessageError(booking.BookingNumberInfo, "Shipping Order Number is required. Enter it on Shipment level (Reference Type SLD) or packs (Shipping Order/Shi Lian Dan Number).");

			booking.Send = false;

			AssertNoMessageError(booking.BookingNumberInfo, "Shipping Order Number is required. Enter it on Shipment level (Reference Type SLD) or packs (Shipping Order/Shi Lian Dan Number).");
		}

		public void TestValidationWhenNoSLDSelected()
		{
			var shipment = CreateEmptyShipmentForEmanifest();

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Direct;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "SGSIN";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CNT0001";

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 2;
			packline.JL_F3_NKPackType = "PLT";
			packline.JL_ActualWeight = 123;
			packline.JL_ActualWeightUQ = "KG";
			packline.JL_ActualVolume = 34;
			packline.JL_ActualVolumeUQ = "M3";
			packline.JL_HarmonisedCode = "ABCDE";
			packline.JL_ExportRefNumber = "REF001";
			packline.JL_DetailedDescription = "pack1";

			container.PackLines.Add(packline);

			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();
			eManifest.Bookings.ForEach(booking => booking.Send = false);

			AssertHasMessageError(eManifest.SendAllBookingsInfo, "You must select at least one SLD to send the message.");

			eManifest.Bookings.ElementAt(0).Send = true;

			AssertNoMessageError(eManifest.SendAllBookingsInfo, "You must select at least one SLD to send the message.");
		}

		#endregion

		public void TestContainerSealNumberValidation()
		{
			var shipment = CreateEmptyShipmentForEmanifest();

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 2;
			packLine1.JL_F3_NKPackType = "PLT";
			packLine1.JL_ActualWeight = 123;
			packLine1.JL_ActualWeightUQ = "KG";
			packLine1.JL_ActualVolume = 34;
			packLine1.JL_ActualVolumeUQ = "M3";
			packLine1.JL_HarmonisedCode = "ABCDE";
			packLine1.JL_ExportRefNumber = "REF001";
			packLine1.JL_DetailedDescription = "pack1";

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 4;
			packLine2.JL_F3_NKPackType = "PLT";
			packLine2.JL_ActualWeight = 55;
			packLine2.JL_ActualWeightUQ = "KG";
			packLine2.JL_ActualVolume = 21;
			packLine2.JL_ActualVolumeUQ = "M3";
			packLine2.JL_HarmonisedCode = "ABCDE";
			packLine2.JL_ExportRefNumber = "REF002";
			packLine2.JL_DetailedDescription = "pack1";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Direct;
			consol.JK_RL_NKLoadPort = "CNNGB";
			consol.JK_RL_NKDischargePort = "SGSIN";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = ZString.Empty;
			container1.JC_SealNum = ZString.Empty;
			container1.PackLines.Add(packLine1);

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "NUMBER2";
			container2.JC_SealNum = "Loose Seal";
			container2.PackLines.Add(packLine2);

			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();
			var eContainer1 = eManifest.Bookings.ElementAt(0).Containers.ElementAt(0);
			var eContainer2 = eManifest.Bookings.ElementAt(1).Containers.ElementAt(0);

			AssertHasMessageError(eContainer1.SealInfo, "In line with China Customs requirements, seal number is required.");
			AssertNoMessageError(eContainer2.SealInfo, "In line with China Customs requirements, seal number is required.");

			AssertHasMessageError(eContainer1.NumberInfo, "In line with China Customs requirements, container number is required.");
			AssertNoMessageError(eContainer2.NumberInfo, "In line with China Customs requirements, container number is required.");

			consol.JK_RL_NKLoadPort = "CNCAN";

			eManifest = new EManifestBuilder(shipment, parameters).Build();
			eContainer1 = eManifest.Bookings.ElementAt(0).Containers.ElementAt(0);
			eContainer2 = eManifest.Bookings.ElementAt(1).Containers.ElementAt(0);

			AssertHasWarning(eContainer1.SealInfo, "In line with China Customs requirements, seal number is required.");
			AssertNoWarning(eContainer2.SealInfo, "In line with China Customs requirements, seal number is required.");

			AssertHasWarning(eContainer1.NumberInfo, "In line with China Customs requirements, container number is required.");
			AssertNoWarning(eContainer2.NumberInfo, "In line with China Customs requirements, container number is required.");
		}

		void AssertReferenceNumbers(IEManifest eManifest)
		{
			AssertEquals("CarrierBookingReference", "驴100", eManifest.CarrierBookingReference);
			AssertEquals("BillOfLadingNumber", "BILLNUMBER", eManifest.BillOfLadingNumber);
			AssertEquals("CarrierContractNumber", "CARCON", eManifest.CarrierContractNumber);
			AssertEquals("QuotationNumber", "QUOT123", eManifest.QuotationNumber);
			AssertEquals("ShipperReference", "AGTREF", eManifest.ShipperReference);
			AssertEquals("FreightForwarderReference", "CON0001", eManifest.FreightForwarderReference);
		}

		void AssertShipmentAddresses(IEManifest eManifest, ForwardingShipment shipment)
		{
			AssertionHelper.AssertAddressData(shipment.ConsignorDocumentaryAddress, eManifest.SendingAgent);
			AssertionHelper.AssertAddressData(shipment.ConsigneeDocumentaryAddress, eManifest.ReceivingAgent);
			AssertionHelper.AssertAddressData(shipment.NotifyPartyDocumentaryAddress, eManifest.NotifyParty);
			AssertionHelper.AssertAddressData(shipment.NotifyParty2DocumentaryAddress, eManifest.NotifyParty2);
			AssertionHelper.AssertAddressData(shipment.ConsignorPickupAddress, eManifest.PickupFrom);
			AssertionHelper.AssertAddressData(shipment.ConsigneeDeliveryAddress, eManifest.DeliverTo);
		}

		void AssertConsolAddresses(IEManifest eManifest, ForwardingShipment shipment)
		{
			var consol = shipment.Consols.Cast<ForwardingConsol>().FirstOrDefault(c => c.JK_RL_NKLoadPort == "CNSHA");

			AssertionHelper.AssertAddressData(consol.SendingForwarderAddress, eManifest.SendingAgent);
			AssertionHelper.AssertAddressData(consol.ReceivingForwarderAddress, eManifest.ReceivingAgent);
			AssertionHelper.AssertAddressData(consol.ShippingLineAddress, eManifest.Carrier);
			AssertionHelper.AssertAddressData(consol.NotifyPartyDocumentaryAddress, eManifest.NotifyParty);
			AssertionHelper.AssertAddressData(consol.NotifyParty2DocumentaryAddress, eManifest.NotifyParty2);
			AssertionHelper.AssertAddressData(consol.CarrierHandlingAgentDocumentaryAddress, eManifest.CarrierHandlingAgent);
			AssertionHelper.AssertAddressData(consol.CarrierBookingAgentDocumentaryAddress, eManifest.CarrierBookingAgent);
			AssertionHelper.AssertAddressData(consol.SendingForwarderAddress, eManifest.Forwarder);
			AssertionHelper.AssertAddressData(GlbBranch.CurrentBranch.OrgProxy.MainAddress, eManifest.CurrentUser);
			AssertionHelper.AssertAddressData(consol.PackDepotAddress, eManifest.PickupFrom);
			AssertionHelper.AssertAddressData(consol.UnpackDepotAddress, eManifest.DeliverTo);
		}

		void AssertTransports(IEManifest eManifest)
		{
			AssertContainsExactElementsInAnyOrder("Transports",
				new[]
				{
					"CNSHA -> SGSIN"
				},
				eManifest.Transports.Select(t => $"{t.PortOfLoading.Code} -> {t.PortOfDischarge.Code}"));

			AssertEquals("Operational Port", "CNSHA", eManifest.OperationalPort.Code);
		}

		void AssertContainers(IEManifest eManifest)
		{
			AssertEquals("Containsers.Count", 1, eManifest.Containers.Count);

			var container1 = eManifest.Containers.Single();
			var container1Formatted = container1.ToAssertString();

			AssertMultilineASCIIEquals("container1",
				@"AAAA0000007|15|WU8CM2QZNK|RFG
   2 PLT|pack1|REF001
      DG SHIPPER NAME
      CN|1234
   5 PLT|pack2|REF001
   3 PLT|pack3|REF002
   5 PLT|pack4|SHPREF001",
				container1Formatted);
		}

		void AssertBookings(IEManifest eManifest)
		{
			AssertEquals("Bookings.Count", 3, eManifest.Bookings.Count);

			var booking1 = eManifest.Bookings.ElementAt(0);
			var booking1Formatted = ToAssertString(booking1);

			AssertMultilineASCIIEquals("booking1",
				@"REF001
   AAAA0000007|7|WU8CM2QZNK|RFG
      2 PLT|pack1|REF001
         DG SHIPPER NAME
         CN|1234
      5 PLT|pack2|REF001",
				booking1Formatted);

			var booking2 = eManifest.Bookings.ElementAt(1);
			var booking2Formatted = ToAssertString(booking2);

			AssertMultilineASCIIEquals("booking2",
				@"REF002
   AAAA0000007|3|WU8CM2QZNK|RFG
      3 PLT|pack3|REF002",
				booking2Formatted);
		}

		const string newLine = "\r\n";

		string ToAssertString(IBooking booking, int indentLevel = 0)
		{
			var indent = new string(' ', indentLevel * 3);

			var res = string.Concat(indent, booking.BookingNumber);

			var containers = booking
				.Containers
				.Select(container => container.ToAssertString(indentLevel + 1))
				.ToArray();

			return containers.Any()
				? string.Concat(res, newLine, string.Join(newLine, containers))
				: res;
		}

		#endregion

		#region TestValidationRulesForCarrierHandlingAgent

		public void TestValidationRulesForCarrierHandlingAgent()
		{
			var shipment = CreateShipment();
			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();
			var carrierHandlingAgent = (Address)eManifest.CarrierHandlingAgent;

			AssertHasMessageError("C1C", carrierHandlingAgent.CompanyNameInfo,
				"C1C Code is missing from Carrier Handling Agent record under Organization > Config > Type C1C.");
			AssertHasWarning("ENP (non Ninbo)", carrierHandlingAgent.CompanyNameInfo,
				"ENP code might be required for eManifest routing via Easipass (entered via Organization > Config > Country CN Type ENP).\r\nWithout this code, eManifest may still be routed to the Carrier Handling Agent directly, or through another service provider (subject to eHub configuration).");
			AssertNoWarning("ENP (Ninbo)", carrierHandlingAgent.CompanyNameInfo,
				"NGB or ENP code might be required for eManifest routing via Ningbo EDI Centre or Easipass (entered via Organization > Config > Country CN Type NGB or ENP).\r\nWithout these codes eManifest may still be routed to the Carrier Handling Agent (subject to eHub configuration).");
			AssertNoWarning("Empty address (non Ninbo)", carrierHandlingAgent.CompanyNameInfo,
				"In the absence of Carrier Handling Agent, the message may be routed to the Carrier (subject to eHub configuration).");

			carrierHandlingAgent.CompanyName = "";

			AssertNoMessageError("C1C", carrierHandlingAgent.CompanyNameInfo,
				"C1C Code is missing from Carrier Handling Agent record under Organization > Config > Type C1C.");
			AssertHasWarning("Empty address (non Ninbo)", carrierHandlingAgent.CompanyNameInfo,
				"In the absence of Carrier Handling Agent, the message may be routed to the Carrier (subject to eHub configuration).");

			eManifest.OperationalPort.Code = "CNNBO";
			carrierHandlingAgent.CompanyName = "Fudge smugglers";

			AssertHasMessageError("C1C", carrierHandlingAgent.CompanyNameInfo,
				"C1C Code is missing from Carrier Handling Agent record under Organization > Config > Type C1C.");
			AssertNoWarning("ENP (non Ninbo)", carrierHandlingAgent.CompanyNameInfo,
				"ENP code might be required for eManifest routing via Easipass (entered via Organization > Config > Country CN Type ENP).\r\nWithout this code, eManifest may still be routed to the Carrier Handling Agent directly, or through another service provider (subject to eHub configuration).");
			AssertHasWarning("ENP (Ninbo)", carrierHandlingAgent.CompanyNameInfo,
				"NGB or ENP code might be required for eManifest routing via Ningbo EDI Centre or Easipass (entered via Organization > Config > Country CN Type NGB or ENP).\r\nWithout these codes eManifest may still be routed to the Carrier Handling Agent (subject to eHub configuration).");
			AssertNoWarning("Empty address (non Ninbo)", carrierHandlingAgent.CompanyNameInfo,
				"In the absence of Carrier Handling Agent, the message may be routed to the Carrier (subject to eHub configuration).");

			carrierHandlingAgent.CompanyName = "";

			AssertNoMessageError("C1C", carrierHandlingAgent.CompanyNameInfo,
				"C1C Code is missing from Carrier Handling Agent record under Organization > Config > Type C1C.");
			AssertHasWarning("Empty address (Ninbo)", carrierHandlingAgent.CompanyNameInfo,
				"In the absence of Carrier Handling Agent, the message may be routed to the Carrier (subject to eHub configuration).");
		}

		#endregion

		#region TestCarrierRegistrationCodeWarnings

		public void TestCarrierRegistrationCodeWarnings_MissingAllNumbers()
		{
			var shipment = CreateShipment();
			var consol = shipment.Consols.Cast<ForwardingConsol>().FirstOrDefault();
			consol.CarrierHandlingAgentDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
			consol.JK_RL_NKLoadPort = "AUSYD";

			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();
			eManifest.CarrierHandlingAgent = AddressBuilder.Create(new CommonContext(Factory), (JobDocAddress)null);

			var carrier = (Address)eManifest.Carrier;

			AssertHasWarning(carrier.CompanyNameInfo, @"Carrier Organization does not have ENP or NGB code types saved, 
which are required to route the message via port systems (Easipass and Ningbo EDI Centre), 
in the absence of the Carrier Handling Agent. The message can still be routed to the carrier either directly or via other service providers.");
		}

		public void TestCarrierRegistrationCodeWarnings_HasAllNumbers()
		{
			var shipment = CreateShipment();
			var consol = shipment.Consols.Cast<ForwardingConsol>().FirstOrDefault();
			consol.CarrierHandlingAgentDocumentaryAddress.E2_OA_Address = ZGuid.Empty;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var ngbCode = carrier.CustomsCodes.AddNew();
			ngbCode.OK_CodeType = "NGB";
			ngbCode.OK_RN_NKCodeCountry = "CN";
			ngbCode.OK_CustomsRegNo = "0001";

			var enpCode = carrier.CustomsCodes.AddNew();
			enpCode.OK_CodeType = "ENP";
			enpCode.OK_RN_NKCodeCountry = "CN";
			enpCode.OK_CustomsRegNo = "0002";

			var cccCode = carrier.CustomsCodes.AddNew();
			cccCode.OK_CodeType = "CCC";
			cccCode.OK_RN_NKCodeCountry = "US";
			cccCode.OK_CustomsRegNo = "00003";

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();

			AssertNoWarnings(((Address)eManifest.Carrier).CompanyNameInfo);
		}

		public void TestCarrierRegistrationCodeWarnings_MissingNGBRegistrationNumber()
		{
			var shipment = CreateShipment();
			var consol = shipment.Consols.Cast<ForwardingConsol>().FirstOrDefault();
			consol.JK_RL_NKLoadPort = "CNNGB";

			var carrier = Factory.NewWithValidTestData<OrgHeader>();

			var enpCode = carrier.CustomsCodes.AddNew();
			enpCode.OK_CodeType = "ENP";
			enpCode.OK_RN_NKCodeCountry = "CN";
			enpCode.OK_CustomsRegNo = "0002";

			var cccCode = carrier.CustomsCodes.AddNew();
			cccCode.OK_CodeType = "CCC";
			cccCode.OK_RN_NKCodeCountry = "US";
			cccCode.OK_CustomsRegNo = "00003";

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var message = @"Carrier Organization does not have NGB code types saved, 
which are required to route the message via port systems (Ningbo EDI Centre).";
			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();

			AssertHasMessageError(((Address)eManifest.Carrier).CompanyNameInfo, message);

			consol.CarrierHandlingAgentDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
			eManifest = new EManifestBuilder(shipment, parameters).Build();

			AssertHasMessageError(((Address)eManifest.Carrier).CompanyNameInfo, message);
		}

		public void TestCarrierRegistrationCodeWarnings_MissingENPRegistrationNumber()
		{
			var shipment = CreateShipment();
			var consol = shipment.Consols.Cast<ForwardingConsol>().FirstOrDefault();
			consol.CarrierHandlingAgentDocumentaryAddress.E2_OA_Address = ZGuid.Empty;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var ngbCode = carrier.CustomsCodes.AddNew();
			ngbCode.OK_CodeType = "NGB";
			ngbCode.OK_RN_NKCodeCountry = "CN";
			ngbCode.OK_CustomsRegNo = "0001";

			var cccCode = carrier.CustomsCodes.AddNew();
			cccCode.OK_CodeType = "CCC";
			cccCode.OK_RN_NKCodeCountry = "US";
			cccCode.OK_CustomsRegNo = "00003";

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();

			AssertHasWarning(((Address)eManifest.Carrier).CompanyNameInfo, @"Carrier Organization does not have ENP code types saved, 
which are required to route the message via port systems (Easipass EDI Centre), 
in the absence of the Carrier Handling Agent. The message can still be routed to the carrier either directly or via other service providers.");
		}

		#endregion

		#region TestPackingLinesValidation

		public void TestPackingLinesValidation_ContactDetailsOnDangerousGoods()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNNBO";
			consol.JK_RL_NKDischargePort = "SGSIN";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA0000007";
			container.JC_DeliveryMode = "CFS/CY";
			container.JC_IsShipperOwned = true;
			container.JC_GrossWeightUQ = "KG";
			container.JC_TareWeight = 1000;
			container.JC_DunnageWeight = 1000;

			var shipment = consol.Shipments.AddNew();
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packline = shipment.OuterPackLines.AddNew();

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "";
			contact.OC_Phone = "";

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "6666";
			subs.DG_Variant = "E";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			var undg = packline.UNDGs.AddNew();
			undg.LinkDefault(subs);
			undg.Substance.DG_PSN = "DG SHIPPER NAME";
			undg.DI_TechnicalName = "WHATEVER";
			undg.DI_IMOClass = "CLAS";
			undg.Substance.DG_PG = "GRO";
			undg.Substance.DG_SubLabel1 = "sub1";
			undg.Substance.DG_SubLabel2 = "su2";
			undg.DI_IsCombustible = true;
			undg.DI_DGFlashPoint = 15.0m;
			undg.DI_MPMarinePollutant = "N";
			undg.DI_DGVolume = 2m;
			undg.DI_UnitOfVolume = "M3";
			undg.DI_DGWeight = 200m;
			undg.DI_UnitOfWeight = "KG";
			undg.DI_IsLimitedQuantity = true;
			undg.DI_PackageCount = 5;
			undg.DI_F3_NKPackType = "BAG";
			undg.DI_OC_DGContact = contact.PK;

			var eManifest = new EManifestBuilder(shipment, new DummyDocDataObjectParameters()).Build();

			var dangerousGood = eManifest
				.Containers
				.Single()
				.PackingLines
				.Single()
				.DangerousGoods
				.Single();

			var dangerousGoodContact = (Contact)dangerousGood.Contact;

			AssertHasMessageError(dangerousGoodContact.FullNameInfo, "Contact Name is required for dangerous goods.");
			AssertHasMessageError(dangerousGoodContact.PhoneInfo, "Contact Phone is required for dangerous goods.");

			dangerousGoodContact.FullName = "Ronald";
			dangerousGoodContact.Phone = "+61411222333";

			AssertNoMessageError(dangerousGoodContact.FullNameInfo, "Contact Name is required for dangerous goods.");
			AssertNoMessageError(dangerousGoodContact.PhoneInfo, "Contact Phone is required for dangerous goods.");
		}

		public void TestPackingLinesValidation_DangerousGoods_Substance()
		{
			var errorMessage = "DG Class, UNDG and Proper Shipping Name are required for dangerous goods.\r\nPlease enter Shipment > Packing > Pack Lines > Dangerous Goods > DG Substance.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNNBO";
			consol.JK_RL_NKDischargePort = "SGSIN";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA0000007";
			container.JC_DeliveryMode = "CFS/CY";
			container.JC_IsShipperOwned = true;
			container.JC_GrossWeightUQ = "KG";
			container.JC_TareWeight = 1000;
			container.JC_DunnageWeight = 1000;

			var shipment = consol.Shipments.AddNew();
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packline = shipment.OuterPackLines.AddNew();

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "";
			contact.OC_Phone = "";

			var undgSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			undgSubstance.DG_Class = "CLAS";
			undgSubstance.DG_PSN = "CLASPSN";

			var undg = packline.UNDGs.AddNew();
			undg.DI_TechnicalName = "WHATEVER";
			undg.DI_IMOClass = "CLAS";
			undg.DI_IsCombustible = true;
			undg.DI_DGFlashPoint = 15.0m;
			undg.DI_MPMarinePollutant = "N";
			undg.DI_DGVolume = 2m;
			undg.DI_UnitOfVolume = "M3";
			undg.DI_DGWeight = 200m;
			undg.DI_UnitOfWeight = "KG";
			undg.DI_IsLimitedQuantity = true;
			undg.DI_PackageCount = 5;
			undg.DI_F3_NKPackType = "BAG";
			undg.DI_OC_DGContact = contact.PK;
			undg.DI_DG = undgSubstance.PK;

			var eManifest = new EManifestBuilder(shipment, new DummyDocDataObjectParameters()).Build();
			DangerousGood GetDangerousGood()
			{
				return (DangerousGood)eManifest.Containers.Single()
					.PackingLines.Single()
					.DangerousGoods.Single();
			}
			var dangerousGood = GetDangerousGood();

			Assert(!dangerousGood.Validator().Any());

			undg.DI_IMOClass = ZString.Empty;

			eManifest = new EManifestBuilder(shipment, new DummyDocDataObjectParameters()).Build();
			dangerousGood = GetDangerousGood();
			Assert(!dangerousGood.Validator().Any());

			undgSubstance.DG_PSN = ZString.Empty;

			eManifest = new EManifestBuilder(shipment, new DummyDocDataObjectParameters()).Build();
			dangerousGood = GetDangerousGood();
			Assert(dangerousGood.Validator().Contains(errorMessage));

			undgSubstance.DG_PSN = "CLASPSN";
			undgSubstance.DG_Class = ZString.Empty;

			eManifest = new EManifestBuilder(shipment, new DummyDocDataObjectParameters()).Build();
			dangerousGood = GetDangerousGood();
			Assert(dangerousGood.Validator().Contains(errorMessage));

			undgSubstance.DG_Class = "CLAS";
			undgSubstance.DG_Code = ZString.Empty;

			eManifest = new EManifestBuilder(shipment, new DummyDocDataObjectParameters()).Build();
			dangerousGood = GetDangerousGood();
			Assert(dangerousGood.Validator().Contains(errorMessage));

			undg.DI_DG = ZGuid.Empty;
			undg.DI_IMOClass = "CLAS";

			eManifest = new EManifestBuilder(shipment, new DummyDocDataObjectParameters()).Build();
			dangerousGood = GetDangerousGood();
			Assert(dangerousGood.Validator().Contains(errorMessage));
		}

		#endregion

		#region TestTransportLegsValidation

		public void TestTransportLegsValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNNBO";
			consol.JK_RL_NKDischargePort = "ARBUE";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA0000007";
			container.JC_DeliveryMode = "CFS/CY";
			container.JC_IsShipperOwned = true;
			container.JC_GrossWeightUQ = "KG";
			container.JC_TareWeight = 1000;
			container.JC_DunnageWeight = 1000;

			var shipment = consol.Shipments.AddNew();
			shipment.OuterPackLines.RemoveAndDeleteAll();

			shipment.OuterPackLines.AddNew();

			var transport1 = (Freight.Business.Transport)consol.Transports.Single();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_LegOrder = 1;
			transport1.JW_RL_NKLoadPort = "CNNBO";
			transport1.JW_RL_NKDiscPort = "CLSCL";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Road;
			transport2.JW_LegOrder = 2;
			transport2.JW_RL_NKLoadPort = "CLSCL";
			transport2.JW_RL_NKDiscPort = "ARCOR";

			var transport3 = consol.Transports.AddNew();
			transport3.JW_TransportMode = Core.Constants.TransportModes.Rail;
			transport3.JW_LegOrder = 3;
			transport3.JW_RL_NKLoadPort = "ARCOR";
			transport3.JW_RL_NKDiscPort = "ARBUE";

			var eManifest = new EManifestBuilder(shipment, new DummyDocDataObjectParameters()).Build();

			var transportDataObjects = eManifest.Transports.ToArray();

			AssertEquals("eManifest has 3 transports", 3, transportDataObjects.Length);

			var transport1DataObject = (DocDataObjects.Transport)transportDataObjects.Single(t => t.LegOrder == 1);
			var transport2DataObject = (DocDataObjects.Transport)transportDataObjects.Single(t => t.LegOrder == 2);
			var transport3DataObject = (DocDataObjects.Transport)transportDataObjects.Single(t => t.LegOrder == 3);

			const string expectedMessageError = "Either Vessel Name or ETD is mandatory.";

			AssertHasMessageError(transport1DataObject.Vessel.NameInfo, expectedMessageError);
			AssertHasMessageError(transport1DataObject.ETDInfo, expectedMessageError);

			AssertNoMessageError(transport2DataObject.Vessel.NameInfo, expectedMessageError);
			AssertNoMessageError(transport2DataObject.ETDInfo, expectedMessageError);

			AssertNoMessageError(transport3DataObject.Vessel.NameInfo, expectedMessageError);
			AssertNoMessageError(transport3DataObject.ETDInfo, expectedMessageError);

			transport1DataObject.Vessel.Name = "Titanic";

			AssertNoMessageError(transport1DataObject.Vessel.NameInfo, expectedMessageError);
			AssertNoMessageError(transport1DataObject.ETDInfo, expectedMessageError);

			transport2DataObject.Vessel.Name = "Flying Dutchman";

			AssertNoMessageError(transport2DataObject.Vessel.NameInfo, expectedMessageError);
			AssertNoMessageError(transport2DataObject.ETDInfo, expectedMessageError);

			transport3DataObject.Vessel.Name = "The Black Pearl";

			AssertNoMessageError(transport3DataObject.Vessel.NameInfo, expectedMessageError);
			AssertNoMessageError(transport3DataObject.ETDInfo, expectedMessageError);

			AssertAsciiCharactersValidation("transport1", transport1DataObject);
			AssertAsciiCharactersValidation("transport2", transport2DataObject);
			AssertAsciiCharactersValidation("transport3", transport3DataObject);
		}

		#endregion

		#region TestReceivingAgentValidation

		public void TestReceivingAgentValidation_ToOrder()
		{
			var shipment = CreateShipment();
			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();
			var receivingAgent = eManifest.ReceivingAgent as Address;

			AssertAddressToOrder("Consignee/Receiving Agent", receivingAgent, false);
		}

		#endregion

		#region TestPopulateShipperCompanyName

		public void TestPopulateShipperCompanyName()
		{
			VerifiedGrossMassTest.InitConsolAndShipmentForShipperCompanyNameTest(Factory, out var consol, out var shipment, out var forwarder, out var consignor);

			var trasport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			trasport.JW_LegOrder = 1;
			trasport.JW_TransportMode = Constants.TransportModes.Sea;
			trasport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			trasport.JW_RL_NKLoadPort = "CNSHA";
			trasport.JW_RL_NKDiscPort = "SGSIN";
			trasport.JW_Vessel = "Sea Dragon";
			trasport.JW_VoyageFlight = "F9999";

			AssertEquals("Precondition", false, shipment.IsDirectShipment);

			var parameters = new DummyDocDataObjectParameters();
			var builder = new EManifestBuilder(shipment, parameters);

			var eManifest = builder.Build();
			AssertEquals($"{forwarder.OH_FullName} {forwarder.MiscServ.Lookups.AsAgentOptions.GetDescriptionFromCode(OrgMiscServLookups.AsAgentOption.AsAgentForCarrier)} AAA Lines", eManifest.SendingAgent.CompanyName);

			consol.JK_AgentType = Constants.AgentType.Direct;
			AssertEquals(true, shipment.IsDirectShipment);

			eManifest = builder.Build();
			AssertEquals($"{consignor.OH_FullName} {consignor.MiscServ.Lookups.AsAgentOptions.GetDescriptionFromCode(OrgMiscServLookups.AsAgentOption.AsCarrier)} BBB Lines", eManifest.SendingAgent.CompanyName);
		}

		#endregion

		#region TestPopulateContainerIsNonOperativeReefer

		public void TestPopulateContainerIsNonOperativeReefer()
		{
			var shipment = CreateEmptyShipmentForEmanifest();

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Direct;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "BRRIO";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CNT0001";
			container1.JC_IsNonOperativeReefer = true;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CNT0002";
			container2.JC_IsNonOperativeReefer = false;

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 2;
			packline1.JL_F3_NKPackType = "PLT";
			packline1.JL_ActualWeight = 123;
			packline1.JL_ActualWeightUQ = "KG";
			packline1.JL_ActualVolume = 34;
			packline1.JL_ActualVolumeUQ = "M3";
			packline1.JL_HarmonisedCode = "ABCDE";
			packline1.JL_ExportRefNumber = "REF001";
			packline1.JL_DetailedDescription = "pack1";

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_PackageCount = 4;
			packline2.JL_F3_NKPackType = "PLT";
			packline2.JL_ActualWeight = 55;
			packline2.JL_ActualWeightUQ = "KG";
			packline2.JL_ActualVolume = 21;
			packline2.JL_ActualVolumeUQ = "M3";
			packline2.JL_HarmonisedCode = "ABCDE";
			packline2.JL_ExportRefNumber = "REF002";
			packline2.JL_DetailedDescription = "pack2";

			container1.PackLines.Add(packline1);
			container2.PackLines.Add(packline2);

			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();

			Assert(eManifest.Containers.First(x => x.Number == "CNT0001").IsNonOperativeReefer);
			Assert(!eManifest.Containers.First(x => x.Number == "CNT0002").IsNonOperativeReefer);
		}

		#endregion

		public void TestReceivingAgentAddressValidation()
		{
			var shipment = CreateShipment();
			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();
			var receivingAgent = eManifest.ReceivingAgent as Address;
			receivingAgent.CompanyName = "test name";
			receivingAgent.AddressLine1 = "";
			receivingAgent.Country.Code = "";

			AssertHasWarning(receivingAgent.CompanyNameInfo, "Consignee/Receiving Agent party name and address information is required.");

			receivingAgent = eManifest.NotifyParty as Address;
			receivingAgent.CompanyName = "";
			receivingAgent.AddressLine1 = "";
			receivingAgent.Country.Code = "";

			AssertNoWarning(receivingAgent.CompanyNameInfo, "Consignee/Receiving Agent party name and address information is required.");
		}

		public void TestAirVentFlowValidation()
		{
			var shipment = CreateShipment();
			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();

			var airVentErrorMessage = "No Air Vent Setting measurement type exists. Ensure each temperature controlled container has one entered on the Containers > Refrigeration tab.";
			var airVentUnitErrorMessage = "Selected Air Vent Setting measurement type is not supported by the carrier. Ensure each temperature controlled container has selected either '2L' or 'MQH' on the Containers > Refrigeration tab.";
			var airVentWarningMessageWhenFlowRateIs0AndUnitIsMQH = "Air Vent is marked as \"Open\".\r\nTo indicate \"Closed\", leave the Consol > Containers > Refrigeration > Air Vent Setting > Unit field blank.";
			var airVentWarningMessageWhenFlowRateIsNonZeroAndUnitIs2L = "Carriers accept airflow only in metric units.\r\nAny value entered as cubic feet per minute (2L) is automatically converted to cubic meters per hour (MQH) during transmission.";

			var bookingContainer = eManifest.Bookings.First().Containers.First();

			shipment.Containers.First().JC_IsControlledAtmosphere = true;
			shipment.Containers.First().JC_AirVentFlow = 0;
			shipment.Containers.First().JC_AirVentFlowRateUnit = "MQH";
			eManifest = new EManifestBuilder(shipment, parameters).Build();
			bookingContainer = eManifest.Bookings.First().Containers.First();
			AssertNoMessageError(((CodeDescription)bookingContainer.AirVentFlow.Unit).CodeInfo, airVentErrorMessage);
			AssertNoMessageError(((CodeDescription)bookingContainer.AirVentFlow.Unit).CodeInfo, airVentUnitErrorMessage);
			AssertHasWarning(((CodeDescription)bookingContainer.AirVentFlow.Unit).CodeInfo, airVentWarningMessageWhenFlowRateIs0AndUnitIsMQH);
			AssertNoWarning(((CodeDescription)bookingContainer.AirVentFlow.Unit).CodeInfo, airVentWarningMessageWhenFlowRateIsNonZeroAndUnitIs2L);

			shipment.Containers.First().JC_AirVentFlowRateUnit = ZString.Empty;
			shipment.Containers.First().JC_AirVentFlow = 1;
			eManifest = new EManifestBuilder(shipment, parameters).Build();
			bookingContainer = eManifest.Bookings.First().Containers.First();
			AssertHasMessageError(((CodeDescription)bookingContainer.AirVentFlow.Unit).CodeInfo, airVentErrorMessage);
			AssertHasMessageError(((CodeDescription)bookingContainer.AirVentFlow.Unit).CodeInfo, airVentUnitErrorMessage);
			AssertNoWarning(((CodeDescription)bookingContainer.AirVentFlow.Unit).CodeInfo, airVentWarningMessageWhenFlowRateIs0AndUnitIsMQH);
			AssertNoWarning(((CodeDescription)bookingContainer.AirVentFlow.Unit).CodeInfo, airVentWarningMessageWhenFlowRateIsNonZeroAndUnitIs2L);

			shipment.Containers.First().JC_AirVentFlowRateUnit = "ABC";
			shipment.Containers.First().JC_AirVentFlow = 1;
			eManifest = new EManifestBuilder(shipment, parameters).Build();
			bookingContainer = eManifest.Bookings.First().Containers.First();
			AssertNoMessageError(((CodeDescription)bookingContainer.AirVentFlow.Unit).CodeInfo, airVentErrorMessage);
			AssertHasMessageError(((CodeDescription)bookingContainer.AirVentFlow.Unit).CodeInfo, airVentUnitErrorMessage);
			AssertNoWarning(((CodeDescription)bookingContainer.AirVentFlow.Unit).CodeInfo, airVentWarningMessageWhenFlowRateIs0AndUnitIsMQH);
			AssertNoWarning(((CodeDescription)bookingContainer.AirVentFlow.Unit).CodeInfo, airVentWarningMessageWhenFlowRateIsNonZeroAndUnitIs2L);

			shipment.Containers.First().JC_IsControlledAtmosphere = false;
			eManifest = new EManifestBuilder(shipment, parameters).Build();
			bookingContainer = eManifest.Bookings.First().Containers.First();
			AssertNoMessageError(((CodeDescription)bookingContainer.AirVentFlow.Unit).CodeInfo, airVentErrorMessage);
			AssertNoMessageError(((CodeDescription)bookingContainer.AirVentFlow.Unit).CodeInfo, airVentUnitErrorMessage);
			AssertNoWarning(((CodeDescription)bookingContainer.AirVentFlow.Unit).CodeInfo, airVentWarningMessageWhenFlowRateIs0AndUnitIsMQH);
			AssertNoWarning(((CodeDescription)bookingContainer.AirVentFlow.Unit).CodeInfo, airVentWarningMessageWhenFlowRateIsNonZeroAndUnitIs2L);

			shipment.Containers.First().JC_AirVentFlowRateUnit = "2L";
			shipment.Containers.First().JC_AirVentFlow = 1;
			eManifest = new EManifestBuilder(shipment, parameters).Build();
			bookingContainer = eManifest.Bookings.First().Containers.First();

			AssertHasWarning(((CodeDescription)bookingContainer.AirVentFlow.Unit).CodeInfo, airVentWarningMessageWhenFlowRateIsNonZeroAndUnitIs2L);
			AssertNoWarning(((CodeDescription)bookingContainer.AirVentFlow.Unit).CodeInfo, airVentWarningMessageWhenFlowRateIs0AndUnitIsMQH);

			foreach (var container in shipment.Containers)
			{
				bookingContainer.HasControlledAtmosphere = true;
				container.JC_IsNonOperativeReefer = true;
			}
			eManifest = new EManifestBuilder(shipment, parameters).Build();
			bookingContainer = eManifest.Bookings.First().Containers.First();
			AssertNoMessageError(((CodeDescription)bookingContainer.AirVentFlow.Unit).CodeInfo, airVentErrorMessage);
			AssertNoMessageError(((CodeDescription)bookingContainer.AirVentFlow.Unit).CodeInfo, airVentUnitErrorMessage);
		}

		public void TestSetTemperatureValidation()
		{
			var shipment = CreateShipment();
			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();

			var bookingContainer = eManifest.Bookings.First().Containers.First();
			bookingContainer.SetTemperature.Value = 0;
			bookingContainer.SetTemperature.Unit.Code = "2L";
			AssertNoMessageError(((CodeDescription)bookingContainer.SetTemperature.Unit).CodeInfo, "Temperature is required when container is a reefer.");

			bookingContainer.SetTemperature.Unit.Code = "";
			AssertHasMessageError(((CodeDescription)bookingContainer.SetTemperature.Unit).CodeInfo, "Temperature is required when container is a reefer.");

			foreach (var container in shipment.Containers)
			{
				container.JC_IsNonOperativeReefer = true;
			}
			eManifest = new EManifestBuilder(shipment, parameters).Build();
			bookingContainer = eManifest.Bookings.First().Containers.First();
			AssertNoMessageError(((CodeDescription)bookingContainer.SetTemperature.Unit).CodeInfo, "Temperature is required when container is a reefer.");
		}

		public void TestAddAsciiCharactersValidation()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "SGSIN";
			var container = consol.Containers.AddNew();
			shipment.OuterPackLines.RemoveAndDeleteAll();
			var packline = shipment.OuterPackLines.AddNew();
			container.PackLines.Add(packline);

			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();

			CombineAssertions(() =>
			{
				AssertAsciiCharactersValidation("SendingAgent", (Address)eManifest.SendingAgent);
				AssertAsciiCharactersValidation("Carrier", (Address)eManifest.Carrier);
				AssertAsciiCharactersValidation("ReceivingAgent", (Address)eManifest.ReceivingAgent);
				AssertAsciiCharactersValidation("CarrierHandlingAgent", (Address)eManifest.CarrierHandlingAgent);
				AssertAsciiCharactersValidation("CarrierBookingAgent", (Address)eManifest.CarrierBookingAgent);
				AssertAsciiCharactersValidation("NotifyParty", (Address)eManifest.NotifyParty);
				AssertAsciiCharactersValidation("NotifyParty2", (Address)eManifest.NotifyParty2);
				AssertAsciiCharactersValidation("Forwarder", (Address)eManifest.Forwarder);
				AssertAsciiCharactersValidation("PickupFrom", (Address)eManifest.PickupFrom);
				AssertAsciiCharactersValidation("DeliverTo", (Address)eManifest.DeliverTo);
				AssertAsciiCharactersValidation("CurrentUser", (Address)eManifest.CurrentUser);
				AssertAsciiCharactersValidation("Containers[0].VerifiedByAddress", ((Container)eManifest.Containers.Single()).VerifiedByAddress);

				AssertAsciiCharactersValidation("PortOfLoad", (Unloco)eManifest.PortOfLoad);
				AssertAsciiCharactersValidation("PortOfDischarge", (Unloco)eManifest.PortOfDischarge);
				AssertAsciiCharactersValidation("PortOfDestination", (Unloco)eManifest.PortOfDestination);
				AssertAsciiCharactersValidation("PlaceOfIssue", (Unloco)eManifest.PlaceOfIssue);
				AssertAsciiCharactersValidation("PlaceOfReceipt", (Unloco)eManifest.PlaceOfReceipt);
				AssertAsciiCharactersValidation("PlaceOfDelivery", (Unloco)eManifest.PlaceOfDelivery);
				AssertAsciiCharactersValidation("FreightPayableAt", (Unloco)eManifest.FreightPayableAt);
				AssertAsciiCharactersValidation("OperationalPort", (Unloco)eManifest.OperationalPort);

				AssertAsciiCharactersValidation("VoyageFlightNumber", ((DocDataObjects.Transport)eManifest.Transports.Main).VoyageFlightNumberInfo);
				AssertAsciiCharactersValidation("Vessel", (Vessel)eManifest.Transports.Main.Vessel);
				AssertAsciiCharactersValidation("SpecialInstructions", eManifest.SpecialInstructionsInfo);
				AssertAsciiCharactersValidation("OtherCharges.Remarks", ((OtherCharges)eManifest.OtherCharges).RemarksInfo);
			});
		}

		public void TestReleaseTypeIsMandatory()
		{
			var shipment = CreateShipment();
			var consol = shipment.Consols[0];
			consol.JK_ReleaseType = ZString.Empty;
			var parameters = new DummyDocDataObjectParameters();

			Assert("Precondition: Shipment is not a direct shipment.", !shipment.IsDirectShipment);
			AssertEquals("Precondition: Consol is an AGT.", Constants.AgentType.Agent, consol.JK_AgentType);
			Assert("Precondition: Consol does not have release type.", consol.JK_ReleaseType.IsEmpty);

			var eManifest = new EManifestBuilder(shipment, parameters).Build();
			eManifest.ValidateAllIncludingChildren();

			AssertHasMessageError((eManifest.ReleaseType as CodeDescription).DescriptionInfo, "Release Type is required.");
		}

		public void TestEmptyContainersCannotBeSent()
		{
			var shipment = CreateEmptyShipmentForEmanifest();

			#region shipment setup

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Direct;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "SGSIN";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CNT0001";
			container1.JC_IsEmptyContainer = true;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CNT0002";

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 2;
			packline1.JL_F3_NKPackType = "PLT";
			packline1.JL_ActualWeight = 123;
			packline1.JL_ActualWeightUQ = "KG";
			packline1.JL_ActualVolume = 34;
			packline1.JL_ActualVolumeUQ = "M3";
			packline1.JL_HarmonisedCode = "ABCDE";
			packline1.JL_ExportRefNumber = "REF001";
			packline1.JL_DetailedDescription = "pack1";

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_PackageCount = 4;
			packline2.JL_F3_NKPackType = "PLT";
			packline2.JL_ActualWeight = 55;
			packline2.JL_ActualWeightUQ = "KG";
			packline2.JL_ActualVolume = 21;
			packline2.JL_ActualVolumeUQ = "M3";
			packline2.JL_HarmonisedCode = "ABCDE";
			packline2.JL_ExportRefNumber = "REF002";
			packline2.JL_DetailedDescription = "pack2";

			var packline3 = shipment.OuterPackLines.AddNew();
			packline3.JL_PackageCount = 5;
			packline3.JL_F3_NKPackType = "PLT";
			packline3.JL_ActualWeight = 36;
			packline3.JL_ActualWeightUQ = "KG";
			packline3.JL_ActualVolume = 125;
			packline3.JL_ActualVolumeUQ = "M3";
			packline3.JL_HarmonisedCode = "ABCDE";
			packline3.JL_ExportRefNumber = "REF003";
			packline3.JL_DetailedDescription = "pack3";

			var packline4 = shipment.OuterPackLines.AddNew();
			packline4.JL_PackageCount = 0;
			packline4.JL_F3_NKPackType = "PLT";
			packline4.JL_ActualWeight = 0;
			packline4.JL_ActualWeightUQ = "KG";
			packline4.JL_ActualVolume = 0;
			packline4.JL_ActualVolumeUQ = "M3";
			packline4.JL_HarmonisedCode = "ABCDE";
			packline4.JL_ExportRefNumber = "REF004";
			packline4.JL_DetailedDescription = "pack4";

			container1.PackLines.Add(packline1);
			container1.PackLines.Add(packline4);

			container2.PackLines.Add(packline2);
			container2.PackLines.Add(packline3);

			#endregion

			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();

			eManifest.ValidateAllIncludingChildren();

			AssertHasMessageError(eManifest.Bookings.SelectMany(b => b.Containers).First().NumberInfo, "This container is flagged as Empty. Remove that flag if incorrect or remove this container from linked packlines.");
		}

		public void TestCopiesOriginalsAreMandatory()
		{
			var shipment = CreateShipment();
			var consol = shipment.Consols.Cast<ForwardingConsol>().First();
			consol.JK_NoOriginalBills = ZByte.Zero;
			consol.JK_NoCopyBills = ZByte.Zero;

			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();

			eManifest.ValidateAllIncludingChildren();

			CombineAssertions(() =>
			{
				AssertHasMessageError(eManifest.NumberOfOriginalsInfo, "Copies/Originals are required.");
				AssertHasMessageError(eManifest.NumberOfCopiesInfo, "Copies/Originals are required.");
			});
		}

		public void TestForwarderIsMandatory()
		{
			var shipment = CreateShipment();
			var consol = shipment.Consols.Cast<ForwardingConsol>().First();
			consol.JK_AgentType = "DRT";
			consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;

			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();

			eManifest.ValidateAllIncludingChildren();

			AssertHasMessageError(((Address)eManifest.Forwarder).CompanyNameInfo, "Forwarder party name and address information is required.");
		}

		public void TestBOLNumberIsMandatory()
		{
			var shipment = CreateShipment();
			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();

			eManifest.ValidateAllIncludingChildren();
			AssertNoMessageError(eManifest.BillOfLadingNumberInfo, "BOL Number is required.");
			eManifest.BillOfLadingNumber = "";
			eManifest.ValidateAllIncludingChildren();
			AssertHasMessageError(eManifest.BillOfLadingNumberInfo, "BOL Number is required.");
		}

		public void TestPlaceOfReceiptIsMandatory()
		{
			var shipment = CreateShipment();
			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();

			eManifest.ValidateAllIncludingChildren();
			AssertNoMessageError(((Unloco)eManifest.PlaceOfReceipt).CodeInfo, "Place of Receipt is required.");
			((Unloco)eManifest.PlaceOfReceipt).Code = "";
			eManifest.ValidateAllIncludingChildren();
			AssertHasMessageError(((Unloco)eManifest.PlaceOfReceipt).CodeInfo, "Place of Receipt is required.");
		}

		public void TestPlaceOfDeliveryIsMandatory()
		{
			var shipment = CreateShipment();
			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();

			eManifest.ValidateAllIncludingChildren();
			AssertNoMessageError(((Unloco)eManifest.PlaceOfDelivery).CodeInfo, "Place of Delivery is required.");
			((Unloco)eManifest.PlaceOfDelivery).Code = "";
			eManifest.ValidateAllIncludingChildren();
			AssertHasMessageError(((Unloco)eManifest.PlaceOfDelivery).CodeInfo, "Place of Delivery is required.");
		}

		public void TestSendingAgentCountryIsMandatory()
		{
			var shipment = CreateShipment(false);
			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();

			eManifest.ValidateAllIncludingChildren();
			AssertNoMessageError(((Address)eManifest.SendingAgent).Country.NameInfo, "Country is required.");
			((Address)eManifest.SendingAgent).Country.Name = "";
			eManifest.ValidateAllIncludingChildren();
			AssertHasMessageError(((Address)eManifest.SendingAgent).Country.NameInfo, "Country is required.");
		}

		public void TestReceivingAgentCountryIsMandatory()
		{
			var shipment = CreateShipment(false);
			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();

			eManifest.ValidateAllIncludingChildren();
			AssertNoMessageError(((Address)eManifest.ReceivingAgent).Country.NameInfo, "Country is required.");

			((Address)eManifest.ReceivingAgent).Country.Name = "";
			eManifest.ValidateAllIncludingChildren();
			AssertHasMessageError(((Address)eManifest.ReceivingAgent).Country.NameInfo, "Country is required.");

			eManifest.ReceivingAgent.CompanyName = "TO ORDER";
			eManifest.ValidateAllIncludingChildren();
			AssertNoMessageError(((Address)eManifest.ReceivingAgent).Country.NameInfo, "Country is required.");
		}

		public void TestNotifyParty1And2CountryIsMandatory()
		{
			var shipment = CreateShipment(false);
			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();

			eManifest.ValidateAllIncludingChildren();
			AssertNoMessageError(((Address)eManifest.NotifyParty).Country.NameInfo, "Country is required.");
			AssertNoMessageError(((Address)eManifest.NotifyParty2).Country.NameInfo, "Country is required.");
			((Address)eManifest.NotifyParty).Country.Name = "";
			((Address)eManifest.NotifyParty2).Country.Name = "";
			eManifest.ValidateAllIncludingChildren();
			AssertHasMessageError(((Address)eManifest.NotifyParty).Country.NameInfo, "Country is required.");
			AssertHasMessageError(((Address)eManifest.NotifyParty2).Country.NameInfo, "Country is required.");
		}

		public void TestNotifyPartyAddressValidation()
		{
			var shipment = CreateShipment();
			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();

			var notifyParty = eManifest.NotifyParty as Address;
			notifyParty.CompanyName = "test name";
			notifyParty.AddressLine1 = "";
			notifyParty.Country.Code = "";

			AssertHasMessageError(notifyParty.CompanyNameInfo, "Notify Party party name and address information is required.");
			AssertHasMessageError(notifyParty.Country.NameInfo, "Country is required.");

			notifyParty = eManifest.NotifyParty as Address;
			notifyParty.CompanyName = "";
			notifyParty.AddressLine1 = "";
			notifyParty.Country.Code = "";

			AssertNoMessageError(notifyParty.CompanyNameInfo, "Notify Party party name and address information is required.");
			AssertNoMessageError(notifyParty.Country.NameInfo, "Country is required.");
		}

		public void TestGoodsDescriptionMaxLengthValidation()
		{
			var shipment = CreateShipment(false);
			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();

			var booking = eManifest.Bookings.ElementAt(0);
			var packLine = booking.Containers.ElementAt(0).PackingLines.ElementAt(0);
			packLine.GoodsDescription = new string('a', 256);
			eManifest.ValidateAllIncludingChildren();
			AssertNoMessageError(packLine.GoodsDescriptionInfo, "Goods Description is too long so will be cut off in the message - Maximum characters 256.");
			packLine.GoodsDescription = new string('a', 257);
			eManifest.ValidateAllIncludingChildren();
			AssertHasMessageError(packLine.GoodsDescriptionInfo, "Goods Description is too long so will be cut off in the message - Maximum characters 256.");
		}

		public void TestPacklineValidation()
		{
			var shipment = CreateShipment(false);
			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();

			var booking = eManifest.Bookings.ElementAt(0);
			var packLine = booking.Containers.ElementAt(0).PackingLines.ElementAt(0);
			packLine.GoodsDescription = "中文";
			packLine.MarksAndNumbers = "中文";

			AssertHasMessageError(packLine.GoodsDescriptionInfo, "Most messaging providers do not support non ASCII characters.");
			AssertHasMessageError(packLine.MarksAndNumbersInfo, "Most messaging providers do not support non ASCII characters.");
		}

		public void TestHSCodeIsMandatoryForBR()
		{
			var shipment = CreateEmptyShipmentForEmanifest();

			#region shipment setup

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Direct;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "BRRIO";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CNT0001";

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 2;
			packline1.JL_F3_NKPackType = "PLT";
			packline1.JL_ActualWeight = 123;
			packline1.JL_ActualWeightUQ = "KG";
			packline1.JL_ActualVolume = 34;
			packline1.JL_ActualVolumeUQ = "M3";
			packline1.JL_HarmonisedCode = "";
			packline1.JL_ExportRefNumber = "REF001";
			packline1.JL_DetailedDescription = "pack1";

			container1.PackLines.Add(packline1);

			#endregion

			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();

			eManifest.ValidateAllIncludingChildren();

			AssertHasWarning(((HarmonizedCode)eManifest.Bookings.SelectMany(b => b.Containers.SelectMany(c => c.PackingLines)).First().HarmonizedCode).CodeInfo, "HS Code is required for Brazil.");
		}

		public void TestTaxInfosValidation()
		{
			TaxNumberTestHelper.AddNewRefDocOrgCusCode(Constants.CountryCodes.China, Constants.CountryCodes.China, OrgCusCode.ChinaCodeTypes.USC, Constants.TaxRelatedDocumentType.ShippingInstruction);

			var shipment = CreateShipment(false);
			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();

			eManifest.ValidateAllIncludingChildren();
			AssertNoWarning(eManifest.SendingAgentTaxInfo.NumberInfo, "Company ID, USC (Unified Social Credit Identifier) is required to comply with CCAM (China Customs Advanced Manifest) reporting in line with Customs Circular No.56.");

			var number = eManifest.SendingAgentTaxInfo.Number;
			eManifest.SendingAgentTaxInfo.Number = "";
			eManifest.ValidateAllIncludingChildren();
			AssertHasWarning(eManifest.SendingAgentTaxInfo.NumberInfo, "Company ID, USC (Unified Social Credit Identifier) is required to comply with CCAM (China Customs Advanced Manifest) reporting in line with Customs Circular No.56.");

			eManifest.SendingAgentTaxInfo.Number = number;
			eManifest.ValidateAllIncludingChildren();
			AssertNoWarning(eManifest.SendingAgentTaxInfo.NumberInfo, "Company ID, USC (Unified Social Credit Identifier) is required to comply with CCAM (China Customs Advanced Manifest) reporting in line with Customs Circular No.56.");
		}

		public void TestTaxInfos()
		{
			TaxNumberTestHelper.AddNewRefDocOrgCusCode(Constants.CountryCodes.China, Constants.CountryCodes.China, OrgCusCode.ChinaCodeTypes.USC, Constants.TaxRelatedDocumentType.ShippingInstruction);
			TaxNumberTestHelper.AddNewRefDocOrgCusCode(Constants.CountryCodes.China, Constants.CountryCodes.Australia, OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, Constants.TaxRelatedDocumentType.ShippingInstruction);
			TaxNumberTestHelper.AddNewRefDocOrgCusCode(Constants.CountryCodes.China, Constants.CountryCodes.UnitedKingdom, OrgCusCode.CodeTypes.VATCode, Constants.TaxRelatedDocumentType.ShippingInstruction);

			var shipment = CreateShipment(false);
			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();

			CombineAssertions(() =>
			{
				AssertEquals("SendingAgentTaxInfo Code", "USC", eManifest.SendingAgentTaxInfo.Code);
				AssertEquals("SendingAgentTaxInfo Description", "USC(desc)", eManifest.SendingAgentTaxInfo.Description);
				AssertEquals("SendingAgentTaxInfo LongLabel", "USC(l)", eManifest.SendingAgentTaxInfo.LongLabel);
				AssertEquals("SendingAgentTaxInfo ShortLabel", "USC(s)", eManifest.SendingAgentTaxInfo.ShortLabel);
				AssertEquals("SendingAgentTaxInfo Number", "1234567890", eManifest.SendingAgentTaxInfo.Number);
				AssertEquals("SendingAgentTaxInfo Country", "CN", eManifest.SendingAgentTaxInfo.Country.Code);

				AssertEquals("ReceivingAgentTaxInfo Code", "ABN", eManifest.ReceivingAgentTaxInfo.Code);
				AssertEquals("ReceivingAgentTaxInfo Description", "ABN(desc)", eManifest.ReceivingAgentTaxInfo.Description);
				AssertEquals("ReceivingAgentTaxInfo LongLabel", "ABN(l)", eManifest.ReceivingAgentTaxInfo.LongLabel);
				AssertEquals("ReceivingAgentTaxInfo ShortLabel", "ABN(s)", eManifest.ReceivingAgentTaxInfo.ShortLabel);
				AssertEquals("ReceivingAgentTaxInfo Number", "410 10 10 10", eManifest.ReceivingAgentTaxInfo.Number);
				AssertEquals("ReceivingAgentTaxInfo Country", "AU", eManifest.ReceivingAgentTaxInfo.Country.Code);

				AssertEquals("NotifyPartyTaxInfo Code", "VAT", eManifest.NotifyPartyTaxInfo.Code);
				AssertEquals("NotifyPartyTaxInfo Description", "VAT(desc)", eManifest.NotifyPartyTaxInfo.Description);
				AssertEquals("NotifyPartyTaxInfo LongLabel", "VAT(l)", eManifest.NotifyPartyTaxInfo.LongLabel);
				AssertEquals("NotifyPartyTaxInfo ShortLabel", "VAT(s)", eManifest.NotifyPartyTaxInfo.ShortLabel);
				AssertEquals("NotifyPartyTaxInfo Number", "123 123 123", eManifest.NotifyPartyTaxInfo.Number);
				AssertEquals("NotifyPartyTaxInfo Country", "GB", eManifest.NotifyPartyTaxInfo.Country.Code);

				AssertEquals("NotifyParty2TaxInfo Code", "USC", eManifest.NotifyParty2TaxInfo.Code);
				AssertEquals("NotifyParty2TaxInfo Description", "USC(desc)", eManifest.NotifyParty2TaxInfo.Description);
				AssertEquals("NotifyParty2TaxInfo LongLabel", "USC(l)", eManifest.NotifyParty2TaxInfo.LongLabel);
				AssertEquals("NotifyParty2TaxInfo ShortLabel", "USC(s)", eManifest.NotifyParty2TaxInfo.ShortLabel);
				AssertEquals("NotifyParty2TaxInfo Number", "1111111111", eManifest.NotifyParty2TaxInfo.Number);
				AssertEquals("NotifyParty2TaxInfo Country", "CN", eManifest.NotifyParty2TaxInfo.Country.Code);
			});
		}

		public void TestPopulateIsRequiredSendAttachment()
		{
			var shipment = CreateShipment();
			var parameters = new DummyDocDataObjectParameters();

			var consol = shipment.Consols.OfType<ForwardingConsol>().First();
			consol.JK_AgentType = Constants.AgentType.CoLoad;

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "OrgHeader";
			orgHeader.OH_RL_NKClosestPort = "CNSHA";
			orgHeader.MainAddress.Address1 = "Unit 1";
			orgHeader.MainAddress.Address2 = "4 What Lane";
			orgHeader.MainAddress.City = "Shanghai";
			orgHeader.MainAddress.Postcode = "5022";
			orgHeader.MainAddress.OA_RN_NKCountryCode = "CN";

			consol.JK_OA_CreditorAddress = orgHeader.MainAddress.PK;

			var refShippingLine = Factory.New<RefShippingLine>();
			orgHeader.OH_RSL_ShippingLine = refShippingLine.PK;

			AssertNull(refShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage));
			Assert(!new EManifestBuilder(shipment, parameters).Build().IsRequiredSendAttachment);

			var messagingRequirement = refShippingLine.ShippingLineMessagingRequirements.AddNew();
			messagingRequirement.RSR_RST_NKType = ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage;
			messagingRequirement.RSR_IsEManifest = true;

			Assert(refShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage).RSR_IsEManifest);
			Assert(new EManifestBuilder(shipment, parameters).Build().IsRequiredSendAttachment);

			messagingRequirement.RSR_IsEManifest = false;

			Assert(!refShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage).RSR_IsEManifest);
			Assert(!new EManifestBuilder(shipment, parameters).Build().IsRequiredSendAttachment);

			messagingRequirement.RSR_IsEManifest = true;
			shipment.Consols.RemoveAll();

			AssertEquals(0, shipment.Consols.Count);
			Assert(!new EManifestBuilder(shipment, parameters).Build().IsRequiredSendAttachment);
		}

		#region TestOverrideAndResetVesselVoyageOnMainTransport

		public void TestOverrideAndResetVesselVoyageOnMainTransport()
		{
			var shipment = CreateShipment(false);
			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();

			AssertEquals("prerequisite; Main Transport Voyage exists and has value", "F9999", eManifest.Transports.Main.VoyageFlightNumber);
			AssertEquals("prerequisite; Transports has 1 element", 1, eManifest.Transports.Count);

			var dynamicEManifest = eManifest.MakeDocDataDynamic();
			var dynamicEManifestTransports = dynamicEManifest.GetDynamicProperty("Transports");
			var dynamicMainTransportVoyage = dynamicEManifestTransports.GetDynamicProperty("Main.VoyageFlightNumber");
			var dynamicCollectionTransport = ((IDynamicDataCollection)dynamicEManifestTransports).ElementAt(0);
			var dynamicCollectionTransportVoyage = dynamicCollectionTransport.GetDynamicProperty("VoyageFlightNumber");

			dynamicMainTransportVoyage.SetValue("ABC");

			AssertEquals("Main Transport Voyage Value", "ABC", dynamicMainTransportVoyage.Value);
			AssertEquals("Main Transport Voyage has been overridden by user in this session", true, dynamicMainTransportVoyage.HasChanges);
			AssertEquals("Main Transport Voyage has been overridden by user", true, dynamicMainTransportVoyage.IsOverridden);

			AssertEquals("Collection Transport Voyage Value", "ABC", dynamicCollectionTransportVoyage.Value);
			AssertEquals("Collection Transport Voyage has been edited by user", true, dynamicCollectionTransportVoyage.HasChanges);
			AssertEquals("Collection Transport Voyage has been overridden by user", true, dynamicCollectionTransportVoyage.IsOverridden);

			var xml = dynamicEManifest.GetOverriddenValuesXml();
			AssertMultilineASCIIEquals("override xml",
$@"<?xml version=""1.0"" encoding=""utf-16"" standalone=""yes""?>
<Entity>
  <Id>{eManifest.Identifier}</Id>
  <Property Name=""Transports"">
    <EntityCollection>
      <Id>{((DocDataObject)eManifest.Transports).Identifier}</Id>
      <Property Name=""Main"">
        <Entity>
          <Id>{((DocDataObject)eManifest.Transports.Main).Identifier}</Id>
          <Property Name=""VoyageFlightNumber"">
            <Value>ABC</Value>
          </Property>
        </Entity>
      </Property>
      <Items>
        <Entity>
          <Id>{((DocDataObject)eManifest.Transports.Main).Identifier}</Id>
          <Property Name=""VoyageFlightNumber"">
            <Value>ABC</Value>
          </Property>
        </Entity>
      </Items>
    </EntityCollection>
  </Property>
</Entity>", xml.ToXmlString());

			var eManifest2 = new EManifestBuilder(shipment, parameters).Build();
			AssertEquals("prerequisite; Main Transport Voyage exists and has value from shipment", "F9999", eManifest2.Transports.Main.VoyageFlightNumber);

			var dynamicEManifest2 = eManifest2.MakeDocDataDynamic();
			dynamicEManifest2.MergeDataFromXml(xml);

			var dynamicEManifestTransports2 = dynamicEManifest2.GetDynamicProperty("Transports");
			var dynamicMainTransportVoyage2 = dynamicEManifestTransports2.GetDynamicProperty("Main.VoyageFlightNumber");
			var dynamicCollectionTransport2 = ((IDynamicDataCollection)dynamicEManifestTransports2).ElementAt(0);
			var dynamicCollectionTransportVoyage2 = dynamicCollectionTransport2.GetDynamicProperty("VoyageFlightNumber");

			AssertEquals("Main Transport Voyage shows overridden value", "ABC", dynamicMainTransportVoyage2.Value);
			AssertEquals("Main Transport Voyage has not been edited by user", false, dynamicMainTransportVoyage2.HasChanges);
			AssertEquals("Main Transport Voyage has been overridden by user", true, dynamicMainTransportVoyage2.IsOverridden);

			AssertEquals("Collection Transport Voyage Value", "ABC", dynamicCollectionTransportVoyage2.Value);
			AssertEquals("Collection Transport Voyage has not been edited by user", false, dynamicCollectionTransportVoyage2.HasChanges);
			AssertEquals("Collection Transport Voyage has been overridden by user", true, dynamicCollectionTransportVoyage2.IsOverridden);

			dynamicEManifest2.CancelChanges();

			AssertEquals("Main Transport Voyage has been reset to original", "F9999", dynamicMainTransportVoyage2.Value);
			AssertEquals("Main Transport Voyage has been edited by user", true, dynamicMainTransportVoyage2.HasChanges);
			AssertEquals("Main Transport Voyage override has been undone", false, dynamicMainTransportVoyage2.IsOverridden);

			AssertEquals("Collection Transport Voyage has been reset to original", "F9999", dynamicCollectionTransportVoyage2.Value);
			AssertEquals("Collection Transport Voyage has been edited by user", true, dynamicCollectionTransportVoyage2.HasChanges);
			AssertEquals("Collection Transport Voyage override has been undone", false, dynamicCollectionTransportVoyage2.IsOverridden);
		}

		#endregion

		#region PopulateContact

		public void TestPoplulateContactInformation()
		{
			var shipment = CreateShipment(false, true);
			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();
			AssertEquals("Contact Name sendingForwarder should be SPIDERMAN", "SPIDERMAN", eManifest.SendingAgent.Contact);
			AssertEquals("Email sendingForwarder should be spider@marvel.com", "spider@marvel.com", eManifest.SendingAgent.Email);
			AssertEquals("Phone sendingForwarder should be 911", "911", eManifest.SendingAgent.Phone);

			AssertEquals("Contact Name receivingForwarder should be Black Panther", "Black Panther", eManifest.ReceivingAgent.Contact);
			AssertEquals("Email receivingForwarder should be black_panther@marvel.com", "black_panther@marvel.com", eManifest.ReceivingAgent.Email);
			AssertEquals("Phone receivingForwarder should be 112", "112", eManifest.ReceivingAgent.Phone);

			AssertEquals("Contact Name sendingForwarder should be SPIDERMAN", "SPIDERMAN", eManifest.Forwarder.Contact);
			AssertEquals("Email sendingForwarder should be spider@marvel.com", "spider@marvel.com", eManifest.Forwarder.Email);
			AssertEquals("Phone sendingForwarder should be 911", "911", eManifest.Forwarder.Phone);
		}

		#endregion

		#region PortOfDestination

		public void TestPopulatePortOfDestination()
		{
			var shipment = CreateShipment();
			var parameters = new DummyDocDataObjectParameters();

			AssertNotNull("Precondition: Shipment has destination.", shipment.Destination);

			var eManifest = new EManifestBuilder(shipment, parameters).Build();

			AssertEquals(eManifest.PortOfDestination.Code, shipment.Destination.Code);
			AssertEquals(eManifest.PortOfDestination.Name, shipment.Destination.RL_PortName);

			shipment.JS_RL_NKDestination = ZString.Empty;

			AssertNull("Precondition: Shipment has not destination.", shipment.Destination);

			eManifest = new EManifestBuilder(shipment, parameters).Build();

			AssertNullOrEmpty(eManifest.PortOfDestination.Code);
			AssertNullOrEmpty(eManifest.PortOfDestination.Name);
		}

		public void TestPortOfDestinationIsMandatory()
		{
			var shipment = CreateShipment();
			var parameters = new DummyDocDataObjectParameters();

			AssertNotNull("Precondition: Shipment has destination.", shipment.Destination);

			var eManifest = new EManifestBuilder(shipment, parameters).Build();
			eManifest.ValidateAllIncludingChildren();

			AssertNoMessageError(((Unloco)eManifest.PortOfDestination).CodeInfo, "Final Destination is required.");

			shipment.JS_RL_NKDestination = ZString.Empty;

			AssertNull("Precondition: Shipment has not destination.", shipment.Destination);

			eManifest = new EManifestBuilder(shipment, parameters).Build();
			eManifest.ValidateAllIncludingChildren();

			AssertHasMessageError(((Unloco)eManifest.PortOfDestination).CodeInfo, "Final Destination is required.");
		}

		#endregion

		public void TestSameContainerAndBookingHaveMultiplePackines()
		{
			var shipment = CreateEmptyShipmentForEmanifest();

			#region shipment setup

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Direct;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "BRRIO";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CNT0001";

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 10;
			packline1.JL_F3_NKPackType = "PLT";
			packline1.JL_ActualWeight = 100;
			packline1.JL_ActualWeightUQ = "KG";
			packline1.JL_ActualVolume = 100;
			packline1.JL_ActualVolumeUQ = "M3";
			packline1.JL_HarmonisedCode = "";
			packline1.JL_ExportRefNumber = "REF001";
			packline1.JL_DetailedDescription = "pack1";

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_PackageCount = 20;
			packline2.JL_F3_NKPackType = "PLT";
			packline2.JL_ActualWeight = 200;
			packline2.JL_ActualWeightUQ = "KG";
			packline2.JL_ActualVolume = 200;
			packline2.JL_ActualVolumeUQ = "M3";
			packline2.JL_HarmonisedCode = "";
			packline2.JL_ExportRefNumber = "REF002";
			packline2.JL_DetailedDescription = "pack2";

			container.PackLines.Add(packline1);
			container.PackLines.Add(packline2);

			#endregion

			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();

			AssertEquals(2, eManifest.Bookings.Count);

			var expectedPackCounts = new ZInt[] { 10, 20 };
			var containers = eManifest.Bookings.SelectMany(a => a.Containers).OfType<BookingContainer>();

			AssertContainsExactElementsInAnyOrder(
				"Each Booking should have it's own PackCount based on container packlines",
				expectedPackCounts,
				containers.Select(c => c.PackCount)
			);

			AssertEquals(
				"Each Booking container should have a unique ID",
				2,
				containers.Select(c => c.Identifier).Distinct().Count()
			);

			AssertEquals(
				"When wrapping the containers in dynamic data, it should still have unique identifiers",
				2,
				containers.Select(c => c.MakeDocDataDynamic().Value.As<BookingContainer>().Identifier).Distinct().Count()
			);
		}

		public void TestWarningForShiLianDanIsUsedInMoreThanOneShipment()
		{
			InsertPackLineTestData("CNSHA", "S00001001", "SLD001");
			InsertEntryNumTestData("CNSHA", "S00001002", "SLD001");

			Factory.Save();

			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(InsertPackLineTestData("CNSHA", "S00001003", "SLD001"), parameters).Build();
			var booking = eManifest.Bookings.ElementAt(0);
			var expectWarning = "This Shi Lian Dan/Shipping Order Number is already in use on: S00001001, S00001002";
			AssertHasWarning(booking.BookingNumberInfo, expectWarning);

			ForwardingShipment InsertPackLineTestData(string origin, string consignRef, string exportRefNumber)
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = origin;
				shipment.JS_UniqueConsignRef = consignRef;
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

				var consol = shipment.Consols.AddNew();
				var container = consol.Containers.AddNew();

				var packline = shipment.OuterPackLines.AddNew();
				packline.SetContainer(container.PK);
				packline.JL_ContainerPackingOrder = 1;
				packline.JL_ExportRefNumber = exportRefNumber;
				return shipment;
			}

			ForwardingShipment InsertEntryNumTestData(string origin, string consignRef, string exportRefNumber)
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_UniqueConsignRef = consignRef;
				shipment.JS_RL_NKOrigin = origin;
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

				var shipmentEntryNum = shipment.Numbers.AddNew();
				shipmentEntryNum.CE_EntryType = ChinaAdditionalReferenceNumberTypes.Codes.ShippingOrderNumber;
				shipmentEntryNum.CE_EntryNum = exportRefNumber;
				return shipment;
			}
		}

		public void TestCarrierMessagingRequirementsValidation_IEL()
		{
			var errorMessage = "This carrier only supports integration via email to local office.\r\nContact name and email address are required to send eManifest.\r\nPlease maintain contact name and email address in carrier Organization > Contact > Email and Receiving Documents > Group SHP.";

			var shipment = CreateShipment();
			var consol = shipment.Consols.OfType<ForwardingConsol>().First();
			var carrier = consol.ShippingLine;

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			carrier.OH_RSL_ShippingLine = shippingLine.PK;

			var contact = carrier.Contacts.AddNew();
			contact.OC_Email = "test@test.com";
			contact.OC_ContactName = "TEST NAME";

			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = "ALL";

			var carrierMessageData = new EManifestBuilder(shipment, null).Build();
			AssertNullOrEmpty(((Address)carrierMessageData.Carrier).Contact);

			var shippingLineMessagingRequirement = shippingLine.ShippingLineMessagingRequirements.AddNew();
			shippingLineMessagingRequirement.RSR_RST_NKType = ShippingLineMessagingRequirement.Types.IntegrationViaEmailToCarrierLocalOffice;
			shippingLineMessagingRequirement.RSR_IsEManifest = true;

			carrierMessageData = new EManifestBuilder(shipment, null).Build();
			AssertEquals("TEST NAME", ((Address)carrierMessageData.Carrier).Contact);
			AssertNoMessageError(((Address)carrierMessageData.Carrier).ContactInfo, errorMessage);

			contact.Documents.RemoveAll();

			carrierMessageData = new EManifestBuilder(shipment, null).Build();
			AssertHasMessageError(((Address)carrierMessageData.Carrier).ContactInfo, errorMessage);

			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;

			shippingLine.RSL_EManifestAvailable = true;
			carrierMessageData = new EManifestBuilder(shipment, null).Build();
			AssertHasMessageError(((Address)carrierMessageData.Carrier).ContactInfo, errorMessage);
		}

		#region TestContainerMode_ShippersConsol

		public void TestContainerMode_ShippersConsol()
		{
			var shipment = CreateShipment();
			var consol = shipment.Consols.OfType<ForwardingConsol>().First();
			consol.JK_ConsolMode = Constants.ContainerModes.ShippersConsol;

			var parameters = new DummyDocDataObjectParameters();
			var eManifest = new EManifestBuilder(shipment, parameters).Build();
			AssertEquals(Constants.ContainerModes.FCL, eManifest.ContainerMode.Code);
			AssertEquals(Constants.ContainerModeDescriptions.FCL, eManifest.ContainerMode.Description);
		}

		#endregion

		#region Implementation

		ForwardingShipment CreateShipment(bool isDirect = false, bool includeContactDetail = false)
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SH0001000";
			shipment.JS_HouseBill = "HOUSEBILL001";
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_HouseBillIssueDate = new ZDateTime(2018, 10, 1);
			shipment.JS_HBLContainerPackModeOverride = Constants.HBLDeliveryModes.Codes.CFS_CY;
			shipment.JS_INCO = Constants.IncoTerms.CostAndFreight;
			shipment.JS_ShippedOnBoard = "SHP";
			shipment.JS_ShippedOnBoardDate = ZDate.Today;
			shipment.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment.JS_E_ARV = ZDate.Today.AddDays(2);
			shipment.JS_GoodsDescription = "goods description";
			shipment.JS_MarksAndNumbers = "marks & numbers";
			shipment.JS_BookingReference = "BKG000001";

			var shipmentInstruction = shipment.Notes.AddNew();
			shipmentInstruction.ST_Description = PredefinedNoteTypes.Instance.SpecialInstructions.Description;
			shipmentInstruction.ST_NoteText = "shipment special instructions";

			var number = shipment.Numbers.AddNew();
			number.CE_EntryType = ChinaAdditionalReferenceNumberTypes.Codes.ShippingOrderNumber;
			number.CE_RN_NKCountryCode = Constants.CountryCodes.China;
			number.CE_EntryNum = "SHPREF001";

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_AgentType = isDirect ? Constants.AgentType.Direct : Constants.AgentType.Agent;
			consol1.JK_ReleaseType = Constants.ShipmentReleaseTypes.OriginalReq;
			consol1.JK_NoOriginalBills = 3;
			consol1.JK_NoCopyBills = 4;
			consol1.JK_RL_NKLoadPort = "CNSHA";
			consol1.JK_RL_NKDischargePort = "SGSIN";
			consol1.JK_BookingReference = "驴100";
			consol1.JK_MasterBillIssueDate = new ZDateTime(2018, 10, 2);
			consol1.JK_MasterBillNum = "BILLNUMBER";
			consol1.JK_AgentsReference = "AGTREF";
			consol1.JK_UniqueConsignRef = "CON0001";
			consol1.JK_PrepaidCollect = Constants.PaymentType.Collect;
			consol1.JK_CarrierContractNumber = "CARCON";

			var quotationNumber = consol1.Numbers.AddNew();
			quotationNumber.CE_RN_NKCountryCode = Constants.CountryCodes.China;
			quotationNumber.CE_EntryType = AdditionalReferences.Codes.CarrierQuoteNumber;
			quotationNumber.CE_EntryNum = "QUOT123";

			var consolInstruction = consol1.Notes.AddNew();
			consolInstruction.ST_Description = PredefinedNoteTypes.Instance.SpecialInstructions.Description;
			consolInstruction.ST_NoteText = "consol special instructions";

			PopulateConsolAddresses(consol1, includeContactDetail);

			var trasport1 = consol1.Transports.OfType<Freight.Business.Transport>().Single();
			trasport1.JW_LegOrder = 1;
			trasport1.JW_TransportMode = Constants.TransportModes.Sea;
			trasport1.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			trasport1.JW_RL_NKLoadPort = "CNSHA";
			trasport1.JW_RL_NKDiscPort = "SGSIN";
			trasport1.JW_Vessel = "Sea Dragon";
			trasport1.JW_VoyageFlight = "F9999";

			var refContainer = Factory.NewWithValidTestData<RefContainer>();
			refContainer.RC_ISOType = "22P1";
			refContainer.RC_ContainerType = "RFG";
			refContainer.RC_TareWeight = 222;

			var container1 = consol1.Containers.AddNew();
			container1.JC_ContainerNum = "AAAA0000007";
			container1.JC_RC = refContainer.PK;
			container1.JC_DeliveryMode = "CFS/CY";
			container1.JC_IsShipperOwned = true;
			container1.JC_GrossWeightUQ = "KG";
			container1.JC_TareWeight = 1000;
			container1.JC_DunnageWeight = 1000;

			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 2;
			packline1.JL_F3_NKPackType = "PLT";
			packline1.JL_ActualWeight = 200;
			packline1.JL_ActualWeightUQ = "KG";
			packline1.JL_ActualVolume = 300;
			packline1.JL_ActualVolumeUQ = "M3";
			packline1.JL_HarmonisedCode = "ABCDE";
			packline1.JL_ExportRefNumber = "REF001";
			packline1.JL_DetailedDescription = "pack1";

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Handsome";
			contact.OC_Phone = "1234567";

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "6666";
			subs.DG_Variant = "E";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var undg = packline1.UNDGs.AddNew();
			undg.LinkDefault(subs);
			undg.Substance.DG_PSN = "DG SHIPPER NAME";
			undg.DI_TechnicalName = "WHATEVER";
			undg.DI_IMOClass = "CLAS";
			undg.Substance.DG_PG = "GRO";
			undg.Substance.DG_SubLabel1 = "sub1";
			undg.Substance.DG_SubLabel2 = "su2";
			undg.DI_IsCombustible = true;
			undg.DI_DGFlashPoint = 15.0m;
			undg.DI_MPMarinePollutant = "N";
			undg.DI_DGVolume = 2m;
			undg.DI_UnitOfVolume = "M3";
			undg.DI_DGWeight = 200m;
			undg.DI_UnitOfWeight = "KG";
			undg.DI_IsLimitedQuantity = true;
			undg.DI_PackageCount = 5;
			undg.DI_F3_NKPackType = "BAG";
			undg.DI_OC_DGContact = contact.PK;

			var hc = Factory.New<JobPackLineHarmonisedCode>();
			hc.JLH_RN_NKCountry = "CN";
			hc.JLH_Code = "1234";

			packline1.HarmonisedCodes.Add(hc);

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_ExportRefNumber = "BBB";
			packline2.JL_PackageCount = 5;
			packline2.JL_ExportRefNumber = "REF001";
			packline2.JL_DetailedDescription = "pack2";

			var packline3 = shipment.OuterPackLines.AddNew();
			packline3.JL_PackageCount = 3;
			packline3.JL_F3_NKPackType = "PLT";
			packline3.JL_ActualWeight = 20000;
			packline3.JL_ActualWeightUQ = "G";
			packline3.JL_ActualVolume = 300;
			packline3.JL_ActualVolumeUQ = "M3";
			packline3.JL_HarmonisedCode = "EEEEE";
			packline3.JL_ExportRefNumber = "REF002";
			packline3.JL_DetailedDescription = "pack3";

			var packline4 = shipment.OuterPackLines.AddNew();
			packline4.JL_PackageCount = 5;
			packline4.JL_F3_NKPackType = "PLT";
			packline4.JL_ActualWeight = 15555;
			packline4.JL_ActualWeightUQ = "G";
			packline4.JL_ActualVolume = 250;
			packline4.JL_ActualVolumeUQ = "M3";
			packline4.JL_HarmonisedCode = "EEEEE";
			packline4.JL_DetailedDescription = "pack4";

			container1.PackLines.Add(packline1);
			container1.PackLines.Add(packline2);
			container1.PackLines.Add(packline3);
			container1.PackLines.Add(packline4);

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_RL_NKLoadPort = "SGSIN";
			consol2.JK_RL_NKDischargePort = "AUBNE";

			PopulateConsolAddresses(consol2);

			var trasport2 = consol2.Transports.OfType<Freight.Business.Transport>().Single();
			trasport2.JW_LegOrder = 1;
			trasport2.JW_TransportMode = Constants.TransportModes.Sea;
			trasport2.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			trasport2.JW_RL_NKLoadPort = "SGSIN";
			trasport2.JW_RL_NKDiscPort = "AUBNE";
			trasport2.JW_Vessel = "Stiff Noodle";
			trasport2.JW_VoyageFlight = "B1111";

			var container2 = consol2.Containers.AddNew();
			container2.JC_ContainerNum = "BBBB0000007";
			container2.JC_DeliveryMode = "CY/CY";

			container2.PackLines.Add(packline1);
			container2.PackLines.Add(packline2);
			container2.PackLines.Add(packline3);

			PopulateShipmentAddresses(shipment);

			Factory.Save();

			return shipment;
		}

		void PopulateShipmentAddresses(ForwardingShipment shipment)
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "I'm consignor";
			consignor.OH_RL_NKClosestPort = "CNBSX";
			consignor.MainAddress.Address1 = "Unit 200";
			consignor.MainAddress.Address2 = "55 haha Lane";
			consignor.MainAddress.City = "wahaha Ave";
			consignor.MainAddress.Postcode = "10000";
			consignor.MainAddress.OA_RN_NKCountryCode = "CN";

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "I'm consignee";
			consignee.OH_RL_NKClosestPort = "AUMEL";
			consignee.MainAddress.Address1 = "Unit 223";
			consignee.MainAddress.Address2 = "553 What Lane";
			consignee.MainAddress.City = "Melbourne";
			consignee.MainAddress.Postcode = "5023";
			consignee.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_FullName = "Notify Me";
			notifyParty.OH_RL_NKClosestPort = "NZAKL";
			notifyParty.MainAddress.Address1 = "Unit 888";
			notifyParty.MainAddress.Address2 = "8 What Lane";
			notifyParty.MainAddress.City = "Auckland";
			notifyParty.MainAddress.Postcode = "5012";
			notifyParty.MainAddress.OA_RN_NKCountryCode = "NZ";

			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;

			var notifyParty2 = Factory.New<OrgHeader>();
			notifyParty2.OH_FullName = "Notify Me Two";
			notifyParty2.OH_RL_NKClosestPort = "NZAKL";
			notifyParty2.MainAddress.Address1 = "Unit 666";
			notifyParty2.MainAddress.Address2 = "8 How Lane";
			notifyParty2.MainAddress.City = "Auckland";
			notifyParty2.MainAddress.Postcode = "5032";
			notifyParty2.MainAddress.OA_RN_NKCountryCode = "NZ";

			shipment.NotifyParty2DocumentaryAddress.E2_OA_Address = notifyParty2.MainAddress.PK;

			var consignorPickupAddress = Factory.New<OrgHeader>();
			consignorPickupAddress.OH_FullName = "consignor Pickup org";
			consignorPickupAddress.OH_RL_NKClosestPort = "CNBZN";
			consignorPickupAddress.MainAddress.Address1 = "Unit 645";
			consignorPickupAddress.MainAddress.Address2 = "234 Drive";
			consignorPickupAddress.MainAddress.City = "unknown city";
			consignorPickupAddress.MainAddress.Postcode = "3243";
			consignorPickupAddress.MainAddress.OA_RN_NKCountryCode = "CN";

			shipment.ConsignorPickupAddress.E2_OA_Address = consignorPickupAddress.MainAddress.PK;

			var consigneeDeliveryAddress = Factory.New<OrgHeader>();
			consigneeDeliveryAddress.OH_FullName = "consignee delivery org";
			consigneeDeliveryAddress.OH_RL_NKClosestPort = "SGJUR";
			consigneeDeliveryAddress.MainAddress.Address1 = "Unit 563";
			consigneeDeliveryAddress.MainAddress.Address2 = "435 Drive";
			consigneeDeliveryAddress.MainAddress.City = "unknown city";
			consigneeDeliveryAddress.MainAddress.Postcode = "4356";
			consigneeDeliveryAddress.MainAddress.OA_RN_NKCountryCode = "SG";

			shipment.ConsigneeDeliveryAddress.E2_OA_Address = consigneeDeliveryAddress.MainAddress.PK;
		}

		void PopulateConsolAddresses(ForwardingConsol consol, bool includeContactDetail = false)
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "MAERSK";
			carrier.OH_RL_NKClosestPort = "DKAAL";
			carrier.MainAddress.Address1 = "Unit 13";
			carrier.MainAddress.Address2 = "4 Lost Lane";
			carrier.MainAddress.City = "Aalborg";
			carrier.MainAddress.Postcode = "2000";
			carrier.MainAddress.OA_RN_NKCountryCode = "DK";

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "I'm Sending Stuff";
			sendingForwarder.OH_RL_NKClosestPort = "CNNJI";
			sendingForwarder.MainAddress.Address1 = "Unit 200";
			sendingForwarder.MainAddress.Address2 = "55 Why Lane";
			sendingForwarder.MainAddress.City = "Conficious Ave";
			sendingForwarder.MainAddress.Postcode = "10000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "CN";
			sendingForwarder.CustomsCodes.AddNew(OrgCusCode.ChinaCodeTypes.USC, "1234567890", "CN");
			if (includeContactDetail)
			{
				var sendingForwarderContact = sendingForwarder.Contacts.AddNew();
				sendingForwarderContact.OC_ContactName = "SPIDERMAN";
				sendingForwarderContact.OC_Email = "spider@marvel.com";
				sendingForwarderContact.OC_Phone = "911";
			}
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "I'm Receiving Stuff";
			receivingForwarder.OH_RL_NKClosestPort = "AUSYD";
			receivingForwarder.MainAddress.Address1 = "Unit 399";
			receivingForwarder.MainAddress.Address2 = "50 What Lane";
			receivingForwarder.MainAddress.City = "Sydney";
			receivingForwarder.MainAddress.Postcode = "5023";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";
			receivingForwarder.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "410 10 10 10", "AU");
			if (includeContactDetail)
			{
				var receivingForwarderContact = receivingForwarder.Contacts.AddNew();
				receivingForwarderContact.OC_ContactName = "Black Panther";
				receivingForwarderContact.OC_Email = "black_panther@marvel.com";
				receivingForwarderContact.OC_Phone = "112";
			}
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_FullName = "consol notify party";
			notifyParty.OH_RL_NKClosestPort = "GBLON";
			notifyParty.MainAddress.Address1 = "Unit 400";
			notifyParty.MainAddress.Address2 = "443 How Lane";
			notifyParty.MainAddress.City = "Angel";
			notifyParty.MainAddress.Postcode = "8888";
			notifyParty.MainAddress.OA_RN_NKCountryCode = "GB";
			notifyParty.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "123 123 123", "GB");

			consol.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;

			var notifyParty2 = Factory.New<OrgHeader>();
			notifyParty2.OH_FullName = "consol notify party 2";
			notifyParty2.OH_RL_NKClosestPort = "CNCAN";
			notifyParty2.MainAddress.Address1 = "Unit 460";
			notifyParty2.MainAddress.Address2 = "333 How Lane";
			notifyParty2.MainAddress.City = "Wonderland";
			notifyParty2.MainAddress.Postcode = "7777";
			notifyParty2.MainAddress.OA_RN_NKCountryCode = "CN";
			notifyParty2.CustomsCodes.AddNew(OrgCusCode.ChinaCodeTypes.USC, "1111111111", "CN");

			consol.NotifyParty2DocumentaryAddress.E2_OA_Address = notifyParty2.MainAddress.PK;

			var carrierHanlingAgent = Factory.New<OrgHeader>();
			carrierHanlingAgent.OH_FullName = "consol carrier handling agent";
			carrierHanlingAgent.OH_RL_NKClosestPort = "CNCAN";
			carrierHanlingAgent.MainAddress.Address1 = "Unit 990";
			carrierHanlingAgent.MainAddress.Address2 = "245 Drive";
			carrierHanlingAgent.MainAddress.City = "unknown city";
			carrierHanlingAgent.MainAddress.Postcode = "4689";
			carrierHanlingAgent.MainAddress.OA_RN_NKCountryCode = "CN";

			consol.CarrierHandlingAgentDocumentaryAddress.E2_OA_Address = carrierHanlingAgent.MainAddress.PK;

			var carrierBookingAgent = Factory.New<OrgHeader>();
			carrierBookingAgent.OH_FullName = "consol carrier booking agent";
			carrierBookingAgent.OH_RL_NKClosestPort = "CNCAN";
			carrierBookingAgent.MainAddress.Address1 = "Unit 990";
			carrierBookingAgent.MainAddress.Address2 = "245 Drive";
			carrierBookingAgent.MainAddress.City = "unknown city";
			carrierBookingAgent.MainAddress.Postcode = "4689";
			carrierBookingAgent.MainAddress.OA_RN_NKCountryCode = "CN";

			consol.CarrierBookingAgentDocumentaryAddress.E2_OA_Address = carrierBookingAgent.MainAddress.PK;

			var packDepotOrg = Factory.New<OrgHeader>();
			packDepotOrg.OH_FullName = "pack depot org";
			packDepotOrg.OH_RL_NKClosestPort = "CNCAN";
			packDepotOrg.MainAddress.Address1 = "Unit 888";
			packDepotOrg.MainAddress.Address2 = "111 Drive";
			packDepotOrg.MainAddress.City = "unknown city";
			packDepotOrg.MainAddress.Postcode = "4679";
			packDepotOrg.MainAddress.OA_RN_NKCountryCode = "CN";

			consol.JK_OA_PackDepotAddress = packDepotOrg.MainAddress.PK;

			var unpackDepotOrg = Factory.New<OrgHeader>();
			unpackDepotOrg.OH_FullName = "unpack depot org";
			unpackDepotOrg.OH_RL_NKClosestPort = "SGSIN";
			unpackDepotOrg.MainAddress.Address1 = "Unit 589";
			unpackDepotOrg.MainAddress.Address2 = "625 Drive";
			unpackDepotOrg.MainAddress.City = "unknown city";
			unpackDepotOrg.MainAddress.Postcode = "9541";
			unpackDepotOrg.MainAddress.OA_RN_NKCountryCode = "SG";

			consol.JK_OA_UnpackDepotAddress = unpackDepotOrg.MainAddress.PK;
		}

		ForwardingShipment CreateEmptyShipmentForEmanifest()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			return shipment;
		}

		#endregion
	}
}
