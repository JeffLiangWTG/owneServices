using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.LandTransport;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Test.Common
{
	class BookingToTransportJobCommonCreatorTest : DtbBookingTestCaseWithFactory
	{
		// SELECT KT_Code, KT_Direction, KT_RatingFreightMode, KT_Description
		// FROM dbo.DtbBookingTmpl
		// ORDER BY KT_RatingFreightMode, KT_Direction, KT_Description
		IEnumerable<TemplateAndResult> GetAllTemplatesAndExpectedResults()
		{
			var cnt = AutoCreatorContainerModes.Codes.Container;
			var lse = AutoCreatorContainerModes.Codes.Loose;
			var mix = AutoCreatorContainerModes.Codes.MixedCargo;

			var mixOnly = new[] { mix };
			var lseOnly = new[] { lse };
			var cntOnly = new[] { cnt };

			// export

			var c_cyd_cfs = new Leg('C', "LCY", "LCF");
			var c_cyd_cnr = new Leg('C', "LCY", "LCE");
			var c_cfs_cto = new Leg('C', "LCF", "LCT");
			var c_cnr_cto = new Leg('C', "LCE", "LCT");
			var l_cnr_cfs = new Leg('L', "LCE", "LCF");

			// import
			var c_cto_cfs = new Leg('C', "LCT", "LCF");
			var c_cto_cne = new Leg('C', "LCT", "LCI");
			var c_cfs_cne = new Leg('C', "LCF", "LCI");
			var c_cfs_cyd = new Leg('C', "LCF", "LCY");
			var c_cne_cyd = new Leg('C', "LCI", "LCY");
			var l_cfs_cne = new Leg('L', "LCF", "LCI");

			// special
			var l_cnr_cne = new Leg('L', "LCE", "LCI");
			var c_cnr_cne = new Leg('C', "LCE", "LCI");

			yield return new TemplateAndResult("IFUD", mixOnly, 0, new[] { c_cto_cfs, c_cfs_cyd, l_cfs_cne });                 // IFUD		DST		BTH		Import FCL/ULD, Unpack at CFS, Deliver Loose to CNE
			yield return new TemplateAndResult("LC2C", mixOnly, 1, new[] { l_cnr_cne, c_cnr_cne }, new[] { "LCE", "LCF", "LCI" }); // LC2C		LOC		BTH		Consignor to Consignee
			yield return new TemplateAndResult("EFPL", mixOnly, 0, new[] { l_cnr_cfs, c_cyd_cfs, c_cfs_cto }, new[] { "LCE", "LCF", "LCI" });  // EFPL		ORG		BTH		Export FCL/ULD, Pack at CFS, Pickup Loose from CNR
			yield return new TemplateAndResult("IECS", cntOnly, 0, new[] { c_cfs_cyd });                           // IECS		DST		CNT		Import CFS to Empty
			yield return new TemplateAndResult("IECE", cntOnly, 0, new[] { c_cne_cyd });                           // IECE		DST		CNT		Import CNE to Empty
			yield return new TemplateAndResult("IFCY", cntOnly, 0, new[] { new Leg('C', "LCE", "LCF"), new Leg('C', "LCF", "LCY") });      // IFCY		DST		CNT		Import FCL, CNR to CFS ==> CYD ??
			yield return new TemplateAndResult("IFCX", cntOnly, 0, new[] { new Leg('C', "LCE", "LCI"), c_cne_cyd });             // IFCX		DST		CNT		Import FCL, CNR to CNE ==> CYD ??
			yield return new TemplateAndResult("IFSD", cntOnly, 0, new[] { c_cto_cfs, c_cfs_cne, c_cne_cyd });                 // IFSD		DST		CNT		Import FCL, Stage and Deliver
			yield return new TemplateAndResult("IFFD", cntOnly, 0, new[] { c_cto_cfs, c_cfs_cyd });                      // IFFD		DST		CNT		Import FCL/ULD, Unpack at CFS
			yield return new TemplateAndResult("IFCS", cntOnly, 0, new[] { c_cto_cfs });                           // IFCS		DST		CNT		Import FCL/ULD, Unpack at CFS, No Empty
			yield return new TemplateAndResult("IFCD", cntOnly, 0, new[] { c_cto_cne, c_cne_cyd });                      // IFCD		DST		CNT		Import FCL/ULD, Unpack at CNE
			yield return new TemplateAndResult("IFCE", cntOnly, 0, new[] { c_cto_cne });                           // IFCE		DST		CNT		Import FCL/ULD, Unpack at CNE, No Empty
			yield return new TemplateAndResult("PFCW", cntOnly, 1, new[] { new Leg('C', "LCE", "LCW") }, new[] { "LCE", "LCF", "LCI" });    // PFCW		LOC		CNT		Warehouse Receive Pickup Full Container
			yield return new TemplateAndResult("EECS", cntOnly, 0, new[] { c_cyd_cfs });                           // EECS		ORG		CNT		Export Empty to CFS
			yield return new TemplateAndResult("EECR", cntOnly, 0, new[] { c_cyd_cnr });                           // EECR		ORG		CNT		Export Empty to CNR
			yield return new TemplateAndResult("EFPS", cntOnly, 0, new[] { c_cyd_cfs, c_cfs_cto });                      // EFPS		ORG		CNT		Export FCL/ULD, Pack at CFS
			yield return new TemplateAndResult("EFCS", cntOnly, 0, new[] { c_cfs_cto });                           // EFCS		ORG		CNT		Export FCL/ULD, Pack at CFS, No Empty
			yield return new TemplateAndResult("EFPR", cntOnly, 0, new[] { c_cyd_cnr, c_cnr_cto });                      // EFPR		ORG		CNT		Export FCL/ULD, Pack at CNR
			yield return new TemplateAndResult("EFCR", cntOnly, 0, new[] { c_cnr_cto });                           // EFCR		ORG		CNT		Export FCL/ULD, Pack at CNR, No Empty
			yield return new TemplateAndResult("IFDS", lseOnly, 1, new[] { l_cnr_cfs }, new[] { "LCE", "LCF", "LCI" });             // IFDS		DST		LSE		Import FTL, CNR to CFS
			yield return new TemplateAndResult("IFDR", lseOnly, 1, new[] { l_cnr_cne }, new[] { "LCE", "LCF", "LCI" });             // IFDR		DST		LSE		Import FTL, CNR to CNE
			yield return new TemplateAndResult("ILDV", lseOnly, 1, new[] { l_cfs_cne }, new[] { "LCE", "LCF", "LCI" });             // ILDV		DST		LSE		Import LCL/LSE/LTL Delivery
			yield return new TemplateAndResult("MPDV", lseOnly, 3, new[] { l_cfs_cne }, new[] { "LCE", "LCF", "LCI" });             // MPDV		DST		LSE		Multi Point Delivery
			yield return new TemplateAndResult("DLCW", lseOnly, 1, new[] { new Leg('L', "LCW", "LCI") }, new[] { "LCE", "LCF", "LCI" });    // DLCW		DST		LSE		Warehouse Release
			yield return new TemplateAndResult("LHCC", lseOnly, 1, new[] { new Leg('L', "LCF", "LCF") }, new[] { "LCE", "LCF" });       // LHCC		LOC		LSE		Line Haul, CFS to CFS
			yield return new TemplateAndResult("PLCW", lseOnly, 1, new[] { new Leg('L', "LCE", "LCW") }, new[] { "LCE", "LCF", "LCI" });    // PLCW		LOC		LSE		Warehouse Receive Pickup Loose
			yield return new TemplateAndResult("EFDS", lseOnly, 1, new[] { l_cfs_cne }, new[] { "LCE", "LCF", "LCI" });             // EFDS		ORG		LSE		Export FTL, CFS to CNE
			yield return new TemplateAndResult("EFDR", lseOnly, 1, new[] { l_cnr_cne }, new[] { "LCE", "LCF", "LCI" });             // EFDR		ORG		LSE		Export FTL, CNR to CNE
			yield return new TemplateAndResult("EFPU", lseOnly, 1, new[] { l_cnr_cfs }, new[] { "LCE", "LCF", "LCI" });             // EFPU		ORG		LSE		Export LCL/LSE/LTL Pickup
			yield return new TemplateAndResult("ELRT", lseOnly, 1, new[] { new Leg('L', "LCF", "LCE") }, new[] { "LCE", "LCF", "LCI" });    // ELRT		ORG		LSE		Export LCL/LSE/LTL Return to CNR
			yield return new TemplateAndResult("MPPU", lseOnly, 3, new[] { l_cnr_cfs }, new[] { "LCE", "LCF" });                // MPPU		ORG		LSE		Multi Point Pickup
		}

		struct TemplateAndResult
		{
			public TemplateAndResult(ZString template, string[] usesTheseRegistryContainerModes, int expectedConsignments, Leg[] expectedLegs, string[] expectedConsignmentInstructions = null)
			{
				this.Template = template;
				this.UsesTheseRegistryContainerModes = usesTheseRegistryContainerModes;
				this.ExpectedConsignments = expectedConsignments;
				this.ExpectedLegs = expectedLegs;
				this.ExpectedConsignmentInstructions = expectedConsignmentInstructions;
			}

			public readonly ZString Template;
			public readonly string[] UsesTheseRegistryContainerModes;
			public readonly int ExpectedConsignments;
			public readonly Leg[] ExpectedLegs; // ie ['LCI','LCY'],
			public readonly string[] ExpectedConsignmentInstructions; // ie ['LCI','LCY'],
		}

		struct Leg
		{
			public Leg(char containerMode, string pickup, string delivery)
			{
				IsContainerised = containerMode == 'C';
				Pickup = pickup;
				Delivery = delivery;
			}

			public readonly bool IsContainerised;
			public readonly string Pickup;
			public readonly string Delivery;
		}

		protected override void SetUp()
		{
			base.SetUp();
			BookingToTransportJobCommonCreator.FailureContextToFakeWhenProcessingFailures = null;
		}

		public void TestCreateTransportJobFromUnsavedBooking()
		{
			var newBooking = Factory.New<DtbBooking>();

			var buffer = new NotificationBuffer();
			var creator = new BookingToTransportJobCommonCreator(buffer);
			creator.TryCreateTransportJobsFromTransportBookings(new[] { newBooking });

			AssertTransportJobIsNotCreatedFromBooking(newBooking, buffer, "Please save the Booking before creating a Transport Job.");
		}

		public void TestCreateTransportJobFromHasChangesBooking()
		{
			var bookingWithChanges = Factory.NewWithValidTestData<DtbBooking>();
			Factory.Save();

			bookingWithChanges.KM_Description = "The description has been changed!";
			Assert("Precondition - Booking is in the DB but has changes.", bookingWithChanges.IsInDatabase && bookingWithChanges.HasChanges);

			var buffer = new NotificationBuffer();
			var creator = new BookingToTransportJobCommonCreator(buffer);
			creator.TryCreateTransportJobsFromTransportBookings(new[] { bookingWithChanges });

			AssertTransportJobIsNotCreatedFromBooking(bookingWithChanges, buffer, "Please save the Booking before creating a Transport Job.");
		}

		public void TestCreatePortTransportJobFromBookingWithDeactivatedLandTransportConsignmentWhenTBHasBeenMadeAvailable()
		{
			var now = new ZDateTime(ZDateTime.Now, DateTimeKind.Utc);
			var dummy = Factory.New<DummyWithDtbBooking>();
			var orgProxyTransportCompany = GlbCompany.CurrentCompany.OrgProxy;
			var branchTransportCompany = Factory.NewWithValidTestData<OrgHeader>();
			var branch = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).Branches.AddNew();
			branch.GB_Code = "~ZZ";
			branch.GB_OH_OrgProxy = branchTransportCompany.PK;
			var validConfirmationDate = now.AddHours(1); // one hour in the future
			var validJobCreatedDate = now.AddDays(-10).AddMinutes(-30); // 10 days, 30 minutes old

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummy);
			var buffer = new NotificationBuffer();
			var creator = new BookingToTransportJobCommonCreator(buffer);
			booking.Factory.Save();

			using (TransportRegistry.Instance.EnableLandTransport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SetupServiceTaskCreatorOptionRegistry(AutoCreatorTargetModules.Codes.LandTransportConsignment, AutoCreatorTargetModules.Codes.LandTransportConsignment, AutoCreatorTargetModules.Codes.LandTransportConsignment, AutoCreatorTargetModules.Codes.LandTransportConsignment))
			{
				creator.TryCreateTransportJobsFromTransportBookings(new[] { booking });
				var consignmentQuery = new ZDBOnlyQuery(typeof(IDtbConsignment));
				consignmentQuery.AddToFilter(DtbConsignmentSchema.LTC_KM_Booking, booking.PK);
				var cancelledLTC = Factory.Load<IDtbConsignment>(consignmentQuery).FirstOrDefault();
				cancelledLTC.LTC_IsActive = false;
				booking.KM_Status = BookingStatuses.Codes.Available;
				booking.Factory.Save();
			}
			AssertEquals("Precondition: Booking should now have an inactive LTC attached", false, booking.GetLandTransportJobs().First().LTC_IsActive);

			using (SetupServiceTaskCreatorOptionRegistry(AutoCreatorTargetModules.Codes.PortTransport, AutoCreatorTargetModules.Codes.PortTransport, AutoCreatorTargetModules.Codes.PortTransport, AutoCreatorTargetModules.Codes.PortTransport))
			{
				creator.TryCreateTransportJobsFromTransportBookings(new[] { booking });
				AssertBookingHasAttachedPortTransportJob(booking);
			}
		}

		public void TestCreateLandTransportConsignmentFromBookingWithDeactivatedPortTransportJobWhenTBHasBeenMadeAvailable()
		{
			var now = new ZDateTime(ZDateTime.Now, DateTimeKind.Utc);
			var dummy = Factory.New<DummyWithDtbBooking>();
			var orgProxyTransportCompany = GlbCompany.CurrentCompany.OrgProxy;
			var branchTransportCompany = Factory.NewWithValidTestData<OrgHeader>();
			var branch = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).Branches.AddNew();
			branch.GB_Code = "~ZZ";
			branch.GB_OH_OrgProxy = branchTransportCompany.PK;
			var validConfirmationDate = now.AddHours(1); // one hour in the future
			var validJobCreatedDate = now.AddDays(-10).AddMinutes(-30); // 10 days, 30 minutes old

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummy);
			booking.Factory.Save();
			var buffer = new NotificationBuffer();
			var creator = new BookingToTransportJobCommonCreator(buffer);
			using (SetupServiceTaskCreatorOptionRegistry(AutoCreatorTargetModules.Codes.PortTransport, AutoCreatorTargetModules.Codes.PortTransport, AutoCreatorTargetModules.Codes.PortTransport, AutoCreatorTargetModules.Codes.PortTransport))
			{
				creator.TryCreateTransportJobsFromTransportBookings(new[] { booking });
				var cancelledPT = AssertBookingHasAttachedPortTransportJob(booking);
				booking.GetPortTransportJobs().First().JJ_IsCancelled = true;
				booking.KM_Status = BookingStatuses.Codes.Available;
				booking.Factory.Save();
			}
			AssertEquals("Precondition: Booking should no longer have an active PT attached", true, booking.GetPortTransportJobs().FirstOrDefault()?.JJ_IsCancelled ?? true);

			using (TransportRegistry.Instance.EnableLandTransport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SetupServiceTaskCreatorOptionRegistry(AutoCreatorTargetModules.Codes.LandTransportConsignment, AutoCreatorTargetModules.Codes.LandTransportConsignment, AutoCreatorTargetModules.Codes.LandTransportConsignment, AutoCreatorTargetModules.Codes.LandTransportConsignment))
			{
				creator.TryCreateTransportJobsFromTransportBookings(new[] { booking });
				AssertEquals("Booking should now have an active LTC attached", true, booking.GetLandTransportJobs().First().LTC_IsActive);
			}
		}

		public void TestCreateTransportJobsFromMultipleBookingsWithSomeHavingChanges()
		{
			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper);
			var bookingWithChanges = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper);
			Factory.Save();
			bookingWithChanges.KM_Description = "The description has been changed!";

			Assert("Precondition - Booking with changes is in the DB but has changes.", bookingWithChanges.IsInDatabase && bookingWithChanges.HasChanges);
			Assert("Precondition - Booking without changes is in the DB and has no changes.", booking.IsInDatabase && !booking.HasChanges);

			var buffer = new NotificationBuffer();
			var creator = new BookingToTransportJobCommonCreator(buffer);
			creator.TryCreateTransportJobsFromTransportBookings(new[] { booking, bookingWithChanges });

			var errors = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Select(w => w.Message);

			AssertTransportJobIsNotCreatedFromBooking(bookingWithChanges, buffer, "Please save the Booking before creating a Transport Job.");
			AssertBookingHasAttachedPortTransportJob(booking);
		}

		public void TestCreateTransportJobFromUnsavedBooking_WhileAnotherValidBookingExistsInDatabase()
		{
			var serviceTaskOptions = new ServiceTaskCreatorOptionCollection();
			var serviceTaskOption = serviceTaskOptions.AddNew();
			serviceTaskOption.ContainerMode = AutoCreatorContainerModes.Codes.Loose;
			serviceTaskOption.TargetModule = AutoCreatorTargetModules.Codes.PortTransport;
			serviceTaskOption.IsSystemDefined = false;

			var now = new ZDateTime(ZDateTime.Now, DateTimeKind.Utc);
			var dummyParentForUnrelatedValidBooking = Factory.New<DummyWithDtbBooking>();
			var dummyParentForBookingWithChanges = Factory.New<DummyWithDtbBooking>();
			var branchTransportCompany = Factory.NewWithValidTestData<OrgHeader>();
			var branch = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).Branches.AddNew();
			branch.GB_Code = "~ZZ";
			branch.GB_OH_OrgProxy = branchTransportCompany.PK;
			var validConfirmationDate = now.AddHours(1); // one hour in the future
			var validJobCreatedDate = now.AddDays(-10).AddMinutes(-30); // 10 days, 30 minutes old

			var unrelatedValidBooking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyParentForUnrelatedValidBooking);
			var bookingWithChanges = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyParentForBookingWithChanges);

			BookingToTransportJobCreatorTestHelper.SetDatesRelatedToAutomaticTransportJobCreationForBooking(Factory, unrelatedValidBooking, validJobCreatedDate, validConfirmationDate);
			BookingToTransportJobCreatorTestHelper.SetDatesRelatedToAutomaticTransportJobCreationForBooking(Factory, bookingWithChanges, validJobCreatedDate, validConfirmationDate);

			Factory.Save();

			bookingWithChanges.KM_Description = "The description has been changed!";

			Assert("Precondition - Booking with changes is in the DB but has changes.", bookingWithChanges.IsInDatabase && bookingWithChanges.HasChanges);
			Assert("Precondition - Unrelated valid booking is in the DB and has no changes.", unrelatedValidBooking.IsInDatabase && !unrelatedValidBooking.HasChanges);

			using (TransportRegistry.Instance.ServiceTaskCreatorOption.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, serviceTaskOptions))
			using (TransportRegistry.Instance.AutoCreateTransportConsignmentsCreationPeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			using (TransportRegistry.Instance.CreateTransportJobsFromTransportBookingEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, now.AddDays(-15).ToDateTime()))
			using (TransportRegistry.Instance.CreateTransportJobsFromTransportBookingWithinPeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 30))
			{
				var buffer = new NotificationBuffer();
				var creator = new BookingToTransportJobCommonCreator(buffer);
				creator.TryCreateTransportJobsFromTransportBookings(new[] { bookingWithChanges });

				var errors = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Select(w => w.Message);

				CombineAssertions(() =>
				{
					AssertTransportJobIsNotCreatedFromBooking(bookingWithChanges, buffer, "Please save the Booking before creating a Transport Job.");
					AssertTransportJobIsNotCreatedFromBooking(unrelatedValidBooking, buffer);
				});
			}
		}

		void AssertTransportJobIsNotCreatedFromBooking(DtbBooking booking, NotificationBuffer buffer, string expectedLogContent = null)
		{
			if (expectedLogContent != null)
			{
				var errors = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Select(w => w.Message);
				AssertMultilineASCIIEquals("Should log creation of Transport Bookings.", expectedLogContent, string.Join("\n", errors));
			}

			AssertBookingHasNoAttachedPortTransports(booking);
		}

		public void TestEnsureAllBookingTemplatesUseCorrectRegistryContainerMode()
		{
			var tests = GetAllTemplatesAndExpectedResults();
			foreach (var test in tests)
			{
				var codes = new[]
{
	AutoCreatorTargetModules.Codes.PortTransport,
	AutoCreatorTargetModules.Codes.LandTransportConsignment
};

				foreach (var code1 in codes)
				{
					foreach (var code2 in codes)
					{
						foreach (var code3 in codes)
						{
							foreach (var code4 in codes)
							{
								AssertBookingUseCorrectRegistry(test, code1, code2, code3, code4);
							}
						}
					}
				}
			}
		}

		void AssertBookingUseCorrectRegistry(TemplateAndResult test, ZString cntTarget, ZString ftlTarget, ZString lseTarget, ZString mixTarget)// Remove this test ?
		{
			var registryContainerModeTargetsThatMatch = new List<ZString>();

			if (test.UsesTheseRegistryContainerModes.Contains(AutoCreatorContainerModes.Codes.MixedCargo))
			{
				registryContainerModeTargetsThatMatch.Add(mixTarget);
			}
			else if (test.UsesTheseRegistryContainerModes.Contains(AutoCreatorContainerModes.Codes.FTL))
			{
				registryContainerModeTargetsThatMatch.Add(ftlTarget);
			}
			else if (test.UsesTheseRegistryContainerModes.Contains(AutoCreatorContainerModes.Codes.Container))
			{
				registryContainerModeTargetsThatMatch.Add(cntTarget);
			}
			else if (test.UsesTheseRegistryContainerModes.Contains(AutoCreatorContainerModes.Codes.Loose))
			{
				registryContainerModeTargetsThatMatch.Add(lseTarget);
			}

			var expectedTarget = registryContainerModeTargetsThatMatch.Distinct().Count() == 1 ? registryContainerModeTargetsThatMatch.First().ToString() : AutoCreatorTargetModules.Codes.PortTransport;
			var expectedTargetDataContext = expectedTarget == AutoCreatorTargetModules.Codes.LandTransportConsignment ? DataContextType.LandTransportConsignmentConsol : DataContextType.LocalTransport;

			using (SetupServiceTaskCreatorOptionRegistry(cntTarget, ftlTarget, lseTarget, mixTarget))
			{
				var booking = CreateNewBookingWithTemplate(test.Template); // this creates both loose and containers, so they all do mix
				var actualTargetDataContext = Enterprise.TransportBookings.Business.BookingToTransportJobCommonCreator.TargetModuleDataObjectWriter.GetBookingDataTarget(booking);
				AssertEquals(string.Format(
@"Registry CNT '{0}' FTL '{1}' LSE '{2}' MIX '{3}'
Template '{3}' :: Supports '{4}'", cntTarget, ftlTarget, lseTarget, mixTarget, test.Template, string.Join(", ", test.UsesTheseRegistryContainerModes)), expectedTargetDataContext, actualTargetDataContext);
			}
		}

		[TestDate(2013, 1, 1, 1, 20, 0)]
		public void TestBookingTemplatesCreatePortTransportJobs()
		{
			using (SetupServiceTaskCreatorOptionRegistry(AutoCreatorTargetModules.Codes.PortTransport, AutoCreatorTargetModules.Codes.PortTransport, AutoCreatorTargetModules.Codes.PortTransport, AutoCreatorTargetModules.Codes.PortTransport))
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11)) // 12 doesn't create instructions!
			{
				var bookingTemplates = GetAllTemplatesAndExpectedResults();
				foreach (var bookingTest in bookingTemplates)
				{
					TestBookingTemplateCreatesPortTransportJob(bookingTest);
				}
			}
		}

		void TestBookingTemplateCreatesPortTransportJob(TemplateAndResult test)
		{
			var booking = CreateNewBookingWithTemplate(test.Template);
			booking.Factory.Save();

			var buffer = new NotificationBuffer();
			var processingManager = new BookingToTransportJobCommonCreator(buffer);
			processingManager.TryCreateTransportJobsFromTransportBookings(new[] { booking });

			CombineAssertions(() =>
			{
				var information = string.Join("\n", buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Warning).Select(w => w.Message));
				var error = string.Join("\n", buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Select(w => w.Message));

				AssertMultilineASCIIEquals(test.Template + ": Should be no errors.", "", error);
				var portTransportJob = (Freight.LocalCartage.Integration.ICommonCartage)AssertBookingHasAttachedPortTransportJob(booking);
				var expectedInformation = string.Format("Successfully created Port Transport {0} from Transport Booking {1}", portTransportJob.JJ_ConsignmentID, booking.KM_JobID);
				AssertMultilineASCIIEquals(test.Template + ": Should log creation of Consignment.", expectedInformation, information);

				var expectedContainerLegs = test.ExpectedLegs.Where(l => l.IsContainerised);
				var expectedLooseLegs = test.ExpectedLegs.Where(l => !l.IsContainerised);
				var legs = portTransportJob.Legs;
				var actualContainerLegs = legs.Where(l => !l.ContainerDescription.IsEmpty);
				var actualLooseLegs = legs.Where(l => l.ContainerDescription.IsEmpty);

				if (expectedContainerLegs.Any())
				{
					Assert(test.Template + ": Has container Legs", actualContainerLegs.Any());

					foreach (var container in booking.Containers)
					{
						var actualSpecificContainerLegs = actualContainerLegs.Where(l => l.ContainerDescription.Contains(container.Package.KP_PackageID)).OrderBy(l => l.JU_DisplayOrder);
						AssertEquals(test.Template + ": expected Container Leg Count", expectedContainerLegs.Count(), actualSpecificContainerLegs.Count());

						for (int i = 0; i < expectedContainerLegs.Count(); i++)
						{
							var expectedLeg = expectedContainerLegs.ElementAt(i);
							var actualLeg = actualSpecificContainerLegs.ElementAt(i);

							var actualPickupAddress = Factory.Load<JobDocAddress>(actualLeg.JU_E2PickupAddressID);
							var actualDeliveryAddress = Factory.Load<JobDocAddress>(actualLeg.JU_E2DeliveryAddressID);

							AssertEquals(test.Template + ": Containerised LegPickup", expectedLeg.Pickup, actualPickupAddress.E2_AddressType);
							AssertEquals(test.Template + ": Containerised LegDelivery", expectedLeg.Delivery, actualDeliveryAddress.E2_AddressType);
						}
					}
				}

				if (expectedLooseLegs.Any())
				{
					Assert(test.Template + ": Has loose Legs", expectedLooseLegs.Any());
					foreach (var package in booking.LoosePackages)
					{
						var actualSpecificLooseLegs = actualLooseLegs.Where(l => l.TotalPackages == package.Package.KP_PackageQty && l.TotalPackagesUnit == package.Package.KP_F3_NKPackType).OrderBy(l => l.JU_DisplayOrder); // assume all package have unique pack count & type
						AssertEquals(test.Template + ": expected Loose Leg Count", expectedLooseLegs.Count(), actualSpecificLooseLegs.Count());

						for (int i = 0; i < expectedLooseLegs.Count(); i++)
						{
							var expectedLeg = expectedLooseLegs.ElementAt(i);
							var actualLeg = actualSpecificLooseLegs.ElementAt(i);

							var actualPickupAddress = Factory.Load<JobDocAddress>(actualLeg.JU_E2PickupAddressID);
							var actualDeliveryAddress = Factory.Load<JobDocAddress>(actualLeg.JU_E2DeliveryAddressID);

							var actualPickupAddressCode = actualPickupAddress != null ? actualPickupAddress.E2_AddressType : ZString.Empty;
							var actualDeliveryAddressCode = actualDeliveryAddress != null ? actualDeliveryAddress.E2_AddressType : ZString.Empty;
							AssertEquals(test.Template + ": Loose LegPickup", expectedLeg.Pickup, actualPickupAddressCode);
							AssertEquals(test.Template + ": Loose LegDelivery", expectedLeg.Delivery, actualDeliveryAddressCode);
						}
					}
				}
			});
		}

		void AssertBookingHasNoAttachedTransportJobs(DtbBooking booking)
		{
			AssertBookingHasNoAttachedPortTransports(booking);
			AssertBookingNasNoAttachedLandTransportConsignment(booking);
		}

		BusinessObject AssertBookingHasAttachedPortTransportJob(DtbBooking booking)
		{
			return AssertBookingHasAttachedPortTransports(booking, 1);
		}

		BusinessObject AssertBookingHasNoAttachedPortTransports(DtbBooking booking)
		{
			return AssertBookingHasAttachedPortTransports(booking, 0);
		}

		BusinessObject AssertBookingHasAttachedPortTransports(DtbBooking booking, int expectedNumberOfAttachedTransportJobs)
		{
			var query = new ZQuery(JobCartageSchema.JJ_ParentID, booking.PK);
			query.AddToFilter(JobCartageSchema.JJ_ParentTableCode, booking.TablePrefix);

			var portTransports = Factory.Load<Freight.LocalCartage.Integration.ICommonCartage>(query);
			AssertEquals(booking.HumanReadableName + " should have " + expectedNumberOfAttachedTransportJobs + " attached Transport Jobs.", expectedNumberOfAttachedTransportJobs, portTransports.Length);

			var portTransport = (EnterpriseBusinessObject)portTransports.FirstOrDefault();

			if (expectedNumberOfAttachedTransportJobs > 0)
			{
				var expectedTransportReference = portTransport[JobCartageSchema.Constants.JJ_ConsignmentID];
				var serviceLogs = portTransport.Logs.GetAllLogs().Cast<StmALog>().Where(l => l.SL_SE_NKEvent == AutoEvents.ServiceCommencedCode);
				AssertEquals(expectedNumberOfAttachedTransportJobs, serviceLogs.Count());
				AssertEquals(expectedTransportReference, serviceLogs.ElementAt(0).ReferenceFreeText);

				AssertEquals(TransportStatuses.Codes.ServiceCommenced, new BusinessObjectFactory().Load<DtbBooking>(booking.PK).KM_Status);
			}

			return portTransport;
		}

		[TestDate(1900, 1, 1)]
		public void TestCreateTransportJobsFromTransportBookings_DoesNotThrowErrorForMinSmallDateTimeNow()
		{
			CoreTestCreateTransportJobsFromTransportBookings_DoesNotThrowErrorForOutOfRangeNow();
		}

		[TestDate(1753, 1, 1)]
		public void TestCreateTransportJobsFromTransportBookings_DoesNotThrowErrorForMinSqlDateTimeNow()
		{
			CoreTestCreateTransportJobsFromTransportBookings_DoesNotThrowErrorForOutOfRangeNow();
		}

		[TestDate(2079, 6, 8)]
		public void TestCreateTransportJobsFromTransportBookings_DoesNotThrowErrorForMaxSmallDateTimeNow()
		{
			CoreTestCreateTransportJobsFromTransportBookings_DoesNotThrowErrorForOutOfRangeNow();
		}

		[TestDate(9999, 12, 31)]
		public void TestCreateTransportJobsFromTransportBookings_DoesNotThrowErrorForMaxSqlDateTimeNow()
		{
			CoreTestCreateTransportJobsFromTransportBookings_DoesNotThrowErrorForOutOfRangeNow();
		}

		void CoreTestCreateTransportJobsFromTransportBookings_DoesNotThrowErrorForOutOfRangeNow()
		{
			var serviceTaskOptions = new ServiceTaskCreatorOptionCollection();
			var serviceTaskOption = serviceTaskOptions.AddNew();
			serviceTaskOption.ContainerMode = AutoCreatorContainerModes.Codes.Loose;
			serviceTaskOption.TargetModule = AutoCreatorTargetModules.Codes.LandTransportConsignment;
			serviceTaskOption.IsSystemDefined = false;

			using (TransportRegistry.Instance.ServiceTaskCreatorOption.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, serviceTaskOptions))
			using (TransportRegistry.Instance.AutoCreateTransportConsignmentsGracePeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1)) // create if saved 1 minutes ago
			using (TransportRegistry.Instance.AutoCreateTransportConsignmentsCreationPeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 11)) // create if due in the next 11 hours
			using (TransportRegistry.Instance.CreateTransportJobsFromTransportBookingEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ZDateTime(2020, 2, 3).AddDays(-10).ToDateTime()))
			using (TransportRegistry.Instance.CreateTransportJobsFromTransportBookingWithinPeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 90))
			{
				var buffer = new NotificationBuffer();
				var creator = new BookingToTransportJobCommonCreator(buffer);
				AssertNoExceptionThrown(FormattableString.Invariant($@"Should not throw exception when calling CreateTransportJobsFromTransportBookings() when ZDateTime.UtcNow is {TestDateAttribute.Date.Day}/{TestDateAttribute.Date.Month}/{TestDateAttribute.Date.Year}"), () => creator.CreateTransportJobsFromTransportBookings());
			}
		}

		[TestDate(2024, 10, 10)]
		public void TestCreateTransportJobsFromTransportBookings_CreateTransportJobsFromTransportBookingEffectiveDate_MinValue_DoesNotThrowError()
		{
			var serviceTaskOptions = new ServiceTaskCreatorOptionCollection();
			var serviceTaskOption = serviceTaskOptions.AddNew();
			serviceTaskOption.ContainerMode = AutoCreatorContainerModes.Codes.Loose;
			serviceTaskOption.TargetModule = AutoCreatorTargetModules.Codes.LandTransportConsignment;
			serviceTaskOption.IsSystemDefined = false;

			var createTransportJobsFromTransportBookingEffectiveDate = DateTime.MinValue;

			using (TransportRegistry.Instance.ServiceTaskCreatorOption.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, serviceTaskOptions))
			using (TransportRegistry.Instance.AutoCreateTransportConsignmentsGracePeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1)) // create if saved 1 minutes ago
			using (TransportRegistry.Instance.AutoCreateTransportConsignmentsCreationPeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 11)) // create if due in the next 11 hours
			using (TransportRegistry.Instance.CreateTransportJobsFromTransportBookingEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, createTransportJobsFromTransportBookingEffectiveDate))
			using (TransportRegistry.Instance.CreateTransportJobsFromTransportBookingWithinPeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 90))
			{
				var buffer = new NotificationBuffer();
				var creator = new BookingToTransportJobCommonCreator(buffer);
				AssertNoExceptionThrown("Should not throw exception when calling CreateTransportJobsFromTransportBookings()",
					() => creator.CreateTransportJobsFromTransportBookings()
				);
			}
		}

		[TestDate(2024, 10, 10)]
		public void TestCreateTransportJobsFromTransportBookings_CreateTransportJobsFromTransportBookingEffectiveDate_MaxValue_DoesNotThrowError()
		{
			var serviceTaskOptions = new ServiceTaskCreatorOptionCollection();
			var serviceTaskOption = serviceTaskOptions.AddNew();
			serviceTaskOption.ContainerMode = AutoCreatorContainerModes.Codes.Loose;
			serviceTaskOption.TargetModule = AutoCreatorTargetModules.Codes.LandTransportConsignment;
			serviceTaskOption.IsSystemDefined = false;

			var createTransportJobsFromTransportBookingEffectiveDate = DateTime.MaxValue;

			using (TransportRegistry.Instance.ServiceTaskCreatorOption.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, serviceTaskOptions))
			using (TransportRegistry.Instance.AutoCreateTransportConsignmentsGracePeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1)) // create if saved 1 minutes ago
			using (TransportRegistry.Instance.AutoCreateTransportConsignmentsCreationPeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 11)) // create if due in the next 11 hours
			using (TransportRegistry.Instance.CreateTransportJobsFromTransportBookingEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, createTransportJobsFromTransportBookingEffectiveDate))
			using (TransportRegistry.Instance.CreateTransportJobsFromTransportBookingWithinPeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 90))
			{
				var buffer = new NotificationBuffer();
				var creator = new BookingToTransportJobCommonCreator(buffer);
				AssertNoExceptionThrown("Should not throw exception when calling CreateTransportJobsFromTransportBookings()",
					() => creator.CreateTransportJobsFromTransportBookings()
				);
			}
		}

		public void TestGetBookingDataTarget_Returns_LandTransportConsignment_DataContext()
		{
			var now = ZDateTime.Now;

			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var booking = Helper.CreateBooking();
				Factory.Save();

				var serviceTaskOptions = new ServiceTaskCreatorOptionCollection();
				var serviceTaskOption = serviceTaskOptions.AddNew();
				serviceTaskOption.ContainerMode = AutoCreatorContainerModes.Codes.Loose;
				serviceTaskOption.TargetModule = AutoCreatorTargetModules.Codes.LandTransportConsignment;
				serviceTaskOption.IsSystemDefined = false;

				using (TransportRegistry.Instance.ServiceTaskCreatorOption.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, serviceTaskOptions))
				{
					var actualTargetDataContext = BookingToTransportJobCommonCreator.TargetModuleDataObjectWriter.GetBookingDataTarget(booking);
					AssertEquals(DataContextType.LandTransportConsignmentConsol, actualTargetDataContext);
				}
			}
		}

		public void TestCreateLandTransportJob_MaximumJobAgeAllowed_CreateTransportJobsFromTransportBookingEffectiveDate()
		{
			var now = new ZDateTime(ZDateTime.Now, DateTimeKind.Utc);
			var createTransportJobsFromTransportBookingEffectiveDate = now.AddDays(-15).ToDateTime();
			var createTransportJobsFromTransportBookingWithinPeriod = 30;
			var assertionMessage = "Should reject booking2 due to CreateTransportJobsFromTransportBookingEffectiveDate";
			CoreTestCreateLandTransportJob_MaximumJobAgeAllowed(createTransportJobsFromTransportBookingEffectiveDate, createTransportJobsFromTransportBookingWithinPeriod, assertionMessage);
		}

		public void TestCreateLandTransportJobByAutomaticallyFindingValidBookings()
		{
			var serviceTaskOptions = new ServiceTaskCreatorOptionCollection();
			var serviceTaskOption = serviceTaskOptions.AddNew();
			serviceTaskOption.ContainerMode = AutoCreatorContainerModes.Codes.Loose;
			serviceTaskOption.TargetModule = AutoCreatorTargetModules.Codes.LandTransportConsignment;
			serviceTaskOption.IsSystemDefined = false;

			var now = new ZDateTime(ZDateTime.Now, DateTimeKind.Utc);
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();
			var branchTransportCompany = Factory.NewWithValidTestData<OrgHeader>();
			var branch = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).Branches.AddNew();
			branch.GB_Code = "~ZZ";
			branch.GB_OH_OrgProxy = branchTransportCompany.PK;
			var validConfirmationDate = now.AddHours(1); // one hour in the future
			var validJobCreatedDate = now.AddDays(-10).AddMinutes(-30); // 10 days, 30 minutes old

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);
			BookingToTransportJobCreatorTestHelper.SetDatesRelatedToAutomaticTransportJobCreationForBooking(Factory, booking, validJobCreatedDate, validConfirmationDate);

			Factory.Save();

			using (TransportRegistry.Instance.ServiceTaskCreatorOption.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, serviceTaskOptions))
			using (TransportRegistry.Instance.AutoCreateTransportConsignmentsCreationPeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			using (TransportRegistry.Instance.CreateTransportJobsFromTransportBookingEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, now.AddDays(-15).ToDateTime()))
			using (TransportRegistry.Instance.CreateTransportJobsFromTransportBookingWithinPeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 30))
			{
				var buffer = new NotificationBuffer();
				var creator = new BookingToTransportJobCommonCreator(buffer);
				creator.CreateTransportJobsFromTransportBookings();

				var informations = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Warning).Select(w => w.Message);
				CombineAssertions(() =>
				{
					AssertBookingHasAttachedLandTransportConsignment(booking);
					AssertMultilineASCIIEquals("Should log creation of Consignments.", @"
Successfully created Land Transport Consignment CN00000001 from Transport Booking TB00000001
					".Trim(), string.Join("\n", informations));
				});
			}
		}

		public void TestCreateLandTransportJobByManuallySupplyingListOfBookings()
		{
			var now = new ZDateTime(ZDateTime.Now, DateTimeKind.Utc);
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();
			var branchTransportCompany = Factory.NewWithValidTestData<OrgHeader>();
			var branch = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).Branches.AddNew();
			branch.GB_Code = "~ZZ";
			branch.GB_OH_OrgProxy = branchTransportCompany.PK;

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);

			var serviceTaskOptions = new ServiceTaskCreatorOptionCollection();
			var serviceTaskOption = serviceTaskOptions.AddNew();
			serviceTaskOption.ContainerMode = AutoCreatorContainerModes.Codes.Loose;
			serviceTaskOption.TargetModule = AutoCreatorTargetModules.Codes.LandTransportConsignment;
			serviceTaskOption.IsSystemDefined = false;

			Factory.Save();

			using (TransportRegistry.Instance.ServiceTaskCreatorOption.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, serviceTaskOptions))
			{
				var buffer = new NotificationBuffer();
				var creator = new BookingToTransportJobCommonCreator(buffer);
				creator.TryCreateTransportJobsFromTransportBookings(new [] { booking });

				var informations = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Warning).Select(w => w.Message);
				CombineAssertions(() =>
				{
					AssertBookingHasAttachedLandTransportConsignment(booking);
					AssertMultilineASCIIEquals("Should log creation of Consignments.", @"
					Successfully created Land Transport Consignment CN00000001 from Transport Booking TB00000001
					".Trim(), string.Join("\n", informations));
				});
			}
		}

		public void TestCreateLandTransportJob_MaximumJobAgeAllowed_CreateTransportJobsFromTransportBookingWithinPeriod()
		{
			var now = new ZDateTime(ZDateTime.Now, DateTimeKind.Utc);
			var createTransportJobsFromTransportBookingEffectiveDate = now.AddDays(-30).ToDateTime();
			var createTransportJobsFromTransportBookingWithinPeriod = 15;
			var assertionMessage = "Should reject booking2 due to CreateTransportJobsFromTransportBookingWithinPeriod";
			CoreTestCreateLandTransportJob_MaximumJobAgeAllowed(createTransportJobsFromTransportBookingEffectiveDate, createTransportJobsFromTransportBookingWithinPeriod, assertionMessage);
		}

		void CoreTestCreateLandTransportJob_MaximumJobAgeAllowed(DateTime createTransportJobsFromTransportBookingEffectiveDate, int createTransportJobsFromTransportBookingWithinPeriod, string assertionMessage)
		{
			var serviceTaskOptions = new ServiceTaskCreatorOptionCollection();
			var serviceTaskOption = serviceTaskOptions.AddNew();
			serviceTaskOption.ContainerMode = AutoCreatorContainerModes.Codes.Loose;
			serviceTaskOption.TargetModule = AutoCreatorTargetModules.Codes.LandTransportConsignment;
			serviceTaskOption.IsSystemDefined = false;

			var now = new ZDateTime(ZDateTime.Now, DateTimeKind.Utc);
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();
			var orgProxyTransportCompany = GlbCompany.CurrentCompany.OrgProxy;
			var branchTransportCompany = Factory.NewWithValidTestData<OrgHeader>();
			var branch = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).Branches.AddNew();
			branch.GB_Code = "~ZZ";
			branch.GB_OH_OrgProxy = branchTransportCompany.PK;
			var validConfirmationDate = now.AddHours(1); // one hour in the future
			var validJobCreatedDate = now.AddDays(-10).AddMinutes(-30); // 10 days, 30 minutes old
			var invalidJobCreatedDate = now.AddDays(-20); // 20 days old

			var booking1 = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent, consolidationJobDirection: nameof(DtbBookingDirection.PIC));
			var booking2 = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent, consolidationJobDirection: nameof(DtbBookingDirection.DLV));

			BookingToTransportJobCreatorTestHelper.SetDatesRelatedToAutomaticTransportJobCreationForBooking(Factory, booking1, validJobCreatedDate, validConfirmationDate);
			BookingToTransportJobCreatorTestHelper.SetDatesRelatedToAutomaticTransportJobCreationForBooking(Factory, booking2, invalidJobCreatedDate, validConfirmationDate);

			Factory.Save();

			using (TransportRegistry.Instance.ServiceTaskCreatorOption.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, serviceTaskOptions))
			using (TransportRegistry.Instance.AutoCreateTransportConsignmentsCreationPeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			using (TransportRegistry.Instance.CreateTransportJobsFromTransportBookingEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, createTransportJobsFromTransportBookingEffectiveDate))
			using (TransportRegistry.Instance.CreateTransportJobsFromTransportBookingWithinPeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, createTransportJobsFromTransportBookingWithinPeriod))
			{
				var buffer = new NotificationBuffer();
				var creator = new BookingToTransportJobCommonCreator(buffer);
				creator.CreateTransportJobsFromTransportBookings();

				var informations = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Warning).Select(w => w.Message);
				CombineAssertions(assertionMessage, () =>
				{
					AssertBookingHasAttachedLandTransportConsignment(booking1);
					AssertBookingNasNoAttachedLandTransportConsignment(booking2);
					AssertMultilineASCIIEquals("Should log creation of Consignments.", @"
Successfully created Land Transport Consignment CN00000001 from Transport Booking TB00000001
					".Trim(), string.Join("\n", informations));
				});
			}
		}

		public void TestCreateLandTransportJobFromQueueItemWhenBookingDoesNotExist()
		{
			var targetModule = AutoCreatorTargetModules.Codes.LandTransportConsignment;
			var serviceTaskOptions = new ServiceTaskCreatorOptionCollection();

			var queueItem = Factory.New<DtbBookingQueue>();
			queueItem.KMQ_ParentID = Guid.NewGuid();
			queueItem.KMQ_TargetModule = targetModule;
			queueItem.KMQ_ParentTableCode = "KM";
			Factory.Save();

			using (TransportRegistry.Instance.EnableLandTransport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var buffer = new NotificationBuffer();
				var creator = new BookingToTransportJobCommonCreator(buffer, targetModule);
				var result = creator.TryCreateTransportJobsForTransportBookingQueueItem(queueItem.KMQ_ParentID);

				var error = string.Join("\n", buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Select(w => w.Message));
				CombineAssertions(() =>
				{
					AssertEquals("LT Consignment should not have been made", 0, result);
					var consignmentQuery = new ZDBOnlyQuery(typeof(IDtbConsignment));
					consignmentQuery.AddToFilter(DtbConsignmentSchema.LTC_KM_Booking, queueItem.KMQ_ParentID);

					var consignments = Factory.Load<IDtbConsignment>(consignmentQuery);
					AssertEquals($"Should not have created a Consignment.", 0, consignments.Length);
					AssertMultilineASCIIEquals("Should log Consignment was not able to be made.", @$"
Transport Jobs could not be created from the ID {queueItem.KMQ_ParentID} because no existing bookings with that ID were found.
					".Trim(), error);
				});
			}
		}

		public void TestCreateLandTransportJobFromQueueItemWhenLandTransportConsignmentAlreadyExistsForBooking()
		{
			var targetModule = AutoCreatorTargetModules.Codes.LandTransportConsignment;

			var now = new ZDateTime(ZDateTime.Now, DateTimeKind.Utc);
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();
			var orgProxyTransportCompany = GlbCompany.CurrentCompany.OrgProxy;
			var branchTransportCompany = Factory.NewWithValidTestData<OrgHeader>();
			var branch = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).Branches.AddNew();
			branch.GB_Code = "~ZZ";
			branch.GB_OH_OrgProxy = branchTransportCompany.PK;
			var validConfirmationDate = now.AddHours(1); // one hour in the future
			var validJobCreatedDate = now.AddDays(-10).AddMinutes(-30); // 10 days, 30 minutes old

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);
			BookingToTransportJobCreatorTestHelper.SetDatesRelatedToAutomaticTransportJobCreationForBooking(Factory, booking, validJobCreatedDate, validConfirmationDate);

			var queueItem = CreateNewDtbBookingQueue(booking, targetModule);
			var queueItem2 = CreateNewDtbBookingQueue(booking, targetModule);

			Factory.Save();

			using (TransportRegistry.Instance.EnableLandTransport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var buffer = new NotificationBuffer();
				var creator = new BookingToTransportJobCommonCreator(buffer, targetModule);
				var result = creator.TryCreateTransportJobsForTransportBookingQueueItem(queueItem.KMQ_ParentID);
				AssertBookingHasAttachedLandTransportConsignment(booking);
				AssertEquals("Precondition: LT Consignment should have been made", 1, result);
				result = creator.TryCreateTransportJobsForTransportBookingQueueItem(queueItem2.KMQ_ParentID);

				var error = string.Join("\n", buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Where(w => w.Message.Contains("existing")).Select(w => w.Message));
				CombineAssertions(() =>
				{
					AssertEquals("LT Consignment should already have been made", 0, result);
					AssertBookingHasAttachedLandTransportConsignment(booking);
					AssertMultilineASCIIEquals("Should log Consignments that have already been created.", @"
Transport Booking already has existing Land Transport Consignment attached.
					".Trim(), error);
				});
			}
		}

		public void TestCreateLandTransportJobFromQueueItem()
		{
			var targetModule = AutoCreatorTargetModules.Codes.LandTransportConsignment;

			var now = new ZDateTime(ZDateTime.Now, DateTimeKind.Utc);
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();
			var orgProxyTransportCompany = GlbCompany.CurrentCompany.OrgProxy;
			var branchTransportCompany = Factory.NewWithValidTestData<OrgHeader>();
			var branch = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).Branches.AddNew();
			branch.GB_Code = "~ZZ";
			branch.GB_OH_OrgProxy = branchTransportCompany.PK;
			var validConfirmationDate = now.AddHours(1); // one hour in the future
			var validJobCreatedDate = now.AddDays(-10).AddMinutes(-30); // 10 days, 30 minutes old

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);
			BookingToTransportJobCreatorTestHelper.SetDatesRelatedToAutomaticTransportJobCreationForBooking(Factory, booking, validJobCreatedDate, validConfirmationDate);

			var queueItem = CreateNewDtbBookingQueue(booking, targetModule);

			Factory.Save();

			using (TransportRegistry.Instance.EnableLandTransport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var buffer = new NotificationBuffer();
				var creator = new BookingToTransportJobCommonCreator(buffer, targetModule);
				var result = creator.TryCreateTransportJobsForTransportBookingQueueItem(queueItem.KMQ_ParentID);

				var informations = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Warning).Select(w => w.Message);
				CombineAssertions(() =>
				{
					AssertEquals("LT Consignment should have been made", 1, result);
					AssertBookingHasAttachedLandTransportConsignment(booking);
					AssertMultilineASCIIEquals("Should log creation of Consignments.", @"
Successfully created Land Transport Consignment CN00000001 from Transport Booking TB00000001
					".Trim(), string.Join("\n", informations));
				});
			}
		}

		public void TestCreatePortTransportJobFromQueueItem()
		{
			var targetModule = AutoCreatorTargetModules.Codes.PortTransport;

			var now = new ZDateTime(ZDateTime.Now, DateTimeKind.Utc);
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();
			var orgProxyTransportCompany = GlbCompany.CurrentCompany.OrgProxy;
			var branchTransportCompany = Factory.NewWithValidTestData<OrgHeader>();
			var branch = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).Branches.AddNew();
			branch.GB_Code = "~ZZ";
			branch.GB_OH_OrgProxy = branchTransportCompany.PK;
			var validConfirmationDate = now.AddHours(1); // one hour in the future
			var validJobCreatedDate = now.AddDays(-10).AddMinutes(-30); // 10 days, 30 minutes old

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);
			BookingToTransportJobCreatorTestHelper.SetDatesRelatedToAutomaticTransportJobCreationForBooking(Factory, booking, validJobCreatedDate, validConfirmationDate);

			var queueItem = CreateNewDtbBookingQueue(booking, targetModule);

			Factory.Save();

			var buffer = new NotificationBuffer();
			var creator = new BookingToTransportJobCommonCreator(buffer, targetModule);
			var result = creator.TryCreateTransportJobsForTransportBookingQueueItem(queueItem.KMQ_ParentID);

			var information = string.Join("\n", buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Warning).Select(w => w.Message));
			var error = string.Join("\n", buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Select(w => w.Message));
			CombineAssertions(() =>
			{
				AssertEquals("Port Transport Job should have been made", 1, result);
				AssertMultilineASCIIEquals("Should be no errors.", "", error);
				var portTransportJob = (Freight.LocalCartage.Integration.ICommonCartage)AssertBookingHasAttachedPortTransportJob(booking);
				var expectedInformation = string.Format("Successfully created Port Transport {0} from Transport Booking {1}", portTransportJob.JJ_ConsignmentID, booking.KM_JobID);
				AssertMultilineASCIIEquals("Should log creation of Port Transport.", expectedInformation, information);
			});
		}

		public void TestCreateLandTransportJob_UpdateExistingConsignment()
		{
			var serviceTaskOptions = new ServiceTaskCreatorOptionCollection();
			var serviceTaskOption = serviceTaskOptions.AddNew();
			serviceTaskOption.ContainerMode = AutoCreatorContainerModes.Codes.Loose;
			serviceTaskOption.TargetModule = AutoCreatorTargetModules.Codes.LandTransportConsignment;
			serviceTaskOption.IsSystemDefined = false;

			var now = new ZDateTime(ZDateTime.Now, DateTimeKind.Utc);

			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();
			var orgProxyTransportCompany = GlbCompany.CurrentCompany.OrgProxy;
			var branchTransportCompany = Factory.NewWithValidTestData<OrgHeader>();
			var branch = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).Branches.AddNew();
			branch.GB_Code = "~ZZ";
			branch.GB_OH_OrgProxy = branchTransportCompany.PK;
			var validConfirmationDate = now.AddHours(1); // one hour in the future
			var validJobCreatedDate = now.AddDays(-10).AddMinutes(-30); // 10 days, 30 minutes old			

			var booking1 = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent, consolidationJobDirection: nameof(DtbBookingDirection.PIC));
			BookingToTransportJobCreatorTestHelper.SetDatesRelatedToAutomaticTransportJobCreationForBooking(Factory, booking1, validJobCreatedDate, validConfirmationDate);
			booking1.KM_Status = TransportStatuses.Codes.PickUpConfirmed;
			booking1.KM_JobID = "TBWITHPICKUP";

			var booking2 = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent, consolidationJobDirection: nameof(DtbBookingDirection.DLV));
			BookingToTransportJobCreatorTestHelper.SetDatesRelatedToAutomaticTransportJobCreationForBooking(Factory, booking2, validJobCreatedDate, validConfirmationDate);
			booking2.KM_Status = TransportStatuses.Codes.PickUpConfirmed;
			booking2.KM_JobID = "TB000000002";

			var consignment = Factory.New<IDtbConsignment>();
			consignment.LTC_Direction = "ORG";
			consignment.LTC_KM_Booking = booking2.PK;
			consignment.LTC_Status = "BKD";

			Factory.Save();

			using (TransportRegistry.Instance.ServiceTaskCreatorOption.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, serviceTaskOptions))
			using (TransportRegistry.Instance.AutoCreateTransportConsignmentsCreationPeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			using (TransportRegistry.Instance.CreateTransportJobsFromTransportBookingEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, now.AddDays(-15).ToDateTime()))
			using (TransportRegistry.Instance.CreateTransportJobsFromTransportBookingWithinPeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 30))
			{
				var buffer = new NotificationBuffer();
				var creator = new BookingToTransportJobCommonCreator(buffer);
				var bookings = new[] { booking1, booking2 };
				creator.TryCreateTransportJobsFromTransportBookings(bookings);

				var informations = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Warning).Select(w => w.Message);
				CombineAssertions(() =>
				{
					AssertBookingHasAttachedLandTransportConsignment(booking1, expectedStatus: TransportStatuses.Codes.PickUpCommenced);
					AssertMultilineASCIIEquals("Should log creation of Consignments.", @"
Successfully created Land Transport Consignment CN00000002 from Transport Booking TBWITHPICKUP
					".Trim(), string.Join("\n", informations));

					var errors = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Select(w => w.Message);
					AssertMultilineASCIIEquals("Should log creation of Transport Bookings.", @"
Transport Jobs could not be created from Transport Booking TB000000002 for the following reasons:
Transport Booking already has existing Land Transport Consignment attached.
					".Trim(), string.Join("\n", errors));
				});
			}
		}

		public void TestInvalidBookingsAreMarkedAsActionRequired()
		{
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();
			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBooking(Helper, GlbCompany.CurrentCompany.OrgProxy, dummyBookingParent, consolidationJobDirection: nameof(DtbBookingDirection.PIC), bookingIsActive: true, transportCompanyIsRoadTransport: true, transportCompanyIsCarrier: true);
			Factory.Save();

			var buffer = new NotificationBuffer();
			var targetModule = AutoCreatorTargetModules.Codes.PortTransport;
			var processingManager = new BookingToTransportJobCommonCreator(buffer, targetModule);
			processingManager.TryCreateTransportJobsFromTransportBookings(new[] { booking });

			AssertBookingHasNoAttachedTransportJobs(booking);
			AssertEquals("Invalid Bookings should have status Action Required (ACR)", TransportStatuses.Codes.ActionRequired, booking.KM_Status);
			var bookingStatusChangeLogs = booking.Logs.GetAllLogs().Cast<StmALog>().Where(l => l.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode).OrderByDescending(l => l.SL_EventTime);
			AssertEquals("There should only be one status change event", 1, bookingStatusChangeLogs.Count());
			var mostRecentStatusChangeLog = bookingStatusChangeLogs.First();
			mostRecentStatusChangeLog.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, out var actualReason);
			AssertEquals("Status change log should contain a parameter for the reason", "Validation Failed when Creating Transport Job", actualReason);
		}

		public void TestInvalidBookingsAreNotMarkedAsActionRequiredWhenCreatedManually()
		{
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();
			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBooking(Helper, GlbCompany.CurrentCompany.OrgProxy, dummyBookingParent, consolidationJobDirection: nameof(DtbBookingDirection.PIC), bookingIsActive: true, transportCompanyIsRoadTransport: true, transportCompanyIsCarrier: true);
			Factory.Save();

			var buffer = new NotificationBuffer();
			var targetModule = AutoCreatorTargetModules.Codes.PortTransport;
			var processingManager = new BookingToTransportJobCommonCreator(buffer, targetModule, isManualProcess: true);
			processingManager.TryCreateTransportJobsFromTransportBookings(new[] { booking });

			AssertBookingHasNoAttachedTransportJobs(booking);
			AssertNotEquals("Invalid Bookings should not have status Action Required (ACR)", TransportStatuses.Codes.ActionRequired, booking.KM_Status);
		}

		public void TestValidBookingsAreMarkedAsActionRequiredWhenDataImportFails()
		{
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();
			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);
			BookingToTransportJobCommonCreator.FailureContextToFakeWhenProcessingFailures = "Unable to import data";
			Factory.Save();

			var buffer = new NotificationBuffer();
			var targetModule = AutoCreatorTargetModules.Codes.PortTransport;
			var processingManager = new BookingToTransportJobCommonCreator(buffer, targetModule);
			processingManager.TryCreateTransportJobsFromTransportBookings(new[] { booking });

			AssertEquals("When data import of Bookings fails, bookings should have status Action Required (ACR)", TransportStatuses.Codes.ActionRequired, booking.KM_Status);
			var bookingStatusChangeLogs = booking.Logs.GetAllLogs().Cast<StmALog>().Where(l => l.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode).OrderByDescending(l => l.SL_EventTime);
			AssertEquals("There should only be one status change event", 1, bookingStatusChangeLogs.Count());
			var mostRecentStatusChangeLog = bookingStatusChangeLogs.First();
			mostRecentStatusChangeLog.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, out var actualReason);
			AssertEquals("Status change log should contain a parameter for the reason", "Validation Failed when Creating Transport Job", actualReason);
		}

		void AssertBookingHasAttachedLandTransportConsignment(DtbBooking booking, string expectedStatus = TransportStatuses.Codes.ServiceCommenced)
		{
			var consignmentQuery = new ZDBOnlyQuery(typeof(IDtbConsignment));
			consignmentQuery.AddToFilter(DtbConsignmentSchema.LTC_KM_Booking, booking.PK);

			var consignments = Factory.Load<IDtbConsignment>(consignmentQuery);
			AssertEquals($"{booking.HumanReadableName} was unable to create a Consignment.", 1, consignments.Length);
			AssertEquals(expectedStatus, new BusinessObjectFactory().Load<DtbBooking>(booking.PK).KM_Status);
			AssertEquals(TransportStatuses.Codes.Booked, booking.LandTransportConsignment.LTC_Status);

			var packageJobQuery = new ZDBOnlyQuery(typeof(PkgPackageJob));
			packageJobQuery.AddToFilter(PkgPackageJobSchema.KJ_ParentID, consignments.First().PK);
			var packageJob = Factory.Load<PkgPackageJob>(packageJobQuery);

			AssertEquals(1, packageJob.First().Packages.Count);
		}

		void AssertBookingNasNoAttachedLandTransportConsignment(DtbBooking booking)
		{
			var consignmentQuery = new ZDBOnlyQuery(typeof(IDtbConsignment));
			consignmentQuery.AddToFilter(DtbConsignmentSchema.LTC_KM_Booking, booking.PK);

			var consignments = Factory.Load<IDtbConsignment>(consignmentQuery);
			AssertEquals($"{booking.HumanReadableName} should not have a Consignment.", 0, consignments.Length);
		}

		DtbBookingQueue CreateNewDtbBookingQueue(DtbBooking parentBooking, string targetModule)
		{
			var bookingQueue = Factory.New<DtbBookingQueue>();
			bookingQueue.KMQ_ParentID = parentBooking.PK;
			bookingQueue.KMQ_ParentTableCode = "KM";
			bookingQueue.KMQ_TargetModule = targetModule;
			return bookingQueue;
		}

		DtbBooking CreateNewBookingWithTemplate(ZString template)
		{
			var dummyParent = Factory.New<DummyWithDtbBooking>();
			var transportCompany = GlbCompany.CurrentCompany.OrgProxy;
			var validConfirmationDate = new ZDateTime(2013, 1, 1, 2, 20, 0, DateTimeKind.Utc); // one hour in the future
			var validJobCreatedDate = new ZDateTime(2013, 1, 1, 0, 50, 0, DateTimeKind.Utc); // 30 minutes old

			var consolidation = Helper.CreateConsolidation(dummyParent);
			var container1 = consolidation.PackageJob.Packages.AddNew("CNT", "CONT123");
			var container2 = consolidation.PackageJob.Packages.AddNew("CNT", "CONT456");
			var container3 = consolidation.PackageJob.Packages.AddNew("CNT", "CONT789");
			container1.Container.K0_RC_ContainerType = Helper.LoadRefContainer("20GP").PK;
			container2.Container.K0_RC_ContainerType = Helper.LoadRefContainer("20GP").PK;
			container3.Container.K0_RC_ContainerType = Helper.LoadRefContainer("20GP").PK;

			var pallet1 = container1.Packages.AddNew("PLT", 1);
			var pallet2 = container2.Packages.AddNew("BAG", 1);
			var pallet3 = container3.Packages.AddNew("BND", 1);
			var box1 = pallet1.Packages.AddNew("BOX");
			var box2 = pallet2.Packages.AddNew("CTN");
			var box3 = pallet2.Packages.AddNew("PCE");

			var topLevelPallet = consolidation.PackageJob.Packages.AddNew("DRM", "TopLevelDrum");

			var booking = consolidation.Bookings.AddNew();
			booking.Address.OrganisationPK = transportCompany.PK;
			booking.Address.Organisation.OH_IsLocalTransport = true;
			booking.KM_KT_NKBookingTemplate = template;

			if (booking.Instructions.FirstOrDefault(i => i.IsPickUp) == null)
			{
				BookingToTransportJobCreatorTestHelper.CreateNewInstruction(Helper, booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, Helper.CreateOrLoadOrganisation("PICKUP_ORG").MainAddress, shouldHaveAtLeastOneConfirmation: true, attachExistingPackageOrCreateAndAttachPackage: false);
			}
			if (booking.Instructions.FirstOrDefault(i => i.IsDelivery) == null)
			{
				BookingToTransportJobCreatorTestHelper.CreateNewInstruction(Helper, booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, Helper.CreateOrLoadOrganisation("DELIVERY_ORG").MainAddress, shouldHaveAtLeastOneConfirmation: true, attachExistingPackageOrCreateAndAttachPackage: false);
			}

			if (template == "MPDV")
			{
				var instructions = booking.Instructions.OrderBy(b => b.KN_Sequence);
				instructions.ElementAt(0).DivotsWithPackages.RemovePackage(topLevelPallet);
				instructions.ElementAt(1).PackageDivots.DeleteAll();
				instructions.ElementAt(1).DivotsWithPackages.AddPackage(pallet1);
				instructions.ElementAt(2).PackageDivots.DeleteAll();
				instructions.ElementAt(2).DivotsWithPackages.AddPackage(pallet2);
				instructions.ElementAt(3).PackageDivots.DeleteAll();
				instructions.ElementAt(3).DivotsWithPackages.AddPackage(pallet3);
			}
			else if (template == "MPPU")
			{
				var instructions = booking.Instructions.OrderBy(b => b.KN_Sequence);
				instructions.ElementAt(0).PackageDivots.DeleteAll();
				instructions.ElementAt(0).DivotsWithPackages.AddPackage(pallet1);
				instructions.ElementAt(1).PackageDivots.DeleteAll();
				instructions.ElementAt(1).DivotsWithPackages.AddPackage(pallet2);
				instructions.ElementAt(2).PackageDivots.DeleteAll();
				instructions.ElementAt(2).DivotsWithPackages.AddPackage(pallet3);
				instructions.ElementAt(3).DivotsWithPackages.RemovePackage(topLevelPallet);
			}

			foreach (var instruction in booking.Instructions)
			{
				OrgHeader org;
				if (instruction.IsPickUp)
				{
					if (instruction.OrganisationType == LocalCartageJobOrgTypeList.Codes.CFS)
					{
						org = Helper.CreateOrLoadOrganisation("CFS");
						org.OH_IsMiscFreightServices = true;
						org.OH_IsPackDepot = true;
					}
					else
					{
						org = Helper.CreateOrLoadOrganisation("PICKUP");
					}
					instruction.Address.OrganisationPK = org.PK;
				}
				else if (instruction.IsDelivery)
				{
					org = Helper.CreateOrLoadOrganisation("DELIVERY");
					instruction.Address.OrganisationPK = org.PK;
				}
			}

			return booking;
		}

		IDisposable SetupServiceTaskCreatorOptionRegistry(ZString cntTarget, ZString ftlTarget, ZString lseTarget, ZString mixTarget)
		{
			var serviceTaskOptions = new ServiceTaskCreatorOptionCollection();
			serviceTaskOptions.Add(new ServiceTaskCreatorOption() { ContainerMode = AutoCreatorContainerModes.Codes.MixedCargo, TargetModule = mixTarget });
			serviceTaskOptions.Add(new ServiceTaskCreatorOption() { ContainerMode = AutoCreatorContainerModes.Codes.Container, TargetModule = cntTarget });
			serviceTaskOptions.Add(new ServiceTaskCreatorOption() { ContainerMode = AutoCreatorContainerModes.Codes.FTL, TargetModule = ftlTarget });
			serviceTaskOptions.Add(new ServiceTaskCreatorOption() { ContainerMode = AutoCreatorContainerModes.Codes.Loose, TargetModule = lseTarget });

			return TransportRegistry.Instance.ServiceTaskCreatorOption.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, serviceTaskOptions);
		}

		public void TestPortTransportJobCreated_WhenTargetModuleIsPortTransport()
		{
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();
			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);

			Factory.Save();

			var buffer = new NotificationBuffer();
			var targetModule = AutoCreatorTargetModules.Codes.PortTransport;
			var processingManager = new BookingToTransportJobCommonCreator(buffer, targetModule);
			processingManager.TryCreateTransportJobsFromTransportBookings(new[] { booking });

			var portTransportJob = (Freight.LocalCartage.Integration.ICommonCartage)AssertBookingHasAttachedPortTransports(booking, 1);
			var expectedInformation = string.Format("Successfully created Port Transport {0} from Transport Booking {1}", portTransportJob.JJ_ConsignmentID, booking.KM_JobID);
			var information = string.Join("\n", buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Warning).Select(w => w.Message));
			CombineAssertions(() =>
			{
				AssertBookingHasAttachedPortTransports(booking, 1);
				AssertMultilineASCIIEquals("Should log creation of Port Transport Job.", expectedInformation, information);
			});
		}

		public void TestLandTransportConsignmentCreated_WhenTargetModuleIsLandTransport()
		{
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();
			var branchTransportCompany = Factory.NewWithValidTestData<OrgHeader>();
			var branch = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).Branches.AddNew();
			branch.GB_Code = "~ZZ";
			branch.GB_OH_OrgProxy = branchTransportCompany.PK;

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);

			Factory.Save();

			var buffer = new NotificationBuffer();
			var targetModule = AutoCreatorTargetModules.Codes.LandTransportConsignment;
			var processingManager = new BookingToTransportJobCommonCreator(buffer, targetModule);

			using (TransportRegistry.Instance.EnableLandTransport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				processingManager.TryCreateTransportJobsFromTransportBookings(new[] { booking });
			}

			var information = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Warning).Select(w => w.Message);
			CombineAssertions(() =>
			{
				AssertBookingHasAttachedLandTransportConsignment(booking);
				AssertMultilineASCIIEquals("Should log creation of Land Transport Consignments.", @"
Successfully created Land Transport Consignment CN00000001 from Transport Booking TB00000001
					".Trim(), string.Join("\n", information));
			});
		}

		public void TestLandTransportConsignment_GeneratesError_WhenLandTransportRegistryNotEnabled()
		{
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();
			var branchTransportCompany = Factory.NewWithValidTestData<OrgHeader>();
			var branch = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).Branches.AddNew();
			branch.GB_Code = "~ZZ";
			branch.GB_OH_OrgProxy = branchTransportCompany.PK;

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);

			Factory.Save();

			using (TransportRegistry.Instance.EnableLandTransport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var buffer = new NotificationBuffer();
				var targetModule = AutoCreatorTargetModules.Codes.LandTransportConsignment;
				var processingManager = new BookingToTransportJobCommonCreator(buffer, targetModule);
				processingManager.TryCreateTransportJobsFromTransportBookings(new[] { booking });

				var errors = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Select(w => w.Message);
				AssertMultilineASCIIEquals("Should log error when Land Transport Registry not enabled and tries to create Land Transport Consignment.",
					"Land Transport module is not enabled, cannot create Land Transport Consignment from Transport Booking", string.Join("\n", errors));
			}
		}

		public void TestGetBookingData_CorrectlySets_TargetModule()
		{
			var now = ZDateTime.Now;

			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var booking = Helper.CreateBooking();
				Factory.Save();
				var actualTargetDataContext = BookingToTransportJobCommonCreator.TargetModuleDataObjectWriter.GetBookingDataTarget(booking, AutoCreatorTargetModules.Codes.PortTransport);
				AssertEquals(DataContextType.LocalTransport, actualTargetDataContext);
			}
		}

		public void TestTryCreateTransportJobsFromTransportBookings_ErrorIsReportedWhenBookingIsNotActive()
		{
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);
			booking.KM_IsActive = false;

			Factory.Save();

			var buffer = new NotificationBuffer();
			var creator = new BookingToTransportJobCommonCreator(buffer);

			creator.TryCreateTransportJobsFromTransportBookings(new[] { booking });

			var errorMessages = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Select(w => w.Message);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("An error should be reported when the booking is not active.",
					new string[] {
						$"Transport Jobs could not be created from Transport Booking {booking.KM_JobID} for the following reasons:",
						"Transport Booking is not active." },
					errorMessages);
				AssertBookingHasNoAttachedTransportJobs(booking);
			});
		}

		public void TestTryCreateTransportJobsFromTransportBookings_ErrorIsReportedWhenTransportCompanyIsNotCarrier()
		{
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);
			booking.Address.Organisation.OH_IsShippingProvider = false;

			Factory.Save();

			var buffer = new NotificationBuffer();
			var creator = new BookingToTransportJobCommonCreator(buffer);

			creator.TryCreateTransportJobsFromTransportBookings(new[] { booking });

			var errorMessages = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Select(w => w.Message);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("An error should be reported when the transport company is not a carrier.",
					new string[] {
						$"Transport Jobs could not be created from Transport Booking {booking.KM_JobID} for the following reasons:",
						"The Transport Company is not a carrier." },
					errorMessages);
				AssertBookingHasNoAttachedTransportJobs(booking);
			});
		}

		public void TestTryCreateTransportJobsFromTransportBookings_ErrorIsReportedWhenTransportCompanyIsNotRoadTransportProvider()
		{
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);
			booking.Address.Organisation.OH_IsLocalTransport = false;

			Factory.Save();

			var buffer = new NotificationBuffer();
			var creator = new BookingToTransportJobCommonCreator(buffer);

			creator.TryCreateTransportJobsFromTransportBookings(new[] { booking });

			var errorMessages = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Select(w => w.Message);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("An error should be reported when the transport company does not have a secondary job type of 'Carrier – Road Transport Provider'.",
					new string[] {
						$"Transport Jobs could not be created from Transport Booking {booking.KM_JobID} for the following reasons:",
						"The Transport Company does not have a secondary job type of 'Carrier – Road Transport Provider'." },
					errorMessages);
				AssertBookingHasNoAttachedTransportJobs(booking);
			});
		}

		public void TestTryCreateTransportJobsFromTransportBookings_ErrorIsReportedWhenASinglePickUpInstructionHasNoAddress()
		{
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);
			booking.Instructions.First(i => i.IsPickUp).Address.Delete();

			Factory.Save();

			var buffer = new NotificationBuffer();
			var creator = new BookingToTransportJobCommonCreator(buffer);

			creator.TryCreateTransportJobsFromTransportBookings(new[] { booking });

			var errorMessages = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Select(w => w.Message);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("An error should be reported when a pickup instruction has no valid address.",
					new string[] {
						$"Transport Jobs could not be created from Transport Booking {booking.KM_JobID} for the following reasons:",
						"There is no Address for 1 Pickup Instruction(s) on this Transport Booking." },
					errorMessages);
				AssertBookingHasNoAttachedTransportJobs(booking);
			});
		}

		public void TestTryCreateTransportJobsFromTransportBookings_ErrorIsReportedWhenASingleDeliveryInstructionHasNoAddress()
		{
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();
			
			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);
			booking.Instructions.First(i => i.IsDelivery).Address.Delete();

			Factory.Save();

			var buffer = new NotificationBuffer();
			var creator = new BookingToTransportJobCommonCreator(buffer);

			creator.TryCreateTransportJobsFromTransportBookings(new[] { booking });

			var errorMessages = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Select(w => w.Message);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("An error should be reported when a delivery instruction has no valid address.",
					new string[] {
						$"Transport Jobs could not be created from Transport Booking {booking.KM_JobID} for the following reasons:",
						"There is no Address for 1 Delivery Instruction(s) on this Transport Booking." },
					errorMessages);
				AssertBookingHasNoAttachedTransportJobs(booking);
			});
		}

		public void TestTryCreateTransportJobsFromTransportBookings_ErrorIsReportedWhenASingleMultiInstructionHasNoAddress()
		{
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);
			BookingToTransportJobCreatorTestHelper.CreateNewInstruction(Helper, booking, InstructionTypes.Codes.Multi, OrganisationTypesList.Codes.CNE, address: null, shouldHaveAtLeastOneConfirmation: true, attachExistingPackageOrCreateAndAttachPackage: true);

			Factory.Save();

			var buffer = new NotificationBuffer();
			var creator = new BookingToTransportJobCommonCreator(buffer);

			creator.TryCreateTransportJobsFromTransportBookings(new[] { booking });

			var errorMessages = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Select(w => w.Message);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("An error should be reported when a multi instruction has no valid address.",
					new string[] {
						$"Transport Jobs could not be created from Transport Booking {booking.KM_JobID} for the following reasons:",
						"There is no Address for 1 Multi Instruction(s) on this Transport Booking." },
					errorMessages);
				AssertBookingHasNoAttachedTransportJobs(booking);
			});
		}

		public void TestTryCreateTransportJobsFromTransportBookings_ErrorIsReportedWhenASinglePickUpInstructionHasNoConfirmations()
		{
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);
			booking.Instructions.First(i => i.IsPickUp).Confirmations.DeleteAll();

			Factory.Save();

			var buffer = new NotificationBuffer();
			var creator = new BookingToTransportJobCommonCreator(buffer);

			creator.TryCreateTransportJobsFromTransportBookings(new[] { booking });

			var errorMessages = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Select(w => w.Message);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("An error should be reported when a pickup instruction has no attached confirmations.",
					new string[] {
						$"Transport Jobs could not be created from Transport Booking {booking.KM_JobID} for the following reasons:",
						"There are no Confirmations attached to 1 Pickup Instruction(s) on this Transport Booking." },
					errorMessages);
				AssertBookingHasNoAttachedTransportJobs(booking);
			});
		}

		public void TestTryCreateTransportJobsFromTransportBookings_TransportJobIsCreatedWhenASingleDeliveryInstructionHasNoConfirmations()
		{
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);
			booking.Instructions.First(i => i.IsDelivery).Confirmations.DeleteAll();
			
			Factory.Save();

			var buffer = new NotificationBuffer();
			var creator = new BookingToTransportJobCommonCreator(buffer);

			creator.TryCreateTransportJobsFromTransportBookings(new[] { booking });

			AssertBookingHasAttachedPortTransportJob(booking);
		}

		public void TestTryCreateTransportJobsFromTransportBookings_TransportJobIsCreatedWhenASingleMultiInstructionHasNoConfirmations()
		{
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);
			BookingToTransportJobCreatorTestHelper.CreateNewInstruction(Helper, booking, InstructionTypes.Codes.Multi, OrganisationTypesList.Codes.CNE, Helper.CreateOrLoadOrganisation("MULTI_ORG").MainAddress, shouldHaveAtLeastOneConfirmation: false, attachExistingPackageOrCreateAndAttachPackage: true);

			Factory.Save();

			var buffer = new NotificationBuffer();
			var creator = new BookingToTransportJobCommonCreator(buffer);

			creator.TryCreateTransportJobsFromTransportBookings(new[] { booking });

			AssertBookingHasAttachedPortTransportJob(booking);
		}

		public void TestTryCreateTransportJobsFromTransportBookings_ErrorIsReportedWhenASinglePickUpInstructionHasNoAttachedPackagesAndLandTransportTarget()
		{
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);
			booking.Instructions.First(i => i.IsPickUp).DivotsWithPackages.DeleteAll();

			Factory.Save();

			var buffer = new NotificationBuffer();
			var creator = new BookingToTransportJobCommonCreator(buffer, AutoCreatorTargetModules.Codes.LandTransportConsignment);

			using (TransportRegistry.Instance.EnableLandTransport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				creator.TryCreateTransportJobsFromTransportBookings(new[] { booking });

				var errorMessages = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Select(w => w.Message);

				CombineAssertions(() =>
				{
					AssertContainsExactElementsInAnyOrder("An error should be reported when a pickup instruction has no assigned packages.",
						new string[] {
						$"Transport Jobs could not be created from Transport Booking {booking.KM_JobID} for the following reasons:",
						"There are no Packages assigned to 1 Pickup Instruction(s) on this Transport Booking." },
						errorMessages);
					AssertBookingHasNoAttachedTransportJobs(booking);
				});
			}
		}

		public void TestTryCreateTransportJobsFromTransportBookings_ErrorIsReportedWhenASingleDeliveryInstructionHasNoAttachedPackagesAndLandTransportTarget()
		{
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);
			booking.Instructions.First(i => i.IsDelivery).DivotsWithPackages.DeleteAll();

			Factory.Save();

			var buffer = new NotificationBuffer();
			var creator = new BookingToTransportJobCommonCreator(buffer, AutoCreatorTargetModules.Codes.LandTransportConsignment);

			using (TransportRegistry.Instance.EnableLandTransport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				creator.TryCreateTransportJobsFromTransportBookings(new[] { booking });

				var errorMessages = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Select(w => w.Message);

				CombineAssertions(() =>
				{
					AssertContainsExactElementsInAnyOrder("An error should be reported when a delivery instruction has no assigned packages.",
						new string[] {
						$"Transport Jobs could not be created from Transport Booking {booking.KM_JobID} for the following reasons:",
						"There are no Packages assigned to 1 Delivery Instruction(s) on this Transport Booking." },
						errorMessages);
					AssertBookingHasNoAttachedTransportJobs(booking);
				});
			}
		}

		public void TestTryCreateTransportJobsFromTransportBookings_ErrorIsReportedWhenASingleMultiInstructionHasNoAttachedPackagesAndLandTransportTarget()
		{
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);
			BookingToTransportJobCreatorTestHelper.CreateNewInstruction(Helper, booking, InstructionTypes.Codes.Multi, OrganisationTypesList.Codes.CNE, Helper.CreateOrLoadOrganisation("MULTI_ORG").MainAddress, shouldHaveAtLeastOneConfirmation: true, attachExistingPackageOrCreateAndAttachPackage: false);

			Factory.Save();

			var buffer = new NotificationBuffer();
			var creator = new BookingToTransportJobCommonCreator(buffer, AutoCreatorTargetModules.Codes.LandTransportConsignment);

			using (TransportRegistry.Instance.EnableLandTransport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				creator.TryCreateTransportJobsFromTransportBookings(new[] { booking });

				var errorMessages = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Select(w => w.Message);

				CombineAssertions(() =>
				{
					AssertContainsExactElementsInAnyOrder("An error should be reported when a multi instruction has no assigned packages.",
						new string[] {
							$"Transport Jobs could not be created from Transport Booking {booking.KM_JobID} for the following reasons:",
							"There are no Packages assigned to 1 Multi Instruction(s) on this Transport Booking." },
						errorMessages);
					AssertBookingHasNoAttachedTransportJobs(booking);
				});
			}
		}

		public void TestTryCreateTransportJobsFromTransportBookings_PortTransportTargetDoesNotValidateEmptyInstructions()
		{
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();
			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);
			booking.Instructions.First(i => i.IsPickUp).DivotsWithPackages.DeleteAll();
			Factory.Save();

			var buffer = new NotificationBuffer();
			var creator = new BookingToTransportJobCommonCreator(buffer, AutoCreatorTargetModules.Codes.PortTransport);
			creator.TryCreateTransportJobsFromTransportBookings(new[] { booking });
			var errorMessages = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Select(w => w.Message);

			var ptJob = booking.GetPortTransportJobs().FirstOrDefault();

			AssertEquals("Should skip package count validation for a Port Transport Job", errorMessages.Contains("There are no Packages assigned to 1 Pickup Instruction(s) on this Transport Booking."), false);
			AssertNotNull("PT Job should exist on Booking", ptJob);
		}

		public void TestTryCreateTransportJobsFromTransportBookings_ErrorIsReportedWhenMultiplePickUpInstructionsHaveNoAddress()
		{
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);
			BookingToTransportJobCreatorTestHelper.CreateNewInstruction(Helper, booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, address: null, shouldHaveAtLeastOneConfirmation: true, attachExistingPackageOrCreateAndAttachPackage: true);
			BookingToTransportJobCreatorTestHelper.CreateNewInstruction(Helper, booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, address: null, shouldHaveAtLeastOneConfirmation: true, attachExistingPackageOrCreateAndAttachPackage: true);

			Factory.Save();

			var buffer = new NotificationBuffer();
			var creator = new BookingToTransportJobCommonCreator(buffer);

			creator.TryCreateTransportJobsFromTransportBookings(new[] { booking });

			var errorMessages = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Select(w => w.Message);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("An error should be reported when multiple pickup instructions have no valid address.",
					new string[] {
						$"Transport Jobs could not be created from Transport Booking {booking.KM_JobID} for the following reasons:",
						"There is no Address for 2 Pickup Instruction(s) on this Transport Booking." },
					errorMessages);
				AssertBookingHasNoAttachedTransportJobs(booking);
			});
		}

		public void TestTryCreateTransportJobsFromTransportBookings_ErrorIsReportedWhenMultipleDeliveryInstructionsHaveNoAddress()
		{
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);
			BookingToTransportJobCreatorTestHelper.CreateNewInstruction(Helper, booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, address: null, shouldHaveAtLeastOneConfirmation: true, attachExistingPackageOrCreateAndAttachPackage: true);
			BookingToTransportJobCreatorTestHelper.CreateNewInstruction(Helper, booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, address: null, shouldHaveAtLeastOneConfirmation: true, attachExistingPackageOrCreateAndAttachPackage: true);

			Factory.Save();

			var buffer = new NotificationBuffer();
			var creator = new BookingToTransportJobCommonCreator(buffer);

			creator.TryCreateTransportJobsFromTransportBookings(new[] { booking });

			var errorMessages = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Select(w => w.Message);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("An error should be reported when multiple delivery instructions have no valid address.",
					new string[] {
						$"Transport Jobs could not be created from Transport Booking {booking.KM_JobID} for the following reasons:",
						"There is no Address for 2 Delivery Instruction(s) on this Transport Booking." },
					errorMessages);
				AssertBookingHasNoAttachedTransportJobs(booking);
			});
		}

		public void TestTryCreateTransportJobsFromTransportBookings_ErrorIsReportedWhenMultipleMultiInstructionsHaveNoAddress()
		{
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);
			BookingToTransportJobCreatorTestHelper.CreateNewInstruction(Helper, booking, InstructionTypes.Codes.Multi, OrganisationTypesList.Codes.CNE, Helper.CreateOrLoadOrganisation("MULTI_ORG").MainAddress, shouldHaveAtLeastOneConfirmation: true, attachExistingPackageOrCreateAndAttachPackage: true);
			BookingToTransportJobCreatorTestHelper.CreateNewInstruction(Helper, booking, InstructionTypes.Codes.Multi, OrganisationTypesList.Codes.CNE, address: null, shouldHaveAtLeastOneConfirmation: true, attachExistingPackageOrCreateAndAttachPackage: true);
			BookingToTransportJobCreatorTestHelper.CreateNewInstruction(Helper, booking, InstructionTypes.Codes.Multi, OrganisationTypesList.Codes.CNE, address: null, shouldHaveAtLeastOneConfirmation: true, attachExistingPackageOrCreateAndAttachPackage: true);

			Factory.Save();

			var buffer = new NotificationBuffer();
			var creator = new BookingToTransportJobCommonCreator(buffer);

			creator.TryCreateTransportJobsFromTransportBookings(new[] { booking });

			var errorMessages = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Select(w => w.Message);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("An error should be reported when multiple multi instructions have no valid address.",
					new string[] {
						$"Transport Jobs could not be created from Transport Booking {booking.KM_JobID} for the following reasons:",
						"There is no Address for 2 Multi Instruction(s) on this Transport Booking." },
					errorMessages);
				AssertBookingHasNoAttachedTransportJobs(booking);
			});
		}

		public void TestTryCreateTransportJobsFromTransportBookings_ErrorIsReportedWhenMultiplePickUpInstructionsHaveNoConfirmations()
		{
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);
			BookingToTransportJobCreatorTestHelper.CreateNewInstruction(Helper, booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, Helper.CreateOrLoadOrganisation("PICKUP_ORG").MainAddress, shouldHaveAtLeastOneConfirmation: false, attachExistingPackageOrCreateAndAttachPackage: true);
			BookingToTransportJobCreatorTestHelper.CreateNewInstruction(Helper, booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, Helper.CreateOrLoadOrganisation("PICKUP_ORG").MainAddress, shouldHaveAtLeastOneConfirmation: false, attachExistingPackageOrCreateAndAttachPackage: true);

			Factory.Save();

			var buffer = new NotificationBuffer();
			var creator = new BookingToTransportJobCommonCreator(buffer);

			creator.TryCreateTransportJobsFromTransportBookings(new[] { booking });

			var errorMessages = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Select(w => w.Message);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("An error should be reported when multiple pickup instructions have no attached confirmations.",
					new string[] {
						$"Transport Jobs could not be created from Transport Booking {booking.KM_JobID} for the following reasons:",
						"There are no Confirmations attached to 2 Pickup Instruction(s) on this Transport Booking." },
					errorMessages);
				AssertBookingHasNoAttachedTransportJobs(booking);
			});
		}

		public void TestTryCreateTransportJobsFromTransportBookings_ErrorIsReportedWhenMultiplePickUpInstructionsHaveNoAttachedPackages()
		{
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);
			BookingToTransportJobCreatorTestHelper.CreateNewInstruction(Helper, booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, Helper.CreateOrLoadOrganisation("PICKUP_ORG").MainAddress, shouldHaveAtLeastOneConfirmation: true, attachExistingPackageOrCreateAndAttachPackage: false);
			BookingToTransportJobCreatorTestHelper.CreateNewInstruction(Helper, booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, Helper.CreateOrLoadOrganisation("PICKUP_ORG").MainAddress, shouldHaveAtLeastOneConfirmation: true, attachExistingPackageOrCreateAndAttachPackage: false);

			Factory.Save();

			var buffer = new NotificationBuffer();
			var creator = new BookingToTransportJobCommonCreator(buffer);

			creator.TryCreateTransportJobsFromTransportBookings(new[] { booking });

			var errorMessages = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Select(w => w.Message);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("An error should be reported when multiple pickup instructions have no attached packages.",
					new string[] {
						$"Transport Jobs could not be created from Transport Booking {booking.KM_JobID} for the following reasons:",
						"There are no Packages assigned to 2 Pickup Instruction(s) on this Transport Booking." },
					errorMessages);
				AssertBookingHasNoAttachedTransportJobs(booking);
			});
		}

		public void TestTryCreateTransportJobsFromTransportBookings_ErrorIsReportedWhenMultipleDeliveryInstructionsHaveNoAttachedPackages()
		{
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);
			BookingToTransportJobCreatorTestHelper.CreateNewInstruction(Helper, booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, Helper.CreateOrLoadOrganisation("DELIVERY_ORG").MainAddress, shouldHaveAtLeastOneConfirmation: true, attachExistingPackageOrCreateAndAttachPackage: false);
			BookingToTransportJobCreatorTestHelper.CreateNewInstruction(Helper, booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, Helper.CreateOrLoadOrganisation("DELIVERY_ORG").MainAddress, shouldHaveAtLeastOneConfirmation: true, attachExistingPackageOrCreateAndAttachPackage: false);

			Factory.Save();

			var buffer = new NotificationBuffer();
			var creator = new BookingToTransportJobCommonCreator(buffer);

			creator.TryCreateTransportJobsFromTransportBookings(new[] { booking });

			var errorMessages = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Select(w => w.Message);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("An error should be reported when multiple delivery instructions have no attached packages.",
					new string[] {
						$"Transport Jobs could not be created from Transport Booking {booking.KM_JobID} for the following reasons:",
						"There are no Packages assigned to 2 Delivery Instruction(s) on this Transport Booking." },
					errorMessages);
				AssertBookingHasNoAttachedTransportJobs(booking);
			});
		}

		public void TestTryCreateTransportJobsFromTransportBookings_ErrorIsReportedWhenMultipleMultiInstructionsHaveNoAttachedPackages()
		{
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);
			BookingToTransportJobCreatorTestHelper.CreateNewInstruction(Helper, booking, InstructionTypes.Codes.Multi, OrganisationTypesList.Codes.CNE, Helper.CreateOrLoadOrganisation("MULTI_ORG").MainAddress, shouldHaveAtLeastOneConfirmation: true, attachExistingPackageOrCreateAndAttachPackage: true);
			BookingToTransportJobCreatorTestHelper.CreateNewInstruction(Helper, booking, InstructionTypes.Codes.Multi, OrganisationTypesList.Codes.CNE, Helper.CreateOrLoadOrganisation("MULTI_ORG").MainAddress, shouldHaveAtLeastOneConfirmation: true, attachExistingPackageOrCreateAndAttachPackage: false);
			BookingToTransportJobCreatorTestHelper.CreateNewInstruction(Helper, booking, InstructionTypes.Codes.Multi, OrganisationTypesList.Codes.CNE, Helper.CreateOrLoadOrganisation("MULTI_ORG").MainAddress, shouldHaveAtLeastOneConfirmation: true, attachExistingPackageOrCreateAndAttachPackage: false);

			Factory.Save();

			var buffer = new NotificationBuffer();
			var creator = new BookingToTransportJobCommonCreator(buffer);

			creator.TryCreateTransportJobsFromTransportBookings(new[] { booking });

			var errorMessages = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Select(w => w.Message);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("An error should be reported when multiple multi instructions have no attached packages.",
					new string[] {
						$"Transport Jobs could not be created from Transport Booking {booking.KM_JobID} for the following reasons:",
						"There are no Packages assigned to 2 Multi Instruction(s) on this Transport Booking." },
					errorMessages);
				AssertBookingHasNoAttachedTransportJobs(booking);
			});
		}

		public void TestTryCreateTransportJobsFromTransportBookings_CorrectErrorsAreReportedWhenAPickUpInstructionHasNoAddressNoConfirmationsAndNoAttachedPackages()
		{
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);
			BookingToTransportJobCreatorTestHelper.CreateNewInstruction(Helper, booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, address: null, shouldHaveAtLeastOneConfirmation: false, attachExistingPackageOrCreateAndAttachPackage: false);

			Factory.Save();
			
			var buffer = new NotificationBuffer();
			var creator = new BookingToTransportJobCommonCreator(buffer);

			creator.TryCreateTransportJobsFromTransportBookings(new[] { booking });

			var errorMessages = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Select(w => w.Message);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("An error should be reported when a pickup instruction has multiple validation errors.",
					new string[] {
						$"Transport Jobs could not be created from Transport Booking {booking.KM_JobID} for the following reasons:",
						"There are no Packages assigned to 1 Pickup Instruction(s) on this Transport Booking.",
						"There is no Address for 1 Pickup Instruction(s) on this Transport Booking.",
						"There are no Confirmations attached to 1 Pickup Instruction(s) on this Transport Booking." },
					errorMessages);
				AssertBookingHasNoAttachedTransportJobs(booking);
			});
		}

		public void TestTryCreateTransportJobsFromTransportBookings_CorrectErrorsAreReportedWhenADeliveryInstructionHasNoAddressNoConfirmationsAndNoAttachedPackages()
		{
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);
			BookingToTransportJobCreatorTestHelper.CreateNewInstruction(Helper, booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, address: null, shouldHaveAtLeastOneConfirmation: false, attachExistingPackageOrCreateAndAttachPackage: false);

			Factory.Save();

			var buffer = new NotificationBuffer();
			var creator = new BookingToTransportJobCommonCreator(buffer);

			creator.TryCreateTransportJobsFromTransportBookings(new[] { booking });

			var errorMessages = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Select(w => w.Message);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("An error should be reported when a delivery instruction has multiple validation errors.",
					new string[] {
						$"Transport Jobs could not be created from Transport Booking {booking.KM_JobID} for the following reasons:",
						"There are no Packages assigned to 1 Delivery Instruction(s) on this Transport Booking.",
						"There is no Address for 1 Delivery Instruction(s) on this Transport Booking." },
					errorMessages);
				AssertBookingHasNoAttachedTransportJobs(booking);
			});
		}

		public void TestTryCreateTransportJobsFromTransportBookings_CorrectErrorsAreReportedWhenAMultiInstructionHasNoAddressNoConfirmationsAndNoAttachedPackages()
		{
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);
			BookingToTransportJobCreatorTestHelper.CreateNewInstruction(Helper, booking, InstructionTypes.Codes.Multi, OrganisationTypesList.Codes.CNE, address: null, shouldHaveAtLeastOneConfirmation: false, attachExistingPackageOrCreateAndAttachPackage: false);

			Factory.Save();

			var buffer = new NotificationBuffer();
			var creator = new BookingToTransportJobCommonCreator(buffer);

			creator.TryCreateTransportJobsFromTransportBookings(new[] { booking });

			var errorMessages = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Select(w => w.Message);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("An error should be reported when a multi instruction has multiple validation errors.",
					new string[] {
						$"Transport Jobs could not be created from Transport Booking {booking.KM_JobID} for the following reasons:",
						"There are no Packages assigned to 1 Multi Instruction(s) on this Transport Booking.",
						"There is no Address for 1 Multi Instruction(s) on this Transport Booking." },
					errorMessages);
				AssertBookingHasNoAttachedTransportJobs(booking);
			});
		}

		public void TestTryCreateTransportJobsFromTransportBookings_ErrorIsReportedWhenBookingHasNoPickUpInstructions()
		{
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);
			booking.Instructions.PickUpInstructions.DeleteAll();

			Factory.Save();

			var buffer = new NotificationBuffer();
			var creator = new BookingToTransportJobCommonCreator(buffer);

			creator.TryCreateTransportJobsFromTransportBookings(new[] { booking });

			var errorMessages = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Select(w => w.Message);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("An error should be reported when the booking has no pickup instructions.",
					new string[] {
						$"Transport Jobs could not be created from Transport Booking {booking.KM_JobID} for the following reasons:",
						"There are no Pickup Instructions attached to this Transport Booking." },
					errorMessages);
				AssertBookingHasNoAttachedTransportJobs(booking);
			});
		}

		public void TestTryCreateTransportJobsFromTransportBookings_ErrorIsReportedWhenBookingHasNoDeliveryInstructions()
		{
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);
			booking.Instructions.DeliveryInstructions.DeleteAll();

			Factory.Save();

			var buffer = new NotificationBuffer();
			var creator = new BookingToTransportJobCommonCreator(buffer);

			creator.TryCreateTransportJobsFromTransportBookings(new[] { booking });

			var errorMessages = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Select(w => w.Message);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("An error should be reported when the booking has no delivery instructions.",
					new string[] {
						$"Transport Jobs could not be created from Transport Booking {booking.KM_JobID} for the following reasons:",
						"There are no Delivery Instructions attached to this Transport Booking." },
					errorMessages);
				AssertBookingHasNoAttachedTransportJobs(booking);
			});
		}

		public void TestTryCreateTransportJobsFromTransportBookings_TransportJobIsCreatedWhenBookingHasNoMultiInstructions()
		{
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);

			AssertEquals("Precondition: Booking should have no Multi Instructions.", 0, booking.Instructions.Count(i => i.IsMulti));

			Factory.Save();

			var buffer = new NotificationBuffer();
			var creator = new BookingToTransportJobCommonCreator(buffer);

			creator.TryCreateTransportJobsFromTransportBookings(new[] { booking });

			AssertBookingHasAttachedPortTransportJob(booking);
		}

		public void TestTryCreateTransportJobsFromTransportBookings_ErrorIsReportedWhenBookingHasMultiplePickUpAndMultipleDeliveryInstructions()
		{
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);
			BookingToTransportJobCreatorTestHelper.CreateNewInstruction(Helper, booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, Helper.CreateOrLoadOrganisation("PICKUP_ORG").MainAddress, shouldHaveAtLeastOneConfirmation: true, attachExistingPackageOrCreateAndAttachPackage: true);
			BookingToTransportJobCreatorTestHelper.CreateNewInstruction(Helper, booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, Helper.CreateOrLoadOrganisation("DELIVERY_ORG").MainAddress, shouldHaveAtLeastOneConfirmation: true, attachExistingPackageOrCreateAndAttachPackage: true);

			Factory.Save();

			var buffer = new NotificationBuffer();
			var creator = new BookingToTransportJobCommonCreator(buffer);

			creator.TryCreateTransportJobsFromTransportBookings(new[] { booking });

			var errorMessages = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Select(w => w.Message);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("An error should be reported when the booking has multiple pickup and delivery instructions.",
					new string[] {
						$"Transport Jobs could not be created from Transport Booking {booking.KM_JobID} for the following reasons:",
						"There are multiple Pickup Instructions and multiple Delivery Instructions attached to this Transport Booking." },
					errorMessages);
				AssertBookingHasNoAttachedTransportJobs(booking);
			});
		}

		public void TestTryCreateTransportJobsFromTransportBookings_TransportJobIsCreatedWhenBookingHasMultiplePickUpAndMultipleMultiInstructions()
		{
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);
			BookingToTransportJobCreatorTestHelper.CreateNewInstruction(Helper, booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, Helper.CreateOrLoadOrganisation("PICKUP_ORG").MainAddress, shouldHaveAtLeastOneConfirmation: true, attachExistingPackageOrCreateAndAttachPackage: true);
			BookingToTransportJobCreatorTestHelper.CreateNewInstruction(Helper, booking, InstructionTypes.Codes.Multi, OrganisationTypesList.Codes.CNE, Helper.CreateOrLoadOrganisation("MULTI_ORG").MainAddress, shouldHaveAtLeastOneConfirmation: true, attachExistingPackageOrCreateAndAttachPackage: true);
			BookingToTransportJobCreatorTestHelper.CreateNewInstruction(Helper, booking, InstructionTypes.Codes.Multi, OrganisationTypesList.Codes.CNE, Helper.CreateOrLoadOrganisation("MULTI_ORG").MainAddress, shouldHaveAtLeastOneConfirmation: true, attachExistingPackageOrCreateAndAttachPackage: true);

			Factory.Save();

			var buffer = new NotificationBuffer();
			var creator = new BookingToTransportJobCommonCreator(buffer);

			creator.TryCreateTransportJobsFromTransportBookings(new[] { booking });

			var errorMessages = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Select(w => w.Message);

			AssertBookingHasAttachedPortTransportJob(booking);
		}

		public void TestTryCreateTransportJobsFromTransportBookings_ErrorIsReportedWhenBookingHasNoBranch()
		{
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);
			booking.KM_GB_Branch = ZGuid.Empty;

			Factory.Save();

			var buffer = new NotificationBuffer();
			var creator = new BookingToTransportJobCommonCreator(buffer);

			creator.TryCreateTransportJobsFromTransportBookings(new[] { booking });

			var errorMessages = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Select(w => w.Message);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("An error should be reported when the booking has no branch.",
					new string[] {
						$"Transport Jobs could not be created from Transport Booking {booking.KM_JobID} for the following reasons:",
						"There is no Branch for this Transport Booking." },
					errorMessages);
				AssertBookingHasNoAttachedTransportJobs(booking);
			});
		}

		public void TestTryCreateTransportJobsFromTransportBookings_ErrorIsReportedWhenPickupAndDeliveryAndMultiInstructionsAllHaveErrors()
		{
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);
			BookingToTransportJobCreatorTestHelper.CreateNewInstruction(Helper, booking, InstructionTypes.Codes.Multi, OrganisationTypesList.Codes.CNE, Helper.CreateOrLoadOrganisation("MULTI_ORG").MainAddress, shouldHaveAtLeastOneConfirmation: true, attachExistingPackageOrCreateAndAttachPackage: false);

			booking.Instructions.First(i => i.IsPickUp).DivotsWithPackages.DeleteAll();
			booking.Instructions.First(i => i.IsDelivery).DivotsWithPackages.DeleteAll();

			Factory.Save();

			var buffer = new NotificationBuffer();
			var creator = new BookingToTransportJobCommonCreator(buffer);

			creator.TryCreateTransportJobsFromTransportBookings(new[] { booking });

			var errorMessages = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Select(w => w.Message);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("An error should be reported when a delivery instruction has no assigned packages.",
					new string[] {
						$"Transport Jobs could not be created from Transport Booking {booking.KM_JobID} for the following reasons:",
						"There are no Packages assigned to 1 Pickup Instruction(s) on this Transport Booking.",
						"There are no Packages assigned to 1 Delivery Instruction(s) on this Transport Booking.",
						"There are no Packages assigned to 1 Multi Instruction(s) on this Transport Booking."
					},
					errorMessages);
				AssertBookingHasNoAttachedTransportJobs(booking);
			});
		}

		public void TestTryCreateTransportJobsFromTransportBookings_TransportJobIsCreatedWhenAnInstructionHasADivotWithOnePackageAndAnotherDivotWithNoPackages()
		{
			var dummyBookingParent = Factory.New<DummyWithDtbBooking>();

			var booking = BookingToTransportJobCreatorTestHelper.CreateNewBookingValidForManuallyCreatingATransportJobFrom(Helper, dummyBookingParent);

			var packageWithZeroQuantity = booking.ConsolidationSingleJob.PackageJob.Packages.AddNew();
			packageWithZeroQuantity.KP_PackageQty = 0;
			packageWithZeroQuantity.KP_IsUnknownQty = true;
			var divotWithNoPackage = Helper.CreatePackageDivot(booking.Instructions.First(i => i.IsPickUp), packageWithZeroQuantity, 0);

			AssertEquals("Precondition: One of the Divots on the instruction should have no attached packages.", 0, divotWithNoPackage.KD_Quantity);

			Factory.Save();

			var buffer = new NotificationBuffer();
			var creator = new BookingToTransportJobCommonCreator(buffer);

			creator.TryCreateTransportJobsFromTransportBookings(new[] { booking });

			var errorMessages = buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Select(w => w.Message);
			AssertBookingHasAttachedPortTransportJob(booking);
		}
	}
}
