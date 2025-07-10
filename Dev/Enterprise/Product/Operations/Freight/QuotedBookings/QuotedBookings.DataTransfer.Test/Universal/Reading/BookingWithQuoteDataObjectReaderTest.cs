using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.DataTransfer.Testing;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal.Test
{
	public class BookingWithQuoteDataObjectReaderTest : ShipmentDataObjectReadingHelperTest
	{
		public void TestShipmentDetails()
		{
			var quotedBooking = GetQuotedBooking();
			AssertContents(quotedBooking);
		}

		public void TestQuoteDetails()
		{
			shipmentDataObject.AdditionalTerms = "additional terms";
			shipmentDataObject.CompanyTariffLevelOverride = (ZByte)5;
			shipmentDataObject.Frequency = 3;
			shipmentDataObject.FrequencyUnit = new CodeDescriptionPair { Code = "FORTNIGHT", Description = "X per fortnight" };
			shipmentDataObject.QuoteNumberOfEntries = 3;
			shipmentDataObject.QuoteNumberOfEntryLines = 4;
			shipmentDataObject.ServiceLevel = new ServiceLevel { Code = "EXP", Description = "Express" };
			shipmentDataObject.ShipmentIncoTerm = new UniversalDataBuss.DataObjects.Universal.IncoTerm()
			{
				Code = "C3P",
				Description = "Collect 3rd Party"
			};
			shipmentDataObject.TransitTime = new CodeDescriptionPair { Code = "3", Description = "3 days" };

			var quotedBooking = GetQuotedBooking();
			CombineAssertions(() =>
			{
				AssertEquals("AdditionalTerms", "additional terms", quotedBooking.AdditionalTerms);
				AssertEquals("CompanyTariffLevelOverride", (ZByte)5, quotedBooking.Quote.CurrentOneOffQuote.TT_CompanyTariffLevelOverride);
				AssertEquals("Frequency", 3, quotedBooking.Frequency);
				AssertEquals("FrequencyUnit", "FORTNIGHT", quotedBooking.FrequencyUnit);
				AssertEquals("QuoteNumberOfEntries", (short)3, quotedBooking.QuoteNumberOfEntries);
				AssertEquals("QuoteNumberOfEntryLines", (short)4, quotedBooking.QuoteNumberOfEntryLines);
				AssertEquals("PaymentTerms", "C3P", quotedBooking.PaymentTerms);
				AssertEquals("Quote should never be improved on import.", false, quotedBooking.Quote.IsApproved);
				AssertEquals("ServiceLevel", "EXP", quotedBooking.ServiceLevel);
				AssertEquals("TransitTime", "3", quotedBooking.TransitTime);
			});
		}

		public void TestQuoteDetails_WhenServiceLevelIsNull_NoExceptionThrown()
		{
			shipmentDataObject.ServiceLevel = null;

			var quotedBooking = GetQuotedBooking();

			AssertEquals("ServiceLevel has not been modified", "STD", quotedBooking.ServiceLevel);
		}

		public void TestCarrier()
		{
			var carrier = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			carrier.OH_IsShippingProvider = true;

			var address = GetNewAddressData_INTHEMSYD(DocAddressType.ShippingLineAddress);
			var addressBO = new OrganisationDataObjectReader(address, logger, Factory).GetMatchedOrNewForTesting();
			AssertNotNull("Address", addressBO);

			Factory.SaveForTesting();

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			shipmentDataObject.OrganizationAddressCollection.Add(address);

			var quotedBooking = GetQuotedBooking();
			AssertEquals(carrier, quotedBooking.Carrier);
		}

		public void TestCreditor()
		{
			var creditor = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			creditor.OH_IsCreditor = true;

			var address = GetNewAddressData_INTHEMSYD(DocAddressType.Creditor);
			var addressBO = new OrganisationDataObjectReader(address, logger, Factory).GetMatchedOrNewForTesting();
			AssertNotNull("Address", addressBO);

			Factory.SaveForTesting();

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			shipmentDataObject.OrganizationAddressCollection.Add(address);

			var quotedBooking = GetQuotedBooking();
			AssertEquals(creditor.PK, quotedBooking.Creditor);
		}

		public void TestCustomFields()
		{
			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = TriggerLineTypes.Codes.QuotedBooking;
			processTaskTemplate.P0_IsActive = true;
			processTaskTemplate.P0_SubType2 = QuotedBooking.BookingWithQuoteCode;

			var customFieldStr = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customFieldStr.XC_Name = "Workflow Custom Field";
			customFieldStr.XC_Type = "STR";

			var customFieldBool = Factory.New<GenCustomColumnDefinition>();
			customFieldBool.XC_Name = "Workflow Flag This!";
			customFieldBool.XC_Type = "BOO";

			Factory.SaveForTesting();

			shipmentDataObject.SetCustomizedFieldCollection(() => new List<CustomizedField>());
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Workflow Custom String Field", new ZString("HELLO")));
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Custom Date", new ZDateTime(2011, 1, 1)));
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Custom Bool", ZBool.True));
			var incorrectCustomField = new CustomizedField { Key = "Workflow Flag This!", Value = new ZString("I am NOT a BOOLEAN!"), DataType = DataType.Boolean };
			shipmentDataObject.CustomizedFieldCollection.Add(incorrectCustomField);

			var quotedBooking = GetQuotedBooking();

			CombineAssertions(delegate
			{
				var customFields = quotedBooking.GetUserDefinedValues();
				var customFieldsString = customFields.Select(f => f.PropertyName + " - " + f.Value).ToList();

				Assert("Workflow Custom String Field", customFieldsString.Contains("Workflow Custom String Field - HELLO"));
				Assert("Custom Date", customFieldsString.Contains("Custom Date - 01-Jan-11 00:00:00"));
				Assert("Custom Bool", customFieldsString.Contains("Custom Bool - Y"));

				AssertContainsExactLinesInExactOrder("Logger.Logs", @"
Information - No matching QuotedBooking found, creating new QuotedBooking.
Information - Populating QuotedBooking...
Warning - Custom Fields - Invalid value [I am NOT a BOOLEAN!]. Value must be a valid Boolean (true or false).
Information - Added Booking with Quote from UniversalShipment.
Information - Successfully saved Booking with Quote - Quote (00001000) - Booking (S00001000).
".Trim(), Logger.Logs);
			});
		}

		public void TestDates()
		{
			var startDate = new ZDate(2022, 10, 11);
			var endDate = new ZDate(2022, 10, 21);

			shipmentDataObject.SetDateCollection(() => new List<Date>());
			shipmentDataObject.DateCollection.Add(DateType.Start, false, startDate);
			shipmentDataObject.DateCollection.Add(DateType.End, false, endDate);
			shipmentDataObject.DateCollection.Add(DateType.Accepted, false, startDate.AddDays(1));
			shipmentDataObject.DateCollection.Add(DateType.ClientAccepted, false, startDate.AddDays(2));
			shipmentDataObject.DateCollection.Add(DateType.FollowUp, false, startDate.AddDays(3));

			var quotedBooking = GetQuotedBooking();

			AssertEquals("StartDate", startDate, quotedBooking.StartDate);
			AssertEquals("EndDate", endDate, quotedBooking.EndDate);
			AssertEquals("Accepted", startDate.AddDays(1), quotedBooking.Quote.TH_Accepted);
			AssertEquals("ClientAccepted", startDate.AddDays(2), quotedBooking.Quote.TH_ClientAccepted);
			AssertEquals("FollowUp", startDate.AddDays(3), quotedBooking.Quote.TH_FollowUpDate);
		}

		public void TestRoundingRoundDownDecimalToItsPrecisionAndScaleOnDataImport()
		{
			var collection = new DefaultNumberOfDecimalsCollection(Enterprise.Registry.Business.Module.Freight);
			var registryEntryWeight = new DefaultNumberOfDecimals();
			registryEntryWeight.UnitOfMeasure = Core.Constants.Weight.Kilograms;
			registryEntryWeight.TransportMode = Core.Constants.TransportModes.Sea;
			registryEntryWeight.NumberOfDecimals = 1;
			registryEntryWeight.RoundingMode = RoundingModes.Up;
			collection.Add(registryEntryWeight);

			var registryEntryVolume = new DefaultNumberOfDecimals();
			registryEntryVolume.UnitOfMeasure = Core.Constants.Volume.CubicMetres;
			registryEntryVolume.TransportMode = Core.Constants.TransportModes.Sea;
			registryEntryVolume.NumberOfDecimals = 1;
			registryEntryVolume.RoundingMode = RoundingModes.Up;
			collection.Add(registryEntryVolume);

			var registryEntryDimension = new DefaultNumberOfDecimals();
			registryEntryDimension.UnitOfMeasure = Core.Constants.Dimension.Metres;
			registryEntryDimension.TransportMode = Core.Constants.TransportModes.Sea;
			registryEntryDimension.NumberOfDecimals = 1;
			registryEntryDimension.RoundingMode = RoundingModes.Up;
			collection.Add(registryEntryDimension);

			var dataObject = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.Commodity = new Commodity() { Code = "GEN" };
			dataObject.DetailedDescription = "SOARE PARTS INV:";
			dataObject.GoodsDescription = "SPARE PARTS INV:";
			dataObject.MarksAndNos = "NGN:D2310789PR, REF:Antonio";
			dataObject.PackQty = 8;
			dataObject.PackType = new PackageType() { Code = "BOX" };
			dataObject.Volume = 999999.999m;
			dataObject.Weight = 999999.999m;
			dataObject.Length = 999999.999m;
			dataObject.Height = 999999.999m;
			dataObject.Width = 999999.999m;
			dataObject.OutturnedLength = 999999.999m;
			dataObject.OutturnedWidth = 999999.999m;
			dataObject.OutturnedHeight = 999999.999m;
			dataObject.OutturnedVolume = 999999.999m;
			dataObject.OutturnedWeight = 999999.999m;
			dataObject.WeightUnit = new UnitOfWeight { Code = Core.Constants.Weight.Kilograms };
			dataObject.VolumeUnit = new UnitOfVolume { Code = Core.Constants.Volume.CubicMetres };
			dataObject.LengthUnit = new UnitOfLength { Code = Core.Constants.Dimension.Metres };

			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { dataObject });

			using (FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var reader = new BookingWithQuoteDataObjectReader(shipmentDataObject, logger, Factory);
				var quotedBookingBizo = reader.ReadIntoBusinessObject();
				AssertEquals((ZDecimal)999999.9, quotedBookingBizo.Quote.CurrentOneOffQuote.LooseCargo[0].TPL_Weight);
				AssertEquals((ZDecimal)999999.9, quotedBookingBizo.Quote.CurrentOneOffQuote.LooseCargo[0].TPL_Volume);
				AssertEquals((ZDecimal)999999.9, quotedBookingBizo.Quote.CurrentOneOffQuote.TT_ActualWeight);
				AssertEquals((ZDecimal)999999.9, quotedBookingBizo.Quote.CurrentOneOffQuote.TT_ActualVolume);
				AssertEquals((ZDecimal)123.5, quotedBookingBizo.Chargeable);

				AssertContainsExactLinesInExactOrder("logger.Logs", @"
Information - No matching QuotedBooking found, creating new QuotedBooking.
Information - Populating QuotedBooking...
Information - No matching ForwardingPackLine found, creating new ForwardingPackLine.
Information - Populating ForwardingPackLine...
Warning - Attempted to insert '7999999976000000000' into Field [TT_ActualVolume] in calculating total. Calculated total has been ignored.
Warning - Attempted to insert '7999999976000000000' into Field [TT_ActualVolume] in calculating total. Calculated total has been ignored.
Information - Added Booking with Quote from UniversalShipment.
".Trim(), logger.Logs);
			}
		}
		public void TestQuoteCharges()
		{
			using (Factory.BOFactory.AddDisposableService())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var localCurrency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency;
				shipmentDataObject.DataContext.CodesMappedToTarget = true;
				shipmentDataObject.JobCosting = new JobCosting(DefaultDataObjectWriterStrategy.TestInstance);
				shipmentDataObject.JobCosting.Branch = new Branch();
				shipmentDataObject.JobCosting.Branch.Code = "SYD";
				shipmentDataObject.JobCosting.SetChargeLineCollection(() => new List<ChargeLine>());

				var chargeLineFRT = OneOffQuoteDataObjectReaderTest.GetChargeLine("SYD", "FRT", "001", ZDate.Today, null,
					ZDate.Today, 100.00m, 100.00m, localCurrency, null, "AALSHI", "ABIGAS",
					"FIS", "Charge Description 1", 1, null, "FIN", 100.00m, 100.00m, localCurrency, null);
				chargeLineFRT.CostInvoiceDate = ZDate.Today;
				chargeLineFRT.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Instruction = InstructionType.Insert
				};
				shipmentDataObject.JobCosting.ChargeLineCollection.Add(chargeLineFRT);

				var chargeLineBAF = OneOffQuoteDataObjectReaderTest.GetChargeLine("SYD", "BAF", "001", ZDate.Today, null,
					ZDate.Today, 200.00m, 200.00m, localCurrency, null, "AALSHI", "ABIGAS",
					"FIS", "Charge Description 1", 1, null, "FIN", 250.00m, 250.00m, localCurrency, null);
				chargeLineBAF.CostInvoiceDate = ZDate.Today;
				chargeLineBAF.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Instruction = InstructionType.Insert
				};
				shipmentDataObject.JobCosting.ChargeLineCollection.Add(chargeLineBAF);

				var quotedBookingBO = GetQuotedBooking();
				AssertNotNull(quotedBookingBO);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching QuotedBooking found, creating new QuotedBooking.
Information - Populating QuotedBooking...
Warning - Whilst importing Charge Line: Job Number= Charge Code=FRT Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=100.00 Sell OS Amount=100.00
Warning - Override Comment: You have not entered an Override Comment.
Warning - Description: Charge description was changed from default. This description will appear on AR Invoice without translation.
Warning - Debtor: You have selected a Debtor that is neither your Local Client or your Overseas Agent.
Whilst invoicing any party is valid, this should be confirmed.
Warning - Whilst importing Charge Line: Job Number= Charge Code=BAF Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=200.00 Sell OS Amount=250.00
Warning - Override Comment: You have not entered an Override Comment.
Warning - Description: Charge description was changed from default. This description will appear on AR Invoice without translation.
Warning - Debtor: You have selected a Debtor that is neither your Local Client or your Overseas Agent.
Whilst invoicing any party is valid, this should be confirmed.
Warning - Revenue Override Comment: You have not entered a Revenue Override Comment.
Information - Added Booking with Quote from UniversalShipment.
Information - Successfully saved Booking with Quote - Quote (00001000) - Booking (S00001000).
".Trim(), Logger.Logs);

				using (var job = quotedBookingBO.Job)
				{
					var charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
					AssertEquals("charges.Length", 2, charges.Length);

					var frtCharge = charges.Single(x => x.ChargeCode.AC_Code == "FRT");
					var bafCharge = charges.Single(x => x.ChargeCode.AC_Code == "BAF");

					OneOffQuoteDataObjectReaderTest.AssertCharge(frtCharge, chargeLineFRT);
					OneOffQuoteDataObjectReaderTest.AssertCharge(bafCharge, chargeLineBAF);
				}
			}
		}

		public void TestQuoteStatistics()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("EEE", "DesEEE");

			using (DataRegistryRating.Instance.OneOffQuoteKPISettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			using (DataRegistryRating.Instance.OneOffQuoteSourceSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			using (DataRegistryRating.Instance.OneOffQuoteRevisionReasonSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			{
				shipmentDataObject.QuoteKPI = new CodeDescriptionPair()
				{
					Code = "EEE",
					Description = "DesEEE"
				};

				shipmentDataObject.QuoteSource = new CodeDescriptionPair()
				{
					Code = "EEE",
					Description = "DesEEE"
				};

				shipmentDataObject.QuoteRevisionReason = new CodeDescriptionPair()
				{
					Code = "EEE",
					Description = "DesEEE"
				};

				var quotedBooking = GetQuotedBooking();

				AssertEquals("Quote Source", "EEE", quotedBooking.OneOffQuoteStatistics.OneOffQuoteSource);
				AssertEquals("Quote KPI", "", quotedBooking.OneOffQuoteStatistics.OneOffQuoteKPI);
				AssertEquals("Quote Revision Reason", "", quotedBooking.OneOffQuoteStatistics.OneOffQuoteRevisionReason);
			}
		}

		public void TestLocations()
		{
			shipmentDataObject.PortOfOrigin = new UNLOCO { Code = "AUMEL" };
			shipmentDataObject.PortOfDestination = new UNLOCO { Code = "AUSYD" };
			shipmentDataObject.PortFirstForeign = new UNLOCO { Code = "AUADL" };

			var quotedBooking = GetQuotedBooking();

			AssertEquals("PortOfOrigin", "AUMEL", quotedBooking.Quote.CurrentOneOffQuote.ReceivalLocation.Code);
			AssertEquals("PortOfDestination", "AUSYD", quotedBooking.Quote.CurrentOneOffQuote.DeliveryLocation.Code);
			AssertEquals("PortFirstForeign", "AUADL", quotedBooking.Quote.CurrentOneOffQuote.ViaLocation.Code);
		}

		public void TestNotes()
		{
			var noteDataObject = SetupNote();
			noteDataObject.IsCustomDescription = false;

			shipmentDataObject.SetNoteCollection(() => new DataObjectList<Note>());
			shipmentDataObject.NoteCollection.Add(noteDataObject);

			var quotedBooking = GetQuotedBooking();

			StmNote[] note = quotedBooking.Notes.FindByDescription("DOG FLOGGER!!");
			AssertEquals("note.Length", 1, note.Length);

			CombineAssertions(() =>
			{
				AssertNoteContents(note[0]);
				AssertEquals("noteBO.ST_IsCustomDescription", true, note[0].ST_IsCustomDescription);
				AssertContainsExactLinesInExactOrder("Logger.Logs", @"
Information - No matching QuotedBooking found, creating new QuotedBooking.
Information - Populating QuotedBooking...
Information - No matching StmNote found, creating new StmNote.
Information - Populating StmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Added Booking with Quote from UniversalShipment.
Information - Successfully saved Booking with Quote - Quote (00001000) - Booking (S00001000) with 1 x StmNote.
".Trim(), Logger.Logs);
			});
		}

		public void TestBWQwithQuoteNumber()
		{
			var noteDataObject = SetupNote();
			noteDataObject.IsCustomDescription = false;

			shipmentDataObject.SetNoteCollection(() => new DataObjectList<Note>());
			shipmentDataObject.NoteCollection.Add(noteDataObject);
			shipmentDataObject.QuoteNumber = "00001000";

			var quotedBooking = GetQuotedBooking();

			StmNote[] note = quotedBooking.Notes.FindByDescription("DOG FLOGGER!!");
			AssertEquals("note.Length", 1, note.Length);

			CombineAssertions(() =>
			{
				AssertNoteContents(note[0]);
				AssertEquals("noteBO.ST_IsCustomDescription", true, note[0].ST_IsCustomDescription);
				AssertContainsExactLinesInExactOrder("Logger.Logs", @"
Information - No matching QuotedBooking found, creating new QuotedBooking.
Information - Populating QuotedBooking...
Information - No matching StmNote found, creating new StmNote.
Information - Populating StmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Added Booking with Quote from UniversalShipment.
Information - Successfully saved Booking with Quote - Quote (00001000) - Booking (S00001000) with 1 x StmNote.
".Trim(), Logger.Logs);
			});
		}

		public void TestCatchNullReferenceExceptionAndWriteDataIntoException()
		{
			var expectedReturnKey = "QuotedBookingDebuggingInfo";
			var exceptionHasBeenSeen = false;
			shipmentDataObject.QuoteNumber = "00001000";
			try
			{
				using (QuotedBookingWrapper.OverrideQuotedBookingWithCustomData())
				{
					var quotedBooking = GetQuotedBooking();
				}
			}
			catch (NullReferenceException ex)
			{
				exceptionHasBeenSeen = true;
				var keysList = ex.Data.Keys.ToList<string>();
				var doesContainKey = keysList.Contains(expectedReturnKey);
				AssertEquals(true, doesContainKey);
			}
			AssertEquals(true, exceptionHasBeenSeen);
		}

		public void TestApprovedBookingIsntApprovedOnImport()
		{
			shipmentDataObject.IsQuoteApprovedByManager = true;
			var quotedBooking = GetQuotedBooking();

			AssertEquals("ObjectState", QuotedBookingState.UnacceptedBookingWithQuote, quotedBooking.ObjectState);
			AssertEquals("IsApproved", expected: false, quotedBooking.Quote.IsApproved);
		}

		public void TestNewBusinessObject()
		{
			var reader = new BookingWithQuoteDataObjectReader(shipmentDataObject, logger, Factory);
			var quotedBookingBizo = reader.ReadIntoBusinessObject();
			AssertType<QuotedBooking>(quotedBookingBizo);
			AssertEquals("ObjectState", QuotedBookingState.UnacceptedBookingWithQuote, quotedBookingBizo.ObjectState);
		}

		public void TestCantReuseQuoteNumber()
		{
			var existingBooking = QuotedBooking.CreateNewQuote(Factory.BOFactory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			Factory.SaveForTesting();

			shipmentDataObject.QuoteNumber = existingBooking.TH_QuoteNumber;
			GetQuotedBooking();

			AssertContains("Provided Quote Number is already in use by an existing quote.", Logger.Logs);
		}

		public void TestDontSetBlankQuoteNumber()
		{
			shipmentDataObject.QuoteNumber = string.Empty;
			var quotedBooking = GetQuotedBooking();

			AssertNotEquals(string.Empty, quotedBooking.Quote.TH_QuoteNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();
			shipmentDataObject.ContainerMode = new ContainerMode() { Code = "FCL", Description = "Full Container Load" };
			shipmentDataObject.AdditionalTerms = "Additional Terms";
			shipmentDataObject.WayBillNumber = "MYHOUSE";
			shipmentDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };
		}

		static void AssertContents(QuotedBooking quotedBooking)
		{
			var shipment = quotedBooking.Booking;

			AssertEquals("JS_AdditionalTerms", "Additional Terms", shipment.JS_AdditionalTerms);

			AssertEquals("JS_TransportMode", "SEA", shipment.JS_TransportMode);
			AssertEquals("JS_PackingMode", "FCL", shipment.JS_PackingMode);

			AssertEquals("JS_AWBServiceLevel", "ELG", shipment.JS_AWBServiceLevel);
			AssertEquals("JS_BookingReference", "BOOK ME", shipment.JS_BookingReference);
			AssertEquals("JS_CFSReference", "CFS Book Ref", shipment.JS_CFSReference);

			AssertEquals("JS_UnitFreightRate", 67.89m, shipment.JS_UnitFreightRate);
			AssertEquals("JS_RX_NKFrtRateCurrency", Enterprise.Core.Constants.CurrencyCodes.CzechRepublic, shipment.JS_RX_NKFrtRateCurrency);
			AssertEquals("JS_GoodsDescription", "RAT HATS", shipment.JS_GoodsDescription);
			AssertEquals("JS_GoodsValue", 5.67m, shipment.JS_GoodsValue);
			AssertEquals("JS_RX_NKGoodsValueCurr", Enterprise.Core.Constants.CurrencyCodes.Ghana, shipment.JS_RX_NKGoodsValueCurr);
			AssertEquals("JS_InsuranceValue", 6.78m, shipment.JS_InsuranceValue);
			AssertEquals("JS_RX_NKInsuranceCurrency", Enterprise.Core.Constants.CurrencyCodes.Kenya, shipment.JS_RX_NKInsuranceCurrency);
			AssertEquals("JS_InterimReceipt", "IR Text", shipment.JS_InterimReceipt);

			AssertEquals("JS_IsDirectBooking", false, shipment.JS_IsDirectBooking);
			AssertEquals("JS_IsForwardRegistered", false, shipment.JS_IsForwardRegistered);

			AssertEquals("JS_OuterPacks", 44, shipment.JS_OuterPacks);
			AssertEquals("JS_F3_NKPackType", "VF", shipment.JS_F3_NKPackType);

			AssertEquals("JS_PackingOrder", 1, shipment.JS_PackingOrder);
			AssertEquals("JS_RS_NKServiceLevel", "PFT", shipment.JS_RS_NKServiceLevel);
			AssertEquals("JS_INCO", "CIF", shipment.JS_INCO);

			AssertEquals("JS_ActualVolume", 23.45m, shipment.JS_ActualVolume);
			AssertEquals("JS_UnitOfVolume", "CF", shipment.JS_UnitOfVolume);
			AssertEquals("JS_ActualWeight", 34.56m, shipment.JS_ActualWeight);
			AssertEquals("JS_UnitOfWeight", "KT", shipment.JS_UnitOfWeight);

			AssertEquals("JS_RL_NKOrigin", "NZDUD", shipment.JS_RL_NKOrigin);
			AssertEquals("JS_RL_NKLoadPort", "NZCHC", shipment.JS_RL_NKLoadPort);
			AssertEquals("JS_RL_NKDischargePort", "AUSYD", shipment.JS_RL_NKDischargePort);
			AssertEquals("JS_RL_NKDestination", "AUBDG", shipment.JS_RL_NKDestination);

			AssertEquals("JS_ActualChargeable", 123.45m, shipment.JS_ActualChargeable);
			AssertEquals("JS_HouseBill", "MYHOUSE", shipment.JS_HouseBill);
		}

		QuotedBooking GetQuotedBooking()
		{
			var reader = new BookingWithQuoteDataObjectReader(shipmentDataObject, Logger, Factory);
			var quotedBooking = reader.ReadIntoBusinessObject();
			Factory.SaveAtEndOfImport(Logger);

			return quotedBooking;
		}

		class QuotedBookingWrapper : QuotedBooking
		{
			QuotedBookingWrapper(ZGuid quotePK, ZGuid bookingPK, bool attemptToLoadFromOther, BusinessObjectFactory factory)
				: base(quotePK, bookingPK, attemptToLoadFromOther, factory)
			{
			}

			public static IDisposable OverrideQuotedBookingWithCustomData()
			{
				OverridableNewDelegate.Value = (quotePK, bookingPK, attemptToLoadFromOther, factoryToWrap) => new QuotedBookingWrapper(ZGuid.Empty, bookingPK, attemptToLoadFromOther, factoryToWrap);
				return new DisposableAction(() => OverridableNewDelegate.Value = null);
			}
		}
	}
}
