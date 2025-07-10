using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.Testing;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Freight.QuotedBookings.Business.QuotedBookingToShipmentConverter;
using static Enterprise.Integration.Customs;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	public class QuotedBookingToShipmentConverterTest : TestCaseWithFactory
	{
		#region Constructor

		public void TestConstructor_RequiredArguments()
		{
			var booking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			var conversionSource = BookingToShipmentConversionSource.Form;
			AssertExceptionThrown(typeof(ArgumentNullException), () => new QuotedBookingToShipmentConverter(null, conversionSource));
			AssertNoExceptionThrown(() => new QuotedBookingToShipmentConverter(booking, conversionSource));
		}

		#endregion

		#region ConvertBookingToShipment

		public void TestConvertBookingToShipment_Successful_IfHasAnyErrorsCalledFirst()
		{
			var booking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			var conversionSource = BookingToShipmentConversionSource.Form;
			AssertEquals("Pre-Condition - The booking has not been converted", false, booking.Booking.JS_IsForwardRegistered);
			var converter = new QuotedBookingToShipmentConverter(booking, conversionSource);
			converter.HasAnyErrors(out string _);
			converter.ConvertBookingToShipment(booking.Booking);
			var shipment = Factory.Load<ForwardingShipment>(booking.Booking.PK);
			AssertEquals("The booking has been converted", true, shipment.JS_IsForwardRegistered);
		}

		public void TestConvertBookingToShipment_ThrowsError_IfHasAnyErrorsNotCalledFirst()
		{
			var booking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			var conversionSource = BookingToShipmentConversionSource.Form;
			var targetShipment = Factory.New<ForwardingShipment>();
			AssertEquals("Pre-Condition - The booking has not been converted", false, booking.Booking.JS_IsForwardRegistered);
			var converter = new QuotedBookingToShipmentConverter(booking, conversionSource);
			var shipment = Factory.Load<ForwardingShipment>(booking.Booking.PK);
			CombineAssertions(() =>
			{
				AssertExceptionThrown(typeof(BookingToShipmentIncorrectUsageException), "HasAnyErrors has not been called before conversion.", () => converter.ConvertBookingToShipment(targetShipment));
				AssertEquals("The booking has not been converted", false, shipment.JS_IsForwardRegistered);
			}

			);
		}

		public void TestConvertBookingToShipment_AddEventLogAndDeniedPartyScreeningStatusNotClear_WhenRegistryEnableComplianceRiskIsOFF()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.ComplianceRiskFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.No))
			using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.All))
			{
				var booking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
				booking.Booking.JS_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;

				var conversionSource = BookingToShipmentConversionSource.Form;
				var targetShipment = Factory.New<ForwardingShipment>();
				AssertEquals("Pre-Condition - The booking has not been converted", false, booking.Booking.JS_IsForwardRegistered);

				var converter = new QuotedBookingToShipmentConverter(booking, conversionSource);
				converter.HasAnyErrors(out string _);
				converter.ConvertBookingToShipment(booking.Booking);

				var shipment = Factory.Load<ForwardingShipment>(booking.Booking.PK);
				var log = shipment.Logs.GetAllLogs()[0];

				CombineAssertions(() =>
				{
					AssertEquals("The booking has been converted", true, shipment.JS_IsForwardRegistered);
					AssertEquals("DPS Update Log", AutoEvents.DeniedPartyStatusUpdated.Code, log.Event.SE_Code);
					AssertEquals("DPS Update Log Text", "Convert Booking to Shipment when screen status is not CLR - Clear", log.ReferenceFreeText);
				});
			}
		}

		public void TestConvertBookingToShipment_AddEventLogAndOverallComplianceRiskStatusNotClear_WhenRegistryEnableComplianceRiskIsON()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.ComplianceRiskFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.All))
			using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.No))
			{
				var booking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
				var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
				complianceRiskStatus.COR_ParentID = ((IComplianceItemRiskStatusProvider)booking).ParentID;
				complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.Blocked;

				var conversionSource = BookingToShipmentConversionSource.Form;
				var targetShipment = Factory.New<ForwardingShipment>();
				AssertEquals("Pre-Condition - The booking has not been converted", false, booking.Booking.JS_IsForwardRegistered);

				var converter = new QuotedBookingToShipmentConverter(booking, conversionSource);
				converter.HasAnyErrors(out string _);
				converter.ConvertBookingToShipment(booking.Booking);

				var shipment = Factory.Load<ForwardingShipment>(booking.Booking.PK);
				var log = shipment.Logs.GetAllLogs()[0];

				CombineAssertions(() =>
				{
					AssertEquals("The booking has been converted", true, shipment.JS_IsForwardRegistered);
					AssertEquals("STU Update Log", AutoEvents.StatusUpdated.Code, log.Event.SE_Code);
					AssertEquals("STU Update Log Text", "Convert Booking to Shipment when Job Compliance Status is not CLR - Clear", log.ReferenceFreeText);
				});
			}
		}

		public void TestConvertBookingToShipment_WithAutoGenerateHouseBillForOriginAndCompanyAreInSameCountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
				booking.Booking.JS_RL_NKOrigin = "AUSYD";
				var converter = new QuotedBookingToShipmentConverter(booking, BookingToShipmentConversionSource.WorkflowTrigger);
				converter.HasAnyErrors(out string errorMessage);
				AssertNull(errorMessage);
				converter.ConvertBookingToShipment(booking.Booking);
				AssertEquals("HouseBill Not Generated Automatically Because Origin and Company Country are not Same", ZString.Empty, booking.Booking.JS_HouseBill);

				booking.Booking.JS_RL_NKOrigin = "USLAX";
				converter.HasAnyErrors(out errorMessage);
				AssertNull(errorMessage);
				converter.ConvertBookingToShipment(booking.Booking);
				AssertEquals("HouseBill Generated Automatically", "PENDING ALLOCATION..", booking.Booking.JS_HouseBill);
			}
		}

		public void TestConvertBookingToShipment_DoNotAutoGenerateHouseBillWhenDirectBooking()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
				booking.Booking.JS_RL_NKOrigin = "USLAX";
				booking.Booking.JS_IsDirectBooking = true;
				var converter = new QuotedBookingToShipmentConverter(booking, BookingToShipmentConversionSource.WorkflowTrigger);
				converter.HasAnyErrors(out _);
				converter.ConvertBookingToShipment(booking.Booking);
				AssertEquals("HouseBill is not generated automatically because booking is direct", ZString.Empty, booking.Booking.JS_HouseBill);
			}
		}

		public void TestConvertBookingToShipment_KeepHouseBillWhenHouseBillIsNotEmpty()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
				booking.Booking.JS_RL_NKOrigin = "AUSYD";
				booking.Booking.JS_HouseBill = "CHISIN001357";
				var converter = new QuotedBookingToShipmentConverter(booking, BookingToShipmentConversionSource.WorkflowTrigger);
				converter.HasAnyErrors(out string errorMessage);
				AssertNull(errorMessage);
				converter.ConvertBookingToShipment(booking.Booking);
				AssertEquals("HouseBill does not change", "CHISIN001357", booking.Booking.JS_HouseBill);
				Assert("JS_HouseBillInfo has not changes", !booking.Booking.JS_HouseBillInfo.HasChanges);

				booking.Booking.JS_RL_NKOrigin = "USLAX";
				converter.HasAnyErrors(out errorMessage);
				AssertNull(errorMessage);
				converter.ConvertBookingToShipment(booking.Booking);
				AssertEquals("HouseBill does not change", "CHISIN001357", booking.Booking.JS_HouseBill);
				Assert("JS_HouseBillInfo has not changes", !booking.Booking.JS_HouseBillInfo.HasChanges);
			}
		}

		public void TestConvertBookingToShipment_GenerateHouseBillWhenNotDirectBooking()
		{
			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			booking.Booking.JS_RL_NKOrigin = "AUSYD";
			booking.Booking.JS_UniqueConsignRef = "S00001111";

			var converter = new QuotedBookingToShipmentConverter(booking, BookingToShipmentConversionSource.WorkflowTrigger);
			converter.HasAnyErrors(out string errorMessage);
			AssertNull(errorMessage);

			AssertEquals(ZString.Empty, booking.Booking.JS_HouseBill);
			converter.ConvertBookingToShipment(booking.Booking);
			AssertEquals("Should generate HouseBill number if it's empty", "PENDING ALLOCATION..", booking.Booking.JS_HouseBill);

			booking.Booking.JS_HouseBill = "HBL12345";
			converter.ConvertBookingToShipment(booking.Booking);
			AssertEquals("Should not regenerate HouseBill number if it's not empty", "HBL12345", booking.Booking.JS_HouseBill);
		}

		public void TestConvertBookingToShipment_CheckDeliveryCartageCoPK()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TestOrg1";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "TestOrg2";
			org2.OH_IsLocalTransport = false;
			Factory.Save();

			var relation = Factory.New<OrgRelatedParty>();
			relation.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;
			relation.PR_PartyType = RelatedPartyTypeList.Codes.LocalTransport;
			relation.PR_OH_Parent = org1.PK;
			relation.PR_OH_RelatedParty = org2.PK;
			relation.PR_FreightTransportMode = "ALL";
			AssertHasError(relation.PR_OH_RelatedPartyInfo, $"Only an organization set up as a Carrier and flagged as Road Transport can be used as a {RelatedPartyTypeList.Descriptions.LocalTransport}.");
			Factory.Save();

			var booking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			booking.Booking.JS_RL_NKOrigin = "AUSYD";
			booking.Booking.JS_UniqueConsignRef = "S00001111";
			booking.ConsigneeDocumentaryAddress.OrganisationPK = org1.PK;
			AssertEquals(false, booking.Booking.JS_IsForwardRegistered);
			Factory.Save();

			booking.Booking.DocsAndCartage.Validation.ValidateAll();
			AssertNoErrors(booking.Booking.DocsAndCartage.DeliveryCartageCoPKInfo);
			AssertNoErrors(booking.Booking.DocsAndCartage.JP_OA_DeliveryCartageCoAddrInfo);
			AssertNoWarnings(booking.Booking.DocsAndCartage.DeliveryCartageCoPKInfo);
			AssertNoWarnings(booking.Booking.DocsAndCartage.JP_OA_DeliveryCartageCoAddrInfo);

			var converter = new QuotedBookingToShipmentConverter(booking, BookingToShipmentConversionSource.WorkflowTrigger);
			converter.HasAnyErrors(out string errorMessage);
			AssertNull(errorMessage);
			converter.ConvertBookingToShipment(booking.Booking);
			AssertEquals(true, booking.Booking.JS_IsForwardRegistered);
			AssertHasWarning(booking.Booking.DocsAndCartage.DeliveryCartageCoPKInfo, "Enter a valid selection.");
			AssertHasWarning(booking.Booking.DocsAndCartage.JP_OA_DeliveryCartageCoAddrInfo, "Enter a valid selection.");
		}

		public void TestConvertBookingToShipment_CreateDeclaration()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
				booking.Booking.JS_RL_NKOrigin = "AUSYD";
				booking.Booking.JS_RL_NKDestination = "SGSIN";
				booking.Booking.JS_OH_ExportBroker = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
				Factory.Save();

				var converter = new QuotedBookingToShipmentConverter(booking, BookingToShipmentConversionSource.Form);
				converter.HasAnyErrors(out string errorMessage);
				AssertNull(errorMessage);

				using (FreightDataRegistry.Instance.CreateBrokerageJobAutomatically.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					converter.ConvertBookingToShipment(booking.Booking);
					var declarations = Factory.Load<IBaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_JS, booking.Booking.PK));
					AssertEquals("Should not create declaration when Registry > Freight > Shipment > Create Brokerage Job Automatically is set to false", 0, declarations.Length);
				}

				using (FreightDataRegistry.Instance.CreateBrokerageJobAutomatically.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					converter.ConvertBookingToShipment(booking.Booking);
					var declarations = Factory.Load<IBaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_JS, booking.Booking.PK));
					AssertEquals(1, declarations.Length);
					var declaration = declarations[0];
					AssertEquals("Enterprise.Customs.AU.Declaration.Business.JobDeclaration", declaration.GetType().FullName);
					AssertEquals("CreateDeclaration executes AU creator", "DEF", (ZString)declaration[JobDeclarationSchema.Constants.JE_PaymentMethod]);

					AssertNoExceptionThrown(() => converter.ConvertBookingToShipment(booking.Booking));
					declarations = Factory.Load<IBaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_JS, booking.Booking.PK));
					AssertEquals("Detects existing declaration", 1, declarations.Length);
				}
			}
		}

		public void TestGetCreateDeclarationHelper()
		{
			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			booking.Booking.JS_RL_NKOrigin = "AUSYD";
			booking.Booking.JS_RL_NKDestination = "SGSIN";
			booking.Booking.JS_OH_ExportBroker = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.Save();

			var converter = new QuotedBookingToShipmentConverter(booking, BookingToShipmentConversionSource.Form);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var helper = converter.GetCreateDeclarationHelper();
				AssertEquals("Enterprise.Customs.AU.Declaration.Business.CreateDeclarationHelper", helper.GetType().FullName);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var helper = converter.GetCreateDeclarationHelper();
				AssertEquals("Enterprise.Customs.US.Business.CreateDeclarationHelper", helper.GetType().FullName);
			}
		}

		public void TestConvertBookingToShipment_AddOutturnToShipment_ThenCloneBooking_OutturnValuesIsSetToDefault()
		{
			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var conversionSource = BookingToShipmentConversionSource.Form;
			var converter = new QuotedBookingToShipmentConverter(booking, conversionSource);
			converter.HasAnyErrors(out string _);
			converter.ConvertBookingToShipment(booking.Booking);

			var shipment = Factory.Load<ForwardingShipment>(booking.Booking.PK);
			PackLine packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_OutturnComment = "Test OutRunComment";
			packline1.JL_MarksAndNumbers = "TestMarksAndNumber";
			packline1.JL_Outturn = 4;
			packline1.JL_OutturnedWidth = 40;
			packline1.JL_OutturnedHeight = 41;
			packline1.JL_OutturnedLength = 42;
			packline1.JL_OutturnedWeight = 43;
			packline1.JL_OutturnedVolume = 44;
			packline1.JL_Damaged = 3;
			packline1.JL_Pillaged = 4;
			Factory.Save();

			CommonShipment clonedShipment = (CommonShipment)booking.Booking.Clone();

			AssertEquals(ZString.Empty, clonedShipment.OuterPackLines[0].JL_OutturnComment);
			AssertEquals(ZString.Empty, clonedShipment.OuterPackLines[0].JL_MarksAndNumbers);
			AssertEquals(0, clonedShipment.OuterPackLines[0].JL_Outturn);
			AssertEquals(0, clonedShipment.OuterPackLines[0].JL_Damaged);
			AssertEquals(0, clonedShipment.OuterPackLines[0].JL_Pillaged);
			AssertEquals((decimal)0, clonedShipment.OuterPackLines[0].JL_OutturnedWidth);
			AssertEquals((decimal)0, clonedShipment.OuterPackLines[0].JL_OutturnedHeight);
			AssertEquals((decimal)0, clonedShipment.OuterPackLines[0].JL_OutturnedLength);
			AssertEquals((decimal)0, clonedShipment.OuterPackLines[0].JL_OutturnedWeight);
			AssertEquals((decimal)0, clonedShipment.OuterPackLines[0].JL_OutturnedVolume);
		}

		public void TestConvertQBToShipment_WhenConvertQBWithCarrier_ThenCarrierShouldBePreserved()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			carrier.MainAddress.OA_Address1 = "Address 1";

			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			booking.Booking.JS_OA_BookedShippingLineAddress = carrier.MainAddress.PK;

			Factory.Save();

			var converter = new QuotedBookingToShipmentConverter(booking, BookingToShipmentConversionSource.Form);
			converter.HasAnyErrors(out string errorMessage);
			AssertNull(errorMessage);
			converter.ConvertBookingToShipment(booking.Booking);

			AssertEquals("Carrier information should be preserved", carrier.MainAddress.PK, booking.Booking.JS_OA_BookedShippingLineAddress);
		}

		public void TestConvertBWQToShipment_WhenConvertBWQWithCarrier_ThenCarrierShouldBePreserved()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			carrier.MainAddress.OA_Address1 = "Address 1";

			var booking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			booking.Booking.JS_OA_BookedShippingLineAddress = carrier.MainAddress.PK;

			Factory.Save();

			var converter = new QuotedBookingToShipmentConverter(booking, BookingToShipmentConversionSource.Form);
			converter.HasAnyErrors(out string errorMessage);
			AssertNull(errorMessage);
			converter.ConvertBookingToShipment(booking.Booking);

			AssertEquals("Carrier information should be preserved", carrier.MainAddress.PK, booking.Booking.JS_OA_BookedShippingLineAddress);
		}

		public void TestConvertBookingToShipment_WithJobComplianceStatusRemainOverrideClear()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(true)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var booking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
				var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
				complianceRiskStatus.COR_ParentID = ((IComplianceItemRiskStatusProvider)booking).ParentID;
				complianceRiskStatus.COR_ParentTableCode = booking.Quote.TablePrefix;
				complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.OverrideClear;
				Factory.Save();

				var converter = new QuotedBookingToShipmentConverter(booking, BookingToShipmentConversionSource.Form);
				converter.HasAnyErrors(out string _);
				converter.ConvertBookingToShipment(booking.Booking);
				var shipment = Factory.Load<ForwardingShipment>(booking.Booking.PK);
				var newComplianceRiskStatus = Factory.LoadTop1<ComplianceRiskStatus>(new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, shipment.PK));
				AssertNotNull(newComplianceRiskStatus);
				AssertEquals(complianceRiskStatus.COR_OverallRisk, newComplianceRiskStatus.COR_OverallRisk);
				AssertEquals(complianceRiskStatus.COR_PartyRisk, newComplianceRiskStatus.COR_PartyRisk);
				AssertEquals(complianceRiskStatus.COR_LocationRisk, newComplianceRiskStatus.COR_LocationRisk);
				Assert(newComplianceRiskStatus.CopyFromBooking);
				Assert(newComplianceRiskStatus.CopyFromBookingFirstLoaded);
			}
		}

		public void TestConvertBookingToShipment_WithDeliveryDueDate()
		{
			var newDeliveryDueDate = new ZDateTime(2022, 10, 04, 09, 30, 0);
			var deliveryDueDateCalculatorManagerMock = DeliveryDueDateCalculationTestHelper.SetupDeliveryDueDateCalculatorManagerMockWithoutSubstitution(newDeliveryDueDate);
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				Env.Security.MaintainShipmentDeliveryDueDateOverride.IsAllowed = true;
				var shipment = QuotedBooking.CreateNewBooking(Factory);
				var quotedBooking = QuotedBooking.New(ZGuid.Empty, shipment.PK, Factory);
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

				AssertEquals("Pre-condition: Delivery Due Date is auto-calculated", newDeliveryDueDate, quotedBooking.DeliveryDueDate);
				Factory.Save();

				var converter = new QuotedBookingToShipmentConverter(quotedBooking, BookingToShipmentConversionSource.Form);
				converter.HasAnyErrors(out string errorMessage);
				AssertNull(errorMessage);
				converter.ConvertBookingToShipment(quotedBooking.Booking);

				Factory.Save();

				var shipmentReloaded = Factory.Load<ForwardingShipment>(shipment.PK);
				var ddeEvent = shipmentReloaded.Logs.MostRecentLogByEventTime(Events.CalculateDeliveryDateWithExceptionsRequested);
				AssertNotNull(ddeEvent);
				AssertEquals("|CHG=Delivery Due Date 04-Oct-22 09:30:00 Converted From Booking", ddeEvent.SL_Reference);
			}
		}

		public void TestConvertBookingToShipment_WithoutDeliveryDueDate()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, shipment.PK, Factory);
			quotedBooking.TransportMode = Core.Constants.TransportModes.Air;

			Factory.Save();

			var converter = new QuotedBookingToShipmentConverter(quotedBooking, BookingToShipmentConversionSource.Form);
			converter.HasAnyErrors(out string errorMessage);
			AssertNull(errorMessage);
			converter.ConvertBookingToShipment(quotedBooking.Booking);

			Factory.Save();

			var shipmentReloaded = Factory.Load<ForwardingShipment>(shipment.PK);
			var ddeEvent = shipmentReloaded.Logs.MostRecentLogByEventTime(Events.CalculateDeliveryDateWithExceptionsRequested);
			AssertNull(ddeEvent);
		}

		CalculateDeliveryDueDateTransportModeCollection ActiveTransportModesForCalculateDeliveryDateOption()
		{
			var activeTransportModes = new CalculateDeliveryDueDateTransportModeCollection();
			activeTransportModes.Add(Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Road, Core.Constants.TransportModeDescriptions.Road, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Rail, Core.Constants.TransportModeDescriptions.Rail, true);
			return activeTransportModes;
		}

		#endregion

		#region IsDeniedPartyOrComplianceRiskStatusNotClearAndFreightMovementRestricted

		public void TestDeniedPartyOrComplianceRiskStatusNotClearAndFreightMovementRestricted_WhenRegistryEnableComplianceRiskIsON()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.ComplianceRiskFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.All))
			using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.No))
			{
				var booking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
				booking.Booking.JS_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

				var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
				complianceRiskStatus.COR_ParentID = ((IComplianceItemRiskStatusProvider)booking).ParentID;
				complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.Held;

				AssertEquals("Should check if compliance risk status not equal to clear", true, IsDeniedPartyOrComplianceRiskStatusNotClearAndFreightMovementRestricted(booking));
			}
		}

		public void TestDeniedPartyOrComplianceRiskStatusNotClearAndFreightMovementRestricted_WhenRegistryEnableComplianceRiskIsOFF()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.ComplianceRiskFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.No))
			using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.All))
			{
				var booking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
				booking.Booking.JS_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;

				var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
				complianceRiskStatus.COR_ParentID = ((IComplianceItemRiskStatusProvider)booking).ParentID;
				complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.Clear;

				AssertEquals("Should check if screening status not equal to clear", true, IsDeniedPartyOrComplianceRiskStatusNotClearAndFreightMovementRestricted(booking));
			}
		}

		#endregion

		#region HasAnyErrors

		public void TestHasAnyErrors_WithNoErrors_ReturnsNull()
		{
			var booking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			Factory.Save();
			var converter = new QuotedBookingToShipmentConverter(booking, BookingToShipmentConversionSource.Form);
			var hasErrors = converter.HasAnyErrors(out string errorMessage);
			CombineAssertions("Should have no errors", () =>
			{
				Assert(!hasErrors);
				AssertNull(errorMessage);
			}

			);
		}

		public void TestHasAnyErrors_IfNoBooking_ReturnsErrorMessage()
		{
			var booking = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
			var converter = new QuotedBookingToShipmentConverter(booking, BookingToShipmentConversionSource.Form);
			var hasErrors = converter.HasAnyErrors(out string errorMessage);
			CombineAssertions(() =>
			{
				Assert(hasErrors);
				AssertEquals("Cannot convert a Spot Quote to Shipment without a Booking", errorMessage);
			}

			);
		}

		public void TestHasAnyErrors_IfNotBooked_ReturnsErrorMessage()
		{
			var booking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			booking.ShipmentStatus = ShipmentStatusList.Codes.BookingRejected;
			Factory.Save();
			var converter = new QuotedBookingToShipmentConverter(booking, BookingToShipmentConversionSource.Form);
			var hasErrors = converter.HasAnyErrors(out string errorMessage);
			CombineAssertions(() =>
			{
				Assert(hasErrors);
				AssertEquals("The Booking is not confirmed yet.  Please confirm the booking by changing the HBL Booking Status to BKD.", errorMessage);
			}

			);
		}

		public void TestHasAnyErrors_IfDirectBooking_ReturnsErrorMessage()
		{
			var booking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			booking.Booking.JS_IsDirectBooking = true;
			Factory.Save();
			var converter = new QuotedBookingToShipmentConverter(booking, BookingToShipmentConversionSource.Form);
			var hasErrors = converter.HasAnyErrors(out string errorMessage);
			CombineAssertions(() =>
			{
				Assert(hasErrors);
				AssertEquals("You cannot convert a direct booking to a shipment. Please choose 'Consolidate' from the menu to create a new Consol.", errorMessage);
			}

			);
		}

		public void TestHasAnyErrors_IfConversionSourceForm_AndUnsaved_ReturnsErrorMessage()
		{
			var booking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			booking.Origin = "AUSYD";
			var converter = new QuotedBookingToShipmentConverter(booking, BookingToShipmentConversionSource.Form);
			var hasErrors = converter.HasAnyErrors(out string errorMessage);
			CombineAssertions(() =>
			{
				Assert(hasErrors);
				AssertEquals("Please save the booking before converting to shipment.", errorMessage);
			}

			);
			converter = new QuotedBookingToShipmentConverter(booking, BookingToShipmentConversionSource.WorkflowTrigger);
			hasErrors = converter.HasAnyErrors(out errorMessage);
			CombineAssertions(() =>
			{
				Assert(!hasErrors);
				AssertNull(errorMessage);
			}

			);
		}

		#endregion
	}
}
