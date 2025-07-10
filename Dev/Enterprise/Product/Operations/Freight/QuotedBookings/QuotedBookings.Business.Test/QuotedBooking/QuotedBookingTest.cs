using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.BufferManagement.Integration;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.ContractManagement.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.CarbonEmissions.Business.Testing;
using Enterprise.Freight.CarbonEmissions.Integration;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalCopy.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Freight.Business.Testing.FreightTestHelper;
using static Enterprise.Freight.QuotedBookings.Business.QuotedBooking;
using static Enterprise.Freight.QuotedBookings.Business.QuotedBookingToShipmentConverter;
using static Enterprise.Integration.Customs;
using static Enterprise.Rating.Business.Quote;
using Constants = Enterprise.Core.Constants;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(QuotedBooking))]
	public class QuotedBookingTest : NonPersistentBusinessObjectTestCase
	{
		#region Job Parent

		public void TestJobParent_QuickBooking() => TestJobParent(QuickBooking, expectedJobHeaderParent: QuickBooking);

		public void TestJobParent_OneOffQuote() => TestJobParent(OneOffQuote, expectedJobHeaderParent: OneOffQuote);

		public void TestJobParent_BookingWithQuote() => TestJobParent(BookingWithQuote, expectedJobHeaderParent: BookingWithQuote);

		void TestJobParent(QuotedBooking quotedBooking, IJobHeaderParent expectedJobHeaderParent)
		{
			try
			{
				quotedBooking.TryLoadOrCreateJob();
				var job = quotedBooking.Job;
				AssertEquals("job.Parent", expectedJobHeaderParent, job.Parent);
			}
			finally
			{
				DisposeJobs(quotedBooking);
			}
		}

		QuotedBooking QuickBooking => quickBooking ??= QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
		QuotedBooking quickBooking;

		QuotedBooking OneOffQuote => oneOffQuote ??= QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
		QuotedBooking oneOffQuote;

		QuotedBooking BookingWithQuote => bookingWithQuote ??= QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
		QuotedBooking bookingWithQuote;

		#endregion

		public void TestContainerEmptyWeightPerTEUForQuoteBooking()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			TestQuotedBookingExposer qb = new TestQuotedBookingExposer(quote.PK, booking.PK, Factory);

			var refCont1 = Factory.New<RefContainer>();
			refCont1.RC_TEU = 1;
			refCont1.RC_TareWeight = 2;

			var refCont2 = Factory.New<RefContainer>();
			refCont2.RC_TEU = 2;
			refCont2.RC_TareWeight = 5;

			var container1 = qb.QuotedBookingContainers.AddNew();
			container1.JC_RC = refCont1.PK;
			container1.JC_ContainerCount = 4;

			var container2 = qb.QuotedBookingContainers.AddNew();
			container2.JC_RC = refCont2.PK;
			container2.JC_ContainerCount = 1;

			var ico2Supporter = qb as ICO2eLegBasedSupporter;

			AssertEquals(2.17m, Math.Round((decimal)ico2Supporter.ContainerEmptyWeightPerTEU, 2));
		}

		public void TestContainerEmptyWeightPerTEUForOneOffQuote()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			TestQuotedBookingExposer qb = new TestQuotedBookingExposer(quote.PK, Guid.Empty, Factory);

			var rateOneOffShipment = Factory.New<RateOneOffShipment>();
			rateOneOffShipment.TT_TH = quote.PK;

			var refCont = Factory.New<RefContainer>();
			refCont.RC_TEU = 1;
			refCont.RC_TareWeight = 2;

			var refOneOffCont = quote.CurrentOneOffQuote.Containers.AddNew();
			refOneOffCont.TC_ContainerCount = 1;
			refOneOffCont.TC_TT = rateOneOffShipment.PK;
			refOneOffCont.TC_RC = refCont.PK;

			var ico2Supporter = qb as ICO2eLegBasedSupporter;

			AssertEquals(2m, ico2Supporter.ContainerEmptyWeightPerTEU);
		}

		#region Pre/Post Carriage

		public void TestCO2eCalculationSupporterRequiresPrePostCarriageLegs()
		{
			var booking = CreateNewQuotedBooking(ZGuid.Empty, QuotedBooking.CreateNewBooking(Factory).PK);
			AssertEquals(false, ((ICO2eLegBasedSupporter)booking).RequiresPrePostCarriageLegs);
		}

		public void TestCO2eCalculationSupporterPreCarriageLegs()
		{
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, QuotedBooking.CreateNewBooking(Factory).PK);
			AssertEquals(0, ((ICO2eLegBasedSupporter)quotedBooking).GetPreCarriageLocations(ZString.Empty).Length);
			AssertEquals(0, quotedBooking.GetPreCarriageLegs().ToArray().Length);
		}

		public void TestCO2eCalculationSupporterPostCarriageLegs()
		{
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, QuotedBooking.CreateNewBooking(Factory).PK);
			AssertEquals(0, ((ICO2eLegBasedSupporter)quotedBooking).GetPostCarriageLocations(ZString.Empty).Length);
			AssertEquals(0, quotedBooking.GetPostCarriageLegs().ToArray().Length);
		}

		#endregion

		public void TestOneOffQuote_WhenConvertToBWQ_ThenCompanyTariffLevelOverrideShouldBeCopied()
		{
			var glbTariff = Factory.New<GlobalTariff>();
			var glbTariff2 = Factory.New<GlobalTariff>();
			Factory.Save();
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
			quotedBooking.CompanyTariffLevel = "2";
			Assert("Precondition: Company Tariff Level Override is 2 in One Off Quote", quote.CurrentOneOffQuote.TT_CompanyTariffLevelOverride == 2);
			AssertNull(quotedBooking.Booking);
			quotedBooking.ConvertQuoteToQuotedBooking();
			Assert("Company Tariff Level Override in Booking is 2", quotedBooking.Booking.JS_CompanyTariffLevelOverride == 2);
			Assert("Given One Off Quote's CompanyTariffLevelOverride is 2, when convert it to BWQ, then BWQ's CompanyTariffLevelOverride should be copied from OOQ", quotedBooking.CompanyTariffLevel == "2");
		}

		public void TestOneOffQuote_WhenConvertToBWQ_ThenHBLDeliveryModeShouldBeCopied()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
			quotedBooking.ContainerPackModeOverride = Constants.HBLDeliveryModes.Codes.DOOR_DOOR;

			AssertNull(quotedBooking.Booking);

			quotedBooking.ConvertQuoteToQuotedBooking();

			AssertEquals
				(
					"When convert OOQ to Booking With Quote, HBLDeliveryMode should be copied from OOQ",
					Constants.HBLDeliveryModes.Codes.DOOR_DOOR,
					quotedBooking.Booking.JS_HBLContainerPackModeOverride
				);
		}

		public void TestOneOffQuote_WhenConvertToBWQ_ThenCreditorShouldBeCopied()
		{
			var creditor = Factory.New<OrgHeader>();
			creditor.OH_Code = "ABC";
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
			quotedBooking.Quote.CurrentOneOffQuote.TT_OH_Creditor = creditor.PK;

			Factory.Save();
			AssertNull("Precondition",quotedBooking.Booking);
			quotedBooking.ConvertQuoteToQuotedBooking();
			var booking = Factory.Load<QuotedBooking>(quotedBooking.PK);
			Assert("Given One Off Quote's Creditor is test, when convert it to BWQ, then BWQ's Creditor should be copied from OOQ", quotedBooking.Creditor == creditor.PK);
		}

		public void TestQuotedBooking_ComplianceRiskStatusObject()
		{
			Test("OVR", "CLR", "CLR", "INC");
			Test("OVR", "CLR", "PSK", "CLR");
			Test("PSK", "PSK", "CLR", "PSK");
			Test("CLR", "CLR", "CLR", "NAP");

			void Test(ZString overallRisk, ZString partyRisk, ZString locationRisk, ZString commodityRisk)
			{
				var booking = QuotedBooking.CreateNewBooking(Factory);
				var instance = Factory.New(ObjectFactory.GetType("ComplianceRiskStatus"));
				instance[ComplianceRiskStatusSchema.COR_ParentTableCode] = booking.TablePrefix;
				instance[ComplianceRiskStatusSchema.COR_ParentID] = booking.PK;
				instance[ComplianceRiskStatusSchema.COR_PartyRisk] = partyRisk;
				instance[ComplianceRiskStatusSchema.COR_LocationRisk] = locationRisk;
				instance[ComplianceRiskStatusSchema.COR_OverallRisk] = overallRisk;
				instance[ComplianceRiskStatusSchema.COR_CommodityRisk] = commodityRisk;

				Factory.Save();

				AssertEquals(overallRisk, booking.ComplianceRiskStatus.JobRisk);
				AssertEquals(partyRisk, booking.ComplianceRiskStatus.PartyRisk);
				AssertEquals(locationRisk, booking.ComplianceRiskStatus.LocationRisk);
				AssertEquals(commodityRisk, booking.ComplianceRiskStatus.CommodityRisk);
			}
		}

		public void TestDocumentPrintMode()
		{
			var oneOffQuote = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
			oneOffQuote.Quote.DocumentPrintMode = QuotationDocumentMode.Draft;
			AssertEquals("OneOffQuote", QuotationDocumentMode.Draft, oneOffQuote.DocumentPrintMode);

			var bookingWithQuote = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			bookingWithQuote.Quote.DocumentPrintMode = QuotationDocumentMode.Draft;
			AssertEquals("BookingWithQuote", QuotationDocumentMode.Unknown, bookingWithQuote.DocumentPrintMode);

			var quickBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			AssertEquals("QuickBooking", QuotationDocumentMode.Unknown, quickBooking.DocumentPrintMode);
		}

		public void TestDefaultManuallyCreated()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			AssertEquals(ShipmentStatusList.Codes.Booked, quotedBooking.ShipmentStatus);

			quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			AssertEquals(ShipmentStatusList.Codes.Booked, quotedBooking.ShipmentStatus);
		}

		public void TestQuotedBookingLoggingStrategyAddEDTEventLogs()
		{
			AssertQuotedBookingLoggingStrategyAddEDTEventLogs(QuoteBookingType.QuickBooking);
			AssertQuotedBookingLoggingStrategyAddEDTEventLogs(QuoteBookingType.BookingWithQuote);

			AssertQuotedBookingLoggingStrategyAddEDTEventLogsForCustomFields(QuoteBookingType.QuickBooking);
			AssertQuotedBookingLoggingStrategyAddEDTEventLogsForCustomFields(QuoteBookingType.BookingWithQuote);
		}

		void AssertQuotedBookingLoggingStrategyAddEDTEventLogs(QuoteBookingType quotedBookingType)
		{
			var quotedBooking = QuotedBooking.New(quotedBookingType, Factory);

			AssertEquals(0, quotedBooking.Booking.Logs.GetAllLogs().OfType<StmALog>().Count(log => log.SL_SE_NKEvent == Events.EditedARecordCode));

			quotedBooking.Booking.JS_MarksAndNumbersShort = "O.o";
			Factory.Save();

			AssertEquals(0, quotedBooking.Booking.Logs.GetAllLogs().OfType<StmALog>().Count(log => log.SL_SE_NKEvent == Events.EditedARecordCode));
			Assert("QuotedBooking must have Booking", quotedBooking.ValidateBookingProperty);

			var marksAndNumbersNote = quotedBooking.Notes.GetAllNotes().OfType<StmNote>().FirstOrDefault(note => note.ST_Description == PredefinedNoteTypes.Instance.MarksAndNumbers.Code);

			if (marksAndNumbersNote == null)
			{
				marksAndNumbersNote = quotedBooking.Notes.AddNew();
				marksAndNumbersNote.ST_ParentID = quotedBooking.Booking.PK;
				marksAndNumbersNote.ST_Table = ForwardingShipment.Schema.TableName;
				marksAndNumbersNote.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			}
			marksAndNumbersNote.ST_NoteText = "ST!NOTE!TEXT";
			Factory.Save();

			AssertEquals(1, quotedBooking.Booking.Logs.GetAllLogs().OfType<StmALog>().Count(log => log.SL_SE_NKEvent == Events.EditedARecordCode));

			quotedBooking.Booking.JS_MarksAndNumbersShort = "S!HOR!T";
			Factory.Save();

			AssertEquals(2, quotedBooking.Booking.Logs.GetAllLogs().OfType<StmALog>().Count(log => log.SL_SE_NKEvent == Events.EditedARecordCode));

			quotedBooking.Booking.DocsAndCartage.JP_OrderItemsAsString = "one two";
			Factory.Save();

			AssertEquals(3, quotedBooking.Booking.Logs.GetAllLogs().OfType<StmALog>().Count(log => log.SL_SE_NKEvent == Events.EditedARecordCode));

			var orderItem = quotedBooking.Booking.DocsAndCartage.OrderItems.AddNew();
			orderItem.JT_OrderReference = "ORDINV12345";

			Factory.Save();

			AssertEquals(4, quotedBooking.Booking.Logs.GetAllLogs().OfType<StmALog>().Count(log => log.SL_SE_NKEvent == Events.EditedARecordCode));

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			quotedBooking.ConsignorDocumentaryAddress.OrganisationPK = orgHeader.PK;
			Factory.Save();

			AssertEquals(5, quotedBooking.Booking.Logs.GetAllLogs().OfType<StmALog>().Count(log => log.SL_SE_NKEvent == Events.EditedARecordCode));

			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			quotedBooking.ConsigneeDocumentaryAddress.OrganisationPK = orgHeader.PK;
			Factory.Save();

			AssertEquals(6, quotedBooking.Booking.Logs.GetAllLogs().OfType<StmALog>().Count(log => log.SL_SE_NKEvent == Events.EditedARecordCode));

			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			quotedBooking.Booking.ConsignorPickupAddress.OrganisationPK = orgHeader.PK;
			Factory.Save();

			AssertEquals(7, quotedBooking.Booking.Logs.GetAllLogs().OfType<StmALog>().Count(log => log.SL_SE_NKEvent == Events.EditedARecordCode));

			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			quotedBooking.Booking.ConsigneeDeliveryAddress.OrganisationPK = orgHeader.PK;
			Factory.Save();

			AssertEquals(8, quotedBooking.Booking.Logs.GetAllLogs().OfType<StmALog>().Count(log => log.SL_SE_NKEvent == Events.EditedARecordCode));

			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			quotedBooking.Booking.ControllingCustomerAddress.OrganisationPK = orgHeader.PK;
			Factory.Save();

			AssertEquals(9, quotedBooking.Booking.Logs.GetAllLogs().OfType<StmALog>().Count(log => log.SL_SE_NKEvent == Events.EditedARecordCode));

			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			quotedBooking.Booking.ControllingAgentDocumentaryAddress.OrganisationPK = orgHeader.PK;
			Factory.Save();

			AssertEquals(10, quotedBooking.Booking.Logs.GetAllLogs().OfType<StmALog>().Count(log => log.SL_SE_NKEvent == Events.EditedARecordCode));

			quotedBooking.Booking.HasChanges = true;
			Factory.Save();

			AssertEquals(11, quotedBooking.Booking.Logs.GetAllLogs().OfType<StmALog>().Count(log => log.SL_SE_NKEvent == Events.EditedARecordCode));

			if (quotedBooking.Quote != null)
			{
				quotedBooking.Quote.HasChanges = true;
			}
			Factory.Save();

			AssertEquals(11, quotedBooking.Booking.Logs.GetAllLogs().OfType<StmALog>().Count(log => log.SL_SE_NKEvent == Events.EditedARecordCode));

			quotedBooking.HasChanges = true;
			Factory.Save();

			AssertEquals(11, quotedBooking.Booking.Logs.GetAllLogs().OfType<StmALog>().Count(log => log.SL_SE_NKEvent == Events.EditedARecordCode));

			quotedBooking.Booking.JS_IsForwardRegistered = true;
			Factory.Save();

			AssertEquals("Converted to the shipment - BusinessObjectLoggingStrategy", 12, quotedBooking.Booking.Logs.GetAllLogs().OfType<StmALog>().Count(log => log.SL_SE_NKEvent == Events.EditedARecordCode));

			quotedBooking.Booking.JS_TransportMode = quotedBooking.Booking.JS_TransportMode == Constants.TransportModes.Sea ? Constants.TransportModes.Air : Constants.TransportModes.Sea;
			Factory.Save();

			AssertEquals("Converted to the shipment - BusinessObjectLoggingStrategy", 13, quotedBooking.Booking.Logs.GetAllLogs().OfType<StmALog>().Count(log => log.SL_SE_NKEvent == Events.EditedARecordCode));
		}

		void AssertQuotedBookingLoggingStrategyAddEDTEventLogsForCustomFields(QuoteBookingType quotedBookingType)
		{
			var quotedBooking = QuotedBooking.New(quotedBookingType, Factory);
			AddCustomFieldValue(quotedBooking.PK, ViewQuotedBookingSchema.Constants.Prefix, "custom1", AddOnColumnDataType.Codes.String, "BBB");
			AddCustomFieldValue(quotedBooking.PK, ViewQuotedBookingSchema.Constants.Prefix, "custom2", AddOnColumnDataType.Codes.String, "AAA");

			Factory.Save();

			AssertEquals(0, quotedBooking.Booking.Logs.GetAllLogs().OfType<StmALog>().Count(log => log.SL_SE_NKEvent == Events.EditedARecordCode));

			var newFactory = new BusinessObjectFactory();
			quotedBooking = newFactory.Load<QuotedBooking>(quotedBooking.PK);
			var customBusinessObject = ((ICustomFieldProvider)quotedBooking).GetCustomBusinessObject();
			customBusinessObject["__CUSTOM1__prop__ZString"] = "AAA";
			newFactory.Save();

			AssertEquals(1, quotedBooking.Booking.Logs.GetAllLogs().OfType<StmALog>().Count(log => log.SL_SE_NKEvent == Events.EditedARecordCode));

			customBusinessObject["__CUSTOM2__prop__ZString"] = "CCC";
			newFactory.Save();

			AssertEquals(2, quotedBooking.Booking.Logs.GetAllLogs().OfType<StmALog>().Count(log => log.SL_SE_NKEvent == Events.EditedARecordCode));

			customBusinessObject["__CUSTOM1__prop__ZString"] = ZString.Empty;
			newFactory.Save();

			AssertEquals(3, quotedBooking.Booking.Logs.GetAllLogs().OfType<StmALog>().Count(log => log.SL_SE_NKEvent == Events.EditedARecordCode));
		}

		public void TestQuotedBookingLoggingStrategyDoNotAddMultipleEDTLogs()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var viewQuotedBooking = newFactory.Load<ViewQuotedBooking>(quotedBooking.PK);
			var quotedBooking1 = viewQuotedBooking.QuotedBooking;
			var quotedBooking2 = QuotedBooking.New(viewQuotedBooking.VB_TH, viewQuotedBooking.VB_JS, newFactory);

			AssertEquals("Pre-condition: 2 instances of QuotedBooking with the same PK", 2, ((IBusinessObjectFactoryInternals)newFactory).AllBusinessObjects.Count(bo => bo.PK == quotedBooking.PK && bo is QuotedBooking));

			quotedBooking1.Booking.JS_GoodsDescription = "Dark Sky";
			newFactory.Save();

			AssertEquals("Only log 1 EDT when there're 1+ instances of QuotedBooking with the same PK", 1, quotedBooking1.Booking.Logs.GetAllLogs().OfType<StmALog>().Count(log => log.SL_SE_NKEvent == Events.EditedARecordCode));
		}

		[ExpectNoExceptions]
		public void TestConvertQuoteToBookingDoesNotThrowExceptionForLoadingMetersUnit()
		{
			using (FreightDataRegistry.Instance.InternationalChargeableFactorAir.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ChargeableFactor(
				new ConversionFactor(150, Weight.Kilograms, LoadingLength.LoadingMeters),
				new ConversionFactor(100, Weight.Pounds, Volume.CubicInches))))
			{
				var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
				quote.CurrentOneOffQuote.TT_UnitOfWeight = "HG";
				quote.CurrentOneOffQuote.TT_ActualWeight = 0.055;
				quote.CurrentOneOffQuote.TT_Chargeable = 0.006;
				quote.CurrentOneOffQuote.TT_ActualVolume = 2;

				var quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
				quotedBooking.TransportMode = "AIR";
				quotedBooking.ConvertQuoteToQuotedBooking();
			}
		}

		public void TestLoadAndDischargePortDefaultedFromSailingWhenEmpty()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);

			var sailing = Factory.NewWithValidTestData<JobSailing>();
			var jobVoyage = Factory.NewWithValidTestData<JobVoyage>();

			var origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_JV = jobVoyage.PK;

			var destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = "NZAKL";
			destination.JB_JV = jobVoyage.PK;

			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;

			Factory.Save();

			AssertEquals("Precondition: Load Port is empty", ZString.Empty, quotedBooking.LoadPort);
			AssertEquals("Precondition: Discharge Port is empty", ZString.Empty, quotedBooking.DischargePort);

			((ISailingChooserParent)quotedBooking).SailingJX = sailing.PK;

			AssertEquals("Load Port has been defaulted", "AUSYD", quotedBooking.LoadPort);
			AssertEquals("Discharge Port has been defaulted", "NZAKL", quotedBooking.DischargePort);
		}

		public void TestValidateUniversalCopyPreconditions_TemplateRequiresBooking_NoBookingExists()
		{
			var ucFactory = new UniversalCopyFactory(typeof(QuotedBooking), null);
			var template = ucFactory.GetNewCopyTemplate(null);
			template.PrepareForSave();
			template.Factory.Save();
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, Guid.Empty, Factory);
			var innerNode = template.CopyTemplateTree.CopyTemplateNode.InnerNode as EntityCopyTemplateNode;
			((RelatedEntityCopyTemplateNode)(innerNode.Nodes.Find(node => node.Name == "Booking"))).CopyMethod = RelatedEntityCopyMethod.Copy;
			Factory.Save();
			var validationErrorMessages = quotedBooking.ValidateUniversalCopyPreconditions(template.CopyTemplateTree.CopyTemplateNode);
			var expectedErrorMessage = "This copy template copies only Booking but the One Off Quote is missing a Booking, and thus cannot create a valid copy.";
			AssertEquals("Validation error message thrown: Required booking is missing", expectedErrorMessage, validationErrorMessages);
		}

		public void TestUniversalCopyTemplate_TemplateHasNumbersInOneOffShipmentInQuote()
		{
			var elementType = typeof(QuotedBooking);
			var copyTemplateTree = new CopyTemplateTree(elementType, elementType, BusinessObjectCopyManager.CopyTreeConfiguration);
			var innerNode = copyTemplateTree.InnerNode as EntityCopyTemplateNode;
			var quoteInnerNode = ((RelatedEntityCopyTemplateNode)(innerNode.Nodes.Find(node => node.Name == "Quote"))).InnerNode as EntityCopyTemplateNode;
			var oneOffShipmentInnerNode = ((CollectionCopyTemplateNode)quoteInnerNode.Nodes.Find(node => node.Name == "OneOffShipment")).InnerNode as TemplateCopyTemplateNode;
			var numbers = ((EntityCopyTemplateNode)oneOffShipmentInnerNode.TemplateNode).Nodes.FirstOrDefault(node => node.Name == "Numbers") as CollectionCopyTemplateNode;
			AssertNotNull("Numbers is present in  Universal Copy Template Tree of Quoted Booking", numbers);
		}

		[ExpectNoExceptions]
		public void TestUniversalCopyChargeableWouldNotThrowException()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);

			var quoteNode = new RelatedEntityCopyTemplateNode
			{
				Name = "Quote",
				CopyMethod = RelatedEntityCopyMethod.Copy,
				InnerNode = new EntityCopyTemplateNode
				{
					Name = "RatingHeader"
				}
			};

			var quotedBookingNode = new EntityCopyTemplateNode { Name = "QuotedBooking" };
			quotedBookingNode.Nodes.Add(new PropertyCopyTemplateNode { Name = "Chargeable", CopyMethod = CopyMethod.Copy });
			quotedBookingNode.Nodes.Add(new PropertyCopyTemplateNode { Name = "ClientAddrPK", CopyMethod = CopyMethod.Copy });
			quotedBookingNode.Nodes.Add(new PropertyCopyTemplateNode { Name = "ClientPK", CopyMethod = CopyMethod.Copy });

			quotedBookingNode.Nodes.Add(quoteNode);
			var copyTree = new CopyTemplateTree { Name = "QuotedBooking", InnerNode = quotedBookingNode };

			var bizOCopyManager = new BusinessObjectCopyManager();
			var copiedQuoteBooking = (QuotedBooking)bizOCopyManager.Copy(quotedBooking, copyTree).Object;
			AssertNotNull(copiedQuoteBooking);
		}

		public void TestUniversalCopyCustomFields()
		{
			var quickBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var customValue = Factory.New<GenCustomAddOnValue>();
			customValue.XV_Type = "STR";
			customValue.XV_Name = "Field 1";
			customValue.XV_Data = "Test Value";
			customValue.XV_ParentID = quickBooking.PK;
			customValue.XV_ParentTableCode = quickBooking.TablePrefix;
			Factory.Save();

			var bookingRelatedEntityCopyNode = new RelatedEntityCopyTemplateNode { Name = "Booking", CopyMethod = RelatedEntityCopyMethod.Copy };
			var bookingEntityCopyNode = new EntityCopyTemplateNode { Name = "Booking" };
			bookingEntityCopyNode.Nodes.Add(new PropertyCopyTemplateNode { Name = "JS_IsBooking", CopyMethod = CopyMethod.Copy });
			bookingRelatedEntityCopyNode.InnerNode = bookingEntityCopyNode;

			var customFieldEntityCopyNode = new EntityCopyTemplateNode { Name = "CustomFields" };
			customFieldEntityCopyNode.Nodes.Add(new PropertyCopyTemplateNode { Name = GenCustomAddOnValueSchema.Constants.XV_Name, CopyMethod = CopyMethod.Copy });
			customFieldEntityCopyNode.Nodes.Add(new PropertyCopyTemplateNode { Name = GenCustomAddOnValueSchema.Constants.XV_Type, CopyMethod = CopyMethod.Copy });
			customFieldEntityCopyNode.Nodes.Add(new PropertyCopyTemplateNode { Name = GenCustomAddOnValueSchema.Constants.XV_Data, CopyMethod = CopyMethod.Copy });
			customFieldEntityCopyNode.Nodes.Add(new PropertyCopyTemplateNode { Name = GenCustomAddOnValueSchema.Constants.XV_ParentTableCode, CopyMethod = CopyMethod.Copy });
			var customFieldsCollectionCopyNode = new CollectionCopyTemplateNode
			{
				Name = "CustomFields",
				ItemPropertyName = GenCustomAddOnValueSchema.Constants.XV_ParentID,
				ItemsTableName = GenCustomAddOnValueSchema.Constants.TableName,
				InnerNode = customFieldEntityCopyNode,
				CopyMethod = CollectionCopyMethod.All
			};

			var quotedBookingNode = new EntityCopyTemplateNode { Name = "QuotedBooking" };
			quotedBookingNode.Nodes.Add(bookingRelatedEntityCopyNode);
			quotedBookingNode.Nodes.Add(customFieldsCollectionCopyNode);

			var copyTree = new CopyTemplateTree { Name = "QuotedBooking", InnerNode = quotedBookingNode };
			var bizOCopyManager = new BusinessObjectCopyManager();
			var copiedQuoteBooking = (QuotedBooking)bizOCopyManager.Copy(quickBooking, copyTree).Object;
			AssertNotNull(copiedQuoteBooking);
			copiedQuoteBooking.Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadedCopiedQuoteBooking = newFactory.Load<QuotedBooking>(copiedQuoteBooking.Booking.PK);
			var copiedCustomField = reloadedCopiedQuoteBooking.GetUserDefinedProperty("Field 1", "STR");
			AssertNotNull(copiedCustomField);
			AssertEquals("Test Value", copiedCustomField.XV_Data);
		}

		public void TestUniversalCopy_QB_CarrierServiceLevel()
		{
			var carrier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsShippingProvider, true));
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			quotedBooking.CreateNewBookingForUniversalCopy();
			quotedBooking.OH_Carrier = carrier.PK;
			quotedBooking.CarrierServiceLevel = "CSL";
			Factory.Save();

			var bookingNode = new RelatedEntityCopyTemplateNode
			{
				Name = "Booking",
				CopyMethod = RelatedEntityCopyMethod.Copy,
				InnerNode = new EntityCopyTemplateNode
				{
					Name = "Booking"
				}
			};

			var quotedBookingNode = new EntityCopyTemplateNode { Name = "QuotedBooking" };
			quotedBookingNode.Nodes.Add(new PropertyCopyTemplateNode { Name = "CarrierServiceLevel", CopyMethod = CopyMethod.Copy });
			quotedBookingNode.Nodes.Add(new PropertyCopyTemplateNode { Name = "OH_Carrier", CopyMethod = CopyMethod.Copy });
			quotedBookingNode.Nodes.Add(bookingNode);

			var copyTree = new CopyTemplateTree { Name = "QuotedBooking", InnerNode = quotedBookingNode };

			var bizOCopyManager = new BusinessObjectCopyManager();
			var copiedQuoteBooking = (QuotedBooking)bizOCopyManager.Copy(quotedBooking, copyTree).Object;

			AssertEquals(carrier.PK, copiedQuoteBooking.OH_Carrier);
			AssertEquals("CSL", copiedQuoteBooking.CarrierServiceLevel);
		}

		public void TestUniversalCopy_OOQ_CarrierServiceLevel()
		{
			var carrier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsShippingProvider, true));
			var quotedBooking = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
			quotedBooking.CreateNewQuoteForUniversalCopy();
			quotedBooking.OH_Carrier = carrier.PK;
			quotedBooking.CarrierServiceLevel = "CSL";
			Factory.Save();

			var quoteNode = new RelatedEntityCopyTemplateNode
			{
				Name = "Quote",
				CopyMethod = RelatedEntityCopyMethod.Copy,
				InnerNode = new EntityCopyTemplateNode
				{
					Name = "RatingHeader"
				}
			};

			var quotedBookingNode = new EntityCopyTemplateNode { Name = "QuotedBooking" };
			quotedBookingNode.Nodes.Add(new PropertyCopyTemplateNode { Name = "CarrierServiceLevel", CopyMethod = CopyMethod.Copy });
			quotedBookingNode.Nodes.Add(new PropertyCopyTemplateNode { Name = "OH_Carrier", CopyMethod = CopyMethod.Copy });
			quotedBookingNode.Nodes.Add(quoteNode);

			var copyTree = new CopyTemplateTree { Name = "QuotedBooking", InnerNode = quotedBookingNode };

			var bizOCopyManager = new BusinessObjectCopyManager();
			var copiedQuoteBooking = (QuotedBooking)bizOCopyManager.Copy(quotedBooking, copyTree).Object;

			AssertEquals(carrier.PK, copiedQuoteBooking.OH_Carrier);
			AssertEquals("CSL", copiedQuoteBooking.CarrierServiceLevel);
		}

		public void TestUniversalCopy_Mode()
		{
			var quickBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			quickBooking.Mode = ContainerModes.FCL;
			Factory.Save();

			var bookingRelatedEntityCopyNode = new RelatedEntityCopyTemplateNode { Name = "Booking", CopyMethod = RelatedEntityCopyMethod.Copy };
			var bookingEntityCopyNode = new EntityCopyTemplateNode { Name = "Booking" };
			bookingRelatedEntityCopyNode.InnerNode = bookingEntityCopyNode;

			var quotedBookingNode = new EntityCopyTemplateNode { Name = "QuotedBooking" };
			quotedBookingNode.Nodes.Add(new PropertyCopyTemplateNode { Name = "OneOffQuoteContainerMode", CopyMethod = CopyMethod.Copy });
			quotedBookingNode.Nodes.Add(new PropertyCopyTemplateNode { Name = "Mode", CopyMethod = CopyMethod.Copy });
			quotedBookingNode.Nodes.Add(bookingRelatedEntityCopyNode);

			var copyTree = new CopyTemplateTree { Name = "QuotedBooking", InnerNode = quotedBookingNode };

			var bizOCopyManager = new BusinessObjectCopyManager();
			var copiedQuickBooking = (QuotedBooking)bizOCopyManager.Copy(quickBooking, copyTree).Object;
			AssertEquals(ContainerModes.FCL, copiedQuickBooking.Mode);
		}

		[ExpectNoExceptions]
		public void TestUniversalCopy_CO2ePerTonneInKg_WouldNotThrowException()
		{
			var quickBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			Factory.Save();

			var bookingRelatedEntityCopyNode = new RelatedEntityCopyTemplateNode { Name = "Booking", CopyMethod = RelatedEntityCopyMethod.Copy };
			var bookingEntityCopyNode = new EntityCopyTemplateNode { Name = "Booking" };
			bookingRelatedEntityCopyNode.InnerNode = bookingEntityCopyNode;

			var quotedBookingNode = new EntityCopyTemplateNode { Name = "QuotedBooking" };
			quotedBookingNode.Nodes.Add(new PropertyCopyTemplateNode { Name = "CO2ePerTonneInKg", CopyMethod = CopyMethod.Copy });
			quotedBookingNode.Nodes.Add(bookingRelatedEntityCopyNode);

			var copyTree = new CopyTemplateTree { Name = "QuotedBooking", InnerNode = quotedBookingNode };

			var bizOCopyManager = new BusinessObjectCopyManager();
			var copiedQuickBooking = (QuotedBooking)bizOCopyManager.Copy(quickBooking, copyTree).Object;
			AssertEquals(0m, copiedQuickBooking.GetCO2ePerTonneInKg());
		}

		public void TestLoadAndDischargePortNotDefaultedFromSailingWhenPopulated()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);

			var sailing = Factory.NewWithValidTestData<JobSailing>();
			var jobVoyage = Factory.NewWithValidTestData<JobVoyage>();

			var origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_JV = jobVoyage.PK;

			var destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = "NZAKL";
			destination.JB_JV = jobVoyage.PK;

			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;

			quotedBooking.LoadPort = "USJFK";
			quotedBooking.DischargePort = "USLAX";

			Factory.Save();

			AssertNotEquals("Precondition: Load Port is not empty", ZString.Empty, quotedBooking.LoadPort);
			AssertNotEquals("Precondition: Discharge Port is not empty", ZString.Empty, quotedBooking.DischargePort);

			((ISailingChooserParent)quotedBooking).SailingJX = sailing.PK;

			AssertEquals("Load Port has not been defaulted", "USJFK", quotedBooking.LoadPort);
			AssertEquals("Discharge Port has not been defaulted", "USLAX", quotedBooking.DischargePort);
		}

		public void TestETDAndETADefaultedFromSailingWhenEmpty()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);

			var sailing = Factory.NewWithValidTestData<JobSailing>();
			var jobVoyage = Factory.NewWithValidTestData<JobVoyage>();

			var etd = new ZDateTime(2019, 05, 01);
			var eta = new ZDateTime(2019, 05, 07);

			var origin = Factory.New<VoyageOrigin>();
			origin.JA_E_DEP = etd;
			origin.JA_JV = jobVoyage.PK;

			var destination = Factory.New<VoyageDestination>();
			destination.JB_E_ARV = eta;
			destination.JB_JV = jobVoyage.PK;

			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;

			Factory.Save();

			AssertEquals("Precondition: ETD is empty", ZDateTime.Empty, quotedBooking.ETD);
			AssertEquals("Precondition: ETA is empty", ZDateTime.Empty, quotedBooking.ETA);

			((ISailingChooserParent)quotedBooking).SailingJX = sailing.PK;

			AssertEquals("ETD has been defaulted", etd, quotedBooking.ETD);
			AssertEquals("ETA has been defaulted", eta, quotedBooking.ETA);
		}

		public void TestETDAndETANotDefaultedFromSailingWhenPopulated()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);

			var sailing = Factory.NewWithValidTestData<JobSailing>();
			var jobVoyage = Factory.NewWithValidTestData<JobVoyage>();

			var origin = Factory.New<VoyageOrigin>();
			origin.JA_E_DEP = new ZDateTime(2019, 05, 01);
			origin.JA_JV = jobVoyage.PK;

			var destination = Factory.New<VoyageDestination>();
			destination.JB_E_ARV = new ZDateTime(2019, 05, 07);
			destination.JB_JV = jobVoyage.PK;

			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;

			var etd = new ZDateTime(2019, 06, 07);
			var eta = new ZDateTime(2019, 06, 14);

			quotedBooking.ETD = etd;
			quotedBooking.ETA = eta;

			Factory.Save();

			AssertNotEquals("Precondition: ETD is not empty", ZDateTime.Empty, quotedBooking.ETD);
			AssertNotEquals("Precondition: ETA is not empty", ZDateTime.Empty, quotedBooking.ETA);

			((ISailingChooserParent)quotedBooking).SailingJX = sailing.PK;

			AssertEquals("ETD has not been defaulted", etd, quotedBooking.ETD);
			AssertEquals("ETA has not been defaulted", eta, quotedBooking.ETA);
		}

		public void TestTemplateSaveAndLoad()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			quotedBooking.Booking.JS_TransportMode = "AIR";

			quotedBooking.OH_Carrier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsShippingProvider, true)).PK;
			Factory.Save();

			var templateRecord = Factory.New<StmTemplateRecord>();
			var templateRecordProvider1 = quotedBooking as ITemplateRecordProvider;
			templateRecordProvider1.TemplateRecord = templateRecord;

			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager()) // Fountain GetNext() requires a transaction
			{
				templateRecordProvider1.SaveToTemplateRecord();
			}

			var quotedBooking2 = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var templateRecordProvider2 = quotedBooking2 as ITemplateRecordProvider;

			AssertNotEquals(quotedBooking.Booking.JS_TransportMode, quotedBooking2.Booking.JS_TransportMode);

			templateRecordProvider2.LoadFromTemplateRecord(templateRecord);

			AssertEquals(quotedBooking.Booking.JS_TransportMode, quotedBooking2.Booking.JS_TransportMode);
			AssertEquals(quotedBooking.OH_Carrier, quotedBooking2.OH_Carrier);
			Assert(quotedBooking2.Booking.IsTemplate);
		}

		public void TestLoadTemplateQuotedBookingByUsingOrganizationCodeAndAddressShortCodeInNewEngine()
		{
			var importBroker = Factory.NewWithValidTestData<OrgHeader>();
			importBroker.OH_Code = "ImportBroker";

			var quotedBooking1 = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			quotedBooking1.Booking.JS_TransportMode = "AIR";
			quotedBooking1.Booking.JS_OH_ImportBroker = importBroker.PK;

			var templateRecord = Factory.New<StmTemplateRecord>();
			var templateRecordProvider1 = quotedBooking1 as ITemplateRecordProvider;
			templateRecordProvider1.TemplateRecord = templateRecord;

			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager()) // Fountain GetNext() requires a transaction
			{
				templateRecordProvider1.SaveToTemplateRecord();
			}

			var quotedBooking2 = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var templateRecordProvider2 = quotedBooking2 as ITemplateRecordProvider;
			AssertEquals(Guid.Empty, quotedBooking2.Booking.JS_OH_ImportBroker);

			using (OrganisationsDataRegistry.Instance.OrgMatchUseDeduplication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				templateRecordProvider2.LoadFromTemplateRecord(templateRecord);
			}

			AssertEquals(importBroker.PK, quotedBooking2.Booking.JS_OH_ImportBroker);
		}

		public void TestPKSchemaColumn()
		{
			var quotedBooking = new QuotedBooking(Factory);
			AssertEquals(ViewQuotedBookingSchema.PK.TableName, quotedBooking.PKSchemaColumn.TableName);
		}

		public void TestLoadByQuoteOrBookingPK()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var loadedByQuotePK = factory2.Load<IQuotedBooking>(quotedBooking.Quote.PK);

			var factory3 = new BusinessObjectFactory();
			var loadedByBookingPK = factory3.Load<IQuotedBooking>(quotedBooking.Booking.PK);

			AssertEquals(loadedByBookingPK.Quote.PK, loadedByQuotePK.Quote.PK);
			AssertEquals(loadedByBookingPK.ForwardingShipment.PK, loadedByQuotePK.ForwardingShipment.PK);
		}

		public void TestJS_RL_NKOriginInfoInfo_ValueChanged_ForWeb()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			var quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
			quotedBooking.ConvertQuoteToQuotedBooking();

			Globals.IsWeb = true;
			quotedBooking.Booking.JS_RL_NKOrigin = "USORD";
			AssertEquals("USORD", quotedBooking.LoadPort);

			quotedBooking.Booking.JS_RL_NKOrigin = "AUSYD";
			AssertEquals("AUSYD", quotedBooking.LoadPort);

			quotedBooking.Booking.JS_RL_NKOrigin = ZString.Empty;
			AssertEquals("AUSYD", quotedBooking.LoadPort);

			using (new DisposableAction(() => quotedBooking.DisableLoadDischargeDefaulting(), () => quotedBooking.EnableLoadDischargeDefaulting()))
			{
				quotedBooking.Booking.JS_RL_NKOrigin = "USORD";
				AssertEquals("AUSYD", quotedBooking.LoadPort);
			}

			quotedBooking.Booking.JS_RL_NKOrigin = "CNSHA";
			AssertEquals("CNSHA", quotedBooking.LoadPort);
		}

		public void TestJS_RL_NKDestinationInfo_ValueChanged_ForWeb()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			var quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
			quotedBooking.ConvertQuoteToQuotedBooking();

			Globals.IsWeb = true;
			quotedBooking.Booking.JS_RL_NKDestination = "USORD";
			AssertEquals("USORD", quotedBooking.DischargePort);

			quotedBooking.Booking.JS_RL_NKDestination = "AUSYD";
			AssertEquals("AUSYD", quotedBooking.DischargePort);

			quotedBooking.Booking.JS_RL_NKDestination = ZString.Empty;
			AssertEquals("AUSYD", quotedBooking.DischargePort);

			using (new DisposableAction(() => quotedBooking.DisableLoadDischargeDefaulting(), () => quotedBooking.EnableLoadDischargeDefaulting()))
			{
				quotedBooking.Booking.JS_RL_NKDestination = "USORD";
				AssertEquals("AUSYD", quotedBooking.DischargePort);
			}

			quotedBooking.Booking.JS_RL_NKDestination = "CNSHA";
			AssertEquals("CNSHA", quotedBooking.DischargePort);
		}

		public void TestGetControllingCustomerSecurityCheckPoint()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			var quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
			AssertNull(quotedBooking.GetControllingCustomerSecurityCheckPoint());

			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_TransportMode = Constants.TransportModes.Air;
			quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);
			AssertEquals(Env.Security.QuickBookingAllowSaveWithoutControllingCustomerAir.Code, quotedBooking.GetControllingCustomerSecurityCheckPoint().Code);

			booking.JS_TransportMode = Constants.TransportModes.Sea;
			AssertEquals(Env.Security.QuickBookingAllowSaveWithoutControllingCustomerSea.Code, quotedBooking.GetControllingCustomerSecurityCheckPoint().Code);

			booking.JS_TransportMode = Constants.TransportModes.Road;
			AssertEquals(Env.Security.QuickBookingAllowSaveWithoutControllingCustomerRoad.Code, quotedBooking.GetControllingCustomerSecurityCheckPoint().Code);

			booking.JS_TransportMode = Constants.TransportModes.Rail;
			AssertEquals(Env.Security.QuickBookingAllowSaveWithoutControllingCustomerRail.Code, quotedBooking.GetControllingCustomerSecurityCheckPoint().Code);

			booking.JS_TransportMode = Constants.TransportModes.Courier;
			AssertEquals(Env.Security.QuickBookingAllowSaveWithoutControllingCustomer.Code, quotedBooking.GetControllingCustomerSecurityCheckPoint().Code);
		}

		public void TestGetControllingAgentSecurityCheckPoint()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			var quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
			AssertNull(quotedBooking.GetControllingAgentSecurityCheckPoint());

			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_TransportMode = Constants.TransportModes.Air;
			quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);
			AssertEquals(Env.Security.QuickBookingAllowSaveWithoutControllingAgentAir.Code, quotedBooking.GetControllingAgentSecurityCheckPoint().Code);

			booking.JS_TransportMode = Constants.TransportModes.Sea;
			AssertEquals(Env.Security.QuickBookingAllowSaveWithoutControllingAgentSea.Code, quotedBooking.GetControllingAgentSecurityCheckPoint().Code);

			booking.JS_TransportMode = Constants.TransportModes.Road;
			AssertEquals(Env.Security.QuickBookingAllowSaveWithoutControllingAgentRoad.Code, quotedBooking.GetControllingAgentSecurityCheckPoint().Code);

			booking.JS_TransportMode = Constants.TransportModes.Rail;
			AssertEquals(Env.Security.QuickBookingAllowSaveWithoutControllingAgentRail.Code, quotedBooking.GetControllingAgentSecurityCheckPoint().Code);

			booking.JS_TransportMode = Constants.TransportModes.Courier;
			AssertEquals(Env.Security.QuickBookingAllowSaveWithoutControllingAgent.Code, quotedBooking.GetControllingAgentSecurityCheckPoint().Code);
		}

		public void TestQuotedBookingWithRelatedNotes_ControllingCustomer()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);

			var controllingCustomer = Factory.New<OrgHeader>();

			FreightDataRegistry.Instance.DefaultShipmentControllingCustomer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			booking.DefaultControllingCustomer(controllingCustomer, forceDefaulting: false);

			StmNote testNote = controllingCustomer.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Break everything");

			AssertEquals(true, quotedBooking.Notes.VisibleNotes.Contains(testNote));
		}

		public void TestReceiverList()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);

			quotedBooking.Mode = Core.Constants.RateMode.LSE;
			AssertType("Booking mode LSE should correspond with 'PackDepotCollection' receiver type", typeof(PackDepotCollection), quotedBooking.Receiver_List);

			quotedBooking.Mode = Core.Constants.RateMode.ULD;
			AssertType("Booking mode ULD should correspond with 'PackDepotCollection' receiver type", typeof(PackDepotCollection), quotedBooking.Receiver_List);

			quotedBooking.Mode = Core.Constants.RateMode.LCL;
			AssertType("Booking mode LCL should correspond with 'PackDepotCollection' receiver type", typeof(PackDepotCollection), quotedBooking.Receiver_List);

			quotedBooking.Mode = Core.Constants.RateMode.FCL;
			AssertType("Booking mode FCL should correspond with 'SeaCTOCollection' receiver type", typeof(SeaCTOCollection), quotedBooking.Receiver_List);

			quotedBooking.Mode = Core.Constants.RateMode.LRO;
			AssertType("Booking mode LRO should correspond with 'PackDepotCollection' receiver type", typeof(PackDepotCollection), quotedBooking.Receiver_List);

			quotedBooking.Mode = Core.Constants.RateMode.FRO;
			AssertType("Booking mode FRO should correspond with 'CTOCollection' receiver type", typeof(CTOCollection), quotedBooking.Receiver_List);

			quotedBooking.Mode = Core.Constants.RateMode.FTL;
			AssertType("Booking mode FTL should correspond with 'PackDepotCollection' receiver type", typeof(PackDepotCollection), quotedBooking.Receiver_List);

			quotedBooking.Mode = Core.Constants.RateMode.COU;
			AssertType("Booking mode COU should correspond with 'CTOCollection' receiver type", typeof(CTOCollection), quotedBooking.Receiver_List);

			quotedBooking.Mode = Core.Constants.RateMode.LRA;
			AssertType("Booking mode LRA should correspond with 'PackDepotCollection' receiver type", typeof(PackDepotCollection), quotedBooking.Receiver_List);

			quotedBooking.Mode = Core.Constants.RateMode.FRA;
			AssertType("Booking mode FRA should correspond with 'CTOCollection' receiver type", typeof(CTOCollection), quotedBooking.Receiver_List);
		}

		public void TestReceiverValidation()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);

			var sea_cto = Factory.New<OrgHeader>();
			sea_cto.OH_IsSeaCTO = true;
			sea_cto.OH_IsMiscFreightServices = true;

			var air_cto = Factory.New<OrgHeader>();
			air_cto.OH_IsAirCTO = true;
			air_cto.OH_IsMiscFreightServices = true;

			var depot = Factory.New<OrgHeader>();
			depot.OH_IsPackDepot = true;
			depot.OH_IsMiscFreightServices = true;

			quotedBooking.Mode = Core.Constants.RateMode.FCL;
			quotedBooking.ExportReceivingDepot = sea_cto.MainAddress.PK;

			AssertNoErrors("FCL booking mode must correspond with a Sea CTO receiver type", quotedBooking.ExportReceivingDepotInfo);
			quotedBooking.Mode = Core.Constants.RateMode.LSE;
			AssertHasErrors("LSE booking mode must correspond with a packing depot receiver type", quotedBooking.ExportReceivingDepotInfo);
			quotedBooking.ExportReceivingDepot = depot.MainAddress.PK;
			AssertNoErrors("LSE booking mode must correspond with a packing depot receiver type", quotedBooking.ExportReceivingDepotInfo);

			quotedBooking.Mode = Core.Constants.RateMode.FRO;
			quotedBooking.ExportReceivingDepot = air_cto.MainAddress.PK;

			AssertNoErrors("FRO booking mode must correspond with a CTO (air or sea) receiver type", quotedBooking.ExportReceivingDepotInfo);
			quotedBooking.Mode = Core.Constants.RateMode.LSE;
			AssertHasErrors("LSE booking mode must correspond with a packing depot receiver type", quotedBooking.ExportReceivingDepotInfo);
			quotedBooking.ExportReceivingDepot = depot.MainAddress.PK;
			AssertNoErrors("LSE booking mode must correspond with a packing depot receiver type", quotedBooking.ExportReceivingDepotInfo);

			quotedBooking.Mode = Core.Constants.RateMode.LCL;
			quotedBooking.ExportReceivingDepot = depot.MainAddress.PK;

			AssertNoErrors("LCL booking mode must correspond with a packing depot receiver type", quotedBooking.ExportReceivingDepotInfo);
			quotedBooking.Mode = Core.Constants.RateMode.FCL;
			AssertHasErrors("FCL booking mode must correspond with a Sea CTO receiver type", quotedBooking.ExportReceivingDepotInfo);
			quotedBooking.ExportReceivingDepot = sea_cto.MainAddress.PK;
			AssertNoErrors("FCL booking mode must correspond with a Sea CTO receiver type", quotedBooking.ExportReceivingDepotInfo);
		}

		public void TestDeliveryList()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);

			quotedBooking.Mode = Core.Constants.RateMode.LSE;
			AssertType("Booking mode LSE should correspond with 'UnpackDepotCollection' delivery type", typeof(UnpackDepotCollection), quotedBooking.Delivery_List);

			quotedBooking.Mode = Core.Constants.RateMode.ULD;
			AssertType("Booking mode ULD should correspond with 'UnpackDepotCollection' delivery type", typeof(UnpackDepotCollection), quotedBooking.Delivery_List);

			quotedBooking.Mode = Core.Constants.RateMode.LCL;
			AssertType("Booking mode LCL should correspond with 'UnpackDepotCollection' delivery type", typeof(UnpackDepotCollection), quotedBooking.Delivery_List);

			quotedBooking.Mode = Core.Constants.RateMode.FCL;
			AssertType("Booking mode FCL should correspond with 'CTOCollection' delivery type", typeof(CTOCollection), quotedBooking.Delivery_List);

			quotedBooking.Mode = Core.Constants.RateMode.LRO;
			AssertType("Booking mode LRO should correspond with 'UnpackDepotCollection' delivery type", typeof(UnpackDepotCollection), quotedBooking.Delivery_List);

			quotedBooking.Mode = Core.Constants.RateMode.FRO;
			AssertType("Booking mode FRO should correspond with 'CTOCollection' delivery type", typeof(CTOCollection), quotedBooking.Delivery_List);

			quotedBooking.Mode = Core.Constants.RateMode.FTL;
			AssertType("Booking mode FTL should correspond with 'UnpackDepotCollection' delivery type", typeof(UnpackDepotCollection), quotedBooking.Delivery_List);

			quotedBooking.Mode = Core.Constants.RateMode.COU;
			AssertType("Booking mode COU should correspond with 'CTOCollection' delivery type", typeof(CTOCollection), quotedBooking.Delivery_List);

			quotedBooking.Mode = Core.Constants.RateMode.LRA;
			AssertType("Booking mode LRA should correspond with 'UnpackDepotCollection' delivery type", typeof(UnpackDepotCollection), quotedBooking.Delivery_List);

			quotedBooking.Mode = Core.Constants.RateMode.FRA;
			AssertType("Booking mode FRA should correspond with 'CTOCollection' delivery type", typeof(CTOCollection), quotedBooking.Delivery_List);
		}

		public void TestEventMasterMatchesQuotedBookingNotForwardingShipment()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);
			var log = quotedBooking.Logs.AddNew(Events.Authorised);

			AssertEquals("booking (JobShipment) and quotedBooking (ViewQuotedBooking) have the same PK", booking.PK, quotedBooking.PK);

			Factory.Save();
			ReleaseFactory();

			var booking2 = Factory.Load<ForwardingShipment>(booking.PK);
			var log2 = Factory.LoadTop1<StmALog>(new ZQuery(StmALogSchema.SL_Parent, booking.PK).AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.AuthorisedCode));

			AssertNotEquals("Should not match log Master by PK if table is different", booking2, log2.Master);
			AssertType("Master should be loaded as QuotedBooking (or client specific alternative)", GetExpectedBusinessObjectType(), log2.Master);
		}

		public void TestUnacceptedBookingWithQuoteState_SettingProperty_FallbackToQuoteWhenBookingHasNoProperty()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(quote.PK, booking.PK);
			AssertEquals("prerequisite", QuotedBookingState.UnacceptedBookingWithQuote, quotedBooking.ObjectState);

			quotedBooking.Via = "AUSYD";
			AssertEquals("AUSYD", quotedBooking.Via);

			quotedBooking.Frequency = 13;
			AssertEquals(13, quotedBooking.Frequency);
		}

		public void TestBookingWithQuote_MilestoneActualDateIsSetWhenIRPEventFires()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			var quoteMilestone = (QuotedBookingProcessTask)quotedBooking.WorkflowItems.Milestones.AddNew();
			quoteMilestone.TriggerConditions.TriggerEventCode = Events.InterimReceiptProducedCode;

			Factory.Save();

			DateTime receiveDate = DateTime.Today.AddDays(1);
			quotedBooking.Booking.JS_A_RCV = receiveDate;
			AssertEquals(receiveDate, quoteMilestone.P9_ActualDate.ToZDateTime());
		}

		public void TestGetHumanReadableName()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			object humanReadableName = quotedBooking["HumanReadableName"];
			AssertNotNull(humanReadableName);
		}

		#region ISendEmailSource Tests

		public void TestISendEmailSourceMembers_BookingWithQuote()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var contact = consignee.Contacts.AddNew();
			contact.OC_ContactName = "Connie Contact";

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			shipment.ConsigneePK = consignee.PK;
			var quotedBooking = CreateNewQuotedBooking(quote.PK, shipment.PK);
			Factory.Save();

			AssertISendEmailSourceMembers(quotedBooking, contact, "Shipment - S00001000", "Enterprise.DocumentWrappers.DocForwardingShipment");
		}

		public void TestISendEmailSourceMembers_QuickBooking()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var contact = consignee.Contacts.AddNew();
			contact.OC_ContactName = "Connie Contact";

			var shipment = QuotedBooking.CreateNewBooking(Factory);
			shipment.ConsigneePK = consignee.PK;
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);
			Factory.Save();

			AssertISendEmailSourceMembers(quotedBooking, contact, "Shipment - S00001000", "Enterprise.DocumentWrappers.DocForwardingShipment");
		}

		public void TestISendEmailSourceMembers_SpotQuote()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var contact = consignee.Contacts.AddNew();
			contact.OC_ContactName = "Connie Contact";

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
			quotedBooking.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			Factory.Save();

			AssertISendEmailSourceMembers(quotedBooking, contact, "Quotation - 00001000", null);
		}

		void AssertISendEmailSourceMembers(ISendEmailSource emailSource, OrgContact expectedContact, string expectedEmailSubject, string expectedDocWrapperType)
		{
			CombineAssertions(() =>
			{
				AssertNotNull("GetAddressBookSelection", emailSource.GetAddressBookSelection());
				Assert("GetAddressBookSelection contains contact", emailSource.GetAddressBookSelection().Recipients.Contains(expectedContact));

				AssertEquals("EmailSubject", expectedEmailSubject, emailSource.EmailSubject);
				AssertEquals("TemplateCategory", MailTemplateCategoryList.Codes.Bookings, emailSource.TemplateCategory);
				AssertEquals("DefaultFromDisplayName", Env.CurrentUser.FullName, emailSource.DefaultFromDisplayName);
				AssertEquals("OverridingDefaultFromEmailAddress", null, emailSource.OverridingDefaultFromEmailAddress);

				if (string.IsNullOrEmpty(expectedDocWrapperType))
				{
					AssertNull("DocWrapperType", emailSource.DocWrapperType);
				}
				else
				{
					AssertEquals("DocWrapperType", expectedDocWrapperType, emailSource.DocWrapperType.ToString());
				}

				AssertNotNull("Logs", emailSource.Logs);
			});
		}

		#endregion

		public void TestISupportsDirectSailing()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			AssertNotNull(quotedBooking);
		}

		public void TestClientAddrNotLostAfterSetting()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();

			var anotherAddress = Factory.NewWithValidTestData<OrgAddress>();
			anotherAddress.OA_OH = organization.PK;

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			quotedBooking.ClientAddrPK = anotherAddress.PK;

			AssertEquals(anotherAddress.PK, quotedBooking.Quote.QuotationClientAddress.E2_OA_Address);
			AssertEquals(organization.PK, quotedBooking.Quote.TH_OH);
		}

		public void TestClientIsSetupCorrectlyWhenCopyingFromQuotedBooking()
		{
			var organization1 = Factory.New<OrgHeader>();
			organization1.OH_Code = "AAAA";

			var organization2 = Factory.New<OrgHeader>();
			organization2.OH_Code = "BBBB";

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			quote.QuotationClientAddress.OrganisationPK = organization1.PK;
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);

			quotedBooking.ConvertQuoteToQuotedBooking();

			JobHeader quotedBookingJob = new JobHeader.Loader(quotedBooking).TryLoadOrCreate();
			quotedBookingJob.JH_OA_LocalChargesAddr = organization2.MainAddress.PK;

			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			var shipmentJob = new JobHeader.Loader(shipment).TryLoadOrCreate();

			shipmentJob.JH_TH_NKQuoteNumber = quotedBooking.QuotedBookingNumber;

			AssertEquals(organization2.PK, shipmentJob.LocalChargesAddr.Header.PK);
		}

		public void TestImportTradeDetail()
		{
			var buyer = Factory.New<OrgHeader>();
			var supplier = Factory.New<OrgHeader>();
			var carrier = Factory.New<OrgHeader>();

			var org = Factory.New<OrgHeader>();
			var sales = org.SalesCollection.AddNew();
			sales.OW_OH_Buyer = buyer.PK;
			sales.OW_OH_Supplier = supplier.PK;
			sales.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", RefUNLOCOSchema.Constants.Prefix).PK;
			sales.OW_DestinationID = ViewLocationHelper.GetLocationFromString(Factory, "AUMEL", RefUNLOCOSchema.Constants.Prefix).PK;
			var tradeDetail = sales.TradeDetails.AddNew();
			tradeDetail.PA_TradeMode = "SEA";
			tradeDetail.PA_TradeType = "FCL";
			tradeDetail.ProspectDetail.PAP_RS_NKServiceLevel = "SRV";
			tradeDetail.CurrentProspectPeriod.PAS_Weight = 100;
			tradeDetail.CurrentProspectPeriod.PAS_WeightUQ = "KG";
			tradeDetail.CurrentProspectPeriod.PAS_Volume = 10;
			tradeDetail.CurrentProspectPeriod.PAS_VolumeUQ = "M3";
			tradeDetail.ProspectDetail.PAP_IncoTradeTerm = "INC";
			tradeDetail.ProspectDetail.PAP_RH_NKCommodityCode = "COM";
			tradeDetail.ProspectDetail.PAP_OH_ServiceProvider = carrier.PK;

			var container = Factory.New<RefContainer>();
			container.RC_Code = "RC1";
			tradeDetail.ProspectDetail.PAP_RC_NKContainer = "RC1";
			tradeDetail.CurrentProspectPeriod.PAS_Units = 10;

			var mockFreightRatingHelper = new Mock<IFreightRatingHelper>();
			mockFreightRatingHelper
				.Setup(x => x.CalculateFreightMode(
						It.IsAny<ZString>(),
						It.IsAny<ZString>(),
						It.IsAny<Func<FreightMode>>()))
				.Returns(FreightMode.FCL);

			using (ObjectFactory.Substitute(mockFreightRatingHelper.Object))
			{
				var spotQuote = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
				spotQuote.ImportTradeDetailData(tradeDetail);

				CombineAssertions(() =>
				{
					AssertEquals("Client", org.PK, spotQuote.ClientDocAddress.OrganisationPK);
					AssertEquals("Consignee", buyer.PK, spotQuote.ConsigneeDocumentaryAddress.OrganisationPK);
					AssertEquals("Consignor", supplier.PK, spotQuote.ConsignorDocumentaryAddress.OrganisationPK);
					AssertEquals("Mode", "FCL", spotQuote.Mode);
					AssertEquals("TransportMode", "SEA", spotQuote.TransportMode);
					AssertEquals("ContainerMode", "FCL", spotQuote.ContainerMode);
					AssertEquals("ServiceLevel", "SRV", spotQuote.ServiceLevel);
					AssertEquals("Origin", "AUSYD", spotQuote.Origin);
					AssertEquals("Destination", "AUMEL", spotQuote.Destination);
					AssertEquals("Weight", 100m, spotQuote.Weight);
					AssertEquals("WeightUnit", "KG", spotQuote.WeightUnit);
					AssertEquals("Volume", 10m, spotQuote.Volume);
					AssertEquals("VolumeUnit", "M3", spotQuote.VolumeUnit);
					AssertEquals("PaymentTerms", "INC", spotQuote.PaymentTerms);
					AssertEquals("Commodity", "COM", spotQuote.Commodity);
					AssertEquals("OH_Carrier", carrier.PK, spotQuote.OH_Carrier);
				});

				AssertEquals(1, spotQuote.Quote.CurrentOneOffQuote.Containers.Count);
				var spotQuoteContainer = spotQuote.Quote.CurrentOneOffQuote.Containers[0];
				AssertEquals((ZShort)10, spotQuoteContainer.TC_ContainerCount);
				AssertEquals(container.PK, spotQuoteContainer.TC_RC);
				mockFreightRatingHelper.VerifyAll();
			}
		}

		public void TestImportTradeDetail_WithIncompatibleData()
		{
			var sales = Factory.New<OrgSales>();
			sales.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "NSW", RefCountryStatesSchema.Constants.Prefix).PK;
			sales.OW_DestinationID = ViewLocationHelper.GetLocationFromString(Factory, "VIC", RefCountryStatesSchema.Constants.Prefix).PK;
			var tradeDetail = sales.TradeDetails.AddNew();
			tradeDetail.PA_TradeMode = "SEA";
			tradeDetail.PA_TradeType = "FCL";

			var mockFreightRatingHelper = new Mock<IFreightRatingHelper>();
			mockFreightRatingHelper
				.Setup(x => x.CalculateFreightMode(
						It.IsAny<ZString>(),
						It.IsAny<ZString>(),
						It.IsAny<Func<FreightMode>>()))
				.Returns(FreightMode.FCL);

			using (ObjectFactory.Substitute(mockFreightRatingHelper.Object))
			{
				var spotQuote = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
				spotQuote.ImportTradeDetailData(tradeDetail);

				CombineAssertions(() =>
				{
					AssertEquals("Client", ZGuid.Empty, spotQuote.ClientDocAddress.OrganisationPK);
					AssertEquals("Consignee", ZGuid.Empty, spotQuote.ConsigneeDocumentaryAddress.OrganisationPK);
					AssertEquals("Consignor", ZGuid.Empty, spotQuote.ConsignorDocumentaryAddress.OrganisationPK);
					AssertEquals("Mode", "FCL", spotQuote.Mode);
					AssertEquals("TransportMode", "SEA", spotQuote.TransportMode);
					AssertEquals("ContainerMode", "FCL", spotQuote.ContainerMode);
					AssertEquals("ServiceLevel", "", spotQuote.ServiceLevel);
					AssertEquals("Origin", "", spotQuote.Origin);
					AssertEquals("Destination", "", spotQuote.Destination);
					AssertEquals("Weight", 0m, spotQuote.Weight);
					AssertEquals("Volume", 0m, spotQuote.Volume);
					AssertEquals("PaymentTerms", "", spotQuote.PaymentTerms);
					AssertEquals("Commodity", "", spotQuote.Commodity);
				});

				AssertEquals(0, spotQuote.Quote.CurrentOneOffQuote.Containers.Count);
				mockFreightRatingHelper.VerifyAll();
			}
		}

		public void TestISupportsPostingOverseasAgentCharge()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			AssertNotNull(@"ForwardingConsol is made to implement the empty interface ISupportsPostingOverseasAgentCharge so that action trigger POA can be made
					applicable specifically to this type"
							, quotedBooking);
		}

		public void TestJobDatesProvider()
		{
			var shipment = Factory.New<CommonShipment>();
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, shipment.PK, Factory);
			AssertType<QuotedBookingJobDatesProvider>(quotedBooking.GetFirstAdapter().JobDatesProvider);
		}

		public void TestSellSpotRateInfo()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			AssertEquals(Money.Invalid, ((ISpotRate)quotedBooking.GetFirstAdapter()).SellSpotRateInfo.Rate);
			AssertEquals(Constants.FreightRateAutoratingModes.Code.StandardRate, ((ISpotRate)quotedBooking.GetFirstAdapter()).SellSpotRateInfo.AutoratedMode);

			var shipment = Factory.New<CommonShipment>();
			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_ParentID = shipment.PK;
			var glbDepartment = Factory.New<GlbDepartment>();
			glbDepartment.GE_Code = "CCC";
			jobHeader.JH_GE = glbDepartment.PK;
			shipment.JS_UnitFreightRate = 20m;
			shipment.JS_RX_NKFrtRateCurrency = "USD";
			quotedBooking = QuotedBooking.New(ZGuid.Empty, shipment.PK, Factory);
			AssertEquals(20m, ((ISpotRate)quotedBooking.GetFirstAdapter()).SellSpotRateInfo.Rate.Amount);
			AssertEquals("USD", ((ISpotRate)quotedBooking.GetFirstAdapter()).SellSpotRateInfo.Rate.Currency.Code);
			AssertEquals(Constants.FreightRateAutoratingModes.Code.FreightPlusRate, ((ISpotRate)quotedBooking.GetFirstAdapter()).SellSpotRateInfo.AutoratedMode);
		}

		public void TestCostSpotRateInfo()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			AssertEquals(Money.Invalid, ((ISpotRate)quotedBooking.GetFirstAdapter()).CostSpotRateInfo.Rate);
			AssertEquals(Constants.FreightRateAutoratingModes.Code.StandardRate, ((ISpotRate)quotedBooking.GetFirstAdapter()).CostSpotRateInfo.AutoratedMode);

			var shipment = Factory.New<CommonShipment>();
			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_ParentID = shipment.PK;
			var glbDepartment = Factory.New<GlbDepartment>();
			glbDepartment.GE_Code = "CCC";
			jobHeader.JH_GE = glbDepartment.PK;
			shipment.JS_FreightCostRate = 20m;
			shipment.JS_RX_NKFreightCostRateCurrency = "USD";
			quotedBooking = QuotedBooking.New(ZGuid.Empty, shipment.PK, Factory);
			AssertEquals(20m, ((ISpotRate)quotedBooking.GetFirstAdapter()).CostSpotRateInfo.Rate.Amount);
			AssertEquals("USD", ((ISpotRate)quotedBooking.GetFirstAdapter()).CostSpotRateInfo.Rate.Currency.Code);
			AssertEquals(Constants.FreightRateAutoratingModes.Code.FreightPlusRate, ((ISpotRate)quotedBooking.GetFirstAdapter()).CostSpotRateInfo.AutoratedMode);
		}

		public void TestSellSpotRateInfoShouldUseJobFromQuotedBooking()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();

			var glbDepartment = Factory.New<GlbDepartment>();
			glbDepartment.GE_Code = "CCC";
			jobHeader.JH_GE = glbDepartment.PK;
			jobHeader.JH_ParentID = quote.PK;

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_FreightCostRateAutoratingMode = Constants.FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_UnitFreightRate = 20m;
			shipment.JS_RX_NKFrtRateCurrency = "USD";

			var quotedBooking = QuotedBooking.New(quote.PK, shipment.PK, Factory);

			var spotRate = (ISpotRate)quotedBooking.GetFirstAdapter();
			var sellSpotRateInfo = spotRate.SellSpotRateInfo;
			AssertEquals(20m, sellSpotRateInfo.Rate.Amount);
			AssertEquals("USD", sellSpotRateInfo.Rate.Currency.Code);
			AssertEquals(Constants.FreightRateAutoratingModes.Code.FreightPlusRate, sellSpotRateInfo.AutoratedMode);
		}

		public void TestCostSpotRateInfoShouldUseJobFromQuotedBooking()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();

			var glbDepartment = Factory.New<GlbDepartment>();
			glbDepartment.GE_Code = "CCC";
			jobHeader.JH_GE = glbDepartment.PK;
			jobHeader.JH_ParentID = quote.PK;

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_FreightCostRateAutoratingMode = Constants.FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_FreightCostRate = 20m;
			shipment.JS_RX_NKFreightCostRateCurrency = "USD";

			var quotedBooking = QuotedBooking.New(quote.PK, shipment.PK, Factory);

			var spotRate = (ISpotRate)quotedBooking.GetFirstAdapter();
			var costSpotRateInfo = spotRate.CostSpotRateInfo;
			AssertEquals(20m, costSpotRateInfo.Rate.Amount);
			AssertEquals("USD", costSpotRateInfo.Rate.Currency.Code);
			AssertEquals(Constants.FreightRateAutoratingModes.Code.FreightPlusRate, costSpotRateInfo.AutoratedMode);
		}

		public void TestHasChangesAfterJobChanged()
		{
			var quotedBooking = GetNewBusinessObject() as QuotedBooking;

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentID = quotedBooking.Quote.PK;
			jobHeader.Parent = quotedBooking.Quote;
			jobHeader.JH_Description = "BASE FOR TEST";

			Factory.Save();

			AssertEquals(false, quotedBooking.HasChanges);
			AssertEquals(false, quotedBooking.Booking.HasChanges);
			AssertEquals(false, quotedBooking.Quote.HasChanges);

			quotedBooking.Job.JH_Description = "CHANGE FOR TEST";
			AssertEquals(true, quotedBooking.HasChanges);
			AssertEquals(true, quotedBooking.Booking.HasChanges);
			AssertEquals(true, quotedBooking.Quote.HasChanges);
		}

		public void TestImportBroker()
		{
			var importBroker = Factory.New<OrgHeader>();
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_OH_ImportBroker = importBroker.PK;
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, shipment.PK, Factory);
			AssertEquals(importBroker.PK, quotedBooking.ImportBroker.PK);
		}

		public void TestExportBroker()
		{
			var exportBroker = Factory.New<OrgHeader>();
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_OH_ExportBroker = exportBroker.PK;
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, shipment.PK, Factory);
			AssertEquals(exportBroker.PK, quotedBooking.ExportBroker.PK);
		}

		public void TestChargesDisplay_List()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			Func<IEnumerable<string>> getCollectionDescriptions = () => quotedBooking.HBLAWBChargesDisplay_List.Cast<CodeDescriptionPair>().Select(x => x.Code);

			quotedBooking.TransportMode = Constants.TransportModes.Air;
			var collection = getCollectionDescriptions();
			AssertCollectionContains("NON", collection);
			AssertCollectionContains("ALL", collection);
			AssertCollectionContains("NPP", collection);
			AssertCollectionContains("ANO", collection);
			AssertCollectionContains("APP", collection);
			AssertCollectionContains("CNO", collection);
			AssertCollectionContains("CAL", collection);
			AssertCollectionContains("CPD", collection);
			AssertCollectionNotContains("SHW", collection);
			AssertCollectionNotContains("PPD", collection);
			AssertCollectionNotContains("AGR", collection);
			AssertCollectionNotContains("CCL", collection);

			quotedBooking.TransportMode = Constants.TransportModes.Sea;
			collection = getCollectionDescriptions();
			AssertCollectionContains("NON", collection);
			AssertCollectionContains("SHW", collection);
			AssertCollectionContains("PPD", collection);
			AssertCollectionContains("AGR", collection);
			AssertCollectionContains("ALL", collection);
			AssertCollectionContains("CCL", collection);
			AssertCollectionContains("CPP", collection);
			AssertCollectionContains("CAL", collection);
			AssertCollectionNotContains("NPP", collection);
			AssertCollectionNotContains("ANO", collection);
			AssertCollectionNotContains("APP", collection);
			AssertCollectionNotContains("CNO", collection);
			AssertCollectionNotContains("CPD", collection);
		}

		public void TestTransitTimesList()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			Func<IEnumerable<string>> getCollectionDescriptions = () => quotedBooking.TransitTimesList.Cast<CodeDescriptionPair>().Select(x => x.Description);

			quotedBooking.Mode = Core.Constants.RateMode.LSE;
			var collection = getCollectionDescriptions();
			AssertCollectionContains("Same Day", collection);
			AssertCollectionContains("5 days", collection);

			quotedBooking.Mode = Core.Constants.RateMode.LCL;
			collection = getCollectionDescriptions();
			AssertCollectionNotContains("Same Day", collection);
			AssertCollectionContains("5 days", collection);

			quotedBooking.Mode = Core.Constants.RateMode.RAI;
			collection = getCollectionDescriptions();
			AssertCollectionNotContains("Same Day", collection);
			AssertCollectionContains("5 days", collection);
		}

		public void TestTransitTime_ReadOnly()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);

			quotedBooking.Mode = Core.Constants.RateMode.LSE;
			quotedBooking.TransitTime = RatingConstants.TransitTimes.SameDay;
			Assert("Valid Mode, should not be read only", !quotedBooking.TransitTimeInfo.ReadOnly);
			AssertEquals("Valid Mode, transit time should persist", RatingConstants.TransitTimes.SameDay, quotedBooking.TransitTime);

			quotedBooking.Mode = ZString.Empty;
			Assert("Empty Mode, should be read only", quotedBooking.TransitTimeInfo.ReadOnly);
			AssertEquals("Empty Mode, transit time should be reset", ZString.Empty, quotedBooking.TransitTime);

			quotedBooking.Mode = Core.Constants.RateMode.LCL;
			quotedBooking.TransitTime = "4";
			Assert("Valid Mode, should not be read only", !quotedBooking.TransitTimeInfo.ReadOnly);
			AssertEquals("Valid Mode, transit time should persist", "4", quotedBooking.TransitTime);

			quotedBooking.Mode = Core.Constants.RateMode.FCL;
			Assert("Valid Mode, should not be read only", !quotedBooking.TransitTimeInfo.ReadOnly);
			AssertEquals("List should contain this Transit Time for FCL too, value should persist", "4", quotedBooking.TransitTime);

			quotedBooking.TransportMode = "XXX";
			Assert("Invalid Mode, should be read only", quotedBooking.TransitTimeInfo.ReadOnly);
			AssertEquals("Invalid Mode, transit time should be reset", ZString.Empty, quotedBooking.TransitTime);
		}

		public void TestCompanyTariffLevelInOneOffQuote_ReadOnly()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			using (DataRegistryRating.Instance.AllowOverrideCompanyTariffLevel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Assert("When AllowOverrideCompanyTariffLevel is false, CompanyTariffLevel should be read-only", quotedBooking.CompanyTariffLevelInfo.ReadOnly);
				quotedBooking.ClientDocAddress.E2_AddressOverride = true;
				Assert("When AllowOverrideCompanyTariffLevel is false but client is overriden, CompanyTariffLevel should be still editable", !quotedBooking.CompanyTariffLevelInfo.ReadOnly);
			}

			using (DataRegistryRating.Instance.AllowOverrideCompanyTariffLevel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				quotedBooking.ClientDocAddress.E2_AddressOverride = false;
				Assert("When AllowOverrideCompanyTariffLevel is true, CompanyTariffLevel should be editable", !quotedBooking.CompanyTariffLevelInfo.ReadOnly);
				quotedBooking.ClientDocAddress.E2_AddressOverride = true;
				Assert("When AllowOverrideCompanyTariffLevel is true, CompanyTariffLevel should be editable", !quotedBooking.CompanyTariffLevelInfo.ReadOnly);
			}
		}

		public void TestNewQuotedBookingCreatesView()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var viewQuotedBooking = Factory.Load<ViewQuotedBooking>(quotedBooking.PK);

			AssertEquals(false, viewQuotedBooking.IsInDatabase);
			AssertNotNull(viewQuotedBooking);
		}

		public void TestNewQuotedBookingCorrectlyCreatesNotesOfTypeForwardingShipmentStmNoteFromTemplate()
		{
			var templateRecord = Factory.New<StmTemplateRecord>();
			templateRecord.STR_ModuleID = "QuotedBooking";
			templateRecord.STR_Data = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingBooking</Type>
          <Key />
        </DataSource>
      </DataSourceCollection>
    </DataContext>
    <NoteCollection Content=""Partial"">
      <Note>
        <Description>Country Rules</Description>
        <IsCustomDescription>false</IsCustomDescription>
        <NoteText>Hello World</NoteText>
        <NoteContext>
          <Code>AAA</Code>
          <Description>Module: A - All, Direction: A - All, Freight: A - All</Description>
        </NoteContext>
        <Visibility>
          <Code>PUB</Code>
          <Description>CLIENT-VISIBLE</Description>
        </Visibility>
      </Note>
    </NoteCollection>
  </Shipment>
</UniversalShipment>";

			var quotedBooking = QuotedBooking.New(Factory, templateRecord);

			var note = quotedBooking.Booking.Notes.GetAllNotes().Single();
			Assert($"Note of incorrect type {note.GetType()} created instead of {typeof(ForwardingShipmentStmNote)}",
				note.GetType() == typeof(ForwardingShipmentStmNote));
		}

		[ExpectNoExceptions]
		public void TestNewQuotedBookingFromTemplateWithNoNotesDoesNotThrow()
		{
			var templateRecord = Factory.New<StmTemplateRecord>();
			templateRecord.STR_ModuleID = "QuotedBooking";
			templateRecord.STR_Data = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingBooking</Type>
          <Key />
        </DataSource>
      </DataSourceCollection>
    </DataContext>
  </Shipment>
</UniversalShipment>";

			QuotedBooking.New(Factory, templateRecord);
		}

		public void TestQuickBookingDefaultsRightModeFromBuyerSupplierRelationships()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_RL_NKClosestPort = "USLGB";

			var link = consignee.SupplierLinks.AddNew(consignor);

			var linkDetails = (OrgSupBuyLinkTrnMode)link.OrgSupBuyLinkTrnModes.First();
			linkDetails.PF_TransportMode = Constants.TransportModes.Sea;
			linkDetails.PF_ContainerMode = Constants.ContainerModes.FCL;

			Factory.Save();

			var booking = QuotedBooking.CreateNewBooking(Factory);

			var quickBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			booking.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			booking.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			booking.JS_RL_NKDestination = "USLGB";

			AssertEquals(Constants.TransportModes.Sea, quickBooking.Booking.JS_TransportMode);
			AssertEquals(Constants.ContainerModes.FCL, quickBooking.Booking.JS_PackingMode);
			AssertEquals(RateMode.FCL, quickBooking.Mode);
		}

		public void TestPortDefaultingFromBuyerConsignorRelationship()
		{
			OrgHeader buyer = Factory.New<OrgHeader>();
			buyer.OH_Code = "AUSBUYER";
			buyer.OH_RL_NKClosestPort = "AUSYD";
			OrgHeader supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "NZSUPPLIER";
			supplier.OH_RL_NKClosestPort = "NZAKL";

			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking1 = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			quotedBooking1.ConsigneeDocumentaryAddress.OrganisationPK = buyer.PK;
			quotedBooking1.ConsignorDocumentaryAddress.OrganisationPK = supplier.PK;

			AssertEquals("No Supplier/Buyer Link: Load should default to supplier's UNLOCO", "NZAKL", quotedBooking1.LoadPort);
			AssertEquals("No Supplier/Buyer Link: Discharge should default to buyer's UNLOCO", "AUSYD", quotedBooking1.DischargePort);

			OrgSupplierBuyerLink link = Factory.New<OrgSupplierBuyerLink>();
			link.OL_OH_Buyer = buyer.PK;
			link.OL_OH_Supplier = supplier.PK;
			OrgSupBuyLinkTrnMode linkTrnMode = link.OrgSupBuyLinkTrnModes[0];
			linkTrnMode.PF_OL = link.PK;
			linkTrnMode.PF_TransportMode = Core.Constants.TransportModes.Sea;
			linkTrnMode.PF_ContainerMode = Core.Constants.ContainerModes.FCL;
			linkTrnMode.PF_RL_NKLoadPort = "NZWLG";
			linkTrnMode.PF_RL_NKDischargePort = "AUBNE";

			Factory.Save();

			QuotedBooking quotedBooking2 = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			quotedBooking2.TransportMode = Core.Constants.TransportModes.Sea;
			quotedBooking2.ContainerMode = Core.Constants.ContainerModes.FCL;
			quotedBooking2.ConsigneeDocumentaryAddress.OrganisationPK = buyer.PK;
			quotedBooking2.ConsignorDocumentaryAddress.OrganisationPK = supplier.PK;

			AssertEquals("Load should default from SupplierBuyerLink", "NZWLG", quotedBooking2.LoadPort);
			AssertEquals("Discharge should default from SupplierBuyerLink", "AUBNE", quotedBooking2.DischargePort);
		}

		public void TestPortsNotUpdatedForQuoteOnly()
		{
			QuotedBooking quoteOnly = GetQuoteOnlyQuotedBooking();
			AssertEquals("Precondition: quote only", QuotedBookingState.QuoteOnly, quoteOnly.ObjectState);
			AssertEquals("Precondition: Load Port is not set", "", quoteOnly.LoadPort);
			AssertEquals("Precondition: Discharge Port is not set", "", quoteOnly.DischargePort);

			((IBuyerSupplierRelationshipConsumer)quoteOnly).LoadPort = "AUSYD";
			((IBuyerSupplierRelationshipConsumer)quoteOnly).DischargePort = "NZAKL";

			AssertEquals("Load Port is not set", "", quoteOnly.LoadPort);
			AssertEquals("Discharge Port is not set", "", quoteOnly.DischargePort);

			QuotedBooking bookingOnly = QuotedBooking.New(ZGuid.Empty, QuotedBooking.CreateNewBooking(Factory).PK, Factory);
			AssertEquals("Precondition: booking", QuotedBookingState.BookingOnly, bookingOnly.ObjectState);

			((IBuyerSupplierRelationshipConsumer)bookingOnly).LoadPort = "AUSYD";
			((IBuyerSupplierRelationshipConsumer)bookingOnly).DischargePort = "NZAKL";

			AssertEquals("ScheduleChooser's load port was set", "AUSYD", bookingOnly.LoadPort);
			AssertEquals("ScheduleChooser's discharge port was set", "NZAKL", bookingOnly.DischargePort);
		}

		public void TestBookingReleaseType()
		{
			FreightDataRegistry.Instance.ReleaseType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "EBL");

			var quoteBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			quoteBooking.Mode = "FCL";

			AssertEquals("Release type should be the default value from registry item", "EBL", quoteBooking.Booking.JS_ReleaseType);

			FreightDataRegistry.Instance.ReleaseType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);

			quoteBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			quoteBooking.Mode = "FCL";

			AssertEquals("Release type should be the default value from registry item", string.Empty, quoteBooking.Booking.JS_ReleaseType);
		}

		public void TestConvertedShipmentReadOnly()
		{
			var organization1 = Factory.New<OrgHeader>();
			organization1.OH_Code = "AAAA";

			var organization2 = Factory.New<OrgHeader>();
			organization2.OH_Code = "BBBB";

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			quote.QuotationClientAddress.OrganisationPK = organization1.PK;
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);

			quotedBooking.ConvertQuoteToQuotedBooking();

			JobHeader quoteJob = new JobHeader.Loader(quotedBooking).TryLoadOrCreate();
			quoteJob.JH_OA_LocalChargesAddr = organization2.MainAddress.PK;
			var shipment = Factory.New<ForwardingShipment>();
			var shipmentJob = new JobHeader.Loader(shipment).TryLoadOrCreate();
			shipmentJob.JH_TH_NKQuoteNumber = quotedBooking.QuotedBookingNumber;
			var department = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FIS");
			var chargeCode1 = LoadAccChargeCode("BAF");
			AddNewDepartmentCharge(department, chargeCode1);
			shipmentJob.JH_GE = department.PK;

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			shipment = otherFactory.Load<ForwardingShipment>(shipment.PK);
			shipment.RegisterEditableChildObject(shipment.Job);
			shipment.SetReadOnlyIncludingChildren(true);
			Assert("Shipment should be readonly", shipment.ReadOnly);
			Assert("the Job of the Shipment should be readonly", shipment.Job.ReadOnly);
			quotedBooking = QuotedBooking.New(quote.PK, shipment.PK, otherFactory);
			Assert(" QuotedBooking initialization should not affect the Shipment", shipment.ReadOnly);
			Assert(" QuotedBooking initialization should not affect the Shipment and its Children", shipment.Job.ReadOnly);
		}

		public void TestMode_ReadOnly()
		{
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			AssertEquals(false, quotedBooking.ModeInfo.ReadOnly);
			quotedBooking.Mode_ReadOnly = true;
			AssertEquals(true, quotedBooking.ModeInfo.ReadOnly);
		}

		public void TestClientAddrPK_ReadOnly()
		{
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			AssertEquals(false, quotedBooking.ClientAddrPKInfo.ReadOnly);
			quotedBooking.ClientAddrPK_ReadOnly = true;
			AssertEquals(true, quotedBooking.ClientAddrPKInfo.ReadOnly);
		}

		public void TestServiceLevel_ReadOnly()
		{
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			AssertEquals(false, quotedBooking.ServiceLevelInfo.ReadOnly);
			quotedBooking.ServiceLevel_ReadOnly = true;
			AssertEquals(true, quotedBooking.ServiceLevelInfo.ReadOnly);
		}

		public void TestDestination_ReadOnly()
		{
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			AssertEquals(false, quotedBooking.DestinationInfo.ReadOnly);
			quotedBooking.Destination_ReadOnly = true;
			AssertEquals(true, quotedBooking.DestinationInfo.ReadOnly);
		}

		public void TestOrigin_ReadOnly()
		{
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			AssertEquals(false, quotedBooking.OriginInfo.ReadOnly);
			quotedBooking.Origin_ReadOnly = true;
			AssertEquals(true, quotedBooking.OriginInfo.ReadOnly);
		}

		public void TestServicesWithQuoteOnlyBooking()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			AssertNoExceptionThrown(() => _ = quotedBooking.Services);
		}

		public void TestCanCreateTransportBooking()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			quotedBooking.CreateNewBookingForUniversalCopy();

			quotedBooking.Booking.JS_IsForwardRegistered = false;
			AssertEquals("Should be allowed to create transport booking.", true, quotedBooking.CanCreateTransportBooking);

			quotedBooking.Booking.JS_IsForwardRegistered = true;
			AssertEquals("Should not be allowed to create transport booking.", false, quotedBooking.CanCreateTransportBooking);
		}

		public void TestBookingParentPK()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			var dtbBookingParent = quotedBooking as IDtbBookingParent;
			AssertEquals("BookingParentPK should be the Quoted Booking PK.", quotedBooking.PK, dtbBookingParent.BookingParentPK);
		}

		public void TestBookingParentTablePrefix()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			var dtbBookingParent = quotedBooking as IDtbBookingParent;
			AssertEquals("BookingParentTablePrefix should be the Quoted Booking table prefix.", quotedBooking.TablePrefix, dtbBookingParent.BookingParentTablePrefix);
		}

		public void TestGetExtendingConfirmMessageBeforeCreateTransportBooking()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			var dtbBookingParent = quotedBooking as IDtbBookingParent;

			var (isShouldShow, caption, message, confirmation) = dtbBookingParent.GetExtendingConfirmMessageBeforeCreateTransportBooking();
			AssertEquals("IsShouldShow should return false.", false, isShouldShow);
			AssertNullOrEmpty("Caption should be null.", caption);
			AssertNullOrEmpty("Message should be null.", message);
			AssertNullOrEmpty("Confirmation should be null.", confirmation);
		}

		#region ITemplateReversible

		public void TestReverse()
		{
			OrgHeader randomOrg = Factory.NewWithValidTestData<OrgHeader>();
			Quote testQuote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);

			testQuote.TH_OneTimeQuote = true;
			testQuote.CurrentOneOffQuote.TT_RL_NKReceivalLocation = "USCHI";
			testQuote.CurrentOneOffQuote.TT_RL_NKDeliveryLocation = "AUSYD";

			testQuote.CurrentOneOffQuote.DeliveryDocAddress.E2_AddressOverride = true;
			testQuote.CurrentOneOffQuote.DeliveryDocAddress.E2_CompanyName = "Quote Company";
			testQuote.CurrentOneOffQuote.PickUpDocAddress.E2_AddressOverride = false;
			testQuote.CurrentOneOffQuote.PickUpDocAddress.OrganisationPK = randomOrg.PK;

			QuotedBooking testQuotedBooking = QuotedBooking.New(testQuote.PK, ZGuid.Empty, Factory);

			AssertEquals("Origin", "USCHI", testQuotedBooking.Origin);
			AssertEquals("Destination", "AUSYD", testQuotedBooking.Destination);

			AssertEquals("Consignee Documentary Address Override", true, testQuotedBooking.ConsigneeDocumentaryAddress.E2_AddressOverride);
			AssertEquals("Consignee Documentary Address Company Name", "Quote Company", testQuotedBooking.ConsigneeDocumentaryAddress.E2_CompanyName);
			AssertEquals("Consignor Documentary Address Override", false, testQuotedBooking.ConsignorDocumentaryAddress.E2_AddressOverride);
			AssertEquals("Consignor Documentary Address Organisation PK", randomOrg.PK, testQuotedBooking.ConsignorDocumentaryAddress.OrganisationPK);

			((ITemplateReversible)testQuotedBooking).Reverse();

			AssertEquals("Origin", "AUSYD", testQuotedBooking.Origin);
			AssertEquals("Destination", "USCHI", testQuotedBooking.Destination);

			AssertEquals("Consignee Documentary Address Override", false, testQuotedBooking.ConsigneeDocumentaryAddress.E2_AddressOverride);
			AssertEquals("Consignee Documentary Address Organisation PK", randomOrg.PK, testQuotedBooking.ConsigneeDocumentaryAddress.OrganisationPK);
			AssertEquals("Consignor Documentary Address Override", true, testQuotedBooking.ConsignorDocumentaryAddress.E2_AddressOverride);
			AssertEquals("Consignor Documentary Address Company Name", "Quote Company", testQuotedBooking.ConsignorDocumentaryAddress.E2_CompanyName);

			OrgHeader randomOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			ForwardingShipment testBooking = QuotedBooking.CreateNewBooking(Factory);

			testBooking.JS_RL_NKOrigin = "USCHI";
			testBooking.JS_RL_NKDestination = "AUSYD";

			testBooking.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			testBooking.ConsigneeDocumentaryAddress.E2_CompanyName = "Booking Company";
			testBooking.ConsignorDocumentaryAddress.E2_AddressOverride = false;
			testBooking.ConsignorDocumentaryAddress.OrganisationPK = randomOrg.PK;

			testQuotedBooking = QuotedBooking.New(ZGuid.Empty, testBooking.PK, Factory);

			AssertEquals("Origin", "USCHI", testQuotedBooking.Origin);
			AssertEquals("Destination", "AUSYD", testQuotedBooking.Destination);

			AssertEquals("Consignee Documentary Address Override", true, testQuotedBooking.ConsigneeDocumentaryAddress.E2_AddressOverride);
			AssertEquals("Consignee Documentary Address Company Name", "Booking Company", testQuotedBooking.ConsigneeDocumentaryAddress.E2_CompanyName);
			AssertEquals("Consignor Documentary Address Override", false, testQuotedBooking.ConsignorDocumentaryAddress.E2_AddressOverride);
			AssertEquals("Consignor Documentary Address Organisation PK", randomOrg.PK, testQuotedBooking.ConsignorDocumentaryAddress.OrganisationPK);

			((ITemplateReversible)testQuotedBooking).Reverse();

			AssertEquals("Origin", "AUSYD", testQuotedBooking.Origin);
			AssertEquals("Destination", "USCHI", testQuotedBooking.Destination);

			AssertEquals("Consignee Documentary Address Override", false, testQuotedBooking.ConsigneeDocumentaryAddress.E2_AddressOverride);
			AssertEquals("Consignee Documentary Address Organisation PK", randomOrg.PK, testQuotedBooking.ConsigneeDocumentaryAddress.OrganisationPK);
			AssertEquals("Consignor Documentary Address Override", true, testQuotedBooking.ConsignorDocumentaryAddress.E2_AddressOverride);
			AssertEquals("Consignor Documentary Address Company Name", "Booking Company", testQuotedBooking.ConsignorDocumentaryAddress.E2_CompanyName);
		}

		#endregion

		#region CanConvertQuoteToQuotedBooking

		public void TestConvertQuoteToQuotedBooking_AdditionalTerms()
		{
			var quotedBooking = GetQuoteOnlyQuotedBooking();
			quotedBooking.Quote.CurrentOneOffQuote.TT_AdditionalTerms = "one-off quote additional terms xxx";

			quotedBooking.ConvertQuoteToQuotedBooking();

			AssertEquals
			(
				"Booking/Shipment AdditionalTerms should be copied from OneOffQuote",
				"one-off quote additional terms xxx",
				quotedBooking.Booking.JS_AdditionalTerms
			);
		}

		public void TestConvertQuoteToQuotedBookingNullifiesAddresses()
		{
			QuotedBooking testQuotedBooking = GetQuoteOnlyQuotedBooking();
			try
			{
				testQuotedBooking.Quote.CurrentOneOffQuote.TT_QuoteApprovedByManager = true;
				JobHeader job = new JobHeader.Loader(testQuotedBooking).TryLoadOrCreate();
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				testQuotedBooking.Factory.Save();

				testQuotedBooking.ConvertQuoteToQuotedBooking();

				AssertEquals(DocAddressType.ConsignorDocumentaryAddress, testQuotedBooking.ConsignorDocumentaryAddress.DocAddressType);
				AssertEquals(DocAddressType.ConsigneeDocumentaryAddress, testQuotedBooking.ConsigneeDocumentaryAddress.DocAddressType);
				Assert(testQuotedBooking.ConsignorDocumentaryAddress.Parent is ForwardingShipment);
				Assert(testQuotedBooking.ConsigneeDocumentaryAddress.Parent is ForwardingShipment);
			}
			finally
			{
				DisposeJobs(testQuotedBooking);
			}
		}

		public void TestConvertQuoteToQuotedBooking_StatusClientAccepted()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var testQuotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
			try
			{
				testQuotedBooking.Quote.CurrentOneOffQuote.TT_QuoteApprovedByManager = true;
				testQuotedBooking.Quote.TH_ClientAccepted = ZDateTime.Now;
				var job = new JobHeader.Loader(testQuotedBooking).TryLoadOrCreate();
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				testQuotedBooking.Factory.Save();

				AssertEquals(QuoteStatusOptions.ClientAccepted, testQuotedBooking.Quote.QuoteStatus);

				testQuotedBooking.ConvertQuoteToQuotedBooking();

				AssertEquals(QuoteStatusOptions.Accepted, testQuotedBooking.Quote.QuoteStatus);
			}
			finally
			{
				DisposeJobs(testQuotedBooking);
			}
		}

		public void TestConvertQuoteToQuotedBooking_StatusApproved()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var testQuotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
			try
			{
				testQuotedBooking.Quote.CurrentOneOffQuote.TT_QuoteApprovedByManager = true;
				var job = new JobHeader.Loader(testQuotedBooking).TryLoadOrCreate();
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				testQuotedBooking.Factory.Save();

				AssertEquals(QuoteStatusOptions.Approved, testQuotedBooking.Quote.QuoteStatus);

				testQuotedBooking.ConvertQuoteToQuotedBooking();

				AssertEquals(QuoteStatusOptions.Accepted, testQuotedBooking.Quote.QuoteStatus);
			}
			finally
			{
				DisposeJobs(testQuotedBooking);
			}
		}

		public void TestConvertOneOffQuoteToQuotedBookingDoesNotChangeOriginDestinationWhenOriginDestinationHaveValues()
		{
			QuotedBooking testQuotedBooking = GetQuoteOnlyQuotedBooking();

			try
			{
				var consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
				var consigneeOrgAddress = consigneeOrg.Addresses.AddNew();
				consigneeOrgAddress.Address1 = "pickup address 1";
				consigneeOrgAddress.AddressCode = "Delivery Point";
				consigneeOrgAddress.OA_RL_NKRelatedPortCode = "AUSYD";
				consigneeOrgAddress.AddAddressType(OrgAddressType.Delivery);
				testQuotedBooking.ConsigneeDocumentaryAddress.OrganisationPK = consigneeOrg.PK;

				var consignorOrg = Factory.NewWithValidTestData<OrgHeader>();
				var consignorOrgAddress = consignorOrg.Addresses.AddNew();
				consignorOrgAddress.Address1 = "Pick up address 1";
				consignorOrgAddress.AddressCode = "Pickup Point";
				consignorOrgAddress.OA_RL_NKRelatedPortCode = "AUMEL";
				consignorOrgAddress.AddAddressType(OrgAddressType.Pickup);
				testQuotedBooking.ConsignorDocumentaryAddress.OrganisationPK = consignorOrg.PK;

				testQuotedBooking.Quote.CurrentOneOffQuote.TT_RL_NKReceivalLocation = "USLAX";
				testQuotedBooking.Quote.CurrentOneOffQuote.TT_RL_NKDeliveryLocation = "HKHKG";

				testQuotedBooking.Factory.Save();

				testQuotedBooking.ConvertQuoteToQuotedBooking();

				CombineAssertions("", () =>
				{
					AssertEquals("Origin should be USLAX", "USLAX", testQuotedBooking.Booking.JS_RL_NKOrigin);
					AssertEquals("Destenitioan should be HKHKG", "HKHKG", testQuotedBooking.Booking.JS_RL_NKDestination);
				});
			}
			finally
			{
				DisposeJobs(testQuotedBooking);
			}
		}

		public void TestConvertOneOffQuoteToQuotedBooking_WithSpecifiedServiceLevel_DontUseFallbackServiceLevels()
		{
			QuotedBooking testQuotedBooking = GetQuoteOnlyQuotedBooking();

			var consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			var consigneeOrgAddress = consigneeOrg.Addresses.AddNew();
			consigneeOrgAddress.Address1 = "pickup address 1";
			consigneeOrgAddress.AddressCode = "Delivery Point";
			consigneeOrgAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			consigneeOrgAddress.AddAddressType(OrgAddressType.Delivery);
			testQuotedBooking.ConsigneeDocumentaryAddress.OrganisationPK = consigneeOrg.PK;

			testQuotedBooking.Quote.CurrentOneOffQuote.TT_RS_NKServiceLevel = "ECN";
			testQuotedBooking.Factory.Save();
			testQuotedBooking.ConvertQuoteToQuotedBooking();
			testQuotedBooking.BuyerSupplierLinksHelper.Register();

			AssertEquals("Converted service level should be ECN", "ECN", testQuotedBooking.ServiceLevel.ToString());

			var consignorOrg = Factory.NewWithValidTestData<OrgHeader>();
			consignorOrg.MiscServ.OM_RS_NKEXDefaultServiceLevel = "AAA";
			var consignorOrgAddress = consignorOrg.Addresses.AddNew();
			consignorOrgAddress.Address1 = "Pick up address 1";
			consignorOrgAddress.AddressCode = "Pickup Point";
			consignorOrgAddress.OA_RL_NKRelatedPortCode = "AUMEL";
			consignorOrgAddress.AddAddressType(OrgAddressType.Pickup);
			testQuotedBooking.ConsignorDocumentaryAddress.OrganisationPK = consignorOrg.PK;

			AssertEquals("Service level should remain the same", "ECN", testQuotedBooking.ServiceLevel.ToString());
		}

		public void TestConvertOneOffQuoteToQuotedBooking_WithoutSpecifiedServiceLevel_UseFallbackServiceLevels()
		{
			QuotedBooking testQuotedBooking = GetQuoteOnlyQuotedBooking();

			var consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			var consigneeOrgAddress = consigneeOrg.Addresses.AddNew();
			consigneeOrgAddress.Address1 = "pickup address 1";
			consigneeOrgAddress.AddressCode = "Delivery Point";
			consigneeOrgAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			consigneeOrgAddress.AddAddressType(OrgAddressType.Delivery);
			testQuotedBooking.ConsigneeDocumentaryAddress.OrganisationPK = consigneeOrg.PK;

			testQuotedBooking.Quote.CurrentOneOffQuote.TT_RS_NKServiceLevel = string.Empty;
			testQuotedBooking.Factory.Save();
			testQuotedBooking.ConvertQuoteToQuotedBooking();
			testQuotedBooking.BuyerSupplierLinksHelper.Register();

			AssertEquals("Converted service level should be empty", string.Empty, testQuotedBooking.ServiceLevel.ToString());

			var consignorOrg = Factory.NewWithValidTestData<OrgHeader>();
			consignorOrg.MiscServ.OM_RS_NKEXDefaultServiceLevel = "AAA";
			var consignorOrgAddress = consignorOrg.Addresses.AddNew();
			consignorOrgAddress.Address1 = "Pick up address 1";
			consignorOrgAddress.AddressCode = "Pickup Point";
			consignorOrgAddress.OA_RL_NKRelatedPortCode = "AUMEL";
			consignorOrgAddress.AddAddressType(OrgAddressType.Pickup);
			testQuotedBooking.ConsignorDocumentaryAddress.OrganisationPK = consignorOrg.PK;

			AssertEquals("Service level should change to AAA", "AAA", testQuotedBooking.ServiceLevel.ToString());
		}

		public void TestConvertQuoteToQuotedBookingWorkflowItems()
		{
			QuotedBooking quotedBooking = GetQuoteOnlyQuotedBooking();
			quotedBooking.Quote.CurrentOneOffQuote.TT_QuoteApprovedByManager = true;

			JobHeader job = new JobHeader.Loader(quotedBooking).TryLoadOrCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			QuotedBookingProcessTask quoteMilestone = (QuotedBookingProcessTask)quotedBooking.WorkflowItems.Milestones.AddNew();
			quoteMilestone.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;

			quotedBooking.Factory.Save();

			quotedBooking.ConvertQuoteToQuotedBooking();

			AssertContainsExactElementsInAnyOrder(new[] { quoteMilestone }, quotedBooking.WorkflowItems);

			QuotedBookingProcessTask convertedQuoteMilestone = (QuotedBookingProcessTask)quotedBooking.WorkflowItems.Milestones.AddNew();
			convertedQuoteMilestone.TriggerConditions.TriggerEventCode = Events.WebDocumentPrintedDocNameCode;

			AssertContainsExactElementsInAnyOrder(new[] { quoteMilestone, convertedQuoteMilestone }, quotedBooking.WorkflowItems);

			quotedBooking.Factory.Save();

			BusinessObjectFactory factory = new BusinessObjectFactory();

			quoteMilestone = factory.Load<QuotedBookingProcessTask>(quoteMilestone.PK);
			convertedQuoteMilestone = factory.Load<QuotedBookingProcessTask>(convertedQuoteMilestone.PK);
			quotedBooking = QuotedBooking.New(quotedBooking.Quote.PK, quotedBooking.Booking.PK, factory);

			AssertContainsExactElementsInAnyOrder(new[] { quoteMilestone, convertedQuoteMilestone }, quotedBooking.WorkflowItems);
		}

		public void TestBusinessObjectsWithRelatedEvents_ShouldReturnBufferManagementBusinessObjects()
		{
			var bmsRegistry = ObjectFactory.Get<IBMSRegistry>();
			var originalBufferManagementEnabled = bmsRegistry.BufferManagementEnabled;

			using (new DisposableAction(() => bmsRegistry.BufferManagementEnabled = true, () => bmsRegistry.BufferManagementEnabled = originalBufferManagementEnabled))
			{
				var quotedBooking = GetQuoteOnlyQuotedBooking();

				var bmsTestHelper = ObjectFactory.Get<IBMTestHelper>();
				var jobHeader = bmsTestHelper.GetJobHeaderForParent(quotedBooking, Factory, addDefaultProcessHeaderIfNone: false);
				AssertNotNull("Precondition: Buffer Mangement BizObj", jobHeader);

				var stmALogParent = quotedBooking as IStmALogParent;
				var actualBusinessObjectsWithRelatedEvents = stmALogParent.BusinessObjectsWithRelatedEvents;
				AssertContainsExactElementsInAnyOrder
				(
					"QuotedBooking BusinessObjectsWithRelatedEvents should contains Buffer Management BizObj",
					new[] { (BusinessObject)jobHeader, quotedBooking.Quote, quotedBooking.Quote.OneOffQuote.FirstOrDefault() },
					actualBusinessObjectsWithRelatedEvents
				);
			}
		}

		public void TestBusinessObjectsWithRelatedEvents_IncludesJobHeader()
		{
			var quotedBooking = GetQuoteOnlyQuotedBooking();
			var job = new JobHeader.Loader(quotedBooking).TryLoadOrCreate();

			var relatedEvents = (quotedBooking as IStmALogParent).BusinessObjectsWithRelatedEvents;
			Assert(relatedEvents.Contains(job));
		}

		public void TestBusinessObjectsWithRelatedEvents_JobNotRegisterEditableChildObject()
		{
			var quotedBooking = GetQuoteOnlyQuotedBooking();
			var job = new JobHeader.Loader(quotedBooking).TryLoadOrCreate();

			var relatedEvents = (quotedBooking as IStmALogParent).BusinessObjectsWithRelatedEvents;
			Assert(relatedEvents.Contains(job));
			AssertEquals("Job is not registered editable child object.", false, quotedBooking.IsRegisteredEditableChildObject(job));
		}

		public void TestConvertQuoteToQuotedBookingAddsBWQEvent()
		{
			var quotedBooking = GetQuoteOnlyQuotedBooking();
			var logParent = (IStmALogParent)quotedBooking.Quote;
			logParent.Logs.AddNew(Events.QuoteAutoratedWithWiseRates, "Rates Service Usage", ZDateTimeOffset.Now.AddHours(-1), true);
			quotedBooking.Factory.Save();

			quotedBooking.ConvertQuoteToQuotedBooking();

			quotedBooking.Factory.Save();

			var bqwLogs = ((IStmALogParent)quotedBooking.Booking).Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.BookingWithQuoteAutoratedWithRatesServiceCode));
			AssertEquals("RatesServiceUsage:Quote 00001000,Booking S00001000", bqwLogs[0].SL_Reference);

			var quotedBookingNoQAWEvent = GetQuoteOnlyQuotedBooking();
			quotedBookingNoQAWEvent.Factory.Save();

			quotedBookingNoQAWEvent.ConvertQuoteToQuotedBooking();
			quotedBookingNoQAWEvent.Factory.Save();

			Assert("Should not have BQW log", !((IStmALogParent)quotedBookingNoQAWEvent.Booking).Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.BookingWithQuoteAutoratedWithRatesServiceCode)).Any());
		}

		public void TestConvertQuoteToQuotedBookingLogs()
		{
			QuotedBooking quotedBooking = GetQuoteOnlyQuotedBooking();
			quotedBooking.Quote.CurrentOneOffQuote.TT_QuoteApprovedByManager = true;

			JobHeader job = new JobHeader.Loader(quotedBooking).TryLoadOrCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			StmALog quoteLog = quotedBooking.Logs.AddNew(Events.Authorised);

			quotedBooking.Factory.Save();

			quotedBooking.ConvertQuoteToQuotedBooking();

			AssertContainsExactElementsInAnyOrder(new[] { quoteLog }, quotedBooking.Logs.GetAllLogs());

			StmALog convertedQuoteLog = quotedBooking.Logs.AddNew(Events.Booked);

			AssertContainsExactElementsInAnyOrder(new[] { quoteLog, convertedQuoteLog }, quotedBooking.Logs.GetAllLogs());

			quotedBooking.Factory.Save();

			BusinessObjectFactory factory = new BusinessObjectFactory();

			quotedBooking = QuotedBooking.New(quotedBooking.Quote.PK, quotedBooking.Booking.PK, factory);

			AssertContainsExactElementsInAnyOrder(new[] { quoteLog.PK, convertedQuoteLog.PK },
					quotedBooking.Logs.GetAllLogs().Select(log => log.PK));
		}

		public void TestConvertQuoteToQuotedBooking_BookingAlreadyCreatedByAnotherUser()
		{
			var shipmentsCount = Factory.GetDatabaseCount(typeof(ForwardingShipment));

			Factory.RefreshEnabled = false;

			var quotedBooking = GetQuoteOnlyQuotedBooking();
			quotedBooking.Quote.CurrentOneOffQuote.TT_QuoteApprovedByManager = true;
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			otherFactory.RefreshEnabled = false;

			var quotedBookingOtherFactory = QuotedBooking.New(quotedBooking.Quote.PK, ZGuid.Empty, otherFactory);

			quotedBooking.ConvertQuoteToQuotedBooking();
			Factory.Save();

			quotedBookingOtherFactory.ConvertQuoteToQuotedBooking();
			otherFactory.Save();

			AssertEquals("only one booking has been created", shipmentsCount + 1, Factory.GetDatabaseCount(typeof(ForwardingShipment)));
			AssertEquals("bookings match", quotedBooking.Booking.PK, quotedBookingOtherFactory.Booking.PK);
		}

		public void TestConvertQuoteToQuotedBooking_AlreadyConfirmedByAnotherUser()
		{
			Factory.RefreshEnabled = false;

			var quotedBooking = GetQuoteOnlyQuotedBooking();
			quotedBooking.Quote.CurrentOneOffQuote.TT_QuoteApprovedByManager = true;
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			otherFactory.RefreshEnabled = false;

			var quotedBookingOtherFactory = QuotedBooking.New(quotedBooking.Quote.PK, ZGuid.Empty, otherFactory);

			quotedBooking.ConvertQuoteToQuotedBooking();
			quotedBookingOtherFactory.ConvertQuoteToQuotedBooking();

			Factory.Save();

			try
			{
				otherFactory.Save();
				Fail("should throw concurrency error");
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}

			AssertMultilineASCIIEquals("user notification",
					@"This quoted booking cannot be saved because the spot quote has also been converted by another user and saved.

You must cancel your changes and reopen converted booking with quote.",
					UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("other quoted booking hasn't been saved", false, quotedBookingOtherFactory.Booking.IsInDatabase);
		}

		public void TestConvertOneOffQuoteToQuotedBooking_ShouldCopyCarrierServiceLevel()
		{
			QuotedBooking testQuotedBooking = GetQuoteOnlyQuotedBooking();
			testQuotedBooking.Quote.CurrentOneOffQuote.TT_PL_NKCarrierServiceLevel = "CSL";
			testQuotedBooking.ConvertQuoteToQuotedBooking();

			AssertEquals("Carrier's Service Level Should Be Coppied", "CSL", testQuotedBooking.Booking.JS_PL_NKCarrierServiceLevel);
		}

		public void TestConvertOneOffQuoteToQuotedBooking_ShouldCopyNumbers()
		{
			QuotedBooking testQuotedBooking = GetQuoteOnlyQuotedBooking();
			var number = testQuotedBooking.Quote.CurrentOneOffQuote.Numbers.AddNew();
			number.CE_RN_NKCountryCode = "US";
			number.CE_EntryType = "CON";
			number.CE_EntryNum = "CN123456789";
			testQuotedBooking.ConvertQuoteToQuotedBooking();

			CombineAssertions("Numbers Should be Coppied", () =>
			{
				AssertEquals("CN123456789", testQuotedBooking.Booking.Numbers[0].CE_EntryNum);
				AssertEquals("CON", testQuotedBooking.Booking.Numbers[0].CE_EntryType);
				AssertEquals("US", testQuotedBooking.Booking.Numbers[0].CE_RN_NKCountryCode);
			});
		}

		#endregion

		#region Quote Comparison

		public void TestIsInComparisonMode()
		{
			QuotedBooking testQuotedBooking = (QuotedBooking)GetNewBusinessObject();
			AssertEquals(ZBool.False, testQuotedBooking.IsInComparisonMode);

			testQuotedBooking.IsInComparisonMode = ZBool.True;
			AssertEquals(ZBool.True, testQuotedBooking.IsInComparisonMode);

			testQuotedBooking.IsInComparisonMode = ZBool.False;
			AssertEquals(ZBool.False, testQuotedBooking.IsInComparisonMode);
		}

		public void TestQuoteWithIsInComparisonMode()
		{
			QuotedBooking testQuotedBooking = (QuotedBooking)GetNewBusinessObject();
			AssertNotNull(testQuotedBooking.Quote);
			AssertEquals(ZBool.False, testQuotedBooking.IsInComparisonMode);
			AssertEquals(ZBool.False, testQuotedBooking.Quote.IsInComparisonMode);

			testQuotedBooking.IsInComparisonMode = ZBool.True;
			AssertEquals(ZBool.True, testQuotedBooking.IsInComparisonMode);
			AssertEquals(ZBool.True, testQuotedBooking.Quote.IsInComparisonMode);

			testQuotedBooking.IsInComparisonMode = ZBool.False;
			AssertEquals(ZBool.False, testQuotedBooking.IsInComparisonMode);
			AssertEquals(ZBool.False, testQuotedBooking.Quote.IsInComparisonMode);
		}

		public void TestServiceLevelWithIsCompareServiceMode()
		{
			QuotedBooking testQuotedBooking = (QuotedBooking)GetNewBusinessObject();

			testQuotedBooking.IsCompareServiceLevel = ZBool.True;
			AssertEquals(ZBool.True, testQuotedBooking.IsCompareServiceLevel);

			testQuotedBooking.ServiceLevel = testQuotedBooking.ServiceLevels[0].RS_Code;
			AssertEquals(testQuotedBooking.ServiceLevels[0].RS_Code, testQuotedBooking.ServiceLevel);
			AssertEquals(ZBool.False, testQuotedBooking.IsCompareServiceLevel);
		}

		public void TestModeWithIsCompareMode()
		{
			var testQuotedBooking = (QuotedBooking)GetNewBusinessObject();

			testQuotedBooking.Mode = "LSE";
			AssertEquals("AIR", testQuotedBooking.TransportMode);
			AssertEquals("LSE", testQuotedBooking.ContainerMode);
			testQuotedBooking.IsCompareMode = ZBool.True;
			AssertEquals(ZString.Empty, testQuotedBooking.Mode);
			AssertEquals(ZString.Empty, testQuotedBooking.TransportMode);
			AssertEquals(ZString.Empty, testQuotedBooking.ContainerMode);

			testQuotedBooking.Mode = "LSE";
			AssertEquals("AIR", testQuotedBooking.TransportMode);
			AssertEquals("LSE", testQuotedBooking.ContainerMode);
			AssertEquals(ZBool.False, testQuotedBooking.IsCompareMode);
		}

		public void TestTransportModeWithIsCompareMode()
		{
			var testQuotedBooking = (QuotedBooking)GetNewBusinessObject();

			testQuotedBooking.IsCompareMode = ZBool.True;
			AssertEquals(ZString.Empty, testQuotedBooking.TransportMode);

			testQuotedBooking.TransportMode = "AIR";
			AssertEquals(ZString.Empty, testQuotedBooking.TransportMode);

			testQuotedBooking.IsCompareMode = ZBool.False;
			testQuotedBooking.TransportMode = "AIR";
			AssertEquals("AIR", testQuotedBooking.TransportMode);
		}

		public void TestContainerModeWithIsCompareMode()
		{
			var testQuotedBooking = (QuotedBooking)GetNewBusinessObject();

			testQuotedBooking.IsCompareMode = ZBool.True;
			testQuotedBooking.TransportMode = "SEA";
			AssertEquals(ZString.Empty, testQuotedBooking.ContainerMode);

			testQuotedBooking.TransportMode = "SEA";
			testQuotedBooking.ContainerMode = "FCL";
			AssertEquals(ZString.Empty, testQuotedBooking.ContainerMode);

			testQuotedBooking.IsCompareMode = ZBool.False;
			testQuotedBooking.TransportMode = "SEA";
			testQuotedBooking.ContainerMode = "FCL";
			AssertEquals("FCL", testQuotedBooking.ContainerMode);
		}

		public void TestComparisonQuoteResults()
		{
			QuotedBooking testQuotedBooking = (QuotedBooking)GetNewBusinessObject();
			AssertNotNull(testQuotedBooking.ComparisonQuoteResults);
			AssertEquals(0, testQuotedBooking.ComparisonQuoteResults.Count);

			ComparisonQuoteResult testQuoteResult = new ComparisonQuoteResult(Factory);
			testQuotedBooking.ComparisonQuoteResults.Add(testQuoteResult);
			AssertEquals(1, testQuotedBooking.ComparisonQuoteResults.Count);
			AssertEquals(testQuoteResult, testQuotedBooking.ComparisonQuoteResults[0]);
		}

		public void TestIsCompareMode()
		{
			var testQuotedBooking = (QuotedBooking)GetNewBusinessObject();
			AssertEquals(ZBool.False, testQuotedBooking.IsCompareMode);

			testQuotedBooking.Mode = "LSE";
			AssertEquals("AIR", testQuotedBooking.TransportMode);
			AssertEquals("LSE", testQuotedBooking.ContainerMode);

			testQuotedBooking.IsCompareMode = ZBool.False;
			AssertEquals(ZBool.False, testQuotedBooking.IsCompareMode);
			AssertEquals("AIR", testQuotedBooking.TransportMode);
			AssertEquals("LSE", testQuotedBooking.ContainerMode);

			testQuotedBooking.IsCompareMode = ZBool.True;
			AssertEquals(ZBool.True, testQuotedBooking.IsCompareMode);
			AssertEquals(ZString.Empty, testQuotedBooking.TransportMode);
			AssertEquals(ZString.Empty, testQuotedBooking.ContainerMode);
		}

		public void TestIsCompareModeInfo()
		{
			var testQuotedBooking = (QuotedBooking)GetNewBusinessObject();
			AssertEquals("IsCompareMode", testQuotedBooking.IsCompareModeInfo.Name);
		}

		public void TestIsCompareServiceLevel()
		{
			QuotedBooking testQuotedBooking = (QuotedBooking)GetNewBusinessObject();
			AssertEquals(ZBool.False, testQuotedBooking.IsCompareServiceLevel);

			testQuotedBooking.ServiceLevel = testQuotedBooking.ServiceLevels[0].RS_Code;
			AssertEquals(testQuotedBooking.ServiceLevels[0].RS_Code, testQuotedBooking.ServiceLevel);

			testQuotedBooking.IsCompareServiceLevel = ZBool.False;
			AssertEquals(ZBool.False, testQuotedBooking.IsCompareServiceLevel);
			AssertEquals(testQuotedBooking.ServiceLevels[0].RS_Code, testQuotedBooking.ServiceLevel);

			testQuotedBooking.IsCompareServiceLevel = ZBool.True;
			AssertEquals(ZBool.True, testQuotedBooking.IsCompareServiceLevel);
			AssertEquals(ZString.Empty, testQuotedBooking.ServiceLevel);
		}

		public void TestIsCompareServiceLevelInfo()
		{
			QuotedBooking testQuotedBooking = (QuotedBooking)GetNewBusinessObject();
			AssertEquals("IsCompareServiceLevel", testQuotedBooking.IsCompareServiceLevelInfo.Name);
		}

		public void TestSelectedComparisonModes()
		{
			var testQuotedBooking = (QuotedBooking)GetNewBusinessObject();
			AssertEquals(true, testQuotedBooking.ComparisonModes.Count > 3);

			AssertEquals(0, testQuotedBooking.SelectedComparisonModes.Count);

			testQuotedBooking.ComparisonModes[1].Bool = ZBool.True;
			testQuotedBooking.ComparisonModes[3].Bool = ZBool.True;
			AssertEquals(2, testQuotedBooking.SelectedComparisonModes.Count);
			AssertEquals(testQuotedBooking.ComparisonModes[1].Code, testQuotedBooking.SelectedComparisonModes[0].Code);
			AssertEquals(testQuotedBooking.ComparisonModes[3].Code, testQuotedBooking.SelectedComparisonModes[1].Code);
		}

		public void TestComparisonModes()
		{
			var testQuotedBooking = (QuotedBooking)GetNewBusinessObject();
			AssertEquals(typeof(CodeDescriptionBoolCollection), testQuotedBooking.ComparisonModes.GetType());
			AssertNotEquals(0, testQuotedBooking.ComparisonModes.Count);
			AssertEquals(testQuotedBooking.ModesForWebTracker.Count, testQuotedBooking.ComparisonModes.Count);

			for (int i = 0; i < testQuotedBooking.ComparisonModes.Count; i++)
			{
				AssertEquals(testQuotedBooking.Modes[i].Code, testQuotedBooking.ComparisonModes[i].Code);
				AssertEquals(testQuotedBooking.Modes[i].Description, testQuotedBooking.ComparisonModes[i].Description);
				AssertEquals(ZBool.False, testQuotedBooking.ComparisonModes[i].Bool);
			}
		}

		public void TestSelectedComparisonServiceLevels()
		{
			QuotedBooking testQuotedBooking = (QuotedBooking)GetNewBusinessObject();
			AssertEquals(true, testQuotedBooking.ComparisonServiceLevels.Count > 3);

			AssertEquals(0, testQuotedBooking.SelectedComparisonServiceLevels.Count);

			testQuotedBooking.ComparisonServiceLevels[1].Bool = ZBool.True;
			testQuotedBooking.ComparisonServiceLevels[3].Bool = ZBool.True;
			AssertEquals(2, testQuotedBooking.SelectedComparisonServiceLevels.Count);
			AssertEquals(testQuotedBooking.ComparisonServiceLevels[1].Code, testQuotedBooking.SelectedComparisonServiceLevels[0].Code);
			AssertEquals(testQuotedBooking.ComparisonServiceLevels[3].Code, testQuotedBooking.SelectedComparisonServiceLevels[1].Code);
		}

		public void TestComparisonServiceLevels()
		{
			QuotedBooking testQuotedBooking = (QuotedBooking)GetNewBusinessObject();
			AssertEquals(typeof(CodeDescriptionBoolCollection), testQuotedBooking.ComparisonServiceLevels.GetType());
			AssertNotEquals(0, testQuotedBooking.ComparisonServiceLevels.Count);
			AssertEquals(testQuotedBooking.ServiceLevels.Count, testQuotedBooking.ComparisonServiceLevels.Count);

			for (int i = 0; i < testQuotedBooking.ComparisonServiceLevels.Count; i++)
			{
				AssertEquals(testQuotedBooking.ServiceLevels[i].RS_Code, testQuotedBooking.ComparisonServiceLevels[i].Code);
				AssertEquals(testQuotedBooking.ServiceLevels[i].RS_DescriptionMultilingual, testQuotedBooking.ComparisonServiceLevels[i].Description);
				AssertEquals(ZBool.False, testQuotedBooking.ComparisonServiceLevels[i].Bool);
			}
		}

		#endregion

		#region Carrier Synchronisation

		public void TestOH_CarrierSynchronisation()
		{
			OrgHeader carrier = Factory.New<OrgHeader>();

			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_OA_BookedShippingLineAddress = carrier.MainAddress.PK;

			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);

			TestQuotedBookingExposer qb = new TestQuotedBookingExposer(quote.PK, booking.PK, Factory);
			qb.CheckAndCopyBookingValuesToQuoteIfRequired();

			AssertEquals(carrier.PK, quote.CurrentOneOffQuote.TT_OH_Carrier);

			booking.JS_OA_BookedShippingLineAddress = ZGuid.Empty;

			qb.CopyQuoteValuesToBooking();
			AssertEquals(carrier.MainAddress.PK, booking.JS_OA_BookedShippingLineAddress);
		}

		public void TestOH_Carrier()
		{
			var carrier = Factory.New<OrgHeader>();
			var carrier2 = Factory.New<OrgHeader>();
			var newPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_OA_BookedShippingLineAddress = carrier.MainAddress.PK;

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			quote.CurrentOneOffQuote.TT_OH_Carrier = carrier2.PK;

			var qb = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			AssertEquals(quote.CurrentOneOffQuote.TT_OH_Carrier, qb.OH_Carrier);

			qb.OH_Carrier = newPK;
			AssertEquals(newPK, quote.CurrentOneOffQuote.TT_OH_Carrier);

			qb = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			AssertEquals(booking.JS_OA_BookedShippingLineAddress, qb.Carrier.MainAddress.PK);
			qb.OH_Carrier = newPK;
			AssertEquals(newPK, booking.BookedShippingLine.PK);

			var att = (ListAttribute)typeof(QuotedBooking).GetProperty(QuotedBooking.Schema.OH_Carrier).GetCustomAttributes(typeof(ListAttribute), false)[0];
			AssertEquals("Carriers", att.ListDataSourceMember);
		}

		public void TestOH_Creditor()
		{
			var creditor = Factory.New<OrgHeader>();
			var newPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			var booking = QuotedBooking.CreateNewBooking(Factory);

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			quote.CurrentOneOffQuote.TT_OH_Creditor = creditor.PK;

			var qb = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			AssertEquals(quote.CurrentOneOffQuote.TT_OH_Creditor, qb.Creditor);

			qb.Creditor = newPK;
			AssertEquals(newPK, quote.CurrentOneOffQuote.TT_OH_Creditor);

			qb = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			qb.OH_Carrier = newPK;
			AssertEquals(newPK, booking.BookedShippingLine.PK);

			var att = (ListAttribute)typeof(QuotedBooking).GetProperty(QuotedBooking.Schema.Creditor).GetCustomAttributes(typeof(ListAttribute), false)[0];
			AssertEquals("Creditors", att.ListDataSourceMember);
		}

		public void TestOOQConvertToBWQ_WhenConvertOOQWithCarrier_ThenCarrierInformationShouldBeCopiedToBWQ()
		{
			var carrier = Factory.New<OrgHeader>();
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			quote.CurrentOneOffQuote.TT_OH_Carrier = carrier.PK;
			var quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
			quotedBooking.ConvertQuoteToQuotedBooking();

			AssertEquals("Carrier information should be copied to BWQ when convert OOQ with Carrier", quotedBooking.Booking.JS_OA_BookedShippingLineAddress, carrier.MainAddress.PK);
		}

		#endregion

		#region JobHeader

		public void TestQuoteNumberCopiedWhenJobCreating()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			quote.TH_QuoteNumber = "234234";
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking.New(quote.PK, booking.PK, Factory);
			AssertNull(booking.Job);
			booking.CreateShipmentJobHeaderWithMutex();
			AssertEquals("234234", booking.Job.JH_TH_NKQuoteNumber);
			booking.Job.Dispose();
		}

		public void TestOnJobCreating()
		{
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);

			var cartage = (BusinessObject)Factory.New<ICommonCartage>();
			Factory.Save();

			cartage[JobCartageSchema.JJ_ParentID] = quotedBooking.Booking.PK;
			cartage[JobCartageSchema.JJ_ParentTableCode] = quotedBooking.Booking.TablePrefix;
			new JobHeader.Loader((IJobHeaderParent)cartage).TryLoadOrCreateWithoutMutexForTestOnly();
			AssertNotNull(((ICommonCartage)cartage).Job);
			Assert(((ICommonCartage)cartage).Job.JH_JH_ParentJob.IsEmpty);
			Assert(((ICommonCartage)cartage).Job.JH_OA_LocalChargesAddr.IsEmpty);
			Factory.Save();

			new JobHeader.Loader(quotedBooking).TryLoadOrCreateWithoutMutexForTestOnly();
			AssertEquals(quotedBooking.Booking.Job.PK, ((ICommonCartage)cartage).Job.JH_JH_ParentJob);
		}

		public void TestOnJobCreating_QuotedBooking()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = CreateNewQuotedBooking(quote.PK, booking.PK);

			var cartage = (BusinessObject)Factory.New<ICommonCartage>();
			cartage[JobCartageSchema.JJ_ParentID] = quotedBooking.Booking.PK;
			cartage[JobCartageSchema.JJ_ParentTableCode] = quotedBooking.Booking.TablePrefix;
			new JobHeader.Loader((IJobHeaderParent)cartage).TryLoadOrCreateWithoutMutexForTestOnly();
			AssertNotNull(((ICommonCartage)cartage).Job);
			Assert(((ICommonCartage)cartage).Job.JH_JH_ParentJob.IsEmpty);
			Assert(((ICommonCartage)cartage).Job.JH_OA_LocalChargesAddr.IsEmpty);

			new JobHeader.Loader(quotedBooking).TryLoadOrCreateWithoutMutexForTestOnly();
			AssertEquals("Booking has no job at this stage", ZGuid.Empty, ((ICommonCartage)cartage).Job.JH_JH_ParentJob);
		}

		public void TestOnJobCreating_QuoteOnly()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			QuotedBooking quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
			new JobHeader.Loader(quotedBooking).TryLoadOrCreateWithoutMutexForTestOnly();
			AssertNotNull("and no exception", quotedBooking.Job);
		}

		public void TestGetPropertiesDontCreateJobHeader()
		{
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			quotedBooking.Mode = Core.Constants.RateMode.FCL;
			AssertNull(quotedBooking.Job);
			Factory.Save();

			ZGuid clientPK = quotedBooking.ClientPK;
			ZString clintName = quotedBooking.ClientFullName;
			AssertNull(quotedBooking.Job);
		}

		public void TestAccessingBooking()
		{
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);

			AssertEquals("Accessing the property 1st time", booking, quotedBooking.Booking);
			AssertEquals("Accessing the property 2nd time", booking, quotedBooking.Booking);

			booking.Delete();

			AssertNull("Accessing deleted booking", quotedBooking.Booking);
		}

		public void TestBookingJobHeaderHasNoDefaultCharges()
		{
			GlbDepartment department = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FIA");
			department.DeptCharges.AddNew();

			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			quote.CurrentOneOffQuote.TT_RL_NKReceivalLocation = "USLAX";
			quote.CurrentOneOffQuote.TT_RL_NKDeliveryLocation = "AUSYD";
			quote.CurrentOneOffQuote.TT_TransportMode = "AIR";
			quote.CurrentOneOffQuote.TT_ContainerMode = "LSE";

			QuotedBooking quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
			try
			{
				quotedBooking.TryLoadOrCreateJob();
				AssertEquals(1, Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, quotedBooking.Job.PK)).Length);
				quotedBooking.ConvertQuoteToQuotedBooking();
				quotedBooking.TryLoadOrCreateJob();
				AssertEquals(1, Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, quotedBooking.Job.PK)).Length);
			}
			finally
			{
				DisposeJobs(quotedBooking);
			}
		}

		public void TestQuoteJobValuesCopiedToBookingJob()
		{
			GlbDepartment department = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FEA");

			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			quote.CurrentOneOffQuote.TT_RL_NKReceivalLocation = "USLAX";
			quote.CurrentOneOffQuote.TT_RL_NKDeliveryLocation = "AUSYD";
			quote.CurrentOneOffQuote.TT_TransportMode = "AIR";
			quote.CurrentOneOffQuote.TT_ContainerMode = "LSE";

			QuotedBooking quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
			quotedBooking.TryLoadOrCreateJob();

			Assert("Precondition", quotedBooking.Job.JH_GE != department.PK);
			Assert("Precondition", quotedBooking.Job.JH_GB != GlbBranch.CurrentBranch.PK);
			quotedBooking.Job.JH_GE = department.PK;
			quotedBooking.Job.JH_GB = GlbBranch.CurrentBranch.PK;

			Factory.Save();

			try
			{
				quotedBooking.ConvertQuoteToQuotedBooking();
				AssertEquals(department.PK, quotedBooking.Job.JH_GE);
				AssertEquals(GlbBranch.CurrentBranch.PK, quotedBooking.Job.JH_GB);
			}
			finally
			{
				DisposeJobs(quotedBooking);
			}
		}

		public void TestOnJobCreatingEvent()
		{
			QuotedBooking quotedBooking = QuotedBooking.New(Integration.QuoteBookingType.BookingWithQuote, Factory);
			quotedBooking.JobCreating += (s, e) =>
			{
				Assert("Event was raised", true);
			};

			((IJobHeaderParent)quotedBooking).OnJobCreating(Factory.NewJobForTesting<JobHeader>());
		}

		public void TestOnJobDeletingEvent()
		{
			QuotedBooking quotedBooking = QuotedBooking.New(Integration.QuoteBookingType.BookingWithQuote, Factory);
			quotedBooking.JobDeleting += (s, e) =>
			{
				Assert("Event was raised", true);
			};

			((IJobHeaderParent)quotedBooking).OnJobDeleting(Factory.NewJobForTesting<JobHeader>());
		}

		public void TestOnJobCreatedEvent()
		{
			QuotedBooking quotedBooking = QuotedBooking.New(Integration.QuoteBookingType.BookingWithQuote, Factory);
			quotedBooking.JobCreated += (s, e) =>
			{
				Assert("Event was raised", true);
			};

			((IJobHeaderParent)quotedBooking).OnJobCreated(Factory.NewJobForTesting<JobHeader>());
		}

		public void TestJobInvoicingSupporter()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var bookingWithQuote = new TestQuotedBookingExposer(quote.PK, booking.PK, Factory);

			var bookingWithQuoteSupporter = bookingWithQuote.InvoicingSupporter;

			Assert(bookingWithQuoteSupporter is BookingInvoicingSupporter);

			var quickBooking = new TestQuotedBookingExposer(ZGuid.Empty, QuotedBooking.CreateNewBooking(Factory).PK, Factory);
			var quickBookingSupporter = quickBooking.InvoicingSupporter;

			Assert(quickBookingSupporter is BookingInvoicingSupporter);

			quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quoteOnly = new TestQuotedBookingExposer(quote.PK, ZGuid.Empty, Factory);
			var quoteOnlySupporter = quoteOnly.InvoicingSupporter;

			Assert(quoteOnlySupporter is QuoteInvoicingSupporter);
		}

		#endregion

		#region Buyer / Supplier / Buyer Links

		public void TestSupplierBuyerLink()
		{
			OrgHeader deliveryCartage = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader pickupCartage = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader carrier = Factory.NewWithValidTestData<OrgHeader>();

			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_RL_NKClosestPort = "USLGB";
			OrgContact consigneeContact1 = consignee.Contacts.AddNew();
			consigneeContact1.OC_ContactName = "ConsigneeContact1";

			OrgContact consigneeContact2 = consignee.Contacts.AddNew();
			consigneeContact2.OC_ContactName = "ConsigneeContact2";

			OrgAddress consigneeAddress1 = consignee.Addresses.AddNew();
			consigneeAddress1.OA_Code = "ConsigneeAddress1";
			consigneeAddress1.OA_Address1 = "ConsigneeAddress1";

			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact consignorContact1 = consignor.Contacts.AddNew();
			consignorContact1.OC_ContactName = "ConsignorContact1";

			OrgAddress consignorAddress1 = consignor.Addresses.AddNew();
			consignorAddress1.OA_Code = "ConsignorAddress1";
			consignorAddress1.OA_Address1 = "ConsignorAddress1";

			OrgSupplierBuyerLink link = consignee.SupplierLinks.AddNew(consignor);

			OrgSupBuyLinkTrnMode linkDetails = link.OrgSupBuyLinkTrnModes.AddNew();
			linkDetails.PF_TransportMode = Core.Constants.TransportModes.Air;
			linkDetails.PF_ContainerMode = Core.Constants.ContainerModes.Loose;
			linkDetails.PF_IncoTerm = "CIF";
			linkDetails.PF_RL_NKPlaceOfDeliveryPort = "USLAX";
			linkDetails.PF_RL_NKPlaceOfReceivalPort = "AUSYD";
			linkDetails.PF_RS_NKDefaultServiceLevel = "DIR";
			linkDetails.PF_OC_OverrideConsigneeContact = consigneeContact1.PK;
			linkDetails.PF_OC_OverrideNotifyParty = consigneeContact2.PK;
			linkDetails.PF_OC_OverrideSupplierContact = consignorContact1.PK;

			linkDetails.PF_OA_OverrideDeliveryAddress = consigneeAddress1.PK;
			linkDetails.PF_OA_OverridePickupAddress = consignorAddress1.PK;
			linkDetails.PF_OH_DeliveryCartageContractor = deliveryCartage.PK;
			linkDetails.PF_OH_PickupCartageContractor = pickupCartage.PK;
			linkDetails.PF_OH_CarrierLine = carrier.PK;

			Factory.Save();

			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = CreateNewQuotedBooking(quote.PK, booking.PK);

			try
			{
				quotedBooking.Mode = Core.Constants.RateMode.LSE;
				quotedBooking.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;

				AssertEquals(consignor.PK, quotedBooking.ConsignorDocumentaryAddress.OrganisationPK);
				AssertEquals("CIF", quotedBooking.PaymentTerms);
				AssertEquals("USLAX", quotedBooking.Destination);
				AssertEquals("AUSYD", quotedBooking.Origin);
				AssertEquals("DIR", quotedBooking.ServiceLevel);
				AssertEquals("ConsigneeContact1", ((IBuyerSupplierRelationshipConsumer)quotedBooking).ConsigneeDeliveryAddress.Contact.OC_ContactName);
				AssertEquals("ConsignorContact1", ((IBuyerSupplierRelationshipConsumer)quotedBooking).ConsignorPickupAddress.Contact.OC_ContactName);

				AssertEquals(consigneeAddress1.PK, ((IBuyerSupplierRelationshipConsumer)quotedBooking).ConsigneeDeliveryAddress.E2_OA_Address);
				AssertEquals(consignorAddress1.PK, ((IBuyerSupplierRelationshipConsumer)quotedBooking).ConsignorPickupAddress.E2_OA_Address);

				AssertEquals(deliveryCartage.PK, ((IBuyerSupplierRelationshipConsumer)quotedBooking).DeliveryCartageCoPK);
				AssertEquals(pickupCartage.PK, ((IBuyerSupplierRelationshipConsumer)quotedBooking).PickupCartageCoPK);
				AssertEquals(carrier.PK, quotedBooking.OH_Carrier);
			}
			finally
			{
				DisposeJobs(quotedBooking);
			}
		}

		public void TestSupplierBuyerLink_SpotQuote()
		{
			FreightDataRegistry.Instance.DefaultShipmentDestinationFromConsolDischarge.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			OrgHeader deliveryCartage = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader pickupCartage = Factory.NewWithValidTestData<OrgHeader>();

			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_RL_NKClosestPort = "USGLB";
			OrgContact consigneeContact1 = consignee.Contacts.AddNew();
			consigneeContact1.OC_ContactName = "ConsigneeContact1";

			OrgContact consigneeContact2 = consignee.Contacts.AddNew();
			consigneeContact2.OC_ContactName = "ConsigneeContact2";

			OrgAddress consigneeAddress1 = consignee.Addresses.AddNew();
			consigneeAddress1.OA_Code = "ConsigneeAddress1";
			consigneeAddress1.OA_Address1 = "ConsigneeAddress1";

			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact consignorContact1 = consignor.Contacts.AddNew();
			consignorContact1.OC_ContactName = "ConsignorContact1";

			OrgAddress consignorAddress1 = consignor.Addresses.AddNew();
			consignorAddress1.OA_Code = "ConsignorAddress1";
			consignorAddress1.OA_Address1 = "ConsignorAddress1";

			OrgSupplierBuyerLink link = consignee.SupplierLinks.AddNew(consignor);

			OrgSupBuyLinkTrnMode linkDetails = link.OrgSupBuyLinkTrnModes.AddNew();
			linkDetails.PF_TransportMode = Core.Constants.TransportModes.Air;
			linkDetails.PF_ContainerMode = Core.Constants.ContainerModes.Loose;
			linkDetails.PF_IncoTerm = "CIF";
			linkDetails.PF_RL_NKPlaceOfDeliveryPort = "USLAX";
			linkDetails.PF_RL_NKPlaceOfReceivalPort = "AUSYD";
			linkDetails.PF_RS_NKDefaultServiceLevel = "DIR";
			linkDetails.PF_OC_OverrideConsigneeContact = consigneeContact1.PK;
			linkDetails.PF_OC_OverrideNotifyParty = consigneeContact2.PK;
			linkDetails.PF_OC_OverrideSupplierContact = consignorContact1.PK;

			linkDetails.PF_OA_OverrideDeliveryAddress = consigneeAddress1.PK;
			linkDetails.PF_OA_OverridePickupAddress = consignorAddress1.PK;
			linkDetails.PF_OH_DeliveryCartageContractor = deliveryCartage.PK;
			linkDetails.PF_OH_PickupCartageContractor = pickupCartage.PK;

			Factory.Save();

			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			QuotedBooking quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);

			quotedBooking.Mode = Core.Constants.RateMode.LSE;
			quotedBooking.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;

			AssertEquals(consignor.PK, quotedBooking.ConsignorDocumentaryAddress.OrganisationPK);
			AssertEquals("CIF", quotedBooking.PaymentTerms);
			AssertEquals("USLAX", quotedBooking.Destination);
			AssertEquals("AUSYD", quotedBooking.Origin);
			AssertEquals("DIR", quotedBooking.ServiceLevel);
		}

		public void TestSaveBuyerSupplierRelationships()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = CreateNewQuotedBooking(quote.PK, booking.PK);
			booking.CreateShipmentJobHeaderWithMutex();
			booking.ShipmentJobHeader.JH_GE = Env.CurrentDepartment.PK;

			quotedBooking.ConsigneeDocumentaryAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			quotedBooking.ConsignorDocumentaryAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			Factory.Save();

			AssertEquals("Prompt to save relationship", false, quotedBooking.BuyerSupplierLinksHelper.ShouldPromptToSaveSupplierBuyerRelationship);
		}

		public void TestCopyQuoteValuesToBooking_DontOverwriteConsignorConsignee_WhenExistsInBookingAndNotInQuote()
		{
			QuotedBooking testQuotedBooking = GetQuoteOnlyQuotedBooking();
			testQuotedBooking.Factory.Save();
			testQuotedBooking.ConvertQuoteToQuotedBooking();

			var consignorOrg = Factory.NewWithValidTestData<OrgHeader>();
			var consignorOrgAddress = consignorOrg.Addresses.AddNew();
			consignorOrgAddress.Address1 = "Pick up address 1";
			consignorOrgAddress.AddressCode = "Pickup Point";
			consignorOrgAddress.OA_RL_NKRelatedPortCode = "AUMEL";
			consignorOrgAddress.AddAddressType(OrgAddressType.Pickup);
			testQuotedBooking.ConsignorDocumentaryAddress.OrganisationPK = consignorOrg.PK;

			var consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			var consigneeOrgAddress = consigneeOrg.Addresses.AddNew();
			consigneeOrgAddress.Address1 = "pickup address 1";
			consigneeOrgAddress.AddressCode = "Delivery Point";
			consigneeOrgAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			consigneeOrgAddress.AddAddressType(OrgAddressType.Delivery);
			testQuotedBooking.ConsigneeDocumentaryAddress.OrganisationPK = consigneeOrg.PK;

			testQuotedBooking.CopyQuoteValuesToBooking();

			AssertEquals(consignorOrg.PK, testQuotedBooking.ConsignorDocumentaryAddress.OrganisationPK);
			AssertEquals(consigneeOrg.PK, testQuotedBooking.ConsigneeDocumentaryAddress.OrganisationPK);
		}

		public void TestCopyQuoteValuesToBooking_CopyAddressesFromQuote_WhenTransportModeChanges()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CONSIGNOR";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CONSIGNEE";

			Factory.Save();

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			quote.CurrentOneOffQuote.TT_TransportMode = Constants.TransportModes.Sea;
			quote.CurrentOneOffQuote.TT_ContainerMode = Constants.ContainerModes.FCL;
			quote.CurrentOneOffQuote.PickUpDocAddress.OrganisationPK = consignor.PK;
			quote.CurrentOneOffQuote.DeliveryDocAddress.OrganisationPK = consignee.PK;
			Factory.Save();

			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_TransportMode = Constants.TransportModes.Air;
			booking.JS_PackingMode = Constants.ContainerModes.Loose;
			Factory.Save();

			var quotedBooking = CreateNewQuotedBooking(quote.PK, booking.PK);

			try
			{
				Assert("Transport mode discrepancy exists", quotedBooking.HasDiscrepancyFor(QuotedBooking.DiscrepancyCheck.TransportMode));
				Assert("Container mode discrepancy exists", quotedBooking.HasDiscrepancyFor(QuotedBooking.DiscrepancyCheck.ContainerMode));
				Assert("Pickup address discrepancy exists", quotedBooking.HasDiscrepancyFor(QuotedBooking.DiscrepancyCheck.Pickup));
				Assert("Delivery address discrepancy exists", quotedBooking.HasDiscrepancyFor(QuotedBooking.DiscrepancyCheck.Delivery));

				quotedBooking.CopyQuoteValuesToBooking();

				AssertEquals("Transport mode should be copied from quote", Constants.TransportModes.Sea, booking.JS_TransportMode);
				AssertEquals("Container mode should be copied from quote", Constants.ContainerModes.FCL, booking.JS_PackingMode);
				AssertEquals("Consignor address should be copied from quote", consignor.PK, booking.ConsignorDocumentaryAddress.OrganisationPK);
				AssertEquals("Consignee address should be copied from quote", consignee.PK, booking.ConsigneeDocumentaryAddress.OrganisationPK);
				AssertEquals("Pickup address should be copied from quote", consignor.PK, booking.ConsignorPickupAddress.OrganisationPK);
				AssertEquals("Delivery address should be copied from quote", consignee.PK, booking.ConsigneeDeliveryAddress.OrganisationPK);
			}
			finally
			{
				DisposeJobs(quotedBooking);
			}
		}

		public void TestJobDeclarationDocumentSupporterHooksUpGetDocumentLogin()
		{
			var menuItem = Factory.New<DocumentCommand>();
			menuItem.SU_MenuPath = "menu/path";
			menuItem.SU_MenuName = "name";
			menuItem.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.CNH);
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TestOrg";
			org.CompanyData.OB_IsDebtor = ZBool.True;
			org.CompanyData.OB_AROnCreditHold = ZBool.True;
			Factory.Save();
			Assert("PreCondition - IsCreditOnHold", org.CreditChecker.IsCreditOnHold());
			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_UniqueConsignRef = "S0001";
			booking.ConsigneeDocumentaryAddress.E2_OA_Address = org.MainAddress.PK;
			var declaration = Factory.New<AU.IJobDeclaration>();
			declaration.JE_JS = booking.PK;
			declaration.JE_OH_Supplier = org.PK;
			booking.JS_ScreeningStatus = "UNK";
			var documentLoginCalled = false;
			((ICreditControlledDocumentDelivery)booking).GetDocumentLogin += (object sender, SecurityLoginEventArgs e) => documentLoginCalled = true;
			booking.DocumentSupporter.GetDataStateBeforeRun(menuItem);
			Assert("DocumentLogin should be called on the Booking, because this is where DocumentDeliveryCreditControlManager adds the event handler.", documentLoginCalled);
		}

		public void TestImportBrokerSetThroughSupplierBuyerLinkWhenChangingConsignorOnNewBooking()
		{
			var linkBroker = Factory.NewWithValidTestData<OrgHeader>();

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_RL_NKClosestPort = "USLGB";

			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			var link = consignor.BuyerLinks.AddNew();
			link.OL_OH_Buyer = consignee.PK;
			link.OL_OH_ImportBroker = linkBroker.PK;

			var linkDetails = link.OrgSupBuyLinkTrnModes.AddNew();
			linkDetails.PF_TransportMode = Constants.TransportModes.Air;
			linkDetails.PF_ContainerMode = Constants.ContainerModes.Loose;

			Factory.Save();

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(quote.PK, booking.PK);

			try
			{
				quotedBooking.Mode = Core.Constants.RateMode.LSE;
				quotedBooking.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;

				AssertEquals(linkBroker.PK, booking.JS_OH_ImportBroker);
			}
			finally
			{
				DisposeJobs(quotedBooking);
			}
		}

		public void TestImportBrokerDoesSetThroughSupplierBuyerLinkWhenChangingConsignorOnSavedBooking()
		{
			var linkBroker = Factory.NewWithValidTestData<OrgHeader>();

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_RL_NKClosestPort = "USLGB";

			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			var link = consignor.BuyerLinks.AddNew();
			link.OL_OH_Buyer = consignee.PK;
			link.OL_OH_ImportBroker = linkBroker.PK;

			var linkDetails = link.OrgSupBuyLinkTrnModes.AddNew();
			linkDetails.PF_TransportMode = Constants.TransportModes.Air;
			linkDetails.PF_ContainerMode = Constants.ContainerModes.Loose;

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(quote.PK, booking.PK);

			Factory.Save();

			try
			{
				quotedBooking.Mode = Core.Constants.RateMode.LSE;
				quotedBooking.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;

				AssertEquals(ZGuid.Empty, booking.JS_OH_ImportBroker);
			}
			finally
			{
				DisposeJobs(quotedBooking);
			}
		}

		public void TestDefaultImportBrokerLoadsFromBuyerSupplierLinksHelperForQuickBooking()
		{
			var linkBroker = Factory.NewWithValidTestData<OrgHeader>();

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_RL_NKClosestPort = "USLGB";

			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			var link = consignor.BuyerLinks.AddNew();
			link.OL_OH_Buyer = consignee.PK;
			link.OL_OH_ImportBroker = linkBroker.PK;

			var linkDetails = link.OrgSupBuyLinkTrnModes.AddNew();
			linkDetails.PF_TransportMode = Constants.TransportModes.Air;
			linkDetails.PF_ContainerMode = Constants.ContainerModes.Loose;

			Factory.Save();

			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);

			try
			{
				quotedBooking.Mode = Core.Constants.RateMode.LSE;
				quotedBooking.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
				quotedBooking.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;

				AssertEquals(linkBroker.PK, quotedBooking.Booking.JS_OH_ImportBroker);
			}
			finally
			{
				DisposeJobs(quotedBooking);
			}
		}

		public void TestDefaultImportBrokerLoadsFromBuyerSupplierLinksHelperForQuotedBooking()
		{
			var linkBroker = Factory.NewWithValidTestData<OrgHeader>();

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_RL_NKClosestPort = "USLGB";

			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			var link = consignor.BuyerLinks.AddNew();
			link.OL_OH_Buyer = consignee.PK;
			link.OL_OH_ImportBroker = linkBroker.PK;

			var linkDetails = link.OrgSupBuyLinkTrnModes.AddNew();
			linkDetails.PF_TransportMode = Constants.TransportModes.Air;
			linkDetails.PF_ContainerMode = Constants.ContainerModes.Loose;

			Factory.Save();

			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			var quotedBooking = CreateNewQuotedBooking(quote.PK, booking.PK);

			try
			{
				quotedBooking.Mode = Core.Constants.RateMode.LSE;
				quotedBooking.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
				quotedBooking.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;

				AssertEquals(linkBroker.PK, quotedBooking.Booking.JS_OH_ImportBroker);
			}
			finally
			{
				DisposeJobs(quotedBooking);
			}
		}

		public void TestDefaultImportBrokerPopulatedWhenConvertingOOQToQuotedBooking()
		{
			var importBroker = Factory.NewWithValidTestData<OrgHeader>();
			importBroker.OH_Code = "IMPORTBROKER";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CONSIGNEE";
			consignee.OH_RL_NKClosestPort = "USLGB";

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CONSIGNOR";

			var link = consignor.BuyerLinks.AddNew();
			link.OL_OH_Buyer = consignee.PK;
			link.OL_OH_ImportBroker = importBroker.PK;

			var linkDetails = link.OrgSupBuyLinkTrnModes.AddNew();
			linkDetails.PF_TransportMode = Constants.TransportModes.Air;
			linkDetails.PF_ContainerMode = Constants.ContainerModes.Loose;

			Factory.Save();

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			quote.CurrentOneOffQuote.TT_TransportMode = Constants.TransportModes.Air;
			quote.CurrentOneOffQuote.TT_ContainerMode = Constants.ContainerModes.Loose;
			quote.CurrentOneOffQuote.PickUpDocAddress.OrganisationPK = consignor.PK;
			quote.CurrentOneOffQuote.DeliveryDocAddress.OrganisationPK = consignee.PK;

			Factory.Save();
			var booking = QuotedBooking.CreateNewBooking(Factory);

			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);

			try
			{
				quotedBooking.ConvertQuoteToQuotedBooking();

				AssertEquals("Consignor address should be set correctly",
					consignor.PK,
					quotedBooking.Booking.ConsignorDocumentaryAddress.OrganisationPK);
				AssertEquals("Consignee address should be set correctly",
					consignee.PK,
					quotedBooking.Booking.ConsigneeDocumentaryAddress.OrganisationPK);

				AssertEquals("Transport mode should be copied correctly",
					Constants.TransportModes.Air,
					quotedBooking.Booking.JS_TransportMode);
				AssertEquals("Container mode should be copied correctly",
					Constants.ContainerModes.Loose,
					quotedBooking.Booking.JS_PackingMode);

				AssertEquals("Import broker should be populated from buyer-supplier link",
					importBroker.PK,
					quotedBooking.Booking.JS_OH_ImportBroker);
			}
			finally
			{
				if (quotedBooking != null)
				{
					DisposeJobs(quotedBooking);
				}
			}
		}

		public void TestMultipleBuyerSupplierLink()
		{
			OrgHeader consignee1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignee2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignor1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignor2 = Factory.NewWithValidTestData<OrgHeader>();

			consignee1.SupplierLinks.AddNew(consignor1);
			consignee1.SupplierLinks.AddNew(consignor2);
			consignee2.SupplierLinks.AddNew(consignor1);
			consignee2.SupplierLinks.AddNew(consignor2);

			Factory.Save();

			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = CreateNewQuotedBooking(quote.PK, booking.PK);

			Assert("Filter contains related consignor property", quotedBooking.Consignee_List.FilterBusinessObjectDefaults.ContainsDefaultFor("Consignee - Related Consignor:Property"));
			Assert("Filter contains related consignee property", quotedBooking.Consignor_List.FilterBusinessObjectDefaults.ContainsDefaultFor("Consignor - Related Consignee:Property"));
		}

		public void TestConsigneeOrConsignorCanNotBeSetWhenDocumentaryAddressIsReadOnly()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = CreateNewQuotedBooking(quote.PK, booking.PK);

			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CONSIGNOR";

			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CONSIGNEE";

			((IBuyerSupplierRelationshipConsumer)quotedBooking).Consignor = consignor;
			((IBuyerSupplierRelationshipConsumer)quotedBooking).Consignee = consignee;

			AssertEquals(consignor, quotedBooking.Consignor);
			AssertEquals(consignee, quotedBooking.Consignee);

			OrgHeader newConsignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "NewCONSIGNOR";

			OrgHeader newConsignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "NewCONSIGNEE";

			quotedBooking.ConsignorDocumentaryAddress.ReadOnly = true;
			quotedBooking.ConsigneeDocumentaryAddress.ReadOnly = true;

			((IBuyerSupplierRelationshipConsumer)quotedBooking).Consignor = newConsignor;
			((IBuyerSupplierRelationshipConsumer)quotedBooking).Consignee = newConsignee;

			AssertEquals("consignor should NOT changed as ConsignorDocumentaryAddress is readOnly", consignor, quotedBooking.Consignor);
			AssertEquals("consignee should NOT changed as ConsigneeDocumentaryAddress is readOnly", consignee, quotedBooking.Consignee);
		}

		public void TestDeregisterBuyerSupplierLinkEventsOnShipmentWhenCreatingABooking()
		{
			var deliveryCartage = Factory.NewWithValidTestData<OrgHeader>();
			var pickupCartage = Factory.NewWithValidTestData<OrgHeader>();

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_RL_NKClosestPort = "USLGB";
			var consigneeContact1 = consignee.Contacts.AddNew();
			consigneeContact1.OC_ContactName = "ConsigneeContact1";

			var consigneeContact2 = consignee.Contacts.AddNew();
			consigneeContact2.OC_ContactName = "ConsigneeContact2";

			var consigneeAddress1 = consignee.Addresses.AddNew();
			consigneeAddress1.OA_Code = "ConsigneeAddress1";
			consigneeAddress1.OA_Address1 = "ConsigneeAddress1";

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignorContact1 = consignor.Contacts.AddNew();
			consignorContact1.OC_ContactName = "ConsignorContact1";

			var consignorAddress1 = consignor.Addresses.AddNew();
			consignorAddress1.OA_Code = "ConsignorAddress1";
			consignorAddress1.OA_Address1 = "ConsignorAddress1";

			var link = consignee.SupplierLinks.AddNew(consignor);

			var linkDetails = link.OrgSupBuyLinkTrnModes.AddNew();
			linkDetails.PF_TransportMode = Constants.TransportModes.Air;
			linkDetails.PF_ContainerMode = Constants.ContainerModes.Loose;
			linkDetails.PF_IncoTerm = "CIF";
			linkDetails.PF_RL_NKPlaceOfDeliveryPort = "USLAX";
			linkDetails.PF_RL_NKPlaceOfReceivalPort = "AUSYD";
			linkDetails.PF_RS_NKDefaultServiceLevel = "DIR";
			linkDetails.PF_OC_OverrideConsigneeContact = consigneeContact1.PK;
			linkDetails.PF_OC_OverrideNotifyParty = consigneeContact2.PK;
			linkDetails.PF_OC_OverrideSupplierContact = consignorContact1.PK;

			linkDetails.PF_OA_OverrideDeliveryAddress = consigneeAddress1.PK;
			linkDetails.PF_OA_OverridePickupAddress = consignorAddress1.PK;
			linkDetails.PF_OH_DeliveryCartageContractor = deliveryCartage.PK;
			linkDetails.PF_OH_PickupCartageContractor = pickupCartage.PK;

			Factory.Save();

			var booking = QuotedBooking.CreateNewBooking(Factory);

			booking.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			booking.JS_TransportMode = Constants.TransportModes.Air;
			booking.JS_PackingMode = Core.Constants.RateMode.LSE;

			var consumer = (IBuyerSupplierRelationshipConsumer)booking;

			AssertEquals(ZGuid.Empty, booking.ConsignorDocumentaryAddress.OrganisationPK);
			AssertEquals(string.Empty, consumer.PaymentTerms);
			AssertEquals(string.Empty, consumer.Origin);
			AssertEquals("STD", consumer.ServiceLevel);
			AssertNull(consumer.ConsigneeDeliveryAddress.Contact);
			AssertNull(consumer.ConsignorPickupAddress.Contact);

			AssertEquals(consignee.GetAddressWithFallback(AddressType.DLV).PK, consumer.ConsigneeDeliveryAddress.E2_OA_Address);
			AssertEquals(ZGuid.Empty, consumer.ConsignorPickupAddress.E2_OA_Address);

			AssertEquals(ZGuid.Empty, consumer.DeliveryCartageCoPK);
			AssertEquals(ZGuid.Empty, consumer.PickupCartageCoPK);
		}

		#endregion

		#region ClientDocAddress

		public void TestClientDocAddress()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			QuotedBooking quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);

			AssertNotNull(quotedBooking.ClientDocAddress);

			quotedBooking.ClientDocAddress.OrganisationPK = org.PK;

			AssertEquals(org.PK, quotedBooking.ClientDocAddress.OrganisationPK);
		}

		#endregion

		#region ClientPK

		public void TestClientPK()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);

			try
			{
				quotedBooking.ClientDocAddress.OrganisationPK = org1.PK;

				AssertEquals(org1.PK, quotedBooking.ClientDocAddress.OrganisationPK);
				AssertEquals(org1.PK, quotedBooking.ClientPK);

				quotedBooking = CreateNewQuotedBooking(quote.PK, booking.PK);
				quotedBooking.ClientDocAddress.OrganisationPK = org1.PK;

				AssertEquals(org1.PK, quotedBooking.ClientDocAddress.OrganisationPK);
				AssertEquals(ZGuid.Empty, quotedBooking.ClientPK);

				quotedBooking.TryLoadOrCreateJob();
				quotedBooking.Job.LocalChargesPK = org2.PK;

				AssertEquals(org1.PK, quotedBooking.ClientDocAddress.OrganisationPK);
				AssertEquals(org2.PK, quotedBooking.ClientPK);
			}
			finally
			{
				DisposeJobs(quotedBooking);
			}
		}

		public void TestClientPK_SetsConsignorAndPickup()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = CreateNewQuotedBooking(quote.PK, booking.PK);

			try
			{
				OrgAddress address = Factory.NewWithValidTestData<OrgAddress>();
				quotedBooking.ClientAddrPK = address.PK;
				AssertEquals(address.OA_OH, booking.ConsignorDocumentaryAddress.Address.OA_OH);
				AssertEquals(address.OA_OH, booking.ConsignorPickupAddress.Address.OA_OH);
			}
			finally
			{
				DisposeJobs(quotedBooking);
			}
		}

		public void TestBookingConsignorValidation()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			CommonShipment shipment = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking booking = CreateNewQuotedBooking(quote.PK, shipment.PK);

			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CONSIGNEE";
			consignee.OH_IsConsignee = true;
			consignee.OH_IsConsignor = false;

			booking.ClientPK = consignee.PK;
			AssertHasErrors("CONSIGNEE is not a valid Consignor Organisation", shipment.ConsignorPKInfo);

			quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			shipment = QuotedBooking.CreateNewBooking(Factory);
			booking = CreateNewQuotedBooking(quote.PK, shipment.PK);

			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CONSIGNOR";
			consignor.OH_IsConsignor = true;

			booking.ClientPK = consignor.PK;
			AssertNoErrors("CONSIGNOR is a valid Consignor Organisation", shipment.ConsignorPKInfo);
		}

		public void TestSpotQuoteConsignorValidation()
		{
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CONSIGNEE";
			consignee.OH_IsConsignee = true;
			consignee.OH_IsConsignor = false;

			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			QuotedBooking quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
			quotedBooking.ClientPK = consignee.PK;

			AssertHasErrors("CONSIGNEE is not a valid Organisation", quote.CurrentOneOffQuote.PickUpDocAddress.OrganisationPKInfo);

			quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			QuotedBooking booking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);

			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CONSIGNOR";
			consignor.OH_IsConsignor = true;

			booking.ClientPK = consignor.PK;
			AssertNoErrors("CONSIGNOR is a valid Consignor Organisation", quote.CurrentOneOffQuote.PickUpDocAddress.OrganisationNameOrPKInfo);
		}

		#endregion

		#region ClientName

		public void TestClientName()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "REAL COMPANY";

			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);

			try
			{
				quotedBooking.ClientDocAddress.E2_AddressOverride = true;
				quotedBooking.ClientDocAddress.E2_CompanyName = "BLAH COMPANY";
				AssertEquals("BLAH COMPANY", quotedBooking.ClientFullName);

				quotedBooking = CreateNewQuotedBooking(quote.PK, booking.PK);
				quotedBooking.ClientPK = org1.PK;
				quotedBooking.TryLoadOrCreateJob();
				quotedBooking.Job.LocalChargesPK = org1.PK;

				AssertEquals("REAL COMPANY", quotedBooking.ClientFullName);
			}
			finally
			{
				DisposeJobs(quotedBooking);
			}
		}

		#endregion

		#region ClientPKInfo

		public void TestClientPKInfo()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);

			Assert(quotedBooking.ClientAddrPKInfo is ZWrappedPropertyInfo);
			AssertEquals(QuotedBooking.Schema.ClientAddrPK, quotedBooking.ClientAddrPKInfo.Name);

			quotedBooking = CreateNewQuotedBooking(quote.PK, booking.PK);

			Assert(!(quotedBooking.ClientAddrPKInfo is ZWrappedPropertyInfo));
			AssertEquals(QuotedBooking.Schema.ClientAddrPK, quotedBooking.ClientAddrPKInfo.Name);
		}

		public void TestClientAddrPK_ZAddressDefaultType()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			var quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);

			AssertEquals("Expected the default address type to be Accounts Receiveable", AddressType.ARM, quotedBooking.ClientAddrPK_ZAddress.DefaultAddressType);

			var booking = QuotedBooking.CreateNewBooking(Factory);
			quotedBooking = CreateNewQuotedBooking(quote.PK, booking.PK);

			AssertEquals("Expected the default address type to be Accounts Receiveable", AddressType.ARM, quotedBooking.ClientAddrPK_ZAddress.DefaultAddressType);
		}

		public void TestClientAddrPK_ZAddress_Instance()
		{
			var orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			var quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
			quotedBooking.ClientAddrPK_ZAddress.OrgPK = orgHeader.PK;
			var hashCode = quotedBooking.ClientAddrPK_ZAddress.GetHashCode();
			AssertEquals("ClientAddrPK_ZAddress should be the same instance", hashCode, quotedBooking.ClientAddrPK_ZAddress.GetHashCode());
		}

		#endregion

		#region BookingPartyDocumentaryAddress

		public void TestBookingPartyDocumentaryAddress()
		{
			var testBooking = QuotedBooking.CreateNewBooking(Factory);
			var testQuotedBooking = CreateNewQuotedBooking(ZGuid.Empty, testBooking.PK);

			AssertNotNull(testQuotedBooking.BookingPartyDocumentaryAddress);
			AssertEquals(testBooking.BookingPartyDocumentaryAddress, testQuotedBooking.BookingPartyDocumentaryAddress);

			var testQuote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			testQuotedBooking = CreateNewQuotedBooking(testQuote.PK, ZGuid.Empty);

			AssertNull(testQuotedBooking.BookingPartyDocumentaryAddress);

			testQuotedBooking = CreateNewQuotedBooking(testQuote.PK, testBooking.PK);
			AssertNotNull(testQuotedBooking.BookingPartyDocumentaryAddress);
			AssertEquals(testBooking.BookingPartyDocumentaryAddress, testQuotedBooking.BookingPartyDocumentaryAddress);
		}

		#endregion

		#region ControllingAgentDocumentaryAddress/ControllingCustomerDocumentaryAddress

		public void TestControllingAgentDocumentaryAddress()
		{
			var testBooking = QuotedBooking.CreateNewBooking(Factory);
			var testQuotedBooking = CreateNewQuotedBooking(ZGuid.Empty, testBooking.PK);

			AssertNotNull(testQuotedBooking.ControllingAgentDocumentaryAddress);
			AssertEquals(testBooking.ControllingAgentDocumentaryAddress, testQuotedBooking.ControllingAgentDocumentaryAddress);

			var testQuote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			testQuotedBooking = CreateNewQuotedBooking(testQuote.PK, ZGuid.Empty);

			AssertNull(testQuotedBooking.ControllingAgentDocumentaryAddress);

			testQuotedBooking = CreateNewQuotedBooking(testQuote.PK, testBooking.PK);
			AssertNotNull(testQuotedBooking.ControllingAgentDocumentaryAddress);
			AssertEquals(testBooking.ControllingAgentDocumentaryAddress, testQuotedBooking.ControllingAgentDocumentaryAddress);
		}

		public void TestControllingCustomerDocumentaryAddress()
		{
			var testBooking = QuotedBooking.CreateNewBooking(Factory);
			var testQuotedBooking = CreateNewQuotedBooking(ZGuid.Empty, testBooking.PK);

			AssertNotNull(testQuotedBooking.ControllingCustomerDocumentaryAddress);
			AssertEquals(testBooking.ControllingCustomerAddress, testQuotedBooking.ControllingCustomerDocumentaryAddress);

			var testQuote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			testQuotedBooking = CreateNewQuotedBooking(testQuote.PK, ZGuid.Empty);

			AssertNull(testQuotedBooking.ControllingCustomerDocumentaryAddress);

			testQuotedBooking = CreateNewQuotedBooking(testQuote.PK, testBooking.PK);
			AssertNotNull(testQuotedBooking.ControllingCustomerDocumentaryAddress);
			AssertEquals(testBooking.ControllingCustomerAddress, testQuotedBooking.ControllingCustomerDocumentaryAddress);
		}

		#endregion

		#region Mode

		public void TestMode()
		{
			ForwardingShipment shipment = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);
			AssertEquals("", quotedBooking.Mode);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals(Core.Constants.RateMode.LCL, quotedBooking.Mode);
		}

		public void TestEmptyTransportModeDoesNotCauseCircularReference()
		{
			ForwardingShipment shipment = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);
			quotedBooking.Mode = "";

			ZQuery query = new ZQuery();
			OrgHeader supplier = Factory.LoadTop1<OrgHeader>(query);

			query.AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, supplier.PK);
			OrgHeader buyer = Factory.LoadTop1<OrgHeader>(query);

			OrgSupplierBuyerLink link = buyer.SupplierLinks.AddNew(supplier);
			OrgSupBuyLinkTrnMode mode = link.OrgSupBuyLinkTrnModes[0];
			mode.PF_TransportMode = "";
			mode.PF_ContainerMode = "";

			quotedBooking.ConsignorDocumentaryAddress.E2_OA_Address = supplier.Addresses.AddNew().PK;
			quotedBooking.ConsigneeDocumentaryAddress.E2_OA_Address = buyer.Addresses.AddNew().PK;

			Assert("Test should not go MIA", true);
			AssertEquals("Consignor is set", quotedBooking.Consignor.PK, supplier.PK);
			AssertEquals("Consignee is set", quotedBooking.Consignee.PK, buyer.PK);
			AssertEquals("Mode is set", quotedBooking.Mode, "");
		}

		public void TestQuoteMode()
		{
			var quotedBooking = GetQuoteOnlyQuotedBooking();
			AssertEquals("", quotedBooking.Mode);

			quotedBooking.Quote.CurrentOneOffQuote.TT_TransportMode = TransportModes.Sea;
			AssertEquals(RateMode.SEA, quotedBooking.Mode);

			quotedBooking.Quote.CurrentOneOffQuote.TT_ContainerMode = ContainerModes.LCL;
			AssertEquals(RateMode.LCL, quotedBooking.Mode);
		}

		public void TestQuoteMode_WhenSupplierBuyerLinkExist_AssignedCorrectly()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_RL_NKClosestPort = "USLGB";

			var link = consignee.SupplierLinks.AddNew(consignor);

			var linkDetails = (OrgSupBuyLinkTrnMode)link.OrgSupBuyLinkTrnModes.First();
			linkDetails.PF_TransportMode = Constants.TransportModes.Sea;
			linkDetails.PF_ContainerMode = Constants.ContainerModes.FCL;

			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			var quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
			quotedBooking.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			quotedBooking.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;

			AssertMode(TransportModes.Air, ContainerModes.Loose, RateMode.LSE);
			AssertMode(TransportModes.Air, ContainerModes.ULD, RateMode.ULD);

			AssertMode(TransportModes.Sea, RateMode.SEA, RateMode.SEA);
			AssertMode(TransportModes.Sea, ContainerModes.FCL, RateMode.FCL);
			AssertMode(TransportModes.Sea, ContainerModes.LCL, RateMode.LCL);

			AssertMode(TransportModes.Road, RateMode.ROA, RateMode.ROA);
			AssertMode(TransportModes.Road, RateMode.LRO, RateMode.LRO);
			AssertMode(TransportModes.Road, ContainerModes.FCL, RateMode.FRO);
			AssertMode(TransportModes.Road, ContainerModes.FTL, RateMode.FTL);

			AssertMode(TransportModes.Rail, RateMode.RAI, RateMode.RAI);
			AssertMode(TransportModes.Rail, RateMode.FWL, RateMode.FWL);
			AssertMode(TransportModes.Rail, ContainerModes.FCL, RateMode.FRA);
			AssertMode(TransportModes.Rail, ContainerModes.LCL, RateMode.LRA);

			AssertMode(TransportModes.Courier, RateMode.COU, RateMode.COU);

			void AssertMode(string transportMode, string containerMode, string mode)
			{
				quotedBooking.TransportMode = transportMode;
				quotedBooking.ContainerMode = containerMode;
				AssertEquals($"OneOffQuote.TT_TransportMode should be equal to {transportMode}", transportMode, quotedBooking.Quote.CurrentOneOffQuote.TT_TransportMode);
				AssertEquals($"OneOffQuote.TT_ContainerMode should be equal to {containerMode}", containerMode, quotedBooking.Quote.CurrentOneOffQuote.TT_ContainerMode);
				AssertEquals($"Correct Mode assigned for TT_TransportMode = {transportMode}, TT_ContainerMode = {containerMode}", mode, quotedBooking.Mode);
			}
		}

		public void TestGivenDifferentSetsOfTransportModeAndContainerMode_WhenConvertToBooking_ThenModesInBWQShouldBeAssignedCorrectly()
		{
			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.Air, ContainerModes.Loose, RateMode.LSE, TransportModes.Air, ContainerModes.Loose, RateMode.LSE);
			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.Air, ContainerModes.ULD, RateMode.ULD, TransportModes.Air, ContainerModes.ULD, RateMode.ULD);
			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.Air, ContainerModes.BuyersConsol, RateMode.BCN, TransportModes.Air, ContainerModes.BuyersConsol, RateMode.BCN);
			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.Air, ContainerModes.ShippersConsol, RateMode.SCN, TransportModes.Air, ContainerModes.ShippersConsol, RateMode.SCN);

			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.Sea, RateMode.SEA, RateMode.SEA, TransportModes.Sea, RateMode.FCL, RateMode.FCL);
			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.Sea, ContainerModes.FCL, RateMode.FCL, TransportModes.Sea, ContainerModes.FCL, RateMode.FCL);
			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.Sea, ContainerModes.LCL, RateMode.LCL, TransportModes.Sea, ContainerModes.LCL, RateMode.LCL);
			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.Sea, ContainerModes.Bulk, RateMode.BLK, TransportModes.Sea, ContainerModes.Bulk, RateMode.BLK);
			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.Sea, ContainerModes.Liquid, ContainerModes.Liquid, TransportModes.Sea, ContainerModes.Liquid, ContainerModes.Liquid);
			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.Sea, ContainerModes.BreakBulk, RateMode.BBK, TransportModes.Sea, ContainerModes.BreakBulk, RateMode.BBK);
			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.Sea, ContainerModes.BuyersConsol, RateMode.BCN, TransportModes.Sea, ContainerModes.BuyersConsol, RateMode.BCN);
			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.Sea, ContainerModes.ShippersConsol, RateMode.SCN, TransportModes.Sea, ContainerModes.ShippersConsol, RateMode.SCN);
			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.Sea, ContainerModes.RollOnRollOff, RateMode.ROR, TransportModes.Sea, ContainerModes.RollOnRollOff, RateMode.ROR);

			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.Road, RateMode.ROA, RateMode.ROA, TransportModes.Road, RateMode.LCL, RateMode.LRO);
			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.Road, RateMode.LRO, RateMode.LRO, TransportModes.Road, RateMode.LCL, RateMode.LRO);
			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.Road, ContainerModes.FCL, RateMode.FRO, TransportModes.Road, ContainerModes.FCL, RateMode.FRO);
			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.Road, ContainerModes.LTL, RateMode.LRO, TransportModes.Road, ContainerModes.LTL, RateMode.LRO);
			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.Road, ContainerModes.FTL, RateMode.FTL, TransportModes.Road, ContainerModes.FTL, RateMode.FTL);
			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.Road, ContainerModes.LCL, RateMode.LRO, TransportModes.Road, ContainerModes.LCL, RateMode.LRO);
			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.Road, ContainerModes.BuyersConsol, RateMode.BCN, TransportModes.Road, ContainerModes.BuyersConsol, RateMode.BCN);
			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.Road, ContainerModes.ShippersConsol, RateMode.SCN, TransportModes.Road, ContainerModes.ShippersConsol, RateMode.SCN);

			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.Rail, RateMode.RAI, RateMode.RAI, TransportModes.Rail, RateMode.LCL, RateMode.LRA);
			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.Rail, RateMode.FWL, RateMode.FWL, TransportModes.Rail, RateMode.LCL, RateMode.LRA);
			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.Rail, ContainerModes.FCL, RateMode.FRA, TransportModes.Rail, ContainerModes.FCL, RateMode.FRA);
			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.Rail, ContainerModes.LCL, RateMode.LRA, TransportModes.Rail, ContainerModes.LCL, RateMode.LRA);
			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.Rail, ContainerModes.Bulk, RateMode.BLK, TransportModes.Rail, ContainerModes.Bulk, RateMode.BLK);
			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.Rail, ContainerModes.Liquid, ContainerModes.Liquid, TransportModes.Rail, ContainerModes.Liquid, ContainerModes.Liquid);
			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.Rail, ContainerModes.BreakBulk, RateMode.BBK, TransportModes.Rail, ContainerModes.BreakBulk, RateMode.BBK);
			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.Rail, ContainerModes.BuyersConsol, RateMode.BCN, TransportModes.Rail, ContainerModes.BuyersConsol, RateMode.BCN);
			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.Rail, ContainerModes.ShippersConsol, RateMode.SCN, TransportModes.Rail, ContainerModes.ShippersConsol, RateMode.SCN);

			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.Courier, RateMode.COU, RateMode.COU, TransportModes.Courier, ContainerModes.OnBoardCourier, RateMode.COU);
			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.Courier, ContainerModes.OnBoardCourier, RateMode.OBC, TransportModes.Courier, ContainerModes.OnBoardCourier, RateMode.COU);
			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.Courier, ContainerModes.Unaccompanied, RateMode.UNA, TransportModes.Courier, ContainerModes.Unaccompanied, RateMode.UNA);

			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.SeaAir, ContainerModes.Loose, RateMode.LSE, TransportModes.SeaAir, ContainerModes.Loose, RateMode.LSE);
			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.SeaAir, ContainerModes.ULD, RateMode.ULD, TransportModes.SeaAir, ContainerModes.ULD, RateMode.ULD);
			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.SeaAir, ContainerModes.LCL, RateMode.LCL, TransportModes.SeaAir, ContainerModes.LCL, RateMode.LCL);

			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.AirSea, ContainerModes.Loose, RateMode.LSE, TransportModes.AirSea, ContainerModes.Loose, RateMode.LSE);
			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.AirSea, ContainerModes.ULD, RateMode.ULD, TransportModes.AirSea, ContainerModes.ULD, RateMode.ULD);
			TestModeAssignedCorrectly_WhenCovertToBooking(TransportModes.AirSea, ContainerModes.LCL, RateMode.LCL, TransportModes.AirSea, ContainerModes.LCL, RateMode.LCL);
		}

		void TestModeAssignedCorrectly_WhenCovertToBooking(ZString transportModeForOOQ, ZString containerModeForOOQ, ZString expectedOOQMode, ZString transportModeForBWQ, ZString containerModeForBWQ, ZString expectedBWQMode)
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
			quotedBooking.TransportMode = transportModeForOOQ;
			quotedBooking.ContainerMode = containerModeForOOQ;

			AssertEquals($"OOQ Transport Mode should be {transportModeForOOQ}", transportModeForOOQ, quotedBooking.TransportMode);
			AssertEquals($"OOQ Container Mode should be {containerModeForOOQ}", containerModeForOOQ, quotedBooking.ContainerMode);
			AssertEquals($"OOQ Mode should be {quotedBooking.Mode}", quotedBooking.Mode, expectedOOQMode);
			quotedBooking.ConvertQuoteToQuotedBooking();
			AssertEquals($"BWQ Transport Mode should be {transportModeForBWQ}", transportModeForBWQ, quotedBooking.TransportMode);
			AssertEquals($"BWQ Container Mode should be {transportModeForBWQ}", containerModeForBWQ, quotedBooking.ContainerMode);
			AssertEquals($"BWQ Mode should be {quotedBooking.Mode}", quotedBooking.Mode, expectedBWQMode);
		}

		public void TestQuoteMode_AssignedCorrectly()
		{
			var quotedBooking = GetQuoteOnlyQuotedBooking();
			AssertMode(quotedBooking, TransportModes.Air, ContainerModes.Loose, RateMode.LSE);
			AssertMode(quotedBooking, TransportModes.Air, ContainerModes.ULD, RateMode.ULD);

			AssertMode(quotedBooking, TransportModes.Sea, RateMode.SEA, RateMode.SEA);
			AssertMode(quotedBooking, TransportModes.Sea, ContainerModes.FCL, RateMode.FCL);
			AssertMode(quotedBooking, TransportModes.Sea, ContainerModes.LCL, RateMode.LCL);

			AssertMode(quotedBooking, TransportModes.Road, RateMode.ROA, RateMode.ROA);
			AssertMode(quotedBooking, TransportModes.Road, RateMode.LRO, RateMode.LRO);
			AssertMode(quotedBooking, TransportModes.Road, ContainerModes.FCL, RateMode.FRO);
			AssertMode(quotedBooking, TransportModes.Road, ContainerModes.FTL, RateMode.FTL);

			AssertMode(quotedBooking, TransportModes.Rail, RateMode.RAI, RateMode.RAI);
			AssertMode(quotedBooking, TransportModes.Rail, RateMode.FWL, RateMode.FWL);
			AssertMode(quotedBooking, TransportModes.Rail, ContainerModes.FCL, RateMode.FRA);
			AssertMode(quotedBooking, TransportModes.Rail, ContainerModes.LCL, RateMode.LRA);

			AssertMode(quotedBooking, TransportModes.Courier, RateMode.COU, RateMode.COU);

			void AssertMode(QuotedBooking qb, string transportMode, string containerMode, string expectedMode)
			{
				qb.TransportMode = transportMode;
				qb.ContainerMode = containerMode;
				AssertEquals($"OneOffQuote.TT_TransportMode should be equal to {transportMode}", transportMode, qb.Quote.CurrentOneOffQuote.TT_TransportMode);
				AssertEquals($"OneOffQuote.TT_ContainerMode should be equal to {containerMode}", containerMode, qb.Quote.CurrentOneOffQuote.TT_ContainerMode);
				AssertEquals($"QuotedBooking.TransportMode should be equal to {transportMode}", transportMode, qb.TransportMode);
				AssertEquals($"QuotedBooking.ContainerMode should be equal to {containerMode}", containerMode, qb.ContainerMode);
				AssertEquals($"Correct Mode assigned for TT_TransportMode = {transportMode}, TT_ContainerMode = {containerMode}", expectedMode, quotedBooking.Mode);
			}
		}

		public void TestQuoteTransportAndContainerMode_AssignedCorrectly()
		{
			var quotedBooking = GetQuoteOnlyQuotedBooking();

			AssertTransportAndContainerMode(quotedBooking, RateMode.LSE, TransportModes.Air, ContainerModes.Loose);
			AssertTransportAndContainerMode(quotedBooking, RateMode.ULD, TransportModes.Air, ContainerModes.ULD);

			AssertTransportAndContainerMode(quotedBooking, RateMode.SEA, TransportModes.Sea, RateMode.SEA);
			AssertTransportAndContainerMode(quotedBooking, RateMode.FCL, TransportModes.Sea, ContainerModes.FCL);
			AssertTransportAndContainerMode(quotedBooking, RateMode.LCL, TransportModes.Sea, ContainerModes.LCL);

			AssertTransportAndContainerMode(quotedBooking, RateMode.ROA, TransportModes.Road, RateMode.ROA);
			AssertTransportAndContainerMode(quotedBooking, RateMode.LRO, TransportModes.Road, RateMode.LRO);
			AssertTransportAndContainerMode(quotedBooking, RateMode.FRO, TransportModes.Road, ContainerModes.FCL);
			AssertTransportAndContainerMode(quotedBooking, RateMode.FTL, TransportModes.Road, ContainerModes.FTL);

			AssertTransportAndContainerMode(quotedBooking, RateMode.RAI, TransportModes.Rail, RateMode.RAI);
			AssertTransportAndContainerMode(quotedBooking, RateMode.FWL, TransportModes.Rail, RateMode.FWL);
			AssertTransportAndContainerMode(quotedBooking, RateMode.FRA, TransportModes.Rail, ContainerModes.FCL);
			AssertTransportAndContainerMode(quotedBooking, RateMode.LRA, TransportModes.Rail, ContainerModes.LCL);

			AssertTransportAndContainerMode(quotedBooking, RateMode.COU, TransportModes.Courier, RateMode.COU);

			void AssertTransportAndContainerMode(QuotedBooking qb, string mode, string expectedTransportMode, string expectedContainerMode)
			{
				qb.Mode = mode;

				AssertEquals($"Correct TT_TransportMode assigned for Mode {mode}", expectedTransportMode, quotedBooking.Quote.CurrentOneOffQuote.TT_TransportMode);
				AssertEquals($"Correct TT_ContainerMode assigned for Mode {mode}", expectedContainerMode, quotedBooking.Quote.CurrentOneOffQuote.TT_ContainerMode);
				AssertEquals($"QuotedBooking.TransportMode should be equal to {expectedTransportMode}", expectedTransportMode, qb.TransportMode);
				AssertEquals($"QuotedBooking.ContainerMode should be equal to {expectedContainerMode}", expectedContainerMode, qb.ContainerMode);
				AssertEquals($"Correct Mode assigned for TT_TransportMode = {expectedTransportMode}, TT_ContainerMode = {expectedContainerMode}", mode, quotedBooking.Mode);
			}
		}

		public void TestMode_BookingContainerModeMapping()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);

			quotedBooking.Mode = RateMode.LRO;
			AssertEquals("Correct container mode retained while mapping mode LRO", ContainerModes.LCL, quotedBooking.ContainerMode);
			quotedBooking.Mode = RateMode.COU;
			AssertEquals("Correct container mode retained while mapping mode COU", ContainerModes.OnBoardCourier, quotedBooking.ContainerMode);
		}

		#endregion

		#region ShipmentStatus

		public void TestShipmentStatus()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);
			AssertEquals(ShipmentStatusList.Codes.Booked, quotedBooking.ShipmentStatus);

			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			AssertEquals(ShipmentStatusList.Codes.Confirmed, quotedBooking.ShipmentStatus);

			quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
			AssertEquals(ShipmentStatusList.Codes.ElectronicBooking, shipment.JS_ShipmentStatus);

			quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.Booked;
			AssertEquals("ShipmentStatus should be set when OnHBLBookingStatusUpdate is null", ShipmentStatusList.Codes.Booked, quotedBooking.ShipmentStatus);

			quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.BookingRejected;
			AssertEquals("ShipmentStatus should be set when OnHBLBookingStatusUpdate is null", ShipmentStatusList.Codes.BookingRejected, quotedBooking.ShipmentStatus);

			quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
			AssertEquals(ShipmentStatusList.Codes.ElectronicBooking, shipment.JS_ShipmentStatus);

			quotedBooking.OnHBLBookingStatusUpdate += new EventHandler<HBLBookingStatusEventArgs>(UpdateHBLBookingStatusWithEmptyReason);
			quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.BookingRejected;
			AssertEquals("ShipmentStatus should not be set to BKJ with empty reason", ShipmentStatusList.Codes.ElectronicBooking, shipment.JS_ShipmentStatus);
			quotedBooking.OnHBLBookingStatusUpdate -= new EventHandler<HBLBookingStatusEventArgs>(UpdateHBLBookingStatusWithEmptyReason);

			quotedBooking.OnHBLBookingStatusUpdate += new EventHandler<HBLBookingStatusEventArgs>(UpdateHBLBookingStatusWithNonEmptyReason);
			quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.Booked;

			Thread.Sleep(100); // Adding a time gap to ensure "Logs.MostRecentLog" returns the latest

			var log = quotedBooking.Logs.MostRecentLog;
			CombineAssertions(() =>
			{
				AssertEquals("ShipmentStatus should be set", ShipmentStatusList.Codes.Booked, quotedBooking.ShipmentStatus);
				AssertEquals("Check event free text", ZString.Empty, log.ReferenceFreeText);
				AssertEquals("Check event type", "Shipment Status", log.Parameters[Params.Type]);
				AssertEquals("Check event new status", "BKD", log.Parameters[Params.New]);
				AssertEquals("Check event old status", "EBK", log.Parameters[Params.Old]);
				AssertEquals("Check event reason", "Booking Confirmed", log.Parameters[Params.Reason]);
				Assert("ShipmentStatus should be readonly", quotedBooking.ShipmentStatusInfo.ReadOnly);
			});

			quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
			AssertEquals(ShipmentStatusList.Codes.ElectronicBooking, shipment.JS_ShipmentStatus);

			Thread.Sleep(100); // Adding a time gap to ensure "Logs.MostRecentLog" returns the latest

			quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.BookingRejected;
			log = quotedBooking.Logs.MostRecentLog;
			CombineAssertions(() =>
			{
				AssertEquals("ShipmentStatus should be set", ShipmentStatusList.Codes.BookingRejected, quotedBooking.ShipmentStatus);
				AssertEquals("Check event free text", ZString.Empty, log.ReferenceFreeText);
				AssertEquals("Check event type", "Shipment Status", log.Parameters[Params.Type]);
				AssertEquals("Check event new status", "BKJ", log.Parameters[Params.New]);
				AssertEquals("Check event old status", "EBK", log.Parameters[Params.Old]);
				AssertEquals("Check event reason", "Booking Rejected, The booking is rejected for some reason.", log.Parameters[Params.Reason]);
				Assert("ShipmentStatus should be readonly", quotedBooking.ShipmentStatusInfo.ReadOnly);
			});

			Thread.Sleep(100); // Adding a time gap to ensure "Logs.MostRecentLog" returns the latest

			quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.EBookingCancellationRequest;
			log = quotedBooking.Logs.MostRecentLog;
			CombineAssertions(() =>
			{
				AssertEquals("ShipmentStatus should be set", ShipmentStatusList.Codes.EBookingCancellationRequest, quotedBooking.ShipmentStatus);
				AssertEquals("Check event free text", ZString.Empty, log.ReferenceFreeText);
				AssertEquals("Check event type", "Shipment Status", log.Parameters[Params.Type]);
				AssertEquals("Check event new status", "EBC", log.Parameters[Params.New]);
				AssertEquals("Check event old status", "BKJ", log.Parameters[Params.Old]);
				AssertEquals("Check event reason", "eBooking Cancell Request", log.Parameters[Params.Reason]);
				Assert("ShipmentStatus should be readonly", quotedBooking.ShipmentStatusInfo.ReadOnly);
			});

			Thread.Sleep(100); // Adding a time gap to ensure "Logs.MostRecentLog" returns the latest

			quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.Booked;
			log = quotedBooking.Logs.MostRecentLog;
			CombineAssertions(() =>
			{
				AssertEquals("ShipmentStatus should be set", ShipmentStatusList.Codes.Booked, quotedBooking.ShipmentStatus);
				AssertEquals("No extra logging when previous status was EBC - Check event free text", ZString.Empty, log.ReferenceFreeText);
				AssertEquals("No extra logging when previous status was EBC - Check event type", "Shipment Status", log.Parameters[Params.Type]);
				AssertEquals("No extra logging when previous status was EBC - Check event new status", "EBC", log.Parameters[Params.New]);
				AssertEquals("No extra logging when previous status was EBC - Check event old status", "BKJ", log.Parameters[Params.Old]);
				AssertEquals("No extra logging when previous status was EBC - Check event reason", "eBooking Cancell Request", log.Parameters[Params.Reason]);
				Assert("No extra logging when previous status was EBC - ShipmentStatus should be readonly", quotedBooking.ShipmentStatusInfo.ReadOnly);
			});

			Thread.Sleep(100); // Adding a time gap to ensure "Logs.MostRecentLog" returns the latest

			quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.BookingCancelled;
			log = quotedBooking.Logs.MostRecentLog;
			CombineAssertions(() =>
			{
				AssertEquals("ShipmentStatus should be set", ShipmentStatusList.Codes.BookingCancelled, quotedBooking.ShipmentStatus);
				AssertEquals("Check event free text", ZString.Empty, log.ReferenceFreeText);
				AssertEquals("Check event type", "Shipment Status", log.Parameters[Params.Type]);
				AssertEquals("Check event new status", "BKX", log.Parameters[Params.New]);
				AssertEquals("Check event old status", "BKD", log.Parameters[Params.Old]);
				AssertEquals("Check event reason", "Booking Cancelled", log.Parameters[Params.Reason]);
				Assert("ShipmentStatus should be readonly", quotedBooking.ShipmentStatusInfo.ReadOnly);
			});

			Thread.Sleep(100); // Adding a time gap to ensure "Logs.MostRecentLog" returns the latest

			quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
			AssertEquals(ShipmentStatusList.Codes.ElectronicBooking, shipment.JS_ShipmentStatus);

			using (quotedBooking.SuspendAutomaticCreationOfStatusChangedLog())
			{
				quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.BookingRejected;
				AssertEquals("No New Log should have been created", log, quotedBooking.Logs.MostRecentLog);
				AssertEquals("Shipment Status should have changed", ShipmentStatusList.Codes.BookingRejected, quotedBooking.ShipmentStatus);
			}
		}

		void UpdateHBLBookingStatusWithNonEmptyReason(object sender, HBLBookingStatusEventArgs e)
		{
			if (e.HBLBookingStatus == ShipmentStatusList.Codes.BookingRejected)
			{
				e.StatusUpdatedReason = "The booking is rejected for some reason.";
			}
		}

		void UpdateHBLBookingStatusWithEmptyReason(object sender, HBLBookingStatusEventArgs e)
		{
			if (e.HBLBookingStatus == ShipmentStatusList.Codes.BookingRejected)
			{
				e.StatusUpdatedReason = ZString.Empty;
			}
		}

		public void TestShipmentStatus_CreateStatusUpdated_OnlyWhenChanged()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);
			AssertEquals(ShipmentStatusList.Codes.Booked, quotedBooking.ShipmentStatus);
			var statusUpdatedLog = quotedBooking.Logs.Find(s => s.SL_SE_NKEvent == Events.StatusUpdatedCode).FirstOrDefault();
			AssertNull("StatusUpdated event will not be created during setting default values.", statusUpdatedLog);

			quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.Booked;
			statusUpdatedLog = quotedBooking.Logs.Find(s => s.SL_SE_NKEvent == Events.StatusUpdatedCode).FirstOrDefault();
			AssertNull("StatusUpdated event will not be created when ShipmentStatus is not changed.", statusUpdatedLog);

			quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.BookingRejected;
			statusUpdatedLog = quotedBooking.Logs.Find(s => s.SL_SE_NKEvent == Events.StatusUpdatedCode).FirstOrDefault();
			AssertNotNull("StatusUpdated event will be created when ShipmentStatus is changed.", statusUpdatedLog);
		}

		#endregion

		#region LogStatusChangedEvent

		public void TestLogStatusChangedEvent_RejectedWhileSettingFormInactive()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);

			using (quotedBooking.SuspendAutomaticCreationOfStatusChangedLog())
			{
				quotedBooking.LogStatusChangedEvent(ShipmentStatusList.Codes.BookingRejected, "Booking Cancelled");

				AssertContains("Log Reason", "|RES=Booking Cancelled|", quotedBooking.Logs.MostRecentLog.SL_Reference);
			}
		}

		public void TestLogStatusChangedEvent_RejectedWithNoReason()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);

			quotedBooking.LogStatusChangedEvent(ShipmentStatusList.Codes.BookingRejected, "");

			AssertContains("Log Reason", "|RES=Booking Rejected|", quotedBooking.Logs.MostRecentLog.SL_Reference);
		}

		public void TestLogStatusChangedEvent_RejectedWithReason()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);

			quotedBooking.LogStatusChangedEvent(ShipmentStatusList.Codes.BookingRejected, "Some Reason");

			AssertContains("Log Reason", "|RES=Booking Rejected, Some Reason|", quotedBooking.Logs.MostRecentLog.SL_Reference);
		}

		public void TestLogStatusChangedEvent_Booked()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);

			quotedBooking.LogStatusChangedEvent(ShipmentStatusList.Codes.Booked, "Booking Confirmed");

			AssertContains("Log Reason", "|RES=Booking Confirmed|", quotedBooking.Logs.MostRecentLog.SL_Reference);
		}

		#endregion

		#region TestICancel

		public void TestICancel()
		{
			Quote quoteOnlyQuote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			Quote quoteBookingQuote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			ForwardingShipment quoteBookingBooking = QuotedBooking.CreateNewBooking(Factory);
			ForwardingShipment bookingOnlyBooking = QuotedBooking.CreateNewBooking(Factory);

			QuotedBooking quoteOnly = QuotedBooking.New(quoteOnlyQuote.PK, ZGuid.Empty, Factory);
			QuotedBooking quoteBooking = QuotedBooking.New(quoteBookingQuote.PK, quoteBookingBooking.PK, Factory);
			QuotedBooking bookingOnly = QuotedBooking.New(ZGuid.Empty, bookingOnlyBooking.PK, Factory);

			ICancellable iQuoteOnly = quoteOnly;
			ICancellable iBookingOnly = bookingOnly;
			ICancellable iQuoteBooking = quoteBooking;

			AssertEquals("can cancel", JobHeaderParentDeletionHelper.CheckIfCanCancelJobHeaderParent(quoteOnlyQuote.PK, quoteOnlyQuote.HumanReadableName), iQuoteOnly.CanCancel());
			AssertEquals("can cancel", bookingOnlyBooking.CanCancel(), iBookingOnly.CanCancel());
			AssertEquals("can cancel", quoteBookingBooking.CanCancel() ?? "", iQuoteBooking.CanCancel() ?? "");

			AssertEquals("can reactivate", null, iQuoteOnly.CanReactivate());
			AssertEquals("can reactivate", bookingOnlyBooking.CanReactivate(), iBookingOnly.CanReactivate());
			AssertEquals("can reactivate", quoteBookingBooking.CanReactivate(), iQuoteBooking.CanReactivate());

			AssertEquals("is cancelled", quoteOnlyQuote.TH_IsCancelled, iQuoteOnly.IsCancelled);
			AssertEquals("is cancelled", ((ICancellable)bookingOnlyBooking).IsCancelled, iBookingOnly.IsCancelled);
			AssertEquals("is cancelled", ((ICancellable)quoteBookingBooking).IsCancelled && quoteBookingQuote.TH_IsCancelled, iQuoteBooking.IsCancelled);

			AssertEquals("IsCancelledHasChanged", quoteOnlyQuote.TH_IsCancelledInfo.HasChanges, iQuoteOnly.IsCancelledHasChanged);
			AssertEquals("IsCancelledHasChanged", ((ICancellable)bookingOnlyBooking).IsCancelledHasChanged, iBookingOnly.IsCancelledHasChanged);
			AssertEquals("IsCancelledHasChanged", ((ICancellable)quoteBookingBooking).IsCancelledHasChanged || quoteBookingQuote.TH_IsCancelledInfo.HasChanges, iQuoteBooking.IsCancelledHasChanged);

			iQuoteOnly.IsCancelled = true;
			iBookingOnly.IsCancelled = true;
			iQuoteBooking.IsCancelled = true;

			AssertEquals("is cancelled", quoteOnlyQuote.TH_IsCancelled, iQuoteOnly.IsCancelled);
			AssertEquals("is cancelled", ((ICancellable)bookingOnlyBooking).IsCancelled, iBookingOnly.IsCancelled);
			AssertEquals("is cancelled", ((ICancellable)quoteBookingBooking).IsCancelled && quoteBookingQuote.TH_IsCancelled, iQuoteBooking.IsCancelled);

			AssertEquals("IsCancelledHasChanged", quoteOnlyQuote.TH_IsCancelledInfo.HasChanges, iQuoteOnly.IsCancelledHasChanged);
			AssertEquals("IsCancelledHasChanged", ((ICancellable)bookingOnlyBooking).IsCancelledHasChanged, iBookingOnly.IsCancelledHasChanged);
			AssertEquals("IsCancelledHasChanged", ((ICancellable)quoteBookingBooking).IsCancelledHasChanged || quoteBookingQuote.TH_IsCancelledInfo.HasChanges, iQuoteBooking.IsCancelledHasChanged);
		}

		public void TestICancellableForEBookings()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);

			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			var cancellableQuotedBooking = quotedBooking as ICancellable;

			AssertEquals("Precondition", ShipmentStatusList.Codes.Booked, quotedBooking.ShipmentStatus);
			AssertEquals("Should be possible to reactivate quoted booking, as its booking is not an eBooking.", null, cancellableQuotedBooking.CanReactivate());

			cancellableQuotedBooking.IsCancelled = true;
			quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.BookingRejected;

			cancellableQuotedBooking.IsCancelled = false;
			AssertEquals("Shipment status should have been updated to BKD upon reactivation.", ShipmentStatusList.Codes.Booked, quotedBooking.ShipmentStatus);

			var cannotReactivateMessage = "A booking created via a Booking Request EDI Message cannot be re-activated.";

			quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
			AssertEquals("Should not be possible to reactivate quoted booking, as its shipment status is EBK.", cannotReactivateMessage, cancellableQuotedBooking.CanReactivate());

			quotedBooking.ShipmentStatus = ShipmentStatusList.Codes.BookingRejected;

			var parameters = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>(Params.Old, ShipmentStatusList.Codes.ElectronicBooking),
				new KeyValuePair<string, string>(Params.Reason, "A reason"),
				new KeyValuePair<string, string>(Params.Type, "A type")
			};

			booking.Logs.CreateOrRecreateEventLog(
				Events.StatusUpdated,
				EstimateActual.Actual,
				ZDateTimeOffset.Now,
				"a purpose description",
				parameters.ToArray());

			AssertEquals("Should be possible to reactivate quoted booking, as no log with New parameter = EBK exists.", null, cancellableQuotedBooking.CanReactivate());

			parameters[0] = new KeyValuePair<string, string>(Params.New, ShipmentStatusList.Codes.ElectronicBooking);
			booking.Logs.CreateOrRecreateEventLog(
				Events.StatusUpdated,
				EstimateActual.Actual,
				ZDateTimeOffset.Now,
				"a purpose description",
				parameters.ToArray());

			AssertEquals("Should not be possible to reactivate quoted booking, as log with New parameter = EBK exists.", cannotReactivateMessage, cancellableQuotedBooking.CanReactivate());
		}

		public void PreventDeleteAttribute()
		{
			Assert(CargoWise.EntityFramework.PreventDeleteAttribute.IsTrue(typeof(QuotedBooking)));
		}

		#endregion

		#region Commodity Defaulting from Quote

		public void TestPackLineCommodityDefaultsWhenConvertingQuoteToBooking()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			TestQuotedBookingExposer qb = new TestQuotedBookingExposer(quote.PK, ZGuid.Empty, Factory);

			var cont1 = quote.CurrentOneOffQuote.LooseCargo.AddNew();
			cont1.TPL_PackLineCount = 1;
			cont1.TPL_F3_NKPackType = "BOX";
			cont1.TPL_Weight = 555m;

			RefCommodityCode registryCommodity = Factory.Load<RefCommodityCode>(Env.Registry.CommodityCode);

			try
			{
				qb.ConvertQuoteToQuotedBooking();
				AssertEquals(registryCommodity.RH_Code, qb.Booking.OuterPackLines[0].JL_RH_NKCommodityCode);
			}
			finally
			{
				DisposeJobs(qb);
			}

			try
			{
				quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
				qb = new TestQuotedBookingExposer(quote.PK, ZGuid.Empty, Factory);

				cont1 = quote.CurrentOneOffQuote.LooseCargo.AddNew();
				cont1.TPL_PackLineCount = 1;
				cont1.TPL_F3_NKPackType = "BOX";
				cont1.TPL_Weight = 555m;

				ZQuery notRegistryCommodityQuery = new ZQuery(RefCommodityCodeSchema.PK, SQLComparisonOperator.NotEqual, Env.Registry.CommodityCode);
				RefCommodityCode commodity = Factory.LoadTop1<RefCommodityCode>(notRegistryCommodityQuery);
				qb.Commodity = commodity.RH_Code;

				qb.ConvertQuoteToQuotedBooking();
				AssertEquals(commodity.RH_Code, qb.Booking.OuterPackLines[0].JL_RH_NKCommodityCode);
			}
			finally
			{
				DisposeJobs(qb);
			}
		}

		public void TestPackLineCommodityDefaultsWhenConvertingBookingToShipment()
		{
			RefCommodityCode registryCommodity = Factory.Load<RefCommodityCode>(Env.Registry.CommodityCode);

			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			TestQuotedBookingExposer qb = new TestQuotedBookingExposer(quote.PK, booking.PK, Factory);

			ForwardingPackLine outerPL1 = booking.OuterPackLines.AddNew();
			AssertEquals(registryCommodity.RH_Code, outerPL1.JL_RH_NKCommodityCode);

			ZQuery notRegistryCommodityQuery = new ZQuery(RefCommodityCodeSchema.PK, SQLComparisonOperator.NotEqual, Env.Registry.CommodityCode);
			ZString commodity = Factory.LoadTop1<RefCommodityCode>(notRegistryCommodityQuery).RH_Code;
			qb.Commodity = commodity;

			ForwardingPackLine outerPL2 = booking.OuterPackLines.AddNew();
			Assert(!outerPL2.JL_RH_NKCommodityCode.IsEmpty);
			AssertEquals(commodity, outerPL2.JL_RH_NKCommodityCode);

			ZQuery anotherCommodityQuery = new ZQuery(RefCommodityCodeSchema.RH_Code, SQLComparisonOperator.NotEqual, qb.Commodity);
			ZQuery query = new ZQuery();
			query.AddToFilter(notRegistryCommodityQuery, JoinCondition.And);
			query.AddToFilter(anotherCommodityQuery, JoinCondition.And);

			qb.Commodity = Factory.LoadTop1<RefCommodityCode>(query).RH_Code;

			AssertEquals(qb.Commodity, outerPL1.JL_RH_NKCommodityCode);
			AssertEquals(qb.Commodity, outerPL2.JL_RH_NKCommodityCode);

			outerPL2.JL_RH_NKCommodityCode = commodity;
			qb.Commodity = registryCommodity.RH_Code;

			AssertEquals(registryCommodity.RH_Code, outerPL1.JL_RH_NKCommodityCode);
			AssertEquals(commodity, outerPL2.JL_RH_NKCommodityCode);

			qb.Commodity = ZString.Empty;

			AssertEquals(registryCommodity.RH_Code, outerPL1.JL_RH_NKCommodityCode);
			AssertEquals(commodity, outerPL2.JL_RH_NKCommodityCode);
		}

		public void TestCommodityOnContainerAndPackLines()
		{
			var containerRef1 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE");
			var containerRef2 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var commodityRef1 = Factory.NewWithValidTestData<RefCommodityCode>();
			var commodityRef2 = Factory.NewWithValidTestData<RefCommodityCode>();
			var commodityRef3 = Factory.NewWithValidTestData<RefCommodityCode>();
			commodityRef1.RH_Code = "NUNO";
			commodityRef2.RH_Code = "NDUE";
			commodityRef3.RH_Code = "NTRE";

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			quote.CurrentOneOffQuote.TT_RH_NKCommodity = commodityRef1.RH_Code;

			var container1 = quote.CurrentOneOffQuote.Containers.AddNew();
			container1.TC_ContainerCount = 1;
			container1.TC_RC = containerRef1.PK;

			var container2 = quote.CurrentOneOffQuote.Containers.AddNew();
			container2.TC_ContainerCount = 2;
			container2.TC_RC = containerRef2.PK;

			var packLine1 = quote.CurrentOneOffQuote.LooseCargo.AddNew();
			packLine1.TPL_PackLineCount = 6;
			packLine1.TPL_F3_NKPackType = Constants.PkgUnit.Pallet;
			packLine1.TPL_Weight = 600m;

			var packLine2 = quote.CurrentOneOffQuote.LooseCargo.AddNew();
			packLine2.TPL_PackLineCount = 4;
			packLine2.TPL_F3_NKPackType = Constants.PkgUnit.Pallet;
			packLine2.TPL_Weight = 400m;

			var packLine3 = quote.CurrentOneOffQuote.LooseCargo.AddNew();
			packLine3.TPL_PackLineCount = 5;
			packLine3.TPL_F3_NKPackType = Constants.PkgUnit.Pallet;
			packLine3.TPL_Weight = 500m;

			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			quotedBooking.Mode = Core.Constants.RateMode.SEA;
			quotedBooking.CopyQuoteValuesToBooking();

			AssertEquals("Booking with quote should have both containers", 2, quotedBooking.QuotedBookingContainers.Count);
			AssertEquals("Commodity Code should have been defaulted from the quote", commodityRef1.RH_Code, quotedBooking.QuotedBookingContainers[0].JC_RH_NKContainerCommodityCode);
			AssertEquals("Commodity Code should have been defaulted from the quote", commodityRef1.RH_Code, quotedBooking.QuotedBookingContainers[1].JC_RH_NKContainerCommodityCode);

			AssertEquals("Booking with quote should have both pack lines", 3, booking.OuterPackLines.Count);
			AssertEquals("Commodity Code should have been defaulted from the quote", commodityRef1.RH_Code, booking.OuterPackLines[0].JL_RH_NKCommodityCode);
			AssertEquals("Commodity Code should have been defaulted from the quote", commodityRef1.RH_Code, booking.OuterPackLines[1].JL_RH_NKCommodityCode);
			AssertEquals("Commodity Code should have been defaulted from the quote", commodityRef1.RH_Code, booking.OuterPackLines[2].JL_RH_NKCommodityCode);

			quotedBooking.QuotedBookingContainers[0].JC_RH_NKContainerCommodityCode = commodityRef2.RH_Code;
			booking.OuterPackLines[0].JL_RH_NKCommodityCode = commodityRef2.RH_Code;
			booking.OuterPackLines[2].JL_RH_NKCommodityCode = ZString.Empty;

			quotedBooking.Commodity = commodityRef3.RH_Code;

			AssertEquals("Expected container commodity to default from quote as it was matched the previous code", commodityRef3.RH_Code, quotedBooking.QuotedBookingContainers[1].JC_RH_NKContainerCommodityCode);
			AssertEquals("Expected pack line commodity to default from quote as it was matched the previous code", commodityRef3.RH_Code, booking.OuterPackLines[1].JL_RH_NKCommodityCode);
			AssertEquals("Expected pack line commodity to default from quote as it was previously blank", commodityRef3.RH_Code, booking.OuterPackLines[2].JL_RH_NKCommodityCode);

			AssertEquals("Container commodity should still have the overriden commodity", commodityRef2.RH_Code, quotedBooking.QuotedBookingContainers[0].JC_RH_NKContainerCommodityCode);
			AssertEquals("Pack line commodity should still have the overriden commodity", commodityRef2.RH_Code, booking.OuterPackLines[0].JL_RH_NKCommodityCode);
		}

		public void TestCommodityWhenCommodityValueIsEmpty()
		{
			var containerRef1 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE");
			var containerRef2 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var commodityRef1 = Factory.NewWithValidTestData<RefCommodityCode>();
			var commodityRef2 = Factory.NewWithValidTestData<RefCommodityCode>();
			var commodityRef3 = Factory.NewWithValidTestData<RefCommodityCode>();
			commodityRef1.RH_Code = "NUNO";
			commodityRef2.RH_Code = "NDUE";
			commodityRef3.RH_Code = "NTRE";

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			quote.CurrentOneOffQuote.TT_RH_NKCommodity = commodityRef1.RH_Code;

			var container1 = quote.CurrentOneOffQuote.Containers.AddNew();
			container1.TC_ContainerCount = 1;
			container1.TC_RC = containerRef1.PK;

			var container2 = quote.CurrentOneOffQuote.Containers.AddNew();
			container2.TC_ContainerCount = 2;
			container2.TC_RC = containerRef2.PK;

			var quotedBooking = QuotedBooking.New(quote.PK, QuotedBooking.CreateNewBooking(Factory).PK, Factory);
			quotedBooking.Mode = Core.Constants.RateMode.FCL;
			quotedBooking.CopyQuoteValuesToBooking();

			AssertEquals("Booking with quote should have both containers", 2, quotedBooking.QuotedBookingContainers.Count);
			AssertEquals("Commodity Code should have been defaulted from the quote (UNO)", commodityRef1.RH_Code, quotedBooking.QuotedBookingContainers[0].JC_RH_NKContainerCommodityCode);
			AssertEquals("Commodity Code should have been defaulted from the quote (UNO)", commodityRef1.RH_Code, quotedBooking.QuotedBookingContainers[1].JC_RH_NKContainerCommodityCode);

			quotedBooking.QuotedBookingContainers[0].JC_RH_NKContainerCommodityCode = commodityRef2.RH_Code;
			quotedBooking.Commodity = commodityRef3.RH_Code;

			AssertEquals("Expected container commodity to still be the overriden commodity (DUE)", commodityRef2.RH_Code, quotedBooking.QuotedBookingContainers[0].JC_RH_NKContainerCommodityCode);
			AssertEquals("Expected container commodity to default from quote (TRE)", commodityRef3.RH_Code, quotedBooking.QuotedBookingContainers[1].JC_RH_NKContainerCommodityCode);

			quotedBooking.Commodity = ZString.Empty;

			AssertEquals("Expected no change", commodityRef2.RH_Code, quotedBooking.QuotedBookingContainers[0].JC_RH_NKContainerCommodityCode);
			AssertEquals("Expected no change", commodityRef3.RH_Code, quotedBooking.QuotedBookingContainers[1].JC_RH_NKContainerCommodityCode);
		}

		public void TestCommodityDefaultValueOnQuotedBookingCollection()
		{
			var commodityRef1 = Factory.NewWithValidTestData<RefCommodityCode>();
			var commodityRef2 = Factory.NewWithValidTestData<RefCommodityCode>();
			commodityRef1.RH_Code = "NUNO";
			commodityRef2.RH_Code = "NDUE";

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			quote.CurrentOneOffQuote.TT_RH_NKCommodity = commodityRef1.RH_Code;
			quote.CurrentOneOffQuote.Containers.AddNew();

			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			quotedBooking.Mode = Core.Constants.RateMode.FCL;
			quotedBooking.CopyQuoteValuesToBooking();

			AssertEquals("Expected have copy container from quote", 1, quotedBooking.QuotedBookingContainers.Count);
			AssertEquals("Commodity Code should have been defaulted from the quote", commodityRef1.RH_Code, quotedBooking.QuotedBookingContainers[0].JC_RH_NKContainerCommodityCode);

			var container2 = quotedBooking.QuotedBookingContainers.AddNew();

			AssertEquals("Newly added container should default commodity code from the quote", commodityRef1.RH_Code, container2.JC_RH_NKContainerCommodityCode);

			quotedBooking.Commodity = commodityRef2.RH_Code;

			var container3 = quotedBooking.QuotedBookingContainers.AddNew();

			AssertEquals("Newly added container should default commodity code from the quote and not use the old code", commodityRef2.RH_Code, container3.JC_RH_NKContainerCommodityCode);
		}

		#endregion

		#region TestConvertCopiesPackCountAndPaymentTerms

		public void TestSynchBookingValuesWithQuoteDims()
		{
			Quote quot = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			TestQuotedBookingExposer qb = new TestQuotedBookingExposer(quot.PK, booking.PK, Factory);

			FreightPacksDataRegistry.Instance.OuterPackUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "BOX");

			ForwardingPackLine outerPL = booking.OuterPackLines.AddNew();
			outerPL.JL_F3_NKPackType = "PLT";
			outerPL.JL_Height = 30;
			outerPL.JL_Width = 20;
			outerPL.JL_Length = 40;
			outerPL.JL_ActualWeight = 15;
			outerPL.JL_UnitOfDimension = Core.Constants.Length.Centimetres;
			outerPL.JL_ActualWeightUQ = Core.Constants.Weight.Kilotonnes;
			outerPL.JL_ActualVolumeUQ = Core.Constants.Volume.CubicDecimetres;

			qb.CheckAndCopyBookingValuesToQuoteIfRequired();
			var cont = quot.CurrentOneOffQuote.LooseCargo[0];

			AssertEquals("pack type", "PLT", cont.TPL_F3_NKPackType);
			AssertEquals("height", 30m, cont.TPL_Height);
			AssertEquals("width", 20m, cont.TPL_Width);
			AssertEquals("length", 40m, cont.TPL_Length);
			AssertEquals("weight", 15m, cont.TPL_Weight);
			AssertEquals("dimension UQ", Core.Constants.Length.Centimetres, cont.TPL_DimensionUQ);
			AssertEquals("weight UQ", Core.Constants.Weight.Kilotonnes, cont.TPL_WeightUQ);
			AssertEquals("volume UQ", Core.Constants.Volume.CubicDecimetres, cont.TPL_VolumeUQ);
		}

		public void TestPackLinesAndContainerSync()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			TestQuotedBookingExposer qb = new TestQuotedBookingExposer(quote.PK, booking.PK, Factory);

			FreightPacksDataRegistry.Instance.OuterPackUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "BOX");

			ForwardingPackLine outerPLtoRemain = booking.OuterPackLines.AddNew();
			outerPLtoRemain.JL_F3_NKPackType = "PLT";
			outerPLtoRemain.JL_Height = 30;
			outerPLtoRemain.JL_Width = 20;
			outerPLtoRemain.JL_Length = 40;
			outerPLtoRemain.JL_ActualWeight = 15;
			outerPLtoRemain.JL_UnitOfDimension = Constants.Length.Centimetres;
			outerPLtoRemain.JL_ActualWeightUQ = Constants.Weight.Kilotonnes;
			outerPLtoRemain.JL_ActualVolumeUQ = Constants.Volume.CubicDecimetres;

			ForwardingPackLine outerPLtoDelete = booking.OuterPackLines.AddNew();
			outerPLtoDelete.JL_F3_NKPackType = "PKG";
			outerPLtoDelete.JL_Height = 40;
			outerPLtoDelete.JL_Width = 30;
			outerPLtoDelete.JL_Length = 45;
			outerPLtoDelete.JL_ActualWeight = 25;
			outerPLtoDelete.JL_UnitOfDimension = Constants.Length.Centimetres;
			outerPLtoDelete.JL_ActualWeightUQ = Constants.Weight.Kilotonnes;
			outerPLtoDelete.JL_ActualVolumeUQ = Constants.Volume.CubicDecimetres;

			ForwardingPackLine outerPLtoChange = booking.OuterPackLines.AddNew();
			outerPLtoChange.JL_F3_NKPackType = "BOX";
			outerPLtoChange.JL_Height = 50;
			outerPLtoChange.JL_Width = 24;
			outerPLtoChange.JL_Length = 43;
			outerPLtoChange.JL_ActualWeight = 12;
			outerPLtoChange.JL_UnitOfDimension = Constants.Length.Centimetres;
			outerPLtoChange.JL_ActualWeightUQ = Constants.Weight.Kilotonnes;
			outerPLtoChange.JL_ActualVolumeUQ = Constants.Volume.CubicDecimetres;

			RefContainer twentyGP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			RefContainer fortyGP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			RefContainer twentyRE = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE");
			RefContainer fortyRE = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE");
			RefContainer fortyFR = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40FR");

			ForwardingContainer containerToRemain = qb.QuotedBookingContainers.AddNew();
			containerToRemain.JC_ContainerCount = 2;
			containerToRemain.JC_RC = twentyGP.PK;

			ForwardingContainer containerToDelete = qb.QuotedBookingContainers.AddNew();
			containerToDelete.JC_ContainerCount = 3;
			containerToDelete.JC_RC = fortyGP.PK;

			ForwardingContainer containerToChange = qb.QuotedBookingContainers.AddNew();
			containerToChange.JC_ContainerCount = 4;
			containerToChange.JC_RC = twentyRE.PK;

			ForwardingContainer containerDuplicate1 = qb.QuotedBookingContainers.AddNew();
			containerDuplicate1.JC_ContainerCount = 7;
			containerDuplicate1.JC_RC = fortyFR.PK;

			ForwardingContainer containerDuplicate2 = qb.QuotedBookingContainers.AddNew();
			containerDuplicate2.JC_ContainerCount = 7;
			containerDuplicate2.JC_RC = fortyFR.PK;

			qb.CheckAndCopyBookingValuesToQuoteIfRequired();
			AssertEquals(3, quote.CurrentOneOffQuote.LooseCargo.Count);
			AssertEquals(5, quote.CurrentOneOffQuote.Containers.Count);

			var ratePackLines = quote.CurrentOneOffQuote.LooseCargo.Cast<RateOneOffPackLine>();
			var rateContainers = quote.CurrentOneOffQuote.Containers.Cast<RateOneOffContainers>();

			ZGuid ratePLToRemain = ratePackLines.FirstOrDefault(line => line.TPL_F3_NKPackType == "PLT").PK;
			ZGuid ratePLToDelete = ratePackLines.FirstOrDefault(line => line.TPL_F3_NKPackType == "PKG").PK;
			AssertNotNull(ratePackLines.FirstOrDefault(line => line.TPL_F3_NKPackType == "BOX"));

			ZGuid rateContainerToRemain = rateContainers.FirstOrDefault(line => line.TC_RC == twentyGP.PK).PK;
			ZGuid rateContainerToDelete = rateContainers.FirstOrDefault(line => line.TC_RC == fortyGP.PK).PK;
			AssertNotNull(rateContainers.FirstOrDefault(line => line.TC_RC == twentyRE.PK));

			AssertEquals(2, rateContainers.Count(line => line.TC_RC == fortyFR.PK));

			booking.OuterPackLines.RemoveAndDelete(outerPLtoDelete);
			qb.QuotedBookingContainers.RemoveAndDelete(containerToDelete);
			qb.QuotedBookingContainers.RemoveAndDelete(containerDuplicate2);

			outerPLtoChange.JL_ActualWeight = 13;
			containerToChange.JC_ContainerCount = 5;

			ForwardingPackLine addedOuterPL = booking.OuterPackLines.AddNew();
			addedOuterPL.JL_F3_NKPackType = "CRT";
			addedOuterPL.JL_Height = 53;
			addedOuterPL.JL_Width = 22;
			addedOuterPL.JL_Length = 44;
			addedOuterPL.JL_ActualWeight = 15;
			addedOuterPL.JL_UnitOfDimension = Constants.Length.Centimetres;
			addedOuterPL.JL_ActualWeightUQ = Constants.Weight.Kilotonnes;
			addedOuterPL.JL_ActualVolumeUQ = Constants.Volume.CubicDecimetres;

			ForwardingContainer addedContainer = qb.QuotedBookingContainers.AddNew();
			addedContainer.JC_ContainerCount = 6;
			addedContainer.JC_RC = fortyRE.PK;

			qb.CheckAndCopyBookingValuesToQuoteIfRequired();

			AssertEquals(3, quote.CurrentOneOffQuote.LooseCargo.Count);
			AssertEquals(4, quote.CurrentOneOffQuote.Containers.Count);

			AssertNotNull(ratePackLines.FirstOrDefault(line => line.TPL_F3_NKPackType == "CRT"));
			AssertNotNull(ratePackLines.FirstOrDefault(line => line.TPL_F3_NKPackType == "BOX"));

			AssertNotNull(rateContainers.FirstOrDefault(line => line.TC_RC == fortyRE.PK));
			AssertNotNull(rateContainers.FirstOrDefault(line => line.TC_RC == twentyRE.PK));
			AssertEquals(1, rateContainers.Count(line => line.TC_RC == fortyFR.PK));

			AssertNull(quote.CurrentOneOffQuote.LooseCargo.FindByPK(ratePLToDelete));
			AssertNotNull(quote.CurrentOneOffQuote.LooseCargo.FindByPK(ratePLToRemain));

			AssertNull(quote.CurrentOneOffQuote.Containers.FindByPK(rateContainerToDelete));
			AssertNotNull(quote.CurrentOneOffQuote.Containers.FindByPK(rateContainerToRemain));
		}

		public void TestConvertCopiesPackCountAndPaymentTerms()
		{
			Quote quot = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking qb = QuotedBooking.New(quot.PK, booking.PK, Factory);
			var r1oc = quot.CurrentOneOffQuote.LooseCargo.AddNew();
			quot.CurrentOneOffQuote.TT_ActualWeight = 1000;
			r1oc.TPL_PackLineCount = 2;
			r1oc.TPL_Height = 2;
			r1oc.TPL_Width = 3;
			r1oc.TPL_Length = 4;
			r1oc.TPL_F3_NKPackType = Constants.PkgUnit.Keg;

			var r2oc = quot.CurrentOneOffQuote.LooseCargo.AddNew();
			r2oc.TPL_PackLineCount = 3;
			r2oc.TPL_F3_NKPackType = Constants.PkgUnit.Keg;

			quot.CurrentOneOffQuote.TT_IncoTerm = "ter";
			qb.CopyQuoteValuesToBooking();
			AssertEquals("inco term", "ter", booking.JS_INCO);
			AssertEquals("packs", 5, booking.JS_OuterPacks);
			AssertEquals("packs", Constants.PkgUnit.Keg, booking.JS_F3_NKPackType);
			AssertEquals("pack lines", 2, booking.OuterPackLines.Count);
			AssertEquals("weight", (ZDecimal)0, booking.OuterPackLines[0].JL_ActualWeight);
			AssertEquals("packType", Constants.PkgUnit.Keg, booking.OuterPackLines[0].JL_F3_NKPackType);
			AssertEquals("weight", (ZDecimal)0, booking.OuterPackLines[1].JL_ActualWeight);
			AssertEquals("packType", Constants.PkgUnit.Keg, booking.OuterPackLines[1].JL_F3_NKPackType);
			AssertEquals("height", Constants.Length.Convert(2, Constants.Length.Metres, booking.OuterPackLines[0].JL_UnitOfDimension), booking.OuterPackLines[0].JL_Height);
			AssertEquals("width", Constants.Length.Convert(3, Constants.Length.Metres, booking.OuterPackLines[0].JL_UnitOfDimension), booking.OuterPackLines[0].JL_Width);
			AssertEquals("length", Constants.Length.Convert(4, Constants.Length.Metres, booking.OuterPackLines[0].JL_UnitOfDimension), booking.OuterPackLines[0].JL_Length);
		}

		public void TestConvertCopiesPackLines_Imperial()
		{
			Quote quot = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking qb = QuotedBooking.New(quot.PK, booking.PK, Factory);
			var r1oc = quot.CurrentOneOffQuote.LooseCargo.AddNew();
			quot.CurrentOneOffQuote.TT_UnitOfVolume = "CF";
			r1oc.TPL_PackLineCount = 1;
			r1oc.TPL_Height = 42;
			r1oc.TPL_Width = 48;
			r1oc.TPL_Length = 69;
			r1oc.TPL_DimensionUQ = Constants.Length.Inches;

			var miRawPropertyInfo = Env.Registry.RawRegistry.GetType().GetProperty("OuterPacklinesMeasurementDefaultUnit",
					BindingFlags.Instance | BindingFlags.NonPublic);

			var propertyValue = miRawPropertyInfo.GetValue(Env.Registry.RawRegistry, null);

			var miPackLinesDefaultUnitSetValueMethodInfo = Env.Registry.RawRegistry.GetType().GetProperty("OuterPacklinesMeasurementDefaultUnit",
					BindingFlags.Instance | BindingFlags.NonPublic).PropertyType.GetMethod("SetValue");

			miPackLinesDefaultUnitSetValueMethodInfo.Invoke(propertyValue, new object[] { Guid.Empty, Guid.Empty, Guid.Empty, Constants.Length.Metres });

			AssertEquals("This test requres the default unit for outer pack lines [Env.Registry.OuterPacklinesMeasurementDefaultUnit] to be meters",
					Env.Registry.OuterPacklinesMeasurementDefaultUnit, Constants.Length.Metres);

			qb.CopyQuoteValuesToBooking();

			AssertEquals("Booking packline should have M as unit of dimension", Constants.Length.Inches, booking.OuterPackLines[0].JL_UnitOfDimension);

			AssertEquals("height", 42m, booking.OuterPackLines[0].JL_Height);
			AssertEquals("width", 48m, booking.OuterPackLines[0].JL_Width);
			AssertEquals("length", 69m, booking.OuterPackLines[0].JL_Length);
		}

		public void TestConvertCopiesWithMetricUnitOfVolumeAndImperialUnitOfDimension()
		{
			Quote quot = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking qb = QuotedBooking.New(quot.PK, booking.PK, Factory);

			var r1oc = quot.CurrentOneOffQuote.LooseCargo.AddNew();
			quot.CurrentOneOffQuote.TT_UnitOfVolume = Constants.Volume.CubicMetres;

			r1oc.TPL_PackLineCount = 1;
			r1oc.TPL_Height = 48;
			r1oc.TPL_Width = 36;
			r1oc.TPL_Length = 79;
			r1oc.TPL_DimensionUQ = Constants.Length.Inches;

			var miRawPropertyInfo = Env.Registry.RawRegistry.GetType().GetProperty("OuterPacklinesMeasurementDefaultUnit",
					BindingFlags.Instance | BindingFlags.NonPublic);

			var propertyValue = miRawPropertyInfo.GetValue(Env.Registry.RawRegistry, null);

			var miPackLinesDefaultUnitSetValueMethodInfo = Env.Registry.RawRegistry.GetType().GetProperty("OuterPacklinesMeasurementDefaultUnit",
					BindingFlags.Instance | BindingFlags.NonPublic).PropertyType.GetMethod("SetValue");

			miPackLinesDefaultUnitSetValueMethodInfo.Invoke(propertyValue, new object[] { Guid.Empty, Guid.Empty, Guid.Empty, Constants.Length.Inches });

			AssertEquals("This test requres the default unit for outer pack lines [Env.Registry.OuterPacklinesMeasurementDefaultUnit] to be inches",
					Env.Registry.OuterPacklinesMeasurementDefaultUnit, Constants.Length.Inches);

			qb.CopyQuoteValuesToBooking();

			AssertEquals("Booking packline should have IN as unit of dimension", Constants.Length.Inches, booking.OuterPackLines[0].JL_UnitOfDimension);

			AssertEquals(String.Format("height - expected 48 inches - actual {0}", booking.OuterPackLines[0].JL_Height),
					48m, booking.OuterPackLines[0].JL_Height);
			AssertEquals(String.Format("width - expected 36 inches - actual {0}", booking.OuterPackLines[0].JL_Width),
					36m, booking.OuterPackLines[0].JL_Width);
			AssertEquals(String.Format("length - expected 79 inches - actual {0}", booking.OuterPackLines[0].JL_Length),
					79m, booking.OuterPackLines[0].JL_Length);
		}

		public void TestCopyQuoteValuesToBooking_PackLineVolumeDoesntCorrespondToSize_UseValueFromVolume()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var packLine = quote.CurrentOneOffQuote.LooseCargo.AddNew();
			packLine.TPL_PackLineCount = 2;
			packLine.TPL_Width = 5;
			packLine.TPL_Height = 5;
			packLine.TPL_Length = 5;
			packLine.TPL_VolumeUQ = Constants.Volume.CubicMetres;
			packLine.TPL_Volume = 10;

			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);

			quotedBooking.CopyQuoteValuesToBooking();

			AssertEquals("Packlines count on booking", 1, booking.OuterPackLines.Count);
			AssertEquals("Pack line's volume", (Decimal)10, booking.OuterPackLines[0].JL_ActualVolume);
		}

		#endregion

		#region Chargeable

		public void TestChargeableSetsBothBookingAndQuote()
		{
			QuotedBooking quotedBooking = (QuotedBooking)GetNewBusinessObject();

			Quote quote = quotedBooking.Quote;
			quote.TH_Accepted = ZDateTime.Now;
			ForwardingShipment booking = quotedBooking.Booking;

			quotedBooking.Mode = Core.Constants.RateMode.LSE;
			quotedBooking.Chargeable = 21;

			AssertEquals(quote.CurrentOneOffQuote.TT_ActualVolume, booking.JS_ActualVolume);
			AssertEquals(quote.CurrentOneOffQuote.TT_ActualWeight, booking.JS_ActualWeight);

			ResetWeightVolume(quote, booking);

			quotedBooking.Mode = Core.Constants.RateMode.SEA;
			quotedBooking.Chargeable = 22;

			AssertEquals(quote.CurrentOneOffQuote.TT_ActualVolume, booking.JS_ActualVolume);
			AssertEquals(quote.CurrentOneOffQuote.TT_ActualWeight, booking.JS_ActualWeight);

			ResetWeightVolume(quote, booking);

			quotedBooking.Mode = Constants.TransportModes.Rail;
			quotedBooking.Chargeable = 23;

			AssertEquals(quote.CurrentOneOffQuote.TT_ActualVolume, booking.JS_ActualVolume);
			AssertEquals(quote.CurrentOneOffQuote.TT_ActualWeight, booking.JS_ActualWeight);

			ResetWeightVolume(quote, booking);

			quotedBooking.Mode = Constants.TransportModes.Road;
			quotedBooking.Chargeable = 24;

			AssertEquals(quote.CurrentOneOffQuote.TT_ActualVolume, booking.JS_ActualVolume);
			AssertEquals(quote.CurrentOneOffQuote.TT_ActualWeight, booking.JS_ActualWeight);
		}

		static void ResetWeightVolume(Quote quote, ForwardingShipment booking)
		{
			quote.CurrentOneOffQuote.TT_ActualVolume = 0;
			booking.JS_ActualVolume = 0;
			quote.CurrentOneOffQuote.TT_ActualWeight = 0;
			booking.JS_ActualWeight = 0;
		}

		#endregion

		#region Volume refresh

		public void TestBookingVolumeRefreshesVolume()
		{
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);
			quotedBooking.VolumeInfo.ValueChanged += new EventHandler(VolumeInfo_ValueChanged);
			booking.JS_ActualVolume = 20;
			AssertEquals("should update volume", 20, volumeValue.ToZInt());
		}

		public void TestQuoteVolumeRefreshesVolume()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			QuotedBooking quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
			quotedBooking.VolumeInfo.ValueChanged += new EventHandler(VolumeInfo_ValueChanged);
			quote.CurrentOneOffQuote.TT_ActualVolume = 35;
			AssertEquals("should update volume", 35, volumeValue.ToZInt());
		}

		ZDecimal volumeValue = -1;

		void VolumeInfo_ValueChanged(object sender, EventArgs e)
		{
			volumeValue = ((QuotedBooking)sender).Volume;
		}

		#endregion

		#region Volume Unit Conversion

		public void TestUnitConversionVolume()
		{
			var quickBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			quickBooking.Mode = "LSE";
			quickBooking.Volume = 0m;
			quickBooking.VolumeUnit = "AA";

			var descriptors = TypeDescriptor.GetProperties(quickBooking);
			var amount1Descriptor = descriptors["Volume"];
			var unitDescriptor = descriptors["VolumeUnit"];

			AssertNoExceptionThrown(() =>
			{
				var unitConversion = ZArchitecture.Business.UnitConversion.Create(quickBooking, new[] { amount1Descriptor }, unitDescriptor, MeasureUnitType.Volume);

				AssertContainsExactElementsInAnyOrder(quickBooking.UnitOfVolumeList, unitConversion.UnitList);
			});
		}

		#endregion

		#region RateOneOffShipment Arithmetic Overflow Exception

		[ExpectNoExceptions]
		public void TestVolume_RateOneoffShipment_ArithmeticOverflowException()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;

			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);

			var packLine1 = booking.OuterPackLines.AddNew();
			packLine1.JL_F3_NKPackType = "PLT";
			packLine1.JL_Height = 560;
			packLine1.JL_Width = 560;
			packLine1.JL_Length = 560;
			packLine1.JL_ActualWeight = 56019;
			packLine1.JL_PackageCount = 12;
			packLine1.JL_UnitOfDimension = Constants.Length.Metres;
			packLine1.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			packLine1.JL_ActualVolumeUQ = Constants.Volume.CubicFeet;
			packLine1.JL_ActualVolume = 298183.27;

			var qb = new TestQuotedBookingExposer(quote.PK, booking.PK, Factory);
			qb.RunPreSaveValidationCoreForTest();

			Factory.Save();
		}

		[ExpectNoExceptions]
		public void TestChargeable_RateOneoffShipment_ArithmeticOverflowException()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;

			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_ActualWeight = 560129;
			booking.JS_UnitOfWeight = Constants.Weight.Kilograms;
			booking.JS_ActualVolume = 729;
			booking.JS_UnitOfVolume = Constants.Volume.CubicFeet;

			var packLine1 = booking.OuterPackLines.AddNew();
			packLine1.JL_ActualWeight = 560129;
			packLine1.JL_ActualVolume = 987654;
			packLine1.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			packLine1.JL_ActualVolumeUQ = Constants.Volume.CubicFeet;

			var qb = new TestQuotedBookingExposer(quote.PK, booking.PK, Factory);
			qb.RunPreSaveValidationCoreForTest();

			Factory.Save();
		}

		#endregion

		#region ObjectState

		[ExpectExceptionMessage(typeof(ArgumentException), "Needs at least one valid Quote or Booking PK")]
		public void TestObjectState_None()
		{
			QuotedBooking quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, ZGuid.Empty);
			AssertEquals("ObjectState should be NONE", QuotedBookingState.None, quotedBooking.ObjectState);
		}

		public void TestObjectState_QuoteOnly()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			QuotedBooking quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
			AssertEquals("ObjectState should be QuoteOnly", QuotedBookingState.QuoteOnly, quotedBooking.ObjectState);
		}

		public void TestObjectState_BookingOnly()
		{
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);
			AssertEquals("ObjectState should be BookingOnly", QuotedBookingState.BookingOnly, quotedBooking.ObjectState);
		}

		public void TestObjectState_QuotedBooking()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = CreateNewQuotedBooking(quote.PK, booking.PK);
			AssertEquals("ObjectState should be QuotedBooking", QuotedBookingState.AcceptedBookingWithQuote, quotedBooking.ObjectState);
		}

		public void TestObjectState_UnacceptedBookingWithQuote()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = CreateNewQuotedBooking(quote.PK, booking.PK);
			AssertEquals("ObjectState should be UnacceptedBookingWithQuote", QuotedBookingState.UnacceptedBookingWithQuote, quotedBooking.ObjectState);
		}

		#endregion

		#region StaticCreates

		public void TestCreateNewQuote_NOT_ApprovedAndAccepted()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			Assert("Should set OneTimeQuote", quote.TH_OneTimeQuote);
			Assert("Should not be Approved", !quote.CurrentOneOffQuote.TT_QuoteApprovedByManager);
			Assert("Should not be Accepted", quote.TH_Accepted.IsEmpty);
		}

		public void TestCreateNewQuote_ApprovedAndAccepted()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			Assert("Should set OneTimeQuote", quote.TH_OneTimeQuote);
			Assert("Should be Approved", quote.CurrentOneOffQuote.TT_QuoteApprovedByManager);
			Assert("Should be Accepted with a valid date", !quote.TH_Accepted.IsEmpty);
		}

		public void TestCreateNewBooking()
		{
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			Assert("Should have a Booked Date", !booking.JS_A_BKD.IsEmpty);
			Assert("Should be a Booking", booking.JS_IsBooking);
			Assert("Should not be Forward Registered", !booking.JS_IsForwardRegistered);
		}

		#endregion

		#region AcceptDiscrepancy

		[ExpectExceptionMessage(typeof(InvalidOperationException), "Invalid State to Accept Discrepancy")]
		public void TestAcceptDiscrepancy_InvalidState()
		{
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking bookingOnly = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);

			try
			{
				bookingOnly.AcceptDiscrepancy();
			}
			finally
			{
				DisposeJobs(bookingOnly);
			}
		}

		public void TestAcceptDiscrepancy_ConvertedBooking()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking convertedQuote = CreateNewQuotedBooking(quote.PK, booking.PK);

			try
			{
				Assert("Precon: shouldn't be accepted", convertedQuote.Quote.TH_Accepted.IsEmpty);

				OrgHeader consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				OrgHeader consignee = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, new ZGuid[] { consignor.PK }));
				OrgHeader client = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, new ZGuid[] { consignor.PK, consignee.PK }));
				RefServiceLevel svcLevel = Factory.LoadTop1<RefServiceLevel>(new ZQuery());

				booking.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
				booking.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
				convertedQuote.TryLoadOrCreateJob();
				convertedQuote.Job.LocalChargesPK = client.PK;
				convertedQuote.Job.JH_GE = Env.CurrentDepartment.PK;
				booking.JS_RS_NKServiceLevel = svcLevel.RS_Code;
				booking.JS_RL_NKOrigin = "AUSYD";
				booking.JS_RL_NKDestination = "NZAKL";
				booking.JS_ActualWeight = 100;
				booking.JS_UnitOfWeight = "LB";
				booking.JS_ActualVolume = 200;
				booking.JS_UnitOfVolume = "CF";
				booking.JS_GoodsValue = 11;
				booking.JS_RX_NKGoodsValueCurr = "USD";
				booking.JS_InsuranceValue = 12;
				booking.JS_RX_NKInsuranceCurrency = "AUD";
				booking.JS_INCO = "111";

				convertedQuote.CheckAndCopyBookingValuesToQuoteIfRequired();

				AssertEquals(ZGuid.Empty, quote.TH_OH);
				AssertEquals("", quote.CurrentOneOffQuote.TT_IncoTerm);
				AssertEquals(ZGuid.Empty, quote.CurrentOneOffQuote.PickUpDocAddress.OrganisationPK);
				AssertEquals(ZGuid.Empty, quote.CurrentOneOffQuote.DeliveryDocAddress.OrganisationPK);
				AssertEquals("", quote.CurrentOneOffQuote.TT_RS_NKServiceLevel);
				AssertEquals("", quote.CurrentOneOffQuote.TT_RL_NKReceivalLocation);
				AssertEquals("", quote.CurrentOneOffQuote.TT_RL_NKDeliveryLocation);
				AssertEquals(0m, quote.CurrentOneOffQuote.TT_ActualWeight);
				AssertEquals("KG", quote.CurrentOneOffQuote.TT_UnitOfWeight);
				AssertEquals(0m, quote.CurrentOneOffQuote.TT_ActualVolume);
				AssertEquals("M3", quote.CurrentOneOffQuote.TT_UnitOfVolume);
				AssertEquals(0m, quote.CurrentOneOffQuote.TT_ValueOfGoods);
				AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, quote.CurrentOneOffQuote.TT_RX_NKGoodsCurrency);
				AssertEquals(0m, quote.CurrentOneOffQuote.TT_InsureVal);
				AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, quote.CurrentOneOffQuote.TT_RX_NKInsureValCurr);

				convertedQuote.AcceptDiscrepancy();
				Assert("Should now be accepted", !convertedQuote.Quote.TH_Accepted.IsEmpty);
				convertedQuote.CheckAndCopyBookingValuesToQuoteIfRequired();

				AssertEquals(client.PK, quote.TH_OH);
				AssertEquals("111", quote.CurrentOneOffQuote.TT_IncoTerm);
				AssertEquals(consignor.PK, quote.CurrentOneOffQuote.PickUpDocAddress.OrganisationPK);
				AssertEquals(consignee.PK, quote.CurrentOneOffQuote.DeliveryDocAddress.OrganisationPK);
				AssertEquals(svcLevel.RS_Code, quote.CurrentOneOffQuote.TT_RS_NKServiceLevel);
				AssertEquals("AUSYD", quote.CurrentOneOffQuote.TT_RL_NKReceivalLocation);
				AssertEquals("NZAKL", quote.CurrentOneOffQuote.TT_RL_NKDeliveryLocation);
				AssertEquals(100m, quote.CurrentOneOffQuote.TT_ActualWeight);
				AssertEquals("LB", quote.CurrentOneOffQuote.TT_UnitOfWeight);
				AssertEquals(200m, quote.CurrentOneOffQuote.TT_ActualVolume);
				AssertEquals("CF", quote.CurrentOneOffQuote.TT_UnitOfVolume);
				AssertEquals(11m, quote.CurrentOneOffQuote.TT_ValueOfGoods);
				AssertEquals("USD", quote.CurrentOneOffQuote.TT_RX_NKGoodsCurrency);
				AssertEquals(12m, quote.CurrentOneOffQuote.TT_InsureVal);
				AssertEquals("AUD", quote.CurrentOneOffQuote.TT_RX_NKInsureValCurr);
			}
			finally
			{
				DisposeJobs(convertedQuote);
			}
		}

		#endregion

		#region TestSyncPickupAndDeliveryAddresses

		public void TestSyncPickupAndDeliveryAddresses()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			quotedBooking.Quote.TH_Accepted = ZDateTime.Now;

			AssertEquals("prerequisite; ObjectState", QuotedBookingState.AcceptedBookingWithQuote, quotedBooking.ObjectState);

			var consignor = Factory.New<OrgHeader>();
			var consignee = Factory.New<OrgHeader>();

			quotedBooking.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			quotedBooking.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;

			AssertEquals("Quote PickUpDocAddress is not synchronized yet",
				ZGuid.Empty, quotedBooking.Quote.CurrentOneOffQuote.PickUpDocAddress.OrganisationPK);

			AssertEquals("Quote DeliveryDocAddress is not synchronized yet",
				ZGuid.Empty, quotedBooking.Quote.CurrentOneOffQuote.DeliveryDocAddress.OrganisationPK);

			quotedBooking.CheckAndCopyBookingValuesToQuoteIfRequired();

			AssertEquals("Quote PickUpDocAddress has been synchronized",
				consignor.PK, quotedBooking.Quote.CurrentOneOffQuote.PickUpDocAddress.OrganisationPK);

			AssertEquals("Quote DeliveryDocAddress has been synchronized",
				consignee.PK, quotedBooking.Quote.CurrentOneOffQuote.DeliveryDocAddress.OrganisationPK);
		}

		#endregion

		#region Schedule

		public void TestScheduleChooserInstantiation()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			QuotedBooking quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
			ScheduleChooser chooser = quotedBooking.ScheduleChooser;
			AssertNotNull("For binding purposes, ScheduleChooser must never return null", chooser);
			AssertEquals("NullSailingChooserParent", chooser.Parent.GetType().Name);

			try
			{
				quotedBooking.ConvertQuoteToQuotedBooking();
				AssertNotEquals(chooser, quotedBooking.ScheduleChooser);
				AssertNotNull(quotedBooking.ScheduleChooser.Parent);
			}
			finally
			{
				DisposeJobs(quotedBooking);
			}
		}

		public void TestCustomScheduleChooser()
		{
			var customScheduleChooserCreator = new Mock<IScheduleChooserCreator>();
			var someSailingParent = new Mock<ISailingChooserParent>();
			var customScheduleChooser = new ScheduleChooser(someSailingParent.Object);
			customScheduleChooserCreator.Setup(m => m.Create(It.IsAny<ISailingChooserParent>())).Returns(customScheduleChooser);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var quote = QuotedBooking.New(ZGuid.Empty, shipment.PK, Factory, customScheduleChooserCreator.Object);
			AssertEquals(customScheduleChooser, quote.ScheduleChooser);
			customScheduleChooserCreator.VerifyAll();
		}

		#region TestLoadPortDefaults

		public void TestLoadPortDefaults()
		{
			QuotedBooking quotedBooking = (QuotedBooking)GetNewBusinessObject();
			ForwardingShipment booking = quotedBooking.Booking;

			string assertionMessage = IsLoadDischargeDefaultedFromOriginDestination ?
							"Load Port Should have defaulted through booking" :
							"Load Port Should not have defaulted through booking";

			booking.JS_RL_NKOrigin = "AU";
			string expectedPort = IsLoadDischargeDefaultedFromOriginDestination ? "AU" : "";
			AssertEquals(assertionMessage, expectedPort, quotedBooking.LoadPort);

			booking.JS_RL_NKOrigin = "AUSYD";
			expectedPort = IsLoadDischargeDefaultedFromOriginDestination ? "AUSYD" : "";
			AssertEquals(assertionMessage, expectedPort, quotedBooking.LoadPort);

			quotedBooking.Origin = "NZAKL";
			AssertEquals("Load Port Should NOT have defaulted as already filled or defaulting is disabled", expectedPort, quotedBooking.LoadPort);

			quotedBooking.Origin = "";
			AssertEquals("Load Port Should NOT have cleared or should be already empty if defaulting is disabled", expectedPort, quotedBooking.LoadPort);

			assertionMessage = IsLoadDischargeDefaultedFromOriginDestination ?
					 "Load Port Should have defaulted from QuotedBooking" :
					 "Load Port Should not have defaulted from QuotedBooking";

			quotedBooking.LoadPort = "";
			quotedBooking.Origin = "NZAKL";
			expectedPort = IsLoadDischargeDefaultedFromOriginDestination ? "NZAKL" : "";
			AssertEquals(assertionMessage, expectedPort, quotedBooking.LoadPort);
		}

		#endregion

		#region TestDischargePortDefaults

		public void TestDischargePortDefaults()
		{
			QuotedBooking quotedBooking = (QuotedBooking)GetNewBusinessObject();
			ForwardingShipment booking = quotedBooking.Booking;

			string assertionMessage = IsLoadDischargeDefaultedFromOriginDestination ?
							"Discharge Port Should have defaulted through booking" :
							"Discharge Port Should not have defaulted through booking";

			booking.JS_RL_NKDestination = "AU";
			string expectedPort = IsLoadDischargeDefaultedFromOriginDestination ? "AU" : "";
			AssertEquals(assertionMessage, expectedPort, quotedBooking.DischargePort);

			booking.JS_RL_NKDestination = "AUSYD";
			expectedPort = IsLoadDischargeDefaultedFromOriginDestination ? "AUSYD" : "";
			AssertEquals(assertionMessage, expectedPort, quotedBooking.DischargePort);

			quotedBooking.Destination = "NZAKL";
			AssertEquals("Discharge Port Should NOT have defaulted as already filled or defaulting is disabled", expectedPort, quotedBooking.DischargePort);

			quotedBooking.Destination = "";
			AssertEquals("Discharge Port Should NOT have cleared or should be already empty if defaulting is disabled", expectedPort, quotedBooking.DischargePort);

			assertionMessage = IsLoadDischargeDefaultedFromOriginDestination ?
					 "Discharge Port Should have defaulted from QuotedBooking" :
					 "Discharge Port Should not have defaulted from QuotedBooking";

			quotedBooking.DischargePort = "";
			quotedBooking.Destination = "NZAKL";
			expectedPort = IsLoadDischargeDefaultedFromOriginDestination ? "NZAKL" : "";
			AssertEquals(assertionMessage, expectedPort, quotedBooking.DischargePort);
		}

		#endregion

		#endregion

		#region Related Notes

		public void TestNoteContextsForRelatedNotes()
		{
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			quotedBooking.Mode = "";

			Assert("Always have Forwarding module", (((IStmNoteParent)quotedBooking).NoteContextsForRelatedNotes.Module & StmNoteContextModule.F) != 0);
			Assert("Always have 'Forwarding, Brokerage, CFS and Orders' module option", (((IStmNoteParent)quotedBooking).NoteContextsForRelatedNotes.Module & StmNoteContextModule.I) != 0);
			Assert("Always have 'Shipment and Declaration' module option", (((IStmNoteParent)quotedBooking).NoteContextsForRelatedNotes.Module & StmNoteContextModule.E) != 0);
			Assert("Not be air yet", (((IStmNoteParent)quotedBooking).NoteContextsForRelatedNotes.FreightMode & StmNoteContextFreightMode.I) == 0);

			quotedBooking.Mode = Core.Constants.RateMode.LSE;
			Assert("Should be air", (((IStmNoteParent)quotedBooking).NoteContextsForRelatedNotes.FreightMode & StmNoteContextFreightMode.I) != 0);
		}

		public void TestSpotQuoteNotesConvertToBookingNotes()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);

			var notes = new QuotedBookingStmNoteCollection(quotedBooking);
			var handlingNote = notes.AddNew();
			handlingNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			var quoteNote = notes.AddNew();
			quoteNote.ST_Description = PredefinedNoteTypes.Instance.QuoteCoverPageText.Description;

			AssertEquals("Pre-condition", quote, quoteNote.Master);
			AssertEquals("Pre-condition", quote, handlingNote.Master);

			quotedBooking.ConvertQuoteToQuotedBooking();
			var booking = quotedBooking.Booking;

			AssertNotNull("Expected to have created a booking successfully", booking);
			AssertEquals("Expected the quote only note still have the quote as it's master", quote, quoteNote.Master);
			AssertEquals("RatingHeader", quoteNote.ST_Table);

			AssertEquals("Expected the goods handling instruction note's master to have been updated to use the newly created booking", booking, handlingNote.Master);
			AssertEquals("JobShipment", handlingNote.ST_Table);

			var buildConsolHelper = new BuildConsolHelper();
			var consol = Factory.New<ForwardingConsol>();
			buildConsolHelper.MakeConsolFromBookingOrStandaloneShipment(consol, booking.PK);
			var shipment = consol.Shipments[0];

			AssertNotNull("Expected to have created a shipment from the booking", shipment);
			AssertEquals("Expected the shipment to have a note attached", 1, shipment.Notes.GetAllNotes().Count);
			AssertEquals(shipment, handlingNote.Master);
			AssertEquals("JobShipment", handlingNote.ST_Table);
		}

		public void TestSpotQuoteConversionToBooking_DoesNotDuplicateHandlingNotes()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.MiscServ.OM_EXHandlingInstuctions = "Test Handing Information";
			quotedBooking.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			quotedBooking.ConvertQuoteToQuotedBooking();

			var notes = quotedBooking.Notes.GetAllNotes().Cast<QuotedBookingStmNote>().ToList();
			AssertEquals(1, notes.Count(x => x.ST_Description == PredefinedNoteTypes.Instance.HandlingInstructions.Description));
			AssertEquals("Test Handing Information", notes[0].ST_NoteDataAsText);
		}

		#region TestQuotedBookingExposer

		internal class TestQuotedBookingExposer : QuotedBooking
		{
			public TestQuotedBookingExposer(ZGuid quotePK, ZGuid bookingPK, BusinessObjectFactory factory)
					: base(quotePK, bookingPK, false, factory)
			{
			}

			public void RunPreSaveValidationCoreForTest()
			{
				base.RunPreSaveValidationCore();
			}
		}

		#endregion

		#endregion

		#region Notes

		public void TestIsNoteParent()
		{
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			StmNote note1 = booking.Notes.AddNew();

			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			StmNote note2 = quote.Notes.AddNew();

			QuotedBooking quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);

			AssertEquals(true, quotedBooking.IsNoteParent(note1));
			AssertEquals(true, quotedBooking.IsNoteParent(note2));
			AssertEquals(false, quotedBooking.IsNoteParent(null));
			AssertEquals(false, quotedBooking.IsNoteParent(Factory.New<StmNote>()));
		}

		public void TestNoteParentPKAndTableName()
		{
			var spotQuote = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
			IStmNoteParent noteParent = spotQuote;

			AssertEquals(spotQuote.PK, noteParent.NotesParentPK);
			AssertEquals(ViewQuotedBookingSchema.Constants.TableName, noteParent.NotesParentTableName);
		}

		public void TestNotesComeFromQuoteAndBooking()
		{
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			StmNote note1 = booking.Notes.AddNew();

			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			StmNote note2 = quote.Notes.AddNew();

			QuotedBooking quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);

			AssertContainsExactElementsInAnyOrder(new[] { note1.PK, note2.PK },
					quotedBooking.Notes.GetAllNotes().Select((elem) => elem.PK));
		}

		public void TestCustomNoteTypes()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);

			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);

			var customNoteType = new PredefinedNoteType((NoResString)"Custom Note Type", StmNoteVisibility.PUB, true, false, true, true);

			var noteTypes = quotedBooking.NoteTypes;
			Assert("Custom Notes Types not load", !noteTypes.Cast<PredefinedNoteType>().Any(c => Equals(c, customNoteType)));

			quotedBooking.CustomNoteTypesDelegate = () => new NoteTypeCollection
			{
				customNoteType
			};

			noteTypes = quotedBooking.NoteTypes;
			Assert("Custom Notes Types has been load", noteTypes.Cast<PredefinedNoteType>().Any(c => Equals(c, customNoteType)));
		}

		public void TestRelatedNotesComeFromQuoteAndBooking()
		{
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			Order order = Factory.New<Order>();
			StmNote bookingOrderNote = order.Notes.AddNew();
			booking.AttachedOrders.Add(order);
			AssertCollectionContains("prerequisite", bookingOrderNote, booking.Notes.VisibleNotes);

			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			OrgHeader quotationClient = Factory.NewWithValidTestData<OrgHeader>();
			StmNote quoteClientNote = quotationClient.Notes.AddNew();
			quote.QuotationClientAddress.OrganisationPK = quotationClient.PK;
			AssertCollectionContains("prerequisite", quoteClientNote, quote.Notes.VisibleNotes);

			QuotedBooking quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			AssertContainsExactElementsInAnyOrder(new[] { bookingOrderNote.PK, quoteClientNote.PK },
					quotedBooking.Notes.VisibleNotes.Select((elem) => elem.PK));
		}

		public void TestNoteTypesComeFromQuoteAndBooking()
		{
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			foreach (var item in booking.NoteTypes)
			{
				AssertCollectionContains(item, quotedBooking.NoteTypes);
			}

			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			foreach (var item in quote.NoteTypes)
			{
				AssertCollectionContains(item, quotedBooking.NoteTypes);
			}

			quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			foreach (var item in booking.NoteTypes.Cast<PredefinedNoteType>().Union(quote.NoteTypes.Cast<PredefinedNoteType>()).Distinct())
			{
				AssertCollectionContains(item, quotedBooking.NoteTypes);
			}
		}

		public void TestQuoteNoteCanBeEditedAfterConversion()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			QuotedBooking quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			StmNote note = quotedBooking.Notes.AddNew(false, PredefinedNoteTypes.Instance.QuoteCoverPageText.Description, "cover page");

			quotedBooking.ConvertQuoteToQuotedBooking();

			note = quotedBooking.Notes.FindByDescription(PredefinedNoteTypes.Instance.QuoteCoverPageText.Description).FirstOrDefault();

			AssertNotNull(note);
			note.ST_NoteText = "updated cover page";

			Factory.Save();

			BusinessObjectFactory otherFactory = new BusinessObjectFactory();

			quote = otherFactory.Load<Quote>(quote.PK);

			note = quote.Notes.FindByDescription(PredefinedNoteTypes.Instance.QuoteCoverPageText.Description).FirstOrDefault();

			AssertNotNull(note);
			AssertEquals("updated cover page", note.ST_NoteText);
		}

		public void TestMarksAndNumsNote_SyncsWithBookingNotes()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);

			booking.JS_MarksAndNumbers = ZString.Empty;
			Factory.Save();

			var noteOnBooking = booking.Notes.FindByDescription(PredefinedNoteTypes.Instance.MarksAndNumbers.Description).FirstOrDefault();
			var noteOnQuotedBooking = quotedBooking.Notes.FindByDescription(PredefinedNoteTypes.Instance.MarksAndNumbers.Description).FirstOrDefault();

			CombineAssertions("There are no notes relating to Marks & Numbers on either booking.", () =>
			{
				AssertNull(noteOnBooking);
				AssertNull(noteOnQuotedBooking);
			});

			booking.JS_MarksAndNumbers = "spare parts";

			noteOnBooking = booking.Notes.FindByDescription(PredefinedNoteTypes.Instance.MarksAndNumbers.Description).Single();
			noteOnQuotedBooking = quotedBooking.Notes.FindByDescription(PredefinedNoteTypes.Instance.MarksAndNumbers.Description).Single();

			CombineAssertions("There is 1 note relating to Marks & Numbers on both booking, and have identical text (i.e. synced).", () =>
			{
				AssertNotNull(noteOnBooking);
				AssertNotNull(noteOnQuotedBooking);
				AssertEquals(noteOnBooking.ST_NoteText, noteOnQuotedBooking.ST_NoteText);
			});

			Factory.Save();
			AssertNoErrors(quotedBooking.Notes);
		}

		#endregion

		#region IDocumentSupportable

		public void TestDocumentSupporterIsQuotedBookingDocumentSupporter()
		{
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);

			AssertNotNull("Document Supporter", quotedBooking.DocumentSupporter);
			AssertNotNull("Is QuotedBookingDocumentSupporter", quotedBooking.DocumentSupporter as QuotedBookingDocumentSupporter);
		}

		#endregion

		#region IDocumentSupportableOverrideType

		public void TestDocumentSupportableType_OneOffQuote()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);

			AssertType("Document Supporter", typeof(QuoteDocumentSupporter), quotedBooking.DocumentSupporter);
			AssertEquals("DocumentSupportableType", typeof(Quote), quotedBooking.DocumentSupportableType);
		}

		public void TestDocumentSupportableType_BookingWithQuote_Unaccepted()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			quote.TH_Accepted = ZDate.Empty;
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);

			AssertType("Document Supporter", typeof(QuotedBookingDocumentSupporter), quotedBooking.DocumentSupporter);
			AssertEquals("DocumentSupportableType", typeof(ForwardingShipment), quotedBooking.DocumentSupportableType);
		}

		public void TestDocumentSupportableType_BookingWithQuote_Accepted()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			quote.TH_Accepted = ZDate.Today.AddDays(-1);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);

			AssertType("Document Supporter", typeof(QuotedBookingDocumentSupporter), quotedBooking.DocumentSupporter);
			AssertEquals("DocumentSupportableType", typeof(ForwardingShipment), quotedBooking.DocumentSupportableType);
		}

		public void TestDocumentSupportableType_QuickBooking()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);

			AssertType("Document Supporter", typeof(QuotedBookingDocumentSupporter), quotedBooking.DocumentSupporter);
			AssertEquals("DocumentSupportableType", typeof(ForwardingShipment), quotedBooking.DocumentSupportableType);
		}

		#endregion

		#region Template Copy

		public void TestTemplateCopy_QB()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			quotedBooking.Factory.Save();

			var quotedBookingFromFactory = Factory.Load<QuotedBooking>(new ZQuery());
			var quotesFromFactory = Factory.Load<RatingHeader>(new ZQuery());

			AssertEquals("should be 1", 1, quotedBookingFromFactory.Length);
			AssertEquals("should be 0", 0, quotesFromFactory.Length);

			var clonedQuotedBooking = (QuotedBooking)((ITemplateCopyable)quotedBooking).TemplateCopy();
			clonedQuotedBooking.Factory.Save();

			AssertNotEquals("Must be new booking", clonedQuotedBooking.Booking.PK, quotedBooking.Booking.PK);
			AssertEquals("No original quote", null, quotedBooking.Quote);
			AssertEquals("No cloned quote", null, clonedQuotedBooking.Quote);
			AssertEquals("Booking Only", QuotedBookingState.BookingOnly, clonedQuotedBooking.ObjectState);

			quotedBookingFromFactory = Factory.Load<QuotedBooking>(new ZQuery());
			quotesFromFactory = Factory.Load<RatingHeader>(new ZQuery());

			AssertContainsExactElementsInAnyOrder(new[] { quotedBooking.PK, clonedQuotedBooking.PK }, quotedBookingFromFactory.Select(x => x.PK).ToList());
			AssertEquals("should still be 0", 0, quotesFromFactory.Length);
		}

		public void TestTemplateCopy_OOQ()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			quotedBooking.Factory.Save();

			var quotedBookingFromFactory = Factory.Load<ViewQuotedBooking>(new ZQuery());
			var quotesFromFactory = Factory.Load<RatingHeader>(new ZQuery());

			AssertEquals("should be 1", 1, quotedBookingFromFactory.Length);
			AssertEquals("should be 1", 1, quotesFromFactory.Length);

			var clonedQuotedBooking = (QuotedBooking)((ITemplateCopyable)quotedBooking).TemplateCopy();
			clonedQuotedBooking.Factory.Save();

			AssertEquals("No original booking", null, quotedBooking.Booking);
			AssertEquals("No cloned booking", null, clonedQuotedBooking.Booking);
			AssertNotEquals("Must be new quote", clonedQuotedBooking.Quote.PK, quotedBooking.Quote.PK);
			Assert("Accepted Date not set", clonedQuotedBooking.Quote.TH_Accepted.IsEmpty);
			AssertEquals("Quote Only", QuotedBookingState.QuoteOnly, clonedQuotedBooking.ObjectState);

			quotedBookingFromFactory = Factory.Load<ViewQuotedBooking>(new ZQuery());
			quotesFromFactory = Factory.Load<RatingHeader>(new ZQuery());

			AssertContainsExactElementsInAnyOrder(new[] { quotedBooking.PK, clonedQuotedBooking.PK }, quotedBookingFromFactory.Select(x => x.PK).ToList());
			AssertEquals("should be 2 including the new", 2, quotedBookingFromFactory.Length);
			AssertEquals("should be 2 including the new", 2, quotesFromFactory.Length);
		}

		public void TestTemplateCopy_BWQ()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			quotedBooking.Factory.Save();

			var quotedBookingFromFactory = Factory.Load<ViewQuotedBooking>(new ZQuery());
			var quotesFromFactory = Factory.Load<RatingHeader>(new ZQuery());

			AssertEquals("should be 1", 1, quotedBookingFromFactory.Length);
			AssertEquals("should be 1", 1, quotesFromFactory.Length);

			var clonedQuotedBooking = (QuotedBooking)((ITemplateCopyable)quotedBooking).TemplateCopy();
			clonedQuotedBooking.Factory.Save();

			AssertNotEquals("Must be new booking", clonedQuotedBooking.Booking.PK, quotedBooking.Booking.PK);
			AssertNotEquals("Must be new quote", clonedQuotedBooking.Quote.PK, quotedBooking.Quote.PK);
			AssertNotNull("Accepted Date set", clonedQuotedBooking.Quote.TH_Accepted);
			AssertEquals("Accepted Booking with Quote", QuotedBookingState.AcceptedBookingWithQuote, clonedQuotedBooking.ObjectState);

			quotedBookingFromFactory = Factory.Load<ViewQuotedBooking>(new ZQuery());
			quotesFromFactory = Factory.Load<RatingHeader>(new ZQuery());

			AssertContainsExactElementsInAnyOrder(new[] { quotedBooking.PK, clonedQuotedBooking.PK }, quotedBookingFromFactory.Select(x => x.PK).ToList());
			AssertEquals("should be 2 including the new", 2, quotedBookingFromFactory.Length);
			AssertEquals("should be 2 including the new", 2, quotesFromFactory.Length);
		}

		public void TestTemplateCopy_BookingWithQuote_ChangeValue()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_RL_NKOrigin = "AUSYD";
			booking.JS_RL_NKDestination = "SGSIN";
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			quote.CurrentOneOffQuote.TT_RL_NKReceivalLocation = "AUSYD";
			quote.CurrentOneOffQuote.TT_RL_NKDeliveryLocation = "SGSIN";

			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			AssertEquals("No Discrepancy", false, quotedBooking.HasDiscrepancyFor(DiscrepancyCheck.Origin));
			var clonedQuotedBooking = (QuotedBooking)((ITemplateCopyable)quotedBooking).TemplateCopy();
			clonedQuotedBooking.Origin = "";
			AssertEquals("No Discrepancy when newly copied", false, clonedQuotedBooking.HasDiscrepancyFor(DiscrepancyCheck.Origin));
		}

		public void TestTemplateCopyWithCustomsDeclaration()
		{
			Env.Security.CustomsDeclarationEnquiryNew.IsAllowed = true;

			var booking = QuotedBooking.CreateNewBooking(Factory);
			var declaration = Factory.New<IBaseJobDeclaration>();
			declaration.JE_JS = booking.PK;
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);

			var clonedQuotedBooking = (QuotedBooking)((ITemplateCopyable)quotedBooking).TemplateCopy();
			var clonedDeclaration = Factory.LoadTop1<IBaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_JS, clonedQuotedBooking.PK));
			AssertNull("Booking copy -> Declaration not copied", clonedDeclaration);

			var clonedShipment = (ForwardingShipment)((ITemplateCopyable)booking).TemplateCopy();
			clonedDeclaration = Factory.LoadTop1<IBaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_JS, clonedShipment.PK));
			AssertNotNull("Shipment copy -> Declaration copied", clonedDeclaration);
		}

		public void TestTemplateCopyWithCustomScheduleChooser()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			var scheduleChooseCreator = new Mock<IScheduleChooserCreator>();
			scheduleChooseCreator.Setup(s => s.Create(It.IsAny<ISailingChooserParent>()))
				.Returns((Func<ISailingChooserParent, ScheduleChooser>)((parent) =>
				{
					return new ScheduleChooserForTesting(parent);
				}));

			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory, scheduleChooseCreator.Object);
			AssertEquals(typeof(ScheduleChooserForTesting), quotedBooking.ScheduleChooser.GetType());
			AssertEquals(quotedBooking, quotedBooking.ScheduleChooser.Parent);

			var clonedQuotedBooking = (QuotedBooking)((ITemplateCopyable)quotedBooking).TemplateCopy();
			AssertEquals(typeof(ScheduleChooserForTesting), clonedQuotedBooking.ScheduleChooser.GetType());
			AssertEquals(clonedQuotedBooking, clonedQuotedBooking.ScheduleChooser.Parent);
			scheduleChooseCreator.VerifyAll();
		}

		class ScheduleChooserForTesting : ScheduleChooser
		{
			public ScheduleChooserForTesting(ISailingChooserParent parent) : base(parent) { }
		}

		public void TestTemplateCopy_BookingWithColoadMasterShipment()
		{
			var masterShipment = Factory.New<ForwardingShipment>();
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);

			quotedBooking.Booking.JS_IsForwardRegistered = true;
			quotedBooking.Booking.JS_JS_ColoadMasterShipment = masterShipment.PK;

			var clonedQuotedBooking = (QuotedBooking)((ITemplateCopyable)quotedBooking).TemplateCopy();

			AssertEquals("Should NOT copy JS_JS_ColoadMasterShipment", ZGuid.Empty, clonedQuotedBooking.Booking.JS_JS_ColoadMasterShipment);
		}

		public void TestTemplateCopy_DefaultJS_ShipmentAsBookedFromConsolidatedBooking()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);

			quotedBooking.Booking.JS_IsForwardRegistered = true;
			quotedBooking.Booking.JS_ShipmentStatus = string.Empty;

			var clonedQuotedBooking = (QuotedBooking)((ITemplateCopyable)quotedBooking).TemplateCopy();

			AssertEquals("Default JS_ShipmentStatus as BKD", ShipmentStatusList.Codes.Booked, clonedQuotedBooking.Booking.JS_ShipmentStatus);
		}

		public void TestTemplateCopyDefaultTheRightDepartment()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quickBooking = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);

			booking.JS_TransportMode = Constants.TransportModes.Air;
			booking.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			booking.JS_RL_NKDestination = "CAAAB";

			quickBooking.TryLoadOrCreateJob();
			using (var job = quickBooking.Job)
			{
				AssertEquals("Precondition: booking dedepartment should be FEA before adding CFS to shipment", "FEA", Factory.Load<GlbDepartment>(job.JH_GE).GE_Code);
			}

			var proxy = GlbBranch.CurrentBranch.OrgProxy;

			booking.JS_OA_ImportReleaseDepot = proxy.MainAddress.PK;
			Assert("Precondition: shipment should not be cfs registered.", !booking.JS_IsCFSRegistered);

			var clonedBooking = (QuotedBooking)((ITemplateCopyable)quickBooking).TemplateCopy();

			clonedBooking.TryLoadOrCreateJob();
			using (var job = clonedBooking.Job)
			{
				AssertEquals("CFS related fields should not be copied", "FEA", Factory.Load<GlbDepartment>(job.JH_GE).GE_Code);
			}
		}

		public void TestTemplateCopyCopyCFSRelatedFieldsForBookingCorrectly()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quickBooking = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);

			booking.JS_TransportMode = Constants.TransportModes.Air;
			booking.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			booking.JS_RL_NKDestination = "CAAAB";

			quickBooking.TryLoadOrCreateJob();
			using (var job = quickBooking.Job)
			{
				AssertEquals("Precondition: booking department should be FEA before adding CFS to shipment", "FEA", Factory.Load<GlbDepartment>(job.JH_GE).GE_Code);
			}

			var proxy = GlbBranch.CurrentBranch.OrgProxy;

			booking.JS_OA_ExportReceivingDepot = proxy.MainAddress.PK;
			booking.JS_OA_ImportReleaseDepot = proxy.MainAddress.PK;
			Assert("Precondition: shipment should not be CFS registered.", !booking.JS_IsCFSRegistered);

			var clonedBooking = (QuotedBooking)((ITemplateCopyable)quickBooking).TemplateCopy();

			AssertEquals("JS_OA_ExportReceivingDepot should be copied.", booking.JS_OA_ExportReceivingDepot, clonedBooking.Booking.JS_OA_ExportReceivingDepot);
			AssertEquals("JS_OA_ImportReleaseDepot should be copied.", booking.JS_OA_ImportReleaseDepot, clonedBooking.Booking.JS_OA_ImportReleaseDepot);
			Assert("Precondition: shipment should not be cfs registered.", !clonedBooking.Booking.JS_IsCFSRegistered);

			clonedBooking.TryLoadOrCreateJob();
			using (var job = clonedBooking.Job)
			{
				AssertEquals("department should still be defaulted from booking", "FEA", Factory.Load<GlbDepartment>(job.JH_GE).GE_Code);
			}
		}

		public void TestTemplateCopy_ExistingRecord()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			Factory.Save();

			var clonedQuotedBooking = (QuotedBooking)((ITemplateCopyable)quotedBooking).TemplateCopy();
			Factory.Save();
			AssertNotEquals("Must be new booking", clonedQuotedBooking.Booking.PK, quotedBooking.Booking.PK);
			AssertNotEquals("Must be new quote", clonedQuotedBooking.Quote.PK, quotedBooking.Quote.PK);
			AssertNotEquals("Must be new quote", clonedQuotedBooking.Booking.JS_TH_OneTimeQuote, quotedBooking.Booking.JS_TH_OneTimeQuote);
		}

		public void TestTemplateCopy_ShouldNotCopyInterimReceipt()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_InterimReceipt = "1234";
			booking.JS_A_RCV = ZDateTime.Today;
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			Factory.Save();

			var clonedQuotedBooking = (QuotedBooking)((ITemplateCopyable)quotedBooking).TemplateCopy();
			Factory.Save();
			AssertEquals(ZString.Empty, clonedQuotedBooking.Booking.JS_InterimReceipt);
			AssertEquals(ZDateTime.Empty, clonedQuotedBooking.Booking.JS_A_RCV);
			AssertNull("InterimReceiptProduced event should not be logged", clonedQuotedBooking.Logs.MostRecentLogByEventTime(AutoEvents.InterimReceiptProduced));
			AssertNull("InterimReceiptProduced event should not be logged", clonedQuotedBooking.Booking.Logs.MostRecentLogByEventTime(AutoEvents.InterimReceiptProduced));
		}

		public void TestTemplateCopyOnlyQuote()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var quotedBooking = (QuotedBooking)GetNewBusinessObject();
			quotedBooking.ClientPK = client.PK;
			var clonedQuotedBooking = (QuotedBooking)quotedBooking.TemplateCopyOnlyQuote();

			AssertNull("No cloned booking", clonedQuotedBooking.Booking);
			AssertNotNull("Must be cloned quote", clonedQuotedBooking.Quote);
			AssertEquals(quotedBooking.Quote.TH_ClientCode, clonedQuotedBooking.Quote.TH_ClientCode);
		}

		public void TestTemplateCopyOnlyQuoteForCo2()
		{
			var quotedBooking = (QuotedBooking)GetNewBusinessObject();
			quotedBooking.SetCO2ePerTonneInKg(50);
			quotedBooking.SetTotalCO2e(100);

			var clonedQuotedBooking = (QuotedBooking)quotedBooking.TemplateCopyOnlyQuote();

			AssertEquals(50m, clonedQuotedBooking.GetCO2ePerTonneInKg());
			AssertEquals(100m, clonedQuotedBooking.GetTotalCO2e());
		}

		public void TestTemplateCopyForCo2()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			quotedBooking.SetCO2ePerTonneInKg(50);
			quotedBooking.SetTotalCO2e(100);

			var clonedQuotedBooking = (QuotedBooking)((ITemplateCopyable)quotedBooking).TemplateCopy();
			AssertEquals(50m, clonedQuotedBooking.GetCO2ePerTonneInKg());
			AssertEquals(100m, clonedQuotedBooking.GetTotalCO2e());
		}

		#endregion

		#region QuotedBookingProcessTasksProvider

		public void TestWorkflowItemsType()
		{
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			Quote quote = null;
			QuotedBooking quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);

			AssertEquals(typeof(QuotedBookingProcessTaskCollection), ((IWorkflowProvider)quotedBooking).WorkflowItems.GetType());

			booking = null;
			quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);

			AssertEquals(typeof(QuotedBookingProcessTaskCollection), ((IWorkflowProvider)quotedBooking).WorkflowItems.GetType());

			booking = QuotedBooking.CreateNewBooking(Factory);
			quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);

			AssertEquals(typeof(QuotedBookingProcessTaskCollection), ((IWorkflowProvider)quotedBooking).WorkflowItems.GetType());
		}

		public void TestWorkflowTemplateClientPriority()
		{
			var client = Factory.New<OrgHeader>();
			client.OH_Code = "TSTAUS";
			client.MainAddress.Address1 = "1 APE DR";
			var clientAddr = client.MainAddress;

			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_Name = "Client-Specific";
			template1.P0_OH_Client = client.PK;
			template1.P0_ProcessType = "QBK";
			template1.P0_SubType1 = "SEA";
			template1.P0_SubType2 = "QBN";

			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_Name = "Non-Client-Specific";
			template2.P0_ProcessType = "QBK";
			template2.P0_SubType1 = "SEA";
			template2.P0_SubType2 = "QBN";
			template2.P0_LoadPortCountry = "AU";

			Factory.Save();

			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			booking.CreateShipmentJobHeaderWithMutex();

			quotedBooking.Origin = "AUBNE";
			quotedBooking.Destination = "USLAX";
			quotedBooking.ClientAddrPK = clientAddr.PK;
			quotedBooking.Mode = Core.Constants.RateMode.LCL;

			var loader = new ProcessTaskTemplate.Loader(Factory);
			var matches = loader.FindMatches(quotedBooking);
			AssertEquals("Client-Specific Template should be the 1st match", template1.PK, matches[0].PK);
			AssertEquals("Non-Client-Specific Template should be be 2nd match", template2.PK, matches[1].PK);

			booking.Job.Dispose();
		}

		#endregion

		#region Courier

		public void TestCourier()
		{
			AssertEquals(Core.Constants.TransportModes.Courier, RatingConstants.GetTransportModeFromMode(Core.Constants.RateMode.COU));
			AssertEquals(Core.Constants.ContainerModes.OnBoardCourier, RatingConstants.GetContainerModeFromMode(Core.Constants.RateMode.COU));
		}

		#endregion

		#region HumanReadableName

		public virtual void TestHumanReadableName()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			QuotedBooking qb = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
			Factory.Save();
			AssertEquals("One Off Quote - Quote (" + quote.TH_QuoteNumber + ")", qb.HumanReadableName);

			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			qb = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);
			Factory.Save();
			AssertEquals("Quick Booking - Booking (" + booking.JS_UniqueConsignRef + ")", qb.HumanReadableName);

			quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			booking = QuotedBooking.CreateNewBooking(Factory);
			Factory.Save();
			qb = CreateNewQuotedBooking(quote.PK, booking.PK);
			Factory.Save();
			AssertEquals("Booking with Quote - Quote (" + quote.TH_QuoteNumber + ") - Booking (" + booking.JS_UniqueConsignRef + ")", qb.HumanReadableName);
		}

		#endregion

		#region HumanReadableShortcutName

		public void TestHumanReadableShortcutName()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var qb = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
			Factory.Save();
			AssertEquals(quote.TH_QuoteNumber, qb.HumanReadableShortcutName);

			var booking = QuotedBooking.CreateNewBooking(Factory);
			qb = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);
			Factory.Save();
			AssertEquals(booking.JS_UniqueConsignRef, qb.HumanReadableShortcutName);

			quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			booking = QuotedBooking.CreateNewBooking(Factory);
			Factory.Save();
			qb = CreateNewQuotedBooking(quote.PK, booking.PK);
			Factory.Save();
			AssertEquals(quote.TH_QuoteNumber + " - " + booking.JS_UniqueConsignRef, qb.HumanReadableShortcutName);
		}

		#endregion

		#region PK

		public void TestPK()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			QuotedBooking qb = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
			Factory.Save();
			AssertEquals(quote.PK, qb.PK);

			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			qb = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);
			Factory.Save();
			AssertEquals(booking.PK, qb.PK);

			quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			Factory.Save();
			booking = QuotedBooking.CreateNewBooking(Factory);
			qb = CreateNewQuotedBooking(quote.PK, booking.PK);
			Factory.Save();
			AssertEquals(quote.PK, qb.PK);
		}

		#endregion

		public void TestTablePrefix()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			QuotedBooking quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
			AssertEquals("VB", quotedBooking.TablePrefix);
		}

		#region Charges

		public void TestDepartmentChargesSetAfterQuoteCharges()
		{
			var quotedBooking = GetQuoteOnlyQuotedBooking();
			quotedBooking.TransportMode = "SEA";
			quotedBooking.Quote.TH_OneTimeQuote = true;
			quotedBooking.Quote.TH_QuoteNumber = "123456";

			var quoteJob = new JobHeader.Loader(quotedBooking.Quote).TryLoadOrCreate();
			var department = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FIS");

			var chargeCode1 = LoadAccChargeCode("BAF");
			var chargeCode2 = LoadAccChargeCode("DADF");
			var chargeCode3 = LoadAccChargeCode("CAF");
			var chargeCode4 = LoadAccChargeCode("FRT");

			AddNewDepartmentCharge(department, chargeCode1);
			AddNewDepartmentCharge(department, chargeCode2);
			AddNewDepartmentCharge(department, chargeCode3);

			var deptCharge4 = AddNewDepartmentCharge(department, chargeCode4);
			quoteJob.JH_GE = department.PK;

			var quoteCharges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, quoteJob.PK));

			Factory.Save();

			AssertContainsExactElementsInAnyOrder("quote charges", new[]
			{
				"Bunker Adjustment Factor",
				"Destination Airline Document Fee",
				"Currency Adjustment Factor",
				"International Freight"
			},
			FormatCharges(quoteCharges));

			foreach (var currentCharge in quoteCharges)
			{
				AssertEquals("Charge should have correct JR_JH link", quoteJob.PK, currentCharge.JR_JH);
				AssertNotNull("Charge should have correct JR_AC link", currentCharge.ChargeCode);
			}

			deptCharge4.Delete();

			var chargeCode5 = LoadAccChargeCode("ECCLR");
			AddNewDepartmentCharge(department, chargeCode5);
			quotedBooking.ConvertQuoteToQuotedBooking();

			var bookingPK = quotedBooking.Booking.PK;
			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			var buildConsolHelper = new BuildConsolHelper();
			buildConsolHelper.MakeConsolFromBookingOrStandaloneShipment(forwardingConsol, bookingPK);

			var shipment = forwardingConsol.Shipments[0];
			shipment.JS_RL_NKOrigin = "HKHKG";
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.CreateShipmentJobHeaderWithMutex();

			var shipmentJob = shipment.Job;
			shipmentJob.JH_GE = Env.CurrentDepartmentPK;
			shipmentJob.JH_GE = department.PK;
			var shipmentCharges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, shipmentJob.PK));

			Factory.Save();

			AssertNotNull(shipmentJob);
			AssertContainsExactElementsInAnyOrder("shipment charges", new[]
			{
				"Bunker Adjustment Factor",
				"Destination Airline Document Fee",
				"Currency Adjustment Factor",
				"Export Customs Clearance Fee",
				"International Freight"
			},
			FormatCharges(shipmentCharges));

			foreach (var currentCharge in shipmentCharges)
			{
				AssertEquals("Charge should have correct JR_JH link", shipmentJob.PK, currentCharge.JR_JH);
				AssertNotNull("Charge should have correct JR_AC link", currentCharge.ChargeCode);
			}

			quoteJob.Dispose();
			shipmentJob.Dispose();
		}

		AccChargeCode LoadAccChargeCode(string code)
		{
			var query = new ZQuery(AccChargeCodeSchema.AC_Code, code);
			query.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);

			return Factory.LoadTop1<AccChargeCode>(query);
		}

		IEnumerable<string> FormatCharges(IEnumerable<JobCharge> charges)
		{
			return charges.Select(charge => string.Format("{0}", charge.JR_Desc));
		}

		public void TestConvertQuoteToQuotedBooking_CleanupPossibleCarriers()
		{
			var quotedBooking = GetQuoteOnlyQuotedBooking();
			quotedBooking.TransportMode = "SEA";
			quotedBooking.Quote.TH_OneTimeQuote = true;
			quotedBooking.Quote.TH_QuoteNumber = "123456";

			var quoteJob = new JobHeader.Loader(quotedBooking.Quote).TryLoadOrCreate();

			var chargeCode1 = LoadAccChargeCode("FRT");

			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier3 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_IsCreditor = true;
			carrier2.OH_IsCreditor = true;
			carrier3.OH_IsCreditor = true;
			var otherCreditor = Factory.NewWithValidTestData<OrgHeader>();
			otherCreditor.OH_IsCreditor = true;

			var jobCharge1 = CreateJobCharge(chargeCode1, quoteJob, carrier1.PK);
			var jobCharge2 = CreateJobCharge(chargeCode1, quoteJob, carrier2.PK);
			var jobCharge3 = CreateJobCharge(chargeCode1, quoteJob, carrier3.PK);
			var jobCharge4 = CreateJobCharge(chargeCode1, quoteJob, otherCreditor.PK);
			var jobCharge5 = CreateJobCharge(chargeCode1, quoteJob, ZGuid.Empty);

			var oneOffQuote = quotedBooking.Quote.CurrentOneOffQuote;
			oneOffQuote.TT_OH_Carrier = carrier1.PK;
			oneOffQuote.PossibleCarriers.AddNew().TTC_OH_Carrier = carrier1.PK;
			oneOffQuote.PossibleCarriers.AddNew().TTC_OH_Carrier = carrier2.PK;
			oneOffQuote.PossibleCarriers.AddNew().TTC_OH_Carrier = carrier3.PK;

			Factory.Save();

			quotedBooking.ConvertQuoteToQuotedBooking();

			CombineAssertions(() =>
			{
				AssertEquals("charge with main carrier creditor is not deleted", false, jobCharge1.IsDeleted);
				AssertEquals("charge with possible carrier is deleted", true, jobCharge2.IsDeleted);
				AssertEquals("charge with possible carrier is deleted", true, jobCharge3.IsDeleted);
				AssertEquals("charge with unrelated creditor is not deleted", false, jobCharge4.IsDeleted);
				AssertEquals("charge with blank creditor is not deleted", false, jobCharge5.IsDeleted);
				AssertEquals("PossibleCarriers is cleared", 0, oneOffQuote.PossibleCarriers.Count);
			});
		}

		public void TestConvertQuoteToQuotedBooking_CleanupPossibleCarriers_WhenThereAreNone()
		{
			var quotedBooking = GetQuoteOnlyQuotedBooking();
			quotedBooking.TransportMode = "SEA";
			quotedBooking.Quote.TH_OneTimeQuote = true;
			quotedBooking.Quote.TH_QuoteNumber = "123456";

			var quoteJob = new JobHeader.Loader(quotedBooking.Quote).TryLoadOrCreate();

			var chargeCode1 = LoadAccChargeCode("BAF");
			var chargeCode2 = LoadAccChargeCode("DADF");

			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_IsCreditor = true;

			var jobCharge1 = CreateJobCharge(chargeCode1, quoteJob, carrier1.PK);
			var jobCharge2 = CreateJobCharge(chargeCode2, quoteJob, ZGuid.Empty);

			var oneOffQuote = quotedBooking.Quote.CurrentOneOffQuote;
			oneOffQuote.TT_OH_Carrier = carrier1.PK;

			Factory.Save();

			quotedBooking.ConvertQuoteToQuotedBooking();

			CombineAssertions(() =>
			{
				AssertEquals("charge with main carrier creditor is not deleted", false, jobCharge1.IsDeleted);
				AssertEquals("charge with blank creditor is not deleted", false, jobCharge2.IsDeleted);
			});
		}

		JobCharge CreateJobCharge(AccChargeCode chargeCode, JobHeader jobHeader, ZGuid creditorOrgPK)
		{
			JobCharge jobCharge = Factory.New<JobCharge>();
			jobCharge.JR_AC = chargeCode.PK;
			jobCharge.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			jobCharge.JR_JH = jobHeader.PK;
			jobCharge.JR_RX_NKCostCurrency = "AUD";
			jobCharge.JR_RX_NKSellCurrency = "AUD";
			jobCharge.JR_LocalCostAmt = 200m;
			jobCharge.JR_OSCostAmt = 200m;
			jobCharge.JR_OH_CostAccount = creditorOrgPK;
			return jobCharge;
		}

		#endregion

		#region TestIJobInvoicingPlugin

		public void TestEditSecurity()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);

			AssertEquals("EditSecurityCheckPoint should be Empty", Env.Security.None, quotedBooking.InvoicingSupporter.EditSecurityCheckpoint);
			AssertEquals("EditSecuritytMessage should be Empty", ZString.Empty, quotedBooking.InvoicingSupporter.EditSecurityMessage);
			AssertEquals("EditSecurityLock should be False", ZBool.False, quotedBooking.InvoicingSupporter.EditSecurityLock);
		}

		public void TestIJobInvoicingPlugInDefaultChargeGroup()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);

			AssertEquals("DefaultChargeGroup should be Empty", ZString.Empty, booking.InvoicingSupporter.DefaultChargeGroup);
		}

		#endregion

		#region IJobHeaderParent Membrers

		public void TestIJobHeaderParent_AllowInvoiceDeletion()
		{
			IJobHeaderParent quotedBooking = (QuotedBooking)GetNewBusinessObject();
			Assert(quotedBooking.AllowInvoiceDeletion);
		}

		#endregion

		#region Origin/Destination Defaulting

		public void TestOriginDestinationDefaulting_SpotQuote()
		{
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_RL_NKClosestPort = "AUSYD";
			OrgAddress consignorAddress = consignor.MainAddress;
			consignorAddress.OA_Code = "ConsignorAddress1";
			consignorAddress.OA_Address1 = "ConsignorAddress1";

			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_RL_NKClosestPort = "NZAKL";
			OrgAddress consigneeAddress = consignee.MainAddress;
			consigneeAddress.OA_Code = "ConsigneeAddress1";
			consigneeAddress.OA_Address1 = "ConsigneeAddress1";

			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			QuotedBooking quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);

			quotedBooking.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			AssertEquals("AUSYD", quotedBooking.Origin);

			quotedBooking.ConsignorDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			quotedBooking.Origin = "";
			consignorAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			quotedBooking.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			AssertEquals("AUBNE", quotedBooking.Origin);

			quotedBooking.Origin = "";
			quotedBooking.ConsignorDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			quotedBooking.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			AssertEquals("", quotedBooking.Origin);

			quotedBooking.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			AssertEquals("NZAKL", quotedBooking.Destination);

			quotedBooking.ConsigneeDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			quotedBooking.Destination = "";
			consigneeAddress.OA_RL_NKRelatedPortCode = "NZCHR";
			quotedBooking.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			AssertEquals("NZCHR", quotedBooking.Destination);

			quotedBooking.Destination = "";
			quotedBooking.ConsigneeDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			quotedBooking.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			AssertEquals("", quotedBooking.Destination);
		}

		#endregion

		public void TestEquipmentsForSeaSpotQuote()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			QuotedBooking quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
			quotedBooking.Mode = Core.Constants.RateMode.SEA;
			quotedBooking.PickupEquipment = Constants.FCLEquipmentNeeded.WaitForUnpack;
			quotedBooking.DeliveryEquipment = Constants.FCLEquipmentNeeded.WaitForUnpack;
			quotedBooking.Validation.ValidateAll();
			AssertNoErrors(quote.OneOffQuote[0].TT_PickupEquipmentInfo);
			AssertNoErrors(quote.OneOffQuote[0].TT_DeliveryEquipmentInfo);
		}

		public void TestQuoteNumberOfEntries()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			quote.CurrentOneOffQuote.TT_NumberOfEntries = 3;
			QuotedBooking quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);

			AssertEquals(quote.CurrentOneOffQuote.TT_NumberOfEntries, quotedBooking.QuoteNumberOfEntries);
			AssertEquals(false, CargoWise.Common.ReflectionUtil.GetPropertyValue(quotedBooking, "QuoteNumberOfEntries_ReadOnly"));

			int hitCount = 0;
			EventHandler incrementHitCount = (s, e) => hitCount++;

			quote.CurrentOneOffQuote.TT_NumberOfEntriesInfo.ValueChanged += incrementHitCount;
			quotedBooking.QuoteNumberOfEntriesInfo.ValueChanged += incrementHitCount;

			quote.CurrentOneOffQuote.TT_NumberOfEntries = 5;
			AssertEquals(2, hitCount);

			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);

			AssertEquals((ZShort)0, quotedBooking.QuoteNumberOfEntries);
			AssertEquals(true, CargoWise.Common.ReflectionUtil.GetPropertyValue(quotedBooking, "QuoteNumberOfEntries_ReadOnly"));
		}

		public void TestQuoteNumberOfEntryLines()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			quote.CurrentOneOffQuote.TT_NumberOfEntryLines = 3;
			QuotedBooking quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);

			AssertEquals(quote.CurrentOneOffQuote.TT_NumberOfEntryLines, quotedBooking.QuoteNumberOfEntryLines);
			AssertEquals(false, CargoWise.Common.ReflectionUtil.GetPropertyValue(quotedBooking, "QuoteNumberOfEntryLines_ReadOnly"));

			int hitCount = 0;
			EventHandler incrementHitCount = (s, e) => hitCount++;

			quote.CurrentOneOffQuote.TT_NumberOfEntryLinesInfo.ValueChanged += incrementHitCount;
			quotedBooking.QuoteNumberOfEntryLinesInfo.ValueChanged += incrementHitCount;

			quote.CurrentOneOffQuote.TT_NumberOfEntryLines = 5;
			AssertEquals(2, hitCount);

			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);

			AssertEquals((ZShort)0, quotedBooking.QuoteNumberOfEntryLines);
			AssertEquals(true, CargoWise.Common.ReflectionUtil.GetPropertyValue(quotedBooking, "QuoteNumberOfEntryLines_ReadOnly"));
		}

		public void TestIsCancelled()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			QuotedBooking quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
			AssertEquals(false, quotedBooking.IsCancelled);
			AssertEquals(false, quote.IsCancelled);

			quote.IsCancelled = true;
			AssertEquals(true, quotedBooking.IsCancelled);

			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);
			AssertEquals(false, quotedBooking.IsCancelled);
			AssertEquals(false, booking.IsCancelled);

			booking.IsCancelled = true;
			AssertEquals(true, quotedBooking.IsCancelled);

			quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			booking = QuotedBooking.CreateNewBooking(Factory);

			quotedBooking = CreateNewQuotedBooking(quote.PK, booking.PK);
			AssertEquals(false, quotedBooking.IsCancelled);
			AssertEquals(false, quote.IsCancelled);
			AssertEquals(false, booking.IsCancelled);

			booking.IsCancelled = true;
			AssertEquals(true, quotedBooking.IsCancelled);

			quote.IsCancelled = true;
			AssertEquals(true, quotedBooking.IsCancelled);
		}

		public void TestIsDomestic()
		{
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);

			quotedBooking.Origin = "AUSYD";
			quotedBooking.Destination = "AUSYD";
			AssertEquals(true, quotedBooking.IsDomestic());

			quotedBooking.Destination = "NZAKL";
			AssertEquals(false, quotedBooking.IsDomestic());
		}

		public void TestQuotedBookingTypeCode()
		{
			QuotedBooking quotedBooking = CreateNewQuotedBooking(QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted).PK, ZGuid.Empty);
			AssertEquals(QuotedBookingState.QuoteOnly, quotedBooking.ObjectState);
			AssertEquals(QuotedBooking.SpotQuoteCode, quotedBooking.QuotedBookingTypeCode);

			quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, QuotedBooking.CreateNewBooking(Factory).PK);
			AssertEquals(QuotedBookingState.BookingOnly, quotedBooking.ObjectState);
			AssertEquals(QuotedBooking.QuickBookingCode, quotedBooking.QuotedBookingTypeCode);

			quotedBooking = CreateNewQuotedBooking(QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted).PK, QuotedBooking.CreateNewBooking(Factory).PK);
			AssertEquals(QuotedBookingState.AcceptedBookingWithQuote, quotedBooking.ObjectState);
			AssertEquals(QuotedBooking.BookingWithQuoteCode, quotedBooking.QuotedBookingTypeCode);

			quotedBooking = CreateNewQuotedBooking(QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted).PK, QuotedBooking.CreateNewBooking(Factory).PK);
			AssertEquals(QuotedBookingState.UnacceptedBookingWithQuote, quotedBooking.ObjectState);
			AssertEquals(QuotedBooking.BookingWithQuoteCode, quotedBooking.QuotedBookingTypeCode);
		}

		public void TestQuotePropertiesEnableSave()
		{
			GlbDepartment department = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FEA");

			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			booking.CreateShipmentJobHeaderWithMutex();
			booking.Job.JH_GE = department.PK;
			booking.Job.JH_GB = GlbBranch.CurrentBranch.PK;
			QuotedBooking quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			quotedBooking.Origin = "AUBNE";
			quotedBooking.Destination = "QADOH";

			Factory.Save();
			quotedBooking.TransitTime = "6";
			Assert("Transit time should enable save", quotedBooking.HasChanges);

			Factory.Save();
			quotedBooking.Frequency = 5;
			Assert("Frequency should enable save", quotedBooking.HasChanges);

			Factory.Save();
			quotedBooking.FrequencyUnit = "WEEKS";
			Assert("Frequency unit should enable save", quotedBooking.HasChanges);

			Factory.Save();
			quotedBooking.QuoteNumberOfEntries = 2;
			Assert("Number of entries should enable save", quotedBooking.HasChanges);

			Factory.Save();
			quotedBooking.QuoteNumberOfEntryLines = 2;
			Assert("Number of entry lines should enable save", quotedBooking.HasChanges);

			Factory.Save();
			quotedBooking.Via = "USNYC";
			Assert("Via should enable save", quotedBooking.HasChanges);

			Factory.Save();
			quotedBooking.StartDate = ZDateTime.BrettsBirthday;
			Assert("Start date should enable save", quotedBooking.HasChanges);

			Factory.Save();
			quotedBooking.EndDate = ZDate.Today;
			Assert("End date should enable save", quotedBooking.HasChanges);

			Factory.Save();
			quotedBooking.Commodity = "AABT";
			Assert("Commodity should enable save", quotedBooking.HasChanges);
		}

		[TestDate(2010, 7, 25)]
		public void TestIncotermsQuote2010()
		{
			QuotedBooking quotedBooking = CreateNewQuotedBooking(QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted).PK, ZGuid.Empty);
			new JobHeader.Loader(quotedBooking).TryLoadOrCreate().JH_GE = GlbDepartment.CurrentDepartment.PK;

			quotedBooking.Origin = "AUSYD";
			quotedBooking.Destination = "AUBNE";

			AssertIncotermLookupCodes(quotedBooking, domesticPaymentTerms);

			quotedBooking.Destination = "NZAKL";

			AssertIncotermLookupCodes(quotedBooking, Constants.IncoTerms.Incoterms2000);

			Factory.Save();

			AssertIncotermLookupCodes(quotedBooking, Constants.IncoTerms.Incoterms2000);
		}

		[TestDate(2011, 7, 25)]
		public void TestIncotermsQuote2011()
		{
			QuotedBooking quotedBooking = CreateNewQuotedBooking(QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted).PK, ZGuid.Empty);
			(new JobHeader.Loader(quotedBooking).TryLoadOrCreate()).JH_GE = GlbDepartment.CurrentDepartment.PK;

			quotedBooking.Origin = "AUSYD";
			quotedBooking.Destination = "AUBNE";

			AssertIncotermLookupCodes(quotedBooking, domesticPaymentTerms);

			quotedBooking.Destination = "NZAKL";

			AssertIncotermLookupCodes(quotedBooking, Constants.IncoTerms.Incoterms2010);

			Factory.Save();

			AssertIncotermLookupCodes(quotedBooking, Constants.IncoTerms.Incoterms2010);
		}

		[TestDate(2010, 7, 25)]
		public void TestIncotermsBooking2010()
		{
			QuotedBooking quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, QuotedBooking.CreateNewBooking(Factory).PK);

			quotedBooking.Origin = "AUSYD";
			quotedBooking.Destination = "AUBNE";

			AssertIncotermLookupCodes(quotedBooking, domesticPaymentTerms);

			quotedBooking.Destination = "NZAKL";

			AssertIncotermLookupCodes(quotedBooking, Constants.IncoTerms.Incoterms2000);

			Factory.Save();

			AssertIncotermLookupCodes(quotedBooking, Constants.IncoTerms.Incoterms2000);
		}

		[TestDate(2011, 7, 25)]
		public void TestIncotermsBooking2011()
		{
			QuotedBooking quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, QuotedBooking.CreateNewBooking(Factory).PK);

			quotedBooking.Origin = "AUSYD";
			quotedBooking.Destination = "AUBNE";

			AssertIncotermLookupCodes(quotedBooking, domesticPaymentTerms);

			quotedBooking.Destination = "NZAKL";

			AssertIncotermLookupCodes(quotedBooking, Constants.IncoTerms.Incoterms2010);

			Factory.Save();

			AssertIncotermLookupCodes(quotedBooking, Constants.IncoTerms.Incoterms2010);
		}

		[TestDate(2010, 7, 25)]
		public void TestIncotermsBookingWithQuote2010()
		{
			QuotedBooking quotedBooking = CreateNewQuotedBooking(QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted).PK, QuotedBooking.CreateNewBooking(Factory).PK);
			(new JobHeader.Loader(quotedBooking).TryLoadOrCreate()).JH_GE = GlbDepartment.CurrentDepartment.PK;
			(new JobHeader.Loader(quotedBooking.Booking).TryLoadOrCreate()).JH_GE = GlbDepartment.CurrentDepartment.PK;

			quotedBooking.Origin = "AUSYD";
			quotedBooking.Destination = "AUBNE";

			AssertIncotermLookupCodes(quotedBooking, domesticPaymentTerms);

			quotedBooking.Destination = "NZAKL";

			AssertIncotermLookupCodes(quotedBooking, Constants.IncoTerms.Incoterms2000);

			Factory.Save();

			AssertIncotermLookupCodes(quotedBooking, Constants.IncoTerms.Incoterms2000);
		}

		[TestDate(2011, 7, 25)]
		public void TestIncotermsBookingWithQuote2011()
		{
			QuotedBooking quotedBooking = CreateNewQuotedBooking(QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted).PK, QuotedBooking.CreateNewBooking(Factory).PK);
			(new JobHeader.Loader(quotedBooking).TryLoadOrCreate()).JH_GE = GlbDepartment.CurrentDepartment.PK;
			(new JobHeader.Loader(quotedBooking.Booking).TryLoadOrCreate()).JH_GE = GlbDepartment.CurrentDepartment.PK;

			quotedBooking.Origin = "AUSYD";
			quotedBooking.Destination = "AUBNE";

			AssertIncotermLookupCodes(quotedBooking, domesticPaymentTerms);

			quotedBooking.Destination = "NZAKL";

			AssertIncotermLookupCodes(quotedBooking, Constants.IncoTerms.Incoterms2010);

			Factory.Save();

			AssertIncotermLookupCodes(quotedBooking, Constants.IncoTerms.Incoterms2010);
		}

		readonly string[] domesticPaymentTerms = new string[]
		{
					Core.Constants.DomesticPaymentTerms.Collect,
					Core.Constants.DomesticPaymentTerms.CollectCOD,
					Core.Constants.DomesticPaymentTerms.CollectThirdParty,
					Core.Constants.DomesticPaymentTerms.Prepaid
		};

		void AssertIncotermLookupCodes(QuotedBooking quotedBooking, IEnumerable<string> expectedCodes)
		{
			string[] actualCodes = (from code in quotedBooking.IncoTerms.Cast<CodeDescriptionPair>() select code.Code).ToArray();
			AssertContainsExactElementsInAnyOrder(expectedCodes, actualCodes);
		}

		public void TestAdditionalTerms()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			QuotedBooking quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);

			AssertEquals("No additional terms for quote only", ZString.Empty, quotedBooking.AdditionalTerms);
			AssertEquals("Additional terms are NOT readonly for quote", false, quotedBooking.AdditionalTermsInfo.ReadOnly);

			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_AdditionalTerms = "follow the write rabbit";
			quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);

			AssertEquals("follow the write rabbit", quotedBooking.AdditionalTerms);
			AssertEquals(false, quotedBooking.AdditionalTermsInfo.ReadOnly);

			quotedBooking.AdditionalTerms = "hello";
			AssertEquals("hello", quotedBooking.AdditionalTerms);
			AssertEquals("hello", booking.JS_AdditionalTerms);
		}

		public void TestUpdateCO2eStatusToNotCurrent_WhenContainerModeIsChangedInOneOffQuote()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);

			try
			{
				quotedBooking.TransportMode = Constants.TransportModes.Sea;
				quotedBooking.ContainerMode = Constants.ContainerModes.LCL;
				var cont = quotedBooking.Quote.CurrentOneOffQuote.Containers.AddNew();
				var refCont = NewRefContainer("20GP111", "22G0", 1m, 2280m);
				cont.TC_RC = refCont.PK;
				cont.TC_ContainerCount = 1;
				quotedBooking.SetCO2eStatus(CO2eStatusList.Codes.Current);
				AssertEquals(CO2eStatusList.Codes.Current, quotedBooking.GetCO2eStatus());
				Factory.Save();

				quotedBooking.ContainerMode = Constants.ContainerModes.FCL;
				AssertEquals(CO2eStatusList.Codes.NotCurrent, quotedBooking.GetCO2eStatus());
				CO2eTestHelper.AssertSTUEvent(quotedBooking, "ContainerMode [LCL]->[FCL]");
			}
			finally
			{
				DisposeJobs(quotedBooking);
			}
		}

		public void TestUpdateCO2eStatusToNotCurrent_WhenContainerModeIsChangedInBooking()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);

			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_TransportMode = Constants.TransportModes.Sea;
			var quotedBooking = CreateNewQuotedBooking(quote.PK, booking.PK);

			try
			{
				quotedBooking.TransportMode = Constants.TransportModes.Sea;
				quotedBooking.ContainerMode = Constants.ContainerModes.LCL;
				var cont = quotedBooking.QuotedBookingContainers.AddNew();
				var refCont = NewRefContainer("20GP111", "22G0", 1m, 2280m);
				cont.JC_RC = refCont.PK;
				cont.JC_ContainerCount = 1;
				quotedBooking.SetCO2eStatus(CO2eStatusList.Codes.Current);
				AssertEquals(CO2eStatusList.Codes.Current, quotedBooking.GetCO2eStatus());
				Factory.Save();

				quotedBooking.ContainerMode = Constants.ContainerModes.FCL;
				AssertEquals(CO2eStatusList.Codes.NotCurrent, quotedBooking.GetCO2eStatus());
				CO2eTestHelper.AssertSTUEvent(quotedBooking, "ContainerMode [LCL]->[FCL]");
			}
			finally
			{
				DisposeJobs(quotedBooking);
			}
		}

		[TestDate(2024, 1, 1)]
		public void TestUpdateCO2eStatusToNotCurrent_WhenRequireTEUAndContainerChange_OOQ()
		{
			// Arrange
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var ooq = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
			Factory.Save();
			ooq.Mode = RateMode.FCL;
			ooq.Weight = 100;
			ooq.WeightUnit = Weight.Kilograms;
			var containers = ooq.Quote.CurrentOneOffQuote.Containers;
			ooq.TransportMode = TransportModes.Sea;
			ooq.ContainerMode = RateMode.FCL;
			ooq.Origin = "AUSYD";
			ooq.Destination = "NZAKL";

			containers.RemoveAll();
			var container1 = containers.AddNew();
			container1.TC_ContainerCount = 2;
			var refContainer1 = NewRefContainer("20GP111", "22G0", 1m, 2280m);
			container1.TC_RC = refContainer1.PK;

			var container2 = containers.AddNew();
			container2.TC_ContainerCount = 1;
			var refContainer2 = NewRefContainer("40REHC111", "45R0", 2.3m, 4420m);
			container2.TC_RC = refContainer2.PK;

			ooq.SetCO2eStatus(CO2eStatusList.Codes.Current);
			Factory.Save();

			// Act & Assert
			AssertCO2eStatus(() => container2.TC_ContainerCount = 2, "TC_ContainerCount [1]->[2]");

			AssertCO2eStatus(() => container1.TC_RC = refContainer2.PK, $"TC_RC [{refContainer1.PK}]->[{refContainer2.PK}]");

			AssertCO2eStatus(() => containers.Remove(container1), "One Off Quote container removed");

			AssertCO2eStatus(() => containers.Remove(container2), "One Off Quote container removed");

			AssertCO2eStatus(() => containers.Add(container1), "One Off Quote container added");

			void AssertCO2eStatus(Action action, string stuReason)
			{
				TestDateAttribute.AddMinutes(1);
				ooq.SetCO2eStatus(CO2eStatusList.Codes.Current);
				action.Invoke();
				AssertEquals(CO2eStatusList.Codes.NotCurrent, ooq.GetCO2eStatus());
				CO2eTestHelper.AssertSTUEvent(ooq, stuReason);
			}
		}

		RefContainer NewRefContainer(ZString code, ZString isoType, decimal teu, decimal tareWeight)
		{
			var refContainer = RefContainer.New(Factory);
			refContainer.RC_Code = code;
			refContainer.RC_ISOType = isoType;
			refContainer.RC_TEU = teu;
			refContainer.RC_TareWeight = tareWeight;

			return refContainer;
		}

		public void TestContainerCountsAndTEUCount()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = CreateNewQuotedBooking(quote.PK, booking.PK);
			QuotedBookingContainerDependentCollection quotedBookingContainers = quotedBooking.QuotedBookingContainers;
			ForwardingContainer container1 = quotedBookingContainers.AddNew();
			ForwardingContainer container2 = quotedBookingContainers.AddNew();
			ForwardingContainer container3 = quotedBookingContainers.AddNew();
			ForwardingContainer container4 = quotedBookingContainers.AddNew();
			ForwardingContainer container5 = quotedBookingContainers.AddNew();
			ForwardingContainer container6 = quotedBookingContainers.AddNew();
			container1.JC_ContainerCount = 2;
			container1.JC_RC = (Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP")).PK;
			container2.JC_ContainerCount = 3;
			container2.JC_RC = (Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP")).PK;
			container3.JC_ContainerCount = 4;
			container3.JC_RC = (Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP")).PK;
			container4.JC_ContainerCount = 5;
			container4.JC_RC = (Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE")).PK;
			container5.JC_ContainerCount = 6;
			container5.JC_RC = (Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE")).PK;
			container6.JC_ContainerCount = 7;
			container6.JC_RC = (Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "DUMP1")).PK;

			AssertEquals(new ZInt(6), quotedBooking.ContainerCount20GP);
			AssertEquals(new ZInt(3), quotedBooking.ContainerCount40GP);
			AssertEquals(new ZInt(5), quotedBooking.ContainerCount20RE);
			AssertEquals(new ZInt(6), quotedBooking.ContainerCount40RE);
			AssertEquals(new ZInt(7), quotedBooking.ContainerCountOther);
			AssertEquals(new ZDecimal(29.00), quotedBooking.TEUCount);
		}

		public void TestContainerCounts_WhenQuoteIsNull()
		{
			var quotedBooking = new QuotedBooking(Factory);

			AssertNull("quote", quotedBooking.Quote);
			AssertEquals("ContainerCount20GP", new ZInt(0), quotedBooking.ContainerCount20GP);
			AssertEquals("ContainerCount40GP", new ZInt(0), quotedBooking.ContainerCount40GP);
			AssertEquals("ContainerCount20RE", new ZInt(0), quotedBooking.ContainerCount20RE);
			AssertEquals("ContainerCount40RE", new ZInt(0), quotedBooking.ContainerCount40RE);
			AssertEquals("ContainerCountOther", new ZInt(0), quotedBooking.ContainerCountOther);
		}

		public void TestContainerCounts_WhenQuoteCurrentOneOffIsNull()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			var quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);

			quote.TH_OneTimeQuote = false;
			quote.OneOffQuote.RemoveAll();

			AssertNull("quote.CurrentOneOffQuote", quotedBooking.Quote.CurrentOneOffQuote);
			AssertEquals("ContainerCount20GP", new ZInt(0), quotedBooking.ContainerCount20GP);
			AssertEquals("ContainerCount40GP", new ZInt(0), quotedBooking.ContainerCount40GP);
			AssertEquals("ContainerCount20RE", new ZInt(0), quotedBooking.ContainerCount20RE);
			AssertEquals("ContainerCount40RE", new ZInt(0), quotedBooking.ContainerCount40RE);
			AssertEquals("ContainerCountOther", new ZInt(0), quotedBooking.ContainerCountOther);
		}

		public void TestContainerCount()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(quote.PK, booking.PK);
			AssertEquals(0, quotedBooking.ContainerCount);

			var quotedBookingContainers = quotedBooking.QuotedBookingContainers;
			quotedBookingContainers.AddNew();
			quotedBookingContainers.AddNew();

			AssertEquals(2, quotedBooking.ContainerCount);
		}

		public void TestContainerCountCanHandleNullContainer()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
			var cont1 = quote.CurrentOneOffQuote.Containers.AddNew();
			cont1.TC_RC = ZGuid.Empty;
			cont1.TC_ContainerCount = 1;
			AssertNoExceptionThrown(() => _ = quotedBooking.ContainerCount20GP);
			AssertNoExceptionThrown(() => _ = quotedBooking.ContainerCount40GP);
			AssertNoExceptionThrown(() => _ = quotedBooking.ContainerCount20RE);
			AssertNoExceptionThrown(() => _ = quotedBooking.ContainerCount40RE);
			AssertNoExceptionThrown(() => _ = quotedBooking.ContainerCountOther);
		}

		public void TestTEUCountFromQuote()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			QuotedBooking quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);

			RateOneOffContainers cont1 = quote.CurrentOneOffQuote.Containers.AddNew();
			RateOneOffContainers cont2 = quote.CurrentOneOffQuote.Containers.AddNew();
			RefContainer ref1 = Factory.New<RefContainer>();
			RefContainer ref2 = Factory.New<RefContainer>();

			cont1.TC_RC = ref1.PK;
			cont2.TC_RC = ref2.PK;
			cont1.TC_ContainerCount = 1;
			cont2.TC_ContainerCount = 2;
			cont1.Container.RC_TEU = 3.00;
			cont2.Container.RC_TEU = 8.00;

			AssertEquals((ZDecimal)19.00, quotedBooking.TEUCount);
		}

		public void TestPickupReadyDeliveryOpenDontSetEstimatedDates()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = CreateNewQuotedBooking(quote.PK, booking.PK);

			quotedBooking.DeliveryOpen = ZDateTime.BrettsBirthday;
			quotedBooking.PickupReady = ZDateTime.BrettsBirthday;
			AssertEquals(ZDateTime.Empty, quotedBooking.Booking.JS_E_ARV);
			AssertEquals(ZDateTime.Empty, quotedBooking.Booking.JS_E_DEP);
		}

		public void TestDeliveryDueDateVisibility()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);

			Env.Security.QuickBookingDeliveryDueDateOverride.IsAllowed = true;
			AssertEquals("Allow override of Delivery Due Date set to true, shouldn't be readonly", false, quotedBooking.DeliveryDueDateInfo.ReadOnly);

			Env.Security.QuickBookingDeliveryDueDateOverride.IsAllowed = false;
			AssertEquals("Allow override of Delivery Due Date set to false, should be readonly", true, quotedBooking.DeliveryDueDateInfo.ReadOnly);
		}

		public void TestEstimatedDeliveryDueDateVisibility()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);

			Env.Security.QuickBookingEstimatedDeliveryDueDateOverride.IsAllowed = true;
			AssertEquals("Allow override of Estimated Delivery Due Date set to true, shouldn't be readonly", false, quotedBooking.DeliveryOpenInfo.ReadOnly);

			Env.Security.QuickBookingEstimatedDeliveryDueDateOverride.IsAllowed = false;
			AssertEquals("Allow override of Estimated Delivery Due Date set to false, should be readonly", true, quotedBooking.DeliveryOpenInfo.ReadOnly);
		}

		public void TestJS_DeliveryDueDate_AutomaticallyRecalculated_OnPickUpRequiredByChanges()
		{
			var newDeliveryDueDate = ZDateTime.Today;
			var deliveryDueDateCalculatorManagerMock = DeliveryDueDateCalculationTestHelper.SetupDeliveryDueDateCalculatorManagerMockWithoutSubstitution(newDeliveryDueDate);
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				Env.Security.MaintainShipmentDeliveryDueDateOverride.IsAllowed = true;
				var shipment = QuotedBooking.CreateNewBooking(Factory);
				var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);
				quotedBooking.TransportMode = Core.Constants.TransportModes.Air;

				var consignor1 = Factory.NewWithValidTestData<OrgHeader>();
				var consignee1 = Factory.NewWithValidTestData<OrgHeader>();
				var pickupCFS1 = Factory.NewWithValidTestData<OrgHeader>();
				var deliveryCFS1 = Factory.NewWithValidTestData<OrgHeader>();

				using (ObjectFactory.Substitute(deliveryDueDateCalculatorManagerMock.Object))
				{
					shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor1.PK;
					shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee1.PK;
					shipment.JS_OA_ExportReceivingDepot = pickupCFS1.MainAddress.PK;
					shipment.JS_OA_ImportReleaseDepot = deliveryCFS1.MainAddress.PK;
					shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR;
					shipment.JS_RS_NKServiceLevel = "STD";
					shipment.DocsAndCartage.JP_PickupRequiredBy = ZDateTime.Now;
				}

				deliveryDueDateCalculatorManagerMock.Verify(manager => manager.Calculate(It.IsAny<ForwardingShipment>()), Times.Once, "Automatic Delivery Due Date Calculation is triggered, when all factors are provided");
				AssertEquals("Pre-condition: Delivery Due Date is auto-calculated", newDeliveryDueDate, quotedBooking.DeliveryDueDate);

				Factory.Save();

				var oldDeliveryDueDate = quotedBooking.DeliveryDueDate;
				newDeliveryDueDate = ZDateTime.Today.AddDays(5);
				deliveryDueDateCalculatorManagerMock = DeliveryDueDateCalculationTestHelper.SetupDeliveryDueDateCalculatorManagerMockWithoutSubstitution(newDeliveryDueDate);
				using (ObjectFactory.Substitute(deliveryDueDateCalculatorManagerMock.Object))
				{
					quotedBooking.PickupClose = ZDateTime.Now.AddDays(1);
					Factory.Save();

					deliveryDueDateCalculatorManagerMock.Verify(manager => manager.Calculate(It.IsAny<ForwardingShipment>()), Times.Once, "Factor change triggered Delivery Due Date recalculation");
					CombineAssertions("Pickup Required By changed", () =>
					{
						AssertEquals("Delivery Due Date is recalculated", newDeliveryDueDate, quotedBooking.DeliveryDueDate);
						var ddeEvent = quotedBooking.Logs.MostRecentLogByEventTime(Events.DeliveryDateUpdated);
						AssertEquals("DDE is logged", $"|ACT=FactorChanged|NEW={newDeliveryDueDate.ToLongTimeString()}|OLD={oldDeliveryDueDate.ToLongTimeString()}|RES=Pickup Required By changed|TYP=Original", ddeEvent.SL_Reference);
					});
				}
			}
		}

		public void TestHBLAWBChargesDisplay()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);

			DocumentsDataRegistry.Instance.HBLChargesDefaultDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DocumentsDataRegistry.HBLChargesDisplayTypes.CollectCharges);
			AssertEquals("Charges Apply should default from HBLChargesDefaultDisplay registry", DocumentsDataRegistry.HBLChargesDisplayTypes.CollectCharges, quotedBooking.HBLAWBChargesDisplay);

			shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.AsAgreed;
			AssertEquals("Charges Apply should come from shipment", DocumentsDataRegistry.HBLChargesDisplayTypes.AsAgreed, quotedBooking.HBLAWBChargesDisplay);

			shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithPrepaidAndCollectCharges;
			AssertEquals("Charges Apply should come from shipment", DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithPrepaidAndCollectCharges, quotedBooking.HBLAWBChargesDisplay);
		}

		#region IsAviationSecurityApplicableForTransportMode

		public void TestIsAviationSecurityApplicableForTransportMode()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);

			shipment.JS_TransportMode = "AIR";
			Assert("AIR", quotedBooking.IsAviationSecurityApplicableForTransportMode);

			shipment.JS_TransportMode = "COU";
			Assert("COU", quotedBooking.IsAviationSecurityApplicableForTransportMode);

			shipment.JS_TransportMode = "SEA";
			Assert("SEA", !quotedBooking.IsAviationSecurityApplicableForTransportMode);
		}

		#endregion

		#region TestContainerInfo_ReadOnly

		public void TestContainerInfo_ReadOnly()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);
			AssertContainersReadOnlyBasedOnModes(quotedBooking);
		}

		public void TestContainerInfo_ReadOnly_WhenIsCFSRegistered()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);
			booking.JS_IsCFSRegistered = true;
			AssertContainersReadOnlyBasedOnModes(quotedBooking);
		}

		void AssertContainersReadOnlyBasedOnModes(QuotedBooking quotedBooking)
		{
			quotedBooking.Mode = Core.Constants.ContainerModes.LCL;
			AssertEquals("Container Grid should be readonly", true, quotedBooking.QuotedBookingContainers.ReadOnly);

			quotedBooking.Mode = Core.Constants.ContainerModes.FCL;
			AssertEquals("Container Grid should NOT be readonly.", false, quotedBooking.QuotedBookingContainers.ReadOnly);

			quotedBooking.Mode = Core.Constants.ContainerModes.ULD;
			AssertEquals("Container Grid should NOT be readonly.", false, quotedBooking.QuotedBookingContainers.ReadOnly);

			quotedBooking.Mode = Core.Constants.ContainerModes.Loose;
			AssertEquals("Container Grid should be readonly.", true, quotedBooking.QuotedBookingContainers.ReadOnly);

			quotedBooking.Mode = Core.Constants.ContainerModes.FTL;
			AssertEquals("Container Grid should NOT be readonly.", false, quotedBooking.QuotedBookingContainers.ReadOnly);
		}

		public void TestBookingContainerAndLooseCargoInfo_ReadOnly()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);
			// Old modes
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Air, ContainerModes.Loose, isContainerReadOnly: true);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Air, ContainerModes.ULD, isContainerReadOnly: false);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Sea, ContainerModes.FCL, isContainerReadOnly: false);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Sea, ContainerModes.LCL, isContainerReadOnly: true);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Road, ContainerModes.LCL, isContainerReadOnly: true);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Road, ContainerModes.FCL, isContainerReadOnly: false);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Road, ContainerModes.FTL, isContainerReadOnly: false);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Road, ContainerModes.LTL, isContainerReadOnly: true);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Rail, ContainerModes.LCL, isContainerReadOnly: true);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Rail, ContainerModes.FCL, isContainerReadOnly: false);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Courier, ContainerModes.OnBoardCourier, isContainerReadOnly: true);
			// New adding modes
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Sea, ContainerModes.BuyersConsol, isContainerReadOnly: false);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Sea, ContainerModes.ShippersConsol, isContainerReadOnly: false);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Air, ContainerModes.BuyersConsol, isContainerReadOnly: false);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Air, ContainerModes.ShippersConsol, isContainerReadOnly: false);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Road, ContainerModes.BuyersConsol, isContainerReadOnly: false);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Road, ContainerModes.ShippersConsol, isContainerReadOnly: false);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Rail, ContainerModes.BuyersConsol, isContainerReadOnly: false);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Rail, ContainerModes.ShippersConsol, isContainerReadOnly: false);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.AirSea, ContainerModes.ULD, isContainerReadOnly: false);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.SeaAir, ContainerModes.ULD, isContainerReadOnly: false);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Sea, ContainerModes.Bulk, isContainerReadOnly: true);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Sea, ContainerModes.Liquid, isContainerReadOnly: true);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Sea, ContainerModes.BreakBulk, isContainerReadOnly: true);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Sea, ContainerModes.RollOnRollOff, isContainerReadOnly: true);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Rail, ContainerModes.Bulk, isContainerReadOnly: true);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Rail, ContainerModes.Liquid, isContainerReadOnly: true);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Rail, ContainerModes.BreakBulk, isContainerReadOnly: true);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Courier, ContainerModes.Unaccompanied, isContainerReadOnly: true);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.AirSea, ContainerModes.Loose, isContainerReadOnly: true);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.AirSea, ContainerModes.LCL, isContainerReadOnly: true);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.SeaAir, ContainerModes.Loose, isContainerReadOnly: true);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.SeaAir, ContainerModes.LCL, isContainerReadOnly: true);
		}
		public void TestQuoteContainerAndLooseCargoInfo_ReadOnly()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
			// Old modes
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Air, ContainerModes.Loose, isContainerReadOnly: true);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Air, ContainerModes.ULD, isContainerReadOnly: false);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Sea, RateMode.SEA, isContainerReadOnly: false);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Sea, ContainerModes.FCL, isContainerReadOnly: false, isLooseCargoReadOnly: true);//l
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Sea, ContainerModes.LCL, isContainerReadOnly: true);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Road, RateMode.ROA, isContainerReadOnly: false);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Road, RateMode.LRO, isContainerReadOnly: false);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Road, ContainerModes.LCL, isContainerReadOnly: false);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Road, ContainerModes.FCL, isContainerReadOnly: false);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Road, ContainerModes.FTL, isContainerReadOnly: false);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Road, ContainerModes.LTL, isContainerReadOnly: false);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Rail, RateMode.RAI, isContainerReadOnly: false);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Rail, RateMode.FWL, isContainerReadOnly: false);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Rail, ContainerModes.LCL, isContainerReadOnly: false);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Rail, ContainerModes.FCL, isContainerReadOnly: false);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Courier, RateMode.COU, isContainerReadOnly: false);
			// New adding modes
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Sea, ContainerModes.BuyersConsol, isContainerReadOnly: false);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Sea, ContainerModes.ShippersConsol, isContainerReadOnly: false);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Air, ContainerModes.BuyersConsol, isContainerReadOnly: false);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Air, ContainerModes.ShippersConsol, isContainerReadOnly: false);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Road, ContainerModes.BuyersConsol, isContainerReadOnly: false);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Road, ContainerModes.ShippersConsol, isContainerReadOnly: false);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Rail, ContainerModes.BuyersConsol, isContainerReadOnly: false);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Rail, ContainerModes.ShippersConsol, isContainerReadOnly: false);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.AirSea, ContainerModes.ULD, isContainerReadOnly: false);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.SeaAir, ContainerModes.ULD, isContainerReadOnly: false);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Sea, ContainerModes.Bulk, isContainerReadOnly: true);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Sea, ContainerModes.Liquid, isContainerReadOnly: true);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Sea, ContainerModes.BreakBulk, isContainerReadOnly: true);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Sea, ContainerModes.RollOnRollOff, isContainerReadOnly: true);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Rail, ContainerModes.Bulk, isContainerReadOnly: true);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Rail, ContainerModes.Liquid, isContainerReadOnly: true);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Rail, ContainerModes.BreakBulk, isContainerReadOnly: true);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Courier, ContainerModes.OnBoardCourier, isContainerReadOnly: true);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.Courier, ContainerModes.Unaccompanied, isContainerReadOnly: true);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.AirSea, ContainerModes.Loose, isContainerReadOnly: true);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.AirSea, ContainerModes.LCL, isContainerReadOnly: true);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.SeaAir, ContainerModes.Loose, isContainerReadOnly: true);
			AssertContainerAndLooseCargoReadOnlyBasedOnModes(quotedBooking, TransportModes.SeaAir, ContainerModes.LCL, isContainerReadOnly: true);
		}

		void AssertContainerAndLooseCargoReadOnlyBasedOnModes(QuotedBooking quotedBooking, string transportMode, string containerMode, bool isContainerReadOnly, bool isLooseCargoReadOnly = false)
		{
			quotedBooking.TransportMode = transportMode;
			quotedBooking.ContainerMode = containerMode;
			if (quotedBooking.Booking != null)
			{
				AssertEquals($"Container Grid should {(isContainerReadOnly ? "" : "NOT ")}be readonly.", isContainerReadOnly, quotedBooking.QuotedBookingContainers.ReadOnly);
				AssertEquals($"Loose Cargo Grid should not be readonly.", isLooseCargoReadOnly, quotedBooking.Booking.OuterPackLines.ReadOnly);
			}
			if (quotedBooking.Quote != null)
			{
				AssertEquals($"Container Grid should {(isContainerReadOnly ? "" : "NOT ")}be readonly.", isContainerReadOnly, quotedBooking.Quote.CurrentOneOffQuote.Containers.ReadOnly);
				AssertEquals($"Loose Cargo Grid should not be readonly.", isLooseCargoReadOnly, quotedBooking.Quote.CurrentOneOffQuote.LooseCargo.ReadOnly);
			}
		}

		#endregion

		#region GetWorkflowInformationProvider

		public void TestGetWorkflowInformationProvider()
		{
			var acceptedQuote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var notAcceptedQuote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var company = Factory.New<GlbCompany>();
			var branch = Factory.New<GlbBranch>();

			branch.GB_GC = company.PK;
			acceptedQuote.TH_GC = company.PK;
			notAcceptedQuote.TH_GC = company.PK;

			AssertWorkflowInformationProvider(ZGuid.Empty, booking.PK, QuotedBookingState.BookingOnly, new[] { GlbCompany.CurrentCompany.PK });
			AssertWorkflowInformationProvider(acceptedQuote.PK, ZGuid.Empty, QuotedBookingState.QuoteOnly, new[] { company.PK });
			AssertWorkflowInformationProvider(acceptedQuote.PK, booking.PK, QuotedBookingState.AcceptedBookingWithQuote, new[] { company.PK });
			AssertWorkflowInformationProvider(notAcceptedQuote.PK, booking.PK, QuotedBookingState.UnacceptedBookingWithQuote, new[] { company.PK });
		}

		void AssertWorkflowInformationProvider(ZGuid quotePK, ZGuid bookingPK, QuotedBookingState expectedState, ZGuid[] expectedCompanies)
		{
			var quotedBooking = CreateNewQuotedBooking(quotePK, bookingPK);
			quotedBooking.Origin = "UAIEV";
			quotedBooking.Destination = "AUSYD";

			var workflowInformationProvider = (quotedBooking as IWorkflowProvider).GetWorkflowInformationProvider();

			AssertEquals("Origin", "Kiev", workflowInformationProvider.Origin);
			AssertEquals("Destination", "Sydney", workflowInformationProvider.Destination);
			AssertEquals("Business Context", TrackingConstants.BusinessContext.Booking, workflowInformationProvider.BusinessContext);
			AssertEquals("ObjectState", expectedState, quotedBooking.ObjectState);
			AssertContainsExactElementsInAnyOrder("Companies", expectedCompanies, workflowInformationProvider.Companies);
		}

		#endregion

		#region ICustomFieldProvider

		#region Custom Fields Test

		[TestedType(typeof(QuotedBooking))]
		class CustomFieldsTest : TestICustomFieldProvider
		{
			protected override BusinessObject GetBizo()
			{
				var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
				return QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			}
		}

		#endregion

		public void TestUserDefinedValuesSupport()
		{
			Assert("QuotedBooking must be decorated with UserDefinedValuesAttribute in order to support custom fields",
					typeof(QuotedBooking).GetCustomAttributes(typeof(UserDefinedValuesAttribute), true).Length > 0);

			Assert("QuotedBooking must be implement ICustomFieldProvider in order to support custom fields",
					typeof(QuotedBooking).GetInterfaces().Contains(typeof(ICustomFieldProvider)));
		}

		public void TestGetCustomBusinessObject()
		{
			CreateQuotedBookingWorkflowWithCustomFields();

			QuotedBooking quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, QuotedBooking.CreateNewBooking(Factory).PK);
			quotedBooking.Mode = "LCL";
			quotedBooking.Origin = "AUSYD";

			IDynamicBusinessObject dynamicBusinessObject = ((ICustomFieldProvider)quotedBooking).GetCustomBusinessObject();

			AssertContainsExactElementsInAnyOrder("expected workflow property aliases + infos",
					new[] { "__CUSTOM1__prop__ZString", "__CUSTOM1__prop__ZStringInfo", "__CUSTOM2__prop__ZInt", "__CUSTOM2__prop__ZIntInfo" }, dynamicBusinessObject.PropertyNames);

			quotedBooking.Mode = "LSE";
			quotedBooking.Origin = "PLWRO";

			Factory.Save();

			quotedBooking = new BusinessObjectFactory().Load<QuotedBooking>(quotedBooking.PK);
			dynamicBusinessObject = ((ICustomFieldProvider)quotedBooking).GetCustomBusinessObject();
			AssertEquals("different workflow match", 0, dynamicBusinessObject.PropertyNames.Length);
		}

		public void TestGetCustomBusinessObjectWithIsForwardRegistered()
		{
			var quotedBooking = GetNewBusinessObject() as QuotedBooking;
			var dynamicBusinessObject = ((ICustomFieldProvider)quotedBooking).GetCustomBusinessObject();

			AssertEquals("JS_IsForwardRegistered should be false", false, quotedBooking.Booking.JS_IsForwardRegistered);
			AssertEquals(quotedBooking.Quote.PK, dynamicBusinessObject.Parent.PK);

			quotedBooking.Booking.JS_IsForwardRegistered = true;
			dynamicBusinessObject = ((ICustomFieldProvider)quotedBooking).GetCustomBusinessObject();
			AssertEquals("JS_IsForwardRegistered should be true", true, quotedBooking.Booking.JS_IsForwardRegistered);
			AssertEquals(quotedBooking.Quote.PK, dynamicBusinessObject.Parent.PK);
		}

		public void TestGetCustomBusinessObject_CustomFieldsMovedForOldConvertedBookings()
		{
			CreateQuotedBookingWorkflowWithCustomFields("BWQ");

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(quote.PK, booking.PK);
			quotedBooking.Booking.JS_IsForwardRegistered = true;
			quotedBooking.Mode = "LCL";
			quotedBooking.Origin = "AUSYD";

			AddCustomFieldValue(booking.PK, JobShipmentSchema.Constants.Prefix, "custom1", AddOnColumnDataType.Codes.String, "AAA");
			AddCustomFieldValue(booking.PK, JobShipmentSchema.Constants.Prefix, "custom2", AddOnColumnDataType.Codes.Integer, "111");

			var dynamicBusinessObject = ((ICustomFieldProvider)quotedBooking).GetCustomBusinessObject();
			AssertEquals(booking.PK, dynamicBusinessObject.Parent.PK);

			var customFieldProvider = quotedBooking as ICustomFieldProvider;
			AssertEquals("AAA", customFieldProvider.GetCustomField("custom1", AddOnColumnDataType.Codes.String));
			AssertEquals(111, customFieldProvider.GetCustomField("custom2", AddOnColumnDataType.Codes.Integer));

			AddCustomFieldValue(quotedBooking.PK, ViewQuotedBookingSchema.Constants.Prefix, "custom1", AddOnColumnDataType.Codes.String, "BBB");
			AddCustomFieldValue(quotedBooking.PK, ViewQuotedBookingSchema.Constants.Prefix, "custom2", AddOnColumnDataType.Codes.Integer, "222");

			Factory.Save();

			quotedBooking = new BusinessObjectFactory().Load<QuotedBooking>(quotedBooking.PK);
			dynamicBusinessObject = ((ICustomFieldProvider)quotedBooking).GetCustomBusinessObject();
			AssertEquals(quotedBooking.PK, dynamicBusinessObject.Parent.PK);

			customFieldProvider = quotedBooking;
			AssertEquals("BBB", customFieldProvider.GetCustomField("custom1", AddOnColumnDataType.Codes.String));
			AssertEquals(222, customFieldProvider.GetCustomField("custom2", AddOnColumnDataType.Codes.Integer));
		}

		public void TestGetCustomField()
		{
			CreateQuotedBookingWorkflowWithCustomFields();

			QuotedBooking quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, QuotedBooking.CreateNewBooking(Factory).PK);
			quotedBooking.Mode = "LCL";
			quotedBooking.Origin = "AUSYD";

			ICustomFieldProvider customFieldProvider = quotedBooking;

			AssertEquals(ZString.Empty, customFieldProvider.GetCustomField("custom1", AddOnColumnDataType.Codes.String));
			AssertEquals(ZInt.Zero, customFieldProvider.GetCustomField("custom2", AddOnColumnDataType.Codes.Integer));

			AddCustomFieldValue(quotedBooking.PK, ViewQuotedBookingSchema.Constants.Prefix, "custom1", AddOnColumnDataType.Codes.String, "AAA");
			AddCustomFieldValue(quotedBooking.PK, ViewQuotedBookingSchema.Constants.Prefix, "custom2", AddOnColumnDataType.Codes.Integer, "111");

			AssertEquals("AAA", customFieldProvider.GetCustomField("custom1", AddOnColumnDataType.Codes.String));
			AssertEquals(111, customFieldProvider.GetCustomField("custom2", AddOnColumnDataType.Codes.Integer));
		}

		public void TestCustomFieldsQuoteConvertedToBookingWithQuote()
		{
			CreateQuotedBookingWorkflowWithCustomFields();

			QuotedBooking quotedBooking = CreateNewQuotedBooking(QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted).PK, ZGuid.Empty);
			quotedBooking.Mode = "LCL";
			quotedBooking.Origin = "AUSYD";

			try
			{
				AddCustomFieldValue(quotedBooking.PK, ViewQuotedBookingSchema.Constants.Prefix, "custom1", AddOnColumnDataType.Codes.String, "BBB");
				AddCustomFieldValue(quotedBooking.PK, ViewQuotedBookingSchema.Constants.Prefix, "custom2", AddOnColumnDataType.Codes.Integer, "222");
				quotedBooking.Quote.CurrentOneOffQuote.TT_QuoteApprovedByManager = true;

				ICustomFieldProvider customFieldProvider = quotedBooking;
				AssertEquals("BBB", customFieldProvider.GetCustomField("custom1", AddOnColumnDataType.Codes.String));
				AssertEquals(222, customFieldProvider.GetCustomField("custom2", AddOnColumnDataType.Codes.Integer));

				quotedBooking.Factory.Save();
				quotedBooking.ConvertQuoteToQuotedBooking();

				AssertNotNull("prerequisite", quotedBooking.Booking);
				AssertEquals("BBB", customFieldProvider.GetCustomField("custom1", AddOnColumnDataType.Codes.String));
				AssertEquals(222, customFieldProvider.GetCustomField("custom2", AddOnColumnDataType.Codes.Integer));
			}
			finally
			{
				DisposeJobs(quotedBooking);
			}
		}

		GenCustomAddOnValue AddCustomFieldValue(ZGuid parentId, ZString parentTableCode, ZString customValueName, ZString customValueType, ZString value)
		{
			GenCustomAddOnValue customAddOnValue = Factory.New<GenCustomAddOnValue>();
			customAddOnValue.XV_ParentID = parentId;
			customAddOnValue.XV_ParentTableCode = parentTableCode;
			customAddOnValue.XV_Name = customValueName;
			customAddOnValue.XV_Type = customValueType;
			customAddOnValue.XV_Data = value;

			return customAddOnValue;
		}

		void CreateQuotedBookingWorkflowWithCustomFields(string bookingType = "QBN")
		{
			ProcessTaskTemplate processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = "QBK";
			processTaskTemplate.P0_SubType1 = "SEA";
			processTaskTemplate.P0_SubType2 = bookingType;
			processTaskTemplate.P0_LoadPortCountry = "AU";

			GenCustomColumnDefinition customField1 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField1.XC_Name = "custom1";
			customField1.XC_Type = AddOnColumnDataType.Codes.String;

			GenCustomColumnDefinition customField2 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField2.XC_Name = "custom2";
			customField2.XC_Type = AddOnColumnDataType.Codes.Integer;

			Factory.Save();
		}

		#endregion

		#region Universal Copy extensions

		public void TestCreateNewQuoteForUniversalCopy()
		{
			var quotedBooking = new QuotedBooking(Factory);
			AssertNull(quotedBooking.Quote);

			quotedBooking.CreateNewQuoteForUniversalCopy();
			AssertNotNull(quotedBooking.Quote);
		}

		public void TestCreateNewBookingForUniversalCopy()
		{
			var quotedBooking = new QuotedBooking(Factory);
			AssertNull(quotedBooking.Booking);

			quotedBooking.CreateNewBookingForUniversalCopy();
			AssertNotNull(quotedBooking.Booking);
		}

		public void TestFinishInitializationAfterUniversalCopy()
		{
			var quotedBooking = new QuotedBooking(Factory);
			quotedBooking.CreateNewQuoteForUniversalCopy();
			quotedBooking.CreateNewBookingForUniversalCopy();

			Assert(!quotedBooking.IsRegisteredEditableChildObject(quotedBooking.Booking));

			quotedBooking.FinishInitializationAfterUniversalCopy();

			Assert(quotedBooking.IsRegisteredEditableChildObject(quotedBooking.Booking));
		}

		public void TestLoadOrCreateJobDesnotDeleteJobChargesAfterUC()
		{
			var quotedBooking = new QuotedBooking(Factory);
			quotedBooking.CreateNewBookingForUniversalCopy();
			Assert("Precondition", quotedBooking.UseJobFromBooking);
			Assert("UC flag is clear by default", !quotedBooking.wasCreatedByUniversalCopy);

			quotedBooking.TryLoadOrCreateJob();
			using (var job = quotedBooking.Job)
			{
				var chargesQuery = new ZQuery(JobChargeSchema.JR_JH, job.PK) { FetchOnlyFromLocalCache = true };
				AssertEquals("Precondition", 0, Factory.Load<JobCharge>(chargesQuery).Length);

				Factory.New<JobCharge>().JR_JH = job.PK;
				AssertEquals("1 charge created", 1, Factory.Load<JobCharge>(chargesQuery).Length);

				quotedBooking.TryLoadOrCreateJob();
				var job1 = quotedBooking.Job;
				AssertSame(job, job1);
				AssertEquals("Defaulted charges should be removed by default", 0, Factory.Load<JobCharge>(chargesQuery).Length);

				Factory.New<JobCharge>().JR_JH = job.PK;
				AssertEquals("1 charge created", 1, Factory.Load<JobCharge>(chargesQuery).Length);

				quotedBooking.FinishInitializationAfterUniversalCopy();
				Assert("UC flag is set", quotedBooking.wasCreatedByUniversalCopy);

				quotedBooking.TryLoadOrCreateJob();
				job1 = quotedBooking.Job;
				AssertSame(job, job1);
				AssertEquals("Charges should remain after UC", 1, Factory.Load<JobCharge>(chargesQuery).Length);
			}
		}

		public void TestHasUniversalCopyInstanceType()
		{
			var quote = Factory.New<Quote>();
			quote.TH_OneTimeQuote = true;
			var booking = Factory.New<ForwardingShipment>();
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			var instanceTypeAttribute = typeof(QuotedBooking).GetCustomAttribute<UniversalCopyInstanceTypeAttribute>();
			AssertEquals(typeof(QuotedBooking), instanceTypeAttribute.InstanceType);
			AssertEquals("NewForUniversalCopy", instanceTypeAttribute.CreationMethod);
			AssertEquals("GetSourceForUniversalCopy", instanceTypeAttribute.GetSourceMethod);
			Assert(instanceTypeAttribute.ShouldSyncTreeNodes);
			AssertNotNull(quotedBooking.NewForUniversalCopy(Factory, null));
			AssertNotNull(quotedBooking.GetSourceForUniversalCopy());
		}

		public void TestUniversalCopyExtraMetadata()
		{
			var extraAttributeQuote = typeof(QuotedBooking).GetProperty("Quote")
				.GetCustomAttributes(typeof(UniversalCopyExtraMetadataAttribute), true).OfType<UniversalCopyExtraMetadataAttribute>()
				.FirstOrDefault();
			AssertEquals(-1, extraAttributeQuote.Priority);
			Assert(!extraAttributeQuote.IsMandatory);

			var extraAttributeBooking = typeof(QuotedBooking).GetProperty("Booking")
				.GetCustomAttributes(typeof(UniversalCopyExtraMetadataAttribute), true).OfType<UniversalCopyExtraMetadataAttribute>()
				.FirstOrDefault();
			AssertEquals(-1, extraAttributeBooking.Priority);
			Assert(!extraAttributeBooking.IsMandatory);
		}

		public void TestUniversalCopyDocumentaryOverrides()
		{
			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var override1 = CreateDocumentaryOverride(booking.PK, booking.TablePrefix, "aaa");
			var override2 = CreateDocumentaryOverride(booking.PK, booking.TablePrefix, "bbb");
			Factory.Save();

			var entityNode = new EntityCopyTemplateNode();

			var node1 = new RelatedEntityCopyTemplateNode
			{
				Name = nameof(QuotedBooking.DocumentaryOverrides),
				CopyMethod = RelatedEntityCopyMethod.Link,
				InnerNode = new EntityCopyTemplateNode()
				{
					Name = nameof(QuotedBooking.DocumentaryOverrides)
				}
			};

			var node2 = new RelatedEntityCopyTemplateNode
			{
				Name = nameof(QuotedBooking.Booking),
				CopyMethod = RelatedEntityCopyMethod.Copy,
				InnerNode = new EntityCopyTemplateNode()
				{
					Name = nameof(QuotedBooking.Booking)
				}
			};

			entityNode.Nodes.Add(node1);
			entityNode.Nodes.Add(node2);

			var copyTree = new CopyTemplateTree
			{
				InnerNode = entityNode
			};

			var copyManager = new BusinessObjectCopyManager();

			var copied = (QuotedBooking)copyManager.Copy(booking, copyTree).Object;

			AssertNotNull("target", copied);
			AssertEquals("Was created by Universal Copy", true, copied.wasCreatedByUniversalCopy);
			AssertNotNull("DocumentaryOverrides", copied.DocumentaryOverrides);

			var copiedOverrides = copied
				.DocumentaryOverrides
				.GetOverrides()
				.Select(o => $"{o.JDD_ParentID}|{o.JDD_ParentTableCode}|{o.JDD_Name}|{o.JDD_OverriddenData}")
				.ToArray();

			AssertContainsExactElementsInAnyOrder("",
				new[]
				{
					$"{copied.PK}|VB|aaa|<ZZZ />",
					$"{copied.PK}|VB|bbb|<ZZZ />"
				},
				copiedOverrides);
		}

		public void TestUniversalCopyAttribute()
		{
			var ucElementsOrder = typeof(QuotedBooking)
					.GetCustomAttributes(typeof(UniversalCopyElementsOrderAttribute), true).OfType<UniversalCopyElementsOrderAttribute>();

			AssertEquals(7, ucElementsOrder.Count());
			AssertEquals(1, ucElementsOrder.Count(a => a.FirstElement == "Booking" && a.SecondElement == "CustomFields"));
			AssertEquals(1, ucElementsOrder.Count(a => a.FirstElement == "Quote" && a.SecondElement == "CustomFields"));
			AssertEquals(1, ucElementsOrder.Count(a => a.FirstElement == "ClientPK" && a.SecondElement == "ClientAddrPK"));

			AssertEquals(1, ucElementsOrder.Count(a => a.FirstElement == "OH_Carrier" && a.SecondElement == "CarrierServiceLevel"));
			AssertEquals(1, ucElementsOrder.Count(a => a.FirstElement == "Booking" && a.SecondElement == "OH_Carrier"));
			AssertEquals(1, ucElementsOrder.Count(a => a.FirstElement == "Quote" && a.SecondElement == "OH_Carrier"));
			AssertEquals(1, ucElementsOrder.Count(a => a.FirstElement == "OneOffQuoteContainerMode" && a.SecondElement == "Mode"));
		}

		public void TestUniversalCopyQuotedBookingClientAddress()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = client.Addresses.AddNew();
			address1.OA_Address1 = "Another Address";

			var quotedBooking = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
			quotedBooking.CreateNewQuoteForUniversalCopy();
			quotedBooking.ClientPK = client.PK;
			quotedBooking.ClientAddrPK = address1.PK;

			Factory.Save();

			var entityNode = new EntityCopyTemplateNode();

			var node1 = new PropertyCopyTemplateNode
			{
				Name = nameof(quotedBooking.ClientAddrPK),
				CopyMethod = CopyMethod.Copy
			};

			var node2 = new PropertyCopyTemplateNode
			{
				Name = nameof(quotedBooking.ClientPK),
				CopyMethod = CopyMethod.Copy
			};

			var node3 = new RelatedEntityCopyTemplateNode
			{
				Name = nameof(QuotedBooking.Quote),
				CopyMethod = RelatedEntityCopyMethod.Copy,
				Priority = -1,
				InnerNode = new EntityCopyTemplateNode()
				{
					Name = nameof(Quote.TH_ClientCode)
				}
			};

			entityNode.Nodes.Add(node1);
			entityNode.Nodes.Add(node2);
			entityNode.Nodes.Add(node3);

			var copyTree = new CopyTemplateTree
			{
				InnerNode = entityNode
			};

			var copyManager = new BusinessObjectCopyManager();

			var copied = (QuotedBooking)copyManager.Copy(quotedBooking, copyTree).Object;

			AssertNotNull("target", copied);
			AssertEquals("Was created by Universal Copy", true, copied.wasCreatedByUniversalCopy);
			AssertEquals(client.PK, copied.ClientPK);
			AssertEquals("Should copy source's address", address1.PK, copied.ClientAddr.PK);
		}

		VisualizerDocumentData CreateDocumentaryOverride(ZGuid parentPK, ZString parentTablePrefix, ZString dataStoreName)
		{
			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentID = parentPK;
			documentData.JDD_ParentTableCode = parentTablePrefix;
			documentData.JDD_Name = dataStoreName;
			documentData.JDD_OverriddenData = "<ZZZ />";

			return documentData;
		}

		#endregion

		#region IDtbBookingParent Tests

		public void TestIDtbBookingParent()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			var dtbBookingParent = quotedBooking as IDtbBookingParent;

			quotedBooking.Booking.JS_UniqueConsignRef = "123";
			quotedBooking.Booking.JS_HouseBill = "456789";
			quotedBooking.Booking.JS_GoodsDescription = "Some Books";
			AssertEquals("Booking with Quote", dtbBookingParent.JobTypeDescription);
			AssertEquals("123", dtbBookingParent.JobNumber);
			AssertEquals(WorkflowDescriptors.QuotedBookingWorkflowDescriptorCode, dtbBookingParent.JobType);
			AssertEquals(ControllerIDs.QuotedBookings, dtbBookingParent.ControllerID);

			AssertContainsExactElementsInAnyOrder(new DtbBookingDirection[] { DtbBookingDirection.PIC }, dtbBookingParent.GetSupportedDirections());
		}

		#endregion

		#region IDocManagerSupport Tests

		public void TestIDocManagerInfo()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);

			AssertEquals(typeof(RatingDocManagerInfo), ((IDocManagerSupport)quote).DocManagerInfo.GetType());
			AssertEquals(typeof(ForwardingShipmentDocManagerInfo), ((IDocManagerSupport)booking).DocManagerInfo.GetType());

			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);

			AssertEquals(typeof(QuotedBookingDocManagerInfo), ((IDocManagerSupport)quotedBooking).DocManagerInfo.GetType());
		}

		#endregion

		#region IImportParentRelatedActivityInfoOnNew

		public void TestImportParentRelatedActivityInfoOnNew()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTAA";
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "test@test.com";
			var inquiry = Factory.New<SalesEnquiry>();
			inquiry.O1_OH_ConvertedToQualifiedLead = org.PK;
			inquiry.O1_OC_LinkedContact = contact.PK;

			var mockDeciderFactory = new Mock<IImportRelatedActivityDeciderFactory>();
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			((IImportParentRelatedActivityInfoOnNew)quotedBooking).ImportParentInfo(inquiry, mockDeciderFactory.Object);

			AssertEquals(org.PK, quotedBooking.ClientPK);
			mockDeciderFactory.Verify(x => x.GetIfAvailable<IImportRelatedActivityTradeDetailDecider>(), Times.Never());
		}

		public void TestImportParentRelatedActivityInfoOnNew_SalesAssociatedEntity()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTAA";
			var opportunity = Factory.New<OrgOpportunity>();
			opportunity.P8_OH = org.PK;
			var sales = org.SalesCollection.AddNew();
			var tradeDetail = sales.TradeDetails.AddNew();

			var mockTradeDetailDecider = new Mock<IImportRelatedActivityTradeDetailDecider>();
			mockTradeDetailDecider.Setup(x => x.GetDecision(opportunity)).Returns(new ImportRelatedActivityTradeDetailDecision(false, tradeDetail));
			var mockDeciderFactory = new Mock<IImportRelatedActivityDeciderFactory>();
			mockDeciderFactory.Setup(m => m.GetIfAvailable<IImportRelatedActivityTradeDetailDecider>()).Returns(mockTradeDetailDecider.Object);

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			var importResult = ((IImportParentRelatedActivityInfoOnNew)quotedBooking).ImportParentInfo(opportunity, mockDeciderFactory.Object);
			AssertEquals(true, importResult);

			AssertEquals(org.PK, quotedBooking.ClientPK);

			mockDeciderFactory.VerifyAll();
		}

		public void TestImportParentRelatedActivityInfoOnNew_SalesAssociatedEntity_ImportRelatedActivityTradeDetailCancelled()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTAA";
			var opportunity = Factory.New<OrgOpportunity>();
			opportunity.P8_OH = org.PK;

			var mockTradeDetailDecider = new Mock<IImportRelatedActivityTradeDetailDecider>();
			mockTradeDetailDecider.Setup(x => x.GetDecision(opportunity)).Returns(new ImportRelatedActivityTradeDetailDecision(true, null));
			var mockDeciderFactory = new Mock<IImportRelatedActivityDeciderFactory>();
			mockDeciderFactory.Setup(m => m.GetIfAvailable<IImportRelatedActivityTradeDetailDecider>()).Returns(mockTradeDetailDecider.Object);

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			var importResult = ((IImportParentRelatedActivityInfoOnNew)quotedBooking).ImportParentInfo(opportunity, mockDeciderFactory.Object);
			AssertEquals(false, importResult);

			mockDeciderFactory.VerifyAll();
			mockTradeDetailDecider.VerifyAll();
		}

		#endregion

		#region SuspendImportBrokerRedefaulting

		public void TestSuspendImportBrokerRedefaulting()
		{
			var buyer = Factory.New<OrgHeader>();
			buyer.OH_Code = "AUSBUYER";
			buyer.OH_RL_NKClosestPort = "AUSYD";
			var address1 = buyer.Addresses.AddNew();
			address1.OA_Code = "ConsigneeAddress1";
			address1.OA_Address1 = "ConsigneeAddress1";

			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			var quotedBookingInterface = quotedBooking as IBuyerSupplierRelationshipConsumer;

			quotedBooking.ConsigneeDocumentaryAddress.E2_OA_AddressInfo.ValueChanged += (sender, e) =>
			{
				AssertEquals(true, booking.IsChangingConsigneeAddress);
			};

			Assert(booking.JS_OH_ImportBroker.IsEmpty);
			AssertEquals(false, booking.IsChangingConsigneeAddress);
			quotedBooking.ConsigneeDocumentaryAddress.E2_OA_Address = address1.PK;
			AssertEquals(false, booking.IsChangingConsigneeAddress);
			Assert(booking.JS_OH_ImportBroker.IsEmpty);
		}

		#endregion

		#region ServiceLevel Defaulting

		public void TestConsigneeDefaultsServiceLevel()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.MiscServ.OM_RS_NKIMDefaultServiceLevel = "AAA";

			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			try
			{
				quotedBooking.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
				AssertEquals("AAA", quotedBooking.ServiceLevel);
			}
			finally
			{
				DisposeJobs(quotedBooking);
			}
		}

		public void TestConsignorDefaultsServiceLevel()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.MiscServ.OM_RS_NKEXDefaultServiceLevel = "AAA";

			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			try
			{
				quotedBooking.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
				AssertEquals("AAA", quotedBooking.ServiceLevel);
			}
			finally
			{
				DisposeJobs(quotedBooking);
			}
		}

		public void TestRegistryDefaultsServiceLevel()
		{
			Env.Registry.ServiceLevel = Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "D2D").PK.ToGuid();

			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			try
			{
				quotedBooking.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
				AssertEquals("D2D", quotedBooking.ServiceLevel);
			}
			finally
			{
				DisposeJobs(quotedBooking);
			}
		}

		public void TestConsigneeConsignorRegistryDefaultServiceLevelWithPreference()
		{
			Env.Registry.ServiceLevel = Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "D2D").PK.ToGuid();

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.MiscServ.OM_RS_NKIMDefaultServiceLevel = "AAA";

			var consignor1 = Factory.NewWithValidTestData<OrgHeader>();
			consignor1.MiscServ.OM_RS_NKEXDefaultServiceLevel = "BBB";

			var consignor2 = Factory.NewWithValidTestData<OrgHeader>();

			var link = consignee.SupplierLinks.AddNew(consignor1);
			var trnMode = link.OrgSupBuyLinkTrnModes.AddNew();
			trnMode.PF_TransportMode = Constants.TransportModes.Air;
			trnMode.PF_ContainerMode = Constants.ContainerModes.Loose;
			trnMode.PF_IncoTerm = "CIF";
			trnMode.PF_RL_NKPlaceOfDeliveryPort = "USLAX";
			trnMode.PF_RL_NKPlaceOfReceivalPort = "AUSYD";
			trnMode.PF_RS_NKDefaultServiceLevel = ZString.Empty;

			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			try
			{
				quotedBooking.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
				AssertEquals("AAA", quotedBooking.ServiceLevel);

				consignee.MiscServ.OM_RS_NKIMDefaultServiceLevel = ZString.Empty;
				quotedBooking.ConsignorDocumentaryAddress.OrganisationPK = consignor2.PK;
				AssertEquals("D2D", quotedBooking.ServiceLevel);

				quotedBooking.ConsignorDocumentaryAddress.OrganisationPK = consignor1.PK;
				AssertEquals("BBB", quotedBooking.ServiceLevel);

				consignor1.MiscServ.OM_RS_NKEXDefaultServiceLevel = ZString.Empty;
				quotedBooking.ConsignorDocumentaryAddress.OrganisationPK = consignor2.PK;
				AssertEquals("D2D", quotedBooking.ServiceLevel);
			}
			finally
			{
				DisposeJobs(quotedBooking);
			}
		}

		#endregion

		#region Add Rules to Notes

		public void TestRulesNoteWontBeDeletedByBookingValidation()
		{
			var rule = Factory.NewWithValidTestData<RefCountryRules>();
			rule.R7_RN_NKOrigin = "AU";
			rule.R7_RN_NKDestination = "DE";
			rule.R7_Notes = "\"<Z0_Description>\" == \"asdj\"";
			rule.R7_IsClientVisible = true;
			rule.R7_IsValidationRule = true;
			rule.R7_IsError = true;
			Factory.Save();

			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_GoodsDescription = "asdj";
			AssertEquals(0, booking.Notes.VisibleNotes.Count);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);
			quotedBooking.Origin = "AUBNE";
			quotedBooking.Destination = "DEHAM";
			AssertEquals(0, quotedBooking.Notes.VisibleNotes.Count);

			quotedBooking.RunPreSaveValidation();
			AssertEquals(1, quotedBooking.Notes.VisibleNotes.Count);
		}

		public void TestAddRulesToNotes()
		{
			var rule1 = Factory.NewWithValidTestData<RefCountryRules>();
			rule1.R7_RN_NKOrigin = "AU";
			rule1.R7_RN_NKDestination = "DE";
			rule1.R7_Notes = "This is a client visible Notes";
			rule1.R7_IsClientVisible = ZBool.True;

			var rule2 = Factory.NewWithValidTestData<RefCountryRules>();
			rule2.R7_RN_NKOrigin = "AU";
			rule2.R7_RN_NKDestination = "DE";
			rule2.R7_Notes = "This is an internal Notes";
			rule2.R7_IsClientVisible = ZBool.False;

			Factory.Save();

			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_TransportMode = Constants.TransportModes.Sea;
			booking.JS_PackingMode = Constants.ContainerModes.LCL;

			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);
			quotedBooking.Origin = "AUBNE";
			quotedBooking.Destination = "DEHAM";

			Factory.Save();
			Assert(quotedBooking.Notes.FindByDescription(PredefinedNoteTypes.Instance.CountryRules.Description).FirstOrDefault().ST_NoteText.Contains("This is a client visible Notes"));
			Assert(quotedBooking.Notes.FindByDescription(PredefinedNoteTypes.Instance.CountryRulesInternal.Description).FirstOrDefault().ST_NoteText.Contains("This is an internal Notes"));
		}

		public void TestAddRulesToNotes_GivenLoadNotesBeforeSave_ThenRulesShouldAppearOnNotes()
		{
			var rule = Factory.NewWithValidTestData<RefCountryRules>();
			rule.R7_RN_NKOrigin = "AU";
			rule.R7_RN_NKDestination = "DE";
			rule.R7_Notes = "Rule #1";
			rule.R7_IsClientVisible = ZBool.True;

			Factory.Save();

			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_TransportMode = Constants.TransportModes.Sea;
			booking.JS_PackingMode = Constants.ContainerModes.LCL;

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AAAA";
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			quote.QuotationClientAddress.OrganisationPK = orgHeader.PK;

			var quotedBooking = CreateNewQuotedBooking(quote.PK, booking.PK);
			quotedBooking.Origin = "AUBNE";
			quotedBooking.Destination = "DEHAM";

			AssertContainsExactElementsInAnyOrder
			(
				"Precondition and simulate accessing form.NotesTabPage",
				Array.Empty<string>(),
				quotedBooking.Notes.VisibleNotes.Cast<StmNote>().Select(note => note.ST_NoteText)
			);

			Factory.Save();

			AssertContainsExactElementsInAnyOrder
			(
				"ElementsInternal",
				new string[] { "Australia to Germany:\r\n  Rule #1\r\n" },
				quotedBooking.Notes.GetAllNotes().Cast<StmNote>().Select(note => note.ST_NoteText)
			);

			AssertContainsExactElementsInAnyOrder
			(
				"VisibleNotes",
				new string[] { "Australia to Germany:\r\n  Rule #1\r\n" },
				quotedBooking.Notes.VisibleNotes.Cast<StmNote>().Select(note => note.ST_NoteText)
			);

			quotedBooking.Origin = "DEHAM";
			quotedBooking.Destination = "AUBNE";
			Factory.Save();
			AssertContainsExactElementsInAnyOrder
			(
				"WHEN updating origin and destination and save THEN ElementsInternal should remain",
				new string[] { "Australia to Germany:\r\n  Rule #1\r\n" },
				quotedBooking.Notes.GetAllNotes().Cast<StmNote>().Select(note => note.ST_NoteText)
			);

			AssertContainsExactElementsInAnyOrder
			(
				"WHEN updating origin and destination and save THEN VisibleNotes should remain",
				new string[] { "Australia to Germany:\r\n  Rule #1\r\n" },
				quotedBooking.Notes.VisibleNotes.Cast<StmNote>().Select(note => note.ST_NoteText)
			);
		}

		#endregion

		public void TestBuyerSupplierLinksHelper()
		{
			var booking1 = QuotedBooking.CreateNewBooking(Factory);
			AssertNull(booking1.BuyerSupplierLinksHelper);

			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, booking1.PK);
			Factory.Save();

			var booking2 = quotedBooking.Booking;
			AssertNotNull(booking2);
			AssertEquals(booking1.PK, booking2.PK);
			AssertNull(booking1.BuyerSupplierLinksHelper);
		}

		#region IsDPSFreightMovementRestricted

		public void TestIsDPSFreightMovementRestricted()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.All);
				var testQuotedBooking = (QuotedBooking)GetNewBusinessObject();
				testQuotedBooking.Booking.JS_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
				Factory.Save();
				AssertEquals("Quoted booking IsDPSFreightMovementRestricted should be true, the same as Shipment", true, ((ICreditControlledDocumentDelivery)testQuotedBooking).IsDPSFreightMovementRestricted);

				testQuotedBooking.Booking.JS_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				Factory.Save();
				AssertEquals("Quoted booking IsDPSFreightMovementRestricted should be true, the same as Shipment", false, ((ICreditControlledDocumentDelivery)testQuotedBooking).IsDPSFreightMovementRestricted);
			}
		}

		public void TestIsDPSFreightMovementRestrictedIsTrueIfComplianceRiskEnabled()
		{
			using (OrganisationsDataRegistry.Instance.ComplianceRiskFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.All))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var testQuotedBooking = new QuotedBooking(Factory);
				testQuotedBooking.CreateNewBookingForUniversalCopy();
				AssertEquals(testQuotedBooking.Booking.PK, ((IComplianceItemRiskStatusProvider)testQuotedBooking).ParentID);

				var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
				complianceRiskStatus.COR_ParentID = testQuotedBooking.Booking.PK;
				complianceRiskStatus.COR_OverallRisk = "PSK";
				complianceRiskStatus.COR_PartyRisk = "PSK";

				AssertEquals(true, ((ICreditControlledDocumentDelivery)testQuotedBooking).IsDPSFreightMovementRestricted);
			}
		}

		public void TestIsDPSFreightMovementRestrictedIsFalseIfComplianceRiskEnabled()
		{
			using (OrganisationsDataRegistry.Instance.ComplianceRiskFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.All))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var testQuotedBooking = new QuotedBooking(Factory);
				testQuotedBooking.CreateNewBookingForUniversalCopy();
				AssertEquals(testQuotedBooking.Booking.PK, ((IComplianceItemRiskStatusProvider)testQuotedBooking).ParentID);

				var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
				complianceRiskStatus.COR_ParentID = testQuotedBooking.Booking.PK;
				complianceRiskStatus.COR_OverallRisk = "OVR";

				AssertEquals(false, ((ICreditControlledDocumentDelivery)testQuotedBooking).IsDPSFreightMovementRestricted);
			}
		}

		public void TestIsDPSFreightMovementRestrictedIsFalseIfComplianceRiskEnabled_WithoutBooking()
		{
			using (OrganisationsDataRegistry.Instance.ComplianceRiskFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.All))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var testQuotedBooking = new QuotedBooking(Factory);
				AssertNull(testQuotedBooking.Booking);

				AssertEquals(false, ((ICreditControlledDocumentDelivery)testQuotedBooking).IsDPSFreightMovementRestricted);
			}
		}

		#endregion

		#region IComplianceItemRiskStatusProvider_IsEnabledComplianceWise

		public void TestIComplianceItemRiskStatusProvider_IsEnabledComplianceWise()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(true)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var testQuotedBooking1 = new QuotedBooking(Factory);
				AssertNull(testQuotedBooking1.Booking);

				AssertEquals(false, ((IComplianceItemRiskStatusProvider)testQuotedBooking1).IsEnabledComplianceWise);

				var testQuotedBooking2 = new QuotedBooking(Factory);
				testQuotedBooking2.CreateNewBookingForUniversalCopy();
				AssertNotNull(testQuotedBooking2.Booking);

				AssertEquals(true, ((IComplianceItemRiskStatusProvider)testQuotedBooking2).IsEnabledComplianceWise);

				var factory = new TemplateRecordBusinessObjectFactory();
				var booking = QuotedBooking.CreateNewBooking(factory);
				var testQuotedBooking3 = QuotedBooking.New(ZGuid.Empty, booking.PK, factory);
				factory.TemplateRecordProvider = testQuotedBooking3;
				factory.TemplateRecordProvider.IsTemplateRecord = true;
				AssertNotNull(testQuotedBooking3.Booking);
				AssertEquals(true, testQuotedBooking3.IsTemplate);

				AssertEquals(false, ((IComplianceItemRiskStatusProvider)testQuotedBooking3).IsEnabledComplianceWise);
			}

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var testQuotedBooking4 = new QuotedBooking(Factory);
				testQuotedBooking4.CreateNewBookingForUniversalCopy();
				AssertNotNull(testQuotedBooking4.Booking);

				AssertEquals(false, ((IComplianceItemRiskStatusProvider)testQuotedBooking4).IsEnabledComplianceWise);
			}
		}

		#endregion

		#region IsAviationSecurityFreightMovementRestricted

		public void TestIsAviationSecurityFreightMovementRestricted()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Germany))
			using (FreightDataRegistry.Instance.AviationSecurityTrainingRestrictions_EU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AviationSecurityTrainingRestriction(true)))
			{
				var shipment = QuotedBooking.CreateNewBooking(Factory);
				var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "DEFRA";
				shipment.JS_RL_NKDestination = "USLAX";

				var orgProxy = GlbBranch.CurrentBranch.OrgProxy;
				var orgProxyApproval = orgProxy.MainAddress.KnownShipperDetails.AddNew();
				orgProxyApproval.OV_OH_OrgHeader = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				orgProxyApproval.OV_EXApprovedOrMajorExporter = "RA";
				orgProxyApproval.OV_EXApprovalNumber = "12345-01";
				orgProxyApproval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(100);

				GlbStaff.CurrentUser.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;

				Factory.Save();

				GlbStaff.CurrentUser.Certificates.DeleteAll();
				AssertEquals(true, ((ICreditControlledDocumentDelivery)quotedBooking).IsAviationSecurityFreightMovementRestricted);

				var bkgCertificate = GlbStaff.CurrentUser.Certificates.AddNew();
				bkgCertificate.XZ_Type = StaffDefaultCertificateIDAndTrainingTypes.BKG;

				var dtaCertificate = GlbStaff.CurrentUser.Certificates.AddNew();
				dtaCertificate.XZ_Type = StaffDefaultCertificateIDAndTrainingTypes.DTA;

				AssertEquals(false, ((ICreditControlledDocumentDelivery)quotedBooking).IsAviationSecurityFreightMovementRestricted);

				shipment.JS_RL_NKDestination = "DEHAM";
				GlbStaff.CurrentUser.Certificates.DeleteAll();
				AssertEquals(false, ((ICreditControlledDocumentDelivery)quotedBooking).IsAviationSecurityFreightMovementRestricted);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Germany))
			using (FreightDataRegistry.Instance.AviationSecurityTrainingRestrictions_EU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AviationSecurityTrainingRestriction(false)))
			{
				var shipment = QuotedBooking.CreateNewBooking(Factory);
				var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "DEFRA";
				shipment.JS_RL_NKDestination = "USLAX";
				Factory.Save();
				GlbStaff.CurrentUser.Certificates.DeleteAll();
				AssertEquals(false, ((ICreditControlledDocumentDelivery)quotedBooking).IsAviationSecurityFreightMovementRestricted);
			}
		}

		#endregion

		public void TestOrganisationsForCreditChecks_OnlyAccessesOrganizationsEvaluatedForCreditControlRegistryOnce()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);
			var creditControlled = (ICreditControlledDocumentDelivery)quotedBooking;
			AssertEquals(0, creditControlled.OrganisationsForCreditChecks.Length);

			var supplier = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var buyer = Factory.LoadTop1<OrgHeader>(new ZQuery().AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, supplier.PK));
			quotedBooking.ConsignorDocumentaryAddress.E2_OA_Address = supplier.Addresses.AddNew().PK;
			quotedBooking.ConsigneeDocumentaryAddress.E2_OA_Address = buyer.Addresses.AddNew().PK;

			Factory.ClearCachedValue<OrgsEvaluatedForCreditControlCollection>(AccountingMasterFilesRegistry.OrganizationsEvaluatedForCreditControlCacheKey());
			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly = 0;
			AssertEquals(2, creditControlled.OrganisationsForCreditChecks.Length);
			Assert(creditControlled.OrganisationsForCreditChecks.Contains(supplier));
			Assert(creditControlled.OrganisationsForCreditChecks.Contains(buyer));
			AssertLessThanOrEqualTo("OrganizationsEvaluatedForCreditControl registry should be accessed once (at most), and then cached in factory, as reading the default value has poor performance.", AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly, 1);
		}

		public void TestOrganisationsForCreditChecks_ControllingCustomersAndControllingAgents()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);
			var creditControlled = (ICreditControlledDocumentDelivery)quotedBooking;
			AssertEquals(0, creditControlled.OrganisationsForCreditChecks.Length);

			var controllingAgent = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var controllingCustomer = Factory.LoadTop1<OrgHeader>(new ZQuery().AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, controllingAgent.PK));
			quotedBooking.ControllingCustomerDocumentaryAddress.E2_OA_Address = controllingCustomer.Addresses.AddNew().PK;
			quotedBooking.ControllingAgentDocumentaryAddress.E2_OA_Address = controllingAgent.Addresses.AddNew().PK;

			AccountingMasterFilesRegistry.Instance.EnableCreditControlEvaluationForControllingCustomersAndControllingAgents.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(0, creditControlled.OrganisationsForCreditChecks.Length);

			AccountingMasterFilesRegistry.Instance.EnableCreditControlEvaluationForControllingCustomersAndControllingAgents.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(2, creditControlled.OrganisationsForCreditChecks.Length);
			Assert(creditControlled.OrganisationsForCreditChecks.Contains(controllingAgent));
			Assert(creditControlled.OrganisationsForCreditChecks.Contains(controllingCustomer));
		}

		public void TestServiceLevelOrTransitTimeCollection()
		{
			var transportModes = new[] { Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Sea };
			foreach (var transportMode in transportModes)
			{
				var shipment = QuotedBooking.CreateNewBooking(Factory);
				var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);
				quotedBooking.TransportMode = transportMode;

				using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
				{
					AssertCollectionIsTransitTimeCollection(shipment, Core.Constants.HBLDeliveryModes.Codes.ARPT_ARPT);
					AssertCollectionIsTransitTimeCollection(shipment, Core.Constants.HBLDeliveryModes.Codes.ARPT_CFS);
					AssertCollectionIsTransitTimeCollection(shipment, Core.Constants.HBLDeliveryModes.Codes.ARPT_DOOR);
					AssertCollectionIsTransitTimeCollection(shipment, Core.Constants.HBLDeliveryModes.Codes.CFS_ARPT);
					AssertCollectionIsTransitTimeCollection(shipment, Core.Constants.HBLDeliveryModes.Codes.CFS_CFS);
					AssertCollectionIsServiceLevelCollection(shipment, Core.Constants.HBLDeliveryModes.Codes.CFS_CY);
					AssertCollectionIsTransitTimeCollection(shipment, Core.Constants.HBLDeliveryModes.Codes.CFS_DOOR);
					AssertCollectionIsServiceLevelCollection(shipment, Core.Constants.HBLDeliveryModes.Codes.CY_CY);
					AssertCollectionIsServiceLevelCollection(shipment, Core.Constants.HBLDeliveryModes.Codes.CY_CFS);
					AssertCollectionIsServiceLevelCollection(shipment, Core.Constants.HBLDeliveryModes.Codes.CY_DOOR);
					AssertCollectionIsTransitTimeCollection(shipment, Core.Constants.HBLDeliveryModes.Codes.DOOR_ARPT);
					AssertCollectionIsTransitTimeCollection(shipment, Core.Constants.HBLDeliveryModes.Codes.DOOR_CFS);
					AssertCollectionIsServiceLevelCollection(shipment, Core.Constants.HBLDeliveryModes.Codes.DOOR_CY);
					AssertCollectionIsTransitTimeCollection(shipment, Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR);
					AssertCollectionIsServiceLevelCollection(shipment, Core.Constants.HBLDeliveryModes.Codes.DOOR_PORT);
					AssertCollectionIsServiceLevelCollection(shipment, Core.Constants.HBLDeliveryModes.Codes.PORT_DOOR);
					AssertCollectionIsServiceLevelCollection(shipment, Core.Constants.HBLDeliveryModes.Codes.PORT_PORT);
				}

				using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = false, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
				{
					AssertEquals(
						"Type of ServiceLevelOrTransitTimeCollection should be RefServiceLevelCollection when DDD registry is disabled.",
						typeof(RefServiceLevelCollection),
						quotedBooking.ServiceLevelOrTransitTimeCollection.GetType());
				}
			}
		}

		void AssertCollectionIsTransitTimeCollection(ForwardingShipment shipment, ZString hblDeliveryMode)
		{
			shipment.JS_HBLContainerPackModeOverride = hblDeliveryMode;
			AssertEquals(
				$"Type of ServiceLevelOrTransitTimeCollection [{shipment.JS_TransportMode}][{hblDeliveryMode}] should be TransitTimeServiceLevelCombinationCollection.",
				typeof(TransitTimeServiceLevelCombinationCollection), shipment.Lookups.ServiceLevelOrTransitTimeCollection.GetType());
		}

		void AssertCollectionIsServiceLevelCollection(ForwardingShipment shipment, ZString hblDeliveryMode)
		{
			shipment.JS_HBLContainerPackModeOverride = hblDeliveryMode;
			AssertEquals(
				$"Type of ServiceLevelOrTransitTimeCollection [{shipment.JS_TransportMode}][{hblDeliveryMode}] should be ActiveServiceLevelCollection.",
				typeof(ActiveServiceLevelCollection), shipment.Lookups.ServiceLevelOrTransitTimeCollection.GetType());
		}

		#region IParentDocManagerSupport

		public void TestIParentDocManagerSupport()
		{
			var booking1 = QuotedBooking.CreateNewBooking(Factory);
			var quoteBooking1 = CreateNewQuotedBooking(ZGuid.Empty, booking1.PK);
			AssertIParentDocManagerSupport(quoteBooking1, true);

			var quote2 = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking2 = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking2 = CreateNewQuotedBooking(quote2.PK, booking2.PK);
			AssertIParentDocManagerSupport(quotedBooking2, true);

			var quote3 = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var booking3 = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking3 = CreateNewQuotedBooking(quote3.PK, booking3.PK);
			AssertIParentDocManagerSupport(quotedBooking3, true);

			var quote4 = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			var quotedBooking4 = CreateNewQuotedBooking(quote4.PK, ZGuid.Empty);
			AssertIParentDocManagerSupport(quotedBooking4, false);
		}

		void AssertIParentDocManagerSupport(QuotedBooking quote, bool fromBooking)
		{
			AssertNotNull(quote);

			var parentDocManager = quote as IParentDocManagerSupport;
			AssertNotNull(parentDocManager);

			if (fromBooking)
			{
				var booking = quote.Booking;
				AssertNotNull(booking);
				AssertEquals(booking.PK, parentDocManager.ParentGuid);
				AssertEquals(JobShipmentSchema.Constants.TableName, parentDocManager.ParentTableName);
			}
			else
			{
				AssertEquals(quote.PK, parentDocManager.ParentGuid);
				AssertEquals(ViewQuotedBookingSchema.Constants.TableName, parentDocManager.ParentTableName);
			}
		}

		#endregion

		#region IDocDataAddresses

		#region TestSupportedAddressTypes + TestGetDocAddress

		public void TestSupportedAddressTypes()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quickBooking = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);
			var addressTypes = ((IDocAddresses)quickBooking).SupportedAddressTypes;

			AssertEquals("New address types might have been added, ensure they are tested.", 11, addressTypes.Count);

			AssertCollectionContains(DocAddressType.ConsignorDocumentaryAddress, addressTypes);
			AssertCollectionContains(DocAddressType.ConsignorPickupDeliveryAddress, addressTypes);
			AssertCollectionContains(DocAddressType.ConsigneeDocumentaryAddress, addressTypes);
			AssertCollectionContains(DocAddressType.ConsigneePickupDeliveryAddress, addressTypes);
			AssertCollectionContains(DocAddressType.PickupAgent, addressTypes);
			AssertCollectionContains(DocAddressType.DeliveryAgent, addressTypes);
			AssertCollectionContains(DocAddressType.ExportBroker, addressTypes);
			AssertCollectionContains(DocAddressType.ImportBroker, addressTypes);
			AssertCollectionContains(DocAddressType.NotifyParty, addressTypes);
			AssertCollectionContains(DocAddressType.NotifyParty2, addressTypes);
			AssertCollectionContains(DocAddressType.NotifyParty3, addressTypes);
		}

		public void TestGetDocAddress()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quickBooking = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);
			IDocAddresses iDocAddresses = quickBooking;

			var addressType = iDocAddresses.GetDocAddressRequirement(DocAddressType.NotifyParty).DefaultDocAddressType;
			AssertEquals(DocAddressType.NotifyParty, addressType);

			addressType = iDocAddresses.GetDocAddressRequirement(DocAddressType.NotifyParty2).DefaultDocAddressType;
			AssertEquals(DocAddressType.NotifyParty2, addressType);

			addressType = iDocAddresses.GetDocAddressRequirement(DocAddressType.NotifyParty3).DefaultDocAddressType;
			AssertEquals(DocAddressType.NotifyParty3, addressType);
		}

		public void TestJobDocAddressParent()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			var quotedBooking = CreateNewQuotedBooking(quote.PK, booking.PK);
			var notifyParty = ((IDocAddresses)quotedBooking).DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.NotifyParty);
			AssertEquals("Links to Booking", booking.PK, notifyParty.E2_ParentID);
			AssertEquals("JS", notifyParty.E2_ParentTableCode);
		}

		#endregion

		#endregion

		#region TestUniversalTemplateIsNotAppliedToBooking

		public void TestUniversalTemplateIsNotAppliedToBooking_BookingWithQuote()
		{
			AssertUniversalTemplateIsNotAppliedToBooking(QuoteBookingType.BookingWithQuote);
		}

		public void TestUniversalTemplateIsNotAppliedToBooking_QuickBooking()
		{
			AssertUniversalTemplateIsNotAppliedToBooking(QuoteBookingType.QuickBooking);
		}

		public void TestUniversalTemplateIsAppliedToConvertedBooking_BookingWithQuote()
		{
			AssertUniversalTemplateIsAppliedToConvertedBooking(QuoteBookingType.BookingWithQuote);
		}

		public void TestUniversalTemplateIsAppliedToConvertedBooking_QuickBooking()
		{
			AssertUniversalTemplateIsAppliedToConvertedBooking(QuoteBookingType.QuickBooking);
		}

		void AssertUniversalTemplateIsNotAppliedToBooking(QuoteBookingType quotedBookingType)
		{
			CreateShipmentUniversalTemplate();

			var quotedBooking = QuotedBooking.New(quotedBookingType, Factory);
			quotedBooking.Booking.JS_GoodsDescription = "aaa";
			Factory.Save();

			AssertEquals("pre: goods description is unchanged", "aaa", quotedBooking.Booking.JS_GoodsDescription);

			quotedBooking.Booking.JS_TransportMode = "AIR";
			Factory.Save();

			AssertEquals("goods description is unchanged", "aaa", quotedBooking.Booking.JS_GoodsDescription);
		}

		void AssertUniversalTemplateIsAppliedToConvertedBooking(QuoteBookingType quotedBookingType)
		{
			CreateShipmentUniversalTemplate();

			var quotedBooking = QuotedBooking.New(quotedBookingType, Factory);
			quotedBooking.Booking.JS_GoodsDescription = "aaa";
			var converter = new QuotedBookingToShipmentConverter(quotedBooking, BookingToShipmentConversionSource.WorkflowTrigger);
			Assert(!converter.HasAnyErrors(out _));

			var shipment = quotedBooking.Booking;
			converter.ConvertBookingToShipment(shipment);
			Factory.Save();

			shipment.JS_TransportMode = "AIR";
			Factory.Save();
			AssertEquals("goods description is changed", "bbb", shipment.JS_GoodsDescription);
		}

		ProcessTaskTemplate CreateShipmentUniversalTemplate()
		{
			var universalTemplate = MasterFilesTestHelper.CreateUniversalTemplate(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);
			universalTemplate.P0_Name = "UniversalTemplate";
			var templateTrigger = (IUniversalTemplateTrigger)universalTemplate.TemplateTriggers.AddNew();
			templateTrigger.TriggerConditions_ForBinding.TriggerEventCode = Events.EditedARecordCode;
			templateTrigger.Description = "Edited";

			var action = (ProcessTaskNotification)templateTrigger.TriggerActions.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = JobShipmentSchema.JS_GoodsDescription.Name;
			action.PQ_FieldValue = "bbb";

			return universalTemplate;
		}

		#endregion

		#region IScreeningPartyForVessel

		public void TestIScreeningPartyForVessel_CodeAndCurrentScreeningStatusOfScreeningParty()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);
			AssertNotNull("Precondition: QuotedBooking should be IScreeningPartyForVessel", quotedBooking);

			SetUpQuotedBooking(quotedBooking, "123456", "CLR");

			AssertEquals("Precondition", "123456", ((IScreeningPartyForVessel)quotedBooking).Code);
			AssertEquals("Precondition", "CLR", ((IScreeningPartyForVessel)quotedBooking).CurrentScreeningStatus);
		}

		#endregion

		#region IScreeningPartyProvider

		public void TestGetWorstScreeningStatus()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);
			AssertEquals(shipment.JS_BookedVesselScreeningStatus, (quotedBooking as IScreeningPartyProvider).GetWorstScreeningStatus());
		}

		public void TestGetWorstScreeningStatusUnlessManuallyCleared()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);
			AssertEquals(shipment.JS_BookedVesselScreeningStatus, (quotedBooking as IScreeningPartyProvider).GetWorstScreeningStatusUnlessManuallyCleared());
		}

		public void TestScreeningParties()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);

			SetUpQuotedBooking(quotedBooking, "123456", "CLR");

			var parties = (quotedBooking as IScreeningPartyProvider).ScreeningParties;
			CombineAssertions(() =>
			{
				AssertEquals(1, parties.Length);
				AssertEquals(quotedBooking, parties.Single().Parent);
				AssertEquals(quotedBooking, parties.Single().NotLinkedVessel);
				AssertEquals("Vessel", parties.Single().Description);
				AssertEquals("123456", parties.Single().Code);
				AssertEquals("CLR", parties.Single().CurrentScreeningStatus);
			});
		}

		public void TestScreeningStatus()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);
			AssertEquals(shipment.JS_BookedVesselScreeningStatus, (quotedBooking as IScreeningPartyProvider).ScreeningStatus);

			(quotedBooking as IScreeningPartyProvider).ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			AssertEquals(ScreeningStatusesList.Codes.Matched, shipment.JS_BookedVesselScreeningStatus);
		}

		#endregion

		#region RestoreImportBroker

		public static ZGuid CreateOrgHeaderInDB(int number)
		{
			OrgHeader header = new BusinessObjectFactory().New(typeof(OrgHeader)) as OrgHeader;
			header.OH_FullName = "F" + number;
			header.OH_RL_NKClosestPort = ZString.Empty;
			header.MainAddress.FillWithValidTestData();
			header.OH_Code = "C" + number;
			header.Factory.Save();

			return header.PK;
		}

		public void TestRestoreImportBroker_QuickBooking()
		{
			FreightImportPKs testPKs = new FreightImportPKs();
			testPKs.AirImportCustomsBroker = CreateOrgHeaderInDB(1);
			testPKs.SeaImportCustomsBroker = CreateOrgHeaderInDB(2);

			var quickBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			quickBooking.Booking.ConsigneePK = ZGuid.NewZGuid();
			Assert(quickBooking.Booking.JS_OH_ImportBroker.IsEmpty);

			var orgA = OrgHeader.New(Factory);
			orgA.SetRelatedParty(testPKs.AirImportCustomsBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.All, ZString.Empty);

			quickBooking.Booking.ConsigneePK = orgA.PK;
			AssertEquals("ImporBroker should be restored", quickBooking.Booking.JS_OH_ImportBroker, testPKs.AirImportCustomsBroker);
		}

		#endregion

		#region RecalculateRelatedParties

		public void TestRecalculateRelatedParties_ShouldSetDefaultValueForPickupDeliveryProperties()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Namibia))
			{
				var booking = QuotedBooking.CreateNewBooking(Factory);
				var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);
				booking.JS_RL_NKOrigin = "AUSYD";
				booking.JS_RL_NKDestination = "CNSHA";

				var result = quotedBooking.RecalculateRelatedParties();
				Assert("When login company is not origin nor destination", !result.WasSuccessful);
				AssertEquals("When login company is not origin nor destination", "You cannot Recalculate Related Parties for this company because the company does not match Pickup or Delivery direction of this job.", result.Log);

				booking.JS_RL_NKOrigin = "NASWP";

				quotedBooking.AttemptToUpdateExistingValueInRecalculation += Shipment_AttempToUpdateExistingValueInRecalculation;

				AssertProperty(quotedBooking,
					value => booking.ConsignorDocumentaryAddress.OrganisationPK = value,
					RelatedPartyTypeList.Codes.CustomsAgentBroker,
					RelatedPartyDirectionList.Codes.Pickup,
					"Export Broker",
					() => booking.JS_OH_ExportBroker,
					value => booking.JS_OH_ExportBroker = value);

				booking.JS_OH_ExportBroker = ZGuid.Empty;

				AssertProperty(quotedBooking,
					value => booking.ConsignorDocumentaryAddress.OrganisationPK = value,
					RelatedPartyTypeList.Codes.LocalTransport,
					RelatedPartyDirectionList.Codes.Pickup,
					"Port Transport",
					() => booking.DocsAndCartage.JP_OA_PickupCartageCoAddr_ZAddress.OrgPK,
					value => booking.DocsAndCartage.JP_OA_PickupCartageCoAddr_ZAddress.OrgPK = value);

				booking.DocsAndCartage.JP_OA_PickupCartageCoAddr_ZAddress.OrgPK = ZGuid.Empty;

				AssertProperty(quotedBooking,
					value => booking.ConsignorDocumentaryAddress.OrganisationPK = value,
					RelatedPartyTypeList.Codes.PickupAgent,
					RelatedPartyDirectionList.Codes.Pickup,
					"Pickup Agent",
					() => booking.PickupAgentDocumentaryAddress.OrganisationPK,
					value => booking.PickupAgentDocumentaryAddress.OrganisationPK = value);

				booking.PickupAgentDocumentaryAddress.OrganisationPK = ZGuid.Empty;

				AssertProperty(quotedBooking,
					value => booking.ConsignorDocumentaryAddress.OrganisationPK = value,
					RelatedPartyTypeList.Codes.ClientCFS,
					RelatedPartyDirectionList.Codes.Pickup,
					QuotedBookingHelper.GetPickupDeliveryOrgTitles(quotedBooking.Mode).PickupOrgTitle,
					() => booking.ExportReceivingDepot.OA_OH,
					value => booking.JS_OA_ExportReceivingDepot = value == ZGuid.Empty ? ZGuid.Empty : Factory.Load<OrgHeader>(value).MainAddress.PK);

				booking.JS_OA_ExportReceivingDepot = ZGuid.Empty;

				using (FreightDataRegistry.Instance.DefaultShipmentControllingCustomer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					quotedBooking.Booking.JS_INCO = Constants.IncoTerms.CarriagePaidTo;
					AssertProperty(quotedBooking,
						value => booking.ConsignorDocumentaryAddress.OrganisationPK = value,
						RelatedPartyTypeList.Codes.ControllingCustomer,
						RelatedPartyDirectionList.Codes.Pickup,
						"Controlling Customer",
						() => booking.ControllingCustomerAddress.OrganisationPK,
						value => booking.ControllingCustomerAddress.OrganisationPK = value);
				}

				booking.ControllingCustomerAddress.OrganisationPK = ZGuid.Empty;
				using (FreightDataRegistry.Instance.DefaultShipmentControllingAgent.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					quotedBooking.Booking.JS_INCO = Constants.IncoTerms.CarriagePaidTo;
					AssertProperty(quotedBooking,
						value => booking.ConsignorDocumentaryAddress.OrganisationPK = value,
						RelatedPartyTypeList.Codes.ControllingAgent,
						RelatedPartyDirectionList.Codes.Sales,
						"Controlling Agent",
						() => booking.ControllingAgentDocumentaryAddress.OrganisationPK,
						value => booking.ControllingAgentDocumentaryAddress.E2_OA_Address = value == ZGuid.Empty ? ZGuid.Empty : Factory.Load<OrgHeader>(value).MainAddress.PK);
				}

				booking.ControllingAgentDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
				booking.JS_RL_NKOrigin = "CNSHA";
				booking.JS_RL_NKDestination = "NASWP";

				AssertProperty(quotedBooking,
					value => booking.ConsigneeDocumentaryAddress.OrganisationPK = value,
					RelatedPartyTypeList.Codes.CustomsAgentBroker,
					RelatedPartyDirectionList.Codes.Delivery,
					"Import Broker",
					() => booking.JS_OH_ImportBroker,
					value => booking.JS_OH_ImportBroker = value);

				booking.JS_OH_ImportBroker = ZGuid.Empty;

				AssertProperty(quotedBooking,
					value => booking.ConsigneeDocumentaryAddress.OrganisationPK = value,
					RelatedPartyTypeList.Codes.DeliveryAgent,
					RelatedPartyDirectionList.Codes.Delivery,
					"Delivery Agent",
					() => booking.JS_OH_DeliveryAgent,
					value => booking.JS_OH_DeliveryAgent = value);

				booking.JS_OH_DeliveryAgent = ZGuid.Empty;

				AssertProperty(quotedBooking,
					value => booking.ConsigneeDocumentaryAddress.OrganisationPK = value,
					RelatedPartyTypeList.Codes.ClientCFS,
					RelatedPartyDirectionList.Codes.Delivery,
					QuotedBookingHelper.GetPickupDeliveryOrgTitles(quotedBooking.Mode).DeliveryOrgTitle,
					() => booking.ImportReleaseDepot.OA_OH,
					value => booking.JS_OA_ImportReleaseDepot = value == ZGuid.Empty ? ZGuid.Empty : Factory.Load<OrgHeader>(value).MainAddress.PK);

				booking.JS_OA_ImportReleaseDepot = ZGuid.Empty;
				using (FreightDataRegistry.Instance.DefaultShipmentControllingCustomer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					quotedBooking.Booking.JS_INCO = Constants.IncoTerms.FreeCarrier;
					AssertProperty(quotedBooking,
						value => booking.ConsigneeDocumentaryAddress.OrganisationPK = value,
						RelatedPartyTypeList.Codes.ControllingCustomer,
						RelatedPartyDirectionList.Codes.Delivery,
						"Controlling Customer",
						() => booking.ControllingCustomerAddress.OrganisationPK,
						value => booking.ControllingCustomerAddress.OrganisationPK = value);
				}

				booking.ControllingCustomerAddress.OrganisationPK = ZGuid.Empty;
				using (FreightDataRegistry.Instance.DefaultShipmentControllingAgent.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					quotedBooking.Booking.JS_INCO = Constants.IncoTerms.FreeCarrier;
					AssertProperty(quotedBooking,
						value => booking.ConsigneeDocumentaryAddress.OrganisationPK = value,
						RelatedPartyTypeList.Codes.ControllingAgent,
						RelatedPartyDirectionList.Codes.Sales,
						"Controlling Agent",
						() => booking.ControllingAgentDocumentaryAddress.OrganisationPK,
						value => booking.ControllingAgentDocumentaryAddress.E2_OA_Address = value == ZGuid.Empty ? ZGuid.Empty : Factory.Load<OrgHeader>(value).MainAddress.PK);
				}
			}
		}

		void AssertProperty(QuotedBooking booking,
				Action<ZGuid> shipperConsigneeSetter,
				ZString partyType, ZString direction,
				string propertyName, Func<ZGuid> propertyGetter, Action<ZGuid> propertySetter)
		{
			var oldValue = Factory.NewWithValidTestData<OrgHeader>();
			var newValue = Factory.NewWithValidTestData<OrgHeader>();

			AddRelatedPartyAndAssignOrg(shipperConsigneeSetter, partyType, direction, newValue);

			propertySetter(oldValue.PK);

			attemptToUpdateEventRaised = false;
			attemptToUpdateUsersAnswer = true;
			var result = booking.RecalculateRelatedParties();
			Assert("Attempt to update event should be raised", attemptToUpdateEventRaised);
			AssertEquals($"{propertyName} shouldn't be changed", oldValue.PK, propertyGetter());
			Assert("When login company is shipment's origin", result.WasSuccessful);
			AssertEquals("When login company is not origin nor destination", ZString.Empty, result.Log);

			attemptToUpdateEventRaised = false;
			attemptToUpdateUsersAnswer = false;
			result = booking.RecalculateRelatedParties();
			Assert("Attempt to update event should be raised", attemptToUpdateEventRaised);
			AssertEquals($"{propertyName} should be changed", newValue.PK, propertyGetter());
			Assert("When login company is shipment's origin", result.WasSuccessful);
			AssertEquals("When login company is not origin nor destination", ZString.Empty, result.Log);

			propertySetter(ZGuid.Empty);
			attemptToUpdateEventRaised = false;
			attemptToUpdateUsersAnswer = false;
			result = booking.RecalculateRelatedParties();
			Assert("Attempt to update event should not be raised", !attemptToUpdateEventRaised);
			AssertEquals($"{propertyName} should be changed", newValue.PK, propertyGetter());
			Assert("When login company is shipment's origin", result.WasSuccessful);
			AssertEquals("When login company is not origin nor destination", ZString.Empty, result.Log);
		}

		void AddRelatedPartyAndAssignOrg(
				Action<ZGuid> shipperConsigneeSetter,
				ZString partyType,
				ZString direction,
				OrgHeader newValue)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.AddRelatedParty(newValue.PK, partyType, direction, Constants.TransportModes.All, ZString.Empty, GlbCompany.CurrentCompany);
			shipperConsigneeSetter(org.PK);
		}

		bool attemptToUpdateUsersAnswer;
		bool attemptToUpdateEventRaised;

		void Shipment_AttempToUpdateExistingValueInRecalculation(object sender, CancelEventArgs e)
		{
			e.Cancel = attemptToUpdateUsersAnswer;
			attemptToUpdateEventRaised = true;
		}

		#endregion

		#region CO2 for OOQ

		public void TestRequireTEU_OOQ()
		{
			var refContainer = NewRefContainer("20GP000", "22G0", 1m, 2280m);
			var refContainerNoTEU = NewRefContainer("20GP111", "22G9", 0m, 2280m);
			void AssertRequireTEU(Action<QuotedBooking> setup, bool expected = true)
			{
				// Arrange
				var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
				var qb = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
				setup(qb);

				// Act & Assert
				AssertEquals(expected, (qb as ICO2eCalculationSupporter).RequireTEU);
			}

			AssertRequireTEU((qb) =>
			{
				qb.TransportMode = TransportModes.Sea;
				qb.ContainerMode = RateMode.FCL;
				var container = qb.Quote.CurrentOneOffQuote.Containers.AddNew();
				container.TC_ContainerCount = 2;
				container.TC_RC = refContainer.PK;
			});

			AssertRequireTEU((qb) =>
			{
				qb.TransportMode = TransportModes.Road;
				qb.ContainerMode = RateMode.FCL;
				var container = qb.Quote.CurrentOneOffQuote.Containers.AddNew();
				container.TC_ContainerCount = 2;
				qb.Weight = 0;
			});

			AssertRequireTEU((qb) =>
			{
				qb.TransportMode = TransportModes.Rail;
				qb.ContainerMode = RateMode.FCL;
				var container = qb.Quote.CurrentOneOffQuote.Containers.AddNew();
				container.TC_ContainerCount = 2;
				qb.Weight = 0;
			});

			AssertRequireTEU((qb) =>
			{
				qb.TransportMode = TransportModes.Sea;
				qb.ContainerMode = RateMode.FCL;
				var container = qb.Quote.CurrentOneOffQuote.Containers.AddNew();
				container.TC_ContainerCount = 2;
				container.TC_RC = refContainerNoTEU.PK;
			}, false);
		}

		public void TestOneOffQuoteWightForCO2Calculation_ShouldIgnoreContainerWeight()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var qb = CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
			Factory.Save();
			qb.Mode = RateMode.FCL;
			qb.Weight = 100;
			qb.WeightUnit = Constants.Weight.Kilograms;

			var gp = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var container1 = quote.CurrentOneOffQuote.Containers.AddNew();
			container1.TC_ContainerCount = 2;
			container1.TC_RC = gp.PK;

			var re = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE");
			var container2 = quote.CurrentOneOffQuote.Containers.AddNew();
			container2.TC_ContainerCount = 4;
			container2.TC_RC = re.PK;

			var aak = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "AAK");
			var container3 = quote.CurrentOneOffQuote.Containers.AddNew();
			container3.TC_ContainerCount = 3;
			container3.TC_RC = aak.PK;
			AssertEquals("AAK container Gross weight should be 0", 0m, aak.RC_GrossWeight);

			var aaa = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "AAA");
			var container4 = quote.CurrentOneOffQuote.Containers.AddNew();
			container4.TC_ContainerCount = 5;
			container4.TC_RC = aaa.PK;
			aaa.RC_GrossWeight = 200m;
			AssertEquals("AAA container Tare weight should be 0", 0m, aaa.RC_TareWeight);

			AssertEquals("Should ignore container weight and use booking weight", 100m, ((ICO2eLegBasedSupporter)qb).Weight);

			qb.WeightUnit = Weight.Tonnes;
			AssertEquals(Weight.Tonnes, ((ICO2eLegBasedSupporter)qb).UnitOfWeight);
			AssertEquals(100m, ((ICO2eLegBasedSupporter)qb).Weight);

			qb.Mode = RateMode.ULD;
			qb.Weight = 100;
			qb.WeightUnit = Weight.Kilograms;
			AssertEquals(100m, ((ICO2eLegBasedSupporter)qb).Weight);
			qb.WeightUnit = Weight.Tonnes;
			AssertEquals(100m, ((ICO2eLegBasedSupporter)qb).Weight);

			qb.Mode = RateMode.LSE;
			qb.Weight = 100;
			qb.WeightUnit = Weight.Kilograms;
			AssertEquals(100m, ((ICO2eLegBasedSupporter)qb).Weight);
			qb.WeightUnit = Weight.Tonnes;
			AssertEquals(100m, ((ICO2eLegBasedSupporter)qb).Weight);

			qb.WeightUnit = string.Empty;
			AssertEquals(Weight.Kilograms, ((ICO2eLegBasedSupporter)qb).UnitOfWeight);
		}

		#endregion

		public void TestLoadConsolidatedBooking_DoesNotDeleteContainer()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			quotedBooking.Mode = "FCL";
			var bookingContainer = quotedBooking.QuotedBookingContainers.AddNew();
			bookingContainer.JC_ContainerNum = "CONT01";
			bookingContainer.JC_ContainerMode = "FCL";
			bookingContainer.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;

			Factory.Save();

			var buildConsolHelper = new BuildConsolHelper();
			var consol = Factory.New<ForwardingConsol>();
			buildConsolHelper.MakeConsolFromBookingOrStandaloneShipment(consol, quotedBooking.Booking.PK);

			AssertEquals("Booking Container has been converted to Consol Container", 1, consol.Containers.Count);
			AssertEquals("CONT01", consol.Containers[0].JC_ContainerNum);

			consol.JK_ConsolMode = "BCN";
			quotedBooking.Booking.JS_PackingMode = "BCN";

			Factory.Save();

			var newFac = new BusinessObjectFactory();
			var shipment = newFac.Load<ForwardingShipment>(quotedBooking.Booking.PK);
			QuotedBooking reloadedQuotedBooking = null;
			BusinessObjectFactory.SetOnFactorySaveHookForTest(delegate(BusinessObjectFactory factory)
			{
				reloadedQuotedBooking = factory.Load<QuotedBooking>(quotedBooking.ViewPK);
			});

			newFac.Save();
			AssertEquals("Booking Container is not deleted", 1, reloadedQuotedBooking.QuotedBookingContainers.Count);
			AssertEquals("Consol Container is not deleted", 1, consol.Containers.Count);
		}

		#region CO2e

		[TestDate(2024, 1, 1)]
		public void TestIfStatusIsChangedToNCU_When_ContainerIsAddedOrDeleted()
		{
			var twentyGP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

			//BookingWithQuote
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			quotedBooking.TransportMode = TransportModes.Sea;
			quotedBooking.ContainerMode = Constants.ContainerModes.FCL;
			AssertEquals(QuotedBookingState.AcceptedBookingWithQuote, quotedBooking.ObjectState);

			//Act & Assert: add new container
			ForwardingContainer container = null;
			AssertCO2eStatus(quotedBooking, () =>
			{
				container = quotedBooking.QuotedBookingContainers.AddNew();
				container.JC_RC = twentyGP.PK;
			}, "Container added");

			//Act & Assert: remove container
			AssertCO2eStatus(quotedBooking, () => quotedBooking.QuotedBookingContainers.RemoveAndDelete(container), "Container removed");

			//BookingOnly
			booking = QuotedBooking.CreateNewBooking(Factory);
			quotedBooking = QuotedBooking.New(Guid.Empty, booking.PK, Factory);
			quotedBooking.TransportMode = TransportModes.Sea;
			quotedBooking.ContainerMode = Constants.ContainerModes.FCL;
			AssertEquals(QuotedBookingState.BookingOnly, quotedBooking.ObjectState);

			//Act & Assert: add new container
			AssertCO2eStatus(quotedBooking, () =>
			{
				container = quotedBooking.QuotedBookingContainers.AddNew();
				container.JC_RC = twentyGP.PK;
			}, "Container added");

			//Act & Assert: remove container
			AssertCO2eStatus(quotedBooking, () => quotedBooking.QuotedBookingContainers.RemoveAndDelete(container), "Container removed");

			//One-Off Quote
			RateOneOffContainers ooqContainer = null;
			var ooq = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
			ooq.TransportMode = TransportModes.Sea;
			ooq.ContainerMode = ContainerModes.FCL;

			var rateOneOffcontainer = ooq.Quote.CurrentOneOffQuote.Containers.AddNew();
			rateOneOffcontainer.TC_ContainerCount = 1;
			rateOneOffcontainer.TC_RC = twentyGP.PK;
			AssertEquals(QuotedBookingState.QuoteOnly, ooq.ObjectState);

			//Act & Assert: add new ooq container
			AssertCO2eStatus(ooq, () =>
			{
				ooqContainer = ooq.Quote.CurrentOneOffQuote.Containers.AddNew();
				ooqContainer.TC_ContainerCount = 1;
				ooqContainer.TC_RC = twentyGP.PK;
			}, "One Off Quote container added");

			//Act & Assert: remove first ooq container
			AssertCO2eStatus(ooq, () => ooq.Quote.CurrentOneOffQuote.Containers.RemoveAndDelete(rateOneOffcontainer), "One Off Quote container removed");
			//Act & Assert: remove second ooq container
			AssertCO2eStatus(ooq, () => ooq.Quote.CurrentOneOffQuote.Containers.RemoveAndDelete(ooqContainer), "One Off Quote container removed");
		}

		[TestDate(2024, 1, 1)]
		public void TestIfStatusIsChangedToNCU_For_AttachedBooking_When_ContainerTypeOrCountIsChanged()
		{
			//BookingWithQuote
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			quotedBooking.TransportMode = TransportModes.Sea;
			quotedBooking.ContainerMode = Constants.ContainerModes.FCL;

			var twentyGP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var fortyGP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");

			var container = quotedBooking.QuotedBookingContainers.AddNew();
			container.JC_RC = fortyGP.PK;
			container.JC_ContainerCount = 1;

			quotedBooking.SetCO2eStatus(CO2eStatusList.Codes.Current);

			AssertEquals(QuotedBookingState.AcceptedBookingWithQuote, quotedBooking.ObjectState);
			AssertCO2eStatus(quotedBooking, () => container.JC_ContainerCount = 2, "JC_ContainerCount [1]->[2]");

			quotedBooking.SetCO2eStatus(CO2eStatusList.Codes.Current);

			AssertCO2eStatus(quotedBooking, () => container.JC_RC = twentyGP.PK, $"JC_RC [{fortyGP.PK}]->[{twentyGP.PK}]");

			//BookingOnly
			booking = QuotedBooking.CreateNewBooking(Factory);
			quotedBooking = QuotedBooking.New(Guid.Empty, booking.PK, Factory);
			quotedBooking.TransportMode = TransportModes.Sea;
			quotedBooking.ContainerMode = Constants.ContainerModes.FCL;

			container = quotedBooking.QuotedBookingContainers.AddNew();
			container.JC_RC = fortyGP.PK;
			container.JC_ContainerCount = 1;

			quotedBooking.SetCO2eStatus(CO2eStatusList.Codes.Current);

			AssertEquals(QuotedBookingState.BookingOnly, quotedBooking.ObjectState);
			AssertCO2eStatus(quotedBooking, () => container.JC_ContainerCount = 2, "JC_ContainerCount [1]->[2]");

			quotedBooking.SetCO2eStatus(CO2eStatusList.Codes.Current);

			AssertCO2eStatus(quotedBooking, () => container.JC_RC = twentyGP.PK, $"JC_RC [{fortyGP.PK}]->[{twentyGP.PK}]");

			//One-Off Quote
			var ooq = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
			ooq.TransportMode = TransportModes.Sea;
			ooq.ContainerMode = ContainerModes.FCL;
			var ooqContainer = ooq.Quote.CurrentOneOffQuote.Containers.AddNew();
			ooqContainer.TC_RC = twentyGP.PK;
			ooqContainer.TC_ContainerCount = 1;

			ooq.SetCO2eStatus(CO2eStatusList.Codes.Current);

			AssertEquals(QuotedBookingState.QuoteOnly, ooq.ObjectState);
			AssertCO2eStatus(ooq, () => ooqContainer.TC_RC = fortyGP.PK, $"TC_RC [{twentyGP.PK}]->[{fortyGP.PK}]");

			ooq.SetCO2eStatus(CO2eStatusList.Codes.Current);

			AssertCO2eStatus(ooq, () => ooqContainer.TC_ContainerCount = 2, $"TC_ContainerCount [1]->[2]");
		}

		void AssertCO2eStatus(QuotedBooking quotedBooking, Action action, string stuReason)
		{
			TestDateAttribute.AddMinutes(1);
			quotedBooking.SetCO2eStatus(CO2eStatusList.Codes.Current);
			Factory.Save();
			action.Invoke();
			AssertEquals(CO2eStatusList.Codes.NotCurrent, quotedBooking.GetCO2eStatus());
			CO2eTestHelper.AssertSTUEvent(quotedBooking, stuReason);
		}

		public void TestShouldNotCallRequireTEU_WhenSkipCO2eStatusCheck_Booking()
		{
			//Arange
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			quotedBooking.TransportMode = TransportModes.Sea;
			quotedBooking.ContainerMode = Constants.ContainerModes.FCL;

			var twentyGP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var fortyGP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");

			var container = quotedBooking.QuotedBookingContainers.AddNew();
			container.JC_RC = fortyGP.PK;
			container.JC_ContainerCount = 1;
			quotedBooking.SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);

			Factory.Save();

			// Act & Assert
			var requireTEUCalled = 0;
			quotedBooking.OnRequireTEUCalled += delegate { requireTEUCalled++; };

			AssertRequireTEUNotCalled(() => container.JC_RC = twentyGP.PK);
			AssertRequireTEUNotCalled(() => container.JC_ContainerCount = 2);

			void AssertRequireTEUNotCalled(Action action)
			{
				Assert(quotedBooking.SkipCO2eStatusCheck());
				action();
				AssertEquals(0, requireTEUCalled);
			}
		}

		public void TestShouldNotCallRequireTEU_WhenSkipCO2eStatusCheck_OOQ()
		{
			//Arrange
			var ooq = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
			ooq.TransportMode = TransportModes.Sea;
			ooq.ContainerMode = ContainerModes.FCL;

			var twentyGP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var fortyGP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");

			var ooqContainer = ooq.Quote.CurrentOneOffQuote.Containers.AddNew();
			ooqContainer.TC_RC = twentyGP.PK;
			ooqContainer.TC_ContainerCount = 1;
			ooq.SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);

			Factory.Save();

			// Act & Assert
			var requireTEUCalled = 0;
			ooq.OnRequireTEUCalled += delegate { requireTEUCalled++; };

			AssertRequireTEUNotCalled(() => ooqContainer.TC_RC = fortyGP.PK);
			AssertRequireTEUNotCalled(() => ooqContainer.TC_ContainerCount = 2);

			void AssertRequireTEUNotCalled(Action action)
			{
				Assert(ooq.SkipCO2eStatusCheck());
				action();
				AssertEquals(0, requireTEUCalled);
			}
		}

		public void TestTotalCO2eForSorting_DecimalPlaces()
		{
			var co2eForSortingDp = typeof(QuotedBooking)
				.GetProperty(nameof(QuotedBooking.TotalCO2eForSorting))
				.GetCustomAttributes(typeof(DecimalPlacesAttribute), true)[0] as DecimalPlacesAttribute;

			AssertEquals("TotalCO2eForSorting should display using 3dp", 3, co2eForSortingDp.DecimalPlaces);
		}

		public void TestIncludeTEU_OneOffQuote()
		{
			// Arrange
			var ooq = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
			ooq.TransportMode = TransportModes.Sea;
			ooq.ContainerMode = ContainerModes.FCL;
			var container = ooq.Quote.CurrentOneOffQuote.Containers.AddNew();
			container.TC_ContainerCount = 1;
			container.TC_RC = NewRefContainer("20GP000", "22G0", 1m, 2280m).PK;

			var supporter = (ICO2eLegBasedSupporter)ooq;
			Assert("Pre-condition", supporter.RequireTEU);
			Assert("Pre-condition", supporter.IncludeTEU);

			// Act & Assert
			ooq.TransportMode = TransportModes.Road;
			Assert(supporter.RequireTEU);
			Assert(supporter.IncludeTEU);

			ooq.ContainerMode = ContainerModes.ShippersConsol;
			Assert(supporter.IncludeTEU);

			ooq.ContainerMode = ContainerModes.BuyersConsol;
			Assert(supporter.IncludeTEU);

			ooq.ContainerMode = ContainerModes.LCL;
			Assert(!supporter.RequireTEU);
			Assert(!supporter.IncludeTEU);
		}

		public void TestIncludeTEU_QuotedBooking()
		{
			// Arrange
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			quotedBooking.TransportMode = TransportModes.Sea;
			quotedBooking.ContainerMode = ContainerModes.FCL;
			var container = quotedBooking.QuotedBookingContainers.AddNew();
			container.JC_ContainerCount = 1;
			var supporter = (ICO2eLegBasedSupporter)quotedBooking;
			Assert("Pre-condition", supporter.RequireTEU);
			Assert("Pre-condition", supporter.IncludeTEU);

			// Act & Assert
			quotedBooking.TransportMode = TransportModes.Road;
			Assert(supporter.RequireTEU);
			Assert(supporter.IncludeTEU);

			quotedBooking.ContainerMode = ContainerModes.ShippersConsol;
			Assert(supporter.IncludeTEU);

			quotedBooking.ContainerMode = ContainerModes.BuyersConsol;
			Assert(supporter.IncludeTEU);

			quotedBooking.ContainerMode = ContainerModes.LCL;
			Assert(!supporter.RequireTEU);
			Assert(!supporter.IncludeTEU);
		}

		void AssertConvertToBWQDoNotCopyJobCO2e(string status)
		{
			// Arrange
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, Guid.Empty, Factory);
			quotedBooking.Origin = "AUSYD";
			quotedBooking.Weight = status == CO2eStatusList.Codes.NotCalculated ? new ZDecimal(0) : 5m;
			quotedBooking.WeightUnit = Constants.Weight.Tonnes;
			if (status != CO2eStatusList.Codes.NotCalculated)
			{
				quotedBooking.SetCO2ePerTonneInKg(100.1111111m);
				quotedBooking.SetTotalCO2e(100.1111111m);
				quotedBooking.SetCO2eStatus(status);
			}
			Factory.Save();
			AssertNull("Precondition", quotedBooking.Booking);

			// Act
			quotedBooking.ConvertQuoteToQuotedBooking();
			var booking = Factory.Load<QuotedBooking>(quotedBooking.PK);

			// Assert
			Assert(booking.Booking.JS_IsBooking);
			AssertEquals(0m, booking.GetTotalCO2e());
			AssertEquals("NON", booking.GetCO2eStatus());
		}

		public void TestConvertToBWQ_PopulatesBookingCO2e()
		{
			foreach (CodeDescriptionPair pair in new CO2eStatusList())
			{
				AssertConvertToBWQDoNotCopyJobCO2e(pair.Code);
			}
		}

		[TestDate(2024, 1, 1)]
		public void TestUpdateCO2eStatusToNotCurrent_OneOffQuote()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			AssertUpdateCO2eStatusToNotCurrent(quotedBooking, true);

			TestDateAttribute.AddMinutes(1);
			quotedBooking.SetCO2eStatus(CO2eStatusList.Codes.Current);
			Factory.Save();
			quote.CurrentOneOffQuote.TT_ActualWeight = 333m;
			AssertEquals(CO2eStatusList.Codes.NotCurrent, quotedBooking.GetCO2eStatus());
			AssertHasWarning(quotedBooking.TotalCO2eForBindingInfo, CO2eTestHelper.CO2eStaleWarning);
			CO2eTestHelper.AssertSTUEvent(quotedBooking, "TT_ActualWeight [5]->[333]");

			TestDateAttribute.AddMinutes(1);
			quotedBooking.SetCO2eStatus(CO2eStatusList.Codes.Current);
			quote.CurrentOneOffQuote.TT_UnitOfWeight = Weight.Kilograms;
			AssertEquals(CO2eStatusList.Codes.NotCurrent, quotedBooking.GetCO2eStatus());
			AssertHasWarning(quotedBooking.TotalCO2eForBindingInfo, CO2eTestHelper.CO2eStaleWarning);
			CO2eTestHelper.AssertSTUEvent(quotedBooking, "TT_UnitOfWeight [T]->[KG]");
		}

		public void TestUpdateCO2eStatusRejected_OneOffQuote()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			quotedBooking.SetCO2eStatus(CO2eStatusList.Codes.Rejected);
			AssertHasWarning(quotedBooking.TotalCO2eForBindingInfo, "The greenhouse gas emissions value could not be calculated.");
			AssertEquals(ZString.Empty, quotedBooking.TotalCO2eForBinding);
		}

		[TestDate(2024, 1, 1)]
		public void TestUpdateCO2eStatusToNotCurrent_QuotedBooking()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);
			AssertUpdateCO2eStatusToNotCurrent(quotedBooking, false);

			quotedBooking.SetCO2eStatus(CO2eStatusList.Codes.Current);
			Factory.Save();

			TestDateAttribute.AddMinutes(1);
			shipment.JS_ActualWeight = 333m;
			AssertEquals(CO2eStatusList.Codes.NotCurrent, quotedBooking.GetCO2eStatus());
			AssertHasWarning(quotedBooking.TotalCO2eForBindingInfo, CO2eTestHelper.CO2eStaleWarning);
			CO2eTestHelper.AssertSTUEvent(quotedBooking, "JS_ActualWeight [5]->[333]");

			TestDateAttribute.AddMinutes(1);
			quotedBooking.SetCO2eStatus(CO2eStatusList.Codes.Current);
			shipment.JS_UnitOfWeight = Weight.Kilograms;
			AssertEquals(CO2eStatusList.Codes.NotCurrent, quotedBooking.GetCO2eStatus());
			AssertHasWarning(quotedBooking.TotalCO2eForBindingInfo, CO2eTestHelper.CO2eStaleWarning);
			CO2eTestHelper.AssertSTUEvent(quotedBooking, "JS_UnitOfWeight [T]->[KG]");

			TestDateAttribute.AddMinutes(1);
			quotedBooking.SetCO2eStatus(CO2eStatusList.Codes.Current);
			quotedBooking.DischargePort = "USLAX";
			AssertEquals(CO2eStatusList.Codes.NotCurrent, quotedBooking.GetCO2eStatus());
			AssertHasWarning(quotedBooking.TotalCO2eForBindingInfo, CO2eTestHelper.CO2eStaleWarning);
			CO2eTestHelper.AssertSTUEvent(quotedBooking, "DischargePort []->[USLAX]");

			TestDateAttribute.AddMinutes(1);
			quotedBooking.SetCO2eStatus(CO2eStatusList.Codes.Current);
			quotedBooking.LoadPort = "DEHAM";
			AssertEquals(CO2eStatusList.Codes.NotCurrent, quotedBooking.GetCO2eStatus());
			AssertHasWarning(quotedBooking.TotalCO2eForBindingInfo, CO2eTestHelper.CO2eStaleWarning);
			CO2eTestHelper.AssertSTUEvent(quotedBooking, "LoadPort []->[DEHAM]");
		}

		public void TestRequireTEU_QuotedBooking()
		{
			var refContainerNoTEU = NewRefContainer("20GP111", "22G0", 0m, 2280m);
			void AssertRequireTEU(Action<QuotedBooking> setup, bool expected = true)
			{
				// Arrange
				var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
				var shipment = QuotedBooking.CreateNewBooking(Factory);
				var qb = CreateNewQuotedBooking(quote.PK, shipment.PK);
				setup(qb);

				// Act & Assert
				AssertEquals(expected, (qb as ICO2eCalculationSupporter).RequireTEU);
			}

			AssertRequireTEU((qb) =>
			{
				qb.TransportMode = TransportModes.Sea;
				qb.ContainerMode = RateMode.FCL;
				var container = qb.QuotedBookingContainers.AddNew();
				container.JC_ContainerCount = 2;
			});

			AssertRequireTEU((qb) =>
			{
				qb.TransportMode = TransportModes.Road;
				qb.ContainerMode = RateMode.FCL;
				var container = qb.QuotedBookingContainers.AddNew();
				container.JC_ContainerCount = 2;
				qb.Weight = 0;
			});

			AssertRequireTEU((qb) =>
			{
				qb.TransportMode = TransportModes.Rail;
				qb.ContainerMode = RateMode.FCL;
				var container = qb.QuotedBookingContainers.AddNew();
				container.JC_ContainerCount = 2;
				qb.Weight = 0;
			});

			AssertRequireTEU((qb) =>
			{
				qb.TransportMode = TransportModes.Sea;
				qb.ContainerMode = RateMode.FCL;
				var container = qb.QuotedBookingContainers.AddNew();
				container.JC_ContainerCount = 2;
				container.JC_RC = refContainerNoTEU.PK;
			}, false);
		}

		void AssertUpdateCO2eStatusToNotCurrent(QuotedBooking quotedBooking, bool isOoq)
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();

			quotedBooking.Mode = RateMode.LSE;
			quotedBooking.Weight = 5m;
			quotedBooking.WeightUnit = Weight.Tonnes;
			quotedBooking.SetCO2ePerTonneInKg(100.1111111m);
			AssertNoWarning(quotedBooking.TotalCO2eForBindingInfo, CO2eTestHelper.CO2eStaleWarning);

			quotedBooking.SetCO2eStatus(CO2eStatusList.Codes.Current);
			Factory.Save();

			TestDateAttribute.AddMinutes(1);
			quotedBooking.OH_Carrier = carrier.PK;
			AssertEquals(CO2eStatusList.Codes.NotCurrent, quotedBooking.GetCO2eStatus());
			AssertHasWarning(quotedBooking.TotalCO2eForBindingInfo, CO2eTestHelper.CO2eStaleWarning);
			var carrierPropertyName = isOoq ? "OH_Carrier" : "OrgPK";
			CO2eTestHelper.AssertSTUEvent(quotedBooking, $"{carrierPropertyName} [{ZGuid.Empty}]->[{carrier.PK}]");

			TestDateAttribute.AddMinutes(1);
			quotedBooking.SetCO2eStatus(CO2eStatusList.Codes.Current);
			var seaMode = isOoq ? "SEA" : "FCL";
			quotedBooking.Mode = seaMode;
			AssertEquals(CO2eStatusList.Codes.NotCurrent, quotedBooking.GetCO2eStatus());
			AssertHasWarning(quotedBooking.TotalCO2eForBindingInfo, CO2eTestHelper.CO2eStaleWarning);
			CO2eTestHelper.AssertSTUEvent(quotedBooking, $"Mode [LSE]->[{seaMode}]");

			TestDateAttribute.AddMinutes(1);
			quotedBooking.SetCO2eStatus(CO2eStatusList.Codes.Current);
			using (new DisposableAction(() => quotedBooking.DisableLoadDischargeDefaulting(), () => quotedBooking.EnableLoadDischargeDefaulting()))
			{
				quotedBooking.Destination = "NZAKL";
			}
			AssertEquals(CO2eStatusList.Codes.NotCurrent, quotedBooking.GetCO2eStatus());
			AssertHasWarning(quotedBooking.TotalCO2eForBindingInfo, CO2eTestHelper.CO2eStaleWarning);
			CO2eTestHelper.AssertSTUEvent(quotedBooking, "Destination []->[NZAKL]");

			TestDateAttribute.AddMinutes(1);
			quotedBooking.SetCO2eStatus(CO2eStatusList.Codes.Current);
			using (new DisposableAction(() => quotedBooking.DisableLoadDischargeDefaulting(), () => quotedBooking.EnableLoadDischargeDefaulting()))
			{
				quotedBooking.Origin = "AUSYD";
			}
			AssertEquals(CO2eStatusList.Codes.NotCurrent, quotedBooking.GetCO2eStatus());
			AssertHasWarning(quotedBooking.TotalCO2eForBindingInfo, CO2eTestHelper.CO2eStaleWarning);
			CO2eTestHelper.AssertSTUEvent(quotedBooking, "Origin []->[AUSYD]");
		}

		[TestDate(2024, 1, 1)]
		public void TestUpdateCO2eStatusToNotCurrent_SailingJX()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);
			var sailing = Factory.NewWithValidTestData<JobSailing>();
			var jobVoyage = Factory.NewWithValidTestData<JobVoyage>();

			var origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_JV = jobVoyage.PK;

			var destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = "NZAKL";
			destination.JB_JV = jobVoyage.PK;

			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;

			quotedBooking.SetCO2eStatus(CO2eStatusList.Codes.Current);
			Factory.Save();

			((ISailingChooserParent)quotedBooking).SailingJX = sailing.PK;

			AssertEquals(CO2eStatusList.Codes.NotCurrent, quotedBooking.GetCO2eStatus());
			AssertHasWarning(quotedBooking.TotalCO2eForBindingInfo, CO2eTestHelper.CO2eStaleWarning);
			CO2eTestHelper.AssertSTUEvent(quotedBooking, $"JS_JX [{ZGuid.Empty}]->[{sailing.PK}]");

			TestDateAttribute.AddMinutes(1);
			quotedBooking.SetCO2eStatus(CO2eStatusList.Codes.Current);
			((ISailingChooserParent)quotedBooking).SailingJX = ZGuid.Empty;

			AssertEquals(CO2eStatusList.Codes.NotCurrent, quotedBooking.GetCO2eStatus());
			AssertHasWarning(quotedBooking.TotalCO2eForBindingInfo, CO2eTestHelper.CO2eStaleWarning);
			CO2eTestHelper.AssertSTUEvent(quotedBooking, $"JS_JX [{sailing.PK}]->[{ZGuid.Empty}]");
		}

		[TestDate(2024, 1, 1)]
		public void TestUpdateCO2eStatusToNotCurrent_SailingDetailChanged()
		{
			// Arrange
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);
			var voyage = Factory.New<JobVoyage>();

			var origin1 = voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "AUSYD";
			origin1.JA_E_DEP = ZDateTime.Today;
			var origin2 = voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "CNSHA";
			origin2.JA_E_DEP = ZDateTime.Today;

			var dest1 = voyage.Destinations.AddNew();
			dest1.JB_RL_NKPortOfDischarge = "NZAKL";
			dest1.JB_E_ARV = ZDateTime.Today.AddDays(5);
			var dest2 = voyage.Destinations.AddNew();
			dest2.JB_RL_NKPortOfDischarge = "USLAX";
			dest2.JB_E_ARV = ZDateTime.Today.AddDays(5);

			voyage.GenerateSailings();
			Factory.Save();

			var sailing1 = voyage.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "NZAKL");
			((ISailingChooserParent)quotedBooking).SailingJX = sailing1.PK;

			void AssertSailingCO2eStatusNotCurrent(Action change)
			{
				TestDateAttribute.AddMinutes(1);
				quotedBooking.SetCO2eStatus(CO2eStatusList.Codes.Current);
				sailing1.SetCO2eStatus(CO2eStatusList.Codes.Current);
				Factory.Save();

				change.Invoke();
				Factory.Save();
				AssertEquals(CO2eStatusList.Codes.NotCurrent, quotedBooking.GetCO2eStatus());
				AssertEquals(CO2eStatusList.Codes.NotCurrent, quotedBooking.Booking.Sailing.GetCO2eStatus());
				AssertHasWarning(quotedBooking.TotalCO2eForBindingInfo, CO2eTestHelper.CO2eStaleWarning);
				CO2eTestHelper.AssertSTUEvent(quotedBooking, "Sailing/Flight");
			}

			// Act & Assert
			CombineAssertions("Change linked sailing detail should update Booking CO2e status to NCU", () =>
			{
				AssertSailingCO2eStatusNotCurrent(() => origin1.JA_RL_NKPortOfLoading = "AUBNE");
				AssertSailingCO2eStatusNotCurrent(() => dest1.JB_RL_NKPortOfDischarge = "USNYC");
				AssertSailingCO2eStatusNotCurrent(() => voyage.JV_VoyageFlight = "111");
				AssertSailingCO2eStatusNotCurrent(() => voyage.JV_AircraftType = "AAA");
				AssertSailingCO2eStatusNotCurrent(() => voyage.JV_RV_NKVessel = "MARIA");
			});
		}

		[TestDate(2024, 1, 1)]
		public void TestUpdateCO2eStatusToNotCurrent_RequiredTemperatureControlChanged_Booking()
		{
			// Arrange
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(quote.PK, booking.PK);
			quotedBooking.Mode = RateMode.ULD;
			Assert(!(quotedBooking as ICO2eProvider).RequireTEU);

			var gp20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var re20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE");

			var container1 = quotedBooking.QuotedBookingContainers.AddNew();
			container1.JC_ContainerCount = 1;
			container1.JC_RC = gp20.PK;

			var packLine = booking.OuterPackLines.AddNew();
			packLine.JL_F3_NKPackType = "PLT";
			packLine.JL_PackageCount = 10;
			quotedBooking.SetCO2eStatus(CO2eStatusList.Codes.Current);
			AssertNoWarning(quotedBooking.TotalCO2eForBindingInfo, CO2eTestHelper.CO2eStaleWarning);
			Factory.Save();

			// Act & Assert
			AssertTemperatureWarning(quotedBooking, () => packLine.JL_RequiredTemperatureMaximum = 5, "JL_RequiredTemperatureMaximum [0]->[5]");
			AssertTemperatureWarning(quotedBooking, () => packLine.JL_RequiredTemperatureMaximum = 3);
			AssertTemperatureWarning(quotedBooking, () => packLine.JL_RequiredTemperatureMaximum = 0, "JL_RequiredTemperatureMaximum [3]->[0]");
			AssertTemperatureWarning(quotedBooking, () => packLine.JL_RequiredTemperatureMinimum = -2, "JL_RequiredTemperatureMinimum [0]->[-2]");
			AssertTemperatureWarning(quotedBooking, () => packLine.JL_RequiredTemperatureMinimum = -1);
			AssertTemperatureWarning(quotedBooking, () => packLine.JL_RequiredTemperatureMinimum = 0, "JL_RequiredTemperatureMinimum [-1]->[0]");
			AssertTemperatureWarning(quotedBooking, () =>
			{
				var packLine2 = Factory.New<ForwardingPackLine>();
				packLine2.JL_RequiredTemperatureMinimum = -1;
				packLine2.JL_FreightMode = FreightConstants.OuterPackType;
				booking.OuterPackLines.Add(packLine2);
			}, "Loose Cargo added");
			AssertTemperatureWarning(quotedBooking, () => booking.OuterPackLines[1].Delete(), "Loose Cargo removed");
			AssertTemperatureWarning(quotedBooking, () => container1.JC_RC = re20.PK, $"JC_RC [{gp20.PK}]->[{re20.PK}]");
			AssertTemperatureWarning(quotedBooking, () => container1.JC_RC = gp20.PK, $"JC_RC [{re20.PK}]->[{gp20.PK}]");
			AssertTemperatureWarning(quotedBooking, () => container1.Delete());

			var container2 = Factory.New<ForwardingContainer>();
			container2.JC_ContainerCount = 1;
			container2.JC_RC = re20.PK;
			AssertTemperatureWarning(quotedBooking, () => quotedBooking.QuotedBookingContainers.Add(container2), "Container added");
			AssertTemperatureWarning(quotedBooking, () => container2.Delete(), "Container removed");
		}

		[TestDate(2024, 1, 1)]
		public void TestUpdateCO2eStatusToNotCurrent_RequiredTemperatureControlChanged_OOQ()
		{
			// Arrange
			var ooq = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
			var containers = ooq.Quote.CurrentOneOffQuote.Containers;

			var gp20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var re20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE");

			var container = containers.AddNew();
			container.TC_RC = gp20.PK;
			AssertNoWarning(ooq.TotalCO2eForBindingInfo, CO2eTestHelper.CO2eStaleWarning);

			// Act & Assert
			AssertTemperatureWarning(ooq, () => container.TC_RC = re20.PK, $"TC_RC [{gp20.PK}]->[{re20.PK}]");
			AssertTemperatureWarning(ooq, () => container.TC_RC = gp20.PK, $"TC_RC [{re20.PK}]->[{gp20.PK}]");
			AssertTemperatureWarning(ooq, () =>
			{
				var container2 = Factory.New<RateOneOffContainers>();
				container2.TC_RC = re20.PK;
				containers.Add(container2);
			}, "Container added");
			AssertTemperatureWarning(ooq, () => containers.RemoveAndDeleteAll(), "Container removed");
		}

		void AssertTemperatureWarning(QuotedBooking co2eProvider, Action action, string reason = "")
		{
			TestDateAttribute.AddMinutes(1);
			co2eProvider.SetCO2eStatus(CO2eStatusList.Codes.Current);
			action();
			Factory.Save();
			if (!string.IsNullOrEmpty(reason))
			{
				AssertHasWarning(co2eProvider.TotalCO2eForBindingInfo, CO2eTestHelper.CO2eStaleWarning);
				CO2eTestHelper.AssertSTUEvent(co2eProvider, reason);
			}
			else
			{
				AssertNoWarning(co2eProvider.TotalCO2eForBindingInfo, CO2eTestHelper.CO2eStaleWarning);
				AssertEquals("Status stays CUR", co2eProvider.GetCO2eStatus(), CO2eStatusList.Codes.Current);
			}
		}

		public void TestUpdateCO2eDistanceInKM()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);

			quotedBooking.SetCO2eDistanceInKM(10_000m);

			AssertEquals(quotedBooking.GetCO2eDistanceInKM(), 10_000m);
		}

		public void TestSaveEmissionsLogToNoteOnCalculated()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory) as ICO2eCalculationSupporter;
			AssertEquals(false, quotedBooking.SaveEmissionsLogToNoteOnCalculated);
		}

		#endregion

		public void TestShouldSupportWorkflowTemplateApplication()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var workflowProvider = quotedBooking as ISometimesWorkflowProvider;

			Assert(!quotedBooking.IsConsolidated);
			Assert(
				"Should support template application when unconverted.",
				workflowProvider.ShouldSupportWorkflowTemplateApplication);

			quotedBooking.Booking.JS_IsCFSRegistered = true;

			Assert(!quotedBooking.IsConsolidated);
			Assert(
				"Should support template application when unconverted.",
				workflowProvider.ShouldSupportWorkflowTemplateApplication);

			quotedBooking.Booking.JS_IsForwardRegistered = true;
			Assert(quotedBooking.IsConsolidated);
			Assert(
				"Should not support template application when converted.",
				!workflowProvider.ShouldSupportWorkflowTemplateApplication);
		}

		#region ULD
		public void TestModeChangeOnBooking_LooseCargoContainerTypeWillBeReset()
		{
			var gp20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, booking.PK);
			quotedBooking.Mode = "ULD";

			ForwardingPackLine outerPL = booking.OuterPackLines.AddNew();
			outerPL.JL_F3_NKPackType = "PLT";
			outerPL.JL_PackageCount = 10;
			outerPL.JL_RC_ContainerType = gp20.PK;

			quotedBooking.Mode = "FCL";

			AssertEquals("Container Type on packline will be reset.", ZGuid.Empty, outerPL.JL_RC_ContainerType);
			AssertEquals("Other attributes on packline will not be changed.", "PLT", outerPL.JL_F3_NKPackType);
			AssertEquals("Other attributes on packline will not be changed.", 10, outerPL.JL_PackageCount);
		}

		public void TestConverOOQToBWQ_WillCopyContainerTypeInfoInPacklines_WhenModeIsULD()
		{
			var gp20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking qb = QuotedBooking.New(quote.PK, booking.PK, Factory);
			qb.Mode = "ULD";

			var looseCargo = quote.CurrentOneOffQuote.LooseCargo.AddNew();
			looseCargo.TPL_F3_NKPackType = Constants.PkgUnit.Keg;
			looseCargo.TPL_PackLineCount = 2;
			looseCargo.TPL_RC_RefContainer = gp20.PK;

			qb.CopyQuoteValuesToBooking();
			AssertEquals("pack lines", 1, booking.OuterPackLines.Count);
			AssertEquals("packType", Constants.PkgUnit.Keg, booking.OuterPackLines[0].JL_F3_NKPackType);
			AssertEquals("Container Type", gp20.PK, booking.OuterPackLines[0].JL_RC_ContainerType);
		}
		#endregion

		public void TestUniversalCopyIgnoreElement()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var componentType = quotedBooking.GetType();
			var ignoreElementAttributes = componentType.GetCustomAttributes(typeof(UniversalCopyIgnoreElementAttribute), true);
			var attribute = ignoreElementAttributes[0] as UniversalCopyIgnoreElementAttribute;
			AssertCollectionContains("Job", "Job", attribute.ElementNames);
		}

		#region CO2e Copying

		public void TestShouldNotUpdateCO2eStatusToNotCurrentWhenCopyingQuoteBooking()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);
			((IBusinessObjectInternals)quotedBooking).IsCopying = false;
			var carrier = Factory.LoadTop1<OrgHeader>(new ZQuery());
			quotedBooking.Mode = Core.Constants.TransportModes.Air;
			quotedBooking.Weight = 5m;
			quotedBooking.WeightUnit = Constants.Weight.Tonnes;
			quotedBooking.SetCO2ePerTonneInKg(100.1111111m);
			quotedBooking.SetCO2eStatus(CO2eStatusList.Codes.Current);
			quotedBooking.OH_Carrier = carrier.PK;
			AssertEquals(CO2eStatusList.Codes.NotCurrent, quotedBooking.GetCO2eStatus());

			var shipment2 = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking2 = CreateNewQuotedBooking(ZGuid.Empty, shipment2.PK);
			((IBusinessObjectInternals)quotedBooking2).IsCopying = true;
			var carrier2 = Factory.LoadTop1<OrgHeader>(new ZQuery());
			quotedBooking2.Mode = Core.Constants.TransportModes.Air;
			quotedBooking2.Weight = 5m;
			quotedBooking2.WeightUnit = Constants.Weight.Tonnes;
			quotedBooking2.SetCO2ePerTonneInKg(100.1111111m);
			quotedBooking2.SetCO2eStatus(CO2eStatusList.Codes.Current);
			quotedBooking2.OH_Carrier = carrier2.PK;
			AssertEquals(CO2eStatusList.Codes.Current, quotedBooking2.GetCO2eStatus());
		}

		public void TestCO2eStatusWhenReversingQuoteBooking()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);
			((IBusinessObjectInternals)quotedBooking).IsCopying = false;
			var carrier = Factory.LoadTop1<OrgHeader>(new ZQuery());
			quotedBooking.Mode = Core.Constants.TransportModes.Air;
			quotedBooking.Weight = 5m;
			quotedBooking.WeightUnit = Constants.Weight.Tonnes;
			quotedBooking.SetCO2ePerTonneInKg(100.1111111m);
			quotedBooking.SetCO2eStatus(CO2eStatusList.Codes.Current);
			quotedBooking.OH_Carrier = carrier.PK;
			quotedBooking.Origin = "AUSYD";
			quotedBooking.Destination = "CNSHA";
			AssertEquals(CO2eStatusList.Codes.NotCurrent, quotedBooking.GetCO2eStatus());

			((ITemplateReversible)quotedBooking).Reverse();
			AssertEquals("quotedBooking reversed", quotedBooking.Origin, "CNSHA");
			AssertEquals("quotedBooking reversed", quotedBooking.Destination, "AUSYD");
			AssertEquals("quotedBooking reversed", CO2eStatusList.Codes.NotCurrent, quotedBooking.GetCO2eStatus());

			var shipment2 = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking2 = CreateNewQuotedBooking(ZGuid.Empty, shipment2.PK);
			((IBusinessObjectInternals)quotedBooking2).IsCopying = true;
			var carrier2 = Factory.LoadTop1<OrgHeader>(new ZQuery());
			quotedBooking2.Mode = Core.Constants.TransportModes.Air;
			quotedBooking2.Weight = 5m;
			quotedBooking2.WeightUnit = Constants.Weight.Tonnes;
			quotedBooking2.SetCO2ePerTonneInKg(100.1111111m);
			quotedBooking2.SetCO2eStatus(CO2eStatusList.Codes.Current);
			quotedBooking2.OH_Carrier = carrier2.PK;
			AssertEquals(CO2eStatusList.Codes.Current, quotedBooking2.GetCO2eStatus());

			((ITemplateReversible)quotedBooking2).Reverse();
			AssertEquals("quotedBooking reversed", CO2eStatusList.Codes.Current, quotedBooking2.GetCO2eStatus());
		}

		#endregion

		public void TestClearingContractNumberClearsAllocationRoutes()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			quotedBooking.CarrierContractNumber = "SHREKTRACT";
			quotedBooking.AllocationLinePK = ZGuid.NewZGuid();

			var container = quotedBooking.QuotedBookingContainers.AddNew();
			container.JC_RCA_AllocationLine = ZGuid.NewZGuid();

			Assert(!quotedBooking.AllocationLinePK.IsEmpty);
			Assert(!container.JC_RCA_AllocationLine.IsEmpty);

			quotedBooking.CarrierContractNumber = string.Empty;

			Assert("Clearing contract number should have cleared route.", quotedBooking.AllocationLinePK.IsEmpty);
			Assert("Clearing contract number should have cleared container routes.", container.JC_RCA_AllocationLine.IsEmpty);
		}

		public void TestAllocationRoutePropagatesToContainers()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var container1 = quotedBooking.QuotedBookingContainers.AddNew();
			var container2 = quotedBooking.QuotedBookingContainers.AddNew();

			Assert(container1.JC_RCA_AllocationLine.IsEmpty);
			Assert(container2.JC_RCA_AllocationLine.IsEmpty);

			var routePK = ZGuid.NewZGuid();
			quotedBooking.AllocationLinePK = routePK;

			AssertEquals("Route should have propagated from booking to containers.", routePK, container1.JC_RCA_AllocationLine);
			AssertEquals("Route should have propagated from booking to containers.", routePK, container2.JC_RCA_AllocationLine);
		}

		public void TestSetAllocationRouteDefaultsPorts()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);

			var route = Factory.New<RatingContractAllocationLine>();
			route.RCA_LoadLocation = "AU";
			route.RCA_DischargeLocation = "NZ";

			quotedBooking.AllocationLinePK = route.PK;
			Assert("Setting route should only populate booking load port if route load location is UNLOCO.", quotedBooking.LoadPort.IsEmpty);
			Assert("Setting route should only populate booking disc port if route disc location is UNLOCO.", quotedBooking.DischargePort.IsEmpty);

			quotedBooking.AllocationLinePK = ZGuid.Empty;
			route.RCA_LoadLocation = "AUSYD";
			route.RCA_DischargeLocation = "NZAKL";

			quotedBooking.AllocationLinePK = route.PK;
			AssertEquals("Setting route should populate load port if empty.", "AUSYD", quotedBooking.LoadPort);
			AssertEquals("Setting route should populate disc port if empty.", "NZAKL", quotedBooking.DischargePort);

			quotedBooking.AllocationLinePK = ZGuid.Empty;
			route.RCA_LoadLocation = "USLAX";
			route.RCA_DischargeLocation = "CNSHA";

			quotedBooking.AllocationLinePK = route.PK;
			AssertEquals("Setting route should not populate load port if not empty.", "AUSYD", quotedBooking.LoadPort);
			AssertEquals("Setting route should not populate disc port if not empty.", "NZAKL", quotedBooking.DischargePort);
		}

		public void TestSetAllocationRouteDefaultsSchedule()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);

			var route = Factory.New<RatingContractAllocationLine>();
			var schedule = Factory.New<JobSailing>();
			var origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = "AUSYD";

			var destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = "NZAKL";

			schedule.JX_JA = origin.PK;
			schedule.JX_JB = destination.PK;

			route.RCA_JX_SailingSchedule = schedule.PK;

			AssertEquals("Prerequisite: No Sailling on Booking", true, quotedBooking.Booking.SailingPK.IsEmpty);

			quotedBooking.AllocationLinePK = route.PK;

			AssertEquals("Sailing is populated", quotedBooking.Booking.SailingPK, route.RCA_JX_SailingSchedule);
		}

		public void TestJS_IsDirectBookingShouldBeResetToFalseWhenTransportModeIsRoadOrRail()
		{
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, QuotedBooking.CreateNewBooking(Factory).PK);
			quotedBooking.Mode = RateMode.FCL;
			quotedBooking.Origin = "AUSYD";
			quotedBooking.Booking.JS_IsDirectBooking = true;

			Factory.Save();

			quotedBooking = new BusinessObjectFactory().Load<QuotedBooking>(quotedBooking.PK);
			Assert("quotedBooking is a direct booking!", quotedBooking.Booking.JS_IsDirectBooking);

			var modes = new List<string>
			{
				RateMode.LRO,
				RateMode.FRO,
				RateMode.FTL,
				RateMode.LRA,
				RateMode.FRA,
				RateMode.COU
			};
			foreach (var mode in modes)
			{
				quotedBooking.Mode = mode;
				Assert("quotedBooking is not a direct booking!", !quotedBooking.Booking.JS_IsDirectBooking);
			}
		}

		public void TestTariffIDIsClearedWhenCommodityCodeChanged()
		{
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, QuotedBooking.CreateNewBooking(Factory).PK);
			quotedBooking.Commodity = "GEN";
			quotedBooking.FMCTariffID = "111";
			AssertEquals("111", quotedBooking.FMCTariffID.ToString());

			quotedBooking.Commodity = "HAZ";

			AssertEquals("", quotedBooking.FMCTariffID.ToString());

			quotedBooking.FMCTariffID = "222";
			AssertEquals("222", quotedBooking.FMCTariffID.ToString());

			quotedBooking.Commodity = "";

			AssertEquals("", quotedBooking.FMCTariffID.ToString());
		}

		public void TestIConfirmAddressParent_ReturnsCorrectAddresses()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var addressParent = (IConfirmAddressParent)quotedBooking;

			quotedBooking.Booking.ConsignorPickupAddress.Address1 = "Pickup";
			quotedBooking.Booking.ConsigneeDeliveryAddress.Address1 = "Delivery";
			quotedBooking.Booking.GetArrivalCFSDocAddress.Address1 = "Arrival CFS";
			quotedBooking.Booking.GetArrivalCTODocAddress.Address1 = "Arrival CTO";
			quotedBooking.Booking.GetDepartureCFSDocAddress.Address1 = "Departure CFS";
			quotedBooking.Booking.GetDepartureCTODocAddress.Address1 = "Departure CTO";
			quotedBooking.Booking.GetDepartureContainerYardDocAddress.Address1 = "Departure Container Yard";
			quotedBooking.Booking.GetArrivalContainerYardDocAddress.Address1 = "Arrival Container Yard";

			CombineAssertions(() =>
			{
				AssertEquals("Consignor Pickup Address should match", "Pickup", addressParent.GetConsignorPickupDocAddress.Address1);
				AssertEquals("Consignee Delivery Address should match", "Delivery", addressParent.GetConsigneeDeliveryDocAddress.Address1);
				AssertEquals("Arrival CFS Address should match", "Arrival CFS", addressParent.GetArrivalCFSDocAddress.Address1);
				AssertEquals("Arrival CTO Address should match", "Arrival CTO", addressParent.GetArrivalCTODocAddress.Address1);
				AssertEquals("Departure CFS Address should match", "Departure CFS", addressParent.GetDepartureCFSDocAddress.Address1);
				AssertEquals("Departure CTO Address should match", "Departure CTO", addressParent.GetDepartureCTODocAddress.Address1);
				AssertEquals("Departure Container Yard Address should match", "Departure Container Yard", addressParent.GetDepartureContainerYardDocAddress.Address1);
				AssertEquals("Arrival Container Yard Address should match", "Arrival Container Yard", addressParent.GetArrivalContainerYardDocAddress.Address1);
			});
		}

		public void TestGetIsSettingDefaultValues()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			quotedBooking.IsSettingDefaultValues = true;

			quotedBooking.Booking.IsSettingDefaultValues = true;
			Assert(quotedBooking.IsSettingDefaultValues);

			quotedBooking.Booking.IsSettingDefaultValues = false;
			Assert(quotedBooking.IsSettingDefaultValues);

			quotedBooking.IsSettingDefaultValues = false;

			quotedBooking.Booking.IsSettingDefaultValues = true;
			Assert(quotedBooking.IsSettingDefaultValues);

			quotedBooking.Booking.IsSettingDefaultValues = false;
			Assert(!quotedBooking.IsSettingDefaultValues);
		}

		public void TestITriggerActionProviderMembers()
		{
			var testQuotedBooking = (QuotedBooking)GetNewBusinessObject();
			Factory.Save();

			var triggerActionProvider = testQuotedBooking as ITriggerActionProvider;
			AssertNotNull(triggerActionProvider);
			AssertEquals("Stand alone invoice header.", ZString.Empty, triggerActionProvider.ReasonForDoNotTriggerAction);

			testQuotedBooking.Booking.JS_IsForwardRegistered = true;
			Factory.Save();

			AssertEquals("Quoted booking has been converted to Shipment", "Booking with Quote - Quote (00001000) - Booking (S00001000) has been converted to Shipment# S00001000.", triggerActionProvider.ReasonForDoNotTriggerAction);
		}

		#region Implementation

		CalculateDeliveryDueDateTransportModeCollection ActiveTransportModesForCalculateDeliveryDateOption()
		{
			var activeTransportModes = new CalculateDeliveryDueDateTransportModeCollection();
			activeTransportModes.Add(Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Road, Core.Constants.TransportModeDescriptions.Road, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Rail, Core.Constants.TransportModeDescriptions.Rail, true);
			return activeTransportModes;
		}

		void DisposeJobs(QuotedBooking quotedBooking)
		{
			if (quotedBooking != null && quotedBooking.Job != null)
			{
				quotedBooking.Job.Dispose();
			}
		}

		protected virtual bool IsLoadDischargeDefaultedFromOriginDestination
		{
			get { return true; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			currentTestQuotedBooking = CreateNewQuotedBooking(quote.PK, booking.PK);
			return currentTestQuotedBooking;
		}

		protected virtual QuotedBooking CreateNewQuotedBooking(ZGuid quotePK, ZGuid bookingPK)
		{
			return QuotedBooking.New(quotePK, bookingPK, Factory);
		}

		QuotedBooking GetQuoteOnlyQuotedBooking()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			return CreateNewQuotedBooking(quote.PK, ZGuid.Empty);
		}

		protected override void TearDown()
		{
			DisposeCurrentTestQuotedBookingJobHeaderMutexes();
			base.TearDown();
		}

		void DisposeCurrentTestQuotedBookingJobHeaderMutexes()
		{
			DisposeJobs(currentTestQuotedBooking);
		}

		QuotedBooking currentTestQuotedBooking;

		GlbDeptCharges AddNewDepartmentCharge(GlbDepartment department, AccChargeCode chargeCode)
		{
			GlbDeptCharges deptCharge = department.DeptCharges.AddNew();
			deptCharge.GD_AC = chargeCode.PK;
			deptCharge.GD_GC = GlbCompany.CurrentCompany.PK;
			deptCharge.GD_GE = department.PK;

			return deptCharge;
		}

		#endregion

		protected void SetUpQuotedBooking(QuotedBooking quotedBooking, ZString vesselName, ZString screeningStatus)
		{
			var sailing = Factory.NewWithValidTestData<JobSailing>();
			var jobVoyage = Factory.NewWithValidTestData<JobVoyage>();

			var origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = "UAIEV";
			origin.JA_JV = jobVoyage.PK;

			var destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = "CLESR";
			destination.JB_JV = jobVoyage.PK;

			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			((ISailingChooserParent)quotedBooking).SailingJX = sailing.PK;

			jobVoyage.JV_RV_NKVessel = vesselName;
			quotedBooking.Booking.JS_BookedVesselScreeningStatus = screeningStatus;
		}
	}
}
