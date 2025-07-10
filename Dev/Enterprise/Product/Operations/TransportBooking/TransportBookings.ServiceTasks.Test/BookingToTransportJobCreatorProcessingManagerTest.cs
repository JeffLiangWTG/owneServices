using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DbUpgrader.Shared;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.TransportBookings.ServiceTasks.Test
{
	class BookingToTransportJobCreatorProcessingManagerTest : DtbBookingTestCaseWithFactory
	{
		// Local Transport Test

		[TestDate(2013, 1, 1, 1, 20, 0)]
		public void TestCreatePortTransportsFromTransportBookings_OldNamespace()
		{
			TestCreatePortTransportsFromTransportBookings(UniversalXmlInfo.Namespace_2011_11);
		}

		[TestDate(2013, 1, 1, 1, 20, 0)]
		public void TestCreatePortTransportsFromTransportBookings_NewNamespace()
		{
			TestCreatePortTransportsFromTransportBookings(UniversalXmlInfo.Namespace_2012_11);
		}

		void TestCreatePortTransportsFromTransportBookings(string nameSpace)
		{
			var today = ZDateTime.UtcToday;
			using (SchemaVersionManager.SetNamespaceForTesting(nameSpace))
			{
				var dummyBookingParent = Factory.New<DummyWithDtbBooking>();
				var nonOrgProxyTransportCompany = Factory.NewWithValidTestData<OrgHeader>();

				var branchTransportCompany = Factory.NewWithValidTestData<OrgHeader>();
				var branch = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).Branches.AddNew();
				branch.GB_Code = "~BR";
				branch.GB_OH_OrgProxy = branchTransportCompany.PK;
				branchTransportCompany.OH_IsShippingProvider = true;
				branchTransportCompany.OH_IsLocalTransport = true;

				var validConfirmationDate = today.AddHours(1);
				var validJobCreatedDate = today.AddMinutes(-30);
				var jobTooYoungDate = today.AddMinutes(-20);
				var jobTooOldDate = today.AddDays(-2);
				var confirmationTooFarInFutureDate = today.AddHours(TransportRegistry.Instance.AutoCreateTransportConsignmentsCreationPeriod.Value).AddMinutes(5);

				var bookingWhereTransportCompanyIsNotOrgProxy = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);
				BookingToTransportJobCreatorTestHelper.SetDatesRelatedToAutomaticTransportJobCreationForBooking(Factory, bookingWhereTransportCompanyIsNotOrgProxy, validJobCreatedDate, validConfirmationDate);
				bookingWhereTransportCompanyIsNotOrgProxy.Address.OrganisationPK = nonOrgProxyTransportCompany.PK;
				bookingWhereTransportCompanyIsNotOrgProxy.KM_JobID = "TransCompNotOrgProxy";

				var bookingWhereTransportCompanyIsOrgProxy = BookingToTransportJobCreatorTestHelper.CreateNewBookingOnConsolidationValidForManuallyCreatingATransportJobFrom(Helper, bookingWhereTransportCompanyIsNotOrgProxy.ConsolidationSingleJob);
				BookingToTransportJobCreatorTestHelper.SetDatesRelatedToAutomaticTransportJobCreationForBooking(Factory, bookingWhereTransportCompanyIsOrgProxy, validJobCreatedDate, validConfirmationDate);
				bookingWhereTransportCompanyIsOrgProxy.KM_JobID = "TransCompIsOrgProxy";

				var bookingWhereTransportCompanyIsBranchOrgProxy = BookingToTransportJobCreatorTestHelper.CreateNewBookingOnConsolidationValidForManuallyCreatingATransportJobFrom(Helper, bookingWhereTransportCompanyIsNotOrgProxy.ConsolidationSingleJob);
				BookingToTransportJobCreatorTestHelper.SetDatesRelatedToAutomaticTransportJobCreationForBooking(Factory, bookingWhereTransportCompanyIsBranchOrgProxy, validJobCreatedDate, validConfirmationDate);
				bookingWhereTransportCompanyIsBranchOrgProxy.Address.OrganisationPK = branchTransportCompany.PK;
				bookingWhereTransportCompanyIsBranchOrgProxy.KM_JobID = "TraCoIsBrnchOrgProxy";

				var bookingWhereJobAgeIsTooYoung = BookingToTransportJobCreatorTestHelper.CreateNewBookingOnConsolidationValidForManuallyCreatingATransportJobFrom(Helper, bookingWhereTransportCompanyIsNotOrgProxy.ConsolidationSingleJob);
				BookingToTransportJobCreatorTestHelper.SetDatesRelatedToAutomaticTransportJobCreationForBooking(Factory, bookingWhereJobAgeIsTooYoung, jobTooYoungDate, validConfirmationDate);
				bookingWhereJobAgeIsTooYoung.KM_JobID = "JobAgeTooYoung";

				var standaloneBooking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper);
				BookingToTransportJobCreatorTestHelper.SetDatesRelatedToAutomaticTransportJobCreationForBooking(Factory, standaloneBooking, validJobCreatedDate, validConfirmationDate);
				standaloneBooking.KM_JobID = "StandaloneBooking";

				var bookingWhereEarliestPickupConfirmationDateTooFarInFuture = BookingToTransportJobCreatorTestHelper.CreateNewBookingOnConsolidationValidForManuallyCreatingATransportJobFrom(Helper, bookingWhereTransportCompanyIsNotOrgProxy.ConsolidationSingleJob);
				BookingToTransportJobCreatorTestHelper.SetDatesRelatedToAutomaticTransportJobCreationForBooking(Factory, bookingWhereEarliestPickupConfirmationDateTooFarInFuture, validJobCreatedDate, confirmationTooFarInFutureDate);
				bookingWhereEarliestPickupConfirmationDateTooFarInFuture.KM_JobID = "PickUpTooFarInFuture";

				var bookingValidToCreateLandTransportFrom = BookingToTransportJobCreatorTestHelper.CreateNewBookingOnConsolidationValidForManuallyCreatingATransportJobFrom(Helper, bookingWhereTransportCompanyIsNotOrgProxy.ConsolidationSingleJob);
				BookingToTransportJobCreatorTestHelper.SetDatesRelatedToAutomaticTransportJobCreationForBooking(Factory, bookingValidToCreateLandTransportFrom, validJobCreatedDate, validConfirmationDate);
				bookingValidToCreateLandTransportFrom.KM_JobID = "ValidLTJobBooking";

				var bookingThatAlreadyHasATransportConsignmentAttached = BookingToTransportJobCreatorTestHelper.CreateNewBookingOnConsolidationValidForManuallyCreatingATransportJobFrom(Helper, bookingWhereTransportCompanyIsNotOrgProxy.ConsolidationSingleJob);
				BookingToTransportJobCreatorTestHelper.SetDatesRelatedToAutomaticTransportJobCreationForBooking(Factory, bookingThatAlreadyHasATransportConsignmentAttached, validJobCreatedDate, validConfirmationDate);
				bookingThatAlreadyHasATransportConsignmentAttached.KM_JobID = "AlreadyHasTCJob";
				var relatedConsignment = Factory.New<ICommonCartage>();
				relatedConsignment.JJ_ParentID = bookingThatAlreadyHasATransportConsignmentAttached.PK;
				relatedConsignment.JJ_ParentTableCode = bookingThatAlreadyHasATransportConsignmentAttached.TablePrefix;
				relatedConsignment.JJ_ConsignmentID = "PORTJOB1";

				var inactiveBooking = BookingToTransportJobCreatorTestHelper.CreateNewBookingOnConsolidationValidForManuallyCreatingATransportJobFrom(Helper, bookingWhereTransportCompanyIsNotOrgProxy.ConsolidationSingleJob);
				BookingToTransportJobCreatorTestHelper.SetDatesRelatedToAutomaticTransportJobCreationForBooking(Factory, inactiveBooking, validJobCreatedDate, validConfirmationDate);
				inactiveBooking.KM_IsActive = false;
				inactiveBooking.KM_JobID = "InactiveBooking";

				var bookingWhereJobAgeIsToOld = BookingToTransportJobCreatorTestHelper.CreateNewBookingOnConsolidationValidForManuallyCreatingATransportJobFrom(Helper, bookingWhereTransportCompanyIsNotOrgProxy.ConsolidationSingleJob);
				BookingToTransportJobCreatorTestHelper.SetDatesRelatedToAutomaticTransportJobCreationForBooking(Factory, bookingWhereJobAgeIsToOld, jobTooOldDate, validConfirmationDate);
				inactiveBooking.KM_JobID = "JobAgeTooOld";

				var bookingWithNoPickupConfirmations = BookingToTransportJobCreatorTestHelper.CreateNewBookingOnConsolidationValidForManuallyCreatingATransportJobFrom(Helper, bookingWhereTransportCompanyIsNotOrgProxy.ConsolidationSingleJob);
				BookingToTransportJobCreatorTestHelper.SetDatesRelatedToAutomaticTransportJobCreationForBooking(Factory, bookingWithNoPickupConfirmations, validJobCreatedDate, validConfirmationDate);
				bookingWithNoPickupConfirmations.PickupConfirmations.DeleteAll();
				bookingWithNoPickupConfirmations.KM_JobID = "NoPicConfirmations";

				var bookingWithMultiplePickupInstructions = BookingToTransportJobCreatorTestHelper.CreateNewBookingOnConsolidationValidForManuallyCreatingATransportJobFrom(Helper, bookingWhereTransportCompanyIsNotOrgProxy.ConsolidationSingleJob);
				BookingToTransportJobCreatorTestHelper.SetDatesRelatedToAutomaticTransportJobCreationForBooking(Factory, bookingWithMultiplePickupInstructions, validJobCreatedDate, validConfirmationDate);
				BookingToTransportJobCreatorTestHelper.CreateNewInstruction(Helper, bookingWithMultiplePickupInstructions, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, Helper.CreateOrLoadOrganisation("PICKUP_ORG").MainAddress, shouldHaveAtLeastOneConfirmation: true, attachExistingPackageOrCreateAndAttachPackage: true);
				bookingWithMultiplePickupInstructions.KM_JobID = "MultiplePickUps";

				var bookingWithMultipleDeliveryInstructions = BookingToTransportJobCreatorTestHelper.CreateNewBookingOnConsolidationValidForManuallyCreatingATransportJobFrom(Helper, bookingWhereTransportCompanyIsNotOrgProxy.ConsolidationSingleJob);
				BookingToTransportJobCreatorTestHelper.SetDatesRelatedToAutomaticTransportJobCreationForBooking(Factory, bookingWithMultipleDeliveryInstructions, validJobCreatedDate, validConfirmationDate);
				BookingToTransportJobCreatorTestHelper.CreateNewInstruction(Helper, bookingWithMultipleDeliveryInstructions, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, Helper.CreateOrLoadOrganisation("DELIVERY_ORG").MainAddress, shouldHaveAtLeastOneConfirmation: true, attachExistingPackageOrCreateAndAttachPackage: true);
				bookingWithMultipleDeliveryInstructions.KM_JobID = "MultipleDeliveries";

				var heldBooking = BookingToTransportJobCreatorTestHelper.CreateNewBookingOnConsolidationValidForManuallyCreatingATransportJobFrom(Helper, bookingWhereTransportCompanyIsNotOrgProxy.ConsolidationSingleJob);
				BookingToTransportJobCreatorTestHelper.SetDatesRelatedToAutomaticTransportJobCreationForBooking(Factory, heldBooking, validJobCreatedDate, validConfirmationDate);
				heldBooking.KM_Status = TransportStatuses.Codes.Held;
				heldBooking.KM_JobID = "HeldBooking";

				var bookingThatAlreadyHasAPortTransportJobAttached = BookingToTransportJobCreatorTestHelper.CreateNewBookingOnConsolidationValidForManuallyCreatingATransportJobFrom(Helper, bookingWhereTransportCompanyIsNotOrgProxy.ConsolidationSingleJob);
				BookingToTransportJobCreatorTestHelper.SetDatesRelatedToAutomaticTransportJobCreationForBooking(Factory, bookingThatAlreadyHasAPortTransportJobAttached, validJobCreatedDate, validConfirmationDate);
				bookingThatAlreadyHasAPortTransportJobAttached.KM_JobID = "AlreadyHasPTJob";
				var relatedPortTransport = Factory.New<ICommonCartage>();
				relatedPortTransport.JJ_ParentID = bookingThatAlreadyHasAPortTransportJobAttached.PK;
				relatedPortTransport.JJ_ParentTableCode = bookingThatAlreadyHasAPortTransportJobAttached.TablePrefix;

				var bookingWithNoPickUpDate = BookingToTransportJobCreatorTestHelper.CreateNewBookingOnConsolidationValidForManuallyCreatingATransportJobFrom(Helper, bookingWhereTransportCompanyIsNotOrgProxy.ConsolidationSingleJob);
				BookingToTransportJobCreatorTestHelper.SetDatesRelatedToAutomaticTransportJobCreationForBooking(Factory, bookingWithNoPickUpDate, validJobCreatedDate, ZDateTime.Empty);
				bookingWithNoPickUpDate.KM_JobID = "NoPickUpDate";

				var bookingWithNoPickupDateButWithAnEstimatedDate = BookingToTransportJobCreatorTestHelper.CreateNewBookingOnConsolidationValidForManuallyCreatingATransportJobFrom(Helper, bookingWhereTransportCompanyIsNotOrgProxy.ConsolidationSingleJob);
				BookingToTransportJobCreatorTestHelper.SetDatesRelatedToAutomaticTransportJobCreationForBooking(Factory, bookingWithNoPickupDateButWithAnEstimatedDate, validJobCreatedDate, ZDateTime.Empty);
				bookingWithNoPickupDateButWithAnEstimatedDate.Instructions.First(i => i.KN_InstructionType == InstructionTypes.Codes.PickUp).Confirmations[0].KK_Estimated = validConfirmationDate;
				bookingWithNoPickupDateButWithAnEstimatedDate.KM_JobID = "OnlyEstPicDate";

				var bookingWithNoPickupDateButWithAnInvalidEstimatedDate = BookingToTransportJobCreatorTestHelper.CreateNewBookingOnConsolidationValidForManuallyCreatingATransportJobFrom(Helper, bookingWhereTransportCompanyIsNotOrgProxy.ConsolidationSingleJob);
				BookingToTransportJobCreatorTestHelper.SetDatesRelatedToAutomaticTransportJobCreationForBooking(Factory, bookingWithNoPickupDateButWithAnInvalidEstimatedDate, validJobCreatedDate, ZDateTime.Empty);
				bookingWithNoPickupDateButWithAnInvalidEstimatedDate.Instructions.First(i => i.KN_InstructionType == InstructionTypes.Codes.PickUp).Confirmations[0].KK_Estimated = confirmationTooFarInFutureDate;
				bookingWithNoPickupDateButWithAnEstimatedDate.KM_JobID = "OnlyInvalidEstDate";

				var standaloneBookingWithConsignment = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper);
				BookingToTransportJobCreatorTestHelper.SetDatesRelatedToAutomaticTransportJobCreationForBooking(Factory, standaloneBookingWithConsignment, validJobCreatedDate, validConfirmationDate);
				standaloneBookingWithConsignment.KM_JobID = "StandaloneWithCon";

				Factory.Save();

				BookingToTransportJobCreatorTestHelper.ForceBookingConsolidationToConsignmentConsolidationInDB(TestConnection, standaloneBookingWithConsignment.KM_KB_Booking);

				TestDateAttribute.Date = today.ToDateTime();
				using (SetupServerTaskOption(AutoCreatorTargetModules.Codes.PortTransport, AutoCreatorTargetModules.Codes.PortTransport, AutoCreatorTargetModules.Codes.PortTransport, AutoCreatorTargetModules.Codes.PortTransport))
				using (TransportRegistry.Instance.AutoCreateTransportConsignmentsCreationPeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
				using (TransportRegistry.Instance.CreateTransportJobsFromTransportBookingEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, jobTooOldDate.AddDays(1).ToDateTime().ToUniversalTime()))
				{
					var logger = new TestServiceLogger();
					var processingManager = new BookingToTransportJobCreatorProcessingManager(logger);
					processingManager.CreateTransportJobsFromTransportBookings();

					CombineAssertions(() =>
					{
						AssertBookingHasNoAttachedPortTransport(bookingWhereTransportCompanyIsNotOrgProxy);
						AssertBookingHasNoAttachedPortTransport(bookingWhereJobAgeIsTooYoung);
						AssertBookingHasNoAttachedPortTransport(bookingWhereEarliestPickupConfirmationDateTooFarInFuture);
						AssertBookingHasNoAttachedPortTransport(inactiveBooking);
						AssertBookingHasNoAttachedPortTransport(bookingWhereJobAgeIsToOld);
						AssertBookingHasNoAttachedPortTransport(bookingWithNoPickupConfirmations);
						AssertBookingHasNoAttachedPortTransport(heldBooking);
						AssertBookingHasNoAttachedPortTransport(bookingWithNoPickupDateButWithAnInvalidEstimatedDate);
						AssertBookingHasNoAttachedPortTransport(standaloneBookingWithConsignment);

						AssertBookingHasAttachedPortTransport(bookingThatAlreadyHasATransportConsignmentAttached);
						AssertBookingHasAttachedPortTransport(bookingThatAlreadyHasAPortTransportJobAttached);

						AssertBookingHasNewPortTransportAttached(bookingWhereTransportCompanyIsOrgProxy);
						AssertBookingHasNewPortTransportAttached(bookingWhereTransportCompanyIsBranchOrgProxy);
						AssertBookingHasNewPortTransportAttached(standaloneBooking);
						AssertBookingHasNewPortTransportAttached(bookingValidToCreateLandTransportFrom);
						AssertBookingHasNewPortTransportAttached(bookingWithMultiplePickupInstructions);
						AssertBookingHasNewPortTransportAttached(bookingWithMultipleDeliveryInstructions);
						AssertBookingHasNewPortTransportAttached(bookingWithNoPickUpDate);
						AssertBookingHasNewPortTransportAttached(bookingWithNoPickupDateButWithAnEstimatedDate);
						AssertMultilineASCIIEquals("Should log creation of Consignments.", @"
Information|Successfully created Port Transport T00001001 from Transport Booking MultipleDeliveries
Information|Successfully created Port Transport T00001002 from Transport Booking MultiplePickUps
Information|Successfully created Port Transport T00001003 from Transport Booking NoPickUpDate
Information|Successfully created Port Transport T00001004 from Transport Booking OnlyInvalidEstDate
Information|Successfully created Port Transport T00001005 from Transport Booking StandaloneBooking
Information|Successfully created Port Transport T00001006 from Transport Booking TraCoIsBrnchOrgProxy
Information|Successfully created Port Transport T00001007 from Transport Booking TransCompIsOrgProxy
Information|Successfully created Port Transport T00001008 from Transport Booking ValidLTJobBooking
					".Trim(), logger.ToString());
					});
				}
			}
		}

		void AssertBookingHasNoAttachedPortTransport(DtbBooking booking)
		{
			var query = new ZQuery();
			query.AddToFilter(JobCartageSchema.JJ_ParentID, booking.PK);
			query.AddToFilter(JobCartageSchema.JJ_ParentTableCode, booking.TablePrefix);

			var portTransports = Factory.Load<ICommonCartage>(query);
			AssertEquals(booking.HumanReadableName + " Port Transport job was created.", 0, portTransports.Length);
		}

		void AssertBookingHasAttachedPortTransport(DtbBooking booking)
		{
			var query = new ZQuery();
			query.AddToFilter(JobCartageSchema.JJ_ParentID, booking.PK);
			query.AddToFilter(JobCartageSchema.JJ_ParentTableCode, booking.TablePrefix);

			var portTransports = Factory.Load<ICommonCartage>(query);
			AssertEquals(booking.HumanReadableName + " Port Transport job was created.", 1, portTransports.Length);
		}

		BusinessObject AssertBookingHasNewPortTransportAttached(DtbBooking booking)
		{
			var query = new ZQuery();
			query.AddToFilter(JobCartageSchema.JJ_ParentID, booking.PK);
			query.AddToFilter(JobCartageSchema.JJ_ParentTableCode, booking.TablePrefix);

			var portTransports = Factory.Load<ICommonCartage>(query);
			AssertEquals(booking.HumanReadableName + " was unable to create a Port Transport.", 1, portTransports.Length);

			var portTransport = (EnterpriseBusinessObject)portTransports[0];
			var expectedTransportReference = portTransport[JobCartageSchema.Constants.JJ_ConsignmentID];
			var serviceLogs = portTransport.Logs.GetAllLogs().Cast<StmALog>().Where(l => l.SL_SE_NKEvent == AutoEvents.ServiceCommencedCode);
			AssertEquals(1, serviceLogs.Count());
			AssertEquals(expectedTransportReference, serviceLogs.ElementAt(0).ReferenceFreeText);

			AssertEquals(TransportStatuses.Codes.ServiceCommenced, new BusinessObjectFactory().Load<DtbBooking>(booking.PK).KM_Status);

			return portTransport;
		}

		[TestDate(2013, 12, 4)]
		public void TestCreatePortTransportsFromTransportBookings_WithNoPackages()
		{
			var today = ZDateTime.UtcToday;

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper);
			BookingToTransportJobCreatorTestHelper.SetDatesRelatedToAutomaticTransportJobCreationForBooking(Factory, booking, today.AddDays(-1), today);
			booking.KM_RatingFreightMode = RatingFreightModes.Codes.Loose;

			Factory.Save();

			using (SetupServerTaskOption(AutoCreatorTargetModules.Codes.PortTransport, AutoCreatorTargetModules.Codes.PortTransport, AutoCreatorTargetModules.Codes.PortTransport, AutoCreatorTargetModules.Codes.PortTransport))
			using (TransportRegistry.Instance.CreateTransportJobsFromTransportBookingEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, today.AddDays(-10).ToDateTime().ToUniversalTime()))
			{
				TestDateAttribute.Date = today.ToDateTime();

				var logger = new TestServiceLogger();
				var processingManager = new BookingToTransportJobCreatorProcessingManager(logger);
				processingManager.CreateTransportJobsFromTransportBookings();
				var portTransport = AssertBookingHasNewPortTransportAttached(booking);
				var looseMovements = Factory.Load<ICommonBookedCtgMove>(new ZQuery(JobBookedCtgMoveSchema.EW_JJ, portTransport.PK));

				AssertEquals("Should default a booked move", 1, looseMovements.Length);
				var looseMovement = (BusinessObject)looseMovements[0];
				AssertEquals(ZDateTime.Now, looseMovement[JobBookedCtgMoveSchema.EW_RequestedPickupTimeStart]);
			}
		}

		public void TestCreatePortTransportsFromTransportBookings_LogsErrorMessages()
		{
			using (SetupServerTaskOption(AutoCreatorTargetModules.Codes.PortTransport, AutoCreatorTargetModules.Codes.PortTransport, AutoCreatorTargetModules.Codes.PortTransport, AutoCreatorTargetModules.Codes.PortTransport))
			using (TransportRegistry.Instance.CreateTransportJobsFromTransportBookingEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Now))
			{
				DbColumnDependencyRemover.DropSchemaBoundReferencingObjects(Db.Connection, Db.SqlDbOwnerSchema, "DtbBookingConfirmation");
				DbColumnDependencyRemover.DropSchemaBoundReferencingObjects(Db.Connection, Db.SqlDbOwnerSchema, "DtbBookingInstructionPkgDivot");
				DbColumnDependencyRemover.DropSchemaBoundReferencingObjects(Db.Connection, Db.SqlDbOwnerSchema, "DtbBookingInstruction");

				TestConnection.ExecuteNonQuery("DROP TABLE DtbBookingConfirmation DROP TABLE DtbBookingInstructionPkgDivot DROP TABLE DtbBookingInstruction");

				var logger = new TestServiceLogger();
				var processingManager = new BookingToTransportJobCreatorProcessingManager(logger);
				processingManager.CreateTransportJobsFromTransportBookings();
				AssertEquals("Error|Invalid object name 'dbo.DtbBookingInstruction'.\r\nInformation|Auto-creation of Transport Jobs failed.\r\n", logger.ToString());
			}
		}

		public void TestCreatePortTransportsFromTransportBookings_LogsWhenNoTransportBookingsWereFound()
		{
			using (SetupServerTaskOption(AutoCreatorTargetModules.Codes.PortTransport, AutoCreatorTargetModules.Codes.PortTransport, AutoCreatorTargetModules.Codes.PortTransport, AutoCreatorTargetModules.Codes.PortTransport))
			using (TransportRegistry.Instance.CreateTransportJobsFromTransportBookingEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Now))
			{
				var logger = new TestServiceLogger();
				var processingManager = new BookingToTransportJobCreatorProcessingManager(logger);
				processingManager.CreateTransportJobsFromTransportBookings();
				AssertEquals("Information|Did not find any Transport Bookings to Create Transport Jobs from.\r\n", logger.ToString());
			}
		}

		IDisposable SetupServerTaskOption(ZString cntOption, ZString mixOption, ZString ftlOption, ZString lseOption)
		{
			var serviceTaskOptions = new ServiceTaskCreatorOptionCollection();
			SetupServiceTaskOption(serviceTaskOptions.AddNew(), AutoCreatorContainerModes.Codes.Container, cntOption, false);
			SetupServiceTaskOption(serviceTaskOptions.AddNew(), AutoCreatorContainerModes.Codes.MixedCargo, mixOption, false);
			SetupServiceTaskOption(serviceTaskOptions.AddNew(), AutoCreatorContainerModes.Codes.FTL, ftlOption, false);
			SetupServiceTaskOption(serviceTaskOptions.AddNew(), AutoCreatorContainerModes.Codes.Loose, lseOption, false);

			return TransportRegistry.Instance.ServiceTaskCreatorOption.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, serviceTaskOptions);
		}

		void SetupServiceTaskOption(ServiceTaskCreatorOption serviceTaskOption, ZString containerMode, ZString targetModule, ZBool isSytemDefined)
		{
			serviceTaskOption.ContainerMode = containerMode;
			serviceTaskOption.TargetModule = targetModule;
			serviceTaskOption.IsSystemDefined = isSytemDefined;
		}
	}
}
