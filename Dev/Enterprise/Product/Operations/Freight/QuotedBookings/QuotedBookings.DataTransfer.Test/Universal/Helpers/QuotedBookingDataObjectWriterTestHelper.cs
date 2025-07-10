using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Testing;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal.Test
{
	internal class QuotedBookingDataObjectWriterTestHelper : Assertion
	{
		public QuotedBookingDataObjectWriterTestHelper(UniversalObjectFactory factory, Func<QuotedBooking, Shipment> getShipmentData)
		{
			this.factory = factory;
			this.getShipmentData = getShipmentData;
		}

		readonly UniversalObjectFactory factory;
		readonly Func<QuotedBooking, Shipment> getShipmentData;

		public void AssertWorkflowCustomFields(QuoteBookingType quotedBookingType)
		{
			var quotedBooking = QuotedBooking.New(quotedBookingType, factory.BOFactory);
			quotedBooking.SetUserDefinedValue("Are you Happy?", ZBool.True);
			quotedBooking.SetUserDefinedValue("What Makes You Happy?", new ZString("Lots Of Ice"));
			quotedBooking.SetUserDefinedValue("The Happy Number", new ZInt(42));
			quotedBooking.SetUserDefinedValue("The Happy Decimal", new ZDecimal(7.7));
			quotedBooking.SetUserDefinedValue("The Date You Are Happy", ZDateTime.BrettsBirthday);

			var shipmentData = getShipmentData(quotedBooking);
			AssertNotNull("Precondition: shipmentData", shipmentData);

			var customFields = shipmentData.CustomizedFieldCollection;
			AssertNotNull(customFields);
			CombineAssertions(() =>
			{
				AssertEquals("customFields.Count", 5, customFields.Count);
				customFields.AssertCustomFieldWasExported(DataType.Boolean, "Are you Happy?", "true");
				customFields.AssertCustomFieldWasExported(DataType.String, "What Makes You Happy?", "Lots Of Ice");
				customFields.AssertCustomFieldWasExported(DataType.Integer, "The Happy Number", "42");
				customFields.AssertCustomFieldWasExported(DataType.Decimal, "The Happy Decimal", "7.7");
				customFields.AssertCustomFieldWasExported(DataType.DateTime, "The Date You Are Happy", ZDateTime.BrettsBirthday.ToISO8601String());
			});
		}

		public void AssertWorkflowCustomFields_FromMatchedWorkflow(QuoteBookingType quotedBookingType, string quotedBookingCode, Func<QuotedBooking, IWorkflowProviderCore> getWorkflowProviderCore)
		{
			var bookingProcessTaskTemplate = factory.NewWithValidTestData<ProcessTaskTemplate>();
			bookingProcessTaskTemplate.P0_ProcessType = TriggerLineTypes.Codes.QuotedBooking;
			bookingProcessTaskTemplate.P0_SubType1 = "SEA";
			bookingProcessTaskTemplate.P0_SubType2 = quotedBookingCode;
			bookingProcessTaskTemplate.P0_LoadPortCountry = "AU";
			bookingProcessTaskTemplate.P0_IsActive = true;

			var bookingCustomField = bookingProcessTaskTemplate.GenCustomColumnDefinitions.AddNew();
			bookingCustomField.XC_Name = $"{quotedBookingCode} Workflow Custom Field";
			bookingCustomField.XC_Type = "STR";

			var shipmentProcessTaskTemplatePK = ZGuid.Empty;
			if (quotedBookingCode != QuotedBooking.SpotQuoteCode)
			{
				var shipmentProcessTaskTemplate = factory.NewWithValidTestData<ProcessTaskTemplate>();
				shipmentProcessTaskTemplate.P0_ProcessType = "SHP";
				shipmentProcessTaskTemplate.P0_SubType1 = "SEA";
				shipmentProcessTaskTemplate.P0_SubType2 = "EXP";
				shipmentProcessTaskTemplate.P0_LoadPortCountry = "AU";
				shipmentProcessTaskTemplate.P0_IsActive = true;

				var shipmentCustomField = shipmentProcessTaskTemplate.GenCustomColumnDefinitions.AddNew();
				shipmentCustomField.XC_Name = "Shipment Workflow Custom Field";
				shipmentCustomField.XC_Type = "STR";

				shipmentProcessTaskTemplatePK = shipmentProcessTaskTemplate.PK;
			}
			factory.SaveForTesting();

			var quotedBooking = QuotedBooking.New(quotedBookingType, factory.BOFactory);
			quotedBooking.Origin = "AUBNE";
			quotedBooking.Destination = "NZAKL";
			quotedBooking.Mode = Core.Constants.RateMode.FCL;

			factory.SaveForTesting();

			var loader = new ProcessTaskTemplate.Loader(factory.BOFactory);
			var quotedBookingWorkflowMatches = loader.FindMatches(quotedBooking);
			Assert("Expected to match at least 1 QBK workflow template", quotedBookingWorkflowMatches.Length > 0);
			AssertEquals("QBK template should be the 1st match", bookingProcessTaskTemplate.PK, quotedBookingWorkflowMatches[0].PK);

			if (quotedBookingCode != QuotedBooking.SpotQuoteCode)
			{
				var shipmentWorkflowMatches = loader.FindMatches(getWorkflowProviderCore(quotedBooking));
				Assert("Expected to match at least 1 SHP workflow template", shipmentWorkflowMatches.Length > 0);
				AssertEquals("SHP template should be the 1st match", shipmentProcessTaskTemplatePK, shipmentWorkflowMatches[0].PK);
			}

			var shipmentData = getShipmentData(quotedBooking);
			AssertNotNull("Precondition: shipmentData", shipmentData);

			var customFields = shipmentData.CustomizedFieldCollection;
			AssertNotNull("custom fields have been included in the uxml", customFields);

			var exportedCustomFieldsInfo = customFields.Select(cf => $"{cf.Key}|{cf.DataType}|{cf.Value}");

			AssertContainsExactElementsInAnyOrder("exported custom fields", new[]
			{
				$"{quotedBookingCode} Workflow Custom Field|String|"
			}, exportedCustomFieldsInfo);
		}

		public void AssertNotes(QuotedBooking quotedBooking)
		{
			var noteBO1 = quotedBooking.Notes.AddNew(true, "CAT EATER!!", "Feee-lix the cat, what a wonderful-wonderful cat.");
			noteBO1.ST_NoteType = nameof(CargoWise.Definitions.StmNoteVisibility.PUB);
			var noteBO2 = quotedBooking.Notes.AddNew(false, "Internal Work Notes", "Flintstones, meet the Flintstones.");
			noteBO2.ST_NoteContext = "DEB";

			var shipmentData = getShipmentData(quotedBooking);

			AssertNotNull("shipmentData", shipmentData);
			AssertNotNull("shipmentData.NoteCollection", shipmentData.NoteCollection);
			AssertEquals("shipmentData.NoteCollection.Count", 2, shipmentData.NoteCollection.Count);

			var note1 = shipmentData.NoteCollection[0];
			CombineAssertions(() =>
			{
				AssertEquals("note1.Description", "CAT EATER!!", note1.Description);
				AssertEquals("note1.IsCustomDescription", ZBool.True, note1.IsCustomDescription);
				AssertEquals("note1.NoteText", "Feee-lix the cat, what a wonderful-wonderful cat.", note1.NoteText);
				AssertEquals("note1.NoteContext.Code", "AAA", note1.NoteContext.Code);
				AssertEquals("note1.NoteContext.Description", "Module: A - All, Direction: A - All, Freight: A - All", note1.NoteContext.Description);
				AssertEquals("note1.Visibility.Code", "PUB", note1.Visibility.Code);
				AssertEquals("note1.Visibility.Description", "CLIENT-VISIBLE", note1.Visibility.Description);
			});

			var note2 = shipmentData.NoteCollection[1];
			CombineAssertions(() =>
			{
				AssertEquals("note2.Description", "Internal Work Notes", note2.Description);
				AssertEquals("note2.IsCustomDescription", ZBool.False, note2.IsCustomDescription);
				AssertEquals("note2.NoteText", "Flintstones, meet the Flintstones.", note2.NoteText);
				AssertEquals("note2.NoteContext.Code", "DEB", note2.NoteContext.Code);
				AssertEquals("note2.NoteContext.Description", "Module: D - Customs/Declarations, Direction: E - Export, Freight: B - Air and Sea", note2.NoteContext.Description);
				AssertEquals("note2.Visibility.Code", "INT", note2.Visibility.Code);
				AssertEquals("note2.Visibility.Description", "INTERNAL", note2.Visibility.Description);
			});
		}

		public void AssertClientAddress(QuoteBookingType quoteBookingType)
		{
			var quotedBooking = QuotedBooking.New(quoteBookingType, factory.BOFactory);
			var job = factory.BOFactory.NewJobForTesting<JobHeader>();
			job.Parent = quotedBooking.Quote;
			job.JH_OA_LocalChargesAddr = OrganizationAddressTestHelper.GetOrganizationBO_INTHEMSYD(factory.BOFactory).MainAddress.PK;
			factory.SaveForTesting();

			var shipmentData = getShipmentData(quotedBooking);
			AssertEquals(1, shipmentData.OrganizationAddressCollection.Count);
			OrganizationAddressTestHelper.AssertAddress(shipmentData.OrganizationAddressCollection, 0, "SendersLocalClient", "INTHEMSYD", "In The Moment", false, "Unit 12, Level 3", "233 Here St", "ThereVille", "OfBliss", "1233", "AU", null, "s.m@moment.com.au", "234098234", null, "1239813209");
		}

		public void AssertCarrier(QuoteBookingType quoteBookingType, Action<QuotedBooking> setCarrier)
		{
			var quotedBooking = QuotedBooking.New(quoteBookingType, factory.BOFactory);
			setCarrier(quotedBooking);

			var shipmentData = getShipmentData(quotedBooking);
			AssertNotNull("shipmentData", shipmentData);
			AssertNotNull("shipmentData.OrganizationAddressCollection", shipmentData.OrganizationAddressCollection);
			AssertEquals(1, shipmentData.OrganizationAddressCollection.Count);

			var organizationAddress = shipmentData.OrganizationAddressCollection[0];
			AssertEquals("ShippingLineAddress", organizationAddress.AddressType);
			AssertEquals("INTHEMSYD", organizationAddress.OrganizationCode);
		}

		public void AssertCreditor(QuoteBookingType quoteBookingType, Action<QuotedBooking> setCreditor)
		{
			var quotedBooking = QuotedBooking.New(quoteBookingType, factory.BOFactory);
			setCreditor(quotedBooking);

			var shipmentData = getShipmentData(quotedBooking);
			AssertNotNull("shipmentData", shipmentData);
			AssertNotNull("shipmentData.OrganizationAddressCollection", shipmentData.OrganizationAddressCollection);
			AssertEquals(1, shipmentData.OrganizationAddressCollection.Count);

			var organizationAddress = shipmentData.OrganizationAddressCollection[0];
			AssertEquals("Creditor", organizationAddress.AddressType);
			AssertEquals("INTHEMSYD", organizationAddress.OrganizationCode);
		}

		public void AssertQuoteCharges_ShouldIncludeCostDataIfRecipientTypeIsORP(QuotedBooking quotedBooking, Func<QuotedBooking, RecipientRoleType, Shipment> getShipmentData)
		{
			var client = factory.BOFactory.New<IOrgHeader>();
			client.OH_FullName = "GIMME GIMME";
			client.OH_RL_NKClosestPort = "AUSYD";
			client.OH_IsDebtor = true;
			quotedBooking.ClientPK = client.PK;

			var creator = ObjectFactory.New<IAccountingTestDataCreator>();
			creator.CreateJobHeader(quotedBooking.Quote, quotedBooking.ClientPK);
			creator.AddChargeLineToCreatedJobHeader("FRT", "FAT RICH TRUNKS", 123.45m, "AUD");

			factory.SaveForTesting();

			CombineAssertions(delegate
			{
				var shipmentData = getShipmentData(quotedBooking, RecipientRoleType.ORP);

				AssertNotNull("shipmentData.JobCosting for ORP", shipmentData.JobCosting);

				shipmentData = getShipmentData(quotedBooking, RecipientRoleType.RAG);

				AssertNull("shipmentData.JobCosting for RAG", shipmentData.JobCosting);
			});
		}
	}
}
